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
            camera.transform.position=new Vector3(44f,36f,-57f);
            camera.transform.LookAt(new Vector3(0f,0f,0f));
            camera.orthographicSize=44f;
            Save(camera,Output+"/unity-valoria-slice-landscape-1280x720.png",1280,720);
            camera.transform.position=new Vector3(42f,39f,-56f);
            camera.transform.LookAt(new Vector3(0f,0f,-3f));
            camera.orthographicSize=47f;
            Save(camera,Output+"/unity-valoria-slice-portrait-390x844.png",390,844);
            var scenePath="Assets/Eldoria/ArtTests/ValoriaGatehouseV1/ValoriaVisualSliceV1.unity";
            EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene(),scenePath);
            long worldTriangles=0;int worldSlots=0;int worldRenderers=0;
            foreach(var mf in world.GetComponentsInChildren<MeshFilter>()){
                if(mf.sharedMesh!=null)for(int si=0;si<mf.sharedMesh.subMeshCount;si++)worldTriangles+=mf.sharedMesh.GetIndexCount(si)/3;
            }
            foreach(var rend in world.GetComponentsInChildren<Renderer>()){worldSlots+=rend.sharedMaterials.Length;worldRenderers++;}
            File.WriteAllText(Output+"/slice-metrics.json",
                "{\\n  \\"environment_triangles\\": "+worldTriangles+",\\n  \\"environment_renderers\\": "+worldRenderers+
                ",\\n  \\"environment_material_slots\\": "+worldSlots+
                ",\\n  \\"includes_original_gatehouse\\": true,\\n  \\"includes_original_walls\\": true,\\n  \\"mobile_device_tested\\": false\\n}"); 
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
            return -.25f+.35f*Mathf.Sin(x*.18f)*Mathf.Cos(z*.14f);
        }
        static void BuildPlateau(Transform parent,Material grass,Material cliff){
            int n=36;float step=2.2f;
            var verts=new Vector3[(n+1)*(n+1)];var tris=new int[n*n*6];
            for(int z=0;z<=n;z++)for(int x=0;x<=n;x++){
                float px=(x-n*.5f)*step,pz=(z-n*.5f)*step;
                verts[z*(n+1)+x]=new Vector3(px,HeightAt(px,pz)-.10f,pz);
            }
            int t=0;for(int z=0;z<n;z++)for(int x=0;x<n;x++){
                int a=z*(n+1)+x,b=a+1,c=a+n+1,d=c+1;
                tris[t++]=a;tris[t++]=c;tris[t++]=b;
                tris[t++]=b;tris[t++]=c;tris[t++]=d;
            }
            var mesh=new Mesh{indexFormat=UnityEngine.Rendering.IndexFormat.UInt32,vertices=verts,triangles=tris};
            mesh.RecalculateNormals();
            var ground=new GameObject("Continuous elevated courtyard terrain");
            ground.transform.SetParent(parent);
            ground.AddComponent<MeshFilter>().sharedMesh=mesh;
            ground.AddComponent<MeshRenderer>().sharedMaterial=grass;
            // Perimeter escarpment follows the terrain boundary rather than detached wall blocks.
            int segments=72;var v=new Vector3[(segments+1)*2];var idx=new int[segments*6];
            for(int i=0;i<=segments;i++){
                float ang=i*Mathf.PI*2f/segments;
                float c=Mathf.Cos(ang),sn=Mathf.Sin(ang);
                float r=35.5f+1.5f*Mathf.Sin(ang*5f);
                float px=c*r,pz=sn*r;
                v[2*i]=new Vector3(px,HeightAt(px,pz)-.15f);
                v[2*i].z=pz;
                v[2*i+1]=new Vector3(c*(r+3f),-7.5f-1.2f*Mathf.Sin(ang*7f),sn*(r+3f));
            }
            for(int i=0;i<segments;i++){
                int a=i*2,b=a+1,c=a+2,d=a+3,j=i*6;
                idx[j]=a;idx[j+1]=b;idx[j+2]=c;
                idx[j+3]=c;idx[j+4]=b;idx[j+5]=d;
            }
            var rockmesh=new Mesh{indexFormat=UnityEngine.Rendering.IndexFormat.UInt32,vertices=v,triangles=idx};
            rockmesh.RecalculateNormals();
            var cliffs=new GameObject("Continuous rocky escarpment");
            cliffs.transform.SetParent(parent);
            cliffs.AddComponent<MeshFilter>().sharedMesh=rockmesh;
            cliffs.AddComponent<MeshRenderer>().sharedMaterial=cliff;
        }
        static void MakePine(Transform parent,Vector3 position,float height,Material wood,Material pine){
            var trunk=GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            trunk.name="Forest pine";
            trunk.transform.SetParent(parent);
            trunk.transform.position=position+Vector3.up*height*.35f;
            trunk.transform.localScale=new Vector3(.19f,height*.35f,.19f);
            trunk.GetComponent<Renderer>().sharedMaterial=wood;
            Object.DestroyImmediate(trunk.GetComponent<Collider>());
            for(int i=0;i<3;i++){
                var crown=GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                crown.name="Needle canopy tier";
                crown.transform.SetParent(trunk.transform,true);
                crown.transform.position=position+Vector3.up*(height*(.55f+i*.17f));
                crown.transform.localScale=new Vector3(height*(.29f-i*.065f),height*.18f,height*(.29f-i*.065f));
                crown.GetComponent<Renderer>().sharedMaterial=pine;
                Object.DestroyImmediate(crown.GetComponent<Collider>());
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