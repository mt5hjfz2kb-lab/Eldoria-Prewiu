using Eldoria.UI.RoyalGold;
using UnityEngine;
using UnityEngine.UI;

namespace Eldoria.PlayerClarity
{
    /// <summary>Native top-left nameplate beside the original crest.
    /// "Valoria" stays the realm name and the true power is unchanged.
    /// Only call SetName with the reread canonical PlayerState value.</summary>
    public sealed class RoyalPlayerNameplate : MonoBehaviour
    {
        private RealmHeader header;
        private RoyalText nickname;
        private bool initialized;

        public static RoyalPlayerNameplate Attach(RoyalHUD hud)
        {
            if (!hud || !hud.Realm) return null;
            var existing = hud.Realm.GetComponent<RoyalPlayerNameplate>();
            if (existing) return existing;
            var plate = hud.Realm.gameObject.AddComponent<RoyalPlayerNameplate>();
            plate.Initialize(hud.Realm);
            return plate;
        }

        private void Initialize(RealmHeader source)
        {
            if (initialized) return;
            header = source;
            var root = (RectTransform)header.transform;
            var go = new GameObject("Player name / beside original shield",
                typeof(RectTransform), typeof(RoyalText));
            var rect = (RectTransform)go.transform;
            rect.SetParent(root, false);
            rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(0, 1);
            rect.anchoredPosition = new Vector2(102f, -37f);
            rect.sizeDelta = new Vector2(220f, 23f);
            nickname = go.GetComponent<RoyalText>();
            var serif = Resources.Load<Font>("EldoriaRetention/reference-serif");
            if (serif) nickname.font = serif;
            nickname.alignment = TextAnchor.MiddleLeft;
            nickname.fontSize = 18;
            nickname.color = new Color(.90f, .74f, .48f, 1f);
            nickname.raycastTarget = false;
            nickname.horizontalOverflow = HorizontalWrapMode.Wrap;
            nickname.verticalOverflow = VerticalWrapMode.Truncate;

            // Reuse the real Realm and Power labels rather than creating
            // a competing header or hiding the realm's identity.
            var realm = (RectTransform)header.Realm.transform;
            realm.anchoredPosition = new Vector2(102, -9);
            realm.sizeDelta = new Vector2(215, 24);
            var power = (RectTransform)header.Power.transform;
            power.anchoredPosition = new Vector2(102, -64);
            power.sizeDelta = new Vector2(215, 26);
            initialized = true;
            nickname.Put("");
        }

        public void SetName(string persistedName)
        {
            if (!initialized || !nickname) return;
            nickname.Put(persistedName ?? "");
        }
    }
}
