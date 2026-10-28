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
            var path=Path.Combine(Output,name+(w<500?"-phone":w>he?"-horizontal":"-vertical")+".png");
            // Supply an ABSOLUTE evidence path: the shared renderer uses
            // Path.Combine(Ev, name), which correctly preserves a rooted name.
            // This captures directly into the retention workflow artifact.
            ValoriaIntegrationBuild.Capture(h,path,w,he,()=>ValidateTextLayout(h));
            Need(File.Exists(path)&&new FileInfo(path).Length>Math.Min(30000,w*he/40),
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
        static void ValidateTextLayout(ValoriaIntegratedWorld h)
        {
            var page=h.Hud.GetComponentsInChildren<RectTransform>(true)
                .First(x=>x.name=="Early retention / independent destination");
            foreach(var label in page.GetComponentsInChildren<Text>())
            {
                if(string.IsNullOrWhiteSpace(label.text) ||
                    label.GetComponentsInParent<CanvasGroup>().Any(g=>g.alpha<.01f))continue;
                Need(label.preferredHeight<=label.rectTransform.rect.height+1f,
                    "Complete visible text fits actual capture layout: "+label.name);
            }
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
            Screenshot(h,"vigilants-first-trial",390,844);
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
            Screenshot(h,"vigilants-chapter-one",390,844);
            yield return null; // real rendered frame before tapping the restored Canvas
            var stageButton=hud.GetComponentsInChildren<RoyalButton>(true)
                .FirstOrDefault(x=>x.name=="Battle against next unique stage");
            Need(stageButton.targetGraphic is RoyalGoldV3RetentionActionGraphic &&
                ((RoyalGoldV3RetentionActionGraphic)stageButton.targetGraphic).Battle,
                "The real battle button uses the approved red and gold action graphic");
            var battlePlate=stageButton?
                stageButton.GetComponent<RoyalGoldV3RetentionActionGraphic>():null;
            Need(battlePlate && battlePlate.Battle &&
                stageButton.targetGraphic==battlePlate && battlePlate.raycastTarget,
                "Real PvE combat action uses the approved red interactive button");
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
            Screenshot(h,"vigilants-real-victory",390,844);
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
                engendroArt.texture.name.Contains("engendro-lore-cutout"),
                "Stage 15 displays the real Engendro with its studio backdrop removed");
            Need(engendroArt.uvRect.width<.90f &&
                engendroArt.uvRect.height<.90f,
                "Stage 15 crops the original gray studio margins so the real creature is visually prominent");
            var currentNumber=hud.GetComponentsInChildren<RoyalText>(true)
                .FirstOrDefault(x=>x.name=="Current live stage");
            Need(currentNumber!=null && currentNumber.gameObject.activeInHierarchy &&
                currentNumber.text=="15",
                "The approved route highlights genuine stage 15, not decorative fake markers");
            var stageTrail=hud.GetComponentsInChildren<RoyalGoldV3StageTrailGraphic>(true)
                .FirstOrDefault();
            var creatureFrame=engendroArt.transform.parent as RectTransform;
            var trailParent=stageTrail?stageTrail.transform.parent as RectTransform:null;
            Need(stageTrail!=null && creatureFrame!=null && trailParent!=null &&
                stageTrail.rectTransform.anchorMin.y*trailParent.rect.height >
                creatureFrame.rect.height+8f,
                "Engendro stage 15 preserves all 25 route markers above the creature portrait");
            Screenshot(h,"vigilants-stage15-engendro",1600,900);
            Screenshot(h,"vigilants-stage15-engendro",900,1600);
            Screenshot(h,"vigilants-stage15-engendro",390,844);
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
            var mapPreviews=hud.GetComponentsInChildren<RawImage>(true)
                .Where(x=>x.name=="Mission map detail (visual only)").ToArray();
            Need(mapPreviews.Length==3 &&
                mapPreviews.All(x=>x.gameObject.activeInHierarchy && x.enabled &&
                    !x.raycastTarget && x.texture!=null),
                "Three authentic expedition previews render without stealing clicks");
            Need(mapPreviews.All(x=>{
                var uv=x.uvRect;
                var rect=x.rectTransform.rect;
                if(uv.height<=.005f||rect.height<=1f)return false;
                var imageAspect=(uv.width*x.texture.width)/
                    (uv.height*x.texture.height);
                var panelAspect=rect.width/rect.height;
                return Mathf.Abs(imageAspect-panelAspect)/
                    Mathf.Max(.1f,panelAspect)<.12f;
            }),"Real map miniatures preserve the authored image aspect without stretching");
            Screenshot(h,"scout-mission-board",900,1600);
            Screenshot(h,"scout-mission-board",390,844);
            yield return null; // real rendered frame before tapping the restored Canvas
            for(int i=1;i<=3;i++)
            {
                var dispatch=hud.GetComponentsInChildren<RoyalButton>(true)
                    .First(x=>x.name=="Dispatch or claim scout "+i);
                var scoutPlate=dispatch.GetComponent<RoyalGoldV3RetentionActionGraphic>();
                Need(scoutPlate && !scoutPlate.Battle && dispatch.targetGraphic==scoutPlate && scoutPlate.raycastTarget,
                    "Actual scout card "+i+" uses the approved blue interactive action graphic");
                int beforeCount=h.City.Gateway.Snapshot().Retention.Orders.Count(x=>x.StartedUtcTicks>0);
                ClickReal(dispatch,"Actual scout dispatch "+i);
                int afterCount=h.City.Gateway.Snapshot().Retention.Orders.Count(x=>x.StartedUtcTicks>0);
                Need(afterCount==beforeCount+1,
                    "Expedition "+i+" dispatches exactly one persistent UTC order");
            }
            var sent=h.City.Gateway.Snapshot().Retention.Orders.Where(x=>x.StartedUtcTicks>0).ToArray();
            Need(sent.Length==3,"All three cards dispatch and persist their genuine UTC return time");
            Screenshot(h,"scout-missions-underway",390,844);
            var clock=h.City.Clock as ValoriaReferenceCleanGameplay.TestClock;
            Need(clock!=null,"Only disposable probe clock advances for offline return proof");
            clock.Step((int)Math.Ceiling((sent.Max(x=>x.EndsUtcTicks)-clock.UtcTicks)/(double)TimeSpan.TicksPerSecond)+1);
            ui.SynchronizeLayout();
            yield return null;
            Screenshot(h,"scout-missions-ready",390,844);
            yield return null;
            for(int i=3;i>=1;i--)
            {
                var claim=hud.GetComponentsInChildren<RoyalButton>(true)
                    .First(x=>x.name=="Dispatch or claim scout "+i);
                ClickReal(claim,"Actual scout reward claim "+i);
            }
            ui.Close();
            ui.Open(RoyalGoldV3EarlyRetentionScreens.Destination.Vigilants);
            yield return null;
            var beforeXp=h.City.Gateway.Snapshot().Retention;
            int pendingXp=EarlyRetentionRules.UnclaimedIdleXp(beforeXp,clock.UtcTicks);
            Need(pendingXp>0,"Real offline interval accrued hero experience");
            var collectXp=hud.GetComponentsInChildren<RoyalButton>(true)
                .First(x=>x.name=="Claim earned hero experience");
            ClickReal(collectXp,"Claim actual offline hero experience");
            var claimed=h.City.Gateway.Snapshot();
            Need(claimed.Retention.AldricExperience==beforeXp.AldricExperience+pendingXp &&
                EarlyRetentionRules.UnclaimedIdleXp(claimed.Retention,clock.UtcTicks)==0,
                "Actual XP button grants earned experience once");
            Need(sent.All(order=>claimed.Retention.Orders.Any(x=>x.Id==order.Id&&x.Claimed)),
                "Each dispatched card is claimed exactly once through its real button");
            var back=hud.GetComponentsInChildren<RoyalButton>(true)
                .First(x=>x.name=="Return to actual kingdom");
            ClickReal(back,"Return to Valoria");
            Need(!ui.IsOpen,"Actual back button closes the activity destination");
            h.City.ReloadForProbe();yield return null;
            var loaded=h.City.Gateway.Snapshot();
            Need(loaded.Retention.LastClearedStage==claimed.Retention.LastClearedStage &&
                loaded.Retention.AldricExperience==claimed.Retention.AldricExperience &&
                sent.All(order=>loaded.Retention.Orders.Any(x=>x.Id==order.Id&&x.Claimed)),
                "Real save reload preserves stage progress and all three reward claims");
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
