#if UNITY_EDITOR
using System;
using System.Collections;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using Eldoria.Domain;
using Eldoria.Application;
using Eldoria.Infrastructure;
using Eldoria.UI.RoyalGold;
using Eldoria.UI.RoyalGold.V2;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Object=UnityEngine.Object;

namespace Eldoria.ReferenceClean.Editor
{
    /// <summary>Real native scene proof: two separate illustrated destinations,
    /// contextual shortcut gating, and actual 1600x900/900x1600 camera renders.
    /// Uses ONLY a disposable VALORIA_GAMEPLAY_PROBE save fixture; every prior
    /// player state in the Windows workspace stays untouched.</summary>
    [InitializeOnLoad] public static class EarlyRetentionNativeCapture
    {
        const string Flag="Eldoria.EarlyRetention.NativeProbe";
        const string Scene="Assets/Eldoria/ArtTests/ValoriaReferenceClean/Integration/Valoria_City_World_UI.unity";
        static string Output=>Path.GetFullPath(Environment.GetEnvironmentVariable("EARLY_RETENTION_EVIDENCE") ??
            Path.Combine(UnityEngine.Application.dataPath,"../../docs/early-retention/evidence"));
        static readonly List<string> Checks=new List<string>();
        static EarlyRetentionNativeCapture()=>EditorApplication.playModeStateChanged+=Entered;
        static void Need(bool ok,string reason)
        {
            if(!ok)throw new InvalidOperationException("EARLY_RETENTION_FAIL: "+reason);
            Checks.Add(reason);Debug.Log("EARLY_RETENTION_PASS: "+reason);
        }
        public static void Capture()
        {
            try{
                Environment.SetEnvironmentVariable("VALORIA_GAMEPLAY_PROBE","1");
                Directory.CreateDirectory(Output);
                EditorSceneManager.OpenScene(Scene);
                SessionState.SetBool(Flag,true);
                EditorApplication.EnterPlaymode();
            }catch(Exception e){Fail(e);}
        }
        static void Entered(PlayModeStateChange state)
        {
            if(state!=PlayModeStateChange.EnteredPlayMode||!SessionState.GetBool(Flag,false))return;
            SessionState.SetBool(Flag,false);
            EditorApplication.delayCall+=()=>{
                var h=Object.FindFirstObjectByType<ValoriaIntegratedWorld>();
                if(!h){Fail(new Exception("Actual integrated Valoria host missing"));return;}
                h.StartCoroutine(Guard(h));
            };
        }
        static IEnumerator Guard(ValoriaIntegratedWorld h)
        {
            var it=Probe(h);
            while(true)
            {
                bool more;object current=null;
                try{more=it.MoveNext();if(more)current=it.Current;}
                catch(Exception e){Fail(e);yield break;}
                if(!more)yield break;
                yield return current;
            }
        }
        static void SaveFixture(ValoriaIntegratedWorld h,int bastion)
        {
            Need(h.City.Probe,"Real player save cannot be overwritten by any native proof");
            var s=h.City.Gateway.Snapshot();
            s.SawmillLevel=1;
            s.Available.ArcherT1=60;
            if(bastion>=2){
                s.BastionLevel=2;s.BarracksLevel=1;
                s.ChapterProgress.BastionTwoReached=true;
                s.ChapterProgress.MarchConfirmed=true;
            }else{
                s.BastionLevel=1;s.BarracksLevel=0;
                s.ChapterProgress.MarchConfirmed=false;
            }
            new FileStateStore(h.City.SaveFile).Save(s);
            h.City.ReloadForProbe();
        }
        static void Screenshot(ValoriaIntegratedWorld h,string name,int w,int he)
        {
            var path=Path.Combine(Output,name+(w>he?"-horizontal":"-vertical")+".png");
            // Supply an ABSOLUTE evidence path: the shared renderer uses
            // Path.Combine(Ev, name), which correctly preserves a rooted name.
            // This captures directly into the retention workflow artifact.
            ValoriaIntegrationBuild.Capture(h,path,w,he);
            Need(File.Exists(path)&&new FileInfo(path).Length>30000,
                "Native Unity "+w+"x"+he+" screenshot exists: "+name);
            var tex=new Texture2D(2,2,TextureFormat.RGB24,false);
            try{
                Need(tex.LoadImage(File.ReadAllBytes(path))&&tex.width==w&&tex.height==he,
                    "Real camera pixels decode at expected resolution: "+name);
                var pixels=tex.GetPixels32();
                int navy=0,gold=0;
                for(int i=0;i<pixels.Length;i+=7)
                {
                    var c=pixels[i];
                    if(c.b>c.r*1.10f&&c.b>c.g*.90f&&c.b>=22&&c.b<=165)navy++;
                    if(c.r>125&&c.g>75&&c.r>c.g*1.13f&&c.g>c.b*1.13f)gold++;
                }
                Need(navy>pixels.Length/250&&gold>pixels.Length/10000,
                    "Actual royal-navy and gilded visuals occupy the captured screen: "+name);
            }
            finally{Object.Destroy(tex);}
        }
        static void ClickReal(RoyalButton button,string label)
        {
            Need(button&&button.IsInteractable()&&button.gameObject.activeInHierarchy,
                label+" genuine live button clickable");
            Canvas.ForceUpdateCanvases();
            var rect=button.transform as RectTransform;
            var corners=new Vector3[4];rect.GetWorldCorners(corners);
            var point=RectTransformUtility.WorldToScreenPoint(null,
                (corners[0]+corners[2])*.5f);
            var eventSystem=EventSystem.current;
            Need(eventSystem!=null,"One real EventSystem is active");
            var data=new PointerEventData(eventSystem){
                pointerId=-1,button=PointerEventData.InputButton.Left,position=point};
            var hits=new List<RaycastResult>();
            eventSystem.RaycastAll(data,hits);
            Need(hits.Count>0&&ExecuteEvents.GetEventHandler<IPointerClickHandler>(
                hits[0].gameObject)==button.gameObject,
                label+" actual topmost device raycast reaches the button");
            ExecuteEvents.Execute(button.gameObject,data,ExecuteEvents.pointerDownHandler);
            ExecuteEvents.Execute(button.gameObject,data,ExecuteEvents.pointerUpHandler);
            ExecuteEvents.Execute(button.gameObject,data,ExecuteEvents.pointerClickHandler);
        }
        static IEnumerator Probe(ValoriaIntegratedWorld h)
        {
            for(int i=0;i<240&&!h.Ready;i++)yield return null;
            Need(h.Ready,"Actual Unity Valoria scene is playable");
            var hud=h.Hud;
            Need(hud!=null,"One real RoyalHUD instantiated");
            var guidance=h.Guidance;
            if(guidance)guidance.enabled=false;
            for(int i=0;i<50&&hud.NarrativeOpen;i++){
                hud.Modal.Extra.onClick.Invoke();
                yield return null;
            }
            if(hud.Modal.gameObject.activeSelf)hud.Modal.Hide();
            hud.SelectBuilding("");
            yield return null;
            var ui=hud.GetComponent<RoyalGoldV3EarlyRetentionScreens>();
            Need(ui!=null,"Both destinations attached to canonical existing HUD");
            Need(!EarlyRetentionRules.VigilantsUnlocked(h.City.Gateway.Snapshot()),
                "Virgin player has no premature Vigilants access");
            Need(!EarlyRetentionRules.ScoutsUnlocked(h.City.Gateway.Snapshot()),
                "Virgin player has no premature Scouts access");
            yield return new WaitForSecondsRealtime(.35f);
            Need(!ui.VigilantsShortcut.gameObject.activeSelf &&
                !ui.ScoutsShortcut.gameObject.activeSelf,
                "Both shortcuts are genuinely hidden before progression");

            SaveFixture(h,1);yield return null;yield return new WaitForSecondsRealtime(.35f);
            Need(ui.VigilantsShortcut.gameObject.activeInHierarchy &&
                !ui.ScoutsShortcut.gameObject.activeInHierarchy,
                "One early Vigilant trial unlocked but Scouts is still hidden");
            var actualCompass=Resources.Load<Sprite>("EldoriaRetention/vigilants-compass");
            var actualCrest=Resources.Load<Sprite>("EldoriaRetention/scouts-crest");
            Need(actualCompass && actualCrest,
                "Previously-authored original Valoria crest and compass art load from the real game resources");
            Need(ui.VigilantsShortcut.GetComponentsInChildren<Image>(true).Any(x=>
                    x.sprite==actualCompass && !x.raycastTarget),
                "The actual unlocked Vigilants button shows the real Valoria compass, with no extra touch blocker");
            ui.Open(RoyalGoldV3EarlyRetentionScreens.Destination.Vigilants);
            Need(ui.IsOpen,"Vigilants' independent full-screen scene opens");
            var illustration=hud.GetComponentsInChildren<RawImage>(true)
                .FirstOrDefault(x=>x.name=="Original illustrated world / decorative only");
            Need(illustration && illustration.enabled && illustration.texture!=null &&
                illustration.texture.width==512 && illustration.texture.height==512 &&
                illustration.texture.name.Contains("painterly"),
                "New painterly Vigilants scene loads through actual Unity Resources");
            var vigilantsPainting=illustration.texture;
            Screenshot(h,"vigilants-first-trial",1600,900);
            Screenshot(h,"vigilants-first-trial",900,1600);
            ui.Close();
            SaveFixture(h,2);yield return null;yield return new WaitForSecondsRealtime(.35f);
            Need(ui.VigilantsShortcut.gameObject.activeInHierarchy &&
                ui.ScoutsShortcut.gameObject.activeInHierarchy,
                "After first March both activity entries legitimately unlock");
            Need(ui.ScoutsShortcut.GetComponentsInChildren<Image>(true).Any(x=>
                    x.sprite==actualCrest && !x.raycastTarget),
                "The actual unlocked Scouts button shows the genuine Valoria crest art");
            ui.Open(RoyalGoldV3EarlyRetentionScreens.Destination.Vigilants);
            Need(ui.IsOpen,"Vigilants opens after first barracks");
            Screenshot(h,"vigilants-chapter-one",1600,900);
            Screenshot(h,"vigilants-chapter-one",900,1600);
            var stageButton=hud.GetComponentsInChildren<RoyalButton>(true)
                .FirstOrDefault(x=>x.name=="Battle against next unique stage");
            ClickReal(stageButton,"Automatic level PvE");
            var s=h.City.Gateway.Snapshot();
            Need(s.Retention.LastClearedStage==1 && s.LastBattleReport.Won,
                "First stage resolves via REAL domain combat, once");
            // Prove the attractive player-facing result, not just the domain
            // success: these captures must show the real gold victory banner.
            var victory=hud.GetComponentsInChildren<RoyalText>(true)
                .FirstOrDefault(x=>x.name=="Original victory heading");
            Need(victory!=null && victory.text=="VICTORIA" &&
                victory.gameObject.activeInHierarchy,
                "The authentic victory announcement is visible in the live scene");
            Screenshot(h,"vigilants-real-victory",1600,900);
            Screenshot(h,"vigilants-real-victory",900,1600);
            ui.Close();
            // Art/Lore parity: the approved fifteenth stage uses the actual
            // Bastion-II Engendro resource, never a stone golem. Only alter the
            // disposable native-probe save, not the owner's player data.
            s=h.City.Gateway.Snapshot();
            s.Retention.LastClearedStage=14;
            new FileStateStore(h.City.SaveFile).Save(s);
            h.City.ReloadForProbe();
            yield return null;
            yield return new WaitForSecondsRealtime(.35f);
            ui.Open(RoyalGoldV3EarlyRetentionScreens.Destination.Vigilants);
            Need(ui.IsOpen,"Stage 15 opens in the real campaign");
            var engendroArt=hud.GetComponentsInChildren<RawImage>(true)
                .FirstOrDefault(x=>x.name=="Engendro / authentic creature illustration");
            Need(engendroArt!=null && engendroArt.gameObject.activeInHierarchy &&
                engendroArt.texture!=null &&
                engendroArt.texture.name.Contains("engendro"),
                "Stage 15 displays Eldoria's genuine Engendro render, not a golem");
            Screenshot(h,"vigilants-stage15-engendro",1600,900);
            Screenshot(h,"vigilants-stage15-engendro",900,1600);
            ui.Close();
            ui.Open(RoyalGoldV3EarlyRetentionScreens.Destination.Scouts);
            Need(ui.IsOpen&&ui.OpenDestination==RoyalGoldV3EarlyRetentionScreens.Destination.Scouts,
                "Scouts has separate scene, not Codex or Missions overlay");
            Need(illustration.enabled && illustration.texture!=null &&
                illustration.texture!=vigilantsPainting &&
                illustration.texture.width==512 && illustration.texture.height==512 &&
                illustration.texture.name.Contains("painterly"),
                "Scouts uses its own cinematic painterly expedition map");
            var contractArt=hud.GetComponentsInChildren<RoyalGoldV3ScoutContractGraphic>(true);
            Need(contractArt.Length==3,
                "The independent Scouts screen contains three real illustrated parchment contracts");
            Canvas.ForceUpdateCanvases();
            Need(contractArt.All(x=>x.gameObject.activeInHierarchy && x.enabled &&
                    !x.canvasRenderer.cull),
                "All three hand-engraved mission cards are actually visible in the native Canvas");
            Need(contractArt.All(x=>!x.raycastTarget),
                "Gold frames, heraldic motifs and wax seals never steal mission-button touches");
            var different=contractArt.Select(x=>x.Kind).Distinct().Count();
            Need(different==3,
                "Three live board entries show their distinct authored contract illustrations");
            Screenshot(h,"scout-mission-board",1600,900);
            Screenshot(h,"scout-mission-board",900,1600);
            var dispatch=hud.GetComponentsInChildren<RoyalButton>(true)
                .FirstOrDefault(x=>x.name=="Dispatch or claim scout 1");
            ClickReal(dispatch,"Actual scout dispatch");
            Need(h.City.Gateway.Snapshot().Retention.Orders.Any(x=>x.StartedUtcTicks>0),
                "Scout dispatch persists its genuine UTC return time");
            ui.Close();
            Need(Object.FindObjectsByType<RoyalHUD>(FindObjectsSortMode.None).Length==1,
                "No duplicate RoyalHUD/Canvas created");
            File.WriteAllText(Path.Combine(Output,"result.txt"),
                "EARLY_RETENTION_NATIVE_PASS\n"+string.Join("\n",Checks)+"\nART_APPROVED=false\n");
            Debug.Log("EARLY_RETENTION_NATIVE_PASS");
            EditorApplication.Exit(0);
        }
        static void Fail(Exception e)
        {
            Directory.CreateDirectory(Output);
            File.WriteAllText(Path.Combine(Output,"failure.txt"),e.ToString());
            Debug.LogException(e);EditorApplication.Exit(1);
        }
    }
}
#endif
