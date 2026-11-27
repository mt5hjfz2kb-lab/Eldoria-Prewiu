#if UNITY_EDITOR
using System;
using System.IO;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using Eldoria.Application;
using Eldoria.Domain;
using Eldoria.Infrastructure;

namespace Eldoria.Experimentos.ValoriaModular25D.Editor
{
    // Matched real Unity camera frames. Candidate shaders default ON in actual
    // runtime. Only this editor test turns the same bounded patches OFF for A.
    public static class Issue200CycleSpatialProof
    {
        sealed class Clock : IClock {
            public long UtcTicks {get;private set;} = new DateTime(2026,11,19,10,0,0,DateTimeKind.Utc).Ticks;
            public void Advance(int s){UtcTicks+=TimeSpan.FromSeconds(s).Ticks;}
        }
        static void CaptureStage(Modular25DWorld world,int level,string folder,Modular25DNativeCapture.Evidence e)
        {
            var plan=ValoriaUrbanPlan.City;
            var fixture=new PlayerState{BastionLevel=level,SawmillLevel=1,BarracksLevel=level>=2?1:0};
            fixture.ChapterThree.GranaryLevel=level>=3?1:0;
            fixture.ChapterThree.StoneworksLevel=level>=5?1:0;
            world.Apply(fixture);
            if(world.BastionRenderer.sprite!=Modular25DArt.BastionForLevel(level) ||
               world.SawmillRenderer.sprite!=Resources.Load<Sprite>("ValoriaModular25D/sawmill_i_hd"))
                throw new Exception("Original stage sprites or approved mill were changed");
            if(world.BarracksRenderer.gameObject.activeSelf!=(level>=2)||world.GranaryRenderer.gameObject.activeSelf!=(level>=3)||world.StoneworksRenderer.gameObject.activeSelf!=(level>=5))
                throw new Exception("Wrong canonical progression");
            if(world.GetComponentInChildren<Modular25DGround>().ChunkCount!=81)
                throw new Exception("Continuous ground absent");
            if(world.SawmillRenderer.GetComponentInChildren<MeshRenderer>().sharedMaterial.GetTexture("_ShadowTex")!=world.SawmillRenderer.sprite.texture)
                throw new Exception("Mill shadow not synchronized");
            foreach(var w in UnityEngine.Object.FindObjectsByType<Modular25DWalker>(FindObjectsSortMode.None)){
                int order=Modular25DWorld.FootOrder(w.transform.position.y);
                foreach(var r in w.GetComponentsInChildren<SpriteRenderer>(true))r.sortingOrder=r.name.Contains("shadow")?order-1:order;
            }
            world.WorkshopAction.StepForNativeCapture(0f);
            // Isolate environment art from transient actor positions during static comparison.
            // Actual animated courier remains visible in the separate postern sequence.
            var moving=new System.Collections.Generic.List<SpriteRenderer>();
            foreach(var w in UnityEngine.Object.FindObjectsByType<Modular25DWalker>(FindObjectsSortMode.None))
                foreach(var r in w.GetComponentsInChildren<SpriteRenderer>(true))if(r.enabled){moving.Add(r);r.enabled=false;}
            foreach(var r in world.WorkshopAction.GetComponentsInChildren<SpriteRenderer>(true))
                if(r.enabled){moving.Add(r);r.enabled=false;}
            if(level==5){
                var peso=GameObject.Find("Original Eldoria art | CasasDelPeso");
                if(peso==null || Vector2.Distance((Vector2)peso.transform.localPosition,plan.MarketRow+plan.PesoArtistOffset)>.001f)
                    throw new Exception("Actual Peso foot not registered");
            }
            if(level==4){
                var textile=GameObject.Find("Original Eldoria art | FrenteTejedores");
                if(textile==null || Vector2.Distance((Vector2)textile.transform.localPosition,plan.Junction+new Vector2(-2.15f,-1.57f)+plan.WeaverArtistOffset)>.001f)
                    throw new Exception("Actual weaver foot not registered");
            }
            var cam=world.ViewCamera;
            var home=Modular25DCameraInput.ExpansionCenterForLevel(level);
            float size=Modular25DCameraInput.ExpansionSizeForLevel(390f/844f,level);
            string n="stage-"+level+"-";
            Modular25DNativeCapture.Render(cam,folder,e,n+"home",390,844,home,size);
            Modular25DNativeCapture.Render(cam,folder,e,n+"retina",1170,2532,home,size);
            Modular25DNativeCapture.Render(cam,folder,e,n+"entry",390,844,new Vector3(0,1.9f,-30),6.600513f);
            Modular25DNativeCapture.Render(cam,folder,e,n+"keep",1170,1170,(Vector3)plan.Keep.Foot+new Vector3(0,1.65f,-30),3.3f);
            Modular25DNativeCapture.Render(cam,folder,e,n+"mill",1170,1170,(Vector3)plan.Mill.Foot+new Vector3(0,.8f,-30),3.3f);
            Modular25DNativeCapture.Render(cam,folder,e,n+"junction",1170,1170,(Vector3)plan.Junction+new Vector3(0,.65f,-30),3.3f);
            // Separate camera alternatives, same geometry/material variant for
            // A/B; never silently change the owner's existing runtime framing.
            var fit=level==1?new Vector3(-1.85f,1.7f,-30):new Vector3(.15f,level>=3?-.55f:1.65f,-30);
            float fitWidth=level==1?5.65f:level==2?7.65f:level<=4?7.85f:9.7f;
            Modular25DNativeCapture.Render(cam,folder,e,n+"framing-study",390,844,fit,fitWidth/(390f/844f));
            if(level>=2)Modular25DNativeCapture.Render(cam,folder,e,n+"barracks",1170,1170,(Vector3)plan.Barracks.Foot+new Vector3(0,.8f,-30),3.3f);
            if(level>=3)Modular25DNativeCapture.Render(cam,folder,e,n+"market",1170,1170,(Vector3)plan.Granary.Foot+new Vector3(0,.9f,-30),4.2f);
            if(level==4)Modular25DNativeCapture.Render(cam,folder,e,n+"textile",1170,1170,(Vector3)plan.Junction+new Vector3(-2.15f,-.85f,-30),3.3f);
            if(level==5){
                var sites=new[]{plan.UpperRow,plan.WorkshopRow,plan.MarketRow,plan.UpperCourt,plan.WestFront,plan.EastFront,plan.Quarry.Foot};
                var names=new[]{"postigo","oficios","peso","adarve","zapateros","gremio","cantera"};
                for(int i=0;i<sites.Length;i++)Modular25DNativeCapture.Render(cam,folder,e,n+names[i],1170,1170,(Vector3)sites[i]+new Vector3(0,.82f,-30),3.3f);
                foreach(var r in moving)r.enabled=true;
                var courier=GameObject.Find("Valoria citizen A").GetComponent<Modular25DWalker>();
                if(courier==null)throw new Exception("Postern courier missing");
                var entry=plan.PostigoLane(0);
                for(int tick=0;Vector2.Distance(courier.transform.position,entry)>.11f;tick++){
                    if(tick>=2400)throw new Exception("Postern footpath unreachable");
                    courier.StepForNativeCapture(1f/24f);
                }
                for(int i=0;i<64;i++){
                    courier.StepForNativeCapture(1f/12f);
                    Modular25DNativeCapture.Render(cam,folder,e,"postigo-motion-"+i.ToString("D3"),540,720,(Vector3)plan.UpperRow+new Vector3(0,.85f,-30),2.7f);
                }
            }
            foreach(var r in moving)r.enabled=true;
            // Check unavailable districts are not introduced by art.
            fixture.BarracksLevel=0;fixture.ChapterThree.GranaryLevel=0;fixture.ChapterThree.StoneworksLevel=0;
            world.Apply(fixture);
            if(world.BarracksRenderer.gameObject.activeSelf||world.GranaryRenderer.gameObject.activeSelf||world.StoneworksRenderer.gameObject.activeSelf)
                throw new Exception("Unbuilt progression leaked");
        }
        public static void Capture(){
            string root=Environment.GetEnvironmentVariable("ISSUE200_PROOF_OUT");
            string source=Environment.GetEnvironmentVariable("ISSUE200_BASE_SHA");
            if(String.IsNullOrEmpty(root)||source!="f27f4774e32efc5ad046478a07747eead4256c3e")throw new Exception("Exact base required");
            Directory.CreateDirectory(root);
            SliceContentProfiles.SetRuntimeProfileOverride(SliceContentProfiles.OwnerIiiId);
            try{
                foreach(bool candidate in new[]{false,true}){
                    ValoriaUrbanPlan.SpatialArtistEnabled=candidate;
                    var folder=Path.Combine(root,candidate?"candidate":"baseline");Directory.CreateDirectory(folder);
                    var scene=EditorSceneManager.OpenScene(Modular25DEditorTasks.ScenePath,OpenSceneMode.Single);
                    Modular25DWorld world=null;
                    foreach(var r in scene.GetRootGameObjects()){world=r.GetComponentInChildren<Modular25DWorld>(true);if(world!=null)break;}
                    if(world==null)throw new Exception("Original scene missing");
                    world.BuildForNativeEditor();
                    var clock=new Clock();var store=new FileStateStore(Path.Combine(folder,"proof-state.json"));
                    var gateway=new LocalGateway(clock,store);var start=gateway.Snapshot();
                    if(start.BastionLevel!=1||start.SawmillLevel!=0)throw new Exception("Wrong canonical start");
                    world.Apply(start);
                    var cam=world.ViewCamera;
                    var e=new Modular25DNativeCapture.Evidence{engine="Unity",engineVersion=UnityEngine.Application.unityVersion,sourceSha=source,mode=candidate?"artist physical placement and road mesh candidate":"matched original geometry and materials control",nativeWorldRender=true,bastionLevel=1};
                    var home=Modular25DCameraInput.HomePosition;var size=Modular25DCameraInput.HomeSizeForAspect(390f/844f);
                    Modular25DNativeCapture.Render(cam,folder,e,"unbuilt",390,844,home,size);
                    var command=new GameCommand("issue200-cycle-"+(candidate?"B":"A"),start.PlayerId,"Build","sawmill",start.Revision);
                    if(!gateway.Execute(command).Ok)throw new Exception("Canonical construction rejected");
                    if(gateway.Snapshot().Resources.Wood!=start.Resources.Wood-SliceRules.SawmillWoodCost)throw new Exception("Debit wrong");
                    world.Apply(gateway.Snapshot());
                    Modular25DNativeCapture.Render(cam,folder,e,"construction",390,844,home,size);
                    clock.Advance(SliceRules.SawmillBuildSeconds+1);
                    var done=new LocalGateway(clock,store).Snapshot();
                    if(done.SawmillLevel!=1||done.BastionLevel!=1)throw new Exception("Save reload failed");
                    e.gatewayBuild=true;e.savedAndReloaded=true;e.sawmillLevel=done.SawmillLevel;
                    for(int level=1;level<=5;level++)CaptureStage(world,level,folder,e);
                    e.progressionStagesCaptured=5;e.sourceFrames=e.frames.Count;e.movingCharactersAreUnityTransforms=true;
                    File.WriteAllText(Path.Combine(folder,"evidence.json"),JsonUtility.ToJson(e,true));
                }
            }finally{ValoriaUrbanPlan.SpatialArtistEnabled=true;SliceContentProfiles.SetRuntimeProfileOverride(null);}
        }
    }
}
#endif
