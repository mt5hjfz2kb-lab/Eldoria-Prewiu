using System.Reflection;
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
        // Canonical Retention.Layout reads its PRIVATE 'safe' RectTransform.
        // After moving the real screen to an independent Canvas, the old
        // disabled RoyalHudLayout still advertises landscape dimensions.
        // Retarget ONLY this presentation field while the destination is
        // visible. Never reparent the canonical safe area or mutate gameplay.
        FieldInfo actualSafeField;
        RectTransform canonicalSafe;
        bool viewportRedirected;
        public bool IsShowing => retention && retention.IsOpen &&
            root && root.parent==canvas.transform;
        public bool ReplacedOldChrome => theme && theme.Installed &&
            theme.OldRoyalChromeHidden && IsShowing && viewportRedirected;
        public RectTransform LiveScreenRect => IsShowing?root:null;
        public bool HasSingleGradedScoutsPainting => IsShowing &&
            retention.OpenDestination == RoyalGoldV3EarlyRetentionScreens.Destination.Scouts &&
            theme && theme.HasIndependentScoutColorGrade;
        public bool OwnsActualSafeViewport => viewportRedirected &&
            actualSafeField!=null && retention &&
            ReferenceEquals(actualSafeField.GetValue(retention),root);

        public void Initialize(ValoriaIntegratedWorld source,Canvas presentation)
        {
            host=source;canvas=presentation;
            retention=source && source.Hud
                ?source.Hud.GetComponent<RoyalGoldV3EarlyRetentionScreens>()
                :null;
            actualSafeField=typeof(RoyalGoldV3EarlyRetentionScreens).GetField(
                "safe",BindingFlags.NonPublic|BindingFlags.Instance);
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
            if(!theme.Apply(root,retention.OpenDestination ==
                RoyalGoldV3EarlyRetentionScreens.Destination.Scouts) ||
               !theme.OldRoyalChromeHidden)
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
            // The source component still uses the abandoned RoyalHUD safe
            // rect for all portrait/landscape calculations. Temporarily
            // substitute the NEW, correctly sized screen viewport. Without
            // this, the native 900x1600 screenshot renders a 1600x900-wide
            // panel cropped halfway off the phone despite green click tests.
            if(actualSafeField==null ||
               actualSafeField.FieldType!=typeof(RectTransform))
            {
                Debug.LogError("VIGILIA_RETENTION_SAFE_VIEWPORT_API_CHANGED");
                retention.Close();Restore();return false;
            }
            canonicalSafe=actualSafeField.GetValue(retention) as RectTransform;
            if(!canonicalSafe)
            {
                Debug.LogError("VIGILIA_RETENTION_CANONICAL_SAFE_MISSING");
                retention.Close();Restore();return false;
            }
            actualSafeField.SetValue(retention,root);
            viewportRedirected=true;
            Canvas.ForceUpdateCanvases();
            retention.SynchronizeLayout();
            return OwnsActualSafeViewport && theme.OldRoyalChromeHidden;
        }

        public void SynchronizeVisual()
        {
            if(!IsShowing || !OwnsActualSafeViewport)return;
            // Unity's offscreen native QA changes target resolution in one
            // frame: apply the actual portrait/landscape layout immediately.
            // The canonical Update() does this normally on real devices.
            Canvas.ForceUpdateCanvases();
            retention.SynchronizeLayout();
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
            if(viewportRedirected && retention && actualSafeField!=null &&
               canonicalSafe)
                actualSafeField.SetValue(retention,canonicalSafe);
            canonicalSafe=null;
            viewportRedirected=false;
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
