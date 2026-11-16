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
        private Font typeface, titleTypeface, emphasisTypeface;
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
        private float feedbackExpires;

        public int VisibleStage => visibleStage;
        public bool IsWorldVisible => worldMode;
        public PlayerState CurrentState => gateway?.Snapshot();
        public string LastMessage => lastMessage;
        private static Color Ivory => AshenInterfaceIdentity.Ivory;
        private static Color Muted => AshenInterfaceIdentity.Muted;

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
            // Embed real OFL-licensed typography rather than platform-dependent
            // LegacyRuntime text. Marcellus handles titles, Alegreya Sans body.
            typeface=Resources.Load<Font>("Fonts/AlegreyaSans-Regular")??
                Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            titleTypeface=Resources.Load<Font>("Fonts/Marcellus-Regular")??typeface;
            emphasisTypeface=Resources.Load<Font>("Fonts/AlegreyaSans-Bold")??typeface;
            // One source-defined premium kit: all controls share identical,
            // generated 9-slice material language; gameplay behaviour is unchanged.
            frame=AshenInterfaceIdentity.Panel;
            primary=AshenInterfaceIdentity.Primary;
            secondary=AshenInterfaceIdentity.Secondary;
            crest=AshenInterfaceIdentity.Crest;
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
            bool title=name=="Realm"||name=="Sheet title";
            bool emphasis=name=="Caption";
            t.font=title?titleTypeface:emphasis?emphasisTypeface:typeface;
            t.fontSize=size;t.fontStyle=FontStyle.Normal;
            t.color=color;t.alignment=alignment;
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
            var colors=button.colors;colors.highlightedColor=new Color(1.0f,.98f,.89f,1);
            colors.pressedColor=new Color(.65f,.68f,.68f,1);colors.disabledColor=new Color(.56f,.59f,.61f,.62f);button.colors=colors;
            var glyph=AshenInterfaceGlyphs.ForControl(name);
            Text captionText=Label(r,"Caption",fontSize,TextAnchor.MiddleCenter,Ivory,
                Vector2.zero,Vector2.one,new Vector2(glyph!=null?27:7,3),
                new Vector2(-7,-3));
            captionText.text=caption;
            if(glyph!=null)
            {
                var iconRect=Node(r,"Functional glyph",new Vector2(0,.5f),new Vector2(0,.5f),
                    new Vector2(7,-11),new Vector2(27,11));
                var icon=Plate(iconRect,glyph,Color.white);
                icon.raycastTarget=false;
            }
            if(clicked!=null)button.onClick.AddListener(()=>clicked());
            return button;
        }
        // Floating icon controls replace the permanent full-width button strips.
        // Their real uGUI Button still calls exactly the same gameplay actions.
        private Button FloatingAction(Transform parent,string name,string caption,
            Vector2 centre,Vector2 size,Action activate,bool showCaption)
        {
            var r=Node(parent,name,centre,centre,-size*.5f,size*.5f);
            var target=Plate(r,null,new Color(1f,1f,1f,.001f));
            var button=r.gameObject.AddComponent<Button>();
            button.targetGraphic=target;
            var colours=button.colors;
            colours.normalColor=Color.white;
            colours.highlightedColor=new Color(1f,.94f,.80f,1f);
            colours.pressedColor=new Color(.70f,.75f,.76f,1f);
            colours.disabledColor=new Color(.5f,.5f,.5f,.42f);
            button.colors=colours;
            button.navigation=new Navigation{mode=Navigation.Mode.None};
            float badgeBottom=showCaption?26f:(size.y-44f)*.5f;
            var badge=Node(r,"Bronze medallion",Vector2.zero,Vector2.zero,
                new Vector2((size.x-44f)*.5f,badgeBottom),
                new Vector2((size.x+44f)*.5f,badgeBottom+44f));
            var art=Plate(badge,AshenInterfaceIdentity.FloatingMedallion,Color.white);
            art.raycastTarget=false;
            var glyph=AshenInterfaceGlyphs.ForControl(name);
            if(glyph!=null)
            {
                var icon=Node(badge,"Semantic icon",Vector2.zero,Vector2.one,
                    new Vector2(11f,11f),new Vector2(-11f,-11f));
                var iconImage=Plate(icon,glyph,Color.white);
                iconImage.raycastTarget=false;
            }
            if(showCaption)
            {
                var label=Label(r,"Compact caption",10,TextAnchor.MiddleCenter,Ivory,
                    Vector2.zero,new Vector2(1,0),new Vector2(0f,2f),
                    new Vector2(0f,23f));
                label.text=caption;
                label.horizontalOverflow=HorizontalWrapMode.Overflow;
            }
            if(activate!=null)button.onClick.AddListener(()=>activate());
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

            // The actual CITY remains visually dominant. No permanent
            // top banner or bottom black bar: one small status chip, four
            // separated bottom medallions and four side utility icons.
            var header=Node(safe,"Compact realm status",new Vector2(0,1),new Vector2(0,1),
                Vector2.zero,Vector2.zero);
            topHud=header;
            var back=Plate(header,AshenInterfaceIdentity.StatusChip,Color.white);
            back.raycastTarget=false;
            var emblem=Node(header,"Crest",new Vector2(0,0),new Vector2(0,1),
                new Vector2(8,6),new Vector2(52,-6));
            var emblemImage=Plate(emblem,crest,Color.white);
            emblemImage.raycastTarget=false;
            levelText=Label(header,"Realm",16,TextAnchor.MiddleLeft,Ivory,
                new Vector2(0,.49f),Vector2.one,new Vector2(55,0),new Vector2(-9,-3));
            resourceText=Label(header,"Resources",11,TextAnchor.MiddleLeft,Muted,
                Vector2.zero,new Vector2(1,.49f),new Vector2(55,3),new Vector2(-8,0));

            var bottom=Node(safe,"Floating navigation",Vector2.zero,new Vector2(1,0),
                Vector2.zero,Vector2.zero);
            bottomHud=bottom;
            string[] nav={"BASTIÓN","EDIFICIOS","EJÉRCITO","MUNDO"};
            string[] shortLabels={"BASTIÓN","OBRAS","TROPAS","MUNDO"};
            for(int i=0;i<nav.Length;i++)
            {
                int n=i;
                FloatingAction(bottom,"Nav · "+nav[i],shortLabels[i],
                    new Vector2((i+.5f)/4f,.50f),new Vector2(78,72),
                    ()=>OpenSection(n),true);
            }
            feedbackText=Label(safe,"Message",12,TextAnchor.MiddleCenter,Ivory,
                new Vector2(0,0),new Vector2(1,0),
                new Vector2(18,66),new Vector2(-18,92));
            feedbackText.gameObject.SetActive(false);

            // Content-driven sheet attached above navigation, not an oversized
            // modal that hides 60% of the kingdom regardless of content.
            popup=Node(safe,"Action sheet",new Vector2(.035f,0),new Vector2(.965f,0),
                Vector2.zero,Vector2.zero);
            popup.pivot=new Vector2(.5f,0);
            popup.anchoredPosition=new Vector2(0,90);
            popup.sizeDelta=new Vector2(0,238);
            var sheetBack=Plate(popup,frame,new Color(1f,1f,1f,.96f));
            sheetBack.raycastTarget=true;
            popupTitle=Label(popup,"Sheet title",18,TextAnchor.MiddleLeft,Ivory,
                new Vector2(0,1),Vector2.one,new Vector2(19,-54),new Vector2(-58,-11));
            popupDescription=Label(popup,"Description",14,TextAnchor.UpperLeft,Muted,
                new Vector2(0,1),Vector2.one,new Vector2(20,-120),new Vector2(-20,-61));
            var titleRule=Node(popup,"Section accent",new Vector2(0,1),new Vector2(1,1),
                new Vector2(20,-59),new Vector2(-63,-57));
            var rule=Plate(titleRule,null,new Color(.61f,.44f,.25f,.84f));
            rule.raycastTarget=false;
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
            float descriptionHeight=Mathf.Clamp(38f+lineEstimate*16f,68f,145f);
            popupDescription.rectTransform.offsetMin=new Vector2(20,-(61+descriptionHeight));
            popupDescription.rectTransform.offsetMax=new Vector2(-20,-61);
            var scrollRect=sheetScroll.GetComponent<RectTransform>();
            scrollRect.offsetMax=new Vector2(-20,-(71+descriptionHeight));
            const float rowStep=54f;
            float contentHeight=Mathf.Max(1f,rows*rowStep);
            ((RectTransform)popupActionArea).sizeDelta=new Vector2(0,contentHeight);
            float preferredHeight=85f+descriptionHeight+Mathf.Min(rows,5)*rowStep+20f;
            float safeHeight=safe.rect.height>200?safe.rect.height:844f;
            float maxHeight=Mathf.Max(215f,Mathf.Min(374f,safeHeight*.49f));
            popup.sizeDelta=new Vector2(popup.sizeDelta.x,Mathf.Clamp(preferredHeight,188f,maxHeight));
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
            FrameCameraForViewport(playerCamera,width,height);
            responsiveScaler.matchWidthOrHeight=1f;
            float viewportWidth=844f*width/height;
            float usableWidth=Mathf.Max(280f,viewportWidth-8f);
            float furnitureWidth=Mathf.Min(760f,usableWidth);
            float sheetWidth=Mathf.Min(600f,usableWidth-8f);
            if(topHud!=null)
            {
                float chipWidth=Mathf.Min(247f,usableWidth-109f);
                AnchorCentered(topHud,new Vector2(0f,1f),
                    new Vector2(chipWidth,58f),
                    new Vector2(9f,-7f));
            }
            if(accountHud!=null)
                AnchorCentered(accountHud,new Vector2(1f,1f),
                    new Vector2(91f,96f),new Vector2(-7f,-7f));
            if(bottomHud!=null)
                AnchorCentered(bottomHud,new Vector2(.5f,0f),
                    new Vector2(Mathf.Min(374f,usableWidth),78f),
                    new Vector2(0,4f));
            if(feedbackText!=null)
                AnchorCentered(feedbackText.rectTransform,new Vector2(.5f,0f),
                    new Vector2(Mathf.Min(furnitureWidth-16f,310f),24f),
                    new Vector2(0,82f));
            if(popup!=null)
            {
                AnchorCentered(popup,new Vector2(.5f,0f),
                    new Vector2(Mathf.Min(sheetWidth,396f),Mathf.Max(188f,popup.sizeDelta.y)),
                    new Vector2(0,90f));
                if(popup.gameObject.activeSelf)SizeSheet(sheetRows);
            }
            if(dailyButton!=null)
            {
                float x=-Mathf.Min(374f,usableWidth)/2f+59f;
                AnchorCentered((RectTransform)dailyButton.transform,
                    new Vector2(.5f,0),new Vector2(105f,30f),
                    new Vector2(x,91f));
            }
            if(timeText!=null)
            {
                float x=Mathf.Min(374f,usableWidth)/2f-89f;
                AnchorCentered(timeText.rectTransform,
                    new Vector2(.5f,0),new Vector2(168f,30f),
                    new Vector2(x,91f));
            }
        }
        // Orthographic portrait rays can begin BELOW the ground even when the
        // camera centre is above it. Increasing terrain size cannot fix this.
        // Translate along the view axis (unchanged composition) until the lowest
        // ray begins above the ground; shared by runtime and genuine captures.
        public static void FrameCameraForViewport(Camera camera,int width,int height)
        {
            if(camera==null||width<1||height<1)return;
            camera.aspect=width/(float)height;
            // Keep the full 46-metre settlement width visible on narrow phones.
            camera.orthographicSize=width<height?23f/camera.aspect:17f;
            float lowest=camera.ViewportPointToRay(new Vector3(.5f,0,0)).origin.y;
            float upward=-camera.transform.forward.y;
            if(lowest<6f && upward>.01f)
                camera.transform.position-=camera.transform.forward*((6f-lowest)/upward);
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
            levelText.text="BASTIÓN "+Roman(visibleStage);
            resourceText.text="M "+Compact(s.Resources.Wood)+
                "   P "+Compact(s.Resources.Stone)+
                "   C "+Compact(s.Resources.Food);
            feedbackText.text=lastMessage;
            // Transient gateway feedback only, never a permanent status ribbon.
            feedbackText.gameObject.SetActive(Time.unscaledTime<feedbackExpires &&
                (popup==null||!popup.gameObject.activeSelf));
        }
        private static string Roman(int level)=>new[]{"I","II","III","IV","V"}[Mathf.Clamp(level-1,0,4)];
        private static string Compact(int number)
        {
            if(number>=1000000)return (number/1000000f).ToString("0.#")+"M";
            if(number>=10000)return (number/1000f).ToString("0.#")+"k";
            return number.ToString();
        }
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
                FrameCameraForViewport(playerCamera,Screen.width,Screen.height);
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
                feedbackExpires=Time.unscaledTime+2.8f;
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
                    new Vector2(0,bottom),new Vector2(0,top),spec.Item2,14);
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
                var description="El corazón de Valoria. Refuerza sus murallas para ampliar tu reino.";
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
                    "Desarrolla tus edificios para sostener el crecimiento de Valoria. " +
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
                    "\nPrepara tus tropas y elige la formación antes de partir.",
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
