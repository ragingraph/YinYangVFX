using UnityEngine;

namespace YinYangVFX
{
    // Ultimate (big-move) effects. Large scale (about 8 units wide). Single colour per prefab.
    public partial class WeaponEffect
    {
        // Origin = target ground area centre (x), swords land at groundY.
        void AnimSkySwordRain()
        {
            float x0 = sweepCenter.x;
            float g = FadeFrom(1.6f);
            const int N = 14;
            for (int i = 0; i < N; i++)
            {
                float ts = 0.06f * i + 0.05f * Rnd(i, 3);
                float p = (_t - ts) / 0.22f;
                if (p < 0f) continue;
                float x = x0 + (i - (N - 1) * 0.5f) * 0.32f + (Rnd(i, 5) - 0.5f) * 0.15f;
                float len = 0.9f + 0.4f * Rnd(i, 7);
                float yEnd = groundY + 0.35f;
                if (p < 1f)
                {
                    float y = Mathf.Lerp(groundY + 5.0f, yEnd, p * p);
                    int n = Seg(x, y + len * 1.6f, x, y, 2);
                    Draw(Taper.Head, n, 0.16f * thickness, primary * 0.5f, 0.8f);
                    n = Seg(x, y + len, x, y, 2);
                    Draw(Taper.Head, n, 0.07f * thickness, primary, 1f);
                }
                else
                {
                    float f = Mathf.Clamp01((_t - ts - 0.22f) / 1.2f);
                    int n = Seg(x, yEnd + len * 0.8f, x, groundY, 2);
                    Draw(Taper.Head, n, 0.08f * thickness, primary, (1f - f) * g);
                    n = Ring(x, groundY, 0.5f * EaseOut(f * 3f), 0.1f * EaseOut(f * 3f));
                    Draw(Taper.Flat, n, 0.03f * thickness, primary, (1f - Mathf.Clamp01(f * 2f)) * g);
                    if (Once(1 << (i + 2)))
                    {
                        Fire();
                        Burst(x, groundY + 0.1f, 14, 4f, 5, 1.4f);
                        Emit(_dots, x, groundY + 0.2f, 0f, 0f, 1.6f, 0.2f, Hue(primary));
                    }
                }
            }
        }

        // Origin = caster body centre; slashes across the whole screen to the right (mirror for left).
        void AnimFullScreenSlash()
        {
            Vector2 c = sweepCenter;
            float g = FadeFrom(0.7f);
            float k = EaseOut(_t / 0.14f);
            if (Once(1))
            {
                Fire();
                Emit(_dots, c.x + 2.5f, c.y, 0f, 0f, 14f, 0.12f, Hue(primary));
                Emit(_dots, c.x + 2.5f, c.y, 0f, 0f, 8f, 0.4f, Tint(0.5f));
            }

            const int m = 24;
            for (int i = 0; i < m; i++)
            {
                float s = (float)i / (m - 1) * k;
                float x = Mathf.Lerp(c.x - 4.5f, c.x + 6.5f, s);
                float y = c.y + 2.4f - 3.6f * s + 0.8f * Mathf.Sin(s * Mathf.PI);
                _buf[i] = new Vector3(x, y, 0f);
            }

            float w = 1f - Mathf.Clamp01((_t - 0.14f) / 0.56f);
            Beam(Taper.Lens, m, 0.55f * (0.4f + 0.6f * w), g);

            if (Once(2))
            {
                for (int j = 0; j < 40; j++)
                {
                    float s = Random.value;
                    float x = Mathf.Lerp(c.x - 4.5f, c.x + 6.5f, s);
                    float y = c.y + 2.4f - 3.6f * s + 0.8f * Mathf.Sin(s * Mathf.PI);
                    Emit(Random.value > 0.5f ? _stars : _sparks, x, y, (Random.value - 0.5f) * 3f, (Random.value - 0.5f) * 3f, 0.1f + 0.1f * Random.value, 0.8f, Hue(primary));
                }
            }

            // shock ring and ground crack lines
            float re = EaseOut((_t - 0.1f) / 0.5f);
            if (_t > 0.1f)
            {
                int rn = Ring(c.x + 2.5f, groundY, 4.0f * re, 0.7f * re);
                Draw(Taper.Flat, rn, 0.06f * thickness, primary, (1f - re) * g);
                for (int j = 0; j < 6; j++)
                {
                    float a = (j - 2.5f) * 0.12f;
                    int n = Seg(c.x + 1f, groundY, c.x + 1f + Mathf.Cos(a) * 5.5f * re, groundY + Mathf.Sin(a) * 0.4f, 2);
                    Draw(Taper.Head, n, 0.04f * thickness, primary, (1f - re) * g);
                }
            }
        }

        // Origin = target area centre; meteors fall diagonally from the upper right to the lower left.
        void AnimMeteorRain()
        {
            float x0 = sweepCenter.x;
            float g = FadeFrom(2.0f);
            const int N = 9;
            Vector2 dir = new Vector2(-0.45f, -1f).normalized;
            for (int i = 0; i < N; i++)
            {
                float ts = 0.2f * i + 0.1f * Rnd(i, 2);
                float p = (_t - ts) / 0.5f;
                if (p < 0f) continue;
                Vector2 hit = new Vector2(x0 + (Rnd(i, 4) - 0.5f) * 5.0f, groundY + 0.05f);
                float dist = 6.0f;
                if (p < 1f)
                {
                    Vector2 hp = hit - dir * dist * (1f - p * p);
                    Vector2 tl = hp - dir * 1.8f;
                    int n = Seg(tl.x, tl.y, hp.x, hp.y, 2);
                    Draw(Taper.Head, n, 0.3f * thickness, primary * 0.5f, 0.9f);
                    n = Seg(tl.x + (hp.x - tl.x) * 0.3f, tl.y + (hp.y - tl.y) * 0.3f, hp.x, hp.y, 2);
                    Draw(Taper.Head, n, 0.14f * thickness, primary, 1f);
                    Emit(_dots, hp.x, hp.y, 0f, 0f, 0.6f, 0.07f, Hue(primary));
                    Emit(_sparks, hp.x, hp.y, -dir.x * 1.5f, -dir.y * 1.5f, 0.07f, 0.4f, Hue(primary));
                }
                else
                {
                    float f = Mathf.Clamp01((_t - ts - 0.5f) / 0.8f);
                    int n = Ring(hit.x, groundY, 1.1f * EaseOut(f * 2f), 0.25f * EaseOut(f * 2f));
                    Draw(Taper.Flat, n, 0.06f * thickness, primary, (1f - f) * g);
                    n = Seg(hit.x, groundY, hit.x, groundY + 1.0f * EaseOut(f * 3f), 2);
                    Draw(Taper.Head, n, 0.35f * (1f - f) * thickness, primary * 0.5f, (1f - f) * g);
                    if (Once(1 << (i + 2)))
                    {
                        Fire();
                        Burst(hit.x, groundY + 0.1f, 30, 6f, 10, 2.2f);
                        Emit(_dots, hit.x, groundY + 0.3f, 0f, 0f, 2.6f, 0.3f, Hue(primary));
                    }
                }
            }
        }

        // Origin = taiji centre (about 1.6 units radius) hovering in the air.
        void AnimTaijiUltimate()
        {
            Vector2 c = sweepCenter + new Vector2(0f, 0.8f);
            float g = FadeFrom(2.0f);
            float open = EaseOut(_t / 0.5f);
            float r = 1.5f * open;
            float th = _t * 150f;
            if (Once(1))
            {
                Burst(c.x, c.y, 20, 4f, 10, 1.6f);
            }

            Vector2 u = new Vector2(Mathf.Cos(th * Mathf.Deg2Rad), Mathf.Sin(th * Mathf.Deg2Rad));
            int n = Ring(c.x, c.y, r, r);
            Draw(Taper.Flat, n, 0.1f * thickness, primary, g);
            n = Ring(c.x, c.y, r * 1.2f, r * 1.2f);
            Draw(Taper.Flat, n, 0.03f * thickness, primary, 0.7f * g);
            n = ArcPts(c.x + u.x * r * 0.5f, c.y + u.y * r * 0.5f, r * 0.5f, r * 0.5f, th + 180f, th + 360f, 20);
            Draw(Taper.Flat, n, 0.1f * thickness, primary, g);
            n = ArcPts(c.x - u.x * r * 0.5f, c.y - u.y * r * 0.5f, r * 0.5f, r * 0.5f, th, th + 180f, 20);
            Draw(Taper.Flat, n, 0.1f * thickness, primary, g);
            n = Ring(c.x + u.x * r * 0.5f, c.y + u.y * r * 0.5f, r * 0.14f, r * 0.14f);
            Draw(Taper.Flat, n, 0.05f * thickness, primary, g);
            n = Ring(c.x - u.x * r * 0.5f, c.y - u.y * r * 0.5f, r * 0.14f, r * 0.14f);
            Draw(Taper.Flat, n, 0.05f * thickness, primary, g);
            Emit(_dots, c.x, c.y, 0f, 0f, 2.0f * r, 0.07f, Tint(0.25f));

            // rays and expanding waves after the taiji has formed
            if (_t > 0.6f)
            {
                float it = _t - 0.6f;
                if (Once(2))
                {
                    Fire();
                    Burst(c.x, c.y, 80, 9f, 30, 4f);
                    Emit(_dots, c.x, c.y, 0f, 0f, 6f, 0.4f, Hue(primary));
                }

                for (int j = 0; j < 3; j++)
                {
                    float d = Mathf.Clamp01((it - 0.15f * j) / 0.9f);
                    float rr = 4.0f * EaseOut(d);
                    n = Ring(c.x, c.y, rr, rr);
                    Draw(Taper.Flat, n, 0.12f * (1f - d) * thickness, primary, (1f - d) * g);
                }

                for (int k = 0; k < 16; k++)
                {
                    float a = (k * 22.5f + th * 0.3f) * Mathf.Deg2Rad;
                    float e = EaseOut(it / 0.5f);
                    n = Seg(c.x + Mathf.Cos(a) * r * 1.3f, c.y + Mathf.Sin(a) * r * 1.3f, c.x + Mathf.Cos(a) * (r * 1.3f + (k % 2 == 0 ? 3.0f : 1.8f) * e), c.y + Mathf.Sin(a) * (r * 1.3f + (k % 2 == 0 ? 3.0f : 1.8f) * e), 2);
                    Draw(Taper.Head, n, 0.07f * thickness, primary, (1f - Mathf.Clamp01(it / 1.2f)) * g);
                }
            }

            if (_t < 1.8f)
            {
                _acc += _dt * 60f;
                while (_acc >= 1f)
                {
                    _acc -= 1f;
                    float a = Random.value * 6.2832f;
                    Emit(Random.value > 0.5f ? _stars : _sparks, c.x + Mathf.Cos(a) * r * 1.2f, c.y + Mathf.Sin(a) * r * 1.2f, Mathf.Cos(a + 1.57f) * 1.5f, Mathf.Sin(a + 1.57f) * 1.5f, 0.08f + 0.08f * Random.value, 0.8f, Hue(primary));
                }
            }
        }

        // Origin = caster body centre; a serpentine dragon of qi shoots to the right.
        void AnimDragonQi()
        {
            Vector2 c = sweepCenter;
            float g = FadeFrom(1.3f);
            float adv = EaseOut(_t / 0.7f);
            float len = 6.0f * adv;
            float tailCut = Mathf.Clamp01((_t - 0.5f) / 0.8f);
            float x0 = c.x + 0.4f + len * tailCut * 0.9f;
            float wl = Mathf.Max(0.3f, len * (1f - tailCut * 0.9f));
            if (Once(1))
            {
                Emit(_dots, c.x + 0.5f, c.y, 0f, 0f, 3f, 0.2f, Hue(primary));
            }

            for (int w = 0; w < 3; w++)
            {
                int n = WaveDir(x0, c.y, 0f, wl, 40, 0.38f - 0.07f * w, 3.2f + w * 0.3f, -_t * 9f + w * 0.8f);
                Vector3 hp = _buf[n - 1];
                Draw(Taper.Tail, n, (0.55f - 0.13f * w) * thickness, primary * (w == 0 ? 0.5f : 1f), g);
                if (w == 2)
                {
                    int r = Ring(hp.x, hp.y, 0.25f, 0.25f);
                    Draw(Taper.Flat, r, 0.08f * thickness, primary, g);
                    Emit(_dots, hp.x, hp.y, 0f, 0f, 1.6f, 0.07f, Hue(primary));
                    // horns and whiskers at the head
                    int h = Seg(hp.x - 0.1f, hp.y + 0.1f, hp.x - 0.5f, hp.y + 0.5f, 2);
                    Draw(Taper.Head, h, 0.05f * thickness, primary, g);
                    h = Seg(hp.x - 0.1f, hp.y - 0.1f, hp.x - 0.5f, hp.y - 0.5f, 2);
                    Draw(Taper.Head, h, 0.05f * thickness, primary, g);
                    if (_t < 1.0f)
                    {
                        Emit(_sparks, hp.x, hp.y, 1f + Random.value, (Random.value - 0.5f) * 2f, 0.07f, 0.5f, Hue(primary));
                    }
                }
            }

            if (_t < 1.0f)
            {
                _acc += _dt * 80f;
                while (_acc >= 1f)
                {
                    _acc -= 1f;
                    Emit(Random.value > 0.5f ? _stars : _sparks, x0 + Random.value * wl, c.y + (Random.value - 0.5f) * 0.8f, (Random.value - 0.5f) * 0.5f, (Random.value - 0.5f) * 1f, 0.08f + 0.08f * Random.value, 0.7f, Hue(primary));
                }
            }

            if (_t >= 0.7f && Once(2))
            {
                Fire();
                float hx = c.x + 0.4f + 6.0f;
                Burst(hx, c.y, 60, 7f, 20, 3f);
                Emit(_dots, hx, c.y, 0f, 0f, 4f, 0.3f, Hue(primary));
            }
        }

        // Origin = caster body centre; dome/ring barrier expands outward.
        void AnimBarrierExpand()
        {
            Vector2 c = sweepCenter;
            float g = FadeFrom(1.1f);
            float e = EaseOut(_t / 0.6f);
            if (Once(1))
            {
                Fire();
                Burst(c.x, c.y, 40, 7f, 14, 2.4f);
                Emit(_dots, c.x, c.y, 0f, 0f, 4f, 0.3f, Hue(primary));
            }

            for (int j = 0; j < 3; j++)
            {
                float r = 4.0f * e * (1f - 0.18f * j);
                int n = Ring(c.x, c.y, r, r);
                Draw(Taper.Flat, n, (0.16f - 0.04f * j) * thickness, primary, (1f - 0.2f * j) * g * (1f - 0.3f * Mathf.Clamp01(_t / 1.4f)));
            }

            // hex-ish spokes inside the barrier
            for (int k = 0; k < 12; k++)
            {
                float a = (k * 30f + _t * 40f) * Mathf.Deg2Rad;
                int n = Seg(c.x + Mathf.Cos(a) * 4.0f * e * 0.55f, c.y + Mathf.Sin(a) * 4.0f * e * 0.55f, c.x + Mathf.Cos(a) * 4.0f * e, c.y + Mathf.Sin(a) * 4.0f * e, 2);
                Draw(Taper.Flat, n, 0.03f * thickness, primary, 0.6f * g);
            }

            int gr = Ring(c.x, groundY, 4.0f * e, 0.8f * e);
            Draw(Taper.Flat, gr, 0.05f * thickness, primary, g);
            Emit(_dots, c.x, c.y, 0f, 0f, 8.0f * e, 0.07f, Tint(0.08f * g));
            if (_t < 0.9f)
            {
                _acc += _dt * 90f;
                while (_acc >= 1f)
                {
                    _acc -= 1f;
                    float a = Random.value * 6.2832f;
                    Emit(Random.value > 0.5f ? _stars : _sparks, c.x + Mathf.Cos(a) * 4.0f * e, c.y + Mathf.Sin(a) * 4.0f * e, Mathf.Cos(a) * 0.8f, Mathf.Sin(a) * 0.8f, 0.09f + 0.09f * Random.value, 0.8f, Hue(primary));
                }
            }
        }
    }
}
