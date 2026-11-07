#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

// Asset-only importer. Does not touch scenes, gameplay, saves, Canvas or existing prefabs.
// Import via Assets/Eldoria/VisualKit/Sprites/* without overriding non-kit textures.
internal sealed class EldoriaVisualKitTextureImporter : AssetPostprocessor
{
    private const string Root = "Assets/Eldoria/VisualKit/Sprites/";
    private const string RuntimeRoot = "Assets/Eldoria/VisualKit/Resources/ForjaOscura/";

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
        // Unity spriteBorder coordinates: left, bottom, right, top.
        // Do not squeeze small navigation buttons until the 9-slice center
        // collapses. Keep mobile min heights from QA table below.
        Vector4 border = Vector4.zero;
        if (name.StartsWith("button_primary_", System.StringComparison.Ordinal) ||
            name.StartsWith("button_secondary_", System.StringComparison.Ordinal))
            border = new Vector4(56, 15, 56, 15);
        else if (name.StartsWith("nav_tile_", System.StringComparison.Ordinal))
            border = new Vector4(48, 22, 48, 22);
        else if (name == "panel_dark" || name == "panel_light")
            border = new Vector4(56, 20, 56, 20);
        else if (name == "panel_quest") border = new Vector4(56, 22, 56, 22);
        else if (name == "resource_chip") border = new Vector4(36, 14, 36, 14);
        else if (name.StartsWith("tab_", System.StringComparison.Ordinal))
            border = new Vector4(36, 10, 36, 10);
        else if (name == "badge_level") border = new Vector4(32, 8, 32, 8);
        else if (name.StartsWith("progress_", System.StringComparison.Ordinal))
            border = new Vector4(20, 3, 20, 3);
        else if (name == "toast_background") border = new Vector4(40, 12, 40, 12);
        // .meta spriteMeshType: 0 means FullRect in Unity 6000.3.
        // This importer never changes GUID, mesh type or gameplay state.
        ti.spriteBorder = border;
    }
}
#endif
