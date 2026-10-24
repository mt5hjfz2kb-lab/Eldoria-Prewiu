#if UNITY_EDITOR
using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Eldoria.World4XCanon.Editor
{
    /// <summary>
    /// NON-DESTRUCTIVE P0 art experiment. Turn editable geographical masks into a
    /// second atlas. Never overwrites the production atlas or any save/gameplay.
    /// The masks encode geography only; the owner image is not projected.
    /// </summary>
    public static class RegionOneP0BakeAuthorialAtlas
    {
        const string MaskRoot = "Assets/Eldoria/World4XCanon/P0ArtMasks";
        static Texture2D Readable(string assetPath)
        {
            var importer = AssetImporter.GetAtPath(assetPath) as TextureImporter;
            if (importer == null) throw new InvalidOperationException("Texture importer missing: " + assetPath);
            if (!importer.isReadable || importer.textureCompression != TextureImporterCompression.Uncompressed)
            {
                importer.isReadable = true;
                importer.textureCompression = TextureImporterCompression.Uncompressed;
                importer.SaveAndReimport();
            }
            var texture = AssetDatabase.LoadAssetAtPath<Texture2D>(assetPath);
            if (!texture) throw new InvalidOperationException("Authored mask missing: " + assetPath);
            return texture;
        }
        static Texture2D Mask(string name) {
            string path=MaskRoot + "/" + name + ".png";
            var importer=AssetImporter.GetAtPath(path) as TextureImporter;
            if(importer && importer.sRGBTexture) {
                importer.sRGBTexture=false;
                importer.SaveAndReimport();
            }
            return Readable(path);
        }
        static Color Layer(Color c, Color pigment, float t) => Color.Lerp(c, pigment, Mathf.Clamp01(t));
        [MenuItem("Eldoria/Region 1/P0 Bake Independent Authorial Layout Atlas")]
        public static void Bake()
        {
            var ids = AssetDatabase.FindAssets("RegionOnePaintedTerrain t:Texture2D");
            string sourcePath = null;
            foreach (var id in ids)
            {
                var path = AssetDatabase.GUIDToAssetPath(id);
                if (path.Contains("/Resources/World4X/")) { sourcePath = path; break; }
            }
            if (sourcePath == null)
                throw new InvalidOperationException("Original painted source atlas not found; abort without changes.");
            var source = Readable(sourcePath);
            var land = Mask("land");
            var forest = Mask("forest");
            var water = Mask("water");
            var roads = Mask("roads");
            var relief = Mask("relief");
            var rift = Mask("rift");
            int w=source.width,h=source.height;
            var output = new Texture2D(w,h,TextureFormat.RGBA32,true,false);
            var sourcePixels = source.GetPixels();
            var pixels = new Color[sourcePixels.Length];
            for(int y=0;y<h;y++)for(int x=0;x<w;x++)
            {
                float u=(x+.5f)/w,v=(y+.5f)/h;
                int i=y*w+x;
                // Image features affect ONLY this independent experiment atlas.
                // Coastline and real interactive nodes remain owned by runtime.
                float l=land.GetPixelBilinear(u,v).r;
                float f=forest.GetPixelBilinear(u,v).r*l;
                float river=water.GetPixelBilinear(u,v).r;
                float road=roads.GetPixelBilinear(u,v).r*l;
                float hills=relief.GetPixelBilinear(u,v).r*l;
                float fracture=rift.GetPixelBilinear(u,v).r*l;
                Color c=sourcePixels[i];
                c=Layer(c,new Color(.71f,.65f,.35f),l*.08f);
                c=Layer(c,new Color(.23f,.38f,.21f),f*.58f);
                c=Layer(c,new Color(.58f,.52f,.36f),hills*.27f);
                c=Layer(c,new Color(.17f,.45f,.63f),river*.89f);
                c=Layer(c,new Color(.82f,.69f,.45f),road*.65f*(1f-river));
                c=Layer(c,new Color(.31f,.19f,.40f),fracture*.84f*(1f-river));
                c.a=1f;
                pixels[i]=c;
            }
            output.SetPixels(pixels);output.Apply(true,false);
            string dirname=Path.GetDirectoryName(sourcePath).Replace('\\','/');
            string outPath=dirname+"/RegionOneP0Terrain.png";
            if (File.Exists(outPath)) throw new InvalidOperationException(
                "P0 output already exists: never overwrite evidence: " + outPath);
            File.WriteAllBytes(outPath,output.EncodeToPNG());
            UnityEngine.Object.DestroyImmediate(output);
            AssetDatabase.ImportAsset(outPath,ImportAssetOptions.ForceSynchronousImport);
            Debug.Log("REGION1_P0_AUTHORIAL_ATLAS_GENERATED path="+outPath+
                " source="+sourcePath+" world_x=-910..910 world_z=-780..780 non_destructive=1");
        }
    }
}
#endif
