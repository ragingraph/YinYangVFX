using UnityEngine;

namespace YinYangVFX
{
    // Concrete defence effects: solid hex shield panels, yin-yang disc, shards, stars, crosses and ink-brush contours.
    // Origin = character body centre.
    public partial class WeaponEffect
    {
        // Closed brush contour of an ellipse (ghost outline / aura).
        void DContour(float cx, float cy, float rx, float ry, float w, float a)
        {
            int n = ArcPts(cx, cy, rx, ry, 90f, 450f, 44);
            DrawBrush(n, w, a);
        }

        // Hex panels spread over an ellipse from startAng to endAng (full = true for a closed ring).
        void DPanels(Vector2 c, float rx, float ry, int count, float phase, float startAng, float endAng, bool full, float size, float baseA, float shimmer)
        {
            for (int i = 0; i < count; i++)
            {
                float t = full ? i / (float)count : (count > 1 ? i / (float)(count - 1) : 0.5f);
                float ang = Mathf.Lerp(startAng, endAng, t) + phase;
                float rad = ang * Mathf.Deg2Rad;
                float al = baseA + shimmer * Mathf.Sin(_t * 3f + i * 1.3f);
                DrawProp(Shape.Hex, c.x + Mathf.Cos(rad) * rx, c.y + Mathf.Sin(rad) * ry, ang, size, al);
            }
        }

        void AnimDShieldOrb()
        {
            Vector2 c = sweepCenter;
            float pu = 1f + 0.12f * _pulse;
            float R = 0.85f * pu, rot = _t * 35f;
            for (int i = 0; i < 3; i++)
            {
                int n = ArcPts(c.x, c.y, R, R * 1.15f, rot + i * 120f, rot + i * 120f + 95f, 24);
                DrawBrush(n, 0.15f, 0.95f);
            }

            DPanels(c, R, R * 1.15f, 8, -rot * 0.6f, 0f, 360f, true, 0.5f * pu, 0.55f, 0.12f);
            int rn = Ring(c.x, c.y, R * 0.97f, R * 1.12f);
            Draw(Taper.Flat, rn, 0.03f * thickness, primary, 0.5f + 0.4f * _pulse);
        }

        void AnimDShieldArc()
        {
            Vector2 c = sweepCenter;
            float pu = 1f + 0.1f * _pulse;
            float rx = 0.95f * pu, ry = 1.05f * pu;
            int n = ArcPts(c.x, c.y, rx, ry, -62f, 62f, 26);
            DrawBrush(n, 0.22f, 0.95f);
            DPanels(c, rx * 0.98f, ry * 0.98f, 5, 0f, -50f, 50f, false, 0.55f * pu, 0.55f, 0.15f);
            n = ArcPts(c.x, c.y, rx * 0.9f, ry * 0.9f, -50f, 50f, 20);
            DrawBrush(n, 0.07f, 0.6f);
            int rn = Ring(c.x, c.y, rx, ry);
            Draw(Taper.Flat, rn, 0.02f * thickness, primary, 0.2f + 0.5f * _pulse);
        }

        void AnimDYinYangShield()
        {
            Vector2 c = sweepCenter;
            float pu = 1f + 0.1f * _pulse;
            DrawProp(Shape.Taiji, c.x, c.y, 90f + _t * 45f, 1.9f * pu, 0.92f);
            int n = ArcPts(c.x, c.y, 1.2f * pu, 1.2f * pu, -_t * 70f, -_t * 70f + 120f, 22);
            DrawBrush(n, 0.12f, 0.9f);
            n = ArcPts(c.x, c.y, 1.2f * pu, 1.2f * pu, -_t * 70f + 180f, -_t * 70f + 300f, 22);
            DrawBrush(n, 0.12f, 0.9f);
            int rn = Ring(c.x, c.y, 1.0f * pu, 1.0f * pu);
            Draw(Taper.Flat, rn, 0.03f * thickness, primary, 0.5f + 0.5f * _pulse);
        }

        void AnimDBlockSparks()
        {
            Vector2 c = sweepCenter;
            Vector2 im = new Vector2(c.x + 0.75f, c.y + 0.05f);
            float a = WFade(0.25f, 0.3f);
            float k = WSw(0f, 0.1f);
            for (int s = 0; s < 2; s++)
            {
                float side = s == 0 ? 1f : -1f;
                int n = Seg(im.x - 0.45f * k, im.y + 0.45f * k * side, im.x + 0.45f * k, im.y - 0.45f * k * side, 8);
                DrawBrush(n, 0.2f, a);
            }

            DrawProp(Shape.Hex, im.x, im.y, 0f, 0.9f * (0.6f + 0.4f * k), WFade(0.05f, 0.3f) * 0.9f);
            if (Once(1)) Fire();
            WFlash(im.x, im.y, _t / 0.45f, 1f, 1.0f);
        }

        void AnimDPerfectBlock()
        {
            Vector2 c = sweepCenter;
            Vector2 im = new Vector2(c.x + 0.75f, c.y + 0.05f);
            float e = EaseOut(_t / 0.25f);
            float a = WFade(0.35f, 0.4f);
            DrawProp(Shape.Taiji, im.x, im.y, 90f + _t * 160f, 0.4f + 1.2f * e, a * 0.9f);
            DrawProp(Shape.Star5, im.x, im.y, 90f - _t * 220f, 0.5f + 0.9f * e, a);
            float r = 1.7f * EaseOut(_t / 0.55f);
            int n = ArcPts(im.x, im.y, r, r, 0f, 360f, 44);
            DrawBrush(n, 0.2f * (1f - _t / duration), a);
            if (Once(1)) Fire();
            WFlash(im.x, im.y, _t / 0.55f, 1f, 1.5f);
        }

        void AnimDShieldRipple()
        {
            Vector2 c = sweepCenter;
            float rx = 0.95f, ry = 1.05f;
            Vector2 im = new Vector2(c.x + rx, c.y + 0.0f);
            float a = WFade(0.6f, 0.35f);
            int n = ArcPts(c.x, c.y, rx, ry, -70f, 70f, 28);
            DrawBrush(n, 0.2f, a);
            for (int i = 0; i < 5; i++)
            {
                float ang = Mathf.Lerp(-52f, 52f, i / 4f);
                float rad = ang * Mathf.Deg2Rad;
                Vector2 p = new Vector2(c.x + Mathf.Cos(rad) * rx * 0.98f, c.y + Mathf.Sin(rad) * ry * 0.98f);
                float dist = Vector2.Distance(p, im);
                float lit = Mathf.Clamp01(1f - Mathf.Abs(_t - 0.05f - dist * 0.18f) / 0.18f);
                DrawProp(Shape.Hex, p.x, p.y, ang, 0.55f, a * (0.3f + 0.7f * lit));
            }

            for (int j = 0; j < 3; j++)
            {
                float d = Mathf.Clamp01((_t - 0.07f * j) / 0.6f);
                int rn = Ring(im.x, im.y, 1.3f * EaseOut(d), 1.3f * EaseOut(d));
                Draw(Taper.Flat, rn, 0.05f * (1f - d) * thickness, primary, (1f - d) * a);
            }

            if (Once(1)) Fire();
        }

        void AnimDReflectWave()
        {
            Vector2 c = sweepCenter;
            float a = WFade(0.7f, 0.2f);
            DrawProp(Shape.Hex, c.x + 0.9f, c.y, 0f, 1.0f, WFade(0.05f, 0.25f) * 0.9f);
            if (_t >= 0.05f)
            {
                float f = Mathf.Clamp01((_t - 0.05f) / 0.7f);
                float cx = c.x + 1.0f + qiTravel * 1.3f * (f * 0.5f + f * f * 0.5f);
                WQiCrescent(cx, c.y, 0.7f + 0.5f * f, 0.5f, a);
                WQiCrescent(cx - 0.8f, c.y, 0.45f + 0.3f * f, 0.3f, a * 0.6f);
                if (f >= 1f && Once(1)) Fire();
                WFlash(cx + 0.3f, c.y, (_t - 0.75f) / 0.15f, 1f, 1.0f);
            }
        }

        void AnimDProjectileReturn()
        {
            Vector2 c = sweepCenter;
            float a = WFade(0.75f, 0.15f);
            float hx = c.x + 1.05f;
            float x, dir;
            if (_t < 0.28f)
            {
                x = Mathf.Lerp(c.x + 3.8f, hx, Mathf.Clamp01(_t / 0.28f));
                dir = 180f;
                int n = Seg(x, c.y, x + 2.2f, c.y, 12);
                DrawBrush(n, 0.18f, a);
            }
            else if (_t < 0.38f)
            {
                x = hx;
                dir = Mathf.Lerp(180f, 0f, (_t - 0.28f) / 0.1f);
            }
            else
            {
                float f = Mathf.Clamp01((_t - 0.38f) / 0.45f);
                x = Mathf.Lerp(hx, c.x + 4.6f, f * f);
                dir = 0f;
                int n = Seg(x - 2.2f, c.y, x, c.y, 12);
                DrawBrush(n, 0.22f, a);
            }

            DrawProp(Shape.Arrow, x, c.y, dir, 1.4f, a);
            DrawProp(Shape.Hex, c.x + 0.95f, c.y, 0f, 0.9f, WFade(0.28f, 0.25f) * (_t >= 0.25f ? 0.9f : 0f));
            if (_t >= 0.38f && Once(1)) Fire();
            WFlash(c.x + 1.0f, c.y, (_t - 0.28f) / 0.3f, 1f, 0.9f);
        }

        void AnimDDodge()
        {
            Vector2 c = sweepCenter;
            float e = EaseOut(_t / 0.3f);
            float hx = c.x - 1.4f * e;
            float fade = WFade(0.3f, 0.4f);
            for (int i = 4; i >= 1; i--)
            {
                float al = (1f - i / 5f) * fade;
                DContour(hx + 0.32f * i, c.y, BodyRx, BodyRy, 0.09f, al);
            }

            for (int j = -1; j <= 1; j++)
            {
                int n = Seg(hx + 1.7f * e, c.y + j * 0.32f, hx + 0.2f, c.y + j * 0.32f, 12);
                DrawBrush(n, 0.12f, fade * 0.8f);
            }

            DContour(hx, c.y, BodyRx, BodyRy, 0.06f, 0.5f * fade);
            if (Once(1)) Fire();
        }

        void AnimDRoll()
        {
            Vector2 c = sweepCenter;
            float p = Mathf.Clamp01(_t / 0.6f);
            float bx = c.x + 2.0f * p, by = groundY + 0.38f;
            float fade = WFade(0.6f, 0.3f);
            float spin = _t * 900f;
            if (_t > 0.02f)
            {
                int n = Seg(c.x, groundY + 0.04f, bx, groundY + 0.04f, 20);
                DrawBrush(n, 0.22f, fade * 0.8f);
            }

            for (int i = 0; i < 3; i++)
            {
                int n = ArcPts(bx, by, 0.38f, 0.38f, spin + i * 120f, spin + i * 120f + 85f, 14);
                DrawBrush(n, 0.12f, fade);
            }

            for (int i = 1; i <= 3; i++)
            {
                float px = bx - 0.45f * i;
                if (px < c.x) continue;
                int n = ArcPts(px, groundY, 0.22f + 0.08f * i, 0.18f + 0.06f * i, 0f, 180f, 12);
                DrawBrush(n, 0.07f, fade * (1f - i * 0.25f));
            }

            if (Once(1)) Fire();
        }

        void AnimDBlink()
        {
            Vector2 c = sweepCenter;
            Vector2 E = c + new Vector2(2.6f, 0f);
            float fade = WFade(0.35f, 0.3f);
            float a0 = WFade(0f, 0.3f), a1 = Mathf.Clamp01((_t - 0.12f) / 0.15f) * fade;
            DContour(c.x, c.y, BodyRx, BodyRy, 0.1f, a0);
            DContour(E.x, E.y, BodyRx, BodyRy, 0.1f, a1);
            float st = Mathf.Clamp01(1f - Mathf.Abs(_t - 0.12f) / 0.2f);
            int n = Seg(c.x, c.y, E.x, E.y, 16);
            DrawBrush(n, 0.45f, st);
            DrawProp(Shape.Star5, c.x, c.y, 90f + _t * 300f, 0.9f * a0, a0);
            DrawProp(Shape.Star5, E.x, E.y, 90f - _t * 300f, 0.9f * Mathf.Clamp01((_t - 0.12f) / 0.15f), a1);
            if (_t >= 0.12f && Once(1)) Fire();
            WFlash(E.x, E.y, (_t - 0.12f) / 0.35f, 1f, 1.0f);
        }

        void AnimDIronBody()
        {
            Vector2 c = sweepCenter;
            float pu = 1f + 0.08f * _pulse;
            DContour(c.x, c.y, (BodyRx + 0.12f) * pu, (BodyRy + 0.12f) * pu, 0.13f, 0.9f);
            DPanels(c, (BodyRx + 0.14f) * pu, (BodyRy + 0.14f) * pu, 10, 0f, 0f, 360f, true, 0.4f, 0.6f, 0.15f);
            float s = Mathf.Repeat(_t, 1.2f) / 1.2f;
            float gx = c.x - 0.7f + 1.4f * s, gy = c.y - 0.7f + 1.4f * s;
            float al = Mathf.Sin(s * Mathf.PI);
            int n = Seg(gx - 0.25f, gy - 0.25f, gx + 0.25f, gy + 0.25f, 6);
            DrawBrush(n, 0.16f, al);
        }

        void AnimDHealPillar()
        {
            Vector2 c = sweepCenter;
            float x = c.x;
            float a = WFade(0.9f, 0.5f);
            float e = EaseOut(_t / 0.35f);
            if (Once(1)) Fire();
            float[] xo = { -0.5f, 0.0f, 0.5f };
            float[] wd = { 0.3f, 0.6f, 0.3f };
            for (int i = 0; i < 3; i++)
            {
                int n = Seg(x + xo[i] * 0.5f, groundY, x + xo[i] * 0.5f, groundY + 2.6f * e, 10);
                DrawBrush(n, wd[i] * 0.7f, a * 0.8f);
            }

            float[] cx = { -0.3f, 0.15f, 0.35f };
            float[] sz = { 0.35f, 0.5f, 0.3f };
            for (int i = 0; i < 3; i++)
            {
                float f = Mathf.Clamp01((_t - 0.12f * i) / 1.0f);
                if (f <= 0f) continue;
                float y = Mathf.Lerp(c.y - 0.4f, c.y + 1.8f, EaseOut(f));
                DrawProp(Shape.Cross, x + cx[i], y, 90f, sz[i], Mathf.Sin(Mathf.Clamp01(f) * Mathf.PI) * a);
            }

            int rn = Ring(x, groundY, 0.9f * e, 0.18f * e);
            Draw(Taper.Flat, rn, 0.04f * thickness, primary, a);
        }

        void AnimDInvincible()
        {
            Vector2 c = sweepCenter;
            float pu = 1f + 0.08f * Mathf.Sin(_t * 4f);
            DContour(c.x, c.y, (BodyRx + 0.3f) * pu, (BodyRy + 0.3f) * pu, 0.16f, 0.95f);
            DContour(c.x, c.y, (BodyRx + 0.55f) * pu, (BodyRy + 0.55f) * pu, 0.06f, 0.55f);
            for (int i = 0; i < 4; i++)
            {
                float ang = (_t * 90f + i * 90f) * Mathf.Deg2Rad;
                DrawProp(Shape.Star5, c.x + Mathf.Cos(ang) * (BodyRx + 0.55f), c.y + Mathf.Sin(ang) * (BodyRy + 0.55f), 90f + _t * 200f, 0.3f, 0.95f);
            }

            for (int i = 0; i < 6; i++)
            {
                float f = Mathf.Repeat(_t * 0.9f + i / 6f, 1f);
                float x = c.x + (i % 2 == 0 ? -1f : 1f) * (BodyRx + 0.25f + 0.1f * (i / 2));
                int n = Seg(x, c.y - 0.6f + f * 1.2f, x, c.y - 0.6f + f * 1.2f + 0.5f, 6);
                DrawBrush(n, 0.07f, Mathf.Sin(f * Mathf.PI) * 0.85f);
            }
        }

        void AnimDShieldBreak()
        {
            Vector2 c = sweepCenter;
            float rx = 0.95f, ry = 1.05f;
            const float tb = 0.18f;
            if (_t < tb)
            {
                int n = ArcPts(c.x, c.y, rx, ry, -62f, 62f, 24);
                DrawBrush(n, 0.22f, 0.95f);
                DPanels(c, rx * 0.98f, ry * 0.98f, 5, 0f, -50f, 50f, false, 0.55f, 0.7f, 0f);
                for (int i = 0; i < 3; i++)
                {
                    float x0 = c.x + rx * (0.85f + 0.1f * i) * 0.9f;
                    int cn = Seg(x0, c.y + (i - 1) * 0.3f, x0 + 0.25f * (i % 2 + 1), c.y + (i - 1) * 0.3f + 0.4f * (i - 1), 3);
                    Draw(Taper.Head, cn, 0.05f * thickness, primary, 1f);
                }

                return;
            }

            float it = _t - tb;
            if (Once(1)) Fire();
            float a = WFade(0.7f, 0.3f);
            for (int i = 0; i < 14; i++)
            {
                float ang = Mathf.Lerp(-72f, 72f, i / 13f) + (Rnd(i, 1) - 0.5f) * 8f;
                Vector2 d = WDir(ang);
                float sp = 2.4f + 2.0f * Rnd(i, 2);
                Vector2 p0 = new Vector2(c.x + d.x * rx, c.y + d.y * ry);
                Vector2 p = p0 + d * sp * it + new Vector2(0f, -2.5f * it * it);
                float rot = ang + it * (260f + 400f * Rnd(i, 3)) * (Rnd(i, 4) > 0.5f ? 1f : -1f);
                DrawProp(Shape.Shard, p.x, p.y, rot, 0.3f + 0.3f * Rnd(i, 5), a * (1f - it * 0.6f));
            }

            float r = 0.95f + 1.4f * EaseOut(it / 0.4f);
            int rn = ArcPts(c.x, c.y, r, r * 1.1f, -75f, 75f, 28);
            DrawBrush(rn, 0.16f * (1f - it), a * (1f - Mathf.Clamp01(it / 0.5f)));
            WFlash(c.x + rx, c.y, it / 0.4f, 1f, 1.3f);
        }

        void AnimDStun()
        {
            Vector2 c = sweepCenter;
            float hy = c.y + BodyRy + 0.22f;
            int n = ArcPts(c.x, hy, 0.46f, 0.14f, 180f, 360f, 14);
            DrawBrush(n, 0.04f, 0.5f);
            n = ArcPts(c.x, hy, 0.46f, 0.14f, 0f, 180f, 14);
            DrawBrush(n, 0.04f, 0.5f);
            for (int pass = 0; pass < 2; pass++)
            {
                for (int i = 0; i < 3; i++)
                {
                    float ang = (_t * 220f + i * 120f) * Mathf.Deg2Rad;
                    float sn = Mathf.Sin(ang);
                    bool front = sn < 0f;
                    if ((pass == 0) == front) continue;
                    float sc = 0.26f + 0.06f * (front ? 1f : -0.4f);
                    DrawProp(Shape.Star5, c.x + Mathf.Cos(ang) * 0.46f, hy + sn * 0.14f, 90f + _t * 300f, sc, front ? 1f : 0.6f);
                }
            }
        }
    }
}
