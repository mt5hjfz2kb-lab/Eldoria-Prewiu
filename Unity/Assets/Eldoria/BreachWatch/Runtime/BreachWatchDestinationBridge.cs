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
        BreachWatchTrainingComposition cinematicTraining;
        BreachWatchMarchComposition cinematicMarch;

        public bool IsShowing => destinationRoot &&
            destinationRoot.gameObject.activeInHierarchy &&
            destinationRoot.parent==vigilia.transform;
        public bool HasApprovedTheme => theme && theme.Installed &&
            theme.RestyledPanels==4 && theme.RestyledButtons>0 &&
            theme.NoLegacyBluePaintingVisible;
        public bool RealMarchFullWidthScorecard => cinematicMarch &&
            cinematicMarch.UsesFullWidthMetrics;
        public bool RealMarchThreeSpaceRoster => cinematicMarch &&
            cinematicMarch.UsesThreeHeroSpaces;
        public bool RealTrainingCinematic => cinematicTraining &&
            cinematicTraining.IsCinematicPortrait;
        public string NativeTrainingLayoutDiagnostic => cinematicTraining
            ?cinematicTraining.NativeLayoutDiagnostic:"training-component=null";
        public string NativeThemeDiagnostic => theme
            ? "installed="+theme.Installed+
              " panels="+theme.RestyledPanels+
              " buttons="+theme.RestyledButtons+
              " replaced="+theme.ReplacedLegacyGraphics+
              " firstOld="+theme.FirstVisibleOldPainting
            : "theme=null";

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
            if(host && host.WorldEntryPending && !host.TrySetWorld(false))return false;
            management.Open(destination);
            if(!management.IsOpen) return false;
            return TryAdoptAlreadyOpen();
        }

        /// <summary>
        /// The canonical Heroes.Close handler may reopen the real Training or
        /// March controller itself. Adopt that EXISTING open destination rather
        /// than running its command twice or leaving it on the hidden Canvas.
        /// </summary>
        public bool TryAdoptAlreadyOpen()
        {
            if(!host || !vigilia || !management ||
               !management.IsOpen || IsShowing ||
               !host.CanOpen(RoyalSection.Army))
                return false;
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
            // Source controller recalculates and may re-enable its original
            // blue paintings. Turn those OFF again after the real reflow.
            theme.ReassertSinglePainting();
            cinematicTraining=root.GetComponent<BreachWatchTrainingComposition>();
            if(!cinematicTraining)
                cinematicTraining=root.gameObject.AddComponent<BreachWatchTrainingComposition>();
            cinematicTraining.Initialize(management);
            cinematicTraining.RefreshPortrait();
            cinematicMarch=root.GetComponent<BreachWatchMarchComposition>();
            if(!cinematicMarch)
                cinematicMarch=root.gameObject.AddComponent<BreachWatchMarchComposition>();
            cinematicMarch.Initialize(management);
            cinematicMarch.RefreshScorecard();
            return theme.NoLegacyBluePaintingVisible;
        }

        // Native offscreen capture changes render-target orientation without
        // a normal Unity LateUpdate frame. Apply exactly the same post-Reflow
        // visual contract as the live scene, or screenshots misrepresent it.
        public void SynchronizeVisual()
        {
            if(!IsShowing)return;
            if(theme)theme.ReassertSinglePainting();
            if(cinematicTraining)cinematicTraining.RefreshPortrait();
            if(cinematicMarch)cinematicMarch.RefreshScorecard();
        }

        void LateUpdate()
        {
            if(!destinationRoot)
            {
                // Real building commands may open Training/March directly
                // through the existing gameplay controller. Adopt that same
                // screen; never leave it invisible on the disabled old HUD.
                if(management && management.IsOpen && host &&
                   host.CanOpen(RoyalSection.Army))
                    TryAdoptAlreadyOpen();
                return;
            }
            if(!management || !management.IsOpen)
            {
                Restore();
                return;
            }
            SynchronizeVisual();
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
            if(cinematicTraining)cinematicTraining.Restore();
            if(cinematicMarch)cinematicMarch.Restore();
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
