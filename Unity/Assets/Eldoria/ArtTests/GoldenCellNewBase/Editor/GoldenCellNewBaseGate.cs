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

        public static void CaptureFinal()
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

            GoldenCellFinished.Build();

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
            const string folder="Assets/Resources/Valoria/GoldenCellExternal";
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
        static Material Stone,Plaster,Wood,Roof,Ground,Rock,Metal,Moss,Blue,Gold,Corruption;

        public static void Build()
        {
            Root=new GameObject("GOLDEN CELL · new artistic base v1").transform;
            BuildMaterials();
            ConfigureLight();
            BuildTerraceAndRoute();
            BuildHeroAccess();
            BuildRockArchitectureTransition();
            BuildSupportArchitecture();
            BuildLife();
            BuildCorruptionHint();
        }

        static void BuildMaterials()
        {
            Stone=Pbr("stone",new Color(.93f,.88f,.76f,1f),new Vector2(2.6f,2.6f),.07f,1.05f,0f);
            Plaster=Pbr("plaster",new Color(.91f,.84f,.70f,1f),new Vector2(2.2f,2.2f),.05f,.82f,0f);
            Wood=Pbr("wood",new Color(.62f,.39f,.20f,1f),new Vector2(2.8f,2.8f),.10f,.92f,0f);
            Roof=Pbr("roof",new Color(.26f,.34f,.39f,1f),new Vector2(2.7f,2.7f),.08f,1.00f,0f);
            Ground=Pbr("ground",new Color(.76f,.68f,.54f,1f),new Vector2(3.4f,3.4f),.06f,1.10f,0f);
            Rock=Pbr("mossrock",new Color(.58f,.58f,.49f,1f),new Vector2(2.1f,2.1f),.04f,1.12f,0f);
            Metal=Pbr("metal",new Color(.34f,.37f,.38f,1f),new Vector2(2.0f,2.0f),.22f,.65f,.62f);
            Moss=Pbr("mossrock",new Color(.50f,.68f,.43f,1f),new Vector2(3.0f,3.0f),.03f,.90f,0f);
            Blue=Solid(new Color(.055f,.19f,.38f,1f),.16f,.05f);
            Gold=Solid(new Color(.76f,.52f,.17f,1f),.28f,.48f);
            Corruption=Solid(new Color(.43f,.09f,.57f,1f),.18f,.06f);
            if(Corruption.HasProperty("_EmissionColor"))
            {
                Corruption.EnableKeyword("_EMISSION");
                Corruption.SetColor("_EmissionColor",new Color(.72f,.12f,1.0f)*1.65f);
            }
        }

        static void ConfigureLight()
        {
            RenderSettings.ambientMode=AmbientMode.Trilight;
            RenderSettings.ambientSkyColor=new Color(.73f,.80f,.84f);
            RenderSettings.ambientEquatorColor=new Color(.54f,.52f,.46f);
            RenderSettings.ambientGroundColor=new Color(.25f,.23f,.20f);
            RenderSettings.ambientIntensity=.95f;
            RenderSettings.fog=true;
            RenderSettings.fogMode=FogMode.Linear;
            RenderSettings.fogColor=new Color(.57f,.65f,.69f);
            RenderSettings.fogStartDistance=48f;
            RenderSettings.fogEndDistance=118f;

            foreach(var l in UnityEngine.Object.FindObjectsByType<Light>(FindObjectsSortMode.None))
            {
                if(l.type!=LightType.Directional)continue;
                l.color=new Color(1f,.90f,.76f);
                l.intensity=1.15f;
                l.shadowStrength=.70f;
                l.shadows=LightShadows.Soft;
                l.transform.rotation=Quaternion.Euler(46f,-36f,0f);
            }
            var fill=new GameObject("GC · cool valley fill").AddComponent<Light>();
            fill.transform.SetParent(Root);fill.type=LightType.Directional;fill.intensity=.24f;
            fill.color=new Color(.59f,.72f,1f);fill.transform.rotation=Quaternion.Euler(34f,145f,0f);fill.shadows=LightShadows.None;
        }

        static void BuildTerraceAndRoute()
        {
            // Thin authored surfaces sit over the certified topology; no gameplay collider is introduced.
            ChamferBox("GC · upper limestone terrace",new Vector3(0f,2.50f,4.72f),new Vector3(12.3f,.30f,5.55f),.18f,Stone);
            ChamferBox("GC · lower cobble plaza",new Vector3(0f,.63f,-.55f),new Vector3(8.4f,.13f,4.8f),.15f,Ground);

            for(int i=0;i<9;i++)
            {
                float z=.15f+i*.53f;
                float y=.79f+i*.204f;
                ChamferBox("GC · processional stair "+i,new Vector3(0f,y,z),new Vector3(4.08f,.20f,.56f),.06f,Ground);
            }

            // Gold inlay defines the processional axis without neon/UI language.
            for(int i=0;i<5;i++)
                ThinStrip("GC · gold road inlay "+i,new Vector3(0f,.715f,-1.65f+i*.62f),new Vector3(.055f,.025f,.46f),Gold);
        }

        static void BuildHeroAccess()
        {
            // A single new hero fragment: civic arch + flanking gatehouses. It is intentionally
            // subordinate to the existing Bastion towers but transforms the access silhouette.
            var centre=new Vector3(0f,3.65f,3.72f);
            ArchRing("GC · monumental arch",centre,1.48f,.42f,.62f,18,Stone);
            ChamferBox("GC · arch west pier",new Vector3(-1.70f,3.30f,3.72f),new Vector3(.70f,2.75f,.82f),.08f,Stone);
            ChamferBox("GC · arch east pier",new Vector3(1.70f,3.30f,3.72f),new Vector3(.70f,2.75f,.82f),.08f,Stone);

            GateHouse("GC · west gatehouse",new Vector3(-4.75f,2.72f,4.15f),false);
            GateHouse("GC · east gatehouse",new Vector3(4.75f,2.72f,4.15f),true);

            // Blue/gold heraldry concentrated at the monumental threshold.
            Banner(new Vector3(-2.32f,4.45f,3.05f),false);
            Banner(new Vector3(2.32f,4.45f,3.05f),true);

            var warm=new GameObject("GC · arch firelight").AddComponent<Light>();
            warm.transform.SetParent(Root);warm.type=LightType.Point;warm.range=8.5f;warm.intensity=3.1f;
            warm.color=new Color(1f,.55f,.24f);warm.transform.position=new Vector3(0f,3.25f,2.95f);warm.shadows=LightShadows.None;
        }

        static void GateHouse(string name,Vector3 p,bool mirror)
        {
            float s=mirror?-1f:1f;
            ChamferBox(name+" · stone base",p+new Vector3(0,.60f,0),new Vector3(2.45f,2.65f,3.05f),.12f,Stone);
            GableRoof(name+" · slate roof",p+new Vector3(0,2.15f,.05f),2.75f,3.45f,.92f,Roof);
            ChamferBox(name+" · corner buttress",p+new Vector3(s*1.38f,.35f,-.28f),new Vector3(.52f,2.50f,.92f),.07f,Stone);
            ChamferBox(name+" · timber door",p+new Vector3(-s*.42f,.40f,-1.56f),new Vector3(.62f,1.32f,.10f),.03f,Wood);
            ChamferBox(name+" · gold lintel",p+new Vector3(-s*.42f,1.20f,-1.61f),new Vector3(.86f,.10f,.08f),.02f,Gold);
            ThinStrip(name+" · blue fascia",p+new Vector3(0,1.45f,-1.60f),new Vector3(1.28f,.08f,.06f),Blue);
        }

        static void BuildRockArchitectureTransition()
        {
            // Stepped masonry progressively gives way to irregular rock, then moss.
            for(int i=0;i<3;i++)
            {
                float off=6.0f+i*.72f;
                float y=1.50f-i*.20f;
                float z=3.72f+i*.73f;
                var west=ChamferBox("GC · west retaining "+i,new Vector3(-off,y,z),new Vector3(1.15f,2.35f-i*.25f,2.60f),.12f,Stone);
                west.transform.rotation=Quaternion.Euler(0,12f+i*7f,0);
                var east=ChamferBox("GC · east retaining "+i,new Vector3(off,y,z),new Vector3(1.15f,2.35f-i*.25f,2.60f),.12f,Stone);
                east.transform.rotation=Quaternion.Euler(0,-12f-i*7f,0);
            }

            RockMass("GC · west mountain shoulder",new Vector3(-8.5f,.42f,5.85f),new Vector3(5.3f,3.3f,6.1f),Rock,17);
            RockMass("GC · east mountain shoulder",new Vector3(8.5f,.42f,5.85f),new Vector3(5.3f,3.3f,6.1f),Rock,29);

            // Moss sits at the actual seam, not randomly over every surface.
            for(int i=0;i<8;i++)
            {
                float side=i<4?-1f:1f;
                float j=i%4;
                var moss=Primitive(PrimitiveType.Sphere,"GC · seam moss "+i,
                    new Vector3(side*(6.35f+j*.65f),.78f+j*.11f,4.35f+j*.55f),
                    new Vector3(.62f,.10f,.48f),Moss);
                moss.transform.rotation=Quaternion.Euler(0,j*31f,0);
            }
        }

        static void BuildSupportArchitecture()
        {
            TimberHouse("GC · west residence",new Vector3(-7.75f,.56f,-1.02f),new Vector3(2.80f,2.05f,2.75f),-8f,false);
            TimberHouse("GC · east workshop",new Vector3(7.70f,.56f,-1.15f),new Vector3(3.12f,2.00f,2.95f),8f,true);

            Arcade("GC · west civic arcade",new Vector3(-4.55f,.73f,-.62f),-3f);
            Arcade("GC · east civic arcade",new Vector3(4.55f,.73f,-.62f),3f);
        }

        static void TimberHouse(string name,Vector3 p,Vector3 size,float yaw,bool workshop)
        {
            var q=Quaternion.Euler(0,yaw,0);
            var footing=ChamferBox(name+" · footing",p+new Vector3(0,.17f,0),new Vector3(size.x*1.06f,.30f,size.z*1.05f),.06f,Stone);footing.transform.rotation=q;
            var body=ChamferBox(name+" · plaster body",p+new Vector3(0,size.y*.52f,0),size,.08f,Plaster);body.transform.rotation=q;
            GableRoof(name+" · slate roof",p+new Vector3(0,size.y+.12f,0),size.x*1.13f,size.z*1.15f,.78f,Roof,yaw);

            foreach(float x in new[]{-.38f,.38f})
            {
                var post=ChamferBox(name+" · timber post",p+q*new Vector3(x*size.x,size.y*.53f,-size.z*.515f),
                    new Vector3(.11f,size.y*.88f,.12f),.02f,Wood);post.transform.rotation=q;
            }
            var beam=ChamferBox(name+" · timber beam",p+q*new Vector3(0,size.y*.76f,-size.z*.52f),
                new Vector3(size.x*.82f,.12f,.13f),.02f,Wood);beam.transform.rotation=q;
            var door=ChamferBox(name+" · door",p+q*new Vector3(0,.62f,-size.z*.525f),
                new Vector3(.52f,1.12f,.11f),.02f,Wood);door.transform.rotation=q;

            if(workshop)
            {
                var awning=ChamferBox(name+" · work awning",p+q*new Vector3(-size.x*.55f,.90f,-.15f),
                    new Vector3(size.x*.42f,.10f,size.z*.70f),.02f,Wood);awning.transform.rotation=q*Quaternion.Euler(0,0,-8f);
                for(int i=0;i<3;i++)
                    ChamferBox(name+" · crate "+i,p+q*new Vector3(-1.4f+i*.58f,.28f,-1.75f),new Vector3(.48f,.48f,.48f),.03f,Wood).transform.rotation=q;
            }
        }

        static void Arcade(string name,Vector3 p,float yaw)
        {
            var q=Quaternion.Euler(0,yaw,0);
            for(int i=-1;i<=1;i++)
            {
                var pier=ChamferBox(name+" · pier "+i,p+q*new Vector3(i*1.22f,.68f,0),new Vector3(.30f,1.36f,.46f),.05f,Stone);pier.transform.rotation=q;
                var beam=ChamferBox(name+" · beam "+i,p+q*new Vector3(i*1.22f,1.42f,0),new Vector3(1.30f,.24f,.49f),.04f,Stone);beam.transform.rotation=q;
            }
        }

        static void BuildLife()
        {
            // Lamps, work props and restrained vegetation make the cell inhabited without hiding circulation.
            foreach(var p in new[]{new Vector3(-2.75f,.72f,-1.55f),new Vector3(2.75f,.72f,-1.55f)})
                Lamp(p);

            for(int i=0;i<4;i++)
            {
                float side=i<2?-1f:1f;
                float j=i%2;
                var shrub=Primitive(PrimitiveType.Sphere,"GC · terrace shrub "+i,
                    new Vector3(side*(6.0f+j*1.25f),1.00f,-.15f+j*.95f),
                    new Vector3(.68f,.82f,.68f),Moss);
                shrub.transform.rotation=Quaternion.Euler(0,i*33f,0);
            }

            // Light smoke over productive side only.
            for(int i=0;i<4;i++)
            {
                var smoke=Primitive(PrimitiveType.Sphere,"GC · workshop smoke "+i,
                    new Vector3(8.35f+i*.06f,3.25f+i*.50f,-.45f),
                    Vector3.one*(.28f+i*.07f),Solid(new Color(.45f,.45f,.43f,1f),.02f,0f));
            }
        }

        static void Lamp(Vector3 p)
        {
            Primitive(PrimitiveType.Cylinder,"GC · lamp post",p+Vector3.up*.78f,new Vector3(.07f,.80f,.07f),Metal);
            ChamferBox("GC · lamp cage",p+Vector3.up*1.62f,new Vector3(.34f,.42f,.34f),.04f,Metal);
            var glow=new GameObject("GC · lamp glow").AddComponent<Light>();
            glow.transform.SetParent(Root);glow.type=LightType.Point;glow.range=3.8f;glow.intensity=1.7f;
            glow.color=new Color(1f,.55f,.25f);glow.transform.position=p+Vector3.up*1.65f;glow.shadows=LightShadows.None;
        }

        static void BuildCorruptionHint()
        {
            // Secondary distant read: thin ground scars, never crystals/cylinders.
            for(int i=0;i<5;i++)
            {
                var strip=ThinStrip("GC · distant corruption scar "+i,
                    new Vector3(10.4f+i*.38f,.38f,10.4f+i*.58f),
                    new Vector3(.055f,.022f,.75f+i*.18f),Corruption);
                strip.transform.rotation=Quaternion.Euler(0,22f+i*11f,0);
            }
            var glow=new GameObject("GC · corruption haze").AddComponent<Light>();
            glow.transform.SetParent(Root);glow.type=LightType.Point;glow.range=9f;glow.intensity=1.25f;
            glow.color=new Color(.54f,.12f,.75f);glow.transform.position=new Vector3(11.2f,1.15f,11.4f);glow.shadows=LightShadows.None;
        }

        static void Banner(Vector3 p,bool mirror)
        {
            Primitive(PrimitiveType.Cylinder,"GC · banner pole",p+new Vector3(mirror?.42f:-.42f,.10f,0),
                new Vector3(.035f,.90f,.035f),Metal);
            ChamferBox("GC · blue banner",p,new Vector3(.62f,1.28f,.045f),.015f,Blue);
            ThinStrip("GC · gold banner trim",p+new Vector3(0,-.56f,-.028f),new Vector3(.62f,.055f,.025f),Gold);
        }

        static void GableRoof(string name,Vector3 p,float width,float depth,float rise,Material mat,float yaw=0f)
        {
            var root=new GameObject(name);root.transform.SetParent(Root);root.transform.position=p;root.transform.rotation=Quaternion.Euler(0,yaw,0);
            float half=width*.5f;
            float slope=Mathf.Sqrt(half*half+rise*rise);
            float ang=Mathf.Atan2(rise,half)*Mathf.Rad2Deg;
            var left=ChamferBox(name+" · left",p+Quaternion.Euler(0,yaw,0)*new Vector3(-width*.235f,rise*.48f,0),
                new Vector3(slope,.14f,depth),.025f,mat);
            left.transform.rotation=Quaternion.Euler(0,yaw,-ang);
            var right=ChamferBox(name+" · right",p+Quaternion.Euler(0,yaw,0)*new Vector3(width*.235f,rise*.48f,0),
                new Vector3(slope,.14f,depth),.025f,mat);
            right.transform.rotation=Quaternion.Euler(0,yaw,ang);
        }

        static void ArchRing(string name,Vector3 center,float inner,float thickness,float depth,int segments,Material mat)
        {
            float outer=inner+thickness;
            var verts=new List<Vector3>();var tris=new List<int>();var uvs=new List<Vector2>();
            for(int i=0;i<=segments;i++)
            {
                float t=Mathf.PI*i/segments;
                float co=Mathf.Cos(t),si=Mathf.Sin(t);
                foreach(float z in new[]{-depth*.5f,depth*.5f})
                {
                    verts.Add(center+new Vector3(co*outer,si*outer,z));
                    verts.Add(center+new Vector3(co*inner,si*inner,z));
                    uvs.Add(new Vector2(i/(float)segments,1));uvs.Add(new Vector2(i/(float)segments,0));
                }
            }
            for(int i=0;i<segments;i++)
            {
                int a=i*4,b=a+1,c=a+2,d=a+3;
                int na=a+4,nb=b+4,nc=c+4,nd=d+4;
                AddQuad(tris,a,na,nb,b);      // back face band
                AddQuad(tris,c,d,nd,nc);      // front face band
                AddQuad(tris,a,c,nc,na);      // outer curve
                AddQuad(tris,b,nb,nd,d);      // inner curve
            }
            var mesh=new Mesh{name=name+" mesh"};mesh.SetVertices(verts);mesh.SetTriangles(tris,0);mesh.SetUVs(0,uvs);mesh.RecalculateNormals();mesh.RecalculateBounds();
            var go=new GameObject(name);go.transform.SetParent(Root);go.AddComponent<MeshFilter>().sharedMesh=mesh;go.AddComponent<MeshRenderer>().sharedMaterial=mat;
        }

        static void RockMass(string name,Vector3 p,Vector3 scale,Material mat,int seed)
        {
            // Low-poly authored rock prism; deterministic and visually irregular rather than a stretched cube.
            var verts=new List<Vector3>();
            int ring=8;
            for(int level=0;level<3;level++)
            {
                float y=level==0?0f:level==1?.48f:1f;
                float radius=level==0?1f:level==1?.90f:.48f;
                for(int i=0;i<ring;i++)
                {
                    float a=Mathf.PI*2f*i/ring;
                    float wobble=.86f+.10f*Mathf.Sin(seed*.7f+i*2.31f+level);
                    verts.Add(new Vector3(Mathf.Cos(a)*radius*wobble,y,Mathf.Sin(a)*radius*(1.04f+.08f*Mathf.Cos(seed+i))));
                }
            }
            var tris=new List<int>();
            for(int l=0;l<2;l++)for(int i=0;i<ring;i++)
            {
                int n=(i+1)%ring;int a=l*ring+i,b=l*ring+n,c=(l+1)*ring+i,d=(l+1)*ring+n;
                AddQuad(tris,a,c,d,b);
            }
            for(int i=1;i<ring-1;i++){tris.Add(16);tris.Add(16+i);tris.Add(16+i+1);}
            var mesh=new Mesh{name=name+" mesh"};mesh.SetVertices(verts);mesh.SetTriangles(tris,0);
            var uvs=new List<Vector2>();foreach(var v in verts)uvs.Add(new Vector2(v.x*.5f+.5f,v.z*.5f+.5f));mesh.SetUVs(0,uvs);
            mesh.RecalculateNormals();mesh.RecalculateBounds();
            var go=new GameObject(name);go.transform.SetParent(Root);go.transform.position=p;go.transform.localScale=scale;
            go.AddComponent<MeshFilter>().sharedMesh=mesh;go.AddComponent<MeshRenderer>().sharedMaterial=mat;
        }

        static GameObject ChamferBox(string name,Vector3 p,Vector3 scale,float bevel,Material mat)
        {
            // Small bevel via nested masonry skin: keeps silhouettes clean at mobile zoom without ProBuilder.
            var go=Primitive(PrimitiveType.Cube,name,p,scale,mat);
            if(bevel>.001f)
            {
                // Corner highlight strips produce readable bevel response from the fixed camera.
                var q=go.transform.rotation;
                float bx=Mathf.Max(.03f,bevel),by=Mathf.Max(.03f,bevel);
                foreach(float sx in new[]{-1f,1f})foreach(float sy in new[]{-1f,1f})
                {
                    var strip=Primitive(PrimitiveType.Cube,name+" · edge",p+new Vector3(sx*(scale.x*.5f-bx*.5f),sy*(scale.y*.5f-by*.5f),-scale.z*.502f),
                        new Vector3(bx,by,scale.z*.02f),mat);
                    strip.transform.rotation=q;
                }
            }
            return go;
        }

        static GameObject ThinStrip(string name,Vector3 p,Vector3 scale,Material mat)
            =>ChamferBox(name,p,scale,.01f,mat);

        static GameObject Primitive(PrimitiveType type,string name,Vector3 p,Vector3 scale,Material mat)
        {
            var go=GameObject.CreatePrimitive(type);go.name=name;go.transform.SetParent(Root);go.transform.position=p;go.transform.localScale=scale;
            var c=go.GetComponent<Collider>();if(c!=null)UnityEngine.Object.DestroyImmediate(c);
            go.GetComponent<Renderer>().sharedMaterial=mat;return go;
        }

        static void AddQuad(List<int> tris,int a,int b,int c,int d)
        { tris.Add(a);tris.Add(b);tris.Add(c);tris.Add(a);tris.Add(c);tris.Add(d); }

        static Material Pbr(string prefix,Color tint,Vector2 tiling,float smooth,float bump,float metallic)
        {
            var shader=Shader.Find("Universal Render Pipeline/Lit")??Shader.Find("Standard");
            var m=new Material(shader){name="GC PBR · "+prefix};
            var diff=Resources.Load<Texture2D>("Valoria/GoldenCellExternal/"+prefix+"_diff");
            var normal=Resources.Load<Texture2D>("Valoria/GoldenCellExternal/"+prefix+"_normal");
            var ao=Resources.Load<Texture2D>("Valoria/GoldenCellExternal/"+prefix+"_ao");
            if(m.HasProperty("_BaseColor"))m.SetColor("_BaseColor",tint);
            if(m.HasProperty("_Color"))m.SetColor("_Color",tint);
            if(diff!=null)
            {
                if(m.HasProperty("_BaseMap")){m.SetTexture("_BaseMap",diff);m.SetTextureScale("_BaseMap",tiling);}
                else if(m.HasProperty("_MainTex")){m.SetTexture("_MainTex",diff);m.SetTextureScale("_MainTex",tiling);}
            }
            if(normal!=null&&m.HasProperty("_BumpMap"))
            {
                m.SetTexture("_BumpMap",normal);m.SetTextureScale("_BumpMap",tiling);m.SetFloat("_BumpScale",bump);m.EnableKeyword("_NORMALMAP");
            }
            if(ao!=null&&m.HasProperty("_OcclusionMap"))
            {
                m.SetTexture("_OcclusionMap",ao);m.SetTextureScale("_OcclusionMap",tiling);m.SetFloat("_OcclusionStrength",1f);
            }
            if(m.HasProperty("_Smoothness"))m.SetFloat("_Smoothness",smooth);
            if(m.HasProperty("_Metallic"))m.SetFloat("_Metallic",metallic);
            return m;
        }

        static Material Solid(Color color,float smooth,float metallic)
        {
            var shader=Shader.Find("Universal Render Pipeline/Lit")??Shader.Find("Standard");
            var m=new Material(shader);
            if(m.HasProperty("_BaseColor"))m.SetColor("_BaseColor",color);
            if(m.HasProperty("_Color"))m.SetColor("_Color",color);
            if(m.HasProperty("_Smoothness"))m.SetFloat("_Smoothness",smooth);
            if(m.HasProperty("_Metallic"))m.SetFloat("_Metallic",metallic);
            return m;
        }
    }

}