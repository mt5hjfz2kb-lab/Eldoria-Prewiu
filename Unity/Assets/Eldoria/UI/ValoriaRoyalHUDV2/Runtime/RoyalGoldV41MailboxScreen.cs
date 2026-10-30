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
        public int UnreadCount=>gateway?.Snapshot()?.Mailbox?.Unread()??0;
        RoyalHUD hud;ICommandGateway gateway;
        RectTransform window,entry;
        RoyalText entryLabel,title,detail,navLabel;
        readonly List<RoyalText> tabLabels=new List<RoyalText>();
        readonly List<RoyalText> messageLabels=new List<RoyalText>();
        readonly List<Button> messageButtons=new List<Button>();
        string[] categories={"informes","recompensas","sistema","narrativa"};
        string category="informes",selectedId;
        int offset;
        float lastWidth,lastHeight,nextRefresh;
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
            Refresh();
        }
        void Update()
        {
            if(hud==null||gateway==null)return;
            if(IsOpen && hud.NarrativeOpen){Close();return;}
            if(IsOpen && (Mathf.Abs(window.rect.width-lastWidth)>.5f ||
                          Mathf.Abs(window.rect.height-lastHeight)>.5f))Layout();
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
            t.font=Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
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
        static void Position(RectTransform r,float x,float y,float w,float h)
        {
            r.anchorMin=r.anchorMax=new Vector2(0,1);
            r.pivot=new Vector2(0,1);r.anchoredPosition=new Vector2(x,-y);
            r.sizeDelta=new Vector2(Mathf.Max(0,w),Mathf.Max(0,h));
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
            title=Label("Buzón heading",w,29,true);title.Put("BUZÓN");
            MakeButton("Volver",w,"VOLVER",Close,out _);
            for(int i=0;i<4;i++)
            {
                string key=categories[i];
                var button=MakeButton("Buzón "+key,w,key.ToUpperInvariant(),()=>SetCategory(key),out var text);
                tabLabels.Add(text);
            }
            for(int i=0;i<6;i++)
            {
                int slot=i;
                var button=MakeButton("Buzón / mensaje "+i,w,"",()=>SelectRow(slot),out var label);
                messageButtons.Add(button);messageLabels.Add(label);
            }
            MakeButton("Página anterior",w,"<",()=>Page(-1),out _);
            MakeButton("Página siguiente",w,">",()=>Page(1),out _);
            navLabel=Label("Página",w,14);
            detail=Label("Contenido del mensaje",w,18);
            MakeButton("Leer",w,"MARCAR LEÍDO",()=>ExecuteMail("MailRead"),out _);
            MakeButton("Eliminar",w,"ELIMINAR",()=>ExecuteMail("MailDelete"),out _);
            w.gameObject.SetActive(false);
            Layout();
        }
        void Layout()
        {
            if(window==null)return;
            float w=window.rect.width,h=window.rect.height;
            if(w<=0||h<=0)return;
            lastWidth=w;lastHeight=h;
            bool wide=w>=h;float pad=Mathf.Max(14,w*.035f);
            // Safe area must be applied on the existing HUD; no additional Canvas.
            float canvasScale=hud.GetComponentInParent<Canvas>()?.scaleFactor??1f;
            Rect area=Screen.safeArea;
            float topInset=Screen.height>0?(Screen.height-area.yMax)/Mathf.Max(.01f,canvasScale):0;
            float bottomInset=Screen.height>0?area.yMin/Mathf.Max(.01f,canvasScale):0;
            float y=pad+topInset;
            Position(entry,pad,Mathf.Max(100,h*.16f),Mathf.Min(140,w*.22f),42);
            Position(title.rectTransform,pad,y,w*.55f,45);
            var children=window.GetComponentsInChildren<Button>(true);
            Button Find(string name)=>children.FirstOrDefault(b=>b.name==name);
            Position((RectTransform)Find("Volver").transform,w-pad-128,y,128,42);
            float tabsY=y+54,tabW=(w-pad*2-3*8)/4;
            for(int i=0;i<4;i++)
                Position((RectTransform)Find("Buzón "+categories[i]).transform,pad+i*(tabW+8),tabsY,tabW,42);
            float bodyY=tabsY+54,bodyH=Mathf.Max(160,h-bodyY-bottomInset-pad-65);
            float leftW=wide?(w-pad*3)*.49f:w-pad*2;
            float listH=wide?bodyH:Mathf.Max(170,bodyH*.54f);
            float rowH=Mathf.Max(31,(listH-10)/6f);
            for(int i=0;i<6;i++)
                Position((RectTransform)messageButtons[i].transform,pad,bodyY+i*rowH,leftW,rowH-5);
            float detailX=wide?pad*2+leftW:pad;
            float detailY=wide?bodyY:bodyY+listH+8;
            float detailW=wide?w-pad*3-leftW:w-pad*2;
            float detailH=wide?bodyH:Mathf.Max(60,bodyH-listH-8);
            Position(detail.rectTransform,detailX+8,detailY+6,detailW-16,Mathf.Max(40,detailH-65));
            Position((RectTransform)Find("Leer").transform,detailX+8,detailY+detailH-53,detailW*.50f-12,44);
            Position((RectTransform)Find("Eliminar").transform,detailX+detailW*.50f+4,detailY+detailH-53,detailW*.50f-12,44);
            float pageY=h-bottomInset-pad-41;
            Position((RectTransform)Find("Página anterior").transform,pad,pageY,44,37);
            Position(navLabel.rectTransform,pad+48,pageY,leftW-100,37);
            Position((RectTransform)Find("Página siguiente").transform,pad+leftW-44,pageY,44,37);
            title.fontSize=wide?28:24;detail.fontSize=wide?17:15;
        }
        void SetCategory(string value){category=value;offset=0;selectedId=null;Refresh();}
        void Page(int delta)
        {
            var s=gateway?.Snapshot();
            int total=s?.Mailbox?.List(category,0,100).Count??0;
            offset=Mathf.Clamp(offset+delta*6,0,Mathf.Max(0,(Mathf.Max(0,total-1)/6)*6));
            selectedId=null;Refresh();
        }
        void SelectRow(int index)
        {
            var list=gateway?.Snapshot()?.Mailbox?.List(category,offset,6);
            if(list==null||index>=list.Count)return;
            selectedId=list[index].EventId;Refresh();
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
        public void Refresh()
        {
            if(hud==null||gateway==null)return;
            if(window==null)Ensure();
            var s=gateway.Snapshot();var mail=s.Mailbox;
            if(entryLabel!=null)entryLabel.Put("CORREO"+(mail?.Unread()>0?" ("+mail.Unread()+")":""));
            if(!IsOpen)return;
            foreach(var t in tabLabels)
            {
                string key=t.transform.parent.name.Replace("Buzón ","");
                t.color=key==category?Gold:Ivory;
                t.Put(key.ToUpperInvariant()+((mail?.Unread(key)??0)>0?" • "+mail.Unread(key):""));
            }
            var list=mail?.List(category,offset,6)??new List<EldoriaV41Message>();
            for(int i=0;i<6;i++)
            {
                bool shown=i<list.Count;
                messageButtons[i].gameObject.SetActive(shown);
                if(shown)
                {
                    var msg=list[i];
                    messageLabels[i].color=msg.Read?Muted:Gold;
                    messageLabels[i].Put((msg.Read?"":"• ")+msg.Title);
                }
            }
            navLabel.Put((list.Count==0?"Sin mensajes":"Mensajes "+(offset+1)+"–"+(offset+list.Count)));
            var selected=mail?.Messages?.Find(m=>m.EventId==selectedId);
            if(selected==null)
            {
                detail.Put("Selecciona un mensaje para consultar su contenido.");
                return;
            }
            var battle=selected.Battle;
            detail.Put(selected.Title+"\n"+selected.Subtitle+
                (battle==null?"":"\n\n"+(battle.Won?"VICTORIA":"DERROTA")+
                "\nHéroe: "+battle.HeroId+" · Tropas: "+battle.Troops+
                "\nRondas: "+battle.Rounds+" · Poder: "+battle.PlayerPower+
                "\nVida restante: "+battle.RemainingHealth+
                "\n\n"+battle.Reason)+
                (string.IsNullOrEmpty(selected.RewardReceiptId)?"":"\n\nRecompensa registrada."));
        }
    }
}
