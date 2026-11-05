using System;
using Eldoria.UI.RoyalGold;
using UnityEngine;
using UnityEngine.UI;

namespace Eldoria.PlayerClarity
{
    /// <summary>
    /// Canonical RoyalHUD first-entry coordinator. Actual PlayerState and
    /// LocalGateway callbacks are supplied by Valoria's owner when its
    /// economy workstream releases those files. Never uses PlayerPrefs or
    /// takes over the existing narrative/quest controller.
    /// </summary>
    [DefaultExecutionOrder(3200)]
    public sealed class RoyalPlayerEntryCoordinator : MonoBehaviour
    {
        private RoyalHUD hud;
        private RoyalPlayerNamePrompt prompt;
        private Func<string> savedName;
        private Func<string,bool> persist;
        private Func<bool> english;
        private float next;
        private PlayerNameHeader headerView;

        public bool IsBlockingNarrative => prompt && prompt.IsOpen;
        public bool HasSavedName => savedName != null && !string.IsNullOrWhiteSpace(savedName());

        public void Initialize(RoyalHUD source,Func<string> readCanonicalName,
            Func<string,bool> commitCanonicalName,Func<bool> readEnglish)
        {
            if (!source || !source.transform) throw new ArgumentNullException(nameof(source));
            if (readCanonicalName == null) throw new ArgumentNullException(nameof(readCanonicalName));
            if (commitCanonicalName == null) throw new ArgumentNullException(nameof(commitCanonicalName));
            if (readEnglish == null) throw new ArgumentNullException(nameof(readEnglish));
            hud=source;
            savedName=readCanonicalName;
            persist=commitCanonicalName;
            english=readEnglish;
            if (!prompt) prompt=RoyalPlayerNamePrompt.Create((RectTransform)hud.transform);
            if (hud.Realm)
            {
                headerView = hud.Realm.GetComponent<PlayerNameHeader>();
                if (!headerView) headerView = hud.Realm.gameObject.AddComponent<PlayerNameHeader>();
                headerView.Initialize(hud.Realm, savedName);
            }
            Refresh();
        }

        private void LateUpdate()
        {
            if (Time.unscaledTime < next) return;
            next=Time.unscaledTime+.18f;
            Refresh();
        }

        public void Refresh()
        {
            if (!hud || savedName==null) return;
            string name=savedName() ?? "";
            if (string.IsNullOrWhiteSpace(name))
            {
                if (prompt && !prompt.IsOpen)
                    prompt.Open(english!=null && english(), CommitAndReadBack);
                return;
            }
            if (prompt && prompt.IsOpen) prompt.gameObject.SetActive(false);
            // Keep Valoria as the kingdom and preserve genuine power.
            // The unique real crest header owns the player name, never
            // a duplicate floating HUD panel or a second Canvas.
            if (headerView) headerView.Refresh();
        }

        private bool CommitAndReadBack(string normalized)
        {
            if (persist==null || savedName==null) return false;
            try
            {
                if (!persist(normalized)) return false;
                if (!string.Equals(savedName(),normalized,StringComparison.Ordinal))return false;
                if (headerView) headerView.Refresh();
                return true;
            }
            catch(Exception ex)
            {
                Debug.LogError("ELDORIA_PLAYER_NAME_SAVE_FAILED: "+ex.Message);
                return false;
            }
        }
    }
}
