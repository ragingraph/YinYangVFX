using UnityEngine;

namespace YinYangVFX
{
    // Elemental attacks: fire, ice, lightning, wind, water, earth, poison, holy, shadow.
    // Origin = the caster's hand (same conventions as the weapon effects). Ground = groundY.
    // Single colour per prefab: pick the colour that suits the element (Crimson = fire, Ice/Azure = ice and water,
    // Gold = lightning, Jade = wind and poison, Violet = shadow ...).
    public partial class WeaponEffect
    {
        // ------------------------------------------------------------------ helpers

        int WaveDir(float x0, float y0, float angDeg, float len, int n, float amp, float freq, float phase)
        {
            float a = angDeg * Mathf.Deg2Rad;
            float dx = Mathf.Cos(a), dy = Mathf.Sin(a);
            for (int i = 0; i < n; i++)
            {
                float s = i / (float)(n - 1);
                float off = amp * Mathf.Sin(s * freq + phase) * s;
                _buf[i] = new Vector3(x0 + dx * len * s - dy * off, y0 + dy * len * s + dx * off, 0f);
            }

            return n;
        }

        int Bolt(Vector2 a, Vector2 b, int n, float jag, int salt)
        {
            Vector2 d = b - a;
            Vector2 perp = new Vector2(-d.y, d.x).normalized;
            for (int i = 0; i < n; i++)
            {
                float s = i / (float)(n - 1);
                float env = Mathf.Sin(Mathf.PI * s);
                float off = (Rnd(i * 7 + salt, 9) - 0.5f) * 2f * jag * env;
                Vector2 q = a + d * s + perp * off;
                _buf[i] = new Vector3(q.x, q.y, 0f);
            }

            return n;
        }

        void DrawBolt(Vector2 a, Vector2 b, int n, float jag, int salt, float w, float g)
        {
            int k = Bolt(a, b, n, jag, salt);
            Draw(Taper.Lens, k, w * 3f * thickness, primary * 0.4f, g * 0.5f);
            Draw(Taper.Lens, k, w * thickness, primary, g);
            Draw(Taper.Lens, k, w * 0.4f * thickness, _core, g);
        }

        // ------------------------------------------------------------------ fire

        void AnimFireball()
        {
            const float fl = 0.45f;
            float p = Mathf.Clamp01(_t / fl);
            float x = Mathf.Lerp(0.3f, 3.2f, p * 0.7f + p * p * 0.3f);
            float g = FadeFrom(fl + 0.2f);

            if (p < 1f)
            {
                Emit(_dots, x, 0f, 0f, 0f, 1.0f, 0.07f, Hue(primary));
                Emit(_dots, x, 0f, 0f, 0f, 0.5f, 0.07f, Tint(0.9f));
                for (int j = 0; j < 5; j++)
                {
                    float y0 = (j - 2) * 0.07f;
                    float len = 0.8f + 0.3f * Rnd(j);
                    int n = WaveDir(x - len, y0, 0f, len, 16, 0.09f, 9f, _t * 30f + j);
                    Draw(Taper.Head, n, (0.2f - 0.03f * Mathf.Abs(j - 2)) * thickness, primary, 0.9f);
                }

                int m = Seg(x - 1.3f, 0f, x, 0f, 10);
                Beam(Taper.Head, m, 0.26f, 1f);
                Trail(x - 0.3f, 0f, 120f);
                return;
            }

            float it = _t - fl;
            if (Once(1))
            {
                Fire();
                Burst(3.2f, 0f, 40, 6f, 12, 3f);
            }

            float e = EaseOut(it / 0.25f);
            for (int j = 0; j < 6; j++)
            {
                float xx = 3.2f + (j - 2.5f) * 0.15f;
                float h = (0.8f + 0.5f * Rnd(j, 3)) * e;
                for (int i = 0; i < 12; i++)
                {
                    float s = i / 11f;
                    _buf[i] = new Vector3(xx + 0.07f * Mathf.Sin(s * 8f + _t * 20f + j) * (1f - s), h * (1f - s), 0f);
                }

                Draw(Taper.Head, 12, 0.16f * thickness, primary, g * (1f - 0.4f * it));
            }

            int rn = Ring(3.2f, 0f, 0.2f + 0.9f * e, 0.2f + 0.9f * e);
            Draw(Taper.Flat, rn, 0.04f * thickness, primary, (1f - e) * g);
        }

        void AnimFlamethrower()
        {
            float g = FadeFrom(0.55f);
            float L = Mathf.Lerp(0.1f, 2.4f, EaseOut(_t / 0.25f));
            if (_t >= 0.1f) Fire();

            for (int j = 0; j < 9; j++)
            {
                float ang = (j - 4) * 5f;
                int n = WaveDir(0.4f, 0f, ang, L * (0.8f + 0.2f * Rnd(j)), 18, 0.12f, 10f, _t * 26f + j * 1.3f);
                Draw(Taper.Head, n, (0.22f - 0.018f * Mathf.Abs(j - 4)) * thickness, primary, g * 0.9f);
            }

            int c = Seg(0.4f, 0f, 0.4f + L, 0f, 10);
            Beam(Taper.Head, c, 0.2f, g);

            if (_t < 0.6f)
            {
                for (int k = 0; k < 6; k++)
                {
                    float r = Random.value;
                    float x = 0.4f + r * L;
                    Emit(_dots, x, (Random.value - 0.5f) * 0.45f * r, 1.2f, 0.3f * Random.value, 0.5f + 0.5f * r, 0.3f, Tint(0.35f));
                }

                Trail(0.4f + L, 0f, 100f);
            }
        }

        // ------------------------------------------------------------------ ice

        void AnimIceSpike()
        {
            float g = FadeFrom(0.5f);
            for (int k = 0; k < 5; k++)
            {
                float st = 0.05f * k;
                float e = EaseOut((_t - st) / 0.12f);
                if (e <= 0f)
                {
                    continue;
                }

                float x = 0.5f + 0.45f * k;
                float H = (0.7f + 0.5f * Rnd(k)) * e;
                float tilt = (Rnd(k, 2) - 0.5f) * 0.2f;
                int n = Seg(x, groundY, x + tilt, groundY + H, 2);
                Draw(Taper.Lens, n, 0.24f * thickness, primary, g);
                Draw(Taper.Lens, n, 0.08f * thickness, _core, g);
                n = Seg(x + 0.08f, groundY, x + 0.3f, groundY + H * 0.55f, 2);
                Draw(Taper.Lens, n, 0.12f * thickness, primary, g * 0.8f);

                if (Once(1 << k))
                {
                    if (k == 0) Fire();
                    Burst(x, groundY, 8, 3f, 4, 1.0f);
                }
            }

            float re = EaseOut(_t / 0.3f);
            int rn = Ring(1.5f, groundY, 1.8f * re, 0.12f + 0.1f * re);
            Draw(Taper.Flat, rn, 0.03f * thickness, primary, (1f - re * 0.6f) * g);
        }

        void AnimFreezeBurst()
        {
            Vector2 c = sweepCenter;
            float g = FadeFrom(0.5f);
            float e = EaseOut(_t / 0.35f);
            if (Once(1))
            {
                Fire();
                Burst(c.x, c.y, 40, 6f, 20, 2.6f);
            }

            int rn = Ring(c.x, c.y, 1.3f * e, 1.3f * e);
            Draw(Taper.Flat, rn, 0.07f * thickness, primary, g);
            Draw(Taper.Flat, rn, 0.02f * thickness, _core, g);

            float rot = _t * 20f;
            for (int k = 0; k < 6; k++)
            {
                float a = (rot + k * 60f) * Mathf.Deg2Rad;
                float R = 1.0f * e;
                float dx = Mathf.Cos(a), dy = Mathf.Sin(a);
                int n = Seg(c.x, c.y, c.x + dx * R, c.y + dy * R, 2);
                Draw(Taper.Lens, n, 0.09f * thickness, primary, g);
                float bx = c.x + dx * R * 0.6f, by = c.y + dy * R * 0.6f;
                float a1 = a + 60f * Mathf.Deg2Rad, a2 = a - 60f * Mathf.Deg2Rad;
                _buf[0] = new Vector3(bx + Mathf.Cos(a1) * R * 0.3f, by + Mathf.Sin(a1) * R * 0.3f, 0f);
                _buf[1] = new Vector3(bx, by, 0f);
                _buf[2] = new Vector3(bx + Mathf.Cos(a2) * R * 0.3f, by + Mathf.Sin(a2) * R * 0.3f, 0f);
                Draw(Taper.Flat, 3, 0.04f * thickness, primary, g * 0.9f);
            }
        }

        // ------------------------------------------------------------------ lightning

        void AnimLightningChain()
        {
            int fr = Mathf.FloorToInt(_t * 30f);
            float flick = 0.7f + 0.3f * Rnd(Mathf.FloorToInt(_t * 60f), 4);
            float g = FadeFrom(0.35f) * flick;
            Vector2 t0 = new Vector2(0.3f, 0f), t1 = new Vector2(2.0f, 0.1f), t2 = new Vector2(3.4f, -0.2f);

            if (_t >= 0.0f)
            {
                DrawBolt(t0, t1, 14, 0.28f, fr, 0.1f, g);
                for (int b = 0; b < 2; b++)
                {
                    float s = 0.3f + 0.35f * b;
                    Vector2 m = Vector2.Lerp(t0, t1, s);
                    DrawBolt(m, m + new Vector2(0.5f, (b == 0 ? 1f : -1f) * 0.6f), 6, 0.12f, fr + b * 5, 0.04f, g * 0.8f);
                }

                if (Once(1))
                {
                    Fire();
                    Burst(t1.x, t1.y, 24, 5f, 8, 1.8f);
                    Burst(t0.x, t0.y, 10, 3f, 4, 1.2f);
                }
            }

            if (_t >= 0.12f)
            {
                DrawBolt(t1, t2, 12, 0.26f, fr + 11, 0.09f, g);
                Vector2 m = Vector2.Lerp(t1, t2, 0.5f);
                DrawBolt(m, m + new Vector2(0.4f, 0.5f), 6, 0.1f, fr + 3, 0.035f, g * 0.7f);
                if (Once(2)) Burst(t2.x, t2.y, 24, 5f, 8, 1.8f);
            }
        }

        void AnimLightningStrike()
        {
            int fr = Mathf.FloorToInt(_t * 30f);
            float g = FadeFrom(0.35f) * (0.75f + 0.25f * Rnd(Mathf.FloorToInt(_t * 60f), 4));
            float ix = impactOffset.x;
            Vector2 top = new Vector2(ix + 0.3f, 4.0f), hit = new Vector2(ix, groundY);

            if (_t < 0.08f)
            {
                int n = Seg(top.x, top.y, hit.x, hit.y, 2);
                Draw(Taper.Flat, n, 0.02f * thickness, primary, 0.5f);
                return;
            }

            DrawBolt(top, hit, 18, 0.35f, fr, 0.16f, g);
            for (int b = 0; b < 3; b++)
            {
                Vector2 m = Vector2.Lerp(top, hit, 0.3f + 0.2f * b);
                DrawBolt(m, m + new Vector2((b % 2 == 0 ? 1f : -1f) * 0.7f, -0.8f), 6, 0.15f, fr + b * 3, 0.05f, g * 0.8f);
            }

            if (Once(1))
            {
                Fire();
                Burst(hit.x, hit.y, 40, 7f, 14, 3.5f);
            }

            float it = _t - 0.08f;
            float e = EaseOut(it / 0.3f);
            int pn = Seg(hit.x, hit.y + 3.5f, hit.x, hit.y, 2);
            Draw(Taper.Head, pn, 0.5f * (1f - e * 0.6f) * thickness, primary * 0.6f, g * 0.6f);
            for (int j = 0; j < 2; j++)
            {
                int rn = Ring(hit.x, groundY, (0.4f + 1.0f * e) * (1f + 0.5f * j), (0.08f + 0.2f * e) * (1f + 0.5f * j));
                Draw(Taper.Flat, rn, 0.035f * thickness, primary, (1f - e) * g);
            }
        }

        // ------------------------------------------------------------------ wind

        void AnimWindBlade()
        {
            float fadeStart = duration - 0.3f;
            float g = FadeFrom(fadeStart);
            float p = Mathf.Clamp01(_t / Mathf.Max(0.1f, duration - 0.2f));
            float fx = Mathf.Lerp(0.5f, qiTravel, 1f - Mathf.Pow(1f - p, 1.4f));
            float r = 0.5f;

            QiCrescent(fx, r, 0.1f, primary, g, 0f, 0);
            QiCrescent(fx - 0.1f, r * 0.82f, 0.06f, primary, g * 0.8f, 0f, 0);
            QiCrescent(fx - 0.2f, r * 0.62f, 0.05f, primary, g * 0.6f, 0f, 0);
            QiCrescent(fx, r, 0.04f, _core, g, 0f, 0);

            for (int j = 0; j < 6; j++)
            {
                float y = (j - 2.5f) * 0.22f;
                int n = WaveDir(fx - 1.5f, y, 0f, 1.4f, 16, 0.07f, 12f, _t * 24f + j);
                Draw(Taper.Head, n, 0.04f * thickness, primary, g * 0.6f);
            }

            if (_t < fadeStart)
            {
                _acc += _dt * 50f;
                while (_acc >= 1f)
                {
                    _acc -= 1f;
                    Emit(_dots, fx - Random.value * 1.2f, groundY + 0.05f, -0.6f, 0.2f, 0.5f, 0.5f, Tint(0.25f));
                    Emit(_sparks, fx - Random.value * 1.2f, (Random.value - 0.5f) * 1.2f, -2f, 0f, 0.05f, 0.35f, Hue(primary));
                }
            }

            if (_t >= 0.15f) Fire();
        }

        void AnimTornado()
        {
            float g = FadeFrom(1.2f);
            float gr = EaseOut(_t / 0.4f);
            float bx = impactOffset.x;
            const int rings = 8;
            Fire();

            for (int k = 0; k < rings; k++)
            {
                float s = k / (float)(rings - 1);
                float y = groundY + s * 2.2f * gr;
                float rx = 0.15f + 0.55f * Mathf.Pow(s, 1.1f);
                float a0 = _t * 400f + k * 40f;
                int n = ArcPts(bx, y, rx, rx * 0.22f, a0, a0 + 220f, 18);
                Draw(Taper.Head, n, 0.07f * thickness, primary, g * (0.6f + 0.4f * s));
            }

            for (int sp = 0; sp < 2; sp++)
            {
                const int m = 24;
                for (int i = 0; i < m; i++)
                {
                    float s = i / (float)(m - 1);
                    float a = (_t * 5f + s * 6f + sp * Mathf.PI) * 1f;
                    float rx = 0.15f + 0.55f * Mathf.Pow(s, 1.1f);
                    _buf[i] = new Vector3(bx + rx * Mathf.Cos(a), groundY + s * 2.2f * gr + rx * 0.22f * Mathf.Sin(a), 0f);
                }

                Draw(Taper.Head, m, 0.09f * thickness, _core, g * 0.8f);
            }

            if (_t < 1.2f)
            {
                _acc += _dt * 60f;
                while (_acc >= 1f)
                {
                    _acc -= 1f;
                    float s = Random.value;
                    float a = Random.value * 6.2832f;
                    float rx = 0.15f + 0.55f * Mathf.Pow(s, 1.1f);
                    Emit(_sparks, bx + rx * Mathf.Cos(a), groundY + s * 2.2f * gr, -Mathf.Sin(a) * 2.5f, 1.2f, 0.06f, 0.5f, Hue(primary));
                    if (Random.value < 0.4f) Emit(_dots, bx + (Random.value - 0.5f) * 0.8f, groundY + 0.05f, (Random.value - 0.5f) * 1f, 0.3f, 0.5f, 0.6f, Tint(0.25f));
                }
            }
        }

        // ------------------------------------------------------------------ water

        void AnimWaterBall()
        {
            const float fl = 0.4f;
            float p = Mathf.Clamp01(_t / fl);
            float x = Mathf.Lerp(0.3f, 3.0f, p);
            float g = FadeFrom(fl + 0.3f);

            if (p < 1f)
            {
                for (int j = 0; j < 3; j++)
                {
                    float r = 0.3f - 0.06f * j;
                    int n = Ring(x, 0f, r, r);
                    Draw(Taper.Flat, n, (j == 0 ? 0.06f : 0.03f) * thickness, primary, 1f - 0.2f * j);
                }

                int w = WaveDir(x - 0.25f, 0.02f, 0f, 0.5f, 12, 0.05f, 12f, _t * 20f);
                Draw(Taper.Flat, w, 0.03f * thickness, _core, 0.9f);
                int m = Seg(x - 1.0f, 0f, x - 0.2f, 0f, 8);
                Beam(Taper.Head, m, 0.18f, 1f);
                Emit(_dots, x, 0f, 0f, 0f, 0.8f, 0.07f, Tint(0.35f));
                Trail(x - 0.3f, 0f, 90f);
                return;
            }

            float it = _t - fl;
            if (Once(1))
            {
                Fire();
                Burst(3.0f, 0f, 30, 5f, 6, 2.2f);
                for (int j = 0; j < 20; j++)
                {
                    Emit(_sparks, 3.0f + (Random.value - 0.5f) * 0.3f, 0f, (Random.value - 0.3f) * 3.5f, 2f + 2.5f * Random.value, 0.06f, 0.9f, Hue(primary));
                }
            }

            float e = EaseOut(it / 0.4f);
            for (int j = 0; j < 2; j++)
            {
                int n = Ring(3.0f, 0f, (0.2f + 0.9f * e) * (1f + 0.4f * j), (0.2f + 0.9f * e) * (1f + 0.4f * j));
                Draw(Taper.Flat, n, 0.035f * thickness, primary, (1f - e) * g * (1f - 0.3f * j));
            }
        }

        void AnimWaterColumn()
        {
            float g = FadeFrom(0.75f);
            float bx = impactOffset.x;
            float e = EaseOut(_t / 0.25f);
            float H = 2.2f * e;
            if (Once(1)) Fire();

            int c = Seg(bx, groundY + H, bx, groundY, 2);
            Draw(Taper.Head, c, 0.7f * thickness, primary * 0.5f, g * 0.7f);
            Draw(Taper.Head, c, 0.25f * thickness, _core, g);

            for (int side = 0; side < 2; side++)
            {
                const int n = 16;
                for (int i = 0; i < n; i++)
                {
                    float s = i / (float)(n - 1);
                    float off = (side == 0 ? -1f : 1f) * (0.3f + 0.06f * Mathf.Sin(s * 14f - _t * 18f));
                    _buf[i] = new Vector3(bx + off * (1f - 0.3f * s), groundY + H * s, 0f);
                }

                Draw(Taper.Flat, n, 0.05f * thickness, primary, g * 0.9f);
            }

            if (_t < 0.7f)
            {
                _acc += _dt * 50f;
                while (_acc >= 1f)
                {
                    _acc -= 1f;
                    Emit(_sparks, bx + (Random.value - 0.5f) * 0.6f, groundY + H, (Random.value - 0.5f) * 1.6f, 0.5f + 1.5f * Random.value, 0.06f, 0.9f, Hue(primary));
                }
            }

            for (int j = 0; j < 3; j++)
            {
                float d = Mathf.Clamp01((_t - 0.12f * j) / 0.6f);
                float ed = EaseOut(d);
                int rn = Ring(bx, groundY, 0.2f + 1.0f * ed, 0.05f + 0.2f * ed);
                Draw(Taper.Flat, rn, 0.03f * thickness, primary, (1f - d) * g);
            }
        }

        // ------------------------------------------------------------------ earth

        void AnimRockSpike()
        {
            float g = FadeFrom(0.55f);
            for (int k = 0; k < 5; k++)
            {
                float st = 0.05f * k;
                float e = EaseOut((_t - st) / 0.1f);
                if (e <= 0f)
                {
                    continue;
                }

                float x = 0.5f + 0.45f * k;
                float H = (0.6f + 0.6f * Rnd(k)) * e;
                _buf[0] = new Vector3(x - 0.17f, groundY, 0f);
                _buf[1] = new Vector3(x - 0.07f, groundY + H * 0.5f, 0f);
                _buf[2] = new Vector3(x + 0.02f, groundY + H, 0f);
                _buf[3] = new Vector3(x + 0.08f, groundY + H * 0.55f, 0f);
                _buf[4] = new Vector3(x + 0.17f, groundY, 0f);
                _buf[5] = _buf[0];
                Draw(Taper.Flat, 6, 0.06f * thickness, primary, g);
                int n = Seg(x, groundY, x + 0.02f, groundY + H * 0.9f, 2);
                Draw(Taper.Lens, n, 0.22f * thickness, primary * 0.7f, g * 0.8f);

                if (Once(1 << k))
                {
                    if (k == 0) Fire();
                    Burst(x, groundY, 10, 3.5f, 2, 1.0f);
                    Emit(_dots, x, groundY + 0.1f, 0f, 0.3f, 0.9f, 0.7f, Tint(0.35f));
                    for (int j = 0; j < 4; j++)
                    {
                        Emit(_sparks, x, groundY + H * 0.5f, (Random.value - 0.5f) * 3f, 1.5f + 2f * Random.value, 0.12f, 0.8f, Hue(primary));
                    }
                }
            }

            float ce = EaseOut(_t / 0.3f);
            for (int c = 0; c < 3; c++)
            {
                int n = Seg(0.4f, groundY, 0.4f + (1.8f - 0.3f * c) * ce, groundY - 0.03f - 0.04f * c, 2);
                Draw(Taper.Tail, n, 0.05f * thickness, primary, g * 0.8f);
            }
        }

        void AnimFallingRocks()
        {
            float g = FadeFrom(0.9f);
            for (int k = 0; k < 6; k++)
            {
                float st = 0.08f * k;
                float p = Mathf.Clamp01((_t - st) / 0.25f);
                if (_t < st)
                {
                    continue;
                }

                float x = 0.7f + 0.45f * k + (Rnd(k) - 0.5f) * 0.2f;
                float y = Mathf.Lerp(3.5f, groundY + 0.15f, p * p);
                if (p < 1f)
                {
                    int n = Seg(x, y + 1.0f, x, y, 6);
                    Draw(Taper.Head, n, 0.1f * thickness, primary, 0.8f);
                    int rn = Ring(x, y, 0.1f, 0.1f);
                    Draw(Taper.Flat, rn, 0.12f * thickness, primary, 1f);
                    Emit(_dots, x, y, 0f, 0f, 0.35f, 0.06f, Hue(primary));
                }
                else
                {
                    if (Once(1 << k))
                    {
                        if (k == 0) Fire();
                        Burst(x, groundY, 12, 3.5f, 2, 1.0f);
                        Emit(_dots, x, groundY + 0.1f, 0f, 0.3f, 0.9f, 0.7f, Tint(0.35f));
                    }

                    float e = EaseOut((_t - st - 0.25f) / 0.3f);
                    int rn = Ring(x, groundY, 0.1f + 0.5f * e, 0.03f + 0.1f * e);
                    Draw(Taper.Flat, rn, 0.03f * thickness, primary, (1f - e) * g);
                }
            }
        }

        // ------------------------------------------------------------------ poison

        void AnimPoisonMist()
        {
            float g = FadeFrom(1.4f);
            if (Once(1)) Fire();

            if (_t < 1.4f)
            {
                _acc += _dt * 30f;
                while (_acc >= 1f)
                {
                    _acc -= 1f;
                    Emit(_dots, 0.6f + 2.0f * Random.value, groundY + 0.1f + 0.7f * Random.value, 0.15f + 0.2f * Random.value, 0.1f, 0.8f + 0.6f * Random.value, 1.0f, Tint(0.2f));
                }
            }

            for (int k = 0; k < 7; k++)
            {
                float x = 0.8f + 1.8f * Rnd(k);
                float y = groundY + Mathf.Repeat(_t * 0.5f + Rnd(k, 1), 1f) * 1.2f;
                float r = 0.05f + 0.07f * Rnd(k, 2);
                int n = Ring(x, y, r, r);
                Draw(Taper.Flat, n, 0.015f * thickness, primary, 0.8f * g);
            }

            for (int w = 0; w < 3; w++)
            {
                int n = WaveDir(0.6f + 0.5f * w, groundY + 0.3f + 0.3f * w, 0f, 1.6f, 16, 0.1f, 8f, _t * 3f + w * 2f);
                Draw(Taper.Head, n, 0.1f * thickness, primary * 0.6f, g * 0.6f);
            }
        }

        void AnimPoisonShot()
        {
            const float fl = 0.4f;
            float p = Mathf.Clamp01(_t / fl);
            float x = Mathf.Lerp(0.3f, 3.0f, p);
            float g = FadeFrom(fl + 0.35f);

            if (p < 1f)
            {
                int n = Seg(x - 1.0f, 0f, x, 0f, 8);
                Beam(Taper.Head, n, 0.18f, 1f);
                int rn = Ring(x, 0f, 0.15f, 0.15f);
                Draw(Taper.Flat, rn, 0.08f * thickness, primary, 1f);
                Emit(_dots, x, 0f, 0f, 0f, 0.7f, 0.07f, Tint(0.4f));
                _acc += _dt * 40f;
                while (_acc >= 1f)
                {
                    _acc -= 1f;
                    Emit(_sparks, x - Random.value * 0.5f, 0f, -0.3f, -0.5f, 0.07f, 0.8f, Hue(primary));
                }

                return;
            }

            float it = _t - fl;
            if (Once(1))
            {
                Fire();
                Burst(3.0f, 0f, 24, 4f, 6, 2f);
            }

            float e = EaseOut(it / 0.35f);
            int pn = Ring(3.0f, groundY, 0.7f * e, 0.12f * e);
            Draw(Taper.Flat, pn, 0.06f * thickness, primary, g);
            for (int k = 0; k < 4; k++)
            {
                float bx = 3.0f - 0.4f + 0.27f * k;
                float by = groundY + Mathf.Repeat(it * 0.6f + 0.2f * k, 0.5f);
                int bn = Ring(bx, by, 0.05f, 0.05f);
                Draw(Taper.Flat, bn, 0.015f * thickness, primary, g * 0.8f);
            }
        }

        // ------------------------------------------------------------------ holy / shadow

        void AnimHolyLight()
        {
            float g = FadeFrom(0.9f);
            float bx = impactOffset.x;
            float e = EaseOut(_t / 0.25f);
            if (_t >= 0.2f) Fire();

            float w = 0.9f * e * (0.9f + 0.1f * Mathf.Sin(_t * 12f));
            int c = Seg(bx, 4.0f, bx, groundY, 2);
            Draw(Taper.Head, c, w * thickness, primary * 0.5f, g * 0.7f);
            Draw(Taper.Head, c, w * 0.35f * thickness, _core, g);

            for (int j = 0; j < 7; j++)
            {
                float a = Mathf.Lerp(-32f, 32f, j / 6f) * Mathf.Deg2Rad;
                int n = Seg(bx, 3.2f, bx + Mathf.Sin(a) * 3.4f, 3.2f - Mathf.Cos(a) * 3.4f, 2);
                Draw(Taper.Head, n, 0.05f * thickness, primary, g * 0.5f * e);
            }

            int rn = Ring(bx, groundY + 1.9f, 0.5f * e, 0.12f * e);
            Draw(Taper.Flat, rn, 0.03f * thickness, primary, g);
            float ge = EaseOut(_t / 0.6f);
            rn = Ring(bx, groundY, 1.0f * ge, 0.2f * ge);
            Draw(Taper.Flat, rn, 0.04f * thickness, primary, (1f - ge * 0.5f) * g);

            int cs = Seg(bx - 0.5f, 3.2f, bx + 0.5f, 3.2f, 2);
            Draw(Taper.Lens, cs, 0.07f * thickness, _core, g * e);
            cs = Seg(bx, 2.8f, bx, 3.6f, 2);
            Draw(Taper.Lens, cs, 0.07f * thickness, _core, g * e);

            if (_t < 0.9f)
            {
                _acc += _dt * 45f;
                while (_acc >= 1f)
                {
                    _acc -= 1f;
                    Emit(Random.value > 0.4f ? _stars : _dots, bx + (Random.value - 0.5f) * 0.9f, groundY + Random.value * 0.3f, 0f, 0.8f + 1.2f * Random.value, 0.1f + 0.12f * Random.value, 0.9f, Hue(primary));
                }
            }
        }

        void AnimShadowOrb()
        {
            float g = FadeFrom(1.0f);
            Vector2 o = new Vector2(1.5f, 0.3f);
            float e = EaseOut(_t / 0.3f);
            if (_t < 0.9f)
            {
                float r = 0.38f * e * (1f + 0.05f * Mathf.Sin(_t * 18f));
                int n = Ring(o.x, o.y, r, r);
                Draw(Taper.Flat, n, 0.07f * thickness, primary, g);
                Draw(Taper.Flat, n, 0.02f * thickness, _core, g);
                Emit(_dots, o.x, o.y, 0f, 0f, 1.3f * e, 0.07f, Tint(0.4f));

                for (int a = 0; a < 3; a++)
                {
                    const int m = 22;
                    for (int i = 0; i < m; i++)
                    {
                        float s = i / (float)(m - 1);
                        float rr = Mathf.Lerp(1.2f, 0.38f, s) * e;
                        float ang = _t * 4f + s * 3.2f + a * 2.094f;
                        _buf[i] = new Vector3(o.x + rr * Mathf.Cos(ang), o.y + rr * Mathf.Sin(ang), 0f);
                    }

                    Draw(Taper.Head, m, 0.08f * thickness, primary, g * 0.9f);
                }

                _acc += _dt * 70f;
                while (_acc >= 1f)
                {
                    _acc -= 1f;
                    float ang = Random.value * 6.2832f;
                    float dx = Mathf.Cos(ang), dy = Mathf.Sin(ang);
                    Emit(_sparks, o.x + dx * 1.3f, o.y + dy * 1.3f, -dx * 2.4f, -dy * 2.4f, 0.06f, 0.45f, Hue(primary));
                }
            }
            else
            {
                if (Once(1))
                {
                    Fire();
                    Burst(o.x, o.y, 50, 7f, 16, 3.2f);
                }

                float be = EaseOut((_t - 0.9f) / 0.35f);
                int n = Ring(o.x, o.y, 0.3f + 1.4f * be, 0.3f + 1.4f * be);
                Draw(Taper.Flat, n, 0.06f * thickness, primary, (1f - be) * g);
            }
        }
    }
}
