using System;
using System.Collections.Generic;
using Eldoria.Domain;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Eldoria.Presentation
{
    // Dressing over certified runtime topology. No gameplay, parcel, camera or collision ownership.
    public static class ProductionVisualIntegration
    {
        static Material landscape;
        static Material sharedStone;
        static Material terrainTerraceStone;
        static readonly Dictionary<string,Material> adapted = new();
        static readonly Color Blue = new Color(.13f,.24f,.38f);
        static readonly Color Rock = new Color(.42f,.43f,.39f);
        static Transform root;
        // Gate-only switch: lets CI compare the current city with/without Stone Architecture v1 while keeping every other visual layer identical.
        // Production placement set after camera review: 2 CornerWallL / 1 HighStraightWall / 2 RockToWallTransition.
        public static bool StoneArchitectureEnabled = true;
        // Gate switch for the citywide Terrain & Terrace v1 composition. Visual-only; topology/collision remain authoritative below.
        // This line also keeps the final composed HEAD inside the Unity/Valoria/world visual path filters.
        public static bool TerrainTerraceEnabled = true;
        // Production Cell v1: small finished slice used to prove the new environment-art process
        // (shared ground language + props + occupation + atmosphere) before scaling citywide.
        // Visual-only; it never owns gameplay topology, colliders or hotspots.
        public static bool ProductionCellEnabled = false;
        // Surface Cell v7: existing-silhouette PBR look-dev proof. Disabled in production until matched-camera review passes. Validation trigger after governance repair.
        public static bool SurfaceCellEnabled = false;
        // Gate-only coherent asset-language proof using a single CC0 Kenney Castle Kit family.
        // Never enabled in production automatically.
        public static bool CoherentCastleProofEnabled = false;
        // Gate-only proof using one higher-detail family already shipped in the project.
        public static bool SlavicDistrictProofEnabled = false; // v2 material-integration proof
        // Compact Footprint Reframe v1 gate: existing geometry only, visual-only, no gameplay authority.
        public static bool CompactFootprintReframeEnabled = false;

        public static void ResetVisualCachesForGate()
        {
            // Deterministic visual QA only: scene reloads must not inherit runtime-created material caches.
            landscape=null;
            sharedStone=null;
            terrainTerraceStone=null;
            adapted.Clear();
            groundSkins.Clear();
            root=null;
        }

        public static void World(PlayerState state)
        {
            root = new GameObject("Frontier · integrated 4X visual layer").transform;
            UnifyLandscape(false);
            BlendStrategicGround();
            Suppress("Sir Aldric ","Aldric ","Archer ","Bow");
            // Replace the primitive foliage read with two mapped, authored tree variants.
            Suppress("Frontier · forest pine", "Frontier · tall evergreen", "Frontier · ridge pine",
                "Frontier · undergrowth", "Frontier · forest moss", "Frontier · west ridge", "Frontier · east ridge",
                "Frontier · north cliff", "Frontier · south terrain transition", "Frontier · stacked timber");
            var clusters = new[] {
                new Vector3(-7,0,1.5f), new Vector3(-13,0,7),
                new Vector3(13,0,2), new Vector3(-10,0,-9), new Vector3(1,0,13)
            };
            for(int c=0;c<clusters.Length;c++)
            for(int i=0;i<(c==0?13:9);i++)
            {
                float a=i*2.39996f+c*.71f;
                float radius=Mathf.Sqrt(i+.7f)*(c==0?.72f:.9f);
                var p=clusters[c]+new Vector3(Mathf.Cos(a)*radius,.05f,Mathf.Sin(a)*radius);
                Imported("4X · forest canopy", "Tree01"+(i%2==0?"A":"B"),p,
                    1.6f+(i%3)*.24f,2.35f+(i%4)*.23f,i*47+c*29,new Color(.54f,.61f,.48f),true);
            }
            // Mountain barriers live behind the nodes; low rock skirts merge into continuous ground.
            foreach(var p in new[]{new Vector3(-17,-.25f,12),new Vector3(14,-.25f,13),new Vector3(-3,-.35f,21)})
            {
                Imported("4X · mountain barrier", "Mountain01",p,12.0f,6.3f,p.x*7,Rock,false);
                Imported("4X · buried foothill", "Rock02",p+new Vector3(2,-.1f,-3),5.3f,2.5f,p.x*11,Rock,false);
            }
            foreach(var p in new[]{new Vector3(-11,.02f,5),new Vector3(10,.02f,7),new Vector3(14,.02f,-6),new Vector3(-13,.02f,-4)})
                Imported("4X · route rock shoulder","Rock01",p,3.4f,1.4f,p.z*19,Rock,false);

            foreach(var spec in new[]{new Vector4(-3.15f,.5f,5.8f,90f),new Vector4(3.15f,-1.8f,5.8f,90f)})
            {
                var route=ValoriaGroundKit.TrailStraight("4X · resource access route",new Vector3(spec.x,.15f,spec.y),spec.z,.85f,spec.w);
                route.transform.SetParent(root,true);
                SkinRoute(route);
            }
            var art=ValoriaExternalAssetLibrary.Load();
            // Wood identity: stocked timber frontage distinct from background forest.
            Piece("4X · wood stock",art!=null?art.Firewood:null,new Vector3(-5.3f,.12f,-.15f),1.7f,.85f,-16,new Color(.62f,.55f,.43f));
            // Existing quarry kit remains independent from the authoritative target.
            foreach(var p in new[]{new Vector3(6.35f,.03f,-2.15f),new Vector3(-4,.03f,7.4f)})
            {
                Imported("4X · cut stone face","Rock02",p+new Vector3(.2f,0,.6f),2.1f,1.2f,35, new Color(.63f,.61f,.53f),false);
                Piece("4X · quarry edge",art!=null?art.SlavicStoneFence:null,p+new Vector3(-.5f,.02f,-.65f),1.45f,.45f,0,new Color(.61f,.57f,.49f));
            }
            // Origin city and deployed party are map representations, not extra gameplay buildings.
            CityOrigin(new Vector3(-1.4f,.12f,-5.8f));
            if(state.March.Phase!="idle")March(new Vector3(1.2f,.08f,-3.8f));
            // The future chapter-III strategic layer is visible only in its proper progression state.
            if(state.BastionLevel>=3)
            {
                Beast("4X · wolf · placeholder",new Vector3(-2.7f,.10f,3.4f),false);
                Beast("4X · boar · placeholder",new Vector3(6.7f,.10f,-6.0f),true);
                Ruin(new Vector3(-1.9f,.08f,9.4f));
                Piece("4X · food cache · placeholder",art!=null?art.SlavicShed:null,
                    new Vector3(-8.8f,.08f,-4.7f),1.45f,1.15f,-14,new Color(.56f,.51f,.42f));
                Piece("4X · food sacks · placeholder",Resources.Load<GameObject>("Valoria/UrbanProps/Sack"),new Vector3(-8.25f,.08f,-5.2f),.8f,.6f,0,new Color(.8f,.73f,.57f));
                Piece("4X · food barrel · placeholder",Resources.Load<GameObject>("Valoria/UrbanProps/Barrel"),new Vector3(-9.3f,.08f,-5.25f),.55f,.75f,0,new Color(.72f,.63f,.48f));
            }
            Node("Valoria",new Vector3(-1.4f,.15f,-5.8f),Blue,1.6f);
            Node("Madera",new Vector3(-5.3f,.15f,-.15f),new Color(.65f,.48f,.22f),1.1f);
            Node("Piedra",new Vector3(6.35f,.15f,-2.15f),new Color(.65f,.63f,.53f),1.35f);
            Node("Engendro",new Vector3(5f,.15f,3.15f),new Color(.43f,.22f,.39f),1.25f);
            Node("Brecha",new Vector3(10f,.15f,8f),new Color(.48f,.23f,.43f),1.55f);
            if(state.March.Phase!="idle")Node("Marcha",new Vector3(1.2f,.15f,-3.8f),Blue,.85f);
            if(state.BastionLevel>=3)
            {
                Node("Lobo",new Vector3(-2.7f,.15f,3.4f),new Color(.63f,.39f,.25f),.85f);
                Node("Jabalí",new Vector3(6.7f,.15f,-6f),new Color(.63f,.39f,.25f),.85f);
                Node("Ruinas",new Vector3(-1.9f,.15f,9.4f),new Color(.64f,.58f,.39f),1.35f);
                Node("Alimento",new Vector3(-8.8f,.15f,-4.7f),new Color(.65f,.48f,.22f),1f);
            }
            if(state.BastionLevel>=3)
            for(int i=0;i<3;i++)Primitive("4X · food provision bundle · placeholder",PrimitiveType.Sphere,
                new Vector3(-8.8f+(i-1)*.28f,.35f,-4.7f),new Vector3(.30f,.48f,.44f),new Color(.47f,.37f,.20f));
            // Existing territorial scar gains an installation silhouette, without neon crystals.
            Imported("4X · breach broken arch","Arch_Gothic",new Vector3(9.4f,.02f,8.4f),2.5f,3.25f,-22,new Color(.23f,.22f,.26f),false);
            Imported("4X · breach ruin flank","Wall_Broken",new Vector3(11.3f,.03f,8.0f),1.6f,1.6f,53,new Color(.27f,.25f,.28f),false);
            Finish();
        }

        public static void City(PlayerState state)
        {
            root = new GameObject("Valoria · integrated construction visual layer").transform;
            UnifyLandscape(true);
            ReplaceCityTrees();
            Suppress("Sir Aldric ","Aldric ","Archer ","Bow");
            // Subordinate inhabited silhouettes replace oversized provisional staging primitives.
            foreach(var p in new[]{new Vector3(-10.5f,.47f,-1.1f),new Vector3(-14.4f,.49f,-.7f),
                new Vector3(-11.3f,.46f,3.45f),new Vector3(-7.6f,.44f,-4.35f),new Vector3(6.4f,.44f,-5.8f),
                new Vector3(-4.8f,2.77f,5.9f),new Vector3(1.0f,.43f,-4.6f)})
                Civilian(p);

            var art=ValoriaExternalAssetLibrary.Load();
            // Fit real support architecture into the already-authored residential footprints.
            if(art!=null&&art.SlavicHouse!=null)
            {
                Suppress("VPD · west rebuilders home · roof", "VPD · west rebuilders upper dwelling · roof");
                var homes=new[]{new Vector3(-12,.34f,-3.25f),new Vector3(-15.25f,.36f,-3.05f),
                    new Vector3(-13.25f,.36f,2.45f),new Vector3(-17.35f,.38f,2.15f),
                    new Vector3(-15.8f,1.28f,5.55f),new Vector3(-18.05f,1.18f,6.05f)};
                for(int i=0;i<homes.Length;i++)
                    Piece("Valoria · reused civil house "+i,art.SlavicHouse,
                        homes[i]+Vector3.up*(i==5?1.12f:i>=4?1.28f:1.18f),i>=4?2.35f:2.20f,.94f,0,new Color(.62f,.57f,.48f));
            }
            // StoneKit surface and border functions. Y-normalized skins never become floors.
            for(int i=0;i<7;i++)
            {
                float z=-6.0f+i*.92f;
                StonePiece(1,"Valoria · stone street slab",new Vector3((i%2==0?-.16f:.19f),.405f,z),
                    new Vector3(2.22f,.11f,1.22f),i%2==0?0:180);
            }
            for(int i=0;i<5;i++)
                StonePiece(2,"Valoria · west street transition",new Vector3(-9.55f-i*1.5f,.445f,-1.02f),
                    new Vector3(1.72f,.10f,1.62f),i*71);
            // Deliberate small courts, buried seams and frontages, outside the central walking envelope.
            StonePiece(2,"Valoria · workshop court",new Vector3(-8.7f,.43f,-4.6f),new Vector3(2.2f,.10f,1.65f),14);
            StonePiece(1,"Valoria · barracks apron",new Vector3(8.0f,.44f,-5.9f),new Vector3(2.35f,.10f,1.48f),0);
            StonePiece(2,"Valoria · upper civil court",new Vector3(-5.1f,2.74f,5.9f),new Vector3(2.3f,.10f,1.7f),97);
            for(int i=0;i<3;i++)
            {
                StonePiece(3,"Valoria · low street edge",new Vector3(-1.75f,.41f,-5.55f+i*1.80f),new Vector3(.24f,.21f,1.95f),0);
                StonePiece(3,"Valoria · east street edge",new Vector3(1.75f,.41f,-5.55f+i*1.80f),new Vector3(.24f,.21f,1.95f),180);
            }
            StonePiece(4,"Valoria · workshop court corner",new Vector3(-9.1f,.37f,-5.2f),new Vector3(1.02f,.28f,1.2f),180);
            StonePiece(4,"Valoria · training court corner",new Vector3(9.5f,.37f,-6.0f),new Vector3(1.1f,.26f,1.2f),90);
            // Thin construction skins follow each certified tread's exact pose; physics stays original.
            for(int i=0;i<12;i++)
                StonePiece(5,"Valoria · worn tread skin",new Vector3(i%3==0?.025f:0f,.466f+i*.18f,.15f+i*.52f),
                    new Vector3(3.06f,.025f,.49f),i%4==1?.7f:0);
            // The certified twelve physical treads are untouched; cheek accents are visual only.
            StonePiece(5,"Valoria · stair cheek stone",new Vector3(-2.2f,.5f,1.1f),new Vector3(.75f,.26f,.48f),0);
            StonePiece(5,"Valoria · upper landing cheek",new Vector3(2.5f,2.70f,6.5f),new Vector3(.8f,.24f,.46f),90);
            StonePiece(6,"Valoria · workshop foundation stone",new Vector3(-9.8f,.22f,-2.25f),new Vector3(.65f,.6f,.72f),24);
            StonePiece(6,"Valoria · retaining foundation stone",new Vector3(4.1f,1.56f,4.75f),new Vector3(.65f,.72f,.70f),72);
            if(state.BastionLevel>=3)
            {
                StonePiece(2,"Valoria · granary court",new Vector3(-17.5f,.43f,-4.5f),new Vector3(2.2f,.1f,1.45f),13);
                StonePiece(3,"Valoria · granary edge",new Vector3(-19.1f,.39f,-4.6f),new Vector3(.24f,.23f,2.15f),15);
            }
            // Existing hard-surface props support work areas; no invented functional buildings.
            foreach(var p in new[]{new Vector3(-12.2f,.40f,3.7f),new Vector3(-16.5f,.40f,.65f),new Vector3(-7.9f,.41f,-4.5f)})
                Piece("Valoria · stocked work frontage",art!=null?art.Firewood:null,p,1.03f,.62f,p.x*13,new Color(.78f,.69f,.55f));
            for(int i=0;i<6;i++)
            {
                var p=new Vector3(-11.8f-(i%3)*2.15f,.40f,i<3?3.75f:-3.7f);
                Piece("Valoria · civil store crate",Resources.Load<GameObject>("Valoria/UrbanProps/Crate"),p,.50f,.50f,i*23,new Color(.75f,.65f,.5f));
                Piece("Valoria · civil store barrel",Resources.Load<GameObject>("Valoria/UrbanProps/Barrel"),p+new Vector3(.45f,0,.17f),.38f,.6f,i*31,new Color(.70f,.61f,.48f));
                if(state.BastionLevel>=3)Piece("Valoria · food sack",Resources.Load<GameObject>("Valoria/UrbanProps/Sack"),p+new Vector3(.1f,0,.49f),.55f,.38f,0,new Color(.8f,.72f,.57f));
            }
            // Certified Stone Architecture v1 is visual dressing only. It adapts to the approved city topology;
            // it never owns circulation, floors, hotspots or gameplay collision.
            if(StoneArchitectureEnabled)IntegrateStoneArchitecture();
            if(TerrainTerraceEnabled)IntegrateTerrainTerraceCitywide();
            IntegrateRescuedTerrainSeams();
            // Certified Mid-Tier District v1: three real Mid/Lower parcels, visual-only, Bastion III+.
            MidTierDistrictProduction.Build(root,state);
            DressBastion();
            var tower=Resources.Load<GameObject>("Valoria/Rescued/TowerWallRock");
            if(tower==null)throw new InvalidOperationException("Persisted TowerWallRock could not import as a prefab");
            Piece("Valoria · rescued hero flank",tower,new Vector3(-3.9f,.18f,3.9f),3.2f,4.2f,18,new Color(.62f,.64f,.60f));
            ComposeHeroFrame(state,art);
            if(CompactFootprintReframeEnabled)ComposeCompactFootprintReframeV1(state,art);
            if(SurfaceCellEnabled)IntegrateSurfaceCell();
            if(CoherentCastleProofEnabled)IntegrateCoherentCastleProof();
            if(SlavicDistrictProofEnabled)IntegrateSlavicDistrictProof();
            if(ProductionCellEnabled)IntegrateProductionCell(state,art);
            Finish();
        }

        public static void AddSurfaceCellForGate()
        {
            if(root==null)throw new InvalidOperationException("Valoria visual integration root is not initialized.");
            IntegrateSurfaceCell();
            Finish();
        }

        static void IntegrateSurfaceCell()
        {
            // Surface Cell v7 — preserve the current Bastion silhouette and replace only its surface language.
            // No new architecture, no route/collider/hotspot ownership, no citywide repaint.
            RenderSettings.ambientMode=UnityEngine.Rendering.AmbientMode.Trilight;
            RenderSettings.ambientSkyColor=new Color(.75f,.80f,.84f);
            RenderSettings.ambientEquatorColor=new Color(.49f,.48f,.44f);
            RenderSettings.ambientGroundColor=new Color(.25f,.23f,.20f);
            RenderSettings.ambientIntensity=.88f;
            RenderSettings.fog=true;
            RenderSettings.fogMode=FogMode.Linear;
            RenderSettings.fogColor=new Color(.55f,.60f,.63f);
            RenderSettings.fogStartDistance=48f;
            RenderSettings.fogEndDistance=122f;

            var camera=Camera.main;
            if(camera!=null)
            {
                camera.clearFlags=CameraClearFlags.SolidColor;
                camera.backgroundColor=new Color(.49f,.58f,.63f);
                camera.allowHDR=true;
                TrySetComponentProperty(camera.gameObject,"UniversalAdditionalCameraData","renderPostProcessing",true);
            }

            foreach(var light in Object.FindObjectsByType<Light>(FindObjectsSortMode.None))
            {
                if(light.name!="Valoria · amber dusk")continue;
                light.color=new Color(1.0f,.87f,.72f);
                light.intensity=1.28f;
                light.shadowStrength=.76f;
                light.shadows=LightShadows.Soft;
                light.transform.rotation=Quaternion.Euler(47f,-34f,0f);
            }

            var fillGo=new GameObject("Valoria · SurfaceCell · cool sky fill");
            fillGo.transform.SetParent(root,true);
            fillGo.transform.rotation=Quaternion.Euler(34f,150f,0f);
            var fill=fillGo.AddComponent<Light>();
            fill.type=LightType.Directional;
            fill.color=new Color(.61f,.72f,.93f);
            fill.intensity=.11f;
            fill.shadows=LightShadows.None;

            InstallUrpFinishV5();

            var art=ValoriaExternalAssetLibrary.Load();
            var cobble=ValoriaKit.ExternalPbrSurfaceMaterial("cobble",
                new Color(.91f,.89f,.84f),new Vector2(3.15f,3.15f),.065f,1.08f)
                ?? ValoriaKit.PbrSurfaceMaterial(art!=null?art.ValoriaCobbleSurface:null,
                    new Color(.83f,.80f,.72f),new Vector2(3.35f,3.35f),.075f,1.0f);
            var dirt=ValoriaKit.ExternalPbrSurfaceMaterial("dirt",
                new Color(.86f,.80f,.69f),new Vector2(3.85f,3.85f),.022f,.92f)
                ?? ValoriaKit.PbrSurfaceMaterial(art!=null?art.ValoriaDirtSurface:null,
                    new Color(.64f,.54f,.41f),new Vector2(4.1f,4.1f),.028f,.88f);
            var bastionStone=ValoriaKit.ExternalPbrSurfaceMaterial("stone",
                new Color(.74f,.70f,.62f),new Vector2(2.15f,2.15f),.035f,1.12f)
                ?? ValoriaKit.PbrSurfaceMaterial(art!=null?art.ValoriaStoneSurface:null,
                    new Color(.70f,.67f,.60f),new Vector2(2.40f,2.40f),.045f,1.05f);
            var bastionDeep=ValoriaKit.ExternalPbrSurfaceMaterial("stone",
                new Color(.53f,.50f,.45f),new Vector2(2.35f,2.35f),.025f,1.05f)
                ?? ValoriaKit.PbrSurfaceMaterial(art!=null?art.ValoriaStoneSurface:null,
                    new Color(.53f,.51f,.47f),new Vector2(2.55f,2.55f),.035f,1.0f);

            foreach(var renderer in Object.FindObjectsByType<Renderer>(FindObjectsSortMode.None))
            {
                if(!renderer.enabled||!renderer.gameObject.activeInHierarchy)continue;
                string n=renderer.gameObject.name;
                string lower=n.ToLowerInvariant();
                if(n.Contains("target")||n.Contains("Hero")||n.Contains("Archer")||n.Contains("worker"))continue;

                // Preserve AP2 authored maps.
                if(n.StartsWith("Aserradero")||n.StartsWith("Cuartel"))
                {
                    PolishImportedSurface(renderer,new Color(.99f,.98f,.96f));
                    continue;
                }

                // The existing fortress silhouette stays. Only masonry receives one coherent stone family.
                bool bastion=n.StartsWith("Bastion ·")||n.StartsWith("Valoria · Bastion hero");
                if(bastion)
                {
                    bool preserve=lower.Contains("banner")||lower.Contains("slit")||
                                  lower.Contains("scaffold")||lower.Contains("roof")||
                                  lower.Contains("crown");
                    if(!preserve)
                    {
                        bool deep=lower.Contains("backing")||lower.Contains("plinth")||
                                  lower.Contains("rubble")||lower.Contains("collapse");
                        renderer.sharedMaterial=deep?bastionDeep:bastionStone;
                    }
                    continue;
                }

                bool earth=lower.Contains("groundkit")&&
                          (lower.Contains("earth")||lower.Contains("wear")||lower.Contains("seam"));
                bool paving=lower.Contains("groundkit")||lower.Contains("vertical stair")||
                            lower.Contains("street slab")||lower.Contains("worn tread")||
                            lower.Contains("apron")||lower.Contains("court")||
                            lower.Contains("landing cheek")||lower.Contains("stair cheek");
                if(earth)renderer.sharedMaterial=dirt;
                else if(paving)renderer.sharedMaterial=cobble;
            }

            // Seat the existing silhouette into the terrace; no replacement fortress is instantiated.
            if(art!=null&&art.SlavicFlatRock!=null)
            {
                TargetFramePiece("Valoria · SurfaceCell · bastion seam west",art.SlavicFlatRock,
                    new Vector3(-4.45f,2.72f,6.10f),2.35f,1.05f,36f,new Color(.73f,.72f,.67f));
                TargetFramePiece("Valoria · SurfaceCell · bastion seam east",art.SlavicFlatRock,
                    new Vector3(4.45f,2.72f,6.25f),2.30f,1.02f,214f,new Color(.73f,.72f,.67f));
            }

            WarmLight("Valoria · SurfaceCell · bastion gate warmth",new Vector3(0f,4.05f,4.05f),
                new Color(1.0f,.57f,.26f),.24f,4.5f);
            WarmLight("Valoria · SurfaceCell · sawmill grazing warmth",new Vector3(-5.6f,1.95f,-4.85f),
                new Color(1.0f,.62f,.31f),.12f,3.7f);
            WarmLight("Valoria · SurfaceCell · barracks grazing warmth",new Vector3(5.55f,1.95f,-5.20f),
                new Color(1.0f,.66f,.36f),.11f,3.5f);
        }

        static void BuildSurfaceCellTargetBastion(ValoriaExternalAssetLibrary art)
        {
            if(art==null)return;
            var p=new Vector3(0f,3.02f,7.25f);

            // Recessed gate and connected front wall. All pieces are presentation-only children of root.
            TargetFramePiece("Valoria · TargetFrame · gate",art.MegaHalfGate,
                p+new Vector3(0f,0f,-2.85f),3.05f,4.20f,0f,new Color(.96f,.91f,.82f));
            TargetFramePiece("Valoria · TargetFrame · front wall west",art.MasonryWall,
                p+new Vector3(-2.85f,0f,-2.22f),3.45f,3.15f,2f,new Color(.92f,.89f,.82f));
            TargetFramePiece("Valoria · TargetFrame · front wall east",art.MasonryWall,
                p+new Vector3(2.85f,0f,-2.22f),3.45f,3.15f,178f,new Color(.92f,.89f,.82f));

            // Strong asymmetric skyline: one central/rear keep and two smaller flank towers.
            TargetFramePiece("Valoria · TargetFrame · high keep",art.MegaTower,
                p+new Vector3(-.45f,.04f,1.10f),3.30f,7.25f,-2f,new Color(.94f,.91f,.84f));
            TargetFramePiece("Valoria · TargetFrame · west tower",art.MegaTower,
                p+new Vector3(-3.35f,.02f,.05f),2.38f,5.35f,5f,new Color(.90f,.87f,.80f));
            TargetFramePiece("Valoria · TargetFrame · east tower",art.MegaTower,
                p+new Vector3(3.20f,.02f,.42f),2.18f,4.78f,-7f,new Color(.89f,.86f,.79f));

            // Rear masonry makes it a fortress volume rather than three disconnected towers.
            TargetFramePiece("Valoria · TargetFrame · rear wall west",art.MasonryWall,
                p+new Vector3(-2.15f,.02f,2.15f),3.05f,2.78f,180f,new Color(.86f,.84f,.78f));
            TargetFramePiece("Valoria · TargetFrame · rear wall east",art.MasonryWall,
                p+new Vector3(2.10f,.02f,2.18f),3.00f,2.75f,180f,new Color(.86f,.84f,.78f));

            // Readable heraldry at gameplay zoom; no additional clutter.
            Flag("Valoria · TargetFrame · standard west",p+new Vector3(-2.10f,.04f,-3.10f),Blue,2.10f);
            Flag("Valoria · TargetFrame · standard east",p+new Vector3(2.10f,.04f,-3.10f),Blue,2.10f);
        }

        static GameObject TargetFramePiece(string name,GameObject source,Vector3 ground,float footprint,float height,float yaw,Color tint)
        {
            if(source==null)return null;
            var go=ValoriaKit.BenchmarkPieceModulated(name,source,ground,footprint,height,
                Quaternion.Euler(0f,yaw,0f),tint);
            if(go==null)return null;
            go.transform.SetParent(root,true);
            foreach(var collider in go.GetComponentsInChildren<Collider>(true))collider.enabled=false;
            foreach(var hotspot in go.GetComponentsInChildren<WorldHotspot>(true))Object.DestroyImmediate(hotspot);
            return go;
        }

        static Type FindRuntimeType(string fullName)
        {
            foreach(var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                var type=assembly.GetType(fullName,false);
                if(type!=null)return type;
            }
            return null;
        }

        static void TrySetComponentProperty(GameObject go,string shortTypeName,string propertyName,object value)
        {
            foreach(var component in go.GetComponents<Component>())
            {
                if(component==null||component.GetType().Name!=shortTypeName)continue;
                var property=component.GetType().GetProperty(propertyName);
                if(property!=null&&property.CanWrite)property.SetValue(component,value,null);
                return;
            }
        }

        static void InstallUrpFinishV5()
        {
            var volumeType=FindRuntimeType("UnityEngine.Rendering.Volume");
            var profileType=FindRuntimeType("UnityEngine.Rendering.VolumeProfile");
            if(volumeType==null||profileType==null)return;

            var go=new GameObject("Valoria · SurfaceCell · URP finish");
            go.transform.SetParent(root,true);
            var volume=go.AddComponent(volumeType);
            SetReflectedMember(volume,"isGlobal",true);
            SetReflectedMember(volume,"priority",90f);
            var profile=ScriptableObject.CreateInstance(profileType);
            SetReflectedMember(volume,"profile",profile);

            var tone=AddVolumeOverride(profile,"UnityEngine.Rendering.Universal.Tonemapping");
            OverrideVolumeEnum(tone,"mode","ACES");

            var color=AddVolumeOverride(profile,"UnityEngine.Rendering.Universal.ColorAdjustments");
            OverrideVolumeValue(color,"postExposure",.05f);
            OverrideVolumeValue(color,"contrast",10f);
            OverrideVolumeValue(color,"saturation",4f);
            OverrideVolumeValue(color,"colorFilter",new Color(1.0f,.992f,.975f,1f));

            var bloom=AddVolumeOverride(profile,"UnityEngine.Rendering.Universal.Bloom");
            OverrideVolumeValue(bloom,"threshold",1.16f);
            OverrideVolumeValue(bloom,"intensity",.10f);
            OverrideVolumeValue(bloom,"scatter",.48f);

            var vignette=AddVolumeOverride(profile,"UnityEngine.Rendering.Universal.Vignette");
            OverrideVolumeValue(vignette,"intensity",.055f);
            OverrideVolumeValue(vignette,"smoothness",.27f);
        }

        static void SetReflectedMember(object target,string name,object value)
        {
            if(target==null)return;
            var type=target.GetType();
            var property=type.GetProperty(name);
            if(property!=null&&property.CanWrite){property.SetValue(target,value,null);return;}
            var field=type.GetField(name);
            if(field!=null)field.SetValue(target,value);
        }

        static object AddVolumeOverride(object profile,string overrideTypeName)
        {
            if(profile==null)return null;
            var overrideType=FindRuntimeType(overrideTypeName);
            if(overrideType==null)return null;
            foreach(var method in profile.GetType().GetMethods())
            {
                if(method.Name!="Add")continue;
                var parameters=method.GetParameters();
                if(!method.IsGenericMethod&&parameters.Length==2&&parameters[0].ParameterType==typeof(Type))
                    return method.Invoke(profile,new object[]{overrideType,true});
                if(method.IsGenericMethodDefinition&&parameters.Length==1&&parameters[0].ParameterType==typeof(bool))
                    return method.MakeGenericMethod(overrideType).Invoke(profile,new object[]{true});
            }
            return null;
        }

        static object GetReflectedMember(object target,string name)
        {
            if(target==null)return null;
            var type=target.GetType();
            var property=type.GetProperty(name);
            if(property!=null)return property.GetValue(target,null);
            var field=type.GetField(name);
            return field!=null?field.GetValue(target):null;
        }

        static void OverrideVolumeValue(object component,string parameterName,object value)
        {
            var parameter=GetReflectedMember(component,parameterName);
            if(parameter==null)return;
            foreach(var method in parameter.GetType().GetMethods())
            {
                if(method.Name!="Override")continue;
                var args=method.GetParameters();
                if(args.Length!=1)continue;
                var expected=args[0].ParameterType;
                object converted=value;
                if(value!=null&&!expected.IsInstanceOfType(value))
                {
                    try{converted=Convert.ChangeType(value,expected);}
                    catch{continue;}
                }
                method.Invoke(parameter,new[]{converted});
                return;
            }
        }

        static void OverrideVolumeEnum(object component,string parameterName,string enumName)
        {
            var parameter=GetReflectedMember(component,parameterName);
            if(parameter==null)return;
            var valueProperty=parameter.GetType().GetProperty("value");
            if(valueProperty==null||!valueProperty.PropertyType.IsEnum)return;
            var value=Enum.Parse(valueProperty.PropertyType,enumName);
            OverrideVolumeValue(component,parameterName,value);
        }

        static void PolishImportedSurface(Renderer renderer,Color tint)
        {
            var mats=renderer.sharedMaterials;
            for(int i=0;i<mats.Length;i++)
            {
                var source=mats[i];
                if(source==null)continue;
                var copy=new Material(source){name="Valoria SurfaceCell · "+source.name};
                if(copy.HasProperty("_BaseColor"))
                {
                    var baseColor=copy.GetColor("_BaseColor");
                    copy.SetColor("_BaseColor",new Color(baseColor.r*tint.r,baseColor.g*tint.g,baseColor.b*tint.b,baseColor.a));
                }
                else if(copy.HasProperty("_Color"))
                {
                    var baseColor=copy.GetColor("_Color");
                    copy.SetColor("_Color",new Color(baseColor.r*tint.r,baseColor.g*tint.g,baseColor.b*tint.b,baseColor.a));
                }
                if(copy.HasProperty("_Smoothness"))copy.SetFloat("_Smoothness",.055f);
                if(copy.HasProperty("_BumpScale"))copy.SetFloat("_BumpScale",1.08f);
                if(copy.HasProperty("_OcclusionStrength"))copy.SetFloat("_OcclusionStrength",1.0f);
                if(copy.HasProperty("_Metallic"))copy.SetFloat("_Metallic",0f);
                mats[i]=copy;
            }
            renderer.sharedMaterials=mats;
        }

        public static void AddCoherentCastleProofForGate()
        {
            if(root==null)throw new InvalidOperationException("Valoria visual integration root is not initialized.");
            IntegrateCoherentCastleProof();
            Finish();
        }

        static void IntegrateCoherentCastleProof()
        {
            // Coherent Castle v2: keep the proven single-family geometry, but replace the toy/pastel
            // palette with Valoria's PBR material language. Gameplay target/stair/route remain untouched.
            Suppress("Bastion ·","Valoria · Bastion hero","Valoria · rescued hero flank","Valoria · TargetFrame");

            var gate=Resources.Load<GameObject>("Valoria/CoherentKitProof/gate");
            var wall=Resources.Load<GameObject>("Valoria/CoherentKitProof/wall");
            var tower=Resources.Load<GameObject>("Valoria/CoherentKitProof/tower-square");
            var towerTop=Resources.Load<GameObject>("Valoria/CoherentKitProof/tower-square-top-roof-high-windows");
            var hills=Resources.Load<GameObject>("Valoria/CoherentKitProof/ground-hills");
            var rocks=Resources.Load<GameObject>("Valoria/CoherentKitProof/rocks-large");
            if(gate==null||wall==null||tower==null||towerTop==null)
                throw new InvalidOperationException("Coherent Castle Proof resources were not staged/imported.");

            var art=ValoriaExternalAssetLibrary.Load();
            var stone=ValoriaKit.ExternalPbrSurfaceMaterial("stone",
                new Color(.72f,.69f,.62f),new Vector2(2.7f,2.7f),.055f,1.05f)
                ?? ValoriaKit.PbrSurfaceMaterial(art!=null?art.ValoriaStoneSurface:null,
                    new Color(.67f,.64f,.57f),new Vector2(2.9f,2.9f),.055f,1.0f);
            var darkStone=ValoriaKit.ExternalPbrSurfaceMaterial("stone",
                new Color(.49f,.49f,.46f),new Vector2(3.2f,3.2f),.045f,1.08f)
                ?? ValoriaKit.DetailedSurfaceMaterial(new Color(.38f,.39f,.38f),"stone",new Vector2(3.2f,3.2f),1.0f);
            var earth=ValoriaKit.ExternalPbrSurfaceMaterial("dirt",
                new Color(.50f,.43f,.33f),new Vector2(4.0f,4.0f),.025f,.92f)
                ?? ValoriaKit.PbrSurfaceMaterial(art!=null?art.ValoriaDirtSurface:null,
                    new Color(.48f,.41f,.31f),new Vector2(4.0f,4.0f),.025f,.9f);
            var slate=ValoriaKit.DetailedSurfaceMaterial(new Color(.18f,.20f,.22f),"slate",new Vector2(4.2f,4.2f),.70f);

            var p=new Vector3(0f,2.95f,7.25f);

            CoherentCastlePiece("Valoria · CoherentProof · hill seat",hills,p+new Vector3(0f,-.52f,.45f),
                8.2f,1.25f,0f,earth);
            CoherentCastlePiece("Valoria · CoherentProof · rock west",rocks,p+new Vector3(-4.0f,-.15f,-.2f),
                2.25f,1.10f,22f,darkStone);
            CoherentCastlePiece("Valoria · CoherentProof · rock east",rocks,p+new Vector3(4.0f,-.15f,.05f),
                2.15f,1.05f,198f,darkStone);

            CoherentCastlePiece("Valoria · CoherentProof · gate",gate,p+new Vector3(0f,0f,-2.65f),
                2.25f,3.40f,0f,stone);
            CoherentCastlePiece("Valoria · CoherentProof · front wall west",wall,p+new Vector3(-2.15f,.02f,-2.20f),
                2.75f,2.70f,0f,stone);
            CoherentCastlePiece("Valoria · CoherentProof · front wall east",wall,p+new Vector3(2.15f,.02f,-2.20f),
                2.75f,2.70f,180f,stone);

            CoherentCastlePiece("Valoria · CoherentProof · keep base",tower,p+new Vector3(-.35f,.02f,.85f),
                2.55f,4.05f,0f,stone);
            CoherentCastlePiece("Valoria · CoherentProof · keep crown",towerTop,p+new Vector3(-.35f,3.35f,.85f),
                2.55f,2.55f,0f,slate);
            CoherentCastlePiece("Valoria · CoherentProof · west tower",tower,p+new Vector3(-3.00f,.02f,-.25f),
                1.85f,3.55f,0f,stone);
            CoherentCastlePiece("Valoria · CoherentProof · west crown",towerTop,p+new Vector3(-3.00f,2.95f,-.25f),
                1.85f,1.90f,0f,slate);
            CoherentCastlePiece("Valoria · CoherentProof · east tower",tower,p+new Vector3(2.90f,.02f,.10f),
                1.70f,3.10f,0f,stone);
            CoherentCastlePiece("Valoria · CoherentProof · east crown",towerTop,p+new Vector3(2.90f,2.58f,.10f),
                1.70f,1.75f,0f,slate);

            CoherentCastlePiece("Valoria · CoherentProof · rear wall west",wall,p+new Vector3(-1.95f,.02f,2.10f),
                2.55f,2.45f,180f,stone);
            CoherentCastlePiece("Valoria · CoherentProof · rear wall east",wall,p+new Vector3(1.85f,.02f,2.15f),
                2.55f,2.45f,180f,stone);

            WarmLight("Valoria · CoherentProof · gate warmth",p+new Vector3(0f,1.55f,-3.10f),
                new Color(1.0f,.56f,.25f),.20f,4.0f);
        }

        static GameObject CoherentCastlePiece(string name,GameObject source,Vector3 ground,float footprint,float height,float yaw,Material overrideMaterial)
        {
            if(source==null)return null;
            var go=ValoriaKit.BenchmarkPiece(name,source,ground,footprint,height,Quaternion.Euler(0f,yaw,0f));
            if(go==null)return null;
            go.transform.SetParent(root,true);
            foreach(var collider in go.GetComponentsInChildren<Collider>(true))collider.enabled=false;
            foreach(var hotspot in go.GetComponentsInChildren<WorldHotspot>(true))Object.DestroyImmediate(hotspot);
            if(overrideMaterial!=null)
            {
                foreach(var renderer in go.GetComponentsInChildren<Renderer>(true))
                {
                    int count=Mathf.Max(1,renderer.sharedMaterials.Length);
                    var mats=new Material[count];
                    for(int i=0;i<count;i++)mats[i]=overrideMaterial;
                    renderer.sharedMaterials=mats;
                }
            }
            return go;
        }

        public static void AddSlavicDistrictProofForGate()
        {
            if(root==null)throw new InvalidOperationException("Valoria visual integration root is not initialized.");
            IntegrateSlavicDistrictProof();
            Finish();
        }

        static void IntegrateSlavicDistrictProof()
        {
            // Civil-language proof only. Keep the dedicated Aserradero/Cuartel/Granero PBR visuals,
            // the current Bastion, and every gameplay target/collider exactly as authored.
            Suppress("Valoria · reused civil house",
                "VPD · west rebuilders home","VPD · west rebuilders upper dwelling",
                "VPD · upper dwelling","VPD · upper civil house fallback");

            var houseA=Resources.Load<GameObject>("Valoria/SlavicCoherentProof/house-a");
            var houseB=Resources.Load<GameObject>("Valoria/SlavicCoherentProof/house-b");
            var road=Resources.Load<GameObject>("Valoria/SlavicCoherentProof/cobble");
            var rock=Resources.Load<GameObject>("Valoria/SlavicCoherentProof/rock");
            var tree=Resources.Load<GameObject>("Valoria/SlavicCoherentProof/tree");
            if(houseA==null||houseB==null||road==null)
                throw new InvalidOperationException("Slavic District proof resources were not staged/imported.");

            // Replace only the visibly provisional residential language with two authored variants
            // from one family. Placement follows the already-reserved residential footprints.
            var homes=new[]{
                new Vector4(-12.00f,-3.25f,168f,0f),
                new Vector4(-15.25f,-3.05f,192f,0f),
                new Vector4(-13.25f, 2.45f,174f,0f),
                new Vector4(-17.35f, 2.15f,198f,0f),
                new Vector4(-15.80f, 5.55f,166f,1f),
                new Vector4(-18.05f, 6.05f,194f,1f)
            };
            for(int i=0;i<homes.Length;i++)
            {
                var h=homes[i];
                float y=h.w>.5f?(i==5?1.18f:1.28f):.38f;
                var source=i%2==0?houseA:houseB;
                SlavicProofPiece("Valoria · SlavicProof · civil house "+i,source,
                    new Vector3(h.x,y,h.y),i>=4?2.70f:2.55f,i>=4?3.25f:3.05f,h.z);
            }

            // Upper east dwelling belongs to the same language instead of the procedural House().
            SlavicProofPiece("Valoria · SlavicProof · upper east dwelling",houseB,
                new Vector3(5.15f,2.89f,7.15f),2.70f,3.10f,188f);

            // Keep the certified road underneath; a small authored cobble skin improves material continuity.
            for(int i=0;i<6;i++)
                SlavicProofPiece("Valoria · SlavicProof · cobble "+i,road,
                    new Vector3((i%2==0?-.10f:.12f),.425f,-6.0f+i*1.05f),
                    2.75f,.16f,i%2==0?0f:180f);

            // Natural edge dressing is sparse and grouped, not citywide clutter.
            if(rock!=null)
            {
                SlavicProofPiece("Valoria · SlavicProof · rock west",rock,
                    new Vector3(-9.4f,.10f,-5.9f),2.35f,1.10f,38f);
                SlavicProofPiece("Valoria · SlavicProof · rock east",rock,
                    new Vector3(9.4f,.10f,-6.1f),2.25f,1.05f,218f);
            }
            if(tree!=null)
            {
                foreach(var spec in new[]{
                    new Vector4(-10.1f,-1.1f,12f,1.0f),new Vector4(-10.7f,3.2f,-18f,.92f),
                    new Vector4(10.1f,-1.4f,-18f,.96f),new Vector4(10.8f,3.4f,17f,.90f)})
                    SlavicProofPiece("Valoria · SlavicProof · edge tree",tree,
                        new Vector3(spec.x,.15f,spec.y),1.55f*spec.w,3.35f*spec.w,spec.z);
            }
        }

        static GameObject SlavicProofPiece(string name,GameObject source,Vector3 ground,float footprint,float height,float yaw)
        {
            if(source==null)return null;
            // Slavic source materials are authored much brighter than the current Valoria exposure.
            // Preserve texture identity but modulate the response by role so the family belongs to the same world.
            Color tint;
            var lower=name.ToLowerInvariant();
            if(lower.Contains("tree"))tint=new Color(.48f,.58f,.44f,1f);
            else if(lower.Contains("rock"))tint=new Color(.62f,.61f,.56f,1f);
            else if(lower.Contains("cobble"))tint=new Color(.68f,.65f,.59f,1f);
            else tint=new Color(.62f,.57f,.49f,1f);
            var go=ValoriaKit.BenchmarkPieceModulated(name,source,ground,footprint,height,
                Quaternion.Euler(0f,yaw,0f),tint);
            if(go==null)return null;
            go.transform.SetParent(root,true);
            foreach(var collider in go.GetComponentsInChildren<Collider>(true))collider.enabled=false;
            foreach(var hotspot in go.GetComponentsInChildren<WorldHotspot>(true))Object.DestroyImmediate(hotspot);
            return go;
        }

        public static void AddProductionCellForGate(PlayerState state)
        {
            if(root==null)throw new InvalidOperationException("Valoria visual integration root is not initialized.");
            IntegrateProductionCell(state,ValoriaExternalAssetLibrary.Load());
            Finish();
        }

        static void IntegrateProductionCell(PlayerState state,ValoriaExternalAssetLibrary art)
        {
            // PRODUCTION CELL v1 — deliberately small finished slice around the lower civic street.
            // The test changes no building parcels, route colliders, hotspots or camera. It demonstrates
            // the new production order: circulation/ground -> shared surface language -> occupation props
            // -> atmospheric motion/light. Only after this reads better should the recipe scale citywide.

            // 1) Ground/circulation polish: irregular widening over the already-certified lower entry.
            var plaza=ValoriaGroundKit.StreetBlendWidening("Valoria · ProductionCell · lower civic apron",
                new Vector3(0,.432f,-6.55f),5.55f,2.75f,0f);
            plaza.transform.SetParent(root,true);
            var westWear=ValoriaGroundKit.TerraceFloor("Valoria · ProductionCell · sawmill yard wear",
                new Vector3(-5.05f,.415f,-4.85f),2.15f,2.55f,7f);
            westWear.transform.SetParent(root,true);
            var eastWear=ValoriaGroundKit.TerraceFloor("Valoria · ProductionCell · barracks yard wear",
                new Vector3(5.10f,.415f,-5.35f),2.05f,2.35f,-8f);
            eastWear.transform.SetParent(root,true);

            // 2) Repeated shared stone language frames the cell without creating a new wall/route.
            StonePiece(3,"Valoria · ProductionCell · west low curb",
                new Vector3(-2.25f,.42f,-7.0f),new Vector3(.22f,.20f,1.45f),0f);
            StonePiece(3,"Valoria · ProductionCell · east low curb",
                new Vector3(2.25f,.42f,-7.0f),new Vector3(.22f,.20f,1.45f),180f);
            StonePiece(4,"Valoria · ProductionCell · sawmill corner",
                new Vector3(-5.45f,.39f,-5.75f),new Vector3(.82f,.24f,.95f),180f);
            StonePiece(4,"Valoria · ProductionCell · barracks corner",
                new Vector3(5.55f,.39f,-6.05f),new Vector3(.82f,.24f,.95f),90f);

            // 3) Set dressing uses the existing production library; no new generated geometry.
            var crate=Resources.Load<GameObject>("Valoria/UrbanProps/Crate");
            var barrel=Resources.Load<GameObject>("Valoria/UrbanProps/Barrel");
            var sack=Resources.Load<GameObject>("Valoria/UrbanProps/Sack");
            foreach(var spec in new[]{
                new Vector4(-5.70f,-5.20f,.52f,18f),new Vector4(-4.95f,-5.62f,.48f,-12f),
                new Vector4(5.85f,-5.62f,.50f,21f),new Vector4(4.95f,-5.82f,.46f,-19f)})
                Piece("Valoria · ProductionCell · work crate",crate,new Vector3(spec.x,.42f,spec.y),spec.z,spec.z,spec.w,new Color(.73f,.63f,.49f));
            Piece("Valoria · ProductionCell · sawmill barrel",barrel,new Vector3(-5.23f,.42f,-5.10f),.43f,.62f,9f,new Color(.68f,.58f,.45f));
            Piece("Valoria · ProductionCell · barracks barrel",barrel,new Vector3(5.22f,.42f,-5.45f),.43f,.62f,-11f,new Color(.68f,.58f,.45f));
            if(state.BastionLevel>=3)
            {
                Piece("Valoria · ProductionCell · provision sack west",sack,new Vector3(-4.72f,.42f,-5.23f),.52f,.38f,0f,new Color(.78f,.70f,.56f));
                Piece("Valoria · ProductionCell · provision sack east",sack,new Vector3(4.70f,.42f,-5.52f),.52f,.38f,0f,new Color(.78f,.70f,.56f));
            }
            if(art!=null&&art.Firewood!=null)
                Piece("Valoria · ProductionCell · active timber stock",art.Firewood,
                    new Vector3(-6.05f,.42f,-4.95f),1.20f,.64f,24f,new Color(.70f,.60f,.46f));

            // 4) Life and movement: sparse readable silhouettes and chimney smoke, not random clutter.
            foreach(var p in new[]{
                new Vector3(-3.85f,.43f,-5.85f),new Vector3(-2.95f,.43f,-6.65f),
                new Vector3(3.55f,.43f,-6.20f),new Vector3(4.15f,.43f,-5.25f)})
                Civilian(p);
            for(int i=0;i<4;i++)
            {
                var smokePos=new Vector3(-7.05f+(i%2==0?-.08f:.09f),3.30f+i*.32f,-2.55f+(i%3-1)*.05f);
                Primitive("Valoria · ProductionCell · sawmill smoke",PrimitiveType.Sphere,smokePos,
                    Vector3.one*(.26f+i*.055f),new Color(.43f-i*.018f,.42f-i*.018f,.40f-i*.015f));
            }

            // 5) Focal hierarchy: restrained heraldry and warm pools identify the working/military fronts.
            Flag("Valoria · ProductionCell · civic blue standard",new Vector3(-1.72f,.44f,-7.15f),Blue,1.65f);
            Flag("Valoria · ProductionCell · military blue standard",new Vector3(4.45f,.44f,-5.05f),Blue,1.45f);
            WarmLight("Valoria · ProductionCell · lower street warmth",new Vector3(-.35f,1.20f,-6.15f),
                new Color(1.0f,.54f,.25f),.58f,2.25f);
        }

        public static void AddStoneArchitectureForGate()
        {
            if(root==null)throw new InvalidOperationException("Valoria visual integration root is not initialized.");
            IntegrateStoneArchitecture();
            Finish();
        }

        static void IntegrateStoneArchitecture()
        {
            // 01 CornerWallL — close real civilian/work courts and articulate terrace corners without forming a defensive maze.
            StoneArchitecturePiece("CornerWallL","Valoria · StoneArch · corner · west work court",
                new Vector3(-11.55f,.30f,4.05f),1.85f,112f);
            StoneArchitecturePiece("CornerWallL","Valoria · StoneArch · corner · west lower court",
                new Vector3(-15.35f,.31f,-4.25f),1.70f,18f);

            // 05 HighStraightWall — one restrained rear terrace limit. Keep military frontage open/readable.
            StoneArchitecturePiece("HighStraightWall","Valoria · StoneArch · high wall · west terrace back",
                new Vector3(-17.55f,.28f,.55f),2.65f,88f);

            // 02 RockToWallTransition — only the two dedicated-building seams that improved the frame.
            StoneArchitecturePiece("RockToWallTransition","Valoria · StoneArch · rock wall seam · sawmill",
                new Vector3(-6.15f,.10f,-2.55f),1.95f,28f);
            StoneArchitecturePiece("RockToWallTransition","Valoria · StoneArch · rock wall seam · barracks",
                new Vector3(5.85f,.10f,-3.75f),1.85f,205f);
        }

        public static void AddTerrainTerraceCitywideForGate()
        {
            if(root==null)throw new InvalidOperationException("Valoria visual integration root is not initialized.");
            IntegrateTerrainTerraceCitywide();
            Finish();
        }

        static void IntegrateTerrainTerraceCitywide()
        {
            // Citywide composition: each module is tucked under an existing certified parcel/court.
            // We align the module TOP to the authored terrace elevation so it reads as buried support,
            // never as a detached foreground pedestal. Roads, stairs, floors, hotspots and colliders remain untouched.
            TerrainTerraceTop("BroadRockPlatform","Valoria · TerrainTerrace · west lower housing shelf",
                new Vector3(-14.2f,0,-3.45f),.40f,4.15f,8f);
            TerrainTerraceTop("SteppedRockTerrace","Valoria · TerrainTerrace · west middle housing rise",
                new Vector3(-15.25f,0,2.85f),.42f,4.05f,96f);
            TerrainTerraceTop("BroadRockPlatform","Valoria · TerrainTerrace · west upper housing shelf",
                new Vector3(-16.75f,0,5.78f),1.24f,4.10f,5f);
            TerrainTerraceTop("SteppedRockTerrace","Valoria · TerrainTerrace · upper civil support",
                new Vector3(-5.25f,0,5.92f),2.72f,3.55f,92f);
            TerrainTerraceTop("BroadRockPlatform","Valoria · TerrainTerrace · workshop edge support",
                new Vector3(-8.95f,0,-4.72f),.42f,3.35f,14f);
            TerrainTerraceTop("SteppedRockTerrace","Valoria · TerrainTerrace · east training edge support",
                new Vector3(8.55f,0,-5.92f),.42f,3.25f,270f);
            TerrainTerraceTop("BroadRockPlatform","Valoria · TerrainTerrace · east upper retaining shelf",
                new Vector3(4.35f,0,4.85f),1.55f,3.40f,182f);
        }

        static void TerrainTerraceTop(string resource,string name,Vector3 xzAnchor,float topY,float targetSpan,float yaw)
        {
            var source=Resources.Load<GameObject>("Valoria/TerrainTerraceKit_v1/"+resource);
            if(source==null)throw new InvalidOperationException("Missing certified Terrain Terrace v1 resource: "+resource);
            var go=Object.Instantiate(source);go.name=name;go.transform.rotation=Quaternion.Euler(0,yaw,0);
            var bounds=Bounds(go);float span=Mathf.Max(bounds.size.x,bounds.size.z);
            if(span<=.001f)throw new InvalidOperationException("Terrain Terrace v1 resource has empty bounds: "+resource);
            go.transform.localScale*=targetSpan/span;
            bounds=Bounds(go);
            go.transform.position+=new Vector3(xzAnchor.x-bounds.center.x,topY-bounds.max.y,xzAnchor.z-bounds.center.z);
            go.transform.SetParent(root,true);
            foreach(var collider in go.GetComponentsInChildren<Collider>(true))collider.enabled=false;
            NormalizeTerrainTerrace(go);
        }

        static void NormalizeTerrainTerrace(GameObject go)
        {
            if(terrainTerraceStone==null)
            {
                terrainTerraceStone=new Material(Shader.Find("Universal Render Pipeline/Lit")){name="Valoria Terrain Terrace · buried support stone"};
                terrainTerraceStone.SetColor("_BaseColor",new Color(.285f,.275f,.245f,1f));
                terrainTerraceStone.SetFloat("_Smoothness",.025f);
                terrainTerraceStone.SetFloat("_Metallic",0f);
            }
            foreach(var renderer in go.GetComponentsInChildren<Renderer>(true))
            {
                var mats=renderer.sharedMaterials;
                for(int i=0;i<mats.Length;i++)mats[i]=terrainTerraceStone;
                renderer.sharedMaterials=mats;
            }
        }

        static void IntegrateRescuedTerrainSeams()
        {
            // Certified seam filler used only as partially buried visual support for the existing Bastion shelf.
            // It does not define a route, floor, landing or collision surface.
            RescuedTerrainSeam("Valoria · rescued seam · bastion west shelf",new Vector3(-4.55f,0,4.85f),2.63f,2.55f,72f);
            RescuedTerrainSeam("Valoria · rescued seam · bastion east shelf",new Vector3(4.65f,0,4.95f),2.60f,2.45f,288f);
        }

        static void RescuedTerrainSeam(string name,Vector3 xzAnchor,float topY,float targetSpan,float yaw)
        {
            var source=Resources.Load<GameObject>("Valoria/Rescued/RockTerrainSeamFiller");
            if(source==null)throw new InvalidOperationException("Missing certified RockTerrainSeamFiller resource.");
            var go=Object.Instantiate(source);go.name=name;go.transform.rotation=Quaternion.Euler(0,yaw,0);
            var bounds=Bounds(go);float span=Mathf.Max(bounds.size.x,bounds.size.z);
            if(span<=.001f)throw new InvalidOperationException("RockTerrainSeamFiller has empty renderer bounds.");
            go.transform.localScale*=targetSpan/span;
            bounds=Bounds(go);
            // Top-align, then bury a small extra slice so the rectangular source termination never reads as a pedestal.
            go.transform.position+=new Vector3(xzAnchor.x-bounds.center.x,topY-bounds.max.y-.12f,xzAnchor.z-bounds.center.z);
            go.transform.SetParent(root,true);
            foreach(var collider in go.GetComponentsInChildren<Collider>(true))collider.enabled=false;
            NormalizeTerrainTerrace(go);
        }

        static void StoneArchitecturePiece(string resource,string name,Vector3 groundAnchor,float targetSpan,float yaw)
        {
            var source=Resources.Load<GameObject>("Valoria/StoneArchitectureKit_v1/"+resource);
            if(source==null)throw new InvalidOperationException("Missing production Stone Architecture v1 resource: "+resource);
            var go=Object.Instantiate(source);go.name=name;go.transform.rotation=Quaternion.Euler(0,yaw,0);
            var bounds=Bounds(go);
            float span=Mathf.Max(bounds.size.x,bounds.size.z);
            if(span<=.001f)throw new InvalidOperationException("Stone Architecture v1 resource has empty bounds: "+resource);
            go.transform.localScale*=targetSpan/span;
            bounds=Bounds(go);
            go.transform.position+=groundAnchor-new Vector3(bounds.center.x,bounds.min.y,bounds.center.z);
            go.transform.SetParent(root,true);
            NormalizeStoneArchitecture(go,resource);
        }

        static void NormalizeStoneArchitecture(GameObject go,string resource)
        {
            foreach(var renderer in go.GetComponentsInChildren<Renderer>(true))
            {
                var mats=renderer.sharedMaterials;
                for(int i=0;i<mats.Length;i++)
                {
                    // Reuse the already-certified StoneKit material when available. Geometry keeps
                    // Stone Architecture identity while surface response joins Valoria's existing masonry.
                    mats[i]=sharedStone!=null
                        ? sharedStone
                        : ValoriaKit.SurfaceMaterial(new Color(.42f,.40f,.35f),"stone",new Vector2(3,3));
                }
                renderer.sharedMaterials=mats;
            }
        }

        static readonly Dictionary<string,Material> groundSkins=new();
        static void BlendStrategicGround()
        {
            var route=GameObject.Find("Frontier · march trail");if(route!=null)SkinRoute(route);
            foreach(var spec in new[]{new[]{"Frontier · quarry shelf","stone"},new[]{"Frontier · corrupted shelf","slate"}})
            {
                var go=GameObject.Find(spec[0]);var renderer=go!=null?go.GetComponent<MeshRenderer>():null;
                if(renderer!=null)renderer.sharedMaterial=BlendedGround(spec[1]);
            }
        }
        static void SkinRoute(GameObject route)
        {
            foreach(var renderer in route.GetComponentsInChildren<MeshRenderer>(true))
                if(renderer.gameObject.name.Contains(" · trail "))renderer.sharedMaterial=BlendedGround("trail");
        }
        static Material BlendedGround(string family)
        {
            if(groundSkins.TryGetValue(family,out var cached)&&cached!=null)return cached;
            const int size=256;var texture=new Texture2D(size,size,TextureFormat.RGBA32,true){name="Eldoria blended "+family,wrapMode=TextureWrapMode.Clamp};
            var pixels=new Color[size*size];
            var color=family=="trail"?new Color(.19f,.15f,.10f):family=="stone"?new Color(.25f,.25f,.21f):new Color(.17f,.15f,.19f);
            for(int y=0;y<size;y++)for(int x=0;x<size;x++)
            {
                float u=x/(float)(size-1)*2-1,v=y/(float)(size-1)*2-1;
                float grain=Mathf.PerlinNoise(x*.16f+7,y*.16f+13),broad=Mathf.PerlinNoise(x*.037f+4,y*.037f+9);
                float edge=family=="trail"?Mathf.Abs(u):Mathf.Sqrt(u*u+v*v);
                float alpha=1-Mathf.SmoothStep(0,1,(edge+(broad-.5f)*.18f-.54f)/.43f);
                float rut=family=="trail"?Mathf.Exp(-Mathf.Pow((u-.38f)/.12f,2))+Mathf.Exp(-Mathf.Pow((u+.38f)/.12f,2)):0;
                var c=color*(.87f+grain*.16f+broad*.08f-rut*.08f);c.a=alpha;pixels[y*size+x]=c;
            }
            texture.SetPixels(pixels);texture.Apply(true,false);
            var m=new Material(Shader.Find("Universal Render Pipeline/Lit")){name="Eldoria surface "+family+" · blended terrain",renderQueue=3000};
            m.SetTexture("_BaseMap",texture);m.SetColor("_BaseColor",Color.white);m.SetFloat("_Surface",1);m.SetFloat("_Blend",0);
            m.SetFloat("_SrcBlend",(float)UnityEngine.Rendering.BlendMode.SrcAlpha);m.SetFloat("_DstBlend",(float)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            m.SetFloat("_ZWrite",0);m.SetFloat("_Smoothness",.015f);m.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");m.SetOverrideTag("RenderType","Transparent");
            groundSkins[family]=m;return m;
        }

        static void ReplaceCityTrees()
        {
            var trunks=new List<MeshRenderer>();
            foreach(var r in Object.FindObjectsByType<MeshRenderer>(FindObjectsSortMode.None))
            {
                var n=r.gameObject.name;
                if(n.EndsWith(" · trunk")&&(n.StartsWith("Valoria ·")||n.StartsWith("VPD ·"))&&n.Contains("pine"))trunks.Add(r);
            }
            int i=0;
            foreach(var r in trunks)
            {
                float scale=r.transform.localScale.y/.9f;
                var p=r.transform.position-Vector3.up*(.9f*scale);
                string prefix=r.gameObject.name.Substring(0,r.gameObject.name.Length-" · trunk".Length);
                Suppress(prefix);
                Imported("Valoria · authored evergreen",i++%2==0?"Tree01A":"Tree01B",p,2.0f*scale,3.1f*scale,i*43,new Color(.30f,.43f,.23f),true);
            }
        }

        static void DressBastion()
        {
            // Hero-frame Bastion: retain the certified functional Bastion/hotspot underneath,
            // but replace the old generic Mega* envelope with the project's authored stone family.
            // No visual piece below owns collision or interaction; Finish() strips both.
            // Suppress the entire previous visual shell, not just selected legacy pieces.
            // The gameplay target/hotspot is an invisible object and remains authoritative.
            Suppress("Bastion ·");
            // Load the concrete Resources prefabs directly. The legacy ScriptableObject references for
            // this very old pack can deserialize as prefab-asset handles that cannot be Instantiate<GameObject>
            // in editor batchmode, even though the Resources prefabs themselves are valid GameObjects.
            var stoneTower=Resources.Load<GameObject>("Valoria/Stone_Tower");
            var stoneWall=Resources.Load<GameObject>("Valoria/Stone_Wall");
            var stoneGate=Resources.Load<GameObject>("Valoria/Stone_Gate");
            if(stoneTower==null||stoneWall==null||stoneGate==null)
                throw new InvalidOperationException("Hero-frame Bastion requires Valoria/Stone_Tower, Stone_Wall and Stone_Gate resources.");

            var p=new Vector3(0,3.0f,7.25f);

            // Strong recessed front gate aligned to the certified stair mouth.
            Piece("Valoria · Bastion hero gate",stoneGate,p+new Vector3(0,.08f,-3.00f),
                4.85f,4.05f,0,new Color(.82f,.80f,.74f));

            // Connected front/side masonry keeps the entrance legible while giving the keep a real base.
            Piece("Valoria · Bastion hero wall west",stoneWall,p+new Vector3(-3.05f,.08f,-2.20f),
                3.85f,3.35f,3,new Color(.80f,.78f,.72f));
            Piece("Valoria · Bastion hero wall east",stoneWall,p+new Vector3(3.05f,.08f,-2.12f),
                3.75f,3.30f,-4,new Color(.80f,.78f,.72f));
            Piece("Valoria · Bastion hero wall rear",stoneWall,p+new Vector3(.15f,.18f,2.25f),
                4.70f,3.45f,180,new Color(.76f,.75f,.70f));

            // Asymmetric tower hierarchy: one dominant rear keep, two unequal supporting masses.
            Piece("Valoria · Bastion hero tower crown",stoneTower,p+new Vector3(-.55f,.20f,1.10f),
                4.10f,6.20f,-2,new Color(.84f,.82f,.76f));
            Piece("Valoria · Bastion hero tower west",stoneTower,p+new Vector3(-3.25f,.12f,.25f),
                2.85f,5.05f,5,new Color(.80f,.78f,.72f));
            Piece("Valoria · Bastion hero tower east",stoneTower,p+new Vector3(3.05f,.10f,.65f),
                2.65f,4.45f,-7,new Color(.78f,.77f,.71f));

            // Rock-to-architecture seams break the pedestal read without creating any route/floor.
            StoneArchitecturePiece("RockToWallTransition","Valoria · Bastion hero rock seam west",
                p+new Vector3(-4.15f,-2.72f,-.70f),2.55f,58f);
            StoneArchitecturePiece("RockToWallTransition","Valoria · Bastion hero rock seam east",
                p+new Vector3(4.15f,-2.72f,-.35f),2.40f,238f);
        }

        static void UnifyLandscape(bool city)
        {
            if(landscape==null)landscape=LandscapeMaterial();
            foreach(var r in Object.FindObjectsByType<MeshRenderer>(FindObjectsSortMode.None))
            {
                string n=r.gameObject.name;
                bool isGround=city?(n.Contains("valley floor")||n.Contains("expansion terrain")||n.Contains("authored apron")||n.Contains("rebuilders terrace")||n.Contains("rebuilders upper shelf")||n.Contains("organic civic ground")):
                    (n.StartsWith("Frontier ·")&&!n.Contains("quarry")&&!n.Contains("corrupted")&&(n.Contains("floor")||n.Contains("earth")||n.Contains("approach")));
                if(!isGround||!r.enabled)continue;
                var filter=r.GetComponent<MeshFilter>();
                if(filter==null||filter.sharedMesh==null)continue;
                var mesh=Object.Instantiate(filter.sharedMesh);
                var v=mesh.vertices;var uv=new Vector2[v.Length];
                if(!city&&n=="Frontier · valley floor")
                {
                    // Extend only the collision-free geographic sheet beyond all official zooms.
                    for(int i=0;i<v.Length;i++){v[i].x*=3f;v[i].z*=3f;}
                    mesh.vertices=v;mesh.RecalculateBounds();
                }
                for(int i=0;i<v.Length;i++)
                {var w=filter.transform.TransformPoint(v[i]);uv[i]=new Vector2((w.x/180f+.5f)/4f,(w.z/180f+.5f)/4f);}
                mesh.uv=uv;filter.sharedMesh=mesh;r.sharedMaterial=landscape;
            }
        }
        static Material LandscapeMaterial()
        {
            const int size=512;
            var texture=new Texture2D(size,size,TextureFormat.RGB24,true){name="Eldoria continuous soil and moss",wrapMode=TextureWrapMode.Clamp};
            var pixels=new Color[size*size];
            var soil=new Color(.28f,.255f,.20f);var grass=new Color(.205f,.25f,.17f);var gravel=new Color(.35f,.34f,.285f);
            for(int y=0;y<size;y++)for(int x=0;x<size;x++)
            {
                float n=Mathf.PerlinNoise(x*.018f+14.3f,y*.018f+5.7f);
                float detail=Mathf.PerlinNoise(x*.27f+3.1f,y*.27f+11.2f);
                var c=Color.Lerp(soil,grass,Mathf.SmoothStep(0,1,(n-.29f)*2.4f));
                c=Color.Lerp(c,gravel,Mathf.Max(0,Mathf.PerlinNoise(x*.04f+31,y*.04f+47)-.58f)*1.0f);
                pixels[y*size+x]=c*(.92f+detail*.14f);
            }
            texture.SetPixels(pixels);texture.Apply(true,false);
            var mat=new Material(Shader.Find("Universal Render Pipeline/Lit")){name="Eldoria surface earth · continuous terrain"};
            mat.SetTexture("_BaseMap",texture);mat.SetTextureScale("_BaseMap",new Vector2(4,4));
            mat.SetColor("_BaseColor",Color.white);mat.SetFloat("_Smoothness",.02f);
            return mat;
        }

        static void ComposeHeroFrame(PlayerState state, ValoriaExternalAssetLibrary art)
        {
            // VALORIA HERO FRAME — whole-frame art direction only.
            // This layer owns no gameplay topology, collision, routes, floors or hotspots.
            // It deliberately attacks the benchmark gaps that cannot be solved by adding one more isolated prefab:
            // depth, skyline hierarchy, warm/cool separation, inhabited density and a readable hero focal point.

            // Pull the current flat owner-review rig toward a brighter cinematic dusk without returning
            // to the previously rejected dark neutral-overcast candidate.
            RenderSettings.ambientMode=UnityEngine.Rendering.AmbientMode.Flat;
            // Keep the hero frame cinematic, but stay above the certified gameplay readability floor.
            RenderSettings.ambientLight=new Color(.80f,.81f,.80f);
            RenderSettings.fog=true;
            RenderSettings.fogMode=FogMode.Linear;
            RenderSettings.fogColor=new Color(.61f,.64f,.66f);
            RenderSettings.fogStartDistance=30f;
            RenderSettings.fogEndDistance=82f;
            var camera=Camera.main;
            if(camera!=null)
            {
                camera.backgroundColor=RenderSettings.fogColor;
                camera.clearFlags=CameraClearFlags.SolidColor;
            }
            foreach(var light in Object.FindObjectsByType<Light>(FindObjectsSortMode.None))
            {
                if(light.name!="Valoria · amber dusk")continue;
                light.color=new Color(1.0f,.86f,.72f);
                light.intensity=1.48f;
                light.shadowStrength=.61f;
                light.transform.rotation=Quaternion.Euler(50f,-31f,0);
            }

            HeroValleyTerrain();

            // Mid-distance geology creates a second depth plane between the inhabited city and the far
            // VisualWorld mountains. Keep this layer on the already-proven neutral rock inventory:
            // the terrain-pack cliff materials were too saturated in the real capture and are deliberately rejected.
            foreach(var p in new[]{
                new Vector3(-15.8f,-.20f,13.4f),new Vector3(14.8f,-.18f,13.8f),
                new Vector3(-19.0f,-.22f,7.4f),new Vector3(18.6f,-.22f,8.2f)})
            {
                Imported("Valoria · hero frame buried ridge","Rock02",p,
                    p.z>10?5.4f:4.3f,p.z>10?2.25f:1.85f,p.x*9f,new Color(.36f,.36f,.33f),false);
            }

            // Inhabited middle-distance: roofs behind roofs, but never across the certified central route.
            // Existing Slavic support architecture is intentionally subordinate to the dedicated hero buildings.
            if(art!=null&&art.SlavicHouse!=null)
            {
                var homes=new[]{
                    new Vector4(-10.8f,10.1f,2.05f,-18f),
                    new Vector4(-14.0f,8.8f,1.92f,8f),
                    new Vector4(10.9f,10.4f,1.96f,21f),
                    new Vector4(14.0f,8.5f,1.82f,-9f),
                    new Vector4(-18.2f,5.2f,1.72f,16f),
                    new Vector4(17.4f,5.6f,1.70f,-17f),
                    new Vector4(-12.6f,4.6f,1.78f,-31f),
                    new Vector4(12.8f,4.8f,1.76f,33f),
                    new Vector4(-9.8f,14.1f,1.70f,11f),
                    new Vector4(9.6f,14.4f,1.72f,-13f)};
                for(int i=0;i<homes.Length;i++)
                {
                    var h=homes[i];
                    Piece("Valoria · hero frame inhabited roofline",art.SlavicHouse,
                        new Vector3(h.x,.40f,h.y),2.05f,h.z,h.w,new Color(.60f,.55f,.47f));
                }
            }

            // A controlled tree line frames the city instead of filling it randomly.
            foreach(var spec in new[]{
                new Vector4(-18.2f,13.7f,1.30f,0),new Vector4(-14.9f,14.4f,1.48f,1),
                new Vector4(-11.9f,13.2f,1.22f,0),new Vector4(11.8f,13.5f,1.24f,1),
                new Vector4(15.0f,14.3f,1.46f,0),new Vector4(18.0f,13.1f,1.26f,1),
                new Vector4(-20.0f,7.0f,1.18f,1),new Vector4(19.8f,7.5f,1.20f,0)})
            {
                Imported("Valoria · hero frame pine",spec.w>.5f?"Tree01B":"Tree01A",
                    new Vector3(spec.x,.03f,spec.y),spec.z,2.65f,spec.x*13f,new Color(.31f,.42f,.28f),true);
            }

            // Secondary ridge vegetation creates parallax and hides the remaining board-like horizon.
            for(int i=0;i<18;i++)
            {
                float side=i<9?-1f:1f;
                int k=i%9;
                float x=side*(17.0f+k*.95f);
                float z=2.0f+(k%5)*3.35f;
                Imported("Valoria · hero frame valley pine",k%2==0?"Tree01A":"Tree01B",
                    new Vector3(x,.12f,z),1.05f+(k%3)*.14f,2.55f,side*(12f+k*17f),
                    new Color(.28f,.38f,.25f),true);
            }

            // Bastion becomes the visual thesis: restrained heraldry and warm occupation cues draw the eye
            // without changing its mesh, footprint or interaction target.
            Flag("Valoria · Bastion banner west",new Vector3(-2.55f,3.05f,5.18f),Blue,3.05f);
            Flag("Valoria · Bastion banner east",new Vector3(2.48f,3.00f,5.24f),Blue,2.85f);
            WarmLight("Valoria · Bastion gate warmth",new Vector3(0,3.25f,4.25f),new Color(1.0f,.53f,.24f),1.65f,5.2f);
            WarmLight("Valoria · Bastion upper warmth",new Vector3(-.65f,6.65f,7.2f),new Color(1.0f,.59f,.30f),.95f,4.0f);

            // Sparse occupation cues: warm pools at real working districts, not a blanket of lights.
            WarmLight("Valoria · sawmill work glow",new Vector3(-7.2f,1.45f,-3.15f),new Color(1.0f,.50f,.21f),1.15f,3.5f);
            WarmLight("Valoria · barracks court glow",new Vector3(7.3f,1.55f,-4.45f),new Color(1.0f,.56f,.27f),1.00f,3.2f);
            WarmLight("Valoria · west quarter hearth",new Vector3(-13.8f,1.45f,-2.4f),new Color(1.0f,.52f,.24f),.82f,2.8f);
            WarmLight("Valoria · upper quarter hearth",new Vector3(-5.3f,3.85f,6.3f),new Color(1.0f,.55f,.27f),.78f,2.6f);

            // Existing production props make the lower city read occupied from strategic distance.
            if(art!=null&&art.Firewood!=null)
            {
                Piece("Valoria · hero frame sawmill timber",art.Firewood,new Vector3(-8.2f,.39f,-5.2f),1.45f,.72f,-13f,new Color(.72f,.62f,.48f));
                Piece("Valoria · hero frame rebuild timber",art.Firewood,new Vector3(-13.4f,.39f,1.45f),1.25f,.62f,18f,new Color(.69f,.59f,.46f));
            }
            if(state.BastionLevel>=3)
                WarmLight("Valoria · granary activity glow",new Vector3(-17.2f,1.25f,-4.65f),new Color(1.0f,.58f,.28f),.72f,2.5f);
        }

        static void ComposeCompactFootprintReframeV1(PlayerState state, ValoriaExternalAssetLibrary art)
        {
            // Toolchain Automation v2 / environment_composition — iteration 4.
            // Existing geometry only: subtract peripheral urban mass, compress the readable core and
            // expose empty development shelves. Gameplay authority remains untouched.

            // Freeze lateral urban expansion. Functional buildings keep their presentation, but generic
            // residential/mid-tier dressing beyond the compact core is hidden from the official frame.
            foreach(var r in Object.FindObjectsByType<Renderer>(FindObjectsSortMode.None))
            {
                if(r==null||!r.enabled||!r.gameObject.activeInHierarchy)continue;
                var b=r.bounds;string chain="";
                for(var t=r.transform;t!=null;t=t.parent)chain+="|"+t.name.ToLowerInvariant();
                bool functional=chain.Contains("aserradero")||chain.Contains("cuartel")||chain.Contains("granary")||chain.Contains("granero");
                bool genericUrban=chain.Contains("mid-tier")||chain.Contains("reused civil house")||
                    chain.Contains("inhabited roofline")||chain.Contains("civil house")||chain.Contains("west inhabited")||
                    chain.Contains("civilian");
                bool peripheralParcel=chain.Contains("valoria mid-tier · d2")||chain.Contains("valoria mid-tier · d3")||
                    chain.Contains("valoria mid-tier · w1")||chain.Contains("valoria mid-tier · w2")||
                    chain.Contains("valoria mid-tier · w3");
                bool heroFrameHouse=chain.Contains("hero frame inhabited roofline");
                bool natural=chain.Contains("terrainterrace")||chain.Contains("rock")||chain.Contains("pine")||
                    chain.Contains("tree")||chain.Contains("ground")||chain.Contains("route")||chain.Contains("street")||
                    chain.Contains("stair")||chain.Contains("bastion")||chain.Contains("seam");
                bool outer=Mathf.Abs(b.center.x)>8.15f&&b.center.z>-7.0f&&b.center.z<12.5f;
                bool keepPeripheralTerrace=peripheralParcel&&(chain.Contains("buried broadrockplatform")||chain.Contains("buried steppedrockterrace"));
                bool residualOuterBuilding=outer&&!functional&&!natural&&b.size.y>.75f&&b.size.y<6.5f&&Mathf.Max(b.size.x,b.size.z)<7.5f;
                if((peripheralParcel&&!keepPeripheralTerrace)||heroFrameHouse||(outer&&genericUrban)||residualOuterBuilding)r.enabled=false;
            }

            // Final subtraction of residual outer house silhouettes that survived legacy naming.
            foreach(var r in Object.FindObjectsByType<Renderer>(FindObjectsSortMode.None))
            {
                if(r==null||!r.enabled||!r.gameObject.activeInHierarchy)continue;
                var b=r.bounds;string chain="";
                for(var t=r.transform;t!=null;t=t.parent)chain+="|"+t.name.ToLowerInvariant();
                bool functional=chain.Contains("aserradero")||chain.Contains("cuartel")||chain.Contains("granary")||chain.Contains("granero")||chain.Contains("bastion");
                bool environment=chain.Contains("rock")||chain.Contains("terrain")||chain.Contains("pine")||chain.Contains("tree")||
                    chain.Contains("ground")||chain.Contains("route")||chain.Contains("street")||chain.Contains("stair")||
                    chain.Contains("platform")||chain.Contains("terrace")||chain.Contains("seam");
                bool outer=Mathf.Abs(b.center.x)>9.35f&&b.center.z>-6.5f&&b.center.z<11.5f;
                bool buildingScale=b.size.y>.9f&&b.size.y<6.5f&&Mathf.Max(b.size.x,b.size.z)<7.5f;
                if(outer&&buildingScale&&!functional&&!environment)r.enabled=false;
            }

            // Remove lateral residential light rhythm so the eye returns to the Bastion/core.
            foreach(var l in Object.FindObjectsByType<Light>(FindObjectsSortMode.None))
            {
                if(l==null||!l.enabled)continue;
                string n=l.name.ToLowerInvariant();
                if(Mathf.Abs(l.transform.position.x)>8.1f&&(n.Contains("quarter")||n.Contains("hearth")))l.enabled=false;
            }

            // Outer frame becomes natural transition and visibly undeveloped territory.
            foreach(var p in new[]{
                new Vector4(-14.9f,-.12f,.7f,18f),new Vector4(-14.2f,-.10f,4.3f,62f),new Vector4(-13.3f,.06f,8.0f,114f),
                new Vector4(14.9f,-.12f,.9f,198f),new Vector4(14.2f,-.10f,4.5f,242f),new Vector4(13.3f,.06f,8.2f,294f)})
                Imported("Valoria · CompactFootprint · edge geology","Rock02",new Vector3(p.x,p.y,p.z),
                    3.55f,1.18f,p.w,new Color(.34f,.34f,.31f),false);
            foreach(var p in new[]{
                new Vector4(-16.2f,1.7f,1.18f,0),new Vector4(-15.3f,5.4f,1.25f,1),new Vector4(-14.4f,9.5f,1.18f,0),
                new Vector4(16.2f,1.9f,1.18f,1),new Vector4(15.3f,5.6f,1.25f,0),new Vector4(14.4f,9.7f,1.18f,1)})
                Imported("Valoria · CompactFootprint · edge pine",p.w>.5f?"Tree01B":"Tree01A",
                    new Vector3(p.x,.05f,p.y),p.z,2.65f,p.x*9f,new Color(.29f,.39f,.26f),true);

            // Deliberately empty buildable terraces: closer to the core and clearly not occupied by houses.
            TerrainTerraceTop("BroadRockPlatform","Valoria · CompactFootprint · west future terrace",
                new Vector3(-7.15f,0,1.55f),.43f,3.05f,12f);
            TerrainTerraceTop("BroadRockPlatform","Valoria · CompactFootprint · east future terrace",
                new Vector3(7.10f,0,1.70f),.43f,3.05f,168f);
            TerrainTerraceTop("SteppedRockTerrace","Valoria · CompactFootprint · west upper growth shelf",
                new Vector3(-4.75f,0,4.65f),1.48f,2.85f,96f);
            TerrainTerraceTop("SteppedRockTerrace","Valoria · CompactFootprint · east upper growth shelf",
                new Vector3(4.75f,0,4.72f),1.48f,2.85f,264f);

            // Central geology shelves visually connect lower city -> stairs -> Bastion without adding buildings.
            TerrainTerraceTop("BroadRockPlatform","Valoria · CompactFootprint · central middle shelf",
                new Vector3(0f,0,4.85f),1.55f,5.25f,0f);
            TerrainTerraceTop("SteppedRockTerrace","Valoria · CompactFootprint · Bastion lower shelf",
                new Vector3(0f,0,6.55f),2.58f,5.65f,90f);
            foreach(var p in new[]{
                new Vector4(-5.55f,.22f,3.55f,40f),new Vector4(5.55f,.22f,3.60f,220f),
                new Vector4(-4.15f,1.15f,5.35f,72f),new Vector4(4.15f,1.15f,5.40f,252f)})
                Imported("Valoria · CompactFootprint · buried core rock","Rock02",
                    new Vector3(p.x,p.y,p.z),1.75f,.68f,p.w,new Color(.33f,.33f,.30f),false);

            // Ceremonial approach: restrained paving rhythm, leaving the central route fully readable.
            for(int i=0;i<4;i++)
            {
                float z=-.25f+i*1.04f;
                StonePiece(2,"Valoria · CompactFootprint · processional edge west",
                    new Vector3(-2.45f,.445f,z),new Vector3(1.05f,.045f,.88f),7f);
                StonePiece(2,"Valoria · CompactFootprint · processional edge east",
                    new Vector3(2.45f,.445f,z),new Vector3(1.05f,.045f,.88f),-7f);
            }
            if(art!=null&&art.Firewood!=null)
                Piece("Valoria · CompactFootprint · restrained reconstruction stock",art.Firewood,
                    new Vector3(-4.45f,.42f,.80f),.78f,.43f,12f,new Color(.67f,.57f,.43f));

            RenderSettings.fogStartDistance=26f;RenderSettings.fogEndDistance=72f;
            WarmLight("Valoria · CompactFootprint · central lower warmth",
                new Vector3(0f,1.12f,.75f),new Color(1f,.50f,.22f),.38f,2.35f);
        }

        static void HeroValleyTerrain()
        {
            // Purely visual heightfield. The certified floors/routes/colliders remain authoritative above it.
            // The centre stays below the playable shelves; elevation appears only toward the outer valley.
            const int nx=41,nz=35;
            const float minX=-36f,maxX=36f,minZ=-20f,maxZ=40f;
            var vertices=new Vector3[nx*nz];
            var uv=new Vector2[vertices.Length];
            var triangles=new int[(nx-1)*(nz-1)*6];
            for(int z=0;z<nz;z++)
            {
                float tz=z/(float)(nz-1);
                float wz=Mathf.Lerp(minZ,maxZ,tz);
                for(int x=0;x<nx;x++)
                {
                    float tx=x/(float)(nx-1);
                    float wx=Mathf.Lerp(minX,maxX,tx);
                    float side=Mathf.Clamp01((Mathf.Abs(wx)-17f)/17f);
                    float rear=Mathf.Clamp01((wz-15f)/23f);
                    float front=Mathf.Clamp01((-wz-10f)/10f);
                    float rise=side*side*4.8f+rear*rear*5.2f+front*front*1.4f;
                    float noise=(Mathf.Sin(wx*.23f)+Mathf.Sin(wz*.29f)+Mathf.Sin((wx+wz)*.13f))*.16f;
                    float centreMask=Mathf.Clamp01((Mathf.Abs(wx)-11f)/10f);
                    float y=-.16f+rise+noise*Mathf.Lerp(.18f,1f,centreMask);
                    vertices[z*nx+x]=new Vector3(wx,y,wz);
                    uv[z*nx+x]=new Vector2(tx*18f,tz*15f);
                }
            }
            int ti=0;
            for(int z=0;z<nz-1;z++)
                for(int x=0;x<nx-1;x++)
                {
                    int a=z*nx+x,b=a+1,d=(z+1)*nx+x,cc=d+1;
                    triangles[ti++]=a;triangles[ti++]=d;triangles[ti++]=b;
                    triangles[ti++]=b;triangles[ti++]=d;triangles[ti++]=cc;
                }
            var mesh=new Mesh{name="Valoria Hero Frame · valley heightfield"};
            mesh.vertices=vertices;mesh.uv=uv;mesh.triangles=triangles;
            mesh.RecalculateNormals();mesh.RecalculateBounds();
            var go=new GameObject("Valoria · Hero Frame valley terrain");
            go.transform.SetParent(root,true);
            go.AddComponent<MeshFilter>().sharedMesh=mesh;
            go.AddComponent<MeshRenderer>().sharedMaterial=
                ValoriaKit.SurfaceMaterial(new Color(.32f,.30f,.25f,1f),"earth",new Vector2(18f,15f));
        }

        static void WarmLight(string name,Vector3 p,Color color,float intensity,float range)
        {
            var go=new GameObject(name);
            go.transform.position=p;
            go.transform.SetParent(root,true);
            var light=go.AddComponent<Light>();
            light.type=LightType.Point;
            light.color=color;
            light.intensity=intensity;
            light.range=range;
            light.shadows=LightShadows.None;
        }

        static void Imported(string name,string resource,Vector3 p,float footprint,float height,float yaw,Color tint,bool foliage)
        {
            var source=Resources.Load<GameObject>("WorldInventory/"+resource);
            if(source==null)throw new InvalidOperationException("Missing recovered inventory: "+resource);
            var go=ValoriaKit.BenchmarkPiece(name,source,p,footprint,height,Quaternion.Euler(0,yaw,0));
            if(go==null)throw new InvalidOperationException("Empty recovered inventory: "+resource);
            go.transform.SetParent(root,true);
            Normalize(go,tint,foliage,resource);
        }
        static void Piece(string name,GameObject source,Vector3 p,float footprint,float height,float yaw,Color tint)
        {
            if(source==null)return;
            var go=ValoriaKit.BenchmarkPieceModulated(name,source,p,footprint,height,Quaternion.Euler(0,yaw,0),tint);
            if(go!=null)
            {
                go.transform.SetParent(root,true);
                foreach(var renderer in go.GetComponentsInChildren<Renderer>(true))
                {
                    var mats=renderer.sharedMaterials;
                    for(int i=0;i<mats.Length;i++)
                    {
                        if(mats[i]==null)continue;
                        // Hero Bastion preserves the authored stone textures but shifts the importer palette
                        // away from blue-grey so it belongs to Valoria's warm natural masonry family.
                        if(name.Contains("Bastion hero"))
                        {
                            var sourceMat=mats[i];
                            Texture baseMap=null,normal=null;
                            foreach(string property in new[]{"_Texture","_BaseMap","_MainTex","_BaseColorTexture","baseColorTexture"})
                                if(sourceMat.HasProperty(property)&&sourceMat.GetTexture(property)!=null){baseMap=sourceMat.GetTexture(property);break;}
                            foreach(string property in new[]{"_BumpMap","_NormalMap","normalTexture"})
                                if(sourceMat.HasProperty(property)&&sourceMat.GetTexture(property)!=null){normal=sourceMat.GetTexture(property);break;}
                            var lit=Shader.Find("Universal Render Pipeline/Lit");
                            var copy=new Material(lit){name="Valoria Hero Bastion · "+sourceMat.name};
                            if(baseMap!=null)copy.SetTexture("_BaseMap",baseMap);
                            if(normal!=null){copy.SetTexture("_BumpMap",normal);copy.EnableKeyword("_NORMALMAP");}
                            // Warm limestone/aged granite: brighter than the rejected blue-black Toon response,
                            // but still clearly heavier than the civilian plaster around it.
                            copy.SetColor("_BaseColor",sourceMat.name.Contains("Color")
                                ?new Color(.72f,.64f,.52f,1f)
                                :new Color(.58f,.55f,.48f,1f));
                            copy.SetFloat("_Metallic",0f);
                            copy.SetFloat("_Smoothness",.035f);
                            mats[i]=copy;
                        }
                        // This recovered prototype's baked atlas has unmapped black UV regions.
                        // Keep its certified geometry, use the existing city surface vocabulary.
                        else if(name.Contains("rescued hero"))
                            mats[i]=ValoriaKit.SurfaceMaterial(mats[i].name.Contains("Rock")?new Color(.25f,.26f,.24f):new Color(.34f,.32f,.27f),
                                mats[i].name.Contains("Rock")?"earth":"stone",new Vector2(3,3));
                        else if(mats[i].HasProperty("_BaseColorFactor"))
                        {
                            var copy=new Material(mats[i]);var c=copy.GetColor("_BaseColorFactor");
                            copy.SetColor("_BaseColorFactor",new Color(c.r*tint.r,c.g*tint.g,c.b*tint.b,c.a));mats[i]=copy;
                        }
                    }
                    renderer.sharedMaterials=mats;
                }
            }
            else if(name.Contains("rescued hero"))throw new InvalidOperationException("Recovered hero geometry has empty renderer bounds");
        }
        static void Normalize(GameObject go,Color tint,bool foliage,string resource)
        {
            foreach(var r in go.GetComponentsInChildren<Renderer>(true))
            {
                var mf=r.GetComponent<MeshFilter>();
                int count=mf!=null&&mf.sharedMesh!=null?mf.sharedMesh.subMeshCount:Mathf.Max(1,r.sharedMaterials.Length);
                var originals=r.sharedMaterials;var mats=new Material[count];
                for(int i=0;i<count;i++)
                {
                    var source=i<originals.Length?originals[i]:null;
                    // Untextured legacy FBX materials carry a white importer fallback.
                    // These neutral masonry families share Valoria's aged-stone surface.
                    if(resource.StartsWith("Mega")||resource=="Arch_Gothic"||resource=="Wall_Broken"||resource=="Column_Round")
                    {
                        mats[i]=ValoriaKit.SurfaceMaterial(go.name.StartsWith("Valoria")?tint:tint.linear,"stone",new Vector2(3,3));
                        continue;
                    }
                    bool leaves=foliage&&i==0;
                    string key=resource+"/"+i+"/"+ColorUtility.ToHtmlStringRGB(tint);
                    if(adapted.TryGetValue(key,out var cached)&&cached!=null){mats[i]=cached;continue;}
                    var m=new Material(Shader.Find("Universal Render Pipeline/Lit")){name="Eldoria adapted · "+resource+" "+i};
                    Texture texture=null,normal=null;
                    if(source!=null&&source.HasProperty("_Albedo"))
                    {
                        // Holotna RGB images are channel masks, not ordinary albedo maps.
                        // Keep their certified URP graph and recolor its palette inputs.
                        Object.Destroy(m);m=new Material(source){name="Eldoria adapted · "+resource+" "+i};
                        Color primary=foliage?(leaves?new Color(.19f,.29f,.13f):new Color(.24f,.17f,.10f)):tint;
                        if(m.HasProperty("_Color"))m.SetColor("_Color",primary.linear);
                        if(m.HasProperty("_Primary_Color"))m.SetColor("_Primary_Color",primary.linear);
                        if(m.HasProperty("_Secondary_Color"))m.SetColor("_Secondary_Color",(foliage?new Color(.24f,.20f,.12f):tint*.72f).linear);
                        if(m.HasProperty("_Tertiary_Color"))m.SetColor("_Tertiary_Color",(tint*.48f).linear);
                        if(foliage)
                        {
                            foreach(string wind in new[]{"_Bend_Strength","_Bend_Distortion","_Wiggle_Strength"})if(m.HasProperty(wind))m.SetFloat(wind,0);
                        }
                        adapted[key]=m;mats[i]=m;continue;
                    }
                    else
                    {
                        foreach(string property in new[]{"_BaseMap","_MainTex","_Albedo"})
                            if(source!=null&&source.HasProperty(property)&&source.GetTexture(property)!=null){texture=source.GetTexture(property);break;}
                        if(texture!=null&&texture.name.ToLowerInvariant().Contains("white"))texture=null;
                        if(texture!=null)m.SetColor("_BaseColor",tint.linear);
                        else m=ValoriaKit.SurfaceMaterial(tint,"stone",new Vector2(3,3));
                    }
                    if(texture!=null)m.SetTexture("_BaseMap",texture);
                    if(normal!=null){m.SetTexture("_BumpMap",normal);m.EnableKeyword("_NORMALMAP");}
                    m.SetFloat("_Smoothness",.025f);m.SetFloat("_Metallic",0);
                    if(leaves){m.SetFloat("_AlphaClip",1);m.SetFloat("_Cutoff",.35f);m.EnableKeyword("_ALPHATEST_ON");m.SetFloat("_Cull",0);}
                    adapted[key]=m;mats[i]=m;
                }
                r.sharedMaterials=mats;
            }
        }
        static void StonePiece(int index,string name,Vector3 p,Vector3 dimensions,float yaw)
        {
            string[] names={"piece_01_10364tris","piece_02_12602tris","piece_03_6433tris","piece_04_7824tris","piece_05_4966tris","piece_06_7602tris"};
            var source=Resources.Load<GameObject>("Valoria/StoneKit/"+names[index-1]);
            if(source==null)throw new InvalidOperationException("Missing certified StoneKit piece: "+index);
            var go=Object.Instantiate(source);go.name=name;go.transform.rotation=Quaternion.identity;
            var bounds=Bounds(go);var s=bounds.size;
            go.transform.localScale=Vector3.Scale(go.transform.localScale,new Vector3(dimensions.x/s.x,dimensions.y/s.y,dimensions.z/s.z));
            bounds=Bounds(go);go.transform.position+=new Vector3(-bounds.center.x,-bounds.min.y,-bounds.center.z);
            go.transform.RotateAround(Vector3.zero,Vector3.up,yaw);go.transform.position+=p;go.transform.SetParent(root,true);
            foreach(var r in go.GetComponentsInChildren<Renderer>(true))
            {
                var materials=r.sharedMaterials;
                for(int i=0;i<materials.Length;i++)
                {
                    if(materials[i]==null)continue;
                    // Six source GLBs embed byte-identical albedo/normal/MR atlases.
                    // Share the first material across instances rather than duplicating GPU textures.
                    if(sharedStone!=null){materials[i]=sharedStone;continue;}
                    var m=new Material(materials[i]);
                    if(m.HasProperty("_BaseColor"))m.SetColor("_BaseColor",new Color(.48f,.46f,.40f));
                    if(m.HasProperty("_BaseColorFactor"))m.SetColor("_BaseColorFactor",new Color(.48f,.46f,.40f));
                    if(m.HasProperty("_Smoothness"))m.SetFloat("_Smoothness",.02f);
                    sharedStone=m;materials[i]=m;
                }
                r.sharedMaterials=materials;
            }
        }
        static Bounds Bounds(GameObject go)
        {
            var rs=go.GetComponentsInChildren<Renderer>(true);var b=rs[0].bounds;
            for(int i=1;i<rs.Length;i++)b.Encapsulate(rs[i].bounds);return b;
        }
        static void Suppress(params string[] prefixes)
        {
            foreach(var r in Object.FindObjectsByType<Renderer>(FindObjectsSortMode.None))
            foreach(var prefix in prefixes)
            for(var t=r.transform;t!=null;t=t.parent)if(t.name.StartsWith(prefix)){r.enabled=false;break;}
        }
        static void Finish()
        {
            foreach(var c in root.GetComponentsInChildren<Collider>(true))c.enabled=false;
            foreach(var h in root.GetComponentsInChildren<WorldHotspot>(true))Object.DestroyImmediate(h);
        }
        static void CityOrigin(Vector3 p)
        {
            // Player City v1 deliberately uses one universal strategic city mesh for every player.
            // Player/ally/enemy identity, name, alliance and future skins remain UI/state concerns.
            var source=Resources.Load<GameObject>("WorldPlayerCity/PlayerCity_v1");
            if(source==null)throw new InvalidOperationException("Missing production universal Player City v1 resource.");
            var go=Object.Instantiate(source);
            go.name="4X · player city · production v1";
            go.transform.rotation=Quaternion.identity;
            var bounds=Bounds(go);
            float span=Mathf.Max(bounds.size.x,bounds.size.z);
            if(span<=.001f)throw new InvalidOperationException("Player City v1 has empty renderer bounds.");
            go.transform.localScale*=3.2f/span;
            bounds=Bounds(go);
            go.transform.position+=p-new Vector3(bounds.center.x,bounds.min.y,bounds.center.z);
            go.transform.SetParent(root,true);
            NormalizePlayerCity(go);
            Flag("4X · Valoria standard",p+new Vector3(.9f,.15f,.5f),Blue,1.4f);
        }

        static void NormalizePlayerCity(GameObject go)
        {
            // Force a stable URP/Lit response for the world map. The glTF shader used by the
            // source asset overexposes under VisualWorld lighting; keep its texture maps but
            // move them onto Eldoria's standard lit material response.
            var lit=Shader.Find("Universal Render Pipeline/Lit");
            if(lit==null)throw new InvalidOperationException("URP Lit shader unavailable for Player City v1.");
            foreach(var renderer in go.GetComponentsInChildren<Renderer>(true))
            {
                var mats=renderer.sharedMaterials;
                for(int i=0;i<mats.Length;i++)
                {
                    var source=mats[i];
                    if(source==null)continue;
                    Texture baseMap=null,normal=null;
                    foreach(string property in new[]{"_BaseMap","_MainTex","_BaseColorTexture","baseColorTexture"})
                        if(source.HasProperty(property)&&source.GetTexture(property)!=null){baseMap=source.GetTexture(property);break;}
                    foreach(string property in new[]{"_BumpMap","_NormalMap","normalTexture"})
                        if(source.HasProperty(property)&&source.GetTexture(property)!=null){normal=source.GetTexture(property);break;}
                    var m=new Material(lit){name="Eldoria · Player City v1 · "+source.name};
                    if(baseMap!=null)m.SetTexture("_BaseMap",baseMap);
                    if(normal!=null)
                    {
                        m.SetTexture("_BumpMap",normal);
                        m.EnableKeyword("_NORMALMAP");
                    }
                    m.SetColor("_BaseColor",new Color(.58f,.55f,.49f,1f));
                    m.SetFloat("_Metallic",0f);
                    m.SetFloat("_Smoothness",.035f);
                    mats[i]=m;
                }
                renderer.sharedMaterials=mats;
            }
        }
        static void March(Vector3 p)
        {
            // Small deliberate symbols; final animated troops remain a library gap.
            for(int i=0;i<5;i++)
            {
                var q=p+new Vector3((i%2)*.38f,0,(i/2)*.34f);
                Primitive("4X · deployed army · placeholder",PrimitiveType.Capsule,q+Vector3.up*.34f,new Vector3(.17f,.35f,.17f),new Color(.28f,.30f,.31f));
            }
            Flag("4X · march heading",p+new Vector3(-.18f,0,.22f),Blue,1.1f);
        }
        static void Ruin(Vector3 p)
        {
            Imported("4X · neutral ruin arch","Arch_Gothic",p,2.2f,2.8f,-12,new Color(.36f,.35f,.29f),false);
            Imported("4X · neutral ruin wall","Wall_Broken",p+new Vector3(1.1f,0,.6f),1.85f,1.2f,78,new Color(.32f,.32f,.27f),false);
            Imported("4X · neutral ruin column","Column_Round",p+new Vector3(-1.1f,0,.3f),.5f,1.35f,0,new Color(.35f,.34f,.29f),false);
        }
        static void Beast(string name,Vector3 p,bool boar)
        {
            // Compact silhouette placeholders explicitly tracked against World P0 Beast Kit.
            var c=boar?new Color(.31f,.25f,.18f):new Color(.39f,.41f,.40f);
            Primitive(name+" body",PrimitiveType.Sphere,p+new Vector3(0,.45f,0),new Vector3(boar?.95f:.53f,boar?.54f:.43f,1.35f),c);
            Primitive(name+" head",PrimitiveType.Sphere,p+new Vector3(0,.56f,-.6f),new Vector3(.48f,.45f,.52f),c*.9f);
            for(int i=0;i<4;i++)Primitive(name+" leg",PrimitiveType.Capsule,p+new Vector3(i%2==0?-.25f:.25f,.23f,i<2?-.35f:.38f),new Vector3(.15f,.26f,.16f),c*.8f);
            if(boar)for(int i=0;i<2;i++)Primitive(name+" tusk",PrimitiveType.Capsule,p+new Vector3(i==0?-.20f:.20f,.49f,-.82f),new Vector3(.08f,.18f,.08f),new Color(.78f,.73f,.59f));
            else
            {
                for(int i=0;i<2;i++)Primitive(name+" ear",PrimitiveType.Cube,p+new Vector3(i==0?-.17f:.17f,.89f,-.53f),new Vector3(.13f,.23f,.13f),c*.75f);
                Primitive(name+" muzzle",PrimitiveType.Sphere,p+new Vector3(0,.53f,-.91f),new Vector3(.21f,.22f,.41f),c*.8f);
                Primitive(name+" tail",PrimitiveType.Capsule,p+new Vector3(0,.47f,.92f),new Vector3(.13f,.32f,.13f),c*.8f);
                root.GetChild(root.childCount-1).rotation=Quaternion.Euler(58,0,0);
            }
        }
        static void Civilian(Vector3 p)
        {
            Primitive("Valoria · worker silhouette · placeholder",PrimitiveType.Capsule,p+Vector3.up*.40f,new Vector3(.20f,.38f,.20f),new Color(.34f,.29f,.22f));
            Primitive("Valoria · worker head · placeholder",PrimitiveType.Sphere,p+Vector3.up*.86f,Vector3.one*.16f,new Color(.49f,.39f,.27f));
        }
        static void Flag(string name,Vector3 p,Color color,float height)
        {
            Primitive(name+" pole",PrimitiveType.Cylinder,p+Vector3.up*height*.5f,new Vector3(.035f,height*.5f,.035f),new Color(.25f,.20f,.13f));
            Primitive(name+" cloth",PrimitiveType.Cube,p+new Vector3(.23f,height*.77f,0),new Vector3(.45f,height*.30f,.035f),color);
        }
        static void Node(string label,Vector3 p,Color color,float radius)
        {
            var ring=new GameObject("4X · "+label+" strategic footprint").AddComponent<LineRenderer>();
            ring.transform.SetParent(root,true);ring.loop=true;ring.useWorldSpace=true;ring.positionCount=32;
            ring.startWidth=ring.endWidth=.065f;ring.sharedMaterial=ValoriaKit.Material(color);
            for(int i=0;i<32;i++){float a=i*Mathf.PI*2/32;ring.SetPosition(i,p+new Vector3(Mathf.Cos(a)*radius,0,Mathf.Sin(a)*radius));}
            var text=new GameObject("4X · "+label+" semantic label").AddComponent<TextMesh>();
            text.transform.SetParent(root,true);text.transform.position=p+new Vector3(0,.38f,-radius-.22f);
            text.transform.rotation=Quaternion.LookRotation(new Vector3(-20,-24,22));
            text.text=label;text.fontSize=64;text.characterSize=.10f;text.anchor=TextAnchor.MiddleCenter;
            text.color=new Color(.94f,.88f,.72f);
            // Strategic identity must survive the pale terrain at the mobile/far zooms.
            // A small camera-aligned backing follows the fixed view; it owns no interaction.
            var backing=GameObject.CreatePrimitive(PrimitiveType.Quad);
            backing.name="4X · "+label+" label backing";
            backing.transform.SetParent(root,true);
            backing.transform.SetPositionAndRotation(text.transform.position+text.transform.forward*.02f,text.transform.rotation);
            backing.transform.localScale=new Vector3(Mathf.Max(.92f,label.Length*.145f+.18f),.34f,1);
            backing.GetComponent<Renderer>().sharedMaterial=ValoriaKit.Material(new Color(.035f,.04f,.032f));
            backing.GetComponent<Collider>().enabled=false;
        }
        static void Primitive(string name,PrimitiveType type,Vector3 p,Vector3 size,Color color)
        {
            var go=GameObject.CreatePrimitive(type);go.name=name;go.transform.position=p;go.transform.localScale=size;
            go.GetComponent<Renderer>().sharedMaterial=ValoriaKit.Material(color);go.transform.SetParent(root,true);
            foreach(var c in go.GetComponentsInChildren<Collider>())c.enabled=false;
        }
    }
}
