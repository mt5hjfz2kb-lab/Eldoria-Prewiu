using System;
using Eldoria.Domain;
using UnityEngine;
using Object=UnityEngine.Object;

namespace Eldoria.Presentation
{
    // Experimental visual-only shell staged by CI. It never owns gameplay collision or hotspots.
    public static class ValoriaFusedTransitionV2
    {
        public static int RenderersBuilt{get;private set;}
        public static void Build(Transform parent,PlayerState state)
        {
            var src=Resources.Load<GameObject>("Valoria/ExperimentalFusedTransition/ValoriaFusedTransition");
            if(src==null)throw new Exception("Fused Hero-to-city transition GLB was not staged.");
            var go=Object.Instantiate(src);
            go.name="Valoria · fused Hero-to-city transition v2 · visual only";
            go.transform.SetParent(parent,true);
            go.transform.localPosition=Vector3.zero;
            go.transform.localRotation=Quaternion.identity;

            var shader=Shader.Find("Eldoria/ValoriaCompositionGround");
            if(shader==null)throw new Exception("Composition triplanar shader missing.");
            var mat=new Material(shader){name="Valoria v2 · fused rock/ground triplanar"};
            foreach(var pair in new[]{("_RockTex","rock_diff"),("_GroundTex","dirt_diff"),("_RockNormal","rock_normal")})
            {
                var tex=Resources.Load<Texture2D>("Valoria/SurfaceCellExternal/"+pair.Item2);
                if(tex==null)throw new Exception("Fused transition surface missing: "+pair.Item2);
                tex.wrapMode=TextureWrapMode.Repeat;tex.anisoLevel=8;mat.SetTexture(pair.Item1,tex);
            }
            var rs=go.GetComponentsInChildren<Renderer>(true);
            if(rs.Length==0)throw new Exception("Fused transition has no renderer.");
            foreach(var r in rs){r.sharedMaterial=mat;r.receiveShadows=true;}
            foreach(var c in go.GetComponentsInChildren<Collider>(true))Object.DestroyImmediate(c);
            foreach(var h in go.GetComponentsInChildren<WorldHotspot>(true))Object.DestroyImmediate(h);
            foreach(var b in go.GetComponentsInChildren<MonoBehaviour>(true))b.enabled=false;
            RenderersBuilt=rs.Length;
        }
    }
}
