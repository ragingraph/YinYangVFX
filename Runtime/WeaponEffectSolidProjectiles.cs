using UnityEngine;

namespace YinYangVFX
{
    // Concrete projectiles: a glowing star orb, a homing arrow, a spinning kunai boomerang, a beam cannon with a
    // flame head, an ink laser and a four-kunai shuriken. Origin = launch point, flying right (mirror for left).
    public partial class WeaponEffect
    {
        void PImpact(float x, float y, float t0, float a, float size)
        {
            float k = (_t - t0) / 0.4f;
            if (k < 0f || k >= 1f) return;
            if (Once(1)) Fire();
            DrawProp(Shape.Star5, x, y, 90f + _t * 200f, size * (0.5f + EaseOut(k)), (1f - k) * a);
            int n = Ring(x, y, size * 0.8f * EaseOut(k), size * 0.8f * EaseOut(k));
            DrawBrush(n, 0.08f * (1f - k), (1f - k) * a);
            HShards(8, x, y, 5.0f, 0.3f, t0, 0.5f, a, 160, 0f, 200f, Shape.Shard);
        }

        void AnimSEnergyBolt()
        {
            Vector2 c = sweepCenter;
            float dist = qiTravel * 1.5f;
            float p = Mathf.Clamp01(_t / 0.55f);
            float hx = c.x + dist * p * p * (3f - 2f * p) * 0.35f + dist * p * 0.65f;
            float g = WFade(0.7f, 0.4f);
            if (p < 1f)
            {
                HStroke(hx - 1.9f, c.y, hx, c.y, 0f, 0.55f, 0.75f);
                DrawProp(Shape.Orb, hx, c.y, 0f, 0.8f + 0.06f * Mathf.Sin(_t * 40f), 1f);
                DrawProp(Shape.Star5, hx, c.y, 90f + _t * 500f, 0.55f, 1f);
                for (int k = 0; k < 3; k++)
                {
                    DrawProp(Shape.Flame, hx - 0.5f - 0.35f * k, c.y + (k - 1) * 0.15f, 180f, 0.6f - 0.1f * k, 0.85f);
                }
            }
            else
            {
                PImpact(hx, c.y, 0.55f, g, 1.6f);
            }
        }

        void AnimSHomingShot()
        {
            Vector2 c = sweepCenter;
            float dist = qiTravel * 1.5f;
            Vector2 a = c, b = new Vector2(c.x + dist * 0.45f, c.y + 1.8f), e = new Vector2(c.x + dist, c.y - 0.2f);
            float p = Mathf.Clamp01(_t / 0.9f);
            float g = WFade(1.0f, 0.4f);
            if (p < 1f)
            {
                const int m = 20;
                for (int i = 0; i < m; i++)
                {
                    float s = Mathf.Clamp01(p - 0.3f * (1f - i / (float)(m - 1)));
                    Vector2 q = Bez(a, b, e, s * s * (3f - 2f * s) * 0.4f + s * 0.6f);
                    _buf[i] = new Vector3(q.x, q.y, 0f);
                }

                Vector3 h = _buf[m - 1], h2 = _buf[m - 2];
                float ang = Mathf.Atan2(h.y - h2.y, h.x - h2.x) * Mathf.Rad2Deg;
                DrawBrush(m, 0.3f, 0.7f);
                DrawProp(Shape.Arrow, h.x, h.y, ang, 0.9f, 1f);
            }
            else
            {
                PImpact(e.x, e.y, 0.9f, g, 1.3f);
            }
        }

        void AnimSBoomerang()
        {
            Vector2 c = sweepCenter;
            float dist = qiTravel * 1.4f;
            float T = 1.5f;
            float ph = Mathf.Clamp01(_t / T) * Mathf.PI * 2f;
            float g = WFade(1.5f, 0.4f);
            if (_t < T)
            {
                const int m = 24;
                for (int i = 0; i < m; i++)
                {
                    float q = Mathf.Max(0f, ph - 0.9f * (1f - i / (float)(m - 1)));
                    _buf[i] = new Vector3(c.x + dist * 0.5f * (1f - Mathf.Cos(q)), c.y + 0.9f * Mathf.Sin(q) * 0.7f, 0f);
                }

                Vector3 hp = _buf[m - 1];
                DrawBrush(m, 0.25f, 0.6f);
                float spin = _t * 900f;
                for (int k = 0; k < 2; k++)
                {
                    DrawProp(Shape.Kunai, hp.x, hp.y, spin + k * 140f, 0.9f, 1f);
                }

                DrawProp(Shape.Star5, hp.x, hp.y, 90f, 0.22f, 1f);
                if (_t > T * 0.45f && _t < T * 0.55f) Once(1);
            }
            else
            {
                if (Once(2)) Fire();
                float k = (_t - T) / 0.4f;
                if (k < 1f) DrawProp(Shape.Star5, c.x, c.y, 90f, 0.8f * Mathf.Sin(k * Mathf.PI), g);
            }
        }

        void AnimSBeamCannon()
        {
            Vector2 c = sweepCenter + new Vector2(0.3f, 0f);
            const float chargeT = 0.5f, fireT = 0.9f;
            float g = WFade(1.6f, 0.6f);
            if (_t < chargeT)
            {
                float k = _t / chargeT;
                DrawProp(Shape.Orb, c.x, c.y, 0f, 0.2f + 0.8f * k, 0.95f);
                DrawProp(Shape.Star5, c.x, c.y, 90f + _t * 600f, 0.1f + 0.5f * k, 1f);
                for (int i = 0; i < 6; i++)
                {
                    float ang = i * 60f + _t * 100f;
                    Vector2 d = WDir(ang);
                    float r = 1.3f * (1f - k) + 0.3f;
                    DrawProp(Shape.Kunai, c.x + d.x * r, c.y + d.y * r, ang + 180f, 0.35f, k);
                }
            }
            else if (_t < chargeT + fireT + 0.8f)
            {
                float on = Mathf.Clamp01((_t - chargeT) / 0.08f) * (1f - Mathf.Clamp01((_t - chargeT - fireT) / 0.5f));
                if (Once(1)) Fire();
                float L = 10.0f * on;
                float w = (1.3f + 0.1f * Mathf.Sin(_t * 50f)) * on;
                HStroke(c.x, c.y, c.x + L, c.y, 0f, w, g * 0.9f);
                HStroke(c.x, c.y, c.x + L, c.y, 0f, w * 0.45f, g);
                DrawProp(Shape.Star5, c.x, c.y, 90f + _t * 400f, 1.4f * on, g);
                DrawProp(Shape.Star5, c.x + L, c.y, 90f - _t * 400f, 1.8f * on, g);
                for (int j = 0; j < 5; j++)
                {
                    float f = SLoop(j, 3, 1.2f);
                    EFlame(c.x + f * L, c.y + (Rnd(j, 4) - 0.5f) * 0.5f, 0.5f, 0f, on * 0.7f, j + 200);
                }
            }
        }

        void AnimSLaser()
        {
            Vector2 c = sweepCenter;
            float L = 9f;
            float k = Mathf.Clamp01(_t / 0.05f);
            float g = WFade(0.1f, 0.5f);
            if (Once(1)) Fire();
            HStroke(c.x, c.y, c.x + L * k, c.y, 0f, 0.5f, g * 0.9f);
            HStroke(c.x, c.y, c.x + L * k, c.y, 0f, 0.18f, g);
            DrawProp(Shape.Star5, c.x, c.y, 90f + _t * 600f, 0.9f * g, g);
            DrawProp(Shape.Star5, c.x + L * k, c.y, 90f - _t * 600f, 1.6f * g, g);
            HShards(8, c.x + L, c.y, 7.0f, 0.3f, 0.05f, 0.45f, 1f, 170, 180f, 140f, Shape.Shard);
        }

        void AnimSShuriken()
        {
            Vector2 c = sweepCenter;
            float dist = qiTravel * 1.4f;
            float p = Mathf.Clamp01(_t / 0.6f);
            float hx = c.x + dist * p, hy = c.y + 0.06f * Mathf.Sin(_t * 30f);
            float g = WFade(0.8f, 0.4f);
            if (p < 1f)
            {
                HStroke(hx - 1.4f, hy, hx, hy, 0f, 0.14f, 0.55f);
                float spin = _t * 1400f;
                for (int k = 0; k < 4; k++)
                {
                    DrawProp(Shape.Kunai, hx, hy, spin + k * 90f, 0.7f, 1f);
                }
            }
            else
            {
                PImpact(hx, hy, 0.6f, g, 1.0f);
            }
        }
    }
}
