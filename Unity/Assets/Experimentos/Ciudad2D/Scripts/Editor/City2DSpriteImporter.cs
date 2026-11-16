#if UNITY_EDITOR
using System;
using UnityEditor;
using UnityEngine;

namespace Eldoria.Experimentos.Ciudad2D.Editor
{
    internal sealed class City2DSpriteImporter : AssetPostprocessor
    {
        private const string SpriteFolder = "Assets/Experimentos/Ciudad2D/Sprites/";

        private void OnPreprocessTexture()
        {
            if (!assetPath.StartsWith(SpriteFolder, StringComparison.Ordinal)) return;
            if (!assetPath.EndsWith(".png", StringComparison.OrdinalIgnoreCase)) return;
            var importer = (TextureImporter)assetImporter;
            bool ground = assetPath.EndsWith("/Terreno.png", StringComparison.OrdinalIgnoreCase);
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.spritePixelsPerUnit = 100f;
            var settings = new TextureImporterSettings();
            importer.ReadTextureSettings(settings);
            settings.spriteMeshType = SpriteMeshType.FullRect;
            settings.spriteAlignment = (int)(ground ? SpriteAlignment.Center : SpriteAlignment.BottomCenter);
            settings.spritePivot = ground ? new Vector2(0.5f, 0.5f) : new Vector2(0.5f, 0f);
            importer.SetTextureSettings(settings);
            importer.mipmapEnabled = false;
            importer.alphaIsTransparency = !ground;
            importer.textureCompression = TextureImporterCompression.CompressedHQ;
            importer.compressionQuality = 100;
            importer.maxTextureSize = 2048;
            importer.npotScale = TextureImporterNPOTScale.None;
            importer.filterMode = FilterMode.Bilinear;
        }

        private static void OnPostprocessAllAssets(string[] importedAssets,
            string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
        {
            foreach (string p in importedAssets)
            {
                if (p.StartsWith(SpriteFolder, StringComparison.Ordinal))
                {
                    EditorApplication.delayCall += City2DSceneBuilder.BuildIfReady;
                    return;
                }
            }
        }
    }
}
#endif
