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
                for(int i=0;i<240;i++)
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
                var before=gateway.Snapshot();
                Assert.That((bool)Member(city,"Probe"),Is.True,"Never test against a real owner save");

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

                for(int i=0;i<1200;i++)
                {
                    yield return null;
                    if((bool)Member(adapter,"IsReady")&&(bool)Member(host,"InWorld"))break;
                }
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
                Assert.That((bool)Call(host,"TrySetWorld",false),Is.True);
                Assert.That((bool)Member(host,"InWorld"),Is.False);
                Assert.That((bool)Call(host,"TrySetWorld",true),Is.True,
                    "Second entry must reuse the already-built real world immediately");
                Assert.That(world.transform.Cast<Transform>()
                    .Count(x=>x.name=="Region 1 canonical continuous geography"),Is.EqualTo(1));
                Assert.That(Member(Member(host,"City"),"Gateway"),Is.SameAs(gateway));
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
