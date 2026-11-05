using System;
using Eldoria.UI.RoyalGold;
using UnityEngine;

namespace Eldoria.PlayerClarity
{
    /// <summary>
    /// Reuse the existing two-line crest header: the REAL kingdom title slot
    /// becomes the saved character nickname, while the existing Power label
    /// stays exactly where its approved prefab places it. Never add a third
    /// overlay line, duplicate Canvas, or touch other HUD layout anchors.
    /// The kingdom's actual identifier/name still exists in PlayerState
    /// and the realm panel; this only changes its small header caption.
    /// </summary>
    public sealed class PlayerNameHeader : MonoBehaviour
    {
        private RealmHeader header;
        private Func<string> read;
        private string displayedName;

        public void Initialize(RealmHeader source, Func<string> readName)
        {
            if (!source) throw new ArgumentNullException(nameof(source));
            if (readName == null) throw new ArgumentNullException(nameof(readName));
            header = source;
            read = readName;
            if (!header.Realm || !header.Power)
                throw new InvalidOperationException("Canonical crest text and power must exist");

            // A maximum-length nickname must stay on ONE readable line
            // without colliding with the Power label or portrait resources.
            header.Realm.resizeTextForBestFit = true;
            header.Realm.resizeTextMinSize = 12;
            header.Realm.resizeTextMaxSize = 30;
            header.Realm.horizontalOverflow = HorizontalWrapMode.Wrap;
            header.Realm.verticalOverflow = VerticalWrapMode.Truncate;
            Refresh();
        }

        private void LateUpdate() => Refresh();

        public void Refresh()
        {
            if (!header || !header.Realm || read == null) return;
            string player = read() ?? "";
            if (string.IsNullOrWhiteSpace(player))
            {
                // On a new/legacy save, the default header continues to
                // display the normal kingdom name until the player chooses.
                if (displayedName != null)
                {
                    header.Realm.Put("Valoria");
                    displayedName = null;
                }
                return;
            }
            // RoyalHUD.Refresh periodically renders the actual realm name.
            // Override just its existing title slot in LateUpdate; do not
            // rewrite Power or add overlapping screen-space graphics.
            if (!string.Equals(header.Realm.text, player, StringComparison.Ordinal))
                header.Realm.Put(player);
            displayedName = player;
        }
    }
}
