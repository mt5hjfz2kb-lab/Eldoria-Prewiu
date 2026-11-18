using System;
using Eldoria.Application;
using Eldoria.Domain;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Eldoria.Valoria25D
{
    /// <summary>
    /// Lightweight ACTIONS panel attached to the original Frontier HUD.
    /// It does not create a canvas, camera, 4X node, creature, timer,
    /// resource source, reward or mission. It only exposes EXISTING
    /// Chapter III+ LocalGateway commands that the legacy I-II world UI
    /// does not yet show. Each action is validated atomically by the gateway.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class City25DExistingWorldMissions : MonoBehaviour
    {
        private ICommandGateway gateway;
        private RectTransform box;
        private Button toggle;
        private readonly Button[] actions=new Button[5];
        private readonly Text[] captions=new Text[5];
        private Text status;
        private bool open;
        private float nextRefresh;
        private int lastPressFrame=-1;
        private Font font;
        private readonly Color parchment=new Color(.95f,.9f,.8f,1f);
        private readonly Color gold=new Color(.8f,.68f,.43f,1f);
        private readonly Color dark=new Color(.06f,.08f,.10f,.96f);

        public bool IsPresentationOnly => true;
        public bool IsOpen => open;

        public void Configure(ICommandGateway owner)
        {
            if(gateway!=null||owner==null)
                throw new InvalidOperationException("World mission panel can be configured only once");
            if(SceneManager.GetActiveScene().name!=City25DWorldTransition.ExistingWorldScene)
                throw new InvalidOperationException("Do not spawn Frontier mission controls in the 2.5D city");
            gateway=owner;
            var original=GameObject.Find("Eldoria HUD")?.GetComponent<Canvas>();
            if(original==null)
                throw new InvalidOperationException("Existing world HUD is required; no second canvas");
            font=Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if(font==null)font=Font.CreateDynamicFontFromOSFont("Arial",16);
            toggle=MakeButton(original.transform,"MISIONES",Toggle);
            Anchor(toggle.GetComponent<RectTransform>(),
                new Vector2(1,1),new Vector2(1,1),
                new Vector2(-114,-122),new Vector2(-8,-82));

            var surface=new GameObject("Valoria25D_CanonicalWorldMissions",
                typeof(RectTransform),typeof(Image));
            surface.transform.SetParent(original.transform,false);
            box=(RectTransform)surface.transform;
            box.GetComponent<Image>().color=dark;
            Anchor(box,new Vector2(1,1),new Vector2(1,1),
                new Vector2(-270,-355),new Vector2(-8,-129));
            status=MakeText(box,"Misiones reales disponibles",14,gold);
            Anchor(status.rectTransform,Vector2.zero,Vector2.one,
                new Vector2(10,165),new Vector2(-10,-5));
            for(int i=0;i<actions.Length;i++)
            {
                int index=i;
                actions[i]=MakeButton(box,"---",()=>Execute(index));
                var rt=actions[i].GetComponent<RectTransform>();
                Anchor(rt,new Vector2(0,0),new Vector2(1,0),
                    new Vector2(10,9+i*33),new Vector2(-10,38+i*33));
                captions[i]=actions[i].GetComponentInChildren<Text>();
                actions[i].gameObject.SetActive(false);
            }
            box.gameObject.SetActive(false);
            Refresh();
        }

        private readonly string[] kinds=new string[5], targets=new string[5];
        private void Update()
        {
            if(gateway==null||Time.unscaledTime<nextRefresh)return;
            nextRefresh=Time.unscaledTime+.75f;
            Refresh();
        }
        private void Refresh()
        {
            if(gateway==null)return;
            var player=gateway.Snapshot();
            bool show=player.BastionLevel>=3;
            if(toggle!=null)toggle.gameObject.SetActive(show);
            if(!show)
            {
                open=false;
                if(box!=null)box.gameObject.SetActive(false);
                return;
            }
            for(int i=0;i<actions.Length;i++)
            {
                kinds[i]=targets[i]=null;
                if(actions[i]!=null)actions[i].gameObject.SetActive(false);
            }
            int used=0;
            void Add(string title,string kind,string target)
            {
                if(used>=actions.Length)return;
                kinds[used]=kind;targets[used]=target;
                captions[used].text=title;
                actions[used].gameObject.SetActive(true);
                used++;
            }
            var c=player.ChapterThree;
            if(c!=null)
            {
                if(player.BastionLevel>=3 && c.Hunts<2)
                {
                    Add("Cazar lobos · MUNDO","Fight","hunt-wolf");
                    Add("Cazar jabalí · MUNDO","Fight","hunt-boar");
                }
                if(player.BastionLevel>=3 && !c.FissureClosed)
                {
                    if(c.InvestigationEndsUtcTicks>0)
                    {
                        if(c.InvestigationEndsUtcTicks>DateTime.UtcNow.Ticks)
                            Add("Investigación en curso... ",null,null);
                    }
                    else if(!c.FissureExplored)
                        Add("Investigar la Brecha","InvestigateFissure","fissure-valoria");
                    else if(!c.LyraContacted)
                        Add("Contactar con Lyra","ContactLyra","fissure-valoria");
                    else
                        Add("Asaltar la Brecha","Fight","fissure-valoria");
                }
                if(player.BastionLevel>=3 && c.MeatRemaining>0)
                    Add("Recolectar carne","Gather","meat-valoria");
                if(player.BastionLevel>=5 && !c.AshStalkerDefeated &&
                   c.AshStalkerRespawnUtcTicks<=DateTime.UtcNow.Ticks)
                    Add("Cazar Acechador","Fight","ash-stalker-valoria");
            }
            if(used==0)status.text="No hay salidas nuevas para este capítulo.";
            else if(status!=null&&status.text.StartsWith("Misiones",StringComparison.Ordinal))
                status.text="Misiones reales · Bastión "+player.BastionLevel;
            for(int i=0;i<used;i++)
                actions[i].interactable=!string.IsNullOrEmpty(kinds[i]);
        }
        private void Toggle()
        {
            open=!open;
            if(box!=null)box.gameObject.SetActive(open);
            if(open)Refresh();
        }
        private void Execute(int index)
        {
            if(gateway==null || lastPressFrame==Time.frameCount ||
               index<0||index>=actions.Length||string.IsNullOrEmpty(kinds[index]))return;
            lastPressFrame=Time.frameCount;
            try
            {
                gateway.Advance();
                var state=gateway.Snapshot();
                var order=new GameCommand(Guid.NewGuid().ToString("N"),state.PlayerId,
                    kinds[index],targets[index],state.Revision);
                var result=gateway.Execute(order);
                status.text=result.Message;
                status.color=result.Ok?gold:new Color(1f,.57f,.48f,1f);
                Debug.Log("VALORIA25D_REAL_WORLD_MISSION kind="+order.Kind+
                    " target="+order.TargetId+" accepted="+result.Ok+
                    " revision="+result.Revision);
            }
            catch(Exception error)
            {
                status.text="Error de acción: "+error.Message;
                status.color=new Color(1f,.57f,.48f,1f);
                Debug.LogError("VALORIA25D_WORLD_COMMAND_FAILURE "+error);
            }
            Refresh();
        }
        private Button MakeButton(Transform parent,string title,UnityEngine.Events.UnityAction click)
        {
            var obj=new GameObject("Button_"+title,typeof(RectTransform),typeof(Image),typeof(Button));
            obj.transform.SetParent(parent,false);
            obj.GetComponent<Image>().color=new Color(.17f,.2f,.23f,.97f);
            var button=obj.GetComponent<Button>();
            button.onClick.AddListener(click);
            var label=MakeText(obj.transform,title,12,parchment);
            label.alignment=TextAnchor.MiddleCenter;
            Anchor(label.rectTransform,Vector2.zero,Vector2.one,Vector2.zero,Vector2.zero);
            return button;
        }
        private Text MakeText(Transform parent,string text,int size,Color color)
        {
            var o=new GameObject("Text",typeof(RectTransform),typeof(Text));
            o.transform.SetParent(parent,false);
            var t=o.GetComponent<Text>();
            t.text=text;t.font=font;t.fontSize=size;t.color=color;
            t.alignment=TextAnchor.MiddleLeft;
            t.resizeTextForBestFit=true;t.resizeTextMinSize=10;t.resizeTextMaxSize=size;
            t.horizontalOverflow=HorizontalWrapMode.Wrap;
            t.verticalOverflow=VerticalWrapMode.Truncate;
            t.raycastTarget=false;
            return t;
        }
        private static void Anchor(RectTransform t,Vector2 lo,Vector2 hi,Vector2 min,Vector2 max)
        {
            t.anchorMin=lo;t.anchorMax=hi;t.offsetMin=min;t.offsetMax=max;
        }
    }
}
