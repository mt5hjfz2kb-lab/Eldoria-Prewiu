using System;
using System.IO;
using System.Linq;
using Eldoria.UI.RoyalGold;
using Eldoria.UI.RoyalGold.Preview;
using Eldoria.UI.RoyalGold.V2;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Eldoria.UI.RoyalGold.V2.Editor
{
    /// <summary>
    /// Manual, isolated asset creation. Does not run automatically, does not
    /// touch main's playable scene and never dispatches/publishes a build.
    /// Use only after checking the shared runner/editor lease.
    /// </summary>
    public static class RoyalGoldV2PrefabBuilder
    {
        private const string SourceRoot = "Assets/Eldoria/UI/ValoriaRoyalHUDV1";
        private const string CandidateRoot = "Assets/Eldoria/UI/ValoriaRoyalHUDV2";
        private const string SourcePrefab = SourceRoot + "/Prefabs/RoyalGoldHUD.prefab";
        private const string TargetPrefab = CandidateRoot + "/Prefabs/RoyalGoldHUDV2.prefab";
        private const string SourcePreview = SourceRoot + "/Preview/RoyalGold_UI_TEST.unity";
        private const string TargetPreview = CandidateRoot + "/Preview/RoyalGold_V2_ISOLATED.unity";

        [MenuItem("Eldoria/Royal Gold V2/Create isolated reusable prefab")]
        public static void CreatePrefab()
        {
            var original = AssetDatabase.LoadAssetAtPath<GameObject>(SourcePrefab);
            if (!original) throw new InvalidOperationException("Original Royal Gold prefab is unavailable.");
            var root = PrefabUtility.LoadPrefabContents(SourcePrefab);
            try
            {
                var hud = root.GetComponentInChildren<RoyalHUD>(true);
                if (!hud) throw new InvalidOperationException("The source has no RoyalHUD.");
                if (root.GetComponentsInChildren<RoyalHUD>(true).Length != 1)
                    throw new InvalidOperationException("Expected exactly one session HUD.");
                var v2 = hud.GetComponent<RoyalGoldV2Hud>();
                if (!v2) v2 = hud.gameObject.AddComponent<RoyalGoldV2Hud>();
                v2.Source = hud;
                var story = hud.GetComponent<RoyalGoldV2Narrative>();
                if (!story) story = hud.gameObject.AddComponent<RoyalGoldV2Narrative>();
                story.Source = hud;
                root.name = "RoyalGoldHUDV2";
                if (root.GetComponentsInChildren<Camera>(true).Length > 0 ||
                    root.GetComponentsInChildren<UnityEngine.EventSystems.EventSystem>(true).Length > 0)
                    throw new InvalidOperationException("Reusable HUD must not contain cameras or event systems.");
                Directory.CreateDirectory(Path.GetDirectoryName(TargetPrefab) ?? throw new InvalidOperationException());
                if (!PrefabUtility.SaveAsPrefabAsset(root, TargetPrefab))
                    throw new InvalidOperationException("Unable to save candidate V2 prefab.");
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(root);
            }
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("ELDORIA_ROYAL_GOLD_V2_PREFAB_CREATED_ISOLATED " + TargetPrefab);
        }

        [MenuItem("Eldoria/Royal Gold V2/Create isolated fixture scene")]
        public static void CreateIsolatedPreview()
        {
            CreatePrefab();
            // Work only in an additive copy of the V1 TEST scene. The input
            // scene and current editing scene remain unchanged on disk.
            Scene scene = default;
            try
            {
                scene = EditorSceneManager.OpenScene(SourcePreview, OpenSceneMode.Additive);
                var existing = scene.GetRootGameObjects()
                    .SelectMany(x => x.GetComponentsInChildren<RoyalHUD>(true)).Single();
                var driver = scene.GetRootGameObjects()
                    .SelectMany(x => x.GetComponentsInChildren<RoyalPreviewDriver>(true)).Single();
                // The legacy TEST fixture keeps its driver on the same
                // GameObject as RoyalHUD. Replacing that object would destroy
                // the fixture itself. Extend the additive TEST copy in place.
                var candidate = existing.GetComponent<RoyalGoldV2Hud>();
                if (!candidate) candidate = existing.gameObject.AddComponent<RoyalGoldV2Hud>();
                candidate.Source = existing;
                var story = existing.GetComponent<RoyalGoldV2Narrative>();
                if (!story) story = existing.gameObject.AddComponent<RoyalGoldV2Narrative>();
                story.Source = existing;
                existing.gameObject.name = "RoyalGoldHUDV2 ISOLATED TEST";
                driver.Hud = existing;
                if (scene.GetRootGameObjects().SelectMany(x => x.GetComponentsInChildren<RoyalHUD>(true)).Count() != 1)
                    throw new InvalidOperationException("Preview has more than one HUD.");
                Directory.CreateDirectory(Path.GetDirectoryName(TargetPreview) ?? throw new InvalidOperationException());
                EditorSceneManager.MarkSceneDirty(scene);
                if (!EditorSceneManager.SaveScene(scene, TargetPreview, true))
                    throw new InvalidOperationException("Unable to save isolated V2 scene.");
            }
            finally
            {
                if (scene.IsValid() && scene.isLoaded)
                    EditorSceneManager.CloseScene(scene, true);
            }
            AssetDatabase.Refresh();
            Debug.Log("ELDORIA_ROYAL_GOLD_V2_PREVIEW_CREATED_ISOLATED " + TargetPreview);
        }
    }
}
