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
        RectTransform root, header, left, right, footer, quantityRow;
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
            if(gateway == null || IsOpen) return;
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
            if (Hud != null) Hud.SelectBuilding("");
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
            maxCount=training?Mathf.Min(500,SliceRules.MaxAffordableArchers(s)):
                s.Available.ArcherT1;
            if(maxCount>0)selectedCount=Mathf.Clamp(selectedCount,1,maxCount);
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
            bool landscape=w>h;
            float side=Mathf.Max(18,w*.035f), top=Mathf.Max(15,h*.035f);
            Place(header,side,top,w-2*side,landscape?84:92);
            float bodyY=top+(landscape?98:106);
            float footH=landscape?92:128;
            float bodyH=Mathf.Max(240,h-bodyY-footH-2*top);
            float gap=18;
            if(landscape)
            {
                float lw=(w-2*side-gap)*.50f;
                Place(left,side,bodyY,lw,bodyH);
                Place(right,side+lw+gap,bodyY,w-2*side-lw-gap,bodyH);
            }
            else
            {
                float lh=bodyH*.49f;
                Place(left,side,bodyY,w-2*side,lh);
                Place(right,side,bodyY+lh+gap,w-2*side,bodyH-lh-gap);
            }
            Place(footer,side,h-top-footH,w-2*side,footH);
            float hw=header.rect.width;
            Place(heading,20,12,hw*.68f,38);
            Place(subheading,20,51,hw*.70f,24);
            Place(switchDestination.transform as RectTransform,hw*.71f,18,hw*.27f,48);
            Place(unitTitle,20,24,left.rect.width-40,40);
            Place(unitDetail,20,72,left.rect.width-40,landscape?70:60);
            Place(quantityRow,22,left.rect.height-132,left.rect.width-44,53);
            float qw=quantityRow.rect.width;
            Place(minus.transform as RectTransform,0,0,54,52);
            Place(plus.transform as RectTransform,qw-54,0,54,52);
            Place(selectedText,65,0,qw-130,52);
            Place(slider.transform as RectTransform,28,left.rect.height-65,left.rect.width-56,20);
            Place(summary,24,24,right.rect.width-48,Mathf.Max(100,right.rect.height*.70f));
            Place(status,24,right.rect.height*.69f,right.rect.width-48,right.rect.height*.29f);
            Place(feedback,20,8,footer.rect.width-40,24);
            float bw=Mathf.Max(140,(footer.rect.width-56)*.46f);
            float by=landscape?37:49;
            Place(back.transform as RectTransform,20,by,bw,50);
            Place(action.transform as RectTransform,footer.rect.width-20-bw,by,bw,50);
            heading.fontSize=landscape?27:23;
            summary.fontSize=landscape?20:17;
            status.fontSize=landscape?16:14;
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
        static void Place(RectTransform r,float x,float y,float w,float h)
        {
            r.anchorMin=r.anchorMax=r.pivot=new Vector2(0,1);
            r.anchoredPosition=new Vector2(x,-y);
            r.sizeDelta=new Vector2(Mathf.Max(1,w),Mathf.Max(1,h));
        }
    }
}
