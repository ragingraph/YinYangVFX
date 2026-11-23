using UnityEngine;

namespace YinYangVFX
{
    // Concrete hit effects: ink-brush slash marks, flying shards, solid stars, a stabbing kunai, a skull on kills.
    // Origin = the point where the hit lands. Single colour per prefab.
    public partial class WeaponEffect
    {
        // Shards flying out from (cx,cy) with drag and gravity. Tip points along the travel direction.
        void HShards(int n, float cx, float cy, float speed, float size, float t0, float life, float a, int salt, float baseAng, float spread, Shape sh)
        {
            float tt = _t - t0;
            if (tt <= 0f || tt >= life) return;
            float f = a * (1f - tt / life);
            for (int i = 0; i < n; i++)
            {
                float ang = (baseAng + (Rnd(i, salt) - 0.5f) * spread) * Mathf.Deg2Rad;
                float sp = speed * (0.6f + 0.8f * Rnd(i, salt + 7));
                float d = sp * (1f - Mathf.Exp(-5f * tt)) / 5f;
                float x = cx + Mathf.Cos(ang) * d;
                float y = cy + Mathf.Sin(ang) * d - 2.0f * tt * tt;
                float vx = Mathf.Cos(ang) * sp * Mathf.Exp(-5f * tt);
                float vy = Mathf.Sin(ang) * sp * Mathf.Exp(-5f * tt) - 4.0f * tt;
                float dir = Mathf.Atan2(vy, vx) * Mathf.Rad2Deg;
                float sz = size * (0.7f + 0.6f * Rnd(i, salt + 3));
                DrawProp(sh, x, y, dir, sz, f);
            }
        }

        // Ink brush stroke from (x0,y0) to (x1,y1) with a sideways bend; head at the end.
        void HStroke(float x0, float y0, float x1, float y1, float bend, float w, float a)
        {
            const int m = 12;
            float dx = x1 - x0, dy = y1 - y0;
            float L = Mathf.Sqrt(dx * dx + dy * dy);
            if (L < 0.05f) return;
            float px = -dy / L, py = dx / L;
            for (int i = 0; i < m; i++)
            {
                float s = i / (float)(m - 1);
                float off = Mathf.Sin(s * Mathf.PI) * bend;
                _buf[i] = new Vector3(x0 + dx * s + px * off, y0 + dy * s + py * off, 0f);
            }

            DrawBrush(m, w, a);
        }

        // Brush stroke through the origin along angle ang, head sweeping from -L to +L, tail following.
        void HSlashMark(float ang, float L, float t0, float w, float bend, float a)
        {
            float k = Mathf.Clamp01((_t - t0) / 0.09f);
            if (k <= 0f) return;
            Vector2 d = WDir(ang);
            float head = -L + 2f * L * (1f - (1f - k) * (1f - k));
            float tail = -L + 1.5f * L * Mathf.Clamp01((_t - t0 - 0.08f) / 0.3f);
            if (tail >= head - 0.05f) return;
            HStroke(d.x * tail, d.y * tail, d.x * head, d.y * head, bend, w, a);
        }

        void AnimHHitSlash()
        {
            float a = WFade(0.2f, 0.3f);
            if (Once(1)) Fire();
            HSlashMark(-35f, 1.35f, 0f, 0.34f, 0.12f, a);
            HSlashMark(-30f, 0.95f, 0.03f, 0.18f, -0.1f, a * 0.85f);
            float k = _t / 0.22f;
            WFlash(0f, 0f, k, a, 0.9f);
            DrawProp(Shape.Star5, 0f, 0f, 90f + _t * 200f, 0.8f * EaseOut(_t / 0.08f), WFade(0.06f, 0.14f));
            HShards(6, 0f, 0f, 5.5f, 0.32f, 0.02f, 0.4f, 1f, 1, 145f, 50f, Shape.Shard);
            HShards(6, 0f, 0f, 5.5f, 0.32f, 0.02f, 0.4f, 1f, 2, -35f, 50f, Shape.Shard);
        }

        void AnimHHitBlunt()
        {
            float a = WFade(0.3f, 0.3f);
            if (Once(1)) Fire();
            float e = EaseOut(_t / 0.1f);
            DrawProp(Shape.Star5, 0f, 0f, 90f, (0.5f + 1.3f * e), WFade(0.05f, 0.25f));
            for (int j = 0; j < 3; j++)
            {
                float d = Mathf.Clamp01((_t - 0.04f * j) / 0.3f);
                int n = Ring(0f, 0f, 1.0f * EaseOut(d), 1.0f * EaseOut(d));
                Draw(Taper.Flat, n, (0.1f - 0.025f * j) * thickness, primary, (1f - d) * a * (1f - 0.2f * j));
            }

            for (int k = 0; k < 8; k++)
            {
                float ang = (k * 45f + 22.5f) * Mathf.Deg2Rad;
                float L = (k % 2 == 0 ? 1.0f : 0.6f) * EaseOut(_t / 0.1f);
                HStroke(Mathf.Cos(ang) * 0.2f, Mathf.Sin(ang) * 0.2f, Mathf.Cos(ang) * (0.2f + L), Mathf.Sin(ang) * (0.2f + L), 0f, 0.14f, a);
            }

            HShards(9, 0f, 0f, 5.0f, 0.5f, 0.0f, 0.55f, 1f, 4, 90f, 140f, Shape.Rock);
            HShards(8, 0f, 0f, 6.5f, 0.3f, 0.0f, 0.45f, 1f, 5, 90f, 360f, Shape.Shard);
        }

        void AnimHHitElemental()
        {
            float a = WFade(0.4f, 0.3f);
            if (Once(1)) Fire();
            float e = EaseOut(_t / 0.2f);
            for (int k = 0; k < 6; k++)
            {
                float ang = k * 60f + _t * 90f;
                Vector2 d = WDir(ang);
                DrawProp(Shape.Flame, d.x * 0.25f * e, d.y * 0.25f * e, ang, (0.5f + 0.9f * e) * (1f + 0.1f * Mathf.Sin(_t * 18f + k)), a);
            }

            DrawProp(Shape.Star5, 0f, 0f, 90f - _t * 240f, 0.55f + 0.9f * EaseOut(_t / 0.12f), a);
            for (int k = 0; k < 2; k++)
            {
                float r = (0.55f + 0.45f * k) * (0.4f + 0.6f * e);
                float a0 = _t * 420f * (k == 0 ? 1f : -1f) + k * 90f;
                int n = ArcPts(0f, 0f, r, r, a0, a0 + 210f, 22);
                DrawBrush(n, 0.16f, a * 0.9f);
            }

            HShards(8, 0f, 0f, 3.5f, 0.24f, 0.05f, 0.5f, 1f, 6, 90f, 360f, Shape.Star5);
        }

        void AnimHHitPierce()
        {
            float a = WFade(0.28f, 0.22f);
            float len = 1.25f;
            float p = Mathf.Clamp01(_t / 0.07f);
            float px = 0.12f - 0.7f * len - 1.6f * (1f - p * p);
            if (_t < 0.07f)
            {
                HStroke(px - 1.4f, 0f, px, 0f, 0f, 0.2f, 0.7f);
            }
            else if (Once(1)) Fire();
            float shake = _t > 0.07f ? Mathf.Sin(_t * 90f) * 0.025f * WFade(0.07f, 0.12f) : 0f;
            DrawProp(Shape.Kunai, px + shake, 0f, 0f, len, a * 1.0f + 0.0f);
            float re = EaseOut((_t - 0.06f) / 0.25f);
            if (_t > 0.06f)
            {
                int rn = Ring(0.15f, 0f, 0.1f + 0.25f * re, 0.2f + 0.55f * re);
                Draw(Taper.Flat, rn, 0.05f * thickness, primary, (1f - re) * a);
                WFlash(0.15f, 0f, (_t - 0.06f) / 0.22f, a, 0.8f);
            }

            HShards(7, 0.15f, 0f, 6.0f, 0.3f, 0.07f, 0.4f, 1f, 8, 0f, 60f, Shape.Shard);
            HShards(5, 0.1f, 0f, 4.0f, 0.25f, 0.07f, 0.4f, 1f, 9, 180f, 70f, Shape.Shard);
        }

        void AnimHCritHit()
        {
            float a = WFade(0.5f, 0.3f);
            if (Once(1)) Fire();
            float e = EaseOut(_t / 0.12f);
            DrawProp(Shape.Star5, 0f, 0f, 90f + _t * 120f, 0.8f + 1.9f * e, WFade(0.25f, 0.25f));
            DrawProp(Shape.Star5, 0f, 0f, 54f - _t * 160f, 0.5f + 1.2f * e, WFade(0.2f, 0.25f));
            HSlashMark(40f, 1.7f, 0f, 0.4f, 0.1f, a);
            HSlashMark(-40f, 1.7f, 0.03f, 0.4f, -0.1f, a);
            HSlashMark(0f, 1.5f, 0.05f, 0.2f, 0f, a * 0.9f);
            HSlashMark(90f, 1.2f, 0.07f, 0.16f, 0f, a * 0.8f);
            for (int j = 0; j < 2; j++)
            {
                float d = Mathf.Clamp01((_t - 0.05f * j) / 0.35f);
                int n = Ring(0f, 0f, 1.5f * EaseOut(d), 1.5f * EaseOut(d));
                Draw(Taper.Flat, n, (0.09f - 0.03f * j) * thickness, primary, (1f - d) * a);
            }

            WFlash(0f, 0f, _t / 0.3f, a, 1.6f);
            HShards(10, 0f, 0f, 7.5f, 0.45f, 0.0f, 0.6f, 1f, 11, 90f, 360f, Shape.Shard);
            HShards(6, 0f, 0f, 4.5f, 0.28f, 0.03f, 0.6f, 1f, 12, 90f, 360f, Shape.Star5);
        }

        // The light behind a "CRITICAL" text: a bold brush banner with a star in the middle and rays.
        void AnimHCritBadge()
        {
            float a = WFade(0.7f, 0.3f);
            if (Once(1)) Fire();
            float e = EaseOut(_t / 0.18f);
            for (int k = 0; k < 12; k++)
            {
                float ang = (k * 30f + _t * 20f) * Mathf.Deg2Rad;
                float L = (k % 2 == 0 ? 1.7f : 1.1f) * e;
                HStroke(Mathf.Cos(ang) * 0.5f, Mathf.Sin(ang) * 0.5f, Mathf.Cos(ang) * (0.5f + L), Mathf.Sin(ang) * (0.5f + L), 0f, 0.14f, a * 0.7f);
            }

            DrawProp(Shape.Star5, 0f, 0f, 90f + _t * 50f, 0.9f + 1.5f * e, a * 0.9f);
            float half = 2.0f * e;
            HStroke(-half, 0f, half, 0f, 0f, 0.5f, a);
            HStroke(half * 0.9f, -0.1f, -half * 0.8f, -0.1f, 0f, 0.18f, a * 0.8f);
            DrawProp(Shape.Star5, -half, 0f, 90f - _t * 120f, 0.5f * e, a);
            DrawProp(Shape.Star5, half, 0f, 90f + _t * 120f, 0.5f * e, a);
            HShards(8, 0f, 0f, 3.5f, 0.22f, 0.04f, 0.6f, 1f, 13, 90f, 360f, Shape.Star5);
        }

        void AnimHKillBurst()
        {
            float a = WFade(0.8f, 0.4f);
            if (Once(1)) Fire();
            float e = EaseOut(_t / 0.2f);
            DrawProp(Shape.Star5, 0f, 0f, 90f + _t * 100f, 1.0f + 2.0f * e, WFade(0.2f, 0.3f));
            for (int j = 0; j < 3; j++)
            {
                float d = Mathf.Clamp01((_t - 0.06f * j) / 0.6f);
                float r = 2.4f * EaseOut(d) * (1f - 0.2f * j);
                int n = Ring(0f, 0f, r, r);
                Draw(Taper.Flat, n, (0.11f - 0.03f * j) * thickness, primary, (1f - d) * a * (1f - 0.2f * j));
            }

            for (int k = 0; k < 10; k++)
            {
                float ang = (k * 36f + Rnd(k) * 12f) * Mathf.Deg2Rad;
                float r1 = (0.4f + 1.9f * e) * (0.6f + 0.4f * Rnd(k, 1));
                HStroke(Mathf.Cos(ang) * 0.3f, Mathf.Sin(ang) * 0.3f, Mathf.Cos(ang) * r1, Mathf.Sin(ang) * r1, 0f, 0.16f, a * 0.9f);
            }

            float sp = Mathf.Clamp01((_t - 0.05f) / 0.12f);
            float sy = 0.15f + 0.9f * EaseOut(_t / 0.9f);
            DrawProp(Shape.Skull, 0f, sy, 90f, 1.0f * EaseOut(sp), WFade(0.5f, 0.55f));
            HShards(14, 0f, 0f, 8.5f, 0.5f, 0.0f, 0.8f, 1f, 21, 90f, 360f, Shape.Shard);
            HShards(8, 0f, 0f, 5.0f, 0.4f, 0.0f, 0.8f, 1f, 22, 90f, 360f, Shape.Rock);
            HShards(8, 0f, 0.3f, 2.0f, 0.5f, 0.1f, 1.0f, 1f, 23, 90f, 60f, Shape.Flame);
        }
    }
}
