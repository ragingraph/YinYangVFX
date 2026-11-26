using UnityEngine;

namespace YinYangVFX
{
    // Concrete pickup / environment effects: a spinning taiji coin, treasure burst, item orb, healing cross,
    // a save lantern, a spinning gear plate, ground spikes. Origin = object centre (ground at groundY).
    public partial class WeaponEffect
    {
        void AnimSCoinPickup()
        {
            Vector2 c = sweepCenter;
            float g = WFade(0.35f, 0.45f);
            float e = EaseOut(_t / 0.3f);
            if (Once(1)) Fire();
            float sw = Mathf.Abs(Mathf.Cos(_t * 18f));
            DrawProp(Shape.Taiji, c.x, c.y + 0.8f * e, 90f + _t * 200f, 0.45f * (0.4f + 0.6f * sw + 0.3f), g);
            DrawProp(Shape.Star5, c.x, c.y + 0.8f * e, 90f + _t * 300f, 0.9f * Mathf.Sin(Mathf.Clamp01(_t / 0.25f) * Mathf.PI), 1f);
            HShards(6, c.x, c.y + 0.4f, 3.0f, 0.2f, 0f, 0.5f, 1f, 210, 90f, 200f, Shape.Star5);
            int n = Ring(c.x, c.y, 0.5f * e, 0.5f * e);
            DrawBrush(n, 0.05f, (1f - e) * g);
        }

        void AnimSChestOpen()
        {
            Vector2 c = sweepCenter;
            float top = c.y + 0.3f;
            float g = WFade(1.3f, 0.5f);
            float e = EaseOut(_t / 0.3f);
            if (Once(1)) Fire();
            float lid = Mathf.Lerp(0f, 110f, EaseOut(_t / 0.25f));
            Vector2 ld = WDir(180f - lid);
            HStroke(c.x + 0.5f, top, c.x + 0.5f + ld.x * 1.0f, top + ld.y * 1.0f, 0f, 0.14f, g);
            HStroke(c.x, groundY, c.x, top + 2.4f * e, 0f, 0.7f * (1f - 0.3f * e), g * 0.5f);
            DrawProp(Shape.Star5, c.x, top + 0.2f, 90f + _t * 140f, 0.5f + 1.0f * e, g);
            for (int k = 0; k < 6; k++)
            {
                float f = Mathf.Clamp01((_t - 0.1f - 0.05f * k) / 1.0f);
                if (f <= 0f) continue;
                float x = c.x + (k - 2.5f) * 0.22f * (0.4f + f);
                float y = top + 1.6f * Mathf.Sin(f * Mathf.PI * 0.8f) - 0.3f * f;
                Shape sh = k % 3 == 0 ? Shape.Taiji : (k % 3 == 1 ? Shape.Orb : Shape.Star5);
                DrawProp(sh, x, y, 90f + _t * 200f, 0.3f, g * (1f - 0.3f * f));
            }

            int n = Ring(c.x, groundY, 1.0f * e, 0.2f * e);
            DrawBrush(n, 0.07f, g);
        }

        void AnimSItemPickup()
        {
            Vector2 c = sweepCenter;
            float g = WFade(0.6f, 0.4f);
            float e = EaseOut(_t / 0.35f);
            if (Once(1)) Fire();
            float y = c.y + 0.9f * e;
            DrawProp(Shape.Orb, c.x, y, 0f, 0.5f * (1f - 0.3f * e), g);
            DrawProp(Shape.Star5, c.x, y, 90f + _t * 260f, 0.4f + 0.7f * Mathf.Sin(Mathf.Clamp01(_t / 0.4f) * Mathf.PI), g);
            int m = ArcPts(c.x, c.y + 0.3f, 0.5f * e, 0.5f * e, _t * 500f, _t * 500f + 240f, 18);
            DrawBrush(m, 0.1f, g);
            HShards(6, c.x, c.y, 3.0f, 0.2f, 0f, 0.5f, 1f, 220, 90f, 360f, Shape.Shard);
        }

        void AnimSHealItem()
        {
            Vector2 c = sweepCenter;
            float g = WFade(1.1f, 0.5f);
            float e = EaseOut(_t / 0.4f);
            if (Once(1)) Fire();
            DrawProp(Shape.Cross, c.x, c.y + 0.4f + 0.5f * e, 90f, 1.1f * e * (1f + 0.05f * Mathf.Sin(_t * 9f)), g);
            for (int k = 0; k < 6; k++)
            {
                float f = SLoop(k, 3, 0.6f);
                DrawProp(Shape.Feather, c.x + (Rnd(k, 4) - 0.5f) * 0.9f, c.y - 0.4f + f * 1.8f, 90f, 0.32f, Mathf.Sin(f * Mathf.PI) * g * e);
            }

            for (int k = 0; k < 3; k++)
            {
                float f = Mathf.Repeat(_t * 0.7f + k / 3f, 1f);
                int n = Ring(c.x, groundY + f * 1.8f, 0.45f, 0.09f);
                DrawBrush(n, 0.06f, Mathf.Sin(f * Mathf.PI) * g * e);
            }

            int gr = Ring(c.x, groundY, 0.7f * e, 0.14f * e);
            DrawBrush(gr, 0.07f, g);
        }

        void AnimSSavePoint()
        {
            Vector2 c = sweepCenter;
            EFlame(c.x, groundY + 0.5f, 0.8f, 90f, 0.95f, 230);
            DrawProp(Shape.Taiji, c.x, groundY + 1.25f + 0.05f * Mathf.Sin(_t * 3f), 90f + _t * 60f, 0.45f, 0.95f);
            int n = Ring(c.x, groundY, 0.6f, 0.12f);
            DrawBrush(n, 0.07f, 0.85f);
            for (int k = 0; k < 3; k++)
            {
                float ang = _t * 90f + k * 120f;
                Vector2 d = WDir(ang);
                DrawProp(Shape.Star5, c.x + d.x * 0.5f, groundY + 0.8f + d.y * 0.15f + 0.3f, 90f + _t * 180f, 0.18f, 0.9f);
            }

            for (int k = 0; k < 3; k++)
            {
                float f = SLoop(k, 5, 0.35f);
                DrawProp(Shape.Feather, c.x + (Rnd(k, 6) - 0.5f) * 0.5f, groundY + 0.3f + f * 1.3f, 90f, 0.22f, Mathf.Sin(f * Mathf.PI) * 0.85f);
            }
        }

        void AnimSMechanismActivate()
        {
            Vector2 c = sweepCenter;
            float g = WFade(0.8f, 0.4f);
            float e = EaseOut(_t / 0.25f);
            if (Once(1)) Fire();
            DrawProp(Shape.Hex, c.x, c.y, 90f + _t * 300f, 0.9f * e, g);
            DrawProp(Shape.Hex, c.x, c.y, 90f - _t * 220f, 0.55f * e, g);
            DrawProp(Shape.Bolt, c.x, c.y, 90f, 1.2f * Mathf.Sin(Mathf.Clamp01(_t / 0.2f) * Mathf.PI), g);
            for (int j = 0; j < 3; j++)
            {
                float d = Mathf.Clamp01((_t - 0.07f * j) / 0.5f);
                int n = Ring(c.x, c.y, 1.1f * EaseOut(d), 1.1f * EaseOut(d));
                DrawBrush(n, 0.07f - 0.015f * j, (1f - d) * g);
            }

            HShards(8, c.x, c.y, 4.0f, 0.25f, 0f, 0.5f, 1f, 240, 90f, 360f, Shape.Shard);
        }

        void AnimSSpikeTrap()
        {
            Vector2 c = sweepCenter;
            const float warn = 0.5f;
            float g = WFade(0.9f, 0.5f);
            const int N = 6;
            if (_t < warn)
            {
                float fl = 0.5f + 0.5f * Mathf.Sin(_t * 40f);
                HStroke(c.x - 1.1f, groundY + 0.02f, c.x + 1.1f, groundY + 0.02f, 0f, 0.12f, fl);
                for (int i = 0; i < N; i++)
                {
                    DrawStanding(Shape.Shard, c.x + (i - 2.5f) * 0.4f, groundY - 0.4f + 0.1f * fl, 0.4f, 0.6f * fl);
                }

                return;
            }

            if (Once(1)) Fire();
            float k = (_t - warn) / 0.12f;
            float up = k < 1f ? 1f - Mathf.Pow(1f - Mathf.Clamp01(k), 4f) : 1f - Mathf.Clamp01((_t - warn - 0.45f) / 0.35f);
            for (int i = 0; i < N; i++)
            {
                float len = (0.9f + 0.25f * (i % 2)) ;
                DrawStanding(Shape.Shard, c.x + (i - 2.5f) * 0.4f, groundY - len + len * 0.97f * up, len, g * 0.0f + 1f);
            }

            int n = Ring(c.x, groundY, 1.2f * EaseOut(k), 0.2f * EaseOut(k));
            DrawBrush(n, 0.07f, Mathf.Clamp01(1f - k * 0.5f) * g);
            HShards(6, c.x, groundY + 0.2f, 4.0f, 0.2f, warn, 0.45f, 1f, 250, 90f, 100f, Shape.Rock);
        }
    }
}
