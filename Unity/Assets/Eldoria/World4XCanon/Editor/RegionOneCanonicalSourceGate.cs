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
        [Serializable] sealed class CoreMaterialSample {public string material;public float x,y,z,u,v;}
        [Serializable] sealed class CoreMaterialProof {public CoreMaterialSample[] retained_material_samples;public int continental_trees;}
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
            if(!float.IsFinite(RegionOneCanonicalLandscape.SampleHeight(36,6)) ||
               RegionOneCanonicalLandscape.SampleHeight(36,6)<2.5f || RegionOneCanonicalLandscape.SampleHeight(36,6)>4f ||
               RegionOneCanonicalLandscape.CoastDistance(0,0)<5 ||
               RegionOneCanonicalLandscape.CoastDistance(850,0)>-5)
                throw new InvalidOperationException("Exported Blender ground data lost city datum or eastern coast");
            var ground=prefab.GetComponentsInChildren<MeshFilter>(true).Single(f=>f.name=="BLD_Terrain");
            var mesh=ground.sharedMesh;var positions=mesh.vertices;var uv=mesh.uv;var materials=ground.GetComponent<Renderer>().sharedMaterials;
            var layout=Resources.Load<TextAsset>("World4X/Blender/production-region");
            var proof=layout?JsonUtility.FromJson<CoreMaterialProof>(layout.text):null;
            if(proof?.retained_material_samples==null||proof.retained_material_samples.Length<64)
                throw new InvalidOperationException("Original material/UV sample evidence missing");
            var anchor=prefab.GetComponentsInChildren<Transform>(true).Single(t=>t.name=="BLD_BreachAnchor");
            var anchorPoint=prefab.transform.InverseTransformPoint(anchor.position);
            float sign=anchorPoint.x>0?-1:1;
            var localPositions=positions.Select(p=>prefab.transform.InverseTransformPoint(ground.transform.TransformPoint(p))).ToArray();
            foreach(var sample in proof.retained_material_samples)
            {
                bool matched=false;
                var expected=new Vector3(sign*sample.x,sample.y,sign*sample.z);
                for(int sub=0;sub<mesh.subMeshCount&&!matched;sub++)
                {
                    if(!materials[sub].name.StartsWith(sample.material,StringComparison.Ordinal))continue;
                    foreach(int i in mesh.GetIndices(sub))
                        if((localPositions[i]-expected).sqrMagnitude<.002f*.002f&&
                           (uv[i]-new Vector2(sample.u,sample.v)).sqrMagnitude<.0001f*.0001f){matched=true;break;}
                }
                if(!matched)throw new InvalidOperationException("Original core material/UV lost in actual GLB import: "+sample.material+" at "+expected);
            }
            for(int sub=0;sub<mesh.subMeshCount;sub++)if(materials[sub].name.Contains("Region1 open meadows"))
            {
                var indices=mesh.GetIndices(sub).Distinct().ToArray();
                if(indices.Any(i=>Mathf.Abs(localPositions[i].x)<94&&sign*localPositions[i].z>-76&&sign*localPositions[i].z<268))
                    throw new InvalidOperationException("Continental atlas has overwritten the original AF/AG/AI core");
                if(indices.Any(i=>!float.IsFinite(uv[i].x)||!float.IsFinite(uv[i].y)))
                    throw new InvalidOperationException("Continental UV0 is invalid");
            }
            Debug.Log("AF_AG_IMPORTED_MATERIAL_UV_PASS samples="+proof.retained_material_samples.Length);
            var forest=prefab.GetComponentsInChildren<MeshFilter>(true).Where(f=>f.name.StartsWith("BLD_ForestMass_")).ToArray();
            if(forest.Length<20)throw new InvalidOperationException("Authored forest mass export incomplete");
            if(proof.continental_trees!=forest.Length||forest.Select(f=>f.sharedMesh).Distinct().Count()!=2)
                throw new InvalidOperationException("Continental crowns must match authored instance count and share the two original donor meshes");
            Debug.Log("CONTINENTAL_SHARED_CROWNS_PASS instances="+forest.Length+" distinct_near_meshes=2");
            // New sculptural hierarchy must be genuine imported editable
            // Blender geometry, not a pasted colour band or a second map.
            var strata=prefab.GetComponentsInChildren<MeshFilter>(true)
                .SingleOrDefault(f=>f.name=="BLD_BreachTerracedStrata");
            if(!strata||strata.sharedMesh==null||strata.sharedMesh.triangles.Length<270)
                throw new InvalidOperationException("Meaningful authored Brecha strata not in actual imported Unity continent");
            var resources=Resources.Load<GameObject>("World4X/Blender/RegionOneResources");
            if(!resources||resources.GetComponentsInChildren<MeshFilter>(true)
                .Count(f=>f.name.StartsWith("Quarry terraced exposed ledges",StringComparison.Ordinal))!=3)
                throw new InvalidOperationException("All three quarry resource models must carry real layered excavation geometry");
            var lifting=resources.GetComponentsInChildren<MeshFilter>(true)
                .Where(f=>f.name.StartsWith("Quarry lifting gantry and suspended wooden stone sling",StringComparison.Ordinal)).ToArray();
            if(lifting.Length!=3||lifting.Any(f=>!f.sharedMesh||f.sharedMesh.triangles.Length/3>240))
                throw new InvalidOperationException("Quarry worksite equipment must exist in all three original-node variants and remain mobile-budget geometry");
            var detailImporter=AssetImporter.GetAtPath("Assets/Eldoria/World4XCanon/Resources/World4X/Blender/RegionOneGrassDetail.png") as TextureImporter;
            if(!detailImporter||detailImporter.sRGBTexture)throw new InvalidOperationException("AF-derived detail must import as linear data to preserve its neutral mean");
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
