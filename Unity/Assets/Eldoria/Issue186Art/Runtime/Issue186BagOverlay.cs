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
            public Text Quantity, Name, Amount, Detail, UseText;
            public Button Seen, Use;
            public ResourceUiPacketRow PresentedPacket;
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
        bool sharedTheme;
        float priorWidth,priorHeight,nextRefresh,nextGuide;
        public bool IsOpen=>panel&&panel.gameObject.activeSelf;
        public int PendingCount=>view?.Bag?.Count??0;

        // Public presentation protocol: PR173 can host the existing saved bag
        // on its actual canvas without referencing this optional assembly type.
        [UnityEngine.Scripting.Preserve]
        public int NewPacketCount(string kind)
        {
            if(view?.Bag==null)return 0;
            int total=0;
            foreach(var item in view.Bag)
                if(item.New&&(string.IsNullOrEmpty(kind)||item.Kind==kind))total++;
            return total;
        }
        [UnityEngine.Scripting.Preserve]
        public int ClaimableMailCount()
        {
            if(view?.Mail==null)return 0;
            int total=0;
            foreach(var item in view.Mail)if(item.CanClaim)total++;
            return total;
        }
        [UnityEngine.Scripting.Preserve]
        public bool OpenOnCanvas(Canvas target)
        {
            if(!target||!target.isActiveAndEnabled||!panel||gateway==null||
                target==hud.GetComponent<Canvas>()||!Unobstructed(true))return false;
            presentationCanvas=target;
            panel.SetParent(target.transform,false);
            panel.localScale=Vector3.one;
            panel.anchorMin=Vector2.zero;panel.anchorMax=Vector2.one;
            panel.offsetMin=panel.offsetMax=Vector2.zero;
            Open();
            return IsOpen;
        }
        [UnityEngine.Scripting.Preserve]
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
            bool heading=name=="Issue186 bag title"||name=="Issue186 saved kind";
            var sharedFont=Resources.Load<Font>(heading?
                "VigiliaArt/Fonts/CormorantGaramond-SemiBold":
                "VigiliaArt/Fonts/LibreBaskerville-Regular");
            label.font=sharedFont?sharedFont:RoyalGoldV41SourceArt.Serif;
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
            var authored=caption=="USAR"?Resources.Load<Sprite>(
                "VigiliaArt/PremiumSurfaces/burgundy_action"):null;
            Graphic frame;
            if(authored)
            {
                var skin=rect.gameObject.AddComponent<Image>();
                skin.sprite=authored;skin.type=Image.Type.Sliced;
                skin.color=Color.white;skin.raycastTarget=true;frame=skin;
            }
            else
            {
                var fallback=rect.gameObject.AddComponent<RoyalGoldV2FrameGraphic>();
                fallback.AntiqueGoldAction=true;fallback.DecorativeCorners=true;
                fallback.color=new Color(.96f,.78f,.67f,1f);
                fallback.raycastTarget=true;frame=fallback;
            }
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
            var sharedStone=Resources.Load<Sprite>("VigiliaArt/PremiumSurfaces/panel_fill");
            sharedTheme=sharedStone;
            var paper=Art(panel,sharedTheme?Color.white:Slate,true);
            if(sharedTheme){paper.sprite=sharedStone;paper.type=Image.Type.Simple;paper.preserveAspect=false;}
            header=Rect("Issue186 burgundy title panel",panel);
            Art(header,sharedTheme?Color.clear:Wine);
            rim=Rect("Issue186 antique outer trim",panel);
            rim.gameObject.AddComponent<CanvasRenderer>();
            var frame=rim.gameObject.AddComponent<RoyalGoldV2FrameGraphic>();
            frame.color=new Color(.69f,.45f,.32f,.55f);
            frame.raycastTarget=false;
            title=TextAt("Issue186 bag title",panel,27,Gold);title.text="BOLSA DE RECURSOS";
            count=TextAt("Issue186 available count",panel,17,Ivory);
            feedback=TextAt("Issue186 real command feedback",panel,14,Gold);
            back=Action("Issue186 bag close",panel,"VOLVER",Close,out _);
            for(int i=0;i<12;i++)
            {
                int index=i;
                var r=new Row();
                r.Root=Rect("Issue186 saved packet row "+i,panel);
                Art(r.Root,sharedTheme?new Color(.18f,.085f,.076f,.16f):
                    new Color(.105f,.066f,.070f,.98f));
                r.Frame=Rect("Issue186 aged metal",r.Root);
                if(sharedTheme)
                    Art(r.Frame,new Color(.52f,.38f,.22f,.40f));
                else
                {
                    r.Frame.gameObject.AddComponent<CanvasRenderer>();
                    var outline=r.Frame.gameObject.AddComponent<RoyalGoldV2FrameGraphic>();
                    outline.color=new Color(.62f,.41f,.33f,.68f);
                    outline.raycastTarget=false;
                }
                var seenRect=Rect("Issue186 card - mark seen only",r.Root);
                var hit=Art(seenRect,new Color(1f,1f,1f,0f),true);
                // A transparent input surface still needs real uGUI depth;
                // keep its mesh registered while its art remains invisible.
                hit.canvasRenderer.cullTransparentMesh=false;
                r.Seen=seenRect.gameObject.AddComponent<Button>();
                r.Seen.targetGraphic=hit;
                r.Seen.onClick.AddListener(()=>MarkSeen(index));
                r.Icon=Art(Rect("Issue186 real packet PNG",seenRect),Color.white);
                r.Quantity=TextAt("Issue186 quantity IN painted plaque",seenRect,
                    12,Gold,TextAnchor.MiddleCenter);
                r.Name=TextAt("Issue186 saved kind",r.Root,20,Ivory);
                r.Amount=TextAt("Issue186 readable saved amount",r.Root,22,Gold);
                var amountFace=Resources.Load<Font>("VigiliaArt/Fonts/LibreBaskerville-Bold");
                if(amountFace)r.Amount.font=amountFace;
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
            hand.rectTransform.sizeDelta=new Vector2(92f,92f);
            halo.rectTransform.sizeDelta=new Vector2(54f,54f);
            hand.raycastTarget=halo.raycastTarget=false;
            hand.gameObject.SetActive(false);halo.gameObject.SetActive(false);
            panel.gameObject.SetActive(false);entry.gameObject.SetActive(false);
        }
        bool Unobstructed(bool externalRequest=false)
        {
            // ChronicleLayout belongs to the suppressed legacy navigation.
            // The optional host owns a separate, active canvas. Retain every
            // real modal, narrative and destination gate on that presentation.
            bool external=externalRequest||(presentationCanvas&&
                presentationCanvas.isActiveAndEnabled&&hud&&
                presentationCanvas!=hud.GetComponent<Canvas>());
            return hud && !hud.NarrativeOpen &&
                !(hud.Navigation&&hud.Navigation.ChronicleLayout&&!external) &&
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
            var activeCanvas=panel.GetComponentInParent<Canvas>();
            float scale=activeCanvas?activeCanvas.scaleFactor:1f;
            var renderCamera=activeCanvas?activeCanvas.worldCamera:null;
            bool offscreen=renderCamera&&renderCamera.targetTexture;
            float viewW=offscreen?renderCamera.targetTexture.width:Screen.width;
            float viewH=offscreen?renderCamera.targetTexture.height:Screen.height;
            var safe=offscreen?new UnityEngine.Rect(0f,0f,viewW,viewH):Screen.safeArea;
            float l=Mathf.Max(12f,w*.026f+safe.xMin/Mathf.Max(.01f,scale));
            float r=Mathf.Max(12f,w*.026f+
                (viewW-safe.xMax)/Mathf.Max(.01f,scale));
            float t=Mathf.Max(12f,h*.018f+
                (viewH-safe.yMax)/Mathf.Max(.01f,scale));
            float b=Mathf.Max(12f,h*.018f+safe.yMin/Mathf.Max(.01f,scale));
            bool shortWide=w>h&&h*scale<600f;
            float entryW=Mathf.Clamp(w*.20f,90f,150f);
            // Mail owns the rightmost shortcut. Bag is directly adjacent.
            At(entry,w-r-entryW*2f-8f,h-b-(w>h?112f:150f)-52f,
                entryW,44f);
            At(entryBadge.rectTransform,entryW-24f,-7f,26f,26f);
            At(header,0f,0f,w,Mathf.Clamp(h*.105f,64f,92f));
            At(rim,7f,7f,w-14f,h-14f);
            // One saved object owns one compact square cell. Original item
            // art distinguishes resource families; no inflated full-width rows.
            float safeWidth=w-l-r;
            float width=Mathf.Min(w>h?1120f:800f,safeWidth);
            float x0=l+(safeWidth-width)*.5f;
            float backW=Mathf.Clamp(width*.21f,96f,128f);
            At((RectTransform)back.transform,x0,t,backW,46f);
            At(title.rectTransform,x0+backW+10f,t,width-backW-10f,46f);
            title.fontSize=shortWide?23:w<500f?21:29;
            float y0=t+Mathf.Clamp(h*.105f,64f,92f);
            At(count.rectTransform,x0,y0,width,30f);
            count.fontSize=shortWide?14:17;
            float cardsTop=y0+37f,footer=h-b-58f;
            int columns=w>h?(w>=1150f?6:4):3;
            float gap=shortWide?7f:10f;
            float cell=Mathf.Min((width-gap*(columns-1))/columns,
                Mathf.Max(80f,footer-52f-cardsTop));
            int lines=Mathf.Clamp(Mathf.FloorToInt(
                (footer-52f-cardsTop+gap)/(cell+gap)),1,12/columns);
            perPage=columns*lines;
            float gridW=columns*cell+(columns-1)*gap;
            float gridLeft=x0+(width-gridW)*.5f;
            for(int i=0;i<rows.Count;i++)
            {
                var item=rows[i];
                if(i>=perPage){item.Root.gameObject.SetActive(false);continue;}
                int column=i%columns,line=i/columns;
                At(item.Root,gridLeft+column*(cell+gap),
                    cardsTop+line*(cell+gap),cell,cell);
                At(item.Frame,2f,2f,cell-4f,cell-4f);
                At((RectTransform)item.Seen.transform,0f,0f,cell,cell);
                float icon=cell*.53f,iconY=cell*.13f;
                At(item.Icon.rectTransform,(cell-icon)*.5f,iconY,icon,icon);
                At(item.Quantity.rectTransform,(cell-icon)*.5f+icon*.15f,
                    iconY+icon*.73f,icon*.70f,icon*.18f);
                At(item.New.rectTransform,cell-25f,-5f,27f,27f);
                At(item.Amount.rectTransform,6f,2f,cell-12f,Mathf.Max(26f,cell*.19f));
                item.Amount.fontSize=Mathf.RoundToInt(Mathf.Clamp(cell*.13f,13f,24f));
                item.Amount.alignment=TextAnchor.MiddleLeft;
                At(item.Name.rectTransform,5f,cell*.63f,cell-10f,cell*.15f);
                item.Name.fontSize=Mathf.RoundToInt(Mathf.Clamp(cell*.09f,10f,17f));
                item.Name.alignment=TextAnchor.MiddleCenter;
                // Novelty is already represented by the original badge.
                // Keep source state available without duplicating a text line.
                item.Detail.gameObject.SetActive(false);
                float useH=Mathf.Max(22f,cell*.20f);
                At((RectTransform)item.Use.transform,6f,cell-useH-5f,cell-12f,useH);
                item.UseText.fontSize=Mathf.RoundToInt(Mathf.Clamp(cell*.10f,11f,17f));
                var skin=item.Use.targetGraphic as Image;
                if(skin&&skin.sprite)
                    skin.pixelsPerUnitMultiplier=Mathf.Max(.01f,
                        skin.sprite.rect.height/(useH*Mathf.Max(.01f,skin.pixelsPerUnit)));
            }
            At(feedback.rectTransform,x0,footer-47f,width,43f);
            float navW=Mathf.Clamp(width*.25f,92f,160f);
            At((RectTransform)prev.transform,x0,footer,navW,48f);
            At((RectTransform)next.transform,x0+width-navW,footer,navW,48f);
            float guideW=Mathf.Max(1f,width-2f*navW-12f);
            At((RectTransform)skipGuide.transform,x0+navW+6f,footer,
                guideW,48f);
        }
        public void Open()
        {
            if(!panel||gateway==null||!Unobstructed())return;
            closeMail?.Invoke();
            panel.gameObject.SetActive(true);panel.SetAsLastSibling();
            page=0;
            feedback.text="Solo USAR añade recursos. Toca una casilla para marcarla vista.";
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
            var packet=PresentedPacket(index);
            if(packet==null||!packet.New)return;
            var result=bridge.MarkBagSeen(packet.ReceiptId);
            feedback.text=result.Ok?"Visto: el paquete permanece en la bolsa.":result.Message;
            Refresh();
        }
        void Use(int index)
        {
            var packet=PresentedPacket(index);
            if(packet==null||!packet.CanUse)return;
            var result=bridge.UsePacket(packet.ReceiptId);
            feedback.text=result.Ok?"Recursos añadidos a tu reino.":result.Message;
            Refresh();
        }
        ResourceUiPacketRow PresentedPacket(int index)
        {
            // The action belongs to the item actually painted in this cell,
            // not to a newly computed page offset during orientation changes.
            if(index<0||index>=rows.Count||!rows[index].Root.gameObject.activeInHierarchy)
                return null;
            return rows[index].PresentedPacket;
        }
        [UnityEngine.Scripting.Preserve]
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
                slot.PresentedPacket=packet;
                slot.Root.gameObject.SetActive(packet!=null);
                if(packet==null)continue;
                Issue186ArtKit.BindPacket(slot.Icon,slot.Quantity,packet.Kind,packet.Amount);
                slot.Quantity.fontSize=12;
                while(slot.Quantity.preferredWidth>slot.Quantity.rectTransform.rect.width-3f
                    &&slot.Quantity.fontSize>8)slot.Quantity.fontSize--;
                slot.Name.text=packet.Kind=="wood"?"MADERA":
                    packet.Kind=="stone"?"PIEDRA":"COMIDA";
                slot.Amount.text=packet.AmountText;
                slot.Detail.text=packet.New?"NUEVO":"Guardado";
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
                Mathf.Abs(panel.rect.height-priorHeight)>.5f))Refresh();
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

