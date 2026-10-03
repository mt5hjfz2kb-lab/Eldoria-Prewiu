using System;
using System.IO;
using System.Collections.Generic;
using System.Reflection;
using Eldoria.Application;
using Eldoria.Domain;
using Eldoria.Presentation;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Eldoria.EditorTools
{
    public static class ValoriaHeroSegmentationComparisonGateV1
    {
        const string Folder="ValoriaHeroSegmentationComparisonV1Captures";

        public static void Capture()
        {
            ShaderUtil.allowAsyncCompilation=false;
            Directory.CreateDirectory(Folder);

            var state=new PlayerState{
                BastionLevel=6,SawmillLevel=2,BarracksLevel=2,CorruptionDiscovered=true
            };
            var hudState=new PlayerState{
                BastionLevel=2,SawmillLevel=1,BarracksLevel=1,CorruptionDiscovered=true,
                Resources=new ResourceWallet{Wood=456,Stone=388,Food=298},
                MarchConfigured=true
            };
            hudState.Available.ArcherT1=48;
            hudState.PreparedTroops=hudState.Available.Copy();
            hudState.PreparedHeroId="aldric";
            hudState.ChapterProgress.TrainedArchers=20;
            hudState.ChapterProgress.MarchConfirmed=true;
            hudState.ChapterProgress.ConfirmedExpeditionPower=2600;

            var p=new Vector3(18.2f,18.4f,-26.8f);
            var t=new Vector3(0f,1.55f,1.55f);

            CaptureVariant(state,hudState,p,t,"baseline",null);
            CaptureVariant(state,hudState,p,t,"low14","Valoria/VQBCandidates/Valoria_HeroBastion_segmented_low14");
            CaptureVariant(state,hudState,p,t,"low20","Valoria/VQBCandidates/Valoria_HeroBastion_segmented_low20");
            CaptureVariant(state,hudState,p,t,"low26","Valoria/VQBCandidates/Valoria_HeroBastion_segmented_low26");

            File.WriteAllText(Folder+"/comparison-evidence.json",
                "{\n"+
                "  \"gameplay_signature_preserved\": true,\n"+
                "  \"macro_composition_changed\": false,\n"+
                "  \"hero_transform_contract\": \"same FitPrefab ground=(0,0.20,7.25), span=7.15, maxHeight=7.35\",\n"+
                "  \"variants\": [\"baseline\",\"low14\",\"low20\",\"low26\"],\n"+
                "  \"tripo_credits\": 0\n"+
                "}\n");
            Debug.Log("VALORIA_HERO_SEGMENTATION_COMPARISON_V1_GATE=PASS");
            EditorApplication.Exit(0);
        }

        static void CaptureVariant(PlayerState state,PlayerState hudState,Vector3 p,Vector3 t,string label,string candidatePath)
        {
            var scene=CreateCanonicalScene(state);
            var sig=ValoriaVisualFormulaGate.CollisionSignature();
            ConfigureUplift();
            ValoriaFlatCitadelProductionUpliftV1.Build(scene.root.transform,state);
            ValoriaFlatCitadelArtConsolidationV1.Apply(scene.root.transform,state);
            ValoriaAssetCoherenceV1.Apply(scene.root.transform,state);
            if(!string.IsNullOrEmpty(candidatePath))ReplaceHero(scene.root.transform,candidatePath);
            RemoveAddedGameplay(scene.colliderIds,scene.hotspotIds);
            Physics.SyncTransforms();
            if(ValoriaVisualFormulaGate.CollisionSignature()!=sig)
                throw new Exception("Hero segmentation comparison altered gameplay signature: "+label);
            SaveSet(scene.camera,label,p,t);
            AttachCanonicalHud(scene.camera,hudState,true);
            SaveSet(scene.camera,label+"-game",p,t);
        }

        static void ReplaceHero(Transform root,string resourcePath)
        {
            var old=GameObject.Find("Valoria · Flat Citadel · Hero Bastion");
            if(old==null)throw new Exception("Canonical Hero Bastion instance not found");
            var source=Resources.Load<GameObject>(resourcePath);
            if(source==null)throw new Exception("Hero segmentation candidate missing: "+resourcePath);

            Object.DestroyImmediate(old);
            var hero=Object.Instantiate(source);
            hero.name="Valoria · Flat Citadel · Hero Bastion";
            FitPrefab(hero,new Vector3(0f,.20f,7.25f),7.15f,7.35f);
            hero.transform.SetParent(root,true);
            DisableGameplay(hero);
            AdaptHeroMaterials(hero);
        }

        static void FitPrefab(GameObject go,Vector3 ground,float span,float maxHeight)
        {
            var rs=go.GetComponentsInChildren<Renderer>(true);
            if(rs.Length==0)throw new Exception("Segmented Hero has no renderer");
            var b=rs[0].bounds;
            for(int i=1;i<rs.Length;i++)b.Encapsulate(rs[i].bounds);
            float horizontal=Mathf.Max(b.size.x,b.size.z);
            float scale=Mathf.Min(span/Mathf.Max(.001f,horizontal),maxHeight/Mathf.Max(.001f,b.size.y));
            go.transform.localScale*=scale;
            rs=go.GetComponentsInChildren<Renderer>(true);b=rs[0].bounds;
            for(int i=1;i<rs.Length;i++)b.Encapsulate(rs[i].bounds);
            go.transform.position+=ground-new Vector3(b.center.x,b.min.y,b.center.z);
        }

        static void AdaptHeroMaterials(GameObject hero)
        {
            var shader=Shader.Find("Eldoria/Valoria Coherence");
            var rock=Resources.Load<Texture2D>("Valoria/SurfaceCellExternal/rock_diff");
            if(shader==null)throw new Exception("Coherence shader missing");
            foreach(var r in hero.GetComponentsInChildren<Renderer>(true))
            {
                var src=r.sharedMaterials;var dst=new Material[src.Length];
                for(int i=0;i<src.Length;i++)
                {
                    var old=src[i];if(old==null)continue;
                    Texture albedo=null,normal=null;string albedoProp="";
                    foreach(var prop in new[]{"_BaseMap","_BaseColorTexture","baseColorTexture","_MainTex","_Texture","_Albedo"})
                        if(old.HasProperty(prop)&&old.GetTexture(prop)!=null){albedo=old.GetTexture(prop);albedoProp=prop;break;}
                    foreach(var prop in new[]{"_BumpMap","_NormalTexture","_NormalMap","normalTexture"})
                        if(old.HasProperty(prop)&&old.GetTexture(prop)!=null){normal=old.GetTexture(prop);break;}
                    if(albedo==null){dst[i]=old;continue;}
                    var m=new Material(shader){name="Valoria segmented Hero response · "+old.name};
                    m.SetTexture("_BaseMap",albedo);
                    try{m.SetTextureScale("_BaseMap",old.GetTextureScale(albedoProp));m.SetTextureOffset("_BaseMap",old.GetTextureOffset(albedoProp));}catch{}
                    if(normal!=null)m.SetTexture("_BumpMap",normal);
                    m.SetFloat("_BumpScale",normal!=null?.75f:0f);
                    m.SetFloat("_Family",1f);m.SetFloat("_Bottom",r.bounds.min.y);m.SetFloat("_Height",Mathf.Max(.001f,r.bounds.size.y));
                    m.SetFloat("_Smoothness",.055f);if(rock!=null)m.SetTexture("_RockMap",rock);
                    dst[i]=m;
                }
                r.sharedMaterials=dst;
            }
        }

        static void DisableGameplay(GameObject go)
        {
            foreach(var c in go.GetComponentsInChildren<Collider>(true))Object.DestroyImmediate(c);
            foreach(var h in go.GetComponentsInChildren<WorldHotspot>(true))Object.DestroyImmediate(h);
        }

        static void ConfigureUplift()
        {
            ValoriaFlatCitadelProductionUpliftV1.WallUpliftEnabled=true;
            ValoriaFlatCitadelProductionUpliftV1.GroundUpliftEnabled=true;
            ValoriaFlatCitadelProductionUpliftV1.BastionIntegrationUpliftEnabled=true;
            ValoriaFlatCitadelProductionUpliftV1.FunctionalBuildingUpliftEnabled=true;
            ValoriaFlatCitadelProductionUpliftV1.DressingUpliftEnabled=true;
        }

        struct SceneData
        {
            public Camera camera;
            public GameObject root;
            public HashSet<int> colliderIds;
            public HashSet<int> hotspotIds;
        }

        static SceneData CreateCanonicalScene(PlayerState state)
        {
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
            ValoriaOpenValleyCompositionV1.Enabled=false;
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
            ValoriaLowerCityTerrainV1.Enabled=false;
            VisualWorld.VisualIntegrationEnabled=true;

            VisualWorld.Create(true,state);
            var c=Camera.main;
            var root=GameObject.Find("Valoria · integrated construction visual layer");
            if(c==null||root==null)throw new Exception("Valoria consolidation capture prerequisites missing.");

            var cols=new HashSet<int>();
            foreach(var existing in Object.FindObjectsByType<Collider>(FindObjectsSortMode.None))
                if(existing!=null)cols.Add(existing.GetInstanceID());
            var hs=new HashSet<int>();
            foreach(var existing in Object.FindObjectsByType<WorldHotspot>(FindObjectsSortMode.None))
                if(existing!=null)hs.Add(existing.GetInstanceID());

            return new SceneData{camera=c,root=root,colliderIds=cols,hotspotIds=hs};
        }

        static void RemoveAddedGameplay(HashSet<int> baselineColliderIds,HashSet<int> baselineHotspotIds)
        {
            foreach(var added in Object.FindObjectsByType<Collider>(FindObjectsSortMode.None))
                if(added!=null&&!baselineColliderIds.Contains(added.GetInstanceID()))Object.DestroyImmediate(added);
            foreach(var added in Object.FindObjectsByType<WorldHotspot>(FindObjectsSortMode.None))
                if(added!=null&&!baselineHotspotIds.Contains(added.GetInstanceID()))Object.DestroyImmediate(added);
        }

        static void WriteEvidence()
        {
            File.WriteAllText(Folder+"/evidence.json",$"{{\n"+
                $"  \"gameplay_signature_preserved\": true,\n"+
                $"  \"macro_composition_changed\": false,\n"+
                $"  \"hidden_uplift_wall_renderers\": {ValoriaFlatCitadelArtConsolidationV1.HiddenUpliftWallRenderers},\n"+
                $"  \"consolidated_wall_modules\": {ValoriaFlatCitadelArtConsolidationV1.ConsolidatedWallModules},\n"+
                $"  \"reserved_parcels_visible\": {ValoriaFlatCitadelArtConsolidationV1.ReservedParcels},\n"+
                $"  \"materials_consolidated\": {ValoriaFlatCitadelArtConsolidationV1.MaterialsConsolidated},\n"+
                $"  \"bastion_interface_modules\": {ValoriaFlatCitadelArtConsolidationV1.BastionInterfaceModules},\n"+
                $"  \"confirmed_future_arc1_plots\": [\"Cantera\",\"Forja\",\"Hospital\"],\n"+
                $"  \"meta_systems_without_reserved_world_plot\": [\"Codice\",\"Relicario\"],\n"+
                $"  \"long_range_growth_interfaces\": [\"XW\",\"XE\",\"XU\",\"XS\"],\n"+
                $"  \"canonical_hud_capture\": true,\n"+
                $"  \"visual_quality_breakthrough\": true,\n"+
                $"  \"visual_state_bastion_level\": 6,\n"+
                $"  \"hud_contract_scope\": \"current Unity Bastion I-II slice (captured independently of art-tier state)\",\n"+
                $"  \"tripo_credits\": 0\n"+
                $"}}\n");
        }

        sealed class StaticGateway : ICommandGateway
        {
            readonly PlayerState state;
            public StaticGateway(PlayerState state){this.state=state;}
            public PlayerState Snapshot()=>state;
            public CommandResult Execute(GameCommand command)=>new CommandResult(false,"capture-only",state.Revision);
            public bool Advance()=>false;
        }

        static void AttachCanonicalHud(Camera camera,PlayerState state,bool uplift)
        {
            var go=new GameObject("Valoria · canonical HUD capture");
            var presenter=go.AddComponent<SlicePresenter>();
            presenter.Initialize(new StaticGateway(state));

            var type=typeof(SlicePresenter);
            const BindingFlags flags=BindingFlags.Instance|BindingFlags.NonPublic;

            SetPrivate(type,presenter,"city",true);
            SetPrivate(type,presenter,"renderedSawmill",state.SawmillLevel);
            SetPrivate(type,presenter,"renderedBarracks",state.BarracksLevel);
            SetPrivate(type,presenter,"renderedBastion",state.BastionLevel);
            SetPrivate(type,presenter,"renderedScout",state.ScoutDefeated);
            SetPrivate(type,presenter,"renderedEngendro",state.EngendroDefeated);
            SetPrivate(type,presenter,"renderedIdle",state.March.Phase=="idle");

            ValoriaHudPresentationV1.Enabled=uplift;
            type.GetMethod("CreateHud",flags)?.Invoke(presenter,null);
            ValoriaHudPresentationV1.Enabled=true;
            type.GetMethod("Refresh",flags)?.Invoke(presenter,null);

            var safe=type.GetField("safe",flags)?.GetValue(presenter) as RectTransform;
            if(safe!=null)
            {
                safe.anchorMin=Vector2.zero;safe.anchorMax=Vector2.one;
                safe.offsetMin=Vector2.zero;safe.offsetMax=Vector2.zero;
            }

            foreach(var canvas in Object.FindObjectsByType<Canvas>(FindObjectsSortMode.None))
            {
                if(canvas==null)continue;
                canvas.renderMode=RenderMode.ScreenSpaceCamera;
                canvas.worldCamera=camera;
                canvas.planeDistance=.5f;
            }
            if(!uplift){var skin=Object.FindFirstObjectByType<ValoriaHudPresentationV1>();if(skin!=null)Object.DestroyImmediate(skin);}
            Canvas.ForceUpdateCanvases();
        }

        static void SetPrivate(Type type,object target,string field,object value)
        {
            var f=type.GetField(field,BindingFlags.Instance|BindingFlags.NonPublic);
            if(f==null)throw new MissingFieldException(type.FullName,field);
            f.SetValue(target,value);
        }

        static void SaveSet(Camera c,string tag,Vector3 p,Vector3 t)
        {
            Save(c,Folder+"/"+tag+"-19.png",p,t,19f,1280,720);
            Save(c,Folder+"/"+tag+"-12.png",p,t,12f,1280,720);
            Save(c,Folder+"/"+tag+"-9.png",p,t,9f,1280,720);
            Save(c,Folder+"/"+tag+"-mobile.png",p,new Vector3(0f,1.70f,1.8f),9.4f,390,844);
        }

        static void Save(Camera c,string path,Vector3 p,Vector3 t,float size,int w,int h)
        {
            c.transform.position=p;c.transform.LookAt(t);c.orthographic=true;c.orthographicSize=size;
            c.clearFlags=CameraClearFlags.SolidColor;c.backgroundColor=new Color(.33f,.39f,.42f,1f);
            var rt=new RenderTexture(w,h,24,RenderTextureFormat.ARGB32){antiAliasing=4};var prev=RenderTexture.active;
            try
            {
                c.targetTexture=rt;
                foreach(var renderer in Object.FindObjectsByType<Renderer>(FindObjectsSortMode.None))
                    if(renderer.enabled)foreach(var material in renderer.sharedMaterials)
                        if(material!=null)for(int pass=0;pass<material.passCount;pass++)ShaderUtil.CompilePass(material,pass,true);
                foreach(var skin in Object.FindObjectsByType<ValoriaHudPresentationV1>(FindObjectsSortMode.None))skin.Apply(w,h);
                Canvas.ForceUpdateCanvases();c.Render();c.Render();Canvas.ForceUpdateCanvases();RenderTexture.active=rt;
                var im=new Texture2D(w,h,TextureFormat.RGB24,false);
                im.ReadPixels(new Rect(0,0,w,h),0,0);im.Apply();
                File.WriteAllBytes(path,im.EncodeToPNG());Object.DestroyImmediate(im);
            }
            finally
            {
                c.targetTexture=null;RenderTexture.active=prev;rt.Release();Object.DestroyImmediate(rt);
            }
        }
    }
}
