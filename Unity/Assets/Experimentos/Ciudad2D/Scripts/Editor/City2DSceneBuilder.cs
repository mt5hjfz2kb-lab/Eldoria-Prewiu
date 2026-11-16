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
    /// <summary>
    /// Monta y guarda una escena de prueba auténtica cuando Unity importa los PNG.
    /// Trabaja SOLO en Assets/Experimentos/Ciudad2D; no incluye la escena en builds.
    /// </summary>
    [InitializeOnLoad]
    public static class City2DSceneBuilder
    {
        public const string Folder = "Assets/Experimentos/Ciudad2D/";
        public const string ScenePath = Folder + "Experimento_Ciudad2D.unity";
        private const string SpritesFolder = Folder + "Sprites/";
        private const string SceneMarker = "Ciudad2D_Piloto_Hitos_I_V";
        private static bool building;

        private struct Placement
        {
            public readonly string Id;
            public readonly string Filename;
            public readonly string Title;
            public readonly Vector2 PixelAnchor;
            public readonly float Scale;
            public Placement(string id, string filename, string title, float x, float y, float scale)
            {
                Id = id; Filename = filename; Title = title;
                PixelAnchor = new Vector2(x, y); Scale = scale;
            }
        }

        private static readonly Placement[] Subsidiaries =
        {
            new Placement("cuartel", "Cuartel.png", "Cuartel", 206, 547, 0.189f * 1254f / 860f),
            new Placement("salon", "SalonHeroes.png", "Salón de Héroes", 719, 546, 0.195f * 1254f / 860f),
            new Placement("aserradero", "Aserradero.png", "Aserradero", 172, 964, 0.213f * 1254f / 860f),
            new Placement("granero", "Granero.png", "Granero", 751, 962, 0.211f * 1254f / 860f),
            new Placement("cantera", "Cantera.png", "Cantera", 470, 1178, 0.225f * 1254f / 860f),
        };

        // Material de escena independiente, sin Collider ni reglas de gameplay.
        private static readonly Placement[] AmbientProps =
        {
            new Placement("decor_0","Props/ArbolAntiguo.png","ArbolAntiguo",319,443,0.44f),
            new Placement("decor_1","Props/MatojosFlores.png","MatojosFlores",608,400,0.30f),
            new Placement("decor_2","Props/MatojosFlores.png","MatojosFlores",611,799,0.27f),
            new Placement("decor_3","Props/MuroDerruido.png","MuroDerruido",493,368,0.30f),
            new Placement("decor_4","Props/CarroMadera.png","CarroMadera",290,1091,0.34f),
            new Placement("decor_5","Props/PozoPiedra.png","PozoPiedra",660,1115,0.34f),
            new Placement("decor_6","Props/AlmacenBarriles.png","AlmacenBarriles",815,1085,0.27f),
            new Placement("decor_7","Props/VallaLenador.png","VallaLenador",315,867,0.28f),
            new Placement("decor_8","Props/AntorchasGuardian.png","AntorchasGuardian",477,868,0.20f),
            new Placement("decor_9","Props/FisuraBrecha.png","FisuraBrecha",527,422,0.27f),
        };

        static City2DSceneBuilder() { EditorApplication.delayCall += BuildIfReady; }

        [MenuItem("Eldoria/Experimentos/Ciudad 2D/Reconstruir escena hitos I-V")]
        public static void Rebuild() { Build(true); }

        public static void BuildIfReady() { Build(false); }

        private static Sprite Load(string filename) =>
            AssetDatabase.LoadAssetAtPath<Sprite>(SpritesFolder + filename);

        private static void Build(bool forced)
        {
            if (building || EditorApplication.isCompiling || EditorApplication.isPlayingOrWillChangePlaymode)
                return;

            var terrain = Load("Terreno.png");
            var first = Load("Bastion_Nivel01.png");
            var fifth = Load("Bastion_Nivel05.png");
            if (terrain == null || first == null || fifth == null)
            {
                if (forced) Debug.LogWarning("[Ciudad2D] Faltan terreno o sprites de Bastión 01/05.");
                return;
            }
            foreach (var spec in Subsidiaries)
                if (Load(spec.Filename) == null)
                {
                    if (forced) Debug.LogWarning("[Ciudad2D] Falta Sprites/" + spec.Filename);
                    return;
                }

            foreach (var prop in AmbientProps)
                if (Load(prop.Filename) == null)
                {
                    if (forced) Debug.LogWarning("[Ciudad2D] Falta atrezzo " + prop.Filename);
                    return;
                }

            if (!forced && AlreadyBuilt()) return;
            building = true;
            Scene previouslyActive = SceneManager.GetActiveScene();
            Scene isolated = default;
            try
            {
                isolated = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Additive);
                SceneManager.SetActiveScene(isolated);

                var background = new GameObject("Terreno_Valoria");
                var groundRenderer = background.AddComponent<SpriteRenderer>();
                groundRenderer.sprite = terrain;
                groundRenderer.sortingOrder = -10000;

                var camRoot = new GameObject("CamaraOrto_Ciudad2D");
                camRoot.tag = "MainCamera";
                camRoot.transform.position = new Vector3(0, 0, -10f);
                var cam = camRoot.AddComponent<Camera>();
                cam.orthographic = true;
                cam.clearFlags = CameraClearFlags.SolidColor;
                cam.backgroundColor = new Color(0.11f, 0.17f, 0.22f, 1f);
                cam.nearClipPlane = 0.01f;
                cam.farClipPlane = 100f;
                cam.transparencySortMode = TransparencySortMode.CustomAxis;
                cam.transparencySortAxis = Vector3.up;
                camRoot.AddComponent<AudioListener>();
                camRoot.AddComponent<Physics2DRaycaster>();
                camRoot.AddComponent<City2DCameraFit>().SetGround(groundRenderer);

                var eventSystem = new GameObject("EventSystem_Ciudad2D");
                eventSystem.AddComponent<EventSystem>();
                var input = eventSystem.AddComponent<InputSystemUIInputModule>();
                if (input.actionsAsset == null) input.AssignDefaultActions();

                var bridgeRoot = new GameObject("SoloLectura_ProgresionVisual");
                var bridge = bridgeRoot.AddComponent<City2DStateBridge>();

                // Todos los sprites se ordenan por la cota de sus pies; no son UI pintada.
                // Bastión 1-4 e hito 5-9 cambian únicamente esta presentación.
                var bastion = new GameObject("Bastión");
                bastion.transform.position = PositionOf(470, 824, terrain);
                var bastionRenderer = bastion.AddComponent<SpriteRenderer>();
                bastionRenderer.sortingOrder = OrderFor(824, terrain);
                bastion.AddComponent<BoxCollider2D>();
                var bastionBuilding = bastion.AddComponent<CityBuilding>();
                bastionBuilding.Configure("bastion", "Bastión", 1, new[]
                {
                    new CityBuilding.VisualTier(1, first, 0.43f),
                    new CityBuilding.VisualTier(5, fifth, 0.47f),
                });
                bridge.Register(bastionBuilding);

                foreach (var spec in Subsidiaries)
                {
                    var sprite = Load(spec.Filename);
                    var obj = new GameObject(spec.Title);
                    obj.transform.position = PositionOf(spec.PixelAnchor.x, spec.PixelAnchor.y, terrain);
                    var renderer = obj.AddComponent<SpriteRenderer>();
                    renderer.sortingOrder = OrderFor(spec.PixelAnchor.y, terrain);
                    obj.AddComponent<BoxCollider2D>();
                    var buildingComponent = obj.AddComponent<CityBuilding>();
                    buildingComponent.Configure(spec.Id, spec.Title, 1,
                        new[] { new CityBuilding.VisualTier(1, sprite, spec.Scale) });
                    bridge.Register(buildingComponent);
                }

                // El atrezzo se ordena por altura de sus pies en el mismo espacio
                // isométrico de las construcciones. No se puede seleccionar.
                foreach (var spec in AmbientProps)
                {
                    var obj = new GameObject("Atrezzo_" + spec.Id);
                    obj.transform.position = PositionOf(spec.PixelAnchor.x, spec.PixelAnchor.y, terrain);
                    obj.transform.localScale = Vector3.one * spec.Scale;
                    var renderer = obj.AddComponent<SpriteRenderer>();
                    renderer.sprite = Load(spec.Filename);
                    renderer.sortingOrder = OrderFor(spec.PixelAnchor.y, terrain);
                }

                var uiRoot = new GameObject("ControlesVisuales_NoJugables");
                uiRoot.AddComponent<City2DPilotHud>().Configure(bridge);

                new GameObject(SceneMarker);
                if (!EditorSceneManager.SaveScene(isolated, ScenePath))
                    throw new InvalidOperationException("Unity no pudo escribir " + ScenePath);
                Debug.Log("[Ciudad2D] Escena I-V guardada: 8 sprites de escena + 9 atrezzo, 6 edificios, 2 hitos, " + ScenePath);
            }
            catch (Exception exception)
            {
                Debug.LogError("[Ciudad2D] Fallo al construir sandbox: " + exception);
            }
            finally
            {
                if (previouslyActive.IsValid() && previouslyActive.isLoaded)
                    SceneManager.SetActiveScene(previouslyActive);
                if (isolated.IsValid() && isolated.isLoaded)
                    EditorSceneManager.CloseScene(isolated, true);
                building = false;
            }
        }

        private static Vector3 PositionOf(float px, float py, Sprite terrain)
        {
            float x = (px / 941f - 0.5f) * terrain.bounds.size.x;
            float y = (0.5f - py / 1672f) * terrain.bounds.size.y;
            return new Vector3(x, y, -0.05f);
        }

        private static int OrderFor(float py, Sprite terrain)
        {
            float y = (0.5f - py / 1672f) * terrain.bounds.size.y;
            return -Mathf.RoundToInt(y * 100f);
        }

        private static bool AlreadyBuilt()
        {
            if (AssetDatabase.LoadAssetAtPath<SceneAsset>(ScenePath) == null) return false;
            var deps = AssetDatabase.GetDependencies(ScenePath, false);
            bool seenFirst = false, seenFifth = false;
            foreach (var path in deps)
            {
                if (path == SpritesFolder + "Bastion_Nivel01.png") seenFirst = true;
                if (path == SpritesFolder + "Bastion_Nivel05.png") seenFifth = true;
            }
            return seenFirst && seenFifth && Array.Exists(deps, x => x == SpritesFolder + "Props/ArbolAntiguo.png");
        }
    }
}
#endif
