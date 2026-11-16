#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

namespace Eldoria.Experimentos.Ciudad2D.Editor
{
    /// <summary>Sandbox I/V; JSON one layout for browser, QA and Unity. Never edits production scenes.</summary>
    [InitializeOnLoad]
    public static class City2DSceneBuilder
    {
        public const string Folder = "Assets/Experimentos/Ciudad2D/";
        public const string ScenePath = Folder + "Experimento_Ciudad2D.unity";
        private const string SpritesFolder = Folder + "Sprites/";
        private const string LayoutPath = Folder + "QA/city_layout.json";
        private const string SceneMarker = "Ciudad2D_Organica_I_V_Layout3";
        private static bool building;

        [Serializable] private sealed class Layout
        {
            public int schema;
            public int coordinate_width;
            public int coordinate_height;
            public BuildingPlacement[] buildings;
            public Decoration[] decor;
            public Glow[] glows;
        }
        [Serializable] private sealed class BuildingPlacement
        {
            public string id, title, filename, level5_filename;
            public float x, y, scale, level5_scale;
        }
        [Serializable] private sealed class Decoration
        {
            public string id, filename;
            public float x, y, scale;
        }
        [Serializable] private sealed class Glow
        {
            public string id;
            public float x, y, radius, opacity;
        }

        static City2DSceneBuilder() { EditorApplication.delayCall += BuildIfReady; }
        [MenuItem("Eldoria/Experimentos/Ciudad 2D/Reconstruir escena organica I-V")]
        public static void Rebuild() { Build(true); }
        public static void BuildIfReady() { Build(false); }

        private static Sprite Sprite(string path) => AssetDatabase.LoadAssetAtPath<Sprite>(SpritesFolder + path);
        private static Layout ReadLayout()
        {
            var asset = AssetDatabase.LoadAssetAtPath<TextAsset>(LayoutPath);
            if (asset == null) return null;
            var result = JsonUtility.FromJson<Layout>(asset.text);
            if (result == null || result.schema != 3 || result.coordinate_width != 941 ||
                result.coordinate_height != 1672 || result.buildings == null ||
                result.buildings.Length != 6 || result.decor == null || result.glows == null)
                throw new InvalidOperationException("Layout3 is invalid; refuse rebuilding sandbox");
            var ids = new HashSet<string>(StringComparer.Ordinal);
            foreach (var spec in result.buildings)
            {
                if (spec == null || string.IsNullOrEmpty(spec.id) || !ids.Add(spec.id) ||
                    spec.scale <= 0 || spec.x < 0 || spec.x > 941 || spec.y < 0 || spec.y > 1672 ||
                    Sprite(spec.filename) == null ||
                    (!string.IsNullOrEmpty(spec.level5_filename) && Sprite(spec.level5_filename) == null))
                    throw new InvalidOperationException("Invalid/missing building layout sprite: " + (spec?.id ?? "null"));
            }
            foreach (var spec in result.decor)
                if (spec == null || spec.scale <= 0 || Sprite(spec.filename) == null)
                    throw new InvalidOperationException("Invalid/missing decoration in layout");
            if (Sprite("Props/HaloAntorcha.png") == null) throw new InvalidOperationException("Missing halo sprite");
            return result;
        }

        private static void Build(bool forced)
        {
            if (building || EditorApplication.isCompiling || EditorApplication.isPlayingOrWillChangePlaymode)
                return;
            var terrain = Sprite("Terreno.png");
            if (terrain == null) return;
            Layout layout;
            try { layout = ReadLayout(); }
            catch (Exception e) { Debug.LogError("[Ciudad2D] " + e); return; }
            if (layout == null) { if (forced) Debug.LogWarning("[Ciudad2D] Missing layout3"); return; }
            if (!forced && AlreadyBuilt()) return;
            building = true;
            Scene prior = SceneManager.GetActiveScene();
            Scene isolated = default;
            try
            {
                isolated = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Additive);
                SceneManager.SetActiveScene(isolated);
                var ground = new GameObject("Terreno_Valoria").AddComponent<SpriteRenderer>();
                ground.sprite = terrain;
                ground.sortingOrder = -10000;
                var camObject = new GameObject("CamaraOrto_Ciudad2D");
                camObject.tag = "MainCamera";
                camObject.transform.position = new Vector3(0,0,-10);
                var cam = camObject.AddComponent<Camera>();
                cam.orthographic = true;
                cam.clearFlags = CameraClearFlags.SolidColor;
                cam.backgroundColor = new Color(.11f,.17f,.22f,1);
                cam.nearClipPlane = .01f; cam.farClipPlane = 100;
                cam.transparencySortMode = TransparencySortMode.CustomAxis;
                cam.transparencySortAxis = Vector3.up;
                camObject.AddComponent<AudioListener>();
                camObject.AddComponent<Physics2DRaycaster>();
                camObject.AddComponent<City2DCameraFit>().SetGround(ground);

                var eventRoot = new GameObject("EventSystem_Ciudad2D");
                eventRoot.AddComponent<EventSystem>();
                var input = eventRoot.AddComponent<InputSystemUIInputModule>();
                if (input.actionsAsset == null) input.AssignDefaultActions();
                var bridge = new GameObject("SoloLectura_ProgresionVisual").AddComponent<City2DStateBridge>();

                foreach (var spec in layout.buildings)
                {
                    var obj = new GameObject(spec.title);
                    obj.transform.position = PositionOf(spec.x,spec.y,terrain);
                    var renderer = obj.AddComponent<SpriteRenderer>();
                    renderer.sortingOrder = OrderFor(spec.y,terrain);
                    obj.AddComponent<BoxCollider2D>();
                    var tiers = string.IsNullOrEmpty(spec.level5_filename)
                        ? new[] { new CityBuilding.VisualTier(1,Sprite(spec.filename),spec.scale) }
                        : new[] {
                            new CityBuilding.VisualTier(1,Sprite(spec.filename),spec.scale),
                            new CityBuilding.VisualTier(5,Sprite(spec.level5_filename),spec.level5_scale)
                        };
                    var buildingComponent = obj.AddComponent<CityBuilding>();
                    buildingComponent.Configure(spec.id,spec.title,1,tiers);
                    bridge.Register(buildingComponent);
                }
                foreach (var spec in layout.decor)
                {
                    var obj = new GameObject("Atrezzo_" + spec.id);
                    obj.transform.position = PositionOf(spec.x,spec.y,terrain);
                    obj.transform.localScale = Vector3.one * spec.scale;
                    var renderer = obj.AddComponent<SpriteRenderer>();
                    renderer.sprite = Sprite(spec.filename);
                    renderer.sortingOrder = OrderFor(spec.y,terrain);
                    // Non-interactive: no collider or saved state.
                }
                foreach (var spec in layout.glows)
                {
                    var obj = new GameObject("LuzEmisiva_" + spec.id);
                    obj.transform.position = PositionOf(spec.x,spec.y,terrain);
                    obj.transform.localScale = Vector3.one * (spec.radius * 2 / 96f);
                    var renderer = obj.AddComponent<SpriteRenderer>();
                    renderer.sprite = Sprite("Props/HaloAntorcha.png");
                    renderer.sortingOrder = 9000;
                    renderer.color = new Color(1f,1f,1f,Mathf.Clamp01(spec.opacity));
                    obj.AddComponent<City2DTorchPulse>().Configure(spec.opacity, spec.x * .017f);
                }
                new GameObject("ControlesVisuales_NoJugables").AddComponent<City2DPilotHud>().Configure(bridge);
                new GameObject(SceneMarker);
                if (!EditorSceneManager.SaveScene(isolated, ScenePath))
                    throw new InvalidOperationException("Unity could not save isolated scene");
                Debug.Log("[Ciudad2D] Scene Layout3 saved with " + layout.buildings.Length +
                          " buildings, " + layout.decor.Length + " props, " + layout.glows.Length + " light overlays");
            }
            catch (Exception e) { Debug.LogError("[Ciudad2D] Build failed " + e); throw; }
            finally
            {
                if (prior.IsValid() && prior.isLoaded) SceneManager.SetActiveScene(prior);
                if (isolated.IsValid() && isolated.isLoaded)
                    EditorSceneManager.CloseScene(isolated,true);
                building = false;
            }
        }
        private static Vector3 PositionOf(float x,float y,Sprite terrain)
        {
            return new Vector3((x/941f-.5f)*terrain.bounds.size.x,
                (.5f-y/1672f)*terrain.bounds.size.y,-.05f);
        }
        private static int OrderFor(float y,Sprite terrain)
        {
            return -Mathf.RoundToInt((.5f-y/1672f)*terrain.bounds.size.y*100f);
        }
        private static bool AlreadyBuilt()
        {
            if (AssetDatabase.LoadAssetAtPath<SceneAsset>(ScenePath) == null) return false;
            var deps = AssetDatabase.GetDependencies(ScenePath,false);
            foreach (var required in new[] {
                SpritesFolder+"Bastion_Nivel01.png", SpritesFolder+"Bastion_Nivel05.png",
                SpritesFolder+"Props/AtalayaValoria.png", SpritesFolder+"Props/HaloAntorcha.png" })
                if (Array.IndexOf(deps,required) < 0) return false;
            return true;
        }
    }
}
#endif
