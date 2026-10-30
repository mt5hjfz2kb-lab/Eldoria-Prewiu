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
        RoyalHUD hud;ICommandGateway gateway;
        RectTransform window,entry;
        RoyalText entryLabel,title,detail,navLabel,resources;
        readonly List<Image> messageIcons=new List<Image>();
        readonly List<RoyalText> messageDates=new List<RoyalText>();
        readonly List<RoyalText> messageSubtitles=new List<RoyalText>();
        readonly List<RoyalText> tabBadges=new List<RoyalText>();
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
            if(window!=null && (Mathf.Abs(window.rect.width-lastWidth)>.5f ||
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
            resources=Label("Recursos actuales",w,16);
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
                label.color=new Color(.08f,.07f,.05f);label.alignment=TextAnchor.MiddleLeft;
                messageSubtitles.Add(Label("Mensaje resumen "+i,button.transform,17));
                messageDates.Add(Label("Fecha del mensaje "+i,button.transform,13));
                var icon=Node("Icono de mensaje "+i,button.transform).gameObject.AddComponent<Image>();
                icon.raycastTarget=false;icon.preserveAspect=true;messageIcons.Add(icon);
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
        public void SynchronizeLayout(){if(window!=null){Canvas.ForceUpdateCanvases();Layout();Refresh();}}
        void Layout()
        {
            if(window==null)return;
            float w=window.rect.width,h=window.rect.height;if(w<=0||h<=0)return;
            lastWidth=w;lastHeight=h;
            float scale=hud.GetComponentInParent<Canvas>()?.scaleFactor??1f;
            Rect safe=Screen.safeArea;
            float pad=Mathf.Max(14,w*.04f),top=pad+(Screen.height-safe.yMax)/Mathf.Max(.01f,scale);
            float bottom=pad+safe.yMin/Mathf.Max(.01f,scale);
            float left=Mathf.Max(pad,safe.xMin/Mathf.Max(.01f,scale)+8);
            float right=Mathf.Max(pad,(Screen.width-safe.xMax)/Mathf.Max(.01f,scale)+8);
            float usable=w-left-right;bool wide=w>h;
            var children=window.GetComponentsInChildren<Button>(true);
            Button Find(string name)=>children.First(b=>b.name==name);
            float entryWidth=Mathf.Min(140,w*.22f);
            Position(entry,w-right-entryWidth,h-bottom-(wide?118:165)-54,entryWidth,44);
            Position(title.rectTransform,left+58,top,usable*.4f,45);
            Position(resources.rectTransform,left+usable*.48f,top,usable*.52f,45);
            var back=Find("Volver");Position((RectTransform)back.transform,left,top,48,44);
            back.GetComponentInChildren<RoyalText>().Put("‹");
            float tabsY=top+100,tabH=wide?44:Mathf.Clamp(h*.09f,46,75),gap=7;
            float tabW=(usable-3*gap)/4;
            for(int i=0;i<4;i++)
            {
                var button=Find("Buzón "+categories[i]);
                Position((RectTransform)button.transform,left+i*(tabW+gap),tabsY,tabW,tabH);
                tabLabels[i].fontSize=Mathf.Clamp(Mathf.RoundToInt(tabW*.13f),16,25);
                Position((RectTransform)tabBadges[i].transform.parent,tabW-27,-9,31,31);
                tabLabels[i].horizontalOverflow=HorizontalWrapMode.Overflow;
            }
            float bodyY=tabsY+tabH+26,bodyH=h-bodyY-bottom-46;
            float rowH=bodyH/5f;
            for(int i=0;i<5;i++)
            {
                var button=messageButtons[i];Position((RectTransform)button.transform,left,bodyY+i*rowH,usable,rowH-7);
                float iconSize=Mathf.Min(rowH-23,usable*.16f);
                Position(messageIcons[i].rectTransform,14,(rowH-7-iconSize)*.5f,iconSize,iconSize);
                Position(messageLabels[i].rectTransform,iconSize+30,7,usable-iconSize-46,(rowH-12)*.48f);
                Position(messageSubtitles[i].rectTransform,iconSize+30,rowH*.48f,usable-iconSize-164,rowH*.39f);
                Position(messageDates[i].rectTransform,usable-126,rowH*.48f,110,rowH*.39f);
                messageDates[i].color=new Color(.20f,.18f,.15f);
                messageLabels[i].fontSize=Mathf.Clamp(Mathf.RoundToInt(rowH*.23f),17,29);
                messageSubtitles[i].fontSize=Mathf.Clamp(Mathf.RoundToInt(rowH*.18f),14,23);
                messageSubtitles[i].color=new Color(.20f,.18f,.15f);
            }
            Position(detail.rectTransform,left+16,bodyY+12,usable-32,Mathf.Max(40,bodyH-70));
            Position((RectTransform)Find("Leer").transform,left,bodyY+bodyH-48,usable*.49f,44);
            Position((RectTransform)Find("Eliminar").transform,left+usable*.51f,bodyY+bodyH-48,usable*.49f,44);
            float pageY=h-bottom-37;
            Position((RectTransform)Find("Página anterior").transform,left,pageY,44,37);
            Position(navLabel.rectTransform,left+52,pageY,usable-104,37);
            Position((RectTransform)Find("Página siguiente").transform,left+usable-44,pageY,44,37);
            detail.fontSize=wide?18:19;
        }
        void SetCategory(string value){category=value;offset=0;selectedId=null;Refresh();}
        void Page(int delta)
        {
            var s=gateway?.Snapshot();
            int total=s?.Mailbox?.Messages.Count(m=>m.Category==category)??0;
            offset=Mathf.Clamp(offset+delta*5,0,Mathf.Max(0,(Mathf.Max(0,total-1)/5)*5));
            selectedId=null;Refresh();
        }
        void SelectRow(int index)
        {
            var list=gateway?.Snapshot()?.Mailbox?.List(category,offset,5);
            if(list==null||index>=list.Count)return;
            selectedId=list[index].EventId;ExecuteMail("MailRead");
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
            switch(id){case "corrupt-scout":return "Acechador Corrupto";case "engendro-valoria":return "Engendro";
                case "hunt-wolf":return "Lobo";case "hunt-boar":return "Jabalí";case "ash-stalker-valoria":return "Acechador de Ceniza";
                case "fissure-valoria":return "Fisura";default:return (id??"").StartsWith("vigilant:")?"Vigilante":"el enemigo";}
        }
        public void Refresh()
        {
            if(hud==null||gateway==null)return;
            if(window==null)Ensure();
            var s=gateway.Snapshot();var mail=s.Mailbox;
            entry.gameObject.SetActive(!IsOpen&&!hud.NarrativeOpen&&
                !(hud.Modal!=null&&hud.Modal.gameObject.activeSelf)&&
                !(hud.GetComponent<RoyalGoldV3ManagementScreens>()?.IsOpen??false)&&
                string.IsNullOrEmpty(hud.SelectedBuildingId));
            resources.Put("Madera "+s.Resources.Wood+"  ·  Piedra "+s.Resources.Stone+"  ·  Comida "+s.Resources.Food);
            if(entryLabel!=null)entryLabel.Put("CORREO"+(mail?.Unread()>0?" ("+mail.Unread()+")":""));
            if(!IsOpen)return;
            foreach(var t in tabLabels)
            {
                string key=t.transform.parent.name.Replace("Buzón ","");
                t.color=key==category?Gold:Ivory;
                var surface=t.transform.parent.GetComponentInChildren<Image>();
                if(surface!=null){surface.sprite=RoyalGoldV41SourceArt.Get(key==category?"tab-active":"tab-idle",18);surface.color=Color.white;}
                int unread=mail?.Unread(key)??0,index=Array.IndexOf(categories,key);
                tabBadges[index].transform.parent.gameObject.SetActive(unread>0);
                tabBadges[index].Put(unread.ToString());
                t.Put(char.ToUpperInvariant(key[0])+key.Substring(1));
            }
            var list=mail?.List(category,offset,5)??new List<EldoriaV41Message>();
            for(int i=0;i<5;i++)
            {
                bool shown=i<list.Count && selectedId==null;
                messageButtons[i].gameObject.SetActive(shown);
                if(shown)
                {
                    var msg=list[i];
                    messageLabels[i].color=new Color(.08f,.07f,.05f);
                    messageLabels[i].Put((msg.Read?"":"● ")+msg.Title);
                    messageSubtitles[i].Put(msg.Battle==null?msg.Subtitle:
                        (msg.Battle.Won?"Victoria contra ":"Derrota ante ")+TargetName(msg.Battle.TargetId));
                    messageDates[i].Put(msg.OccurredAtUtcTicks>0?new DateTime(msg.OccurredAtUtcTicks,DateTimeKind.Utc).ToString("dd/MM HH:mm"):"");
                    string icon=msg.Category=="recompensas"?"mail-reward":msg.Category=="narrativa"?"mail-narrative":
                        msg.Category=="sistema"?"mail-system":msg.Type=="battle"?"mail-battle":"mail-march";
                    messageIcons[i].sprite=RoyalGoldV41SourceArt.Get(icon);
                }
            }
            navLabel.Put((list.Count==0?"Sin mensajes":"Mensajes "+(offset+1)+"–"+(offset+list.Count)));
            var selected=mail?.Messages?.Find(m=>m.EventId==selectedId);
            detail.gameObject.SetActive(selected!=null);
            foreach(var button in window.GetComponentsInChildren<Button>(true))
                if(button.name=="Leer"||button.name=="Eliminar")button.gameObject.SetActive(selected!=null);
            if(selected==null)
            {
                detail.Put("Selecciona un mensaje para consultar su contenido.");
                return;
            }
            var battle=selected.Battle;
            detail.Put(selected.Title+"\n"+selected.Subtitle+
                (battle==null?"":"\n\n"+(battle.Won?"VICTORIA":"DERROTA")+
                "\nHéroe: "+HeroName(battle.HeroId)+" · Tropas: "+battle.Troops+
                "\nRondas: "+battle.Rounds+" · Poder: "+battle.PlayerPower+
                "\nVida restante: "+battle.RemainingHealth+
                "\nBotín: "+battle.RewardWood+" madera · "+battle.RewardStone+" piedra · "+battle.RewardFood+" comida"+
                "\n\n"+battle.Reason)+
                (string.IsNullOrEmpty(selected.RewardReceiptId)?"":"\n\nRecompensa registrada."));
        }
    }
}
