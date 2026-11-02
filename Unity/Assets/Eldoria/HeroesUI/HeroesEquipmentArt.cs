using System;
using UnityEngine;
using UnityEngine.UI;

namespace Eldoria.HeroesUI
{
    /// <summary>
    /// Four purely visual, locked equipment positions around the canonical 3D
    /// hero. No inventory entities, loadout, levels, costs, statistics, events,
    /// persistence or unlock conditions are created by this view.
    /// </summary>
    internal static class HeroesEquipmentArt
    {
        static readonly Color Gold = new Color(.88f, .69f, .39f, 1f);
        static readonly Color Parchment = new Color(.94f, .88f, .75f, 1f);
        static readonly Color Dim = new Color(.66f, .66f, .65f, 1f);
        static readonly string[] Positions = { "CABEZA", "TORSO", "ARMA", "PIES" };
        static readonly string[] Descriptions = {
            "Posición no disponible", "Posición no disponible",
            "Posición no disponible", "Posición no disponible"
        };
        static Sprite[] emblems;
        static Sprite metalBorder, smallLock;
        static Font font;
        static Font Serif => font ? font : (font =
            Resources.Load<Font>("EldoriaRetention/reference-serif") ??
            Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf"));

        static RectTransform Rect(string name, Transform parent, float x, float y, float w, float h)
        {
            var r = new GameObject(name, typeof(RectTransform)).GetComponent<RectTransform>();
            r.SetParent(parent, false);
            r.anchorMin = r.anchorMax = r.pivot = new Vector2(0, 1);
            r.anchoredPosition = new Vector2(x, -y);
            r.sizeDelta = new Vector2(Mathf.Max(1f,w), Mathf.Max(1f,h));
            return r;
        }

        static Image Surface(string name, Transform parent, float x, float y,
            float w, float h, Color tint, Sprite sprite = null, bool sliced = false)
        {
            var img = Rect(name,parent,x,y,w,h).gameObject.AddComponent<Image>();
            img.raycastTarget = false;
            img.color = tint;
            if (sprite)
            {
                img.sprite = sprite;
                img.type = sliced ? Image.Type.Sliced : Image.Type.Simple;
                if (sliced) img.fillCenter = false;
                else img.preserveAspect = true;
            }
            return img;
        }

        static Text Label(string name, Transform parent, string text, float x, float y,
            float w, float h, int size, Color color, TextAnchor align = TextAnchor.MiddleCenter)
        {
            var t = Rect(name,parent,x,y,w,h).gameObject.AddComponent<Text>();
            t.font = Serif;
            t.text = text;
            t.alignment = align;
            t.fontSize = size;
            t.color = color;
            t.horizontalOverflow = HorizontalWrapMode.Wrap;
            t.verticalOverflow = VerticalWrapMode.Truncate;
            t.resizeTextForBestFit = false;
            t.raycastTarget = false;
            return t;
        }

        static float Segment(float x, float y, float ax, float ay, float bx, float by)
        {
            float dx=bx-ax,dy=by-ay;
            float t=Mathf.Clamp01(((x-ax)*dx+(y-ay)*dy)/(dx*dx+dy*dy+.00001f));
            return Vector2.Distance(new Vector2(x,y),new Vector2(ax+t*dx,ay+t*dy));
        }

        // Mathf.SmoothStep(from,to,t) expects a normalized t, not an input
        // interval! Applying it directly to pixel distance made all four
        // category emblems almost fully opaque squares in genuine Unity shots.
        static float Fade(float from,float to,float value)
        {
            float t=Mathf.Clamp01((value-from)/Mathf.Max(.0001f,to-from));
            return t*t*(3f-2f*t);
        }

        static float Line(float x,float y,float ax,float ay,float bx,float by,float thickness)
            => 1f-Fade(thickness*.55f,thickness,Segment(x,y,ax,ay,bx,by));

        static Sprite DrawSprite(string name,int size,Func<float,float,float> alpha, Vector4 border)
        {
            var texture=new Texture2D(size,size,TextureFormat.RGBA32,false);
            texture.name="Equipamiento / "+name+" source texture";
            texture.filterMode=FilterMode.Bilinear;
            texture.wrapMode=TextureWrapMode.Clamp;
            var pixels=new Color32[size*size];
            for(int y=0;y<size;y++)
                for(int x=0;x<size;x++)
                {
                    float u=(x+.5f)/size, v=(y+.5f)/size;
                    float a=Mathf.Clamp01(alpha(u,v));
                    pixels[y*size+x] = new Color(1f,1f,1f,a);
                }
            texture.SetPixels32(pixels);
            texture.Apply(false,true);
            var sprite=Sprite.Create(texture,new Rect(0,0,size,size),
                new Vector2(.5f,.5f),100,0,SpriteMeshType.FullRect,border);
            sprite.name="Equipamiento / "+name;
            return sprite;
        }

        static float IconShape(int i,float x,float y)
        {
            float a=0f;
            if(i==0) // Engraved head-slot silhouette, not an equippable helmet.
            {
                for(int n=0;n<10;n++)
                {
                    float t=n/9f*Mathf.PI;
                    a=Mathf.Max(a,Line(x,y,.5f+.26f*Mathf.Cos(t),.44f+.29f*Mathf.Sin(t),
                        .5f+.26f*Mathf.Cos(t+.10f),.44f+.29f*Mathf.Sin(t+.10f),.035f));
                }
                a=Mathf.Max(a,Line(x,y,.20f,.43f,.80f,.43f,.035f));
                a=Mathf.Max(a,Line(x,y,.30f,.38f,.30f,.23f,.034f));
                a=Mathf.Max(a,Line(x,y,.70f,.38f,.70f,.23f,.034f));
            }
            else if(i==1) // Torso-slot heraldic armour outline.
            {
                float[] p={.28f,.81f,.39f,.70f,.61f,.70f,.72f,.81f,.83f,.65f,
                    .73f,.27f,.50f,.16f,.27f,.27f,.17f,.65f,.28f,.81f};
                for(int k=0;k<p.Length-2;k+=2)
                    a=Mathf.Max(a,Line(x,y,p[k],p[k+1],p[k+2],p[k+3],.035f));
                a=Mathf.Max(a,Line(x,y,.50f,.69f,.50f,.30f,.025f));
            }
            else if(i==2) // Sword-shaped inventory category mark, not an item.
            {
                a=Mathf.Max(a,Line(x,y,.30f,.27f,.70f,.76f,.052f));
                a=Mathf.Max(a,Line(x,y,.28f,.53f,.52f,.34f,.043f));
                a=Mathf.Max(a,Line(x,y,.27f,.28f,.22f,.22f,.060f));
                a=Mathf.Max(a,Line(x,y,.70f,.76f,.75f,.84f,.038f));
                a=Mathf.Max(a,Line(x,y,.75f,.84f,.64f,.80f,.034f));
            }
            else // Foot-slot engraved pair.
            {
                a=Mathf.Max(a,Line(x,y,.27f,.76f,.27f,.34f,.070f));
                a=Mathf.Max(a,Line(x,y,.27f,.34f,.46f,.27f,.065f));
                a=Mathf.Max(a,Line(x,y,.46f,.27f,.46f,.22f,.065f));
                a=Mathf.Max(a,Line(x,y,.58f,.74f,.58f,.35f,.070f));
                a=Mathf.Max(a,Line(x,y,.58f,.35f,.77f,.28f,.065f));
                a=Mathf.Max(a,Line(x,y,.77f,.28f,.77f,.22f,.065f));
            }
            return Mathf.Clamp01(a);
        }

        static void EnsureSprites()
        {
            if(emblems!=null && metalBorder && smallLock)return;
            metalBorder=DrawSprite("independent engraved nine-slice border",96,
                (x,y)=>{
                    float edge=Mathf.Min(Mathf.Min(x,1-x),Mathf.Min(y,1-y));
                    float band=1f-Fade(.065f,.115f,edge);
                    float inner=1f-Fade(.155f,.19f,edge);
                    float corners=(Mathf.Abs(x-.5f)>.35f && Mathf.Abs(y-.5f)>.35f)?1f:0f;
                    return Mathf.Clamp01(band*.90f+inner*.24f+corners*.25f);
                },new Vector4(19,19,19,19));
            emblems=new Sprite[4];
            for(int n=0;n<4;n++)
            {
                int index=n;
                emblems[n]=DrawSprite("original slot engraving "+index,96,
                    (x,y)=>IconShape(index,x,y),Vector4.zero);
            }
            smallLock=DrawSprite("small locked-state emblem",64,
                (x,y)=>{
                    float a=0f;
                    for(int n=0;n<9;n++)
                    {
                        float angle=Mathf.PI*n/8f;
                        a=Mathf.Max(a,Line(x,y,.5f+.18f*Mathf.Cos(angle),
                            .48f+.22f*Mathf.Sin(angle),
                            .5f+.18f*Mathf.Cos(angle+.12f),
                            .48f+.22f*Mathf.Sin(angle+.12f),.053f));
                    }
                    if(x>.27f&&x<.73f&&y>.15f&&y<.50f)a=1f;
                    if(x>.45f&&x<.55f&&y>.26f&&y<.40f)a=.20f;
                    return a;
                },Vector4.zero);
        }

        /// <summary>
        /// The only character is the already-rendered canonical hero underneath.
        /// The four ornate overlays are read-only and contain NO buttons.
        /// </summary>
        internal static void BuildSlots(RectTransform stage,float width,float height,Sprite approvedFrame)
        {
            EnsureSprites();
            float size=Mathf.Clamp(Mathf.Min(width*.248f,height*.29f),51f,104f);
            float inset=Mathf.Max(4f,width*.018f);
            float left=inset,right=width-inset-size;
            float top=Mathf.Max(11f,height*.125f);
            float lower=Mathf.Min(height-size-43f,Mathf.Max(top+size+8f,height*.54f));
            if(lower<top+size+5f)lower=top+size+5f;
            float[] x={left,right,left,right}, y={top,top,lower,lower};
            for(int i=0;i<4;i++)
            {
                var slot=Rect("Equipment / reserved slot "+i,stage,x[i],y[i],size,size);
                Surface("Equipment / semi-transparent hammered recess",slot,1,1,
                    size-2,size-2,new Color(.055f,.067f,.082f,.87f));
                Surface("Equipment / thin interior brass trace",slot,5f,5f,size-10f,size-10f,
                    new Color(.91f,.66f,.31f,.27f),metalBorder,true);
                var frame=Surface("Equipment / nine-slice ornamental frame",slot,0,0,
                    size,size,new Color(.84f,.68f,.45f,.96f),
                    approvedFrame?approvedFrame:metalBorder,true);
                frame.pixelsPerUnitMultiplier=size<75f?2.3f:1.7f;
                Surface("Equipment / original category icon",slot,size*.23f,size*.10f,
                    size*.54f,size*.53f,new Color(.90f,.76f,.55f,.87f),emblems[i]);
                Surface("Equipment / engraved locked seal",slot,size*.69f,size*.07f,
                    size*.22f,size*.22f,new Color(.91f,.74f,.43f,.98f),smallLock);
                Surface("Equipment / label divider",slot,8f,size*.68f,size-16f,1.5f,
                    new Color(.81f,.58f,.28f,.71f));
                Label("Equipment / slot category "+i,slot,Positions[i],4f,size*.71f,
                    size-8f,size*.21f,size<62f?12:size<80f?13:14,Parchment);
            }
            // Separate ornament only; no model clipping or extra photograph.
            Surface("Equipment / vertical heraldic left line",stage,inset+size*.5f,
                top+size+7f,1.5f,Mathf.Max(1f,lower-top-size-14f),
                new Color(.78f,.57f,.34f,.47f));
            Surface("Equipment / vertical heraldic right line",stage,right+size*.5f,
                top+size+7f,1.5f,Mathf.Max(1f,lower-top-size-14f),
                new Color(.78f,.57f,.34f,.47f));
        }

        /// <summary>
        /// Context is visually independent from four stage slots; neither
        /// its labels nor ornamentation are baked into a full-screen backdrop.
        /// </summary>
        internal static void BuildContext(RectTransform pane,float width,float height)
        {
            EnsureSprites();
            float margin=Mathf.Clamp(width*.045f,11f,27f);
            float usable=Mathf.Max(80f,width-margin*2f);
            bool compact=height<270f;
            // A carved, limited-height nameplate ensures legibility over the
            // authentic brightly lit 3D hall. Never cover the whole pane with
            // one giant opaque rectangle, nor flatten the reference artwork.
            Surface("Equipment / small translucent information plaque",pane,
                margin-6f,5f,usable+12f,compact?122f:174f,
                new Color(.035f,.041f,.053f,.84f));
            Surface("Equipment / plaque engraved left edge",pane,
                margin-6f,5f,2f,compact?122f:174f,
                new Color(.79f,.59f,.34f,.83f));
            Label("Equipment / contextual heading",pane,"EQUIPAMIENTO",
                margin,12f,usable,compact?31f:48f,
                compact?21:28,Gold,TextAnchor.MiddleLeft);
            Surface("Equipment / section gold rule",pane,margin,compact?46f:62f,
                usable,1.5f,new Color(.91f,.67f,.33f,.85f));
            Label("Equipment / canonical locked status",pane,
                "CUATRO POSICIONES BLOQUEADAS",margin,compact?53f:76f,
                usable,compact?31f:36f,compact?16:17,Parchment,TextAnchor.MiddleLeft);
            Label("Equipment / absence of invented items",pane,
                "Aún no disponible en esta etapa del reino.",margin,
                compact?85f:117f,usable,compact?40f:58f,
                compact?15:17,Dim,TextAnchor.UpperLeft);

            float bottom=height-12f;
            if(compact)
            {
                // Small phone / short landscape: four clear category seals in
                // one row; no minuscule equipment statistics or empty giant pane.
                float start=Mathf.Max(133f,height*.67f);
                float gap=Mathf.Clamp(usable*.025f,4f,13f);
                float cell=(usable-gap*3f)/4f;
                float iconSize=Mathf.Min(27f,cell*.43f);
                for(int i=0;i<4;i++)
                {
                    float x=margin+i*(cell+gap);
                    var card=Rect("Equipment / context mini-category "+i,pane,
                        x,start,cell,Mathf.Max(22f,bottom-start));
                    Surface("Equipment / mini category symbol",card,
                        (cell-iconSize)*.5f,2f,iconSize,iconSize,
                        new Color(.80f,.72f,.59f,.94f),emblems[i]);
                    Label("Equipment / mini category text",card,Positions[i],
                        0f,iconSize+4f,cell,Mathf.Max(17f,bottom-start-iconSize-4f),
                        cell<68f?11:13,Parchment,TextAnchor.MiddleCenter);
                }
            }
            else
            {
                float listTop=Mathf.Max(182f,height*.33f);
                float remaining=Mathf.Max(78f,bottom-listTop);
                float gap=Mathf.Clamp(remaining*.025f,5f,13f);
                float cellH=Mathf.Min(74f,(remaining-gap*3f)/4f);
                for(int i=0;i<4;i++)
                {
                    float y=listTop+i*(cellH+gap);
                    var plate=Rect("Equipment / context locked row "+i,pane,
                        margin,y,usable,cellH);
                    Surface("Equipment / brushed translucent row",plate,0,0,usable,cellH,
                        new Color(.055f,.061f,.072f,.87f));
                    Surface("Equipment / row ornament left",plate,0f,0f,2.5f,cellH,
                        new Color(.79f,.60f,.35f,.89f));
                    Surface("Equipment / row category icon",plate,8f,6f,
                        Mathf.Max(22f,cellH-12f),Mathf.Max(22f,cellH-12f),
                        new Color(.77f,.73f,.65f,.94f),emblems[i]);
                    float tx=Mathf.Max(36f,cellH+4f);
                    Label("Equipment / context category "+i,plate,Positions[i],tx,
                        0f,Mathf.Max(30f,usable-tx-10f),cellH*.52f,
                        cellH<47f?12:18,Parchment,TextAnchor.MiddleLeft);
                    Label("Equipment / context lock text "+i,plate,
                        Descriptions[i],tx,cellH*.49f,Mathf.Max(30f,usable-tx-10f),
                        cellH*.45f,cellH<47f?11:14,Dim,TextAnchor.MiddleLeft);
                }
            }
        }
    }
}
