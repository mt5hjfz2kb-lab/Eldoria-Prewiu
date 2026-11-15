#if UNITY_EDITOR
using System;
using System.Collections;
using System.IO;
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
            // Optional logic-only smoke mode for isolated CI when 51 unversioned,
            // previously audited Bastion III art files are unavailable on the
            // Windows runner. This is NEVER a release/candidate or visual gate.
            // The production test remains strict by default.
            bool logicOnly=Environment.GetEnvironmentVariable(
                "ELDORIA_WORLD_P0_LOGIC_ONLY_MISSING_ART")=="1";
            var previousLogAssertionMode=LogAssert.ignoreFailingMessages;
            var unexpectedErrors=new System.Collections.Generic.List<string>();
            int missingBastionArt=0;
            UnityEngine.Application.LogCallback callback=(message,stack,type)=>{
                if(type!=LogType.Error&&type!=LogType.Exception&&type!=LogType.Assert)return;
                if(type==LogType.Error&&message.StartsWith(
                    "BASTION_III_MISSING_ART BastionIII/",StringComparison.Ordinal)){
                    missingBastionArt++;return;
                }
                unexpectedErrors.Add(message);
            };
            if(logicOnly){
                LogAssert.ignoreFailingMessages=true;
                UnityEngine.Application.logMessageReceived+=callback;
                Debug.Log("ELDORIA_WORLD_P0_LOGIC_ONLY_PROBE_NOT_A_RELEASE_CERTIFICATE");
            }
            var previousProbe=Environment.GetEnvironmentVariable("VALORIA_GAMEPLAY_PROBE");
            var hostType=FindType("Eldoria.ReferenceClean.ValoriaIntegratedWorld");
            Assert.That(hostType,Is.Not.Null,"Integrated host must exist in the real player assembly");
            var optIn=hostType.GetProperty("P0LazyWorldEditorTestOverride",
                BindingFlags.Static|BindingFlags.Public);
            Assert.That(optIn,Is.Not.Null,"Editor-only lazy flag must remain testable");
            bool previousFlag=(bool)optIn.GetValue(null);
            var renderOptIn=hostType.GetProperty("P0EntryRenderBudgetEditorTestOverride",
                BindingFlags.Static|BindingFlags.Public);
            Assert.That(renderOptIn,Is.Not.Null);
            bool previousRenderFlag=(bool)renderOptIn.GetValue(null);
            var meshOptIn=hostType.GetProperty("P0StaticMeshCpuEditorTestOverride",BindingFlags.Static|BindingFlags.Public);
            Assert.That(meshOptIn,Is.Not.Null);
            bool previousMeshFlag=(bool)meshOptIn.GetValue(null);
            meshOptIn.SetValue(null,true);
            Environment.SetEnvironmentVariable("VALORIA_GAMEPLAY_PROBE","1");
            optIn.SetValue(null,true);
            renderOptIn.SetValue(null,true);
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
                var cityCamera=(Camera)Member(city,"PlayerCamera");

                var staticRoots=new[]{(GameObject)Member(city,"BastionOne"),(GameObject)Member(city,"BastionTwo"),
                    (GameObject)Member(host,"WorldBastionOne"),(GameObject)Member(host,"WorldBastionTwo")};
                var staticMeshes=staticRoots.SelectMany(root=>root.GetComponentsInChildren<MeshFilter>(true))
                    .Select(filter=>filter.sharedMesh).Where(mesh=>mesh).Distinct().ToArray();
                Assert.That(staticMeshes.Length,Is.EqualTo(4));
                Assert.That(staticMeshes.All(mesh=>!mesh.isReadable),Is.True,
                    "Static CPU copies must be retired before allocating Region 1");
                foreach(var root in staticRoots)
                    foreach(var collider in root.GetComponentsInChildren<MeshCollider>(true)){
                        var bounds=collider.bounds;bool hit=false;
                        // Use several real roof/courtyard positions: one arbitrary
                        // central ray could legitimately pass through a courtyard.
                        for(int x=1;x<5&&!hit;x++)for(int z=1;z<5&&!hit;z++){
                            var from=new Vector3(Mathf.Lerp(bounds.min.x,bounds.max.x,x/5f),bounds.max.y+5,
                                Mathf.Lerp(bounds.min.z,bounds.max.z,z/5f));
                            hit=collider.Raycast(new Ray(from,Vector3.down),out _,bounds.size.y+10);
                        }
                        if(collider.enabled&&collider.gameObject.activeInHierarchy)
                            Assert.That(hit,Is.True,"Static collider must keep real cooked hits after CPU retirement");
                    }
                bool originalCityMsaa=cityCamera.allowMSAA;
                bool originalWorldMsaa=camera.allowMSAA;
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
                Assert.That((bool)Call(host,"NavigateToObjective","forest-valoria"),Is.False,
                    "The actual quest navigation cannot expose an incomplete world");
                Assert.That((bool)Member(adapter,"IsLoading"),Is.True);
                Assert.That(cityCamera.allowMSAA,Is.False,
                    "City MSAA must be disabled before the deferred terrain build starts");
                Assert.That(camera.allowMSAA,Is.False);
                Assert.That(world.transform.Find("Region 1 canonical continuous geography"),Is.Null,
                    "Camera preparation must yield before allocating the canonical terrain");
                Assert.That((bool)Call(host,"NavigateToObjective","quarry-valoria"),Is.False,
                    "A second objective must replace the target in the same in-flight load");
                Assert.That((bool)Call(host,"TrySetWorld",false),Is.True,
                    "Returning to the city must cancel the pending entry and target");
                yield return null;
                Assert.That((bool)Member(host,"InWorld"),Is.False,
                    "A cancelled coroutine must never force the world open");
                Assert.That(cityCamera.allowMSAA,Is.False,
                    "Cancelling navigation must not restore costly buffers during the in-flight build");
                Assert.That((bool)Call(host,"NavigateToObjective","forest-valoria"),Is.False);
                Assert.That((bool)Call(host,"NavigateToObjective","quarry-valoria"),Is.False,
                    "A new request can rejoin the same background load after cancellation");
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
                Assert.That(cityCamera.allowMSAA,Is.False);
                Assert.That(camera.allowMSAA,Is.False);
                Assert.That((string)Member(host,"Selected"),Is.EqualTo("quarry-valoria"),
                    "First deferred quest entry must select its latest target without another tap");
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
                // Renderer allocation is a separate mobile-first safety invariant:
                // the canonical 48 resources and 36 enemies exist logically,
                // but off-screen 3D GLB copies must never all occupy memory.
                yield return new WaitForSecondsRealtime(2f);
                var visualTransforms=world.GetComponentsInChildren<Transform>(true);
                int resourceNear=visualTransforms.Count(x=>x.name.StartsWith(
                    "Production resource visual region1:",StringComparison.Ordinal));
                int resourceBillboards=visualTransforms.Count(x=>x.name.StartsWith(
                    "Production resource impostor region1:",StringComparison.Ordinal));
                int creatureModels=visualTransforms.Count(x=>x.name.StartsWith(
                    "Original Tripo ",StringComparison.Ordinal));
                Assert.That(resourceNear,Is.LessThanOrEqualTo(10),
                    "Resource GLB residency must follow the on-screen, bounded budget");
                Assert.That(creatureModels,Is.LessThanOrEqualTo(after.WorldCreatureLocations.Count),
                    "Each saved creature can retain at most one visible LOD, never a near/far pair");
                Assert.That(resourceBillboards,Is.GreaterThanOrEqualTo(48),
                    "All canonical resource nodes keep their lightweight regional artwork");
                Debug.Log("ELDORIA_WORLD_STREAMING_VISUALS resource_near="+resourceNear+
                    " resource_billboards="+resourceBillboards+
                    " creature_models="+creatureModels+
                    " allocated="+UnityEngine.Profiling.Profiler.GetTotalAllocatedMemoryLong()+
                    " managed="+GC.GetTotalMemory(false));

                // Actual Unity camera evidence is mandatory when the original
                // 51 Bastion III source assets are present. Never capture the
                // logic-only missing-art sandbox as artistic proof.
                if(!logicOnly&&Environment.GetEnvironmentVariable("ELDORIA_WORLD_P0_CAPTURE_REAL_ART")=="1"){
                    string output=Environment.GetEnvironmentVariable("ELDORIA_WORLD_P0_CAPTURE_DIR");
                    Assert.That(output,Is.Not.Null.And.Not.Empty,"Owned output directory required");
                    Assert.That(Directory.Exists(output),Is.False,
                        "Capture must use a new empty output, never overwrite prior evidence");
                    var capture=FindType("Eldoria.World4XCanon.Editor.RegionOneCanonicalCapture");
                    Assert.That(capture,Is.Not.Null,"Real integrated Unity world capture unavailable");
                    var method=capture.GetMethod("CaptureCamera",BindingFlags.Static|BindingFlags.NonPublic);
                    Assert.That(method,Is.Not.Null);
                    Directory.CreateDirectory(output);
                    var navigation=Member(host,"WorldNavigation");
                    var previousTarget=(Vector3)Member(navigation,"target");
                    var previousZoom=camera.orthographicSize;
                    var previousAspect=camera.aspect;
                    try {
                        foreach(var size in new[]{(1280,720,"landscape"),(390,844,"portrait")}){
                            camera.aspect=size.Item1/(float)size.Item2;
                            foreach(var stage in new[]{(2,"cercana"),(1,"regional"),(0,"panoramica")}){
                                Call(navigation,"SetZoomLevel",stage.Item1,true);
                                Call(navigation,"PanTo",Member(host,"WorldHome"));
                                Call(navigation,"FitToAuthoredBounds");
                                Call(adapter,"RefreshForCapture");
                                // The old synchronous six-view loop never let LateUpdate
                                // stream models or finish deferred Destroy between cameras.
                                // Allow the full 36-target budget (2 per .25s) to settle.
                                yield return new WaitForSecondsRealtime(6f);
                                Call(adapter,"RefreshForCapture");
                                yield return null;
                                var populationType=FindType("Eldoria.World4XCanon.RegionOneCreaturePopulation");
                                var population=world.GetComponentInChildren(populationType,true);
                                Assert.That(population,Is.Not.Null);
                                var visible=populationType.GetMethod("OnScreen",BindingFlags.Instance|BindingFlags.NonPublic);
                                var landscapeType=FindType("Eldoria.World4XCanon.RegionOneResourcePlacement");
                                var panoramic=(bool)landscapeType.GetMethod("IsPanoramic",BindingFlags.Static|BindingFlags.Public)
                                    .Invoke(null,new object[]{camera.orthographicSize,(float)Member(navigation,"globalZoom")});
                                int expected=after.WorldCreatureLocations.Count(c=>c.Generation>=1&&c.Alive&&!panoramic&&
                                    (bool)visible.Invoke(population,new object[]{new Vector3(c.X,c.Y,c.Z)}));
                                int actual=world.GetComponentsInChildren<Transform>(true).Count(x=>
                                    x.name.StartsWith("Original Tripo ",StringComparison.Ordinal));
                                Assert.That(actual,Is.EqualTo(expected),
                                    "Every on-screen saved enemy needs one original model after streaming: "+stage.Item2+" "+size.Item3);
                                Debug.Log("ELDORIA_WORLD_SETTLED_VISIBLE_ENEMIES view="+stage.Item2+" orientation="+size.Item3+
                                    " expected="+expected+" actual="+actual);
                                Canvas.ForceUpdateCanvases();
                                method.Invoke(null,new object[]{output,camera,navigation,stage.Item2,size.Item3,size.Item1,size.Item2});
                            }
                        }
                    } finally {
                        camera.aspect=previousAspect;
                        Call(navigation,"RestoreView",previousTarget,previousZoom);
                    }
                    Assert.That(Directory.GetFiles(output,"*-unity.png").Length,Is.EqualTo(6),
                        "All three real world zoom levels in two orientations are required");
                    Assert.That(Directory.GetFiles(output,"*-source.json").Length,Is.EqualTo(6),
                        "Original Unity camera provenance must accompany every screenshot");
                    Debug.Log("ELDORIA_WORLD_P0_SIX_REAL_UNITY_VIEWS_VERIFIED");
                }
                // Exercise the same camera/root repeatedly. This must not allocate
                // a second Region 1, lose HUD/gateway or re-enter construction.
                for(int cycle=0;cycle<12;cycle++)
                {
                    Assert.That((bool)Call(host,"TrySetWorld",false),Is.True);
                    Assert.That(cityCamera.allowMSAA,Is.EqualTo(originalCityMsaa),
                        "Returning must restore the exact original city antialiasing setting");
                    Assert.That(camera.allowMSAA,Is.EqualTo(originalWorldMsaa));
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
                if(logicOnly){
                    Assert.That(missingBastionArt,Is.GreaterThan(0),
                        "The explicit missing-art probe must not accidentally run against full production art");
                    Assert.That(unexpectedErrors,Is.Empty,
                        "Unrelated Unity errors cannot be dismissed by logic-only native tests");
                    Debug.Log("ELDORIA_WORLD_P0_LOGIC_PROBE_MISSING_BASTION_ART_COUNT="+missingBastionArt+
                              " production_visual_assets_validated=false");
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
                if(logicOnly){
                    Assert.That(unexpectedErrors,Is.Empty,
                        "Unity runtime logged an unrelated error during the entire real world journey");
                    Debug.Log("ELDORIA_WORLD_P0_LOGICAL_ONLY_FULL_REENTRY_COMPLETED visual_art_certified=false");
                }
            }
            finally
            {
                if(logicOnly){
                    UnityEngine.Application.logMessageReceived-=callback;
                    LogAssert.ignoreFailingMessages=previousLogAssertionMode;
                }
                optIn.SetValue(null,previousFlag);
                renderOptIn.SetValue(null,previousRenderFlag);
                meshOptIn.SetValue(null,previousMeshFlag);
                Environment.SetEnvironmentVariable("VALORIA_GAMEPLAY_PROBE",previousProbe);
            }
        }
    }
}
#endif
