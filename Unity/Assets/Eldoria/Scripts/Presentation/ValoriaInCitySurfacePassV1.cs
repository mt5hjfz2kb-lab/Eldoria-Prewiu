using System;
using System.Collections.Generic;
using Eldoria.Domain;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Eldoria.Presentation
{
    // VALORIA IN-CITY SURFACE & LIGHTING PASS v1
    // Surface-first benchmark proof. Existing geometry only; no gameplay/collider/hotspot ownership.
    public static class ValoriaInCitySurfacePassV1
    {
        public static bool Enabled=false;
        const string RootName="Valoria · In-City Surface Pass v1 · visual only";

        static readonly HashSet<string> EarthTargets=new HashSet<string>(StringComparer.Ordinal)
        {
            "VPD · lower terrace earth",
            "VPD · upper terrace earth",
            "VPD · L0 civic floor",
            "VPD · L0 west plot",
            "VPD · L0 east plot",
            "VPD · lower entry apron",
            "VPD · L1 landing",
            "VPD · L1 west plot",
            "VPD · L1 east plot",
            "Sawmill yard",
            "Training yard",
            "Valoria work court"
        };

        static readonly string[] StonePrefixes=
        {
            "VPD · L0 main street",
            "VPD · GroundKit main street",
            "VPD · GroundKit entry widening",
            "VPD · GroundKit L1 landing",
            "VPD · vertical stair ",
            "Bastion stair",
            "Valoria · worn stone route"
        };

        public static int LastEarthRenderers{get;private set;}
        public static int LastStoneRenderers{get;private set;}
        public static int LastRetainingRenderers{get;private set;}
        public static int LastLights{get;private set;}

        public static void Build(Transform parent,PlayerState state)
        {
            if(!Enabled||parent==null||state==null)return;
            var old=GameObject.Find(RootName);
            if(old!=null)Object.DestroyImmediate(old);

            LastEarthRenderers=0;
            LastStoneRenderers=0;
            LastRetainingRenderers=0;
            LastLights=0;

            var root=new GameObject(RootName);
            root.transform.SetParent(parent,true);

            var earth=ValoriaKit.ExternalPbrSurfaceMaterial(
                "dirt",new Color(.62f,.56f,.45f,1f),new Vector2(3.2f,3.2f),.012f,.94f)
                ?? ValoriaKit.DetailedSurfaceMaterial(
                    new Color(.34f,.305f,.245f,1f),"earth",new Vector2(3.2f,3.2f),.90f);

            var stone=ValoriaKit.ExternalPbrSurfaceMaterial(
                "rock",new Color(.72f,.67f,.57f,1f),new Vector2(2.6f,2.6f),.018f,.96f)
                ?? ValoriaKit.DetailedSurfaceMaterial(
                    new Color(.48f,.445f,.375f,1f),"earth",new Vector2(2.6f,2.6f),.94f);

            foreach(var r in Object.FindObjectsByType<Renderer>(FindObjectsSortMode.None))
            {
                if(r==null||!r.enabled||!r.gameObject.activeInHierarchy)continue;

                string earthOwner=FindOwner(r.transform,EarthTargets,null);
                if(earthOwner!=null)
                {
                    if(earth!=null)r.sharedMaterial=earth;
                    LastEarthRenderers++;
                    continue;
                }

                string stoneOwner=FindOwner(r.transform,null,StonePrefixes);
                if(stoneOwner!=null)
                {
                    if(stone!=null)r.sharedMaterial=stone;
                    LastStoneRenderers++;
                    continue;
                }

                if(IsRetainingSurface(r.transform))
                {
                    NudgeTexturedSurface(r,new Color(.76f,.71f,.62f,1f),.055f);
                    LastRetainingRenderers++;
                }
            }

            AddWarmLight(root.transform,"lower approach",new Vector3(0f,1.15f,-5.0f),.16f,3.8f);
            AddWarmLight(root.transform,"west work yard",new Vector3(-6.1f,1.35f,-2.4f),.13f,3.4f);
            AddWarmLight(root.transform,"east training yard",new Vector3(6.3f,1.30f,-3.1f),.12f,3.2f);
            AddWarmLight(root.transform,"upper landing",new Vector3(0f,3.15f,6.7f),.15f,3.6f);
        }

        static string FindOwner(Transform t,HashSet<string> exact,string[] prefixes)
        {
            while(t!=null)
            {
                string n=t.gameObject.name;
                if(exact!=null&&exact.Contains(n))return n;
                if(prefixes!=null)
                    for(int i=0;i<prefixes.Length;i++)
                        if(n.StartsWith(prefixes[i],StringComparison.Ordinal))return n;
                t=t.parent;
            }
            return null;
        }

        static bool IsRetainingSurface(Transform t)
        {
            while(t!=null)
            {
                string n=t.gameObject.name;
                if((n.StartsWith("VPD ·",StringComparison.Ordinal)||n.StartsWith("Valoria",StringComparison.Ordinal))
                    && n.IndexOf("retaining",StringComparison.OrdinalIgnoreCase)>=0)
                    return true;
                t=t.parent;
            }
            return false;
        }

        static void NudgeTexturedSurface(Renderer r,Color tint,float smoothness)
        {
            var mat=r.sharedMaterial;
            if(mat==null)return;

            var block=new MaterialPropertyBlock();
            r.GetPropertyBlock(block);
            if(mat.HasProperty("_BaseColor"))block.SetColor("_BaseColor",tint);
            else if(mat.HasProperty("_Color"))block.SetColor("_Color",tint);
            if(mat.HasProperty("_Smoothness"))block.SetFloat("_Smoothness",smoothness);
            r.SetPropertyBlock(block);
        }

        static void AddWarmLight(Transform root,string role,Vector3 position,float intensity,float range)
        {
            var go=new GameObject("Valoria · In-City Surface v1 · "+role+" warmth");
            go.transform.SetParent(root,true);
            go.transform.position=position;
            var light=go.AddComponent<Light>();
            light.type=LightType.Point;
            light.color=new Color(1f,.62f,.34f);
            light.intensity=intensity;
            light.range=range;
            light.shadows=LightShadows.None;
            LastLights++;
        }
    }
}
