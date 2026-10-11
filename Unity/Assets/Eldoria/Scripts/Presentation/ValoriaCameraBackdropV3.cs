using UnityEngine;
using Object=UnityEngine.Object;

namespace Eldoria.Presentation
{
    // Adaptive, stylized, camera-locked distant landscape for Valoria.
    // Keeps the production orthographic camera and owns no gameplay state/collision.
    public static class ValoriaCameraBackdropV3
    {
        public static bool Enabled=false;

        public static Vector4 DesktopCrop=new Vector4(.60f,.33f,.35f,.41f);
        public static Vector4 MobileCrop=new Vector4(.24f,.27f,.42f,.47f);

        const string RootName="Valoria · Camera Backdrop v3";
        static Material backdropMaterial;
        static Material hazeMaterial;
        static Transform backdropQuad;
        static Transform hazeQuad;
        static Texture2D stylized;

        public static bool Build(Transform parent,Camera camera)
        {
            if(!Enabled||parent==null||camera==null)return false;
            var old=GameObject.Find(RootName);
            if(old!=null)Object.DestroyImmediate(old);

            var source=Resources.Load<Texture2D>("Valoria/SkyCandidates/alps_field");
            if(source==null)return false;
            stylized=BuildStylized(source);
            if(stylized==null)return false;

            var shader=Shader.Find("Unlit/Texture") ?? Shader.Find("Universal Render Pipeline/Unlit");
            if(shader==null)return false;

            var root=new GameObject(RootName).transform;
            root.SetParent(parent,true);

            var go=GameObject.CreatePrimitive(PrimitiveType.Quad);
            go.name=RootName+" · distant landscape";
            go.transform.SetParent(camera.transform,false);
            go.transform.localPosition=new Vector3(0f,0f,52f);
            go.transform.localRotation=Quaternion.identity;
            go.transform.localScale=new Vector3(65f,36.5f,1f);

            backdropMaterial=new Material(shader){name="Valoria · stylized Alps Field backdrop"};
            backdropMaterial.mainTexture=stylized;
            if(backdropMaterial.HasProperty("_BaseMap"))backdropMaterial.SetTexture("_BaseMap",stylized);
            if(backdropMaterial.HasProperty("_MainTex"))backdropMaterial.SetTexture("_MainTex",stylized);
            if(backdropMaterial.HasProperty("_BaseColor"))backdropMaterial.SetColor("_BaseColor",Color.white);
            if(backdropMaterial.HasProperty("_Color"))backdropMaterial.SetColor("_Color",Color.white);
            if(backdropMaterial.HasProperty("_Cull"))backdropMaterial.SetFloat("_Cull",0f);
            if(backdropMaterial.HasProperty("_CullMode"))backdropMaterial.SetFloat("_CullMode",0f);
            if(backdropMaterial.HasProperty("_ZWrite"))backdropMaterial.SetFloat("_ZWrite",0f);

            var renderer=go.GetComponent<Renderer>();
            renderer.sharedMaterial=backdropMaterial;
            renderer.shadowCastingMode=UnityEngine.Rendering.ShadowCastingMode.Off;
            renderer.receiveShadows=false;
            var collider=go.GetComponent<Collider>();if(collider!=null)collider.enabled=false;
            backdropQuad=go.transform;

            BuildHaze(camera);
            ApplyForAspect(camera.aspect);
            return true;
        }

        static Texture2D BuildStylized(Texture source)
        {
            const int width=640,height=320;
            var rt=RenderTexture.GetTemporary(width,height,0,RenderTextureFormat.ARGB32,RenderTextureReadWrite.Linear);
            var prev=RenderTexture.active;
            try
            {
                Graphics.Blit(source,rt);
                RenderTexture.active=rt;
                var tex=new Texture2D(width,height,TextureFormat.RGB24,false,true)
                {
                    name="Valoria · Alps Field stylized distant plate",
                    filterMode=FilterMode.Bilinear,
                    wrapMode=TextureWrapMode.Clamp
                };
                tex.ReadPixels(new Rect(0,0,width,height),0,0);
                tex.Apply(false,false);
                var pixels=tex.GetPixels();
                var haze=new Color(.55f,.64f,.69f,1f);
                for(int y=0;y<height;y++)
                {
                    float yn=y/(float)(height-1);
                    float lowerHaze=Mathf.Clamp01((.62f-yn)/.62f)*.18f;
                    for(int x=0;x<width;x++)
                    {
                        int i=y*width+x;
                        var c=pixels[i];
                        float lum=.2126f*c.r+.7152f*c.g+.0722f*c.b;
                        var grey=new Color(lum,lum,lum,1f);
                        c=Color.Lerp(grey,c,.52f);
                        c=new Color(
                            .5f+(c.r-.5f)*.80f,
                            .5f+(c.g-.5f)*.80f,
                            .5f+(c.b-.5f)*.80f,1f);
                        c=new Color(c.r*.86f,c.g*.91f,c.b*.95f,1f);
                        c=Color.Lerp(c,haze,lowerHaze+.045f);
                        pixels[i]=c;
                    }
                }
                tex.SetPixels(pixels);
                tex.Apply(false,false);
                return tex;
            }
            finally
            {
                RenderTexture.active=prev;
                RenderTexture.ReleaseTemporary(rt);
            }
        }

        static void BuildHaze(Camera camera)
        {
            var shader=Shader.Find("Unlit/Transparent") ?? Shader.Find("Legacy Shaders/Transparent/Diffuse");
            if(shader==null)return;
            var tex=new Texture2D(4,256,TextureFormat.RGBA32,false)
            {
                name="Valoria · distant valley atmospheric seam",
                wrapMode=TextureWrapMode.Clamp,
                filterMode=FilterMode.Bilinear
            };
            var pixels=new Color[4*256];
            for(int y=0;y<256;y++)
            {
                float t=y/255f;
                float a=Mathf.Pow(1f-t,2.1f)*.55f;
                var c=new Color(.52f,.59f,.61f,a);
                for(int x=0;x<4;x++)pixels[y*4+x]=c;
            }
            tex.SetPixels(pixels);tex.Apply();

            var haze=GameObject.CreatePrimitive(PrimitiveType.Quad);
            haze.name=RootName+" · seam haze";
            haze.transform.SetParent(camera.transform,false);
            haze.transform.localPosition=new Vector3(0f,-5.35f,51.4f);
            haze.transform.localRotation=Quaternion.identity;
            haze.transform.localScale=new Vector3(65f,14.8f,1f);

            hazeMaterial=new Material(shader){name="Valoria · distant valley seam haze"};
            hazeMaterial.mainTexture=tex;
            if(hazeMaterial.HasProperty("_MainTex"))hazeMaterial.SetTexture("_MainTex",tex);
            if(hazeMaterial.HasProperty("_Color"))hazeMaterial.SetColor("_Color",Color.white);
            if(hazeMaterial.HasProperty("_Cull"))hazeMaterial.SetFloat("_Cull",0f);
            if(hazeMaterial.HasProperty("_ZWrite"))hazeMaterial.SetFloat("_ZWrite",0f);
            var renderer=haze.GetComponent<Renderer>();
            renderer.sharedMaterial=hazeMaterial;
            renderer.shadowCastingMode=UnityEngine.Rendering.ShadowCastingMode.Off;
            renderer.receiveShadows=false;
            var collider=haze.GetComponent<Collider>();if(collider!=null)collider.enabled=false;
            hazeQuad=haze.transform;
        }

        public static void ApplyForAspect(float aspect)
        {
            var crop=aspect<1f?MobileCrop:DesktopCrop;
            ApplyCrop(crop);
            if(backdropQuad!=null)
                backdropQuad.localScale=aspect<1f?new Vector3(31f,36.5f,1f):new Vector3(65f,36.5f,1f);
            if(hazeQuad!=null)
                hazeQuad.localScale=aspect<1f?new Vector3(31f,14.8f,1f):new Vector3(65f,14.8f,1f);
        }

        public static void ApplyCrop(Vector4 crop)
        {
            if(backdropMaterial==null)return;
            var scale=new Vector2(crop.z,crop.w);
            var offset=new Vector2(crop.x,crop.y);
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
        }
    }
}
