using System;
using System.Reflection;
using System.Collections.Generic;
using Eldoria.ReferenceClean;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Eldoria.BreachWatch
{
    // Optional PR187 capability bridge. A standalone PR173 build keeps its
    // existing controls when the actual saved bag component is not installed.
    [DisallowMultipleComponent]
    public sealed class BreachWatchResourceBagBridge : MonoBehaviour
    {
        ValoriaIntegratedWorld host;
        Canvas canvas;
        Component bag;
        MethodInfo open, restore, refresh, countNew, countClaimable;
        readonly Button[] buttons=new Button[3];
        readonly UnityAction[] listeners=new UnityAction[3];
        readonly Image[] novelty=new Image[3];
        Image claimable;
        readonly Dictionary<Button,Graphic> originalBagTargets=
            new Dictionary<Button,Graphic>();
        readonly Dictionary<Button,Selectable.Transition> originalBagTransitions=
            new Dictionary<Button,Selectable.Transition>();
        readonly Dictionary<Graphic,bool> originalBagGraphics=
            new Dictionary<Graphic,bool>();
        readonly List<GameObject> bagArtwork=new List<GameObject>();
        static readonly string[] Kinds={"wood","stone","food"};
        bool installed,failed;

        public bool Installed=>installed&&bag;
        public void Initialize(ValoriaIntegratedWorld source,Canvas active,
            RectTransform[] capsules,Button actualMailbox)
        {
            host=source;canvas=active;
            if(!host||!host.Hud||!canvas)return;
            bag=host.Hud.GetComponent("Issue186BagOverlay");
            if(!bag)return;
            var type=bag.GetType();
            open=type.GetMethod("OpenOnCanvas",new[]{typeof(Canvas)});
            restore=type.GetMethod("RestorePresentation",Type.EmptyTypes);
            refresh=type.GetMethod("Refresh",Type.EmptyTypes);
            countNew=type.GetMethod("NewPacketCount",new[]{typeof(string)});
            countClaimable=type.GetMethod("ClaimableMailCount",Type.EmptyTypes);
            if(open==null||restore==null||refresh==null||countNew==null||
                countClaimable==null)return;
            for(int i=0;i<buttons.Length;i++)
            {
                if(i>=capsules.Length||!capsules[i])continue;
                // Capsules are already the approved, real resource surface.
                // Do not repaint them or create an alternative floating HUD.
                buttons[i]=capsules[i].gameObject.AddComponent<Button>();
                buttons[i].targetGraphic=capsules[i].GetComponent<Graphic>();
                buttons[i].transition=Selectable.Transition.None;
                listeners[i]=TryOpen;
                buttons[i].onClick.AddListener(listeners[i]);
                novelty[i]=Badge(capsules[i],"Saved bag NEW",
                    "Issue186ResourceArt/badge_new",20f,true);
            }
            if(actualMailbox)
                claimable=Badge((RectTransform)actualMailbox.transform,
                    "Saved rewards claimable",
                    "Issue186ResourceArt/badge_claimable",24f);
            installed=true;
            Synchronize();
        }
        static Image Badge(RectTransform parent,string name,string asset,float size,
            bool clearQuantity=false)
        {
            var sprite=Resources.Load<Sprite>(asset);
            if(!sprite)return null;
            var obj=new GameObject(name,typeof(RectTransform),typeof(CanvasRenderer),
                typeof(Image));
            obj.layer=5;obj.transform.SetParent(parent,false);
            var r=(RectTransform)obj.transform;
            r.anchorMin=r.anchorMax=new Vector2(1f,1f);
            // Resource values occupy the capsule's middle band. Attach the
            // unchanged authored alert to its upper rim instead of painting
            // over the final digit. Header top margins retain the full circle.
            r.pivot=clearQuantity?new Vector2(1f,.5f):new Vector2(1f,1f);
            r.anchoredPosition=clearQuantity?new Vector2(-3f,6f):new Vector2(-3f,-2f);
            r.sizeDelta=new Vector2(size,size);
            var image=obj.GetComponent<Image>();
            image.sprite=sprite;image.preserveAspect=true;image.raycastTarget=false;
            obj.SetActive(false);
            return image;
        }
        bool CanOpen()
        {
            if(!Installed||!host.Ready||host.InWorld||host.Hud.NarrativeOpen||
                (host.Hud.Modal&&host.Hud.Modal.gameObject.activeSelf))return false;
            var ui=GetComponent<BreachWatchHud>();
            return ui&&!(ui.RealDestinations&&ui.RealDestinations.IsShowing)&&
                !(ui.RealHeroes&&ui.RealHeroes.IsShowing)&&
                !(ui.RealRetention&&ui.RealRetention.IsShowing)&&
                !(ui.RealMailbox&&ui.RealMailbox.IsShowing);
        }
        void TryOpen()
        {
            if(!CanOpen())return;
            try
            {
                // Use the normal selection route to close building context.
                // Never change the save, unlock state or selected resource.
                host.Hud.SelectBuilding("");
                var opened=open.Invoke(bag,new object[]{canvas});
                if(opened is bool && (bool)opened)StyleRealBagBack();
            }
            catch(Exception e){Fail(e);}
        }
        public void Synchronize()
        {
            if(!Installed||failed)return;
            try
            {
                refresh.Invoke(bag,null);
                StyleRealBagBack();
                bool allowed=CanOpen();
                for(int i=0;i<buttons.Length;i++)
                {
                    if(buttons[i])buttons[i].interactable=allowed;
                    if(novelty[i])novelty[i].gameObject.SetActive(
                        (int)countNew.Invoke(bag,new object[]{Kinds[i]})>0);
                }
                // Read and claim are distinct saved states. Opening or reading
                // mail must not silently erase the pending-reward indicator.
                if(claimable)claimable.gameObject.SetActive(
                    (int)countClaimable.Invoke(bag,null)>0);
            }
            catch(Exception e){Fail(e);}
        }
        void StyleRealBagBack()
        {
            if(!canvas)return;
            // Repaint the real secondary Volver only, preserve its callback,
            // its source controller and any first-use tutorial gesture.
            Button back=null;
            foreach(var button in canvas.GetComponentsInChildren<Button>(true))
                if(button && button.name=="Issue186 bag close" &&
                    button.gameObject.activeInHierarchy)
                {back=button;break;}
            if(!back || originalBagTargets.ContainsKey(back))return;
            var old=back.targetGraphic;
            var rect=back.transform as RectTransform;
            if(!rect)return;
            var obj=new GameObject("Vigilia / unified secondary Volver",
                typeof(RectTransform),typeof(CanvasRenderer),typeof(Image));
            obj.layer=rect.gameObject.layer;obj.transform.SetParent(rect,false);
            var visual=(RectTransform)obj.transform;
            visual.anchorMin=Vector2.zero;visual.anchorMax=Vector2.one;
            visual.offsetMin=Vector2.zero;visual.offsetMax=Vector2.zero;
            visual.SetAsFirstSibling();
            var image=obj.GetComponent<Image>();
            if(!BreachWatchControlKit.Paint(image,
                BreachWatchControlKit.Role.Secondary))
            {Destroy(obj);return;}
            originalBagTargets.Add(back,old);
            originalBagTransitions.Add(back,back.transition);
            if(old)
            {
                originalBagGraphics[old]=old.enabled;
                old.enabled=false;
            }
            back.targetGraphic=image;
            back.transition=Selectable.Transition.None;
            bagArtwork.Add(obj);
        }
        void Fail(Exception e)
        {
            failed=true;
            foreach(var button in buttons)if(button)button.interactable=false;
            foreach(var badge in novelty)if(badge)badge.gameObject.SetActive(false);
            if(claimable)claimable.gameObject.SetActive(false);
            Debug.LogException(e);
        }
        public void Restore()
        {
            foreach(var entry in originalBagTargets)
                if(entry.Key)entry.Key.targetGraphic=entry.Value;
            originalBagTargets.Clear();
            foreach(var entry in originalBagTransitions)
                if(entry.Key)entry.Key.transition=entry.Value;
            originalBagTransitions.Clear();
            foreach(var entry in originalBagGraphics)
                if(entry.Key)entry.Key.enabled=entry.Value;
            originalBagGraphics.Clear();
            foreach(var obj in bagArtwork)if(obj)Destroy(obj);
            bagArtwork.Clear();
            if(bag&&restore!=null)
            {
                try{restore.Invoke(bag,null);}
                catch(Exception e){Debug.LogException(e);}
            }
            installed=false;
        }
        void OnDestroy(){Restore();}
    }
}
