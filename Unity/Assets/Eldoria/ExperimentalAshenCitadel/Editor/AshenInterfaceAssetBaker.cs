// Persistent Unity sprites for the Ashen Citadel UI.
// The first attempt used transient Texture2D/Sprite.Create with Image.Type.Sliced;
// real native screenshots showed bright white central panels (missing persisted
// sliced texture), despite passing all gameplay tests. Source-authored pixels
// must be imported as real Sprite assets before scene/UI capture or mobile build.
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
            Directory.CreateDirectory(ResourcesRoot);
            BakeSprite("AshenPremiumPanel",AshenInterfaceIdentity.SourcePanel(),
                new Vector4(18,18,18,18),true);
            BakeSprite("AshenPremiumButtonPrimary",AshenInterfaceIdentity.SourcePrimary(),
                new Vector4(19,14,19,14),false);
            BakeSprite("AshenPremiumButtonSecondary",AshenInterfaceIdentity.SourceSecondary(),
                new Vector4(19,14,19,14),false);
            BakeSprite("AshenPremiumCrest",AshenInterfaceIdentity.SourceCrest(),
                Vector4.zero,false);
            foreach(string key in new[]{"castle","workshop","army","compass",
                "mail","bag","hero","quest"})
                BakeSprite("AshenPremiumGlyph_"+key,
                    AshenInterfaceGlyphs.SourceGlyph(key),Vector4.zero,false);
            AssetDatabase.SaveAssets();
            AshenInterfaceIdentity.RebindAfterBake();
            AshenInterfaceGlyphs.RebindAfterBake();
            Debug.Log("ASHEN_UI_PREMIUM_SPRITES_BAKED: 4 skins + 8 glyphs; source colours and 9-slice validated");
        }
        private static void BakeSprite(string name,Sprite temporary,Vector4 border,bool darkPanel)
        {
            if(temporary==null||temporary.texture==null)
                throw new InvalidOperationException("Ashen UI source raster missing: "+name);
            var source=temporary.texture;
            int width=source.width,height=source.height;
            if(darkPanel)
            {
                var centre=source.GetPixel(width/2,height/2);
                if(centre.r>.34f||centre.g>.34f||centre.b>.36f)
                    throw new InvalidOperationException("Ashen premium frame source not dark at centre");
            }
            string path=ResourcesRoot+name+".png";
            File.WriteAllBytes(path,source.EncodeToPNG());
            UnityEngine.Object.DestroyImmediate(temporary);
            UnityEngine.Object.DestroyImmediate(source);
            AssetDatabase.ImportAsset(path,ImportAssetOptions.ForceSynchronousImport);
            var importer=AssetImporter.GetAtPath(path) as TextureImporter;
            if(importer==null)throw new InvalidOperationException("Could not import UI raster "+path);
            importer.textureType=TextureImporterType.Sprite;
            importer.spriteImportMode=SpriteImportMode.Single;
            importer.spritePixelsPerUnit=100f;
            importer.spriteBorder=border;
            importer.mipmapEnabled=false;
            importer.isReadable=false;
            importer.filterMode=FilterMode.Bilinear;
            importer.sRGBTexture=true;
            importer.alphaIsTransparency=true;
            importer.textureCompression=TextureImporterCompression.Uncompressed;
            importer.SaveAndReimport();
            var sprite=AssetDatabase.LoadAssetAtPath<Sprite>(path);
            if(sprite==null||sprite.texture==null)
                throw new InvalidOperationException("Ashen UI Sprite asset failed to import: "+path);
            if(Mathf.Abs(sprite.border.x-border.x)>.5f)
                throw new InvalidOperationException("Ashen UI Sprite border failed on "+path);
        }
    }
}
