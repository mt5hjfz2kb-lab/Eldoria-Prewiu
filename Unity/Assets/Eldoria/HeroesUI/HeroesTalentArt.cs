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
        // Soft-edged authored heraldic silhouettes: these motifs are atmosphere,
        // NOT new talents. No gameplay icons or numerical stats are baked here.
        static float Stroke(float x,float y,float ax,float ay,
            float bx,float by,float thickness)
        {
            float dx=bx-ax,dy=by-ay;
            float t=Sat(((x-ax)*dx+(y-ay)*dy)/(dx*dx+dy*dy+.00001f));
            float dist=Mathf.Sqrt(Mathf.Pow(x-(ax+t*dx),2f)+
                Mathf.Pow(y-(ay+t*dy),2f));
            return 1f-Smooth(thickness*.64f,thickness,dist);
        }
        static float RectShape(float x,float y,float minX,float minY,
            float maxX,float maxY,float softness)
        {
            return Smooth(minX-softness,minX+softness,x)*
                (1f-Smooth(maxX-softness,maxX+softness,x))*
                Smooth(minY-softness,minY+softness,y)*
                (1f-Smooth(maxY-softness,maxY+softness,y));
        }
        static float Emblem(int kind,int rank,float x,float y)
        {
            float g=0f;
            if(kind==0) // engraved strike motifs
            {
                if(rank==0){
                    g=Mathf.Max(Stroke(x,y,-.25f,-.25f,.22f,.28f,.055f),
                        Stroke(x,y,.25f,-.25f,-.22f,.28f,.055f));
                    g=Mathf.Max(g,Stroke(x,y,-.30f,-.14f,-.13f,-.25f,.05f));
                }else if(rank==1){
                    g=Mathf.Max(Stroke(x,y,-.24f,-.22f,.22f,.25f,.049f),
                        Stroke(x,y,.24f,-.22f,-.22f,.25f,.049f));
                    g=Mathf.Max(g,Stroke(x,y,-.05f,.04f,.05f,.04f,.08f));
                }else{
                    for(int a=0;a<5;a++){
                        float theta=(a*72f+90f)*Mathf.Deg2Rad;
                        g=Mathf.Max(g,Stroke(x,y,0,0,
                            .33f*Mathf.Cos(theta),.33f*Mathf.Sin(theta),.060f));
                    }
                }
            }else if(kind==1) // shield / breastplate / battlement
            {
                if(rank==0){
                    float edge=.28f*(1f-Smooth(-.1f,.38f,y));
                    float shield=1f-Smooth(.034f,.075f,
                        Mathf.Abs(Mathf.Abs(x)-edge));
                    g=Mathf.Max(shield*RectShape(x,y,-.38f,-.34f,.38f,.35f,.06f),
                        Stroke(x,y,0,.20f,0,-.18f,.053f));
                    g=Mathf.Max(g,Stroke(x,y,-.25f,.32f,.25f,.32f,.055f));
                }else if(rank==1){
                    g=Mathf.Max(Stroke(x,y,-.29f,.21f,-.12f,-.20f,.066f),
                        Stroke(x,y,.29f,.21f,.12f,-.20f,.066f));
                    g=Mathf.Max(g,Stroke(x,y,-.17f,-.23f,.17f,-.23f,.064f));
                    g=Mathf.Max(g,Stroke(x,y,-.27f,.20f,.27f,.20f,.065f));
                }else{
                    g=Mathf.Max(RectShape(x,y,-.22f,-.23f,.22f,.08f,.03f),
                        RectShape(x,y,-.31f,.05f,.31f,.13f,.03f));
                    for(int i=-1;i<=1;i++)
                        g=Mathf.Max(g,RectShape(x,y,i*.18f-.055f,.13f,
                            i*.18f+.055f,.30f,.02f));
                }
            }else{ // radiance / protection / flowering
                if(rank==1){
                    g=Mathf.Max(RectShape(x,y,-.075f,-.29f,.075f,.29f,.025f),
                        RectShape(x,y,-.29f,-.075f,.29f,.075f,.025f));
                }else{
                    int petals=rank==0?6:8;
                    for(int i=0;i<petals;i++){
                        float angle=2f*Mathf.PI*i/petals;
                        float px=.24f*Mathf.Cos(angle),py=.24f*Mathf.Sin(angle);
                        float rr=Mathf.Sqrt(Mathf.Pow((x-px)*1.25f,2f)+
                            Mathf.Pow((y-py)*1.25f,2f));
                        g=Mathf.Max(g,1f-Smooth(.095f,.155f,rr));
                    }
                    g=Mathf.Max(g,1f-Smooth(.035f,.11f,
                        Mathf.Sqrt(x*x+y*y)));
                }
            }
            return Sat(g);
        }
        static Color Background(int kind,float u,float v)
        {
            var pigment=Metal[kind];
            float grain=Noise(Mathf.Floor(u*310),Mathf.Floor(v*450),kind);
            float weave=.09f+.04f*grain+.045f*(1f-v);
            Color c=Blend(new Color(.019f,.024f,.037f),pigment,weave);
            // Inlaid tapering vertical columns and illuminated arch tracery.
            float edge=Mathf.Min(u,1f-u);
            float side=Ring(edge,.078f,.014f)+
                Ring(edge,.117f,.006f);
            float vaulted=Ring(Mathf.Sqrt(Mathf.Pow((u-.5f)*1.18f,2f)+
                Mathf.Pow((v-.63f)*.85f,2f)),.51f,.021f);
            c=Blend(c,pigment,side*.22f+vaulted*.14f);
            if(kind==0)
            {
                // Emberlit wall silhouettes and faded heraldic strokes.
                float battlement=RectShape(u,v,.17f,.05f,.83f,.24f,.02f);
                float tower=RectShape(u,v,.60f,.14f,.76f,.46f,.02f);
                float flames=Noise(Mathf.Floor(u*24),Mathf.Floor(v*44),1f);
                c=Blend(c,new Color(.31f,.10f,.058f),
                    battlement*.23f+tower*.20f+flames*.024f);
                for(int i=0;i<6;i++){
                    float ax=.13f+i*.14f;
                    float angle=(i*3f+1f)*.61f;
                    float stroke=Stroke(u,v,ax,.26f,
                        ax+.17f*Mathf.Cos(angle),.52f+.08f*i,.008f);
                    c=Blend(c,new Color(.53f,.24f,.09f),stroke*.30f);
                }
            }else if(kind==1)
            {
                float walls=RectShape(u,v,.1f,.07f,.9f,.35f,.02f);
                float keep=RectShape(u,v,.35f,.18f,.65f,.55f,.02f);
                float tower=RectShape(u,v,.17f,.15f,.33f,.44f,.02f)+
                    RectShape(u,v,.69f,.15f,.84f,.48f,.02f);
                float teeth=0f;
                for(int i=0;i<7;i++)
                    teeth=Mathf.Max(teeth,RectShape(u,v,
                        .17f+i*.103f,.33f,.225f+i*.103f,.39f,.015f));
                c=Blend(c,new Color(.095f,.24f,.32f),
                    walls*.33f+keep*.30f+tower*.32f+teeth*.25f);
                float mist=Mathf.Exp(-Mathf.Pow((v-.52f)*2.3f,2f));
                c=Blend(c,new Color(.30f,.42f,.49f),mist*.09f);
            }else
            {
                // Sanctuary canopy with gold dust, tree branches and sunlight.
                float trunk=Stroke(u,v,.18f,.03f,.27f,.88f,.022f);
                trunk=Mathf.Max(trunk,Stroke(u,v,.27f,.42f,.56f,.86f,.014f));
                trunk=Mathf.Max(trunk,Stroke(u,v,.25f,.54f,.08f,.93f,.015f));
                c=Blend(c,new Color(.35f,.22f,.11f),trunk*.34f);
                float foliage=0f;
                for(int i=0;i<13;i++){
                    float xx=Noise(i,4,4),yy=.63f+Noise(i,5,5)*.32f;
                    float radius=.10f+Noise(i,6,6)*.095f;
                    float dist=Mathf.Sqrt(Mathf.Pow(u-xx,2f)+
                        Mathf.Pow(v-yy,2f));
                    foliage=Mathf.Max(foliage,
                        1f-Smooth(radius*.57f,radius,dist));
                }
                c=Blend(c,new Color(.30f,.30f,.12f),foliage*.19f);
                float rays=1f-Smooth(.006f,.035f,
                    Mathf.Abs(Frac(u*.43f+v*.22f)-.5f));
                c=Blend(c,new Color(.72f,.57f,.24f),rays*.08f);
            }
            float vignette=1f-Smooth(.02f,.16f,edge);
            c=Blend(c,new Color(.011f,.015f,.025f),vignette*.39f);
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
            float edge=Mathf.Min(Mathf.Min(u,1f-u),Mathf.Min(v,1f-v));
            float rim=1f-Smooth(.020f,.095f,edge);
            float middle=Mathf.Exp(-Mathf.Pow((u-.5f)*2.6f,2f));
            // Dark inlaid enamel maintains native ivory text contrast.
            Color c=Blend(new Color(.025f,.030f,.044f),Metal[kind],
                .10f+.08f*middle);
            c=Blend(c,new Color(.85f,.66f,.37f),rim*.67f);
            float incised=Ring(v,.15f,.020f)+Ring(v,.83f,.022f);
            c=Blend(c,new Color(.92f,.74f,.48f),incised*.55f);
            float wing=(1f-Smooth(.20f,.40f,Mathf.Abs(u-.5f)))*
                Ring(v,.51f+.18f*Mathf.Sin(u*16f),.025f);
            c=Blend(c,Metal[kind],wing*.53f);
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
            float ornament=.022f*Mathf.Cos(a*12f),edge=.88f+ornament;
            float outer=1f-Smooth(edge-.018f,edge+.023f,r);
            float halo=Ring(r,.83f,.085f);
            float goldRing=Ring(r,.72f,.046f);
            float fineRing=Ring(r,.61f,.018f);
            float shading=Sat(1.06f-r*.70f+.20f*dy);
            Color c=Blend(new Color(.016f,.022f,.032f),Metal[kind],
                .12f+.12f*shading);
            c=Blend(c,new Color(.94f,.73f,.40f),goldRing*.92f);
            c=Blend(c,Metal[kind],halo*.85f+fineRing*.65f);
            float glyph=Emblem(kind,rank,dx,dy);
            c=Blend(c,new Color(.93f,.81f,.59f),glyph*.85f);
            c.a=outer*Sat(.90f+goldRing*.30f);
            return c;
        }
        static Color Lock(float u,float v)
        {
            float x=u-.5f,y=v-.5f;
            float body=RectShape(x,y,-.24f,-.25f,.24f,.12f,.027f);
            float bow=Ring(Mathf.Sqrt(Mathf.Pow(x*1.15f,2f)+
                Mathf.Pow((y-.14f)*1.1f,2f)),.25f,.077f);
            bow*=Smooth(-.04f,.02f,y);
            float hole=1f-Smooth(.045f,.08f,
                Mathf.Sqrt(x*x+Mathf.Pow(y+.065f,2f)));
            float alpha=Sat(body+bow);
            Color c=Blend(new Color(.88f,.83f,.72f,alpha),
                new Color(.31f,.27f,.22f,alpha),Sat(-y*.30f));
            c=Blend(c,new Color(.10f,.09f,.08f,alpha),hole*.90f);
            c.a=alpha;
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
            int font=tiny?(trackW>140f?14:12):trackW<105f?12:trackW<170f?15:24;
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
                    float lockSize=Mathf.Clamp(node*.55f,16f,50f);
                    Image("Talent / visible authentic lock overlay",nodeRect,
                        (node-lockSize)*.5f,node*.29f,lockSize,lockSize,
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
                compactLabel?16:width<350f?19:25,Ivory);
            Text("Talents / secondary reserved-state explanation",plaque,
                "Aún no disponibles en esta etapa del reino.",
                7f,compactLabel?18f:noteH*.47f,
                labelW,compactLabel?19f:noteH*.45f,
                compactLabel?13:width<350f?14:18,
                new Color(.84f,.77f,.65f));
        }
    }
}
