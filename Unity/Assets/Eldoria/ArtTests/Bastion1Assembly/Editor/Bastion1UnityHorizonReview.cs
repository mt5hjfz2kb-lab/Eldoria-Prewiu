using System;
using System.Collections.Generic;
using System.IO;
using Eldoria.Presentation;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Eldoria.EditorTools
{
    // Reuses the SAME historical distant mountain: its vertices, triangles,
    // transforms and gameplay are all preserved. Only its existing surface
    // normals and material are edited on isolated copies for real A/B.
    public static class Bastion1UnityHorizonReview
    {
        const string Before = "Assets/Eldoria/ArtTests/Bastion1Assembly/Bastion1UnityFinishLookCandidate.unity";
        const string Candidate = "Assets/Eldoria/ArtTests/Bastion1Assembly/Bastion1UnityHorizonCandidate.unity";
        const string Generated = "Assets/Eldoria/ArtTests/Bastion1Assembly/GeneratedHorizonLook";
        const string Evidence = "Bastion1UnityHorizonAB";

        [MenuItem("Eldoria/Art/Bastion I native Unity distant world material-normal A-B")]
        public static void Capture()
        {
            try
            {
                if(AssetDatabase.LoadAssetAtPath<SceneAsset>(Before)==null)
                    throw new FileNotFoundException("Previous real Unity lookdev scene missing",Before);
                Directory.CreateDirectory(Evidence);
                var scene=EditorSceneManager.OpenScene(Before,OpenSceneMode.Single);
                var presentation=UnityEngine.Object.FindFirstObjectByType<ValoriaParcelPresentation>();
                if(presentation==null||!presentation.MeshMode||
                   presentation.ProductionCamera==null||
                   presentation.Bindings==null||presentation.Bindings.Length<2||
                   presentation.Bindings[0].Ground==null||
                   presentation.Bindings[0].Construction==null||
                   presentation.Bindings[0].Built==null||
                   presentation.Bindings[1].Built==null)
                    throw new Exception("Original playable progression bindings not retained");
                var camera=presentation.ProductionCamera;
                var pose=camera.transform.position;
                var rotate=camera.transform.rotation;
                var originalSize=camera.orthographicSize;
                var originalMeshCount=CountMeshFilters();
                RenderSet(camera,"before",originalSize);

                var mountain=GameObject.Find("Valoria true mountain backdrop");
                if(mountain==null)throw new Exception("Existing photographed mountain node missing");
                var filter=mountain.GetComponent<MeshFilter>();
                var renderer=mountain.GetComponent<MeshRenderer>();
                if(filter==null||filter.sharedMesh==null||renderer==null||
                   renderer.sharedMaterial==null)
                    throw new Exception("Existing visible mountain mesh or material not found");
                var original=filter.sharedMesh;
                int triangles=original.triangles.Length/3;
                int vertices=original.vertexCount;
                if(triangles!=3300||vertices!=9896)
                    throw new Exception("Source mountain geometry changed, refuse modification: "+
                                        triangles+" tris "+vertices+" verts");

                // Select a PBR stone texture ALREADY USED by the original scenery.
                // Never generate a new texture, terrain mesh or world object.
                Texture texture=null;
                foreach(var r in UnityEngine.Object.FindObjectsByType<MeshRenderer>(
                             FindObjectsInactive.Include,FindObjectsSortMode.None))
                {
                    if(!r.gameObject.name.ToLowerInvariant().StartsWith("cliff_rocks_material_group_"))continue;
                    var m=r.sharedMaterial;
                    if(m!=null && m.HasProperty("_BaseMap") && m.GetTexture("_BaseMap")!=null)
                    {texture=m.GetTexture("_BaseMap");break;}
                }
                if(texture==null)
                    throw new Exception("Existing historic cliff PBR texture missing; cannot fake source");

                Directory.CreateDirectory(Generated);
                string meshPath=Generated+"/horizon_original_topology_softened_normals.asset";
                string matPath=Generated+"/horizon_existing_rock_albedo.mat";
                if(AssetDatabase.LoadMainAssetAtPath(meshPath)!=null)AssetDatabase.DeleteAsset(meshPath);
                if(AssetDatabase.LoadMainAssetAtPath(matPath)!=null)AssetDatabase.DeleteAsset(matPath);

                // Clone only because the original reusable model must remain immutable.
                var altered=UnityEngine.Object.Instantiate(original);
                altered.name="Same mountain geometry with smoothed shared-position normals";
                int averaged=SmoothSharedPositionNormals(altered);
                if(altered.vertexCount!=vertices || altered.triangles.Length/3!=triangles ||
                   altered.bounds!=original.bounds)
                    throw new Exception("Original mountain topology or bounding box was damaged");
                AssetDatabase.CreateAsset(altered,meshPath);
                filter.sharedMesh=altered;

                var oldMaterial=renderer.sharedMaterial;
                var newMaterial=new Material(oldMaterial){name="Bastion I native reused cliff PBR distant rock"};
                if(!newMaterial.HasProperty("_BaseColor")||!newMaterial.HasProperty("_BaseMap"))
                    throw new Exception("Original mountain material is not a supported URP Lit source");
                // Render from the same textured rock family at distant atmospheric value.
                newMaterial.SetTexture("_BaseMap",texture);
                newMaterial.SetTextureScale("_BaseMap",new Vector2(2.4f,2.4f));
                var c=oldMaterial.GetColor("_BaseColor");
                newMaterial.SetColor("_BaseColor",
                    new Color(Mathf.Clamp(c.r*1.8f,.3f,.63f),
                              Mathf.Clamp(c.g*1.65f,.32f,.65f),
                              Mathf.Clamp(c.b*1.55f,.38f,.72f),c.a));
                AssetDatabase.CreateAsset(newMaterial,matPath);
                renderer.sharedMaterial=newMaterial;
                AssetDatabase.SaveAssets();

                if(CountMeshFilters()!=originalMeshCount ||
                   camera.transform.position!=pose || camera.transform.rotation!=rotate ||
                   Mathf.Abs(camera.orthographicSize-originalSize)>.0001f)
                    throw new Exception("Scene mesh object count or gameplay camera was modified");
                if(!EditorSceneManager.SaveScene(scene,Candidate,true))
                    throw new Exception("Could not save isolated horizon study");
                RenderSet(camera,"after",originalSize);
                File.WriteAllText(Path.Combine(Evidence,"horizon-result.json"),
                    "{\n  \"schema\": \"eldoria-bastion1-unity-horizon-original-mesh-v1\",\n"+
                    "  \"source_scene_unchanged\": true,\n  \"new_models\": 0,\n"+
                    "  \"same_mesh_vertices\": "+vertices+",\n  \"same_mesh_triangles\": "+triangles+",\n"+
                    "  \"normals_averaged\": "+averaged+",\n"+
                    "  \"existing_rock_texture_reused\": true,\n"+
                    "  \"camera_unchanged\": true,\n"+
                    "  \"gameplay_code_unchanged\": true,\n"+
                    "  \"art_approved\": false,\n  \"performance_approved\": false\n}\n");
                Debug.Log("BASTION1_UNITY_HORIZON_AB_COMPLETE old_mesh_preserved=true vertices="+vertices+
                    " triangles="+triangles+" normals_averaged="+averaged+
                    " art_review_required=true");
                EditorApplication.Exit(0);
            }
            catch(Exception ex)
            {
                Debug.LogError("BASTION1_UNITY_HORIZON_AB_FAILED "+ex);
                EditorApplication.Exit(1);
            }
        }

        static int SmoothSharedPositionNormals(Mesh mesh)
        {
            var vertices=mesh.vertices;
            var indices=mesh.triangles;
            var normalSums=new Dictionary<Vector3Int,Vector3>();
            for(int t=0;t+2<indices.Length;t+=3)
            {
                var a=vertices[indices[t]];
                var b=vertices[indices[t+1]];
                var c=vertices[indices[t+2]];
                var face=Vector3.Cross(b-a,c-a);
                if(face.sqrMagnitude<1e-10f)continue;
                foreach(var v in new[]{a,b,c})
                {
                    var key=new Vector3Int(Mathf.RoundToInt(v.x*1000f),
                                           Mathf.RoundToInt(v.y*1000f),
                                           Mathf.RoundToInt(v.z*1000f));
                    normalSums.TryGetValue(key,out var sum);
                    normalSums[key]=sum+face;
                }
            }
            var old=mesh.normals;
            if(old==null||old.Length!=vertices.Length)
                throw new Exception("Existing source lacks valid normals");
            var normals=(Vector3[])old.Clone();
            int count=0;
            for(int i=0;i<vertices.Length;i++)
            {
                var v=vertices[i];
                var key=new Vector3Int(Mathf.RoundToInt(v.x*1000f),
                                       Mathf.RoundToInt(v.y*1000f),
                                       Mathf.RoundToInt(v.z*1000f));
                if(!normalSums.TryGetValue(key,out var sum)||sum.sqrMagnitude<1e-10f)continue;
                var average=sum.normalized;
                if(Vector3.Dot(average,old[i])<0f)average=-average;
                if(Vector3.Angle(average,old[i])>.5f){normals[i]=average;count++;}
            }
            if(count<300)
                throw new Exception("No meaningful existing mountain facet smoothing");
            mesh.normals=normals;
            return count;
        }

        static int CountMeshFilters()
        {
            int n=0;
            foreach(var f in UnityEngine.Object.FindObjectsByType<MeshFilter>(
                        FindObjectsInactive.Include,FindObjectsSortMode.None))
                if(f.sharedMesh!=null)n++;
            return n;
        }
        static void RenderSet(Camera camera,string stage,float size)
        {
            Bastion1ReferenceCapture.Render(camera,Path.GetFullPath(Evidence+"/"+stage+"-landscape.png"),1440,1000);
            Bastion1ReferenceCapture.Render(camera,Path.GetFullPath(Evidence+"/"+stage+"-mobile.png"),780,520);
            camera.orthographicSize=76f;
            try {Bastion1ReferenceCapture.Render(camera,Path.GetFullPath(Evidence+"/"+stage+"-portrait.png"),390,844);}
            finally{camera.orthographicSize=size;}
        }
    }
}
