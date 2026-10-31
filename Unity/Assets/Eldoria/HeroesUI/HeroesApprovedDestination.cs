using System;
using UnityEngine;
using UnityEngine.UI;

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
            var material=Rect("Royal Gold / independent material",r);
            Place(material,0,0,w,h);
            var graphic=material.gameObject.AddComponent<HeroesGoldFrameGraphic>();
            graphic.SurfaceTop=new Color(.045f,.059f,.079f,.82f);
            graphic.SurfaceBottom=new Color(.014f,.021f,.033f,.91f);
            graphic.raycastTarget=false;
            return r;
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
            float tabWidth=portrait?(w-46)/2f:Mathf.Clamp(w*.16f,140,190),top=portrait?210:100;
            for(int i=0;i<pages.Length;i++){
                var page=pages[i];float x=portrait?20+(i%2)*(tabWidth+6):20,y=portrait?96+(i/2)*49:110+i*62;
                var b=Button("Hero tab "+page,captions[i],x,y,tabWidth,portrait?43:52,()=>Select(page));
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
        Texture2D hallArchitecture;
        void DrawApprovedSalonEnvironment(float w,float h)
        {
            if(!hallArchitecture)
                hallArchitecture=Resources.Load<Texture2D>("EldoriaV41/heroes-hall-clean-v1");
            var scene=Rect("Hall / clean architectural plate",content);
            Place(scene,0,0,w,h);
            var plate=scene.gameObject.AddComponent<RawImage>();
            plate.texture=hallArchitecture;plate.raycastTarget=false;
            // Uniform cover crop: no stretching columns on a narrow phone.
            if(hallArchitecture){
                float sourceAspect=hallArchitecture.width/(float)hallArchitecture.height;
                float viewAspect=w/h;
                float cropW=Mathf.Min(1f,viewAspect/sourceAspect);
                float cropH=Mathf.Min(1f,sourceAspect/viewAspect);
                plate.uvRect=new UnityEngine.Rect((1f-cropW)*.5f,(1f-cropH)*.5f,cropW,cropH);
            }
            var header=Surface("Hall / readable header",content,0,0,w,
                CurrentPage==Page.Salon?Mathf.Min(180f,h*.20f):88f,
                new Color(.015f,.022f,.034f,.73f));
            header.GetComponent<Image>().raycastTarget=false;
            var footer=Surface("Hall / navigation ground",content,0,h-80,w,80,
                new Color(.015f,.022f,.034f,.91f));
            footer.GetComponent<Image>().raycastTarget=false;
            Surface("Hall footer gold line",content,16,h-79,w-32,2,gold)
                .GetComponent<Image>().raycastTarget=false;
        }
        Sprite futureSlotArt;
        void BuildSalon(float w,float h)
        {
            // The owner's six-slot v14 salon composition is visual only:
            // slots beyond the real host roster have NO data, actions or unlock.
            // Do not present any fictional hero or mutate progression state.
            float sceneryH=Mathf.Min(240f,h*.29f);
            Label("Heroes hall title",content,"SALÓN DE HÉROES",26,18,w*.61f,48,30,
                TextAnchor.MiddleLeft).color=gold;
            Label("Heroes hall introduction",content,
                "Aquí residen los héroes de Valoria.\nReclútalos, fortalécelos y lidera con ellos.",
                27,71,w*.60f,Mathf.Max(40f,sceneryH-82f),18,
                TextAnchor.UpperLeft);
            var heroes=ReadHeroes?.Invoke()??Array.Empty<HeroState>();
            bool showFutureSlots=true;
            int displayCount=showFutureSlots?Math.Max(6,heroes.Length):heroes.Length;
            int columns=w>=380f?3:2;
            int rows=Mathf.Max(1,Mathf.CeilToInt(displayCount/(float)columns));
            float gap=Mathf.Clamp(w*.016f,8f,16f);
            float cardTop=sceneryH+12f;
            float spaceH=h-cardTop-85f;
            float cardH=Mathf.Min(650f,Mathf.Max(105f,(spaceH-gap*(rows-1))/rows));
            float cardW=Mathf.Min(340f,(w-40f-gap*(columns-1))/columns);
            bool compact=cardH<260f;
            if(!futureSlotArt && showFutureSlots)
            {
                var source=Resources.Load<Texture2D>("EldoriaV41/hero-locked");
                if(source)futureSlotArt=Sprite.Create(source,new Rect(0,0,source.width,source.height),
                    new Vector2(.5f,.5f),100,0,SpriteMeshType.FullRect);
            }
            for(int i=0;i<displayCount;i++)
            {
                int col=i%columns,row=i/columns;
                float left=(w-(cardW*columns+gap*(columns-1)))*.5f+col*(cardW+gap);
                float top=cardTop+row*(cardH+gap);
                bool real=i<heroes.Length;
                var card=Frame(real?"Hero card "+heroes[i].Id:"Reserved future hero slot "+(i-heroes.Length),
                    content,left,top,cardW,cardH);
                string title=real?heroes[i].DisplayName:"RESERVADO";
                Label("Hero card name",card,title,10,12,cardW-20,
                    compact?31:42,compact?19:25,TextAnchor.MiddleCenter)
                    .color=real?gold:new Color(.57f,.53f,.44f);
                Sprite portrait=real?heroes[i].Portrait:futureSlotArt;
                bool unlocked=real&&heroes[i].Unlocked;
                if(compact)
                {
                    float portraitW=cardW*.43f;
                    float portraitH=Mathf.Max(30,cardH-104f);
                    if(portrait)
                    {
                        var image=Rect(real?"Verified portrait":"Reserved silhouette",card)
                            .gameObject.AddComponent<Image>();
                        Place(image.rectTransform,8,43,portraitW,portraitH);
                        image.sprite=portrait;image.preserveAspect=true;image.raycastTarget=false;
                        image.color=unlocked?Color.white:new Color(.42f,.42f,.44f,.76f);
                    }
                    Label("Hero card state",card,
                        real?(unlocked?heroes[i].Role+"\nNIVEL "+heroes[i].Level+
                            "\nPODER "+heroes[i].HeroPower:"Bloqueado por la historia"):
                            "Aún no\n disponible",
                        portraitW+10,47,cardW-portraitW-17,Mathf.Max(32,cardH-106),
                        15,TextAnchor.MiddleCenter);
                }
                else
                {
                    float portraitH=Mathf.Max(80,cardH-193f);
                    if(portrait)
                    {
                        var image=Rect(real?"Verified portrait":"Reserved silhouette",card)
                            .gameObject.AddComponent<Image>();
                        Place(image.rectTransform,12,60,cardW-24,portraitH);
                        image.sprite=portrait;image.preserveAspect=true;image.raycastTarget=false;
                        image.color=unlocked?Color.white:new Color(.42f,.42f,.44f,.76f);
                    }
                    Label("Hero card state",card,
                        real?(unlocked?heroes[i].Role+"\nNIVEL "+heroes[i].Level+
                            " · PODER "+heroes[i].HeroPower:"Se incorpora durante la historia"):
                            "HUECO RESERVADO\nSin héroe reclutado",
                        12,cardH-131,cardW-24,63,18,TextAnchor.MiddleCenter);
                }
                if(real)
                {
                    int index=i;var hero=heroes[index];
                    Button("Select hero "+hero.Id,hero.Unlocked?"VER HÉROE":"BLOQUEADO",
                        left+9,top+cardH-53,cardW-18,42,
                        ()=>OpenHero(hero.Id),hero.Unlocked);
                }
                else
                    Label("Non-interactive future slot",card,"NO DISPONIBLE",
                        8,cardH-53,cardW-16,42,compact?14:18,
                        TextAnchor.MiddleCenter).color=new Color(.57f,.53f,.44f);
            }
            int unlockedCount=0;foreach(var hero in heroes)if(hero.Unlocked)unlockedCount++;
            Label("Authoritative hall population",content,
                unlockedCount+" / "+heroes.Length+" HÉROES DISPONIBLES",20,h-72,w-40,38,
                16,TextAnchor.MiddleCenter).color=gold;
        }
        void BuildReservedTalents(float x,float y,float w,float h,bool portrait)
        {
            // These are the three reserved visual paths in the exact approved
            // master. No talent data, level, price or bonus exists in the host.
            // All nodes remain disabled and create no command or save state.
            var board=Frame("Heroes / reserved talent paths",content,x,y,w,h);
            float gap=portrait?12f:18f,trackW=(w-32-2*gap)/3f;
            float noteH=portrait?120f:90f,trackH=h-noteH-28;
            string[] titles={"OFENSIVA","DEFENSA","SOPORTE"};
            Color[] accents={new Color(.76f,.24f,.12f),new Color(.17f,.51f,.90f),gold};
            for(int path=0;path<3;path++){
                float left=16+path*(trackW+gap);
                var track=Frame("Reserved talent path "+titles[path],board,left,14,trackW,trackH);
                track.GetComponentInChildren<HeroesGoldFrameGraphic>().Border=accents[path];
                Label("Reserved path name",track,titles[path],6,14,trackW-12,56,26,TextAnchor.MiddleCenter).color=ivory;
                float nodeSize=Mathf.Min(76f,trackW*.55f,Mathf.Max(28f,(trackH-110f)/3.4f));
                float start=80f;
                float step=Mathf.Max(0f,(trackH-start-nodeSize-18f)/2f);
                float center=trackW*.5f;
                Surface("Path connection",track,center-1.5f,start+nodeSize*.5f,3,2*step,accents[path]).GetComponent<Image>().raycastTarget=false;
                for(int rank=0;rank<3;rank++){
                    float nodeY=start+rank*step;
                    var node=Frame("Reserved talent node "+path+"-"+rank,track,center-nodeSize*.5f,nodeY,nodeSize,nodeSize);
                    node.GetComponentInChildren<HeroesGoldFrameGraphic>().Border=accents[path];
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
                18,h-noteH,w-36,noteH-12,26,TextAnchor.MiddleCenter);
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
                    "EXP  "+hero.Experience+" / "+hero.ExperienceToNext+
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
