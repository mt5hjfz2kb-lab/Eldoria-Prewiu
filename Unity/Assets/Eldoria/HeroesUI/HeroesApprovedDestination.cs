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
        public RectTransform VisibleRoot => IsOpen ? root : null;
        RectTransform root, content;
        HeroesModelSnapshot preview;
        Font font;
        float observedWidth, observedHeight;
        readonly Color gold = new Color(.78f,.63f,.37f), ivory = new Color(.96f,.91f,.80f),
            midnight = new Color(.035f,.049f,.069f), panel = new Color(.065f,.079f,.099f);

        public void Initialize(RectTransform safeArea, Func<HeroState[]> readHeroes)
        {
            if (!safeArea || readHeroes == null) throw new ArgumentNullException();
            ExistingHudSafeArea=safeArea;ReadHeroes=readHeroes;
            font=Resources.Load<Font>("EldoriaRetention/reference-serif") ?? Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
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
        public void Close(){if(!IsOpen)return;Suspend();Closed?.Invoke();}
        public HeroState? CurrentAuthoritativeHero()
        {foreach(var h in ReadHeroes?.Invoke()??Array.Empty<HeroState>())if(h.Id==CurrentHeroId)return h;return null;}
        public void Refresh(){if(IsOpen)Rebuild();}
        void LateUpdate()
        {
            if(IsOpen&&(Mathf.Abs(root.rect.width-observedWidth)>1f||Mathf.Abs(root.rect.height-observedHeight)>1f))Rebuild();
            if(IsOpen&&CurrentPage==Page.Salon&&leftHallLight&&rightHallLight){
                float t=Time.unscaledTime;
                var left=leftHallLight.color;
                left.a=.24f+.025f*Mathf.Sin(t*1.7f);
                leftHallLight.color=left;
                var right=rightHallLight.color;
                right.a=.20f+.023f*Mathf.Sin(t*1.5f+1.3f);
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
            t.font=font;t.text=value??"";t.fontSize=size;t.color=ivory;t.alignment=align;t.raycastTarget=false;
            t.horizontalOverflow=HorizontalWrapMode.Wrap;t.verticalOverflow=VerticalWrapMode.Truncate;
            t.resizeTextForBestFit=true;t.resizeTextMinSize=12;t.resizeTextMaxSize=size;return t;
        }
        Button Button(string name,string caption,float x,float y,float w,float h,Action action,bool enabled=true)
        {
            var r=Frame(name,content,x,y,w,h);var b=r.gameObject.AddComponent<Button>();b.interactable=enabled;
            b.onClick.AddListener(()=>action());var t=Label(name+" label",r,caption,5,3,w-10,h-6,20,TextAnchor.MiddleCenter);
            t.color=enabled?ivory:new Color(.48f,.48f,.48f);return b;
        }
        void Rebuild()
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
            Button("Heroes hall","‹",20,14,48,48,()=>Select(Page.Salon));
            Label("Hero name",content,hero.DisplayName?.ToUpperInvariant(),82,12,w-164,36,30,TextAnchor.MiddleCenter).color=gold;
            Label("Hero role and real level",content,hero.Role+" · NIVEL "+hero.Level,82,48,w-164,26,20,TextAnchor.MiddleCenter);
            var pages=new[]{Page.Resumen,Page.Habilidades,Page.Talentos,Page.Equipamiento};
            string[] captions={"RESUMEN","HABILIDADES","TALENTOS","EQUIPAMIENTO"};
            bool shortProfile=!portrait&&h<500f;
            float tabWidth=portrait?(w-46)/2f:Mathf.Clamp(w*.16f,140,190),top=portrait?210:shortProfile?96:100;
            for(int i=0;i<pages.Length;i++){
                var page=pages[i];float x=portrait?20+(i%2)*(tabWidth+6):20,y=portrait?96+(i/2)*49:shortProfile?96+i*46:110+i*62;
                var b=Button("Hero tab "+page,captions[i],x,y,tabWidth,portrait?43:shortProfile?40:52,()=>Select(page));
                if(CurrentPage==page)b.targetGraphic.color=new Color(.95f,.78f,.43f);
            }
            float bodyX=portrait?20:tabWidth+36,bodyW=w-bodyX-20;
            float bodyH=h-top-92,modelW=portrait?bodyW:bodyW*.43f,modelH=portrait?bodyH*.62f:bodyH;
            var stage=Rect("Hero model stage",content);
            Place(stage,bodyX,top,modelW,modelH);
            var source=ReadModelRoot?.Invoke(hero.Id) ?? (hero.Id=="aldric"?ApprovedAldricModel:ApprovedLyraModel);
            var raw=Rect("Genuine hero model render",stage).gameObject.AddComponent<RawImage>();
            Place(raw.rectTransform,5,5,modelW-10,modelH-46);raw.raycastTarget=false;
            if(source&&preview.Show(source,raw,hero.Id=="lyra"?30f:90f)){
                float imageH=modelH-46,imageW=Mathf.Min(modelW-10,imageH*2f/3f);
                Place(raw.rectTransform,(modelW-imageW)*.5f,5,imageW,imageH);
                if(CurrentPage!=Page.Talentos){
                    Button("Rotate hero left","‹",bodyX+7,top+modelH-78,32,32,()=>{preview.Rotate(-30);Rebuild();});
                    Button("Rotate hero right","›",bodyX+modelW-39,top+modelH-78,32,32,()=>{preview.Rotate(30);Rebuild();});
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
                }
                else Label("Hero identity without substitute art",stage,hero.DisplayName,14,modelH*.35f,modelW-28,70,25,TextAnchor.MiddleCenter);
            }
            Label("Real hero availability",stage,hero.Available?"EN VALORIA":"EN EXPEDICIÓN",8,modelH-37,modelW-16,30,16,TextAnchor.MiddleCenter);
            float infoX=portrait?bodyX:bodyX+modelW+12,infoY=portrait?top+modelH+12:top;
            float infoW=portrait?bodyW:bodyW-modelW-12,infoH=portrait?bodyH-modelH-12:bodyH;
            var info=Frame("Hero authoritative information",content,infoX,infoY,infoW,infoH);
            BuildInformation(info,hero,infoW,infoH);
            if(CurrentPage==Page.Talentos){
                stage.gameObject.SetActive(false);info.gameObject.SetActive(false);
                BuildReservedTalents(bodyX,top,bodyW,bodyH,portrait);
            }
            Button("Previous available hero","‹",20,h-66,64,46,()=>CycleHero(-1));
            Label("Current hero identity",content,hero.DisplayName,94,h-66,w-188,46,22,TextAnchor.MiddleCenter).color=gold;
            Button("Next available hero","›",w-84,h-66,64,46,()=>CycleHero(1));
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
            bool stacked=cell<145f;
            float size=Mathf.Min(stacked?20f:23f,height*(stacked?.44f:.50f));
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
            glyph.fontSize=25;
            glyph.resizeTextForBestFit=false;
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
                    Place(caption.rectTransform,3f,size+4f,
                        cell-6f,Mathf.Max(17f,height-size-6f));
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
        Material hallPaintMaterial, hallPortraitPaintMaterial, hallAldricMatteMaterial;
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
        void OnDestroy(){if(hallFrameSprite)Destroy(hallFrameSprite);if(hallLightTexture)Destroy(hallLightTexture);if(hallPortraitTransition)Destroy(hallPortraitTransition);if(hallPaintMaterial)Destroy(hallPaintMaterial);if(hallPortraitPaintMaterial)Destroy(hallPortraitPaintMaterial);if(hallAldricMatteMaterial)Destroy(hallAldricMatteMaterial);}
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
            leftHallLight.color=new Color(1f,.53f,.17f,.26f);
            rightHallLight.color=new Color(1f,.65f,.27f,.21f);
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
                new Color(.012f,.022f,.036f,.77f)).GetComponent<Image>().raycastTarget=false;
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
                new Color(.010f,.019f,.034f,widePhone ? .75f : .66f))
                .GetComponent<Image>().raycastTarget=false;
            Surface("Hall / header gilding",content,12f,5f,w-24f,2f,
                new Color(.81f,.60f,.29f,.75f)).GetComponent<Image>().raycastTarget=false;
        }
        Sprite futureSlotArt, hallFrameSprite;
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
                float border=(1f-t*t*(3f-2f*t))*.96f;
                float skyRight=Mathf.Clamp01((u-.59f)/.34f)*
                    Mathf.Clamp01((v-.36f)/.43f);
                float alpha=Mathf.Max(border,skyRight*.84f);
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
                new Color(.055f,.083f,.115f,1f)).GetComponent<Image>().raycastTarget=false;
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
                alcove.uvRect=slot==0?new UnityEngine.Rect(.03f,.22f,.30f,.56f):
                    slot==1?new UnityEngine.Rect(.64f,.22f,.30f,.56f):
                    new UnityEngine.Rect(.36f,.13f,.29f,.62f);
                alcove.color=slot<2?
                    new Color(.88f,.85f,.86f,.62f):
                    new Color(.62f,.67f,.76f,.30f);
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
                float top=fullBody?-(imageH-h)*.27f:
                    imageH>h?-Mathf.Min(6f,(imageH-h)*.06f):
                    (h-imageH)*.5f;
                Place(image.rectTransform,(w-imageW)*.5f,top,imageW,imageH);
                image.sprite=sprite;image.raycastTarget=false;
                image.color=unlocked?Color.white:new Color(.42f,.46f,.51f,.90f);
                if(slot<2 && hallPaintMaterial){
                    // Render approved source sprites as illustrated character
                    // cards. Originals, Tripo geometry and saved hero identities
                    // remain untouched; only card material is painted.
                    // ClipRect support keeps Lyra safely inside its 2.5D frame.
                    if(!hallPortraitPaintMaterial){
                        hallPortraitPaintMaterial=new Material(hallPaintMaterial){
                            name="Eldoria / illustrated canonical hero card",
                            hideFlags=HideFlags.DontSave};
                        hallPortraitPaintMaterial.SetFloat("_Strength",.28f);
                    }
                    if(slot==0){
                        if(!hallAldricMatteMaterial){
                            hallAldricMatteMaterial=new Material(hallPortraitPaintMaterial){
                                name="Eldoria / authored Aldric soft-edge stage",
                                hideFlags=HideFlags.DontSave};
                            hallAldricMatteMaterial.SetFloat("_PortraitFeather",.14f);
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
            // Future hero silhouettes are an illustrated reserve, not
            // unfinished game buttons. The existing 'hero-locked' picture
            // contains a large placeholder PLUS glyph; place an independent
            // physical bronze padlock seal over it as in the approved sheet.
            // The silhouette remains intact, no fake unlock action exists.
            if(slot>=2){
                float seal=Mathf.Clamp(Mathf.Min(w,h)*.46f,28f,104f);
                float sealX=(w-seal)*.5f;
                float sealY=Mathf.Clamp(h*.75f-seal*.5f,3f,h-seal-4f);
                var plaque=Frame("Hall / genuine future hero lock seal",viewport,
                    sealX,sealY,seal,seal);
                var deep=plaque.GetComponent<Image>();
                deep.raycastTarget=false;
                foreach(var g in plaque.GetComponentsInChildren<Graphic>())
                    g.raycastTarget=false;
                var lockInk=new Color(.68f,.53f,.29f,.90f);
                Surface("Hall / bronze lock body",plaque,seal*.34f,seal*.49f,
                    seal*.32f,seal*.29f,lockInk).GetComponent<Image>().raycastTarget=false;
                Surface("Hall / lock left shackle",plaque,seal*.39f,seal*.31f,
                    seal*.07f,seal*.21f,lockInk).GetComponent<Image>().raycastTarget=false;
                Surface("Hall / lock right shackle",plaque,seal*.54f,seal*.31f,
                    seal*.07f,seal*.21f,lockInk).GetComponent<Image>().raycastTarget=false;
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
            float footerH=phoneLandscape?64f:illustratedDesktop?88f:72f;
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
            bool narrowPortrait=portraitLayout&&w<600f;
            int displayCount=Mathf.Max(6,heroes.Length);
            int columns=narrowPortrait?2:3;
            int rows=Mathf.CeilToInt(displayCount/(float)columns);
            float gap=Mathf.Clamp(w*.017f,8f,18f);
            float cardTop=headerH+(illustratedDesktop?3f:phoneLandscape?8f:13f);
            float availableH=h-cardTop-footerH-14f;
            // Approved six-panel sheet gives the three featured portraits a
            // taller primary gallery; the future reserves form a lower,
            // shallower band. Uniform rows made the four empty reserves loom
            // as large as Aldric/Lyra and ignored the approved hierarchy.
            // Short mobile landscape keeps equal rows for legibility.
            float cardGridH=Mathf.Max(rows*78f,availableH-gap*(rows-1));
            bool tieredRows=(rows==2&&!phoneLandscape)||narrowPortrait;
            float primaryH=narrowPortrait?cardGridH*.40f:
                tieredRows?cardGridH*.625f:cardGridH/rows;
            float secondaryH=narrowPortrait?(cardGridH-primaryH)/Mathf.Max(1,rows-1):
                tieredRows?cardGridH-primaryH:cardGridH/rows;
            if(tieredRows&&secondaryH<110f){
                secondaryH=110f;
                primaryH=cardGridH-secondaryH*(rows-1);
            }
            // Approved master: 134px-wide featured cards in a 512px-wide
            // composition. At 1600x900 the art-preserving scale derives
            // from HEIGHT (575px source), never from the full monitor width.
            float desktopCardWidth=illustratedDesktop?h*(134f/575f):390f;
            float cardW=Mathf.Min(desktopCardWidth,
                (w-42f-gap*(columns-1))/columns);
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
                int col=i%columns,row=i/columns;
                float cardH=tieredRows?(row==0?primaryH:secondaryH):primaryH;
                float left=(w-(cardW*columns+gap*(columns-1)))*.5f+col*(cardW+gap);
                float top=cardTop+(tieredRows?
                    (row==0?0f:primaryH+gap+(row-1)*(secondaryH+gap)):
                    row*(cardH+gap));
                bool real=i<heroes.Length;
                Surface("Hall / card cast shadow",content,left+5f,top+7f,
                    cardW,cardH,new Color(0f,0f,0f,.60f))
                    .GetComponent<Image>().raycastTarget=false;
                var card=Frame(real?"Hero card "+heroes[i].Id:"Reserved future hero slot "+(i-heroes.Length),
                    content,left,top,cardW,cardH);
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
                Sprite portrait=real?heroes[i].Portrait:futureSlotArt;
                bool lateral=phoneLandscape;
                float portraitX=6f,portraitY=6f;
                float portraitW=lateral?Mathf.Min(cardH-12f,cardW*.44f):cardW-12f;
                float identityH=lateral?0f:cardH<160f?43f:cardW<220f?85f:74f;
                float portraitH=lateral?cardH-12f:Mathf.Max(24f,cardH-identityH-12f);
                CardPortrait(card,portrait,unlocked,!real||heroes[i].Id=="lyra",
                    i,portraitX,portraitY,portraitW,portraitH);
                float ix=lateral?portraitW+10f:5f;
                float iy=lateral?4f:cardH-identityH;
                float iw=lateral?cardW-portraitW-15f:cardW-10f;
                float ih=lateral?cardH-8f:identityH-5f;
                Surface("Hero card identity shade",card,ix,iy,iw,ih,
                    new Color(.008f,.016f,.027f,lateral ? .84f : .91f))
                    .GetComponent<Image>().raycastTarget=false;
                bool compactCard=!lateral&&cardW<220f;
                if(real){
                    var namePlate=Label("Hero card name",card,heroes[i].DisplayName,ix+3f,
                        iy+(lateral?5f:2f),iw-6f,
                        lateral?Mathf.Min(39f,ih*.44f):compactCard?23f:28f,
                        lateral?25:cardW<150?17:23,
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
                            "NIVEL "+heroes[i].Level+"  ·  PODER "+heroes[i].HeroPower):
                            (compactCard?"BLOQUEADO\nPOR HISTORIA":"BLOQUEADO POR LA HISTORIA"),
                        ix+3f,iy+(lateral?ih*.49f:largeCard?49f:compactCard?26f:30f),iw-6f,
                        lateral?Mathf.Max(40f,ih*.48f):largeCard?20f:
                            compactCard?Mathf.Max(36f,identityH-46f):Mathf.Max(20f,identityH-39f),
                        lateral?20:compactCard?13:17,TextAnchor.MiddleCenter);
                    if(lateral)statePlate.resizeTextForBestFit=false;
                } else {
                    var lockedTitle=Label("Hero card name",card,
                        lateral?"POR\nDESCUBRIR":compactCard?"POR\nDESCUBRIR":"POR DESCUBRIR",ix+3f,
                        iy+(lateral?3f:compactCard?4f:3f),iw-6f,
                        lateral?Mathf.Max(43f,ih*.49f):compactCard?40f:27f,
                        lateral?19:compactCard?13:19,TextAnchor.MiddleCenter);
                    lockedTitle.color=new Color(.78f,.69f,.51f);
                    var lockedState=Label("Hero card state",card,"BLOQUEADO",ix+3f,
                        iy+(lateral?ih*.58f:compactCard?46f:30f),iw-6f,
                        lateral?Mathf.Max(27f,ih*.34f):
                            compactCard?22f:Mathf.Max(18f,identityH-38f),
                        lateral?19:compactCard?12:15,TextAnchor.MiddleCenter);
                    lockedState.color=new Color(.77f,.76f,.70f);
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
                        new Color(.60f,.67f,.78f,.82f);
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
            float navGap=phoneLandscape?6f:10f;
            float navWidth=Mathf.Min(w-22f,illustratedDesktop?
                referenceStageWidth:portraitLayout?540f:820f);
            float navCell=(navWidth-navGap*3f)/4f;
            float navLeft=(w-navWidth)*.5f;
            float navHeight=phoneLandscape?44f:illustratedDesktop?58f:47f;
            float navY=h-footerH+(phoneLandscape?9f:illustratedDesktop?15f:12f);
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
            var navRelicary=Button("Hall / nav reliquary","RELICARIO",
                navLeft+(navCell+navGap)*3f,navY,navCell,navHeight,()=>{},false);
            HallControlArt(navRelicary.transform,"tab-idle",navCell,navHeight,true);
            HallNavIcon(navRelicary,7,navCell,navHeight,false);
        }
        void BuildReservedTalents(float x,float y,float w,float h,bool portrait)
        {
            // These are the three reserved visual paths in the exact approved
            // master. No talent data, level, price or bonus exists in the host.
            // All nodes remain disabled and create no command or save state.
            var board=Frame("Heroes / reserved talent paths",content,x,y,w,h);
            float gap=portrait?12f:18f,trackW=(w-32-2*gap)/3f;
            bool compact=h<260f;
            float noteH=compact?52f:portrait?120f:90f,trackH=h-noteH-28;
            string[] titles={"OFENSIVA","DEFENSA","SOPORTE"};
            Color[] accents={new Color(.76f,.24f,.12f),new Color(.17f,.51f,.90f),gold};
            for(int path=0;path<3;path++){
                float left=16+path*(trackW+gap);
                var track=Frame("Reserved talent path "+titles[path],board,left,14,trackW,trackH);
                TintFrame(track,accents[path]);
                Label("Reserved path name",track,titles[path],6,compact?4:14,trackW-12,compact?28:56,compact?18:26,TextAnchor.MiddleCenter).color=ivory;
                float nodeSize=compact?Mathf.Min(32f,trackW*.22f):Mathf.Min(76f,trackW*.55f,Mathf.Max(28f,(trackH-110f)/3.4f));
                float start=compact?44f:80f;
                float step=Mathf.Max(0f,(trackH-start-nodeSize-18f)/2f);
                float center=trackW*.5f;
                Surface("Path connection",track,compact?trackW*.18f:center-1.5f,start+nodeSize*.5f,compact?trackW*.64f:3,compact?3:2*step,accents[path]).GetComponent<Image>().raycastTarget=false;
                for(int rank=0;rank<3;rank++){
                    float nodeY=compact?start:start+rank*step;
                    float nodeX=compact?trackW*(.18f+rank*.32f)-nodeSize*.5f:center-nodeSize*.5f;
                    var node=Frame("Reserved talent node "+path+"-"+rank,track,nodeX,nodeY,nodeSize,nodeSize);
                    TintFrame(node,accents[path]);
                    var locked=node.gameObject.AddComponent<Button>();locked.interactable=false;
                    // Native uGUI lock geometry, not a baked mock level or icon
                    // claiming an implemented talent.
                    float lockW=nodeSize*.42f,lockH=nodeSize*.36f,lockX=(nodeSize-lockW)*.5f;
                    var lockColor=new Color(.58f,.57f,.54f);
                    Surface("Lock body",node,lockX,nodeSize*.46f,lockW,lockH,lockColor).GetComponent<Image>().raycastTarget=false;
                    Surface("Lock shackle top",node,lockX+lockW*.18f,nodeSize*.25f,lockW*.64f,nodeSize*.08f,lockColor).GetComponent<Image>().raycastTarget=false;
                    Surface("Lock shackle left",node,lockX+lockW*.18f,nodeSize*.27f,lockW*.15f,nodeSize*.22f,lockColor).GetComponent<Image>().raycastTarget=false;
                    Surface("Lock shackle right",node,lockX+lockW*.67f,nodeSize*.27f,lockW*.15f,nodeSize*.22f,lockColor).GetComponent<Image>().raycastTarget=false;
                }
            }
            Label("Reserved talent availability",board,"TALENTOS BLOQUEADOS\nAún no disponibles en esta etapa del reino.",
                18,h-noteH,w-36,noteH-12,compact?18:26,TextAnchor.MiddleCenter);
        }
        void BuildInformation(RectTransform pane,HeroState hero,float w,float h)
        {
            if(CurrentPage==Page.Resumen){
                string[] names={"VIDA","ATAQUE","DEFENSA","RUPTURA","PODER"};
                int[] values={hero.Health,hero.Attack,hero.Defense,hero.Break,hero.HeroPower};
                bool dense=h<340f;
                int columns=dense?2:1;
                int rows=Mathf.CeilToInt(names.Length/(float)columns);
                float row=(h-88f)/rows,cell=(w-24f)/columns;
                for(int i=0;i<names.Length;i++){
                    float x=12+(i%columns)*cell,y=8+(i/columns)*row;
                    Label(names[i],pane,names[i],x,y,cell*.61f,row,17);
                    Label(names[i]+" real value",pane,values[i].ToString(),
                        x+cell*.61f,y,cell*.35f,row,20,TextAnchor.MiddleRight).color=gold;
                }
                Label("Persistent experience",pane,
                    "EXP  "+hero.Experience+(hero.ExperienceToNext>0?" / "+hero.ExperienceToNext:"")+
                    "\nCAPACIDAD DE MARCHA  "+hero.Capacity,
                    12,h-76f,w-24f,64f,17);
            }else if(CurrentPage==Page.Habilidades){
                Label("Canonical hero ability",pane,hero.AbilityName??"CARACTERÍSTICAS",16,18,w-32,48,24).color=gold;
                ScrollDescription(pane,hero.AbilityDescription??"",16,76,w-32,Mathf.Max(50,h-96));
            }else{
                string title=CurrentPage==Page.Talentos?"TALENTOS":"EQUIPAMIENTO";
                Label("Unavailable system title",pane,title,16,18,w-32,52,24).color=gold;
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
