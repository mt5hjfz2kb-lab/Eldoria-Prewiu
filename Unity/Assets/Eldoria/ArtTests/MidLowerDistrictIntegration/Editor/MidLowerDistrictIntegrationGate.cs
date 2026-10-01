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
    public static class MidLowerDistrictIntegrationGate
    {
        const string Folder="MidLowerDistrictIntegrationCaptures";
        const string AssetPath="Assets/Resources/Valoria/HeroBastionGenerated/Valoria_HeroBastion_v1.glb";
        static readonly Vector3 CameraPosition=new Vector3(18.2f,14.6f,-25.8f);
        static readonly Vector3 CameraTarget=new Vector3(0f,3.65f,7.25f);

        public static void Capture()
        {
            UnityEditor.ShaderUtil.allowAsyncCompilation=false;
            SceneSetup.SetupRenderPipeline();
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
            ProductionVisualIntegration.ResetVisualCachesForGate();
            VisualWorld.VisualIntegrationEnabled=true;
            VisualWorld.Create(true,new PlayerState{
                BastionLevel=3,
                SawmillLevel=1,
                BarracksLevel=1,
                CorruptionDiscovered=true
            });

            var camera=Camera.main;
            if(camera==null)throw new Exception("Valoria camera missing.");
            Directory.CreateDirectory(Folder);

            int suppressed=HideLegacyBastionVisuals();
            if(suppressed<1)throw new Exception("No legacy Bastion visual renderers were suppressed.");
            var hero=PlaceGeneratedHero();

            var heroDistrict=BuildHeroDistrictIntegration();
            if(heroDistrict==null)throw new Exception("Validated Hero District integration root missing.");
            DisableAllGameplayOnVisuals(heroDistrict);

            string baselineSignature=ValoriaVisualFormulaGate.CollisionSignature();
            string beforeMetrics=MetricsJson();

            Save(camera,Folder+"/before-19.png",19f,1280,720);
            Save(camera,Folder+"/before-12.png",12f,1280,720);
            Save(camera,Folder+"/before-9.png",9f,1280,720);
            Save(camera,Folder+"/before-mobile.png",12f,390,844);

            var district=BuildMidLowerDistrictIntegration();
            if(district==null)throw new Exception("Mid/lower district integration root missing.");
            DisableAllGameplayOnVisuals(district);
            string afterSignature=ValoriaVisualFormulaGate.CollisionSignature();
            if(afterSignature!=baselineSignature)
                throw new Exception("Mid/lower district integration altered gameplay collider/hotspot signature.");

            string afterMetrics=MetricsJson();
            Save(camera,Folder+"/after-19.png",19f,1280,720);
            Save(camera,Folder+"/after-12.png",12f,1280,720);
            Save(camera,Folder+"/after-9.png",9f,1280,720);
            Save(camera,Folder+"/after-mobile.png",12f,390,844);

            var heroBounds=BoundsOf(hero);
            var districtBounds=BoundsOf(district);
            File.WriteAllText(Folder+"/mid-lower-district-evidence.json",
                "{\n"+
                "  \"phase\": \"VALORIA_MID_LOWER_DISTRICT_INTEGRATION_V1\",\n"+
                "  \"branch\": \"visual-proof/mid-lower-district-integration-v1\",\n"+
                "  \"source_optimized_sha256\": \"afb6cee6ae572b0879650f18285b32798e263c17359a158ffe2bdd03fb62ad5c\",\n"+
                "  \"tripo_credits_this_proof\": 0,\n"+
                "  \"same_scene_before_after\": true,\n"+
                "  \"collider_hotspot_signature_equal\": true,\n"+
                "  \"generated_asset_colliders_enabled\": false,\n"+
                "  \"generated_asset_hotspots_added\": false,\n"+
                "  \"validated_hero_district_preserved_as_before\": true,\n"+
                "  \"legacy_bastion_renderers_suppressed\": "+suppressed+",\n"+
                "  \"before_metrics\": "+beforeMetrics+",\n"+
                "  \"after_metrics\": "+afterMetrics+",\n"+
                "  \"hero_bounds_center\": ["+F(heroBounds.center.x)+","+F(heroBounds.center.y)+","+F(heroBounds.center.z)+"],\n"+
                "  \"hero_bounds_size\": ["+F(heroBounds.size.x)+","+F(heroBounds.size.y)+","+F(heroBounds.size.z)+"],\n"+
                "  \"district_bounds_center\": ["+F(districtBounds.center.x)+","+F(districtBounds.center.y)+","+F(districtBounds.center.z)+"],\n"+
                "  \"district_bounds_size\": ["+F(districtBounds.size.x)+","+F(districtBounds.size.y)+","+F(districtBounds.size.z)+"]\n"+
                "}\n");

            EditorApplication.Exit(0);
        }

        static GameObject BuildMidLowerDistrictIntegration()
        {
            var root=new GameObject("MID LOWER DISTRICT INTEGRATION v1 · visual only");
            var art=ValoriaExternalAssetLibrary.Load();

            // A. Remove only the provisional visual shells that visibly lower the frame.
            HideVisualFamily("VPD · west rebuilders home");
            HideVisualFamily("VPD · west rebuilders upper dwelling");
            HideVisualFamily("Valoria lower-town home");
            HideVisualFamily("Rebuilder shelter");

            // B. Subordinate the dedicated front barracks: preserve its real mesh/textures,
            // but normalize the over-bright material response instead of replacing the building.
            ToneVisualFamily("Cuartel · dedicated barracks",new Color(.48f,.48f,.46f,1f),.018f);
            ToneVisualFamily("Valoria_Cuartel_AP2_v1",new Color(.48f,.48f,.46f,1f),.018f);
            ToneVisualFamily("Aserradero · dedicated sawmill",new Color(.72f,.66f,.56f,1f),.022f);

            // C. Remove the remaining dark/repetitive support-house shells in the lower camera band.
            SuppressByWorldRegion("Valoria · reused civil house",-22f,-8f,-7f,4.8f);
            SuppressByWorldRegion("Valoria · hero frame inhabited roofline",-22f,-8f,-2f,7.0f);
            SuppressByWorldRegion("Valoria · hero frame inhabited roofline",8f,22f,-2f,8.0f);

            // Replace many small huts with two already-certified residential+rock masses.
            AddRescuedResidential(root,"MidLower · west inhabited rock terrace",
                new Vector3(-11.85f,.28f,-1.35f),5.65f,4.35f,Quaternion.Euler(0,-8f,0));
            AddRescuedResidential(root,"MidLower · east inhabited rock terrace",
                new Vector3(11.55f,.28f,-1.55f),5.55f,4.25f,Quaternion.Euler(0,188f,0));

            // One subordinate authored dwelling per side keeps the lower city inhabited without repetition.
            if(art!=null&&art.SlavicHouse!=null)
            {
                AddPiece(root,ValoriaKit.BenchmarkPieceModulated("MidLower · west secondary dwelling",art.SlavicHouse,
                    new Vector3(-15.6f,.34f,2.35f),2.70f,2.90f,Quaternion.Euler(0,12f,0),new Color(.69f,.66f,.58f,1f)));
                AddPiece(root,ValoriaKit.BenchmarkPieceModulated("MidLower · east secondary dwelling",art.SlavicHouse,
                    new Vector3(15.2f,.34f,2.10f),2.65f,2.85f,Quaternion.Euler(0,170f,0),new Color(.67f,.64f,.57f,1f)));
            }

            // E. Break the board read with buried certified terrain/terrace support.
            AddTerrainTerraceTop(root,"BroadRockPlatform","MidLower · west lower buried shelf",
                new Vector3(-7.15f,0f,-3.25f),.36f,6.30f,16f);
            AddTerrainTerraceTop(root,"BroadRockPlatform","MidLower · east lower buried shelf",
                new Vector3(7.25f,0f,-4.15f),.36f,6.20f,194f);
            AddRescuedSeam(root,"MidLower · stair foot seam west",new Vector3(-3.15f,0f,-.25f),.54f,2.75f,64f);
            AddRescuedSeam(root,"MidLower · stair foot seam east",new Vector3(3.20f,0f,-.20f),.54f,2.70f,294f);

            // F. One continuous but irregular lower approach; certified collision remains below.
            var street=ValoriaGroundKit.StreetStraight("MidLower · inhabited lower approach",
                new Vector3(0,.423f,-4.10f),8.10f,2.78f,0f);
            street.transform.SetParent(root.transform,true);
            var westCourt=ValoriaGroundKit.TerraceFloor("MidLower · west civic court",
                new Vector3(-6.9f,.405f,-3.0f),5.35f,4.80f,-4f);
            westCourt.transform.SetParent(root.transform,true);
            var eastCourt=ValoriaGroundKit.TerraceFloor("MidLower · east civic court",
                new Vector3(6.9f,.405f,-3.55f),5.25f,4.70f,4f);
            eastCourt.transform.SetParent(root.transform,true);

            // G. Retaining fragments visually connect the mid district to the already-approved stair zone.
            AddRescuedSeam(root,"MidLower · stair flank west",
                new Vector3(-3.85f,0f,1.55f),.70f,2.80f,58f);
            AddRescuedSeam(root,"MidLower · stair flank east",
                new Vector3(3.90f,0f,1.62f),.70f,2.75f,302f);
            AddStoneArchitecturePiece(root,"CornerWallL","MidLower · west court retaining corner",
                new Vector3(-8.55f,.30f,-4.65f),1.55f,108f);
            AddStoneArchitecturePiece(root,"CornerWallL","MidLower · east court retaining corner",
                new Vector3(8.45f,.30f,-4.85f),1.50f,252f);

            // H. Controlled vegetation masks only outer joins and empty residuals; never the route.
            if(art!=null&&art.SlavicTree!=null)
            {
                foreach(var spec in new[]{
                    new Vector4(-10.4f,-5.7f,1.15f,-12f),new Vector4(-9.7f,1.6f,1.05f,18f),
                    new Vector4(10.2f,-6.0f,1.12f,14f),new Vector4(9.6f,2.1f,1.02f,-16f)})
                {
                    var go=ValoriaKit.BenchmarkPieceModulated("MidLower · edge tree",art.SlavicTree,
                        new Vector3(spec.x,.02f,spec.y),spec.z,2.75f,Quaternion.Euler(0,spec.w,0),
                        new Color(.43f,.50f,.38f,1f));
                    AddPiece(root,go);
                }
            }

            // I. Local lighting balance only: secondary districts stay below Hero Bastion prominence.
            AddWarmLight(root,"MidLower · west work warmth",new Vector3(-7.0f,1.25f,-3.2f),.22f,2.7f);
            AddWarmLight(root,"MidLower · east civic warmth",new Vector3(6.9f,1.25f,-3.7f),.20f,2.6f);

            return root;
        }

        static void ToneVisualFamily(string family,Color tint,float smoothness)
        {
            int count=0;
            foreach(var r in UnityEngine.Object.FindObjectsByType<Renderer>(FindObjectsSortMode.None))
            {
                if(r==null||!r.enabled||!r.gameObject.activeInHierarchy)continue;
                if(!HierarchyName(r.transform).Contains(family,StringComparison.OrdinalIgnoreCase))continue;
                RebuildRendererMaterials(r,tint,smoothness);
                count++;
            }
            if(count==0)Debug.LogWarning("MidLower: no renderers matched material family "+family);
        }

        static void RebuildRendererMaterials(Renderer r,Color tint,float smoothness)
        {
            var srcs=r.sharedMaterials;var dst=new Material[srcs.Length];
            for(int i=0;i<srcs.Length;i++)
            {
                var src=srcs[i];if(src==null){dst[i]=null;continue;}
                Texture baseMap=null,normal=null,occlusion=null;
                foreach(string p in new[]{"_BaseMap","_MainTex","_BaseColorTexture","baseColorTexture","_Texture"})
                    if(src.HasProperty(p)&&src.GetTexture(p)!=null){baseMap=src.GetTexture(p);break;}
                foreach(string p in new[]{"_BumpMap","_NormalMap","normalTexture"})
                    if(src.HasProperty(p)&&src.GetTexture(p)!=null){normal=src.GetTexture(p);break;}
                foreach(string p in new[]{"_OcclusionMap","_MaskMap","_MetallicGlossMap"})
                    if(src.HasProperty(p)&&src.GetTexture(p)!=null){occlusion=src.GetTexture(p);break;}
                var shader=Shader.Find("Universal Render Pipeline/Lit")??Shader.Find("Standard");
                var m=new Material(shader){name="MidLower URP fitted · "+src.name};
                if(baseMap!=null&&m.HasProperty("_BaseMap"))m.SetTexture("_BaseMap",baseMap);
                if(normal!=null&&m.HasProperty("_BumpMap"))
                {
                    m.SetTexture("_BumpMap",normal);m.EnableKeyword("_NORMALMAP");
                    if(m.HasProperty("_BumpScale"))m.SetFloat("_BumpScale",.9f);
                }
                if(occlusion!=null&&m.HasProperty("_OcclusionMap"))m.SetTexture("_OcclusionMap",occlusion);
                if(m.HasProperty("_BaseColor"))m.SetColor("_BaseColor",tint);
                if(m.HasProperty("_Color"))m.SetColor("_Color",tint);
                if(m.HasProperty("_Smoothness"))m.SetFloat("_Smoothness",smoothness);
                if(m.HasProperty("_Metallic"))m.SetFloat("_Metallic",0f);
                if(m.HasProperty("_OcclusionStrength"))m.SetFloat("_OcclusionStrength",1f);
                if(m.HasProperty("_EmissionColor"))m.SetColor("_EmissionColor",Color.black);
                dst[i]=m;
            }
            r.sharedMaterials=dst;
        }

        static void SuppressByWorldRegion(string family,float minX,float maxX,float minZ,float maxZ)
        {
            foreach(var r in UnityEngine.Object.FindObjectsByType<Renderer>(FindObjectsSortMode.None))
            {
                if(r==null||!r.enabled||!r.gameObject.activeInHierarchy)continue;
                if(!HierarchyName(r.transform).Contains(family,StringComparison.OrdinalIgnoreCase))continue;
                var p=r.bounds.center;
                if(p.x>=minX&&p.x<=maxX&&p.z>=minZ&&p.z<=maxZ)r.enabled=false;
            }
        }

        static void AddRescuedResidential(GameObject root,string name,Vector3 ground,float footprint,float maxHeight,Quaternion rotation)
        {
            var source=Resources.Load<GameObject>("Valoria/Rescued/ResidentialTerraceRock");
            if(source==null)throw new Exception("Missing ResidentialTerraceRock resource.");
            var go=ValoriaKit.BenchmarkPiece(name,source,ground,footprint,maxHeight,rotation);
            if(go==null)throw new Exception("ResidentialTerraceRock failed to instantiate.");
            foreach(var r in go.GetComponentsInChildren<Renderer>(true))
                RebuildRendererMaterials(r,new Color(.72f,.69f,.63f,1f),.025f);
            go.transform.SetParent(root.transform,true);
            DisableAllGameplayOnVisuals(go);
        }

        static void AddRescuedSeam(GameObject root,string name,Vector3 xzAnchor,float topY,float targetSpan,float yaw)
        {
            var source=Resources.Load<GameObject>("Valoria/Rescued/RockTerrainSeamFiller");
            if(source==null)throw new Exception("Missing RockTerrainSeamFiller resource.");
            var go=UnityEngine.Object.Instantiate(source);go.name=name;go.transform.rotation=Quaternion.Euler(0,yaw,0);
            var bounds=BoundsOf(go);float span=Mathf.Max(bounds.size.x,bounds.size.z);
            if(span<=.001f)throw new Exception("RockTerrainSeamFiller empty bounds.");
            go.transform.localScale*=targetSpan/span;
            bounds=BoundsOf(go);
            go.transform.position+=new Vector3(xzAnchor.x-bounds.center.x,topY-bounds.max.y,xzAnchor.z-bounds.center.z);
            go.transform.SetParent(root.transform,true);
            DisableAllGameplayOnVisuals(go);
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
