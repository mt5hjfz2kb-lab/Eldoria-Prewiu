using System;
using Eldoria.Application;
using Eldoria.Presentation;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Eldoria.Valoria25D
{
    /// <summary>
    /// Explicit bridge to the existing, unchanged Frontier 4X scene.
    /// Shares the one actual LocalGateway during world actions, then reloads
    /// the 2.5D city with the exact same canonical persisted save.
    /// No world mesh, rules, fight, march or resource is authored here.
    /// The temporary original SlicePresenter belongs ONLY to the Frontier
    /// scene, so it cannot survive back into the 2.5D city as a second HUD.
    /// </summary>
    public static class City25DWorldTransition
    {
        public const string CityScene = "Valoria25D_Gameplay";
        public const string ExistingWorldScene = "Frontier";
        private static ICommandGateway pendingGateway;
        private static int lastOpenFrame = -1;
        public static bool HasPendingWorldEntry => pendingGateway != null;

        public static bool TryEnter(ICommandGateway gateway, out string error)
        {
            error = null;
            if (gateway == null)
            {
                error = "No se ha cargado la partida real.";
                return false;
            }
            if (pendingGateway != null || lastOpenFrame == Time.frameCount)
            {
                error = "Ya se está abriendo el mapa 4X.";
                return false;
            }
            if (SceneManager.GetActiveScene().name != CityScene)
            {
                error = "Debes estar en Valoria para abrir el mapa.";
                return false;
            }
            if (!UnityEngine.Application.CanStreamedLevelBeLoaded(ExistingWorldScene))
            {
                error = "El mundo 4X original no está incluido en esta candidata.";
                return false;
            }
            lastOpenFrame = Time.frameCount;
            try
            {
                gateway.Advance();
                pendingGateway = gateway;
                SceneManager.sceneLoaded -= OnExistingWorldLoaded;
                SceneManager.sceneLoaded += OnExistingWorldLoaded;
                SceneManager.LoadScene(ExistingWorldScene, LoadSceneMode.Single);
                Debug.Log("VALORIA25D_WORLD_ENTRY_REQUESTED existing_world=true canonical_gateway=true");
                return true;
            }
            catch (Exception ex)
            {
                SceneManager.sceneLoaded -= OnExistingWorldLoaded;
                pendingGateway = null;
                error = "No se pudo abrir el mundo: " + ex.Message;
                Debug.LogError("VALORIA25D_WORLD_NAV_FAILURE " + ex);
                return false;
            }
        }

        private static void OnExistingWorldLoaded(Scene loaded, LoadSceneMode mode)
        {
            SceneManager.sceneLoaded -= OnExistingWorldLoaded;
            var existingGateway = pendingGateway;
            pendingGateway = null;
            if (loaded.name != ExistingWorldScene || existingGateway == null)
            {
                Debug.LogError("VALORIA25D_UNEXPECTED_WORLD_LOAD: " + loaded.name);
                return;
            }
            try
            {
                if (UnityEngine.Object.FindFirstObjectByType<SlicePresenter>() != null)
                    throw new InvalidOperationException("A second world presenter already exists");
                var worldOnly = new GameObject("Valoria25D_ExistingWorldPresenterOnly");
                // DO NOT mark this object DontDestroyOnLoad. Destruction with
                // Frontier prevents the main 3D HUD entering the 2.5D city.
                var presenter = worldOnly.AddComponent<SlicePresenter>();
                presenter.Initialize(existingGateway);
                presenter.HideOwnerResetInExternalWorld=true;
                presenter.ExternalSceneNameResolver = name =>
                    name == "Valoria" || name == "ValoriaWebGL" || name == CityScene
                        ? CityScene : name;
                presenter.OnSceneLoaded(loaded,mode);
                // Add only a small command surface into the original World HUD.
                // No second canvas, event system, camera or replacement 4X art.
                var missions=new GameObject("Valoria25D_CanonicalWorldActions");
                missions.transform.SetParent(worldOnly.transform,false);
                missions.AddComponent<City25DExistingWorldMissions>().Configure(existingGateway);
                if (UnityEngine.Object.FindFirstObjectByType<SlicePresenter>() != presenter)
                    throw new InvalidOperationException("The original world did not keep one presenter");
                Debug.Log("VALORIA25D_ORIGINAL_FRONTIER_READY owner_gateway_shared=true "+
                          "world_art_unmodified=true legacy_presenter_scene_only=true");
            }
            catch (Exception error)
            {
                Debug.LogError("VALORIA25D_EXISTING_WORLD_INIT_FAILURE "+error);
                // Never fabricate a new scene or silently lose canonical save
                // when the existing world fails to initialize.
            }
        }
    }
}
