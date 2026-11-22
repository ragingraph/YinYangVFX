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

        // Five solid ink swords shoot out of the ground FAST (launch, streak upward, decelerate, hang, fade).
        // Origin = ground point.
        void AnimInkSwords()
        {
            Vector2 c = sweepCenter;
            float g = FadeFrom(1.5f);
            float[] dx = { -0.62f, -0.31f, 0f, 0.31f, 0.62f };
            float[] sz = { 1.5f, 2.0f, 2.5f, 2.1f, 1.6f };
            float[] ts = { 0.0f, 0.05f, 0.09f, 0.06f, 0.02f };
            float[] up = { 1.6f, 2.4f, 3.2f, 2.6f, 1.9f }; // how far each sword flies (pommel height at the end)
            if (Once(1)) Fire();

            for (int j = 0; j < 3; j++)
            {
                float d = Mathf.Clamp01((_t - 0.06f * j) / 0.6f);
                int rn = Ring(c.x, groundY, 1.9f * EaseOut(d), 0.3f * EaseOut(d));
                Draw(Taper.Flat, rn, 0.04f * (1f - d) * thickness, primary, (1f - d) * g);
            }

            for (int i = 0; i < 5; i++)
            {
                float x = c.x + dx[i];
                float tt = _t - ts[i];
                if (tt < 0f) continue;
                float p = Mathf.Clamp01(tt / 0.22f);
                float e = 1f - Mathf.Pow(1f - p, 4f);            // explosive start, quick brake
                float v = Mathf.Pow(1f - p, 3f);                 // 1 = full speed, 0 = stopped
                float bottom = groundY - sz[i] + e * (sz[i] + up[i]) + Mathf.Max(0f, tt - 0.22f) * 0.3f;
                float a = g * Mathf.Clamp01(tt * 25f);

                // speed streaks behind the blade
                float trailBottom = Mathf.Max(groundY, bottom - 0.4f - 2.6f * v);
                float top = bottom + sz[i] * 0.75f;
                if (top > trailBottom + 0.05f)
                {
                    int n = Seg(x, trailBottom, x, top, 2);
                    Draw(Taper.Head, n, 0.34f * thickness * (0.35f + 0.65f * v), primary * 0.5f, 0.75f * a);
                    for (int sd = -1; sd <= 1; sd += 2)
                    {
                        float sx = x + sd * (0.09f + 0.04f * Rnd(i, sd + 3));
                        n = Seg(sx, Mathf.Max(groundY, bottom - 0.2f - 3.2f * v), sx, bottom + sz[i] * 0.4f, 2);
                        Draw(Taper.Head, n, 0.03f * thickness, primary, v * a);
                    }
                }

                DrawSword(x, sz[i], bottom, a);

                if (Once(1 << (i + 2)))
                {
                    Burst(x, groundY + 0.05f, 16, 5f, 4, 1.8f);
                    for (int j = 0; j < 8; j++)
                    {
                        float ang = Random.value * 3.14f, sp = 2f + 3f * Random.value;
                        Emit(_dots, x, groundY + 0.05f, Mathf.Cos(ang) * sp * (Random.value < .5f ? -1f : 1f), Mathf.Sin(ang) * sp * 0.6f, 0.06f + 0.08f * Random.value, 0.8f, Hue(primary));
                    }

                    Emit(_dots, x, groundY + 0.15f, 0f, 0f, 1.4f, 0.18f, Hue(primary));
                }

                if (p < 1f)
                {
                    // sparks thrown off the tail while it is fast
                    Emit(_sparks, x + (Random.value - 0.5f) * 0.2f, Mathf.Max(groundY, bottom), (Random.value - 0.5f) * 1.2f, -1f - 2f * Random.value, 0.06f, 0.4f, Hue(primary));
                    Emit(_stars, x, Mathf.Max(groundY, bottom) + 0.1f, 0f, 0f, 0.12f + 0.1f * Random.value, 0.4f, Hue(primary));
                }
            }

            Emit(_dots, c.x, groundY + 0.1f, 0f, 0f, 2.4f * Mathf.Clamp01(1f - _t * 2f), 0.07f, Tint(0.15f * g));
        }
    }
}
