using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace YinYangVFX
{
    public enum WeaponKind
    {
        SpearThrust = 0,
        SpearSweep = 1,
        HammerSmash = 2,
        SwordQi = 3,
        CrossSlash = 4,
        SpearRising = 5,
        SpearFlurry = 6,
        HammerSweep = 7,
        HammerQuake = 8,
        SwordSlash = 9,
        SwordWhirl = 10,
        SwordLunge = 11,
        BladeChop = 12,
        BladeDrag = 13,
        HalberdCrescent = 14,
        HalberdLiftThrust = 15,
        WhipLash = 16,
        NunchakuFlurry = 17,
        PalmWave = 18,
        Dart = 19,
        ArrowShot = 20,
        ShieldOrb = 21,
        ShieldArc = 22,
        YinYangShield = 23,
        BlockSparks = 24,
        PerfectBlock = 25,
        ShieldRipple = 26,
        ReflectWave = 27,
        ProjectileReturn = 28,
        DodgeAfterimage = 29,
        RollDust = 30,
        Blink = 31,
        IronBody = 32,
        HealPillar = 33,
        InvincibleGlow = 34,
        ShieldBreak = 35,
        StunStars = 36,
        FireBall = 37,
        Flamethrower = 38,
        IceSpike = 39,
        FreezeBurst = 40,
        LightningChain = 41,
        LightningStrike = 42,
        WindBlade = 43,
        Tornado = 44,
        WaterBall = 45,
        WaterColumn = 46,
        RockSpike = 47,
        FallingRocks = 48,
        PoisonMist = 49,
        PoisonShot = 50,
        HolyLight = 51,
        ShadowOrb = 52,
        HitSlash = 53,
        HitBlunt = 54,
        HitElemental = 55,
        HitPierce = 56,
        CritHit = 57,
        CritBadge = 58,
        KillBurst = 59,
        StatusBurn = 60,
        StatusFrozen = 61,
        StatusPoison = 62,
        StatusParalyze = 63,
        StatusSlow = 64,
        StatusSilence = 65,
        StatusCurse = 66,
        StatusBleed = 67,
        BuffAttackUp = 68,
        BuffSpeedUp = 69,
        BuffDefenseUp = 70,
        BuffCritUp = 71,
        DebuffAttackDown = 72,
        DebuffDefenseDown = 73,
        ChargeAura = 74,
        RageBurst = 75,
        Awaken = 76,
        Berserk = 77,
        CharacterAura = 78,
        DeathMonster = 79,
        DeathAsh = 80,
        DeathLight = 81,
        DeathSoul = 82,
        DeathBoss = 83,
    }

    /// <summary>
    /// Chinese-weapon attack effects, drawn procedurally (tapered light strokes + sparks) in three palette colours.
    ///
    ///   WeaponEffect.Spawn(prefab, position, angleDegrees: 0f, mirror: false, size: 1f);
    ///
    /// Where the effect is placed (side-scroller, character facing right):
    ///   SpearThrust : the weapon hand. The thrust runs to the right.
    ///   SpearSweep  : the character. The ring circles the body.
    ///   HammerSmash : the weapon hand. The impact lands "impactOffset" away (default: in front, on the ground).
    ///   SwordQi     : the weapon hand. The crescent flies right.
    ///   CrossSlash  : the target. The X is centred on the position.
    /// mirror = true faces left. angleDegrees rotates the whole effect (after mirroring).
    /// size scales everything (1 = a character about 1 unit tall).
    /// onImpact fires at the moment of impact (hammer hit, thrust reaching full length, X crossing): use it for
    /// screen shake and hit-stop.
    /// Hit-stop (Time.timeScale = 0) freezes it, unless useUnscaledTime is on.
    /// </summary>
    [DisallowMultipleComponent]
    public partial class WeaponEffect : MonoBehaviour
    {
        [Header("Kind")]
        public WeaponKind kind = WeaponKind.SpearThrust;

        [Header("Palette (HDR: values above 1 feed Bloom)")]
        [ColorUsage(false, true)] public Color primary = new Color(0.25f, 1f, 3f, 1f);
        [ColorUsage(false, true)] public Color secondary = new Color(3f, 0.8f, 0.12f, 1f);
        [ColorUsage(false, true)] public Color tertiary = new Color(1.4f, 0.45f, 3f, 1f);

        [Header("Materials (filled by the builder)")]
        public Material lineMaterial;
        public Material dotMaterial;
        public Material starMaterial;

        [Header("Timing")]
        public float duration = 0.8f;
        public bool useUnscaledTime = false;

        [Header("Shape (local units; a character is about 1 unit tall)")]
        [Tooltip("Line thickness multiplier.")]
        public float thickness = 1f;
        [Tooltip("Spear thrust: how far the tip travels.")]
        public float reach = 2.0f;
        [Tooltip("Spear sweep: centre of the ring, relative to the position.")]
        public Vector2 sweepCenter = new Vector2(0f, -0.2f);
        [Tooltip("Spear sweep: half width / half height of the ring (the flat ring gives the 3D look).")]
        public Vector2 sweepRadius = new Vector2(1.12f, 0.42f);
        [Tooltip("Hammer: where the hammer lands, relative to the position.")]
        public Vector2 impactOffset = new Vector2(1.55f, -0.7f);
        [Tooltip("Sword qi: how far the crescent flies.")]
        public float qiTravel = 3.2f;
        [Tooltip("Height of the ground, relative to the position (ground ring, dust, qi ground streak).")]
        public float groundY = -0.68f;

        [Header("Looping (shields, buffs, stun)")]
        [Tooltip("Keeps playing until Stop() is called. Set by the builder for shield / buff / stun effects.")]
        public bool loop = false;
        [Tooltip("Optional: the effect follows this transform every frame (the character).")]
        public Transform follow;
        public Vector2 followOffset = Vector2.zero;

        [Header("Richness")]
        [Tooltip("Spark / star particles are emitted this many times over (1 = original amount).")]
        public float particleMultiplier = 2.5f;
        [Tooltip("Chance per stroke per frame of a glittering star appearing on it.")]
        [Range(0f, 1f)] public float sparkle = 0.3f;

        [Header("Events")]
        public UnityEvent onImpact;

        [Header("Rendering")]
        public int sortingOrder = 100;

        [Header("Scene instances")]
        public bool playOnStart = false;

        enum Taper { Head = 0, Lens = 1, Tail = 2, Flat = 3 }

        class Ln
        {
            public LineRenderer lr;
            public MaterialPropertyBlock mpb;
        }

        static readonly int ID_Tint = Shader.PropertyToID("_Tint");
        static readonly Dictionary<WeaponEffect, Stack<WeaponEffect>> Pools = new Dictionary<WeaponEffect, Stack<WeaponEffect>>();
        static AnimationCurve[] _curves;
        static Gradient[] _grads;

        readonly List<Ln>[] _lines = new List<Ln>[4];
        readonly int[] _lineUsed = new int[4];
        readonly Vector3[] _buf = new Vector3[96];

        ParticleSystem _dots;
        ParticleSystem _sparks;
        ParticleSystem _stars;

        bool _built;
        bool _playing;
        bool _fired;
        float _t;
        float _dt;
        float _seed;
        float _scale = 1f;
        float _acc;
        int _flags;
        float _crackScale = 1f;
        float _gm = 1f;
        float _pulse;
        float _outT = -1f;
        Color _core;
        Matrix4x4 _l2w;
        Vector2[][] _cracks;
        WeaponEffect _sourcePrefab;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ResetStatics()
        {
            Pools.Clear();
        }

        // ------------------------------------------------------------------ public API

        public static WeaponEffect Spawn(WeaponEffect prefab, Vector3 position, float angleDegrees = 0f,
                                         bool mirror = false, float size = 1f)
        {
            if (prefab == null)
            {
                return null;
            }

            if (!Pools.TryGetValue(prefab, out Stack<WeaponEffect> pool))
            {
                pool = new Stack<WeaponEffect>();
                Pools.Add(prefab, pool);
            }

            WeaponEffect fx = null;
            while (pool.Count > 0 && fx == null)
            {
                fx = pool.Pop(); // may be a destroyed object after a scene change
            }

            if (fx == null)
            {
                fx = Instantiate(prefab);
                fx._sourcePrefab = prefab;
            }

            fx.Play(position, angleDegrees, mirror, size);
            return fx;
        }

        /// <summary>Plays from wherever this object is now (its scale is used as the size).</summary>
        [ContextMenu("Play")]
        public void Play()
        {
            Play(transform.position, transform.eulerAngles.z, transform.localScale.x < 0f,
                 Mathf.Max(0.01f, Mathf.Abs(transform.localScale.y)));
        }

        public void Play(Vector3 position, float angleDegrees = 0f, bool mirror = false, float size = 1f)
        {
            EnsureBuilt();

            transform.SetPositionAndRotation(position, Quaternion.Euler(0f, 0f, angleDegrees));
            transform.localScale = new Vector3(mirror ? -size : size, size, size);
            gameObject.SetActive(true);

            _t = 0f;
            _fired = false;
            _acc = 0f;
            _flags = 0;
            _gm = 1f;
            _pulse = 0f;
            _outT = -1f;
            _crackScale = kind == WeaponKind.HammerQuake ? 1.7f : 1f;
            _seed = Random.value * 100f;
            _core = Color.Lerp(primary, new Color(3f, 3.2f, 3.4f, 1f), 0.45f);
            ClearParticles();
            HideAllLines();
            if (kind == WeaponKind.HammerSmash || kind == WeaponKind.HammerQuake)
            {
                BuildCracks();
            }

            _playing = true;
        }

        public void SetPalette(Color primaryColor, Color secondaryColor, Color tertiaryColor)
        {
            primary = primaryColor;
            secondary = secondaryColor;
            tertiary = tertiaryColor;
        }

        /// <summary>Looping effects: fades out, then returns to the pool.</summary>
        public void Stop()
        {
            if (_playing && loop && _outT < 0f)
            {
                _outT = 0f;
            }
        }

        /// <summary>Looping effects: flash brighter for a moment (call when the shield / body is hit).</summary>
        public void Pulse()
        {
            _pulse = 1f;
        }

        public void StopImmediately()
        {
            Release();
        }

        // ------------------------------------------------------------------ lifecycle

        void Awake()
        {
            EnsureBuilt();
        }

        void Start()
        {
            if (playOnStart && !_playing)
            {
                Play();
            }
        }

        void Update()
        {
            if (!_playing)
            {
                return;
            }

            float dt = useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
            if (dt <= 0f)
            {
                return; // hit-stop: everything holds still
            }

            _t += dt;
            _dt = dt;
            if (follow != null)
            {
                transform.position = follow.position + new Vector3(followOffset.x, followOffset.y, 0f);
            }

            _pulse = Mathf.Max(0f, _pulse - dt * 3f);
            if (loop)
            {
                if (_outT >= 0f)
                {
                    _outT += dt;
                }

                _gm = Mathf.Clamp01(_t / 0.25f) * (1f - Mathf.Clamp01(_outT / 0.35f));
            }
            else
            {
                _gm = 1f;
            }

            _l2w = transform.localToWorldMatrix;
            _scale = Mathf.Abs(transform.lossyScale.y);

            switch (kind)
            {
                case WeaponKind.SpearThrust: AnimThrust(); break;
                case WeaponKind.SpearSweep: AnimSweep(); break;
                case WeaponKind.HammerSmash: AnimHammer(); break;
                case WeaponKind.SwordQi: AnimQi(); break;
                case WeaponKind.CrossSlash: AnimCross(); break;
                case WeaponKind.SpearRising: AnimRising(); break;
                case WeaponKind.SpearFlurry: AnimFlurry(); break;
                case WeaponKind.HammerSweep: AnimHammerSweep(); break;
                case WeaponKind.HammerQuake: AnimQuake(); break;
                case WeaponKind.SwordSlash: AnimSwordSlash(); break;
                case WeaponKind.SwordWhirl: AnimWhirl(); break;
                case WeaponKind.SwordLunge: AnimLunge(); break;
                case WeaponKind.BladeChop: AnimChop(); break;
                case WeaponKind.BladeDrag: AnimDrag(); break;
                case WeaponKind.HalberdCrescent: AnimHalberdCrescent(); break;
                case WeaponKind.HalberdLiftThrust: AnimHalberdLift(); break;
                case WeaponKind.WhipLash: AnimWhip(); break;
                case WeaponKind.NunchakuFlurry: AnimNunchaku(); break;
                case WeaponKind.PalmWave: AnimPalm(); break;
                case WeaponKind.Dart: AnimDart(); break;
                case WeaponKind.ArrowShot: AnimArrow(); break;
                case WeaponKind.ShieldOrb: AnimShieldOrb(); break;
                case WeaponKind.ShieldArc: AnimShieldArc(); break;
                case WeaponKind.YinYangShield: AnimYinYangShield(); break;
                case WeaponKind.BlockSparks: AnimBlockSparks(); break;
                case WeaponKind.PerfectBlock: AnimPerfectBlock(); break;
                case WeaponKind.ShieldRipple: AnimShieldRipple(); break;
                case WeaponKind.ReflectWave: AnimReflectWave(); break;
                case WeaponKind.ProjectileReturn: AnimProjectileReturn(); break;
                case WeaponKind.DodgeAfterimage: AnimDodge(); break;
                case WeaponKind.RollDust: AnimRoll(); break;
                case WeaponKind.Blink: AnimBlink(); break;
                case WeaponKind.IronBody: AnimIronBody(); break;
                case WeaponKind.HealPillar: AnimHealPillar(); break;
                case WeaponKind.InvincibleGlow: AnimInvincible(); break;
                case WeaponKind.ShieldBreak: AnimShieldBreak(); break;
                case WeaponKind.StunStars: AnimStun(); break;
                case WeaponKind.FireBall: AnimFireball(); break;
                case WeaponKind.Flamethrower: AnimFlamethrower(); break;
                case WeaponKind.IceSpike: AnimIceSpike(); break;
                case WeaponKind.FreezeBurst: AnimFreezeBurst(); break;
                case WeaponKind.LightningChain: AnimLightningChain(); break;
                case WeaponKind.LightningStrike: AnimLightningStrike(); break;
                case WeaponKind.WindBlade: AnimWindBlade(); break;
                case WeaponKind.Tornado: AnimTornado(); break;
                case WeaponKind.WaterBall: AnimWaterBall(); break;
                case WeaponKind.WaterColumn: AnimWaterColumn(); break;
                case WeaponKind.RockSpike: AnimRockSpike(); break;
                case WeaponKind.FallingRocks: AnimFallingRocks(); break;
                case WeaponKind.PoisonMist: AnimPoisonMist(); break;
                case WeaponKind.PoisonShot: AnimPoisonShot(); break;
                case WeaponKind.HolyLight: AnimHolyLight(); break;
                case WeaponKind.ShadowOrb: AnimShadowOrb(); break;
                case WeaponKind.HitSlash: AnimHitSlash(); break;
                case WeaponKind.HitBlunt: AnimHitBlunt(); break;
                case WeaponKind.HitElemental: AnimHitElemental(); break;
                case WeaponKind.HitPierce: AnimHitPierce(); break;
                case WeaponKind.CritHit: AnimCritHit(); break;
                case WeaponKind.CritBadge: AnimCritBadge(); break;
                case WeaponKind.KillBurst: AnimKillBurst(); break;
                case WeaponKind.StatusBurn: AnimStatusBurn(); break;
                case WeaponKind.StatusFrozen: AnimStatusFrozen(); break;
                case WeaponKind.StatusPoison: AnimStatusPoison(); break;
                case WeaponKind.StatusParalyze: AnimStatusParalyze(); break;
                case WeaponKind.StatusSlow: AnimStatusSlow(); break;
                case WeaponKind.StatusSilence: AnimStatusSilence(); break;
                case WeaponKind.StatusCurse: AnimStatusCurse(); break;
                case WeaponKind.StatusBleed: AnimStatusBleed(); break;
                case WeaponKind.BuffAttackUp: AnimBuffAttackUp(); break;
                case WeaponKind.BuffSpeedUp: AnimBuffSpeedUp(); break;
                case WeaponKind.BuffDefenseUp: AnimBuffDefenseUp(); break;
                case WeaponKind.BuffCritUp: AnimBuffCritUp(); break;
                case WeaponKind.DebuffAttackDown: AnimDebuffAttackDown(); break;
                case WeaponKind.DebuffDefenseDown: AnimDebuffDefenseDown(); break;
                case WeaponKind.ChargeAura: AnimChargeAura(); break;
                case WeaponKind.RageBurst: AnimRageBurst(); break;
                case WeaponKind.Awaken: AnimAwaken(); break;
                case WeaponKind.Berserk: AnimBerserk(); break;
                case WeaponKind.CharacterAura: AnimCharacterAura(); break;
                case WeaponKind.DeathMonster: AnimDeathMonster(); break;
                case WeaponKind.DeathAsh: AnimDeathAsh(); break;
                case WeaponKind.DeathLight: AnimDeathLight(); break;
                case WeaponKind.DeathSoul: AnimDeathSoul(); break;
                case WeaponKind.DeathBoss: AnimDeathBoss(); break;
            }

            EndFrame();

            if (loop)
            {
                if (_outT >= 0.35f && (ParticlesDone() || _outT >= 1.5f))
                {
                    Release();
                }

                return;
            }

            if (_t >= duration && (ParticlesDone() || _t >= duration + 1.5f))
            {
                Release();
            }
        }

        void Release()
        {
            _playing = false;
            ClearParticles();
            HideAllLines();

            if (_sourcePrefab != null && Pools.TryGetValue(_sourcePrefab, out Stack<WeaponEffect> pool))
            {
                follow = null;
                gameObject.SetActive(false);
                pool.Push(this);
            }
            else
            {
                gameObject.SetActive(false); // scene instance: call Play() to use it again
            }
        }

        void OnValidate()
        {
            duration = Mathf.Max(0.3f, duration);
            thickness = Mathf.Max(0.05f, thickness);
        }

        // ------------------------------------------------------------------ build (lines + particles)

        void EnsureBuilt()
        {
            if (_built)
            {
                return;
            }

            _built = true;
            if (_curves == null)
            {
                _curves = new AnimationCurve[]
                {
                    MakeCurve(s => Mathf.Pow(s, 1.3f) * Mathf.Min(1f, (1f - s) / 0.05f)), // Head: thin tail, thick head, sharp tip
                    MakeCurve(s => Mathf.Pow(Mathf.Sin(Mathf.PI * s), 0.8f)),               // Lens: pointed at both ends
                    MakeCurve(s => Mathf.Pow(1f - s, 0.9f)),                                 // Tail: thick start, thin end
                    MakeCurve(s => 1f),                                                      // Flat
                };

                Gradient head = new Gradient();
                head.SetKeys(new[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(Color.white, 1f) },
                             new[] { new GradientAlphaKey(0.1f, 0f), new GradientAlphaKey(1f, 1f) });
                Gradient full = new Gradient();
                full.SetKeys(new[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(Color.white, 1f) },
                             new[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(1f, 1f) });
                _grads = new[] { head, full, full, full };
            }

            for (int i = 0; i < _lines.Length; i++)
            {
                _lines[i] = new List<Ln>();
            }

            Material star = starMaterial != null ? starMaterial : dotMaterial;
            _dots = MakeParticles("Dots", dotMaterial, false, 0f, false);
            _sparks = MakeParticles("Sparks", dotMaterial, true, 0.45f, false);
            _stars = MakeParticles("Stars", star, false, 0f, true);
        }

        static AnimationCurve MakeCurve(System.Func<float, float> f)
        {
            const int n = 17;
            Keyframe[] keys = new Keyframe[n];
            for (int i = 0; i < n; i++)
            {
                float s = i / (float)(n - 1);
                keys[i] = new Keyframe(s, Mathf.Max(0f, f(s)), 0f, 0f);
            }

            return new AnimationCurve(keys);
        }

        ParticleSystem MakeParticles(string objName, Material mat, bool stretch, float gravity, bool shrink)
        {
            GameObject go = new GameObject(objName);
            go.transform.SetParent(transform, false);
            ParticleSystem ps = go.AddComponent<ParticleSystem>();
            ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

            ParticleSystem.MainModule main = ps.main;
            main.loop = true;
            main.playOnAwake = false;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.startLifetime = 0.6f;
            main.startSpeed = 0f;
            main.startSize = 0.1f;
            main.maxParticles = 1500;
            main.gravityModifier = gravity;
            main.useUnscaledTime = useUnscaledTime;

            ParticleSystem.EmissionModule em = ps.emission;
            em.enabled = false;
            ParticleSystem.ShapeModule sh = ps.shape;
            sh.enabled = false;

            ParticleSystem.ColorOverLifetimeModule col = ps.colorOverLifetime;
            col.enabled = true;
            Gradient g = new Gradient();
            g.SetKeys(new[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(Color.white, 1f) },
                      new[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(1f, 0.5f), new GradientAlphaKey(0f, 1f) });
            col.color = g;

            if (shrink)
            {
                ParticleSystem.SizeOverLifetimeModule sol = ps.sizeOverLifetime;
                sol.enabled = true;
                sol.size = new ParticleSystem.MinMaxCurve(1f, new AnimationCurve(new Keyframe(0f, 1f), new Keyframe(1f, 0f)));
            }

            ParticleSystemRenderer r = go.GetComponent<ParticleSystemRenderer>();
            r.sharedMaterial = mat;
            r.sortingOrder = sortingOrder + 1;
            if (stretch)
            {
                r.renderMode = ParticleSystemRenderMode.Stretch;
                r.velocityScale = 0.04f;
                r.lengthScale = 1.6f;
            }

            ps.Play();
            return ps;
        }

        Ln NewLine(Taper taper)
        {
            GameObject go = new GameObject("Line");
            go.transform.SetParent(transform, false);
            LineRenderer lr = go.AddComponent<LineRenderer>();
            lr.useWorldSpace = true;
            lr.alignment = LineAlignment.View;
            lr.textureMode = LineTextureMode.Stretch;
            lr.numCapVertices = 0;
            lr.numCornerVertices = 0;
            lr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            lr.receiveShadows = false;
            lr.sharedMaterial = lineMaterial;
            lr.sortingOrder = sortingOrder;
            lr.widthCurve = _curves[(int)taper];
            lr.colorGradient = _grads[(int)taper];
            lr.enabled = false;
            return new Ln { lr = lr, mpb = new MaterialPropertyBlock() };
        }

        // ------------------------------------------------------------------ drawing helpers

        /// <summary>Draws the first n points of the scratch buffer (local space) as one light stroke.</summary>
        void Draw(Taper taper, int n, float width, Color tint, float gain)
        {
            gain *= _gm * (1f + _pulse * 1.2f) * 1.25f;
            width *= 1.3f;
            if (n < 2 || gain <= 0.003f)
            {
                return;
            }

            int ti = (int)taper;
            List<Ln> list = _lines[ti];
            int idx = _lineUsed[ti]++;
            if (idx >= list.Count)
            {
                list.Add(NewLine(taper));
            }

            Ln l = list[idx];
            l.lr.positionCount = n;
            for (int i = 0; i < n; i++)
            {
                l.lr.SetPosition(i, _l2w.MultiplyPoint3x4(_buf[i]));
            }

            l.lr.widthMultiplier = width * _scale;
            Color c = tint * gain;
            c.a = 1f;
            l.mpb.SetColor(ID_Tint, c);
            l.lr.SetPropertyBlock(l.mpb);
            l.lr.enabled = true;

            if (width >= 0.06f && gain > 0.3f && Random.value < sparkle)
            {
                Vector3 q = _buf[Random.Range(0, n)];
                Emit(_stars, q.x, q.y, (Random.value - 0.5f) * 0.6f, (Random.value - 0.5f) * 0.6f, 0.1f + 0.14f * Random.value, 0.45f, Hue(primary));
            }
        }

        void EndFrame()
        {
            for (int ti = 0; ti < _lines.Length; ti++)
            {
                List<Ln> list = _lines[ti];
                for (int i = _lineUsed[ti]; i < list.Count; i++)
                {
                    list[i].lr.enabled = false;
                }

                _lineUsed[ti] = 0;
            }
        }

        void HideAllLines()
        {
            if (!_built)
            {
                return;
            }

            for (int ti = 0; ti < _lines.Length; ti++)
            {
                foreach (Ln l in _lines[ti])
                {
                    l.lr.enabled = false;
                }

                _lineUsed[ti] = 0;
            }
        }

        int Seg(float x0, float y0, float x1, float y1, int n)
        {
            for (int i = 0; i < n; i++)
            {
                float s = n > 1 ? i / (float)(n - 1) : 0f;
                _buf[i] = new Vector3(Mathf.Lerp(x0, x1, s), Mathf.Lerp(y0, y1, s), 0f);
            }

            return n;
        }

        int Ring(float cx, float cy, float rx, float ry)
        {
            const int n = 40;
            for (int i = 0; i <= n; i++)
            {
                float a = Mathf.PI * 2f * i / n;
                _buf[i] = new Vector3(cx + rx * Mathf.Cos(a), cy + ry * Mathf.Sin(a), 0f);
            }

            return n + 1;
        }

        static Vector2 Bez(Vector2 a, Vector2 b, Vector2 c, float s)
        {
            float u = 1f - s;
            return u * u * a + 2f * u * s * b + s * s * c;
        }

        float Rnd(int i, int salt = 0)
        {
            float v = Mathf.Sin((i + 1) * 12.9898f + _seed * 78.233f + salt * 37.719f) * 43758.5453f;
            return v - Mathf.Floor(v);
        }

        static float EaseOut(float x)
        {
            float u = 1f - Mathf.Clamp01(x);
            return 1f - u * u * u;
        }

        // ------------------------------------------------------------------ particles

        static Color Hue(Color c)
        {
            float m = Mathf.Max(c.r, Mathf.Max(c.g, c.b));
            if (m <= 0.0001f)
            {
                return Color.white;
            }

            return new Color(c.r / m, c.g / m, c.b / m, 1f);
        }

        void Emit(ParticleSystem ps, float x, float y, float vx, float vy, float size, float life, Color color)
        {
            int copies = 1;
            if (ps != _dots)
            {
                copies = Mathf.Max(1, Mathf.FloorToInt(particleMultiplier));
                if (Random.value < particleMultiplier - Mathf.Floor(particleMultiplier))
                {
                    copies++;
                }
            }

            for (int c = 0; c < copies; c++)
            {
                float jx = 0f, jy = 0f, jv = 1f, js = 1f;
                if (c > 0)
                {
                    jx = (Random.value - 0.5f) * 0.12f;
                    jy = (Random.value - 0.5f) * 0.12f;
                    jv = 0.75f + 0.5f * Random.value;
                    js = 0.7f + 0.6f * Random.value;
                }

                ParticleSystem.EmitParams ep = new ParticleSystem.EmitParams();
                ep.position = _l2w.MultiplyPoint3x4(new Vector3(x + jx, y + jy, 0f));
                ep.velocity = _l2w.MultiplyVector(new Vector3(vx * jv, vy * jv, 0f));
                ep.startSize = size * js * _scale;
                ep.startLifetime = life;
                ep.startColor = color;
                ps.Emit(ep, 1);
            }
        }

        void ClearParticles()
        {
            if (_dots != null) _dots.Clear(true);
            if (_sparks != null) _sparks.Clear(true);
            if (_stars != null) _stars.Clear(true);
        }

        bool ParticlesDone()
        {
            return _dots.particleCount == 0 && _sparks.particleCount == 0 && _stars.particleCount == 0;
        }

        Color Pick(int i)
        {
            int m = i % 3;
            return Hue(m == 0 ? primary : m == 1 ? secondary : tertiary);
        }

        // ------------------------------------------------------------------ 1. spear thrust

        void AnimThrust()
        {
            const float t0 = 0.06f; // wind-up ends
            const float t1 = 0.2f;  // full extension
            float fade = Mathf.Clamp01((_t - t1) / Mathf.Max(0.05f, duration - t1));
            float g = 1f - fade;
            float p = Mathf.Clamp01((_t - t0) / (t1 - t0));
            float e = EaseOut(p);
            float tip = Mathf.Lerp(0.5f, reach, e);

            if (_t < t0)
            {
                Emit(_dots, 0.5f, 0f, 0f, 0f, 0.4f, 0.08f, Hue(primary));
                return;
            }

            float len = Mathf.Lerp(0.2f, Mathf.Min(1.9f, reach), e);
            float x0 = Mathf.Lerp(tip - len, tip - 0.05f, fade);

            int n = Seg(x0, 0f, tip, 0f, 12);
            Draw(Taper.Head, n, 0.28f * thickness, secondary * 0.6f, g * 0.7f);
            Draw(Taper.Head, n, 0.15f * thickness, primary, g);
            Draw(Taper.Head, n, 0.06f * thickness, _core, g);

            // two coiling ribbons around the shaft
            const int hn = 36;
            for (int h = 0; h < 2; h++)
            {
                for (int i = 0; i < hn; i++)
                {
                    float s = i / (float)(hn - 1);
                    float x = x0 + 0.1f + Mathf.Max(0f, tip - x0 - 0.1f) * s;
                    float y = 0.16f * Mathf.Pow(s, 0.8f) * Mathf.Sin(6.2832f * 3.2f * s + (h == 0 ? 0f : 3.1416f) + _t * 18f);
                    _buf[i] = new Vector3(x, y, 0f);
                }

                Draw(Taper.Head, hn, 0.07f * thickness, h == 0 ? tertiary : secondary, g * 0.9f);
            }

            // ghost spears
            for (int j = 1; j <= 3; j++)
            {
                n = Seg(tip - 1.25f - 0.22f * j, 0f, tip - 0.22f * j, 0f, 2);
                Draw(Taper.Head, n, 0.03f * thickness, primary, g * 0.28f / j);
            }

            // speed lines
            for (int j = 0; j < 7; j++)
            {
                float side = Rnd(j, 5) > 0.5f ? 1f : -1f;
                float y = side * (0.1f + 0.46f * Rnd(j));
                float xe = tip - (0.1f + 0.6f * Rnd(j, 1));
                float ln = 0.45f + 0.85f * Rnd(j, 2);
                n = Seg(xe - ln, y, xe, y, 2);
                Draw(Taper.Head, n, 0.028f * thickness, primary * 0.8f, g * 0.6f);
            }

            if (p < 1f)
            {
                _acc += _dt * 90f;
                while (_acc >= 1f)
                {
                    _acc -= 1f;
                    float k = Random.value;
                    Emit(_sparks, tip - k * 0.5f, (Random.value - 0.5f) * 0.3f, -2f - 3f * Random.value, (Random.value - 0.5f) * 0.8f,
                         0.05f, 0.3f, Pick(Random.Range(0, 3)));
                }
            }
            else
            {
                if (!_fired)
                {
                    _fired = true;
                    if (onImpact != null) onImpact.Invoke();
                    Emit(_dots, tip + 0.05f, 0f, 0f, 0f, 1.5f, 0.22f, Hue(primary));
                    Emit(_dots, tip + 0.05f, 0f, 0f, 0f, 0.9f, 0.45f, Hue(primary));
                    for (int j = 0; j < 22; j++)
                    {
                        float a = Mathf.Lerp(-80f, 80f, Random.value) * Mathf.Deg2Rad;
                        float sp = 2.5f + 4f * Random.value;
                        Emit(_sparks, tip + 0.08f, 0f, Mathf.Cos(a) * sp, Mathf.Sin(a) * sp * 1.4f, 0.06f, 0.35f, Pick(j));
                    }

                    for (int j = 0; j < 14; j++)
                    {
                        Emit(_stars, tip + (Random.value - 0.2f) * 0.7f, (Random.value - 0.5f) * 1.2f, Random.value * 1.2f, (Random.value - 0.5f) * 1f,
                             0.12f + 0.2f * Random.value, 0.5f + 0.3f * Random.value, Pick(j));
                    }
                }

                // air-burst rings
                float bp = Mathf.Clamp01((_t - t1) / 0.35f);
                float sc = 1f + 0.8f * EaseOut(bp);
                for (int j = 0; j < 3; j++)
                {
                    float rx = (0.1f + 0.1f * j) * sc;
                    float ry = (0.28f + 0.18f * j) * sc;
                    n = Ring(tip + 0.08f + (sc - 1f) * 0.14f * j, 0f, rx, ry);
                    Draw(Taper.Flat, n, (0.03f - 0.005f * j) * thickness, j == 0 ? primary : j == 1 ? secondary : tertiary,
                         (1f - bp * 0.6f) * g * (1f - 0.2f * j));
                }
            }
        }

        // ------------------------------------------------------------------ 2. spear sweep

        void AnimSweep()
        {
            const float sweepTime = 0.5f;
            float p = Mathf.Clamp01(_t / sweepTime);
            float pe = 1f - (1f - p) * (1f - p);
            float fade = Mathf.Clamp01((_t - sweepTime) / Mathf.Max(0.05f, duration - sweepTime));
            float g = 1f - fade;

            Vector2 c = sweepCenter;
            Vector2 headPos = c;
            for (int layer = 0; layer < 3; layer++)
            {
                float rm = layer == 0 ? 1f : layer == 1 ? 0.9f : 1.13f;
                float tail = layer == 0 ? 235f : layer == 1 ? 200f : 170f;
                float lag = layer == 0 ? 0f : layer == 1 ? 18f : 34f;
                float wm = layer == 0 ? 0.1f : layer == 1 ? 0.06f : 0.05f;
                Color col = layer == 0 ? primary : layer == 1 ? secondary : tertiary;

                float head = -150f + 420f * pe - lag;
                float a0 = Mathf.Max(head - tail, -150f);
                float a1 = Mathf.Max(head, a0 + 1f);
                const int n = 56;
                for (int i = 0; i < n; i++)
                {
                    float a = Mathf.Lerp(a0, a1, i / (float)(n - 1)) * Mathf.Deg2Rad;
                    _buf[i] = new Vector3(c.x + sweepRadius.x * rm * Mathf.Cos(a), c.y + sweepRadius.y * rm * Mathf.Sin(a), 0f);
                }

                Draw(Taper.Head, n, wm * 2.4f * thickness, col * 0.4f, g * 0.5f);
                Draw(Taper.Head, n, wm * thickness, col, g);
                if (layer == 0)
                {
                    Draw(Taper.Head, n, wm * 0.4f * thickness, _core, g);
                    headPos = _buf[n - 1];
                }

                // glitter left behind the tip
                if (p < 1f)
                {
                    _acc += _dt * 70f;
                    while (_acc >= 1f)
                    {
                        _acc -= 1f;
                        int i = Random.Range(n / 2, n);
                        Vector3 q = _buf[i];
                        Emit(Random.value > 0.5f ? _stars : _sparks, q.x + (Random.value - 0.5f) * 0.15f, q.y + (Random.value - 0.5f) * 0.15f,
                             (Random.value - 0.5f) * 0.6f, (Random.value - 0.5f) * 0.6f, 0.07f + 0.1f * Random.value, 0.4f, Pick(layer));
                    }
                }
            }

            if (p < 1f)
            {
                Emit(_dots, headPos.x, headPos.y, 0f, 0f, 0.5f, 0.07f, Hue(primary));
            }

            if (!_fired && p >= 0.55f)
            {
                _fired = true;
                if (onImpact != null) onImpact.Invoke();
            }

            // ground rings
            for (int j = 0; j < 2; j++)
            {
                int n = Ring(c.x, groundY, sweepRadius.x * (j == 0 ? 0.8f : 1.05f), j == 0 ? 0.09f : 0.14f);
                Draw(Taper.Flat, n, 0.016f * thickness, j == 0 ? primary : secondary, g * (j == 0 ? 0.6f : 0.35f));
            }
        }

        // ------------------------------------------------------------------ 3. hammer smash

        void BuildCracks()
        {
            const int count = 10;
            const int pts = 12;
            if (_cracks == null || _cracks.Length != count)
            {
                _cracks = new Vector2[count][];
                for (int j = 0; j < count; j++)
                {
                    _cracks[j] = new Vector2[pts];
                }
            }

            for (int j = 0; j < count; j++)
            {
                float phi = (Mathf.Lerp(-170f, 170f, j / (float)(count - 1)) + (Rnd(j) - 0.5f) * 16f) * Mathf.Deg2Rad;
                float len = (0.7f + 0.8f * Rnd(j, 1)) * _crackScale;
                float off = 0f;
                for (int i = 0; i < pts; i++)
                {
                    float f = i / (float)(pts - 1) * len;
                    off += (Rnd(j * 20 + i, 2) - 0.5f) * 0.1f;
                    _cracks[j][i] = new Vector2(impactOffset.x + f * Mathf.Cos(phi) - off * Mathf.Sin(phi) * 0.3f,
                                                impactOffset.y + f * Mathf.Sin(phi) * 0.28f + off * Mathf.Cos(phi) * 0.3f);
                }
            }
        }

        void AnimHammer()
        {
            const float chargeEnd = 0.25f;
            const float swingEnd = 0.4f;
            Vector2 imp = impactOffset;
            Vector2 headRest = new Vector2(0.35f, 0.95f);

            if (_t < chargeEnd)
            {
                // gathering energy around the raised hammer head
                float k = _t / chargeEnd;
                Emit(_dots, headRest.x, headRest.y, 0f, 0f, 0.5f + 0.5f * k, 0.07f, Hue(secondary));
                int n = Ring(headRest.x, headRest.y, 0.55f * (1f - 0.55f * k), 0.55f * (1f - 0.55f * k));
                Draw(Taper.Flat, n, 0.02f * thickness, secondary, 0.4f + 0.6f * k);
                _acc += _dt * 50f;
                while (_acc >= 1f)
                {
                    _acc -= 1f;
                    float a = Random.value * 6.2832f;
                    float r = 0.5f;
                    Emit(_stars, headRest.x + Mathf.Cos(a) * r, headRest.y + Mathf.Sin(a) * r, -Mathf.Cos(a) * 1.2f, -Mathf.Sin(a) * 1.2f,
                         0.12f + 0.1f * Random.value, 0.4f, Pick(Random.Range(0, 3)));
                }
            }
            else if (_t < swingEnd)
            {
                float sp = (_t - chargeEnd) / (swingEnd - chargeEnd);
                float h = sp * sp;
                Vector2 b0 = new Vector2(-0.2f, 0.85f);
                Vector2 b1 = new Vector2(imp.x + 0.2f, 1.1f);
                Vector2 b2 = imp + new Vector2(0f, 0.1f);
                float s0 = Mathf.Max(0f, h - 0.7f);
                for (int layer = 0; layer < 3; layer++)
                {
                    Vector2 off = layer == 0 ? Vector2.zero : layer == 1 ? new Vector2(-0.14f, -0.14f) : new Vector2(0.12f, 0.12f);
                    const int n = 28;
                    for (int i = 0; i < n; i++)
                    {
                        float s = Mathf.Lerp(s0, h, i / (float)(n - 1));
                        Vector2 q = Bez(b0, b1, b2, s) + off * (1f - s);
                        _buf[i] = new Vector3(q.x, q.y, 0f);
                    }

                    Draw(Taper.Head, n, (layer == 0 ? 0.3f : layer == 1 ? 0.16f : 0.12f) * thickness,
                         layer == 0 ? primary : layer == 1 ? tertiary : secondary, layer == 0 ? 1f : 0.75f);
                    if (layer == 0)
                    {
                        Draw(Taper.Head, n, 0.1f * thickness, _core, 1f);
                        Vector3 hp = _buf[n - 1];
                        Emit(_dots, hp.x, hp.y, 0f, 0f, 0.7f, 0.08f, Hue(primary));
                        Emit(_stars, hp.x + (Random.value - 0.5f) * 0.3f, hp.y + (Random.value - 0.5f) * 0.3f, 0f, 0f, 0.15f, 0.3f, Pick(Random.Range(0, 3)));
                    }
                }
            }
            else
            {
                float it = _t - swingEnd;
                float fade = Mathf.Clamp01((it - 0.35f) / Mathf.Max(0.05f, duration - swingEnd - 0.35f));
                float g = 1f - fade;

                if (!_fired)
                {
                    _fired = true;
                    if (onImpact != null) onImpact.Invoke();

                    Emit(_dots, imp.x, imp.y + 0.05f, 0f, 0f, 2.6f, 0.25f, Hue(primary));
                    Emit(_dots, imp.x, imp.y + 0.1f, 0f, 0f, 1.5f, 0.45f, Hue(primary));
                    for (int j = 0; j < 9; j++)
                    {
                        float a = Mathf.Lerp(8f, 172f, j / 8f) * Mathf.Deg2Rad;
                        Emit(_sparks, imp.x, imp.y + 0.05f, Mathf.Cos(a) * 7f, Mathf.Sin(a) * 7f, 0.05f, 0.18f, Pick(j));
                    }

                    for (int j = 0; j < 34; j++)
                    {
                        Emit(_sparks, imp.x + (Random.value - 0.5f) * 0.4f, imp.y + 0.05f, (Random.value - 0.5f) * 2.6f, 1.4f + 1.7f * Random.value,
                             0.05f + 0.05f * Random.value, 0.8f + 0.3f * Random.value, Pick(j));
                    }

                    for (int j = 0; j < 12; j++)
                    {
                        Emit(_stars, imp.x + (Random.value - 0.5f) * 2f, imp.y + Random.value * 1.1f, (Random.value - 0.5f) * 1f, Random.value * 1f,
                             0.15f + 0.2f * Random.value, 0.6f + 0.4f * Random.value, Pick(j));
                    }

                    Color dust = Hue(primary) * 0.35f;
                    dust.a = 1f;
                    for (int j = 0; j < 6; j++)
                    {
                        Emit(_dots, imp.x + (j - 2.5f) * 0.3f, imp.y + 0.1f, (j - 2.5f) * 0.25f, 0.2f, 0.9f, 0.9f, dust);
                    }
                }

                // light pillar
                float pil = 1f - Mathf.Clamp01(it / 0.3f);
                if (pil > 0f)
                {
                    int n = Seg(imp.x, imp.y + 4.5f, imp.x, imp.y + 0.05f, 2);
                    Draw(Taper.Head, n, 0.34f * thickness, secondary, pil * 0.9f);
                    Draw(Taper.Head, n, 0.12f * thickness, _core, pil);
                }

                // cracks
                float gr = EaseOut(it / 0.35f);
                for (int j = 0; j < _cracks.Length; j++)
                {
                    int m = Mathf.Clamp(Mathf.CeilToInt(gr * _cracks[j].Length), 2, _cracks[j].Length);
                    for (int i = 0; i < m; i++)
                    {
                        _buf[i] = new Vector3(_cracks[j][i].x, _cracks[j][i].y, 0f);
                    }

                    Draw(Taper.Tail, m, 0.055f * thickness, primary, g * 0.9f);
                }

                // shockwave rings
                float rp = EaseOut(it / 0.5f);
                for (int j = 0; j < 3; j++)
                {
                    float rx = (j == 0 ? 1.25f : j == 1 ? 0.88f : 0.5f) * rp;
                    float ry = rx * 0.22f;
                    int n = Ring(imp.x, imp.y, rx, ry);
                    Draw(Taper.Flat, n, (0.034f - 0.006f * j) * thickness, j == 0 ? primary : j == 1 ? tertiary : secondary,
                         g * (j == 0 ? 0.9f : 0.7f) * (1f - rp * 0.35f));
                }
            }
        }

        // ------------------------------------------------------------------ 4. sword qi

        void QiCrescent(float fx, float radius, float width, Color col, float gain, float shift, int taperKind)
        {
            const int n = 24;
            float rx = radius * 0.5f;
            float ry = radius * 1.15f;
            for (int i = 0; i < n; i++)
            {
                float a = Mathf.Lerp(-68f, 68f, i / (float)(n - 1)) * Mathf.Deg2Rad;
                _buf[i] = new Vector3(fx - rx - shift + rx * Mathf.Cos(a), ry * Mathf.Sin(a), 0f);
            }

            Draw(Taper.Lens, n, width * thickness, col, gain);
        }

        void AnimQi()
        {
            float fadeStart = duration - 0.35f;
            float fade = Mathf.Clamp01((_t - fadeStart) / 0.35f);
            float g = 1f - fade;
            float p = Mathf.Clamp01(_t / Mathf.Max(0.1f, duration - 0.2f));
            float fx = Mathf.Lerp(0.55f, qiTravel, 1f - Mathf.Pow(1f - p, 1.5f));
            float grow = EaseOut(_t / 0.6f);
            float r = Mathf.Lerp(0.34f, 0.62f, grow);
            float wm = Mathf.Lerp(0.09f, 0.15f, grow);

            QiCrescent(fx - 0.2f, r * 0.78f, wm * 0.78f, tertiary, g * 0.8f, 0f, 0);
            QiCrescent(fx - 0.09f, r * 0.9f, wm * 0.9f, secondary, g * 0.85f, 0f, 0);
            QiCrescent(fx - 0.09f, r * 0.9f, wm * 0.9f, secondary, g * 0.26f, 0.22f, 0);
            for (int j = 3; j >= 1; j--)
            {
                QiCrescent(fx, r, wm * (1f - 0.1f * j), primary, g * 0.3f * Mathf.Pow(0.62f, j - 1), 0.22f * j, 0);
            }

            QiCrescent(fx, r, wm, primary, g, 0f, 0);
            QiCrescent(fx, r, wm * 0.4f, _core, g, 0f, 0);

            for (int j = 0; j < 5; j++)
            {
                float y = (j - 2) * 0.32f + (Rnd(j) - 0.5f) * 0.1f;
                float xe = fx - 0.45f - 0.06f * j;
                int n = Seg(xe - 0.8f, y, xe, y, 2);
                Draw(Taper.Head, n, 0.026f * thickness, primary, g * 0.5f);
            }

            int gn = Seg(fx - 1.6f, groundY, fx, groundY, 2);
            Draw(Taper.Head, gn, 0.05f * thickness, primary, g * 0.5f);

            if (_t < duration - 0.3f)
            {
                Emit(_dots, fx, 0f, 0f, 0f, r * 1.1f, 0.06f, Hue(primary) * 0.7f + new Color(0.3f, 0.3f, 0.3f, 1f));
                _acc += _dt * 60f;
                while (_acc >= 1f)
                {
                    _acc -= 1f;
                    float x = fx - Random.value * 1.4f;
                    float y = (Random.value - 0.5f) * 1.6f;
                    Emit(Random.value > 0.5f ? _stars : _sparks, x, y, -0.5f - Random.value, 0f, 0.07f + 0.12f * Random.value, 0.45f, Pick(Random.Range(0, 3)));
                }
            }

            if (!_fired && _t >= 0.15f)
            {
                _fired = true;
                if (onImpact != null) onImpact.Invoke();
            }
        }

        // ------------------------------------------------------------------ 5. cross slash (X + horizontal)

        static readonly Vector2[][] SlashCtl =
        {
            new[] { new Vector2(-0.70f, 0.66f), new Vector2(-0.05f, 0.14f), new Vector2(0.70f, -0.61f) },
            new[] { new Vector2(-0.70f, -0.68f), new Vector2(0.05f, -0.36f), new Vector2(0.75f, 0.62f) },
            new[] { new Vector2(-0.85f, -0.01f), new Vector2(0.05f, 0.14f), new Vector2(0.95f, -0.04f) },
        };

        static readonly float[] SlashStart = { 0f, 0.16f, 0.3f };

        void AnimCross()
        {
            const float fadeStart = 0.5f;
            float fade = Mathf.Clamp01((_t - fadeStart) / Mathf.Max(0.05f, duration - fadeStart));
            float g = 1f - fade;

            for (int k = 0; k < 3; k++)
            {
                float h = EaseOut((_t - SlashStart[k]) / (k == 2 ? 0.14f : 0.12f));
                if (h <= 0f)
                {
                    continue;
                }

                Color col = k == 0 ? primary : k == 1 ? tertiary : secondary;
                bool done = h >= 0.999f;
                float s0 = done ? 0f : Mathf.Max(0f, h - 0.85f);
                const int n = 24;
                for (int i = 0; i < n; i++)
                {
                    float s = Mathf.Lerp(s0, h, i / (float)(n - 1));
                    Vector2 q = Bez(SlashCtl[k][0], SlashCtl[k][1], SlashCtl[k][2], s);
                    _buf[i] = new Vector3(q.x, q.y, 0f);
                }

                Taper tp = done ? Taper.Lens : Taper.Head;
                float wk = k == 2 ? 0.75f : 1f;
                Draw(tp, n, 0.19f * wk * thickness, col * 0.5f, g * 0.5f);
                Draw(tp, n, 0.09f * wk * thickness, col, g);
                Draw(tp, n, 0.04f * wk * thickness, _core, g * 0.8f);

                if (!done)
                {
                    Vector3 hp = _buf[n - 1];
                    Emit(_dots, hp.x, hp.y, 0f, 0f, 0.45f, 0.07f, Hue(primary));
                    Emit(_stars, hp.x + (Random.value - 0.5f) * 0.3f, hp.y + (Random.value - 0.5f) * 0.3f, 0f, 0f, 0.12f + 0.1f * Random.value, 0.35f, Hue(col));
                }
            }

            if (_t >= SlashStart[2])
            {
                if (!_fired)
                {
                    _fired = true;
                    if (onImpact != null) onImpact.Invoke();

                    Emit(_dots, 0f, 0f, 0f, 0f, 2.4f, 0.22f, Hue(primary));
                    Emit(_dots, 0f, 0f, 0f, 0f, 1.6f, 0.4f, Hue(tertiary));
                    for (int j = 0; j < 26; j++)
                    {
                        float a = Random.value * 6.2832f;
                        float sp = 2.5f + 5f * Random.value;
                        Emit(_sparks, Mathf.Cos(a) * 0.1f, Mathf.Sin(a) * 0.1f, Mathf.Cos(a) * sp, Mathf.Sin(a) * sp, 0.05f + 0.04f * Random.value, 0.4f, Pick(j));
                    }

                    for (int j = 0; j < 12; j++)
                    {
                        float a = Random.value * 6.2832f;
                        float r = 0.2f + 0.7f * Random.value;
                        Emit(_stars, Mathf.Cos(a) * r, Mathf.Sin(a) * r, Mathf.Cos(a) * 0.6f, Mathf.Sin(a) * 0.6f, 0.15f + 0.2f * Random.value, 0.6f, Pick(j));
                    }
                }

                float bp = Mathf.Clamp01((_t - SlashStart[2]) / 0.4f);
                for (int j = 0; j < 3; j++)
                {
                    float rr = (0.26f + 0.2f * j) * (1f + 0.5f * EaseOut(bp));
                    int n = Ring(0f, 0f, rr, rr);
                    Draw(Taper.Flat, n, (0.03f - 0.006f * j) * thickness, j == 1 ? secondary : primary, (1f - bp * 0.5f) * g * (0.9f - 0.25f * j));
                }
            }
        }
    }
}
