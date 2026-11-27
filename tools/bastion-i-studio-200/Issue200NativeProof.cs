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
    public static class Issue200NativeProof
    {
        sealed class Clock : IClock {
            public long UtcTicks {get;private set;} = new DateTime(2026,11,19,10,0,0,DateTimeKind.Utc).Ticks;
            public void Advance(int s){UtcTicks+=TimeSpan.FromSeconds(s).Ticks;}
        }
        static Sprite Candidate(string folder,string name,Sprite original) {
            var tex=new Texture2D(2,2,TextureFormat.RGBA32,true);
            if(!ImageConversion.LoadImage(tex,File.ReadAllBytes(Path.Combine(folder,name+".png")),false))
                throw new Exception("Failed to decode "+name);
            tex.name="Issue200 "+name;
            tex.filterMode=FilterMode.Trilinear;
            tex.wrapMode=TextureWrapMode.Clamp;
            float ppu=original.pixelsPerUnit;
            var pivot=new Vector2(original.pivot.x/original.rect.width,original.pivot.y/original.rect.height);
            if(tex.width!=(int)original.rect.width||tex.height!=(int)original.rect.height)
                throw new Exception("Authored sprite canvas must retain original world dimensions");
            var sprite=Sprite.Create(tex,new Rect(0,0,tex.width,tex.height),pivot,ppu,0,SpriteMeshType.FullRect);
            sprite.name="Issue200_"+name;
            return sprite;
        }
        static void SetArt(Modular25DWorld world,string folder) {
            var keep=world.BastionRenderer; var mill=world.SawmillRenderer;
            keep.sprite=Candidate(folder,"bastion-fit",keep.sprite);
            mill.sprite=Candidate(folder,"sawmill-fit",mill.sprite);
            foreach(var renderer in new[]{keep,mill}){
                renderer.GetComponent<Modular25DArchitecturalShadow>()?.Refresh();
                var shadow=renderer.GetComponentInChildren<MeshRenderer>();
                if(shadow==null||shadow.sharedMaterial.GetTexture("_ShadowTex")!=renderer.sprite.texture)
                    throw new Exception("Candidate shadow not linked to independent sprite");
            }
            var texture=new Texture2D(2,2,TextureFormat.RGBA32,true);
            if(!ImageConversion.LoadImage(texture,File.ReadAllBytes(Path.Combine(folder,"meadow.png")),false))
                throw new Exception("Meadow decode failed");
            texture.wrapMode=TextureWrapMode.Repeat;texture.filterMode=FilterMode.Trilinear;
            var ground=world.GetComponentInChildren<Modular25DGround>();
            if(ground==null||ground.ChunkCount!=81)throw new Exception("Original continuous terrain absent");
            var mats=new HashSet<Material>();
            foreach(var r in ground.GetComponentsInChildren<MeshRenderer>()) {
                var m=r.sharedMaterial;
                if(m!=null && mats.Add(m) && m.mainTexture!=null) {
                    m.mainTexture=texture;
                    if(m.HasProperty("_BaseMap"))m.SetTexture("_BaseMap",texture);
                }
            }
            if(mats.Count==0)throw new Exception("No ground materials");
        }
        public static void Capture(){
            var root=Environment.GetEnvironmentVariable("ISSUE200_PROOF_OUT");
            var assets=Environment.GetEnvironmentVariable("ISSUE200_PROOF_ASSETS");
            if(String.IsNullOrEmpty(root)||String.IsNullOrEmpty(assets))throw new Exception("Explicit scratch paths required");
            Directory.CreateDirectory(root);
            SliceContentProfiles.SetRuntimeProfileOverride(SliceContentProfiles.OwnerIiiId);
            try {
                foreach(var candidate in new[]{false,true}) {
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
                    if(candidate)SetArt(world,assets);
                    var cam=world.ViewCamera;var home=Modular25DCameraInput.HomePosition;
                    var portrait=Modular25DCameraInput.HomeSizeForAspect(390f/844f);
                    var report=new Modular25DNativeCapture.Evidence{
                        engine="Unity",engineVersion=UnityEngine.Application.unityVersion,
                        sourceSha=Environment.GetEnvironmentVariable("ISSUE200_BASE_SHA"),mode=candidate?"Issue200 modular proof candidate":"Issue200 matched baseline",
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
