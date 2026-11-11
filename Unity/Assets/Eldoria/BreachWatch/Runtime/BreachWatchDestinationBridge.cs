using Eldoria.ReferenceClean;
using Eldoria.UI.RoyalGold;
using Eldoria.UI.RoyalGold.V2;
using UnityEngine;
using UnityEngine.UI;

namespace Eldoria.BreachWatch
{
    /// <summary>
    /// Reversible adapter for the REAL training and march screens.
    /// The destination's existing controllers, save gateway, actions and text
    /// are never duplicated. It moves only the visible RectTransform into
    /// the approved independent Canvas while that destination is open.
    ///
    /// Pilot only: do not expose a new navigation button until native
    /// snapshots and interaction tests approve each destination.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class BreachWatchDestinationBridge : MonoBehaviour
    {
        ValoriaIntegratedWorld host;
        Canvas vigilia;
        RoyalGoldV3ManagementScreens management;
        RectTransform destinationRoot;
        Transform originalParent;
        int originalSibling;
        BreachWatchSharedTheme theme;

        public bool IsShowing => destinationRoot &&
            destinationRoot.gameObject.activeInHierarchy &&
            destinationRoot.parent==vigilia.transform;
        public bool HasApprovedTheme => theme && theme.Installed &&
            theme.RestyledPanels==4 && theme.RestyledButtons>0 &&
            theme.NoLegacyBluePaintingVisible;

        public void Initialize(ValoriaIntegratedWorld source, Canvas canvas)
        {
            host=source;
            vigilia=canvas;
            management=host && host.Hud
                ? host.Hud.GetComponent<RoyalGoldV3ManagementScreens>()
                : null;
        }

        public bool TryOpen(RoyalGoldV3ManagementScreens.Destination destination)
        {
            if(!host || !vigilia || !management ||
               !host.CanOpen(RoyalSection.Army) || IsShowing)
                return false;

            // Canonical guards (chapters, modal, training availability)
            // remain owned by the existing destination controller.
            management.Open(destination);
            if(!management.IsOpen) return false;
            var root=management.VisibleRoot;
            if(!root) return false;

            originalParent=root.parent;
            originalSibling=root.GetSiblingIndex();
            theme=root.GetComponent<BreachWatchSharedTheme>();
            if(!theme)theme=root.gameObject.AddComponent<BreachWatchSharedTheme>();
            if(!theme.Apply(root) || !theme.NoLegacyBluePaintingVisible)
            {
                management.Close();
                Restore();
                return false;
            }

            destinationRoot=root;
            root.SetParent(vigilia.transform,false);
            root.anchorMin=Vector2.zero;
            root.anchorMax=Vector2.one;
            root.offsetMin=Vector2.zero;
            root.offsetMax=Vector2.zero;
            root.SetAsLastSibling();
            Canvas.ForceUpdateCanvases();
            management.SynchronizeLayout();
            return true;
        }

        void LateUpdate()
        {
            if(!destinationRoot)return;
            if(!management || !management.IsOpen)
            {
                Restore();
                return;
            }
            // The canonical destination's own Update handles Back/Escape,
            // live counts, sliders and action delivery. No parallel UI logic.
        }

        public void Close()
        {
            if(management && management.IsOpen)management.Close();
            Restore();
        }

        void Restore()
        {
            if(theme)theme.Restore();
            if(destinationRoot && originalParent)
            {
                destinationRoot.SetParent(originalParent,false);
                destinationRoot.SetSiblingIndex(
                    Mathf.Clamp(originalSibling,0,
                        originalParent.childCount-1));
            }
            destinationRoot=null;
            originalParent=null;
        }

        void OnDestroy() { Restore(); }
    }
}
