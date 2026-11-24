#if UNITY_EDITOR
using System;
using System.IO;
using System.Text;
using System.Collections.Generic;
using System.Security.Cryptography;
using Eldoria.Application;
using Eldoria.Domain;
using Eldoria.Infrastructure;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Eldoria.Experimentos.ValoriaModular25D.Editor
{
    /// <summary>
    /// Genuine engine-source validation, not image compositing and not PlayMode.
    /// Frames are Camera.Render from actual SpriteRenderers, meshes, Unity
    /// sorting and Transform movement. The isolated real LocalGateway, with
    /// a saved and reloaded FileStateStore, supplies the three sawmill stages.
    /// Do not infer touch/UI/iPhone/art acceptance from this Editor-only gate.
    /// </summary>
    public static class Modular25DNativeCapture
    {
        [Serializable] public sealed class Frame
        {
            public string name, file, sha256, type;
            public int width, height, visiblePixelKinds;
            public float orthographicSize, cameraX, cameraY;
        }
        [Serializable] public sealed class Evidence
        {
            public string engine, engineVersion, sourceBranch, sourceSha, scenePath, generatedAtUtc;
            public string mode, artApproved="NO", physicalIphone="NOT_TESTED";
            public bool nativeWorldRender, gatewayBuild, savedAndReloaded, noDoublePayment, didNotAlterBeta;
            public bool continuousTerrainPresent, paintedGroundSourceLoaded, roadAndFootprintKits, noLegacyHudInEditorPreview, movingCharactersAreUnityTransforms, workshopActorAnimated, composedSettlementZones;
            public bool historicSchema1Restored;
            public bool progressionVisualFixturesOnly=true;
            public int progressionStagesCaptured;
            public int movementFrameRate=15, movementFrames=120;
            public float movementSimulationSeconds=8f;
            public int sourceFrames, originalArtResourcesLoaded, bastionLevel, initialWood, afterStartWood, afterCompletedWood, sawmillLevel, completionReceiptCount;
            public string disclaimer="Unity Editor Camera.Render on real scene, no PlayMode, no UI overlay, no physical device.";
            public List<Frame> frames = new List<Frame>();
        }
        sealed class Clock : IClock
        {
            public long UtcTicks { get; private set; }
            public Clock(long ticks) { UtcTicks=ticks; }
            public void AdvanceSeconds(int seconds) { UtcTicks+=TimeSpan.FromSeconds(seconds).Ticks; }
        }

        public static void Capture()
        {
            var output=Path.GetFullPath(Path.Combine(UnityEngine.Application.dataPath,"../Artifacts/ValoriaModular25D/Native"));
            Directory.CreateDirectory(output);
            var sourceSha=Environment.GetEnvironmentVariable("GITHUB_SHA") ?? "LOCAL_UNPINNED";
            var report=new Evidence{
                engine="Unity", engineVersion=UnityEngine.Application.unityVersion,
                sourceBranch=Environment.GetEnvironmentVariable("GITHUB_REF_NAME") ?? "LOCAL",
                sourceSha=sourceSha,
                scenePath=Modular25DEditorTasks.ScenePath,
                mode="Editor_CameraRender_REAL_UNITY_NOT_PLAYMODE",
                generatedAtUtc=DateTime.UtcNow.ToString("o")
            };

            // A genuinely independently saved test sandbox, never the shipped
            // iPhone/WebGL saved game or a fake independent economy.
            SliceContentProfiles.SetRuntimeProfileOverride(SliceContentProfiles.OwnerIiiId);
            try
            {
                var scene=EditorSceneManager.OpenScene(Modular25DEditorTasks.ScenePath,OpenSceneMode.Single);
                var roots=scene.GetRootGameObjects();
                Modular25DWorld world=null;
                foreach(var root in roots)
                {
                    world=root.GetComponentInChildren<Modular25DWorld>(true);
                    if(world!=null)break;
                }
                if(world==null)throw new Exception("Native source scene lacks real Modular25DWorld");
                world.BuildForNativeEditor();
                if(world.ViewCamera==null||world.SawmillRenderer==null)
                    throw new Exception("Unity Camera/Renderer source failed to instantiate");
                var ground=world.GetComponentInChildren<Modular25DGround>();
                if(ground==null || ground.ChunkCount!=81)
                    throw new Exception("Ground chunks are not actually present: "+(ground==null?-1:ground.ChunkCount));
                var landscape=world.GetComponentInChildren<Modular25DLandscape>();
                if(landscape==null||landscape.RoadCount!=6+ValoriaUrbanPlan.City.BoroughStreets.Length||landscape.FootprintCount!=1||
                   !landscape.OriginalCobbleRoadLoaded)
                    throw new Exception("Actual Unity spatial terrain integration missing: roads="+
                        (landscape==null?-1:landscape.RoadCount)+" footprints="+
                        (landscape==null?-1:landscape.FootprintCount));
                report.continuousTerrainPresent=true;
                var actualGroundRenderer=ground.GetComponentInChildren<MeshRenderer>();
                if(!ground.OriginalProjectMeadowLoaded ||
                   actualGroundRenderer==null ||
                   actualGroundRenderer.sharedMaterial==null ||
                   actualGroundRenderer.sharedMaterial.mainTexture==null ||
                   actualGroundRenderer.sharedMaterial.mainTexture!=Resources.Load<Texture2D>("ValoriaModular25D/BastionMeadow") ||
                   actualGroundRenderer.sharedMaterial.mainTexture.width!=1024)
                    throw new Exception("Native capture requires the source-pinned 1024px Bastion meadow material on the actual terrain renderer.");
                report.paintedGroundSourceLoaded=true;
                report.roadAndFootprintKits=true;
                report.composedSettlementZones=world.SettlementZonesPlaced==4 &&
                    world.RecoveredPropsPlaced>=15 && ground.HasCivicCourt && landscape.GetComponentsInChildren<SpriteRenderer>(true).Length==1;
                if(!report.composedSettlementZones)
                    throw new Exception("Semantic four-zone settlement / civic clearing not built");
                var originalArt=new[]{
                    "bastion_i","sawmill_i","sawmill_available","sawmill_under_construction",
                    "oak_0","oak_1","ruin_arch_0","ruin_arch_1",
                    "stone","bush_0","bush_1","bush_2","tuft_0","tuft_1"
                };
                foreach(var key in originalArt)
                {
                    var sprite=Resources.Load<Sprite>("ValoriaModular25D/"+key);
                    if(sprite==null)
                        throw new Exception("Missing original independent 2.5D Sprite: "+key);
                    report.originalArtResourcesLoaded++;
                }
                var importedProps=new[]{
                    "ArbolAntiguo","ArboledaAntigua","MatojosFlores","MuroDerruido",
                    "PozoPiedra","CarroMadera","VallaLenador","HuertosValoria"
                };
                foreach(var name in importedProps)
                {
                    var original=Resources.Load<Sprite>(
                        "ValoriaModular25D/RecoveredProps/"+name);
                    if(original==null)
                        throw new Exception("Missing original artist-rendered Eldoria source: "+name);
                    report.originalArtResourcesLoaded++;
                }
                foreach(var actor in new[]{"villager_walk_0","villager_walk_1",
                    "worker_saw_0","worker_saw_1"})
                {
                    var illustration=actor.StartsWith("worker_saw_",StringComparison.Ordinal)
                        ? Modular25DArt.Sawyer(actor.EndsWith("_0",StringComparison.Ordinal)?0:1)
                        : Resources.Load<Sprite>("ValoriaModular25D/"+actor);
                    if(illustration==null)
                        throw new Exception("Original painted two-frame actor source unavailable: "+actor);
                    report.originalArtResourcesLoaded++;
                }
                if(world.RecoveredPropsPlaced<10||report.originalArtResourcesLoaded!=26)
                    throw new Exception("Visual source cannot pass if high-detail originals are not ACTUALLY instantiated: "+
                        world.RecoveredPropsPlaced+" placed, "+report.originalArtResourcesLoaded+" registered.");
                report.noLegacyHudInEditorPreview=world.Gameplay==null&&world.Hud==null;
                if(!report.noLegacyHudInEditorPreview)
                    throw new Exception("Native Editor probe unexpectedly spawned gameplay or legacy HUD");
                var clock=new Clock(new DateTime(2026,11,19,10,0,0,DateTimeKind.Utc).Ticks);
                var historicSource=Path.GetFullPath(Path.Combine(UnityEngine.Application.dataPath,
                    "../../docs/evidence/valoria-layered-construction-20261016/playable-entry-v3/complete-playerstate-before-reset.json"));
                var historicCopy=Path.Combine(output,"historic-schema1-copy.json");
                File.Copy(historicSource,historicCopy,false);
                var oldStore=new FileStateStore(historicCopy);
                var previous=oldStore.Load();
                var restored=new LocalGateway(clock,oldStore).Snapshot();
                if(previous.BastionLevel!=2 || previous.BarracksLevel!=1 || restored.BastionLevel!=previous.BastionLevel ||
                    restored.BarracksLevel!=previous.BarracksLevel || restored.SawmillLevel!=previous.SawmillLevel ||
                    restored.PlayerId!=previous.PlayerId)
                    throw new InvalidDataException("Archived Valoria save did not retain its districts or player");
                world.Apply(restored);
                if(!world.BarracksRenderer.gameObject.activeSelf || world.GranaryRenderer.gameObject.activeSelf ||
                    (Vector2)world.BarracksRenderer.transform.position!=ValoriaUrbanPlan.City.Barracks.Foot)
                    throw new InvalidDataException("Archived BII must resolve to the current military parcel");
                report.historicSchema1Restored=true;
                var savePath=Path.Combine(output,"isolated-localgateway-"+sourceSha+".json");
                // Run unique source SHA in scratch; never erase another's data.
                if(File.Exists(savePath))
                    throw new IOException("Evidence sandbox exists; refuse accidental reset: "+savePath);
                var store=new FileStateStore(savePath);
                var gateway=new LocalGateway(clock,store);
                var state=gateway.Snapshot();
                report.bastionLevel=state.BastionLevel;
                report.initialWood=state.Resources.Wood;
                if(state.BastionLevel!=1 || state.SawmillLevel!=0)
                    throw new Exception("Native sandbox did not start at Bastion I");

                var cam=world.ViewCamera;
                var home=Modular25DCameraInput.HomePosition;
                var portrait=Modular25DCameraInput.HomeSizeForAspect(390f/844f);
                var landscapeOrtho=Modular25DCameraInput.HomeSizeForAspect(844f/390f);
                world.Apply(state);
                AssertCurrentBuildingArt(world);
                if(world.WorkshopAction==null||world.WorkshopAction.gameObject.activeInHierarchy)
                    throw new Exception("Worker visible or missing at unbuilt sawmill");
                Render(cam,output,report,"01-available-390x844",390,844,home,portrait);
                Render(cam,output,report,"02-available-1170x2532",1170,2532,home,portrait);
                Render(cam,output,report,"03-landscape-844x390",844,390,home,landscapeOrtho);
                Render(cam,output,report,"04-bastion-closeup",1170,2532,(Vector3)ValoriaUrbanPlan.City.Keep.Foot+new Vector3(0,1.3f,-30f),4.5f);
                Render(cam,output,report,"05-available-sawmill-close",1170,2532,(Vector3)ValoriaUrbanPlan.City.Mill.Foot+new Vector3(0,.8f,-30f),4.6f);

                // These are exactly the user's canonical game rules, not
                // developer-controlled presentation toggles or fake cost values.
                var command=new GameCommand("mod25d-native-sawmill-"+sourceSha,state.PlayerId,
                    "Build","sawmill",state.Revision);
                var built=gateway.Execute(command);
                if(!built.Ok)throw new Exception("Canonical Build:sawmill rejected: "+built.Message);
                var during=gateway.Snapshot();
                report.afterStartWood=during.Resources.Wood;
                if(during.Resources.Wood!=report.initialWood-SliceRules.SawmillWoodCost ||
                   ParcelBuildingStates.For(during,"sawmill")!=ParcelBuildingState.UNDER_CONSTRUCTION)
                    throw new Exception("Canonical debit or worksite transition was not applied");
                world.Apply(during);
                AssertCurrentBuildingArt(world);
                if(world.WorkshopAction.gameObject.activeInHierarchy)
                    throw new Exception("Worker productive before completion of real worksite");
                Render(cam,output,report,"06-real-building-worksite",390,844,home,portrait);
                Render(cam,output,report,"07-worksite-closeup",1170,2532,(Vector3)ValoriaUrbanPlan.City.Mill.Foot+new Vector3(0,.8f,-30f),4.6f);
                report.gatewayBuild=true;

                clock.AdvanceSeconds(SliceRules.SawmillBuildSeconds+1);
                gateway=new LocalGateway(clock,store); // Real reload from FileStateStore.
                var complete=gateway.Snapshot();
                if(complete.SawmillLevel!=1 ||
                   ParcelBuildingStates.For(complete,"sawmill")!=ParcelBuildingState.BUILT ||
                   !complete.CompletedCommandIds.Contains(command.Id) ||
                   complete.CompletedTaskIds.FindAll(x=>x==("sawmill:"+command.Id)).Count!=1)
                    throw new Exception("Genuine offline completion/save reload failed");
                report.sawmillLevel=complete.SawmillLevel;
                report.afterCompletedWood=complete.Resources.Wood;
                report.completionReceiptCount=complete.ChapterProgress.GrantedRewards.FindAll(x=>x=="c1-sawmill").Count;
                if(report.completionReceiptCount!=1)throw new Exception("Chapter-I receipt missing or duplicated");
                world.Apply(complete);
                AssertCurrentBuildingArt(world);
                if(!world.WorkshopAction.gameObject.activeInHierarchy)
                    throw new Exception("Finished sawmill worker not active after real save reload");
                Render(cam,output,report,"08-sawmill-built-390x844",390,844,home,portrait);
                Render(cam,output,report,"09-sawmill-built-1170x2532",1170,2532,home,portrait);
                Render(cam,output,report,"10-built-sawmill-closeup",1170,2532,(Vector3)ValoriaUrbanPlan.City.Mill.Foot+new Vector3(0,.8f,-30f),4.6f);
                // Wider photographic checkpoints are REAL Unity frames of
                // actual independent objects; no synthesized composite.
                Render(cam,output,report,"11-city-mobile-hero-390x844",
                    390,844,home,portrait*1.04f);
                Render(cam,output,report,"12-ruins-backdrop",
                    390,844,(Vector3)ValoriaUrbanPlan.City.Keep.Foot+new Vector3(0,2.1f,-30f),6.2f);
                Render(cam,output,report,"13-tree-occlusion-ground",
                    390,844,new Vector3(-2.42f,-1.35f,-30f),5.2f);
                Render(cam,output,report,"14-civic-commons-detail",
                    1170,2532,(Vector3)ValoriaUrbanPlan.City.Junction+new Vector3(0,0,-30f),5.3f);
                Render(cam,output,report,"15-lumberyard-worker-detail",
                    1170,2532,(Vector3)ValoriaUrbanPlan.City.Mill.Foot+new Vector3(.8f,-.2f,-30f),4.0f);

                var moneyBeforeRetry=complete.Resources.Wood;
                var again=gateway.Execute(new GameCommand("mod25d-native-duplicate-attempt-"+sourceSha,
                    complete.PlayerId,"Build","sawmill",complete.Revision));
                if(again.Ok||gateway.Snapshot().Resources.Wood!=moneyBeforeRetry)
                    throw new Exception("Duplicate construction was accepted/paid twice");
                report.noDoublePayment=true;
                report.savedAndReloaded=true;
                report.didNotAlterBeta=savePath.StartsWith(output,StringComparison.OrdinalIgnoreCase);
                // Real Unity Editor scene graph and real camera changes in
                // 120 sequential frames at 1/15 s match the export frame rate.
                // This is a real-time-speed Editor capture, not a PlayMode/FPS benchmark.
                var walkers=UnityEngine.Object.FindObjectsByType<Modular25DWalker>(FindObjectsSortMode.None);
                if(walkers.Length<2)throw new Exception("Missing real independent actor sprites");
                var workshop=UnityEngine.Object.FindObjectsByType<Modular25DWorkshopAction>(
                    FindObjectsSortMode.None);
                if(workshop.Length!=1||world.WorkshopWorkersPlaced!=1||!workshop[0].Configured)
                    throw new Exception("One real illustrated lumberyard worker required");
                cam.orthographicSize=6.4f;
                for(int i=0;i<report.movementFrames;i++)
                {
                    foreach(var w in walkers)w.StepForNativeCapture(1f/report.movementFrameRate);
                    workshop[0].StepForNativeCapture(1f/report.movementFrameRate);
                    var phase=i/(float)(report.movementFrames-1);
                    var offset=new Vector3(Mathf.Lerp(-1.70f,.45f,phase),
                        Mathf.Lerp(.13f,1.75f,phase),-30f);
                    float ortho=portrait-Mathf.Sin(phase*Mathf.PI)*1.2f;
                    Render(cam,output,report,"movement-"+i.ToString("D3"),390,844,offset,ortho);
                }
                report.movingCharactersAreUnityTransforms=true;
                report.workshopActorAnimated=workshop[0].WorkCycles>3f;
                if(!report.workshopActorAnimated)
                    throw new Exception("No verifiable animated work cycle at sawmill log rack");
                // Presentation fixtures, deliberately not an end-to-end campaign claim.
                // No fixture is written into the gateway, beta, or any player save.
                foreach(int level in new[]{5,4,3,2,1})
                {
                    var fixture=new PlayerState { BastionLevel=level, SawmillLevel=1,
                        BarracksLevel=level>=2?1:0 };
                    fixture.ChapterThree.GranaryLevel=level>=3?1:0;
                    fixture.ChapterThree.StoneworksLevel=level>=5?1:0;
                    world.Apply(fixture);
                    if(world.BastionRenderer.sprite!=Modular25DArt.BastionForLevel(level))
                        throw new Exception("Wrong stage artwork "+level);
                    if(world.BarracksRenderer.gameObject.activeSelf!=(level>=2) ||
                       world.GranaryRenderer.gameObject.activeSelf!=(level>=3) ||
                       world.StoneworksRenderer.gameObject.activeSelf!=(level>=5))
                        throw new Exception("Progression visual gate mismatch "+level);
                    // The previous art review mistakenly used Bastion V's
                    // full-region camera for EVERY stage II-V. That made the
                    // native certification frames look smaller than the live
                    // progressively framed player. Render each level using
                    // the exact same Home state as Modular25DCameraInput.
                    var framing=level==1?home:
                        Modular25DCameraInput.ExpansionCenterForLevel(level);
                    float size=level==1?portrait:
                        Modular25DCameraInput.ExpansionSizeForLevel(390f/844f,level);
                    Render(cam,output,report,"stage-"+level+"-fixture-390x844",390,844,framing,size);
                    Render(cam,output,report,"stage-"+level+"-registered-entry",390,844,new Vector3(0,1.9f,-30f),6.600513f);
                    cam.transform.position=framing;cam.orthographicSize=size;
                    ValidateUrbanPlan(world,level);
                    AssertProgressionComposition(cam,world,level);
                    Render(cam,output,report,"stage-"+level+"-fixture-1170x2532",1170,2532,framing,size);
                    Render(cam,output,report,"stage-"+level+"-keep-detail",1170,1170,(Vector3)ValoriaUrbanPlan.City.Keep.Foot+new Vector3(0,1.65f,-30f),3.3f);
                    if(level==5)
                    {
                        RecordPlan(output,sourceSha);
                        Render(cam,output,report,"stage-5-district-1170x2532",1170,2532,(Vector3)ValoriaUrbanPlan.City.Freight+new Vector3(0,1,-30f),6.6f);
                        Render(cam,output,report,"stage-5-baseline-camera",1170,2532,new Vector3(-.02f,-1.5f,-30f),9.15f);
                        var stops = new[] { world.BastionRenderer, world.SawmillRenderer,
                            world.BarracksRenderer, world.GranaryRenderer, world.StoneworksRenderer };
                        for (int n=0; n<stops.Length; n++)
                        {
                            var center=stops[n].bounds.center;
                            center.z=-30f;
                            Render(cam,output,report,"district-close-"+n,390,844,center,6.600513f);
                        }
                        // Cameras recorded in the source manifest of the reviewed bedeb5f release.
                        var earlierCameras=new[]{new Vector2(0.00000000f,4.64062452f), new Vector2(-4.00000000f,0.22281241f), new Vector2(6.00000000f,1.90277839f), new Vector2(3.00000000f,-3.58159614f), new Vector2(-7.00000000f,-6.21760559f)};
                        for(int n=0;n<earlierCameras.Length;n++)
                            Render(cam,output,report,"stage-5-registered-"+n,390,844,
                                (Vector3)earlierCameras[n]+Vector3.back*30f,6.600513f);
                        var city=ValoriaUrbanPlan.City;
                        var sites=new[]{city.UpperRow,city.CourtCenter,city.Junction,city.WorkshopRow,
                            city.Mill.Foot,city.Barracks.Foot,city.Granary.Foot,city.MarketRow,city.Quarry.Foot,
                            city.Growth[0].center,city.Growth[1].center};
                        var names=new[]{"postigo","nucleo","calle-mayor","portales","aserradero","distrito-militar",
                            "abastos","casa-del-peso","cantera","reserva-oeste","reserva-nordeste"};
                        for(int i=0;i<sites.Length;i++)
                            Render(cam,output,report,"stage-5-"+names[i],780,1040,
                                (Vector3)sites[i]+new Vector3(0,.75f,-30),4.8f);
                        var tour=new[]{(Vector2)framing,city.CourtCenter,city.UpperRow,city.UpperCourt,
                            city.Growth[1].center,city.Barracks.Foot,city.Junction,city.MarketRow,
                            city.Granary.Foot,city.Quarry.Foot,city.Growth[0].center,city.Mill.Foot,
                            city.WorkshopRow,city.CourtCenter};
                        const int framesPerLeg=24, fps=12;
                        int lastFrame=(tour.Length-1)*framesPerLeg;
                        for(int frame=0;frame<=lastFrame;frame++)
                        {
                            int leg=Mathf.Min(frame/framesPerLeg,tour.Length-2);
                            float progress=(frame-leg*framesPerLeg)/(float)framesPerLeg;
                            float eased=Mathf.SmoothStep(0f,1f,Mathf.InverseLerp(.16f,.86f,progress));
                            var center=Vector3.Lerp(tour[leg],tour[leg+1],eased);
                            center.z=-30f;
                            float zoom=leg==0 ? Mathf.Lerp(size,6.600513f,eased) : 6.600513f;
                            foreach(var walker in walkers) walker.StepForNativeCapture(1f/fps);
                            workshop[0].StepForNativeCapture(1f/fps);
                            Render(cam,output,report,"district-tour-"+frame.ToString("D3"),390,844,center,zoom);
                        }
                    }
                    report.progressionStagesCaptured++;
                    // Unlocking a tier alone must never conjure completed architecture.
                    fixture.BarracksLevel=0;fixture.ChapterThree.GranaryLevel=0;fixture.ChapterThree.StoneworksLevel=0;
                    world.Apply(fixture);
                    if(world.BarracksRenderer.gameObject.activeSelf || world.GranaryRenderer.gameObject.activeSelf ||
                       world.StoneworksRenderer.gameObject.activeSelf)
                        throw new Exception("Unbuilt future architecture leaked "+level);
                }
                world.Apply(complete);
                report.nativeWorldRender=report.frames.Count>=42;
                report.sourceFrames=report.frames.Count;
                if(!report.nativeWorldRender)throw new Exception("Fewer frames than expected");
                File.WriteAllText(Path.Combine(output,"evidence.json"),
                    JsonUtility.ToJson(report,true),Encoding.UTF8);
                Debug.Log("[MODULAR25D] REAL_UNITY_EDITOR_NATIVE_FRAMES "+report.frames.Count+
                  " canonical_gameplay=PASS save_reload=PASS exact_sha="+sourceSha+
                  " NO_ART_APPROVAL NO_PLAYMODE NO_DEVICE_QA");
            }
            finally
            {
                SliceContentProfiles.SetRuntimeProfileOverride(null);
            }
        }

        // Native Unity geometry gate: level unlocks must be visible on the
        // 390x844 player camera without reducing every castle to a tiny icon.
        // This checks REAL authored SpriteRenderer bounds, not painted mockups.
        static void AssertProgressionComposition(Camera camera,Modular25DWorld world,int level)
        {
            if(level<=1)return;
            var travel=ValoriaUrbanPlan.City.CameraTravel;
            foreach(var visual in new[]{world.BastionRenderer,world.SawmillRenderer,world.BarracksRenderer,
                    world.GranaryRenderer,world.StoneworksRenderer})
            {
                if(!visual || !visual.gameObject.activeInHierarchy || !visual.sprite)continue;
                Vector2 center=visual.bounds.center;
                if(!travel.Contains(center))throw new InvalidOperationException("District unreachable by pan: "+visual.name);
                var available=2f*camera.orthographicSize*camera.aspect;
                if(visual.bounds.size.x>available)throw new InvalidOperationException("District wider than visit camera: "+visual.name);
            }
            var home=camera.WorldToViewportPoint(world.BastionRenderer.bounds.center);
            if(home.x<.18f || home.x>.82f || home.y<.25f || home.y>.80f)
                throw new InvalidOperationException("Bastion outside home composition");
        }

        [Serializable] sealed class MappedLot
        {
            public string id;
            public Vector2 foot,door,size;
            public Vector2[] road;
        }
        [Serializable] sealed class MappedReserve
        {
            public Vector2 lower,size;
            public Vector2[] access;
        }
        [Serializable] sealed class RecordedPlan
        {
            public string source;
            public MappedLot[] lots;
            public MappedReserve[] reserves;
            public Vector2[] freightRoad;
            public Vector2 junction,freight;
        }
        static void RecordPlan(string output,string source)
        {
            var p=ValoriaUrbanPlan.City;
            var plots=new[]{p.Keep,p.Mill,p.Barracks,p.Granary,p.Quarry};
            var routes=p.GrowthApproaches;
            var report=new RecordedPlan{source=source,junction=p.Junction,freight=p.Freight,
                freightRoad=p.FreightRoad,lots=new MappedLot[plots.Length],reserves=new MappedReserve[routes.Length]};
            for(int i=0;i<plots.Length;i++){
                var lot=plots[i];
                report.lots[i]=new MappedLot{id=lot.Id,foot=lot.Foot,door=lot.Door,size=lot.GroundSize,road=p.Approach(lot)};
            }
            for(int i=0;i<routes.Length;i++)
                report.reserves[i]=new MappedReserve{lower=p.Growth[i].min,size=p.Growth[i].size,access=routes[i]};
            File.WriteAllText(Path.Combine(output,"plano-urbano.json"),JsonUtility.ToJson(report,true),Encoding.UTF8);
        }

        static void ValidateUrbanPlan(Modular25DWorld world, int level)
        {
            var plan=ValoriaUrbanPlan.City;
            var plots=new[]{plan.Keep,plan.Mill,plan.Barracks,plan.Granary,plan.Quarry};
            var buildings=new[]{world.BastionRenderer,world.SawmillRenderer,world.BarracksRenderer,
                world.GranaryRenderer,world.StoneworksRenderer};
            var landscape=world.GetComponentInChildren<Modular25DLandscape>();
            var roads=landscape.GetComponentsInChildren<MeshFilter>(true);
            if(roads.Length!=6+plan.BoroughStreets.Length)throw new InvalidDataException("Unexpected legacy or duplicate street mesh");
            for(int i=0;i<plots.Length;i++)
            {
                var plot=plots[i];
                var b=buildings[i];
                Vector2 foot=b.transform.position;
                if(Vector2.Distance(foot,plot.Foot)>.001f)
                    throw new InvalidDataException(plot.Id+" has a second source of position");
                var road=System.Array.Find(roads,r=>r.name==plot.Id+" | acceso");
                if(!road || road.gameObject.activeInHierarchy!=b.gameObject.activeInHierarchy)
                    throw new InvalidDataException("Street visible state differs from parcel "+plot.Id+" at "+level);
                var mesh=road.sharedMesh.vertices;
                var end=(Vector2)road.transform.TransformPoint(mesh[mesh.Length-5]);
                if(Vector2.Distance(end,plot.Door)>.002f)throw new InvalidDataException("Street misses threshold "+plot.Id);
                if(!b.gameObject.activeInHierarchy)continue;
                // Check the effective GPU sprite registration, not only its transform.
                // A centred texture can satisfy the road-coordinate check while
                // drawing the entrance halfway across the approach.
                var sprite=b.sprite;
                float support=sprite.pivot.y/sprite.rect.height;
                if(support>=.25f || support<0f)
                    throw new InvalidDataException(plot.Id+" imports its centre as its ground contact: "+support);
                if(i==0){
                    var gate=b.transform.TransformPoint(new Vector3(0,
                        (.07f*sprite.rect.height-sprite.pivot.y)/sprite.pixelsPerUnit,0));
                    if(Vector2.Distance(gate,plot.Door)>.42f)
                        throw new InvalidDataException("Painted keep gate misses the road at stage "+level);
                }
                for(int j=i+1;j<plots.Length;j++)
                {
                    if(!buildings[j].gameObject.activeInHierarchy)continue;
                    if(b.bounds.Intersects(buildings[j].bounds))
                        throw new InvalidDataException("Architectural silhouettes intersect: "+plot.Id+"/"+plots[j].Id);
                }
                var occupied=new Rect(foot-plot.GroundSize*.5f,plot.GroundSize);
                foreach(var reserve in plan.Growth)
                    if(reserve.Overlaps(occupied))throw new InvalidDataException("Parcel occupies expansion area: "+plot.Id);
            }
            var frontageFeet=new System.Collections.Generic.List<Rect>();
            foreach(var prop in plan.Environment)
            {
                if(level<prop.FirstLevel || level>prop.LastLevel)continue;
                if(prop.Asset!="AceraZapateros" && prop.Asset!="CasaDelGremio" && prop.Asset!="ManzanaDelAdarve" && prop.Asset!="CalleDelPostigo" && prop.Asset!="PortalesDeOficios" && prop.Asset!="CasasDelPeso" && prop.Asset!="CasasDelCamino" && prop.Asset!="CasasDeOficio" && prop.Asset!="ArmeriaDelPatio" &&
                   prop.Asset!="PuestoDeAbastos" && prop.Asset!="FrenteTejedores" && prop.Asset!="LonjaGrano")continue;
                var sprite=Resources.Load<Sprite>("ValoriaModular25D/RecoveredProps/"+prop.Asset);
                if(!sprite)throw new InvalidDataException("Missing street frontage: "+prop.Asset);
                float width=sprite.rect.width/sprite.pixelsPerUnit*prop.Scale;
                var size=new Vector2(width*.85f,Mathf.Min(.68f,width*.30f));
                var footprint=new Rect(prop.Foot-size*.5f,size);
                foreach(var reserve in plan.Growth)
                    if(reserve.Overlaps(footprint))throw new InvalidDataException("Street frontage occupies reserve "+prop.Asset);
                if(frontageFeet.Exists(other=>other.Overlaps(footprint)))throw new InvalidDataException("Frontages overlap at "+prop.Asset);
                frontageFeet.Add(footprint);
            }
            // Sweep a cart-width test along each reserved access in the built city.
            foreach(var route in plan.GrowthApproaches)
            {
                for(int n=1;n<route.Length;n++)
                {
                    int samples=Mathf.CeilToInt(Vector2.Distance(route[n-1],route[n])/.15f);
                    for(int step=0;step<=samples;step++)
                    {
                        Vector2 point=Vector2.Lerp(route[n-1],route[n],step/(float)samples);
                        var cart=new Rect(point-new Vector2(.38f,.38f),new Vector2(.76f,.76f));
                        if(frontageFeet.Exists(front=>front.Overlaps(cart)))
                            throw new InvalidDataException("Furniture or frontage blocks future cart access at "+point);
                        foreach(var parcel in plots)
                        {
                            var clearance=parcel.GroundSize+new Vector2(.76f,.76f);
                            if(new Rect(parcel.Foot-clearance*.5f,clearance).Contains(point))
                                throw new InvalidDataException("Growth approach blocked by "+parcel.Id);
                        }
                    }
                }
            }
            if(!world.HitSawmill(plan.Mill.Foot) || world.HitSawmill(plan.Keep.Foot))
                throw new InvalidDataException("Sawmill touch target detached from its parcel");
        }

        static void AssertCurrentBuildingArt(Modular25DWorld world)
        {
            var renderer=world.SawmillRenderer.GetComponentInChildren<MeshRenderer>();
            if(renderer==null||renderer.sharedMaterial==null||
               renderer.sharedMaterial.GetTexture("_ShadowTex")!=world.SawmillRenderer.sprite.texture)
                throw new Exception("Ground shadow must match the CURRENT canonical building state before render");
            if(world.BastionRenderer.sprite!=Resources.Load<Sprite>("ValoriaModular25D/bastion_i_hd"))
                throw new Exception("Native camera did not receive high-definition Bastion artwork");
        }

        internal static void Render(Camera cam,string folder,Evidence report,string name,
            int width,int height,Vector3 position,float orthoSize)
        {
            cam.transform.position=position;
            cam.orthographicSize=orthoSize;
            cam.aspect=width/(float)height;
            if(name=="01-available-390x844"||name=="02-available-1170x2532"||
               name=="03-landscape-844x390"||name=="08-sawmill-built-390x844"||
               name=="09-sawmill-built-1170x2532")
            {
                // Previous tightly zoomed run 447295 cropped BOTH premium
                // buildings despite a point-only static camera unit test.
                // Assert exact native SpriteRenderer GPU-space bounds, never
                // synthetic rectangles or a mock composite of the scene.
                float halfH=cam.orthographicSize;
                float halfW=halfH*cam.aspect;
                float left=cam.transform.position.x-halfW+.12f;
                float right=cam.transform.position.x+halfW-.12f;
                float bottom=cam.transform.position.y-halfH+.12f;
                float top=cam.transform.position.y+halfH-.12f;
                foreach(var buildingName in new[]{"Bastion_I_unique"})
                {
                    var building=GameObject.Find(buildingName)?.GetComponent<SpriteRenderer>();
                    if(building==null||building.sprite==null||!building.enabled)
                        throw new Exception("Native premium sprite absent: "+buildingName);
                    var aabb=building.bounds;
                    if(aabb.min.x<left||aabb.max.x>right||
                       aabb.min.y<bottom||aabb.max.y>top)
                        throw new Exception("MOBILE_CROPPED_AUTHORED_BUILDING "+name+
                            " "+buildingName+" bounds="+aabb+
                            " camera="+cam.transform.position+
                            " orthographicSize="+halfH);
                }
            }
            var temp=new RenderTexture(width,height,24,RenderTextureFormat.ARGB32);
            var prevTarget=cam.targetTexture;
            var prevActive=RenderTexture.active;
            Texture2D image=null;
            try
            {
                temp.Create();
                cam.targetTexture=temp;
                cam.Render();
                RenderTexture.active=temp;
                image=new Texture2D(width,height,TextureFormat.RGBA32,false);
                image.ReadPixels(new Rect(0,0,width,height),0,0);
                image.Apply();
                var pixels=image.GetPixels32();
                var colors=new HashSet<int>();
                for(int i=0;i<pixels.Length;i+=Math.Max(1,pixels.Length/2048))
                {
                    var c=pixels[i];
                    colors.Add((c.r<<16)|(c.g<<8)|c.b);
                }
                if(colors.Count<12)throw new Exception("Native image is blank: "+name+" unique="+colors.Count);
                var bytes=image.EncodeToPNG();
                var path=Path.Combine(folder,name+".png");
                File.WriteAllBytes(path,bytes);
                string sha;
                using(var h=SHA256.Create())
                    sha=BitConverter.ToString(h.ComputeHash(bytes)).Replace("-","").ToLowerInvariant();
                report.frames.Add(new Frame{
                    name=name,file=Path.GetFileName(path),sha256=sha,
                    type="Unity.Camera.Render actual scene GPU frame (not mockup)",
                    width=width,height=height,visiblePixelKinds=colors.Count,
                    orthographicSize=orthoSize,cameraX=position.x,cameraY=position.y
                });
            }
            finally
            {
                cam.targetTexture=prevTarget;
                RenderTexture.active=prevActive;
                if(image!=null)UnityEngine.Object.DestroyImmediate(image);
                temp.Release();UnityEngine.Object.DestroyImmediate(temp);
            }
        }
    }
}
#endif

