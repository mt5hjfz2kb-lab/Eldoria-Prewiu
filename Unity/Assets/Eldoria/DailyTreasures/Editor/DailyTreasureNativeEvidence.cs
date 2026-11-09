#if UNITY_EDITOR
using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Security.Cryptography;
using Eldoria.Application;
using Eldoria.Domain;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Object=UnityEngine.Object;

namespace Eldoria.DailyTreasures.Editor
{
    /// <summary>
    /// Real Unity-rendered isolated component evidence. No canonical scene, save,
    /// old HUD or user data is touched. This deliberately does NOT certify the
    /// final city integration, motion, sound, iOS, or artistic approval.
    /// </summary>
    public static class DailyTreasureNativeEvidence
    {
        const string Folder="DailyTreasureNativeEvidence";
        sealed class SimulatedClock : IClock { public long UtcTicks { get; set; } }
        sealed class TransientStore : IStateStore
        {
            public PlayerState State;
            public PlayerState Load()=>State;
            public void Save(PlayerState state){State=state;}
        }
        [Serializable] sealed class Shot
        {
            public string file,sha256,view;
            public int width,height,bytes;
            public bool nativeCanvas,coversReward,readyToClaim,preserved;
        }
        [Serializable] sealed class Report
        {
            public string status,source,note;
            public int images,realGatewayClaims,graphicalPointerHits;
            // These are artistic style probes only; they do not imply an
            // actual earned milestone, a claim or any gateway battle.
            public int visualOnlyMilestoneCaptures;
            public bool artApproved,fullSceneVerified,iosVerified,authoredSpritesImported;
            public Shot[] shots;
        }
        static void Need(bool ok,string reason)
        {
            if(!ok)throw new InvalidOperationException(reason);
        }
        // Source integrity checks run on Ubuntu. This second gate validates
        // what Unity actually imported, not merely the nine .meta texts.
        // Native art is still UNAPPROVED until the resulting images are seen.
        static void RequireNineImportedSprites()
        {
            var expected=new[]{
                (name:"treasure_closed_512",width:512,height:512,border:0),
                (name:"treasure_opened_512",width:512,height:512,border:0),
                (name:"valoria_frame_main_512",width:512,height:512,border:46),
                (name:"valoria_frame_card_256",width:256,height:256,border:31),
                (name:"valoria_frame_button_512x128",width:512,height:128,border:24),
                (name:"resource_wood",width:128,height:128,border:0),
                (name:"resource_stone",width:128,height:128,border:0),
                (name:"resource_hourglass",width:128,height:128,border:0),
                (name:"resource_food",width:128,height:128,border:0)
            };
            foreach(var item in expected)
            {
                var sprite=Resources.Load<Sprite>("DailyTreasureArt/"+item.name);
                Need(sprite!=null,"Unity did not import the authored Sprite: "+item.name);
                Need(Mathf.Abs(sprite.rect.width-item.width)<.01f &&
                     Mathf.Abs(sprite.rect.height-item.height)<.01f,
                     "Unity imported wrong Sprite dimensions: "+item.name);
                var importer=AssetImporter.GetAtPath(AssetDatabase.GetAssetPath(sprite))
                    as TextureImporter;
                Need(importer!=null &&
                     importer.textureType==TextureImporterType.Sprite &&
                     importer.spriteImportMode==SpriteImportMode.Single &&
                     importer.alphaIsTransparency && !importer.mipmapEnabled,
                     "Actual Unity Sprite import settings invalid: "+item.name);
                var border=sprite.border;
                Need(Mathf.Abs(border.x-item.border)<.01f &&
                     Mathf.Abs(border.y-item.border)<.01f &&
                     Mathf.Abs(border.z-item.border)<.01f &&
                     Mathf.Abs(border.w-item.border)<.01f,
                     "Actual Unity Sprite nine-slice borders invalid: "+item.name);
            }
        }

        // The rehearsal deliberately has legacy + Vigilia canvases while
        // migrating, but must never leave two enabled or add an EventSystem.
        static void VerifyUiOwnership(int canvasCount,EventSystem eventSystem)
        {
            var canvases=Object.FindObjectsByType<Canvas>(
                FindObjectsInactive.Include,FindObjectsSortMode.None);
            var events=Object.FindObjectsByType<EventSystem>(
                FindObjectsInactive.Include,FindObjectsSortMode.None);
            Need(canvases.Length==canvasCount &&
                 canvases.Count(c=>c.isActiveAndEnabled)==1 &&
                 events.Length==1 && events[0]==eventSystem,
                 "Duplicate enabled Canvas or EventSystem in treasury native proof");
        }
        static string Sha(string file)
        {
            using(var hash=SHA256.Create())
            using(var data=File.OpenRead(file))
                return BitConverter.ToString(hash.ComputeHash(data)).Replace("-","").ToLowerInvariant();
        }
        static long Day(int day)
            =>new DateTime(2026,11,day,12,0,0,DateTimeKind.Utc).Ticks;
        static Button FindButton(Component component,string name)
            =>component.GetComponentsInChildren<Button>(true)
                .Single(x=>x.name==name);
        static RectTransform FindRect(Component component,string name)
            =>component.GetComponentsInChildren<RectTransform>(true)
                .Single(x=>x.name==name);

        static void VerifyGenuineSceneComposition(
            DailyTreasurePanel panel,string eventId,int width,int height)
        {
            var expected=DailyTreasureStageDirection.Compose(eventId,width,height);
            Need(panel.ActiveMilestoneStage==expected.Kind,
                "Real event is presented with the wrong category art/layout");
            var painted=FindRect(panel,
                "Genuine Eldoria milestone scenery / visual only");
            var chest=FindRect(panel,"Authored chest / opened");
            var receipt=FindRect(panel,"Confirmed received items");
            var banner=FindRect(panel,
                "Milestone heraldic event banner / visual only");
            Need(banner.gameObject.activeInHierarchy,
                "Original event-specific heraldic banner is hidden");
            Need(Mathf.Abs(painted.anchoredPosition.x-expected.SceneX)<1.1f &&
                 Mathf.Abs(painted.anchoredPosition.y-expected.SceneY)<1.1f &&
                 Mathf.Abs(painted.rect.width-expected.SceneW)<1.1f &&
                 Mathf.Abs(painted.rect.height-expected.SceneH)<1.1f,
                 "Wrong native painterly scene size/position for "+
                 expected.Kind);
            Need(Mathf.Abs(chest.anchoredPosition.x-expected.ChestX)<1.1f &&
                 Mathf.Abs(chest.anchoredPosition.y-expected.ChestY)<1.1f &&
                 Mathf.Abs(chest.rect.width-expected.ChestW)<1.1f &&
                 Mathf.Abs(chest.rect.height-expected.ChestH)<1.1f,
                 "Original premium coffer lost event-specific composition for "+
                 expected.Kind);
            Need(Mathf.Abs(receipt.anchoredPosition.x-expected.ReceiptX)<1.1f &&
                 Mathf.Abs(receipt.anchoredPosition.y-expected.ReceiptY)<1.1f &&
                 Mathf.Abs(receipt.rect.width-expected.ReceiptW)<1.1f &&
                 Mathf.Abs(receipt.rect.height-expected.ReceiptH)<1.1f,
                 "Actual event report is not placed legibly below chest");
            var screen=FindRect(panel,"Tributo de los siete días / modal");
            var backdrop=screen.GetComponent<Image>();
            Need(backdrop &&
                 Mathf.Abs(backdrop.color.a-expected.BackdropOpacity)<.03f,
                "The Bastion city / event stage is too opaque or too faint");
            if(expected.SideBySide)
                Need(painted.anchoredPosition.x+painted.rect.width*.5f+7f<=
                     chest.anchoredPosition.x-chest.rect.width*.5f,
                     "Real native landscape art is obscured by the reward chest");
            else if(expected.Kind==DailyTreasureStageKind.Bastion)
                Need(!panel.HasMilestoneScenery,
                    "Do not disguise a Bastion level by painting a fake city");
            else
                Need(panel.HasMilestoneScenery,
                    "Original real Eldoria artwork does not appear behind chest");
            var ink=FindRect(panel,"Bastion / real-city milestone caption ink");
            var plate=FindRect(panel,
                "Basalt and worn bronze reward parchment").GetComponent<Image>();
            if(expected.Kind==DailyTreasureStageKind.Bastion)
            {
                Need(ink.gameObject.activeInHierarchy && plate &&
                     Mathf.Abs(plate.color.a-
                         DailyTreasureStageDirection.BastionPlateOpacity)<.015f,
                     "Bastion real city is concealed by its own main frame");
                Need((1f-backdrop.color.a)*(1f-plate.color.a)>.34f,
                     "Less than 34% of the true built city remains visible");
                Need(Mathf.Abs(ink.rect.width-expected.ReceiptW-16f)<1.1f &&
                     Mathf.Abs(ink.rect.height-expected.ReceiptH-12f)<1.1f &&
                     ink.GetComponent<Image>().color.a>.75f,
                     "City visible but the committed Bastion story is illegible");
            }
            else Need(!ink.gameObject.activeInHierarchy,
                "Non-city milestones must not gain a redundant dark panel");
        }
        static void ClickAtNativeScreenPosition(Canvas canvas,EventSystem events,
            Camera camera,Button button,int width,int height)
        {
            Need(button && button.gameObject.activeInHierarchy && button.interactable,
                "Button not active or interactable: "+(button ? button.name : "null"));
            var corners=new Vector3[4];
            ((RectTransform)button.transform).GetWorldCorners(corners);
            var bottomLeft=RectTransformUtility.WorldToScreenPoint(camera,corners[0]);
            var topRight=RectTransformUtility.WorldToScreenPoint(camera,corners[2]);
            Need(topRight.x-bottomLeft.x>=43.5f &&
                 topRight.y-bottomLeft.y>=43.5f,
                "Touch area below 44 physical pixels: "+button.name);
            Need(bottomLeft.x>=-2f && bottomLeft.y>=-2f &&
                 topRight.x<=width+2f && topRight.y<=height+2f,
                "Real touch control offscreen: "+button.name);
            var pos=(bottomLeft+topRight)*.5f;
            var data=new PointerEventData(events){
                position=pos,pointerId=-1,button=PointerEventData.InputButton.Left
            };
            var hits=new List<RaycastResult>();
            canvas.GetComponent<GraphicRaycaster>().Raycast(data,hits);
            Need(hits.Count>0 &&
                 (hits[0].gameObject==button.gameObject ||
                  hits[0].gameObject.transform.IsChildOf(button.transform)),
                 "Another graphic steals native click: "+button.name);
            // Invoke the exact Unity event handlers used by real players.
            // NEVER bypass the pointer by directly invoking onClick.
            ExecuteEvents.Execute(button.gameObject,data,ExecuteEvents.pointerDownHandler);
            ExecuteEvents.Execute(button.gameObject,data,ExecuteEvents.pointerUpHandler);
            ExecuteEvents.Execute(button.gameObject,data,ExecuteEvents.pointerClickHandler);
        }

        [MenuItem("Eldoria/Tesoros de Valoria/Captura Unity nativa de componente")]
        public static void Capture()
        {
            string output=Path.GetFullPath(Path.Combine(Application.dataPath,"../"+Folder));
            Directory.CreateDirectory(output);
            var report=new Report{
                status="NATIVE_COMPONENT_PROOF_PENDING",
                source=Environment.GetEnvironmentVariable("GITHUB_SHA")??"local",
                note="One real Unity UI component with a disposable gateway, not the canonical city scene. This does not certify animations in time or owner artistic quality.",
                artApproved=false,fullSceneVerified=false,iosVerified=false
            };
            var pictures=new List<Shot>();
            int inputChecks=0;
            try
            {
                EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
                RequireNineImportedSprites();
                report.authoredSpritesImported=true;
                foreach(var resolution in new []{(1600,900),(900,1600),(844,390),(390,844)})
                {
                    int width=resolution.Item1,height=resolution.Item2;
                    var clock=new SimulatedClock{UtcTicks=Day(1)};
                    var store=new TransientStore();
                    var gateway=new LocalGateway(clock,store);
                    // Seed previous six days using real commands, not direct
                    // mutations of game resources, reward counts or inbox.
                    for(int day=1;day<=6;day++)
                    {
                        clock.UtcTicks=Day(day);
                        var state=gateway.Snapshot();
                        var cmd=new GameCommand("qa-daily-"+day,state.PlayerId,
                            "ClaimDailyTreasure","valoria",state.Revision);
                        Need(gateway.Execute(cmd).Ok,"Real gateway seeding failed: "+day);
                    }
                    clock.UtcTicks=Day(7);
                    var before=gateway.Snapshot();
                    Need(before.DailyTreasures.CycleDay==6 && before.Mailbox.Messages.Count>=6,
                        "Day-seven candidate has no honest prior claim ledger");
                    var go=new GameObject("Disposable reward-capture camera");
                    var camera=go.AddComponent<Camera>();
                    camera.clearFlags=CameraClearFlags.SolidColor;
                    camera.backgroundColor=new Color(.025f,.028f,.030f,1f);
                    camera.orthographic=true;camera.orthographicSize=height*.005f;
                    camera.transform.position=new Vector3(0f,0f,-25f);
                    camera.nearClipPlane=.1f;camera.farClipPlane=100f;
                    camera.aspect=width/(float)height;
                    camera.cullingMask=1<<5;
                    // The project uses URP. A hand-created Camera without its
                    // pipeline metadata may silently render an empty frame.
                    var pipeline=UnityEngine.Rendering.GraphicsSettings.currentRenderPipeline;
                    if(pipeline!=null)
                    {
                        var additional=System.Type.GetType(
                            "UnityEngine.Rendering.Universal.UniversalAdditionalCameraData, Unity.RenderPipelines.Universal.Runtime");
                        Need(additional!=null,
                            "Active URP requires UniversalAdditionalCameraData on the disposable QA camera");
                        go.AddComponent(additional);
                    }
                    var rt=new RenderTexture(width,height,24,RenderTextureFormat.ARGB32);
                    camera.targetTexture=rt;
                    var canvasObj=new GameObject("Disposable native uGUI component",
                        typeof(RectTransform),typeof(Canvas),typeof(CanvasScaler),typeof(GraphicRaycaster));
                    canvasObj.layer=5;
                    var rect=(RectTransform)canvasObj.transform;
                    rect.sizeDelta=new Vector2(width,height);
                    rect.localScale=Vector3.one*.01f;
                    var canvas=canvasObj.GetComponent<Canvas>();
                    canvas.renderMode=RenderMode.WorldSpace;
                    canvas.worldCamera=camera;
                    var panel=DailyTreasurePanel.Install(rect);
                    panel.Bind(gateway,()=>clock.UtcTicks,()=>{});
                    // Imported assets alone are NOT evidence of their use.
                    // Fail rather than reporting an authored-art PASS when the
                    // component quietly falls back to procedural geometry.
                    Need(panel.HasAuthoredChestArt,
                        "Native panel lost the two commissioned chest poses");
                    var exactClosed=FindRect(panel,
                        "Authored chest / closed").GetComponent<Image>();
                    var exactOpened=FindRect(panel,
                        "Authored chest / opened").GetComponent<Image>();
                    Need(exactClosed && exactOpened &&
                         exactClosed.sprite==Resources.Load<Sprite>(
                            "DailyTreasureArt/treasure_closed_512") &&
                         exactOpened.sprite==Resources.Load<Sprite>(
                            "DailyTreasureArt/treasure_opened_512") &&
                         exactClosed.preserveAspect && exactOpened.preserveAspect,
                         "Native UI does not display the two original imported chest sprites");
                    report.authoredSpritesImported &=
                        panel.HasAuthoredChestArt;
                    var eventObj=new GameObject("Disposable event system",typeof(EventSystem));
                    var eventSystem=eventObj.GetComponent<EventSystem>();
                    VerifyUiOwnership(1,eventSystem);
                    // Regression for Unity 6's slash-as-path rule. The
                    // Heroes screen's REAL literal name has a slash and
                    // starts inactive; a reward must wait while it is
                    // visibly open, then resume when dismissed. No payout
                    // or extra Canvas is created by this isolated probe.
                    var heroProbe=new GameObject(
                        "Heroes / canonical destination",typeof(RectTransform));
                    heroProbe.layer=5;
                    heroProbe.transform.SetParent(rect,false);
                    heroProbe.SetActive(false);
                    Need(!panel.ProbeHeroRoomVisibilityForNativeEvidence(),
                        "Inactive Heroes room incorrectly blocks daily reward");
                    heroProbe.SetActive(true);
                    Need(panel.ProbeHeroRoomVisibilityForNativeEvidence(),
                        "Visible Lyra room not detected: slash-name lookup regression");
                    Need(!panel.PresentCommittedMilestone(
                            "This must not cover real Lyra",
                            "Visual-only rejection probe",true,"milestone:hero:lyra"),
                        "Direct ceremony API improperly covered canonical hero room");
                    heroProbe.SetActive(false);
                    Need(!panel.ProbeHeroRoomVisibilityForNativeEvidence(),
                        "Closed Heroes room incorrectly blocks daily reward");
                    Object.DestroyImmediate(heroProbe);
                    Canvas.ForceUpdateCanvases();
                    pictures.Add(Render(camera,rt,output,width,height,
                        "hud-entry",
                        "Genuine unclaimed original chest medallion on live native canvas",
                        true,false));
                    // The entry medallion itself must accept a true pointer
                    // sequence; invoking Open() would conceal hitbox failures.
                    ClickAtNativeScreenPosition(canvas,eventSystem,camera,
                        FindButton(panel,"Daily reward / optional medallion"),
                        width,height);
                    inputChecks++;
                    Need(panel.IsOpen,"Real entry medallion did not open calendar");
                    Canvas.ForceUpdateCanvases();
                    var button=FindButton(panel,"Claim exact daily reward");
                    Need(button.interactable,"Correct day seven must be claimable");
                    // Capture BEFORE interacting, then tap through Unity's actual
                    // raycaster and selectable pointer sequence.
                    pictures.Add(Render(camera,rt,output,width,height,"calendar",
                        "Available calendar before claim",true,false));
                    ClickAtNativeScreenPosition(canvas,eventSystem,camera,
                        button,width,height);
                    inputChecks++;
                    var after=gateway.Snapshot();
                    Need(after.DailyTreasures.CycleDay==7 &&
                        after.SpeedupFiveMinutes==before.SpeedupFiveMinutes+1,
                        "Native button failed to pay actual day-seven reward");
                    Need(after.Mailbox.Messages.Count==before.Mailbox.Messages.Count+1,
                        "Day-seven native button did not write the mailbox receipt");
                    Need(!DailyTreasureRules.CanClaim(after.DailyTreasures,clock.UtcTicks),
                        "Duplicate seven-day claim still available");
                    var beforeReplay=after.Resources.Wood;
                    var repeat=new GameCommand("qa-repeat-native-"+width+"x"+height,
                        after.PlayerId,"ClaimDailyTreasure","valoria",after.Revision);
                    Need(!gateway.Execute(repeat).Ok &&
                         gateway.Snapshot().Resources.Wood==beforeReplay,
                        "A second native reward command paid twice");
                    report.realGatewayClaims++;
                    Canvas.ForceUpdateCanvases();
                    pictures.Add(Render(camera,rt,output,width,height,"closed",
                        "Same real claim: closed chest",true,true));
                    // Capture the exact authored runtime frame at its peak:
                    // this distinguishes a convincing transition from two
                    // attractive static endpoints and detects ghosting.
                    panel.SampleForNativeEvidence(
                        DailyTreasureRevealTimeline.RevealPeakSeconds);
                    Canvas.ForceUpdateCanvases();
                    pictures.Add(Render(camera,rt,output,width,height,"opening",
                        "Same real paid claim: illustrated opening at peak light",true,true));
                    ClickAtNativeScreenPosition(canvas,eventSystem,camera,
                        FindButton(panel,"Skip animation / keep earned items"),width,height);
                    inputChecks++;
                    Canvas.ForceUpdateCanvases();
                    Need(panel.EarnedItemCardCount==4 &&
                         panel.HasRevealedIllustratedEarnedItems,
                        "Day seven must show four ORIGINAL painted resource cards after real claim");
                    pictures.Add(Render(camera,rt,output,width,height,"opened",
                        "Same real claim: chest opened and exact items",true,true));
                    if((width==390&&height==844)||(width==844&&height==390))
                    {
                        // Show the actual between-keyframes chest-to-inventory
                        // item flight, never only the two static endpoints.
                        panel.SamplePaidItemFlightForNativeEvidence(.23f);
                        Canvas.ForceUpdateCanvases();
                        pictures.Add(Render(camera,rt,output,width,height,
                            "day7-prize-flight",
                            "Real four-item day-seven chest-to-inventory arc",true,true));
                        panel.SamplePaidItemFlightForNativeEvidence(2f);
                    }
                    Need(gateway.Snapshot().Resources.Wood==after.Resources.Wood &&
                         gateway.Snapshot().Resources.Stone==after.Resources.Stone,
                         "Skipping ceremony must never pay rewards again");
                    ClickAtNativeScreenPosition(canvas,eventSystem,camera,
                        FindButton(panel,"Close tributes"),width,height);
                    inputChecks++;
                    Need(!panel.IsOpen,
                        "Real close button did not leave the reward UI");

                    // Distinct day-four ceremonial proof on the SAME exact
                    // Unity canvas and render target, with its OWN disposable
                    // real player state, not changing a day-seven save to
                    // fabricate an earlier reward after the fact.
                    clock.UtcTicks=Day(1);
                    var specialStore=new TransientStore();
                    var specialGateway=new LocalGateway(clock,specialStore);
                    for(int day=1;day<=3;day++)
                    {
                        clock.UtcTicks=Day(day);
                        var earned=specialGateway.Snapshot();
                        var seed=new GameCommand("qa-special-seed-"+day,
                            earned.PlayerId,"ClaimDailyTreasure","valoria",
                            earned.Revision);
                        Need(specialGateway.Execute(seed).Ok,
                            "Day-four candidate seeding failed "+day);
                    }
                    clock.UtcTicks=Day(4);
                    var specialBefore=specialGateway.Snapshot();
                    panel.Bind(specialGateway,()=>clock.UtcTicks,()=>{});
                    ClickAtNativeScreenPosition(canvas,eventSystem,camera,
                        FindButton(panel,"Daily reward / optional medallion"),
                        width,height);
                    inputChecks++;
                    Need(panel.IsOpen,"Special-day entry not interactive");
                    Canvas.ForceUpdateCanvases();
                    pictures.Add(Render(camera,rt,output,width,height,
                        "special-day4-calendar",
                        "Real day-four candidate before claim",true,false));
                    ClickAtNativeScreenPosition(canvas,eventSystem,camera,
                        FindButton(panel,"Claim exact daily reward"),
                        width,height);
                    inputChecks++;
                    var specialAfter=specialGateway.Snapshot();
                    Need(specialAfter.DailyTreasures.CycleDay==4 &&
                         specialAfter.Resources.Wood==specialBefore.Resources.Wood+90 &&
                         specialAfter.Resources.Stone==specialBefore.Resources.Stone+60 &&
                         specialAfter.Mailbox.Messages.Count==specialBefore.Mailbox.Messages.Count+1,
                        "Day-four chest must represent its exact earned resource contents");
                    report.realGatewayClaims++;
                    panel.SampleForNativeEvidence(
                        DailyTreasureRevealTimeline.RevealPeakSeconds);
                    Canvas.ForceUpdateCanvases();
                    pictures.Add(Render(camera,rt,output,width,height,
                        "special-day4-opening",
                        "Real special chest opening at verified peak light",true,true));
                    ClickAtNativeScreenPosition(canvas,eventSystem,camera,
                        FindButton(panel,"Skip animation / keep earned items"),
                        width,height);
                    inputChecks++;
                    Canvas.ForceUpdateCanvases();
                    Need(panel.EarnedItemCardCount==2 &&
                         panel.HasRevealedIllustratedEarnedItems,
                        "Day four must show two painted, real paid wood and stone cards");
                    pictures.Add(Render(camera,rt,output,width,height,
                        "special-day4-received",
                        "Actual day-four payment shown as two original illustrated resource cards",
                        true,true));
                    if((width==390&&height==844)||(width==844&&height==390))
                    {
                        panel.SamplePaidItemFlightForNativeEvidence(.19f);
                        Canvas.ForceUpdateCanvases();
                        pictures.Add(Render(camera,rt,output,width,height,
                            "special-day4-prize-flight",
                            "Real two-item day-four chest-to-inventory arc",true,true));
                        panel.SamplePaidItemFlightForNativeEvidence(2f);
                    }
                    Need(specialGateway.Snapshot().Resources.Wood==
                         specialAfter.Resources.Wood &&
                         specialGateway.Snapshot().Resources.Stone==
                         specialAfter.Resources.Stone,
                        "Skipping the special chest may not award extra resources");
                    ClickAtNativeScreenPosition(canvas,eventSystem,camera,
                        FindButton(panel,"Close tributes"),width,height);
                    inputChecks++;
                    Need(!panel.IsOpen,"Day-four close must work");

                    // Rehearse PR173's exact independent canvas identity and
                    // safe-area naming without editing or relying on that
                    // other workstream's assembly. The old Royal canvas is
                    // explicitly switched off, just as Vigilia does in game.
                    var nativeObj=new GameObject("VIGILIA / independent canvas",
                        typeof(RectTransform),typeof(Canvas),
                        typeof(CanvasScaler),typeof(GraphicRaycaster));
                    nativeObj.layer=5;
                    var nativeRoot=(RectTransform)nativeObj.transform;
                    nativeRoot.sizeDelta=new Vector2(width,height);
                    nativeRoot.localScale=Vector3.one*.01f;
                    var nativeCanvas=nativeObj.GetComponent<Canvas>();
                    nativeCanvas.renderMode=RenderMode.WorldSpace;
                    nativeCanvas.worldCamera=camera;
                    var safeObj=new GameObject("Safe composition",
                        typeof(RectTransform));
                    safeObj.layer=5;
                    safeObj.transform.SetParent(nativeObj.transform,false);
                    var safeRect=(RectTransform)safeObj.transform;
                    safeRect.anchorMin=Vector2.zero;
                    safeRect.anchorMax=Vector2.one;
                    safeRect.offsetMin=Vector2.zero;
                    safeRect.offsetMax=Vector2.zero;
                    canvas.enabled=false;
                    panel.ProbePresentationCanvasForNativeEvidence();
                    Need(panel.UsesVigiliaCanvas &&
                         panel.transform.parent==safeRect,
                         "Daily treasure canvas remained invisible on Vigilia");
                    VerifyUiOwnership(2,eventSystem);
                    Canvas.ForceUpdateCanvases();
                    ClickAtNativeScreenPosition(nativeCanvas,eventSystem,camera,
                        FindButton(panel,"Daily reward / optional medallion"),
                        width,height);
                    inputChecks++;
                    Need(panel.IsOpen,
                        "Independent Vigilia reward medallion cannot open");
                    ClickAtNativeScreenPosition(nativeCanvas,eventSystem,camera,
                        FindButton(panel,"Close tributes"),width,height);
                    inputChecks++;
                    Need(!panel.IsOpen,
                        "Independent Vigilia reward modal cannot close");
                    nativeCanvas.enabled=false;
                    panel.ProbePresentationCanvasForNativeEvidence();
                    Need(!panel.UsesVigiliaCanvas &&
                         panel.transform.parent==rect,
                         "Daily treasure UI cannot restore to original canvas");
                    canvas.enabled=true;
                    Object.DestroyImmediate(nativeObj);
                    VerifyUiOwnership(1,eventSystem);

                    // Four DIFFERENT real-world milestone categories share
                    // one premium authored chest and ornate frame. Test each
                    // category's ambient palette in portrait and landscape.
                    // These are VISUAL-ONLY style probes (no battle, no hero
                    // unlock, no resources, no second reward, no fake receipt).
                    if((width==390&&height==844)||(width==844&&height==390))
                    {
                        var styleIds=new[]{
                            "milestone:hero:lyra","milestone:bastion:5",
                            "vigilant:chapter:25","scout:first:exploration"};
                        var styleLabels=new[]{
                            "Lyra se une a Valoria",
                            "El Bastión alcanza el nivel 5",
                            "Capítulo de los Vigilantes completado",
                            "Descubrimiento de los exploradores"};
                        var styleDetails=new[]{
                            "La exploradora puede participar en nuevas marchas.",
                            "Valoria extiende sus fronteras.",
                            "El combate y su resultado constan en el buzón.",
                            "Nuevo informe de exploración registrado."};
                        var styleSuffixes=new[]{"lyra","bastion","vigilants","scouts"};
                        // The hero-hall, exploratory map and Vigilants
                        // landmark use real painterly source textures from
                        // Eldoria, not just a different color behind a chest.
                        var requiredScenery=new[]{
                            "EldoriaV41/heroes-hall-illustrated-v2",
                            (string)null,
                            "EldoriaRetention/vigilants-painterly-v2",
                            "EldoriaRetention/scouts-painterly-v2"
                        };
                        for(int style=0;style<styleIds.Length;style++)
                        {
                            var wallet=specialGateway.Snapshot();
                            Need(panel.PresentCommittedMilestone(
                                    styleLabels[style],styleDetails[style],
                                    legendary:style!=3,eventId:styleIds[style]),
                                "Premium milestone style presentation unavailable: "+
                                styleIds[style]);
                            Need((requiredScenery[style]==null &&
                                  !panel.HasMilestoneScenery) ||
                                 (requiredScenery[style]!=null &&
                                  panel.HasMilestoneScenery &&
                                  panel.MilestoneSceneryKey==requiredScenery[style]),
                                "Wrong or absent original painterly milestone scenery: "+
                                styleIds[style]);
                            Canvas.ForceUpdateCanvases();
                            VerifyGenuineSceneComposition(
                                panel,styleIds[style],width,height);
                            panel.SampleForNativeEvidence(
                                DailyTreasureRevealTimeline.RevealPeakSeconds);
                            Canvas.ForceUpdateCanvases();
                            pictures.Add(Render(camera,rt,output,width,height,
                                "style-"+styleSuffixes[style]+"-opening",
                                "Visual style preview ONLY, not an earned milestone: "+
                                    styleIds[style],true,false));
                            ClickAtNativeScreenPosition(canvas,eventSystem,camera,
                                FindButton(panel,"Skip animation / keep earned items"),
                                width,height);
                            inputChecks++;
                            Canvas.ForceUpdateCanvases();
                            pictures.Add(Render(camera,rt,output,width,height,
                                "style-"+styleSuffixes[style]+"-result",
                                "Visual result layout ONLY, with no milestone command executed: "+
                                    styleIds[style],true,false));
                            ClickAtNativeScreenPosition(canvas,eventSystem,camera,
                                FindButton(panel,"Close tributes"),width,height);
                            inputChecks++;
                            Need(!panel.IsOpen,
                                "Premium milestone style refuses to close: "+styleIds[style]);
                            Need(!panel.HasMilestoneScenery &&
                                 panel.MilestoneSceneryKey==null,
                                "Optional painterly texture still pinned after ceremony close: "+
                                styleIds[style]);
                            var afterStyle=specialGateway.Snapshot();
                            Need(wallet.Resources.Wood==afterStyle.Resources.Wood &&
                                 wallet.Resources.Stone==afterStyle.Resources.Stone &&
                                 wallet.Resources.Food==afterStyle.Resources.Food &&
                                 wallet.Mailbox.Messages.Count==afterStyle.Mailbox.Messages.Count,
                                 "Visual-only style probe granted resources or mail: "+
                                 styleIds[style]);
                            report.visualOnlyMilestoneCaptures+=2;
                        }
                    }

                    // Narrative priority can arrive after a daily modal
                    // was ALREADY opened, not just before the initial tap.
                    // This must preempt the overlay and stop its Foley, while
                    // the real Day 4 claim and mailbox receipt remain saved.
                    var beforeInterrupt=specialGateway.Snapshot();
                    panel.Open();
                    Need(panel.IsOpen,"Previously paid calendar cannot be reopened");
                    panel.Suppressed=true;
                    Need(!panel.IsOpen &&
                         !FindButton(panel,"Daily reward / optional medallion")
                            .gameObject.activeInHierarchy,
                        "Narrative failed to preempt an open daily calendar");
                    panel.Suppressed=false;
                    Need(FindButton(panel,"Daily reward / optional medallion")
                            .gameObject.activeInHierarchy,
                        "Reward access did not recover after story dismissal");

                    // Apply the same interruption while a premium animation
                    // is genuinely playing. This is an event-style UI probe
                    // ONLY, with no new hero or combat reward command.
                    Need(panel.PresentCommittedMilestone(
                            "Hito previo a un tutorial",
                            "La información permanece en el buzón",
                            legendary:true,eventId:"vigilant:chapter:25"),
                        "Could not open an optional ceremonial overlay");
                    Need(panel.IsOpen,
                        "Premium ceremony was not visible before interruption");
                    panel.Suppressed=true;
                    Need(!panel.IsOpen,
                        "Tutorial overlay did not interrupt the premium coffer");
                    Need(!panel.PresentCommittedMilestone(
                            "This must not cover a tutorial",
                            "Visual-only rejection probe",true,"vigilant:chapter:25"),
                        "Direct ceremony API bypassed narrative/tutorial priority");
                    panel.Suppressed=false;
                    var afterInterrupt=specialGateway.Snapshot();
                    Need(beforeInterrupt.Resources.Wood==afterInterrupt.Resources.Wood &&
                         beforeInterrupt.Resources.Stone==afterInterrupt.Resources.Stone &&
                         beforeInterrupt.Resources.Food==afterInterrupt.Resources.Food &&
                         beforeInterrupt.Mailbox.Messages.Count==
                            afterInterrupt.Mailbox.Messages.Count,
                        "Interrupting a premium ceremony corrupted an already-paid reward");

                    Object.DestroyImmediate(eventObj);
                    Object.DestroyImmediate(canvasObj);
                    camera.targetTexture=null;
                    rt.Release();
                    Object.DestroyImmediate(rt);
                    Object.DestroyImmediate(go);
                }
                Need(pictures.Count==52 && inputChecks==56 &&
                     report.realGatewayClaims==8 &&
                     report.visualOnlyMilestoneCaptures==16,
                    "Real daily rewards and four artistic milestone styles not fully captured");
                report.status=report.authoredSpritesImported
                    ? "NATIVE_COMPONENT_RENDERED_WITH_AUTHORED_SPRITES_ART_REVIEW_PENDING"
                    : "NATIVE_COMPONENT_RENDERED_FALLBACK_ONLY_ART_REJECTED";
                report.graphicalPointerHits=inputChecks;
                report.images=pictures.Count;
                report.shots=pictures.ToArray();
                File.WriteAllText(Path.Combine(output,"report.json"),JsonUtility.ToJson(report,true));
                Debug.Log("DAILY_TREASURE_NATIVE_COMPONENT_CAPTURED: 52 PNG (4 entry, 28 daily, 4 prize flights, 16 visual style); NOT ART_APPROVED / NOT FULL GAMEPASS");
                EditorApplication.Exit(0);
            }
            catch(Exception e)
            {
                report.status="NATIVE_COMPONENT_FAILED";
                report.note=e.ToString();
                File.WriteAllText(Path.Combine(output,"report.json"),JsonUtility.ToJson(report,true));
                Debug.LogException(e);
                EditorApplication.Exit(1);
            }
        }
        static Shot Render(Camera camera,RenderTexture rt,string output,
            int width,int height,string variant,string description,bool native,bool saved)
        {
            var previous=RenderTexture.active;
            var png=new Texture2D(width,height,TextureFormat.RGB24,false);
            try
            {
                Canvas.ForceUpdateCanvases();
                camera.Render(); camera.Render();
                RenderTexture.active=rt;
                png.ReadPixels(new Rect(0,0,width,height),0,0);
                png.Apply(false);
                string name="real-unity-daily-"+variant+"-"+width+"x"+height+".png";
                string path=Path.Combine(output,name);
                File.WriteAllBytes(path,png.EncodeToPNG());
                long bytes=new FileInfo(path).Length;
                Need(bytes>7000,"Suspiciously empty native capture: "+path);
                return new Shot{file=name,sha256=Sha(path),view=description,
                    width=width,height=height,bytes=(int)bytes,
                    nativeCanvas=native,coversReward=saved,
                    readyToClaim=variant=="calendar" ||
                        variant=="special-day4-calendar",preserved=saved};
            }
            finally
            {
                RenderTexture.active=previous;
                Object.DestroyImmediate(png);
            }
        }
    }
}
#endif
