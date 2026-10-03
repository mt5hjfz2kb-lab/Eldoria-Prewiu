using System;
using System.IO;
using Eldoria.Domain;
using Eldoria.Presentation;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Object=UnityEngine.Object;

namespace Eldoria.EditorTools
{
    // Integrated whole-frame comparison over the current strongest stack.
    public static class ValoriaStrongestCompositeGateV2
    {
        const string Folder="ValoriaStrongestCompositeV2Captures";

        static readonly (string id,string resource,Vector3 p,float yaw,float span,float height)[] PremiumSpecs={
            // v19: use the already-certified Mid-Tier GLBs instead of the much lower-detail
            // Slavic prefab family. Positions stay compact around the fortress nucleus.
            ("lower_west","Piece02",new Vector3(-2.55f,.98f,1.72f),18f,2.52f,2.92f),
            ("lower_east","Piece03",new Vector3( 2.55f,.98f,1.70f),186f,2.50f,2.90f),
            ("mid_west","Piece03",new Vector3(-3.15f,1.42f,3.35f),12f,2.32f,2.72f),
            ("mid_east","Piece02",new Vector3( 3.18f,1.42f,3.38f),190f,2.32f,2.72f)
        };

        public static void Capture()
        {
            ShaderUtil.allowAsyncCompilation=false;
            Directory.CreateDirectory(Folder);
            SceneSetup.SetupRenderPipeline();
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);

            ProductionVisualIntegration.ResetVisualCachesForGate();
            ProductionVisualIntegration.StoneArchitectureEnabled=true;
            ProductionVisualIntegration.TerrainTerraceEnabled=true;
            ProductionVisualIntegration.SurfaceCellEnabled=false;
            ProductionVisualIntegration.ProductionCellEnabled=false;
            ProductionVisualIntegration.CoherentCastleProofEnabled=false;
            ProductionVisualIntegration.SlavicDistrictProofEnabled=false;
            ProductionVisualIntegration.CompactFootprintReframeEnabled=true;

            AssetVisualUpliftPassV1.Enabled=false;
            AssetLibraryReprocessingPassV1.Enabled=true;
            MidTierDistrictProduction.Enabled=true;
            ValoriaFullFrameArchitectureBatchV1.Enabled=true;
            ValoriaFullFrameForegroundEdgePassV1.Enabled=true;
            ValoriaOpenValleyCompositionV1.Enabled=true;
            ValoriaReferenceConvergencePassV2.Enabled=false;
            ValoriaInCitySurfacePassV1.Enabled=false;
            ValoriaStairLandingIntegrationV1.Enabled=false;
            ValoriaFullFrameConvergenceIteration1.Enabled=false;
            ValoriaFullFrameConvergenceIteration2.Enabled=false;
            ValoriaBenchmarkCompositeV2.Enabled=false;
            ValoriaEnvironmentUpliftV1.Enabled=false;
            ValoriaArchitectureCoherenceV1.Enabled=false;
            ValoriaBackplateCandidateV1.Enabled=false;
            ValoriaCliffIslandReframeV1.Enabled=false;
            ValoriaCliffIslandCleanupV2.Enabled=false;
            ValoriaResidualCleanupV1.Enabled=false;
            ValoriaMaterialResidueCleanupV2.Enabled=false;
            ValoriaFullFrameArtifactCleanupV1.Enabled=false;
            ValoriaWorldFrameMountainTerrainV1.Enabled=false;
            VisualWorld.VisualIntegrationEnabled=true;

            var state=new PlayerState{BastionLevel=3,SawmillLevel=1,BarracksLevel=1,CorruptionDiscovered=true};
            VisualWorld.Create(true,state);
            var c=Camera.main;
            var root=GameObject.Find("Valoria · integrated construction visual layer");
            if(c==null||root==null)throw new Exception("Valoria capture prerequisites missing.");

            var p=new Vector3(18.2f,14.6f,-25.8f);
            var t=new Vector3(0f,3.35f,5.6f);
            var baseline=ValoriaVisualFormulaGate.CollisionSignature();

            BuildStrongestBase(root.transform,state,c);
            SaveSet(c,"before",p,t);

            int suppressed=SuppressDisconnectedResidue();
            int lowerBoardSuppressed=SuppressLowerPeripheralBoard();
            int auditPruned=PruneAuditConfirmedLowerResidue();
            int premium=ReplaceSecondaryArchitecture(root.transform);
            int localSupports=0;
            int ruins=0;
            SuppressLegacySecondaryPresentation();
            int lowerSurfaceNormalized=NormalizeLowerCitySurfaces();
            int routeStyled=StyleCoreRoute();
            int sideToneAdjusted=ToneCoreSideBuildings();
            int coreVisualsReframed=ReframeCoreSideVisuals(root.transform);
            ValoriaLowerCityPlateauV1.Enabled=false;
            ValoriaLowerCityTerrainV1.Enabled=true;
            ValoriaLowerCityTerrainV1.Build(root.transform,state);
            int terrainVisualsHidden=HideLowerTerrainVisual();
            ValoriaLowerCityTerracedMeshV1.Enabled=true;
            ValoriaLowerCityTerracedMeshV1.Build(root.transform,state);
            int terracePieces=ValoriaLowerCityTerracedMeshV1.PiecesBuilt;
            ValoriaCompactStairMeshV1.Enabled=true;
            ValoriaCompactStairMeshV1.Build(root.transform,state);
            int stairPieces=1;
            int terrainEdgeRocks=0;

            Physics.SyncTransforms();
            if(ValoriaVisualFormulaGate.CollisionSignature()!=baseline)
                throw new Exception("Strongest Composite v2 altered gameplay signature.");

            SaveSet(c,"after",p,t);
            ValoriaBackplateCandidateV1.FitAspect(1280f/720f);
            Save(c,Folder+"/after-reference-8.1.png",p,new Vector3(0f,3.95f,5.90f),8.1f,1280,720);
            ValoriaBackplateCandidateV1.FitAspect(390f/844f);
            Save(c,Folder+"/after-reference-mobile.png",p,new Vector3(0f,3.95f,5.90f),8.1f,390,844);
            ValoriaBackplateCandidateV1.FitAspect(1280f/720f);
            WriteVisibleRendererAudit(c,Folder+"/visible-renderers.tsv");
            File.WriteAllText(Folder+"/evidence.json",$"{{\n"+
                $"  \"collider_hotspot_signature_equal\": true,\n"+
                $"  \"disconnected_renderers_suppressed\": {suppressed},\n"+
                $"  \"lower_board_renderers_suppressed\": {lowerBoardSuppressed},\n"+
                $"  \"audit_pruned_renderers\": {auditPruned},\n"+
                $"  \"core_route_renderers_styled\": {routeStyled},\n"+
                $"  \"core_side_renderers_toned\": {sideToneAdjusted},\n"+
                $"  \"core_side_visuals_reframed\": {coreVisualsReframed},\n"+
                $"  \"lower_surface_renderers_normalized\": {lowerSurfaceNormalized},\n"+
                $"  \"lower_plateau_fragment_renderers_suppressed\": {ValoriaLowerCityPlateauV1.SuppressedFragmentRenderers},\n"+
                $"  \"terrain_hole_samples\": {ValoriaLowerCityTerrainV1.HoleSamples},\n"+
                $"  \"terrain_surface_samples\": {ValoriaLowerCityTerrainV1.SurfaceSamples},\n"+
                $"  \"terrain_legacy_renderers_suppressed\": {ValoriaLowerCityTerrainV1.SuppressedLegacyRenderers},\n"+
                $"  \"terrain_visuals_hidden\": {terrainVisualsHidden},\n"+
                $"  \"authored_lower_terrace_pieces\": {terracePieces},\n"+
                $"  \"authored_stair_pieces\": {stairPieces},\n"+
                $"  \"terrain_edge_rock_meshes\": {terrainEdgeRocks},\n"+
                $"  \"premium_secondary_loaded\": {premium},\n"+
                $"  \"local_lower_supports_loaded\": {localSupports},\n"+
                $"  \"buried_side_ruin_pieces\": {ruins},\n"+
                $"  \"background\": \"Kiara 3 Morning CC0\",\n"+
                $"  \"existing_assets_only\": true,\n"+
                $"  \"tripo_credits\": 0\n"+
                $"}}\n");

            Debug.Log("VALORIA_STRONGEST_COMPOSITE_V2_GATE=PASS");
            EditorApplication.Exit(0);
        }

        static void BuildStrongestBase(Transform root,PlayerState state,Camera c)
        {
            ValoriaBenchmarkCompositeV2.Enabled=true;ValoriaBenchmarkCompositeV2.Build(root,state);
            ValoriaEnvironmentUpliftV1.Enabled=true;ValoriaEnvironmentUpliftV1.Build(root,state);
            ValoriaArchitectureCoherenceV1.Enabled=true;ValoriaArchitectureCoherenceV1.Build(root,state);
            ValoriaCliffIslandReframeV1.Enabled=true;ValoriaCliffIslandReframeV1.Build(root,state);
            ValoriaBackplateCandidateV1.Enabled=true;
            if(!ValoriaBackplateCandidateV1.Build(root,c,"kiara3_1"))throw new Exception("Kiara 3 backplate unavailable.");
            ValoriaBackplateCandidateV1.FitAspect(1280f/720f);
            ValoriaCliffIslandCleanupV2.Enabled=true;ValoriaCliffIslandCleanupV2.Build(root,state);
            ValoriaResidualCleanupV1.Enabled=true;ValoriaResidualCleanupV1.Build(root,state);
            ValoriaMaterialResidueCleanupV2.Enabled=true;ValoriaMaterialResidueCleanupV2.Build(root,state);
            ValoriaFullFrameArtifactCleanupV1.Enabled=true;ValoriaFullFrameArtifactCleanupV1.Build(root,state);
        }

        static int SuppressDisconnectedResidue()
        {
            int count=0;
            foreach(var r in Object.FindObjectsByType<Renderer>(FindObjectsSortMode.None))
            {
                if(r==null||!r.enabled||!r.gameObject.activeInHierarchy)continue;
                var b=r.bounds;
                string chain=Chain(r.transform);

                if(chain.Contains("backplate")||chain.Contains("bastion")||
                   chain.Contains("aserradero")||chain.Contains("cuartel")||
                   chain.Contains("granary")||chain.Contains("granero"))continue;

                bool presentation=chain.Contains("valoria ·")||chain.Contains("vpd ·")||
                    chain.Contains("mid-tier")||chain.Contains("assetlibrary");
                if(!presentation)continue;

                bool farOuter=Mathf.Abs(b.center.x)>9.6f||b.center.z>10.9f||b.center.z<-8.2f;
                bool detachedSmall=Mathf.Abs(b.center.x)>8.3f &&
                    b.size.y<4.5f && Mathf.Max(b.size.x,b.size.z)<6.5f;

                if(farOuter||detachedSmall)
                {
                    r.enabled=false;
                    count++;
                }
            }
            return count;
        }

        static int SuppressLowerPeripheralBoard()
        {
            int count=0;
            foreach(var r in Object.FindObjectsByType<Renderer>(FindObjectsSortMode.None))
            {
                if(r==null||!r.enabled||!r.gameObject.activeInHierarchy)continue;
                var b=r.bounds;
                string chain=Chain(r.transform);

                if(chain.Contains("backplate")||chain.Contains("bastion")||
                   chain.Contains("aserradero")||chain.Contains("cuartel")||
                   chain.Contains("granary")||chain.Contains("granero"))continue;

                bool presentation=chain.Contains("valoria ·")||chain.Contains("vpd ·")||
                    chain.Contains("assetlibrary")||chain.Contains("mid-tier");
                if(!presentation)continue;

                bool lowerPeripheral=b.center.z<-4.35f && Mathf.Abs(b.center.x)>4.65f && b.center.y<1.35f;
                bool legacyBoard=chain.Contains("lower cliff authored rock")||
                    chain.Contains("foreground edge")||
                    chain.Contains("terrainterrace")||
                    chain.Contains("expansion edge geology")||
                    chain.Contains("environment uplift · rock");

                bool nakedEdge=(chain.Contains("street edge")||chain.Contains("street transition")) &&
                    Mathf.Abs(b.center.x)>6.5f;

                if((lowerPeripheral&&legacyBoard)||nakedEdge)
                {
                    r.enabled=false;
                    count++;
                }
            }
            return count;
        }

        static int PruneAuditConfirmedLowerResidue()
        {
            int count=0;
            foreach(var r in Object.FindObjectsByType<Renderer>(FindObjectsSortMode.None))
            {
                if(r==null||!r.enabled||!r.gameObject.activeInHierarchy)continue;
                string chain=Chain(r.transform);
                var b=r.bounds;

                // New continuous substrate now owns these old visual pads/seams.
                bool oldPads=
                    chain.Contains("vpd · groundkit west workshop terrace")||
                    chain.Contains("vpd · groundkit east military terrace")||
                    chain.Contains("vpd · rescued seam west")||
                    chain.Contains("vpd · rescued seam east")||
                    chain.Contains("valoria · residual cleanup · rock lower");

                // The old military presentation is visibly duplicated by the new coherent barracks family.
                bool oldMilitary=
                    chain.Contains("valoria · cliff cleanup · military hall")||
                    chain.Contains("valoria · cliff cleanup · military store")||
                    chain.Contains("valoria · cliff cleanup · military roof crown");

                // Asset-library D1 pieces belong to the superseded lower-district generation.
                bool oldMidTier=
                    chain.Contains("assetlibrary reprocessing · midtier d1");

                // Only remove the low/front Cliff Island rocks now replaced by the organic substrate.
                bool lowCliffEdge=
                    chain.Contains("valoria · cliff island · edge") &&
                    b.center.y<1.15f && b.center.z<0.25f;

                // GroundKit carries both broad blockout slabs and smaller authored cobble overlays.
                // Keep the authored cobble pieces, but remove the broad visual bases that read as a board.
                bool duplicateStreet=
                    chain.Contains("valoria · stone street slab")||
                    chain.Contains("vpd · groundkit main street · street ")||
                    chain.Contains("vpd · groundkit l1 landing · widening base")||
                    chain.Contains("vpd · groundkit l1 landing · worn centre")||
                    chain.Contains("valoria · low street edge")||
                    chain.Contains("valoria · east street edge");

                bool redundantTerraces=
                    chain.Contains("vpd · groundkit l1 west terrace")||
                    chain.Contains("vpd · groundkit l1 east terrace")||
                    chain.Contains("vpd · rescued seam residential")||
                    chain.Contains("valoria · compactfootprint · west future terrace")||
                    chain.Contains("valoria · compactfootprint · east future terrace")||
                    chain.Contains("valoria · compactfootprint · west upper growth shelf")||
                    chain.Contains("valoria · compactfootprint · east upper growth shelf")||
                    chain.Contains("valoria · compactfootprint · central middle shelf")||
                    chain.Contains("valoria · compactfootprint · bastion lower shelf")||
                    chain.Contains("assetlibrary reprocessing · central west buried terrace")||
                    chain.Contains("assetlibrary reprocessing · central east buried terrace");

                bool redundantRetaining=
                    (chain.Contains("vpd · authored retaining rock") && b.center.y<1.55f)||
                    chain.Contains("valoria · barracks apron");

                bool badForegroundTree=
                    chain.Contains("valoria · environment uplift · tree") &&
                    b.center.z<0.2f && b.center.y<0.55f;

                bool redundantCliffEdges=
                    chain.Contains("valoria · cliff island · edge") &&
                    Mathf.Abs(b.center.x)>5.7f && b.center.y<2.55f;

                bool redundantMidRocks=
                    chain.Contains("valoria · residual cleanup · rock mid west")||
                    chain.Contains("valoria · residual cleanup · rock mid east");

                bool supersededHeroSupports=
                    chain.Contains("assetlibrary reprocessing · hero lower terrace")||
                    chain.Contains("assetlibrary reprocessing · hero upper terrace")||
                    chain.Contains("assetlibrary reprocessing · hero approach seam")||
                    chain.Contains("assetlibrary reprocessing · hero defensive flank")||
                    chain.Contains("valoria · rescued seam · bastion west shelf")||
                    chain.Contains("valoria · rescued seam · bastion east shelf");

                bool broadPlotSeam=
                    chain.Contains("vpd · groundkit west plot seam · seam earth")||
                    chain.Contains("vpd · groundkit east plot seam · seam earth");

                bool floatingCleanupProps=
                    chain.Contains("valoria · cliff cleanup · barrel")||
                    chain.Contains("valoria · cliff cleanup · crate");

                bool obsoleteSideSeams=
                    chain.Contains("valoria · stonearch · rock wall seam · sawmill")||
                    chain.Contains("valoria · stonearch · rock wall seam · barracks")||
                    chain.Contains("vpd · groundkit west plot seam · buried rock")||
                    chain.Contains("vpd · groundkit east plot seam · buried rock");

                bool broadDecorativeCourts=
                    chain.Contains("valoria · workshop court")||
                    chain.Contains("valoria · barracks apron");

                bool legacyStairDressing=
                    chain.Contains("valoria · worn tread skin")||
                    chain.Contains("vpd · retaining stone face")||
                    chain.Contains("valoria · stair cheek stone")||
                    chain.Contains("valoria · upper landing cheek")||
                    chain.Contains("valoria · retaining foundation stone")||
                    chain.Contains("valoria · compactfootprint · processional edge")||
                    chain.Contains("vpd · groundkit main street");

                // v18: these certified-era hero retaining slabs were useful while the
                // fortress was isolated, but in the compact full frame they read as
                // four giant rectangular pillars and overpower the inhabited city.
                bool oversizedHeroRetaining=
                    chain.Contains("assetlibrary reprocessing · hero retaining wall")||
                    chain.Contains("assetlibrary reprocessing · hero retaining corner");

                // Leftover pre-reframe sawmill frontage sits outside the compact shelf
                // and is now duplicate visual noise; gameplay authority is elsewhere.
                bool detachedLegacySawmill=
                    chain.Contains("valoria · stocked work frontage")||
                    chain.Contains("valoria · hero frame sawmill timber");

                if(oldPads||oldMilitary||oldMidTier||lowCliffEdge||duplicateStreet||redundantTerraces||redundantRetaining||badForegroundTree||redundantCliffEdges||redundantMidRocks||supersededHeroSupports||broadPlotSeam||floatingCleanupProps||obsoleteSideSeams||broadDecorativeCourts||legacyStairDressing||oversizedHeroRetaining||detachedLegacySawmill)
                {
                    r.enabled=false;
                    count++;
                }
            }
            return count;
        }

        static int StyleCoreRoute()
        {
            int count=0;
            var stair=ValoriaKit.ExternalPbrSurfaceMaterial(
                "cobble",new Color(.34f,.33f,.30f,1f),new Vector2(2.35f,2.35f),.025f,.94f)
                ?? ValoriaKit.DetailedSurfaceMaterial(new Color(.33f,.32f,.29f,1f),"stone",new Vector2(2.35f,2.35f),.96f);

            var routeEarth=ValoriaKit.ExternalPbrSurfaceMaterial(
                "dirt",new Color(.37f,.325f,.255f,1f),new Vector2(1.15f,1.15f),.016f,.90f)
                ?? ValoriaKit.DetailedSurfaceMaterial(new Color(.36f,.315f,.25f,1f),"earth",new Vector2(1.15f,1.15f),.90f);

            foreach(var r in Object.FindObjectsByType<Renderer>(FindObjectsSortMode.None))
            {
                if(r==null||!r.enabled||!r.gameObject.activeInHierarchy)continue;
                string chain=Chain(r.transform);
                string name=r.gameObject.name.ToLowerInvariant();

                Material target=null;
                if(chain.Contains("vpd · vertical stair"))target=stair;
                else if(name.StartsWith("vpd · groundkit main street · street ")||
                        name.Contains("groundkit l1 landing · widening base")||
                        name.Contains("groundkit l1 landing · worn centre"))
                    target=routeEarth;

                if(target==null)continue;
                var mats=r.sharedMaterials;
                for(int i=0;i<mats.Length;i++)mats[i]=target;
                r.sharedMaterials=mats;
                count++;
            }
            return count;
        }

        static int ReframeCoreSideVisuals(Transform root)
        {
            int count=0;
            var original=GameObject.Find("Aserradero · dedicated sawmill");
            if(original!=null)
            {
                var clone=Object.Instantiate(original);
                clone.name="Valoria · Strongest v2 · compact sawmill visual";
                clone.transform.position+=new Vector3(4.20f,.72f,4.75f);
                clone.transform.SetParent(root,true);
                DisableGameplay(clone);

                foreach(var r in original.GetComponentsInChildren<Renderer>(true))r.enabled=false;
                foreach(var l in original.GetComponentsInChildren<Light>(true))l.enabled=false;
                count++;
            }
            return count;
        }

        static int ToneCoreSideBuildings()
        {
            int count=0;
            foreach(var r in Object.FindObjectsByType<Renderer>(FindObjectsSortMode.None))
            {
                if(r==null||!r.enabled||!r.gameObject.activeInHierarchy)continue;
                string chain=Chain(r.transform);
                if(!chain.Contains("aserradero · dedicated sawmill"))continue;

                var src=r.sharedMaterials;
                var dst=new Material[src.Length];
                for(int i=0;i<src.Length;i++)
                {
                    if(src[i]==null){dst[i]=null;continue;}
                    var m=new Material(src[i]){name="Valoria · sawmill grounded · "+src[i].name};
                    var tint=new Color(.54f,.45f,.34f,1f);
                    if(m.HasProperty("_BaseColor"))m.SetColor("_BaseColor",tint);
                    else if(m.HasProperty("_Color"))m.SetColor("_Color",tint);
                    if(m.HasProperty("_Smoothness"))m.SetFloat("_Smoothness",.06f);
                    dst[i]=m;
                }
                r.sharedMaterials=dst;
                count++;
            }
            return count;
        }

        static int NormalizeLowerCitySurfaces()
        {
            int count=0;
            var rock=ValoriaKit.ExternalPbrSurfaceMaterial(
                "rock",new Color(.38f,.37f,.33f,1f),new Vector2(2.7f,2.7f),.018f,.93f)
                ?? ValoriaKit.SurfaceMaterial(new Color(.37f,.36f,.32f,1f),"stone",new Vector2(2.7f,2.7f));
            var earth=ValoriaKit.ExternalPbrSurfaceMaterial(
                "dirt",new Color(.35f,.30f,.23f,1f),new Vector2(3.1f,3.1f),.014f,.94f)
                ?? ValoriaKit.SurfaceMaterial(new Color(.34f,.29f,.22f,1f),"earth",new Vector2(3.1f,3.1f));

            foreach(var r in Object.FindObjectsByType<Renderer>(FindObjectsSortMode.None))
            {
                if(r==null||!r.enabled||!r.gameObject.activeInHierarchy)continue;
                var b=r.bounds;
                string chain=Chain(r.transform);

                if(chain.Contains("backplate")||chain.Contains("bastion hero")||
                   chain.Contains("bastion · dedicated")||chain.Contains("banner")||
                   chain.Contains("flag")||chain.Contains("aserradero")||
                   chain.Contains("cuartel")||chain.Contains("granary")||
                   chain.Contains("granero")||chain.Contains("premium secondary"))
                    continue;

                bool presentation=chain.Contains("valoria ·")||chain.Contains("vpd ·")||
                    chain.Contains("assetlibrary");
                if(!presentation||b.center.y>3.0f)continue;

                bool surface=chain.Contains("rock")||chain.Contains("geology")||
                    chain.Contains("terrain")||chain.Contains("cliff")||
                    chain.Contains("platform")||chain.Contains("terrace")||
                    chain.Contains("support")||chain.Contains("foundation");
                if(!surface)continue;

                var target=(chain.Contains("earth")||chain.Contains("mud")||
                    chain.Contains("groundkit")||chain.Contains("court"))?earth:rock;
                var mats=r.sharedMaterials;
                for(int i=0;i<mats.Length;i++)mats[i]=target;
                r.sharedMaterials=mats;
                count++;
            }
            return count;
        }

        static void SuppressLegacySecondaryPresentation()
        {
            foreach(var r in Object.FindObjectsByType<Renderer>(FindObjectsSortMode.None))
            {
                if(r==null||!r.enabled||!r.gameObject.activeInHierarchy)continue;
                string chain=Chain(r.transform);

                if(chain.Contains("backplate")||chain.Contains("bastion")||
                   chain.Contains("aserradero")||chain.Contains("cuartel")||
                   chain.Contains("granary")||chain.Contains("granero")||
                   chain.Contains("premium secondary"))continue;

                bool obsoleteArchitecture=
                    chain.Contains("composite v2")||
                    chain.Contains("mid-tier district")||
                    chain.Contains("full frame architecture")||
                    chain.Contains("rescued upper civil residence");

                bool obsoletePeripheral=
                    chain.Contains("foreground edge")||
                    chain.Contains("lower cliff authored rock")||
                    chain.Contains("terrainterrace");

                if(obsoleteArchitecture||obsoletePeripheral)r.enabled=false;
            }
        }

        static int ReplaceSecondaryArchitecture(Transform root)
        {
            HideFamily("Valoria · Mid-Tier District v1 · production visual only");
            HideFamily("Valoria · Full Frame Architecture Batch v1");

            var proof=new GameObject("Valoria · Strongest v2 · premium secondary family").transform;
            proof.SetParent(root,true);
            int loaded=0;

            foreach(var s in PremiumSpecs)
            {
                var src=Resources.Load<GameObject>("Valoria/MidTierArchitectureKit_v1/"+s.resource);
                if(src==null)continue;
                var go=Object.Instantiate(src);
                if(go==null)continue;
                go.name="Valoria · Strongest v2 · premium secondary · "+s.id;
                go.transform.rotation=Quaternion.Euler(0f,s.yaw,0f);
                Fit(go,s.p,s.span,s.height);
                Neutralize(go);
                go.transform.SetParent(proof,true);
                DisableGameplay(go);
                loaded++;
            }
            return loaded;
        }

        static int HideLowerTerrainVisual()
        {
            int count=0;
            foreach(var t in Object.FindObjectsByType<Terrain>(FindObjectsSortMode.None))
            {
                if(t==null||!t.gameObject.activeInHierarchy)continue;
                if(t.name.IndexOf("Lower City TerrainData",StringComparison.OrdinalIgnoreCase)<0)continue;
                t.drawHeightmap=false;
                t.drawTreesAndFoliage=false;
                count++;
            }
            return count;
        }

        static int AddAuthoredLowerTerraces(Transform root)
        {
            var art=ValoriaExternalAssetLibrary.Load();
            if(art==null||art.SlavicFlatRock==null)return 0;

            var proof=new GameObject("Valoria · Strongest v2 · authored lower terraces").transform;
            proof.SetParent(root,true);
            int count=0;

            var rock=art.SlavicFlatRock;
            count+=Add(proof,rock,"sawmill terrace a",new Vector3(-7.10f,-.42f,-2.85f),4.20f,.78f,14f,new Color(.46f,.46f,.42f,1f));
            count+=Add(proof,rock,"sawmill terrace b",new Vector3(-5.45f,-.46f,-2.05f),2.80f,.62f,48f,new Color(.43f,.44f,.41f,1f));
            count+=Add(proof,rock,"barracks terrace a",new Vector3(6.70f,-.44f,-3.90f),4.10f,.78f,194f,new Color(.46f,.46f,.42f,1f));
            count+=Add(proof,rock,"barracks terrace b",new Vector3(5.15f,-.48f,-2.75f),2.75f,.62f,226f,new Color(.43f,.44f,.41f,1f));

            count+=Add(proof,rock,"central landing west",new Vector3(-2.15f,-.38f,-1.10f),3.25f,.62f,22f,new Color(.45f,.45f,.42f,1f));
            count+=Add(proof,rock,"central landing east",new Vector3(2.15f,-.40f,-1.10f),3.25f,.62f,202f,new Color(.45f,.45f,.42f,1f));
            count+=Add(proof,rock,"mid terrace west",new Vector3(-3.35f,.88f,3.20f),2.70f,.58f,34f,new Color(.47f,.47f,.44f,1f));
            count+=Add(proof,rock,"mid terrace east",new Vector3(3.35f,.88f,3.25f),2.70f,.58f,214f,new Color(.47f,.47f,.44f,1f));

            if(art.SlavicMudFlat!=null)
            {
                count+=Add(proof,art.SlavicMudFlat,"sawmill earth patch",new Vector3(-7.0f,-.02f,-2.78f),3.30f,.20f,6f,new Color(.62f,.55f,.42f,1f));
                count+=Add(proof,art.SlavicMudFlat,"barracks earth patch",new Vector3(6.65f,-.02f,-3.82f),3.20f,.20f,188f,new Color(.62f,.55f,.42f,1f));
                count+=Add(proof,art.SlavicMudFlat,"central earth patch",new Vector3(0f,-.04f,-1.05f),3.00f,.18f,0f,new Color(.60f,.53f,.41f,1f));
            }
            return count;
        }

        static int ReplaceVerticalStairVisual(Transform root)
        {
            int hidden=0;
            foreach(var r in Object.FindObjectsByType<Renderer>(FindObjectsSortMode.None))
            {
                if(r==null||!r.enabled||!r.gameObject.activeInHierarchy)continue;
                if(!Chain(r.transform).Contains("vpd · vertical stair"))continue;
                r.enabled=false;
                hidden++;
            }

            var art=ValoriaExternalAssetLibrary.Load();
            if(art==null||art.SlavicCobbleRoad==null)return hidden;

            var proof=new GameObject("Valoria · Strongest v2 · authored compact stair").transform;
            proof.SetParent(root,true);
            int added=0;
            for(int i=0;i<11;i++)
            {
                float y=.42f+i*.195f;
                float z=.18f+i*.535f;
                float span=Mathf.Lerp(2.30f,2.62f,i/10f);
                added+=Add(proof,art.SlavicCobbleRoad,"compact stair "+i,
                    new Vector3(0f,y,z),span,.22f,(i%2==0?0f:180f),
                    new Color(.58f,.56f,.51f,1f));
            }
            return added;
        }

        static int AddTerrainEdgeRocks(Transform root)
        {
            var art=ValoriaExternalAssetLibrary.Load();
            if(art==null)return 0;
            var rockA=art.SlavicBoulder??art.SlavicFlatRock;
            var rockB=art.SlavicFlatRock??rockA;
            var rockC=Resources.Load<GameObject>("WorldInventory/Rock02")??rockB;
            if(rockA==null&&rockB==null&&rockC==null)return 0;

            var edgeRoot=new GameObject("Valoria · Strongest v2 · hybrid terrain cliff edges").transform;
            edgeRoot.SetParent(root,true);
            int count=0;

            var specs=new[]{
                new Vector4(-8.35f,-4.15f,28f,0f), new Vector4(-6.15f,-5.15f,61f,1f),
                new Vector4(-5.15f,-2.15f,104f,2f), new Vector4(-3.70f,-.55f,142f,1f),
                new Vector4( 3.65f,-.75f,214f,0f), new Vector4( 5.25f,-2.55f,242f,2f),
                new Vector4( 6.55f,-5.15f,278f,1f), new Vector4( 8.55f,-4.55f,318f,0f)
            };

            for(int i=0;i<specs.Length;i++)
            {
                var s=specs[i];
                GameObject src=s.w<.5f?rockA:(s.w<1.5f?rockB:rockC);
                if(src==null)continue;
                float span=(i==1||i==6)?2.75f:2.45f;
                float height=(i==1||i==6)?1.72f:1.38f;
                count+=Add(edgeRoot,src,"terrain cliff edge "+i,
                    new Vector3(s.x,-1.02f,s.y),span,height,s.z,
                    new Color(.48f,.48f,.44f,1f));
            }
            return count;
        }

        static int AddLocalLowerSupports(Transform root)
        {
            var art=ValoriaExternalAssetLibrary.Load();
            if(art==null||art.SlavicFlatRock==null)return 0;

            var supportRoot=new GameObject("Valoria · Strongest v2 · local lower supports").transform;
            supportRoot.SetParent(root,true);

            int count=0;
            count+=Add(supportRoot,art.SlavicFlatRock,"sawmill local bed",
                new Vector3(-7.18f,-.10f,-2.78f),3.55f,.82f,18f,new Color(.50f,.49f,.45f,1f));
            count+=Add(supportRoot,art.SlavicFlatRock,"barracks local bed",
                new Vector3(6.60f,-.12f,-3.82f),3.70f,.88f,196f,new Color(.49f,.48f,.44f,1f));

            // One smaller shoulder each ties the buildings back toward the stair/central rock
            // without rebuilding a full-width lower platform.
            count+=Add(supportRoot,art.SlavicFlatRock,"sawmill inner shoulder",
                new Vector3(-5.55f,-.18f,-2.55f),2.10f,.58f,42f,new Color(.46f,.46f,.42f,1f));
            count+=Add(supportRoot,art.SlavicFlatRock,"barracks inner shoulder",
                new Vector3(5.05f,-.18f,-3.35f),2.15f,.58f,214f,new Color(.46f,.46f,.42f,1f));
            return count;
        }

        static int AddBuriedSideRuins(Transform root)
        {
            var art=ValoriaExternalAssetLibrary.Load();
            if(art==null)return 0;
            var proof=new GameObject("Valoria · Strongest v2 · buried side ruins").transform;
            proof.SetParent(root,true);
            int count=0;

            count+=Add(proof,art.MegaHalfGate,"west half arch",new Vector3(-8.45f,.32f,6.00f),2.55f,3.90f,28f,new Color(.58f,.56f,.51f,1f));
            count+=Add(proof,art.MegaDestroyedTower,"west broken tower",new Vector3(-9.20f,.18f,7.35f),1.90f,3.35f,42f,new Color(.52f,.51f,.48f,1f));
            count+=Add(proof,art.MegaHalfGate,"east half arch",new Vector3(8.55f,.30f,6.10f),2.60f,4.00f,205f,new Color(.58f,.56f,.51f,1f));
            count+=Add(proof,art.MegaDestroyedTower,"east broken tower",new Vector3(9.25f,.16f,7.45f),1.85f,3.25f,222f,new Color(.52f,.51f,.48f,1f));

            var rock=Resources.Load<GameObject>("WorldInventory/Rock02");
            if(rock!=null)
            {
                count+=Add(proof,rock,"west burial",new Vector3(-8.70f,.06f,5.55f),2.10f,.90f,18f,new Color(.39f,.39f,.35f,1f));
                count+=Add(proof,rock,"east burial",new Vector3(8.80f,.06f,5.65f),2.10f,.90f,198f,new Color(.39f,.39f,.35f,1f));
            }
            return count;
        }

        static int Add(Transform root,GameObject source,string role,Vector3 ground,float footprint,float maxHeight,float yaw,Color tint)
        {
            if(source==null)return 0;
            var go=ValoriaKit.BenchmarkPieceIntegrated("Valoria · Strongest v2 · "+role,source,ground,footprint,maxHeight,
                Quaternion.Euler(0f,yaw,0f),tint);
            if(go==null)return 0;
            go.transform.SetParent(root,true);
            DisableGameplay(go);
            return 1;
        }

        static void HideFamily(string rootName)
        {
            var go=GameObject.Find(rootName);
            if(go==null)return;
            foreach(var r in go.GetComponentsInChildren<Renderer>(true))r.enabled=false;
            foreach(var l in go.GetComponentsInChildren<Light>(true))l.enabled=false;
        }

        static void Fit(GameObject go,Vector3 ground,float span,float maxHeight)
        {
            var rs=go.GetComponentsInChildren<Renderer>(true);
            if(rs.Length==0)return;
            var b=rs[0].bounds;
            for(int i=1;i<rs.Length;i++)b.Encapsulate(rs[i].bounds);
            float scale=Mathf.Min(span/Mathf.Max(.001f,Mathf.Max(b.size.x,b.size.z)),
                                  maxHeight/Mathf.Max(.001f,b.size.y));
            go.transform.localScale*=scale;
            rs=go.GetComponentsInChildren<Renderer>(true);
            b=rs[0].bounds;
            for(int i=1;i<rs.Length;i++)b.Encapsulate(rs[i].bounds);
            go.transform.position+=ground-new Vector3(b.center.x,b.min.y,b.center.z);
        }

        static void Neutralize(GameObject go)
        {
            foreach(var r in go.GetComponentsInChildren<Renderer>(true))
            {
                var src=r.sharedMaterials;var dst=new Material[src.Length];
                for(int i=0;i<src.Length;i++)
                {
                    if(src[i]==null){dst[i]=null;continue;}
                    var m=new Material(src[i]){name="Valoria Strongest v2 · "+src[i].name};
                    string n=(r.name+" "+src[i].name).ToLowerInvariant();
                    Color tint=(n.Contains("roof")||n.Contains("tile")||n.Contains("shingle"))
                        ?new Color(.34f,.37f,.38f,1f)
                        :(n.Contains("wood")||n.Contains("beam")||n.Contains("timber"))
                            ?new Color(.36f,.28f,.21f,1f)
                            :new Color(.67f,.63f,.55f,1f);
                    if(m.HasProperty("_BaseColor"))m.SetColor("_BaseColor",tint);
                    else if(m.HasProperty("_Color"))m.SetColor("_Color",tint);
                    if(m.HasProperty("_Smoothness"))m.SetFloat("_Smoothness",.035f);
                    dst[i]=m;
                }
                r.sharedMaterials=dst;
            }
        }

        static void DisableGameplay(GameObject go)
        {
            foreach(var c in go.GetComponentsInChildren<Collider>(true))c.enabled=false;
            foreach(var h in go.GetComponentsInChildren<WorldHotspot>(true))Object.DestroyImmediate(h);
            foreach(var b in go.GetComponentsInChildren<MonoBehaviour>(true))if(!(b is WorldHotspot))b.enabled=false;
        }

        static string Chain(Transform t)
        {
            string s="";
            for(var p=t;p!=null;p=p.parent)s+="|"+p.name.ToLowerInvariant();
            return s;
        }

        static void WriteVisibleRendererAudit(Camera c,string path)
        {
            var rows=new System.Collections.Generic.List<string>();
            foreach(var r in Object.FindObjectsByType<Renderer>(FindObjectsSortMode.None))
            {
                if(r==null||!r.enabled||!r.gameObject.activeInHierarchy)continue;
                var b=r.bounds;
                if(Mathf.Max(b.size.x,Mathf.Max(b.size.y,b.size.z))<.42f)continue;
                var v=c.WorldToViewportPoint(b.center);
                if(v.z<=0f||v.x<-.12f||v.x>1.12f||v.y<-.30f||v.y>1.08f)continue;

                var mats=new System.Text.StringBuilder();
                float minLum=99f,maxLum=-1f;
                bool anyTex=false;
                foreach(var m in r.sharedMaterials)
                {
                    if(m==null)continue;
                    Color col=Color.white;
                    if(m.HasProperty("_BaseColor"))col=m.GetColor("_BaseColor");
                    else if(m.HasProperty("_Color"))col=m.GetColor("_Color");
                    else if(m.HasProperty("_BaseColorFactor"))col=m.GetColor("_BaseColorFactor");
                    float lum=.2126f*col.r+.7152f*col.g+.0722f*col.b;
                    minLum=Mathf.Min(minLum,lum);maxLum=Mathf.Max(maxLum,lum);
                    Texture tex=null;
                    if(m.HasProperty("_BaseMap"))tex=m.GetTexture("_BaseMap");
                    if(tex==null&&m.HasProperty("_MainTex"))tex=m.GetTexture("_MainTex");
                    if(tex==null&&m.HasProperty("_Albedo"))tex=m.GetTexture("_Albedo");
                    if(tex!=null)anyTex=true;
                    if(mats.Length>0)mats.Append(";");
                    mats.Append(m.name);
                }
                if(minLum>90f){minLum=.5f;maxLum=.5f;}

                var chain=new System.Text.StringBuilder();
                for(var t=r.transform;t!=null;t=t.parent)
                {
                    if(chain.Length>0)chain.Append(" <- ");
                    chain.Append(t.name);
                }

                float area=Mathf.Max(.001f,b.size.x*b.size.y+b.size.x*b.size.z+b.size.y*b.size.z);
                rows.Add($"{v.x:F3}\t{v.y:F3}\t{v.z:F2}\t{area:F2}\t{minLum:F3}\t{maxLum:F3}\t{(anyTex?"tex":"no_tex")}\t{r.gameObject.name}\t{mats}\tcenter=({b.center.x:F2},{b.center.y:F2},{b.center.z:F2})\tsize=({b.size.x:F2},{b.size.y:F2},{b.size.z:F2})\t{chain}");
            }
            rows.Sort((a,b)=>string.CompareOrdinal(a,b));
            var sb=new System.Text.StringBuilder();
            sb.AppendLine("viewport_x\tviewport_y\tdepth\tbound_area\tmin_lum\tmax_lum\ttexture\trenderer\tmaterials\tbounds_center\tbounds_size\tchain");
            foreach(var row in rows)sb.AppendLine(row);
            File.WriteAllText(path,sb.ToString());
        }

        static void SaveSet(Camera c,string tag,Vector3 p,Vector3 t)
        {
            Save(c,Folder+"/"+tag+"-19.png",p,t,19f,1280,720);
            Save(c,Folder+"/"+tag+"-12.png",p,t,12f,1280,720);
            Save(c,Folder+"/"+tag+"-9.png",p,t,9f,1280,720);
            var productionTarget=new Vector3(0f,3.95f,5.90f);
            Save(c,Folder+"/"+tag+"-production.png",p,productionTarget,8.1f,1280,720);
            ValoriaBackplateCandidateV1.FitAspect(390f/844f);
            Save(c,Folder+"/"+tag+"-mobile.png",p,productionTarget,8.1f,390,844);
            ValoriaBackplateCandidateV1.FitAspect(1280f/720f);
        }

        static void Save(Camera c,string path,Vector3 p,Vector3 t,float size,int w,int h)
        {
            c.transform.position=p;c.transform.LookAt(t);c.orthographic=true;c.orthographicSize=size;
            var rt=new RenderTexture(w,h,24,RenderTextureFormat.ARGB32);var prev=RenderTexture.active;
            try{
                c.targetTexture=rt;c.Render();c.Render();RenderTexture.active=rt;
                var im=new Texture2D(w,h,TextureFormat.RGB24,false);
                im.ReadPixels(new Rect(0,0,w,h),0,0);im.Apply();
                File.WriteAllBytes(path,im.EncodeToPNG());Object.DestroyImmediate(im);
            } finally {
                c.targetTexture=null;RenderTexture.active=prev;rt.Release();Object.DestroyImmediate(rt);
            }
        }
    }
}
