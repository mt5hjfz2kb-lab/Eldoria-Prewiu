#if UNITY_EDITOR
using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using Eldoria.Experimentos.Ciudad2D;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;

namespace Eldoria.Valoria25D.Editor
{
    /// <summary>
    /// Create a NEW scene from the exact, imported V6 artwork. Never overwrite
    /// the inherited V6 source scene, any legacy/main city, or production scenes.
    /// No generated simulation state, command, world or saved resource is embedded.
    /// </summary>
    public static class City25DGameplaySceneBuilder
    {
        public const string Original="Assets/Valoria25D/Scenes/Valoria25D_PremiumV6_Source.unity";
        public const string Playable="Assets/Valoria25D/Scenes/Valoria25D_Gameplay.unity";

        private const string ArchivedGameplaySha256 =
            "024e5dd1be2b83e8a687737b0c5417ceebb766480843d2167e0bf5f7da7c4b30";
        private const string ImmutableArtSha256 =
            "aa566bf6a075868ad852ac865aa8a8cb867d1ade601ac3dd74e8e091f44f0b06";

        static string Hash(string path)
        {
            using(var hash=SHA256.Create())
                return BitConverter.ToString(hash.ComputeHash(File.ReadAllBytes(path)))
                    .Replace("-","").ToLowerInvariant();
        }

        [MenuItem("Eldoria/Valoria25D/Verificar escena jugable archivada (no sobrescribir)")]
        public static void VerifyExisting()
        {
            var root=Path.GetFullPath(Path.Combine(UnityEngine.Application.dataPath,".."));
            string sourcePath=Path.Combine(root,Original);
            string existingPath=Path.Combine(root,Playable);
            if(!File.Exists(sourcePath)||Hash(sourcePath)!=ImmutableArtSha256)
                throw new InvalidOperationException("Original V6 scene changed or missing");
            if(!File.Exists(existingPath)||Hash(existingPath)!=ArchivedGameplaySha256)
                throw new InvalidOperationException(
                    "Previously approved gameplay scene absent or changed; never overwrite silently");
            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
            AssetDatabase.ImportAsset(Playable,ImportAssetOptions.ForceSynchronousImport);
            if(AssetDatabase.LoadAssetAtPath<SceneAsset>(Playable)==null)
                throw new InvalidOperationException("Archived gameplay scene failed Unity import");
            Scene scene=EditorSceneManager.OpenScene(Playable,OpenSceneMode.Single);
            var players=UnityEngine.Object.FindObjectsByType<City25DPlayableController>(
                FindObjectsSortMode.None).Where(x=>x.gameObject.scene==scene).ToArray();
            var plots=UnityEngine.Object.FindObjectsByType<CityBuilding>(
                FindObjectsSortMode.None).Where(x=>x.gameObject.scene==scene).ToArray();
            var shadows=UnityEngine.Object.FindObjectsByType<City2DPremiumShadowGate>(
                FindObjectsSortMode.None).Where(x=>x.gameObject.scene==scene).ToArray();
            var cameras=UnityEngine.Object.FindObjectsByType<Camera>(
                FindObjectsSortMode.None).Where(x=>x.gameObject.scene==scene).ToArray();
            var systems=UnityEngine.Object.FindObjectsByType<EventSystem>(
                FindObjectsSortMode.None).Where(x=>x.gameObject.scene==scene).ToArray();
            if(!scene.IsValid()||!scene.isLoaded||players.Length!=1||
               plots.Length!=6||shadows.Length!=4||
               cameras.Length!=1||systems.Length!=1||
               UnityEngine.Object.FindObjectsByType<City2DPilotHud>(
                   FindObjectsInactive.Include,FindObjectsSortMode.None)
                   .Any(x=>x.gameObject.scene==scene))
                throw new InvalidOperationException(
                    "Prior-certified 2.5D playable source lost a unique root or premium V6 geometry");
            Debug.Log("VALORIA25D_ARCHIVED_SOURCE_SHA_VERIFIED_REUSE "+
                      "source="+ImmutableArtSha256+" gameplay="+ArchivedGameplaySha256+
                      " one_camera=true one_input=true six_plots=true four_shadows=true");
        }

        [MenuItem("Eldoria/Valoria25D/Crear escena jugable aislada")]
        public static void Build()
        {
            string root=Path.GetFullPath(Path.Combine(UnityEngine.Application.dataPath,".."));
            if(!File.Exists(Path.Combine(root,Original)))
                throw new InvalidOperationException("Premium V6 source scene missing");
            if(File.Exists(Path.Combine(root,Playable)))
                throw new InvalidOperationException("Never overwrite a previously built Valoria25D scene");
            // Git-sourced SceneAsset may be present before Unity has imported it.
            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
            AssetDatabase.ImportAsset(Original,ImportAssetOptions.ForceSynchronousImport);
            if(AssetDatabase.LoadAssetAtPath<SceneAsset>(Original)==null)
                throw new InvalidOperationException("V6 source not imported as a Unity SceneAsset");
            Scene source=EditorSceneManager.OpenScene(Original,OpenSceneMode.Single);
            if(!source.IsValid()||!source.isLoaded)
                throw new InvalidOperationException("Premium V6 scene invalid");
            var plots=UnityEngine.Object.FindObjectsByType<CityBuilding>(
                FindObjectsSortMode.None).Where(x=>x.gameObject.scene==source).ToArray();
            var shadows=UnityEngine.Object.FindObjectsByType<City2DPremiumShadowGate>(
                FindObjectsSortMode.None).Where(x=>x.gameObject.scene==source).ToArray();
            if(plots.Length!=6 || shadows.Length!=4)
                throw new InvalidOperationException("Imported V6 lost six plots or four premium shadows");
            var names=plots.Select(x=>x.BuildingId).OrderBy(x=>x).ToArray();
            var expected=new[]{"aserradero","bastion","cantera","cuartel","granero","salon"};
            if(!names.SequenceEqual(expected))
                throw new InvalidOperationException("V6 building mapping changed: "+string.Join(",",names));
            if(UnityEngine.Object.FindObjectsByType<Camera>(FindObjectsSortMode.None)
                .Count(x=>x.gameObject.scene==source)!=1 ||
               UnityEngine.Object.FindObjectsByType<EventSystem>(FindObjectsSortMode.None)
                .Count(x=>x.gameObject.scene==source)!=1 ||
               UnityEngine.Object.FindObjectsByType<City2DCameraFit>(FindObjectsSortMode.None)
                .Count(x=>x.gameObject.scene==source)!=1)
                throw new InvalidOperationException("V6 source must have one camera and EventSystem");
            // Remove demo HUD object from this new scene only. It is never needed
            // or allowed to compete for touches with the new gameplay UI.
            var demos=UnityEngine.Object.FindObjectsByType<City2DPilotHud>(
                FindObjectsInactive.Include,FindObjectsSortMode.None);
            foreach(var demo in demos)
                if(demo.gameObject.scene==source)
                    UnityEngine.Object.DestroyImmediate(demo.gameObject);
            if(UnityEngine.Object.FindObjectsByType<City2DPilotHud>(
                FindObjectsInactive.Include,FindObjectsSortMode.None).Any(x=>x.gameObject.scene==source))
                throw new InvalidOperationException("Visual-demo HUD not removed");

            var rootObject=new GameObject("Valoria25D_RealGameplayRoot");
            rootObject.AddComponent<City25DPlayableController>();
            if(!EditorSceneManager.SaveScene(source,Playable,true))
                throw new InvalidOperationException("Unable to save isolated gameplay scene");
            Scene actual=EditorSceneManager.OpenScene(Playable,OpenSceneMode.Single);
            if(!actual.IsValid()||!actual.isLoaded ||
               UnityEngine.Object.FindObjectsByType<City25DPlayableController>(
                    FindObjectsSortMode.None).Length!=1)
                throw new InvalidOperationException("New scene lacks one gameplay composition root");
            Debug.Log("VALORIA25D_SCENE_BUILT_ONE_ROOT_SIX_PLOTS_FOUR_PREMIUM_SHADOWS"+
                      " no_demo_hud=true no_gameplay_changes=true");
        }
    }
}
#endif
