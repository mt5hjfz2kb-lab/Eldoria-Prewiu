using System;
using Eldoria.Domain;
using UnityEngine;
using Object=UnityEngine.Object;

namespace Eldoria.Presentation
{
    // Global architecture harmonization for visible secondary buildings.
    // Keeps geometry and gameplay authority intact while unifying surface language.
    public static class ValoriaArchitectureCoherenceV1
    {
        public static bool Enabled=false;
        public static int StyledRenderers{get;private set;}

        public static void Build(Transform parent,PlayerState state)
        {
            if(!Enabled)return;
            StyledRenderers=0;

            foreach(var r in Object.FindObjectsByType<Renderer>(FindObjectsSortMode.None))
            {
                if(r==null||!r.enabled||!r.gameObject.activeInHierarchy)continue;
                var chain=Chain(r.transform);
                if(!Target(chain))continue;

                var mats=r.sharedMaterials;
                var copy=new Material[mats.Length];
                for(int i=0;i<mats.Length;i++)
                {
                    var source=mats[i];
                    copy[i]=source==null?null:Coherent(source,chain);
                }
                r.sharedMaterials=copy;
                StyledRenderers++;
            }
        }

        static bool Target(string chain)
        {
            if(chain.Contains("bastion · dedicated")||chain.Contains("valoria · bastion hero"))return false;
            return chain.Contains("cuartel · dedicated barracks")||
                   chain.Contains("aserradero · dedicated sawmill")||
                   chain.Contains("full frame architecture")||
                   chain.Contains("composite v2")||
                   chain.Contains("upper dwelling")||
                   chain.Contains("rescued upper civil residence")||
                   chain.Contains("mid-tier");
        }

        static Material Coherent(Material source,string chain)
        {
            var name=(source.name??"").ToLowerInvariant();
            bool roof=name.Contains("roof")||name.Contains("slate")||name.Contains("tile")||
                      chain.Contains("roof");
            bool wood=name.Contains("wood")||name.Contains("timber")||name.Contains("beam")||
                      name.Contains("plank")||name.Contains("door")||chain.Contains("workshop");
            bool metal=name.Contains("metal")||name.Contains("iron");

            var tint=roof?new Color(.27f,.30f,.31f,1f):
                     wood?new Color(.42f,.28f,.17f,1f):
                     metal?new Color(.26f,.27f,.27f,1f):
                     new Color(.69f,.64f,.55f,1f);

            // Keep all authored maps whenever possible.
            var m=new Material(source){name="Valoria coherent · "+source.name};
            if(m.HasProperty("_BaseColorFactor"))
            {
                var current=m.GetColor("_BaseColorFactor");
                var mixed=MultiplySafe(current,tint,.72f);
                m.SetColor("_BaseColorFactor",mixed);
            }
            if(m.HasProperty("_BaseColor"))
            {
                var current=m.GetColor("_BaseColor");
                var mixed=MultiplySafe(current,tint,.68f);
                // prevent black imported factors from crushing otherwise useful textures
                if(mixed.maxColorComponent<.16f)mixed=tint*.72f;
                mixed.a=1f;
                m.SetColor("_BaseColor",mixed);
            }
            else if(m.HasProperty("_Color"))
            {
                var current=m.GetColor("_Color");
                var mixed=MultiplySafe(current,tint,.68f);
                if(mixed.maxColorComponent<.16f)mixed=tint*.72f;
                mixed.a=1f;
                m.SetColor("_Color",mixed);
            }

            if(m.HasProperty("_Metallic"))m.SetFloat("_Metallic",metal?.18f:0f);
            if(m.HasProperty("_Smoothness"))m.SetFloat("_Smoothness",metal?.16f:.035f);
            if(m.HasProperty("_RoughnessFactor"))m.SetFloat("_RoughnessFactor",metal?.72f:.91f);
            if(m.HasProperty("_OcclusionStrength"))m.SetFloat("_OcclusionStrength",1f);
            if(m.HasProperty("_EmissionColor"))m.SetColor("_EmissionColor",Color.black);
            return m;
        }

        static Color MultiplySafe(Color source,Color tint,float tintWeight)
        {
            if(source.maxColorComponent<.12f)source=Color.white;
            var multiplied=new Color(source.r*tint.r,source.g*tint.g,source.b*tint.b,1f);
            return Color.Lerp(source,multiplied,tintWeight);
        }

        static string Chain(Transform t)
        {
            string s="";
            for(var p=t;p!=null;p=p.parent)s+="|"+p.name.ToLowerInvariant();
            return s;
        }
    }
}
