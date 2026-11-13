using System;
using System.Collections.Generic;
using UnityEngine;

namespace Eldoria.BreachWatch
{
    /// <summary>
    /// First-party materials for the independent BreachWatch canvas.
    /// This is NOT a recolor of RoyalHUD/Forja; its authored surfaces, cuts,
    /// mineral structure, corrosion, rivets and embossing are generated once
    /// from this reproducible source and bound only to BreachWatch UI Images.
    /// </summary>
    internal static class BreachWatchMaterialLibrary
    {
        static readonly Dictionary<BreachWatchPlate.PlateType, Sprite> Cache =
            new Dictionary<BreachWatchPlate.PlateType, Sprite>();

        static int Hash(int x, int y, int seed)
        {
            unchecked
            {
                uint n = (uint)(x * 374761393 + y * 668265263 + seed * 1442695);
                n = (n ^ (n >> 13)) * 1274126177u;
                return (int)((n ^ (n >> 16)) & 0x7fffffff);
            }
        }

        static float Grain(int x, int y, int stride, int seed)
        {
            // Smooth lattice noise: real mineral modulation, not repeated lines.
            float u = x / (float)stride, v = y / (float)stride;
            int ix = Mathf.FloorToInt(u), iy = Mathf.FloorToInt(v);
            u -= ix; v -= iy;
            u = u * u * (3 - 2 * u);
            v = v * v * (3 - 2 * v);
            float a = (Hash(ix, iy, seed) % 1001) / 1000f;
            float b = (Hash(ix + 1, iy, seed) % 1001) / 1000f;
            float c = (Hash(ix, iy + 1, seed) % 1001) / 1000f;
            float d = (Hash(ix + 1, iy + 1, seed) % 1001) / 1000f;
            return Mathf.Lerp(Mathf.Lerp(a,b,u),Mathf.Lerp(c,d,u),v);
        }

        static Color32 Blend(Color32 a, Color32 b, float t)
        {
            t = Mathf.Clamp01(t);
            return new Color32(
                (byte)Mathf.RoundToInt(Mathf.Lerp(a.r,b.r,t)),
                (byte)Mathf.RoundToInt(Mathf.Lerp(a.g,b.g,t)),
                (byte)Mathf.RoundToInt(Mathf.Lerp(a.b,b.b,t)),
                (byte)Mathf.RoundToInt(Mathf.Lerp(a.a,b.a,t)));
        }
        static Color32 Tint(int r,int g,int b,int a=255) =>
            new Color32((byte)Mathf.Clamp(r,0,255),(byte)Mathf.Clamp(g,0,255),
                (byte)Mathf.Clamp(b,0,255),(byte)Mathf.Clamp(a,0,255));

        static void Pixel(Color32[] a,int w,int h,int x,int y,Color32 color,float opacity=1f)
        {
            if(x<0||x>=w||y<0||y>=h)return;
            int i=y*w+x;
            a[i]=Blend(a[i],color,opacity);
        }
        static void Stroke(Color32[] a,int w,int h,Vector2 from,Vector2 to,
                           Color32 color,float opacity,int radius=1)
        {
            var d=to-from;int steps=Mathf.Max(1,Mathf.CeilToInt(d.magnitude*1.4f));
            for(int i=0;i<=steps;i++)
            {
                Vector2 p=Vector2.Lerp(from,to,i/(float)steps);
                int ix=Mathf.RoundToInt(p.x), iy=Mathf.RoundToInt(p.y);
                for(int yy=-radius;yy<=radius;yy++)
                for(int xx=-radius;xx<=radius;xx++)
                {
                    float weight=1f-Mathf.Sqrt(xx*xx+yy*yy)/Mathf.Max(1,radius+1);
                    if(weight>0)Pixel(a,w,h,ix+xx,iy+yy,color,opacity*weight);
                }
            }
        }
        static void Rivet(Color32[] pixels,int width,int height,int cx,int cy,bool copper)
        {
            Color32 black=Tint(4,8,10),metal=copper?Tint(131,91,68):Tint(78,80,81);
            for(int y=-8;y<=8;y++)
            for(int x=-8;x<=8;x++)
            {
                float d=Mathf.Sqrt(x*x+y*y);
                if(d>7.5f)continue;
                Color32 tone=d>6?black:d>4.6?Tint(41,39,37):
                    x+y>1?Tint(19,23,26):metal;
                float fade=1f-Mathf.Max(0,d-6);
                Pixel(pixels,width,height,cx+x,cy+y,tone,fade);
            }
            Stroke(pixels,width,height,new Vector2(cx-3,cy-3),
                new Vector2(cx+3,cy+3),Tint(7,10,13),.8f,1);
            Pixel(pixels,width,height,cx-2,cy+2,Tint(191,148,103),.65f);
        }

        static Sprite Author(BreachWatchPlate.PlateType kind)
        {
            bool emblem=kind==BreachWatchPlate.PlateType.Selected || kind==BreachWatchPlate.PlateType.Navigation;
            bool resource=kind==BreachWatchPlate.PlateType.Resource;
            bool action=kind==BreachWatchPlate.PlateType.Action;
            bool inactive=kind==BreachWatchPlate.PlateType.Dangerous;
            bool fissure=kind==BreachWatchPlate.PlateType.Obsidian;
            int w=emblem?192:resource?384:action||inactive?560:768;
            int h=emblem?192:resource?132:action||inactive?176:384;
            int seed=((int)kind+1)*1199;
            var pixels=new Color32[w*h];
            int cut=emblem?29:resource?22:action?26:38;
            float maxD=Mathf.Min(w,h)*.5f;
            for(int y=0;y<h;y++)
            for(int x=0;x<w;x++)
            {
                // A restrained eight-sided silhouette, no default rectangular UI plate.
                if(x+y<cut || (w-1-x)+y<cut ||
                   x+(h-1-y)<cut || (w-1-x)+(h-1-y)<cut)
                {
                    pixels[y*w+x]=new Color32(0,0,0,0);
                    continue;
                }
                float diag= Mathf.Min(Mathf.Min(x+y-cut,(w-1-x)+y-cut),
                    Mathf.Min(x+(h-1-y)-cut,(w-1-x)+(h-1-y)-cut))*.70710678f;
                float d=Mathf.Min(Mathf.Min(x,w-1-x),Mathf.Min(y,h-1-y));
                d=Mathf.Min(d,diag);
                // Vary mineral hue at multiple physical grain sizes. A slight
                // diagonal directional key gives carved surfaces their volume.
                float coarse=Grain(x,y,49,seed);
                float medium=Grain(x,y,13,seed+8);
                float fine=(Hash(x,y,seed+17)%256)/255f;
                float marbling=.52f*coarse+.34f*medium+.14f*fine;
                float light=(y/(float)h)*.19f + ((w-x)/(float)w)*.075f;
                float mineral=(marbling-.5f)*35f;
                int red=24+(int)(mineral+light*30);
                int green=27+(int)(mineral+light*30);
                int blue=29+(int)(mineral+light*30);
                if(resource){red-=1;green-=2;blue-=2;}
                if(fissure){red+=2;green-=3;blue-=1;}
                if(action){red+=19;green+=7;blue-=5;}
                if(inactive){red+=8;green-=2;blue-=1;}

                if(d<2) pixels[y*w+x]=Tint(1,3,5);
                else if(d<4) pixels[y*w+x]=Tint(5,8,9);
                else if(d<7)
                {
                    int metal=(int)(medium*15f);
                    pixels[y*w+x]=Tint(55+metal,49+metal,46+metal);
                }
                else if(d<11)
                {
                    int oxidation=(int)(coarse*29);
                    // Burnished iron/copper border with a bevel highlight.
                    pixels[y*w+x]=Tint(94+oxidation+(int)(light*50),
                        68+oxidation/2,48+oxidation/3);
                }
                else if(d<15)
                    pixels[y*w+x]=Tint(13,16,18);
                else if(d<21)
                {
                    int edge=(int)(medium*15);
                    pixels[y*w+x]=Tint(44+edge,46+edge,47+edge);
                }
                else if(d<23)
                    pixels[y*w+x]=Tint(8,11,12);
                else
                {
                    // Slate/leather center, warmer only on the actual action.
                    if(action){red+=3;green-=1;blue-=3;}
                    pixels[y*w+x]=Tint(red,green,blue);
                }
                if(d>24 && d<maxD-4 && (Hash(x,y,seed+53)%251)==0)
                    pixels[y*w+x]=Blend(pixels[y*w+x],Tint(111,98,84),.21f);
            }

            // Fixed-corner mineral scars, hammered surface nicks and
            // actual rivet heads are not baked into nine-slice stretch areas.
            for(int corner=0;corner<4;corner++)
            {
                int px=corner%2==0?48:w-49;
                int py=corner<2?43:h-44;
                Rivet(pixels,w,h,px,py,kind==BreachWatchPlate.PlateType.Action);
                int sx=corner%2==0?1:-1, sy=corner<2?1:-1;
                var p=new Vector2(px+sx*18,py+sy*15);
                for(int segment=0;segment<3;segment++)
                {
                    var q=p+new Vector2(sx*(12+segment*4),sy*(8+segment*3));
                    Stroke(pixels,w,h,p+Vector2.down,q+Vector2.down,Tint(1,5,8),.62f,2);
                    Stroke(pixels,w,h,p,q,Tint(123,88,70),.42f,1);
                    p=q;
                }
            }

            // 2D chiselled edge joints. The center stays quiet for readable text.
            var inc=resource?44:emblem?55:78;
            for(int i=inc;i<w-inc;i+=inc)
            {
                int off=(Hash(i,11,seed)%9)-4;
                Stroke(pixels,w,h,new Vector2(i+off,h-14),
                    new Vector2(i+off+3,h-20),Tint(7,11,12),.62f,1);
                Stroke(pixels,w,h,new Vector2(i+off,12),
                    new Vector2(i+off-2,18),Tint(157,111,77),.42f,1);
            }

            if(emblem)
            {
                // A metalwork rondel nested in the hewn stone octagon.
                for(int i=0;i<100;i++)
                {
                    float t=i*Mathf.PI*2f/100f;
                    var a=new Vector2(w/2f+73*Mathf.Cos(t),h/2f+73*Mathf.Sin(t));
                    var b=new Vector2(w/2f+73*Mathf.Cos(t+.065f),h/2f+73*Mathf.Sin(t+.065f));
                    Stroke(pixels,w,h,a,b,Tint(140,98,67),.54f,1);
                }
            }
            if(fissure)
            {
                int center=w/2;
                var points=new[]{
                    new Vector2(center-20,h-23),new Vector2(center+8,h*.68f),
                    new Vector2(center-7,h*.52f),new Vector2(center+18,h*.29f),
                    new Vector2(center-12,22)
                };
                for(int i=1;i<points.Length;i++)
                {
                    Stroke(pixels,w,h,points[i-1],points[i],Tint(1,3,6),.7f,3);
                    Stroke(pixels,w,h,points[i-1]+Vector2.right,points[i]+Vector2.right,
                        Tint(156,69,48),.64f,1);
                }
            }
            var texture=new Texture2D(w,h,TextureFormat.RGBA32,false,false);
            texture.name="Vigilia / original painted "+kind;
            texture.wrapMode=TextureWrapMode.Clamp;
            texture.filterMode=FilterMode.Bilinear;
            texture.SetPixels32(pixels);texture.Apply(false,true);
            var border=emblem?new Vector4(45,45,45,45):
                resource?new Vector4(60,36,60,36):
                action||inactive?new Vector4(68,42,68,42):
                new Vector4(84,52,84,52);
            // Border in screen-UI units remains about 12-25 at 100 reference PPU.
            return Sprite.Create(texture,new Rect(0,0,w,h),new Vector2(.5f,.5f),360f,
                0,SpriteMeshType.FullRect,border);
        }

        public static Sprite For(BreachWatchPlate.PlateType type)
        {
            if(Cache.TryGetValue(type,out var sprite) && sprite) return sprite;
            // Original independently sourced art; no RoyalHUD/Forja textures.
            // Always fall back to the first-party mineral material if one
            // optional research asset is absent during a source-only build.
            string sourceName = type==BreachWatchPlate.PlateType.Navigation ||
                type==BreachWatchPlate.PlateType.Selected ? "nav_sculpted_medallion" :
                type==BreachWatchPlate.PlateType.Portrait ? "portrait_forged_iron" :
                type==BreachWatchPlate.PlateType.Banner ? "valoria_burgundy_banner" :
                (type==BreachWatchPlate.PlateType.Action ||
                 type==BreachWatchPlate.PlateType.Dangerous)
                    ? "action_antique_bronze" : null;
            if(!string.IsNullOrEmpty(sourceName))
            {
                sprite=Resources.Load<Sprite>("VigiliaArt/"+sourceName);
                if(sprite){Cache[type]=sprite;return sprite;}
            }
            sprite=Author(type);Cache[type]=sprite;return sprite;
        }
    }
}
