#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

// Asset-only importer. Does not touch scenes, gameplay, saves, Canvas or existing prefabs.
// Import via Assets/EldoriaVisualKit/Sprites/* without overriding non-kit textures.
internal sealed class EldoriaVisualKitTextureImporter : AssetPostprocessor
{
    private const string Root = "Assets/EldoriaVisualKit/Sprites/";
    private const string RuntimeRoot = "Assets/EldoriaVisualKit/Resources/ForjaOscura/";

    private void OnPreprocessTexture()
    {
        if (!assetPath.StartsWith(Root, System.StringComparison.Ordinal) &&
            !assetPath.StartsWith(RuntimeRoot, System.StringComparison.Ordinal)) return;
        var ti = assetImporter as TextureImporter;
        if (ti == null) return;
        ti.textureType = TextureImporterType.Sprite;
        ti.spriteImportMode = SpriteImportMode.Single;
        ti.alphaSource = TextureImporterAlphaSource.FromInput;
        ti.alphaIsTransparency = true;
        ti.mipmapEnabled = false;
        ti.filterMode = FilterMode.Bilinear;
        ti.wrapMode = TextureWrapMode.Clamp;
        ti.spritePixelsPerUnit = 100;
        ti.isReadable = false;
        ti.npotScale = TextureImporterNPOTScale.None;
        ti.textureCompression = TextureImporterCompression.Uncompressed;
        // Individual borders exactly match Source/manifest and are retained
        // regardless of landscape, portrait or content size.
        var name = System.IO.Path.GetFileNameWithoutExtension(assetPath);
        int border = 0;
        if (name.StartsWith("button_primary_", System.StringComparison.Ordinal) ||
            name.StartsWith("button_secondary_", System.StringComparison.Ordinal) ||
            name == "panel_dark" || name == "panel_light" || name == "panel_quest")
            border = 56;
        else if (name.StartsWith("nav_tile_", System.StringComparison.Ordinal)) border = 48;
        else if (name == "resource_chip" || name.StartsWith("tab_", System.StringComparison.Ordinal)) border = 36;
        else if (name == "badge_level") border = 32;
        else if (name.StartsWith("progress_", System.StringComparison.Ordinal)) border = 20;
        else if (name == "toast_background") border = 40;
        ti.spriteBorder = new Vector4(border,border,border,border);
    }
}
#endif
