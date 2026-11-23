using UnityEngine;

namespace YinYangVFX
{
    // Concrete buff / debuff effects (one-shot, ~1.3 s): a real sword rising, solid arrows, shield panels,
    // spinning stars; debuffs droop and shatter. Origin = character body centre (sweepCenter).
    public partial class WeaponEffect
    {
        // Columns of solid arrows moving up (dir 90) or down (dir -90).
        void BArrows(float dir, float a, float size)
        {
            Vector2 c = sweepCenter;
            for (int col = 0; col < 3; col++)
            {
                for (int k = 0; k < 2; k++)
                {
                    float f = Mathf.Repeat(_t * 0.9f + k * 0.5f + col * 0.17f, 1f);
                    float y = dir > 0f ? c.y - 0.5f + f * 1.4f : c.y + 0.9f - f * 1.4f;
                    DrawProp(Shape.Arrow, c.x + (col - 1) * 0.45f, y, dir, size, Mathf.Sin(f * Mathf.PI) * a);
                }
            }
        }

        void BGroundRing(float e, float a, bool expand)
        {
            Vector2 c = sweepCenter;
            float r = expand ? 0.2f + 0.9f * e : 1.1f - 0.9f * e;
            int n = Ring(c.x, groundY, r, r * 0.2f);
            DrawBrush(n, 0.08f, (expand ? 1f - e * 0.5f : 0.5f + 0.5f * e) * a);
        }

        void AnimSBuffAttackUp()
        {
            Vector2 c = sweepCenter;
            float e = EaseOut(_t / 0.3f), g = WFade(0.8f, 0.5f);
            if (Once(1)) Fire();
            float len = 1.7f * e;
            float y = c.y - 0.35f + 0.5f * e + 0.05f * Mathf.Sin(_t * 8f);
            int n = Seg(c.x, c.y - 0.5f, c.x, c.y + 1.2f * e, 6);
            DrawBrush(n, 0.45f, g * 0.45f);
            DrawProp(Shape.Sword, c.x, y, 90f, len, g);
            for (int k = 0; k < 2; k++)
            {
                float side = k == 0 ? -1f : 1f;
                EFlame(c.x + side * 0.3f, c.y - 0.45f, 0.7f * e, 90f + side * 10f, g * 0.9f, k + 60);
            }

            BArrows(90f, g * 0.8f, 0.42f);
            BGroundRing(e, g, true);
        }

        void AnimSBuffSpeedUp()
        {
            Vector2 c = sweepCenter;
            float e = EaseOut(_t / 0.3f), g = WFade(0.8f, 0.5f);
            if (Once(1)) Fire();
            for (int j = 0; j < 6; j++)
            {
                float y = c.y - 0.5f + 1.1f * Rnd(j);
                float f = Mathf.Repeat(_t * 1.6f + Rnd(j, 1), 1f);
                float x = c.x + 0.7f - f * 2.0f;
                HStroke(x - 0.9f, y, x, y, 0f, 0.1f, g * Mathf.Sin(f * Mathf.PI) * 0.9f);
            }

            for (int k = 0; k < 3; k++)
            {
                float f = Mathf.Repeat(_t * 1.1f + k / 3f, 1f);
                DrawProp(Shape.Arrow, c.x - 0.5f + f * 1.5f, c.y + (k - 1) * 0.32f, 0f, 0.55f, Mathf.Sin(f * Mathf.PI) * g);
            }

            for (int k = 0; k < 2; k++)
            {
                float side = k == 0 ? 1f : -1f;
                DrawProp(Shape.Feather, c.x - 0.25f, c.y + 0.15f + side * 0.12f, 180f + side * 35f, 0.7f * e, g);
            }

            BGroundRing(e, g, true);
        }

        void AnimSBuffDefenseUp()
        {
            Vector2 c = sweepCenter;
            float e = EaseOut(_t / 0.3f), g = WFade(0.8f, 0.5f);
            if (Once(1)) Fire();
            DrawProp(Shape.Hex, c.x, c.y + 0.1f, 90f, (0.8f + 0.5f * e) * (1f + 0.05f * Mathf.Sin(_t * 9f)), g);
            DrawProp(Shape.Taiji, c.x, c.y + 0.1f, 90f + _t * 60f, 0.5f * e, g * 0.9f);
            for (int k = 0; k < 6; k++)
            {
                float ang = k * 60f + 30f + _t * 40f;
                Vector2 d = WDir(ang);
                DrawProp(Shape.Hex, c.x + d.x * 0.85f * e, c.y + 0.1f + d.y * 0.85f * e, 90f, 0.28f, g * 0.85f);
            }

            int n = Ring(c.x, c.y + 0.1f, 1.0f * e + 0.1f, 1.0f * e + 0.1f);
            DrawBrush(n, 0.07f, (1f - e) * g);
            BArrows(90f, g * 0.6f, 0.34f);
            BGroundRing(e, g, true);
        }

        void AnimSBuffCritUp()
        {
            Vector2 c = sweepCenter;
            float e = EaseOut(_t / 0.3f), g = WFade(0.8f, 0.5f);
            if (Once(1)) Fire();
            float cy = c.y + 0.1f;
            DrawProp(Shape.Star5, c.x, cy, 90f + _t * 140f, 0.4f + 1.1f * e, g);
            DrawProp(Shape.Star5, c.x, cy, 54f - _t * 100f, 0.3f + 0.7f * e, g * 0.8f);
            for (int k = 0; k < 2; k++)
            {
                float a = (_t * 40f + k * 90f + 45f) * Mathf.Deg2Rad;
                float L = 1.0f * e;
                HStroke(c.x - Mathf.Cos(a) * L, cy - Mathf.Sin(a) * L, c.x + Mathf.Cos(a) * L, cy + Mathf.Sin(a) * L, 0f, 0.2f, g * 0.9f);
            }

            for (int k = 0; k < 4; k++)
            {
                float ang = _t * 160f + k * 90f;
                Vector2 d = WDir(ang);
                DrawProp(Shape.Kunai, c.x + d.x * 0.7f, cy + d.y * 0.7f, ang + 90f, 0.35f * e, g);
            }

            BGroundRing(e, g, true);
        }

        void AnimSDebuffAttackDown()
        {
            Vector2 c = sweepCenter;
            float g = WFade(0.9f, 0.4f);
            if (Once(1)) Fire();
            float p = EaseOut(_t / 0.5f);
            float ang = Mathf.Lerp(90f, -35f, p * p);
            Vector2 d = WDir(ang);
            float len = 1.4f;
            if (_t < 0.55f)
            {
                DrawProp(Shape.Sword, c.x - 0.2f, c.y - 0.2f, ang, len, g);
                WTrail(new Vector2(c.x - 0.2f, c.y - 0.2f), 0.9f, ang, -Mathf.Min(50f, (90f - ang)), 0.18f, g * 0.7f);
            }
            else
            {
                float k = (_t - 0.55f) / 0.75f;
                float sz = len * 0.5f;
                for (int i = 0; i < 2; i++)
                {
                    float off = i == 0 ? 0f : 0.55f;
                    Vector2 bp = new Vector2(c.x - 0.2f, c.y - 0.2f) + d * (0.45f + off * 1.2f);
                    float fall = k * k * 1.0f;
                    DrawProp(Shape.Sword, bp.x + (i == 0 ? -0.3f : 0.3f) * k, bp.y - fall, ang + (i == 0 ? -80f : 70f) * k, sz * (i == 0 ? 1.0f : 0.7f), g * (1f - k * 0.5f));
                }
            }

            BArrows(-90f, g * 0.8f, 0.4f);
            BGroundRing(EaseOut(_t / 0.5f), g, false);
        }

        void AnimSDebuffDefenseDown()
        {
            Vector2 c = sweepCenter;
            float g = WFade(0.95f, 0.35f);
            if (Once(1)) Fire();
            float cy = c.y + 0.1f;
            if (_t < 0.5f)
            {
                float sh = Mathf.Sin(_t * 80f) * 0.03f * (_t / 0.5f);
                DrawProp(Shape.Hex, c.x + sh, cy, 90f, 1.2f, g);
                float k = _t / 0.5f;
                HStroke(c.x - 0.15f, cy + 0.5f * k, c.x + 0.1f, cy - 0.5f * k, 0.12f, 0.06f, 0.9f);
                HStroke(c.x + 0.1f, cy + 0.1f, c.x - 0.45f * k, cy - 0.2f * k, 0.05f, 0.05f, 0.8f);
            }
            else
            {
                float k = (_t - 0.5f) / 0.8f;
                HShards(10, c.x, cy, 4.0f, 0.5f, 0.5f, 0.8f, g, 31, -90f, 220f, Shape.Hex);
                HShards(8, c.x, cy, 3.0f, 0.35f, 0.5f, 0.8f, g, 32, -90f, 300f, Shape.Shard);
                int n = Ring(c.x, cy, 0.3f + 0.9f * EaseOut(k), 0.3f + 0.9f * EaseOut(k));
                DrawBrush(n, 0.07f, (1f - k) * g);
            }

            BArrows(-90f, g * 0.7f, 0.36f);
            BGroundRing(EaseOut(_t / 0.5f), g, false);
        }
    }
}
