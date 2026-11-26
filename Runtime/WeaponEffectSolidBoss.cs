using UnityEngine;

namespace YinYangVFX
{
    // Concrete boss / scene effects: ground cracks with rising rocks and flame wall, a rune warning circle,
    // arrow lane warning, taiji phase change, ink wipe scene transition, rock-spike earthquake.
    // Origin = boss feet / scene centre (ground at groundY).
    public partial class WeaponEffect
    {
        void AnimSBossEntrance()
        {
            Vector2 c = sweepCenter;
            float x = c.x;
            float g = WFade(1.9f, 0.5f);
            float e = EaseOut(_t / 0.5f);
            if (_t > 0.5f && Once(1)) Fire();
            for (int i = 0; i < 6; i++)
            {
                GroundCrack(x, i % 2 == 0 ? 1f : -1f, (1.2f + 1.4f * (i / 2)) * e * 1.6f, i + 3, 0.07f, g);
            }

            for (int j = 0; j < 2; j++)
            {
                float d = Mathf.Clamp01((_t - 0.2f * j) / 1.0f);
                int n = Ring(x, groundY, 4.0f * EaseOut(d), 0.7f * EaseOut(d));
                DrawBrush(n, 0.12f - 0.04f * j, (1f - d) * g);
            }

            float pk = EaseOut((_t - 0.3f) / 0.5f);
            if (pk > 0f)
            {
                HStroke(x, groundY, x, groundY + 4.5f * pk, 0f, 1.6f, g * 0.45f);
                for (int k = 0; k < 9; k++)
                {
                    float xx = x + (k - 4f) * 0.38f;
                    EFlame(xx, groundY + 0.8f * pk, (1.6f - 0.18f * Mathf.Abs(k - 4f)) * pk, 90f, g * 0.9f, k + 260);
                }

                DrawProp(Shape.Skull, x, groundY + 2.2f + 0.4f * pk, 90f, 2.0f * EaseOut((_t - 0.6f) / 0.5f), g * 0.9f);
            }

            for (int k = 0; k < 8; k++)
            {
                float f = SLoop(k, 7, 0.5f);
                DrawProp(Shape.Rock, x + (Rnd(k, 8) - 0.5f) * 3.0f, groundY + 0.2f + f * 2.8f, Rnd(k, 9) * 360f + f * 120f, 0.35f + 0.25f * Rnd(k, 10), Mathf.Sin(f * Mathf.PI) * g * e);
            }
        }

        void AnimSWarningCircle()
        {
            Vector2 c = sweepCenter;
            float R = 1.8f;
            float cyc = Mathf.Repeat(_t, 1.0f);
            float fl = 0.55f + 0.45f * Mathf.Sin(_t * 12f);
            int n = Ring(c.x, groundY, R, R * 0.22f);
            DrawBrush(n, 0.16f, fl);
            n = Ring(c.x, groundY, R * cyc, R * 0.22f * cyc);
            DrawBrush(n, 0.09f, 1f - cyc * 0.5f);
            for (int k = 0; k < 8; k++)
            {
                float a = (k * 45f + _t * 20f) * Mathf.Deg2Rad;
                DrawProp(Shape.Hex, c.x + Mathf.Cos(a) * R * 0.9f, groundY + Mathf.Sin(a) * R * 0.2f, 90f, 0.3f, fl);
            }

            DrawProp(Shape.Skull, c.x, groundY + 0.6f, 90f, 0.7f, fl);
        }

        void AnimSDashWarning()
        {
            Vector2 c = sweepCenter;
            float L = 6.5f, hw = 0.35f;
            float fl = 0.55f + 0.45f * Mathf.Sin(_t * 14f);
            HStroke(c.x, groundY + hw, c.x + L, groundY + hw, 0f, 0.1f, fl);
            HStroke(c.x, groundY - hw * 0.2f, c.x + L, groundY - hw * 0.2f, 0f, 0.1f, fl);
            for (int i = 0; i < 5; i++)
            {
                float f = Mathf.Repeat(_t * 1.4f + i / 5f, 1f);
                DrawProp(Shape.Arrow, c.x + f * L, groundY + hw * 0.4f, 0f, 0.7f, Mathf.Sin(f * Mathf.PI) * fl + 0.2f);
            }

            DrawProp(Shape.Skull, c.x + L, groundY + hw * 0.4f, 90f, 0.5f, fl);
        }

        void AnimSBossPhaseChange()
        {
            Vector2 c = sweepCenter + new Vector2(0f, 1.0f);
            float g = WFade(1.9f, 0.5f);
            float e = EaseOut(_t / 0.6f);
            if (_t > 0.8f && Once(1)) Fire();
            DrawProp(Shape.Taiji, c.x, c.y, 90f + _t * 180f, 4.0f * e, g * 0.8f);
            DrawProp(Shape.Star5, c.x, c.y, 90f - _t * 120f, 2.0f + 3.0f * Mathf.Sin(Mathf.Clamp01((_t - 0.7f) / 0.6f) * Mathf.PI), g);
            for (int k = 0; k < 10; k++)
            {
                float ang = k * 36f + _t * 50f;
                Vector2 d = WDir(ang);
                EFlame(c.x + d.x * 2.6f * e, c.y + d.y * 2.6f * e, 1.1f, ang, g * 0.9f, k + 280);
            }

            for (int j = 0; j < 3; j++)
            {
                float d = Mathf.Clamp01((_t - 0.8f - 0.1f * j) / 1.0f);
                int n = Ring(c.x, c.y, 5.0f * EaseOut(d), 5.0f * EaseOut(d));
                DrawBrush(n, 0.16f - 0.04f * j, (1f - d) * g);
            }

            HStroke(c.x, groundY, c.x, c.y + 3.5f * e, 0f, 1.2f, g * 0.4f);
            HShards(16, c.x, c.y, 9.0f, 0.6f, 0.8f, 1.2f, 1f, 270, 90f, 360f, Shape.Shard);
        }

        void AnimSSceneTransition()
        {
            Vector2 c = sweepCenter;
            float g = WFade(1.0f, 0.6f);
            if (_t >= 0.3f && Once(1)) Fire();
            for (int i = 0; i < 5; i++)
            {
                float st = 0.06f * i;
                float k = EaseOut((_t - st) / 0.45f);
                if (k <= 0f) continue;
                float y = c.y + (i - 2f) * 1.1f;
                HStroke(c.x - 10f, y, c.x - 10f + 20f * k, y, 0.05f * (i % 2 == 0 ? 1f : -1f), 1.5f, g);
            }

            float te = Mathf.Sin(Mathf.Clamp01((_t - 0.2f) / 1.1f) * Mathf.PI);
            DrawProp(Shape.Taiji, c.x, c.y, 90f + _t * 160f, 6.5f * te, g);
            DrawProp(Shape.Star5, c.x, c.y, 90f, 3.0f * Mathf.Sin(Mathf.Clamp01((_t - 0.3f) / 0.4f) * Mathf.PI), 1f);
        }

        void AnimSEarthShake()
        {
            Vector2 c = sweepCenter;
            float x = c.x;
            float g = WFade(1.2f, 0.6f);
            float e = EaseOut(_t / 0.3f);
            if (Once(1)) Fire();
            for (int i = 0; i < 8; i++)
            {
                GroundCrack(x, i % 2 == 0 ? 1f : -1f, (1.0f + 1.5f * (i / 2)) * e * 1.5f, i + 5, 0.07f, g);
            }

            for (int i = 0; i < 9; i++)
            {
                float ts = 0.04f * Mathf.Abs(i - 4);
                float k = Mathf.Clamp01((_t - ts) / 0.15f);
                if (k <= 0f) continue;
                float len = 1.0f + 0.4f * Rnd(i, 2);
                float up = 1f - Mathf.Pow(1f - k, 4f);
                float down = 1f - Mathf.Clamp01((_t - 0.7f) / 0.5f);
                DrawStanding(Shape.Rock, x + (i - 4f) * 0.9f, groundY - len + len * 0.97f * up * down, len, g);
            }

            for (int j = 0; j < 2; j++)
            {
                float d = Mathf.Clamp01((_t - 0.1f * j) / 0.9f);
                int n = Ring(x, groundY, 5.0f * EaseOut(d), 0.8f * EaseOut(d));
                DrawBrush(n, 0.12f - 0.04f * j, (1f - d) * g);
            }

            HShards(14, x, groundY + 0.2f, 6.0f, 0.5f, 0.05f, 0.9f, 1f, 290, 90f, 120f, Shape.Rock);
            HShards(10, x - 2f, groundY + 0.2f, 5.0f, 0.35f, 0.1f, 0.9f, 1f, 291, 90f, 140f, Shape.Shard);
            HShards(10, x + 2f, groundY + 0.2f, 5.0f, 0.35f, 0.1f, 0.9f, 1f, 292, 90f, 140f, Shape.Shard);
        }
    }
}
