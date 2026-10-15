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
    public static class ReferenceFirstCompositionV9Capture
    {
        const string Root = "Assets/Eldoria/ArtTests/ReferenceFirstPilot/V9";
        const string FortPath = Root + "/Imported/BastionFamilyV1.glb";
        const string LowerPath = Root + "/Imported/LowerGateFamilyV1.glb";
        const string WallPath = "Assets/Eldoria/Resources/Valoria/Stone_Wall.prefab";
        const string TowerPath = "Assets/Eldoria/Resources/Valoria/Stone_Tower.prefab";
        const string TreePath = "Assets/Eldoria/Resources/WorldInventory/Tree01A.prefab";
        const string RockPath = "Assets/Eldoria/Resources/WorldInventory/Rock01.prefab";
        const int Width = 1536, Height = 1024;

        [Serializable] class Report
        {
            public string stage = "REAL_UNITY_V9_REUSED_MOUNTAIN_PREFAB_NOT_APPROVED";
            public string reference = "USER CANONICAL 1513x1039 SHA256 de476e0516f744f9cd899419404971fa5e8d42bd49f2f7db7670d1cb325f8a4f";
            public string upperFortress = FortPath, lowerGate = LowerPath;
            public string revision;
            public string[] frames = {"17-real-mountains-overview.png", "18-real-mountains-close.png"};
            public int architectureInstances, forestInstances, rockInstances, distantTreeInstances, pavingStoneInstances, mountainSourceInstances;
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
            // Unlike failed extruded V8 silhouette, this uses an already imported REAL mesh.
            GameObject mountain = Load("Assets/Eldoria/Resources/WorldInventory/Mountain01.prefab");
            var shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null) throw new InvalidOperationException("URP/Lit shader unavailable");
            Directory.CreateDirectory(Root + "/Materials");
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            var root = new GameObject("REFERENCE FIRST V9 | real existing mountain prefab backdrop, no generated walls | NOT APPROVED").transform;
            var stage = Group(root,"Temporary island/road/bridge base, not production assets");
            var buildings = Group(root,"Existing upper/lower stone architecture, no source changes");
            var nature = Group(root,"Actual reused forest/rock prefabs");

            Material grass = Material("ground-olive", shader, Color.white, null);
            var organicGround = OrganicGroundTexture();
            grass.SetTexture("_BaseMap", organicGround);
            grass.SetTextureScale("_BaseMap", new Vector2(1.2f,1.2f));
            EditorUtility.SetDirty(grass);
            Material cliff = Material("rock-grey", shader, new Color(.54f,.53f,.53f),
                "Assets/Eldoria/ArtTests/OriginalHero/Textures/rock.png");
            Material path = Material("paving-weathered", shader, new Color(.77f,.73f,.67f),
                "Assets/Eldoria/ArtTests/OriginalHero/Textures/limestone.png");
            Material river = Material("river-slateblue", shader, new Color(.08f,.22f,.29f), null);

            // Irregular continuous escarpment; no oversized grassy mountain prefabs from V1.
            Island(stage,"Continuous wide grass plateau",0f,-8f,72f,82f,6f,-13f,grass,cliff);
            Island(stage,"Upper raised fortified terrace",0f,63f,53f,23f,14f,5.7f,grass,cliff);
            // Reference-first REUSE instead of hand-building geometry.
            // This scene uses NatureStarterKit's actual Mountain01 prefab, no generated
            // double-faced polygon backgrounds. V8 silhouette failures are explicitly discarded.
            var backdrop = Group(root,"REAL recycled mountain meshes, never fake skyline blocks");
            int mountainCount=0;
            float[] bx = {-208f,-166f,-120f,-82f,83f,125f,170f,208f};
            float[] bz = {132f,115f,106f,132f,121f,105f,125f,136f};
            float[] h  = {90f,84f,75f,54f,65f,77f,85f,90f};
            float[] bottom={-55f,-48f,-43f,-42f,-43f,-42f,-47f,-57f};
            for(int i=0;i<bx.Length;i++)
            {
                Place(mountain,backdrop,"Reusable real Alpine mountain "+i,bx[i],bz[i],
                      bottom[i],h[i],i*39f);
                mountainCount++;
            }
            // Keep actual water only at the bridge/bottom-of-cliff, as in the approved reference.
            IrregularWater(stage,river);

            // Use one consistent warm architectural palette across the reused, original 3D meshes.
            Material stoneWarm = Material("coherent-warm-limestone",shader,new Color(.89f,.80f,.70f),
                "Assets/Eldoria/ArtTests/OriginalHero/Textures/oldstone.png");
            Material woodBrown = Material("coherent-oak-timber",shader,new Color(.48f,.32f,.19f),
                "Assets/Eldoria/ArtTests/OriginalHero/Textures/oak.png");
            Material ironDark = Material("coherent-iron-dark",shader,new Color(.19f,.19f,.21f),null);
            Material flagBlue = Material("coherent-royal-heraldry-blue",shader,new Color(.08f,.18f,.52f),null);
            int architecture = 0;
            Place(fortress,buildings,"UPPER DEFENSIVE GATE",0,63,14.0f,18.5f,180f); architecture++;
            Place(lowerGate,buildings,"LOWER BRIDGE GATE",0,-77,6.0f,14.5f,180f); architecture++;
            // Real modular walls provide the scene's long, low defensive silhouette.
            for(int i=-3;i<=3;i++) if(i!=0)
            {
                Place(wall,buildings,"Upper wall "+i,i*14f,64,13.8f,9.3f,90f); architecture++;
                Place(wall,buildings,"Lower wall "+i,i*10f,-77,5.8f,8.0f,90f); architecture++;
            }
            for(int i=0;i<4;i++)
            {
                Place(wall,buildings,"West wall "+i,-55f,-45+i*20f,6f,7.5f,0f); architecture++;
                Place(wall,buildings,"East wall "+i,55f,-45+i*20f,6f,7.5f,0f); architecture++;
            }
            foreach(float x in new[]{-30f,30f})
            {
                Place(tower,buildings,"Upper corner tower "+x,x,63,14f,11f,0f);architecture++;
                Place(tower,buildings,"Lower corner tower "+x,x,-77,6f,11f,0f);architecture++;
            }

            int unifiedSlots=HarmonizeArchitecture(buildings,stoneWarm,woodBrown,ironDark,flagBlue);
            Debug.Log("V7_STONE_PALETTE_REPLACED: "+unifiedSlots+" renderer material slots");
            // V4 organic ground texture replaces rectangular soil patches.
            // Backing masonry connects the existing wall family without moving the gatehouse.
            var masonry=Material("warm-connecting-masonry",shader,new Color(.63f,.60f,.54f),
                "Assets/Eldoria/ArtTests/OriginalHero/Textures/limestone.png");
            foreach(float side in new[]{-1f,1f})
                Box(stage,"Upper CONTINUOUS stone defensive core "+side,
                    new Vector3(side*30f,16.3f,66f),new Vector3(32f,4.8f,3.2f),masonry);
            // Join low lateral parapets without filling the open central courtyard.
            for(int side=-1; side<=1;side+=2)
            for(int i=0;i<5;i++)
            {
                float z=-57f+i*17f;
                float x=side*(53f+1.2f*Mathf.Sin(i*1.8f));
                Box(stage,"Continuous low flank wall "+side+" "+i,
                    new Vector3(x,7.1f,z),new Vector3(2.1f,2.7f,17f),masonry);
            }
            // Reference-correct single route: bridge over water -> lower gate -> open courtyard -> stair -> upper gate.
            for(int i=0;i<13;i++)
            {
                float z=-116f+i*3.3f;
                Box(stage,"BRIDGE stone deck "+i,new Vector3(0,5.9f,z),new Vector3(8.4f,.42f,3.31f),path);
                foreach(float x in new[]{-4.55f,4.55f})
                    Box(stage,"BRIDGE parapet "+i,new Vector3(x,7.1f,z),new Vector3(.7f,2.1f,3.31f),cliff);
            }
            for(int i=0;i<26;i++)
            {
                float z=-74f+i*3.8f;
                if(z>=24f)break;
                Box(stage,"MAIN AXIS "+i,new Vector3(0,6.12f,z),new Vector3(7.7f,.23f,3.7f),path);
            }
            for(int i=0;i<12;i++)
            {
                float z=24f+i*1.3f;
                float y=6f+(i+1)*.66f;
                Box(stage,"CENTRAL STAIR step "+i,new Vector3(0,y,z),new Vector3(9.8f,.66f,1.31f),path);
                foreach(float x in new[]{-5.45f,5.45f})
                    Box(stage,"CENTRAL STAIR cheek "+i,new Vector3(x,y,z),new Vector3(.82f,.88f,1.34f),cliff);
            }
            for(int i=0;i<6;i++)
            {
                float z=41f+i*3.8f;
                Box(stage,"UPPER GATE approach "+i,new Vector3(0,14.12f,z),new Vector3(9f,.22f,3.7f),path);
            }
            // Low-cost modular masonry: visible stone block rhythm above the existing flat road.
            // The slabs are new Unity primitives, not paid assets or fake visual screenshots.
            Material paverA=Material("road-paver-light",shader,new Color(.80f,.75f,.68f),
                "Assets/Eldoria/ArtTests/OriginalHero/Textures/limestone.png");
            Material paverB=Material("road-paver-ochre",shader,new Color(.69f,.64f,.56f),
                "Assets/Eldoria/ArtTests/OriginalHero/Textures/limestone.png");
            int pavingCount=0;
            // Bridge spans the water; two offset stone pavers per course.
            for(int n=0;n<13;n++)for(int side=0;side<2;side++)
            {
                float z=-116f+n*3.3f;
                float x=side==0?-2.03f:2.03f;
                Box(stage,"Bridge jointed flagstone "+n+" "+side,new Vector3(x,6.15f,z),
                    new Vector3(4.05f,.055f,3.07f),(n+side)%3==0?paverB:paverA);
                pavingCount++;
            }
            for(int n=0;n<26;n++)for(int side=0;side<2;side++)
            {
                float z=-74f+n*3.8f;
                if(z>=24f)continue;
                float x=side==0?-1.90f:1.90f;
                Box(stage,"Central old paving joint "+n+" "+side,new Vector3(x,6.27f,z),
                    new Vector3(3.66f,.07f,3.54f),(n+side)%4==0?paverB:paverA);
                pavingCount++;
            }
            var banner=Material("banner-royal-blue",shader,new Color(.09f,.18f,.65f),null);
            var amber=Material("torch-amber",shader,new Color(1f,.49f,.19f),null);
            foreach(float x in new[]{-8f,8f})
                Box(buildings,"Lower blue banner "+x,new Vector3(x,13.8f,-79f),new Vector3(2.3f,5.2f,.12f),banner);
            Box(buildings,"Upper blue banner",new Vector3(0,22f,62f),new Vector3(2.4f,5.4f,.12f),banner);
            foreach(float x in new[]{-7f,7f})
            {
                Torch(buildings,"Lower fire "+x,new Vector3(x,9.3f,-82f),amber);
                Torch(buildings,"Upper fire "+x,new Vector3(x,16f,59f),amber);
            }

            // No floating distant trees until mountain meshes prove to be a valid background.
            int distantTrees=0;
            // Correct placement: trees are rooted ON the plateau, never floating in water.
            int forest=0;
            for(int i=0;i<74;i++)
            {
                float t=(i+.3f)*2f*Mathf.PI/74f;
                float x=Mathf.Cos(t)*(58f+(i%6)*1.65f)+Mathf.Sin(i*12.98f)*3.1f;
                float z=-8f+Mathf.Sin(t)*(68f+(i%5)*1.9f)+Mathf.Cos(i*8.19f)*2.5f;
                if(z>27f || (z < -46f && Mathf.Abs(x) < 32f) || (i%11==0))continue;
                Place(tree,nature,"Lower plateau rooted conifer "+i,x,z,5.95f,
                      9f+(i%6)*1.2f,i*47f);
                forest++;
            }
            // Upper tree belt belongs behind the top wall and grows on the upper terrace.
            for(int i=0;i<25;i++)
            {
                float t=(i+.3f)*Mathf.PI/25f;
                float x=Mathf.Cos(t)*45f+Mathf.Sin(i*7.13f)*2.5f;
                float z=72f+Mathf.Sin(t)*8f+Mathf.Cos(i*3.9f)*2f;
                Place(tree,nature,"Upper rooted pine belt "+i,x,z,14.05f,
                      12f+(i%4)*1.1f,i*39f);
                forest++;
            }
            int rocks=0;
            for(int i=0;i<44;i++)
            {
                float t=(i+.2f)*2f*Mathf.PI/44f;
                float x=Mathf.Cos(t)*(65f+(i%3)*1.2f);
                float z=-8f+Mathf.Sin(t)*(75f+(i%4));
                if(z>28f || (z < -64f && Mathf.Abs(x)<17f))continue;
                Place(rock,nature,"Rooted escarpment boulder "+i,x,z,5.85f,
                      2.2f+(i%3)*1.0f,i*29f);
                rocks++;
            }
            for(int i=0;i<12;i++)
            {
                float x=-42f+(i%6)*16.8f;
                float z=75f+(i/6)*4f;
                Place(rock,nature,"Upper wall rubble "+i,x,z,14f,
                      1.6f+(i%3)*.55f,i*43f);
                rocks++;
            }
            RenderSettings.ambientMode=AmbientMode.Flat;
            RenderSettings.ambientLight=new Color(.36f,.37f,.40f);
            RenderSettings.fog=true;
            RenderSettings.fogMode=FogMode.ExponentialSquared;
            RenderSettings.fogColor=new Color(.31f,.40f,.52f);
            RenderSettings.fogDensity=.0029f;
            var sun=new GameObject("Warm low dusk key").AddComponent<Light>();
            sun.type=LightType.Directional;
            sun.color=new Color(1f,.84f,.69f);
            sun.intensity=1.45f;
            sun.shadows=LightShadows.Soft;
            sun.transform.rotation=Quaternion.Euler(43f,-34f,0f);
            var cam=new GameObject("Original 3D mountain V9 camera").AddComponent<Camera>();
            cam.orthographic=true;
            cam.clearFlags=CameraClearFlags.SolidColor;
            cam.backgroundColor=RenderSettings.fogColor;
            cam.nearClipPlane=.1f;cam.farClipPlane=650f;
            string sceneFile=Root+"/ReferenceFirstCompositionV9.unity";
            if(!EditorSceneManager.SaveScene(scene,sceneFile))throw new IOException("Cannot save isolated composition scene");
            AssetDatabase.SaveAssets();
            string folder=Path.GetFullPath(Path.Combine(UnityEngine.Application.dataPath,"../../artifacts/reference-first-composition-v9"));
            Directory.CreateDirectory(folder);
            // Reference-comparable elevated view, now against a TRUE multi-depth Alpine valley.
            Save(cam,folder,"17-real-mountains-overview.png",new Vector3(47,92,-137),new Vector3(0,10,-3),78f);
            Save(cam,folder,"18-real-mountains-close.png",new Vector3(37,82,-122),new Vector3(0,10,-7),66f);
            var report=new Report{revision=Environment.GetEnvironmentVariable("GITHUB_SHA")??"LOCAL",
                architectureInstances=architecture,forestInstances=forest,rockInstances=rocks,
                distantTreeInstances=distantTrees,pavingStoneInstances=pavingCount,
                mountainSourceInstances=mountainCount};
            File.WriteAllText(Path.Combine(folder,"render-manifest-v9.json"),JsonUtility.ToJson(report,true));
            Debug.Log("REFERENCE_FIRST_V9_RENDERED: architecture="+architecture+" trees="+forest+" rocks="+rocks+" ART_APPROVED=false");
        }

        // Repeatable, no-cost organic terrain map saved as a Unity-native Texture2D asset.
        static Texture2D OrganicGroundTexture()
        {
            string assetPath=Root+"/Materials/OrganicTerrainBase.asset";
            var tex=AssetDatabase.LoadAssetAtPath<Texture2D>(assetPath);
            if(tex!=null)return tex;
            const int res=512;
            tex=new Texture2D(res,res,TextureFormat.RGB24,true);
            tex.name="ValoriaV4 OrganicTerrainBase";
            tex.wrapMode=TextureWrapMode.Repeat;
            tex.filterMode=FilterMode.Trilinear;
            var pixels=new Color[res*res];
            for(int y=0;y<res;y++) for(int x=0;x<res;x++)
            {
                float fx=(float)x/res,fy=(float)y/res;
                float big=Mathf.PerlinNoise(fx*4.5f+11.4f,fy*4.5f+17.7f);
                float mid=Mathf.PerlinNoise(fx*22f+3.7f,fy*22f+8.9f);
                float fine=Mathf.PerlinNoise(fx*85f+1.2f,fy*85f+4.1f);
                float earth=Mathf.SmoothStep(0f,1f,Mathf.Clamp01((big*.78f+mid*.22f-.40f)*3.3f));
                var col=Color.Lerp(new Color(.38f,.38f,.21f),new Color(.46f,.36f,.23f),earth*.75f);
                float g=(mid-.5f)*.09f+(fine-.5f)*.052f;
                pixels[y*res+x]=new Color(Mathf.Clamp01(col.r+g),Mathf.Clamp01(col.g+g),Mathf.Clamp01(col.b+g*.55f));
            }
            tex.SetPixels(pixels);tex.Apply(true,false);
            AssetDatabase.CreateAsset(tex,assetPath);
            AssetDatabase.SaveAssets();
            return tex;
        }
        // Overrides only live scene instances; original mesh source and importer data are untouched.
        static int HarmonizeArchitecture(Transform parent,Material stone,Material timber,
                                          Material iron,Material flag)
        {
            int count=0;
            foreach(var r in parent.GetComponentsInChildren<Renderer>(true))
            {
                var slots=r.sharedMaterials;
                if(slots==null || slots.Length==0)continue;
                string node=r.gameObject.name.ToLowerInvariant();
                bool altered=false;
                for(int i=0;i<slots.Length;i++)
                {
                    string mat=slots[i]!=null?slots[i].name.ToLowerInvariant():"";
                    string key=node+" "+mat;
                    Material chosen=stone;
                    if(key.Contains("banner")||key.Contains("flag")||key.Contains("herald")||key.Contains("cloth"))
                        chosen=flag;
                    else if(key.Contains("timber")||key.Contains("wood")||key.Contains("oak")||key.Contains("beam")||key.Contains("plank"))
                        chosen=timber;
                    else if(key.Contains("portcullis")||key.Contains("iron")||key.Contains("grill")||key.Contains("metal"))
                        chosen=iron;
                    if(slots[i]!=chosen){slots[i]=chosen;altered=true;count++;}
                }
                if(altered)r.sharedMaterials=slots;
            }
            return count;
        }
        static void Torch(Transform parent,string name,Vector3 pos,Material fire)
        {
            Box(parent,name+" bowl",pos,new Vector3(.9f,.5f,.8f),fire);
            var light=new GameObject(name+" warm light").AddComponent<Light>();
            light.transform.SetParent(parent,true);
            light.transform.position=pos+new Vector3(0,.6f,-.6f);
            light.type=LightType.Point;
            light.color=new Color(1f,.56f,.20f);
            light.range=12f;
            light.intensity=3f;
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
        // Three-level cliff face adds structural depth and irregular fractures, while retaining
        // a connected low-poly geological mesh and the approved one-plateau silhouette.
        static void Island(Transform parent,string name,float cx,float cz,float rx,float rz,float high,float low,Material grass,Material rock)
        {
            const int N=96;
            var cap=new Vector3[N+1];var capUV=new Vector2[N+1];var capTris=new int[N*3];
            var walls=new Vector3[N*3];var wallUV=new Vector2[N*3];var wallTris=new int[N*12];
            cap[0]=new Vector3(cx,high,cz);
            for(int i=0;i<N;i++)
            {
                float a=i*2f*Mathf.PI/N;
                float shape=1f+.052f*Mathf.Sin(i*.59f)+.041f*Mathf.Cos(i*1.53f)+
                            .015f*Mathf.Sin(i*2.23f)+.014f*Mathf.Sin(i*4.13f);
                float x=cx+Mathf.Cos(a)*rx*shape,z=cz+Mathf.Sin(a)*rz*shape;
                cap[i+1]=new Vector3(x,high,z);
                capUV[i+1]=new Vector2(x*.04f,z*.04f);
                Vector3 outward=new Vector3(Mathf.Cos(a),0,Mathf.Sin(a));
                walls[3*i]=new Vector3(x,high,z);
                walls[3*i+1]=new Vector3(x,high-(high-low)*.54f + Mathf.Sin(i*.97f)*1.0f,z)
                             +outward*(1.15f+Mathf.Sin(i*1.87f)*.65f);
                walls[3*i+2]=new Vector3(x,low+Mathf.Cos(i*.71f)*.66f,z)
                             +outward*(3.8f+Mathf.Sin(i*2.41f)*1.45f);
                wallUV[3*i]=new Vector2(i*.17f,1);
                wallUV[3*i+1]=new Vector2(i*.17f,.5f);
                wallUV[3*i+2]=new Vector2(i*.17f,0);
                int next=(i+1)%N;
                capTris[3*i]=0;capTris[3*i+1]=next+1;capTris[3*i+2]=i+1;
                for(int layer=0;layer<2;layer++)
                {
                    int index=i*12+layer*6;
                    int p=i*3+layer,q=next*3+layer;
                    wallTris[index]=p;wallTris[index+1]=q;wallTris[index+2]=p+1;
                    wallTris[index+3]=q;wallTris[index+4]=q+1;wallTris[index+5]=p+1;
                }
            }
            MeshObject(parent,name+" stable grassy cap",cap,capUV,capTris,grass);
            MeshObject(parent,name+" continuous stratified cliff face",walls,wallUV,wallTris,rock);
        }

        static float LandscapeHeight(float x,float z)
        {
            float outer=Mathf.Abs(x);
            float ridgeline=Mathf.Clamp01((outer-62f)/156f);
            float back=Mathf.Clamp01((z-90f)/200f);
            float softNoise=Mathf.PerlinNoise(x*.021f+5.3f,z*.018f+3.7f);
            float ridgeNoise=Mathf.PerlinNoise(x*.008f+18f,z*.012f+11f);
            // Valley depressed at its centre; distant mountain shoulders rise left and right.
            float shoulder=Mathf.Clamp01((outer-68f)/78f);
            float backShoulder=Mathf.Clamp01((z-76f)/48f);
            return -31f+ridgeline*55f+back*(20f+ridgeline*43f)+
                   backShoulder*shoulder*28f+
                   (softNoise-.5f)*10f+(ridgeNoise-.5f)*21f;
        }
        static void DistantValley(Transform root,Material near,Material far)
        {
            const int cols=62,rows=56;
            const float xmin=-345f,xmax=345f,zmin=-195f,zmax=350f;
            var points=new Vector3[(cols+1)*(rows+1)];
            var tex=new Vector2[points.Length];
            for(int j=0;j<=rows;j++)
            for(int i=0;i<=cols;i++)
            {
                float x=Mathf.Lerp(xmin,xmax,(float)i/cols);
                float z=Mathf.Lerp(zmin,zmax,(float)j/rows);
                int p=j*(cols+1)+i;
                points[p]=new Vector3(x,LandscapeHeight(x,z),z);
                tex[p]=new Vector2(x*.025f,z*.025f);
            }
            var tri=new int[cols*rows*6];
            for(int j=0;j<rows;j++)
            for(int i=0;i<cols;i++)
            {
                int p=j*(cols+1)+i,q=(j*cols+i)*6;
                tri[q]=p;tri[q+1]=p+cols+1;tri[q+2]=p+1;
                tri[q+3]=p+1;tri[q+4]=p+cols+1;tri[q+5]=p+cols+2;
            }
            MeshObject(root,"Distant Alpine valley - 3D native terrain mesh",points,tex,tri,near);
            // In V7 mountains at z=345 were outside the useful image. V8 makes BOTH
            // mountain bands visible behind the rear fortification at this actual camera.
            MountainRibbon(root,"Near valley mountain ridge",100f,8f,15f,near);
            MountainRibbon(root,"Far blue atmospheric mountain",138f,-9f,12f,far);
        }
        // Double-faced native geometry: mountain silhouette remains visible under URP
        // irrespective of material culling and cannot become an empty uniform sky.
        static void MountainRibbon(Transform root,string name,float z,float baseY,float amplitude,Material mat)
        {
            const int N=72;
            var vertices=new Vector3[(N+1)*2];
            var uvs=new Vector2[vertices.Length];
            var tri=new int[N*12];
            for(int i=0;i<=N;i++)
            {
                float x=Mathf.Lerp(-255f,255f,(float)i/N);
                float centerDip=Mathf.Clamp01(Mathf.Abs(x)/155f);
                float peak=baseY+centerDip*16f+amplitude*
                        (.38f*Mathf.Sin(i*.77f)+.4f*Mathf.Sin(i*.19f)+
                        .22f*Mathf.Sin(i*1.33f));
                vertices[2*i]=new Vector3(x,peak,z);
                vertices[2*i+1]=new Vector3(x,-86f,z);
                uvs[2*i]=new Vector2(i*.085f,1);
                uvs[2*i+1]=new Vector2(i*.085f,0);
                if(i<N)
                {
                    int p=i*2,k=i*12;
                    tri[k]=p;tri[k+1]=p+2;tri[k+2]=p+1;
                    tri[k+3]=p+2;tri[k+4]=p+3;tri[k+5]=p+1;
                    // Reverse triangles as a second face to avoid single-sided culling.
                    tri[k+6]=p+1;tri[k+7]=p+2;tri[k+8]=p;
                    tri[k+9]=p+1;tri[k+10]=p+3;tri[k+11]=p+2;
                }
            }
            MeshObject(root,name+" TRUE 3D cliff backdrop",vertices,uvs,tri,mat);
        }

        static void IrregularWater(Transform parent,Material mat)
        {
            // Non-rectangular footprint; winding river under the lower bridge near screen edge.
            Vector2[] outline={
                new Vector2(-72,-160),new Vector2(-46,-165),new Vector2(-18,-162),
                new Vector2(21,-155),new Vector2(65,-147),new Vector2(70,-116),
                new Vector2(36,-107),new Vector2(-5,-111),new Vector2(-35,-104),
                new Vector2(-69,-126)
            };
            var verts=new Vector3[outline.Length+1];
            var uv=new Vector2[verts.Length];
            var indices=new int[outline.Length*6];
            verts[0]=new Vector3(0,-13.45f,-136f);
            for(int i=0;i<outline.Length;i++)
            {
                verts[i+1]=new Vector3(outline[i].x,-13.45f,outline[i].y);
                uv[i+1]=new Vector2(outline[i].x*.015f,outline[i].y*.015f);
                int next=(i+1)%outline.Length,k=6*i;
                indices[k]=0;indices[k+1]=i+1;indices[k+2]=next+1;
                indices[k+3]=0;indices[k+4]=next+1;indices[k+5]=i+1;
            }
            MeshObject(parent,"Water channel with IRREGULAR shore, native 3D mesh",verts,uv,indices,mat);
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
                cam.targetTexture=rt;
                // V4/V5 first view glitched on stone/vegetation palette; force warmup per camera.
                cam.Render();cam.Render();cam.Render();
                RenderTexture.active=rt;
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
