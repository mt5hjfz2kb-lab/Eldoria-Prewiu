using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Eldoria.Domain;
using Eldoria.Presentation;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;

namespace Eldoria.EditorTools
{
    /// <summary>
    /// Zero-production-impact comparison of downloaded free world packs.
    /// This gate is intended to run in a temporary copy of the Unity project.
    /// </summary>
    public static class WorldPack4XComparisonGate
    {
        static readonly Vector3 OfficialPosition = new Vector3(20f,24f,-21f);
        static readonly Vector3 OfficialTarget = new Vector3(0f,0f,1f);
        static string Folder;

        public static void Capture()
        {
            SceneSetup.SetupRenderPipeline();
            Folder=Path.GetFullPath(Path.Combine(Application.dataPath,"..","WorldPack4XComparisonCaptures"));
            if(Directory.Exists(Folder))Directory.Delete(Folder,true);
            Directory.CreateDirectory(Folder);

            CaptureCurrentFrontier();
            CaptureNatureStarter();
            CaptureJermesa();
            CaptureHolotna();
            CaptureQuaterniusRuins();

            File.WriteAllText(Path.Combine(Folder,"comparison-summary.json"),
                "{\n"+
                "  \"schema_version\": 1,\n"+
                "  \"camera\": \"fixed Eldoria 4X isometric\",\n"+
                "  \"zoom_sizes\": [18,14,10,7],\n"+
                "  \"mobile\": \"390x844 at size 14\",\n"+
                "  \"production_modified\": false,\n"+
                "  \"tripo_credits\": 0\n"+
                "}\n");
            AssetDatabase.SaveAssets();
            EditorApplication.Exit(0);
        }

        static void CaptureCurrentFrontier()
        {
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
            var state=new PlayerState{
                BastionLevel=2,SawmillLevel=1,BarracksLevel=1,CorruptionDiscovered=true
            };
            VisualWorld.Create(false,state);
            var camera=Camera.main;
            if(camera==null)throw new Exception("Frontier camera missing.");
            CaptureZoomLadder(camera,"current-frontier");
            WriteMetrics("current-frontier",UnityEngine.Object.FindObjectsByType<Renderer>(FindObjectsSortMode.None));
        }

        static void CaptureNatureStarter()
        {
            PrepareCandidateScene(out var camera);
            var assets=new List<string>();
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
                "Assets/NatureStarterKit2/Nature/bush04.prefab"
            };
            for(int i=0;i<14;i++)
            {
                var path=treePaths[i%treePaths.Length];
                var p=new Vector3(-8f+(i%7)*2.55f,0f,-1.2f+(i/7)*4.1f+(i%2)*.55f);
                if(SpawnNormalized(path,"NatureStarter tree "+i,p,3.6f+(i%3)*.35f,17f+i*41f,null))assets.Add(path);
            }
            for(int i=0;i<10;i++)
            {
                var path=bushPaths[i%bushPaths.Length];
                var p=new Vector3(-7.4f+(i%5)*3.4f,0f,-3.4f+(i/5)*7.2f);
                if(SpawnNormalized(path,"NatureStarter bush "+i,p,1.05f+(i%2)*.16f,31f+i*47f,null))assets.Add(path);
            }
            AddNeutralGeography();
            CaptureZoomLadder(camera,"naturestarter");
            WriteMetrics("naturestarter",UnityEngine.Object.FindObjectsByType<Renderer>(FindObjectsSortMode.None),assets);
        }

        static void CaptureJermesa()
        {
            PrepareCandidateScene(out var camera);
            var assets=new List<string>();
            string basePath="Assets/Hill Rock Mountain Terrain/Prefab/";
            var mountain=basePath+"mountain_terrain_02.prefab";
            if(SpawnNormalized(mountain,"Jermesa mountain",new Vector3(0,0,6.0f),7.4f,0f,null))assets.Add(mountain);
            var rocks=new[]{"rock_set_01.prefab","rock_set_02.prefab","rock_set_03.prefab","rock_set_04.prefab"};
            for(int i=0;i<8;i++)
            {
                var path=basePath+rocks[i%rocks.Length];
                var p=new Vector3(-7.8f+(i%4)*5.0f,0f,-2.6f+(i/4)*5.1f);
                if(SpawnNormalized(path,"Jermesa rock "+i,p,1.5f+(i%3)*.32f,19f+i*37f,null))assets.Add(path);
            }
            var tree=basePath+"tree_02.prefab";
            for(int i=0;i<10;i++)
            {
                var p=new Vector3(-8.0f+(i%5)*3.9f,0f,-4.0f+(i/5)*4.0f);
                if(SpawnNormalized(tree,"Jermesa tree "+i,p,3.6f+(i%3)*.35f,11f+i*43f,null))assets.Add(tree);
            }
            AddRoute();
            CaptureZoomLadder(camera,"jermesa");
            WriteMetrics("jermesa",UnityEngine.Object.FindObjectsByType<Renderer>(FindObjectsSortMode.None),assets);
        }

        static void CaptureHolotna()
        {
            PrepareCandidateScene(out var camera);
            var all=AssetDatabase.FindAssets("t:GameObject")
                .Select(AssetDatabase.GUIDToAssetPath)
                .Where(p=>IsHolotnaPath(p))
                .Distinct().ToList();

            var mountain=Pick(all,"mountain","terrain","cliff");
            var rocks=PickMany(all,6,"rock","stone","cliff");
            var trees=PickMany(all,6,"tree","pine","fir");
            var bridge=Pick(all,"bridge");

            var assets=new List<string>();
            if(mountain!=null&&SpawnNormalized(mountain,"Holotna mountain",new Vector3(0,0,6.0f),7.4f,0f,null))assets.Add(mountain);
            for(int i=0;i<8;i++)
            {
                var path=rocks.Count>0?rocks[i%rocks.Count]:null;
                if(path==null)break;
                var p=new Vector3(-7.8f+(i%4)*5.0f,0f,-2.6f+(i/4)*5.1f);
                if(SpawnNormalized(path,"Holotna rock "+i,p,1.55f+(i%3)*.30f,23f+i*41f,null))assets.Add(path);
            }
            for(int i=0;i<12;i++)
            {
                if(trees.Count==0)break;
                var path=trees[i%trees.Count];
                var p=new Vector3(-8.4f+(i%6)*3.3f,0f,-4.0f+(i/6)*4.2f);
                if(SpawnNormalized(path,"Holotna tree "+i,p,3.6f+(i%3)*.38f,7f+i*47f,null))assets.Add(path);
            }
            if(bridge!=null&&SpawnNormalized(bridge,"Holotna bridge",new Vector3(0,.02f,-2.2f),1.8f,0f,null))assets.Add(bridge);
            AddRoute();
            CaptureZoomLadder(camera,"holotna");
            WriteMetrics("holotna",UnityEngine.Object.FindObjectsByType<Renderer>(FindObjectsSortMode.None),assets);
            File.WriteAllLines(Path.Combine(Folder,"holotna-discovered-assets.txt"),all);
        }

        static void CaptureQuaterniusRuins()
        {
            PrepareCandidateScene(out var camera);
            var root="Assets/WorldPackCompare/Quaternius/FBX/";
            var choices=new[]{
                root+"Arch_Gothic.fbx",
                root+"Wall_Broken.fbx",
                root+"Wall_Overgrown.fbx",
                root+"Wall_ArchRound_Broken.fbx",
                root+"Column_Round.fbx",
                root+"Column_Square.fbx",
                root+"BridgeSection.fbx"
            };
            var assets=new List<string>();
            var layout=new[]{
                new Vector3(-2.6f,0,2.0f),new Vector3(0f,0,2.1f),new Vector3(2.6f,0,2.0f),
                new Vector3(-3.6f,0,-.6f),new Vector3(3.6f,0,-.6f),
                new Vector3(-1.4f,0,-2.1f),new Vector3(1.4f,0,-2.1f)
            };
            for(int i=0;i<choices.Length;i++)
                if(SpawnNormalized(choices[i],"Quaternius ruin "+i,layout[i],2.8f+(i%2)*.45f,i*19f,NeutralStoneMaterial()))
                    assets.Add(choices[i]);
            AddRoute();
            CaptureZoomLadder(camera,"quaternius-ruins");
            WriteMetrics("quaternius-ruins",UnityEngine.Object.FindObjectsByType<Renderer>(FindObjectsSortMode.None),assets);
        }

        static void PrepareCandidateScene(out Camera camera)
        {
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
            SceneSetup.SetupRenderPipeline();
            RenderSettings.ambientMode=AmbientMode.Flat;
            RenderSettings.ambientLight=new Color(.78f,.77f,.72f);
            RenderSettings.fog=true;
            RenderSettings.fogMode=FogMode.Linear;
            RenderSettings.fogColor=new Color(.61f,.62f,.58f);
            RenderSettings.fogStartDistance=34f;
            RenderSettings.fogEndDistance=105f;
            var sun=new GameObject("World compare sun").AddComponent<Light>();
            sun.type=LightType.Directional;
            sun.color=new Color(1f,.94f,.84f);
            sun.intensity=1.55f;
            sun.shadows=LightShadows.Soft;
            sun.shadowStrength=.48f;
            sun.transform.rotation=Quaternion.Euler(48f,-32f,0f);

            camera=new GameObject("World compare camera").AddComponent<Camera>();
            camera.orthographic=true;
            camera.backgroundColor=RenderSettings.fogColor;
            camera.clearFlags=CameraClearFlags.SolidColor;

            var ground=GameObject.CreatePrimitive(PrimitiveType.Cube);
            ground.name="Comparison terrain";
            ground.transform.position=new Vector3(0,-.28f,1.5f);
            ground.transform.localScale=new Vector3(28f,.5f,23f);
            ground.GetComponent<Renderer>().sharedMaterial=EarthMaterial();
            StripCollider(ground);
        }

        static void AddNeutralGeography()
        {
            for(int i=0;i<6;i++)
            {
                var rock=GameObject.CreatePrimitive(PrimitiveType.Sphere);
                rock.name="Neutral geography "+i;
                rock.transform.position=new Vector3(-9f+i*3.7f,.28f,5.7f+(i%2)*.5f);
                rock.transform.localScale=new Vector3(2.4f,1.2f,1.8f)*(1f+(i%3)*.12f);
                rock.GetComponent<Renderer>().sharedMaterial=NeutralStoneMaterial();
                StripCollider(rock);
            }
            AddRoute();
        }

        static void AddRoute()
        {
            var road=GameObject.CreatePrimitive(PrimitiveType.Cube);
            road.name="4X readability route";
            road.transform.position=new Vector3(0,.015f,-1.1f);
            road.transform.localScale=new Vector3(2.1f,.06f,13.4f);
            road.GetComponent<Renderer>().sharedMaterial=RoadMaterial();
            StripCollider(road);
        }

        static bool SpawnNormalized(string path,string name,Vector3 position,float targetHeight,float yaw,Material overrideMaterial)
        {
            if(String.IsNullOrEmpty(path))return false;
            var prefab=AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if(prefab==null)return false;
            var go=UnityEngine.Object.Instantiate(prefab);
            go.name=name;
            go.transform.position=position;
            go.transform.rotation=Quaternion.Euler(0,yaw,0);
            StripColliders(go);
            var renderers=go.GetComponentsInChildren<Renderer>(true);
            if(renderers.Length==0){UnityEngine.Object.DestroyImmediate(go);return false;}
            var b=BoundsOf(renderers);
            var h=Mathf.Max(.001f,b.size.y);
            go.transform.localScale*=targetHeight/h;
            renderers=go.GetComponentsInChildren<Renderer>(true);
            b=BoundsOf(renderers);
            go.transform.position+=new Vector3(position.x-b.center.x,-b.min.y,position.z-b.center.z);
            if(overrideMaterial!=null)
                foreach(var r in renderers)
                {
                    var mats=new Material[r.sharedMaterials.Length];
                    for(int i=0;i<mats.Length;i++)mats[i]=overrideMaterial;
                    r.sharedMaterials=mats;
                }
            return true;
        }

        static Bounds BoundsOf(Renderer[] rs)
        {
            var b=rs[0].bounds;
            for(int i=1;i<rs.Length;i++)b.Encapsulate(rs[i].bounds);
            return b;
        }

        static void StripColliders(GameObject go)
        {
            if(go==null)return;
            foreach(var c in go.GetComponentsInChildren<Collider>(true))c.enabled=false;
        }

        static void StripCollider(GameObject go)
        {
            var c=go.GetComponent<Collider>();
            if(c!=null)UnityEngine.Object.DestroyImmediate(c);
        }

        static bool IsHolotnaPath(string path)
        {
            if(String.IsNullOrEmpty(path))return false;
            var p=path.ToLowerInvariant();
            if(!p.StartsWith("assets/"))return false;
            if(p.Contains("hill rock mountain terrain"))return false;
            if(p.Contains("naturestarterkit2"))return false;
            if(p.Contains("eldoria/"))return false;
            if(p.Contains("emaceart/"))return false;
            if(p.Contains("worldpackcompare/quaternius"))return false;
            return p.Contains("mountain")||p.Contains("holotna");
        }

        static string Pick(List<string> paths,params string[] words)
        {
            foreach(var w in words)
            {
                var match=paths.FirstOrDefault(p=>
                    (p.EndsWith(".prefab",StringComparison.OrdinalIgnoreCase)||
                     p.EndsWith(".fbx",StringComparison.OrdinalIgnoreCase)) &&
                    Path.GetFileNameWithoutExtension(p).IndexOf(w,StringComparison.OrdinalIgnoreCase)>=0);
                if(match!=null)return match;
            }
            return paths.FirstOrDefault(p=>p.EndsWith(".prefab",StringComparison.OrdinalIgnoreCase));
        }

        static List<string> PickMany(List<string> paths,int max,params string[] words)
        {
            var result=new List<string>();
            foreach(var p in paths)
            {
                if(!(p.EndsWith(".prefab",StringComparison.OrdinalIgnoreCase)||p.EndsWith(".fbx",StringComparison.OrdinalIgnoreCase)))continue;
                var n=Path.GetFileNameWithoutExtension(p);
                if(words.Any(w=>n.IndexOf(w,StringComparison.OrdinalIgnoreCase)>=0))
                {
                    result.Add(p);
                    if(result.Count>=max)break;
                }
            }
            return result;
        }

        static void CaptureZoomLadder(Camera camera,string prefix)
        {
            Save(camera,prefix+"-18.png",18f,1280,720);
            Save(camera,prefix+"-14.png",14f,1280,720);
            Save(camera,prefix+"-10.png",10f,1280,720);
            Save(camera,prefix+"-7.png",7f,1280,720);
            Save(camera,prefix+"-mobile.png",14f,390,844);
        }

        static void Save(Camera camera,string filename,float size,int width,int height)
        {
            camera.transform.position=OfficialPosition;
            camera.transform.LookAt(OfficialTarget);
            camera.orthographic=true;
            camera.orthographicSize=size;
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
                File.WriteAllBytes(Path.Combine(Folder,filename),image.EncodeToPNG());
                UnityEngine.Object.DestroyImmediate(image);
            }
            finally
            {
                camera.targetTexture=null;
                RenderTexture.active=previous;
                rt.Release();
                UnityEngine.Object.DestroyImmediate(rt);
            }
        }

        static void WriteMetrics(string id,IEnumerable<Renderer> renderers,IEnumerable<string> assets=null)
        {
            long triangles=0;
            var rendererList=renderers.Where(r=>r!=null&&r.enabled).ToList();
            var materials=new HashSet<Material>();
            foreach(var r in rendererList)
            {
                foreach(var m in r.sharedMaterials)if(m!=null)materials.Add(m);
                var mf=r.GetComponent<MeshFilter>();
                if(mf!=null&&mf.sharedMesh!=null)
                    triangles+=mf.sharedMesh.triangles.LongLength/3;
                var smr=r as SkinnedMeshRenderer;
                if(smr!=null&&smr.sharedMesh!=null)
                    triangles+=smr.sharedMesh.triangles.LongLength/3;
            }
            var assetList=(assets??Enumerable.Empty<string>()).Distinct().OrderBy(x=>x).ToArray();
            var escaped=assetList.Select(a=>"    \""+a.Replace("\\","/").Replace("\"","\\\"")+"\"");
            File.WriteAllText(Path.Combine(Folder,id+"-metrics.json"),
                "{\n"+
                "  \"id\": \""+id+"\",\n"+
                "  \"active_renderers\": "+rendererList.Count+",\n"+
                "  \"unique_materials\": "+materials.Count+",\n"+
                "  \"triangles\": "+triangles+",\n"+
                "  \"selected_assets\": [\n"+String.Join(",\n",escaped)+"\n  ]\n"+
                "}\n");
        }

        static Material EarthMaterial()=>FlatMaterial("WorldCompareEarth",new Color(.24f,.22f,.17f),.08f);
        static Material RoadMaterial()=>FlatMaterial("WorldCompareRoad",new Color(.36f,.29f,.20f),.18f);
        static Material NeutralStoneMaterial()=>FlatMaterial("WorldCompareStone",new Color(.43f,.43f,.40f),.15f);

        static Material FlatMaterial(string name,Color color,float smoothness)
        {
            var shader=Shader.Find("Universal Render Pipeline/Lit")??Shader.Find("Standard");
            var m=new Material(shader){name=name,color=color};
            if(m.HasProperty("_BaseColor"))m.SetColor("_BaseColor",color);
            if(m.HasProperty("_Smoothness"))m.SetFloat("_Smoothness",smoothness);
            return m;
        }
    }
}
