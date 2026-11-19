#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;

namespace YinYangVFX.EditorTools
{
    /// <summary>
    /// Menu:  Tools > YinYang VFX > Build Sword Slash
    /// Creates the textures, materials, prefab, bloom profile and a test scene for the sword slash.
    /// Safe to run again: existing assets are updated in place so references (GUIDs) are kept.
    /// </summary>
    public static class SlashVFXBuilder
    {
        const string PackageRoot = "Packages/com.ragingraph.yinyangvfx";
        const string Root = "Assets/YinYangVFX/SwordSlash"; // generated files go here (the package folder itself is read-only)
        const string TexDir = Root + "/Textures";
        const string MatDir = Root + "/Materials";
        const string PrefabDir = Root + "/Prefabs";
        const string SceneDir = Root + "/Scenes";
        const string ProfileDir = Root + "/Settings";

        const string ArcShaderPath = PackageRoot + "/Shaders/SlashArc.shader";
        const string ParticleShaderPath = PackageRoot + "/Shaders/ParticleAdditive.shader";

        [MenuItem("Tools/YinYang VFX/Build Sword Slash")]
        public static void Build()
        {
            BuildSet("Sword Slash", Presets, false, "VFX_SwordSlash_", "VFX_SwordSlash_Test",
                new Color(0.03f, 0.03f, 0.06f, 1f), VerticalShape);
        }

        [MenuItem("Tools/YinYang VFX/Build Sword Slash (Horizontal)")]
        public static void BuildHorizontal()
        {
            BuildSet("Horizontal Sword Slash", Presets, false, "VFX_SwordSlashH_", "VFX_SwordSlashH_Test",
                new Color(0.03f, 0.03f, 0.06f, 1f), HorizontalShape);
        }

        [MenuItem("Tools/YinYang VFX/Build Sword Slash (Toward Camera)")]
        public static void BuildApproach()
        {
            BuildSet("Toward-Camera Sword Slash", Presets, false, "VFX_SwordSlashZ_", "VFX_SwordSlashZ_Test",
                new Color(0.03f, 0.03f, 0.06f, 1f), ApproachShape);
        }

        [MenuItem("Tools/YinYang VFX/Build Ink Slash (Toward Camera, Dark)")]
        public static void BuildInkApproach()
        {
            BuildSet("Toward-Camera Ink Slash", InkPresets, true, "VFX_InkSlashZ_", "VFX_InkSlashZ_Test",
                new Color(0.50f, 0.52f, 0.58f, 1f), ApproachShape);
        }

        [MenuItem("Tools/YinYang VFX/Build Ink Slash (Dark)")]
        public static void BuildInk()
        {
            // Light grey background so the black blade is visible in the test scene.
            BuildSet("Ink Slash", InkPresets, true, "VFX_InkSlash_", "VFX_InkSlash_Test",
                new Color(0.50f, 0.52f, 0.58f, 1f), VerticalShape);
        }

        [MenuItem("Tools/YinYang VFX/Build Ink Slash (Horizontal, Dark)")]
        public static void BuildInkHorizontal()
        {
            BuildSet("Horizontal Ink Slash", InkPresets, true, "VFX_InkSlashH_", "VFX_InkSlashH_Test",
                new Color(0.50f, 0.52f, 0.58f, 1f), HorizontalShape);
        }

        // Geometry of the blade arc. Angles are in degrees, 0 = to the right, 90 = up.
        // The sweep always runs clockwise from the "top" end, so a centre angle of 90 gives a flat rainbow-shaped arc
        // swinging left to right (mirror = true in Spawn flips it to right to left).
        struct SlashShape
        {
            public float centerAngle;
            public float halfSpan;
            public float radius;
            public Vector2 quadSize;   // world units across at scale 1 (x wider than y flattens the circle into an ellipse)
            public float sweepDuration;
            public bool centerOnArc;   // move the arc so its visual middle sits on the object's position
            public bool approach;      // grows from small/dim to big/bright (comes toward the camera)

            public SlashShape(float centerAngle, float halfSpan, float radius, Vector2 quadSize,
                              float sweepDuration, bool centerOnArc, bool approach = false)
            {
                this.approach = approach;
                this.centerAngle = centerAngle;
                this.halfSpan = halfSpan;
                this.radius = radius;
                this.quadSize = quadSize;
                this.sweepDuration = sweepDuration;
                this.centerOnArc = centerOnArc;
            }
        }

        // Top to bottom, arc on the right side of the position (the original slash).
        static readonly SlashShape VerticalShape = new SlashShape(0f, 78f, 0.62f, new Vector2(4f, 4f), 0.26f, false);

        // Chop that comes toward the camera: starts small and dim far away, grows and speeds up, arcs over the position.
        static readonly SlashShape ApproachShape = new SlashShape(0f, 78f, 0.62f, new Vector2(4f, 4f), 0.30f, true, true);

        // Wide, flat crescent swinging left to right, centred on the position.
        static readonly SlashShape HorizontalShape = new SlashShape(90f, 70f, 0.62f, new Vector2(5f, 2.5f), 0.24f, true);

        static void BuildSet(string label, ColorPreset[] presets, bool ink, string prefabPrefix,
                             string sceneName, Color background, SlashShape shape)
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                EditorUtility.DisplayDialog(label, "Stop Play mode first, then run the build again.", "OK");
                return;
            }

            // Ask about unsaved scene changes up front, so a Cancel stops everything instead of
            // leaving new prefabs next to an old test scene.
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            {
                Debug.LogWarning(label + " build cancelled (scene save prompt was cancelled). Nothing was changed.");
                return;
            }

            Shader arcShader = AssetDatabase.LoadAssetAtPath<Shader>(ArcShaderPath);
            Shader particleShader = AssetDatabase.LoadAssetAtPath<Shader>(ParticleShaderPath);
            if (arcShader == null || particleShader == null)
            {
                Debug.LogError(label + ": shaders not found under " + PackageRoot + "/Shaders. Let Unity finish importing, then try again.");
                return;
            }

            EnsureFolder(TexDir);
            EnsureFolder(MatDir);
            EnsureFolder(PrefabDir);
            EnsureFolder(SceneDir);
            EnsureFolder(ProfileDir);

            // ---- textures
            Sprite quad = CreateWhiteSquareSprite(TexDir + "/VFX_WhiteSquare.png");
            Texture2D softDot = CreateSoftDotTexture(TexDir + "/VFX_SoftDot.png");

            // ---- one set of materials + one prefab per colour preset
            // Only paths are kept: object references to assets get unloaded when the new scene opens.
            string[] prefabPaths = new string[presets.Length];
            for (int i = 0; i < presets.Length; i++)
            {
                ColorPreset p = presets[i];
                string tag = ink ? "Ink_" + p.name : p.name;

                Material arcMain = GetOrCreateMaterial(MatDir + "/MAT_SlashArc_Main_" + tag + ".mat", arcShader);
                if (ink)
                {
                    // Ink: the wide blade body is the dark colour, the thin leading edge glows.
                    // (In the shader the "core" colour sits on the leading edge, "edge" colour on the wide body.)
                    ConfigureArcInk(arcMain, p.edge, p.core);
                }
                else
                {
                    ConfigureArcMain(arcMain, p.edge, p.core);
                }

                // For ink the additive glow behind the blade is only a faint aura.
                Material arcGlow = GetOrCreateMaterial(MatDir + "/MAT_SlashArc_Glow_" + tag + ".mat", arcShader);
                ConfigureArcGlow(arcGlow, ink ? p.edge * 0.12f : p.edge);

                // Glow: sparks run hotter (pushed toward white) than the edge; motes are the plain hue.
                // Ink: sparks are the rim colour, motes are dimmer.
                Color sparkTint = ink ? p.edge * 1.2f : p.edge * 0.8f + new Color(0.9f, 0.9f, 0.9f, 0f);
                sparkTint.a = 1f;
                Material sparkMat = GetOrCreateMaterial(MatDir + "/MAT_SlashSpark_" + tag + ".mat", particleShader);
                sparkMat.SetTexture("_MainTex", softDot);
                sparkMat.SetColor("_Tint", sparkTint);

                Color moteTint = ink ? p.edge * 0.5f : p.edge * 0.8f;
                moteTint.a = 1f;
                Material moteMat = GetOrCreateMaterial(MatDir + "/MAT_SlashMote_" + tag + ".mat", particleShader);
                moteMat.SetTexture("_MainTex", softDot);
                moteMat.SetColor("_Tint", moteTint);

                EditorUtility.SetDirty(arcMain);
                EditorUtility.SetDirty(arcGlow);
                EditorUtility.SetDirty(sparkMat);
                EditorUtility.SetDirty(moteMat);

                prefabPaths[i] = PrefabDir + "/" + prefabPrefix + p.name + ".prefab";
                BuildPrefab(prefabPaths[i], quad, arcMain, arcGlow, sparkMat, moteMat, shape);
            }

            // ---- test scene (also builds the bloom profile)
            string profilePath = ProfileDir + "/VFX_Bloom_Profile.asset";
            GetOrCreateBloomProfile(profilePath);
            AssetDatabase.SaveAssets(); // make sure the profile is on disk before the scene swap
            BuildTestScene(SceneDir + "/" + sceneName + ".unity", prefabPaths, profilePath, prefabPrefix, background);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log(label + " built (" + presets.Length + " colours). Open " + SceneDir + "/" + sceneName +
                      ".unity and press Play. Prefabs: " + PrefabDir + "/" + prefabPrefix + "<Colour>.prefab");
            EditorUtility.DisplayDialog(label,
                "Built " + presets.Length + " colours.\nThe test scene " + sceneName + " is now open with all colours " +
                "in a grid. Press Play to see them.", "OK");
        }

        // ------------------------------------------------------------------ colour presets

        // edge = saturated HDR hue of the blade rim, glow and sparks. core = the hot centre line (near white, tinted).
        // Values above 1 are intentional: they are what makes Bloom light up.
        struct ColorPreset
        {
            public string name;
            public Color edge;
            public Color core;

            public ColorPreset(string name, Color edge, Color core)
            {
                this.name = name;
                this.edge = edge;
                this.core = core;
            }
        }

        static readonly ColorPreset[] Presets =
        {
            new ColorPreset("Blue",   new Color(0.25f, 1.10f, 3.20f), new Color(3.0f, 3.0f, 3.2f)),
            new ColorPreset("Cyan",   new Color(0.10f, 2.20f, 2.80f), new Color(2.8f, 3.2f, 3.2f)),
            new ColorPreset("Green",  new Color(0.30f, 2.60f, 0.60f), new Color(2.8f, 3.2f, 2.8f)),
            new ColorPreset("Gold",   new Color(3.20f, 1.90f, 0.30f), new Color(3.2f, 3.0f, 2.4f)),
            new ColorPreset("Orange", new Color(3.40f, 1.10f, 0.15f), new Color(3.2f, 2.6f, 2.0f)),
            new ColorPreset("Red",    new Color(3.20f, 0.20f, 0.30f), new Color(3.2f, 2.0f, 2.0f)),
            new ColorPreset("Purple", new Color(1.80f, 0.35f, 3.20f), new Color(3.0f, 2.4f, 3.2f)),
            new ColorPreset("Pink",   new Color(3.20f, 0.50f, 1.80f), new Color(3.2f, 2.4f, 2.8f)),
            new ColorPreset("White",  new Color(1.60f, 1.60f, 2.00f), new Color(3.2f, 3.2f, 3.2f)),
        };

        // Ink (dark) slashes. edge = HDR colour of the glowing leading edge (keep it moderate, ~1.3-2.2, or Bloom
        // spreads it over the black body). core = the near-black body colour.
        static readonly ColorPreset[] InkPresets =
        {
            new ColorPreset("Yin",     new Color(1.30f, 1.40f, 1.80f), new Color(0.015f, 0.015f, 0.020f)),
            new ColorPreset("Violet",  new Color(1.20f, 0.20f, 2.20f), new Color(0.030f, 0.000f, 0.060f)),
            new ColorPreset("Crimson", new Color(2.20f, 0.10f, 0.18f), new Color(0.060f, 0.000f, 0.010f)),
            new ColorPreset("Jade",    new Color(0.15f, 1.90f, 0.90f), new Color(0.000f, 0.040f, 0.020f)),
        };

        // ------------------------------------------------------------------ materials

        static void ConfigureArcMain(Material m, Color edge, Color core)
        {
            core.a = 1f;
            edge.a = 1f;
            m.SetColor("_CoreColor", core);
            m.SetColor("_EdgeColor", edge);
            m.SetFloat("_Thickness", 0.22f);
            m.SetFloat("_OuterRatio", 0.18f);
            m.SetFloat("_TipSharp", 0.8f);
            m.SetFloat("_HeadTaper", 0.16f);
            m.SetFloat("_TrailPow", 1.2f);
            m.SetFloat("_CorePower", 4f);
            m.SetFloat("_InnerFalloff", 1.8f);
            m.SetVector("_NoiseScale", new Vector4(3f, 14f, 0f, 0f));
            m.SetFloat("_NoiseSpeed", 2f);
            m.SetFloat("_Distort", 0.35f);
            m.SetFloat("_Streak", 0.45f);
            m.SetFloat("_DissolveScale", 3f);
        }

        static void ConfigureArcInk(Material m, Color rim, Color body)
        {
            rim.a = 1f;
            body.a = 1f;
            m.SetColor("_CoreColor", rim);   // thin glowing leading edge
            m.SetColor("_EdgeColor", body);  // wide dark body
            m.SetFloat("_Thickness", 0.30f);
            m.SetFloat("_OuterRatio", 0.12f);
            m.SetFloat("_TipSharp", 0.8f);
            m.SetFloat("_HeadTaper", 0.16f);
            m.SetFloat("_TrailPow", 1.2f);
            m.SetFloat("_CorePower", 8f);
            m.SetFloat("_InnerFalloff", 1.0f);
            m.SetVector("_NoiseScale", new Vector4(3f, 14f, 0f, 0f));
            m.SetFloat("_NoiseSpeed", 2f);
            m.SetFloat("_Distort", 0.35f);
            m.SetFloat("_Streak", 0.45f);
            m.SetFloat("_DissolveScale", 3f);

            // Normal alpha blending so the body can be black; boost makes the dark body opaque.
            m.SetFloat("_SrcBlend", (float)UnityEngine.Rendering.BlendMode.SrcAlpha);
            m.SetFloat("_DstBlend", (float)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            m.SetFloat("_AlphaBoost", 4f);
        }

        static void ConfigureArcGlow(Material m, Color edge)
        {
            Color glowCore = edge * 0.95f;
            Color glowEdge = edge * 0.4f;
            glowCore.a = 1f;
            glowEdge.a = 1f;
            m.SetColor("_CoreColor", glowCore);
            m.SetColor("_EdgeColor", glowEdge);
            m.SetFloat("_Thickness", 0.4f);
            m.SetFloat("_OuterRatio", 0.3f);
            m.SetFloat("_TipSharp", 0.8f);
            m.SetFloat("_HeadTaper", 0.16f);
            m.SetFloat("_TrailPow", 1.2f);
            m.SetFloat("_CorePower", 1.5f);
            m.SetFloat("_InnerFalloff", 1.2f);
            m.SetVector("_NoiseScale", new Vector4(3f, 14f, 0f, 0f));
            m.SetFloat("_NoiseSpeed", 2f);
            m.SetFloat("_Distort", 0.25f);
            m.SetFloat("_Streak", 0.2f);
            m.SetFloat("_DissolveScale", 3f);
        }

        internal static Material GetOrCreateMaterial(string path, Shader shader)
        {
            Material m = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (m == null)
            {
                m = new Material(shader);
                AssetDatabase.CreateAsset(m, path);
            }
            else
            {
                m.shader = shader;
            }

            return m;
        }

        // ------------------------------------------------------------------ textures

        internal static Sprite CreateWhiteSquareSprite(string path)
        {
            const int size = 64;
            Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            Color32[] px = new Color32[size * size];
            for (int i = 0; i < px.Length; i++)
            {
                px[i] = new Color32(255, 255, 255, 255);
            }

            tex.SetPixels32(px);
            tex.Apply();
            File.WriteAllBytes(path, tex.EncodeToPNG());
            Object.DestroyImmediate(tex);

            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
            TextureImporter imp = (TextureImporter)AssetImporter.GetAtPath(path);
            imp.textureType = TextureImporterType.Sprite;
            imp.spriteImportMode = SpriteImportMode.Single;
            imp.spritePixelsPerUnit = size; // 64 px / 64 ppu = exactly 1 world unit wide
            imp.mipmapEnabled = false;
            imp.wrapMode = TextureWrapMode.Clamp;
            imp.filterMode = FilterMode.Bilinear;
            imp.textureCompression = TextureImporterCompression.Uncompressed;

            TextureImporterSettings s = new TextureImporterSettings();
            imp.ReadTextureSettings(s);
            s.spriteMeshType = SpriteMeshType.FullRect; // keeps UV exactly 0..1 for the shader
            s.spriteAlignment = (int)SpriteAlignment.Center;
            imp.SetTextureSettings(s);
            imp.SaveAndReimport();

            return AssetDatabase.LoadAssetAtPath<Sprite>(path);
        }

        internal static Texture2D CreateSoftDotTexture(string path)
        {
            const int size = 64;
            Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            Color32[] px = new Color32[size * size];
            float c = (size - 1) * 0.5f;
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float r = Mathf.Sqrt((x - c) * (x - c) + (y - c) * (y - c)) / c;
                    float a = Mathf.Pow(Mathf.Clamp01(1f - r), 2.2f);
                    px[y * size + x] = new Color32(255, 255, 255, (byte)Mathf.RoundToInt(a * 255f));
                }
            }

            tex.SetPixels32(px);
            tex.Apply();
            File.WriteAllBytes(path, tex.EncodeToPNG());
            Object.DestroyImmediate(tex);

            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
            TextureImporter imp = (TextureImporter)AssetImporter.GetAtPath(path);
            imp.textureType = TextureImporterType.Default;
            imp.alphaSource = TextureImporterAlphaSource.FromInput;
            imp.alphaIsTransparency = true;
            imp.mipmapEnabled = false;
            imp.wrapMode = TextureWrapMode.Clamp;
            imp.filterMode = FilterMode.Bilinear;
            imp.textureCompression = TextureImporterCompression.Uncompressed;
            imp.SaveAndReimport();

            return AssetDatabase.LoadAssetAtPath<Texture2D>(path);
        }

        // ------------------------------------------------------------------ prefab

        static SlashEffect BuildPrefab(string path, Sprite quad, Material arcMain, Material arcGlow,
                                       Material sparkMat, Material moteMat, SlashShape shape)
        {
            GameObject root = new GameObject(Path.GetFileNameWithoutExtension(path));
            SlashEffect fx = root.AddComponent<SlashEffect>();

            Vector3 arcScale = new Vector3(shape.quadSize.x, shape.quadSize.y, 1f);
            Vector3 arcOffset = shape.centerOnArc ? ArcCenterOffset(shape) : Vector3.zero;

            GameObject glowGo = new GameObject("ArcGlow");
            glowGo.transform.SetParent(root.transform, false);
            glowGo.transform.localPosition = arcOffset;
            glowGo.transform.localScale = arcScale;
            SpriteRenderer glowSr = glowGo.AddComponent<SpriteRenderer>();
            glowSr.sprite = quad;
            glowSr.sharedMaterial = arcGlow;
            glowSr.sortingOrder = 0;

            GameObject mainGo = new GameObject("ArcMain");
            mainGo.transform.SetParent(root.transform, false);
            mainGo.transform.localPosition = arcOffset;
            mainGo.transform.localScale = arcScale;
            SpriteRenderer mainSr = mainGo.AddComponent<SpriteRenderer>();
            mainSr.sprite = quad;
            mainSr.sharedMaterial = arcMain;
            mainSr.sortingOrder = 1;

            ParticleSystem sparks = CreateParticleChild(root.transform, "Sparks", sparkMat, 2, true);
            ParticleSystem motes = CreateParticleChild(root.transform, "Motes", moteMat, 1, false);

            fx.arcCenterAngle = shape.centerAngle;
            fx.arcHalfSpan = shape.halfSpan;
            fx.arcRadius = shape.radius;
            fx.sweepDuration = shape.sweepDuration;
            if (shape.approach)
            {
                fx.approach = true;
                fx.startScale = 0.3f;
                fx.endScale = 1.3f;
                fx.startAlpha = 0.35f;
                fx.totalDuration = 0.6f;
                fx.impactTime = 0.22f;
                // Ease-in: the blade speeds up as it comes closer.
                fx.sweepCurve = new AnimationCurve(
                    new Keyframe(0f, 0f, 0f, 0.3f),
                    new Keyframe(1f, 1f, 2.4f, 0f));
            }
            fx.arcMain = mainSr;
            fx.arcGlow = glowSr;
            fx.sparks = sparks;
            fx.motes = motes;

            // Leave the instance disabled in the prefab? No: keep it active so it can be dragged into a scene
            // for previewing. Spawn() always re-activates it anyway.
            GameObject saved = PrefabUtility.SaveAsPrefabAsset(root, path);
            Object.DestroyImmediate(root);
            return saved.GetComponent<SlashEffect>();
        }

        // Local offset that puts the middle of the visible arc (not the middle of its circle) on the object's origin.
        static Vector3 ArcCenterOffset(SlashShape shape)
        {
            float minX = float.MaxValue, maxX = float.MinValue, minY = float.MaxValue, maxY = float.MinValue;
            for (int i = 0; i <= 8; i++)
            {
                float a = (shape.centerAngle - shape.halfSpan + 2f * shape.halfSpan * i / 8f) * Mathf.Deg2Rad;
                float x = Mathf.Cos(a) * shape.radius;
                float y = Mathf.Sin(a) * shape.radius;
                minX = Mathf.Min(minX, x);
                maxX = Mathf.Max(maxX, x);
                minY = Mathf.Min(minY, y);
                maxY = Mathf.Max(maxY, y);
            }

            // Disc units (-1..1 across the quad) -> local units: the quad is 1 unit wide at scale 1, so 0.5 * size per disc unit.
            return new Vector3(-(minX + maxX) * 0.25f * shape.quadSize.x, -(minY + maxY) * 0.25f * shape.quadSize.y, 0f);
        }

        static ParticleSystem CreateParticleChild(Transform parent, string name, Material mat, int sortingOrder, bool streak)
        {
            GameObject go = new GameObject(name);
            go.transform.SetParent(parent, false);
            ParticleSystem ps = go.AddComponent<ParticleSystem>();

            // Adding the component starts it playing in the editor; stop and clear before configuring.
            ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

            ParticleSystem.MainModule main = ps.main;
            main.playOnAwake = false;
            main.loop = true; // stays alive so Emit() from script always simulates; SlashEffect stops it itself
            main.duration = 1f;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.maxParticles = 300;
            main.startLifetime = 0.5f;
            main.startSpeed = 0f;
            main.startSize = 0.1f;
            main.startColor = streak ? Color.white : new Color(1f, 1f, 1f, 0.8f); // hue comes from the material tint
            main.gravityModifier = 0f;

            ParticleSystem.EmissionModule emission = ps.emission;
            emission.enabled = false;

            ParticleSystem.ShapeModule shape = ps.shape;
            shape.enabled = false;

            ParticleSystem.ColorOverLifetimeModule col = ps.colorOverLifetime;
            col.enabled = true;
            Gradient g = new Gradient();
            g.SetKeys(
                new[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(Color.white, 1f) },
                new[] { new GradientAlphaKey(0f, 0f), new GradientAlphaKey(1f, 0.08f), new GradientAlphaKey(0f, 1f) });
            col.color = g;

            ParticleSystem.SizeOverLifetimeModule size = ps.sizeOverLifetime;
            size.enabled = true;
            size.size = new ParticleSystem.MinMaxCurve(1f, new AnimationCurve(
                new Keyframe(0f, 1f, 0f, -0.4f), new Keyframe(1f, 0f, -1.6f, 0f)));

            ParticleSystem.LimitVelocityOverLifetimeModule drag = ps.limitVelocityOverLifetime;
            drag.enabled = true;
            drag.dampen = streak ? 0.08f : 0.03f;
            drag.limit = 100f;

            ParticleSystemRenderer r = ps.GetComponent<ParticleSystemRenderer>();
            r.sharedMaterial = mat;
            r.sortingOrder = sortingOrder;
            if (streak)
            {
                r.renderMode = ParticleSystemRenderMode.Stretch;
                r.velocityScale = 0.1f;
                r.lengthScale = 2.2f;
            }
            else
            {
                r.renderMode = ParticleSystemRenderMode.Billboard;
            }

            return ps;
        }

        // ------------------------------------------------------------------ bloom + scene

        internal static VolumeProfile GetOrCreateBloomProfile(string path)
        {
            VolumeProfile profile = AssetDatabase.LoadAssetAtPath<VolumeProfile>(path);
            if (profile == null)
            {
                profile = ScriptableObject.CreateInstance<VolumeProfile>();
                AssetDatabase.CreateAsset(profile, path);
            }

            if (!profile.TryGet(out Bloom bloom))
            {
                bloom = profile.Add<Bloom>(true);
                AssetDatabase.AddObjectToAsset(bloom, profile);
            }

            bloom.active = true;
            bloom.threshold.Override(0.9f);
            bloom.intensity.Override(0.9f);
            bloom.scatter.Override(0.6f);
            bloom.tint.Override(Color.white);
            bloom.highQualityFiltering.Override(false); // cheaper on phones

            EditorUtility.SetDirty(bloom);
            EditorUtility.SetDirty(profile);
            return profile;
        }

        static void BuildTestScene(string path, string[] prefabPaths, string profilePath,
                                   string prefabPrefix, Color background)
        {
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            GameObject camGo = new GameObject("Main Camera");
            camGo.tag = "MainCamera";
            camGo.transform.position = new Vector3(0f, 0f, -10f);
            Camera cam = camGo.AddComponent<Camera>();
            cam.orthographic = true;
            cam.orthographicSize = 3.5f;
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = background;
            cam.allowHDR = true;
            UniversalAdditionalCameraData camData = cam.GetUniversalAdditionalCameraData();
            camData.renderPostProcessing = true;

            GameObject volGo = new GameObject("Bloom Volume");
            Volume vol = volGo.AddComponent<Volume>();
            vol.isGlobal = true;
            vol.sharedProfile = AssetDatabase.LoadAssetAtPath<VolumeProfile>(profilePath);

            // 3 x 3 grid, every colour slashing at once (staggered a little so they do not all peak together).
            const int columns = 3;
            const float spacingX = 3.6f;
            const float spacingY = 2.2f;
            int rows = Mathf.CeilToInt(prefabPaths.Length / (float)columns);
            for (int i = 0; i < prefabPaths.Length; i++)
            {
                // Load fresh after the new scene is open (earlier references may have been unloaded).
                SlashEffect prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPaths[i]).GetComponent<SlashEffect>();
                int cx = i % columns;
                int cy = i / columns;
                GameObject demoGo = new GameObject("Slash Demo " + Path.GetFileNameWithoutExtension(prefabPaths[i]).Replace(prefabPrefix, ""));
                demoGo.transform.position = new Vector3((cx - (columns - 1) * 0.5f) * spacingX,
                                                        ((rows - 1) * 0.5f - cy) * spacingY, 0f);
                SlashEffectDemo demo = demoGo.AddComponent<SlashEffectDemo>();
                demo.prefab = prefab;
                demo.sizeMultiplier = 0.5f;
                demo.interval = 1.6f;
                demo.startDelay = 0.3f + 0.12f * i;
            }

            EditorSceneManager.SaveScene(scene, path);
        }

        // ------------------------------------------------------------------ util

        internal static void EnsureFolder(string path)
        {
            if (AssetDatabase.IsValidFolder(path))
            {
                return;
            }

            string parent = Path.GetDirectoryName(path).Replace('\\', '/');
            string leaf = Path.GetFileName(path);
            if (!AssetDatabase.IsValidFolder(parent))
            {
                EnsureFolder(parent);
            }

            AssetDatabase.CreateFolder(parent, leaf);
        }
    }
}
#endif
