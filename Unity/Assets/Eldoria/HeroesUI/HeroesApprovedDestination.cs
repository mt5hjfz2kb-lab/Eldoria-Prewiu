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
            var r=Surface(name,parent,x,y,w,h,gold);
            Surface("Inset",r,1,1,w-2,h-2,panel).GetComponent<Image>().raycastTarget=false;return r;
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
            if(CurrentPage==Page.Salon)DrawApprovedSalonEnvironment(w,h);
            bool portrait=h>w;float margin=20;
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
            float bodyH=h-top-92,modelW=portrait?bodyW:bodyW*.43f,modelH=portrait?bodyH*.44f:bodyH;
            var stage=Frame("Hero model stage",content,bodyX,top,modelW,modelH);
            var source=ReadModelRoot?.Invoke(hero.Id) ?? (hero.Id=="aldric"?ApprovedAldricModel:ApprovedLyraModel);
            var raw=Rect("Genuine hero model render",stage).gameObject.AddComponent<RawImage>();
            Place(raw.rectTransform,5,5,modelW-10,modelH-46);raw.raycastTarget=false;
            if(source&&preview.Show(source,raw,hero.Id=="lyra"?0f:90f)){
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
        // The scenery is cropped only from the owner's archived v14 master.
        // The image is a non-interactive environmental strip; live cards, labels
        // and buttons remain separate controls bound to the authoritative host.
        Sprite approvedHallStrip;
        void DrawApprovedSalonEnvironment(float w,float h)
        {
            var backdrop=Surface("Hall / navy environmental ground",content,0,0,w,h,
                new Color(.023f,.031f,.047f,1f));
            backdrop.GetComponent<Image>().raycastTarget=false;
            var source=Resources.Load<Texture2D>("EldoriaV41/HeroesApprovedMaster");
            if(source && source.width==1536 && source.height==1024)
            {
                if(!approvedHallStrip)approvedHallStrip=Sprite.Create(source,
                    new Rect(0,856,512,168),new Vector2(.5f,.5f),100,
                    0,SpriteMeshType.FullRect);
                var mural=Surface("Approved v14 hall architecture / art only",
                    content,0,0,w,Mathf.Min(210,h*.28f),Color.white);
                var image=mural.GetComponent<Image>();
                image.sprite=approvedHallStrip;image.raycastTarget=false;
                // The original strip contains concept lettering on the left.
                // Cover those pixels completely; the real title below is live.
                Surface("Hide concept lettering",content,0,0,w*.68f,
                    Mathf.Min(210,h*.28f),new Color(.029f,.039f,.056f,1f))
                    .GetComponent<Image>().raycastTarget=false;
            }
            float topHeight=Mathf.Min(210,h*.28f);
            Surface("Bronze hall lintel",content,0,topHeight-3,w,3,
                new Color(.66f,.43f,.19f)).GetComponent<Image>().raycastTarget=false;
            // Architectural accents and warm torchlight match the approved
            // navy / gold language without baking text or fake hero states.
            for(int i=0;i<5;i++)
            {
                float bx=w*(i+.5f)/5f;
                Surface("Hall vaulted column "+i,content,bx-2,topHeight,4,
                    Mathf.Max(1,h-topHeight-86),new Color(.22f,.16f,.12f,.35f))
                    .GetComponent<Image>().raycastTarget=false;
            }
            for(int i=0;i<2;i++)
            {
                float bx=i==0?10:w-21;
                Surface("Hall copper torch "+i,content,bx,topHeight+12,11,42,
                    new Color(.39f,.25f,.10f,.70f)).GetComponent<Image>().raycastTarget=false;
                Surface("Hall torch glow "+i,content,bx-5,topHeight,21,22,
                    new Color(.92f,.58f,.18f,.31f)).GetComponent<Image>().raycastTarget=false;
            }
            Surface("Hall footer gold line",content,16,h-79,w-32,2,gold)
                .GetComponent<Image>().raycastTarget=false;
        }
        void BuildSalon(float w,float h)
        {
            // Match the owner's v14 hierarchy: illuminated hall first,
            // live heading and instructions second, then real hero cards.
            float sceneryH=Mathf.Min(210f,h*.28f);
            Label("Heroes hall title",content,"SALÓN DE HÉROES",26,18,w*.61f,48,30,
                TextAnchor.MiddleLeft).color=gold;
            Label("Heroes hall introduction",content,
                "Aquí residen los héroes de Valoria.\nReclútalos, fortalécelos y lidera con ellos.",
                27,71,w*.60f,Mathf.Max(42f,sceneryH-84f),18,
                TextAnchor.UpperLeft);

            var heroes=ReadHeroes?.Invoke()??Array.Empty<HeroState>();
            int count=heroes.Length;
            bool compact=h<470f;
            float gap=Mathf.Clamp(w*.018f,8f,18f);
            float cardTop=sceneryH+12f;
            float cardH=Mathf.Max(152f,Mathf.Min(540f,h-cardTop-86f));
            int columns=Mathf.Clamp(count,1,compact?3:2);
            float cardW=Mathf.Min(330f,(w-40f-gap*(columns-1))/columns);
            float rowGap=12f;
            for(int i=0;i<count;i++)
            {
                var hero=heroes[i];
                int col=i%columns,row=i/columns;
                float left=(w-(cardW*columns+gap*(columns-1)))*.5f+col*(cardW+gap);
                float top=cardTop+row*(cardH+rowGap);
                var card=Frame("Hero card "+hero.Id,content,left,top,cardW,cardH);
                // Reuse the editable, gradient Royal Gold panel from the
                // owner-approved v14 integration package; keep live hit targets.
                var ornament=Rect("Royal Gold card / v14 geometry",card);
                Place(ornament,0,0,cardW,cardH);
                ornament.gameObject.AddComponent<HeroesGoldFrameGraphic>().raycastTarget=false;
                Label("Hero card name",card,hero.DisplayName,10,12,cardW-20,
                    compact?31:42,compact?21:25,TextAnchor.MiddleCenter).color=gold;
                if(compact)
                {
                    float portraitW=cardW*.43f;
                    float portraitH=Mathf.Max(36,cardH-99f);
                    if(hero.Portrait)
                    {
                        var img=Rect("Verified portrait",card).gameObject.AddComponent<Image>();
                        Place(img.rectTransform,9,46,portraitW,portraitH);
                        img.sprite=hero.Portrait;img.preserveAspect=true;
                        img.color=hero.Unlocked?Color.white:new Color(.38f,.38f,.40f);
                        img.raycastTarget=false;
                    }
                    Label("Hero card state",card,
                        hero.Unlocked?hero.Role+"\nNIVEL "+hero.Level+"\nPODER "+hero.HeroPower:
                        "Se incorpora\n durante la historia",
                        portraitW+12,52,cardW-portraitW-22,Mathf.Max(45,cardH-102),
                        15,TextAnchor.MiddleCenter);
                }
                else
                {
                    float portraitH=Mathf.Max(75,cardH-195);
                    if(hero.Portrait)
                    {
                        var img=Rect("Verified portrait",card).gameObject.AddComponent<Image>();
                        Place(img.rectTransform,12,60,cardW-24,portraitH);
                        img.sprite=hero.Portrait;img.preserveAspect=true;
                        img.color=hero.Unlocked?Color.white:new Color(.38f,.38f,.40f);
                        img.raycastTarget=false;
                    }
                    Label("Hero card state",card,
                        hero.Unlocked?hero.Role+"\nNIVEL "+hero.Level+" · PODER "+hero.HeroPower:
                        "Se incorpora durante la historia",
                        12,cardH-131,cardW-24,63,18,TextAnchor.MiddleCenter);
                }
                Button("Select hero "+hero.Id,hero.Unlocked?"VER HÉROE":"BLOQUEADO",
                    left+9,top+cardH-53,cardW-18,42,
                    ()=>OpenHero(hero.Id),hero.Unlocked);
            }
            int unlocked=0;foreach(var hero in heroes)if(hero.Unlocked)unlocked++;
            Label("Authoritative hall population",content,
                unlocked+" / "+heroes.Length+" HÉROES DISPONIBLES",20,h-72,w-40,38,
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
                track.GetComponent<Image>().color=accents[path];
                Label("Reserved path name",track,titles[path],6,14,trackW-12,56,26,TextAnchor.MiddleCenter).color=ivory;
                float nodeSize=Mathf.Min(76f,trackW*.55f,Mathf.Max(28f,(trackH-110f)/3.4f));
                float start=80f;
                float step=Mathf.Max(0f,(trackH-start-nodeSize-18f)/2f);
                float center=trackW*.5f;
                Surface("Path connection",track,center-1.5f,start+nodeSize*.5f,3,2*step,accents[path]).GetComponent<Image>().raycastTarget=false;
                for(int rank=0;rank<3;rank++){
                    float nodeY=start+rank*step;
                    var node=Frame("Reserved talent node "+path+"-"+rank,track,center-nodeSize*.5f,nodeY,nodeSize,nodeSize);
                    node.GetComponent<Image>().color=accents[path];
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
                string[] names={"VIDA","ATAQUE","DEFENSA","RUPTURA","PODER"};int[] values={hero.Health,hero.Attack,hero.Defense,hero.Break,hero.HeroPower};
                float row=Mathf.Min(48,(h-110)/5f);
                for(int i=0;i<names.Length;i++){
                    Label(names[i],pane,names[i],16,12+i*row,w*.60f,row,20);
                    Label(names[i]+" real value",pane,values[i].ToString(),w*.62f,12+i*row,w*.32f,row,22,TextAnchor.MiddleRight).color=gold;
                }
                Label("Persistent experience",pane,"EXPERIENCIA  "+hero.Experience+"\nCAPACIDAD DE MARCHA  "+hero.Capacity,16,18+5*row,w-32,74,18);
            }else if(CurrentPage==Page.Habilidades){
                Label("Canonical hero ability",pane,hero.AbilityName??"CARACTERÍSTICAS",16,18,w-32,48,24).color=gold;
                Label("Canonical hero ability description",pane,hero.AbilityDescription??"",16,76,w-32,Mathf.Max(50,h-96),20);
            }else{
                string title=CurrentPage==Page.Talentos?"TALENTOS":"EQUIPAMIENTO";
                Label("Unavailable system title",pane,title,16,18,w-32,52,24).color=gold;
                Label("Unavailable system state",pane,"Aún no disponible en esta etapa del reino.",16,90,w-32,Mathf.Max(60,h-112),22);
            }
        }
        void CycleHero(int direction)
        {
            var heroes=ReadHeroes?.Invoke()??Array.Empty<HeroState>();int current=Array.FindIndex(heroes,h=>h.Id==CurrentHeroId);
            for(int n=1;n<=heroes.Length;n++){int i=(current+direction*n+heroes.Length*2)%heroes.Length;if(heroes[i].Unlocked){OpenHero(heroes[i].Id);return;}}
        }
    }
}
