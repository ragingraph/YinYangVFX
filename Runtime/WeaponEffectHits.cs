using UnityEngine;

namespace YinYangVFX
{
    // Hit effects. Origin = the point where the hit lands (put it on the enemy). Single colour per prefab.
    // The "crit badge" is only the light behind a "CRITICAL" text: put your own text sprite on top.
    public partial class WeaponEffect
    {
        void AnimHitSlash()
        {
            float g = FadeFrom(0.18f);
            float e = EaseOut(_t / 0.08f);
            if (Once(1))
            {
                Fire();
                Burst(0f, 0f, 14, 5f, 6, 2.0f);
                for (int j = 0; j < 26; j++)
                {
                    float a = (35f + 90f + (Random.value - 0.5f) * 80f) * Mathf.Deg2Rad;
                    float sp = 4f + 5f * Random.value;
                    Emit(_sparks, 0f, 0f, Mathf.Cos(a) * sp, Mathf.Sin(a) * sp, 0.05f + 0.04f * Random.value, 0.35f, Hue(primary));
                    Emit(_sparks, 0f, 0f, -Mathf.Cos(a) * sp, -Mathf.Sin(a) * sp, 0.05f + 0.04f * Random.value, 0.3f, Hue(primary));
                }
            }

            float a0 = -35f * Mathf.Deg2Rad;
            float len = 1.3f * e;
            float dx = Mathf.Cos(a0), dy = Mathf.Sin(a0);
            int n = Seg(-dx * len, -dy * len, dx * len, dy * len, 2);
            Draw(Taper.Lens, n, 0.16f * thickness, primary, g);
            Draw(Taper.Lens, n, 0.06f * thickness, _core, g);
            n = Seg(-dx * len * 0.6f - dy * 0.12f, -dy * len * 0.6f + dx * 0.12f, dx * len * 0.6f - dy * 0.12f, dy * len * 0.6f + dx * 0.12f, 2);
            Draw(Taper.Lens, n, 0.07f * thickness, primary, g * 0.7f);
            int r = Ring(0f, 0f, 0.1f + 0.5f * e, 0.1f + 0.5f * e);
            Draw(Taper.Flat, r, 0.035f * thickness, primary, (1f - e * 0.5f) * g);
        }

        void AnimHitBlunt()
        {
            float g = FadeFrom(0.25f);
            if (Once(1))
            {
                Fire();
                Burst(0f, 0f, 30, 6f, 8, 2.8f);
                for (int j = 0; j < 6; j++)
                {
                    Emit(_dots, (Random.value - 0.5f) * 0.5f, (Random.value - 0.5f) * 0.4f, (Random.value - 0.5f) * 1.2f, 0.3f, 0.7f, 0.6f, Tint(0.3f));
                }
            }

            for (int j = 0; j < 3; j++)
            {
                float d = Mathf.Clamp01((_t - 0.04f * j) / 0.3f);
                float e = EaseOut(d);
                int n = Ring(0f, 0f, 1.0f * e, 1.0f * e);
                Draw(Taper.Flat, n, (0.09f - 0.025f * j) * thickness, primary, (1f - d * 0.8f) * g * (1f - 0.2f * j));
            }

            float se = EaseOut(_t / 0.1f);
            for (int k = 0; k < 8; k++)
            {
                float a = (k * 45f + 22.5f) * Mathf.Deg2Rad;
                float L = (k % 2 == 0 ? 0.9f : 0.55f) * se;
                int n = Seg(Mathf.Cos(a) * 0.12f, Mathf.Sin(a) * 0.12f, Mathf.Cos(a) * L, Mathf.Sin(a) * L, 2);
                Draw(Taper.Lens, n, 0.11f * thickness, primary, g);
            }
        }

        void AnimHitElemental()
        {
            float g = FadeFrom(0.35f);
            float e = EaseOut(_t / 0.35f);
            if (Once(1))
            {
                Fire();
                Burst(0f, 0f, 24, 5f, 14, 2.4f);
            }

            float rot = _t * 360f;
            for (int k = 0; k < 3; k++)
            {
                float r = (0.3f + 0.35f * k) * (0.4f + 0.6f * e);
                float a0 = rot * (k % 2 == 0 ? 1f : -1f) + k * 70f;
                int n = ArcPts(0f, 0f, r, r, a0, a0 + 200f, 20);
                Draw(Taper.Head, n, 0.1f * thickness, primary, g * (1f - 0.15f * k));
            }

            for (int p = 0; p < 6; p++)
            {
                float a = (p * 60f + rot * 0.5f) * Mathf.Deg2Rad;
                const int m = 12;
                for (int i = 0; i < m; i++)
                {
                    float s = i / (float)(m - 1);
                    float rr = (0.15f + 1.1f * s) * e;
                    float aa = a + s * 1.1f;
                    _buf[i] = new Vector3(rr * Mathf.Cos(aa), rr * Mathf.Sin(aa), 0f);
                }

                Draw(Taper.Head, m, 0.08f * thickness, primary, g * 0.8f);
            }

            if (_t < 0.3f)
            {
                _acc += _dt * 60f;
                while (_acc >= 1f)
                {
                    _acc -= 1f;
                    float a = Random.value * 6.2832f;
                    Emit(_stars, Mathf.Cos(a) * 0.3f, Mathf.Sin(a) * 0.3f, Mathf.Cos(a) * 1.5f, Mathf.Sin(a) * 1.5f, 0.12f + 0.1f * Random.value, 0.5f, Hue(primary));
                }
            }
        }

        void AnimHitPierce()
        {
            float g = FadeFrom(0.2f);
            float e = EaseOut(_t / 0.07f);
            if (Once(1))
            {
                Fire();
                Burst(0f, 0f, 12, 4f, 5, 1.8f);
                for (int j = 0; j < 22; j++)
                {
                    float a = (Random.value - 0.5f) * 70f * Mathf.Deg2Rad;
                    float sp = 5f + 6f * Random.value;
                    Emit(_sparks, 0.1f, 0f, Mathf.Cos(a) * sp, Mathf.Sin(a) * sp, 0.05f, 0.3f, Hue(primary));
                }
            }

            int n = Seg(-0.3f * e, 0f, 1.3f * e, 0f, 2);
            Draw(Taper.Lens, n, 0.14f * thickness, primary, g);
            Draw(Taper.Lens, n, 0.05f * thickness, _core, g);
            for (int k = 0; k < 6; k++)
            {
                float a = Mathf.Lerp(-55f, 55f, k / 5f) * Mathf.Deg2Rad;
                int m = Seg(0.1f, 0f, 0.1f + Mathf.Cos(a) * 0.9f * e, Mathf.Sin(a) * 0.9f * e, 2);
                Draw(Taper.Head, m, 0.04f * thickness, primary, g * 0.8f);
            }

            float re = EaseOut(_t / 0.25f);
            int rn = Ring(0.2f, 0f, 0.1f + 0.2f * re, 0.2f + 0.5f * re);
            Draw(Taper.Flat, rn, 0.03f * thickness, primary, (1f - re) * g);
        }

        void AnimCritHit()
        {
            float g = FadeFrom(0.45f);
            float e = EaseOut(_t / 0.12f);
            if (Once(1))
            {
                Fire();
                Emit(_dots, 0f, 0f, 0f, 0f, 4.2f, 0.25f, Hue(primary));
                Burst(0f, 0f, 50, 8f, 20, 3.5f);
            }

            float rot = _t * 40f;
            for (int k = 0; k < 4; k++)
            {
                float a = (rot + k * 45f) * Mathf.Deg2Rad;
                float L = (k % 2 == 0 ? 1.7f : 1.1f) * e;
                int n = Seg(-Mathf.Cos(a) * L, -Mathf.Sin(a) * L, Mathf.Cos(a) * L, Mathf.Sin(a) * L, 2);
                Draw(Taper.Lens, n, 0.2f * thickness, primary * 0.5f, g * 0.6f);
                Draw(Taper.Lens, n, 0.1f * thickness, primary, g);
                Draw(Taper.Lens, n, 0.04f * thickness, _core, g);
            }

            for (int j = 0; j < 2; j++)
            {
                float d = Mathf.Clamp01((_t - 0.05f * j) / 0.35f);
                float re = EaseOut(d);
                int n = Ring(0f, 0f, 1.4f * re, 1.4f * re);
                Draw(Taper.Flat, n, (0.08f - 0.03f * j) * thickness, primary, (1f - d * 0.8f) * g);
            }
        }

        void AnimCritBadge()
        {
            float g = FadeFrom(0.6f);
            float e = EaseOut(_t / 0.18f);
            if (Once(1))
            {
                Fire();
                Burst(0f, 0f, 20, 5f, 14, 2.0f);
            }

            float rot = _t * 25f;
            for (int k = 0; k < 14; k++)
            {
                float a = (rot + k * (360f / 14f)) * Mathf.Deg2Rad;
                float r0 = 0.35f;
                float r1 = r0 + (k % 2 == 0 ? 1.4f : 0.9f) * e;
                int n = Seg(Mathf.Cos(a) * r0, Mathf.Sin(a) * r0, Mathf.Cos(a) * r1, Mathf.Sin(a) * r1, 2);
                Draw(Taper.Head, n, 0.06f * thickness, primary, g * 0.5f);
            }

            float len = 2.4f * e;
            int h = Seg(-len, 0f, len, 0f, 2);
            Draw(Taper.Lens, h, 0.22f * thickness, primary * 0.5f, g * 0.7f);
            Draw(Taper.Lens, h, 0.1f * thickness, primary, g);
            Draw(Taper.Lens, h, 0.035f * thickness, _core, g);
            h = Seg(0f, -0.5f * e, 0f, 0.5f * e, 2);
            Draw(Taper.Lens, h, 0.08f * thickness, primary, g * 0.8f);

            float d = 0.5f * e;
            _buf[0] = new Vector3(0f, d, 0f);
            _buf[1] = new Vector3(d * 1.4f, 0f, 0f);
            _buf[2] = new Vector3(0f, -d, 0f);
            _buf[3] = new Vector3(-d * 1.4f, 0f, 0f);
            _buf[4] = _buf[0];
            Draw(Taper.Flat, 5, 0.04f * thickness, primary, g);

            if (_t < 0.5f)
            {
                _acc += _dt * 50f;
                while (_acc >= 1f)
                {
                    _acc -= 1f;
                    Emit(_stars, (Random.value - 0.5f) * 3f, (Random.value - 0.5f) * 0.9f, 0f, 0.3f, 0.12f + 0.12f * Random.value, 0.5f, Hue(primary));
                }
            }
        }

        void AnimKillBurst()
        {
            float g = FadeFrom(0.8f);
            float e = EaseOut(_t / 0.2f);
            if (Once(1))
            {
                Fire();
                Emit(_dots, 0f, 0f, 0f, 0f, 5f, 0.3f, Hue(primary));
                Burst(0f, 0f, 90, 9f, 34, 4f);
                for (int j = 0; j < 30; j++)
                {
                    float a = Random.value * 6.2832f;
                    Emit(_dots, 0f, 0f, Mathf.Cos(a) * 1.5f, Mathf.Sin(a) * 1.5f, 0.6f + 0.5f * Random.value, 0.9f, Tint(0.3f));
                }
            }

            for (int j = 0; j < 3; j++)
            {
                float d = Mathf.Clamp01((_t - 0.06f * j) / 0.6f);
                float re = EaseOut(d);
                int n = Ring(0f, 0f, 2.4f * re * (1f - 0.2f * j), 2.4f * re * (1f - 0.2f * j));
                Draw(Taper.Flat, n, (0.1f - 0.03f * j) * thickness, primary, (1f - d * 0.8f) * g * (1f - 0.2f * j));
            }

            for (int k = 0; k < 14; k++)
            {
                float a = (k * (360f / 14f) + Rnd(k) * 12f) * Mathf.Deg2Rad;
                float r0 = 0.3f + 1.5f * e * (0.6f + 0.4f * Rnd(k, 1));
                int n = Seg(Mathf.Cos(a) * (r0 - 0.9f * e), Mathf.Sin(a) * (r0 - 0.9f * e), Mathf.Cos(a) * r0, Mathf.Sin(a) * r0, 2);
                Draw(Taper.Head, n, 0.07f * thickness, primary, g * 0.9f * (1f - 0.5f * Mathf.Clamp01((_t - 0.2f) / 0.6f)));
            }

            if (_t > 0.2f && _t < 0.9f)
            {
                _acc += _dt * 40f;
                while (_acc >= 1f)
                {
                    _acc -= 1f;
                    float a = Random.value * 6.2832f;
                    float r = 0.3f + Random.value * 1.2f;
                    Emit(_stars, Mathf.Cos(a) * r, Mathf.Sin(a) * r, 0f, 0.5f + Random.value, 0.1f + 0.14f * Random.value, 0.8f, Hue(primary));
                }
            }
        }
    }
}
