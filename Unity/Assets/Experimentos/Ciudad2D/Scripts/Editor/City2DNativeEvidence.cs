#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using UnityEditor;
using Eldoria.Domain;
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
            var evidenceRoot = Path.GetFullPath(Path.Combine(UnityEngine.Application.dataPath, "..", OutputFolder));
            Directory.CreateDirectory(evidenceRoot);
            // En ejecucion manual no descartar trabajo sin guardar.
            if (!UnityEngine.Application.isBatchMode && !EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                return;
            var previousSetup = EditorSceneManager.GetSceneManagerSetup();
            // Unity batchmode starts from an unnamed, unsaved default scene.
            // An additive scene cannot be created from that state in Unity 6.3.
            // Save an isolated, disposable scratch scene before invoking the builder.
            if (UnityEngine.Application.isBatchMode)
            {
                var scratch = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
                const string scratchPath = City2DSceneBuilder.Folder + "QA/City2D_NativeScratch.unity";
                if (!EditorSceneManager.SaveScene(scratch, scratchPath))
                    throw new InvalidOperationException("Could not save disposable City2D native scratch scene");
            }
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
                if (ground.GetComponent<SpriteRenderer>().sprite.name != "Terreno_Organico")
                    throw new InvalidOperationException("Escena nativa no usa el candidato orgánico versionado");

                VerifyUncompressedSpriteImports();
                if (ground.GetComponent<SpriteRenderer>().sprite.texture.width != 1412 ||
                    ground.GetComponent<SpriteRenderer>().sprite.texture.height != 2508)
                    throw new InvalidOperationException("Retina terrain absent from authentic Unity render");

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
                    City2DPlotPhase expected = b.BuildingId == "bastion" ? City2DPlotPhase.Built :
                        b.BuildingId == "aserradero" ? City2DPlotPhase.Available : City2DPlotPhase.Locked;
                    if (b.PlotPhase != expected)
                        throw new InvalidOperationException("Parcela prematura: " + b.BuildingId + " " + b.PlotPhase);
                    var col = b.GetComponent<BoxCollider2D>();
                    var renderer = b.GetComponent<SpriteRenderer>();
                    if (col == null || renderer == null || renderer.sprite == null ||
                        col.enabled != (expected != City2DPlotPhase.Locked) ||
                        renderer.enabled != (expected == City2DPlotPhase.Built))
                        throw new InvalidOperationException("Edificio visible o collider incorrecto: " + b.BuildingId);
                }
                if (bastion == null) throw new InvalidOperationException("Falta Bastion");
                if (CityBuilding.PlannedMilestone(4) != 1 || CityBuilding.PlannedMilestone(5) != 5 ||
                    CityBuilding.PlannedMilestone(9) != 5 || CityBuilding.PlannedMilestone(10) != 10)
                    throw new InvalidOperationException("La formula de hitos ha cambiado");

                // Usar el mismo PlayerState real que recibe el juego: SOLO LECTURA.
                // El objeto es una fixture temporal, no un guardado real.
                var fixture = new PlayerState
                {
                    BastionLevel = 4,
                    SawmillLevel = 2,
                    BarracksLevel = 2,
                    ChapterThree = new BastionThreeState
                    {
                        GranaryLevel = 2,
                        StoneworksLevel = 0
                    }
                };
                bridge.ApplySnapshot(fixture);
                if (bastion.Level != 4 || bastion.SelectedArtLevel != 1)
                    throw new InvalidOperationException("Snapshot de nivel 4 no mostro hito I");
                fixture.BastionLevel = 5;
                bridge.ApplySnapshot(fixture);
                if (bastion.Level != 5 || bastion.SelectedArtLevel != 5)
                    throw new InvalidOperationException("Snapshot de nivel 5 no mostro hito V");
                if (fixture.SawmillLevel != 2 || fixture.BarracksLevel != 2 ||
                    fixture.ChapterThree.GranaryLevel != 2 || fixture.ChapterThree.StoneworksLevel != 0)
                    throw new InvalidOperationException("La presentacion altero la fixture de partida");

                // A corrupted or legacy-invalid snapshot must not display a future building.
                fixture.BastionLevel = 4; // Test truly pre-V, after testing the V transition above.
                fixture.ChapterThree.StoneworksLevel = 1;
                if (City2DProgressionPolicy.FromSnapshot(fixture, "cantera").Phase != City2DPlotPhase.Locked)
                    throw new InvalidOperationException("Invalid pre-V stoneworks appeared early");
                fixture.ChapterThree.StoneworksLevel = 0;
                if (City2DProgressionPolicy.FromSnapshot(fixture, "cantera").Phase != City2DPlotPhase.Locked)
                    throw new InvalidOperationException("Cantera disponible antes de Bastión V");
                fixture.BastionLevel = 5;
                bridge.ApplySnapshot(fixture);
                if (City2DProgressionPolicy.FromSnapshot(fixture, "cantera").Phase != City2DPlotPhase.Available)
                    throw new InvalidOperationException("La cantera debe ser una parcela, no una cantera construida");
                fixture.BuildingTaskId = "c3:stoneworks:qa-only";
                fixture.BuildingCompletesUtcTicks = 123456789;
                bridge.ApplySnapshot(fixture);
                var quarry = Array.Find(all, item => item.BuildingId == "cantera");
                if (quarry == null || quarry.PlotPhase != City2DPlotPhase.UnderConstruction ||
                    quarry.GetComponent<SpriteRenderer>().enabled)
                    throw new InvalidOperationException("Obra visible indebidamente como edificio terminado");
                fixture.BuildingCompletesUtcTicks = 0;
                fixture.BuildingTaskId = "";
                fixture.ChapterThree.StoneworksLevel = 1;
                bridge.ApplySnapshot(fixture);
                if (quarry.PlotPhase != City2DPlotPhase.Built || !quarry.GetComponent<SpriteRenderer>().enabled)
                    throw new InvalidOperationException("La cantera construida no aparece al terminar");
                if (fixture.BastionLevel != 5 || fixture.ChapterThree.StoneworksLevel != 1)
                    throw new InvalidOperationException("La presentación mutó los datos de la fixture");
                
                // Comprobar transiciones sin modificar datos ni save.
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
                foreach (var chapter in new[] {1,2,3,4,5})
                {
                    CaptureOne(evidenceRoot, camera, fitting, bastion, chapter, 390, 844, records);
                    CaptureOne(evidenceRoot, camera, fitting, bastion, chapter, 844, 390, records);
                }
                CaptureOne(evidenceRoot, camera, fitting, bastion, 5, 1170, 2532, records);
                CaptureOne(evidenceRoot, camera, fitting, bastion, 5, 1600, 900, records);

                // Una captura por si misma nunca debe aprobar una experiencia de juego.
                string json = "{\n" +
                    "  \"engine\":\"Unity\",\n" +
                    "  \"mode\":\"Editor_RenderTexture_no_PlayMode\",\n" +
                    "  \"level_transition_pass\":true,\n" +
                    "  \"domain_snapshot_read_only_pass\":true,\n" +
                    "  \"visibility_policy_pass\":true,\n" +
                    "  \"six_sprite_colliders_pass\":true,\n" +
                    "  \"game_state_modified\":false,\n" +
                    "  \"quality_import_uncompressed_pass\":true,\n" +
                    "  \"retina_terrain_rendered_pass\":true,\n" +
                    "  \"quality_import_no_mipmap_pass\":true,\n" +
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
                if (!UnityEngine.Application.isBatchMode && previousSetup != null && previousSetup.Length > 0)
                    EditorSceneManager.RestoreSceneManagerSetup(previousSetup);
            }
        }

        // Editor import audit: verifies effective texture assets, never inferred
        // from file extensions or a software-upscaled screenshot.
        private static void VerifyUncompressedSpriteImports()
        {
            string folder = City2DSceneBuilder.Folder + "Sprites/";
            var expected = new[] {
                new { File="Terreno_Organico.png", Width=941, Height=1672 },
                new { File="Retina/Terreno_Organico.png", Width=1412, Height=2508 },
                new { File="Bastion_Nivel05.png", Width=764, Height=854 },
                new { File="Bastion_Nivel01.png", Width=762, Height=699 },
                new { File="Cuartel.png", Width=860, Height=860 }
            };
            foreach (var sprite in expected)
            {
                string path = folder + sprite.File;
                var importer = AssetImporter.GetAtPath(path) as TextureImporter;
                var asset = AssetDatabase.LoadAssetAtPath<Sprite>(path);
                if (importer == null || asset == null || asset.texture == null)
                    throw new InvalidOperationException("Texture audit missing sprite: " + path);
                var tex = asset.texture;
                if (importer.textureCompression != TextureImporterCompression.Uncompressed)
                    throw new InvalidOperationException("Texture audit compressed: " + path);
                if (importer.mipmapEnabled)
                    throw new InvalidOperationException("Texture audit mipmaps enabled: " + path);
                if (tex.filterMode != FilterMode.Bilinear)
                    throw new InvalidOperationException("Texture audit filter mismatch: " + path);
                if (tex.width != sprite.Width || tex.height != sprite.Height)
                    throw new InvalidOperationException("Texture audit rescaled source: " + path +
                        " got " + tex.width + "x" + tex.height);
                Debug.Log("[Ciudad2D] NATIVE_TEXTURE_QUALITY_PASS " + sprite.File +
                    " " + tex.width + "x" + tex.height + " " + tex.graphicsFormat);
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
            var bridge = UnityEngine.Object.FindFirstObjectByType<City2DStateBridge>();
            if (bridge == null) throw new InvalidOperationException("Bridge not found during capture");
            bridge.SetDemoBastionLevel(level);
            AssertMilestone(bastion, level >= 5 ? 5 : 1);
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
