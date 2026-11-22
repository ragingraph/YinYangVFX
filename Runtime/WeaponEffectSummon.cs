using UnityEngine;

namespace YinYangVFX
{
    // Summon / teleport effects. Origin = the spot on the ground / body centre where it happens.
    // Single colour per prefab.
    public partial class WeaponEffect
    {
        // Ground magic circle (ellipse, perspective-flattened) with spokes and rotating runes.
        void MagicCircle(float cx, float cy, float rx, float a, float rot)
        {
            float ry = rx * 0.22f;
            int n = Ring(cx, cy, rx, ry);
            Draw(Taper.Flat, n, 0.05f * thickness, primary, a);
            n = Ring(cx, cy, rx * 0.72f, ry * 0.72f);
            Draw(Taper.Flat, n, 0.03f * thickness, primary, a * 0.8f);
            for (int k = 0; k < 6; k++)
            {
                float t0 = rot + k * 60f;
                float t1 = t0 + 24f;
                n = ArcPts(cx, cy, rx * 0.86f, ry * 0.86f, t0, t1, 8);
                Draw(Taper.Flat, n, 0.07f * thickness, primary, a);
            }

            for (int k = 0; k < 3; k++)
            {
                float ang = (rot * -0.7f + k * 120f) * Mathf.Deg2Rad;
                float ang2 = ang + 2.0944f;
                Vector2 p0 = new Vector2(cx + Mathf.Cos(ang) * rx * 0.72f, cy + Mathf.Sin(ang) * ry * 0.72f);
                Vector2 p1 = new Vector2(cx + Mathf.Cos(ang2) * rx * 0.72f, cy + Mathf.Sin(ang2) * ry * 0.72f);
                n = Seg(p0.x, p0.y, p1.x, p1.y, 2);
                Draw(Taper.Flat, n, 0.025f * thickness, primary, a * 0.7f);
            }
        }

        void AnimSummonCircle()
        {
            Vector2 c = sweepCenter;
            float x = c.x, y = groundY;
            float open = EaseOut(_t / 0.5f);
            float g = FadeFrom(1.6f);
            if (Once(1))
            {
                Burst(x, y, 12, 3f, 8, 1.2f);
            }

            MagicCircle(x, y, 1.1f * open, g, _t * 80f);

            // light column rises from the circle
            float up = EaseOut((_t - 0.3f) / 0.4f);
            float colFade = 1f - Mathf.Clamp01((_t - 0.8f) / 0.8f);
            if (_t > 0.3f)
            {
                int n = Seg(x, y, x, y + 2.4f * up, 2);
                Draw(Taper.Head, n, 0.9f * thickness * (0.6f + 0.4f * colFade), primary * 0.5f, colFade * g);
                n = Seg(x, y, x, y + 2.4f * up, 2);
                Draw(Taper.Head, n, 0.28f * thickness, primary, colFade * g);
            }

            if (_t > 0.45f && Once(2))
            {
                Fire();
                Burst(x, y + 0.3f, 50, 6f, 16, 2.6f);
                Emit(_dots, x, y + 0.5f, 0f, 0f, 3.0f, 0.3f, Hue(primary));
            }

            if (_t < 1.4f)
            {
                _acc += _dt * 70f;
                while (_acc >= 1f)
                {
                    _acc -= 1f;
                    float a = Random.value * 6.2832f;
                    Emit(Random.value > 0.5f ? _stars : _sparks, x + Mathf.Cos(a) * 0.9f * open, y + Mathf.Sin(a) * 0.2f * open, 0f, 1f + 2f * Random.value, 0.08f + 0.08f * Random.value, 0.9f, Hue(primary));
                }
            }
        }

        // Looping vertical portal. Use with loop=true; call Stop() to close it.
        void AnimPortalOpen()
        {
            Vector2 c = sweepCenter;
            float x = c.x, y = c.y + 0.1f;
            float open = EaseOut(_t / 0.4f);
            float ry = 0.95f * open, rx = 0.38f * open;
            float spin = _t * 140f;

            int n = Ring(x, y, rx * 1.25f, ry * 1.12f);
            Draw(Taper.Flat, n, 0.14f * thickness, primary * 0.5f, 0.9f);
            n = Ring(x, y, rx, ry);
            Draw(Taper.Flat, n, 0.07f * thickness, primary, 1f);

            for (int k = 0; k < 4; k++)
            {
                float rr = 0.85f - k * 0.2f;
                n = ArcPts(x, y, rx * rr, ry * rr, spin * (k % 2 == 0 ? 1f : -1f) + k * 70f, spin * (k % 2 == 0 ? 1f : -1f) + k * 70f + 120f, 14);
                Draw(Taper.Head, n, 0.05f * thickness, primary, 0.8f);
            }

            Emit(_dots, x, y, 0f, 0f, 1.6f * open, 0.07f, Tint(0.2f + 0.2f * _pulse));
            if (Live())
            {
                _acc += _dt * 45f;
                while (_acc >= 1f)
                {
                    _acc -= 1f;
                    float a = Random.value * 6.2832f;
                    float px = x + Mathf.Cos(a) * rx * 1.1f, py = y + Mathf.Sin(a) * ry * 1.1f;
                    Emit(Random.value > 0.5f ? _stars : _sparks, px, py, -Mathf.Cos(a) * 1.2f, -Mathf.Sin(a) * 1.2f, 0.07f + 0.07f * Random.value, 0.5f, Hue(primary));
                }
            }
        }

        void AnimTeleportOut()
        {
            Vector2 c = sweepCenter;
            float k = Mathf.Clamp01(_t / 0.35f);
            float g = FadeFrom(0.4f);
            if (Once(1))
            {
                Fire();
                Emit(_dots, c.x, c.y, 0f, 0f, 2.6f, 0.2f, Hue(primary));
            }

            // body-shaped lines collapse into a vertical streak
            for (int i = 0; i < 9; i++)
            {
                float x = c.x + (i - 4) * 0.085f * (1f - k);
                int n = Seg(x, c.y - BodyRy * (1f - 0.2f * k), x, c.y + BodyRy * (1f + 1.2f * k), 2);
                Draw(Taper.Lens, n, 0.06f * thickness * (1f - 0.5f * k), primary, g);
            }

            if (_t < 0.3f)
            {
                _acc += _dt * 150f;
                while (_acc >= 1f)
                {
                    _acc -= 1f;
                    Emit(Random.value > 0.5f ? _stars : _sparks, c.x + (Random.value - 0.5f) * 0.6f, c.y + (Random.value - 0.5f) * 1.1f, (Random.value - 0.5f) * 0.4f, 2f + 4f * Random.value, 0.08f + 0.08f * Random.value, 0.5f, Hue(primary));
                }
            }

            int rn = Ring(c.x, groundY, 0.9f * EaseOut(_t / 0.4f), 0.2f * EaseOut(_t / 0.4f));
            Draw(Taper.Flat, rn, 0.04f * thickness, primary, g * (1f - Mathf.Clamp01(_t / 0.5f)));
        }

        void AnimTeleportIn()
        {
            Vector2 c = sweepCenter;
            float g = FadeFrom(0.3f);
            float k = Mathf.Clamp01(_t / 0.3f);
            if (_t >= 0.25f && Once(1))
            {
                Fire();
                Burst(c.x, c.y, 24, 4f, 10, 1.8f);
                Emit(_dots, c.x, c.y, 0f, 0f, 2.8f, 0.25f, Hue(primary));
            }

            // vertical streak expands into body width
            for (int i = 0; i < 9; i++)
            {
                float x = c.x + (i - 4) * 0.085f * k;
                int n = Seg(x, c.y - BodyRy * (1f + 1.2f * (1f - k)), x, c.y + BodyRy * (1f + 1.2f * (1f - k)), 2);
                Draw(Taper.Lens, n, 0.06f * thickness * (0.5f + 0.5f * k), primary, g);
            }

            if (_t < 0.3f)
            {
                _acc += _dt * 120f;
                while (_acc >= 1f)
                {
                    _acc -= 1f;
                    float a = Random.value * 6.2832f;
                    Emit(Random.value > 0.5f ? _stars : _sparks, c.x + Mathf.Cos(a) * 1.0f, c.y + Mathf.Sin(a) * 1.0f, -Mathf.Cos(a) * 3f, -Mathf.Sin(a) * 3f, 0.08f + 0.08f * Random.value, 0.3f, Hue(primary));
                }
            }

            float re = EaseOut((_t - 0.2f) / 0.4f);
            int rn = Ring(c.x, c.y, 1.0f * re, 1.0f * re);
            Draw(Taper.Flat, rn, 0.05f * thickness, primary, (1f - re) * (_t > 0.2f ? 1f : 0f));
        }

        void AnimRevive()
        {
            Vector2 c = sweepCenter;
            float x = c.x;
            float g = FadeFrom(1.4f);
            float rise = EaseOut(_t / 0.6f);
            if (Once(1))
            {
                Burst(x, groundY, 14, 3f, 10, 1.2f);
            }

            MagicCircle(x, groundY, 0.9f * rise, g, _t * 60f);

            // golden pillar with soft shaft
            float pf = 1f - Mathf.Clamp01((_t - 0.9f) / 0.9f);
            int n = Seg(x, groundY, x, groundY + 3.0f * rise, 2);
            Draw(Taper.Head, n, 0.7f * thickness * pf, primary * 0.5f, pf * g);
            n = Seg(x, groundY, x, groundY + 3.0f * rise, 2);
            Draw(Taper.Head, n, 0.2f * thickness * pf, primary, pf * g);

            // rising rings
            for (int i = 0; i < 3; i++)
            {
                float f = Mathf.Repeat(_t * 0.9f + i / 3f, 1f);
                float yy = groundY + f * 1.5f;
                n = Ring(x, yy, 0.5f * (1f - 0.3f * f), 0.12f * (1f - 0.3f * f));
                Draw(Taper.Flat, n, 0.035f * thickness, primary, (1f - f) * g);
            }

            if (_t >= 0.9f && Once(2))
            {
                Fire();
                Burst(x, c.y, 60, 6f, 22, 3f);
                Emit(_dots, x, c.y, 0f, 0f, 3.4f, 0.4f, Hue(primary));
            }

            if (_t < 1.5f)
            {
                _acc += _dt * 60f;
                while (_acc >= 1f)
                {
                    _acc -= 1f;
                    Emit(Random.value > 0.4f ? _stars : _sparks, x + (Random.value - 0.5f) * 0.9f, groundY + Random.value * 0.3f, (Random.value - 0.5f) * 0.3f, 1.2f + 2f * Random.value, 0.09f + 0.09f * Random.value, 1.2f, Hue(primary));
                }
            }
        }

        void AnimLevelUp()
        {
            Vector2 c = sweepCenter;
            float x = c.x;
            float g = FadeFrom(1.4f);
            float e = EaseOut(_t / 0.35f);
            if (Once(1))
            {
                Fire();
                Burst(x, c.y, 40, 6f, 20, 2.4f);
                Emit(_dots, x, c.y, 0f, 0f, 3.0f, 0.3f, Hue(primary));
            }

            // pillar and 5 shock rings climbing the body
            int n = Seg(x, groundY, x, groundY + 3.2f * e, 2);
            Draw(Taper.Head, n, 0.75f * thickness * (1f - 0.4f * e), primary * 0.5f, g * (1f - Mathf.Clamp01(_t / 1.2f)));
            for (int i = 0; i < 5; i++)
            {
                float d = Mathf.Clamp01((_t - 0.08f * i) / 0.9f);
                float yy = groundY + d * 2.4f;
                float rr = 0.7f * (1f - 0.35f * d);
                n = Ring(x, yy, rr, rr * 0.22f);
                Draw(Taper.Flat, n, 0.06f * thickness * (1f - d * 0.5f), primary, (1f - d) * g);
            }

            // chevron arrows rising
            for (int i = 0; i < 3; i++)
            {
                float f = Mathf.Repeat(_t * 0.8f + i / 3f, 1f);
                float yy = groundY + 0.3f + f * 1.8f;
                float w = 0.28f;
                n = Seg(x - w, yy - w * 0.7f, x, yy, 2);
                Draw(Taper.Flat, n, 0.07f * thickness, primary, Mathf.Sin(f * Mathf.PI) * g);
                n = Seg(x + w, yy - w * 0.7f, x, yy, 2);
                Draw(Taper.Flat, n, 0.07f * thickness, primary, Mathf.Sin(f * Mathf.PI) * g);
            }

            if (_t < 1.3f)
            {
                _acc += _dt * 70f;
                while (_acc >= 1f)
                {
                    _acc -= 1f;
                    Emit(Random.value > 0.5f ? _stars : _sparks, x + (Random.value - 0.5f) * 0.9f, groundY + Random.value * 0.4f, (Random.value - 0.5f) * 0.3f, 1.5f + 2.5f * Random.value, 0.09f + 0.09f * Random.value, 1.1f, Hue(primary));
                }
            }
        }
    }
}
