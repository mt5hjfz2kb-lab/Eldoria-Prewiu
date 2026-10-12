using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using GLTFast;
using UnityEditor;
using UnityEngine;

namespace Eldoria.EditorTools
{
    // Explicit glTF -> URP bindings after genuine engine captures exposed
    // glTF shader texture-state leakage. Preserve factors and repack channels.
    static class Bastion1MaterialBindings
    {
        [Serializable] class Root {public SourceMaterial[] materials;}
        [Serializable] class TextureInfo {public int index;public float scale=1f;}
        [Serializable] class Pbr
        {
            public float[] baseColorFactor;
            public TextureInfo baseColorTexture,metallicRoughnessTexture;
            public float metallicFactor=1f,roughnessFactor=1f;
        }
        [Serializable] class SourceMaterial
        {
            public string name,alphaMode;
            public float alphaCutoff=.5f;
            public bool doubleSided;
            public Pbr pbrMetallicRoughness;
            public TextureInfo normalTexture;
            public float[] emissiveFactor;
        }
        public static void Apply(GameObject root,GltfImport importer,string glbPath)
        {
            string normalized=Path.Combine(Path.GetDirectoryName(glbPath),Path.GetFileNameWithoutExtension(glbPath)+"-material-bindings.json");
            if(!File.Exists(normalized))throw new Exception("Explicit glTF material defaults missing");
            var source=JsonUtility.FromJson<Root>(File.ReadAllText(normalized));
            if(source.materials==null)throw new Exception("glTF source material table missing");
            var replacements=new Dictionary<Material,Material>();
            string folder="Assets/Eldoria/ArtTests/Bastion1Assembly/GeneratedNormals";
            Directory.CreateDirectory(folder);
            for(int i=0;i<source.materials.Length;i++)
            {
                var src=source.materials[i];var old=importer.GetMaterial(i);
                if(old==null)throw new Exception("glTF material unavailable: "+i);
                var p=src.pbrMetallicRoughness;
                var m=new Material(Shader.Find("Universal Render Pipeline/Lit")){name="BastionI_URP_"+src.name};
                m.SetTexture("_BaseMap",null); // URP Lit uses its default white for factor-only materials.
                m.SetTexture("_BumpMap",null);m.SetTexture("_MetallicGlossMap",null);
                m.DisableKeyword("_NORMALMAP");m.DisableKeyword("_METALLICSPECGLOSSMAP");
                Color color=Color.white;
                if(p!=null&&p.baseColorFactor!=null&&p.baseColorFactor.Length==4)
                    color=new Color(p.baseColorFactor[0],p.baseColorFactor[1],p.baseColorFactor[2],p.baseColorFactor[3]);
                m.SetColor("_BaseColor",color);
                if(p!=null&&p.baseColorTexture!=null&&p.baseColorTexture.index>=0)
                {
                    var texture=importer.GetTexture(p.baseColorTexture.index);
                    if(texture==null)throw new Exception("glTF albedo texture unavailable: "+src.name);
                    m.SetTexture("_BaseMap",texture);
                }
                m.SetFloat("_Metallic",p!=null?p.metallicFactor:1f);
                m.SetFloat("_Smoothness",p!=null?1f-p.roughnessFactor:0f);
                if(p!=null&&p.metallicRoughnessTexture!=null&&p.metallicRoughnessTexture.index>=0)
                {
                    var pixels=ReadLinear(importer.GetTexture(p.metallicRoughnessTexture.index));
                    var values=pixels.GetPixels();
                    for(int n=0;n<values.Length;n++)
                        values[n]=new Color(values[n].b*p.metallicFactor,0,0,1f-values[n].g*p.roughnessFactor);
                    pixels.SetPixels(values);pixels.Apply(true,false);pixels.name="Repacked glTF B metallic G roughness: "+src.name;
                    m.SetTexture("_MetallicGlossMap",pixels);m.SetFloat("_Smoothness",1f);m.EnableKeyword("_METALLICSPECGLOSSMAP");
                }
                if(src.normalTexture!=null&&src.normalTexture.index>=0)
                {
                    var pixels=ReadLinear(importer.GetTexture(src.normalTexture.index));
                    // Use Unity's normal importer instead of treating glTF RGB as DXT5nm.
                    string path=folder+"/"+Path.GetFileNameWithoutExtension(glbPath)+"-normal-"+src.normalTexture.index+".png";
                    File.WriteAllBytes(path,pixels.EncodeToPNG());UnityEngine.Object.DestroyImmediate(pixels);
                    AssetDatabase.ImportAsset(path,ImportAssetOptions.ForceSynchronousImport);
                    var settings=(TextureImporter)AssetImporter.GetAtPath(path);
                    settings.textureType=TextureImporterType.NormalMap;settings.sRGBTexture=false;
                    settings.mipmapEnabled=true;settings.SaveAndReimport();
                    m.SetTexture("_BumpMap",AssetDatabase.LoadAssetAtPath<Texture2D>(path));
                    m.SetFloat("_BumpScale",src.normalTexture.scale);m.EnableKeyword("_NORMALMAP");
                }
                bool leaves=src.name.IndexOf("Leaf",StringComparison.OrdinalIgnoreCase)>=0;
                if(src.alphaMode=="MASK"||leaves)
                {
                    m.SetFloat("_AlphaClip",1f);m.SetFloat("_Cutoff",leaves?.35f:src.alphaCutoff);
                    m.EnableKeyword("_ALPHATEST_ON");m.renderQueue=2450;m.SetOverrideTag("RenderType","TransparentCutout");
                }
                m.SetFloat("_Cull",src.doubleSided?0f:2f);
                if(src.emissiveFactor!=null&&src.emissiveFactor.Length==3)
                {
                    m.SetColor("_EmissionColor",new Color(src.emissiveFactor[0],src.emissiveFactor[1],src.emissiveFactor[2]));
                    m.EnableKeyword("_EMISSION");
                }
                replacements.Add(old,m);
                Debug.Log("BASTION1_MATERIAL_BINDING "+i+" "+src.name+" originalShader="+old.shader.name+
                    " albedo="+(m.GetTexture("_BaseMap")!=null?m.GetTexture("_BaseMap").name:"factor only")+" color="+color+
                    " metallic="+(p!=null?p.metallicFactor:1f)+" roughness="+(p!=null?p.roughnessFactor:1f));
            }
            foreach(var renderer in root.GetComponentsInChildren<Renderer>(true))
            {
                var materials=renderer.sharedMaterials;
                for(int i=0;i<materials.Length;i++)
                {if(!replacements.TryGetValue(materials[i],out var replacement))throw new Exception("Unmapped candidate material");materials[i]=replacement;}
                renderer.sharedMaterials=materials;
            }
        }
        static Texture2D ReadLinear(Texture2D source)
        {
            if(source==null)throw new Exception("PBR input texture missing");
            var prior=RenderTexture.active;
            var rt=RenderTexture.GetTemporary(source.width,source.height,0,RenderTextureFormat.ARGB32,RenderTextureReadWrite.Linear);
            try
            {
                Graphics.Blit(source,rt);RenderTexture.active=rt;
                var result=new Texture2D(source.width,source.height,TextureFormat.RGBA32,true,true);
                result.ReadPixels(new Rect(0,0,source.width,source.height),0,0);result.Apply(true,false);return result;
            }
            finally{RenderTexture.active=prior;RenderTexture.ReleaseTemporary(rt);}
        }
    }
}
