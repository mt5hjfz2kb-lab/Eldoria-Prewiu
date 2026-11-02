using System;
using UnityEngine;
using UnityEngine.UI;

namespace Eldoria.HeroesUI
{
    /// <summary>
    /// Art-only three-path talent cabinet. All badges are inert and all nine buttons
    /// explicitly disabled. Text is native uGUI; no levels, bonuses or costs exist.
    /// Gold/ember/cobalt assets are generated as independent reusable sprites,
    /// with sliced metal edges rather than stretching flattened UI screenshots.
    /// </summary>
    internal static class HeroesTalentArt
    {
        static readonly Color Ivory = new Color(.96f,.89f,.75f,1f);
        static readonly Color[] Metal = {
            new Color(.91f,.38f,.18f), new Color(.29f,.69f,1f),
            new Color(.95f,.73f,.30f)
        };
        static Sprite[] panels, borders, headers, threads, seals;
        static Sprite[] nodes;
        static Sprite plate, padlock;
        static Font serif;
        static Font Serif => serif ? serif : (serif = Resources.Load<Font>(
            "EldoriaRetention/reference-serif") ??
            Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf"));

        static float Sat(float x) => Mathf.Clamp01(x);
        static float Smooth(float a,float b,float x) {
            float q=Sat((x-a)/(b-a)); return q*q*(3f-2f*q);
        }
        static float Frac(float x) => x-Mathf.Floor(x);
        static float Noise(float x,float y,float z) =>
            Frac(Mathf.Sin(x*127.1f+y*311.7f+z*74.7f)*43758.5453f);
        static float Ring(float r,float center,float width) =>
            1f-Smooth(width*.70f,width,Mathf.Abs(r-center));
        static Color Blend(Color a,Color b,float f) => Color.Lerp(a,b,Sat(f));
        static Sprite SpriteFrom(string name,int width,int height,
            Func<float,float,Color> shade,Vector4 border)
        {
            var texture=new Texture2D(width,height,TextureFormat.RGBA32,false);
            texture.name="Talentos / "+name+" texture";
            texture.filterMode=FilterMode.Bilinear;
            texture.wrapMode=TextureWrapMode.Clamp;
            var pixels=new Color32[width*height];
            for(int y=0;y<height;y++)
                for(int x=0;x<width;x++)
                    pixels[y*width+x]=(Color32)shade(
                        (x+.5f)/width,(y+.5f)/height);
            texture.SetPixels32(pixels);
            texture.Apply(false,true);
            var sprite=Sprite.Create(texture,new Rect(0,0,width,height),
                new Vector2(.5f,.5f),100,0,SpriteMeshType.FullRect,border);
            sprite.name="Talentos / "+name;
            return sprite;
        }
        static Color Background(int kind,float u,float v)
        {
            // Dark authored tapestry: concentric recessed metal, stone grain,
            // embossed threadwork and muted atmospheric accents.
            float edge=Mathf.Min(Mathf.Min(u,1f-u),Mathf.Min(v,1f-v));
            var baseInk=new Color(.030f,.033f,.043f,1f);
            var tint=Metal[kind];
            float grain=Noise(Mathf.Floor(u*240),Mathf.Floor(v*300),kind);
            float cloth=.09f+.06f*(1f-v)+.043f*grain;
            Color c=Blend(baseInk,tint,cloth);
            float moon=Mathf.Exp(-(Mathf.Pow((u-.60f)*1.2f,2)+Mathf.Pow((v-.72f)*1.5f,2))*8f);
            c=Blend(c,tint,.08f*moon);
            // Faded motifs are kept in the surface, distinct from live nodes.
            float arch=Mathf.Abs(u-.5f);
            float archBody=(1f-Smooth(.34f,.42f,arch))*
                (1f-Smooth(.73f,.9f,v));
            float archCrown=Ring(Mathf.Sqrt(Mathf.Pow((u-.5f)*1.6f,2)+
                Mathf.Pow((v-.48f)*1.10f,2)),.66f,.025f);
            c=Blend(c,new Color(.15f,.12f,.09f),archBody*.15f);
            c=Blend(c,tint,archCrown*.20f);
            // Fine filigree at both sides and an engraved ornamental spine.
            float thread=(Ring(u,.075f,.006f)+Ring(u,.925f,.006f))*.5f;
            float vine=(Mathf.Abs(Mathf.Sin(v*27f+u*9f))>.97f?1f:0f)
                * (1f-Smooth(.14f,.20f,Mathf.Min(u,1f-u)));
            c=Blend(c,tint,.20f*thread+.18f*vine);
            c=Blend(c,new Color(.010f,.013f,.020f),.35f*(1f-Smooth(.015f,.15f,edge)));
            return c;
        }
        static Color Border(float u,float v,Color tint)
        {
            float e=Mathf.Min(Mathf.Min(u,1f-u),Mathf.Min(v,1f-v));
            float shining=1f-Smooth(.018f,.026f,e);
            float inset=Ring(e,.045f,.010f);
            float bevel=Ring(e,.023f,.018f);
            Color c=new Color(.05f,.045f,.052f,0f);
            c=Blend(c,new Color(.95f,.77f,.45f,1f),
                shining*.75f+inset*.49f);
            c=Blend(c,tint,bevel*.85f);
            // The frame's entire middle remains transparent for live scenery.
            c.a=Sat(shining+inset*.72f+bevel*.66f);
            float corner=Mathf.Max(1f-Smooth(.08f,.2f,u),1f-Smooth(.8f,.92f,u))*
                Mathf.Max(1f-Smooth(.08f,.2f,v),1f-Smooth(.8f,.92f,v));
            c.a=Sat(c.a+corner*.40f);
            return c;
        }
        static Color Header(float u,float v,int kind)
        {
            float e=Mathf.Min(Mathf.Min(u,1f-u),Mathf.Min(v,1f-v));
            float t=1f-Smooth(.018f,.12f,e);
            float light=Mathf.Exp(-Mathf.Pow((u-.5f)*2.3f,2))*0.25f;
            Color c=Blend(new Color(.040f,.042f,.056f),Metal[kind],
                .17f+light);
            c=Blend(c,new Color(.89f,.70f,.37f),t*.75f);
            // Embossed side flourishes; no words baked in texture.
            float wave=Sat(.015f-Mathf.Abs(v-.5f-.17f*Mathf.Sin((u-.5f)*22f)));
            float wings=(1f-Smooth(.18f,.40f,Mathf.Abs(u-.5f)));
            c=Blend(c,Metal[kind],wave*wings*.65f);
            return c;
        }
        static Color Rail(float u,float v,int kind)
        {
            float line=1f-Smooth(.08f,.23f,Mathf.Abs(u-.5f));
            float diamond=Mathf.Abs(u-.5f)*1.2f+
                Mathf.Abs(Frac(v*4f)-.5f)*.55f;
            float jewel=1f-Smooth(.09f,.16f,diamond);
            float glow=1f-Smooth(.18f,.5f,Mathf.Abs(u-.5f));
            var c=Metal[kind];c.a=Sat(line*.80f+jewel+glow*.15f);
            return Blend(new Color(1f,.90f,.69f,c.a),c,.46f);
        }
        static Color Medallion(float u,float v,int kind,int rank)
        {
            float dx=(u-.5f)*2f,dy=(v-.5f)*2f;
            float r=Mathf.Sqrt(dx*dx+dy*dy),a=Mathf.Atan2(dy,dx);
            float spurs=.024f*Mathf.Cos(a*12f),edge=.88f+spurs;
            float outer=1f-Smooth(edge-.015f,edge+.016f,r);
            float halo=Ring(r,.84f,.10f);
            float golden=Ring(r,.73f,.042f);
            float inner=Ring(r,.63f,.016f);
            float shading=Sat(1.1f-r*.67f+.12f*dy);
            Color c=Blend(new Color(.02f,.026f,.036f),Metal[kind],
                .18f+.13f*shading);
            c=Blend(c,new Color(.97f,.79f,.43f),golden*.90f);
            c=Blend(c,Metal[kind],halo*.85f+inner*.70f);
            // Distinct rank/runic decorative motif: never claims mechanics.
            float spoke=Mathf.Abs(Mathf.Sin(a*(kind+rank+4)));
            float sigil=(1f-Smooth(.16f,.24f,r))+
                (1f-Smooth(.025f,.065f,spoke))*
                Smooth(.26f,.35f,r)*(1f-Smooth(.48f,.52f,r));
            c=Blend(c,new Color(.87f,.76f,.58f),sigil*.38f);
            c.a=outer*Sat(.95f+(halo+golden)*.18f);
            return c;
        }
        static Color Lock(float u,float v)
        {
            float dx=Mathf.Abs(u-.5f),dy=Mathf.Abs(v-.53f);
            float box=(1f-Smooth(.20f,.23f,dx))*
                (1f-Smooth(.12f,.17f,v-.46f))*
                Smooth(.45f,.50f,v);
            float shackle=Mathf.Sqrt(Mathf.Pow(dx*1.7f,2)+
                Mathf.Pow((v-.50f)*1.4f,2));
            float bow=Ring(shackle,.29f,.09f)*(1f-Smooth(.55f,.63f,v));
            float hole=(1f-Smooth(.04f,.07f,dx))*
                (1f-Smooth(.04f,.08f,Mathf.Abs(v-.68f)));
            var c=new Color(.86f,.82f,.72f,Sat(box+bow));
            c=Blend(c,new Color(.1f,.09f,.08f,1f),hole*.95f);
            return c;
        }
        static void Ensure()
        {
            if(panels!=null && panels[0])return;
            panels=new Sprite[3];borders=new Sprite[3];
            headers=new Sprite[3];threads=new Sprite[3];
            nodes=new Sprite[9];seals=new Sprite[3];
            for(int k=0;k<3;k++)
            {
                int path=k;
                panels[k]=SpriteFrom("painted parchment "+k,160,320,
                    (u,v)=>Background(path,u,v),Vector4.zero);
                borders[k]=SpriteFrom("chased nine-slice metal "+k,128,128,
                    (u,v)=>Border(u,v,Metal[path]),
                    new Vector4(24,24,24,24));
                headers[k]=SpriteFrom("enameled title plaque "+k,256,72,
                    (u,v)=>Header(u,v,path),
                    new Vector4(34,20,34,20));
                threads[k]=SpriteFrom("engraved connector "+k,32,128,
                    (u,v)=>Rail(u,v,path),Vector4.zero);
                seals[k]=SpriteFrom("header crest "+k,144,144,
                    (u,v)=>Medallion(u,v,path,0),Vector4.zero);
                for(int rank=0;rank<3;rank++)
                {
                    int rk=rank;
                    nodes[k*3+rank]=SpriteFrom("reserved node "+k+" "+rank,
                        160,160,(u,v)=>Medallion(u,v,path,rk),Vector4.zero);
                }
            }
            plate=SpriteFrom("unavailable brass cartouche",256,88,
                (u,v)=>Header(u,v,2),new Vector4(38,22,38,22));
            padlock=SpriteFrom("locked overlay",96,96,Lock,Vector4.zero);
        }
        static RectTransform Rect(string name,Transform parent,
            float x,float y,float w,float h)
        {
            var t=new GameObject(name,typeof(RectTransform)).GetComponent<RectTransform>();
            t.SetParent(parent,false);
            t.anchorMin=t.anchorMax=t.pivot=new Vector2(0f,1f);
            t.anchoredPosition=new Vector2(x,-y);
            t.sizeDelta=new Vector2(Mathf.Max(1,w),Mathf.Max(1,h));
            return t;
        }
        static Image Image(string name,Transform parent,float x,float y,
            float w,float h,Sprite sprite,Color color,bool sliced=false)
        {
            var obj=Rect(name,parent,x,y,w,h);
            var image=obj.gameObject.AddComponent<Image>();
            image.sprite=sprite;
            image.type=sliced?UnityEngine.UI.Image.Type.Sliced:
                UnityEngine.UI.Image.Type.Simple;
            image.color=color;
            image.raycastTarget=false;
            return image;
        }
        static Text Text(string name,Transform parent,string value,
            float x,float y,float w,float h,int font,Color color)
        {
            var rect=Rect(name,parent,x,y,w,h);
            var text=rect.gameObject.AddComponent<Text>();
            text.font=Serif;
            text.text=value;
            text.fontSize=font;
            text.color=color;
            text.alignment=TextAnchor.MiddleCenter;
            text.resizeTextForBestFit=false;
            text.horizontalOverflow=HorizontalWrapMode.Wrap;
            text.verticalOverflow=VerticalWrapMode.Truncate;
            text.raycastTarget=false;
            return text;
        }
        internal static void Build(RectTransform board,float width,float height,bool portrait)
        {
            Ensure();
            // The existing shared Canvas, lateral navigation, selection and
            // save bindings stay authoritative. No extra Canvas or model.
            bool tiny=height<280f;
            float outer=tiny?7f:12f;
            float gap=tiny?5f:Mathf.Clamp(width*.016f,7f,18f);
            float trackW=Mathf.Max(25f,(width-outer*2-2*gap)/3f);
            float noteH=tiny?39f:Mathf.Clamp(height*.13f,52f,92f);
            float trackH=Mathf.Max(80f,height-noteH-outer*2-6f);
            int font=tiny?12:trackW<105f?11:trackW<170f?15:24;
            string[] titles={"OFENSIVA","DEFENSA","SOPORTE"};
            for(int path=0;path<3;path++)
            {
                float left=outer+path*(trackW+gap);
                var track=Rect("Reserved talent path "+titles[path],board,
                    left,outer,trackW,trackH);
                Image("Talents / painted parchment and carved columns",track,
                    0,0,trackW,trackH,panels[path],Color.white);
                Image("Talents / ornate nine-slice structural border",track,
                    0,0,trackW,trackH,borders[path],Color.white,true);
                float headerH=tiny?30f:Mathf.Clamp(trackH*.16f,37f,82f);
                Image("Talents / embossed crest plaque",track,
                    4f,3f,trackW-8f,headerH,headers[path],Color.white,true);
                if(trackW>145f && trackH>300f)
                    Image("Talents / independent title medallion",track,
                        (trackW-51f)*.5f,4f,51f,51f,seals[path],
                        new Color(1f,1f,1f,.80f));
                Text("Reserved path name",track,titles[path],4f,3f,
                    trackW-8f,headerH,font,Ivory);
                float top=headerH+(tiny?7f:Mathf.Clamp(trackH*.035f,9f,26f));
                float node=Mathf.Min(tiny?28f:96f,
                    trackW*(tiny?.68f:.62f),
                    (trackH-top-14f)/3.55f);
                node=Mathf.Max(18f,node);
                float room=trackH-top-node-13f;
                float step=Mathf.Max(node+1f,room*.5f);
                if(top+2f*step+node>trackH-5f)
                    step=(trackH-5f-top-node)/2f;
                float center=trackW*.5f;
                Image("Talents / connected forged metallic trunk",track,
                    center-Mathf.Max(4f,node*.11f),top+node*.5f,
                    Mathf.Max(8f,node*.22f),Mathf.Max(2f,step*2f),
                    threads[path],new Color(1f,1f,1f,.85f));
                for(int rank=0;rank<3;rank++)
                {
                    float y=top+rank*step;
                    var nodeRect=Rect("Reserved talent node "+path+"-"+rank,
                        track,center-node*.5f,y,node,node);
                    // Nine historical UI button identities are retained for
                    // native verification, but NONE is actionable.
                    var disabled=nodeRect.gameObject.AddComponent<Button>();
                    disabled.interactable=false;
                    disabled.transition=Selectable.Transition.None;
                    Image("Talent / faint magic aura",nodeRect,
                        -node*.10f,-node*.10f,node*1.20f,node*1.20f,
                        nodes[path*3+rank],
                        new Color(Metal[path].r,Metal[path].g,
                            Metal[path].b,.25f));
                    Image("Talent / engraved metallic circular socket",nodeRect,
                        0,0,node,node,nodes[path*3+rank],Color.white);
                    float lockSize=Mathf.Clamp(node*.43f,12f,40f);
                    Image("Talent / visible authentic lock overlay",nodeRect,
                        (node-lockSize)*.5f,node*.39f,lockSize,lockSize,
                        padlock,new Color(.98f,.92f,.79f,1f));
                }
            }
            // This is an actual noninteractive locked state, not a purchase UI.
            float noteY=height-noteH-outer;
            var plaque=Rect("Talents / locked manuscript cartouche",board,
                outer+4f,noteY,width-2*outer-8f,noteH);
            Image("Talents / forged sliced plaque",plaque,0,0,
                width-2*outer-8f,noteH,plate,Color.white,true);
            float labelW=width-2*outer-20f;
            bool compactLabel=noteH<48f;
            Text("Reserved talent availability",plaque,
                "TALENTOS BLOQUEADOS",7f,0f,labelW,
                compactLabel?19f:noteH*.49f,
                compactLabel?15f:width<350f?17:25,Ivory);
            Text("Talents / secondary reserved-state explanation",plaque,
                "Aún no disponibles en esta etapa del reino.",
                7f,compactLabel?18f:noteH*.47f,
                labelW,compactLabel?19f:noteH*.45f,
                compactLabel?11:width<350f?12:18,
                new Color(.84f,.77f,.65f));
        }
    }
}
