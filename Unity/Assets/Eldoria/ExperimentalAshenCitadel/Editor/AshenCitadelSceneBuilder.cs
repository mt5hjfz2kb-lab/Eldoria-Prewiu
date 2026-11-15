// Authored Unity scene assembly; never edits production scenes, HUD or build settings.
// Run in Unity Editor: Eldoria / Experiments / Ashen Citadel / Build isolated scene
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using Eldoria.ExperimentalAshenCitadel;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

namespace Eldoria.ExperimentalAshenCitadel.Editor
{
    public static class AshenCitadelSceneBuilder
    {
        private const string Root="Assets/Eldoria/ExperimentalAshenCitadel";
        private const string ScenePath=Root+"/Scenes/AshenCitadel.unity";
        private const string MeshPath=Root+"/Meshes/";
        private const string MaterialPath=Root+"/Materials/";
        private static readonly CultureInfo Invariant=CultureInfo.InvariantCulture;
        private static readonly Dictionary<string,Material> Colors=new Dictionary<string,Material>(StringComparer.OrdinalIgnoreCase);

        [MenuItem("Eldoria/Experiments/Ashen Citadel/Build isolated scene")]
        public static void Build()
        {
            Directory.CreateDirectory(Root+"/Scenes");
            Directory.CreateDirectory(MaterialPath);
            AssetDatabase.Refresh();
            CreateMaterials();
            var scene=EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
            RenderSettings.ambientMode=AmbientMode.Trilight;
            RenderSettings.ambientSkyColor=new Color(.32f,.34f,.34f);
            RenderSettings.ambientEquatorColor=new Color(.22f,.25f,.24f);
            RenderSettings.ambientGroundColor=new Color(.095f,.10f,.09f);
            RenderSettings.fog=true;RenderSettings.fogColor=new Color(.26f,.29f,.29f);
            RenderSettings.fogMode=FogMode.Linear;RenderSettings.fogStartDistance=48;RenderSettings.fogEndDistance=135;

            var key=new GameObject("Sun · amber key");
            var sun=key.AddComponent<Light>();
            sun.type=LightType.Directional;sun.intensity=1.35f;
            sun.color=new Color(1f,.84f,.69f);
            sun.shadows=LightShadows.Soft;sun.shadowStrength=.55f;
            sun.transform.rotation=Quaternion.Euler(48,-35,0);
            var fill=new GameObject("Sky · cold fill").AddComponent<Light>();
            fill.type=LightType.Directional;fill.intensity=.23f;fill.color=new Color(.54f,.67f,.77f);
            fill.transform.rotation=Quaternion.Euler(56,145,0);
            fill.shadows=LightShadows.None;

            var cameraGO=new GameObject("Ashen · Player camera");
            cameraGO.tag="MainCamera";
            var camera=cameraGO.AddComponent<Camera>();
            camera.orthographic=true;camera.orthographicSize=20.6f;
            camera.clearFlags=CameraClearFlags.SolidColor;
            camera.backgroundColor=new Color(.25f,.28f,.28f);
            camera.allowHDR=false;camera.allowMSAA=true;camera.depth=0;
            cameraGO.AddComponent<AudioListener>();
            cameraGO.transform.position=new Vector3(20,24,-28);
            cameraGO.transform.LookAt(new Vector3(0,2.5f,0));

            var sceneRoot=new GameObject("Ashen City · authored hierarchy");
            AddGround(sceneRoot.transform,0,0,146,142);
            var stagesRoot=new GameObject("Bastion stage variants");stagesRoot.transform.SetParent(sceneRoot.transform,false);
            var stages=new GameObject[5];
            for(int n=1;n<=5;n++)
            {
                var imported=Import("AshenBastion_"+n,stagesRoot.transform);
                imported.name="Stage_"+n;
                BindClick(imported,"Bastión","bastion","building",
                    new Vector3(0,3.4f,0),new Vector3(18.6f,8f,17.6f));
                imported.SetActive(n==1);stages[n-1]=imported;
            }
            var sawmill=Import("Ashen_Sawmill",sceneRoot.transform);
            sawmill.transform.position=new Vector3(-13.4f,.08f,-4.5f);
            BindClick(sawmill,"Aserradero","sawmill","building",new Vector3(0,2f,0),new Vector3(5.1f,5f,5.1f));
            var barracks=Import("Ashen_Barracks",sceneRoot.transform);
            barracks.transform.position=new Vector3(13.4f,.08f,-4.5f);
            BindClick(barracks,"Cuartel","barracks","building",new Vector3(0,2f,0),new Vector3(5.1f,5f,5.1f));
            var granary=Import("Ashen_Granary",sceneRoot.transform);
            granary.transform.position=new Vector3(-13.4f,.08f,4.8f);
            BindClick(granary,"Granero","granary","building",new Vector3(0,2f,0),new Vector3(5.1f,5f,5.1f));
            var stoneworks=Import("Ashen_Stoneworks",sceneRoot.transform);
            stoneworks.transform.position=new Vector3(13.4f,.08f,4.8f);
            BindClick(stoneworks,"Taller de piedra","stoneworks","building",new Vector3(0,2f,0),new Vector3(5.1f,5f,5.1f));
            // Distinctive landscape frame, sparse around the playable fortress.
            var treePrefab=AssetDatabase.LoadAssetAtPath<GameObject>(MeshPath+"Ashen_Pine.obj");
            var rockPrefab=AssetDatabase.LoadAssetAtPath<GameObject>(MeshPath+"Ashen_Rock.obj");
            if(!treePrefab||!rockPrefab)throw new InvalidOperationException("Missing authored flora assets");
            var random=new System.Random(1795);
            // An organic silhouette instead of a circular row of identical trees.
            // Conifers form small pockets along the ravine, never on the main road.
            for(int i=0;i<86;i++)
            {
                float angle=i*2.39996323f+Mathf.Sin(i*1.13f)*.20f;
                float radius=17.5f+Mathf.Sqrt((i+.5f)/86f)*24f;
                var pos=new Vector3(Mathf.Cos(angle)*radius,0,Mathf.Sin(angle)*radius*.80f);
                if(Mathf.Abs(pos.x)<5f && pos.z<0)continue;
                var tree=ImportPrefab(treePrefab,sceneRoot.transform,"Pine · "+i);
                tree.transform.position=pos;
                tree.transform.localScale=new Vector3(.77f+.55f*(float)random.NextDouble(),
                    .72f+.72f*(float)random.NextDouble(),.77f+.55f*(float)random.NextDouble());
                tree.transform.rotation=Quaternion.Euler(0,(i*47f)%360f,0);
            }
            for(int i=0;i<27;i++)
            {
                float angle=i*2.349963f;
                float radius=19f+20f*(float)random.NextDouble();
                var rock=ImportPrefab(rockPrefab,sceneRoot.transform,"Natural fieldstone · "+i);
                rock.transform.position=new Vector3(Mathf.Cos(angle)*radius,-.04f,Mathf.Sin(angle)*radius*.87f);
                rock.transform.localScale=Vector3.one*Mathf.Lerp(.36f,1.00f,(float)random.NextDouble());
                rock.transform.rotation=Quaternion.Euler(0,(i*83f)%360f,0);
            }
            var world=new GameObject("Ashen Frontier · authored experiment");
            AddGround(world.transform,0,55,146,148);
            // Three readable regions with authored meshes; not imported legacy landmarks.
            for(int i=0;i<30;i++)
            {
                float x=-25+(i%8)*3.2f;
                float z=53+(i/8)*5.2f;
                if(x>-6f && x<8f)continue;
                var pine=ImportPrefab(treePrefab,world.transform,"Forest · "+i);
                pine.transform.position=new Vector3(x,0,z);
                pine.transform.localScale=Vector3.one*(.7f+(i%4)*.10f);
            }
            for(int i=0;i<13;i++)
            {
                var stone=ImportPrefab(rockPrefab,world.transform,"Quarry · "+i);
                stone.transform.position=new Vector3(11+(i%4)*2.7f,-.04f,43+(i/4)*2.5f);
                stone.transform.localScale=Vector3.one*(.45f+i%4*.18f);
            }
            AddRift(world.transform,new Vector3(13,.07f,73));
            AddRouteMarkers(world.transform);
            world.SetActive(false);

            var controller=new GameObject("ASHEN · authoritative game adapter").AddComponent<AshenCitadelExperience>();
            controller.Configure(stages,sawmill,barracks,granary,stoneworks,sceneRoot,world,camera);
            if(!EditorSceneManager.SaveScene(scene,ScenePath))
                throw new InvalidOperationException("Could not save new isolated Unity scene");
            AssetDatabase.SaveAssets();AssetDatabase.Refresh();
            Debug.Log("ASHEN_ART_SCENE_BUILT path="+ScenePath+" stages=5 auxiliary=4 world=yes");
        }
        // Coarse author-time proxies, no cooked mesh colliders in the mobile runtime.
        private static void BindClick(GameObject target,string title,string id,string kind,
            Vector3 localCenter,Vector3 localSize)
        {
            var collider=target.AddComponent<BoxCollider>();
            collider.center=localCenter;collider.size=localSize;collider.isTrigger=true;
            target.AddComponent<AshenCitadelPickTarget>().Configure(title,id,kind);
        }

        private static GameObject Import(string name,Transform parent)
        {
            var asset=AssetDatabase.LoadAssetAtPath<GameObject>(MeshPath+name+".obj");
            if(asset==null)throw new FileNotFoundException("No imported OBJ: "+name);
            return ImportPrefab(asset,parent,name);
        }
        private static GameObject ImportPrefab(GameObject model,Transform parent,string name)
        {
            var go=(GameObject)PrefabUtility.InstantiatePrefab(model);
            go.name=name;go.transform.SetParent(parent,false);
            int count=0;
            foreach(var r in go.GetComponentsInChildren<MeshRenderer>(true))
            {
                var mats=r.sharedMaterials;
                for(int i=0;i<mats.Length;i++)
                {
                    var src=mats[i]?.name??"";
                    string id=Colors.Keys.OrderByDescending(k=>k.Length).FirstOrDefault(
                        key=>src.Equals(key,StringComparison.OrdinalIgnoreCase)||
                             src.StartsWith(key+" ",StringComparison.OrdinalIgnoreCase)||
                             src.EndsWith(":"+key,StringComparison.OrdinalIgnoreCase));
                    if(id==null)
                        throw new InvalidOperationException("Unknown imported material in "+name+": '"+src+
                            "'. Fix OBJ/MTL binding; do not hide magenta with a fallback.");
                    mats[i]=Colors[id];count++;
                }
                r.sharedMaterials=mats;r.shadowCastingMode=ShadowCastingMode.On;r.receiveShadows=true;
            }
            if(count==0)throw new InvalidOperationException("No MeshRenderer/material on "+name);
            return go;
        }
        private static void CreateMaterials()
        {
            Colors.Clear();
            string assetPath=MeshPath+"AshenPalette.mtl";
            if(!File.Exists(assetPath))throw new FileNotFoundException(assetPath);
            var shader=Shader.Find("Universal Render Pipeline/Lit");
            if(shader==null)throw new InvalidOperationException("URP Lit shader missing; no fallback/pink renders accepted");
            string current=null;
            foreach(var raw in File.ReadAllLines(assetPath))
            {
                var line=raw.Trim();
                if(line.StartsWith("newmtl ",StringComparison.Ordinal)){current=line.Substring(7).Trim();continue;}
                if(!line.StartsWith("Kd ",StringComparison.Ordinal)||string.IsNullOrEmpty(current))continue;
                var parts=line.Substring(3).Split(new[]{' '},StringSplitOptions.RemoveEmptyEntries);
                if(parts.Length!=3)throw new InvalidDataException("Bad Kd for "+current);
                var col=new Color(float.Parse(parts[0],Invariant),float.Parse(parts[1],Invariant),float.Parse(parts[2],Invariant),1);
                var mPath=MaterialPath+current+".mat";
                var mat=AssetDatabase.LoadAssetAtPath<Material>(mPath);
                if(!mat){mat=new Material(shader){name="Ashen "+current};AssetDatabase.CreateAsset(mat,mPath);}
                mat.shader=shader;mat.SetColor("_BaseColor",col);
                // Match the actual Blender source: six UV-mapped 128 px reusable
                // artist-authored textures, multiplied by this palette color.
                var textureFamily=
                    new[]{"basalt","stone","stoneLight","cut","plaster"}.Contains(current)?"Stone":
                    new[]{"roof","slate"}.Contains(current)?"Roof":
                    new[]{"wood","oak"}.Contains(current)?"Wood":
                    new[]{"soil","moss","leaf","leafLight"}.Contains(current)?"Moss":
                    new[]{"iron","brass","warm"}.Contains(current)?"Metal":
                    new[]{"banner","violet"}.Contains(current)?"Cloth":null;
                if(textureFamily==null)throw new InvalidDataException("Unknown source palette key: "+current);
                var texturePath=Root+"/Textures/Ashen"+textureFamily+".png";
                var texture=AssetDatabase.LoadAssetAtPath<Texture2D>(texturePath);
                if(texture==null)throw new FileNotFoundException("Required authored texture not imported: "+texturePath);
                mat.SetTexture("_BaseMap",texture);
                mat.SetFloat("_Smoothness",current=="brass"?.35f:.09f);
                mat.SetFloat("_Metallic",current=="brass"?.42f:.01f);
                EditorUtility.SetDirty(mat);Colors[current]=mat;
            }
            if(Colors.Count<16)throw new InvalidDataException("Incomplete authored material palette: "+Colors.Count);
            AssetDatabase.SaveAssets();
        }
        // Contiguous terrain extends beyond every portrait and landscape camera
        // instead of exposing the rectangular edge of a tabletop city.
        private static void AddGround(Transform root,float x,float z,float w,float d)
        {
            var floor=GameObject.CreatePrimitive(PrimitiveType.Cube);
            floor.name="Deep valley · continuous terrain";
            floor.transform.SetParent(root,false);
            floor.transform.position=new Vector3(x,-.36f,z);
            floor.transform.localScale=new Vector3(w,.60f,d);
            floor.GetComponent<Renderer>().sharedMaterial=Colors["moss"];
            UnityEngine.Object.DestroyImmediate(floor.GetComponent<Collider>());

            // An authored winding causeway, not a single long rectangular strip.
            // These vertices are saved in the experimental Unity scene/mesh asset.
            var vertices=new List<Vector3>();
            var triangles=new List<int>();
            for(int i=0;i<=22;i++)
            {
                float t=i/22f;
                float at=z-34f+37f*t;
                float drift=Mathf.Sin(t*4.20f)*.57f+Mathf.Sin(t*10.7f)*.12f;
                float width=2.30f+.32f*Mathf.Sin(t*6.31f);
                vertices.Add(new Vector3(x+drift-width,.035f,at));
                vertices.Add(new Vector3(x+drift+width,.035f,at));
                if(i==0)continue;
                int previous=(i-1)*2,current=i*2;
                triangles.Add(previous);triangles.Add(current);triangles.Add(current+1);
                triangles.Add(previous);triangles.Add(current+1);triangles.Add(previous+1);
            }
            var roadMesh=new Mesh{name="Ashen · soft-curved worn approach"};
            roadMesh.SetVertices(vertices);roadMesh.SetTriangles(triangles,0);
            roadMesh.RecalculateNormals();roadMesh.RecalculateBounds();
            var path=Root+"/Meshes/AshenRoadMesh.asset";
            var existing=AssetDatabase.LoadAssetAtPath<Mesh>(path);
            if(existing==null){AssetDatabase.CreateAsset(roadMesh,path);existing=roadMesh;}
            else{EditorUtility.CopySerialized(roadMesh,existing);UnityEngine.Object.DestroyImmediate(roadMesh);}
            var road=new GameObject("Stone-dust road · terrain embedded");
            road.transform.SetParent(root,false);
            road.AddComponent<MeshFilter>().sharedMesh=existing;
            road.AddComponent<MeshRenderer>().sharedMaterial=Colors["soil"];

            // Small irregular stone islands give scale without outlining a square.
            var rng=new System.Random(3719+(int)z);
            for(int i=0;i<24;i++)
            {
                float progress=(float)rng.NextDouble();
                float zz=z-29f+29f*progress;
                float xx=x+Mathf.Sin(progress*4.20f)*.57f+
                    (rng.Next(2)==0?-1f:1f)*Mathf.Lerp(1.9f,4.25f,(float)rng.NextDouble());
                var pebble=GameObject.CreatePrimitive(PrimitiveType.Cube);
                pebble.name="Weathered route stone · "+i;
                pebble.transform.SetParent(root,false);
                pebble.transform.position=new Vector3(xx,.039f,zz);
                pebble.transform.rotation=Quaternion.Euler(0,(float)rng.NextDouble()*180f,0);
                pebble.transform.localScale=new Vector3(.22f+.27f*(float)rng.NextDouble(),
                    .048f,.37f+.25f*(float)rng.NextDouble());
                pebble.GetComponent<Renderer>().sharedMaterial=Colors["cut"];
                UnityEngine.Object.DestroyImmediate(pebble.GetComponent<Collider>());
            }
        }
        private static void AddRift(Transform parent,Vector3 center)
        {
            var root=new GameObject("BRECHA · geologic incision");root.transform.SetParent(parent,false);
            root.transform.position=center;
            // A discontinuous recessed seam; no upright neon totem.
            var vertices=new List<Vector3>();
            var tris=new List<int>();
            var points=new[]{new Vector3(-4,0,-3),new Vector3(-2.5f,0,-1.6f),
                new Vector3(-2.8f,0,.6f),new Vector3(-1.3f,0,2.2f),
                new Vector3(.3f,0,3.8f),new Vector3(2.2f,0,5.8f)};
            for(int i=0;i<points.Length;i++)
            {
                var p=points[i];float w=.33f+(i%3)*.10f;
                vertices.Add(p+new Vector3(-w,.04f,0));
                vertices.Add(p+new Vector3(w,.04f,0));
                vertices.Add(p+new Vector3(-w,-.24f,0));
                vertices.Add(p+new Vector3(w,-.24f,0));
                if(i==0)continue;
                int a=(i-1)*4,b=i*4;
                foreach(var tri in new[]{a,a+2,b+2,a,b+2,b,a+1,b+1,b+3,a+1,b+3,a+3})
                    tris.Add(tri);
            }
            var mesh=new Mesh{name="Ashen · fissure recessed geometry",indexFormat=IndexFormat.UInt16};
            mesh.SetVertices(vertices);mesh.SetTriangles(tris,0);mesh.RecalculateNormals();
            var assetPath=Root+"/Meshes/AshenFissureMesh.asset";
            var prior=AssetDatabase.LoadAssetAtPath<Mesh>(assetPath);
            if(prior==null){AssetDatabase.CreateAsset(mesh,assetPath);prior=mesh;}
            else{EditorUtility.CopySerialized(mesh,prior);UnityEngine.Object.DestroyImmediate(mesh);}
            var go=new GameObject("Fractured seam");go.transform.SetParent(root.transform,false);
            go.AddComponent<MeshFilter>().sharedMesh=prior;
            go.AddComponent<MeshRenderer>().sharedMaterial=Colors["violet"];
            for(int i=0;i<12;i++)
            {
                var slab=GameObject.CreatePrimitive(PrimitiveType.Cube);
                slab.name="Rift · fractured basalt lip";slab.transform.SetParent(root.transform,false);
                slab.transform.localPosition=new Vector3(-4+i*.54f,.04f,-3+i*.72f);
                slab.transform.localRotation=Quaternion.Euler(0,37+i*17,0);
                slab.transform.localScale=new Vector3(.8f,.14f,.38f);
                slab.GetComponent<Renderer>().sharedMaterial=Colors[i%3==0?"cut":"basalt"];
                UnityEngine.Object.DestroyImmediate(slab.GetComponent<Collider>());
            }
        }
        private static void AddRouteMarkers(Transform parent)
        {
            var milestones=new[]{
                new Vector3(-15,.05f,66),new Vector3(15,.05f,48),
                new Vector3(6,.05f,60),new Vector3(17,.05f,67),
                new Vector3(13,.05f,73)};
            for(int i=0;i<milestones.Length;i++)
            {
                var obj=GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                obj.name="Route landmark "+i;
                obj.transform.SetParent(parent,false);
                obj.transform.position=milestones[i];
                obj.transform.localScale=new Vector3(.84f,.14f,.84f);
                obj.GetComponent<Renderer>().sharedMaterial=Colors[i==4?"violet":"stoneLight"];
                var landmark=obj.AddComponent<AshenCitadelPickTarget>();
                string[] titles={"Bosque","Cantera","Explorador corrupto","Engendro","Brecha"};
                string[] ids={"forest-valoria","quarry-valoria","corrupt-scout","engendro-valoria","fissure-valoria"};
                string[] kinds={"gather","gather","fight","fight","fissure"};
                landmark.Configure(titles[i],ids[i],kinds[i]);
                var collider=obj.GetComponent<Collider>();
                if(collider!=null)collider.isTrigger=true;
            }
        }

        // QA artwork only: switches five distinct mesh variants in a SINGLE
        // scene/camera without changing PlayerState or pretending the player advanced.
        [MenuItem("Eldoria/Experiments/Ashen Citadel/Capture five levels")]
        public static void CaptureFiveLevels()
        {
            if(!File.Exists(ScenePath))Build();
            var scene=EditorSceneManager.OpenScene(ScenePath,OpenSceneMode.Single);
            var stages=GameObject.Find("Bastion stage variants");
            var camera=GameObject.Find("Ashen · Player camera")?.GetComponent<Camera>();
            if(stages==null||camera==null)throw new InvalidOperationException("Experimental scene incomplete");
            var outDir=Path.GetFullPath(Path.Combine(UnityEngine.Application.dataPath,"..","ArtEvidence"));
            Directory.CreateDirectory(outDir);
            foreach(var viewport in new[]{new Vector2Int(390,844),new Vector2Int(1600,900)})
            {
                for(int l=1;l<=5;l++)
                {
                    for(int j=1;j<=5;j++)
                    {
                        var t=stages.transform.Find("Stage_"+j);
                        if(t==null)throw new InvalidOperationException("Missing authored stage "+j);
                        t.gameObject.SetActive(j==l);
                    }
                    camera.orthographicSize=viewport.y>viewport.x?20.6f:14f;
                    var rt=new RenderTexture(viewport.x,viewport.y,24,RenderTextureFormat.ARGB32);
                    camera.targetTexture=rt;camera.aspect=(float)viewport.x/viewport.y;
                    RenderTexture.active=rt;camera.Render();
                    var image=new Texture2D(viewport.x,viewport.y,TextureFormat.RGB24,false);
                    image.ReadPixels(new Rect(0,0,viewport.x,viewport.y),0,0);image.Apply(false);
                    var pixels=image.GetPixels32();
                    int pink=0;
                    double brightness=0,brightnessSquare=0;
                    foreach(var c in pixels)
                    {
                        if(c.r>205 && c.b>180 && c.g<80)pink++;
                        double v=(.2126*c.r+.7152*c.g+.0722*c.b);
                        brightness+=v;brightnessSquare+=v*v;
                    }
                    float pct=100f*pink/pixels.Length;
                    double mean=brightness/pixels.Length;
                    double variance=brightnessSquare/pixels.Length-mean*mean;
                    if(pct>.5f)throw new InvalidOperationException("PINK SHADER "+pct.ToString("F2",Invariant)+"% on Bastion "+l);
                    if(mean<14||variance<60)
                        throw new InvalidOperationException("Blank/flat camera render. Mean="+mean.ToString("F2",Invariant)+
                            " variance="+variance.ToString("F2",Invariant)+". No art certification.");
                    var name="Ashen_Bastion_"+l+"_"+viewport.x+"x"+viewport.y+".png";
                    File.WriteAllBytes(Path.Combine(outDir,name),image.EncodeToPNG());
                    Debug.Log("ASHEN_CAPTURE "+name+" suspect_magenta="+pct.ToString("F3",Invariant)+"%");
                    camera.targetTexture=null;RenderTexture.active=null;
                    UnityEngine.Object.DestroyImmediate(rt);UnityEngine.Object.DestroyImmediate(image);
                }
            }
            Debug.Log("ASHEN_CAPTURE_COMPLETE 10 Unity-camera PNG in "+outDir);
        }
        public static void BuildAndCapture(){Build();CaptureFiveLevels();}
    }
}
