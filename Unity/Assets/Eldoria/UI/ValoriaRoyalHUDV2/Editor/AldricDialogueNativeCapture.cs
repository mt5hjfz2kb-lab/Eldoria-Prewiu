#if UNITY_EDITOR
using System;
using System.IO;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using Eldoria.UI.RoyalGold;
using Eldoria.UI.RoyalGold.V2;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using Object = UnityEngine.Object;

namespace Eldoria.UI.RoyalGold.V2.Editor
{
    /// <summary>
    /// Read-only Unity native acceptance for an exact, owner-approved Sir Aldric bust.
    /// Uses the existing real integrated scene and RoyalHUD. No new narrative state,
    /// game save modification, Tripo calls, or runtime Unity prefab writes.
    /// </summary>
    [InitializeOnLoad]
    public static class AldricDialogueNativeCapture
    {
        private const string Flag = "Eldoria.Aldric.NativeDialogue.Capture";
        private const string Scene = "Assets/Eldoria/ArtTests/ValoriaReferenceClean/Integration/Valoria_City_World_UI.unity";
        private static string Evidence => Path.GetFullPath(
            Environment.GetEnvironmentVariable("ALDRIC_NATIVE_OUTPUT") ??
            Path.Combine(UnityEngine.Application.dataPath, "../../docs/evidence/valoria-reference-clean/aldric-bust-probe"));
        private static readonly List<string> Checks = new List<string>();
        private static string Source => Environment.GetEnvironmentVariable("GITHUB_SHA") ?? "local";

        static AldricDialogueNativeCapture()
        {
            EditorApplication.playModeStateChanged += Changed;
        }
        static void Assert(bool ok, string reason)
        {
            if (!ok) throw new InvalidOperationException("ALDRIC_NATIVE_FAIL " + reason);
            Debug.Log("ALDRIC_NATIVE_PASS " + reason);
            Checks.Add(reason);
        }
        static object Read(object o, string property)
        {
            var p = o.GetType().GetProperty(property, BindingFlags.Public | BindingFlags.Instance);
            if (p == null) throw new InvalidOperationException("Missing property " + property);
            return p.GetValue(o);
        }
        static void Finish(Exception e)
        {
            Directory.CreateDirectory(Evidence);
            File.WriteAllText(Path.Combine(Evidence, "FAILURE.txt"), e.ToString());
            Debug.LogException(e);
            EditorApplication.Exit(1);
        }
        public static void Capture()
        {
            try
            {
                Environment.SetEnvironmentVariable("VALORIA_GAMEPLAY_PROBE", "1");
                Directory.CreateDirectory(Evidence);
                EditorSceneManager.OpenScene(Scene);
                SessionState.SetBool(Flag, true);
                EditorApplication.EnterPlaymode();
            }
            catch (Exception e) { Finish(e); }
        }
        static void Changed(PlayModeStateChange state)
        {
            if (state != PlayModeStateChange.EnteredPlayMode || !SessionState.GetBool(Flag, false)) return;
            SessionState.SetBool(Flag, false);
            EditorApplication.delayCall += () =>
            {
                var host = Object.FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None)
                    .FirstOrDefault(x => x && x.GetType().Name == "ValoriaIntegratedWorld");
                if (host == null) { Finish(new Exception("Integrated world host not found")); return; }
                host.StartCoroutine(Guard(host));
            };
        }
        static IEnumerator Guard(MonoBehaviour host)
        {
            var it = Probe(host);
            while (true)
            {
                bool more; object value=null;
                try { more=it.MoveNext(); if(more)value=it.Current; }
                catch(Exception e) { Finish(e); yield break; }
                if (!more) yield break;
                yield return value;
            }
        }
        static void Screenshot(MonoBehaviour host, string name, int width, int height)
        {
            var type = AppDomain.CurrentDomain.GetAssemblies()
                .Select(x => x.GetType("Eldoria.ReferenceClean.Editor.ValoriaIntegrationBuild", false))
                .FirstOrDefault(x => x != null);
            Assert(type != null, "Existing native integration camera capture helper available");
            var method=type.GetMethod("Capture", BindingFlags.Public|BindingFlags.Static);
            Assert(method != null, "Existing true 3D/Canvas capture method available");
            string path=Path.Combine(Evidence, name+"-"+(width>height?"horizontal":"vertical")+".png");
            method.Invoke(null,new object[] {host,path,width,height});
            Assert(File.Exists(path) && new FileInfo(path).Length>35000, name+" true Unity camera capture written");
            var texture=new Texture2D(2,2,TextureFormat.RGBA32,false);
            try
            {
                Assert(texture.LoadImage(File.ReadAllBytes(path)), "Actual camera PNG decodes");
                Assert(texture.width==width && texture.height==height, "Actual native capture dimensions "+width+"x"+height);
                var pixels=texture.GetPixels32(); int warm=0;
                for (int y=0;y<texture.height/2;y+=3) for(int x=0;x<texture.width;x+=3)
                {
                    var p=pixels[y*texture.width+x];
                    if (p.r>=188&&p.g>=162&&p.b>=95&&p.b<=210&&p.r>p.g&&p.g>p.b+14) warm++;
                }
                Assert(warm>width*height/1200, "Actual dialogue parchment visually present in "+name);
            }
            finally { Object.Destroy(texture); }
        }
        static IEnumerator Probe(MonoBehaviour host)
        {
            RoyalHUD hud=null;
            for(int frame=0;frame<200;frame++)
            {
                yield return null;
                if (!(bool)Read(host,"Ready")) continue;
                hud=Read(host,"Hud") as RoyalHUD;
                if(hud)break;
            }
            Assert(hud != null, "Canonical integrated RoyalHUD ready");
            var guidance=Read(host,"Guidance") as Behaviour;
            if(guidance)guidance.enabled=false;
            var finish=hud.GetComponent<RoyalGoldV3CinematicFinish>();
            Assert(finish!=null, "ONE canonical V3 cinematic presenter active");
            var button=hud.Modal.Extra;
            Assert(button!=null, "Original narrative Continue button present");
            for(int i=0;i<60 && hud.NarrativeOpen;i++)
            {
                button.onClick.Invoke(); yield return null;
            }
            Assert(!hud.NarrativeOpen, "Existing prologue can complete with original Continue");

            var neutral=Resources.Load<Texture2D>("UI/AldricDialogue/aldric_neutral");
            Assert(neutral&&neutral.width==512&&neutral.height==512,"Owner-approved head/shoulders neutral PNG loaded from real Unity Resources");
            foreach(var n in new[]{"aldric_firme","aldric_preocupado","aldric_sereno"})
                Assert(Resources.Load<Texture2D>("UI/AldricDialogue/"+n)!=null,"Alternate source-locked bust PNG "+n);

            var speakerNames=new[]{"Sir Aldric","Lyra","Orín","NARRADOR","Aldric"};
            var phrase="Valoria necesita reconstruir sus murallas mientras vigilamos el bosque y protegemos a las familias. ";
            string longText=string.Concat(Enumerable.Repeat(phrase,4)).Substring(0,218);
            foreach(var speaker in speakerNames)
            {
                hud.ShowNarrative(speaker,longText);
                yield return null;
                finish.SynchronizeNow();
                Assert(hud.NarrativeOpen,"Real HUD opens interactive narrative for "+speaker);
                var frame = hud.Modal.Title.transform.parent as RectTransform;
                Assert(frame!=null,"Canonical narrative paper frame exists");
                var portrait=frame.Find("Canonical Aldric portrait") as RectTransform;
                var sigil=frame.Find("Narrator sigil") as RectTransform;
                Assert(portrait&&sigil,"Both canonical portrait and fallback sigil exist");
                var raw=portrait.GetComponent<RawImage>();
                Assert(raw!=null,"Original RawImage, no duplicate UI");
                bool aldric=(speaker=="Sir Aldric"||speaker=="Aldric");
                Assert(raw.gameObject.activeSelf==aldric,"Correct portrait identity for "+speaker);
                Assert(sigil.gameObject.activeSelf!=aldric,"Correct alternate speaker sigil for "+speaker);
                if(aldric)
                {
                    Assert(raw.texture==neutral,"Actual approved 3D head-shoulder PNG visible for "+speaker);
                    Assert(portrait.rect.width>=115,"Portrait legible at dialog scale");
                }
                if(hud.NarrativeIsRevealing) button.onClick.Invoke();
                yield return null;
                finish.SynchronizeNow();
                Assert(hud.Modal.Body.text.Length>100,"Full first page revealed with actual Continue");
                Assert(button.gameObject.activeInHierarchy&&button.interactable,"Original Continue remains interactive");
                Screenshot(host,"aldric-dialogue-"+(aldric?"aldric":speaker=="Orín"?"orin":speaker.ToLowerInvariant()),1600,900);
                Screenshot(host,"aldric-dialogue-"+(aldric?"aldric":speaker=="Orín"?"orin":speaker.ToLowerInvariant()),900,1600);
                for(int k=0;k<25 && hud.NarrativeOpen;k++) { button.onClick.Invoke(); yield return null; }
                Assert(!hud.NarrativeOpen,"No dialogue progression regression after "+speaker);
            }
            Directory.CreateDirectory(Evidence);
            var output=Path.Combine(Evidence,"ALDRIC_NATIVE_PASS.txt");
            File.WriteAllLines(output,new[]{"result=PASS","source="+Source,"mode=real integrated Unity camera and HUD","notes=not physical Android device"}.Concat(Checks));
            Debug.Log("ALDRIC_DIALOGUE_UNITY_INTEGRATED_PASS images=10 checks="+Checks.Count);
            EditorApplication.Exit(0);
        }
    }
}
#endif
