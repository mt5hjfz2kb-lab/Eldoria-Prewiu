// ASHEN CITADEL · premium interface skin — generated procedural source, not concept art.
// All panels share one restrained dark-slate/brass language; the genuine
// Eldoria gateway still owns the commands and save state.
using System;
using UnityEngine;

namespace Eldoria.ExperimentalAshenCitadel
{
    public static class AshenInterfaceIdentity
    {
        internal static readonly Color Ivory=new Color(.95f,.91f,.82f);
        internal static readonly Color Muted=new Color(.74f,.77f,.74f);
        private static Sprite panel;
        private static Sprite primary;
        private static Sprite secondary;
        private static Sprite crest;

        public static Sprite Panel => panel??(panel=Resources.Load<Sprite>("AshenPremiumPanel")??
            Make("Ashen · slate frame",256,256,18,18,SamplePanel));
        public static Sprite Primary => primary??(primary=Resources.Load<Sprite>("AshenPremiumButtonPrimary")??
            Make("Ashen · action",256,72,19,14,(x,y)=>SampleButton(x,y,true)));
        public static Sprite Secondary => secondary??(secondary=Resources.Load<Sprite>("AshenPremiumButtonSecondary")??
            Make("Ashen · quiet action",256,72,19,14,(x,y)=>SampleButton(x,y,false)));
        public static Sprite Crest => crest??(crest=Resources.Load<Sprite>("AshenPremiumCrest")??
            Make("Ashen · citadel seal",128,128,0,0,SampleCrest));
        // Editor bakes the source-defined raster into real Unity Sprite assets.
        // Runtime may use a generated fallback if a scene has never been rebuilt.
        public static void RebindAfterBake()
        {
            panel=null;primary=null;secondary=null;crest=null;
        }

        private static Sprite Make(string name,int width,int height,int bx,int by,Func<int,int,Color32> colour)
        {
            var texture=new Texture2D(width,height,TextureFormat.RGBA32,false,false)
            {
                name=name+" texture",wrapMode=TextureWrapMode.Clamp,
                filterMode=FilterMode.Bilinear
            };
            var pixels=new Color32[width*height];
            for(int y=0;y<height;y++)for(int x=0;x<width;x++)
                pixels[y*width+x]=colour(x,y);
            texture.SetPixels32(pixels);
            texture.Apply(false,false);
            var border=new Vector4(bx,by,bx,by);
            var sprite=Sprite.Create(texture,new Rect(0,0,width,height),
                new Vector2(.5f,.5f),100f,0,SpriteMeshType.FullRect,border);
            sprite.name=name;
            return sprite;
        }
        private static float Noise(int x,int y)
        {
            uint h=(uint)x*374761393u+(uint)y*668265263u+0x9e3779b9u;
            h=(h^(h>>13))*1274126177u;
            return (h^(h>>16)&255u)/255f;
        }
        private static Color32 C(float r,float g,float b,float a=255)
            =>new Color32((byte)Mathf.Clamp(Mathf.RoundToInt(r),0,255),
                (byte)Mathf.Clamp(Mathf.RoundToInt(g),0,255),
                (byte)Mathf.Clamp(Mathf.RoundToInt(b),0,255),
                (byte)Mathf.Clamp(Mathf.RoundToInt(a),0,255));

        private static Color32 SamplePanel(int x,int y)
        {
            int edge=Mathf.Min(Mathf.Min(x,255-x),Mathf.Min(y,255-y));
            if(edge<=1)return C(8,13,17,255);
            if(edge==2)return C(80,62,40,255);
            if(edge<=4)return C(124,94,56,244);
            if(edge==5)return C(57,52,42,253);
            float top=y/255f;
            float n=(Noise(x,y)-.5f)*5f;
            float vignette=Mathf.Pow(Mathf.Abs(x-128)/128f,1.5f)*4f;
            float c=4.5f*top;
            if(edge<9)return C(24,31,38,250);
            // Tiny edge flourishes instead of repeated ornament around every button.
            bool corner=(x<16||x>239)&&(y<16||y>239);
            if(corner&&edge<12&&(x+y)%5<2)return C(153,116,63,253);
            return C(17+c+n-vignette,25+c+n-vignette,32+c+n-vignette,248);
        }
        private static Color32 SampleButton(int x,int y,bool important)
        {
            float cx=x-127.5f,cy=y-35.5f;
            float qx=Mathf.Abs(cx)-122f,qy=Mathf.Abs(cy)-30f;
            float dist=Mathf.Sqrt(Mathf.Max(qx,0)*Mathf.Max(qx,0)+Mathf.Max(qy,0)*Mathf.Max(qy,0))
                +Mathf.Min(Mathf.Max(qx,qy),0);
            if(dist>5f)return C(0,0,0,0);
            if(dist>3.0f)return C(8,14,18,240);
            if(dist>1.8f)return C(important?163:99,important?124:84,important?65:52,252);
            if(dist>0.1f)return C(important?64:49,important?56:51,important?39:49,246);
            float light=Mathf.Clamp01(y/71f);
            float n=(Noise(x,y)-.5f)*3f;
            return important
                ?C(51+10*light+n,42+8*light+n,36+7*light+n,250)
                :C(31+6*light+n,41+7*light+n,49+7*light+n,244);
        }
        private static Color32 SampleCrest(int x,int y)
        {
            float dx=x-63.5f,dy=y-63.5f,r=Mathf.Sqrt(dx*dx+dy*dy);
            if(r>61)return C(0,0,0,0);
            if(r>57)return C(12,20,25,0);
            if(r>54)return C(147,111,58,246);
            if(r>50)return C(32,35,33,254);
            if(r>46)return C(93,75,48,253);
            if(r>43)return C(17,27,34,252);
            // Central gatehouse icon, easily legible at 56–64 on a phone.
            int ax=Mathf.Abs(x-64);
            bool wall=ax<22&&y>35&&y<79;
            bool crenels=y>=79&&y<88&&ax<23&&((x/9)%2==0);
            bool roof=ax<15&&y>=87&&y<93;
            bool portal=ax<7&&y>35&&y<61;
            bool slit=ax>11&&ax<15&&y>60&&y<72;
            if(portal)return C(15,26,32,255);
            if(wall||crenels||roof)return C(185,157,103,254);
            if(slit)return C(24,38,43,255);
            if(ax<31&&y>28&&y<34)return C(132,98,53,247);
            return C(19,31,38,250);
        }
    }
}
