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
            Assert.That((int)Member(world,"RecoveredPropsPlaced"),Is.GreaterThanOrEqualTo(15),
                "Four semantic settlement zones must instantiate original authored props");
            Assert.That((int)Member(world,"SettlementZonesPlaced"),Is.EqualTo(4));
            Assert.That((int)Member(world,"WorkshopWorkersPlaced"),Is.EqualTo(1));
            Assert.NotNull(Resources.Load<Sprite>("ValoriaModular25D/worker_saw_0"));
            Assert.NotNull(Resources.Load<Sprite>("ValoriaModular25D/worker_saw_1"),
                "Specific original artist-painted saw and trestle must animate at the built sawmill");
            var work=Member(world,"WorkshopAction") as Component;
            Assert.NotNull(work,"One original-art real workshop worker is required");
            Assert.IsFalse(work.gameObject.activeInHierarchy,"Unbuilt sawmill cannot be productive");
            Assert.That(all.Count(x=>x && x.GetType().Name=="Modular25DWorkshopAction"),
                Is.EqualTo(1),"Sawmill must have one animated worker at the real log rack");
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
            Assert.That((int)Member(land,"CivicCourtyardsBuilt"),Is.EqualTo(1),
                "The civic courtyard is a third independent, soft-alpha ground element, not a building platform");
            var pathMeshes=land.GetComponentsInChildren<MeshFilter>(true);
            Assert.That(pathMeshes.Length,Is.EqualTo(2),
                "Both artistic road ribbons must remain separate world meshes");
            foreach(var road in pathMeshes)
            {
                Assert.NotNull(road.sharedMesh);
                var edgeAlpha=road.sharedMesh.colors32;
                Assert.That(edgeAlpha.Length,Is.EqualTo(road.sharedMesh.vertexCount),
                    "Every original road mesh vertex must carry real alpha");
                Assert.That(edgeAlpha[0].a,Is.EqualTo(0),
                    "Road must fade fully into grass at outer bank");
                Assert.That(edgeAlpha[4].a,Is.GreaterThan(110),
                    "Paved centre must remain readable and continuous");
                Assert.That(edgeAlpha[8].a,Is.EqualTo(0),
                    "Opposite bank must fade without a rectangular platform");
            }
            var sceneCanvas=world.GetComponentsInChildren<Canvas>(true);
            Assert.That(sceneCanvas.Count(x=>x.enabled && x.gameObject.activeInHierarchy),
                Is.EqualTo(1),"Only compact modular HUD should be active");
            var firstMessage=Member(ui,"notice") as Text;
            Assert.NotNull(firstMessage);
            Assert.IsTrue(string.IsNullOrEmpty(firstMessage.text),
                "A premium game HUD must not show a permanent prototype debug banner");
            Assert.NotNull(EventSystem.current,"Unity real touch event system missing");
            var header=world.GetComponentsInChildren<RectTransform>(true)
                .FirstOrDefault(x=>x && x.gameObject.name=="Independent resources");
            Assert.NotNull(header,"Mobile resource bar missing");
            Assert.That(header.GetComponentsInChildren<Image>(true).Count(x=>x.sprite!=null && x.type==Image.Type.Sliced),
                Is.GreaterThanOrEqualTo(3),"Three independently rendered resource plates are required");
            Assert.That((string)ui.GetType().GetField("UiEdition").GetRawConstantValue(),
                Does.Contain("Umbral"),"This isolated branch must use the new layout, not Vigilia controls");
            var uiSheet=world.GetComponentsInChildren<RectTransform>(true)
                .FirstOrDefault(x=>x && x.gameObject.name=="Sawmill parchment sheet");
            Assert.NotNull(uiSheet,"The new contextual parchment panel must exist");
            Assert.NotNull(uiSheet.GetComponent<Image>().sprite,"New parchment nine-slice was not created");
            Assert.That(uiSheet.GetComponent<Image>().type,Is.EqualTo(Image.Type.Sliced));
            Assert.That(header.GetComponentsInChildren<Text>(true)
                .Any(x=>x.text=="MADERA"),Is.True,"Live wood capsule missing");
            foreach(var key in new[]{"CormorantGaramond-SemiBold","SourceSans3-Semibold"})
                Assert.NotNull(Resources.Load<Font>("ValoriaModular25D/UI/Fonts/"+key));
            if(Screen.width>Screen.height)
            {
                // A separate actual Chromium landscape screenshot checks the
                // clear center. In Unity ensure the strip is compact and
                // right-anchored, not the old full-width center obstruction.
                Assert.That(header.anchorMin.x,Is.EqualTo(1f).Within(.001f));
                Assert.That(header.pivot.x,Is.EqualTo(1f).Within(.001f));
                Assert.LessOrEqual(header.sizeDelta.x,333.1f,"Landscape resource ledger obstructs the city");
                Assert.Greater(header.anchoredPosition.y,-80f);
            }
            else
                Assert.Less(header.anchoredPosition.y,
                    -header.rect.height*.5f-18f,
                    "Portrait HUD header is cropped at upper edge");
            Assert.That(world.GetComponentsInChildren<SpriteRenderer>(true)
                .Any(x=>x && x.gameObject.name=="Aserradero_contact"),Is.False,
                "Do not restore the second dark contact-shadow oval around the original Aserradero");

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
            var center=buttons.FirstOrDefault(x=>x && x.gameObject.name=="Center camera");
            Assert.NotNull(center,"Functional centering action absent");
            foreach(var control in new[]{inspect,center})
            {
                var image=control.GetComponent<Image>();
                Assert.NotNull(image,"An operational compact HUD button has no graphic");
                Assert.NotNull(image.sprite,"Original Vigilia 9-slice button art is not imported");
                Assert.That(image.type,Is.EqualTo(Image.Type.Sliced),
                    "Premium artwork must use proper scalable borders");
                Assert.That(image.color.r,Is.GreaterThan(.95f),
                    "Artwork was darkened by the old solid-color prototype HUD tint");
                Assert.That(image.color.g,Is.GreaterThan(.95f));
                Assert.That(image.color.b,Is.GreaterThan(.95f));
            }
            var data=new PointerEventData(EventSystem.current){
                button=PointerEventData.InputButton.Left,
                position=new Vector2(Screen.width*.5f,Screen.height*.5f)};
            ExecuteEvents.Execute<IPointerClickHandler>(inspect.gameObject,data,
                ExecuteEvents.pointerClickHandler);
            yield return null;
            var panel=Member(ui,"buildingSheet") as GameObject;
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
            Assert.IsFalse(work.gameObject.activeInHierarchy,
                "Sawyer must be inactive while real worksite timer runs");
            var save=Member(gameplay,"SandboxSavePath") as string;
            Assert.NotNull(save);
            Assert.That(save,Does.Contain("eldoria-experiment-modular25d-playmode-"+run));
            Assert.IsTrue(File.Exists(save),"Actual persistent test save missing");
            Debug.Log("MOD25D_QA_STEP_4_BUILD_AND_FILE_VERIFIED");

            // Camera Home is a runtime action, not an inert button/gesture.
            var camera=Member(world,"ViewCamera") as Camera;
            var original=camera.transform.position;
            Assert.That(camera.orthographicSize,
                Is.EqualTo(Mathf.Clamp(3.55f/Mathf.Max(.42f,camera.aspect),5.05f,7.95f)).Within(.02f),
                "Player and captured authentic Unity Camera must share actual framing");
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
