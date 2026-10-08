using System;
using System.Collections;
using System.Reflection;
using Eldoria.Application;
using Eldoria.Domain;
using Eldoria.Presentation;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace Eldoria.Tests
{
    public sealed class ParcelProgressionRuntimeTests
    {
        sealed class Clock:IClock {public long UtcTicks{get;set;}=DateTime.UtcNow.Ticks;public void Step(int seconds){UtcTicks+=TimeSpan.FromSeconds(seconds).Ticks;}}
        sealed class Store:IStateStore {PlayerState s;public PlayerState Load()=>s;public void Save(PlayerState state){s=state;}}
        static void Invoke(SlicePresenter presenter,string method,params object[] args)
            => typeof(SlicePresenter).GetMethod(method,BindingFlags.Instance|BindingFlags.NonPublic).Invoke(presenter,args);

        GameObject testRoot;
        Scene testScene;
        string previousProfile;
        SlicePresenter testPresenter;
        [UnityTearDown] public IEnumerator Cleanup()
        {
            if(testPresenter!=null)
            {
                var safe=typeof(SlicePresenter).GetField("safe",BindingFlags.Instance|BindingFlags.NonPublic).GetValue(testPresenter) as RectTransform;
                if(safe!=null)UnityEngine.Object.Destroy(safe.root.gameObject);
            }
            if(testRoot!=null)UnityEngine.Object.Destroy(testRoot);
            SliceContentProfiles.SetRuntimeProfileOverride(previousProfile);
            yield return null;
            if(testScene.IsValid())yield return SceneManager.UnloadSceneAsync(testScene);
        }
        [UnityTest] public IEnumerator RealBuildingPanelDrivesExistingGatewayAndParcelLayers()
        {
            previousProfile=SliceContentProfiles.ActiveRuntimeProfile;
            SliceContentProfiles.SetRuntimeProfileOverride(SliceContentProfiles.OwnerIiiId);
            testScene=SceneManager.CreateScene("Parcel runtime UI isolation");
            var clock=new Clock();var store=new Store();var gateway=new LocalGateway(clock,store);
            var root=new GameObject("Runtime parcel test");testRoot=root;SceneManager.MoveGameObjectToScene(root,testScene);var visual=root.AddComponent<ValoriaParcelPresentation>();
            GameObject Child(string name){var go=new GameObject(name);go.transform.SetParent(root.transform);return go;}
            visual.LeftBuilt=Child("left built");visual.RightBuilt=Child("right built");
            visual.LeftGround=Child("left ground");visual.RightGround=Child("right ground");
            visual.BastionLevelTwoVisuals=new[]{Child("bastion ii banner"),Child("bastion ii reinforcement")};
            visual.LeftConstruction=Child("left construction");visual.RightConstruction=Child("right construction");
            visual.LeftTarget=Child("left target").AddComponent<BoxCollider>();visual.RightTarget=Child("right target").AddComponent<BoxCollider>();
            visual.Bindings=new[]{
                new ValoriaParcelPresentation.Binding{ParcelId="LeftCabinParcel",BuildingId="sawmill",VariantBit=1,Built=visual.LeftBuilt,Ground=visual.LeftGround,Construction=visual.LeftConstruction,Target=visual.LeftTarget},
                new ValoriaParcelPresentation.Binding{ParcelId="RightCampParcel",BuildingId="barracks",VariantBit=2,Built=visual.RightBuilt,Ground=visual.RightGround,Construction=visual.RightConstruction,Target=visual.RightTarget}
            };
            var camera=Child("camera").AddComponent<Camera>();camera.tag="MainCamera";visual.ProductionCamera=camera;
            var uiRoot=Child("player UI");var presenter=uiRoot.AddComponent<SlicePresenter>();testPresenter=presenter;
            presenter.Initialize(gateway);presenter.OnSceneLoaded(testScene,LoadSceneMode.Single);
            Assert.That(visual.LeftState,Is.EqualTo(ParcelBuildingState.AVAILABLE));
            Assert.That(visual.RightState,Is.EqualTo(ParcelBuildingState.NOT_BUILT));
            Assert.That(visual.PresentedBastionLevel,Is.EqualTo(1));
            Assert.That(visual.BastionLevelTwoVisuals[0].activeSelf,Is.False);
            Assert.That(visual.LeftBuilt.activeSelf,Is.False);Assert.That(visual.RightTarget.enabled,Is.False);
            Invoke(presenter,"Select","sawmill");
            var action=(Button)typeof(SlicePresenter).GetField("buildingAction",BindingFlags.Instance|BindingFlags.NonPublic).GetValue(presenter);
            Assert.That(action,Is.Not.Null);Assert.That(action.interactable,Is.True);
            action.onClick.Invoke();
            Assert.That(gateway.Snapshot().Resources.Wood,Is.EqualTo(SliceContentProfiles.Active.InitialWood-SliceRules.SawmillWoodCost));
            Assert.That(visual.LeftState,Is.EqualTo(ParcelBuildingState.UNDER_CONSTRUCTION));
            Assert.That(visual.LeftConstruction.activeSelf,Is.True);Assert.That(visual.LeftBuilt.activeSelf,Is.False);
            var resumed=new LocalGateway(clock,store);Assert.That(ParcelBuildingStates.For(resumed.Snapshot(),"sawmill"),Is.EqualTo(ParcelBuildingState.UNDER_CONSTRUCTION));
            clock.Step(SliceRules.SawmillBuildSeconds+1);gateway.Advance();Invoke(presenter,"Refresh");
            Assert.That(visual.LeftState,Is.EqualTo(ParcelBuildingState.BUILT));Assert.That(visual.LeftBuilt.activeSelf,Is.True);
            Assert.That(visual.LeftGround.activeSelf,Is.False);Assert.That(visual.LeftConstruction.activeSelf,Is.False);
            Assert.That(SliceRules.CurrentObjectiveKey(gateway.Snapshot()),Is.EqualTo("b1.gather-wood"));
            int spend=gateway.Snapshot().Resources.Wood;gateway.Advance();Assert.That(gateway.Snapshot().Resources.Wood,Is.EqualTo(spend));
            void Command(string kind,string target){var s=gateway.Snapshot();Assert.That(gateway.Execute(new GameCommand(Guid.NewGuid().ToString("N"),s.PlayerId,kind,target,s.Revision)).Ok,Is.True);}
            void March(string kind,string target){Command(kind,target);clock.Step(120);gateway.Advance();}
            March("Gather","forest-valoria");March("Gather","forest-valoria");March("Gather","quarry-valoria");March("Fight","corrupt-scout");
            Command("AdvanceBastion","bastion");Invoke(presenter,"Refresh");
            Assert.That(visual.PresentedBastionLevel,Is.EqualTo(2));
            Assert.That(visual.BastionLevelTwoVisuals[0].activeSelf,Is.True);
            Assert.That(visual.BastionLevelTwoVisuals[1].activeSelf,Is.True);
            Assert.That(visual.RightState,Is.EqualTo(ParcelBuildingState.AVAILABLE));Assert.That(visual.RightTarget.enabled,Is.True);
            Invoke(presenter,"Select","barracks");Assert.That(action.interactable,Is.True);action.onClick.Invoke();
            Assert.That(visual.RightState,Is.EqualTo(ParcelBuildingState.UNDER_CONSTRUCTION));Assert.That(visual.RightConstruction.activeSelf,Is.True);
            resumed=new LocalGateway(clock,store);Assert.That(ParcelBuildingStates.For(resumed.Snapshot(),"barracks"),Is.EqualTo(ParcelBuildingState.UNDER_CONSTRUCTION));
            clock.Step(SliceRules.BarracksBuildSeconds+1);gateway.Advance();Invoke(presenter,"Refresh");
            Assert.That(visual.RightState,Is.EqualTo(ParcelBuildingState.BUILT));Assert.That(visual.RightGround.activeSelf,Is.False);Assert.That(visual.ActiveVariant,Is.EqualTo(3));
            visual.Home();
            visual.Pan(new Vector2(-150f,0f));
            Assert.That(camera.transform.position.x,Is.GreaterThan(1f),"A normal mobile drag must visibly move the city camera");
            visual.Home();
            Assert.That(ValoriaParcelPresentation.PresentationHomeScale,Is.GreaterThanOrEqualTo(.95f),"HOME must not restart at the over-zoomed 80% FOV crop");
            visual.Pan(new Vector2(-10000,10000));Assert.That(camera.transform.position.x,Is.EqualTo(ValoriaParcelPresentation.HorizontalPanHalfExtent));
            Assert.That(camera.transform.position.y,Is.EqualTo(-ValoriaParcelPresentation.VerticalPanHalfExtent));
            visual.Zoom(-1000);Assert.That(camera.fieldOfView,Is.EqualTo(visual.PresentationHomeFov*.92f).Within(.001));visual.Home();Assert.That(camera.transform.position,Is.EqualTo(Vector3.zero));Assert.That(camera.fieldOfView,Is.EqualTo(visual.PresentationHomeFov).Within(.001));
            yield return null;
        }

        [UnityTest] public IEnumerator ProductionBuildingPickingReachesCertifiedDepthAndPrioritizesItsTarget()
        {
            previousProfile=SliceContentProfiles.ActiveRuntimeProfile;
            testScene=SceneManager.CreateScene("Certified production depth picking");
            testRoot=new GameObject("Production picking test");SceneManager.MoveGameObjectToScene(testRoot,testScene);
            var visual=testRoot.AddComponent<ValoriaParcelPresentation>();
            var cameraGo=new GameObject("Owned production camera");cameraGo.transform.SetParent(testRoot.transform);
            var camera=cameraGo.AddComponent<Camera>();camera.fieldOfView=visual.HomeFov;camera.farClipPlane=1000;
            visual.ProductionCamera=camera;
            WorldHotspot Target(string name,string id,float depth)
            {
                var go=new GameObject(name,typeof(BoxCollider),typeof(WorldHotspot));go.transform.SetParent(testRoot.transform);
                go.transform.position=new Vector3(0,0,depth);go.transform.localScale=new Vector3(11,8,.2f);
                var spot=go.GetComponent<WorldHotspot>();spot.Id=id;return spot;
            }
            var building=Target("InteractiveProxy_LeftCabinParcel","sawmill",240);
            visual.LeftTarget=building.GetComponent<Collider>();
            Target("InteractiveProxy_MainRoad","bastion",220);
            var uiGo=new GameObject("Picking presenter");uiGo.transform.SetParent(testRoot.transform);
            testPresenter=uiGo.AddComponent<SlicePresenter>();
            var flags=BindingFlags.Instance|BindingFlags.NonPublic;
            typeof(SlicePresenter).GetField("productionParcels",flags).SetValue(testPresenter,visual);
            typeof(SlicePresenter).GetField("city",flags).SetValue(testPresenter,true);
            Physics.SyncTransforms();yield return null;
            var pixel=camera.WorldToScreenPoint(visual.LeftTarget.bounds.center);
            var selected=typeof(SlicePresenter).GetMethod("ResolveHotspot",flags).Invoke(testPresenter,new object[]{new Vector2(pixel.x,pixel.y)}) as WorldHotspot;
            Assert.That(selected,Is.SameAs(building),"A visible building at the actual production depth must win over a nearer broad road proxy");
        }

#if UNITY_EDITOR
        [UnityTest] public IEnumerator WorldHotspotBindingsSurviveStablePrefabSerialization()
        {
            const string path="Assets/__EldoriaHotspotSerializationTest.prefab";
            const string source="Assets/Eldoria/Scripts/Presentation/WorldHotspot.cs";
            previousProfile=SliceContentProfiles.ActiveRuntimeProfile;
            testScene=SceneManager.CreateScene("Hotspot serialization isolation");
            testRoot=new GameObject("Canonical hotspot bindings");SceneManager.MoveGameObjectToScene(testRoot,testScene);
            // The published defect omitted Id on only three of nine instances of one type.
            var names=new[]{"UpperWalls","MainRoad","CentralStair","TerrainCliffSupport","Bridge","Bastion","LeftCabin","LowerGate","RightCamp"};
            var ids=new[]{"bastion","bastion","bastion","bastion","bastion","bastion","sawmill","gate","barracks"};
            for(int i=0;i<names.Length;i++)
            {
                var go=new GameObject(names[i]);go.transform.SetParent(testRoot.transform);
                var hotspot=go.AddComponent<WorldHotspot>();hotspot.Id=ids[i];
                Assert.That(UnityEditor.AssetDatabase.GetAssetPath(UnityEditor.MonoScript.FromMonoBehaviour(hotspot)),Is.EqualTo(source),
                    "Every instance must use a source asset, not a scene-local secondary MonoScript");
            }
            try
            {
                UnityEditor.PrefabUtility.SaveAsPrefabAsset(testRoot,path);
                UnityEditor.AssetDatabase.SaveAssets();UnityEditor.AssetDatabase.ImportAsset(path,UnityEditor.ImportAssetOptions.ForceUpdate);
                var prefab=UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>(path);
                Assert.That(prefab.GetComponentsInChildren<WorldHotspot>(true).Length,Is.EqualTo(names.Length));
                var copy=UnityEngine.Object.Instantiate(prefab);copy.transform.SetParent(testRoot.transform);
                for(int i=0;i<names.Length;i++)
                {
                    Assert.That(prefab.transform.Find(names[i]).GetComponent<WorldHotspot>().Id,Is.EqualTo(ids[i]));
                    Assert.That(copy.transform.Find(names[i]).GetComponent<WorldHotspot>().Id,Is.EqualTo(ids[i]));
                }
                string yaml=System.IO.File.ReadAllText(path),guid=UnityEditor.AssetDatabase.AssetPathToGUID(source);
                Assert.That(System.Text.RegularExpressions.Regex.Matches(yaml,"guid: "+guid).Count,Is.EqualTo(names.Length));
                Assert.That(System.Text.RegularExpressions.Regex.Matches(yaml,"  Id: ").Count,Is.EqualTo(names.Length));
                yield return null;
            }
            finally { UnityEditor.AssetDatabase.DeleteAsset(path); }
        }

#endif

        [UnityTest] public IEnumerator PresentedFrameProjectionAndPickingStayInverseAfterCropPanZoom()
        {
            previousProfile=SliceContentProfiles.ActiveRuntimeProfile;
            testScene=SceneManager.CreateScene("Presented frame coordinate isolation");
            testRoot=new GameObject("Presented projection test");SceneManager.MoveGameObjectToScene(testRoot,testScene);
            var visual=testRoot.AddComponent<ValoriaParcelPresentation>();
            var cameraGo=new GameObject("Projection camera");cameraGo.transform.SetParent(testRoot.transform);
            var camera=cameraGo.AddComponent<Camera>();visual.ProductionCamera=camera;
            camera.transform.position=new Vector3(2.8f,1.1f,0);
            var loader=testRoot.AddComponent<ValoriaWebGLSplatStateLoader>();loader.enabled=false;loader.Presentation=visual;
            var canvasGo=new GameObject("Projection test canvas",typeof(RectTransform),typeof(Canvas));canvasGo.transform.SetParent(testRoot.transform);
            canvasGo.GetComponent<Canvas>().renderMode=RenderMode.ScreenSpaceOverlay;
            var frameGo=new GameObject("Presented texture",typeof(RectTransform));frameGo.transform.SetParent(canvasGo.transform,false);
            var rect=frameGo.GetComponent<RectTransform>();rect.anchorMin=rect.anchorMax=new Vector2(.5f,.5f);
            var flags=BindingFlags.Instance|BindingFlags.NonPublic;
            typeof(ValoriaWebGLSplatStateLoader).GetField("webBackgroundRect",flags).SetValue(loader,rect);
            var texture=new Texture2D(2,2);
            typeof(ValoriaWebGLSplatStateLoader).GetField("webBackgroundTexture",flags).SetValue(loader,texture);
            foreach(bool portrait in new[]{false,true})
            foreach(var key in new[]{"valoria-state0-pan-0.5-zoom0.9.png","valoria-state0-pan0.5-zoom1.1.png","valoria-bastion-ii.png"})
            {
                rect.sizeDelta=portrait?new Vector2(1230,845):new Vector2(844,844*845f/1230f);
                rect.anchoredPosition=new Vector2(portrait?140f:5f,-7f);rect.localScale=Vector3.one*1.025f;
                typeof(ValoriaWebGLSplatStateLoader).GetField("loadedVisualKey",flags).SetValue(loader,key);
                Canvas.ForceUpdateCanvases();
                var target=new Vector3(.2f,-.3f,12f);
                Assert.That(loader.TryProjectPresentedPoint(target,out var pixel),Is.True);
                Assert.That(loader.TryPresentedRay(new Vector2(pixel.x,pixel.y),out var ray),Is.True);
                var closest=ray.origin+ray.direction*Vector3.Dot(target-ray.origin,ray.direction);
                Assert.That(Vector3.Distance(closest,target),Is.LessThan(.001f),"Picking must follow displayed frame after "+key+(portrait?" portrait":" landscape"));
            }
            typeof(ValoriaWebGLSplatStateLoader).GetField("webBackgroundTexture",flags).SetValue(loader,null);
            Assert.That(loader.TryPresentedRay(Vector2.zero,out _),Is.False,"An unavailable frame must not allow invisible building selection");
            UnityEngine.Object.Destroy(texture);
            yield return null;
        }
    }
}


