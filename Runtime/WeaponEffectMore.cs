using UnityEngine;

namespace YinYangVFX
{
    // Second batch of weapon effects (spear rising / flurry, hammer sweep / quake, sword slash / whirl / lunge,
    // blade chop / drag, halberd crescent / lift-thrust, whip, nunchaku, palm wave, dart, arrow).
    // Every effect uses a single colour: the "primary" colour of the prefab.
    public partial class WeaponEffect
    {
        // ------------------------------------------------------------------ helpers

        float FadeFrom(float start)
        {
            return 1f - Mathf.Clamp01((_t - start) / Mathf.Max(0.05f, duration - start));
        }

        void Fire()
        {
            if (!_fired)
            {
                _fired = true;
                if (onImpact != null) onImpact.Invoke();
            }
        }

        bool Once(int bit)
        {
            if ((_flags & bit) != 0)
            {
                return false;
            }

            _flags |= bit;
            return true;
        }

        /// <summary>Wide halo + mid glow + body + hot core + a thin energy ribbon, all in the same colour.</summary>
        void Beam(Taper tp, int n, float w, float g)
        {
            Draw(tp, n, w * 3.2f * thickness, primary * 0.35f, g * 0.45f);
            Draw(tp, n, w * 1.7f * thickness, primary * 0.8f, g * 0.7f);
            Draw(tp, n, w * thickness, primary, g);
            Draw(tp, n, w * 0.4f * thickness, _core, g);

            // ribbon winding around the stroke (modifies the scratch buffer, so it goes last)
            if (n >= 4)
            {
                float amp = Mathf.Clamp(w * 0.7f, 0.05f, 0.22f);
                Vector3 prev = _buf[0];
                for (int i = 0; i < n; i++)
                {
                    Vector3 a = _buf[Mathf.Max(i - 1, 0)];
                    Vector3 b = _buf[Mathf.Min(i + 1, n - 1)];
                    Vector3 d = (b - a);
                    d.z = 0f;
                    if (d.sqrMagnitude > 1e-6f)
                    {
                        d.Normalize();
                    }

                    float s = i / (float)(n - 1);
                    float off = amp * Mathf.Sin(s * 16f - _t * 28f) * Mathf.Sin(Mathf.PI * Mathf.Clamp01(s));
                    Vector3 cur = _buf[i];
                    _buf[i] = new Vector3(cur.x - d.y * off, cur.y + d.x * off, 0f);
                    prev = cur;
                }

                Draw(tp, n, w * 0.22f * thickness, primary, g * 0.9f);
            }
        }

        int BezPts(Vector2 a, Vector2 b, Vector2 c, float s0, float s1, int n)
        {
            for (int i = 0; i < n; i++)
            {
                Vector2 q = Bez(a, b, c, Mathf.Lerp(s0, s1, i / (float)(n - 1)));
                _buf[i] = new Vector3(q.x, q.y, 0f);
            }

            return n;
        }

        int ArcPts(float cx, float cy, float rx, float ry, float a0Deg, float a1Deg, int n)
        {
            for (int i = 0; i < n; i++)
            {
                float a = Mathf.Lerp(a0Deg, a1Deg, i / (float)(n - 1)) * Mathf.Deg2Rad;
                _buf[i] = new Vector3(cx + rx * Mathf.Cos(a), cy + ry * Mathf.Sin(a), 0f);
            }

            return n;
        }

        void Burst(float x, float y, int sparks, float speed, int stars, float flash)
        {
            Emit(_dots, x, y, 0f, 0f, flash, 0.22f, Hue(primary));
            Emit(_dots, x, y, 0f, 0f, flash * 0.55f, 0.4f, Hue(primary));
            Emit(_dots, x, y, 0f, 0f, flash * 1.9f, 0.5f, Tint(0.3f));
            for (int j = 0; j < sparks; j++)
            {
                float a = Random.value * 6.2832f;
                float sp = speed * (0.4f + 0.8f * Random.value);
                Emit(_sparks, x, y, Mathf.Cos(a) * sp, Mathf.Sin(a) * sp, 0.05f + 0.04f * Random.value, 0.35f + 0.2f * Random.value, Hue(primary));
            }

            for (int j = 0; j < stars; j++)
            {
                Emit(_stars, x + (Random.value - 0.5f) * 0.8f, y + (Random.value - 0.5f) * 0.8f, (Random.value - 0.5f) * 1f, (Random.value - 0.5f) * 1f,
                     0.13f + 0.2f * Random.value, 0.5f + 0.3f * Random.value, Hue(primary));
            }
        }

        void Trail(float x, float y, float rate)
        {
            _acc += _dt * rate;
            while (_acc >= 1f)
            {
                _acc -= 1f;
                Emit(_sparks, x + (Random.value - 0.5f) * 0.15f, y + (Random.value - 0.5f) * 0.15f, (Random.value - 0.5f) * 1.2f,
                     (Random.value - 0.3f) * 1.4f, 0.05f, 0.3f, Hue(primary));
            }
        }

        // ------------------------------------------------------------------ spear: rising

        void AnimRising()
        {
            float g = FadeFrom(0.25f);
            float h = EaseOut(_t / 0.2f);
            Vector2 b0 = new Vector2(0.3f, -0.6f), b1 = new Vector2(1.5f, -0.3f), b2 = new Vector2(0.9f, 1.2f);
            float s0 = Mathf.Max(0f, h - 0.75f);
            int n = BezPts(b0, b1, b2, s0, h, 28);
            Beam(Taper.Head, n, 0.24f, g);
            n = BezPts(b0 + new Vector2(0.15f, 0.1f), b1 + new Vector2(0.2f, 0.15f), b2 + new Vector2(0.1f, 0f), s0, h, 28);
            Draw(Taper.Head, n, 0.08f * thickness, primary, g * 0.8f);

            if (h < 1f)
            {
                Vector2 q = Bez(b0, b1, b2, h);
                Emit(_dots, q.x, q.y, 0f, 0f, 0.6f, 0.07f, Hue(primary));
                Trail(q.x, q.y, 80f);
            }

            if (Once(1))
            {
                for (int j = 0; j < 12; j++)
                {
                    Emit(_sparks, 0.5f + Random.value * 0.6f, groundY + 0.1f, (Random.value - 0.3f) * 1.5f, 1.5f + 2f * Random.value, 0.05f, 0.5f, Hue(primary));
                }
            }

            if (_t >= 0.12f) Fire();
            if (_t >= 0.2f && Once(2)) Burst(b2.x, b2.y, 16, 4f, 8, 1.4f);
        }

        // ------------------------------------------------------------------ spear: flurry

        void AnimFlurry()
        {
            for (int k = 0; k < 5; k++)
            {
                float start = 0.08f + 0.09f * k;
                float lt = _t - start;
                if (lt < 0f)
                {
                    continue;
                }

                float y = (Rnd(k, 3) - 0.5f) * 0.7f;
                float tipX = reach * (0.75f + 0.25f * Rnd(k, 4));
                float p = EaseOut(lt / 0.06f);
                float fade = Mathf.Clamp01((lt - 0.06f) / 0.3f);
                float g = 1f - fade;
                float tip = Mathf.Lerp(0.5f, tipX, p);
                float x0 = Mathf.Lerp(tip - 1.3f, tip - 0.05f, fade);
                int n = Seg(x0, y, tip, y, 10);
                Beam(Taper.Head, n, 0.14f, g);

                if (lt >= 0.06f)
                {
                    if (Once(1 << k))
                    {
                        if (onImpact != null) onImpact.Invoke();
                        Burst(tip + 0.05f, y, 8, 3.5f, 3, 0.8f);
                    }

                    float rp = Mathf.Clamp01((lt - 0.06f) / 0.25f);
                    n = Ring(tip + 0.05f, y, 0.06f + 0.06f * rp, 0.2f + 0.15f * rp);
                    Draw(Taper.Flat, n, 0.025f * thickness, primary, (1f - rp) * g);
                }
            }
        }

        // ------------------------------------------------------------------ hammer: sweep

        void AnimHammerSweep()
        {
            const float sweepTime = 0.28f;
            float p = Mathf.Clamp01(_t / sweepTime);
            float pe = p * p;
            float g = FadeFrom(0.45f);
            float cy = groundY + 0.18f;
            float head = 180f + 180f * pe;
            float a0 = Mathf.Max(180f, head - 110f);
            float a1 = Mathf.Max(head, a0 + 1f);

            int n = ArcPts(0f, cy, 1.7f, 0.5f, a0, a1, 30);
            Beam(Taper.Head, n, 0.34f, g);
            Vector3 hp = _buf[n - 1];
            n = ArcPts(0f, cy + 0.3f, 1.6f, 0.5f, a0, a1, 30);
            Draw(Taper.Head, n, 0.12f * thickness, primary, g * 0.7f);

            if (p < 1f)
            {
                Emit(_dots, hp.x, hp.y, 0f, 0f, 0.8f, 0.08f, Hue(primary));
                Trail(hp.x, groundY + 0.05f, 110f);
            }

            if (p >= 0.85f) Fire();
            if (p >= 1f && Once(1))
            {
                Burst(1.55f, groundY + 0.05f, 28, 5f, 8, 1.8f);
                for (int j = 0; j < 6; j++)
                {
                    Emit(_dots, 0.9f + j * 0.3f, groundY + 0.1f, 0.4f, 0.2f, 0.9f, 0.9f, Hue(primary) * 0.35f + new Color(0f, 0f, 0f, 1f));
                }
            }

            if (_t >= sweepTime)
            {
                float bp = Mathf.Clamp01((_t - sweepTime) / 0.45f);
                float e = EaseOut(bp);
                for (int j = 0; j < 2; j++)
                {
                    n = Ring(1.3f + 0.6f * e, groundY, (0.4f + 1.1f * e) * (1f + 0.4f * j), (0.1f + 0.22f * e) * (1f + 0.4f * j));
                    Draw(Taper.Flat, n, (0.034f - 0.008f * j) * thickness, primary, (1f - bp * 0.6f) * g * (1f - 0.3f * j));
                }
            }
        }

        // ------------------------------------------------------------------ hammer: charge + quake

        void AnimQuake()
        {
            const float chargeEnd = 0.6f;
            Vector2 imp = impactOffset;
            Vector2 hr = new Vector2(0.35f, 0.95f);

            if (_t < chargeEnd)
            {
                float k = _t / chargeEnd;
                for (int j = 0; j < 3; j++)
                {
                    float kk = Mathf.Repeat(k * 1.6f + j / 3f, 1f);
                    float r = 0.9f * (1f - kk);
                    int rn = Ring(hr.x, hr.y, r, r);
                    Draw(Taper.Flat, rn, 0.022f * thickness, primary, kk * (0.5f + 0.5f * k));
                }

                Emit(_dots, hr.x, hr.y, 0f, 0f, 0.6f + 1.4f * k, 0.07f, Hue(primary));
                int n = Seg(hr.x, hr.y - 0.1f, hr.x, hr.y + 2.5f, 2);
                Draw(Taper.Head, n, (0.05f + 0.25f * k) * thickness, primary, 0.3f + 0.7f * k);

                _acc += _dt * 70f;
                while (_acc >= 1f)
                {
                    _acc -= 1f;
                    float a = Random.value * 6.2832f;
                    Emit(_stars, hr.x + Mathf.Cos(a) * 0.9f, hr.y + Mathf.Sin(a) * 0.9f, -Mathf.Cos(a) * 1.8f, -Mathf.Sin(a) * 1.8f,
                         0.12f + 0.1f * Random.value, 0.45f, Hue(primary));
                }

                return;
            }

            float it = _t - chargeEnd;
            float g = FadeFrom(chargeEnd + 0.55f);
            if (Once(1))
            {
                Fire();
                Emit(_dots, imp.x, imp.y + 0.05f, 0f, 0f, 4f, 0.3f, Hue(primary));
                Burst(imp.x, imp.y + 0.05f, 60, 6.5f, 24, 3.5f);
                for (int j = 0; j < 9; j++)
                {
                    float a = Mathf.Lerp(8f, 172f, j / 8f) * Mathf.Deg2Rad;
                    Emit(_sparks, imp.x, imp.y + 0.05f, Mathf.Cos(a) * 8f, Mathf.Sin(a) * 8f, 0.06f, 0.22f, Hue(primary));
                }

                for (int j = 0; j < 50; j++)
                {
                    Emit(_sparks, imp.x + (Random.value - 0.5f) * 0.6f, imp.y + 0.05f, (Random.value - 0.5f) * 4f, 1.6f + 2.4f * Random.value,
                         0.05f + 0.06f * Random.value, 0.9f + 0.4f * Random.value, Hue(primary));
                }

                for (int j = 0; j < 8; j++)
                {
                    Emit(_dots, imp.x + (j - 3.5f) * 0.4f, imp.y + 0.1f, (j - 3.5f) * 0.3f, 0.25f, 1.1f, 1.1f, Hue(primary) * 0.35f + new Color(0f, 0f, 0f, 1f));
                }
            }

            float pil = 1f - Mathf.Clamp01(it / 0.45f);
            if (pil > 0f)
            {
                int n = Seg(imp.x, imp.y + 5.5f, imp.x, imp.y + 0.05f, 2);
                Draw(Taper.Head, n, 0.7f * thickness, primary * 0.6f, pil * 0.8f);
                Draw(Taper.Head, n, 0.25f * thickness, _core, pil);
            }

            float gr = EaseOut(it / 0.45f);
            for (int j = 0; j < _cracks.Length; j++)
            {
                int m = Mathf.Clamp(Mathf.CeilToInt(gr * _cracks[j].Length), 2, _cracks[j].Length);
                for (int i = 0; i < m; i++)
                {
                    _buf[i] = new Vector3(_cracks[j][i].x, _cracks[j][i].y, 0f);
                }

                Draw(Taper.Tail, m, 0.07f * thickness, primary, g * 0.9f);
            }

            float rp = EaseOut(it / 0.7f);
            for (int j = 0; j < 4; j++)
            {
                float rx = (j == 0 ? 2.4f : j == 1 ? 1.7f : j == 2 ? 1.1f : 0.6f) * rp;
                int n = Ring(imp.x, imp.y, rx, rx * 0.22f);
                Draw(Taper.Flat, n, (0.04f - 0.007f * j) * thickness, primary, g * (1f - 0.15f * j) * (1f - rp * 0.4f));
            }
        }

        // ------------------------------------------------------------------ sword: slash / whirl / lunge

        void AnimSwordSlash()
        {
            float g = FadeFrom(0.15f);
            float h = EaseOut(_t / 0.14f);
            Vector2 b0 = new Vector2(0.15f, 1.0f), b1 = new Vector2(1.6f, 0.9f), b2 = new Vector2(1.3f, -0.6f);
            bool done = h >= 0.999f;
            float s0 = done ? 0f : Mathf.Max(0f, h - 0.85f);
            int n = BezPts(b0, b1, b2, s0, h, 26);
            Beam(done ? Taper.Lens : Taper.Head, n, 0.2f, g);

            if (!done)
            {
                Vector2 q = Bez(b0, b1, b2, h);
                Emit(_dots, q.x, q.y, 0f, 0f, 0.55f, 0.07f, Hue(primary));
                Trail(q.x, q.y, 90f);
            }

            if (_t >= 0.08f) Fire();
            if (done && Once(1)) Burst(b2.x, b2.y, 10, 3.5f, 5, 1.1f);
        }

        void AnimWhirl()
        {
            const float spinTime = 0.6f;
            float p = Mathf.Clamp01(_t / spinTime);
            float pe = 1f - (1f - p) * (1f - p);
            float g = FadeFrom(spinTime);
            Vector2 c = sweepCenter;
            Vector3 hp = Vector3.zero;

            for (int layer = 0; layer < 3; layer++)
            {
                float yo = (layer - 1) * 0.3f;
                float rm = layer == 1 ? 1f : 0.85f;
                float lag = layer * 25f;
                float head = -90f + 780f * pe - lag;
                float a0 = Mathf.Max(head - 300f, -90f);
                float a1 = Mathf.Max(head, a0 + 1f);
                const int n = 70;
                for (int i = 0; i < n; i++)
                {
                    float a = Mathf.Lerp(a0, a1, i / (float)(n - 1)) * Mathf.Deg2Rad;
                    _buf[i] = new Vector3(c.x + sweepRadius.x * 0.85f * rm * Mathf.Cos(a), c.y + yo + sweepRadius.y * rm * Mathf.Sin(a), 0f);
                }

                Draw(Taper.Head, n, (layer == 1 ? 0.3f : 0.2f) * thickness, primary * 0.45f, g * 0.5f);
                Draw(Taper.Head, n, (layer == 1 ? 0.13f : 0.08f) * thickness, primary, g);
                if (layer == 1)
                {
                    Draw(Taper.Head, n, 0.05f * thickness, _core, g);
                    hp = _buf[n - 1];
                }
            }

            if (p < 1f)
            {
                Emit(_dots, hp.x, hp.y, 0f, 0f, 0.6f, 0.07f, Hue(primary));
                _acc += _dt * 60f;
                while (_acc >= 1f)
                {
                    _acc -= 1f;
                    Emit(_stars, c.x + (Random.value - 0.5f) * 1.8f, c.y - 0.5f, 0f, 1.2f + Random.value, 0.1f + 0.12f * Random.value, 0.5f, Hue(primary));
                }
            }

            if (p >= 0.3f) Fire();
            int rn = Ring(c.x, groundY, sweepRadius.x * 0.8f, 0.09f);
            Draw(Taper.Flat, rn, 0.016f * thickness, primary, g * 0.6f);
        }

        void AnimLunge()
        {
            const float t0 = 0.04f;
            const float t1 = 0.12f;
            float fade = Mathf.Clamp01((_t - t1) / Mathf.Max(0.05f, duration - t1));
            float g = 1f - fade;
            float p = Mathf.Clamp01((_t - t0) / (t1 - t0));
            float e = EaseOut(p);
            float tip = Mathf.Lerp(0.4f, reach * 0.7f, e);

            if (_t < t0)
            {
                Emit(_dots, 0.4f, 0f, 0f, 0f, 0.35f, 0.07f, Hue(primary));
                return;
            }

            float x0 = Mathf.Lerp(tip - 0.9f * e - 0.1f, tip - 0.03f, fade);
            int n = Seg(x0, 0f, tip, 0f, 10);
            Beam(Taper.Head, n, 0.2f, g);

            for (int j = 0; j < 4; j++)
            {
                float y = (j % 2 == 0 ? 1f : -1f) * (0.12f + 0.3f * Rnd(j));
                float xe = tip - 0.1f - 0.4f * Rnd(j, 1);
                n = Seg(xe - 0.5f - 0.4f * Rnd(j, 2), y, xe, y, 2);
                Draw(Taper.Head, n, 0.025f * thickness, primary, g * 0.6f);
            }

            if (p >= 1f)
            {
                if (Once(1))
                {
                    Fire();
                    Burst(tip + 0.05f, 0f, 14, 4f, 6, 1.3f);
                }

                float fp = Mathf.Clamp01((_t - t1) / 0.25f);
                float len = 0.45f * (1f - fp * 0.5f);
                n = Seg(tip - len, -len * 0.7f, tip + len, len * 0.7f, 2);
                Draw(Taper.Lens, n, 0.07f * thickness, primary, (1f - fp) * g);
                n = Seg(tip - len, len * 0.7f, tip + len, -len * 0.7f, 2);
                Draw(Taper.Lens, n, 0.07f * thickness, primary, (1f - fp) * g);
            }
        }

        // ------------------------------------------------------------------ big blade: chop / drag

        void AnimChop()
        {
            float g = FadeFrom(0.3f);
            float h = Mathf.Pow(Mathf.Clamp01(_t / 0.2f), 1.6f);
            Vector2 b0 = new Vector2(-0.3f, 1.5f), b1 = new Vector2(1.9f, 1.3f), b2 = new Vector2(impactOffset.x * 1.1f, groundY + 0.1f);
            float s0 = Mathf.Max(0f, h - 0.7f);
            int n = BezPts(b0, b1, b2, s0, h, 30);
            Beam(Taper.Head, n, 0.5f, g);
            n = BezPts(b0 + new Vector2(-0.25f, 0.1f), b1 + new Vector2(-0.3f, 0.1f), b2 + new Vector2(-0.2f, 0.1f), s0, h, 30);
            Draw(Taper.Head, n, 0.22f * thickness, primary, g * 0.5f);

            if (h < 1f)
            {
                Vector2 q = Bez(b0, b1, b2, h);
                Emit(_dots, q.x, q.y, 0f, 0f, 0.9f, 0.08f, Hue(primary));
                Trail(q.x, q.y, 100f);
            }

            if (h >= 1f && Once(1))
            {
                Fire();
                Burst(b2.x, b2.y, 30, 5f, 10, 2.2f);
            }

            if (h >= 1f)
            {
                float rp = EaseOut((_t - 0.2f) / 0.4f);
                for (int j = 0; j < 2; j++)
                {
                    n = Ring(b2.x, groundY, (0.2f + 1.0f * rp) * (1f + 0.5f * j), (0.05f + 0.2f * rp) * (1f + 0.5f * j));
                    Draw(Taper.Flat, n, (0.036f - 0.008f * j) * thickness, primary, (1f - rp * 0.6f) * g * (1f - 0.3f * j));
                }
            }
        }

        void AnimDrag()
        {
            float g = FadeFrom(0.55f);
            float y = groundY + 0.02f;
            float dp = Mathf.Clamp01(_t / 0.3f);
            float headX = Mathf.Lerp(-0.6f, 1.4f, dp);

            if (_t < 0.3f)
            {
                int n = Seg(headX - 1.1f, y, headX, y, 10);
                Beam(Taper.Head, n, 0.1f, g);
                Emit(_dots, headX, y, 0f, 0f, 0.6f, 0.07f, Hue(primary));
                _acc += _dt * 140f;
                while (_acc >= 1f)
                {
                    _acc -= 1f;
                    Emit(_sparks, headX + (Random.value - 0.5f) * 0.2f, y, -0.5f + Random.value * 2.5f, 1f + 2.5f * Random.value, 0.05f, 0.4f, Hue(primary));
                }
            }
            else
            {
                float u = Mathf.Clamp01((_t - 0.3f) / 0.17f);
                Vector2 b0 = new Vector2(1.4f, groundY), b1 = new Vector2(1.9f, 0f), b2 = new Vector2(0.7f, 1.3f);
                float s0 = Mathf.Max(0f, u - 0.75f);
                int n = BezPts(b0, b1, b2, s0, u, 28);
                Beam(Taper.Head, n, 0.3f, g);
                if (u < 1f)
                {
                    Vector2 q = Bez(b0, b1, b2, u);
                    Emit(_dots, q.x, q.y, 0f, 0f, 0.7f, 0.07f, Hue(primary));
                    Trail(q.x, q.y, 90f);
                }

                float rg = 1f - Mathf.Clamp01((_t - 0.3f) / 0.3f);
                n = Seg(-0.6f, y, 1.4f, y, 2);
                Draw(Taper.Head, n, 0.05f * thickness, primary, rg * 0.7f);

                if (Once(1))
                {
                    Fire();
                    Burst(1.4f, groundY, 26, 5f, 8, 1.8f);
                }
            }
        }

        // ------------------------------------------------------------------ halberd

        void AnimHalberdCrescent()
        {
            float g = FadeFrom(0.25f);
            float p = EaseOut(_t / 0.2f);
            float head = 80f - 160f * p;
            float aStart = Mathf.Min(80f, head + 110f);
            int n = ArcPts(0.2f, 0f, 1.5f, 0.9f, aStart, head, 32);
            Beam(Taper.Head, n, 0.36f, g);
            Vector3 hp = _buf[n - 1];
            n = ArcPts(0.2f, 0f, 1.0f, 0.6f, aStart, head, 28);
            Draw(Taper.Head, n, 0.14f * thickness, primary, g * 0.7f);

            if (p < 1f)
            {
                Emit(_dots, hp.x, hp.y, 0f, 0f, 0.7f, 0.07f, Hue(primary));
                Trail(hp.x, hp.y, 90f);
            }

            if (p >= 0.5f) Fire();
            if (p >= 1f && Once(1)) Burst(hp.x, hp.y, 14, 4f, 6, 1.3f);
        }

        void AnimHalberdLift()
        {
            const float t1 = 0.18f;
            float g = FadeFrom(0.5f);
            float p = Mathf.Clamp01(_t / t1);
            float e = EaseOut(p);
            float tip = Mathf.Lerp(0.5f, 1.75f, e);
            float tg = 1f - Mathf.Clamp01((_t - t1) / 0.25f);

            if (tg > 0f)
            {
                int n = Seg(tip - 1.0f * e - 0.1f, 0f, tip, 0f, 10);
                Beam(Taper.Head, n, 0.2f, g * tg);
            }

            if (p >= 1f && Once(1))
            {
                Fire();
                Burst(tip + 0.05f, 0f, 14, 4f, 6, 1.3f);
            }

            float u = Mathf.Clamp01((_t - 0.22f) / 0.16f);
            if (u > 0f)
            {
                Vector2 b0 = new Vector2(1.75f, 0f), b1 = new Vector2(2.1f, 0.5f), b2 = new Vector2(1.3f, 1.35f);
                int n = BezPts(b0, b1, b2, Mathf.Max(0f, u - 0.75f), u, 26);
                Beam(Taper.Head, n, 0.3f, g);
                if (u < 1f)
                {
                    Vector2 q = Bez(b0, b1, b2, u);
                    Emit(_dots, q.x, q.y, 0f, 0f, 0.7f, 0.07f, Hue(primary));
                    Trail(q.x, q.y, 80f);
                }
                else if (Once(2))
                {
                    Burst(b2.x, b2.y, 16, 4f, 8, 1.4f);
                }
            }
        }

        // ------------------------------------------------------------------ whip / nunchaku

        void AnimWhip()
        {
            float g = FadeFrom(0.5f);
            float h = EaseOut(_t / 0.26f);
            float s0 = Mathf.Clamp01((_t - 0.3f) / 0.3f);
            float len = reach * 1.2f;
            const int n = 44;
            for (int i = 0; i < n; i++)
            {
                float u = Mathf.Lerp(s0 * h, h, i / (float)(n - 1));
                float x = 0.3f + u * len;
                float y = 0.55f * Mathf.Sin(u * 8.5f - _t * 22f) * Mathf.Pow(u, 0.7f) * (1f - 0.5f * u) + 0.5f * u * (1f - u);
                _buf[i] = new Vector3(x, y, 0f);
            }

            Beam(Taper.Head, n, 0.1f, g);
            Vector3 tip = _buf[n - 1];
            if (h < 1f)
            {
                Emit(_dots, tip.x, tip.y, 0f, 0f, 0.5f, 0.07f, Hue(primary));
                Trail(tip.x, tip.y, 60f);
            }

            if (_t >= 0.25f)
            {
                if (Once(1))
                {
                    Fire();
                    Burst(tip.x, tip.y, 18, 5f, 8, 1.6f);
                }

                float rp = Mathf.Clamp01((_t - 0.25f) / 0.3f);
                int rn = Ring(tip.x, tip.y, 0.1f + 0.35f * rp, 0.1f + 0.35f * rp);
                Draw(Taper.Flat, rn, 0.03f * thickness, primary, (1f - rp) * g);
            }
        }

        void AnimNunchaku()
        {
            float g = FadeFrom(0.6f);
            for (int k = 0; k < 4; k++)
            {
                float lt = _t - 0.14f * k;
                if (lt < 0f)
                {
                    continue;
                }

                float h = EaseOut(lt / 0.1f);
                float sgn = (k % 2 == 0) ? 1f : -1f;
                float jit = (Rnd(k) - 0.5f) * 0.3f;
                Vector2 b0 = new Vector2(0.5f, 0.8f * sgn + jit), b1 = new Vector2(1.5f, 0.9f * sgn), b2 = new Vector2(1.6f, -0.5f * sgn);
                int n = BezPts(b0, b1, b2, Mathf.Max(0f, h - 0.8f), h, 18);
                float lg = g * (1f - Mathf.Clamp01((lt - 0.1f) / 0.3f));
                Beam(Taper.Head, n, 0.16f, lg);

                if (h < 1f)
                {
                    Vector2 q = Bez(b0, b1, b2, h);
                    Emit(_dots, q.x, q.y, 0f, 0f, 0.5f, 0.07f, Hue(primary));
                }
                else if (Once(1 << k))
                {
                    if (onImpact != null) onImpact.Invoke();
                    Burst(b2.x, b2.y, 8, 3f, 4, 0.9f);
                }
            }
        }

        // ------------------------------------------------------------------ palm wave

        void AnimPalm()
        {
            float g = FadeFrom(0.35f);
            float p = EaseOut(_t / 0.45f);
            float travel = reach * 1.25f;
            if (_t >= 0.05f) Fire();

            for (int j = 0; j < 4; j++)
            {
                float d = Mathf.Clamp01(p - j * 0.1f);
                if (d <= 0f)
                {
                    continue;
                }

                float x = 0.45f + travel * d;
                int n = Ring(x, 0f, (0.1f + 0.05f * j) * (1f + d), (0.25f + 0.5f * d) * (1f + 0.15f * j));
                Draw(Taper.Flat, n, (0.04f - 0.006f * j) * thickness, primary, (1f - d * 0.7f) * g * (1f - 0.15f * j));
            }

            float ox = 0.45f + travel * p;
            if (p < 1f)
            {
                Emit(_dots, ox, 0f, 0f, 0f, 0.9f + 0.4f * (1f - p), 0.07f, Hue(primary));
                Trail(ox - 0.2f, 0f, 70f);
            }

            for (int j = 0; j < 6; j++)
            {
                float y = (j % 2 == 0 ? 1f : -1f) * (0.1f + 0.5f * Rnd(j));
                int n = Seg(ox - 0.4f - 0.8f * Rnd(j, 1), y, ox - 0.1f, y, 2);
                Draw(Taper.Head, n, 0.026f * thickness, primary, g * 0.5f * (1f - p * 0.5f));
            }

            if (p >= 1f && Once(1)) Burst(ox, 0f, 10, 3f, 5, 1.4f);
        }

        // ------------------------------------------------------------------ dart / arrow

        Vector2 DartPos(float q)
        {
            return new Vector2(Mathf.Lerp(0.3f, reach * 1.6f, q), 0.25f * Mathf.Sin(q * Mathf.PI));
        }

        void AnimDart()
        {
            const float flight = 0.4f;
            float p = Mathf.Clamp01(_t / flight);
            float g = FadeFrom(flight + 0.15f);
            Vector2 pos = DartPos(p);

            if (p < 1f)
            {
                const int tn = 14;
                for (int i = 0; i < tn; i++)
                {
                    float q = Mathf.Clamp01(p - (1f - i / (float)(tn - 1)) * 0.3f);
                    Vector2 w = DartPos(q);
                    _buf[i] = new Vector3(w.x, w.y, 0f);
                }

                Draw(Taper.Head, tn, 0.1f * thickness, primary, 1f);

                float ang = _t * 1500f;
                for (int k = 0; k < 2; k++)
                {
                    float a = (ang + 90f * k) * Mathf.Deg2Rad;
                    int n = Seg(pos.x - Mathf.Cos(a) * 0.18f, pos.y - Mathf.Sin(a) * 0.18f, pos.x + Mathf.Cos(a) * 0.18f, pos.y + Mathf.Sin(a) * 0.18f, 2);
                    Draw(Taper.Lens, n, 0.07f * thickness, _core, 1f);
                }

                Emit(_dots, pos.x, pos.y, 0f, 0f, 0.4f, 0.06f, Hue(primary));
                Trail(pos.x, pos.y, 60f);
            }
            else
            {
                if (Once(1))
                {
                    Fire();
                    Burst(pos.x, pos.y, 16, 4f, 8, 1.4f);
                }

                float rp = Mathf.Clamp01((_t - flight) / 0.3f);
                int rn = Ring(pos.x, pos.y, 0.1f + 0.4f * rp, 0.1f + 0.4f * rp);
                Draw(Taper.Flat, rn, 0.03f * thickness, primary, (1f - rp) * g);
            }
        }

        void AnimArrow()
        {
            const float chargeEnd = 0.35f;
            Vector2 o = new Vector2(0.4f, 0f);

            if (_t < chargeEnd)
            {
                float k = _t / chargeEnd;
                for (int j = 0; j < 2; j++)
                {
                    float kk = Mathf.Repeat(k * 1.5f + j * 0.5f, 1f);
                    float r = 0.5f * (1f - kk);
                    int rn = Ring(o.x, 0f, r, r);
                    Draw(Taper.Flat, rn, 0.02f * thickness, primary, kk);
                }

                Emit(_dots, o.x, 0f, 0f, 0f, 0.3f + 0.5f * k, 0.07f, Hue(primary));
                int n = Seg(o.x, 0f, o.x + 1f * k, 0f, 2);
                Draw(Taper.Head, n, 0.02f * thickness, primary, k * 0.7f);
                return;
            }

            float lt = _t - chargeEnd;
            float g = FadeFrom(chargeEnd + 0.35f);
            float far = reach * 1.8f;
            float p = EaseOut(lt / 0.1f);
            float tip = Mathf.Lerp(o.x, far, p);
            float fade = Mathf.Clamp01((lt - 0.1f) / 0.3f);
            float x0 = Mathf.Lerp(tip - 1.6f, tip - 0.05f, fade);
            int sn = Seg(Mathf.Max(o.x, x0), 0f, tip, 0f, 10);
            Beam(Taper.Head, sn, 0.12f, g);

            for (int j = 0; j < 3; j++)
            {
                float y = (j - 1) * 0.16f;
                sn = Seg(Mathf.Max(o.x, x0 - 0.3f), y, tip - 0.2f, y, 2);
                Draw(Taper.Head, sn, 0.02f * thickness, primary, g * 0.5f);
            }

            if (Once(1)) Burst(o.x, 0f, 6, 2f, 3, 0.9f);

            if (p >= 1f)
            {
                if (Once(2))
                {
                    Fire();
                    Burst(tip, 0f, 20, 5f, 10, 1.8f);
                }

                float bp = Mathf.Clamp01((lt - 0.1f) / 0.3f);
                int rn = Ring(tip, 0f, 0.1f + 0.3f * bp, 0.3f + 0.5f * bp);
                Draw(Taper.Flat, rn, 0.03f * thickness, primary, (1f - bp) * g);
            }
        }
    }
}
