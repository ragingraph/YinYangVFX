using UnityEngine;

namespace YinYangVFX
{
    // UI effects. Origin = element centre. These are world-space (LineRenderer + particles), so use them
    // with a Screen Space - Camera / World Space canvas, or place them in front of the camera. Single colour per prefab.
    public partial class WeaponEffect
    {
        // Point on a rectangle perimeter, s in [0,1).
        Vector2 PerimPt(float cx, float cy, float hw, float hh, float s)
        {
            s = Mathf.Repeat(s, 1f);
            float P = 4f * (hw + hh) * 1f;
            float d = s * P * 0.5f * 2f; // total perimeter = 4*(hw+hh)
            d = s * 4f * (hw + hh);
            float w = 2f * hw, h = 2f * hh;
            if (d < w) return new Vector2(cx - hw + d, cy + hh);
            d -= w;
            if (d < h) return new Vector2(cx + hw, cy + hh - d);
            d -= h;
            if (d < w) return new Vector2(cx + hw - d, cy - hh);
            d -= w;
            return new Vector2(cx - hw, cy - hh + d);
        }

        // Looping button highlight: a light runs around the button outline.
        void AnimButtonGlow()
        {
            Vector2 c = sweepCenter;
            float hw = 1.2f, hh = 0.42f;
            const int m = 24;
            for (int i = 0; i <= 4; i++)
            {
                Vector2 p = PerimPt(c.x, c.y, hw, hh, i * 0.25f + 0.0001f);
                _buf[i] = new Vector3(p.x, p.y, 0f);
            }

            Draw(Taper.Flat, 5, 0.04f * thickness, primary, 0.45f + 0.15f * Mathf.Sin(_t * 3f));
            float s = _t * 0.5f;
            for (int i = 0; i < m; i++)
            {
                Vector2 p = PerimPt(c.x, c.y, hw, hh, s - 0.22f * (1f - i / (float)(m - 1)));
                _buf[i] = new Vector3(p.x, p.y, 0f);
            }

            Vector3 hp = _buf[m - 1];
            Beam(Taper.Head, m, 0.12f, 1f);
            Emit(_dots, hp.x, hp.y, 0f, 0f, 0.5f, 0.07f, Hue(primary));
            Emit(_dots, c.x, c.y, 0f, 0f, 2.6f, 0.07f, Tint(0.05f + 0.05f * _pulse));
            if (Live())
            {
                _acc += _dt * 40f;
                while (_acc >= 1f)
                {
                    _acc -= 1f;
                    Emit(Random.value > 0.5f ? _stars : _sparks, hp.x, hp.y, (Random.value - 0.5f) * 0.8f, (Random.value - 0.5f) * 0.8f, 0.06f + 0.05f * Random.value, 0.5f, Hue(primary));
                }
            }
        }

        void AnimRewardBurst()
        {
            Vector2 c = sweepCenter;
            float g = FadeFrom(1.2f);
            float e = EaseOut(_t / 0.35f);
            if (Once(1))
            {
                Fire();
                Burst(c.x, c.y, 60, 6f, 30, 3.0f);
                Emit(_dots, c.x, c.y, 0f, 0f, 3.5f, 0.3f, Hue(primary));
                for (int j = 0; j < 22; j++)
                {
                    float a = Random.value * 6.2832f, sp = 2f + 3f * Random.value;
                    Emit(_stars, c.x, c.y, Mathf.Cos(a) * sp, Mathf.Sin(a) * sp + 2f, 0.12f + 0.12f * Random.value, 1.3f, Hue(primary));
                }
            }

            for (int k = 0; k < 16; k++)
            {
                float a = (k * 22.5f + _t * 25f) * Mathf.Deg2Rad;
                float L = (k % 2 == 0 ? 2.2f : 1.4f) * e;
                int n = Seg(c.x + Mathf.Cos(a) * 0.3f, c.y + Mathf.Sin(a) * 0.3f, c.x + Mathf.Cos(a) * (0.3f + L), c.y + Mathf.Sin(a) * (0.3f + L), 2);
                Draw(Taper.Head, n, 0.07f * thickness, primary, (1f - Mathf.Clamp01(_t / 1.0f)) * g);
            }

            for (int j = 0; j < 2; j++)
            {
                float d = Mathf.Clamp01((_t - 0.1f * j) / 0.7f);
                int rn = Ring(c.x, c.y, 2.2f * EaseOut(d), 2.2f * EaseOut(d));
                Draw(Taper.Flat, rn, 0.08f * (1f - d) * thickness, primary, (1f - d) * g);
            }

            Flare(c.x, c.y, 1.0f * e * (1f - Mathf.Clamp01(_t / 0.9f)), g, _t * 60f);
        }

        void AnimUILevelUp()
        {
            Vector2 c = sweepCenter;
            float g = FadeFrom(1.3f);
            float e = EaseOut(_t / 0.4f);
            if (Once(1))
            {
                Fire();
                Burst(c.x, c.y, 40, 6f, 20, 2.4f);
                Emit(_dots, c.x, c.y, 0f, 0f, 3f, 0.3f, Hue(primary));
            }

            float bf = 1f - Mathf.Clamp01((_t - 0.8f) / 0.5f);
            for (int s = -1; s <= 1; s += 2)
            {
                int n = Seg(c.x - 3.0f * e, c.y + 0.5f * s, c.x + 3.0f * e, c.y + 0.5f * s, 2);
                Draw(Taper.Lens, n, 0.06f * thickness, primary, bf * g);
            }

            int bn = Seg(c.x - 3.0f * e, c.y, c.x + 3.0f * e, c.y, 2);
            Draw(Taper.Lens, bn, 0.5f * thickness * bf, primary * 0.4f, bf * g);
            Flare(c.x, c.y, 1.2f * e * bf, g, 0f);
            for (int i = 0; i < 3; i++)
            {
                float f = Mathf.Repeat(_t * 0.9f + i / 3f, 1f);
                float yy = c.y + 0.3f + f * 1.6f;
                for (int sd = -1; sd <= 1; sd += 2)
                {
                    float x = c.x + sd * 1.4f;
                    int n = Seg(x - 0.2f, yy - 0.15f, x, yy, 2);
                    Draw(Taper.Flat, n, 0.07f * thickness, primary, Mathf.Sin(f * Mathf.PI) * g);
                    n = Seg(x + 0.2f, yy - 0.15f, x, yy, 2);
                    Draw(Taper.Flat, n, 0.07f * thickness, primary, Mathf.Sin(f * Mathf.PI) * g);
                }
            }

            if (_t < 1.0f)
            {
                _acc += _dt * 70f;
                while (_acc >= 1f)
                {
                    _acc -= 1f;
                    Emit(Random.value > 0.5f ? _stars : _sparks, c.x + (Random.value - 0.5f) * 5f, c.y + (Random.value - 0.5f) * 0.6f, (Random.value - 0.5f) * 0.5f, 1f + 2f * Random.value, 0.08f + 0.08f * Random.value, 1.0f, Hue(primary));
                }
            }
        }

        void AnimAchievement()
        {
            Vector2 c = sweepCenter;
            float g = FadeFrom(1.5f);
            float R = 0.7f;
            float k = Mathf.Clamp01(_t / 0.6f);
            int n = ArcPts(c.x, c.y, R, R, 90f, 90f - 360f * EaseOut(k), 40);
            Draw(Taper.Flat, n, 0.09f * thickness, primary, g);
            if (k >= 1f)
            {
                n = Ring(c.x, c.y, R, R);
                Draw(Taper.Flat, n, 0.09f * thickness, primary, g);
            }

            n = Ring(c.x, c.y, R * 0.7f, R * 0.7f);
            Draw(Taper.Flat, n, 0.03f * thickness, primary, 0.6f * g * k);
            if (_t >= 0.6f && Once(1))
            {
                Fire();
                Burst(c.x, c.y, 36, 5f, 16, 2.6f);
                Emit(_dots, c.x, c.y, 0f, 0f, 3.0f, 0.3f, Hue(primary));
            }

            if (_t >= 0.6f)
            {
                float it = _t - 0.6f;
                float re = EaseOut(it / 0.5f);
                n = Ring(c.x, c.y, R + 1.3f * re, R + 1.3f * re);
                Draw(Taper.Flat, n, 0.06f * (1f - re) * thickness, primary, (1f - re) * g);
                // diagonal shine sweeping across the badge
                float sh = Mathf.Clamp01(it / 0.5f);
                float sx = c.x - 1.0f + 2.0f * sh;
                n = Seg(sx - 0.3f, c.y - R, sx + 0.3f, c.y + R, 2);
                Draw(Taper.Lens, n, 0.18f * thickness, primary, Mathf.Sin(sh * Mathf.PI) * g);
                Flare(c.x, c.y, 0.9f * (1f - Mathf.Clamp01(it / 0.8f)), g, 45f);
            }

            Emit(_dots, c.x, c.y, 0f, 0f, 2.0f * R, 0.07f, Tint(0.12f * k));
        }

        // Countdown ring: arc shrinks over `duration`; pulse each second; onImpact at the end. Set duration to the countdown length.
        void AnimCountdownRing()
        {
            Vector2 c = sweepCenter;
            float R = 0.9f;
            float rem = 1f - Mathf.Clamp01(_t / duration);
            int n = Ring(c.x, c.y, R, R);
            Draw(Taper.Flat, n, 0.03f * thickness, primary, 0.3f);
            if (rem > 0.001f)
            {
                n = ArcPts(c.x, c.y, R, R, 90f, 90f + 360f * rem, 48);
                Vector3 hp = _buf[n - 1];
                Draw(Taper.Flat, n, 0.12f * thickness, primary, 1f);
                Emit(_dots, hp.x, hp.y, 0f, 0f, 0.5f, 0.07f, Hue(primary));
                Emit(_sparks, hp.x, hp.y, (Random.value - 0.5f) * 1.2f, (Random.value - 0.5f) * 1.2f, 0.06f, 0.4f, Hue(primary));
            }

            float tk = Mathf.Repeat(_t, 1f);
            if (tk < 0.4f)
            {
                float f = EaseOut(tk / 0.4f);
                n = Ring(c.x, c.y, R + 0.5f * f, R + 0.5f * f);
                Draw(Taper.Flat, n, 0.05f * (1f - f) * thickness, primary, 1f - f);
            }

            if (_t >= duration - 0.05f && Once(1))
            {
                Fire();
                Burst(c.x, c.y, 24, 5f, 10, 2.2f);
                Emit(_dots, c.x, c.y, 0f, 0f, 2.6f, 0.25f, Hue(primary));
            }
        }

        // Looping highlight twinkles over a 2.6 x 1.0 area.
        void AnimSparkleLoop()
        {
            Vector2 c = sweepCenter;
            for (int i = 0; i < 6; i++)
            {
                float cyc = Mathf.Repeat(_t * 0.7f + Rnd(i, 1), 1f);
                int seed = Mathf.FloorToInt(_t * 0.7f + Rnd(i, 1)) * 13 + i;
                float x = c.x + (Rnd(seed, 2) - 0.5f) * 2.6f, y = c.y + (Rnd(seed, 3) - 0.5f) * 1.0f;
                float s = Mathf.Sin(cyc * Mathf.PI);
                Flare(x, y, 0.28f * s, s, 0f);
                Emit(_dots, x, y, 0f, 0f, 0.3f * s, 0.07f, Hue(primary));
            }

            if (Live())
            {
                _acc += _dt * 8f;
                while (_acc >= 1f)
                {
                    _acc -= 1f;
                    Emit(_stars, c.x + (Random.value - 0.5f) * 2.6f, c.y + (Random.value - 0.5f) * 1.0f, 0f, 0.2f, 0.09f + 0.07f * Random.value, 0.8f, Hue(primary));
                }
            }
        }
    }
}
