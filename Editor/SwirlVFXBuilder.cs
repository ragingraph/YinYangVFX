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
    /// Menu:  Tools > YinYang VFX > Build Yin-Yang Swirl
    /// Creates materials, prefabs (4 colours), a bloom profile and a test scene for the yin-yang vortex.
    /// Safe to run again: existing assets are updated in place so references (GUIDs) are kept.
    /// </summary>
    public static class SwirlVFXBuilder
    {
        const string PackageRoot = "Packages/com.ragingraph.yinyangvfx";
        const string Root = "Assets/YinYangVFX/Swirl"; // generated files go here (the package folder itself is read-only)
        const string TexDir = Root + "/Textures";
        const string MatDir = Root + "/Materials";
        const string PrefabDir = Root + "/Prefabs";
        const string SceneDir = Root + "/Scenes";
        const string ProfileDir = Root + "/Settings";

        const string SwirlShaderPath = PackageRoot + "/Shaders/YinYangSwirl.shader";
        const string ParticleShaderPath = PackageRoot + "/Shaders/ParticleAdditive.shader";

        struct SwirlPreset
        {
            public string name;
            public Color yang; // light half, HDR but modest (~1.0-1.6) so it stays coloured instead of blowing out to white
            public Color yin;  // dark half, near black
            public Color rim;  // glowing ring, HDR (~3)

            public SwirlPreset(string name, Color yang, Color yin, Color rim)
            {
                this.name = name;
                this.yang = yang;
                this.yin = yin;
                this.rim = rim;
            }
        }

        static readonly SwirlPreset[] Presets =
        {
            new SwirlPreset("Classic", new Color(1.35f, 1.20f, 0.85f), new Color(0.02f, 0.02f, 0.03f), new Color(3.0f, 1.8f, 0.4f)),
            new SwirlPreset("Ice",     new Color(0.90f, 1.30f, 1.60f), new Color(0.01f, 0.02f, 0.05f), new Color(0.3f, 1.6f, 3.0f)),
            new SwirlPreset("Violet",  new Color(1.30f, 0.95f, 1.60f), new Color(0.03f, 0.00f, 0.06f), new Color(1.6f, 0.3f, 3.0f)),
            new SwirlPreset("Crimson", new Color(1.60f, 0.95f, 0.85f), new Color(0.05f, 0.00f, 0.01f), new Color(3.0f, 0.25f, 0.3f)),
        };

        [MenuItem("Tools/YinYang VFX/Build Yin-Yang Swirl")]
        public static void Build()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                EditorUtility.DisplayDialog("Yin-Yang Swirl", "Stop Play mode first, then run the build again.", "OK");
                return;
            }

            // Ask about unsaved scene changes up front, so a Cancel stops everything.
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            {
                Debug.LogWarning("Yin-Yang Swirl build cancelled (scene save prompt was cancelled). Nothing was changed.");
                return;
            }

            Shader swirlShader = AssetDatabase.LoadAssetAtPath<Shader>(SwirlShaderPath);
            Shader particleShader = AssetDatabase.LoadAssetAtPath<Shader>(ParticleShaderPath);
            if (swirlShader == null || particleShader == null)
            {
                Debug.LogError("Yin-Yang Swirl: shaders not found under " + PackageRoot + "/Shaders. Let Unity finish importing, then try again.");
                return;
            }

            SlashVFXBuilder.EnsureFolder(TexDir);
            SlashVFXBuilder.EnsureFolder(MatDir);
            SlashVFXBuilder.EnsureFolder(PrefabDir);
            SlashVFXBuilder.EnsureFolder(SceneDir);
            SlashVFXBuilder.EnsureFolder(ProfileDir);

            Sprite quad = SlashVFXBuilder.CreateWhiteSquareSprite(TexDir + "/VFX_WhiteSquare.png");
            Texture2D softDot = SlashVFXBuilder.CreateSoftDotTexture(TexDir + "/VFX_SoftDot.png");

            // Only paths are kept: object references to assets get unloaded when the new scene opens.
            string[] prefabPaths = new string[Presets.Length];
            for (int i = 0; i < Presets.Length; i++)
            {
                SwirlPreset p = Presets[i];

                Material swirlMat = SlashVFXBuilder.GetOrCreateMaterial(MatDir + "/MAT_Swirl_" + p.name + ".mat", swirlShader);
                Color yang = p.yang; yang.a = 1f;
                Color yin = p.yin; yin.a = 1f;
                Color rim = p.rim; rim.a = 1f;
                swirlMat.SetColor("_YangColor", yang);
                swirlMat.SetColor("_YinColor", yin);
                swirlMat.SetColor("_RimColor", rim);
                swirlMat.SetFloat("_Twist", 0.8f);
                swirlMat.SetFloat("_Arms", 2f);
                swirlMat.SetFloat("_SpiralTight", 1.6f);
                swirlMat.SetFloat("_ArmSpeed", 0.35f);
                swirlMat.SetFloat("_OuterRadius", 1.6f);
                swirlMat.SetFloat("_DissolveScale", 2.5f);

                // Light motes: additive glow in the rim colour.
                Material lightMat = SlashVFXBuilder.GetOrCreateMaterial(MatDir + "/MAT_SwirlLightMote_" + p.name + ".mat", particleShader);
                lightMat.SetTexture("_MainTex", softDot);
                lightMat.SetColor("_Tint", new Color(rim.r * 0.8f + 0.5f, rim.g * 0.8f + 0.5f, rim.b * 0.8f + 0.5f, 1f));
                lightMat.SetFloat("_SrcBlend", (float)BlendMode.SrcAlpha);
                lightMat.SetFloat("_DstBlend", (float)BlendMode.One);

                // Ink motes: normal alpha blending so they are really dark.
                Material inkMat = SlashVFXBuilder.GetOrCreateMaterial(MatDir + "/MAT_SwirlInkMote_" + p.name + ".mat", particleShader);
                inkMat.SetTexture("_MainTex", softDot);
                inkMat.SetColor("_Tint", new Color(yin.r, yin.g, yin.b, 1f));
                inkMat.SetFloat("_SrcBlend", (float)BlendMode.SrcAlpha);
                inkMat.SetFloat("_DstBlend", (float)BlendMode.OneMinusSrcAlpha);

                EditorUtility.SetDirty(swirlMat);
                EditorUtility.SetDirty(lightMat);
                EditorUtility.SetDirty(inkMat);

                prefabPaths[i] = PrefabDir + "/VFX_YinYangSwirl_" + p.name + ".prefab";
                BuildPrefab(prefabPaths[i], quad, swirlMat, lightMat, inkMat);
            }

            string profilePath = ProfileDir + "/VFX_Bloom_Profile.asset";
            SlashVFXBuilder.GetOrCreateBloomProfile(profilePath);
            AssetDatabase.SaveAssets(); // make sure the profile is on disk before the scene swap
            BuildTestScene(SceneDir + "/VFX_YinYangSwirl_Test.unity", prefabPaths, profilePath);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("Yin-Yang Swirl built (" + Presets.Length + " colours). Open " + SceneDir +
                      "/VFX_YinYangSwirl_Test.unity and press Play. Prefabs: " + PrefabDir + "/VFX_YinYangSwirl_<Colour>.prefab");
            EditorUtility.DisplayDialog("Yin-Yang Swirl",
                "Built " + Presets.Length + " colours.\nThe test scene VFX_YinYangSwirl_Test is now open with all colours " +
                "in a grid. Press Play to see them.", "OK");
        }

        // ------------------------------------------------------------------ prefab

        static void BuildPrefab(string path, Sprite quad, Material swirlMat, Material lightMat, Material inkMat)
        {
            GameObject root = new GameObject(Path.GetFileNameWithoutExtension(path));
            YinYangSwirl fx = root.AddComponent<YinYangSwirl>();

            // The quad covers radius 1.6 in "disc units": 3.2 world units wide, so the taiji disc has radius 1.
            GameObject discGo = new GameObject("Disc");
            discGo.transform.SetParent(root.transform, false);
            discGo.transform.localScale = new Vector3(3.2f, 3.2f, 1f);
            SpriteRenderer sr = discGo.AddComponent<SpriteRenderer>();
            sr.sprite = quad;
            sr.sharedMaterial = swirlMat;
            sr.sortingOrder = 1;

            ParticleSystem ink = CreateOrbitMotes(root.transform, "InkMotes", inkMat, 0, new Color(1f, 1f, 1f, 0.9f), 26f);
            ParticleSystem light = CreateOrbitMotes(root.transform, "LightMotes", lightMat, 2, Color.white, 34f);

            fx.disc = sr;
            fx.lightMotes = light;
            fx.inkMotes = ink;

            PrefabUtility.SaveAsPrefabAsset(root, path);
            Object.DestroyImmediate(root);
        }

        // Motes that spawn on a ring around the vortex and spiral inward while orbiting.
        static ParticleSystem CreateOrbitMotes(Transform parent, string name, Material mat, int sortingOrder,
                                               Color startColor, float rate)
        {
            GameObject go = new GameObject(name);
            go.transform.SetParent(parent, false);
            ParticleSystem ps = go.AddComponent<ParticleSystem>();

            // Adding the component starts it playing in the editor; stop and clear before configuring.
            ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

            ParticleSystem.MainModule main = ps.main;
            main.playOnAwake = false;
            main.loop = true;
            main.duration = 1f;
            main.simulationSpace = ParticleSystemSimulationSpace.Local;
            main.scalingMode = ParticleSystemScalingMode.Hierarchy; // follows the root's scale
            main.maxParticles = 200;
            main.startLifetime = new ParticleSystem.MinMaxCurve(1.0f, 1.8f);
            main.startSpeed = 0f;
            main.startSize = new ParticleSystem.MinMaxCurve(0.04f, 0.11f);
            main.startColor = startColor;
            main.gravityModifier = 0f;

            ParticleSystem.EmissionModule emission = ps.emission;
            emission.enabled = true;
            emission.rateOverTime = rate;

            ParticleSystem.ShapeModule shape = ps.shape;
            shape.enabled = true;
            shape.shapeType = ParticleSystemShapeType.Circle;
            shape.radius = 1.55f;
            shape.radiusThickness = 0.15f; // spawn near the outer edge
            shape.arc = 360f;

            // All three axes must use the same curve mode; orbital Z + a negative radial speed = inward spiral.
            ParticleSystem.VelocityOverLifetimeModule vel = ps.velocityOverLifetime;
            vel.enabled = true;
            vel.space = ParticleSystemSimulationSpace.Local;
            vel.x = new ParticleSystem.MinMaxCurve(0f);
            vel.y = new ParticleSystem.MinMaxCurve(0f);
            vel.z = new ParticleSystem.MinMaxCurve(0f);
            vel.orbitalX = new ParticleSystem.MinMaxCurve(0f);
            vel.orbitalY = new ParticleSystem.MinMaxCurve(0f);
            vel.orbitalZ = new ParticleSystem.MinMaxCurve(2.6f); // radians per second, counter-clockwise like the disc
            vel.radial = new ParticleSystem.MinMaxCurve(-0.9f);

            ParticleSystem.ColorOverLifetimeModule col = ps.colorOverLifetime;
            col.enabled = true;
            Gradient g = new Gradient();
            g.SetKeys(
                new[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(Color.white, 1f) },
                new[] { new GradientAlphaKey(0f, 0f), new GradientAlphaKey(1f, 0.2f), new GradientAlphaKey(0f, 1f) });
            col.color = g;

            ParticleSystem.SizeOverLifetimeModule size = ps.sizeOverLifetime;
            size.enabled = true;
            size.size = new ParticleSystem.MinMaxCurve(1f, new AnimationCurve(
                new Keyframe(0f, 0.6f), new Keyframe(0.3f, 1f), new Keyframe(1f, 0.1f)));

            ParticleSystemRenderer r = ps.GetComponent<ParticleSystemRenderer>();
            r.sharedMaterial = mat;
            r.sortingOrder = sortingOrder;
            r.renderMode = ParticleSystemRenderMode.Billboard;

            return ps;
        }

        // ------------------------------------------------------------------ test scene

        static void BuildTestScene(string path, string[] prefabPaths, string profilePath)
        {
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            GameObject camGo = new GameObject("Main Camera");
            camGo.tag = "MainCamera";
            camGo.transform.position = new Vector3(0f, 0f, -10f);
            Camera cam = camGo.AddComponent<Camera>();
            cam.orthographic = true;
            cam.orthographicSize = 3.6f;
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.30f, 0.31f, 0.37f, 1f); // mid grey so both the light and dark halves read
            cam.allowHDR = true;
            UniversalAdditionalCameraData camData = cam.GetUniversalAdditionalCameraData();
            camData.renderPostProcessing = true;

            GameObject volGo = new GameObject("Bloom Volume");
            Volume vol = volGo.AddComponent<Volume>();
            vol.isGlobal = true;
            vol.sharedProfile = AssetDatabase.LoadAssetAtPath<VolumeProfile>(profilePath);

            // 2 x 2 grid of looping instances.
            const int columns = 2;
            const float spacingX = 3.8f;
            const float spacingY = 3.6f;
            const float size = 0.9f;
            int rows = Mathf.CeilToInt(prefabPaths.Length / (float)columns);
            for (int i = 0; i < prefabPaths.Length; i++)
            {
                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPaths[i]);
                GameObject inst = (GameObject)PrefabUtility.InstantiatePrefab(prefab, scene);
                // Plain scene objects: values set from script below are then always saved with the scene.
                PrefabUtility.UnpackPrefabInstance(inst, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);
                int cx = i % columns;
                int cy = i / columns;
                inst.transform.position = new Vector3((cx - (columns - 1) * 0.5f) * spacingX,
                                                      ((rows - 1) * 0.5f - cy) * spacingY, 0f);
                inst.transform.localScale = Vector3.one * size;

                YinYangSwirl fx = inst.GetComponent<YinYangSwirl>();
                fx.loop = true;
                fx.playOnStart = true;
            }

            EditorSceneManager.SaveScene(scene, path);
        }
    }
}
#endif
