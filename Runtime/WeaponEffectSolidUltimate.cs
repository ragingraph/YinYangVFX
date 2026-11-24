using UnityEngine;

namespace YinYangVFX
{
    // Concrete ultimate effects (large scale): a rain of real swords, a screen-wide ink slash, fire meteors,
    // a giant taiji with trigram plates, an ink dragon, an expanding barrier of plates.
    public partial class WeaponEffect
    {
        void AnimSSkySwordRain()
        {
            float x0 = sweepCenter.x;
            float g = WFade(1.6f, 0.8f);
            const int N = 14;
            for (int i = 0; i < N; i++)
            {
                float ts = 0.06f * i + 0.05f * Rnd(i, 3);
                float p = (_t - ts) / 0.22f;
                if (p < 0f) continue;
                float x = x0 + (i - (N - 1) * 0.5f) * 0.32f + (Rnd(i, 5) - 0.5f) * 0.15f;
                float len = 1.4f + 0.5f * Rnd(i, 7);
                float tipEnd = groundY - 0.05f;
                if (p < 1f)
                {
                    float tip = Mathf.Lerp(groundY + 5.0f, tipEnd, p * p);
                    HStroke(x, tip + len * 2.4f, x, tip + len * 0.7f, 0f, 0.2f, 0.6f);
                    DrawProp(Shape.Sword, x, tip + len * 0.8f, -90f, len, 1f);
                }
                else
                {
                    float f = Mathf.Clamp01((_t - ts - 0.22f) / 1.2f);
                    DrawProp(Shape.Sword, x, tipEnd + len * 0.8f, -90f, len, (1f - f) * g);
                    int n = Ring(x, groundY, 0.55f * EaseOut(f * 3f), 0.11f * EaseOut(f * 3f));
                    DrawBrush(n, 0.05f, (1f - Mathf.Clamp01(f * 2f)) * g);
                    if (Once(1 << (i + 2))) Fire();
                    HShards(3, x, groundY + 0.1f, 4.0f, 0.22f, ts + 0.22f, 0.5f, 1f, 110 + i, 90f, 120f, Shape.Shard);
                }
            }
        }

        void AnimSFullScreenSlash()
        {
            Vector2 c = sweepCenter;
            float g = WFade(0.7f, 0.3f);
            float k = EaseOut(_t / 0.14f);
            if (Once(1)) Fire();
            const int m = 24;
            float len = 11.0f * k;
            for (int i = 0; i < m; i++)
            {
                float s = i / (float)(m - 1);
                float x = c.x + 0.5f + len * s;
                float y = c.y + 0.3f + 1.2f * Mathf.Sin(s * Mathf.PI) * (1f - 0.3f * s) - 0.6f * s;
                _buf[i] = new Vector3(x, y, 0f);
            }

            DrawBrush(m, 2.0f * (1f - 0.5f * Mathf.Clamp01((_t - 0.2f) / 0.5f)), g);
            for (int i = 0; i < m; i++)
            {
                _buf[i] += new Vector3(-0.3f, -0.45f, 0f);
            }

            DrawBrush(m, 0.7f, g * 0.8f);
            float fx = c.x + 4.5f;
            DrawProp(Shape.Star5, fx, c.y, 90f + _t * 200f, 3.2f * Mathf.Sin(Mathf.Clamp01(_t / 0.4f) * Mathf.PI), 1f);
            HStroke(fx, groundY, fx, c.y + 3.0f * k, 0f, 0.3f * (1f - k * 0.5f), g * 0.8f);
            HShards(14, fx, c.y, 8.0f, 0.5f, 0.08f, 0.7f, 1f, 121, 90f, 360f, Shape.Shard);
        }

        void AnimSMeteorRain()
        {
            float x0 = sweepCenter.x;
            float g = WFade(2.0f, 1.0f);
            const int N = 9;
            Vector2 dir = new Vector2(-0.45f, -1f).normalized;
            float ang = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
            for (int i = 0; i < N; i++)
            {
                float ts = 0.2f * i + 0.1f * Rnd(i, 2);
                float p = (_t - ts) / 0.5f;
                if (p < 0f) continue;
                Vector2 hit = new Vector2(x0 + (Rnd(i, 4) - 0.5f) * 5.0f, groundY + 0.05f);
                float dist = 6.0f;
                if (p < 1f)
                {
                    Vector2 pos = hit - dir * dist * (1f - p * p);
                    int n = Seg(pos.x - dir.x * 2.0f, pos.y - dir.y * 2.0f, pos.x, pos.y, 8);
                    DrawBrush(n, 0.5f, 0.55f);
                    EFlame(pos.x - dir.x * 0.6f, pos.y - dir.y * 0.6f, 1.4f, ang + 180f, 0.95f, i + 150);
                    DrawProp(Shape.Rock, pos.x, pos.y, ang, 0.9f, 1f);
                }
                else
                {
                    float f = Mathf.Clamp01((_t - ts - 0.5f) / 1.0f);
                    if (Once(1 << (i + 2))) Fire();
                    DrawProp(Shape.Star5, hit.x, hit.y + 0.3f, 90f, 1.4f * Mathf.Sin(Mathf.Clamp01(f * 3f) * Mathf.PI), 1f);
                    EFlame(hit.x, hit.y + 0.45f, 0.9f * (1f - f), 90f, (1f - f) * g, i + 170);
                    int n = Ring(hit.x, groundY, 0.9f * EaseOut(f * 2f), 0.18f * EaseOut(f * 2f));
                    DrawBrush(n, 0.07f, (1f - f) * g);
                    HShards(5, hit.x, hit.y, 5.0f, 0.3f, ts + 0.5f, 0.7f, 1f, 130 + i, 90f, 140f, Shape.Rock);
                }
            }
        }

        void AnimSTaijiUltimate()
        {
            Vector2 c = sweepCenter + new Vector2(0f, 1.0f);
            float g = WFade(2.2f, 0.8f);
            float open = EaseOut(_t / 0.5f);
            if (Once(1)) Fire();
            DrawProp(Shape.Taiji, c.x, c.y + 0.6f, 90f + _t * 120f, 3.4f * open, g);
            int n = Ring(c.x, c.y + 0.6f, 2.2f * open, 2.2f * open);
            DrawBrush(n, 0.14f, g * 0.8f);
            for (int k = 0; k < 8; k++)
            {
                float ang = (k * 45f - _t * 60f) * Mathf.Deg2Rad;
                DrawProp(Shape.Hex, c.x + Mathf.Cos(ang) * 2.6f * open, c.y + 0.6f + Mathf.Sin(ang) * 2.6f * open, 90f, 0.55f, g);
            }

            float bp = Mathf.Clamp01((_t - 1.2f) / 0.3f);
            if (bp > 0f)
            {
                for (int k = 0; k < 5; k++)
                {
                    float x = c.x + (k - 2) * 1.0f;
                    HStroke(x, c.y - 0.6f, x, groundY - (c.y - 0.6f - groundY) * 0.0f + (1f - bp) * 3.0f, 0f, 0.4f, g * 0.8f * (1f - 0.1f * Mathf.Abs(k - 2)));
                    if (bp >= 1f)
                    {
                        int rn = Ring(x, groundY, 0.7f * EaseOut((_t - 1.5f) / 0.5f), 0.14f * EaseOut((_t - 1.5f) / 0.5f));
                        DrawBrush(rn, 0.07f, g);
                    }
                }
            }

            int gr = Ring(c.x, groundY, 3.0f * open, 0.6f * open);
            DrawBrush(gr, 0.12f, g);
        }

        void AnimSDragonQi()
        {
            Vector2 c = sweepCenter;
            float g = WFade(1.3f, 0.7f);
            float adv = EaseOut(_t / 0.7f);
            float tailCut = Mathf.Clamp01((_t - 0.5f) / 0.8f);
            if (Once(1)) Fire();
            float hx = c.x + 0.5f + 8.0f * adv;
            float tx = c.x + 0.5f + 7.0f * tailCut;
            float L = Mathf.Max(0.5f, hx - tx);
            int n = WaveDir(tx, c.y, 0f, L, 28, 0.4f, 0.9f, -_t * 6f);
            DrawBrush(n, 0.9f, g);
            Vector3 hd = _buf[n - 1];
            for (int j = 0; j < 5; j++)
            {
                float s = 0.15f + 0.17f * j;
                float fx = tx + L * s;
                EFlame(fx, c.y + 0.35f * Mathf.Sin((fx - tx) * 0.9f * 6.28f / L * 0f + -_t * 6f + s * 6f) + 0.3f, 0.6f, 90f, g * 0.8f, j + 190);
            }

            DrawProp(Shape.Kunai, hd.x - 0.2f, hd.y + 0.35f, 150f, 0.8f, g);
            DrawProp(Shape.Kunai, hd.x - 0.2f, hd.y - 0.35f, -150f, 0.8f, g);
            DrawProp(Shape.Flame, hd.x + 0.4f, hd.y, 0f, 0.9f, g);
            DrawProp(Shape.Orb, hd.x, hd.y + 0.1f, 0f, 0.28f, g);
            DrawProp(Shape.Feather, hd.x - 0.5f, hd.y + 0.2f, 170f, 0.9f, g * 0.8f);
            DrawProp(Shape.Feather, hd.x - 0.5f, hd.y - 0.2f, -170f, 0.9f, g * 0.8f);
        }

        void AnimSBarrierExpand()
        {
            Vector2 c = sweepCenter;
            float g = WFade(1.1f, 0.7f);
            float e = EaseOut(_t / 0.6f);
            if (Once(1)) Fire();
            DrawProp(Shape.Taiji, c.x, c.y, 90f + _t * 100f, 1.2f * (1f - 0.5f * e) + 0.2f, g * (1f - 0.5f * e));
            for (int ring = 0; ring < 2; ring++)
            {
                int cnt = ring == 0 ? 10 : 16;
                float R = (ring == 0 ? 2.4f : 4.0f) * e;
                for (int k = 0; k < cnt; k++)
                {
                    float ang = (k * 360f / cnt + _t * (ring == 0 ? 40f : -30f)) * Mathf.Deg2Rad;
                    DrawProp(Shape.Hex, c.x + Mathf.Cos(ang) * R, c.y + Mathf.Sin(ang) * R * 0.9f, 90f, ring == 0 ? 0.7f : 0.55f, g);
                }

                int n = Ring(c.x, c.y, R, R * 0.9f);
                DrawBrush(n, 0.12f - 0.04f * ring, g * 0.8f);
            }

            int gr = Ring(c.x, groundY, 4.2f * e, 0.7f * e);
            DrawBrush(gr, 0.1f, g * 0.8f);
            HShards(14, c.x, c.y, 8.0f, 0.4f, 0f, 0.7f, 1f, 141, 90f, 360f, Shape.Shard);
        }
    }
}
