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
        Text heading, resources, power, objective, description, message, buildingTitle, buildingBody;
        GameObject buildingPanel;
        Button buildingAction;
        string feedback="";
        float refreshAt;
        int lastWidth,lastHeight;
        bool city;
        int renderedSawmill, renderedBarracks, renderedBastion;
        bool renderedScout, renderedEngendro, renderedIdle;
        public void Initialize(ICommandGateway commands){gateway=commands;}
        public void OnSceneLoaded(Scene scene,LoadSceneMode mode)
        {
            if(scene.name=="Bootstrap")return;
            city=scene.name!="Frontier";
            var state=gateway.Snapshot();
            renderedSawmill=state.SawmillLevel;renderedBarracks=state.BarracksLevel;renderedBastion=state.BastionLevel;
            renderedScout=state.ScoutDefeated;renderedEngendro=state.EngendroDefeated;
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
            var spot=ResolveHotspot(point);
            if(spot!=null)Select(spot.Id);
        }
        WorldHotspot ResolveHotspot(Vector2 point)
        {
            if(Camera.main==null)return null;

            // Valoria uses a fixed orthographic camera. Resolve the building whose projected
            // interaction footprint contains the tap before consulting depth, so stacked
            // decorative/city colliders cannot steal a building selection.
            if(city)
            {
                var projected=ResolveProjectedHotspot(point);
                if(projected!=null)return projected;
            }

            var ray=Camera.main.ScreenPointToRay(point);
            var hits=Physics.RaycastAll(ray,100f);
            System.Array.Sort(hits,(a,b)=>a.distance.CompareTo(b.distance));
            foreach(var hit in hits)
            {
                var spot=hit.collider.GetComponent<WorldHotspot>();
                if(spot!=null)return spot;
            }

            return city?ResolveProjectedHotspot(point):null;
        }

        WorldHotspot ResolveProjectedHotspot(Vector2 point)
        {
            WorldHotspot best=null;
            float bestDistance=float.MaxValue;
            float bestArea=float.MaxValue;
            foreach(var candidate in FindObjectsByType<WorldHotspot>(FindObjectsSortMode.None))
            {
                var collider=candidate.GetComponent<Collider>();
                if(collider==null||!collider.enabled)continue;
                var b=collider.bounds;
                var corners=new[]{
                    new Vector3(b.min.x,b.min.y,b.min.z),new Vector3(b.max.x,b.min.y,b.min.z),
                    new Vector3(b.min.x,b.max.y,b.min.z),new Vector3(b.max.x,b.max.y,b.min.z),
                    new Vector3(b.min.x,b.min.y,b.max.z),new Vector3(b.max.x,b.min.y,b.max.z),
                    new Vector3(b.min.x,b.max.y,b.max.z),new Vector3(b.max.x,b.max.y,b.max.z)
                };
                float minX=float.MaxValue,minY=float.MaxValue,maxX=float.MinValue,maxY=float.MinValue;
                bool inFront=false;
                foreach(var corner in corners)
                {
                    var sp=Camera.main.WorldToScreenPoint(corner);
                    if(sp.z<=0)continue;
                    inFront=true;
                    minX=Mathf.Min(minX,sp.x);minY=Mathf.Min(minY,sp.y);
                    maxX=Mathf.Max(maxX,sp.x);maxY=Mathf.Max(maxY,sp.y);
                }
                if(!inFront||point.x<minX||point.x>maxX||point.y<minY||point.y>maxY)continue;
                var centre=Camera.main.WorldToScreenPoint(b.center);
                float d=(new Vector2(centre.x,centre.y)-point).sqrMagnitude;
                float area=Mathf.Max(1f,(maxX-minX)*(maxY-minY));
                if(d<bestDistance*.92f || (Mathf.Abs(d-bestDistance)<64f && area<bestArea))
                {
                    bestDistance=d;bestArea=area;best=candidate;
                }
            }
            return best;
        }

        void Select(string id)
        {
            if(id=="gate")SceneManager.LoadScene("Frontier");
            else if(id=="forest-valoria")Send("Gather",id);
            else if(id=="corrupt-scout"||id=="engendro-valoria")Send("Fight",id);
            else if(id=="sawmill"||id=="barracks"||id=="bastion")OpenBuildingPanel(id);
        }
        void OpenBuildingPanel(string id)
        {
            if(buildingPanel==null)return;
            var s=gateway.Snapshot();
            buildingPanel.SetActive(true);
            buildingAction.onClick.RemoveAllListeners();
            if(id=="sawmill")
            {
                buildingTitle.text="ASERRADERO";
                buildingBody.text=s.SawmillLevel>0
                    ?"Edificio económico activo · produce y sostiene la reconstrucción de Valoria."
                    :"Parcela económica dañada · requiere "+SliceRules.SawmillWoodCost+" madera para reconstruirse.";
                buildingAction.GetComponentInChildren<Text>().text=s.SawmillLevel>0?"ASERRADERO ACTIVO":"RECONSTRUIR";
                buildingAction.interactable=s.SawmillLevel==0;
                if(s.SawmillLevel==0)buildingAction.onClick.AddListener(()=>{buildingPanel.SetActive(false);Send("Build","sawmill");});
            }
            else if(id=="barracks")
            {
                buildingTitle.text="CUARTEL";
                buildingBody.text=s.BarracksLevel>0
                    ?"Guarnición activa · desde aquí se entrenan los arqueros de la marcha."
                    :"Parcela militar preparada para levantar el Cuartel.";
                buildingAction.GetComponentInChildren<Text>().text=s.BarracksLevel>0?"CUARTEL ACTIVO":"CONSTRUIR CUARTEL";
                buildingAction.interactable=s.BarracksLevel==0;
                if(s.BarracksLevel==0)buildingAction.onClick.AddListener(()=>{buildingPanel.SetActive(false);Send("Build","barracks");});
            }
            else
            {
                buildingTitle.text="BASTIÓN";
                buildingBody.text="Núcleo de Valoria · nivel "+s.BastionLevel+". Su ascenso gobierna la progresión de la ciudad.";
                buildingAction.GetComponentInChildren<Text>().text="ASCENDER BASTIÓN";
                buildingAction.interactable=s.JourneyComplete&&s.BastionLevel==1;
                if(buildingAction.interactable)buildingAction.onClick.AddListener(()=>{buildingPanel.SetActive(false);Send("AdvanceBastion","bastion");});
            }
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
            if(s.SawmillLevel!=renderedSawmill||s.BarracksLevel!=renderedBarracks||s.BastionLevel!=renderedBastion||
                s.ScoutDefeated!=renderedScout||s.EngendroDefeated!=renderedEngendro||
                (s.March.Phase=="idle")!=renderedIdle)
            { feedback="";SceneManager.LoadScene(SceneManager.GetActiveScene().name);return; }
            var parts=SliceRules.TotalPower(s);
            heading.text=city?"VALORIA · BASTIÓN "+s.BastionLevel:"FRONTERA DE VALORIA";
            resources.text="MADERA  "+s.Resources.Wood+"    PIEDRA  "+s.Resources.Stone;
            power.text="⚔ PODER  "+parts.Total+"    MARCHA  "+SliceRules.Expedition(
                s.March.Phase=="idle"?s.Available:s.March.Troops,"aldric").Power;
            objective.text=s.BastionLevel==1
                ? (s.JourneyComplete ? "CAPÍTULO I COMPLETO · asciende el Bastión"
                    : s.SawmillLevel==0 ? "Necesidad: reparar el Aserradero · "+SliceRules.SawmillWoodCost+" madera"
                    : "El Aserradero produce. Observa la marca de La Brecha.")
                : (s.BarracksLevel==0 ? "BASTIÓN II · levanta el Cuartel"
                    : s.Available.Total<48 && s.March.Phase=="idle" ? "BASTIÓN II · recluta 12 arqueros"
                    : !s.EngendroDefeated ? "BASTIÓN II · derrota al Engendro de la ruta"
                    : "CAPÍTULO II COMPLETO · Valoria puede defenderse");
            string march=s.March.Phase=="idle"?"Aldric + "+s.Available.Total+" arqueros listos":
                "Aldric + "+s.March.Troops.Total+" arqueros · "+s.March.Phase;
            var expedition=SliceRules.Expedition(s.March.Phase=="idle"?s.Available:s.March.Troops,"aldric");
            description.text=city
                ? (s.BastionLevel>=2
                    ? (s.BarracksLevel>0
                        ? "El Cuartel vuelve a formar soldados. Refuerza la marcha antes de afrontar al Engendro."
                        : "Aldric: «Ya tenemos madera. Ahora necesitamos una guarnición que pueda mantener abierta la ruta.»")
                    : (s.SawmillLevel>0 ? "El fuego vuelve a la madera. La corrupción aún se ve en la frontera."
                        : "Aldric: «La Brecha dejó Valoria en ruinas. Trae madera del bosque; volveremos a levantar el Aserradero.»"))
                : (s.ForestRemaining>0 ? "Bosque: "+s.ForestRemaining+" madera. ":"Bosque agotado. ")
                    +(s.BastionLevel>=2
                        ? (s.EngendroDefeated?"El Engendro ha caído. ":"Engendro: VIDA 760, DEF 72. ")
                        : (s.ScoutDefeated?"La ruta corrupta está despejada. ":"Explorador: VIDA 620, DEF 64. "))
                    +march+"\nTu marcha: ATQ "+expedition.Attack+" · DEF "+expedition.Defense+" · VIDA "+expedition.Health+
                    (string.IsNullOrEmpty(s.LastBattleReason)?"":"\nInforme: "+s.LastBattleReason);
            message.text=string.IsNullOrEmpty(feedback)?
                (s.EngendroDefeated?"Bastión II asegurado. La Brecha sigue siendo una amenaza.":
                 s.JourneyComplete&&s.BastionLevel==1?"Valoria vuelve a respirar. Asciende el Bastión para continuar.":
                 city?"Toca la puerta para salir; vuelve con recursos para construir.":
                 "Toca un objetivo o usa los botones para enviar la Marcha."):feedback;
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
            else if(s.RecruitmentCompletesUtcTicks>0)
                message.text="Entrenamiento: "+Math.Max(0,(int)Math.Ceiling((s.RecruitmentCompletesUtcTicks-DateTime.UtcNow.Ticks)/(double)TimeSpan.TicksPerSecond))+" s";
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
            scaler.matchWidthOrHeight=1f;
            safe=new GameObject("Safe area",typeof(RectTransform)).GetComponent<RectTransform>();safe.SetParent(canvasGo.transform,false);
            UpdateSafeArea();
            var top=Panel("Top stone",safe,new Color(.055f,.075f,.10f,.82f),78,true);
            heading=Label("Heading",top,14,new Color(.91f,.78f,.53f),18);
            resources=Label("Resources",top,11,Color.white,16);
            power=Label("Power",top,10,new Color(.83f,.77f,.62f),16);
            objective=Label("Objective",top,10,new Color(.90f,.84f,.73f),18);
            var bottom=Panel("Decision rail",safe,new Color(.055f,.075f,.10f,.82f),104,false);
            description=Label("Story and world",bottom,10,new Color(.85f,.88f,.89f),24);
            var row1=Row("Actions",bottom);
            var row2=Row("Travel",bottom);
            var state=gateway.Snapshot();
            if(city)
            {
                Button(row1,"IR AL MUNDO",()=>SceneManager.LoadScene("Frontier"));
                if(state.BastionLevel==1)
                {
                    if(state.SawmillLevel==0) Button(row1,"ASERRADERO · 80",()=>Send("Build","sawmill"));
                    else if(state.JourneyComplete) Button(row1,"ASCENDER A BASTIÓN II",()=>Send("AdvanceBastion","bastion"));
                }
                else if(state.BarracksLevel==0)
                    Button(row1,"CUARTEL · 140 M / 90 P",()=>Send("Build","barracks"));
                else
                    Button(row1,"RECLUTAR +12 · 50 M",()=>Send("Recruit","archer:t1"));
            }
            else
            {
                Button(row1,"BOSQUE · RECOLECTAR",()=>Send("Gather","forest-valoria"));
                Button(row1,state.BastionLevel>=2?"ENGENDRO · PvE":"AMENAZA · PvE",
                    ()=>Send("Fight",state.BastionLevel>=2?"engendro-valoria":"corrupt-scout"));
            }
            Button(row2,city?"+ CÁMARA":"VOLVER A VALORIA",()=>{
                if(city) Zoom(-1); else SceneManager.LoadScene("Valoria");
            });
            Button(row2,city?"− CÁMARA":"ACERCAR CÁMARA",()=>Zoom(city?1:-1));
            message=Label("Feedback",bottom,9,new Color(.88f,.72f,.51f),18);
            CreateBuildingPanel(canvasGo.transform);
        }
        void CreateBuildingPanel(Transform parent)
        {
            buildingPanel=new GameObject("Building interaction panel",typeof(RectTransform),typeof(Image),typeof(VerticalLayoutGroup));
            var rt=buildingPanel.GetComponent<RectTransform>();rt.SetParent(parent,false);
            rt.anchorMin=new Vector2(.08f,.30f);rt.anchorMax=new Vector2(.92f,.70f);rt.offsetMin=rt.offsetMax=Vector2.zero;
            buildingPanel.GetComponent<Image>().color=new Color(.055f,.075f,.10f,.96f);
            var layout=buildingPanel.GetComponent<VerticalLayoutGroup>();layout.padding=new RectOffset(18,18,16,16);
            layout.spacing=8;layout.childControlHeight=true;layout.childForceExpandHeight=false;
            buildingTitle=Label("Building title",buildingPanel.transform,18,new Color(.98f,.86f,.64f),34);
            buildingBody=Label("Building body",buildingPanel.transform,12,new Color(.90f,.91f,.90f),72);
            var actionGo=new GameObject("Building action",typeof(RectTransform),typeof(Image),typeof(Button),typeof(LayoutElement));
            actionGo.transform.SetParent(buildingPanel.transform,false);
            actionGo.GetComponent<Image>().color=new Color(.25f,.22f,.17f,.98f);
            actionGo.GetComponent<LayoutElement>().preferredHeight=34;
            buildingAction=actionGo.GetComponent<Button>();
            var actionText=Label("Text",actionGo.transform,11,new Color(.98f,.86f,.64f),34);
            actionText.text="ACCIÓN";actionText.alignment=TextAnchor.MiddleCenter;
            var ar=actionText.rectTransform;ar.anchorMin=Vector2.zero;ar.anchorMax=Vector2.one;ar.offsetMin=ar.offsetMax=Vector2.zero;
            var closeGo=new GameObject("Cerrar",typeof(RectTransform),typeof(Image),typeof(Button),typeof(LayoutElement));
            closeGo.transform.SetParent(buildingPanel.transform,false);
            closeGo.GetComponent<Image>().color=new Color(.15f,.16f,.16f,.98f);
            closeGo.GetComponent<LayoutElement>().preferredHeight=30;
            closeGo.GetComponent<Button>().onClick.AddListener(()=>buildingPanel.SetActive(false));
            var closeText=Label("Text",closeGo.transform,10,Color.white,30);closeText.text="CERRAR";closeText.alignment=TextAnchor.MiddleCenter;
            var cr=closeText.rectTransform;cr.anchorMin=Vector2.zero;cr.anchorMax=Vector2.one;cr.offsetMin=cr.offsetMax=Vector2.zero;
            buildingPanel.SetActive(false);
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
            var group=t.GetComponent<VerticalLayoutGroup>();group.padding=new RectOffset(10,10,4,4);
            group.spacing=1;group.childForceExpandHeight=false;group.childControlHeight=true;
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
            t.SetParent(parent,false);t.GetComponent<LayoutElement>().preferredHeight=26;
            var layout=t.GetComponent<HorizontalLayoutGroup>();layout.spacing=7;layout.childForceExpandWidth=true;
            layout.childControlWidth=true;return t;
        }
        static void Button(Transform parent,string label,Action onClick)
        {
            var go=new GameObject(label,typeof(RectTransform),typeof(Image),typeof(Button),typeof(LayoutElement));
            go.transform.SetParent(parent,false);
            go.GetComponent<Image>().color=new Color(.25f,.22f,.17f,.98f);
            go.GetComponent<LayoutElement>().minHeight=24;
            go.GetComponent<Button>().onClick.AddListener(()=>onClick());
            var text=Label("Text",go.transform,9,new Color(.98f,.86f,.64f),24);
            text.text=label;text.alignment=TextAnchor.MiddleCenter;
            var rect=text.rectTransform;rect.anchorMin=Vector2.zero;rect.anchorMax=Vector2.one;
            rect.offsetMin=rect.offsetMax=Vector2.zero;
        }
    }
}
