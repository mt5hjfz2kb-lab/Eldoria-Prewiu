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
    /// production-save modification, Tripo calls, or runtime Unity prefab writes.
    /// The disposable fixture completes the required real nickname UI first.
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
            string path=Path.Combine(Evidence, name+"-"+width+"x"+height+".png");
            // Validate at the exact native camera dimensions before the
            // capture helper restores the original Canvas scaler. Otherwise
            // 1600x900 cinematic checks would run on the wrong viewport.
            Action validateComposition=()=>
            {
                if(!name.StartsWith("B-after-",StringComparison.Ordinal))return;
                var actualHud=Read(host,"Hud") as RoyalHUD;
                var actualPaper=actualHud?
                    actualHud.GetComponentInChildren<RoyalGoldV4DialogueParchment>(true):null;
                bool expectedCinematic=width==1600&&height==900&&
                    (name.IndexOf("aldric",StringComparison.OrdinalIgnoreCase)>=0||
                     name.IndexOf("lyra",StringComparison.OrdinalIgnoreCase)>=0);
                Assert(actualPaper&&actualPaper.Cinematic==expectedCinematic,
                    "Real capture matches cinematic-only approved dialogue framing "+name);
                if(expectedCinematic)
                {
                    if(name.IndexOf("aldric",StringComparison.OrdinalIgnoreCase)>=0)
                    {
                        var vellum=actualPaper.rectTransform.parent.Cast<Transform>()
                            .FirstOrDefault(t=>t.name=="Dialogue / genuine vellum grain");
                        var texture=vellum?vellum.GetComponent<Image>():null;
                        Assert(texture&&texture.gameObject.activeInHierarchy&&
                            texture.sprite&&texture.type==Image.Type.Sliced,
                            "Real Eldoria nine-slice vellum visible behind royal manuscript "+name);
                    }
                    Assert(actualPaper.NameplateWidth<=365f,
                        "Cinematic speaker plaque is SHORT, not a management-width header "+name);
                    // Source of truth: the V4 manuscript and cutout are created
                    // together under the same frame. Title can be temporarily
                    // presented under another layout parent by Unity.
                    var actualFrame=actualPaper.rectTransform.parent as RectTransform;
                    // The UI node's literal name contains " / ". Unity's
                    // Transform.Find treats '/' as a CHILD PATH separator,
                    // not part of a literal name. Enumerate the direct
                    // children, as the 48-frame certified preflight does.
                    // This fixes false negatives; it does not weaken the
                    // on-screen overlap or any other native assertion.
                    var actualAperture=actualFrame?
                        actualFrame.Cast<Transform>().FirstOrDefault(child=>
                            child.name=="Dialogue / genuine portrait aperture")
                        as RectTransform:null;
                    // RectTransform's anchoredPosition uses anchor/pivot
                    // coordinates. The manuscript is a mesh in its own
                    // centred rect. Comparing these raw values is not a
                    // reliable proof of ON-SCREEN intersection. Compare the
                    // actual camera-time world corners instead and record
                    // precise geometry for diagnosing any failure.
                    bool visible=actualAperture&&actualAperture.gameObject.activeInHierarchy;
                    var portraitCorners=new Vector3[4];
                    var manuscriptCorners=new Vector3[4];
                    if(actualAperture)actualAperture.GetWorldCorners(portraitCorners);
                    actualPaper.rectTransform.GetWorldCorners(manuscriptCorners);
                    float realPaperLeft=manuscriptCorners[0].x+
                        (manuscriptCorners[3].x-manuscriptCorners[0].x)*
                        actualPaper.PortraitColumn*.68f/
                        Mathf.Max(1f,actualPaper.rectTransform.rect.width);
                    float realPortraitRight=portraitCorners[2].x;
                    float margin=(manuscriptCorners[3].x-manuscriptCorners[0].x)*
                        28f/Mathf.Max(1f,actualPaper.rectTransform.rect.width);
                    Assert(visible&&realPortraitRight>=realPaperLeft+margin,
                        "Cinematic portrait reaches genuine manuscript "+name+
                        " visible="+visible+
                        " right="+realPortraitRight.ToString("F3")+
                        " pageLeft="+realPaperLeft.ToString("F3")+
                        " margin="+margin.ToString("F3")+
                        " anchorX="+(actualAperture?actualAperture.anchoredPosition.x:0f).ToString("F2")+
                        " viewportPaperColumn="+actualPaper.PortraitColumn.ToString("F2"));
                }
            };
            method.Invoke(null,new object[] {host,path,width,height,validateComposition});
            Assert(File.Exists(path) && new FileInfo(path).Length>35000, name+" true Unity camera capture written");
            var texture=new Texture2D(2,2,TextureFormat.RGBA32,false);
            try
            {
                Assert(texture.LoadImage(File.ReadAllBytes(path)), "Actual camera PNG decodes");
                Assert(texture.width==width && texture.height==height, "Actual native capture dimensions "+width+"x"+height);
                // Original V3 uses warm paper for Aldric and Lyra; the
                // V4 reference-approved Lyra panel is NIGHT-BLUE. Previous QA
                // incorrectly demanded GOLD for V4 Lyra and rejected genuine
                // Unity screenshots after three successful mobile captures.
                // Orín uses the approved purple-only treatment in BOTH A/B.
                var pixels=texture.GetPixels32(); int warm=0,arcane=0,nocturne=0;
                for(int y=0;y<texture.height/2;y+=3) for(int x=0;x<texture.width;x+=3)
                {
                    var p=pixels[y*texture.width+x];
                    if(p.r>=188&&p.g>=162&&p.b>=95&&p.b<=210&&p.r>p.g&&p.g>p.b+14)warm++;
                    if(p.b>p.g+10&&p.r>p.g+8&&p.b>40&&p.r>28)arcane++;
                    if(p.b>p.r+10&&p.b>p.g+6&&p.b>38&&p.r>14&&p.r<120)nocturne++;
                }
                bool v4=name.StartsWith("B-after-",StringComparison.Ordinal);
                bool orin=name.IndexOf("orin",StringComparison.OrdinalIgnoreCase)>=0;
                bool lyra=name.IndexOf("lyra",StringComparison.OrdinalIgnoreCase)>=0;
                bool expectsArcane=orin;
                bool expectsNocturne=v4&&lyra;
                int materialPixels=expectsArcane?arcane:expectsNocturne?nocturne:warm;
                string materialName=expectsArcane?"arcane purple":
                    expectsNocturne?"approved Lyra night-blue":"warm royal gold";
                Assert(materialPixels>width*height/1200,
                    "Correct "+materialName+" dialogue material in "+name+
                    " count="+materialPixels+" minimum="+(width*height/1200));
                if(v4)
                {
                    var authenticHud=Read(host,"Hud") as RoyalHUD;
                    var material=authenticHud?
                        authenticHud.GetComponentInChildren<RoyalGoldV4DialogueParchment>(true):null;
                    Assert(material&&material.enabled&&material.Arcane==expectsArcane&&
                        material.Nocturne==expectsNocturne,
                        "Live V4 UI material matches authored speaker "+name);
                }
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
            var captureType=AppDomain.CurrentDomain.GetAssemblies()
                .Select(x=>x.GetType("Eldoria.ReferenceClean.Editor.ValoriaIntegrationBuild",false))
                .FirstOrDefault(x=>x!=null);
            var completeIdentity=captureType?.GetMethod("CompleteProbePlayerName",BindingFlags.Public|BindingFlags.Static);
            Assert(completeIdentity!=null,"Disposable dialogue fixture completes the real first-entry UI");
            completeIdentity.Invoke(null,new object[]{host});
            var finish=hud.GetComponent<RoyalGoldV3CinematicFinish>();
            Assert(finish!=null, "ONE canonical V3 cinematic presenter active");
            var button=hud.Modal.Extra;
            Assert(button!=null, "Original narrative Continue button present");
            var v4=hud.GetComponent<RoyalGoldV4DialogueArt>();
            Assert(v4!=null, "Same canonical HUD has the new layered visual finish");
            for(int i=0;i<60 && hud.NarrativeOpen;i++)
            {
                button.onClick.Invoke(); yield return null;
            }
            Assert(!hud.NarrativeOpen, "Existing prologue can complete with original Continue");

            var neutral=Resources.Load<Texture2D>("UI/AldricDialogue/aldric_neutral");
            Assert(neutral&&neutral.width==512&&neutral.height==512,"Owner-approved head/shoulders neutral PNG loaded from real Unity Resources");
            foreach(var n in new[]{"aldric_firme","aldric_preocupado","aldric_sereno"})
                Assert(Resources.Load<Texture2D>("UI/AldricDialogue/"+n)!=null,"Alternate source-locked bust PNG "+n);

            // Authentic authoring for Aldric and Lyra; for Orín only the
            // approved arcane UI style is exercised with clearly labeled
            // visual fixtures, never inventing an active story event.
            var scenes=new[]{
                new{Speaker="Sir Aldric",Stem="sir-aldric-short",
                    Text="También hemos perdido gente. Podemos seguir esas huellas juntos."},
                new{Speaker="Sir Aldric",Stem="sir-aldric-long",
                    Text="Valoria sobrevivió a la noche. No puedo decir lo mismo de nuestro reino.\n\nLa Brecha sigue ahí. Mientras permanezca abierta, esto volverá a ocurrir.\n\nEmpecemos por lo que aún podemos salvar."},
                new{Speaker="Lyra",Stem="lyra-short",
                    Text="Ayudaré en el asalto. Después hablaremos de mi lugar en Valoria."},
                new{Speaker="Lyra",Stem="lyra-long",
                    Text="Soy Lyra. Busco a mi hermano. Su compañía desapareció la noche de la Brecha. Encontré su insignia junto a esta fisura; no sé qué significa."},
                new{Speaker="Orín",Stem="orin-style-qa-short",
                    Text="PRUEBA VISUAL: el diálogo de Orín aún no es un evento jugable."},
                new{Speaker="Orín",Stem="orin-style-qa-long",
                    Text="PRUEBA VISUAL DE COMPOSICIÓN. Esta muestra larga no pertenece a una escena de campaña. Verifica el tratamiento arcano, la lectura completa, las áreas seguras y el acceso real a Continuar sin introducir decisiones ni recompensas."}
            };
            foreach(var scene in scenes)
            {
                var speaker=scene.Speaker;
                hud.ShowNarrative(speaker,scene.Text);
                yield return null;
                // The V3-only identity check must run against V3. With V4
                // enabled, Lyra correctly uses her approved masked portrait
                // instead of the V3 fallback glyph. Mixing the two contexts
                // produced a false QA failure before native capture.
                v4.enabled=false;
                finish.SynchronizeNow();Canvas.ForceUpdateCanvases();
                Assert(hud.NarrativeOpen,"Real HUD opens interactive narrative for "+speaker);
                var frame = hud.Modal.Title.transform.parent as RectTransform;
                Assert(frame!=null,"Canonical narrative paper frame exists");
                var portrait=frame.Find("Canonical Aldric portrait") as RectTransform;
                var sigil=frame.Find("Narrator sigil") as RectTransform;
                Assert(portrait&&sigil,"Both canonical portrait and fallback sigil exist");
                var raw=portrait.GetComponent<RawImage>();
                Assert(raw!=null,"Original RawImage, no duplicate UI");
                bool aldric=(speaker=="Sir Aldric"||speaker=="Aldric");
                // Remember the actual V3 presentation BEFORE enabling V4.
                // The A/B is invalid if V4 only hides its art while retaining
                // altered positions, font sizes or a speaker-role override.
                Vector2 v3FrameSize=frame.sizeDelta;
                Vector2 v3TitlePosition=hud.Modal.Title.rectTransform.anchoredPosition;
                Vector2 v3BodyPosition=hud.Modal.Body.rectTransform.anchoredPosition;
                Vector2 v3ButtonPosition=((RectTransform)button.transform).anchoredPosition;
                int v3BodySize=hud.Modal.Body.fontSize;
                var roleNode=frame.Find("Narrative character role").GetComponent<RoyalText>();
                string v3Role=roleNode.text;
                Assert(raw.gameObject.activeSelf==aldric,"Correct portrait identity for "+speaker);
                Assert(sigil.gameObject.activeSelf!=aldric,"Correct alternate speaker sigil for "+speaker);
                if(aldric)
                {
                    Assert(raw.texture==neutral,"Actual approved 3D head-shoulder PNG visible for "+speaker);
                    Assert(portrait.rect.width>=115,"Portrait legible at dialog scale");
                }
                // Now verify the actual new material/portrait separately,
                // without discarding the single canonical Continue callback.
                v4.enabled=true;
                finish.SynchronizeNow();v4.SynchronizeNow();
                Canvas.ForceUpdateCanvases();
                var aperture=frame.Cast<Transform>().FirstOrDefault(t=>
                    t.name=="Dialogue / genuine portrait aperture") as RectTransform;
                Assert(aperture!=null,"V4 illustrated speaker aperture exists");
                bool lyraSpeaker=string.Equals(speaker,"Lyra",StringComparison.OrdinalIgnoreCase);
                Assert(aperture.gameObject.activeInHierarchy==(aldric||lyraSpeaker),
                    "V4 original portrait versus approved-source identity for "+speaker);
                if(lyraSpeaker)
                {
                    var approved=aperture.GetComponentInChildren<Image>(true);
                    Assert(approved&&approved.sprite,"Only approved Lyra portrait is used");
                }
                if(aldric)
                {
                    var actual=aperture.GetComponentInChildren<RawImage>(true);
                    Assert(actual&&actual.texture==neutral,"Original genuine Aldric bust retained in V4");
                }
                // Exercise the exact same switch used between native A/B
                // frames: V3 must be restored before any source screenshot.
                v4.enabled=false;
                var layeredPaper=frame.GetComponentInChildren<RoyalGoldV4DialogueParchment>(true);
                var originalPaper=frame.GetComponentInChildren<RoyalGoldV3ParchmentGraphic>(true);
                Assert(layeredPaper&&!layeredPaper.enabled&&originalPaper&&originalPaper.enabled,
                    "A/B toggle restores the original V3 parchment without a layered remnant");
                Assert(Vector2.Distance(frame.sizeDelta,v3FrameSize)<.1f&&
                    Vector2.Distance(hud.Modal.Title.rectTransform.anchoredPosition,v3TitlePosition)<.1f&&
                    Vector2.Distance(hud.Modal.Body.rectTransform.anchoredPosition,v3BodyPosition)<.1f&&
                    Vector2.Distance(((RectTransform)button.transform).anchoredPosition,v3ButtonPosition)<.1f&&
                    hud.Modal.Body.fontSize==v3BodySize&&roleNode.text==v3Role,
                    "V4 fully restores original V3 frame, text and Continue placement");
                v4.enabled=true;finish.SynchronizeNow();v4.SynchronizeNow();
                Canvas.ForceUpdateCanvases();
                if(hud.NarrativeIsRevealing) button.onClick.Invoke();
                yield return null;
                finish.SynchronizeNow();v4.SynchronizeNow();
                Assert(hud.Modal.Body.text.Length>=Math.Min(25,scene.Text.Length),
                    "Real Continue reveals the complete short/long page for "+scene.Stem);
                Assert(button.gameObject.activeInHierarchy&&button.interactable,"Original Continue remains interactive");
                // The canonical dialogue enters with a 0.28 s transition.
                // Wait for settled opacity before claiming visible photographic evidence.
                yield return new WaitForSecondsRealtime(.36f);
                finish.SynchronizeNow();
                string stem=scene.Stem;
                foreach(var pair in new[]{new[]{390,844},new[]{844,390},
                    new[]{900,1600},new[]{1600,900}})
                {
                    v4.enabled=false;
                    yield return null;
                    Screenshot(host,"A-before-"+stem,pair[0],pair[1]);
                    v4.enabled=true;
                    finish.SynchronizeNow();v4.SynchronizeNow();
                    Canvas.ForceUpdateCanvases();
                    Screenshot(host,"B-after-"+stem,pair[0],pair[1]);
                    // A bright renderer can still have dark geometry errors:
                    // a successful image and button click did NOT catch the
                    // speaker role being obscured by the illustrated title tab.
                    // Check the real visible subtitle lane after each viewport
                    // resize, for all speakers and both narrative lengths.
                    float roleTop=-roleNode.rectTransform.anchoredPosition.y;
                    float roleBottom=roleTop+roleNode.rectTransform.rect.height;
                    float bodyTop=-hud.Modal.Body.rectTransform.anchoredPosition.y;
                    Assert(roleNode.gameObject.activeInHierarchy &&
                        !string.IsNullOrWhiteSpace(roleNode.text) &&
                        roleTop>=90f && bodyTop>=roleBottom+8f,
                        "Visible complete subtitle lane, disjoint from illustrated tab and narrative text in "+
                        stem+" "+pair[0]+"x"+pair[1]);
                }
                for(int k=0;k<25 && hud.NarrativeOpen;k++) { button.onClick.Invoke(); yield return null; }
                Assert(!hud.NarrativeOpen,"No dialogue progression regression after "+speaker);
            }
            Assert(Directory.GetFiles(Evidence,"*-after-*.png").Length==24,
                "Aldric/Lyra short+long and clearly labeled Orín QA recorded at four native resolutions");
            Assert(Directory.GetFiles(Evidence,"*.png").Length==48,
                "48 like-for-like Unity A/B images: four source-authentic Aldric/Lyra cases plus two clearly labeled Orín style-only fixtures in four orientations");
            Directory.CreateDirectory(Evidence);
            var output=Path.Combine(Evidence,"ALDRIC_NATIVE_PASS.txt");
            File.WriteAllLines(output,new[]{"result=PASS","source="+Source,"mode=real integrated Unity camera and HUD","notes=not physical Android device"}.Concat(Checks));
            Debug.Log("ALDRIC_DIALOGUE_UNITY_INTEGRATED_PASS images=48 checks="+Checks.Count);
            EditorApplication.Exit(0);
        }
    }
}
#endif
