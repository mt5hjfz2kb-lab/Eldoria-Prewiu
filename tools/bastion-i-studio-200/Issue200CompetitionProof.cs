#if UNITY_EDITOR
using System;
using System.IO;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using Eldoria.Application;
using Eldoria.Domain;
using Eldoria.Infrastructure;

namespace Eldoria.Experimentos.ValoriaModular25D.Editor
{
    public static class Issue200CompetitionProof
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
        static void ValidateArt(Modular25DWorld world,int mask,int level){
            foreach(var p in ValoriaUrbanPlan.City.Environment){
                bool found=false;int ix=Array.IndexOf(Issue200CompetitionArt.Names,p.Asset);
                foreach(var r in world.GetComponentsInChildren<SpriteRenderer>(true)){
                    if(r.name!="Original Eldoria art | "+p.Asset||Vector2.Distance(r.transform.localPosition,p.Foot)>.001f)continue;
                    var old=Resources.Load<Sprite>("ValoriaModular25D/RecoveredProps/"+p.Asset);
                    var expected=level==5&&ix>=0&&(mask&(1<<ix))!=0?Resources.Load<Sprite>("Issue200Competition/"+p.Asset):old;
                    if(r.sprite!=expected||r.sprite.pivot!=old.pivot||r.sprite.pixelsPerUnit!=old.pixelsPerUnit||r.sprite.rect.size!=old.rect.size||
                       Mathf.Abs(r.transform.localScale.x-p.Scale)>.001f||r.sortingOrder!=Modular25DWorld.FootOrder(p.Foot.y))throw new Exception("Sprite import or foot changed "+p.Asset);
                    if(r.gameObject.activeInHierarchy&&ix==0){
                        var sh=r.GetComponentInChildren<MeshRenderer>();
                        if(!sh||sh.sharedMaterial.GetTexture("_ShadowTex")!=expected.texture)throw new Exception("Projected frontage shadow mismatch");
                    }
                    found=true;break;
                }
                if(!found)throw new Exception("Missing original placement "+p.Asset);
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
                foreach(int mask in new[]{0,1,2,4,7}){
                    Issue200CompetitionArt.PreviewMask=mask;
                    var folder=Path.Combine(root,mask==0?"baseline":mask==7?"bundle":"asset-"+mask);Directory.CreateDirectory(folder);
                    var scene=EditorSceneManager.OpenScene(Modular25DEditorTasks.ScenePath,OpenSceneMode.Single);
                    Modular25DWorld world=null;
                    foreach(var r in scene.GetRootGameObjects()){world=r.GetComponentInChildren<Modular25DWorld>(true);if(world!=null)break;}
                    if(world==null)throw new Exception("Original scene missing");
                    world.BuildForNativeEditor();
                    var clock=new Clock();var store=new FileStateStore(Path.Combine(folder,"proof-state.json"));
                    var gateway=new LocalGateway(clock,store);var start=gateway.Snapshot();
                    if(start.BastionLevel!=1||start.SawmillLevel!=0)throw new Exception("Wrong canonical start");
                    world.Apply(start);
                    var command=new GameCommand("issue200-market-"+(mask.ToString()),start.PlayerId,"Build","sawmill",start.Revision);
                    if(!gateway.Execute(command).Ok)throw new Exception("Canonical build rejected");
                    if(gateway.Snapshot().Resources.Wood!=start.Resources.Wood-SliceRules.SawmillWoodCost)throw new Exception("Wrong debit");
                    clock.Advance(SliceRules.SawmillBuildSeconds+1);
                    var done=new LocalGateway(clock,store).Snapshot();
                    if(done.SawmillLevel!=1||done.BastionLevel!=1)throw new Exception("Save reload failed");
                    var e=new Modular25DNativeCapture.Evidence{engine="Unity",engineVersion=UnityEngine.Application.unityVersion,sourceSha=source,mode="Competition mask "+mask,nativeWorldRender=true,gatewayBuild=true,savedAndReloaded=true,bastionLevel=5,sawmillLevel=1};
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
                        ValidateArt(world,mask,level);
                        var moving=HideActors(world);
                        Modular25DNativeCapture.Render(cam,folder,e,"stage-"+level+"-home",390,844,home,size);
                        if(level==5){
                            Modular25DNativeCapture.Render(cam,folder,e,"wide",1170,1170,new Vector3(.5f,-1.4f,-30),10f);
                            foreach(var name in Issue200CompetitionArt.Names){
                                var r=Prop(name);
                                var center=(Vector3)r.transform.position+new Vector3(0,.85f,-30);
                                Modular25DNativeCapture.Render(cam,folder,e,name+"-detail",1170,1170,center,3.3f);
                                Modular25DNativeCapture.Render(cam,folder,e,name+"-mobile",390,844,center,6.600513f);
                            }
                        }
                        foreach(var r in moving)r.enabled=true;
                    }
                    if(mask==0||mask==7){
                        var walkers=UnityEngine.Object.FindObjectsByType<Modular25DWalker>(FindObjectsSortMode.None);
                        for(int i=0;i<36;i++){
                            foreach(var w in walkers)w.StepForNativeCapture(1f/12);
                            world.WorkshopAction.StepForNativeCapture(1f/12);
                            float t=i/35f;
                            var center=new Vector3(Mathf.Lerp(-6.5f,5.85f,t),Mathf.Lerp(-5.5f,1.3f,t),-30);
                            Modular25DNativeCapture.Render(cam,folder,e,"motion-"+i.ToString("D3"),390,844,center,8f);
                        }
                    }
                    var empty=Fixture(5);empty.BarracksLevel=0;empty.ChapterThree.GranaryLevel=0;empty.ChapterThree.StoneworksLevel=0;
                    world.Apply(empty);
                    if(world.BarracksRenderer.gameObject.activeSelf||world.GranaryRenderer.gameObject.activeSelf||world.StoneworksRenderer.gameObject.activeSelf||
                       GameObject.Find("Original Eldoria art | CasasDelPeso")||GameObject.Find("Original Eldoria art | PuestoDeAbastos"))
                        throw new Exception("Unbuilt market art leaked");
                    world.Apply(Fixture(4));ValidateArt(world,mask,4);
                    e.progressionStagesCaptured=5;e.sourceFrames=e.frames.Count;e.movingCharactersAreUnityTransforms=true;
                    File.WriteAllText(Path.Combine(folder,"evidence.json"),JsonUtility.ToJson(e,true));
                }
            }finally{Issue200CompetitionArt.PreviewMask=7;SliceContentProfiles.SetRuntimeProfileOverride(null);}
        }
    }
}
#endif
