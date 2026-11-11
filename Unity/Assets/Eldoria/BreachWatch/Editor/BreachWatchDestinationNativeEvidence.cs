#if UNITY_EDITOR
using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;
using System.Security.Cryptography;
using Eldoria.Application;
using Eldoria.Domain;
using Eldoria.ReferenceClean;
using Eldoria.UI.RoyalGold;
using Eldoria.UI.RoyalGold.V2;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Object=UnityEngine.Object;

namespace Eldoria.BreachWatch.Editor
{
    /// <summary>
    /// Independent, disposable native capture for the REAL migrated
    /// Training, March, Heroes and Mail destinations. Uses the canonical
    /// screens and a temporary in-memory Chapter II/III probe snapshot.
    /// No player save, live campaign or production scene is modified.
    /// These images are an ART REVIEW, never automatic artistic acceptance.
    /// </summary>
    [InitializeOnLoad]
    public static class BreachWatchDestinationNativeEvidence
    {
        const string Flag="Vigilia.DestinationNative.Run";
        const string Scene="Assets/Eldoria/ArtTests/ValoriaReferenceClean/Integration/Valoria_City_World_UI.unity";
        const string Folder="BreachWatchDestinationEvidence";
        static ValoriaIntegratedWorld host;
        static int phase,storyPages;
        static double deadline,afterName;
        static bool busy;

        [Serializable] public sealed class Screen
        {
            public string destination,file,sha256;
            public int width,height;
            public bool oldCanvasOff,newCanvasOwnsScreen;
            public int interactiveButtons;
        }
        [Serializable] public sealed class Report
        {
            public string status,source,notes;
            public bool probeOnly,saveUnchanged,artworkApproved;
            public int screenshotCount;
            public Screen[] screens;
        }
        static BreachWatchDestinationNativeEvidence()
        {
            EditorApplication.update-=Tick;
            EditorApplication.update+=Tick;
        }
        static void Need(bool ok,string why)
        {
            if(!ok)throw new InvalidOperationException(why);
        }
        [MenuItem("Eldoria/Vigilia de la Brecha/Captura destinos reales, prueba aislada")]
        public static void CaptureBatch()
        {
            Need(!EditorApplication.isPlaying,"Must start from fresh Unity Editor");
            Environment.SetEnvironmentVariable("VALORIA_GAMEPLAY_PROBE","1");
            EditorSceneManager.OpenScene(Scene,OpenSceneMode.Single);
            SessionState.SetBool(Flag,true);
            phase=0;storyPages=0;busy=false;
            deadline=EditorApplication.timeSinceStartup+300;
            EditorApplication.EnterPlaymode();
        }
        static void Tick()
        {
            if(!SessionState.GetBool(Flag,false)||busy||!EditorApplication.isPlaying)
                return;
            try
            {
                if(deadline==0)deadline=EditorApplication.timeSinceStartup+300;
                Need(EditorApplication.timeSinceStartup<deadline,
                    "Destination probe startup timed out");
                if(!host)host=Object.FindFirstObjectByType<ValoriaIntegratedWorld>();
                if(!host||!host.Ready||!host.City||!host.Hud)return;
                Need(host.City.Probe,"Refuse migration QA on a real player save");
                if(phase==0)
                {
                    Eldoria.ReferenceClean.Editor.ValoriaIntegrationBuild.CompleteProbePlayerName(host);
                    afterName=EditorApplication.timeSinceStartup;
                    phase=1;return;
                }
                if(phase==1)
                {
                    if(host.Hud.NarrativeOpen)
                    {
                        Need(storyPages++<30,"Cannot finish canonical introduction");
                        host.Hud.Modal.Extra.onClick.Invoke();
                        return;
                    }
                    if(EditorApplication.timeSinceStartup-afterName<1.0)return;
                    phase=2;
                }
                if(phase==2)
                {
                    busy=true;
                    SessionState.SetBool(Flag,false);
                    CaptureRealDestinations();
                    Debug.Log("VIGILIA_REAL_DESTINATIONS_NATIVE_CAPTURED_ART_REVIEW_PENDING");
                    EditorApplication.Exit(0);
                }
            }
            catch(Exception e)
            {
                SessionState.SetBool(Flag,false);
                Debug.LogException(e);
                var dir=Path.Combine(UnityEngine.Application.dataPath,"../"+Folder);
                Directory.CreateDirectory(dir);
                File.WriteAllText(Path.Combine(dir,"error.txt"),e.ToString());
                EditorApplication.Exit(1);
            }
        }

        static void CaptureRealDestinations()
        {
            var folder=Path.GetFullPath(Path.Combine(UnityEngine.Application.dataPath,"../"+Folder));
            Directory.CreateDirectory(folder);
            Need(!host.Hud.NarrativeOpen,"Narrative must finish before UI fixture");
            var gateway=host.City.Gateway;
            var stateField=typeof(LocalGateway).GetField("state",
                BindingFlags.Instance|BindingFlags.NonPublic);
            Need(stateField!=null,"LocalGateway in-memory state unavailable");
            var actual=(PlayerState)stateField.GetValue(gateway);
            var before=JsonUtility.ToJson(gateway.Snapshot());
            var fixture=JsonUtility.FromJson<PlayerState>(JsonUtility.ToJson(actual));
            Need(fixture!=null,"Cannot clone disposable probe player state");
            fixture.BastionLevel=2;
            fixture.SawmillLevel=1;
            fixture.BarracksLevel=1;
            Need(fixture.ChapterProgress!=null,"In-memory chapter II progression missing");
            // The Scouts destination is introduced ONLY after canonical
            // march confirmation; this is an isolated QA state, never a save.
            fixture.ChapterProgress.MarchConfirmed=true;
            if(fixture.ChapterThree==null)fixture.ChapterThree=new BastionThreeState();
            fixture.ChapterThree.LyraRecruited=true;
            if(fixture.Mailbox==null)fixture.Mailbox=new EldoriaV41Mailbox();
            if(fixture.Mailbox.ReceivedEventIds==null)
                fixture.Mailbox.ReceivedEventIds=new List<string>();
            fixture.Mailbox.ReceivedEventIds.Add("VIGILIA_NATIVE_FIXTURE_ONLY");
            // Test-only in-memory injection of an advanced progression state.
            // Do not call the gateway commit/save path or execute purchases.
            stateField.SetValue(gateway,fixture);
            var shots=new List<Screen>();
            var profileShots=new List<Screen>();
            var unlockedNavShots=new List<Screen>();
            var retentionShots=new List<Screen>();
            try
            {
                host.City.Apply(fixture);
                host.Hud.Refresh();
                var ui=BreachWatchHud.Install(host);
                Need(ui.IndependentCanvas&&ui.LegacyPresentationHidden,
                    "Real destination previews must not show original HUD canvas");
                var military=ui.RealDestinations;
                Need(military!=null,"Missing staged canonical military bridge");
                // Check the genuine Chapter III navigation, not merely
                // source code. The 5 approved medallions must all fit after
                // their own story unlock; Army/Heroes MUST use the new Canvas.
                var navField=typeof(BreachWatchHud).GetField("navButtons",
                    BindingFlags.Instance|BindingFlags.NonPublic);
                var nav=navField?.GetValue(ui) as Button[];
                ui.Reflow(900,1600);
                ui.RenderNow();
                Need(nav!=null && nav.Length==5 && nav[2] &&
                     nav[4] && nav[2].interactable && nav[4].interactable,
                     "Real army and heroine medallions missing after Chapter III unlock");
                foreach(var size in new[]{(900,1600),(390,844)})
                    unlockedNavShots.Add(Capture(ui,folder,"nav-advanced",
                        size.Item1,size.Item2,
                        ()=>nav[2].interactable && nav[4].interactable &&
                            ui.LegacyPresentationHidden));
                nav[2].onClick.Invoke();
                Need(military.IsShowing && military.HasApprovedTheme,
                     "Army navigation did not open the ONE real repainted march screen");
                military.Close();
                nav[4].onClick.Invoke();
                Need(ui.RealHeroes && ui.RealHeroes.IsShowing &&
                     ui.RealHeroes.ReplacedOldFrames,
                     "Heroes navigation did not open the ONE real character screen");
                ui.RealHeroes.Close();
                if(military.IsShowing)military.Close();
                var available=host.CanOpen(RoyalSection.Army);
                var manager=host.Hud.GetComponent<RoyalGoldV3ManagementScreens>();
                Debug.Log("VIGILIA_TRAINING_PREVIEW_GATE available="+available+
                    " manager="+(manager!=null)+
                    " modal="+(host.Hud.Modal&&host.Hud.Modal.gameObject.activeSelf)+
                    " narrative="+host.Hud.NarrativeOpen+
                    " b="+gateway.Snapshot().BastionLevel+
                    " barracks="+gateway.Snapshot().BarracksLevel);
                bool opened=military.TryOpen(
                    RoyalGoldV3ManagementScreens.Destination.Training);
                Debug.Log("VIGILIA_TRAINING_PREVIEW_STATE opened="+opened+
                    " canonical="+(manager!=null&&manager.IsOpen)+
                    " showing="+military.IsShowing+
                    " approved="+military.HasApprovedTheme+
                    " diagnostics="+military.NativeThemeDiagnostic);
                Need(available && opened && military.IsShowing &&
                     military.HasApprovedTheme,
                    "Training staging failed: gate="+available+
                    " opened="+opened+" canonical="+
                    (manager!=null&&manager.IsOpen)+
                    " showing="+military.IsShowing+
                    " theme="+military.HasApprovedTheme);
                CaptureMany(ui,folder,"training",shots,()=>military.IsShowing &&
                    military.HasApprovedTheme);
                military.Close();
                Need(!military.IsShowing,"Training return failed");
                Need(military.TryOpen(RoyalGoldV3ManagementScreens.Destination.March)
                    && military.HasApprovedTheme,
                    "March displayed original blue UI or failed to open");
                CaptureMany(ui,folder,"march",shots,()=>military.IsShowing &&
                    military.HasApprovedTheme);
                military.Close();

                var heroes=ui.RealHeroes;
                Need(heroes!=null && heroes.TryOpen() &&
                    heroes.IsShowing && heroes.ReplacedOldFrames,
                    "Hero destination not adopted or still has old Royal frames");
                CaptureMany(ui,folder,"heroes",shots,()=>heroes.IsShowing &&
                    heroes.ReplacedOldFrames);

                // The Salon alone does NOT validate the rest of the game UI.
                // Capture the four real interactive Aldric profile pages and
                // Lyra's authentic profile under the same independent Canvas.
                // Do not approve a good lobby if abilities, talents, gear or
                // role details still use old frames, clip text or lose touch.
                var canonicalHeroes=host.Hud.GetComponent<
                    Eldoria.HeroesUI.HeroesApprovedDestination>();
                Need(canonicalHeroes && canonicalHeroes.OpenHero("aldric"),
                    "Cannot open real Sir Aldric profile in advanced fixture");
                var pages=new[] {
                    Eldoria.HeroesUI.HeroesApprovedDestination.Page.Resumen,
                    Eldoria.HeroesUI.HeroesApprovedDestination.Page.Habilidades,
                    Eldoria.HeroesUI.HeroesApprovedDestination.Page.Talentos,
                    Eldoria.HeroesUI.HeroesApprovedDestination.Page.Equipamiento
                };
                foreach(var page in pages)
                {
                    canonicalHeroes.Select(page);
                    Need(canonicalHeroes.IsOpen &&
                         canonicalHeroes.CurrentPage==page,
                        "Canonical hero tab was not selected: "+page);
                    string name="hero-aldric-"+page.ToString().ToLowerInvariant();
                    foreach(var size in new[]{(900,1600),(390,844)})
                        profileShots.Add(Capture(ui,folder,name,
                            size.Item1,size.Item2,
                            ()=>heroes.IsShowing && heroes.ReplacedOldFrames &&
                            canonicalHeroes.CurrentPage==page));
                }
                Need(canonicalHeroes.OpenHero("lyra"),
                    "Genuine Lyra cannot open despite being recruited in fixture");
                foreach(var size in new[]{(900,1600),(390,844)})
                    profileShots.Add(Capture(ui,folder,"hero-lyra-resumen",
                        size.Item1,size.Item2,
                        ()=>heroes.IsShowing && heroes.ReplacedOldFrames &&
                            canonicalHeroes.CurrentHeroId=="lyra"));
                heroes.Close();
                // Close heroes may return to a real management controller;
                // the bridge must adopt and repaint it, not leave it hidden.
                if(military.IsShowing)military.Close();

                var mail=ui.RealMailbox;
                Need(mail!=null && mail.TryOpen() && mail.IsShowing &&
                     mail.ReplacedOldPainting,
                     "Real introduced mailbox still uses old parchment or is inaccessible");
                CaptureMany(ui,folder,"mail",shots,()=>mail.IsShowing &&
                    mail.ReplacedOldPainting);
                mail.Close();
                Need(!mail.IsShowing,"Mailbox exit failed");
                // Previously unverified canonical Vigilantes/Exploradores
                // cannot remain invisible under the disabled original Canvas.
                // Preserve actual saved progression, background, reports,
                // encounter decisions and hotkeys. Test native UI only.
                var retention=ui.RealRetention;
                Need(retention!=null && retention.TryOpen(
                    RoyalGoldV3EarlyRetentionScreens.Destination.Vigilants) &&
                    retention.ReplacedOldChrome,
                    "Real unlocked Vigilants still opens old HUD chrome");
                CaptureMany(ui,folder,"vigilants",retentionShots,
                    ()=>retention.IsShowing && retention.ReplacedOldChrome);
                retention.Close();
                Need(!retention.IsShowing,
                    "Vigilants did not return to new city Canvas");
                Need(retention.TryOpen(
                    RoyalGoldV3EarlyRetentionScreens.Destination.Scouts) &&
                    retention.ReplacedOldChrome,
                    "Real unlocked Scouts still opens old HUD chrome");
                CaptureMany(ui,folder,"scouts",retentionShots,
                    ()=>retention.IsShowing && retention.ReplacedOldChrome);
                retention.Close();
                Need(!retention.IsShowing,
                    "Scouts did not return to new city Canvas");
            }
            finally
            {
                // No persistence writes; restore exact original in-memory
                // authoritative snapshot before making any PASS claim.
                stateField.SetValue(gateway,actual);
                host.City.Apply(actual);
                host.Hud.Refresh();
            }
            var after=JsonUtility.ToJson(gateway.Snapshot());
            Need(before==after,"Migration fixture changed original player state");
            Need(shots.Count==16,"Expected sixteen real native destination captures");
            Need(profileShots.Count==10,
                "Expected ten additional real Unity hero profile/page captures");
            Need(unlockedNavShots.Count==2,
                "Two genuinely unlocked city navigation captures required");
            Need(retentionShots.Count==8,
                "Expected four real Unity orientations for each retained destination");
            var report=new Report{
                status="VIGILIA_DESTINATIONS_CAPTURED_ART_REVIEW_PENDING",
                source=Environment.GetEnvironmentVariable("GITHUB_SHA")??"local",
                notes="Four real authoritative destinations in test-only injected advanced state. Native phone/desktop images; no player save mutation, no real purchase and NO visual approval.",
                probeOnly=true,saveUnchanged=true,artworkApproved=false,
                screenshotCount=shots.Count,screens=shots.ToArray()
            };
            File.WriteAllText(Path.Combine(folder,"BreachWatchDestinationEvidence.json"),
                JsonUtility.ToJson(report,true));
            // Separate, SHA256-labelled supplemental art audit. The canonical
            // 16-image destination manifest stays unchanged for existing CI.
            var heroReport=new Report {
                status="VIGILIA_REAL_HERO_PROFILES_CAPTURED_ART_REVIEW_PENDING",
                source=report.source,
                notes="Real Aldric summary/abilities/talents/equipment and Lyra summary. Ten portrait/mobile screenshots, original gameplay state restored.",
                probeOnly=true,saveUnchanged=true,artworkApproved=false,
                screenshotCount=profileShots.Count,
                screens=profileShots.ToArray()
            };
            File.WriteAllText(Path.Combine(folder,
                "BreachWatchHeroProfilesEvidence.json"),
                JsonUtility.ToJson(heroReport,true));
            var navigationReport=new Report {
                status="VIGILIA_ACTUAL_UNLOCKED_NAVIGATION_CAPTURED_ART_REVIEW_PENDING",
                source=report.source,
                notes="Two real 5-medallion Chapter III HUD renders and real Army/Heroes click routing, old Canvas disabled, no save mutation.",
                probeOnly=true,saveUnchanged=true,artworkApproved=false,
                screenshotCount=unlockedNavShots.Count,
                screens=unlockedNavShots.ToArray()
            };
            File.WriteAllText(Path.Combine(folder,
                "BreachWatchAdvancedNavigationEvidence.json"),
                JsonUtility.ToJson(navigationReport,true));
            var retentionReport=new Report {
                status="VIGILIA_RETENTION_DESTINATIONS_CAPTURED_ART_REVIEW_PENDING",
                source=report.source,
                notes="Real unlocked Vigilants and Scouts, four orientations each, canonical frame replaced, temporary state restored, no reward claim or data mutation.",
                probeOnly=true,saveUnchanged=true,artworkApproved=false,
                screenshotCount=retentionShots.Count,
                screens=retentionShots.ToArray()
            };
            File.WriteAllText(Path.Combine(folder,
                "BreachWatchRetentionEvidence.json"),
                JsonUtility.ToJson(retentionReport,true));
        }

        static void CaptureMany(BreachWatchHud ui,string folder,string name,
            List<Screen> output,Func<bool> verify)
        {
            foreach(var size in new[]{(900,1600),(390,844),(1600,900),(844,390)})
                output.Add(Capture(ui,folder,name,size.Item1,size.Item2,verify));
        }

        static Screen Capture(BreachWatchHud ui,string folder,string name,
            int width,int height,Func<bool> verify)
        {
            var camera=host.PlayerCamera;
            Need(camera,"Canonical game camera missing");
            var lastRt=camera.targetTexture;
            var lastAspect=camera.aspect;
            var lastMask=camera.cullingMask;
            var lastActive=RenderTexture.active;
            var rt=new RenderTexture(width,height,24,RenderTextureFormat.ARGB32);
            var texture=new Texture2D(width,height,TextureFormat.RGB24,false);
            var file="real-unity-"+name+"-"+width+"x"+height+".png";
            try
            {
                camera.targetTexture=rt;
                camera.aspect=width/(float)height;
                camera.cullingMask|=1<<5;
                ui.NativeCanvas.worldCamera=camera;
                ui.Reflow(width,height);
                ui.RenderNow();
                Canvas.ForceUpdateCanvases();
                // Destination layouts get the actual changed Canvas bounds.
                var management=host.Hud.GetComponent<RoyalGoldV3ManagementScreens>();
                if(management && management.IsOpen)management.SynchronizeLayout();
                if(ui.RealDestinations && ui.RealDestinations.IsShowing)
                    ui.RealDestinations.SynchronizeVisual();
                if(name=="march" && height>width)
                    Need(ui.RealDestinations &&
                         ui.RealDestinations.RealMarchTwoCardRoster,
                         "Actual portrait march still shows the permanently "+
                         "disabled third hero or lacks the two enlarged real slots; "+
                         width+"x"+height);
                if(name=="training" && height>width)
                    Need(ui.RealDestinations && 
                         ui.RealDestinations.RealTrainingCinematic,
                         "Full archer composition did not render in portrait "+
                         width+"x"+height+"; reject decorative-only PASS. "+
                         ui.RealDestinations.NativeTrainingLayoutDiagnostic);
                var mailbox=host.Hud.GetComponent<RoyalGoldV41MailboxScreen>();
                if(mailbox && mailbox.IsOpen)mailbox.SynchronizeLayout();
                var heroes=ui.RealHeroes;
                if(heroes && heroes.IsShowing)
                {
                    host.Hud.GetComponent<Eldoria.HeroesUI.HeroesApprovedDestination>().Refresh();
                    heroes.SynchronizeVisual();
                }
                Canvas.ForceUpdateCanvases();
                if(name=="heroes")
                    Need(heroes && heroes.HallGradeActive,
                        "Original blue Heroes Hall is not warmed by new scene material: "+
                        (heroes?heroes.HallGradeDiagnostic:"heroes bridge null"));
                Need(verify(),"Real destination visual replacement regressed: "+name);
                camera.Render();Canvas.ForceUpdateCanvases();camera.Render();
                RenderTexture.active=rt;
                texture.ReadPixels(new Rect(0,0,width,height),0,0);
                texture.Apply();
                string full=Path.Combine(folder,file);
                File.WriteAllBytes(full,texture.EncodeToPNG());
                int buttons=ui.GetComponentsInChildren<Button>(true)
                    .Count(b=>b && b.IsActive() && b.interactable);
                Need(buttons>0,"Real destination has no interactive controls: "+name);
                return new Screen{
                    destination=name,file=file,sha256=Hash(full),width=width,height=height,
                    oldCanvasOff=ui.LegacyPresentationHidden,
                    newCanvasOwnsScreen=verify(),interactiveButtons=buttons
                };
            }
            finally
            {
                camera.targetTexture=lastRt;camera.aspect=lastAspect;
                camera.cullingMask=lastMask;
                RenderTexture.active=lastActive;
                rt.Release();Object.DestroyImmediate(rt);
                Object.DestroyImmediate(texture);
            }
        }
        static string Hash(string path)
        {
            using(var sha=SHA256.Create())
            using(var stream=File.OpenRead(path))
                return BitConverter.ToString(sha.ComputeHash(stream))
                    .Replace("-","").ToLowerInvariant();
        }
    }
}
#endif
