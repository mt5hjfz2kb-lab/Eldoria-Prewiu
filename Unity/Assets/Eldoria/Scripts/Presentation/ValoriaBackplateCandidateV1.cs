using UnityEngine;
using Object=UnityEngine.Object;

namespace Eldoria.Presentation
{
    // Fixed-camera backplate proof using high-resolution CC0 mountain-valley photos.
    public static class ValoriaBackplateCandidateV1
    {
        public static bool Enabled=false;
        const string RootName="Valoria · Mountain Backplate Candidate v1";
        static Transform plate;

        public static bool Build(Transform parent,Camera camera,string resourceName)
        {
            if(!Enabled||parent==null||camera==null)return false;
            var old=GameObject.Find(RootName);if(old!=null)Object.DestroyImmediate(old);
            var tex=Resources.Load<Texture2D>("Valoria/BackplateCandidates/"+resourceName);
            if(tex==null)return false;

            var shader=Shader.Find("Unlit/Texture")??Shader.Find("Universal Render Pipeline/Unlit");
            if(shader==null)return false;

            var root=new GameObject(RootName).transform;root.SetParent(parent,true);
            var quad=GameObject.CreatePrimitive(PrimitiveType.Quad);
            quad.name=RootName+" · "+resourceName;
            quad.transform.SetParent(camera.transform,false);
            quad.transform.localPosition=new Vector3(0f,0f,58f);
            quad.transform.localRotation=Quaternion.identity;
            quad.transform.localScale=new Vector3(68f,38.25f,1f);

            var mat=new Material(shader){name="Valoria backplate · "+resourceName};
            if(mat.HasProperty("_MainTex"))mat.SetTexture("_MainTex",tex);
            if(mat.HasProperty("_BaseMap"))mat.SetTexture("_BaseMap",tex);
            if(mat.HasProperty("_Color"))mat.SetColor("_Color",new Color(.84f,.88f,.91f,1f));
            if(mat.HasProperty("_BaseColor"))mat.SetColor("_BaseColor",new Color(.84f,.88f,.91f,1f));
            if(mat.HasProperty("_Cull"))mat.SetFloat("_Cull",0f);
            if(mat.HasProperty("_CullMode"))mat.SetFloat("_CullMode",0f);
            if(mat.HasProperty("_ZWrite"))mat.SetFloat("_ZWrite",0f);
            var r=quad.GetComponent<Renderer>();
            r.sharedMaterial=mat;
            r.shadowCastingMode=UnityEngine.Rendering.ShadowCastingMode.Off;
            r.receiveShadows=false;
            var c=quad.GetComponent<Collider>();if(c!=null)c.enabled=false;
            plate=quad.transform;

            camera.clearFlags=CameraClearFlags.SolidColor;
            camera.backgroundColor=new Color(.48f,.64f,.76f);
            camera.farClipPlane=Mathf.Max(camera.farClipPlane,500f);
            return true;
        }

        public static void FitAspect(float aspect)
        {
            if(plate==null)return;
            plate.localScale=aspect<1f?new Vector3(31f,38.25f,1f):new Vector3(68f,38.25f,1f);
        }
    }
}
