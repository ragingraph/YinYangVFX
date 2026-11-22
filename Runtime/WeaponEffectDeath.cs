using UnityEngine;

namespace YinYangVFX
{
    // Death / dissolve effects. Origin = the enemy's body centre (hide or fade your sprite when onImpact fires).
    // Single colour per prefab.
    public partial class WeaponEffect
    {
        void AnimDeathMonster()
        {
            Vector2 c = sweepCenter;
            float e = EaseOut(_t / 0.12f), g = FadeFrom(0.4f);
            if (Once(1))
            {
                Fire();
                Emit(_dots, c.x, c.y, 0f, 0f, 3.2f, 0.25f, Hue(primary));
                Burst(c.x, c.y, 40, 7f, 14, 2.6f);
                for (int j = 0; j < 14; j++)
                {
                    Emit(_sparks, c.x + (Random.value - 0.5f) * 0.4f, c.y, (Random.value - 0.5f) * 4f, 1.5f + 2.5f * Random.value, 0.08f + 0.06f * Random.value, 0.9f, Hue(primary));
                }

                for (int j = 0; j < 6; j++)
                {
                    Emit(_dots, c.x + (Random.value - 0.5f) * 0.4f, c.y + (Random.value - 0.5f) * 0.5f, (Random.value - 0.5f) * 1.2f, 0.4f, 0.9f, 0.7f, Tint(0.3f));
                }
            }

            for (int k = 0; k < 8; k++)
            {
                float a = (k * 45f + 10f) * Mathf.Deg2Rad;
                float L = (k % 2 == 0 ? 1.3f : 0.8f) * e;
                int n = Seg(c.x + Mathf.Cos(a) * 0.15f, c.y + Mathf.Sin(a) * 0.15f, c.x + Mathf.Cos(a) * L, c.y + Mathf.Sin(a) * L, 2);
                Draw(Taper.Head, n, 0.09f * thickness, primary, g);
            }

            float re = EaseOut(_t / 0.4f);
            int rn = Ring(c.x, c.y, 1.4f * re, 1.4f * re);
            Draw(Taper.Flat, rn, 0.06f * thickness, primary, (1f - re * 0.7f) * g);
        }

        void AnimDeathAsh()
        {
            Vector2 c = sweepCenter;
            float g = FadeFrom(1.3f);
            if (Once(1)) Fire();

            if (_t < 0.9f)
            {
                Color ash = Hue(primary) * 0.55f;
                ash.a = 1f;
                for (int k = 0; k < 3; k++)
                {
                    float a = Random.value * 6.2832f, r = Random.value;
                    Emit(_dots, c.x + Mathf.Cos(a) * BodyRx * r, c.y + Mathf.Sin(a) * BodyRy * r, 0.3f + 0.5f * Random.value, 0.3f + 0.7f * Random.value, 0.12f + 0.18f * Random.value, 1.3f, ash);
                }

                _acc += _dt * 70f;
                while (_acc >= 1f)
                {
                    _acc -= 1f;
                    float a = Random.value * 6.2832f, r = Random.value;
                    Emit(_sparks, c.x + Mathf.Cos(a) * BodyRx * r, c.y + Mathf.Sin(a) * BodyRy * r, 0.4f + 0.6f * Random.value, 0.3f + 0.8f * Random.value, 0.04f, 1.2f, Hue(primary));
                }

                Emit(_dots, c.x, c.y, 0f, 0f, 1.6f * (1f - _t), 0.07f, Tint(0.18f));
            }

            for (int w = 0; w < 3; w++)
            {
                int n = WaveDir(c.x + (w - 1) * 0.25f, c.y - 0.2f, 70f, 1.3f * Mathf.Clamp01(_t * 2f), 14, 0.1f, 7f, _t * 3f + w);
                Draw(Taper.Head, n, 0.09f * thickness, primary * 0.5f, g * 0.6f);
            }

            float re = EaseOut(_t / 0.5f);
            int rn = Ring(c.x, groundY, 0.8f * re, 0.14f * re);
            Draw(Taper.Flat, rn, 0.025f * thickness, primary, (1f - re * 0.6f) * g * 0.8f);
        }

        void AnimDeathLight()
        {
            Vector2 c = sweepCenter;
            float g = FadeFrom(1.1f);
            if (Once(1))
            {
                Fire();
                Emit(_dots, c.x, c.y, 0f, 0f, 2.6f, 0.4f, Hue(primary));
                Burst(c.x, c.y, 20, 4f, 20, 2.2f);
            }

            if (_t < 1.0f)
            {
                _acc += _dt * 80f;
                while (_acc >= 1f)
                {
                    _acc -= 1f;
                    float a = Random.value * 6.2832f, r = Random.value;
                    Emit(Random.value > 0.4f ? _stars : _sparks, c.x + Mathf.Cos(a) * BodyRx * r, c.y + Mathf.Sin(a) * BodyRy * r, (Random.value - 0.5f) * 0.4f, 0.8f + 1.8f * Random.value,
                         0.09f + 0.1f * Random.value, 1.0f, Hue(primary));
                }
            }

            for (int k = 0; k < 8; k++)
            {
                float x = c.x + (Rnd(k) - 0.5f) * 0.7f;
                float f = Mathf.Repeat(_t * 0.9f + Rnd(k, 1), 1f);
                float y = c.y - 0.5f + f * 2.0f;
                int n = Seg(x, y - 0.6f, x, y, 2);
                Draw(Taper.Head, n, 0.04f * thickness, primary, g * Mathf.Sin(f * Mathf.PI) * 0.9f);
            }

            float pe = EaseOut(_t / 0.4f);
            int pn = Seg(c.x, c.y + 2.2f * pe, c.x, groundY, 2);
            Draw(Taper.Head, pn, 0.5f * (1f - 0.5f * pe) * thickness, primary * 0.5f, g * (1f - 0.5f * pe));
            int rn = Ring(c.x, c.y, 1.5f * pe, 1.5f * pe);
            Draw(Taper.Flat, rn, 0.05f * thickness, primary, (1f - pe) * g);
        }

        void AnimDeathSoul()
        {
            Vector2 c = sweepCenter;
            float g = FadeFrom(1.6f);
            float p = Mathf.Clamp01(_t / 1.5f);
            float ep = p * p * (3f - 2f * p);
            if (Once(1))
            {
                Fire();
                Burst(c.x, c.y, 12, 3f, 8, 1.6f);
            }

            const int m = 28;
            for (int i = 0; i < m; i++)
            {
                float q = Mathf.Clamp01(ep - (1f - i / (float)(m - 1)) * 0.35f);
                _buf[i] = new Vector3(c.x + 0.25f * Mathf.Sin(q * 9f + _t * 2f) * q, c.y + q * 2.6f, 0f);
            }

            Beam(Taper.Head, m, 0.1f, g);
            Vector3 hp = _buf[m - 1];
            float r = 0.16f + 0.03f * Mathf.Sin(_t * 9f);
            int n = Ring(hp.x, hp.y, r, r);
            Draw(Taper.Flat, n, 0.06f * thickness, primary, g);
            n = Ring(hp.x, hp.y, r * 2.1f, r * 2.1f);
            Draw(Taper.Flat, n, 0.015f * thickness, primary, g * 0.6f);
            Emit(_dots, hp.x, hp.y, 0f, 0f, 0.9f, 0.07f, Tint(0.3f));
            if (_t < 1.5f)
            {
                _acc += _dt * 40f;
                while (_acc >= 1f)
                {
                    _acc -= 1f;
                    Emit(Random.value > 0.5f ? _stars : _sparks, hp.x + (Random.value - 0.5f) * 0.3f, hp.y - Random.value * 0.4f, (Random.value - 0.5f) * 0.5f, -0.2f, 0.08f + 0.08f * Random.value, 0.9f, Hue(primary));
                }
            }

            if (_t >= 1.5f && Once(2)) Burst(hp.x, hp.y, 16, 4f, 12, 2f);
        }

        void AnimDeathBoss()
        {
            Vector2 c = sweepCenter;
            const float bigT = 1.2f;
            float g = FadeFrom(2.0f);

            if (_t < bigT)
            {
                float k = _t / bigT;
                int tick = Mathf.FloorToInt(_t / 0.15f);
                if (Once(1 << (tick + 3)))
                {
                    float a = Random.value * 6.2832f, r = Random.value;
                    Burst(c.x + Mathf.Cos(a) * BodyRx * r, c.y + Mathf.Sin(a) * BodyRy * r, 10, 4f, 4, 1.0f + k);
                }

                for (int j = 0; j < 8; j++)
                {
                    float a = (j * 45f + Rnd(j) * 15f) * Mathf.Deg2Rad;
                    float L = (0.5f + 1.5f * Rnd(j, 1)) * k;
                    int n = Seg(c.x, c.y, c.x + Mathf.Cos(a) * L, c.y + Mathf.Sin(a) * L, 2);
                    Draw(Taper.Tail, n, 0.05f * thickness, primary, 0.6f + 0.4f * k);
                }

                int rn = Ring(c.x, c.y, BodyRx * (1.2f + 0.3f * Mathf.Sin(_t * 40f)), BodyRy * 1.2f);
                Draw(Taper.Flat, rn, 0.05f * thickness, primary, 0.5f + 0.5f * k);
                return;
            }

            float it = _t - bigT;
            if (Once(1))
            {
                Fire();
                Emit(_dots, c.x, c.y, 0f, 0f, 8f, 0.4f, Hue(primary));
                Emit(_dots, c.x, c.y, 0f, 0f, 5f, 0.8f, Tint(0.4f));
                Burst(c.x, c.y, 110, 10f, 40, 5f);
                for (int j = 0; j < 30; j++)
                {
                    float a = Random.value * 6.2832f;
                    Emit(_dots, c.x, c.y, Mathf.Cos(a) * 2f, Mathf.Sin(a) * 2f, 0.8f + 0.7f * Random.value, 1.2f, Tint(0.3f));
                }
            }

            for (int j = 0; j < 4; j++)
            {
                float d = Mathf.Clamp01((it - 0.07f * j) / 0.9f);
                float r = 4.0f * EaseOut(d) * (1f - 0.2f * j);
                int n = Ring(c.x, c.y, r, r);
                Draw(Taper.Flat, n, (0.14f - 0.03f * j) * thickness, primary, (1f - d * 0.85f) * g * (1f - 0.15f * j));
            }

            float e = EaseOut(it / 0.3f);
            for (int k = 0; k < 20; k++)
            {
                float a = (k * 18f + Rnd(k) * 8f) * Mathf.Deg2Rad;
                float r0 = 0.4f + 1.2f * e * (0.5f + 0.5f * Rnd(k, 1));
                int n = Seg(c.x + Mathf.Cos(a) * (r0 - 1.6f * e), c.y + Mathf.Sin(a) * (r0 - 1.6f * e), c.x + Mathf.Cos(a) * r0 * (1f + 2f * e), c.y + Mathf.Sin(a) * r0 * (1f + 2f * e), 2);
                Draw(Taper.Head, n, 0.08f * thickness, primary, g * (1f - Mathf.Clamp01(it / 1.0f)));
            }

            int pn = Seg(c.x, c.y + 4.5f * e, c.x, groundY, 2);
            Draw(Taper.Head, pn, 1.1f * (1f - 0.6f * Mathf.Clamp01(it)) * thickness, primary * 0.5f, g * (1f - Mathf.Clamp01(it / 0.8f)));
            int gr = Ring(c.x, groundY, 4.4f * EaseOut(it / 0.8f), 0.7f * EaseOut(it / 0.8f));
            Draw(Taper.Flat, gr, 0.05f * thickness, primary, (1f - Mathf.Clamp01(it / 0.9f)) * g);

            if (it > 0.2f && _t < duration - 0.5f)
            {
                _acc += _dt * 50f;
                while (_acc >= 1f)
                {
                    _acc -= 1f;
                    Emit(Random.value > 0.5f ? _stars : _sparks, c.x + (Random.value - 0.5f) * 2.5f, c.y + (Random.value - 0.5f) * 1.5f, (Random.value - 0.5f) * 0.6f, 0.5f + 1.2f * Random.value, 0.09f + 0.1f * Random.value, 1.2f, Hue(primary));
                }
            }
        }
    }
}
