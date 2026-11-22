using UnityEngine;

namespace YinYangVFX
{
    // Pickup / environment effects. Origin = object centre (ground at groundY). Single colour per prefab.
    public partial class WeaponEffect
    {
        void Flare(float x, float y, float len, float a, float rot)
        {
            for (int k = 0; k < 4; k++)
            {
                float ang = (rot + k * 90f) * Mathf.Deg2Rad;
                float L = len * (k % 2 == 0 ? 1f : 0.6f);
                int n = Seg(x - Mathf.Cos(ang) * L, y - Mathf.Sin(ang) * L, x + Mathf.Cos(ang) * L, y + Mathf.Sin(ang) * L, 2);
                Draw(Taper.Lens, n, 0.08f * thickness, primary, a);
            }
        }

        void AnimCoinPickup()
        {
            Vector2 c = sweepCenter;
            float g = FadeFrom(0.35f);
            float e = EaseOut(_t / 0.3f);
            if (Once(1))
            {
                Fire();
                Burst(c.x, c.y, 14, 4f, 8, 1.8f);
                Emit(_dots, c.x, c.y, 0f, 0f, 1.6f, 0.2f, Hue(primary));
            }

            int n = Ring(c.x, c.y, 0.6f * e, 0.6f * e);
            Draw(Taper.Flat, n, 0.05f * thickness, primary, (1f - e) * g);
            Flare(c.x, c.y, 0.7f * (1f - 0.5f * Mathf.Clamp01(_t / 0.5f)), g, 20f + _t * 120f);
            for (int k = 0; k < 4; k++)
            {
                float f = Mathf.Clamp01(_t / 0.6f);
                float a = (k * 90f + 45f) * Mathf.Deg2Rad;
                int s = Seg(c.x + Mathf.Cos(a) * (0.3f + 0.9f * f), c.y + Mathf.Sin(a) * (0.3f + 0.9f * f), c.x + Mathf.Cos(a) * (0.5f + 1.2f * f), c.y + Mathf.Sin(a) * (0.5f + 1.2f * f), 2);
                Draw(Taper.Head, s, 0.04f * thickness, primary, (1f - f) * g);
            }
        }

        void AnimChestOpen()
        {
            Vector2 c = sweepCenter;
            float top = c.y + 0.3f;
            float g = FadeFrom(1.3f);
            float e = EaseOut(_t / 0.3f);
            if (Once(1))
            {
                Fire();
                Burst(c.x, top, 40, 5f, 18, 2.6f);
                Emit(_dots, c.x, top, 0f, 0f, 3.0f, 0.4f, Hue(primary));
            }

            for (int k = 0; k < 9; k++)
            {
                float a = (90f + (k - 4) * 14f) * Mathf.Deg2Rad;
                float L = (1.4f + 1.4f * Rnd(k, 2)) * e;
                int n = Seg(c.x + Mathf.Cos(a) * 0.1f, top + Mathf.Sin(a) * 0.1f, c.x + Mathf.Cos(a) * L, top + Mathf.Sin(a) * L, 2);
                Draw(Taper.Head, n, (0.1f + 0.1f * Rnd(k, 3)) * thickness, primary * 0.7f, (1f - Mathf.Clamp01((_t - 0.4f) / 0.9f)) * g);
            }

            int rn = Ring(c.x, top, 0.9f * e, 0.2f * e);
            Draw(Taper.Flat, rn, 0.05f * thickness, primary, (1f - e * 0.7f) * g);
            Emit(_dots, c.x, top, 0f, 0f, 1.8f, 0.07f, Tint(0.25f * g));
            if (_t < 1.0f)
            {
                _acc += _dt * 50f;
                while (_acc >= 1f)
                {
                    _acc -= 1f;
                    Emit(Random.value > 0.4f ? _stars : _sparks, c.x + (Random.value - 0.5f) * 0.5f, top, (Random.value - 0.5f) * 1.2f, 1.5f + 2.5f * Random.value, 0.08f + 0.09f * Random.value, 1.0f, Hue(primary));
                }
            }
        }

        void AnimItemPickup()
        {
            Vector2 c = sweepCenter;
            float g = FadeFrom(0.6f);
            float inK = Mathf.Clamp01(_t / 0.25f);
            if (_t < 0.25f)
            {
                int n = Ring(c.x, c.y, 0.9f * (1f - inK), 0.9f * (1f - inK));
                Draw(Taper.Flat, n, 0.05f * thickness, primary, inK);
                _acc += _dt * 80f;
                while (_acc >= 1f)
                {
                    _acc -= 1f;
                    float a = Random.value * 6.2832f;
                    Emit(Random.value > 0.5f ? _stars : _sparks, c.x + Mathf.Cos(a) * 0.9f, c.y + Mathf.Sin(a) * 0.9f, -Mathf.Cos(a) * 3.5f, -Mathf.Sin(a) * 3.5f, 0.07f + 0.06f * Random.value, 0.25f, Hue(primary));
                }

                return;
            }

            float it = _t - 0.25f;
            if (Once(1))
            {
                Fire();
                Burst(c.x, c.y, 22, 5f, 12, 2.4f);
                Emit(_dots, c.x, c.y, 0f, 0f, 2.4f, 0.25f, Hue(primary));
            }

            Flare(c.x, c.y, 1.1f * EaseOut(it / 0.15f) * (1f - Mathf.Clamp01(it / 0.5f) * 0.6f), g, 45f + it * 90f);
            float re = EaseOut(it / 0.4f);
            int rn = Ring(c.x, c.y, 1.0f * re, 1.0f * re);
            Draw(Taper.Flat, rn, 0.06f * thickness, primary, (1f - re) * g);
        }

        void AnimHealItem()
        {
            Vector2 c = sweepCenter;
            float g = FadeFrom(1.1f);
            if (Once(1))
            {
                Fire();
                Burst(c.x, c.y, 18, 3f, 12, 1.8f);
                Emit(_dots, c.x, c.y, 0f, 0f, 2.2f, 0.3f, Hue(primary));
            }

            for (int i = 0; i < 3; i++)
            {
                float f = Mathf.Repeat(_t * 0.9f + i / 3f, 1f);
                if (_t / 0.9f + i / 3f > 2.2f) continue;
                float y = c.y + f * 1.6f, s = 0.2f * (1f - 0.3f * f);
                float al = Mathf.Sin(f * Mathf.PI) * g;
                int n = Seg(c.x - s, y, c.x + s, y, 2);
                Draw(Taper.Flat, n, 0.09f * thickness, primary, al);
                n = Seg(c.x, y - s, c.x, y + s, 2);
                Draw(Taper.Flat, n, 0.09f * thickness, primary, al);
            }

            float e = EaseOut(_t / 0.5f);
            int rn = Ring(c.x, groundY, 0.8f * e, 0.16f * e);
            Draw(Taper.Flat, rn, 0.04f * thickness, primary, (1f - e * 0.5f) * g);
            int pn = Seg(c.x, groundY, c.x, groundY + 1.8f * e, 2);
            Draw(Taper.Head, pn, 0.5f * thickness, primary * 0.4f, g * (1f - Mathf.Clamp01(_t / 1.0f)));
            if (_t < 1.0f)
            {
                _acc += _dt * 40f;
                while (_acc >= 1f)
                {
                    _acc -= 1f;
                    Emit(Random.value > 0.5f ? _stars : _sparks, c.x + (Random.value - 0.5f) * 0.7f, groundY + Random.value * 0.3f, (Random.value - 0.5f) * 0.3f, 0.8f + 1.5f * Random.value, 0.08f + 0.08f * Random.value, 1.0f, Hue(primary));
                }
            }
        }

        // Looping save point / checkpoint. Use with loop=true.
        void AnimSavePoint()
        {
            Vector2 c = sweepCenter;
            float x = c.x;
            float spin = _t * 70f;
            MagicCircle(x, groundY, 0.7f, 0.9f, spin);
            for (int i = 0; i < 3; i++)
            {
                float f = Mathf.Repeat(_t * 0.5f + i / 3f, 1f);
                int n = Ring(x, groundY + f * 1.4f, 0.4f * (1f - 0.4f * f), 0.09f * (1f - 0.4f * f));
                Draw(Taper.Flat, n, 0.03f * thickness, primary, Mathf.Sin(f * Mathf.PI) * 0.8f);
            }

            int pn = Seg(x, groundY, x, groundY + 1.8f, 2);
            Draw(Taper.Head, pn, 0.35f * thickness * (0.85f + 0.15f * Mathf.Sin(_t * 3f)), primary * 0.35f, 0.9f);
            Emit(_dots, x, groundY + 0.1f, 0f, 0f, 1.2f, 0.07f, Tint(0.2f + 0.2f * _pulse));
            if (Live())
            {
                _acc += _dt * 14f;
                while (_acc >= 1f)
                {
                    _acc -= 1f;
                    Emit(Random.value > 0.4f ? _stars : _sparks, x + (Random.value - 0.5f) * 0.9f, groundY + 0.1f, (Random.value - 0.5f) * 0.2f, 0.5f + 0.8f * Random.value, 0.07f + 0.06f * Random.value, 1.6f, Hue(primary));
                }
            }
        }

        void AnimMechanismActivate()
        {
            Vector2 c = sweepCenter;
            float g = FadeFrom(0.8f);
            if (Once(1))
            {
                Fire();
                Burst(c.x, c.y, 24, 4f, 8, 2.0f);
                Emit(_dots, c.x, c.y, 0f, 0f, 2.4f, 0.25f, Hue(primary));
            }

            for (int j = 0; j < 3; j++)
            {
                float d = Mathf.Clamp01((_t - 0.1f * j) / 0.6f);
                int n = Ring(c.x, c.y, 1.2f * EaseOut(d), 1.2f * EaseOut(d));
                Draw(Taper.Flat, n, 0.07f * (1f - d) * thickness, primary, (1f - d) * g);
            }

            float spin = _t * 300f;
            for (int k = 0; k < 4; k++)
            {
                int n = ArcPts(c.x, c.y, 0.55f, 0.55f, spin + k * 90f, spin + k * 90f + 55f, 8);
                Draw(Taper.Flat, n, 0.08f * thickness, primary, g);
            }

            for (int k = 0; k < 8; k++)
            {
                float a = (k * 45f + spin * 0.2f) * Mathf.Deg2Rad;
                float e = EaseOut(_t / 0.4f);
                int n = Seg(c.x + Mathf.Cos(a) * 0.6f, c.y + Mathf.Sin(a) * 0.6f, c.x + Mathf.Cos(a) * (0.6f + 0.8f * e), c.y + Mathf.Sin(a) * (0.6f + 0.8f * e), 2);
                Draw(Taper.Head, n, 0.05f * thickness, primary, (1f - Mathf.Clamp01(_t / 0.8f)) * g);
            }
        }

        // Spike trap: warning line flashes, then spikes shoot up.
        void AnimSpikeTrap()
        {
            Vector2 c = sweepCenter;
            const float warn = 0.5f;
            float g = FadeFrom(0.9f);
            if (_t < warn)
            {
                float fl = 0.5f + 0.5f * Mathf.Sin(_t * 40f);
                int n = Seg(c.x - 1.0f, groundY, c.x + 1.0f, groundY, 2);
                Draw(Taper.Flat, n, 0.06f * thickness, primary, fl);
                n = Ring(c.x, groundY, 1.0f, 0.1f);
                Draw(Taper.Flat, n, 0.03f * thickness, primary, fl * 0.7f);
                return;
            }

            float it = _t - warn;
            float up = EaseOut(it / 0.08f) * (1f - Mathf.Clamp01((it - 0.35f) / 0.3f));
            if (Once(1))
            {
                Fire();
                Burst(c.x, groundY, 24, 5f, 8, 1.8f);
            }

            for (int k = 0; k < 5; k++)
            {
                float x = c.x + (k - 2) * 0.4f;
                float h = (0.9f + 0.3f * Rnd(k, 1)) * up;
                int n = Seg(x, groundY, x, groundY + h, 2);
                Draw(Taper.Head, n, 0.16f * thickness, primary * 0.5f, g);
                n = Seg(x, groundY, x, groundY + h, 2);
                Draw(Taper.Head, n, 0.06f * thickness, primary, g);
            }

            int rn = Ring(c.x, groundY, 1.4f * EaseOut(it / 0.3f), 0.2f * EaseOut(it / 0.3f));
            Draw(Taper.Flat, rn, 0.04f * thickness, primary, (1f - Mathf.Clamp01(it / 0.4f)) * g);
        }
    }
}
