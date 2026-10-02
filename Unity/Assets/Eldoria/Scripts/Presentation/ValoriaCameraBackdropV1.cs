using UnityEngine;
using Object=UnityEngine.Object;

namespace Eldoria.Presentation
{
    // Camera-matched backdrop candidate for fixed orthographic Valoria framing.
    // Proof-only until a crop is selected and persisted.
    public static class ValoriaCameraBackdropV1
    {
        public static bool Enabled=false;
        public static float CropX=0f;
        public static float CropY=.18f;
        public static float CropW=.36f;
        public static float CropH=.58f;

        const string RootName="Valoria · Camera Backdrop v1";
        static Material material;
        static Transform quad;

        public static bool Build(Transform parent,Camera camera)
        {
            if(!Enabled||parent==null||camera==null)return false;
            var old=GameObject.Find(RootName);
            if(old!=null)Object.DestroyImmediate(old);

            var tex=Resources.Load<Texture2D>("Valoria/SkyCandidates/alps_field");
            if(tex==null)return false;

            var shader=Shader.Find("Universal Render Pipeline/Unlit") ?? Shader.Find("Unlit/Texture");
            if(shader==null)return false;

            var root=new GameObject(RootName).transform;
            root.SetParent(parent,true);

            var go=GameObject.CreatePrimitive(PrimitiveType.Quad);
            go.name=RootName+" · quad";
            go.transform.SetParent(root,true);

            var dir=(new Vector3(0f,3.15f,5.8f)-camera.transform.position).normalized;
            go.transform.position=camera.transform.position+dir*52f;
            go.transform.rotation=Quaternion.LookRotation(dir,camera.transform.up);
            go.transform.localScale=new Vector3(58f,32.5f,1f);

            material=new Material(shader){name="Valoria · Alps Field backdrop material"};
            if(material.HasProperty("_BaseMap"))material.SetTexture("_BaseMap",tex);
            if(material.HasProperty("_MainTex"))material.SetTexture("_MainTex",tex);
            if(material.HasProperty("_BaseColor"))material.SetColor("_BaseColor",Color.white);
            if(material.HasProperty("_Color"))material.SetColor("_Color",Color.white);
            if(material.HasProperty("_Cull"))material.SetFloat("_Cull",0f);
            if(material.HasProperty("_CullMode"))material.SetFloat("_CullMode",0f);
            if(material.HasProperty("_ZWrite"))material.SetFloat("_ZWrite",0f);
            material.mainTexture=tex;
            go.GetComponent<Renderer>().sharedMaterial=material;
            var collider=go.GetComponent<Collider>(); if(collider!=null)collider.enabled=false;
            quad=go.transform;

            ApplyCrop();
            return true;
        }

        public static void ApplyCrop()
        {
            if(material==null)return;
            var scale=new Vector2(CropW,CropH);
            var offset=new Vector2(CropX,CropY);
            material.mainTextureScale=scale;
            material.mainTextureOffset=offset;
            if(material.HasProperty("_BaseMap"))
            {
                material.SetTextureScale("_BaseMap",scale);
                material.SetTextureOffset("_BaseMap",offset);
            }
            if(material.HasProperty("_MainTex"))
            {
                material.SetTextureScale("_MainTex",scale);
                material.SetTextureOffset("_MainTex",offset);
            }
        }

        public static void Refit(Camera camera)
        {
            if(quad==null||camera==null)return;
            var dir=(new Vector3(0f,3.15f,5.8f)-camera.transform.position).normalized;
            quad.position=camera.transform.position+dir*52f;
            quad.rotation=Quaternion.LookRotation(dir,camera.transform.up);
        }
    }
}
