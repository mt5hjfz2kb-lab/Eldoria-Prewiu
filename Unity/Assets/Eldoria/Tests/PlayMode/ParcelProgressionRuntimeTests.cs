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

        [UnityTest] public IEnumerator RealBuildingPanelDrivesExistingGatewayAndParcelLayers()
        {
            SliceContentProfiles.SetRuntimeProfileOverride(SliceContentProfiles.OwnerIiiId);
            foreach(var old in UnityEngine.Object.FindObjectsByType<SlicePresenter>(FindObjectsSortMode.None))UnityEngine.Object.DestroyImmediate(old.gameObject);
            var clock=new Clock();var store=new Store();var gateway=new LocalGateway(clock,store);
            var root=new GameObject("Runtime parcel test");var visual=root.AddComponent<ValoriaParcelPresentation>();
            GameObject Child(string name){var go=new GameObject(name);go.transform.SetParent(root.transform);return go;}
            visual.LeftBuilt=Child("left built");visual.RightBuilt=Child("right built");
            visual.LeftGround=Child("left ground");visual.RightGround=Child("right ground");
            visual.LeftConstruction=Child("left construction");visual.RightConstruction=Child("right construction");
            visual.LeftTarget=Child("left target").AddComponent<BoxCollider>();visual.RightTarget=Child("right target").AddComponent<BoxCollider>();
            var camera=Child("camera").AddComponent<Camera>();camera.tag="MainCamera";visual.ProductionCamera=camera;
            var uiRoot=Child("player UI");var presenter=uiRoot.AddComponent<SlicePresenter>();
            presenter.Initialize(gateway);presenter.OnSceneLoaded(SceneManager.GetActiveScene(),LoadSceneMode.Single);
            Assert.That(visual.LeftState,Is.EqualTo(ParcelBuildingState.AVAILABLE));
            Assert.That(visual.RightState,Is.EqualTo(ParcelBuildingState.NOT_BUILT));
            Assert.That(visual.LeftBuilt.activeSelf,Is.False);Assert.That(visual.RightTarget.enabled,Is.False);
            Invoke(presenter,"Select","sawmill");
            Button action=null;foreach(var b in presenter.GetComponentsInChildren<Button>(true))if(b.gameObject.name=="Building action")action=b;
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
            visual.Pan(new Vector2(-10000,10000));Assert.That(camera.transform.position.x,Is.EqualTo(.5f));Assert.That(camera.transform.position.y,Is.Zero);
            visual.Zoom(-1000);Assert.That(camera.fieldOfView,Is.EqualTo(visual.HomeFov*.9f).Within(.001));visual.Home();Assert.That(camera.transform.position,Is.EqualTo(Vector3.zero));
            UnityEngine.Object.Destroy(root);SliceContentProfiles.SetRuntimeProfileOverride(null);yield return null;
        }
    }
}
