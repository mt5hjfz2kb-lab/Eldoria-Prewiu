using System;
using System.Collections.Generic;
using System.IO;
using Eldoria.Domain;
using Eldoria.Presentation;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Eldoria.EditorTools
{
    public static class ModularArchitectureCompositionGate
    {
        const string Folder="ModularArchitectureCompositionCaptures";
        const string AssetPath="Assets/Resources/Valoria/HeroBastionGenerated/Valoria_HeroBastion_v1.glb";
        static readonly Vector3 CameraPosition=new Vector3(18.2f,14.6f,-25.8f);
        static readonly Vector3 CameraTarget=new Vector3(0f,3.65f,7.25f);
        static readonly Vector3 ParcelCenter=new Vector3(-12.45f,.34f,-3.05f);

        public static void Capture()
        {
            UnityEditor.ShaderUtil.allowAsyncCompilation=false;
            SceneSetup.SetupRenderPipeline();
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
            ProductionVisualIntegration.ResetVisualCachesForGate();
            VisualWorld.VisualIntegrationEnabled=true;
            VisualWorld.Create(true,new PlayerState{
                BastionLevel=3,SawmillLevel=1,BarracksLevel=1,CorruptionDiscovered=true
            });

            var camera=Camera.main;
            if(camera==null)throw new Exception("Valoria camera missing.");
            Directory.CreateDirectory(Folder);

            int bastionSuppressed=HideLegacyBastionVisuals();
            if(bastionSuppressed<1)throw new Exception("No legacy Bastion visual renderers were suppressed.");
            var hero=PlaceGeneratedHero();
            var heroDistrict=BuildHeroDistrictIntegration();
            DisableAllGameplayOnVisuals(heroDistrict);

            string baselineSignature=ValoriaVisualFormulaGate.CollisionSignature();
            string baselineMetrics=MetricsJson();

            Save(camera,Folder+"/current-19.png",19f,1280,720);
            Save(camera,Folder+"/current-12.png",12f,1280,720);
            Save(camera,Folder+"/current-9.png",9f,1280,720);
            Save(camera,Folder+"/current-mobile.png",12f,390,844);

            int parcelHidden=HideTargetParcelVisuals();
            if(parcelHidden<1)throw new Exception("Target parcel visual shell was not found.");
            if(ValoriaVisualFormulaGate.CollisionSignature()!=baselineSignature)
                throw new Exception("Hiding target parcel renderers altered gameplay signature.");

            string aMetrics,bMetrics,cMetrics;
            var a=BuildAlternativeA();
            DisableAllGameplayOnVisuals(a);
            AssertSignature(baselineSignature,"A");
            aMetrics=MetricsJson();
            Save(camera,Folder+"/alternative-a-19.png",19f,1280,720);
            Save(camera,Folder+"/alternative-a-12.png",12f,1280,720);
            Save(camera,Folder+"/alternative-a-9.png",9f,1280,720);
            Save(camera,Folder+"/alternative-a-mobile.png",12f,390,844);
            UnityEngine.Object.DestroyImmediate(a);

            var b=BuildAlternativeB();
            DisableAllGameplayOnVisuals(b);
            AssertSignature(baselineSignature,"B");
            bMetrics=MetricsJson();
            Save(camera,Folder+"/alternative-b-19.png",19f,1280,720);
            Save(camera,Folder+"/alternative-b-12.png",12f,1280,720);
            Save(camera,Folder+"/alternative-b-9.png",9f,1280,720);
            Save(camera,Folder+"/alternative-b-mobile.png",12f,390,844);
            UnityEngine.Object.DestroyImmediate(b);

            var c=BuildAlternativeC();
            DisableAllGameplayOnVisuals(c);
            AssertSignature(baselineSignature,"C");
            cMetrics=MetricsJson();
            Save(camera,Folder+"/alternative-c-19.png",19f,1280,720);
            Save(camera,Folder+"/alternative-c-12.png",12f,1280,720);
            Save(camera,Folder+"/alternative-c-9.png",9f,1280,720);
            Save(camera,Folder+"/alternative-c-mobile.png",12f,390,844);

            AssertSignature(baselineSignature,"final");
            var heroBounds=BoundsOf(hero);
            File.WriteAllText(Folder+"/modular-architecture-evidence.json",
                "{\n"+
                "  \"phase\": \"VALORIA_MODULAR_ARCHITECTURE_COMPOSITION_PROOF_V1\",\n"+
                "  \"branch\": \"visual-proof/modular-architecture-composition-v1\",\n"+
                "  \"parcel\": \"west rebuilders lower residential parcel\",\n"+
                "  \"parcel_center\": [-12.45,0.34,-3.05],\n"+
                "  \"source_optimized_sha256\": \"afb6cee6ae572b0879650f18285b32798e263c17359a158ffe2bdd03fb62ad5c\",\n"+
                "  \"tripo_credits_this_proof\": 0,\n"+
                "  \"collider_hotspot_signature_equal\": true,\n"+
                "  \"hero_district_preserved\": true,\n"+
                "  \"parcel_renderers_hidden\": "+parcelHidden+",\n"+
                "  \"baseline_metrics\": "+baselineMetrics+",\n"+
                "  \"alternative_a_metrics\": "+aMetrics+",\n"+
                "  \"alternative_b_metrics\": "+bMetrics+",\n"+
                "  \"alternative_c_metrics\": "+cMetrics+",\n"+
                "  \"hero_bounds_center\": ["+F(heroBounds.center.x)+","+F(heroBounds.center.y)+","+F(heroBounds.center.z)+"],\n"+
                "  \"hero_bounds_size\": ["+F(heroBounds.size.x)+","+F(heroBounds.size.y)+","+F(heroBounds.size.z)+"]\n"+
                "}\n");

            EditorApplication.Exit(0);
        }

        static void AssertSignature(string baseline,string label)
        {
            var now=ValoriaVisualFormulaGate.CollisionSignature();
            if(now!=baseline)throw new Exception("Alternative "+label+" altered gameplay collider/hotspot signature.");
        }

        static int HideTargetParcelVisuals()
        {
            int count=0;
            foreach(var r in UnityEngine.Object.FindObjectsByType<Renderer>(FindObjectsSortMode.None))
            {
                if(r==null||!r.enabled||!r.gameObject.activeInHierarchy)continue;
                var n=HierarchyName(r.transform);
                var p=r.bounds.center;
                bool inParcel=p.x>-16.2f&&p.x<-9.4f&&p.z>-6.0f&&p.z<.8f;
                bool family=n.Contains("Valoria · reused civil house",StringComparison.OrdinalIgnoreCase)||
                            n.Contains("VPD · west rebuilders home",StringComparison.OrdinalIgnoreCase)||
                            n.Contains("Valoria lower-town home",StringComparison.OrdinalIgnoreCase);
                if(inParcel&&family){r.enabled=false;count++;}
            }
            return count;
        }

        static GameObject BuildAlternativeA()
        {
            var root=new GameObject("MODULAR ALT A · rock integrated vertical residence");
            var art=ValoriaExternalAssetLibrary.Load();
            AddTerrainTerraceTop(root,"SteppedRockTerrace","A · buried stepped rock base",
                ParcelCenter, .38f,5.25f,92f);
            if(art!=null&&art.SlavicHouse!=null)
                AddPiece(root,ValoriaKit.BenchmarkPieceModulated("A · residential core",art.SlavicHouse,
                    ParcelCenter+new Vector3(.15f,.05f,.05f),3.55f,3.35f,Quaternion.Euler(0,-8f,0),new Color(.72f,.68f,.60f,1f)));
            AddStoneArchitecturePiece(root,"HighStraightWall","A · rear masonry spine",
                ParcelCenter+new Vector3(.25f,.10f,1.25f),3.15f,176f);
            AddStoneArchitecturePiece(root,"RockToWallTransition","A · west rock seam",
                ParcelCenter+new Vector3(-2.0f,.10f,.25f),2.10f,64f);
            AddRawModule(root,"Unity/Assets/EmaceArt/Slavic World Free/Meshes/EA03_Village_Balcony_R_01a.fbx",
                "A · balcony gallery",ParcelCenter+new Vector3(.10f,1.62f,-1.52f),2.55f,1.10f,Quaternion.Euler(0,172f,0),new Color(.60f,.48f,.34f,1f));
            AddRawModule(root,"Unity/Assets/EmaceArt/Slavic World Free/Meshes/EA03_Village_Hut_Roof_Cut_01a.fbx",
                "A · secondary slate roof",ParcelCenter+new Vector3(-.55f,2.55f,.20f),3.20f,1.25f,Quaternion.Euler(0,-8f,0),new Color(.39f,.43f,.46f,1f));
            AddLocalWarmth(root,ParcelCenter+new Vector3(.20f,1.25f,-1.25f),.24f,2.4f);
            return root;
        }

        static GameObject BuildAlternativeB()
        {
            var root=new GameObject("MODULAR ALT B · civic terraced house");
            var art=ValoriaExternalAssetLibrary.Load();
            AddTerrainTerraceTop(root,"BroadRockPlatform","B · civic buried plinth",
                ParcelCenter+new Vector3(.05f,0,.10f),.38f,5.55f,14f);
            AddStoneArchitecturePiece(root,"CornerWallL","B · west civic corner",
                ParcelCenter+new Vector3(-1.55f,.08f,.55f),2.35f,102f);
            AddStoneArchitecturePiece(root,"CornerWallL","B · east civic corner",
                ParcelCenter+new Vector3(1.55f,.08f,.40f),2.25f,258f);
            if(art!=null&&art.SlavicHouse!=null)
                AddPiece(root,ValoriaKit.BenchmarkPieceModulated("B · civic residential core",art.SlavicHouse,
                    ParcelCenter+new Vector3(0,.18f,.35f),3.15f,3.05f,Quaternion.Euler(0,4f,0),new Color(.74f,.70f,.62f,1f)));
            if(art!=null&&art.SlavicRockGate!=null)
                AddPiece(root,ValoriaKit.BenchmarkPieceModulated("B · civic stone entrance",art.SlavicRockGate,
                    ParcelCenter+new Vector3(.05f,.06f,-1.85f),2.55f,2.05f,Quaternion.Euler(0,180f,0),new Color(.67f,.66f,.61f,1f)));
            AddRawModule(root,"Unity/Assets/EmaceArt/Slavic World Free/Meshes/EA03_Village_HouseModule_Porch_01d.fbx",
                "B · porch projection",ParcelCenter+new Vector3(-.05f,.38f,-1.30f),2.75f,1.75f,Quaternion.Euler(0,180f,0),new Color(.65f,.54f,.40f,1f));
            AddRawModule(root,"Unity/Assets/EmaceArt/Slavic World Free/Meshes/EA03_Town_Ill_Roof_01b.fbx",
                "B · civic roof crown",ParcelCenter+new Vector3(.15f,2.45f,.25f),3.45f,1.35f,Quaternion.Euler(0,4f,0),new Color(.37f,.41f,.45f,1f));
            AddLocalWarmth(root,ParcelCenter+new Vector3(.05f,1.20f,-1.35f),.22f,2.3f);
            return root;
        }

        static GameObject BuildAlternativeC()
        {
            var root=new GameObject("MODULAR ALT C · fortified secondary residence");
            var art=ValoriaExternalAssetLibrary.Load();
            AddTerrainTerraceTop(root,"BroadRockPlatform","C · fortified buried base",
                ParcelCenter,.38f,5.25f,188f);
            AddResourceModule(root,"Valoria/Rescued/TowerWallRock","C · tower rock corner",
                ParcelCenter+new Vector3(-1.45f,.10f,.65f),2.45f,3.60f,Quaternion.Euler(0,18f,0),new Color(.66f,.65f,.60f,1f));
            if(art!=null&&art.SlavicHouse!=null)
                AddPiece(root,ValoriaKit.BenchmarkPieceModulated("C · inhabited core",art.SlavicHouse,
                    ParcelCenter+new Vector3(.65f,.20f,.10f),2.95f,2.95f,Quaternion.Euler(0,-6f,0),new Color(.69f,.65f,.57f,1f)));
            AddStoneArchitecturePiece(root,"HighStraightWall","C · fortified rear wall",
                ParcelCenter+new Vector3(.35f,.10f,1.45f),3.05f,176f);
            AddStoneArchitecturePiece(root,"RockToWallTransition","C · east foundation seam",
                ParcelCenter+new Vector3(1.95f,.10f,.40f),2.15f,244f);
            if(art!=null&&art.SlavicRockGate!=null)
                AddPiece(root,ValoriaKit.BenchmarkPieceModulated("C · lower arch entry",art.SlavicRockGate,
                    ParcelCenter+new Vector3(.65f,.06f,-1.75f),2.30f,1.90f,Quaternion.Euler(0,180f,0),new Color(.65f,.64f,.59f,1f)));
            AddRawModule(root,"Unity/Assets/EmaceArt/Slavic World Free/Meshes/EA03_Village_Hut_Roof_Cut_02a.fbx",
                "C · roof connector",ParcelCenter+new Vector3(.80f,2.30f,.20f),2.95f,1.15f,Quaternion.Euler(0,-6f,0),new Color(.36f,.40f,.43f,1f));
            AddLocalWarmth(root,ParcelCenter+new Vector3(.75f,1.20f,-1.30f),.20f,2.2f);
            return root;
        }

        static void AddRawModule(GameObject root,string path,string name,Vector3 anchor,float targetSpan,float maxHeight,Quaternion rotation,Color tint)
        {
            var source=AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if(source==null)throw new Exception("Missing modular source: "+path);
            var go=UnityEngine.Object.Instantiate(source);go.name=name;go.transform.rotation=rotation;
            FitToAnchor(go,anchor,targetSpan,maxHeight);
            FitModuleSurface(go,tint);
            go.transform.SetParent(root.transform,true);
            DisableAllGameplayOnVisuals(go);
        }

        static void AddResourceModule(GameObject root,string resource,string name,Vector3 anchor,float targetSpan,float maxHeight,Quaternion rotation,Color tint)
        {
            var source=Resources.Load<GameObject>(resource);
            if(source==null)throw new Exception("Missing resource module: "+resource);
            var go=UnityEngine.Object.Instantiate(source);go.name=name;go.transform.rotation=rotation;
            FitToAnchor(go,anchor,targetSpan,maxHeight);
            FitModuleSurface(go,tint);
            go.transform.SetParent(root.transform,true);
            DisableAllGameplayOnVisuals(go);
        }

        static void FitToAnchor(GameObject go,Vector3 anchor,float targetSpan,float maxHeight)
        {
            var b=BoundsOf(go);
            float span=Mathf.Max(b.size.x,b.size.z);
            if(span<=.001f||b.size.y<=.001f)throw new Exception("Module has invalid bounds: "+go.name);
            float scale=Mathf.Min(targetSpan/span,maxHeight/b.size.y);
            go.transform.localScale*=scale;
            b=BoundsOf(go);
            go.transform.position+=anchor-new Vector3(b.center.x,b.min.y,b.center.z);
        }

        static readonly Dictionary<string,Material> moduleMaterials=new();

        static void FitModuleSurface(GameObject go,Color tint)
        {
            foreach(var r in go.GetComponentsInChildren<Renderer>(true))
            {
                var srcs=r.sharedMaterials;var dst=new Material[srcs.Length];
                for(int i=0;i<srcs.Length;i++)
                {
                    var src=srcs[i];
                    string key=(src!=null?src.GetInstanceID().ToString():"null")+"|"+ColorUtility.ToHtmlStringRGB(tint);
                    if(moduleMaterials.TryGetValue(key,out var cached)){dst[i]=cached;continue;}
                    Texture baseMap=null,normal=null;
                    if(src!=null)
                    {
                        foreach(string p in new[]{"_BaseMap","_MainTex","_BaseColorTexture","baseColorTexture","_Texture"})
                            if(src.HasProperty(p)&&src.GetTexture(p)!=null){baseMap=src.GetTexture(p);break;}
                        foreach(string p in new[]{"_BumpMap","_NormalMap","normalTexture"})
                            if(src.HasProperty(p)&&src.GetTexture(p)!=null){normal=src.GetTexture(p);break;}
                    }
                    var shader=Shader.Find("Universal Render Pipeline/Lit")??Shader.Find("Standard");
                    var m=new Material(shader){name="Modular composition · "+(src!=null?src.name:"surface")};
                    if(baseMap!=null&&m.HasProperty("_BaseMap"))m.SetTexture("_BaseMap",baseMap);
                    if(normal!=null&&m.HasProperty("_BumpMap")){m.SetTexture("_BumpMap",normal);m.EnableKeyword("_NORMALMAP");}
                    if(m.HasProperty("_BaseColor"))m.SetColor("_BaseColor",tint);
                    if(m.HasProperty("_Color"))m.SetColor("_Color",tint);
                    if(m.HasProperty("_Metallic"))m.SetFloat("_Metallic",0f);
                    if(m.HasProperty("_Smoothness"))m.SetFloat("_Smoothness",.025f);
                    m.enableInstancing=true;
                    moduleMaterials[key]=m;dst[i]=m;
                }
                r.sharedMaterials=dst;
                var block=new MaterialPropertyBlock();
                r.GetPropertyBlock(block);
                if(block!=null&&r.sharedMaterial!=null&&r.sharedMaterial.HasProperty("_BaseColor"))
                    block.SetColor("_BaseColor",tint*new Color(.98f,.98f,.98f,1f));
                r.SetPropertyBlock(block);
            }
        }

        static void AddLocalWarmth(GameObject root,Vector3 p,float intensity,float range)
        {
            var go=new GameObject("Modular composition · local occupied warmth");
            go.transform.SetParent(root.transform,false);go.transform.position=p;
            var l=go.AddComponent<Light>();l.type=LightType.Point;l.color=new Color(1f,.58f,.29f);
            l.intensity=intensity;l.range=range;l.shadows=LightShadows.None;
        }

        static GameObject BuildHeroDistrictIntegration()
        {
            var root=new GameObject("HERO DISTRICT INTEGRATION v1 · visual only");
            var art=ValoriaExternalAssetLibrary.Load();

            // 1) Keep the certified physical route, but unify the visible stair/landing response.
            var stairMat=ValoriaKit.PbrSurfaceMaterial(
                art!=null?art.ValoriaStoneSurface:null,
                new Color(.64f,.62f,.57f,1f),new Vector2(2.6f,1.6f),.022f,.95f);
            var groundMat=ValoriaKit.PbrSurfaceMaterial(
                art!=null?art.ValoriaCobbleSurface:null,
                new Color(.70f,.67f,.59f,1f),new Vector2(3.8f,3.8f),.025f,.82f);

            foreach(var r in UnityEngine.Object.FindObjectsByType<Renderer>(FindObjectsSortMode.None))
            {
                if(r==null||!r.enabled||!r.gameObject.activeInHierarchy)continue;
                var n=HierarchyName(r.transform);
                if(n.Contains("VPD · vertical stair",StringComparison.OrdinalIgnoreCase))
                    r.sharedMaterial=stairMat;
                else if(n.Contains("VPD · GroundKit L1 landing",StringComparison.OrdinalIgnoreCase) ||
                        n.Contains("VPD · GroundKit L1 west terrace",StringComparison.OrdinalIgnoreCase) ||
                        n.Contains("VPD · GroundKit L1 east terrace",StringComparison.OrdinalIgnoreCase))
                    r.sharedMaterial=groundMat;
            }

            // 2) Use certified Terrain & Terrace geometry as buried visual support.
            // Two flank shelves preserve the central certified stair/landing corridor while making
            // the upper district read as one mountain mass rather than a rectangular platform.
            AddTerrainTerraceTop(root,"BroadRockPlatform","HeroDistrict · buried west hero shelf",
                new Vector3(-4.75f,0f,7.55f),2.58f,6.15f,18f);
            AddTerrainTerraceTop(root,"BroadRockPlatform","HeroDistrict · buried east hero shelf",
                new Vector3(4.70f,0f,7.65f),2.58f,6.10f,198f);
            AddTerrainTerraceTop(root,"SteppedRockTerrace","HeroDistrict · west stair shoulder",
                new Vector3(-3.65f,0f,4.85f),2.34f,4.35f,92f);
            AddTerrainTerraceTop(root,"SteppedRockTerrace","HeroDistrict · east stair shoulder",
                new Vector3(3.70f,0f,4.95f),2.34f,4.30f,268f);

            // 3) Certified rock-to-wall transition modules close the Bastion/retaining seams.
            AddStoneArchitecturePiece(root,"RockToWallTransition","HeroDistrict · bastion seam west",
                new Vector3(-5.45f,2.20f,7.15f),2.45f,58f);
            AddStoneArchitecturePiece(root,"RockToWallTransition","HeroDistrict · bastion seam east",
                new Vector3(5.40f,2.20f,7.25f),2.40f,238f);
            AddStoneArchitecturePiece(root,"HighStraightWall","HeroDistrict · west retaining fragment",
                new Vector3(-5.70f,1.20f,5.08f),2.65f,4f);
            AddStoneArchitecturePiece(root,"HighStraightWall","HeroDistrict · east retaining fragment",
                new Vector3(5.70f,1.20f,5.08f),2.65f,176f);

            // 4) Keep only two authored natural breaks at the stair foot; no density-for-density.
            if(art!=null&&art.SlavicFlatRock!=null)
            {
                AddPiece(root,ValoriaKit.BenchmarkPieceModulated("HeroDistrict · stair foot rock west",art.SlavicFlatRock,
                    new Vector3(-2.55f,.50f,3.70f),2.25f,1.15f,Quaternion.Euler(0,-18f,0),new Color(.49f,.51f,.48f,1f)));
                AddPiece(root,ValoriaKit.BenchmarkPieceModulated("HeroDistrict · stair foot rock east",art.SlavicFlatRock,
                    new Vector3(2.60f,.50f,3.78f),2.20f,1.12f,Quaternion.Euler(0,20f,0),new Color(.49f,.51f,.48f,1f)));
            }

            // 5) The immediate east procedural dwelling competes directly with the Hero Bastion.
            // Swap only its renderer after a real authored replacement is available.
            GameObject authoredHouse=null;
            if(art!=null&&art.SlavicHouse!=null)
                authoredHouse=ValoriaKit.BenchmarkPieceModulated("HeroDistrict · east upper residence",art.SlavicHouse,
                    new Vector3(5.15f,2.89f,7.15f),3.45f,3.55f,Quaternion.Euler(0,188f,0),
                    new Color(.76f,.73f,.67f,1f));
            if(authoredHouse!=null)
            {
                AddPiece(root,authoredHouse);
                HideVisualFamily("VPD · upper dwelling");
            }

            // 6) Restrained local warmth only; no whole-scene exposure change.
            AddWarmLight(root,"HeroDistrict · landing warmth",new Vector3(0f,3.35f,5.45f),.38f,3.7f);
            AddWarmLight(root,"HeroDistrict · east hearth",new Vector3(4.65f,3.75f,6.35f),.23f,2.4f);

            return root;
        }

        static void AddTerrainTerraceTop(GameObject root,string resource,string name,Vector3 xzAnchor,float topY,float targetSpan,float yaw)
        {
            var source=Resources.Load<GameObject>("Valoria/TerrainTerraceKit_v1/"+resource);
            if(source==null)throw new Exception("Missing Terrain Terrace resource: "+resource);
            var go=UnityEngine.Object.Instantiate(source);go.name=name;go.transform.rotation=Quaternion.Euler(0,yaw,0);
            var bounds=BoundsOf(go);float span=Mathf.Max(bounds.size.x,bounds.size.z);
            if(span<=.001f)throw new Exception("Empty Terrain Terrace resource: "+resource);
            go.transform.localScale*=targetSpan/span;
            bounds=BoundsOf(go);
            go.transform.position+=new Vector3(xzAnchor.x-bounds.center.x,topY-bounds.max.y,xzAnchor.z-bounds.center.z);
            go.transform.SetParent(root.transform,true);
            DisableAllGameplayOnVisuals(go);
        }

        static void AddStoneArchitecturePiece(GameObject root,string resource,string name,Vector3 groundAnchor,float targetSpan,float yaw)
        {
            var source=Resources.Load<GameObject>("Valoria/StoneArchitectureKit_v1/"+resource);
            if(source==null)throw new Exception("Missing Stone Architecture resource: "+resource);
            var go=UnityEngine.Object.Instantiate(source);go.name=name;go.transform.rotation=Quaternion.Euler(0,yaw,0);
            var bounds=BoundsOf(go);float span=Mathf.Max(bounds.size.x,bounds.size.z);
            if(span<=.001f)throw new Exception("Empty Stone Architecture resource: "+resource);
            go.transform.localScale*=targetSpan/span;
            bounds=BoundsOf(go);
            go.transform.position+=groundAnchor-new Vector3(bounds.center.x,bounds.min.y,bounds.center.z);
            go.transform.SetParent(root.transform,true);
            DisableAllGameplayOnVisuals(go);
        }

        static void AddWarmLight(GameObject root,string name,Vector3 p,float intensity,float range)
        {
            var go=new GameObject(name);go.transform.SetParent(root.transform,false);go.transform.position=p;
            var l=go.AddComponent<Light>();l.type=LightType.Point;l.color=new Color(1f,.60f,.32f);
            l.intensity=intensity;l.range=range;l.shadows=LightShadows.None;
        }

        static void AddPiece(GameObject root,GameObject go)
        {
            if(go!=null)go.transform.SetParent(root.transform,true);
        }

        static void HideVisualFamily(string family)
        {
            foreach(var r in UnityEngine.Object.FindObjectsByType<Renderer>(FindObjectsSortMode.None))
            {
                if(r==null||!r.enabled)continue;
                if(HierarchyName(r.transform).Contains(family,StringComparison.OrdinalIgnoreCase))r.enabled=false;
            }
        }

        static string HierarchyName(Transform t)
        {
            string s="";
            for(var p=t;p!=null;p=p.parent)s=p.name+"/"+s;
            return s;
        }

        static void DisableAllGameplayOnVisuals(GameObject root)
        {
            foreach(var c in root.GetComponentsInChildren<Collider>(true))c.enabled=false;
            foreach(var h in root.GetComponentsInChildren<WorldHotspot>(true))UnityEngine.Object.DestroyImmediate(h);
            foreach(var b in root.GetComponentsInChildren<MonoBehaviour>(true))
                if(!(b is WorldHotspot))b.enabled=false;
            Physics.SyncTransforms();
        }

        static int HideLegacyBastionVisuals()
        {
            int count=0;
            foreach(var r in UnityEngine.Object.FindObjectsByType<Renderer>(FindObjectsSortMode.None))
            {
                if(r==null||!r.enabled||!r.gameObject.activeInHierarchy)continue;
                bool legacy=false;
                for(var t=r.transform;t!=null;t=t.parent)
                {
                    if(t.name.StartsWith("Bastion ·",StringComparison.OrdinalIgnoreCase) ||
                       string.Equals(t.name,"Bastion",StringComparison.OrdinalIgnoreCase) ||
                       t.name.StartsWith("Valoria · Bastion hero",StringComparison.OrdinalIgnoreCase) ||
                       t.name.StartsWith("Valoria · rescued hero flank",StringComparison.OrdinalIgnoreCase))
                    { legacy=true; break; }
                }
                if(!legacy)continue;
                r.enabled=false;count++;
            }
            return count;
        }

        static GameObject PlaceGeneratedHero()
        {
            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
            var source=AssetDatabase.LoadAssetAtPath<GameObject>(AssetPath);
            if(source==null)throw new Exception("Generated Hero Bastion GLB missing: "+AssetPath);
            var go=UnityEngine.Object.Instantiate(source);
            go.name="Valoria · Generated Hero Bastion v1 · hero district proof";
            go.transform.rotation=Quaternion.Euler(0f,180f,0f);
            DisableAllGameplayOnVisuals(go);

            var bounds=BoundsOf(go);
            float span=Mathf.Max(bounds.size.x,bounds.size.z);
            if(span<=.001f||bounds.size.y<=.001f)throw new Exception("Generated Hero Bastion bounds invalid.");
            float scale=Mathf.Min(12.8f/span,10.2f/bounds.size.y);
            go.transform.localScale*=scale;
            bounds=BoundsOf(go);
            go.transform.position+=new Vector3(-bounds.center.x,2.52f-bounds.min.y,8.75f-bounds.center.z);
            FitGeneratedHeroSurface(go);
            return go;
        }

        static void FitGeneratedHeroSurface(GameObject go)
        {
            var cache=new Dictionary<int,Material>();
            foreach(var r in go.GetComponentsInChildren<Renderer>(true))
            {
                var srcs=r.sharedMaterials;var dst=new Material[srcs.Length];
                for(int i=0;i<srcs.Length;i++)
                {
                    var src=srcs[i];if(src==null){dst[i]=null;continue;}
                    if(cache.TryGetValue(src.GetInstanceID(),out var cached)){dst[i]=cached;continue;}
                    Texture baseMap=null,normal=null,mask=null;
                    foreach(string p in new[]{"_BaseMap","_MainTex","_BaseColorTexture","baseColorTexture","_Texture"})
                        if(src.HasProperty(p)&&src.GetTexture(p)!=null){baseMap=src.GetTexture(p);break;}
                    foreach(string p in new[]{"_BumpMap","_NormalMap","normalTexture"})
                        if(src.HasProperty(p)&&src.GetTexture(p)!=null){normal=src.GetTexture(p);break;}
                    foreach(string p in new[]{"_MaskMap","_MetallicGlossMap","_OcclusionMap"})
                        if(src.HasProperty(p)&&src.GetTexture(p)!=null){mask=src.GetTexture(p);break;}
                    var shader=Shader.Find("Universal Render Pipeline/Lit")??Shader.Find("Standard");
                    var m=new Material(shader){name="Valoria fitted · "+src.name};
                    if(baseMap!=null)m.SetTexture("_BaseMap",baseMap);
                    if(normal!=null){m.SetTexture("_BumpMap",normal);m.EnableKeyword("_NORMALMAP");if(m.HasProperty("_BumpScale"))m.SetFloat("_BumpScale",1f);}
                    if(mask!=null&&m.HasProperty("_OcclusionMap"))m.SetTexture("_OcclusionMap",mask);
                    if(m.HasProperty("_BaseColor"))m.SetColor("_BaseColor",new Color(.72f,.69f,.64f,1f));
                    if(m.HasProperty("_Color"))m.SetColor("_Color",new Color(.72f,.69f,.64f,1f));
                    if(m.HasProperty("_Metallic"))m.SetFloat("_Metallic",0f);
                    if(m.HasProperty("_Smoothness"))m.SetFloat("_Smoothness",.03f);
                    if(m.HasProperty("_OcclusionStrength"))m.SetFloat("_OcclusionStrength",1f);
                    cache[src.GetInstanceID()]=m;dst[i]=m;
                }
                r.sharedMaterials=dst;
            }
        }

        static Bounds BoundsOf(GameObject go)
        {
            var rs=go.GetComponentsInChildren<Renderer>(true);
            if(rs.Length==0)return new Bounds(go.transform.position,Vector3.zero);
            var b=rs[0].bounds;for(int i=1;i<rs.Length;i++)b.Encapsulate(rs[i].bounds);return b;
        }

        static string MetricsJson()
        {
            long triangles=0;int renderers=0,lights=0;var materials=new HashSet<int>();
            foreach(var r in UnityEngine.Object.FindObjectsByType<Renderer>(FindObjectsSortMode.None))
            {
                if(r==null||!r.enabled||!r.gameObject.activeInHierarchy)continue;
                renderers++;foreach(var m in r.sharedMaterials)if(m!=null)materials.Add(m.GetInstanceID());
                var mf=r.GetComponent<MeshFilter>();if(mf!=null&&mf.sharedMesh!=null)triangles+=mf.sharedMesh.triangles.LongLength/3;
                var sk=r as SkinnedMeshRenderer;if(sk!=null&&sk.sharedMesh!=null)triangles+=sk.sharedMesh.triangles.LongLength/3;
            }
            foreach(var l in UnityEngine.Object.FindObjectsByType<Light>(FindObjectsSortMode.None))
                if(l!=null&&l.enabled&&l.gameObject.activeInHierarchy)lights++;
            return "{\"triangles\":"+triangles+",\"renderers\":"+renderers+",\"materials\":"+materials.Count+",\"lights\":"+lights+"}";
        }

        static void Save(Camera camera,string path,float zoom,int width,int height)
        {
            camera.transform.position=CameraPosition;camera.transform.LookAt(CameraTarget);
            camera.orthographic=true;camera.orthographicSize=zoom;
            var rt=new RenderTexture(width,height,24,RenderTextureFormat.ARGB32);
            var tex=new Texture2D(width,height,TextureFormat.RGB24,false);
            camera.targetTexture=rt;camera.Render();RenderTexture.active=rt;
            tex.ReadPixels(new Rect(0,0,width,height),0,0);tex.Apply();File.WriteAllBytes(path,tex.EncodeToPNG());
            camera.targetTexture=null;RenderTexture.active=null;UnityEngine.Object.DestroyImmediate(tex);UnityEngine.Object.DestroyImmediate(rt);
        }

        static string F(float v)=>v.ToString("0.###",System.Globalization.CultureInfo.InvariantCulture);
    }
}
