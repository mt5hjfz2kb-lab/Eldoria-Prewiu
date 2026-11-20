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
        float nextTick,feedbackHideAt;
        Canvas hudCanvas;
        RectTransform hudTop, inspectRect, centerRect, sawmillPanelRect, feedbackRect;
        int lastLayoutWidth=-1, lastLayoutHeight=-1;
        Rect lastSafeArea;

        static readonly Color PanelInk = new Color(.075f,.095f,.108f,.86f);
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
            hudCanvas=canvas;
            canvas.renderMode=RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder=240;
            var scaler=canvasObj.GetComponent<CanvasScaler>();
            scaler.uiScaleMode=CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution=new Vector2(390,844);
            scaler.screenMatchMode=CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight=.3f;

            var top=MakePanel("Live resource strip",canvasObj.transform,
                new Vector2(.5f,1),new Vector2(0,-96),new Vector2(368,64),PanelInk);
            hudTop=top;
            level=MakeLabel("BASTIÓN I",top,16,Gold,new Vector2(0,18),new Vector2(340,23));
            resources=MakeLabel("",top,14,Ivory,new Vector2(0,-12),new Vector2(342,23));

            var select=MakeButton("Inspect sawmill",canvasObj.transform,"ASERRADERO",
                new Vector2(0,0),new Vector2(88,47),new Vector2(144,52),OpenSawmill);
            inspectRect=select.GetComponent<RectTransform>();
            // Leave the source-owned bronze 9-slice at full artist color.
            // Reapplying PanelInk after MakeButton dimmed its gilded border to
            // nearly black, undermining the new Vigilia compact HUD.
            var home=MakeButton("Center camera",canvasObj.transform,"CENTRAR",
                new Vector2(1,0),new Vector2(-84,47),new Vector2(130,52),
                ()=>GetComponent<Modular25DCameraInput>().Home());
            centerRect=home.GetComponent<RectTransform>();
            // Same original bronze profile art and button state feedback;
            // do not multiply RGB by the old dark monochrome overlay.

            panel=MakePanel("Real sawmill build action",canvasObj.transform,
                new Vector2(.5f,0),new Vector2(0,188),new Vector2(364,226),PanelInk).gameObject;
            var body=panel.GetComponent<RectTransform>();
            sawmillPanelRect=body;
            MakeLabel("ASERRADERO · VALORIA",body,18,Gold,new Vector2(-25,88),new Vector2(305,27));
            MakeButton("Close panel",body,"×",new Vector2(1,1),new Vector2(-20,-22),
                new Vector2(40,40),()=>panel.SetActive(false));
            stage=MakeLabel("",body,15,Ivory,new Vector2(0,46),new Vector2(330,26));
            cost=MakeLabel("",body,13,Ivory,new Vector2(0,14),new Vector2(330,28));
            clock=MakeLabel("",body,13,Gold,new Vector2(0,-17),new Vector2(330,25));
            build=MakeButton("Execute canonical Build command",body,"CONSTRUIR",
                new Vector2(.5f,.0f),new Vector2(0,36),new Vector2(232,49),
                ()=>gateway.BuildSawmill());
            message=MakeLabel("",canvasObj.transform,13,Ivory,
                new Vector2(0,-155),new Vector2(360,28),new Vector2(.5f,1));
            feedbackRect=message.rectTransform;
            // No additional floating opaque panel: a discreet 1-pixel text
            // shadow preserves legibility over painted grass and bright paths.
            var feedbackShade=message.gameObject.AddComponent<Shadow>();
            feedbackShade.effectColor=new Color(.035f,.041f,.035f,.89f);
            feedbackShade.effectDistance=new Vector2(1f,-1f);
            feedbackShade.useGraphicAlpha=true;
            panel.SetActive(false);
            UpdateResponsiveLayout();
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
        void Start()
        {
            Canvas.ForceUpdateCanvases();
            UpdateResponsiveLayout();
            Refresh(gateway.Snapshot());
            Feedback(gateway.LastMessage);
        }

        // Constrain the portrait HUD to screen safe areas, not just a 390x844
        // screenshot. The previous top strip had its first row outside GameView.
        // Read the actual CanvasScaler factor: Screen.safeArea uses physical pixels.
        void UpdateResponsiveLayout()
        {
            if(hudCanvas==null||hudTop==null)return;
            int width=Screen.width,height=Screen.height;
            Rect safe=Screen.safeArea;
            if(width<=0||height<=0)return;
            float scale=Mathf.Max(.01f,hudCanvas.scaleFactor);
            float safeTop=Mathf.Max(0f,(height-safe.yMax)/scale);
            float safeBottom=Mathf.Max(0f,safe.yMin/scale);
            float topClearance=Mathf.Max(64f,safeTop+10f);
            float bottomClearance=Mathf.Max(36f,safeBottom+10f);

            bool landscape=width>height;
            if(landscape)
            {
                // Native 844x390 WebGL capture: the old 368x64 strip covered
                // the Bastion and intercepted touch gestures in the middle.
                // Keep the resource ledger in the upper-right clear sky.
                // Uniform scaling also keeps both gold hairlines aligned.
                hudTop.anchorMin=hudTop.anchorMax=new Vector2(1f,1f);
                hudTop.pivot=new Vector2(1f,1f);
                hudTop.localScale=Vector3.one*.67f;
                hudTop.anchoredPosition=new Vector2(-12f,-Mathf.Max(10f,safeTop+8f));

                feedbackRect.anchorMin=feedbackRect.anchorMax=new Vector2(0f,1f);
                feedbackRect.pivot=new Vector2(0f,1f);
                feedbackRect.sizeDelta=new Vector2(260f,36f);
                feedbackRect.anchoredPosition=new Vector2(12f,-Mathf.Max(12f,safeTop+10f));
                message.fontSize=11;

                // The functional controls remain reachable but no longer form
                // two 70px-high blocks across the narrow gameplay viewport.
                inspectRect.localScale=Vector3.one*.78f;
                centerRect.localScale=Vector3.one*.78f;
                inspectRect.anchoredPosition=new Vector2(65f,bottomClearance+25f);
                centerRect.anchoredPosition=new Vector2(-65f,bottomClearance+25f);

                // Scale the real build panel, not a visual mockup or a second
                // Canvas. Keep its canonical button and countdown operable.
                sawmillPanelRect.localScale=Vector3.one*.72f;
                sawmillPanelRect.anchoredPosition=new Vector2(0f,bottomClearance+108f);
            }
            else
            {
                // Restore all layout properties on orientation change.
                hudTop.anchorMin=hudTop.anchorMax=new Vector2(.5f,1f);
                hudTop.pivot=new Vector2(.5f,.5f);
                hudTop.localScale=Vector3.one;
                hudTop.anchoredPosition=new Vector2(0f,-topClearance-32f);

                feedbackRect.anchorMin=feedbackRect.anchorMax=new Vector2(.5f,1f);
                feedbackRect.pivot=new Vector2(.5f,.5f);
                feedbackRect.sizeDelta=new Vector2(360f,28f);
                feedbackRect.anchoredPosition=new Vector2(0f,-topClearance-88f);
                message.fontSize=13;

                inspectRect.localScale=Vector3.one;
                centerRect.localScale=Vector3.one;
                inspectRect.anchoredPosition=new Vector2(88f,bottomClearance+34f);
                centerRect.anchoredPosition=new Vector2(-84f,bottomClearance+34f);
                sawmillPanelRect.localScale=Vector3.one;
                sawmillPanelRect.anchoredPosition=
                    new Vector2(0f,bottomClearance+52f+17f+113f);
            }
            lastLayoutWidth=width;
            lastLayoutHeight=height;
            lastSafeArea=safe;
        }

        void Update()
        {
            if(lastLayoutWidth!=Screen.width || lastLayoutHeight!=Screen.height ||
               lastSafeArea!=Screen.safeArea)
                UpdateResponsiveLayout();
            if(feedbackHideAt>0f&&Time.unscaledTime>=feedbackHideAt)
            {
                feedbackHideAt=0f;
                if(message!=null)message.text="";
            }
            if(Time.unscaledTime<nextTick)return;
            nextTick=Time.unscaledTime+.3f;
            if(panel!=null&&panel.activeSelf)Refresh(gateway.Snapshot());
        }

        void Feedback(string text)
        {
            if(message==null)return;
            var value=text??"";
            // Debug provenance remains available in the build evidence/logs,
            // but must not permanently float across the premium game scene.
            if(value.StartsWith("Partida experimental aislada",StringComparison.OrdinalIgnoreCase))
                value="";
            message.text=value;
            feedbackHideAt=value.Length>0?Time.unscaledTime+2.7f:0f;
        }

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
            string surface=name=="Live resource strip"?"slate_resource":
                name=="Real sawmill build action"?"brass_portrait":"profile_plate";
            var original=Resources.Load<Sprite>("ValoriaModular25D/UI/"+surface);
            if(original!=null)
            {
                image.sprite=original;
                image.type=Image.Type.Sliced;
                image.color=new Color(1f,1f,1f,color.a);
            }
            else
            {
                Debug.LogError("[MOD25D] Missing source-owned 9-slice: "+surface);
                image.color=color;
                AddHairline(rect,"Fallback bronze trim",new Vector2(0,size.y*.5f-.8f),
                    new Vector2(size.x-2f,1.6f),new Color(.75f,.61f,.38f,.49f));
            }
            image.raycastTarget=true;
            return rect;
        }

        static void AddHairline(RectTransform parent,string name,Vector2 at,
            Vector2 extent,Color tint)
        {
            var go=new GameObject(name,typeof(RectTransform),typeof(Image));
            go.transform.SetParent(parent,false);
            var rect=go.GetComponent<RectTransform>();
            rect.anchorMin=rect.anchorMax=new Vector2(.5f,.5f);
            rect.pivot=new Vector2(.5f,.5f);
            rect.anchoredPosition=at;
            rect.sizeDelta=extent;
            var image=go.GetComponent<Image>();
            image.color=tint;
            image.raycastTarget=false;
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
            string family=fontSize>=16?"CormorantGaramond-SemiBold":"SourceSans3-Semibold";
            var ownedFont=Resources.Load<Font>("ValoriaModular25D/UI/Fonts/"+family);
            if(ownedFont==null)
                Debug.LogError("[MOD25D] Licensed original font missing: "+family);
            label.font=ownedFont!=null?ownedFont:Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
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
            var graphic=rect.GetComponent<Image>();
            string surface=name=="Execute canonical Build command"?"burgundy_action":"profile_plate";
            var art=Resources.Load<Sprite>("ValoriaModular25D/UI/"+surface);
            if(art!=null)
            {
                graphic.sprite=art;
                graphic.type=Image.Type.Sliced;
                graphic.color=Color.white;
            }
            else Debug.LogError("[MOD25D] Missing primary/secondary UI art: "+surface);
            button.targetGraphic=graphic;
            button.transition=Selectable.Transition.ColorTint;
            var states=button.colors;
            states.normalColor=Color.white;
            states.highlightedColor=new Color(1f,.97f,.86f,1f);
            states.pressedColor=new Color(.79f,.76f,.69f,1f);
            states.selectedColor=new Color(1f,.96f,.83f,1f);
            states.disabledColor=new Color(.62f,.63f,.62f,.77f);
            states.fadeDuration=.085f;
            button.colors=states;
            MakeLabel(text,rect,14,Ivory,Vector2.zero,new Vector2(size.x-8,size.y-6));
            button.onClick.AddListener(click);
            return button;
        }
    }
}
