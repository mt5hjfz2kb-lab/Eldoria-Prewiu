using System;
using UnityEngine;

namespace Eldoria.UI.RoyalGold
{
    /// <summary>
    /// The original crest, title and Power remain the only three HUD pieces.
    /// An optional presentation-only title binding never mutates the kingdom
    /// or the save. Resolve it IN Render so periodic RoyalHUD.Refresh cannot
    /// overwrite the player's nickname for a frame before LateUpdate.
    /// </summary>
    public sealed class RealmHeader : MonoBehaviour
    {
        public RoyalText Realm, Power;
        public RoyalButton Info;
        public Func<string, string> DisplayTitleOverride;
        public string CurrentRealmTitle { get; private set; } = "Valoria";

        public void Render(RoyalSnapshot snapshot, bool english)
        {
            CurrentRealmTitle = snapshot.Realm ?? "";
            string title = DisplayTitleOverride != null
                ? DisplayTitleOverride(CurrentRealmTitle)
                : CurrentRealmTitle;
            Realm.Put(string.IsNullOrWhiteSpace(title) ? CurrentRealmTitle : title);
            Power.Put(RoyalFormatting.L(english, "Poder · ", "Power · ") +
                RoyalFormatting.Amount(snapshot.Power, english));
        }
    }
}
