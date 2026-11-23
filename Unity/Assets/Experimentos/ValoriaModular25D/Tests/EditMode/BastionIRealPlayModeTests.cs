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
            foreach(var key in new[]{"bastion_i","sawmill_i"})
            {
                var baselineSprite=Resources.Load<Sprite>("ValoriaModular25D/"+key);
                var hd=Resources.Load<Sprite>("ValoriaModular25D/"+key+"_hd");
                Assert.NotNull(baselineSprite,"Original artwork must remain available");
                Assert.NotNull(hd,"Retina restoration must actually import as Sprite");
                Assert.That(hd.rect.width,Is.EqualTo(baselineSprite.rect.width*3f).Within(.01f));
                Assert.That(hd.rect.height,Is.EqualTo(baselineSprite.rect.height*3f).Within(.01f));
                Assert.That(hd.pixelsPerUnit,Is.EqualTo(baselineSprite.pixelsPerUnit*3f).Within(.01f));
                Assert.That(Vector3.Distance(hd.bounds.size,baselineSprite.bounds.size),Is.LessThan(.001f),
                    "Higher texture resolution must not resize the actual building");
                Assert.That(Vector3.Distance(hd.bounds.center,baselineSprite.bounds.center),Is.LessThan(.001f),
                    "Restored art must preserve the original foot pivot");
                string factory=key=="bastion_i"?"Bastion":"Sawmill";
                Assert.AreSame(hd,originalArtType.GetMethod(factory).Invoke(null,null),
                    "The playable scene must actually use the restored art");
            }
            var idle=originalArtType.GetMethod("CitizenIdle");
            Assert.NotNull(idle);
            var idleFront=idle.Invoke(null,new object[]{false}) as Sprite;
            var idleRear=idle.Invoke(null,new object[]{true}) as Sprite;
            Assert.NotNull(idleFront);Assert.NotNull(idleRear);
            Assert.AreNotEqual(idleFront.rect,idleRear.rect);
            Assert.That(idleFront.rect.height,Is.EqualTo(224f));
            Assert.NotNull(sawyerMethod.Invoke(null,new object[]{0}) as Sprite,
                "Original artist-painted sawyer frame 0 must remain importable");
            Assert.NotNull(sawyerMethod.Invoke(null,new object[]{1}) as Sprite,
                "Original artist-painted sawyer frame 1 must remain importable; PNG cannot be replaced");
            var work=Member(world,"WorkshopAction") as Component;
            Assert.NotNull(work,"One original-art real workshop worker is required");
            Assert.IsFalse(work.gameObject.activeInHierarchy,"Unbuilt sawmill cannot be productive");
            Assert.That(all.Count(x=>x && x.GetType().Name=="Modular25DWorkshopAction"),
                Is.EqualTo(1),"Sawmill must have one animated worker at the real log rack");
            var citizen=originalArtType.GetMethod("Citizen",BindingFlags.Public|BindingFlags.Static);
            var carpenter=originalArtType.GetMethod("Carpenter",BindingFlags.Public|BindingFlags.Static);
            Assert.NotNull(citizen);
            Assert.NotNull(carpenter);
            var poses=new System.Collections.Generic.HashSet<Rect>();
            for(int index=0;index<8;index++)
            {
                var frame=citizen.Invoke(null,new object[]{index%4,index>=4}) as Sprite;
                Assert.NotNull(frame);
                Assert.GreaterOrEqual(frame.rect.height,192f);
                poses.Add(frame.rect);
            }
            Assert.That(poses.Count,Is.EqualTo(8),"Front and back walking phases must use distinct authored frames");
            for(int index=0;index<4;index++)
            {
                var frame=carpenter.Invoke(null,new object[]{index}) as Sprite;
                Assert.NotNull(frame);
                Assert.GreaterOrEqual(frame.rect.height,192f);
                poses.Add(frame.rect);
            }
            Assert.That(poses.Count,Is.EqualTo(12),"Carpenter phases must have their own artwork");
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
            Assert.That((int)Member(land,"RoadCount"),Is.EqualTo(6));
            Assert.That((int)Member(land,"FootprintCount"),Is.EqualTo(1));
            Assert.That((int)Member(land,"CivicCourtyardsBuilt"),Is.EqualTo(1),
                "The civic courtyard is a third independent, soft-alpha ground element, not a building platform");
            var pathMeshes=land.GetComponentsInChildren<MeshFilter>(false);
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
                Assert.That(edgeAlpha[(edgeAlpha.Length/18)*9+4].a,Is.GreaterThan(110),
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
            Assert.That(header.GetComponentsInChildren<Image>(true).Count(x=>x.sprite!=null && x.sprite.name=="wallet_inset" && x.type==Image.Type.Simple),
                Is.GreaterThanOrEqualTo(3),"Three independently rendered resource plates are required");
            var material=header.GetComponentsInChildren<Image>(true)
                .First(x=>x&&x.sprite!=null&&x.sprite.name=="wallet_inset");
            var onScreenAspect=material.rectTransform.rect.width/
                Mathf.Max(1f,material.rectTransform.rect.height);
            var sourceAspect=material.sprite.rect.width/material.sprite.rect.height;
            Assert.That(Mathf.Abs(onScreenAspect/sourceAspect-1f),Is.LessThan(.12f),
                "Do not squash the original premium jewel resource source in the phone HUD");
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

            // Actual mobile Canvas: one 46-point TOP ribbon includes the
            // player nickname + all 3 independent painted resource cells.
            var canvas=header.GetComponentInParent<Canvas>();
            Assert.NotNull(canvas);
            Assert.IsTrue(canvas.pixelPerfect,"Retina UI should align to physical pixels");
            float uiScale=Mathf.Max(.01f,canvas.scaleFactor);
            float logicalWidth=Screen.width/uiScale;
            float insetLeft=Mathf.Max(0f,Screen.safeArea.xMin/uiScale);
            float insetRight=Mathf.Max(0f,(Screen.width-Screen.safeArea.xMax)/uiScale);
            float safeTop=Mathf.Max(0f,(Screen.height-Screen.safeArea.yMax)/uiScale)+2f;
            float safeBottom=Mathf.Max(0f,Screen.safeArea.yMin/uiScale)+8f;
            bool wide=Screen.width>Screen.height;
            var identity=world.GetComponentsInChildren<RectTransform>(true)
                .FirstOrDefault(x=>x && x.gameObject.name=="Valoria identity");
            var ribbon=world.GetComponentsInChildren<RectTransform>(true)
                .FirstOrDefault(x=>x && x.gameObject.name=="Shared player and resource ribbon");
            var handle=world.GetComponentsInChildren<RectTransform>(true)
                .FirstOrDefault(x=>x && x.gameObject.name=="Toggle left activities");
            var drawer=world.GetComponentsInChildren<RectTransform>(true)
                .FirstOrDefault(x=>x && x.gameObject.name=="Left activity drawer");
            Assert.NotNull(identity);Assert.NotNull(ribbon);
            Assert.NotNull(handle);Assert.NotNull(drawer);
            var drawerSkin=drawer.GetComponent<Image>();
            Assert.NotNull(drawerSkin);
            Assert.NotNull(drawerSkin.sprite);
            Assert.That(drawerSkin.sprite.name,Is.EqualTo("activity_panel_wide"),
                "The left drawer must use a sculpted wide painting instead of squashing a square frame");
            Assert.That(drawerSkin.sprite.rect.width,Is.EqualTo(720f).Within(.1f));
            Assert.That(drawerSkin.sprite.rect.height,Is.EqualTo(500f).Within(.1f));
            Assert.That(drawerSkin.type,Is.EqualTo(Image.Type.Simple));
            Assert.LessOrEqual(ribbon.rect.height,47f);
            var ribbonImage=ribbon.GetComponent<Image>();
            Assert.NotNull(ribbonImage);
            Assert.NotNull(ribbonImage.sprite,"Missing ORIGINAL 1512px velvet-gold Unity material");
            Assert.That(ribbonImage.sprite.name,Is.EqualTo("top_velvet_ribbon"),
                "Single horizontal player+wallet bar must use original high-DPI painted material");
            Assert.That(ribbonImage.sprite.rect.width,Is.GreaterThanOrEqualTo(1512f),
                "Retina ribbon may not upscale a low-resolution 378px source");
            Assert.That(ribbonImage.type,Is.EqualTo(Image.Type.Simple));
            Assert.That(ribbonImage.color,Is.EqualTo(Color.white),
                "Do not dark-tint original velvet and reduce painted details");
            Assert.LessOrEqual(identity.rect.height,45f);
            Assert.LessOrEqual(header.rect.height,43f);
            Assert.That(header.anchoredPosition.y,Is.EqualTo(-safeTop-3f).Within(1.5f),
                "Resources must be in the same TOP horizontal ribbon as player");
            Assert.That(identity.anchoredPosition.y,Is.EqualTo(-safeTop-1f).Within(1.5f));
            Assert.That(ribbon.anchoredPosition.y,Is.EqualTo(-safeTop).Within(1.5f));
            Assert.That(header.GetComponentsInChildren<Image>(true)
                .Count(x=>x.sprite!=null&&x.sprite.name=="wallet_inset"
                    &&x.preserveAspect),Is.EqualTo(3));
            float resourceLeft=logicalWidth+header.anchoredPosition.x-header.rect.width;
            float identityRight=identity.anchoredPosition.x+identity.rect.width;
            Assert.GreaterOrEqual(resourceLeft,identityRight+4f,
                "Player nickname and resource panels visually overlap on single strip");
            var nickname=identity.GetComponentsInChildren<Text>(true)
                .FirstOrDefault(x=>x.text=="VALORIA");
            Assert.NotNull(nickname);
            Canvas.ForceUpdateCanvases();
            Assert.That(nickname.cachedTextGenerator.lineCount, Is.EqualTo(1),
                "The saved nickname must render on one line inside the ribbon");
            Assert.GreaterOrEqual(nickname.cachedTextGenerator.characterCountVisible, nickname.text.Length,
                "The last letter of the player name was clipped");
            float nicknameRight=identity.rect.width*.5f+
                nickname.rectTransform.anchoredPosition.x+
                nickname.rectTransform.rect.width*.5f;
            Assert.LessOrEqual(nicknameRight,identity.rect.width-3f);
            float nicknameLeft=identity.rect.width*.5f+
                nickname.rectTransform.anchoredPosition.x-
                nickname.rectTransform.rect.width*.5f;
            var bezel=actualFace.parent as RectTransform;
            Assert.NotNull(bezel);
            Assert.GreaterOrEqual(nicknameLeft,bezel.anchoredPosition.x+bezel.rect.width*.5f+2f,
                "The full player name area must clear the actual illustrated portrait");
            var nameMethod=ui.GetType().GetMethod("PlayerDisplayName",
                BindingFlags.Public|BindingFlags.Static);
            Assert.NotNull(nameMethod);
            Assert.That((string)nameMethod.Invoke(null,new object[]{"Lyra"}),
                Is.EqualTo("Lyra"),"Real player nick cannot be a hardcoded city name");
            Assert.That((string)nameMethod.Invoke(null,new object[]{" "}),
                Is.EqualTo("VALORIA"));
            Assert.That((int)ui.GetType().GetField("FutureNavigationSlots").GetRawConstantValue(),
                Is.EqualTo(8));
            var cellMethod=ui.GetType().GetMethod("NavigationCell",
                BindingFlags.Public|BindingFlags.Static);
            Assert.NotNull(cellMethod);
            var cells=new Rect[8];
            for(int i=0;i<8;i++)
            {
                cells[i]=(Rect)cellMethod.Invoke(null,new object[]{
                    i,logicalWidth,insetLeft,insetRight,safeBottom,wide});
                Assert.GreaterOrEqual(cells[i].width,44f);
                Assert.GreaterOrEqual(cells[i].xMin,insetLeft+1f);
                Assert.LessOrEqual(cells[i].xMax,logicalWidth-insetRight-1f);
                if(i>0)Assert.LessOrEqual(cells[i-1].xMax+2f,cells[i].xMin);
            }
            foreach(float testWidth in new[]{320f,390f,430f,750f})
            {
                var reserved=new Rect[8];
                for(int i=0;i<reserved.Length;i++)
                {
                    reserved[i]=(Rect)cellMethod.Invoke(null,new object[]{i,testWidth,0f,0f,8f,false});
                    Assert.GreaterOrEqual(reserved[i].width,44f);
                    Assert.GreaterOrEqual(reserved[i].xMin,1f);
                    Assert.LessOrEqual(reserved[i].xMax,testWidth-1f);
                    for(int j=0;j<i;j++)Assert.IsFalse(reserved[i].Overlaps(reserved[j]),
                        "All eight future touch zones must fit without overlap");
                }
            }
            var mailboxRect=world.GetComponentsInChildren<RectTransform>(true)
                .FirstOrDefault(x=>x&&x.gameObject.name=="OpenSystem/mail");
            Assert.NotNull(mailboxRect,"Main's Correo destination must remain in the bottom navigation");
            Assert.GreaterOrEqual(mailboxRect.rect.width,72f);
            Assert.IsNull(world.GetComponentsInChildren<RectTransform>(true)
                .FirstOrDefault(x=>x&&x.gameObject.name=="Inspect sawmill"));
            Assert.IsNull(world.GetComponentsInChildren<RectTransform>(true)
                .FirstOrDefault(x=>x&&x.gameObject.name=="Center camera"));
            Assert.IsNull(world.GetComponentsInChildren<RectTransform>(true)
                .FirstOrDefault(x=>x&&x.gameObject.name=="Inspect city status"));
            Assert.That(world.GetComponentsInChildren<RectTransform>(true)
                .Count(x=>x&&x.gameObject.name.StartsWith("Abrir ")),Is.EqualTo(0),
                "Building districts are contextual world interactions, never invented bottom destinations");
            var quest=world.GetComponentsInChildren<RectTransform>(true)
                .FirstOrDefault(x=>x&&x.gameObject.name=="Current real construction objective");
            Assert.NotNull(quest);
            Assert.That(quest.parent,Is.EqualTo(drawer));
            Assert.IsFalse(drawer.gameObject.activeInHierarchy,
                "Progress/quest drawer is hidden by default on the left");
            Assert.GreaterOrEqual(handle.rect.width,44f);
            Assert.That(handle.anchorMin.x,Is.EqualTo(0f).Within(.001f));
            Assert.NotNull(handle.GetComponent<Button>());
            Call(ui,"ToggleActivities");
            yield return null;
            Assert.IsTrue(drawer.gameObject.activeInHierarchy,
                "Tapping left handle must reveal real mission/status");
            Call(ui,"ToggleActivities");
            yield return null;
            Assert.IsFalse(drawer.gameObject.activeInHierarchy,
                "Drawer must close without covering the city");
            if(wide)
            {
                Assert.That(header.anchorMin.x,Is.EqualTo(1f).Within(.001f));
                Assert.GreaterOrEqual(identity.anchoredPosition.x,insetLeft+3f);
                Assert.GreaterOrEqual(-header.anchoredPosition.x,insetRight+3f);
            }
            Assert.That(world.GetComponentsInChildren<SpriteRenderer>(true)
                .Any(x=>x && x.gameObject.name=="Aserradero_contact"),Is.False,
                "Do not restore the second dark contact-shadow oval around the original Aserradero");
            foreach(var key in new[]{"BarracksRenderer","GranaryRenderer","StoneworksRenderer"})
                Assert.IsFalse((Member(world,key) as SpriteRenderer)
                    .GetComponents<Component>().Any(component=>
                        component.GetType().Name=="Modular25DArchitecturalShadow"),
                    "Expansion art has painted contact; a second alpha projection leaves a rectangular platform");

            Debug.Log("MOD25D_QA_STEP_2_WORLD_HUD_AND_ART");
            var first=Call(gameplay,"Snapshot");
            // Isolated presentation fixtures; never save or execute them as campaign progress.
            for(int tier=1;tier<=5;tier++)
            {
                var fixture=Activator.CreateInstance(first.GetType());
                first.GetType().GetField("BastionLevel").SetValue(fixture,tier);
                first.GetType().GetField("SawmillLevel").SetValue(fixture,1);
                first.GetType().GetField("BarracksLevel").SetValue(fixture,tier>=2?1:0);
                var chapter=Member(fixture,"ChapterThree");
                chapter.GetType().GetField("GranaryLevel").SetValue(chapter,tier>=3?1:0);
                chapter.GetType().GetField("StoneworksLevel").SetValue(chapter,tier>=5?1:0);
                Call(world,"Apply",fixture);
                Assert.AreSame(originalArtType.GetMethod("BastionForLevel").Invoke(null,new object[]{tier}),
                    (Member(world,"BastionRenderer") as SpriteRenderer).sprite);
                Assert.AreEqual(tier>=2,(Member(world,"BarracksRenderer") as SpriteRenderer).gameObject.activeSelf);
                Assert.AreEqual(tier>=3,(Member(world,"GranaryRenderer") as SpriteRenderer).gameObject.activeSelf);
                Assert.AreEqual(tier>=5,(Member(world,"StoneworksRenderer") as SpriteRenderer).gameObject.activeSelf);
                first.GetType().GetField("BarracksLevel").SetValue(fixture,0);
                chapter.GetType().GetField("GranaryLevel").SetValue(chapter,0);
                chapter.GetType().GetField("StoneworksLevel").SetValue(chapter,0);
                Call(world,"Apply",fixture);
                foreach(var key in new[]{"BarracksRenderer","GranaryRenderer","StoneworksRenderer"})
                    Assert.IsFalse((Member(world,key) as SpriteRenderer).gameObject.activeSelf,
                        "Tier unlock alone must not create completed buildings");
            }
            Call(world,"Apply",first);

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
            var compact=ui.GetType().GetMethod("CompactResource",
                BindingFlags.Public|BindingFlags.Static);
            Assert.NotNull(compact,"Missing compact source-bound future resource formatting");
            Assert.That(compact.Invoke(null,new object[]{1000L}).ToString(),
                Is.EqualTo("1K"));
            Assert.That(compact.Invoke(null,new object[]{1000000L}).ToString(),
                Is.EqualTo("1M"));
            foreach(var chip in header.GetComponentsInChildren<RectTransform>(true)
                .Where(x=>x && x.name.StartsWith("Resource ")))
            {
                var icon=chip.GetComponentsInChildren<Image>(true)
                    .FirstOrDefault(x=>x.sprite!=null && x.sprite.name.StartsWith("icon_"));
                var number=chip.GetComponentInChildren<Text>(true);
                Assert.NotNull(icon);
                Assert.NotNull(number);
                float artRight=icon.rectTransform.anchoredPosition.x+
                    icon.rectTransform.rect.width;
                float textLeft=chip.rect.width+
                    number.rectTransform.anchoredPosition.x-
                    number.rectTransform.rect.width;
                Assert.LessOrEqual(artRight,textLeft+2f,
                    "Long-term resource amounts must not overlay illustrated materials");
            }
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
            var mailbox=buttons.FirstOrDefault(x=>x&&x.gameObject.name=="OpenSystem/mail");
            Assert.NotNull(mailbox,"Main's functional Correo destination is absent");
            foreach(var control in new[]{mailbox})
            {
                var caption=control.GetComponentInChildren<Text>(true);
                Assert.NotNull(caption,"Missing original Corona icon caption");
                Assert.LessOrEqual(caption.rectTransform.anchoredPosition.y,
                    -control.GetComponent<RectTransform>().rect.width*.5f-4f,
                    "Captions must sit BELOW the original sculpted medallions at every responsive size");
                Assert.NotNull(caption.GetComponent<Outline>(),
                    "Ivory-on-grass navigation caption requires unobtrusive four-way outline");
                var image=control.GetComponent<Image>();
                Assert.NotNull(image,"An operational HUD medallion has no graphic");
                Assert.NotNull(image.sprite,"Original Corona illustrated button art is not imported");
                Assert.That(image.type,Is.EqualTo(Image.Type.Simple),
                    "Complete Corona illustrated medallion cannot be distorted by 9-slice");
                Assert.That(image.sprite.name,Is.EqualTo("icon_supplies"),
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
            var currentNameplate=world.GetComponentsInChildren<RectTransform>(true)
                .FirstOrDefault(x=>x && x.gameObject.name=="Sawmill world nameplate");
            Assert.NotNull(currentNameplate);
            var data=new PointerEventData(EventSystem.current){
                button=PointerEventData.InputButton.Left,
                position=new Vector2(Screen.width*.5f,Screen.height*.5f)};
            ExecuteEvents.Execute<IPointerClickHandler>(currentNameplate.gameObject,data,
                ExecuteEvents.pointerClickHandler);
            yield return null;
            var panel=Member(ui,"buildingSheet") as GameObject;
            Assert.NotNull(panel);
            Assert.IsTrue(panel.activeInHierarchy,"Real Unity pointer click failed to open build panel");
            Assert.IsFalse(mailbox.gameObject.activeInHierarchy,
                "Real Corona modal must hide behind-panel dock hit targets");
            var padding=currentNameplate.GetComponent<Image>().raycastPadding;
            Assert.That(currentNameplate.rect.height-padding.y-padding.w,Is.GreaterThanOrEqualTo(44f),
                "Commercial world nameplate must retain a touch-sized hit target");
            Assert.That(currentNameplate.GetComponent<Image>().sprite.name,
                Is.EqualTo("world_rune"),
                "Teal construction sprite must be reserved for the real Build action");
            foreach(var crafted in new[]{
                currentNameplate.GetComponent<Image>(),
                buttons.First(x=>x&&x.gameObject.name=="Execute canonical Build command")
                    .GetComponent<Image>()
            })
            {
                var sourceRatio=crafted.sprite.rect.width/crafted.sprite.rect.height;
                var targetRatio=crafted.rectTransform.rect.width/
                    Mathf.Max(1f,crafted.rectTransform.rect.height);
                Assert.That(Mathf.Abs(sourceRatio/targetRatio-1f),Is.LessThan(.12f),
                    "A premium illustrated diamond cannot be flattened to a line by a mismatched UI Rect");
            }


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
            Assert.NotNull(worldTrack);
            Assert.IsFalse(worldTrack.gameObject.activeSelf,
                "The construction timer belongs in the LEFT drawer, never over the town");
            Assert.IsFalse(currentNameplate.gameObject.activeInHierarchy,
                "Construction sign must not block the Aserradero façade");
            Assert.IsFalse(drawer.gameObject.activeInHierarchy,
                "Successful Build dismisses the modal and leaves task drawer hidden");
            Call(ui,"ToggleActivities");
            yield return null;
            Assert.IsTrue(drawer.gameObject.activeInHierarchy,
                "User can reopen live construction info from the left handle");
            var leftStatus=Member(ui,"activityProgress") as Text;
            Assert.NotNull(leftStatus);
            Assert.IsTrue(leftStatus.text.StartsWith("Finaliza en "),
                "The left drawer must show the real remaining clock");
            var leftTrack=Member(ui,"activityTrack") as RectTransform;
            Assert.NotNull(leftTrack);
            Assert.IsTrue(leftTrack.gameObject.activeInHierarchy);
            var leftFill=Member(ui,"activityFill") as RectTransform;
            Assert.NotNull(leftFill);
            Assert.That(leftFill.sizeDelta.x,Is.InRange(2f,174f));
            Call(ui,"ToggleActivities");
            yield return null;
            Assert.IsFalse(drawer.gameObject.activeInHierarchy);
            Assert.IsFalse(panel.activeInHierarchy,
                "Successful canonical construction must immediately dismiss the detail panel");
            var onScreenNotice=Member(ui,"notice") as Text;
            Assert.NotNull(onScreenNotice);
            Assert.IsTrue(string.IsNullOrEmpty(onScreenNotice.text),
                "Do not obscure the premium Bastion with a generic Action started debug toast after Build");
            Assert.IsTrue(mailbox.gameObject.activeInHierarchy,
                "Close after Build must restore main's navigation without a second tap");
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
            Assert.IsTrue(mailbox.gameObject.activeInHierarchy,
                "Main's Correo button must return when the construction sheet closes");
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



