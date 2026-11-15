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
    public sealed partial class AshenCitadelExperience : MonoBehaviour
    {
        [SerializeField] private GameObject[] stages = new GameObject[5];
        [SerializeField] private GameObject sawmill, barracks, granary, stoneworks;
        [SerializeField] private GameObject cityRoot, worldRoot;
        [SerializeField] private Camera playerCamera;

        private LocalGateway gateway;
        private Font typeface;
        private Sprite frame, primary, secondary, crest;
        private RectTransform safe;
        private RectTransform popup, topHud, bottomHud, accountHud;
        private CanvasScaler responsiveScaler;
        private Text levelText, resourceText, feedbackText, popupTitle, popupDescription;
        private Transform popupActionArea;
        private RectTransform sheetActionsViewport;
        private ScrollRect sheetScroll;
        private Button sheetClose;
        private string sheetDescription="";
        private int sheetRows;
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
            // The owner-facing I–V profile sets actual game costs and milestones.
            // Used only in this isolated scene with its own save key.
            SliceContentProfiles.SetRuntimeProfileOverride(SliceContentProfiles.OwnerIiiId);
            typeface = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            frame = MakeSprite("AshenPanel", new Vector4(20,20,20,20));
            primary = MakeSprite("AshenButtonPrimary", new Vector4(18,12,18,12));
            secondary = MakeSprite("AshenButtonSecondary", new Vector4(18,12,18,12));
            crest = MakeSprite("AshenCrest", Vector4.zero);
            try
            {
                // Own persistent save key. Production's filename and WebGL key are untouched.
                var save = Path.Combine(UnityEngine.Application.persistentDataPath, "eldoria-ashen-experiment-v1.json");
                gateway = new LocalGateway(new SystemClock(), new FileStateStore(save), EldoriaV41GatewayResolver.Campaign());
            }
            catch(Exception exception)
            {
                Debug.LogError("ASHEN: isolated save unreadable, not overwriting it: " + exception);
                lastMessage = "Guardado experimental ilegible; sesión temporal.";
                gateway = new LocalGateway(new SystemClock(), new SessionStore(), EldoriaV41GatewayResolver.Campaign());
            }
            if(gateway.Snapshot().WorldId == "world-local")
                gateway.ConfigureLocalMarchRoutes(new AshenCitadelRoutes());
            EnsureInput();
            BuildInterface();
            BuildAdditionalAccesses();
            ReflowForViewport(Screen.width,Screen.height);
            ShowWorld(false);
            Refresh();
            if(string.IsNullOrWhiteSpace(gateway.Snapshot().PlayerName))ShowNicknamePrompt();
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
            var root=new GameObject("Ashen · Royal Interface",typeof(RectTransform),typeof(Canvas),
                typeof(CanvasScaler),typeof(GraphicRaycaster));
            var canvas=root.GetComponent<Canvas>();
            canvas.renderMode=RenderMode.ScreenSpaceOverlay;canvas.sortingOrder=90;
            var scaler=root.GetComponent<CanvasScaler>();
            scaler.uiScaleMode=CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution=new Vector2(390,844);
            scaler.screenMatchMode=CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            // Height-based scaling preserves the same readable text/touch height
            // between a 390x844 iPhone portrait and a 1600x900 desktop landscape.
            scaler.matchWidthOrHeight=1f;
            responsiveScaler=scaler;
            safe=Node(root.transform,"Device safe area",Vector2.zero,Vector2.one,Vector2.zero,Vector2.zero);

            // The background environment remains visible between the compact
            // header, 46 px account ribbon and the shallow 74 px footer.
            var header=Node(safe,"Header",new Vector2(0,1),Vector2.one,
                new Vector2(4,-103),new Vector2(-4,-3));
            topHud=header;
            Plate(header,frame,Color.white);
            var emblem=Node(header,"Crest",new Vector2(0,0),new Vector2(0,1),
                new Vector2(10,17),new Vector2(78,-16));
            Plate(emblem,crest,Color.white);
            levelText=Label(header,"Realm",18,TextAnchor.MiddleLeft,Ivory,
                new Vector2(0,.49f),Vector2.one,new Vector2(86,0),new Vector2(-10,-4));
            resourceText=Label(header,"Resources",13,TextAnchor.MiddleLeft,Muted,
                new Vector2(0,0),new Vector2(1,.50f),new Vector2(86,2),new Vector2(-8,0));

            var bottom=Node(safe,"Bottom navigation",Vector2.zero,new Vector2(1,0),
                new Vector2(4,4),new Vector2(-4,78));
            bottomHud=bottom;
            Plate(bottom,frame,Color.white);
            string[] nav={"BASTIÓN","EDIFICIOS","EJÉRCITO","MUNDO"};
            for(int i=0;i<nav.Length;i++)
            {
                int n=i;
                float x0=i/4f,x1=(i+1)/4f;
                Button(bottom,"Nav · "+nav[i],nav[i],secondary,new Vector2(x0,.12f),
                    new Vector2(x1,.90f),new Vector2(4,0),new Vector2(-4,0),
                    ()=>OpenSection(n),12);
            }
            feedbackText=Label(safe,"Message",13,TextAnchor.MiddleCenter,Ivory,
                new Vector2(0,0),new Vector2(1,0),new Vector2(12,82),new Vector2(-12,112));

            // Content-driven sheet attached above navigation, not an oversized
            // modal that hides 60% of the kingdom regardless of content.
            popup=Node(safe,"Action sheet",new Vector2(.035f,0),new Vector2(.965f,0),
                Vector2.zero,Vector2.zero);
            popup.pivot=new Vector2(.5f,0);
            popup.anchoredPosition=new Vector2(0,114);
            popup.sizeDelta=new Vector2(0,260);
            Plate(popup,frame,Color.white);
            popupTitle=Label(popup,"Sheet title",18,TextAnchor.MiddleLeft,Ivory,
                new Vector2(0,1),Vector2.one,new Vector2(20,-57),new Vector2(-57,-9));
            popupDescription=Label(popup,"Description",13,TextAnchor.UpperLeft,Muted,
                new Vector2(0,1),Vector2.one,new Vector2(20,-120),new Vector2(-20,-61));
            sheetClose=Button(popup,"Close","×",secondary,new Vector2(1,1),Vector2.one,
                new Vector2(-52,-53),new Vector2(-13,-13),CloseSheet,21);

            var scroll=Node(popup,"Actions scroll",Vector2.zero,Vector2.one,
                new Vector2(20,15),new Vector2(-20,-132));
            sheetActionsViewport=Node(scroll,"Viewport",Vector2.zero,Vector2.one,
                Vector2.zero,Vector2.zero);
            sheetActionsViewport.gameObject.AddComponent<RectMask2D>();
            var content=Node(sheetActionsViewport,"Content",new Vector2(0,1),Vector2.one,
                Vector2.zero,Vector2.zero);
            content.pivot=new Vector2(.5f,1);
            content.sizeDelta=Vector2.zero;
            popupActionArea=content;
            sheetScroll=scroll.gameObject.AddComponent<ScrollRect>();
            sheetScroll.horizontal=false;sheetScroll.vertical=true;
            sheetScroll.viewport=sheetActionsViewport;
            sheetScroll.content=content;
            sheetScroll.scrollSensitivity=22;
            popup.gameObject.SetActive(false);
            ApplySafeArea();
        }
        private void CloseSheet()
        {
            popup.gameObject.SetActive(false);
            if(dailyButton!=null)dailyButton.gameObject.SetActive(true);
            if(timeText!=null)timeText.gameObject.SetActive(true);
        }
        private void SizeSheet(int rows)
        {
            sheetRows=rows;
            int hardBreaks=sheetDescription.Split('\n').Length;
            float lineEstimate=Mathf.Ceil(sheetDescription.Length/44f)+Mathf.Max(0,hardBreaks-1);
            float descriptionHeight=Mathf.Clamp(37f+lineEstimate*14f,66f,135f);
            popupDescription.rectTransform.offsetMin=new Vector2(20,-(61+descriptionHeight));
            popupDescription.rectTransform.offsetMax=new Vector2(-20,-61);
            var scrollRect=sheetScroll.GetComponent<RectTransform>();
            scrollRect.offsetMax=new Vector2(-20,-(71+descriptionHeight));
            const float rowStep=54f;
            float contentHeight=Mathf.Max(1f,rows*rowStep);
            ((RectTransform)popupActionArea).sizeDelta=new Vector2(0,contentHeight);
            float preferredHeight=85f+descriptionHeight+Mathf.Min(rows,5)*rowStep+20f;
            float safeHeight=safe.rect.height>200?safe.rect.height:844f;
            float maxHeight=Mathf.Max(230f,Mathf.Min(490f,safeHeight*.65f));
            popup.sizeDelta=new Vector2(popup.sizeDelta.x,Mathf.Clamp(preferredHeight,192f,maxHeight));
            if(sheetScroll!=null)sheetScroll.verticalNormalizedPosition=1f;
        }

        private void ApplySafeArea()
        {
            if(safe==null || Screen.width<=0 || Screen.height<=0)return;
            var a=Screen.safeArea;
            safe.anchorMin=new Vector2(a.xMin/Screen.width,a.yMin/Screen.height);
            safe.anchorMax=new Vector2(a.xMax/Screen.width,a.yMax/Screen.height);
            safe.offsetMin=Vector2.zero;safe.offsetMax=Vector2.zero;
            lastWidth=Screen.width;lastHeight=Screen.height;
            ReflowForViewport(lastWidth,lastHeight);
        }

        // Runtime orientation changes and camera-space evidence share exactly one
        // layout rule. Screen height drives the CanvasScaler; logical width depends
        // only on aspect, not device DPI. No absolute desktop-pixel offsets.
        public void ReflowForViewport(int width,int height)
        {
            if(width<1||height<1||safe==null||responsiveScaler==null)return;
            responsiveScaler.matchWidthOrHeight=1f;
            float viewportWidth=844f*width/height;
            float usableWidth=Mathf.Max(280f,viewportWidth-8f);
            float furnitureWidth=Mathf.Min(760f,usableWidth);
            float sheetWidth=Mathf.Min(600f,usableWidth-8f);
            if(topHud!=null)
                AnchorCentered(topHud,new Vector2(.5f,1f),new Vector2(furnitureWidth,100f),
                    new Vector2(0,-3f));
            if(accountHud!=null)
                AnchorCentered(accountHud,new Vector2(.5f,1f),new Vector2(furnitureWidth,43f),
                    new Vector2(0,-109f));
            if(bottomHud!=null)
                AnchorCentered(bottomHud,new Vector2(.5f,0f),new Vector2(furnitureWidth,74f),
                    new Vector2(0,4f));
            if(feedbackText!=null)
                AnchorCentered(feedbackText.rectTransform,new Vector2(.5f,0f),
                    new Vector2(furnitureWidth-16f,28f),new Vector2(0,82f));
            if(popup!=null)
            {
                AnchorCentered(popup,new Vector2(.5f,0f),
                    new Vector2(sheetWidth,Mathf.Max(195f,popup.sizeDelta.y)),
                    new Vector2(0,114f));
                if(popup.gameObject.activeSelf)SizeSheet(sheetRows);
            }
            if(dailyButton!=null)
            {
                float x=-furnitureWidth/2f+63f;
                AnchorCentered((RectTransform)dailyButton.transform,
                    new Vector2(.5f,0),new Vector2(110f,35f),new Vector2(x,115f));
            }
            if(timeText!=null)
            {
                float x=furnitureWidth/2f-100f;
                AnchorCentered(timeText.rectTransform,
                    new Vector2(.5f,0),new Vector2(175f,34f),new Vector2(x,115f));
            }
        }
        private static void AnchorCentered(RectTransform element,Vector2 anchor,
                                           Vector2 size,Vector2 offset)
        {
            element.anchorMin=element.anchorMax=anchor;
            element.pivot=anchor;
            element.sizeDelta=size;
            element.anchoredPosition=offset;
        }
        private void Update()
        {
            if(gateway==null)return;
            if(Screen.width!=lastWidth||Screen.height!=lastHeight)ApplySafeArea();
            ProcessSceneTap();
            if(Time.unscaledTime<nextRefresh)return;
            nextRefresh=Time.unscaledTime+.30f;
            gateway.Advance();
            Refresh();
            RefreshClockLabel();
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
            levelText.text="CIUDADELA DE CENIZA\nBASTIÓN "+Roman(visibleStage);
            resourceText.text="MADERA "+s.Resources.Wood+" · PIEDRA "+s.Resources.Stone+
                "\nCOMIDA "+s.Resources.Food+" · TROPAS "+s.Available.Total;
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
            popupTitle.text=title;
            sheetDescription=description??"";
            popupDescription.text=sheetDescription;
            actions=actions??new List<(string,Action,bool)>();
            for(int i=popupActionArea.childCount-1;i>=0;i--)
                Destroy(popupActionArea.GetChild(i).gameObject);
            SizeSheet(actions.Count);
            const int rowHeight=47,rowPitch=54;
            for(int i=0;i<actions.Count;i++)
            {
                var spec=actions[i];
                float top=-i*rowPitch,bottom=top-rowHeight;
                var item=Button(popupActionArea,spec.Item1,spec.Item1,
                    spec.Item3?primary:secondary,
                    new Vector2(0,1),new Vector2(1,1),
                    new Vector2(0,bottom),new Vector2(0,top),spec.Item2,13);
                item.interactable=spec.Item3;
            }
            if(dailyButton!=null)dailyButton.gameObject.SetActive(false);
            if(timeText!=null)timeText.gameObject.SetActive(false);
            Canvas.ForceUpdateCanvases();
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
