using System.Collections.Generic;
using UnityEngine;

namespace YinYangVFX
{
    /// <summary>Concrete (filled, ink-style) shapes that live in one procedural atlas texture (VFX_Shapes.png).</summary>
    public enum Shape
    {
        Sword = 0,
        Spear = 1,
        Hammer = 2,
        Halberd = 3,
        Arrow = 4,
        Kunai = 5,
        Stick = 6,
        Brush = 7,
        Taiji = 8,
        Hex = 9,
        Shard = 10,
        Star5 = 11,
        Cross = 12,
    }

    public static class ShapeAtlas
    {
        public const int W = 1024;
        public const int H = 2048;

        // x, y, width, height in pixels (y counted from the bottom). Order = Shape enum.
        public static readonly int[][] Rects =
        {
            new[] { 0, 0, 160, 640 },     // Sword
            new[] { 160, 0, 112, 896 },   // Spear
            new[] { 272, 0, 256, 640 },   // Hammer
            new[] { 528, 0, 256, 768 },   // Halberd
            new[] { 784, 0, 96, 640 },    // Arrow
            new[] { 880, 0, 128, 384 },   // Kunai
            new[] { 0, 900, 96, 480 },    // Stick (nunchaku stick)
            new[] { 128, 900, 512, 128 }, // Brush (stroke strip: u = tail..head, v = across)
            new[] { 0, 1040, 256, 256 },   // Taiji (yin-yang disc)
            new[] { 260, 1040, 192, 192 }, // Hex (shield panel)
            new[] { 460, 1040, 128, 256 }, // Shard (broken piece, tip up)
            new[] { 600, 1040, 128, 128 }, // Star5
            new[] { 740, 1040, 128, 128 }, // Cross (heal)
        };

        // Where the hand / pivot sits on each shape (u across, v from the bottom).
        public static readonly float[] PivotU = { 0.5f, 0.5f, 0.5f, 0.27f, 0.5f, 0.5f, 0.5f, 0.5f, 0.5f, 0.5f, 0.5f, 0.5f, 0.5f };
        public static readonly float[] PivotV = { 0.2f, 0.35f, 0.2f, 0.4f, 0.5f, 0.3f, 0.12f, 0.5f, 0.5f, 0.5f, 0.5f, 0.5f, 0.5f };
    }

    // Solid shapes: props (sword, spear, hammer ...) and brush strokes drawn as real meshes.
    public partial class WeaponEffect
    {
        /// <summary>Alpha-blended material using the shape atlas VFX_Shapes.png (set by the builder).</summary>
        public Material shapeMaterial;

        class Piece
        {
            public GameObject go;
            public Mesh mesh;
            public MeshRenderer mr;
            public MaterialPropertyBlock mpb = new MaterialPropertyBlock();
        }

        readonly List<Piece> _pieces = new List<Piece>();
        int _pieceUsed;
        static readonly List<Vector3> MV = new List<Vector3>();
        static readonly List<Vector2> MUV = new List<Vector2>();
        static readonly List<Color32> MC = new List<Color32>();
        static readonly List<int> MT = new List<int>();

        Piece GetPiece()
        {
            int idx = _pieceUsed++;
            if (idx >= _pieces.Count)
            {
                Piece q = new Piece();
                q.go = new GameObject("Shape");
                q.go.transform.SetParent(transform, false);
                q.mesh = new Mesh { name = "ShapePiece" };
                q.mesh.MarkDynamic();
                q.go.AddComponent<MeshFilter>().sharedMesh = q.mesh;
                q.mr = q.go.AddComponent<MeshRenderer>();
                q.mr.sharedMaterial = shapeMaterial;
                q.mr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                q.mr.receiveShadows = false;
                _pieces.Add(q);
            }

            return _pieces[idx];
        }

        void CommitPiece(Piece q, int idx)
        {
            q.mesh.Clear();
            q.mesh.SetVertices(MV);
            q.mesh.SetUVs(0, MUV);
            q.mesh.SetColors(MC);
            q.mesh.SetTriangles(MT, 0);
            q.mesh.RecalculateBounds();
            Color tint = primary * 1.5f;
            tint.a = 1f;
            q.mpb.SetColor(ID_Tint, tint);
            q.mr.SetPropertyBlock(q.mpb);
            q.mr.sortingOrder = sortingOrder + 2 + idx;
            q.go.SetActive(true);
        }

        static Vector2 AtlasUV(int[] r, float u01, float v01)
        {
            float x = r[0] + 0.5f + (r[2] - 1f) * u01;
            float y = r[1] + 0.5f + (r[3] - 1f) * v01;
            return new Vector2(x / ShapeAtlas.W, y / ShapeAtlas.H);
        }

        /// <summary>
        /// Draws a prop. (px,py) = where its pivot / grip sits (local units), dirDeg = direction its tip points
        /// (0 = right, 90 = up), len = total length of the shape in local units, a = opacity.
        /// </summary>
        void DrawProp(Shape sh, float px, float py, float dirDeg, float len, float a)
        {
            a *= _gm;
            if (shapeMaterial == null || a <= 0.01f)
            {
                return;
            }

            int si = (int)sh;
            int[] r = ShapeAtlas.Rects[si];
            float wd = len * r[2] / r[3];
            float rad = dirDeg * Mathf.Deg2Rad;
            Vector2 d = new Vector2(Mathf.Cos(rad), Mathf.Sin(rad));
            Vector2 rt = new Vector2(d.y, -d.x);
            float pu = ShapeAtlas.PivotU[si], pv = ShapeAtlas.PivotV[si];
            byte ab = (byte)Mathf.RoundToInt(Mathf.Clamp01(a) * 255f);
            MV.Clear(); MUV.Clear(); MC.Clear(); MT.Clear();
            for (int i = 0; i < 4; i++)
            {
                float cu = (i == 1 || i == 2) ? 1f : 0f;
                float cv = (i >= 2) ? 1f : 0f;
                Vector2 p = new Vector2(px, py) + rt * ((cu - pu) * wd) + d * ((cv - pv) * len);
                MV.Add(new Vector3(p.x, p.y, 0f));
                MUV.Add(AtlasUV(r, cu, cv));
                MC.Add(new Color32(255, 255, 255, ab));
            }

            MT.Add(0); MT.Add(1); MT.Add(2); MT.Add(0); MT.Add(2); MT.Add(3);
            int idx = _pieceUsed;
            CommitPiece(GetPiece(), idx);
        }

        /// <summary>A prop standing upright (tip up) whose part below groundY is cropped, so it emerges from the ground.</summary>
        void DrawStanding(Shape sh, float x, float bottom, float len, float a)
        {
            a *= _gm;
            if (shapeMaterial == null || a <= 0.01f)
            {
                return;
            }

            int[] r = ShapeAtlas.Rects[(int)sh];
            float w = len * r[2] / r[3];
            float top = bottom + len;
            float v0 = 0f;
            if (bottom < groundY)
            {
                v0 = (groundY - bottom) / len;
                bottom = groundY;
            }

            if (top <= groundY + 0.005f)
            {
                return;
            }

            byte ab = (byte)Mathf.RoundToInt(Mathf.Clamp01(a) * 255f);
            float z = 0.002f * len;
            MV.Clear(); MUV.Clear(); MC.Clear(); MT.Clear();
            MV.Add(new Vector3(x - w * 0.5f, bottom, z));
            MV.Add(new Vector3(x + w * 0.5f, bottom, z));
            MV.Add(new Vector3(x + w * 0.5f, top, z));
            MV.Add(new Vector3(x - w * 0.5f, top, z));
            MUV.Add(AtlasUV(r, 0f, v0));
            MUV.Add(AtlasUV(r, 1f, v0));
            MUV.Add(AtlasUV(r, 1f, 1f));
            MUV.Add(AtlasUV(r, 0f, 1f));
            for (int i = 0; i < 4; i++)
            {
                MC.Add(new Color32(255, 255, 255, ab));
            }

            MT.Add(0); MT.Add(1); MT.Add(2); MT.Add(0); MT.Add(2); MT.Add(3);
            int idx = _pieceUsed;
            CommitPiece(GetPiece(), idx);
        }

        /// <summary>
        /// Draws the first n points of the scratch buffer (local space) as a solid ink brush stroke:
        /// thin tail -> thick body -> sharp head. wMax = full width, a = opacity.
        /// </summary>
        void DrawBrush(int n, float wMax, float a)
        {
            a *= _gm;
            if (shapeMaterial == null || n < 2 || a <= 0.01f)
            {
                return;
            }

            int[] r = ShapeAtlas.Rects[(int)Shape.Brush];
            MV.Clear(); MUV.Clear(); MC.Clear(); MT.Clear();
            for (int i = 0; i < n; i++)
            {
                Vector2 p = _buf[i];
                Vector2 pr = _buf[Mathf.Max(0, i - 1)];
                Vector2 nx = _buf[Mathf.Min(n - 1, i + 1)];
                Vector2 tg = nx - pr;
                if (tg.sqrMagnitude < 1e-8f)
                {
                    tg = Vector2.right;
                }

                tg.Normalize();
                Vector2 nm = new Vector2(-tg.y, tg.x);
                float t = i / (float)(n - 1);
                float hw = Mathf.Max(0.004f, wMax * 0.5f * Mathf.Pow(Mathf.Sin(Mathf.PI * Mathf.Pow(t, 0.7f)), 0.9f));
                byte ab = (byte)Mathf.RoundToInt(Mathf.Clamp01(a * (0.2f + 0.8f * Mathf.Pow(t, 0.6f))) * 255f);
                MV.Add(new Vector3(p.x + nm.x * hw, p.y + nm.y * hw, 0f));
                MV.Add(new Vector3(p.x - nm.x * hw, p.y - nm.y * hw, 0f));
                MUV.Add(AtlasUV(r, t, 1f));
                MUV.Add(AtlasUV(r, t, 0f));
                MC.Add(new Color32(255, 255, 255, ab));
                MC.Add(new Color32(255, 255, 255, ab));
                if (i < n - 1)
                {
                    int b = i * 2;
                    MT.Add(b); MT.Add(b + 1); MT.Add(b + 2);
                    MT.Add(b + 1); MT.Add(b + 3); MT.Add(b + 2);
                }
            }

            int idx = _pieceUsed;
            CommitPiece(GetPiece(), idx);
        }

        void EndSwords()
        {
            for (int i = _pieceUsed; i < _pieces.Count; i++)
            {
                _pieces[i].go.SetActive(false);
            }

            _pieceUsed = 0;
        }

        void HideAllSwords()
        {
            for (int i = 0; i < _pieces.Count; i++)
            {
                _pieces[i].go.SetActive(false);
            }

            _pieceUsed = 0;
        }

        // Five solid ink swords launch from the ground and keep accelerating straight up until they leave the screen.
        // No light particles; airflow (wind ribbons + vortex rings) wraps the flying swords. Origin = ground point.
        void AnimInkSwords()
        {
            Vector2 c = sweepCenter;
            float[] dx = { -0.62f, -0.31f, 0f, 0.31f, 0.62f };
            float[] sz = { 1.5f, 2.0f, 2.5f, 2.1f, 1.6f };
            float[] ts = { 0.0f, 0.05f, 0.09f, 0.06f, 0.02f };
            if (Once(1)) Fire();

            for (int j = 0; j < 3; j++)
            {
                float d = Mathf.Clamp01((_t - 0.05f * j) / 0.45f);
                int rn = Ring(c.x, groundY, 1.9f * EaseOut(d), 0.3f * EaseOut(d));
                Draw(Taper.Flat, rn, 0.04f * (1f - d) * thickness, primary, 1f - d);
            }

            for (int i = 0; i < 5; i++)
            {
                float x = c.x + dx[i];
                float tt = _t - ts[i];
                if (tt < 0f) continue;
                float S = sz[i];
                float bottom = groundY - S + 16f * tt + 34f * tt * tt;
                float a = Mathf.Clamp01(tt * 30f);
                float v = Mathf.Clamp01(0.55f + tt * 1.6f);
                float tip = bottom + S;

                float trailBottom = Mathf.Max(groundY, bottom - 3.5f - 6f * tt);
                if (tip > trailBottom + 0.05f)
                {
                    int n = Seg(x, trailBottom, x, bottom + S * 0.7f, 2);
                    Draw(Taper.Head, n, 0.36f * thickness, primary * 0.5f, 0.7f * a);
                    n = Seg(x, trailBottom, x, bottom + S * 0.5f, 2);
                    Draw(Taper.Head, n, 0.1f * thickness, primary, 0.9f * a);
                }

                for (int k = 0; k < 5; k++)
                {
                    float side = (k % 2 == 0 ? 1f : -1f);
                    float off = side * (0.14f + 0.07f * (k / 2) + 0.04f * Rnd(i * 7 + k, 1));
                    float y0 = Mathf.Max(groundY, bottom - 1.0f - 1.6f * Rnd(i * 5 + k, 2));
                    float y1 = bottom + S * (0.95f + 0.4f * Rnd(i * 3 + k, 3));
                    if (y1 <= y0 + 0.1f) continue;
                    int n = WaveDir(x + off, y0, 90f, y1 - y0, 26, 0.07f + 0.05f * Rnd(i * 11 + k, 4), 3.0f + 1.5f * Rnd(i * 13 + k, 5), -tt * 26f + k * 1.7f + i);
                    Draw(Taper.Head, n, 0.035f * thickness, primary, 0.55f * v * a);
                }

                for (int j = 0; j < 3; j++)
                {
                    float f = Mathf.Repeat(tt * 5f + j / 3f + i * 0.17f, 1f);
                    float ry = bottom - 0.2f - f * 2.2f;
                    if (ry <= groundY) continue;
                    float rx = 0.18f + 0.5f * f;
                    int n = Ring(x, ry, rx, rx * 0.2f);
                    Draw(Taper.Flat, n, 0.025f * thickness, primary, (1f - f) * 0.7f * a);
                }

                DrawStanding(Shape.Sword, x, bottom, S, a);
            }
        }
    }
}
