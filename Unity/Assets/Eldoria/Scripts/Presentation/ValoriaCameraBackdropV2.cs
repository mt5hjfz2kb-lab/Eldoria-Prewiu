using UnityEngine;
using Object=UnityEngine.Object;

namespace Eldoria.Presentation
{
    // Atmospheric camera-locked backdrop for fixed orthographic Valoria.
    // Proof-only until one crop is selected and the CC0 texture is persisted.
    public static class ValoriaCameraBackdropV2
    {
        public static bool Enabled=false;
        public static float CropX=.58f;
        public static float CropY=.32f;
        public static float CropW=.38f;
        public static float CropH=.42f;
        public static Color BackdropTint=new Color(.74f,.78f,.80f,1f);
        public static float HazeStrength=.62f;

        const string RootName="Valoria · Camera Backdrop v2";
        static Material backdropMaterial;
        static Material hazeMaterial;
        static Transform backdropQuad;
        static Transform hazeQuad;

        public static bool Build(Transform parent,Camera camera)
        {
            if(!Enabled||parent==null||camera==null)return false;
            var old=GameObject.Find(RootName);
            if(old!=null)Object.DestroyImmediate(old);

            var tex=Resources.Load<Texture2D>("Valoria/SkyCandidates/alps_field");
            if(tex==null)return false;

            var shader=Shader.Find("Unlit/Texture") ?? Shader.Find("Universal Render Pipeline/Unlit");
            if(shader==null)return false;

            var root=new GameObject(RootName).transform;
            root.SetParent(parent,true);

            var go=GameObject.CreatePrimitive(PrimitiveType.Quad);
            go.name=RootName+" · landscape";
            go.transform.SetParent(camera.transform,false);
            go.transform.localPosition=new Vector3(0f,0f,52f);
            go.transform.localRotation=Quaternion.identity;
            go.transform.localScale=new Vector3(64f,36f,1f);

            backdropMaterial=new Material(shader){name="Valoria · Alps Field atmospheric backdrop"};
            if(backdropMaterial.HasProperty("_BaseMap"))backdropMaterial.SetTexture("_BaseMap",tex);
            if(backdropMaterial.HasProperty("_MainTex"))backdropMaterial.SetTexture("_MainTex",tex);
            if(backdropMaterial.HasProperty("_BaseColor"))backdropMaterial.SetColor("_BaseColor",BackdropTint);
            if(backdropMaterial.HasProperty("_Color"))backdropMaterial.SetColor("_Color",BackdropTint);
            if(backdropMaterial.HasProperty("_Cull"))backdropMaterial.SetFloat("_Cull",0f);
            if(backdropMaterial.HasProperty("_CullMode"))backdropMaterial.SetFloat("_CullMode",0f);
            if(backdropMaterial.HasProperty("_ZWrite"))backdropMaterial.SetFloat("_ZWrite",0f);
            backdropMaterial.mainTexture=tex;
            go.GetComponent<Renderer>().sharedMaterial=backdropMaterial;
            go.GetComponent<Renderer>().shadowCastingMode=UnityEngine.Rendering.ShadowCastingMode.Off;
            go.GetComponent<Renderer>().receiveShadows=false;
            var col=go.GetComponent<Collider>();if(col!=null)col.enabled=false;
            backdropQuad=go.transform;

            BuildHaze(camera,root);
            ApplyLook();
            return true;
        }

        static void BuildHaze(Camera camera,Transform root)
        {
            var shader=Shader.Find("Unlit/Transparent") ?? Shader.Find("Legacy Shaders/Transparent/Diffuse");
            if(shader==null)return;

            var tex=new Texture2D(4,256,TextureFormat.RGBA32,false)
            {
                name="Valoria · backdrop seam haze",
                wrapMode=TextureWrapMode.Clamp,
                filterMode=FilterMode.Bilinear
            };
            var pixels=new Color[4*256];
            for(int y=0;y<256;y++)
            {
                float t=y/255f;
                float a=Mathf.Clamp01(1f-t*1.55f)*HazeStrength;
                a*=Mathf.SmoothStep(1f,0f,t);
                var c=new Color(.55f,.61f,.64f,a);
                for(int x=0;x<4;x++)pixels[y*4+x]=c;
            }
            tex.SetPixels(pixels);tex.Apply();

            var haze=GameObject.CreatePrimitive(PrimitiveType.Quad);
            haze.name=RootName+" · atmospheric seam haze";
            haze.transform.SetParent(camera.transform,false);
            haze.transform.localPosition=new Vector3(0f,-5.7f,51.6f);
            haze.transform.localRotation=Quaternion.identity;
            haze.transform.localScale=new Vector3(64f,13.5f,1f);

            hazeMaterial=new Material(shader){name="Valoria · backdrop seam haze material"};
            hazeMaterial.mainTexture=tex;
            if(hazeMaterial.HasProperty("_MainTex"))hazeMaterial.SetTexture("_MainTex",tex);
            if(hazeMaterial.HasProperty("_Color"))hazeMaterial.SetColor("_Color",Color.white);
            if(hazeMaterial.HasProperty("_Cull"))hazeMaterial.SetFloat("_Cull",0f);
            if(hazeMaterial.HasProperty("_ZWrite"))hazeMaterial.SetFloat("_ZWrite",0f);
            haze.GetComponent<Renderer>().sharedMaterial=hazeMaterial;
            haze.GetComponent<Renderer>().shadowCastingMode=UnityEngine.Rendering.ShadowCastingMode.Off;
            haze.GetComponent<Renderer>().receiveShadows=false;
            var hc=haze.GetComponent<Collider>();if(hc!=null)hc.enabled=false;
            hazeQuad=haze.transform;
        }

        public static void ApplyLook()
        {
            if(backdropMaterial!=null)
            {
                var scale=new Vector2(CropW,CropH);
                var offset=new Vector2(CropX,CropY);
                backdropMaterial.mainTextureScale=scale;
                backdropMaterial.mainTextureOffset=offset;
                if(backdropMaterial.HasProperty("_BaseMap"))
                {
                    backdropMaterial.SetTextureScale("_BaseMap",scale);
                    backdropMaterial.SetTextureOffset("_BaseMap",offset);
                }
                if(backdropMaterial.HasProperty("_MainTex"))
                {
                    backdropMaterial.SetTextureScale("_MainTex",scale);
                    backdropMaterial.SetTextureOffset("_MainTex",offset);
                }
                if(backdropMaterial.HasProperty("_BaseColor"))backdropMaterial.SetColor("_BaseColor",BackdropTint);
                if(backdropMaterial.HasProperty("_Color"))backdropMaterial.SetColor("_Color",BackdropTint);
            }
        }

        public static void SetHazeVisible(bool visible)
        {
            if(hazeQuad!=null)hazeQuad.gameObject.SetActive(visible);
        }
    }
}
