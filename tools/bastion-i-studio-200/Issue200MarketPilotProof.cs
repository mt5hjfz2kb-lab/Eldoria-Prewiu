#if UNITY_EDITOR
using System;
using System.IO;
using System.Collections.Generic;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using Eldoria.Application;
using Eldoria.Domain;
using Eldoria.Infrastructure;

namespace Eldoria.Experimentos.ValoriaModular25D.Editor
{
    public static class Issue200MarketPilotProof
    {
        sealed class Clock : IClock {
            public long UtcTicks {get;private set;} = new DateTime(2026,11,19,10,0,0,DateTimeKind.Utc).Ticks;
            public void Advance(int s){UtcTicks+=TimeSpan.FromSeconds(s).Ticks;}
        }
        static PlayerState Fixture(int level){
            var s=new PlayerState{BastionLevel=level,SawmillLevel=1,BarracksLevel=level>=2?1:0};
            s.ChapterThree.GranaryLevel=level>=3?1:0;
            s.ChapterThree.StoneworksLevel=level>=5?1:0;
            return s;
        }
        static SpriteRenderer Prop(string name){
            var go=GameObject.Find("Original Eldoria art | "+name);
            if(!go)throw new Exception("Active original frontage absent: "+name);
            return go.GetComponent<SpriteRenderer>();
        }
        static void ValidateMarket(bool candidate,int level){
            var plan=ValoriaUrbanPlan.City;
            if(level<4)return;
            foreach(var name in level==5?new[]{"CasasDelPeso","PuestoDeAbastos"}:new[]{"PuestoDeAbastos"}){
                var r=Prop(name);
                var expected=Resources.Load<Sprite>((candidate&&level==5?"Issue200MarketPilot/":"ValoriaModular25D/RecoveredProps/")+name);
                if(!expected||r.sprite!=expected)throw new Exception("Wrong imported sprite "+name);
                var old=Resources.Load<Sprite>("ValoriaModular25D/RecoveredProps/"+name);
                if(r.sprite.pixelsPerUnit!=old.pixelsPerUnit||r.sprite.pivot!=old.pivot||r.sprite.rect.size!=old.rect.size)
                    throw new Exception("Import pivot, canvas or PPU changed "+name);
                Vector2 foot=name=="CasasDelPeso"?plan.MarketRow:plan.Granary.Foot+new Vector2(.73f,-2.12f);
                if(candidate&&level==5)foot+=name=="CasasDelPeso"?Modular25DWorld.MarketPilotPesoOffset:Modular25DWorld.MarketPilotStallOffset;
                if(Vector2.Distance(foot,r.transform.localPosition)>.001f||r.sortingOrder!=Modular25DWorld.FootOrder(foot.y))
                    throw new Exception("Foot registration failed "+name);
                if(name=="CasasDelPeso"){
                    var projection=r.GetComponentInChildren<MeshRenderer>();
                    if(!projection||projection.sharedMaterial.GetTexture("_ShadowTex")!=expected.texture)
                        throw new Exception("Shadow not bound to actual frontage");
                }
            }
        }
        static List<SpriteRenderer> HideActors(Modular25DWorld world){
            var moving=new List<SpriteRenderer>();
            foreach(var w in UnityEngine.Object.FindObjectsByType<Modular25DWalker>(FindObjectsSortMode.None)){
                int order=Modular25DWorld.FootOrder(w.transform.position.y);
                foreach(var r in w.GetComponentsInChildren<SpriteRenderer>(true)){
                    r.sortingOrder=r.name.Contains("shadow")?order-1:order;
                    if(r.enabled){moving.Add(r);r.enabled=false;}
                }
            }
            foreach(var r in world.WorkshopAction.GetComponentsInChildren<SpriteRenderer>(true))
                if(r.enabled){moving.Add(r);r.enabled=false;}
            return moving;
        }
        public static void Capture(){
            string root=Environment.GetEnvironmentVariable("ISSUE200_PROOF_OUT");
            string source=Environment.GetEnvironmentVariable("ISSUE200_BASE_SHA");
            if(String.IsNullOrEmpty(root)||source!="f27f4774e32efc5ad046478a07747eead4256c3e")throw new Exception("Exact base required");
            Directory.CreateDirectory(root);
            SliceContentProfiles.SetRuntimeProfileOverride(SliceContentProfiles.OwnerIiiId);
            try{
                foreach(bool candidate in new[]{false,true}){
                    Modular25DWorld.MarketPilotEnabled=candidate;
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
                    var command=new GameCommand("issue200-market-"+(candidate?"B":"A"),start.PlayerId,"Build","sawmill",start.Revision);
                    if(!gateway.Execute(command).Ok)throw new Exception("Canonical build rejected");
                    if(gateway.Snapshot().Resources.Wood!=start.Resources.Wood-SliceRules.SawmillWoodCost)throw new Exception("Wrong debit");
                    clock.Advance(SliceRules.SawmillBuildSeconds+1);
                    var done=new LocalGateway(clock,store).Snapshot();
                    if(done.SawmillLevel!=1||done.BastionLevel!=1)throw new Exception("Save reload failed");
                    var e=new Modular25DNativeCapture.Evidence{engine="Unity",engineVersion=UnityEngine.Application.unityVersion,sourceSha=source,mode=candidate?"Imported authored market district candidate":"Original market district control",nativeWorldRender=true,gatewayBuild=true,savedAndReloaded=true,bastionLevel=5,sawmillLevel=1};
                    var cam=world.ViewCamera;var plan=ValoriaUrbanPlan.City;
                    var home=Modular25DCameraInput.HomePosition;var size=Modular25DCameraInput.HomeSizeForAspect(390f/844f);
                    for(int level=1;level<=5;level++){
                        world.Apply(Fixture(level));
                        if(world.BastionRenderer.sprite!=Modular25DArt.BastionForLevel(level)||
                           world.SawmillRenderer.sprite!=Resources.Load<Sprite>("ValoriaModular25D/sawmill_i_hd")||
                           world.BarracksRenderer.gameObject.activeSelf!=(level>=2)||
                           world.GranaryRenderer.gameObject.activeSelf!=(level>=3)||
                           world.StoneworksRenderer.gameObject.activeSelf!=(level>=5)||
                           world.GetComponentInChildren<Modular25DGround>().ChunkCount!=81)
                            throw new Exception("Canonical progression or art changed "+level);
                        ValidateMarket(candidate,level);
                        var moving=HideActors(world);
                        Modular25DNativeCapture.Render(cam,folder,e,"stage-"+level+"-home",390,844,home,size);
                        if(level==5){
                            var center=new Vector3(3.15f,-4.15f,-30);
                            Modular25DNativeCapture.Render(cam,folder,e,"market-mobile",390,844,center,6.600513f);
                            Modular25DNativeCapture.Render(cam,folder,e,"market-overview",1170,1170,center,4.7f);
                            Modular25DNativeCapture.Render(cam,folder,e,"market-retina",1170,2532,center,6.600513f);
                            Modular25DNativeCapture.Render(cam,folder,e,"peso",1170,1170,(Vector3)plan.MarketRow+new Vector3(0,.82f,-30),3.3f);
                            Modular25DNativeCapture.Render(cam,folder,e,"granary",1170,1170,(Vector3)plan.Granary.Foot+new Vector3(0,.9f,-30),4.2f);
                            Modular25DNativeCapture.Render(cam,folder,e,"stall",1170,1170,(Vector3)plan.Granary.Foot+new Vector3(.73f,-1.37f,-30),2.8f);
                            Modular25DNativeCapture.Render(cam,folder,e,"junction",1170,1170,(Vector3)plan.Junction+new Vector3(0,.65f,-30),3.3f);
                            Modular25DNativeCapture.Render(cam,folder,e,"entry",390,844,new Vector3(0,1.9f,-30),6.600513f);
                            Modular25DNativeCapture.Render(cam,folder,e,"wide",1170,1170,new Vector3(.5f,-1.4f,-30),10f);
                        }
                        foreach(var r in moving)r.enabled=true;
                    }
                    var walkers=UnityEngine.Object.FindObjectsByType<Modular25DWalker>(FindObjectsSortMode.None);
                    for(int i=0;i<60;i++){
                        foreach(var w in walkers)w.StepForNativeCapture(1f/12);
                        world.WorkshopAction.StepForNativeCapture(1f/12);
                        float t=i/59f;var center=new Vector3(Mathf.Lerp(.64f,3.95f,t),Mathf.Lerp(-1.72f,-5.25f,t),-30);
                        Modular25DNativeCapture.Render(cam,folder,e,"motion-"+i.ToString("D3"),390,844,center,6.6f-Mathf.Sin(t*Mathf.PI)*1.25f);
                    }
                    var empty=Fixture(5);empty.BarracksLevel=0;empty.ChapterThree.GranaryLevel=0;empty.ChapterThree.StoneworksLevel=0;
                    world.Apply(empty);
                    if(world.BarracksRenderer.gameObject.activeSelf||world.GranaryRenderer.gameObject.activeSelf||world.StoneworksRenderer.gameObject.activeSelf||
                       GameObject.Find("Original Eldoria art | CasasDelPeso")||GameObject.Find("Original Eldoria art | PuestoDeAbastos"))
                        throw new Exception("Unbuilt market art leaked");
                    world.Apply(Fixture(4));ValidateMarket(candidate,4);
                    e.progressionStagesCaptured=5;e.sourceFrames=e.frames.Count;e.movingCharactersAreUnityTransforms=true;
                    File.WriteAllText(Path.Combine(folder,"evidence.json"),JsonUtility.ToJson(e,true));
                }
            }finally{Modular25DWorld.MarketPilotEnabled=true;SliceContentProfiles.SetRuntimeProfileOverride(null);}
        }
    }
}
#endif
