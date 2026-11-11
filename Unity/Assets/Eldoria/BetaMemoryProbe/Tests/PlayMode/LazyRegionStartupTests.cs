#if UNITY_EDITOR
using System;
using System.Collections;
using System.Linq;
using System.Reflection;
using Eldoria.Application;
using Eldoria.Domain;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace Eldoria.BetaMemory.Tests
{
    // Real scene, real gateway, disposable probe save. NOT physical iPhone QA.
    public sealed class LazyRegionStartupTests
    {
        const BindingFlags PublicInstance=BindingFlags.Instance|BindingFlags.Public;
        static object Member(object item,string name)
        {
            Assert.That(item,Is.Not.Null,"Missing target: "+name);
            var type=item.GetType();
            var field=type.GetField(name,PublicInstance);
            if(field!=null)return field.GetValue(item);
            var property=type.GetProperty(name,PublicInstance);
            Assert.That(property,Is.Not.Null,"Missing property: "+name);
            return property.GetValue(item);
        }

        static object Call(object item,string method,params object[] args)
        {
            Assert.That(item,Is.Not.Null);
            var candidate=item.GetType().GetMethods(PublicInstance)
                .FirstOrDefault(m=>m.Name==method&&m.GetParameters().Length==args.Length);
            Assert.That(candidate,Is.Not.Null,"Missing method: "+method);
            return candidate.Invoke(item,args);
        }

        static Type FindType(string name)
        {
            return AppDomain.CurrentDomain.GetAssemblies()
                .Select(a=>a.GetType(name,false))
                .FirstOrDefault(t=>t!=null);
        }

        [UnityTest]
        public IEnumerator RealWorldStartsOnFirstEligibleEntryWithoutSecondGatewayOrNewHUD()
        {
            var previousProbe=Environment.GetEnvironmentVariable("VALORIA_GAMEPLAY_PROBE");
            var hostType=FindType("Eldoria.ReferenceClean.ValoriaIntegratedWorld");
            Assert.That(hostType,Is.Not.Null,"Integrated host must exist in the real player assembly");
            var optIn=hostType.GetProperty("P0LazyWorldEditorTestOverride",
                BindingFlags.Static|BindingFlags.Public);
            Assert.That(optIn,Is.Not.Null,"Editor-only lazy flag must remain testable");
            bool previousFlag=(bool)optIn.GetValue(null);
            Environment.SetEnvironmentVariable("VALORIA_GAMEPLAY_PROBE","1");
            optIn.SetValue(null,true);
            try
            {
                SceneManager.LoadScene("Valoria_City_World_UI");
                MonoBehaviour host=null,adapter=null;
                float cityStartupStarted=Time.realtimeSinceStartup;
                // Under batchmode, 240 rendered frames can elapse in a fraction of
                // a second. Use wall-clock bounds, not an arbitrary frame count.
                while(Time.realtimeSinceStartup-cityStartupStarted<45f)
                {
                    yield return null;
                    host=UnityEngine.Object.FindObjectsByType<MonoBehaviour>(
                        FindObjectsSortMode.None).FirstOrDefault(x=>x&&x.GetType()==hostType);
                    adapter=UnityEngine.Object.FindObjectsByType<MonoBehaviour>(
                        FindObjectsSortMode.None).FirstOrDefault(x=>x&&
                        x.GetType().Name=="RegionOneCanonicalLandscape");
                    if(host&&adapter&&(bool)Member(host,"Ready")&&Member(host,"Hud")!=null)break;
                }
                Assert.That(host,Is.Not.Null);
                Assert.That(adapter,Is.Not.Null,"Canonical adapter must attach before first entry");
                Assert.That((bool)Member(host,"Ready"),Is.True,"City HUD must become ready first");
                Assert.That((bool)Member(adapter,"IsReady"),Is.False);
                Assert.That((bool)Member(adapter,"IsLoading"),Is.False);
                var world=(GameObject)Member(host,"WorldRoot");
                var camera=(Camera)Member(host,"WorldCamera");
                var hud=Member(host,"Hud") as Component;
                var city=Member(host,"City");
                Assert.That(world,Is.Not.Null);
                Assert.That(hud,Is.Not.Null);
                Assert.That(world.transform.Find("Region 1 canonical continuous geography"),Is.Null,
                    "Opt-in lazy path must not allocate Region 1 during city startup");
                Assert.That((bool)Member(host,"InWorld"),Is.False);
                Assert.That(world.activeInHierarchy,Is.False);
                Assert.That(camera.enabled,Is.False);
                var gateway=(LocalGateway)Member(city,"Gateway");
                Assert.That((bool)Member(city,"Probe"),Is.True,"Never test against a real owner save");
                // Real revision-guarded gameplay command on a disposable save,
                // not a fabricated in-memory nickname. Verify the same saved
                // name survives real terrain loading and disk deserialization.
                var original=gateway.Snapshot();
                if(string.IsNullOrWhiteSpace(original.PlayerName))
                {
                    var rename=gateway.Execute(new GameCommand(
                        "lazy-world-native-nickname-"+Guid.NewGuid().ToString("N"),
                        original.PlayerId,"ChoosePlayerName","Beta QA",original.Revision));
                    Assert.That(rename.Ok,Is.True,"Real nickname command must commit: "+rename.Message);
                }
                var before=gateway.Snapshot();
                Assert.That(before.PlayerName,Is.Not.Empty,
                    "Nickname must be genuinely saved before Region 1 starts");

                // Preserve the existing sawmill progression gate; do not bypass it.
                if(!(before.SawmillLevel>0||before.Resources.Wood<SliceRules.SawmillWoodCost))
                {
                    var offer=Member(Member(Call(host,"Read","sawmill",false),"Building"),"Action");
                    Assert.That(offer,Is.Not.Null,"Only a genuine gateway Build offer can unlock World");
                    Assert.That(Member(offer,"Kind"),Is.EqualTo("Build"));
                    Call(host,"Submit",offer);
                    Call(Member(city,"Clock"),"Step",SliceRules.SawmillBuildSeconds+1);
                    Call(gateway,"Advance");
                    yield return null;
                }
                Assert.That((bool)Call(host,"TrySetWorld",true),Is.False,
                    "Incomplete Region 1 must never appear through the world camera");
                Assert.That((bool)Member(adapter,"IsLoading"),Is.True);
                Assert.That((bool)Call(host,"TrySetWorld",true),Is.False,
                    "Repeated tap must join the same in-flight load, not install twice");
                Assert.That((bool)Member(host,"InWorld"),Is.False);
                Assert.That(world.activeInHierarchy,Is.False);
                Assert.That(camera.enabled,Is.False);

                float worldLoadStarted=Time.realtimeSinceStartup;
                while(Time.realtimeSinceStartup-worldLoadStarted<120f)
                {
                    yield return null;
                    if((bool)Member(adapter,"IsReady")&&(bool)Member(host,"InWorld"))break;
                }
                Debug.Log("ELDORIA_LAZY_REAL_WORLD_NATIVE_LOAD_ELAPSED_SECONDS="+
                    (Time.realtimeSinceStartup-worldLoadStarted).ToString("F3",System.Globalization.CultureInfo.InvariantCulture));
                Assert.That((bool)Member(adapter,"IsReady"),Is.True,
                    "Real staged terrain, resources and population must all finish");
                Assert.That((bool)Member(host,"InWorld"),Is.True,
                    "Eligible first request must enter full world automatically");
                Assert.That(world.activeInHierarchy,Is.True);
                Assert.That(camera.enabled,Is.True);
                Assert.That(world.transform.Find("Region 1 canonical continuous geography"),Is.Not.Null);
                Assert.That(world.transform.Cast<Transform>()
                    .Count(x=>x.name=="Region 1 canonical continuous geography"),Is.EqualTo(1),
                    "There must be a single real Region 1 owner");
                Assert.That(Member(host,"Hud"),Is.SameAs(hud),"Never replace 19-overlay HUD");
                Assert.That(Member(Member(host,"City"),"Gateway"),Is.SameAs(gateway),
                    "Do not recreate LocalGateway or reset player progress while loading");
                var after=gateway.Snapshot();
                Assert.That(after.PlayerId,Is.EqualTo(before.PlayerId));
                Assert.That(after.BastionLevel,Is.EqualTo(before.BastionLevel));
                Assert.That(after.PlayerName,Is.EqualTo(before.PlayerName));
                Assert.That(after.WorldResourceLocations.Count(x=>x.TargetId.StartsWith("region1:")),
                    Is.EqualTo(48),"Canonical resources must be present, not an empty diagnostic map");
                Assert.That(after.WorldCreatureLocations.Count,Is.EqualTo(36),
                    "Actual combat population must survive deferred installation");
                // Exercise the same camera/root repeatedly. This must not allocate
                // a second Region 1, lose HUD/gateway or re-enter construction.
                for(int cycle=0;cycle<12;cycle++)
                {
                    Assert.That((bool)Call(host,"TrySetWorld",false),Is.True);
                    Assert.That((bool)Member(host,"InWorld"),Is.False);
                    Assert.That(world.activeInHierarchy,Is.False);
                    Assert.That(camera.enabled,Is.False);
                    Assert.That((bool)Call(host,"TrySetWorld",true),Is.True,
                        "Repeated entry must reuse existing world cycle "+cycle);
                    Assert.That(world.activeInHierarchy,Is.True);
                    Assert.That(camera.enabled,Is.True);
                    Assert.That(world.transform.Cast<Transform>()
                        .Count(x=>x.name=="Region 1 canonical continuous geography"),Is.EqualTo(1),
                        "No duplicate scene across world navigation cycles");
                    Assert.That(Member(host,"Hud"),Is.SameAs(hud));
                    Assert.That(Member(Member(host,"City"),"Gateway"),Is.SameAs(gateway));
                    yield return null;
                }
                Debug.Log("ELDORIA_LAZY_REAL_WORLD_NATIVE_12_REENTRY_CYCLES_COMPLETE");

                // A full Region 1 is not usable by testers if its real saved
                // population disappears after a reload. Reopen the disposable
                // on-disk probe save, retaining this HUD, WorldRoot and clock.
                var saved=gateway.Snapshot();
                var clock=Member(city,"Clock");
                Call(city,"ReloadForProbe");
                yield return new WaitForSecondsRealtime(.45f);
                var reloaded=(LocalGateway)Member(city,"Gateway");
                Assert.That(reloaded,Is.Not.SameAs(gateway),
                    "The reload must genuinely deserialize a fresh gateway");
                Assert.That(Member(city,"Clock"),Is.SameAs(clock),
                    "Construction deadlines cannot restart when the save is reopened");
                var restored=reloaded.Snapshot();
                Assert.That(restored.PlayerId,Is.EqualTo(saved.PlayerId));
                Assert.That(restored.PlayerName,Is.EqualTo(saved.PlayerName));
                Assert.That(restored.BastionLevel,Is.EqualTo(saved.BastionLevel));
                Assert.That(restored.SawmillLevel,Is.EqualTo(saved.SawmillLevel));
                Assert.That(restored.WorldResourceLocations.Count(x=>x.TargetId.StartsWith("region1:")),
                    Is.EqualTo(48),"Region resources must survive actual on-disk reload");
                Assert.That(restored.WorldCreatureLocations.Count,Is.EqualTo(36),
                    "World enemies must survive actual on-disk reload");
                Assert.That(Member(host,"Hud"),Is.SameAs(hud),
                    "Rebinding the saved gateway must never replace the pretester HUD");
                Assert.That(world.transform.Cast<Transform>()
                    .Count(x=>x.name=="Region 1 canonical continuous geography"),Is.EqualTo(1),
                    "Reopening the save cannot build another copy of the world");
                Assert.That((bool)Call(host,"TrySetWorld",false),Is.True);
                Assert.That((bool)Call(host,"TrySetWorld",true),Is.True,
                    "Saved Region 1 must remain immediately accessible after reload");
                Assert.That(Member(city,"Gateway"),Is.SameAs(reloaded));
                Assert.That(camera.enabled,Is.True);
            }
            finally
            {
                optIn.SetValue(null,previousFlag);
                Environment.SetEnvironmentVariable("VALORIA_GAMEPLAY_PROBE",previousProbe);
            }
        }
    }
}
#endif
