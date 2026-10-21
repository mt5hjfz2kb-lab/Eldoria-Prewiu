using System;
using Eldoria.Application;
using Eldoria.Domain;
using Eldoria.UI.RoyalGold;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

namespace Eldoria.UI.RoyalGold.V2
{
    /// <summary>
    /// V3 management destinations on the existing Royal HUD canvas. Owns no save,
    /// clock, camera, gameplay scene or resource balance. All purchases use LocalGateway.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class RoyalGoldV3ManagementScreens : MonoBehaviour
    {
        public enum Destination { Training, March }
        public RoyalHUD Hud { get; private set; }
        public bool IsOpen => root != null && root.gameObject.activeSelf;
        public Destination CurrentDestination { get; private set; }
        ICommandGateway gateway;
        Func<long> utcTicks;
        Action onCommand;
        // Optional host navigation; same scene, preserving the city camera state.
        public Action ReturnToCity { get; set; }
        RectTransform root, header, left, right, footer, quantityRow;
        RoyalGoldV3ManagementBackdropGraphic destinationArtwork;
        RoyalText heading, subheading, unitTitle, unitDetail, selectedText, summary, status, feedback;
        RoyalButton minus, plus, action, back, switchDestination;
        RoyalText actionLabel, switchLabel;
        Slider slider;
        int selectedCount = 10;
        int maxCount;
        float nextRefresh, lastWidth = -1, lastHeight = -1, submitAfter;
        string notice = "";
        string observedTask = "";
        int observedCount;
        static readonly Color Ivory = new Color(.95f,.925f,.865f);
        static readonly Color Gold = new Color(.89f,.73f,.46f);
        static readonly Color Muted = new Color(.78f,.82f,.84f);

        public void Initialize(RoyalHUD hud, ICommandGateway authoritativeGateway,
            Func<long> authoritativeUtcTicks, Action refreshed)
        {
            if(Hud != hud && Hud != null) Hud.SelectionChanged -= OnSelection;
            if(Hud != hud && hud != null) hud.SelectionChanged += OnSelection;
            Hud = hud;
            gateway = authoritativeGateway;
            utcTicks = authoritativeUtcTicks;
            onCommand = refreshed;
        }

        void OnSelection(string id)
        {
            if(gateway == null) return;
            // Canonical navigation clears selection before switching world/city.
            // Never leave a full-screen management destination over the world.
            if(IsOpen)
            {
                if(string.IsNullOrEmpty(id))
                {
                    root.gameObject.SetActive(false);
                    notice="";
                    return;
                }
                if(id=="march-main" && CurrentDestination!=Destination.March)
                    Open(Destination.March);
                else if(id=="barracks" && CurrentDestination!=Destination.Training)
                    Open(Destination.Training);
                return;
            }
            var s = gateway.Snapshot();
            if(s.BastionLevel < 2 || s.BarracksLevel < 1) return;
            if(id == "barracks") Open(Destination.Training);
            else if(id == "march-main") Open(Destination.March);
        }
        void OnDestroy()
        {
            if(Hud != null) Hud.SelectionChanged -= OnSelection;
        }

        // Invoked from the canonical IRoyalSession, not by a duplicate HUD or modal.
        public bool TryOpen(RoyalAction offer)
        {
            if (gateway == null || offer == null) return false;
            var s = gateway.Snapshot();
            if (offer.Kind == "Recruit" && offer.Target == "archer:t1" &&
                s.BarracksLevel > 0 && s.BastionLevel >= 2)
            {
                Open(Destination.Training);
                return true;
            }
            if (offer.Kind == "ConfigureMarch" && offer.Target == "march-main" &&
                s.BarracksLevel > 0 && s.BastionLevel >= 2)
            {
                Open(Destination.March);
                return true;
            }
            return false;
        }

        public void Open(Destination destination)
        {
            if (gateway == null || Hud == null) return;
            var s = gateway.Snapshot();
            if (s.BastionLevel < 2 || s.BarracksLevel < 1) return;
            EnsureView();
            CurrentDestination = destination;
            destinationArtwork.MarchTheme = destination == Destination.March;
            destinationArtwork.SetVerticesDirty();
            submitAfter = 0f;
            selectedCount = destination == Destination.Training
                ? Mathf.Min(SliceRules.RecruitArchers, Math.Max(1, SliceRules.MaxAffordableArchers(s)))
                : Mathf.Min(Math.Max(1, s.Available.ArcherT1),
                    s.MarchConfigured ? Mathf.Max(1, s.PreparedTroops.ArcherT1) : Math.Max(1, s.Available.ArcherT1));
            notice = "";
            root.gameObject.SetActive(true);
            root.SetAsLastSibling();
            lastWidth = -1;
            Reflow();
            Refresh();
        }

        public void Close()
        {
            if (root != null) root.gameObject.SetActive(false);
            notice = "";
            // World -> city is a canonical navigation request, never a scene reload.
            ReturnToCity?.Invoke();
            if (Hud != null) Hud.SelectBuilding("");
        }

        // Called by the native acceptance capture after switching the existing
        // canvas between real landscape/portrait render targets in one frame.
        // No scene/camera/gameplay mutation; runtime Update still handles resizing.
        public void SynchronizeLayout()
        {
            if(!IsOpen) return;
            Canvas.ForceUpdateCanvases();
            Reflow();
            Refresh();
        }

        void Update()
        {
            if (!IsOpen || gateway == null) return;
            root.SetAsLastSibling(); // Ensure the city and every other HUD layer stay behind.
            if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame) { Close(); return; }
            if (Mathf.Abs(lastWidth-root.rect.width) > .5f ||
                Mathf.Abs(lastHeight-root.rect.height) > .5f) Reflow();
            if (Time.unscaledTime >= nextRefresh)
            {
                nextRefresh = Time.unscaledTime + .22f;
                Refresh();
            }
        }

        void EnsureView()
        {
            if (root != null) return;
            var go = new GameObject("V3 / Management destination", typeof(RectTransform),
                typeof(CanvasRenderer), typeof(Image));
            root = (RectTransform)go.transform;
            root.SetParent(Hud.transform, false);
            root.anchorMin = Vector2.zero; root.anchorMax = Vector2.one;
            root.offsetMin = root.offsetMax = Vector2.zero;
            // Opaque, raycast-blocking royal midnight: the city is NOT visible through it.
            var bg = go.GetComponent<Image>();
            bg.color = new Color(.026f,.045f,.075f,1f);
            bg.raycastTarget = true;
            var artwork=Rect("V3 / Royal military hall artwork",root);
            artwork.anchorMin=Vector2.zero;
            artwork.anchorMax=Vector2.one;
            artwork.offsetMin=artwork.offsetMax=Vector2.zero;
            artwork.gameObject.AddComponent<CanvasRenderer>();
            var backdrop=artwork.gameObject.AddComponent<RoyalGoldV3ManagementBackdrop>();
            backdrop.color=Color.white;
            backdrop.raycastTarget=false;
            var themeArt=Rect("V3 / Training bow and marching compass",root);
            themeArt.anchorMin=Vector2.zero;
            themeArt.anchorMax=Vector2.one;
            themeArt.offsetMin=themeArt.offsetMax=Vector2.zero;
            themeArt.gameObject.AddComponent<CanvasRenderer>();
            destinationArtwork=themeArt.gameObject.AddComponent<RoyalGoldV3ManagementBackdropGraphic>();
            destinationArtwork.color=Color.white;
            destinationArtwork.raycastTarget=false;
            header = Frame("V3 / Destination header",root,false);
            left = Frame("V3 / Featured military unit",root,false);
            right = Frame("V3 / Authoritative status and cost",root,false);
            footer = Frame("V3 / Destination actions",root,false);
            heading = Text("Destination title",header,27,Gold,true);
            subheading = Text("Royal gold description",header,15,Muted);
            unitTitle = Text("Unit or hero title",left,26,Ivory,true);
            unitDetail = Text("Unit profile and composition",left,18,Muted);
            quantityRow = Rect("Quantity controls",left);
            minus = Button("Reduce quantity",quantityRow,"−",()=>SetQuantity(selectedCount-1),false,out _);
            plus = Button("Increase quantity",quantityRow,"+",()=>SetQuantity(selectedCount+1),false,out _);
            selectedText = Text("Authoritative quantity",quantityRow,26,Gold,true);
            selectedText.alignment = TextAnchor.MiddleCenter;
            var sliderGo = Rect("Quantity slider",left);
            sliderGo.gameObject.AddComponent<CanvasRenderer>();
            sliderGo.gameObject.AddComponent<Image>().color = new Color(.11f,.16f,.20f,1f);
            slider = sliderGo.gameObject.AddComponent<Slider>();
            slider.wholeNumbers = true;
            slider.minValue = 1f; slider.maxValue = 30f;
            var fillArea = Rect("Fill Area",sliderGo);
            fillArea.anchorMin = new Vector2(0,.28f);fillArea.anchorMax = new Vector2(1,.72f);
            fillArea.offsetMin = new Vector2(6,0);fillArea.offsetMax = new Vector2(-6,0);
            var fill = Rect("Fill",fillArea);
            fill.anchorMin = Vector2.zero;fill.anchorMax = Vector2.one;
            fill.offsetMin = fill.offsetMax = Vector2.zero;
            fill.gameObject.AddComponent<CanvasRenderer>();
            var fillImage=fill.gameObject.AddComponent<Image>();
            fillImage.color=Gold;slider.fillRect=fill;
            var handle = Rect("Handle",sliderGo);
            handle.anchorMin = new Vector2(.5f,0);handle.anchorMax = new Vector2(.5f,1);
            handle.sizeDelta = new Vector2(20,0);
            handle.gameObject.AddComponent<CanvasRenderer>();
            var handleImage = handle.gameObject.AddComponent<Image>();
            handleImage.color=Ivory;slider.handleRect=handle;
            slider.targetGraphic=handleImage;
            slider.onValueChanged.AddListener(v=>SetQuantity(Mathf.RoundToInt(v)));
            summary=Text("Training or expedition breakdown",right,20,Ivory);
            status=Text("Available and pending troops",right,16,Muted);
            feedback=Text("Command feedback",footer,15,Gold);
            back=Button("Return to Valoria",footer,"VOLVER A LA CIUDAD",Close,false,out _);
            action=Button("Confirm military command",footer,"CONFIRMAR",Confirm,true,out actionLabel);
            switchDestination=Button("Switch destination",header,"MARCHAS",Switch,true,out switchLabel);
            root.gameObject.SetActive(false);
        }

        void Switch()
        {
            Open(CurrentDestination == Destination.Training ? Destination.March : Destination.Training);
        }
        void SetQuantity(int requested)
        {
            int clamped = Mathf.Clamp(requested,1,Mathf.Max(1,maxCount));
            if (clamped == selectedCount) return;
            selectedCount = clamped;
            slider.SetValueWithoutNotify(selectedCount);
            Refresh();
        }
        void Refresh()
        {
            if (!IsOpen || gateway == null) return;
            var s=gateway.Snapshot();
            if(s.BastionLevel<2 || s.BarracksLevel<1){Close();return;}
            bool training=CurrentDestination==Destination.Training;
            bool trainingActive=training && s.RecruitmentCompletesUtcTicks>0;
            maxCount=training?Mathf.Min(500,SliceRules.MaxAffordableArchers(s)):
                s.Available.ArcherT1;
            // A pending operation has already spent its resources. Show the
            // real saved quantity rather than a newly affordable quantity.
            if(trainingActive)
            {
                selectedCount=Mathf.Clamp(s.PendingRecruitArchers,1,500);
                maxCount=Mathf.Max(maxCount,selectedCount);
            }
            else if(maxCount>0)selectedCount=Mathf.Clamp(selectedCount,1,maxCount);
            else selectedCount=1;
            slider.minValue=1;
            slider.maxValue=Mathf.Max(1,maxCount);
            slider.SetValueWithoutNotify(selectedCount);
            slider.interactable=maxCount>1 && (!training || s.RecruitmentCompletesUtcTicks==0);
            minus.interactable=selectedCount>1 && slider.interactable;
            plus.interactable=selectedCount<maxCount && slider.interactable;
            selectedText.Put(maxCount>0?selectedCount.ToString():"—");
            heading.Put(training?"CUARTEL · ENTRENAMIENTO":"SALA DE MARCHAS");
            subheading.Put(training?"VALORIA · RECLUTAMIENTO DE TROPAS":"VALORIA · PREPARACIÓN DE EXPEDICIONES");
            unitTitle.Put(training?"ARQUEROS · T1":"SIR ALDRIC");
            unitDetail.Put(training?"Infantería de proyectiles · Nivel I\nUnidad disponible: Arquero T1":
                "Héroe disponible · Mando de expedición\nUnidad seleccionable: Arqueros T1");
            switchLabel.Put(training?"PREPARAR MARCHA":"ENTRENAR TROPAS");
            switchDestination.interactable=true;
            bool busy=s.RecruitmentCompletesUtcTicks>0;
            if(training)
            {
                if(busy)
                {
                    string task=s.RecruitmentTaskId??"";
                    if(task!=observedTask){observedTask=task;observedCount=s.PendingRecruitArchers;}
                }
                else if(observedTask.Length>0)
                {
                    if(s.CompletedTaskIds.Contains(observedTask))notice="+"+observedCount+" arqueros incorporados a Valoria.";
                    observedTask="";
                }
                int wood=SliceRules.RecruitWoodCostFor(selectedCount);
                int stone=SliceRules.RecruitStoneCostFor(selectedCount);
                int seconds=SliceRules.RecruitSecondsFor(selectedCount);
                long leftSeconds=busy?Math.Max(0,(long)Math.Ceiling(
                    (s.RecruitmentCompletesUtcTicks-utcTicks())/(double)TimeSpan.TicksPerSecond)):0;
                summary.Put("COSTE DEL ENTRENAMIENTO\n\nMadera   "+wood+
                    "\nPiedra   "+stone+"\n\nTIEMPO TOTAL\n"+Clock(seconds));
                status.Put("DISPONIBLES  "+s.Available.ArcherT1+
                    "\nENTRENANDO  "+s.PendingRecruitArchers+
                    (busy?"\n\nCUENTA ATRÁS  "+Clock(leftSeconds):"\n\nSin entrenamiento activo"));
                actionLabel.Put(busy?"ENTRENANDO · "+Clock(leftSeconds):"ENTRENAR "+selectedCount+" ARQUEROS");
                action.interactable=!busy && maxCount>0 && Time.unscaledTime>=submitAfter;
            }
            else
            {
                var troops=s.Available.Copy();
                troops.ArcherT1=maxCount>0?selectedCount:0;
                var power=SliceRules.Expedition(troops,"aldric");
                summary.Put("COMPOSICIÓN DE LA MARCHA\n\nSir Aldric · Líder\n"+
                    "Arqueros T1   "+troops.ArcherT1+"\n\nPODER  "+power.Power+
                    "\nATAQUE  "+power.Attack+"   DEFENSA  "+power.Defense+
                    "\nVIDA  "+power.Health+"   RUPTURA  "+power.Break);
                status.Put("DISPONIBLES  "+s.Available.ArcherT1+
                    "\nYA PREPARADOS  "+(s.MarchConfigured?s.PreparedTroops.Total:0)+
                    "\nEN EXPEDICIÓN  "+(s.March.Phase=="idle"?0:s.March.Troops.Total));
                bool allowed=s.March.Phase=="idle" && maxCount>0;
                actionLabel.Put(allowed?"CONFIRMAR COMPOSICIÓN":"MARCHA NO DISPONIBLE");
                action.interactable=allowed && Time.unscaledTime>=submitAfter;
            }
            feedback.Put(notice);
        }
        void Confirm()
        {
            if(!IsOpen||Time.unscaledTime<submitAfter)return;
            var s=gateway.Snapshot();
            bool training=CurrentDestination==Destination.Training;
            if(training&&(s.RecruitmentCompletesUtcTicks>0 || selectedCount>SliceRules.MaxAffordableArchers(s)))return;
            if(!training&&(s.March.Phase!="idle" || selectedCount>s.Available.ArcherT1))return;
            string target=training?"archer:t1:"+selectedCount:"march-main:"+selectedCount;
            string kind=training?"Recruit":"ConfigureMarch";
            submitAfter=Time.unscaledTime+.45f;
            var result=gateway.Execute(new GameCommand(Guid.NewGuid().ToString("N"),
                s.PlayerId,kind,target,s.Revision));
            notice=result.Ok?(training?"Entrenamiento iniciado. Puedes regresar a la ciudad.":
                "Marcha preparada y guardada."):result.Message;
            onCommand?.Invoke();
            Hud.Refresh();
            Refresh();
        }
        static string Clock(long seconds)
        {
            long h=seconds/3600,m=(seconds/60)%60,s=seconds%60;
            return h>0?h+":"+m.ToString("00")+":"+s.ToString("00"):
                m.ToString("00")+":"+s.ToString("00");
        }

        void Reflow()
        {
            if(root==null)return;
            lastWidth=root.rect.width;lastHeight=root.rect.height;
            float w=lastWidth,h=lastHeight;
            if(w<1||h<1)return;
            bool landscape=w>h;
            float canvasScale=Hud.GetComponentInParent<Canvas>()?.scaleFactor??1f;
            Rect area=Screen.safeArea;
            float insetLeft=Screen.width>0?area.xMin/Mathf.Max(.01f,canvasScale):0;
            float insetRight=Screen.width>0?(Screen.width-area.xMax)/Mathf.Max(.01f,canvasScale):0;
            float side=Mathf.Max(18,w*.027f);
            float leftSide=Mathf.Max(side,insetLeft+12),rightSide=Mathf.Max(side,insetRight+12);
            float usable=w-leftSide-rightSide;
            float safeTop=Screen.height>0?(Screen.height-area.yMax)/Mathf.Max(.01f,canvasScale):0;
            float safeBottom=Screen.height>0?area.yMin/Mathf.Max(.01f,canvasScale):0;
            float top=Mathf.Max(Mathf.Max(10,h*.024f),safeTop+8);
            float bottom=Mathf.Max(Mathf.Max(10,h*.024f),safeBottom+8);
            float headerH=landscape?Mathf.Clamp(h*.205f,66,95):Mathf.Clamp(h*.115f,84,104);
            float footH=landscape?Mathf.Clamp(h*.21f,70,92):Mathf.Clamp(h*.155f,102,132);
            float bodyY=top+headerH+8;
            float bodyH=Mathf.Max(120,h-bodyY-footH-bottom-8);
            float gap=landscape?Mathf.Clamp(w*.018f,12,25):10;
            Place(header,leftSide,top,usable,headerH);
            if(landscape)
            {
                float lw=(usable-gap)*.49f;
                Place(left,leftSide,bodyY,lw,bodyH);
                Place(right,leftSide+lw+gap,bodyY,usable-lw-gap,bodyH);
            }
            else
            {
                float lh=(bodyH-gap)*.48f;
                Place(left,leftSide,bodyY,usable,lh);
                Place(right,leftSide,bodyY+lh+gap,usable,bodyH-lh-gap);
            }
            Place(footer,leftSide,h-bottom-footH,usable,footH);
            float hw=header.rect.width;
            Place(heading.rectTransform,landscape?14:15,7,hw*.67f,landscape?32:37);
            Place(subheading.rectTransform,landscape?14:15,landscape?39:47,hw*.70f,22);
            Place(switchDestination.transform as RectTransform,hw*.71f,landscape?9:16,hw*.27f,landscape?42:48);
            float uw=left.rect.width,uh=left.rect.height;
            Place(unitTitle.rectTransform,landscape?15:18,landscape?7:18,uw-35,landscape?32:40);
            Place(unitDetail.rectTransform,landscape?15:18,landscape?46:69,uw-35,landscape?42:60);
            float qy=landscape?Mathf.Max(90,uh-95):uh-129;
            Place(quantityRow,landscape?14:20,qy,uw-(landscape?28:40),landscape?39:48);
            float qw=quantityRow.rect.width;
            float ctrl=landscape?43:50;
            Place(minus.transform as RectTransform,0,0,ctrl,landscape?39:48);
            Place(plus.transform as RectTransform,qw-ctrl,0,ctrl,landscape?39:48);
            Place(selectedText.rectTransform,ctrl+8,0,qw-2*(ctrl+8),landscape?39:48);
            Place(slider.transform as RectTransform,landscape?19:25,uh-(landscape?21:45),uw-(landscape?38:50),landscape?14:19);
            float rw=right.rect.width,rh=right.rect.height;
            float statusH=landscape?Mathf.Min(62,rh*.28f):Mathf.Min(108,rh*.37f);
            Place(summary.rectTransform,landscape?14:19,landscape?10:18,rw-(landscape?28:38),
                rh-statusH-(landscape?18:22));
            Place(status.rectTransform,landscape?14:19,rh-statusH-4,rw-(landscape?28:38),statusH);
            float fw=footer.rect.width;
            Place(feedback.rectTransform,14,4,fw-28,landscape?20:25);
            float bw=Mathf.Max(90,(fw-42)*.48f);
            float by=landscape?25:footH-60;
            Place(back.transform as RectTransform,12,by,bw,landscape?42:49);
            Place(action.transform as RectTransform,fw-12-bw,by,bw,landscape?42:49);
            heading.fontSize=landscape?Mathf.RoundToInt(Mathf.Clamp(h*.062f,20,27)):23;
            subheading.fontSize=landscape?12:15;
            unitTitle.fontSize=landscape?20:25;
            unitDetail.fontSize=landscape?13:17;
            selectedText.fontSize=landscape?21:25;
            summary.fontSize=landscape?Mathf.RoundToInt(Mathf.Clamp(h*.04f,13,20)):17;
            status.fontSize=landscape?12:14;
            actionLabel.fontSize=landscape?14:16;
            switchLabel.fontSize=landscape?13:15;
            feedback.fontSize=landscape?12:14;
        }
        static RectTransform Rect(string name,Transform parent)
        {
            var g=new GameObject(name,typeof(RectTransform));
            g.transform.SetParent(parent,false);
            return (RectTransform)g.transform;
        }
        static RectTransform Frame(string name,RectTransform parent,bool primary)
        {
            var r=Rect(name,parent);
            r.gameObject.AddComponent<CanvasRenderer>();
            var graphic=r.gameObject.AddComponent<RoyalGoldV2FrameGraphic>();
            graphic.DecorativeCorners=true;
            graphic.Primary=primary;
            graphic.color=Color.white;
            graphic.raycastTarget=false;
            return r;
        }
        static RoyalText Text(string name,RectTransform parent,int size,Color color,bool bold=false)
        {
            var r=Rect(name,parent);
            r.gameObject.AddComponent<CanvasRenderer>();
            var t=r.gameObject.AddComponent<RoyalText>();
            t.font=Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            t.fontSize=size;t.fontStyle=bold?FontStyle.Bold:FontStyle.Normal;
            t.color=color;t.raycastTarget=false;
            t.alignment=TextAnchor.UpperLeft;
            t.horizontalOverflow=HorizontalWrapMode.Wrap;
            t.verticalOverflow=VerticalWrapMode.Overflow;
            return t;
        }
        static RoyalButton Button(string name,RectTransform parent,string caption,
            Action click,bool primary,out RoyalText text)
        {
            var r=Frame(name,parent,primary);
            var graphic=r.GetComponent<RoyalGoldV2FrameGraphic>();
            graphic.raycastTarget=true;
            var button=r.gameObject.AddComponent<RoyalButton>();
            button.targetGraphic=graphic;
            text=Text("Button label",r,17,Ivory,true);
            text.Put(caption);text.alignment=TextAnchor.MiddleCenter;
            text.rectTransform.anchorMin=Vector2.zero;text.rectTransform.anchorMax=Vector2.one;
            text.rectTransform.offsetMin=new Vector2(6,3);text.rectTransform.offsetMax=new Vector2(-6,-3);
            button.onClick.AddListener(()=>click());
            return button;
        }
        static void Place(RoyalText label,float x,float y,float w,float h)
        {
            Place(label.rectTransform,x,y,w,h);
        }
        static void Place(RectTransform r,float x,float y,float w,float h)
        {
            r.anchorMin=r.anchorMax=r.pivot=new Vector2(0,1);
            r.anchoredPosition=new Vector2(x,-y);
            r.sizeDelta=new Vector2(Mathf.Max(1,w),Mathf.Max(1,h));
        }
    }
}
