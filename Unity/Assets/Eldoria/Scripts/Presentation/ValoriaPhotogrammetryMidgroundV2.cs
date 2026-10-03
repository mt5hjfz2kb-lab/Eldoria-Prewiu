using System;
using Eldoria.Domain;
using UnityEngine;
using Object=UnityEngine.Object;

namespace Eldoria.Presentation
{
    // Experimental full-width CC0 photogrammetry midground. Visual-only.
    public static class ValoriaPhotogrammetryMidgroundV2
    {
        public static int RenderersBuilt{get;private set;}
        public static void Build(Transform parent,PlayerState state)
        {
            var src=Resources.Load<GameObject>("Valoria/ExperimentalPhotogrammetryMidground/ValoriaPhotogrammetryMidground");
            if(src==null)throw new Exception("Photogrammetry midground GLB was not staged.");
            var go=Object.Instantiate(src);
            go.name="Valoria · photogrammetry midground v2 · visual only";
            go.transform.SetParent(parent,true);
            go.transform.localPosition=Vector3.zero;
            go.transform.localRotation=Quaternion.identity;
            go.transform.localScale=Vector3.one;

            var rs=go.GetComponentsInChildren<Renderer>(true);
            if(rs.Length==0)throw new Exception("Photogrammetry midground has no renderers.");

            // Geometry comes from photogrammetry, but Valoria owns the material language.
            // World-space triplanar projection removes scan-albedo mismatch and UV/texel-density seams.
            var shader=Shader.Find("Eldoria/ValoriaCompositionGround");
            if(shader==null)throw new Exception("Valoria triplanar ground shader missing.");
            var terrainMat=new Material(shader){name="Valoria Shell v2 · unified rock-ground triplanar"};
            foreach(var pair in new[]{("_RockTex","rock_diff"),("_GroundTex","dirt_diff"),("_RockNormal","rock_normal")})
            {
                var tex=Resources.Load<Texture2D>("Valoria/SurfaceCellExternal/"+pair.Item2);
                if(tex==null)throw new Exception("Valoria shared terrain texture missing: "+pair.Item2);
                tex.wrapMode=TextureWrapMode.Repeat;tex.anisoLevel=8;terrainMat.SetTexture(pair.Item1,tex);
            }
            foreach(var r in rs)
            {
                r.sharedMaterial=terrainMat;
                r.receiveShadows=true;
                r.shadowCastingMode=UnityEngine.Rendering.ShadowCastingMode.On;
            }
            foreach(var c in go.GetComponentsInChildren<Collider>(true))Object.DestroyImmediate(c);
            foreach(var h in go.GetComponentsInChildren<WorldHotspot>(true))Object.DestroyImmediate(h);
            foreach(var b in go.GetComponentsInChildren<MonoBehaviour>(true))b.enabled=false;
            RenderersBuilt=rs.Length;
        }
    }
}
