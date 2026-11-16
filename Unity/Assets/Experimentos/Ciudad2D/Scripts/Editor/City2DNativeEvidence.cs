#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Eldoria.Experimentos.Ciudad2D.Editor
{
    /// <summary>
    /// Capturas REALES del render de Unity, no composiciones externas.
    /// Invocable en CI: -batchmode -quit -force-d3d11 -projectPath Unity
    /// -executeMethod Eldoria.Experimentos.Ciudad2D.Editor.City2DNativeEvidence.Capture
    /// Sin introducir la escena en BuildSettings, sin tocar save/PlayerState.
    /// Nota: no es PlayMode ni prueba de dispositivos fisicos.
    /// </summary>
    public static class City2DNativeEvidence
    {
        private const string OutputFolder = "Ciudad2DNativeEvidence";
        private const string ExpectedSceneName = "Experimento_Ciudad2D.unity";

        [MenuItem("Eldoria/Experimentos/Ciudad 2D/Capturas nativas I-V")]
        public static void Capture()
        {
            var evidenceRoot = Path.GetFullPath(Path.Combine(Application.dataPath, "..", OutputFolder));
            Directory.CreateDirectory(evidenceRoot);
            // En ejecucion manual no descartar trabajo sin guardar.
            if (!Application.isBatchMode && !EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                return;
            var previousSetup = EditorSceneManager.GetSceneManagerSetup();
            Camera camera = null;
            RenderTexture priorTarget = null;
            var records = new List<string>();
            try
            {
                string reportPath = Path.Combine(evidenceRoot, "evidence.json");
                if (File.Exists(reportPath)) File.Delete(reportPath); // Nunca aceptar un PASS antiguo.
                City2DSceneBuilder.Rebuild();
                var scene = EditorSceneManager.OpenScene(City2DSceneBuilder.ScenePath, OpenSceneMode.Single);
                if (!scene.IsValid() || !scene.isLoaded || !scene.path.EndsWith(ExpectedSceneName, StringComparison.Ordinal))
                    throw new InvalidOperationException("No se ha abierto la escena aislada genuina");

                camera = Camera.main;
                if (camera == null) throw new InvalidOperationException("La escena carece de MainCamera");
                var fitting = camera.GetComponent<City2DCameraFit>();
                var ground = GameObject.Find("Terreno_Valoria");
                if (fitting == null || ground == null || ground.GetComponent<SpriteRenderer>()?.sprite == null)
                    throw new InvalidOperationException("Camara/terreno reales ausentes");

                var bridge = UnityEngine.Object.FindFirstObjectByType<City2DStateBridge>();
                if (bridge == null) throw new InvalidOperationException("Falta el puente de estado");
                bridge.RebindSceneBuildings(); // Debera hacer lo mismo tras deserializar en PlayMode.
                if (bridge.RegisteredBuildingCount != 6)
                    throw new InvalidOperationException("Los seis eventos de edificios no han sido registrados");

                var all = UnityEngine.Object.FindObjectsByType<CityBuilding>(FindObjectsSortMode.None);
                if (all.Length != 6) throw new InvalidOperationException("Se esperaban seis edificios");
                CityBuilding bastion = null;
                foreach (var b in all)
                {
                    if (b.BuildingId == "bastion") bastion = b;
                    if (b.Level <= 0 || b.SelectedArtLevel != 1)
                        throw new InvalidOperationException("Edificio sin primer hito: " + b.BuildingId);
                    var col = b.GetComponent<BoxCollider2D>();
                    if (col == null || !col.enabled || b.GetComponent<SpriteRenderer>()?.sprite == null)
                        throw new InvalidOperationException("Collider o sprite faltante: " + b.BuildingId);
                }
                if (bastion == null) throw new InvalidOperationException("Falta Bastion");
                if (CityBuilding.PlannedMilestone(4) != 1 || CityBuilding.PlannedMilestone(5) != 5 ||
                    CityBuilding.PlannedMilestone(9) != 5 || CityBuilding.PlannedMilestone(10) != 10)
                    throw new InvalidOperationException("La formula de hitos ha cambiado");

                // Comprobar transiciones sin modificar PlayerState ni save.
                bastion.SetLevel(4);
                AssertMilestone(bastion, 1);
                bastion.SetLevel(5);
                AssertMilestone(bastion, 5);
                bastion.SetLevel(9);
                AssertMilestone(bastion, 5);
                bastion.SetLevel(10);
                AssertMilestone(bastion, 5); // Aun no existe sprite de nivel 10.
                bastion.SetLevel(0);
                if (bastion.GetComponent<BoxCollider2D>().enabled ||
                    bastion.GetComponent<SpriteRenderer>().enabled)
                    throw new InvalidOperationException("Un edificio nivel 0 debe ocultarse");
                bastion.SetLevel(1);

                priorTarget = camera.targetTexture;
                CaptureOne(evidenceRoot, camera, fitting, bastion, 1, 390, 844, records);
                CaptureOne(evidenceRoot, camera, fitting, bastion, 5, 390, 844, records);
                CaptureOne(evidenceRoot, camera, fitting, bastion, 1, 844, 390, records);
                CaptureOne(evidenceRoot, camera, fitting, bastion, 5, 844, 390, records);
                CaptureOne(evidenceRoot, camera, fitting, bastion, 5, 1170, 2532, records);
                CaptureOne(evidenceRoot, camera, fitting, bastion, 5, 1600, 900, records);

                // Una captura por si misma nunca debe aprobar una experiencia de juego.
                string json = "{\n" +
                    "  \"engine\":\"Unity\",\n" +
                    "  \"mode\":\"Editor_RenderTexture_no_PlayMode\",\n" +
                    "  \"level_transition_pass\":true,\n" +
                    "  \"six_sprite_colliders_pass\":true,\n" +
                    "  \"game_state_modified\":false,\n" +
                    "  \"art_approved\":false,\n" +
                    "  \"device_qa\":\"PENDING\",\n" +
                    "  \"captures\":[\n" + string.Join(",\n", records) + "\n  ]\n}\n";
                File.WriteAllText(reportPath, json, Encoding.UTF8);
                Debug.Log("[Ciudad2D] NATIVE_RENDER_CAPTURE_PASS " + records.Count);
            }
            catch (Exception e)
            {
                Debug.LogError("[Ciudad2D] NATIVE_RENDER_CAPTURE_FAIL " + e);
                throw;
            }
            finally
            {
                if (camera != null) camera.targetTexture = priorTarget;
                // Respetar escenas previas del editor interactivo; CI es desechable.
                if (!Application.isBatchMode && previousSetup != null && previousSetup.Length > 0)
                    EditorSceneManager.RestoreSceneManagerSetup(previousSetup);
            }
        }

        private static void AssertMilestone(CityBuilding b, int expected)
        {
            if (b.SelectedArtLevel != expected)
                throw new InvalidOperationException("Hito incorrecto: nivel " + b.Level +
                    ", esperado " + expected + ", obtenido " + b.SelectedArtLevel);
        }

        private static void CaptureOne(string root, Camera cam, City2DCameraFit fit,
            CityBuilding bastion, int level, int width, int height, List<string> evidence)
        {
            bastion.SetLevel(level);
            AssertMilestone(bastion, level == 1 ? 1 : 5);
            fit.SetEvidenceViewport(width, height);
            var rt = new RenderTexture(width, height, 24, RenderTextureFormat.ARGB32);
            var previous = RenderTexture.active;
            var previousTarget = cam.targetTexture;
            Texture2D readback = null;
            try
            {
                rt.Create();
                cam.targetTexture = rt;
                cam.aspect = (float)width / height;
                cam.Render();
                RenderTexture.active = rt;
                readback = new Texture2D(width, height, TextureFormat.RGB24, false);
                readback.ReadPixels(new Rect(0, 0, width, height), 0, 0);
                readback.Apply();
                byte[] png = readback.EncodeToPNG();
                if (png == null || png.Length < 8000)
                    throw new InvalidOperationException("PNG de Unity vacio");
                string filename = "bastion-" + level + "-" + width + "x" + height + ".png";
                File.WriteAllBytes(Path.Combine(root, filename), png);
                using (var sha = SHA256.Create())
                {
                    string digest = BitConverter.ToString(sha.ComputeHash(png)).Replace("-", "").ToLowerInvariant();
                    evidence.Add("    {\"file\":\"" + filename + "\",\"width\":" + width +
                        ",\"height\":" + height + ",\"level\":" + level +
                        ",\"sha256\":\"" + digest + "\"}");
                }
            }
            finally
            {
                cam.targetTexture = previousTarget;
                RenderTexture.active = previous;
                if (readback != null) UnityEngine.Object.DestroyImmediate(readback);
                rt.Release();
                UnityEngine.Object.DestroyImmediate(rt);
            }
        }
    }
}
#endif
