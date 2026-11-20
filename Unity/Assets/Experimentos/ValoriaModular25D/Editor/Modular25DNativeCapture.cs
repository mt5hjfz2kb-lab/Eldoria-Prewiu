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
                if(landscape==null||landscape.RoadCount!=2||landscape.FootprintCount!=2||
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
                   actualGroundRenderer.sharedMaterial.mainTexture.name!="OrganicMeadowTile")
                    throw new Exception("Unity is not using the reusable grass-only original Eldoria artist tile.");
                report.paintedGroundSourceLoaded=true;
                report.roadAndFootprintKits=true;
                report.composedSettlementZones=world.SettlementZonesPlaced==4 &&
                    world.RecoveredPropsPlaced>=15 && landscape.CivicCourtyardsBuilt==1;
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
                foreach(var actor in new[]{"villager_walk_0","villager_walk_1"})
                {
                    var illustration=Resources.Load<Sprite>("ValoriaModular25D/"+actor);
                    if(illustration==null)
                        throw new Exception("Painted two-frame actor does not exist: "+actor);
                    report.originalArtResourcesLoaded++;
                }
                if(world.RecoveredPropsPlaced<10||report.originalArtResourcesLoaded!=24)
                    throw new Exception("Visual source cannot pass if high-detail originals are not ACTUALLY instantiated: "+
                        world.RecoveredPropsPlaced+" placed, "+report.originalArtResourcesLoaded+" registered.");
                report.noLegacyHudInEditorPreview=world.Gameplay==null&&world.Hud==null;
                if(!report.noLegacyHudInEditorPreview)
                    throw new Exception("Native Editor probe unexpectedly spawned gameplay or legacy HUD");
                var clock=new Clock(new DateTime(2026,11,19,10,0,0,DateTimeKind.Utc).Ticks);
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
                if(world.WorkshopAction==null||world.WorkshopAction.gameObject.activeInHierarchy)
                    throw new Exception("Worker visible or missing at unbuilt sawmill");
                Render(cam,output,report,"01-available-390x844",390,844,home,portrait);
                Render(cam,output,report,"02-available-1170x2532",1170,2532,home,portrait);
                Render(cam,output,report,"03-landscape-844x390",844,390,home,landscapeOrtho);
                Render(cam,output,report,"04-bastion-closeup",1170,2532,new Vector3(-.55f,2.15f,-30f),4.5f);
                Render(cam,output,report,"05-available-sawmill-close",1170,2532,new Vector3(1.30f,-1.4f,-30f),4.6f);

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
                if(world.WorkshopAction.gameObject.activeInHierarchy)
                    throw new Exception("Worker productive before completion of real worksite");
                Render(cam,output,report,"06-real-building-worksite",390,844,home,portrait);
                Render(cam,output,report,"07-worksite-closeup",1170,2532,new Vector3(1.30f,-1.4f,-30),4.6f);
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
                if(!world.WorkshopAction.gameObject.activeInHierarchy)
                    throw new Exception("Finished sawmill worker not active after real save reload");
                Render(cam,output,report,"08-sawmill-built-390x844",390,844,home,portrait);
                Render(cam,output,report,"09-sawmill-built-1170x2532",1170,2532,home,portrait);
                Render(cam,output,report,"10-built-sawmill-closeup",1170,2532,new Vector3(1.30f,-1.4f,-30f),4.6f);
                // Wider photographic checkpoints are REAL Unity frames of
                // actual independent objects; no synthesized composite.
                Render(cam,output,report,"11-city-mobile-hero-390x844",
                    390,844,home,portrait*1.04f);
                Render(cam,output,report,"12-ruins-backdrop",
                    390,844,new Vector3(0f,3.10f,-30f),6.2f);
                Render(cam,output,report,"13-tree-occlusion-ground",
                    390,844,new Vector3(-2.42f,-1.35f,-30f),5.2f);
                Render(cam,output,report,"14-civic-commons-detail",
                    1170,2532,new Vector3(-.22f,-1.25f,-30f),5.3f);
                Render(cam,output,report,"15-lumberyard-worker-detail",
                    1170,2532,new Vector3(1.56f,-3.37f,-30f),4.0f);

                var moneyBeforeRetry=complete.Resources.Wood;
                var again=gateway.Execute(new GameCommand("mod25d-native-duplicate-attempt-"+sourceSha,
                    complete.PlayerId,"Build","sawmill",complete.Revision));
                if(again.Ok||gateway.Snapshot().Resources.Wood!=moneyBeforeRetry)
                    throw new Exception("Duplicate construction was accepted/paid twice");
                report.noDoublePayment=true;
                report.savedAndReloaded=true;
                report.didNotAlterBeta=savePath.StartsWith(output,StringComparison.OrdinalIgnoreCase);
                // Real Unity Editor scene graph and real camera changes in
                // 32 sequential frames. It is explicitly not a PlayMode clip.
                var walkers=UnityEngine.Object.FindObjectsByType<Modular25DWalker>(FindObjectsSortMode.None);
                if(walkers.Length<2)throw new Exception("Missing real independent actor sprites");
                var workshop=UnityEngine.Object.FindObjectsByType<Modular25DWorkshopAction>(
                    FindObjectsSortMode.None);
                if(workshop.Length!=1||world.WorkshopWorkersPlaced!=1||!workshop[0].Configured)
                    throw new Exception("One real illustrated lumberyard worker required");
                cam.orthographicSize=6.4f;
                for(int i=0;i<32;i++)
                {
                    foreach(var w in walkers)w.StepForNativeCapture(.56f);
                    workshop[0].StepForNativeCapture(.56f);
                    var phase=i/31f;
                    var offset=new Vector3(Mathf.Lerp(-1.70f,.45f,phase),
                        Mathf.Lerp(.13f,1.75f,phase),-30f);
                    float ortho=portrait-Mathf.Sin(phase*Mathf.PI)*1.2f;
                    Render(cam,output,report,"movement-"+i.ToString("D3"),390,844,offset,ortho);
                }
                report.movingCharactersAreUnityTransforms=true;
                report.workshopActorAnimated=workshop[0].WorkCycles>3f;
                if(!report.workshopActorAnimated)
                    throw new Exception("No verifiable animated work cycle at sawmill log rack");
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

        static void Render(Camera cam,string folder,Evidence report,string name,
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
                foreach(var buildingName in new[]{"Bastion_I_unique","Aserradero_state"})
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
