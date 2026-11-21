#if UNITY_EDITOR
using System;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace YinYangVFX.EditorTools
{
    /// <summary>
    /// Menu: Tools > YinYang VFX > Weapons > ...
    /// Builds textures, materials, 6 colour prefabs and a test scene for each Chinese-weapon effect.
    /// Safe to run again: existing assets are updated in place so references (GUIDs) are kept.
    /// </summary>
    public static class WeaponVFXBuilder
    {
        const string PackageRoot = "Packages/com.ragingraph.yinyangvfx";
        const string Root = "Assets/YinYangVFX/Weapons";
        const string TexDir = Root + "/Textures";
        const string MatDir = Root + "/Materials";
        const string PrefabDir = Root + "/Prefabs";
        const string SceneDir = Root + "/Scenes";
        const string ProfileDir = Root + "/Settings";
        const string ParticleShaderPath = PackageRoot + "/Shaders/ParticleAdditive.shader";

        struct Palette
        {
            public string name;
            public Color primary, secondary, tertiary;

            public Palette(string name, Color p, Color s, Color t)
            {
                this.name = name;
                primary = p;
                secondary = s;
                tertiary = t;
            }
        }

        // One colour per prefab: all three layers use the same hue (the hot core still glows near white).
        static Palette One(string name, Color c)
        {
            return new Palette(name, c, c, c);
        }

        static readonly Palette[] Palettes =
        {
            One("Ice", new Color(0.25f, 1.0f, 3.0f)),
            One("Crimson", new Color(3.0f, 0.25f, 0.2f)),
            One("Jade", new Color(0.2f, 3.0f, 0.8f)),
            One("Gold", new Color(3.0f, 1.5f, 0.15f)),
            One("Violet", new Color(1.6f, 0.4f, 3.0f)),
            One("Azure", new Color(0.1f, 0.5f, 3.0f)),
        };

        struct KindInfo
        {
            public WeaponKind kind;
            public string label;
            public string id;
            public float duration;
            public float cellWidth;
            public float originShift; // moves the spawn point left of the cell centre for effects that run to the right

            public KindInfo(WeaponKind kind, string label, string id, float duration, float cellWidth, float originShift)
            {
                this.kind = kind;
                this.label = label;
                this.id = id;
                this.duration = duration;
                this.cellWidth = cellWidth;
                this.originShift = originShift;
            }
        }

        static readonly KindInfo[] Kinds =
        {
            new KindInfo(WeaponKind.SpearThrust, "Spear Thrust", "SpearThrust", 0.7f, 4.2f, 1.1f),
            new KindInfo(WeaponKind.SpearSweep, "Spear Sweep", "SpearSweep", 0.8f, 3.4f, 0f),
            new KindInfo(WeaponKind.HammerSmash, "Hammer Smash", "HammerSmash", 1.2f, 4.2f, 0.9f),
            new KindInfo(WeaponKind.SwordQi, "Sword Qi", "SwordQi", 1.3f, 4.6f, 1.6f),
            new KindInfo(WeaponKind.CrossSlash, "Cross Slash (X)", "CrossSlash", 0.95f, 3.0f, 0f),
            new KindInfo(WeaponKind.SpearRising, "Spear Rising", "SpearRising", 0.7f, 4.0f, 0.8f),
            new KindInfo(WeaponKind.SpearFlurry, "Spear Flurry", "SpearFlurry", 0.9f, 4.2f, 1.1f),
            new KindInfo(WeaponKind.HammerSweep, "Hammer Sweep", "HammerSweep", 1.0f, 4.6f, 1.0f),
            new KindInfo(WeaponKind.HammerQuake, "Hammer Quake", "HammerQuake", 1.6f, 4.6f, 1.0f),
            new KindInfo(WeaponKind.SwordSlash, "Sword Slash", "SwordSlash", 0.55f, 3.2f, 0.6f),
            new KindInfo(WeaponKind.SwordWhirl, "Sword Whirl", "SwordWhirl", 1.0f, 3.4f, 0.0f),
            new KindInfo(WeaponKind.SwordLunge, "Sword Lunge", "SwordLunge", 0.5f, 3.4f, 0.9f),
            new KindInfo(WeaponKind.BladeChop, "Blade Chop", "BladeChop", 0.8f, 4.4f, 1.0f),
            new KindInfo(WeaponKind.BladeDrag, "Blade Drag", "BladeDrag", 1.0f, 4.0f, 0.9f),
            new KindInfo(WeaponKind.HalberdCrescent, "Halberd Crescent", "HalberdCrescent", 0.7f, 4.0f, 0.8f),
            new KindInfo(WeaponKind.HalberdLiftThrust, "Halberd Lift-Thrust", "HalberdLiftThrust", 0.9f, 4.0f, 1.0f),
            new KindInfo(WeaponKind.WhipLash, "Whip Lash", "WhipLash", 0.9f, 4.4f, 1.0f),
            new KindInfo(WeaponKind.NunchakuFlurry, "Nunchaku Flurry", "NunchakuFlurry", 1.0f, 3.6f, 0.7f),
            new KindInfo(WeaponKind.PalmWave, "Palm Wave", "PalmWave", 0.8f, 4.6f, 0.7f),
            new KindInfo(WeaponKind.Dart, "Dart Throw", "Dart", 0.9f, 4.8f, 1.2f),
            new KindInfo(WeaponKind.ArrowShot, "Arrow Shot", "ArrowShot", 1.0f, 5.0f, 1.4f),
        };

        // ------------------------------------------------------------------ menu

        [MenuItem("Tools/YinYang VFX/Weapons/Build Spear Thrust")]
        public static void BuildThrust() { Run(WeaponKind.SpearThrust); }

        [MenuItem("Tools/YinYang VFX/Weapons/Build Spear Sweep")]
        public static void BuildSweep() { Run(WeaponKind.SpearSweep); }

        [MenuItem("Tools/YinYang VFX/Weapons/Build Hammer Smash")]
        public static void BuildHammer() { Run(WeaponKind.HammerSmash); }

        [MenuItem("Tools/YinYang VFX/Weapons/Build Sword Qi")]
        public static void BuildQi() { Run(WeaponKind.SwordQi); }

        [MenuItem("Tools/YinYang VFX/Weapons/Build Cross Slash (X)")]
        public static void BuildCross() { Run(WeaponKind.CrossSlash); }

        [MenuItem("Tools/YinYang VFX/Weapons/Build Spear Rising", false, 11)]
        public static void BuildSpearRising() { Run(WeaponKind.SpearRising); }

        [MenuItem("Tools/YinYang VFX/Weapons/Build Spear Flurry", false, 12)]
        public static void BuildSpearFlurry() { Run(WeaponKind.SpearFlurry); }

        [MenuItem("Tools/YinYang VFX/Weapons/Build Hammer Sweep", false, 13)]
        public static void BuildHammerSweep() { Run(WeaponKind.HammerSweep); }

        [MenuItem("Tools/YinYang VFX/Weapons/Build Hammer Quake", false, 14)]
        public static void BuildHammerQuake() { Run(WeaponKind.HammerQuake); }

        [MenuItem("Tools/YinYang VFX/Weapons/Build Sword Slash", false, 15)]
        public static void BuildSwordSlash() { Run(WeaponKind.SwordSlash); }

        [MenuItem("Tools/YinYang VFX/Weapons/Build Sword Whirl", false, 16)]
        public static void BuildSwordWhirl() { Run(WeaponKind.SwordWhirl); }

        [MenuItem("Tools/YinYang VFX/Weapons/Build Sword Lunge", false, 17)]
        public static void BuildSwordLunge() { Run(WeaponKind.SwordLunge); }

        [MenuItem("Tools/YinYang VFX/Weapons/Build Blade Chop", false, 18)]
        public static void BuildBladeChop() { Run(WeaponKind.BladeChop); }

        [MenuItem("Tools/YinYang VFX/Weapons/Build Blade Drag", false, 19)]
        public static void BuildBladeDrag() { Run(WeaponKind.BladeDrag); }

        [MenuItem("Tools/YinYang VFX/Weapons/Build Halberd Crescent", false, 20)]
        public static void BuildHalberdCrescent() { Run(WeaponKind.HalberdCrescent); }

        [MenuItem("Tools/YinYang VFX/Weapons/Build Halberd Lift-Thrust", false, 21)]
        public static void BuildHalberdLiftThrust() { Run(WeaponKind.HalberdLiftThrust); }

        [MenuItem("Tools/YinYang VFX/Weapons/Build Whip Lash", false, 22)]
        public static void BuildWhipLash() { Run(WeaponKind.WhipLash); }

        [MenuItem("Tools/YinYang VFX/Weapons/Build Nunchaku Flurry", false, 23)]
        public static void BuildNunchakuFlurry() { Run(WeaponKind.NunchakuFlurry); }

        [MenuItem("Tools/YinYang VFX/Weapons/Build Palm Wave", false, 24)]
        public static void BuildPalmWave() { Run(WeaponKind.PalmWave); }

        [MenuItem("Tools/YinYang VFX/Weapons/Build Dart Throw", false, 25)]
        public static void BuildDart() { Run(WeaponKind.Dart); }

        [MenuItem("Tools/YinYang VFX/Weapons/Build Arrow Shot", false, 26)]
        public static void BuildArrowShot() { Run(WeaponKind.ArrowShot); }

        [MenuItem("Tools/YinYang VFX/Weapons/Build ALL Weapon Effects", false, 100)]
        public static void BuildAll()
        {
            WeaponKind[] all = (WeaponKind[])Enum.GetValues(typeof(WeaponKind));
            Run(all);
        }

        // ------------------------------------------------------------------ build

        static void Run(params WeaponKind[] kinds)
        {
            const string title = "Weapon VFX";
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                EditorUtility.DisplayDialog(title, "Stop Play mode first, then run the build again.", "OK");
                return;
            }

            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            {
                Debug.LogWarning(title + " build cancelled (scene save prompt was cancelled). Nothing was changed.");
                return;
            }

            Shader particleShader = AssetDatabase.LoadAssetAtPath<Shader>(ParticleShaderPath);
            if (particleShader == null)
            {
                Debug.LogError(title + ": shader not found at " + ParticleShaderPath + ". Let Unity finish importing, then try again.");
                return;
            }

            SlashVFXBuilder.EnsureFolder(TexDir);
            SlashVFXBuilder.EnsureFolder(MatDir);
            SlashVFXBuilder.EnsureFolder(PrefabDir);
            SlashVFXBuilder.EnsureFolder(SceneDir);
            SlashVFXBuilder.EnsureFolder(ProfileDir);

            // textures + shared materials (paths only are kept across scene swaps)
            WriteTexture(TexDir + "/VFX_SoftLine.png", 8, 64, SoftLinePixel);
            WriteTexture(TexDir + "/VFX_Star.png", 64, 64, StarPixel);
            Texture2D softDot = SlashVFXBuilder.CreateSoftDotTexture(TexDir + "/VFX_SoftDot.png");
            Texture2D softLine = AssetDatabase.LoadAssetAtPath<Texture2D>(TexDir + "/VFX_SoftLine.png");
            Texture2D star = AssetDatabase.LoadAssetAtPath<Texture2D>(TexDir + "/VFX_Star.png");

            Material lineMat = MakeMaterial(MatDir + "/MAT_WeaponLine.mat", particleShader, softLine, new Color(2f, 2f, 2f, 1f));
            Material dotMat = MakeMaterial(MatDir + "/MAT_WeaponDot.mat", particleShader, softDot, new Color(2.6f, 2.6f, 2.6f, 1f));
            Material starMat = MakeMaterial(MatDir + "/MAT_WeaponStar.mat", particleShader, star, new Color(3.2f, 3.2f, 3.2f, 1f));
            string lineMatPath = AssetDatabase.GetAssetPath(lineMat);
            string dotMatPath = AssetDatabase.GetAssetPath(dotMat);
            string starMatPath = AssetDatabase.GetAssetPath(starMat);

            string profilePath = ProfileDir + "/VFX_Bloom_Profile.asset";
            SlashVFXBuilder.GetOrCreateBloomProfile(profilePath);
            string squarePath = TexDir + "/VFX_WhiteSquare.png";
            SlashVFXBuilder.CreateWhiteSquareSprite(squarePath);
            AssetDatabase.SaveAssets();

            string lastScene = null;
            foreach (WeaponKind kind in kinds)
            {
                KindInfo info = Kinds[(int)kind];
                string[] prefabPaths = new string[Palettes.Length];
                for (int i = 0; i < Palettes.Length; i++)
                {
                    prefabPaths[i] = PrefabDir + "/VFX_" + info.id + "_" + Palettes[i].name + ".prefab";
                    BuildPrefab(prefabPaths[i], info, Palettes[i], lineMatPath, dotMatPath, starMatPath);
                }

                AssetDatabase.SaveAssets();
                lastScene = SceneDir + "/VFX_" + info.id + "_Test.unity";
                BuildTestScene(lastScene, prefabPaths, profilePath, squarePath, info);
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            string names = "";
            foreach (WeaponKind k in kinds)
            {
                names += (names.Length > 0 ? ", " : "") + Kinds[(int)k].label;
            }

            Debug.Log(title + " built: " + names + ". Prefabs: " + PrefabDir + "/VFX_<Effect>_<Colour>.prefab");
            EditorUtility.DisplayDialog(title,
                "Built: " + names + "\n6 colours each.\nThe test scene (" + Path.GetFileName(lastScene) +
                ") is now open. Press Play to see them.", "OK");
        }

        static void BuildPrefab(string path, KindInfo info, Palette pal, string lineMatPath, string dotMatPath, string starMatPath)
        {
            GameObject root = new GameObject(Path.GetFileNameWithoutExtension(path));
            WeaponEffect fx = root.AddComponent<WeaponEffect>();
            fx.kind = info.kind;
            fx.duration = info.duration;
            fx.primary = pal.primary;
            fx.secondary = pal.secondary;
            fx.tertiary = pal.tertiary;
            fx.lineMaterial = AssetDatabase.LoadAssetAtPath<Material>(lineMatPath);
            fx.dotMaterial = AssetDatabase.LoadAssetAtPath<Material>(dotMatPath);
            fx.starMaterial = AssetDatabase.LoadAssetAtPath<Material>(starMatPath);

            PrefabUtility.SaveAsPrefabAsset(root, path);
            UnityEngine.Object.DestroyImmediate(root);
        }

        static void BuildTestScene(string path, string[] prefabPaths, string profilePath, string squarePath, KindInfo info)
        {
            UnityEngine.SceneManagement.Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            GameObject camGo = new GameObject("Main Camera");
            camGo.tag = "MainCamera";
            camGo.transform.position = new Vector3(0f, 0f, -10f);
            Camera cam = camGo.AddComponent<Camera>();
            cam.orthographic = true;
            cam.orthographicSize = Mathf.Max(3.7f, 3f * info.cellWidth * 0.5f / 1.7f);
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.035f, 0.04f, 0.075f, 1f);
            cam.allowHDR = true;
            cam.GetUniversalAdditionalCameraData().renderPostProcessing = true;

            GameObject volGo = new GameObject("Bloom Volume");
            Volume vol = volGo.AddComponent<Volume>();
            vol.isGlobal = true;
            vol.sharedProfile = AssetDatabase.LoadAssetAtPath<VolumeProfile>(profilePath);

            Sprite square = AssetDatabase.LoadAssetAtPath<Sprite>(squarePath);
            const int columns = 3;
            const float spacingY = 3.1f;
            int rows = Mathf.CeilToInt(prefabPaths.Length / (float)columns);
            for (int i = 0; i < prefabPaths.Length; i++)
            {
                WeaponEffect prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPaths[i]).GetComponent<WeaponEffect>();
                int cx = i % columns;
                int cy = i / columns;
                float cellX = (cx - (columns - 1) * 0.5f) * info.cellWidth;
                float cellY = ((rows - 1) * 0.5f - cy) * spacingY;

                // a dim stand-in "character" and ground line so the scale and direction are readable
                if (info.kind != WeaponKind.CrossSlash)
                {
                    MakeBlock("Dummy", square, new Vector3(cellX - info.originShift, cellY - 0.2f, 0f), new Vector3(0.14f, 1.0f, 1f), new Color(0.22f, 0.24f, 0.34f));
                }

                MakeBlock("Ground", square, new Vector3(cellX, cellY - 0.68f - 0.02f, 0f), new Vector3(info.cellWidth * 0.95f, 0.025f, 1f), new Color(0.16f, 0.17f, 0.25f));

                GameObject demoGo = new GameObject("Demo " + Path.GetFileNameWithoutExtension(prefabPaths[i]));
                demoGo.transform.position = new Vector3(cellX - info.originShift, cellY, 0f);
                WeaponEffectDemo demo = demoGo.AddComponent<WeaponEffectDemo>();
                demo.prefab = prefab;
                demo.interval = info.duration + 0.9f;
                demo.startDelay = 0.4f + 0.15f * i;
            }

            EditorSceneManager.SaveScene(scene, path);
        }

        static void MakeBlock(string name, Sprite sprite, Vector3 pos, Vector3 scale, Color color)
        {
            GameObject go = new GameObject(name);
            go.transform.position = pos;
            go.transform.localScale = scale;
            SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = sprite;
            sr.color = color;
            sr.sortingOrder = 0;
        }

        // ------------------------------------------------------------------ textures / materials

        static Material MakeMaterial(string path, Shader shader, Texture2D tex, Color tint)
        {
            Material m = SlashVFXBuilder.GetOrCreateMaterial(path, shader);
            m.SetTexture("_MainTex", tex);
            m.SetColor("_Tint", tint);
            m.SetFloat("_SrcBlend", 5f); // SrcAlpha
            m.SetFloat("_DstBlend", 1f); // One -> additive glow
            EditorUtility.SetDirty(m);
            return m;
        }

        // Soft line cross-section: bright in the middle, fading to the edges.
        static Color32 SoftLinePixel(int x, int y, int w, int h)
        {
            float v = Mathf.Abs(y / (float)(h - 1) * 2f - 1f);
            float a = Mathf.Pow(Mathf.Clamp01(1f - v), 1.5f);
            return new Color32(255, 255, 255, (byte)Mathf.RoundToInt(a * 255f));
        }

        // Four-point sparkle.
        static Color32 StarPixel(int x, int y, int w, int h)
        {
            float u = (x / (float)(w - 1)) * 2f - 1f;
            float v = (y / (float)(h - 1)) * 2f - 1f;
            float au = Mathf.Abs(u);
            float av = Mathf.Abs(v);
            float spikeH = Mathf.Exp(-av * 24f) * Mathf.Pow(Mathf.Clamp01(1f - au), 2f);
            float spikeV = Mathf.Exp(-au * 24f) * Mathf.Pow(Mathf.Clamp01(1f - av), 2f);
            float core = Mathf.Pow(Mathf.Clamp01(1f - Mathf.Sqrt(u * u + v * v) * 1.6f), 2f);
            float a = Mathf.Clamp01(Mathf.Max(spikeH, spikeV) + core);
            return new Color32(255, 255, 255, (byte)Mathf.RoundToInt(a * 255f));
        }

        static void WriteTexture(string path, int w, int h, Func<int, int, int, int, Color32> pixel)
        {
            Texture2D tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
            Color32[] px = new Color32[w * h];
            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    px[y * w + x] = pixel(x, y, w, h);
                }
            }

            tex.SetPixels32(px);
            tex.Apply();
            File.WriteAllBytes(path, tex.EncodeToPNG());
            UnityEngine.Object.DestroyImmediate(tex);

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
        }
    }
}
#endif
