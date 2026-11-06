#if UNITY_EDITOR
using System;
using UnityEditor;
using UnityEngine;

internal static class ForjaOscuraSpriteAudit
{
    [MenuItem("Eldoria/Forja Oscura/Validar sprites importados")]
    public static void Validate()
    {
        string root = "Assets/EldoriaVisualKit/Resources/ForjaOscura/";
        var guids = AssetDatabase.FindAssets("", new[] { root });
        int icons = 0, surfaces = 0, invalid = 0;
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            if (!path.EndsWith(".png", StringComparison.OrdinalIgnoreCase)) continue;
            var importer = AssetImporter.GetAtPath(path) as TextureImporter;
            var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
            if (path.Contains("/Icons/")) icons++;
            else if (path.Contains("/Surfaces/")) surfaces++;
            bool sliced = path.Contains("/Surfaces/") && !path.Contains("/medallion_");
            if (importer == null || sprite == null || importer.textureType != TextureImporterType.Sprite ||
                !importer.alphaIsTransparency || (sliced && sprite.border.sqrMagnitude == 0f))
            {
                invalid++;
                Debug.LogError("[FORJA] Invalid import or border: " + path);
            }
        }
        if (icons != 20 || surfaces != 21 || invalid > 0)
            throw new InvalidOperationException("[FORJA] Sprite audit FAIL: icons=" + icons +
                ", surfaces=" + surfaces + ", invalid=" + invalid + ". Expected 20/21/0.");
        Debug.Log("[FORJA] Asset import audit PASS: 20 illustrated icons + 21 sliced surfaces. " +
            "This verifies technical imports, NOT native visual or player acceptance.");
    }
}
#endif
