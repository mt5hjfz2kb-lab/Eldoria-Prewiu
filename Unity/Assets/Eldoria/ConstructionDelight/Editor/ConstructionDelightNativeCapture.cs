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
            Application.dataPath,"../../artifacts/construction-delight-native"));
        static ConstructionDelightNativeCapture()
        {
            EditorApplication.playModeStateChanged += OnPlayMode;
        }

        public static void Run()
        {
            try
            {
                Environment.SetEnvironmentVariable("VALORIA_GAMEPLAY_PROBE","1");
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
            var systems=fx.GetComponentsInChildren<ParticleSystem>();
            var renderers=fx.GetComponentsInChildren<ParticleSystemRenderer>();
            Require(systems.Length>=7&&renderers.Length>=7,"Complete visual layer not instantiated");
            Require(renderers.All(x=>x && x.sharedMaterial && x.sharedMaterial.shader &&
                    x.sharedMaterial.shader.isSupported),
                "At least one particle shader missing/unsupported; no visual acceptance");

            yield return new WaitForSecondsRealtime(.4f);
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

            yield return new WaitForSecondsRealtime(.42f);
            Require(systems.Any(p=>p.isPlaying && p.particleCount>0),
                "No real, flowing construction particles over the scene");
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
            Require(city.Gateway.Snapshot().SawmillLevel==1,"Real level did not increment");

            yield return new WaitForSecondsRealtime(.36f);
            Require(systems.Any(p=>p.name.Contains("Completion") && p.particleCount>0),
                "Completion particle burst missing from actual build");
            Comparison(host,renderers,"finish",844,390);
            Comparison(host,renderers,"finish",390,844);
            yield return new WaitForSecondsRealtime(2f);
            Shot(host.PlayerCamera,"city-finished-landscape",844,390);
            Shot(host.PlayerCamera,"city-finished-portrait",390,844);

            var snapshotBefore=JsonUtility.ToJson(city.Gateway.Snapshot());
            city.ReloadForProbe();
            yield return new WaitForSecondsRealtime(.35f);
            Require(snapshotBefore==JsonUtility.ToJson(city.Gateway.Snapshot()),
                "Graphics changed the authoritative save state after reload");
            Require(!systems.Any(p=>p.name.Contains("Completion") && p.particleCount>0),
                "False completion animation triggered on reload");

            File.WriteAllText(Path.Combine(Output,"result.json"),
                "{\n"+
                "  \"status\": \"NATIVE_PASS\",\n"+
                "  \"canvas\": \"true Unity city camera\",\n"+
                "  \"save_unchanged_after_reload\": true,\n"+
                "  \"construction_from\": \"LocalGateway.Execute(Build)\",\n"+
                "  \"completion_from\": \"TestClock + LocalGateway.Advance\",\n"+
                "  \"images\": 12,\n"+
                "  \"art_approved\": false\n"+
                "}\n");
            Debug.Log("CONSTRUCTION_DELIGHT_NATIVE_PASS images=12 save-preserved=true");
            EditorApplication.Exit(0);
        }

        static void Comparison(ValoriaIntegratedWorld host,
            ParticleSystemRenderer[] effects, string stage, int width, int height)
        {
            var enabled=effects.Select(x=>x.enabled).ToArray();
            try
            {
                for(int i=0;i<effects.Length;i++)effects[i].enabled=false;
                Shot(host.PlayerCamera,stage+"-A-no-fx-"+width+"x"+height,width,height);
                for(int i=0;i<effects.Length;i++)effects[i].enabled=enabled[i];
                Shot(host.PlayerCamera,stage+"-B-delight-"+width+"x"+height,width,height);
            }
            finally
            {
                for(int i=0;i<effects.Length;i++)if(effects[i])effects[i].enabled=enabled[i];
            }
        }

        static void Shot(Camera camera,string name,int width,int height)
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
                camera.targetTexture=rt;
                camera.Render();camera.Render();
                RenderTexture.active=rt;
                image.ReadPixels(new Rect(0,0,width,height),0,0);
                image.Apply(false,false);
                File.WriteAllBytes(Path.Combine(Output,name+".png"),image.EncodeToPNG());
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
