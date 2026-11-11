using System;
using Eldoria.ReferenceClean;
using Eldoria.HeroesUI;
using Eldoria.UI.RoyalGold.V2;
using UnityEngine;
using UnityEngine.UI;

namespace Eldoria.BreachWatch
{
    /// <summary>
    /// Migration of ONE canonical heroes destination to the independent
    /// Vigilia Canvas. Does not create hero data or a second hero screen.
    /// Adopts story-driven recruitment as well as explicit navigation.
    /// Restores its old parent and callbacks if the staged replacement fails.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class BreachWatchHeroesBridge : MonoBehaviour
    {
        ValoriaIntegratedWorld host;
        Canvas canvas;
        HeroesApprovedDestination heroes;
        BreachWatchDestinationBridge army;
        BreachWatchHeroesTheme theme;
        RectTransform root;
        Transform parent;
        int siblingIndex;
        Action originalClosed;
        Action bridgeClosed;
        public bool IsShowing => heroes && heroes.IsOpen && root &&
            root.parent==canvas.transform;
        public bool ReplacedOldFrames => IsShowing && theme &&
            theme.RestyledFrames>0 && !theme.VisibleOldRoyalFrame;

        public void Initialize(ValoriaIntegratedWorld source,Canvas presentation,
            BreachWatchDestinationBridge militaryDestination)
        {
            host=source;canvas=presentation;army=militaryDestination;
            heroes=source && source.Hud
                ?source.Hud.GetComponent<HeroesApprovedDestination>():null;
            if(!heroes)return;
            originalClosed=heroes.Closed;
            bridgeClosed=OnCanonicalHeroesClosed;
            heroes.Closed=bridgeClosed;
        }

        public bool TryOpen()
        {
            if(!heroes || !canvas || IsShowing)return false;
            if(!heroes.IsOpen)heroes.Open(); // canonical story unlock guard
            return heroes.IsOpen && TryAdoptAlreadyOpen();
        }

        public bool TryAdoptAlreadyOpen()
        {
            if(!heroes || !heroes.IsOpen || !canvas || IsShowing)return false;
            var actual=heroes.VisibleRoot;
            if(!actual)return false;
            parent=actual.parent;
            siblingIndex=actual.GetSiblingIndex();
            root=actual;
            root.SetParent(canvas.transform,false);
            root.anchorMin=Vector2.zero;root.anchorMax=Vector2.one;
            root.offsetMin=Vector2.zero;root.offsetMax=Vector2.zero;
            root.SetAsLastSibling();
            theme=root.GetComponent<BreachWatchHeroesTheme>();
            if(!theme)theme=root.gameObject.AddComponent<BreachWatchHeroesTheme>();
            // The canonical page rebuilds on a new parent Canvas so real
            // text, portrait and touch sizes use Vigilia's mobile scale.
            heroes.Refresh();
            Canvas.ForceUpdateCanvases();
            if(!theme.Initialize(root) || !theme.Refresh(root) ||
               theme.VisibleOldRoyalFrame)
            {
                // A failed visual migration must never leave an otherwise
                // valid canonical hero dialog invisible on the old Canvas.
                heroes.Suspend();
                Restore();
                return false;
            }
            return true;
        }

        public void SynchronizeVisual()
        {
            // Canonical hero pages are rebuilt on live selection and on
            // offscreen orientation changes. Apply the replacement to the
            // NEW authoritative content before accepting each screenshot.
            if(IsShowing && theme && root)theme.Refresh(root);
        }

        void LateUpdate()
        {
            if(!heroes || !canvas)return;
            if(heroes.IsOpen && !IsShowing)
            {
                // Canonical story may open recruitment without our own button.
                // Do not leave that legitimate screen on a disabled Canvas.
                TryAdoptAlreadyOpen();
            }
            else if(root && !heroes.IsOpen)Restore();
            if(IsShowing)SynchronizeVisual();
        }

        void OnCanonicalHeroesClosed()
        {
            // Preserve exactly the original close behavior, including
            // returning to an already-open real management destination.
            originalClosed?.Invoke();
            Restore();
            if(army)army.TryAdoptAlreadyOpen();
        }

        public void Close()
        {
            if(heroes && heroes.IsOpen)heroes.Close();
            Restore();
        }

        void Restore()
        {
            if(theme)theme.Restore();
            if(root && parent)
            {
                root.SetParent(parent,false);
                root.SetSiblingIndex(Mathf.Clamp(siblingIndex,0,parent.childCount-1));
            }
            root=null;parent=null;
        }

        void OnDestroy()
        {
            Restore();
            if(heroes && heroes.Closed==bridgeClosed)
                heroes.Closed=originalClosed;
        }
    }
}
