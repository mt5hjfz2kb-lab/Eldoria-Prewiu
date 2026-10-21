using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Eldoria.Application;
using Eldoria.Domain;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;
using Object=UnityEngine.Object;

namespace Eldoria.Tests
{
    public sealed class RoyalV3ManagementIntegrationTests
    {
        sealed class FakeClock:IClock
        {
            public long UtcTicks {get;set;} = new DateTime(2026,10,21,12,0,0,DateTimeKind.Utc).Ticks;
            public void Add(int seconds) {UtcTicks+=TimeSpan.FromSeconds(seconds).Ticks;}
        }
        sealed class Memory:IStateStore
        {
            public PlayerState Stored;
            public PlayerState Load()=>Stored;
            public void Save(PlayerState s){Stored=s;}
        }
        static object Member(object target,string name)
        {
            var t=target.GetType();
            var p=t.GetProperty(name,BindingFlags.Public|BindingFlags.Instance);
            if(p!=null)return p.GetValue(target);
            var f=t.GetField(name,BindingFlags.Public|BindingFlags.Instance);
            if(f!=null)return f.GetValue(target);
            throw new InvalidOperationException("Missing "+name+" on "+t);
        }
        static object Call(object target,string method,params object[] args)
        {
            var m=target.GetType().GetMethods(BindingFlags.Public|BindingFlags.Instance).
                FirstOrDefault(x=>x.Name==method&&x.GetParameters().Length==args.Length);
            Assert.That(m,Is.Not.Null,method+" missing");
            return m.Invoke(target,args);
        }
        static IEnumerator Boot(Action<Component,Component> received)
        {
            SceneManager.LoadScene("Valoria_City_World_UI");
            for(int frame=0;frame<180;frame++)
            {
                yield return null;
                var host=Object.FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None).
                    FirstOrDefault(x=>x&&x.GetType().Name=="ValoriaIntegratedWorld");
                if(!host || !(bool)Member(host,"Ready"))continue;
                var hud=Member(host,"Hud") as Component;
                if(hud){received(host,hud);yield break;}
            }
            Assert.Fail("Canonical scene/HUD did not boot");
        }

        [UnityTest]
        public IEnumerator FullScreenRoyalTrainingAndMarchesPreserveCameraAndDoNotWriteOwnerSave()
        {
            if(Environment.GetEnvironmentVariable("VALORIA_GAMEPLAY_PROBE")!="1")
                Assert.Ignore("Requires a disposable Unity native gameplay-probe save");
            string previousProfile=SliceContentProfiles.ActiveRuntimeProfile;
            SliceContentProfiles.SetRuntimeProfileOverride(SliceContentProfiles.OwnerIiiId);
            try
            {
                Component host=null,hud=null;
                yield return Boot((h,u)=>{host=h;hud=u;});
                var management=hud.GetComponent("RoyalGoldV3ManagementScreens");
                Assert.That(management,Is.Not.Null,"Integrated HUD must own V3 management navigator");
                Assert.That(Member(management,"IsOpen"),Is.False);
                var camera=Member(host,"PlayerCamera") as Camera;
                Assert.That(camera,Is.Not.Null);
                var position=camera.transform.position;
                float zoom=camera.orthographicSize;
                var state=new PlayerState{
                    BastionLevel=2,BarracksLevel=1,
                    Resources=new ResourceWallet{Wood=9000,Stone=9000},
                    Available=new ArmyRoster{ArcherT1=36}};
                var clock=new FakeClock();
                var store=new Memory{Stored=state};
                var fake=new LocalGateway(clock,store);
                // The destination is exercised against a disposable in-memory
                // canonical gateway, NEVER against the player's persisted save.
                Call(management,"Initialize",hud,fake,new Func<long>(()=>clock.UtcTicks),new Action(()=>{}));
                var destination=management.GetType().GetNestedType("Destination");
                Assert.That(destination,Is.Not.Null);
                // A fresh integrated boot may still be displaying Aldric's
                // real opening narrative. This test isolates management UI and
                // does not advance or alter the player's saved story.
                var guidance=Member(host,"Guidance") as Behaviour;
                if(guidance)guidance.enabled=false;
                var modal=Member(hud,"Modal") as Component;
                if(modal && modal.gameObject.activeInHierarchy)Call(modal,"Hide");
                // Exercise the actual RoyalHUD → Cuartel navigation event,
                // not a direct artificial call into the screen component.
                Call(hud,"SelectBuilding","barracks");
                yield return null;
                Canvas.ForceUpdateCanvases();
                Assert.That(Member(management,"IsOpen"),Is.True);
                var overlay=GameObject.Find("V3 / Management destination");
                Assert.That(overlay,Is.Not.Null);
                var background=overlay.GetComponent<Image>();
                Assert.That(background.color.a,Is.EqualTo(1f));
                Assert.That(background.raycastTarget,Is.True);
                var canvas=overlay.GetComponentInParent<Canvas>();
                Assert.That(canvas,Is.SameAs(hud.GetComponent<Canvas>()));
                var rect=overlay.GetComponent<RectTransform>();
                Assert.That(rect.anchorMin,Is.EqualTo(Vector2.zero));
                Assert.That(rect.anchorMax,Is.EqualTo(Vector2.one));
                var panel=GameObject.Find("V3 / Authoritative status and cost");
                Assert.That(panel,Is.Not.Null);
                var labels=panel.GetComponentsInChildren<Text>();
                Assert.That(labels.Any(x=>x.text.Contains("400")&&x.text.Contains("10:00")),
                    Is.True,"Real 20-T1 cost and time must appear in training");
                var pointer=new PointerEventData(EventSystem.current){position=
                    new Vector2(Screen.width*.5f,Screen.height*.5f)};
                var hits=new List<RaycastResult>();
                EventSystem.current.RaycastAll(pointer,hits);
                Assert.That(hits.Count,Is.GreaterThan(0));
                Assert.That(hits[0].gameObject.transform.IsChildOf(rect),Is.True,
                    "Management must be topmost and block any city gesture");
                Assert.That(camera.transform.position,Is.EqualTo(position));
                Assert.That(camera.orthographicSize,Is.EqualTo(zoom));

                var confirm=GameObject.Find("Confirm military command")?.GetComponent<Button>();
                Assert.That(confirm,Is.Not.Null);
                Assert.That(confirm.interactable,Is.True);
                confirm.onClick.Invoke();
                Assert.That(fake.Snapshot().PendingRecruitArchers,Is.EqualTo(20));
                Assert.That(fake.Snapshot().RecruitmentCompletesUtcTicks-clock.UtcTicks,
                    Is.EqualTo(TimeSpan.FromMinutes(10).Ticks));
                clock.Add(600);
                fake.Advance();
                yield return null;
                Assert.That(fake.Snapshot().Available.ArcherT1,Is.EqualTo(56));
                var switchAction=GameObject.Find("Switch destination")?.GetComponent<Button>();
                Assert.That(switchAction,Is.Not.Null,"V3 must navigate between destinations");
                switchAction.onClick.Invoke();
                yield return null;
                Assert.That(Member(management,"CurrentDestination").ToString(),Is.EqualTo("March"));
                var slider=GameObject.Find("Quantity slider")?.GetComponent<Slider>();
                Assert.That(slider,Is.Not.Null);
                slider.value=7;
                var composition=GameObject.Find("Training or expedition breakdown")?.GetComponent<Text>();
                Assert.That(composition.text,Does.Contain("7"));
                confirm=GameObject.Find("Confirm military command")?.GetComponent<Button>();
                confirm.onClick.Invoke();
                Assert.That(fake.Snapshot().PreparedTroops.ArcherT1,Is.EqualTo(7));
                Assert.That(fake.Snapshot().PreparedHeroId,Is.EqualTo("aldric"));
                Call(management,"Close");
                yield return null;
                Assert.That(Member(management,"IsOpen"),Is.False);
                Assert.That(camera.transform.position,Is.EqualTo(position));
                Assert.That(camera.orthographicSize,Is.EqualTo(zoom));
                Assert.That(store.Stored.SchemaVersion,Is.EqualTo(1));
            }
            finally {SliceContentProfiles.SetRuntimeProfileOverride(null);}
        }
    }
}
