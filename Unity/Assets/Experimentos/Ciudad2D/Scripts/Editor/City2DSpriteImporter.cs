#if UNITY_EDITOR
using System;
using UnityEditor;
using UnityEngine;

namespace Eldoria.Experimentos.Ciudad2D.Editor
{
    /// <summary>Importador limitado exclusivamente a los sprites del sandbox.</summary>
    internal sealed class City2DSpriteImporter : AssetPostprocessor
    {
        private const string SpriteFolder = "Assets/Experimentos/Ciudad2D/Sprites/";

        private void OnPreprocessTexture()
        {
            if (!assetPath.StartsWith(SpriteFolder, StringComparison.Ordinal) ||
                !assetPath.EndsWith(".png", StringComparison.OrdinalIgnoreCase)) return;

            var importer = (TextureImporter)assetImporter;
            bool terrain = assetPath.EndsWith("/Terreno.png", StringComparison.OrdinalIgnoreCase);
            bool halo = assetPath.EndsWith("/Props/HaloAntorcha.png", StringComparison.OrdinalIgnoreCase);
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.spritePixelsPerUnit = 100f;
            importer.mipmapEnabled = false;
            importer.alphaIsTransparency = !terrain;
            importer.textureCompression = TextureImporterCompression.CompressedHQ;
            importer.compressionQuality = 100;
            importer.maxTextureSize = 2048;
            importer.npotScale = TextureImporterNPOTScale.None;
            importer.filterMode = FilterMode.Bilinear;

            var settings = new TextureImporterSettings();
            importer.ReadTextureSettings(settings);
            settings.spriteMeshType = SpriteMeshType.FullRect;
            settings.spriteAlignment = (int)((terrain || halo) ? SpriteAlignment.Center : SpriteAlignment.BottomCenter);
            settings.spritePivot = terrain || halo ? new Vector2(0.5f, 0.5f) : new Vector2(0.5f, 0f);
            importer.SetTextureSettings(settings);

            // La optimización final por plataforma depende de capturas/profiler físicos.
            // No se impone Android ASTC ni iOS ASTC sin comprobar compresión visual.
        }

        private static void OnPostprocessAllAssets(string[] imported, string[] deleted,
            string[] moved, string[] movedFrom)
        {
            foreach (string path in imported)
            {
                if (!path.StartsWith(SpriteFolder, StringComparison.Ordinal) &&
                    path != "Assets/Experimentos/Ciudad2D/QA/city_layout.json") continue;
                EditorApplication.delayCall += City2DSceneBuilder.Rebuild;
                break;
            }
        }
    }
}
#endif
