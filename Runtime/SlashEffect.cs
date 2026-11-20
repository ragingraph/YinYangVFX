using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace YinYangVFX
{
    /// <summary>
    /// Plays the sword-slash effect: sweeps the procedural arc shader, emits sparks and motes
    /// along the moving blade tip, and returns itself to a pool when everything has faded.
    ///
    /// Usage:
    ///   SlashEffect.Spawn(slashPrefab, attackerPosition, angleDegrees, mirror, sizeMultiplier);
    ///
    /// angleDegrees = 0 slashes toward +X (to the right). mirror = true flips it to the left side.
    /// Time.timeScale = 0 (hit-stop) freezes the effect too, unless useUnscaledTime is on.
    /// </summary>
    [DisallowMultipleComponent]
    public class SlashEffect : MonoBehaviour
    {
        [Header("References (filled by the builder)")]
        public SpriteRenderer arcMain;
        public SpriteRenderer arcGlow;
        public ParticleSystem sparks;
        public ParticleSystem motes;

        [Header("Timing (seconds)")]
        [Tooltip("Time for the blade progress to go 0 -> 1. The tip itself crosses the arc in roughly a third of this.")]
        public float sweepDuration = 0.26f;
        [Tooltip("Whole effect length; the arc is fully burned out by about 92% of this.")]
        public float totalDuration = 0.55f;
        [Tooltip("The glow layer trails the main blade by this much.")]
        public float glowDelay = 0.03f;
        public AnimationCurve sweepCurve = new AnimationCurve(
            new Keyframe(0f, 0f, 0f, 1.8f),
            new Keyframe(1f, 1f, 0.2f, 0f));
        public bool useUnscaledTime = false;

        [Header("Arc shape (pushed to the shader, also used to place sparks)")]
        public float arcCenterAngle = 0f;
        public float arcHalfSpan = 78f;
        [Range(0.1f, 0.95f)] public float arcRadius = 0.62f;
        public float tailLength = 0.85f;
        [Tooltip("Later parts of the arc have a bigger radius (perspective: nearer to the camera). 0 = a plain circle arc.")]
        [Range(0f, 1f)] public float radiusRamp = 0f;
        [Tooltip("Sparks and motes get smaller / fewer at the thin start of the arc and bigger / more at the thick end.")]
        public bool growAlongBlade = false;

        [Header("Sparks (sizes and speeds are relative to the arc size)")]
        public int sparksPerSweep = 70;
        public Vector2 sparkLifetime = new Vector2(0.22f, 0.5f);
        public Vector2 sparkSize = new Vector2(0.014f, 0.038f);
        public Vector2 sparkOutSpeed = new Vector2(0.3f, 1.0f);
        public Vector2 sparkAlongSpeed = new Vector2(0.4f, 1.6f);
        public float sparkSpread = 0.35f;

        [Header("Motes (slow glowing dust left behind the blade)")]
        public int motesPerSweep = 40;
        public Vector2 moteLifetime = new Vector2(0.5f, 1.0f);
        public Vector2 moteSize = new Vector2(0.01f, 0.03f);
        public Vector2 moteSpeed = new Vector2(0.05f, 0.3f);

        [Header("Approach (a slash that comes toward the camera)")]
        [Tooltip("Grows the arc from far (small, dim) to near (big, bright) while it sweeps.")]
        public bool approach = false;
        public float startScale = 0.3f;
        public float endScale = 1.3f;
        [Tooltip("0..1 over the whole effect. Ease-in makes it speed up as it comes closer.")]
        public AnimationCurve scaleCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
        [Range(0f, 1f)] public float startAlpha = 0.35f;
        [Tooltip("Seconds for the arc to reach full brightness.")]
        public float alphaRiseTime = 0.12f;

        [Header("Impact event (works for every slash)")]
        [Tooltip("Seconds after the start when onImpact fires. Hook camera shake / hit-stop / hit sparks here.")]
        public float impactTime = 0.2f;
        public UnityEvent onImpact = new UnityEvent();

        [Header("Lifecycle")]
        [Tooltip("Only used for instances that were not created by Spawn().")]
        public bool destroyWhenDone = false;

        enum State { Idle, Playing, Draining }

        static readonly int ID_Progress = Shader.PropertyToID("_Progress");
        static readonly int ID_Dissolve = Shader.PropertyToID("_Dissolve");
        static readonly int ID_Seed = Shader.PropertyToID("_Seed");
        static readonly int ID_ArcCenter = Shader.PropertyToID("_ArcCenter");
        static readonly int ID_ArcHalfSpan = Shader.PropertyToID("_ArcHalfSpan");
        static readonly int ID_Radius = Shader.PropertyToID("_Radius");
        static readonly int ID_TailLength = Shader.PropertyToID("_TailLength");
        static readonly int ID_RadiusRamp = Shader.PropertyToID("_RadiusRamp");

        static readonly Dictionary<SlashEffect, Stack<SlashEffect>> Pools =
            new Dictionary<SlashEffect, Stack<SlashEffect>>();

        State _state = State.Idle;
        float _time;
        float _seed;
        float _sparkAcc;
        float _moteAcc;
        MaterialPropertyBlock _mpb;
        SlashEffect _sourcePrefab;
        Vector3 _baseMain;
        Vector3 _baseGlow;
        Vector3 _posMain;
        Vector3 _posGlow;
        bool _baseCached;
        bool _impactFired;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ResetStatics()
        {
            Pools.Clear();
        }

        // ------------------------------------------------------------------ public API

        /// <summary>Gets a pooled instance (or makes one), places it and plays it.</summary>
        public static SlashEffect Spawn(SlashEffect prefab, Vector3 position, float angleDegrees = 0f,
                                        bool mirror = false, float sizeMultiplier = 1f)
        {
            if (prefab == null)
            {
                return null;
            }

            if (!Pools.TryGetValue(prefab, out Stack<SlashEffect> pool))
            {
                pool = new Stack<SlashEffect>();
                Pools.Add(prefab, pool);
            }

            SlashEffect fx = null;
            while (pool.Count > 0 && fx == null)
            {
                fx = pool.Pop(); // may be a destroyed object after a scene change
            }

            if (fx == null)
            {
                fx = Instantiate(prefab);
                fx._sourcePrefab = prefab;
            }

            fx.Play(position, angleDegrees, mirror, sizeMultiplier);
            return fx;
        }

        /// <summary>Plays from wherever this object currently is.</summary>
        [ContextMenu("Play")]
        public void Play()
        {
            Play(transform.position, transform.eulerAngles.z, false, Mathf.Max(0.01f, transform.localScale.x));
        }

        public void Play(Vector3 position, float angleDegrees = 0f, bool mirror = false, float sizeMultiplier = 1f)
        {
            if (arcMain == null || arcGlow == null)
            {
                Debug.LogError("SlashEffect: arcMain / arcGlow are not assigned. Run Tools > YinYang VFX > Build Sword Slash.", this);
                return;
            }

            transform.SetPositionAndRotation(position, Quaternion.Euler(0f, 0f, angleDegrees));
            transform.localScale = Vector3.one * sizeMultiplier;
            SetMirror(arcMain.transform, mirror);
            SetMirror(arcGlow.transform, mirror);

            gameObject.SetActive(true);

            _time = 0f;
            _seed = Random.value * 50f;
            _sparkAcc = 0f;
            _moteAcc = 0f;

            arcMain.enabled = true;
            arcGlow.enabled = true;
            _impactFired = false;
            ApplyApproach(0f);
            ApplyShader(0f, 0f, 0f, 0f);

            if (sparks != null)
            {
                sparks.Clear(true);
                sparks.Play(true);
            }

            if (motes != null)
            {
                motes.Clear(true);
                motes.Play(true);
            }

            _state = State.Playing;
        }

        /// <summary>Cuts the effect off immediately.</summary>
        public void StopImmediately()
        {
            if (sparks != null) sparks.Clear(true);
            if (motes != null) motes.Clear(true);
            Release();
        }

        // ------------------------------------------------------------------ update

        void Update()
        {
            if (_state == State.Idle)
            {
                return;
            }

            if (_state == State.Draining)
            {
                bool sparksDone = sparks == null || sparks.particleCount == 0;
                bool motesDone = motes == null || motes.particleCount == 0;
                if (sparksDone && motesDone)
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

            float prevTime = _time;
            _time += dt;

            ApplyApproach(_time);
            if (!_impactFired && _time >= impactTime)
            {
                _impactFired = true;
                onImpact.Invoke();
            }

            float progress = EvalProgress(_time);
            float prevProgress = EvalProgress(prevTime);
            ApplyShader(progress, EvalDissolve(_time), EvalProgress(_time - glowDelay), EvalDissolve(_time - glowDelay));
            EmitAlongBlade(prevProgress, progress);

            if (_time >= totalDuration)
            {
                arcMain.enabled = false;
                arcGlow.enabled = false;
                if (sparks != null) sparks.Stop(true, ParticleSystemStopBehavior.StopEmitting);
                if (motes != null) motes.Stop(true, ParticleSystemStopBehavior.StopEmitting);
                _state = State.Draining;
            }
        }

        // ------------------------------------------------------------------ approach (far -> near)

        void CacheBase()
        {
            if (_baseCached)
            {
                return;
            }

            _baseMain = AbsScale(arcMain.transform.localScale);
            _baseGlow = AbsScale(arcGlow.transform.localScale);
            _posMain = arcMain.transform.localPosition;
            _posGlow = arcGlow.transform.localPosition;
            _baseCached = true;
        }

        static Vector3 AbsScale(Vector3 s)
        {
            return new Vector3(Mathf.Abs(s.x), Mathf.Abs(s.y), Mathf.Abs(s.z));
        }

        void ApplyApproach(float time)
        {
            CacheBase();
            if (!approach && time > 0f)
            {
                return; // normal slashes keep their prefab scale; only the start frame resets it
            }

            float k = 1f;
            float alpha = 1f;
            if (approach)
            {
                float n = Mathf.Clamp01(time / Mathf.Max(0.0001f, totalDuration));
                k = Mathf.Lerp(startScale, endScale, scaleCurve.Evaluate(n));
                alpha = Mathf.Lerp(startAlpha, 1f, Mathf.Clamp01(time / Mathf.Max(0.0001f, alphaRiseTime)));
            }

            SetScale(arcMain.transform, _baseMain, _posMain, k);
            SetScale(arcGlow.transform, _baseGlow, _posGlow, k);
            SetAlpha(arcMain, alpha);
            SetAlpha(arcGlow, alpha);
        }

        static void SetScale(Transform t, Vector3 baseScale, Vector3 basePos, float k)
        {
            float sign = t.localScale.x < 0f ? -1f : 1f; // keep the mirror flip
            t.localScale = new Vector3(sign * baseScale.x * k, baseScale.y * k, baseScale.z);
            // The arc's centre offset scales with the arc (so it grows around the object's position) and flips with the mirror.
            t.localPosition = new Vector3(sign * basePos.x * k, basePos.y * k, basePos.z * k);
        }

        static void SetAlpha(SpriteRenderer sr, float alpha)
        {
            Color c = sr.color;
            c.a = alpha;
            sr.color = c;
        }

        // ------------------------------------------------------------------ timeline

        float EvalProgress(float time)
        {
            if (time <= 0f)
            {
                return 0f;
            }

            return sweepCurve.Evaluate(Mathf.Clamp01(time / Mathf.Max(0.0001f, sweepDuration)));
        }

        float EvalDissolve(float time)
        {
            float start = sweepDuration * 0.5f;
            float end = totalDuration * 0.92f;
            float d = Mathf.Clamp01((time - start) / Mathf.Max(0.0001f, end - start));
            return d * d * (3f - 2f * d);
        }

        // ------------------------------------------------------------------ shader

        void ApplyShader(float progress, float dissolve, float glowProgress, float glowDissolve)
        {
            if (_mpb == null)
            {
                _mpb = new MaterialPropertyBlock();
            }

            Push(arcMain, progress, dissolve, tailLength);
            Push(arcGlow, glowProgress, glowDissolve, tailLength * 1.2f);
        }

        void Push(SpriteRenderer sr, float progress, float dissolve, float tail)
        {
            sr.GetPropertyBlock(_mpb);
            _mpb.SetFloat(ID_Progress, progress);
            _mpb.SetFloat(ID_Dissolve, dissolve);
            _mpb.SetFloat(ID_Seed, _seed);
            _mpb.SetFloat(ID_ArcCenter, arcCenterAngle);
            _mpb.SetFloat(ID_ArcHalfSpan, arcHalfSpan);
            _mpb.SetFloat(ID_Radius, arcRadius);
            _mpb.SetFloat(ID_TailLength, tail);
            _mpb.SetFloat(ID_RadiusRamp, radiusRamp);
            sr.SetPropertyBlock(_mpb);
        }

        // ------------------------------------------------------------------ particles

        void EmitAlongBlade(float prevProgress, float progress)
        {
            // Position of the blade tip along the arc, 0 = top end, 1 = bottom end.
            float t0 = Mathf.Min(prevProgress * (1f + tailLength), 1f);
            float t1 = Mathf.Min(progress * (1f + tailLength), 1f);
            if (t1 <= t0)
            {
                return;
            }

            float span = t1 - t0;
            float size = Mathf.Abs(arcMain.transform.lossyScale.x);
            // Thin start of the arc -> fewer, smaller sparks; thick end -> more, bigger ones.
            float w = growAlongBlade ? Mathf.Lerp(0.3f, 1f, Mathf.Clamp01((t0 + t1) * 0.5f)) : 1f;

            if (sparks != null)
            {
                _sparkAcc += span * sparksPerSweep * w;
                int n = (int)_sparkAcc;
                _sparkAcc -= n;
                for (int i = 0; i < n; i++)
                {
                    EmitParticle(sparks, Mathf.Lerp(t0, t1, Random.value), size * w,
                        Random.Range(-0.02f, 0.06f), sparkLifetime, sparkSize, sparkOutSpeed, sparkAlongSpeed, sparkSpread);
                }
            }

            if (motes != null)
            {
                _moteAcc += span * motesPerSweep * w;
                int n = (int)_moteAcc;
                _moteAcc -= n;
                for (int i = 0; i < n; i++)
                {
                    EmitParticle(motes, Mathf.Lerp(t0, t1, Random.value), size * w,
                        -Random.Range(0f, 0.18f), moteLifetime, moteSize, moteSpeed, moteSpeed, 0.3f);
                }
            }
        }

        void EmitParticle(ParticleSystem ps, float t, float size, float radialOffset,
                          Vector2 lifetime, Vector2 particleSize, Vector2 outSpeed, Vector2 alongSpeed, float spread)
        {
            float ang = (arcCenterAngle + arcHalfSpan - t * 2f * arcHalfSpan) * Mathf.Deg2Rad;
            Vector2 dir = new Vector2(Mathf.Cos(ang), Mathf.Sin(ang));
            Vector2 tangent = new Vector2(dir.y, -dir.x); // direction the blade is travelling

            Transform arc = arcMain.transform;
            float radius = arcRadius * (1f + radiusRamp * Mathf.Clamp01(t)) + radialOffset;

            // The arc sprite is 1 unit wide, so a point at "p" (-1..1) sits at p * 0.5 in its local space.
            Vector3 position = arc.TransformPoint(new Vector3(dir.x, dir.y, 0f) * (radius * 0.5f));
            Vector3 outward = arc.TransformVector(new Vector3(dir.x, dir.y, 0f)).normalized;
            Vector3 along = arc.TransformVector(new Vector3(tangent.x, tangent.y, 0f)).normalized;
            Vector2 jitter = Random.insideUnitCircle * spread;

            Vector3 velocity = (outward * Random.Range(outSpeed.x, outSpeed.y)
                              + along * Random.Range(alongSpeed.x, alongSpeed.y)
                              + new Vector3(jitter.x, jitter.y, 0f)) * size;

            ParticleSystem.EmitParams ep = new ParticleSystem.EmitParams();
            ep.position = position;
            ep.velocity = velocity;
            ep.startLifetime = Random.Range(lifetime.x, lifetime.y);
            ep.startSize = Random.Range(particleSize.x, particleSize.y) * size;
            ps.Emit(ep, 1);
        }

        // ------------------------------------------------------------------ helpers

        static void SetMirror(Transform t, bool mirror)
        {
            Vector3 s = t.localScale;
            s.x = Mathf.Abs(s.x) * (mirror ? -1f : 1f);
            t.localScale = s;
        }

        void Release()
        {
            _state = State.Idle;

            if (_sourcePrefab != null && Pools.TryGetValue(_sourcePrefab, out Stack<SlashEffect> pool))
            {
                gameObject.SetActive(false);
                pool.Push(this);
            }
            else if (destroyWhenDone)
            {
                Destroy(gameObject);
            }
            else
            {
                gameObject.SetActive(false);
            }
        }

        void OnValidate()
        {
            sweepDuration = Mathf.Max(0.02f, sweepDuration);
            totalDuration = Mathf.Max(sweepDuration + 0.05f, totalDuration);
            arcHalfSpan = Mathf.Clamp(arcHalfSpan, 20f, 175f);
            startScale = Mathf.Max(0.05f, startScale);
            endScale = Mathf.Max(startScale, endScale);
            alphaRiseTime = Mathf.Max(0.01f, alphaRiseTime);
            impactTime = Mathf.Max(0f, impactTime);
        }
    }
}
