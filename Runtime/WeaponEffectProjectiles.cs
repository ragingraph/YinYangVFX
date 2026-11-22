using UnityEngine;

namespace YinYangVFX
{
    // Flying projectiles. Origin = launch point; they fly to the right for about `qiTravel` units
    // (mirror for left). onImpact fires on arrival. Single colour per prefab.
    public partial class WeaponEffect
    {
        void AnimEnergyBolt()
        {
            Vector2 c = sweepCenter;
            float dist = qiTravel * 1.5f;
            float p = Mathf.Clamp01(_t / 0.55f);
            float hx = c.x + dist * p * p * (3f - 2f * p) * 0.35f + dist * p * 0.65f;
            float g = FadeFrom(0.7f);
            if (Once(1)) Burst(c.x, c.y, 8, 3f, 4, 1.2f);
            if (p < 1f)
            {
                int n = Seg(hx - 1.6f, c.y, hx, c.y, 2);
                Draw(Taper.Head, n, 0.4f * thickness, primary * 0.5f, 0.9f);
                n = Seg(hx - 1.0f, c.y, hx, c.y, 2);
                Draw(Taper.Head, n, 0.16f * thickness, primary, 1f);
                float r = 0.2f + 0.03f * Mathf.Sin(_t * 40f);
                n = Ring(hx, c.y, r, r);
                Draw(Taper.Flat, n, 0.07f * thickness, primary, 1f);
                n = Ring(hx, c.y, r * 1.9f, r * 1.9f);
                Draw(Taper.Flat, n, 0.02f * thickness, primary, 0.6f);
                Emit(_dots, hx, c.y, 0f, 0f, 0.9f, 0.07f, Hue(primary));
                Emit(_dots, hx, c.y, 0f, 0f, 1.5f, 0.07f, Tint(0.3f));
                _acc += _dt * 80f;
                while (_acc >= 1f)
                {
                    _acc -= 1f;
                    Emit(Random.value > 0.5f ? _stars : _sparks, hx - Random.value * 0.8f, c.y + (Random.value - 0.5f) * 0.3f, -1f - Random.value, (Random.value - 0.5f) * 1.5f, 0.07f + 0.07f * Random.value, 0.5f, Hue(primary));
                }
            }
            else
            {
                float f = Mathf.Clamp01((_t - 0.55f) / 0.45f);
                float xe = c.x + dist;
                if (Once(2))
                {
                    Fire();
                    Burst(xe, c.y, 30, 6f, 12, 2.4f);
                    Emit(_dots, xe, c.y, 0f, 0f, 3f, 0.25f, Hue(primary));
                }

                int n = Ring(xe, c.y, 0.9f * EaseOut(f * 2f), 0.9f * EaseOut(f * 2f));
                Draw(Taper.Flat, n, 0.06f * thickness, primary, (1f - f) * g);
            }
        }

        void AnimHomingShot()
        {
            Vector2 c = sweepCenter;
            float dist = qiTravel * 1.5f;
            Vector2 a = c, b = new Vector2(c.x + dist * 0.45f, c.y + 1.8f), e = new Vector2(c.x + dist, c.y - 0.2f);
            float p = Mathf.Clamp01(_t / 0.9f);
            float g = FadeFrom(1.0f);
            if (Once(1)) Burst(c.x, c.y, 8, 3f, 4, 1.2f);
            if (p < 1f)
            {
                const int m = 20;
                for (int i = 0; i < m; i++)
                {
                    float s = Mathf.Clamp01(p - 0.3f * (1f - i / (float)(m - 1)));
                    Vector2 q = Bez(a, b, e, s * s * (3f - 2f * s) * 0.4f + s * 0.6f);
                    _buf[i] = new Vector3(q.x, q.y, 0f);
                }

                Vector3 hp = _buf[m - 1];
                Beam(Taper.Head, m, 0.22f, 1f);
                int n = Ring(hp.x, hp.y, 0.16f, 0.16f);
                Draw(Taper.Flat, n, 0.06f * thickness, primary, 1f);
                Emit(_dots, hp.x, hp.y, 0f, 0f, 1.0f, 0.07f, Hue(primary));
                _acc += _dt * 70f;
                while (_acc >= 1f)
                {
                    _acc -= 1f;
                    Emit(Random.value > 0.5f ? _stars : _sparks, hp.x, hp.y, (Random.value - 0.5f) * 1.5f, (Random.value - 0.5f) * 1.5f, 0.07f + 0.07f * Random.value, 0.6f, Hue(primary));
                }
            }
            else
            {
                float f = Mathf.Clamp01((_t - 0.9f) / 0.4f);
                if (Once(2))
                {
                    Fire();
                    Burst(e.x, e.y, 26, 5f, 10, 2.2f);
                    Emit(_dots, e.x, e.y, 0f, 0f, 2.6f, 0.25f, Hue(primary));
                }

                int n = Ring(e.x, e.y, 0.8f * EaseOut(f * 2f), 0.8f * EaseOut(f * 2f));
                Draw(Taper.Flat, n, 0.06f * thickness, primary, (1f - f) * g);
            }
        }

        void AnimBoomerang()
        {
            Vector2 c = sweepCenter;
            float dist = qiTravel * 1.4f;
            float T = 1.5f;
            float ph = Mathf.Clamp01(_t / T) * Mathf.PI * 2f;
            float g = FadeFrom(1.5f);
            if (Once(1)) Burst(c.x, c.y, 6, 3f, 4, 1.0f);
            if (_t < T)
            {
                const int m = 24;
                for (int i = 0; i < m; i++)
                {
                    float q = Mathf.Max(0f, ph - 0.9f * (1f - i / (float)(m - 1)));
                    _buf[i] = new Vector3(c.x + dist * 0.5f * (1f - Mathf.Cos(q)), c.y + 0.9f * Mathf.Sin(q) * 0.7f, 0f);
                }

                Vector3 hp = _buf[m - 1];
                Beam(Taper.Head, m, 0.18f, 0.8f);
                float spin = _t * 900f;
                for (int k = 0; k < 2; k++)
                {
                    int n = ArcPts(hp.x, hp.y, 0.32f, 0.32f, spin + k * 180f, spin + k * 180f + 140f, 10);
                    Draw(Taper.Head, n, 0.12f * thickness, primary, 1f);
                }

                Emit(_dots, hp.x, hp.y, 0f, 0f, 0.9f, 0.07f, Hue(primary));
                if (ph > Mathf.PI && Once(2))
                {
                    Fire();
                    Burst(c.x + dist, c.y, 20, 5f, 8, 1.8f);
                }

                _acc += _dt * 60f;
                while (_acc >= 1f)
                {
                    _acc -= 1f;
                    Emit(Random.value > 0.5f ? _stars : _sparks, hp.x, hp.y, (Random.value - 0.5f) * 1.2f, (Random.value - 0.5f) * 1.2f, 0.07f + 0.06f * Random.value, 0.5f, Hue(primary));
                }
            }
            else
            {
                float f = Mathf.Clamp01((_t - T) / 0.3f);
                int n = Ring(c.x, c.y, 0.6f * f, 0.6f * f);
                Draw(Taper.Flat, n, 0.05f * thickness, primary, (1f - f) * g);
            }
        }

        void AnimBeamCannon()
        {
            Vector2 c = sweepCenter + new Vector2(0.3f, 0f);
            const float chargeT = 0.5f, fireT = 0.9f;
            float g = FadeFrom(1.6f);
            if (_t < chargeT)
            {
                float k = _t / chargeT;
                float r = 0.1f + 0.3f * k;
                int n = Ring(c.x, c.y, r, r);
                Draw(Taper.Flat, n, 0.08f * thickness, primary, 1f);
                Emit(_dots, c.x, c.y, 0f, 0f, 2.0f * r + 0.3f, 0.07f, Hue(primary));
                _acc += _dt * 120f;
                while (_acc >= 1f)
                {
                    _acc -= 1f;
                    float a = Random.value * 6.2832f;
                    Emit(Random.value > 0.5f ? _stars : _sparks, c.x + Mathf.Cos(a) * 1.0f, c.y + Mathf.Sin(a) * 1.0f, -Mathf.Cos(a) * 2.5f, -Mathf.Sin(a) * 2.5f, 0.07f + 0.07f * Random.value, 0.4f, Hue(primary));
                }

                return;
            }

            float it = _t - chargeT;
            if (Once(1))
            {
                Fire();
                Burst(c.x, c.y, 30, 6f, 10, 3f);
                Emit(_dots, c.x, c.y, 0f, 0f, 4f, 0.3f, Hue(primary));
                Burst(c.x + 8f, c.y, 40, 7f, 14, 3f);
            }

            float w = it < fireT ? EaseOut(it / 0.08f) : 1f - Mathf.Clamp01((it - fireT) / 0.6f);
            float fl = 0.85f + 0.15f * Rnd(Mathf.FloorToInt(_t * 50f), 3);
            for (int j = 0; j < 3; j++)
            {
                int n = Seg(c.x, c.y, c.x + 8f, c.y, 12);
                float ww = (1.1f - 0.4f * j) * w * fl;
                Draw(Taper.Flat, n, ww * thickness * 0.5f, primary * (j == 0 ? 0.5f : 1f), g);
            }

            int cn = Ring(c.x, c.y, 0.6f * w, 0.6f * w);
            Draw(Taper.Flat, cn, 0.06f * thickness, primary, g);
            Emit(_dots, c.x, c.y, 0f, 0f, 2.4f * w, 0.07f, Tint(0.3f));
            _acc += _dt * 120f;
            while (_acc >= 1f && w > 0.1f)
            {
                _acc -= 1f;
                float px = c.x + Random.value * 8f;
                Emit(Random.value > 0.5f ? _stars : _sparks, px, c.y + (Random.value - 0.5f) * 0.8f * w, (Random.value - 0.5f) * 1f, (Random.value - 0.5f) * 2f, 0.08f + 0.08f * Random.value, 0.6f, Hue(primary));
            }
        }

        void AnimLaser()
        {
            Vector2 c = sweepCenter;
            float L = 9f;
            float k = Mathf.Clamp01(_t / 0.05f);
            float g = FadeFrom(0.1f);
            if (Once(1))
            {
                Fire();
                Burst(c.x, c.y, 12, 4f, 6, 1.8f);
                Burst(c.x + L, c.y, 36, 7f, 12, 3f);
                Emit(_dots, c.x + L, c.y, 0f, 0f, 3f, 0.2f, Hue(primary));
            }

            float w = 1f - 0.5f * Mathf.Clamp01(_t / duration);
            for (int j = 0; j < 2; j++)
            {
                int n = Seg(c.x, c.y, c.x + L * k, c.y, 12);
                Draw(Taper.Flat, n, (j == 0 ? 0.22f : 0.07f) * w * thickness, j == 0 ? primary * 0.6f : primary, g);
            }

            float ex = c.x + L * k;
            int rn = Ring(ex, c.y, 0.3f * (1f - g * 0.5f), 0.3f * (1f - g * 0.5f));
            Draw(Taper.Flat, rn, 0.04f * thickness, primary, g);
            if (_t < 0.25f)
            {
                _acc += _dt * 150f;
                while (_acc >= 1f)
                {
                    _acc -= 1f;
                    Emit(Random.value > 0.5f ? _stars : _sparks, c.x + Random.value * L, c.y + (Random.value - 0.5f) * 0.2f, (Random.value - 0.5f) * 0.8f, (Random.value - 0.5f) * 1.5f, 0.06f + 0.06f * Random.value, 0.4f, Hue(primary));
                }
            }
        }

        void AnimShuriken()
        {
            Vector2 c = sweepCenter;
            float dist = qiTravel * 1.4f;
            float p = Mathf.Clamp01(_t / 0.6f);
            float hx = c.x + dist * p, hy = c.y + 0.06f * Mathf.Sin(_t * 30f);
            float g = FadeFrom(0.8f);
            if (Once(1)) Burst(c.x, c.y, 6, 3f, 4, 1.0f);
            if (p < 1f)
            {
                float spin = _t * 1400f;
                for (int k = 0; k < 4; k++)
                {
                    float a = (spin + k * 90f) * Mathf.Deg2Rad;
                    float b = a + 0.5f;
                    int n = Seg(hx + Mathf.Cos(a) * 0.38f, hy + Mathf.Sin(a) * 0.38f, hx + Mathf.Cos(b) * 0.1f, hy + Mathf.Sin(b) * 0.1f, 2);
                    Draw(Taper.Head, n, 0.1f * thickness, primary, 1f);
                }

                int rn = Ring(hx, hy, 0.12f, 0.12f);
                Draw(Taper.Flat, rn, 0.04f * thickness, primary, 1f);
                // afterimage streaks
                for (int j = 1; j <= 3; j++)
                {
                    int n = Ring(hx - j * 0.35f, hy, 0.3f - 0.06f * j, 0.3f - 0.06f * j);
                    Draw(Taper.Flat, n, 0.03f * thickness, primary, 0.5f - 0.13f * j);
                }

                Emit(_dots, hx, hy, 0f, 0f, 0.8f, 0.07f, Hue(primary));
                Emit(_sparks, hx, hy, -2f - Random.value, (Random.value - 0.5f) * 1.5f, 0.06f, 0.4f, Hue(primary));
            }
            else
            {
                float f = Mathf.Clamp01((_t - 0.6f) / 0.4f);
                float xe = c.x + dist;
                if (Once(2))
                {
                    Fire();
                    Burst(xe, c.y, 24, 5f, 8, 2.0f);
                    Emit(_dots, xe, c.y, 0f, 0f, 2.4f, 0.2f, Hue(primary));
                }

                int n = Ring(xe, c.y, 0.7f * EaseOut(f * 2f), 0.7f * EaseOut(f * 2f));
                Draw(Taper.Flat, n, 0.05f * thickness, primary, (1f - f) * g);
            }
        }
    }
}
