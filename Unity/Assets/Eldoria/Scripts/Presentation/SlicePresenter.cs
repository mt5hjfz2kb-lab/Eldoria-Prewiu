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
        Text heading, resources, stoneResource, power, objective, description, message, buildingTitle, buildingBody, primaryActionText;
        GameObject buildingPanel;
        Button buildingAction, primaryAction;
        string feedback="";
        float refreshAt;
        float resetQaArmedUntil;
        int lastWidth,lastHeight;
        bool city;
        int renderedSawmill, renderedBarracks, renderedBastion;
        bool renderedScout, renderedEngendro, renderedIdle;
        Vector2 pointerStart,pointerLast;
        bool pointerActive,pointerDragged,pointerStartedOverUi;
        Vector3 cameraHome;
        float panHalfX=5f,panHalfZ=4f;
        const float PanGestureThreshold=12f;
        Camera OfficialCamera => GameObject.Find("Isometric camera")?.GetComponent<Camera>() ?? Camera.main;
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
            if(city&&OfficialCamera!=null)
            {
                cameraHome=OfficialCamera.transform.position;
                ConfigureCityPanBounds(state.BastionLevel);
            }
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
            HandlePointerInput();
        }
        void HandlePointerInput()
        {
            var camera=OfficialCamera;
            if(camera==null)return;
            var mouse=Mouse.current;
            var touch=Touchscreen.current;

            bool touchPressed=touch!=null&&touch.primaryTouch.press.wasPressedThisFrame;
            bool touchHeld=touch!=null&&touch.primaryTouch.press.isPressed;
            bool touchReleased=touch!=null&&touch.primaryTouch.press.wasReleasedThisFrame;
            bool mousePressed=mouse!=null&&mouse.leftButton.wasPressedThisFrame;
            bool mouseHeld=mouse!=null&&mouse.leftButton.isPressed;
            bool mouseReleased=mouse!=null&&mouse.leftButton.wasReleasedThisFrame;

            bool usingTouch=touchHeld||touchPressed||touchReleased;
            Vector2 point=usingTouch&&touch!=null
                ?touch.primaryTouch.position.ReadValue()
                :(mouse!=null?mouse.position.ReadValue():Vector2.zero);
            bool pressed=usingTouch?touchPressed:mousePressed;
            bool held=usingTouch?touchHeld:mouseHeld;
            bool released=usingTouch?touchReleased:mouseReleased;

            if(pressed)
            {
                pointerActive=true;
                pointerDragged=false;
                pointerStart=pointerLast=point;
                pointerStartedOverUi=EventSystem.current!=null&&EventSystem.current.IsPointerOverGameObject();
            }

            if(pointerActive&&held)
            {
                if(IsPanGesture(pointerStart,point))pointerDragged=true;
                if(city&&pointerDragged&&!pointerStartedOverUi)
                    PanCameraByScreenDelta(point-pointerLast);
                pointerLast=point;
            }

            if(pointerActive&&released)
            {
                bool shouldSelect=!pointerStartedOverUi&&!pointerDragged;
                pointerActive=false;
                if(shouldSelect)
                {
                    var spot=ResolveHotspot(point);
                    if(spot!=null)Select(spot.Id);
                }
            }
        }

        static bool IsPanGesture(Vector2 start,Vector2 current)
            => (current-start).sqrMagnitude>=PanGestureThreshold*PanGestureThreshold;

        void ConfigureCityPanBounds(int bastionLevel)
        {
            // The West Rebuilders quarter is already authored/inhabited in the I-II city,
            // so early-game bounds must let a portrait mobile viewport actually reach it.
            // Portrait gets a little extra horizontal travel because its visible world width is smaller.
            var camera=OfficialCamera;
            float aspect=camera!=null&&camera.aspect>0f?camera.aspect:(Screen.height>0?Screen.width/(float)Screen.height:.5625f);
            float portraitExtra=Mathf.Clamp((.80f-aspect)*8f,0f,2.5f);
            if(bastionLevel<=10){panHalfX=13.5f+portraitExtra;panHalfZ=7f;}
            else if(bastionLevel<=15){panHalfX=15f+portraitExtra;panHalfZ=9f;}
            else if(bastionLevel<=20){panHalfX=17.5f+portraitExtra;panHalfZ=11f;}
            else if(bastionLevel<=25){panHalfX=20f+portraitExtra;panHalfZ=13f;}
            else {panHalfX=23f+portraitExtra;panHalfZ=16f;}
        }

        void PanCameraByScreenDelta(Vector2 screenDelta)
        {
            var camera=OfficialCamera;
            if(!city||camera==null)return;
            float worldPerPixel=(camera.orthographicSize*2f)/Mathf.Max(1f,camera.pixelHeight);
            var right=Vector3.ProjectOnPlane(camera.transform.right,Vector3.up).normalized;
            var up=Vector3.ProjectOnPlane(camera.transform.up,Vector3.up).normalized;
            if(up.sqrMagnitude<.001f)up=Vector3.forward;
            var desired=camera.transform.position+(-right*screenDelta.x-up*screenDelta.y)*worldPerPixel;
            var offset=desired-cameraHome;
            offset.x=Mathf.Clamp(offset.x,-panHalfX,panHalfX);
            offset.z=Mathf.Clamp(offset.z,-panHalfZ,panHalfZ);
            camera.transform.position=new Vector3(cameraHome.x+offset.x,cameraHome.y,cameraHome.z+offset.z);
        }

        void RecenterCamera()
        {
            if(!city||OfficialCamera==null)return;
            OfficialCamera.transform.position=cameraHome;
        }

        void FocusCityHotspot(string objectName)
        {
            var camera=OfficialCamera;
            var target=GameObject.Find(objectName);
            if(!city||camera==null||target==null)return;
            var collider=target.GetComponent<Collider>();
            var focus=collider!=null?collider.bounds.center:target.transform.position;

            // Translate the fixed-orientation camera so the target becomes the visual centre.
            // Rotation/zoom stay untouched, preserving the authored 4X camera language.
            var ray=camera.ViewportPointToRay(new Vector3(.5f,.5f,0f));
            var plane=new Plane(Vector3.up,new Vector3(0,focus.y,0));
            if(!plane.Raycast(ray,out var distance))return;
            var centre=ray.GetPoint(distance);
            var desired=camera.transform.position+(focus-centre);
            var offset=desired-cameraHome;
            offset.x=Mathf.Clamp(offset.x,-panHalfX,panHalfX);
            offset.z=Mathf.Clamp(offset.z,-panHalfZ,panHalfZ);
            camera.transform.position=new Vector3(cameraHome.x+offset.x,cameraHome.y,cameraHome.z+offset.z);
        }

        WorldHotspot ResolveHotspot(Vector2 point)
        {
            var camera=OfficialCamera;
            if(camera==null)return null;
            var ray=camera.ScreenPointToRay(point);
            var hits=Physics.RaycastAll(ray,100f);
            System.Array.Sort(hits,(a,b)=>a.distance.CompareTo(b.distance));

            if(city)
            {
                // Fixed-camera 4X selection: if a tap ray intersects more than one canonical
                // building target, select the target whose centreline is closest to the ray.
                // This keeps clicks deterministic even when silhouettes overlap in projection.
                WorldHotspot bestTarget=null;
                float bestLineDistance=float.MaxValue;
                foreach(var hit in hits)
                {
                    var spot=hit.collider.GetComponent<WorldHotspot>();
                    if(spot==null||!spot.gameObject.name.EndsWith("· target",System.StringComparison.Ordinal))continue;
                    var centre=hit.collider.bounds.center;
                    float along=Mathf.Max(0f,Vector3.Dot(centre-ray.origin,ray.direction));
                    var closest=ray.origin+ray.direction*along;
                    float d=(centre-closest).sqrMagnitude;
                    if(d<bestLineDistance){bestLineDistance=d;bestTarget=spot;}
                }
                if(bestTarget!=null)return bestTarget;
            }

            foreach(var hit in hits)
            {
                var spot=hit.collider.GetComponent<WorldHotspot>();
                if(spot!=null)return spot;
            }
            return null;
        }

        void Select(string id)
        {
            if(id=="gate")SceneManager.LoadScene("Frontier");
            else if(id=="forest-valoria"||id=="quarry-valoria")Send("Gather",id);
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
                if(s.BarracksLevel==0)
                {
                    buildingBody.text="Parcela militar preparada para levantar el Cuartel.";
                    buildingAction.GetComponentInChildren<Text>().text="CONSTRUIR CUARTEL";
                    buildingAction.interactable=true;
                    buildingAction.onClick.AddListener(()=>{buildingPanel.SetActive(false);Send("Build","barracks");});
                }
                else if((s.ChapterProgress?.TrainedArchers??0)<SliceContentProfiles.Active.Chapter2TrainArchers)
                {
                    buildingBody.text="Guarnición activa · "+s.Available.Total+" arqueros disponibles · "+
                        (s.ChapterProgress?.TrainedArchers??0)+"/"+SliceContentProfiles.Active.Chapter2TrainArchers+" entrenados en este capítulo. "+
                        "Entrena refuerzos antes de preparar la Marcha contra el Engendro.";
                    buildingAction.GetComponentInChildren<Text>().text="RECLUTAR +"+SliceRules.RecruitArchers;
                    buildingAction.interactable=s.RecruitmentCompletesUtcTicks==0;
                    if(buildingAction.interactable)
                        buildingAction.onClick.AddListener(()=>{buildingPanel.SetActive(false);Send("Recruit","archer:t1");});
                }
                else
                {
                    var preview=SliceRules.Expedition(s.Available,"aldric");
                    buildingBody.text=(s.MarchConfigured
                        ?"Marcha confirmada · Sir Aldric + "+s.PreparedTroops.Total+" arqueros."
                        :"Tropas suficientes. Prepara la expedición antes de atacar.")+
                        "\nPoder disponible "+preview.Power+".";
                    buildingAction.GetComponentInChildren<Text>().text=s.MarchConfigured?"REVISAR MARCHA":"PREPARAR MARCHA";
                    buildingAction.interactable=s.March.Phase=="idle";
                    if(buildingAction.interactable)
                        buildingAction.onClick.AddListener(OpenMarchPanel);
                }
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
        void OpenMarchPanel()
        {
            if(buildingPanel==null)return;
            var s=gateway.Snapshot();
            var prepared=SliceRules.Expedition(s.Available,"aldric");
            buildingPanel.SetActive(true);
            buildingAction.onClick.RemoveAllListeners();
            buildingTitle.text="PREPARAR MARCHA";
            buildingBody.text="Sir Aldric · "+s.Available.Total+" Arqueros disponibles\n"+
                "ATQ "+prepared.Attack+" · DEF "+prepared.Defense+" · VIDA "+prepared.Health+
                " · RUP "+prepared.Break+"\nPoder de expedición "+prepared.Power+
                "\nConfirma esta composición antes de atacar al Engendro.";
            buildingAction.GetComponentInChildren<Text>().text="CONFIRMAR MARCHA";
            buildingAction.interactable=s.March.Phase=="idle"&&s.Available.Total>0;
            if(buildingAction.interactable)
                buildingAction.onClick.AddListener(()=>{buildingPanel.SetActive(false);Send("ConfigureMarch","march-main");});
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
            heading.text="VALORIA\nBastión "+s.BastionLevel;
            resources.text="♣  MADERA\n"+s.Resources.Wood;
            if(stoneResource!=null)stoneResource.text="◆  PIEDRA\n"+s.Resources.Stone;
            var marchPreview=s.March.Phase!="idle"?s.March.Troops:
                (s.MarchConfigured?s.PreparedTroops:s.Available);
            power.text="⚔  PODER\n"+parts.Total;
            var marchPower=SliceRules.Expedition(
                marchPreview,s.March.Phase!="idle"?s.March.HeroId:(s.MarchConfigured?s.PreparedHeroId:"aldric")).Power;
            var cp=s.ChapterProgress??new ChapterProgressState();
            objective.text=ObjectiveText(s,cp);
            ConfigurePrimaryAction(s);
            string march=s.March.Phase=="idle"?"Aldric + "+s.Available.Total+" arqueros listos":
                "Aldric + "+s.March.Troops.Total+" arqueros · "+s.March.Phase;
            var expedition=SliceRules.Expedition(s.March.Phase=="idle"?s.Available:s.March.Troops,"aldric");
            if(expedition.Power!=marchPower) marchPower=expedition.Power;
            description.text=city
                ? (s.BastionLevel>=2
                    ? (s.BarracksLevel>0
                        ? ((cp.TrainedArchers<SliceContentProfiles.Active.Chapter2TrainArchers)
                            ? ((s.Resources.Wood<SliceRules.RecruitWoodCost||s.Resources.Stone<SliceRules.RecruitStoneCost)
                                ? "El Cuartel necesita recursos para equipar a los nuevos arqueros. Vuelve al Mundo, reúne lo que falta y regresa."
                                : "El Cuartel está listo. Entrena a los arqueros que necesita la primera Marcha de Bastión II.")
                            : (s.MarchConfigured
                                ? "La Marcha está preparada. Sal a la frontera y enfrenta al Engendro."
                                : "Los arqueros están listos. Prepara y confirma la Marcha antes de afrontar al Engendro."))
                        : "Aldric: «Ya tenemos madera. Ahora necesitamos una guarnición que pueda mantener abierta la ruta.»")
                    : (s.SawmillLevel>0 ? "El fuego vuelve a la madera. La corrupción aún se ve en la frontera."
                        : "Aldric: «La Brecha dejó Valoria en ruinas. Trae madera del bosque; volveremos a levantar el Aserradero.»"))
                : FrontierDescription(s,march,expedition);
            message.text=string.IsNullOrEmpty(feedback)?
                (s.EngendroDefeated?"Bastión II asegurado. La Brecha sigue siendo una amenaza.":
                 s.JourneyComplete&&s.BastionLevel==1?"Valoria vuelve a respirar. Asciende el Bastión para continuar.":
                 city?"Toca la puerta para salir; vuelve con recursos para construir.":
                 "Toca un objetivo o usa los botones para enviar la Marcha."):feedback;
            RefreshClock();
        }
        string ObjectiveText(PlayerState s,ChapterProgressState cp)
        {
            switch(SliceRules.CurrentObjectiveKey(s))
            {
                case "b1.build-sawmill":
                    return s.Resources.Wood<SliceRules.SawmillWoodCost
                        ? "BASTIÓN I · consigue madera para reparar el Aserradero · "+s.Resources.Wood+"/"+SliceRules.SawmillWoodCost
                        : "BASTIÓN I · repara el Aserradero · "+SliceRules.SawmillWoodCost+" madera";
                case "b1.gather-wood": return "BASTIÓN I · recupera madera · "+cp.GatheredWood+"/"+SliceContentProfiles.Active.Chapter1GatherWood;
                case "b1.gather-stone": return "BASTIÓN I · recupera piedra · "+cp.GatheredStone+"/"+SliceContentProfiles.Active.Chapter1GatherStone;
                case "b1.clear-route": return "BASTIÓN I · despeja la ruta corrupta";
                case "b1.return": return "BASTIÓN I · regresa a Valoria";
                case "b1.ascend": return "CAPÍTULO I COMPLETO · asciende el Bastión";
                case "b2.build-barracks": return "BASTIÓN II · levanta el Cuartel";
                case "b2.train-archers":
                    return (s.Resources.Wood<SliceRules.RecruitWoodCost||s.Resources.Stone<SliceRules.RecruitStoneCost)
                        ? "BASTIÓN II · reúne recursos para equipar arqueros · M "+s.Resources.Wood+"/"+SliceRules.RecruitWoodCost+
                          " · P "+s.Resources.Stone+"/"+SliceRules.RecruitStoneCost
                        : "BASTIÓN II · entrena arqueros · "+cp.TrainedArchers+"/"+SliceContentProfiles.Active.Chapter2TrainArchers;
                case "b2.prepare-march": return "BASTIÓN II · prepara y confirma la Marcha";
                case "b2.raise-expedition-power": return "BASTIÓN II · Poder de expedición · "+cp.ConfirmedExpeditionPower+"/"+SliceContentProfiles.Active.Chapter2ExpeditionPower;
                case "b2.defeat-engendro": return "BASTIÓN II · derrota al Engendro de la ruta";
                case "b2.complete": return "CAPÍTULO II COMPLETO · Valoria puede defenderse";
                default: return "ELDORIA · objetivo no disponible";
            }
        }

        string FrontierDescription(PlayerState s,string march,CombatStats expedition)
        {
            string text=(s.ForestRemaining>0 ? "Bosque: "+s.ForestRemaining+" madera. ":"Bosque agotado. ")
                +(s.QuarryRemaining>0 ? "Cantera: "+s.QuarryRemaining+" piedra. ":"Cantera agotada. ")
                +(s.BastionLevel>=2
                    ? (s.EngendroDefeated?"El Engendro ha caído. ":"Engendro: VIDA 760, DEF 72. ")
                    : (s.ScoutDefeated?"La ruta corrupta está despejada. ":"Explorador: VIDA 620, DEF 64. "))
                +march+"\nTu marcha: ATQ "+expedition.Attack+" · DEF "+expedition.Defense+" · VIDA "+expedition.Health;
            var r=s.LastBattleReport;
            if(r!=null&&!string.IsNullOrEmpty(r.TargetId))
            {
                string target=r.TargetId=="engendro-valoria"?"Engendro":"Explorador corrupto";
                string reward=r.Won&&(r.RewardWood>0||r.RewardStone>0)
                    ?" · Recompensa "+r.RewardWood+" M / "+r.RewardStone+" P":"";
                text+="\nÚLTIMO COMBATE · "+target+" · "+(r.Won?"VICTORIA":"DERROTA")+
                    "\nAldric + "+r.Troops+" arqueros · Poder "+r.PlayerPower+
                    " · "+r.Rounds+" rondas · Vida restante "+r.RemainingHealth+reward+
                    (string.IsNullOrEmpty(r.Reason)?"":"\n"+r.Reason);
            }
            return text;
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
                new GameObject("UI events",typeof(EventSystem),typeof(InputSystemUIInputModule));

            var canvasGo=new GameObject("Eldoria HUD",typeof(RectTransform),typeof(Canvas),typeof(CanvasScaler),typeof(GraphicRaycaster));
            var canvas=canvasGo.GetComponent<Canvas>();canvas.renderMode=RenderMode.ScreenSpaceOverlay;canvas.sortingOrder=100;
            var scaler=canvasGo.GetComponent<CanvasScaler>();scaler.uiScaleMode=CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution=new Vector2(390,844);scaler.screenMatchMode=CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight=1f;
            safe=new GameObject("Safe area",typeof(RectTransform)).GetComponent<RectTransform>();safe.SetParent(canvasGo.transform,false);
            UpdateSafeArea();

            // Port the canonical v0.26.4 reference grammar into Unity:
            // compact kingdom header, separate quest card, floating objective dock, fixed bottom navigation.
            var top=HorizontalPanel("Reference topbar",safe,new Color(.04f,.065f,.085f,.96f),68,true);
            var topLayout=top.GetComponent<HorizontalLayoutGroup>();
            topLayout.childForceExpandWidth=false;topLayout.childAlignment=TextAnchor.MiddleLeft;

            var crest=ChipLabel("Realm crest",top,20,new Color(.78f,.68f,.45f),
                new Color(.025f,.04f,.055f,.96f),42);
            crest.text="♙";crest.alignment=TextAnchor.MiddleCenter;
            heading=Label("Heading",top,10,new Color(.95f,.94f,.90f),44);
            heading.GetComponent<LayoutElement>().preferredWidth=70;
            heading.alignment=TextAnchor.MiddleLeft;

            resources=ResourceChip("Wood resource",top,"♣","MADERA",62);
            stoneResource=ResourceChip("Stone resource",top,"◆","PIEDRA",62);
            power=ResourceChip("Power",top,"⚔","PODER",72,new Color(.085f,.075f,.045f,.96f));

            var quest=new GameObject("Quest panel",typeof(RectTransform),typeof(Image),typeof(VerticalLayoutGroup));
            var qrt=quest.GetComponent<RectTransform>();qrt.SetParent(safe,false);
            qrt.anchorMin=qrt.anchorMax=new Vector2(0,1);qrt.pivot=new Vector2(0,1);
            qrt.sizeDelta=new Vector2(300,66);qrt.anchoredPosition=new Vector2(10,-78);
            quest.GetComponent<Image>().color=new Color(.035f,.055f,.075f,.90f);
            var qLayout=quest.GetComponent<VerticalLayoutGroup>();qLayout.padding=new RectOffset(11,11,7,7);
            qLayout.spacing=1;qLayout.childControlHeight=true;qLayout.childForceExpandHeight=false;
            var kicker=Label("Quest kicker",quest.transform,7,new Color(.78f,.68f,.45f),12);
            kicker.text="OBJETIVO ACTUAL";kicker.alignment=TextAnchor.MiddleLeft;
            objective=Label("Objective",quest.transform,10,new Color(.88f,.90f,.90f),40);

            var nav=HorizontalPanel("Bottom navigation",safe,new Color(.035f,.055f,.075f,.97f),68,false);
            var cityNav=NavButton(nav,"⌂","CIUDAD",()=>{if(!city)SceneManager.LoadScene("Valoria");});
            var worldNav=NavButton(nav,"◎","MUNDO",()=>{if(city)SceneManager.LoadScene("Frontier");});
            var heroesNav=NavButton(nav,"♞","HÉROES",()=>{});
            var chestNav=NavButton(nav,"▣","ARCÓN",()=>{});
            var codexNav=NavButton(nav,"⌘","CÓDICE",()=>{});
            heroesNav.interactable=false;chestNav.interactable=false;codexNav.interactable=false;
            StyleNavButton(cityNav,city);StyleNavButton(worldNav,!city);
            StyleNavButton(heroesNav,false);StyleNavButton(chestNav,false);StyleNavButton(codexNav,false);

            var dock=new GameObject("World objective dock",typeof(RectTransform),typeof(Image),typeof(VerticalLayoutGroup));
            var drt=dock.GetComponent<RectTransform>();drt.SetParent(safe,false);
            drt.anchorMin=drt.anchorMax=new Vector2(.5f,0);drt.pivot=new Vector2(.5f,0);
            drt.sizeDelta=new Vector2(360,94);drt.anchoredPosition=new Vector2(0,78);
            dock.GetComponent<Image>().color=new Color(.035f,.055f,.075f,.95f);
            var dLayout=dock.GetComponent<VerticalLayoutGroup>();dLayout.padding=new RectOffset(10,10,7,7);
            dLayout.spacing=3;dLayout.childControlHeight=true;dLayout.childForceExpandHeight=false;
            description=Label("Story and world",dock.transform,8,new Color(.86f,.88f,.88f),22);
            var row1=Row("Primary objective action",dock.transform);
            row1.GetComponent<LayoutElement>().preferredHeight=46;
            primaryAction=Button(row1,"CONTINUAR",InvokePrimaryObjective);
            primaryAction.GetComponent<LayoutElement>().minHeight=46;
            primaryAction.GetComponent<Image>().color=new Color(.73f,.61f,.36f,.98f);
            primaryActionText=primaryAction.GetComponentInChildren<Text>();
            if(primaryActionText!=null)primaryActionText.color=new Color(.07f,.09f,.11f);
            message=Label("Feedback",dock.transform,7,new Color(.91f,.73f,.48f),12);

            ConfigurePrimaryAction(gateway.Snapshot());
            CreateBuildingPanel(canvasGo.transform);
        }

        static void StyleNavButton(Button button,bool active)
        {
            if(button==null)return;
            var image=button.GetComponent<Image>();
            var text=button.GetComponentInChildren<Text>();
            image.color=active?new Color(.085f,.11f,.14f,.98f):new Color(.035f,.055f,.075f,.01f);
            if(text!=null)text.color=active?new Color(.95f,.82f,.56f):new Color(.52f,.57f,.60f);
        }

        void ConfigurePrimaryAction(PlayerState s)
        {
            if(primaryAction==null||primaryActionText==null||s==null)return;
            primaryAction.interactable=true;
            if(s.BuildingCompletesUtcTicks>0)
            {
                primaryActionText.text="RECONSTRUCCIÓN EN CURSO";
                primaryAction.interactable=false;
                return;
            }
            if(s.RecruitmentCompletesUtcTicks>0)
            {
                primaryActionText.text="ENTRENAMIENTO EN CURSO";
                primaryAction.interactable=false;
                return;
            }
            if(s.March.Phase!="idle")
            {
                primaryActionText.text="MARCHA EN CURSO";
                primaryAction.interactable=false;
                return;
            }

            switch(SliceRules.CurrentObjectiveKey(s))
            {
                case "b1.build-sawmill":
                    primaryActionText.text=city
                        ?(s.Resources.Wood>=SliceRules.SawmillWoodCost?"RECONSTRUIR ASERRADERO":"BUSCAR MADERA")
                        :"VOLVER A VALORIA";
                    break;
                case "b1.gather-wood":
                    primaryActionText.text=city?"SALIR AL BOSQUE":"RECOLECTAR MADERA";
                    break;
                case "b1.gather-stone":
                    primaryActionText.text=city?"SALIR A LA CANTERA":"RECOLECTAR PIEDRA";
                    break;
                case "b1.clear-route":
                    primaryActionText.text=city?"IR A LA FRONTERA":"DESPEJAR LA RUTA";
                    break;
                case "b1.return":
                    primaryActionText.text=city?"RUTA ASEGURADA":"REGRESAR A VALORIA";
                    primaryAction.interactable=!city;
                    break;
                case "b1.ascend":
                    primaryActionText.text=city?"ASCENDER A BASTIÓN II":"REGRESAR A VALORIA";
                    break;
                case "b2.build-barracks":
                    primaryActionText.text=city?"CONSTRUIR CUARTEL":"REGRESAR A VALORIA";
                    break;
                case "b2.train-archers":
                    if(s.Resources.Wood<SliceRules.RecruitWoodCost)
                        primaryActionText.text=city?"BUSCAR MADERA":"RECOLECTAR MADERA";
                    else if(s.Resources.Stone<SliceRules.RecruitStoneCost)
                        primaryActionText.text=city?"BUSCAR PIEDRA":"RECOLECTAR PIEDRA";
                    else
                        primaryActionText.text=city?"ENTRENAR ARQUEROS":"REGRESAR A VALORIA";
                    break;
                case "b2.prepare-march":
                case "b2.raise-expedition-power":
                    primaryActionText.text=city?"PREPARAR MARCHA":"REGRESAR A VALORIA";
                    break;
                case "b2.defeat-engendro":
                    primaryActionText.text=city?"IR CONTRA EL ENGENDRO":"ATACAR AL ENGENDRO";
                    break;
                case "b2.complete":
                    primaryActionText.text=city?"BASTIÓN II ASEGURADO":"REGRESAR A VALORIA";
                    primaryAction.interactable=!city;
                    break;
                default:
                    primaryActionText.text="CONTINUAR";
                    break;
            }
        }

        void InvokePrimaryObjective()
        {
            var s=gateway.Snapshot();
            string key=SliceRules.CurrentObjectiveKey(s);
            if(city)
            {
                if(key=="b1.build-sawmill"&&s.Resources.Wood>=SliceRules.SawmillWoodCost)
                {
                    FocusCityHotspot("Aserradero · target");
                    OpenBuildingPanel("sawmill");
                }
                else if(key=="b1.ascend")
                {
                    FocusCityHotspot("Bastion · target");
                    OpenBuildingPanel("bastion");
                }
                else if(key=="b2.build-barracks"||
                        (key=="b2.train-archers"&&s.Resources.Wood>=SliceRules.RecruitWoodCost&&s.Resources.Stone>=SliceRules.RecruitStoneCost))
                {
                    FocusCityHotspot("Cuartel · target");
                    OpenBuildingPanel("barracks");
                }
                else if(key=="b2.prepare-march"||key=="b2.raise-expedition-power")
                {
                    FocusCityHotspot("Cuartel · target");
                    OpenMarchPanel();
                }
                else SceneManager.LoadScene("Frontier");
                return;
            }

            if(key=="b1.gather-wood")Send("Gather","forest-valoria");
            else if(key=="b1.gather-stone")Send("Gather","quarry-valoria");
            else if(key=="b1.clear-route")Send("Fight","corrupt-scout");
            else if(key=="b2.train-archers")
            {
                if(s.Resources.Wood<SliceRules.RecruitWoodCost)Send("Gather","forest-valoria");
                else if(s.Resources.Stone<SliceRules.RecruitStoneCost)Send("Gather","quarry-valoria");
                else SceneManager.LoadScene("Valoria");
            }
            else if(key=="b2.defeat-engendro")Send("Fight","engendro-valoria");
            else SceneManager.LoadScene("Valoria");
        }
        void CreateBuildingPanel(Transform parent)
        {
            buildingPanel=new GameObject("Building interaction panel",typeof(RectTransform),typeof(Image),typeof(VerticalLayoutGroup));
            var rt=buildingPanel.GetComponent<RectTransform>();rt.SetParent(parent,false);
            rt.anchorMin=new Vector2(0,0);rt.anchorMax=new Vector2(1,0);rt.pivot=new Vector2(.5f,0);
            rt.sizeDelta=new Vector2(-20,206);rt.anchoredPosition=new Vector2(0,78);
            buildingPanel.GetComponent<Image>().color=new Color(.045f,.065f,.085f,.98f);
            var layout=buildingPanel.GetComponent<VerticalLayoutGroup>();layout.padding=new RectOffset(13,13,12,12);
            layout.spacing=6;layout.childControlHeight=true;layout.childForceExpandHeight=false;
            buildingTitle=Label("Building title",buildingPanel.transform,14,new Color(.96f,.88f,.69f),24);
            buildingBody=Label("Building body",buildingPanel.transform,10,new Color(.86f,.89f,.90f),52);
            var actionGo=new GameObject("Building action",typeof(RectTransform),typeof(Image),typeof(Button),typeof(LayoutElement));
            actionGo.transform.SetParent(buildingPanel.transform,false);
            actionGo.GetComponent<Image>().color=new Color(.73f,.61f,.36f,.98f);
            actionGo.GetComponent<LayoutElement>().preferredHeight=46;
            buildingAction=actionGo.GetComponent<Button>();
            var actionText=Label("Text",actionGo.transform,10,new Color(.07f,.09f,.11f),46);
            actionText.text="ACCIÓN";actionText.alignment=TextAnchor.MiddleCenter;
            var ar=actionText.rectTransform;ar.anchorMin=Vector2.zero;ar.anchorMax=Vector2.one;ar.offsetMin=ar.offsetMax=Vector2.zero;
            var closeGo=new GameObject("Cerrar",typeof(RectTransform),typeof(Image),typeof(Button),typeof(LayoutElement));
            closeGo.transform.SetParent(buildingPanel.transform,false);
            closeGo.GetComponent<Image>().color=new Color(.15f,.16f,.16f,.98f);
            closeGo.GetComponent<LayoutElement>().preferredHeight=32;
            closeGo.GetComponent<Button>().onClick.AddListener(()=>buildingPanel.SetActive(false));
            var closeText=Label("Text",closeGo.transform,9,new Color(.78f,.82f,.84f),32);closeText.text="CERRAR";closeText.alignment=TextAnchor.MiddleCenter;
            var cr=closeText.rectTransform;cr.anchorMin=Vector2.zero;cr.anchorMax=Vector2.one;cr.offsetMin=cr.offsetMax=Vector2.zero;
            buildingPanel.SetActive(false);
        }
        void ResetQaFreshSave()
        {
            if(Time.unscaledTime>resetQaArmedUntil)
            {
                resetQaArmedUntil=Time.unscaledTime+6f;
                feedback="Pulsa NUEVA PARTIDA QA otra vez en 6 s para borrar solo el save local de Eldoria.";
                Refresh();
                return;
            }
            resetQaArmedUntil=0f;
            SliceBoot.ResetLocalSaveAndRestart();
        }

        void Zoom(float amount){if(OfficialCamera!=null)OfficialCamera.orthographicSize=Mathf.Clamp(OfficialCamera.orthographicSize+amount,9,19);}
        void UpdateSafeArea()
        {
            lastWidth=Screen.width;lastHeight=Screen.height;
            if(safe==null||lastWidth==0||lastHeight==0)return;
            Rect r=Screen.safeArea;
            safe.anchorMin=new Vector2(r.xMin/lastWidth,r.yMin/lastHeight);
            safe.anchorMax=new Vector2(r.xMax/lastWidth,r.yMax/lastHeight);
            safe.offsetMin=safe.offsetMax=Vector2.zero;
        }
        static RectTransform HorizontalPanel(string name,Transform parent,Color color,float height,bool top)
        {
            var t=new GameObject(name,typeof(RectTransform),typeof(Image),typeof(HorizontalLayoutGroup)).GetComponent<RectTransform>();
            t.SetParent(parent,false);t.anchorMin=new Vector2(0,top?1:0);t.anchorMax=new Vector2(1,top?1:0);
            t.pivot=new Vector2(.5f,top?1:0);t.sizeDelta=new Vector2(0,height);t.anchoredPosition=Vector2.zero;
            t.GetComponent<Image>().color=color;
            var group=t.GetComponent<HorizontalLayoutGroup>();group.padding=new RectOffset(8,8,8,8);
            group.spacing=6;group.childForceExpandHeight=true;group.childForceExpandWidth=true;
            group.childControlHeight=true;group.childControlWidth=true;
            return t;
        }

        static Text ChipLabel(string name,Transform parent,int size,Color textColor,Color background,float width)
        {
            var go=new GameObject(name+" chip",typeof(RectTransform),typeof(Image),typeof(LayoutElement));
            go.transform.SetParent(parent,false);
            go.GetComponent<Image>().color=background;
            var le=go.GetComponent<LayoutElement>();le.preferredWidth=width;le.preferredHeight=44;
            var text=Label(name,go.transform,size,textColor,44);
            text.alignment=TextAnchor.MiddleCenter;
            var rt=text.rectTransform;rt.anchorMin=Vector2.zero;rt.anchorMax=Vector2.one;rt.offsetMin=new Vector2(5,2);rt.offsetMax=new Vector2(-5,-2);
            return text;
        }

        static Text ResourceChip(string name,Transform parent,string icon,string label,float width,
            Color? background=null)
        {
            var text=ChipLabel(name,parent,8,new Color(.95f,.94f,.90f),
                background??new Color(.025f,.04f,.055f,.92f),width);
            text.text=icon+"  "+label;
            text.alignment=TextAnchor.MiddleCenter;
            text.lineSpacing=.84f;
            return text;
        }

        static Button NavButton(Transform parent,string icon,string label,Action onClick)
        {
            var button=Button(parent,label,onClick);
            var element=button.GetComponent<LayoutElement>();
            element.minHeight=56;element.preferredHeight=56;
            var text=button.GetComponentInChildren<Text>();
            if(text!=null)
            {
                text.text=icon+"\n"+label;
                text.fontSize=8;text.lineSpacing=.78f;text.alignment=TextAnchor.MiddleCenter;
            }
            return button;
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
        static Button Button(Transform parent,string label,Action onClick)
        {
            var go=new GameObject(label,typeof(RectTransform),typeof(Image),typeof(Button),typeof(LayoutElement));
            go.transform.SetParent(parent,false);
            go.GetComponent<Image>().color=new Color(.19f,.16f,.12f,.98f);
            go.GetComponent<LayoutElement>().minHeight=28;
            var button=go.GetComponent<Button>();
            button.onClick.AddListener(()=>onClick());
            var text=Label("Text",go.transform,10,new Color(.98f,.86f,.64f),28);
            text.text=label;text.alignment=TextAnchor.MiddleCenter;
            var rect=text.rectTransform;rect.anchorMin=Vector2.zero;rect.anchorMax=Vector2.one;
            rect.offsetMin=rect.offsetMax=Vector2.zero;
            return button;
        }
    }
}
