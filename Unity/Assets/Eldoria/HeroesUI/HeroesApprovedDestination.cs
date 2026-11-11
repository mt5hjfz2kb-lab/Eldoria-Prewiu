using System;
using UnityEngine;
using UnityEngine.UI;
using Eldoria.UI.RoyalGold.V2;
using Eldoria.UI.RoyalGold;

namespace Eldoria.HeroesUI
{
    /// <summary>Real controls on the existing HUD. The host owns all data, models and commands.</summary>
    [DisallowMultipleComponent]
    public sealed class HeroesApprovedDestination : MonoBehaviour
    {
        [Serializable] public struct HeroState
        {
            public string Id, DisplayName, Role, AbilityName, AbilityDescription;
            public int Level, Health, Attack, Defense, Break, HeroPower;
            public int Experience, ExperienceToNext, Capacity, Mastery;
            public bool Unlocked, Available;
            public Sprite Portrait;
        }
        public enum Page { Salon, Resumen, Habilidades, Talentos, Equipamiento }
        public Func<HeroState[]> ReadHeroes;
        public Func<string, bool> CanOpenHero;
        public Func<bool> CanOpenDestination;
        public Func<string, GameObject> ReadModelRoot;
        public Action<string> HeroViewed;
        public Action Closed;
        public RectTransform ExistingHudSafeArea;
        // Kept for serialized-reference compatibility; never rendered as interactive UI.
        public Sprite[] ApprovedReferenceScreens = new Sprite[6];
        public GameObject ApprovedAldricModel, ApprovedLyraModel;
        public Page CurrentPage { get; private set; } = Page.Salon;
        public string CurrentHeroId { get; private set; }
        public bool IsOpen => root && root.gameObject.activeSelf;
        bool recruitmentPresentation;
        public bool IsRecruitmentPresentation => IsOpen && recruitmentPresentation;
        public void PresentRecruitment(string id)
        {
            Open();
            recruitmentPresentation=true;
            if(!OpenHero(id))recruitmentPresentation=false;
        }
        public RectTransform VisibleRoot => IsOpen ? root : null;
        RectTransform root, content;
        HeroesModelSnapshot preview;
        Font font,compactFont;
        float observedWidth, observedHeight;
        readonly Color gold = new Color(.78f,.63f,.37f), ivory = new Color(.96f,.91f,.80f),
            midnight = new Color(.035f,.049f,.069f), panel = new Color(.065f,.079f,.099f);

        public void Initialize(RectTransform safeArea, Func<HeroState[]> readHeroes)
        {
            if (!safeArea || readHeroes == null) throw new ArgumentNullException();
            ExistingHudSafeArea=safeArea;ReadHeroes=readHeroes;
            compactFont=Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            font=Resources.Load<Font>("EldoriaRetention/reference-serif") ?? compactFont;
            if(root)return;
            root=Rect("Heroes / canonical destination",safeArea);
            root.anchorMin=Vector2.zero;root.anchorMax=Vector2.one;
            root.offsetMin=root.offsetMax=Vector2.zero;
            root.gameObject.AddComponent<Image>().color=midnight;
            preview=root.gameObject.AddComponent<HeroesModelSnapshot>();
            root.gameObject.SetActive(false);
        }
        public void Open()
        {
            recruitmentPresentation=false;
            if(!root)throw new InvalidOperationException("Initialize on the canonical HUD first");
            if(CanOpenDestination!=null&&!CanOpenDestination())return;
            CurrentPage=Page.Salon;CurrentHeroId=null;root.gameObject.SetActive(true);root.SetAsLastSibling();Rebuild();
        }
        public bool OpenHero(string id)
        {
            if(!IsOpen)return false;
            foreach(var hero in ReadHeroes?.Invoke() ?? Array.Empty<HeroState>())
                if(hero.Id==id&&hero.Unlocked&&(CanOpenHero==null||CanOpenHero(id)))
                {CurrentHeroId=id;CurrentPage=Page.Resumen;HeroViewed?.Invoke(id);Rebuild();return true;}
            return false;
        }
        public void Select(Page page)
        {
            if(!IsOpen||(page!=Page.Salon&&string.IsNullOrEmpty(CurrentHeroId)))return;
            CurrentPage=page;Rebuild();
        }
        public void Suspend(){if(!root)return;root.gameObject.SetActive(false);preview.Release();}
        public void Close(){if(!IsOpen)return;bool recruitment=recruitmentPresentation;recruitmentPresentation=false;Suspend();if(!recruitment)Closed?.Invoke();}
        public HeroState? CurrentAuthoritativeHero()
        {foreach(var h in ReadHeroes?.Invoke()??Array.Empty<HeroState>())if(h.Id==CurrentHeroId)return h;return null;}
        public void Refresh(){if(IsOpen)Rebuild();}
        void LateUpdate()
        {
            if(IsOpen&&(Mathf.Abs(root.rect.width-observedWidth)>1f||Mathf.Abs(root.rect.height-observedHeight)>1f))Rebuild();
            if(IsOpen&&CurrentPage==Page.Salon&&leftHallLight&&rightHallLight){
                float t=Time.unscaledTime;
                var left=leftHallLight.color;
                left.a=.31f+.025f*Mathf.Sin(t*1.7f);
                leftHallLight.color=left;
                var right=rightHallLight.color;
                right.a=.28f+.023f*Mathf.Sin(t*1.5f+1.3f);
                rightHallLight.color=right;
            }
        }
        RectTransform Rect(string name,Transform parent)
        {var r=new GameObject(name,typeof(RectTransform)).GetComponent<RectTransform>();r.SetParent(parent,false);return r;}
        void Place(RectTransform r,float x,float y,float w,float h)
        {r.anchorMin=r.anchorMax=r.pivot=new Vector2(0,1);r.anchoredPosition=new Vector2(x,-y);r.sizeDelta=new Vector2(w,h);}
        RectTransform Surface(string name,Transform parent,float x,float y,float w,float h,Color color)
        {var r=Rect(name,parent);Place(r,x,y,w,h);r.gameObject.AddComponent<Image>().color=color;return r;}
        RectTransform Frame(string name,Transform parent,float x,float y,float w,float h)
        {
            var r=Surface(name,parent,x,y,w,h,Color.clear);
            // Native 41198120660 proves the custom panel mesh was invisible
            // while ordinary Images rendered correctly on the same Canvas.
            // Use the proven uGUI path for actual readable cards and controls.
            var ink=Surface("Royal Gold / translucent ink",r,2,2,w-4,h-4,
                new Color(.018f,.030f,.048f,.79f));
            ink.GetComponent<Image>().raycastTarget=false;
            float edge=1.5f;
            Surface("Royal Gold / top rim",r,0,0,w,edge,gold).GetComponent<Image>().raycastTarget=false;
            Surface("Royal Gold / bottom rim",r,0,h-edge,w,edge,gold).GetComponent<Image>().raycastTarget=false;
            Surface("Royal Gold / left rim",r,0,0,edge,h,gold).GetComponent<Image>().raycastTarget=false;
            Surface("Royal Gold / right rim",r,w-edge,0,edge,h,gold).GetComponent<Image>().raycastTarget=false;
            return r;
        }
        void TintFrame(RectTransform frame,Color tint)
        {
            foreach(Transform child in frame)
                if(child.name.StartsWith("Royal Gold /")&&child.name.EndsWith("rim"))
                    child.GetComponent<Image>().color=tint;
        }
        Text Label(string name,Transform parent,string value,float x,float y,float w,float h,int size,TextAnchor align=TextAnchor.MiddleLeft)
        {
            var r=Rect(name,parent);Place(r,x,y,w,h);var t=r.gameObject.AddComponent<Text>();
            t.font=(CurrentPage!=Page.Salon && content && content.rect.width>content.rect.height && content.rect.height<500f) ? compactFont : font;t.text=value??"";t.fontSize=size;t.color=ivory;t.alignment=align;t.raycastTarget=false;
            t.horizontalOverflow=HorizontalWrapMode.Wrap;t.verticalOverflow=VerticalWrapMode.Truncate;
            t.resizeTextForBestFit=true;t.resizeTextMinSize=Mathf.Min(12,size);t.resizeTextMaxSize=size;return t;
        }
        Button Button(string name,string caption,float x,float y,float w,float h,Action action,bool enabled=true)
        {
            var r=Frame(name,content,x,y,w,h);var b=r.gameObject.AddComponent<Button>();b.interactable=enabled;
            b.onClick.AddListener(()=>action());var t=Label(name+" label",r,caption,5,3,w-10,h-6,20,TextAnchor.MiddleCenter);
            t.color=enabled?ivory:new Color(.48f,.48f,.48f);return b;
        }
        // UI-built heraldry keeps the reference's icon positions and does
        // not depend on Unity's previously invisible custom mesh graphics.
        // These icons are decorative children of ACTUAL click targets.
        readonly Sprite[] exactProfileIcons=new Sprite[6];
        Sprite ApprovedProfileNavigationIcon(int type)
        {
            if(type<0||type>=exactProfileIcons.Length)return null;
            if(exactProfileIcons[type])return exactProfileIcons[type];
            var master=Resources.Load<Texture2D>("EldoriaV41/HeroesApprovedMaster");
            if(!master||master.width!=1536||master.height!=1024)return null;
            // Only text-free ornament icon crops from the real owner-approved
            // second panel. No level, character, fake background UI or labels.
            int[] x={527,535,536,535,534,534};
            int[] y={63,135,207,282,358,432};
            int[] width={43,24,24,24,28,28};
            int[] height={39,27,25,24,29,29};
            var area=new UnityEngine.Rect(x[type],
                master.height-y[type]-height[type],width[type],height[type]);
            var sprite=Sprite.Create(master,area,new Vector2(.5f,.5f),100f,
                0,SpriteMeshType.FullRect);
            sprite.name="Exact approved hero navigation icon "+type;
            exactProfileIcons[type]=sprite;
            return sprite;
        }
        void ProfileReferenceSigil(Transform parent,int type,float width,float height,float size)
        {
            bool lateral=height<44f&&width>=100f;
            float centerX=lateral?14f:width*.5f,
                centerY=lateral?height*.5f:height*.35f;
            float unit=size/48f;
            Color bright=new Color(.94f,.73f,.39f,.98f);
            void Stroke(string name,float dx,float dy,float w,float h,float angle)
            {
                var rt=Surface("Profile reference / "+name,parent,
                    centerX+dx*unit-w*unit*.5f,centerY+dy*unit-h*unit*.5f,
                    w*unit,h*unit,bright);
                rt.pivot=new Vector2(.5f,.5f);
                rt.anchoredPosition=new Vector2(centerX+dx*unit,-centerY-dy*unit);
                rt.localRotation=Quaternion.Euler(0,0,angle);
                rt.GetComponent<Image>().raycastTarget=false;
            }
            // Prefer ONLY the exact, separate pictograms from the approved
            // original six-screen sheet. The surrounding image/labels/buttons
            // are NOT reused as fake UI; all interactions remain live.
            Sprite exact=ApprovedProfileNavigationIcon(type);
            if(exact){
                var rt=Rect("Profile reference / exact approved pictogram "+type,parent);
                float side=Mathf.Min(size,lateral?19f:43f);
                Place(rt,centerX-side*.5f,centerY-side*.5f,side,side);
                var art=rt.gameObject.AddComponent<Image>();
                art.sprite=exact;art.preserveAspect=true;art.raycastTarget=false;
                return;
            }
            if(type==0||type==1){ // sword + guard: profile crest / Summary.
                Stroke("sword blade",4,-5,6,34,-43);
                Stroke("sword pommel",-9,11,7,7,-43);
                Stroke("sword cross",-3,5,22,4,-43);
            }else if(type==2){ // Skills: embossed radiant circle.
                for(int a=0;a<8;a++){
                    float rad=a*Mathf.PI/4f;
                    Stroke("skill ray "+a,Mathf.Sin(rad)*14,Mathf.Cos(rad)*14,
                        4f,10f,-a*45f);
                }
                Stroke("skill kernel",0,0,10,10,45);
            }else if(type==3){ // Talents: star/compass.
                Stroke("talent north",0,0,5,38,0);
                Stroke("talent east",0,0,38,5,0);
                Stroke("talent slash",0,0,5,30,45);
                Stroke("talent inverse",0,0,5,30,-45);
            }else if(type==4){ // Equipment: original shield-shaped crest.
                Stroke("equipment top",0,-11,29,4,0);
                Stroke("equipment left",-11,0,4,25,-14);
                Stroke("equipment right",11,0,4,25,14);
                Stroke("equipment heel",0,15,21,4,0);
            }else{
                Stroke("history binding",-11,0,4,29,0);
                Stroke("history top",0,-13,24,4,0);
                Stroke("history bottom",0,13,24,4,0);
                Stroke("history pages",11,0,4,29,0);
            }
        }
        void Rebuild()
        {
            RebuildContent();
            if(!content || CurrentPage==Page.Salon)return;
            // uGUI Text rasterizes dynamic glyphs using Canvas.scaleFactor,
            // ignoring the inverse scaling used by this pixel-sized layout.
            // Rasterize larger glyphs and shrink only their leaf transforms:
            // identical visible bounds/type size, without a second Canvas.
            float rasterScale=Mathf.Ceil(Mathf.Max(1f,content.localScale.x));
            if(rasterScale<=1f)return;
            foreach(var label in content.GetComponentsInChildren<Text>(true))
            {
                if(!label.font || !label.font.dynamic)continue;
                var rect=label.rectTransform;
                Vector2 size=rect.rect.size;
                rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal,size.x*rasterScale);
                rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical,size.y*rasterScale);
                rect.localScale/=rasterScale;
                label.fontSize=Mathf.RoundToInt(label.fontSize*rasterScale);
                label.resizeTextMinSize=Mathf.RoundToInt(label.resizeTextMinSize*rasterScale);
                label.resizeTextMaxSize=Mathf.RoundToInt(label.resizeTextMaxSize*rasterScale);
            }
        }
        void RebuildContent()
        {
            if(!IsOpen)return;
            if(content){content.gameObject.SetActive(false);Destroy(content.gameObject);}
            content=Rect("Heroes / live controls",root);content.anchorMin=Vector2.zero;content.anchorMax=Vector2.one;
            content.offsetMin=content.offsetMax=Vector2.zero;
            float w=root.rect.width,h=root.rect.height;observedWidth=w;observedHeight=h;
            // The canonical Canvas scales a desktop reference down on phones.
            // Lay this destination out in visible pixels when that happens,
            // so a 48-unit touch target and 20-unit type remain readable.
            // Preserve the existing safe area and never change the shared HUD.
            var canvas=root.GetComponentInParent<Canvas>();
            float pixelScale=canvas?canvas.scaleFactor:1f;
            float layoutScale=1f/Mathf.Clamp(pixelScale,.1f,1f);
            content.anchorMin=content.anchorMax=content.pivot=new Vector2(0,1);
            content.anchoredPosition=Vector2.zero;
            content.localScale=Vector3.one*layoutScale;
            w/=layoutScale;h/=layoutScale;content.sizeDelta=new Vector2(w,h);
            if(w<200||h<200)return;
            DrawApprovedSalonEnvironment(w,h);
            bool portrait=h>w;float margin=20;
            if(CurrentPage!=Page.Salon)
                Surface("Top antique gold rule",content,margin,82,w-2*margin,1,gold).GetComponent<Image>().raycastTarget=false;
            Button("Close heroes","×",w-68,14,48,48,Close);
            if(CurrentPage==Page.Salon){BuildSalon(w,h);return;}
            var selected=CurrentAuthoritativeHero();
            if(!selected.HasValue||!selected.Value.Unlocked){CurrentPage=Page.Salon;CurrentHeroId=null;BuildSalon(w,h);return;}
            var hero=selected.Value;
            bool shortProfile=!portrait&&h<500f;
            bool narrowPortrait=portrait&&w<600f;
            // The approved character profile has ONE vertical icon rail
            // regardless of orientation. The old portrait-only four big
            // horizontal tabs were a reinterpretation rejected by the owner.
            float titleX=narrowPortrait?128f:portrait?200f:shortProfile?157f:200f;
            Label("Profile reference / Eldoria brand",content,"ELDORIA",20,11,
                narrowPortrait?98f:140f,33,narrowPortrait?16:shortProfile?18:22).color=gold;
            Label("Hero name",content,hero.DisplayName,titleX,12,
                Mathf.Max(65f,w-titleX-82f),34,
                narrowPortrait?21:shortProfile?24:32).color=ivory;
            Label("Hero role and real level",content,
                (recruitmentPresentation?"SE UNE A VALORIA · ":hero.Role+" · ")+"NIVEL "+hero.Level,titleX,47,
                Mathf.Max(65f,w-titleX-82f),29,
                narrowPortrait?13:shortProfile?15:20).color=ivory;
            var pages=new[]{Page.Resumen,Page.Habilidades,Page.Talentos,Page.Equipamiento};
            string[] captions={"RESUMEN","HABILIDADES","TALENTOS","EQUIPAMIENTO"};
            float tabWidth=portrait?(narrowPortrait?90f:132f):shortProfile?112f:123f;
            float top=portrait?(narrowPortrait?182f:194f):shortProfile?86f:100f;
            float startNav=portrait?(narrowPortrait?132f:170f):shortProfile?118f:190f;
            float navH=portrait?(narrowPortrait?46f:74f):
                shortProfile?32f:Mathf.Clamp((h-startNav-132f)/5f,72f,94f);
            float navGap=portrait?(narrowPortrait?6f:10f):shortProfile?4f:10f;
            var returnButton=Button("Heroes hall","",20,
                startNav-navH-navGap,tabWidth,navH,()=>Select(Page.Salon));
            ProfileReferenceSigil(returnButton.transform,0,tabWidth,navH,
                narrowPortrait?24f:shortProfile?19f:42f);
            for(int n=0;n<5;n++){
                float navY=startNav+n*(navH+navGap);
                if(n==4){
                    var locked=Frame("Hero tab Historia / not yet available",
                        content,20,navY,tabWidth,navH);
                    ProfileReferenceSigil(locked,5,tabWidth,navH,
                        narrowPortrait?21f:shortProfile?16f:31f);
                    var label=Label("Profile reference / Historia unavailable",locked,
                        "HISTORIA",shortProfile?28f:2f,
                        shortProfile?0f:navH-(narrowPortrait?20f:28f),
                        shortProfile?tabWidth-30f:tabWidth-4f,
                        shortProfile?navH:narrowPortrait?19f:23f,
                        shortProfile?11:narrowPortrait?11:14,TextAnchor.MiddleCenter);
                    label.color=new Color(.63f,.62f,.59f,.82f);
                    continue;
                }
                var page=pages[n];
                var button=Button("Hero tab "+page,"",20,navY,tabWidth,navH,
                    ()=>Select(page));
                TintFrame(button.GetComponent<RectTransform>(),CurrentPage==page?
                    new Color(.97f,.70f,.33f,1f):new Color(.45f,.38f,.26f,.85f));
                var plate=button.GetComponent<RectTransform>();
                var panelInk=plate.Find("Royal Gold / translucent ink");
                if(CurrentPage==page&&panelInk)
                    panelInk.GetComponent<Image>().color=new Color(.47f,.25f,.075f,.81f);
                ProfileReferenceSigil(button.transform,n+1,tabWidth,navH,
                    narrowPortrait?21f:shortProfile?16f:32f);
                var caption=Label("Profile reference / "+page+" caption",
                    button.transform,captions[n],shortProfile?28f:3f,
                    shortProfile?0f:navH-(narrowPortrait?20f:28f),
                    shortProfile?tabWidth-30f:tabWidth-6f,
                    shortProfile?navH:narrowPortrait?19f:23f,
                    shortProfile?11:narrowPortrait?11:14,TextAnchor.MiddleCenter);
                caption.color=CurrentPage==page?ivory:new Color(.86f,.83f,.76f);
                if(shortProfile){
                    // Full approved word EQUIPAMIENTO, never EQUIPO.
                    caption.resizeTextForBestFit=true;
                    caption.resizeTextMinSize=9;
                    caption.resizeTextMaxSize=11;
                }
            }
            float bodyX=tabWidth+(portrait?32f:36f),
                bodyW=w-bodyX-20f;
            float bodyH=h-top-(shortProfile?76f:92f),modelW=portrait?bodyW:bodyW*(shortProfile?.47f:.58f),modelH=portrait?bodyH*.62f:bodyH;
            // Reserve the four real attribute rows and role/footer before sizing
            // the portrait stage. Otherwise the 36-unit row minimum can overlap
            // the role divider at 900x1600.
            if(portrait&&CurrentPage==Page.Resumen&&w-40f>460f&&bodyH>=700f)
                modelH=Mathf.Min(modelH,bodyH-456f);
            var stage=Rect("Hero model stage",content);
            Place(stage,bodyX,top,modelW,modelH);
            if(CurrentPage!=Page.Talentos)BuildProfileStageDepth(stage,modelW,modelH);
            var source=ReadModelRoot?.Invoke(hero.Id) ?? (hero.Id=="aldric"?ApprovedAldricModel:ApprovedLyraModel);
            var raw=Rect("Genuine hero model render",stage).gameObject.AddComponent<RawImage>();
            Place(raw.rectTransform,5,5,modelW-10,modelH-46);raw.raycastTarget=false;
            if(source&&preview.Show(source,raw,hero.Id=="lyra"?30f:90f)){
                float imageH=modelH-46,imageW=Mathf.Min(modelW-10,imageH*2f/3f);
                Place(raw.rectTransform,(modelW-imageW)*.5f,5,imageW,imageH);
                if(CurrentPage!=Page.Talentos){
                    Button("Rotate hero left","<",bodyX+7,top+modelH-78,32,32,()=>{preview.Rotate(-30);Rebuild();});
                    Button("Rotate hero right",">",bodyX+modelW-39,top+modelH-78,32,32,()=>{preview.Rotate(30);Rebuild();});
                }
            }
            else{
                raw.enabled=false;
                if(hero.Portrait){
                    var img=Rect("Approved hero portrait",stage).gameObject.AddComponent<Image>();
                    float ratio=hero.Portrait.rect.width/Mathf.Max(1f,hero.Portrait.rect.height);
                    float imageH=modelH-62,imageW=imageH*ratio;
                    if(imageW>modelW-24){imageW=modelW-24;imageH=imageW/ratio;}
                    // Explicitly center the actual source aspect. preserveAspect
                    // alone respects this layout's top-left pivot and left the
                    // complete Lyra portrait stranded on half of a wide stage.
                    Place(img.rectTransform,(modelW-imageW)*.5f,12+(modelH-62-imageH)*.5f,imageW,imageH);
                    img.sprite=hero.Portrait;img.preserveAspect=true;img.raycastTarget=false;
                    // An approved illustrative fallback must not be pasted as
                    // a bright photographic rectangle inside the physical
                    // 2.5D hall. Dissolve its REAL source-rect edges with the
                    // same existing painterly shader used for portraits,
                    // keeping Lyra's face, armour, identity and silhouette.
                    // The true 3D model still takes over asynchronously.
                    FinishApprovedFallback(img,hero.Portrait);
                }
                else Label("Hero identity without substitute art",stage,hero.DisplayName,14,modelH*.35f,modelW-28,70,25,TextAnchor.MiddleCenter);
            }
            if(CurrentPage==Page.Equipamiento)
                HeroesEquipmentArt.BuildSlots(stage,modelW,modelH,hallFrameSprite);
            Label("Real hero availability",stage,hero.Available?"EN VALORIA":"EN EXPEDICIÓN",8,modelH-37,modelW-16,30,16,TextAnchor.MiddleCenter);
            float infoX=portrait?20f:bodyX+modelW+12f,
                infoY=portrait?top+modelH+12f:top;
            float infoW=portrait?w-40f:bodyW-modelW-12f,
                infoH=portrait?bodyH-modelH-12f:bodyH;
            var info=CurrentPage==Page.Resumen
                ? ProfileSummaryBoard(content,infoX,infoY,infoW,infoH)
                : CurrentPage==Page.Equipamiento
                    ? Rect("Equipment / open contextual field",content)
                    : Frame("Hero authoritative information",content,infoX,infoY,infoW,infoH);
            if(CurrentPage==Page.Equipamiento)Place(info,infoX,infoY,infoW,infoH);
            BuildInformation(info,hero,infoW,infoH);
            if(CurrentPage==Page.Talentos){
                stage.gameObject.SetActive(false);info.gameObject.SetActive(false);
                BuildReservedTalents(bodyX,top,bodyW,bodyH,portrait);
            }
            // Restore the reference's real portrait selector rather than a
            // text-only previous/name/next bar. Only authoritative unlocked
            // hero identities are clickable; four future portraits stay inert.
            var selector=Surface("Profile / carved hero selector rail",content,
                18f,h-78f,w-36f,65f,new Color(.018f,.027f,.044f,.90f));
            selector.GetComponent<Image>().raycastTarget=false;
            Surface("Profile / selector brass horizon",selector,6f,0f,w-48f,2f,
                new Color(.83f,.63f,.32f,.78f)).GetComponent<Image>().raycastTarget=false;
            Button("Previous available hero","<",20,h-66,64,46,()=>CycleHero(-1));
            Button("Next available hero",">",w-84,h-66,64,46,()=>CycleHero(1));
            // Hero identity is already prominent in the reference header.
            // Duplicating it next to the portrait strip obscured the first
            // actual hero portrait at 900x1600.
            if(!portrait)
                Label("Current hero identity",content,hero.DisplayName,
                    90,h-66,Mathf.Max(10f,Mathf.Min(185f,w*.16f)),46,
                    w<500f?17:20,TextAnchor.MiddleCenter).color=gold;
            BuildProfilePortraitRail(w,h);
        }
        // Production art is independent from the approved sheet. Never crop
        // controls, lettering or characters out of that sheet into runtime UI.
        // Actual isolated textures already shipping in the approved Royal
        // Gold UI kit. Never paste baked labels or the complete reference
        // screenshot into a live control.
        void HallControlArt(Transform parent,string asset,float w,float h,bool button=false)
        {
            var source=Resources.Load<Texture2D>("EldoriaV41/"+asset);
            if(!source)return;
            var layer=Rect("Hall / authored control material "+asset,parent);
            Place(layer,0,0,w,h);
            var visual=layer.gameObject.AddComponent<RawImage>();
            visual.texture=source;visual.raycastTarget=false;
            // Real Button caption always remains above the authored plate.
            if(button)layer.SetSiblingIndex(Mathf.Min(5,parent.childCount-1));
        }
        void HallNavIcon(Button button,int iconId,float cell,float height,bool selected)
        {
            // Reuse the canonical vector Royal Gold pictograms rather than
            // pasting the concept sheet's button icons into a flat screenshot.
            // These are visual-only children of genuine state-bound buttons.
            bool stacked=cell<115f&&height>=54f;
            float size=Mathf.Min(stacked?28f:30f,height*(stacked?.54f:.58f));
            var frame=Rect("Hall / nav actual vector pictogram "+iconId,button.transform);
            Place(frame,stacked?(cell-size)*.5f:Mathf.Max(5f,cell*.058f),
                stacked?1f:(height-size)*.5f,size,size);
            // The old custom RoyalIconGraphic mesh passed component-based
            // gates yet painted ZERO pixels in real Unity captures. Render a
            // real serif glyph (the same proven uGUI Text path as the live
            // hero captions) inside a raised, separately authored medallion.
            var medallion=Surface("Hall / nav bronze medallion",button.transform,
                stacked?(cell-size-6f)*.5f:Mathf.Max(3f,cell*.045f),
                stacked?1f:5f,size+6f,stacked?size+3f:height-10f,
                selected?new Color(.76f,.53f,.24f,.48f):
                    new Color(.027f,.043f,.067f,.82f));
            medallion.GetComponent<Image>().raycastTarget=false;
            // Layer the glyph ABOVE its opaque medallion, not underneath.
            frame.SetAsLastSibling();
            var glyph=frame.gameObject.AddComponent<Text>();
            glyph.font=font;
            string icon=iconId==12?"‹":iconId==4?"✦":iconId==5?"›":"◇";
            // A missing Unicode glyph in the project's serif font silently
            // renders an empty black medallion, even with green UI tests.
            // Use a guaranteed ASCII visual fallback for each identity.
            char fallback=iconId==12?'<':iconId==4?'H':iconId==5?'>':'R';
            glyph.text=font&&font.HasCharacter(icon[0])?icon:fallback.ToString();
            glyph.alignment=TextAnchor.MiddleCenter;
            // Unity Text's default vertical truncation hid the glyph inside
            // a 20px phone icon rectangle although component QA passed.
            // Render a font-safe pictogram that actually fits the aperture.
            glyph.fontSize=24;
            glyph.resizeTextForBestFit=false;
            glyph.verticalOverflow=VerticalWrapMode.Overflow;
            glyph.horizontalOverflow=HorizontalWrapMode.Overflow;
            glyph.color=selected?new Color(.15f,.10f,.045f,.98f):
                new Color(.96f,.81f,.53f,.98f);
            glyph.raycastTarget=false;
            // Resolve the original live button caption BY NAME, not the
            // newly appended pictogram. GetComponentInChildren<Text>() chose
            // whichever text came first and shifted the icon out of sight.
            Text caption=null;
            foreach(var candidate in button.GetComponentsInChildren<Text>())
                if(candidate.name==button.name+" label"){caption=candidate;break;}
            if(caption){
                if(stacked){
                    // Four narrow mobile destinations need their icon ABOVE
                    // the caption, not beside it; the latter squeezed 6-9
                    // letters into 35 px and made them wrap over two lines.
                    Place(caption.rectTransform,3f,size+9f,
                        cell-6f,Mathf.Max(17f,height-size-15f));
                    caption.fontSize=14;
                    caption.resizeTextForBestFit=false;
                    caption.horizontalOverflow=HorizontalWrapMode.Overflow;
                    caption.verticalOverflow=VerticalWrapMode.Truncate;
                    caption.alignment=TextAnchor.MiddleCenter;
                } else {
                    Place(caption.rectTransform,size+9f,3f,
                        Mathf.Max(21f,cell-size-15f),height-6f);
                }
            }
        }
        Texture2D hallArchitecture, hallIllustratedV2, hallLightTexture, hallPortraitTransition;
        Material hallPaintMaterial, hallPortraitPaintMaterial, hallAldricMatteMaterial ,hallProfileFallbackMaterial;
        RawImage leftHallLight, rightHallLight;
        Texture2D HallGlow()
        {
            if (hallLightTexture) return hallLightTexture;
            const int n=64;
            hallLightTexture=new Texture2D(n,n,TextureFormat.RGBA32,false,true);
            hallLightTexture.name="Hall / living torchlight falloff";
            hallLightTexture.wrapMode=TextureWrapMode.Clamp;
            hallLightTexture.filterMode=FilterMode.Bilinear;
            hallLightTexture.hideFlags=HideFlags.HideAndDontSave;
            var px=new Color[n*n];
            for(int y=0;y<n;y++) for(int x=0;x<n;x++){
                float dx=(x+.5f-n*.5f)/(n*.5f);
                float dy=(y+.5f-n*.5f)/(n*.5f);
                float alpha=Mathf.Pow(Mathf.Clamp01(1f-Mathf.Sqrt(dx*dx+dy*dy)),1.7f);
                px[y*n+x]=new Color(1f,1f,1f,alpha);
            }
            hallLightTexture.SetPixels(px);
            hallLightTexture.Apply(false,true);
            return hallLightTexture;
        }
        RawImage HallLight(Transform parent,string name,float x,float y,float w,float h)
        {
            var light=Rect(name,parent).gameObject.AddComponent<RawImage>();
            Place(light.rectTransform,x,y,w,h);
            light.texture=HallGlow();
            light.raycastTarget=false;
            return light;
        }
        void OnDestroy()
        {
            foreach(var sprite in exactProfileIcons)
                if(sprite)Destroy(sprite);
            foreach(var sprite in approvedReservedPortraits)
                if(sprite)Destroy(sprite);
            if(futureSlotArt)Destroy(futureSlotArt);
            if(hallFrameSprite)Destroy(hallFrameSprite);
            if(hallLightTexture)Destroy(hallLightTexture);
            if(hallPortraitTransition)Destroy(hallPortraitTransition);
            if(hallPaintMaterial)Destroy(hallPaintMaterial);
            if(hallPortraitPaintMaterial)Destroy(hallPortraitPaintMaterial);
            if(hallAldricMatteMaterial)Destroy(hallAldricMatteMaterial);
            if(hallProfileFallbackMaterial)Destroy(hallProfileFallbackMaterial);
        }
        void FinishApprovedFallback(Image image,Sprite source)
        {
            if(!image || !source || !source.texture)return;
            var shader=Resources.Load<Shader>("EldoriaV41/HallPainterly");
            if(!shader)return;
            if(!hallProfileFallbackMaterial)
            {
                hallProfileFallbackMaterial=new Material(shader){
                    name="Eldoria / approved heroine soft architecture blend",
                    hideFlags=HideFlags.DontSave};
            }
            // The sprite can be a subregion of HeroesApprovedMaster.
            // Normalize against that sprite's source pixels, NOT against
            // the full atlas; otherwise only the atlas edges fade.
            var r=source.rect;
            var texture=source.texture;
            hallProfileFallbackMaterial.SetVector("_PortraitUVRect",new Vector4(
                r.xMin/texture.width,r.yMin/texture.height,
                r.xMax/texture.width,r.yMax/texture.height));
            hallProfileFallbackMaterial.SetFloat("_Strength",.34f);
            hallProfileFallbackMaterial.SetFloat("_PortraitFeather",.20f);
            image.material=hallProfileFallbackMaterial;
        }

        void DrawApprovedSalonEnvironment(float w,float h)
        {
            if(!hallArchitecture)
                hallArchitecture=Resources.Load<Texture2D>("EldoriaV41/heroes-hall-clean-v1");
            if(CurrentPage==Page.Salon&&!hallIllustratedV2)
                hallIllustratedV2=Resources.Load<Texture2D>("EldoriaV41/heroes-hall-illustrated-v2");
            var scene=Rect("Hall / painted architectural stage",content);
            Place(scene,0,0,w,h);
            if(CurrentPage==Page.Salon){
                // New authored Hall image is an independently generated,
                // character-free, UI-free painterly architectural source.
                // It replaces the old photorealistic raster entirely:
                // no concept-sheet crop, fake hero, or baked button is used.
                // Existing UI/2.5D depth planes remain separate and live.
                var painted=scene.gameObject.AddComponent<RawImage>();
                painted.name="Hall / painterly architectural plate";
                var source=hallIllustratedV2?hallIllustratedV2:hallArchitecture;
                painted.texture=source;
                painted.raycastTarget=false;
                if(source){
                    float sourceAspect=source.width/(float)source.height;
                    float viewAspect=w/h;
                    float cropW=Mathf.Min(1f,viewAspect/sourceAspect);
                    float cropH=Mathf.Min(1f,sourceAspect/viewAspect);
                    painted.uvRect=new UnityEngine.Rect((1f-cropW)*.5f,(1f-cropH)*.5f,cropW,cropH);
                }
                // Continue using the portrait-specific brush material for
                // actual approved hero images, not for the clean background.
                if(!hallPaintMaterial){
                    var hallShader=Resources.Load<Shader>("EldoriaV41/HallPainterly");
                    if(hallShader)hallPaintMaterial=new Material(hallShader){
                        name="Eldoria / painted hero portrait material",
                        hideFlags=HideFlags.DontSave};
                }
                if(!hallIllustratedV2&&hallPaintMaterial)
                    painted.material=hallPaintMaterial;
                // Real 2.5D: four separately rendered depth planes in
                // foreground geometry, not part of the single photo or the
                // opaque hero cards. Uses the production-proven RoyalGold
                // layered-finish CanvasRenderer installation pattern.
                HeroesHallParallaxFinish.Install(scene);
            }else{
                // Individual profiles are a separate Block03 responsibility.
                // Preserve their previously certified render while Block02
                // changes only the Salon's illustrative world.
                var plate=scene.gameObject.AddComponent<RawImage>();
                plate.texture=hallArchitecture;plate.raycastTarget=false;
                if(hallArchitecture){
                    float sourceAspect=hallArchitecture.width/(float)hallArchitecture.height;
                    float viewAspect=w/h;
                    float cropW=Mathf.Min(1f,viewAspect/sourceAspect);
                    float cropH=Mathf.Min(1f,sourceAspect/viewAspect);
                    plate.uvRect=new UnityEngine.Rect((1f-cropW)*.5f,(1f-cropH)*.5f,cropW,cropH);
                }
                DrawProfileArchitecturalLayers(scene,w,h);
            }
            leftHallLight=rightHallLight=null;
            if(CurrentPage!=Page.Salon){
                Surface("Hall / profile top ink",content,0,0,w,88f,
                    new Color(.015f,.022f,.034f,.73f)).GetComponent<Image>().raycastTarget=false;
                Surface("Hall / profile footer",content,0,h-80f,w,80f,
                    new Color(.015f,.022f,.034f,.91f)).GetComponent<Image>().raycastTarget=false;
                Surface("Hall footer gold line",content,16,h-79,w-32,2,gold)
                    .GetComponent<Image>().raycastTarget=false;
                return;
            }
            bool widePhone=w>h&&h<500f;
            // Genuine independent light volumes above the scenic plate and
            // behind gameplay controls. Their alpha subtly changes in play;
            // this is not a cropped photograph, a new scene or an extra camera.
            leftHallLight=HallLight(scene,"Hall / left living torch",0,h*.29f,
                Mathf.Min(w*.37f,460f),h*.62f);
            rightHallLight=HallLight(scene,"Hall / right living torch",
                w-Mathf.Min(w*.37f,460f),h*.29f,Mathf.Min(w*.37f,460f),h*.62f);
            leftHallLight.color=new Color(1f,.57f,.20f,.31f);
            rightHallLight.color=new Color(1f,.68f,.29f,.28f);
            float headerH=widePhone?76f:h>w?Mathf.Clamp(h*.19f,130f,174f):142f;
            float footerH=widePhone?64f:72f;
            // Separate architectural overlays: stone pilasters, royal banners,
            // cornice and grounded plinth. No large opaque overlay hides the hall.
            var depth=new Color(.017f,.028f,.049f,.53f);
            float pillar=widePhone?12f:Mathf.Min(30f,w*.035f);
            Surface("Hall / left foreground pilaster",content,0,headerH,pillar,
                Mathf.Max(8f,h-headerH-footerH),depth).GetComponent<Image>().raycastTarget=false;
            Surface("Hall / right foreground pilaster",content,w-pillar,headerH,pillar,
                Mathf.Max(8f,h-headerH-footerH),depth).GetComponent<Image>().raycastTarget=false;
            Surface("Hall / left gilded column seam",content,pillar-2f,headerH,2,
                Mathf.Max(8f,h-headerH-footerH),new Color(.78f,.56f,.25f,.67f))
                .GetComponent<Image>().raycastTarget=false;
            Surface("Hall / right gilded column seam",content,w-pillar,headerH,2,
                Mathf.Max(8f,h-headerH-footerH),new Color(.78f,.56f,.25f,.67f))
                .GetComponent<Image>().raycastTarget=false;
            // Varying translucent strips give depth without pretending the
            // one-piece background is authored 3-D geometry.
            Surface("Hall / raised cornice shadow",content,0,headerH-8f,w,16f,
                new Color(.005f,.008f,.015f,.48f)).GetComponent<Image>().raycastTarget=false;
            Surface("Hall / gilded cornice",content,pillar,headerH-2f,w-2*pillar,2f,
                new Color(.85f,.66f,.33f,.83f)).GetComponent<Image>().raycastTarget=false;
            Surface("Hall / dark floor plinth",content,0,h-footerH-12f,w,footerH+12f,
                new Color(.025f,.029f,.041f,.50f)).GetComponent<Image>().raycastTarget=false;
            Surface("Hall / illuminated floor edge",content,pillar,h-footerH-12f,
                w-2*pillar,2f,new Color(.84f,.63f,.32f,.79f))
                .GetComponent<Image>().raycastTarget=false;
            // Royal blue fabric is a distinct transparent foreground accent.
            if(!widePhone){
                float bannerWidth=Mathf.Min(32f,w*.075f);
                foreach(float x in new[]{pillar+4f,w-pillar-4f-bannerWidth}){
                    var banner=Surface("Hall / layered royal banner",content,x,headerH+6,
                        bannerWidth,Mathf.Min(110f,(h-headerH-footerH)*.34f),
                        new Color(.025f,.085f,.20f,.82f));
                    banner.GetComponent<Image>().raycastTarget=false;
                    Surface("Hall / banner gold hem",banner,2f,3f,bannerWidth-4f,2f,gold)
                        .GetComponent<Image>().raycastTarget=false;
                    Surface("Hall / banner gold spine",banner,bannerWidth*.5f-1f,8f,2f,
                        Mathf.Max(10f,banner.rect.height-16f),new Color(.82f,.61f,.30f,.62f))
                        .GetComponent<Image>().raycastTarget=false;
                }
            }
            Surface("Hall / elevated header veil",content,0,0,w,headerH-8f,
                new Color(.014f,.022f,.037f,widePhone ? .53f : .43f))
                .GetComponent<Image>().raycastTarget=false;
            Surface("Hall / header gilding",content,12f,5f,w-24f,2f,
                new Color(.81f,.60f,.29f,.75f)).GetComponent<Image>().raycastTarget=false;
        }

        // Block03: independent layered scenery for hero profiles, installed
        // behind the genuine model and every state-bound control. The Salon
        // is intentionally untouched by this treatment.
        void DrawProfileArchitecturalLayers(RectTransform scene,float w,float h)
        {
            bool compact=w>h&&h<500f;
            // The painted far wall remains visible. Architecture is split
            // into foreground, middleground, light volumes and floor.
            // The Salon's full-screen perspective-floor mesh draws long
            // graticule lines across the model on profile pages. Native A/B
            // showed it visually flattens the hero rather than integrating
            // them; use distinct, quieter architectural planes here instead.
            Surface("Profile / deep vault cornice shade",scene,0,h*.10f,w,h*.18f,
                new Color(.015f,.022f,.037f,.20f)).GetComponent<Image>().raycastTarget=false;
            float side=Mathf.Clamp(w*.033f,compact?8f:12f,46f);
            float floorY=h*(compact?.80f:.83f);
            var shadow=new Color(.011f,.018f,.030f,.55f);
            Surface("Profile / left foreground stone pier",scene,0,h*.17f,side,h*.66f,shadow)
                .GetComponent<Image>().raycastTarget=false;
            Surface("Profile / right foreground stone pier",scene,w-side,h*.17f,side,h*.66f,shadow)
                .GetComponent<Image>().raycastTarget=false;
            Surface("Profile / left gilded bevel",scene,side-2f,h*.17f,2f,h*.66f,
                new Color(.67f,.48f,.23f,.56f)).GetComponent<Image>().raycastTarget=false;
            Surface("Profile / right gilded bevel",scene,w-side,h*.17f,2f,h*.66f,
                new Color(.67f,.48f,.23f,.56f)).GetComponent<Image>().raycastTarget=false;
            Surface("Profile / distant floor shade",scene,0,floorY,w,h-floorY,
                new Color(.010f,.015f,.024f,.34f)).GetComponent<Image>().raycastTarget=false;
            // The actual lit stone floor and gilded dais are now 3D objects
            // inside the portrait camera. No fake 2D floor horizon is drawn
            // across the hero's feet.
            float glowSize=Mathf.Min(w*.38f,520f);
            var warm=HallLight(scene,"Profile / torchlight architectural bounce",
                side,h*.22f,glowSize,h*.62f);
            warm.color=new Color(1f,.58f,.25f,.18f);
            var opposite=HallLight(scene,"Profile / opposite window bounce",
                w-glowSize-side,h*.10f,glowSize,h*.69f);
            opposite.color=new Color(.50f,.63f,.83f,.11f);
        }
        void BuildProfileStageDepth(RectTransform stage,float w,float h)
        {
            // The painted hall already owns the arches, columns and floor.
            // Repeating those edges as a rectangular overlay around the 3D
            // character made the portrait appear pasted on top of the world.
            // Keep only diffuse bounced light behind the *real* skinned mesh.
            // Its physical receiver/light/camera remain in HeroesModelSnapshot.
            var warm=HallLight(stage,"Profile / living window bounce",
                w*.12f,h*.27f,w*.75f,h*.62f);
            warm.color=new Color(1f,.64f,.31f,.09f);
        }
        void BuildProfilePortraitRail(float w,float h)
        {
            var heroes=ReadHeroes?.Invoke()??Array.Empty<HeroState>();
            int visible=w<470f?2:w<720f?4:5;
            float available=Mathf.Max(95f,w-210f);
            float gap=w<470f?8f:10f;
            float cell=Mathf.Min(65f,(available-gap*(visible-1))/visible);
            cell=Mathf.Max(28f,cell);
            float all=visible*cell+(visible-1)*gap;
            float start=(w-all)*.5f;
            for(int i=0;i<visible;i++){
                bool actual=i<heroes.Length;
                var plate=Frame("Profile / "+(actual?"hero portrait "+heroes[i].Id:
                    "future portrait "+(i-heroes.Length)),content,
                    start+i*(cell+gap),h-70f,cell,cell);
                bool selected=actual&&heroes[i].Id==CurrentHeroId;
                TintFrame(plate,selected?new Color(1f,.76f,.38f,1f):
                    new Color(.50f,.43f,.32f,.75f));
                Sprite sprite=actual?heroes[i].Portrait:
                    ApprovedFuturePortrait(i-heroes.Length);
                if(sprite){
                    var image=Rect("Profile / genuine or reserved archive portrait",plate)
                        .gameObject.AddComponent<Image>();
                    Place(image.rectTransform,4f,4f,cell-8f,cell-8f);
                    image.sprite=sprite;image.preserveAspect=true;
                    image.color=actual?Color.white:new Color(.33f,.36f,.40f,.72f);
                    image.raycastTarget=false;
                }else{
                    Label("Profile / unavailable portrait",plate,"?",3f,3f,
                        cell-6f,cell-6f,19,TextAnchor.MiddleCenter).color=gold;
                }
                if(!actual||!heroes[i].Unlocked){
                    Label("Profile / locked selection",plate,"×",cell*.65f,cell*.55f,
                        cell*.30f,cell*.32f,13,TextAnchor.MiddleCenter).color=ivory;
                    continue;
                }
                string targetId=heroes[i].Id;
                var button=plate.gameObject.AddComponent<Button>();
                button.interactable=CanOpenHero==null||CanOpenHero(targetId);
                button.onClick.AddListener(()=>OpenHero(targetId));
            }
        }

        RectTransform ProfileSummaryBoard(Transform parent,float x,float y,float w,float h)
        {
            // No opaque black sheet behind the full information column.
            // Only separate carved tiles float in an open architectural bay.
            var bay=Rect("Hero authoritative information",parent);
            Place(bay,x,y,w,h);
            Surface("Profile / information bay top cornice",bay,0,0,w,2f,
                new Color(.82f,.63f,.35f,.65f)).GetComponent<Image>().raycastTarget=false;
            Surface("Profile / information bay left shadow",bay,0,3f,2f,h-6f,
                new Color(.10f,.09f,.08f,.53f)).GetComponent<Image>().raycastTarget=false;
            Surface("Profile / information bay right shadow",bay,w-2f,3f,2f,h-6f,
                new Color(.10f,.09f,.08f,.53f)).GetComponent<Image>().raycastTarget=false;
            Surface("Profile / information bay lower rim",bay,0,h-2f,w,2f,
                new Color(.77f,.58f,.32f,.56f)).GetComponent<Image>().raycastTarget=false;
            return bay;
        }
        RectTransform ProfileModule(string name,Transform parent,float x,float y,float w,float h)
        {
            var r=Surface(name,parent,x,y,w,h,new Color(.057f,.052f,.060f,.90f));
            r.GetComponent<Image>().raycastTarget=false;
            // Bevel and inset are separate uGUI layers. These render correctly
            // on actual Unity Canvas, unlike the rejected invisible custom mesh.
            Surface(name+" / top brass bevel",r,0,0,w,2f,
                new Color(.84f,.66f,.38f,.84f)).GetComponent<Image>().raycastTarget=false;
            Surface(name+" / lower recessed edge",r,1f,h-2f,w-2f,2f,
                new Color(.32f,.25f,.16f,.79f)).GetComponent<Image>().raycastTarget=false;
            Surface(name+" / left recessed edge",r,0,2f,1f,Mathf.Max(0,h-4f),
                new Color(.61f,.45f,.26f,.53f)).GetComponent<Image>().raycastTarget=false;
            Surface(name+" / right recessed edge",r,w-1f,2f,1f,Mathf.Max(0,h-4f),
                new Color(.61f,.45f,.26f,.53f)).GetComponent<Image>().raycastTarget=false;
            // Soft warm polish across each carved stone surface avoids four
            // flat opaque black boxes without introducing fake stats or art.
            var lacquer=HallLight(r,name+" / bronze ambient brush",
                w*.10f,h*.08f,w*.80f,h*.80f);
            lacquer.color=new Color(.92f,.65f,.38f,.10f);
            Surface(name+" / engraved brass spine",r,3f,5f,1f,Mathf.Max(0f,h-10f),
                new Color(.78f,.56f,.31f,.32f)).GetComponent<Image>().raycastTarget=false;
            if(w>65f&&h>28f){
                Surface(name+" / upper inset reflection",r,5f,5f,w-10f,1f,
                    new Color(.95f,.75f,.45f,.18f)).GetComponent<Image>().raycastTarget=false;
                Surface(name+" / lower inset shadow",r,5f,h-6f,w-10f,1f,
                    new Color(.015f,.015f,.022f,.82f)).GetComponent<Image>().raycastTarget=false;
                float corner=Mathf.Clamp(h*.14f,3.5f,9f);
                for(int side=0;side<2;side++){
                    float cx=side==0?5f:w-5f-corner;
                    Surface(name+" / chased upper corner",r,cx,5f,corner,2f,
                        new Color(.92f,.70f,.40f,.67f)).GetComponent<Image>().raycastTarget=false;
                    Surface(name+" / corner drop",r,side==0?5f:w-7f,5f,2f,corner,
                        new Color(.92f,.70f,.40f,.67f)).GetComponent<Image>().raycastTarget=false;
                }
            }
            return r;
        }
        // Each real combat stat receives its own inlaid heraldic seal,
        // assembled from known-working uGUI Images (not an opaque screenshot).
        void AttributeRelief(RectTransform plate,int id,float w,float h)
        {
            float size=Mathf.Clamp(h*.49f,20f,49f),cx=11f+size*.5f,cy=h*.50f;
            var recess=Surface("Attribute / carved oval recess",plate,
                cx-size*.5f,cy-size*.5f,size,size,new Color(.11f,.082f,.062f,.83f));
            recess.GetComponent<Image>().raycastTarget=false;
            var coin=Surface("Attribute / chased bronze diamond",plate,
                cx-size*.32f,cy-size*.32f,size*.64f,size*.64f,
                new Color(.65f,.46f,.25f,.80f));
            coin.pivot=new Vector2(.5f,.5f);
            coin.anchoredPosition=new Vector2(cx,-cy);
            coin.localRotation=Quaternion.Euler(0f,0f,45f);
            coin.GetComponent<Image>().raycastTarget=false;
            var inset=Surface("Attribute / royal enamel inlay",plate,
                cx-size*.235f,cy-size*.235f,size*.47f,size*.47f,
                new Color(.035f,.074f,.113f,.95f));
            inset.pivot=new Vector2(.5f,.5f);
            inset.anchoredPosition=new Vector2(cx,-cy);
            inset.localRotation=Quaternion.Euler(0f,0f,45f);
            inset.GetComponent<Image>().raycastTarget=false;
            float thickness=Mathf.Clamp(size*.10f,2f,4f);
            void Mark(string suffix,float dx,float dy,float len,float thick,float angle)
            {
                var mark=Surface("Attribute / "+id+" "+suffix,plate,
                    cx+dx-len*.5f,cy+dy-thick*.5f,len,thick,
                    new Color(.96f,.79f,.53f,.96f));
                mark.pivot=new Vector2(.5f,.5f);
                mark.anchoredPosition=new Vector2(cx+dx,-cy-dy);
                mark.localRotation=Quaternion.Euler(0f,0f,angle);
                mark.GetComponent<Image>().raycastTarget=false;
            }
            if(id==0){Mark("vital vertical",0,0,thickness,size*.30f,90);Mark("vital cross",0,0,size*.30f,thickness,0);}
            else if(id==1){Mark("sword left",-thickness*.25f,0,size*.32f,thickness*.6f,48);Mark("sword right",thickness*.25f,0,size*.32f,thickness*.6f,-48);}
            else if(id==2){Mark("shield rim",0,-size*.11f,size*.27f,thickness*.7f,0);Mark("shield spine",0,0,thickness*.8f,size*.29f,90);Mark("shield foot",0,size*.16f,size*.19f,thickness*.6f,0);}
            else{Mark("rift edge one",0,0,size*.35f,thickness*.6f,45);Mark("rift edge two",0,0,size*.35f,thickness*.6f,-45);}
        }
        // Desktop matches the approved profile's compact heraldic power
        // column: unified panel, four combat rows, role and real march data.
        // Never create fictional skills, gear, stars or combat bonuses.
        void BuildReferenceCombatSummary(RectTransform pane,HeroState hero,float w,float h)
        {
            float outer=12f;
            var board=ProfileModule("Profile / royal integrated combat cabinet",pane,
                outer,outer,w-outer*2f,h-outer*2f);
            float bw=w-outer*2f,bh=h-outer*2f;
            float header=Mathf.Clamp(bh*.19f,88f,140f);
            Label("Profile / original power legend",board,"PODER DEL HÉROE",
                18f,8f,bw-36f,35f,22).color=gold;
            Label("Real hero power",board,hero.HeroPower.ToString(),
                18f,header*.36f,bw-36f,header*.51f,Mathf.Clamp((int)(bw*.065f),28,46),
                TextAnchor.MiddleCenter).color=gold;
            Surface("Profile / heraldic divider",board,17f,header,bw-34f,2f,
                new Color(.88f,.65f,.32f,.77f)).GetComponent<Image>().raycastTarget=false;
            Label("Combat attributes title",board,"ATRIBUTOS DE COMBATE",
                18f,header+7f,bw-36f,28f,Mathf.Clamp((int)(bw*.033f),15,22)).color=ivory;
            string[] names={"VIDA","ATAQUE","DEFENSA","RUPTURA"};
            int[] values={hero.Health,hero.Attack,hero.Defense,hero.Break};
            float roleH=Mathf.Clamp(bh*.17f,77f,116f);
            float footerH=Mathf.Clamp(bh*.11f,51f,73f);
            float rowTop=header+42f,roleY=bh-roleH-footerH-12f;
            float rowH=Mathf.Clamp((roleY-rowTop)/4f,36f,95f);
            for(int i=0;i<4;i++){
                float y=rowTop+i*rowH;
                var line=ProfileModule("Profile / "+names[i]+" carved attribute",board,
                    16f,y,bw-32f,Mathf.Max(30f,rowH-5f));
                AttributeRelief(line,i,bw-32f,rowH-5f);
                float left=20f+Mathf.Clamp((rowH-5f)*.49f,20f,49f);
                Label("Profile / verified "+names[i],line,names[i],
                    left,4f,Mathf.Max(35f,bw-left-125f),rowH-12f,21).color=ivory;
                Label(names[i]+" real value",line,values[i].ToString(),
                    bw-144f,3f,111f,rowH-11f,28,TextAnchor.MiddleRight).color=gold;
            }
            Surface("Profile / role cabinet divider",board,17f,roleY,bw-34f,2f,
                new Color(.85f,.62f,.32f,.69f)).GetComponent<Image>().raycastTarget=false;
            Label("Profile / real march role title",board,"ROL DEL HÉROE",
                19f,roleY+4f,bw-38f,28f,19).color=gold;
            Label("Profile / actual hero role and availability",board,
                hero.Role+"  ·  "+(hero.Available?"EN VALORIA":"EN EXPEDICIÓN"),
                19f,roleY+33f,bw-38f,Mathf.Max(28f,roleH-36f),19).color=ivory;
            var footer=ProfileModule("Profile / authoritative progression base",board,
                15f,bh-footerH-8f,bw-30f,footerH);
            string exp="EXP "+hero.Experience+(hero.ExperienceToNext>0?
                " / "+hero.ExperienceToNext:"");
            Label("Persistent experience",footer,exp,11f,3f,(bw-30f)*.49f,
                footerH-6f,17).color=ivory;
            Label("Actual march capacity",footer,"MARCHA "+hero.Capacity,
                (bw-30f)*.49f,3f,(bw-30f)*.48f,footerH-6f,17,
                TextAnchor.MiddleRight).color=gold;
        }

        void BuildCombatSummary(RectTransform pane,HeroState hero,float w,float h)
        {
            // Native world-corner checks cover both heroes at all four resolutions.
            // Below 444 layout units the minimum-height rows cannot fit with
            // the power header, real role and progression footer.
            if(w>460f&&h>=444f){BuildReferenceCombatSummary(pane,hero,w,h);return;}
            float margin=Mathf.Clamp(w*.029f,8f,22f);
            bool compact=h<280f;
            float headerH=compact?36f:Mathf.Min(76f,h*.17f);
            float footerH=compact?39f:Mathf.Min(76f,h*.17f);
            float gap=compact?5f:Mathf.Min(14f,h*.025f);
            float colGap=Mathf.Clamp(w*.025f,6f,18f);
            float colW=Mathf.Max(30f,(w-2f*margin-colGap)*.5f);
            float upper=margin+headerH+gap;
            float lower=h-margin-footerH-gap;
            float space=Mathf.Max(56f,lower-upper);
            float cellH=Mathf.Clamp((space-gap)*.5f,28f,compact?60f:130f);
            float cellsY=upper+Mathf.Max(0f,(space-(cellH*2f+gap))*.5f);
            var titleBar=ProfileModule("Profile / heraldic power banner",pane,
                margin,margin,w-2f*margin,headerH);
            float powerW=Mathf.Clamp(w*.32f,106f,235f);
            int titleFont=compact?15:Mathf.RoundToInt(Mathf.Clamp(w*.025f,17f,25f));
            Label("Combat attributes title",titleBar,"ATRIBUTOS",10f,4f,
                Mathf.Max(50f,w-2f*margin-powerW-18f),headerH-8f,
                titleFont).color=ivory;
            Label("Real hero power",titleBar,"PODER  "+hero.HeroPower,
                Mathf.Max(0f,w-2f*margin-powerW-8f),4f,
                powerW,headerH-8f,compact?17:26,TextAnchor.MiddleRight).color=gold;
            string[] names={"VIDA","ATAQUE","DEFENSA","RUPTURA"};
            int[] values={hero.Health,hero.Attack,hero.Defense,hero.Break};
            for(int i=0;i<4;i++){
                float x=margin+(i%2)*(colW+colGap);
                float y=cellsY+(i/2)*(cellH+gap);
                var plate=ProfileModule("Profile / "+names[i]+" carved attribute",pane,
                    x,y,colW,cellH);
                bool tall=cellH>76f;
                AttributeRelief(plate,i,colW,cellH);
                float iconEnd=11f+Mathf.Clamp(cellH*.49f,20f,49f)+9f;
                float numbersW=Mathf.Clamp(colW*.28f,38f,105f);
                float labelW=Mathf.Max(24f,colW-iconEnd-numbersW-10f);
                int nameSize=compact?15:tall?20:17;
                int valueSize=compact?22:tall?36:25;
                var statLabel=Label(names[i],plate,names[i],iconEnd,tall?14f:4f,
                    labelW,tall?32f:cellH-7f,nameSize);
                statLabel.color=ivory;
                if(compact)statLabel.fontStyle=FontStyle.Bold;
                Label(names[i]+" real value",plate,values[i].ToString(),
                    colW-numbersW-6f,tall?13f:4f,numbersW,tall?65f:cellH-7f,
                    valueSize,TextAnchor.MiddleRight).color=gold;
                if(tall)Surface("Profile / engraved attribute underline",plate,
                    11f,cellH-17f,colW-22f,1f,new Color(.76f,.53f,.28f,.42f))
                    .GetComponent<Image>().raycastTarget=false;
            }
            var footer=ProfileModule("Profile / progression and capacity",pane,
                margin,h-margin-footerH,w-2f*margin,footerH);
            string exp="EXP  "+hero.Experience+
                (hero.ExperienceToNext>0?" / "+hero.ExperienceToNext:"");
            Label("Persistent experience",footer,exp,9f,3f,
                (w-2f*margin)*.52f,footerH-6f,compact?14:18);
            Label("Actual march capacity",footer,"MARCHA  "+hero.Capacity,
                (w-2f*margin)*.51f,3f,(w-2f*margin)*.46f,footerH-6f,
                compact?14:18,TextAnchor.MiddleRight).color=gold;
        }

        Sprite futureSlotArt, hallFrameSprite, realBaluarteIcon;
        // Only the illustration portion of the genuine approved Baluarte
        // reference is reused: crop excludes every baked Nv.1/status label.
        // The gameplay icon is a standalone sprite, never a flattened panel.
        Sprite RealAbilityArtwork(string heroId)
        {
            if(heroId!="aldric")return ApprovedProfileNavigationIcon(2);
            if(realBaluarteIcon)return realBaluarteIcon;
            var master=Resources.Load<Texture2D>("EldoriaV41/HeroesApprovedMaster");
            if(master==null||master.width!=1536||master.height!=1024)
                return ApprovedProfileNavigationIcon(2);
            realBaluarteIcon=Sprite.Create(master,
                new Rect(1157,master.height-(326+41),63,41),
                new Vector2(.5f,.5f),100f,0,SpriteMeshType.FullRect);
            realBaluarteIcon.name="Baluarte / exact approved decorative pictogram";
            return realBaluarteIcon;
        }
        readonly Sprite[] approvedReservedPortraits=new Sprite[4];
        // The approved master is archived as art direction, NEVER a flattened
        // functional UI. Only its four individual, text-free, lock-free
        // MONOCHROME FUTURE CHARACTER ILLUSTRATIONS are reusable decorative
        // source artwork. The two live hero identities remain 100% runtime.
        Sprite ApprovedFuturePortrait(int index)
        {
            if(index<0||index>=approvedReservedPortraits.Length)return null;
            if(approvedReservedPortraits[index])return approvedReservedPortraits[index];
            var master=Resources.Load<Texture2D>("EldoriaV41/HeroesApprovedMaster");
            if(!master||master.width!=1536||master.height!=1024)return null;
            // Coordinates measured from the top-left 512x575 approved Salon.
            // Exclude all baked labels, levels, star ratings, buttons and
            // lock pictograms. Game state and live UI remain authoritative.
            int[] xs={335,36,185,334}, ys={210,407,407,407};
            int[] widths={130,130,130,130}, heights={102,61,61,61};
            var crop=new UnityEngine.Rect(xs[index],
                master.height-(ys[index]+heights[index]),
                widths[index],heights[index]);
            var sprite=Sprite.Create(master,crop,new Vector2(.5f,.5f),
                100f,0,SpriteMeshType.FullRect);
            sprite.name="Approved future illustration / passive "+index;
            approvedReservedPortraits[index]=sprite;
            return sprite;
        }
        // Portrait geometry lives inside its own aperture, never below an
        // opaque full-width card label in short landscape.

        // Only the genuine, state-bound hero portrait or the authentic locked
        // silhouette may occupy an interactive card. The approved six-panel
        // concept is a reference, NOT an atlas to paint over Lyra's validated
        // Tripo identity, Aldric's authored portrait, or future hero slots.
        // Decorative architectural layers remain separate and raycast-free.
        Texture2D HallPortraitTransition()
        {
            if(hallPortraitTransition)return hallPortraitTransition;
            const int n=128;
            hallPortraitTransition=new Texture2D(n,n,TextureFormat.RGBA32,false,true);
            hallPortraitTransition.name="Hall / genuine portrait edge-to-stone paint";
            hallPortraitTransition.wrapMode=TextureWrapMode.Clamp;
            hallPortraitTransition.filterMode=FilterMode.Bilinear;
            hallPortraitTransition.hideFlags=HideFlags.HideAndDontSave;
            var pixels=new Color[n*n];
            Color night=new Color(.055f,.083f,.115f,1f);
            for(int y=0;y<n;y++)for(int x=0;x<n;x++){
                float u=(x+.5f)/n,v=(y+.5f)/n;
                float edge=Mathf.Min(Mathf.Min(u,1f-u),Mathf.Min(v,1f-v));
                // A reversible, non-destructive frame-specific gradient.
                // The approved portrait GLB/sprite stays exact; only its
                // bright rectangular sky edges dissolve into the card.
                // Preserve the center of the genuine Aldric portrait,
                // while feathering its bright sky into the stone aperture.
                // The side vignette masks the otherwise obvious straight
                // raster cut; the upper-right sky gets a separate feather.
                float t=Mathf.Clamp01(edge/.21f);
                float border=(1f-t*t*(3f-2f*t))*.62f;
                float skyRight=Mathf.Clamp01((u-.59f)/.34f)*
                    Mathf.Clamp01((v-.36f)/.43f);
                float alpha=Mathf.Max(border,skyRight*.58f);
                pixels[y*n+x]=new Color(night.r,night.g,night.b,alpha);
            }
            hallPortraitTransition.SetPixels(pixels);
            hallPortraitTransition.Apply(false,true);
            return hallPortraitTransition;
        }
        void CardPortrait(RectTransform card,Sprite sprite,bool unlocked,bool fullBody,
            int slot,float x,float y,float w,float h)
        {
            var viewport=Rect("Hero card portrait aperture",card);
            Place(viewport,x,y,w,h);
            viewport.gameObject.AddComponent<RectMask2D>();
            Surface("Hall / portrait depth backing",viewport,0,0,w,h,
                new Color(.075f,.088f,.101f,1f)).GetComponent<Image>().raycastTarget=false;
            // Modular 2.5D character alcove: another genuine art plane behind
            // the actual runtime sprite (including the imported Tripo Lyra),
            // rather than leaving a giant black rectangular card. It reuses
            // only the new clean architectural painting, never a flattened
            // concept card. Locked slots are dim but not empty.
            if(hallIllustratedV2){
                var alcove=Rect("Hall / separate illustrated character alcove",viewport)
                    .gameObject.AddComponent<RawImage>();
                Place(alcove.rectTransform,0,0,w,h);
                alcove.texture=hallIllustratedV2;
                // Sample EXACTLY the same painted chamber seen behind this
                // aperture: the original per-slot UV crops pasted unrelated
                // banners/walls inside each frame. Full-scene UV continuity
                // makes 2.5D heroes feel seated in ONE coherent architecture.
                // This affects only the raycast-free background of each card.
                float sceneW=Mathf.Max(1f,content.rect.width);
                float sceneH=Mathf.Max(1f,content.rect.height);
                float sourceAspect=hallIllustratedV2.width/(float)hallIllustratedV2.height;
                float viewportAspect=sceneW/sceneH;
                float visibleU=Mathf.Min(1f,viewportAspect/sourceAspect);
                float visibleV=Mathf.Min(1f,sourceAspect/viewportAspect);
                float absoluteX=card.anchoredPosition.x+x;
                float absoluteY=-card.anchoredPosition.y+y;
                alcove.uvRect=new UnityEngine.Rect(
                    (1f-visibleU)*.5f+visibleU*absoluteX/sceneW,
                    (1f-visibleV)*.5f+visibleV*(1f-(absoluteY+h)/sceneH),
                    visibleU*w/sceneW,visibleV*h/sceneH);
                alcove.color=slot<2?
                    new Color(.98f,.94f,.86f,.86f):
                    new Color(.79f,.78f,.77f,.61f);
                alcove.raycastTarget=false;
            }
            if(sprite){
                var image=Rect("Verified portrait",viewport).gameObject.AddComponent<Image>();
                float ratio=sprite.rect.width/Mathf.Max(1f,sprite.rect.height);
                // Cover the actual aperture, not the whole horizontal card.
                // Legacy fit-inside produced a tiny figure even when a
                // mask and legitimate approved portrait were present.
                // Tall 3D Lyra requires an aperture cover, while Aldric's
                // approved illustrated bust must be FIT inside the aperture.
                // Applying cover to both enlarged Aldric's face to a cropped
                // forehead at 1600x900; the two sources are not interchangeable.
                float imageW=fullBody?Mathf.Max(w,h*ratio):
                    Mathf.Max(Mathf.Min(w,h*ratio),w*.64f);
                float imageH=imageW/ratio;
                // The previous centered bust cropped Aldric above his
                // eyebrows when the approved gallery made cards shallower.
                // A portrait photograph is not an ornamental texture: keep
                // his identity, hairline and face entirely in the visible
                // upper aperture, while lower armour may crop naturally.
                float top=fullBody?-(imageH-h)*(w>h?.12f:.27f):
                    imageH>h?-Mathf.Min(6f,(imageH-h)*.06f):
                    (h-imageH)*.5f;
                Place(image.rectTransform,(w-imageW)*.5f,top,imageW,imageH);
                image.sprite=sprite;image.raycastTarget=false;
                image.color=unlocked?Color.white:
                    // The approved reserves are already dark monochrome
                    // paintings. Applying the old 42% grey tint erased faces.
                    slot>=2?new Color(.90f,.91f,.94f,.97f):
                    new Color(.42f,.46f,.51f,.90f);
                if(slot<2 && hallPaintMaterial){
                    // Render approved source sprites as illustrated character
                    // cards. Originals, Tripo geometry and saved hero identities
                    // remain untouched; only card material is painted.
                    // ClipRect support keeps Lyra safely inside its 2.5D frame.
                    if(!hallPortraitPaintMaterial){
                        hallPortraitPaintMaterial=new Material(hallPaintMaterial){
                            name="Eldoria / illustrated canonical hero card",
                            hideFlags=HideFlags.DontSave};
                        hallPortraitPaintMaterial.SetFloat("_Strength",.12f);
                    }
                    if(slot==0){
                        if(!hallAldricMatteMaterial){
                            hallAldricMatteMaterial=new Material(hallPortraitPaintMaterial){
                                name="Eldoria / authored Aldric soft-edge stage",
                                hideFlags=HideFlags.DontSave};
                            hallAldricMatteMaterial.SetFloat("_PortraitFeather",.22f);
                            hallAldricMatteMaterial.SetFloat("_Strength",.21f);
                        }
                        image.material=hallAldricMatteMaterial;
                    }else image.material=hallPortraitPaintMaterial;
                }
                if(slot==0){
                    // Aldric's genuine illustrated bust has an originally
                    // baked bright sky. This separate feathered material stage
                    // eliminates the visibly pasted rectangular photo edge
                    // while keeping the authoritative source completely intact.
                    var transition=Rect("Hall / original portrait edge transition",viewport)
                        .gameObject.AddComponent<RawImage>();
                    Place(transition.rectTransform,(w-imageW)*.5f,top,imageW,imageH);
                    transition.texture=HallPortraitTransition();
                    transition.raycastTarget=false;
                }
            }
            // Four distinct, archival monochrome portraits receive subtle
            // warm/cool diffuse bounce, like painted figures inside the hall.
            // A true noninteractive light plane, not another fictitious hero.
            // Keep the silhouettes subdued but no longer crushed to black.
            if(slot>=2){
                var bounce=HallLight(viewport,"Hall / reserved portrait pictorial bounce",
                    w*.04f,h*.045f,w*.92f,h*.80f);
                bounce.color=slot%2==0?
                    new Color(.72f,.60f,.43f,.24f):
                    new Color(.49f,.57f,.65f,.24f);
            }
            // Decorative locks follow the approved source: small seals that
            // FLOAT over four independent monochrome paintings. The old Frame
            // built an opaque black box so large it hid each approved face.
            // The locking rule remains the real narrative/gameplay state.
            if(slot>=2){
                float seal=Mathf.Clamp(Mathf.Min(w,h)*.29f,28f,60f);
                float sealX=(w-seal)*.5f;
                float sealY=Mathf.Clamp(h*.70f-seal*.5f,3f,h-seal-4f);
                var plaque=Rect("Hall / genuine future hero lock seal",viewport);
                Place(plaque,sealX,sealY,seal,seal);
                var lockInk=new Color(.89f,.79f,.59f,.95f);
                var shadow=new Color(.018f,.022f,.030f,.52f);
                Surface("Hall / lock contrast only",plaque,seal*.27f,seal*.45f,
                    seal*.46f,seal*.40f,shadow).GetComponent<Image>().raycastTarget=false;
                Surface("Hall / bronze lock body",plaque,seal*.34f,seal*.48f,
                    seal*.32f,seal*.27f,lockInk).GetComponent<Image>().raycastTarget=false;
                Surface("Hall / lock left shackle",plaque,seal*.39f,seal*.30f,
                    seal*.07f,seal*.22f,lockInk).GetComponent<Image>().raycastTarget=false;
                Surface("Hall / lock right shackle",plaque,seal*.54f,seal*.30f,
                    seal*.07f,seal*.22f,lockInk).GetComponent<Image>().raycastTarget=false;
                Surface("Hall / lock shackle arch",plaque,seal*.39f,seal*.28f,
                    seal*.22f,seal*.08f,lockInk).GetComponent<Image>().raycastTarget=false;
            }
            // Do not draw a concept-sheet crop on top of this approved image:
            // that flattened the live hero identity and hid Lyra's real model.
            // Luminous sill is anchored to the portrait viewport and is
            // decorative only; it does not intercept a hero selection.
            Surface("Hall / portrait contact sill",viewport,0,h-4f,w,4f,
                new Color(.66f,.48f,.24f,unlocked ? .82f : .40f))
                .GetComponent<Image>().raycastTarget=false;
        }
        void BuildSalon(float w,float h)
        {
            // The exact approved 3x2 grammar: two REAL heroes and four inert
            // future slots. No new hero data, skills, resources or save writes.
            bool portraitLayout=h>w;
            bool phoneLandscape=!portraitLayout&&h<500f;
            bool illustratedDesktop=!portraitLayout&&!phoneLandscape&&h>=600f;
            // Art-first 1600x900 composition measured against the approved
            // first panel: 34% heraldic hall and categories, 52% layered
            // portrait gallery, 10% ornamental navigation. Do not solve
            // a composition failure through repeated micro-margin edits.
            float headerH=phoneLandscape?76f:illustratedDesktop?
                Mathf.Clamp(h*.34f,265f,345f):
                portraitLayout?Mathf.Clamp(h*.19f,130f,174f):142f;
            // Dedicated vertical-phone footer region; icons and live captions
            // need distinct physical rows above the screen safe edge.
            float footerH=phoneLandscape?64f:illustratedDesktop?88f:
                portraitLayout&&w<600f?90f:72f;
            // Title and actual player count are separate from the arch backdrop.
            // Reconstruct the 512x575 approved art composition as a protected
            // central stage on wide desktop. Stretching it to 1600 pixels
            // made six cards landscape-wide instead of portrait-led.
            float referenceStageWidth=illustratedDesktop?
                Mathf.Min(w-40f,h*(512f/575f)):w;
            float referenceStageLeft=(w-referenceStageWidth)*.5f;
            float titleX=phoneLandscape?18f:illustratedDesktop?
                referenceStageLeft+referenceStageWidth*.105f:42f;
            Label("Hall / Eldoria heraldry",content,"✦  ELDORIA",titleX,
                phoneLandscape?7f:illustratedDesktop?18f:13f,
                phoneLandscape?170f:245f,phoneLandscape?25f:illustratedDesktop?38f:33f,
                phoneLandscape?17:illustratedDesktop?27:24).color=gold;
            Label("Heroes hall title",content,"SALÓN DE HÉROES",titleX,
                phoneLandscape?30f:illustratedDesktop?62f:46f,
                phoneLandscape?w*.48f:illustratedDesktop?
                    referenceStageWidth*.88f:w*.66f,
                phoneLandscape?31f:illustratedDesktop?65f:47f,
                phoneLandscape?24:illustratedDesktop?46:34).color=ivory;
            if(!phoneLandscape)
                Label("Heroes hall introduction",content,
                    portraitLayout?"Reclútalos y forja su leyenda.":
                        illustratedDesktop?
                            "Aquí residen los héroes de Valoria.\nReclútalos, fortalécelos y lidera con ellos.":
                            "Aquí residen los héroes de Valoria. Reclútalos, fortalécelos y lidera con ellos.",
                    titleX,illustratedDesktop?143f:92f,
                    illustratedDesktop?Mathf.Min(referenceStageWidth*.87f,900f):
                        Mathf.Min(w*.65f,900f),
                    illustratedDesktop?77f:portraitLayout?25f:40f,
                    illustratedDesktop?24:portraitLayout?17:21,TextAnchor.MiddleLeft);
            // The reference contains four category tabs; only TODOS has a
            // backing game-state contract. Future categories are visibly
            // reserved, not fake functional filters or touch targets.
            string[] filters={"TODOS","COMBATE","APOYO","ESTRATEGIA"};
            // Four full-width category plaques reproduce the approved
            // reference rhythm instead of being a compressed right-corner
            // status strip. The desktop composition is primary; mobile gets
            // its own, separately validated positions.
            float barX=phoneLandscape?w*.52f:illustratedDesktop?
                referenceStageLeft+referenceStageWidth*.055f:
                portraitLayout?18f:w*.55f;
            float barY=phoneLandscape?32f:illustratedDesktop?
                headerH-70f:headerH-40f;
            float barW=phoneLandscape?w*.46f:illustratedDesktop?
                referenceStageWidth*.89f:portraitLayout?w-36f:w*.40f;
            float filterGap=4f,filterW=(barW-3*filterGap)/4f;
            for(int i=0;i<filters.Length;i++){
                var strip=Surface("Hall / category "+filters[i],content,
                    barX+i*(filterW+filterGap),barY,filterW,illustratedDesktop?49f:phoneLandscape?32f:31f,
                    i==0?new Color(.39f,.27f,.10f,.92f):new Color(.026f,.038f,.054f,.83f));
                strip.GetComponent<Image>().raycastTarget=false;
                HallControlArt(strip,i==0?"tab-active":"tab-idle",filterW,illustratedDesktop?49f:phoneLandscape?32f:31f);
                Surface("Hall / category top edge",strip,0,0,filterW,1.5f,
                    i==0?gold:new Color(.45f,.39f,.27f,.65f))
                    .GetComponent<Image>().raycastTarget=false;
                var label=Label("Hall / category label",strip,filters[i],3,3,filterW-6,
                    illustratedDesktop?43f:phoneLandscape?25f:24f,
                    illustratedDesktop?22:phoneLandscape?16:14,TextAnchor.MiddleCenter);
                label.color=i==0?ivory:new Color(.67f,.63f,.54f);
            }
            var heroes=ReadHeroes?.Invoke()??Array.Empty<HeroState>();
            // Desktop follows the approved three-column gallery. A real
            // narrow portrait phone needs two columns and three physical rows:
            // scaling three full cards down produced unreadable ribbons.
            // A 900x1600 phone has exactly the same tall aspect as 390x844.
            // Keeping three narrow columns on the former created portraits
            // three times taller than wide and made Aldric a pasted rectangle.
            // Preserve the approved three-column gallery on landscape/desktop;
            // use two columns and three tiers on BOTH tall phone resolutions.
            bool narrowPortrait=portraitLayout&&(w<600f||h>w*1.35f);
            int displayCount=Mathf.Max(6,heroes.Length);
            // Short mobile landscape features both genuine heroes above four
            // smaller, inert reserves; three equally wide cards caused
            // clipped real power and unreadable portrait identities.
            bool compactLandscape=phoneLandscape&&w<1050f&&displayCount==6;
            int columns=narrowPortrait?2:3;
            int rows=compactLandscape?2:Mathf.CeilToInt(displayCount/(float)columns);
            float gap=Mathf.Clamp(w*.017f,8f,18f);
            float cardTop=headerH+(illustratedDesktop?3f:phoneLandscape?8f:13f);
            float availableH=h-cardTop-footerH-14f;
            // Approved six-panel sheet gives the three featured portraits a
            // taller primary gallery; the future reserves form a lower,
            // shallower band. Uniform rows made the four empty reserves loom
            // as large as Aldric/Lyra and ignored the approved hierarchy.
            // Short mobile landscape keeps equal rows for legibility.
            float cardGridH=Mathf.Max(rows*78f,availableH-gap*(rows-1));
            bool tieredRows=(rows==2&&!phoneLandscape)||narrowPortrait||compactLandscape;
            float primaryH=compactLandscape?cardGridH*.60f:
                narrowPortrait?cardGridH*.46f:
                tieredRows?cardGridH*.625f:cardGridH/rows;
            float secondaryH=compactLandscape?cardGridH-primaryH:
                narrowPortrait?(cardGridH-primaryH)/Mathf.Max(1,rows-1):
                tieredRows?cardGridH-primaryH:cardGridH/rows;
            if(tieredRows&&!compactLandscape&&secondaryH<110f){
                secondaryH=110f;
                primaryH=cardGridH-secondaryH*(rows-1);
            }
            // Approved master: 134px-wide featured cards in a 512px-wide
            // composition. At 1600x900 the art-preserving scale derives
            // from HEIGHT (575px source), never from the full monitor width.
            float desktopCardWidth=illustratedDesktop?h*(134f/575f):390f;
            float standardCardW=Mathf.Min(desktopCardWidth,
                (w-42f-gap*(columns-1))/columns);
            // On a tall tablet the cathedral must remain visible around the portraits.
            // Tiny phones keep the full-width hit targets rather than shrinking them.
            if(portraitLayout&&!narrowPortrait)
                standardCardW=Mathf.Min(standardCardW,(w*.88f-gap*(columns-1))/columns);
            if(!hallFrameSprite){
                var metal=Resources.Load<Texture2D>("EldoriaV41/HeroCardNineSliceGold");
                if(metal)hallFrameSprite=Sprite.Create(metal,
                    new Rect(0,0,metal.width,metal.height),
                    new Vector2(.5f,.5f),300f,0,SpriteMeshType.FullRect,
                    new Vector4(112,112,112,112));
            }
            if(!futureSlotArt){
                var source=Resources.Load<Texture2D>("EldoriaV41/hero-locked");
                if(source)futureSlotArt=Sprite.Create(source,new Rect(0,0,source.width,source.height),
                    new Vector2(.5f,.5f),100,0,SpriteMeshType.FullRect);
            }
            for(int i=0;i<displayCount;i++){
                int col=compactLandscape?(i<2?i:i-2):i%columns;
                int row=compactLandscape?(i<2?0:1):i/columns;
                int groupColumns=compactLandscape?(i<2?2:4):columns;
                float cardW=compactLandscape?
                    Mathf.Min(i<2?390f:200f,
                        (w-42f-gap*(groupColumns-1))/groupColumns):
                    standardCardW;
                float cardH=tieredRows?(row==0?primaryH:secondaryH):primaryH;
                float left=(w-(cardW*groupColumns+gap*(groupColumns-1)))*.5f+
                    col*(cardW+gap);
                float top=cardTop+(compactLandscape?(row==0?0f:primaryH+gap):
                    tieredRows?(row==0?0f:primaryH+gap+(row-1)*(secondaryH+gap)):
                    row*(cardH+gap));
                bool real=i<heroes.Length;
                bool compactReserve=compactLandscape&&i>=2;
                Surface("Hall / card cast shadow",content,left+5f,top+7f,
                    cardW,cardH,new Color(0f,0f,0f,.34f))
                    .GetComponent<Image>().raycastTarget=false;
                var card=Frame(real?"Hero card "+heroes[i].Id:"Reserved future hero slot "+(i-heroes.Length),
                    content,left,top,cardW,cardH);
                var stageInk=card.Find("Royal Gold / translucent ink");
                if(stageInk)stageInk.GetComponent<Image>().color=
                    new Color(.047f,.061f,.078f,.32f);
                // Reuse the proven 2.5D Royal Gold assembly, including
                // separate metallic light and shadow planes on EACH card.
                // This makes the physical card a layered object in the hall,
                // rather than a flat rectangle over a photograph. It cannot
                // intercept input and never owns state.
                RoyalGoldV3LayeredFinish.Install(card,
                    RoyalGoldV3LayeredFinish.Scene.Panel,card.childCount);
                // A freestanding engraved frame, not an opaque UI rectangle
                // flattened into the architectural background.
                Surface("Hall / card inner bronze bevel",card,4f,4f,cardW-8f,2f,
                    new Color(.84f,.60f,.24f,.75f)).GetComponent<Image>().raycastTarget=false;
                Surface("Hall / card left bevel",card,4f,6f,2f,cardH-12f,
                    new Color(.82f,.55f,.21f,.43f)).GetComponent<Image>().raycastTarget=false;
                Surface("Hall / card right bevel",card,cardW-6f,6f,2f,cardH-12f,
                    new Color(.82f,.55f,.21f,.43f)).GetComponent<Image>().raycastTarget=false;
                bool unlocked=real&&heroes[i].Unlocked;
                Sprite portrait=real?heroes[i].Portrait:(ApprovedFuturePortrait(i-heroes.Length)??futureSlotArt);
                bool lateral=phoneLandscape;
                float portraitX=6f,portraitY=6f;
                float portraitW=lateral?
                    Mathf.Min(cardW*(compactReserve?.32f:.53f),
                        cardH*(compactReserve?.78f:1.80f)):
                    cardW-12f;
                // Future portraits need more of their image and less opaque UI ink.
                // The two real identities retain enough room for genuine name,
                // role, level and power; locked slots have only two short labels.
                float identityH=lateral?0f:!real?
                    Mathf.Clamp(cardH*.32f,60f,78f):
                    cardW<220f?92f:105f;
                float portraitH=lateral?cardH-12f:Mathf.Max(24f,cardH-identityH-12f);
                CardPortrait(card,portrait,unlocked,!real||heroes[i].Id=="lyra",
                    i,portraitX,portraitY,portraitW,portraitH);
                // Phone landscape owns a real safe text gutter. The old 5px right
                // allowance let gold corners cover actual hero power digits.
                float ix=lateral?portraitW+(compactReserve?10f:16f):5f;
                float iy=lateral?4f:cardH-identityH;
                float iw=lateral?cardW-portraitW-(compactReserve?22f:34f):cardW-10f;
                float ih=lateral?cardH-8f:identityH-5f;
                Surface("Hero card identity shade",card,ix,iy,iw,ih,
                    new Color(.033f,.041f,.054f,lateral ? .83f : .81f))
                    .GetComponent<Image>().raycastTarget=false;
                // Art-first desktop cards are ~210 units wide but tall. They
                // must use the full hero identity hierarchy, not the narrow
                // 390px phone's compact 13px text branch.
                bool compactCard=!lateral&&cardW<200f;
                if(real){
                    var namePlate=Label("Hero card name",card,heroes[i].DisplayName,ix+3f,
                        iy+(lateral?5f:2f),iw-6f,
                        lateral?Mathf.Min(39f,ih*.44f):compactCard?23f:28f,
                        lateral?22:cardW<150?17:23,
                        TextAnchor.MiddleCenter);
                    namePlate.color=gold;
                    if(lateral)namePlate.resizeTextForBestFit=false;
                    bool largeCard=!lateral&&cardH>=260f&&identityH>=72f&&cardW>=200f;
                    if(largeCard)
                        Label("Hero / authoritative role",card,heroes[i].Role,
                            ix+3f,iy+29f,iw-6f,17f,15,TextAnchor.MiddleCenter)
                            .color=new Color(.81f,.75f,.63f);
                    var statePlate=Label("Hero card state",card,
                        unlocked?(lateral?"NV. "+heroes[i].Level+"\nPODER "+heroes[i].HeroPower:
                            compactCard?"NV. "+heroes[i].Level+"\nPODER "+heroes[i].HeroPower:
                            largeCard?"NV. "+heroes[i].Level+"\nPODER "+heroes[i].HeroPower:
                            "NIVEL "+heroes[i].Level+"  ·  PODER "+heroes[i].HeroPower):
                            (compactCard?"BLOQUEADO\nPOR HISTORIA":"BLOQUEADO POR LA HISTORIA"),
                        ix+3f,iy+(lateral?ih*.49f:largeCard?46f:compactCard?26f:30f),iw-6f,
                        lateral?Mathf.Max(40f,ih*.48f):largeCard?38f:
                            compactCard?Mathf.Max(36f,identityH-46f):Mathf.Max(20f,identityH-39f),
                        lateral?19:compactCard?13:largeCard?14:17,TextAnchor.MiddleCenter);
                    // At 512:575 reference proportions, real hero power must
                    // remain on ONE visible line instead of wrapping under the
                    // image and being truncated by its 20-unit stat row.
                    if(lateral||largeCard){
                        statePlate.resizeTextForBestFit=false;
                        if(largeCard){
                            // Reserve two independent, visible stat lines below
                            // the role. Long one-line power silently wrapped
                            // and got truncated by Unity Text at 1600x900.
                            statePlate.horizontalOverflow=HorizontalWrapMode.Wrap;
                            statePlate.verticalOverflow=VerticalWrapMode.Truncate;
                        }
                    }
                } else {
                    // The old two-line POR / DESCUBRIR caption consumed the
                    // entire reserved card on a 390px phone. An anonymous,
                    // distinct illustrated face is the focal point instead.
                    bool tightLocked=!lateral&&identityH<81f;
                    var lockedTitle=Label("Hero card name",card,
                        lateral?"POR\nDESCUBRIR":"POR DESCUBRIR",ix+3f,
                        iy+(tightLocked?2f+Mathf.Max(0f,(identityH-60f)*.65f):3f),iw-6f,
                        lateral?Mathf.Max(43f,ih*.49f):
                            tightLocked?22f:27f,
                        compactReserve?15:lateral&&cardW<210f?16:lateral?19:
                            tightLocked?cardW<220f?14:17:19,TextAnchor.MiddleCenter);
                    lockedTitle.color=new Color(.86f,.77f,.57f);
                    var lockedState=Label("Hero card state",card,"BLOQUEADO",ix+3f,
                        iy+(lateral?ih*.58f:tightLocked?26f+Mathf.Max(0f,(identityH-60f)*.65f):30f),iw-6f,
                        lateral?Mathf.Max(27f,ih*.34f):
                            tightLocked?16f:
                            Mathf.Max(18f,identityH-38f),
                        compactReserve?15:lateral&&cardW<210f?16:lateral?19:
                            tightLocked?13:15,TextAnchor.MiddleCenter);
                    lockedState.color=new Color(.79f,.77f,.73f);
                    if(lateral){lockedTitle.resizeTextForBestFit=false;lockedState.resizeTextForBestFit=false;}
                }
                // Authored 9-slice metallic artwork is a reusable,
                // center-transparent material frame, NEVER a flattened card.
                // Preserves live character renders, names and hitboxes across
                // all four responsive resolutions; ornamented corners retain
                // their size while the card can stretch naturally.
                if(hallFrameSprite){
                    var frameArt=Rect("Hall / sculpted gilded card artwork",card);
                    Place(frameArt,0f,0f,cardW,cardH);
                    var metal=frameArt.gameObject.AddComponent<Image>();
                    metal.sprite=hallFrameSprite;
                    metal.type=Image.Type.Sliced;
                    // The original full-size gold corner engraving occupies
                    // the entire narrow portrait card if left at desktop
                    // pixel units. Keep the same approved nine-slice texture
                    // but scale only its borders on 2-column mobile layouts.
                    metal.pixelsPerUnitMultiplier=cardW<220f?2.2f:1f;
                    metal.fillCenter=false;
                    metal.raycastTarget=false;
                    metal.color=unlocked?Color.white:
                        new Color(.73f,.70f,.64f,.90f);
                }
                // Retain the original thin structural rim behind authored art.
                Surface("Hall / card heraldic top",card,8f,6f,
                    Mathf.Max(12f,cardW-16f),1.5f,
                    new Color(.89f,.72f,.38f,unlocked ? .82f : .37f))
                    .GetComponent<Image>().raycastTarget=false;
                if(real){
                    var hero=heroes[i];
                    var hit=Surface("Select hero "+hero.Id,card,0,0,cardW,cardH,Color.clear);
                    var button=hit.gameObject.AddComponent<Button>();
                    button.targetGraphic=hit.GetComponent<Image>();
                    button.interactable=unlocked;
                    button.onClick.AddListener(()=>OpenHero(hero.Id));
                }
            }
            // Canonical bottom action bar follows the approved four-destination
            // grammar. VOLVER and MARCHAS are REAL, HEROES stays on this live
            // view, and RELICARIO is genuinely disabled by progression.
            // No fake clickable pixels, no additional scene or canvas.
            float navGap=portraitLayout&&w<600f?4f:phoneLandscape?6f:10f;
            float navWidth=Mathf.Min(w-(portraitLayout&&w<600f?12f:22f),illustratedDesktop?
                referenceStageWidth:portraitLayout?540f:820f);
            float navCell=(navWidth-navGap*3f)/4f;
            float navLeft=(w-navWidth)*.5f;
            float navHeight=phoneLandscape?44f:illustratedDesktop?58f:
                portraitLayout&&w<600f?76f:47f;
            float navY=h-footerH+(phoneLandscape?9f:illustratedDesktop?15f:
                portraitLayout&&w<600f?6f:12f);
            var navBack=Button("Hall / nav return","VOLVER",navLeft,navY,navCell,navHeight,Close);
            HallControlArt(navBack.transform,"tab-idle",navCell,navHeight,true);
            HallNavIcon(navBack,12,navCell,navHeight,false);
            Surface("Hall / selected heroes tab glow",content,
                navLeft+navCell+navGap,navY,navCell,navHeight,
                new Color(.47f,.25f,.065f,.91f)).GetComponent<Image>().raycastTarget=false;
            var selected=Button("Hall / nav heroes",
                "HÉROES",
                navLeft+navCell+navGap,navY,navCell,navHeight,()=>Select(Page.Salon));
            HallControlArt(selected.transform,"gold-button",navCell,navHeight,true);
            HallNavIcon(selected,4,navCell,navHeight,true);
            // The selected gold plate is luminous: use dark ink rather than
            // pale yellow-on-gold, which was illegible on small devices.
            foreach(var t in selected.GetComponentsInChildren<Text>())
                t.color=new Color(.064f,.048f,.026f);
            var navMarches=Button("Hall / nav marches","MARCHAS",
                navLeft+(navCell+navGap)*2f,navY,navCell,navHeight,()=>{
                    var management=GetComponent<RoyalGoldV3ManagementScreens>();
                    if(management){Suspend();management.Open(RoyalGoldV3ManagementScreens.Destination.March);}
                });
            HallControlArt(navMarches.transform,"tab-idle",navCell,navHeight,true);
            HallNavIcon(navMarches,5,navCell,navHeight,false);
            var navRelicary=Button("Hall / nav reliquary","RELICARIO\nPRÓXIMO",
                navLeft+(navCell+navGap)*3f,navY,navCell,navHeight,()=>{},false);
            HallControlArt(navRelicary.transform,"tab-idle",navCell,navHeight,true);
            HallNavIcon(navRelicary,7,navCell,navHeight,false);
        }
        void BuildReservedTalents(float x,float y,float w,float h,bool portrait)
        {
            // Owner-approved three branches / nine disabled nodes; all previous
            // gameplay, source portraits, navigation, and saves remain untouched.
            // Nine distinct metallic medallion sprites and a layered header,
            // connector and ornamental nine-slice frame are authored separately.
            // Do not use a flattened reference sheet as a clickable backdrop.
            var board=Frame("Heroes / reserved talent paths",content,x,y,w,h);
            HeroesTalentArt.Build(board,w,h,portrait);
        }
        void BuildInformation(RectTransform pane,HeroState hero,float w,float h)
        {
            if(CurrentPage==Page.Resumen){
                BuildCombatSummary(pane,hero,w,h);
            }else if(CurrentPage==Page.Habilidades){
                // Exactly one source-bound ability. Decorative reserve cards are
                // inert; navigation, models, progression and saves are untouched.
                HeroesSkillCards.Build(pane,hero.Id,hero.AbilityName,
                    hero.AbilityDescription,w,h,hallFrameSprite,
                    RealAbilityArtwork(hero.Id));
            }else if(CurrentPage==Page.Equipamiento){
                // Four pictorial locked positions frame the original live model.
                // The contextual engraving is translucent and creates no actions.
                HeroesEquipmentArt.BuildContext(pane,w,h);
            }else{
                Label("Unavailable system title",pane,"TALENTOS",16,18,w-32,52,24).color=gold;
                Label("Unavailable system state",pane,"Aún no disponible en esta etapa del reino.",16,90,w-32,Mathf.Max(60,h-112),22);
            }
        }
        void ScrollDescription(RectTransform pane,string text,float x,float y,float w,float h)
        {
            // The host supplies the complete real description. Long localized
            // text scrolls rather than being clipped to a concept-art card.
            var viewport=Surface("Ability description viewport",pane,x,y,w,h,Color.clear);
            viewport.gameObject.AddComponent<RectMask2D>();
            var scroller=viewport.gameObject.AddComponent<ScrollRect>();
            var body=Rect("Ability description content",viewport);
            Place(body,0,0,w,h);
            var description=Label("Canonical hero ability description",body,text,0,0,w,h,20,TextAnchor.UpperLeft);
            description.resizeTextForBestFit=false;
            float textHeight=Mathf.Max(h,description.preferredHeight+12f);
            Place(body,0,0,w,textHeight);Place(description.rectTransform,0,0,w,textHeight);
            scroller.viewport=viewport;scroller.content=body;
            scroller.horizontal=false;scroller.vertical=true;
            scroller.movementType=ScrollRect.MovementType.Clamped;
            scroller.scrollSensitivity=24f;
        }
        void CycleHero(int direction)
        {
            var heroes=ReadHeroes?.Invoke()??Array.Empty<HeroState>();int current=Array.FindIndex(heroes,h=>h.Id==CurrentHeroId);
            for(int n=1;n<=heroes.Length;n++){int i=(current+direction*n+heroes.Length*2)%heroes.Length;if(heroes[i].Unlocked){OpenHero(heroes[i].Id);return;}}
        }
    }
}

