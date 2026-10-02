using Eldoria.Domain;
using System;
using UnityEngine;
using Object=UnityEngine.Object;

namespace Eldoria.Presentation
{
    // Opens the rear valley visually while preserving all reserved gameplay/topology space.
    public static class ValoriaOpenValleyCompositionV1
    {
        public static bool Enabled=false;
        const string RootName="Valoria · Open Valley Composition v1";

        public static int SuppressedRenderers{get;private set;}

        public static void Build(Transform parent,PlayerState state)
        {
            if(!Enabled||parent==null)return;
            var old=GameObject.Find(RootName);
            if(old!=null)Object.DestroyImmediate(old);
            var root=new GameObject(RootName).transform;
            root.SetParent(parent,true);

            SuppressedRenderers=0;

            // These two IrregularGround objects are visual reserve skins only.
            SuppressVisualChain("VPD · upper civic mountain shelf");
            SuppressVisualChain("VPD · future valley shelf");

            // Historical whole-frame skins are also presentation-only.
            var heroValley=GameObject.Find("Valoria · Hero Frame valley terrain");
            if(heroValley!=null)Object.DestroyImmediate(heroValley);
            var worldFrame=GameObject.Find("Valoria · World Frame Mountain Terrain v1");
            if(worldFrame!=null)Object.DestroyImmediate(worldFrame);
            ValoriaWorldFrameMountainTerrainV1.Enabled=false;

            // Keep only small natural edge cues near the compact city. No gameplay ownership.
            var rock=Resources.Load<GameObject>("WorldInventory/Rock02");
            if(rock!=null)
            {
                AddRock(root,rock,"rear west lip",new Vector3(-9.6f,-.18f,11.8f),3.4f,1.35f,41f);
                AddRock(root,rock,"rear east lip",new Vector3(9.5f,-.18f,12.0f),3.4f,1.35f,219f);
            }
        }

        static void SuppressVisualChain(string name)
        {
            foreach(var r in Object.FindObjectsByType<Renderer>(FindObjectsSortMode.None))
            {
                if(r==null||!r.enabled||!r.gameObject.activeInHierarchy)continue;
                for(var t=r.transform;t!=null;t=t.parent)
                {
                    if(!string.Equals(t.name,name,StringComparison.Ordinal))continue;
                    r.enabled=false;
                    SuppressedRenderers++;
                    break;
                }
            }
        }

        static void AddRock(Transform root,GameObject source,string role,Vector3 p,float footprint,float height,float yaw)
        {
            var go=ValoriaKit.BenchmarkPieceIntegrated("Valoria · Open Valley · "+role,source,p,footprint,height,
                Quaternion.Euler(0f,yaw,0f),new Color(.39f,.40f,.36f,1f));
            if(go==null)return;
            go.transform.SetParent(root,true);
            foreach(var c in go.GetComponentsInChildren<Collider>(true))c.enabled=false;
            foreach(var b in go.GetComponentsInChildren<MonoBehaviour>(true))b.enabled=false;
        }
    }
}
