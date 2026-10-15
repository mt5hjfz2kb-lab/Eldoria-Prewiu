using System;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using Object = UnityEngine.Object;

namespace Eldoria.EditorTools.ReferenceFirstPilot
{
    // A separate real-Unity architectural composition experiment; never touches the player scene.
    public static class ReferenceFirstCompositionV2Capture
    {
        const string Root = "Assets/Eldoria/ArtTests/ReferenceFirstPilot/V2";
        const string FortPath = Root + "/Imported/BastionFamilyV1.glb";
        const string LowerPath = Root + "/Imported/LowerGateFamilyV1.glb";
        const string WallPath = "Assets/Eldoria/Resources/Valoria/Stone_Wall.prefab";
        const string TowerPath = "Assets/Eldoria/Resources/Valoria/Stone_Tower.prefab";
        const string TreePath = "Assets/Eldoria/Resources/WorldInventory/Tree01A.prefab";
        const string RockPath = "Assets/Eldoria/Resources/WorldInventory/Rock01.prefab";
        const int Width = 1536, Height = 1024;

        [Serializable] class Report
        {
            public string stage = "REAL_UNITY_ISOLATED_V2_NOT_APPROVED";
            public string reference = "references/VALORIA_APPROVED_VISUAL_REFERENCE.jpg";
            public string upperFortress = FortPath, lowerGate = LowerPath;
            public string revision;
            public string[] frames = {"03-fortified-plateau-overview.png", "04-fortified-plateau-architecture.png"};
            public int architectureInstances, forestInstances, rockInstances;
            public int width = Width, height = Height;
            public bool artApproved = false, licenseVerified = false, productionModified = false, publication = false;
        }

        public static void Capture()
        {
            if (!UnityEngine.Application.isBatchMode) throw new InvalidOperationException("Batchmode only, to protect open player scenes");
            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
            GameObject fortress = Load(FortPath), lowerGate = Load(LowerPath);
            GameObject wall = Load(WallPath), tower = Load(TowerPath);
            GameObject tree = Load(TreePath), rock = Load(RockPath);
            var shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null) throw new InvalidOperationException("URP/Lit shader unavailable");
            Directory.CreateDirectory(Root + "/Materials");
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            var root = new GameObject("Reference-first V2 | isolated visual composition only").transform;
            var stage = Group(root,"Temporary island/road/bridge base, not production assets");
            var buildings = Group(root,"Existing upper/lower stone architecture, no source changes");
            var nature = Group(root,"Actual reused forest/rock prefabs");

            Material grass = Material("ground-olive", shader, new Color(.31f,.33f,.23f), null);
            Material cliff = Material("rock-grey", shader, new Color(.58f,.57f,.55f),
                "Assets/Eldoria/ArtTests/OriginalHero/Textures/rock.png");
            Material path = Material("paving-weathered", shader, new Color(.74f,.69f,.62f),
                "Assets/Eldoria/ArtTests/OriginalHero/Textures/limestone.png");
            Material river = Material("river-slateblue", shader, new Color(.06f,.14f,.19f), null);

            // Irregular continuous escarpment; no oversized grassy mountain prefabs from V1.
            Island(stage,"Lower island",0f,-3f,55f,61f,6f,-13f,grass,cliff);
            Island(stage,"Upper fortress terrace",0f,28f,34f,26f,14f,5.7f,grass,cliff);
            var water = GameObject.CreatePrimitive(PrimitiveType.Plane);
            water.name="Distant blue water, temporary shader";
            water.transform.SetParent(stage,false);
            water.transform.position=new Vector3(0f,-13.2f,0f);
            water.transform.localScale=new Vector3(25f,1f,25f);
            water.GetComponent<Renderer>().sharedMaterial=river;
            Object.DestroyImmediate(water.GetComponent<Collider>());

            int architecture = 0;
            Place(fortress,buildings,"Upper fortress - original BastionFamilyV1",0,29,14.0f,18.5f,180f); architecture++;
            Place(lowerGate,buildings,"Lower stone gate - original LowerGateFamilyV1",0,-37,6.0f,14.5f,180f); architecture++;
            // Real modular walls provide the scene's long, low defensive silhouette.
            for(int i=-3;i<=3;i++) if(i!=0)
            {
                Place(wall,buildings,"Upper wall "+i,i*10f,30,13.8f,7.8f,90f); architecture++;
                Place(wall,buildings,"Lower wall "+i,i*10f,-37,5.8f,7.2f,90f); architecture++;
            }
            for(int i=0;i<4;i++)
            {
                Place(wall,buildings,"West wall "+i,-42f,-20+i*15f,6f,7.5f,0f); architecture++;
                Place(wall,buildings,"East wall "+i,42f,-20+i*15f,6f,7.5f,0f); architecture++;
            }
            foreach(float x in new[]{-30f,30f})
            {
                Place(tower,buildings,"Upper corner tower "+x,x,29,14f,11f,0f);architecture++;
                Place(tower,buildings,"Lower corner tower "+x,x,-37,6f,11f,0f);architecture++;
            }

            // A recognisable road from the foreground bridge to the upper defensive gateway.
            for(int n=0;n<44;n++)
            {
                float z=-93f+n*3f;
                float y=z < -54f ? -5.5f : z < -7 ? 6.2f : Mathf.Lerp(6.2f,14.18f,Mathf.Clamp01((z+7f)/36f));
                Box(stage,"Temporary paving, replace with authored road",new Vector3(0,y,z),
                    new Vector3(7.0f,.20f,3.03f),path);
            }
            for(int i=0;i<7;i++)
            {
                float z=-88+i*7.5f;
                foreach(float x in new[]{-4.2f,4.2f})
                    Box(stage,"Temporary bridge parapet post",new Vector3(x,-4f,z),new Vector3(1f,2.7f,1f),cliff);
            }

            int forest=0;
            for(int i=0;i<65;i++)
            {
                float t=i*2f*Mathf.PI/65f;
                float x=Mathf.Cos(t)*(64f+(i%5)*3f);
                float z=Mathf.Sin(t)*(55f+(i%4)*4f)+12f;
                if(z < -39f && Mathf.Abs(x) < 14f) continue;
                Place(tree,nature,"Reused real conifer "+i,x,z,5.9f,8.5f+(i%4)*1.1f,i*47f); forest++;
            }
            int rocks=0;
            for(int i=0;i<36;i++)
            {
                float t=(i+.2f)*2f*Mathf.PI/36f;
                float x=Mathf.Cos(t)*(54f+(i%4));
                float z=Mathf.Sin(t)*(59f+(i%5));
                if(z < -45f && Mathf.Abs(x)<15f)continue;
                Place(rock,nature,"Reused real rock "+i,x,z,-6f,3.9f+(i%3)*1.1f,i*29f); rocks++;
            }
            RenderSettings.ambientMode=AmbientMode.Flat;
            RenderSettings.ambientLight=new Color(.25f,.32f,.43f);
            RenderSettings.fog=true;
            RenderSettings.fogMode=FogMode.ExponentialSquared;
            RenderSettings.fogColor=new Color(.22f,.31f,.42f);
            RenderSettings.fogDensity=.0029f;
            var sun=new GameObject("Warm low dusk key").AddComponent<Light>();
            sun.type=LightType.Directional;
            sun.color=new Color(1f,.78f,.57f);
            sun.intensity=1.55f;
            sun.shadows=LightShadows.Soft;
            sun.transform.rotation=Quaternion.Euler(43f,-34f,0f);
            var cam=new GameObject("Composition V2 camera").AddComponent<Camera>();
            cam.orthographic=true;
            cam.clearFlags=CameraClearFlags.SolidColor;
            cam.backgroundColor=RenderSettings.fogColor;
            cam.nearClipPlane=.1f;cam.farClipPlane=650f;
            string sceneFile=Root+"/ReferenceFirstCompositionV2.unity";
            if(!EditorSceneManager.SaveScene(scene,sceneFile))throw new IOException("Cannot save isolated composition scene");
            AssetDatabase.SaveAssets();
            string folder=Path.GetFullPath(Path.Combine(UnityEngine.Application.dataPath,"../../artifacts/reference-first-composition-v2"));
            Directory.CreateDirectory(folder);
            Save(cam,folder,"03-fortified-plateau-overview.png",new Vector3(104,112,-125),new Vector3(0,4,0),84f);
            Save(cam,folder,"04-fortified-plateau-architecture.png",new Vector3(78,81,-89),new Vector3(0,8,-3),64f);
            var report=new Report{revision=Environment.GetEnvironmentVariable("GITHUB_SHA")??"LOCAL",
                architectureInstances=architecture,forestInstances=forest,rockInstances=rocks};
            File.WriteAllText(Path.Combine(folder,"render-manifest-v2.json"),JsonUtility.ToJson(report,true));
            Debug.Log("REFERENCE_FIRST_V2_RENDERED: architecture="+architecture+" trees="+forest+" rocks="+rocks+" ART_APPROVED=false");
        }

        static Transform Group(Transform parent,string name)
        {
            var go=new GameObject(name);
            go.transform.SetParent(parent,false);
            return go.transform;
        }
        static GameObject Load(string path)
        {
            var prefab=AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if(prefab==null)throw new FileNotFoundException("Existing source cannot be imported: "+path);
            return prefab;
        }
        static Bounds BoundsOf(GameObject go)
        {
            bool first=true;var b=new Bounds(go.transform.position,Vector3.zero);
            foreach(var r in go.GetComponentsInChildren<Renderer>(true))
                if(r.enabled&&r.gameObject.activeInHierarchy)
                {
                    if(first){b=r.bounds;first=false;}else b.Encapsulate(r.bounds);
                }
            if(first || b.size.y < .002f)throw new InvalidOperationException("Missing real render mesh: "+go.name);
            return b;
        }
        static void Place(GameObject prefab,Transform parent,string name,float x,float z,float ground,float height,float yaw)
        {
            var go=(GameObject)PrefabUtility.InstantiatePrefab(prefab);
            if(go==null)throw new InvalidOperationException("Cannot instantiate "+prefab.name);
            go.name=name;go.transform.SetParent(parent,true);
            var bounds=BoundsOf(go);
            go.transform.localScale*=Mathf.Clamp(height/bounds.size.y,.0005f,60f);
            go.transform.rotation=Quaternion.Euler(0,yaw,0);
            bounds=BoundsOf(go);
            go.transform.position+=new Vector3(x-bounds.center.x,ground-bounds.min.y,z-bounds.center.z);
        }
        static Material Material(string name,Shader shader,Color color,string texture)
        {
            string p=Root+"/Materials/"+name+".mat";
            var m=AssetDatabase.LoadAssetAtPath<Material>(p);
            if(m==null){m=new Material(shader);AssetDatabase.CreateAsset(m,p);}
            m.shader=shader;
            m.SetColor("_BaseColor",color);
            m.SetFloat("_Smoothness",.08f);
            if(texture!=null)
            {
                var tex=AssetDatabase.LoadAssetAtPath<Texture2D>(texture);
                if(tex==null)throw new FileNotFoundException("Material texture missing "+texture);
                m.SetTexture("_BaseMap",tex);
            }
            EditorUtility.SetDirty(m);
            return m;
        }
        static void Box(Transform parent,string name,Vector3 position,Vector3 scale,Material material)
        {
            var go=GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name=name;go.transform.SetParent(parent,false);go.transform.position=position;
            go.transform.localScale=scale;go.GetComponent<Renderer>().sharedMaterial=material;
            Object.DestroyImmediate(go.GetComponent<Collider>());
        }
        static void Island(Transform parent,string name,float cx,float cz,float rx,float rz,float high,float low,Material grass,Material rock)
        {
            const int N=64;
            var cap=new Vector3[N+1];var capUv=new Vector2[N+1];var capTri=new int[N*3];
            var sides=new Vector3[N*2];var sideUv=new Vector2[N*2];var sideTri=new int[N*6];
            cap[0]=new Vector3(cx,high,cz);
            for(int i=0;i<N;i++)
            {
                float a=i*2f*Mathf.PI/N;
                float wobble=1f+.04f*Mathf.Sin(i*.59f)+.033f*Mathf.Cos(i*1.53f);
                float x=cx+Mathf.Cos(a)*rx*wobble,z=cz+Mathf.Sin(a)*rz*wobble;
                cap[i+1]=new Vector3(x,high,z);capUv[i+1]=new Vector2(x*.04f,z*.04f);
                sides[i*2]=new Vector3(x,high,z);
                sides[i*2+1]=new Vector3(x+Mathf.Cos(a)*4f,low,z+Mathf.Sin(a)*4f);
                sideUv[i*2]=new Vector2(i*.20f,1f);sideUv[i*2+1]=new Vector2(i*.20f,0f);
                int next=(i+1)%N;
                capTri[i*3]=0;capTri[i*3+1]=next+1;capTri[i*3+2]=i+1;
                int k=i*6;
                sideTri[k]=i*2;sideTri[k+1]=i*2+1;sideTri[k+2]=next*2;
                sideTri[k+3]=next*2;sideTri[k+4]=i*2+1;sideTri[k+5]=next*2+1;
            }
            MeshObject(parent,name+" ground",cap,capUv,capTri,grass);
            MeshObject(parent,name+" rock wall",sides,sideUv,sideTri,rock);
        }
        static void MeshObject(Transform parent,string name,Vector3[] vertices,Vector2[] uv,int[] indices,Material m)
        {
            var go=new GameObject(name);go.transform.SetParent(parent,false);
            var mesh=new Mesh{name=name};mesh.vertices=vertices;mesh.uv=uv;
            mesh.triangles=indices;mesh.RecalculateNormals();mesh.RecalculateBounds();
            go.AddComponent<MeshFilter>().sharedMesh=mesh;
            go.AddComponent<MeshRenderer>().sharedMaterial=m;
        }
        static void Save(Camera cam,string directory,string file,Vector3 position,Vector3 target,float size)
        {
            cam.transform.position=position;cam.transform.LookAt(target);cam.orthographicSize=size;
            var rt=new RenderTexture(Width,Height,24,RenderTextureFormat.ARGB32);
            var prev=RenderTexture.active;Texture2D png=null;
            try
            {
                cam.targetTexture=rt;cam.Render();RenderTexture.active=rt;
                png=new Texture2D(Width,Height,TextureFormat.RGB24,false);
                png.ReadPixels(new Rect(0,0,Width,Height),0,0);png.Apply();
                File.WriteAllBytes(Path.Combine(directory,file),png.EncodeToPNG());
            }
            finally
            {
                cam.targetTexture=null;RenderTexture.active=prev;
                if(png!=null)Object.DestroyImmediate(png);
                rt.Release();Object.DestroyImmediate(rt);
            }
        }
    }
}
