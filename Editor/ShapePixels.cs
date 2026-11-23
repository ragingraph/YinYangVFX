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
                case Shape.Taiji: return Taiji(x, y, w, h);
                case Shape.Hex: return Hex(x, y, w, h);
                case Shape.Shard: return Shard(x, y, w, h);
                case Shape.Star5: return Star5(x, y, w, h);
                case Shape.Cross: return Cross(x, y, w, h);
                case Shape.Flame: return Flame(x, y, w, h);
                case Shape.Ice: return Ice(x, y, w, h);
                case Shape.Bolt: return Bolt(x, y, w, h);
                case Shape.Rock: return Rock(x, y, w, h);
                case Shape.Orb: return Orb(x, y, w, h);
                case Shape.DarkOrb: return DarkOrb(x, y, w, h);
                case Shape.Drop: return Drop(x, y, w, h);
                case Shape.Feather: return Feather(x, y, w, h);
                case Shape.Skull: return Skull(x, y, w, h);
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

        static bool InPoly(float x, float y, float[] p)
        {
            bool c = false;
            int n = p.Length / 2;
            for (int i = 0, j = n - 1; i < n; j = i++)
            {
                float xi = p[2 * i], yi = p[2 * i + 1], xj = p[2 * j], yj = p[2 * j + 1];
                if (((yi > y) != (yj > y)) && (x < (xj - xi) * (y - yi) / (yj - yi) + xi))
                {
                    c = !c;
                }
            }

            return c;
        }

        static float EdgeD(float x, float y, float[] p)
        {
            int n = p.Length / 2;
            float best = 1e9f;
            for (int i = 0; i < n; i++)
            {
                int j = (i + 1) % n;
                float ax = p[2 * i], ay = p[2 * i + 1], bx = p[2 * j], by = p[2 * j + 1];
                float dx = bx - ax, dy = by - ay;
                float t = Mathf.Clamp01(((x - ax) * dx + (y - ay) * dy) / (dx * dx + dy * dy + 1e-6f));
                float ex = ax + dx * t - x, ey = ay + dy * t - y;
                best = Mathf.Min(best, Mathf.Sqrt(ex * ex + ey * ey));
            }

            return best;
        }

        static Color32 Taiji(int x, int y, int w, int h)
        {
            float dx = x + 0.5f - w * 0.5f, dy = y + 0.5f - h * 0.5f;
            const float R = 108f;
            float r = Mathf.Sqrt(dx * dx + dy * dy);
            if (r > R + 1f)
            {
                float ang = Mathf.Atan2(dy, dx) * Mathf.Rad2Deg;
                float m = Mathf.Abs(Mathf.Repeat(ang + 22.5f, 45f) - 22.5f);
                if (r > 114f && r < 118f) return Out(1f, 1f);
                if (r > 121f && r < 127f && m < 3f) return Out(1f, 1f);
                return Out(0f, 0f);
            }

            if (r > R - 6f) return Out(Mathf.Clamp01(R + 1f - r), 1f);
            float d1 = Mathf.Sqrt(dx * dx + (dy - R * 0.5f) * (dy - R * 0.5f));
            float d2 = Mathf.Sqrt(dx * dx + (dy + R * 0.5f) * (dy + R * 0.5f));
            bool bright = (dx > 0f && d2 >= R * 0.5f) || d1 < R * 0.5f;
            if (d1 < R * 0.125f) bright = false;
            if (d2 < R * 0.125f) bright = true;
            float edge = Mathf.Min(Mathf.Abs(d1 - R * 0.5f), Mathf.Abs(d2 - R * 0.5f));
            if (edge < 2f && Mathf.Abs(dx) < 4f + R) return Out(1f, 1f);
            return bright ? Out(0.95f, Mathf.Lerp(1f, 0.7f, r / R)) : Out(0.9f, 0.16f);
        }

        static Color32 Hex(int x, int y, int w, int h)
        {
            float dx = x + 0.5f - w * 0.5f, dy = y + 0.5f - h * 0.5f;
            const float R = 92f;
            float ax = Mathf.Abs(dx), ay = Mathf.Abs(dy);
            float e = Mathf.Min(R * 0.866f - ay, R * 0.866f - ax * 0.866f - ay * 0.5f);
            if (e < 0f) return Out(0f, 0f);
            if (e < 6f) return Out(1f, 1f);
            // inner honeycomb: smaller hex outline + spokes
            float e2 = Mathf.Min(R * 0.5f - ay, R * 0.5f - ax * 0.866f - ay * 0.5f);
            if (Mathf.Abs(e2) < 2.2f) return Out(0.9f, 1f);
            float ang = Mathf.Atan2(dy, dx) * Mathf.Rad2Deg;
            float m = Mathf.Abs(Mathf.Repeat(ang + 30f, 60f) - 30f);
            float rr = Mathf.Sqrt(dx * dx + dy * dy);
            if (m < 1.6f && rr > R * 0.45f && rr < R * 0.85f) return Out(0.8f, 0.9f);
            return Out(0.4f + 0.2f * (rr / R), 0.55f + 0.4f * (rr / R));
        }

        static Color32 Shard(int x, int y, int w, int h)
        {
            float px = x + 0.5f, yt = h - (y + 0.5f);
            float[] poly = { 64f, 2f, 118f, 150f, 104f, 252f, 22f, 254f, 8f, 140f };
            if (!InPoly(px, yt, poly)) return Out(0f, 0f);
            float e = EdgeD(px, yt, poly);
            if (e < 4f) return Out(1f, 1f);
            float b = px < 64f + (yt / 256f) * 2f ? 0.95f : 0.5f;
            if (Mathf.Abs(px - (64f - yt * 0.1f + 12f)) < 1.6f && yt > 60f && yt < 190f) b = 1f;
            return Out(0.95f, b);
        }

        static Color32 Star5(int x, int y, int w, int h)
        {
            float px = x + 0.5f - w * 0.5f, py = (h - (y + 0.5f)) - h * 0.5f;
            float[] poly = new float[20];
            for (int i = 0; i < 10; i++)
            {
                float a = (-90f + i * 36f) * Mathf.Deg2Rad;
                float rr = (i % 2 == 0) ? 60f : 25f;
                poly[2 * i] = Mathf.Cos(a) * rr;
                poly[2 * i + 1] = Mathf.Sin(a) * rr;
            }

            if (!InPoly(px, py, poly)) return Out(0f, 0f);
            float e = EdgeD(px, py, poly);
            if (e < 3f) return Out(1f, 1f);
            float ang = Mathf.Atan2(py, px) * Mathf.Rad2Deg + 90f;
            float wedge = Mathf.Repeat(ang, 72f);
            return Out(0.95f, wedge < 36f ? 1f : 0.62f);
        }

        static Color32 Cross(int x, int y, int w, int h)
        {
            float ax = Mathf.Abs(x + 0.5f - w * 0.5f), ay = Mathf.Abs(y + 0.5f - h * 0.5f);
            bool In(float hw, float hl) => (ax < hw && ay < hl) || (ay < hw && ax < hl);
            if (!In(22f, 60f)) return Out(0f, 0f);
            if (!In(18f, 56f)) return Out(1f, 1f);
            if (!In(10f, 46f)) return Out(0.95f, 0.6f);
            return Out(0.95f, 1f);
        }

        static float DistPoly(float x, float y, float[] p)
        {
            int n = p.Length / 2;
            float best = 1e9f;
            for (int i = 0; i < n - 1; i++)
            {
                float ax = p[2 * i], ay = p[2 * i + 1], bx = p[2 * i + 2], by = p[2 * i + 3];
                float dx = bx - ax, dy = by - ay;
                float t = Mathf.Clamp01(((x - ax) * dx + (y - ay) * dy) / (dx * dx + dy * dy + 1e-6f));
                float ex = ax + dx * t - x, ey = ay + dy * t - y;
                best = Mathf.Min(best, Mathf.Sqrt(ex * ex + ey * ey));
            }

            return best;
        }

        static readonly float[,] FlameT =
        {
            { 0f, 4f, 46f, 0f }, { -54f, 74f, 34f, 1f }, { 52f, 68f, 36f, 2f }, { -92f, 150f, 24f, 3f },
            { 90f, 142f, 26f, 4f }, { -22f, 120f, 30f, 5f }, { 26f, 126f, 28f, 6f },
        };

        static Color32 Flame(int x, int y, int w, int h)
        {
            float px = x + 0.5f - w * 0.5f, yt = h - (y + 0.5f);
            float baseY = h - 6f, A = 0f, B = 0f;
            for (int k = 0; k < 7; k++)
            {
                float tipY = FlameT[k, 1], hwMax = FlameT[k, 2];
                if (yt < tipY || yt > baseY) continue;
                float t = (yt - tipY) / (baseY - tipY);
                float hw = hwMax * Mathf.Sin(Mathf.Clamp01(Mathf.Pow(t, 0.55f)) * Mathf.PI * 0.5f) * Mathf.Clamp01((baseY - yt) / 14f + 0.35f);
                float cen = FlameT[k, 0] + Mathf.Sin(t * 6f + FlameT[k, 3]) * hwMax * 0.3f * (1f - t * 0.5f);
                float d = Mathf.Abs(px - cen);
                if (d < hw + 1f)
                {
                    float a = Mathf.Clamp01(hw + 1f - d);
                    float inner = 1f - d / Mathf.Max(hw, 1f);
                    float b = Mathf.Lerp(0.5f, 1f, Mathf.Pow(inner, 0.6f));
                    if (k >= 5) b = 1f;
                    A = Mathf.Max(A, a);
                    B = Mathf.Max(B, b);
                }
            }

            return Out(A, B);
        }

        static readonly float[][] IceShards =
        {
            new[] { 18f, 150f, 44f, 210f, 40f, 316f, 4f, 316f, 2f, 200f },
            new[] { 240f, 160f, 252f, 230f, 250f, 316f, 214f, 316f, 210f, 220f },
            new[] { 64f, 76f, 96f, 150f, 92f, 316f, 30f, 316f, 24f, 170f },
            new[] { 196f, 96f, 228f, 184f, 224f, 316f, 168f, 316f, 164f, 160f },
            new[] { 128f, 4f, 164f, 112f, 156f, 316f, 100f, 316f, 92f, 112f },
        };

        static Color32 Ice(int x, int y, int w, int h)
        {
            float px = x + 0.5f, yt = h - (y + 0.5f);
            float A = 0f, B = 0f;
            foreach (float[] sh in IceShards)
            {
                if (!InPoly(px, yt, sh)) continue;
                float cx = 0f;
                for (int i = 0; i < sh.Length; i += 2) cx += sh[i];
                cx /= sh.Length / 2;
                float b = px < cx ? 0.95f : 0.5f;
                if (Mathf.Abs(px - cx) < 1.5f) b = 1f;
                if (EdgeD(px, yt, sh) < 3f) b = 1f;
                if (H(x / 6, y / 30) > 0.9f) b *= 0.8f;
                A = 0.93f;
                B = b;
            }

            return Out(A, B);
        }

        static Color32 Bolt(int x, int y, int w, int h)
        {
            float px = x + 0.5f, yt = h - (y + 0.5f);
            float[] main = { 112f, 2f, 74f, 118f, 120f, 186f, 64f, 312f, 106f, 372f, 86f, 509f };
            float[][] br =
            {
                new[] { 74f, 118f, 24f, 186f, 12f, 262f },
                new[] { 120f, 186f, 172f, 250f, 182f, 336f },
                new[] { 64f, 312f, 22f, 372f, 14f, 430f },
            };
            float hwMain = 8f + 13f * Mathf.Sin(Mathf.PI * Mathf.Clamp01(yt / 512f));
            float d = DistPoly(px, yt, main);
            float A = 0f, B = 0f;
            if (d < hwMain + 1f)
            {
                A = Mathf.Clamp01(hwMain + 1f - d);
                B = d < hwMain * 0.5f ? 1f : 0.82f;
            }

            foreach (float[] b in br)
            {
                float db = DistPoly(px, yt, b);
                if (db < 7f && Mathf.Clamp01(7f - db) > A)
                {
                    A = Mathf.Clamp01(7f - db);
                    B = db < 3f ? 1f : 0.8f;
                }
            }

            return Out(A, B);
        }

        static readonly float[] RockR = { 100f, 112f, 96f, 118f, 104f, 92f, 114f, 100f, 108f, 94f, 110f };
        static readonly float[] RockFacet = { 1f, 0.55f, 0.8f, 0.4f, 0.95f, 0.5f, 0.7f, 0.35f, 0.85f, 0.6f, 0.45f };

        static Color32 Rock(int x, int y, int w, int h)
        {
            float px = x + 0.5f - w * 0.5f, py = (h - (y + 0.5f)) - h * 0.5f;
            int n = RockR.Length;
            float[] poly = new float[n * 2];
            for (int i = 0; i < n; i++)
            {
                float a = (i + 0.3f) * 2f * Mathf.PI / n;
                poly[2 * i] = Mathf.Cos(a) * RockR[i] * 1.1f;
                poly[2 * i + 1] = Mathf.Sin(a) * RockR[i] * 0.92f;
            }

            // note: poly y is up, px/py in this function has y down from centre -> flip py for the test
            float qy = -py;
            if (!InPoly(px, qy, poly)) return Out(0f, 0f);
            float e = EdgeD(px, qy, poly);
            if (e < 3.5f) return Out(1f, 1f);
            float ang = Mathf.Atan2(qy / 0.92f, px / 1.1f);
            if (ang < 0f) ang += 2f * Mathf.PI;
            float f = ang / (2f * Mathf.PI) * n;
            int idx = Mathf.Min(n - 1, Mathf.FloorToInt(f));
            float b = RockFacet[idx] * Mathf.Lerp(1.15f, 0.6f, (py + h * 0.5f) / h);
            if (f - idx < 0.025f || f - idx > 0.975f) b = 0.95f;
            float[] crack = { -28f, 40f, -10f, 12f, -26f, -14f, -8f, -42f };
            if (DistPoly(px, qy, crack) < 1.8f) b = 1f;
            return Out(1f, b);
        }

        static Color32 Orb(int x, int y, int w, int h)
        {
            float dx = x + 0.5f - w * 0.5f, dy = y + 0.5f - h * 0.5f;
            float r = Mathf.Sqrt(dx * dx + dy * dy);
            const float R = 88f;
            if (r > R + 1f) return Out(0f, 0f);
            if (r > R - 8f) return Out(Mathf.Clamp01(R + 1f - r), 1f);
            float ca = Mathf.Cos(0.6f), sa = Mathf.Sin(0.6f);
            float ux = (dx + 34f) * ca + (dy - 36f) * sa, uy = -(dx + 34f) * sa + (dy - 36f) * ca;
            if ((ux * ux) / (27f * 27f) + (uy * uy) / (13f * 13f) < 1f) return Out(1f, 1f);
            float sx = dx - 36f, sy = dy + 34f;
            if (sx * sx + sy * sy < 64f) return Out(0.95f, 0.9f);
            float b = 0.3f + 0.3f * (r / R);
            if (dy < -28f && r > 52f) b += 0.18f;
            return Out(0.78f, b);
        }

        static Color32 DarkOrb(int x, int y, int w, int h)
        {
            float dx = x + 0.5f - w * 0.5f, dy = y + 0.5f - h * 0.5f;
            float r = Mathf.Sqrt(dx * dx + dy * dy);
            float ang = Mathf.Atan2(dy, dx);
            const float R = 82f;
            if (r > R + 1f)
            {
                int i = Mathf.FloorToInt((ang + Mathf.PI) / (2f * Mathf.PI) * 18f);
                if (r < 90f + H(i, 4) * 9f && H(i, 3) > 0.42f) return Out(0.85f, 0.8f);
                return Out(0f, 0f);
            }

            if (r > R - 6f) return Out(Mathf.Clamp01(R + 1f - r), 1f);
            float u = Mathf.Repeat(ang + r * 0.045f, 2f * Mathf.PI / 3f);
            if (u < 0.1f) return Out(1f, 0.6f);
            return Out(0.97f, 0.05f);
        }

        static Color32 Drop(int x, int y, int w, int h)
        {
            float px = x + 0.5f - w * 0.5f, ax = Mathf.Abs(px), yt = h - (y + 0.5f);
            float hw;
            if (yt < 4f) return Out(0f, 0f);
            if (yt < 86f) hw = 40f * Mathf.Pow((yt - 4f) / 82f, 1.5f);
            else
            {
                float dy = yt - 86f;
                hw = dy < 40f ? Mathf.Sqrt(Mathf.Max(0f, 1600f - dy * dy)) : 0f;
            }

            if (ax > hw + 1f) return Out(0f, 0f);
            float a = Mathf.Clamp01(hw + 1f - ax);
            float hx = px + 14f, hy = (yt - 92f) / 2f;
            if (hx * hx / 36f + hy * hy / 36f < 1f && yt > 70f) return Out(1f, 1f);
            if (hw - ax < 4f) return Out(a, 1f);
            return Out(0.82f * a, 0.45f + 0.3f * (yt / h));
        }

        static Color32 Feather(int x, int y, int w, int h)
        {
            float px = x + 0.5f - w * 0.5f, ax = Mathf.Abs(px), yt = h - (y + 0.5f);
            float t = yt / h;
            if (t > 0.88f) return ax < 2f ? Out(Mathf.Clamp01(3f - ax), 0.9f) : Out(0f, 0f);
            float hw = 30f * Mathf.Sin(Mathf.Pow(t / 0.88f, 0.8f) * Mathf.PI * 0.92f);
            if (ax > hw + 1f && ax > 2f) return Out(0f, 0f);
            if (ax < 1.8f) return Out(1f, 1f);
            if (ax > hw - 2f) return Out(Mathf.Clamp01(hw + 1f - ax), 1f);
            float stripe = Mathf.Repeat(yt * 0.2f - ax * 0.45f, 1f);
            return Out(0.95f, stripe < 0.3f ? 0.55f : 1f);
        }

        static Color32 Skull(int x, int y, int w, int h)
        {
            float px = x + 0.5f - w * 0.5f, ax = Mathf.Abs(px), yt = h - (y + 0.5f);
            float cr = (px * px) / (44f * 44f) + ((yt - 50f) * (yt - 50f)) / (40f * 40f);
            bool jaw = ax < 27f && yt > 70f && yt < 116f && (yt < 108f || ax < 22f);
            if (cr >= 1f && !jaw) return Out(0f, 0f);
            float e1 = (px - 21f) * (px - 21f) + (yt - 54f) * (yt - 54f);
            float e2 = (px + 21f) * (px + 21f) + (yt - 54f) * (yt - 54f);
            if (e1 < 144f || e2 < 144f) return Out(1f, 0.03f);
            if (ax < 8f && yt > 68f && yt < 86f && ax < (yt - 68f) * 0.6f + 1f) return Out(1f, 0.03f);
            if (yt > 100f && yt < 116f && Mathf.Abs(Mathf.Repeat(px + 4.5f, 9f) - 4.5f) < 1f) return Out(1f, 0.03f);
            if (cr > 0.8f || (jaw && (ax > 24f || yt > 112f))) return Out(1f, 1f);
            return Out(1f, 0.78f + 0.2f * (1f - yt / h));
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
