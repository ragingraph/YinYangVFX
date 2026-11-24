using UnityEngine;

namespace YinYangVFX
{
    // Concrete summon / teleport effects: ink magic circle with solid rune plates, a spinning taiji portal,
    // feather and star bursts, healing cross, climbing arrows. Origin = ground spot / body centre (sweepCenter).
    public partial class WeaponEffect
    {
        void SCircle(float cx, float cy, float rx, float a, float rot)
        {
            float ry = rx * 0.22f;
            int n = Ring(cx, cy, rx, ry);
            DrawBrush(n, 0.09f, a);
            n = Ring(cx, cy, rx * 0.7f, ry * 0.7f);
            DrawBrush(n, 0.06f, a * 0.8f);
            for (int k = 0; k < 6; k++)
            {
                float ang = (rot + k * 60f) * Mathf.Deg2Rad;
                float x = cx + Mathf.Cos(ang) * rx * 0.86f, y = cy + Mathf.Sin(ang) * ry * 0.86f;
                DrawProp(k % 2 == 0 ? Shape.Hex : Shape.Star5, x, y, 90f, 0.22f * Mathf.Min(1f, rx), a);
            }
        }

        void SPillar(float x, float yTop, float w, float a)
        {
            HStroke(x, groundY, x, yTop, 0f, w, a);
        }

        void AnimSSummonCircle()
        {
            Vector2 c = sweepCenter;
            float x = c.x;
            float open = EaseOut(_t / 0.5f);
            float g = WFade(1.4f, 0.4f);
            SCircle(x, groundY, 1.1f * open, g, _t * 80f);
            if (_t > 0.45f)
            {
                float k = EaseOut((_t - 0.45f) / 0.5f);
                if (Once(1)) Fire();
                SPillar(x, groundY + 2.6f * k, 0.8f * (1f - 0.4f * k), g * 0.5f);
                DrawProp(Shape.Taiji, x, groundY + 0.2f + 1.0f * k, 90f + _t * 120f, 1.1f * k, g);
                for (int j = 0; j < 4; j++)
                {
                    float side = j % 2 == 0 ? -1f : 1f;
                    EFlame(x + side * (0.3f + 0.2f * (j / 2)), groundY + 0.3f, 0.7f * k, 90f, g * 0.9f, j + 140);
                }

                HShards(8, x, groundY + 0.3f, 4.0f, 0.3f, 0.45f, 0.8f, 1f, 91, 90f, 120f, Shape.Shard);
            }
        }

        void AnimSPortalOpen()
        {
            Vector2 c = sweepCenter;
            float x = c.x, y = c.y + 0.1f;
            float open = EaseOut(_t / 0.4f);
            float ry = 0.95f * open, rx = 0.4f * open;
            int n = Ring(x, y, rx * 1.2f, ry * 1.1f);
            DrawBrush(n, 0.18f, 0.55f);
            n = Ring(x, y, rx, ry);
            DrawBrush(n, 0.1f, 0.95f);
            DrawProp(Shape.Taiji, x, y, 90f + _t * 140f, 1.1f * open, 0.65f);
            for (int k = 0; k < 4; k++)
            {
                float ang = (_t * 120f + k * 90f) * Mathf.Deg2Rad;
                DrawProp(Shape.Star5, x + Mathf.Cos(ang) * rx * 1.2f, y + Mathf.Sin(ang) * ry * 1.1f, 90f + _t * 200f, 0.22f * open, 0.95f);
            }

            for (int k = 0; k < 3; k++)
            {
                int m = ArcPts(x, y, rx * (0.4f + 0.2f * k), ry * (0.4f + 0.2f * k), _t * 300f * (k % 2 == 0 ? 1f : -1f) + k * 100f, _t * 300f * (k % 2 == 0 ? 1f : -1f) + k * 100f + 150f, 16);
                DrawBrush(m, 0.07f, 0.8f);
            }
        }

        void AnimSTeleportOut()
        {
            Vector2 c = sweepCenter;
            float k = Mathf.Clamp01(_t / 0.35f);
            float g = WFade(0.4f, 0.4f);
            if (Once(1)) Fire();
            float w = Mathf.Lerp(0.8f, 0.08f, k);
            HStroke(c.x, c.y - 0.6f * (1f - 0.3f * k), c.x, c.y + 0.6f + 1.8f * k * k, 0f, w, g);
            DrawProp(Shape.Star5, c.x, c.y, 90f + _t * 400f, 1.4f * Mathf.Sin(Mathf.Clamp01(_t / 0.3f) * Mathf.PI), 1f);
            for (int i = 0; i < 8; i++)
            {
                float f = Mathf.Clamp01((_t - 0.05f) / 0.6f);
                float x = c.x + (Rnd(i, 1) - 0.5f) * 0.7f;
                DrawProp(Shape.Feather, x, c.y - 0.4f + f * (1.0f + 2.0f * Rnd(i, 2)), 90f, 0.4f, (1f - f) * 0.95f);
            }

            int n = Ring(c.x, groundY, 0.6f * EaseOut(_t / 0.4f), 0.12f * EaseOut(_t / 0.4f));
            DrawBrush(n, 0.07f, g);
        }

        void AnimSTeleportIn()
        {
            Vector2 c = sweepCenter;
            float k = Mathf.Clamp01(_t / 0.3f);
            float g = WFade(0.5f, 0.3f);
            float w = Mathf.Lerp(0.08f, 0.8f, k);
            if (_t < 0.3f)
            {
                HStroke(c.x, c.y + 2.6f - 2.0f * k * k, c.x, c.y - 0.6f, 0f, w, 1f);
            }
            else if (Once(1)) Fire();
            float e = EaseOut((_t - 0.3f) / 0.2f);
            if (_t >= 0.3f)
            {
                DrawProp(Shape.Star5, c.x, c.y, 90f + _t * 300f, 0.6f + 1.2f * e, WFade(0.35f, 0.3f));
                HStroke(c.x, c.y - 0.6f, c.x, c.y + 0.9f, 0f, 0.5f * (1f - e), g);
            }

            for (int i = 0; i < 8; i++)
            {
                float f = Mathf.Clamp01(_t / 0.6f);
                float x = c.x + (Rnd(i, 1) - 0.5f) * 0.7f;
                DrawProp(Shape.Feather, x, c.y + 0.9f + (1.0f + 1.5f * Rnd(i, 2)) * (1f - f), -90f, 0.4f, Mathf.Sin(f * Mathf.PI) * 0.9f);
            }

            int n = Ring(c.x, groundY, 0.8f * e, 0.16f * e);
            DrawBrush(n, 0.08f, g);
        }

        void AnimSRevive()
        {
            Vector2 c = sweepCenter;
            float x = c.x;
            float g = WFade(1.4f, 0.8f);
            float rise = EaseOut(_t / 0.6f);
            SCircle(x, groundY, 0.9f * rise, g, _t * 60f);
            if (_t > 0.4f && Once(1)) Fire();
            float pk = EaseOut((_t - 0.3f) / 0.6f);
            SPillar(x, groundY + 2.8f * pk, 0.7f, g * 0.5f);
            DrawProp(Shape.Cross, x, c.y + 0.1f + 0.3f * pk, 90f, 1.3f * pk * (1f + 0.05f * Mathf.Sin(_t * 8f)), g);
            for (int k = 0; k < 8; k++)
            {
                float f = SLoop(k, 6, 0.4f);
                DrawProp(Shape.Feather, x + (Rnd(k, 7) - 0.5f) * 1.0f, groundY + f * 2.6f, 90f, 0.35f, Mathf.Sin(f * Mathf.PI) * g * pk);
            }

            for (int j = 0; j < 3; j++)
            {
                float f = Mathf.Repeat(_t * 0.6f + j / 3f, 1f);
                int n = Ring(x, groundY + f * 2.2f, 0.55f * (1f - f * 0.3f), 0.1f);
                DrawBrush(n, 0.06f, Mathf.Sin(f * Mathf.PI) * g * pk);
            }
        }

        void AnimSLevelUp()
        {
            Vector2 c = sweepCenter;
            float x = c.x;
            float g = WFade(1.4f, 0.6f);
            float e = EaseOut(_t / 0.35f);
            if (Once(1)) Fire();
            SPillar(x, c.y + 2.4f * e, 0.75f * (1f - 0.3f * e), g * 0.5f);
            DrawProp(Shape.Star5, x, c.y + 0.3f + 1.4f * e, 90f + _t * 140f, 0.5f + 0.8f * e, g);
            for (int j = 0; j < 5; j++)
            {
                float f = Mathf.Clamp01((_t - 0.12f * j) / 1.0f);
                if (f <= 0f) continue;
                int n = Ring(x, groundY + f * 2.2f, 0.5f, 0.1f);
                DrawBrush(n, 0.07f, Mathf.Sin(f * Mathf.PI) * g);
            }

            for (int col = 0; col < 3; col++)
            {
                for (int k = 0; k < 2; k++)
                {
                    float f = Mathf.Repeat(_t * 0.8f + k * 0.5f + col * 0.17f, 1f);
                    DrawProp(Shape.Arrow, x + (col - 1) * 0.45f, groundY + f * 2.3f, 90f, 0.5f, Mathf.Sin(f * Mathf.PI) * g);
                }
            }

            HShards(10, x, c.y, 6.0f, 0.3f, 0f, 0.8f, 1f, 95, 90f, 360f, Shape.Star5);
            int gr = Ring(x, groundY, 1.1f * e, 0.22f * e);
            DrawBrush(gr, 0.09f, g);
        }
    }
}
