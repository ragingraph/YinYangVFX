using UnityEngine;

namespace YinYangVFX
{
    // Concrete element effects: real flames, ice crystals, rocks, thick lightning bolts, water orbs/drops,
    // feathers, skulls and a dark orb, drawn as solid shapes with ink-brush ribbons. Origin = caster body centre.
    public partial class WeaponEffect
    {
        void EFlame(float x, float y, float len, float dir, float a, int salt)
        {
            float w = Mathf.Sin(_t * 14f + salt * 1.7f);
            DrawProp(Shape.Flame, x, y, dir + 4f * w, len * (1f + 0.08f * Mathf.Sin(_t * 17f + salt)), a);
        }

        // Jagged lightning polyline from A to B in the scratch buffer.
        int EJag(Vector2 A, Vector2 B, int seg, float amp, int salt)
        {
            Vector2 d = (B - A).normalized, perp = new Vector2(-d.y, d.x);
            int sl = Mathf.FloorToInt(_t * 28f);
            for (int i = 0; i <= seg; i++)
            {
                float s = i / (float)seg;
                float off = (i == 0 || i == seg) ? 0f : (Rnd(i, salt + sl) - 0.5f) * 2f * amp;
                Vector2 p = Vector2.Lerp(A, B, s) + perp * off;
                _buf[i] = new Vector3(p.x, p.y, 0f);
            }

            return seg + 1;
        }

        // Projectile flight helper: head x for progress p (0..1) from start.
        float EHead(float start, float dist, float p)
        {
            return start + dist * (p * 0.7f + p * p * 0.3f);
        }

        void AnimEFireBall()
        {
            Vector2 c = sweepCenter;
            float dist = qiTravel * 1.4f;
            float p = Mathf.Clamp01(_t / 0.5f);
            float a = WFade(0.85f, 0.3f);
            float hx = EHead(c.x + 0.5f, dist, p), hy = c.y + 0.05f;
            if (p < 1f)
            {
                int n = Seg(hx - 2.2f, hy, hx, hy, 14);
                DrawBrush(n, 0.5f, a * 0.8f);
                for (int i = 0; i < 4; i++)
                {
                    float oy = (i - 1.5f) * 0.22f;
                    EFlame(hx - 0.35f - 0.3f * i, hy + oy, 1.0f - 0.12f * i, 180f + oy * 25f, a * 0.9f, i);
                }

                DrawProp(Shape.Orb, hx, hy, 0f, 0.8f, a);
                EFlame(hx + 0.05f, hy, 0.7f, 0f, a, 9);
            }
            else
            {
                float k = (_t - 0.5f) / 0.5f;
                float xe = c.x + 0.5f + dist;
                if (Once(1)) Fire();
                for (int i = 0; i < 8; i++)
                {
                    float ang = i * 45f + 20f;
                    Vector2 d = WDir(ang);
                    float e = EaseOut(k);
                    EFlame(xe + d.x * 0.9f * e, hy + d.y * 0.9f * e, 0.9f + 0.4f * e, ang, a * (1f - k), i + 3);
                }

                int rn = ArcPts(xe, hy, 1.4f * EaseOut(k), 1.4f * EaseOut(k), 0f, 360f, 44);
                DrawBrush(rn, 0.2f * (1f - k), a * (1f - k));
                WFlash(xe, hy, k, 1f, 1.4f);
            }
        }

        void AnimEFlamethrower()
        {
            Vector2 c = sweepCenter;
            Vector2 h = c + new Vector2(0.5f, 0.0f);
            float on = Mathf.Clamp01((_t - 0.04f) / 0.12f) * (1f - Mathf.Clamp01((_t - 0.65f) / 0.25f));
            if (on <= 0f) return;
            int n = Seg(h.x, h.y, h.x + 4.0f * on, h.y, 14);
            DrawBrush(n, 0.9f, 0.35f * on);
            for (int i = 9; i >= 0; i--)
            {
                float d = 0.3f + 0.4f * i;
                float x = h.x + d * (0.6f + 0.4f * on), y = h.y + 0.08f * Mathf.Sin(_t * 12f + i);
                EFlame(x, y, 0.55f + 0.14f * i, 0f + 6f * Mathf.Sin(_t * 9f + i), on * (1f - i * 0.04f), i);
            }

            if (Once(1)) Fire();
            WFlash(h.x + 3.6f, h.y, (_t - 0.1f) / 0.5f, on, 1.0f);
        }

        void AnimEIceSpike()
        {
            Vector2 c = sweepCenter;
            float a = WFade(0.62f, 0.28f);
            int N = 5;
            if (Once(1)) Fire();
            for (int i = 0; i < N; i++)
            {
                float ts = 0.06f * i;
                float tt = _t - ts;
                if (tt < 0f) continue;
                float len = 0.9f + 0.18f * i;
                float x = c.x + 0.7f + 0.62f * i;
                float p = Mathf.Clamp01(tt / 0.16f);
                float e = 1f - Mathf.Pow(1f - p, 4f);
                DrawStanding(Shape.Ice, x, groundY - len + e * len * 0.97f, len, a);
                int rn = Ring(x, groundY, 0.45f * EaseOut(tt / 0.3f), 0.07f);
                Draw(Taper.Flat, rn, 0.03f * thickness, primary, (1f - Mathf.Clamp01(tt / 0.4f)) * a);
            }

            int bn = Seg(c.x + 0.4f, groundY + 0.03f, c.x + 0.7f + 0.62f * 4f + 0.4f, groundY + 0.03f, 16);
            DrawBrush(bn, 0.12f, a * 0.8f);
        }

        void AnimEFreezeBurst()
        {
            Vector2 c = sweepCenter;
            float a = WFade(0.7f, 0.3f);
            if (Once(1)) Fire();
            float[] xo = { -1.3f, -0.65f, 0f, 0.65f, 1.3f };
            float[] sz = { 1.1f, 1.45f, 1.9f, 1.45f, 1.1f };
            for (int i = 0; i < 5; i++)
            {
                float tt = _t - 0.04f * Mathf.Abs(i - 2);
                if (tt < 0f) continue;
                float p = Mathf.Clamp01(tt / 0.15f);
                float e = 1f - Mathf.Pow(1f - p, 4f);
                DrawStanding(Shape.Ice, c.x + xo[i], groundY - sz[i] + e * sz[i] * 0.97f, sz[i], a);
            }

            for (int i = 0; i < 10; i++)
            {
                float ang = i * 36f + 18f;
                Vector2 d = WDir(ang);
                float it = _t;
                float sp = 2.2f + 1.4f * Rnd(i, 1);
                Vector2 p = new Vector2(c.x, c.y) + d * sp * it + new Vector2(0f, -1.2f * it * it);
                DrawProp(Shape.Shard, p.x, p.y, ang + it * 200f, 0.3f + 0.25f * Rnd(i, 2), a * (1f - it * 0.5f));
            }

            DrawProp(Shape.Star5, c.x, c.y, 90f + _t * 120f, 1.6f * EaseOut(_t / 0.2f), a * 0.8f * (1f - _t));
            int rn = ArcPts(c.x, groundY, 3.0f * EaseOut(_t / 0.5f), 0.55f * EaseOut(_t / 0.5f), 0f, 360f, 40);
            DrawBrush(rn, 0.12f, a * (1f - Mathf.Clamp01(_t / 0.7f)));
            WFlash(c.x, c.y, _t / 0.4f, 1f, 1.6f);
        }

        void AnimELightningChain()
        {
            Vector2 c = sweepCenter;
            Vector2[] P = { c + new Vector2(0.6f, 0.1f), c + new Vector2(1.7f, 0.5f), c + new Vector2(3.0f, -0.2f), c + new Vector2(4.3f, 0.35f) };
            float a = WFade(0.5f, 0.2f);
            for (int s = 0; s < 3; s++)
            {
                float k = Mathf.Clamp01((_t - 0.07f * s) / 0.08f);
                if (k <= 0f) continue;
                Vector2 B = Vector2.Lerp(P[s], P[s + 1], k);
                int n = EJag(P[s], B, 9, 0.28f, s * 5);
                DrawBrush(n, 0.34f, a);
                n = EJag(P[s], B, 7, 0.4f, s * 5 + 40);
                DrawBrush(n, 0.12f, a * 0.8f);
                if (k >= 1f)
                {
                    DrawProp(Shape.Star5, P[s + 1].x, P[s + 1].y, 90f + _t * 400f, 0.55f, a);
                    WFlash(P[s + 1].x, P[s + 1].y, (_t - 0.07f * s - 0.08f) / 0.25f, a, 0.8f);
                }
            }

            if (_t >= 0.08f && Once(1)) Fire();
        }

        void AnimELightningStrike()
        {
            Vector2 c = sweepCenter;
            float ix = c.x + 1.6f;
            float top = c.y + 4.6f;
            float len = top - groundY;
            int fl = Mathf.FloorToInt(_t * 24f);
            float flick = (fl % 3 == 1) ? 0.3f : 1f;
            float a = WFade(0.5f, 0.25f) * flick;
            if (_t >= 0.06f)
            {
                DrawProp(Shape.Bolt, ix, (top + groundY) * 0.5f, 90f, len, a);
                for (int s = -1; s <= 1; s += 2)
                {
                    int n = EJag(new Vector2(ix + 0.1f * s, groundY + 1.8f), new Vector2(ix + s * 1.3f, groundY + 0.05f), 7, 0.22f, s + 8);
                    DrawBrush(n, 0.12f, a * 0.9f);
                }
            }
            else
            {
                int n = Seg(ix - 0.05f, groundY + 0.02f, ix + 0.05f, groundY + 0.02f, 2);
                DrawBrush(n, 0.12f, 0.6f);
            }

            if (_t >= 0.06f && Once(1)) Fire();
            WGroundHit(ix, (_t - 0.06f) / 0.5f, a, 1.8f);
            WFlash(ix, groundY + 0.1f, (_t - 0.06f) / 0.3f, 1f, 1.5f);
        }

        void AnimEWindBlade()
        {
            Vector2 c = sweepCenter;
            float dist = qiTravel * 1.4f;
            float f = Mathf.Clamp01(_t / 0.8f);
            float a = WFade(0.85f, 0.3f);
            float cx = c.x + 0.8f + dist * (f * 0.6f + f * f * 0.4f);
            for (int i = 0; i < 3; i++)
            {
                float oy = (i - 1) * 0.55f;
                WQiCrescent(cx - 0.5f * i, c.y + oy * 0.5f, 0.55f + 0.2f * (1 - Mathf.Abs(i - 1)), 0.32f, a * (1f - 0.2f * Mathf.Abs(i - 1)));
            }

            for (int k = 0; k < 6; k++)
            {
                float y = c.y + (k - 2.5f) * 0.28f;
                int n = WaveDir(cx - 2.8f, y, 0f, 2.6f, 20, 0.06f, 3.0f, -_t * 20f + k);
                DrawBrush(n, 0.05f, a * 0.55f);
            }

            if (f >= 1f && Once(1)) Fire();
            WFlash(cx + 0.3f, c.y, (_t - 0.8f) / 0.3f, 1f, 1.0f);
        }

        void AnimETornado()
        {
            Vector2 c = sweepCenter;
            float x = c.x + 1.5f;
            float g = Mathf.Clamp01(_t / 0.3f) * WFade(1.4f, 0.4f);
            if (Once(1)) Fire();
            float spin = _t * 540f;
            for (int i = 0; i < 8; i++)
            {
                float y = groundY + 0.15f + i * 0.4f;
                float sw = 0.1f * Mathf.Sin(_t * 3f + i * 0.5f);
                float rx = 0.2f + 0.11f * i;
                int n = ArcPts(x + sw, y, rx, rx * 0.22f, spin * (1f + 0.05f * i) + i * 60f, spin * (1f + 0.05f * i) + i * 60f + 220f, 20);
                DrawBrush(n, 0.14f, g * 0.95f);
            }

            for (int i = 0; i < 6; i++)
            {
                float f = Mathf.Repeat(_t * 0.7f + i / 6f, 1f);
                float y = groundY + 0.2f + f * 3.0f;
                float rx = 0.3f + 0.4f * f;
                float ang = (spin * 1.2f + i * 60f) * Mathf.Deg2Rad;
                DrawProp(Shape.Rock, x + Mathf.Cos(ang) * rx, y + Mathf.Sin(ang) * rx * 0.22f, spin + i * 50f, 0.22f, g * Mathf.Sin(f * Mathf.PI));
            }

            int rn = ArcPts(x, groundY, 1.0f, 0.2f, 0f, 360f, 36);
            DrawBrush(rn, 0.1f, g * 0.7f);
        }

        void AnimEWaterBall()
        {
            Vector2 c = sweepCenter;
            float dist = qiTravel * 1.4f;
            float p = Mathf.Clamp01(_t / 0.5f);
            float a = WFade(0.9f, 0.25f);
            float hy = c.y + 0.05f;
            if (p < 1f)
            {
                float hx = EHead(c.x + 0.5f, dist, p);
                for (int r = 0; r < 2; r++)
                {
                    int n = WaveDir(hx - 2.2f, hy + (r == 0 ? 0.08f : -0.08f), 0f, 2.1f, 24, 0.12f, 2.5f, -_t * 18f + r * 3.1f);
                    DrawBrush(n, 0.18f, a * 0.8f);
                }

                DrawProp(Shape.Orb, hx, hy, 0f, 0.85f, a);
                for (int i = 0; i < 4; i++)
                {
                    float ang = (_t * 380f + i * 90f) * Mathf.Deg2Rad;
                    DrawProp(Shape.Drop, hx + Mathf.Cos(ang) * 0.62f, hy + Mathf.Sin(ang) * 0.5f, Mathf.Rad2Deg * ang, 0.32f, a);
                }
            }
            else
            {
                float k = (_t - 0.5f) / 0.5f;
                float xe = c.x + 0.5f + dist;
                if (Once(1)) Fire();
                for (int i = 0; i < 10; i++)
                {
                    float ang = 20f + i * 14f;
                    Vector2 d = WDir(ang);
                    float sp = 2.0f + 1.5f * Rnd(i, 1);
                    Vector2 q = new Vector2(xe, hy) + d * sp * k * 0.6f + new Vector2((Rnd(i, 2) - 0.5f) * 0.8f * k, -2.2f * k * k);
                    DrawProp(Shape.Drop, q.x, q.y, Mathf.Atan2(d.y * sp * 0.6f - 4.4f * k, d.x * sp * 0.6f) * Mathf.Rad2Deg, 0.3f + 0.15f * Rnd(i, 3), a * (1f - k * 0.7f));
                }

                DrawProp(Shape.Orb, xe, hy, 0f, 0.85f + 1.0f * k, a * (1f - k));
                int rn = ArcPts(xe, hy, 1.3f * EaseOut(k), 1.3f * EaseOut(k), 0f, 360f, 40);
                DrawBrush(rn, 0.18f * (1f - k), a * (1f - k));
                WFlash(xe, hy, k, 1f, 1.1f);
            }
        }

        void AnimEWaterColumn()
        {
            Vector2 c = sweepCenter;
            float ix = c.x + 1.4f;
            float e = 1f - Mathf.Pow(1f - Mathf.Clamp01(_t / 0.18f), 4f);
            float a = WFade(0.8f, 0.4f);
            if (Once(1)) Fire();
            float[] xo = { -0.22f, 0.0f, 0.22f };
            float[] wd = { 0.4f, 0.75f, 0.4f };
            for (int i = 0; i < 3; i++)
            {
                int n = WaveDir(ix + xo[i], groundY, 90f, 2.9f * e, 26, 0.1f, 2.4f, -_t * 14f + i * 2f);
                DrawBrush(n, wd[i] * (1f - 0.3f * Mathf.Clamp01((_t - 0.4f) / 0.6f)), a * 0.85f);
            }

            DrawProp(Shape.Orb, ix, groundY + 2.9f * e, 0f, 0.9f, a * e);
            for (int i = 0; i < 9; i++)
            {
                float side = (i % 2 == 0 ? 1f : -1f);
                float sp = 0.7f + 0.9f * Rnd(i, 1);
                float tt = Mathf.Max(0f, _t - 0.08f - 0.03f * i);
                Vector2 q = new Vector2(ix + side * sp * tt * 1.6f, groundY + 1.2f + 2.0f * Rnd(i, 2) * Mathf.Min(1f, tt * 4f) - 3.0f * tt * tt);
                DrawProp(Shape.Drop, q.x, q.y, 90f + side * 25f * tt * 4f, 0.28f + 0.12f * Rnd(i, 3), a * Mathf.Clamp01(tt * 8f));
            }

            int rn = ArcPts(ix, groundY, 1.6f * EaseOut(_t / 0.5f), 0.3f * EaseOut(_t / 0.5f), 0f, 360f, 36);
            DrawBrush(rn, 0.12f, a * (1f - Mathf.Clamp01(_t / 0.9f)));
        }

        void AnimERockSpike()
        {
            Vector2 c = sweepCenter;
            float a = WFade(0.62f, 0.28f);
            if (Once(1)) Fire();
            for (int i = 0; i < 5; i++)
            {
                float tt = _t - 0.06f * i;
                if (tt < 0f) continue;
                float len = 1.0f + 0.22f * i;
                float x = c.x + 0.8f + 0.62f * i;
                float e = 1f - Mathf.Pow(1f - Mathf.Clamp01(tt / 0.16f), 4f);
                DrawStanding(Shape.Shard, x, groundY - len + e * len * 0.95f, len, a);
                float fl = Mathf.Clamp01(tt / 0.5f);
                for (int r = -1; r <= 1; r += 2)
                {
                    Vector2 q = new Vector2(x + r * (0.25f + 0.7f * fl), groundY + 0.2f + 0.9f * Mathf.Sin(fl * Mathf.PI) * (0.6f + 0.4f * Rnd(i, r + 3)));
                    DrawProp(Shape.Rock, q.x, q.y, tt * 300f * r, 0.26f, a * (1f - fl * 0.7f));
                }
            }

            int bn = Seg(c.x + 0.5f, groundY + 0.03f, c.x + 3.6f, groundY + 0.03f, 16);
            DrawBrush(bn, 0.14f, a * 0.8f);
        }

        void AnimEFallingRocks()
        {
            Vector2 c = sweepCenter;
            float a = WFade(0.95f, 0.25f);
            for (int i = 0; i < 6; i++)
            {
                float ts = 0.09f * i;
                float tt = _t - ts;
                if (tt < 0f) continue;
                float x = c.x + 0.9f + 0.45f * i + (Rnd(i, 1) - 0.5f) * 0.3f;
                float sz = 0.55f + 0.35f * Rnd(i, 2);
                float fall = 0.3f;
                float landY = groundY + sz * 0.4f;
                float y = tt < fall ? Mathf.Lerp(c.y + 4.5f, landY, tt * tt / (fall * fall)) : landY + 0.3f * Mathf.Abs(Mathf.Sin((tt - fall) * 14f)) * Mathf.Exp(-(tt - fall) * 8f);
                float rot = tt * (180f + 120f * Rnd(i, 3)) * (i % 2 == 0 ? 1f : -1f);
                if (tt < fall)
                {
                    int n = Seg(x, y + 1.4f, x, y, 4);
                    DrawBrush(n, 0.1f, a * 0.6f);
                }

                DrawProp(Shape.Rock, x, y, rot * (tt < fall ? 1f : 0.2f), sz, a);
                if (tt >= fall)
                {
                    if (Once(1 << (i + 2))) Fire();
                    float k = Mathf.Clamp01((tt - fall) / 0.45f);
                    int rn = ArcPts(x, groundY, 0.9f * EaseOut(k), 0.15f * EaseOut(k), 0f, 180f, 14);
                    DrawBrush(rn, 0.1f * (1f - k), a * (1f - k));
                }
            }
        }

        void AnimEPoisonMist()
        {
            Vector2 c = sweepCenter;
            float x = c.x + 1.2f;
            float g = Mathf.Clamp01(_t / 0.4f) * WFade(1.5f, 0.5f);
            if (Once(1)) Fire();
            for (int i = 0; i < 7; i++)
            {
                float y = groundY + 0.25f + i * 0.2f;
                float drift = Mathf.Sin(_t * 0.9f + i * 1.3f) * 0.5f;
                int n = ArcPts(x + drift * 0.6f, y, 1.5f - 0.1f * i, 0.18f + 0.03f * i, 180f + 20f * i, 360f - 20f * i, 20);
                DrawBrush(n, 0.34f, g * 0.45f);
            }

            for (int i = 0; i < 4; i++)
            {
                float f = Mathf.Repeat(_t * 0.4f + i / 4f, 1f);
                float sx = x + (i - 1.5f) * 0.55f + 0.2f * Mathf.Sin(_t * 2f + i);
                DrawProp(Shape.Skull, sx, groundY + 0.3f + f * 1.9f, 90f, 0.42f, g * Mathf.Sin(f * Mathf.PI) * 0.95f);
            }

            for (int i = 0; i < 8; i++)
            {
                float f = Mathf.Repeat(_t * 0.5f + i / 8f, 1f);
                float bx = x + (Rnd(i, 1) - 0.5f) * 2.2f;
                DrawProp(Shape.Orb, bx, groundY + 0.1f + f * 2.0f, 0f, 0.16f + 0.14f * Rnd(i, 2), g * Mathf.Sin(f * Mathf.PI) * 0.8f);
            }
        }

        void AnimEPoisonShot()
        {
            Vector2 c = sweepCenter;
            float dist = qiTravel * 1.4f;
            float p = Mathf.Clamp01(_t / 0.5f);
            float a = WFade(0.95f, 0.2f);
            float hy = c.y + 0.05f;
            if (p < 1f)
            {
                float hx = EHead(c.x + 0.5f, dist, p);
                int n = Seg(hx - 2.0f, hy, hx, hy, 12);
                DrawBrush(n, 0.34f, a * 0.8f);
                DrawProp(Shape.Orb, hx, hy, 0f, 0.8f, a);
                DrawProp(Shape.Skull, hx, hy, 0f, 0.38f, a);
                for (int i = 0; i < 3; i++)
                {
                    float f = Mathf.Repeat(_t * 3f + i / 3f, 1f);
                    DrawProp(Shape.Drop, hx - 0.3f - 0.3f * i, hy - f * 0.5f, 270f, 0.22f, a * (1f - f));
                }
            }
            else
            {
                float k = (_t - 0.5f) / 0.7f;
                float xe = c.x + 0.5f + dist;
                if (Once(1)) Fire();
                for (int i = 0; i < 8; i++)
                {
                    float ang = 25f + i * 18f;
                    Vector2 d = WDir(ang);
                    Vector2 q = new Vector2(xe, hy) + d * 1.4f * Mathf.Min(1f, k * 3f) + new Vector2(0f, -1.6f * k * k);
                    DrawProp(Shape.Drop, q.x, q.y, ang, 0.3f, a * (1f - k));
                }

                int rn = ArcPts(xe, groundY, 1.3f * EaseOut(k), 0.22f * EaseOut(k), 0f, 360f, 32);
                DrawBrush(rn, 0.16f, a * (1f - k));
                WFlash(xe, hy, k * 1.4f, 1f, 1.0f);
            }
        }

        void AnimEHolyLight()
        {
            Vector2 c = sweepCenter;
            float x = c.x;
            float g = Mathf.Clamp01(_t / 0.25f) * WFade(1.0f, 0.4f);
            if (Once(1)) Fire();
            float[] wd = { 1.1f, 0.6f, 0.22f };
            for (int i = 0; i < 3; i++)
            {
                int n = Seg(x, groundY + 4.8f, x, groundY, 10);
                DrawBrush(n, wd[i], g * (0.28f + 0.3f * i));
            }

            DrawProp(Shape.Feather, x - 0.55f, c.y + 0.4f, 55f, 1.7f * EaseOut(_t / 0.4f), g);
            DrawProp(Shape.Feather, x + 0.55f, c.y + 0.4f, 125f, 1.7f * EaseOut(_t / 0.4f), g);
            for (int i = 0; i < 6; i++)
            {
                float f = Mathf.Repeat(_t * 0.6f + i / 6f, 1f);
                float fx = x + (Rnd(i, 1) - 0.5f) * 1.5f + 0.25f * Mathf.Sin(_t * 3f + i * 2f);
                DrawProp(Shape.Feather, fx, groundY + 4.0f - f * 3.6f, 270f + 25f * Mathf.Sin(_t * 3f + i), 0.5f, g * Mathf.Sin(f * Mathf.PI));
            }

            DrawProp(Shape.Star5, x, c.y + 1.7f, 90f + _t * 80f, 0.55f * EaseOut(_t / 0.3f), g);
            int rn = ArcPts(x, groundY, 1.3f * EaseOut(_t / 0.5f), 0.25f * EaseOut(_t / 0.5f), 0f, 360f, 36);
            DrawBrush(rn, 0.12f, g);
        }

        void AnimEShadowOrb()
        {
            Vector2 c = sweepCenter;
            Vector2 h = c + new Vector2(0.6f, 0.2f);
            float a = WFade(1.35f, 0.25f);
            float dist = qiTravel * 1.1f;
            float ch = Mathf.Clamp01(_t / 0.45f);
            float fl = Mathf.Clamp01((_t - 0.5f) / 0.55f);
            float ox = h.x + dist * fl * fl;
            float oy = h.y - 0.2f * fl;
            float sz = 0.25f + 0.65f * EaseOut(ch);
            if (fl < 1f)
            {
                for (int i = 0; i < 3; i++)
                {
                    int n = ArcPts(ox, oy, sz * (0.75f + 0.12f * i), sz * (0.75f + 0.12f * i), _t * (300f + 60f * i) + i * 110f, _t * (300f + 60f * i) + i * 110f + 130f, 14);
                    DrawBrush(n, 0.09f, a);
                }

                DrawProp(Shape.DarkOrb, ox, oy, _t * 120f, sz * 1.25f, a);
                if (fl > 0f)
                {
                    for (int r = 0; r < 2; r++)
                    {
                        int n = WaveDir(ox - 2.2f, oy + (r - 0.5f) * 0.2f, 0f, 2.1f, 24, 0.14f, 2.2f, -_t * 16f + r * 2.5f);
                        DrawBrush(n, 0.16f, a * 0.7f);
                    }
                }
            }
            else
            {
                float k = (_t - 1.05f) / 0.55f;
                if (Once(1)) Fire();
                DrawProp(Shape.DarkOrb, ox, oy, _t * 120f, (0.9f + 1.4f * EaseOut(k)) * 1.25f, a * (1f - k));
                for (int i = 0; i < 8; i++)
                {
                    float ang = i * 45f + 15f;
                    Vector2 d = WDir(ang);
                    int n = Seg(ox + d.x * 0.5f, oy + d.y * 0.5f, ox + d.x * (0.5f + 1.6f * EaseOut(k)), oy + d.y * (0.5f + 1.6f * EaseOut(k)), 6);
                    DrawBrush(n, 0.16f, a * (1f - k));
                }

                int rn = ArcPts(ox, oy, 1.8f * EaseOut(k), 1.8f * EaseOut(k), 0f, 360f, 40);
                DrawBrush(rn, 0.2f * (1f - k), a * (1f - k));
            }
        }
    }
}
