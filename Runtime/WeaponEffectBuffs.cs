using UnityEngine;

namespace YinYangVFX
{
    // Buff / debuff applied effects (one-shot, ~1.3 s). Origin = the character (body centre = "sweepCenter").
    // Buffs rise upward and glow; debuffs sink and break. For a status that stays on the character, play these once
    // when the buff is applied and use a Status effect (or your own icon) for the duration.
    public partial class WeaponEffect
    {
        void Chevron(float x, float y, float w, float h, float dir, float width, float gain)
        {
            _buf[0] = new Vector3(x - w, y - dir * h, 0f);
            _buf[1] = new Vector3(x, y, 0f);
            _buf[2] = new Vector3(x + w, y - dir * h, 0f);
            Draw(Taper.Flat, 3, width * thickness, primary, gain);
        }

        void ChevronRight(float x, float y, float w, float h, float width, float gain)
        {
            _buf[0] = new Vector3(x - h, y + w, 0f);
            _buf[1] = new Vector3(x, y, 0f);
            _buf[2] = new Vector3(x - h, y - w, 0f);
            Draw(Taper.Flat, 3, width * thickness, primary, gain);
        }

        void RiseChevrons(float dir, float g)
        {
            Vector2 c = sweepCenter;
            for (int col = 0; col < 3; col++)
            {
                for (int k = 0; k < 2; k++)
                {
                    float f = Mathf.Repeat(_t * 0.9f + k * 0.5f + col * 0.17f, 1f);
                    float y = dir > 0f ? c.y - 0.5f + f * 1.3f : c.y + 0.8f - f * 1.3f;
                    Chevron(c.x + (col - 1) * 0.42f, y, 0.16f, 0.12f, dir, 0.05f, Mathf.Sin(f * Mathf.PI) * g);
                }
            }
        }

        void Shield(float scale, float gain, float width)
        {
            Vector2 c = sweepCenter;
            _buf[0] = new Vector3(c.x - 0.4f * scale, c.y + 0.5f * scale, 0f);
            _buf[1] = new Vector3(c.x, c.y + 0.62f * scale, 0f);
            _buf[2] = new Vector3(c.x + 0.4f * scale, c.y + 0.5f * scale, 0f);
            _buf[3] = new Vector3(c.x + 0.4f * scale, c.y + 0.05f * scale, 0f);
            _buf[4] = new Vector3(c.x, c.y - 0.35f * scale, 0f);
            _buf[5] = new Vector3(c.x - 0.4f * scale, c.y + 0.05f * scale, 0f);
            _buf[6] = _buf[0];
            Draw(Taper.Flat, 7, width * thickness, primary * 0.5f, gain * 0.5f);
            Draw(Taper.Flat, 7, width * 0.4f * thickness, primary, gain);
        }

        void SwordIcon(float cx, float cy, float len, float gain, float tilt)
        {
            float a = (90f + tilt) * Mathf.Deg2Rad;
            float dx = Mathf.Cos(a), dy = Mathf.Sin(a);
            int n = Seg(cx - dx * len * 0.4f, cy - dy * len * 0.4f, cx + dx * len * 0.6f, cy + dy * len * 0.6f, 2);
            Draw(Taper.Lens, n, 0.12f * thickness, primary, gain);
            Draw(Taper.Lens, n, 0.045f * thickness, _core, gain);
            n = Seg(cx - dy * 0.18f * len, cy + dx * 0.18f * len, cx + dy * 0.18f * len, cy - dx * 0.18f * len, 2);
            Draw(Taper.Lens, n, 0.07f * thickness, primary, gain);
        }

        void BuffStart(float burst)
        {
            if (Once(1))
            {
                Fire();
                Vector2 c = sweepCenter;
                Burst(c.x, c.y, 16, 4f, 10, burst);
            }
        }

        void GroundRing(float e, float g, bool expand)
        {
            Vector2 c = sweepCenter;
            float r = expand ? 0.2f + 0.9f * e : 1.1f - 0.9f * e;
            int n = Ring(c.x, groundY, r, r * 0.2f);
            Draw(Taper.Flat, n, 0.035f * thickness, primary, (expand ? 1f - e * 0.5f : 0.5f + 0.5f * e) * g);
        }

        void AnimBuffAttackUp()
        {
            Vector2 c = sweepCenter;
            float e = EaseOut(_t / 0.3f), g = FadeFrom(0.8f);
            BuffStart(1.6f);
            SwordIcon(c.x, c.y + 0.1f, 1.5f * e, g, 0f);
            RiseChevrons(1f, g);
            GroundRing(e, g, true);
            if (_t < 0.8f && Random.value < 0.6f) Emit(_sparks, c.x + (Random.value - 0.5f) * 0.8f, c.y - 0.5f, 0f, 1f + Random.value, 0.05f, 0.7f, Hue(primary));
        }

        void AnimBuffSpeedUp()
        {
            Vector2 c = sweepCenter;
            float e = EaseOut(_t / 0.3f), g = FadeFrom(0.8f);
            BuffStart(1.4f);
            for (int k = 0; k < 3; k++)
            {
                float f = Mathf.Repeat(_t * 1.1f + k / 3f, 1f);
                ChevronRight(c.x - 0.6f + f * 1.4f, c.y + (k - 1) * 0.3f, 0.16f, 0.14f, 0.05f, Mathf.Sin(f * Mathf.PI) * g);
            }

            for (int j = 0; j < 7; j++)
            {
                float y = c.y - 0.5f + 1.1f * Rnd(j);
                float f = Mathf.Repeat(_t * 1.6f + Rnd(j, 1), 1f);
                float x = c.x + 0.6f - f * 1.8f;
                int n = Seg(x - 0.5f, y, x, y, 2);
                Draw(Taper.Head, n, 0.03f * thickness, primary, g * Mathf.Sin(f * Mathf.PI) * 0.9f);
            }

            GroundRing(e, g, true);
        }

        void AnimBuffDefenseUp()
        {
            float e = EaseOut(_t / 0.3f), g = FadeFrom(0.8f);
            BuffStart(1.6f);
            Shield(0.6f + 0.4f * e, g, 0.12f);
            Shield(1.0f + 0.5f * e, g * (1f - e) * 0.8f, 0.06f);
            RiseChevrons(1f, g * 0.7f);
            GroundRing(e, g, true);
        }

        void AnimBuffCritUp()
        {
            Vector2 c = sweepCenter;
            float e = EaseOut(_t / 0.3f), g = FadeFrom(0.8f);
            BuffStart(1.8f);
            float r = 0.35f + 0.1f * Mathf.Sin(_t * 8f);
            int n = Ring(c.x, c.y + 0.1f, r, r);
            Draw(Taper.Flat, n, 0.04f * thickness, primary, g);
            for (int k = 0; k < 4; k++)
            {
                float a = (k * 90f + _t * 30f) * Mathf.Deg2Rad;
                n = Seg(c.x + Mathf.Cos(a) * (r - 0.08f), c.y + 0.1f + Mathf.Sin(a) * (r - 0.08f), c.x + Mathf.Cos(a) * (r + 0.2f), c.y + 0.1f + Mathf.Sin(a) * (r + 0.2f), 2);
                Draw(Taper.Lens, n, 0.05f * thickness, primary, g);
            }

            float rot = _t * 40f * Mathf.Deg2Rad;
            for (int k = 0; k < 2; k++)
            {
                float a = rot + k * Mathf.PI * 0.5f;
                float L = 0.9f * e;
                n = Seg(c.x - Mathf.Cos(a) * L, c.y + 0.1f - Mathf.Sin(a) * L, c.x + Mathf.Cos(a) * L, c.y + 0.1f + Mathf.Sin(a) * L, 2);
                Draw(Taper.Lens, n, 0.1f * thickness, primary, g * 0.9f);
                Draw(Taper.Lens, n, 0.035f * thickness, _core, g);
            }

            RiseChevrons(1f, g * 0.6f);
            GroundRing(e, g, true);
            if (_t < 0.8f && Random.value < 0.5f) Emit(_stars, c.x + (Random.value - 0.5f) * 1.0f, c.y + (Random.value - 0.3f) * 1.0f, 0f, 0.4f, 0.12f + 0.1f * Random.value, 0.6f, Hue(primary));
        }

        void AnimDebuffAttackDown()
        {
            Vector2 c = sweepCenter;
            float e = EaseOut(_t / 0.3f), g = FadeFrom(0.8f);
            BuffStart(1.2f);
            float fall = Mathf.Clamp01((_t - 0.25f) / 0.6f);
            SwordIcon(c.x - 0.05f * fall, c.y + 0.35f - 0.5f * fall * fall, 0.8f * e, g, 18f * fall);
            SwordIcon(c.x + 0.05f * fall, c.y - 0.05f - 0.6f * fall * fall, 0.7f * e, g * 0.8f, -22f * fall);
            RiseChevrons(-1f, g);
            GroundRing(e, g, false);
        }

        void AnimDebuffDefenseDown()
        {
            Vector2 c = sweepCenter;
            float e = EaseOut(_t / 0.3f), g = FadeFrom(0.8f);
            BuffStart(1.2f);
            float fall = Mathf.Clamp01((_t - 0.25f) / 0.6f);
            Shield(1f - 0.15f * fall, g, 0.12f);
            for (int j = 0; j < 3; j++)
            {
                float a = (60f + j * 70f) * Mathf.Deg2Rad;
                int n = Seg(c.x, c.y + 0.1f, c.x + Mathf.Cos(a) * 0.4f * e, c.y + 0.1f + Mathf.Sin(a) * 0.4f * e, 2);
                Draw(Taper.Tail, n, 0.04f * thickness, _core, g);
            }

            if (Once(2))
            {
                for (int j = 0; j < 10; j++)
                {
                    Emit(_sparks, c.x + (Random.value - 0.5f) * 0.6f, c.y + (Random.value - 0.3f) * 0.8f, (Random.value - 0.5f) * 1.2f, -0.5f - Random.value, 0.09f, 0.8f, Hue(primary));
                }
            }

            RiseChevrons(-1f, g);
            GroundRing(e, g, false);
        }
    }
}
