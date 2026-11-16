#if UNITY_EDITOR
using System;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

namespace Eldoria.Experimentos.Ciudad2D.Editor
{
    [InitializeOnLoad]
    public static class City2DSceneBuilder
    {
        public const string Folder = "Assets/Experimentos/Ciudad2D/";
        public const string ScenePath = Folder + "Experimento_Ciudad2D.unity";
        private const string Sprites = Folder + "Sprites/";
        private const string SceneMarker = "Ciudad2D_EscenaGenerada";
        private static bool building;

        private struct Placement
        {
            public string Path;
            public string Name;
            public Vector2 PixelAnchor;
            public float Scale;
            public Placement(string path, string name, float x, float y, float scale)
            {
                Path = path;
                Name = name;
                PixelAnchor = new Vector2(x, y);
                Scale = scale;
            }
        }

        private static readonly Placement[] Buildings =
        {
            new Placement("Bastion.png", "Bastión", 470, 824, 0.293f),
            new Placement("Cuartel.png", "Cuartel", 206, 547, 0.189f),
            new Placement("SalonHeroes.png", "Salón de Héroes", 719, 546, 0.195f),
            new Placement("Aserradero.png", "Aserradero", 172, 964, 0.213f),
            new Placement("Granero.png", "Granero", 751, 962, 0.211f),
            new Placement("Cantera.png", "Cantera", 470, 1178, 0.225f),
        };

        static City2DSceneBuilder()
        {
            EditorApplication.delayCall += BuildIfReady;
        }

        [MenuItem("Eldoria/Experimentos/Ciudad 2D/Reconstruir escena")]
        public static void Rebuild()
        {
            Build(true);
        }

        public static void BuildIfReady()
        {
            Build(false);
        }

        private static void Build(bool forced)
        {
            if (building || EditorApplication.isCompiling || EditorApplication.isPlayingOrWillChangePlaymode)
                return;

            var terrain = AssetDatabase.LoadAssetAtPath<Sprite>(Sprites + "Terreno.png");
            if (terrain == null)
            {
                if (forced) Debug.LogWarning("[Ciudad2D] Falta Sprites/Terreno.png.");
                return;
            }
            foreach (var b in Buildings)
            {
                if (AssetDatabase.LoadAssetAtPath<Sprite>(Sprites + b.Path) == null)
                {
                    if (forced) Debug.LogWarning("[Ciudad2D] Falta Sprites/" + b.Path);
                    return;
                }
            }
            if (!forced && SceneAlreadyBuilt()) return;

            building = true;
            Scene current = SceneManager.GetActiveScene();
            Scene isolated = default;
            try
            {
                isolated = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Additive);
                SceneManager.SetActiveScene(isolated);

                var terrainObject = new GameObject("Terreno");
                var terrainRenderer = terrainObject.AddComponent<SpriteRenderer>();
                terrainRenderer.sprite = terrain;
                terrainRenderer.sortingOrder = -10000;
                terrainObject.transform.position = Vector3.zero;

                var cameraObject = new GameObject("Camera_Ciudad2D");
                cameraObject.tag = "MainCamera";
                cameraObject.transform.position = new Vector3(0f, 0f, -10f);
                var camera = cameraObject.AddComponent<Camera>();
                camera.orthographic = true;
                camera.clearFlags = CameraClearFlags.SolidColor;
                camera.backgroundColor = new Color(0.24f, 0.39f, 0.34f, 1f);
                camera.nearClipPlane = 0.01f;
                camera.farClipPlane = 100f;
                camera.transparencySortMode = TransparencySortMode.CustomAxis;
                camera.transparencySortAxis = Vector3.up;
                cameraObject.AddComponent<AudioListener>();
                cameraObject.AddComponent<Physics2DRaycaster>();
                cameraObject.AddComponent<City2DCameraFit>().SetGround(terrainRenderer);

                var events = new GameObject("EventSystem_Ciudad2D");
                events.AddComponent<EventSystem>();
                var input = events.AddComponent<InputSystemUIInputModule>();
                if (input.actionsAsset == null) input.AssignDefaultActions();

                foreach (var spec in Buildings)
                {
                    var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(Sprites + spec.Path);
                    var go = new GameObject(spec.Name);
                    var renderer = go.AddComponent<SpriteRenderer>();
                    renderer.sprite = sprite;
                    float x = (spec.PixelAnchor.x / 941f - 0.5f) * terrain.bounds.size.x;
                    float y = (0.5f - spec.PixelAnchor.y / 1672f) * terrain.bounds.size.y;
                    go.transform.position = new Vector3(x, y, -0.05f);
                    go.transform.localScale = Vector3.one * spec.Scale;
                    renderer.sortingOrder = -Mathf.RoundToInt(y * 100f);
                    var hit = go.AddComponent<BoxCollider2D>();
                    hit.size = new Vector2(sprite.bounds.size.x * 0.70f, sprite.bounds.size.y * 0.55f);
                    hit.offset = new Vector2(0f, sprite.bounds.size.y * 0.33f);
                    go.AddComponent<CityBuilding>().Configure(spec.Name, 10);
                }

                new GameObject(SceneMarker);
                if (!EditorSceneManager.SaveScene(isolated, ScenePath))
                    throw new InvalidOperationException("Unity no pudo guardar " + ScenePath);
                Debug.Log("[Ciudad2D] Escena 2D creada con 7 sprites y 6 edificios interactivos: " + ScenePath);
            }
            catch (Exception e)
            {
                Debug.LogError("[Ciudad2D] Falló la creación aislada de la escena: " + e);
            }
            finally
            {
                if (current.IsValid() && current.isLoaded) SceneManager.SetActiveScene(current);
                if (isolated.IsValid() && isolated.isLoaded)
                    EditorSceneManager.CloseScene(isolated, true);
                building = false;
            }
        }

        private static bool SceneAlreadyBuilt()
        {
            var asset = AssetDatabase.LoadAssetAtPath<SceneAsset>(ScenePath);
            if (asset == null) return false;
            var scene = SceneManager.GetSceneByPath(ScenePath);
            if (scene.IsValid() && scene.isLoaded)
                foreach (var root in scene.GetRootGameObjects())
                    if (root.name == SceneMarker) return true;
            var deps = AssetDatabase.GetDependencies(ScenePath, false);
            foreach (var dependency in deps)
                if (dependency == Sprites + "Bastion.png") return true;
            return false;
        }
    }
}
#endif
