using System.Collections.Generic;
using UnityEngine;

namespace YinYangVFX
{
    /// <summary>
    /// Plays the yin-yang vortex: a spinning taiji disc with light/dark spiral arms and orbiting motes.
    ///
    /// One-shot (portal flash, skill cast):
    ///   YinYangSwirl.Spawn(prefab, position, size: 1f, duration: 2.5f);
    ///
    /// Looping (aura, charge-up, portal that stays open):
    ///   var swirl = YinYangSwirl.Spawn(prefab, position, 1f, loop: true);
    ///   ...
    ///   swirl.Stop();   // fades out, then returns to the pool
    ///
    /// size = radius of the taiji disc in world units (the prefab is built with a disc radius of 1).
    /// Hit-stop (Time.timeScale = 0) freezes it, unless useUnscaledTime is on.
    /// </summary>
    [DisallowMultipleComponent]
    public class YinYangSwirl : MonoBehaviour
    {
        [Header("References (filled by the builder)")]
        public SpriteRenderer disc;
        public ParticleSystem lightMotes;
        public ParticleSystem inkMotes;

        [Header("Timing (seconds)")]
        public float fadeInTime = 0.5f;
        [Tooltip("Total length of a one-shot, including the fade-out. Ignored when Loop is on.")]
        public float duration = 3f;
        public float fadeOutTime = 0.6f;
        public bool loop = false;
        public bool useUnscaledTime = false;

        [Header("Motion")]
        [Tooltip("Degrees per second. Spins faster while appearing.")]
        public float spinSpeed = 110f;

        [Header("Scene instances")]
        [Tooltip("For an instance placed in a scene: start by itself.")]
        public bool playOnStart = false;

        enum State { Idle, Playing, Draining }

        static readonly int ID_Appear = Shader.PropertyToID("_Appear");
        static readonly int ID_Fade = Shader.PropertyToID("_Fade");
        static readonly int ID_Spin = Shader.PropertyToID("_Spin");
        static readonly int ID_Seed = Shader.PropertyToID("_Seed");

        static readonly Dictionary<YinYangSwirl, Stack<YinYangSwirl>> Pools =
            new Dictionary<YinYangSwirl, Stack<YinYangSwirl>>();

        State _state = State.Idle;
        float _time;
        float _spin;
        float _seed;
        float _outTime = -1f; // < 0 = not fading out yet
        MaterialPropertyBlock _mpb;
        YinYangSwirl _sourcePrefab;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ResetStatics()
        {
            Pools.Clear();
        }

        // ------------------------------------------------------------------ public API

        /// <summary>Gets a pooled instance (or makes one), places it and plays it.</summary>
        /// <param name="duration">&gt; 0 overrides the prefab's one-shot length.</param>
        public static YinYangSwirl Spawn(YinYangSwirl prefab, Vector3 position, float size = 1f,
                                         float duration = -1f, bool loop = false)
        {
            if (prefab == null)
            {
                return null;
            }

            if (!Pools.TryGetValue(prefab, out Stack<YinYangSwirl> pool))
            {
                pool = new Stack<YinYangSwirl>();
                Pools.Add(prefab, pool);
            }

            YinYangSwirl fx = null;
            while (pool.Count > 0 && fx == null)
            {
                fx = pool.Pop(); // may be a destroyed object after a scene change
            }

            if (fx == null)
            {
                fx = Instantiate(prefab);
                fx._sourcePrefab = prefab;
            }

            fx.loop = loop;
            if (duration > 0f)
            {
                fx.duration = duration;
            }

            fx.Play(position, size);
            return fx;
        }

        /// <summary>Plays from wherever this object currently is (uses its scale as the size).</summary>
        [ContextMenu("Play")]
        public void Play()
        {
            Play(transform.position, Mathf.Max(0.01f, transform.localScale.x));
        }

        public void Play(Vector3 position, float size = 1f)
        {
            if (disc == null)
            {
                Debug.LogError("YinYangSwirl: disc is not assigned. Run Tools > YinYang VFX > Build Yin-Yang Swirl.", this);
                return;
            }

            transform.position = position;
            transform.localScale = Vector3.one * size;
            gameObject.SetActive(true);

            _time = 0f;
            _spin = Random.value * 360f;
            _seed = Random.value * 50f;
            _outTime = -1f;

            disc.enabled = true;
            ApplyShader(0f, 0f);

            RestartParticles(lightMotes);
            RestartParticles(inkMotes);

            _state = State.Playing;
        }

        /// <summary>Starts the fade-out. The object returns to the pool when everything has faded.</summary>
        public void Stop()
        {
            if (_state == State.Playing && _outTime < 0f)
            {
                _outTime = 0f;
            }
        }

        /// <summary>Cuts the effect off immediately.</summary>
        public void StopImmediately()
        {
            if (lightMotes != null) lightMotes.Clear(true);
            if (inkMotes != null) inkMotes.Clear(true);
            Release();
        }

        // ------------------------------------------------------------------ update

        void Start()
        {
            if (playOnStart && _state == State.Idle)
            {
                Play(transform.position, Mathf.Max(0.01f, transform.localScale.x));
            }
        }

        void Update()
        {
            if (_state == State.Idle)
            {
                return;
            }

            if (_state == State.Draining)
            {
                bool lightDone = lightMotes == null || lightMotes.particleCount == 0;
                bool inkDone = inkMotes == null || inkMotes.particleCount == 0;
                if (lightDone && inkDone)
                {
                    Release();
                }

                return;
            }

            float dt = useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
            if (dt <= 0f)
            {
                return; // hit-stop: everything holds still
            }

            _time += dt;

            float appear = Mathf.Clamp01(_time / Mathf.Max(0.0001f, fadeInTime));

            // One-shots start their fade-out by themselves; loops wait for Stop().
            if (_outTime < 0f && !loop && _time >= duration - fadeOutTime)
            {
                _outTime = 0f;
            }

            float fade = 0f;
            if (_outTime >= 0f)
            {
                _outTime += dt;
                fade = Mathf.Clamp01(_outTime / Mathf.Max(0.0001f, fadeOutTime));
                StopEmitting(lightMotes);
                StopEmitting(inkMotes);
            }

            // spin: fast at the start, settling to spinSpeed
            _spin += spinSpeed * (1f + 2f * (1f - appear)) * dt;
            ApplyShader(appear, fade);

            if (fade >= 1f)
            {
                disc.enabled = false;
                _state = State.Draining;
            }
        }

        // ------------------------------------------------------------------ helpers

        void ApplyShader(float appear, float fade)
        {
            if (_mpb == null)
            {
                _mpb = new MaterialPropertyBlock();
            }

            disc.GetPropertyBlock(_mpb);
            _mpb.SetFloat(ID_Appear, appear);
            _mpb.SetFloat(ID_Fade, fade);
            _mpb.SetFloat(ID_Spin, _spin);
            _mpb.SetFloat(ID_Seed, _seed);
            disc.SetPropertyBlock(_mpb);
        }

        static void RestartParticles(ParticleSystem ps)
        {
            if (ps == null)
            {
                return;
            }

            ps.Clear(true);
            ps.Play(true);
        }

        static void StopEmitting(ParticleSystem ps)
        {
            if (ps != null && ps.isEmitting)
            {
                ps.Stop(true, ParticleSystemStopBehavior.StopEmitting);
            }
        }

        void Release()
        {
            _state = State.Idle;

            if (_sourcePrefab != null && Pools.TryGetValue(_sourcePrefab, out Stack<YinYangSwirl> pool))
            {
                gameObject.SetActive(false);
                pool.Push(this);
            }
            else
            {
                // A scene instance (or one not made by Spawn): just hide it. Call Play() to use it again.
                gameObject.SetActive(false);
            }
        }

        void OnValidate()
        {
            fadeInTime = Mathf.Max(0.05f, fadeInTime);
            fadeOutTime = Mathf.Max(0.05f, fadeOutTime);
            duration = Mathf.Max(fadeInTime + fadeOutTime, duration);
        }
    }
}
