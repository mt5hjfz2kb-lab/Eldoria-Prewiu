using System;
using System.Collections;
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
        ValoriaParcelPresentation productionParcels;
        RectTransform safe;
        Text heading, resources, stoneResource, power, objective, description, message, buildingTitle, buildingBody, primaryActionText, sawmillLevelBadge, barracksLevelBadge, bastionLevelBadge, resetButtonText;
        GameObject buildingPanel, objectiveDock, cityAmbientLayer, constructionActivityFx;
        string currentBuildingId="";
        RectTransform[] ambientMotes=Array.Empty<RectTransform>();
        Vector2[] ambientMoteOrigins=Array.Empty<Vector2>();
        Button buildingAction, primaryAction, homeButton, cityNavButton, worldNavButton, resetButton;
        string feedback="";
        float refreshAt;
        float resetQaArmedUntil;
        bool ownerResetArmed;
        int ownerResetExecutionSerial;
        int lastWidth,lastHeight;
        bool city;
        int renderedSawmill, renderedBarracks, renderedBastion;
        bool renderedScout, renderedEngendro, renderedIdle;
        Vector2 pointerStart,pointerLast;
        bool pointerActive,pointerDragged,pointerStartedOverUi;
        bool pinchActive;
        float lastPinchDistance;
        Vector3 cameraHome;
        float cameraHomeOrthographicSize;
        float panHalfX=5f,panHalfZ=4f;
        const float PanGestureThreshold=12f;
        public const float MinOrthographicZoom=9f;
        public const float MaxOrthographicZoom=19f;
        const float PinchZoomSensitivity=1f;
        Camera OfficialCamera => productionParcels!=null&&productionParcels.ProductionCamera!=null
            ? productionParcels.ProductionCamera
            : GameObject.Find("Isometric camera")?.GetComponent<Camera>() ?? Camera.main;
        static string CitySceneName
        {
            get
            {
#if UNITY_WEBGL && !UNITY_EDITOR
                return "ValoriaWebGL";
#else
                return "Valoria";
#endif
            }
        }
        public void Initialize(ICommandGateway commands){gateway=commands;}
        public void OnSceneLoaded(Scene scene,LoadSceneMode mode)
        {
            if(scene.name=="Bootstrap")return;
            city=scene.name!="Frontier";
            var state=gateway.Snapshot();
            renderedSawmill=state.SawmillLevel;renderedBarracks=state.BarracksLevel;renderedBastion=state.BastionLevel;
            renderedScout=state.ScoutDefeated;renderedEngendro=state.EngendroDefeated;
            renderedIdle=state.March.Phase=="idle";
            productionParcels=null;
            foreach(var sceneRoot in scene.GetRootGameObjects())
            { productionParcels=sceneRoot.GetComponentInChildren<ValoriaParcelPresentation>(true);if(productionParcels!=null)break; }
            if(productionParcels!=null) productionParcels.Apply(state);
            else if(city) VisualWorld.Create(true,state);
            if(productionParcels!=null) productionParcels.Home();
            else WorldRegion1Runtime.Create(state);
            if(OfficialCamera!=null&&productionParcels==null)
            {
                if(city)
                {
                    float aspect=Screen.height>0?Screen.width/(float)Screen.height:OfficialCamera.aspect;
                    ValoriaMobileNavigableCityV1.ApplyHomePose(OfficialCamera,aspect);
                    ConfigureCityPanBounds(state.BastionLevel);
                }
                else
                {
                    panHalfX=10f;
                    panHalfZ=8f;
                }
                cameraHome=OfficialCamera.transform.position;
                cameraHomeOrthographicSize=OfficialCamera.orthographicSize;
            }
            CreateHud();Refresh();
            LogPlayableState("scene-loaded");
#if UNITY_WEBGL && !UNITY_EDITOR
            if(!city) StartCoroutine(LogWorldHotspotNextFrame("World Region 1 · forest target","forest-valoria"));
#endif
        }
        void Update()
        {
            if(gateway==null || safe==null)return;
            if(Time.unscaledTime>refreshAt)
            {
                refreshAt=Time.unscaledTime+.22f;
                if(gateway.Advance())
                {
                    Refresh();
                    LogPlayableState("advance");
                }
                else RefreshClock();
            }
            if(lastWidth!=Screen.width||lastHeight!=Screen.height)
            {
                UpdateSafeArea();
#if UNITY_WEBGL && !UNITY_EDITOR
                StartCoroutine(LogPlayableUiGeometryNextFrame());
#endif
            }
            HandlePointerInput();
            AnimateCityAmbientation();
            UpdateBuildingLevelBadgePositions();
            if(buildingPanel!=null&&buildingPanel.activeInHierarchy&&city) PositionBuildingPanel(currentBuildingId);
        }
        void HandlePointerInput()
        {
            var camera=OfficialCamera;
            if(camera==null)return;
            var mouse=Mouse.current;
            var touch=Touchscreen.current;

            if(HandlePinchZoom(touch,camera))return;

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
                pointerStartedOverUi=IsPointerOverInteractiveUi(point);
            }

            if(pointerActive&&held)
            {
                if(IsPanGesture(pointerStart,point))pointerDragged=true;
                if(pointerDragged&&!pointerStartedOverUi)
                    PanCameraByScreenDelta(point-pointerLast);
                pointerLast=point;
            }

            if(pointerActive&&released)
            {
                bool shouldSelect=!pointerStartedOverUi&&!pointerDragged;
#if UNITY_WEBGL && !UNITY_EDITOR
                if(usingTouch&&!pointerDragged)
                {
                    TryScheduleWebResetFallback(point);
                    TryScheduleWebBottomNavFallback(point);
                }
#endif
                pointerActive=false;
                if(shouldSelect)
                {
                    var spot=ResolveHotspot(point);
                    if(spot!=null)Select(spot.Id);
                }
            }
        }

#if UNITY_WEBGL && !UNITY_EDITOR
        void TryScheduleWebResetFallback(Vector2 point)
        {
            if(resetButton==null||!resetButton.gameObject.activeInHierarchy)return;
            var rect=resetButton.GetComponent<RectTransform>();
            if(rect==null||!RectTransformUtility.RectangleContainsScreenPoint(rect,point,null))return;
            StartCoroutine(WebResetFallbackAfterUi(ownerResetArmed,ownerResetExecutionSerial));
        }

        IEnumerator WebResetFallbackAfterUi(bool armedAtRelease,int executionSerialAtRelease)
        {
            // Unity UI remains the primary input path. This fallback only recovers the
            // occasional WebGL/touch release that is visible over the reset Button but
            // never reaches Button.onClick.
            yield return null;
            if(ownerResetExecutionSerial!=executionSerialAtRelease)yield break;
            if(ownerResetArmed!=armedAtRelease)yield break;
            Debug.Log("ELDORIA_PLAYABLE_RESET fallback=True armed="+ownerResetArmed);
            InvokeOwnerReset();
        }

        void TryScheduleWebBottomNavFallback(Vector2 point)
        {
            var area=Screen.safeArea;
            bool landscape=area.width>area.height*1.08f;
            // ReferenceUiArtPass deliberately moves the visible landscape controls:
            // MUNDO is the far-left medallion and BASTIÓN/REINO is the far-right medallion.
            // Keep this WebGL fallback aligned with what the player actually sees. It exists
            // only to recover a touch that the overlay/raycast stack fails to deliver to the
            // underlying Unity Button; normal Button navigation remains the primary path.
            float navHeight=Mathf.Max(28f,area.height*(landscape?.18f:(68f/844f)));
            if(point.y<area.yMin||point.y>area.yMin+navHeight)return;
            float nx=Mathf.Clamp01((point.x-area.xMin)/Mathf.Max(1f,area.width));
            // The controlled vertical-slice migration exposes only two real navigation
            // surfaces: CIUDAD/REINO on the left and MUNDO on the right.
            if(nx<.5f)
            {
                if(!city) StartCoroutine(WebNavFallbackAfterUi("Valoria",false));
            }
            else
            {
                if(city) StartCoroutine(WebNavFallbackAfterUi("Frontier",true));
            }
        }

        IEnumerator WebNavFallbackAfterUi(string target,bool expectedCity)
        {
            string before=SceneManager.GetActiveScene().name;
            yield return null;
            if(SceneManager.GetActiveScene().name!=before)yield break;
            Debug.Log("ELDORIA_PLAYABLE_NAV target="+target+" city="+expectedCity+" source=webgl-touch-fallback");
            SceneManager.LoadScene(target=="Valoria"?CitySceneName:target);
        }
#endif

        bool IsPointerOverInteractiveUi(Vector2 point)
        {
            if(EventSystem.current==null)return false;
            var data=new PointerEventData(EventSystem.current){position=point};
            var hits=new System.Collections.Generic.List<RaycastResult>();
            EventSystem.current.RaycastAll(data,hits);
            foreach(var hit in hits)
            {
                if(hit.gameObject==null)continue;
                if(hit.gameObject.GetComponentInParent<Selectable>()!=null)return true;
                if(buildingPanel!=null&&buildingPanel.activeInHierarchy&&
                    hit.gameObject.transform.IsChildOf(buildingPanel.transform))return true;
            }
            return false;
        }

        bool HandlePinchZoom(Touchscreen touch,Camera camera)
        {
            if(touch==null)return EndPinchIfNeeded();
            var first=touch.touches[0];
            var second=touch.touches[1];
            bool twoTouches=first.press.isPressed&&second.press.isPressed;
            if(!twoTouches)return EndPinchIfNeeded();

            var firstPosition=first.position.ReadValue();
            var secondPosition=second.position.ReadValue();
            float distance=Vector2.Distance(firstPosition,secondPosition);
            if(distance<=0f)return true;

            // A two-finger gesture exclusively owns this frame: it may never fall through
            // to one-finger pan/tap selection.
            pointerActive=false;
            pointerDragged=true;

            if(!pinchActive)
            {
                pinchActive=true;
                lastPinchDistance=distance;
                return true;
            }

            if(productionParcels!=null)
            { productionParcels.Zoom(-(distance-lastPinchDistance)*.025f);lastPinchDistance=distance;return true; }
            camera.orthographicSize=CalculatePinchZoom(
                camera.orthographicSize,lastPinchDistance,distance,Mathf.Max(1f,camera.pixelHeight));
            lastPinchDistance=distance;
            return true;
        }

        bool EndPinchIfNeeded()
        {
            if(!pinchActive)return false;
            pinchActive=false;
            lastPinchDistance=0f;
            // Consume the first frame after the second finger leaves so lifting a pinch
            // cannot become a building tap or start a one-finger pan accidentally.
            pointerActive=false;
            pointerDragged=true;
            return true;
        }

        public static float CalculatePinchZoom(float currentSize,float previousDistance,float currentDistance,float pixelHeight)
        {
            if(previousDistance<=0f||currentDistance<=0f||pixelHeight<=0f)
                return Mathf.Clamp(currentSize,MinOrthographicZoom,MaxOrthographicZoom);
            float normalizedDelta=(currentDistance-previousDistance)/pixelHeight;
            float next=currentSize-normalizedDelta*MaxOrthographicZoom*PinchZoomSensitivity;
            return Mathf.Clamp(next,MinOrthographicZoom,MaxOrthographicZoom);
        }

        static bool IsPanGesture(Vector2 start,Vector2 current)
            => (current-start).sqrMagnitude>=PanGestureThreshold*PanGestureThreshold;

        void ConfigureCityPanBounds(int bastionLevel)
        {
            var camera=OfficialCamera;
            float aspect=camera!=null&&camera.aspect>0f?camera.aspect:(Screen.height>0?Screen.width/(float)Screen.height:.5625f);
            var half=ValoriaMobileNavigableCityV1.PanHalfExtents(bastionLevel,aspect);
            panHalfX=half.x;panHalfZ=half.y;
        }

        void PanCameraByScreenDelta(Vector2 screenDelta)
        {
            var camera=OfficialCamera;
            if(camera==null)return;
            if(city&&productionParcels!=null)
            {
                productionParcels.Pan(screenDelta);
#if UNITY_WEBGL && !UNITY_EDITOR
                var pc = productionParcels.ProductionCamera;
                if(pc!=null) Debug.Log("ELDORIA_PLAYABLE_TOUCH_PAN x=" + pc.transform.position.x.ToString("F3") +
                    " y=" + pc.transform.position.y.ToString("F3") +
                    " z=" + pc.transform.position.z.ToString("F3") +
                    " dx=" + screenDelta.x.ToString("F1") + " dy=" + screenDelta.y.ToString("F1"));
#endif
                return;
            }
            float worldPerPixel=(camera.orthographicSize*2f)/Mathf.Max(1f,camera.pixelHeight);
            var right=Vector3.ProjectOnPlane(camera.transform.right,Vector3.up).normalized;
            var up=Vector3.ProjectOnPlane(camera.transform.up,Vector3.up).normalized;
            if(up.sqrMagnitude<.001f)up=Vector3.forward;
            var desired=camera.transform.position+(-right*screenDelta.x-up*screenDelta.y)*worldPerPixel;
            if(city)
            {
                float aspect=camera.aspect>0f?camera.aspect:(Screen.height>0?Screen.width/(float)Screen.height:.5625f);
                camera.transform.position=ValoriaMobileNavigableCityV1.ClampToEnvelope(cameraHome,desired,gateway.Snapshot().BastionLevel,aspect);
            }
            else
            {
                var offset=desired-cameraHome;
                offset.x=Mathf.Clamp(offset.x,-panHalfX,panHalfX);
                offset.z=Mathf.Clamp(offset.z,-panHalfZ,panHalfZ);
                camera.transform.position=new Vector3(cameraHome.x+offset.x,cameraHome.y,cameraHome.z+offset.z);
            }
        }

        void RecenterCamera()
        {
            if(OfficialCamera==null)return;
            if(city&&productionParcels!=null)
            {
                productionParcels.Home();
#if UNITY_WEBGL && !UNITY_EDITOR
                Debug.Log("ELDORIA_PLAYABLE_HOME production=1");
#endif
                return;
            }
            OfficialCamera.transform.position=cameraHome;
            OfficialCamera.orthographicSize=cameraHomeOrthographicSize;
#if UNITY_WEBGL && !UNITY_EDITOR
            Debug.Log("ELDORIA_PLAYABLE_HOME production=0");
#endif
        }

        void FocusCityHotspot(string objectName)
        {
            var camera=OfficialCamera;
            var target=GameObject.Find(objectName);
            if(!city||camera==null||target==null)return;
            if(productionParcels!=null)
            {
                productionParcels.Home();
#if UNITY_WEBGL && !UNITY_EDITOR
                var frameLoader=productionParcels.GetComponent<ValoriaWebGLSplatStateLoader>();
                var targetCollider=target.GetComponent<Collider>();
                if(frameLoader!=null)frameLoader.FocusPresentedPoint(targetCollider!=null?targetCollider.bounds.center:target.transform.position);
#endif
                return;
            }
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
#if UNITY_WEBGL && !UNITY_EDITOR
            if(city&&productionParcels!=null)
            {
                var frameLoader=productionParcels.GetComponent<ValoriaWebGLSplatStateLoader>();
                if(frameLoader!=null&&!frameLoader.TryPresentedRay(point,out ray))return null;
            }
#endif
            float reach=city&&productionParcels!=null?Mathf.Max(100f,camera.farClipPlane):100f;
            var hits=Physics.RaycastAll(ray,reach);
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
                    if(spot==null)continue;
                    bool buildingTarget=productionParcels!=null
                        ? spot.gameObject.name==InteractiveTargetName(spot.Id)
                        : spot.gameObject.name.EndsWith("· target",System.StringComparison.Ordinal);
                    if(!buildingTarget)continue;
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
            else if(id=="valoria-map-city")SceneManager.LoadScene(CitySceneName);
            else if(!city&&(id=="forest-valoria"||id=="quarry-valoria"||id=="corrupt-scout"||id=="engendro-valoria"||id=="old-watch-ruin"))
                OpenWorldPanel(id);
            else if(id=="forest-valoria"||id=="quarry-valoria")Send("Gather",id);
            else if(id=="corrupt-scout"||id=="engendro-valoria")Send("Fight",id);
            else if(id=="sawmill"||id=="barracks"||id=="bastion")OpenBuildingPanel(id);
        }

        void SetBuildingPanelOpen(bool open)
        {
            if(buildingPanel!=null)buildingPanel.SetActive(open);
            if(objectiveDock!=null)objectiveDock.SetActive(!open);
        }

        void CloseBuildingPanel()
        {
            SetBuildingPanelOpen(false);
        }

        void OpenWorldPanel(string id)
        {
            if(buildingPanel==null)return;
            var s=gateway.Snapshot();
            SetBuildingPanelOpen(true);
            buildingAction.onClick.RemoveAllListeners();
#if UNITY_WEBGL && !UNITY_EDITOR
            Canvas.ForceUpdateCanvases();
            LogPlayableButtonCenter("buildingAction",buildingAction);
#endif
            buildingAction.interactable=true;

            if(id=="forest-valoria")
            {
                buildingTitle.text="BOSQUE DE VALORIA";
                buildingBody.text="Nodo de madera · "+s.ForestRemaining+" disponibles.\nEnvía una Marcha desde Valoria y la recompensa se acredita al regresar.";
                buildingAction.GetComponentInChildren<Text>().text="ENVIAR MARCHA";
                buildingAction.interactable=s.ForestRemaining>0&&s.March.Phase=="idle";
                if(buildingAction.interactable)buildingAction.onClick.AddListener(()=>{CloseBuildingPanel();Send("Gather",id);});
            }
            else if(id=="quarry-valoria")
            {
                buildingTitle.text="CANTERA";
                buildingBody.text="Nodo de piedra · "+s.QuarryRemaining+" disponibles.\nLa cantera demuestra la segunda familia económica del mapa 4X.";
                buildingAction.GetComponentInChildren<Text>().text="ENVIAR MARCHA";
                buildingAction.interactable=s.QuarryRemaining>0&&s.March.Phase=="idle";
                if(buildingAction.interactable)buildingAction.onClick.AddListener(()=>{CloseBuildingPanel();Send("Gather",id);});
            }
            else if(id=="old-watch-ruin")
            {
                buildingTitle.text="ANTIGUA ATALAYA";
                buildingBody.text="Ruina neutral. Desde aquí se vigilaba la ruta antes de la Brecha.\nEs un POI narrativo: inspeccionarlo no concede recursos ni altera el combate.";
                buildingAction.GetComponentInChildren<Text>().text="INSPECCIONADO";
                buildingAction.interactable=false;
            }
            else
            {
                bool engendro=id=="engendro-valoria";
                buildingTitle.text=engendro?"ENGENDRO DE LA FISURA":"EXPLORADOR CORRUPTO";
                buildingBody.text=engendro
                    ?"Amenaza de Bastión II. Requiere la Marcha preparada en Valoria."
                    :"Primera amenaza PvE de la Región I. La victoria despeja la ruta y la recompensa vuelve con la Marcha.";
                buildingAction.GetComponentInChildren<Text>().text="ATACAR";
                buildingAction.interactable=s.March.Phase=="idle"&&!(engendro?s.EngendroDefeated:s.ScoutDefeated);
                if(buildingAction.interactable)buildingAction.onClick.AddListener(()=>{CloseBuildingPanel();Send("Fight",id);});
            }
#if UNITY_WEBGL && !UNITY_EDITOR
            StartCoroutine(LogPlayableButtonRectNextFrame(buildingAction,"WORLD_ACTION"));
#endif
        }
        void OpenBuildingPanel(string id)
        {
            if(buildingPanel==null)return;
            var s=gateway.Snapshot();
            currentBuildingId=id;
            SetBuildingPanelOpen(true);
            PositionBuildingPanel(id);
            buildingAction.onClick.RemoveAllListeners();
            if(id=="sawmill")
            {
                buildingTitle.text="ASERRADERO";
                buildingBody.text=s.SawmillLevel>0
                    ?"Edificio económico activo · produce y sostiene la reconstrucción de Valoria."
                    :"Parcela económica dañada · requiere "+SliceRules.SawmillWoodCost+" madera para reconstruirse.";
                buildingAction.GetComponentInChildren<Text>().text=s.SawmillLevel>0?"ASERRADERO ACTIVO":"RECONSTRUIR";
                buildingAction.interactable=ParcelBuildingStates.For(s,"sawmill")==ParcelBuildingState.AVAILABLE&&s.BuildingCompletesUtcTicks==0;
                if(ParcelBuildingStates.For(s,"sawmill")==ParcelBuildingState.UNDER_CONSTRUCTION)
                { buildingBody.text="Reconstrucción en curso · el aserradero aparecerá al terminar la obra.";buildingAction.GetComponentInChildren<Text>().text="EN CONSTRUCCIÓN"; }
                if(s.SawmillLevel==0)buildingAction.onClick.AddListener(()=>{CloseBuildingPanel();Send("Build","sawmill");});
            }
            else if(id=="barracks")
            {
                buildingTitle.text="CUARTEL";
                if(s.BarracksLevel==0)
                {
                    buildingBody.text="Construir Cuartel · "+SliceRules.BarracksWoodCost+" madera / "+SliceRules.BarracksStoneCost+" piedra.";
                    buildingAction.GetComponentInChildren<Text>().text="CONSTRUIR CUARTEL";
                    buildingAction.interactable=ParcelBuildingStates.For(s,"barracks")==ParcelBuildingState.AVAILABLE&&s.BuildingCompletesUtcTicks==0;
                    if(ParcelBuildingStates.For(s,"barracks")==ParcelBuildingState.UNDER_CONSTRUCTION)
                    { buildingBody.text="Construcción del cuartel en curso · la parcela militar se activará al terminar.";buildingAction.GetComponentInChildren<Text>().text="EN CONSTRUCCIÓN"; }
                    else if(s.BastionLevel<2)buildingBody.text="Parcela militar reservada · disponible al alcanzar Bastión II.";
                    buildingAction.onClick.AddListener(()=>{CloseBuildingPanel();Send("Build","barracks");});
                }
                else if((s.ChapterProgress?.TrainedArchers??0)<SliceContentProfiles.Active.Chapter2TrainArchers)
                {
                    buildingBody.text="Guarnición activa · "+s.Available.Total+" arqueros disponibles · "+
                        (s.ChapterProgress?.TrainedArchers??0)+"/"+SliceContentProfiles.Active.Chapter2TrainArchers+" entrenados en este capítulo. "+
                        "Entrenar "+SliceRules.RecruitArchers+" arqueros cuesta "+SliceRules.RecruitWoodCost+" madera / "+SliceRules.RecruitStoneCost+" piedra.";
                    buildingAction.GetComponentInChildren<Text>().text="RECLUTAR +"+SliceRules.RecruitArchers;
                    buildingAction.interactable=s.RecruitmentCompletesUtcTicks==0;
                    if(buildingAction.interactable)
                        buildingAction.onClick.AddListener(()=>{CloseBuildingPanel();Send("Recruit","archer:t1");});
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
                if(s.BastionLevel>=2)
                {
                    buildingBody.text="Bastión II consolidado · Valoria ha crecido. Desbloqueos: Cuartel, Capítulo II, preparación de Marcha y amenaza Engendro.";
                    buildingAction.GetComponentInChildren<Text>().text="BASTIÓN II ACTIVO";
                    buildingAction.interactable=false;
                }
                else
                {
                    var next=BastionProgressionCatalog.ForLevel(2);
                    var missing=BastionProgressionCatalog.MissingSummary(s,next);
                    buildingBody.text="Núcleo de Valoria · nivel I.\nRequisitos para Bastión II: objetivos de Bastión I completos · "+
                        next.WoodCost+" madera · "+next.StoneCost+" piedra.\n"+
                        (string.IsNullOrEmpty(missing)?"LISTO PARA ASCENDER":"Falta: "+missing)+
                        "\nDesbloquea: Cuartel · Capítulo II · preparación de Marcha.";
                    buildingAction.GetComponentInChildren<Text>().text="ASCENDER A BASTIÓN II";
                    buildingAction.interactable=BastionProgressionCatalog.RequirementsMet(s,next);
                    if(buildingAction.interactable)buildingAction.onClick.AddListener(()=>{CloseBuildingPanel();Send("AdvanceBastion","bastion");});
                }
            }
            RefreshBuildingPanelClock(s);
#if UNITY_WEBGL && !UNITY_EDITOR
            StartCoroutine(LogPlayableButtonCenterNextFrame("buildingAction",buildingAction));
            StartCoroutine(LogPlayableButtonCenterNextFrame("buildingClose",buildingPanel.transform.Find("Cerrar")?.GetComponent<Button>()));
            PublishPresentedCityGeometry();
#endif
        }

        Vector3 PresentedScreenPoint(Camera camera,Vector3 world)
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            if(city&&productionParcels!=null)
            {
                var frameLoader=productionParcels.GetComponent<ValoriaWebGLSplatStateLoader>();
                if(frameLoader!=null)return frameLoader.TryProjectPresentedPoint(world,out var point)?point:Vector3.zero;
            }
#endif
            return camera.WorldToScreenPoint(world);
        }
        void PositionBuildingPanel(string id)
        {
            if(!city||buildingPanel==null||safe==null||string.IsNullOrEmpty(id))return;
            string objectName=InteractiveTargetName(id);
            if(objectName==null)return;
            var target=GameObject.Find(objectName);var camera=OfficialCamera;
            if(target==null||camera==null)return;
            var collider=target.GetComponent<Collider>();
            var world=collider!=null?collider.bounds.center:target.transform.position;
            var screen=PresentedScreenPoint(camera,world);
            if(screen.z<=0)return;
            if(!RectTransformUtility.ScreenPointToLocalPointInRectangle(safe,screen,null,out var local))return;
            var rt=buildingPanel.GetComponent<RectTransform>();
            float halfW=Mathf.Max(1f,safe.rect.width*.5f), halfH=Mathf.Max(1f,safe.rect.height*.5f);
            local.x=Mathf.Clamp(local.x,-halfW+rt.rect.width*.5f+8f,halfW-rt.rect.width*.5f-8f);
            // Place the CTA below the selected building while keeping it above the nav/dock.
            float bottomReserve=Screen.width>Screen.height?136f:160f;
            float minY=-halfH+bottomReserve+rt.rect.height*.5f;
            float maxY=halfH-64f-rt.rect.height*.5f;
            local.y=Mathf.Clamp(local.y-80f,Mathf.Min(minY,maxY),Mathf.Max(minY,maxY));
            rt.anchoredPosition=local;

            // Keep the spatial CTA associated with the building without allowing it to
            // collide with the persistent quest card. This is especially important in
            // landscape, where the canonical sawmill target sits beneath the upper-left HUD.
            var quest=safe.Find("Quest panel") as RectTransform;
            if(quest!=null&&quest.gameObject.activeInHierarchy)
            {
                Canvas.ForceUpdateCanvases();
                var panelCorners=new Vector3[4];var questCorners=new Vector3[4];
                rt.GetWorldCorners(panelCorners);quest.GetWorldCorners(questCorners);
                var panelRect=new Rect(panelCorners[0].x,panelCorners[0].y,panelCorners[2].x-panelCorners[0].x,panelCorners[2].y-panelCorners[0].y);
                var questRect=new Rect(questCorners[0].x,questCorners[0].y,questCorners[2].x-questCorners[0].x,questCorners[2].y-questCorners[0].y);
                if(panelRect.Overlaps(questRect))
                {
                    float overlap=panelRect.yMax-questRect.yMin;
                    rt.anchoredPosition+=new Vector2(0,-overlap-10f);
                }
            }
        }

        void RefreshBuildingPanelClock(PlayerState s)
        {
            if(buildingPanel==null||!buildingPanel.activeInHierarchy||string.IsNullOrEmpty(currentBuildingId))return;
            int seconds=Math.Max(0,(int)Math.Ceiling((s.BuildingCompletesUtcTicks-DateTime.UtcNow.Ticks)/(double)TimeSpan.TicksPerSecond));
            if(s.BuildingCompletesUtcTicks>0&&(currentBuildingId=="sawmill"||currentBuildingId=="barracks"))
            {
                string label=currentBuildingId=="sawmill"?"ASERRADERO":"CUARTEL";
                buildingBody.text=label+" · obra en curso\nTiempo restante: "+seconds+" s";
                buildingAction.GetComponentInChildren<Text>().text="CONSTRUYENDO · "+seconds+" s";
                buildingAction.interactable=false;
            }
        }

        void OpenMarchPanel()
        {
            if(buildingPanel==null)return;
            var s=gateway.Snapshot();
            var prepared=SliceRules.Expedition(s.Available,"aldric");
            currentBuildingId="barracks";
            SetBuildingPanelOpen(true);
            PositionBuildingPanel("barracks");
            buildingAction.onClick.RemoveAllListeners();
            buildingTitle.text="PREPARAR MARCHA";
            buildingBody.text="Sir Aldric · "+s.Available.Total+" Arqueros disponibles\n"+
                "ATQ "+prepared.Attack+" · DEF "+prepared.Defense+" · VIDA "+prepared.Health+
                " · RUP "+prepared.Break+"\nPoder de expedición "+prepared.Power+
                "\nConfirma esta composición antes de atacar al Engendro.";
            buildingAction.GetComponentInChildren<Text>().text="CONFIRMAR MARCHA";
            buildingAction.interactable=s.March.Phase=="idle"&&s.Available.Total>0;
            if(buildingAction.interactable)
                buildingAction.onClick.AddListener(()=>{CloseBuildingPanel();Send("ConfigureMarch","march-main");});
#if UNITY_WEBGL && !UNITY_EDITOR
            StartCoroutine(LogPlayableButtonCenterNextFrame("buildingAction",buildingAction));
#endif
        }
        void Send(string kind,string target)
        {
            var s=gateway.Snapshot();
            int powerBefore=SliceRules.TotalPower(s).Total;
            var result=gateway.Execute(new GameCommand(Guid.NewGuid().ToString("N"),s.PlayerId,kind,target,s.Revision));
            feedback=result.Message;
            if(result.Ok&&kind=="AdvanceBastion")
            {
                var after=gateway.Snapshot();
                int gain=Math.Max(0,SliceRules.TotalPower(after).Total-powerBefore);
                feedback="BASTIÓN II · MI REINO HA CRECIDO · +"+gain+" PODER · Cuartel y Capítulo II desbloqueados";
            }
#if UNITY_WEBGL && !UNITY_EDITOR
            Debug.Log("ELDORIA_PLAYABLE_COMMAND kind="+kind+" target="+target+" ok="+result.Ok+" revision="+result.Revision);
#endif
            Refresh();
            LogPlayableState("command-"+kind+"-"+target);
        }

        void LogPlayableState(string tag)
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            if(gateway==null)return;
            var state=gateway.Snapshot();
            Debug.Log("ELDORIA_PLAYABLE_STATE tag="+tag+
                " scene="+SceneManager.GetActiveScene().name+
                " revision="+state.Revision+
                " wood="+state.Resources.Wood+
                " stone="+state.Resources.Stone+
                " gatheredWood="+(state.ChapterProgress?.GatheredWood??0)+
                " gatheredStone="+(state.ChapterProgress?.GatheredStone??0)+
                " march="+state.March.Phase+
                " sawmill="+state.SawmillLevel+
                " bastion="+state.BastionLevel+
                " barracks="+state.BarracksLevel+
                " archers="+(state.Available?.ArcherT1??0)+
                " trained="+(state.ChapterProgress?.TrainedArchers??0)+
                " configured="+state.MarchConfigured+
                " scout="+state.ScoutDefeated+
                " engendro="+state.EngendroDefeated+
                " objective="+SliceRules.CurrentObjectiveKey(state));
#endif
        }
        void Refresh()
        {
            if(heading==null)return;
            var s=gateway.Snapshot();
            UpdateBuildingLevelBadges(s);
            if(productionParcels!=null) productionParcels.Apply(s);
            if(!city)WorldRegion1Runtime.Refresh(s);
            if(city&&productionParcels==null&&(s.SawmillLevel!=renderedSawmill||s.BarracksLevel!=renderedBarracks||s.BastionLevel!=renderedBastion||
                s.ScoutDefeated!=renderedScout||s.EngendroDefeated!=renderedEngendro||
                (s.March.Phase=="idle")!=renderedIdle))
            { feedback="";SceneManager.LoadScene(SceneManager.GetActiveScene().name);return; }
            var parts=SliceRules.TotalPower(s);
            heading.text=city?"VALORIA\nBastión "+s.BastionLevel:"MUNDO\nRegión I";
            resources.text="MADERA\n"+s.Resources.Wood;
            if(stoneResource!=null)stoneResource.text="PIEDRA\n"+s.Resources.Stone;
            var marchPreview=s.March.Phase!="idle"?s.March.Troops:
                (s.MarchConfigured?s.PreparedTroops:s.Available);
            power.text="PODER\n"+parts.Total;
            var marchPower=SliceRules.Expedition(
                marchPreview,s.March.Phase!="idle"?s.March.HeroId:(s.MarchConfigured?s.PreparedHeroId:"aldric")).Power;
            var cp=s.ChapterProgress??new ChapterProgressState();
            objective.text=ObjectiveText(s,cp);
            ConfigurePrimaryAction(s);
#if UNITY_WEBGL && !UNITY_EDITOR
            StartCoroutine(LogPlayableButtonCenterNextFrame("primary",primaryAction));
#endif
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
                 city?"Pulsa MUNDO para abrir el mapa 4X; vuelve con recursos para construir.":
                 "Selecciona una ciudad, recurso, ruina o amenaza para actuar en la Región I."):feedback;
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
                case "b1.ascend": return ResourceNeed(s,SliceRules.Bastion2WoodCost,SliceRules.Bastion2StoneCost)!=null
                    ? "CAPÍTULO I · reúne recursos para ascender · M "+s.Resources.Wood+"/"+SliceRules.Bastion2WoodCost+" · P "+s.Resources.Stone+"/"+SliceRules.Bastion2StoneCost
                    : "CAPÍTULO I COMPLETO · asciende el Bastión";
                case "b2.build-barracks": return ResourceNeed(s,SliceRules.BarracksWoodCost,SliceRules.BarracksStoneCost)!=null
                    ? "BASTIÓN II · reúne recursos para el Cuartel · M "+s.Resources.Wood+"/"+SliceRules.BarracksWoodCost+" · P "+s.Resources.Stone+"/"+SliceRules.BarracksStoneCost
                    : "BASTIÓN II · levanta el Cuartel";
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
            RefreshBuildingPanelClock(s);
            if(constructionActivityFx!=null)constructionActivityFx.SetActive(city&&s.BuildingCompletesUtcTicks>0);
        }
        void CreateHud()
        {
            if(EventSystem.current==null)
                new GameObject("UI events",typeof(EventSystem),typeof(InputSystemUIInputModule));

            var canvasGo=new GameObject("Eldoria HUD",typeof(RectTransform),typeof(Canvas),typeof(CanvasScaler),typeof(GraphicRaycaster));
            var canvas=canvasGo.GetComponent<Canvas>();canvas.renderMode=RenderMode.ScreenSpaceOverlay;canvas.sortingOrder=100;
            var scaler=canvasGo.GetComponent<CanvasScaler>();scaler.uiScaleMode=CanvasScaler.ScaleMode.ScaleWithScreenSize;
            bool landscapeHud=Screen.width>Screen.height;
            scaler.referenceResolution=landscapeHud?new Vector2(844,390):new Vector2(390,844);
            scaler.screenMatchMode=CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight=.5f;
            safe=new GameObject("Safe area",typeof(RectTransform)).GetComponent<RectTransform>();safe.SetParent(canvasGo.transform,false);
            UpdateSafeArea();
            CreateCityAmbientation(safe);

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
            {
                homeButton=Button(top,"CENTRAR",RecenterCamera);
                var homeLayout=homeButton.GetComponent<LayoutElement>();
                homeLayout.minWidth=50;homeLayout.preferredWidth=50;homeLayout.minHeight=34;
                homeButton.gameObject.name=city?"City home / recenter":"World home / recenter";
            }

            resetButton=Button(safe,"REINICIAR",InvokeOwnerReset);
            resetButton.gameObject.name="Owner reset";
            var resetRt=resetButton.GetComponent<RectTransform>();
            resetRt.anchorMin=resetRt.anchorMax=new Vector2(1,1);resetRt.pivot=new Vector2(1,1);
            resetRt.sizeDelta=new Vector2(78,34);resetRt.anchoredPosition=new Vector2(-10,-78);
            resetButton.GetComponent<LayoutElement>().ignoreLayout=true;
            resetButtonText=resetButton.GetComponentInChildren<Text>();
            if(resetButtonText!=null)resetButtonText.fontSize=11;

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
            cityNavButton=NavButton(nav,"⌂","CIUDAD",()=>{
#if UNITY_WEBGL && !UNITY_EDITOR
                Debug.Log("ELDORIA_PLAYABLE_NAV target=Valoria city="+city);
#endif
                if(!city)SceneManager.LoadScene(CitySceneName);
            });
            worldNavButton=NavButton(nav,"◎","MUNDO",()=>{
#if UNITY_WEBGL && !UNITY_EDITOR
                Debug.Log("ELDORIA_PLAYABLE_NAV target=Frontier city="+city);
#endif
                if(city)SceneManager.LoadScene("Frontier");
            });
            // Do not expose prototype/future buttons without real gameplay behind them.
            // The migrated vertical-slice hierarchy keeps only the two live navigation surfaces.
            StyleNavButton(cityNavButton,city);StyleNavButton(worldNavButton,!city);

            var dock=new GameObject("World objective dock",typeof(RectTransform),typeof(Image),typeof(VerticalLayoutGroup));
            objectiveDock=dock;
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
            CreateBuildingPanel(safe);
#if UNITY_WEBGL && !UNITY_EDITOR
            StartCoroutine(LogPlayableUiGeometryNextFrame());
#endif
        }

#if UNITY_WEBGL && !UNITY_EDITOR
        System.Collections.IEnumerator LogPlayableUiGeometryNextFrame()
        {
            yield return null;
            Canvas.ForceUpdateCanvases();
            LogPlayableButtonCenter("home",homeButton);
            LogPlayableButtonCenter("cityNav",cityNavButton);
            LogPlayableButtonCenter("worldNav",worldNavButton);
            LogPlayableButtonCenter("primary",primaryAction);
            LogPlayableButtonCenter("reset",resetButton);
            if(city){StartCoroutine(LogWorldHotspotNextFrame("Aserradero · target","sawmill"));StartCoroutine(LogWorldHotspotNextFrame("Bastion · target","bastion"));}
        }

        System.Collections.IEnumerator LogPlayableButtonCenterNextFrame(string id,Button button)
        {
            yield return null;
            Canvas.ForceUpdateCanvases();
            LogPlayableButtonCenter(id,button);
        }

        static void LogPlayableButtonCenter(string id,Button button)
        {
            if(button==null)return;
            var rect=button.GetComponent<RectTransform>();
            if(rect==null)return;
            var point=RectTransformUtility.WorldToScreenPoint(null,rect.TransformPoint(rect.rect.center));
            Debug.Log("ELDORIA_PLAYABLE_UI id="+id+" x="+point.x.ToString("F1")+" y="+point.y.ToString("F1")+
                " enabled="+button.interactable+" visible="+button.gameObject.activeInHierarchy+
                " label="+button.GetComponentInChildren<Text>()?.text);
        }
#endif

        static void StyleNavButton(Button button,bool active)
        {
            if(button==null)return;
            var image=button.GetComponent<Image>();
            var text=button.GetComponentInChildren<Text>();
            image.color=active?new Color(.085f,.11f,.14f,.98f):new Color(.035f,.055f,.075f,.01f);
            if(text!=null)text.color=active?new Color(.95f,.82f,.56f):new Color(.52f,.57f,.60f);
        }

        static string ResourceNeed(PlayerState s,int wood,int stone)
        {
            if(s.Resources.Wood<wood)return "forest-valoria";
            if(s.Resources.Stone<stone)return "quarry-valoria";
            return null;
        }
        static string ObjectiveResourceNeed(PlayerState s)
        {
            switch(SliceRules.CurrentObjectiveKey(s))
            {
                case "b1.build-sawmill": return ResourceNeed(s,SliceRules.SawmillWoodCost,0);
                case "b1.ascend": return ResourceNeed(s,SliceRules.Bastion2WoodCost,SliceRules.Bastion2StoneCost);
                case "b2.build-barracks": return ResourceNeed(s,SliceRules.BarracksWoodCost,SliceRules.BarracksStoneCost);
                case "b2.train-archers": return ResourceNeed(s,SliceRules.RecruitWoodCost,SliceRules.RecruitStoneCost);
                default: return null;
            }
        }

        void ConfigurePrimaryAction(PlayerState s)
        {
            if(primaryAction==null||primaryActionText==null||s==null)return;
            SetPrimaryActionEnabled(true);
            if(s.BuildingCompletesUtcTicks>0)
            {
                primaryActionText.text="RECONSTRUCCIÓN EN CURSO";
                SetPrimaryActionEnabled(false);
                return;
            }
            if(s.RecruitmentCompletesUtcTicks>0)
            {
                primaryActionText.text="ENTRENAMIENTO EN CURSO";
                SetPrimaryActionEnabled(false);
                return;
            }
            if(s.March.Phase!="idle")
            {
                primaryActionText.text="MARCHA EN CURSO";
                SetPrimaryActionEnabled(false);
                return;
            }

            var needed=ObjectiveResourceNeed(s);
            if(needed!=null)
            {
                bool wood=needed=="forest-valoria";
                primaryActionText.text=city?(wood?"BUSCAR MADERA":"BUSCAR PIEDRA"):(wood?"RECOLECTAR MADERA":"RECOLECTAR PIEDRA");
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
                    SetPrimaryActionEnabled(!city);
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
                    SetPrimaryActionEnabled(!city);
                    break;
                default:
                    primaryActionText.text="CONTINUAR";
                    break;
            }
        }

        void SetPrimaryActionEnabled(bool enabled)
        {
            primaryAction.interactable=enabled;
            // The default Button disabled tint darkens the ochre plate, leaving its dark
            // active label unreadable. Keep completion/wait feedback legible on mobile.
            var colors=primaryAction.colors;
            colors.disabledColor=Color.white;
            primaryAction.colors=colors;
            primaryAction.GetComponent<Image>().color=enabled
                ?new Color(.73f,.61f,.36f,.98f)
                :new Color(.14f,.17f,.19f,.98f);
            primaryActionText.color=enabled
                ?new Color(.07f,.09f,.11f)
                :new Color(.94f,.84f,.63f);
        }

        void InvokePrimaryObjective()
        {
            var s=gateway.Snapshot();
            string key=SliceRules.CurrentObjectiveKey(s);
            var needed=ObjectiveResourceNeed(s);
            if(needed!=null)
            {
                if(city)SceneManager.LoadScene("Frontier");
                else Send("Gather",needed);
                return;
            }
            if(city)
            {
                if(key=="b1.build-sawmill"&&s.Resources.Wood>=SliceRules.SawmillWoodCost)
                {
                    FocusCityHotspot(productionParcels!=null?InteractiveTargetName("sawmill"):"Aserradero · target");
                    OpenBuildingPanel("sawmill");
                }
                else if(key=="b1.ascend")
                {
                    FocusCityHotspot(productionParcels!=null?InteractiveTargetName("bastion"):"Bastion · target");
                    OpenBuildingPanel("bastion");
                }
                else if(key=="b2.build-barracks"||
                        (key=="b2.train-archers"&&s.Resources.Wood>=SliceRules.RecruitWoodCost&&s.Resources.Stone>=SliceRules.RecruitStoneCost))
                {
                    FocusCityHotspot(productionParcels!=null?InteractiveTargetName("barracks"):"Cuartel · target");
                    OpenBuildingPanel("barracks");
                }
                else if(key=="b2.prepare-march"||key=="b2.raise-expedition-power")
                {
                    FocusCityHotspot(productionParcels!=null?InteractiveTargetName("barracks"):"Cuartel · target");
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
                else SceneManager.LoadScene(CitySceneName);
            }
            else if(key=="b2.defeat-engendro")Send("Fight","engendro-valoria");
            else SceneManager.LoadScene(CitySceneName);
        }
        void CreateBuildingPanel(Transform parent)
        {
            buildingPanel=new GameObject("Building interaction panel",typeof(RectTransform),typeof(Image),typeof(VerticalLayoutGroup));
            var rt=buildingPanel.GetComponent<RectTransform>();rt.SetParent(parent,false);
            rt.anchorMin=rt.anchorMax=new Vector2(.5f,.5f);rt.pivot=new Vector2(.5f,.5f);
            rt.sizeDelta=new Vector2(276,178);rt.anchoredPosition=new Vector2(0,-70);
            buildingPanel.GetComponent<Image>().color=new Color(.045f,.065f,.085f,.94f);
            var layout=buildingPanel.GetComponent<VerticalLayoutGroup>();layout.padding=new RectOffset(12,12,8,8);
            layout.spacing=2;layout.childControlHeight=true;layout.childForceExpandHeight=false;
            buildingTitle=Label("Building title",buildingPanel.transform,15,new Color(.96f,.88f,.69f),24);
            buildingBody=Label("Building body",buildingPanel.transform,12,new Color(.86f,.89f,.90f),56);
            var actionGo=new GameObject("Building action",typeof(RectTransform),typeof(Image),typeof(Button),typeof(LayoutElement));
            actionGo.transform.SetParent(buildingPanel.transform,false);
            actionGo.GetComponent<Image>().color=new Color(.73f,.61f,.36f,.98f);
            actionGo.GetComponent<LayoutElement>().preferredHeight=44;
            buildingAction=actionGo.GetComponent<Button>();
            var actionText=Label("Text",actionGo.transform,13,new Color(.07f,.09f,.11f),44);
            actionText.text="ACCIÓN";actionText.alignment=TextAnchor.MiddleCenter;
            var ar=actionText.rectTransform;ar.anchorMin=Vector2.zero;ar.anchorMax=Vector2.one;ar.offsetMin=ar.offsetMax=Vector2.zero;
            var closeGo=new GameObject("Cerrar",typeof(RectTransform),typeof(Image),typeof(Button),typeof(LayoutElement));
            closeGo.transform.SetParent(buildingPanel.transform,false);
            closeGo.GetComponent<Image>().color=new Color(.15f,.16f,.16f,.98f);
            closeGo.GetComponent<LayoutElement>().preferredHeight=32;
            closeGo.GetComponent<Button>().onClick.AddListener(CloseBuildingPanel);
            var closeText=Label("Text",closeGo.transform,9,new Color(.78f,.82f,.84f),32);closeText.text="CERRAR";closeText.alignment=TextAnchor.MiddleCenter;
            var cr=closeText.rectTransform;cr.anchorMin=Vector2.zero;cr.anchorMax=Vector2.one;cr.offsetMin=cr.offsetMax=Vector2.zero;
            CloseBuildingPanel();
        }

        void CreateCityAmbientation(Transform parent)
        {
            if(!city)return;
            cityAmbientLayer=new GameObject("City ambient life",typeof(RectTransform));
            var layer=cityAmbientLayer.GetComponent<RectTransform>();layer.SetParent(parent,false);
            layer.anchorMin=Vector2.zero;layer.anchorMax=Vector2.one;layer.offsetMin=layer.offsetMax=Vector2.zero;
            ambientMotes=new RectTransform[8];ambientMoteOrigins=new Vector2[8];
            for(int i=0;i<ambientMotes.Length;i++)
            {
                var go=new GameObject("Ambient ember "+i,typeof(RectTransform),typeof(Image));
                var rt=go.GetComponent<RectTransform>();rt.SetParent(layer,false);
                rt.anchorMin=rt.anchorMax=new Vector2(.12f+.105f*i,.22f+.035f*(i%3));
                rt.sizeDelta=new Vector2(3f+(i%2),3f+(i%2));
                ambientMoteOrigins[i]=new Vector2((i%2==0?-10f:12f),i*2f);
                rt.anchoredPosition=ambientMoteOrigins[i];
                var image=go.GetComponent<Image>();image.color=new Color(.94f,.72f,.34f,.24f);image.raycastTarget=false;
                ambientMotes[i]=rt;
            }
            var mist=new GameObject("Ambient drifting mist",typeof(RectTransform),typeof(Image));
            var mrt=mist.GetComponent<RectTransform>();mrt.SetParent(layer,false);mrt.anchorMin=new Vector2(0,.18f);mrt.anchorMax=new Vector2(1,.34f);
            mrt.offsetMin=new Vector2(-30,0);mrt.offsetMax=new Vector2(30,0);
            var mi=mist.GetComponent<Image>();mi.color=new Color(.72f,.78f,.72f,.035f);mi.raycastTarget=false;
            constructionActivityFx=new GameObject("Construction activity FX",typeof(RectTransform),typeof(Image));
            var crt=constructionActivityFx.GetComponent<RectTransform>();crt.SetParent(layer,false);crt.anchorMin=crt.anchorMax=new Vector2(.22f,.43f);
            crt.sizeDelta=new Vector2(44,44);
            var ci=constructionActivityFx.GetComponent<Image>();ci.color=new Color(.95f,.70f,.28f,.11f);ci.raycastTarget=false;
            constructionActivityFx.SetActive(false);

            sawmillLevelBadge=CreateBuildingLevelBadge(layer,"Sawmill level badge");
            barracksLevelBadge=CreateBuildingLevelBadge(layer,"Barracks level badge");
            bastionLevelBadge=CreateBuildingLevelBadge(layer,"Bastion level badge");
        }

        Text CreateBuildingLevelBadge(Transform parent,string name)
        {
            var go=new GameObject(name,typeof(RectTransform),typeof(Image));
            var rt=go.GetComponent<RectTransform>();rt.SetParent(parent,false);rt.anchorMin=rt.anchorMax=new Vector2(.5f,.5f);rt.sizeDelta=new Vector2(46,22);
            var image=go.GetComponent<Image>();image.color=new Color(.035f,.055f,.075f,.90f);image.raycastTarget=false;
            var text=Label(name+" text",go.transform,8,new Color(.96f,.82f,.48f),22);
            text.alignment=TextAnchor.MiddleCenter;text.fontStyle=FontStyle.Bold;
            var tr=text.rectTransform;tr.anchorMin=Vector2.zero;tr.anchorMax=Vector2.one;tr.offsetMin=tr.offsetMax=Vector2.zero;
            go.SetActive(false);
            return text;
        }

        void UpdateBuildingLevelBadges(PlayerState s)
        {
            SetBuildingLevelBadge(sawmillLevelBadge,"sawmill",s.SawmillLevel);
            SetBuildingLevelBadge(barracksLevelBadge,"barracks",s.BarracksLevel);
            SetBuildingLevelBadge(bastionLevelBadge,"bastion",s.BastionLevel);
        }

        void SetBuildingLevelBadge(Text badge,string id,int level)
        {
            if(badge==null)return;
            badge.transform.parent.gameObject.SetActive(city&&level>0);
            badge.text="Nv. "+level;
        }

        void UpdateBuildingLevelBadgePositions()
        {
            if(!city||safe==null)return;
            PositionBuildingLevelBadge(sawmillLevelBadge,"sawmill",new Vector2(34,20));
            PositionBuildingLevelBadge(barracksLevelBadge,"barracks",new Vector2(34,20));
            PositionBuildingLevelBadge(bastionLevelBadge,"bastion",new Vector2(38,24));
        }

        void PositionBuildingLevelBadge(Text badge,string id,Vector2 offset)
        {
            if(badge==null||!badge.transform.parent.gameObject.activeSelf)return;
            var target=GameObject.Find(InteractiveTargetName(id));var camera=OfficialCamera;
            if(target==null||camera==null)return;
            var collider=target.GetComponent<Collider>();
            var world=collider!=null?collider.bounds.center:target.transform.position;
            var screen=PresentedScreenPoint(camera,world);
            if(screen.z<=0)return;
            if(!RectTransformUtility.ScreenPointToLocalPointInRectangle(safe,screen,null,out var local))return;
            badge.transform.parent.GetComponent<RectTransform>().anchoredPosition=local+offset;
        }

        static string InteractiveTargetName(string id)
        {
            return id=="sawmill"?"InteractiveProxy_LeftCabinParcel":id=="barracks"?"InteractiveProxy_RightCampParcel":id=="bastion"?"InteractiveProxy_Bastion":null;
        }

        void AnimateCityAmbientation()
        {
            if(!city||ambientMotes==null)return;
            float t=Time.unscaledTime;
            for(int i=0;i<ambientMotes.Length;i++)
            {
                var rt=ambientMotes[i];if(rt==null)continue;
                var origin=ambientMoteOrigins[i];
                rt.anchoredPosition=origin+new Vector2(Mathf.Sin(t*.55f+i)*7f,Mathf.Repeat(t*(4.5f+i*.35f)+i*11f,52f));
            }
            if(constructionActivityFx!=null&&constructionActivityFx.activeSelf)
            {
                var rt=constructionActivityFx.GetComponent<RectTransform>();
                float pulse=1f+Mathf.Sin(t*4f)*.12f;rt.localScale=new Vector3(pulse,pulse,1f);
            }
        }
#if UNITY_WEBGL && !UNITY_EDITOR
        IEnumerator LogPlayableButtonRectNextFrame(Button button,string name)
        {
            yield return null;
            if(button==null)yield break;
            var rt=button.GetComponent<RectTransform>();
            if(rt==null)yield break;
            var corners=new Vector3[4];
            rt.GetWorldCorners(corners);
            var a=RectTransformUtility.WorldToScreenPoint(null,corners[0]);
            var b=RectTransformUtility.WorldToScreenPoint(null,corners[2]);
            float x=(a.x+b.x)*.5f;
            float y=Screen.height-(a.y+b.y)*.5f;
            Debug.Log("ELDORIA_PLAYABLE_UI name="+name+" x="+x.ToString("F1")+" y="+y.ToString("F1")+
                " w="+Mathf.Abs(b.x-a.x).ToString("F1")+" h="+Mathf.Abs(b.y-a.y).ToString("F1"));
        }

        public void PublishPresentedCityGeometry()
        {
            if(!city)return;
            StartCoroutine(LogWorldHotspotNextFrame("Aserradero · target","sawmill"));
            StartCoroutine(LogWorldHotspotNextFrame("Cuartel · target","barracks"));
            StartCoroutine(LogWorldHotspotNextFrame("Bastion · target","bastion"));
        }
        IEnumerator LogWorldHotspotNextFrame(string objectName,string id)
        {
            yield return null;
            var camera=OfficialCamera;
            var target=GameObject.Find(city&&productionParcels!=null?InteractiveTargetName(id):objectName);
            if(camera==null||target==null)yield break;
            var collider=target.GetComponent<Collider>();
            var world=collider!=null?collider.bounds.center:target.transform.position;
            var point=PresentedScreenPoint(camera,world);
            if(point.z<=0)yield break;
            Debug.Log("ELDORIA_PLAYABLE_HOTSPOT id="+id+" x="+point.x.ToString("F1")+
                " y="+(Screen.height-point.y).ToString("F1"));
        }
#endif

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

        void Zoom(float amount)
        {
            if(productionParcels!=null){productionParcels.Zoom(amount);return;}
            if(OfficialCamera==null)return;
            float min=city?9f:10f;
            float max=city?19f:18f;
            OfficialCamera.orthographicSize=Mathf.Clamp(OfficialCamera.orthographicSize+amount,min,max);
        }
        void UpdateSafeArea()
        {
            lastWidth=Screen.width;lastHeight=Screen.height;
            if(safe==null||lastWidth==0||lastHeight==0)return;
            var canvas=safe.GetComponentInParent<Canvas>();
            var scaler=canvas!=null?canvas.GetComponent<CanvasScaler>():null;
            if(scaler!=null)
            {
                scaler.referenceResolution=lastWidth>lastHeight?new Vector2(844,390):new Vector2(390,844);
                scaler.matchWidthOrHeight=.5f;
            }
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
            text.text=label;
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

        void InvokeOwnerReset()
        {
            if(!ownerResetArmed)
            {
                ownerResetArmed=true;
#if UNITY_WEBGL && !UNITY_EDITOR
                Debug.Log("ELDORIA_PLAYABLE_RESET armed=True");
#endif
                if(resetButtonText!=null)resetButtonText.text="CONFIRMAR";
                feedback="Pulsa de nuevo para empezar desde Bastión I.";
                if(message!=null)message.text=feedback;
                return;
            }
            ownerResetArmed=false;
            ownerResetExecutionSerial++;
#if UNITY_WEBGL && !UNITY_EDITOR
            Debug.Log("ELDORIA_PLAYABLE_RESET execute=True serial="+ownerResetExecutionSerial);
#endif
            if(resetButton!=null)resetButton.interactable=false;
            if(resetButtonText!=null)resetButtonText.text="REINICIANDO…";
            feedback="Reiniciando desde Bastión I…";
            if(message!=null)message.text=feedback;
            SliceBoot.ResetLocalSaveAndRestart();
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


