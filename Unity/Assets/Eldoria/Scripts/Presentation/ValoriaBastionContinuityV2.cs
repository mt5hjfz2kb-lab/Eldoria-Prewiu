using System;
using Eldoria.Domain;
using UnityEngine;
using Object=UnityEngine.Object;

namespace Eldoria.Presentation
{
    // Full fixed-camera visual set. Gameplay objects remain authoritative but their renderers are hidden.
    public static class ValoriaBastionContinuityV2
    {
        public static int RenderersBuilt{get;private set;}
        public static int ExistingRenderersHidden{get;private set;}

        public static void Build(Transform parent,PlayerState state)
        {
            var src=Resources.Load<GameObject>("Valoria/ExperimentalBastionContinuity/ValoriaBastionContinuity");
            if(src==null)throw new Exception("Fixed-camera DCC set was not staged.");

            ExistingRenderersHidden=HideExistingVisuals();

            var go=Object.Instantiate(src);
            go.name="Valoria · fixed-camera DCC set v1 · visual only";
            go.transform.SetParent(parent,true);
            go.transform.localPosition=Vector3.zero;
            go.transform.localRotation=Quaternion.identity;
            go.transform.localScale=Vector3.one;

            var ground=ValoriaKit.ExternalPbrSurfaceMaterial(
                "dirt",new Color(.34f,.30f,.24f,1f),new Vector2(2.8f,2.8f),.014f,.94f)
                ?? ValoriaKit.DetailedSurfaceMaterial(new Color(.34f,.30f,.24f,1f),"earth",new Vector2(2.8f,2.8f),.94f);
            var stone=ValoriaKit.ExternalPbrSurfaceMaterial(
                "stone",new Color(.39f,.38f,.35f,1f),new Vector2(2.2f,2.2f),.018f,.93f)
                ?? ValoriaKit.DetailedSurfaceMaterial(new Color(.39f,.38f,.35f,1f),"stone",new Vector2(2.2f,2.2f),.93f);

            var rs=go.GetComponentsInChildren<Renderer>(true);
            if(rs.Length<20)throw new Exception("Fixed-camera DCC set renderer set incomplete.");
            foreach(var r in rs)
            {
                string n=r.gameObject.name.ToLowerInvariant();
                // Preserve authored/source materials for Hero and all real architecture/rock assets.
                // Only generated support geometry gets the common world material vocabulary.
                if(n.Contains("dcc_macroform"))
                {
                    var mats=r.sharedMaterials;
                    for(int i=0;i<mats.Length;i++)mats[i]=(i==0?rock:ground);
                    r.sharedMaterials=mats;
                }
                else if(n.Contains("dcc_rock"))
                    ReplaceAll(r,rock);
                else if(n.Contains("dcc_landing")||n.Contains("dcc_step"))
                    ReplaceAll(r,stone);
                r.receiveShadows=true;
                r.shadowCastingMode=UnityEngine.Rendering.ShadowCastingMode.On;
            }
            foreach(var c in go.GetComponentsInChildren<Collider>(true))Object.DestroyImmediate(c);
            foreach(var h in go.GetComponentsInChildren<WorldHotspot>(true))Object.DestroyImmediate(h);
            foreach(var b in go.GetComponentsInChildren<MonoBehaviour>(true))b.enabled=false;
            RenderersBuilt=rs.Length;
        }

        static void ReplaceAll(Renderer r,Material m)
        {
            var mats=r.sharedMaterials;
            for(int i=0;i<mats.Length;i++)mats[i]=m;
            r.sharedMaterials=mats;
        }

        static int HideExistingVisuals()
        {
            int count=0;
            foreach(var r in Object.FindObjectsByType<Renderer>(FindObjectsSortMode.None))
            {
                if(r==null||!r.enabled||!r.gameObject.activeInHierarchy)continue;
                string chain=Chain(r.transform);
                if(chain.Contains("backplate"))continue;
                // All previous presentation renderers are replaced by the authored DCC set.
                // This includes renderers attached to gameplay-authoritative objects; only rendering
                // is disabled, leaving hotspots/colliders/state untouched.
                r.enabled=false;
                count++;
            }
            return count;
        }

        static string Chain(Transform t)
        {
            var s="";
            for(var p=t;p!=null;p=p.parent)s+=p.name.ToLowerInvariant()+"|";
            return s;
        }
    }
}
