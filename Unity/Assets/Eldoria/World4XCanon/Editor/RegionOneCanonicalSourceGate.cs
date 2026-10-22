#if UNITY_EDITOR
using System;
using System.IO;
using Eldoria.ReferenceClean;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Eldoria.World4XCanon.Editor
{
    /// <summary>Source contract smoke gate. NOT a visual, mobile or functional acceptance test.</summary>
    public static class RegionOneCanonicalSourceGate
    {
        const string ScenePath = "Assets/Eldoria/ArtTests/ValoriaReferenceClean/Integration/Valoria_City_World_UI.unity";

        [MenuItem("Eldoria/Region 1/Validate canonical world source")]
        public static void Validate()
        {
            if(Environment.GetEnvironmentVariable("ELDORIA_REGION1_P0_COMPARE")=="1")
                RegionOneP0BakeAuthorialAtlas.Bake();
            if (RegionOneCanonicalLandscape.LandscapeVersion != 1 ||
                RegionOneHybridTerrainArt.StyleRevision != 1)
                throw new InvalidOperationException("Unexpected or missing hybrid-world art style version");
            if (RegionOneCanonicalLandscape.SampleHeight(0f, 0f) > -2.5f)
                throw new InvalidOperationException("Overlay would obstruct original hotspot geometry");
            if (!RegionOneCanonicalLandscape.IsForest(-310f, 230f) ||
                !RegionOneCanonicalLandscape.IsForest(-208f,153f) ||
                RegionOneCanonicalLandscape.IsForest(0f, 0f))
                throw new InvalidOperationException("Forest coverage lost geographic anchors");
            if (!RegionOneCanonicalLandscape.IsRoad(0f, 0f) ||
                !RegionOneCanonicalLandscape.IsRiver(-104f, 0f) ||
                RegionOneCanonicalLandscape.IsCoast(0f, 0f) ||
                !RegionOneCanonicalLandscape.IsCoast(520f, -200f))
                throw new InvalidOperationException("Geographic continuity contract failed");

            if (RegionOneCanonicalLandscape.LandscapeHalfWidth < 800f ||
                RegionOneCanonicalLandscape.LandscapeHalfDepth < 700f ||
                RegionOneCanonicalLandscape.BreachOffsetX * RegionOneCanonicalLandscape.BreachOffsetX +
                RegionOneCanonicalLandscape.BreachOffsetZ * RegionOneCanonicalLandscape.BreachOffsetZ > 160f * 160f)
                throw new InvalidOperationException("Region 1 cannot regress to an isolated local diorama or an off-centre Breach.");
            var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            if (!scene.IsValid()) throw new InvalidOperationException("Canonical integrated scene is missing");
            var host = UnityEngine.Object.FindFirstObjectByType<ValoriaIntegratedWorld>(FindObjectsInactive.Include);
            if (!host || !host.WorldRoot || !host.WorldNavigation || !host.WorldCamera)
                throw new InvalidOperationException("Integrated Valoria -> 4X world route is absent");
            var source = AssetDatabase.LoadAssetAtPath<MonoScript>(
                "Assets/Eldoria/World4XCanon/RegionOneCanonicalLandscape.cs");
            if (!source || source.GetClass() != typeof(RegionOneCanonicalLandscape))
                throw new InvalidOperationException("Canonical runtime is not an importable MonoBehaviour");
            Debug.Log("REGION1_CANON_SOURCE_GATE_PASS same_scene=1 deterministic_geography=1 no_save_mutations=1");
            string evidence = Environment.GetEnvironmentVariable("ELDORIA_REGION1_SOURCE_EVIDENCE");
            if (!string.IsNullOrEmpty(evidence))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(evidence)));
                File.WriteAllText(evidence, "{\"source_gate\":\"PASS\",\"scene\":\"integrated\",\"unity_visual_capture\":false,\"physical_ios\":false}");
            }
        }
    }
}
#endif
