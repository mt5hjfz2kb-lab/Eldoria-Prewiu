using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using Eldoria.Application;
using Eldoria.Domain;
using Eldoria.UI.RoyalGold;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Eldoria.UI.RoyalGold.V2
{
    // A presentation-only first-use guide for the real, saved I–V journey.
    // Attaches to the existing RoyalHUD; it NEVER creates a Canvas/EventSystem,
    // sends game commands, changes progress, or paints over an untouchable target.
    // The authoritative objective changes only after the real player action.
    [DisallowMultipleComponent]
    public sealed class Issue186FirstUseCoach : MonoBehaviour
    {
        RoyalHUD hud;
        ICommandGateway gateway;
        MonoBehaviour host, vigilia;
        Canvas canvas;
        RectTransform surface;
        Image hand, halo, noteBack;
        Text note;
        float nextRefresh;
        readonly List<RaycastResult> hits = new List<RaycastResult>(16);

        public bool Visible => hand && hand.gameObject.activeInHierarchy;
        public string TargetId {get;private set;} = "";
        public string Rejection {get;private set;} = "not-initialized";
        public string ObjectiveKey {get;private set;} = "";

        public void Initialize(RoyalHUD owner,ICommandGateway authority)
        {
            hud=owner; gateway=authority;
            Rejection="waiting-for-vigilia";
        }

        static object Property(object obj,string name)
        {
            return obj?.GetType().GetProperty(name,
                BindingFlags.Public|BindingFlags.Instance)?.GetValue(obj);
        }

        bool FindLiveSurface()
        {
            if(canvas && canvas.isActiveAndEnabled && vigilia && surface)return true;
            if(surface)Destroy(surface.gameObject);
            surface=null;canvas=null;vigilia=null;hand=null;halo=null;note=null;noteBack=null;
            var objectRoot=GameObject.Find("VIGILIA / independent canvas");
            if(!objectRoot)return false;
            var candidate=objectRoot.GetComponent<Canvas>();
            if(!candidate || !candidate.isActiveAndEnabled)return false;
            foreach(var component in objectRoot.GetComponents<MonoBehaviour>())
                if(component && component.GetType().Name=="BreachWatchHud")
                    {vigilia=component;break;}
            if(!vigilia)return false;
            canvas=candidate;
            surface=new GameObject("Issue186 actual first-use coach",
                typeof(RectTransform)).GetComponent<RectTransform>();
            surface.gameObject.layer=5;
            surface.SetParent(canvas.transform,false);
            surface.anchorMin=Vector2.zero;surface.anchorMax=Vector2.one;
            surface.offsetMin=surface.offsetMax=Vector2.zero;
            hand=CreateImage("Nonblocking actual hand",surface,Color.white);
            halo=CreateImage("Nonblocking target halo",surface,
                new Color(1f,1f,1f,.23f));
            hand.sprite=Resources.Load<Sprite>("Issue186ResourceArt/tutorial_pointing_hand");
            halo.sprite=Resources.Load<Sprite>("Issue186ResourceArt/tutorial_target_halo");
            hand.preserveAspect=halo.preserveAspect=true;
            hand.rectTransform.sizeDelta=new Vector2(76f,48f);
            halo.rectTransform.sizeDelta=new Vector2(54f,54f);
            noteBack=CreateImage("First-use caption plate",surface,
                new Color(.10f,.055f,.061f,.94f));
            var art=Resources.Load<Sprite>("VigiliaArt/PremiumSurfaces/panel_fill");
            if(art){noteBack.sprite=art;noteBack.type=Image.Type.Sliced;
                noteBack.color=new Color(1f,1f,1f,.94f);}
            noteBack.rectTransform.sizeDelta=new Vector2(240f,50f);
            var noteObj=new GameObject("Contextual instruction",
                typeof(RectTransform),typeof(CanvasRenderer),typeof(Text));
            noteObj.layer=5;noteObj.transform.SetParent(noteBack.transform,false);
            var nr=noteObj.GetComponent<RectTransform>();
            nr.anchorMin=Vector2.zero;nr.anchorMax=Vector2.one;
            nr.offsetMin=new Vector2(9,5);nr.offsetMax=new Vector2(-9,-5);
            note=noteObj.GetComponent<Text>();
            note.font=Resources.Load<Font>("VigiliaArt/Fonts/LibreBaskerville-Regular");
            if(!note.font)note.font=Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            note.fontSize=14;note.color=new Color(.96f,.87f,.72f,1f);
            note.alignment=TextAnchor.MiddleCenter;
            note.horizontalOverflow=HorizontalWrapMode.Wrap;
            note.verticalOverflow=VerticalWrapMode.Truncate;
            note.raycastTarget=false;
            Hide("waiting-for-authorized-step");
            return true;
        }

        static Image CreateImage(string name,Transform parent,Color tint)
        {
            var obj=new GameObject(name,typeof(RectTransform),
                typeof(CanvasRenderer),typeof(Image));
            obj.layer=5;obj.transform.SetParent(parent,false);
            var image=obj.GetComponent<Image>();
            image.color=tint;image.raycastTarget=false;return image;
        }
        void Hide(string reason)
        {
            Rejection=reason;TargetId="";
            if(hand)hand.gameObject.SetActive(false);
            if(halo)halo.gameObject.SetActive(false);
            if(noteBack)noteBack.gameObject.SetActive(false);
        }
        void OnDestroy(){if(surface)Destroy(surface.gameObject);}

        void Update()
        {
            if(Time.unscaledTime<nextRefresh)return;
            nextRefresh=Time.unscaledTime+.18f;
            if(!hud || gateway==null || !FindLiveSurface())
                {Hide("no-independent-vigilia-canvas");return;}
            if(!host)
            {
                foreach(var candidate in UnityEngine.Object.FindObjectsByType<
                    MonoBehaviour>(FindObjectsInactive.Exclude,FindObjectsSortMode.None))
                    if(candidate && candidate.GetType().Name=="ValoriaIntegratedWorld")
                        {host=candidate;break;}
            }
            if(!host){Hide("canonical-world-host-not-ready");return;}
            var state=gateway.Snapshot();
            if(state==null || string.IsNullOrWhiteSpace(state.PlayerName))
                {Hide("nickname-not-confirmed");return;}
            if(hud.NarrativeOpen ||
                (hud.Modal && hud.Modal.gameObject.activeInHierarchy))
                {Hide("canonical-story-or-modal-active");return;}
            var bag=hud.GetComponent<Issue186BagOverlay>();
            if(bag && bag.IsOpen){Hide("saved-bag-owns-its-first-use-hand");return;}
            if(Property(vigilia,"RealMailbox") is object mail &&
                Property(mail,"IsShowing") is bool mailOpen && mailOpen)
                {Hide("mailbox-open");return;}
            if(Property(vigilia,"RealDestinations") is object dst &&
                Property(dst,"IsShowing") is bool dstOpen && dstOpen)
                {Hide("management-screen-open");return;}
            if(Property(vigilia,"RealHeroes") is object heroes &&
                Property(heroes,"IsShowing") is bool heroesOpen && heroesOpen)
                {Hide("hero-screen-open");return;}
            if(Property(vigilia,"RealRetention") is object retention &&
                Property(retention,"IsShowing") is bool retentionOpen && retentionOpen)
                {Hide("retention-screen-open");return;}
            if(!NarrativeCoherenceRules.Seen(state,"start"))
                {Hide("first-story-not-finished");return;}

            var key=SliceRules.CurrentObjectiveKey(state);
            ObjectiveKey=key??"";
            if(string.IsNullOrEmpty(key)){Hide("no-current-objective");return;}
            bool inWorld=Property(host,"InWorld") is bool b && b;
            string target=null;
            switch(key)
            {
                case "b1.build-sawmill":target="sawmill";break;
                case "b1.gather-wood":target="forest-valoria";break;
                case "b1.gather-stone":target="quarry-valoria";break;
                case "b1.clear-route":target="corrupt-scout";break;
                case "b1.return":target="valoria-map-city";break;
                case "b1.ascend":target="bastion";break;
                case "b2.build-barracks":target="barracks";break;
                case "b2.train-archers":target="barracks";break;
                case "b2.prepare-march":target="march-main";break;
                case "b2.defeat-engendro":target="engendro-valoria";break;
                case "b3.granary":target="granary";break;
                case "b5.stoneworks":target="stoneworks";break;
                default:Hide("objective-has-no-first-use-route");return;
            }
            var guide=state.ResourceGuidance;
            if(!GuidedTutorialRules.ShouldPoint(guide,"journey:"+key,target,true,true))
                {Hide("first-use-dismissed-or-completed");return;}

            if(key=="b2.prepare-march")
            {
                ShowButton(Nav(2),key,"Toca el ejército para preparar tu primera marcha.",
                    "march-navigation");return;
            }
            if(key=="b1.return")
            {
                if(inWorld)ShowButton(Nav(3),key,
                    "Regresa a la ciudad para continuar.", "city-return");
                else Hide("already-in-city");
                return;
            }
            bool worldTarget=key=="b1.gather-wood" || key=="b1.gather-stone" ||
                key=="b1.clear-route" || key=="b2.defeat-engendro";
            if(worldTarget!=inWorld)
            {
                ShowButton(Nav(3),key,worldTarget
                    ?"Abre el mundo para continuar tu misión."
                    :"Regresa a Valoria para construir.","world-toggle");
                return;
            }
            string selected=hud.SelectedBuildingId??"";
            bool selectedTarget=selected==target ||
                (target=="barracks"&&selected=="upgrade-barracks");
            if(selectedTarget)
            {
                if(key=="b2.train-archers")
                    ShowButton(Property(vigilia,"ActualBarracksTrainingEntry") as Button,
                        key,"Abre el entrenamiento del Cuartel.","barracks-training");
                else
                    ShowButton(Property(vigilia,"BuildingAction") as Button,key,
                        worldTarget?"Pulsa la acción para completar el objetivo.":
                        "Pulsa el botón del edificio para continuar.","building-action");
                return;
            }
            ShowWorldTarget(target,key,
                worldTarget?"Toca el objetivo señalado en el mundo.":
                "Toca el edificio señalado para abrir sus acciones.");
        }
        Button Nav(int index)
        {
            var list=Property(vigilia,"NavButtons") as IEnumerable;
            if(list==null)return null;
            int i=0;foreach(var value in list)if(i++==index)return value as Button;
            return null;
        }
        void ShowButton(Button control,string key,string caption,string id)
        {
            if(!control || !control.isActiveAndEnabled ||
                !control.gameObject.activeInHierarchy || !control.interactable)
                {Hide("real-button-not-available:"+id);return;}
            if(!Issue186ArtKit.BindFirstUsePointer(
                gateway.Snapshot().ResourceGuidance,"journey:"+key,id,
                true,canvas,(RectTransform)control.transform,hand,halo))
                {Hide("real-first-hit:"+Issue186ArtKit.LastPointerRejectReason);return;}
            var camera=canvas.renderMode==RenderMode.ScreenSpaceOverlay?null:canvas.worldCamera;
            var screen=RectTransformUtility.WorldToScreenPoint(camera,
                ((RectTransform)control.transform).TransformPoint(
                    ((RectTransform)control.transform).rect.center));
            SetCaption(screen,caption);
            TargetId=id;Rejection="";
        }
        bool NoUiBlocker(Vector2 screen)
        {
            var events=EventSystem.current;
            if(!events || !events.isActiveAndEnabled)return false;
            hits.Clear();
            events.RaycastAll(new PointerEventData(events){position=screen},hits);
            foreach(var hit in hits)
                if(hit.module is GraphicRaycaster)
                    return false;
            return true;
        }
        void ShowWorldTarget(string targetId,string key,string caption)
        {
            var type=host.GetType();
            var found=type.GetMethod("FindTarget",new[]{typeof(string)})
                ?.Invoke(host,new object[]{targetId}) as Component;
            var camera=Property(host,"PlayerCamera") as Camera;
            var picker=type.GetMethod("Pick",new[]{typeof(Vector2)});
            if(!found || !found.gameObject.activeInHierarchy || !camera || picker==null)
                {Hide("real-world-object-or-camera-missing:"+targetId);return;}
            var colliders=found.GetComponentsInChildren<Collider>(true);
            if(colliders.Length==0)
                {Hide("real-world-object-has-no-collider:"+targetId);return;}
            Vector2 point=default;
            bool match=false;
            foreach(var collider in colliders)
            {
                if(!collider || !collider.enabled || !collider.gameObject.activeInHierarchy)
                    continue;
                var bounds=collider.bounds;
                var middle=camera.WorldToScreenPoint(bounds.center);
                if(middle.z<=camera.nearClipPlane)continue;
                float span=Mathf.Min(70f,Mathf.Max(16f,
                    Mathf.Abs(camera.WorldToScreenPoint(bounds.max).x-
                              camera.WorldToScreenPoint(bounds.min).x)));
                foreach(float dx in new float[]{0f,-.20f,.20f,-.42f,.42f})
                {
                    foreach(float dy in new float[]{0f,-.20f,.20f,-.42f,.42f})
                    {
                        var test=new Vector2(middle.x+span*dx,middle.y+span*dy);
                        if(!Screen.safeArea.Contains(test)||!NoUiBlocker(test))continue;
                        object picked=null;
                        try{picked=picker.Invoke(host,new object[]{test});}
                        catch(TargetInvocationException){continue;}
                        if(picked is Component selected &&
                            selected.GetInstanceID()==found.GetInstanceID())
                            {point=test;match=true;break;}
                    }
                    if(match)break;
                }
                if(match)break;
            }
            if(!match){Hide("no-real-physical-hit:"+targetId);return;}
            if(!PlaceWorldHand(point))
                {Hide("real-world-hand-outside-safe-area");return;}
            SetCaption(point,caption);
            TargetId=targetId;Rejection="";
        }
        bool PlaceWorldHand(Vector2 target)
        {
            if(!hand || !halo || !hand.sprite || !halo.sprite || !surface)return false;
            var cam=canvas.renderMode==RenderMode.ScreenSpaceOverlay?null:canvas.worldCamera;
            var rect=hand.rectTransform;
            rect.anchorMin=rect.anchorMax=new Vector2(.5f,.5f);
            rect.pivot=new Vector2(.16f,.696f);
            rect.sizeDelta=new Vector2(76f,76f*hand.sprite.rect.height/
                Mathf.Max(1f,hand.sprite.rect.width));
            rect.localRotation=Quaternion.identity;
            var safe=Screen.safeArea;
            safe.xMin+=6;safe.xMax-=6;safe.yMin+=6;safe.yMax-=6;
            bool fit=false;
            foreach(var sign in new[]{1f,-1f})
            {
                if(!RectTransformUtility.ScreenPointToLocalPointInRectangle(surface,
                    target,cam,out var local))continue;
                rect.localScale=new Vector3(sign,1f,1f);
                rect.anchoredPosition=local;
                var corners=new Vector3[4];
                rect.GetWorldCorners(corners);
                bool safeCorners=true;
                foreach(var corner in corners)
                    if(!safe.Contains(RectTransformUtility.WorldToScreenPoint(cam,corner)))
                        {safeCorners=false;break;}
                if(safeCorners){fit=true;break;}
            }
            if(!fit)return false;
            hand.gameObject.SetActive(true);
            var haloRect=halo.rectTransform;
            haloRect.anchorMin=haloRect.anchorMax=new Vector2(.5f,.5f);
            haloRect.pivot=new Vector2(.5f,.5f);
            if(RectTransformUtility.ScreenPointToLocalPointInRectangle(surface,
                target,cam,out var centered))
            {
                haloRect.anchoredPosition=centered;
                halo.gameObject.SetActive(true);
            }
            surface.SetAsLastSibling();
            return true;
        }
        void SetCaption(Vector2 target,string caption)
        {
            if(!noteBack || !note || !surface)return;
            var camera=canvas.renderMode==RenderMode.ScreenSpaceOverlay?null:canvas.worldCamera;
            float width=Mathf.Min(240f,Mathf.Max(140f,Screen.safeArea.width-24f));
            float y=target.y+86f;
            if(y+30f>Screen.safeArea.yMax-8f)y=target.y-86f;
            var point=new Vector2(
                Mathf.Clamp(target.x,Screen.safeArea.xMin+width*.5f+5f,
                    Screen.safeArea.xMax-width*.5f-5f),
                Mathf.Clamp(y,Screen.safeArea.yMin+35f,Screen.safeArea.yMax-35f));
            var r=noteBack.rectTransform;
            r.anchorMin=r.anchorMax=new Vector2(.5f,.5f);
            r.pivot=new Vector2(.5f,.5f);
            r.sizeDelta=new Vector2(width,50f);
            if(RectTransformUtility.ScreenPointToLocalPointInRectangle(
                surface,point,camera,out var local))r.anchoredPosition=local;
            note.text=caption;noteBack.gameObject.SetActive(true);
            surface.SetAsLastSibling();
        }
    }
}
