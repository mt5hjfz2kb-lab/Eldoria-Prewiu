using UnityEngine;
using UnityEngine.UI;

namespace Eldoria.HeroesUI
{
    /// <summary>
    /// Art-only, source-bound hero abilities. Uses the approved engraved icon and
    /// nine-slice assets, never writes game state or invents additional abilities.
    /// Every number, name and description comes from the host's existing HeroState.
    /// </summary>
    internal static class HeroesSkillCards
    {
        static readonly Color Gold = new Color(.88f, .70f, .40f, 1f);
        static readonly Color Ivory = new Color(.94f, .89f, .79f, 1f);
        static readonly Color Faded = new Color(.67f, .66f, .64f, 1f);
        static readonly Color Dark = new Color(.046f, .050f, .062f, .96f);
        static Font displayFont;
        static Font Font => displayFont ? displayFont :
            (displayFont = Resources.Load<Font>("EldoriaRetention/reference-serif") ??
                Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf"));

        static RectTransform Rect(string name, Transform parent, float x, float y, float w, float h)
        {
            var r = new GameObject(name, typeof(RectTransform)).GetComponent<RectTransform>();
            r.SetParent(parent, false);
            r.anchorMin = r.anchorMax = r.pivot = new Vector2(0f, 1f);
            r.anchoredPosition = new Vector2(x, -y);
            r.sizeDelta = new Vector2(Mathf.Max(1f,w), Mathf.Max(1f,h));
            return r;
        }

        static Image Surface(string name, Transform parent, float x, float y, float w, float h, Color color)
        {
            var r = Rect(name, parent, x, y, w, h);
            var img = r.gameObject.AddComponent<Image>();
            img.color = color;
            img.raycastTarget = false;
            return img;
        }

        static Text Label(string name, Transform parent, string value,
            float x, float y, float w, float h, int size, TextAnchor anchor, Color color)
        {
            var r = Rect(name, parent, x, y, w, h);
            var t = r.gameObject.AddComponent<Text>();
            t.font = Font;
            t.text = value ?? string.Empty;
            t.color = color;
            t.alignment = anchor;
            t.fontSize = size;
            t.horizontalOverflow = HorizontalWrapMode.Wrap;
            t.verticalOverflow = VerticalWrapMode.Truncate;
            t.resizeTextForBestFit = true;
            t.resizeTextMinSize = Mathf.Min(size, 13);
            t.resizeTextMaxSize = size;
            t.raycastTarget = false;
            return t;
        }

        static RectTransform Card(string name, RectTransform parent,
            float x, float y, float w, float h, Sprite nineSlice, bool active)
        {
            var root = Rect(name, parent, x, y, w, h);
            Surface("Skill / translucent painted enamel", root, 0f, 0f, w, h,
                active ? new Color(.105f, .077f, .072f, .94f) : Dark);
            // Each layer is independent. Never bake state, text or the character
            // into the decorative image. The gilded border is genuinely sliced.
            Surface("Skill / inner paper glaze", root, 7f, 7f, w-14f, h-14f,
                active ? new Color(.22f, .15f, .11f, .35f) :
                    new Color(.13f, .13f, .14f, .27f));
            Surface("Skill / luminous top engraving", root, 11f, 8f, w-22f, 1.5f,
                active ? Gold : new Color(.45f,.40f,.34f,.70f));
            Surface("Skill / secondary baseline", root, 11f, h-10f, w-22f, 1.5f,
                new Color(.66f,.48f,.30f,active?.50f:.24f));
            if(nineSlice)
            {
                var art = Surface("Skill / independent nine-slice frame", root,
                    0f, 0f, w, h, active ? Color.white :
                    new Color(.68f,.68f,.66f,.87f));
                art.sprite = nineSlice;
                art.type = Image.Type.Sliced;
                art.fillCenter = false;
                art.pixelsPerUnitMultiplier = w<270f ? 2.2f : 1.4f;
            }
            // Subtle topological ornament; intentionally not an opaque full
            // rectangular overlay or a pasted screenshot.
            for (int side=0;side<2;side++)
            {
                float cx=side==0?13f:w-22f;
                Surface("Skill / chased top corner",root,cx,12f,9f,2f,
                    new Color(.88f,.67f,.35f,active?.88f:.46f));
                Surface("Skill / chased side corner",root,
                    side==0?13f:w-15f,12f,2f,9f,
                    new Color(.88f,.67f,.35f,active?.88f:.46f));
            }
            return root;
        }

        static void Sigil(RectTransform card, float x, float y, float size,
            Sprite approvedSymbol, bool active, bool locked)
        {
            var recess=Surface("Skill / heraldic icon recess",card,x,y,size,size,
                active?new Color(.30f,.22f,.13f,.91f):
                    new Color(.18f,.18f,.19f,.92f));
            recess.rectTransform.localRotation=Quaternion.Euler(0f,0f,0f);
            Surface("Skill / gold icon left inset",card,x+4f,y+4f,2f,size-8f,
                active?Gold:new Color(.42f,.41f,.39f,.72f));
            Surface("Skill / gold icon top inset",card,x+4f,y+4f,size-8f,2f,
                active?Gold:new Color(.42f,.41f,.39f,.72f));
            if(approvedSymbol)
            {
                var icon=Surface("Skill / approved reference icon sprite",
                    card,x+size*.18f,y+size*.18f,size*.64f,size*.64f,
                    active?new Color(.97f,.85f,.62f,1f):
                        new Color(.63f,.62f,.59f,.88f));
                icon.sprite=approvedSymbol;
                icon.preserveAspect=true;
            }
            if(locked)
            {
                float s=size*.24f;
                var c=new Color(.81f,.77f,.67f,.96f);
                Surface("Skill / lock shackle",card,x+size*.50f-s*.5f,
                    y+size*.31f,s,s*.75f,c);
                Surface("Skill / lock inset",card,x+size*.50f-s*.31f,
                    y+size*.38f,s*.62f,s*.52f,new Color(.16f,.16f,.17f,1f));
                Surface("Skill / lock body",card,x+size*.50f-s*.72f,
                    y+size*.49f,s*1.44f,s*.90f,c);
            }
        }

        // The only active ability is bound to the real host. The optional two
        // decorative reserved cards carry no IDs, button, level or gameplay.
        // Small mobile layouts hide reserves to prioritize actual description.
        internal static bool ShowReserves(float width,float height)
            => width>=290f && height>=345f;

        internal static void Build(RectTransform pane, string heroId, string abilityName,
            string description, float width, float height, Sprite frame, Sprite skillIcon)
        {
            float margin=Mathf.Clamp(width*.028f,7f,18f);
            float usableW=Mathf.Max(80f,width-2f*margin);
            bool compact=height<305f || width<290f;
            float titleH=compact?32f:48f;
            Label("Skills / art-directed heading",pane,"HABILIDADES",
                margin,margin,usableW,titleH,compact?19:26,
                TextAnchor.MiddleLeft,Gold);
            Surface("Skills / chapter divider",pane,margin,margin+titleH,
                usableW,1.5f,new Color(.82f,.62f,.34f,.86f));

            float top=margin+titleH+9f;
            float available=Mathf.Max(70f,height-top-margin);
            bool reserves=ShowReserves(width,height);
            float gap=reserves?Mathf.Clamp(height*.02f,7f,14f):0f;
            float reserveH=reserves?Mathf.Clamp(available*.22f,76f,125f):0f;
            float activeH=available-(reserves?2f*reserveH+2f*gap:0f);
            if(reserves && activeH<140f)
            {
                reserves=false; reserveH=0f;gap=0f;activeH=available;
            }
            var primary=Card("Skills / only real skill card",pane,
                margin,top,usableW,activeH,frame,true);
            float iconSize=Mathf.Clamp(Mathf.Min(usableW*.17f,activeH*.39f),32f,70f);
            Sigil(primary,12f,12f,iconSize,skillIcon,true,false);
            float textX=iconSize+23f;
            float titleW=Mathf.Max(40f,usableW-textX-13f);
            Label("Skills / actual ability type",primary,"HABILIDAD ACTIVA",
                textX,12f,titleW,21f,compact?13:15,TextAnchor.MiddleLeft,Gold);
            bool hasAbility=!string.IsNullOrWhiteSpace(abilityName);
            Label("Skills / actual ability name",primary,
                hasAbility?abilityName:"SIN HABILIDAD REGISTRADA",
                textX,34f,titleW,Mathf.Max(25f,iconSize-14f),
                compact?17:24,TextAnchor.MiddleLeft,Ivory);
            float descriptionY=Mathf.Max(iconSize+22f,compact?78f:99f);
            float descriptionH=Mathf.Max(20f,activeH-descriptionY-16f);
            Surface("Skills / manuscript description rule",primary,13f,
                descriptionY-6f,usableW-26f,1f,
                new Color(.79f,.62f,.40f,.48f));
            Description(primary,description??string.Empty,
                14f,descriptionY,usableW-28f,descriptionH,
                compact?18:19);
            // Subtle hero accent, not a fake ability power, tier or cooldown.
            Surface("Skills / character identity edge",primary,7f,
                18f,2f,Mathf.Max(10f,activeH-36f),
                heroId=="lyra"?new Color(.45f,.69f,.62f,.78f):
                    new Color(.88f,.68f,.40f,.78f));

            if(!reserves)return;
            for(int i=0;i<2;i++)
            {
                float y=top+activeH+gap+i*(reserveH+gap);
                var reserved=Card("Skills / future slot unavailable "+i,pane,
                    margin,y,usableW,reserveH,frame,false);
                float lockSize=Mathf.Clamp(reserveH*.58f,36f,62f);
                Sigil(reserved,14f,(reserveH-lockSize)*.5f,
                    lockSize,skillIcon,false,true);
                float tx=lockSize+29f;
                float tw=Mathf.Max(45f,usableW-tx-14f);
                Label("Skills / future state",reserved,"POR DESCUBRIR",
                    tx,Mathf.Max(4f,reserveH*.18f),tw,reserveH*.36f,
                    21,TextAnchor.MiddleLeft,Faded);
                Label("Skills / no unapproved mechanics",reserved,"BLOQUEADA",
                    tx,reserveH*.54f,tw,reserveH*.28f,16,
                    TextAnchor.MiddleLeft,new Color(.59f,.55f,.49f));
            }
        }

        static void Description(RectTransform card,string description,
            float x,float y,float w,float h,int fontSize)
        {
            var viewport=Rect("Skills / scroll viewport",card,x,y,w,h);
            var image=viewport.gameObject.AddComponent<Image>();
            image.color=new Color(.0f,.0f,.0f,.001f);
            image.raycastTarget=true; // ScrollRect receives mobile drag/touch.
            viewport.gameObject.AddComponent<RectMask2D>();
            var scroll=viewport.gameObject.AddComponent<ScrollRect>();
            var body=Rect("Skills / source-bound description content",
                viewport,0f,0f,w,h);
            var label=Label("Canonical hero ability description",body,
                description,0f,0f,w,h,fontSize,
                TextAnchor.UpperLeft,Ivory);
            label.resizeTextForBestFit=false;
            label.horizontalOverflow=HorizontalWrapMode.Wrap;
            label.verticalOverflow=VerticalWrapMode.Overflow;
            float required=Mathf.Max(h,label.preferredHeight+12f);
            body.sizeDelta=new Vector2(w,required);
            label.rectTransform.sizeDelta=new Vector2(w,required);
            scroll.viewport=viewport;
            scroll.content=body;
            scroll.horizontal=false;
            scroll.vertical=true;
            scroll.movementType=ScrollRect.MovementType.Clamped;
            scroll.scrollSensitivity=25f;
        }
    }
}
