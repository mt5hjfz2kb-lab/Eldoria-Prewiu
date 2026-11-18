using System;
using Eldoria.Domain;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

namespace Eldoria.Experimentos.ValoriaModular25D
{
    /// <summary>
    /// Compact read-only status + one real canonical build action.
    /// All values come from PlayerState; no decorative or inert buttons.
    /// </summary>
    [DefaultExecutionOrder(-200)]
    public sealed class Modular25DInterface : MonoBehaviour
    {
        Modular25DGameplay gateway;
        Modular25DWorld world;
        GameObject panel;
        Text resources,level,stage,cost,clock,message;
        Button build;
        float nextTick;

        static readonly Color PanelInk = new Color(.075f,.095f,.108f,.91f);
        static readonly Color Gold = new Color(.86f,.7f,.43f);
        static readonly Color Ivory = new Color(.92f,.88f,.77f);

        void Awake()
        {
            world=GetComponent<Modular25DWorld>();
            gateway=GetComponent<Modular25DGameplay>();
            if(EventSystem.current==null)
                new GameObject("ValoriaModular25D Events",
                    typeof(EventSystem),typeof(InputSystemUIInputModule));
            var canvasObj=new GameObject("ValoriaModular25D compact HUD",
                typeof(RectTransform),typeof(Canvas),typeof(CanvasScaler),typeof(GraphicRaycaster));
            canvasObj.transform.SetParent(transform,false);
            var canvas=canvasObj.GetComponent<Canvas>();
            canvas.renderMode=RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder=240;
            var scaler=canvasObj.GetComponent<CanvasScaler>();
            scaler.uiScaleMode=CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution=new Vector2(390,844);
            scaler.screenMatchMode=CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight=.3f;

            var top=MakePanel("Live resource strip",canvasObj.transform,
                new Vector2(.5f,1),new Vector2(0,-18),new Vector2(368,64),PanelInk);
            level=MakeLabel("BASTIÓN I",top,16,Gold,new Vector2(0,18),new Vector2(340,23));
            resources=MakeLabel("",top,14,Ivory,new Vector2(0,-12),new Vector2(342,23));

            var select=MakeButton("Inspect sawmill",canvasObj.transform,"ASERRADERO",
                new Vector2(0,0),new Vector2(88,47),new Vector2(144,52),OpenSawmill);
            select.GetComponent<Image>().color=PanelInk;
            var home=MakeButton("Center camera",canvasObj.transform,"CENTRAR",
                new Vector2(1,0),new Vector2(-84,47),new Vector2(130,52),
                ()=>GetComponent<Modular25DCameraInput>().Home());
            home.GetComponent<Image>().color=PanelInk;

            panel=MakePanel("Real sawmill build action",canvasObj.transform,
                new Vector2(.5f,0),new Vector2(0,188),new Vector2(364,226),PanelInk).gameObject;
            var body=panel.GetComponent<RectTransform>();
            MakeLabel("ASERRADERO · VALORIA",body,18,Gold,new Vector2(-25,88),new Vector2(305,27));
            MakeButton("Close panel",body,"×",new Vector2(1,1),new Vector2(-20,-22),
                new Vector2(40,40),()=>panel.SetActive(false));
            stage=MakeLabel("",body,15,Ivory,new Vector2(0,46),new Vector2(330,26));
            cost=MakeLabel("",body,13,Ivory,new Vector2(0,14),new Vector2(330,28));
            clock=MakeLabel("",body,13,Gold,new Vector2(0,-17),new Vector2(330,25));
            build=MakeButton("Execute canonical Build command",body,"CONSTRUIR",
                new Vector2(.5f,.0f),new Vector2(0,36),new Vector2(232,49),
                ()=>gateway.BuildSawmill());
            message=MakeLabel("",canvasObj.transform,12,Gold,
                new Vector2(0,-98),new Vector2(360,28),new Vector2(.5f,1));
            panel.SetActive(false);
        }

        void OnEnable()
        {
            if(gateway==null)return;
            gateway.StateChanged+=Refresh;
            gateway.MessageChanged+=Feedback;
        }
        void OnDisable()
        {
            if(gateway==null)return;
            gateway.StateChanged-=Refresh;
            gateway.MessageChanged-=Feedback;
        }
        void Start(){Refresh(gateway.Snapshot());Feedback(gateway.LastMessage);}

        void Update()
        {
            if(Time.unscaledTime<nextTick)return;
            nextTick=Time.unscaledTime+.3f;
            if(panel!=null&&panel.activeSelf)Refresh(gateway.Snapshot());
        }

        void Feedback(string text){if(message!=null)message.text=text??"";}

        public void OpenSawmill()
        {
            if(panel==null)return;
            panel.SetActive(true);
            Refresh(gateway.Snapshot());
        }

        void Refresh(PlayerState s)
        {
            if(resources==null)return;
            if(s==null)
            {
                resources.text="Sin conexión con el guardado experimental";
                if(build!=null)build.interactable=false;
                return;
            }
            resources.text="MADERA "+s.Resources.Wood+"    PIEDRA "+s.Resources.Stone+
                "    COMIDA "+s.Resources.Food;
            level.text="BASTIÓN "+s.BastionLevel+"  ·  VALORIA";
            var state=ParcelBuildingStates.For(s,"sawmill");
            cost.text="Coste canónico: "+SliceRules.SawmillWoodCost+" madera";
            switch(state)
            {
                case ParcelBuildingState.BUILT:
                    stage.text="Construido · Aserradero nivel "+s.SawmillLevel;
                    clock.text="Estado conservado en el guardado";
                    build.GetComponentInChildren<Text>().text="YA CONSTRUIDO";
                    build.interactable=false;break;
                case ParcelBuildingState.UNDER_CONSTRUCTION:
                    stage.text="Obra iniciada · recursos ya descontados";
                    clock.text="Tiempo real restante: "+gateway.SawmillSecondsRemaining()+" s";
                    build.GetComponentInChildren<Text>().text="EN CONSTRUCCIÓN";
                    build.interactable=false;break;
                case ParcelBuildingState.AVAILABLE:
                    stage.text="Parcela disponible para construir";
                    clock.text=s.Resources.Wood<SliceRules.SawmillWoodCost
                        ?"Madera insuficiente; el sistema rechazará la orden."
                        :"Listo para reconstruir";
                    build.GetComponentInChildren<Text>().text="CONSTRUIR";
                    build.interactable=true;break;
                default:
                    stage.text="Parcela bloqueada por la progresión";
                    clock.text="Requisitos del juego aún no cumplidos";
                    build.GetComponentInChildren<Text>().text="BLOQUEADO";
                    build.interactable=false;break;
            }
        }

        static RectTransform MakePanel(string name,Transform parent,Vector2 anchor,Vector2 offset,Vector2 size,Color color)
        {
            var obj=new GameObject(name,typeof(RectTransform),typeof(Image));
            obj.transform.SetParent(parent,false);
            var rect=obj.GetComponent<RectTransform>();
            rect.anchorMin=anchor;rect.anchorMax=anchor;
            rect.pivot=new Vector2(.5f,.5f);
            rect.anchoredPosition=offset;rect.sizeDelta=size;
            var image=obj.GetComponent<Image>();
            image.color=color;image.raycastTarget=true;
            return rect;
        }

        static Text MakeLabel(string text,Transform parent,int fontSize,Color color,
            Vector2 position,Vector2 size,Vector2? anchor=null)
        {
            var obj=new GameObject("Label - "+text,typeof(RectTransform),typeof(Text));
            obj.transform.SetParent(parent,false);
            var rect=obj.GetComponent<RectTransform>();
            rect.anchorMin=anchor??new Vector2(.5f,.5f);
            rect.anchorMax=rect.anchorMin;
            rect.pivot=new Vector2(.5f,.5f);
            rect.anchoredPosition=position;rect.sizeDelta=size;
            var label=obj.GetComponent<Text>();
            label.font=Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            label.text=text;label.fontSize=fontSize;label.color=color;
            label.alignment=TextAnchor.MiddleCenter;
            label.horizontalOverflow=HorizontalWrapMode.Wrap;
            label.verticalOverflow=VerticalWrapMode.Truncate;
            label.raycastTarget=false;
            return label;
        }

        static Button MakeButton(string name,Transform parent,string text,
            Vector2 anchor,Vector2 offset,Vector2 size,UnityEngine.Events.UnityAction click)
        {
            var rect=MakePanel(name,parent,anchor,offset,size,new Color(.26f,.31f,.31f,.97f));
            var button=rect.gameObject.AddComponent<Button>();
            button.targetGraphic=rect.GetComponent<Image>();
            MakeLabel(text,rect,14,Ivory,Vector2.zero,new Vector2(size.x-8,size.y-6));
            button.onClick.AddListener(click);
            return button;
        }
    }
}
