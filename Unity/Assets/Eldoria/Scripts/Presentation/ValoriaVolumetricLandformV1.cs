using System;
using Eldoria.Domain;
using UnityEngine;
using Object=UnityEngine.Object;

namespace Eldoria.Presentation
{
    public static class ValoriaVolumetricLandformV1
    {
        public static int RenderersBuilt{get;private set;}

        public static void Build(Transform parent,PlayerState state)
        {
            if(parent==null||state==null)throw new ArgumentNullException();
            var src=Resources.Load<GameObject>("Valoria/ExperimentalVolumetricLandform/ValoriaVolumetricLandform");
            if(src==null)throw new Exception("Volumetric landform GLB was not staged.");
            var go=Object.Instantiate(src);
            go.name="Valoria · volumetric contour-loft landform · visual only";
            go.transform.SetParent(parent,true);
            go.transform.localPosition=Vector3.zero;
            go.transform.localRotation=Quaternion.identity;
            go.transform.localScale=Vector3.one;

            var shader=Shader.Find("Eldoria/ValoriaCompositionGround");
            if(shader==null)throw new Exception("Composition ground shader missing.");
            var mat=new Material(shader){name="Valoria volumetric landform · triplanar rock-earth"};
            foreach(var pair in new[]{("_RockTex","rock_diff"),("_GroundTex","dirt_diff"),("_RockNormal","rock_normal")})
            {
                var tex=Resources.Load<Texture2D>("Valoria/SurfaceCellExternal/"+pair.Item2);
                if(tex==null)throw new Exception("Volumetric surface missing: "+pair.Item2);
                tex.wrapMode=TextureWrapMode.Repeat;
                tex.anisoLevel=8;
                mat.SetTexture(pair.Item1,tex);
            }

            var rs=go.GetComponentsInChildren<Renderer>(true);
            if(rs.Length==0)throw new Exception("Volumetric landform has no renderers.");
            foreach(var r in rs)
            {
                var mats=r.sharedMaterials;
                for(int i=0;i<mats.Length;i++)mats[i]=mat;
                r.sharedMaterials=mats;
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
