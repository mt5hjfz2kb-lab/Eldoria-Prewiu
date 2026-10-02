using System;
using Eldoria.Domain;
using UnityEngine;
using Object=UnityEngine.Object;

namespace Eldoria.Presentation
{
    // Final residue cleanup over the strongest composite. Visual-only.
    public static class ValoriaMaterialResidueCleanupV2
    {
        public static bool Enabled=false;
        const string RootName="Valoria · Material Residue Cleanup v2";
        public static int SuppressedPeripheral{get;private set;}
        public static int SuppressedDisconnectedPeripheral{get;private set;}
        public static int NormalizedEnvironment{get;private set;}
        public static int NormalizedArchitecture{get;private set;}

        public static void Build(Transform parent,PlayerState state)
        {
            if(!Enabled||parent==null)return;
            var old=GameObject.Find(RootName);if(old!=null)Object.DestroyImmediate(old);
            var root=new GameObject(RootName).transform;root.SetParent(parent,true);

            SuppressedPeripheral=0;SuppressedDisconnectedPeripheral=0;NormalizedEnvironment=0;NormalizedArchitecture=0;

            SuppressPeripheralLegacy();
            SuppressDisconnectedPeripheralResidue();
            NormalizeResidualMaterials();
        }

        static void SuppressPeripheralLegacy()
        {
            foreach(var r in Object.FindObjectsByType<Renderer>(FindObjectsSortMode.None))
            {
                if(r==null||!r.enabled||!r.gameObject.activeInHierarchy)continue;
                string chain=Chain(r.transform);
                bool peripheralDwelling=
                    chain.Contains("vpd · west rebuilders upper dwelling")||
                    chain.Contains("vpd · west rebuilders home");
                if(peripheralDwelling)
                {
                    r.enabled=false;
                    SuppressedPeripheral++;
                }
            }
        }

        static void SuppressDisconnectedPeripheralResidue()
        {
            foreach(var r in Object.FindObjectsByType<Renderer>(FindObjectsSortMode.None))
            {
                if(r==null||!r.enabled||!r.gameObject.activeInHierarchy)continue;
                var b=r.bounds;
                bool outside=Mathf.Abs(b.center.x)>10.6f||b.center.z>12.0f||b.center.z<-9.3f;
                if(!outside)continue;

                string chain=Chain(r.transform);

                // Preserve accepted authored anchors and the current frame systems.
                bool keep=
                    chain.Contains("bastion")||
                    chain.Contains("aserradero")||
                    chain.Contains("cuartel")||
                    chain.Contains("granary")||
                    chain.Contains("granero")||
                    chain.Contains("backplate")||
                    chain.Contains("jagged peaks")||
                    chain.Contains("cliff island")||
                    chain.Contains("foreground edge");

                if(keep)continue;

                // This cleanup acts only on Valoria's presentation hierarchy. Gameplay
                // colliders/routes/reservations remain separate and untouched.
                bool presentation=
                    chain.Contains("valoria ·")||
                    chain.Contains("vpd ·")||
                    chain.Contains("assetlibrary")||
                    chain.Contains("mid-tier");

                if(!presentation)continue;

                r.enabled=false;
                SuppressedDisconnectedPeripheral++;
            }
        }

        static void NormalizeResidualMaterials()
        {
            var rock=ValoriaKit.ExternalPbrSurfaceMaterial(
                "rock",new Color(.48f,.47f,.43f,1f),new Vector2(3.1f,3.1f),.022f,1.04f)
                ?? ValoriaKit.SurfaceMaterial(new Color(.46f,.45f,.41f,1f),"stone",new Vector2(3.1f,3.1f));

            var earth=ValoriaKit.ExternalPbrSurfaceMaterial(
                "dirt",new Color(.43f,.38f,.30f,1f),new Vector2(3.4f,3.4f),.018f,.98f)
                ?? ValoriaKit.SurfaceMaterial(new Color(.40f,.36f,.29f,1f),"earth",new Vector2(3.4f,3.4f));

            var plaster=ValoriaKit.ExternalPbrSurfaceMaterial(
                "stone",new Color(.68f,.63f,.54f,1f),new Vector2(2.4f,2.4f),.028f,1.02f)
                ?? ValoriaKit.SurfaceMaterial(new Color(.66f,.61f,.53f,1f),"stone",new Vector2(2.4f,2.4f));

            foreach(var r in Object.FindObjectsByType<Renderer>(FindObjectsSortMode.None))
            {
                if(r==null||!r.enabled||!r.gameObject.activeInHierarchy)continue;
                string chain=Chain(r.transform);

                if(chain.Contains("backplate candidate")||chain.Contains("bastion · target")||
                   chain.Contains("valoria · bastion hero")||chain.Contains("aserradero")||
                   chain.Contains("cuartel")||chain.Contains("granary")||chain.Contains("granero"))
                    continue;

                bool environment=
                    chain.Contains("rescued seam")||chain.Contains("rock")||chain.Contains("geology")||
                    chain.Contains("terrain")||chain.Contains("cliff")||chain.Contains("boulder")||
                    chain.Contains("platform")||chain.Contains("terrace")||chain.Contains("apron");

                bool secondaryArchitecture=
                    chain.Contains("rescued upper civil residence")||
                    chain.Contains("mid-tier")||chain.Contains("civil residence");

                var mats=r.sharedMaterials;
                bool dirty=false;
                for(int i=0;i<mats.Length;i++)
                {
                    var m=mats[i];
                    if(m==null)continue;

                    Color col=Color.white;bool hasColor=false;
                    if(m.HasProperty("_BaseColor")){col=m.GetColor("_BaseColor");hasColor=true;}
                    else if(m.HasProperty("_Color")){col=m.GetColor("_Color");hasColor=true;}
                    else if(m.HasProperty("_BaseColorFactor")){col=m.GetColor("_BaseColorFactor");hasColor=true;}

                    float lum=hasColor?(.2126f*col.r+.7152f*col.g+.0722f*col.b):.5f;
                    bool hasTex=(m.HasProperty("_BaseMap")&&m.GetTexture("_BaseMap")!=null)||
                                (m.HasProperty("_MainTex")&&m.GetTexture("_MainTex")!=null)||
                                (m.HasProperty("_Albedo")&&m.GetTexture("_Albedo")!=null);

                    bool extreme=!hasTex&&(lum<.24f||lum>.78f);
                    bool knownSeam=chain.Contains("rescued seam");
                    bool knownBrokenRock=environment&&(m.name.Contains("Valoria ",StringComparison.OrdinalIgnoreCase)||!hasTex);

                    if(environment&&(extreme||knownSeam||knownBrokenRock))
                    {
                        mats[i]=chain.Contains("earth")||chain.Contains("dirt")||chain.Contains("ground")?earth:rock;
                        dirty=true;NormalizedEnvironment++;
                    }
                    else if(secondaryArchitecture&&!hasTex&&(lum<.22f||lum>.82f))
                    {
                        mats[i]=plaster;
                        dirty=true;NormalizedArchitecture++;
                    }
                }
                if(dirty)r.sharedMaterials=mats;
            }
        }

        static string Chain(Transform t)
        {
            string s="";
            for(var p=t;p!=null;p=p.parent)s+="|"+p.name.ToLowerInvariant();
            return s;
        }
    }
}
