using UnityEditor;
using UnityEngine;

namespace Eldoria.EditorTools
{
    // Import policy for persisted Golden Surface V2 textures.
    // Runs during AssetDatabase import, never from the capture executeMethod.
    public sealed class GoldenSurfaceV2TextureImporter : AssetPostprocessor
    {
        void OnPreprocessTexture()
        {
            if(!assetPath.StartsWith("Assets/Eldoria/ArtTests/GoldenSurfaceV2/Textures/")) return;
            var ti=(TextureImporter)assetImporter;
            bool normal=assetPath.EndsWith("_normal.png");
            bool linear=normal||assetPath.EndsWith("_ao.png")||assetPath.EndsWith("_smoothness.png");
            ti.textureType=normal?TextureImporterType.NormalMap:TextureImporterType.Default;
            ti.sRGBTexture=!linear;
            ti.mipmapEnabled=true;
            ti.wrapMode=TextureWrapMode.Repeat;
            ti.filterMode=FilterMode.Trilinear;
            ti.anisoLevel=4;
            ti.textureCompression=TextureImporterCompression.Compressed;
        }
    }
}
