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
        const string ShaderName = "Existing URP materials + editor-baked painterly texture palette";
        static readonly List<string> Notes = new List<string>();

        static NativeABCapture()
        {
            EditorApplication.playModeStateChanged -= OnPlayMode;
            EditorApplication.playModeStateChanged += OnPlayMode;
        }

        static void Mark(string stage)
        {
            var dir=Path.GetFullPath(Path.Combine(UnityEngine.Application.dataPath,"../StylizedDirectionPilotCaptures"));
            Directory.CreateDirectory(dir);
            File.AppendAllText(Path.Combine(dir,"stages.txt"),DateTime.UtcNow.ToString("o")+" "+stage+"\n");
            Debug.Log("ELDORIA_STYLIZED_PROBE_STAGE "+stage);
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void SkipUnrelatedWorldForEditorCityEvidence()
        {
            if(!SessionState.GetBool(Flag,false))return;
            ValoriaIntegratedWorld.P0LazyWorldEditorTestOverride=true;
            Mark("ISOLATED_LAZY_WORLD_SET_BEFORE_SCENE");
        }

        // Deterministic EDIT-MODE proof. Unlike the player-runtime path below,
        // it does not load the canonical World4X or require PlayMode/coroutines.
        // It evaluates the already-serialized current integrated scene models.
        public static void CaptureEditorOnly()
        {
            try
            {
                Mark("EDITOR_ONLY_CAPTURE_START");
                EditorSceneManager.OpenScene(Scene,OpenSceneMode.Single);
                var city=Object.FindFirstObjectByType<ValoriaReferenceCleanGameplay>(FindObjectsInactive.Include);
                if(!city || !city.PlayerCamera)throw new InvalidOperationException("Actual integrated source city camera missing");
                city.Apply(new Eldoria.Domain.PlayerState
                {
                    BastionLevel=2,SawmillLevel=1,BarracksLevel=1,CorruptionDiscovered=true
                });
                var camera=city.PlayerCamera;
                var outDir=Path.GetFullPath(Path.Combine(UnityEngine.Application.dataPath,"../StylizedDirectionPilotCaptures"));
                Directory.CreateDirectory(outDir);
                Mark("EDITOR_REAL_SCENE_LOADED");
                string wideA=Path.Combine(outDir,"A-genuine-city-1600x900.png");
                string phoneA=Path.Combine(outDir,"A-genuine-city-390x844.png");
                string wideB=Path.Combine(outDir,"B-stylized-city-1600x900.png");
                string phoneB=Path.Combine(outDir,"B-stylized-city-390x844.png");
                SaveEditorCamera(camera,wideA,1600,900);
                SaveEditorCamera(camera,phoneA,390,844);
                Mark("EDITOR_BASELINE_FILES_WRITTEN");
                int meshes,mats;
                using(var style=new MaterialStyleScope(city.gameObject.scene))
                {
                    meshes=style.RendererCount;
                    mats=style.MaterialCount;
                    if(meshes<10||mats<5)throw new InvalidOperationException("No substantial source mesh palette applied");
                    SaveEditorCamera(camera,wideB,1600,900);
                    SaveEditorCamera(camera,phoneB,390,844);
                }
                Mark("EDITOR_STYLIZED_FILES_WRITTEN");
                // A second candidate must remain visible: clone each original shader,
                // retaining GLTF/PBR variants, alpha clip, textures and lightmaps.
                string wideC=Path.Combine(outDir,"C-cohesive-original-shaders-1600x900.png");
                string phoneC=Path.Combine(outDir,"C-cohesive-original-shaders-390x844.png");
                int safeRenderers=0,safeMaterials=0;
                using(var style=new OriginalShaderPaletteScope(city.gameObject.scene))
                {
                    safeRenderers=style.RendererCount;
                    safeMaterials=style.MaterialCount;
                    if(safeRenderers<10||safeMaterials<5)
                        throw new InvalidOperationException("Safe palette touched too few genuine materials");
                    SaveEditorCamera(camera,wideC,1600,900);
                    SaveEditorCamera(camera,phoneC,390,844);
                    File.WriteAllLines(Path.Combine(outDir,"safe-original-shaders-material-audit.txt"),style.Audit);
                }
                Mark("EDITOR_SAFE_MATERIAL_PALETTE_FILES_WRITTEN");
                var safeWide=Difference(wideA,wideC);
                var safePhone=Difference(phoneA,phoneC);
                if(safeWide.meanPixelDelta<1 || safePhone.meanPixelDelta<1)
                    throw new InvalidOperationException("Safe palette is not visually distinct");
                var wide=Difference(wideA,wideB);
                var phone=Difference(phoneA,phoneB);
                float retained=CentralBuildingEdgeRatio(wideA,wideB);
                if(retained<.55f)
                    throw new InvalidOperationException("Rejected: candidate hides actual buildings; retained edge ratio="+retained.ToString("F3"));
                Mark("EDITOR_BUILDING_DETAIL_RETENTION="+retained.ToString("F3"));
                if(wide.meanPixelDelta<1.0||phone.meanPixelDelta<1.0)
                    throw new InvalidOperationException("Style treatment invisible on actual Unity camera");
                var evidence=new Evidence
                {
                    sourceSha=Environment.GetEnvironmentVariable("GITHUB_SHA")??"local-unpinned",
                    scene=Scene,shader=ShaderName,saveSandbox="NO_GAME_RUNNING_NO_SAVE",
                    modifiedRendererCount=meshes,modifiedMaterialCount=mats,
                    wide=wide,phone=phone,artApproved=false,mobileTested=false,
                    kind="ACTUAL_UNITY_EDITOR_SCENE_ART_AB_NOT_PLAYMODE_OR_DEVICE"
                };
                File.WriteAllText(Path.Combine(outDir,"safe-original-shaders-evidence.json"),
                    "{\\\"candidate\\\":\\\"C_original_shader_palette\\\",\\\"real_unity_editor\\\":true,\\\"not_playmode\\\":true,\\\"artApproved\\\":false,\\\"renderers\\\":"+
                    safeRenderers+",\\\"materials\\\":"+safeMaterials+
                    ",\\\"wideMeanDelta\\\":"+safeWide.meanPixelDelta.ToString(System.Globalization.CultureInfo.InvariantCulture)+
                    ",\\\"phoneMeanDelta\\\":"+safePhone.meanPixelDelta.ToString(System.Globalization.CultureInfo.InvariantCulture)+"}");
                File.WriteAllText(Path.Combine(outDir,"evidence.json"),JsonUtility.ToJson(evidence,true));
                Mark("EDITOR_ART_AB_PASS_PENDING_HUMAN_QUALITY");
                EditorApplication.Exit(0);
            }
            catch(Exception ex){Fail(ex);}
        }

        static void SaveEditorCamera(Camera camera,string path,int width,int height)
        {
            var rt=new RenderTexture(width,height,24,RenderTextureFormat.ARGB32);
            var tex=new Texture2D(width,height,TextureFormat.RGB24,false);
            var saved=RenderTexture.active;
            var target=camera.targetTexture;
            bool enabled=camera.enabled;
            float aspect=camera.aspect,size=camera.orthographicSize;
            try
            {
                camera.enabled=true;
                camera.aspect=width/(float)height;
                // Adjust framing, not source geometry. Compare A/B with identical values.
                if(height>width)camera.orthographicSize=size*1.18f;
                camera.targetTexture=rt;
                camera.Render();
                RenderTexture.active=rt;
                tex.ReadPixels(new Rect(0,0,width,height),0,0);
                tex.Apply(false,false);
                File.WriteAllBytes(path,tex.EncodeToPNG());
                if(new FileInfo(path).Length<30000)
                    throw new InvalidOperationException("Editor render returned blank camera: "+path);
            }
            finally
            {
                camera.targetTexture=target;
                camera.aspect=aspect;
                camera.orthographicSize=size;
                camera.enabled=enabled;
                RenderTexture.active=saved;
                rt.Release();
                Object.DestroyImmediate(rt);
                Object.DestroyImmediate(tex);
            }
        }

        public static void Capture()
        {
            try
            {
                // Use precisely the existing native proof save sandbox.
                Environment.SetEnvironmentVariable("VALORIA_GAMEPLAY_PROBE","1");
                Mark("NATIVE_METHOD_ENTERED");
                ValoriaIntegratedWorld.P0LazyWorldEditorTestOverride=true;
                if (!AssetDatabase.LoadAssetAtPath<SceneAsset>(Scene)) throw new FileNotFoundException("Canonical integrated playable city scene",Scene);
                EditorSceneManager.OpenScene(Scene,OpenSceneMode.Single);
                Mark("REAL_CITY_SCENE_OPENED");
                SessionState.SetBool(Flag,true);
                Mark("ENTER_PLAYMODE_REQUESTED");
                EditorApplication.EnterPlaymode();
            }
            catch(Exception ex) { Fail(ex); }
        }

        static void OnPlayMode(PlayModeStateChange change)
        {
            if(change != PlayModeStateChange.EnteredPlayMode || !SessionState.GetBool(Flag,false)) return;
            Mark("PLAYMODE_CALLBACK_RECEIVED");
            SessionState.SetBool(Flag,false);
            ValoriaIntegratedWorld.P0LazyWorldEditorTestOverride=true;
            EditorApplication.delayCall += () =>
            {
                Mark("PLAYMODE_DELAYCALL_ENTERED");
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
            var output = Path.GetFullPath(Path.Combine(UnityEngine.Application.dataPath,"../StylizedDirectionPilotCaptures"));
            Directory.CreateDirectory(output);
            Mark("COROUTINE_RUNNING");
            float deadline=Time.realtimeSinceStartup+120f;
            while(!host.Ready && Time.realtimeSinceStartup<deadline) yield return null;
            Mark("READY_CHECK_COMPLETE="+host.Ready);
            if(!host.Ready || !host.City || !host.City.Probe)
                throw new InvalidOperationException("Canonical city did not enter the disposable native probe save");
            if(host.City.Gateway.Snapshot().PlayerName == "")
                ValoriaIntegrationBuild.CompleteProbePlayerName(host);
            yield return null;
            if(!host.PlayerCamera || !host.Hud) throw new InvalidOperationException("Player camera/HUD unavailable");

            var beforeState = JsonUtility.ToJson(host.City.Gateway.Snapshot());
            var baselineWide=Path.Combine(output,"A-genuine-city-1600x900.png");
            var baselinePhone=Path.Combine(output,"A-genuine-city-390x844.png");
            Mark("CAPTURE_BASELINE_WIDE_START");
            ValoriaIntegrationBuild.Capture(host,baselineWide,1600,900);
            Mark("CAPTURE_BASELINE_PHONE_START");
            ValoriaIntegrationBuild.Capture(host,baselinePhone,390,844);
            Mark("CAPTURE_BASELINE_COMPLETE");

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
                Mark("CAPTURE_STYLIZED_WIDE_START");
                ValoriaIntegrationBuild.Capture(host,candidateWide,1600,900);
                Mark("CAPTURE_STYLIZED_PHONE_START");
                ValoriaIntegrationBuild.Capture(host,candidatePhone,390,844);
                Mark("CAPTURE_STYLIZED_COMPLETE");
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
            Mark("REAL_UNITY_AB_FILES_WRITTEN");
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

        static float CentralBuildingEdgeRatio(string original,string candidate)
        {
            var a=new Texture2D(2,2,TextureFormat.RGB24,false);
            var b=new Texture2D(2,2,TextureFormat.RGB24,false);
            try
            {
                if(!a.LoadImage(File.ReadAllBytes(original)) || !b.LoadImage(File.ReadAllBytes(candidate)) ||
                   a.width!=b.width || a.height!=b.height)throw new InvalidOperationException("Geometry gate images invalid");
                var p=a.GetPixels32();var q=b.GetPixels32();int w=a.width,h=a.height;
                int originalEdges=0,styledEdges=0;
                for(int y=(int)(h*.10f);y<(int)(h*.43f);y+=2)
                for(int x=(int)(w*.40f);x<(int)(w*.68f)-3;x+=2)
                {
                    int i=y*w+x,j=i+3;
                    float e=Mathf.Abs(.21f*(p[i].r-p[j].r)+.72f*(p[i].g-p[j].g)+.07f*(p[i].b-p[j].b));
                    float f=Mathf.Abs(.21f*(q[i].r-q[j].r)+.72f*(q[i].g-q[j].g)+.07f*(q[i].b-q[j].b));
                    if(e>19f)originalEdges++;
                    if(f>19f)styledEdges++;
                }
                if(originalEdges<250)throw new InvalidOperationException("Real Bastion ROI has no edge evidence");
                return styledEdges/(float)originalEdges;
            }
            finally {Object.DestroyImmediate(a);Object.DestroyImmediate(b);}
        }

        // Art-process change: reuse original URP shaders and real texture maps
        // rather than replacing mesh materials with an unvalidated forward pass.
        // CPU palette textures are editor-only scratch, never imported/persisted.

        // Artwork-preserving alternative: no shader replacement, no cutout or
        // roof/stone geometry loss; safe enough to evaluate a production palette.
        sealed class OriginalShaderPaletteScope : IDisposable
        {
            readonly Dictionary<Renderer,Material[]> old=new Dictionary<Renderer,Material[]>();
            readonly List<Material> copies=new List<Material>();
            readonly Dictionary<Material,Material> palette=new Dictionary<Material,Material>();
            readonly List<string> audit=new List<string>();
            readonly AmbientMode oldAmbient=RenderSettings.ambientMode;
            readonly Color oldFill=RenderSettings.ambientLight;
            readonly List<Sun> suns=new List<Sun>();
            struct Sun { public Light light; public Color color; public float intensity; public float shadow; }
            public int RendererCount=>old.Count;
            public int MaterialCount=>palette.Count;
            public IEnumerable<string> Audit=>audit;
            public OriginalShaderPaletteScope(UnityEngine.SceneManagement.Scene scene)
            {
                RenderSettings.ambientMode=AmbientMode.Flat;
                RenderSettings.ambientLight=new Color(.64f,.69f,.70f);
                foreach(var l in Object.FindObjectsByType<Light>(FindObjectsSortMode.None))
                {
                    if(!l||l.gameObject.scene!=scene||l.type!=LightType.Directional)continue;
                    suns.Add(new Sun{light=l,color=l.color,intensity=l.intensity,shadow=l.shadowStrength});
                    l.color=new Color(1f,.90f,.78f);
                    l.intensity*=.91f;
                    l.shadowStrength=Mathf.Max(l.shadowStrength,.62f);
                }
                foreach(var renderer in Object.FindObjectsByType<MeshRenderer>(FindObjectsSortMode.None))
                {
                    if(!renderer||renderer.gameObject.scene!=scene||
                       !renderer.gameObject.activeInHierarchy||!renderer.enabled)continue;
                    var src=renderer.sharedMaterials;
                    var dst=(Material[])src.Clone();
                    bool changed=false;
                    for(int i=0;i<src.Length;i++)
                    {
                        Material source=src[i];
                        if(!source||source.renderQueue>=3000)continue;
                        if(!palette.TryGetValue(source,out var copy))
                        {
                            copy=new Material(source){name="SAFE_PALETTE_PROOF / "+source.name};
                            var key=(renderer.name+" "+source.name).ToLowerInvariant();
                            var factor=TintFactor(key);
                            string prop=copy.HasProperty("_BaseColor")?"_BaseColor":
                                copy.HasProperty("baseColorFactor")?"baseColorFactor":
                                copy.HasProperty("_Color")?"_Color":null;
                            if(prop!=null)
                            {
                                var c=copy.GetColor(prop);
                                c=new Color(c.r*factor.r,c.g*factor.g,c.b*factor.b,c.a);
                                copy.SetColor(prop,c);
                            }
                            if(copy.HasProperty("_Smoothness"))
                                copy.SetFloat("_Smoothness",Mathf.Min(.22f,copy.GetFloat("_Smoothness")));
                            if(copy.HasProperty("_Metallic"))
                                copy.SetFloat("_Metallic",Mathf.Min(.14f,copy.GetFloat("_Metallic")));
                            copies.Add(copy);
                            palette.Add(source,copy);
                            audit.Add(source.name+" | "+source.shader.name+" | "+renderer.name+" | "+
                                factor.ToString("F3")+" | "+(prop??"NO_COLOR_PROPERTY"));
                        }
                        dst[i]=copy;changed=true;
                    }
                    if(changed){old.Add(renderer,src);renderer.sharedMaterials=dst;}
                }
            }
            static Color TintFactor(string key)
            {
                if(key.Contains("roof")||key.Contains("shingle")||key.Contains("slate")||
                   key.Contains("tejado")||key.Contains("techo"))
                    return new Color(.77f,.79f,.71f,1);
                if(key.Contains("stone")||key.Contains("stonework")||key.Contains("masonry")||
                   key.Contains("brick")||key.Contains("wall")||key.Contains("castle"))
                    return new Color(.89f,.92f,.95f,1);
                if(key.Contains("tree")||key.Contains("forest")||key.Contains("pine")||
                   key.Contains("leaf")||key.Contains("foliage"))
                    return new Color(.88f,.92f,.78f,1);
                if(key.Contains("wood")||key.Contains("timber")||key.Contains("sawmill"))
                    return new Color(.92f,.85f,.78f,1);
                if(key.Contains("road")||key.Contains("path")||key.Contains("paving"))
                    return new Color(.92f,.90f,.86f,1);
                return new Color(.93f,.93f,.90f,1);
            }
            public void Dispose()
            {
                foreach(var pair in old)if(pair.Key)pair.Key.sharedMaterials=pair.Value;
                foreach(var l in suns)if(l.light)
                {
                    l.light.color=l.color;
                    l.light.intensity=l.intensity;
                    l.light.shadowStrength=l.shadow;
                }
                RenderSettings.ambientMode=oldAmbient;
                RenderSettings.ambientLight=oldFill;
                foreach(var c in copies)if(c)Object.DestroyImmediate(c);
            }
        }

        sealed class MaterialStyleScope : IDisposable
        {
            readonly Dictionary<Renderer,Material[]> originals=new Dictionary<Renderer,Material[]>();
            readonly Dictionary<Material,Material> clones=new Dictionary<Material,Material>();
            readonly Dictionary<Texture,Texture2D> paintedTextures=new Dictionary<Texture,Texture2D>();
            readonly Color savedAmbient=RenderSettings.ambientLight;
            readonly AmbientMode savedAmbientMode=RenderSettings.ambientMode;
            readonly Color savedFog=RenderSettings.fogColor;
            readonly bool savedFogEnabled=RenderSettings.fog;
            readonly List<LightState> lights=new List<LightState>();
            static readonly string[] BaseTextureKeys={"_BaseMap","_MainTex","baseColorTexture","_BaseColorTexture"};
            public int RendererCount=>originals.Count;
            public int MaterialCount=>clones.Count;
            struct LightState {public Light light;public Color color;public float intensity;public float shadow;}

            public MaterialStyleScope(UnityEngine.SceneManagement.Scene scene)
            {
                RenderSettings.ambientMode=AmbientMode.Flat;
                RenderSettings.ambientLight=new Color(.68f,.72f,.70f,1);
                // No fog or post-process replacement: preserve actual scene.
                foreach(var sun in Object.FindObjectsByType<Light>(FindObjectsSortMode.None))
                {
                    if(sun.gameObject.scene!=scene || sun.type!=LightType.Directional)continue;
                    lights.Add(new LightState{light=sun,color=sun.color,intensity=sun.intensity,shadow=sun.shadowStrength});
                    sun.color=new Color(1f,.94f,.84f);
                    sun.intensity*=.88f;
                    sun.shadowStrength=Mathf.Max(sun.shadowStrength,.65f);
                }
                foreach(var r in Object.FindObjectsByType<MeshRenderer>(FindObjectsSortMode.None))
                {
                    if(!r || r.gameObject.scene!=scene || !r.enabled || !r.gameObject.activeInHierarchy)continue;
                    var old=r.sharedMaterials;
                    var next=(Material[])old.Clone();
                    bool changed=false;
                    for(int i=0;i<old.Length;i++)
                    {
                        var m=old[i];
                        if(!m || m.renderQueue>=3000 || !m.shader ||
                           m.shader.name.Contains("Sprites/") || m.shader.name.Contains("Particle"))continue;
                        if(!clones.TryGetValue(m,out var replacement))
                        {
                            replacement=new Material(m){name="PILOT BAKED PALETTE / "+m.name};
                            bool vfx=m.name.IndexOf("flame",StringComparison.OrdinalIgnoreCase)>=0||
                                     m.name.IndexOf("emission",StringComparison.OrdinalIgnoreCase)>=0||
                                     m.name.IndexOf("fire",StringComparison.OrdinalIgnoreCase)>=0||
                                     m.name.IndexOf("glow",StringComparison.OrdinalIgnoreCase)>=0||
                                     m.name.IndexOf("breach",StringComparison.OrdinalIgnoreCase)>=0;
                            if(!vfx)
                            {
                                foreach(var property in BaseTextureKeys)
                                {
                                    if(!m.HasProperty(property))continue;
                                    var source=m.GetTexture(property);
                                    if(source==null)continue;
                                    if(!paintedTextures.TryGetValue(source,out var baked))
                                    {
                                        baked=PaintOnce(source);
                                        paintedTextures.Add(source,baked);
                                    }
                                    if(baked)replacement.SetTexture(property,baked);
                                }
                                if(replacement.HasProperty("_BaseColor"))
                                {
                                    var c=replacement.GetColor("_BaseColor");
                                    replacement.SetColor("_BaseColor",Palette(c));
                                }
                                else if(replacement.HasProperty("baseColorFactor"))
                                {
                                    var c=replacement.GetColor("baseColorFactor");
                                    replacement.SetColor("baseColorFactor",Palette(c));
                                }
                                else if(replacement.HasProperty("_Color"))
                                {
                                    var c=replacement.GetColor("_Color");
                                    replacement.SetColor("_Color",Palette(c));
                                }
                                if(replacement.HasProperty("_Smoothness"))
                                    replacement.SetFloat("_Smoothness",replacement.GetFloat("_Smoothness")*.72f);
                            }
                            clones.Add(m,replacement);
                        }
                        next[i]=replacement;changed=true;
                    }
                    if(changed){originals.Add(r,old);r.sharedMaterials=next;}
                }
                Mark("EDITOR_ORIGINAL_SHADERS_RETAINED_AND_TEXTURES_BAKED="+paintedTextures.Count);
            }

            static Color Palette(Color c)
            {
                float r=c.r,g=c.g,b=c.b;
                float lum=.2126f*r+.7152f*g+.0722f*b;
                float blue=Mathf.Clamp01((b-Mathf.Max(r,g)-.035f)*3.2f);
                float ochre=Mathf.Clamp01((r-b-.11f)*3f)*Mathf.Clamp01((g-b-.10f)*3f);
                bool warmFire=r>g*1.35f && g>b*1.3f;
                if(warmFire)return c;
                var col=new Color(r,g,b,c.a);
                col=Color.Lerp(col,new Color(lum*.77f,lum*.91f,lum*1.02f,c.a),blue*.84f);
                col=Color.Lerp(col,new Color(lum*.92f,lum*.99f,lum*.84f,c.a),ochre*.55f);
                col=Color.Lerp(col,new Color(lum,lum,lum,c.a),.12f);
                col.a=c.a;
                return col;
            }

            static Texture2D PaintOnce(Texture source)
            {
                int w=Mathf.Clamp(source.width,4,1024),h=Mathf.Clamp(source.height,4,1024);
                var rt=RenderTexture.GetTemporary(w,h,0,RenderTextureFormat.ARGB32);
                var previouslyActive=RenderTexture.active;
                Texture2D tex=null;
                try
                {
                    Graphics.Blit(source,rt);
                    RenderTexture.active=rt;
                    tex=new Texture2D(w,h,TextureFormat.RGBA32,true,false);
                    tex.ReadPixels(new Rect(0,0,w,h),0,0);
                    tex.Apply(false,false);
                    var pixels=tex.GetPixels32();
                    for(int k=0;k<pixels.Length;k++)
                    {
                        Color c=pixels[k];
                        // Preserve alpha and silhouette exactly; only recolor paint.
                        var p=Palette(c);
                        pixels[k]=new Color(p.r,p.g,p.b,c.a);
                    }
                    tex.SetPixels32(pixels);
                    tex.Apply(true,true); // read-only on GPU; avoid persistent readable CPU copy
                    tex.name="ART-PILOT-ONLY / "+source.name;
                    tex.filterMode=source.filterMode;
                    tex.wrapMode=source.wrapMode;
                    return tex;
                }
                catch
                {
                    if(tex)Object.DestroyImmediate(tex);
                    throw;
                }
                finally
                {
                    RenderTexture.active=previouslyActive;
                    RenderTexture.ReleaseTemporary(rt);
                }
            }
            public void Dispose()
            {
                foreach(var pair in originals)if(pair.Key)pair.Key.sharedMaterials=pair.Value;
                foreach(var mat in clones.Values)if(mat)Object.DestroyImmediate(mat);
                foreach(var tex in paintedTextures.Values)if(tex)Object.DestroyImmediate(tex);
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
