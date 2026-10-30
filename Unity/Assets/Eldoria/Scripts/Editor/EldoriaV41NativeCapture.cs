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
                management.SynchronizeLayout();mailbox.SynchronizeLayout();Canvas.ForceUpdateCanvases();
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
        static IEnumerator Exercise(ValoriaIntegratedWorld host)
        {
            for(int i=0;i<300&&!host.Ready;i++)yield return null;
            Need(host.Ready&&host.City.Probe,"Canonical disposable gameplay only");
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
            Shot(host,"training-phone",900,1600);Shot(host,"training-landscape",1600,900);
            Need(screen.VisibleAction.interactable,"Training command is actionable");
            screen.VisibleAction.onClick.Invoke();
            Need(host.City.Gateway.Snapshot().PendingRecruitArchers>0,"Actual training reserves recruits");
            screen.Open(RoyalGoldV3ManagementScreens.Destination.March);yield return null;
            Shot(host,"march-reference-ratio",941,1672);Shot(host,"march-landscape",1600,900);
            Need(screen.VisibleAction.interactable,"Actual march composer actionable");
            screen.VisibleAction.onClick.Invoke();
            Need(host.City.Gateway.Snapshot().MarchConfigured,"Composition persisted by real gateway");
            screen.Close();yield return null;
            var gateway=host.City.Gateway;var snap=gateway.Snapshot();
            Need(gateway.Execute(new Eldoria.Application.GameCommand("v41-native-battle",snap.PlayerId,"Fight","corrupt-scout",snap.Revision)).Ok,"Real battle departure");
            ((ValoriaReferenceCleanGameplay.TestClock)host.City.Clock).Step(30);gateway.Advance();yield return null;
            Need(gateway.Snapshot().Mailbox.List("informes").Count==1,"Real result creates one report");
            var mailbox=host.Hud.GetComponent<RoyalGoldV41MailboxScreen>();mailbox.Open();yield return null;
            Shot(host,"mailbox-reference-ratio",1169,1345);Shot(host,"mailbox-phone",900,1600);Shot(host,"mailbox-landscape",1600,900);
            var row=host.Hud.GetComponentsInChildren<Button>().First(x=>x.name=="Buzón / mensaje 0");row.onClick.Invoke();yield return null;
            Need(gateway.Snapshot().Mailbox.Unread("informes")==0,"Opening actual message marks it read");
            Shot(host,"mailbox-real-report",900,1600);
            int wood=gateway.Snapshot().Resources.Wood;
            host.Hud.GetComponentsInChildren<Button>().First(x=>x.name=="Eliminar").onClick.Invoke();
            Need(gateway.Snapshot().Mailbox.List("informes").Count==0&&gateway.Snapshot().Resources.Wood==wood,"Delete neither replays battle nor pays again");
            host.City.ReloadForProbe();yield return null;
            Need(host.City.Gateway.Snapshot().Mailbox.List("informes").Count==0,"Deleted battle remains deleted after real file reload");
            File.WriteAllText(Path.Combine(Output,"result.json"),"{\"functional\":\"PASS\",\"visual_review\":\"REQUIRED\",\"art_approved\":false}");
            Debug.Log("ELDORIA_V41_NATIVE_PASS");EditorApplication.Exit(0);
        }
        static void Fail(Exception e){Debug.LogException(e);Directory.CreateDirectory(Output);File.WriteAllText(Path.Combine(Output,"failure.txt"),e.ToString());EditorApplication.Exit(1);}
    }
}
#endif
