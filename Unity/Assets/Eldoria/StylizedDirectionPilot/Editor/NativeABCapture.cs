#if UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using Eldoria.ReferenceClean;
using Eldoria.ReferenceClean.Editor;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using Object = UnityEngine.Object;

namespace Eldoria.StylizedDirectionPilot.Editor
{
    /// <summary>
    /// Isolated, zero-credit A/B on the genuine interactive Valoria scene.
    /// EDITOR ONLY: no scene, material, camera, player save or beta is serialized.
    /// Do not mistake a PNG/correct compilation for a native mobile art approval.
    /// </summary>
    [InitializeOnLoad]
    public static class NativeABCapture
    {
        const string Flag = "Eldoria.StylizedDirectionPilot.Capture";
        const string Scene = "Assets/Eldoria/ArtTests/ValoriaReferenceClean/Integration/Valoria_City_World_UI.unity";
        const string ShaderName = "Eldoria/StylizedDirectionPilot/Surface";
        static readonly List<string> Notes = new List<string>();

        static NativeABCapture()
        {
            EditorApplication.playModeStateChanged -= OnPlayMode;
            EditorApplication.playModeStateChanged += OnPlayMode;
        }

        public static void Capture()
        {
            try
            {
                // Use precisely the existing native proof save sandbox.
                Environment.SetEnvironmentVariable("VALORIA_GAMEPLAY_PROBE","1");
                if (!File.Exists(Scene)) throw new FileNotFoundException("Canonical integrated playable city scene",Scene);
                EditorSceneManager.OpenScene(Scene,OpenSceneMode.Single);
                SessionState.SetBool(Flag,true);
                EditorApplication.EnterPlaymode();
            }
            catch(Exception ex) { Fail(ex); }
        }

        static void OnPlayMode(PlayModeStateChange change)
        {
            if(change != PlayModeStateChange.EnteredPlayMode || !SessionState.GetBool(Flag,false)) return;
            SessionState.SetBool(Flag,false);
            EditorApplication.delayCall += () =>
            {
                var host = Object.FindFirstObjectByType<ValoriaIntegratedWorld>();
                if(!host) { Fail(new Exception("Integrated Valoria runtime host is absent")); return; }
                host.StartCoroutine(Guard(host));
            };
        }

        static IEnumerator Guard(ValoriaIntegratedWorld host)
        {
            var source = Perform(host);
            while(true)
            {
                bool hasNext;
                object step = null;
                try { hasNext=source.MoveNext(); if(hasNext)step=source.Current; }
                catch(Exception ex) { Fail(ex); yield break; }
                if(!hasNext) yield break;
                yield return step;
            }
        }

        static IEnumerator Perform(ValoriaIntegratedWorld host)
        {
            var output = Path.GetFullPath(Path.Combine(Application.dataPath,"../StylizedDirectionPilotCaptures"));
            Directory.CreateDirectory(output);
            float deadline=Time.realtimeSinceStartup+120f;
            while(!host.Ready && Time.realtimeSinceStartup<deadline) yield return null;
            if(!host.Ready || !host.City || !host.City.Probe)
                throw new InvalidOperationException("Canonical city did not enter the disposable native probe save");
            if(host.City.Gateway.Snapshot().PlayerName == "")
                ValoriaIntegrationBuild.CompleteProbePlayerName(host);
            yield return null;
            if(!host.PlayerCamera || !host.Hud) throw new InvalidOperationException("Player camera/HUD unavailable");

            var beforeState = JsonUtility.ToJson(host.City.Gateway.Snapshot());
            var baselineWide=Path.Combine(output,"A-genuine-city-1600x900.png");
            var baselinePhone=Path.Combine(output,"A-genuine-city-390x844.png");
            ValoriaIntegrationBuild.Capture(host,baselineWide,1600,900);
            ValoriaIntegrationBuild.Capture(host,baselinePhone,390,844);

            string afterState;
            int changedRenderers,changedMaterials;
            using (var style=new MaterialStyleScope(host.gameObject.scene))
            {
                changedRenderers=style.RendererCount;
                changedMaterials=style.MaterialCount;
                if(changedRenderers < 10 || changedMaterials < 5)
                    throw new InvalidOperationException("Candidate cannot be judged: insufficient real materials");
                var candidateWide=Path.Combine(output,"B-stylized-city-1600x900.png");
                var candidatePhone=Path.Combine(output,"B-stylized-city-390x844.png");
                ValoriaIntegrationBuild.Capture(host,candidateWide,1600,900);
                ValoriaIntegrationBuild.Capture(host,candidatePhone,390,844);
            }
            afterState=JsonUtility.ToJson(host.City.Gateway.Snapshot());
            if(beforeState!=afterState)
                throw new InvalidOperationException("Visual-only A/B changed the actual probe game state");
            var wide=Difference(baselineWide,Path.Combine(output,"B-stylized-city-1600x900.png"));
            var phone=Difference(baselinePhone,Path.Combine(output,"B-stylized-city-390x844.png"));
            if(wide.meanPixelDelta < 1.0 || phone.meanPixelDelta < 1.0)
                throw new InvalidOperationException("Visual-only candidate does not differ from baseline; art pass ineffectual");
            var evidence=new Evidence
            {
                sourceSha=Environment.GetEnvironmentVariable("GITHUB_SHA")??"local-unpinned",
                scene=Scene,shader=ShaderName,saveSandbox="VALORIA_GAMEPLAY_PROBE",
                modifiedRendererCount=changedRenderers,modifiedMaterialCount=changedMaterials,
                wide=wide,phone=phone,artApproved=false,mobileTested=false,
                kind="REAL_UNITY_EDITOR_CAMERA_AB_UNAPPROVED_VISUAL_PILOT"
            };
            File.WriteAllText(Path.Combine(output,"evidence.json"),JsonUtility.ToJson(evidence,true));
            Notes.Add("STYLIZED_DIRECTION_PILOT_COMPLETE baseline+candidates=4 png; gameplay state unchanged");
            foreach(var note in Notes) Debug.Log(note);
            EditorApplication.Exit(0);
        }

        [Serializable]
        sealed class DifferenceMetric
        {
            public string baselineSha256;
            public string candidateSha256;
            public double meanPixelDelta;
            public float changedFraction;
        }
        [Serializable]
        sealed class Evidence
        {
            public string sourceSha,scene,shader,saveSandbox,kind;
            public int modifiedRendererCount,modifiedMaterialCount;
            public DifferenceMetric wide,phone;
            public bool artApproved,mobileTested;
        }

        static DifferenceMetric Difference(string a,string b)
        {
            if(!File.Exists(a)||!File.Exists(b))throw new IOException("Missing exact A/B capture");
            using(var sha=SHA256.Create())
            {
                string hA=BitConverter.ToString(sha.ComputeHash(File.ReadAllBytes(a))).Replace("-","");
                string hB=BitConverter.ToString(sha.ComputeHash(File.ReadAllBytes(b))).Replace("-","");
                var ta=new Texture2D(2,2,TextureFormat.RGB24,false);
                var tb=new Texture2D(2,2,TextureFormat.RGB24,false);
                try
                {
                    if(!ta.LoadImage(File.ReadAllBytes(a))||!tb.LoadImage(File.ReadAllBytes(b))||
                       ta.width!=tb.width||ta.height!=tb.height)
                        throw new InvalidOperationException("Native A/B dimensions differ or PNG decode failed");
                    var ca=ta.GetPixels32();
                    var cb=tb.GetPixels32();
                    double delta=0;int changed=0,sampled=0;
                    for(int i=0;i<ca.Length;i+=17)
                    {
                        int d=Math.Abs(ca[i].r-cb[i].r)+Math.Abs(ca[i].g-cb[i].g)+Math.Abs(ca[i].b-cb[i].b);
                        delta+=d/3.0;
                        if(d>18)changed++;
                        sampled++;
                    }
                    return new DifferenceMetric
                    {
                        baselineSha256=hA,candidateSha256=hB,
                        meanPixelDelta=delta/Math.Max(1,sampled),
                        changedFraction=(float)changed/Math.Max(1,sampled)
                    };
                }
                finally { Object.Destroy(ta); Object.Destroy(tb); }
            }
        }
        static void Fail(Exception ex)
        {
            Debug.LogError("STYLIZED_DIRECTION_PILOT_FAIL "+ex);
            EditorApplication.Exit(1);
        }

        sealed class MaterialStyleScope : IDisposable
        {
            readonly Dictionary<Renderer,Material[]> originals=new Dictionary<Renderer,Material[]>();
            readonly Dictionary<Material,Material> clones=new Dictionary<Material,Material>();
            readonly Color savedAmbient=RenderSettings.ambientLight;
            readonly AmbientMode savedAmbientMode=RenderSettings.ambientMode;
            readonly Color savedFog=RenderSettings.fogColor;
            readonly bool savedFogEnabled=RenderSettings.fog;
            readonly List<LightState> lights=new List<LightState>();
            public int RendererCount=>originals.Count;
            public int MaterialCount=>clones.Count;
            struct LightState {public Light light;public Color color;public float intensity;public float shadow;}
            public MaterialStyleScope(UnityEngine.SceneManagement.Scene scene)
            {
                var shader=Shader.Find(ShaderName);
                if(!shader||!shader.isSupported)
                    throw new InvalidOperationException("Stylized URP shader unavailable");
                RenderSettings.ambientMode=AmbientMode.Flat;
                RenderSettings.ambientLight=new Color(.65f,.72f,.72f,1);
                RenderSettings.fogColor=new Color(.44f,.49f,.50f,1);
                // Do not enable/change fog globally: one scene may not use it.
                foreach(var sun in Object.FindObjectsByType<Light>(FindObjectsSortMode.None))
                {
                    if(sun.gameObject.scene!=scene||sun.type!=LightType.Directional)continue;
                    lights.Add(new LightState {light=sun,color=sun.color,intensity=sun.intensity,shadow=sun.shadowStrength});
                    sun.color=new Color(.97f,.89f,.77f);
                    sun.intensity*=.87f;
                    sun.shadowStrength=Mathf.Max(sun.shadowStrength,.64f);
                }
                foreach(var r in Object.FindObjectsByType<MeshRenderer>(FindObjectsSortMode.None))
                {
                    if(!r || r.gameObject.scene!=scene || !r.gameObject.activeInHierarchy || !r.enabled)continue;
                    var original=r.sharedMaterials;
                    var next=(Material[])original.Clone();
                    bool changed=false;
                    for(int i=0;i<next.Length;i++)
                    {
                        var m=original[i];
                        if(!m || m.renderQueue>=3000 || (m.shader&&
                            (m.shader.name.Contains("Sprites/")||m.shader.name.Contains("Particle"))))continue;
                        if(!clones.TryGetValue(m,out var replacement))
                        {
                            replacement=new Material(shader) {name="PILOT ONLY / "+m.name};
                            var tex = m.HasProperty("_BaseMap") ? m.GetTexture("_BaseMap") :
                                      m.HasProperty("baseColorTexture")?m.GetTexture("baseColorTexture") :
                                      m.HasProperty("_MainTex")?m.GetTexture("_MainTex"):null;
                            if(tex)replacement.SetTexture("_BaseMap",tex);
                            if(m.HasProperty("_BaseMap"))replacement.SetTextureScale("_BaseMap",m.GetTextureScale("_BaseMap"));
                            Color c=m.HasProperty("_BaseColor")?m.GetColor("_BaseColor") :
                              m.HasProperty("baseColorFactor")?m.GetColor("baseColorFactor") :
                              m.HasProperty("_Color")?m.GetColor("_Color"):Color.white;
                            replacement.SetColor("_BaseColor",c);
                            replacement.SetFloat("_StyleStrength",.85f);
                            replacement.SetFloat("_Cutoff",.25f);
                            clones.Add(m,replacement);
                        }
                        next[i]=replacement;changed=true;
                    }
                    if(changed){originals.Add(r,original);r.sharedMaterials=next;}
                }
            }
            public void Dispose()
            {
                foreach(var pair in originals)if(pair.Key)pair.Key.sharedMaterials=pair.Value;
                foreach(var mat in clones.Values)if(mat)Object.Destroy(mat);
                foreach(var light in lights)if(light.light)
                {
                    light.light.color=light.color;
                    light.light.intensity=light.intensity;
                    light.light.shadowStrength=light.shadow;
                }
                RenderSettings.ambientMode=savedAmbientMode;
                RenderSettings.ambientLight=savedAmbient;
                RenderSettings.fog=savedFogEnabled;
                RenderSettings.fogColor=savedFog;
            }
        }
    }
}
#endif
