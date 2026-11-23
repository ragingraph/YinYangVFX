using UnityEngine;

namespace YinYangVFX
{
    // Concrete charge / aura effects: converging kunai and flames, a glowing orb, wings, pillars, spinning taiji.
    // Origin = character body centre (sweepCenter). Looping: ChargeAura, Berserk, CharacterAura.
    public partial class WeaponEffect
    {
        void AnimSChargeAura()
        {
            Vector2 c = sweepCenter;
            float pk = _pulse;
            for (int k = 0; k < 8; k++)
            {
                float f = SLoop(k, 1, 0.9f);
                float ang = (k * 45f + Rnd(k, 2) * 20f);
                Vector2 d = WDir(ang);
                float r = 1.5f * (1f - f) + 0.25f;
                DrawProp(Shape.Kunai, c.x + d.x * r * 0.8f, c.y + d.y * r, ang + 180f, 0.4f, Mathf.Sin(f * Mathf.PI) * 0.95f);
            }

            for (int k = 0; k < 6; k++)
            {
                float x = c.x + (k - 2.5f) * 0.2f;
                EFlame(x, groundY + 0.35f, 0.7f - 0.05f * Mathf.Abs(k - 2.5f), 90f, 0.85f, k + 80);
            }

            for (int j = 0; j < 2; j++)
            {
                float kk = Mathf.Repeat(_t * 1.0f + j * 0.5f, 1f);
                float r = 1.1f * (1f - kk);
                int n = Ring(c.x, c.y, r * 0.8f, r);
                DrawBrush(n, 0.07f, kk * 0.9f);
            }

            DrawProp(Shape.Orb, c.x, c.y, 0f, 0.35f + 0.08f * Mathf.Sin(_t * 8f) + 0.8f * pk, 0.8f);
            DrawProp(Shape.Star5, c.x, c.y, 90f + _t * 120f, 0.25f + 0.9f * pk, 0.9f * pk);
            int gr = Ring(c.x, groundY, 0.7f, 0.14f);
            DrawBrush(gr, 0.08f, 0.9f);
        }

        void AnimSRageBurst()
        {
            Vector2 c = sweepCenter;
            float e = EaseOut(_t / 0.25f), g = WFade(0.6f, 0.6f);
            if (Once(1)) Fire();
            HStroke(c.x, groundY, c.x, c.y + 2.4f * e, 0f, 0.9f * (1f - 0.5f * e), g * (1f - e) * 0.9f);
            DrawProp(Shape.Star5, c.x, c.y, 90f + _t * 160f, 0.8f + 2.2f * e, WFade(0.25f, 0.35f));
            for (int k = 0; k < 10; k++)
            {
                float ang = k * 36f + 18f;
                Vector2 d = WDir(ang);
                float r = (0.4f + 0.6f * e) * (0.8f + 0.4f * Rnd(k, 1));
                EFlame(c.x + d.x * r, c.y + d.y * r, (0.6f + 0.9f * e) * (k % 2 == 0 ? 1f : 0.7f), ang, g, k + 90);
            }

            for (int j = 0; j < 2; j++)
            {
                float d = Mathf.Clamp01((_t - 0.07f * j) / 0.5f);
                float r = 2.4f * EaseOut(d) * (1f - 0.3f * j);
                int n = Ring(c.x, c.y, r, r);
                DrawBrush(n, 0.12f - 0.04f * j, (1f - d * 0.8f) * g);
            }

            int gr = Ring(c.x, groundY, 2.4f * e, 0.4f * e);
            DrawBrush(gr, 0.08f, (1f - e * 0.6f) * g);
            HShards(10, c.x, c.y, 7.0f, 0.4f, 0f, 0.7f, 1f, 41, 90f, 360f, Shape.Shard);
        }

        void AnimSAwaken()
        {
            Vector2 c = sweepCenter;
            float g = WFade(1.4f, 0.6f);
            float re = EaseOut(_t / 0.6f);
            DrawProp(Shape.Taiji, c.x, groundY + 0.02f, 90f + _t * 40f, 2.0f * re, 0.35f * g);
            int n = Ring(c.x, groundY, 1.0f * re, 0.2f * re);
            DrawBrush(n, 0.09f, g);
            if (_t > 0.35f && Once(1)) Fire();
            float pe = EaseOut((_t - 0.4f) / 0.5f);
            if (pe > 0f)
            {
                HStroke(c.x, groundY, c.x, groundY + 3.2f * pe, 0f, 0.8f, g * 0.55f);
                for (int sp = 0; sp < 2; sp++)
                {
                    const int m = 20;
                    for (int i = 0; i < m; i++)
                    {
                        float s = i / (float)(m - 1);
                        float a = _t * 4f + s * 8f + sp * Mathf.PI;
                        _buf[i] = new Vector3(c.x + 0.45f * Mathf.Cos(a), groundY + s * 3.0f * pe, 0f);
                    }

                    DrawBrush(m, 0.12f, g * 0.9f);
                }
            }

            float we = EaseOut((_t - 0.5f) / 0.5f);
            if (we > 0f)
            {
                for (int side = 0; side < 2; side++)
                {
                    float sg = side == 0 ? 1f : -1f;
                    for (int k = 0; k < 5; k++)
                    {
                        float ang = side == 0 ? 20f + k * 22f : 160f - k * 22f;
                        Vector2 d = WDir(ang);
                        DrawProp(Shape.Feather, c.x - sg * 0.12f + d.x * 0.1f * we, c.y + 0.25f + d.y * 0.1f, ang - 0f, (0.9f + 0.35f * (k % 3)) * we, g * 0.95f);
                    }
                }

                DrawProp(Shape.Star5, c.x, c.y + 1.5f * we, 90f + _t * 100f, 0.6f * we, g);
            }

            for (int k = 0; k < 6; k++)
            {
                float f = SLoop(k, 2, 0.5f);
                DrawProp(Shape.Feather, c.x + (Rnd(k, 3) - 0.5f) * 1.2f, groundY + f * 2.5f, 90f, 0.3f, Mathf.Sin(f * Mathf.PI) * g * re);
            }
        }

        void AnimSBerserk()
        {
            Vector2 c = sweepCenter;
            float pk = 1f + 0.3f * _pulse;
            float sx = 0.02f * Mathf.Sin(_t * 70f);
            for (int j = 0; j < 8; j++)
            {
                float x = c.x + (j - 3.5f) * 0.13f + sx;
                float h = (0.95f - 0.07f * Mathf.Abs(j - 3.5f)) * pk;
                EFlame(x, groundY + h * 0.5f, h, 90f + (j - 3.5f) * 3f, 0.95f, j + 100);
            }

            for (int j = 0; j < 4; j++)
            {
                float side = j % 2 == 0 ? -1f : 1f;
                float yy = c.y + 0.1f + 0.3f * (j / 2);
                EFlame(c.x + side * 0.4f, yy, 0.7f * pk, 90f + side * -30f, 0.85f, j + 120);
            }

            for (int k = 0; k < 4; k++)
            {
                float side = k % 2 == 0 ? -1f : 1f;
                DrawStanding(Shape.Shard, c.x + side * (0.65f + 0.25f * (k / 2)), groundY - 0.03f * 0.5f, 0.5f - 0.1f * (k / 2), 0.9f);
            }

            DrawProp(Shape.Skull, c.x, c.y + 1.1f + 0.04f * Mathf.Sin(_t * 9f), 90f, 0.4f, 0.8f);
            int gr = Ring(c.x, groundY, 0.8f, 0.16f);
            DrawBrush(gr, 0.09f, 0.9f);
        }

        void AnimSCharacterAura()
        {
            Vector2 c = sweepCenter;
            DrawProp(Shape.Taiji, c.x, c.y, 90f + _t * 30f, 1.7f * (1f + 0.04f * Mathf.Sin(_t * 3f)), 0.3f);
            int n = Ring(c.x, c.y, 0.75f, 0.95f);
            DrawBrush(n, 0.08f, 0.6f);
            for (int k = 0; k < 3; k++)
            {
                float ang = _t * 70f + k * 120f;
                Vector2 d = WDir(ang);
                DrawProp(k == 1 ? Shape.Orb : Shape.Star5, c.x + d.x * 0.75f, c.y + d.y * 0.95f, 90f + _t * 150f, 0.28f, 0.95f);
            }

            for (int k = 0; k < 5; k++)
            {
                float f = SLoop(k, 4, 0.4f);
                DrawProp(Shape.Feather, c.x + (Rnd(k, 5) - 0.5f) * 0.9f, c.y - 0.5f + f * 1.4f, 90f, 0.3f, Mathf.Sin(f * Mathf.PI) * 0.9f);
            }

            int gr = Ring(c.x, groundY, 0.7f, 0.14f);
            DrawBrush(gr, 0.08f, 0.9f);
        }
    }
}
