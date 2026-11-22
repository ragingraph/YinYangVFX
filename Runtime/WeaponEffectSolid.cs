using UnityEngine;

namespace YinYangVFX
{
    // Concrete weapon effects: the real weapon (sword, spear, hammer ...) is drawn as a solid shape and
    // moved along its path, with solid ink brush strokes as the swing trail. Origin = character body centre.
    public partial class WeaponEffect
    {
        static Vector2 WDir(float deg)
        {
            float r = deg * Mathf.Deg2Rad;
            return new Vector2(Mathf.Cos(r), Mathf.Sin(r));
        }

        // Fast start, soft end progress 0..1 over [t0, t0+dur].
        float WSw(float t0, float dur)
        {
            float p = Mathf.Clamp01((_t - t0) / dur);
            return 1f - (1f - p) * (1f - p) * (1f - p);
        }

        float WFade(float start, float span)
        {
            return 1f - Mathf.Clamp01((_t - start) / span);
        }

        // Brush trail along a circle around pivot, ending at angHead, covering span degrees behind it (swing goes clockwise).
        void WTrail(Vector2 pivot, float r, float angHead, float span, float w, float a)
        {
            if (Mathf.Abs(span) < 4f) return;
            int n = ArcPts(pivot.x, pivot.y, r, r, angHead + span, angHead, 26);
            DrawBrush(n, w, a);
        }

        // Radial hit flash + ring. k = 0..1 progress.
        void WFlash(float x, float y, float k, float a, float size)
        {
            if (k <= 0f || k >= 1f) return;
            float e = EaseOut(k);
            for (int i = 0; i < 10; i++)
            {
                float ang = (i * 36f + _seed * 13f) * Mathf.Deg2Rad;
                float r0 = size * 0.22f * e, r1 = size * (0.55f + 0.45f * (i % 2)) * e;
                int n = Seg(x + Mathf.Cos(ang) * r0, y + Mathf.Sin(ang) * r0, x + Mathf.Cos(ang) * r1, y + Mathf.Sin(ang) * r1, 2);
                Draw(Taper.Head, n, 0.07f * thickness, primary, (1f - k) * a);
            }

            int rn = Ring(x, y, size * 0.8f * e, size * 0.8f * e);
            Draw(Taper.Flat, rn, 0.05f * (1f - k) * thickness, primary, (1f - k) * a);
        }

        // Ground impact: shock rings + cracks.
        void WGroundHit(float x, float k, float a, float size)
        {
            if (k <= 0f) return;
            float e = EaseOut(k);
            for (int j = 0; j < 2; j++)
            {
                float d = Mathf.Clamp01(k - 0.08f * j);
                int rn = Ring(x, groundY, size * EaseOut(d), size * 0.2f * EaseOut(d));
                Draw(Taper.Flat, rn, 0.08f * (1f - d) * thickness, primary, (1f - d) * a);
            }

            for (int i = 0; i < 4; i++)
            {
                GroundCrack(x, i % 2 == 0 ? 1f : -1f, (0.8f + 0.5f * (i / 2)) * size * 0.8f * e, i + 2, 0.05f, (1f - k) * a);
            }
        }

        // A weapon swinging round pivot: brush trail behind, prop on top.
        void WSwing(Shape sh, float len, Vector2 pivot, float ang, float span, float trailR, float trailW, float a)
        {
            WTrail(pivot, trailR, ang, span, trailW, a);
            DrawProp(sh, pivot.x, pivot.y, ang, len, a);
        }

        // Vertical qi crescent that flies to the right; returns head x.
        void WQiCrescent(float cx, float cy, float radius, float w, float a)
        {
            int n = ArcPts(cx - radius * 0.9f, cy, radius, radius * 1.15f, -62f, 62f, 28);
            DrawBrush(n, w, a);
            int t = Seg(cx - radius * 0.9f - 1.4f, cy, cx - radius * 0.9f + radius * 0.6f, cy, 10);
            DrawBrush(t, w * 0.35f, a * 0.7f);
        }

        void AnimSSpearThrust()
        {
            Vector2 c = sweepCenter;
            const float len = 2.7f;
            float p = WSw(0f, 0.16f), back = Mathf.Clamp01((_t - 0.38f) / 0.25f), a = WFade(0.5f, 0.2f);
            float px = c.x + 1.05f * p * (1f - back * 0.85f), py = c.y + 0.05f;
            if (_t > 0.02f)
            {
                int n = Seg(px - 0.3f, py, px + len * 0.55f, py, 14);
                DrawBrush(n, 0.34f * (1f - p * 0.5f), a * (1f - back));
            }

            DrawProp(Shape.Spear, px, py, 0f, len, a);
            if (_t >= 0.14f && Once(1)) Fire();
            WFlash(px + len * 0.65f, py, (_t - 0.14f) / 0.3f, a, 0.8f);
        }

        void AnimSSpearSweep()
        {
            Vector2 c = sweepCenter;
            float p = WSw(0.02f, 0.32f), a = WFade(0.5f, 0.28f);
            float ang = Mathf.Lerp(115f, -55f, p);
            Vector2 pv = c + new Vector2(0.15f, 0.05f);
            WSwing(Shape.Spear, 2.7f, pv, ang, 110f * p, 1.45f, 0.5f, a);
            if (_t >= 0.2f && Once(1)) Fire();
            Vector2 tip = pv + WDir(ang) * 1.75f;
            WFlash(tip.x, tip.y, (_t - 0.2f) / 0.3f, a, 0.7f);
        }

        void AnimSHammerSmash()
        {
            Vector2 c = sweepCenter;
            const float len = 2.3f;
            float raise = WSw(0f, 0.22f), sm = Mathf.Clamp01((_t - 0.26f) / 0.09f);
            sm *= sm;
            float ang = _t < 0.26f ? Mathf.Lerp(40f, 125f, raise) : Mathf.Lerp(125f, -20f, sm);
            float a = WFade(0.85f, 0.3f);
            Vector2 pv = c + new Vector2(0.2f, 0.1f);
            WSwing(Shape.Hammer, len, pv, ang, 100f * sm, len * 0.55f, 0.7f, a);
            float ix = pv.x + len * 0.75f * Mathf.Cos(-20f * Mathf.Deg2Rad);
            if (_t >= 0.35f && Once(1)) Fire();
            WGroundHit(ix, (_t - 0.35f) / 0.7f, a, 2.0f);
            WFlash(ix, groundY + 0.1f, (_t - 0.35f) / 0.3f, a, 0.9f);
        }

        void AnimSSwordQi()
        {
            Vector2 c = sweepCenter;
            float p = WSw(0f, 0.2f), a = WFade(0.95f, 0.3f);
            Vector2 pv = c + new Vector2(0.2f, 0.05f);
            float swordA = WFade(0.25f, 0.15f);
            WSwing(Shape.Sword, 1.9f, pv, Mathf.Lerp(105f, -35f, p), 100f * p, 1.0f, 0.45f, swordA);
            if (_t >= 0.12f)
            {
                float f = Mathf.Clamp01((_t - 0.12f) / 0.85f);
                float cx = c.x + 0.9f + qiTravel * (f * f * 0.4f + f * 0.6f);
                WQiCrescent(cx, c.y + 0.05f, 0.7f + 0.5f * f, 0.5f, a);
                if (f >= 1f && Once(1)) Fire();
                WFlash(cx + 0.3f, c.y, (_t - 0.97f) / 0.3f, 1f, 1.1f);
            }
        }

        void AnimSCrossSlash()
        {
            Vector2 c = sweepCenter;
            Vector2 im = new Vector2(c.x + 1.6f, c.y + 0.1f);
            float a = WFade(0.55f, 0.35f);
            for (int s = 0; s < 2; s++)
            {
                float t0 = 0.05f + 0.15f * s;
                float k = Mathf.Clamp01((_t - t0) / 0.14f);
                if (k <= 0f) continue;
                float side = s == 0 ? 1f : -1f;
                Vector2 A = im + new Vector2(-1.5f, 1.5f * side), B = im + new Vector2(1.5f, -1.5f * side);
                Vector2 dir = (B - A).normalized, perp = new Vector2(-dir.y, dir.x);
                int n = 26;
                float k0 = Mathf.Clamp01(k - 0.55f);
                for (int i = 0; i < n; i++)
                {
                    float u = Mathf.Lerp(k0, k, i / (float)(n - 1));
                    Vector2 q = Vector2.Lerp(A, B, u) + perp * 0.28f * Mathf.Sin(u * Mathf.PI) * side;
                    _buf[i] = new Vector3(q.x, q.y, 0f);
                }

                DrawBrush(n, 0.5f, a);
                Vector2 head = Vector2.Lerp(A, B, k);
                float fa = WFade(t0 + 0.14f, 0.12f);
                DrawProp(Shape.Sword, head.x, head.y, Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg, 1.5f, fa * (k >= 1f ? 1f : 1f));
            }

            if (_t >= 0.3f && Once(1)) Fire();
            WFlash(im.x, im.y, (_t - 0.3f) / 0.4f, a, 1.2f);
        }

        void AnimSSpearRising()
        {
            Vector2 c = sweepCenter;
            float p = WSw(0.02f, 0.3f), a = WFade(0.45f, 0.25f);
            float ang = Mathf.Lerp(-35f, 115f, p);
            Vector2 pv = c + new Vector2(0.2f, -0.1f);
            WTrail(pv, 1.5f, ang, -100f * p, 0.5f, a);
            DrawProp(Shape.Spear, pv.x, pv.y, ang, 2.7f, a);
            if (_t >= 0.18f && Once(1)) Fire();
            Vector2 tip = pv + WDir(ang) * 1.75f;
            WFlash(tip.x, tip.y, (_t - 0.18f) / 0.3f, a, 0.7f);
        }

        void AnimSSpearFlurry()
        {
            Vector2 c = sweepCenter;
            const float len = 2.6f;
            float cyc = 0.13f;
            float a = WFade(0.72f, 0.2f);
            if (Once(1)) Fire();
            for (int i = 0; i < 6; i++)
            {
                float t0 = 0.04f + cyc * 0.8f * i;
                float p = (_t - t0) / cyc;
                if (p < 0f || p > 1.6f) continue;
                float ang = ((i % 3) - 1) * 7f;
                float ext = Mathf.Sin(Mathf.Clamp01(p) * Mathf.PI * (p > 1f ? 0f : 1f));
                float px = c.x + 0.05f + 1.0f * ext;
                float al = a * (p > 1f ? 0f : 1f);
                int n = Seg(px - 0.2f, c.y + 0.05f, px + len * 0.55f, c.y + 0.05f + 0.1f * ang * 0.1f, 10);
                DrawBrush(n, 0.22f, al * 0.7f);
                DrawProp(Shape.Spear, px, c.y + 0.05f, ang, len, al * (p < 0.15f ? 0.5f : 1f));
                Vector2 tip = new Vector2(px, c.y + 0.05f) + WDir(ang) * len * 0.65f;
                WFlash(tip.x, tip.y, (p - 0.45f) / 0.5f, a, 0.6f);
            }
        }

        void AnimSHammerSweep()
        {
            Vector2 c = sweepCenter;
            const float len = 2.3f;
            float p = WSw(0.15f, 0.3f), a = WFade(0.75f, 0.25f);
            float ang = _t < 0.15f ? Mathf.Lerp(150f, 170f, _t / 0.15f) : Mathf.Lerp(170f, -15f, p);
            Vector2 pv = c + new Vector2(0.1f, -0.15f);
            WSwing(Shape.Hammer, len, pv, ang, 150f * p, 1.4f, 0.8f, a);
            if (_t >= 0.4f && Once(1)) Fire();
            float ix = pv.x + 1.7f;
            WGroundHit(ix, (_t - 0.4f) / 0.5f, a, 1.6f);
        }

        void AnimSHammerQuake()
        {
            Vector2 c = sweepCenter;
            const float len = 2.8f;
            float raise = WSw(0f, 0.3f), sm = Mathf.Clamp01((_t - 0.36f) / 0.1f);
            sm *= sm;
            float ang = _t < 0.36f ? Mathf.Lerp(30f, 130f, raise) : Mathf.Lerp(130f, -22f, sm);
            float a = WFade(1.2f, 0.4f);
            Vector2 pv = c + new Vector2(0.2f, 0.25f);
            WSwing(Shape.Hammer, len, pv, ang, 110f * sm, len * 0.55f, 0.9f, a);
            float ix = pv.x + len * 0.75f * Mathf.Cos(-22f * Mathf.Deg2Rad);
            if (_t >= 0.46f && Once(1)) Fire();
            float k = (_t - 0.46f) / 1.0f;
            WGroundHit(ix, k, a, 3.4f);
            if (k > 0f)
            {
                float e = EaseOut(k);
                for (int s = -1; s <= 1; s += 2)
                {
                    int n = Seg(ix, groundY + 0.02f, ix + s * 4.5f * e, groundY + 0.02f, 16);
                    DrawBrush(n, 0.2f, (1f - k) * a);
                }
            }
        }

        void AnimSSwordSlash()
        {
            Vector2 c = sweepCenter;
            float p = WSw(0f, 0.2f), a = WFade(0.3f, 0.25f);
            float ang = Mathf.Lerp(135f, -45f, p);
            Vector2 pv = c + new Vector2(0.15f, 0.0f);
            WSwing(Shape.Sword, 1.9f, pv, ang, 130f * p, 1.3f, 0.6f, a);
            if (_t >= 0.12f && Once(1)) Fire();
            Vector2 tip = pv + WDir(ang) * 1.5f;
            WFlash(tip.x, tip.y, (_t - 0.12f) / 0.3f, a, 0.8f);
        }

        void AnimSSwordWhirl()
        {
            Vector2 c = sweepCenter;
            float a = WFade(0.75f, 0.25f);
            float p = Mathf.Clamp01(_t / 0.7f);
            float spin = -720f * (1f - (1f - p) * (1f - p));
            float ang = 90f + spin;
            WTrail(c, 1.1f, ang, 230f * Mathf.Min(1f, _t * 6f), 0.5f, a);
            WTrail(c, 1.1f, ang + 180f, 150f * Mathf.Min(1f, _t * 6f), 0.4f, a * 0.7f);
            DrawProp(Shape.Sword, c.x, c.y, ang, 2.0f, a);
            DrawProp(Shape.Sword, c.x, c.y, ang + 180f, 1.6f, a * 0.65f);
            if (Once(1)) Fire();
        }

        void AnimSSwordLunge()
        {
            Vector2 c = sweepCenter;
            const float len = 2.1f;
            float p = WSw(0f, 0.14f), back = Mathf.Clamp01((_t - 0.3f) / 0.15f), a = WFade(0.3f, 0.2f);
            float px = c.x - 0.1f + 1.15f * p * (1f - 0.8f * back), py = c.y + 0.05f;
            if (_t > 0.01f)
            {
                int n = Seg(px - 0.9f, py, px + len * 0.6f, py, 14);
                DrawBrush(n, 0.5f * (1f - back), a);
            }

            for (int g = 3; g >= 1; g--)
            {
                DrawProp(Shape.Sword, px - 0.28f * g, py, 0f, len, a * 0.2f);
            }

            DrawProp(Shape.Sword, px, py, 0f, len, a);
            if (_t >= 0.12f && Once(1)) Fire();
            WFlash(px + len * 0.65f, py, (_t - 0.12f) / 0.3f, a, 0.9f);
        }

        void AnimSBladeChop()
        {
            Vector2 c = sweepCenter;
            float raise = WSw(0f, 0.15f), sm = Mathf.Clamp01((_t - 0.17f) / 0.08f);
            sm *= sm;
            float ang = _t < 0.17f ? Mathf.Lerp(60f, 115f, raise) : Mathf.Lerp(115f, -52f, sm);
            float a = WFade(0.55f, 0.25f);
            Vector2 pv = c + new Vector2(0.25f, 0.1f);
            WSwing(Shape.Sword, 2.3f, pv, ang, 120f * sm, 1.45f, 0.9f, a);
            Vector2 tip = pv + WDir(-52f) * 1.9f;
            if (_t >= 0.25f && Once(1)) Fire();
            WGroundHit(tip.x, (_t - 0.25f) / 0.45f, a, 1.7f);
            WFlash(tip.x, tip.y, (_t - 0.25f) / 0.3f, a, 1.0f);
        }

        void AnimSBladeDrag()
        {
            Vector2 c = sweepCenter;
            const float len = 2.0f;
            float p = Mathf.Clamp01((_t - 0.05f) / 0.6f), a = WFade(0.75f, 0.25f);
            float px = c.x + 0.2f + 2.4f * (1f - (1f - p) * (1f - p));
            float py = groundY + 0.8f * len * 0.5f;
            Vector2 tip = new Vector2(px, py) + WDir(-30f) * len * 0.8f;
            float startX = c.x + 0.2f + len * 0.69f;
            if (_t > 0.05f)
            {
                int n = Seg(startX, groundY + 0.03f, tip.x, groundY + 0.03f, 20);
                DrawBrush(n, 0.18f, a);
                int m = Seg(tip.x - 1.3f, groundY + 0.2f, tip.x, tip.y, 10);
                DrawBrush(m, 0.4f, a * 0.8f);
            }

            DrawProp(Shape.Sword, px, py, -30f, len, a);
            if (_t >= 0.1f && Once(1)) Fire();
            WFlash(tip.x, groundY + 0.1f, Mathf.Repeat(_t * 3f, 1f), a * (p < 1f ? 0.8f : 0f), 0.5f);
        }

        void AnimSHalberdCrescent()
        {
            Vector2 c = sweepCenter;
            float p = WSw(0f, 0.22f), a = WFade(0.45f, 0.25f);
            float ang = Mathf.Lerp(120f, -40f, p);
            Vector2 pv = c + new Vector2(0.15f, 0.0f);
            WSwing(Shape.Halberd, 3.0f, pv, ang, 120f * p, 1.7f, 0.7f, a);
            if (_t >= 0.15f)
            {
                float f = Mathf.Clamp01((_t - 0.15f) / 0.5f);
                WQiCrescent(c.x + 1.6f + 1.6f * f, c.y, 0.9f + 0.3f * f, 0.55f, WFade(0.4f, 0.28f));
                if (_t >= 0.22f && Once(1)) Fire();
            }
        }

        void AnimSHalberdLift()
        {
            Vector2 c = sweepCenter;
            float lift = WSw(0f, 0.3f);
            float th = Mathf.Clamp01((_t - 0.36f) / 0.1f);
            float a = WFade(0.7f, 0.2f);
            float ang = Mathf.Lerp(-55f, 70f, lift);
            Vector2 pv = c + new Vector2(0.15f, -0.1f);
            if (_t >= 0.36f)
            {
                ang = Mathf.Lerp(70f, 4f, th);
                pv += new Vector2(1.1f * th * (1f - Mathf.Clamp01((_t - 0.55f) / 0.2f) * 0.8f), 0.4f * (1f - th));
            }

            WTrail(pv, 1.5f, ang, _t < 0.36f ? -110f * lift : 0f, 0.5f, a);
            if (_t >= 0.36f)
            {
                int n = Seg(pv.x - 0.2f, pv.y, pv.x + 2.2f, pv.y, 12);
                DrawBrush(n, 0.4f * (1f - Mathf.Clamp01((_t - 0.5f) / 0.2f)), a);
            }

            DrawProp(Shape.Halberd, pv.x, pv.y, ang, 3.0f, a);
            if (_t >= 0.44f && Once(1)) Fire();
            Vector2 tip = pv + WDir(ang) * 1.8f;
            WFlash(tip.x, tip.y, (_t - 0.44f) / 0.35f, a, 0.9f);
        }

        void AnimSWhipLash()
        {
            Vector2 c = sweepCenter;
            float a = WFade(0.65f, 0.25f);
            Vector2 H = c + new Vector2(0.3f, 0.1f);
            float p = WSw(0f, 0.22f);
            Vector2 T = new Vector2(c.x + 2.8f, c.y + 0.0f);
            // whip: handle stick + a long rope that snaps forward
            DrawProp(Shape.Stick, H.x - 0.1f, H.y - 0.05f, 25f, 0.8f, a);
            int n = 30;
            for (int i = 0; i < n; i++)
            {
                float s = i / (float)(n - 1);
                float reach = s * p;
                float wave = (1f - p * 0.6f) * 0.55f * Mathf.Sin(s * 8f - _t * 28f) * Mathf.Sin(s * Mathf.PI) * (1f - p * 0.5f);
                Vector2 q = Vector2.Lerp(H, T, reach) + new Vector2(0f, wave + 0.5f * (1f - p) * (1f - s) * s * 4f);
                _buf[i] = new Vector3(q.x, q.y, 0f);
            }

            DrawBrush(n, 0.16f, a);
            if (_t >= 0.2f && Once(1)) Fire();
            WFlash(T.x, T.y, (_t - 0.2f) / 0.35f, a, 1.0f);
        }

        void AnimSNunchaku()
        {
            Vector2 c = sweepCenter;
            Vector2 H = c + new Vector2(0.3f, 0.1f);
            float a = WFade(0.78f, 0.22f);
            float spin = _t * 760f;
            float angA = spin;
            Vector2 tipA = H + WDir(angA) * 1.2f * 0.88f;
            float angB = angA + 110f + 55f * Mathf.Sin(_t * 18f);
            Vector2 pivB = tipA + WDir(angA + 50f) * 0.32f;
            Vector2 tipB = pivB + WDir(angB) * 1.2f * 0.88f;
            WTrail(H, 1.6f, angA, 160f, 0.4f, a);
            int n = Seg(tipA.x, tipA.y, pivB.x, pivB.y, 6);
            DrawBrush(n, 0.09f, a);
            DrawProp(Shape.Stick, H.x, H.y, angA, 1.2f, a);
            DrawProp(Shape.Stick, pivB.x, pivB.y, angB, 1.2f, a);
            if (Once(1)) Fire();
            WFlash(tipB.x, tipB.y, Mathf.Repeat(_t * 4f, 1f), a * 0.6f, 0.5f);
        }

        void AnimSPalmWave()
        {
            Vector2 c = sweepCenter;
            float a = WFade(0.5f, 0.3f);
            WFlash(c.x + 0.6f, c.y + 0.05f, _t / 0.25f, 1f, 0.9f);
            for (int i = 0; i < 3; i++)
            {
                float f = Mathf.Clamp01((_t - 0.04f - 0.09f * i) / 0.5f);
                if (f <= 0f) continue;
                float cx = c.x + 0.9f + 2.4f * EaseOut(f);
                int n = ArcPts(cx - 0.55f, c.y + 0.05f, 0.75f + 0.25f * i, 1.0f + 0.25f * i, -50f, 50f, 24);
                DrawBrush(n, 0.45f - 0.08f * i, a * (1f - f * 0.5f));
            }

            if (_t >= 0.2f && Once(1)) Fire();
        }

        void AnimSDart()
        {
            Vector2 c = sweepCenter;
            float dist = qiTravel * 1.4f;
            float p = Mathf.Clamp01(_t / 0.4f);
            float a = WFade(0.6f, 0.3f);
            float x = c.x + 0.5f + dist * p;
            float y = c.y + 0.05f + (p >= 1f ? 0.03f * Mathf.Sin(_t * 90f) * (1f - Mathf.Clamp01((_t - 0.4f) / 0.3f)) : 0f);
            if (p < 1f)
            {
                int n = Seg(x - 1.6f, y, x, y, 14);
                DrawBrush(n, 0.26f, a);
            }

            DrawProp(Shape.Kunai, x, y, 0f, 0.9f, a);
            if (p >= 1f && Once(1)) Fire();
            WFlash(x + 0.35f, y, (_t - 0.4f) / 0.3f, a, 0.7f);
        }

        void AnimSArrow()
        {
            Vector2 c = sweepCenter;
            float dist = qiTravel * 1.5f;
            float p = Mathf.Clamp01(_t / 0.4f);
            float a = WFade(0.75f, 0.25f);
            float x = c.x + 0.4f + dist * p;
            float y = c.y + 0.05f + (p >= 1f ? 0.025f * Mathf.Sin(_t * 80f) * (1f - Mathf.Clamp01((_t - 0.4f) / 0.35f)) : 0f);
            if (p < 1f)
            {
                int n = Seg(x - 2.6f, y, x, y, 16);
                DrawBrush(n, 0.2f, a);
            }

            DrawProp(Shape.Arrow, x, y, 0f, 1.7f, a);
            if (p >= 1f && Once(1)) Fire();
            WFlash(x + 0.85f, y, (_t - 0.4f) / 0.3f, a, 0.8f);
        }
    }
}
