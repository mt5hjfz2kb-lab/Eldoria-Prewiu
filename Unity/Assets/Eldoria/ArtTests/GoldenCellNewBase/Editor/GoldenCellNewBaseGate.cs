using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using GLTFast;
using Eldoria.Domain;
using Eldoria.Presentation;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;

namespace Eldoria.EditorTools
{
    public static class GoldenCellNewBaseGate
    {
        const string Folder="GoldenCellCaptures";
        static readonly Vector3 CameraPosition=new Vector3(18.2f,14.6f,-25.8f);
        static readonly Vector3 CameraTarget=new Vector3(0f,3.65f,7.25f);

        public static void CaptureBlockout()
        {
            UnityEditor.ShaderUtil.allowAsyncCompilation=false;
            SceneSetup.SetupRenderPipeline();
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
            ProductionVisualIntegration.ResetVisualCachesForGate();
            VisualWorld.VisualIntegrationEnabled=true;
            VisualWorld.Create(true,new PlayerState{BastionLevel=3,SawmillLevel=1,BarracksLevel=1,CorruptionDiscovered=true});
            var camera=Camera.main;
            if(camera==null)throw new Exception("Valoria camera missing.");
            Directory.CreateDirectory(Folder);
            var baseline=ValoriaVisualFormulaGate.CollisionSignature();

            Save(camera,Folder+"/before-19.png",19f,1280,720);
            Save(camera,Folder+"/before-12.png",12f,1280,720);
            Save(camera,Folder+"/before-9.png",9f,1280,720);
            Save(camera,Folder+"/before-mobile.png",12f,390,844);

            GoldenCellBlockout.Build();

            if(ValoriaVisualFormulaGate.CollisionSignature()!=baseline)
                throw new Exception("Golden Cell blockout altered gameplay collider/hotspot signature.");

            Save(camera,Folder+"/blockout-19.png",19f,1280,720);
            Save(camera,Folder+"/blockout-12.png",12f,1280,720);
            Save(camera,Folder+"/blockout-9.png",9f,1280,720);
            Save(camera,Folder+"/blockout-mobile.png",12f,390,844);

            File.WriteAllText(Folder+"/blockout-evidence.json",
                "{\n"+
                "  \"phase\": \"BLOCKOUT_ONLY\",\n"+
                "  \"branch\": \"visual-proof/golden-cell-new-base-v1\",\n"+
                "  \"cell\": \"Bastion main access / upper civic terrace\",\n"+
                "  \"same_scene_before_after\": true,\n"+
                "  \"collider_hotspot_signature_equal\": true,\n"+
                "  \"tripo_credits\": 0,\n"+
                "  \"final_assets_fabricated\": false,\n"+
                "  \"scope\": [\"gate massing\",\"plaza\",\"terrace\",\"rock-architecture seam\",\"two support buildings\"]\n"+
                "}\n");
            EditorApplication.Exit(0);
        }

        public static async void CaptureFinal()
        {
            UnityEditor.ShaderUtil.allowAsyncCompilation=false;
            PrepareGoldenCellTextures();
            SceneSetup.SetupRenderPipeline();
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
            ProductionVisualIntegration.ResetVisualCachesForGate();
            VisualWorld.VisualIntegrationEnabled=true;
            VisualWorld.Create(true,new PlayerState{BastionLevel=3,SawmillLevel=1,BarracksLevel=1,CorruptionDiscovered=true});
            var camera=Camera.main;
            if(camera==null)throw new Exception("Valoria camera missing.");
            Directory.CreateDirectory(Folder);
            var baseline=ValoriaVisualFormulaGate.CollisionSignature();

            Save(camera,Folder+"/before-19.png",19f,1280,720);
            Save(camera,Folder+"/before-12.png",12f,1280,720);
            Save(camera,Folder+"/before-9.png",9f,1280,720);
            Save(camera,Folder+"/before-mobile.png",12f,390,844);

            await GoldenCellFinished.BuildAsync();

            if(ValoriaVisualFormulaGate.CollisionSignature()!=baseline)
                throw new Exception("Golden Cell finished art altered gameplay collider/hotspot signature.");

            Save(camera,Folder+"/after-19.png",19f,1280,720);
            Save(camera,Folder+"/after-12.png",12f,1280,720);
            Save(camera,Folder+"/after-9.png",9f,1280,720);
            Save(camera,Folder+"/after-mobile.png",12f,390,844);
            File.WriteAllText(Folder+"/final-evidence.json",
                "{\n"+
                "  \"phase\": \"FINISHED_GOLDEN_CELL\",\n"+
                "  \"branch\": \"visual-proof/golden-cell-new-base-v1\",\n"+
                "  \"same_scene_before_after\": true,\n"+
                "  \"collider_hotspot_signature_equal\": true,\n"+
                "  \"tripo_credits\": 0,\n"+
                "  \"paid_assets\": 0\n"+
                "}\n");
            EditorApplication.Exit(0);
        }

        static void PrepareGoldenCellTextures()
        {
            foreach(var folder in new[]{
                "Assets/Resources/Valoria/GoldenCellExternal",
                "Assets/Resources/Valoria/GoldenCellPBR"})
            {
                if(!Directory.Exists(folder))continue;
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
            }
            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
        }

        static void Save(Camera camera,string path,float zoom,int width,int height)
        {
            camera.transform.position=CameraPosition;
            camera.transform.LookAt(CameraTarget);
            camera.orthographic=true;
            camera.orthographicSize=zoom;
            var rt=new RenderTexture(width,height,24,RenderTextureFormat.ARGB32);
            var tex=new Texture2D(width,height,TextureFormat.RGB24,false);
            camera.targetTexture=rt;camera.Render();RenderTexture.active=rt;
            tex.ReadPixels(new Rect(0,0,width,height),0,0);tex.Apply();
            File.WriteAllBytes(path,tex.EncodeToPNG());
            camera.targetTexture=null;RenderTexture.active=null;
            UnityEngine.Object.DestroyImmediate(tex);UnityEngine.Object.DestroyImmediate(rt);
        }
    }

    static class GoldenCellBlockout
    {
        static Transform Root;
        static Material Stone,Ground,Wood,Rock,Roof,Blue;

        public static void Build()
        {
            Root=new GameObject("GOLDEN CELL · blockout v2").transform;
            Stone=Mat(new Color(.48f,.45f,.40f));Ground=Mat(new Color(.34f,.30f,.25f));
            Wood=Mat(new Color(.27f,.18f,.11f));Rock=Mat(new Color(.26f,.27f,.26f));
            Roof=Mat(new Color(.12f,.16f,.20f));Blue=Mat(new Color(.07f,.18f,.33f));

            // New cell sits around/below the existing Bastion instead of covering its silhouette.
            Block("GC2 · upper civic shelf",new Vector3(0f,2.47f,4.65f),new Vector3(12.6f,.36f,5.8f),Stone);
            Block("GC2 · lower plaza",new Vector3(0f,.62f,-.55f),new Vector3(8.8f,.16f,5.1f),Ground);

            // Processional stair — broad, straight and obvious from mobile camera.
            for(int i=0;i<9;i++)
            {
                float z=.15f+i*.54f;
                float y=.78f+i*.205f;
                Block("GC2 · stair "+i,new Vector3(0f,y,z),new Vector3(4.15f,.22f,.56f),Ground);
            }

            // Monumental framing wings are lower than Bastion: they support hierarchy instead of replacing it.
            Wing("GC2 · west gate wing",new Vector3(-5.05f,2.95f,4.25f),false);
            Wing("GC2 · east gate wing",new Vector3(5.05f,2.95f,4.25f),true);

            // Rock / architecture seam: stone retaining wall steps into angled mountain shoulders.
            for(int i=0;i<3;i++)
            {
                Block("GC2 · west retaining "+i,new Vector3(-6.15f-i*.72f,1.55f-i*.22f,3.7f+i*.72f),
                    new Vector3(1.15f,2.5f-i*.25f,2.7f),Stone).transform.rotation=Quaternion.Euler(0,12f+i*6f,0);
                Block("GC2 · east retaining "+i,new Vector3(6.15f+i*.72f,1.55f-i*.22f,3.7f+i*.72f),
                    new Vector3(1.15f,2.5f-i*.25f,2.7f),Stone).transform.rotation=Quaternion.Euler(0,-12f-i*6f,0);
            }
            Wedge("GC2 · west mountain shoulder",new Vector3(-8.55f,.72f,6.0f),new Vector3(5.2f,3.1f,6.5f),-9f,Rock);
            Wedge("GC2 · east mountain shoulder",new Vector3(8.55f,.72f,6.0f),new Vector3(5.2f,3.1f,6.5f),9f,Rock);

            // Small inhabited/productive anchors establish human scale without filling the cell.
            House("GC2 · residence",new Vector3(-7.7f,.55f,-1.05f),new Vector3(2.9f,2.15f,2.8f),-8f);
            House("GC2 · workshop",new Vector3(7.7f,.55f,-1.2f),new Vector3(3.25f,2.05f,3.0f),8f);

            // Low arcades create civic rhythm along the plaza edges.
            Arcade("GC2 · west arcade",new Vector3(-4.6f,.72f,-.55f),-3f);
            Arcade("GC2 · east arcade",new Vector3(4.6f,.72f,-.55f),3f);

            // Heraldic identity stays subordinate.
            Banner(new Vector3(-4.05f,4.65f,3.05f));Banner(new Vector3(4.05f,4.65f,3.05f));
        }

        static void Wing(string name,Vector3 p,bool mirror)
        {
            float s=mirror?-1f:1f;
            Block(name+" · lower mass",p,new Vector3(2.35f,2.4f,3.1f),Stone);
            Block(name+" · buttress",p+new Vector3(s*1.35f,-.35f,-.25f),new Vector3(.65f,2.7f,1.15f),Stone);
            var roof=Block(name+" · roof",p+new Vector3(0,1.55f,.10f),new Vector3(2.7f,.42f,3.45f),Roof);
            roof.transform.rotation=Quaternion.Euler(0,0,s*5f);
            Block(name+" · dark opening",p+new Vector3(-s*.45f,-.25f,-1.58f),new Vector3(.72f,1.15f,.08f),Wood);
        }

        static void Arcade(string name,Vector3 p,float yaw)
        {
            var root=new GameObject(name);root.transform.SetParent(Root);root.transform.position=p;root.transform.rotation=Quaternion.Euler(0,yaw,0);
            for(int i=-1;i<=1;i++)
            {
                Block(name+" · pier "+i,p+new Vector3(i*1.25f,.65f,0),new Vector3(.28f,1.3f,.45f),Stone);
                Block(name+" · beam "+i,p+new Vector3(i*1.25f,1.35f,0),new Vector3(1.35f,.28f,.48f),Stone);
            }
        }

        static void House(string name,Vector3 p,Vector3 size,float yaw)
        {
            var root=new GameObject(name);root.transform.SetParent(Root);root.transform.position=p;root.transform.rotation=Quaternion.Euler(0,yaw,0);
            var body=Block(name+" · body",p+Vector3.up*(size.y*.5f),size,Stone);body.transform.rotation=root.transform.rotation;
            var roof=Block(name+" · roof",p+new Vector3(0,size.y+.43f,0),new Vector3(size.x*1.12f,.48f,size.z*1.10f),Roof);
            roof.transform.rotation=root.transform.rotation*Quaternion.Euler(0,0,4f);
            var door=Block(name+" · door",p+root.transform.rotation*new Vector3(0,.62f,-size.z*.51f),new Vector3(.48f,1.1f,.10f),Wood);
            door.transform.rotation=root.transform.rotation;
        }

        static void Banner(Vector3 p){ Block("GC2 · blue banner",p,new Vector3(.52f,1.35f,.05f),Blue); }

        static GameObject Wedge(string name,Vector3 p,Vector3 scale,float zRot,Material m)
        { var go=Block(name,p,scale,m);go.transform.rotation=Quaternion.Euler(0,0,zRot);return go; }

        static GameObject Block(string name,Vector3 p,Vector3 scale,Material m)
        {
            var go=GameObject.CreatePrimitive(PrimitiveType.Cube);go.name=name;go.transform.SetParent(Root);
            go.transform.position=p;go.transform.localScale=scale;
            var col=go.GetComponent<Collider>();if(col!=null)UnityEngine.Object.DestroyImmediate(col);
            go.GetComponent<Renderer>().sharedMaterial=m;return go;
        }

        static Material Mat(Color color)
        {
            var shader=Shader.Find("Universal Render Pipeline/Lit")??Shader.Find("Standard");
            var m=new Material(shader);if(m.HasProperty("_BaseColor"))m.SetColor("_BaseColor",color);
            if(m.HasProperty("_Color"))m.SetColor("_Color",color);if(m.HasProperty("_Smoothness"))m.SetFloat("_Smoothness",.025f);
            return m;
        }
    }

    static class GoldenCellFinished
    {
        static Transform Root;
        static Material Stone,StoneDark,BastionStone,Cobble,Dirt,Wood,Roof,Metal,Moss,Plaster,Blue,Gold,Corrupt;

        public static void Build()
        {
            Root=new GameObject("GOLDEN CELL · finished v2 hero replacement proof").transform;
            HideOldBastionVisuals();
            SetupMaterials();
            SetupLighting();
            UnifyExistingBastionSurface();

            // Replace only the cell's visual read. Gameplay/colliders remain untouched.
            BuildTerraceAndPlaza();
            BuildProcessionalAccess();
            await BuildGateWingsAsync();
            BuildRockArchitectureTransition();
            BuildResidence();
            BuildWorkshop();
            BuildHeraldry();
            BuildVegetationAndLife();
            BuildCorruptionHint();
        }

        static void HideOldBastionVisuals()
        {
            // Final strict proof: retain all gameplay colliders/hotspots, hide only the previous
            // Bastion renderer family so it cannot set the visual quality ceiling.
            foreach(var r in UnityEngine.Object.FindObjectsByType<Renderer>(FindObjectsSortMode.None))
            {
                if(r==null||!r.enabled)continue;
                var n=r.gameObject.name;
                if(n.StartsWith("Bastion",StringComparison.OrdinalIgnoreCase))
                    r.enabled=false;
            }
        }

        static void SetupMaterials()
        {
            Stone=Pbr("stone",new Color(.94f,.90f,.82f,1f),new Vector2(3.0f,3.0f),.09f,1.05f);
            StoneDark=Pbr("stone",new Color(.60f,.60f,.56f,1f),new Vector2(3.5f,3.5f),.07f,1.10f);
            BastionStone=Pbr("stone",new Color(.63f,.61f,.56f,1f),new Vector2(3.25f,3.25f),.055f,1.08f);
            Cobble=Pbr("ground",new Color(.86f,.80f,.69f,1f),new Vector2(5.4f,5.4f),.08f,1.15f);
            Dirt=Pbr("ground",new Color(.58f,.48f,.36f,1f),new Vector2(4.4f,4.4f),.025f,.55f);
            Wood=Pbr("wood",new Color(.62f,.40f,.22f,1f),new Vector2(3.8f,3.8f),.06f,1.0f);
            Roof=Pbr("roof",new Color(.30f,.37f,.43f,1f),new Vector2(4.8f,4.8f),.14f,1.0f);
            Metal=Pbr("metal",new Color(.34f,.35f,.36f,1f),new Vector2(4.0f,4.0f),.34f,.65f);
            Moss=Pbr("mossrock",new Color(.52f,.69f,.43f,1f),new Vector2(3.6f,3.6f),.035f,.95f);
            Plaster=Pbr("plaster",new Color(.86f,.78f,.65f,1f),new Vector2(3.0f,3.0f),.055f,.7f);
            Blue=Simple(new Color(.045f,.19f,.36f,1f),.18f,.05f);
            Gold=Simple(new Color(.68f,.45f,.12f,1f),.34f,.62f);
            Corrupt=Simple(new Color(.28f,.055f,.37f,1f),.22f,.05f);
        }

        static void UnifyExistingBastionSurface()
        {
            // Final Golden Cell pass: keep the certified Bastion silhouette and interaction untouched,
            // but bring its visible shell into the same material family as the new civic base.
            foreach(var r in UnityEngine.Object.FindObjectsByType<Renderer>(FindObjectsSortMode.None))
            {
                bool bastion=false;
                for(var t=r.transform;t!=null;t=t.parent)
                {
                    if(t.name.StartsWith("Bastion ·",StringComparison.Ordinal)||
                       t.name.StartsWith("Valoria · Bastion hero",StringComparison.Ordinal))
                    { bastion=true; break; }
                }
                if(!bastion)continue;

                string n=r.gameObject.name.ToLowerInvariant();
                Material material;
                if(n.Contains("banner")||n.Contains("flag"))material=Blue;
                else if(n.Contains("door")||n.Contains("timber")||n.Contains("gate leaf")||n.Contains("portcullis"))material=Wood;
                else if(n.Contains("slit")||n.Contains("metal"))material=Metal;
                else if(n.Contains("plinth")||n.Contains("backing")||n.Contains("rubble")||n.Contains("foundation"))
                    material=StoneDark;
                else material=BastionStone;

                int count=Mathf.Max(1,r.sharedMaterials.Length);
                var mats=new Material[count];
                for(int i=0;i<count;i++)mats[i]=material;
                r.sharedMaterials=mats;
            }
        }

        static void SetupLighting()
        {
            RenderSettings.ambientMode=AmbientMode.Trilight;
            RenderSettings.ambientSkyColor=new Color(.70f,.76f,.81f);
            RenderSettings.ambientEquatorColor=new Color(.52f,.50f,.43f);
            RenderSettings.ambientGroundColor=new Color(.24f,.22f,.18f);
            RenderSettings.ambientIntensity=1.02f;
            RenderSettings.fog=true;
            RenderSettings.fogMode=FogMode.Linear;
            RenderSettings.fogColor=new Color(.61f,.68f,.72f);
            RenderSettings.fogStartDistance=40f;
            RenderSettings.fogEndDistance=105f;

            foreach(var l in UnityEngine.Object.FindObjectsByType<Light>(FindObjectsSortMode.None))
            {
                if(l.type!=LightType.Directional)continue;
                l.intensity=.92f;
                l.color=new Color(1f,.91f,.78f);
                l.shadowStrength=.62f;
                l.shadows=LightShadows.Soft;
                l.transform.rotation=Quaternion.Euler(47f,-35f,0f);
            }

            WarmLight("GC · gate fire",new Vector3(0f,3.2f,4.7f),6.0f,1.55f);
            WarmLight("GC · residence hearth",new Vector3(-7.4f,2.2f,-.9f),4.5f,1.9f);
            WarmLight("GC · workshop hearth",new Vector3(7.5f,2.1f,-1.0f),4.7f,2.0f);
        }

        static void BuildTerraceAndPlaza()
        {
            BeveledBlock("GC · upper terrace",new Vector3(0f,2.42f,4.72f),new Vector3(12.7f,.38f,5.85f),Stone,.10f);
            BeveledBlock("GC · lower plaza",new Vector3(0f,.63f,-.55f),new Vector3(8.9f,.14f,5.15f),Cobble,.06f);

            // Retaining courses give the terrace real construction logic.
            for(int i=-5;i<=5;i++)
            {
                float x=i*1.08f;
                BeveledBlock("GC · terrace face "+i,new Vector3(x,1.55f,2.28f),
                    new Vector3(1.0f,1.45f,.62f),i%2==0?Stone:StoneDark,.055f);
            }

            // Dirt/moss margins break hard board edges.
            BeveledBlock("GC · west verge",new Vector3(-5.45f,.57f,-.25f),new Vector3(2.1f,.06f,4.6f),Dirt,.02f);
            BeveledBlock("GC · east verge",new Vector3(5.45f,.57f,-.25f),new Vector3(2.1f,.06f,4.6f),Dirt,.02f);
        }

        static void BuildProcessionalAccess()
        {
            for(int i=0;i<9;i++)
            {
                float z=.15f+i*.54f;
                float y=.78f+i*.205f;
                var step=BeveledBlock("GC · processional step "+i,new Vector3(0f,y,z),
                    new Vector3(4.18f,.22f,.56f),Cobble,.05f);
                if(i==0||i==4||i==8)
                {
                    BeveledBlock("GC · stair edge west "+i,new Vector3(-2.25f,y+.16f,z),
                        new Vector3(.25f,.55f,.62f),Stone,.04f);
                    BeveledBlock("GC · stair edge east "+i,new Vector3(2.25f,y+.16f,z),
                        new Vector3(.25f,.55f,.62f),Stone,.04f);
                }
            }
        }

        static async Task BuildGateWingsAsync()
        {
            bool heroLoaded=await TryLoadHeroFortAsync();
            if(!heroLoaded)
            {
                GateWing("GC · west wing",new Vector3(-5.0f,2.86f,4.30f),false);
                GateWing("GC · east wing",new Vector3(5.0f,2.86f,4.30f),true);
                BuildArch("GC · civic arch",new Vector3(0f,3.15f,4.12f),2.55f,2.55f,.52f,Stone);
            }
            BeveledBlock("GC · gate threshold",new Vector3(0f,2.63f,4.30f),new Vector3(5.0f,.14f,1.20f),Cobble,.04f);
        }

        static async Task<bool> TryLoadHeroFortAsync()
        {
            string folder=Path.Combine(Application.dataPath,"Resources","Valoria","GoldenCellHero");
            if(!Directory.Exists(folder))return false;
            var files=Directory.GetFiles(folder,"*.gltf",SearchOption.TopDirectoryOnly);
            if(files.Length==0)return false;

            string path=files[0];
            var holder=new GameObject("GC · CC0 hero fort access · glTFast");
            holder.transform.SetParent(Root,true);
            holder.transform.rotation=Quaternion.Euler(0f,180f,0f);

            var gltf=new GltfImport();
            var settings=new ImportSettings
            {
                GenerateMipMaps=true,
                AnisotropicFilterLevel=3,
                NodeNameMethod=NameImportMethod.OriginalUnique
            };
            bool loaded=await gltf.Load(new Uri(path).AbsoluteUri,settings);
            if(!loaded)
            {
                UnityEngine.Object.DestroyImmediate(holder);
                return false;
            }
            bool instantiated=await gltf.InstantiateMainSceneAsync(holder.transform);
            if(!instantiated)
            {
                UnityEngine.Object.DestroyImmediate(holder);
                return false;
            }

            foreach(var col in holder.GetComponentsInChildren<Collider>(true))col.enabled=false;
            foreach(var hotspot in holder.GetComponentsInChildren<WorldHotspot>(true))
                UnityEngine.Object.DestroyImmediate(hotspot);

            var renderers=holder.GetComponentsInChildren<Renderer>(true);
            if(renderers.Length==0)
            {
                UnityEngine.Object.DestroyImmediate(holder);
                return false;
            }

            Bounds b=renderers[0].bounds;
            for(int i=1;i<renderers.Length;i++)b.Encapsulate(renderers[i].bounds);
            float sx=10.6f/Mathf.Max(.01f,b.size.x);
            float sy=5.6f/Mathf.Max(.01f,b.size.y);
            float sz=5.9f/Mathf.Max(.01f,b.size.z);
            float scale=Mathf.Min(sx,sy,sz);
            holder.transform.localScale*=scale;

            renderers=holder.GetComponentsInChildren<Renderer>(true);
            b=renderers[0].bounds;
            for(int i=1;i<renderers.Length;i++)b.Encapsulate(renderers[i].bounds);
            var desiredCenter=new Vector3(0f,b.center.y,4.55f);
            holder.transform.position+=new Vector3(desiredCenter.x-b.center.x,2.50f-b.min.y,desiredCenter.z-b.center.z);

            foreach(var r in renderers)
            {
                var mats=r.sharedMaterials;
                for(int i=0;i<mats.Length;i++)
                {
                    if(mats[i]==null)continue;
                    var copy=new Material(mats[i]){name="GC hero · "+mats[i].name};
                    if(copy.HasProperty("_BaseColor"))
                    {
                        var bc=copy.GetColor("_BaseColor");
                        copy.SetColor("_BaseColor",new Color(bc.r*.92f,bc.g*.90f,bc.b*.84f,bc.a));
                    }
                    if(copy.HasProperty("_Smoothness"))copy.SetFloat("_Smoothness",Mathf.Min(.14f,copy.GetFloat("_Smoothness")));
                    mats[i]=copy;
                }
                r.sharedMaterials=mats;
            }
            return true;
        }

        static void GateWing(string name,Vector3 p,bool mirror)
        {
            float side=mirror?1f:-1f;
            BeveledBlock(name+" · body",p,new Vector3(2.28f,2.35f,3.05f),Stone,.13f);
            BeveledBlock(name+" · lower plinth",p+new Vector3(0,-1.24f,0),new Vector3(2.62f,.30f,3.36f),StoneDark,.07f);
            BeveledBlock(name+" · buttress",p+new Vector3(side*1.32f,-.28f,-.18f),new Vector3(.62f,2.85f,1.04f),StoneDark,.08f);

            // Slate cap with layered eaves.
            var roof=BeveledBlock(name+" · roof",p+new Vector3(0,1.45f,.08f),new Vector3(2.65f,.34f,3.40f),Roof,.08f);
            roof.transform.rotation=Quaternion.Euler(0,0,side*5f);
            BeveledBlock(name+" · eave",p+new Vector3(0,1.25f,-1.58f),new Vector3(2.75f,.16f,.22f),Wood,.03f);

            // Deep opening + timber frame.
            BeveledBlock(name+" · recess",p+new Vector3(-side*.32f,-.30f,-1.54f),new Vector3(.86f,1.28f,.12f),StoneDark,.03f);
            BeveledBlock(name+" · door",p+new Vector3(-side*.32f,-.35f,-1.62f),new Vector3(.62f,1.12f,.08f),Wood,.02f);
            BeveledBlock(name+" · lintel",p+new Vector3(-side*.32f,.33f,-1.64f),new Vector3(.94f,.16f,.10f),Wood,.02f);

            // Gold-capped corner post.
            BeveledBlock(name+" · heraldic post",p+new Vector3(-side*.82f,.18f,-1.67f),new Vector3(.14f,1.52f,.14f),Metal,.02f);
            BeveledBlock(name+" · gold cap",p+new Vector3(-side*.82f,.98f,-1.67f),new Vector3(.22f,.18f,.22f),Gold,.02f);
        }

        static void BuildRockArchitectureTransition()
        {
            for(int i=0;i<4;i++)
            {
                float t=i/3f;
                float wx=-6.15f-i*.76f, ex=6.15f+i*.76f;
                float y=1.50f-i*.20f,z=3.72f+i*.74f;
                var wm=BeveledBlock("GC · west retaining "+i,new Vector3(wx,y,z),
                    new Vector3(1.16f,2.46f-i*.24f,2.68f),Stone,.11f);
                wm.transform.rotation=Quaternion.Euler(0,12f+i*6f,0);
                var em=BeveledBlock("GC · east retaining "+i,new Vector3(ex,y,z),
                    new Vector3(1.16f,2.46f-i*.24f,2.68f),Stone,.11f);
                em.transform.rotation=Quaternion.Euler(0,-12f-i*6f,0);
                MossPatch(new Vector3(wx+.20f,y+1.25f,z-.42f),.55f,.24f);
                MossPatch(new Vector3(ex-.20f,y+1.25f,z-.42f),.55f,.24f);
            }

            // Procedural rock clusters merge the last masonry course into mountain geology.
            RockCluster("GC · west rock seam",new Vector3(-9.0f,.62f,6.2f),false);
            RockCluster("GC · east rock seam",new Vector3(9.0f,.62f,6.2f),true);
        }

        static void BuildResidence()
        {
            var p=new Vector3(-7.55f,.58f,-1.02f);
            BeveledBlock("GC · residence masonry",p+new Vector3(0,1.0f,0),new Vector3(2.85f,2.05f,2.72f),Plaster,.09f);
            TimberFrame("GC · residence",p,new Vector3(2.85f,2.05f,2.72f));
            GableRoof("GC · residence roof",p+new Vector3(0,2.20f,0),3.25f,3.12f,.95f,-8f);
            BeveledBlock("GC · residence chimney",p+new Vector3(.72f,2.78f,.15f),new Vector3(.34f,1.18f,.34f),StoneDark,.04f);
        }

        static void BuildWorkshop()
        {
            var p=new Vector3(7.55f,.58f,-1.12f);
            BeveledBlock("GC · workshop masonry",p+new Vector3(0,.92f,0),new Vector3(3.15f,1.92f,2.92f),Plaster,.09f);
            TimberFrame("GC · workshop",p,new Vector3(3.15f,1.92f,2.92f));
            GableRoof("GC · workshop roof",p+new Vector3(0,2.05f,0),3.58f,3.30f,.90f,8f);

            // Productive read.
            for(int i=0;i<4;i++)
                Log("GC · stacked timber "+i,p+new Vector3(2.05f,.28f+i*.22f,-.20f+(i%2)*.12f),.92f,.18f,8f+i*3f);
            BeveledBlock("GC · work canopy",p+new Vector3(2.05f,1.35f,.15f),new Vector3(1.55f,.14f,2.30f),Roof,.03f);
            BeveledBlock("GC · canopy post 1",p+new Vector3(1.42f,.72f,-.72f),new Vector3(.14f,1.45f,.14f),Wood,.02f);
            BeveledBlock("GC · canopy post 2",p+new Vector3(2.68f,.72f,-.72f),new Vector3(.14f,1.45f,.14f),Wood,.02f);
        }

        static void BuildHeraldry()
        {
            Banner(new Vector3(-4.05f,4.55f,3.02f),false);
            Banner(new Vector3(4.05f,4.55f,3.02f),true);
            BeveledBlock("GC · central gold medallion",new Vector3(0f,5.0f,3.82f),new Vector3(.52f,.52f,.12f),Gold,.05f);
        }

        static void BuildVegetationAndLife()
        {
            // Purposeful greenery: only at seams and domestic edges.
            Shrub(new Vector3(-8.4f,.66f,-2.2f),.62f);
            Shrub(new Vector3(-6.7f,.66f,-2.0f),.48f);
            Shrub(new Vector3(8.45f,.66f,-2.15f),.58f);
            Shrub(new Vector3(6.85f,.66f,-2.0f),.45f);

            for(int i=0;i<5;i++)
            {
                var p=new Vector3(-5.8f+i*2.9f,.74f,-2.55f+(i%2)*.30f);
                BeveledBlock("GC · bollard "+i,p,new Vector3(.22f,.78f,.22f),StoneDark,.04f);
                if(i%2==0)WarmLight("GC · plaza lantern "+i,p+new Vector3(0,.65f,0),2.8f,.8f);
            }

            // Human-scale silhouettes.
            Person("GC · guard west",new Vector3(-1.45f,2.72f,3.25f));
            Person("GC · guard east",new Vector3(1.45f,2.72f,3.25f));
            Person("GC · resident",new Vector3(-5.8f,.72f,-1.15f));
            Person("GC · worker",new Vector3(5.9f,.72f,-1.3f));
        }

        static void BuildCorruptionHint()
        {
            // Secondary environmental threat at the edge of the cell, not a neon prop.
            for(int i=0;i<5;i++)
            {
                float x=10.8f+i*.42f,z=7.8f+i*.35f;
                var scar=BeveledBlock("GC · corruption scar "+i,new Vector3(x,.33f,z),
                    new Vector3(.52f,.08f,1.05f),Corrupt,.015f);
                scar.transform.rotation=Quaternion.Euler(0,-24f+i*9f,0);
            }
            var glow=new GameObject("GC · distant corruption glow").AddComponent<Light>();
            glow.transform.SetParent(Root);glow.type=LightType.Point;glow.range=6.8f;glow.intensity=1.15f;
            glow.color=new Color(.52f,.16f,.68f);glow.transform.position=new Vector3(11.6f,1.0f,8.8f);
        }

        static void BuildArch(string name,Vector3 center,float radius,float rise,float depth,Material material)
        {
            // Two piers.
            BeveledBlock(name+" · west pier",center+new Vector3(-radius,0,0),new Vector3(.66f,2.8f,depth),material,.08f);
            BeveledBlock(name+" · east pier",center+new Vector3(radius,0,0),new Vector3(.66f,2.8f,depth),material,.08f);

            // Voussoir ring.
            int segments=11;
            for(int i=0;i<segments;i++)
            {
                float t=i/(float)(segments-1);
                float a=Mathf.Lerp(18f,162f,t)*Mathf.Deg2Rad;
                float x=Mathf.Cos(a)*radius;
                float y=Mathf.Sin(a)*rise;
                var block=BeveledBlock(name+" · voussoir "+i,center+new Vector3(x,y+1.28f,0),
                    new Vector3(.58f,.48f,depth+.08f),i%2==0?material:StoneDark,.05f);
                block.transform.rotation=Quaternion.Euler(0,0,-Mathf.Rad2Deg*a+90f);
            }
        }

        static void TimberFrame(string prefix,Vector3 p,Vector3 size)
        {
            foreach(float x in new[]{-size.x*.38f,0f,size.x*.38f})
                BeveledBlock(prefix+" · timber post",p+new Vector3(x,1.05f,-size.z*.505f),
                    new Vector3(.12f,1.72f,.10f),Wood,.02f);
            BeveledBlock(prefix+" · timber beam",p+new Vector3(0,1.48f,-size.z*.51f),
                new Vector3(size.x*.88f,.12f,.10f),Wood,.02f);
            BeveledBlock(prefix+" · door",p+new Vector3(0,.62f,-size.z*.52f),
                new Vector3(.58f,1.18f,.08f),Wood,.015f);
        }

        static void GableRoof(string name,Vector3 p,float width,float depth,float rise,float yaw)
        {
            float half=width*.52f;
            float angle=Mathf.Atan2(rise,half)*Mathf.Rad2Deg;
            float slope=Mathf.Sqrt(half*half+rise*rise);
            var left=BeveledBlock(name+" · west",p+new Vector3(-width*.235f,rise*.46f,0),
                new Vector3(slope,.16f,depth),Roof,.025f);
            left.transform.rotation=Quaternion.Euler(0,yaw,-angle);
            var right=BeveledBlock(name+" · east",p+new Vector3(width*.235f,rise*.46f,0),
                new Vector3(slope,.16f,depth),Roof,.025f);
            right.transform.rotation=Quaternion.Euler(0,yaw,angle);
        }

        static void RockCluster(string name,Vector3 p,bool mirror)
        {
            float s=mirror?1f:-1f;
            for(int i=0;i<7;i++)
            {
                float x=(i%3)*.85f*s,z=(i/3)*1.05f,y=(i%2)*.18f;
                var rock=IrregularRock(name+" "+i,p+new Vector3(x,y,z),
                    new Vector3(1.25f+(i%2)*.35f,.90f+(i%3)*.18f,1.35f+(i%2)*.28f));
                rock.transform.rotation=Quaternion.Euler(i*7f,mirror?i*29f:-i*31f,i*5f);
            }
        }

        static GameObject IrregularRock(string name,Vector3 p,Vector3 scale)
        {
            var mesh=new Mesh{name=name+" mesh"};
            var verts=new[]{
                new Vector3(-.62f,-.42f,-.52f),new Vector3(.58f,-.45f,-.48f),
                new Vector3(.70f,-.30f,.44f),new Vector3(-.55f,-.35f,.60f),
                new Vector3(-.42f,.46f,-.35f),new Vector3(.38f,.58f,-.28f),
                new Vector3(.52f,.42f,.35f),new Vector3(-.30f,.66f,.42f)
            };
            var tris=new[]{0,1,4,1,5,4,1,2,5,2,6,5,2,3,6,3,7,6,3,0,7,0,4,7,4,5,7,5,6,7,0,3,1,1,3,2};
            mesh.vertices=verts;mesh.triangles=tris;mesh.RecalculateNormals();mesh.RecalculateBounds();
            var go=new GameObject(name);go.transform.SetParent(Root);go.transform.position=p;go.transform.localScale=scale;
            go.AddComponent<MeshFilter>().sharedMesh=mesh;go.AddComponent<MeshRenderer>().sharedMaterial=StoneDark;return go;
        }

        static void MossPatch(Vector3 p,float w,float h)
        {
            var m=BeveledBlock("GC · moss patch",p,new Vector3(w,h,.035f),Moss,.01f);
            m.transform.rotation=Quaternion.Euler(0,0,6f);
        }

        static void Shrub(Vector3 p,float size)
        {
            for(int i=0;i<3;i++)
            {
                var g=GameObject.CreatePrimitive(PrimitiveType.Sphere);g.name="GC · shrub";g.transform.SetParent(Root);
                g.transform.position=p+new Vector3((i-1)*.22f,i*.10f,(i%2)*.16f);
                g.transform.localScale=new Vector3(size*.75f,size*(.72f+i*.08f),size*.75f);
                var c=g.GetComponent<Collider>();if(c!=null)UnityEngine.Object.DestroyImmediate(c);
                g.GetComponent<Renderer>().sharedMaterial=Moss;
            }
        }

        static void Person(string name,Vector3 p)
        {
            var body=GameObject.CreatePrimitive(PrimitiveType.Capsule);body.name=name;body.transform.SetParent(Root);
            body.transform.position=p+Vector3.up*.62f;body.transform.localScale=new Vector3(.18f,.42f,.18f);
            var c=body.GetComponent<Collider>();if(c!=null)UnityEngine.Object.DestroyImmediate(c);
            body.GetComponent<Renderer>().sharedMaterial=Blue;
            var head=GameObject.CreatePrimitive(PrimitiveType.Sphere);head.name=name+" · head";head.transform.SetParent(Root);
            head.transform.position=p+Vector3.up*1.35f;head.transform.localScale=Vector3.one*.22f;
            var hc=head.GetComponent<Collider>();if(hc!=null)UnityEngine.Object.DestroyImmediate(hc);
            head.GetComponent<Renderer>().sharedMaterial=Simple(new Color(.55f,.39f,.28f),.04f,0f);
        }

        static void Log(string name,Vector3 p,float length,float radius,float yaw)
        {
            var go=GameObject.CreatePrimitive(PrimitiveType.Cylinder);go.name=name;go.transform.SetParent(Root);
            go.transform.position=p;go.transform.localScale=new Vector3(radius,length*.5f,radius);
            go.transform.rotation=Quaternion.Euler(90f,yaw,0);
            var c=go.GetComponent<Collider>();if(c!=null)UnityEngine.Object.DestroyImmediate(c);
            go.GetComponent<Renderer>().sharedMaterial=Wood;
        }

        static void Banner(Vector3 p,bool mirror)
        {
            BeveledBlock("GC · banner pole",p+new Vector3(mirror?.42f:-.42f,.12f,0),new Vector3(.05f,1.75f,.05f),Metal,.01f);
            var cloth=BeveledBlock("GC · blue banner",p,new Vector3(.72f,1.38f,.045f),Blue,.01f);
            cloth.transform.rotation=Quaternion.Euler(0,0,mirror?3f:-3f);
            BeveledBlock("GC · gold banner trim",p+new Vector3(0,-.64f,-.03f),new Vector3(.74f,.08f,.055f),Gold,.01f);
        }

        static void WarmLight(string name,Vector3 p,float range,float intensity)
        {
            var l=new GameObject(name).AddComponent<Light>();l.transform.SetParent(Root);
            l.type=LightType.Point;l.range=range;l.intensity=intensity;l.color=new Color(1f,.53f,.23f);
            l.shadows=LightShadows.None;l.transform.position=p;
        }

        static Material Pbr(string prefix,Color tint,Vector2 tiling,float smooth,float bump)
        {
            var diff=Resources.Load<Texture2D>("Valoria/GoldenCellExternal/"+prefix+"_diff");
            if(diff==null)return Procedural(prefix=="stone"?"stone":"ground",tint,smooth);
            var shader=Shader.Find("Universal Render Pipeline/Lit")??Shader.Find("Standard");
            var m=new Material(shader){name="Golden Cell PBR · "+prefix};
            if(m.HasProperty("_BaseColor"))m.SetColor("_BaseColor",tint);
            if(m.HasProperty("_Color"))m.SetColor("_Color",tint);
            if(m.HasProperty("_BaseMap")){m.SetTexture("_BaseMap",diff);m.SetTextureScale("_BaseMap",tiling);}
            else if(m.HasProperty("_MainTex")){m.SetTexture("_MainTex",diff);m.SetTextureScale("_MainTex",tiling);}
            var normal=Resources.Load<Texture2D>("Valoria/GoldenCellExternal/"+prefix+"_normal");
            if(normal!=null&&m.HasProperty("_BumpMap"))
            {
                m.SetTexture("_BumpMap",normal);m.SetTextureScale("_BumpMap",tiling);
                if(m.HasProperty("_BumpScale"))m.SetFloat("_BumpScale",bump);m.EnableKeyword("_NORMALMAP");
            }
            var ao=Resources.Load<Texture2D>("Valoria/GoldenCellExternal/"+prefix+"_ao");
            if(ao!=null&&m.HasProperty("_OcclusionMap"))
            {
                m.SetTexture("_OcclusionMap",ao);m.SetTextureScale("_OcclusionMap",tiling);
                if(m.HasProperty("_OcclusionStrength"))m.SetFloat("_OcclusionStrength",1f);
            }
            if(m.HasProperty("_Smoothness"))m.SetFloat("_Smoothness",smooth);
            if(m.HasProperty("_Metallic"))m.SetFloat("_Metallic",0f);
            return m;
        }

        static Material Procedural(string kind,Color baseColor,float smooth)
        {
            const int size=128;
            var tex=new Texture2D(size,size,TextureFormat.RGBA32,true){wrapMode=TextureWrapMode.Repeat,filterMode=FilterMode.Trilinear};
            var px=new Color[size*size];
            for(int y=0;y<size;y++)for(int x=0;x<size;x++)
            {
                float n=Mathf.PerlinNoise(x*.065f+4.7f,y*.065f+13.2f);
                float shade=.78f+n*.32f;
                if(kind=="wood")
                {
                    float grain=.11f*Mathf.Sin(x*.35f+y*.035f)+.035f*Mathf.Sin(x*.11f);
                    bool seam=x%24<2;shade+=grain-(seam?.22f:0f);
                }
                else if(kind=="slate")
                {
                    bool row=y%15<2;bool joint=(x+((y/15)%2)*10)%22<2;
                    shade+=(row||joint)?-.22f:.04f*Mathf.Sin(x*.13f+y*.21f);
                }
                else if(kind=="moss")
                {
                    shade=.62f+n*.45f;
                }
                else
                {
                    shade=.74f+n*.28f;
                }
                px[y*size+x]=new Color(Mathf.Clamp01(baseColor.r*shade),Mathf.Clamp01(baseColor.g*shade),Mathf.Clamp01(baseColor.b*shade),1f);
            }
            tex.SetPixels(px);tex.Apply(true,false);
            var shader=Shader.Find("Universal Render Pipeline/Lit")??Shader.Find("Standard");
            var m=new Material(shader){name="Golden Cell procedural · "+kind};
            if(m.HasProperty("_BaseColor"))m.SetColor("_BaseColor",Color.white);
            if(m.HasProperty("_BaseMap")){m.SetTexture("_BaseMap",tex);m.SetTextureScale("_BaseMap",new Vector2(3.2f,3.2f));}
            if(m.HasProperty("_Smoothness"))m.SetFloat("_Smoothness",smooth);
            if(m.HasProperty("_Metallic"))m.SetFloat("_Metallic",0f);
            return m;
        }

        static Material Simple(Color color,float smooth,float metallic)
        {
            var shader=Shader.Find("Universal Render Pipeline/Lit")??Shader.Find("Standard");
            var m=new Material(shader);
            if(m.HasProperty("_BaseColor"))m.SetColor("_BaseColor",color);
            if(m.HasProperty("_Color"))m.SetColor("_Color",color);
            if(m.HasProperty("_Smoothness"))m.SetFloat("_Smoothness",smooth);
            if(m.HasProperty("_Metallic"))m.SetFloat("_Metallic",metallic);
            return m;
        }

        static GameObject BeveledBlock(string name,Vector3 p,Vector3 scale,Material material,float bevel)
        {
            // Main mass + thin darker/light edge strips create a bevel read without ProBuilder.
            var go=GameObject.CreatePrimitive(PrimitiveType.Cube);go.name=name;go.transform.SetParent(Root);
            go.transform.position=p;go.transform.localScale=scale;
            var c=go.GetComponent<Collider>();if(c!=null)UnityEngine.Object.DestroyImmediate(c);
            go.GetComponent<Renderer>().sharedMaterial=material;
            if(bevel>0f)
            {
                float t=Mathf.Min(bevel,Mathf.Min(scale.x,scale.y)*.18f);
                Edge(name+" · top edge",p+new Vector3(0,scale.y*.5f+t*.15f,-scale.z*.5f+t*.5f),
                    new Vector3(scale.x*.96f,t,t),material);
                Edge(name+" · left edge",p+new Vector3(-scale.x*.5f+t*.5f,0,-scale.z*.5f+t*.5f),
                    new Vector3(t,scale.y*.92f,t),material);
            }
            return go;
        }

        static void Edge(string name,Vector3 p,Vector3 scale,Material m)
        {
            var e=GameObject.CreatePrimitive(PrimitiveType.Cube);e.name=name;e.transform.SetParent(Root);
            e.transform.position=p;e.transform.localScale=scale;
            var c=e.GetComponent<Collider>();if(c!=null)UnityEngine.Object.DestroyImmediate(c);
            e.GetComponent<Renderer>().sharedMaterial=m;
        }
    }
}
