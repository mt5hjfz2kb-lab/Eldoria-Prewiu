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

namespace Eldoria.Experimentos.Ciudad2DDepthPilot.Tests
{
    /// <summary>Native Editor/PlayMode regression of the opt-in B scene only.</summary>
    public sealed class DepthPilotPlayModeTests
    {
        private const string ScenePath =
            "Assets/Experimentos/Ciudad2DDepthPilot/Valoria_Depth_B.unity";

        private static Type T(string name, string ns)
        {
            var value = Type.GetType(ns + "." + name + ", Assembly-CSharp");
            Assert.NotNull(value, "Missing assembly presentation type " + name);
            return value;
        }
        private static Type V(string name) =>
            T(name, "Eldoria.Experimentos.Ciudad2D");
        private static Type D(string name) =>
            T(name, "Eldoria.Experimentos.Ciudad2DDepthPilot");
        private static object Read(Component component, string property)
        {
            var field = component.GetType().GetProperty(property,
                BindingFlags.Instance | BindingFlags.Public);
            Assert.NotNull(field);
            return field.GetValue(component);
        }
        private static Component Get(Type type, Func<Component, bool> predicate = null)
        {
            var all = UnityEngine.Object.FindObjectsByType(type, FindObjectsSortMode.None)
                .Cast<Component>();
            var result = predicate == null ? all.FirstOrDefault() : all.FirstOrDefault(predicate);
            Assert.NotNull(result, "Required component " + type.Name);
            return result;
        }
        private static void Invoke(Component owner, string name, params object[] args)
        {
            var method = owner.GetType().GetMethod(name,
                BindingFlags.Instance | BindingFlags.Public);
            Assert.NotNull(method, name);
            method.Invoke(owner, args);
        }
        [UnityTest, Timeout(120000)]
        public IEnumerator PilotB_RealPlayMode_StatesOcclusionSelectionAndZoom()
        {
            Assert.IsTrue(File.Exists(Path.Combine(UnityEngine.Application.dataPath,
                "../" + ScenePath)), "B scene must be generated in isolated Editor proof");
            yield return new EnterPlayMode();
            var loaded = EditorSceneManager.LoadSceneAsyncInPlayMode(
                ScenePath, new LoadSceneParameters(LoadSceneMode.Single));
            Assert.NotNull(loaded);
            while (!loaded.isDone) yield return null;
            yield return null;
            Assert.AreEqual("Valoria_Depth_B", SceneManager.GetActiveScene().name);
            var bridge = Get(V("City2DStateBridge"));
            Assert.IsFalse((bool)Read(bridge, "IsLiveBound"));
            Assert.AreEqual(6, (int)Read(bridge, "RegisteredBuildingCount"));

            var workers = UnityEngine.Object.FindObjectsByType(
                D("DepthPilotActivity"), FindObjectsSortMode.None).Cast<Component>().ToArray();
            var contacts = UnityEngine.Object.FindObjectsByType(
                D("DepthPilotContact"), FindObjectsSortMode.None).Cast<Component>().ToArray();
            Assert.AreEqual(2, workers.Length);
            Assert.AreEqual(3, contacts.Length);
            foreach (var w in workers)
            {
                Assert.IsTrue((bool)Read(w, "PurePresentation"));
                Assert.IsEmpty(w.GetComponentsInChildren<Collider2D>());
            }
            foreach (var contact in contacts)
            {
                Assert.IsTrue((bool)Read(contact, "PurePresentation"));
                Assert.IsNull(contact.GetComponent<Collider2D>());
            }
            var saw = Get(V("CityBuilding"), x => Read(x, "BuildingId").ToString() == "aserradero");
            var granary = Get(V("CityBuilding"), x => Read(x, "BuildingId").ToString() == "granero");

            Invoke(bridge, "SetDemoBastionLevel", 1);
            yield return null;
            foreach (var w in workers)
                if (Read(w, "BuildingId").ToString() == "granary" ||
                    Read(w, "BuildingId").ToString() == "granero")
                    Assert.IsFalse((bool)Read(w, "Visible"));
            Assert.IsFalse((bool)Read(contacts.Single(x =>
                x.gameObject.name.EndsWith("granero")), "Visible"));

            Invoke(bridge, "SetDemoBastionLevel", 5);
            yield return null;
            Assert.IsTrue((bool)Read(granary, "Level").Equals(2));
            Assert.IsTrue((bool)Read(saw, "Level").Equals(2));
            foreach (var w in workers)
            {
                Invoke(w, "Sample", 1.25f);
                Assert.IsTrue((bool)Read(w, "Visible"));
            }
            foreach (var contact in contacts)
            {
                Invoke(contact, "Refresh");
                Assert.IsTrue((bool)Read(contact, "Visible"));
            }
            var visual = saw.GetComponent<SpriteRenderer>();
            var plume = GameObject.Find("AmbienteVivo_smoke_aserradero_1");
            Assert.NotNull(plume);
            Assert.Less(plume.GetComponent<SpriteRenderer>().sortingOrder,
                visual.sortingOrder, "Roof must occlude own chimney smoke");
            var glow = GameObject.Find("LuzEmisiva_entrada_bastion_oeste");
            Assert.NotNull(glow);
            Assert.Less(glow.GetComponent<SpriteRenderer>().sortingOrder,
                9000, "Glow cannot bypass all painter occlusion");

            Assert.NotNull(EventSystem.current);
            var click = new PointerEventData(EventSystem.current)
            {
                button = PointerEventData.InputButton.Left,
                position = Camera.main.WorldToScreenPoint(saw.transform.position)
            };
            Assert.IsTrue(ExecuteEvents.Execute(saw.gameObject, click,
                ExecuteEvents.pointerClickHandler), "Sawmill pointer regressed");

            var camera = Camera.main;
            Assert.NotNull(camera);
            Assert.IsTrue(camera.orthographic);
            var fit = Get(T("City2DCameraFit", "Eldoria.Experimentos.Ciudad2D"));
            Invoke(fit, "SetEvidenceViewport", 390, 844);
            Invoke(fit, "SetEvidenceZoom", 1.5f);
            Invoke(fit, "SetEvidenceFocus", new Vector2(-0.3f, -0.3f));
            Vector3 start = camera.transform.position;
            float size = camera.orthographicSize;
            Invoke(fit, "SetEvidenceFocus", new Vector2(0.3f, 0.3f));
            Assert.AreNotEqual(start, camera.transform.position,
                "Pan cannot be a repeated static image");
            Invoke(fit, "SetEvidenceZoom", 1.9f);
            Assert.Less(camera.orthographicSize, size);
            Invoke(fit, "ClearEvidenceViewport");
            Assert.IsFalse((bool)Read(bridge, "IsLiveBound"));
            Debug.Log("[DEPTH_PILOT] REAL_EDITOR_PLAYMODE_PRESENTATION_ONLY_PASS");
            yield return new ExitPlayMode();
        }
    }
}
#endif
