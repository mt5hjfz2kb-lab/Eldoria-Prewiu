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
            public bool artApproved,fullSceneVerified,iosVerified;
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
                    var eventObj=new GameObject("Disposable event system",typeof(EventSystem));
                    var eventSystem=eventObj.GetComponent<EventSystem>();
                    Canvas.ForceUpdateCanvases();
                    panel.Open();
                    Canvas.ForceUpdateCanvases();
                    var button=FindButton(panel,"Claim exact daily reward");
                    Need(button.interactable,"Correct day seven must be claimable");
                    // Genuine GraphicRaycaster pointer target (no invented click
                    // coordinates or direct geometry assertion as a substitute).
                    var corners=new Vector3[4];
                    ((RectTransform)button.transform).GetWorldCorners(corners);
                    var pos=RectTransformUtility.WorldToScreenPoint(camera,(corners[0]+corners[2])*.5f);
                    var hits=new List<RaycastResult>();
                    canvasObj.GetComponent<GraphicRaycaster>().Raycast(
                        new PointerEventData(eventSystem){position=pos},hits);
                    Need(hits.Any(h=>h.gameObject==button.gameObject ||
                        h.gameObject.transform.IsChildOf(button.transform)),
                        "Day-seven button has no native raycast target");
                    inputChecks++;
                    pictures.Add(Render(camera,rt,output,width,height,"calendar",
                        "Available calendar before claim",true,true));
                    button.onClick.Invoke();
                    var after=gateway.Snapshot();
                    Need(after.DailyTreasures.CycleDay==7 &&
                        after.SpeedupFiveMinutes==before.SpeedupFiveMinutes+1,
                        "Native button failed to pay actual day-seven reward");
                    Need(after.Mailbox.Messages.Count==before.Mailbox.Messages.Count+1,
                        "Day-seven native button did not write the mailbox receipt");
                    Need(!button.interactable || !DailyTreasureRules.CanClaim(after.DailyTreasures,clock.UtcTicks),
                        "Duplicate seven-day claim still available");
                    report.realGatewayClaims++;
                    Canvas.ForceUpdateCanvases();
                    pictures.Add(Render(camera,rt,output,width,height,"closed",
                        "Same real claim: closed chest",true,true));
                    FindButton(panel,"Skip animation / keep earned items").onClick.Invoke();
                    Canvas.ForceUpdateCanvases();
                    pictures.Add(Render(camera,rt,output,width,height,"opened",
                        "Same real claim: chest opened and exact items",true,true));
                    Need(gateway.Snapshot().Resources.Wood==after.Resources.Wood &&
                         gateway.Snapshot().Resources.Stone==after.Resources.Stone,
                         "Skipping ceremony must never pay rewards again");
                    panel.Close();
                    Object.DestroyImmediate(eventObj);
                    Object.DestroyImmediate(canvasObj);
                    camera.targetTexture=null;
                    rt.Release();
                    Object.DestroyImmediate(rt);
                    Object.DestroyImmediate(go);
                }
                Need(pictures.Count==12 && inputChecks==4 && report.realGatewayClaims==4,
                    "Native reward component coverage incomplete");
                report.status="NATIVE_COMPONENT_RENDERED_ART_AND_FULL_SCENE_PENDING";
                report.graphicalPointerHits=inputChecks;
                report.images=pictures.Count;
                report.shots=pictures.ToArray();
                File.WriteAllText(Path.Combine(output,"report.json"),JsonUtility.ToJson(report,true));
                Debug.Log("DAILY_TREASURE_NATIVE_COMPONENT_CAPTURED: 12 PNG; NOT ART_APPROVED / NOT FULL GAMEPASS");
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
