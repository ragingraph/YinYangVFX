#if UNITY_EDITOR
using UnityEngine;

namespace YinYangVFX.EditorTools
{
    /// <summary>Procedural pixel art for the shape atlas (VFX_Shapes.png). RGB = brightness (tinted per prefab), alpha = silhouette.</summary>
    internal static class ShapePixels
    {
        static void Put(ref float A, ref float B, float a, float b)
        {
            if (a > A)
            {
                A = a;
                B = b;
            }
        }

        static float H(int a, int b)
        {
            unchecked
            {
                uint h = (uint)(a * 374761393 + b * 668265263);
                h = (h ^ (h >> 13)) * 1274126177u;
                return ((h ^ (h >> 16)) & 0xFFFFFF) / (float)0x1000000;
            }
        }

        static Color32 Out(float a, float b)
        {
            byte c = (byte)Mathf.RoundToInt(Mathf.Clamp01(b) * 255f);
            return new Color32(c, c, c, (byte)Mathf.RoundToInt(Mathf.Clamp01(a) * 255f));
        }

        public static Color32 Atlas(int x, int y, int W, int Hh)
        {
            int[][] rects = ShapeAtlas.Rects;
            for (int i = 0; i < rects.Length; i++)
            {
                int[] r = rects[i];
                if (x >= r[0] && x < r[0] + r[2] && y >= r[1] && y < r[1] + r[3])
                {
                    return Pixel(i, x - r[0], y - r[1], r[2], r[3]);
                }
            }

            return new Color32(0, 0, 0, 0);
        }

        static Color32 Pixel(int shape, int x, int y, int w, int h)
        {
            switch ((Shape)shape)
            {
                case Shape.Sword: return WeaponVFXBuilder.InkSwordPixel(Mathf.Min(191, x * 192 / w), Mathf.Min(767, y * 768 / h), 192, 768);
                case Shape.Spear: return Spear(x, y, w, h);
                case Shape.Hammer: return Hammer(x, y, w, h);
                case Shape.Halberd: return Halberd(x, y, w, h);
                case Shape.Arrow: return Arrow(x, y, w, h);
                case Shape.Kunai: return Kunai(x, y, w, h);
                case Shape.Stick: return Stick(x, y, w, h);
                case Shape.Brush: return Brush(x, y, w, h);
            }

            return new Color32(0, 0, 0, 0);
        }

        static float Side(float px, float hw, float light, float dark)
        {
            return Mathf.Lerp(light, dark, Mathf.Clamp01((px + hw) / (2f * hw)));
        }

        static Color32 Spear(int x, int y, int w, int h)
        {
            float px = x + 0.5f - w * 0.5f, ax = Mathf.Abs(px), s = 1f - (y + 0.5f) / h;
            float A = 0f, B = 0f;
            if (s < 0.17f)
            {
                float t = s / 0.17f;
                float hw = 17f * Mathf.Sin(Mathf.Pow(t, 0.6f) * Mathf.PI) + (H(y / 3, 5) - 0.5f) * 1.2f * Mathf.Clamp01(t * 8f);
                if (ax < hw + 1f)
                {
                    float b = Side(px, hw, 1f, 0.4f);
                    if (ax < 1.5f || ax > hw - 2.2f) b = 1f;
                    Put(ref A, ref B, Mathf.Clamp01(hw + 1f - ax), b);
                }
            }

            if (s >= 0.15f && s < 0.975f && ax < 3.8f)
            {
                Put(ref A, ref B, Mathf.Clamp01(4.8f - ax), ax > 3f ? 1f : Side(px, 3.8f, 0.95f, 0.4f));
            }

            if (s >= 0.165f && s < 0.175f && ax < 9f)
            {
                Put(ref A, ref B, 1f, 1f);
            }

            if (s > 0.175f && s < 0.34f)
            {
                float u = (s - 0.175f) / 0.165f;
                for (int k = 0; k < 7; k++)
                {
                    float cen = (k - 3) * 6.5f * Mathf.Pow(u, 0.8f) + Mathf.Sin(u * 6f + k) * 2.5f;
                    if (Mathf.Abs(px - cen) < 1.9f * (1f - 0.35f * u))
                    {
                        Put(ref A, ref B, 0.92f * (1f - 0.35f * u), 0.8f);
                    }
                }
            }

            if (s > 0.97f && ax < 5.5f)
            {
                Put(ref A, ref B, 1f, 0.85f);
            }

            return Out(A, B);
        }

        static Color32 Hammer(int x, int y, int w, int h)
        {
            float px = x + 0.5f - w * 0.5f, ax = Mathf.Abs(px), s = 1f - (y + 0.5f) / h;
            float A = 0f, B = 0f;
            if (s > 0.03f && s < 0.31f)
            {
                float dTop = (s - 0.03f) * h, dBot = (0.31f - s) * h;
                float hw = 106f - Mathf.Max(0f, 14f - Mathf.Min(dTop, dBot));
                if (ax < hw)
                {
                    float b = Mathf.Lerp(1f, 0.5f, (s - 0.03f) / 0.28f) * Side(px, hw, 1f, 0.62f);
                    if ((ax > 64f && ax < 71f)) b = 1f;
                    if (ax > hw - 3f || dTop < 3f) b = 1f;
                    if (H(x / 18, y / 3) > 0.86f) b *= 0.7f;
                    float crack = Mathf.Abs(px + 26f - (s - 0.03f) * 120f);
                    if (crack < 1.6f && s > 0.05f && s < 0.2f) b = 1.4f;
                    Put(ref A, ref B, Mathf.Clamp01(hw - ax + 1f), b);
                }
            }

            if (s >= 0.3f && s < 0.985f && ax < 6.5f)
            {
                float b = Side(px, 6.5f, 0.95f, 0.4f);
                if (Mathf.Repeat(y * 0.06f, 1f) < 0.18f) b *= 0.6f;
                Put(ref A, ref B, Mathf.Clamp01(7.5f - ax), ax > 5.5f ? 1f : b);
            }

            if (s > 0.96f && ax < 11f)
            {
                Put(ref A, ref B, 1f, 0.9f);
            }

            return Out(A, B);
        }

        static Color32 Halberd(int x, int y, int w, int h)
        {
            float px = x + 0.5f - 70f, ax = Mathf.Abs(px), yt = h - (y + 0.5f), s = yt / h;
            float A = 0f, B = 0f;
            if (s < 0.15f)
            {
                float t = s / 0.15f;
                float hw = 15f * Mathf.Sin(Mathf.Pow(t, 0.6f) * Mathf.PI);
                if (ax < hw + 1f)
                {
                    float b = Side(px, hw, 1f, 0.4f);
                    if (ax < 1.5f || ax > hw - 2f) b = 1f;
                    Put(ref A, ref B, Mathf.Clamp01(hw + 1f - ax), b);
                }
            }

            if (s >= 0.13f && s < 0.985f && ax < 4.5f)
            {
                Put(ref A, ref B, Mathf.Clamp01(5.5f - ax), ax > 3.5f ? 1f : Side(px, 4.5f, 0.95f, 0.4f));
            }

            // crescent blade on the right of the shaft
            float dxo = px - 12f, dyo = yt - 235f;
            float dOut = Mathf.Sqrt(dxo * dxo + dyo * dyo);
            float dxi = px + 8f, dyi = yt - 235f;
            float dIn = Mathf.Sqrt(dxi * dxi + dyi * dyi);
            if (dOut <= 92f && dIn >= 66f && px > 4f)
            {
                float b = Mathf.Lerp(1f, 0.5f, Mathf.Clamp01((92f - dOut) / 42f));
                if (dOut > 88f || dIn < 70f) b = 1f;
                if (H(x / 3, y / 20) > 0.85f) b *= 0.7f;
                Put(ref A, ref B, Mathf.Clamp01(92f - dOut + 0.5f), b);
            }

            // tassel
            if (s > 0.17f && s < 0.34f && px < 0f)
            {
                float u = (s - 0.17f) / 0.17f;
                for (int k = 0; k < 5; k++)
                {
                    float cen = -(k + 1) * 5f * Mathf.Pow(u, 0.8f) + Mathf.Sin(u * 6f + k) * 2f;
                    if (Mathf.Abs(px - cen) < 1.9f)
                    {
                        Put(ref A, ref B, 0.9f, 0.8f);
                    }
                }
            }

            return Out(A, B);
        }

        static Color32 Arrow(int x, int y, int w, int h)
        {
            float px = x + 0.5f - w * 0.5f, ax = Mathf.Abs(px), s = 1f - (y + 0.5f) / h;
            float A = 0f, B = 0f;
            if (s < 0.12f)
            {
                float t = s / 0.12f;
                float hw = 15f * Mathf.Sin(Mathf.Pow(t, 0.6f) * Mathf.PI * 0.95f);
                if (ax < hw + 1f)
                {
                    float b = Side(px, hw, 1f, 0.45f);
                    if (ax < 1.2f || ax > hw - 2f) b = 1f;
                    Put(ref A, ref B, Mathf.Clamp01(hw + 1f - ax), b);
                }
            }

            if (s >= 0.1f && s < 0.985f && ax < 2.6f)
            {
                Put(ref A, ref B, Mathf.Clamp01(3.4f - ax), ax > 1.8f ? 1f : 0.7f);
            }

            if (s > 0.76f && s < 0.985f)
            {
                float u = (s - 0.76f) / 0.225f;
                float hw = 26f * Mathf.Min(1f, u * 5f) * (u > 0.8f ? (1f - u) * 5f : 1f);
                if (ax > 2.6f && ax < hw)
                {
                    float stripe = Mathf.Repeat(y * 0.35f + ax * 0.5f, 1f);
                    Put(ref A, ref B, stripe < 0.82f ? 0.95f : 0.35f, Side(px, hw, 0.95f, 0.55f));
                }
            }

            if (s > 0.975f && ax < 5f)
            {
                Put(ref A, ref B, 1f, 1f);
            }

            return Out(A, B);
        }

        static Color32 Kunai(int x, int y, int w, int h)
        {
            float px = x + 0.5f - w * 0.5f, ax = Mathf.Abs(px), s = 1f - (y + 0.5f) / h, yt = h - (y + 0.5f);
            float A = 0f, B = 0f;
            if (s < 0.58f)
            {
                float t = s / 0.58f;
                float hw = 22f * Mathf.Sin(Mathf.Pow(t, 0.5f) * Mathf.PI * 0.5f) + (H(y / 3, 9) - 0.5f) * 1.2f * Mathf.Clamp01(t * 6f);
                if (ax < hw + 1f)
                {
                    float b = Side(px, hw, 1f, 0.38f);
                    if (ax < 1.5f || ax > hw - 2.2f) b = 1f;
                    if (H(x / 2, y / 24) > 0.83f) b *= 0.72f;
                    Put(ref A, ref B, Mathf.Clamp01(hw + 1f - ax), b);
                }
            }

            if (s >= 0.575f && s < 0.62f && ax < 25f)
            {
                Put(ref A, ref B, 1f, 0.95f);
            }

            if (s >= 0.62f && s < 0.80f && ax < 9f)
            {
                float wrap = Mathf.Repeat(y * 0.12f + px * 0.4f, 1f);
                Put(ref A, ref B, 1f, wrap < 0.3f ? 0.95f : 0.3f);
            }

            float dy = yt - 0.9f * h, dr = Mathf.Sqrt(px * px + dy * dy);
            if (dr < 29f && dr > 18f)
            {
                Put(ref A, ref B, Mathf.Clamp01(Mathf.Min(29.5f - dr, dr - 17.5f)), 1f);
            }

            return Out(A, B);
        }

        static Color32 Stick(int x, int y, int w, int h)
        {
            float px = x + 0.5f - w * 0.5f, ax = Mathf.Abs(px), s = 1f - (y + 0.5f) / h;
            float A = 0f, B = 0f;
            if (s > 0.02f && s < 0.98f && ax < 16f)
            {
                float b = Side(px, 16f, 1f, 0.35f);
                if (ax > 13.5f) b = 1f;
                if (H(x / 3, y / 14) > 0.8f) b *= 0.75f;
                if (Mathf.Repeat(s * 14f, 1f) < 0.05f) b *= 0.55f;
                Put(ref A, ref B, Mathf.Clamp01(17f - ax), b);
            }

            if ((s > 0.015f && s < 0.12f) || (s > 0.88f && s < 0.985f))
            {
                if (ax < 19f)
                {
                    Put(ref A, ref B, Mathf.Clamp01(20f - ax), (Mathf.Repeat(s * 90f, 1f) < 0.5f) ? 1f : 0.7f);
                }
            }

            float cy = (s < 0.5f) ? 0.045f : 0.955f;
            float dy = (s - cy) * h;
            if (Mathf.Sqrt(px * px + dy * dy) < 6f)
            {
                return Out(1f, 0.1f);
            }

            return Out(A, B);
        }

        static Color32 Brush(int x, int y, int w, int h)
        {
            float u = (x + 0.5f) / w, v = (y + 0.5f) / h * 2f - 1f, av = Mathf.Abs(v);
            float edge = 0.92f + (H(x / 5, 1) - 0.5f) * 0.14f * (v > 0f ? 1f : 0.6f) + (H(x / 2, 7) - 0.5f) * 0.06f;
            if (av > edge)
            {
                return Out(0f, 0f);
            }

            float a = Mathf.Clamp01((edge - av) * 14f);
            float b = Mathf.Lerp(0.45f, 1f, Mathf.Pow(u, 0.8f)) * (v < 0f ? Mathf.Lerp(1f, 0.55f, av) : 1f);
            if (v > 0.55f) b = Mathf.Max(b, 0.95f);
            if (v > 0.8f) b = 1f;
            if (H(0, y / 3) > 0.76f && u < 0.9f)
            {
                a *= 0.3f + 0.7f * u;
                b *= 0.8f;
            }

            if (u > 0.97f)
            {
                a *= Mathf.Clamp01((1f - u) * 33f);
            }

            return Out(a, b);
        }
    }
}
#endif
