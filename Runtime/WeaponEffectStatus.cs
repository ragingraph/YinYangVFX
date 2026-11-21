using UnityEngine;

namespace YinYangVFX
{
    // Status effects (all looping): burning, frozen, poisoned, paralysed, slowed, silenced, cursed, bleeding.
    // Spawn once, set .follow to the character, call Pulse() when the status ticks, Stop() to remove it.
    // The body is an ellipse around "sweepCenter" (default 0,-0.2; about 0.34 wide, 0.56 tall); feet = "groundY".
    public partial class WeaponEffect
    {
        const float BodyRx = 0.34f;
        const float BodyRy = 0.56f;

        void AnimStatusBurn()
        {
            Vector2 c = sweepCenter;
            float baseY = c.y - 0.5f;
            for (int j = 0; j < 6; j++)
            {
                float x = c.x + (j - 2.5f) * 0.12f;
                float h = 0.55f + 0.35f * Rnd(j) + 0.15f * Mathf.Sin(_t * 9f + j * 2f);
                const int n = 12;
                for (int i = 0; i < n; i++)
                {
                    float s = i / (float)(n - 1);
                    float xo = 0.07f * Mathf.Sin(s * 9f + _t * 14f + j) * (1f - s);
                    _buf[i] = new Vector3(x + xo, baseY + h * (1f - s), 0f);
                }

                Draw(Taper.Head, n, (0.2f - 0.02f * Mathf.Abs(j - 2.5f)) * thickness, primary, 0.9f);
                if (j == 2 || j == 3)
                {
                    Draw(Taper.Head, n, 0.08f * thickness, _core, 1f);
                }
            }

            if (Live())
            {
                Emit(_dots, c.x, baseY + 0.1f, 0f, 0f, 1.2f, 0.07f, Tint(0.2f + 0.2f * _pulse));
                _acc += _dt * 25f;
                while (_acc >= 1f)
                {
                    _acc -= 1f;
                    Emit(_sparks, c.x + (Random.value - 0.5f) * 0.5f, c.y - 0.3f + Random.value * 0.6f, (Random.value - 0.5f) * 0.3f, 0.8f + Random.value, 0.05f, 0.8f, Hue(primary));
                }
            }
        }

        void AnimStatusFrozen()
        {
            Vector2 c = sweepCenter;
            const int m = 24;
            for (int k = 0; k <= m; k++)
            {
                float a = (k % m) * 15f * Mathf.Deg2Rad;
                float r = (k % 2 == 0) ? 1f : 1.14f;
                _buf[k] = new Vector3(c.x + BodyRx * 1.15f * r * Mathf.Cos(a), c.y + BodyRy * 1.1f * r * Mathf.Sin(a), 0f);
            }

            Draw(Taper.Flat, m + 1, 0.04f * thickness, primary, 1f);
            int n = Ring(c.x, c.y, BodyRx * 1.0f, BodyRy * 1.0f);
            Draw(Taper.Flat, n, 0.1f * thickness, primary * 0.5f, 0.4f);

            for (int k = 0; k < 6; k++)
            {
                float a = (20f + k * 60f) * Mathf.Deg2Rad;
                float ex = c.x + BodyRx * 1.15f * Mathf.Cos(a), ey = c.y + BodyRy * 1.1f * Mathf.Sin(a);
                n = Seg(ex, ey, ex + Mathf.Cos(a) * 0.3f, ey + Mathf.Sin(a) * 0.4f, 2);
                Draw(Taper.Lens, n, 0.1f * thickness, primary, 0.9f);
            }

            if (Live())
            {
                Emit(_dots, c.x, c.y, 0f, 0f, 1.4f, 0.07f, Tint(0.12f + 0.2f * _pulse));
                _acc += _dt * 10f;
                while (_acc >= 1f)
                {
                    _acc -= 1f;
                    float a = Random.value * 6.2832f;
                    Emit(_stars, c.x + Mathf.Cos(a) * BodyRx * 1.2f, c.y + Mathf.Sin(a) * BodyRy * 1.1f, 0f, -0.1f, 0.12f, 0.6f, Hue(primary));
                    Emit(_dots, c.x + (Random.value - 0.5f) * 0.8f, groundY + 0.1f, 0f, -0.05f, 0.6f, 0.8f, Tint(0.15f));
                }
            }
        }

        void AnimStatusPoison()
        {
            Vector2 c = sweepCenter;
            for (int k = 0; k < 8; k++)
            {
                float x = c.x + (Rnd(k) - 0.5f) * 0.8f;
                float y = c.y - 0.6f + Mathf.Repeat(_t * 0.45f + Rnd(k, 1), 1f) * 1.2f;
                float r = 0.04f + 0.05f * Rnd(k, 2);
                int n = Ring(x, y, r, r);
                Draw(Taper.Flat, n, 0.015f * thickness, primary, 0.8f);
            }

            for (int w = 0; w < 3; w++)
            {
                int n = WaveDir(c.x + (w - 1) * 0.25f, c.y - 0.5f, 90f, 0.9f, 14, 0.07f, 9f, _t * 4f + w);
                Draw(Taper.Head, n, 0.1f * thickness, primary * 0.6f, 0.7f);
            }

            if (Live())
            {
                Emit(_dots, c.x, c.y, 0f, 0f, 1.3f, 0.07f, Tint(0.1f + 0.2f * _pulse));
                _acc += _dt * 8f;
                while (_acc >= 1f)
                {
                    _acc -= 1f;
                    Emit(_sparks, c.x + (Random.value - 0.5f) * 0.6f, c.y + Random.value * 0.4f, 0f, -0.3f, 0.07f, 0.8f, Hue(primary));
                }
            }
        }

        void AnimStatusParalyze()
        {
            Vector2 c = sweepCenter;
            int fr = Mathf.FloorToInt(_t * 20f);
            float flick = 0.6f + 0.4f * Rnd(Mathf.FloorToInt(_t * 40f), 6);
            for (int k = 0; k < 3; k++)
            {
                float a = Rnd(k, fr) * 6.2832f;
                float a2 = a + (0.6f + 0.6f * Rnd(k, fr + 3)) * (Rnd(k, fr + 5) > 0.5f ? 1f : -1f);
                Vector2 p0 = new Vector2(c.x + BodyRx * Mathf.Cos(a), c.y + BodyRy * Mathf.Sin(a));
                Vector2 p1 = new Vector2(c.x + BodyRx * 1.3f * Mathf.Cos(a2), c.y + BodyRy * 1.3f * Mathf.Sin(a2));
                DrawBolt(p0, p1, 6, 0.08f, fr + k * 13, 0.04f, flick);
            }

            int n = Ring(c.x, groundY, 0.5f, 0.1f);
            Draw(Taper.Flat, n, 0.02f * thickness, primary, flick * 0.8f);

            if (Live())
            {
                _acc += _dt * 12f;
                while (_acc >= 1f)
                {
                    _acc -= 1f;
                    Emit(_sparks, c.x + (Random.value - 0.5f) * 0.8f, c.y + (Random.value - 0.5f) * 1.1f, (Random.value - 0.5f) * 2f, (Random.value - 0.5f) * 2f, 0.05f, 0.25f, Hue(primary));
                }
            }
        }

        void AnimStatusSlow()
        {
            Vector2 c = sweepCenter;
            int n = Ring(c.x, groundY + 0.02f, 0.55f, 0.11f);
            Draw(Taper.Flat, n, 0.03f * thickness, primary, 0.9f);
            float a0 = _t * 60f;
            n = ArcPts(c.x, groundY + 0.02f, 0.7f, 0.14f, a0, a0 + 120f, 14);
            Draw(Taper.Head, n, 0.05f * thickness, primary, 0.9f);

            for (int j = 0; j < 6; j++)
            {
                n = WaveDir(c.x + (j - 2.5f) * 0.14f, c.y + 0.1f, -90f, 0.7f, 10, 0.03f, 6f, _t * 1.5f + j);
                Draw(Taper.Head, n, 0.035f * thickness, primary, 0.6f);
            }

            for (int k = 0; k < 3; k++)
            {
                float f = Mathf.Repeat(_t * 0.4f + k / 3f, 1f);
                float y = c.y + 0.5f - f * 1.0f;
                _buf[0] = new Vector3(c.x - 0.15f, y + 0.1f, 0f);
                _buf[1] = new Vector3(c.x, y, 0f);
                _buf[2] = new Vector3(c.x + 0.15f, y + 0.1f, 0f);
                Draw(Taper.Flat, 3, 0.03f * thickness, primary, Mathf.Sin(f * Mathf.PI));
            }

            if (Live())
            {
                Emit(_dots, c.x, c.y, 0f, 0f, 1.3f, 0.07f, Tint(0.08f + 0.2f * _pulse));
            }
        }

        void AnimStatusSilence()
        {
            Vector2 c = sweepCenter;
            Vector2 hc = new Vector2(c.x, c.y + 0.78f);
            int n = Ring(hc.x, hc.y, 0.3f, 0.3f);
            Draw(Taper.Flat, n, 0.04f * thickness, primary, 1f);
            n = Seg(hc.x - 0.22f, hc.y + 0.22f, hc.x + 0.22f, hc.y - 0.22f, 2);
            Draw(Taper.Lens, n, 0.07f * thickness, primary, 1f);
            Draw(Taper.Lens, n, 0.03f * thickness, _core, 1f);

            for (int k = 0; k < 3; k++)
            {
                n = ArcPts(hc.x + 0.35f + 0.12f * k, hc.y, 0.2f + 0.1f * k, 0.2f + 0.1f * k, -40f, 40f, 10);
                Draw(Taper.Lens, n, 0.03f * thickness, primary, (1f - 0.25f * k) * (0.6f + 0.4f * Mathf.Sin(_t * 6f - k)));
            }

            for (int k = 0; k < 4; k++)
            {
                float a = (_t * 60f + k * 90f) * Mathf.Deg2Rad;
                n = Seg(hc.x + Mathf.Cos(a) * 0.32f, hc.y + Mathf.Sin(a) * 0.32f, hc.x + Mathf.Cos(a) * 0.42f, hc.y + Mathf.Sin(a) * 0.42f, 2);
                Draw(Taper.Flat, n, 0.02f * thickness, primary, 0.8f);
            }

            if (Live())
            {
                Emit(_dots, hc.x, hc.y, 0f, 0f, 0.9f, 0.07f, Tint(0.1f + 0.2f * _pulse));
            }
        }

        void AnimStatusCurse()
        {
            Vector2 c = sweepCenter;
            int n = Ring(c.x, groundY, 0.7f, 0.14f);
            Draw(Taper.Flat, n, 0.03f * thickness, primary, 0.9f);
            n = Ring(c.x, groundY, 0.5f, 0.1f);
            Draw(Taper.Flat, n, 0.02f * thickness, primary, 0.6f);

            for (int k = 0; k < 8; k++)
            {
                float a = (_t * 50f + k * 45f) * Mathf.Deg2Rad;
                float px = c.x + 0.6f * Mathf.Cos(a), py = groundY + 0.12f * Mathf.Sin(a);
                _buf[0] = new Vector3(px - 0.05f, py + 0.08f, 0f);
                _buf[1] = new Vector3(px, py, 0f);
                _buf[2] = new Vector3(px + 0.05f, py + 0.08f, 0f);
                Draw(Taper.Flat, 3, 0.02f * thickness, primary, 0.8f);
            }

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

                Draw(Taper.Head, m, 0.07f * thickness, primary, 0.8f);
            }

            if (Live())
            {
                Emit(_dots, c.x, c.y, 0f, 0f, 1.3f, 0.07f, Tint(0.1f + 0.2f * _pulse));
                _acc += _dt * 14f;
                while (_acc >= 1f)
                {
                    _acc -= 1f;
                    float side = Random.value > 0.5f ? 1f : -1f;
                    Emit(_stars, c.x + side * 0.6f, c.y + Random.value * 0.8f, -side * 0.6f, -0.2f, 0.12f, 0.8f, Hue(primary));
                }
            }
        }

        void AnimStatusBleed()
        {
            Vector2 c = sweepCenter;
            for (int k = 0; k < 3; k++)
            {
                float a = (30f + k * 110f) * Mathf.Deg2Rad;
                float px = c.x + BodyRx * 0.8f * Mathf.Cos(a), py = c.y + BodyRy * 0.7f * Mathf.Sin(a);
                int n = Seg(px - 0.12f, py + 0.1f, px + 0.12f, py - 0.1f, 2);
                Draw(Taper.Lens, n, 0.06f * thickness, primary, 0.6f + 0.4f * Mathf.Sin(_t * 3f + k));
            }

            float p = Mathf.Repeat(_t * 1.2f, 1f);
            int rn = Ring(c.x, c.y, 0.15f + 0.45f * p, 0.15f + 0.45f * p);
            Draw(Taper.Flat, rn, 0.03f * thickness, primary, (1f - p) * 0.8f);

            if (Live())
            {
                Emit(_dots, c.x, c.y, 0f, 0f, 1.2f, 0.07f, Tint(0.08f + 0.2f * _pulse));
                _acc += _dt * 10f;
                while (_acc >= 1f)
                {
                    _acc -= 1f;
                    Emit(_sparks, c.x + (Random.value - 0.5f) * 0.5f, c.y + (Random.value - 0.4f) * 0.6f, (Random.value - 0.5f) * 0.3f, -0.2f, 0.07f, 0.9f, Hue(primary));
                }
            }
        }
    }
}
