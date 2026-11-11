using Eldoria.ReferenceClean;
using Eldoria.UI.RoyalGold.V2;
using UnityEngine;
using UnityEngine.UI;

namespace Eldoria.BreachWatch
{
    /// <summary>
    /// Moves the ONE canonical Vigilants/Scouts destination into Vigilia;
    /// preserves real time, combat progression, scouting choices, reward
    /// receipts and original saved state. Never creates a second screen.
    /// Staged until native screens and interactions pass artistic QA.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class BreachWatchRetentionBridge : MonoBehaviour
    {
        ValoriaIntegratedWorld host;
        Canvas canvas;
        RoyalGoldV3EarlyRetentionScreens retention;
        BreachWatchRetentionTheme theme;
        RectTransform root;
        Transform parent;
        int sibling;
        public bool IsShowing => retention && retention.IsOpen &&
            root && root.parent==canvas.transform;
        public bool ReplacedOldChrome => theme && theme.Installed &&
            theme.OldRoyalChromeHidden && IsShowing;

        public void Initialize(ValoriaIntegratedWorld source,Canvas presentation)
        {
            host=source;canvas=presentation;
            retention=source && source.Hud
                ?source.Hud.GetComponent<RoyalGoldV3EarlyRetentionScreens>()
                :null;
        }

        public bool TryOpen(RoyalGoldV3EarlyRetentionScreens.Destination destination)
        {
            if(!retention || !canvas || !host || IsShowing)return false;
            retention.Open(destination); // canonical Chapter I/II gate
            return retention.IsOpen && TryAdoptAlreadyOpen();
        }

        public bool TryAdoptAlreadyOpen()
        {
            if(!retention || !retention.IsOpen || !canvas || IsShowing)return false;
            var original=retention.VisibleScreen;
            var rect=original?original.transform as RectTransform:null;
            if(!rect)return false;
            parent=rect.parent;sibling=rect.GetSiblingIndex();
            root=rect;
            theme=root.GetComponent<BreachWatchRetentionTheme>();
            if(!theme)theme=root.gameObject.AddComponent<BreachWatchRetentionTheme>();
            if(!theme.Apply(root) || !theme.OldRoyalChromeHidden)
            {
                retention.Close();
                Restore();
                return false;
            }
            root.SetParent(canvas.transform,false);
            root.anchorMin=Vector2.zero;root.anchorMax=Vector2.one;
            root.offsetMin=Vector2.zero;root.offsetMax=Vector2.zero;
            root.SetAsLastSibling();
            Canvas.ForceUpdateCanvases();
            retention.SynchronizeLayout();
            return true;
        }

        void LateUpdate()
        {
            if(!retention || !canvas)return;
            // The canonical world can open these after an unlock without
            // invoking our new navigation. Adopt that same real screen.
            if(retention.IsOpen && !IsShowing)TryAdoptAlreadyOpen();
            else if(root && !retention.IsOpen)Restore();
        }

        public void Close()
        {
            if(retention && retention.IsOpen)retention.Close();
            Restore();
        }

        void Restore()
        {
            if(theme)theme.Restore();
            if(root && parent)
            {
                root.SetParent(parent,false);
                root.SetSiblingIndex(Mathf.Clamp(sibling,0,parent.childCount-1));
            }
            root=null;parent=null;
        }

        void OnDestroy() { Restore(); }
    }
}
