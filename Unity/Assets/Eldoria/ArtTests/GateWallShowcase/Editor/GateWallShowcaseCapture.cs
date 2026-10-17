using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace Eldoria.ArtTests.GateWallShowcase
{
    /// <summary>
    /// Asset-first study of Valoria's original StoneDefensiveFamilyV2: one gatehouse,
    /// six continuous curtain sections, two guard towers, and a terrain-integrated
    /// approach. Generates a real, editable Unity scene plus genuine engine renders.
    /// No changes to canonical Valoria, LocalGateway, or the Bastion construction.
    /// </summary>
    public static class GateWallShowcaseCapture
    {
        private const string AssetsFolder = "Assets/Eldoria/ArtTests/GateWallShowcase/Generated";
        private const string Source = "Assets/Eldoria/Resources/Valoria/ProductionArt/StoneDefensiveFamilyV2/";
        private const string OutputRelative = "../../docs/evidence/eldoria-gate-wall-showcase-20261017";
        private static readonly Dictionary<string,Material> materials = new Dictionary<string,Material>();
        private static readonly List<string> modelDescriptions = new List<string>();
        private static Transform root;
        private static int sourceInstances;

        private static Color C(int hex)
        {
            return new Color(((hex >> 16) & 255)/255f,((hex >> 8) & 255)/255f,(hex & 255)/255f,1f);
        }

        [MenuItem("Eldoria/Art tests/Gate and wall showcase/Build scene and capture three real PNGs")]
        public static void Capture()
        {
            // Editor process will exit non-zero on any genuine problem. No fake passes.
            if (!AssetDatabase.IsValidFolder(AssetsFolder))
            {
                if (!AssetDatabase.IsValidFolder("Assets/Eldoria/ArtTests/GateWallShowcase"))
                    AssetDatabase.CreateFolder("Assets/Eldoria/ArtTests","GateWallShowcase");
                AssetDatabase.CreateFolder("Assets/Eldoria/ArtTests/GateWallShowcase","Generated");
            }
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            var previous = AssetDatabase.FindAssets("t:Material",new[]{AssetsFolder});
            foreach(var guid in previous)
            {
                var p = AssetDatabase.GUIDToAssetPath(guid);
                if(p.Contains("GateWall_")) AssetDatabase.DeleteAsset(p);
            }
            foreach(var name in new[]{"GateWall_terrain.asset","GateWall_terrain_mesh.asset"})
                AssetDatabase.DeleteAsset(AssetsFolder+"/"+name);
            materials.Clear();
            modelDescriptions.Clear();
            sourceInstances=0;
            var all = new GameObject("VALORIA / GATE AND WALL / EXPERIMENTAL SOURCE STUDY");
            root=all.transform;
            CreateLighting();
            CreateTerrain();
            CreateSourceFortifications();
            CreateRoadAndBridges();
            CreateNaturalRockVerge();
            CreateArchitectureDetails();
            if (sourceInstances < 7)
                throw new InvalidOperationException("Model import gate FAILED: only "+sourceInstances+" authentic architectural GLB instances");
            var camera = CreateCamera();
            var scenePath = AssetsFolder+"/GateWallShowcase_REAL_UNITY.unity";
            EditorSceneManager.MarkSceneDirty(scene);
            AssetDatabase.SaveAssets();
            if(!EditorSceneManager.SaveScene(scene,scenePath))
                throw new IOException("Unable to save real native Unity gate-wall scene");
            string outDir=Path.GetFullPath(Path.Combine(UnityEngine.Application.dataPath,OutputRelative));
            Directory.CreateDirectory(outDir);
            SaveView(camera,Path.Combine(outDir,"01_hero_three_quarter_REAL_UNITY.png"),new Vector3(25,18,-39),new Vector3(0,8,1),46f);
            SaveView(camera,Path.Combine(outDir,"02_frontal_gate_REAL_UNITY.png"),new Vector3(0,13,-32),new Vector3(0,7,0),46f);
            SaveView(camera,Path.Combine(outDir,"03_wall_connection_REAL_UNITY.png"),new Vector3(-26,16,-30),new Vector3(-8,7,1),44f);
            string evidence="{\n"+
                "  \"kind\": \"genuine_Unity_editor_scene\",\n"+
                "  \"engine\": \""+UnityEngine.Application.unityVersion+"\",\n"+
                "  \"asset_family\": \"StoneDefensiveFamilyV2\",\n"+
                "  \"source_instances\": "+sourceInstances+",\n"+
                "  \"native_scene\": \""+scenePath+"\",\n"+
                "  \"mobile_certified\": false,\n"+
                "  \"gameplay_integrated\": false,\n"+
                "  \"art_direction_approved\": false,\n"+
                "  \"models\": ["+string.Join(",",modelDescriptions.ToArray())+"]\n"+
                "}";
            File.WriteAllText(Path.Combine(outDir,"evidence.json"),evidence);
            Debug.Log("ELDORIA_GATE_WALL_REAL_UNITY_CAPTURE_COMPLETE " + sourceInstances);
            Debug.Log("ELDORIA_GATE_WALL_NATIVE_SCENE_SAVED " + scenePath);
        }

        private static Material Mat(string name,Color color,float smooth=.13f)
        {
            if(materials.TryGetValue(name,out var old) && old!=null)return old;
            var shader=Shader.Find("Universal Render Pipeline/Lit");
            if(shader==null)shader=Shader.Find("Standard");
            if(shader==null) throw new InvalidOperationException("URP and Standard shaders both unavailable");
            var mat=new Material(shader){name="GateWall_"+name};
            if(mat.HasProperty("_BaseColor"))mat.SetColor("_BaseColor",color);
            if(mat.HasProperty("_Color"))mat.SetColor("_Color",color);
            if(mat.HasProperty("_Smoothness"))mat.SetFloat("_Smoothness",smooth);
            if(mat.HasProperty("_Metallic"))mat.SetFloat("_Metallic",0);
            AssetDatabase.CreateAsset(mat,AssetsFolder+"/GateWall_"+name+".mat");
            materials[name]=mat;
            return mat;
        }

        private static GameObject Primitive(string name,PrimitiveType type,Vector3 p,Vector3 scale,Material mat)
        {
            var obj=GameObject.CreatePrimitive(type);
            obj.name=name;
            obj.transform.SetParent(root,false);
            obj.transform.position=p;
            obj.transform.localScale=scale;
            obj.GetComponent<Renderer>().sharedMaterial=mat;
            var collider=obj.GetComponent<Collider>();
            if(collider!=null)Object.DestroyImmediate(collider);
            return obj;
        }
        private static GameObject Block(string name,float x,float y,float z,float sx,float sy,float sz,Material mat)
        {
            return Primitive(name,PrimitiveType.Cube,new Vector3(x,y,z),new Vector3(sx,sy,sz),mat);
        }

        private static float Ground(float x,float z)
        {
            float hills = Mathf.Pow(Mathf.Clamp01((Mathf.Abs(x)-30f)/20f),1.45f)*11f;
            float rear = Mathf.SmoothStep(0,1,Mathf.InverseLerp(12f,48f,z))*8f;
            float ripple = (Mathf.PerlinNoise(x*.075f+14,z*.075f+25)-.5f)*.6f;
            // A consistent flat construction platform beneath ALL source elements.
            float site = 1f - Mathf.SmoothStep(0f,1f,Mathf.InverseLerp(23f,40f,Mathf.Abs(x)));
            float zsite = 1f - Mathf.SmoothStep(0f,1f,Mathf.InverseLerp(7f,18f,Mathf.Abs(z)));
            return (hills+rear+ripple)*(1f-site*zsite);
        }

        private static Texture2D BuildEarthTexture()
        {
            const int n=512;
            var tex=new Texture2D(n,n,TextureFormat.RGBA32,false,false);
            tex.name="GateWall_terrain_albedo";
            tex.wrapMode=TextureWrapMode.Repeat;
            tex.filterMode=FilterMode.Trilinear;
            var pixels=new Color[n*n];
            Color grass=C(0x646d58),lichen=C(0x888873),rock=C(0x626866),soil=C(0x665d50);
            for(int z=0;z<n;z++)for(int x=0;x<n;x++)
            {
                float u=(float)x/n,v=(float)z/n;
                float broad=Mathf.PerlinNoise(u*5.3f+30,v*5.3f+20);
                float mid=Mathf.PerlinNoise(u*23f+4,v*23f+71);
                float fine=Mathf.PerlinNoise(u*109f+17,v*109f+44);
                var baseC=Color.Lerp(grass,lichen,Mathf.Clamp01(mid*.4f+broad*.6f));
                baseC=Color.Lerp(baseC,rock,Mathf.Clamp01((broad-.52f)*1.5f));
                baseC=Color.Lerp(baseC,soil,Mathf.Clamp01((mid-.70f)*1.0f));
                float shade=.85f+.25f*fine;
                pixels[z*n+x]=new Color(baseC.r*shade,baseC.g*shade,baseC.b*shade,1f);
            }
            tex.SetPixels(pixels);tex.Apply();
            AssetDatabase.CreateAsset(tex,AssetsFolder+"/GateWall_terrain.asset");
            return tex;
        }

        private static void CreateTerrain()
        {
            var grass=Mat("moss_rock",C(0x8d9182));
            var instance=new Material(grass){name="GateWall_ground_painted"};
            var tex=BuildEarthTexture();
            if(instance.HasProperty("_BaseMap")) instance.SetTexture("_BaseMap",tex);
            if(instance.HasProperty("_MainTex")) instance.SetTexture("_MainTex",tex);
            AssetDatabase.CreateAsset(instance,AssetsFolder+"/GateWall_ground_painted.mat");
            const int n=128;
            const float side=138f;
            var verts=new Vector3[(n+1)*(n+1)];
            var uvs=new Vector2[verts.Length];
            int[] triangles=new int[n*n*6];
            for(int j=0;j<=n;j++)for(int i=0;i<=n;i++)
            {
                int k=j*(n+1)+i;
                float x=(i/(float)n-.5f)*side,z=(j/(float)n-.5f)*side;
                verts[k]=new Vector3(x,Ground(x,z),z);
                uvs[k]=new Vector2(x*.065f,z*.065f);
            }
            for(int j=0;j<n;j++)for(int i=0;i<n;i++)
            {
                int a=j*(n+1)+i,b=a+1,c=a+n+1,d=c+1,t=(j*n+i)*6;
                triangles[t]=a;triangles[t+1]=c;triangles[t+2]=b;
                triangles[t+3]=b;triangles[t+4]=c;triangles[t+5]=d;
            }
            var mesh=new Mesh{name="GateWall_continuous_sculpted_terrain"};
            mesh.indexFormat=IndexFormat.UInt32;mesh.vertices=verts;mesh.uv=uvs;
            mesh.triangles=triangles;mesh.RecalculateNormals();mesh.RecalculateBounds();
            AssetDatabase.CreateAsset(mesh,AssetsFolder+"/GateWall_terrain_mesh.asset");
            var o=new GameObject("Continuous sculpted natural ground");
            o.transform.SetParent(root,false);
            o.AddComponent<MeshFilter>().sharedMesh=mesh;
            o.AddComponent<MeshRenderer>().sharedMaterial=instance;
        }

        private static GameObject SourceModel(string filename,string name,float x,float z,float targetWidth,float yOffset=0)
        {
            var path=filename.StartsWith("Assets/") ? filename : Source+filename;
            var prefab=AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if(prefab==null) throw new FileNotFoundException("Authentic Valoria source prefab failed import: "+path);
            var obj=PrefabUtility.InstantiatePrefab(prefab) as GameObject;
            if(obj==null) throw new InvalidOperationException("Unable to instantiate "+path);
            obj.transform.SetParent(root,true);
            obj.name="AUTHENTIC ELDORIA MODEL / "+name;
            obj.transform.rotation=Quaternion.identity;
            obj.transform.localScale=Vector3.one;
            obj.transform.position=Vector3.zero;
            Renderer[] rr=obj.GetComponentsInChildren<Renderer>(true);
            if(rr.Length==0)throw new InvalidOperationException("Original model has no renderer: "+filename);
            Bounds bounds=rr[0].bounds;
            for(int i=1;i<rr.Length;i++)bounds.Encapsulate(rr[i].bounds);
            float nativeWidth=bounds.size.x;
            if(nativeWidth<.001f)throw new InvalidOperationException("Original source geometry degenerate: "+filename);
            float factor=targetWidth/nativeWidth;
            obj.transform.localScale=Vector3.one*factor;
            bounds=rr[0].bounds;
            for(int i=1;i<rr.Length;i++)bounds.Encapsulate(rr[i].bounds);
            obj.transform.position += new Vector3(x-bounds.center.x,Ground(x,z)+yOffset-bounds.min.y,z-bounds.center.z);
            sourceInstances++;
            modelDescriptions.Add("{\"asset\":\""+filename+"\",\"role\":\""+name+
                "\",\"width\":"+targetWidth.ToString("F2",System.Globalization.CultureInfo.InvariantCulture)+"}");
            return obj;
        }

        private static void CreateSourceFortifications()
        {
            // Front axis: +Z is interior, -Z is player approach.
            // Composition stays modular: outer tower | wall | gate | wall | tower.
            SourceModel("Valoria_MainGate_StoneDefV2.glb","main double-height gate",0,0,17.0f,.24f);
            SourceModel("Valoria_WallSegment_StoneDefV2.glb","left curtain",-14.0f,0,11.0f,.18f);
            SourceModel("Valoria_WallSegment_StoneDefV2.glb","right curtain",14.0f,0,11.0f,.18f);
            SourceModel("Valoria_Tower_StoneDefV2.glb","left flanking watchtower",-22.2f,0,5.2f,.14f);
            SourceModel("Valoria_Tower_StoneDefV2.glb","right flanking watchtower",22.2f,0,5.2f,.14f);
            SourceModel("Valoria_WallSegment_StoneDefV2.glb","left outer rampart",-30.0f,0,10.1f,.14f);
            SourceModel("Valoria_WallSegment_StoneDefV2.glb","right outer rampart",30.0f,0,10.1f,.14f);
            // Landscape supports drawn from Eldoria's own textured rock source.
            string rock="Assets/Eldoria/Resources/Valoria/Rescued/RockTerrainSeamFiller.glb";
            SourceModel(rock,"left cliff wall contact",-40f,5f,12f,-1.0f);
            SourceModel(rock,"right cliff wall contact",40f,5f,12f,-1.0f);
            SourceModel(rock,"rear west bedrock",-35f,15f,14f,-2.1f);
            SourceModel(rock,"rear east bedrock",35f,15f,14f,-2.1f);
        }

        private static void CreateRoadAndBridges()
        {
            var cobble=Mat("paving_ashlar",C(0x858478));
            var pale=Mat("paving_aged_edge",C(0x979184));
            var dark=Mat("paving_shadow",C(0x77786f));
            var earth=Mat("road_underlay",C(0x52534b));
            Block("Road base continuous - no floating approach",0,.07f,-15.8f,6.9f,.18f,32f,earth);
            var random=new System.Random(1432);
            for(int row=0;row<32;row++)
            {
                float z=-30.8f+row*.94f;
                for(int col=0;col<5;col++)
                {
                    float x=(col-2f)*1.26f;
                    float jitter=(float)(random.NextDouble()-.5)*.12f;
                    Block("Uneven cut limestone paving",x+jitter,Ground(x,z)+.22f,z,
                        1.12f+(float)random.NextDouble()*.08f,.14f,.85f,
                        (row+col)%6==0?pale:(row+col)%5==0?dark:cobble);
                }
            }
            // Heavy ashlar foundation visually connects wall and landscape.
            var foundation=Mat("foundation_footstone",C(0x676961));
            for(int i=0;i<23;i++)
            {
                float x=-43f+i*3.9f;
                if(Mathf.Abs(x)<6.3f)continue;
                Block("Buried fortification foundation stone",x,Ground(x,.6f)+.38f,.6f,3.9f,.75f,4f,foundation);
            }
        }

        private static void CreateNaturalRockVerge()
        {
            // Restrained foreground study: no low-poly cylinder trees masking the
            // authentic source models, and no decorative blobs in front of the gate.
            var basalt=Mat("mountain_slate",C(0x555956));
            var sandstone=Mat("mountain_warm_stone",C(0x77786c));
            var lichen=Mat("moss_on_stone",C(0x626d56));
            var random=new System.Random(271828);
            for(int i=0;i<105;i++)
            {
                float x=(float)(random.NextDouble()*128-64);
                float z=(float)(random.NextDouble()*94-33);
                if(Mathf.Abs(x)<37 && z>-11f && z<17f)continue;
                if(Mathf.Abs(x)<10 && z<-4f)continue;
                float y=Ground(x,z);
                float r=.35f+(float)random.NextDouble()*1.10f;
                var rock=Primitive("Distant subdued highland rock",PrimitiveType.Sphere,
                    new Vector3(x,y+.12f,z),new Vector3(r*1.3f,r*.51f,r*1.05f),
                    i%6==0?lichen:(i%3==0?basalt:sandstone));
                rock.transform.rotation=Quaternion.Euler(0,(float)random.NextDouble()*360,7f);
            }
        }

        private static void CreateArchitectureDetails()
        {
            var blue=Mat("valoria_banner_blue",C(0x183b67));
            var gold=Mat("heraldic_antique_gold",C(0xb99a53));
            var wood=Mat("weathered_timber",C(0x514434));
            var iron=Mat("black_wrought_iron",C(0x282f30));
            var fire=Mat("emberlight",C(0xf4a949),.34f);
            var torchStone=Mat("torch_stone",C(0x7a7468));
            for(int s=-1;s<=1;s+=2)
            {
                float x=s*6f;
                Block("Valoria flagstaff",x,8.3f,-3f,.13f,9f,.13f,wood);
                Block("Valoria dark blue heraldic banner",x+s*.46f,10.25f,-3.1f,.9f,3.6f,.11f,blue);
                Block("Gold royal heraldic charge",x+s*.46f,10.5f,-3.19f,.25f,.72f,.07f,gold);
                for(int j=0;j<2;j++)
                {
                    float z=-8f-j*11.0f;
                    Block("Carved roadside limestone brazier",s*4.3f,1.4f,z,1.1f,2.8f,1.1f,torchStone);
                    Primitive("Wrought iron fire bowl",PrimitiveType.Cylinder,
                        new Vector3(s*4.3f,3.0f,z),new Vector3(.73f,.18f,.73f),iron);
                    Primitive("Amber flame globe",PrimitiveType.Sphere,
                        new Vector3(s*4.3f,3.5f,z),new Vector3(.5f,.85f,.5f),fire);
                    var point=new GameObject("Warm fire reflection point lamp");
                    point.transform.SetParent(root,false);
                    point.transform.position=new Vector3(s*4.3f,3.75f,z);
                    var l=point.AddComponent<Light>();
                    l.type=LightType.Point; l.range=9; l.intensity=2.1f;
                    l.color=C(0xffb66c);l.shadows=LightShadows.None;
                }
            }
            // Heavy double-leaf oak doors are recessed BEHIND the original arch.
            var oak=Mat("oak_gate_leaf",C(0x453529));
            Block("Heavy oak original-gate double leaves",0,3.8f,1.9f,5.4f,7.15f,.28f,oak);
            Block("Center door meeting strip",0,3.8f,1.64f,.12f,7.15f,.18f,iron);
            for(int i=-4;i<=4;i++)
                Block("Vertical wrought iron gate rib",i*.61f,3.8f,1.68f,.13f,7.0f,.14f,iron);
            for(int j=0;j<3;j++)
                Block("Heavy horizontal iron braces",0,1.3f+j*2.35f,1.64f,5.45f,.20f,.17f,iron);
            var boss=Mat("gate_iron_nail_boss",C(0x978267),.25f);
            for(int i=-4;i<=4;i++)
                for(int j=0;j<3;j++)
                    Primitive("Gate iron fastening boss",PrimitiveType.Sphere,
                        new Vector3(i*.61f,1.3f+j*2.35f,1.50f),
                        new Vector3(.11f,.11f,.09f),boss);
            // Lighting stays anchored to the architectural asset, no painted-in detailing.
        }

        private static void CreateLighting()
        {
            var sunGO=new GameObject("Afternoon sunlight • camera left");
            sunGO.transform.SetParent(root,false);
            sunGO.transform.rotation=Quaternion.Euler(39,-48,-9);
            var sun=sunGO.AddComponent<Light>();
            sun.type=LightType.Directional;
            sun.intensity=1.52f;
            sun.color=C(0xfff0d6);
            sun.shadows=LightShadows.Soft;
            sun.shadowStrength=.64f;
            RenderSettings.sun=sun;
            RenderSettings.ambientMode=AmbientMode.Flat;
            RenderSettings.ambientLight=C(0x75818a);
            RenderSettings.fog=true;
            RenderSettings.fogMode=FogMode.Linear;
            RenderSettings.fogColor=C(0x99aab3);
            RenderSettings.fogStartDistance=75f;
            RenderSettings.fogEndDistance=150f;
        }

        private static Camera CreateCamera()
        {
            var o=new GameObject("Real Unity showcase camera");
            o.transform.SetParent(root,false);
            var cam=o.AddComponent<Camera>();
            cam.tag="MainCamera";
            cam.clearFlags=CameraClearFlags.SolidColor;
            cam.backgroundColor=C(0xa6b3ba);
            cam.nearClipPlane=.1f;cam.farClipPlane=260f;
            cam.orthographic=false;
            cam.allowHDR=true;
            return cam;
        }

        private static void SaveView(Camera cam,string path,Vector3 pos,Vector3 target,float fov)
        {
            cam.transform.position=pos;
            cam.transform.LookAt(target);
            cam.fieldOfView=fov;
            var rt=new RenderTexture(1600,900,24,RenderTextureFormat.ARGB32);
            rt.antiAliasing=4;
            rt.Create();
            var before=RenderTexture.active;
            var beforeTarget=cam.targetTexture;
            var image=new Texture2D(1600,900,TextureFormat.RGB24,false);
            try
            {
                cam.targetTexture=rt;
                RenderTexture.active=rt;
                cam.Render();
                image.ReadPixels(new Rect(0,0,1600,900),0,0,false);
                image.Apply(false,false);
                File.WriteAllBytes(path,image.EncodeToPNG());
                if(new FileInfo(path).Length<20000)throw new IOException("Implausibly small native render: "+path);
                Debug.Log("ELDORIA_REAL_ENGINE_FRAME "+path);
            }
            finally
            {
                cam.targetTexture=beforeTarget;
                RenderTexture.active=before;
                rt.Release();
                Object.DestroyImmediate(rt);
                Object.DestroyImmediate(image);
            }
        }
    }
}