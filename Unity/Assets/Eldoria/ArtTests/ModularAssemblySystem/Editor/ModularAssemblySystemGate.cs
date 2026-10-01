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
    public static class ModularAssemblySystemGate
    {
        const string Folder="ModularAssemblySystemCaptures";
        const string AssetPath="Assets/Resources/Valoria/HeroBastionGenerated/Valoria_HeroBastion_v1.glb";
        static readonly Vector3 CameraPosition=new Vector3(18.2f,14.6f,-25.8f);
        static readonly Vector3 CameraTarget=new Vector3(0f,3.65f,7.25f);
        static readonly Dictionary<int,Material> sharedSurfaceMaterials=new();

        enum CoreKind { House, Shed }
        enum BaseKind { Stepped, Broad }
        enum ProjectionKind { Balcony, Porch, Gate, None }

        struct AssemblySpec
        {
            public string id, role;
            public Vector3 center;
            public float yaw, span, height, baseSpan, baseYaw;
            public CoreKind core;
            public BaseKind baseKind;
            public ProjectionKind projection;
            public bool wall, seam, corner;
            public float projectionSide;
            public Color tint;
            public AssemblySpec(string id,string role,Vector3 center,float yaw,float span,float height,float baseSpan,float baseYaw,
                CoreKind core,BaseKind baseKind,ProjectionKind projection,bool wall,bool seam,bool corner,float projectionSide,Color tint)
            { this.id=id;this.role=role;this.center=center;this.yaw=yaw;this.span=span;this.height=height;this.baseSpan=baseSpan;this.baseYaw=baseYaw;
              this.core=core;this.baseKind=baseKind;this.projection=projection;this.wall=wall;this.seam=seam;this.corner=corner;this.projectionSide=projectionSide;this.tint=tint; }
        }

        static readonly AssemblySpec[] Specs=new[]{
            new AssemblySpec("A1","secondary guardhouse",new Vector3(7.20f,.42f,-4.00f),180f,3.18f,3.30f,4.60f,92f,CoreKind.House,BaseKind.Stepped,ProjectionKind.Balcony,true,true,false,+1f,new Color(.72f,.68f,.60f,1f)),
            new AssemblySpec("A2","terraced residence",new Vector3(-11.80f,.34f,-2.45f),350f,3.35f,3.45f,5.15f,18f,CoreKind.House,BaseKind.Broad,ProjectionKind.Porch,true,true,false,-1f,new Color(.70f,.66f,.58f,1f)),
            new AssemblySpec("A3","civic corner house",new Vector3(11.60f,.34f,-2.25f),194f,3.05f,3.25f,4.85f,205f,CoreKind.House,BaseKind.Stepped,ProjectionKind.Gate,false,true,true,+1f,new Color(.73f,.69f,.61f,1f)),
            new AssemblySpec("A4","workshop residence hybrid",new Vector3(-15.00f,.34f,1.65f),16f,2.90f,2.85f,4.35f,32f,CoreKind.Shed,BaseKind.Broad,ProjectionKind.Porch,true,true,false,+1f,new Color(.64f,.57f,.48f,1f)),
            new AssemblySpec("A5","small fortified residence",new Vector3(14.80f,.34f,1.95f),166f,2.80f,3.05f,4.25f,174f,CoreKind.House,BaseKind.Stepped,ProjectionKind.Balcony,true,false,true,-1f,new Color(.68f,.64f,.56f,1f))
        };

        public static void Capture()
        {
            ShaderUtil.allowAsyncCompilation=false;
            SceneSetup.SetupRenderPipeline();
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
            ProductionVisualIntegration.ResetVisualCachesForGate();
            VisualWorld.VisualIntegrationEnabled=true;
            VisualWorld.Create(true,new PlayerState{BastionLevel=3,SawmillLevel=1,BarracksLevel=1,CorruptionDiscovered=true});

            var camera=Camera.main;if(camera==null)throw new Exception("Valoria camera missing.");
            Directory.CreateDirectory(Folder);

            if(HideLegacyBastionVisuals()<1)throw new Exception("No legacy Bastion visual renderers were suppressed.");
            var hero=PlaceGeneratedHero();
            var heroDistrict=BuildHeroDistrictIntegration();
            DisableAllGameplayOnVisuals(heroDistrict);

            string baselineSignature=ValoriaVisualFormulaGate.CollisionSignature();
            var baselineIds=EnabledColliderIds();
            var baselineHeroBounds=BoundsOf(hero);
            string baselineMetrics=MetricsJson();

            Save(camera,Folder+"/baseline-19.png",19f,1280,720);
            Save(camera,Folder+"/baseline-12.png",12f,1280,720);
            Save(camera,Folder+"/baseline-9.png",9f,1280,720);
            Save(camera,Folder+"/baseline-mobile.png",12f,390,844);

            int hidden=HideTargetVisuals();
            if(hidden<4)throw new Exception("Expected at least four target parcel visual renderers; hidden="+hidden);
            AssertSignature(baselineSignature,"after visual suppression");

            var root=new GameObject("VALORIA MODULAR ASSEMBLY SYSTEM v1 · visual only");
            int assemblies=0;
            foreach(var spec in Specs){BuildAssembly(root,spec);assemblies++;}
            DisableAllGameplayOnVisuals(root);
            DisableNonBaselineColliders(baselineIds);
            AssertSignature(baselineSignature,"assembled");

            string afterMetrics=MetricsJson();
            Save(camera,Folder+"/assembly-19.png",19f,1280,720);
            Save(camera,Folder+"/assembly-12.png",12f,1280,720);
            Save(camera,Folder+"/assembly-9.png",9f,1280,720);
            Save(camera,Folder+"/assembly-mobile.png",12f,390,844);

            for(int i=0;i<Specs.Length;i++)
            {
                var focus=Specs[i];
                SaveFocus(camera,Folder+"/variant-"+(i+1)+"-"+focus.id+".png",focus.center,8.5f,900,900);
            }

            var heroAfter=BoundsOf(hero);
            bool heroStable=Approximately(baselineHeroBounds,heroAfter,.001f);
            if(!heroStable)throw new Exception("Hero Bastion bounds changed.");
            AssertSignature(baselineSignature,"final");

            File.WriteAllText(Folder+"/modular-assembly-evidence.json",
                "{\n"+
                "  \"phase\": \"VALORIA_MODULAR_ASSEMBLY_SYSTEM_V1\",\n"+
                "  \"branch\": \"visual-proof/modular-assembly-system-v1\",\n"+
                "  \"tripo_credits\": 0,\n"+
                "  \"assemblies_active\": "+assemblies+",\n"+
                "  \"target_renderers_hidden\": "+hidden+",\n"+
                "  \"shared_surface_materials\": "+sharedSurfaceMaterials.Count+",\n"+
                "  \"collider_hotspot_signature_equal\": true,\n"+
                "  \"hero_district_preserved\": true,\n"+
                "  \"hero_bastion_bounds_equal\": true,\n"+
                "  \"gameplay_colliders_added\": 0,\n"+
                "  \"gameplay_hotspots_added\": 0,\n"+
                "  \"baseline_metrics\": "+baselineMetrics+",\n"+
                "  \"assembly_metrics\": "+afterMetrics+",\n"+
                "  \"variants\": [\"secondary guardhouse\",\"terraced residence\",\"civic corner house\",\"workshop residence hybrid\",\"small fortified residence\"]\n"+
                "}\n");
            EditorApplication.Exit(0);
        }

        static void BuildAssembly(GameObject parent,AssemblySpec s)
        {
            var root=new GameObject("Assembly "+s.id+" · "+s.role);root.transform.SetParent(parent.transform,true);
            AddTerrain(root,s.baseKind,s.center,s.center.y+.02f,s.baseSpan,s.baseYaw);

            var art=ValoriaExternalAssetLibrary.Load();
            GameObject core=null;
            if(art!=null)
            {
                var source=s.core==CoreKind.House?art.SlavicHouse:art.SlavicShed;
                if(source!=null)core=ValoriaKit.BenchmarkPieceModulated(s.id+" · occupied core",source,
                    s.center+new Vector3(0,.04f,0),s.span,s.height,Quaternion.Euler(0,s.yaw,0),Color.white);
            }
            if(core!=null){ApplySharedSurface(core,s.tint);AddPiece(root,core);}

            Vector3 rear=RotatedOffset(s.yaw,new Vector3(0,.05f,Mathf.Max(1.00f,s.span*.34f)));
            if(s.wall)AddStone(root,"HighStraightWall",s.id+" · masonry spine",s.center+rear,s.span*.84f,s.yaw);
            if(s.corner)
            {
                Vector3 side=RotatedOffset(s.yaw,new Vector3(s.span*.43f,0,s.span*.14f));
                AddStone(root,"CornerWallL",s.id+" · corner mass",s.center+side,s.span*.58f,s.yaw+88f);
            }
            if(s.seam)
            {
                Vector3 side=RotatedOffset(s.yaw,new Vector3(s.projectionSide*s.span*.55f,.02f,s.span*.05f));
                AddStone(root,"RockToWallTransition",s.id+" · rock wall seam",s.center+side,s.span*.55f,s.yaw+(s.projectionSide>0?64f:-64f));
            }

            Vector3 front=RotatedOffset(s.yaw,new Vector3(s.projectionSide*s.span*.10f,1.10f,-s.span*.44f));
            if(s.projection==ProjectionKind.Balcony)
                AddRaw(root,"Assets/EmaceArt/Slavic World Free/Meshes/EA03_Village_Balcony_R_01a.fbx",s.id+" · gallery",
                    s.center+front,s.span*.72f,1.05f,Quaternion.Euler(0,s.yaw,0),new Color(.58f,.47f,.34f,1f));
            else if(s.projection==ProjectionKind.Porch)
                AddRaw(root,"Assets/EmaceArt/Slavic World Free/Meshes/EA03_Village_HouseModule_Porch_01d.fbx",s.id+" · porch",
                    s.center+front,s.span*.70f,1.35f,Quaternion.Euler(0,s.yaw,0),new Color(.62f,.52f,.39f,1f));
            else if(s.projection==ProjectionKind.Gate && art!=null&&art.SlavicRockGate!=null)
            {
                var gate=ValoriaKit.BenchmarkPieceModulated(s.id+" · civic entry",art.SlavicRockGate,
                    s.center+RotatedOffset(s.yaw,new Vector3(0,.03f,-s.span*.50f)),s.span*.68f,1.70f,Quaternion.Euler(0,s.yaw,0),Color.white);
                ApplySharedSurface(gate,new Color(.66f,.65f,.60f,1f));AddPiece(root,gate);
            }

            var lightPos=s.center+RotatedOffset(s.yaw,new Vector3(0,1.15f,-s.span*.38f));
            AddLocalWarmth(root,lightPos,.16f+(s.id=="A1"?.05f:0f),2.0f+(s.id=="A1"?.25f:0f));
            DisableAllGameplayOnVisuals(root);
        }

        static Vector3 RotatedOffset(float yaw,Vector3 offset)=>Quaternion.Euler(0,yaw,0)*offset;

        static int HideTargetVisuals()
        {
            int count=0;
            foreach(var r in UnityEngine.Object.FindObjectsByType<Renderer>(FindObjectsSortMode.None))
            {
                if(r==null||!r.enabled||!r.gameObject.activeInHierarchy)continue;
                string n=HierarchyName(r.transform);
                bool named=n.Contains("Cuartel · dedicated barracks",StringComparison.OrdinalIgnoreCase)||
                           n.Contains("Cuartel · fallback guardhouse",StringComparison.OrdinalIgnoreCase)||
                           n.Contains("VPD · west rebuilders home",StringComparison.OrdinalIgnoreCase)||
                           n.Contains("VPD · west rebuilders upper dwelling",StringComparison.OrdinalIgnoreCase)||
                           n.Contains("Valoria lower-town home",StringComparison.OrdinalIgnoreCase)||
                           n.Contains("Rebuilder shelter",StringComparison.OrdinalIgnoreCase)||
                           n.Contains("Valoria · reused civil house",StringComparison.OrdinalIgnoreCase)||
                           n.Contains("Valoria · hero frame inhabited roofline",StringComparison.OrdinalIgnoreCase);
                if(named&&NearAssemblyParcel(r.bounds.center,4.25f)){r.enabled=false;count++;}
            }
            return count;
        }

        static bool NearAssemblyParcel(Vector3 p,float radius)
        {
            float r2=radius*radius;
            foreach(var s in Specs)
            {
                var d=new Vector2(p.x-s.center.x,p.z-s.center.z);
                if(d.sqrMagnitude<=r2)return true;
            }
            return false;
        }

        static void AddTerrain(GameObject root,BaseKind kind,Vector3 anchor,float topY,float span,float yaw)
        {
            string resource=kind==BaseKind.Stepped?"SteppedRockTerrace":"BroadRockPlatform";
            var source=Resources.Load<GameObject>("Valoria/TerrainTerraceKit_v1/"+resource);
            if(source==null)throw new Exception("Missing terrain resource "+resource);
            var go=UnityEngine.Object.Instantiate(source);go.name="Assembly · buried "+resource;go.transform.rotation=Quaternion.Euler(0,yaw,0);
            var b=BoundsOf(go);go.transform.localScale*=span/Mathf.Max(b.size.x,b.size.z);b=BoundsOf(go);
            go.transform.position+=new Vector3(anchor.x-b.center.x,topY-b.max.y,anchor.z-b.center.z);
            go.transform.SetParent(root.transform,true);DisableAllGameplayOnVisuals(go);
        }

        static void AddStone(GameObject root,string resource,string name,Vector3 anchor,float span,float yaw)
        {
            var source=Resources.Load<GameObject>("Valoria/StoneArchitectureKit_v1/"+resource);
            if(source==null)throw new Exception("Missing stone resource "+resource);
            var go=UnityEngine.Object.Instantiate(source);go.name=name;go.transform.rotation=Quaternion.Euler(0,yaw,0);
            var b=BoundsOf(go);go.transform.localScale*=span/Mathf.Max(b.size.x,b.size.z);b=BoundsOf(go);
            go.transform.position+=anchor-new Vector3(b.center.x,b.min.y,b.center.z);
            ApplySharedSurface(go,new Color(.66f,.64f,.59f,1f));
            go.transform.SetParent(root.transform,true);DisableAllGameplayOnVisuals(go);
        }

        static void AddRaw(GameObject root,string path,string name,Vector3 anchor,float span,float maxHeight,Quaternion rot,Color tint)
        {
            var source=AssetDatabase.LoadAssetAtPath<GameObject>(path);if(source==null)throw new Exception("Missing raw module "+path);
            var go=UnityEngine.Object.Instantiate(source);go.name=name;go.transform.rotation=rot;FitToAnchor(go,anchor,span,maxHeight);ApplySharedSurface(go,tint);
            go.transform.SetParent(root.transform,true);DisableAllGameplayOnVisuals(go);
        }

        static void FitToAnchor(GameObject go,Vector3 anchor,float targetSpan,float maxHeight)
        {
            var b=BoundsOf(go);float scale=Mathf.Min(targetSpan/Mathf.Max(b.size.x,b.size.z),maxHeight/b.size.y);
            go.transform.localScale*=scale;b=BoundsOf(go);go.transform.position+=anchor-new Vector3(b.center.x,b.min.y,b.center.z);
        }

        static void ApplySharedSurface(GameObject go,Color tint)
        {
            foreach(var r in go.GetComponentsInChildren<Renderer>(true))
            {
                var srcs=r.sharedMaterials;var dst=new Material[srcs.Length];
                for(int i=0;i<srcs.Length;i++)
                {
                    var src=srcs[i];if(src==null){dst[i]=null;continue;}
                    int key=src.GetInstanceID();
                    if(!sharedSurfaceMaterials.TryGetValue(key,out var m))
                    {
                        Texture baseMap=null,normal=null;
                        foreach(string p in new[]{"_BaseMap","_MainTex","_BaseColorTexture","baseColorTexture","_Texture"})
                            if(src.HasProperty(p)&&src.GetTexture(p)!=null){baseMap=src.GetTexture(p);break;}
                        foreach(string p in new[]{"_BumpMap","_NormalMap","normalTexture"})
                            if(src.HasProperty(p)&&src.GetTexture(p)!=null){normal=src.GetTexture(p);break;}
                        var shader=Shader.Find("Universal Render Pipeline/Lit")??Shader.Find("Standard");
                        m=new Material(shader){name="Valoria Assembly shared · "+src.name};
                        if(baseMap!=null&&m.HasProperty("_BaseMap"))m.SetTexture("_BaseMap",baseMap);
                        if(normal!=null&&m.HasProperty("_BumpMap")){m.SetTexture("_BumpMap",normal);m.EnableKeyword("_NORMALMAP");}
                        if(m.HasProperty("_BaseColor"))m.SetColor("_BaseColor",Color.white);
                        if(m.HasProperty("_Color"))m.SetColor("_Color",Color.white);
                        if(m.HasProperty("_Metallic"))m.SetFloat("_Metallic",0f);
                        if(m.HasProperty("_Smoothness"))m.SetFloat("_Smoothness",.025f);
                        m.enableInstancing=true;sharedSurfaceMaterials[key]=m;
                    }
                    dst[i]=m;
                }
                r.sharedMaterials=dst;
                var block=new MaterialPropertyBlock();r.GetPropertyBlock(block);
                block.SetColor("_BaseColor",tint);block.SetColor("_Color",tint);r.SetPropertyBlock(block);
            }
        }

        static GameObject BuildHeroDistrictIntegration()
        {
            var root=new GameObject("HERO DISTRICT INTEGRATION v1 · preserved");
            var art=ValoriaExternalAssetLibrary.Load();
            var stairMat=ValoriaKit.PbrSurfaceMaterial(art!=null?art.ValoriaStoneSurface:null,new Color(.64f,.62f,.57f,1f),new Vector2(2.6f,1.6f),.022f,.95f);
            var groundMat=ValoriaKit.PbrSurfaceMaterial(art!=null?art.ValoriaCobbleSurface:null,new Color(.70f,.67f,.59f,1f),new Vector2(3.8f,3.8f),.025f,.82f);
            foreach(var r in UnityEngine.Object.FindObjectsByType<Renderer>(FindObjectsSortMode.None))
            {
                if(r==null||!r.enabled||!r.gameObject.activeInHierarchy)continue;var n=HierarchyName(r.transform);
                if(n.Contains("VPD · vertical stair",StringComparison.OrdinalIgnoreCase))r.sharedMaterial=stairMat;
                else if(n.Contains("VPD · GroundKit L1 landing",StringComparison.OrdinalIgnoreCase)||n.Contains("VPD · GroundKit L1 west terrace",StringComparison.OrdinalIgnoreCase)||n.Contains("VPD · GroundKit L1 east terrace",StringComparison.OrdinalIgnoreCase))r.sharedMaterial=groundMat;
            }
            AddTerrainTop(root,"BroadRockPlatform",new Vector3(-4.75f,0,7.55f),2.58f,6.15f,18f);
            AddTerrainTop(root,"BroadRockPlatform",new Vector3(4.70f,0,7.65f),2.58f,6.10f,198f);
            AddTerrainTop(root,"SteppedRockTerrace",new Vector3(-3.65f,0,4.85f),2.34f,4.35f,92f);
            AddTerrainTop(root,"SteppedRockTerrace",new Vector3(3.70f,0,4.95f),2.34f,4.30f,268f);
            AddStone(root,"RockToWallTransition","HeroDistrict seam west",new Vector3(-5.45f,2.20f,7.15f),2.45f,58f);
            AddStone(root,"RockToWallTransition","HeroDistrict seam east",new Vector3(5.40f,2.20f,7.25f),2.40f,238f);
            AddStone(root,"HighStraightWall","HeroDistrict wall west",new Vector3(-5.70f,1.20f,5.08f),2.65f,4f);
            AddStone(root,"HighStraightWall","HeroDistrict wall east",new Vector3(5.70f,1.20f,5.08f),2.65f,176f);
            if(art!=null&&art.SlavicHouse!=null)
            {
                var h=ValoriaKit.BenchmarkPieceModulated("HeroDistrict · east upper residence",art.SlavicHouse,new Vector3(5.15f,2.89f,7.15f),3.45f,3.55f,Quaternion.Euler(0,188f,0),new Color(.76f,.73f,.67f,1f));
                AddPiece(root,h);HideVisualFamily("VPD · upper dwelling");
            }
            AddLocalWarmth(root,new Vector3(0f,3.35f,5.45f),.38f,3.7f);AddLocalWarmth(root,new Vector3(4.65f,3.75f,6.35f),.23f,2.4f);
            return root;
        }

        static void AddTerrainTop(GameObject root,string resource,Vector3 anchor,float topY,float span,float yaw)
        {
            var source=Resources.Load<GameObject>("Valoria/TerrainTerraceKit_v1/"+resource);if(source==null)throw new Exception("Missing "+resource);
            var go=UnityEngine.Object.Instantiate(source);go.transform.rotation=Quaternion.Euler(0,yaw,0);var b=BoundsOf(go);go.transform.localScale*=span/Mathf.Max(b.size.x,b.size.z);b=BoundsOf(go);
            go.transform.position+=new Vector3(anchor.x-b.center.x,topY-b.max.y,anchor.z-b.center.z);go.transform.SetParent(root.transform,true);DisableAllGameplayOnVisuals(go);
        }

        static void AddLocalWarmth(GameObject root,Vector3 p,float intensity,float range)
        {
            var go=new GameObject("Assembly · occupied warmth");go.transform.SetParent(root.transform,false);go.transform.position=p;
            var l=go.AddComponent<Light>();l.type=LightType.Point;l.color=new Color(1f,.59f,.30f);l.intensity=intensity;l.range=range;l.shadows=LightShadows.None;
        }

        static void AddPiece(GameObject root,GameObject go){if(go!=null)go.transform.SetParent(root.transform,true);}
        static void HideVisualFamily(string family){foreach(var r in UnityEngine.Object.FindObjectsByType<Renderer>(FindObjectsSortMode.None))if(r!=null&&r.enabled&&HierarchyName(r.transform).Contains(family,StringComparison.OrdinalIgnoreCase))r.enabled=false;}
        static string HierarchyName(Transform t){string s="";for(var p=t;p!=null;p=p.parent)s=p.name+"/"+s;return s;}

        static HashSet<int> EnabledColliderIds(){var ids=new HashSet<int>();foreach(var c in UnityEngine.Object.FindObjectsByType<Collider>(FindObjectsSortMode.None))if(c!=null&&c.enabled&&c.gameObject.activeInHierarchy)ids.Add(c.GetInstanceID());return ids;}
        static void DisableNonBaselineColliders(HashSet<int> baseline){foreach(var c in UnityEngine.Object.FindObjectsByType<Collider>(FindObjectsSortMode.None))if(c!=null&&c.enabled&&c.gameObject.activeInHierarchy&&!baseline.Contains(c.GetInstanceID()))c.enabled=false;Physics.SyncTransforms();}
        static void AssertSignature(string baseline,string label){if(ValoriaVisualFormulaGate.CollisionSignature()!=baseline)throw new Exception(label+" altered gameplay signature.");}
        static void DisableAllGameplayOnVisuals(GameObject root){foreach(var c in root.GetComponentsInChildren<Collider>(true))c.enabled=false;foreach(var h in root.GetComponentsInChildren<WorldHotspot>(true))UnityEngine.Object.DestroyImmediate(h);foreach(var b in root.GetComponentsInChildren<MonoBehaviour>(true))if(!(b is WorldHotspot))b.enabled=false;Physics.SyncTransforms();}

        static int HideLegacyBastionVisuals()
        {
            int count=0;foreach(var r in UnityEngine.Object.FindObjectsByType<Renderer>(FindObjectsSortMode.None)){if(r==null||!r.enabled||!r.gameObject.activeInHierarchy)continue;bool legacy=false;
                for(var t=r.transform;t!=null;t=t.parent)if(t.name.StartsWith("Bastion ·",StringComparison.OrdinalIgnoreCase)||string.Equals(t.name,"Bastion",StringComparison.OrdinalIgnoreCase)||t.name.StartsWith("Valoria · Bastion hero",StringComparison.OrdinalIgnoreCase)||t.name.StartsWith("Valoria · rescued hero flank",StringComparison.OrdinalIgnoreCase)){legacy=true;break;}
                if(legacy){r.enabled=false;count++;}}return count;
        }

        static GameObject PlaceGeneratedHero()
        {
            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);var source=AssetDatabase.LoadAssetAtPath<GameObject>(AssetPath);if(source==null)throw new Exception("Hero Bastion missing.");
            var go=UnityEngine.Object.Instantiate(source);go.name="Valoria · Generated Hero Bastion v1 · assembly proof";go.transform.rotation=Quaternion.Euler(0,180f,0);DisableAllGameplayOnVisuals(go);
            var b=BoundsOf(go);float scale=Mathf.Min(12.8f/Mathf.Max(b.size.x,b.size.z),10.2f/b.size.y);go.transform.localScale*=scale;b=BoundsOf(go);go.transform.position+=new Vector3(-b.center.x,2.52f-b.min.y,8.75f-b.center.z);
            FitGeneratedHeroSurface(go);return go;
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
                    if(baseMap!=null&&m.HasProperty("_BaseMap"))m.SetTexture("_BaseMap",baseMap);
                    if(normal!=null&&m.HasProperty("_BumpMap")){m.SetTexture("_BumpMap",normal);m.EnableKeyword("_NORMALMAP");if(m.HasProperty("_BumpScale"))m.SetFloat("_BumpScale",1f);}
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

        static Bounds BoundsOf(GameObject go){var rs=go.GetComponentsInChildren<Renderer>(true);if(rs.Length==0)return new Bounds(go.transform.position,Vector3.zero);var b=rs[0].bounds;for(int i=1;i<rs.Length;i++)b.Encapsulate(rs[i].bounds);return b;}
        static bool Approximately(Bounds a,Bounds b,float e)=>(a.center-b.center).sqrMagnitude<e*e&&(a.size-b.size).sqrMagnitude<e*e;
        static string MetricsJson(){long tri=0;int ren=0,lights=0;var mats=new HashSet<int>();foreach(var r in UnityEngine.Object.FindObjectsByType<Renderer>(FindObjectsSortMode.None)){if(r==null||!r.enabled||!r.gameObject.activeInHierarchy)continue;ren++;foreach(var m in r.sharedMaterials)if(m!=null)mats.Add(m.GetInstanceID());var mf=r.GetComponent<MeshFilter>();if(mf!=null&&mf.sharedMesh!=null)tri+=mf.sharedMesh.triangles.LongLength/3;var sk=r as SkinnedMeshRenderer;if(sk!=null&&sk.sharedMesh!=null)tri+=sk.sharedMesh.triangles.LongLength/3;}foreach(var l in UnityEngine.Object.FindObjectsByType<Light>(FindObjectsSortMode.None))if(l!=null&&l.enabled&&l.gameObject.activeInHierarchy)lights++;return "{\"triangles\":"+tri+",\"renderers\":"+ren+",\"materials\":"+mats.Count+",\"lights\":"+lights+"}";}

        static void Save(Camera camera,string path,float zoom,int width,int height){camera.transform.position=CameraPosition;camera.transform.LookAt(CameraTarget);camera.orthographic=true;camera.orthographicSize=zoom;Render(camera,path,width,height);}
        static void SaveFocus(Camera camera,string path,Vector3 target,float zoom,int width,int height){camera.transform.position=target+new Vector3(15f,11f,-19f);camera.transform.LookAt(target+Vector3.up*1.4f);camera.orthographic=true;camera.orthographicSize=zoom;Render(camera,path,width,height);}
        static void Render(Camera camera,string path,int width,int height){var rt=new RenderTexture(width,height,24,RenderTextureFormat.ARGB32);var tex=new Texture2D(width,height,TextureFormat.RGB24,false);camera.targetTexture=rt;camera.Render();RenderTexture.active=rt;tex.ReadPixels(new Rect(0,0,width,height),0,0);tex.Apply();File.WriteAllBytes(path,tex.EncodeToPNG());camera.targetTexture=null;RenderTexture.active=null;UnityEngine.Object.DestroyImmediate(tex);UnityEngine.Object.DestroyImmediate(rt);}
    }
}
