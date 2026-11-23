using UnityEngine;

namespace YinYangVFX
{
    // Concrete status effects (all looping): real flames, ice crystals, poison drops and skull, lightning bolts,
    // weights and arrows, a sealed ring, curse chains, blood drops. Origin = character body centre (sweepCenter).
    public partial class WeaponEffect
    {
        float SLoop(int i, int salt, float speed)
        {
            return Mathf.Repeat(_t * speed + Rnd(i, salt), 1f);
        }

        void AnimSBurn()
        {
            Vector2 c = sweepCenter;
            float pk = 1f + 0.35f * _pulse;
            float baseY = c.y - 0.45f;
            for (int j = 0; j < 7; j++)
            {
                float x = c.x + (j - 3f) * 0.14f;
                float h = (0.75f - 0.07f * Mathf.Abs(j - 3f)) * pk;
                EFlame(x, baseY + h * 0.5f - 0.05f, h, 90f + 5f * (j - 3f) * 0.5f, 0.95f, j + 20);
            }

            for (int j = 0; j < 4; j++)
            {
                float yy = c.y + 0.1f + 0.2f * j;
                float side = (j % 2 == 0) ? -1f : 1f;
                EFlame(c.x + side * BodyRx * 0.9f, yy, 0.5f * pk, 90f + side * -25f, 0.8f, j + 40);
            }

            for (int j = 0; j < 4; j++)
            {
                float f = SLoop(j, 5, 0.7f);
                float x = c.x + (Rnd(j, 6) - 0.5f) * 0.6f;
                DrawProp(Shape.Flame, x, c.y + 0.2f + f * 1.1f, 90f, 0.3f * (1f - f), Mathf.Sin(f * Mathf.PI) * 0.8f);
            }
        }

        void AnimSFrozen()
        {
            Vector2 c = sweepCenter;
            float pk = 1f + 0.15f * _pulse;
            for (int k = 0; k < 6; k++)
            {
                float side = k < 3 ? -1f : 1f;
                int q = k % 3;
                float x = c.x + side * (0.12f + 0.2f * q);
                float len = (0.9f - 0.12f * q) * pk;
                DrawStanding(Shape.Ice, x, groundY - 0.03f * len, len, 0.95f);
            }

            DrawStanding(Shape.Ice, c.x, groundY - 0.03f * 1.3f, 1.3f * pk, 0.95f);
            for (int k = 0; k < 5; k++)
            {
                float ang = 20f + k * 72f;
                Vector2 d = WDir(ang);
                DrawProp(Shape.Ice, c.x + d.x * BodyRx * 1.0f, c.y + d.y * BodyRy * 0.9f, ang, 0.38f * pk, 0.9f);
            }

            int n = Ring(c.x, c.y, BodyRx * 1.25f, BodyRy * 1.2f);
            DrawBrush(n, 0.07f, 0.55f);
            for (int k = 0; k < 3; k++)
            {
                float f = SLoop(k, 3, 0.35f);
                DrawProp(Shape.Star5, c.x + (Rnd(k, 4) - 0.5f) * 0.8f, c.y + 0.7f - f * 0.3f, 90f + _t * 90f, 0.2f, Mathf.Sin(f * Mathf.PI) * 0.9f);
            }
        }

        void AnimSPoison()
        {
            Vector2 c = sweepCenter;
            for (int w = 0; w < 3; w++)
            {
                int n = WaveDir(c.x + (w - 1) * 0.28f, c.y - 0.5f, 90f, 1.0f, 14, 0.08f, 9f, _t * 4f + w);
                DrawBrush(n, 0.12f, 0.55f);
            }

            for (int k = 0; k < 6; k++)
            {
                float f = SLoop(k, 1, 0.45f);
                float x = c.x + (Rnd(k, 2) - 0.5f) * 0.8f + 0.05f * Mathf.Sin(_t * 3f + k);
                float s = 0.18f + 0.18f * Rnd(k, 2);
                DrawProp(Shape.Orb, x, c.y - 0.55f + f * 1.2f, 0f, s, Mathf.Sin(f * Mathf.PI) * 0.9f);
            }

            float by = c.y + 0.95f + 0.05f * Mathf.Sin(_t * 3f);
            DrawProp(Shape.Skull, c.x, by, 90f, 0.5f * (1f + 0.15f * _pulse), 0.95f);
            for (int k = 0; k < 3; k++)
            {
                float f = SLoop(k, 8, 0.8f);
                DrawProp(Shape.Drop, c.x + (k - 1) * 0.12f, by - 0.25f - f * 0.35f, 90f, 0.16f, (1f - f) * 0.9f);
            }
        }

        void AnimSParalyze()
        {
            Vector2 c = sweepCenter;
            int fr = Mathf.FloorToInt(_t * 14f);
            float flick = 0.7f + 0.3f * Rnd(Mathf.FloorToInt(_t * 40f), 6);
            for (int k = 0; k < 4; k++)
            {
                float side = (k % 2 == 0) ? -1f : 1f;
                float x = c.x + side * (0.25f + 0.2f * Rnd(k, fr)) + 0f;
                float len = 0.9f + 0.35f * Rnd(k, fr + 2);
                float y = c.y + (Rnd(k, fr + 4) - 0.5f) * 0.5f;
                bool on = Rnd(k, fr + 9) > 0.25f;
                if (on) DrawProp(Shape.Bolt, x, y, 90f, len, flick);
            }

            int n = Ring(c.x, groundY, 0.5f, 0.1f);
            DrawBrush(n, 0.05f, 0.6f * flick);
            for (int k = 0; k < 3; k++)
            {
                float ang = Rnd(k, fr + 1) * 360f;
                Vector2 d = WDir(ang);
                DrawProp(Shape.Star5, c.x + d.x * BodyRx * 1.2f, c.y + d.y * BodyRy * 1.1f, ang, 0.22f, flick);
            }
        }

        void AnimSSlow()
        {
            Vector2 c = sweepCenter;
            int n = Ring(c.x, groundY + 0.02f, 0.6f, 0.12f);
            DrawBrush(n, 0.08f, 0.8f);
            DrawProp(Shape.Taiji, c.x, c.y + 0.95f, 90f - _t * 25f, 0.5f * (1f + 0.15f * _pulse), 0.9f);
            for (int k = 0; k < 3; k++)
            {
                float f = SLoop(k, 0, 0.35f);
                float x = c.x + (k - 1) * 0.3f;
                DrawProp(Shape.Arrow, x, c.y + 0.7f - f * 1.0f, -90f, 0.55f, Mathf.Sin(f * Mathf.PI) * 0.95f);
            }

            for (int j = 0; j < 2; j++)
            {
                float side = j == 0 ? -1f : 1f;
                DrawProp(Shape.Rock, c.x + side * 0.3f, groundY + 0.18f, 0f, 0.36f, 0.9f);
            }

            for (int j = 0; j < 4; j++)
            {
                int m = WaveDir(c.x + (j - 1.5f) * 0.18f, c.y + 0.5f, -90f, 0.8f, 10, 0.03f, 6f, _t * 1.5f + j);
                DrawBrush(m, 0.05f, 0.5f);
            }
        }

        void AnimSSilence()
        {
            Vector2 c = sweepCenter;
            Vector2 hc = new Vector2(c.x, c.y + 0.8f);
            int n = Ring(hc.x, hc.y, 0.34f, 0.34f);
            DrawBrush(n, 0.09f, 0.95f);
            DrawProp(Shape.Cross, hc.x, hc.y, 45f, 0.5f * (1f + 0.2f * _pulse), 1f);
            for (int k = 0; k < 3; k++)
            {
                float f = SLoop(k, 3, 0.6f);
                int m = ArcPts(hc.x + 0.45f + 0.1f * k, hc.y, 0.2f + 0.1f * k, 0.2f + 0.1f * k, -40f, 40f, 10);
                DrawBrush(m, 0.06f, (1f - f) * 0.7f * (1f - 0.2f * k));
            }

            for (int k = 0; k < 2; k++)
            {
                float side = k == 0 ? -1f : 1f;
                DrawProp(Shape.Hex, c.x + side * 0.45f, c.y + 0.2f, 90f, 0.3f, 0.7f + 0.2f * Mathf.Sin(_t * 4f + k * 3f));
            }
        }

        void AnimSCurse()
        {
            Vector2 c = sweepCenter;
            int rn = Ring(c.x, groundY, 0.72f, 0.14f);
            DrawBrush(rn, 0.08f, 0.8f);
            for (int t = 0; t < 3; t++)
            {
                const int m = 20;
                for (int i = 0; i < m; i++)
                {
                    float s = i / (float)(m - 1);
                    float a = _t * 3f + s * 5f + t * 2.094f;
                    float r = 0.38f + 0.04f * Mathf.Sin(s * 8f + _t * 5f);
                    _buf[i] = new Vector3(c.x + r * Mathf.Cos(a), c.y - 0.55f + s * 1.1f, 0f);
                }

                DrawBrush(m, 0.1f, 0.75f);
            }

            float by = c.y + 1.0f + 0.06f * Mathf.Sin(_t * 2.5f);
            DrawProp(Shape.DarkOrb, c.x, by, 0f, 0.55f * (1f + 0.15f * _pulse), 0.95f);
            DrawProp(Shape.Skull, c.x, by, 90f, 0.34f, 0.95f);
            for (int k = 0; k < 4; k++)
            {
                float ang = _t * 70f + k * 90f;
                Vector2 d = WDir(ang);
                DrawProp(Shape.Shard, c.x + d.x * 0.62f, groundY + 0.1f + d.y * 0.12f, 90f, 0.22f, 0.8f);
            }
        }

        void AnimSBleed()
        {
            Vector2 c = sweepCenter;
            for (int k = 0; k < 3; k++)
            {
                float a = (30f + k * 110f) * Mathf.Deg2Rad;
                float px = c.x + BodyRx * 0.8f * Mathf.Cos(a), py = c.y + BodyRy * 0.7f * Mathf.Sin(a);
                HStroke(px - 0.16f, py + 0.12f, px + 0.16f, py - 0.12f, 0.03f, 0.1f, 0.65f + 0.3f * Mathf.Sin(_t * 3f + k));
            }

            for (int k = 0; k < 7; k++)
            {
                float f = SLoop(k, 1, 0.9f);
                float x = c.x + (Rnd(k, 2) - 0.5f) * 0.7f;
                float y0 = c.y + 0.4f - 0.6f * Rnd(k, 3);
                float y = y0 - f * f * (y0 - groundY);
                DrawProp(Shape.Drop, x, y, 90f, 0.2f + 0.12f * Rnd(k, 4), 1f - 0.4f * f);
            }

            float pr = 0.3f + 0.1f * Mathf.Sin(_t * 2f) + 0.12f * _pulse;
            int n = Ring(c.x, groundY + 0.01f, pr, pr * 0.2f);
            DrawBrush(n, 0.07f, 0.8f);
        }
    }
}
