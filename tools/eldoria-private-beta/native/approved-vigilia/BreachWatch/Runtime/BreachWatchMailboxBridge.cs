using Eldoria.ReferenceClean;
using Eldoria.UI.RoyalGold.V2;
using UnityEngine;
using UnityEngine.UI;

namespace Eldoria.BreachWatch
{
    /// <summary>
    /// Staged one-screen migration: reuses the actual persistent mailbox.
    /// The legacy MAIL *painting* is disabled by BreachWatchMailboxTheme;
    /// messages, actions, receipts and categories still come from the
    /// original authoritative controller. No second UI is shown underneath.
    /// This method is intentionally unlinked from first-chapter navigation
    /// until native portrait/landscape screenshots prove its quality.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class BreachWatchMailboxBridge : MonoBehaviour
    {
        ValoriaIntegratedWorld host;
        Canvas vigilia;
        RoyalGoldV41MailboxScreen mailbox;
        RectTransform window;
        Transform canonicalParent;
        int originalSibling;
        BreachWatchMailboxTheme theme;

        public bool IsShowing => window && window.gameObject.activeInHierarchy &&
            window.parent==vigilia.transform;
        public bool ReplacedOldPainting => theme && theme.IsStyled &&
            theme.OldBackdropHidden && theme.ReskinnedButtons>=3;

        public void Initialize(ValoriaIntegratedWorld source,Canvas presentation)
        {
            host=source;
            vigilia=presentation;
            mailbox=source && source.Hud
                ? source.Hud.GetComponent<RoyalGoldV41MailboxScreen>()
                : null;
        }

        public bool TryOpen()
        {
            if(!host || !host.Hud || !vigilia || !mailbox || IsShowing)
                return false;
            // Canonical Open guards story introduction, modal and narrative.
            mailbox.Open();
            if(!mailbox.IsOpen)return false;
            // Transform.Find treats literal slashes in the name as path
            // delimiters; this canonical destination uses a slash in its
            // actual GameObject name. Compare direct children explicitly.
            RectTransform real=null;
            foreach(Transform child in host.Hud.transform)
                if(child.name=="V41 / Buzón destination")
                {real=child as RectTransform;break;}
            if(!real)
            {
                mailbox.Close();
                return false;
            }
            canonicalParent=real.parent;
            originalSibling=real.GetSiblingIndex();
            theme=real.GetComponent<BreachWatchMailboxTheme>();
            if(!theme)theme=real.gameObject.AddComponent<BreachWatchMailboxTheme>();
            if(!theme.Apply(real) || !theme.OldBackdropHidden)
            {
                mailbox.Close();
                theme.Restore();
                canonicalParent=null;
                return false;
            }
            window=real;
            window.SetParent(vigilia.transform,false);
            window.anchorMin=Vector2.zero;
            window.anchorMax=Vector2.one;
            window.offsetMin=Vector2.zero;
            window.offsetMax=Vector2.zero;
            window.SetAsLastSibling();
            Canvas.ForceUpdateCanvases();
            mailbox.SynchronizeLayout();
            theme.SynchronizeVisual();
            return true;
        }

        public void SynchronizeVisual()
        {
            if(IsShowing && theme)theme.SynchronizeVisual();
        }

        void LateUpdate()
        {
            if(!window)return;
            if(!mailbox || !mailbox.IsOpen) Restore();
        }

        public void Close()
        {
            if(mailbox && mailbox.IsOpen)mailbox.Close();
            Restore();
        }

        void Restore()
        {
            if(theme)theme.Restore();
            if(window && canonicalParent)
            {
                window.SetParent(canonicalParent,false);
                window.SetSiblingIndex(Mathf.Clamp(originalSibling,0,
                    canonicalParent.childCount-1));
            }
            window=null;
            canonicalParent=null;
        }

        void OnDestroy() { Restore(); }
    }
}
