#if UNITY_EDITOR
using System;
using System.Collections;
using System.IO;
using System.Linq;
using Eldoria.Domain;
using Eldoria.Infrastructure;
using Eldoria.ReferenceClean;
using Eldoria.ReferenceClean.Editor;
using Eldoria.UI.RoyalGold.V2;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Object=UnityEngine.Object;

namespace Eldoria.Editor
{
    [InitializeOnLoad]
    public static class EldoriaV41NativeCapture
    {
        const string Flag="Eldoria.V41.Native";
        static string Output=>Path.GetFullPath(Environment.GetEnvironmentVariable("ELDORIA_V41_EVIDENCE")??"v41-native");
        static EldoriaV41NativeCapture()=>EditorApplication.playModeStateChanged+=Entered;
        static void Need(bool good,string reason){if(!good)throw new InvalidOperationException(reason);Debug.Log("V41_CHECK "+reason);}
        public static void Capture()
        {
            try
            {
                Environment.SetEnvironmentVariable("VALORIA_GAMEPLAY_PROBE","1");
                Directory.CreateDirectory(Output);
                EditorSceneManager.OpenScene("Assets/Eldoria/ArtTests/ValoriaReferenceClean/Integration/Valoria_City_World_UI.unity");
                SessionState.SetBool(Flag,true);EditorApplication.EnterPlaymode();
            }
            catch(Exception e){Fail(e);}
        }
        static void Entered(PlayModeStateChange state)
        {
            if(state!=PlayModeStateChange.EnteredPlayMode||!SessionState.GetBool(Flag,false))return;
            SessionState.SetBool(Flag,false);
            EditorApplication.delayCall+=()=>{
                var host=Object.FindFirstObjectByType<ValoriaIntegratedWorld>();
                if(!host){Fail(new Exception("Canonical host missing"));return;}
                host.StartCoroutine(Guard(host));
            };
        }
        static IEnumerator Guard(ValoriaIntegratedWorld host)
        {
            var probe=Exercise(host);
            while(true)
            {
                bool more;object value=null;
                try{more=probe.MoveNext();if(more)value=probe.Current;}
                catch(Exception e){Fail(e);yield break;}
                if(!more)yield break;yield return value;
            }
        }
        static void Shot(ValoriaIntegratedWorld host,string name,int width,int height)
        {
            var management=host.Hud.GetComponent<RoyalGoldV3ManagementScreens>();
            var mailbox=host.Hud.GetComponent<RoyalGoldV41MailboxScreen>();
            ValoriaIntegrationBuild.Capture(host,Path.Combine(Output,name+".png"),width,height,()=>{
                if(name.Contains("small-phone"))
                {
                    // Exercise the actual production height-based scaler,
                    // not merely the older 800-unit capture convention.
                    host.Hud.GetComponent<Canvas>().scaleFactor=height/900f;
                    Canvas.ForceUpdateCanvases();
                }
                management.SynchronizeLayout();mailbox.SynchronizeLayout();Canvas.ForceUpdateCanvases();
                var heroes=host.Hud.GetComponent<Eldoria.HeroesUI.HeroesApprovedDestination>();
                if(heroes&&heroes.IsOpen){heroes.Refresh();Canvas.ForceUpdateCanvases();}
                if(management.IsOpen)ValidateMilitaryLayout(management,name);
                // Render again after mailbox reflow inside the canonical off-screen target.
                host.PlayerCamera.Render();host.PlayerCamera.Render();
                RenderTexture.active=host.PlayerCamera.targetTexture;
                var pixels=new Texture2D(width,height,TextureFormat.RGB24,false);
                try{pixels.ReadPixels(new Rect(0,0,width,height),0,0);pixels.Apply();File.WriteAllBytes(Path.Combine(Output,name+".png"),pixels.EncodeToPNG());}
                finally{Object.Destroy(pixels);}
                Need(host.Hud.GetComponentsInChildren<Canvas>(true).Length==1,"One canonical Canvas "+name);
                foreach(var image in host.Hud.GetComponentsInChildren<Image>(false).Where(x=>x.name.StartsWith("Approved source /")))
                    Need(image.sprite!=null,"Source texture loaded "+image.name);
            });
            Need(new FileInfo(Path.Combine(Output,name+".png")).Length>15000,"Real nonempty capture "+name);
        }
        static Rect Bounds(RectTransform rect)
        {
            var corners=new Vector3[4];rect.GetWorldCorners(corners);
            return Rect.MinMaxRect(corners[0].x,corners[0].y,corners[2].x,corners[2].y);
        }
        static void ValidateMilitaryLayout(RoyalGoldV3ManagementScreens screen,string shot)
        {
            var labels=screen.VisibleRoot.GetComponentsInChildren<Text>();
            var title=labels.First(t=>t.name=="Destination title");
            var subtitle=labels.First(t=>t.name=="Royal gold description");
            Need(title.preferredHeight<=title.rectTransform.rect.height+1,
                "Full destination title fits its actual text area "+shot+" (needed "+title.preferredHeight+", available "+title.rectTransform.rect.height+")");
            Need(!Bounds(title.rectTransform).Overlaps(Bounds(subtitle.rectTransform)),
                "Destination title and subtitle never overlap "+shot);
            foreach(var button in screen.VisibleRoot.GetComponentsInChildren<Button>()
                .Where(b=>b.transform.parent==title.transform.parent&&b.gameObject.activeInHierarchy))
                Need(!Bounds((RectTransform)button.transform).Overlaps(Bounds(title.rectTransform))&&
                     !Bounds((RectTransform)button.transform).Overlaps(Bounds(subtitle.rectTransform)),
                     "Header navigation does not cover title or subtitle "+button.name+" "+shot);
            if(screen.VisibleRoot.rect.height>screen.VisibleRoot.rect.width||shot.Contains("small-phone"))
                foreach(var label in labels.Where(t=>!string.IsNullOrWhiteSpace(t.text)))
                {
                    var canvas=screen.GetComponentInParent<Canvas>();
                    float physicalScale=canvas.scaleFactor;
                    for(var parent=label.transform;parent&&parent!=canvas.transform;parent=parent.parent)
                        physicalScale*=Mathf.Abs(parent.localScale.x);
                    Need(label.fontSize*physicalScale>=12,
                        "Small-phone text is at least twelve actual pixels "+label.name);
                    Need(label.preferredHeight<=label.rectTransform.rect.height+1,
                        "Small-phone complete label fits "+label.name+" (needed "+label.preferredHeight+", available "+label.rectTransform.rect.height+")");
                }
            if(shot=="training-small-phone-landscape")
            {
                var profile=labels.First(t=>t.name=="Unit profile and composition");
                var tier=screen.VisibleRoot.GetComponentsInChildren<Button>().First(b=>b.name=="Select archer tier");
                Need(!Bounds(profile.rectTransform).Overlaps(Bounds((RectTransform)tier.transform)),
                    "Compact training profile is not covered by the tier selector");
            }
            var quantity=labels.First(t=>t.name=="Authoritative quantity");
            Need(quantity.rectTransform.rect.width>=36,
                "Quantity remains legible between real minus and plus controls "+shot);
        }
        static IEnumerator Exercise(ValoriaIntegratedWorld host)
        {
            for(int i=0;i<300&&!host.Ready;i++)yield return null;
            Need(host.Ready&&host.City.Probe,"Canonical disposable gameplay only");
            var archer=Resources.Load<Texture2D>("EldoriaV41/training-archer");
            Need(archer!=null&&archer.width==184&&archer.height==476,"Original source aspect survives Unity texture import");
            if(host.Guidance)host.Guidance.enabled=false;
            for(int i=0;i<50&&host.Hud.NarrativeOpen;i++){host.Hud.Modal.Extra.onClick.Invoke();yield return null;}
            if(host.Hud.Modal.gameObject.activeSelf)host.Hud.Modal.Hide();
            host.Hud.SelectBuilding("");
            var state=host.City.Gateway.Snapshot();
            state.BastionLevel=4;state.BarracksLevel=4;state.SawmillLevel=2;
            state.ChapterThree.LyraRecruited=true;
            state.Available=new ArmyRoster{ArcherT1=200,ArcherT2=80};
            state.Resources=new ResourceWallet{Wood=9000,Stone=6000,Food=4000};
            state.MarchConfigured=false;state.March=new MarchState();state.ExtraMarches.Clear();
            state.Retention.Orders.Clear();state.ChapterThree.InvestigationEndsUtcTicks=0;
            new FileStateStore(host.City.SaveFile).Save(state);host.City.ReloadForProbe();yield return null;
            var screen=host.Hud.GetComponent<RoyalGoldV3ManagementScreens>();
            screen.Open(RoyalGoldV3ManagementScreens.Destination.Training);yield return null;
            Shot(host,"training-reference-ratio",554,542);
            Shot(host,"training-phone",900,1600);Shot(host,"training-small-phone",390,844);Shot(host,"training-small-phone-landscape",844,390);Shot(host,"training-landscape",1600,900);
            Need(screen.VisibleAction.interactable,"Training command is actionable");
            screen.VisibleAction.onClick.Invoke();
            Need(host.City.Gateway.Snapshot().PendingRecruitArchers>0,"Actual training reserves recruits");
            screen.Open(RoyalGoldV3ManagementScreens.Destination.March);yield return null;
            Need(!screen.VisibleRoot.GetComponentsInChildren<Text>().Any(t=>t.text=="APOYO · SIR ALDRIC"),"Empty saved slots never display invented Aldric companions");
            var controls=screen.VisibleRoot.GetComponentsInChildren<Button>();
            controls.First(b=>b.name=="Hero slot 1").onClick.Invoke();
            controls.First(b=>b.name=="Edit march by percent").onClick.Invoke();
            screen.VisibleRoot.GetComponentInChildren<Slider>().value=50;
            Need(screen.SelectedQuantity==60,"Real percent slider keeps 120 troops and assigns 50 percent to T1");
            Shot(host,"march-reference-ratio",941,1672);Shot(host,"march-landscape",1600,900);Shot(host,"march-small-phone",390,844);Shot(host,"march-small-phone-landscape",844,390);
            Need(screen.VisibleAction.interactable,"Actual march composer actionable");
            screen.VisibleAction.onClick.Invoke();
            var composed=host.City.Gateway.Snapshot();
            Need(composed.MarchConfigured&&composed.PreparedTroops.ArcherT1==60&&composed.PreparedTroops.ArcherT2==60,
                "Mixed-tier percentage composition persisted by real gateway");
            Need(composed.PreparedHeroIds[0]=="aldric"&&composed.PreparedHeroIds[1]=="lyra"&&string.IsNullOrEmpty(composed.PreparedHeroIds[2]),
                "Actual support selection persists only recruited heroes");
            screen.Close();yield return null;
            var gateway=host.City.Gateway;var snap=gateway.Snapshot();
            Need(gateway.Execute(new Eldoria.Application.GameCommand("v41-native-battle",snap.PlayerId,"Fight","corrupt-scout",snap.Revision)).Ok,"Real battle departure");
            ((ValoriaReferenceCleanGameplay.TestClock)host.City.Clock).Step(30);gateway.Advance();yield return null;
            Need(gateway.Snapshot().Mailbox.List("informes").Count==1,"Real result creates one report");
            var mailbox=host.Hud.GetComponent<RoyalGoldV41MailboxScreen>();mailbox.Refresh();yield return null;
            Need(mailbox.EntryButton&&mailbox.EntryButton.gameObject.activeInHierarchy,"Mailbox shortcut is visible in the actual HUD");
            Need(EventSystem.current!=null,"Canonical EventSystem handles pointer input");
            var entry=(RectTransform)mailbox.EntryButton.transform;
            var canvas=host.Hud.GetComponent<Canvas>();
            var point=RectTransformUtility.WorldToScreenPoint(canvas.renderMode==RenderMode.ScreenSpaceOverlay?null:canvas.worldCamera,entry.TransformPoint(entry.rect.center));
            var hits=new System.Collections.Generic.List<RaycastResult>();
            EventSystem.current.RaycastAll(new PointerEventData(EventSystem.current){position=point},hits);
            Need(hits.Count>0&&hits[0].gameObject.GetComponentInParent<Button>()==mailbox.EntryButton,"Mailbox entry receives the top actual pointer raycast");
            Shot(host,"mailbox-entry-city",900,1600);
            mailbox.EntryButton.onClick.Invoke();yield return null;
            Need(mailbox.IsOpen,"Actual HUD shortcut opens mailbox");
            Shot(host,"mailbox-reference-ratio",1169,1345);Shot(host,"mailbox-phone",900,1600);Shot(host,"mailbox-landscape",1600,900);
            var row=host.Hud.GetComponentsInChildren<Button>().First(x=>x.name=="Buzón / mensaje 0");row.onClick.Invoke();yield return null;
            Need(gateway.Snapshot().Mailbox.Unread("informes")==0,"Opening actual message marks it read");
            Shot(host,"mailbox-real-report",900,1600);
            int wood=gateway.Snapshot().Resources.Wood;
            host.Hud.GetComponentsInChildren<Button>().First(x=>x.name=="Eliminar").onClick.Invoke();
            Need(gateway.Snapshot().Mailbox.List("informes").Count==0&&gateway.Snapshot().Resources.Wood==wood,"Delete neither replays battle nor pays again");
            host.City.ReloadForProbe();yield return null;
            Need(host.City.Gateway.Snapshot().Mailbox.List("informes").Count==0,"Deleted battle remains deleted after real file reload");
            var heroes=host.Hud.GetComponent<Eldoria.HeroesUI.HeroesApprovedDestination>();
            Need(heroes!=null,"Heroes destination is mounted on the existing royal HUD");
            mailbox.Close();screen.Open(RoyalGoldV3ManagementScreens.Destination.Training);
            var heroEntry=screen.VisibleRoot.GetComponentsInChildren<Button>().First(b=>b.name=="Open canonical heroes");
            Need(heroEntry.interactable,"Narratively unlocked heroes entry is actionable");
            heroEntry.onClick.Invoke();yield return null;
            Need(heroes.IsOpen,"Real military navigation opens heroes hall");
            string saveBeforeHeroes=File.ReadAllText(host.City.SaveFile);
            Shot(host,"heroes-salon-landscape",1600,900);Shot(host,"heroes-salon-phone",900,1600);
            Need(heroes.OpenHero("aldric"),"Actual unlocked Aldric can be selected");
            float heroDeadline=Time.realtimeSinceStartup+90;
            while(!heroes.ReadModelRoot("aldric")&&Time.realtimeSinceStartup<heroDeadline)yield return null;
            Need(heroes.ReadModelRoot("aldric")!=null,"Approved Aldric geometry is actually available for the profile");
            foreach(var page in new[]{Eldoria.HeroesUI.HeroesApprovedDestination.Page.Resumen,
                Eldoria.HeroesUI.HeroesApprovedDestination.Page.Habilidades,
                Eldoria.HeroesUI.HeroesApprovedDestination.Page.Talentos,
                Eldoria.HeroesUI.HeroesApprovedDestination.Page.Equipamiento})
            {
                heroes.Select(page);yield return null;
                Shot(host,"heroes-aldric-"+page+"-landscape",1600,900);
                Shot(host,"heroes-aldric-"+page+"-phone",900,1600);
            }
            Need(heroes.OpenHero("lyra"),"Narratively recruited Lyra opens her own profile");
            Need(heroes.CurrentAuthoritativeHero().Value.Portrait!=null,
                "Lyra uses her exact approved portrait rather than missing art or Aldric");
            Shot(host,"heroes-lyra-resumen-landscape",1600,900);Shot(host,"heroes-lyra-resumen-phone",900,1600);
            Need(File.ReadAllText(host.City.SaveFile)==saveBeforeHeroes,
                "All six hero screens preserve the authoritative save byte for byte");
            heroes.Close();yield return null;
            File.WriteAllText(Path.Combine(Output,"result.json"),"{\"functional\":\"PASS\",\"visual_review\":\"REQUIRED\",\"art_approved\":false}");
            Debug.Log("ELDORIA_V41_NATIVE_PASS");EditorApplication.Exit(0);
        }
        static void Fail(Exception e){Debug.LogException(e);Directory.CreateDirectory(Output);File.WriteAllText(Path.Combine(Output,"failure.txt"),e.ToString());EditorApplication.Exit(1);}
    }
}
#endif
