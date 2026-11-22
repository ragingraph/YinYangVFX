using UnityEngine;

namespace YinYangVFX
{
    // Rising-from-the-ground effects. Origin = ground contact point (groundY). Single colour per prefab.
    // Forward waves go to the right (mirror for left).
    public partial class WeaponEffect
    {
        // One spike / blade shape rising from the ground: thick base, sharp tip.
        void RiseSpike(float x, float h, float w, float a)
        {
            if (h <= 0.01f) return;
            int n = Seg(x, groundY + h, x, groundY, 2);
            Draw(Taper.Head, n, w * 1.8f * thickness, primary * 0.5f, a);
            n = Seg(x, groundY + h, x, groundY, 2);
            Draw(Taper.Head, n, w * 0.7f * thickness, primary, a);
        }

        float RiseK(float t, float rise, float hold, float fall)
        {
            if (t < 0f) return 0f;
            if (t < rise) return EaseOut(t / rise);
            if (t < rise + hold) return 1f;
            return 1f - Mathf.Clamp01((t - rise - hold) / fall);
        }

        void AnimGroundSpikeWave()
        {
            Vector2 c = sweepCenter;
            const int N = 10;
            if (Once(1)) Fire();
            for (int i = 0; i < N; i++)
            {
                float ts = 0.06f * i;
                float x = c.x + 0.35f + i * 0.45f;
                float k = RiseK(_t - ts, 0.1f, 0.5f, 0.4f);
                float h = (0.8f + 0.5f * Rnd(i, 1)) * (0.6f + 0.4f * i / (float)(N - 1)) * k;
                RiseSpike(x, h, 0.16f, 1f);
                if (_t >= ts && Once(1 << (i + 2)))
                {
                    Burst(x, groundY + 0.05f, 8, 3.5f, 3, 1.3f);
                    for (int j = 0; j < 3; j++)
                    {
                        Emit(_dots, x + (Random.value - 0.5f) * 0.3f, groundY + 0.1f, (Random.value - 0.5f) * 1.2f, 0.4f + 0.6f * Random.value, 0.5f + 0.4f * Random.value, 0.8f, Tint(0.3f));
                    }
                }

                if (k > 0f && k < 1f)
                {
                    int rn = Ring(x, groundY, 0.35f * EaseOut((_t - ts) / 0.3f), 0.07f);
                    Draw(Taper.Flat, rn, 0.03f * thickness, primary, 0.8f);
                }
            }
        }

        void AnimGroundBurst()
        {
            Vector2 c = sweepCenter;
            float x = c.x;
            float g = FadeFrom(0.9f);
            float e = EaseOut(_t / 0.18f);
            if (Once(1))
            {
                Fire();
                Burst(x, groundY, 50, 7f, 14, 3.0f);
                Emit(_dots, x, groundY + 0.4f, 0f, 0f, 3.2f, 0.3f, Hue(primary));
                for (int j = 0; j < 16; j++)
                {
                    Emit(_dots, x + (Random.value - 0.5f) * 0.8f, groundY + 0.1f, (Random.value - 0.5f) * 2.5f, 1f + 2.5f * Random.value, 0.5f + 0.5f * Random.value, 1.0f, Tint(0.3f));
                }
            }

            float pf = 1f - Mathf.Clamp01((_t - 0.1f) / 0.6f);
            int n = Seg(x, groundY + 3.0f * e, x, groundY, 2);
            Draw(Taper.Head, n, 0.9f * thickness * pf, primary * 0.5f, pf);
            n = Seg(x, groundY + 3.0f * e, x, groundY, 2);
            Draw(Taper.Head, n, 0.3f * thickness * pf, primary, pf);
            for (int j = 0; j < 2; j++)
            {
                float d = Mathf.Clamp01((_t - 0.07f * j) / 0.6f);
                n = Ring(x, groundY, 2.2f * EaseOut(d), 0.4f * EaseOut(d));
                Draw(Taper.Flat, n, 0.08f * (1f - d) * thickness, primary, (1f - d) * g);
            }

            for (int k = 0; k < 4; k++)
            {
                GroundCrack(x, k % 2 == 0 ? 1f : -1f, (0.9f + 0.5f * (k / 2)) * e * 2f, k + 1, 0.05f, g);
            }

            if (_t < 0.5f)
            {
                _acc += _dt * 80f;
                while (_acc >= 1f)
                {
                    _acc -= 1f;
                    Emit(Random.value > 0.5f ? _stars : _sparks, x + (Random.value - 0.5f) * 0.7f, groundY + 0.1f, (Random.value - 0.5f) * 2f, 2f + 3f * Random.value, 0.08f + 0.09f * Random.value, 0.9f, Hue(primary));
                }
            }
        }

        void AnimRisingBlades()
        {
            Vector2 c = sweepCenter;
            const int N = 9;
            if (Once(1)) Fire();
            int order = 0;
            for (int i = 0; i < N; i++)
            {
                int j = (i * 4) % N; // scatter the order
                float x = c.x + (j - (N - 1) * 0.5f) * 0.3f;
                float ts = 0.07f * i;
                float k = RiseK(_t - ts, 0.12f, 0.9f, 0.45f);
                float h = (1.0f + 0.5f * Rnd(j, 2)) * k;
                RiseSpike(x, h, 0.1f, 1f);
                if (h > 0.2f)
                {
                    float tipY = groundY + h;
                    int n = Seg(x - 0.14f, tipY - 0.25f, x + 0.14f, tipY - 0.25f, 2);
                    Draw(Taper.Lens, n, 0.04f * thickness, primary, k);
                }

                if (_t >= ts && Once(1 << (i + 2)))
                {
                    Burst(x, groundY + 0.05f, 8, 3f, 3, 1.2f);
                    order++;
                }
            }

            float re = EaseOut(_t / 0.4f);
            int rn = Ring(c.x, groundY, 1.5f * re, 0.28f * re);
            Draw(Taper.Flat, rn, 0.05f * thickness, primary, (1f - Mathf.Clamp01(_t / 1.2f)));
            if (_t < 1.0f)
            {
                _acc += _dt * 40f;
                while (_acc >= 1f)
                {
                    _acc -= 1f;
                    Emit(Random.value > 0.5f ? _stars : _sparks, c.x + (Random.value - 0.5f) * 2.4f, groundY + 0.05f, 0f, 1f + 2f * Random.value, 0.07f + 0.07f * Random.value, 0.9f, Hue(primary));
                }
            }
        }

        void AnimTendrilRise()
        {
            Vector2 c = sweepCenter;
            const int N = 7;
            if (Once(1)) Fire();
            for (int i = 0; i < N; i++)
            {
                float x = c.x + (i - (N - 1) * 0.5f) * 0.3f;
                float ts = 0.05f * Mathf.Abs(i - (N - 1) * 0.5f);
                float k = RiseK(_t - ts, 0.45f, 0.5f, 0.5f);
                float len = (1.2f + 0.9f * Rnd(i, 3)) * k;
                if (len < 0.05f) continue;
                int n = WaveDir(x, groundY, 90f, len, 20, 0.14f + 0.05f * Rnd(i, 4), 2.4f + Rnd(i, 5), _t * 5f + i);
                Vector3 tp = _buf[n - 1];
                Draw(Taper.Head, n, 0.14f * thickness, primary * 0.5f, 1f);
                n = WaveDir(x, groundY, 90f, len, 20, 0.14f + 0.05f * Rnd(i, 4), 2.4f + Rnd(i, 5), _t * 5f + i);
                Draw(Taper.Head, n, 0.06f * thickness, primary, 1f);
                if (k > 0.1f)
                {
                    Emit(_dots, tp.x, tp.y, 0f, 0f, 0.4f, 0.07f, Hue(primary));
                }
            }

            int rn = Ring(c.x, groundY, 1.3f * EaseOut(_t / 0.4f), 0.25f * EaseOut(_t / 0.4f));
            Draw(Taper.Flat, rn, 0.04f * thickness, primary, 1f - Mathf.Clamp01(_t / 1.2f));
            if (_t < 1.0f)
            {
                _acc += _dt * 50f;
                while (_acc >= 1f)
                {
                    _acc -= 1f;
                    Emit(Random.value > 0.5f ? _stars : _sparks, c.x + (Random.value - 0.5f) * 2.0f, groundY + Random.value * 1.5f, (Random.value - 0.5f) * 0.4f, 0.4f + 0.8f * Random.value, 0.07f + 0.06f * Random.value, 0.9f, Hue(primary));
                }
            }
        }

        void AnimPillarRise()
        {
            Vector2 c = sweepCenter;
            float x = c.x;
            float g = FadeFrom(1.4f);
            float e = EaseOut(_t / 0.35f);
            float H = 4.0f;
            if (Once(1))
            {
                Fire();
                Burst(x, groundY, 30, 6f, 12, 2.4f);
                Emit(_dots, x, groundY + 0.3f, 0f, 0f, 3.0f, 0.3f, Hue(primary));
            }

            float pf = 1f - Mathf.Clamp01((_t - 0.8f) / 0.8f);
            int n = Seg(x, groundY + H * e, x, groundY, 2);
            Draw(Taper.Head, n, 1.1f * thickness * pf, primary * 0.4f, pf * g);
            n = Seg(x, groundY + H * e, x, groundY, 2);
            Draw(Taper.Head, n, 0.4f * thickness * pf, primary * 0.8f, pf * g);
            n = Seg(x, groundY + H * e, x, groundY, 2);
            Draw(Taper.Head, n, 0.14f * thickness * pf, primary, pf * g);
            for (int s = 0; s < 2; s++)
            {
                n = WaveDir(x, groundY, 90f, H * e, 30, 0.38f, 3.0f, _t * 8f + s * Mathf.PI);
                Draw(Taper.Head, n, 0.05f * thickness, primary, pf * g);
            }

            for (int j = 0; j < 3; j++)
            {
                float d = Mathf.Clamp01((_t - 0.1f * j) / 0.8f);
                n = Ring(x, groundY, 1.6f * EaseOut(d), 0.3f * EaseOut(d));
                Draw(Taper.Flat, n, 0.07f * (1f - d) * thickness, primary, (1f - d) * g);
            }

            if (_t < 1.3f)
            {
                _acc += _dt * 70f;
                while (_acc >= 1f)
                {
                    _acc -= 1f;
                    Emit(Random.value > 0.5f ? _stars : _sparks, x + (Random.value - 0.5f) * 0.9f, groundY + Random.value * 0.3f, (Random.value - 0.5f) * 0.4f, 2f + 3f * Random.value, 0.08f + 0.09f * Random.value, 1.0f, Hue(primary));
                }
            }
        }

        // A wall of light bars rising outward from the centre, holding, then sinking.
        void AnimRiseWall()
        {
            Vector2 c = sweepCenter;
            const int N = 13;
            if (Once(1)) Fire();
            for (int i = 0; i < N; i++)
            {
                float off = i - (N - 1) * 0.5f;
                float x = c.x + off * 0.34f;
                float ts = 0.04f * Mathf.Abs(off);
                float k = RiseK(_t - ts, 0.15f, 0.9f, 0.4f);
                float h = (1.6f - 0.06f * off * off * 0.25f) * k;
                RiseSpike(x, h, 0.22f, 1f);
                if (h > 0.1f)
                {
                    int n = Seg(x - 0.15f, groundY + h, x + 0.15f, groundY + h, 2);
                    Draw(Taper.Flat, n, 0.04f * thickness, primary, k);
                }

                if (_t >= ts && Once(1 << (i + 2)))
                {
                    Burst(x, groundY + 0.05f, 6, 3f, 2, 1.0f);
                }
            }

            float re = EaseOut(_t / 0.5f);
            int rn = Ring(c.x, groundY, 2.6f * re, 0.35f * re);
            Draw(Taper.Flat, rn, 0.05f * thickness, primary, 1f - Mathf.Clamp01(_t / 1.5f));
            if (_t < 1.2f)
            {
                _acc += _dt * 70f;
                while (_acc >= 1f)
                {
                    _acc -= 1f;
                    Emit(Random.value > 0.5f ? _stars : _sparks, c.x + (Random.value - 0.5f) * 4.4f, groundY + Random.value * 1.5f, (Random.value - 0.5f) * 0.3f, 0.5f + 1.5f * Random.value, 0.07f + 0.07f * Random.value, 0.9f, Hue(primary));
                }
            }
        }
    }
}
