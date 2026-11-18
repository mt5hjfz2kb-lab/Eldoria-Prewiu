using Eldoria.Domain;
using Eldoria.Experimentos.Ciudad2D;
using UnityEngine;
using UnityEngine.UI;

namespace Eldoria.Valoria25D
{
    /// <summary>
    /// Small reusable gameplay shell. Layout is deliberately neutral:
    /// all authoritative commands remain outside the UI implementation.
    /// One canvas only; uses the pre-existing V6 EventSystem and 2D raycaster.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class City25DMinimalHud : MonoBehaviour
    {
        private City25DPlayableController controller;
        private CanvasScaler scaler;
        private RectTransform panel;
        private Text resources, power, title, explanation, cost, requirements;
        private Text primaryText, recruitText, feedback;
        private Button primary, recruit;
        private bool showingPanel;
        private bool isLandscape;
        private static readonly Color Dark=new Color(.035f,.061f,.10f,.93f);
        private static readonly Color Panel=new Color(.095f,.085f,.074f,.95f);
        private static readonly Color Accent=new Color(.76f,.62f,.36f,1f);
        private static readonly Color Light=new Color(.96f,.89f,.73f,1f);
        private static readonly Color Muted=new Color(.78f,.77f,.72f,1f);
        private Font font;
        public bool PanelIsOpen => showingPanel;
        public bool IsPresentationOnly => true;
        public string DisplayedTitle => title!=null?title.text:"";

        public void Configure(City25DPlayableController source)
        {
            if(controller!=null)throw new System.InvalidOperationException("HUD already configured");
            controller=source;
            font=Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if(font==null)font=Font.CreateDynamicFontFromOSFont("Arial",19);
            var root=new GameObject("Valoria25D_MinimalCanvas",typeof(RectTransform),
                typeof(Canvas),typeof(CanvasScaler),typeof(GraphicRaycaster));
            root.transform.SetParent(transform,false);
            var canvas=root.GetComponent<Canvas>();
            canvas.renderMode=RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder=70;
            scaler=root.GetComponent<CanvasScaler>();
            scaler.uiScaleMode=CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution=new Vector2(390,844);
            scaler.screenMatchMode=CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            var rt=root.GetComponent<RectTransform>();

            var header=Box(rt,"RecursosReales",Dark);
            Anchor(header,new Vector2(0,1),new Vector2(1,1),
                new Vector2(0,-92),new Vector2(0,0));
            resources=Text(header,"Madera · Piedra · Comida",15,Light,TextAnchor.MiddleLeft);
            Anchor(resources.rectTransform,new Vector2(0,0),new Vector2(1,0),
                new Vector2(12,7),new Vector2(-12,36));
            power=Text(header,"Bastión · Poder",14,Accent,TextAnchor.MiddleLeft);
            Anchor(power.rectTransform,new Vector2(0,1),new Vector2(1,1),
                new Vector2(12,-33),new Vector2(-12,-7));

            var dock=Box(rt,"NavegacionCompacta",Dark);
            Anchor(dock,new Vector2(0,0),new Vector2(1,0),
                new Vector2(0,0),new Vector2(0,54));
            var bastionBtn=Button(dock,"BASTIÓN",()=>controller.Select("bastion"),.17f,.45f);
            Anchor(bastionBtn.GetComponent<RectTransform>(),new Vector2(0,0),new Vector2(0,1),
                new Vector2(5,6),new Vector2(100,-6));
            var worldBtn=Button(dock,"MUNDO",()=>controller.ShowWorldEntry(),.17f,.45f);
            Anchor(worldBtn.GetComponent<RectTransform>(),new Vector2(1,0),new Vector2(1,1),
                new Vector2(-105,6),new Vector2(-5,-6));
            feedback=Text(dock,"Selecciona una parcela",11,Muted,TextAnchor.MiddleCenter);
            Anchor(feedback.rectTransform,new Vector2(0,0),new Vector2(1,1),
                new Vector2(109,4),new Vector2(-110,-4));

            panel=Box(rt,"PanelEdificioFuncional",Panel);
            Anchor(panel,new Vector2(0,0),new Vector2(1,0),
                new Vector2(0,58),new Vector2(0,288));
            title=Text(panel,"Bastión",19,Light,TextAnchor.MiddleLeft);
            Anchor(title.rectTransform,new Vector2(0,1),new Vector2(1,1),
                new Vector2(12,-39),new Vector2(-44,-7));
            var close=Button(panel,"×",ClosePanel,.22f,.25f);
            Anchor(close.GetComponent<RectTransform>(),new Vector2(1,1),new Vector2(1,1),
                new Vector2(-42,-40),new Vector2(-6,-6));
            explanation=Text(panel,"",14,Muted,TextAnchor.UpperLeft);
            Anchor(explanation.rectTransform,new Vector2(0,1),new Vector2(1,1),
                new Vector2(12,-112),new Vector2(-12,-45));
            cost=Text(panel,"",14,Light,TextAnchor.MiddleLeft);
            Anchor(cost.rectTransform,new Vector2(0,1),new Vector2(1,1),
                new Vector2(12,-145),new Vector2(-12,-113));
            requirements=Text(panel,"",12,Accent,TextAnchor.MiddleLeft);
            Anchor(requirements.rectTransform,new Vector2(0,1),new Vector2(1,1),
                new Vector2(12,-178),new Vector2(-12,-146));
            primary=Button(panel,"CONSTRUIR / MEJORAR",controller.RunPrimary,.34f,.23f);
            Anchor(primary.GetComponent<RectTransform>(),new Vector2(0,0),new Vector2(.67f,0),
                new Vector2(10,8),new Vector2(-4,47));
            primaryText=primary.GetComponentInChildren<Text>();
            recruit=Button(panel,"RECLUTAR",controller.RunRecruit,.24f,.23f);
            Anchor(recruit.GetComponent<RectTransform>(),new Vector2(.67f,0),new Vector2(1,0),
                new Vector2(4,8),new Vector2(-10,47));
            recruitText=recruit.GetComponentInChildren<Text>();
            recruit.gameObject.SetActive(false);
            showingPanel=false;
            panel.gameObject.SetActive(false);
            AdjustLayout();
        }

        public void OpenPanel()
        {
            showingPanel=true;
            if(panel!=null)panel.gameObject.SetActive(true);
            controller?.Refresh();
        }
        public void ClosePanel()
        {
            showingPanel=false;
            if(panel!=null)panel.gameObject.SetActive(false);
        }
        public void SetFeedback(string text,bool isError)
        {
            if(feedback==null)return;
            feedback.text=text??"";
            feedback.color=isError?new Color(1f,.58f,.50f,1f):Muted;
        }
        public void ApplySnapshot(PlayerState state,string selectedId,long nowTicks)
        {
            if(state==null||resources==null)return;
            resources.text="Madera "+state.Resources.Wood.ToString("N0")+
                "   Piedra "+state.Resources.Stone.ToString("N0")+
                "   Comida "+state.Resources.Food.ToString("N0");
            var parts=SliceRules.TotalPower(state);
            power.text="BASTIÓN "+state.BastionLevel+
                "  ·  PODER "+parts.Total.ToString("N0")+
                "  ·  Tropas "+state.Available.Total;
            if(!showingPanel||panel==null||!panel.gameObject.activeSelf)return;
            var offer=City25DActionCatalog.Building(state,selectedId,nowTicks);
            var level=City25DActionCatalog.BuildingLevel(state,selectedId);
            var appearance=City2DProgressionPolicy.FromSnapshot(state,selectedId);
            title.text=City25DActionCatalog.Display(selectedId)+" · NIVEL "+level;
            explanation.text=offer.Description+"\nFase: "+appearance.Phase;
            cost.text=offer.Cost;
            requirements.text=string.IsNullOrEmpty(offer.Requirements)?"":("Falta: "+offer.Requirements);
            primary.interactable=offer.CanAttempt;
            primaryText.text=offer.Kind=="Build"?"CONSTRUIR":
                offer.Kind=="UpgradeBuilding"?"MEJORAR":
                offer.Kind=="AdvanceBastion"?"MEJORAR BASTIÓN":
                offer.CanAttempt?"ACCION":"NO DISPONIBLE";
            bool showRecruit=selectedId=="cuartel"&&state.BarracksLevel>0;
            recruit.gameObject.SetActive(showRecruit);
            if(showRecruit)
            {
                var next=City25DActionCatalog.Recruit(state,nowTicks);
                recruit.interactable=next.CanAttempt;
                recruitText.text=next.CanAttempt?"RECLUTAR":"EN CURSO";
                if(!string.IsNullOrEmpty(next.Description))
                    requirements.text=(requirements.text.Length>0?
                        requirements.text+"\n":"")+next.Description;
            }
        }
        private void Update()
        {
            if(scaler==null)return;
            bool next=Screen.width>Screen.height;
            if(isLandscape!=next)AdjustLayout();
        }
        private void AdjustLayout()
        {
            if(panel==null)return;
            isLandscape=Screen.width>Screen.height;
            scaler.matchWidthOrHeight=isLandscape?1f:0f;
            if(isLandscape)
            {
                // With landscape geometry, keep the city visible to the right.
                Anchor(panel,new Vector2(0,0),new Vector2(.53f,0),
                    new Vector2(0,56),new Vector2(0,275));
            }
            else
            {
                Anchor(panel,new Vector2(0,0),new Vector2(1,0),
                    new Vector2(0,58),new Vector2(0,288));
            }
        }
        private static RectTransform Box(Transform parent,string name,Color color)
        {
            var go=new GameObject(name,typeof(RectTransform),typeof(Image));
            go.transform.SetParent(parent,false);
            go.GetComponent<Image>().color=color;
            return (RectTransform)go.transform;
        }
        private Text Text(Transform parent,string content,int size,Color color,TextAnchor alignment)
        {
            var go=new GameObject("Text",typeof(RectTransform),typeof(Text));
            go.transform.SetParent(parent,false);
            var t=go.GetComponent<Text>();
            t.font=font;t.fontSize=size;t.text=content;
            t.color=color;t.alignment=alignment;
            t.horizontalOverflow=HorizontalWrapMode.Wrap;
            t.verticalOverflow=VerticalWrapMode.Truncate;
            t.resizeTextForBestFit=true;t.resizeTextMinSize=10;t.resizeTextMaxSize=size;
            t.raycastTarget=false;
            return t;
        }
        private Button Button(Transform parent,string label,UnityEngine.Events.UnityAction onClick,
                              float r,float g)
        {
            var go=new GameObject("Button_"+label,typeof(RectTransform),
                typeof(Image),typeof(Button));
            go.transform.SetParent(parent,false);
            go.GetComponent<Image>().color=new Color(r,g,.18f,.97f);
            var btn=go.GetComponent<Button>();
            btn.onClick.AddListener(onClick);
            var text=Text(go.transform,label,14,Light,TextAnchor.MiddleCenter);
            Anchor(text.rectTransform,Vector2.zero,Vector2.one,
                Vector2.zero,Vector2.zero);
            return btn;
        }
        private static void Anchor(RectTransform rt,Vector2 min,Vector2 max,
                                   Vector2 offsetMin,Vector2 offsetMax)
        {
            rt.anchorMin=min;rt.anchorMax=max;
            rt.offsetMin=offsetMin;rt.offsetMax=offsetMax;
        }
    }
}
