using System;
using System.IO;
using System.Threading.Tasks;
using GLTFast;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using Object = UnityEngine.Object;

namespace Eldoria.EditorTools
{
    // Isolated production-asset candidate. No modifications to Valoria's playable scene.
    public static class ValoriaGatehouseRealUnityV1
    {
        const string AssetPath = "Assets/Eldoria/ArtTests/ValoriaGatehouseV1/valoria_gate_reference.glb";
        const string PriorPath = "Assets/Eldoria/Resources/Valoria/ProductionArt/CoherentFamilyV1/Valoria_MainGate_CohV1.glb";
        const string Output = "ValoriaGatehouseRealUnityV1Captures";

        public static async void Capture()
        {
            try { await Execute(); EditorApplication.Exit(0); }
            catch(Exception ex) { Debug.LogException(ex); EditorApplication.Exit(1); }
        }

        static async Task Execute()
        {
            Directory.CreateDirectory(Output);
            SceneSetup.SetupRenderPipeline();
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
            var root = new GameObject("Blender authored gatehouse candidate");
            var gltf = new GltfImport(null, new UninterruptedDeferAgent());
            var path = Path.GetFullPath(AssetPath);
            if(!File.Exists(path)) throw new FileNotFoundException(path);
            if(!await gltf.LoadFile(path)) throw new Exception("glTFast could not load exact Blender GLB");
            if(!await gltf.InstantiateMainSceneAsync(root.transform)) throw new Exception("glTFast scene instantiation failed");
            var renderers=root.GetComponentsInChildren<Renderer>(true);
            if(renderers.Length==0) throw new Exception("GLB instantiated without renderers");
            var bounds=renderers[0].bounds;
            foreach(var r in renderers) bounds.Encapsulate(r.bounds);
            if(bounds.size.x<0.001f) throw new Exception("GLB invalid width");
            root.transform.localScale*=17.3f/bounds.size.x;
            renderers=root.GetComponentsInChildren<Renderer>(true);
            bounds=renderers[0].bounds;
            foreach(var r in renderers) bounds.Encapsulate(r.bounds);
            root.transform.position-=new Vector3(bounds.center.x,bounds.min.y,bounds.center.z);
            var light=new GameObject("Valoria matched sunlight").AddComponent<Light>();
            light.type=LightType.Directional;light.intensity=1.2f;light.shadows=LightShadows.Soft;
            light.transform.rotation=Quaternion.Euler(42f,-35f,0f);
            RenderSettings.ambientMode=AmbientMode.Flat;
            RenderSettings.ambientLight=new Color(.48f,.52f,.58f);
            var camera=new GameObject("Valoria strategic orthographic camera").AddComponent<Camera>();
            camera.orthographic=true;camera.orthographicSize=9f;
            camera.clearFlags=CameraClearFlags.SolidColor;camera.backgroundColor=new Color(.27f,.32f,.38f);
            camera.transform.position=new Vector3(18.2f,14.6f,-25.8f);
            camera.transform.LookAt(new Vector3(0f,3.15f,5.8f));
            Save(camera,Output+"/unity-strategic-1280x720.png",1280,720);
            camera.transform.position=new Vector3(18.2f,14.6f,-31.6f);
            camera.transform.LookAt(new Vector3(0f,3.15f,0f));
            camera.orthographicSize=16f;
            Save(camera,Output+"/unity-mobile-390x844.png",390,844);
            var wallPath="Assets/Eldoria/Resources/Valoria/ProductionArt/CoherentFamilyV1/Valoria_WallSegment_CohV1.glb";
            var wall=new GameObject("Original wall segment left");
            var wallImport=new GltfImport(null,new UninterruptedDeferAgent());
            if(!await wallImport.LoadFile(Path.GetFullPath(wallPath)))throw new Exception("Existing wall GLB could not load");
            if(!await wallImport.InstantiateMainSceneAsync(wall.transform))throw new Exception("Existing wall GLB instantiate failed");
            var wallRenderers=wall.GetComponentsInChildren<Renderer>();
            var wb=wallRenderers[0].bounds;foreach(var wr in wallRenderers)wb.Encapsulate(wr.bounds);
            wall.transform.localScale*=6f/wb.size.x;
            wallRenderers=wall.GetComponentsInChildren<Renderer>();wb=wallRenderers[0].bounds;
            foreach(var wr in wallRenderers)wb.Encapsulate(wr.bounds);
            wall.transform.position=new Vector3(-11.65f-wb.center.x,-wb.min.y,-wb.center.z);
            var wallRight=Object.Instantiate(wall);
            wallRight.name="Original wall segment right";
            wallRight.transform.position+=new Vector3(23.3f,0f,0f);
            camera.transform.position=new Vector3(18.2f,14.6f,-25.8f);
            camera.transform.LookAt(new Vector3(0f,3.15f,5.8f));
            camera.orthographicSize=12f;
            Save(camera,Output+"/unity-gate-with-original-walls-1280x720.png",1280,720);
            wall.SetActive(false);wallRight.SetActive(false);
            var prior=new GameObject("Current Valoria architecture comparison");
            var priorImport=new GltfImport(null,new UninterruptedDeferAgent());
            if(!await priorImport.LoadFile(Path.GetFullPath(PriorPath)))throw new Exception("Cannot load existing Valoria gate");
            if(!await priorImport.InstantiateMainSceneAsync(prior.transform))throw new Exception("Cannot instantiate existing Valoria gate");
            var previousParts=prior.GetComponentsInChildren<Renderer>();
            var previousBounds=previousParts[0].bounds;
            foreach(var p in previousParts)previousBounds.Encapsulate(p.bounds);
            prior.transform.localScale*=17.3f/previousBounds.size.x;
            previousParts=prior.GetComponentsInChildren<Renderer>();
            previousBounds=previousParts[0].bounds;
            foreach(var p in previousParts)previousBounds.Encapsulate(p.bounds);
            prior.transform.position-=new Vector3(previousBounds.center.x,previousBounds.min.y,previousBounds.center.z);
            root.SetActive(false);
            camera.transform.position=new Vector3(18.2f,14.6f,-25.8f);
            camera.transform.LookAt(new Vector3(0f,3.15f,5.8f));
            camera.orthographicSize=9f;
            Save(camera,Output+"/unity-existing-gate-1280x720.png",1280,720);
            prior.SetActive(false);root.SetActive(true);
            // Visual vertical slice: isolated environment around the authored gateway.
            prior.SetActive(false); root.SetActive(true); wall.SetActive(true); wallRight.SetActive(true);
            var world = new GameObject("Valoria Bastion I - visual vertical slice");
            var grass = MaterialOf("Highland meadow",new Color(.34f,.39f,.20f));
            var rock = MaterialOf("Stratified slate cliffs",new Color(.28f,.30f,.35f));
            var pathMat = MaterialOf("Warm cobbled approach",new Color(.48f,.39f,.31f));
            var pine = MaterialOf("Pine dark needles",new Color(.105f,.18f,.13f));
            var wood = MaterialOf("Pine trunk",new Color(.23f,.16f,.11f));
            var undergrowth = MaterialOf("Undergrowth",new Color(.25f,.32f,.16f));
            // Terrain is continuous under the courtyard, with a descending outer escarpment.
            BuildPlateau(world.transform,grass,rock);
            RenderSettings.fog=true; RenderSettings.fogColor=new Color(.34f,.39f,.45f); RenderSettings.fogMode=FogMode.Linear; RenderSettings.fogStartDistance=85f; RenderSettings.fogEndDistance=180f;
            // Reuse authored fortification at the upper defensive terrace.
            var upper=Object.Instantiate(root);
            upper.name="Upper Valoria fortified gatehouse";
            upper.transform.SetParent(world.transform,true);
            upper.transform.localScale*=.79f;
            upper.transform.position=new Vector3(0f,HeightAt(0f,27f),27f);
            var upperWallLeft=Object.Instantiate(wall);
            upperWallLeft.name="Upper western curtain wall";
            upperWallLeft.transform.SetParent(world.transform,true);
            upperWallLeft.transform.localScale*=.78f;
            upperWallLeft.transform.position=new Vector3(-9.1f,HeightAt(-9f,27f),27f);
            var upperWallRight=Object.Instantiate(wallRight);
            upperWallRight.name="Upper eastern curtain wall";
            upperWallRight.transform.SetParent(world.transform,true);
            upperWallRight.transform.localScale*=.78f;
            upperWallRight.transform.position=new Vector3(9.1f,HeightAt(9f,27f),27f);
            // Route winds in from the south; individually placed pavers avoid flat decal geometry.
            for(int i=0;i<70;i++){
                float z=-40f+i*.83f;
                float x=1.8f*Mathf.Sin(z*.075f);
                var paving=GameObject.CreatePrimitive(PrimitiveType.Cube);
                paving.name="Approach stone "+i;
                paving.transform.SetParent(world.transform);
                paving.transform.position=new Vector3(x,HeightAt(x,z)+.08f,z);
                paving.transform.localScale=new Vector3(3.0f,.09f,.72f);
                paving.GetComponent<Renderer>().sharedMaterial=pathMat;
                Object.DestroyImmediate(paving.GetComponent<Collider>());
            }
            var rand=new System.Random(93011);
            for(int i=0;i<105;i++){
                float a=(float)rand.NextDouble()*Mathf.PI*2f;
                float r=12f+(float)rand.NextDouble()*31f;
                float x=Mathf.Cos(a)*r, z=Mathf.Sin(a)*r;
                if(Mathf.Abs(x)<5f && z>-40f && z<28f)continue;
                if(Mathf.Abs(x)>36f || Mathf.Abs(z)>36f)continue;
                float h=2.0f+(float)rand.NextDouble()*4.7f;
                MakePine(world.transform,new Vector3(x,HeightAt(x,z),z),h,wood,pine);
            }
            for(int i=0;i<52;i++){
                float angle=(float)rand.NextDouble()*Mathf.PI*2f;
                float r=36f+(float)rand.NextDouble()*4f;
                float bx=Mathf.Cos(angle)*r,bz=Mathf.Sin(angle)*r;
                var boulder=GameObject.CreatePrimitive(PrimitiveType.Sphere);
                boulder.name="Cliff talus and weathered boulder";
                boulder.transform.SetParent(world.transform);
                boulder.transform.position=new Vector3(bx,HeightAt(bx,bz)-.4f,bz);
                boulder.transform.localScale=new Vector3(1.1f+(float)rand.NextDouble()*2.1f,.7f+(float)rand.NextDouble()*1.7f,.9f+(float)rand.NextDouble()*1.7f);
                boulder.GetComponent<Renderer>().sharedMaterial=rock;
                Object.DestroyImmediate(boulder.GetComponent<Collider>());
            }
            for(int i=0;i<150;i++){
                float x=(float)(rand.NextDouble()*72-36),z=(float)(rand.NextDouble()*72-36);
                if(Mathf.Abs(x)<4f&&z>-40f&&z<30f)continue;
                var brush=GameObject.CreatePrimitive(PrimitiveType.Sphere);
                brush.name="Heather and brush";
                brush.transform.SetParent(world.transform);
                brush.transform.position=new Vector3(x,HeightAt(x,z)+.2f,z);
                brush.transform.localScale=new Vector3(.55f+.7f*(float)rand.NextDouble(),.35f,.6f);
                brush.GetComponent<Renderer>().sharedMaterial=undergrowth;
                Object.DestroyImmediate(brush.GetComponent<Collider>());
            }
            camera.transform.position=new Vector3(44f,34f,-57f);
            camera.transform.LookAt(new Vector3(0f,0f,0f));
            camera.orthographicSize=35f;
            Save(camera,Output+"/unity-valoria-slice-landscape-1280x720.png",1280,720);
            camera.transform.position=new Vector3(42f,38f,-56f);
            camera.transform.LookAt(new Vector3(0f,0f,-3f));
            camera.orthographicSize=38f;
            Save(camera,Output+"/unity-valoria-slice-portrait-390x844.png",390,844);
            var scenePath="Assets/Eldoria/ArtTests/ValoriaGatehouseV1/ValoriaVisualSliceV1.unity";
            EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene(),scenePath);
            long worldTriangles=0;int worldSlots=0;int worldRenderers=0;
            foreach(var mf in world.GetComponentsInChildren<MeshFilter>()){
                if(mf.sharedMesh!=null)for(int si=0;si<mf.sharedMesh.subMeshCount;si++)worldTriangles+=mf.sharedMesh.GetIndexCount(si)/3;
            }
            foreach(var rend in world.GetComponentsInChildren<Renderer>()){worldSlots+=rend.sharedMaterials.Length;worldRenderers++;}
            File.WriteAllText(Output+"/slice-metrics.txt",
                "environment_triangles="+worldTriangles+Environment.NewLine+
                "environment_renderers="+worldRenderers+Environment.NewLine+
                "environment_material_slots="+worldSlots+Environment.NewLine+
                "includes_original_gatehouse=true"+Environment.NewLine+
                "includes_original_walls=true"+Environment.NewLine+
                "mobile_device_tested=false"+Environment.NewLine);
            long triangles=0;int materials=0;
            foreach(var mesh in root.GetComponentsInChildren<MeshFilter>(true))
                if(mesh.sharedMesh!=null) for(int i=0;i<mesh.sharedMesh.subMeshCount;i++)triangles+=mesh.sharedMesh.GetIndexCount(i)/3;
            foreach(var renderer in renderers)materials+=renderer.sharedMaterials.Length;
            File.WriteAllText(Output+"/metrics.json",
                "{\n  \"source\": \"exact authored Blender GLB\",\n  \"triangles_instantiated\": "+triangles+
                ",\n  \"renderer_count\": "+renderers.Length+",\n  \"material_slots\": "+materials+
                ",\n  \"reference_source\": \""+PriorPath+"\",\n  \"physical_mobile_tested\": false\n}\n");
            // A Unity snapshot is not an artistic approval or proof of runtime parity.
            Debug.Log("GATEHOUSE_UNITY_CAPTURE_COMPLETE "+Path.GetFullPath(Output));
        }

        static Material MaterialOf(string name,Color color){
            var m=new Material(Shader.Find("Universal Render Pipeline/Lit")??Shader.Find("Standard"));
            m.name=name;m.color=color;return m;
        }
        static float HeightAt(float x,float z){
            float rise=Mathf.SmoothStep(0f,5.2f,Mathf.InverseLerp(8f,26f,z));
            return -.25f+rise+.35f*Mathf.Sin(x*.18f)*Mathf.Cos(z*.14f);
        }

        static void BuildPlateau(Transform parent,Material grass,Material cliff){
            const int sectors=96, rings=18;
            float radius=41f;
            var verts=new Vector3[(rings+1)*(sectors+1)];
            var indices=new int[rings*sectors*6];
            for(int ring=0;ring<=rings;ring++){
                float fraction=(float)ring/rings;
                for(int sec=0;sec<=sectors;sec++){
                    float angle=sec*Mathf.PI*2f/sectors;
                    float perimeter=radius+2.0f*Mathf.Sin(angle*7f)+1.1f*Mathf.Cos(angle*13f);
                    float x=Mathf.Cos(angle)*perimeter*fraction;
                    float z=Mathf.Sin(angle)*perimeter*fraction;
                    verts[ring*(sectors+1)+sec]=new Vector3(x,HeightAt(x,z),z);
                }
            }
            int k=0;
            for(int ring=0;ring<rings;ring++)for(int sec=0;sec<sectors;sec++){
                int a=ring*(sectors+1)+sec,b=a+1,c=a+sectors+1,d=c+1;
                indices[k++]=a;indices[k++]=b;indices[k++]=c;
                indices[k++]=b;indices[k++]=d;indices[k++]=c;
            }
            var surface=new Mesh{indexFormat=UnityEngine.Rendering.IndexFormat.UInt32,vertices=verts,triangles=indices};
            surface.RecalculateNormals();
            var ground=new GameObject("Organic circular highland grass surface");
            ground.transform.SetParent(parent);
            ground.AddComponent<MeshFilter>().sharedMesh=surface;
            ground.AddComponent<MeshRenderer>().sharedMaterial=grass;
            var edge=new Vector3[(sectors+1)*3];
            var faces=new int[sectors*12];
            for(int sec=0;sec<=sectors;sec++){
                float ang=sec*Mathf.PI*2f/sectors;
                float outer=radius+2f*Mathf.Sin(ang*7f)+1.1f*Mathf.Cos(ang*13f);
                float x=outer*Mathf.Cos(ang),z=outer*Mathf.Sin(ang);
                int i=sec*3;
                edge[i]=new Vector3(x,HeightAt(x,z)-.05f,z);
                edge[i+1]=new Vector3((outer+1.1f)*Mathf.Cos(ang),-3.8f+Mathf.Sin(ang*17f)*.6f,(outer+1.1f)*Mathf.Sin(ang));
                edge[i+2]=new Vector3((outer+3.6f)*Mathf.Cos(ang),-9.5f+Mathf.Sin(ang*11f)*1.3f,(outer+3.6f)*Mathf.Sin(ang));
            }
            for(int sec=0;sec<sectors;sec++){
                int a=3*sec,b=a+1,c=a+2,d=a+3,e=d+1,f=d+2,j=sec*12;
                faces[j]=a;faces[j+1]=d;faces[j+2]=b;
                faces[j+3]=d;faces[j+4]=e;faces[j+5]=b;
                faces[j+6]=b;faces[j+7]=e;faces[j+8]=c;
                faces[j+9]=e;faces[j+10]=f;faces[j+11]=c;
            }
            var strata=new Mesh{indexFormat=UnityEngine.Rendering.IndexFormat.UInt32,vertices=edge,triangles=faces};
            strata.RecalculateNormals();
            var cliffs=new GameObject("Jagged contiguous cliff ring");
            cliffs.transform.SetParent(parent);
            cliffs.AddComponent<MeshFilter>().sharedMesh=strata;
            cliffs.AddComponent<MeshRenderer>().sharedMaterial=cliff;
        }
        static void MakePine(Transform parent,Vector3 position,float height,Material wood,Material pine){
            var trunk=GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            trunk.name="Alpine pine trunk";
            trunk.transform.SetParent(parent);
            trunk.transform.position=position+Vector3.up*height*.33f;
            trunk.transform.localScale=new Vector3(.09f,height*.34f,.09f);
            trunk.GetComponent<Renderer>().sharedMaterial=wood;
            Object.DestroyImmediate(trunk.GetComponent<Collider>());
            for(int level=0;level<3;level++){
                float baseY=height*(.36f+level*.19f);
                float crownHeight=height*(.43f-level*.07f);
                float crownRadius=height*(.17f-level*.045f);
                const int sides=7;
                var v=new Vector3[sides+1];var ix=new int[sides*3];
                for(int i=0;i<sides;i++){
                    float a=i*Mathf.PI*2f/sides;
                    v[i]=new Vector3(crownRadius*Mathf.Cos(a),0f,crownRadius*Mathf.Sin(a));
                    ix[i*3]=i;ix[i*3+1]=sides;ix[i*3+2]=(i+1)%sides;
                }
                v[sides]=new Vector3(0,crownHeight,0);
                var mesh=new Mesh{vertices=v,triangles=ix};mesh.RecalculateNormals();
                var section=new GameObject("Tapered evergreen branches");
                section.transform.SetParent(parent);
                section.transform.position=position+Vector3.up*baseY;
                section.AddComponent<MeshFilter>().sharedMesh=mesh;
                section.AddComponent<MeshRenderer>().sharedMaterial=pine;
            }
        }
        static void Save(Camera camera,string name,int w,int h)
        {
            var rt=new RenderTexture(w,h,24,RenderTextureFormat.ARGB32){antiAliasing=4};
            var old=RenderTexture.active;
            try {
                camera.targetTexture=rt;camera.Render();camera.Render();
                RenderTexture.active=rt;
                var pixels=new Texture2D(w,h,TextureFormat.RGB24,false);
                pixels.ReadPixels(new Rect(0,0,w,h),0,0);pixels.Apply();
                File.WriteAllBytes(name,pixels.EncodeToPNG());Object.DestroyImmediate(pixels);
            } finally {
                camera.targetTexture=null;RenderTexture.active=old;rt.Release();Object.DestroyImmediate(rt);
            }
        }
    }
}