using UnityEngine;

namespace YinYangVFX
{
    // Boss / scene effects. Origin = boss feet / scene centre (ground at groundY). Single colour per prefab.
    public partial class WeaponEffect
    {
        // Zigzag crack along the ground, from x0 heading in direction dir (+1/-1), length L.
        void GroundCrack(float x0, float dir, float L, int seed, float w, float a)
        {
            const int m = 12;
            for (int i = 0; i < m; i++)
            {
                float s = i / (float)(m - 1);
                _buf[i] = new Vector3(x0 + dir * L * s, groundY + (i == 0 ? 0f : (Rnd(i, seed) - 0.5f) * 0.18f), 0f);
            }

            Draw(Taper.Head, m, w * thickness, primary, a);
        }

        void AnimBossEntrance()
        {
            Vector2 c = sweepCenter;
            float x = c.x;
            float g = FadeFrom(1.7f);
            float e = EaseOut(_t / 0.5f);
            if (_t >= 0.3f && Once(1))
            {
                Fire();
                Burst(x, groundY, 70, 8f, 20, 3.4f);
                Emit(_dots, x, groundY + 0.5f, 0f, 0f, 5f, 0.4f, Hue(primary));
                for (int j = 0; j < 20; j++)
                {
                    Emit(_dots, x + (Random.value - 0.5f) * 2f, groundY + 0.1f, (Random.value - 0.5f) * 3f, 0.3f + 0.7f * Random.value, 0.8f + 0.7f * Random.value, 1.3f, Tint(0.25f));
                }
            }

            for (int k = 0; k < 4; k++)
            {
                GroundCrack(x, k % 2 == 0 ? 1f : -1f, (1.5f + 1.2f * (k / 2)) * e * 2f, k + 1, 0.06f, g);
            }

            if (_t >= 0.3f)
            {
                float it = _t - 0.3f;
                float pf = 1f - Mathf.Clamp01(it / 1.0f);
                int n = Seg(x, groundY, x, groundY + 4.5f * EaseOut(it / 0.25f), 2);
                Draw(Taper.Head, n, 1.0f * thickness * pf, primary * 0.5f, pf * g);
                n = Seg(x, groundY, x, groundY + 4.5f * EaseOut(it / 0.25f), 2);
                Draw(Taper.Head, n, 0.3f * thickness * pf, primary, pf * g);
                for (int j = 0; j < 3; j++)
                {
                    float d = Mathf.Clamp01((it - 0.12f * j) / 0.9f);
                    n = Ring(x, groundY, 4.0f * EaseOut(d), 0.7f * EaseOut(d));
                    Draw(Taper.Flat, n, (0.12f - 0.03f * j) * thickness, primary, (1f - d) * g);
                }
            }
            else
            {
                // gathering before landing: ground ring shrinking
                float k = _t / 0.3f;
                int n = Ring(x, groundY, 2.0f * (1f - k), 0.35f * (1f - k));
                Draw(Taper.Flat, n, 0.06f * thickness, primary, k);
            }

            if (_t > 0.3f && _t < 1.5f)
            {
                _acc += _dt * 60f;
                while (_acc >= 1f)
                {
                    _acc -= 1f;
                    Emit(Random.value > 0.5f ? _stars : _sparks, x + (Random.value - 0.5f) * 2.5f, groundY + Random.value * 0.2f, (Random.value - 0.5f) * 1.5f, 1.5f + 3f * Random.value, 0.09f + 0.1f * Random.value, 1.0f, Hue(primary));
                }
            }
        }

        // Looping red ground warning circle (attack area). Use loop=true; call Stop() to end.
        void AnimWarningCircle()
        {
            Vector2 c = sweepCenter;
            float R = 1.8f;
            float cyc = Mathf.Repeat(_t, 1.0f);
            float fl = 0.55f + 0.45f * Mathf.Sin(_t * 12f);
            int n = Ring(c.x, groundY, R, R * 0.22f);
            Draw(Taper.Flat, n, 0.09f * thickness, primary, fl);
            n = Ring(c.x, groundY, R * cyc, R * 0.22f * cyc);
            Draw(Taper.Flat, n, 0.05f * thickness, primary, 1f - cyc * 0.5f);
            for (int k = 0; k < 8; k++)
            {
                float a = (k * 45f + _t * 20f) * Mathf.Deg2Rad;
                n = Seg(c.x + Mathf.Cos(a) * R * 0.85f, groundY + Mathf.Sin(a) * R * 0.19f, c.x + Mathf.Cos(a) * R, groundY + Mathf.Sin(a) * R * 0.22f, 2);
                Draw(Taper.Flat, n, 0.06f * thickness, primary, fl);
            }

            Emit(_dots, c.x, groundY, 0f, 0f, R * 1.8f, 0.07f, Tint(0.12f + 0.1f * fl));
            if (Live())
            {
                _acc += _dt * 12f;
                while (_acc >= 1f)
                {
                    _acc -= 1f;
                    float a = Random.value * 6.2832f;
                    Emit(_sparks, c.x + Mathf.Cos(a) * R, groundY + Mathf.Sin(a) * R * 0.22f, 0f, 1f + Random.value, 0.07f, 0.6f, Hue(primary));
                }
            }
        }

        // Looping dash warning band along the ground to the right (mirror for left).
        void AnimDashWarning()
        {
            Vector2 c = sweepCenter;
            float L = 6.5f, hw = 0.35f;
            float fl = 0.55f + 0.45f * Mathf.Sin(_t * 14f);
            int n = Seg(c.x, groundY + hw, c.x + L, groundY + hw, 2);
            Draw(Taper.Flat, n, 0.05f * thickness, primary, fl);
            n = Seg(c.x, groundY - hw * 0.2f, c.x + L, groundY - hw * 0.2f, 2);
            Draw(Taper.Flat, n, 0.05f * thickness, primary, fl);
            for (int i = 0; i < 5; i++)
            {
                float f = Mathf.Repeat(_t * 1.4f + i / 5f, 1f);
                float x = c.x + f * L, w = 0.22f;
                float al = Mathf.Sin(f * Mathf.PI);
                n = Seg(x - w, groundY + hw, x, groundY + hw * 0.4f, 2);
                Draw(Taper.Flat, n, 0.07f * thickness, primary, al);
                n = Seg(x - w, groundY - hw * 0.2f, x, groundY + hw * 0.4f, 2);
                Draw(Taper.Flat, n, 0.07f * thickness, primary, al);
            }

            Emit(_dots, c.x + L * 0.5f, groundY + hw * 0.4f, 0f, 0f, 0.1f, 0.07f, Tint(0f));
        }

        void AnimBossPhaseChange()
        {
            Vector2 c = sweepCenter;
            const float chargeT = 0.7f;
            float g = FadeFrom(1.6f);
            if (_t < chargeT)
            {
                float k = _t / chargeT;
                float sh = 0.04f * Mathf.Sin(_t * 90f) * k;
                for (int j = 0; j < 3; j++)
                {
                    float f = Mathf.Repeat(k * 2f + j / 3f, 1f);
                    int n = Ring(c.x + sh, c.y, 1.8f * (1f - f), 1.8f * (1f - f));
                    Draw(Taper.Flat, n, 0.05f * thickness, primary, f);
                }

                Emit(_dots, c.x, c.y, 0f, 0f, 1.5f + 1.5f * k, 0.07f, Tint(0.2f * k));
                _acc += _dt * 100f;
                while (_acc >= 1f)
                {
                    _acc -= 1f;
                    float a = Random.value * 6.2832f;
                    Emit(Random.value > 0.5f ? _stars : _sparks, c.x + Mathf.Cos(a) * 1.6f, c.y + Mathf.Sin(a) * 1.6f, -Mathf.Cos(a) * 3f, -Mathf.Sin(a) * 3f, 0.08f + 0.08f * Random.value, 0.5f, Hue(primary));
                }

                return;
            }

            float it = _t - chargeT;
            if (Once(1))
            {
                Fire();
                Burst(c.x, c.y, 90, 10f, 30, 4.5f);
                Emit(_dots, c.x, c.y, 0f, 0f, 7f, 0.4f, Hue(primary));
            }

            for (int j = 0; j < 3; j++)
            {
                float d = Mathf.Clamp01((it - 0.1f * j) / 0.9f);
                int n = Ring(c.x, c.y, 4.5f * EaseOut(d), 4.5f * EaseOut(d));
                Draw(Taper.Flat, n, 0.14f * (1f - d) * thickness, primary, (1f - d) * g);
            }

            float e = EaseOut(it / 0.25f), pf = 1f - Mathf.Clamp01(it / 0.9f);
            int pn = Seg(c.x, groundY, c.x, groundY + 5.0f * e, 2);
            Draw(Taper.Head, pn, 0.8f * thickness * pf, primary * 0.5f, pf * g);
            for (int k = 0; k < 12; k++)
            {
                float a = (k * 30f) * Mathf.Deg2Rad;
                int n = Seg(c.x + Mathf.Cos(a) * 0.6f, c.y + Mathf.Sin(a) * 0.6f, c.x + Mathf.Cos(a) * (0.6f + (k % 2 == 0 ? 3.5f : 2.2f) * e), c.y + Mathf.Sin(a) * (0.6f + (k % 2 == 0 ? 3.5f : 2.2f) * e), 2);
                Draw(Taper.Head, n, 0.07f * thickness, primary, pf * g);
            }

            int gr = Ring(c.x, groundY, 4.4f * EaseOut(it / 0.8f), 0.8f * EaseOut(it / 0.8f));
            Draw(Taper.Flat, gr, 0.05f * thickness, primary, pf * g);
        }

        // Scene transition cue: expanding rings and radial rays; onImpact fires at the screen-covering moment.
        void AnimSceneTransition()
        {
            Vector2 c = sweepCenter;
            float g = FadeFrom(0.9f);
            float e = EaseOut(_t / 0.5f);
            if (_t >= 0.3f && Once(1))
            {
                Fire();
                Emit(_dots, c.x, c.y, 0f, 0f, 16f, 0.25f, Hue(primary));
            }

            for (int j = 0; j < 3; j++)
            {
                float d = Mathf.Clamp01((_t - 0.08f * j) / 0.8f);
                float r = 8.0f * EaseOut(d);
                int n = Ring(c.x, c.y, r, r);
                Draw(Taper.Flat, n, (0.5f - 0.14f * j) * (1f - d * 0.5f) * thickness, primary * (j == 0 ? 0.6f : 1f), (1f - d) * g + 0.0f);
            }

            for (int k = 0; k < 24; k++)
            {
                float a = (k * 15f + Rnd(k) * 6f) * Mathf.Deg2Rad;
                float r0 = 8.0f * e * 0.4f, r1 = 8.0f * e * (0.7f + 0.3f * Rnd(k, 2));
                int n = Seg(c.x + Mathf.Cos(a) * r0, c.y + Mathf.Sin(a) * r0, c.x + Mathf.Cos(a) * r1, c.y + Mathf.Sin(a) * r1, 2);
                Draw(Taper.Head, n, 0.1f * thickness, primary, (1f - Mathf.Clamp01(_t / 0.9f)) * g);
            }

            if (_t < 0.6f)
            {
                _acc += _dt * 150f;
                while (_acc >= 1f)
                {
                    _acc -= 1f;
                    float a = Random.value * 6.2832f, r = 8.0f * e * Random.value;
                    Emit(Random.value > 0.5f ? _stars : _sparks, c.x + Mathf.Cos(a) * r, c.y + Mathf.Sin(a) * r, Mathf.Cos(a) * 2f, Mathf.Sin(a) * 2f, 0.09f + 0.1f * Random.value, 0.7f, Hue(primary));
                }
            }
        }

        void AnimEarthShake()
        {
            Vector2 c = sweepCenter;
            float x = c.x;
            float g = FadeFrom(1.2f);
            float e = EaseOut(_t / 0.3f);
            if (Once(1))
            {
                Fire();
                Burst(x, groundY, 50, 6f, 8, 2.8f);
                for (int j = 0; j < 24; j++)
                {
                    Emit(_dots, x + (Random.value - 0.5f) * 5f, groundY + 0.1f, (Random.value - 0.5f) * 2f, 0.4f + 1.0f * Random.value, 0.7f + 0.7f * Random.value, 1.2f, Tint(0.25f));
                }
            }

            for (int k = 0; k < 6; k++)
            {
                GroundCrack(x + (k % 3 - 1) * 0.3f, k % 2 == 0 ? 1f : -1f, (1.2f + 0.8f * (k / 2)) * e * 2.4f, k + 3, 0.07f, g);
            }

            for (int j = 0; j < 2; j++)
            {
                float d = Mathf.Clamp01((_t - 0.1f * j) / 0.8f);
                int n = Ring(x, groundY, 5.0f * EaseOut(d), 0.7f * EaseOut(d));
                Draw(Taper.Flat, n, 0.09f * (1f - d) * thickness, primary, (1f - d) * g);
            }

            if (_t < 0.8f)
            {
                _acc += _dt * 80f;
                while (_acc >= 1f)
                {
                    _acc -= 1f;
                    Emit(Random.value > 0.6f ? _stars : _sparks, x + (Random.value - 0.5f) * 5f, groundY + 0.05f, (Random.value - 0.5f) * 1.5f, 1.5f + 3f * Random.value, 0.08f + 0.09f * Random.value, 0.9f, Hue(primary));
                }
            }
        }
    }
}
