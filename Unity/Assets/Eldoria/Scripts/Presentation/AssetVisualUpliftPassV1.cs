using System;
using System.Collections.Generic;
using Eldoria.Domain;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Eldoria.Presentation
{
    // ASSET VISUAL UPLIFT PASS v1
    // Surface-only uplift of existing canonical geometry. No colliders, hotspots, topology or paid generation.
    public static class AssetVisualUpliftPassV1
    {
        public static bool Enabled = true;

        static readonly Dictionary<string,Material> ImportedMaterialCache=new Dictionary<string,Material>();

        public static void ApplyExistingScene(PlayerState state)
        {
            if(!Enabled||state==null||state.BastionLevel<3)return;
            RefineHeroBastion();
            RefineDedicatedProductionBuildings();
            RefineRescuedSupport();
        }

        public static void ApplyImportedFamily(GameObject go,string family)
        {
            if(!Enabled||go==null)return;
            foreach(var renderer in go.GetComponentsInChildren<Renderer>(true))
            {
                if(renderer==null)continue;
                var source=renderer.sharedMaterials;
                var result=new Material[source.Length];
                for(int i=0;i<source.Length;i++)
                    result[i]=BuildPreservedMaterial(source[i],family,i);
                renderer.sharedMaterials=result;
                renderer.SetPropertyBlock(null);
            }
        }

        public static void ApplyMidTier(GameObject go,Color fallbackTint)
        {
            if(!Enabled||go==null)return;
            foreach(var renderer in go.GetComponentsInChildren<Renderer>(true))
            {
                if(renderer==null)continue;
                var mats=renderer.sharedMaterials;
                for(int i=0;i<mats.Length;i++)
                {
                    var mat=mats[i];
                    if(mat==null)continue;
                    var role=Classify(mat.name,i);
                    var block=new MaterialPropertyBlock();
                    renderer.GetPropertyBlock(block,i);
                    var tint=MidTierTint(role,fallbackTint);
                    SetColor(block,mat,tint);
                    if(mat.HasProperty("_Smoothness"))
                        block.SetFloat("_Smoothness",role=="roof"?.025f:role=="timber"?.045f:.055f);
                    if(mat.HasProperty("_OcclusionStrength"))block.SetFloat("_OcclusionStrength",1f);
                    renderer.SetPropertyBlock(block,i);
                }
            }
        }

        static void RefineHeroBastion()
        {
            foreach(var renderer in Object.FindObjectsByType<Renderer>(FindObjectsSortMode.None))
            {
                if(renderer==null||!renderer.enabled||!renderer.gameObject.activeInHierarchy)continue;
                var chain=Hierarchy(renderer.transform).ToLowerInvariant();
                if(!chain.Contains("certified hero bastion"))continue;
                ApplyRendererRole(renderer,"hero");
            }
        }

        static void RefineDedicatedProductionBuildings()
        {
            foreach(var renderer in Object.FindObjectsByType<Renderer>(FindObjectsSortMode.None))
            {
                if(renderer==null||!renderer.enabled||!renderer.gameObject.activeInHierarchy)continue;
                var chain=Hierarchy(renderer.transform).ToLowerInvariant();
                string family=null;
                if(chain.Contains("aserradero"))family="production";
                else if(chain.Contains("cuartel"))family="military";
                else if(chain.Contains("granero"))family="granary";
                if(family!=null)ApplyRendererRole(renderer,family);
            }
        }

        static void RefineRescuedSupport()
        {
            foreach(var renderer in Object.FindObjectsByType<Renderer>(FindObjectsSortMode.None))
            {
                if(renderer==null||!renderer.enabled||!renderer.gameObject.activeInHierarchy)continue;
                var chain=Hierarchy(renderer.transform).ToLowerInvariant();
                if(!chain.Contains("rescued")&&!chain.Contains("assetlibrary reprocessing"))continue;
                if(chain.Contains("certified hero bastion"))continue;
                ApplyRendererRole(renderer,"support");
            }
        }

        static void ApplyRendererRole(Renderer renderer,string family)
        {
            var mats=renderer.sharedMaterials;
            for(int i=0;i<mats.Length;i++)
            {
                var mat=mats[i];
                if(mat==null)continue;
                var role=Classify(mat.name,i);
                var block=new MaterialPropertyBlock();
                renderer.GetPropertyBlock(block,i);
                SetColor(block,mat,RoleTint(family,role));
                if(mat.HasProperty("_Smoothness"))block.SetFloat("_Smoothness",RoleSmoothness(role));
                if(mat.HasProperty("_Metallic"))block.SetFloat("_Metallic",role=="metal"?.12f:0f);
                if(mat.HasProperty("_OcclusionStrength"))block.SetFloat("_OcclusionStrength",1f);
                if(mat.HasProperty("_BumpScale")&&(role=="stone"||role=="rock"))block.SetFloat("_BumpScale",1.08f);
                renderer.SetPropertyBlock(block,i);
            }
        }

        static Material BuildPreservedMaterial(Material source,string family,int slot)
        {
            if(source==null)return null;
            var role=Classify(source.name,slot);
            string key=family+"|"+role+"|"+source.GetInstanceID();
            if(ImportedMaterialCache.TryGetValue(key,out var cached)&&cached!=null)return cached;

            var shader=Shader.Find("Universal Render Pipeline/Lit")??Shader.Find("Standard");
            var material=new Material(shader){name="Valoria Uplift · "+family+" · "+role+" · "+source.name};
            CopyTexture(source,material,new[]{"_BaseMap","_MainTex","_BaseColorTexture","baseColorTexture","_Texture"},"_BaseMap");
            CopyTexture(source,material,new[]{"_BumpMap","_NormalMap","normalTexture"},"_BumpMap");
            CopyTexture(source,material,new[]{"_OcclusionMap"},"_OcclusionMap");
            CopyTexture(source,material,new[]{"_MetallicGlossMap","_MaskMap"},"_MetallicGlossMap");

            if(material.GetTexture("_BumpMap")!=null)
            {
                material.EnableKeyword("_NORMALMAP");
                if(material.HasProperty("_BumpScale"))material.SetFloat("_BumpScale",family=="terrain"?1.16f:1.08f);
            }
            var tint=family=="terrain"
                ? (role=="rock"?new Color(.33f,.34f,.32f,1f):new Color(.37f,.35f,.30f,1f))
                : RoleTint("support",role);
            if(material.HasProperty("_BaseColor"))material.SetColor("_BaseColor",tint);
            if(material.HasProperty("_Color"))material.SetColor("_Color",tint);
            if(material.HasProperty("_Smoothness"))material.SetFloat("_Smoothness",RoleSmoothness(role));
            if(material.HasProperty("_Metallic"))material.SetFloat("_Metallic",role=="metal"?.10f:0f);
            if(material.HasProperty("_OcclusionStrength"))material.SetFloat("_OcclusionStrength",1f);
            ImportedMaterialCache[key]=material;
            return material;
        }

        static void CopyTexture(Material source,Material target,string[] sourceProperties,string targetProperty)
        {
            if(!target.HasProperty(targetProperty))return;
            foreach(var p in sourceProperties)
            {
                if(!source.HasProperty(p))continue;
                var tex=source.GetTexture(p);
                if(tex==null)continue;
                target.SetTexture(targetProperty,tex);
                if(source.HasProperty(p))
                {
                    try
                    {
                        target.SetTextureScale(targetProperty,source.GetTextureScale(p));
                        target.SetTextureOffset(targetProperty,source.GetTextureOffset(p));
                    }
                    catch{}
                }
                return;
            }
        }

        static string Classify(string name,int slot)
        {
            var n=(name??"").ToLowerInvariant();
            if(n.Contains("roof")||n.Contains("slate")||n.Contains("tile")||n.Contains("shingle"))return "roof";
            if(n.Contains("wood")||n.Contains("timber")||n.Contains("beam")||n.Contains("plank"))return "timber";
            if(n.Contains("rock")||n.Contains("cliff")||n.Contains("terrain"))return "rock";
            if(n.Contains("metal")||n.Contains("iron")||n.Contains("steel")||n.Contains("gold"))return "metal";
            if(n.Contains("banner")||n.Contains("cloth")||n.Contains("fabric"))return "accent";
            if(n.Contains("stone")||n.Contains("wall")||n.Contains("brick")||n.Contains("masonry"))return "stone";
            if(slot==1)return "rock";
            if(slot==2)return "roof";
            if(slot==3)return "timber";
            return "stone";
        }

        static Color RoleTint(string family,string role)
        {
            if(role=="roof")return new Color(.24f,.25f,.26f,1f);
            if(role=="timber")return family=="production"?new Color(.43f,.30f,.20f,1f):new Color(.37f,.27f,.20f,1f);
            if(role=="rock")return new Color(.34f,.35f,.34f,1f);
            if(role=="metal")return new Color(.34f,.33f,.30f,1f);
            if(role=="accent")return family=="hero"?new Color(.34f,.46f,.61f,1f):new Color(.40f,.34f,.27f,1f);
            if(family=="military")return new Color(.55f,.54f,.50f,1f);
            if(family=="granary")return new Color(.58f,.54f,.46f,1f);
            if(family=="hero")return new Color(.63f,.61f,.56f,1f);
            if(family=="production")return new Color(.57f,.52f,.44f,1f);
            return new Color(.50f,.48f,.43f,1f);
        }

        static Color MidTierTint(string role,Color fallback)
        {
            if(role=="roof")return new Color(.24f,.25f,.26f,1f);
            if(role=="timber")return new Color(.40f,.29f,.21f,1f);
            if(role=="rock")return new Color(.35f,.36f,.34f,1f);
            if(role=="metal")return new Color(.34f,.33f,.30f,1f);
            if(role=="accent")return new Color(.31f,.40f,.50f,1f);
            return Color.Lerp(fallback,new Color(.58f,.56f,.51f,1f),.45f);
        }

        static float RoleSmoothness(string role)
        {
            if(role=="metal")return .16f;
            if(role=="timber")return .045f;
            if(role=="roof")return .025f;
            if(role=="rock")return .018f;
            return .05f;
        }

        static void SetColor(MaterialPropertyBlock block,Material material,Color color)
        {
            if(material.HasProperty("_BaseColor"))block.SetColor("_BaseColor",color);
            else if(material.HasProperty("_Color"))block.SetColor("_Color",color);
        }

        static string Hierarchy(Transform t)
        {
            string s="";
            for(var p=t;p!=null;p=p.parent)s=p.name+"/"+s;
            return s;
        }
    }
}
