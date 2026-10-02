using Eldoria.Domain;
using UnityEngine;
using Object=UnityEngine.Object;

namespace Eldoria.Presentation
{
    // Whole-frame sanitation layer for pathological imported/material responses still visible
    // in the strongest integrated Valoria frame. Visual-only.
    public static class ValoriaFullFrameArtifactCleanupV1
    {
        public static bool Enabled=false;
        public static int RockRenderersNormalized{get;private set;}
        public static int TerraceRenderersNormalized{get;private set;}
        public static int FoliageRenderersLifted{get;private set;}
        public static int DarkFlatRenderersLifted{get;private set;}

        public static void Build(Transform parent,PlayerState state)
        {
            if(!Enabled||parent==null)return;

            RockRenderersNormalized=0;
            TerraceRenderersNormalized=0;
            FoliageRenderersLifted=0;
            DarkFlatRenderersLifted=0;

            var rock=ValoriaKit.ExternalPbrSurfaceMaterial(
                "rock",new Color(.46f,.45f,.41f,1f),new Vector2(2.9f,2.9f),.022f,1.03f)
                ?? ValoriaKit.SurfaceMaterial(new Color(.44f,.43f,.39f,1f),"stone",new Vector2(2.9f,2.9f));

            var terrace=ValoriaKit.ExternalPbrSurfaceMaterial(
                "rock",new Color(.50f,.48f,.43f,1f),new Vector2(3.3f,3.3f),.020f,1.01f)
                ?? ValoriaKit.SurfaceMaterial(new Color(.48f,.46f,.41f,1f),"stone",new Vector2(3.3f,3.3f));

            var foliage=ValoriaKit.Material(new Color(.24f,.34f,.23f,1f));
            var darkStone=ValoriaKit.Material(new Color(.38f,.37f,.34f,1f));

            foreach(var r in Object.FindObjectsByType<Renderer>(FindObjectsSortMode.None))
            {
                if(r==null||!r.enabled||!r.gameObject.activeInHierarchy)continue;
                string chain=Chain(r.transform);

                // Never touch the hero castle, HUD/world labels or photographic backplate.
                if(chain.Contains("bastion hero")||
                   chain.Contains("bastion · dedicated")||
                   chain.Contains("backplate")||
                   chain.Contains("banner")||
                   chain.Contains("flag"))continue;

                bool rockFamily=
                    chain.Contains("rock01")||chain.Contains("rock02")||
                    chain.Contains("slavicboulder")||chain.Contains("slavicflatrock")||
                    chain.Contains("edge geology")||chain.Contains("buried core rock")||
                    chain.Contains("residual cleanup · rock")||
                    chain.Contains("foreground edge · front ridge")||
                    chain.Contains("foreground edge · west boulder")||
                    chain.Contains("foreground edge · east boulder");

                bool terraceFamily=
                    chain.Contains("cliff island · lower")||
                    chain.Contains("cliff island · middle")||
                    chain.Contains("cliff island · upper")||
                    chain.Contains("broadrockplatform")||
                    chain.Contains("steppedrockterrace");

                bool foliageFamily=
                    chain.Contains("tree01")||chain.Contains("pine")||
                    chain.Contains("environment uplift · tree")||
                    chain.Contains("edge pine");

                if(rockFamily)
                {
                    SetAll(r,rock);
                    RockRenderersNormalized++;
                    continue;
                }

                if(terraceFamily)
                {
                    SetAll(r,terrace);
                    TerraceRenderersNormalized++;
                    continue;
                }

                if(foliageFamily && IsPathologicallyDark(r))
                {
                    SetAll(r,foliage);
                    FoliageRenderersLifted++;
                    continue;
                }

                // Catch only textureless near-black presentation renderers. This is intentionally
                // conservative: textured roofs/timber and the hero castle are preserved.
                if(IsPathologicallyDark(r) && HasNoBaseTexture(r) &&
                   (chain.Contains("valoria ·")||chain.Contains("vpd ·")))
                {
                    SetAll(r,darkStone);
                    DarkFlatRenderersLifted++;
                }
            }
        }

        static void SetAll(Renderer r,Material material)
        {
            var mats=r.sharedMaterials;
            for(int i=0;i<mats.Length;i++)mats[i]=material;
            r.sharedMaterials=mats;
        }

        static bool IsPathologicallyDark(Renderer r)
        {
            bool any=false;
            foreach(var m in r.sharedMaterials)
            {
                if(m==null)continue;
                any=true;
                var c=BaseColor(m);
                float lum=.2126f*c.r+.7152f*c.g+.0722f*c.b;
                if(lum>=.16f)return false;
            }
            return any;
        }

        static bool HasNoBaseTexture(Renderer r)
        {
            foreach(var m in r.sharedMaterials)
            {
                if(m==null)continue;
                Texture t=null;
                if(m.HasProperty("_BaseMap"))t=m.GetTexture("_BaseMap");
                if(t==null&&m.HasProperty("_MainTex"))t=m.GetTexture("_MainTex");
                if(t!=null)return false;
            }
            return true;
        }

        static Color BaseColor(Material m)
        {
            if(m.HasProperty("_BaseColor"))return m.GetColor("_BaseColor");
            if(m.HasProperty("_Color"))return m.GetColor("_Color");
            if(m.HasProperty("_BaseColorFactor"))return m.GetColor("_BaseColorFactor");
            return Color.white;
        }

        static string Chain(Transform t)
        {
            string s="";
            for(var p=t;p!=null;p=p.parent)s+="|"+p.name.ToLowerInvariant();
            return s;
        }
    }
}
