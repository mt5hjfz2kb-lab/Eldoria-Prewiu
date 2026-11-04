#if UNITY_EDITOR
using System;
using System.Collections;
using System.IO;
using System.Linq;
using Eldoria.Application;
using Eldoria.Presentation;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using Object = UnityEngine.Object;

namespace Eldoria.ReferenceClean.Editor
{
    /// <summary>
    /// Native, reproducible camera A/B on the exact integrated scene.
    /// Does not serialize/modify any owned scene, use real-player saves, or
    /// bypass game rules. All construction actions go through LocalGateway.
    /// </summary>
    [InitializeOnLoad]
    public static class ConstructionDelightNativeCapture
    {
        const string Flag = "Eldoria.ConstructionDelight.NativeCapture";
        const string Scene = "Assets/Eldoria/ArtTests/ValoriaReferenceClean/Integration/Valoria_City_World_UI.unity";
        static readonly string Output = Path.GetFullPath(Path.Combine(
            UnityEngine.Application.dataPath,"../../artifacts/construction-delight-native"));
        static ConstructionDelightNativeCapture()
        {
            EditorApplication.playModeStateChanged += OnPlayMode;
        }

        public static void Run()
        {
            try
            {
                Environment.SetEnvironmentVariable("VALORIA_GAMEPLAY_PROBE","1");
                // Isolated QA folder only; keep previous run artifacts in GitHub
                // while refusing stale frame mixes after a failed first pass.
                if(Directory.Exists(Output))Directory.Delete(Output,true);
                Directory.CreateDirectory(Output);
                EditorSceneManager.OpenScene(Scene,OpenSceneMode.Single);
                SessionState.SetBool(Flag,true);
                EditorApplication.EnterPlaymode();
            }
            catch(Exception e) { Fail(e); }
        }

        static void OnPlayMode(PlayModeStateChange mode)
        {
            if(mode != PlayModeStateChange.EnteredPlayMode ||
               !SessionState.GetBool(Flag,false)) return;
            SessionState.SetBool(Flag,false);
            EditorApplication.delayCall += () =>
            {
                var host = Object.FindFirstObjectByType<ValoriaIntegratedWorld>();
                if(host == null) { Fail(new InvalidOperationException("Real integrated host missing")); return; }
                host.StartCoroutine(Guard(host));
            };
        }

        static IEnumerator Guard(ValoriaIntegratedWorld host)
        {
            var probe = Probe(host);
            while(true)
            {
                bool moved;
                object yielded=null;
                try { moved=probe.MoveNext(); if(moved)yielded=probe.Current; }
                catch(Exception e) { Fail(e); yield break; }
                if(!moved)yield break;
                yield return yielded;
            }
        }

        static IEnumerator Probe(ValoriaIntegratedWorld host)
        {
            int frames=0;
            while(!host.Ready && frames++<300)yield return null;
            Require(host.Ready,"Integrated scene did not become ready");
            var city=host.City;
            Require(city && city.Ready && city.Probe,"QA must have isolated save and TestClock");
            var fx=city.GetComponent<ValoriaConstructionDelightV1>();
            Require(fx && fx.enabled,"Runtime self-attaching Delight component missing");
            var owned=fx.transform.Find("ConstructionDelightOwnedSprites");
            Require(owned,"Dedicated construction particle root missing");
            var renderers=owned.GetComponentsInChildren<MeshRenderer>(true);
            Require(renderers.Length>=7,"Complete batched mesh visual layer not instantiated");
            Require(renderers.All(x=>x && x.sharedMaterial && x.sharedMaterial.shader &&
                    x.sharedMaterial.shader.isSupported),
                "At least one particle shader missing/unsupported; no visual acceptance");

            yield return new WaitForSecondsRealtime(.4f);
            // Use the authentic player camera, focused on the real building.
            // A far-away whole-kingdom shot cannot reveal cheap-looking FX.
            var genuineTarget=host.FindTarget("sawmill");
            Require(genuineTarget,"Real sawmill hotspot missing");
            // Preserve the default real-player overview before changing zoom.
            Shot(host.PlayerCamera,"city-overview-landscape",844,390);
            Shot(host.PlayerCamera,"city-overview-portrait",390,844);
            // 14 is the actual player camera's minimum allowed zoom.
            // Native close-up QA must show the material quality of the effect.
            host.Zoom(14f-host.PlayerCamera.orthographicSize);
            // First move the REAL city camera to the hotspot; the integrated
            // safe-area Focus() refuses fully offscreen city targets.
            city.Focus(genuineTarget.gameObject);
            host.Focus(genuineTarget.gameObject);
            yield return new WaitForSecondsRealtime(.32f);
            // Baseline: identical real scene, no action.
            Shot(host.PlayerCamera,"city-clean-landscape",844,390);
            Shot(host.PlayerCamera,"city-clean-portrait",390,844);

            var initial=city.Gateway.Snapshot();
            Require(initial.SawmillLevel==0,"Probe save is not fresh");
            var command=new GameCommand("delight-sawmill-test",initial.PlayerId,
                "Build","sawmill",initial.Revision);
            var result=city.Gateway.Execute(command);
            Require(result.Ok,"Canonical sawmill build rejected: "+result.Message);
            Require(city.Gateway.Snapshot().BuildingCompletesUtcTicks>0,
                "Canonical construction timer did not start");
            // A direct QA Gateway command bypasses the game's building button.
            // Explicitly apply its real state to activate the real scaffold.
            city.Presenter.RefreshState();
            host.Hud.Refresh();

            yield return new WaitForSecondsRealtime(.42f);
            Require(fx.ActiveConstructionSprites>0,
                "No real, flowing construction sprites over the scene; task="+
                city.Gateway.Snapshot().BuildingTaskId+
                " visual_work="+fx.ActiveBuildingId);
            Comparison(host,renderers,"work",844,390);
            Comparison(host,renderers,"work",390,844);

            // The ONLY way to complete is through the real gateway and the
            // native isolated TestClock. No PlayerState edits or mock result.
            var clock=city.Clock as ValoriaReferenceCleanGameplay.TestClock;
            Require(clock!=null,"TestClock not installed");
            clock.Step(Eldoria.Domain.SliceRules.SawmillBuildSeconds+1);
            Require(city.Gateway.Advance(),"Real gateway did not complete build");
            city.Presenter.RefreshState();
            host.Hud.Refresh();
            var finishedState=city.Gateway.Snapshot();
            Require(finishedState.SawmillLevel==1,"Real level did not increment");
            Require(finishedState.CompletedTaskIds!=null &&
                    finishedState.CompletedTaskIds.Contains("sawmill:delight-sawmill-test"),
                "Real completion lacks the persisted LocalGateway task receipt");

            yield return new WaitForSecondsRealtime(.36f);
            Require(fx.ActiveFinishSprites>0,
                "Completion gilded sprite flourish missing from actual build");
            Comparison(host,renderers,"finish",844,390);
            Comparison(host,renderers,"finish",390,844);
            // An artistic approval needs temporal evidence, not one lucky frame.
            // Sample the true particle lifetime in BOTH mobile orientations.
            for(int frame=0;frame<6;frame++)
            {
                yield return new WaitForSecondsRealtime(.20f);
                Shot(host.PlayerCamera,"finish-motion-"+frame+"-landscape",844,390);
                Shot(host.PlayerCamera,"finish-motion-"+frame+"-portrait",390,844);
            }
            yield return new WaitForSecondsRealtime(.55f);
            Shot(host.PlayerCamera,"city-finished-landscape",844,390);
            Shot(host.PlayerCamera,"city-finished-portrait",390,844);

            var snapshotBefore=JsonUtility.ToJson(city.Gateway.Snapshot());
            city.ReloadForProbe();
            yield return new WaitForSecondsRealtime(.35f);
            Require(snapshotBefore==JsonUtility.ToJson(city.Gateway.Snapshot()),
                "Graphics changed the authoritative save state after reload");
            Require(fx.ActiveFinishSprites==0,
                "False completion animation triggered on reload");

            File.WriteAllText(Path.Combine(Output,"result.json"),
                "{\n"+
                "  \"status\": \"NATIVE_PASS\",\n"+
                "  \"canvas\": \"true Unity city camera\",\n"+
                "  \"save_unchanged_after_reload\": true,\n"+
                "  \"construction_from\": \"LocalGateway.Execute(Build)\",\n"+
                "  \"completion_from\": \"TestClock + LocalGateway.Advance\",\n"+
                "  \"images\": 26,\n"+
                "  \"art_approved\": false\n"+
                "}\n");
            Debug.Log("CONSTRUCTION_DELIGHT_NATIVE_PASS images=26 save-preserved=true");
            EditorApplication.Exit(0);
        }

        static void Comparison(ValoriaIntegratedWorld host,
            MeshRenderer[] effects, string stage, int width, int height)
        {
            var enabled=effects.Select(x=>x.enabled).ToArray();
            try
            {
                for(int i=0;i<effects.Length;i++)effects[i].enabled=false;
                var before=Shot(host.PlayerCamera,stage+"-A-no-fx-"+width+"x"+height,width,height);
                for(int i=0;i<effects.Length;i++)effects[i].enabled=enabled[i];
                var after=Shot(host.PlayerCamera,stage+"-B-delight-"+width+"x"+height,width,height);
                int changedPixels=0;
                for(int p=0;p<before.Length;p++)
                {
                    int difference=Math.Abs(before[p].r-after[p].r)+
                                   Math.Abs(before[p].g-after[p].g)+
                                   Math.Abs(before[p].b-after[p].b);
                    if(difference>18)changedPixels++;
                }
                // Measure local visual change at the REAL sawmill hotspot,
                // not just an unrelated animated sky/background pixel.
                var site=host.FindTarget("sawmill");
                Require(site,"Canonical sawmill hotspot absent during A/B");
                var collider=site.GetComponent<Collider>();
                Require(collider,"Cannot establish genuine sawmill capture region");
                var camera=host.PlayerCamera;
                float previousAspect=camera.aspect;
                Vector3 screenSite;
                try
                {
                    camera.aspect=width/(float)height;
                    screenSite=camera.WorldToViewportPoint(collider.bounds.center);
                }
                finally { camera.aspect=previousAspect; }
                Require(screenSite.z>0f && screenSite.x>0f && screenSite.x<1f &&
                        screenSite.y>0f && screenSite.y<1f,
                    "Sawmill site not framed inside real camera");
                int x0=Mathf.Clamp(Mathf.FloorToInt((screenSite.x-.24f)*width),0,width-1);
                int x1=Mathf.Clamp(Mathf.CeilToInt((screenSite.x+.24f)*width),0,width);
                int y0=Mathf.Clamp(Mathf.FloorToInt((screenSite.y-.24f)*height),0,height-1);
                int y1=Mathf.Clamp(Mathf.CeilToInt((screenSite.y+.24f)*height),0,height);
                int siteChanged=0;
                for(int y=y0;y<y1;y++)for(int x=x0;x<x1;x++)
                {
                    int index=y*width+x;
                    int difference=Math.Abs(before[index].r-after[index].r)+
                                   Math.Abs(before[index].g-after[index].g)+
                                   Math.Abs(before[index].b-after[index].b);
                    if(difference>18)siteChanged++;
                }
                Debug.Log("CONSTRUCTION_DELIGHT_PIXEL_DELTA stage="+stage+
                          " resolution="+width+"x"+height+
                          " global="+changedPixels+" sawmill_site="+siteChanged);
                Require(changedPixels>=50 && siteChanged>=30,
                    "Effects insufficient at AUTHENTIC sawmill on "+stage+" "+
                    width+"x"+height+": global="+changedPixels+" site="+siteChanged);
            }
            finally
            {
                for(int i=0;i<effects.Length;i++)if(effects[i])effects[i].enabled=enabled[i];
            }
        }

        static Color32[] Shot(Camera camera,string name,int width,int height)
        {
            Require(camera,"Actual player camera missing");
            var priorTarget=camera.targetTexture;
            var priorAspect=camera.aspect;
            var priorActive=RenderTexture.active;
            var rt=new RenderTexture(width,height,24,RenderTextureFormat.ARGB32)
            { name="Construction Delight proof "+name };
            var image=new Texture2D(width,height,TextureFormat.RGB24,false);
            try
            {
                camera.aspect=width/(float)height;
                // Camera.Render can omit URP camera-stack content. Request a real
                // pipeline frame at the exact target resolution instead.
                // StandardRequest is supported by URP base cameras on Unity 6.
                var request=new RenderPipeline.StandardRequest{destination=rt};
                Require(RenderPipeline.SupportsRenderRequest(camera,request),
                    "URP StandardRequest unavailable for the actual player camera");
                RenderPipeline.SubmitRenderRequest(camera,request);
                RenderTexture.active=rt;
                image.ReadPixels(new Rect(0,0,width,height),0,0);
                image.Apply(false,false);
                var pixels=image.GetPixels32();
                File.WriteAllBytes(Path.Combine(Output,name+".png"),image.EncodeToPNG());
                return pixels;
            }
            finally
            {
                camera.targetTexture=priorTarget;
                camera.aspect=priorAspect;
                RenderTexture.active=priorActive;
                rt.Release();
                Object.Destroy(rt);Object.Destroy(image);
            }
        }

        static void Require(bool ok,string reason)
        {
            if(!ok)throw new InvalidOperationException(reason);
        }

        static void Fail(Exception error)
        {
            Debug.LogException(error);
            try
            {
                Directory.CreateDirectory(Output);
                File.WriteAllText(Path.Combine(Output,"failure.txt"),error.ToString());
            }
            catch(Exception extra){Debug.LogError("Failed writing native failure evidence: "+extra);}
            EditorApplication.Exit(1);
        }
    }
}
#endif
