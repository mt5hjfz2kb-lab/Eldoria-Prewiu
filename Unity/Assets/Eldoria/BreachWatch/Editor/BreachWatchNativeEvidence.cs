#if UNITY_EDITOR
using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Security.Cryptography;
using Eldoria.ReferenceClean;
using Eldoria.ReferenceClean.Editor;
using Eldoria.UI.RoyalGold;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.EventSystems;
using Object=UnityEngine.Object;

namespace Eldoria.BreachWatch.Editor
{
    // A genuine Unity Editor/game scene capture, NOT a conceptual mockup.
    // Source: canonical city + same state/session + new independent Canvas.
    // No gameplay modifications, no scene saves, no publication.
    [InitializeOnLoad]
    public static class BreachWatchNativeEvidence
    {
        const string Flag="BreachWatch.Proof.Active";
        const string Scene="Assets/Eldoria/ArtTests/ValoriaReferenceClean/Integration/Valoria_City_World_UI.unity";
        static bool running;
        static double deadline;
        static string Dir => Path.GetFullPath(Path.Combine(Application.dataPath,"../BreachWatchNativeEvidence"));
        [Serializable]
        public sealed class Sample
        {
            public string file,resolution,mode,sha256;
            public int bytes,wood,stone,food,power,bastionLevel;
            public bool independentCanvas,oldCanvasHidden,realBuildingAction,screenChanges;
        }
        [Serializable]
        public sealed class Report
        {
            public string kind="ELDORIA_BREACHWATCH_REAL_UNITY",version="6000.3.23f1",
                state="UNITY_NATIVE_SCREEN_CAPTURED_ART_REVIEW_PENDING",
                sourceCommit="GITHUB_SHA_ENV",
                scene=Scene;
            public int screenshotCount;
            public bool isolatedProbe=true,existingHudUnmodified=true,oldArtReused=false,
                physicalAndroidTested=false,webglTested=false,artApproved=false;
            public List<Sample> shots=new List<Sample>();
        }
        static BreachWatchNativeEvidence(){EditorApplication.update-=Poll;EditorApplication.update+=Poll;}
        [MenuItem("Eldoria/Vigilia de la Brecha/Capturar primera pantalla nativa")]
        public static void CaptureBatch()
        {
            if(EditorApplication.isPlaying)throw new InvalidOperationException("BreachWatch needs a clean Editor start");
            Environment.SetEnvironmentVariable("VALORIA_GAMEPLAY_PROBE","1");
            EditorSceneManager.OpenScene(Scene,OpenSceneMode.Single);
            Directory.CreateDirectory(Dir);
            SessionState.SetBool(Flag,true);
            deadline=EditorApplication.timeSinceStartup+300;
            EditorApplication.EnterPlaymode();
        }
        static void Poll()
        {
            if(!SessionState.GetBool(Flag,false)||running||!EditorApplication.isPlaying)return;
            if(deadline==0)deadline=EditorApplication.timeSinceStartup+300;
            if(EditorApplication.timeSinceStartup>deadline){Fail("Timed out waiting for real Valoria session");return;}
            var host=Object.FindFirstObjectByType<ValoriaIntegratedWorld>();
            if(!host||!host.Ready||!host.Hud||!host.City||!host.City.Probe)return;
            running=true;SessionState.SetBool(Flag,false);
            EditorApplication.delayCall+=()=>Run(host);
        }
        static void Assert(bool valid,string message)
        {
            if(!valid)throw new InvalidOperationException("BREACHWATCH_NATIVE_PROOF_FAIL: "+message);
        }
        static void Fail(string message)
        {
            Directory.CreateDirectory(Dir);
            File.WriteAllText(Path.Combine(Dir,"failure.txt"),message);
            Debug.LogError("BREACHWATCH_NATIVE_PROOF_FAIL "+message);
            EditorApplication.Exit(1);
        }
        static string Sha(byte[] bytes)
        {
            using(var h=SHA256.Create())return BitConverter.ToString(h.ComputeHash(bytes)).Replace("-","").ToLowerInvariant();
        }
        static Color32[] CapturePixels(Camera camera,int width,int height,Action prepare)
        {
            var rt=new RenderTexture(width,height,24,RenderTextureFormat.ARGB32);
            var tex=new Texture2D(width,height,TextureFormat.RGBA32,false);
            var oldTexture=camera.targetTexture;
            var oldAspect=camera.aspect;
            var oldMask=camera.cullingMask;
            var oldActive=RenderTexture.active;
            try
            {
                camera.targetTexture=rt;
                camera.aspect=width/(float)height;
                camera.cullingMask|=(1<<5);
                prepare?.Invoke();
                Canvas.ForceUpdateCanvases();
                camera.Render();Canvas.ForceUpdateCanvases();
                camera.Render();Canvas.ForceUpdateCanvases();
                RenderTexture.active=rt;
                tex.ReadPixels(new Rect(0,0,width,height),0,0,false);
                tex.Apply(false,false);
                return tex.GetPixels32();
            }
            finally
            {
                camera.targetTexture=oldTexture;camera.aspect=oldAspect;camera.cullingMask=oldMask;
                RenderTexture.active=oldActive;
                rt.Release();Object.Destroy(rt);Object.Destroy(tex);
            }
        }
        static void Save(Report report,Color32[] pixels,int width,int height,string state,BreachWatchHud hud,ValoriaIntegratedWorld host)
        {
            var tex=new Texture2D(width,height,TextureFormat.RGBA32,false);
            try
            {
                tex.SetPixels32(pixels);tex.Apply(false,false);
                byte[] bytes=tex.EncodeToPNG();
                Assert(bytes.Length>4096,"PNG is empty or implausibly small "+state);
                string name="native-"+state+"-"+width+"x"+height+".png";
                File.WriteAllBytes(Path.Combine(Dir,name),bytes);
                var snapshot=host.Read(hud?.SelectedBuildingId??"",host.English);
                var sample=new Sample{
                    file=name,resolution=width+"x"+height,mode=state,sha256=Sha(bytes),
                    bytes=bytes.Length,wood=snapshot.Wood,stone=snapshot.Stone,food=snapshot.Food,
                    power=snapshot.Power,bastionLevel=snapshot.BastionLevel,
                    independentCanvas=hud&&hud.IndependentCanvas,
                    oldCanvasHidden=hud&&hud.LegacyPresentationHidden,
                    realBuildingAction= snapshot.Building!=null && snapshot.Building.Action!=null};
                report.shots.Add(sample);
                Debug.Log("BREACHWATCH_REAL_UNITY_PNG "+name+" "+bytes.Length+" "+sample.sha256);
            }
            finally{Object.Destroy(tex);}
        }
        static bool Different(Color32[] a,Color32[] b,int width,int height)
        {
            int changed=0,total=0;
            // Compare border-area pixels containing the UI, not moving city foliage.
            for(int y=height-height/5;y<height-5;y+=3)
            for(int x=5;x<width/3;x+=3)
            {
                int i=y*width+x;total++;
                if(Math.Abs(a[i].r-b[i].r)+Math.Abs(a[i].g-b[i].g)+Math.Abs(a[i].b-b[i].b)>42)changed++;
            }
            return total>0 && changed>Math.Max(40,total/20);
        }
        static void ValidateRealSelection(BreachWatchHud hud,ValoriaIntegratedWorld host)
        {
            Assert(hud&&hud.IndependentCanvas,"The new HUD is not an independent Canvas");
            Assert(hud.LegacyPresentationHidden,"Legacy visual HUD is not completely hidden");
            Assert(hud.NativeCanvas.transform.parent==null,"New HUD is nested inside legacy hierarchy");
            Assert(!hud.GetComponentsInChildren<Eldoria.UI.RoyalGold.RoyalHUD>(true).Any(),"Old HUD component in new presentation");
            Assert(!hud.GetComponentsInChildren<MonoBehaviour>(true).Any(c=>c && c.GetType().Name=="ForjaOscuraLiveHudSkin"),"Legacy Forja skin in new presentation");
            Assert(hud.SelectedBuildingId=="bastion","Requested Bastion selection was not retained");
            var real=host.Read("bastion",host.English);
            Assert(real!=null && real.Building!=null && real.Building.Id=="bastion","Canonical Bastion snapshot missing");
            Assert(hud.CurrentSnapshot!=null && hud.CurrentSnapshot.Building!=null,"New presentation lacks real building binding");
            Assert(hud.CurrentSnapshot.Building.Level==real.Building.Level,"Building level differs from the game");
            Assert(hud.CurrentSnapshot.Wood==real.Wood && hud.CurrentSnapshot.Stone==real.Stone
                && hud.CurrentSnapshot.Power==real.Power,"Wallet or power differs from canonical game");
            Assert(hud.BuildingPanel&&hud.BuildingPanel.gameObject.activeInHierarchy,"New contextual panel not visible");
            Assert(hud.BuildingAction&&hud.BuildingAction.GetComponent<Graphic>().raycastTarget,"New genuine action has no raycast graphic");
            Assert(hud.NavButtons.Count==5 && hud.NavButtons.All(b=>b&&b.GetComponent<Graphic>()&&b.GetComponent<Graphic>().raycastTarget),
                "Exactly five functional new navigation targets are required");
            Assert(hud.NavButtons[2].interactable==false,"Unimplemented Army screen must not appear implemented");
        }
        static void Run(ValoriaIntegratedWorld host)
        {
            try
            {
                ValoriaIntegrationBuild.CompleteProbePlayerName(host);
                Assert(host&&host.Ready&&host.City.Probe,"Only isolated real gameplay probe allowed");
                if(host.Hud.Modal)host.Hud.Modal.Hide(); // visual proof starts after fixture naming, no save changes
                var stateBefore=host.City.Gateway.Snapshot();
                var report=new Report{sourceCommit=Environment.GetEnvironmentVariable("GITHUB_SHA")??"local-editor"};
                var camera=host.PlayerCamera;
                Assert(camera!=null,"Canonical world camera missing");
                var originalCanvas=host.Hud.GetComponent<Canvas>();
                var originalGroup=host.Hud.GetComponent<CanvasGroup>();
                var originalRaycaster=host.Hud.GetComponent<GraphicRaycaster>();
                var originalMode=originalCanvas.renderMode;
                var originalCam=originalCanvas.worldCamera;
                var originalDistance=originalCanvas.planeDistance;
                Assert(originalCanvas&&originalGroup,"Canonical baseline UI missing");
                // First native shot of the previous visual presentation, not a fake picture.
                // Screenshots of the new system are from an ENTIRELY SEPARATE Canvas.
                var sizes=new[]{new Vector2Int(1600,900),new Vector2Int(900,1600),
                    new Vector2Int(844,390),new Vector2Int(390,844)};
                foreach(var size in sizes)
                {
                    host.Hud.SelectBuilding("bastion");
                    var baseline=CapturePixels(camera,size.x,size.y,()=>{
                        originalCanvas.enabled=true;
                        originalCanvas.renderMode=RenderMode.ScreenSpaceCamera;
                        originalCanvas.worldCamera=camera;
                        originalCanvas.planeDistance=1f;
                        originalGroup.alpha=1;
                        if(originalRaycaster)originalRaycaster.enabled=true;
                    });
                    Save(report,baseline,size.x,size.y,"legacy-bastion",null,host);
                }
                originalCanvas.renderMode=originalMode;originalCanvas.worldCamera=originalCam;
                originalCanvas.planeDistance=originalDistance;
                // Build a truly independent object, then switch OFF the old canvas.
                var hud=BreachWatchHud.Install(host);
                Assert(hud.IndependentCanvas&&hud.LegacyPresentationHidden,"Old visual controller was not replaced in preview");
                foreach(var size in sizes)
                {
                    hud.SelectBuilding("");
                    var city=CapturePixels(camera,size.x,size.y,()=>{
                        hud.Reflow(size.x,size.y);hud.RenderNow();
                    });
                    Save(report,city,size.x,size.y,"vigilia-city",hud,host);
                    hud.SelectBuilding("bastion");
                    var selected=CapturePixels(camera,size.x,size.y,()=>{
                        hud.Reflow(size.x,size.y);hud.RenderNow();ValidateRealSelection(hud,host);
                    });
                    Assert(selected.Length==city.Length,"Nonmatching camera buffer dimensions");
                    Save(report,selected,size.x,size.y,"vigilia-bastion",hud,host);
                    report.shots[report.shots.Count-1].screenChanges=Different(city,selected,size.x,size.y);
                }
                var stateAfter=host.City.Gateway.Snapshot();
                Assert(stateBefore.PlayerId==stateAfter.PlayerId &&
                    stateBefore.Resources.Wood==stateAfter.Resources.Wood &&
                    stateBefore.Resources.Stone==stateAfter.Resources.Stone &&
                    stateBefore.Resources.Food==stateAfter.Resources.Food &&
                    stateBefore.Revision==stateAfter.Revision,
                    "Preview changed the live gameplay economy or saved state");
                Assert(report.shots.Count==12,"Expected 12 genuine Unity PNGs");
                Assert(report.shots.Where(s=>s.mode=="vigilia-bastion").All(s=>s.independentCanvas&&s.oldCanvasHidden),
                    "Proof uses legacy layers");
                File.WriteAllText(Path.Combine(Dir,"BreachWatchNativeEvidence.json"),
                    JsonUtility.ToJson(report,true));
                Debug.Log("BREACHWATCH_NATIVE_12_REAL_CAPTURES_PASS_ART_PENDING source="+report.sourceCommit);
                EditorApplication.Exit(0);
            }
            catch(Exception e){Debug.LogException(e);Fail(e.ToString());}
        }
    }
}
#endif