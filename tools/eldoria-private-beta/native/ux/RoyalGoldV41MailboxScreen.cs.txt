using System;
using System.Collections.Generic;
using System.Linq;
using Eldoria.Application;
using Eldoria.Domain;
using Eldoria.UI.RoyalGold;
using UnityEngine;
using UnityEngine.UI;

namespace Eldoria.UI.RoyalGold.V2
{
    /// <summary>
    /// One-canvas functional mailbox. Rendering uses existing Royal Gold components only.
    /// Artwork is a TEMPORARY structural shell, NOT the owner's final art-approved screen.
    /// Keep the PR in draft until exact-reference backgrounds, tabs and mobile A/B pass.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class RoyalGoldV41MailboxScreen : MonoBehaviour
    {
        public bool IsOpen=>window!=null&&window.gameObject.activeSelf;
        public Button EntryButton=>entry?entry.GetComponent<Button>():null;
        public int UnreadCount=>gateway?.Snapshot()?.Mailbox?.Unread()??0;
        public Func<bool> CanShowEntry { get; set; }
        RoyalHUD hud;ICommandGateway gateway;
        RectTransform window,entry;
        RoyalText entryLabel,title,detail,navLabel,resources;
        RectTransform detailSheet;
        RoyalGoldV41ReportArtwork reportArtwork;
        RawImage scoutVignette;
        Texture2D scoutVignettesAtlas;
        int scoutFamily=-1;
        RoyalText reportKicker,reportHeading,reportOrigin,reportDate,reportSeal;
        RoyalText reportSection,reportFooter,reportToggleText,reportComposition;
        readonly RoyalText[] metricNames=new RoyalText[4];
        readonly RoyalText[] metricValues=new RoyalText[4];
        Button reportToggle;
        bool reportExpanded=true;
        int reportMetricCount;
        RoyalGoldV41ReportArtwork.Motif reportMotif;
        readonly RoyalText[] resourceAmounts=new RoyalText[3];
        readonly RoyalIconGraphic[] resourceIcons=new RoyalIconGraphic[3];
        readonly List<Image> messageIcons=new List<Image>();
        readonly List<RoyalText> messageDates=new List<RoyalText>();
        readonly List<RoyalText> messageSubtitles=new List<RoyalText>();
        readonly List<RoyalText> tabBadges=new List<RoyalText>();
        readonly List<RoyalText> tabLabels=new List<RoyalText>();
        readonly List<RoyalText> messageLabels=new List<RoyalText>();
        readonly List<Button> messageButtons=new List<Button>();
        readonly List<Image> messageSurfaces=new List<Image>();
        readonly List<Image> messageUnreadDots=new List<Image>();
        readonly List<RoyalText> messageArrows=new List<RoyalText>();
        RectTransform listBoard,detailBoard,detailViewport;
        RoyalText emptyState,listSummary;
        Image watermark;
        ScrollRect detailScroll;
        int visibleRowCount=-1;
        int currentPageSize=5;
        bool detailVisible;
        public int VisibleMessageRows=>messageButtons.Count(b=>b&&b.gameObject.activeSelf);
        // Read-only QA observations; preserve command authority and save logic.
        public int CurrentPageOffset=>offset;
        public int CurrentPageSize=>currentPageSize;
        public static int VisibleCountFor(int total,int offset,int pageSize=5)=>
            Mathf.Clamp(total-offset,0,Mathf.Clamp(pageSize,1,5));
        string[] categories={"informes","recompensas","sistema","narrativa"};
        string category="informes",selectedId;
        int offset;
        float lastWidth,lastHeight,nextRefresh,lastCanvasScale;
        float layoutUnit=1f;
        static readonly Color Night=new Color(.025f,.046f,.076f,1);
        static readonly Color Muted=new Color(.79f,.82f,.83f,1);
        static readonly Color Ivory=new Color(.96f,.93f,.88f,1);
        static readonly Color Gold=new Color(.90f,.72f,.40f,1);

        public void Initialize(RoyalHUD owner,ICommandGateway authority)
        {
            hud=owner;gateway=authority;
            if(hud!=null && gateway!=null)Ensure();
            Refresh();
        }
        void OnEnable(){Canvas.willRenderCanvases+=BeforeCanvasRender;}
        void OnDisable(){Canvas.willRenderCanvases-=BeforeCanvasRender;}
        void BeforeCanvasRender()
        {
            if(!window||!hud)return;
            if(Mathf.Abs(window.rect.width-lastWidth)>.5f||
                Mathf.Abs(window.rect.height-lastHeight)>.5f||
                !Mathf.Approximately(lastCanvasScale,hud.GetComponentInParent<Canvas>()?.scaleFactor??1f))
            {
                // An aspect-ratio change can switch 5-row portrait pagination
                // to 2-row compact landscape pagination. Recreate the real
                // visible rows, not just their RectTransforms.
                if(IsOpen)Refresh();else Layout();
            }
            RefreshEntryVisibility();
        }
        bool MailboxIntroduced
        {
            get
            {
                var mailbox=gateway?.Snapshot()?.Mailbox;
                // The first real delivery introduces mail. Its event ledger
                // survives deletion, so an introduced destination stays visible.
                return mailbox!=null&&((mailbox.ReceivedEventIds?.Count??0)>0 ||
                    (mailbox.Messages?.Count??0)>0);
            }
        }
        void RefreshEntryVisibility()
        {
            if(!entry||!hud)return;
            entry.gameObject.SetActive(!(hud.Navigation&&hud.Navigation.ChronicleLayout)&&MailboxIntroduced&&!IsOpen&&!hud.NarrativeOpen&&
                !(hud.Modal!=null&&hud.Modal.gameObject.activeSelf)&&
                !(hud.GetComponent<RoyalGoldV3ManagementScreens>()?.IsOpen??false)&&
                !(hud.GetComponent<RoyalGoldV3EarlyRetentionScreens>()?.IsOpen??false)&&
                (CanShowEntry==null||CanShowEntry())&&string.IsNullOrEmpty(hud.SelectedBuildingId));
        }
        public void Open()
        {
            if(hud==null||gateway==null || hud.NarrativeOpen ||
               (hud.Modal!=null&&hud.Modal.gameObject.activeSelf))return;
            Ensure();window.gameObject.SetActive(true);
            window.SetAsLastSibling();offset=0;selectedId=null;
            Layout();Refresh();
        }
        public void Close()
        {
            if(window!=null)window.gameObject.SetActive(false);
            Layout();Refresh();
        }
        void Update()
        {
            if(hud==null||gateway==null)return;
            if(IsOpen && hud.NarrativeOpen){Close();return;}
            if(window!=null && (Mathf.Abs(window.rect.width-lastWidth)>.5f ||
                          Mathf.Abs(window.rect.height-lastHeight)>.5f))
            {
                if(IsOpen)Refresh();else Layout();
            }
            if(Time.unscaledTime>=nextRefresh)
            {
                nextRefresh=Time.unscaledTime+.6f;
                Refresh();
            }
        }
        static RectTransform Node(string name,Transform parent)
        {
            var go=new GameObject(name,typeof(RectTransform));
            go.transform.SetParent(parent,false);return (RectTransform)go.transform;
        }
        static RoyalText Label(string name,Transform parent,int fontSize,bool strong=false)
        {
            var r=Node(name,parent);
            r.gameObject.AddComponent<CanvasRenderer>();
            var t=r.gameObject.AddComponent<RoyalText>();
            t.font=RoyalGoldV41SourceArt.Serif;
            t.fontSize=fontSize;t.fontStyle=strong?FontStyle.Bold:FontStyle.Normal;
            t.color=Ivory;t.alignment=TextAnchor.MiddleLeft;
            t.horizontalOverflow=HorizontalWrapMode.Wrap;
            t.verticalOverflow=VerticalWrapMode.Truncate;
            t.raycastTarget=false;return t;
        }
        static Button MakeButton(string id,Transform parent,string text,Action action,out RoyalText label)
        {
            var rect=Node(id,parent);
            rect.gameObject.AddComponent<CanvasRenderer>();
            var graphic=rect.gameObject.AddComponent<RoyalGoldV2FrameGraphic>();
            graphic.DecorativeCorners=true;graphic.Primary=false;
            graphic.raycastTarget=true;graphic.color=Color.white;
            var button=rect.gameObject.AddComponent<Button>();
            button.targetGraphic=graphic;
            label=Label(id+" label",rect,17,true);label.Put(text);
            label.alignment=TextAnchor.MiddleCenter;
            label.rectTransform.anchorMin=Vector2.zero;
            label.rectTransform.anchorMax=Vector2.one;
            label.rectTransform.offsetMin=label.rectTransform.offsetMax=Vector2.zero;
            button.onClick.AddListener(()=>action());
            return button;
        }
        int Pixels(float logical)=>Mathf.Max(1,Mathf.RoundToInt(logical*layoutUnit));
        void Position(RectTransform r,float x,float y,float w,float h)
        {
            r.anchorMin=r.anchorMax=new Vector2(0,1);
            r.pivot=new Vector2(0,1);r.anchoredPosition=new Vector2(x,-y)*layoutUnit;
            r.sizeDelta=new Vector2(Mathf.Max(0,w),Mathf.Max(0,h))*layoutUnit;
        }
        void Ensure()
        {
            if(window!=null)return;
            if(hud==null)return;
            RoyalText buttonCaption;
            var btn=MakeButton("V41 / Buzón shortcut",hud.transform,"CORREO",Open,out buttonCaption);
            entry=(RectTransform)btn.transform;entryLabel=buttonCaption;
            var w=Node("V41 / Buzón destination",hud.transform);
            window=w;w.anchorMin=Vector2.zero;w.anchorMax=Vector2.one;
            w.offsetMin=w.offsetMax=Vector2.zero;
            w.gameObject.AddComponent<CanvasRenderer>();
            var bg=w.gameObject.AddComponent<Image>();bg.color=Night;bg.raycastTarget=true;
            // Full-scene painted vellum and brass underlay replaces the blank
            // black screen; it never owns interaction or game state.
            var scenery=Node("Mailbox / illustrated matte background",w);
            scenery.anchorMin=Vector2.zero;scenery.anchorMax=Vector2.one;
            scenery.offsetMin=scenery.offsetMax=Vector2.zero;
            scenery.gameObject.AddComponent<CanvasRenderer>();
            var matte=scenery.gameObject.AddComponent<RoyalGoldV41MailboxBackdrop>();
            matte.color=Color.white;matte.raycastTarget=false;
            listBoard=PaperFrame("Mailbox / adaptive collection frame",w,
                new Color(.19f,.155f,.115f,.95f),.68f);
            detailBoard=PaperFrame("Mailbox / report document",w,
                new Color(.925f,.865f,.752f,.99f),.86f);
            detailBoard.gameObject.SetActive(false);
            watermark=RoyalGoldV41SourceArt.Surface(w,"mail-narrative");
            watermark.name="Mailbox / passive manuscript watermark";
            watermark.color=new Color(.84f,.70f,.43f,.28f);
            watermark.raycastTarget=false;
            // Above the matte but behind the manuscript: a decorative
            // source icon, never a fabricated message or input.
            watermark.rectTransform.SetSiblingIndex(1);
            emptyState=Label("Mailbox / genuine empty state",w,24,true);
            emptyState.color=new Color(.93f,.83f,.64f);
            emptyState.alignment=TextAnchor.MiddleCenter;
            listSummary=Label("Mailbox / real collection summary",w,16);
            listSummary.color=new Color(.83f,.76f,.61f);
            title=Label("Buzón heading",w,29,true);title.Put("BUZÓN");
            resources=Label("Recursos actuales",w,16);
            resources.gameObject.SetActive(false); // Retain real backing data only.
            for(int i=0;i<3;i++)
            {
                var pictogram=Node("Mailbox / real resource icon "+i,w);
                pictogram.gameObject.AddComponent<CanvasRenderer>();
                resourceIcons[i]=pictogram.gameObject.AddComponent<RoyalIconGraphic>();
                resourceIcons[i].Icon=i+1;resourceIcons[i].raycastTarget=false;
                resourceIcons[i].color=i==0?
                    new Color(.98f,.72f,.34f):i==1?
                    new Color(.83f,.86f,.90f):new Color(.95f,.79f,.49f);
                resourceAmounts[i]=Label("Mailbox / real resource amount "+i,w,17,true);
                resourceAmounts[i].color=Ivory;
                resourceAmounts[i].alignment=TextAnchor.MiddleLeft;
            }
            MakeButton("Volver",w,"VOLVER",()=>{if(selectedId!=null){selectedId=null;Refresh();}else Close();},out _);
            for(int i=0;i<4;i++)
            {
                string key=categories[i];
                var button=MakeButton("Buzón "+key,w,key.ToUpperInvariant(),()=>SetCategory(key),out var text);
                tabLabels.Add(text);
                button.GetComponent<RoyalGoldV2FrameGraphic>().enabled=false;
                var surface=RoyalGoldV41SourceArt.Surface((RectTransform)button.transform,"tab-idle",16);
                surface.raycastTarget=true;button.targetGraphic=surface;
                var badge=RoyalGoldV41SourceArt.Surface((RectTransform)button.transform,"unread-dot");
                badge.name="Unread badge";badge.rectTransform.SetAsLastSibling();
                var count=Label("Unread count",badge.transform,16,true);count.alignment=TextAnchor.MiddleCenter;
                count.rectTransform.anchorMin=Vector2.zero;count.rectTransform.anchorMax=Vector2.one;
                count.rectTransform.offsetMin=count.rectTransform.offsetMax=Vector2.zero;tabBadges.Add(count);
            }
            for(int i=0;i<5;i++)
            {
                int slot=i;
                var button=MakeButton("Buzón / mensaje "+i,w,"",()=>SelectRow(slot),out var label);
                messageButtons.Add(button);messageLabels.Add(label);
                button.GetComponent<RoyalGoldV2FrameGraphic>().enabled=false;
                var surface=RoyalGoldV41SourceArt.Surface((RectTransform)button.transform,"mail-card",20);
                surface.raycastTarget=true;button.targetGraphic=surface;
                messageSurfaces.Add(surface);
                label.color=new Color(.08f,.07f,.05f);label.alignment=TextAnchor.MiddleLeft;
                var unread=RoyalGoldV41SourceArt.Surface((RectTransform)button.transform,"unread-dot");
                unread.name="Mailbox / actual unread indicator "+i;
                unread.raycastTarget=false;
                unread.rectTransform.SetAsLastSibling();
                messageUnreadDots.Add(unread);
                messageSubtitles.Add(Label("Mensaje resumen "+i,button.transform,17));
                messageDates.Add(Label("Fecha del mensaje "+i,button.transform,13));
                var icon=Node("Icono de mensaje "+i,button.transform).gameObject.AddComponent<Image>();
                icon.raycastTarget=false;icon.preserveAspect=true;messageIcons.Add(icon);
                var arrow=Label("Mailbox / message open chevron "+i,button.transform,28,true);
                arrow.Put("›");arrow.alignment=TextAnchor.MiddleCenter;
                arrow.color=new Color(.27f,.19f,.10f);messageArrows.Add(arrow);
            }
            MakeButton("Página anterior",w,"<",()=>Page(-1),out _);
            MakeButton("Página siguiente",w,">",()=>Page(1),out _);
            navLabel=Label("Página",w,14);
            detailViewport=Node("Mailbox / report scroll viewport",w);
            detailViewport.gameObject.AddComponent<CanvasRenderer>();
            var touchSurface=detailViewport.gameObject.AddComponent<Image>();
            touchSurface.color=Color.clear;touchSurface.raycastTarget=true;
            detailViewport.gameObject.AddComponent<RectMask2D>();
            detailScroll=detailViewport.gameObject.AddComponent<ScrollRect>();
            detailScroll.horizontal=false;detailScroll.vertical=true;
            detailScroll.inertia=false;detailScroll.movementType=ScrollRect.MovementType.Clamped;
            detailScroll.viewport=detailViewport;
            detailSheet=Node("Mailbox / scrollable assembled report",detailViewport);
            detailSheet.gameObject.AddComponent<CanvasRenderer>();
            reportArtwork=detailSheet.gameObject.AddComponent<RoyalGoldV41ReportArtwork>();
            reportArtwork.raycastTarget=false;
            // Exactly the same four real-source vignette quadrants as the
            // Explorers table. Every detail reads its persisted Kind; no new
            // map coordinates, unseen enemy or fabricated expedition exists.
            scoutVignettesAtlas=Resources.Load<Texture2D>("EldoriaRetention/scout-mission-vignettes-v4");
            var vignette=Node("Mailbox / authentic discovery family illustration",detailSheet);
            vignette.gameObject.AddComponent<CanvasRenderer>();
            scoutVignette=vignette.gameObject.AddComponent<RawImage>();
            scoutVignette.raycastTarget=false;
            scoutVignette.texture=scoutVignettesAtlas?
                scoutVignettesAtlas:Resources.Load<Texture2D>("EldoriaRetention/scouts-painterly-v2");
            scoutVignette.color=new Color(1f,.96f,.88f,1f);
            scoutVignette.gameObject.SetActive(false);
            reportKicker=Label("Mailbox / manuscript series",detailSheet,17,true);
            reportHeading=Label("Mailbox / report genuine heading",detailSheet,29,true);
            reportHeading.verticalOverflow=VerticalWrapMode.Overflow;
            reportOrigin=Label("Mailbox / actual event origin",detailSheet,15);
            reportDate=Label("Mailbox / stored event date",detailSheet,14);
            reportSeal=Label("Mailbox / outcome seal caption",detailSheet,15,true);
            reportSeal.alignment=TextAnchor.MiddleCenter;
            for(int i=0;i<4;i++)
            {
                metricNames[i]=Label("Mailbox / metric caption "+i,detailSheet,14,true);
                metricValues[i]=Label("Mailbox / genuine metric "+i,detailSheet,22,true);
            }
            reportComposition=Label("Mailbox / own roster historical composition",detailSheet,17,true);
            reportComposition.verticalOverflow=VerticalWrapMode.Overflow;
            reportSection=Label("Mailbox / report section heading",detailSheet,17,true);
            reportToggle=MakeButton("Mailbox / collapse or expand report",detailSheet,
                "OCULTAR",()=>{reportExpanded=!reportExpanded;Refresh();},out reportToggleText);
            detail=Label("Contenido del mensaje",detailSheet,19);
            detail.alignment=TextAnchor.UpperLeft;
            detail.verticalOverflow=VerticalWrapMode.Overflow;
            reportFooter=Label("Mailbox / original record and receipt",detailSheet,16);
            reportFooter.verticalOverflow=VerticalWrapMode.Overflow;
            detailScroll.content=detailSheet;
            MakeButton("Leer",w,"MARCAR LEÍDO",()=>ExecuteMail("MailRead"),out _);
            MakeButton("Eliminar",w,"ELIMINAR",()=>ExecuteMail("MailDelete"),out _);
            w.gameObject.SetActive(false);
            Layout();
        }
        public void SynchronizeLayout(){if(window!=null){Canvas.ForceUpdateCanvases();Layout();Refresh();}}
        static RectTransform PaperFrame(string name,Transform parent,Color color,float rimOpacity)
        {
            var rect=Node(name,parent);
            rect.gameObject.AddComponent<CanvasRenderer>();
            var back=rect.gameObject.AddComponent<Image>();
            back.color=color;back.raycastTarget=false;
            var edge=RoyalGoldV41SourceArt.Surface(rect,"HeroCardNineSliceGold",48);
            edge.name=name+" / independent metal nine-slice";
            if(edge.sprite)
            {
                edge.type=Image.Type.Sliced;edge.fillCenter=false;
                edge.pixelsPerUnitMultiplier=3.4f;
                edge.color=new Color(.93f,.73f,.38f,rimOpacity);
            }
            else edge.enabled=false;
            edge.raycastTarget=false;
            return rect;
        }
        void Layout()
        {
            if(window==null || gateway==null)return;
            float w=window.rect.width,h=window.rect.height;
            if(w<=0f||h<=0f)return;
            lastWidth=w;lastHeight=h;
            float canvasScale=hud.GetComponentInParent<Canvas>()?.scaleFactor??1f;
            lastCanvasScale=canvasScale;
            // Rasterize larger glyphs at their final size. Transform magnification
            // blurred the atlas and clipped list titles in genuine 844x390 captures.
            layoutUnit=IsOpen&&w>h&&h*canvasScale<600f?
                Mathf.Max(1f,.9f/Mathf.Max(.01f,canvasScale)):1f;
            window.localScale=Vector3.one;
            w/=layoutUnit;h/=layoutUnit;
            float scale=canvasScale*layoutUnit;
            Rect safe=Screen.safeArea;
            float left=Mathf.Max(12f,w*.026f+safe.xMin/Mathf.Max(.01f,scale));
            float right=Mathf.Max(12f,w*.026f+(Screen.width-safe.xMax)/Mathf.Max(.01f,scale));
            float top=Mathf.Max(12f,h*.018f+(Screen.height-safe.yMax)/Mathf.Max(.01f,scale));
            float bottom=Mathf.Max(12f,h*.018f+safe.yMin/Mathf.Max(.01f,scale));
            float usable=Mathf.Max(180f,w-left-right);
            bool wide=w>h;
            float fontScale=Mathf.Clamp(w/900f,.76f,1.38f);
            var buttons=window.GetComponentsInChildren<Button>(true);
            foreach(var button in buttons)
            {
                var caption=button.GetComponentInChildren<RoyalText>(true);
                if(caption)caption.fontSize=Pixels(17);
            }
            listSummary.fontSize=Pixels(16);
            Button Find(string name)=>buttons.First(b=>b.name==name);
            float entryWidth=Mathf.Clamp(w*.20f,90f,150f);
            Position(entry,w-right-entryWidth,h-bottom-(wide?112f:150f)-52f,entryWidth,44f);

            float headerH=wide?Mathf.Clamp(h*.085f,48f,79f):Mathf.Clamp(h*.072f,60f,100f);
            Position(title.rectTransform,left+Mathf.Max(53f,headerH*.67f),
                top,usable*.40f,headerH);
            title.fontSize=Pixels(wide?Mathf.Clamp(Mathf.RoundToInt(w*.023f),25,38):
                Mathf.Clamp(Mathf.RoundToInt(w*.042f),25,37));
            Position(resources.rectTransform,left+usable*.53f,top+2f,usable*.46f,headerH-4f);
            resources.fontSize=Pixels(Mathf.Clamp(Mathf.RoundToInt(17f*fontScale),15,22));
            // Three genuine live counters reuse the kingdom's own vector
            // resource glyphs. No fictitious "+" purchase action.
            float resourceX=left+usable*.51f;
            float stripWidth=usable*.49f;
            float resourceW=stripWidth/3f;
            for(int i=0;i<3;i++)
            {
                float glyphSize=Mathf.Clamp(resourceW*.25f,15f,31f);
                Position((RectTransform)resourceIcons[i].transform,
                    resourceX+i*resourceW+3f,top+(headerH-glyphSize)*.5f,
                    glyphSize,glyphSize);
                Position(resourceAmounts[i].rectTransform,
                    resourceX+i*resourceW+glyphSize+6f,top+3f,
                    Mathf.Max(24f,resourceW-glyphSize-8f),headerH-6f);
                resourceAmounts[i].fontSize=Pixels(Mathf.Clamp(
                    Mathf.RoundToInt(18f*fontScale),15,24));
            }
            var back=Find("Volver");
            Position((RectTransform)back.transform,left,top,Mathf.Max(48f,headerH*.72f),headerH);
            back.GetComponentInChildren<RoyalText>().Put("<");

            float tabsY=top+headerH+Mathf.Clamp(h*.028f,9f,24f);
            float tabH=wide?Mathf.Clamp(h*.088f,45f,72f):
                Mathf.Clamp(h*.079f,49f,91f);
            float tabGap=wide?Mathf.Clamp(w*.008f,6f,15f):5f;
            float tabW=(usable-3f*tabGap)/4f;
            for(int i=0;i<4;i++)
            {
                var b=Find("Buzón "+categories[i]);
                Position((RectTransform)b.transform,left+i*(tabW+tabGap),
                    tabsY,tabW,tabH);
                tabLabels[i].fontSize=Pixels(Mathf.Clamp(
                    Mathf.RoundToInt(tabW*(wide?.112f:.135f)),15,27));
                tabLabels[i].horizontalOverflow=HorizontalWrapMode.Wrap;
                Position((RectTransform)tabBadges[i].transform.parent,
                    tabW-23f,-9f,27f,27f);
                tabBadges[i].fontSize=Pixels(15);
            }

            var mail=gateway.Snapshot().Mailbox;
            int total=mail?.Messages?.Count(m=>m.Category==category)??0;
            // Portrait and desktop fit five messages. A very short mobile
            // landscape page fits only TWO readable cards; pagination keeps
            // the full list and its authoritative order.
            int pageSize=wide && h*scale<600f ? 2 : 5;
            if(pageSize!=currentPageSize)
                offset=(offset/pageSize)*pageSize;
            currentPageSize=pageSize;
            offset=Mathf.Clamp(offset,0,
                Mathf.Max(0,(Mathf.Max(0,total-1)/currentPageSize)*currentPageSize));
            int count=VisibleCountFor(total,offset,currentPageSize);
            bool selected=!string.IsNullOrEmpty(selectedId)&&
                mail?.Messages?.Any(m=>m.EventId==selectedId)==true;
            float contentTop=tabsY+tabH+Mathf.Clamp(h*.022f,12f,24f);
            float navH=wide?Mathf.Clamp(h*.055f,39f,51f):47f;
            float available=Mathf.Max(110f,h-contentTop-bottom-navH-7f);
            float rowH=wide?Mathf.Clamp(h*.14f,layoutUnit>1f?88f:71f,124f):
                Mathf.Clamp(h*.105f,87f,145f);
            rowH=Mathf.Max(48f,
                Mathf.Min(rowH,(available-46f)/Mathf.Max(1,count)));
            float bodyHeight=selected?available:Mathf.Min(available,
                Mathf.Max(wide?143f:196f,Mathf.Max(1,count)*rowH+46f));
            float margin=selected?0f:Mathf.Min(78f,Mathf.Max(0f,available-bodyHeight)*.10f);
            float bodyY=contentTop+margin;
            Position(listBoard,left,bodyY,usable,bodyHeight);
            listBoard.gameObject.SetActive(!selected);
            Position(detailBoard,left,contentTop,usable,available);
            detailBoard.gameObject.SetActive(selected);
            float lowerStart=bodyY+bodyHeight+Mathf.Min(30f,h*.035f);
            float lowerEnd=h-bottom-navH;
            float lowerRoom=Mathf.Max(0f,lowerEnd-lowerStart);
            float watermarkSize=Mathf.Min(usable*.55f,
                Mathf.Clamp(lowerRoom*.65f,105f,460f));
            Position(watermark.rectTransform,
                left+(usable-watermarkSize)*.5f,
                lowerStart+Mathf.Max(0f,(lowerRoom-watermarkSize)*.5f),
                watermarkSize,watermarkSize);
            watermark.gameObject.SetActive(!selected && count<=1);
            Position(listSummary.rectTransform,left+18f,
                bodyY+bodyHeight-33f,usable-36f,27f);
            listSummary.gameObject.SetActive(!selected);
            Position(emptyState.rectTransform,left+30f,bodyY+42f,
                usable-60f,Mathf.Max(65f,bodyHeight-86f));
            emptyState.fontSize=Pixels(Mathf.Clamp(Mathf.RoundToInt(23f*fontScale),19,28));
            emptyState.gameObject.SetActive(!selected && count==0);
            float rowPad=wide?12f:11f;
            float innerW=usable-2f*rowPad;
            for(int i=0;i<messageButtons.Count;i++)
            {
                var button=messageButtons[i];
                float rowY=bodyY+rowPad+i*rowH;
                Position((RectTransform)button.transform,
                    left+rowPad,rowY,innerW,rowH-6f);
                float iconSize=Mathf.Clamp((rowH-6f)*.58f,40f,92f);
                float iconX=Mathf.Max(9f,rowH*.09f);
                float iconY=(rowH-6f-iconSize)*.50f;
                Position(messageIcons[i].rectTransform,iconX,iconY,iconSize,iconSize);
                Position(messageUnreadDots[i].rectTransform,
                    iconX-7f,iconY-8f,wide?19f:18f,wide?19f:18f);
                float textX=iconX+iconSize+Mathf.Clamp(w*.016f,10f,22f);
                float arrowW=wide?37f:27f;
                float copyWidth=Mathf.Max(70f,innerW-textX-arrowW-16f);
                Position(messageLabels[i].rectTransform,textX,
                    Mathf.Max(6f,rowH*.12f),copyWidth,rowH*.35f);
                Position(messageSubtitles[i].rectTransform,textX,rowH*.52f,
                    Mathf.Max(45f,copyWidth-(wide?130f:72f)),rowH*.31f);
                float dateW=wide?119f:65f;
                Position(messageDates[i].rectTransform,
                    textX+copyWidth-dateW,rowH*.53f,dateW,rowH*.29f);
                Position(messageArrows[i].rectTransform,
                    innerW-arrowW-7f,(rowH-6f-36f)*.50f,
                    arrowW,36f);
                messageArrows[i].fontSize=Pixels(28);
                messageLabels[i].fontSize=Pixels(Mathf.Clamp(
                    Mathf.RoundToInt((wide?23f:26f)*fontScale),18,30));
                messageSubtitles[i].fontSize=Pixels(Mathf.Clamp(
                    Mathf.RoundToInt((wide?17f:21f)*fontScale),15,25));
                messageDates[i].fontSize=Pixels(Mathf.Clamp(
                    Mathf.RoundToInt((wide?14f:16f)*fontScale),13,18));
            }
            float actionH=Mathf.Clamp(available*.13f,40f,57f);
            float detailPad=layoutUnit>1f?12f/scale:Mathf.Clamp(usable*.038f,15f,35f);
            Position(detailViewport,left+detailPad,contentTop+detailPad,
                usable-detailPad*2f,available-actionH-detailPad*3f);
            LayoutReport(detailViewport.rect.width/layoutUnit,detailViewport.rect.height/layoutUnit);
            detailScroll.verticalNormalizedPosition=Mathf.Clamp01(
                detailScroll.verticalNormalizedPosition);
            var read=Find("Leer");var del=Find("Eliminar");
            Position((RectTransform)read.transform,
                left+detailPad,contentTop+available-actionH-detailPad,
                (usable-3f*detailPad)*.5f,actionH);
            Position((RectTransform)del.transform,
                left+detailPad*2f+(usable-3f*detailPad)*.5f,
                contentTop+available-actionH-detailPad,
                (usable-3f*detailPad)*.5f,actionH);
            bool showPages=!selected&&total>currentPageSize;
            var prev=Find("Página anterior");var next=Find("Página siguiente");
            prev.gameObject.SetActive(showPages);next.gameObject.SetActive(showPages);
            float navY=h-bottom-navH;
            Position((RectTransform)prev.transform,left,navY,44f,navH);
            Position(navLabel.rectTransform,left+(showPages?53f:5f),
                navY,usable-(showPages?106f:10f),navH);
            navLabel.alignment=showPages?TextAnchor.MiddleCenter:TextAnchor.MiddleLeft;
            navLabel.fontSize=Pixels(Mathf.Clamp(Mathf.RoundToInt(16f*fontScale),14,21));
            Position((RectTransform)next.transform,left+usable-44f,navY,44f,navH);
            visibleRowCount=count;detailVisible=selected;
        }
        void LayoutReport(float width,float viewportHeight)
        {
            if(detailSheet==null||width<100f)return;
            bool narrow=width<580f;
            bool shortViewport=viewportHeight<230f;
            float inner=width-48f;
            reportKicker.fontSize=Pixels(narrow?15:19);
            reportHeading.fontSize=Pixels(shortViewport?24:narrow?24:32);
            reportHeading.color=new Color(.96f,.85f,.64f);
            reportKicker.color=new Color(.91f,.73f,.42f);
            reportOrigin.color=new Color(.94f,.86f,.72f);
            reportDate.color=new Color(.87f,.77f,.60f);
            reportSeal.color=new Color(.99f,.86f,.61f);
            Position(reportKicker.rectTransform,24f,shortViewport?10f:25f,Mathf.Max(75f,width-151f),26f);
            float headingTop=shortViewport?37f:58f;
            Position(reportHeading.rectTransform,24f,headingTop,
                Mathf.Max(76f,width-153f),50f);
            float headingH=Mathf.Max(shortViewport?32f:55f,reportHeading.preferredHeight/layoutUnit+6f);
            Position(reportHeading.rectTransform,24f,headingTop,
                Mathf.Max(76f,width-153f),headingH);
            float headerH=Mathf.Max(shortViewport?100f:150f,headingTop+headingH+(shortViewport?32f:56f));
            Position(reportSeal.rectTransform,width-99f,45f,69f,51f);
            reportSeal.fontSize=Pixels(narrow?12:15);
            Position(reportOrigin.rectTransform,24f,headerH-35f,
                Mathf.Max(75f,inner*.65f),25f);
            Position(reportDate.rectTransform,24f+inner*.65f,headerH-35f,
                inner*.35f,25f);
            reportOrigin.fontSize=Pixels(narrow?12:15);
            reportDate.fontSize=Pixels(narrow?11:14);

            int cols=narrow?2:4;
            float cardH=narrow?104f:94f;
            float gap=11f,cardW=(width-48f-gap*(cols-1))/cols;
            float metricTop=headerH+19f;
            float captionH=24f,valueH=0f;
            // Measure the real saved names at the final column width. At
            // 900x1600 four columns can wrap both heroes onto several lines.
            for(int i=0;i<reportMetricCount;i++)
            {
                metricNames[i].fontSize=Pixels(narrow?12:15);
                metricValues[i].fontSize=Pixels(narrow?18:26);
                Position(metricNames[i].rectTransform,0f,0f,cardW-22f,24f);
                Position(metricValues[i].rectTransform,0f,0f,cardW-22f,55f);
                captionH=Mathf.Max(captionH,metricNames[i].preferredHeight/layoutUnit+3f);
                valueH=Mathf.Max(valueH,metricValues[i].preferredHeight/layoutUnit+5f);
            }
            float valueTop=10f+captionH;
            cardH=Mathf.Max(cardH,valueTop+valueH+5f);
            for(int i=0;i<4;i++)
            {
                bool visible=i<reportMetricCount;
                metricNames[i].gameObject.SetActive(visible);
                metricValues[i].gameObject.SetActive(visible);
                if(!visible)continue;
                int cx=i%cols,cy=i/cols;
                float x=24f+cx*(cardW+gap),y=metricTop+cy*(cardH+gap);
                metricNames[i].color=new Color(.37f,.26f,.17f);
                metricValues[i].color=new Color(.20f,.125f,.075f);
                metricNames[i].fontSize=Pixels(narrow?12:15);
                metricValues[i].fontSize=Pixels(narrow?18:26);
                Position(metricNames[i].rectTransform,x+11f,y+10f,cardW-22f,captionH);
                Position(metricValues[i].rectTransform,x+11f,y+valueTop,
                    cardW-22f,cardH-valueTop-5f);
            }
            float metricHeight=reportMetricCount>0?
                Mathf.Ceil(reportMetricCount/(float)cols)*(cardH+gap)-gap:0f;
            float compositionTop=metricTop+metricHeight+14f;
            float compositionH=0f;
            if(reportMetricCount>0)
            {
                Position(reportComposition.rectTransform,30f,compositionTop,
                    width-60f,36f);
                reportComposition.fontSize=Pixels(narrow?15:18);
                reportComposition.color=new Color(.41f,.27f,.14f);
                compositionH=Mathf.Max(42f,reportComposition.preferredHeight/layoutUnit+15f);
                Position(reportComposition.rectTransform,30f,compositionTop,
                    width-60f,compositionH);
            }
            float storyTop=compositionTop+compositionH+16f;
            Rect artPicture=Rect.zero;
            if(scoutVignette!=null && scoutVignette.gameObject.activeSelf &&
                scoutVignette.texture!=null)
            {
                float imageWidth=Mathf.Min(width-64f,narrow?width-64f:width*.68f);
                float imageHeight=narrow?
                    Mathf.Min(180f,imageWidth*.58f):Mathf.Min(244f,imageWidth*.46f);
                float imageX=(width-imageWidth)*.5f;
                float imageY=compositionTop+compositionH+10f;
                Position(scoutVignette.rectTransform,imageX,imageY,imageWidth,imageHeight);
                var source=scoutVignette.texture;
                float aspect=imageWidth/Mathf.Max(1f,imageHeight);
                float sourceAspect=(float)source.width/Mathf.Max(1,source.height);
                var kind=Mathf.Clamp(scoutFamily,0,3);
                // Preserve the exact inset 2x2 source atlas family mapping
                // from RoyalGoldV3EarlyRetentionScreens.
                Rect tile=scoutVignettesAtlas!=null?
                    new Rect((kind%2)*.5f+.01f,(kind<2?.5f:0f)+.01f,.48f,.48f):
                    new Rect(0f,0f,1f,1f);
                float cropW=tile.width,cropH=cropW*sourceAspect/aspect;
                if(cropH>tile.height)
                {
                    cropH=tile.height;
                    cropW=cropH*aspect/sourceAspect;
                }
                scoutVignette.uvRect=new Rect(tile.center.x-cropW*.5f,
                    tile.center.y-cropH*.5f,cropW,cropH);
                artPicture=new Rect(imageX,imageY,imageWidth,imageHeight);
                storyTop=imageY+imageHeight+29f;
            }
            reportSection.color=new Color(.35f,.225f,.105f);
            reportSection.fontSize=Pixels(narrow?16:19);
            Position(reportSection.rectTransform,38f,storyTop+18f,
                narrow?width-76f:Mathf.Max(75f,width-210f),narrow?46f:30f);
            Position((RectTransform)reportToggle.transform,width-178f,
                storyTop+(narrow?66f:13f),139f,37f);
            reportToggleText.fontSize=Pixels(narrow?12:15);
            reportToggleText.Put(reportExpanded?"OCULTAR":"DESPLEGAR");
            detail.fontSize=Pixels(narrow?19:23);
            detail.color=new Color(.25f,.175f,.115f);
            detail.gameObject.SetActive(reportExpanded);
            float textH=0f;
            float storyContentTop=narrow?111f:59f;
            if(reportExpanded)
            {
                Position(detail.rectTransform,40f,storyTop+storyContentTop,width-80f,40f);
                textH=Mathf.Max(40f,detail.preferredHeight/layoutUnit+12f);
                Position(detail.rectTransform,40f,storyTop+storyContentTop,width-80f,textH);
            }
            float storyHeight=reportExpanded?
                Mathf.Max(storyContentTop+textH+25f,narrow?169f:111f):
                (narrow?111f:69f);
            float footerTop=storyTop+storyHeight+20f;
            reportFooter.fontSize=Pixels(narrow?16:19);
            reportFooter.color=new Color(.38f,.25f,.14f);
            Position(reportFooter.rectTransform,26f,footerTop,width-52f,70f);
            float footerH=Mathf.Max(48f,reportFooter.preferredHeight/layoutUnit+10f);
            Position(reportFooter.rectTransform,26f,footerTop,width-52f,footerH);
            float totalHeight=Mathf.Max(viewportHeight+4f,footerTop+footerH+32f);
            // Refresh runs while the reader is scrolling. Preserve the reading
            // position; SelectRow explicitly resets it for a different report.
            float readingOffset=detailSheet.anchoredPosition.y;
            Position(detailSheet,0f,0f,width,totalHeight);
            detailSheet.anchoredPosition=new Vector2(0f,
                Mathf.Clamp(readingOffset,0f,Mathf.Max(0f,(totalHeight-viewportHeight)*layoutUnit)));
            reportArtwork.Configure(reportMotif,reportMetricCount,cols,metricTop,
                cardH,storyTop,storyHeight,footerTop+footerH,artPicture,layoutUnit);
        }
        void SetCategory(string value){category=value;offset=0;selectedId=null;reportExpanded=true;Refresh();}
        void Page(int delta)
        {
            var s=gateway?.Snapshot();
            int total=s?.Mailbox?.Messages.Count(m=>m.Category==category)??0;
            offset=Mathf.Clamp(offset+delta*currentPageSize,0,
                Mathf.Max(0,(Mathf.Max(0,total-1)/currentPageSize)*currentPageSize));
            selectedId=null;Refresh();
        }
        void SelectRow(int index)
        {
            var list=gateway?.Snapshot()?.Mailbox?.List(category,offset,currentPageSize);
            if(list==null||index>=list.Count)return;
            selectedId=list[index].EventId;
            reportExpanded=string.IsNullOrEmpty(list[index].Battle?.Outcome);
            if(detailScroll!=null)detailScroll.verticalNormalizedPosition=1f;
            ExecuteMail("MailRead");
        }
        void ExecuteMail(string kind)
        {
            if(string.IsNullOrEmpty(selectedId)||gateway==null)return;
            var s=gateway.Snapshot();
            var result=gateway.Execute(new GameCommand(Guid.NewGuid().ToString("N"),s.PlayerId,
                kind,selectedId,s.Revision));
            if(result.Ok && kind=="MailDelete")selectedId=null;
            Refresh();
        }
        static string HeroName(string id)=>id=="aldric+lyra"?"Sir Aldric y Lyra":id=="lyra"?"Lyra":id=="aldric"?"Sir Aldric":"—";
        static string TargetName(string id)
        {
            if((id??"").StartsWith("vigilant:",StringComparison.Ordinal)&&
                int.TryParse(id.Substring(9),out int stage)&&stage>0){
                var encounter=EarlyRetentionRules.Stage(stage);
                return "Etapa "+stage+" · "+encounter.Name;
            }
            switch(id){case "corrupt-scout":return "Acechador Corrupto";case "engendro-valoria":return "Engendro";
                case "hunt-wolf":return "Lobo";case "hunt-boar":return "Jabalí";case "ash-stalker-valoria":return "Acechador de Ceniza";
                case "fissure-valoria":return "Fisura";default:return (id??"").StartsWith("vigilant:")?"Vigilante":"el enemigo";}
        }
        static string CompactAmount(int value)
        {
            if(value>=1000000)return (value/1000000f).ToString("0.#")+"M";
            if(value>=1000)return (value/1000f).ToString("0.#")+"K";
            return value.ToString();
        }
        static string ObservedReason(EldoriaV41CombatRecord battle)
        {
            string reason=(battle?.Reason??"").Trim();
            // A legacy defeat reason may carry automatic tactical advice.
            // The illustrated report only displays the observed outcome.
            // Native resolver reasons sometimes contain an action directive
            // or a shorthand about damage/break. The report documents only
            // what the player's own march observed, never derives tactics.
            if(reason.StartsWith("Prepara otra composición:",StringComparison.OrdinalIgnoreCase) ||
                reason.IndexOf("Refuerza las tropas",StringComparison.OrdinalIgnoreCase)>=0 ||
                reason.IndexOf("Mejora las tropas",StringComparison.OrdinalIgnoreCase)>=0)
                return "La expedición no superó el encuentro registrado.";
            if(reason.IndexOf("Ruptura super",StringComparison.OrdinalIgnoreCase)>=0)
                return "La marcha venció al objetivo registrado.";
            return !string.IsNullOrEmpty(reason)?reason:
                battle!=null&&battle.Won?"Encuentro superado.":"No se registró victoria.";
        }
        void PresentReport(EldoriaV41Message selected)
        {
            var battle=selected.Battle;
            bool scouting=battle==null&&selected.Type=="exploration";
            // Hunting records have no combat rounds/health. Showing 0 rounds
            // and 0% life under a victory heading falsely implies a battle.
            bool hunt=battle!=null&&(battle.TargetId=="hunt-wolf"||battle.TargetId=="hunt-boar");
            bool hasNarrative=scouting&&!string.IsNullOrEmpty(selected.DiscoveryText);
            reportMetricCount=battle!=null?(hunt?2:4):0;
            scoutFamily=hasNarrative?selected.DiscoveryKind:-1;
            if(scoutVignette!=null)
                scoutVignette.gameObject.SetActive(hasNarrative&&scoutFamily>=0&&
                    scoutFamily<=3&&scoutVignette.texture!=null);
            reportMotif=battle!=null?(battle.EffectiveOutcome=="draw"?RoyalGoldV41ReportArtwork.Motif.Archive:battle.EffectiveOutcome=="victory"?
                RoyalGoldV41ReportArtwork.Motif.BattleVictory:
                RoyalGoldV41ReportArtwork.Motif.BattleDefeat):
                hasNarrative&&selected.DiscoveryKind>=0&&selected.DiscoveryKind<=3?
                (RoyalGoldV41ReportArtwork.Motif)((int)RoyalGoldV41ReportArtwork.Motif.Frontier+
                    selected.DiscoveryKind):RoyalGoldV41ReportArtwork.Motif.Archive;
            bool compact=detailViewport!=null&&detailViewport.rect.width<580f;
            reportKicker.Put(battle!=null?(compact?"CRÓNICA DEL CAMINO":
                "CRÓNICA DEL CAMINO · INFORME AUTÉNTICO"):
                scouting?(compact?"HALLAZGO REGISTRADO":
                "CUADERNO DEL CARTÓGRAFO · HALLAZGO REGISTRADO"):
                compact?"ARCHIVO REAL":"ARCHIVO REAL · MENSAJE RECIBIDO");
            reportHeading.Put(hunt?(battle.Won?"Cacería completada":"Cacería sin éxito"):
                scouting&&!string.IsNullOrEmpty(selected.Subtitle)?
                selected.Subtitle:selected.Title);
            // Result and family are spelled out in the real title/kicker.
            // Let the raised wax seal remain a legible original engraving.
            reportSeal.Put("");
            reportOrigin.Put(hunt?"CACERÍA · "+TargetName(battle.TargetId):
                battle!=null?"ENCUENTRO · "+TargetName(battle.TargetId):
                scouting&&hasNarrative?
                    "ORIGEN · "+(selected.DiscoveryKind==0?"FRONTERA":
                        selected.DiscoveryKind==1?"CAMINO":
                        selected.DiscoveryKind==2?"SUMINISTROS":
                        selected.DiscoveryKind==3?"BRECHA":"ENCARGO"):
                scouting?"INFORME DE EXPLORACIÓN":"ARCHIVO DE VALORIA");
            reportDate.Put(selected.OccurredAtUtcTicks>0?
                new DateTime(selected.OccurredAtUtcTicks,DateTimeKind.Utc).ToString("dd/MM/yy HH:mm"):"");
            if(battle!=null)
            {
                string[] names={"HÉROES","TROPAS ENVIADAS","RONDAS","VIDA RESTANTE"};
                string[] values={HeroName(battle.HeroId),battle.Troops.ToString(),
                    battle.Rounds.ToString(),battle.RemainingHealth.ToString()+(string.IsNullOrEmpty(battle.Outcome)?"":"%")};
                for(int i=0;i<4;i++)
                {
                    metricNames[i].Put(names[i]);metricValues[i].Put(values[i]);
                }
                reportComposition.gameObject.SetActive(true);
                var own=battle.OwnRoster;
                reportComposition.Put(!string.IsNullOrEmpty(battle.PlayerComposition)?
                    "COMPOSICIÓN PROPIA · "+battle.PlayerComposition:own!=null&&own.Total>0?
                    "COMPOSICIÓN PROPIA · ARQUEROS  T1: "+own.ArcherT1+
                    "  ·  T2: "+own.ArcherT2+"  ·  T3: "+own.ArcherT3:
                    "COMPOSICIÓN PROPIA · desglose no archivado en este informe");
                bool evolved=!string.IsNullOrEmpty(battle.Outcome);
                reportSection.Put(hunt?"RESULTADO DE LA CACERÍA":evolved?"MI MARCHA · ANÁLISIS OPCIONAL":"OBSERVACIONES DEL ENCUENTRO");
                detail.Put((hunt?(battle.Won?"La cacería regresó con su botín.":"La cacería no tuvo éxito."):ObservedReason(battle))+(evolved&&!hunt?
                    "\n\nDESGASTE OBSERVADO · "+battle.ObservedAttrition+
                    "\n\nDAÑO PROPIO CAUSADO · "+battle.OwnDamageByFamily+
                    "\n\nDAÑO PROPIO RECIBIDO · "+battle.OwnReceivedByFamily+
                    "\n\nVIDA PROPIA RESTANTE · "+battle.OwnRemainingByFamily:""));
                reportFooter.Put(hunt?
                    "PODER DE LA MARCHA · "+battle.PlayerPower+
                    "\nEl botín de la cacería se entrega al regresar. Consulta Recompensas para ver las cantidades recibidas.":
                    (evolved?"RESISTENCIA OBSERVADA DEL ENEMIGO · "+battle.EnemyObservedResistance:
                    "PODER PROPIO REGISTRADO · "+battle.PlayerPower)+
                    "\nBOTÍN REGISTRADO · "+battle.RewardWood+" madera · "+
                    battle.RewardStone+" piedra · "+battle.RewardFood+" comida"+
                    (string.IsNullOrEmpty(selected.RewardReceiptId)?"":
                    "\nLa recompensa ya estaba registrada; abrir el informe no la duplica."));
            }
            else if(scouting)
            {
                reportComposition.gameObject.SetActive(false);
                reportSection.Put("RELATO CONSERVADO");
                detail.Put(hasNarrative?selected.DiscoveryText:
                    "Este mensaje anterior solo conserva el título. No se recrea un relato sin el registro original.");
                reportFooter.Put(!string.IsNullOrEmpty(selected.DiscoveryOrderId)?
                    "PROCEDENCIA · ENCARGO REAL ARCHIVADO":
                    "PROCEDENCIA · MENSAJE ANTIGUO O SIN REFERENCIA DE ORDEN");
                if(!string.IsNullOrEmpty(selected.RewardReceiptId))
                    reportFooter.Put(reportFooter.text+
                        "\nRecompensa registrada al reclamar; leer no realiza otro abono.");
            }
            else
            {
                reportComposition.gameObject.SetActive(false);
                reportSection.Put("CONTENIDO RECIBIDO");
                detail.Put(selected.Subtitle??"");
                reportFooter.Put("MENSAJE DEL REGISTRO DEL REINO");
            }
            reportToggle.gameObject.SetActive(!string.IsNullOrEmpty(detail.text));
        }
        public void Refresh()
        {
            if(hud==null||gateway==null)return;
            if(window==null)Ensure();
            var s=gateway.Snapshot();var mail=s.Mailbox;
            RefreshEntryVisibility();
            resources.Put("Madera "+s.Resources.Wood+"  ·  Piedra "+s.Resources.Stone+"  ·  Comida "+s.Resources.Food);
            resourceAmounts[0].Put(CompactAmount(s.Resources.Wood));
            resourceAmounts[1].Put(CompactAmount(s.Resources.Stone));
            resourceAmounts[2].Put(CompactAmount(s.Resources.Food));
            if(entryLabel!=null)entryLabel.Put("CORREO"+(mail?.Unread()>0?" ("+mail.Unread()+")":""));
            if(!IsOpen)return;
            // Layout can change responsive page size (5 portrait / 2 compact
            // landscape) and snap the offset. Apply that BEFORE fetching the
            // authoritative visible page; otherwise row widgets briefly show
            // a stale page after an orientation or scale transition.
            Layout();
            foreach(var t in tabLabels)
            {
                string key=t.transform.parent.name.Replace("Buzón ","");
                t.color=key==category?Gold:Ivory;
                var surface=t.transform.parent.GetComponentInChildren<Image>();
                if(surface!=null){surface.sprite=RoyalGoldV41SourceArt.Get(key==category?"tab-active":"tab-idle",18);surface.color=Color.white;}
                int unread=mail?.Unread(key)??0,index=Array.IndexOf(categories,key);
                tabBadges[index].transform.parent.gameObject.SetActive(unread>0);
                tabBadges[index].Put(unread.ToString());
                // Four real categories; compact mobile uses an unambiguous
                // short caption rather than breaking Recompensas into fragments.
                t.Put(window.rect.width<520f && key=="recompensas"?"Premios":
                    char.ToUpperInvariant(key[0])+key.Substring(1));
            }
            var list=mail?.List(category,offset,currentPageSize)??new List<EldoriaV41Message>();
            int total=mail?.Messages?.Count(m=>m.Category==category)??0;
            listSummary.Put(total==0?"SIN CORREO EN ESTA CATEGORÍA":
                total==1?"1 MENSAJE REAL · "+char.ToUpperInvariant(category[0])+category.Substring(1):
                total+" MENSAJES REALES · "+char.ToUpperInvariant(category[0])+category.Substring(1));
            emptyState.Put("No hay mensajes en "+category+".");
            for(int i=0;i<5;i++)
            {
                bool shown=i<list.Count && selectedId==null;
                messageButtons[i].gameObject.SetActive(shown);
                if(shown)
                {
                    var msg=list[i];
                    messageLabels[i].color=msg.Read?
                        new Color(.26f,.22f,.18f):new Color(.11f,.08f,.045f);
                    messageLabels[i].Put(msg.Battle!=null?TargetName(msg.Battle.TargetId):msg.Title);
                    messageUnreadDots[i].gameObject.SetActive(!msg.Read);
                    messageSurfaces[i].color=msg.Read?
                        new Color(.84f,.84f,.83f,.92f):Color.white;
                    // Ink belongs on the pale parchment; the inherited ivory
                    // made both subtitles and timestamps nearly invisible.
                    messageSubtitles[i].color=msg.Read?
                        new Color(.38f,.33f,.29f):new Color(.28f,.22f,.16f);
                    messageDates[i].color=new Color(.38f,.31f,.24f);
                    string preview=msg.Battle==null?msg.Subtitle:
                        (msg.Battle.EffectiveOutcome=="draw"?"Empate ante ":msg.Battle.EffectiveOutcome=="victory"?"Victoria contra ":"Derrota ante ")+TargetName(msg.Battle.TargetId);
                    // Let the measured two-line row wrap; a fixed 22-character
                    // cut hid the encounter/stage on every narrow phone.
                    messageSubtitles[i].Put(preview);
                    messageDates[i].Put(msg.OccurredAtUtcTicks>0?new DateTime(msg.OccurredAtUtcTicks,DateTimeKind.Utc).ToString("dd/MM HH:mm"):"");
                    string icon=msg.Category=="recompensas"?"mail-reward":msg.Category=="narrativa"?"mail-narrative":
                        msg.Category=="sistema"?"mail-system":msg.Type=="battle"?"mail-battle":"mail-march";
                    messageIcons[i].sprite=RoyalGoldV41SourceArt.Get(icon);
                }
            }
            navLabel.Put(total==0?"No hay mensajes":
                total<=currentPageSize?"Mostrando "+list.Count+" de "+total+" mensajes":
                "Mensajes "+(offset+1)+"–"+(offset+list.Count)+" de "+total);
            var selected=mail?.Messages?.Find(m=>m.EventId==selectedId);
            detailViewport.gameObject.SetActive(selected!=null);
            foreach(var button in window.GetComponentsInChildren<Button>(true))
                if(button.name=="Leer"||button.name=="Eliminar")button.gameObject.SetActive(selected!=null);
            if(selected==null)
            {
                detail.Put("");
                Layout();
                return;
            }
            PresentReport(selected);
            navLabel.Put("DESLIZA PARA LEER EL INFORME");
            if(!detailVisible)
                detailScroll.verticalNormalizedPosition=1f;
            Layout();
        }
    }

    /// <summary>
    /// Non-interactive illustrated environment. Drawn as lightweight uGUI
    /// geometry so the sparse mailbox never falls back to a black void and
    /// no full reference screenshot or baked message text is displayed.
    /// </summary>
    [AddComponentMenu("Eldoria/UI/Mailbox V4.1 matte")]
    public sealed class RoyalGoldV41MailboxBackdrop : MaskableGraphic
    {
        static readonly Color Bronze=new Color(.55f,.37f,.18f,.63f);
        void Fill(VertexHelper vh,Rect r,Color bottom,Color top)
        {
            if(r.width<=0f||r.height<=0f)return;
            int index=vh.currentVertCount;
            vh.AddVert(new Vector3(r.xMin,r.yMin),bottom,Vector2.zero);
            vh.AddVert(new Vector3(r.xMin,r.yMax),top,Vector2.up);
            vh.AddVert(new Vector3(r.xMax,r.yMax),top,Vector2.one);
            vh.AddVert(new Vector3(r.xMax,r.yMin),bottom,Vector2.right);
            vh.AddTriangle(index,index+1,index+2);
            vh.AddTriangle(index,index+2,index+3);
        }
        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();
            var r=rectTransform.rect;
            if(r.width<10f||r.height<10f)return;
            Fill(vh,r,new Color(.32f,.255f,.19f,1f),
                new Color(.105f,.133f,.149f,1f));
            // Layered shadows and faint brass dividers form a single
            // coherent parchment-reading environment on every aspect.
            float edge=Mathf.Clamp(r.width*.012f,4f,16f);
            Fill(vh,new Rect(r.xMin+edge,r.yMin+edge,
                r.width-edge*2f,2f),Bronze,Bronze);
            Fill(vh,new Rect(r.xMin+edge,r.yMax-edge-2f,
                r.width-edge*2f,2f),Bronze,Bronze);
            var subtle=new Color(.81f,.67f,.47f,.16f);
            for(int i=0;i<4;i++)
            {
                float y=r.yMin+r.height*(.19f+i*.20f);
                Fill(vh,new Rect(r.xMin+edge*1.8f,y,
                    r.width-edge*3.6f,1.25f),subtle,subtle);
            }
            var corner=new Color(.87f,.69f,.4f,.37f);
            float size=Mathf.Min(22f,r.width*.035f);
            Fill(vh,new Rect(r.xMin+edge,r.yMax-edge-size,2f,size),
                corner,corner);
            Fill(vh,new Rect(r.xMax-edge-2f,r.yMax-edge-size,2f,size),
                corner,corner);
            Fill(vh,new Rect(r.xMin+edge,r.yMin+edge,2f,size),
                corner,corner);
            Fill(vh,new Rect(r.xMax-edge-2f,r.yMin+edge,2f,size),
                corner,corner);
        }
    }
}

