using System;
using Eldoria.Domain;
using UnityEngine;
using UnityEngine.Rendering;
using Object = UnityEngine.Object;

namespace Eldoria.Presentation
{
    /// <summary>
    /// Production-oriented visual uplift over the accepted Flat Citadel macro-composition:
    /// keep the flat-city macro fixed; improve large visual systems without changing gameplay authority.
    /// Gameplay topology, hotspots, colliders and progression remain owned by the canonical scene.
    /// </summary>
    public static class ValoriaFlatCitadelProductionUpliftV1
    {
        public static bool Enabled = true;
        public const string RootName = "Valoria · Flat Citadel Production Uplift v1";
        public static int HiddenLegacyRenderers { get; private set; }
        public static int WallPieces { get; private set; }
        public static int FunctionalBuildings { get; private set; }
        public static int NaturePieces { get; private set; }
        public static int AuthoredWallModules { get; private set; }
        public static int AuthoredWallTowers { get; private set; }
        public static bool WallUpliftEnabled = true;
        public static bool GroundUpliftEnabled = false;
        public static bool BastionIntegrationUpliftEnabled = false;
        public static bool FunctionalBuildingUpliftEnabled = false;
        public static bool DressingUpliftEnabled = false;

        static readonly Color Earth = new Color(.43f,.37f,.27f,1f);
        static readonly Color WarmStone = new Color(.58f,.56f,.50f,1f);
        static readonly Color WallStone = new Color(.50f,.49f,.45f,1f);
        static readonly Color Blue = new Color(.16f,.28f,.44f,1f);
        static readonly Color Amber = new Color(1f,.56f,.20f,1f);

        public static void Build(Transform parent, PlayerState state)
        {
            if(!Enabled || parent==null) return;

            var old=GameObject.Find(RootName);
            if(old!=null) Object.DestroyImmediate(old);

            var root=new GameObject(RootName).transform;
            root.SetParent(parent,true);

            int clonedHeroPieces=CloneCanonicalHeroBastion(root,new Vector3(0f,-.58f,0f));
            HiddenLegacyRenderers=HideAllNonProofRenderers(root);

            if(GroundUpliftEnabled) BuildFlatCitySurfaceProduction(root);
            else BuildFlatCitySurfaceBaseline(root);
            if(GroundUpliftEnabled) BuildPrimaryAxisProduction(root);
            else BuildPrimaryAxisBaseline(root);
            if(GroundUpliftEnabled) BuildGroundProduction(root);
            BuildBastionRise(root);
            BuildFunctionalArchitecture(root,state);
            if(WallUpliftEnabled) BuildOuterWallProduction(root);
            else BuildOuterWallBaseline(root);
            BuildSparseNature(root);
            BuildLifeCues(root);

            DisableGameplay(root.gameObject);
        }

        static int CloneCanonicalHeroBastion(Transform proofRoot,Vector3 offset)
        {
            var sources=new System.Collections.Generic.HashSet<GameObject>();
            foreach(var t in Object.FindObjectsByType<Transform>(FindObjectsSortMode.None))
            {
                if(t==null||t==proofRoot)continue;
                string n=t.name.ToLowerInvariant();
                bool hero=n.StartsWith("bastion ·")||n.StartsWith("valoria · bastion hero");
                if(!hero)continue;

                // Clone only the highest object in a same-family chain to avoid duplicate child meshes.
                var p=t.parent;
                bool parentHero=false;
                while(p!=null)
                {
                    string pn=p.name.ToLowerInvariant();
                    if(pn.StartsWith("bastion ·")||pn.StartsWith("valoria · bastion hero")){parentHero=true;break;}
                    p=p.parent;
                }
                if(!parentHero && t.GetComponentInChildren<Renderer>(true)!=null)sources.Add(t.gameObject);
            }

            int count=0;
            foreach(var source in sources)
            {
                var clone=Object.Instantiate(source);
                clone.name="Valoria · Flat Citadel · hero clone · "+source.name;
                clone.transform.position+=offset;
                clone.transform.SetParent(proofRoot,true);
                foreach(var col in clone.GetComponentsInChildren<Collider>(true))Object.DestroyImmediate(col);
                foreach(var h in clone.GetComponentsInChildren<WorldHotspot>(true))Object.DestroyImmediate(h);
                count++;
            }
            return count;
        }

        static int HideAllNonProofRenderers(Transform proofRoot)
        {
            int count=0;
            foreach(var r in Object.FindObjectsByType<Renderer>(FindObjectsSortMode.None))
            {
                if(r==null||!r.enabled)continue;
                if(r.transform.IsChildOf(proofRoot))continue;
                r.enabled=false;
                count++;
            }
            return count;
        }

        static void BuildFlatCitySurfaceBaseline(Transform root)
        {
            // Large surrounding natural floor means the city no longer reads as a raised island/podium.
            AddSlab(root,"surrounding meadow",new Vector3(0,-.06f,1.0f),new Vector3(31f,.08f,26f),
                ValoriaKit.DetailedSurfaceMaterial(new Color(.34f,.40f,.27f,1f),"earth",new Vector2(6.5f,6.5f),.88f));

            // 82% of the useful footprint is one coherent, near-flat buildable plane.
            // The perimeter is intentionally irregular enough not to read as a rectangular test board.
            Vector2[] ring={
                new Vector2(-10.4f,-7.0f),new Vector2(-6.2f,-7.7f),new Vector2(-1.8f,-7.5f),
                new Vector2( 3.0f,-7.7f),new Vector2( 8.0f,-7.1f),new Vector2(10.4f,-5.0f),
                new Vector2(10.7f, 0.4f),new Vector2(10.2f, 6.2f),new Vector2( 7.7f, 9.7f),
                new Vector2( 2.9f,10.5f),new Vector2(-2.8f,10.4f),new Vector2(-7.9f, 9.5f),
                new Vector2(-10.3f,6.0f),new Vector2(-10.8f,.2f)
            };
            CreatePrism(root,"main buildable city plane",ring,.015f,.075f,
                ValoriaKit.DetailedSurfaceMaterial(Earth,"earth",new Vector2(3.2f,3.2f),.92f),
                ValoriaKit.DetailedSurfaceMaterial(Earth*.72f,"rock",new Vector2(3.0f,3.0f),1.0f));

            // Reserve parcels are readable but quiet. They are intentionally empty space for future growth.
            AddParcel(root,"west growth parcel",new Vector3(-6.6f,.105f,3.2f),new Vector3(4.0f,.05f,3.1f),new Color(.38f,.34f,.27f,1f));
            AddParcel(root,"east growth parcel",new Vector3( 6.5f,.105f,3.0f),new Vector3(4.1f,.05f,3.0f),new Color(.38f,.34f,.27f,1f));
        }

        static void BuildPrimaryAxisBaseline(Transform root)
        {
            var roadMat=ValoriaKit.DetailedSurfaceMaterial(new Color(.55f,.53f,.48f,1f),"stone",new Vector2(2.3f,2.3f),1.0f);

            AddSlab(root,"gate road",new Vector3(0,.13f,-4.7f),new Vector3(2.8f,.06f,4.3f),roadMat);
            AddSlab(root,"central plaza",new Vector3(0,.14f,.65f),new Vector3(6.4f,.07f,5.0f),roadMat);
            AddSlab(root,"bastion approach",new Vector3(0,.15f,4.1f),new Vector3(3.4f,.07f,2.5f),roadMat);

            // Two restrained cross streets make the construction plots legible at strategic zoom.
            AddSlab(root,"west branch",new Vector3(-4.2f,.135f,-1.0f),new Vector3(5.3f,.055f,1.55f),roadMat);
            AddSlab(root,"east branch",new Vector3( 4.2f,.135f,-1.0f),new Vector3(5.3f,.055f,1.55f),roadMat);

            // Plaza monument establishes a visual center without filling the city.
            ValoriaKit.Cylinder("Valoria · Flat Citadel · plaza plinth",
                new Vector3(0,.26f,.65f),new Vector3(.70f,.18f,.70f),WarmStone,Quaternion.identity);
            ValoriaKit.Banner("Valoria · Flat Citadel · central standard",
                new Vector3(0,1.25f,.65f),new Vector3(.42f,1.35f,.06f),Blue);
        }

        static void BuildGroundProduction(Transform root)
        {
            // Replace proof-grade rectangular road/parcel slabs with authored cobble and irregular plot language.
            HideProofGroundSlabs(root);

            var art=ValoriaExternalAssetLibrary.Load();
            var cobble=art!=null?art.SlavicCobbleRoad:null;
            var mud=art!=null?art.SlavicMudFlat:null;
            var moss=art!=null?art.SlavicMoss:null;

            var roadTint=new Color(.58f,.55f,.49f,1f);
            if(cobble!=null)
            {
                // Gate -> plaza -> Bastion: one continuous readable route with controlled variation.
                for(int i=0;i<8;i++)
                {
                    float z=-5.15f+i*1.28f;
                    float x=(i%3-1)*.08f;
                    float yaw=(i%4==0?-3f:(i%4==1?2f:0f));
                    float span=(i<2?2.65f:(i>5?3.0f:2.85f));
                    AddGroundAsset(root,cobble,"main road "+i,new Vector3(x,.135f,z),span,.16f,yaw,roadTint*(.96f+(i%2)*.035f));
                }

                // Cross streets terminate in buildable parcels rather than reading as a giant paved board.
                for(int side=-1;side<=1;side+=2)
                {
                    for(int i=0;i<4;i++)
                    {
                        float x=side*(1.85f+i*1.25f);
                        AddGroundAsset(root,cobble,(side<0?"west":"east")+" branch "+i,
                            new Vector3(x,.132f,-.72f+(i%2)*.06f),2.05f,.14f,90f+(i%2==0?2f:-2f),
                            roadTint*(.95f+(i%2)*.04f));
                    }
                }

                // Plaza: authored stone patches with a larger centre and four edge pieces;
                // readable as civic space, but not a single rectangular tile.
                AddGroundAsset(root,cobble,"plaza centre",new Vector3(0,.142f,.72f),4.05f,.15f,0f,new Color(.62f,.59f,.53f,1f));
                foreach(var s in new[]{
                    new Vector4(-2.25f,.72f, 90f,2.45f), new Vector4(2.25f,.72f,90f,2.45f),
                    new Vector4(0f,-.95f,0f,2.55f), new Vector4(0f,2.35f,0f,2.55f)})
                    AddGroundAsset(root,cobble,"plaza edge",new Vector3(s.x,.137f,s.y),s.w,.13f,s.z,new Color(.57f,.55f,.50f,1f));
            }

            // Buildable plots: warmer compacted earth fields with irregular silhouettes.
            AddIrregularGroundPatch(root,"west buildable parcel",
                new[]{new Vector2(-8.55f,1.55f),new Vector2(-4.75f,1.20f),new Vector2(-4.45f,4.15f),
                      new Vector2(-5.55f,5.05f),new Vector2(-8.70f,4.45f),new Vector2(-9.0f,2.65f)},
                .105f,new Color(.46f,.39f,.28f,1f));
            AddIrregularGroundPatch(root,"east buildable parcel",
                new[]{new Vector2(4.55f,1.35f),new Vector2(8.45f,1.50f),new Vector2(8.95f,2.80f),
                      new Vector2(8.60f,4.50f),new Vector2(5.35f,4.95f),new Vector2(4.35f,4.0f)},
                .105f,new Color(.46f,.39f,.28f,1f));

            // Functional plots get distinct but related surface identities.
            AddIrregularGroundPatch(root,"sawmill work yard",
                new[]{new Vector2(-8.5f,-3.65f),new Vector2(-4.25f,-3.25f),new Vector2(-4.15f,-.65f),
                      new Vector2(-5.0f,.05f),new Vector2(-8.55f,-.20f),new Vector2(-9.0f,-2.0f)},
                .112f,new Color(.50f,.39f,.25f,1f));
            AddIrregularGroundPatch(root,"barracks training yard",
                new[]{new Vector2(4.1f,-3.55f),new Vector2(8.35f,-3.75f),new Vector2(8.85f,-2.10f),
                      new Vector2(8.45f,-.35f),new Vector2(4.85f,.05f),new Vector2(4.05f,-1.0f)},
                .112f,new Color(.43f,.40f,.33f,1f));

            // Small authored dirt/moss overlays soften the city/meadow and road/parcel joins.
            if(mud!=null)
            {
                foreach(var s in new[]{
                    new Vector4(-7.7f,4.35f,18f,1.65f),new Vector4(-5.0f,1.45f,72f,1.25f),
                    new Vector4(7.65f,4.20f,192f,1.65f),new Vector4(5.05f,1.35f,108f,1.25f),
                    new Vector4(-3.1f,-3.9f,35f,1.20f),new Vector4(3.35f,-3.95f,210f,1.20f)})
                    AddGroundAsset(root,mud,"earth transition",new Vector3(s.x,.118f,s.y),s.w,.10f,s.z,new Color(.56f,.49f,.37f,1f));
            }
            if(moss!=null)
            {
                foreach(var s in new[]{
                    new Vector4(-8.75f,5.15f,15f,1.1f),new Vector4(8.65f,5.0f,205f,1.1f),
                    new Vector4(-8.9f,-4.55f,30f,.95f),new Vector4(8.85f,-4.55f,220f,.95f)})
                    AddGroundAsset(root,moss,"grass transition",new Vector3(s.x,.12f,s.y),s.w,.08f,s.z,new Color(.44f,.55f,.36f,1f));
            }
        }

        static void HideProofGroundSlabs(Transform root)
        {
            foreach(var r in root.GetComponentsInChildren<Renderer>(true))
            {
                string n=r.gameObject.name.ToLowerInvariant();
                if(n.Contains("gate road")||n.Contains("central plaza")||n.Contains("bastion approach")||
                   n.Contains("west branch")||n.Contains("east branch")||n.Contains("growth parcel"))
                    r.enabled=false;
            }
        }

        static void AddGroundAsset(Transform root,GameObject source,string role,Vector3 ground,float footprint,float maxHeight,float yaw,Color tint)
        {
            if(source==null)return;
            var go=ValoriaKit.BenchmarkPieceModulated("Valoria · Flat Citadel Production · "+role,source,ground,footprint,maxHeight,
                Quaternion.Euler(0f,yaw,0f),tint);
            if(go==null)return;
            go.transform.SetParent(root,true);
            foreach(var col in go.GetComponentsInChildren<Collider>(true))Object.DestroyImmediate(col);
            foreach(var h in go.GetComponentsInChildren<WorldHotspot>(true))Object.DestroyImmediate(h);
        }

        static void AddIrregularGroundPatch(Transform root,string role,Vector2[] ring,float y,Color tint)
        {
            if(ring==null||ring.Length<3)return;
            int n=ring.Length;
            var verts=new Vector3[n+1];
            Vector2 center=Vector2.zero;
            foreach(var p in ring)center+=p;
            center/=n;
            verts[0]=new Vector3(center.x,y,center.y);
            for(int i=0;i<n;i++)verts[i+1]=new Vector3(ring[i].x,y,ring[i].y);
            var tris=new int[n*3];
            for(int i=0;i<n;i++){tris[i*3]=0;tris[i*3+1]=i+1;tris[i*3+2]=((i+1)%n)+1;}
            var mesh=new Mesh{name="Valoria Flat Citadel ground · "+role};
            mesh.vertices=verts;mesh.triangles=tris;mesh.RecalculateNormals();mesh.RecalculateBounds();
            var go=new GameObject("Valoria · Flat Citadel Production · "+role);
            go.transform.SetParent(root,true);
            go.AddComponent<MeshFilter>().sharedMesh=mesh;
            var r=go.AddComponent<MeshRenderer>();
            r.sharedMaterial=ValoriaKit.DetailedSurfaceMaterial(tint,"earth",new Vector2(2.3f,2.3f),.94f);
            r.shadowCastingMode=ShadowCastingMode.Off;r.receiveShadows=true;
        }


        static void BuildFlatCitySurfaceProduction(Transform root)
        {
            // Keep the accepted macro footprint but give the natural surround and buildable ground
            // distinct material/value families instead of one uniform plane.
            var meadow=ValoriaKit.DetailedSurfaceMaterial(new Color(.31f,.39f,.25f,1f),"earth",new Vector2(7.2f,7.2f),.90f);
            AddSlab(root,"production surrounding meadow",new Vector3(0,-.06f,1.0f),new Vector3(31f,.08f,26f),meadow);

            Vector2[] ring={
                new Vector2(-10.4f,-7.0f),new Vector2(-6.2f,-7.7f),new Vector2(-1.8f,-7.5f),
                new Vector2( 3.0f,-7.7f),new Vector2( 8.0f,-7.1f),new Vector2(10.4f,-5.0f),
                new Vector2(10.7f, 0.4f),new Vector2(10.2f, 6.2f),new Vector2( 7.7f, 9.7f),
                new Vector2( 2.9f,10.5f),new Vector2(-2.8f,10.4f),new Vector2(-7.9f, 9.5f),
                new Vector2(-10.3f,6.0f),new Vector2(-10.8f,.2f)
            };
            var cityMat=ValoriaKit.ExternalPbrSurfaceMaterial("dirt",new Color(.52f,.46f,.35f,1f),new Vector2(1.6f,1.6f),.015f,.86f)
                ?? ValoriaKit.DetailedSurfaceMaterial(new Color(.48f,.42f,.32f,1f),"earth",new Vector2(3.4f,3.4f),.92f);
            CreatePrism(root,"production buildable city plane",ring,.015f,.075f,cityMat,
                ValoriaKit.DetailedSurfaceMaterial(new Color(.34f,.34f,.31f,1f),"rock",new Vector2(3.0f,3.0f),1.0f));

            var parcelMat=ValoriaKit.ExternalPbrSurfaceMaterial("dirt",new Color(.42f,.35f,.24f,1f),new Vector2(1.25f,1.25f),.012f,.82f)
                ?? ValoriaKit.DetailedSurfaceMaterial(new Color(.39f,.33f,.24f,1f),"earth",new Vector2(2.6f,2.6f),.92f);
            AddSlab(root,"west growth parcel",new Vector3(-6.55f,.102f,3.25f),new Vector3(4.45f,.035f,3.35f),parcelMat);
            AddSlab(root,"east growth parcel",new Vector3( 6.45f,.102f,3.15f),new Vector3(4.45f,.035f,3.35f),parcelMat);
            AddSlab(root,"granary work parcel",new Vector3(-2.75f,.103f,-4.40f),new Vector3(3.55f,.035f,2.65f),parcelMat);

            // Broad, restrained worn-earth fields break the empty-board read without adding clutter.
            var worn=ValoriaKit.DetailedSurfaceMaterial(new Color(.44f,.39f,.30f,1f),"earth",new Vector2(2.4f,2.4f),.88f);
            AddSlab(root,"worn west shoulder",new Vector3(-7.1f,.104f,-3.0f),new Vector3(3.7f,.025f,1.35f),worn);
            AddSlab(root,"worn east shoulder",new Vector3( 6.9f,.104f,-3.15f),new Vector3(3.7f,.025f,1.35f),worn);

            BuildParcelEdges(root);
        }

        static void BuildParcelEdges(Transform root)
        {
            var art=ValoriaExternalAssetLibrary.Load();
            if(art==null||art.SlavicStoneFence==null)return;
            var specs=new[]{
                new Vector4(-6.55f,1.56f,0f,4.15f),new Vector4(-6.55f,4.90f,0f,4.15f),
                new Vector4(-8.70f,3.25f,90f,3.05f),new Vector4(-4.40f,3.25f,90f,3.05f),
                new Vector4( 6.45f,1.46f,0f,4.15f),new Vector4( 6.45f,4.80f,0f,4.15f),
                new Vector4( 4.30f,3.15f,90f,3.05f),new Vector4( 8.60f,3.15f,90f,3.05f)
            };
            for(int i=0;i<specs.Length;i++)
            {
                var s=specs[i];
                var go=ValoriaKit.BenchmarkPieceModulated("Valoria · Flat Citadel Production · parcel edge "+i,
                    art.SlavicStoneFence,new Vector3(s.x,.09f,s.y),s.w,.55f,Quaternion.Euler(0,s.z,0),
                    new Color(.78f,.76f,.69f,1f));
                if(go!=null)go.transform.SetParent(root,true);
            }
        }

        static void BuildPrimaryAxisProduction(Transform root)
        {
            var art=ValoriaExternalAssetLibrary.Load();
            var stone=ValoriaKit.ExternalPbrSurfaceMaterial("stone",new Color(.66f,.63f,.56f,1f),new Vector2(1.8f,1.8f),.020f,.95f)
                ?? ValoriaKit.DetailedSurfaceMaterial(new Color(.58f,.56f,.50f,1f),"stone",new Vector2(2.3f,2.3f),1.0f);

            // Plaza is one calm visual anchor. Authored cobble modules define circulation on top.
            AddSlab(root,"production central plaza",new Vector3(0,.118f,.70f),new Vector3(6.5f,.055f,5.2f),stone);

            if(art!=null&&art.SlavicCobbleRoad!=null)
            {
                for(int i=0;i<8;i++)
                {
                    float z=-5.55f+i*1.35f;
                    var road=ValoriaKit.BenchmarkPieceModulated("Valoria · Flat Citadel Production · main road "+i,
                        art.SlavicCobbleRoad,new Vector3(0,.135f,z),2.55f,.22f,
                        Quaternion.Euler(0,(i%3-1)*2.5f,0),new Color(.92f,.89f,.81f,1f));
                    if(road!=null)road.transform.SetParent(root,true);
                }
                for(int side=-1;side<=1;side+=2)
                for(int i=0;i<3;i++)
                {
                    float x=side*(2.15f+i*1.55f);
                    var branch=ValoriaKit.BenchmarkPieceModulated("Valoria · Flat Citadel Production · branch road "+side+" "+i,
                        art.SlavicCobbleRoad,new Vector3(x,.135f,-1.0f),2.45f,.22f,
                        Quaternion.Euler(0,90f+(i%2==0?2f:-2f),0),new Color(.90f,.87f,.79f,1f));
                    if(branch!=null)branch.transform.SetParent(root,true);
                }
            }
            else
            {
                AddSlab(root,"production gate road",new Vector3(0,.13f,-4.7f),new Vector3(2.8f,.06f,4.3f),stone);
                AddSlab(root,"production bastion approach",new Vector3(0,.13f,4.1f),new Vector3(3.4f,.06f,2.5f),stone);
                AddSlab(root,"production west branch",new Vector3(-4.2f,.13f,-1.0f),new Vector3(5.3f,.05f,1.55f),stone);
                AddSlab(root,"production east branch",new Vector3(4.2f,.13f,-1.0f),new Vector3(5.3f,.05f,1.55f),stone);
            }

            ValoriaKit.Cylinder("Valoria · Flat Citadel Production · plaza plinth",
                new Vector3(0,.24f,.70f),new Vector3(.64f,.16f,.64f),WarmStone,Quaternion.identity);
            ValoriaKit.Banner("Valoria · Flat Citadel Production · central standard",
                new Vector3(0,1.25f,.70f),new Vector3(.42f,1.35f,.06f),Blue);
        }

        static void BuildBastionRise(Transform root)
        {
            // One controlled elevation only: a civic/defensive plinth, not a mountain.
            Vector2[] ring={
                new Vector2(-4.25f,4.9f),new Vector2(-3.5f,8.8f),new Vector2(-1.6f,9.7f),
                new Vector2(1.8f,9.7f),new Vector2(3.6f,8.7f),new Vector2(4.25f,5.0f),
                new Vector2(2.4f,4.35f),new Vector2(-2.4f,4.35f)
            };
            CreatePrism(root,"bastion civic rise",ring,.08f,.72f,
                ValoriaKit.DetailedSurfaceMaterial(new Color(.46f,.44f,.39f,1f),"stone",new Vector2(2.6f,2.6f),1.0f),
                ValoriaKit.DetailedSurfaceMaterial(new Color(.37f,.36f,.33f,1f),"stone",new Vector2(2.2f,2.2f),1.0f));

            for(int i=0;i<7;i++)
            {
                float y=.16f+i*.085f;
                float z=4.25f+i*.25f;
                AddSlab(root,"bastion stair "+i,new Vector3(0,y,z),
                    new Vector3(3.25f,.08f,.42f),
                    ValoriaKit.DetailedSurfaceMaterial(WarmStone*.86f,"stone",new Vector2(2f,2f),1.0f));
            }
        }

        static void BuildFunctionalArchitecture(Transform root, PlayerState state)
        {
            FunctionalBuildings=0;
            int before=0;

            var heroSource=Resources.Load<GameObject>("Valoria/HeroBastionGenerated/Valoria_HeroBastion_v1");
            if(heroSource!=null)
            {
                var hero=Object.Instantiate(heroSource);
                hero.name="Valoria · Flat Citadel · Hero Bastion";
                FitPrefab(hero,new Vector3(0,.76f,7.25f),7.6f,7.9f);
                hero.transform.SetParent(root,true);
                DisableGameplay(hero);
                FunctionalBuildings++;
            }
            else
            {
                before=root.childCount;
                ValoriaKit.BastionCore("Bastion · Flat Citadel",new Vector3(0,.78f,7.25f),Glow);
                ReparentNew(root,before); FunctionalBuildings++;
            }

            before=root.childCount;
            ValoriaKit.SawmillArchitecture("Aserradero · Flat Citadel",new Vector3(-5.9f,.16f,-1.55f),state.SawmillLevel>0,Glow);
            ReparentNew(root,before); FunctionalBuildings++;

            before=root.childCount;
            ValoriaKit.BarracksArchitecture("Cuartel · Flat Citadel",new Vector3(5.9f,.16f,-1.75f),state.BarracksLevel>0,Glow);
            ReparentNew(root,before); FunctionalBuildings++;

            before=root.childCount;
            ValoriaKit.GranaryArchitecture("Granero · Flat Citadel",new Vector3(-2.7f,.15f,-4.4f),true,Glow);
            ReparentNew(root,before); FunctionalBuildings++;

            // Only two small rebuilt homes: this is a settlement with headroom, not a finished metropolis.
            before=root.childCount;
            ValoriaKit.House("Valoria · Flat Citadel · cottage west",new Vector3(-5.8f,.18f,3.55f),new Vector3(2.0f,1.15f,1.7f),true,Glow);
            ReparentNew(root,before);
            before=root.childCount;
            ValoriaKit.House("Valoria · Flat Citadel · cottage east",new Vector3(5.7f,.18f,3.45f),new Vector3(2.0f,1.15f,1.7f),true,Glow);
            ReparentNew(root,before);
        }

        static void BuildOuterWallBaseline(Transform root)
        {
            WallPieces=0;
            var art=ValoriaExternalAssetLibrary.Load();

            // Front wall deliberately opens at the main gate.
            for(int i=0;i<4;i++)
            {
                float x=-8.8f+i*2.2f;
                WallSegment(root,new Vector3(x,.12f,-6.35f),new Vector3(2.1f,1.18f,.42f),0f); WallPieces++;
            }
            for(int i=0;i<4;i++)
            {
                float x=2.2f+i*2.2f;
                WallSegment(root,new Vector3(x,.12f,-6.35f),new Vector3(2.1f,1.18f,.42f),0f); WallPieces++;
            }

            for(int i=0;i<8;i++)
            {
                float z=-4.9f+i*2.0f;
                WallSegment(root,new Vector3(-9.55f,.12f,z),new Vector3(.42f,1.18f,1.95f),0f); WallPieces++;
                WallSegment(root,new Vector3( 9.55f,.12f,z),new Vector3(.42f,1.18f,1.95f),0f); WallPieces++;
            }
            for(int i=0;i<8;i++)
            {
                float x=-7.7f+i*2.2f;
                WallSegment(root,new Vector3(x,.12f,9.25f),new Vector3(2.1f,1.18f,.42f),0f); WallPieces++;
            }

            // Authored gate and four compact corner towers improve silhouette while the wall remains continuous.
            if(art!=null && art.SlavicRockGate!=null)
            {
                var g=ValoriaKit.BenchmarkPieceTinted("Valoria · Flat Citadel · main gate",art.SlavicRockGate,
                    new Vector3(0,.12f,-6.45f),3.5f,3.1f,Quaternion.identity,WarmStone*.88f);
                if(g!=null) g.transform.SetParent(root,true);
            }
            foreach(var p in new[]{
                new Vector3(-9.5f,.22f,-6.2f),new Vector3(9.5f,.22f,-6.2f),
                new Vector3(-9.5f,.22f,9.1f),new Vector3(9.5f,.22f,9.1f)})
            {
                ValoriaKit.Wall("Valoria · Flat Citadel · corner tower",p+new Vector3(0,.85f,0),
                    new Vector3(1.15f,2.05f,1.15f),WallStone,false);
                ReparentNewest(root);
            }
        }


        static void BuildOuterWallProduction(Transform root)
        {
            WallPieces=0;
            AuthoredWallModules=0;
            AuthoredWallTowers=0;

            var wall=Resources.Load<GameObject>("Valoria/Stone_Wall");
            var tower=Resources.Load<GameObject>("Valoria/Stone_Tower");
            var gate=Resources.Load<GameObject>("Valoria/Stone_Gate");
            if(wall==null||tower==null||gate==null)
                throw new InvalidOperationException("Flat Citadel production wall requires Valoria/Stone_Wall, Stone_Tower and Stone_Gate.");

            // Main gatehouse: one strong authored gate + twin towers + restrained heraldry.
            AddWallModule(root,gate,"main gate",new Vector3(0f,.10f,-6.55f),4.55f,3.55f,0f,new Color(.78f,.76f,.70f,1f));
            AddWallModule(root,tower,"gate tower west",new Vector3(-3.15f,.08f,-6.25f),2.35f,3.70f,4f,new Color(.73f,.72f,.67f,1f),true);
            AddWallModule(root,tower,"gate tower east",new Vector3(3.15f,.08f,-6.25f),2.22f,3.45f,-5f,new Color(.72f,.71f,.66f,1f),true);
            AddWallBanner(root,new Vector3(-1.70f,2.45f,-6.62f));
            AddWallBanner(root,new Vector3(1.70f,2.35f,-6.62f));

            // Front curtains: fewer, longer authored bays with asymmetry and tower interruptions.
            float[] frontX={-8.05f,-5.55f,5.55f,8.05f};
            for(int i=0;i<frontX.Length;i++)
            {
                float yaw=(i%2==0?1.2f:-1.1f);
                float h=(i==0||i==3)?2.35f:2.15f;
                AddWallModule(root,wall,"front curtain "+i,new Vector3(frontX[i],.06f,-6.30f),3.25f,h,yaw,
                    new Color(.70f-(i%2)*.025f,.69f-(i%2)*.02f,.64f,1f));
            }

            // Side walls use staggered authored bays. Mid-wall towers break repetition and create defensive rhythm.
            float[] sideZ={-3.65f,-.70f,2.45f,5.55f,8.10f};
            for(int i=0;i<sideZ.Length;i++)
            {
                float span=(i==2?3.45f:3.20f);
                float h=2.10f+(i%2)*.12f;
                AddWallModule(root,wall,"west curtain "+i,new Vector3(-9.48f,.06f,sideZ[i]),span,h,90f+(i%2==0?1.5f:-1.5f),
                    new Color(.69f,.68f,.63f,1f));
                AddWallModule(root,wall,"east curtain "+i,new Vector3(9.48f,.06f,sideZ[i]),span,h,90f+(i%2==0?-1.5f:1.5f),
                    new Color(.69f,.68f,.63f,1f));
                if(i==2)
                {
                    AddWallModule(root,tower,"west mid tower",new Vector3(-9.55f,.08f,3.95f),2.05f,3.20f,8f,new Color(.71f,.70f,.65f,1f),true);
                    AddWallModule(root,tower,"east mid tower",new Vector3(9.55f,.08f,3.95f),1.98f,3.00f,-11f,new Color(.70f,.69f,.64f,1f),true);
                }
            }

            // Rear defensive line: lower curtains keep Hero Bastion dominant, with stronger corner towers.
            float[] rearX={-7.25f,-4.25f,-1.45f,1.45f,4.25f,7.25f};
            for(int i=0;i<rearX.Length;i++)
                AddWallModule(root,wall,"rear curtain "+i,new Vector3(rearX[i],.06f,9.28f),3.15f,1.92f,(i%2==0?1.0f:-1.0f),
                    new Color(.66f,.66f,.62f,1f));

            foreach(var spec in new[]{
                new Vector4(-9.35f,-6.10f, 10f,3.55f),
                new Vector4( 9.35f,-6.10f,-12f,3.45f),
                new Vector4(-9.30f, 9.05f, 22f,3.75f),
                new Vector4( 9.30f, 9.05f,-18f,3.60f)})
            {
                AddWallModule(root,tower,"corner tower",new Vector3(spec.x,.08f,spec.y),2.55f,spec.w,spec.z,
                    new Color(.72f,.71f,.66f,1f),true);
            }

            // Grounded transition at the gate mouth; avoids a clean prefab cut into the meadow.
            var art=ValoriaExternalAssetLibrary.Load();
            if(art!=null&&art.SlavicFlatRock!=null)
            {
                foreach(var p in new[]{new Vector3(-2.65f,.035f,-6.65f),new Vector3(2.70f,.035f,-6.62f)})
                {
                    var seam=ValoriaKit.BenchmarkPieceTinted("Valoria · Flat Citadel Production · gate foundation",
                        art.SlavicFlatRock,p,1.85f,.42f,Quaternion.Euler(0,p.x<0?18f:198f,0),new Color(.42f,.42f,.39f,1f));
                    if(seam!=null)seam.transform.SetParent(root,true);
                }
            }
        }

        static void AddWallModule(Transform root,GameObject source,string role,Vector3 ground,float footprint,float maxHeight,float yaw,Color tint,bool tower=false)
        {
            // Preserve the source texture channels explicitly. The first material correction proved that
            // replacing the shader without carrying every possible imported albedo property can bleach the wall.
            var go=ValoriaKit.BenchmarkPiece("Valoria · Flat Citadel Production · "+role,source,ground,footprint,maxHeight,
                Quaternion.Euler(0f,yaw,0f));
            if(go==null)throw new InvalidOperationException("Failed to build authored wall module: "+role);
            NormalizeWallMaterials(go,Color.Lerp(new Color(.62f,.60f,.55f,1f),tint,.20f));
            go.transform.SetParent(root,true);
            NormalizeWallMaterials(go,tint,tower);
            foreach(var col in go.GetComponentsInChildren<Collider>(true))Object.DestroyImmediate(col);
            foreach(var h in go.GetComponentsInChildren<WorldHotspot>(true))Object.DestroyImmediate(h);
            AuthoredWallModules++;
            WallPieces++;
            if(tower)AuthoredWallTowers++;
        }

        static void NormalizeWallMaterials(GameObject go,Color tint,bool tower)
        {
            var lit=Shader.Find("Universal Render Pipeline/Lit")??Shader.Find("Standard");
            if(lit==null)return;

            foreach(var r in go.GetComponentsInChildren<Renderer>(true))
            {
                var src=r.sharedMaterials;
                var dst=new Material[src.Length];
                for(int i=0;i<src.Length;i++)
                {
                    var old=src[i];
                    if(old==null){dst[i]=null;continue;}

                    Texture baseMap=null,normal=null;
                    Vector2 scale=Vector2.one,offset=Vector2.zero;
                    foreach(string prop in new[]{"_BaseMap","_MainTex","_BaseColorTexture","baseColorTexture"})
                    {
                        if(old.HasProperty(prop)&&old.GetTexture(prop)!=null)
                        {
                            baseMap=old.GetTexture(prop);
                            try{scale=old.GetTextureScale(prop);offset=old.GetTextureOffset(prop);}catch{}
                            break;
                        }
                    }
                    foreach(string prop in new[]{"_BumpMap","_NormalMap","normalTexture"})
                        if(old.HasProperty(prop)&&old.GetTexture(prop)!=null){normal=old.GetTexture(prop);break;}

                    var m=new Material(lit){name="Valoria Flat Citadel wall · "+old.name};
                    if(baseMap!=null)
                    {
                        if(m.HasProperty("_BaseMap")){m.SetTexture("_BaseMap",baseMap);m.SetTextureScale("_BaseMap",scale);m.SetTextureOffset("_BaseMap",offset);}
                        if(m.HasProperty("_MainTex")){m.SetTexture("_MainTex",baseMap);m.SetTextureScale("_MainTex",scale);m.SetTextureOffset("_MainTex",offset);}
                    }
                    if(normal!=null&&m.HasProperty("_BumpMap"))
                    {
                        m.SetTexture("_BumpMap",normal);m.EnableKeyword("_NORMALMAP");
                    }

                    // Same warm, readable stone family as Hero Bastion. Towers are only slightly darker
                    // so silhouette variation comes from geometry, not black-value contrast.
                    var baseColor=Color.Lerp(new Color(.72f,.69f,.62f,1f),tint,.26f);
                    if(tower)baseColor*=.96f;
                    baseColor.a=1f;
                    if(m.HasProperty("_BaseColor"))m.SetColor("_BaseColor",baseColor);
                    if(m.HasProperty("_Color"))m.SetColor("_Color",baseColor);
                    if(m.HasProperty("_Metallic"))m.SetFloat("_Metallic",0f);
                    if(m.HasProperty("_Smoothness"))m.SetFloat("_Smoothness",.025f);
                    dst[i]=m;
                }
                r.sharedMaterials=dst;
            }
        }

        static void NormalizeWallMaterials(GameObject go,Color tint)
        {
            var lit=Shader.Find("Universal Render Pipeline/Lit")??Shader.Find("Standard");
            if(lit==null)return;
            foreach(var renderer in go.GetComponentsInChildren<Renderer>(true))
            {
                var src=renderer.sharedMaterials;
                var dst=new Material[src.Length];
                for(int i=0;i<src.Length;i++)
                {
                    var source=src[i];
                    if(source==null){dst[i]=null;continue;}
                    Texture baseMap=null,normal=null,mask=null;
                    foreach(string property in new[]{"_Texture","_BaseMap","_MainTex","_BaseColorTexture","baseColorTexture","_Albedo"})
                        if(source.HasProperty(property)&&source.GetTexture(property)!=null){baseMap=source.GetTexture(property);break;}
                    foreach(string property in new[]{"_BumpMap","_NormalMap","normalTexture"})
                        if(source.HasProperty(property)&&source.GetTexture(property)!=null){normal=source.GetTexture(property);break;}
                    foreach(string property in new[]{"_MetallicGlossMap","_MaskMap","metallicRoughnessTexture"})
                        if(source.HasProperty(property)&&source.GetTexture(property)!=null){mask=source.GetTexture(property);break;}

                    var m=new Material(lit){name="Valoria Flat Citadel wall · "+source.name};
                    if(baseMap!=null)
                    {
                        if(m.HasProperty("_BaseMap"))m.SetTexture("_BaseMap",baseMap);
                        if(m.HasProperty("_MainTex"))m.SetTexture("_MainTex",baseMap);
                    }
                    if(normal!=null&&m.HasProperty("_BumpMap"))
                    {
                        m.SetTexture("_BumpMap",normal);
                        m.EnableKeyword("_NORMALMAP");
                    }
                    if(mask!=null&&m.HasProperty("_MetallicGlossMap"))
                        m.SetTexture("_MetallicGlossMap",mask);
                    if(m.HasProperty("_BaseColor"))m.SetColor("_BaseColor",tint);
                    if(m.HasProperty("_Color"))m.SetColor("_Color",tint);
                    if(m.HasProperty("_Metallic"))m.SetFloat("_Metallic",0f);
                    if(m.HasProperty("_Smoothness"))m.SetFloat("_Smoothness",.035f);
                    if(m.HasProperty("_SpecularHighlights"))m.SetFloat("_SpecularHighlights",1f);
                    if(m.HasProperty("_EnvironmentReflections"))m.SetFloat("_EnvironmentReflections",1f);
                    dst[i]=m;
                }
                renderer.sharedMaterials=dst;
            }
        }

        static void AddWallBanner(Transform root,Vector3 p)
        {
            int before=root.childCount;
            ValoriaKit.Banner("Valoria · Flat Citadel Production · wall banner",p,new Vector3(.48f,1.45f,.065f),Blue);
            ReparentNew(root,before);
        }

        static void BuildSparseNature(Transform root)
        {
            NaturePieces=0;
            foreach(var p in new[]{
                new Vector3(-11.2f,.02f,-4.8f),new Vector3(-11.4f,.02f,1.0f),new Vector3(-11.0f,.02f,7.4f),
                new Vector3(11.2f,.02f,-4.5f),new Vector3(11.3f,.02f,1.4f),new Vector3(11.0f,.02f,7.3f),
                new Vector3(-7.9f,.02f,10.8f),new Vector3(7.8f,.02f,10.9f)})
            {
                int before=root.childCount;
                ValoriaKit.PineTree("Valoria · Flat Citadel · perimeter pine",p,.58f);
                ReparentNew(root,before); NaturePieces++;
            }

            var art=ValoriaExternalAssetLibrary.Load();
            if(art!=null && art.SlavicBoulder!=null)
            {
                foreach(var p in new[]{new Vector3(-10.7f,.05f,4.5f),new Vector3(10.8f,.05f,4.2f)})
                {
                    var go=ValoriaKit.BenchmarkPieceTinted("Valoria · Flat Citadel · perimeter boulder",art.SlavicBoulder,
                        p,1.55f,1.0f,Quaternion.Euler(0,p.x<0?28f:205f,0),new Color(.38f,.39f,.36f,1f));
                    if(go!=null){go.transform.SetParent(root,true);NaturePieces++;}
                }
            }
        }

        static void BuildLifeCues(Transform root)
        {
            var art=ValoriaExternalAssetLibrary.Load();
            if(art!=null && art.Firewood!=null)
            {
                var wood=ValoriaKit.BenchmarkPiece("Valoria · Flat Citadel · sawmill stock",art.Firewood,
                    new Vector3(-7.2f,.16f,-2.9f),1.15f,.8f,Quaternion.Euler(0,18f,0));
                if(wood!=null) wood.transform.SetParent(root,true);
            }
            Glow("Valoria · Flat Citadel · bastion warmth",new Vector3(0,3.7f,7.0f),Amber,1.25f,5.6f);
            Glow("Valoria · Flat Citadel · sawmill warmth",new Vector3(-5.3f,1.4f,-1.8f),Amber,1.0f,3.0f);
            Glow("Valoria · Flat Citadel · barracks warmth",new Vector3(5.3f,1.4f,-1.9f),Amber,.95f,2.8f);
        }

        static void WallSegment(Transform root,Vector3 p,Vector3 size,float yaw)
        {
            ValoriaKit.Wall("Valoria · Flat Citadel · outer wall",p+new Vector3(0,size.y*.50f,0),size,WallStone,false);
            ReparentNewest(root);
        }

        static void AddParcel(Transform root,string name,Vector3 p,Vector3 size,Color tint)
        {
            AddSlab(root,name,p,size,ValoriaKit.DetailedSurfaceMaterial(tint,"earth",new Vector2(2.8f,2.8f),.95f));
        }

        static void AddSlab(Transform root,string name,Vector3 p,Vector3 size,Material mat)
        {
            var go=GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name="Valoria · Flat Citadel · "+name;
            go.transform.SetParent(root,true);
            go.transform.position=p;
            go.transform.localScale=size;
            go.GetComponent<Renderer>().sharedMaterial=mat;
            var c=go.GetComponent<Collider>(); if(c!=null)c.enabled=false;
        }

        static void CreatePrism(Transform root,string role,Vector2[] ring,float bottom,float top,Material topMat,Material sideMat)
        {
            int n=ring.Length;
            var topVerts=new Vector3[n+1];
            topVerts[0]=new Vector3(0,top,1.2f);
            for(int i=0;i<n;i++)topVerts[i+1]=new Vector3(ring[i].x,top,ring[i].y);
            var topTris=new int[n*3];
            for(int i=0;i<n;i++)
            {
                topTris[i*3]=0;
                topTris[i*3+1]=i+1;
                topTris[i*3+2]=((i+1)%n)+1;
            }
            var topMesh=new Mesh{name="Flat Citadel "+role+" top"};
            topMesh.vertices=topVerts; topMesh.triangles=topTris; topMesh.RecalculateNormals(); topMesh.RecalculateBounds();
            var topGo=new GameObject("Valoria · Flat Citadel · "+role+" top");
            topGo.transform.SetParent(root,true);
            topGo.AddComponent<MeshFilter>().sharedMesh=topMesh;
            var tr=topGo.AddComponent<MeshRenderer>();tr.sharedMaterial=topMat;tr.shadowCastingMode=ShadowCastingMode.Off;tr.receiveShadows=true;

            var sideVerts=new Vector3[n*4];
            var sideTris=new int[n*6];
            for(int i=0;i<n;i++)
            {
                int j=(i+1)%n;
                int v=i*4;
                sideVerts[v]=new Vector3(ring[i].x,top,ring[i].y);
                sideVerts[v+1]=new Vector3(ring[j].x,top,ring[j].y);
                sideVerts[v+2]=new Vector3(ring[i].x,bottom,ring[i].y);
                sideVerts[v+3]=new Vector3(ring[j].x,bottom,ring[j].y);
                int t=i*6;
                sideTris[t]=v;sideTris[t+1]=v+2;sideTris[t+2]=v+1;
                sideTris[t+3]=v+1;sideTris[t+4]=v+2;sideTris[t+5]=v+3;
            }
            var sideMesh=new Mesh{name="Flat Citadel "+role+" sides"};
            sideMesh.vertices=sideVerts;sideMesh.triangles=sideTris;sideMesh.RecalculateNormals();sideMesh.RecalculateBounds();
            var sideGo=new GameObject("Valoria · Flat Citadel · "+role+" sides");
            sideGo.transform.SetParent(root,true);
            sideGo.AddComponent<MeshFilter>().sharedMesh=sideMesh;
            var sr=sideGo.AddComponent<MeshRenderer>();sr.sharedMaterial=sideMat;sr.shadowCastingMode=ShadowCastingMode.On;sr.receiveShadows=true;
        }

        static void FitPrefab(GameObject go,Vector3 ground,float span,float maxHeight)
        {
            var rs=go.GetComponentsInChildren<Renderer>(true);
            if(rs.Length==0)return;
            var b=rs[0].bounds;
            for(int i=1;i<rs.Length;i++)b.Encapsulate(rs[i].bounds);
            float horizontal=Mathf.Max(b.size.x,b.size.z);
            float scale=Mathf.Min(span/Mathf.Max(.001f,horizontal),maxHeight/Mathf.Max(.001f,b.size.y));
            go.transform.localScale*=scale;
            rs=go.GetComponentsInChildren<Renderer>(true);
            b=rs[0].bounds;
            for(int i=1;i<rs.Length;i++)b.Encapsulate(rs[i].bounds);
            go.transform.position+=ground-new Vector3(b.center.x,b.min.y,b.center.z);
        }

        static void ReparentNew(Transform root,int previousChildCount)
        {
            // ValoriaKit helpers create under the canonical integration root.
            // Pull only newly-created top-level visuals into this proof root.
            var canonical=root.parent;
            if(canonical==null)return;
            for(int i=canonical.childCount-1;i>=0;i--)
            {
                var c=canonical.GetChild(i);
                if(c==root)continue;
                if(c.GetSiblingIndex()<previousChildCount)continue;
                if(c.name.StartsWith("Valoria · Flat Citadel") || c.name.Contains("Flat Citadel"))
                    c.SetParent(root,true);
            }
        }

        static void ReparentNewest(Transform root)
        {
            var canonical=root.parent;
            if(canonical==null)return;
            for(int i=canonical.childCount-1;i>=0;i--)
            {
                var c=canonical.GetChild(i);
                if(c==root)continue;
                if(c.name.Contains("Flat Citadel")){c.SetParent(root,true);return;}
            }
        }

        static void Glow(string name,Vector3 p,Color color,float intensity,float range)
        {
            var go=new GameObject(name);
            go.transform.position=p;
            var l=go.AddComponent<Light>();
            l.type=LightType.Point;l.color=color;l.intensity=intensity;l.range=range;l.shadows=LightShadows.None;
            var root=GameObject.Find(RootName);
            if(root!=null)go.transform.SetParent(root.transform,true);
        }

        static void DisableGameplay(GameObject go)
        {
            foreach(var c in go.GetComponentsInChildren<Collider>(true))Object.DestroyImmediate(c);
            foreach(var h in go.GetComponentsInChildren<WorldHotspot>(true))Object.DestroyImmediate(h);
        }

        static string Chain(Transform t)
        {
            string s="";
            for(var p=t;p!=null;p=p.parent)s+="|"+p.name.ToLowerInvariant();
            return s;
        }
    }
}
