using System;
using System.Collections.Generic;
using System.IO;
using GLTFast;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Eldoria.EditorTools
{
    // Dedicated candidate. Never touches canonical Valoria or its build settings.
    public static class Bastion1ReferenceCapture
    {
        const string Study="Assets/Eldoria/ArtTests/Bastion1Assembly";
        static readonly Dictionary<UnityEngine.Object,string> Saved=new Dictionary<UnityEngine.Object,string>();
        [MenuItem("Eldoria/Art/Build Bastion I reference assembly candidate")]
        public static async void Capture()
        {
            var previousGraphics=GraphicsSettings.defaultRenderPipeline;
            var previousQuality=QualitySettings.renderPipeline;
            try
            {
                // The preceding batch capture can serialize its temporary StudyURP
                // into GraphicsSettings. Deleting that generated asset on the next
                // capture otherwise leaves a null pipeline. Resolve the immutable
                // project URP asset by its committed GUID for every matched frame.
                var original=AssetDatabase.LoadAssetAtPath<UniversalRenderPipelineAsset>(
                    AssetDatabase.GUIDToAssetPath("48e3a09b57f028a4bb2e693d044ea6ca"));
                if(original==null)throw new Exception("Canonical committed URP asset unavailable");
                var pipeline=UnityEngine.Object.Instantiate(original);
                pipeline.name="Bastion I comparative URP, unprofiled";
                pipeline.shadowDistance=180f;pipeline.shadowCascadeCount=1;
                pipeline.msaaSampleCount=1;pipeline.useSRPBatcher=true;
                var settings=new SerializedObject(pipeline);
                var shadowResolution=settings.FindProperty("m_MainLightShadowmapResolution");
                if(shadowResolution==null)throw new Exception("URP main shadow resolution unavailable");
                shadowResolution.intValue=1024;settings.ApplyModifiedPropertiesWithoutUndo();
                Debug.Log("BASTION1_MOBILE_PROFILE shadow_distance=180 cascades=1 shadow_atlas=1024 msaa=1 geometry_unchanged=true");
                Directory.CreateDirectory(Study+"/Generated");
                AssetDatabase.DeleteAsset(Study+"/Generated/StudyURP.asset");
                AssetDatabase.CreateAsset(pipeline,Study+"/Generated/StudyURP.asset");
                GraphicsSettings.defaultRenderPipeline=pipeline;QualitySettings.renderPipeline=pipeline;
                DebugManager.instance.Reset();
                var active=EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
                var source=Path.GetFullPath(Study+"/GeneratedSource/Bastion1ReferenceAssembly.glb");
                if(!File.Exists(source))throw new FileNotFoundException("Run the exact source assembler first",source);
                var importer=new GltfImport(null,new UninterruptedDeferAgent());
                if(!await importer.Load(new Uri(source).AbsoluteUri,new ImportSettings { GenerateMipMaps=true,AnisotropicFilterLevel=2 }))throw new Exception("glTFast source import failed");
                var root=new GameObject("BastionI_ReferenceAssembly_CANDIDATE");
                if(!await importer.InstantiateMainSceneAsync(root.transform))throw new Exception("glTFast instantiate failed");
                Bastion1MaterialBindings.Apply(root,importer,source);
                Bastion1RoadClearance.Apply(root);
                await Bastion1ExistingRoadIntegration.Apply(root);
                StageExistingGroundedEnvironmentalDetails(root);
                InspectRoadContacts(root);
                var sun=new GameObject("Bastion I afternoon sun").AddComponent<Light>();
                sun.type=LightType.Directional;sun.color=new Color(1f,.86f,.72f);sun.intensity=1.25f;
                sun.shadows=LightShadows.Soft;sun.transform.rotation=Quaternion.Euler(38f,-38f,0f);
                RenderSettings.ambientMode=AmbientMode.Trilight;
                RenderSettings.ambientSkyColor=new Color(.37f,.46f,.61f);
                RenderSettings.ambientEquatorColor=new Color(.38f,.40f,.38f);
                RenderSettings.ambientGroundColor=new Color(.31f,.27f,.22f);
                RenderSettings.fog=true;RenderSettings.fogMode=FogMode.ExponentialSquared;RenderSettings.fogDensity=.0028f;
                RenderSettings.fogColor=new Color(.30f,.38f,.45f);
                foreach(var p in new[]{new Vector3(-4,2,20),new Vector3(4,2,20),new Vector3(-4,5.5f,-21),new Vector3(4,5.5f,-21)})
                {
                    var warm=new GameObject("Bastion I amber practical").AddComponent<Light>();
                    warm.type=LightType.Point;warm.color=new Color(1,.42f,.13f);warm.intensity=2.4f;warm.range=7f;
                    warm.shadows=LightShadows.None;warm.transform.position=p;
                }
                var camera=new GameObject("Bastion I strategic camera").AddComponent<Camera>();
                camera.orthographic=true;camera.orthographicSize=26f;
                camera.nearClipPlane=.1f;camera.farClipPlane=350f;
                camera.clearFlags=CameraClearFlags.SolidColor;camera.backgroundColor=new Color(.25f,.34f,.42f);
                // glTF exporter: Blender (x,y,z)->(x,z,-y); glTFast mirrors x.
                camera.transform.position=new Vector3(-32f,49f,76f);camera.transform.LookAt(Vector3.zero);
                var controls=camera.gameObject.AddComponent<Eldoria.Art.Bastion1AssemblyCamera>();
                controls.ConfigureHome();
                // Existing authored world-asset depth; no generated geometry or future buildings.
                var directory=Path.GetFullPath("Bastion1ReferenceCaptures");Directory.CreateDirectory(directory);
                foreach(var stale in Directory.GetFiles(directory,"*.png"))File.Delete(stale);
                Render(camera,Path.Combine(directory,"bastion1-unity-landscape.png"),1440,1000);
                Render(camera,Path.Combine(directory,"bastion1-unity-mobile-landscape.png"),780,520);
                // Portrait shows complete construction envelope, without artificial close zoom.
                camera.orthographicSize=76f;
                Render(camera,Path.Combine(directory,"bastion1-unity-portrait.png"),390,844);
                camera.orthographicSize=26f;
                // Focalized source check: relative clamps cannot teleport this camera
                // to the old image-plane origin on the first player drag.
                var home=camera.transform.position;
                controls.PanByScreenDelta(new Vector2(100000f,-100000f));
                var moved=camera.transform.position-home;
                if(Mathf.Abs(moved.x)>12.001f||Mathf.Abs(moved.z)>12.001f||moved.sqrMagnitude<.01f)
                    throw new Exception("Candidate camera pan bounds invalid");
                controls.ZoomBy(-10000f);controls.Home();
                if(Vector3.Distance(camera.transform.position,home)>.001f)
                    throw new Exception("Candidate Home pose was not restored");
                var renderers=root.GetComponentsInChildren<Renderer>(true);
                var uniqueMeshes=new HashSet<Mesh>();var uniqueMaterials=new HashSet<Material>();var uniqueTextures=new HashSet<Texture>();long triangles=0;
                AssetDatabase.StartAssetEditing();
                try
                {
                foreach(var filter in root.GetComponentsInChildren<MeshFilter>(true))
                {
                    if(filter.sharedMesh==null)continue;
                    triangles+=filter.sharedMesh.triangles.Length/3;uniqueMeshes.Add(filter.sharedMesh);Persist(filter.sharedMesh);
                }
                foreach(var renderer in renderers)foreach(var material in renderer.sharedMaterials)
                {
                    if(material==null)continue;uniqueMaterials.Add(material);
                    foreach(var property in material.GetTexturePropertyNames())
                    {var texture=material.GetTexture(property);if(texture!=null){uniqueTextures.Add(texture);Persist(texture);}}
                    Persist(material);
                }
                }
                finally { AssetDatabase.StopAssetEditing(); }
                // Only selection anchors; neither coarse gate blockers nor invented buildings.
                var upper=Find(root.transform,"UpperBastion");
                if(upper!=null)
                {
                    var handle=new GameObject("bastion.selection.candidate");handle.transform.SetParent(upper,false);
                    var box=handle.AddComponent<BoxCollider>();box.center=new Vector3(0,3,0);box.size=new Vector3(10,6,6);box.isTrigger=true;
                }
                AssetDatabase.SaveAssets();
                EditorSceneManager.SaveScene(active,Study+"/Bastion1ReferenceAssembly.unity");
                await ValoriaBastion1MeshPlayerScene.Build(active,root,camera);
                string evidence="{\"engine\":\"Unity 6000.3.23f1\",\"renderers\":"+renderers.Length+
                    ",\"shared_meshes\":"+uniqueMeshes.Count+",\"shared_materials\":"+uniqueMaterials.Count+
                    ",\"shared_textures\":"+uniqueTextures.Count+",\"visible_triangles\":"+triangles+
                    ",\"art_pass\":false,\"mobile_performance_pass\":false,\"gameplay_pass\":false,\"main_promotion_allowed\":false}";
                File.WriteAllText(Path.Combine(directory,"unity-evidence.json"),evidence);
                GraphicsSettings.defaultRenderPipeline=previousGraphics;QualitySettings.renderPipeline=previousQuality;
                Debug.Log("BASTION1_REFERENCE_CAPTURE_COMPLETE "+evidence);
                EditorApplication.Exit(0);
            }
            catch(Exception ex)
            {
                GraphicsSettings.defaultRenderPipeline=previousGraphics;QualitySettings.renderPipeline=previousQuality;
                Debug.LogError("BASTION1_REFERENCE_CAPTURE_FAILED "+ex);EditorApplication.Exit(1);
            }
        }
        static void StageExistingGroundedEnvironmentalDetails(GameObject scene)
        {
            // Reuse already installed commercial-safe NatureStarterKit foliage and
            // existing Eldoria rock prefabs. These are visuals only, NOT new asset families.
            // Ground each instance by raycasting against the actual existing terrain
            // geometry. No artificial floor heights, no floating decorations.
            var terrain=Find(scene.transform,"Valoria natural playable hilltop and rock scarps");
            if(terrain==null)throw new Exception("Valoria editable plateau missing for genuine ground contacts");
            var temporary=new List<MeshCollider>();
            var legitimate=new HashSet<Collider>();
            var materials=new Dictionary<Material,Material>();
            var visualRoot=new GameObject("Bastion I reused grounded environmental detail");
            visualRoot.transform.SetParent(scene.transform,false);
            int placements=0,misses=0;
            try
            {
                foreach(var f in terrain.GetComponentsInChildren<MeshFilter>(true))
                {
                    if(f.sharedMesh==null)continue;
                    var col=f.gameObject.AddComponent<MeshCollider>();
                    col.sharedMesh=f.sharedMesh;temporary.Add(col);legitimate.Add(col);
                }
                if(temporary.Count<1)throw new Exception("Authored terrain has no raycastable mesh");
                Physics.SyncTransforms();
                // Only back/side margins, clear of the central road and the
                // reserved parcel centres at (+16,3) and (-16,3).
                var bushes=new[]{
                    new Vector2(-22f,-25f),new Vector2(-17f,-28f),
                    new Vector2(-20f,-12f),new Vector2(-23f,-5f),
                    new Vector2(-21f,11f),new Vector2(-16f,16f),
                    new Vector2(-12f,12f),new Vector2(-24f,17f),
                    new Vector2(22f,-26f),new Vector2(17f,-29f),
                    new Vector2(20f,-12f),new Vector2(23f,-6f),
                    new Vector2(20f,11f),new Vector2(15f,15f),
                    new Vector2(11f,12f),new Vector2(24f,16f)
                };
                var rocks=new[]{
                    new Vector2(-24f,-17f),new Vector2(-23f,8f),
                    new Vector2(-18f,18f),new Vector2(-11f,-29f),
                    new Vector2(24f,-18f),new Vector2(22f,8f),
                    new Vector2(18f,18f),new Vector2(12f,-29f)
                };
                for(int i=0;i<bushes.Length;i++)
                {
                    var path="Assets/NatureStarterKit2/Nature/bush0"+(1+i%6)+".prefab";
                    if(SpawnGroundedSource(path,visualRoot.transform,bushes[i],.70f+.16f*(i%4),
                        i*53f,legitimate,materials))placements++;else misses++;
                }
                for(int i=0;i<rocks.Length;i++)
                {
                    var path="Assets/Eldoria/Resources/WorldInventory/Rock0"+(1+i%2)+".prefab";
                    if(SpawnGroundedSource(path,visualRoot.transform,rocks[i],.68f+.20f*(i%3),
                        i*41f,legitimate,materials))placements++;else misses++;
                }
            }
            finally
            {
                foreach(var col in temporary)UnityEngine.Object.DestroyImmediate(col);
                Physics.SyncTransforms();
            }
            if(placements<14)throw new Exception("Insufficient genuine grounded scenic pieces: "+
                placements+"/24 placed, "+misses+" terrain misses");
            Debug.Log("BASTION1_GROUNDED_EXISTING_ASSETS placed="+placements+
                " missed="+misses+" source_prefabs=NatureStarterKit2_and_WorldInventory"+
                " new_mesh_families=0 gameplay_colliders_added=0");
        }

        static bool SpawnGroundedSource(string path,Transform parent,Vector2 position,
            float targetHeight,float rotation,HashSet<Collider> terrain,
            Dictionary<Material,Material> converted)
        {
            var source=AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if(source==null)throw new Exception("Existing staged scenic asset missing "+path);
            var hits=Physics.RaycastAll(new Vector3(position.x,90f,position.y),
                Vector3.down,130f,~0,QueryTriggerInteraction.Ignore);
            var found=false;Vector3 ground=Vector3.zero;
            foreach(var hit in hits)
            {
                if(!terrain.Contains(hit.collider)||hit.normal.y<.70f)continue;
                if(!found||hit.point.y>ground.y){found=true;ground=hit.point;}
            }
            if(!found)return false;
            // Discard collision, scripts and unused real-world gameplay behaviour
            // from these *visual-only* copies, never from original source prefabs.
            var go=PrefabUtility.InstantiatePrefab(source) as GameObject;
            if(go==null)throw new Exception("Existing prefab instance unavailable "+path);
            go.name="Grounded existing scenic "+source.name;
            go.transform.SetParent(parent,true);
            go.transform.position=Vector3.zero;
            go.transform.rotation=Quaternion.Euler(0f,rotation,0f);
            foreach(var collider in go.GetComponentsInChildren<Collider>(true))
                UnityEngine.Object.DestroyImmediate(collider);
            foreach(var behavior in go.GetComponentsInChildren<MonoBehaviour>(true))
                UnityEngine.Object.DestroyImmediate(behavior);
            var renderers=go.GetComponentsInChildren<MeshRenderer>(true);
            if(renderers.Length==0)throw new Exception("Existing prefab has no MeshRenderer "+path);
            var bounds=renderers[0].bounds;
            foreach(var renderer in renderers)bounds.Encapsulate(renderer.bounds);
            if(bounds.size.y<.001f)throw new Exception("Existing prefab height is invalid "+path);
            go.transform.localScale*=targetHeight/bounds.size.y;
            bounds=renderers[0].bounds;
            foreach(var renderer in renderers)bounds.Encapsulate(renderer.bounds);
            go.transform.position+=new Vector3(ground.x-bounds.center.x,
                ground.y-bounds.min.y-.045f,ground.z-bounds.center.z);
            foreach(var renderer in renderers)
            {
                renderer.shadowCastingMode=ShadowCastingMode.Off;
                var original=renderer.sharedMaterials;
                var adapted=new Material[original.Length];
                for(int i=0;i<original.Length;i++)
                {
                    var old=original[i];if(old==null)continue;
                    if(!converted.TryGetValue(old,out var mat))
                    {
                        var shader=Shader.Find("Universal Render Pipeline/Lit");
                        if(shader==null)throw new Exception("Unity URP/Lit missing for scenery conversion");
                        mat=new Material(shader){name="BastionI_reused_"+old.name};
                        var tex=old.HasProperty("_BaseMap")?old.GetTexture("_BaseMap"):
                            old.HasProperty("_MainTex")?old.GetTexture("_MainTex"):null;
                        if(tex!=null)mat.SetTexture("_BaseMap",tex);
                        var tint=old.HasProperty("_BaseColor")?old.GetColor("_BaseColor"):
                            old.HasProperty("_Color")?old.GetColor("_Color"):Color.white;
                        mat.SetColor("_BaseColor",tint);
                        mat.SetFloat("_Smoothness",.06f);
                        var label=old.name.ToLowerInvariant();
                        if(label.Contains("leaf")||label.Contains("foliage")||
                            label.Contains("bush")||label.Contains("branch"))
                        {
                            mat.SetFloat("_AlphaClip",1f);
                            mat.SetFloat("_Cutoff",.35f);
                            mat.EnableKeyword("_ALPHATEST_ON");
                            mat.SetFloat("_Cull",0f);
                        }
                        Persist(mat);converted.Add(old,mat);
                    }
                    adapted[i]=mat;
                }
                renderer.sharedMaterials=adapted;
            }
            Debug.Log("BASTION1_EXISTING_PROP_GROUND_CONTACT prefab="+source.name+
                " pos="+go.transform.position+" height="+targetHeight);
            return true;
        }

        static Transform Find(Transform root,string name)
        {foreach(var item in root.GetComponentsInChildren<Transform>(true))if(item.name==name)return item;return null;}
        internal static void Persist(UnityEngine.Object obj)
        {
            if(obj==null||AssetDatabase.Contains(obj)||Saved.ContainsKey(obj))return;
            string path=Study+"/Generated/"+Saved.Count.ToString("D4")+".asset";
            AssetDatabase.DeleteAsset(path);AssetDatabase.CreateAsset(obj,path);Saved.Add(obj,path);
        }
        internal static void Render(Camera camera,string path,int width,int height)
        {
            var rt=new RenderTexture(width,height,24,RenderTextureFormat.ARGB32){antiAliasing=1};
            var prior=RenderTexture.active;Texture2D pixels=null;
            bool priorAsync=ShaderUtil.allowAsyncCompilation;
            ShaderUtil.allowAsyncCompilation=false;
            try
            {
                camera.aspect=(float)width/height;camera.targetTexture=rt;
                // Compile exact variants synchronously; discard GPU setup frames.
                camera.Render();camera.Render();camera.Render();RenderTexture.active=rt;
                pixels=new Texture2D(width,height,TextureFormat.RGB24,false);
                pixels.ReadPixels(new Rect(0,0,width,height),0,0);pixels.Apply();
                var corner=pixels.GetPixel(0,0);bool nonempty=false;
                for(int y=12;y<height;y+=24)for(int x=12;x<width;x+=24)
                    if(Vector4.Distance(pixels.GetPixel(x,y),corner)>.03f)nonempty=true;
                if(!nonempty)throw new Exception("Empty rendered scene: "+path);
                File.WriteAllBytes(path,pixels.EncodeToPNG());
                File.WriteAllText(path+".camera.json",JsonUtility.ToJson(new CameraEvidence
                {
                    width=width,height=height,position=camera.transform.position,
                    rotation=camera.transform.rotation,orthographicSize=camera.orthographicSize,
                    worldToCamera=camera.worldToCameraMatrix,projection=camera.projectionMatrix,
                    colorSpace=QualitySettings.activeColorSpace.ToString()
                },true));
            }
            finally
            {
                ShaderUtil.allowAsyncCompilation=priorAsync;
                camera.targetTexture=null;RenderTexture.active=prior;
                if(pixels!=null)UnityEngine.Object.DestroyImmediate(pixels);rt.Release();UnityEngine.Object.DestroyImmediate(rt);
            }
        }
        [Serializable] class CameraEvidence
        {
            public int width,height;
            public Vector3 position;
            public Quaternion rotation;
            public float orthographicSize;
            public Matrix4x4 worldToCamera,projection;
            public string colorSpace;
        }
        static void InspectRoadContacts(GameObject root)
        {
            var temporary=new List<MeshCollider>();
            try
            {
                foreach(var filter in root.GetComponentsInChildren<MeshFilter>(true))
                {
                    string name=filter.name.ToLowerInvariant();
                    if(!name.Contains("stone_path")&&!name.Contains("mainroad")&&!name.Contains("stairs")&&!name.Contains("playable hilltop")&&
                       !name.Contains("cliff plateau"))continue;
                    if(filter.sharedMesh==null)continue;
                    var collider=filter.gameObject.AddComponent<MeshCollider>();
                    collider.sharedMesh=filter.sharedMesh;temporary.Add(collider);
                }
                Physics.SyncTransforms();
                foreach(float z in new[]{-12f,-10f,-8f,-6f,-5f,-2f,2f,6f,10f,14f})
                {
                    var hits=Physics.RaycastAll(new Vector3(0,30,z),Vector3.down,80f);
                    Array.Sort(hits,(a,b)=>a.distance.CompareTo(b.distance));
                    foreach(var hit in hits)
                        Debug.Log("BASTION1_REAL_ROAD_CONTACT z="+z+" surface="+hit.collider.name+
                            " parent="+hit.collider.transform.parent?.name+" y="+hit.point.y);
                }
            }
            finally
            {
                foreach(var collider in temporary)UnityEngine.Object.DestroyImmediate(collider);
                Physics.SyncTransforms();
            }
        }
    }
}
