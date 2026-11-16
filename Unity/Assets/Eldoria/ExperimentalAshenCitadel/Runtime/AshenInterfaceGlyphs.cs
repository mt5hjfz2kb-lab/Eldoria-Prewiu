// Ashen Citadel native iconography: 8 recognisable role-specific glyphs.
// Functional button actions are unchanged; icons are drawn procedurally at
// runtime from authored geometric strokes and are not unlicensed imported art.
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Eldoria.ExperimentalAshenCitadel
{
    internal static class AshenInterfaceGlyphs
    {
        private static readonly Dictionary<string,Sprite> Cache=new Dictionary<string,Sprite>();
        internal static Sprite ForControl(string controlName)
        {
            string key=null;
            if(controlName.StartsWith("Nav · BASTIÓN"))key="castle";
            else if(controlName.StartsWith("Nav · EDIFICIOS"))key="workshop";
            else if(controlName.StartsWith("Nav · EJÉRCITO"))key="army";
            else if(controlName.StartsWith("Nav · MUNDO"))key="compass";
            else if(controlName.StartsWith("Access · CORREO"))key="mail";
            else if(controlName.StartsWith("Access · BOLSA"))key="bag";
            else if(controlName.StartsWith("Access · HÉROES"))key="hero";
            else if(controlName.StartsWith("Access · RETOS"))key="quest";
            if(key==null)return null;
            if(Cache.TryGetValue(key,out var cached))return cached;
            const int size=64;
            var tex=new Texture2D(size,size,TextureFormat.RGBA32,false,false)
            {name="Ashen glyph "+key,wrapMode=TextureWrapMode.Clamp,filterMode=FilterMode.Bilinear};
            var pixels=new Color32[size*size];
            for(int y=0;y<size;y++)for(int x=0;x<size;x++)
            {
                float px=(x-31.5f)/27f,py=(y-31.5f)/27f;
                float dist=GeometryDistance(key,px,py);
                float coverage=1f-Mathf.SmoothStep(.0f,.08f,dist);
                pixels[y*size+x]=new Color32(190,154,96,
                    (byte)Mathf.RoundToInt(Mathf.Clamp01(coverage)*250f));
            }
            tex.SetPixels32(pixels);tex.Apply(false,true);
            var sprite=Sprite.Create(tex,new Rect(0,0,size,size),new Vector2(.5f,.5f),100f);
            sprite.name="Ashen glyph "+key;Cache[key]=sprite;return sprite;
        }
        private static float Box(float x,float y,float cx,float cy,float hx,float hy)
        {
            float dx=Mathf.Abs(x-cx)-hx,dy=Mathf.Abs(y-cy)-hy;
            return Mathf.Sqrt(Mathf.Max(0,dx)*Mathf.Max(0,dx)+Mathf.Max(0,dy)*Mathf.Max(0,dy))
                 +Mathf.Min(Mathf.Max(dx,dy),0f);
        }
        private static float Stroke(float x,float y,float ax,float ay,float bx,float by,float radius)
        {
            float dx=bx-ax,dy=by-ay;
            float t=Mathf.Clamp01(((x-ax)*dx+(y-ay)*dy)/(dx*dx+dy*dy));
            return Mathf.Sqrt(Mathf.Pow(x-(ax+dx*t),2f)+Mathf.Pow(y-(ay+dy*t),2f))-radius;
        }
        private static float Circle(float x,float y,float r,float thickness)
            =>Mathf.Abs(Mathf.Sqrt(x*x+y*y)-r)-thickness;
        private static float Min(float a,float b)=>Mathf.Min(a,b);
        private static float GeometryDistance(string key,float x,float y)
        {
            float d=10f;
            switch(key)
            {
                case "castle":
                    d=Box(x,y,0,-.10f,.59f,.46f);
                    for(int i=-2;i<=2;i++)
                        d=Min(d,Box(x,y,i*.23f,.45f,.075f,.19f));
                    d=Mathf.Max(d,-Box(x,y,0,-.40f,.15f,.34f));
                    return d;
                case "workshop":
                    d=Stroke(x,y,-.74f,-.04f,0f,.61f,.075f);
                    d=Min(d,Stroke(x,y,0f,.61f,.78f,-.06f,.075f));
                    d=Min(d,Box(x,y,0f,-.35f,.59f,.36f));
                    d=Min(d,Box(x,y,-.24f,-.33f,.13f,.35f));
                    return d;
                case "army":
                    d=Stroke(x,y,-.66f,-.60f,.62f,.61f,.095f);
                    d=Min(d,Stroke(x,y,-.62f,.63f,.66f,-.58f,.095f));
                    d=Min(d,Circle(x,y,.12f,.07f));return d;
                case "compass":
                    d=Circle(x,y,.65f,.07f);
                    d=Min(d,Stroke(x,y,-.39f,-.39f,.39f,.39f,.06f));
                    d=Min(d,Stroke(x,y,-.39f,.39f,.39f,-.39f,.06f));
                    d=Min(d,Box(x,y,0f,0f,.10f,.10f));return d;
                case "mail":
                    d=Mathf.Abs(Box(x,y,0f,0f,.69f,.45f))-.065f;
                    d=Min(d,Stroke(x,y,-.66f,.34f,0f,-.16f,.065f));
                    d=Min(d,Stroke(x,y,0f,-.16f,.66f,.34f,.065f));return d;
                case "bag":
                    d=Circle(x,y+.20f,.54f,.10f);
                    d=Min(d,Box(x,y,0f,-.33f,.50f,.32f));
                    d=Min(d,Stroke(x,y,-.28f,.60f,.25f,.60f,.10f));return d;
                case "hero":
                    d=Stroke(x,y,-.65f,.54f,0f,.72f,.09f);
                    d=Min(d,Stroke(x,y,0f,.72f,.65f,.54f,.09f));
                    d=Min(d,Stroke(x,y,-.65f,.54f,-.52f,-.32f,.09f));
                    d=Min(d,Stroke(x,y,.65f,.54f,.52f,-.32f,.09f));
                    d=Min(d,Stroke(x,y,-.52f,-.32f,0f,-.72f,.09f));
                    d=Min(d,Stroke(x,y,.52f,-.32f,0f,-.72f,.09f));return d;
                case "quest":
                    d=Stroke(x,y,-.43f,-.67f,-.43f,.69f,.08f);
                    d=Min(d,Stroke(x,y,-.43f,.60f,.57f,.47f,.085f));
                    d=Min(d,Stroke(x,y,.57f,.47f,.28f,.07f,.085f));
                    d=Min(d,Stroke(x,y,.28f,.07f,-.43f,.22f,.085f));return d;
                default:return 10f;
            }
        }
    }
}
