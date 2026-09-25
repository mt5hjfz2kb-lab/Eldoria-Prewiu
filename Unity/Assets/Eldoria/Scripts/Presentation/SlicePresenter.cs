using System;
using Eldoria.Application;
using Eldoria.Domain;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Eldoria.Presentation
{
    public sealed class SlicePresenter:MonoBehaviour
    {
        ICommandGateway gateway;
        RectTransform safe;
        Text heading, resources, power, objective, description, message;
        string feedback="";
        float refreshAt;
        int lastWidth,lastHeight;
        bool city;
        int renderedSawmill;
        bool renderedScout, renderedIdle;
        public void Initialize(ICommandGateway commands){gateway=commands;}
        public void OnSceneLoaded(Scene scene,LoadSceneMode mode)
        {
            if(scene.name=="Bootstrap")return;
            city=scene.name!="Frontier";
            var state=gateway.Snapshot();
            renderedSawmill=state.SawmillLevel;renderedScout=state.ScoutDefeated;
            renderedIdle=state.March.Phase=="idle";
            VisualWorld.Create(city,state);
            CreateHud();Refresh();
        }
        void Update()
        {
            if(gateway==null || safe==null)return;
            if(Time.unscaledTime>refreshAt)
            {
                refreshAt=Time.unscaledTime+.22f;
                if(gateway.Advance())Refresh();
                else RefreshClock();
            }
            if(lastWidth!=Screen.width||lastHeight!=Screen.height) UpdateSafeArea();
            var mouse=Mouse.current;
            var touch=Touchscreen.current;
            bool tapped=mouse!=null&&mouse.leftButton.wasPressedThisFrame;
            if(touch!=null&&touch.primaryTouch.press.wasPressedThisFrame)tapped=true;
            if(!tapped||Camera.main==null)return;
            if(EventSystem.current!=null&&EventSystem.current.IsPointerOverGameObject())return;
            Vector2 point=touch!=null&&touch.primaryTouch.press.isPressed
                ?touch.primaryTouch.position.ReadValue():(mouse!=null?mouse.position.ReadValue():Vector2.zero);
            var ray=Camera.main.ScreenPointToRay(point);
            if(Physics.Raycast(ray,out var hit,100))
            {
                var spot=hit.collider.GetComponent<WorldHotspot>();
                if(spot!=null)Select(spot.Id);
            }
        }
        void Select(string id)
        {
            if(id=="gate")SceneManager.LoadScene("Frontier");
            else if(id=="sawmill")Send("Build","sawmill");
            else if(id=="forest-valoria")Send("Gather",id);
            else if(id=="corrupt-scout")Send("Fight",id);
        }
        void Send(string kind,string target)
        {
            var s=gateway.Snapshot();
            var result=gateway.Execute(new GameCommand(Guid.NewGuid().ToString("N"),s.PlayerId,kind,target,s.Revision));
            feedback=result.Message;
            Refresh();
        }
        void Refresh()
        {
            if(heading==null)return;
            var s=gateway.Snapshot();
            if(s.SawmillLevel!=renderedSawmill||s.ScoutDefeated!=renderedScout||
                (s.March.Phase=="idle")!=renderedIdle)
            { feedback="";SceneManager.LoadScene(SceneManager.GetActiveScene().name);return; }
            var parts=SliceRules.TotalPower(s);
            heading.text=city?"VALORIA · LAS CENIZAS":"FRONTERA DE VALORIA";
            resources.text="MADERA  "+s.Resources.Wood+"    PIEDRA  "+s.Resources.Stone;
            power.text="⚔ PODER  "+parts.Total+"    MARCHA  "+SliceRules.Expedition(
                s.March.Phase=="idle"?s.Available:s.March.Troops,"aldric").Power;
            objective.text=s.JourneyComplete?"VALORIA HA VUELTO A CRECER":
                s.SawmillLevel==0 ? "Necesidad: reparar el Aserradero · "+SliceRules.SawmillWoodCost+" madera"
                 : "El Aserradero produce. Observa la marca de La Brecha.";
            string march=s.March.Phase=="idle"?"Aldric + "+s.Available.Total+" arqueros listos":
                "Aldric + "+s.March.Troops.Total+" arqueros · "+s.March.Phase;
            var expedition=SliceRules.Expedition(s.March.Phase=="idle"?s.Available:s.March.Troops,"aldric");
            description.text=city
                ? (s.SawmillLevel>0 ? "El fuego vuelve a la madera. La corrupción aún se ve en la frontera."
                    : "Aldric: «La Brecha dejó Valoria en ruinas. Trae madera del bosque; volveremos a levantar el Aserradero.»")
                : (s.ForestRemaining>0 ? "Bosque: "+s.ForestRemaining+" madera. ":"Bosque agotado. ")
                    +(s.ScoutDefeated?"La ruta corrupta está despejada. ":"Explorador: VIDA 620, DEF 64. ")+march+
                    "\nTu marcha: ATQ "+expedition.Attack+" · DEF "+expedition.Defense+" · VIDA "+expedition.Health+
                    (string.IsNullOrEmpty(s.LastBattleReason)?"":"\nInforme: "+s.LastBattleReason);
            message.text=string.IsNullOrEmpty(feedback)?
                (s.JourneyComplete?"Fin de esta primera slice. La Brecha no se ha cerrado.":
                 city?"Toca la puerta para salir; vuelve con madera para construir.":
                 "Toca el bosque o usa los botones para enviar la Marcha."):feedback;
            RefreshClock();
        }
        void RefreshClock()
        {
            if(message==null)return;
            var s=gateway.Snapshot();
            if(s.March.Phase!="idle")
                message.text="Marcha: "+s.March.Phase+" · destino "+s.March.TargetId+" · regreso y recompensa automáticos";
            else if(s.BuildingCompletesUtcTicks>0)
                message.text="Reconstrucción: "+Math.Max(0,(int)Math.Ceiling((s.BuildingCompletesUtcTicks-DateTime.UtcNow.Ticks)/(double)TimeSpan.TicksPerSecond))+" s";
        }
        void CreateHud()
        {
            if(EventSystem.current==null)
            {
                var ev=new GameObject("UI events",typeof(EventSystem),typeof(InputSystemUIInputModule));
            }
            var canvasGo=new GameObject("Eldoria HUD",typeof(RectTransform),typeof(Canvas),typeof(CanvasScaler),typeof(GraphicRaycaster));
            var canvas=canvasGo.GetComponent<Canvas>();canvas.renderMode=RenderMode.ScreenSpaceOverlay;canvas.sortingOrder=100;
            var scaler=canvasGo.GetComponent<CanvasScaler>();scaler.uiScaleMode=CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution=new Vector2(390,844);scaler.screenMatchMode=CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight=.5f;
            safe=new GameObject("Safe area",typeof(RectTransform)).GetComponent<RectTransform>();safe.SetParent(canvasGo.transform,false);
            UpdateSafeArea();
            var top=Panel("Top stone",safe,new Color(.055f,.075f,.10f,.90f),104,true);
            heading=Label("Heading",top,17,new Color(.91f,.78f,.53f),26);
            resources=Label("Resources",top,13,Color.white,22);
            power=Label("Power",top,12,new Color(.83f,.77f,.62f),22);
            objective=Label("Objective",top,12,new Color(.90f,.84f,.73f),26);
            var bottom=Panel("Decision rail",safe,new Color(.055f,.075f,.10f,.90f),154,false);
            description=Label("Story and world",bottom,12,new Color(.85f,.88f,.89f),48);
            var row1=Row("Actions",bottom);
            var row2=Row("Travel",bottom);
            if(city)
            {
                Button(row1,"IR AL MUNDO",()=>SceneManager.LoadScene("Frontier"));
                Button(row1,"ASERRADERO · 80",()=>Send("Build","sawmill"));
            }
            else
            {
                Button(row1,"BOSQUE · RECOLECTAR",()=>Send("Gather","forest-valoria"));
                Button(row1,"AMENAZA · PvE",()=>Send("Fight","corrupt-scout"));
            }
            Button(row2,city?"+ CÁMARA":"VOLVER A VALORIA",()=>{
                if(city) Zoom(-1); else SceneManager.LoadScene("Valoria");
            });
            Button(row2,city?"− CÁMARA":"ACERCAR CÁMARA",()=>Zoom(city?1:-1));
            message=Label("Feedback",bottom,11,new Color(.88f,.72f,.51f),30);
        }
        void Zoom(float amount){if(Camera.main!=null)Camera.main.orthographicSize=Mathf.Clamp(Camera.main.orthographicSize+amount,9,19);}
        void UpdateSafeArea()
        {
            lastWidth=Screen.width;lastHeight=Screen.height;
            if(safe==null||lastWidth==0||lastHeight==0)return;
            Rect r=Screen.safeArea;
            safe.anchorMin=new Vector2(r.xMin/lastWidth,r.yMin/lastHeight);
            safe.anchorMax=new Vector2(r.xMax/lastWidth,r.yMax/lastHeight);
            safe.offsetMin=safe.offsetMax=Vector2.zero;
        }
        static RectTransform Panel(string name,Transform parent,Color color,float height,bool top)
        {
            var t=new GameObject(name,typeof(RectTransform),typeof(Image),typeof(VerticalLayoutGroup)).GetComponent<RectTransform>();
            t.SetParent(parent,false);t.anchorMin=new Vector2(0,top?1:0);t.anchorMax=new Vector2(1,top?1:0);
            t.pivot=new Vector2(.5f,top?1:0);t.sizeDelta=new Vector2(0,height);t.anchoredPosition=Vector2.zero;
            t.GetComponent<Image>().color=color;
            var group=t.GetComponent<VerticalLayoutGroup>();group.padding=new RectOffset(13,13,6,6);
            group.spacing=2;group.childForceExpandHeight=false;group.childControlHeight=true;
            return t;
        }
        static Font Font()
        {
            var f=Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            return f;
        }
        static Text Label(string name,Transform parent,int size,Color color,int height)
        {
            var t=new GameObject(name,typeof(RectTransform),typeof(Text),typeof(LayoutElement)).GetComponent<Text>();
            t.transform.SetParent(parent,false);t.font=Font();t.fontSize=size;t.color=color;
            t.alignment=TextAnchor.MiddleLeft;t.horizontalOverflow=HorizontalWrapMode.Wrap;
            t.verticalOverflow=VerticalWrapMode.Truncate;
            t.GetComponent<LayoutElement>().preferredHeight=height;
            return t;
        }
        static RectTransform Row(string name,Transform parent)
        {
            var t=new GameObject(name,typeof(RectTransform),typeof(HorizontalLayoutGroup),typeof(LayoutElement)).GetComponent<RectTransform>();
            t.SetParent(parent,false);t.GetComponent<LayoutElement>().preferredHeight=34;
            var layout=t.GetComponent<HorizontalLayoutGroup>();layout.spacing=7;layout.childForceExpandWidth=true;
            layout.childControlWidth=true;return t;
        }
        static void Button(Transform parent,string label,Action onClick)
        {
            var go=new GameObject(label,typeof(RectTransform),typeof(Image),typeof(Button),typeof(LayoutElement));
            go.transform.SetParent(parent,false);
            go.GetComponent<Image>().color=new Color(.25f,.22f,.17f,.98f);
            go.GetComponent<LayoutElement>().minHeight=32;
            go.GetComponent<Button>().onClick.AddListener(()=>onClick());
            var text=Label("Text",go.transform,11,new Color(.98f,.86f,.64f),32);
            text.text=label;text.alignment=TextAnchor.MiddleCenter;
            var rect=text.rectTransform;rect.anchorMin=Vector2.zero;rect.anchorMax=Vector2.one;
            rect.offsetMin=rect.offsetMax=Vector2.zero;
        }
    }
}
