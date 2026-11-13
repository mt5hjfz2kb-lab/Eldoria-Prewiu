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
        static int mailboxFirstHitClicks;

        static void VerifyMarchLayoutRestoration(RoyalGoldV3ManagementScreens manager,
            BreachWatchMarchComposition composition)
        {
            composition.Restore();
            manager.SynchronizeLayout();
            var root=manager.VisibleRoot;
            // Snapshot actual controller geometry independently of the adapter.
            // Two cycles detect accumulated drift after reopening the screen.
            for(int cycle=0;cycle<2;cycle++)
            {
                var controls=root.GetComponentsInChildren<RectTransform>(true)
                    .Where(x=>x && !x.name.Contains("information") &&
                        !x.GetComponentsInParent<RectTransform>(true)
                            .Any(p=>p.name=="March / army information sheet"))
                    .ToArray();
                var before=controls.Select(x=>new[] {x.anchorMin,x.anchorMax,
                    x.pivot,x.anchoredPosition,x.sizeDelta}).ToArray();
                composition.RefreshScorecard();
                Need(composition.UsesThreeHeroSpaces,
                    "March restore probe did not exercise actual hero layout");
                composition.Restore();
                for(int i=0;i<controls.Length;i++)
                {
                    var x=controls[i];if(!x)continue;
                    var after=new[] {x.anchorMin,x.anchorMax,x.pivot,
                        x.anchoredPosition,x.sizeDelta};
                    for(int j=0;j<after.Length;j++)
                        Need(Vector2.Distance(before[i][j],after[j])<.01f,
                            "March layout restore drift: "+x.name+
                            " field="+j+" cycle="+cycle);
                }
            }
            Debug.Log("VIGILIA_REAL_MARCH_LAYOUT_RESTORED_TWO_CYCLES");
        }

        [Serializable] sealed class CaptionInkProof
        {
            public string control,text;
            public int fontSize,visibleCharacters,outsidePixels,changedPixels;
            public float preferredWidth,preferredHeight;
            public Rect textRect,ownerScreenRect;
            public Vector3 localScale;
            public bool wrap,bestFit;
        }
        [Serializable] sealed class CaptionInkProofSet
        { public List<CaptionInkProof> captions=new List<CaptionInkProof>(); }

        static void VerifyMarchCaptionInk(RectTransform root,Camera camera,
            RenderTexture rt,Texture2D original,string folder,int width,int height)
        {
            var proof=new CaptionInkProofSet();
            var originals=original.GetPixels32();
            var owners=root.GetComponentsInChildren<RectTransform>(true)
                .Where(x=>x && (x.name.StartsWith("Hero slot ") ||
                    x.name=="Edit march by percent")).ToArray();
            foreach(var owner in owners)
            {
                var label=owner.GetComponentsInChildren<Text>(true)
                    .FirstOrDefault(x=>x && x.name=="Button label");
                Need(label,"Missing actual caption for "+owner.name);
                var corners=new Vector3[4];owner.GetWorldCorners(corners);
                var points=corners.Select(x=>camera.WorldToScreenPoint(x)).ToArray();
                var bounds=Rect.MinMaxRect(points.Min(x=>x.x)-2f,points.Min(x=>x.y)-2f,
                    points.Max(x=>x.x)+2f,points.Max(x=>x.y)+2f);
                var item=new CaptionInkProof { control=owner.name,text=label.text,
                    fontSize=label.fontSize,visibleCharacters=label.cachedTextGenerator.characterCountVisible,
                    preferredWidth=label.preferredWidth,preferredHeight=label.preferredHeight,
                    textRect=label.rectTransform.rect,ownerScreenRect=bounds,
                    localScale=label.rectTransform.localScale,
                    wrap=label.horizontalOverflow==HorizontalWrapMode.Wrap,bestFit=label.resizeTextForBestFit };
                bool enabled=label.enabled;
                var hidden=new Texture2D(width,height,TextureFormat.RGB24,false);
                try
                {
                    label.enabled=false;Canvas.ForceUpdateCanvases();AuditExclusivePresentation();camera.Render();
                    RenderTexture.active=rt;hidden.ReadPixels(new Rect(0,0,width,height),0,0);hidden.Apply();
                    var pixels=hidden.GetPixels32();
                    for(int y=0;y<height;y++)for(int x=0;x<width;x++)
                    {
                        int index=y*width+x;var a=originals[index];var b=pixels[index];
                        if(Mathf.Abs(a.r-b.r)+Mathf.Abs(a.g-b.g)+Mathf.Abs(a.b-b.b)<=80)continue;
                        item.changedPixels++;
                        if(!bounds.Contains(new Vector2(x,y)))item.outsidePixels++;
                    }
                }
                finally
                { label.enabled=enabled;Canvas.ForceUpdateCanvases();AuditExclusivePresentation();camera.Render();Object.DestroyImmediate(hidden); }
                proof.captions.Add(item);
            }
            File.WriteAllText(Path.Combine(folder,"march-caption-ink-"+width+"x"+height+".json"),
                JsonUtility.ToJson(proof,true));
            Need(proof.captions.Count==4 && proof.captions.All(x=>x.changedPixels>8 && x.outsidePixels<=6),
                "Actual GPU caption ink spills beyond its own control: "+
                string.Join("; ",proof.captions.Select(x=>x.control+" font="+x.fontSize+
                    " outside="+x.outsidePixels+" visible="+x.changedPixels)));
        }

        [Serializable] public sealed class Screen
        {
            public string destination,file,sha256;
            public int width,height;
            public bool oldCanvasOff,newCanvasOwnsScreen;
            public int interactiveButtons;
            public bool authoredMailTextureVerified;
            public bool heroIntroGpuClear;
            public int heroIntroVisiblePixels,heroIntroOverlapPixels;
            public int authoredMailChangedPixels;
            public int authoredMailUpperChangedPixels;
        }
        [Serializable] public sealed class Report
        {
            public string status,source,notes;
            public bool probeOnly,saveUnchanged,artworkApproved;
            public int screenshotCount;
            public int actualMailboxFirstHitClicks;
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
        static void AuditExclusivePresentation()
        {
            if(!BreachWatchProductionPresentation.Required)return;
            var ui=Object.FindFirstObjectByType<BreachWatchHud>();
            Need(ui && ui.ProductionPresentation,"Production new UI was not installed");
            var isolation=ui.GetComponent<BreachWatchProductionPresentation>();
            Need(isolation,"Production art isolation boundary missing");
            ui.GetComponent<BreachWatchTypography>()?.SynchronizeNow();
            isolation.Enforce();
            Need(isolation.RetiredCanvasDisabled,"Retired UI Canvas reactivated");
            string old=isolation.FirstVisibleRetiredPainting();
            Need(string.IsNullOrEmpty(old),"Legacy UI artwork still visible: "+old);
            Debug.Log("VIGILIA_EXCLUSIVE_PRESENTATION_NATIVE_PASS");
        }

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
            // This disposable advanced fixture must satisfy the same earned
            // introductions as the integrated production narrative gate.
            foreach(var beat in new[]{"march-preparation","vigilants-introduction","scouts-introduction"})
                if(!fixture.ChapterProgress.SeenNarrative.Contains(beat))
                    fixture.ChapterProgress.SeenNarrative.Add(beat);
            if(fixture.ChapterThree==null)fixture.ChapterThree=new BastionThreeState();
            fixture.ChapterThree.LyraRecruited=true;
            if(fixture.Mailbox==null)fixture.Mailbox=new EldoriaV41Mailbox();
            if(fixture.Mailbox.ReceivedEventIds==null)
                fixture.Mailbox.ReceivedEventIds=new List<string>();
            fixture.Mailbox.ReceivedEventIds.Add("VIGILIA_NATIVE_FIXTURE_ONLY");
            // Test-only in-memory injection of an advanced progression state.
            // Do not call the gateway commit/save path or execute purchases.
            stateField.SetValue(gateway,fixture);
            mailboxFirstHitClicks=0;
            var shots=new List<Screen>();
            var profileShots=new List<Screen>();
            var unlockedNavShots=new List<Screen>();
            var retentionShots=new List<Screen>();
            var mailContentShots=new List<Screen>();
            var treasureShots=new List<Screen>();
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
                     "Army navigation did not open the ONE real repainted march screen: "+military.NativeThemeDiagnostic);
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
                Need(host.Select("barracks"),"Built barracks cannot be selected in city");
                ui.RenderNow();
                var trainingEntry=ui.ActualBarracksTrainingEntry;
                Need(trainingEntry && trainingEntry.gameObject.activeInHierarchy &&
                     trainingEntry.interactable,"Built barracks has no actual Training access");
                CaptureMany(ui,folder,"barracks-entry",shots,()=>
                    trainingEntry.gameObject.activeInHierarchy && !military.IsShowing);
                ClickActualMailControl(ui,trainingEntry,false);
                bool opened=military.IsShowing && manager.CurrentDestination==
                    RoyalGoldV3ManagementScreens.Destination.Training;
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
                var marchManager=host.Hud.GetComponent<RoyalGoldV3ManagementScreens>();
                var marchComposition=marchManager.VisibleRoot.GetComponent<BreachWatchMarchComposition>();
                Need(marchComposition && !marchComposition.SecondaryDetailsOpen,
                    "March must open with secondary details closed");
                ClickActualMailControl(ui,marchComposition.InformationButton,false);
                Need(marchComposition.SecondaryDetailsOpen,
                    "Genuine information first-hit did not open actual army details");
                string informationFolder=Path.Combine(folder,"MarchInformationEvidence");
                Directory.CreateDirectory(informationFolder);
                Capture(ui,informationFolder,"march-information",390,844,
                    ()=>military.IsShowing && marchComposition.SecondaryDetailsOpen);
                ClickActualMailControl(ui,marchComposition.InformationReturnButton,false);
                Need(!marchComposition.SecondaryDetailsOpen && military.IsShowing,
                    "Genuine details return did not preserve preparation");
                VerifyMarchLayoutRestoration(marchManager,marchComposition);
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
                Need(heroes.OriginalLyraIllustrationBound,
                    "Lyra must reuse her original-reference identity, not the old silver-haired fallback");
                var lyra=canonicalHeroes.ReadHeroes().First(hero=>hero.Id=="lyra");
                Need(lyra.Portrait && lyra.Portrait.name==
                    BreachWatchHeroesBridge.OriginalLyraBustName,
                    "Hero list/profile data are not bound to the original Lyra bust");
                var lyraPath=AssetDatabase.GetAssetPath(lyra.Portrait.texture);
                Need(lyraPath=="Assets/Eldoria/BreachWatch/Resources/VigiliaArt/Heroes/canonical_lyra_illustration.png" ||
                     lyraPath=="Assets/Eldoria/DailyTreasures/Resources/DailyTreasureCeremonies/canonical_lyra_illustration.png",
                    "Lyra portrait is not the original approved-reference source texture");
                var lyraImporter=AssetImporter.GetAtPath(lyraPath) as TextureImporter;
                Need(lyraImporter && !lyraImporter.isReadable &&
                     !lyraImporter.mipmapEnabled && lyraImporter.maxTextureSize<=1024 &&
                     lyra.Portrait.texture.width<=1024 && lyra.Portrait.texture.height<=1024,
                    "Original Lyra texture import must remain bounded, mipless and non-readable");
                var lyraSourceFile=Path.Combine(UnityEngine.Application.dataPath,lyraPath.Substring(7));
                Need(Hash(lyraSourceFile)=="58cca82816e86ab8ae70ee29b3b941db591e80ecc85148da55487beecb1a4fef",
                    "Lyra source PNG changed from the actual original-reference extraction");
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
                // Actual first delivery is present in this isolated advanced
                // player state. The bell must appear only now, not in the
                // fresh-save Chapter-I screen. Invoke the REAL UI control,
                // not just the direct service/bridge method.
                ui.RenderNow();
                var mailButton=ui.ActualMailboxEntry;
                Need(mailButton && mailButton.gameObject.activeInHierarchy &&
                     mailButton.interactable,
                     "Genuine mail entry medallion not shown after first delivery");
                ClickActualMailControl(ui,mailButton);
                Need(mail!=null && mail.IsShowing && mail.ReplacedOldPainting &&
                     !mailButton.gameObject.activeInHierarchy,
                     "Mail medallion did not open canonical new UI or stayed over its panel");
                CaptureMany(ui,folder,"mail",shots,()=>mail.IsShowing &&
                    mail.ReplacedOldPainting);

                // Art QA of the POPULATED mailbox is indispensable. The
                // original 16 screenshots exercised only empty categories.
                // A purely disposable in-memory canonical domain event now
                // tests one real row and its real report detail, with NO
                // command, receipt or persisted message in the player's save.
                fixture.Mailbox.Receive(new EldoriaV41Message {
                    EventId="VIGILIA_NATIVE_MAIL_QA_NEVER_PERSIST",
                    Category="informes",
                    Type="system",
                    Title="Informe de prueba visual (no real)",
                    Subtitle="Registro temporal de prueba para verificar "+
                        "la distribución, el contraste y la lectura del buzón.",
                    OccurredAtUtcTicks=new DateTime(2026,11,10,10,0,0,
                        DateTimeKind.Utc).Ticks
                });
                stateField.SetValue(gateway,fixture);
                host.City.Apply(fixture);
                var canonicalMailbox=host.Hud.GetComponent<RoyalGoldV41MailboxScreen>();
                Need(canonicalMailbox && mail.IsShowing,
                    "Populated in-memory fixture lost the actual mailbox UI");
                canonicalMailbox.Refresh();
                mail.SynchronizeVisual();
                Need(canonicalMailbox.VisibleMessageRows>0 &&
                     mail.ReplacedOldPainting,
                    "A real category message still has obsolete art or no live row");
                foreach(var size in new[]{(900,1600),(390,844)})
                    mailContentShots.Add(Capture(ui,folder,"mail-with-report-row",
                        size.Item1,size.Item2,
                        ()=>mail.IsShowing &&
                            canonicalMailbox.VisibleMessageRows>0 &&
                            mail.ReplacedOldPainting));
                // The canonical controller is attached to the DISABLED old
                // HUD but its actual window has been reparented to the
                // independent Vigilia Canvas. Searching controller children
                // falsely reports no row even when native captures show one.
                // Assert on the REAL, VISIBLE canvas hierarchy instead.
                var firstRow=ui.GetComponentsInChildren<Button>(true)
                    .FirstOrDefault(x=>x && x.name=="Buzón / mensaje 0");
                Need(firstRow && firstRow.gameObject.activeInHierarchy &&
                     firstRow.interactable,
                    "First actual report row is not clickable");
                ClickActualMailControl(ui,firstRow);
                canonicalMailbox.Refresh();
                mail.SynchronizeVisual();
                var actualDocument=ui
                    .GetComponentsInChildren<RectTransform>(true)
                    .FirstOrDefault(x=>x && x.name=="Mailbox / report document");
                Need(actualDocument && actualDocument.gameObject.activeInHierarchy,
                    "Real report content was not opened by its own canonical button");
                foreach(var size in new[]{(900,1600),(390,844)})
                    mailContentShots.Add(Capture(ui,folder,"mail-report-document",
                        size.Item1,size.Item2,
                        ()=>mail.IsShowing &&
                            actualDocument.gameObject.activeInHierarchy &&
                            mail.ReplacedOldPainting));
                mail.Close();
                Need(!mail.IsShowing,"Mailbox exit failed");
                // Previously unverified canonical Vigilantes/Exploradores
                // cannot remain invisible under the disabled original Canvas.
                // Preserve actual saved progression, background, reports,
                // encounter decisions and hotkeys. Test native UI only.
                // These real destinations unlock at Bastion III in the current
                // campaign. Advance only the disposable in-memory QA fixture;
                // the production gate and original player save stay intact.
                fixture.BastionLevel=3;
                // Produce the disposable orders through the real UTC domain
                // rule; an empty board cannot certify readable live contracts.
                EarlyRetentionRules.Synchronize(fixture,DateTime.UtcNow.Ticks);
                stateField.SetValue(gateway,fixture);
                Need(EarlyRetentionRules.VigilantsUnlocked(gateway.Snapshot()) &&
                     EarlyRetentionRules.ScoutsUnlocked(gateway.Snapshot()),
                    "Retention QA fixture does not satisfy real progression gates");
                var retention=ui.RealRetention;
                Need(retention!=null && retention.TryOpen(
                    RoyalGoldV3EarlyRetentionScreens.Destination.Vigilants) &&
                    retention.ReplacedOldChrome,
                    "Real unlocked Vigilants still opens old HUD chrome");
                CaptureMany(ui,folder,"vigilants",retentionShots,
                    ()=>retention.IsShowing && retention.ReplacedOldChrome && retention.RestoredPlaneCount==7);
                retention.Close();
                Need(!retention.IsShowing,
                    "Vigilants did not return to new city Canvas");
                Need(retention.TryOpen(
                    RoyalGoldV3EarlyRetentionScreens.Destination.Scouts) &&
                    retention.ReplacedOldChrome,
                    "Real unlocked Scouts still opens old HUD chrome");
                // Assert the recovered authored composition, even in production.
                // A disabled canonical painter is not evidence of visible artwork.
                Need(retention.HasSingleRestoredScoutsPainting && retention.RestoredPlaneCount==1,
                    "Scouts lost the actual approved illustration on Vigilia");
                AuditExclusivePresentation();
                Need(fixture.Retention.Orders.Any(order=>!order.Claimed),
                    "Scout destination evidence requires real generated orders");
                var ink=retention.LiveScreenRect.GetComponentsInChildren<Text>(true)
                    .Where(label=>label.name=="Readable contract heading" ||
                        label.name=="Real scout order reward and status").ToArray();
                Need(ink.Length==6 && ink.All(label=>label.color.grayscale<.35f),
                    "Scout contract text must remain readable on recovered light paper");
                var paper=retention.LiveScreenRect.GetComponentsInChildren<Image>(true)
                    .Where(image=>image && image.name=="Vigilia / recovered blank cartographer paper").ToArray();
                Need(paper.Length==3 && paper.All(image=>image.isActiveAndEnabled &&
                    image.sprite && image.sprite.texture.name=="scout-parchment-v3" && !image.raycastTarget),
                    "Scouts require three visible live paper contracts, without intercepting input");
                var oldPaper=retention.LiveScreenRect.GetComponentsInChildren<Image>(true)
                    .Where(image=>image && image.name=="Reference parchment / visual only").ToArray();
                Need(oldPaper.All(image=>!image.enabled),"Retired Scout material renderer was reactivated");
                CaptureMany(ui,folder,"scouts",retentionShots,
                    ()=>retention.IsShowing && retention.ReplacedOldChrome &&
                        retention.HasSingleRestoredScoutsPainting);
                retention.Close();
                Need(!retention.IsShowing,"Scouts did not return to new city Canvas");
                ui.SelectBuilding("");
                var treasures=Object.FindObjectsByType<Eldoria.DailyTreasures.DailyTreasurePanel>(
                    FindObjectsInactive.Include,FindObjectsSortMode.None);
                Need(treasures.Length==1,"Combined session must have one authoritative treasure panel");
                var treasure=treasures[0];
                treasure.ProbePresentationCanvasForNativeEvidence();
                treasure.Suppressed=false;
                Need(treasure.UsesVigiliaCanvas && treasure.transform.IsChildOf(ui.transform),
                    "Treasure entry stranded on the retired Canvas");
                var treasureEntry=treasure.GetComponentsInChildren<Button>(true)
                    .Single(x=>x.name=="Daily reward / optional medallion");
                ClickActualMailControl(ui,treasureEntry,false);
                Need(treasure.IsOpen,"Real treasure medallion failed to open the calendar");
                CaptureMany(ui,folder,"treasures",treasureShots,
                    ()=>treasure.IsOpen && treasure.UsesVigiliaCanvas);
                var treasureClose=treasure.GetComponentsInChildren<Button>(true)
                    .Single(x=>x.name=="Close tributes");
                ClickActualMailControl(ui,treasureClose,false);
                Need(!treasure.IsOpen,"Real treasure close did not return to new HUD");
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
            Need(mailContentShots.Count==4,
                "Expected two real-populated category and report images each");
            var report=new Report{
                status="VIGILIA_DESTINATIONS_CAPTURED_ART_REVIEW_PENDING",
                source=Environment.GetEnvironmentVariable("GITHUB_SHA")??"local",
                notes="Four real authoritative destinations in test-only injected advanced state. Native phone/desktop images; no player save mutation, no real purchase and NO visual approval.",
                probeOnly=true,saveUnchanged=true,artworkApproved=false,
                screenshotCount=shots.Count,screens=shots.ToArray()
            };
            File.WriteAllText(Path.Combine(folder,"BreachWatchDestinationEvidence.json"),
                JsonUtility.ToJson(report,true));
            Need(treasureShots.Count==4,"Four combined treasure calendar views required");
            File.WriteAllText(Path.Combine(folder,"BreachWatchIntegratedTreasureEvidence.json"),
                JsonUtility.ToJson(new Report {
                    source=report.source,status="COMBINED_TREASURE_ENTRY_CALENDAR_TWO_TOUCHES_PASS",
                    notes="Actual single live treasure panel on Vigilia: real entry/close touches, four viewports, production font audit. Claims are verified by the separate native gateway fixture.",
                    probeOnly=true,saveUnchanged=true,artworkApproved=false,
                    screenshotCount=treasureShots.Count,screens=treasureShots.ToArray()
                },true));
            var mailReport=new Report {
                status="VIGILIA_REAL_POPULATED_MAIL_REPORT_ART_REVIEW_PENDING",
                source=report.source,
                notes="Only in-memory, explicitly labelled synthetic report for canonical message list/report UI QA. No player messages or save were modified.",
                probeOnly=true,saveUnchanged=true,artworkApproved=false,
                actualMailboxFirstHitClicks=mailboxFirstHitClicks,
                screenshotCount=mailContentShots.Count,
                screens=mailContentShots.ToArray()
            };
            File.WriteAllText(Path.Combine(folder,
                "BreachWatchPopulatedMailEvidence.json"),
                JsonUtility.ToJson(mailReport,true));
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

        static float PaintedBandCoverage(Texture2D screenshot,float from,float to)
        {
            // Independent IMAGE-SPACE guard: a blue legacy screen could be
            // disabled and a new RectTransform could fit its viewport while
            // the rendered result still contained enormous black bands.
            // Check actual baked pixels from the genuine Unity GPU, not
            // layout source or an image model's subjective interpretation.
            if(!screenshot)return 0f;
            int good=0,total=0;
            int w=screenshot.width,h=screenshot.height;
            for(int y=Mathf.FloorToInt(h*from);y<Mathf.CeilToInt(h*to);
                y+=Mathf.Max(1,h/80))
                for(int x=Mathf.FloorToInt(w*.08f);x<Mathf.CeilToInt(w*.92f);
                    x+=Mathf.Max(1,w/60))
                {
                    var c=screenshot.GetPixel(x,y);
                    if(c.r+c.g+c.b>.12f)good++;
                    total++;
                }
            return total>0?good/(float)total:0f;
        }

        static void ClickActualMailControl(BreachWatchHud ui,Button button,bool countMail=true)
        {
            Need(button && button.isActiveAndEnabled && button.IsInteractable(),
                "Actual canonical mail control is unavailable");
            Need(button.targetGraphic && button.targetGraphic.raycastTarget,
                "Actual mail control lost its configured pointer input after activation: "+button.name);
            var camera=host.PlayerCamera;Need(camera,"Canonical input camera missing");
            var priorTarget=camera.targetTexture;var priorAspect=camera.aspect;
            int priorMask=camera.cullingMask;
            var target=new RenderTexture(390,844,24,RenderTextureFormat.ARGB32);
            target.Create();
            try
            {
                camera.targetTexture=target;camera.aspect=390f/844f;camera.cullingMask|=1<<5;
                ui.NativeCanvas.worldCamera=camera;ui.Reflow(390,844);ui.RenderNow();
                var management=host.Hud.GetComponent<RoyalGoldV3ManagementScreens>();
                if(management && management.IsOpen)management.SynchronizeLayout();
                if(ui.RealDestinations && ui.RealDestinations.IsShowing)
                    ui.RealDestinations.SynchronizeVisual();
                var canonical=host.Hud.GetComponent<RoyalGoldV41MailboxScreen>();
                if(canonical && canonical.IsOpen)canonical.SynchronizeLayout();
                if(ui.RealMailbox && ui.RealMailbox.IsShowing)ui.RealMailbox.SynchronizeVisual();
                var treasureControl=button.GetComponentInParent<Eldoria.DailyTreasures.DailyTreasurePanel>();
                if(treasureControl)treasureControl.ProbeOrientationLayoutForNativeEvidence();
                ui.GetComponent<BreachWatchTypography>()?.SynchronizeNow();
                Canvas.ForceUpdateCanvases();AuditExclusivePresentation();camera.Render();Canvas.ForceUpdateCanvases();AuditExclusivePresentation();camera.Render();
                var system=EventSystem.current;
                Need(system && system.isActiveAndEnabled,"Real mail EventSystem unavailable");
                var rect=(RectTransform)button.transform;
                var data=new PointerEventData(system) { position=
                    RectTransformUtility.WorldToScreenPoint(camera,rect.TransformPoint(rect.rect.center)) };
                var hits=new List<RaycastResult>();system.RaycastAll(data,hits);
                Need(hits.Count>0 && ExecuteEvents.GetEventHandler<IPointerClickHandler>(
                    hits[0].gameObject)==button.gameObject,
                    "Actual mail first pointer hit is not its control: "+button.name+
                    "; first="+(hits.Count>0?hits[0].gameObject.name:"none"));
                var handler=ExecuteEvents.GetEventHandler<IPointerClickHandler>(hits[0].gameObject);
                Need(ExecuteEvents.Execute<IPointerClickHandler>(handler,data,
                    ExecuteEvents.pointerClickHandler),"Actual mail first hit has no click handler");
                if(countMail)mailboxFirstHitClicks++;
                Debug.Log("VIGILIA_ACTUAL_MAIL_FIRST_HIT control="+button.name+
                    "; hit="+hits[0].gameObject.name+"; handler="+handler.name);
            }
            finally
            {
                camera.targetTexture=priorTarget;camera.aspect=priorAspect;camera.cullingMask=priorMask;
                target.Release();Object.DestroyImmediate(target);
            }
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
            string retentionOverflow=null;
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
                // Editor camera.Render does not execute the normal LateUpdate
                // font pass. Measure the same imported fonts as the real player.
                ui.GetComponent<BreachWatchTypography>()?.SynchronizeNow();
                if(name=="march")
                {
                    Need(ui.RealDestinations &&
                         ui.RealDestinations.RealMarchThreeSpaceRoster,
                         "Actual march lacks three reserved hero spaces or "+
                         "the original selectable source-bound choices at "+
                         width+"x"+height);
                    var marchRoot=management ? management.VisibleRoot:null;
                    Need(marchRoot,"Real canonical March layout missing");
                    if(height>width && height>=700)
                        Need(marchRoot.GetComponent<BreachWatchMarchComposition>().CompactPortraitApplied,
                            "Owner-requested compact portrait military sheet not actually applied");

                    var composition=marchRoot.GetComponent<BreachWatchMarchComposition>();
                    Need(composition && composition.SecondaryDetailsBehindInformation && !composition.SecondaryDetailsOpen,
                        "Secondary army information saturates the initial preparation screen");
                    var realHeroSlots=marchRoot.
                        GetComponentsInChildren<RectTransform>(true)
                        .Where(x=>x && x.name.StartsWith("Hero slot "))
                        .OrderBy(x=>x.name).ToArray();
                    Need(realHeroSlots.Length==3 &&
                         realHeroSlots[0].gameObject.activeInHierarchy &&
                         realHeroSlots[1].gameObject.activeInHierarchy &&
                         realHeroSlots[2].gameObject.activeInHierarchy,
                         "March is missing its reserved third hero space or "+
                         "missing selectable real hero cards at "+
                         width+"x"+height);
                    Canvas.ForceUpdateCanvases();
                    foreach(var heroSlot in realHeroSlots)
                    {
                        var caption=heroSlot.GetComponentsInChildren<Text>(true)
                            .FirstOrDefault(x=>x.name=="Button label");
                        Need(caption && caption.preferredHeight<=caption.rectTransform.rect.height+1f,
                            "Actual hero caption exceeds its name band at "+width+"x"+height);
                    }
                    var edit= marchRoot.GetComponentsInChildren<RectTransform>(true)
                        .FirstOrDefault(x=>x.name=="Edit march by percent");
                    var editText=edit ? edit.GetComponentInChildren<Text>(true):null;
                    Need(editText && editText.preferredHeight<=editText.rectTransform.rect.height+1f,
                        "Actual quantity caption exceeds its button at "+width+"x"+height+
                        (editText ? " font="+editText.fontSize+" rect="+editText.rectTransform.rect+
                            " preferred="+editText.preferredWidth+"x"+editText.preferredHeight:" missing"));
                }
                if(name=="training" && height>width)
                    Need(ui.RealDestinations && 
                         ui.RealDestinations.RealTrainingCinematic,
                         "Full archer composition did not render in portrait "+
                         width+"x"+height+"; reject decorative-only PASS. "+
                         ui.RealDestinations.NativeTrainingLayoutDiagnostic);
                if(name=="vigilants" || name=="scouts")
                {
                    var retention=ui.RealRetention;
                    Need(retention && retention.IsShowing &&
                         retention.OwnsActualSafeViewport &&
                         retention.LiveScreenRect,
                         "Real retention screen is still laid out on hidden RoyalHUD");
                    retention.SynchronizeVisual();
                    var rect=retention.LiveScreenRect.rect;
                    float desiredAspect=width/(float)height;
                    float actualAspect=rect.width/Mathf.Max(1f,rect.height);
                    Need(Mathf.Abs(actualAspect-desiredAspect)<.13f,
                         "Retention viewport incorrectly reports "+
                         rect.width+"x"+rect.height+
                         " for render target "+width+"x"+height);
                    var manuscript=retention.LiveScreenRect.
                        GetComponentsInChildren<RectTransform>(true).
                        FirstOrDefault(x=>x.name=="Independent royal manuscript room");
                    Need(manuscript,
                         "Canonical retention scenic manuscript is missing");
                    // Only the authored BACKGROUND paintings deliberately
                    // overscan off-camera (mountain/cliff parallax). That is
                    // correct: cropping the scenery to the room rectangle
                    // destroys the panoramic look. Measure the actual
                    // manuscript frame and REAL clickable buttons instead.
                    // Native pixel-band coverage below independently catches
                    // empty black letterboxing even when rects appear valid.
                    var panelCorners=new Vector3[4];
                    manuscript.GetWorldCorners(panelCorners);
                    float minX=float.MaxValue,maxX=float.MinValue;
                    float minY=float.MaxValue,maxY=float.MinValue;
                    foreach(var corner in panelCorners)
                    {
                        var local=retention.LiveScreenRect.
                            InverseTransformPoint(corner);
                        minX=Mathf.Min(minX,local.x);
                        maxX=Mathf.Max(maxX,local.x);
                        minY=Mathf.Min(minY,local.y);
                        maxY=Mathf.Max(maxY,local.y);
                    }
                    var actual=retention.LiveScreenRect.rect;
                    var offenders=new List<string>();
                    if(minX<actual.xMin-4f||maxX>actual.xMax+4f||
                       minY<actual.yMin-4f||maxY>actual.yMax+4f)
                        offenders.Add("main manuscript frame "+
                            minX.ToString("F0")+".."+maxX.ToString("F0")+
                            " y="+minY.ToString("F0")+".."+maxY.ToString("F0"));
                    foreach(var button in retention.LiveScreenRect.
                        GetComponentsInChildren<Button>(true))
                    {
                        if(!button || !button.isActiveAndEnabled ||
                           !button.gameObject.activeInHierarchy ||
                           !button.interactable)
                            continue;
                        var rectButton=button.transform as RectTransform;
                        if(!rectButton)continue;
                        var corners=new Vector3[4];
                        rectButton.GetWorldCorners(corners);
                        foreach(var corner in corners)
                        {
                            var p=retention.LiveScreenRect.
                                InverseTransformPoint(corner);
                            if(p.x<actual.xMin-7f||p.x>actual.xMax+7f||
                               p.y<actual.yMin-7f||p.y>actual.yMax+7f)
                            {
                                offenders.Add("real button "+button.name);
                                break;
                            }
                        }
                    }
                    if(offenders.Count>0)
                        retentionOverflow="Real retention UI is offscreen: "+
                            string.Join("; ",offenders)+
                            " / viewport="+actual.width+"x"+actual.height;
                }
                var mailbox=host.Hud.GetComponent<RoyalGoldV41MailboxScreen>();
                if(mailbox && mailbox.IsOpen)mailbox.SynchronizeLayout();
                if(ui.RealMailbox && ui.RealMailbox.IsShowing)
                    ui.RealMailbox.SynchronizeVisual();
                var heroes=ui.RealHeroes;
                if(heroes && heroes.IsShowing)
                {
                    host.Hud.GetComponent<Eldoria.HeroesUI.HeroesApprovedDestination>().Refresh();
                    heroes.SynchronizeVisual();
                }
                if(name=="treasures")
                {
                    var treasure=ui.GetComponentInChildren<Eldoria.DailyTreasures.DailyTreasurePanel>(true);
                    Need(treasure && treasure.IsOpen,"Combined treasure disappeared during reflow");
                    treasure.ProbeOrientationLayoutForNativeEvidence();
                    ui.GetComponent<BreachWatchTypography>()?.SynchronizeNow();
                }
                ui.GetComponent<BreachWatchTypography>()?.SynchronizeNow();
                Canvas.ForceUpdateCanvases();
                if(name=="heroes")
                    Need(!ui.GetComponentsInChildren<Button>(true).Any(x=>
                         x && x.name=="Hall / nav reliquary"),
                         "Inert, unimplemented Relicario UI still exposed in Hero salon");
                if(name=="heroes")
                    Need(heroes && heroes.HallGradeActive,
                        "Original blue Heroes Hall is not warmed by new scene material: "+
                        (heroes?heroes.HallGradeDiagnostic:"heroes bridge null"));
                if(name=="heroes" && width==1600 && height==900)
                    Need(heroes && heroes.DesktopHallIntroductionUnobstructed,
                        "Hall title/intro must stay above four real category tabs");
                Need(verify(),"Real destination visual replacement regressed: "+name);
                AuditExclusivePresentation();camera.Render();Canvas.ForceUpdateCanvases();AuditExclusivePresentation();camera.Render();
                RenderTexture.active=rt;
                texture.ReadPixels(new Rect(0,0,width,height),0,0);
                texture.Apply();
                string full=Path.Combine(folder,file);
                File.WriteAllBytes(full,texture.EncodeToPNG());
                if(name=="vigilants" || name=="scouts")
                {
                    var realRoom=ui.RealRetention.LiveScreenRect;
                    var backCaption=realRoom.GetComponentsInChildren<Text>(true)
                        .Single(label=>label.transform.parent.name=="Return to actual kingdom");
                    var footerCaption=realRoom.GetComponentsInChildren<Text>(true)
                        .Single(label=>label.name=="No artificial notifications");
                    foreach(var caption in new[]{backCaption,footerCaption})
                        Need(caption.cachedTextGenerator.vertexCount>=4 &&
                             caption.preferredHeight<=caption.rectTransform.rect.height+2f,
                            "Retention caption clipped or invisible: "+caption.name+
                            " required="+caption.preferredHeight+" box="+caption.rectTransform.rect.size+
                            " screenshot="+file);
                }
                if(name=="march")
                    VerifyMarchCaptionInk(management.VisibleRoot,camera,rt,texture,folder,width,height);
                Need(retentionOverflow==null,
                     retentionOverflow+" / actual native screenshot="+file);
                if((name=="vigilants"||name=="scouts") && height>width)
                {
                    float top=PaintedBandCoverage(texture,.80f,.92f);
                    float bottom=PaintedBandCoverage(texture,.08f,.20f);
                    Need(top>.30f && bottom>.30f,
                        "True Unity screenshot still shows giant black bars: "+
                        name+" "+width+"x"+height+
                        " top="+top.ToString("F2")+
                        " bottom="+bottom.ToString("F2")+
                        " saved="+file);
                }
                // Previous native screenshot showed actual legible glyphs
                // above/behind the category plaques even though a purely
                // logical RectTransform check declared PASS. Render the
                // SAME real Hall with its introduction disabled for ONE
                // diagnostic frame. Count ONLY glyph pixels whose removal
                // changes GPU output; a correct layout has no such pixels
                // in the 14px safety gutter just above the tab artwork.
                bool heroIntroGpuClear=false;
                int heroIntroVisiblePixels=0,heroIntroOverlapPixels=0;
                if(name=="heroes" && width==1600 && height==900)
                {
                    var intro=ui.GetComponentsInChildren<Text>(true)
                        .FirstOrDefault(x=>x && x.name=="Heroes hall introduction" &&
                            x.isActiveAndEnabled && x.gameObject.activeInHierarchy);
                    var tab=ui.GetComponentsInChildren<RectTransform>(true)
                        .FirstOrDefault(x=>x && x.name=="Hall / category TODOS" &&
                            x.gameObject.activeInHierarchy);
                    Need(intro && tab,
                        "Native Hall title/band/category controls must be real and visible");
                    var tc=new Vector3[4];
                    tab.GetWorldCorners(tc);
                    // Viewport coordinates are relative to the ACTUAL native
                    // render target, not editor monitor resolution.
                    float tabTop=camera.WorldToViewportPoint(tc[1]).y*height;
                    var ic=new Vector3[4];
                    intro.rectTransform.GetWorldCorners(ic);
                    int startX=Mathf.Clamp(Mathf.FloorToInt(
                        camera.WorldToViewportPoint(ic[0]).x*width),0,width-1);
                    int endX=Mathf.Clamp(Mathf.CeilToInt(
                        camera.WorldToViewportPoint(ic[2]).x*width),startX+1,width);
                    int lowY=Mathf.Clamp(Mathf.FloorToInt(tabTop-3f),0,height-1);
                    int highY=Mathf.Clamp(Mathf.CeilToInt(tabTop+110f),lowY+1,height);
                    var withIntro=texture.GetPixels32();
                    var withoutIntro=new Texture2D(width,height,TextureFormat.RGB24,false);
                    try
                    {
                        intro.enabled=false;
                        Canvas.ForceUpdateCanvases();AuditExclusivePresentation();camera.Render();
                        RenderTexture.active=rt;
                        withoutIntro.ReadPixels(new Rect(0,0,width,height),0,0);
                        withoutIntro.Apply();
                        var hidden=withoutIntro.GetPixels32();
                        for(int y=lowY;y<highY;y++)
                        for(int x=startX;x<endX;x++)
                        {
                            int i=y*width+x;
                            var a=withIntro[i];var b=hidden[i];
                            int diff=Mathf.Abs(a.r-b.r)+Mathf.Abs(a.g-b.g)+
                                Mathf.Abs(a.b-b.b);
                            if(diff<=80)continue; // ignore animated torch lighting
                            heroIntroVisiblePixels++;
                            if(y<=tabTop+14f)
                                heroIntroOverlapPixels++;
                        }
                        Need(heroIntroVisiblePixels>90,
                            "Hall intro was not visibly rasterized in the native GPU test");
                        Need(heroIntroOverlapPixels<=6,
                            "Actual Hall introduction still touches/overlaps category art: "+
                            heroIntroOverlapPixels+" glyph pixels inside the 14px gutter");
                        heroIntroGpuClear=true;
                    }
                    finally
                    {
                        intro.enabled=true;
                        Canvas.ForceUpdateCanvases();AuditExclusivePresentation();camera.Render();
                        Object.DestroyImmediate(withoutIntro);
                    }
                }
                bool authoredMailTextureVerified=false;
                int authoredMailChangedPixels=0;
                int authoredMailUpperChangedPixels=0;
                // Desktop 1600x900 must now show the same genuine painted
                // parchment as portrait, not the prior 70%-empty dark wall.
                // Keep very short 844x390 landscape unpainted for usability.
                bool spaciousMailLandscape=width>=1100 && height>=650 &&
                    width>height;
                if(name=="mail" && (height>width || spaciousMailLandscape))
                {
                    var art=ui.GetComponentsInChildren<BreachWatchMailboxEmptyArtwork>(true)
                        .FirstOrDefault(x=>x && x.gameObject.activeInHierarchy);
                    Need(art && art.isActiveAndEnabled && !art.raycastTarget,
                        "Authored empty mail illustration is missing or intercepts input");
                    var expected=Resources.Load<Sprite>("VigiliaArt/MailboxArtwork/sealed_letter");
                    Need(expected && art.mainTexture==expected.texture &&
                         expected.texture.width<=512 && expected.texture.height<=512,
                        "Empty mail did not bind its actual bounded authored texture");
                    var originalPixels=texture.GetPixels32();
                    var withoutArt=new Texture2D(width,height,TextureFormat.RGB24,false);
                    try
                    {
                        art.enabled=false;
                        Canvas.ForceUpdateCanvases();AuditExclusivePresentation();camera.Render();
                        RenderTexture.active=rt;
                        withoutArt.ReadPixels(new Rect(0,0,width,height),0,0);
                        withoutArt.Apply();
                        var hiddenPixels=withoutArt.GetPixels32();
                        for(int pixel=0;pixel<originalPixels.Length;pixel++)
                        {
                            var a=originalPixels[pixel];var b=hiddenPixels[pixel];
                            if(Mathf.Abs(a.r-b.r)>12 || Mathf.Abs(a.g-b.g)>12 ||
                               Mathf.Abs(a.b-b.b)>12)
                            {
                                authoredMailChangedPixels++;
                                // Unity framebuffer is bottom-origin.
                                // Upper mail image MUST have GPU coverage.
                                if(pixel/width>=height*.51f)
                                    authoredMailUpperChangedPixels++;
                            }
                        }
                        Need(authoredMailChangedPixels>width*height*.01f,
                            "Authored mail imported but not genuinely visible in GPU output");
                        Need(authoredMailUpperChangedPixels>width*height*.0005f,
                            "Mail art still clipped behind the existing collection board "+
                            "at "+width+"x"+height);
                        authoredMailTextureVerified=true;
                    }
                    finally
                    {
                        art.enabled=true;Canvas.ForceUpdateCanvases();AuditExclusivePresentation();camera.Render();
                        Object.DestroyImmediate(withoutArt);
                    }
                }
                if(name=="mail-with-report-row" || name=="mail-report-document")
                    Need(!ui.GetComponentsInChildren<BreachWatchMailboxEmptyArtwork>(true)
                        .Any(x=>x && x.gameObject.activeInHierarchy),
                        "Empty mailbox artwork is covering populated correspondence");
                int buttons=ui.GetComponentsInChildren<Button>(true)
                    .Count(b=>b && b.IsActive() && b.interactable);
                Need(buttons>0,"Real destination has no interactive controls: "+name);
                // Existing baseline GPU subtraction must finish before an
                // A/B render can rebuild dynamic font atlases or advance effects.
                if(name=="training"||name=="march"||name=="heroes"||
                   name.StartsWith("hero-")||name.Contains("mail")||name=="treasures"||
                   name=="vigilants"||name=="scouts")
                    BreachWatchTypographyEvidence.Capture(ui.NativeCanvas,camera,rt,full,width,height);
                return new Screen{
                    destination=name,file=file,sha256=Hash(full),width=width,height=height,
                    oldCanvasOff=ui.LegacyPresentationHidden,
                    newCanvasOwnsScreen=verify(),interactiveButtons=buttons,
                    authoredMailTextureVerified=authoredMailTextureVerified,
                    heroIntroGpuClear=heroIntroGpuClear,
                    heroIntroVisiblePixels=heroIntroVisiblePixels,
                    heroIntroOverlapPixels=heroIntroOverlapPixels,
                    authoredMailChangedPixels=authoredMailChangedPixels,
                    authoredMailUpperChangedPixels=authoredMailUpperChangedPixels
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
