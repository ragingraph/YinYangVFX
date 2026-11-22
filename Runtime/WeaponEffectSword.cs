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
        /// Draws one sword (tip up) rising from the ground at local x. s = total sword length in local units,
        /// k = 0..1 how far it has risen (visible height = 0.86*s*k), a = opacity.
        /// </summary>
        void DrawSword(float x, float s, float k, float a)
        {
            a *= _gm;
            if (swordMaterial == null || a <= 0.01f || k <= 0.001f)
            {
                return;
            }

            float w = s * 0.25f; // texture aspect 192:768
            float bottom = groundY - s + k * s * 0.86f;
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

        // Five solid ink swords burst out of the ground, hold, then fade. Origin = ground point.
        void AnimInkSwords()
        {
            Vector2 c = sweepCenter;
            float g = FadeFrom(1.5f);
            float[] dx = { -0.62f, -0.31f, 0f, 0.31f, 0.62f };
            float[] sz = { 1.5f, 2.0f, 2.5f, 2.1f, 1.6f };
            float[] ts = { 0.0f, 0.07f, 0.12f, 0.09f, 0.03f };
            if (Once(1)) Fire();

            float pe = EaseOut(_t / 0.35f);
            int rn;
            for (int j = 0; j < 3; j++)
            {
                float d = Mathf.Clamp01((_t - 0.08f * j) / 0.9f);
                rn = Ring(c.x, groundY, 1.9f * EaseOut(d), 0.3f * EaseOut(d));
                Draw(Taper.Flat, rn, 0.035f * (1f - d) * thickness, primary, (1f - d) * g);
            }

            for (int i = 0; i < 5; i++)
            {
                float x = c.x + dx[i];
                float k = EaseOut((_t - ts[i]) / 0.28f);
                // soft glow behind the blade (additive lines), then the solid sword on top
                float h = 0.86f * sz[i] * k;
                if (h > 0.05f)
                {
                    int n = Seg(x, groundY + h, x, groundY, 2);
                    Draw(Taper.Head, n, 0.3f * thickness, primary * 0.45f, 0.55f * g);
                }

                DrawSword(x, sz[i], k, g * Mathf.Clamp01(k * 4f));
                if (_t >= ts[i] && Once(1 << (i + 2)))
                {
                    Burst(x, groundY + 0.05f, 10, 3.5f, 3, 1.3f);
                    for (int j = 0; j < 6; j++)
                    {
                        float a = Random.value * 3.14f, sp = 1.5f + 2.5f * Random.value;
                        Emit(_dots, x, groundY + 0.05f, Mathf.Cos(a) * sp * (Random.value < .5f ? -1f : 1f), Mathf.Sin(a) * sp, 0.05f + 0.07f * Random.value, 0.9f, Hue(primary));
                    }
                }
            }

            if (_t < 1.0f)
            {
                _acc += _dt * 30f;
                while (_acc >= 1f)
                {
                    _acc -= 1f;
                    Emit(_stars, c.x + (Random.value - 0.5f) * 1.6f, groundY + 0.05f, 0f, 0.8f + 1.4f * Random.value, 0.07f + 0.06f * Random.value, 0.9f, Hue(primary));
                }
            }

            Emit(_dots, c.x, groundY + 0.8f, 0f, 0f, 3.2f * pe, 0.07f, Tint(0.1f * g));
        }
    }
}
