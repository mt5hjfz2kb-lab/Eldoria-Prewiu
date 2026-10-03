using System;
using Eldoria.Domain;
using UnityEngine;
using Object=UnityEngine.Object;

namespace Eldoria.Presentation
{
    public static class ValoriaBastionContinuityV2
    {
        public static int RenderersBuilt{get;private set;}
        public static void Build(Transform parent,PlayerState state)
        {
            var src=Resources.Load<GameObject>("Valoria/ExperimentalBastionContinuity/ValoriaBastionContinuity");
            if(src==null)throw new Exception("Bastion continuity GLB was not staged.");
            var go=Object.Instantiate(src);
            go.name="Valoria · authored Bastion-to-city continuity · visual only";
            go.transform.SetParent(parent,true);
            go.transform.localPosition=Vector3.zero;
            go.transform.localRotation=Quaternion.identity;
            go.transform.localScale=Vector3.one;

            var rock=ValoriaKit.ExternalPbrSurfaceMaterial(
                "rock",new Color(.37f,.35f,.31f,1f),new Vector2(2.45f,2.45f),.018f,.93f)
                ?? ValoriaKit.DetailedSurfaceMaterial(new Color(.37f,.35f,.31f,1f),"stone",new Vector2(2.45f,2.45f),.93f);
            var ground=ValoriaKit.ExternalPbrSurfaceMaterial(
                "dirt",new Color(.34f,.30f,.24f,1f),new Vector2(2.9f,2.9f),.014f,.94f)
                ?? ValoriaKit.DetailedSurfaceMaterial(new Color(.34f,.30f,.24f,1f),"earth",new Vector2(2.9f,2.9f),.94f);
            var stone=ValoriaKit.ExternalPbrSurfaceMaterial(
                "stone",new Color(.39f,.38f,.35f,1f),new Vector2(2.2f,2.2f),.018f,.93f)
                ?? ValoriaKit.DetailedSurfaceMaterial(new Color(.39f,.38f,.35f,1f),"stone",new Vector2(2.2f,2.2f),.93f);

            var rs=go.GetComponentsInChildren<Renderer>(true);
            if(rs.Length==0)throw new Exception("Bastion continuity has no renderers.");
            foreach(var r in rs)
            {
                string n=r.gameObject.name.ToLowerInvariant();
                Material m=n.Contains("retaining")||n.Contains("civic")?stone:
                           n.Contains("macroform")?rock:ground;
                var mats=r.sharedMaterials;
                for(int i=0;i<mats.Length;i++)mats[i]=m;
                r.sharedMaterials=mats;r.receiveShadows=true;
                r.shadowCastingMode=UnityEngine.Rendering.ShadowCastingMode.On;
            }
            foreach(var c in go.GetComponentsInChildren<Collider>(true))Object.DestroyImmediate(c);
            foreach(var h in go.GetComponentsInChildren<WorldHotspot>(true))Object.DestroyImmediate(h);
            foreach(var b in go.GetComponentsInChildren<MonoBehaviour>(true))b.enabled=false;
            RenderersBuilt=rs.Length;
        }
    }
}
