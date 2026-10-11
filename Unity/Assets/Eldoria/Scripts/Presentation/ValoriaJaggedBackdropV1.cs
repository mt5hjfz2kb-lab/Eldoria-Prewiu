using Eldoria.Domain;
using UnityEngine;
using UnityEngine.Rendering;
using Object=UnityEngine.Object;

namespace Eldoria.Presentation
{
    public static class ValoriaJaggedBackdropV1
    {
        public static bool Enabled=false;
        public static float VerticalBias=0f;
        const string RootName="Valoria · Jagged Peaks Backdrop v1";

        public static bool Build(Transform parent,PlayerState state)
        {
            if(!Enabled||parent==null)return false;

            var old=GameObject.Find(RootName);
            if(old!=null)Object.DestroyImmediate(old);

            var tex=Resources.Load<Texture2D>("Valoria/SkyCandidates/jagged_peaks_valley");
            var camera=Camera.main;
            if(tex==null||camera==null)return false;

            // Use the same proven camera-anchored technique as ValoriaBackplateCandidateV1.
            // The root follows the fixed gameplay camera; rebuilding for a new bias destroys
            // the previous plate cleanly instead of accumulating quads.
            var root=new GameObject(RootName).transform;
            root.SetParent(camera.transform,false);
            root.localPosition=Vector3.zero;
            root.localRotation=Quaternion.identity;
            root.localScale=Vector3.one;

            var quad=GameObject.CreatePrimitive(PrimitiveType.Quad);
            quad.name="Valoria · Jagged Peaks · CC0 backplate";
            quad.transform.SetParent(root,false);
            quad.transform.localPosition=new Vector3(0f,VerticalBias,58f);
            quad.transform.localRotation=Quaternion.identity;
            quad.transform.localScale=new Vector3(68f,38.25f,1f);

            var shader=Shader.Find("Unlit/Texture")??Shader.Find("Universal Render Pipeline/Unlit");
            if(shader==null)
            {
                Object.DestroyImmediate(root.gameObject);
                return false;
            }

            var mat=new Material(shader){name="Valoria · Jagged Peaks CC0 backplate"};
            if(mat.HasProperty("_MainTex"))mat.SetTexture("_MainTex",tex);
            if(mat.HasProperty("_BaseMap"))mat.SetTexture("_BaseMap",tex);
            var tint=new Color(.84f,.88f,.91f,1f);
            if(mat.HasProperty("_Color"))mat.SetColor("_Color",tint);
            if(mat.HasProperty("_BaseColor"))mat.SetColor("_BaseColor",tint);
            if(mat.HasProperty("_Cull"))mat.SetFloat("_Cull",0f);
            if(mat.HasProperty("_CullMode"))mat.SetFloat("_CullMode",0f);
            if(mat.HasProperty("_ZWrite"))mat.SetFloat("_ZWrite",0f);

            var renderer=quad.GetComponent<Renderer>();
            renderer.sharedMaterial=mat;
            renderer.shadowCastingMode=ShadowCastingMode.Off;
            renderer.receiveShadows=false;

            var collider=quad.GetComponent<Collider>();
            if(collider!=null)collider.enabled=false;

            camera.clearFlags=CameraClearFlags.SolidColor;
            camera.backgroundColor=new Color(.48f,.64f,.76f);
            camera.farClipPlane=Mathf.Max(camera.farClipPlane,500f);
            camera.allowHDR=true;
            return true;
        }
    }
}
