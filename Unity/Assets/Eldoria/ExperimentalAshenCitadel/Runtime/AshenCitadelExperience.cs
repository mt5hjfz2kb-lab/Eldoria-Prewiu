// ASHEN CITADEL — independently authored prototype runtime.
// The authoritative PlayerState, mission prerequisites, timers, army, rewards and
// combat remain in Eldoria.Domain/Application. There is NO debug level setter.
using System;
using System.Collections.Generic;
using System.IO;
using Eldoria.Application;
using Eldoria.Domain;
using Eldoria.Infrastructure;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

namespace Eldoria.ExperimentalAshenCitadel
{
    [DefaultExecutionOrder(-500)]
    public sealed class AshenCitadelExperience : MonoBehaviour
    {
        [SerializeField] private GameObject[] stages = new GameObject[5];
        [SerializeField] private GameObject sawmill, barracks, granary, stoneworks;
        [SerializeField] private GameObject cityRoot, worldRoot;
        [SerializeField] private Camera playerCamera;

        private LocalGateway gateway;
        private Font typeface;
        private Sprite frame, primary, secondary, crest;
        private RectTransform safe;
        private RectTransform popup;
        private Text levelText, resourceText, feedbackText, popupTitle, popupDescription;
        private Transform popupActionArea;
        private bool worldMode;
        private float nextRefresh;
        private int lastWidth, lastHeight;
        private string lastMessage = "La fortaleza despierta.";
        private int visibleStage;

        public int VisibleStage => visibleStage;
        public bool IsWorldVisible => worldMode;
        public PlayerState CurrentState => gateway?.Snapshot();
        public string LastMessage => lastMessage;
        private static Color Ivory => new Color(.94f,.90f,.82f,1);
        private static Color Muted => new Color(.75f,.73f,.66f,1);

        public void Configure(GameObject[] levelObjects, GameObject mill, GameObject military,
                              GameObject food, GameObject mine, GameObject city, GameObject world,
                              Camera camera)
        {
            if(levelObjects == null || levelObjects.Length != 5)
                throw new ArgumentException("Five authored Bastion meshes are required");
            stages = levelObjects; sawmill = mill; barracks = military; granary = food;
            stoneworks = mine; cityRoot = city; worldRoot = world; playerCamera = camera;
        }

        private void Awake()
        {
            typeface = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            frame = MakeSprite("AshenPanel", new Vector4(20,20,20,20));
            primary = MakeSprite("AshenButtonPrimary", new Vector4(18,12,18,12));
            secondary = MakeSprite("AshenButtonSecondary", new Vector4(18,12,18,12));
            crest = MakeSprite("AshenCrest", Vector4.zero);
            try
            {
                // Own persistent save key. Production's filename and WebGL key are untouched.
                var save = Path.Combine(Application.persistentDataPath, "eldoria-ashen-experiment-v1.json");
                gateway = new LocalGateway(new SystemClock(), new FileStateStore(save));
            }
            catch(Exception exception)
            {
                Debug.LogError("ASHEN: isolated save unreadable, not overwriting it: " + exception);
                lastMessage = "Guardado experimental ilegible; sesión temporal.";
                gateway = new LocalGateway(new SystemClock(), new SessionStore());
            }
            if(gateway.Snapshot().WorldId == "world-local")
                gateway.ConfigureLocalMarchRoutes(new AshenCitadelRoutes());
            EnsureInput();
            BuildInterface();
            ShowWorld(false);
            Refresh();
        }

        private static Sprite MakeSprite(string resourceName, Vector4 border)
        {
            var tex = Resources.Load<Texture2D>(resourceName);
            if(tex == null) { Debug.LogError("ASHEN missing original UI texture: " + resourceName); return null; }
            return Sprite.Create(tex, new Rect(0,0,tex.width,tex.height),
                new Vector2(.5f,.5f), 100f, 0, SpriteMeshType.FullRect, border);
        }
        private static void EnsureInput()
        {
            if(EventSystem.current != null) return;
            var go = new GameObject("Ashen · Input System");
            go.AddComponent<EventSystem>();
            go.AddComponent<InputSystemUIInputModule>();
        }
        private static RectTransform Node(Transform parent,string name,Vector2 min,Vector2 max,Vector2 insetMin,Vector2 insetMax)
        {
            var go = new GameObject(name,typeof(RectTransform));
            go.transform.SetParent(parent,false);
            var r = (RectTransform)go.transform;
            r.anchorMin=min; r.anchorMax=max; r.offsetMin=insetMin; r.offsetMax=insetMax;
            return r;
        }
        private static Image Plate(RectTransform rect,Sprite sprite,Color tint)
        {
            var image=rect.gameObject.AddComponent<Image>();
            image.sprite=sprite;
            image.type=sprite!=null && (sprite.border.x>0 || sprite.border.y>0)?Image.Type.Sliced:Image.Type.Simple;
            image.color=tint;
            return image;
        }
        private Text Label(Transform parent,string name,int size,TextAnchor alignment,Color color,
                           Vector2 min,Vector2 max,Vector2 insetMin,Vector2 insetMax)
        {
            var r=Node(parent,name,min,max,insetMin,insetMax);
            var t=r.gameObject.AddComponent<Text>();
            t.font=typeface;t.fontSize=size;t.fontStyle=FontStyle.Bold;t.color=color;t.alignment=alignment;
            t.horizontalOverflow=HorizontalWrapMode.Wrap;
            t.verticalOverflow=VerticalWrapMode.Truncate;
            t.raycastTarget=false;
            return t;
        }
        private Button Button(Transform parent,string name,string caption,Sprite skin,
                              Vector2 min,Vector2 max,Vector2 insetMin,Vector2 insetMax,
                              Action clicked,int fontSize=15)
        {
            var r=Node(parent,name,min,max,insetMin,insetMax);
            var img=Plate(r,skin,Color.white);
            var button=r.gameObject.AddComponent<Button>();
            button.targetGraphic=img;
            var colors=button.colors;colors.highlightedColor=new Color(1.0f,.94f,.82f,1);
            colors.pressedColor=new Color(.75f,.64f,.57f,1);button.colors=colors;
            Label(r,"Caption",fontSize,TextAnchor.MiddleCenter,Ivory,Vector2.zero,Vector2.one,
                  new Vector2(7,3),new Vector2(-7,-3)).text=caption;
            if(clicked!=null)button.onClick.AddListener(()=>clicked());
            return button;
        }
        private void BuildInterface()
        {
            var go=new GameObject("Ashen · Royal Interface",typeof(RectTransform),typeof(Canvas),typeof(CanvasScaler),typeof(GraphicRaycaster));
            var canvas=go.GetComponent<Canvas>();canvas.renderMode=RenderMode.ScreenSpaceOverlay;canvas.sortingOrder=90;
            var scaler=go.GetComponent<CanvasScaler>();scaler.uiScaleMode=CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution=new Vector2(390,844);scaler.screenMatchMode=CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight=.35f;
            safe=Node(go.transform,"Device safe area",Vector2.zero,Vector2.one,Vector2.zero,Vector2.zero);
            var top=Node(safe,"Header",new Vector2(0,1),Vector2.one,new Vector2(4,-117),new Vector2(-4,-3));
            Plate(top,frame,Color.white);
            var cr=Node(top,"Crest",new Vector2(0,0),new Vector2(0,1),new Vector2(9,12),new Vector2(92,-11));
            Plate(cr,crest,Color.white);
            levelText=Label(top,"Realm",21,TextAnchor.MiddleLeft,Ivory,new Vector2(0,0.52f),Vector2.one,
                new Vector2(94,0),new Vector2(-12,-5));
            resourceText=Label(top,"Resources",15,TextAnchor.MiddleLeft,Muted,new Vector2(0,0),new Vector2(1,.53f),
                new Vector2(94,1),new Vector2(-12,-2));
            var bottom=Node(safe,"Bottom navigation",Vector2.zero,new Vector2(1,0),new Vector2(4,4),new Vector2(-4,118));
            Plate(bottom,frame,Color.white);
            string[] captions={"BASTIÓN","EDIFICIOS","EJÉRCITO","MUNDO"};
            for(int i=0;i<captions.Length;i++)
            {
                int n=i;float x0=i/4f,x1=(i+1)/4f;
                Button(bottom,"Nav · "+captions[i],captions[i],secondary,new Vector2(x0,.13f),
                    new Vector2(x1,.95f),new Vector2(5,0),new Vector2(-5,0),
                    ()=>OpenSection(n),13);
            }
            feedbackText=Label(safe,"Message",13,TextAnchor.MiddleCenter,Ivory,
                new Vector2(0,0),new Vector2(1,0),new Vector2(12,124),new Vector2(-12,158));
            popup=Node(safe,"Action sheet",new Vector2(.045f,.20f),new Vector2(.955f,.78f),
                Vector2.zero,Vector2.zero);
            Plate(popup,frame,Color.white);
            popupTitle=Label(popup,"Sheet title",20,TextAnchor.MiddleCenter,Ivory,
                new Vector2(0,1),Vector2.one,new Vector2(42,-67),new Vector2(-42,-12));
            popupDescription=Label(popup,"Description",13,TextAnchor.UpperLeft,Muted,
                new Vector2(0,1),Vector2.one,new Vector2(22,-140),new Vector2(-22,-72));
            var close=Button(popup,"Close","×",secondary,new Vector2(1,1),Vector2.one,
                new Vector2(-48,-58),new Vector2(-13,-18),()=>popup.gameObject.SetActive(false),22);
            var scroll=Node(popup,"Actions scroll",new Vector2(0,0),new Vector2(1,1),
                new Vector2(22,18),new Vector2(-22,-151));
            var viewport=Node(scroll,"Viewport",Vector2.zero,Vector2.one,Vector2.zero,Vector2.zero);
            viewport.gameObject.AddComponent<RectMask2D>();
            var content=Node(viewport,"Content",new Vector2(0,1),Vector2.one,
                new Vector2(0,-20),Vector2.zero);
            popupActionArea=content;
            var layout=content.gameObject.AddComponent<VerticalLayoutGroup>();
            layout.spacing=7;layout.childControlWidth=true;layout.childControlHeight=false;
            layout.childForceExpandWidth=true;layout.childForceExpandHeight=false;
            var fitter=content.gameObject.AddComponent<ContentSizeFitter>();
            fitter.verticalFit=ContentSizeFitter.FitMode.PreferredSize;
            var scrollRect=scroll.gameObject.AddComponent<ScrollRect>();
            scrollRect.horizontal=false;scrollRect.vertical=true;scrollRect.viewport=viewport;
            scrollRect.content=content;scrollRect.scrollSensitivity=22;
            popup.gameObject.SetActive(false);
            ApplySafeArea();
        }
        private void ApplySafeArea()
        {
            if(safe==null || Screen.width<=0 || Screen.height<=0)return;
            var a=Screen.safeArea;
            safe.anchorMin=new Vector2(a.xMin/Screen.width,a.yMin/Screen.height);
            safe.anchorMax=new Vector2(a.xMax/Screen.width,a.yMax/Screen.height);
            safe.offsetMin=Vector2.zero;safe.offsetMax=Vector2.zero;
            lastWidth=Screen.width;lastHeight=Screen.height;
        }
        private void Update()
        {
            if(gateway==null)return;
            if(Screen.width!=lastWidth||Screen.height!=lastHeight)ApplySafeArea();
            if(Time.unscaledTime<nextRefresh)return;
            nextRefresh=Time.unscaledTime+.30f;
            gateway.Advance();
            Refresh();
        }
        private void Refresh()
        {
            var s=gateway.Snapshot();
            visibleStage=Mathf.Clamp(s.BastionLevel,1,5);
            for(int i=0;i<stages.Length;i++)if(stages[i]!=null)
                stages[i].SetActive(!worldMode && i==visibleStage-1);
            if(sawmill!=null)sawmill.SetActive(!worldMode && s.SawmillLevel>0);
            if(barracks!=null)barracks.SetActive(!worldMode && s.BarracksLevel>0);
            if(granary!=null)granary.SetActive(!worldMode && s.ChapterThree!=null && s.ChapterThree.GranaryLevel>0);
            if(stoneworks!=null)stoneworks.SetActive(!worldMode && s.ChapterThree!=null && s.ChapterThree.StoneworksLevel>0);
            levelText.text="LA CIUDADELA DE CENIZA  ·  I–V\nBASTIÓN "+Roman(visibleStage);
            resourceText.text="MADERA "+s.Resources.Wood+"    PIEDRA "+s.Resources.Stone+
                "\nCOMIDA "+s.Resources.Food+"    TROPAS "+s.Available.Total;
            feedbackText.text=lastMessage;
        }
        private static string Roman(int level)=>new[]{"I","II","III","IV","V"}[Mathf.Clamp(level-1,0,4)];
        private void ShowWorld(bool show)
        {
            worldMode=show;
            if(cityRoot!=null)cityRoot.SetActive(!show);
            if(worldRoot!=null)worldRoot.SetActive(show);
            if(playerCamera!=null)
            {
                var p=show?new Vector3(18,19,33):new Vector3(18,19,-22);
                playerCamera.transform.position=p;
                playerCamera.transform.LookAt(show?new Vector3(0,0,55):new Vector3(0,1.8f,0));
                playerCamera.orthographicSize=Screen.width<Screen.height?20.6f:14f;
            }
            Refresh();
        }
        private void Send(string kind,string target)
        {
            try
            {
                var s=gateway.Snapshot();
                var result=gateway.Execute(new GameCommand(Guid.NewGuid().ToString("N"),
                    s.PlayerId,kind,target,s.Revision));
                lastMessage=(result.Ok?"✓ ":"• ")+result.Message;
                Debug.Log("ASHEN_GATEWAY kind="+kind+" target="+target+" ok="+result.Ok+" revision="+result.Revision);
            }
            catch(Exception e){lastMessage="Error en operación real: "+e.Message;Debug.LogException(e);}
            Refresh();
        }
        private void Open(string title,string description,List<(string,Action,bool)> actions)
        {
            popup.gameObject.SetActive(true);
            popupTitle.text=title;popupDescription.text=description;
            for(int i=popupActionArea.childCount-1;i>=0;i--)
                Destroy(popupActionArea.GetChild(i).gameObject);
            foreach(var spec in actions)
            {
                var item=Button(popupActionArea,spec.Item1,spec.Item1,spec.Item3?primary:secondary,
                    new Vector2(0,0),new Vector2(1,0),Vector2.zero,new Vector2(0,47),spec.Item2,14);
                item.GetComponent<LayoutElement>() ?? item.gameObject.AddComponent<LayoutElement>();
                item.gameObject.GetComponent<LayoutElement>().preferredHeight=47;
                item.interactable=spec.Item3;
            }
        }
        private static void Add(List<(string,Action,bool)> a,string label,Action action,bool enabled=true)
            =>a.Add((label,action,enabled));
        private void OpenSection(int section)
        {
            var s=gateway.Snapshot();
            var actions=new List<(string,Action,bool)>();
            if(section==0)
            {
                ShowWorld(false);
                var description="Progreso real del reino, sin saltos de nivel. Objetivo: "+SliceRules.CurrentObjectiveKey(s)+
                    "\nLas mejoras conservan sus requisitos, costes y temporizadores originales.";
                if(s.BastionLevel<5)
                {
                    var next=BastionProgressionCatalog.ForLevel(s.BastionLevel+1);
                    description+="\nNivel "+Roman(s.BastionLevel+1)+": "+next.WoodCost+" madera · "+next.StoneCost+
                        " piedra · "+next.FoodCost+" comida";
                    Add(actions,"ASCENDER AL BASTIÓN "+Roman(next.Level),()=>Send("AdvanceBastion","bastion"),
                        BastionProgressionCatalog.RequirementsMet(s,next));
                    if(!BastionProgressionCatalog.RequirementsMet(s,next))
                        description+="\nPendiente: "+BastionProgressionCatalog.MissingSummary(s,next);
                }
                else description+="\nBastión V: máximo actual de la beta.";
                Add(actions,"ACELERAR OBRA (1 MIN)",()=>Send("UseSpeedup",s.BastionUpgradeCompletesUtcTicks>0?"bastion:1m":"building:1m"),
                    s.SpeedupOneMinute>0 && (s.BastionUpgradeCompletesUtcTicks>0||s.BuildingCompletesUtcTicks>0));
                Open("BASTIÓN · "+Roman(s.BastionLevel),description,actions);
            }
            else if(section==1)
            {
                ShowWorld(false);
                var c=s.ChapterThree;
                AddBuilding(actions,"ASERRADERO",s.SawmillLevel,"sawmill",s.BastionLevel>=1);
                AddBuilding(actions,"CUARTEL",s.BarracksLevel,"barracks",s.BastionLevel>=2);
                AddBuilding(actions,"GRANERO",c?.GranaryLevel??0,"granary",s.BastionLevel>=3);
                AddBuilding(actions,"TALLER DE PIEDRA",c?.StoneworksLevel??0,"stoneworks",s.BastionLevel>=5);
                Open("EDIFICIOS DEL REINO",
                    "Las construcciones aparecen en el mundo únicamente cuando la autoridad confirma su nivel. " +
                    "Una obra en curso bloquea las nuevas hasta completarse.",actions);
            }
            else if(section==2)
            {
                ShowWorld(false);
                Add(actions,"ENTRENAR 10 ARQUEROS T1",()=>Send("Recruit","archer:t1:10"),s.BarracksLevel>=1);
                Add(actions,"ENTRENAR 10 ARQUEROS T2",()=>Send("Recruit","archer:t2:10"),s.BarracksLevel>=4&&s.BastionLevel>=4);
                Add(actions,"CONFIRMAR MARCHA AUTOMÁTICA",()=>Send("ConfigureMarch","march-main"),s.BastionLevel>=2&&s.BarracksLevel>=1);
                Open("EJÉRCITO Y MARCHAS",
                    "Tropas disponibles: "+s.Available.Total+" · reclutamiento "+(s.RecruitmentCompletesUtcTicks>0?"activo":"libre")+
                    "\nEl motor conserva sus reglas T1–T5, reserva de tropas, capacidad y héroes.",
                    actions);
            }
            else
            {
                ShowWorld(true);
                Add(actions,"RECOGER MADERA · BOSQUE",()=>Send("Gather","forest-valoria"));
                Add(actions,"RECOGER PIEDRA · CANTERA",()=>Send("Gather","quarry-valoria"));
                Add(actions,"ATACAR EXPLORADOR",()=>Send("Fight","corrupt-scout"),!s.ScoutDefeated);
                Add(actions,"ATACAR ENGENDRO",()=>Send("Fight","engendro-valoria"),s.BastionLevel>=2);
                Add(actions,"INVESTIGAR BRECHA",()=>Send("InvestigateFissure","fissure-valoria"),s.BastionLevel>=3);
                Add(actions,"CONTACTAR CON LYRA",()=>Send("ContactLyra","fissure-valoria"),s.BastionLevel>=3);
                Add(actions,"ATACAR BRECHA",()=>Send("Fight","fissure-valoria"),s.BastionLevel>=3);
                Add(actions,"ELEGIR PRODUCCIÓN",()=>Send("ChooseDevelopment","production"),s.BastionLevel>=4);
                Open("FRONTERA Y BRECHA",
                    "Los combates y expediciones se ejecutan en LocalGateway. La ruta es " +
                    "un recorrido experimental separado; las recompensas, restricciones y guardados no se falsifican.",
                    actions);
            }
        }
        private void AddBuilding(List<(string,Action,bool)> actions,string name,int level,string id,bool unlocked)
        {
            string label=name+" · "+(level==0?"CONSTRUIR":"NIV. "+level+" → "+(level+1));
            bool can=unlocked && level<5 && (level==0 || level<gateway.Snapshot().BastionLevel);
            Add(actions,label,()=>Send(level==0?"Build":"UpgradeBuilding",id),can);
        }
        private sealed class SessionStore : IStateStore
        {
            private PlayerState state;
            public PlayerState Load()=>state;
            public void Save(PlayerState value){state=value;}
        }
    }
}
