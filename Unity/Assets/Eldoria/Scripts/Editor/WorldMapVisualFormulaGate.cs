using System.IO;
using Eldoria.Domain;
using Eldoria.Presentation;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Eldoria.EditorTools
{
    public static class WorldMapVisualFormulaGate
    {
        public static void Capture()
        {
            CaptureMatchedWedge();
            SceneSetup.SetupRenderPipeline();
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
            var state=new PlayerState
            {
                BastionLevel=2,
                SawmillLevel=1,
                BarracksLevel=1,
                CorruptionDiscovered=true
            };
            VisualWorld.Create(false,state);
            var camera=Camera.main;
            if(camera==null)throw new System.Exception("Frontier camera missing.");

            var forestHotspot=GameObject.Find("Bosque de Valoria · recolectar");
            if(forestHotspot==null||forestHotspot.GetComponent<WorldHotspot>()==null)
                throw new System.Exception("Forest gameplay hotspot missing.");
            var quarry=GameObject.Find("Frontier · quarry resource kit");
            if(quarry==null)throw new System.Exception("Quarry Resource Kit v1 missing.");
            foreach(var c in quarry.GetComponentsInChildren<Collider>(true))
                if(c.enabled)throw new System.Exception("Quarry visual kit must not own gameplay collision.");
            var route=GameObject.Find("Frontier · march route kit");
            if(route==null)throw new System.Exception("World Route Kit v1 missing.");
            foreach(var c in route.GetComponentsInChildren<Collider>(true))
                if(c.enabled)throw new System.Exception("Route visual kit must not own gameplay collision.");

            int quarryRenderers=0;
            foreach(var r in quarry.GetComponentsInChildren<Renderer>(true))if(r.enabled)quarryRenderers++;
            int routeRenderers=0;
            foreach(var r in route.GetComponentsInChildren<Renderer>(true))if(r.enabled)routeRenderers++;

            const string folder="WorldMapVisualFormulaCaptures";
            Directory.CreateDirectory(folder);
            var officialPosition=new Vector3(20f,24f,-21f);
            var officialTarget=new Vector3(0f,0f,1f);

            Save(camera,folder+"/frontier-overview-18.png",officialPosition,officialTarget,18f,1280,720);
            Save(camera,folder+"/frontier-overview-14.png",officialPosition,officialTarget,14f,1280,720);
            Save(camera,folder+"/frontier-overview-10.png",officialPosition,officialTarget,10f,1280,720);
            Save(camera,folder+"/frontier-overview-7.png",officialPosition,officialTarget,7f,1280,720);
            Save(camera,folder+"/frontier-overview-mobile.png",officialPosition,officialTarget,14f,390,844);

            var quarryShift=new Vector3(6.2f,-1.1f,-3.0f);
            Save(camera,folder+"/quarry-kit-10.png",officialPosition+quarryShift,officialTarget+quarryShift,10f,1280,720);
            Save(camera,folder+"/quarry-kit-7.png",officialPosition+quarryShift,officialTarget+quarryShift,7f,1280,720);
            Save(camera,folder+"/quarry-kit-mobile.png",officialPosition+quarryShift,officialTarget+quarryShift,10f,390,844);

            var routeShift=new Vector3(0f,-.35f,-2.2f);
            Save(camera,folder+"/route-kit-10.png",officialPosition+routeShift,officialTarget+routeShift,10f,1280,720);
            Save(camera,folder+"/route-kit-7.png",officialPosition+routeShift,officialTarget+routeShift,7f,1280,720);
            Save(camera,folder+"/route-kit-mobile.png",officialPosition+routeShift,officialTarget+routeShift,10f,390,844);

            // Isolated comparison candidate already present in repo: NatureStarterKit2.
            // This never mutates production Frontier; it only asks whether this older vegetation
            // family survives Eldoria's fixed 4X camera better than the rejected Slavic foliage.
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
            SceneSetup.SetupRenderPipeline();
            RenderSettings.ambientMode=UnityEngine.Rendering.AmbientMode.Flat;
            RenderSettings.ambientLight=new Color(.94f,.92f,.86f);
            RenderSettings.fog=true;
            RenderSettings.fogMode=FogMode.Linear;
            RenderSettings.fogColor=new Color(.68f,.68f,.63f);
            RenderSettings.fogStartDistance=36f;
            RenderSettings.fogEndDistance=110f;
            var compareSun=new GameObject("World compare sun").AddComponent<Light>();
            compareSun.type=LightType.Directional;
            compareSun.color=new Color(1f,.95f,.86f);
            compareSun.intensity=2.30f;
            compareSun.transform.rotation=Quaternion.Euler(48f,-32f,0);
            compareSun.shadows=LightShadows.Soft;
            compareSun.shadowStrength=.46f;

            var compareCamera=new GameObject("World compare camera").AddComponent<Camera>();
            compareCamera.orthographic=true;
            compareCamera.clearFlags=CameraClearFlags.SolidColor;
            compareCamera.backgroundColor=RenderSettings.fogColor;

            var treePaths=new[]{
                "Assets/NatureStarterKit2/Nature/tree01.prefab",
                "Assets/NatureStarterKit2/Nature/tree02.prefab",
                "Assets/NatureStarterKit2/Nature/tree03.prefab",
                "Assets/NatureStarterKit2/Nature/tree04.prefab"
            };
            var bushPaths=new[]{
                "Assets/NatureStarterKit2/Nature/bush01.prefab",
                "Assets/NatureStarterKit2/Nature/bush02.prefab",
                "Assets/NatureStarterKit2/Nature/bush03.prefab",
                "Assets/NatureStarterKit2/Nature/bush04.prefab",
                "Assets/NatureStarterKit2/Nature/bush05.prefab",
                "Assets/NatureStarterKit2/Nature/bush06.prefab"
            };
            var positions=new[]{
                new Vector3(-3.0f,0f,1.2f),new Vector3(-1.4f,0f,2.2f),
                new Vector3(.2f,0f,1.1f),new Vector3(1.7f,0f,2.0f),
                new Vector3(3.0f,0f,1.0f),new Vector3(-2.2f,0f,-.8f),
                new Vector3(.9f,0f,-.9f),new Vector3(2.4f,0f,-.5f)
            };
            for(int i=0;i<positions.Length;i++)
            {
                var prefab=AssetDatabase.LoadAssetAtPath<GameObject>(treePaths[i%treePaths.Length]);
                if(prefab==null)throw new System.Exception("NatureStarter tree missing: "+treePaths[i%treePaths.Length]);
                var tree=Object.Instantiate(prefab);
                tree.name="NatureStarter candidate tree "+(i+1);
                tree.transform.position=positions[i];
                tree.transform.rotation=Quaternion.Euler(0,i*41f,0);
                NormalizeNatureCandidate(tree,true);
                FitCandidate(tree,4.8f+(i%3)*.35f);
                foreach(var c in tree.GetComponentsInChildren<Collider>(true))c.enabled=false;
            }
            for(int i=0;i<6;i++)
            {
                var prefab=AssetDatabase.LoadAssetAtPath<GameObject>(bushPaths[i]);
                if(prefab==null)continue;
                var bush=Object.Instantiate(prefab);
                bush.name="NatureStarter candidate bush "+(i+1);
                bush.transform.position=new Vector3(-3.2f+i*1.25f,0f,-2.1f+(i%2)*.45f);
                bush.transform.rotation=Quaternion.Euler(0,i*53f,0);
                NormalizeNatureCandidate(bush,false);
                FitCandidate(bush,1.75f+(i%2)*.25f);
                foreach(var c in bush.GetComponentsInChildren<Collider>(true))c.enabled=false;
            }
            Save(compareCamera,folder+"/naturestarter-overview-18.png",new Vector3(20f,24f,-21f),new Vector3(0,0,0),18f,1280,720);
            Save(compareCamera,folder+"/naturestarter-overview-14.png",new Vector3(20f,24f,-21f),new Vector3(0,0,0),14f,1280,720);
            Save(compareCamera,folder+"/naturestarter-overview-10.png",new Vector3(20f,24f,-21f),new Vector3(0,0,0),10f,1280,720);
            Save(compareCamera,folder+"/naturestarter-overview-mobile.png",new Vector3(20f,24f,-21f),new Vector3(0,0,0),14f,390,844);

            CaptureNatureStarterCandidates(folder);

            File.WriteAllText(folder+"/world-map-metrics.json",
                "{\n"+
                "  \"schema_version\": 2,\n"+
                "  \"quarry_kit\": \"WorldResourceKit.QuarryResourcePocket\",\n"+
                "  \"quarry_active_renderers\": "+quarryRenderers+",\n"+
                "  \"route_kit\": \"WorldRouteKit.MarchRoute\",\n"+
                "  \"route_active_renderers\": "+routeRenderers+",\n"+
                "  \"visual_owns_gameplay_collision\": false,\n"+
                "  \"forest_hotspot_preserved\": true,\n"+
                "  \"source_family\": \"Slavic hard-surface subset + Eldoria procedural terrain\",\n"+
                "  \"tripo_credits\": 0\n"+
                "}\n");
            UnityEditor.EditorApplication.Exit(0);
        }

        static void CaptureMatchedWedge()
        {
            const string folder="WorldMapVisualFormulaCaptures";
            Directory.CreateDirectory(folder);
            string baseline=null;
            foreach(bool enabled in new[]{false,true})
            {
                EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
                SceneSetup.SetupRenderPipeline();
                VisualWorld.VisualIntegrationEnabled=enabled;
                VisualWorld.Create(false,new PlayerState{BastionLevel=3,SawmillLevel=1,BarracksLevel=1,CorruptionDiscovered=true,March=new MarchState{Phase="outbound",TargetId="forest-valoria",Troops=new ArmyRoster{ArcherT1=8}}});
                var signature=ValoriaVisualFormulaGate.CollisionSignature();
                if(!enabled)baseline=signature;
                else if(signature!=baseline)throw new System.Exception("4X visual integration altered authoritative colliders/hotspots.");
                var camera=Camera.main;string label=enabled?"after":"before";
                foreach(float zoom in new[]{18f,14f,10f,7f})
                    Save(camera,folder+"/world-"+label+"-"+zoom+".png",new Vector3(20,24,-21),new Vector3(0,0,1),zoom,1280,720);
                Save(camera,folder+"/world-"+label+"-mobile.png",new Vector3(20,24,-21),new Vector3(0,0,1),14,390,844);
                var shift=new Vector3(6,0,5);
                Save(camera,folder+"/world-threat-"+label+"-mobile.png",new Vector3(20,24,-21)+shift,new Vector3(0,0,1)+shift,14,390,844);
                var root=GameObject.Find("Frontier · integrated 4X visual layer");
                if(enabled)
                {
                    if(root==null)throw new System.Exception("4X visual layer missing.");
                    foreach(var c in root.GetComponentsInChildren<Collider>(true))if(c.enabled)throw new System.Exception("4X visual collider enabled.");
                    File.WriteAllText(folder+"/world-wedge-evidence.json","{\"camera_matched\":true,\"state_bastion\":3,\"collider_hotspot_signature_equal\":true,\"placeholder_families\":[\"Player City Kit v1\",\"Beast Kit v1\",\"March Representation Kit v1\"],\"tripo_credits\":0}");
                }
            }
            VisualWorld.VisualIntegrationEnabled=true;
        }

        static void CaptureNatureStarterCandidates(string folder)
        {
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
            SceneSetup.SetupRenderPipeline();
            RenderSettings.ambientMode=UnityEngine.Rendering.AmbientMode.Flat;
            RenderSettings.ambientLight=new Color(.82f,.82f,.80f);
            RenderSettings.fog=true;
            RenderSettings.fogMode=FogMode.Linear;
            RenderSettings.fogColor=new Color(.68f,.68f,.63f);
            RenderSettings.fogStartDistance=30f;
            RenderSettings.fogEndDistance=75f;

            var sun=new GameObject("Nature candidate sun").AddComponent<Light>();
            sun.type=LightType.Directional;
            sun.color=new Color(1f,.95f,.86f);
            sun.intensity=1.65f;
            sun.shadows=LightShadows.Soft;
            sun.shadowStrength=.50f;
            sun.transform.rotation=Quaternion.Euler(48f,-32f,0f);

            var camera=new GameObject("Nature candidate camera").AddComponent<Camera>();
            camera.orthographic=true;
            camera.backgroundColor=RenderSettings.fogColor;
            camera.clearFlags=CameraClearFlags.SolidColor;

            var paths=new[]{
                "Assets/NatureStarterKit2/Nature/tree01.prefab",
                "Assets/NatureStarterKit2/Nature/tree02.prefab",
                "Assets/NatureStarterKit2/Nature/tree03.prefab",
                "Assets/NatureStarterKit2/Nature/tree04.prefab",
                "Assets/NatureStarterKit2/Nature/bush01.prefab",
                "Assets/NatureStarterKit2/Nature/bush02.prefab",
                "Assets/NatureStarterKit2/Nature/bush03.prefab",
                "Assets/NatureStarterKit2/Nature/bush04.prefab"
            };

            for(int i=0;i<paths.Length;i++)
            {
                var prefab=AssetDatabase.LoadAssetAtPath<GameObject>(paths[i]);
                if(prefab==null)continue;
                var go=Object.Instantiate(prefab);
                go.name="NatureStarter candidate "+(i+1);
                float x=(i<4?-4.5f+(i*3.0f):-4.5f+((i-4)*3.0f));
                float z=i<4?1.6f:-2.3f;
                go.transform.position=new Vector3(x,0f,z);
                go.transform.rotation=Quaternion.Euler(0f,i*37f,0f);
                NormalizeNatureCandidate(go,i<4);
                FitCandidate(go,i<4?4.8f:2.0f);
                foreach(var c in go.GetComponentsInChildren<Collider>(true))c.enabled=false;
            }

            Save(camera,folder+"/naturestarter-candidates.png",
                new Vector3(11.5f,10.5f,-15.5f),new Vector3(0f,1.6f,0f),8.2f,1280,720);
        }

        static void NormalizeNatureCandidate(GameObject root,bool tree)
        {
            var lit=Shader.Find("Universal Render Pipeline/Lit");
            if(lit==null)return;
            foreach(var renderer in root.GetComponentsInChildren<Renderer>(true))
            {
                var mats=renderer.sharedMaterials;
                for(int i=0;i<mats.Length;i++)
                {
                    var source=mats[i];
                    if(source==null)continue;
                    var baseMap=source.HasProperty("_BaseMap")?source.GetTexture("_BaseMap"):
                        source.HasProperty("_MainTex")?source.GetTexture("_MainTex"):null;
                    var normal=source.HasProperty("_BumpMap")?source.GetTexture("_BumpMap"):null;
                    var mat=new Material(lit){name="Eldoria nature candidate · "+source.name};
                    if(baseMap!=null)mat.SetTexture("_BaseMap",baseMap);
                    if(normal!=null)
                    {
                        mat.SetTexture("_BumpMap",normal);
                        mat.EnableKeyword("_NORMALMAP");
                    }
                    mat.SetColor("_BaseColor",tree?new Color(.72f,.78f,.68f,1f):new Color(.68f,.76f,.64f,1f));
                    mat.SetFloat("_Metallic",0f);
                    mat.SetFloat("_Smoothness",.03f);
                    mat.SetFloat("_AlphaClip",1f);
                    mat.SetFloat("_Cutoff",.34f);
                    mat.EnableKeyword("_ALPHATEST_ON");
                    mat.renderQueue=(int)UnityEngine.Rendering.RenderQueue.AlphaTest;
                    mats[i]=mat;
                }
                renderer.sharedMaterials=mats;
            }
        }

        static void FitCandidate(GameObject root,float targetHeight)
        {
            var renderers=root.GetComponentsInChildren<Renderer>(true);
            if(renderers.Length==0)return;
            var bounds=renderers[0].bounds;
            for(int i=1;i<renderers.Length;i++)bounds.Encapsulate(renderers[i].bounds);
            if(bounds.size.y>.001f)root.transform.localScale*=targetHeight/bounds.size.y;
            renderers=root.GetComponentsInChildren<Renderer>(true);
            bounds=renderers[0].bounds;
            for(int i=1;i<renderers.Length;i++)bounds.Encapsulate(renderers[i].bounds);
            root.transform.position+=new Vector3(0f,-bounds.min.y,0f);
        }

        static void Save(Camera camera,string path,Vector3 position,Vector3 target,float size,int width,int height)
        {
            camera.transform.position=position;
            camera.transform.LookAt(target);
            camera.orthographic=true;
            camera.orthographicSize=size;
            camera.backgroundColor=RenderSettings.fogColor;
            var rt=new RenderTexture(width,height,24,RenderTextureFormat.ARGB32);
            var previous=RenderTexture.active;
            try
            {
                camera.targetTexture=rt;
                camera.Render();
                RenderTexture.active=rt;
                var image=new Texture2D(width,height,TextureFormat.RGB24,false);
                image.ReadPixels(new Rect(0,0,width,height),0,0);
                image.Apply();
                File.WriteAllBytes(path,image.EncodeToPNG());
                Object.DestroyImmediate(image);
            }
            finally
            {
                camera.targetTexture=null;
                RenderTexture.active=previous;
                rt.Release();
                Object.DestroyImmediate(rt);
            }
        }
    }
}
