#if UNITY_EDITOR
using System;
using System.Collections;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using NUnit.Framework;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;

// Editor-only test assembly; reflection permits safely interrogating the
// predefined Assembly-CSharp without changing any production assembly.
namespace Eldoria.Experimentos.Ciudad2D.PlayModeProof
{
    public sealed class RealSceneInteractions
    {
        private const string ScenePath = "Assets/Experimentos/Ciudad2D/Experimento_Ciudad2D.unity";
        private const BindingFlags PublicInstance = BindingFlags.Instance | BindingFlags.Public;

        private static Type RuntimeType(string name)
        {
            Type found = Type.GetType("Eldoria.Experimentos.Ciudad2D." + name + ", Assembly-CSharp");
            Assert.NotNull(found, "Missing runtime MonoBehaviour " + name);
            return found;
        }

        private static object Value(Component c, string property)
        {
            var pi = c.GetType().GetProperty(property, PublicInstance);
            Assert.NotNull(pi, "Runtime contract missing " + property);
            return pi.GetValue(c);
        }

        private static Component Find(string type, string key = null)
        {
            var components = UnityEngine.Object.FindObjectsByType(RuntimeType(type), FindObjectsSortMode.None);
            Assert.IsNotEmpty(components, "Runtime component not found: " + type);
            if (key == null) return (Component)components[0];
            foreach (var candidate in components)
            {
                var component = (Component)candidate;
                if (string.Equals(Value(component, "BuildingId")?.ToString(), key, StringComparison.Ordinal))
                    return component;
            }
            Assert.Fail("Missing actual building " + key);
            return null;
        }

        private static void Call(Component component, string method, params object[] arguments)
        {
            MethodInfo info = component.GetType().GetMethod(method, PublicInstance);
            Assert.NotNull(info, "Missing method " + method);
            try { info.Invoke(component, arguments); }
            catch (TargetInvocationException ex) { throw ex.InnerException ?? ex; }
        }

        private static void SetDomainMember(object value, string name, object next)
        {
            var type = value.GetType();
            var field = type.GetField(name, PublicInstance);
            if (field != null) { field.SetValue(value, next); return; }
            var property = type.GetProperty(name, PublicInstance);
            Assert.NotNull(property, "Domain snapshot missing " + name);
            property.SetValue(value, next);
        }

        private static object ReadDomainMember(object value, string name)
        {
            var type = value.GetType();
            var field = type.GetField(name, PublicInstance);
            if (field != null) return field.GetValue(value);
            var property = type.GetProperty(name, PublicInstance);
            Assert.NotNull(property, "Domain snapshot missing " + name);
            return property.GetValue(value);
        }

        private static string Plot(Component b) => Value(b, "PlotPhase").ToString();
        private static int Level(Component b) => Convert.ToInt32(Value(b, "Level"));

        private static void AssertBuilding(string id, string phase, bool visible)
        {
            var b = Find("CityBuilding", id);
            Assert.AreEqual(phase, Plot(b), id + " phase");
            var renderer = b.GetComponent<SpriteRenderer>();
            Assert.NotNull(renderer);
            Assert.AreEqual(visible, renderer.enabled, id + " renderer visibility");
            var collider = b.GetComponent<Collider2D>();
            Assert.NotNull(collider);
            if (phase == "Locked") Assert.IsFalse(collider.enabled, id + " cannot be touched before unlock");
        }

        [UnityTest, Timeout(120000)]
        public IEnumerator GenuineSavedScene_EnteredPlayMode_HudPointerAndCameraContract()
        {
            // This is a REAL transition to Editor PlayMode (not an edit-mode
            // RenderTexture proof). Does not include the scene in Build Settings.
            yield return new EnterPlayMode();
            Assert.IsTrue(Application.isPlaying, "Unity did not enter genuine PlayMode");

            Assert.IsTrue(File.Exists(Path.Combine(Application.dataPath, "../" + ScenePath)));
            var pending = EditorSceneManager.LoadSceneAsyncInPlayMode(
                ScenePath, new LoadSceneParameters(LoadSceneMode.Single));
            Assert.NotNull(pending, "Unity cannot load isolated scene in PlayMode");
            while (!pending.isDone) yield return null;
            yield return null;
            yield return null;

            Assert.AreEqual("Experimento_Ciudad2D", SceneManager.GetActiveScene().name);
            var bridge = Find("City2DStateBridge");
            Assert.AreEqual(6, Convert.ToInt32(Value(bridge, "RegisteredBuildingCount")));
            Assert.IsFalse((bool)Value(bridge, "IsLiveBound"), "Demo must not write a real save");
            AssertBuilding("bastion", "Built", true);
            AssertBuilding("aserradero", "Available", false);
            AssertBuilding("cuartel", "Locked", false);
            AssertBuilding("granero", "Locked", false);
            AssertBuilding("cantera", "Locked", false);
            AssertBuilding("salon", "Locked", false);

            Assert.NotNull(EventSystem.current, "EventSystem missing in PlayMode");
            var buttons = UnityEngine.Object.FindObjectsByType<Button>(FindObjectsSortMode.None);
            Assert.GreaterOrEqual(buttons.Length, 5, "HUD chapter buttons did not initialize");
            Button chapterThree = buttons.FirstOrDefault(b => b.name == "Hito_III");
            Assert.NotNull(chapterThree, "Chapter III HUD not operational");

            var eventData = new PointerEventData(EventSystem.current) {
                button = PointerEventData.InputButton.Left
            };
            // Go through the real EventSystem pointerClickHandler, not by
            // invoking the chapter bridge directly.
            bool handled = ExecuteEvents.Execute(chapterThree.gameObject, eventData,
                ExecuteEvents.pointerClickHandler);
            Assert.IsTrue(handled, "HUD cannot receive simulated pointer click in PlayMode");
            yield return null;
            Assert.AreEqual(3, Convert.ToInt32(Value(bridge, "DemoBastionLevel")));
            AssertBuilding("aserradero", "Built", true);
            AssertBuilding("cuartel", "Built", true);
            AssertBuilding("granero", "Available", false);
            AssertBuilding("cantera", "Locked", false);

            var cuartel = Find("CityBuilding", "cuartel");
            var cuartelCollider = cuartel.GetComponent<BoxCollider2D>();
            Physics2D.SyncTransforms();
            Assert.IsTrue(cuartelCollider.enabled, "Finished barracks has no input collider");
            var worldPoint = cuartelCollider.bounds.center;
            var hit = Physics2D.OverlapPoint(worldPoint);
            Assert.NotNull(hit, "A visible Cuartel has no physical pointer target");
            Assert.AreEqual(cuartelCollider, hit, "Cuartel collider is not the hit target");

            var cam = Camera.main;
            Assert.NotNull(cam);
            Assert.IsTrue(cam.orthographic, "City camera must be orthographic");
            Assert.Greater(cam.orthographicSize, 0f);
            var screenPoint = cam.WorldToScreenPoint(cuartel.transform.position);
            Assert.IsFalse(float.IsNaN(screenPoint.x) || float.IsNaN(screenPoint.y));
            eventData.position = new Vector2(screenPoint.x, screenPoint.y);
            handled = ExecuteEvents.Execute(cuartel.gameObject, eventData,
                ExecuteEvents.pointerClickHandler);
            Assert.IsTrue(handled, "Cuartel does not process pointerClick");
            yield return null;

            var labels = UnityEngine.Object.FindObjectsByType<Text>(FindObjectsSortMode.None);
            Assert.IsTrue(labels.Any(t => t.text != null && t.text.Contains("Cuartel") &&
                t.text.Contains("Built")), "HUD selection did not update after real component click");

            // Authoritative DOMAIN contract replayed as an ephemeral fixture.
            // No ICommandGateway write, no save file and no upgrade action.
            var stateType = Type.GetType("Eldoria.Domain.PlayerState, Assembly-CSharp");
            var chapterType = Type.GetType("Eldoria.Domain.BastionThreeState, Assembly-CSharp");
            Assert.NotNull(stateType, "Cannot resolve real domain PlayerState");
            Assert.NotNull(chapterType, "Cannot resolve real chapter state");
            object state = Activator.CreateInstance(stateType);
            object chapter = Activator.CreateInstance(chapterType);
            SetDomainMember(state, "BastionLevel", 5);
            SetDomainMember(state, "SawmillLevel", 2);
            SetDomainMember(state, "BarracksLevel", 2);
            SetDomainMember(chapter, "GranaryLevel", 1);
            SetDomainMember(chapter, "StoneworksLevel", 0);
            SetDomainMember(state, "ChapterThree", chapter);
            Call(bridge, "ApplySnapshot", state);
            yield return null;
            AssertBuilding("cantera", "Available", false);
            SetDomainMember(chapter, "StoneworksLevel", 1);
            Call(bridge, "ApplySnapshot", state);
            yield return null;
            AssertBuilding("cantera", "Built", true);
            Assert.AreEqual(1, Convert.ToInt32(ReadDomainMember(chapter, "StoneworksLevel")),
                "Presentation must never mutate domain progress");
            Assert.IsFalse((bool)Value(bridge, "IsLiveBound"), "No gateway write may occur");

            var fit = Find("City2DCameraFit");
            Call(fit, "SetEvidenceViewport", 1170, 2532);
            Assert.Greater(cam.orthographicSize, 0f);
            Assert.Less(cam.orthographicSize, 20f);
            CapturePlaymodeCamera(cam, "city2d-playmode-iPhone-1170x2532.png");
            Call(fit, "SetEvidenceViewport", 844, 390);
            Assert.Greater(cam.orthographicSize, 0f);
            CapturePlaymodeCamera(cam, "city2d-playmode-horizontal-844x390.png");
            Call(fit, "ClearEvidenceViewport");

            Assert.IsFalse((bool)Value(bridge, "IsLiveBound"));
            Debug.Log("[Ciudad2D] REAL_PLAYMODE_PASS scene_loaded=true pointer_dispatch=true " +
                "HUD_updated=true camera_both_orientations=true gateway_write=false");
            yield return new ExitPlayMode();
        }

        private static void CapturePlaymodeCamera(Camera camera, string fileName)
        {
            var path = Path.GetFullPath(Path.Combine(
                Application.dataPath, "../Ciudad2DPlayModeEvidence", fileName));
            Directory.CreateDirectory(Path.GetDirectoryName(path));
            int w = fileName.Contains("horizontal") ? 844 : 1170;
            int h = fileName.Contains("horizontal") ? 390 : 2532;
            var target = new RenderTexture(w, h, 24, RenderTextureFormat.ARGB32);
            var priorTarget = camera.targetTexture;
            var priorActive = RenderTexture.active;
            Texture2D readback = null;
            try
            {
                target.Create();
                camera.targetTexture = target;
                camera.aspect = (float)w / h;
                camera.Render();
                RenderTexture.active = target;
                readback = new Texture2D(w, h, TextureFormat.RGB24, false);
                readback.ReadPixels(new Rect(0, 0, w, h), 0, 0);
                readback.Apply();
                var bytes = readback.EncodeToPNG();
                Assert.Greater(bytes.Length, 8000, "Empty genuine PlayMode image");
                File.WriteAllBytes(path, bytes);
                using (var sha = SHA256.Create())
                {
                    string digest = BitConverter.ToString(sha.ComputeHash(bytes))
                        .Replace("-", "").ToLowerInvariant();
                    Debug.Log("[Ciudad2D] PLAYMODE_CAMERA_CAPTURE " + fileName +
                        " sha256=" + digest + " bytes=" + bytes.Length);
                }
            }
            finally
            {
                camera.targetTexture = priorTarget;
                RenderTexture.active = priorActive;
                if (readback != null) UnityEngine.Object.DestroyImmediate(readback);
                target.Release();
                UnityEngine.Object.DestroyImmediate(target);
            }
        }
    }
}
#endif
