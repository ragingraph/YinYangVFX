using UnityEngine;

namespace YinYangVFX
{
    // Concrete rising-from-the-ground effects: shard spikes, rock burst, standing swords, tendrils with leaves,
    // a stone pillar, a rock wall. All launch upward fast. Origin = ground contact point (groundY).
    public partial class WeaponEffect
    {
        // A prop launched from the ground: len, k = RiseK 0..1.
        void GUp(Shape sh, float x, float len, float k, float a)
        {
            if (k <= 0f) return;
            DrawStanding(sh, x, groundY - len + len * 0.97f * k, len, a);
        }

        void GDust(float x, float t0, float size, float a)
        {
            float f = (_t - t0) / 0.35f;
            if (f < 0f || f >= 1f) return;
            int n = Ring(x, groundY, size * EaseOut(f), size * 0.2f * EaseOut(f));
            DrawBrush(n, 0.06f, (1f - f) * a);
        }

        void AnimSGroundSpikeWave()
        {
            Vector2 c = sweepCenter;
            const int N = 10;
            if (Once(1)) Fire();
            for (int i = 0; i < N; i++)
            {
                float ts = 0.06f * i;
                float x = c.x + 0.35f + i * 0.45f;
                float k = RiseK(_t - ts, 0.07f, 0.5f, 0.4f);
                float len = (0.9f + 0.5f * Rnd(i, 1)) * (0.6f + 0.4f * i / (float)(N - 1));
                GUp(Shape.Shard, x, len, k, 1f);
                GDust(x, ts, 0.45f, 0.9f);
                HShards(2, x, groundY + 0.1f, 3.5f, 0.2f, ts, 0.5f, 1f, 320 + i, 90f, 100f, Shape.Rock);
            }

            HStroke(c.x + 0.3f, groundY + 0.03f, c.x + 0.3f + 4.6f * EaseOut(_t / 0.6f), groundY + 0.03f, 0f, 0.14f, WFade(0.9f, 0.5f));
        }

        void AnimSGroundBurst()
        {
            Vector2 c = sweepCenter;
            float x = c.x;
            float g = WFade(0.9f, 0.4f);
            float e = 1f - Mathf.Pow(1f - Mathf.Clamp01(_t / 0.1f), 4f);
            if (Once(1)) Fire();
            HStroke(x, groundY, x, groundY + 3.0f * e, 0f, 1.0f * (1f - 0.5f * e), g * 0.5f);
            DrawProp(Shape.Star5, x, groundY + 0.5f, 90f + _t * 200f, 0.6f + 1.6f * e, WFade(0.25f, 0.4f));
            for (int i = 0; i < 4; i++)
            {
                GroundCrack(x, i % 2 == 0 ? 1f : -1f, (0.9f + 0.5f * (i / 2)) * 1.4f * e, i + 4, 0.06f, g);
            }

            GDust(x, 0f, 2.4f, g);
            GDust(x, 0.06f, 1.6f, g);
            HShards(10, x, groundY + 0.1f, 7.0f, 0.55f, 0f, 0.8f, 1f, 340, 90f, 100f, Shape.Rock);
            HShards(10, x, groundY + 0.1f, 8.0f, 0.3f, 0f, 0.7f, 1f, 341, 90f, 150f, Shape.Shard);
        }

        void AnimSRisingBlades()
        {
            Vector2 c = sweepCenter;
            const int N = 9;
            if (Once(1)) Fire();
            for (int i = 0; i < N; i++)
            {
                int j = (i * 4) % N;
                float x = c.x + (j - (N - 1) * 0.5f) * 0.32f;
                float ts = 0.07f * i;
                float k = RiseK(_t - ts, 0.08f, 0.9f, 0.45f);
                float len = 1.3f + 0.5f * Rnd(j, 2);
                if (k > 0f)
                {
                    HStroke(x, groundY, x, groundY + len * k, 0f, 0.18f, 0.5f * Mathf.Clamp01(1.5f - k));
                }

                GUp(Shape.Sword, x, len, k, 1f);
                GDust(x, ts, 0.35f, 0.8f);
            }
        }

        void AnimSTendrilRise()
        {
            Vector2 c = sweepCenter;
            const int N = 7;
            if (Once(1)) Fire();
            for (int i = 0; i < N; i++)
            {
                float x = c.x + (i - (N - 1) * 0.5f) * 0.3f;
                float ts = 0.05f * Mathf.Abs(i - (N - 1) * 0.5f);
                float k = RiseK(_t - ts, 0.2f, 0.5f, 0.5f);
                if (k <= 0.01f) continue;
                float len = (1.4f + 1.0f * Rnd(i, 3)) * k;
                int n = WaveDir(x, groundY, 90f, len, 16, 0.12f, 6f, -_t * 5f + i);
                DrawBrush(n, 0.2f, 1f);
                Vector3 hp = _buf[n - 1];
                DrawProp(Shape.Feather, hp.x, hp.y, 90f + 25f * Mathf.Sin(_t * 6f + i), 0.55f * k, 1f);
                if (len > 0.7f)
                {
                    Vector3 mp = _buf[n / 2];
                    DrawProp(Shape.Feather, mp.x, mp.y, i % 2 == 0 ? 35f : 145f, 0.4f * k, 0.9f);
                }

                GDust(x, ts, 0.35f, 0.7f);
            }
        }

        void AnimSPillarRise()
        {
            Vector2 c = sweepCenter;
            float x = c.x;
            float g = WFade(1.4f, 0.7f);
            float k = RiseK(_t, 0.15f, 1.2f, 0.7f);
            if (Once(1)) Fire();
            float H = 4.0f;
            GUp(Shape.Stick, x, H, k, 1f);
            HStroke(x, groundY, x, groundY + H * k, 0f, 0.9f, 0.3f * (1f - Mathf.Clamp01((_t - 0.3f) / 0.4f)));
            GDust(x, 0f, 1.4f, g);
            GDust(x, 0.08f, 1.0f, g);
            for (int i = 0; i < 4; i++)
            {
                GroundCrack(x, i % 2 == 0 ? 1f : -1f, 0.9f + 0.4f * (i / 2), i + 6, 0.06f, g);
            }

            HShards(10, x, groundY + 0.2f, 6.0f, 0.4f, 0f, 0.8f, 1f, 350, 90f, 120f, Shape.Rock);
        }

        void AnimSRiseWall()
        {
            Vector2 c = sweepCenter;
            const int N = 13;
            if (Once(1)) Fire();
            for (int i = 0; i < N; i++)
            {
                float off = i - (N - 1) * 0.5f;
                float x = c.x + off * 0.34f;
                float ts = 0.04f * Mathf.Abs(off);
                float k = RiseK(_t - ts, 0.08f, 0.9f, 0.4f);
                float len = 1.6f - 0.06f * off * off * 0.25f;
                GUp(Shape.Shard, x, len * 1.3f, k, 1f);
                GUp(Shape.Rock, x, len * 0.6f, k, 1f);
                GDust(x, ts, 0.4f, 0.7f);
            }

            HStroke(c.x - 2.3f, groundY + 0.03f, c.x + 2.3f, groundY + 0.03f, 0f, 0.14f, WFade(1.0f, 0.5f));
            HShards(8, c.x, groundY + 0.1f, 5.0f, 0.3f, 0f, 0.6f, 1f, 360, 90f, 160f, Shape.Rock);
        }
    }
}
