#if UNITY_EDITOR
using UnityEditor;
namespace Eldoria.ReferenceClean.Editor {
 // Only management reference portraits; never modifies source model textures.
 public sealed class BastionIVVPortraitImport:AssetPostprocessor {
  void OnPreprocessTexture(){
   if(!assetPath.StartsWith("Assets/Eldoria/ArtTests/ValoriaReferenceClean/Integration/Resources/BastionIVV/Portraits/",System.StringComparison.Ordinal))return;
   var importer=(TextureImporter)assetImporter;importer.textureType=TextureImporterType.Default;importer.textureShape=TextureImporterShape.Texture2D;importer.maxTextureSize=1024;importer.mipmapEnabled=false;importer.isReadable=false;importer.textureCompression=TextureImporterCompression.Compressed;
  }
 }
}
#endif
