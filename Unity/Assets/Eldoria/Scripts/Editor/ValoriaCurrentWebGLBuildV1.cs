using System;
using System.Collections.Generic;
using System.IO;
using Eldoria.Presentation;
using Gsplat;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Rendering.Universal;

namespace Eldoria.EditorTools
{
    /// <summary>
    /// Builds the certified Valoria production runtime scene for browser playtesting.
    /// The original production scene is never regenerated or saved. For WebGL only, a temporary
    /// scene copy strips every serialized GsplatAsset reference and loads the exact external PLY variants at runtime.
    /// Region 1 is included as the already-certified dedicated 4X World scene so Mundo/Reino navigation works in-player.
    /// </summary>
    public static class ValoriaCurrentWebGLBuildV1
    {
        const string ValoriaScene = "Assets/Eldoria/ProductionSlice/Runtime/Valoria.unity";
        const string FrontierScene = "Assets/Eldoria/Scenes/Frontier.unity";
        const string TempFolder = "Assets/Eldoria/WebGLTemp";
        const string TempScene = TempFolder + "/ValoriaWebGL.unity";
        const string Output = "Builds/WebGL";
        const string RendererDataPath = "Assets/Eldoria/Content/EldoriaForwardRenderer.asset";

        [MenuItem("Eldoria/Build current Valoria production WebGL")]
        public static void Build()
        {
            Require(ValoriaScene);
            Require(FrontierScene);
            for (var i = 0; i < 4; i++)
                Require($"Assets/Eldoria/ProductionSlice/Runtime/state-{i}.asset");

            PlayerSettings.WebGL.compressionFormat = WebGLCompressionFormat.Gzip;
            PlayerSettings.WebGL.decompressionFallback = true;

            if (Directory.Exists(Output))
                Directory.Delete(Output, true);
            Directory.CreateDirectory(Output);

            if (AssetDatabase.IsValidFolder(TempFolder))
                AssetDatabase.DeleteAsset(TempFolder);
            Directory.CreateDirectory(TempFolder);
            AssetDatabase.Refresh();

            var scene = EditorSceneManager.OpenScene(ValoriaScene, OpenSceneMode.Single);
            var visual = UnityEngine.Object.FindFirstObjectByType<ValoriaParcelPresentation>();
            if (visual == null) throw new Exception("Current Valoria production presentation is missing.");
            if (visual.SceneSplats == null) throw new Exception("Current Valoria production SHARP renderer is missing.");

            // Web transport only. The certified production scene/assets stay untouched.
            // Clear the known presentation references first, then strip *all* serialized
            // GsplatAsset references from the temporary scene so no hidden/prefab reference
            // can silently drag the ~GB production assets into the WebGL player.
            foreach (var presentation in UnityEngine.Object.FindObjectsByType<ValoriaParcelPresentation>(
                         FindObjectsInactive.Include, FindObjectsSortMode.None))
                presentation.StateAssets = Array.Empty<GsplatAsset>();

            foreach (var renderer in UnityEngine.Object.FindObjectsByType<GsplatRenderer>(
                         FindObjectsInactive.Include, FindObjectsSortMode.None))
                renderer.GsplatAsset = null;

            var stripped = StripSerializedGsplatReferences(scene);
            var remappedMaterials = RemapMaterialsOutOfStateAssets(scene);

            var loader = visual.GetComponent<ValoriaWebGLSplatStateLoader>();
            if (loader == null) loader = visual.gameObject.AddComponent<ValoriaWebGLSplatStateLoader>();
            loader.Presentation = visual;
            loader.Renderer = visual.SceneSplats;
            loader.FilePrefix = "splat-state-";

            EditorUtility.SetDirty(visual);
            EditorUtility.SetDirty(loader);
            EditorUtility.SetDirty(visual.SceneSplats);

            if (!EditorSceneManager.SaveScene(scene, TempScene, true))
                throw new Exception("Could not save temporary Valoria WebGL scene.");

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            AssertNoEmbeddedGsplatDependencies();
            Debug.Log($"VALORIA_WEBGL_SPLAT_STRIP_PASS stripped_refs={stripped} remapped_materials={remappedMaterials}");

            var externalLibrary = Resources.Load<ValoriaExternalAssetLibrary>("Valoria/ExternalAssetLibrary");
            var externalBackup = PruneExternalLibraryForWebGL(externalLibrary);
            // This browser path presents certified frames, not live compute splats.
            // Do not serialize an unused package renderer into level0 or activate its GPU feature.
            var removedRenderers=0;
            foreach(var splat in UnityEngine.Object.FindObjectsByType<GsplatRenderer>(FindObjectsInactive.Include,FindObjectsSortMode.None))
            { UnityEngine.Object.DestroyImmediate(splat); removedRenderers++; }
            visual.SceneSplats=null;
            loader.Renderer=null;
            EditorSceneManager.SaveScene(scene,TempScene,true);
            AssetDatabase.SaveAssets();
            var disabledFeatures=new Dictionary<ScriptableRendererFeature,bool>();
            var forward=AssetDatabase.LoadAssetAtPath<UniversalRendererData>(RendererDataPath);
            if(forward!=null)foreach(var feature in forward.rendererFeatures)
                if(feature!=null&&feature.GetType().FullName=="Gsplat.GsplatURPFeature")
                { disabledFeatures[feature]=feature.isActive;feature.SetActive(false);EditorUtility.SetDirty(feature); }
            AssetDatabase.SaveAssets();
            Debug.Log("VALORIA_WEBGL_FRAME_SCENE_PASS stripped_renderers="+removedRenderers);

            try
            {
                var report = BuildPipeline.BuildPlayer(new BuildPlayerOptions
                {
                    scenes = new[] { TempScene, FrontierScene },
                    locationPathName = Output,
                    target = BuildTarget.WebGL,
                    options = BuildOptions.CleanBuildCache | BuildOptions.StrictMode
                });

                if (report.summary.result != BuildResult.Succeeded)
                    throw new Exception("Current Valoria + Region 1 WebGL build failed: " + report.summary.result);

                if (!File.Exists(Path.Combine(Output, "index.html")))
                    throw new Exception("Current Valoria + Region 1 WebGL build did not produce index.html.");

                Debug.Log($"VALORIA_REGION1_CURRENT_WEBGL_PASS size={report.summary.totalSize} bytes");
            }
            finally
            {
                foreach(var pair in disabledFeatures)
                { pair.Key.SetActive(pair.Value);EditorUtility.SetDirty(pair.Key); }
                AssetDatabase.SaveAssets();
                RestoreExternalLibraryAfterWebGL(externalLibrary, externalBackup);
                AssetDatabase.DeleteAsset(TempFolder);
                AssetDatabase.Refresh();
            }
        }

        static ScriptableRendererFeature EnsureGsplatUrpFeatureForWebGL(out bool added)
        {
            added = false;
            var rendererData = AssetDatabase.LoadAssetAtPath<UniversalRendererData>(RendererDataPath);
            if (rendererData == null)
                throw new Exception("Active Eldoria URP renderer data is missing: " + RendererDataPath);

            foreach (var existing in rendererData.rendererFeatures)
            {
                if (existing != null && existing.GetType().FullName == "Gsplat.GsplatURPFeature")
                {
                    Debug.Log("VALORIA_WEBGL_GSPLAT_URP_FEATURE_PASS existing=true");
                    return existing;
                }
            }

            Type featureType = null;
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                featureType = assembly.GetType("Gsplat.GsplatURPFeature", false);
                if (featureType != null) break;
            }
            if (featureType == null || !typeof(ScriptableRendererFeature).IsAssignableFrom(featureType))
                throw new Exception("UnitySplats GsplatURPFeature type is unavailable; WebGL cannot render certified SHARP beauty.");

            var feature = ScriptableObject.CreateInstance(featureType) as ScriptableRendererFeature;
            if (feature == null)
                throw new Exception("Could not instantiate UnitySplats GsplatURPFeature.");
            feature.name = "Gsplat URP Feature · WebGL transport";
            AssetDatabase.AddObjectToAsset(feature, rendererData);
            rendererData.rendererFeatures.Add(feature);
            feature.Create();
            EditorUtility.SetDirty(feature);
            EditorUtility.SetDirty(rendererData);
            AssetDatabase.SaveAssets();
            added = true;
            Debug.Log("VALORIA_WEBGL_GSPLAT_URP_FEATURE_PASS existing=false added=true");
            return feature;
        }

        static void RestoreGsplatUrpFeatureAfterWebGL(ScriptableRendererFeature feature, bool added)
        {
            if (!added || feature == null) return;
            var rendererData = AssetDatabase.LoadAssetAtPath<UniversalRendererData>(RendererDataPath);
            if (rendererData != null)
            {
                rendererData.rendererFeatures.Remove(feature);
                EditorUtility.SetDirty(rendererData);
            }
            UnityEngine.Object.DestroyImmediate(feature, true);
            AssetDatabase.SaveAssets();
            Debug.Log("VALORIA_WEBGL_GSPLAT_URP_FEATURE_RESTORE_PASS");
        }

        static Dictionary<string, UnityEngine.Object> PruneExternalLibraryForWebGL(ValoriaExternalAssetLibrary library)
        {
            var backup = new Dictionary<string, UnityEngine.Object>();
            if (library == null) return backup;

            var keep = new HashSet<string>(StringComparer.Ordinal)
            {
                "SlavicBoulder", "SlavicFlatRock", "SlavicMudFlat", "SlavicMoss"
            };
            var so = new SerializedObject(library);
            var iterator = so.GetIterator();
            while (iterator.Next(true))
            {
                if (iterator.propertyType != SerializedPropertyType.ObjectReference) continue;
                if (iterator.name == "m_Script") continue;
                backup[iterator.propertyPath] = iterator.objectReferenceValue;
                if (!keep.Contains(iterator.name))
                    iterator.objectReferenceValue = null;
            }
            so.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(library);
            AssetDatabase.SaveAssets();
            Debug.Log($"VALORIA_WEBGL_EXTERNAL_LIBRARY_PRUNE_PASS kept={keep.Count} fields={backup.Count}");
            return backup;
        }

        static void RestoreExternalLibraryAfterWebGL(ValoriaExternalAssetLibrary library,
            Dictionary<string, UnityEngine.Object> backup)
        {
            if (library == null || backup == null || backup.Count == 0) return;
            var so = new SerializedObject(library);
            foreach (var pair in backup)
            {
                var property = so.FindProperty(pair.Key);
                if (property != null && property.propertyType == SerializedPropertyType.ObjectReference)
                    property.objectReferenceValue = pair.Value;
            }
            so.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(library);
            AssetDatabase.SaveAssets();
            Debug.Log("VALORIA_WEBGL_EXTERNAL_LIBRARY_RESTORE_PASS");
        }

        static int StripSerializedGsplatReferences(Scene scene)
        {
            var stripped = 0;
            foreach (var root in scene.GetRootGameObjects())
            {
                foreach (var component in root.GetComponentsInChildren<Component>(true))
                {
                    if (component == null) continue;
                    var serialized = new SerializedObject(component);
                    var property = serialized.GetIterator();
                    var changed = false;

                    while (property.Next(true))
                    {
                        if (property.propertyType != SerializedPropertyType.ObjectReference)
                            continue;
                        if (property.objectReferenceValue is not GsplatAsset)
                            continue;

                        property.objectReferenceValue = null;
                        stripped++;
                        changed = true;
                    }

                    if (changed)
                    {
                        serialized.ApplyModifiedPropertiesWithoutUndo();
                        EditorUtility.SetDirty(component);
                    }
                }
            }
            return stripped;
        }

        static int RemapMaterialsOutOfStateAssets(Scene scene)
        {
            var remapped = 0;
            var cloned = new Dictionary<Material, Material>();

            foreach (var root in scene.GetRootGameObjects())
            {
                foreach (var renderer in root.GetComponentsInChildren<Renderer>(true))
                {
                    var mats = renderer.sharedMaterials;
                    var changed = false;
                    for (var i = 0; i < mats.Length; i++)
                    {
                        var source = mats[i];
                        if (source == null) continue;
                        var path = AssetDatabase.GetAssetPath(source);
                        if (string.IsNullOrEmpty(path)) continue;
                        if (!path.StartsWith("Assets/Eldoria/ProductionSlice/Runtime/state-", StringComparison.OrdinalIgnoreCase) ||
                            !path.EndsWith(".asset", StringComparison.OrdinalIgnoreCase))
                            continue;

                        if (!cloned.TryGetValue(source, out var copy))
                        {
                            copy = new Material(source)
                            {
                                name = source.name + "_WebGL"
                            };
                            var safeName = MakeSafeAssetName(source.name);
                            var assetPath = AssetDatabase.GenerateUniqueAssetPath(
                                TempFolder + "/Material-" + safeName + ".mat");
                            AssetDatabase.CreateAsset(copy, assetPath);
                            cloned[source] = copy;
                        }

                        mats[i] = copy;
                        changed = true;
                        remapped++;
                    }

                    if (changed)
                    {
                        renderer.sharedMaterials = mats;
                        EditorUtility.SetDirty(renderer);
                    }
                }
            }

            AssetDatabase.SaveAssets();
            return remapped;
        }

        static string MakeSafeAssetName(string value)
        {
            if (string.IsNullOrEmpty(value)) return "Unnamed";
            foreach (var c in Path.GetInvalidFileNameChars())
                value = value.Replace(c, '_');
            return value;
        }

        static void AssertNoEmbeddedGsplatDependencies()
        {
            var bad = new List<string>();
            foreach (var dependency in AssetDatabase.GetDependencies(TempScene, true))
            {
                if (dependency == TempScene) continue;
                if (AssetDatabase.LoadAssetAtPath<GsplatAsset>(dependency) != null)
                    bad.Add(dependency);
            }

            if (bad.Count > 0)
                throw new Exception(
                    "Temporary WebGL scene still embeds GsplatAsset dependencies:\n" +
                    string.Join("\n", bad));

            Debug.Log("VALORIA_WEBGL_EXTERNAL_SPLAT_DEPENDENCY_PASS");
        }

        static void Require(string path)
        {
            if (!File.Exists(path))
                throw new FileNotFoundException("Required current-production asset missing", path);
        }
    }
}

