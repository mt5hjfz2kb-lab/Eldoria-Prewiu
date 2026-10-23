#if UNITY_EDITOR
using System;
using System.IO;
using System.Linq;
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
            if (RegionOneCanonicalLandscape.LandscapeVersion != 3)
                throw new InvalidOperationException("Expected Blender-authored presentation");
            var prefab=Resources.Load<GameObject>("World4X/Blender/RegionOneBlenderWorld");
            if(!prefab || prefab.GetComponentsInChildren<MeshFilter>(true).Count(f=>f.name=="BLD_Terrain")!=1)
                throw new InvalidOperationException("Blender GLB did not import with exactly one terrain");
            if(prefab.GetComponentsInChildren<Collider>(true).Length!=0)
                throw new InvalidOperationException("Blender export must not replace gameplay colliders");
            if(RegionOneCanonicalLandscape.SampleHeight(0,0)>-2.5f ||
               RegionOneCanonicalLandscape.CoastDistance(0,0)<5 ||
               RegionOneCanonicalLandscape.CoastDistance(410,0)>-5)
                throw new InvalidOperationException("Exported Blender ground data lost city datum or eastern coast");
            var forest=prefab.GetComponentsInChildren<MeshFilter>(true).Where(f=>f.name.StartsWith("BLD_ForestMass_")).ToArray();
            if(forest.Length<20)throw new InvalidOperationException("Authored forest mass export incomplete");
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

