using System;
using Eldoria.Domain;
using UnityEngine;
using Object=UnityEngine.Object;

namespace Eldoria.Presentation
{
    // Whole-frame value hierarchy correction driven by visible-renderer audit.
    // Presentation-only: preserves gameplay topology, colliders, hotspots and routes.
    public static class ValoriaFullFrameValueHierarchyV1
    {
        public static bool Enabled=false;
        public static int RockFamilyNormalized{get;private set;}
        public static int EarthFamilyNormalized{get;private set;}
        public static int CobbleFamilyNormalized{get;private set;}
        public static int DarkArchitectureLifted{get;private set;}

        public static void Build(Transform parent,PlayerState state)
        {
            if(!Enabled||parent==null)return;

            RockFamilyNormalized=0;
            EarthFamilyNormalized=0;
            CobbleFamilyNormalized=0;
            DarkArchitectureLifted=0;

            var rock=ValoriaKit.ExternalPbrSurfaceMaterial(
                "rock",new Color(.34f,.335f,.31f,1f),new Vector2(3.4f,3.4f),.020f,1.08f)
                ?? ValoriaKit.DetailedSurfaceMaterial(new Color(.33f,.325f,.30f,1f),"stone",new Vector2(3.4f,3.4f),1.06f);

            var earth=ValoriaKit.ExternalPbrSurfaceMaterial(
                "dirt",new Color(.31f,.275f,.225f,1f),new Vector2(4.1f,4.1f),.018f,.98f)
                ?? ValoriaKit.DetailedSurfaceMaterial(new Color(.30f,.27f,.22f,1f),"earth",new Vector2(4.1f,4.1f),.98f);

            var cobble=ValoriaKit.ExternalPbrSurfaceMaterial(
                "cobble",new Color(.40f,.385f,.35f,1f),new Vector2(3.1f,3.1f),.024f,1.04f)
                ?? ValoriaKit.DetailedSurfaceMaterial(new Color(.39f,.375f,.34f,1f),"stone",new Vector2(3.1f,3.1f),1.02f);

            foreach(var r in Object.FindObjectsByType<Renderer>(FindObjectsSortMode.None))
            {
                if(r==null||!r.enabled||!r.gameObject.activeInHierarchy)continue;
                string chain=Chain(r.transform);

                if(chain.Contains("backplate")||
                   chain.Contains("bastion hero")||
                   chain.Contains("bastion · dedicated")||
                   chain.Contains("aserradero")||
                   chain.Contains("cuartel")||
                   chain.Contains("banner")||
                   chain.Contains("flag"))
                    continue;

                bool street=
                    chain.Contains("groundkit main street")||
                    chain.Contains("groundkit entry widening")||
                    chain.Contains("groundkit l1 landing")||
                    chain.Contains("processional")||
                    chain.Contains("street slab")||
                    chain.Contains("worn centre");

                bool earthFamily=
                    chain.Contains("upper terrace earth")||
                    chain.Contains("groundkit west workshop terrace")||
                    chain.Contains("groundkit east military terrace")||
                    chain.Contains("groundkit l1 west terrace")||
                    chain.Contains("groundkit l1 east terrace")||
                    chain.Contains("groundkit west plot seam")||
                    chain.Contains("groundkit east plot seam");

                bool rockFamily=
                    chain.Contains("cliff island · lower")||
                    chain.Contains("cliff island · middle")||
                    chain.Contains("cliff island · upper")||
                    chain.Contains("assetlibrary reprocessing · hero lower terrace")||
                    chain.Contains("assetlibrary reprocessing · hero upper terrace")||
                    chain.Contains("terrainterrace")||
                    chain.Contains("compactfootprint · central middle shelf")||
                    chain.Contains("compactfootprint · bastion lower shelf")||
                    chain.Contains("broadrockplatform")||
                    chain.Contains("steppedrockterrace");

                if(street)
                {
                    SetAll(r,cobble);
                    CobbleFamilyNormalized++;
                    continue;
                }

                if(earthFamily)
                {
                    SetAll(r,earth);
                    EarthFamilyNormalized++;
                    continue;
                }

                if(rockFamily)
                {
                    SetAll(r,rock);
                    RockFamilyNormalized++;
                    continue;
                }

                if(chain.Contains("rescued upper civil residence"))
                {
                    LiftDarkArchitecture(r);
                    DarkArchitectureLifted++;
                }
            }
        }

        static void LiftDarkArchitecture(Renderer r)
        {
            var src=r.sharedMaterials;
            var dst=new Material[src.Length];
            for(int i=0;i<src.Length;i++)
            {
                var m=src[i];
                if(m==null){dst[i]=null;continue;}
                var copy=new Material(m){name="Valoria ValueHierarchy · "+m.name};
                string n=(r.name+" "+m.name).ToLowerInvariant();

                Color target;
                if(n.Contains("roof")||n.Contains("tile")||n.Contains("slate"))
                    target=new Color(.30f,.33f,.34f,1f);
                else if(n.Contains("wood")||n.Contains("beam")||n.Contains("timber"))
                    target=new Color(.36f,.27f,.19f,1f);
                else
                    target=new Color(.58f,.54f,.47f,1f);

                if(copy.HasProperty("_BaseColor"))copy.SetColor("_BaseColor",target);
                else if(copy.HasProperty("_Color"))copy.SetColor("_Color",target);
                else if(copy.HasProperty("_BaseColorFactor"))copy.SetColor("_BaseColorFactor",target);
                if(copy.HasProperty("_Smoothness"))copy.SetFloat("_Smoothness",.035f);
                if(copy.HasProperty("_Metallic"))copy.SetFloat("_Metallic",0f);
                if(copy.HasProperty("_EmissionColor"))copy.SetColor("_EmissionColor",Color.black);
                dst[i]=copy;
            }
            r.sharedMaterials=dst;
        }

        static void SetAll(Renderer r,Material material)
        {
            var mats=r.sharedMaterials;
            for(int i=0;i<mats.Length;i++)mats[i]=material;
            r.sharedMaterials=mats;
        }

        static string Chain(Transform t)
        {
            string s="";
            for(var p=t;p!=null;p=p.parent)s+="|"+p.name.ToLowerInvariant();
            return s;
        }
    }
}
