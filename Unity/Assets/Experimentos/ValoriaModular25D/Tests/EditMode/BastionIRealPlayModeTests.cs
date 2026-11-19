#if UNITY_EDITOR
using System;
using System.Collections;
using System.IO;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;
using Object=UnityEngine.Object;

namespace Eldoria.Modular25D.Tests
{
    /// <summary>
    /// The ACTUAL saved Unity scene enters actual PlayMode. All new sprites,
    /// UI and canonical gameplay are queried by reflection because the test
    /// assembly must not reference predefined Assembly-CSharp. No beta data.
    /// This is not a device/iPhone proof or an automatic artistic approval.
    /// </summary>
    public sealed class BastionIRealPlayModeTests
    {
        const string ScenePath =
            "Assets/Experimentos/ValoriaModular25D/Scenes/BastionI_Modular25D.unity";
        static object Member(object obj,string key)
        {
            Assert.NotNull(obj,"No owner for "+key);
            var type=obj.GetType();
            var flag=BindingFlags.Public|BindingFlags.NonPublic|BindingFlags.Instance;
            var prop=type.GetProperty(key,flag);
            if(prop!=null)return prop.GetValue(obj);
            var field=type.GetField(key,flag);
            Assert.NotNull(field,"Missing field "+type.Name+"."+key);
            return field.GetValue(obj);
        }
        static object Call(object obj,string method,params object[] args)
        {
            var flags=BindingFlags.Public|BindingFlags.NonPublic|BindingFlags.Instance;
            var name=obj.GetType().GetMethods(flags).FirstOrDefault(x=>
                x.Name==method&&x.GetParameters().Length==args.Length);
            Assert.NotNull(name,"Missing method "+obj.GetType().Name+"."+method);
            return name.Invoke(obj,args);
        }

        [UnityTest]
        public IEnumerator BastionIRealScene_ActualPlayMode_UI_Build_TouchAndSave()
        {
            var run=Environment.GetEnvironmentVariable("ELDORIA_MOD25D_PLAYMODE_RUN");
            if(string.IsNullOrEmpty(run)||run.Length<7||!run.All(char.IsDigit))
                Assert.Ignore("Requires an isolated QA numeric run ID. Never touch player beta saves.");

            // A disabled guard presenter ships IN the saved scene: entering
            // PlayMode here tests that the canonical SliceBoot does not spawn
            // a second persistent old HUD over this standalone experiment.
            EditorSceneManager.OpenScene(ScenePath,OpenSceneMode.Single);
            yield return new EnterPlayMode();
            Debug.Log("MOD25D_QA_STEP_1_ENTER_PLAY");
            for(int i=0;i<12;i++)yield return null;
            Assert.That(UnityEngine.Application.isPlaying,Is.True);
            Assert.That(SceneManager.GetActiveScene().name,
                Is.EqualTo("BastionI_Modular25D"));

            var all=Object.FindObjectsByType<MonoBehaviour>(FindObjectsInactive.Include,
                FindObjectsSortMode.None);
            var worlds=all.Where(x=>x && x.GetType().Name=="Modular25DWorld").ToArray();
            Assert.That(worlds.Length,Is.EqualTo(1),"Only one game world may run");
            var world=worlds[0];
            Assert.That((int)Member(world,"RecoveredPropsPlaced"),Is.GreaterThanOrEqualTo(10),
                "Full original-art props must be active, not fake low-poly fallbacks");
            Assert.NotNull(Member(world,"ViewCamera") as Camera);
            var gameplay=Member(world,"Gameplay") as Component;
            var ui=Member(world,"Hud") as Component;
            Assert.NotNull(gameplay,"Real game gateway is not running");
            Assert.NotNull(ui,"Compact real command HUD is not running");
            Assert.That((bool)Member(gameplay,"IsAuthoritative"),Is.True);
            Assert.That(all.Where(x=>x&&x.GetType().Name=="SlicePresenter"&&x.enabled).Count(),
                Is.EqualTo(0),"Legacy HUD must not capture clicks or overwrite saves");

            var land=all.FirstOrDefault(x=>x && x.GetType().Name=="Modular25DLandscape");
            Assert.NotNull(land);
            Assert.That((int)Member(land,"RoadCount"),Is.EqualTo(2));
            Assert.That((int)Member(land,"FootprintCount"),Is.EqualTo(2));
            var sceneCanvas=world.GetComponentsInChildren<Canvas>(true);
            Assert.That(sceneCanvas.Count(x=>x.enabled && x.gameObject.activeInHierarchy),
                Is.EqualTo(1),"Only compact modular HUD should be active");
            Assert.NotNull(EventSystem.current,"Unity real touch event system missing");

            Debug.Log("MOD25D_QA_STEP_2_WORLD_HUD_AND_ART");
            var first=Call(gameplay,"Snapshot");
            Assert.NotNull(first);
            Assert.That((int)Member(first,"BastionLevel"),Is.EqualTo(1));
            Assert.That((int)Member(first,"SawmillLevel"),Is.EqualTo(0));
            var wallet=Member(first,"Resources");
            int initialWood=(int)Member(wallet,"Wood");
            Assert.GreaterOrEqual(initialWood,80);

            var buttons=world.GetComponentsInChildren<Button>(true);
            var inspect=buttons.FirstOrDefault(x=>x && x.gameObject.name=="Inspect sawmill");
            Assert.NotNull(inspect,"Functional sawmill inspection button absent");
            var data=new PointerEventData(EventSystem.current){
                button=PointerEventData.InputButton.Left,
                position=new Vector2(Screen.width*.5f,Screen.height*.5f)};
            ExecuteEvents.Execute<IPointerClickHandler>(inspect.gameObject,data,
                ExecuteEvents.pointerClickHandler);
            yield return null;
            var panel=Member(ui,"panel") as GameObject;
            Assert.NotNull(panel);
            Assert.IsTrue(panel.activeInHierarchy,"Real Unity pointer click failed to open build panel");

            var build=buttons.FirstOrDefault(x=>x && x.gameObject.name==
                "Execute canonical Build command");
            Assert.NotNull(build,"No authoritative construction button");
            Assert.IsTrue(build.interactable);
            ExecuteEvents.Execute<IPointerClickHandler>(build.gameObject,data,
                ExecuteEvents.pointerClickHandler);
            yield return null;
            Debug.Log("MOD25D_QA_STEP_3_CLICKED_CANONICAL_BUILD");
            var after=Call(gameplay,"Snapshot");
            Assert.That((int)Member(Member(after,"Resources"),"Wood"),
                Is.EqualTo(initialWood-80),"Canonical real debit mismatch");
            Assert.That((int)Member(after,"SawmillLevel"),Is.EqualTo(0),
                "A scheduled build cannot complete instantly");
            Assert.That((long)Member(after,"BuildingCompletesUtcTicks"),Is.GreaterThan(0));
            Assert.IsFalse(build.interactable,"Second click must not double-spend");
            var save=Member(gameplay,"SandboxSavePath") as string;
            Assert.NotNull(save);
            Assert.That(save,Does.Contain("eldoria-experiment-modular25d-playmode-"+run));
            Assert.IsTrue(File.Exists(save),"Actual persistent test save missing");
            Debug.Log("MOD25D_QA_STEP_4_BUILD_AND_FILE_VERIFIED");

            // Camera Home is a runtime action, not an inert button/gesture.
            var camera=Member(world,"ViewCamera") as Camera;
            var original=camera.transform.position;
            var input=all.FirstOrDefault(x=>x&&x.GetType().Name=="Modular25DCameraInput");
            Assert.NotNull(input);
            camera.transform.position+=new Vector3(1.4f,-1.2f,0);
            Call(input,"Home");
            Debug.Log("MOD25D_QA_STEP_5_CAMERA_HOME");
            Assert.That(Vector3.Distance(camera.transform.position,original),
                Is.LessThan(.02f),"Home did not restore the world camera");

            // Real Unity Transform and sprite foot ordering change in PlayMode.
            // Only Citizen A has zero spawn delay. FindObjectsByType has no
            // guaranteed enumeration order: choosing B (1.8s delay) and
            // waiting 26 ultra-fast batchmode frames was a false regression.
            var walker=all.FirstOrDefault(x=>x&&
                x.GetType().Name=="Modular25DWalker"&&
                x.gameObject.name=="Valoria citizen A");
            Assert.NotNull(walker,"The actual moving road-side Citizen A must exist");
            Debug.Log("MOD25D_QA_STEP_6_BEFORE_WALK");
            var before=walker.transform.position;
            float deadline=Time.realtimeSinceStartup+2.0f;
            while(Vector3.Distance(before,walker.transform.position)<.03f &&
                  Time.realtimeSinceStartup<deadline)yield return null;
            Debug.Log("MOD25D_QA_STEP_7_AFTER_WALK");
            Assert.Greater(Vector3.Distance(before,walker.transform.position),.025f,
                "Citizen A did not traverse a real Unity world path within bounded realtime");

            Debug.Log("ELDORIA_MODULAR25D_PLAYMODE_PASS source-scene real Canvas pointer "+
                "canonical cost save camera and moving inhabitant; NO_DEVICE_APPROVAL");
            yield return new ExitPlayMode();
        }
    }
}
#endif
