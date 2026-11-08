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
            public bool artApproved,fullSceneVerified,iosVerified,authoredSpritesImported;
            public Shot[] shots;
        }
        static void Need(bool ok,string reason)
        {
            if(!ok)throw new InvalidOperationException(reason);
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
                    if(!panel.HasAuthoredChestArt)
                        Debug.LogWarning("DAILY_TREASURE_ART_NOT_IMPORTED: procedural fallback only; never call this art approval");
                    report.authoredSpritesImported = report.authoredSpritesImported || panel.HasAuthoredChestArt;
                    var eventObj=new GameObject("Disposable event system",typeof(EventSystem));
                    var eventSystem=eventObj.GetComponent<EventSystem>();
                    Canvas.ForceUpdateCanvases();
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
                        "Available calendar before claim",true,true));
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
                    pictures.Add(Render(camera,rt,output,width,height,"opened",
                        "Same real claim: chest opened and exact items",true,true));
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
                        "Real day-four candidate before claim",true,true));
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
                    Need(specialGateway.Snapshot().Resources.Wood==
                         specialAfter.Resources.Wood &&
                         specialGateway.Snapshot().Resources.Stone==
                         specialAfter.Resources.Stone,
                        "Skipping the special chest may not award extra resources");
                    ClickAtNativeScreenPosition(canvas,eventSystem,camera,
                        FindButton(panel,"Close tributes"),width,height);
                    inputChecks++;
                    Need(!panel.IsOpen,"Day-four close must work");
                    Object.DestroyImmediate(eventObj);
                    Object.DestroyImmediate(canvasObj);
                    camera.targetTexture=null;
                    rt.Release();
                    Object.DestroyImmediate(rt);
                    Object.DestroyImmediate(go);
                }
                Need(pictures.Count==24 && inputChecks==32 && report.realGatewayClaims==8,
                    "Day-four and day-seven native reward coverage incomplete");
                report.status=report.authoredSpritesImported
                    ? "NATIVE_COMPONENT_RENDERED_WITH_AUTHORED_SPRITES_ART_REVIEW_PENDING"
                    : "NATIVE_COMPONENT_RENDERED_FALLBACK_ONLY_ART_REJECTED";
                report.graphicalPointerHits=inputChecks;
                report.images=pictures.Count;
                report.shots=pictures.ToArray();
                File.WriteAllText(Path.Combine(output,"report.json"),JsonUtility.ToJson(report,true));
                Debug.Log("DAILY_TREASURE_NATIVE_COMPONENT_CAPTURED: 24 PNG; day-four/day-seven; NOT ART_APPROVED / NOT FULL GAMEPASS");
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
                    nativeCanvas=native,coversReward=true,
                    readyToClaim=variant=="calendar",preserved=saved};
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
