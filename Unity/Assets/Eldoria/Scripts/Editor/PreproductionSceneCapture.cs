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
        [Serializable] public class Placement { public string module; public string label; public Vector3 position; public float yaw; public float scale=1; }
        [Serializable] public class Replacement { public string asset; public string source_review; public string[] names; public Vector3 position; public float yaw; public float scale=1; public Placement[] placements; }
        [Serializable] public class RetainedFamily { public string asset; public string source_review; public string[] names; public Vector3 position; public float yaw; public float scale=1; }
        [Serializable] public class SourceReview { public string verdict; }
        [Serializable] public class ShoreSegment { public string name; public Vector3[] points; public float width=.8f; public int[] color; }
        [Serializable] public class WaterTreatment { public bool enabled; public string water_name="Water"; public int[] color; public float smoothness=.34f; public float wave_amplitude=.025f; public int grid=16; public ShoreSegment[] shores; }
        [Serializable] public class PremiumTreatment { public bool enabled; public int[] ambient_color; public float ambient_intensity=.42f; public int[] background_color; public int[] fog_color; public bool fog=true; public float fog_start=62f; public float fog_end=145f; public float key_intensity=1.18f; public float shadow_strength=.82f; public float hero_min_z=-42f; public float hero_max_z=-10f; public float hero_min_x=-20f; public float hero_max_x=22f; public float albedo_variation=.06f; public float stone_smoothness=.18f; }
        [Serializable] public class Request { public string mode; public string input; public View[] views; public Replacement replacement; public RetainedFamily[] retained_families; public WaterTreatment water; public PremiumTreatment premium; }
        [Serializable] public class Bound { public string name; public Vector4 bbox; }
        [Serializable] public class ViewResult { public string name; public int width; public int height; public Bound[] bounds; public string phase; }
        [Serializable] public class Evidence { public string engine=UnityEngine.Application.unityVersion; public string classification="GREYBOX_ONLY"; public int meshes; public int triangles; public int colliders; public int tripo_credits=0; public bool production_scene_opened=false; public bool production_scene_saved=false; public ViewResult[] views; public string source_asset; public Vector3 source_position; public float source_yaw; public int source_triangles; public int source_vertices; public int source_renderers; public int source_materials; public int source_textures; public int source_submesh_draws; public string[] source_texture_sizes; public long source_mesh_bytes; public long source_texture_bytes; public bool source_uv; public bool source_normals; public bool source_tangents; public int placement_instances; public int placement_renderers; public int placement_triangles; public int placement_unique_materials; public int placement_unique_meshes; public bool water_treated; public bool water_opaque; public int water_grid_triangles; public int shore_segments; public int shore_triangles; public bool premium_uplift; public bool shadows_enabled; public bool fog_enabled; public int premium_material_renderers; public int premium_ground_patches; public bool surface_authoring; public int surface_authored_renderers; public int surface_uv_fixed_renderers; public int surface_texture_count; public string surface_texture_dimensions; public long surface_texture_bytes_estimated; public int surface_material_instances; }
        const string Folder="ValoriaProductionArtResetV1Captures";

        public static void Capture()
        {
            var root=Path.GetFullPath(Path.Combine(UnityEngine.Application.dataPath,"../.."));
            var request=JsonUtility.FromJson<Request>(File.ReadAllText(Path.Combine(root,"pipeline/valoria-production-art-reset-run-request.json")));
            bool instanceMode=request.mode=="authored_family_instances";
            bool replacementMode=request.mode=="authored_family_replacement" || instanceMode;
            if(request.mode!="preproduction_mesh_scene" && !replacementMode) throw new Exception("Wrong isolated capture mode");
            if(replacementMode) {
                if(request.replacement==null || (!instanceMode && request.replacement.scale!=1)) throw new Exception("A production replacement must use explicit unit scale");
                if(instanceMode && (request.replacement.placements==null || request.replacement.placements.Length==0)) throw new Exception("Authored family instances require an explicit placement plan");
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
                evidence.source_asset=request.replacement.asset;evidence.source_position=request.replacement.position;evidence.source_yaw=request.replacement.yaw;
                var mats=new HashSet<Material>();var texs=new HashSet<Texture>();var textureSizes=new List<string>();
                evidence.source_uv=true;evidence.source_normals=true;evidence.source_tangents=true;
                foreach(var mf in prefab.GetComponentsInChildren<MeshFilter>(true)) { var m=mf.sharedMesh; if(m==null)continue; evidence.source_vertices+=m.vertexCount;evidence.source_mesh_bytes+=UnityEngine.Profiling.Profiler.GetRuntimeMemorySizeLong(m);for(int i=0;i<m.subMeshCount;i++){evidence.source_triangles+=(int)m.GetIndexCount(i)/3;evidence.source_submesh_draws++;}evidence.source_uv &= m.uv.Length==m.vertexCount;evidence.source_normals &= m.normals.Length==m.vertexCount;evidence.source_tangents &= m.tangents.Length==m.vertexCount; }
                foreach(var rr in prefab.GetComponentsInChildren<Renderer>(true)) { evidence.source_renderers++;foreach(var m in rr.sharedMaterials) {if(m==null||!mats.Add(m))continue;foreach(var prop in m.GetTexturePropertyNames()) {var t=m.GetTexture(prop);if(t!=null&&texs.Add(t)){evidence.source_texture_bytes+=UnityEngine.Profiling.Profiler.GetRuntimeMemorySizeLong(t);textureSizes.Add(t.name+":"+t.width+"x"+t.height);}} } }
                evidence.source_materials=mats.Count;evidence.source_textures=texs.Count;evidence.source_texture_sizes=textureSizes.ToArray();

                if(instanceMode)
                {
                    evidence.classification="CUMULATIVE_AUTHORED_FAMILY_PLACEMENT_REVIEW";
                    var placedMaterials=new HashSet<Material>();var placedMeshes=new HashSet<Mesh>();
                    foreach(var placement in request.replacement.placements)
                    {
                        if(placement.scale<=0f) throw new Exception("Placement scale must be positive");
                        var holder=new GameObject("Authored vegetation · "+(string.IsNullOrEmpty(placement.label)?placement.module:placement.label));
                        holder.transform.position=placement.position;holder.transform.rotation=Quaternion.Euler(0,placement.yaw,0);holder.transform.localScale=Vector3.one*placement.scale;
                        var inst=Object.Instantiate(prefab);inst.name="Reusable source instance · "+placement.module;
                        inst.transform.position=Vector3.zero;inst.transform.rotation=Quaternion.identity;inst.transform.localScale=Vector3.one;
                        var module=FindNamed(inst.transform,placement.module);if(module==null) throw new Exception("Reusable module missing: "+placement.module);
                        foreach(var rr in inst.GetComponentsInChildren<Renderer>(true)) rr.enabled=rr.transform==module || rr.transform.IsChildOf(module);
                        var enabled=inst.GetComponentsInChildren<Renderer>(true);Bounds b=new Bounds();bool has=false;
                        foreach(var rr in enabled) if(rr.enabled){if(!has){b=rr.bounds;has=true;}else b.Encapsulate(rr.bounds);}
                        if(!has) throw new Exception("Reusable module has no renderers: "+placement.module);
                        inst.transform.SetParent(holder.transform,false);inst.transform.localPosition=-new Vector3(b.center.x,b.min.y,b.center.z);
                        foreach(var rr in inst.GetComponentsInChildren<Renderer>(true)) if(rr.enabled)
                        {
                            evidence.placement_renderers++;
                            foreach(var m in rr.sharedMaterials)if(m!=null){m.enableInstancing=true;placedMaterials.Add(m);}
                            var mf=rr.GetComponent<MeshFilter>();if(mf!=null&&mf.sharedMesh!=null&&placedMeshes.Add(mf.sharedMesh))
                                for(int sm=0;sm<mf.sharedMesh.subMeshCount;sm++){}
                            if(mf!=null&&mf.sharedMesh!=null)for(int sm=0;sm<mf.sharedMesh.subMeshCount;sm++)evidence.placement_triangles+=(int)mf.sharedMesh.GetIndexCount(sm)/3;
                        }
                        evidence.placement_instances++;
                    }
                    evidence.placement_unique_materials=placedMaterials.Count;evidence.placement_unique_meshes=placedMeshes.Count;
                    renderers.Clear();foreach(var rr in Object.FindObjectsByType<Renderer>(FindObjectsSortMode.None))if(rr.enabled&&rr.gameObject.activeInHierarchy)renderers.Add(rr);
                }
                else
                {
                    assetRoot=Object.Instantiate(prefab);assetRoot.name="Authored family under review";assetRoot.transform.position=request.replacement.position;assetRoot.transform.rotation=Quaternion.Euler(0,request.replacement.yaw,0);assetRoot.transform.localScale=Vector3.one;
                    if(assetRoot.GetComponentsInChildren<Collider>().Length!=0) throw new Exception("Visual family must not contain gameplay colliders");
                    renderers.RemoveAll(r=>!r.gameObject.activeInHierarchy);renderers.AddRange(assetRoot.GetComponentsInChildren<Renderer>());
                    evidence.classification="ISOLATED_PRODUCTION_FAMILY_REVIEW";
                }
            }
            if(phase==1 && request.water!=null && request.water.enabled)
            {
                ApplyWaterTreatment(request.water,evidence);
                renderers.Clear();foreach(var rr in Object.FindObjectsByType<Renderer>(FindObjectsSortMode.None))if(rr.enabled&&rr.gameObject.activeInHierarchy)renderers.Add(rr);
            }
            if(phase==1 && request.premium!=null && request.premium.enabled)
            {
                ApplyPremiumPresentation(request.premium,light,camera,evidence);
                renderers.Clear();foreach(var rr in Object.FindObjectsByType<Renderer>(FindObjectsSortMode.None))if(rr.enabled&&rr.gameObject.activeInHierarchy)renderers.Add(rr);
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

        static void ApplyWaterTreatment(WaterTreatment treatment,Evidence evidence)
        {
            var water=GameObject.Find(treatment.water_name);if(water==null)throw new Exception("Water placeholder missing: "+treatment.water_name);
            var mf=water.GetComponent<MeshFilter>();var rr=water.GetComponent<MeshRenderer>();if(mf==null||rr==null)throw new Exception("Water placeholder requires mesh renderer");
            var old=rr.bounds;int grid=Mathf.Clamp(treatment.grid,4,32);float y=old.max.y+.015f;
            var verts=new Vector3[(grid+1)*(grid+1)];var uv=new Vector2[verts.Length];var tris=new int[grid*grid*6];
            int vi=0;for(int z=0;z<=grid;z++)for(int x=0;x<=grid;x++)
            {
                float u=(float)x/grid,v=(float)z/grid;
                float px=Mathf.Lerp(old.min.x,old.max.x,u),pz=Mathf.Lerp(old.min.z,old.max.z,v);
                float wave=treatment.wave_amplitude*(Mathf.Sin(px*.115f+pz*.071f)+Mathf.Sin(px*.043f-pz*.097f)*.55f);
                verts[vi]=new Vector3(px,y+wave,pz);uv[vi]=new Vector2(u*9f,v*9f);vi++;
            }
            int ti=0;for(int z=0;z<grid;z++)for(int x=0;x<grid;x++)
            {
                int a=z*(grid+1)+x,b=a+1,c=a+(grid+1),d=c+1;
                tris[ti++]=a;tris[ti++]=c;tris[ti++]=b;tris[ti++]=b;tris[ti++]=c;tris[ti++]=d;
            }
            var mesh=new Mesh{name="Valoria Water Surface v1"};mesh.vertices=verts;mesh.triangles=tris;mesh.uv=uv;mesh.RecalculateNormals();mesh.RecalculateBounds();mf.sharedMesh=mesh;
            var mat=new Material(Shader.Find("Universal Render Pipeline/Lit")){name="Valoria Water v1"};
            var col=treatment.color!=null&&treatment.color.Length>=3?new Color(treatment.color[0]/255f,treatment.color[1]/255f,treatment.color[2]/255f,1f):new Color(.11f,.28f,.34f,1f);
            const int texSize=128;var albedo=new Texture2D(texSize,texSize,TextureFormat.RGB24,false){name="Valoria Water Albedo v1",wrapMode=TextureWrapMode.Repeat,filterMode=FilterMode.Bilinear};
            var normal=new Texture2D(texSize,texSize,TextureFormat.RGBA32,false,true){name="Valoria Water Normal v1",wrapMode=TextureWrapMode.Repeat,filterMode=FilterMode.Bilinear};
            var ac=new Color[texSize*texSize];var nc=new Color[texSize*texSize];
            for(int py=0;py<texSize;py++)for(int px=0;px<texSize;px++)
            {
                float u=(float)px/texSize,v=(float)py/texSize;
                float h1=Mathf.Sin((u*3.0f+v*.55f)*Mathf.PI*2f);
                float h2=Mathf.Sin((v*4.0f-u*.35f)*Mathf.PI*2f+.7f);
                float h3=Mathf.Sin((u*7.0f+v*5.0f)*Mathf.PI*2f+1.3f);
                float mix=h1*.55f+h2*.30f+h3*.15f;
                float shade=1f+mix*.055f;
                ac[py*texSize+px]=new Color(Mathf.Clamp01(col.r*shade),Mathf.Clamp01(col.g*shade),Mathf.Clamp01(col.b*shade),1f);
                float du=(Mathf.Cos((u*3.0f+v*.55f)*Mathf.PI*2f)*3f*.55f + Mathf.Cos((u*7.0f+v*5.0f)*Mathf.PI*2f+1.3f)*7f*.15f)*.055f;
                float dv=(Mathf.Cos((v*4.0f-u*.35f)*Mathf.PI*2f+.7f)*4f*.30f + Mathf.Cos((u*7.0f+v*5.0f)*Mathf.PI*2f+1.3f)*5f*.15f)*.055f;
                var n=new Vector3(-du,-dv,1f).normalized;nc[py*texSize+px]=new Color(n.x*.5f+.5f,n.y*.5f+.5f,n.z*.5f+.5f,1f);
            }
            albedo.SetPixels(ac);albedo.Apply();normal.SetPixels(nc);normal.Apply();
            mat.SetColor("_BaseColor",Color.white);mat.SetTexture("_BaseMap",albedo);mat.SetTexture("_BumpMap",normal);mat.SetFloat("_BumpScale",.28f);mat.EnableKeyword("_NORMALMAP");
            mat.SetFloat("_Smoothness",Mathf.Clamp01(treatment.smoothness));mat.SetFloat("_Metallic",.035f);mat.enableInstancing=true;rr.sharedMaterial=mat;
            evidence.water_treated=true;evidence.water_opaque=true;evidence.water_grid_triangles=tris.Length/3;

            if(treatment.shores!=null)foreach(var shore in treatment.shores)
            {
                if(shore.points==null||shore.points.Length<2)continue;
                var go=new GameObject("Valoria Shore · "+shore.name);var smf=go.AddComponent<MeshFilter>();var sr=go.AddComponent<MeshRenderer>();
                var sv=new Vector3[shore.points.Length*2];var su=new Vector2[sv.Length];var st=new int[(shore.points.Length-1)*6];
                for(int i=0;i<shore.points.Length;i++)
                {
                    var p=shore.points[i];Vector3 tangent;
                    if(i==0)tangent=shore.points[1]-p;else if(i==shore.points.Length-1)tangent=p-shore.points[i-1];else tangent=shore.points[i+1]-shore.points[i-1];
                    tangent.y=0;tangent.Normalize();var side=new Vector3(-tangent.z,0,tangent.x)*(shore.width*.5f);
                    sv[i*2]=p-side;sv[i*2+1]=p+side;su[i*2]=new Vector2(0,i*.45f);su[i*2+1]=new Vector2(1,i*.45f);
                    if(i<shore.points.Length-1){int q=i*6,a=i*2;st[q]=a;st[q+1]=a+2;st[q+2]=a+1;st[q+3]=a+1;st[q+4]=a+2;st[q+5]=a+3;}
                }
                var sm=new Mesh{name="Irregular Shore Contact "+shore.name};sm.vertices=sv;sm.triangles=st;sm.uv=su;sm.RecalculateNormals();sm.RecalculateBounds();smf.sharedMesh=sm;
                var sc=shore.color!=null&&shore.color.Length>=3?new Color(shore.color[0]/255f,shore.color[1]/255f,shore.color[2]/255f,1f):new Color(.16f,.20f,.18f,1f);
                var shoreMat=new Material(Shader.Find("Universal Render Pipeline/Lit")){name="Valoria Wet Shore v1"};shoreMat.SetColor("_BaseColor",sc);shoreMat.SetFloat("_Smoothness",.18f);shoreMat.enableInstancing=true;sr.sharedMaterial=shoreMat;
                evidence.shore_segments++;evidence.shore_triangles+=st.Length/3;
            }
        }


        static void ApplyPremiumPresentation(PremiumTreatment treatment,Light key,Camera camera,Evidence evidence)
        {
            Color ambient=treatment.ambient_color!=null&&treatment.ambient_color.Length>=3
                ?new Color(treatment.ambient_color[0]/255f,treatment.ambient_color[1]/255f,treatment.ambient_color[2]/255f)
                :new Color(.50f,.46f,.40f);
            RenderSettings.ambientMode=UnityEngine.Rendering.AmbientMode.Flat;
            RenderSettings.ambientLight=ambient*Mathf.Clamp(treatment.ambient_intensity,.2f,.75f);
            RenderSettings.fog=treatment.fog;
            RenderSettings.fogMode=FogMode.Linear;
            RenderSettings.fogStartDistance=Mathf.Max(1f,treatment.fog_start);
            RenderSettings.fogEndDistance=Mathf.Max(RenderSettings.fogStartDistance+5f,treatment.fog_end);
            RenderSettings.fogColor=treatment.fog_color!=null&&treatment.fog_color.Length>=3
                ?new Color(treatment.fog_color[0]/255f,treatment.fog_color[1]/255f,treatment.fog_color[2]/255f)
                :new Color(.49f,.57f,.61f);
            camera.backgroundColor=treatment.background_color!=null&&treatment.background_color.Length>=3
                ?new Color(treatment.background_color[0]/255f,treatment.background_color[1]/255f,treatment.background_color[2]/255f)
                :RenderSettings.fogColor;

            key.intensity=Mathf.Clamp(treatment.key_intensity,.75f,2f);
            key.color=new Color(1f,.91f,.78f);
            key.transform.rotation=Quaternion.Euler(42f,-28f,0f);
            key.shadows=LightShadows.Soft;
            key.shadowStrength=Mathf.Clamp01(treatment.shadow_strength);
            key.shadowBias=.035f;
            key.shadowNormalBias=.28f;
            QualitySettings.shadows=ShadowQuality.All;
            QualitySettings.shadowDistance=180f;

            foreach(var renderer in Object.FindObjectsByType<Renderer>(FindObjectsSortMode.None))
            {
                if(!renderer.enabled||!renderer.gameObject.activeInHierarchy)continue;
                var bounds=renderer.bounds;string rendererName=renderer.name.ToLowerInvariant();
                bool giantCaster=bounds.size.x>18f||bounds.size.z>18f||rendererName.Contains("ground")||rendererName.Contains("platform")||rendererName.Contains("cliff")||rendererName.Contains("water")||rendererName.Contains("shore");
                renderer.shadowCastingMode=giantCaster?UnityEngine.Rendering.ShadowCastingMode.Off:UnityEngine.Rendering.ShadowCastingMode.On;
                renderer.receiveShadows=true;
            }
            // Surface-first pivot: actual authored albedo/normal/AO/smoothness maps by material family.
            // No broad ground overlay meshes are added in this method.
            GoldenSurfaceAuthoringV1.Apply(treatment,evidence);

            var fill=new GameObject("Premium cool sky fill").AddComponent<Light>();
            fill.type=LightType.Directional;fill.intensity=.16f;fill.color=new Color(.68f,.79f,.88f);fill.transform.rotation=Quaternion.Euler(58f,148f,0f);fill.shadows=LightShadows.None;

            // Flat overlay patches from the previous method are intentionally disabled for Golden Surface v1.

            var water=GameObject.Find("Water");
            if(water!=null)
            {
                var wr=water.GetComponent<MeshRenderer>();
                if(wr!=null&&wr.sharedMaterial!=null)
                {
                    var wm=new Material(wr.sharedMaterial){name="Valoria Water PremiumHeroV1"};
                    if(wm.HasProperty("_Smoothness"))wm.SetFloat("_Smoothness",.58f);
                    if(wm.HasProperty("_Metallic"))wm.SetFloat("_Metallic",.02f);
                    wm.enableInstancing=true;wr.sharedMaterial=wm;
                }
            }
            foreach(var shore in Object.FindObjectsByType<MeshRenderer>(FindObjectsSortMode.None))
            {
                if(!shore.gameObject.name.StartsWith("Valoria Shore"))continue;
                if(shore.sharedMaterial==null)continue;
                var sm=new Material(shore.sharedMaterial){name=shore.sharedMaterial.name+" · PremiumHeroV1"};
                if(sm.HasProperty("_BaseColor"))sm.SetColor("_BaseColor",new Color(.15f,.18f,.13f,1f));
                if(sm.HasProperty("_Smoothness"))sm.SetFloat("_Smoothness",.30f);
                sm.SetFloat("_Cull",0f);sm.enableInstancing=true;shore.sharedMaterial=sm;
            }

            var cameraData=camera.gameObject.GetComponent<UnityEngine.Rendering.Universal.UniversalAdditionalCameraData>();
            if(cameraData==null)cameraData=camera.gameObject.AddComponent<UnityEngine.Rendering.Universal.UniversalAdditionalCameraData>();
            cameraData.renderPostProcessing=true;
            cameraData.antialiasing=UnityEngine.Rendering.Universal.AntialiasingMode.FastApproximateAntialiasing;
            var volume=new GameObject("Premium grading volume").AddComponent<UnityEngine.Rendering.Volume>();
            volume.isGlobal=true;volume.priority=100f;
            var profile=ScriptableObject.CreateInstance<UnityEngine.Rendering.VolumeProfile>();volume.sharedProfile=profile;
            var grading=profile.Add<UnityEngine.Rendering.Universal.ColorAdjustments>(true);
            grading.postExposure.Override(.08f);grading.contrast.Override(14f);grading.saturation.Override(6f);grading.colorFilter.Override(new Color(1f,.985f,.95f));
            var balance=profile.Add<UnityEngine.Rendering.Universal.WhiteBalance>(true);
            balance.temperature.Override(8f);balance.tint.Override(-2f);
            var tonemap=profile.Add<UnityEngine.Rendering.Universal.Tonemapping>(true);
            tonemap.mode.Override(UnityEngine.Rendering.Universal.TonemappingMode.ACES);
            var bloom=profile.Add<UnityEngine.Rendering.Universal.Bloom>(true);
            bloom.intensity.Override(.08f);bloom.threshold.Override(1.05f);bloom.scatter.Override(.55f);

            evidence.premium_uplift=true;evidence.shadows_enabled=true;evidence.fog_enabled=RenderSettings.fog;
        }

        static void CreatePremiumStoneDetail(out Texture2D albedo,out Texture2D normal)
        {
            const int size=128;
            albedo=new Texture2D(size,size,TextureFormat.RGB24,true){name="Valoria Premium Stone Detail",wrapMode=TextureWrapMode.Repeat,filterMode=FilterMode.Trilinear,anisoLevel=4};
            normal=new Texture2D(size,size,TextureFormat.RGBA32,true,true){name="Valoria Premium Stone Detail Normal",wrapMode=TextureWrapMode.Repeat,filterMode=FilterMode.Trilinear,anisoLevel=4};
            var ac=new Color[size*size];var nc=new Color[size*size];
            for(int y=0;y<size;y++)for(int x=0;x<size;x++)
            {
                float u=(float)x/size,v=(float)y/size;
                float strata=Mathf.Sin((u*2.7f+v*.45f)*Mathf.PI*2f)*.55f+Mathf.Sin((v*5.1f-u*.35f)*Mathf.PI*2f+.8f)*.25f;
                float grain=Mathf.Sin((u*11f+v*7f)*Mathf.PI*2f+1.2f)*.20f;
                float value=.50f+(strata+grain)*.055f;
                ac[y*size+x]=new Color(value,value*.995f,value*.97f,1f);
                float du=(Mathf.Cos((u*2.7f+v*.45f)*Mathf.PI*2f)*2.7f*.55f+Mathf.Cos((u*11f+v*7f)*Mathf.PI*2f+1.2f)*11f*.20f)*.018f;
                float dv=(Mathf.Cos((v*5.1f-u*.35f)*Mathf.PI*2f+.8f)*5.1f*.25f+Mathf.Cos((u*11f+v*7f)*Mathf.PI*2f+1.2f)*7f*.20f)*.018f;
                var n=new Vector3(-du,-dv,1f).normalized;nc[y*size+x]=new Color(n.x*.5f+.5f,n.y*.5f+.5f,n.z*.5f+.5f,1f);
            }
            albedo.SetPixels(ac);albedo.Apply(true,false);normal.SetPixels(nc);normal.Apply(true,false);
        }

        static void CreatePremiumGroundPatch(string name,Vector3[] points,Color color,float smoothness,Evidence evidence)
        {
            if(points==null||points.Length<3)return;
            var go=new GameObject("Premium ground blend · "+name);
            var mesh=new Mesh{name=name+" mesh"};var tris=new int[(points.Length-2)*3];
            for(int i=0;i<points.Length-2;i++){tris[i*3]=0;tris[i*3+1]=i+2;tris[i*3+2]=i+1;}
            mesh.vertices=points;mesh.triangles=tris;mesh.RecalculateNormals();mesh.RecalculateBounds();
            go.AddComponent<MeshFilter>().sharedMesh=mesh;
            var rr=go.AddComponent<MeshRenderer>();rr.shadowCastingMode=UnityEngine.Rendering.ShadowCastingMode.Off;rr.receiveShadows=true;
            var mat=new Material(Shader.Find("Universal Render Pipeline/Lit")){name=name+" material"};
            mat.SetColor("_BaseColor",color);mat.SetFloat("_Smoothness",smoothness);mat.SetFloat("_Metallic",0f);mat.SetFloat("_Cull",0f);mat.enableInstancing=true;
            rr.sharedMaterial=mat;evidence.premium_ground_patches++;
        }

        static Transform FindNamed(Transform root,string name)
        {
            if(root.name==name)return root;
            foreach(Transform child in root){var found=FindNamed(child,name);if(found!=null)return found;}
            return null;
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

