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
            if(source&&preview.Show(source,raw)){
                float imageH=modelH-46,imageW=Mathf.Min(modelW-10,imageH*2f/3f);
                Place(raw.rectTransform,(modelW-imageW)*.5f,5,imageW,imageH);
                Button("Rotate hero left","‹",bodyX+7,top+modelH-78,32,32,()=>{preview.Rotate(-30);Rebuild();});
                Button("Rotate hero right","›",bodyX+modelW-39,top+modelH-78,32,32,()=>{preview.Rotate(30);Rebuild();});
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
            Button("Previous available hero","‹",20,h-66,64,46,()=>CycleHero(-1));
            Label("Current hero identity",content,hero.DisplayName,94,h-66,w-188,46,22,TextAnchor.MiddleCenter).color=gold;
            Button("Next available hero","›",w-84,h-66,64,46,()=>CycleHero(1));
        }
        void BuildSalon(float w,float h)
        {
            Label("Heroes hall title",content,"SALÓN DE HÉROES",74,12,w-148,52,30,TextAnchor.MiddleCenter).color=gold;
            var heroes=ReadHeroes?.Invoke()??Array.Empty<HeroState>();int count=Math.Max(1,heroes.Length);
            float gap=16,cardW=Mathf.Min(330,(w-40-gap*(count-1))/count),cardH=Mathf.Min(520,h-190);
            float x=(w-(cardW*count+gap*(count-1)))/2;
            for(int i=0;i<heroes.Length;i++){
                var hero=heroes[i];float left=x+i*(cardW+gap);var card=Frame("Hero card "+hero.Id,content,left,110,cardW,cardH);
                Label("Hero card name",card,hero.DisplayName,12,16,cardW-24,50,26,TextAnchor.MiddleCenter).color=gold;
                if(hero.Portrait){var image=Rect("Verified portrait",card).gameObject.AddComponent<Image>();Place(image.rectTransform,14,78,cardW-28,Mathf.Max(50,cardH-210));image.sprite=hero.Portrait;image.preserveAspect=true;image.raycastTarget=false;}
                Label("Hero card state",card,hero.Unlocked?hero.Role+"\nNIVEL "+hero.Level+" · PODER "+hero.HeroPower:"Se incorpora durante la historia",12,cardH-122,cardW-24,62,20,TextAnchor.MiddleCenter);
                Button("Select hero "+hero.Id,hero.Unlocked?"VER HÉROE":"BLOQUEADO",left+12,110+cardH-54,cardW-24,42,()=>OpenHero(hero.Id),hero.Unlocked);
            }
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
