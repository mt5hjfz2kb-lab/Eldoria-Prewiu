using System;
using System.Collections.Generic;
using Eldoria.Application;
using Eldoria.Domain;
using Eldoria.Issue186Art;
using UnityEngine;
using UnityEngine.UI;

namespace Eldoria.UI.RoyalGold.V2
{
    // The one saved gateway is authoritative. This opt-in Royal Gold art
    // candidate neither creates a second Canvas nor alters resources itself.
    [DisallowMultipleComponent]
    public sealed class Issue186BagOverlay : MonoBehaviour
    {
        sealed class Row
        {
            public RectTransform Root, Frame;
            public Image Icon, New;
            public Text Quantity, Name, Detail, UseText;
            public Button Seen, Use;
        }
        static readonly Color Wine=new Color(.13f,.055f,.062f,.98f);
        static readonly Color Slate=new Color(.06f,.045f,.052f,1f);
        static readonly Color Gold=new Color(.89f,.72f,.42f,1f);
        static readonly Color Ivory=new Color(.96f,.90f,.81f,1f);
        readonly List<Row> rows=new List<Row>();
        RoyalHUD hud;
        ICommandGateway gateway;
        ResourceExperienceUiBridge bridge;
        Action closeMail;
        RectTransform entry, panel, header, rim;
        Text entryLabel, title, count, feedback;
        Image entryBadge, hand, halo;
        Button back, prev, next, skipGuide;
        Canvas presentationCanvas;
        ResourceUiSnapshot view;
        int page, perPage=4;
        float priorWidth,priorHeight,nextRefresh,nextGuide;
        public bool IsOpen=>panel&&panel.gameObject.activeSelf;
        public int PendingCount=>view?.Bag?.Count??0;

        // Public presentation protocol: PR173 can host the existing saved bag
        // on its actual canvas without referencing this optional assembly type.
        public int NewPacketCount(string kind)
        {
            if(view?.Bag==null)return 0;
            int total=0;
            foreach(var item in view.Bag)
                if(item.New&&(string.IsNullOrEmpty(kind)||item.Kind==kind))total++;
            return total;
        }
        public int ClaimableMailCount()
        {
            if(view?.Mail==null)return 0;
            int total=0;
            foreach(var item in view.Mail)if(item.CanClaim)total++;
            return total;
        }
        public bool OpenOnCanvas(Canvas target)
        {
            if(!target||!target.isActiveAndEnabled||!panel||gateway==null||
                !Unobstructed())return false;
            presentationCanvas=target;
            panel.SetParent(target.transform,false);
            panel.localScale=Vector3.one;
            panel.anchorMin=Vector2.zero;panel.anchorMax=Vector2.one;
            panel.offsetMin=panel.offsetMax=Vector2.zero;
            Open();
            return IsOpen;
        }
        public void RestorePresentation()
        {
            if(!panel)return;
            panel.gameObject.SetActive(false);
            if(hand)hand.gameObject.SetActive(false);
            if(halo)halo.gameObject.SetActive(false);
            presentationCanvas=null;
            if(hud)
            {
                panel.SetParent(hud.transform,false);
                panel.localScale=Vector3.one;
                panel.anchorMin=Vector2.zero;panel.anchorMax=Vector2.one;
                panel.offsetMin=panel.offsetMax=Vector2.zero;
            }
            Refresh();
        }
        void OnDestroy()
        {
            // The external host does not own this optional component's panel.
            if(panel&&panel.parent!=transform)Destroy(panel.gameObject);
        }
        static RectTransform Rect(string name,Transform parent)
        {
            var obj=new GameObject(name,typeof(RectTransform));
            obj.layer=5; // UI camera culling must include every owned child.
            obj.transform.SetParent(parent,false);
            return (RectTransform)obj.transform;
        }
        static Image Art(RectTransform rect,Color color,bool tap=false)
        {
            rect.gameObject.AddComponent<CanvasRenderer>();
            var img=rect.gameObject.AddComponent<Image>();
            img.color=color;img.raycastTarget=tap;
            return img;
        }
        static Text TextAt(string name,Transform parent,int size,Color color,
            TextAnchor alignment=TextAnchor.MiddleLeft)
        {
            var rect=Rect(name,parent);
            rect.gameObject.AddComponent<CanvasRenderer>();
            var label=rect.gameObject.AddComponent<Text>();
            label.font=RoyalGoldV41SourceArt.Serif;
            label.fontSize=size;label.color=color;label.alignment=alignment;
            label.horizontalOverflow=HorizontalWrapMode.Wrap;
            label.verticalOverflow=VerticalWrapMode.Truncate;
            label.raycastTarget=false;
            return label;
        }
        static Button Action(string name,Transform parent,string caption,
            Action clicked,out Text label)
        {
            var rect=Rect(name,parent);
            rect.gameObject.AddComponent<CanvasRenderer>();
            var frame=rect.gameObject.AddComponent<RoyalGoldV2FrameGraphic>();
            frame.AntiqueGoldAction=true;frame.DecorativeCorners=true;
            frame.color=new Color(.96f,.78f,.67f,1f);
            frame.raycastTarget=true;
            var button=rect.gameObject.AddComponent<Button>();
            button.targetGraphic=frame;
            label=TextAt(name+" caption",rect,16,Ivory,TextAnchor.MiddleCenter);
            label.text=caption;
            label.rectTransform.anchorMin=Vector2.zero;
            label.rectTransform.anchorMax=Vector2.one;
            label.rectTransform.offsetMin=label.rectTransform.offsetMax=Vector2.zero;
            button.onClick.AddListener(()=>clicked());
            return button;
        }
        static void At(RectTransform r,float x,float y,float w,float h)
        {
            r.anchorMin=r.anchorMax=new Vector2(0f,1f);
            r.pivot=new Vector2(0f,1f);
            r.anchoredPosition=new Vector2(x,-y);
            r.sizeDelta=new Vector2(Mathf.Max(1f,w),Mathf.Max(1f,h));
        }
        public void Initialize(RoyalHUD owner,ICommandGateway authority,Action closeMailbox)
        {
            if(!owner||authority==null)return;
            hud=owner;gateway=authority;closeMail=closeMailbox;
            bridge=new ResourceExperienceUiBridge(gateway);
            if(!panel)Build();
            Refresh();
        }
        void Build()
        {
            entry=(RectTransform)Action("Issue186 resource BAG entry",hud.transform,
                "BOLSA",Open,out entryLabel).transform;
            var newEntry=Rect("Issue186 entry novelty",entry);
            entryBadge=Art(newEntry,Color.white);
            panel=Rect("Issue186 saved resource bag",hud.transform);
            panel.anchorMin=Vector2.zero;panel.anchorMax=Vector2.one;
            panel.offsetMin=panel.offsetMax=Vector2.zero;
            Art(panel,Slate,true);
            header=Rect("Issue186 burgundy title panel",panel);
            Art(header,Wine);
            rim=Rect("Issue186 antique outer trim",panel);
            rim.gameObject.AddComponent<CanvasRenderer>();
            var frame=rim.gameObject.AddComponent<RoyalGoldV2FrameGraphic>();
            frame.color=new Color(.69f,.45f,.32f,.55f);
            frame.raycastTarget=false;
            title=TextAt("Issue186 bag title",panel,27,Gold);title.text="BOLSA DE RECURSOS";
            count=TextAt("Issue186 available count",panel,17,Ivory);
            feedback=TextAt("Issue186 real command feedback",panel,14,Gold);
            back=Action("Issue186 bag close",panel,"VOLVER",Close,out _);
            for(int i=0;i<4;i++)
            {
                int index=i;
                var r=new Row();
                r.Root=Rect("Issue186 saved packet row "+i,panel);
                Art(r.Root,new Color(.105f,.066f,.070f,.98f));
                r.Frame=Rect("Issue186 aged metal",r.Root);
                r.Frame.gameObject.AddComponent<CanvasRenderer>();
                var outline=r.Frame.gameObject.AddComponent<RoyalGoldV2FrameGraphic>();
                outline.color=new Color(.62f,.41f,.33f,.68f);
                outline.raycastTarget=false;
                var seenRect=Rect("Issue186 card - mark seen only",r.Root);
                var hit=Art(seenRect,new Color(1f,1f,1f,0f),true);
                r.Seen=seenRect.gameObject.AddComponent<Button>();
                r.Seen.targetGraphic=hit;
                r.Seen.onClick.AddListener(()=>MarkSeen(index));
                r.Icon=Art(Rect("Issue186 real packet PNG",seenRect),Color.white);
                r.Quantity=TextAt("Issue186 quantity IN painted plaque",seenRect,
                    12,Gold,TextAnchor.MiddleCenter);
                r.Name=TextAt("Issue186 saved kind",r.Root,20,Ivory);
                r.Detail=TextAt("Issue186 nonconsuming state",r.Root,13,Gold);
                r.New=Art(Rect("Issue186 authentic novelty badge",r.Root),Color.white);
                r.Use=Action("Issue186 saved packet USE "+i,r.Root,"USAR",
                    ()=>Use(index),out r.UseText);
                rows.Add(r);
            }
            prev=Action("Issue186 bag previous",panel,"ANTERIOR",
                ()=>Page(-1),out _);
            next=Action("Issue186 bag next",panel,"SIGUIENTE",
                ()=>Page(1),out _);
            skipGuide=Action("Issue186 dismiss first-use guidance",panel,
                "OMITIR AYUDA",DismissGuide,out _);
            hand=Art(Rect("Issue186 authentic tutorial hand",panel),Color.white);
            halo=Art(Rect("Issue186 authentic tutorial halo",panel),Color.white);
            hand.rectTransform.sizeDelta=new Vector2(72f,72f);
            halo.rectTransform.sizeDelta=new Vector2(54f,54f);
            hand.raycastTarget=halo.raycastTarget=false;
            hand.gameObject.SetActive(false);halo.gameObject.SetActive(false);
            panel.gameObject.SetActive(false);entry.gameObject.SetActive(false);
        }
        bool Unobstructed()
        {
            return hud && !hud.NarrativeOpen &&
                !(hud.Navigation&&hud.Navigation.ChronicleLayout) &&
                !(hud.Modal&&hud.Modal.gameObject.activeSelf) &&
                !(hud.GetComponent<RoyalGoldV3ManagementScreens>()?.IsOpen??false) &&
                !(hud.GetComponent<RoyalGoldV3EarlyRetentionScreens>()?.IsOpen??false) &&
                string.IsNullOrEmpty(hud.SelectedBuildingId);
        }
        void Layout()
        {
            if(!panel||!hud)return;
            float w=panel.rect.width,h=panel.rect.height;
            if(w<200f||h<200f)return;
            priorWidth=w;priorHeight=h;
            float scale=panel.GetComponentInParent<Canvas>()?.scaleFactor??1f;
            var safe=Screen.safeArea;
            float l=Mathf.Max(12f,w*.026f+safe.xMin/Mathf.Max(.01f,scale));
            float r=Mathf.Max(12f,w*.026f+
                (Screen.width-safe.xMax)/Mathf.Max(.01f,scale));
            float t=Mathf.Max(12f,h*.018f+
                (Screen.height-safe.yMax)/Mathf.Max(.01f,scale));
            float b=Mathf.Max(12f,h*.018f+safe.yMin/Mathf.Max(.01f,scale));
            bool shortWide=w>h&&h*scale<600f;
            float entryW=Mathf.Clamp(w*.20f,90f,150f);
            // Mail owns the rightmost shortcut. Bag is directly adjacent.
            At(entry,w-r-entryW*2f-8f,h-b-(w>h?112f:150f)-52f,
                entryW,44f);
            At(entryBadge.rectTransform,entryW-24f,-7f,26f,26f);
            At(header,0f,0f,w,Mathf.Clamp(h*.105f,64f,92f));
            At(rim,7f,7f,w-14f,h-14f);
            float backW=Mathf.Clamp(w*.16f,70f,118f);
            At((RectTransform)back.transform,l,t,backW,46f);
            At(title.rectTransform,l+backW+10f,t,
                w-l-r-backW-10f,46f);
            title.fontSize=shortWide?23:w<500f?21:29;
            float y0=t+Mathf.Clamp(h*.105f,64f,92f);
            At(count.rectTransform,l,y0,w-l-r,30f);
            count.fontSize=shortWide?14:17;
            float cardsTop=y0+37f,footer=h-b-58f;
            perPage=shortWide?2:4;
            float gap=shortWide?7f:11f;
            float rowH=Mathf.Max(52f,Mathf.Min(shortWide?122f:146f,
                (Mathf.Max(95f,footer-cardsTop)-gap*(perPage-1))/perPage));
            float width=w-l-r;
            float icon=Mathf.Min(96f,rowH-13f,Mathf.Max(48f,w*.21f));
            for(int i=0;i<rows.Count;i++)
            {
                var item=rows[i];
                if(i>=perPage){item.Root.gameObject.SetActive(false);continue;}
                At(item.Root,l,cardsTop+i*(rowH+gap),width,rowH);
                At(item.Frame,3f,3f,width-6f,rowH-6f);
                At((RectTransform)item.Seen.transform,10f,
                    (rowH-icon)*.5f,icon,icon);
                At(item.Icon.rectTransform,0f,0f,icon,icon);
                At(item.Quantity.rectTransform,icon*.15f,icon*.73f,
                    icon*.70f,icon*.18f);
                At(item.New.rectTransform,icon-2f,
                    Mathf.Max(0f,(rowH-icon)*.5f-8f),27f,27f);
                float useW=Mathf.Clamp(width*.245f,86f,122f);
                float useX=width-useW-12f;
                At((RectTransform)item.Use.transform,useX,
                    (rowH-46f)*.5f,useW,46f);
                float textX=icon+22f,textW=Mathf.Max(58f,useX-textX-9f);
                At(item.Name.rectTransform,textX,rowH*.17f,textW,32f);
                item.Name.fontSize=shortWide?16:width<400f?18:21;
                At(item.Detail.rectTransform,textX,rowH*.52f,textW,rowH*.36f);
                item.Detail.fontSize=shortWide?11:13;
                item.UseText.fontSize=shortWide?14:16;
            }
            At(feedback.rectTransform,l,footer-34f,w-l-r,31f);
            float navW=Mathf.Clamp(width*.25f,92f,160f);
            At((RectTransform)prev.transform,l,footer,navW,48f);
            At((RectTransform)next.transform,w-r-navW,footer,navW,48f);
            float guideW=Mathf.Max(1f,width-2f*navW-12f);
            At((RectTransform)skipGuide.transform,l+navW+6f,footer,
                guideW,48f);
        }
        public void Open()
        {
            if(!panel||gateway==null||!Unobstructed())return;
            closeMail?.Invoke();
            panel.gameObject.SetActive(true);panel.SetAsLastSibling();
            page=0;
            feedback.text="Toca una tarjeta para marcarla vista. Solo USAR añade recursos.";
            Layout();Refresh();
        }
        public void Close()
        {
            if(panel)panel.gameObject.SetActive(false);
            if(hand)hand.gameObject.SetActive(false);
            if(halo)halo.gameObject.SetActive(false);
            Refresh();
        }
        void DismissGuide()
        {
            if(bridge==null||view==null||!view.UseTutorialPending)return;
            var result=bridge.DismissTutorial(ResourceRewardCommands.FirstUseStep);
            feedback.text=result.Ok?"Ayuda omitida. Puedes usar los paquetes cuando quieras.":
                result.Message;
            Refresh();
        }
        void Page(int delta){page=Mathf.Max(0,page+delta);Refresh();}
        ResourceUiPacketRow Packet(int index)
        {
            if(view?.Bag==null||index<0||index>=perPage)return null;
            int id=page*perPage+index;
            return id<view.Bag.Count?view.Bag[id]:null;
        }
        void MarkSeen(int index)
        {
            var packet=Packet(index);
            if(packet==null||!packet.New)return;
            var result=bridge.MarkBagSeen(packet.ReceiptId);
            feedback.text=result.Ok?"Visto: el paquete permanece en la bolsa.":result.Message;
            Refresh();
        }
        void Use(int index)
        {
            var packet=Packet(index);
            if(packet==null||!packet.CanUse)return;
            var result=bridge.UsePacket(packet.ReceiptId);
            feedback.text=result.Ok?"Recursos añadidos a tu reino.":result.Message;
            Refresh();
        }
        public void Refresh()
        {
            if(!panel||bridge==null||!hud)return;
            view=bridge.Refresh();
            int available=view.Bag.Count,newCount=0;
            foreach(var item in view.Bag)if(item.New)newCount++;
            entry.gameObject.SetActive(!presentationCanvas&&available>0&&Unobstructed()&&!IsOpen);
            entryLabel.text=newCount>0?"BOLSA ("+newCount+")":"BOLSA";
            Issue186ArtKit.BindAttention(entryBadge,
                newCount>0?AttentionLevel.New:AttentionLevel.None);
            if(!IsOpen)return;
            Layout();
            page=Mathf.Clamp(page,0,Mathf.Max(0,(available-1)/perPage));
            count.text=available==0?"Sin paquetes por utilizar":
                available+" paquetes · "+newCount+" nuevos · uso voluntario";
            skipGuide.gameObject.SetActive(view.UseTutorialPending);
            prev.interactable=page>0;
            next.interactable=(page+1)*perPage<available;
            for(int i=0;i<rows.Count;i++)
            {
                var slot=rows[i];
                var packet=Packet(i);
                slot.Root.gameObject.SetActive(packet!=null);
                if(packet==null)continue;
                Issue186ArtKit.BindPacket(slot.Icon,slot.Quantity,packet.Kind,packet.Amount);
                slot.Quantity.fontSize=12;
                while(slot.Quantity.preferredWidth>slot.Quantity.rectTransform.rect.width-3f
                    &&slot.Quantity.fontSize>8)slot.Quantity.fontSize--;
                slot.Name.text=packet.Kind=="wood"?"MADERA":
                    packet.Kind=="stone"?"PIEDRA":"COMIDA";
                slot.Detail.text=packet.New?"NUEVO · toca para marcar visto":
                    "Guardado · "+packet.AmountText;
                slot.Use.interactable=packet.CanUse;
                slot.UseText.text=packet.CanUse?"USAR":"SIN ESPACIO";
                Issue186ArtKit.BindAttention(slot.New,
                    packet.New?AttentionLevel.New:AttentionLevel.None);
            }
        }
        void Update()
        {
            if(!panel||!hud||bridge==null)return;
            if(IsOpen&&(!Unobstructed()||
                (presentationCanvas&&!presentationCanvas.isActiveAndEnabled)))
                {Close();return;}
            if(IsOpen&&(Mathf.Abs(panel.rect.width-priorWidth)>.5f||
                Mathf.Abs(panel.rect.height-priorHeight)>.5f))Layout();
            if(Time.unscaledTime>=nextRefresh)
            {
                nextRefresh=Time.unscaledTime+.6f;Refresh();
            }
            if(IsOpen&&Time.unscaledTime>=nextGuide)
            {
                nextGuide=Time.unscaledTime+.16f;
                int target=-1;
                if(view!=null&&view.UseTutorialPending)
                    for(int i=0;i<perPage;i++)
                        if(Packet(i)?.CanUse==true){target=i;break;}
                var canvas=panel.GetComponentInParent<Canvas>();
                if(target<0||!canvas)
                {
                    hand.gameObject.SetActive(false);
                    halo.gameObject.SetActive(false);
                }
                else
                {
                    // The real EventSystem rejects any button covered by a modal.
                    // Only the authoritative Use command marks guidance complete.
                    Issue186ArtKit.BindFirstUsePointer(
                        gateway.Snapshot().ResourceGuidance,
                        ResourceRewardCommands.FirstUseStep,
                        ResourceRewardCommands.UsePacket,true,canvas,
                        (RectTransform)rows[target].Use.transform,hand,halo);
                }
            }
        }
    }
}

