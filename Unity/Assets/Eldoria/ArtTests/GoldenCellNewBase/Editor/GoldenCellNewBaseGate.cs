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
        static Material Limestone,DeepStone,Oak,Slate,Rock,Ground,Blue,Plaster;

        public static void Build()
        {
            Root=new GameObject("GOLDEN CELL · finished new artistic base").transform;
            BuildMaterials();
            SuppressOldCellVisuals();

            // TERRAIN / TERRACE — layered geology first, architecture seated into it.
            RockMass("GC FINAL · west rock foundation",new Vector3(-6.3f,1.45f,7.0f),new Vector3(5.8f,3.5f,7.4f),-11f);
            RockMass("GC FINAL · east rock foundation",new Vector3(6.2f,1.35f,7.1f),new Vector3(5.6f,3.3f,7.2f),12f);
            RockMass("GC FINAL · lower west seam",new Vector3(-5.1f,.52f,1.2f),new Vector3(4.7f,1.5f,4.2f),-7f);
            RockMass("GC FINAL · lower east seam",new Vector3(5.0f,.48f,1.0f),new Vector3(4.4f,1.4f,4.0f),9f);

            Block("GC FINAL · upper terrace",new Vector3(0f,2.62f,6.35f),new Vector3(12.8f,.46f,7.1f),Limestone);
            Block("GC FINAL · plaza",new Vector3(0f,2.84f,2.65f),new Vector3(8.25f,.12f,4.05f),Ground);
            Block("GC FINAL · retaining west",new Vector3(-5.35f,3.18f,4.75f),new Vector3(.72f,2.10f,5.0f),DeepStone);
            Block("GC FINAL · retaining east",new Vector3(5.35f,3.18f,4.75f),new Vector3(.72f,2.10f,5.0f),DeepStone);

            // MAIN ACCESS — keep the approved blockout hierarchy, replace block masses with a layered fortress facade.
            Tower("GC FINAL · west gate tower",new Vector3(-3.15f,2.88f,6.80f),2.58f,5.55f,3.10f);
            Tower("GC FINAL · east gate tower",new Vector3(3.15f,2.88f,6.80f),2.58f,5.25f,3.10f);
            Keep("GC FINAL · high keep",new Vector3(.15f,2.90f,8.55f),4.35f,6.20f,3.65f);

            // Recessed gatehouse: readable opening, deep jambs, timber gate and warm interior.
            Block("GC FINAL · gatehouse lintel",new Vector3(0f,6.55f,6.15f),new Vector3(3.55f,1.00f,2.25f),Limestone);
            Block("GC FINAL · gate jamb west",new Vector3(-1.55f,4.56f,6.15f),new Vector3(.80f,3.95f,2.25f),DeepStone);
            Block("GC FINAL · gate jamb east",new Vector3(1.55f,4.56f,6.15f),new Vector3(.80f,3.95f,2.25f),DeepStone);
            Block("GC FINAL · gate recess",new Vector3(0f,4.45f,6.32f),new Vector3(2.35f,3.35f,.34f),Rock);
            var gate=Block("GC FINAL · timber portcullis",new Vector3(0f,4.20f,6.10f),new Vector3(2.02f,2.85f,.16f),Oak);
            AddGateSlats(gate.transform.position,new Vector3(2.02f,2.85f,.20f));

            // Curtain walls close the fortress volume without flattening the skyline.
            Block("GC FINAL · west curtain",new Vector3(-4.52f,4.12f,7.15f),new Vector3(1.85f,2.55f,3.15f),Limestone);
            Block("GC FINAL · east curtain",new Vector3(4.52f,4.05f,7.15f),new Vector3(1.85f,2.40f,3.15f),Limestone);
            Battlements("GC FINAL · west curtain crown",new Vector3(-4.52f,5.48f,6.35f),3.0f,0f);
            Battlements("GC FINAL · east curtain crown",new Vector3(4.52f,5.36f,6.35f),3.0f,0f);

            // PROCESSIONAL ROUTE — worn stone stair, broader bottom and tighter upper landing.
            for(int i=0;i<10;i++)
            {
                float t=i/9f;
                float z=-.55f+i*.49f;
                float y=.70f+i*.235f;
                float w=Mathf.Lerp(4.85f,3.75f,t);
                var step=Block("GC FINAL · processional stair "+i,new Vector3(0f,y,z),
                    new Vector3(w,.22f,.55f),Ground);
                step.transform.rotation=Quaternion.Euler(0f,(i%3-1)*.55f,0f);
            }
            Block("GC FINAL · stair west cheek",new Vector3(-2.42f,1.68f,1.58f),new Vector3(.42f,2.25f,4.4f),DeepStone);
            Block("GC FINAL · stair east cheek",new Vector3(2.42f,1.68f,1.58f),new Vector3(.42f,2.25f,4.4f),DeepStone);

            // SUPPORT BUILDINGS — detailed authored family, normalized into the Golden Cell palette.
            var houseA=AssetDatabase.LoadAssetAtPath<GameObject>(
                "Assets/EmaceArt/Slavic World Free/Prefabs/Town/Building/EA03_Town_House_Comp_01a_PRE.prefab");
            var shed=AssetDatabase.LoadAssetAtPath<GameObject>(
                "Assets/EmaceArt/Slavic World Free/Prefabs/Village/Building/Shed/EA03_Village_OutBuilding_Shed_03b_PRE.prefab");
            var admin=AssetDatabase.LoadAssetAtPath<GameObject>(
                "Assets/EmaceArt/Slavic World Free/Prefabs/Town/Administrative/EA03_Town_Building_Administrative _01a_PRE.prefab");
            PlaceAuthored("GC FINAL · residence",houseA,new Vector3(-7.55f,2.93f,2.35f),3.25f,3.55f,166f,
                new Color(.57f,.52f,.45f,1f));
            PlaceAuthored("GC FINAL · productive shed",shed,new Vector3(7.35f,2.92f,2.15f),3.45f,3.40f,194f,
                new Color(.54f,.48f,.39f,1f));
            PlaceAuthored("GC FINAL · civic annex",admin,new Vector3(-8.0f,2.96f,7.65f),2.65f,3.15f,120f,
                new Color(.55f,.52f,.47f,1f));

            // ROCK ↔ ARCHITECTURE TRANSITIONS — buttresses disappear into geology instead of ending on flat boards.
            Buttress(new Vector3(-5.28f,2.95f,5.05f),-8f);
            Buttress(new Vector3(5.28f,2.95f,5.05f),8f);
            Rubble(new Vector3(-4.95f,2.82f,3.20f),8,.42f);
            Rubble(new Vector3(4.95f,2.82f,3.15f),8,.42f);

            // SPARSE VEGETATION — grouped framing, no uniform scatter.
            var tree=AssetDatabase.LoadAssetAtPath<GameObject>(
                "Assets/EmaceArt/Slavic World Free/Prefabs/Nature/Tree/EA03_Nature_Tree_02b_PRE.prefab");
            foreach(var spec in new[]{
                new Vector4(-9.1f,1.0f,15f,1.0f),new Vector4(-8.7f,5.1f,-18f,.86f),
                new Vector4(9.2f,.7f,-13f,.94f),new Vector4(8.9f,5.0f,19f,.82f)})
                PlaceAuthored("GC FINAL · edge tree",tree,new Vector3(spec.x,.18f,spec.y),
                    1.45f*spec.w,3.45f*spec.w,spec.z,new Color(.48f,.58f,.43f,1f));

            // HERALDRY + LIGHT — cool slate/blue against warm gate and windows.
            Banner(new Vector3(-2.05f,6.78f,5.00f));
            Banner(new Vector3(2.05f,6.78f,5.00f));
            WarmPoint("GC FINAL · gate warmth",new Vector3(0f,4.15f,5.65f),new Color(1f,.53f,.23f),2.2f,5.0f);
            WarmPoint("GC FINAL · residence warmth",new Vector3(-7.2f,4.1f,1.15f),new Color(1f,.58f,.28f),.85f,3.1f);
            WarmPoint("GC FINAL · workshop warmth",new Vector3(7.0f,4.0f,.95f),new Color(1f,.58f,.25f),.75f,3.0f);

            // Directional balance local to the experiment only.
            var fillGo=new GameObject("GC FINAL · cool rim");
            fillGo.transform.SetParent(Root,true);
            fillGo.transform.rotation=Quaternion.Euler(38f,145f,0f);
            var fill=fillGo.AddComponent<Light>();
            fill.type=LightType.Directional;fill.shadows=LightShadows.None;
            fill.color=new Color(.58f,.68f,.82f);fill.intensity=.16f;
        }

        static void BuildMaterials()
        {
            Limestone=TexMat("Assets/Eldoria/ArtTests/OriginalHero/Textures/limestone.png",new Color(.72f,.69f,.61f),.03f,2.5f);
            DeepStone=TexMat("Assets/Eldoria/ArtTests/OriginalHero/Textures/oldstone.png",new Color(.48f,.47f,.43f),.025f,2.8f);
            Oak=TexMat("Assets/Eldoria/ArtTests/OriginalHero/Textures/oak.png",new Color(.50f,.34f,.21f),.035f,3.0f);
            Slate=TexMat("Assets/Eldoria/ArtTests/OriginalHero/Textures/slate.png",new Color(.24f,.29f,.34f),.04f,3.0f);
            Rock=TexMat("Assets/Eldoria/ArtTests/OriginalHero/Textures/rock.png",new Color(.39f,.40f,.38f),.02f,2.7f);
            Ground=TexMat("Assets/Eldoria/ArtTests/OriginalHero/Textures/oldstone.png",new Color(.52f,.47f,.38f),.025f,4.3f);
            Plaster=TexMat("Assets/Eldoria/ArtTests/OriginalHero/Textures/limestone.png",new Color(.64f,.58f,.49f),.02f,3.2f);
            Blue=Flat(new Color(.10f,.24f,.40f),.03f);
        }

        static Material TexMat(string assetPath,Color tint,float smooth,float tiling)
        {
            var shader=Shader.Find("Universal Render Pipeline/Lit")??Shader.Find("Standard");
            var m=new Material(shader){name="GoldenCell · "+Path.GetFileNameWithoutExtension(assetPath)};
            var tex=AssetDatabase.LoadAssetAtPath<Texture2D>(assetPath);
            if(m.HasProperty("_BaseColor"))m.SetColor("_BaseColor",tint); else m.color=tint;
            if(tex!=null)
            {
                if(m.HasProperty("_BaseMap")){m.SetTexture("_BaseMap",tex);m.SetTextureScale("_BaseMap",Vector2.one*tiling);}
                else if(m.HasProperty("_MainTex")){m.SetTexture("_MainTex",tex);m.SetTextureScale("_MainTex",Vector2.one*tiling);}
            }
            if(m.HasProperty("_Smoothness"))m.SetFloat("_Smoothness",smooth);
            if(m.HasProperty("_Metallic"))m.SetFloat("_Metallic",0f);
            return m;
        }

        static Material Flat(Color c,float smooth)
        {
            var shader=Shader.Find("Universal Render Pipeline/Lit")??Shader.Find("Standard");
            var m=new Material(shader);if(m.HasProperty("_BaseColor"))m.SetColor("_BaseColor",c);else m.color=c;
            if(m.HasProperty("_Smoothness"))m.SetFloat("_Smoothness",smooth);return m;
        }

        static void SuppressOldCellVisuals()
        {
            foreach(var r in UnityEngine.Object.FindObjectsByType<Renderer>(FindObjectsSortMode.None))
            {
                var p=r.bounds.center;
                if(Mathf.Abs(p.x)>10.8f||p.z<-1.8f||p.z>11.5f)continue;
                string n=r.gameObject.name;
                if(n.Contains("Hero")||n.Contains("Archer")||n.Contains("target")||n.Contains("worker"))continue;
                // Keep the route below the new stair authoritative but hide the competing old visual shell.
                if(n.StartsWith("Bastion")||n.StartsWith("Valoria · Bastion")||
                   n.StartsWith("VPD · upper")||n.StartsWith("VPD · L1")||
                   n.StartsWith("Valoria · rescued hero")||n.StartsWith("Valoria · stone street")||
                   n.StartsWith("VPD · GroundKit L1"))
                    r.enabled=false;
            }
        }

        static void Tower(string name,Vector3 ground,float width,float height,float depth)
        {
            Block(name+" · shaft",ground+new Vector3(0,height*.46f,0),new Vector3(width,height*.92f,depth),Limestone);
            Block(name+" · base",ground+new Vector3(0,.35f,0),new Vector3(width*1.12f,.70f,depth*1.10f),DeepStone);
            Battlements(name+" · crown",ground+new Vector3(0,height,0),width,0f);
            // vertical stone ribs
            foreach(float x in new[]{-width*.40f,width*.40f})
                Block(name+" · rib",ground+new Vector3(x,height*.47f,-depth*.51f),new Vector3(.22f,height*.76f,.16f),DeepStone);
            // arrow slit
            Block(name+" · slit",ground+new Vector3(0,height*.57f,-depth*.515f),new Vector3(.16f,.72f,.08f),Rock);
        }

        static void Keep(string name,Vector3 ground,float width,float height,float depth)
        {
            Block(name+" · body",ground+new Vector3(0,height*.48f,0),new Vector3(width,height*.96f,depth),Limestone);
            Block(name+" · lower course",ground+new Vector3(0,.48f,0),new Vector3(width*1.08f,.96f,depth*1.07f),DeepStone);
            Battlements(name+" · crown",ground+new Vector3(0,height,0),width,0f);
            for(int i=-1;i<=1;i++)
                Block(name+" · slit "+i,ground+new Vector3(i*1.05f,height*.60f,-depth*.51f),
                    new Vector3(.13f,.72f,.08f),Rock);
            var roof=Block(name+" · inner slate cap",ground+new Vector3(0,height+.38f,0),
                new Vector3(width*.66f,.62f,depth*.70f),Slate);
            roof.transform.rotation=Quaternion.Euler(0f,0f,2f);
        }

        static void Battlements(string name,Vector3 p,float span,float yaw)
        {
            for(int i=0;i<5;i++)
            {
                float x=Mathf.Lerp(-span*.46f,span*.46f,i/4f);
                var merlon=Block(name+" · merlon "+i,p+new Vector3(x,.32f,0f),
                    new Vector3(span*.13f,.64f,.62f),DeepStone);
                merlon.transform.rotation=Quaternion.Euler(0f,yaw,0f);
            }
        }

        static void AddGateSlats(Vector3 center,Vector3 size)
        {
            for(int i=0;i<7;i++)
            {
                float x=Mathf.Lerp(-size.x*.45f,size.x*.45f,i/6f);
                Block("GC FINAL · gate vertical "+i,center+new Vector3(x,0,-.12f),
                    new Vector3(.075f,size.y,.08f),DeepStone);
            }
            for(int i=0;i<4;i++)
            {
                float y=Mathf.Lerp(-size.y*.40f,size.y*.40f,i/3f);
                Block("GC FINAL · gate horizontal "+i,center+new Vector3(0,y,-.13f),
                    new Vector3(size.x,.075f,.08f),DeepStone);
            }
        }

        static void Buttress(Vector3 foot,float yaw)
        {
            var lower=Block("GC FINAL · buttress lower",foot+new Vector3(0,.75f,0),
                new Vector3(.90f,1.55f,1.15f),DeepStone);
            lower.transform.rotation=Quaternion.Euler(-5f,yaw,0f);
            var upper=Block("GC FINAL · buttress upper",foot+new Vector3(0,1.75f,.12f),
                new Vector3(.62f,1.15f,.88f),Limestone);
            upper.transform.rotation=Quaternion.Euler(-3f,yaw,0f);
        }

        static void RockMass(string name,Vector3 p,Vector3 scale,float zRot)
        {
            var a=Block(name+" · A",p,scale,Rock);a.transform.rotation=Quaternion.Euler(0f,-9f,zRot);
            var b=Block(name+" · B",p+new Vector3(scale.x*.30f,.30f,scale.z*.18f),
                scale*.67f,DeepStone);b.transform.rotation=Quaternion.Euler(7f,17f,-zRot*.6f);
        }

        static void Rubble(Vector3 p,int count,float scale)
        {
            for(int i=0;i<count;i++)
            {
                float a=i*2.39996f;
                float r=.25f+.13f*i;
                var s=scale*(.65f+(i%3)*.16f);
                var go=GameObject.CreatePrimitive(PrimitiveType.Sphere);
                go.name="GC FINAL · rubble";go.transform.SetParent(Root,true);
                go.transform.position=p+new Vector3(Mathf.Cos(a)*r,s*.42f,Mathf.Sin(a)*r);
                go.transform.localScale=new Vector3(s*1.3f,s*.65f,s);
                go.transform.rotation=Quaternion.Euler(i*11f,i*37f,i*7f);
                var col=go.GetComponent<Collider>();if(col!=null)UnityEngine.Object.DestroyImmediate(col);
                go.GetComponent<Renderer>().sharedMaterial=i%2==0?Rock:DeepStone;
            }
        }

        static GameObject PlaceAuthored(string name,GameObject prefab,Vector3 ground,float footprint,float height,float yaw,Color tint)
        {
            if(prefab==null)return null;
            var go=ValoriaKit.BenchmarkPieceModulated(name,prefab,ground,footprint,height,Quaternion.Euler(0f,yaw,0f),tint);
            if(go==null)return null;
            go.transform.SetParent(Root,true);
            foreach(var col in go.GetComponentsInChildren<Collider>(true))col.enabled=false;
            return go;
        }

        static void Banner(Vector3 p)
        {
            Block("GC FINAL · banner",p,new Vector3(.68f,1.70f,.06f),Blue);
            Block("GC FINAL · banner pole",p+new Vector3(-.42f,.25f,0f),new Vector3(.055f,2.25f,.055f),Oak);
        }

        static void WarmPoint(string name,Vector3 p,Color c,float intensity,float range)
        {
            var go=new GameObject(name);go.transform.SetParent(Root,true);go.transform.position=p;
            var l=go.AddComponent<Light>();l.type=LightType.Point;l.color=c;l.intensity=intensity;l.range=range;
            l.shadows=LightShadows.Soft;
        }

        static GameObject Block(string name,Vector3 p,Vector3 scale,Material m)
        {
            var go=GameObject.CreatePrimitive(PrimitiveType.Cube);go.name=name;go.transform.SetParent(Root,true);
            go.transform.position=p;go.transform.localScale=scale;
            var c=go.GetComponent<Collider>();if(c!=null)UnityEngine.Object.DestroyImmediate(c);
            go.GetComponent<Renderer>().sharedMaterial=m;return go;
        }
    }
}
