using System;
using System.IO;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Eldoria.EditorTools
{
    // Generic, editor-only blockout and source-approved family replacement review. Never opens or saves production scenes.
    public static class PreproductionSceneCapture
    {
        [Serializable] public class MeshInput { public string name; public string family; public Vector3[] vertices; public int[] indices; public int[] color; }
        [Serializable] public class Input { public string classification; public MeshInput[] meshes; }
        [Serializable] public class View { public string name; public int width; public int height; public float pitch; public float yaw; public float span; public Vector3 center; public bool perspective; public float distance; }
        [Serializable] public class Replacement { public string asset; public string source_review; public string[] names; public Vector3 position; public float yaw; public float scale=1; }
        [Serializable] public class RetainedFamily { public string asset; public string source_review; public string[] names; public Vector3 position; public float yaw; public float scale=1; }
        [Serializable] public class SourceReview { public string verdict; }
        [Serializable] public class Request { public string mode; public string input; public View[] views; public Replacement replacement; public RetainedFamily[] retained_families; }
        [Serializable] public class Bound { public string name; public Vector4 bbox; }
        [Serializable] public class ViewResult { public string name; public int width; public int height; public Bound[] bounds; public string phase; }
        [Serializable] public class Evidence { public string engine=UnityEngine.Application.unityVersion; public string classification="GREYBOX_ONLY"; public int meshes; public int triangles; public int colliders; public int tripo_credits=0; public bool production_scene_opened=false; public bool production_scene_saved=false; public ViewResult[] views; public string source_asset; public Vector3 source_position; public float source_yaw; public int source_triangles; public int source_vertices; public int source_renderers; public int source_materials; public int source_textures; public int source_submesh_draws; public string[] source_texture_sizes; public long source_mesh_bytes; public long source_texture_bytes; public bool source_uv; public bool source_normals; public bool source_tangents; }
        const string Folder="ValoriaProductionArtResetV1Captures";

        public static void Capture()
        {
            var root=Path.GetFullPath(Path.Combine(UnityEngine.Application.dataPath,"../.."));
            var request=JsonUtility.FromJson<Request>(File.ReadAllText(Path.Combine(root,"pipeline/valoria-production-art-reset-run-request.json")));
            bool replacementMode=request.mode=="authored_family_replacement";
            if(request.mode!="preproduction_mesh_scene" && !replacementMode) throw new Exception("Wrong isolated capture mode");
            if(replacementMode) {
                if(request.replacement==null || request.replacement.scale!=1) throw new Exception("A production replacement must use explicit unit scale");
                var review=JsonUtility.FromJson<SourceReview>(File.ReadAllText(Path.Combine(root,request.replacement.source_review)));
                if(review.verdict!="ART SOURCE PASS") throw new Exception("Isolated art source gate required before Unity");
            }
            var input=JsonUtility.FromJson<Input>(File.ReadAllText(Path.Combine(root,request.input)));
            if(input.classification!="GREYBOX_ONLY") throw new Exception("Final geometry prohibited");
            SceneSetup.SetupRenderPipeline();
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
            ShaderUtil.allowAsyncCompilation=false;
            var materials=new Dictionary<string,Material>();
            var renderers=new List<Renderer>();
            int triangles=0;
            foreach(var item in input.meshes)
            {
                var go=new GameObject(item.name);
                var vertices=new Vector3[item.indices.Length];
                var indices=new int[item.indices.Length];
                // Flat face normals, no shading polish. Both sides survive coordinate handedness conversion.
                for(int i=0;i<indices.Length;i++)
                {
                    // X/Z/Y conversion reflects handedness: reverse triangle winding.
                    int corner=i%3;int source=i-corner+(corner==1?2:corner==2?1:0);
                    vertices[i]=item.vertices[item.indices[source]];indices[i]=i;
                }
                var mesh=new Mesh{name=item.name+"_GREYBOX"};mesh.vertices=vertices;mesh.triangles=indices;mesh.RecalculateNormals();mesh.RecalculateBounds();
                go.AddComponent<MeshFilter>().sharedMesh=mesh;
                var renderer=go.AddComponent<MeshRenderer>();
                if(!materials.TryGetValue(item.family,out var material))
                {
                    material=new Material(Shader.Find("Universal Render Pipeline/Lit"));
                    material.SetColor("_BaseColor",new Color(item.color[0]/255f,item.color[1]/255f,item.color[2]/255f));
                    material.SetFloat("_Smoothness",0);material.SetFloat("_Cull",0);
                    materials.Add(item.family,material);
                }
                renderer.sharedMaterial=material;renderers.Add(renderer);triangles+=indices.Length/3;
            }
            RenderSettings.ambientMode=UnityEngine.Rendering.AmbientMode.Flat;
            RenderSettings.ambientLight=new Color(.7f,.7f,.7f);RenderSettings.fog=false;
            var light=new GameObject("Greybox directional light").AddComponent<Light>();
            light.type=LightType.Directional;light.intensity=.7f;light.transform.rotation=Quaternion.Euler(48,-35,0);light.shadows=LightShadows.None;
            var camera=new GameObject("Preproduction camera").AddComponent<Camera>();
            camera.clearFlags=CameraClearFlags.SolidColor;camera.backgroundColor=new Color(.53f,.57f,.60f);
            camera.nearClipPlane=.1f;camera.farClipPlane=400f;
            Directory.CreateDirectory(Folder);
            var results=new List<ViewResult>();
            var evidence=new Evidence{meshes=input.meshes.Length,triangles=triangles};
            // Already accepted families may be retained identically in both phases so a new family
            // is judged against the real accumulated production context without reopening them.
            if(replacementMode && request.retained_families!=null)
            {
                foreach(var retained in request.retained_families)
                {
                    if(retained==null || retained.scale!=1) throw new Exception("Retained production family must use explicit unit scale");
                    var retainedReview=JsonUtility.FromJson<SourceReview>(File.ReadAllText(Path.Combine(root,retained.source_review)));
                    if(retainedReview.verdict!="ART SOURCE PASS") throw new Exception("Retained family requires accepted ART SOURCE PASS");
                    foreach(var name in retained.names)
                    {
                        var go=GameObject.Find(name);if(go==null) throw new Exception("Retained placeholder missing: "+name);go.SetActive(false);
                    }
                    AssetDatabase.ImportAsset(retained.asset,ImportAssetOptions.ForceSynchronousImport|ImportAssetOptions.ForceUpdate);
                    var retainedPrefab=AssetDatabase.LoadAssetAtPath<GameObject>(retained.asset);
                    if(retainedPrefab==null) throw new Exception("Retained glTFast import failed: "+retained.asset);
                    var retainedRoot=Object.Instantiate(retainedPrefab);retainedRoot.name="Accepted retained family";
                    retainedRoot.transform.position=retained.position;retainedRoot.transform.rotation=Quaternion.Euler(0,retained.yaw,0);retainedRoot.transform.localScale=Vector3.one;
                    if(retainedRoot.GetComponentsInChildren<Collider>().Length!=0) throw new Exception("Retained visual family must not contain gameplay colliders");
                    // Rebuild the active visual renderer set after placeholder suppression.
                    renderers.Clear();
                    foreach(var rr in Object.FindObjectsByType<Renderer>(FindObjectsSortMode.None))
                        if(rr.gameObject.activeInHierarchy) renderers.Add(rr);
                }
            }
            GameObject assetRoot=null;
            for(int phase=0;phase<(replacementMode?2:1);phase++)
            {
            if(phase==1)
            {
                foreach(var name in request.replacement.names)
                {
                    var go=GameObject.Find(name);if(go==null) throw new Exception("Replacement placeholder missing: "+name);go.SetActive(false);
                }
                AssetDatabase.ImportAsset(request.replacement.asset,ImportAssetOptions.ForceSynchronousImport|ImportAssetOptions.ForceUpdate);
                var prefab=AssetDatabase.LoadAssetAtPath<GameObject>(request.replacement.asset);
                if(prefab==null) throw new Exception("glTFast import failed: "+request.replacement.asset);
                assetRoot=Object.Instantiate(prefab);assetRoot.name="Authored family under review";assetRoot.transform.position=request.replacement.position;assetRoot.transform.rotation=Quaternion.Euler(0,request.replacement.yaw,0);assetRoot.transform.localScale=Vector3.one;
                if(assetRoot.GetComponentsInChildren<Collider>().Length!=0) throw new Exception("Visual family must not contain gameplay colliders");
                renderers.RemoveAll(r=>!r.gameObject.activeInHierarchy);renderers.AddRange(assetRoot.GetComponentsInChildren<Renderer>());
                evidence.classification="ISOLATED_PRODUCTION_FAMILY_REVIEW";evidence.source_asset=request.replacement.asset;evidence.source_position=request.replacement.position;evidence.source_yaw=request.replacement.yaw;
                var mats=new HashSet<Material>();var texs=new HashSet<Texture>();var textureSizes=new List<string>();
                evidence.source_uv=true;evidence.source_normals=true;evidence.source_tangents=true;
                foreach(var mf in assetRoot.GetComponentsInChildren<MeshFilter>()) { var m=mf.sharedMesh; if(m==null)continue; evidence.source_vertices+=m.vertexCount;evidence.source_mesh_bytes+=UnityEngine.Profiling.Profiler.GetRuntimeMemorySizeLong(m);for(int i=0;i<m.subMeshCount;i++){evidence.source_triangles+=(int)m.GetIndexCount(i)/3;evidence.source_submesh_draws++;}evidence.source_uv &= m.uv.Length==m.vertexCount;evidence.source_normals &= m.normals.Length==m.vertexCount;evidence.source_tangents &= m.tangents.Length==m.vertexCount; }
                foreach(var r in assetRoot.GetComponentsInChildren<Renderer>()) { evidence.source_renderers++;foreach(var m in r.sharedMaterials) {if(m==null||!mats.Add(m))continue;foreach(var prop in m.GetTexturePropertyNames()) {var t=m.GetTexture(prop);if(t!=null&&texs.Add(t)){evidence.source_texture_bytes+=UnityEngine.Profiling.Profiler.GetRuntimeMemorySizeLong(t);textureSizes.Add(t.name+":"+t.width+"x"+t.height);}} } }
                evidence.source_materials=mats.Count;evidence.source_textures=texs.Count;evidence.source_texture_sizes=textureSizes.ToArray();
            }
            foreach(var view in request.views)
            {
                float pitch=view.pitch*Mathf.Deg2Rad,yaw=view.yaw*Mathf.Deg2Rad;
                var vector=new Vector3(Mathf.Sin(yaw)*Mathf.Cos(pitch),Mathf.Sin(pitch),-Mathf.Cos(yaw)*Mathf.Cos(pitch));
                camera.transform.position=view.center+vector*view.distance;camera.transform.LookAt(view.center);
                camera.orthographic=!view.perspective;camera.orthographicSize=view.span/2f;
                camera.fieldOfView=2f*Mathf.Atan(view.span/2f/view.distance)*Mathf.Rad2Deg;
                camera.aspect=(float)view.width/view.height;
                var bounds=new List<Bound>();
                foreach(var renderer in renderers)
                {
                    var mesh=renderer.GetComponent<MeshFilter>().sharedMesh;
                    float xmin=float.PositiveInfinity,ymin=xmin,xmax=float.NegativeInfinity,ymax=xmax;
                    foreach(var vertex in mesh.vertices)
                    {
                        var p=camera.WorldToViewportPoint(renderer.transform.TransformPoint(vertex));
                        xmin=Mathf.Min(xmin,p.x*view.width);xmax=Mathf.Max(xmax,p.x*view.width);
                        ymin=Mathf.Min(ymin,(1-p.y)*view.height);ymax=Mathf.Max(ymax,(1-p.y)*view.height);
                    }
                    bounds.Add(new Bound{name=renderer.name,bbox=new Vector4(xmin,ymin,xmax,ymax)});
                }
                string prefix=replacementMode?(phase==0?"BEFORE-":"AFTER-"):"";
                Save(camera,Folder+"/"+prefix+view.name+".png",view.width,view.height);
                results.Add(new ViewResult{name=prefix+view.name,width=view.width,height=view.height,bounds=bounds.ToArray(),phase=phase==0?"BLOCKOUT":"INTEGRATED"});
            }
            }
            int colliders=Object.FindObjectsByType<Collider>(FindObjectsSortMode.None).Length;
            if(colliders!=0) throw new Exception("Preproduction scene must have zero gameplay colliders");
            evidence.colliders=colliders;evidence.views=results.ToArray();
            File.WriteAllText(Folder+"/evidence.json",JsonUtility.ToJson(evidence,true));
            Debug.Log("PREPRODUCTION_CAPTURE_TECH_PASS; VISUAL_REVIEW_REQUIRED");
            EditorApplication.Exit(0);
        }

        static void Save(Camera camera,string path,int width,int height)
        {
            var rt=new RenderTexture(width,height,24,RenderTextureFormat.ARGB32){antiAliasing=1};
            var previous=RenderTexture.active;
            try
            {
                camera.targetTexture=rt;camera.Render();camera.Render();RenderTexture.active=rt;
                var image=new Texture2D(width,height,TextureFormat.RGB24,false);
                image.ReadPixels(new Rect(0,0,width,height),0,0);image.Apply();File.WriteAllBytes(path,image.EncodeToPNG());Object.DestroyImmediate(image);
            }
            finally{camera.targetTexture=null;RenderTexture.active=previous;rt.Release();Object.DestroyImmediate(rt);}
        }
    }
}

