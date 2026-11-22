using System.Collections.Generic;
using UnityEngine;

namespace YinYangVFX
{
    // Solid (filled) ink-style swords rising out of the ground. Uses a procedural sword texture
    // (VFX_InkSword.png, built by the builder) on cropped quads, so it looks like a real sword, not strokes.
    public partial class WeaponEffect
    {
        /// <summary>Alpha-blended material using VFX_InkSword.png (set by the builder).</summary>
        public Material swordMaterial;

        class SwordQuad
        {
            public GameObject go;
            public Mesh mesh;
            public MeshRenderer mr;
            public MaterialPropertyBlock mpb = new MaterialPropertyBlock();
            public Vector3[] v = new Vector3[4];
            public Vector2[] uv = new Vector2[4];
            public Color32[] c = new Color32[4];
        }

        readonly List<SwordQuad> _swords = new List<SwordQuad>();
        int _swordUsed;

        SwordQuad NewSword()
        {
            SwordQuad q = new SwordQuad();
            q.go = new GameObject("Sword");
            q.go.transform.SetParent(transform, false);
            q.mesh = new Mesh { name = "SwordQuad" };
            q.mesh.MarkDynamic();
            q.mesh.vertices = q.v;
            q.mesh.uv = q.uv;
            q.mesh.triangles = new[] { 0, 1, 2, 0, 2, 3 };
            q.go.AddComponent<MeshFilter>().sharedMesh = q.mesh;
            q.mr = q.go.AddComponent<MeshRenderer>();
            q.mr.sharedMaterial = swordMaterial;
            q.mr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            q.mr.receiveShadows = false;
            return q;
        }

        /// <summary>
        /// Draws one sword (tip up) at local x. s = total sword length in local units, bottom = y of the
        /// pommel end (anything below groundY is cropped, so a sword launching out of the ground emerges from it),
        /// a = opacity.
        /// </summary>
        void DrawSword(float x, float s, float bottom, float a)
        {
            a *= _gm;
            if (swordMaterial == null || a <= 0.01f)
            {
                return;
            }

            float w = s * 0.25f; // texture aspect 192:768
            float top = bottom + s;
            float v0 = 0f;
            if (bottom < groundY)
            {
                v0 = (groundY - bottom) / s;
                bottom = groundY;
            }

            if (top <= groundY + 0.005f)
            {
                return;
            }

            int idx = _swordUsed++;
            if (idx >= _swords.Count)
            {
                _swords.Add(NewSword());
            }

            SwordQuad q = _swords[idx];
            float z = 0.002f * s;
            q.v[0] = new Vector3(x - w * 0.5f, bottom, z);
            q.v[1] = new Vector3(x + w * 0.5f, bottom, z);
            q.v[2] = new Vector3(x + w * 0.5f, top, z);
            q.v[3] = new Vector3(x - w * 0.5f, top, z);
            q.uv[0] = new Vector2(0f, v0);
            q.uv[1] = new Vector2(1f, v0);
            q.uv[2] = new Vector2(1f, 1f);
            q.uv[3] = new Vector2(0f, 1f);
            byte ab = (byte)Mathf.RoundToInt(Mathf.Clamp01(a) * 255f);
            for (int i = 0; i < 4; i++)
            {
                q.c[i] = new Color32(255, 255, 255, ab);
            }

            q.mesh.vertices = q.v;
            q.mesh.uv = q.uv;
            q.mesh.colors32 = q.c;
            q.mesh.RecalculateBounds();

            Color tint = primary * 1.5f;
            tint.a = 1f;
            q.mpb.SetColor(ID_Tint, tint);
            q.mr.SetPropertyBlock(q.mpb);
            q.mr.sortingOrder = sortingOrder + 2;
            q.go.SetActive(true);
        }

        void EndSwords()
        {
            for (int i = _swordUsed; i < _swords.Count; i++)
            {
                _swords[i].go.SetActive(false);
            }

            _swordUsed = 0;
        }

        void HideAllSwords()
        {
            for (int i = 0; i < _swords.Count; i++)
            {
                _swords[i].go.SetActive(false);
            }

            _swordUsed = 0;
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

            // ground shock rings at launch (lines only)
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
                // starts buried, explodes upward and keeps accelerating until it is far off the top of the screen
                float bottom = groundY - S + 16f * tt + 34f * tt * tt;
                float a = Mathf.Clamp01(tt * 30f);
                float v = Mathf.Clamp01(0.55f + tt * 1.6f); // streak strength grows with speed
                float tip = bottom + S;

                // long speed trail behind (below) the blade
                float trailBottom = Mathf.Max(groundY, bottom - 3.5f - 6f * tt);
                if (tip > trailBottom + 0.05f)
                {
                    int n = Seg(x, trailBottom, x, bottom + S * 0.7f, 2);
                    Draw(Taper.Head, n, 0.36f * thickness, primary * 0.5f, 0.7f * a);
                    n = Seg(x, trailBottom, x, bottom + S * 0.5f, 2);
                    Draw(Taper.Head, n, 0.1f * thickness, primary, 0.9f * a);
                }

                // airflow: wind ribbons streaming around the blade
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

                // air vortex rings spreading behind the blade
                for (int j = 0; j < 3; j++)
                {
                    float f = Mathf.Repeat(tt * 5f + j / 3f + i * 0.17f, 1f);
                    float ry = bottom - 0.2f - f * 2.2f;
                    if (ry <= groundY) continue;
                    float rx = 0.18f + 0.5f * f;
                    int n = Ring(x, ry, rx, rx * 0.2f);
                    Draw(Taper.Flat, n, 0.025f * thickness, primary, (1f - f) * 0.7f * a);
                }

                DrawSword(x, S, bottom, a);
            }
        }
    }
}
