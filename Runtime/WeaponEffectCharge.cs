using UnityEngine;

namespace YinYangVFX
{
    // Charge-up / aura effects. Origin = the character (body centre = "sweepCenter"). Single colour per prefab.
    //   ChargeAura / Berserk / CharacterAura : looping (follow, Pulse, Stop). Pulse() on ChargeAura = release flash.
    //   RageBurst / Awaken                  : one-shot, fire onImpact at the burst.
    public partial class WeaponEffect
    {
        void AnimChargeAura()
        {
            Vector2 c = sweepCenter;
            for (int j = 0; j < 3; j++)
            {
                float kk = Mathf.Repeat(_t * 1.2f + j / 3f, 1f);
                float r = 1.1f * (1f - kk);
                int n = Ring(c.x, c.y, r * 0.8f, r);
                Draw(Taper.Flat, n, 0.03f * thickness, primary, kk);
            }

            for (int k = 0; k < 8; k++)
            {
                float f = Mathf.Repeat(_t * 1.5f + Rnd(k), 1f);
                float a = (k * 45f + Rnd(k, 1) * 20f) * Mathf.Deg2Rad;
                float r0 = 1.5f * (1f - f);
                float r1 = r0 + 0.35f;
                int n = Seg(c.x + Mathf.Cos(a) * r1 * 0.8f, c.y + Mathf.Sin(a) * r1, c.x + Mathf.Cos(a) * r0 * 0.8f, c.y + Mathf.Sin(a) * r0, 2);
                Draw(Taper.Head, n, 0.04f * thickness, primary, Mathf.Sin(f * Mathf.PI) * 0.9f);
            }

            int gr = Ring(c.x, groundY, 0.7f, 0.14f);
            Draw(Taper.Flat, gr, 0.03f * thickness, primary, 0.9f);
            for (int k = 0; k < 4; k++)
            {
                float a = (_t * 80f + k * 90f) * Mathf.Deg2Rad;
                int n = Seg(c.x + Mathf.Cos(a) * 0.6f, groundY + Mathf.Sin(a) * 0.12f, c.x + Mathf.Cos(a) * 0.8f, groundY + Mathf.Sin(a) * 0.16f, 2);
                Draw(Taper.Flat, n, 0.025f * thickness, primary, 0.8f);
            }

            if (Live())
            {
                Emit(_dots, c.x, c.y, 0f, 0f, 1.2f + 0.5f * Mathf.Sin(_t * 8f) + 1.5f * _pulse, 0.07f, Tint(0.14f + 0.3f * _pulse));
                _acc += _dt * 20f;
                while (_acc >= 1f)
                {
                    _acc -= 1f;
                    Emit(Random.value > 0.5f ? _stars : _sparks, c.x + (Random.value - 0.5f) * 0.7f, groundY + 0.05f, 0f, 1f + 1.5f * Random.value, 0.07f + 0.08f * Random.value, 0.8f, Hue(primary));
                }
            }
        }

        void AnimRageBurst()
        {
            Vector2 c = sweepCenter;
            float e = EaseOut(_t / 0.25f), g = FadeFrom(0.6f);
            if (Once(1))
            {
                Fire();
                Emit(_dots, c.x, c.y, 0f, 0f, 4f, 0.3f, Hue(primary));
                Burst(c.x, c.y, 50, 7f, 16, 3f);
            }

            for (int j = 0; j < 2; j++)
            {
                float d = Mathf.Clamp01((_t - 0.07f * j) / 0.5f);
                float r = 2.4f * EaseOut(d) * (1f - 0.3f * j);
                int n = Ring(c.x, c.y, r, r);
                Draw(Taper.Flat, n, (0.1f - 0.03f * j) * thickness, primary, (1f - d * 0.8f) * g);
            }

            for (int k = 0; k < 12; k++)
            {
                float a = (k * 30f + Rnd(k) * 10f) * Mathf.Deg2Rad;
                float len = (1.0f + 0.7f * Rnd(k, 1)) * e;
                const int m = 10;
                for (int i = 0; i < m; i++)
                {
                    float s = i / (float)(m - 1);
                    float off = 0.06f * Mathf.Sin(s * 10f + _t * 25f + k);
                    float rr = 0.3f + len * (1f - s);
                    _buf[i] = new Vector3(c.x + Mathf.Cos(a) * rr - Mathf.Sin(a) * off, c.y + Mathf.Sin(a) * rr + Mathf.Cos(a) * off, 0f);
                }

                Draw(Taper.Head, m, 0.12f * thickness, primary, g * 0.9f);
            }

            int pn = Seg(c.x, c.y + 2.2f * e, c.x, groundY, 2);
            Draw(Taper.Head, pn, 0.8f * (1f - 0.5f * e) * thickness, primary * 0.5f, g * (1f - e) * 0.9f);
            int gr = Ring(c.x, groundY, 2.4f * e, 0.4f * e);
            Draw(Taper.Flat, gr, 0.04f * thickness, primary, (1f - e * 0.6f) * g);
        }

        void AnimAwaken()
        {
            Vector2 c = sweepCenter;
            float g = FadeFrom(1.4f);
            float re = EaseOut(_t / 0.6f);

            int n = Ring(c.x, groundY, 1.0f * re, 0.2f * re);
            Draw(Taper.Flat, n, 0.04f * thickness, primary, g);
            n = Ring(c.x, groundY, 0.7f * re, 0.14f * re);
            Draw(Taper.Flat, n, 0.025f * thickness, primary, g * 0.8f);
            float rot = _t * 40f * Mathf.Deg2Rad;
            for (int t = 0; t < 2; t++)
            {
                for (int i = 0; i < 4; i++)
                {
                    float a = rot * (t == 0 ? 1f : -1f) + (i % 3) * 2.094f + t * 1.047f;
                    _buf[i] = new Vector3(c.x + 0.85f * re * Mathf.Cos(a), groundY + 0.17f * re * Mathf.Sin(a), 0f);
                }

                Draw(Taper.Flat, 4, 0.02f * thickness, primary, g * 0.8f);
            }

            float pe = EaseOut((_t - 0.4f) / 0.5f);
            if (pe > 0f)
            {
                int pn = Seg(c.x, groundY + 3.0f * pe, c.x, groundY, 2);
                Draw(Taper.Head, pn, 0.7f * thickness, primary * 0.5f, g * 0.8f);
                Draw(Taper.Head, pn, 0.25f * thickness, _core, g);
                for (int sp = 0; sp < 2; sp++)
                {
                    const int m = 20;
                    for (int i = 0; i < m; i++)
                    {
                        float s = i / (float)(m - 1);
                        float a = _t * 4f + s * 8f + sp * Mathf.PI;
                        _buf[i] = new Vector3(c.x + 0.4f * Mathf.Cos(a), groundY + s * 3.0f * pe, 0f);
                    }

                    Draw(Taper.Head, m, 0.05f * thickness, primary, g * 0.9f);
                }
            }

            if (_t >= 1.0f)
            {
                if (Once(1))
                {
                    Fire();
                    Emit(_dots, c.x, c.y, 0f, 0f, 5f, 0.35f, Hue(primary));
                    Burst(c.x, c.y, 70, 8f, 30, 4f);
                }

                float we = EaseOut((_t - 1.0f) / 0.4f);
                for (int side = -1; side <= 1; side += 2)
                {
                    for (int f = 0; f < 3; f++)
                    {
                        float a = (35f + f * 22f) * Mathf.Deg2Rad;
                        float L = (1.2f - 0.15f * f) * we;
                        int fn = Seg(c.x + side * 0.15f, c.y + 0.35f, c.x + side * (0.15f + Mathf.Cos(a) * L), c.y + 0.35f + Mathf.Sin(a) * L, 2);
                        Draw(Taper.Lens, fn, 0.12f * thickness, primary, g);
                        Draw(Taper.Lens, fn, 0.04f * thickness, _core, g);
                    }
                }

                int rn = Ring(c.x, c.y, 2.6f * we, 2.6f * we);
                Draw(Taper.Flat, rn, 0.07f * thickness, primary, (1f - we) * g);
            }

            if (_t < 1.0f)
            {
                _acc += _dt * 40f;
                while (_acc >= 1f)
                {
                    _acc -= 1f;
                    Emit(Random.value > 0.5f ? _stars : _dots, c.x + (Random.value - 0.5f) * 0.8f, groundY + Random.value * 0.3f, 0f, 1f + 2f * Random.value, 0.1f + 0.1f * Random.value, 0.9f, Hue(primary));
                }
            }
        }

        void AnimBerserk()
        {
            Vector2 c = sweepCenter;
            int fr = Mathf.FloorToInt(_t * 20f);
            float flick = 0.75f + 0.25f * Rnd(Mathf.FloorToInt(_t * 40f), 8);
            for (int k = 0; k < 6; k++)
            {
                float side = (k % 2 == 0) ? 1f : -1f;
                float x0 = c.x + side * (0.15f + 0.1f * (k / 2));
                float h = 0.9f + 0.4f * Rnd(k, fr);
                DrawBolt(new Vector2(x0, groundY), new Vector2(x0 + side * (0.1f + 0.15f * Rnd(k, 2)), c.y + h * 0.9f), 8, 0.12f, fr + k * 7, 0.07f, flick);
            }

            int n = Ring(c.x, c.y, BodyRx * 1.2f, BodyRy * 1.15f);
            Draw(Taper.Flat, n, 0.06f * thickness, primary, flick);
            n = Ring(c.x, groundY, 0.6f + 0.1f * Mathf.Sin(_t * 12f), 0.12f);
            Draw(Taper.Flat, n, 0.03f * thickness, primary, 0.9f);

            if (Live())
            {
                Emit(_dots, c.x, c.y, 0f, 0f, 1.6f, 0.07f, Tint(0.18f + 0.3f * _pulse));
                _acc += _dt * 30f;
                while (_acc >= 1f)
                {
                    _acc -= 1f;
                    Emit(_sparks, c.x + (Random.value - 0.5f) * 0.6f, groundY + 0.1f, (Random.value - 0.5f) * 0.6f, 1.5f + 2f * Random.value, 0.06f, 0.6f, Hue(primary));
                }
            }
        }

        void AnimCharacterAura()
        {
            Vector2 c = sweepCenter;
            float breathe = 1f + 0.05f * Mathf.Sin(_t * 2.5f);
            int n = Ring(c.x, c.y, BodyRx * 1.3f * breathe, BodyRy * 1.2f * breathe);
            Draw(Taper.Flat, n, 0.05f * thickness, primary, 0.9f);
            n = Ring(c.x, c.y, BodyRx * 1.6f * breathe, BodyRy * 1.4f * breathe);
            Draw(Taper.Flat, n, 0.1f * thickness, primary * 0.4f, 0.35f);
            float h = _t * 120f;
            n = ArcPts(c.x, c.y, BodyRx * 1.3f * breathe, BodyRy * 1.2f * breathe, h - 60f, h, 16);
            Draw(Taper.Head, n, 0.07f * thickness, _core, 1f);

            for (int w = 0; w < 4; w++)
            {
                float side = (w % 2 == 0) ? -1f : 1f;
                n = WaveDir(c.x + side * (0.35f + 0.08f * (w / 2)), groundY, 90f, 1.5f, 14, 0.06f, 8f, _t * 3f + w * 1.7f);
                Draw(Taper.Head, n, 0.06f * thickness, primary, 0.6f);
            }

            int pn = Seg(c.x, groundY + 2.2f, c.x, groundY, 2);
            Draw(Taper.Head, pn, 0.55f * thickness, primary * 0.4f, 0.25f);
            int gr = Ring(c.x, groundY, 0.6f, 0.12f);
            Draw(Taper.Flat, gr, 0.025f * thickness, primary, 0.8f);

            if (Live())
            {
                Emit(_dots, c.x, c.y, 0f, 0f, 1.5f, 0.07f, Tint(0.1f + 0.25f * _pulse));
                _acc += _dt * 14f;
                while (_acc >= 1f)
                {
                    _acc -= 1f;
                    Emit(Random.value > 0.5f ? _stars : _dots, c.x + (Random.value - 0.5f) * 0.8f, groundY + Random.value * 0.4f, 0f, 0.6f + Random.value, 0.09f + 0.08f * Random.value, 1.0f, Hue(primary));
                }
            }
        }
    }
}
