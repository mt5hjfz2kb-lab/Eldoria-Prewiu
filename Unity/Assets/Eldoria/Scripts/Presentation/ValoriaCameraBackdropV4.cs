using UnityEngine;
using Object=UnityEngine.Object;

namespace Eldoria.Presentation
{
    // Camera-locked stylized valley backdrop for Valoria's fixed orthographic presentation.
    // Uses the CC0 Alps Field source staged by CI, but deliberately down-samples, blurs and grades it
    // so it reads as distant painted world depth rather than a photographic plate.
    public static class ValoriaCameraBackdropV4
    {
        public static bool Enabled=false;
        public static Vector4 Crop=new Vector4(.52f,.06f,.42f,.50f);

        const string RootName="Valoria · Camera Backdrop v4";
        static Material backdropMaterial;
        static Material hazeMaterial;
        static Texture2D stylized;
        static Transform backdropQuad;
        static Transform hazeQuad;

        public static bool Build(Transform parent,Camera camera)
        {
            if(!Enabled||parent==null||camera==null)return false;
            var old=GameObject.Find(RootName);if(old!=null)Object.DestroyImmediate(old);
            var source=Resources.Load<Texture2D>("Valoria/SkyCandidates/alps_field");
            if(source==null)return false;

            stylized=BuildStylized(source);
            if(stylized==null)return false;

            var shader=Shader.Find("Unlit/Texture")??Shader.Find("Universal Render Pipeline/Unlit");
            if(shader==null)return false;

            var root=new GameObject(RootName).transform;
            root.SetParent(parent,true);

            var quad=GameObject.CreatePrimitive(PrimitiveType.Quad);
            quad.name=RootName+" · painted valley";
            quad.transform.SetParent(camera.transform,false);
            quad.transform.localPosition=new Vector3(0f,0f,54f);
            quad.transform.localRotation=Quaternion.identity;
            quad.transform.localScale=new Vector3(66f,37.2f,1f);

            backdropMaterial=new Material(shader){name="Valoria · painted Alps backdrop v4"};
            backdropMaterial.mainTexture=stylized;
            if(backdropMaterial.HasProperty("_BaseMap"))backdropMaterial.SetTexture("_BaseMap",stylized);
            if(backdropMaterial.HasProperty("_MainTex"))backdropMaterial.SetTexture("_MainTex",stylized);
            if(backdropMaterial.HasProperty("_BaseColor"))backdropMaterial.SetColor("_BaseColor",Color.white);
            if(backdropMaterial.HasProperty("_Color"))backdropMaterial.SetColor("_Color",Color.white);
            if(backdropMaterial.HasProperty("_Cull"))backdropMaterial.SetFloat("_Cull",0f);
            if(backdropMaterial.HasProperty("_CullMode"))backdropMaterial.SetFloat("_CullMode",0f);
            if(backdropMaterial.HasProperty("_ZWrite"))backdropMaterial.SetFloat("_ZWrite",0f);
            var renderer=quad.GetComponent<Renderer>();
            renderer.sharedMaterial=backdropMaterial;
            renderer.shadowCastingMode=UnityEngine.Rendering.ShadowCastingMode.Off;
            renderer.receiveShadows=false;
            var collider=quad.GetComponent<Collider>();if(collider!=null)collider.enabled=false;
            backdropQuad=quad.transform;

            ApplyCrop();
            BuildLowerHaze(root,camera);
            return true;
        }

        static Texture2D BuildStylized(Texture source)
        {
            const int w=320,h=180;
            var rt=RenderTexture.GetTemporary(w,h,0,RenderTextureFormat.ARGB32,RenderTextureReadWrite.Linear);
            var prev=RenderTexture.active;
            try
            {
                Graphics.Blit(source,rt);
                RenderTexture.active=rt;
                var tex=new Texture2D(w,h,TextureFormat.RGB24,false,true)
                {
                    name="Valoria · painted valley source v4",
                    wrapMode=TextureWrapMode.Clamp,
                    filterMode=FilterMode.Bilinear
                };
                tex.ReadPixels(new Rect(0,0,w,h),0,0);
                tex.Apply(false,false);

                var pixels=tex.GetPixels();
                for(int pass=0;pass<3;pass++)pixels=Blur(pixels,w,h,2);

                for(int y=0;y<h;y++)
                {
                    float fy=y/(float)(h-1);
                    float distanceTint=Mathf.SmoothStep(0f,1f,fy);
                    for(int x=0;x<w;x++)
                    {
                        int i=y*w+x;
                        var c=pixels[i];
                        float lum=.2126f*c.r+.7152f*c.g+.0722f*c.b;
                        var grey=new Color(lum,lum,lum,1f);
                        c=Color.Lerp(grey,c,.72f);
                        c=new Color(
                            Mathf.Clamp01(.50f+(c.r-.50f)*.88f),
                            Mathf.Clamp01(.50f+(c.g-.50f)*.92f),
                            Mathf.Clamp01(.50f+(c.b-.50f)*.96f),1f);

                        // Push the lower valley toward cool green/stone and the upper sky toward blue.
                        var lower=new Color(.36f,.48f,.43f,1f);
                        var upper=new Color(.49f,.65f,.78f,1f);
                        c=Color.Lerp(c,lower,(1f-fy)*.18f);
                        c=Color.Lerp(c,upper,distanceTint*.10f);
                        pixels[i]=c;
                    }
                }

                tex.SetPixels(pixels);tex.Apply(false,false);
                return tex;
            }
            finally
            {
                RenderTexture.active=prev;
                RenderTexture.ReleaseTemporary(rt);
            }
        }

        static Color[] Blur(Color[] src,int w,int h,int radius)
        {
            var dst=new Color[src.Length];
            for(int y=0;y<h;y++)
            for(int x=0;x<w;x++)
            {
                Color sum=Color.black;int count=0;
                for(int oy=-radius;oy<=radius;oy++)
                {
                    int yy=Mathf.Clamp(y+oy,0,h-1);
                    for(int ox=-radius;ox<=radius;ox++)
                    {
                        int xx=Mathf.Clamp(x+ox,0,w-1);
                        sum+=src[yy*w+xx];count++;
                    }
                }
                dst[y*w+x]=sum/Mathf.Max(1,count);
            }
            return dst;
        }

        static void ApplyCrop()
        {
            if(backdropMaterial==null)return;
            var scale=new Vector2(Crop.z,Crop.w);
            var offset=new Vector2(Crop.x,Crop.y);
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

        static void BuildLowerHaze(Transform root,Camera camera)
        {
            var shader=Shader.Find("Unlit/Transparent")??Shader.Find("Legacy Shaders/Transparent/Diffuse");
            if(shader==null)return;
            var tex=new Texture2D(4,256,TextureFormat.RGBA32,false)
            {
                name="Valoria · backdrop lower atmospheric blend v4",
                wrapMode=TextureWrapMode.Clamp,
                filterMode=FilterMode.Bilinear
            };
            var pixels=new Color[4*256];
            for(int y=0;y<256;y++)
            {
                float t=y/255f;
                float a=Mathf.Pow(1f-t,2.0f)*.78f;
                var c=new Color(.47f,.52f,.50f,a);
                for(int x=0;x<4;x++)pixels[y*4+x]=c;
            }
            tex.SetPixels(pixels);tex.Apply();

            var haze=GameObject.CreatePrimitive(PrimitiveType.Quad);
            haze.name=RootName+" · lower haze";
            haze.transform.SetParent(camera.transform,false);
            haze.transform.localPosition=new Vector3(0f,-4.9f,53.2f);
            haze.transform.localRotation=Quaternion.identity;
            haze.transform.localScale=new Vector3(66f,16f,1f);

            hazeMaterial=new Material(shader){name="Valoria · backdrop haze v4"};
            hazeMaterial.mainTexture=tex;
            if(hazeMaterial.HasProperty("_MainTex"))hazeMaterial.SetTexture("_MainTex",tex);
            if(hazeMaterial.HasProperty("_Color"))hazeMaterial.SetColor("_Color",Color.white);
            if(hazeMaterial.HasProperty("_Cull"))hazeMaterial.SetFloat("_Cull",0f);
            if(hazeMaterial.HasProperty("_ZWrite"))hazeMaterial.SetFloat("_ZWrite",0f);
            var r=haze.GetComponent<Renderer>();
            r.sharedMaterial=hazeMaterial;
            r.shadowCastingMode=UnityEngine.Rendering.ShadowCastingMode.Off;
            r.receiveShadows=false;
            var c=haze.GetComponent<Collider>();if(c!=null)c.enabled=false;
            hazeQuad=haze.transform;
        }

        public static void ApplyForAspect(float aspect)
        {
            if(backdropQuad!=null)backdropQuad.localScale=aspect<1f?new Vector3(34f,37.2f,1f):new Vector3(66f,37.2f,1f);
            if(hazeQuad!=null)hazeQuad.localScale=aspect<1f?new Vector3(34f,16f,1f):new Vector3(66f,16f,1f);
        }
    }
}
