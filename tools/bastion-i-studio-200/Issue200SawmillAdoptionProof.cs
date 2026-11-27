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
    // Scratch-only renderer: real scene, canonical gateway and existing rendering components.
    // Never copied to production branches. Same camera and scene for A and B.
    public static class Issue200SawmillAdoptionProof
    {
        sealed class Clock : IClock {
            public long UtcTicks {get;private set;} = new DateTime(2026,11,19,10,0,0,DateTimeKind.Utc).Ticks;
            public void Advance(int s){UtcTicks+=TimeSpan.FromSeconds(s).Ticks;}
        }
        public static void Capture(){
            var root=Environment.GetEnvironmentVariable("ISSUE200_PROOF_OUT");
            var assets=Environment.GetEnvironmentVariable("ISSUE200_PROOF_ASSETS");
            if(String.IsNullOrEmpty(root)||String.IsNullOrEmpty(assets))throw new Exception("Explicit scratch paths required");
            Directory.CreateDirectory(root);
            SliceContentProfiles.SetRuntimeProfileOverride(SliceContentProfiles.OwnerIiiId);
            try {
                foreach(var candidate in new[]{true}) {
                    string folder=Path.Combine(root,candidate?"candidate":"baseline");Directory.CreateDirectory(folder);
                    var scene=EditorSceneManager.OpenScene(Modular25DEditorTasks.ScenePath,OpenSceneMode.Single);
                    Modular25DWorld world=null;
                    foreach(var r in scene.GetRootGameObjects()){world=r.GetComponentInChildren<Modular25DWorld>(true);if(world!=null)break;}
                    if(world==null)throw new Exception("Actual scene missing");
                    world.BuildForNativeEditor();
                    var clock=new Clock();var store=new FileStateStore(Path.Combine(folder,"proof-state.json"));
                    var gateway=new LocalGateway(clock,store);var start=gateway.Snapshot();
                    if(start.BastionLevel!=1||start.SawmillLevel!=0)throw new Exception("Wrong starting fixture");
                    var command=new GameCommand("issue200-proof-"+(candidate?"B":"A"),start.PlayerId,"Build","sawmill",start.Revision);
                    var result=gateway.Execute(command);if(!result.Ok)throw new Exception("Canonical build rejected");
                    if(gateway.Snapshot().Resources.Wood!=start.Resources.Wood-SliceRules.SawmillWoodCost)throw new Exception("Wrong debit");
                    clock.Advance(SliceRules.SawmillBuildSeconds+1);
                    var done=new LocalGateway(clock,store).Snapshot();
                    if(done.BastionLevel!=1||done.SawmillLevel!=1)throw new Exception("Canonical state did not reload");
                    world.Apply(done);
                    if(world.BarracksRenderer.gameObject.activeSelf||world.GranaryRenderer.gameObject.activeSelf||world.StoneworksRenderer.gameObject.activeSelf)throw new Exception("Future progression leaked");
                    if(world.BastionRenderer.sprite!=Resources.Load<Sprite>("ValoriaModular25D/bastion_i_hd"))
                        throw new Exception("Original Bastion sprite not loaded");
                    if(world.SawmillRenderer.sprite!=Resources.Load<Sprite>("ValoriaModular25D/sawmill_i_hd"))
                        throw new Exception("Owner-approved imported sawmill not loaded");
                    if((Vector2)world.WorkshopAction.transform.localPosition!=ValoriaUrbanPlan.City.MillBench)
                        throw new Exception("Local sawyer contact anchor mismatch");
                    var shadow=world.SawmillRenderer.GetComponentInChildren<MeshRenderer>();
                    if(shadow==null||shadow.sharedMaterial.GetTexture("_ShadowTex")!=world.SawmillRenderer.sprite.texture)
                        throw new Exception("Shadow not using new imported sprite");
                    var repeat=gateway.Execute(command);
                    if(new LocalGateway(clock,store).Snapshot().Resources.Wood!=done.Resources.Wood)
                        throw new Exception("Duplicate command changed wood");
                    // Initialize the actual sprites at their real ground-order before either capture.
                    foreach(var walker in UnityEngine.Object.FindObjectsByType<Modular25DWalker>(FindObjectsSortMode.None)){
                        int order=Modular25DWorld.FootOrder(walker.transform.position.y);
                        foreach(var renderer in walker.GetComponentsInChildren<SpriteRenderer>(true))
                            renderer.sortingOrder=renderer.name.Contains("shadow")?order-1:order;
                    }
                    world.WorkshopAction.StepForNativeCapture(0f);
                    var cam=world.ViewCamera;var home=Modular25DCameraInput.HomePosition;
                    var portrait=Modular25DCameraInput.HomeSizeForAspect(390f/844f);
                    var report=new Modular25DNativeCapture.Evidence{
                        engine="Unity",engineVersion=UnityEngine.Application.unityVersion,
                        sourceSha=Environment.GetEnvironmentVariable("ISSUE200_ADOPTED_SHA"),mode="Runtime imported owner-selected sawmill only; actual experimental source",
                        nativeWorldRender=true,gatewayBuild=true,savedAndReloaded=true,bastionLevel=1,sawmillLevel=1
                    };
                    Modular25DNativeCapture.Render(cam,folder,report,"home",390,844,home,portrait);
                    Modular25DNativeCapture.Render(cam,folder,report,"registered-entry",390,844,new Vector3(0,1.9f,-30f),6.600513f);
                    Modular25DNativeCapture.Render(cam,folder,report,"retina",1170,2532,home,portrait);
                    Modular25DNativeCapture.Render(cam,folder,report,"wide",844,390,home,Modular25DCameraInput.HomeSizeForAspect(844f/390f));
                    Modular25DNativeCapture.Render(cam,folder,report,"keep-close",1170,1170,(Vector3)ValoriaUrbanPlan.City.Keep.Foot+new Vector3(0,1.65f,-30f),3.3f);
                    Modular25DNativeCapture.Render(cam,folder,report,"mill-close",1170,1170,(Vector3)ValoriaUrbanPlan.City.Mill.Foot+new Vector3(0,.8f,-30f),3.3f);
                    var walkers=UnityEngine.Object.FindObjectsByType<Modular25DWalker>(FindObjectsSortMode.None);
                    var workshop=UnityEngine.Object.FindObjectsByType<Modular25DWorkshopAction>(FindObjectsSortMode.None);
                    if(walkers.Length<2||workshop.Length!=1)throw new Exception("Original moving sprites missing");
                    for(int i=0;i<90;i++){
                        foreach(var w in walkers)w.StepForNativeCapture(1f/15);
                        workshop[0].StepForNativeCapture(1f/15);
                        float t=i/89f;
                        var p=new Vector3(Mathf.Lerp(-1.7f,.45f,t),Mathf.Lerp(.13f,1.75f,t),-30f);
                        Modular25DNativeCapture.Render(cam,folder,report,"motion-"+i.ToString("D3"),390,844,p,portrait-Mathf.Sin(t*Mathf.PI)*1.2f);
                    }
                    report.sourceFrames=report.frames.Count;
                    report.movingCharactersAreUnityTransforms=true;
                    File.WriteAllText(Path.Combine(folder,"evidence.json"),JsonUtility.ToJson(report,true));
                }
            } finally { SliceContentProfiles.SetRuntimeProfileOverride(null); }
        }
    }
}
#endif
