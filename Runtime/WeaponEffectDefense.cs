using UnityEngine;

namespace YinYangVFX
{
    // Defence effects: shields, blocks, reflects, dodges, buffs, shield break, stun.
    // Origin conventions (side-scroller, character facing right, character about 1 unit tall):
    //   body effects (shields, iron body, invincible glow, heal, reflect, stun, dodge, roll, blink, shield break):
    //       the position is the character; "sweepCenter" (default 0,-0.2) is the body centre, "groundY" the ground.
    //   hit-point effects (block sparks, perfect block, shield ripple, projectile return): the position is the point
    //       where the hit lands (e.g. the front of the shield).
    // Looping effects (loop = true): spawn once, set .follow to the character, call Pulse() on every hit, Stop() to end.
    public partial class WeaponEffect
    {
        Color Tint(float alpha)
        {
            Color h = Hue(primary);
            h.a = alpha;
            return h;
        }

        bool Live()
        {
            return _outT < 0f;
        }

        // ------------------------------------------------------------------ shields (looping)

        void AnimShieldOrb()
        {
            Vector2 c = sweepCenter;
            float R = 0.85f * (1f + 0.025f * Mathf.Sin(_t * 6f));

            int n = Ring(c.x, c.y, R, R);
            Draw(Taper.Flat, n, 0.05f * thickness, primary, 1f);
            Draw(Taper.Flat, n, 0.02f * thickness, _core, 0.9f);
            n = Ring(c.x, c.y, R * 0.93f, R * 0.93f);
            Draw(Taper.Flat, n, 0.1f * thickness, primary * 0.4f, 0.35f);

            for (int j = 0; j < 3; j++)
            {
                float lat = (j - 1) * 0.5f;
                float rr = R * Mathf.Sqrt(1f - lat * lat);
                n = Ring(c.x, c.y + lat * R, rr, rr * 0.18f);
                Draw(Taper.Flat, n, 0.012f * thickness, primary, 0.45f);
            }

            for (int j = 0; j < 3; j++)
            {
                float a = _t * 1.2f + j * Mathf.PI / 3f;
                float k = Mathf.Abs(Mathf.Cos(a));
                n = Ring(c.x, c.y, Mathf.Max(0.02f, R * k), R);
                Draw(Taper.Flat, n, 0.012f * thickness, primary, 0.15f + 0.3f * k);
            }

            if (Live())
            {
                Emit(_dots, c.x, c.y, 0f, 0f, R * 2.3f, 0.07f, Tint(0.18f + 0.2f * _pulse));
                _acc += _dt * 8f;
                while (_acc >= 1f)
                {
                    _acc -= 1f;
                    float a = Random.value * 6.2832f;
                    Emit(_stars, c.x + Mathf.Cos(a) * R, c.y + Mathf.Sin(a) * R, -Mathf.Sin(a) * 0.3f, Mathf.Cos(a) * 0.3f, 0.12f, 0.5f, Hue(primary));
                }
            }
        }

        void AnimShieldArc()
        {
            Vector2 c = sweepCenter;
            float cx = c.x + 0.15f - 0.35f;
            int n = ArcPts(cx, c.y, 0.65f, 0.95f, -62f, 62f, 28);
            Draw(Taper.Lens, n, 0.22f * thickness, primary * 0.4f, 0.5f);
            Draw(Taper.Lens, n, 0.1f * thickness, primary, 1f);
            Draw(Taper.Lens, n, 0.04f * thickness, _core, 1f);
            for (int j = 1; j <= 2; j++)
            {
                n = ArcPts(cx - 0.07f * j, c.y, 0.65f, 0.95f, -56f, 56f, 24);
                Draw(Taper.Lens, n, 0.03f * thickness, primary, 0.5f / j);
            }

            float s = Mathf.Repeat(_t * 0.9f, 1f);
            float ang = Mathf.Lerp(-55f, 55f, s) * Mathf.Deg2Rad;
            float bx = cx + 0.65f * Mathf.Cos(ang);
            float by = c.y + 0.95f * Mathf.Sin(ang);
            n = Seg(bx - 0.3f, by, bx + 0.02f, by, 2);
            Draw(Taper.Head, n, 0.06f * thickness, _core, 0.9f);

            if (Live())
            {
                Emit(_dots, cx + 0.45f, c.y, 0f, 0f, 1.8f, 0.07f, Tint(0.12f + 0.2f * _pulse));
                _acc += _dt * 6f;
                while (_acc >= 1f)
                {
                    _acc -= 1f;
                    float a = Mathf.Lerp(-60f, 60f, Random.value) * Mathf.Deg2Rad;
                    Emit(_stars, cx + 0.65f * Mathf.Cos(a), c.y + 0.95f * Mathf.Sin(a), 0.1f, 0.2f, 0.12f, 0.5f, Hue(primary));
                }
            }
        }

        void AnimYinYangShield()
        {
            Vector2 c = sweepCenter;
            float R = 0.95f;
            float ph = _t * 60f * Mathf.Deg2Rad;
            float cs = Mathf.Cos(ph), sn = Mathf.Sin(ph);

            int n = Ring(c.x, c.y, R, R);
            Draw(Taper.Flat, n, 0.04f * thickness, primary, 1f);
            Draw(Taper.Flat, n, 0.012f * thickness, _core, 0.9f);

            const int m = 20;
            for (int i = 0; i < m; i++)
            {
                float a = Mathf.Lerp(90f, -90f, i / (float)(m - 1)) * Mathf.Deg2Rad;
                float x = R * 0.5f * Mathf.Cos(a);
                float y = R * 0.5f + R * 0.5f * Mathf.Sin(a);
                _buf[i] = new Vector3(c.x + x * cs - y * sn, c.y + x * sn + y * cs, 0f);
            }

            for (int i = 0; i < m; i++)
            {
                float a = Mathf.Lerp(90f, 270f, i / (float)(m - 1)) * Mathf.Deg2Rad;
                float x = R * 0.5f * Mathf.Cos(a);
                float y = -R * 0.5f + R * 0.5f * Mathf.Sin(a);
                _buf[m + i] = new Vector3(c.x + x * cs - y * sn, c.y + x * sn + y * cs, 0f);
            }

            Draw(Taper.Flat, 2 * m, 0.05f * thickness, primary, 0.9f);

            for (int e = 0; e < 2; e++)
            {
                float y = (e == 0 ? 1f : -1f) * R * 0.5f;
                n = Ring(c.x - y * sn, c.y + y * cs, R * 0.14f, R * 0.14f);
                Draw(Taper.Flat, n, 0.03f * thickness, primary, 0.9f);
            }

            n = Ring(c.x, c.y, R * 1.12f, R * 1.12f);
            Draw(Taper.Flat, n, 0.012f * thickness, primary, 0.5f);
            for (int k = 0; k < 8; k++)
            {
                float a = -ph * 0.7f + k * 45f * Mathf.Deg2Rad;
                n = Seg(c.x + Mathf.Cos(a) * R * 1.05f, c.y + Mathf.Sin(a) * R * 1.05f, c.x + Mathf.Cos(a) * R * 1.2f, c.y + Mathf.Sin(a) * R * 1.2f, 2);
                Draw(Taper.Flat, n, 0.02f * thickness, primary, 0.7f);
            }

            if (Live())
            {
                Emit(_dots, c.x, c.y, 0f, 0f, R * 2.2f, 0.07f, Tint(0.14f + 0.2f * _pulse));
                _acc += _dt * 8f;
                while (_acc >= 1f)
                {
                    _acc -= 1f;
                    float a = Random.value * 6.2832f;
                    Emit(_stars, c.x + Mathf.Cos(a) * R, c.y + Mathf.Sin(a) * R, 0f, 0.2f, 0.12f, 0.5f, Hue(primary));
                }
            }
        }

        // ------------------------------------------------------------------ block / parry (hit point = origin)

        void AnimBlockSparks()
        {
            float g = FadeFrom(0.12f);
            float e = EaseOut(_t / 0.12f);
            Fire();
            if (Once(1)) Burst(0f, 0f, 26, 7f, 10, 2.4f);

            float len = Mathf.Lerp(0.2f, 0.9f, e);
            for (int k = 0; k < 2; k++)
            {
                float a = (k == 0 ? 45f : -45f) * Mathf.Deg2Rad;
                int n = Seg(-Mathf.Cos(a) * len, -Mathf.Sin(a) * len, Mathf.Cos(a) * len, Mathf.Sin(a) * len, 2);
                Draw(Taper.Lens, n, 0.12f * thickness, primary, g);
                Draw(Taper.Lens, n, 0.05f * thickness, _core, g);
            }

            int h = Seg(-len * 1.3f, 0f, len * 1.3f, 0f, 2);
            Draw(Taper.Lens, h, 0.05f * thickness, primary, g * 0.8f);
            h = Seg(0f, -len * 0.8f, 0f, len * 0.8f, 2);
            Draw(Taper.Lens, h, 0.04f * thickness, primary, g * 0.6f);

            int r = Ring(0f, 0f, 0.1f + 0.6f * e, 0.1f + 0.6f * e);
            Draw(Taper.Flat, r, 0.04f * (1f - e * 0.5f) * thickness, primary, g);
        }

        void AnimPerfectBlock()
        {
            float g = FadeFrom(0.25f);
            float e = EaseOut(_t / 0.35f);
            Fire();
            if (Once(1)) Burst(0f, 0f, 30, 8f, 16, 3.2f);

            for (int j = 0; j < 3; j++)
            {
                float d = Mathf.Clamp01(e - 0.12f * j);
                float r = 1.6f * d;
                int n = Ring(0f, 0f, r, r);
                Draw(Taper.Flat, n, (0.06f - 0.014f * j) * thickness, primary, (1f - d * 0.6f) * g);
            }

            float len = Mathf.Lerp(0.3f, 2.2f, e);
            int f = Seg(-len, 0f, len, 0f, 2);
            Draw(Taper.Lens, f, 0.1f * thickness, primary, g);
            Draw(Taper.Lens, f, 0.04f * thickness, _core, g);
            f = Seg(0f, -len * 0.5f, 0f, len * 0.5f, 2);
            Draw(Taper.Lens, f, 0.07f * thickness, primary, g * 0.8f);

            for (int k = 0; k < 12; k++)
            {
                float a = k * 30f * Mathf.Deg2Rad;
                float r0 = 0.3f + 1.2f * e;
                int n = Seg(Mathf.Cos(a) * r0, Mathf.Sin(a) * r0, Mathf.Cos(a) * (r0 + 0.3f), Mathf.Sin(a) * (r0 + 0.3f), 2);
                Draw(Taper.Head, n, 0.03f * thickness, primary, g * 0.7f);
            }
        }

        void AnimShieldRipple()
        {
            float g = FadeFrom(0.4f);
            Fire();
            if (Once(1))
            {
                Emit(_dots, 0f, 0f, 0f, 0f, 1.4f, 0.25f, Hue(primary));
                Burst(0f, 0f, 10, 3f, 4, 0.9f);
            }

            int n = ArcPts(-0.9f, 0f, 0.95f, 0.95f, -50f, 50f, 28);
            Draw(Taper.Lens, n, 0.07f * thickness, primary, g * 0.8f);

            for (int j = 0; j < 4; j++)
            {
                float d = Mathf.Clamp01((_t - 0.08f * j) / 0.5f);
                if (d <= 0f)
                {
                    continue;
                }

                float r = 0.1f + 0.9f * EaseOut(d);
                n = Ring(0f, 0f, r * 0.45f, r);
                Draw(Taper.Flat, n, 0.035f * thickness, primary, (1f - d) * g);
            }
        }

        // ------------------------------------------------------------------ reflect

        void AnimReflectWave()
        {
            Vector2 c = sweepCenter;
            float g = FadeFrom(0.45f);
            Fire();
            if (Once(1))
            {
                Burst(c.x, c.y, 18, 6f, 8, 1.8f);
                for (int j = 0; j < 20; j++)
                {
                    float a = j * 18f * Mathf.Deg2Rad;
                    Emit(_sparks, c.x, c.y, Mathf.Cos(a) * 9f, Mathf.Sin(a) * 9f, 0.06f, 0.25f, Hue(primary));
                }
            }

            for (int j = 0; j < 3; j++)
            {
                float d = Mathf.Clamp01((_t - 0.06f * j) / 0.55f);
                float r = 2.4f * EaseOut(d) * (1f - 0.25f * j);
                int n = Ring(c.x, c.y, r, r);
                Draw(Taper.Flat, n, (0.09f - 0.025f * j) * thickness * (1f - d * 0.5f), primary, (1f - d * 0.7f) * g * (1f - 0.2f * j));
                if (j == 0)
                {
                    Draw(Taper.Flat, n, 0.03f * thickness, _core, (1f - d * 0.7f) * g);
                }
            }

            float e = EaseOut(_t / 0.55f);
            int gn = Ring(c.x, groundY, 3.2f * e, 0.4f * e);
            Draw(Taper.Flat, gn, 0.03f * thickness, primary, (1f - e * 0.6f) * g * 0.7f);
        }

        void AnimProjectileReturn()
        {
            const float inT = 0.14f;
            float g = FadeFrom(inT + 0.4f);
            if (_t < inT)
            {
                float x = Mathf.Lerp(1.9f, 0f, _t / inT);
                int n = Seg(x + 0.9f, 0f, x, 0f, 8);
                Beam(Taper.Head, n, 0.09f, 1f);
                Emit(_dots, x, 0f, 0f, 0f, 0.35f, 0.06f, Hue(primary));
                return;
            }

            float rt = _t - inT;
            if (Once(1))
            {
                Fire();
                Burst(0f, 0f, 22, 6f, 8, 2f);
            }

            float p = EaseOut(rt / 0.35f);
            float bx = Mathf.Lerp(0f, 2.8f, p);
            int tn = Seg(Mathf.Max(0f, bx - 1.4f), 0f, bx, 0f, 8);
            Beam(Taper.Head, tn, 0.14f, g);
            int rn = Ring(bx, 0f, 0.1f, 0.22f);
            Draw(Taper.Flat, rn, 0.03f * thickness, primary, g);
            if (p < 1f)
            {
                Emit(_dots, bx, 0f, 0f, 0f, 0.5f, 0.06f, Hue(primary));
                Trail(bx, 0f, 60f);
            }

            float e = EaseOut(rt / 0.4f);
            rn = Ring(0f, 0f, 0.1f + 0.5f * e, 0.1f + 0.5f * e);
            Draw(Taper.Flat, rn, 0.035f * thickness, primary, (1f - e) * g);
            int an = ArcPts(-0.5f, 0f, 0.55f, 0.8f, -45f, 45f, 20);
            Draw(Taper.Lens, an, 0.08f * thickness, primary, Mathf.Clamp01(1f - rt * 3f));
        }

        // ------------------------------------------------------------------ dodge

        void AnimDodge()
        {
            Vector2 c = sweepCenter;
            float g = FadeFrom(0.25f);
            float D = 1.8f;
            if (Once(1))
            {
                Fire();
                Emit(_dots, 0f, groundY + 0.1f, 0f, 0f, 1.0f, 0.4f, Tint(0.4f));
            }

            float lead = D * EaseOut(Mathf.Clamp01(_t / 0.18f));
            for (int k = 0; k < 5; k++)
            {
                float gx = D * EaseOut(Mathf.Clamp01((_t - 0.03f * k) / 0.18f));
                int n = Seg(gx, groundY + 0.05f, gx, c.y + 0.45f, 2);
                Draw(Taper.Lens, n, 0.3f * thickness, primary, g * (0.5f - 0.09f * k));
            }

            for (int j = 0; j < 8; j++)
            {
                float y = groundY + 0.1f + (c.y + 0.4f - groundY) * Rnd(j);
                float len = 0.5f + 0.9f * Rnd(j, 1);
                int n = Seg(lead - len, y, lead, y, 2);
                Draw(Taper.Head, n, 0.025f * thickness, primary, g * 0.7f);
            }

            if (_t < 0.2f)
            {
                Trail(lead, groundY + 0.1f, 80f);
            }
        }

        void AnimRoll()
        {
            Vector2 c = sweepCenter;
            float g = FadeFrom(0.5f);
            float D = 1.6f;
            float p = Mathf.Clamp01(_t / 0.5f);
            float x = D * p;
            if (Once(1)) Fire();

            float a1 = -90f + 540f * p;
            int n = ArcPts(x, c.y - 0.15f, 0.45f, 0.45f, a1 - 200f, a1, 24);
            Draw(Taper.Head, n, 0.1f * thickness, primary, g);
            Draw(Taper.Head, n, 0.04f * thickness, _core, g);

            int rn = Ring(x, groundY, 0.35f, 0.07f);
            Draw(Taper.Flat, rn, 0.02f * thickness, primary, g * 0.7f);

            if (p < 1f)
            {
                _acc += _dt * 40f;
                while (_acc >= 1f)
                {
                    _acc -= 1f;
                    Emit(_dots, x - Random.value * 0.4f, groundY + 0.05f, -0.4f * Random.value, 0.3f + 0.3f * Random.value, 0.6f, 0.6f, Tint(0.35f));
                }
            }
        }

        void AnimBlink()
        {
            Vector2 c = sweepCenter;
            float g = FadeFrom(0.4f);
            float D = reach * 1.4f;
            float a = Mathf.Clamp01(_t / 0.2f);

            if (Once(1))
            {
                Fire();
                Burst(0f, c.y, 18, 5f, 8, 1.6f);
            }

            if (a < 1f)
            {
                int n = Seg(0f, groundY, 0f, groundY + 1.5f, 2);
                Draw(Taper.Lens, n, 0.55f * (1f - a) * thickness, primary, 1f - a * 0.5f);
                int rn = Ring(0f, c.y, 0.7f * (1f - a), 0.7f * (1f - a));
                Draw(Taper.Flat, rn, 0.03f * thickness, primary, 1f - a);
            }

            float tp = Mathf.Clamp01((_t - 0.05f) / 0.12f);
            int tn = Seg(0f, c.y, D * tp, c.y, 10);
            Beam(Taper.Head, tn, 0.12f, g * (1f - Mathf.Clamp01((_t - 0.2f) / 0.3f)));

            float b = Mathf.Clamp01((_t - 0.17f) / 0.25f);
            if (_t >= 0.17f)
            {
                if (Once(2)) Burst(D, c.y, 18, 5f, 8, 1.6f);
                int cn = Seg(D, groundY, D, groundY + 1.5f, 2);
                Draw(Taper.Lens, cn, 0.55f * Mathf.Sin(b * Mathf.PI) * thickness, primary, g);
                int rn = Ring(D, c.y, 0.1f + 0.8f * EaseOut(b), 0.1f + 0.8f * EaseOut(b));
                Draw(Taper.Flat, rn, 0.03f * thickness, primary, (1f - b) * g);
            }
        }

        // ------------------------------------------------------------------ buffs

        void BodyOutline(float rx, float ry, float flick, float shimmerSpeed)
        {
            Vector2 c = sweepCenter;
            for (int j = 0; j < 3; j++)
            {
                int n = Ring(c.x, c.y, rx * (1f + 0.07f * j), ry * (1f + 0.07f * j));
                Draw(Taper.Flat, n, (j == 0 ? 0.04f : 0.1f) * thickness, j == 0 ? primary : primary * 0.4f, (j == 0 ? 1f : 0.3f) * flick);
            }

            float h = _t * shimmerSpeed;
            int sn = ArcPts(c.x, c.y, rx, ry, h - 50f, h, 16);
            Draw(Taper.Head, sn, 0.07f * thickness, _core, flick);

            if (Live())
            {
                Emit(_dots, c.x, c.y, 0f, 0f, 1.5f, 0.07f, Tint(0.12f + 0.2f * _pulse));
                _acc += _dt * 10f;
                while (_acc >= 1f)
                {
                    _acc -= 1f;
                    float a = Random.value * 6.2832f;
                    Emit(_stars, c.x + Mathf.Cos(a) * rx, c.y + Mathf.Sin(a) * ry * 0.8f, 0f, 0.5f + 0.6f * Random.value, 0.1f, 0.6f, Hue(primary));
                }
            }
        }

        void AnimIronBody()
        {
            BodyOutline(0.34f, 0.56f, 0.85f + 0.15f * Mathf.Sin(_t * 3f), 200f);
        }

        void AnimInvincible()
        {
            float f = Mathf.Sin(_t * 50f) > 0f ? 1f : 0.45f;
            BodyOutline(0.42f, 0.64f, f, 360f);
        }

        void AnimHealPillar()
        {
            Vector2 c = sweepCenter;
            float g = FadeFrom(0.9f);
            float e = EaseOut(_t / 0.3f);
            if (Once(1))
            {
                Fire();
                Emit(_dots, c.x, groundY + 0.1f, 0f, 0f, 1.6f, 0.4f, Hue(primary));
            }

            for (int j = 0; j < 2; j++)
            {
                float d = Mathf.Clamp01((_t - 0.15f * j) / 0.6f);
                float ed = EaseOut(d);
                int n = Ring(c.x, groundY, 0.2f + 0.9f * ed, 0.05f + 0.2f * ed);
                Draw(Taper.Flat, n, 0.03f * thickness, primary, (1f - d) * g);
            }

            float w = Mathf.Lerp(0.15f, 0.9f, e) * (0.9f + 0.1f * Mathf.Sin(_t * 14f));
            int cn = Seg(c.x, groundY + 2.4f, c.x, groundY, 2);
            Draw(Taper.Head, cn, w * thickness, primary * 0.6f, g * 0.7f);
            Draw(Taper.Head, cn, w * 0.35f * thickness, _core, g);

            if (_t < 0.9f)
            {
                _acc += _dt * 60f;
                while (_acc >= 1f)
                {
                    _acc -= 1f;
                    Emit(Random.value > 0.5f ? _stars : _dots, c.x + (Random.value - 0.5f) * 0.8f, groundY + Random.value * 0.2f, 0f, 1f + 1.5f * Random.value,
                         0.1f + 0.1f * Random.value, 0.8f, Hue(primary));
                }
            }
        }

        // ------------------------------------------------------------------ shield break / stun

        void AnimShieldBreak()
        {
            Vector2 c = sweepCenter;
            const float R = 0.85f;
            const float breakT = 0.14f;

            if (_t < breakT)
            {
                float k = _t / breakT;
                int n = Ring(c.x, c.y, R, R);
                Draw(Taper.Flat, n, 0.05f * thickness, primary, 1f);
                for (int j = 0; j < 6; j++)
                {
                    float a = (j * 60f + Rnd(j) * 20f) * Mathf.Deg2Rad;
                    float r1 = R * (1f - 0.75f * k);
                    n = Seg(c.x + R * Mathf.Cos(a), c.y + R * Mathf.Sin(a), c.x + r1 * Mathf.Cos(a), c.y + r1 * Mathf.Sin(a), 2);
                    Draw(Taper.Tail, n, 0.03f * thickness, _core, 1f);
                }

                return;
            }

            float it = _t - breakT;
            float g = FadeFrom(0.5f);
            if (Once(1))
            {
                Fire();
                Burst(c.x, c.y, 50, 5f, 20, 3f);
                for (int j = 0; j < 24; j++)
                {
                    float a = Random.value * 6.2832f;
                    float sp = 2f + 4f * Random.value;
                    Emit(_sparks, c.x + Mathf.Cos(a) * R, c.y + Mathf.Sin(a) * R, Mathf.Cos(a) * sp, Mathf.Sin(a) * sp, 0.1f + 0.08f * Random.value, 0.7f, Hue(primary));
                }
            }

            float rp = EaseOut(it / 0.5f);
            int rn = Ring(c.x, c.y, R * (1f + 0.8f * rp), R * (1f + 0.8f * rp));
            Draw(Taper.Flat, rn, 0.04f * thickness, primary, (1f - rp) * g);
            rn = Ring(c.x, c.y, R * (1f + 0.4f * rp), R * (1f + 0.4f * rp));
            Draw(Taper.Flat, rn, 0.02f * thickness, _core, (1f - rp) * g * 0.6f);
        }

        void AnimStun()
        {
            Vector2 c = sweepCenter;
            Vector2 hc = new Vector2(c.x, c.y + 0.65f);
            int n = Ring(hc.x, hc.y, 0.38f, 0.1f);
            Draw(Taper.Flat, n, 0.015f * thickness, primary, 0.5f);

            if (Live())
            {
                for (int k = 0; k < 4; k++)
                {
                    float a = _t * 4f + k * Mathf.PI * 0.5f;
                    float depth = (Mathf.Sin(a) + 1f) * 0.5f;
                    Emit(_stars, hc.x + 0.38f * Mathf.Cos(a), hc.y + 0.1f * Mathf.Sin(a), 0f, 0f, 0.14f + 0.1f * depth, 0.05f, Hue(primary));
                }
            }
        }
    }
}
