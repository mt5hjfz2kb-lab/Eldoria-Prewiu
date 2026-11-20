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
            // This NUnit asmdef intentionally has NO reference to Assembly-CSharp.
            // Query the real public Sprite factory through reflection, preserving
            // the experiment's assembly isolation and the exact original art.
            var originalArtType=AppDomain.CurrentDomain.GetAssemblies()
                .Select(a=>a.GetType("Eldoria.Experimentos.ValoriaModular25D.Modular25DArt",false))
                .FirstOrDefault(t=>t!=null);
            Assert.NotNull(originalArtType,"Original artist sprite factory not loaded");
            var sawyerMethod=originalArtType.GetMethod("Sawyer",BindingFlags.Public|BindingFlags.Static);
            Assert.NotNull(sawyerMethod);
            Assert.NotNull(sawyerMethod.Invoke(null,new object[]{0}) as Sprite,
                "Original artist-painted sawyer frame 0 must remain importable");
            Assert.NotNull(sawyerMethod.Invoke(null,new object[]{1}) as Sprite,
                "Original artist-painted sawyer frame 1 must remain importable; PNG cannot be replaced");
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
            Assert.That(header.GetComponentsInChildren<Image>(true).Count(x=>x.sprite!=null && x.sprite.name=="resource_siglet" && x.type==Image.Type.Simple),
                Is.GreaterThanOrEqualTo(3),"Three independently rendered resource plates are required");
            Assert.That((string)ui.GetType().GetField("UiEdition").GetRawConstantValue(),
                Does.Contain("Corona de la Brecha"),"Original chosen art direction must replace Umbral");
            var uiSheet=world.GetComponentsInChildren<RectTransform>(true)
                .FirstOrDefault(x=>x && x.gameObject.name=="Sawmill parchment sheet");
            Assert.NotNull(uiSheet,"The new contextual parchment panel must exist");
            Assert.NotNull(uiSheet.GetComponent<Image>().sprite,"Original painted obsidian panel must be loaded");
            var titleRect=uiSheet.GetComponentsInChildren<Text>(true)
                .FirstOrDefault(x=>x.text=="ASERRADERO")?.rectTransform;
            Assert.NotNull(titleRect);
            Assert.That(titleRect.anchoredPosition.x,Is.GreaterThan(-40f),
                "Portrait+landscape dark-fantasy panel title cannot overlap the ornate left frame");
            Assert.That(titleRect.rect.width,Is.LessThan(200f),
                "Bastion I construction modal must reserve its close medallion and corner filigree");
            Assert.That(uiSheet.GetComponent<Image>().type,Is.EqualTo(Image.Type.Simple),
                "Do not slice hand-painted edge and center gems into thin lines");
            Assert.That(header.GetComponentsInChildren<Text>(true)
                .Count(x=>x.text=="0" || int.TryParse(x.text,out _)),Is.GreaterThanOrEqualTo(3),
                "Three resource values must be readable without tiny labels on gold borders");
            Assert.That(header.GetComponentsInChildren<Image>(true)
                .Count(x=>x.sprite!=null && x.sprite.name.StartsWith("icon_")),Is.EqualTo(3),
                "Exactly three painted resource icons must identify wood, stone and food");
            foreach(var item in new[]{"icon_wood","icon_stone","icon_food",
                "icon_hammer","icon_city","icon_task"})
                Assert.NotNull(Resources.Load<Sprite>(
                    "ValoriaModular25D/UI/CoronaBrecha/"+item),
                    "Selected original source icon missing: "+item);
            var gameIcons=world.GetComponentsInChildren<Image>(true)
                .Where(x=>x && x.sprite!=null && x.sprite.name.StartsWith("icon_"))
                .Select(x=>x.sprite.name).Distinct().ToArray();
            Assert.That(gameIcons.Length,Is.GreaterThanOrEqualTo(6),
                "Six illustrated Corona resource/action icons must actually render");

            // Commercial-world readable labels must be actual tap targets
            // positioned by the same camera Transform as the 2.5D scene.
            var tagKeep=world.GetComponentsInChildren<RectTransform>(true)
                .FirstOrDefault(x=>x && x.gameObject.name=="Bastion world nameplate");
            var tagMill=world.GetComponentsInChildren<RectTransform>(true)
                .FirstOrDefault(x=>x && x.gameObject.name=="Sawmill world nameplate");
            Assert.NotNull(tagKeep,"No world-anchored Bastion identity plate");
            Assert.NotNull(tagMill,"No world-anchored Aserradero status plate");
            Assert.NotNull(tagKeep.GetComponent<Button>());
            Assert.NotNull(tagMill.GetComponent<Button>());
            Assert.That(tagKeep.GetComponentsInChildren<Text>(true)
                .Any(x=>x.text.Contains("BASTIÓN")),Is.True);
            Assert.That(tagMill.GetComponentsInChildren<Text>(true)
                .Any(x=>x.text.Contains("ASERRADERO")),Is.True);
            foreach(var key in new[]{"CormorantGaramond-SemiBold","SourceSans3-Semibold"})
                Assert.NotNull(Resources.Load<Font>("ValoriaModular25D/UI/Fonts/"+key));
            // The game HUD must use authored, border-preserving pieces,
            // not a substituted procedural panel on a flat mockup.
            // On a Canvas with referencePixelsPerUnit=100 and sprite PPU=200,
            // a 2x baked PNG spriteBorder value equals the combined onscreen
            // left+right or top+bottom borders in reference UI points. These
            // MUST be smaller than the smallest visible control dimensions.
            // Otherwise Unity shrinks/overlaps the borders and the central
            // 9-slice field vanishes (confirmed source-quality regression).
            foreach(var spec in new[]{
                new {key="identity_crest",border=20f,height=68f},
                new {key="resource_siglet",border=14f,height=43f},
                new {key="nav_medallion",border=19f,height=80f},
                new {key="portrait_ring",border=10f,height=27f},
                new {key="world_rune",border=10f,height=34f},
                new {key="objective_rune",border=17f,height=50f},
                new {key="action_arcane",border=14f,height=42f},
                new {key="panel_obsidian",border=56f,height=170f}
            })
            {
                var skin=Resources.Load<Sprite>(
                    "ValoriaModular25D/UI/CoronaBrecha/"+spec.key);
                Assert.NotNull(skin,"Corona original 9-slice missing: "+spec.key);
                Assert.That(skin.border.x,Is.EqualTo(2f*spec.border).Within(.1f),
                    "Original Corona sprite border contract changed: "+spec.key);
                Assert.That(skin.border.y,Is.LessThan(spec.height),
                    "9-slice vertical borders collapse the center at real UI size: "+spec.key);
            }

            var aldric=Resources.Load<Sprite>("ValoriaModular25D/UI/UmbralII/aldric_avatar");
            Assert.NotNull(aldric,"The original Sir Aldric headshot must be imported for the functional city HUD");
            var actualFace=world.GetComponentsInChildren<RectTransform>(true)
                .FirstOrDefault(x=>x && x.gameObject.name=="Authentic Sir Aldric character portrait");
            Assert.NotNull(actualFace,"No character headshot displayed in Umbral II");
            var actualFaceImage=actualFace.GetComponent<Image>();
            Assert.NotNull(actualFaceImage);
            Assert.That(actualFaceImage.sprite,Is.EqualTo(aldric),
                "The displayed identity avatar is not the source-certified Aldric image");
            Assert.That(actualFaceImage.raycastTarget,Is.False,
                "Decorative portraits must not intercept taps");
            if(Screen.width>Screen.height)
            {
                // A separate actual Chromium landscape screenshot checks the
                // clear center. In Unity ensure the strip is compact and
                // right-anchored, not the old full-width center obstruction.
                Assert.That(header.anchorMin.x,Is.EqualTo(1f).Within(.001f));
                Assert.That(header.pivot.x,Is.EqualTo(1f).Within(.001f));
                Assert.LessOrEqual(header.sizeDelta.x,365.1f,"Landscape resource ledger must remain compact and non-overlapping");
                Assert.Greater(header.anchoredPosition.y,-80f);
                var identityWide=world.GetComponentsInChildren<RectTransform>(true)
                    .First(x=>x&&x.gameObject.name=="Valoria identity");
                float scaleWide=Mathf.Max(.01f,
                    header.GetComponentInParent<Canvas>().scaleFactor);
                Assert.GreaterOrEqual(identityWide.anchoredPosition.x,
                    Mathf.Max(0f,Screen.safeArea.xMin/scaleWide)+8f,
                    "Real landscape safe area must protect the keep crest from the camera island");
                Assert.GreaterOrEqual(-header.anchoredPosition.x,
                    Mathf.Max(0f,(Screen.width-Screen.safeArea.xMax)/scaleWide)+8f,
                    "Resources must not be hidden below the landscape notch");
            }
            else
            {
                Assert.Less(header.anchoredPosition.y,
                    -header.rect.height*.5f-18f,
                    "Portrait HUD header is cropped at upper edge");
                var identity=world.GetComponentsInChildren<RectTransform>(true)
                    .FirstOrDefault(x=>x && x.gameObject.name=="Valoria identity");
                Assert.NotNull(identity,"Corona identity crest missing");
                // resourceRow has top pivot=1; its anchoredPosition is
                // the visual top edge, not its center. Demand 8pt gap.
                Assert.LessOrEqual(header.anchoredPosition.y,
                    identity.anchoredPosition.y-identity.rect.height-8f,
                    "Portrait resources overlap Valoria identity golden crest");
                var quest=world.GetComponentsInChildren<RectTransform>(true)
                    .FirstOrDefault(x=>x&&x.name=="Current real construction objective");
                Assert.NotNull(quest);
                Assert.That(quest.anchorMin.y,Is.EqualTo(0f).Within(.01f),
                    "The main objective should occupy bottom-left, away from the scene's keep");
                var cameraDock=world.GetComponentsInChildren<RectTransform>(true)
                    .FirstOrDefault(x=>x&&x.name=="Center camera");
                Assert.NotNull(cameraDock);
                float sceneWidth=Screen.width/Mathf.Max(.01f,
                    header.GetComponentInParent<Canvas>().scaleFactor);
                Assert.LessOrEqual(
                    quest.anchoredPosition.x+quest.rect.width+8f,
                    sceneWidth+cameraDock.anchoredPosition.x-cameraDock.rect.width,
                    "The quest and camera medallion must not overlap at phone size");
            }
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
            var woodText=Member(ui,"woodValue") as Text;
            var stoneText=Member(ui,"stoneValue") as Text;
            var foodText=Member(ui,"foodValue") as Text;
            foreach(var counter in new[]{woodText,stoneText,foodText})
            {
                Assert.NotNull(counter,"Actual commercial resource counter missing");
                Assert.That(counter.verticalOverflow,Is.EqualTo(VerticalWrapMode.Overflow),
                    "Do not crop all SourceSans resource glyphs at 14/20px height");
                Assert.That(counter.rectTransform.rect.height,Is.GreaterThanOrEqualTo(30f),
                    "Resource glyph box must fit actual imported font line metrics");
                Assert.That(counter.text,Is.Not.Empty,"Numeric counters cannot be empty");
            }
            Assert.That(woodText.text,Is.EqualTo(initialWood.ToString("N0")),
                "Visible wood label must reflect the canonical gateway snapshot");
            var originalWoodArt=header.GetComponentsInChildren<Image>(true)
                .FirstOrDefault(x=>x.sprite!=null && x.sprite.name=="icon_wood");
            Assert.NotNull(originalWoodArt);
            Assert.That(originalWoodArt.color,Is.EqualTo(Color.white),
                "Never tint the original illustrated logs with gold or red");
            Assert.That(woodText.color.r,Is.EqualTo(248f/255f).Within(.004f),
                "Readable ivory wood count must retain red channel");
            Assert.That(woodText.color.g,Is.EqualTo(238f/255f).Within(.004f),
                "Readable ivory wood count must retain green channel");
            Assert.That(woodText.color.b,Is.EqualTo(220f/255f).Within(.004f),
                "Readable ivory wood count must retain blue channel");

            var buttons=world.GetComponentsInChildren<Button>(true);
            var inspect=buttons.FirstOrDefault(x=>x && x.gameObject.name=="Inspect sawmill");
            Assert.NotNull(inspect,"Functional sawmill inspection button absent");
            var center=buttons.FirstOrDefault(x=>x && x.gameObject.name=="Center camera");
            Assert.NotNull(center,"Functional centering action absent");
            foreach(var control in new[]{inspect,center})
            {
                var image=control.GetComponent<Image>();
                Assert.NotNull(image,"An operational HUD medallion has no graphic");
                Assert.NotNull(image.sprite,"Original Corona illustrated button art is not imported");
                Assert.That(image.type,Is.EqualTo(Image.Type.Simple),
                    "Complete Corona illustrated medallion cannot be distorted by 9-slice");
                var expected=control.gameObject.name=="Inspect sawmill"
                    ? "icon_hammer" : "icon_city";
                Assert.That(image.sprite.name,Is.EqualTo(expected),
                    "Do not nest a tiny 42px medallion within another ornate ring");
                Assert.That(image.preserveAspect,Is.True,
                    "Premium medallion must retain original 1:1 artist silhouette");
                Assert.That(image.color.r,Is.GreaterThan(.95f),
                    "Artwork was darkened by old prototype UI tint");
                Assert.That(image.color.g,Is.GreaterThan(.95f));
                Assert.That(image.color.b,Is.GreaterThan(.95f));
                Assert.That(control.GetComponentsInChildren<Image>(true)
                    .Count(x=>x.sprite!=null && x.sprite.name.StartsWith("icon_")),
                    Is.EqualTo(1),"Only ONE illustrated premium icon per action, no nested duplicate");
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
            Assert.IsFalse(inspect.gameObject.activeInHierarchy,
                "Real Corona modal must hide behind-panel dock hit targets");
            Assert.IsFalse(center.gameObject.activeInHierarchy,
                "An open construction modal must not overlap center camera navigation");
            var currentNameplate=world.GetComponentsInChildren<RectTransform>(true)
                .FirstOrDefault(x=>x && x.gameObject.name=="Sawmill world nameplate");
            Assert.NotNull(currentNameplate);
            Assert.That(currentNameplate.rect.height,Is.GreaterThanOrEqualTo(44f),
                "Commercial world nameplate must retain a touch-sized hit target");
            Assert.That(currentNameplate.GetComponent<Image>().sprite.name,
                Is.EqualTo("world_rune"),
                "Teal construction sprite must be reserved for the real Build action");


            var progress=world.GetComponentsInChildren<RectTransform>(true)
                .FirstOrDefault(x=>x && x.gameObject.name=="Canonical construction progress frame");
            Assert.NotNull(progress,"Progress bar source is missing from the real Aserradero panel");
            Assert.IsFalse(progress.gameObject.activeSelf,
                "Before construction, do not show a fake or pre-filled timer");
            var fill=world.GetComponentsInChildren<RectTransform>(true)
                .FirstOrDefault(x=>x && x.gameObject.name=="Real sawmill elapsed time");
            Assert.NotNull(fill,"Canonical elapsed time fill object must exist");
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
            Assert.That(woodText.text,Is.EqualTo((initialWood-80).ToString("N0")),
                "Actual on-screen HUD wood glyphs must update when the authoritative command spends wood");
            Assert.That((int)Member(after,"SawmillLevel"),Is.EqualTo(0),
                "A scheduled build cannot complete instantly");
            Assert.That((long)Member(after,"BuildingCompletesUtcTicks"),Is.GreaterThan(0));
            Assert.IsFalse(build.interactable,"Second click must not double-spend");
            Assert.That(Member(ui,"cachedSawmillPhase").ToString(),
                Is.EqualTo("UNDER_CONSTRUCTION"),
                "Floating 2.5D nameplate must track canonical BUILD phase; no stale fixed completed/available position");
            var worldTrack=world.GetComponentsInChildren<RectTransform>(true)
                .FirstOrDefault(x=>x&&x.gameObject.name=="World sawmill canonical progress track");
            Assert.NotNull(worldTrack,"Auto-closing construction needs an in-world visual work timer");
            Assert.IsTrue(worldTrack.gameObject.activeSelf,
                "Canonical building phase must expose its real progress on the Aserradero world label");
            Assert.That(worldTrack.GetComponentInChildren<Image>(true).color.a,
                Is.GreaterThan(.1f),"World progress track is fully transparent");
            var sawmillName=world.GetComponentsInChildren<RectTransform>(true)
                .FirstOrDefault(x=>x&&x.gameObject.name=="Sawmill world nameplate");
            Assert.NotNull(sawmillName);
            Assert.That(sawmillName.GetComponentsInChildren<Text>(true)
                .Any(x=>x.text.StartsWith("OBRA")),Is.True,
                "The actual remaining time must be visible without reopening the panel");
            var progressClock=sawmillName.GetComponentsInChildren<Text>(true)
                .First(x=>x.text.StartsWith("OBRA"));
            Assert.That(progressClock.rectTransform.anchoredPosition.y,
                Is.GreaterThan(0f),"Visible in-world countdown must sit above its elapsed-time track");
            Assert.That(progressClock.rectTransform.rect.height,Is.LessThan(25f),
                "Countdown label must not cover the progress bar at the badge bottom");
            Assert.IsFalse(panel.activeInHierarchy,
                "Successful canonical construction must immediately dismiss the detail panel");
            Assert.IsTrue(center.gameObject.activeInHierarchy,
                "Close after Build must restore navigation without a second tap");
            Call(ui,"OpenSawmill");
            yield return null;
            Assert.IsTrue(progress.gameObject.activeInHierarchy,
                "Reopening the construction panel must show real canonical progress");
            Assert.That(fill.sizeDelta.x,Is.GreaterThanOrEqualTo(2f));
            Assert.That(fill.sizeDelta.x,Is.LessThanOrEqualTo(181f),
                "Progress UI must be bounded by the real canonical construction time");
            Assert.IsFalse(work.gameObject.activeInHierarchy,
                "Sawyer must be inactive while real worksite timer runs");
            var save=Member(gameplay,"SandboxSavePath") as string;
            Assert.NotNull(save);
            Assert.That(save,Does.Contain("eldoria-experiment-modular25d-playmode-"+run));
            Assert.IsTrue(File.Exists(save),"Actual persistent test save missing");
            Assert.That(currentNameplate.GetComponent<Image>().sprite.name,
                Is.EqualTo("world_rune"),
                "Sawmill phase refresh must never stretch the teal action plate across the map");
            Call(ui,"ClosePanels");
            yield return null;
            Assert.IsFalse(panel.activeInHierarchy,"Close button must dismiss modal");
            Assert.IsTrue(center.gameObject.activeInHierarchy,
                "Original active camera button must return when the construction sheet closes");
            Debug.Log("MOD25D_QA_STEP_4_BUILD_AND_FILE_VERIFIED");

            // Camera Home is a runtime action, not an inert button/gesture.
            var camera=Member(world,"ViewCamera") as Camera;
            var original=camera.transform.position;
            Assert.That(camera.orthographicSize,
                Is.EqualTo(Mathf.Clamp(3.05f/Mathf.Max(.42f,camera.aspect),4.90f,7.10f)).Within(.02f),
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
