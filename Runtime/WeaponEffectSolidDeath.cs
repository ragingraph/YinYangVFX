using UnityEngine;

namespace YinYangVFX
{
    // Concrete death effects: shards and rocks, ash flakes, rising feathers and light, a soul with a trailing tail,
    // a multi-stage boss explosion. Origin = enemy body centre (sweepCenter).
    public partial class WeaponEffect
    {
        // Slowly drifting flakes rising from a body area. dx = wind sideways, rise = speed up.
        void DFlakes(int n, float t0, float life, float spread, float rise, float wind, float size, Shape sh, float a, int salt)
        {
            Vector2 c = sweepCenter;
            float tt = _t - t0;
            if (tt <= 0f || tt >= life) return;
            for (int i = 0; i < n; i++)
            {
                float st = Rnd(i, salt) * life * 0.4f;
                float k = Mathf.Clamp01((tt - st) / (life * 0.6f));
                if (k <= 0f) continue;
                float x = c.x + (Rnd(i, salt + 1) - 0.5f) * spread + wind * k + 0.1f * Mathf.Sin(k * 6f + i);
                float y = c.y + (Rnd(i, salt + 2) - 0.5f) * 1.0f + rise * k * (0.5f + Rnd(i, salt + 3));
                float sz = size * (0.6f + 0.8f * Rnd(i, salt + 4));
                DrawProp(sh, x, y, Rnd(i, salt + 5) * 360f + k * 200f, sz * (1f - 0.4f * k), a * Mathf.Sin((1f - k) * Mathf.PI * 0.5f));
            }
        }

        void AnimSDeathMonster()
        {
            Vector2 c = sweepCenter;
            float e = EaseOut(_t / 0.12f), g = WFade(0.4f, 0.6f);
            if (Once(1)) Fire();
            DrawProp(Shape.Star5, c.x, c.y, 90f + _t * 180f, 0.5f + 1.6f * e, WFade(0.1f, 0.3f));
            for (int k = 0; k < 8; k++)
            {
                float a = (k * 45f + 10f) * Mathf.Deg2Rad;
                float L = (k % 2 == 0 ? 1.3f : 0.8f) * e;
                HStroke(c.x + Mathf.Cos(a) * 0.2f, c.y + Mathf.Sin(a) * 0.2f, c.x + Mathf.Cos(a) * (0.2f + L), c.y + Mathf.Sin(a) * (0.2f + L), 0f, 0.14f, g);
            }

            float re = EaseOut(_t / 0.4f);
            int rn = Ring(c.x, c.y, 1.4f * re, 1.4f * re);
            DrawBrush(rn, 0.1f, (1f - re * 0.7f) * g);
            HShards(10, c.x, c.y, 6.0f, 0.45f, 0f, 0.7f, 1f, 51, 90f, 360f, Shape.Shard);
            HShards(6, c.x, c.y, 4.0f, 0.4f, 0f, 0.8f, 1f, 52, 90f, 200f, Shape.Rock);
            DrawProp(Shape.Skull, c.x, c.y + 0.3f + 0.6f * EaseOut(_t / 0.8f), 90f, 0.7f * EaseOut(_t / 0.12f), WFade(0.3f, 0.6f));
        }

        void AnimSDeathAsh()
        {
            Vector2 c = sweepCenter;
            if (Once(1)) Fire();
            DFlakes(26, 0.1f, 1.8f, 0.8f, 1.3f, 1.4f, 0.22f, Shape.Rock, 0.9f, 61);
            DFlakes(18, 0.1f, 1.8f, 0.8f, 1.8f, 1.8f, 0.3f, Shape.Shard, 0.8f, 71);
            float p = EaseOut(_t / 0.7f);
            float g = WFade(1.2f, 0.8f);
            EFlame(c.x, groundY + 0.25f, 0.6f * (1f - p * 0.3f), 90f, g * 0.9f, 7);
            EFlame(c.x - 0.25f, groundY + 0.2f, 0.4f, 95f, g * 0.8f, 8);
            EFlame(c.x + 0.25f, groundY + 0.2f, 0.4f, 85f, g * 0.8f, 9);
            HStroke(c.x - 0.6f * p, groundY + 0.03f, c.x + 0.6f * p, groundY + 0.03f, 0.06f, 0.2f * p, 0.9f * WFade(1.4f, 0.6f));
        }

        void AnimSDeathLight()
        {
            Vector2 c = sweepCenter;
            float g = WFade(1.3f, 0.5f);
            if (Once(1)) Fire();
            float pe = EaseOut(_t / 0.4f);
            HStroke(c.x, groundY, c.x, c.y + 2.6f * pe, 0f, 0.7f * (1f - 0.3f * pe), g * 0.55f);
            DrawProp(Shape.Star5, c.x, c.y, 90f + _t * 120f, 0.6f + 1.0f * pe, WFade(0.4f, 0.5f));
            for (int k = 0; k < 8; k++)
            {
                float f = SLoop(k, 1, 0.55f);
                float x = c.x + (Rnd(k, 2) - 0.5f) * 0.8f;
                DrawProp(Shape.Feather, x, c.y - 0.4f + f * 2.4f * pe, 90f, 0.4f + 0.2f * Rnd(k, 3), Mathf.Sin(f * Mathf.PI) * g);
            }

            for (int k = 0; k < 4; k++)
            {
                float f = SLoop(k, 4, 0.7f);
                DrawProp(Shape.Orb, c.x + (Rnd(k, 5) - 0.5f) * 0.6f, c.y + f * 2.0f, 0f, 0.22f, Mathf.Sin(f * Mathf.PI) * g);
            }

            int n = Ring(c.x, groundY, 1.0f * pe, 0.2f * pe);
            DrawBrush(n, 0.08f, g);
        }

        void AnimSDeathSoul()
        {
            Vector2 c = sweepCenter;
            float g = WFade(1.7f, 0.5f);
            if (Once(1)) Fire();
            float p = Mathf.Clamp01(_t / 2.0f);
            for (int s = 0; s < 3; s++)
            {
                float st = 0.12f * s;
                float k = Mathf.Clamp01((_t - st) / 1.9f);
                if (k <= 0f) continue;
                float ox = (s - 1) * 0.35f;
                float y = c.y + 0.1f + 2.6f * k * k * (0.5f + 0.5f * k) + 0.2f * k;
                float x = c.x + ox + 0.18f * Mathf.Sin(k * 9f + s * 2f);
                float sz = (s == 1 ? 0.95f : 0.6f) * (1f - 0.25f * k);
                const int m = 14;
                for (int i = 0; i < m; i++)
                {
                    float f = i / (float)(m - 1);
                    float yy = Mathf.Lerp(y - 0.9f * sz - 0.4f, y, f);
                    float kk = Mathf.Clamp01(k - (1f - f) * 0.1f);
                    _buf[i] = new Vector3(c.x + ox + 0.18f * Mathf.Sin(kk * 9f + s * 2f), yy, 0f);
                }

                DrawBrush(m, 0.16f * sz, g * 0.7f);
                DrawProp(Shape.Drop, x, y, 90f, sz, g * (s == 1 ? 1f : 0.85f));
            }

            DrawProp(Shape.Skull, c.x, c.y + 0.3f + 0.2f * p, 90f, 0.6f, WFade(0.25f, 0.5f));
            int n = Ring(c.x, groundY, 0.7f * EaseOut(_t / 0.5f), 0.14f * EaseOut(_t / 0.5f));
            DrawBrush(n, 0.07f, WFade(0.5f, 1.2f));
        }

        void AnimSDeathBoss()
        {
            Vector2 c = sweepCenter;
            float g = WFade(2.5f, 0.5f);
            if (_t < 1.4f)
            {
                float k = _t / 1.4f;
                float sh = Mathf.Sin(_t * 90f) * 0.04f * k;
                for (int i = 0; i < 5; i++)
                {
                    float a = (i * 72f + 20f) * Mathf.Deg2Rad;
                    float L = (0.5f + 0.5f * Rnd(i, 1)) * k * 1.4f;
                    HStroke(c.x + sh, c.y, c.x + sh + Mathf.Cos(a) * L, c.y + Mathf.Sin(a) * L, 0.06f, 0.08f, 0.9f);
                }

                int nb = Mathf.FloorToInt(_t * 9f);
                for (int j = 0; j < 3; j++)
                {
                    float st = (nb - j) / 9f;
                    float f = (_t - st) * 9f;
                    if (f < 0f || f > 1f) continue;
                    float bx = c.x + (Rnd(nb - j, 1) - 0.5f) * 1.6f, by = c.y + (Rnd(nb - j, 2) - 0.5f) * 1.2f;
                    DrawProp(Shape.Star5, bx, by, 90f + f * 120f, 0.8f * Mathf.Sin(f * Mathf.PI), 0.9f);
                    EFlame(bx, by, 0.7f * Mathf.Sin(f * Mathf.PI), 90f, 0.9f, nb - j);
                }
            }
            else
            {
                if (Once(1)) Fire();
                float k = (_t - 1.4f) / 1.6f;
                float e = EaseOut((_t - 1.4f) / 0.3f);
                DrawProp(Shape.Star5, c.x, c.y, 90f + _t * 100f, 1.2f + 4.0f * e, WFade(1.7f, 0.6f));
                HStroke(c.x, groundY, c.x, c.y + 4.0f * e, 0f, 1.4f * (1f - 0.5f * e), g * (1f - e * 0.6f));
                for (int j = 0; j < 3; j++)
                {
                    float d = Mathf.Clamp01((_t - 1.4f - 0.1f * j) / 1.2f);
                    float r = 4.0f * EaseOut(d);
                    int n = Ring(c.x, c.y, r, r);
                    DrawBrush(n, 0.18f - 0.05f * j, (1f - d) * g);
                }

                int gr = Ring(c.x, groundY, 4.0f * e, 0.6f * e);
                DrawBrush(gr, 0.1f, g);
                HShards(18, c.x, c.y, 9.0f, 0.7f, 1.4f, 1.4f, 1f, 81, 90f, 360f, Shape.Shard);
                HShards(12, c.x, c.y, 6.0f, 0.65f, 1.4f, 1.5f, 1f, 82, 90f, 240f, Shape.Rock);
                DrawProp(Shape.Skull, c.x, c.y + 0.4f + 1.4f * EaseOut(k), 90f, 1.3f * EaseOut((_t - 1.5f) / 0.2f), WFade(2.0f, 0.9f));
            }
        }
    }
}
