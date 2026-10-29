#if UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Eldoria.Application;
using Eldoria.Domain;
using Eldoria.Infrastructure;
using Eldoria.UI.RoyalGold;
using Eldoria.UI.RoyalGold.V2;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using Object=UnityEngine.Object;

namespace Eldoria.ReferenceClean.Editor
{
    /// <summary>
    /// Full real-scene, non-production-save proof of the owner's global three-march rule.
    /// Does not reserve the editor; the workflow alone acquires the canonical Windows lease.
    /// </summary>
    [InitializeOnLoad]
    public static class MarchCapacityNativeCapture
    {
        const string Flag="Eldoria.GlobalThreeMarch.Native";
        const string Scene="Assets/Eldoria/ArtTests/ValoriaReferenceClean/Integration/Valoria_City_World_UI.unity";
        static string Output=>Path.GetFullPath(Environment.GetEnvironmentVariable("MARCH_CAPACITY_EVIDENCE") ??
            Path.Combine(UnityEngine.Application.dataPath,"../../docs/marches/native"));
        static readonly List<string> Checks=new List<string>();
        static MarchCapacityNativeCapture()=>EditorApplication.playModeStateChanged+=Entered;
        static void Need(bool good,string reason)
        {
            if(!good)throw new InvalidOperationException("GLOBAL_THREE_MARCH_NATIVE_FAIL: "+reason);
            Checks.Add(reason);
            Debug.Log("GLOBAL_THREE_MARCH_NATIVE_PASS: "+reason);
        }
        public static void Capture()
        {
            try
            {
                Environment.SetEnvironmentVariable("VALORIA_GAMEPLAY_PROBE","1");
                Directory.CreateDirectory(Output);
                EditorSceneManager.OpenScene(Scene);
                SessionState.SetBool(Flag,true);
                EditorApplication.EnterPlaymode();
            }
            catch(Exception e){Fail(e);}
        }
        static void Entered(PlayModeStateChange state)
        {
            if(state!=PlayModeStateChange.EnteredPlayMode||!SessionState.GetBool(Flag,false))return;
            SessionState.SetBool(Flag,false);
            EditorApplication.delayCall+=()=>{
                var host=Object.FindFirstObjectByType<ValoriaIntegratedWorld>();
                if(!host){Fail(new Exception("Canonical integrated Valoria host missing"));return;}
                host.StartCoroutine(Guard(host));
            };
        }
        static IEnumerator Guard(ValoriaIntegratedWorld host)
        {
            var probe=Exercise(host);
            while(true)
            {
                bool more;
                object step=null;
                try{more=probe.MoveNext();if(more)step=probe.Current;}
                catch(Exception e){Fail(e);yield break;}
                if(!more)yield break;
                yield return step;
            }
        }
        static GameCommand Command(LocalGateway g,string id,string kind,string target)
        {
            var snap=g.Snapshot();
            return new GameCommand(id,snap.PlayerId,kind,target,snap.Revision);
        }
        static void Image(ValoriaIntegratedWorld host,string name,int width,int height)
        {
            string path=Path.Combine(Output,name+".png");
            ValoriaIntegrationBuild.Capture(host,path,width,height,()=>{
                Canvas.ForceUpdateCanvases();
                Need(host.Hud!=null&&host.PlayerCamera!=null,
                    "One real HUD and active camera at "+name);
                if(name.Contains("active")||name.Contains("investigation-and"))
                {
                    var markers=host.Hud.GetComponentsInChildren<RectTransform>(false)
                        .Where(x=>x.name=="Aldric expedition standard"||x.name=="Expedition standard 2"||x.name=="Expedition standard 3").ToArray();
                    Need(markers.Length==3,"All three standards remain visible in actual capture "+name);
                    var canvas=host.Hud.GetComponent<Canvas>();
                    for(int i=0;i<markers.Length;i++)
                    {
                        var marker=markers[i];var graphic=marker.GetComponent<Graphic>();
                        Need(graphic&&marker.GetComponent<CanvasRenderer>(),
                            "Actual expedition has a native CanvasRenderer in "+name);
                        foreach(var caption in marker.GetComponentsInChildren<RoyalText>(false))
                            Need(caption.GetComponent<CanvasRenderer>()&&caption.font&&
                                caption.depth>graphic.depth&&!caption.canvasRenderer.cull&&
                                caption.preferredHeight<=caption.rectTransform.rect.height+1f,
                                "Real slot number and troop/deadline caption render completely in "+name);
                        Need((host.PlayerCamera.cullingMask&(1<<marker.gameObject.layer))!=0&&
                            graphic.depth>=0&&!graphic.canvasRenderer.cull,
                            "Actual world camera renders expedition standard "+i+" at "+name);
                        Debug.Log("ACTUAL_MARCH_STANDARD capture="+name+" slot="+i+" local="+marker.anchoredPosition+
                            " size="+marker.rect.size+" scale="+marker.localScale+" canvas_scale="+canvas.scaleFactor);
                        Need(marker.rect.width*marker.localScale.x*canvas.scaleFactor>=28f,
                            "Actual expedition icon stays at least 28px in "+name);
                        for(int j=0;j<i;j++)
                            Need(Vector2.Distance(marker.anchoredPosition,markers[j].anchoredPosition)*canvas.scaleFactor>=65f,
                                "Departing standards do not overlap in "+name);
                    }
                }
                if(name.StartsWith("march-composer"))
                {
                    var text=host.Hud.GetComponentsInChildren<RoyalText>(false).First(x=>x.text!=null&&x.text.Contains("MARCHAS ACTIVAS"));
                    Need(text.preferredHeight<=text.rectTransform.rect.height+1f,
                        "Every actual capacity/hero/slot line fits the March Room in "+name);
                }
            });
            Need(File.Exists(path)&&new FileInfo(path).Length>24000,
                "Actual camera PNG is present and nonempty: "+name);
            var tex=new Texture2D(2,2,TextureFormat.RGB24,false);
            try
            {
                Need(tex.LoadImage(File.ReadAllBytes(path))&&tex.width==width&&tex.height==height,
                    "PNG decodes to exact Unity camera dimensions: "+name);
                var colors=new HashSet<int>();
                var pixels=tex.GetPixels32();
                for(int i=0;i<pixels.Length;i+=47)
                {
                    var p=pixels[i];
                    colors.Add((p.r>>3)<<10|(p.g>>3)<<5|(p.b>>3));
                }
                Need(colors.Count>48,"World/UI pixels are not a blank test image: "+name);
            }
            finally{Object.Destroy(tex);}
        }
        static IEnumerator Exercise(ValoriaIntegratedWorld host)
        {
            for(int i=0;i<300&&!host.Ready;i++)yield return null;
            Need(host.Ready && host.City!=null && host.City.Probe,
                "Scene booted real gameplay with disposable-only save");
            Need(host.City.Clock is ValoriaReferenceCleanGameplay.TestClock,
                "Native proof owns the existing disposable UTC test clock");
            if(host.Guidance)host.Guidance.enabled=false;
            var hud=host.Hud;
            Need(hud!=null,"Real RoyalHUD instantiated");
            for(int i=0;i<50&&hud.NarrativeOpen;i++)
            {
                hud.Modal.Extra.onClick.Invoke();
                yield return null;
            }
            if(hud.Modal.gameObject.activeSelf)hud.Modal.Hide();
            hud.SelectBuilding("");
            var fresh=host.City.Gateway.Snapshot();
            fresh.SawmillLevel=1;
            fresh.BastionLevel=3;
            fresh.ChapterThree.LyraRecruited=true;
            fresh.BarracksLevel=1;
            fresh.Available=new ArmyRoster{ArcherT1=195};
            fresh.MarchConfigured=false;
            fresh.PreparedTroops=new ArmyRoster();
            fresh.March=new MarchState();
            fresh.ExtraMarches=new List<MarchState>();
            fresh.ChapterThree.InvestigationEndsUtcTicks=0;
            fresh.Retention.Orders=new List<ScoutOrder>();
            // Only this isolated probe path, never an owner's real or old save.
            new FileStateStore(host.City.SaveFile).Save(fresh);
            host.City.ReloadForProbe();
            yield return null;
            var gateway=host.City.Gateway;
            var snap=gateway.Snapshot();
            Need(snap.Available.Total==195 && MarchCapacityRules.Capacity(snap,"aldric")==60,
                "Live Gateway uses Barracks-I level-one hero troop capacity");
            Need(MarchCapacityRules.ActiveCount(snap,host.City.Clock.UtcTicks)==0,
                "No expedition active before actual dispatch");
            Need(host.TrySetWorld(true),"Actual world navigation is available");
            yield return new WaitForSecondsRealtime(.3f);
            Image(host,"three-marches-before-horizontal",1600,900);
            Need(gateway.Execute(Command(gateway,"native-config-lyra","ConfigureMarch","march-main:60:lyra")).Ok,
                "Prepare Lyra for primary march, independently of Aldric");
            Need(gateway.Execute(Command(gateway,"native-march-1","Gather","forest-valoria")).Ok,
                "First real forest gathering starts");
            Need(gateway.Execute(Command(gateway,"native-config-aldric","ConfigureMarch","march-main:60:aldric")).Ok,
                "Prepare Aldric for an additional march");
            Need(gateway.Execute(Command(gateway,"native-march-2","Gather","quarry-valoria")).Ok,
                "Second independent real quarry gathering starts");
            Need(gateway.Execute(Command(gateway,"native-march-3","Fight","corrupt-scout")).Ok,
                "Third independent real enemy march starts");
            ((ValoriaReferenceCleanGameplay.TestClock)host.City.Clock).Step(1);
            snap=gateway.Snapshot();
            Need(MarchCapacityRules.ActiveCount(snap,host.City.Clock.UtcTicks)==3 &&
                snap.ExtraMarches.Count==2,
                "Exactly three saved active world marches in one authoritative Gateway");
            Need(snap.Available.Total==15 && snap.March.Troops.Total==60 &&
                snap.ExtraMarches[0].Troops.Total==60 && snap.ExtraMarches[1].Troops.Total==60,
                "Three distinct sixty-troop detachments reserve 180 of 195 trained units");
            Need(!gateway.Execute(Command(gateway,"native-reject-4","Gather","forest-valoria")).Ok,
                "Fourth expedition is rejected despite fifteen available troops");
            yield return new WaitForSecondsRealtime(.3f);
            var marchDisplay=host.GetComponent<ValoriaMarchPresentation>();
            Need(marchDisplay!=null,"Authoritative world-march route presenter is attached");
            Need(hud.GetComponentsInChildren<ValoriaMarchRouteGraphic>(true).Length>=3,
                "Actual HUD owns three distinct route graphics");
            Need(hud.GetComponentsInChildren<RectTransform>(true).Count(x=>
                x.name=="Expedition standard 2"||x.name=="Expedition standard 3")==2,
                "Two new real expedition markers exist alongside canonical primary marker");
            Image(host,"three-marches-active-horizontal",1600,900);
            Image(host,"three-marches-active-vertical",900,1600);
            Image(host,"three-marches-active-phone",390,844);
            Need(host.TrySetWorld(false),"City can be inspected while three armies are away");
            var aldric=host.GetComponent<Eldoria.AldricCity.AldricCityRoaming>();
            for(int i=0;i<300&&(aldric==null||!aldric.ApprovedVisualReady);i++)yield return null;
            yield return new WaitForSecondsRealtime(.4f);
            Need(aldric!=null&&aldric.ApprovedVisualReady&&!aldric.IsCityActorVisible&&
                snap.March.HeroId=="lyra"&&snap.ExtraMarches[0].HeroId=="aldric",
                "Actual Aldric is absent when only additional marches contain him");
            host.City.ReloadForProbe();
            yield return new WaitForSecondsRealtime(.4f);
            gateway=host.City.Gateway;
            Need(MarchCapacityRules.ActiveCount(gateway.Snapshot(),host.City.Clock.UtcTicks)==3&&
                !aldric.IsCityActorVisible,"Three in-flight armies and actual hero absence survive reload");
            var clock=(ValoriaReferenceCleanGameplay.TestClock)host.City.Clock;
            clock.Step(30);
            gateway.Advance();
            snap=gateway.Snapshot();
            Need(MarchCapacityRules.ActiveCount(snap,clock.UtcTicks)==0 &&
                snap.Available.Total==195,"All three real march returns complete offline without lost troops");
            int wood=snap.Resources.Wood,stone=snap.Resources.Stone;
            host.City.ReloadForProbe();
            yield return null;
            snap=host.City.Gateway.Snapshot();
            Need(snap.Available.Total==195 &&
                snap.Resources.Wood==wood && snap.Resources.Stone==stone &&
                snap.ExtraMarches.Count==2,
                "All three routes/returns survive save reload with no duplicated resources");
            Need(host.TrySetWorld(false),"Player can return safely to the canonical city");
            var manager=hud.GetComponent<RoyalGoldV3ManagementScreens>();
            Need(manager!=null,"Actual March Room management panel attached");
            manager.Open(RoyalGoldV3ManagementScreens.Destination.March);
            yield return new WaitForSecondsRealtime(.2f);
            Need(manager.IsOpen,"Actual capacity-limited march composer opens");
            Need(hud.GetComponentsInChildren<RoyalText>(true).Any(t=>
                t.text!=null&&t.text.Contains("MARCHAS ACTIVAS")),
                "Actual visible March Room shows global active-slot count");
            Need(hud.GetComponentsInChildren<RoyalText>(true).Any(t=>
                t.text!=null&&t.text.Contains("CAPACIDAD DE")),
                "Real march room shows current hero-dependent capacity, not a decorative number");
            var sliders=manager.VisibleRoot.GetComponentsInChildren<Slider>(true);
            Need(sliders.Length==1&&Mathf.Approximately(sliders[0].maxValue,
                    MarchCapacityRules.Capacity(host.City.Gateway.Snapshot(),"aldric")),
                "The actual touch slider cannot select more troops than Aldric can command");
            Image(host,"march-composer-horizontal",1600,900);
            Image(host,"march-composer-vertical",900,1600);
            manager.Close();
            gateway=host.City.Gateway;
            Need(gateway.Execute(Command(gateway,"native-investigate","InvestigateFissure","fissure-valoria")).Ok,
                "Actual investigation starts in one global slot");
            Need(gateway.Execute(Command(gateway,"native-mixed-world1","Gather","forest-valoria")).Ok&&
                gateway.Execute(Command(gateway,"native-mixed-world2","Gather","quarry-valoria")).Ok,
                "Investigation allows both other global march slots");
            Need(!gateway.Execute(Command(gateway,"native-mixed-fourth","Fight","corrupt-scout")).Ok,
                "Mixed fourth operation is rejected");
            Need(host.TrySetWorld(true),"Mixed-operation world can be inspected");
            yield return new WaitForSecondsRealtime(.4f);
            Need(hud.GetComponentsInChildren<RectTransform>(false).Count(x=>
                x.name=="Aldric expedition standard"||x.name=="Expedition standard 2"||x.name=="Expedition standard 3")==3,
                "Investigation and BOTH world armies have active route markers");
            Image(host,"investigation-and-two-marches-horizontal",1600,900);
            Image(host,"investigation-and-two-marches-phone",390,844);
            File.WriteAllText(Path.Combine(Output,"result.txt"),
                "GLOBAL_THREE_MARCH_NATIVE_PASS\n"+
                "captured=8\n"+
                string.Join("\n",Checks)+"\n"+
                "owner-save-modified=false\n");
            Debug.Log("GLOBAL_THREE_MARCH_NATIVE_PASS");
            EditorApplication.Exit(0);
        }
        static void Fail(Exception error)
        {
            Directory.CreateDirectory(Output);
            File.WriteAllText(Path.Combine(Output,"failure.txt"),error.ToString());
            Debug.LogException(error);
            EditorApplication.Exit(1);
        }
    }
}
#endif
