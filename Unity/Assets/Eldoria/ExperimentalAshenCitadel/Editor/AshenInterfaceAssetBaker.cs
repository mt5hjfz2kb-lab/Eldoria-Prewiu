// Persistent authored PNG Sprite imports for the Ashen Citadel UI.
// Previous Unity headless Texture2D.SetPixels32 produced white panels even
// with a mathematically dark palette. The canonical assets now come from
// deterministic Node/png source committed alongside its SHA256 manifest.
// This Editor step ONLY configures native TextureImporter and validates
// persistent resources; it never overwrites them with transient textures.
using System;
using System.IO;
using Eldoria.ExperimentalAshenCitadel;
using UnityEditor;
using UnityEngine;

namespace Eldoria.ExperimentalAshenCitadel.Editor
{
    public static class AshenInterfaceAssetBaker
    {
        private const string ResourcesRoot=
            "Assets/Eldoria/ExperimentalAshenCitadel/Resources/";
        public static void Bake()
        {
            VerifyImportedSprite("AshenPremiumPanel",new Vector4(18,18,18,18));
            VerifyImportedSprite("AshenPremiumButtonPrimary",new Vector4(19,14,19,14));
            VerifyImportedSprite("AshenPremiumButtonSecondary",new Vector4(19,14,19,14));
            VerifyImportedSprite("AshenPremiumCrest",Vector4.zero);
            VerifyImportedSprite("AshenPremiumMedallion",Vector4.zero);
            VerifyImportedSprite("AshenPremiumStatusChip",new Vector4(16,14,16,14));
            foreach(string key in new[]{"castle","workshop","army","compass",
                "mail","bag","hero","quest"})
                VerifyImportedSprite("AshenPremiumGlyph_"+key,Vector4.zero);
            AssetDatabase.SaveAssets();
            AshenInterfaceIdentity.RebindAfterBake();
            AshenInterfaceGlyphs.RebindAfterBake();
            Debug.Log("ASHEN_UI_PREMIUM_SPRITES_IMPORTED 14 canonical PNGs; 9-slice borders verified");
        }
        private static void VerifyImportedSprite(string name,Vector4 border)
        {
            string path=ResourcesRoot+name+".png";
            if(!File.Exists(path))
                throw new InvalidOperationException("Missing canonical Ashen premium source PNG: "+
                    path+" (run tools/ashen-citadel/generate_ui_atlas.mjs)");
            byte[] data=File.ReadAllBytes(path);
            if(data.Length<128||data[0]!=137||data[1]!=80||data[2]!=78||data[3]!=71)
                throw new InvalidOperationException("Corrupt Ashen premium PNG: "+path);
            AssetDatabase.ImportAsset(path,ImportAssetOptions.ForceSynchronousImport);
            var importer=AssetImporter.GetAtPath(path) as TextureImporter;
            if(importer==null)
                throw new InvalidOperationException("Ashen PNG importer unavailable: "+path);
            bool changed=importer.textureType!=TextureImporterType.Sprite ||
                importer.spriteImportMode!=SpriteImportMode.Single ||
                importer.spriteBorder!=border || importer.mipmapEnabled ||
                !importer.sRGBTexture || !importer.alphaIsTransparency;
            importer.textureType=TextureImporterType.Sprite;
            importer.spriteImportMode=SpriteImportMode.Single;
            importer.spritePixelsPerUnit=100f;
            importer.spriteBorder=border;
            importer.mipmapEnabled=false;
            importer.filterMode=FilterMode.Bilinear;
            importer.sRGBTexture=true;
            importer.alphaIsTransparency=true;
            importer.textureCompression=TextureImporterCompression.Uncompressed;
            if(changed)importer.SaveAndReimport();
            var sprite=AssetDatabase.LoadAssetAtPath<Sprite>(path);
            if(sprite==null||sprite.texture==null)
                throw new InvalidOperationException("Ashen imported Sprite is missing: "+path);
            if(Mathf.Abs(sprite.border.x-border.x)>.5f)
                throw new InvalidOperationException("Ashen Sprite border mismatched: "+path);
        }
    }
}
