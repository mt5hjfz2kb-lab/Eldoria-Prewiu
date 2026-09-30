using System;
using System.Collections.Generic;
using System.IO;
using Eldoria.Domain;
using Eldoria.Presentation;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;

namespace Eldoria.EditorTools
{
    /// <summary>
    /// Production-oriented transfer of the Golden Cell visual language.
    /// Goal: reproduce the useful visual read with reusable combined-mesh modules
    /// instead of hundreds of primitive helper renderers.
    /// Experimental branch only. Validation trigger v1.
    /// </summary>
    public static class ValoriaProductionModuleKitGate
    {
        const string Folder="ValoriaProductionModuleKitCaptures";
        static readonly Vector3 CameraPosition=new Vector3(18.2f,14.6f,-25.8f);
        static readonly Vector3 CameraTarget=new Vector3(0f,3.65f,7.25f);

        public static void Capture()
        {
            UnityEditor.ShaderUtil.allowAsyncCompilation=false;
            PrepareTextures();
            SceneSetup.SetupRenderPipeline();
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
            ProductionVisualIntegration.ResetVisualCachesForGate();
            VisualWorld.VisualIntegrationEnabled=true;
            VisualWorld.Create(true,new PlayerState
            {
                BastionLevel=3,SawmillLevel=1,BarracksLevel=1,CorruptionDiscovered=true
            });

            var camera=Camera.main;
            if(camera==null)throw new Exception("Valoria camera missing.");
            Directory.CreateDirectory(Folder);
            var baselineSignature=ValoriaVisualFormulaGate.CollisionSignature();
            var before=Metrics();

            Save(camera,Folder+"/before-19.png",19f,1280,720);
            Save(camera,Folder+"/before-12.png",12f,1280,720);
            Save(camera,Folder+"/before-9.png",9f,1280,720);
            Save(camera,Folder+"/before-mobile.png",12f,390,844);

            ValoriaProductionModuleCell.Build();
            var after=Metrics();

            if(ValoriaVisualFormulaGate.CollisionSignature()!=baselineSignature)
                throw new Exception("Production Module Kit altered gameplay collider/hotspot signature.");
            if(after.renderers-before.renderers>20)
                throw new Exception("Production Module Kit renderer budget exceeded: +"+(after.renderers-before.renderers));

            Save(camera,Folder+"/after-19.png",19f,1280,720);
            Save(camera,Folder+"/after-12.png",12f,1280,720);
            Save(camera,Folder+"/after-9.png",9f,1280,720);
            Save(camera,Folder+"/after-mobile.png",12f,390,844);

            File.WriteAllText(Folder+"/evidence.json",
                "{\n"+
                "  \"schema_version\": 1,\n"+
                "  \"branch\": \"visual-proof/valoria-production-module-kit-v1\",\n"+
                "  \"same_scene_before_after\": true,\n"+
                "  \"collider_hotspot_signature_equal\": true,\n"+
                "  \"renderer_budget_delta_max\": 20,\n"+
                "  \"before\": "+before.Json()+",\n"+
                "  \"after\": "+after.Json()+",\n"+
                "  \"delta_renderers\": "+(after.renderers-before.renderers)+",\n"+
                "  \"delta_triangles\": "+(after.triangles-before.triangles)+",\n"+
                "  \"tripo_credits\": 0,\n"+
                "  \"paid_assets\": 0\n"+
                "}\n");

            EditorApplication.Exit(0);
        }

        static void PrepareTextures()
        {
            const string folder="Assets/Resources/Valoria/SurfaceCellExternal";
            if(!Directory.Exists(folder))return;
            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
            foreach(var guid in AssetDatabase.FindAssets("t:Texture2D",new[]{folder}))
            {
                var path=AssetDatabase.GUIDToAssetPath(guid);
                var importer=AssetImporter.GetAtPath(path) as TextureImporter;
                if(importer==null)continue;
                string lower=Path.GetFileNameWithoutExtension(path).ToLowerInvariant();
                importer.wrapMode=TextureWrapMode.Repeat;
                importer.filterMode=FilterMode.Trilinear;
                importer.mipmapEnabled=true;
                importer.maxTextureSize=1024;
                if(lower.EndsWith("_normal"))
                {
                    importer.textureType=TextureImporterType.NormalMap;
                    importer.sRGBTexture=false;
                }
                else
                {
                    importer.textureType=TextureImporterType.Default;
                    importer.sRGBTexture=!lower.EndsWith("_ao");
                }
                importer.SaveAndReimport();
            }
            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
        }

        static MetricSnapshot Metrics()
        {
            long tris=0;
            int renderers=0,lights=0;
            var materials=new HashSet<int>();
            foreach(var r in UnityEngine.Object.FindObjectsByType<Renderer>(FindObjectsSortMode.None))
            {
                if(r==null||!r.enabled||!r.gameObject.activeInHierarchy)continue;
                renderers++;
                foreach(var m in r.sharedMaterials)if(m!=null)materials.Add(m.GetInstanceID());
                var mf=r.GetComponent<MeshFilter>();
                if(mf!=null&&mf.sharedMesh!=null)tris+=mf.sharedMesh.triangles.LongLength/3;
                var sk=r as SkinnedMeshRenderer;
                if(sk!=null&&sk.sharedMesh!=null)tris+=sk.sharedMesh.triangles.LongLength/3;
            }
            foreach(var l in UnityEngine.Object.FindObjectsByType<Light>(FindObjectsSortMode.None))
                if(l!=null&&l.enabled&&l.gameObject.activeInHierarchy)lights++;
            return new MetricSnapshot{triangles=tris,renderers=renderers,materials=materials.Count,lights=lights};
        }

        struct MetricSnapshot
        {
            public long triangles;
            public int renderers,materials,lights;
            public string Json()=>"{\"triangles\":"+triangles+",\"renderers\":"+renderers+
                ",\"materials\":"+materials+",\"lights\":"+lights+"}";
        }

        static void Save(Camera camera,string path,float zoom,int width,int height)
        {
            camera.transform.position=CameraPosition;
            camera.transform.LookAt(CameraTarget);
            camera.orthographic=true;
            camera.orthographicSize=zoom;
            var rt=new RenderTexture(width,height,24,RenderTextureFormat.ARGB32);
            var tex=new Texture2D(width,height,TextureFormat.RGB24,false);
            camera.targetTexture=rt;
            camera.Render();
            RenderTexture.active=rt;
            tex.ReadPixels(new Rect(0,0,width,height),0,0);
            tex.Apply();
            File.WriteAllBytes(path,tex.EncodeToPNG());
            camera.targetTexture=null;
            RenderTexture.active=null;
            UnityEngine.Object.DestroyImmediate(tex);
            UnityEngine.Object.DestroyImmediate(rt);
        }
    }

    static class ValoriaProductionModuleCell
    {
        static Transform Root;
        static Material Stone,StoneDark,Cobble,Dirt,Wood,Roof,Plaster,Blue,Metal;

        public static void Build()
        {
            Root=new GameObject("VALORIA · Production Module Kit v1").transform;
            BuildMaterials();
            TuneLighting();
            UnifyBastionSurface();

            BuildTerraceModule();
            BuildProcessionalStairModule();
            BuildGateModules();
            BuildRockWallSeamModules();
            BuildSupportModules();
        }

        static void BuildMaterials()
        {
            Stone=External("stone",new Color(.76f,.72f,.64f),new Vector2(3.1f,3.1f),.06f,1.05f,"stone");
            StoneDark=External("stone",new Color(.48f,.47f,.43f),new Vector2(3.5f,3.5f),.045f,1.10f,"stone");
            Cobble=External("ground",new Color(.82f,.77f,.68f),new Vector2(5.1f,5.1f),.055f,1.0f,"ground");
            Dirt=External("ground",new Color(.53f,.44f,.34f),new Vector2(4.0f,4.0f),.02f,.55f,"ground");
            Wood=External("wood",new Color(.50f,.32f,.18f),new Vector2(3.5f,3.5f),.045f,.9f,"wood");
            Roof=External("roof",new Color(.24f,.30f,.35f),new Vector2(4.4f,4.4f),.09f,.8f,"slate");
            Plaster=External("plaster",new Color(.71f,.63f,.52f),new Vector2(3.0f,3.0f),.035f,.7f,"stone");
            Metal=External("metal",new Color(.29f,.30f,.31f),new Vector2(3.5f,3.5f),.22f,.55f,"stone");
            Blue=ValoriaKit.DetailedSurfaceMaterial(new Color(.055f,.18f,.32f),"slate",new Vector2(2f,2f),.35f);
        }

        static Material External(string prefix,Color tint,Vector2 tiling,float smooth,float bump,string fallback)
        {
            return ValoriaKit.ExternalPbrSurfaceMaterial(prefix,tint,tiling,smooth,bump)
                   ?? ValoriaKit.DetailedSurfaceMaterial(tint,fallback,tiling,bump);
        }

        static void TuneLighting()
        {
            RenderSettings.ambientMode=AmbientMode.Trilight;
            RenderSettings.ambientSkyColor=new Color(.69f,.74f,.79f);
            RenderSettings.ambientEquatorColor=new Color(.48f,.47f,.42f);
            RenderSettings.ambientGroundColor=new Color(.23f,.21f,.18f);
            RenderSettings.ambientIntensity=.96f;
            RenderSettings.fog=true;
            RenderSettings.fogMode=FogMode.Linear;
            RenderSettings.fogColor=new Color(.60f,.65f,.68f);
            RenderSettings.fogStartDistance=43f;
            RenderSettings.fogEndDistance=108f;

            foreach(var l in UnityEngine.Object.FindObjectsByType<Light>(FindObjectsSortMode.None))
            {
                if(l.type!=LightType.Directional)continue;
                l.color=new Color(1f,.90f,.76f);
                l.intensity=.94f;
                l.shadowStrength=.66f;
                l.shadows=LightShadows.Soft;
                l.transform.rotation=Quaternion.Euler(47f,-35f,0f);
            }
        }

        static void UnifyBastionSurface()
        {
            foreach(var r in UnityEngine.Object.FindObjectsByType<Renderer>(FindObjectsSortMode.None))
            {
                bool bastion=false;
                for(var t=r.transform;t!=null;t=t.parent)
                {
                    if(t.name.StartsWith("Bastion ·",StringComparison.Ordinal)||
                       t.name.StartsWith("Valoria · Bastion hero",StringComparison.Ordinal))
                    {bastion=true;break;}
                }
                if(!bastion)continue;
                var n=r.gameObject.name.ToLowerInvariant();
                Material m;
                if(n.Contains("banner")||n.Contains("flag"))m=Blue;
                else if(n.Contains("door")||n.Contains("timber")||n.Contains("portcullis")||n.Contains("gate leaf"))m=Wood;
                else if(n.Contains("metal")||n.Contains("slit"))m=Metal;
                else if(n.Contains("foundation")||n.Contains("plinth")||n.Contains("rubble"))m=StoneDark;
                else m=Stone;
                var mats=new Material[Mathf.Max(1,r.sharedMaterials.Length)];
                for(int i=0;i<mats.Length;i++)mats[i]=m;
                r.sharedMaterials=mats;
            }
        }

        static void BuildTerraceModule()
        {
            var b=new ModuleMeshBuilder("VPMK · terrace",new[]{Stone,StoneDark,Cobble,Dirt});
            b.Box(new Vector3(0f,2.42f,4.72f),new Vector3(12.7f,.38f,5.85f),0);
            for(int i=-5;i<=5;i++)
                b.Box(new Vector3(i*1.08f,1.55f,2.28f),new Vector3(1.0f,1.45f,.62f),i%2==0?0:1);
            b.Box(new Vector3(-5.45f,.57f,-.25f),new Vector3(2.1f,.06f,4.6f),3);
            b.Box(new Vector3(5.45f,.57f,-.25f),new Vector3(2.1f,.06f,4.6f),3);
            b.Commit(Root);
        }

        static void BuildProcessionalStairModule()
        {
            var b=new ModuleMeshBuilder("VPMK · processional stair",new[]{Cobble,Stone});
            for(int i=0;i<9;i++)
            {
                float z=.15f+i*.54f;
                float y=.78f+i*.205f;
                b.Box(new Vector3(0f,y,z),new Vector3(4.18f,.22f,.56f),0);
            }
            b.Box(new Vector3(-2.28f,1.65f,2.30f),new Vector3(.32f,2.10f,4.8f),1);
            b.Box(new Vector3(2.28f,1.65f,2.30f),new Vector3(.32f,2.10f,4.8f),1);
            b.Commit(Root);
        }

        static void BuildGateModules()
        {
            GateWing("VPMK · west gate wing",new Vector3(-5.0f,2.86f,4.30f),false);
            GateWing("VPMK · east gate wing",new Vector3(5.0f,2.86f,4.30f),true);

            var arch=new ModuleMeshBuilder("VPMK · gate arch",new[]{Stone,StoneDark,Wood,Blue});
            arch.Box(new Vector3(-2.55f,3.55f,4.12f),new Vector3(.72f,3.1f,.72f),0);
            arch.Box(new Vector3(2.55f,3.55f,4.12f),new Vector3(.72f,3.1f,.72f),0);
            arch.Box(new Vector3(0f,5.00f,4.12f),new Vector3(5.7f,.62f,.76f),1);
            arch.Box(new Vector3(0f,3.80f,4.08f),new Vector3(2.15f,2.55f,.18f),2);
            arch.Commit(Root);
        }

        static void GateWing(string name,Vector3 p,bool mirror)
        {
            float side=mirror?1f:-1f;
            var b=new ModuleMeshBuilder(name,new[]{Stone,StoneDark,Roof,Wood,Metal,Blue});
            b.Box(p,new Vector3(2.28f,2.35f,3.05f),0);
            b.Box(p+new Vector3(0,-1.24f,0),new Vector3(2.62f,.30f,3.36f),1);
            b.Box(p+new Vector3(side*1.32f,-.28f,-.18f),new Vector3(.62f,2.85f,1.04f),1);
            b.Box(p+new Vector3(0,1.45f,.08f),new Vector3(2.65f,.34f,3.40f),2);
            b.Box(p+new Vector3(-side*.32f,-.35f,-1.62f),new Vector3(.62f,1.12f,.08f),3);
            b.Box(p+new Vector3(-side*.82f,.18f,-1.67f),new Vector3(.14f,1.52f,.14f),4);
            b.Commit(Root);
        }

        static void BuildRockWallSeamModules()
        {
            Seam("VPMK · west seam",-1f);
            Seam("VPMK · east seam",1f);
        }

        static void Seam(string name,float side)
        {
            var b=new ModuleMeshBuilder(name,new[]{Stone,StoneDark,Dirt});
            for(int i=0;i<4;i++)
            {
                float x=side*(6.15f+i*.76f);
                float y=1.50f-i*.20f;
                float z=3.72f+i*.74f;
                b.Box(new Vector3(x,y,z),new Vector3(1.16f,2.46f-i*.24f,2.68f),i%2==0?0:1);
            }
            b.Box(new Vector3(side*9.0f,.62f,6.2f),new Vector3(2.7f,1.3f,3.0f),1);
            b.Box(new Vector3(side*10.2f,.38f,7.0f),new Vector3(2.0f,.55f,2.2f),2);
            b.Commit(Root);
        }

        static void BuildSupportModules()
        {
            Support("VPMK · residence",new Vector3(-7.55f,.58f,-1.02f),false);
            Support("VPMK · workshop",new Vector3(7.55f,.58f,-1.12f),true);
        }

        static void Support(string name,Vector3 p,bool workshop)
        {
            var b=new ModuleMeshBuilder(name,new[]{Plaster,Wood,Roof,StoneDark});
            b.Box(p+new Vector3(0,1.0f,0),new Vector3(workshop?3.15f:2.85f,2.0f,workshop?2.92f:2.72f),0);
            b.Box(p+new Vector3(0,2.18f,0),new Vector3(workshop?3.50f:3.20f,.38f,workshop?3.25f:3.05f),2);
            for(int i=-1;i<=1;i++)
                b.Box(p+new Vector3(i*.82f,1.05f,-1.48f),new Vector3(.12f,1.72f,.10f),1);
            b.Box(p+new Vector3(0,.62f,-1.49f),new Vector3(.58f,1.18f,.08f),1);
            if(!workshop)b.Box(p+new Vector3(.72f,2.65f,.15f),new Vector3(.34f,.95f,.34f),3);
            b.Commit(Root);
        }
    }

    /// <summary>Builds many boxes into one mesh renderer with material submeshes.</summary>
    sealed class ModuleMeshBuilder
    {
        readonly string Name;
        readonly Material[] Materials;
        readonly List<Vector3> Vertices=new List<Vector3>();
        readonly List<Vector2> Uvs=new List<Vector2>();
        readonly List<int>[] Indices;

        public ModuleMeshBuilder(string name,Material[] materials)
        {
            Name=name;Materials=materials;
            Indices=new List<int>[materials.Length];
            for(int i=0;i<Indices.Length;i++)Indices[i]=new List<int>();
        }

        public void Box(Vector3 center,Vector3 size,int material)
        {
            material=Mathf.Clamp(material,0,Materials.Length-1);
            Vector3 h=size*.5f;
            // six independent faces keep normals/UVs correct while remaining one renderer.
            Face(center,new Vector3(-h.x,-h.y,-h.z),new Vector3(h.x,-h.y,-h.z),new Vector3(h.x,h.y,-h.z),new Vector3(-h.x,h.y,-h.z),Vector3.back,material,size.x,size.y);
            Face(center,new Vector3(h.x,-h.y,h.z),new Vector3(-h.x,-h.y,h.z),new Vector3(-h.x,h.y,h.z),new Vector3(h.x,h.y,h.z),Vector3.forward,material,size.x,size.y);
            Face(center,new Vector3(-h.x,-h.y,h.z),new Vector3(-h.x,-h.y,-h.z),new Vector3(-h.x,h.y,-h.z),new Vector3(-h.x,h.y,h.z),Vector3.left,material,size.z,size.y);
            Face(center,new Vector3(h.x,-h.y,-h.z),new Vector3(h.x,-h.y,h.z),new Vector3(h.x,h.y,h.z),new Vector3(h.x,h.y,-h.z),Vector3.right,material,size.z,size.y);
            Face(center,new Vector3(-h.x,h.y,-h.z),new Vector3(h.x,h.y,-h.z),new Vector3(h.x,h.y,h.z),new Vector3(-h.x,h.y,h.z),Vector3.up,material,size.x,size.z);
            Face(center,new Vector3(-h.x,-h.y,h.z),new Vector3(h.x,-h.y,h.z),new Vector3(h.x,-h.y,-h.z),new Vector3(-h.x,-h.y,-h.z),Vector3.down,material,size.x,size.z);
        }

        void Face(Vector3 c,Vector3 a,Vector3 b,Vector3 d,Vector3 e,Vector3 normal,int material,float u,float v)
        {
            int start=Vertices.Count;
            Vertices.Add(c+a);Vertices.Add(c+b);Vertices.Add(c+d);Vertices.Add(c+e);
            Uvs.Add(new Vector2(0,0));Uvs.Add(new Vector2(u,0));Uvs.Add(new Vector2(u,v));Uvs.Add(new Vector2(0,v));
            Indices[material].Add(start);Indices[material].Add(start+1);Indices[material].Add(start+2);
            Indices[material].Add(start);Indices[material].Add(start+2);Indices[material].Add(start+3);
        }

        public GameObject Commit(Transform parent)
        {
            var mesh=new Mesh{name=Name+" mesh",indexFormat=UnityEngine.Rendering.IndexFormat.UInt32};
            mesh.SetVertices(Vertices);
            mesh.SetUVs(0,Uvs);
            mesh.subMeshCount=Indices.Length;
            for(int i=0;i<Indices.Length;i++)mesh.SetTriangles(Indices[i],i,true);
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();

            var go=new GameObject(Name);
            go.transform.SetParent(parent,true);
            go.AddComponent<MeshFilter>().sharedMesh=mesh;
            go.AddComponent<MeshRenderer>().sharedMaterials=Materials;
            return go;
        }
    }
}
