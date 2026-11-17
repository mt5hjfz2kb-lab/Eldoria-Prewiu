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

namespace Eldoria.Experimentos.Ciudad2D.PlayModeProof
{
    /// <summary>
    /// True Editor PlayMode of the separate premium scene, not a physical iPhone.
    /// Uses reflection because this assembly may not reference Assembly-CSharp.
    /// No gateway call, save, construction command, or new gameplay component.
    /// </summary>
    public sealed class PremiumVisualSceneProofV4
    {
        private const string ScenePath =
            "Assets/Experimentos/Ciudad2D/QA/Experimento_Ciudad2D_PremiumV4.unity";
        private static Type Runtime(string name)
        {
            var found=Type.GetType("Eldoria.Experimentos.Ciudad2D."+name+", Assembly-CSharp");
            Assert.NotNull(found,"Missing candidate presentation type: "+name);
            return found;
        }
        private static object Property(Component component,string name)
        {
            var p=component.GetType().GetProperty(name,BindingFlags.Instance|BindingFlags.Public);
            Assert.NotNull(p,"Presentation property missing: "+name);
            return p.GetValue(component);
        }
        private static Component Find(string type,string id=null)
        {
            var entries=UnityEngine.Object.FindObjectsByType(Runtime(type),FindObjectsSortMode.None);
            Assert.IsNotEmpty(entries,type+" missing from genuine scene");
            foreach(var x in entries)
            {
                var c=(Component)x;
                if(id==null || string.Equals(Property(c,"BuildingId")?.ToString(),id,
                                              StringComparison.Ordinal))
                    return c;
            }
            Assert.Fail("Cannot find candidate building "+id);
            return null;
        }
        private static void SetStage(Component bridge,int stage)
        {
            var m=bridge.GetType().GetMethod("SetDemoBastionLevel",
                BindingFlags.Public|BindingFlags.Instance);
            Assert.NotNull(m,"Read-only visual stage setter missing");
            m.Invoke(bridge,new object[]{stage});
        }
        private static int ActiveShadows()
        {
            var all=UnityEngine.Object.FindObjectsByType(Runtime("City2DPremiumShadowGate"),
                FindObjectsSortMode.None);
            Assert.AreEqual(4,all.Length,"Four independent premium shadow components required");
            int count=0;
            foreach(var item in all)
            {
                var gate=(Component)item;
                Assert.IsTrue((bool)Property(gate,"IsPurePresentation"));
                Assert.IsNull(gate.GetComponent<Collider2D>(),"Premium shadow is not a touch target");
                var method=gate.GetType().GetMethod("Refresh",BindingFlags.Public|BindingFlags.Instance);
                method.Invoke(gate,null);
                if((bool)Property(gate,"IsVisible"))count++;
            }
            return count;
        }

        private static int ActiveTrainingDetails()
        {
            var all=UnityEngine.Object.FindObjectsByType(
                Runtime("City2DFunctionalDetailGate"),FindObjectsSortMode.None);
            Assert.AreEqual(2,all.Length,"one target cutout and one contact shadow expected");
            int shown=0;
            foreach(var item in all)
            {
                var detail=(Component)item;
                Assert.IsTrue((bool)Property(detail,"IsPurePresentation"));
                Assert.IsNull(detail.GetComponent<Collider2D>(),"Decor cannot consume tap input");
                detail.GetType().GetMethod("Refresh",BindingFlags.Public|BindingFlags.Instance)
                    .Invoke(detail,null);
                if((bool)Property(detail,"IsVisible"))shown++;
            }
            return shown;
        }

        [UnityTest, Timeout(120000)]
        public IEnumerator PremiumV4Scene_GenuinePlayMode_StagesSpritesShadowsPointer()
        {
            yield return new EnterPlayMode();
            Assert.IsTrue(UnityEngine.Application.isPlaying);
            Assert.IsTrue(File.Exists(Path.Combine(UnityEngine.Application.dataPath,"../"+ScenePath)));
            var load=EditorSceneManager.LoadSceneAsyncInPlayMode(
                ScenePath,new LoadSceneParameters(LoadSceneMode.Single));
            Assert.NotNull(load);
            while(!load.isDone)yield return null;
            yield return null;
            Assert.AreEqual("Experimento_Ciudad2D_PremiumV4",SceneManager.GetActiveScene().name);

            var bridge=Find("City2DStateBridge");
            Assert.IsFalse((bool)Property(bridge,"IsLiveBound"));
            Assert.AreEqual(6,(int)Property(bridge,"RegisteredBuildingCount"));
            Assert.AreEqual(6,(int)Property(bridge,"RegisteredAmbientEffectCount"),
                "Premium must retain four embers and two chimney smoke sources");
            var saw=Find("CityBuilding","aserradero");
            var barracks=Find("CityBuilding","cuartel");
            var granary=Find("CityBuilding","granero");
            var quarry=Find("CityBuilding","cantera");
            Assert.AreEqual("Aserradero_Premium_V2",saw.GetComponent<SpriteRenderer>().sprite.name);
            Assert.AreEqual("Cuartel_Premium_V2",barracks.GetComponent<SpriteRenderer>().sprite.name);
            Assert.AreEqual("Granero_Premium_V1",granary.GetComponent<SpriteRenderer>().sprite.name);
            Assert.AreEqual("Cantera_Premium_V1",quarry.GetComponent<SpriteRenderer>().sprite.name);
            SetStage(bridge,1);yield return null;
            Assert.AreEqual(0,ActiveShadows(),"No shadows on unbuilt city plots at Bastion I");
            Assert.AreEqual(0,ActiveTrainingDetails(),"No archery targets before Cuartel is completed");
            Assert.IsFalse(granary.GetComponent<SpriteRenderer>().enabled);
            SetStage(bridge,3);yield return null;
            Assert.AreEqual(2,ActiveShadows(),"Only finished Barracks and Sawmill at Bastion III");
            Assert.AreEqual(2,ActiveTrainingDetails(),"Built barracks must have visible training targets and contact");
            Assert.IsFalse(granary.GetComponent<SpriteRenderer>().enabled);
            Assert.IsTrue(barracks.GetComponent<Collider2D>().enabled);
            Assert.IsTrue(saw.GetComponent<Collider2D>().enabled);
            Assert.NotNull(EventSystem.current);
            var click=new PointerEventData(EventSystem.current) {
                button=PointerEventData.InputButton.Left,
                position=Camera.main.WorldToScreenPoint(barracks.transform.position)
            };
            bool handled=ExecuteEvents.Execute(barracks.gameObject,click,
                ExecuteEvents.pointerClickHandler);
            Assert.IsTrue(handled,"Premium Cuartel cannot be selected by pointer");
            yield return null;
            Assert.IsTrue(UnityEngine.Object.FindObjectsByType<Text>(
                FindObjectsSortMode.None).Any(x=>x.text!=null &&
                     x.text.Contains("Cuartel") && x.text.Contains("Built")),
                "Premium scene selection must update existing HUD");
            // Transient presentation-only construction fixture; never issues a command.
            // Verify that the training decoration does not exist on an unfinished plot.
            var phaseClass=Runtime("City2DPlotPhase");
            var viewClass=Runtime("City2DPlotView");
            var setter=barracks.GetType().GetMethod("SetAppearance",
                BindingFlags.Instance|BindingFlags.Public);
            Assert.NotNull(setter);
            var working=Activator.CreateInstance(viewClass,
                new object[]{Enum.Parse(phaseClass,"UnderConstruction"),0,3});
            setter.Invoke(barracks,new[]{working});
            yield return null;
            Assert.AreEqual("UnderConstruction",Property(barracks,"PlotPhase").ToString());
            Assert.IsFalse(barracks.GetComponent<SpriteRenderer>().enabled);
            Assert.AreEqual(0,ActiveTrainingDetails(),
                "Training prop cannot appear while Cuartel is under construction");
            var finished=Activator.CreateInstance(viewClass,
                new object[]{Enum.Parse(phaseClass,"Built"),1,3});
            setter.Invoke(barracks,new[]{finished});
            yield return null;
            Assert.AreEqual(2,ActiveTrainingDetails(),
                "Targets and contact must return after Cuartel has been built");
            SetStage(bridge,5);yield return null;
            // Canonical demo stage V unlocks Cantera but does NOT build it.
            // It would violate game rules to expect its sprite/shadow here.
            Assert.AreEqual(3,ActiveShadows(),"Three completed plots; quarry is available, not built");
            Assert.AreEqual(2,ActiveTrainingDetails(),"Training detail persists at Bastion V");
            Assert.AreEqual("Available",Property(quarry,"PlotPhase").ToString());
            Assert.IsFalse(quarry.GetComponent<SpriteRenderer>().enabled);
            Assert.IsTrue(granary.GetComponent<SpriteRenderer>().enabled);
            // Granero V4 retains V3 proportions and is the only art-scale modification; verify its event still
            // selects the actual building and reaches the canonical HUD.
            var granaryTap=new PointerEventData(EventSystem.current) {
                button=PointerEventData.InputButton.Left,
                position=Camera.main.WorldToScreenPoint(granary.transform.position)
            };
            Assert.IsTrue(ExecuteEvents.Execute(granary.gameObject,granaryTap,
                ExecuteEvents.pointerClickHandler),"Enlarged Granero V3 cannot be selected");
            yield return null;
            Assert.IsTrue(UnityEngine.Object.FindObjectsByType<Text>(
                FindObjectsSortMode.None).Any(x=>x.text!=null &&
                     x.text.Contains("Granero") && x.text.Contains("Built")),
                "Granero V3 click must reach the existing HUD");

            // A separate, temporary visual-only fixture represents a player
            // who actually built the quarry after reaching Bastion V. This
            // calls the existing presentation setter; never grants resources,
            // executes commands or writes a PlayerState/save.
            var phaseType=Runtime("City2DPlotPhase");
            var viewType=Runtime("City2DPlotView");
            var built=Enum.Parse(phaseType,"Built");
            var builtView=Activator.CreateInstance(viewType,new object[]{built,1,5});
            var showQuarry=quarry.GetType().GetMethod("SetAppearance",
                BindingFlags.Instance|BindingFlags.Public);
            Assert.NotNull(showQuarry);
            showQuarry.Invoke(quarry,new[]{builtView});
            yield return null;
            Assert.AreEqual(4,ActiveShadows(),
                "Four independent shadows only AFTER quarry was built");
            Assert.IsTrue(quarry.GetComponent<SpriteRenderer>().enabled);
            Assert.IsTrue(quarry.GetComponent<Collider2D>().enabled,
                "Built quarry must retain its actual interactive collider");
            Assert.IsFalse((bool)Property(bridge,"IsLiveBound"),
                "Visual preview cannot bind or write authoritative game state");
            Assert.NotNull(Camera.main);
            Assert.IsTrue(Camera.main.orthographic);
            Debug.Log("[Ciudad2D] PREMIUM_TRUE_PLAYMODE_PASS 4 candidate sprites, touch, HUD, read-only stages, shadow gates");
            yield return new ExitPlayMode();
        }
    }
}
#endif
