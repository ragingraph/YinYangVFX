using UnityEngine;

namespace YinYangVFX
{
    // Concrete UI effects: an ink light running round a button, reward burst of coins/stars, level-up arrows,
    // medal with wings, countdown ink ring, twinkling stars. Origin = element centre (world space).
    public partial class WeaponEffect
    {
        void AnimSButtonGlow()
        {
            Vector2 c = sweepCenter;
            float hw = 1.2f, hh = 0.42f;
            const int m = 24;
            for (int i = 0; i <= 4; i++)
            {
                Vector2 p = PerimPt(c.x, c.y, hw, hh, i * 0.25f + 0.0001f);
                _buf[i] = new Vector3(p.x, p.y, 0f);
            }

            DrawBrush(5, 0.07f, 0.5f + 0.15f * Mathf.Sin(_t * 3f));
            float s = _t * 0.5f;
            for (int i = 0; i < m; i++)
            {
                Vector2 p = PerimPt(c.x, c.y, hw, hh, s - 0.22f * (1f - i / (float)(m - 1)));
                _buf[i] = new Vector3(p.x, p.y, 0f);
            }

            Vector3 hp = _buf[m - 1];
            DrawBrush(m, 0.2f, 1f);
            DrawProp(Shape.Star5, hp.x, hp.y, 90f + _t * 300f, 0.32f, 1f);
        }

        void AnimSRewardBurst()
        {
            Vector2 c = sweepCenter;
            float g = WFade(1.1f, 0.5f);
            float e = EaseOut(_t / 0.25f);
            if (Once(1)) Fire();
            DrawProp(Shape.Star5, c.x, c.y, 90f + _t * 140f, 0.8f + 2.0f * e, WFade(0.2f, 0.6f));
            for (int k = 0; k < 12; k++)
            {
                float ang = k * 30f + 10f * Rnd(k);
                Vector2 d = WDir(ang);
                float f = Mathf.Clamp01(_t / 1.2f);
                float r = (0.3f + 2.2f * (1f - Mathf.Pow(1f - f, 3f))) * (0.7f + 0.3f * Rnd(k, 1));
                Shape sh = k % 3 == 0 ? Shape.Taiji : (k % 3 == 1 ? Shape.Orb : Shape.Star5);
                DrawProp(sh, c.x + d.x * r, c.y + d.y * r - 0.5f * f * f, 90f + _t * 240f, 0.32f, g);
            }

            for (int j = 0; j < 2; j++)
            {
                float d = Mathf.Clamp01((_t - 0.06f * j) / 0.6f);
                int n = Ring(c.x, c.y, 2.4f * EaseOut(d), 2.4f * EaseOut(d));
                DrawBrush(n, 0.1f - 0.03f * j, (1f - d) * g);
            }

            HShards(8, c.x, c.y, 5.0f, 0.3f, 0f, 0.8f, 1f, 300, 90f, 360f, Shape.Star5);
        }

        void AnimSUILevelUp()
        {
            Vector2 c = sweepCenter;
            float g = WFade(1.4f, 0.4f);
            float e = EaseOut(_t / 0.35f);
            if (Once(1)) Fire();
            DrawProp(Shape.Star5, c.x, c.y + 0.3f, 90f + _t * 120f, 1.0f + 1.4f * e, g);
            for (int col = 0; col < 5; col++)
            {
                for (int k = 0; k < 2; k++)
                {
                    float f = Mathf.Repeat(_t * 0.9f + k * 0.5f + col * 0.13f, 1f);
                    DrawProp(Shape.Arrow, c.x + (col - 2) * 0.7f, c.y - 1.0f + f * 2.8f, 90f, 0.6f, Mathf.Sin(f * Mathf.PI) * g);
                }
            }

            for (int j = 0; j < 3; j++)
            {
                float d = Mathf.Clamp01((_t - 0.1f * j) / 0.9f);
                int n = Ring(c.x, c.y, 3.0f * EaseOut(d), 1.2f * EaseOut(d));
                DrawBrush(n, 0.1f, (1f - d) * g);
            }

            HStroke(c.x - 2.8f * e, c.y - 0.9f, c.x + 2.8f * e, c.y - 0.9f, 0f, 0.2f, g);
        }

        void AnimSAchievement()
        {
            Vector2 c = sweepCenter;
            float g = WFade(1.5f, 0.5f);
            float R = 0.8f;
            float k = Mathf.Clamp01(_t / 0.6f);
            int n = ArcPts(c.x, c.y, R, R, 90f, 90f - 360f * EaseOut(k), 40);
            DrawBrush(n, 0.16f, g);
            if (k >= 1f && Once(1)) Fire();
            float pe = EaseOut((_t - 0.4f) / 0.3f);
            if (pe > 0f)
            {
                DrawProp(Shape.Taiji, c.x, c.y, 90f + _t * 60f, 1.0f * pe, g);
                DrawProp(Shape.Star5, c.x, c.y + R + 0.35f, 90f + _t * 100f, 0.5f * pe, g);
                for (int side = 0; side < 2; side++)
                {
                    float sg = side == 0 ? 1f : -1f;
                    for (int i = 0; i < 4; i++)
                    {
                        float ang = side == 0 ? 10f + i * 18f : 170f - i * 18f;
                        DrawProp(Shape.Feather, c.x + sg * (R + 0.1f), c.y, ang, (0.9f + 0.2f * (i % 2)) * pe, g);
                    }
                }
            }

            HShards(8, c.x, c.y, 4.0f, 0.25f, 0.6f, 0.8f, 1f, 310, 90f, 360f, Shape.Star5);
        }

        void AnimSCountdownRing()
        {
            Vector2 c = sweepCenter;
            float R = 0.9f;
            float rem = 1f - Mathf.Clamp01(_t / duration);
            int n = Ring(c.x, c.y, R, R);
            DrawBrush(n, 0.06f, 0.3f);
            if (rem > 0.001f)
            {
                n = ArcPts(c.x, c.y, R, R, 90f, 90f + 360f * rem, 48);
                Vector3 hp = _buf[n - 1];
                DrawBrush(n, 0.22f, 1f);
                DrawProp(Shape.Star5, hp.x, hp.y, 90f + _t * 400f, 0.32f, 1f);
            }

            for (int i = 0; i < 12; i++)
            {
                float ang = 90f + i * 30f;
                Vector2 d = WDir(ang);
                DrawProp(Shape.Shard, c.x + d.x * (R + 0.2f), c.y + d.y * (R + 0.2f), ang, 0.16f, 0.6f);
            }

            float tk = Mathf.Repeat(_t, 1f);
            int pn = Ring(c.x, c.y, R * (1f + 0.1f * tk), R * (1f + 0.1f * tk));
            DrawBrush(pn, 0.04f, (1f - tk) * 0.5f);
        }

        void AnimSSparkleLoop()
        {
            Vector2 c = sweepCenter;
            for (int i = 0; i < 10; i++)
            {
                float f = SLoop(i, 1, 0.55f);
                float x = c.x + (Rnd(i, 2) - 0.5f) * 2.4f;
                float y = c.y + (Rnd(i, 3) - 0.5f) * 1.4f;
                DrawProp(Shape.Star5, x, y, 90f + f * 180f, (0.2f + 0.3f * Rnd(i, 4)) * Mathf.Sin(f * Mathf.PI), Mathf.Sin(f * Mathf.PI));
            }
        }
    }
}
