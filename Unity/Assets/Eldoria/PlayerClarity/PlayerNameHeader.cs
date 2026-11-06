using System;
using Eldoria.UI.RoyalGold;
using UnityEngine;

namespace Eldoria.PlayerClarity
{
    /// <summary>
    /// Bind the persisted nickname to the EXISTING crest title slot.
    /// Power stays in its original second row, and no extra UGUI text,
    /// overlay or save authority is introduced. The title is resolved during
    /// RoyalHUD.Render so it remains correct in Update, LateUpdate, offscreen
    /// screenshots, portrait orientation and a reload of the same player.
    /// </summary>
    public sealed class PlayerNameHeader : MonoBehaviour
    {
        private RealmHeader header;
        private Func<string> read;
        private Func<string, string> boundTitle;
        private string displayedName;

        public void Initialize(RealmHeader source, Func<string> readName)
        {
            if (!source) throw new ArgumentNullException(nameof(source));
            if (readName == null) throw new ArgumentNullException(nameof(readName));
            if (!source.Realm || !source.Power)
                throw new InvalidOperationException("Original crest and Power labels are required");
            if (header && header != source &&
                ReferenceEquals(header.DisplayTitleOverride, boundTitle))
                header.DisplayTitleOverride = null;
            header = source;
            read = readName;
            if (boundTitle == null) boundTitle = ResolveTitle;
            header.DisplayTitleOverride = boundTitle;
            header.Realm.resizeTextForBestFit = true;
            header.Realm.resizeTextMinSize = 12;
            header.Realm.resizeTextMaxSize = 30;
            header.Realm.horizontalOverflow = HorizontalWrapMode.Wrap;
            header.Realm.verticalOverflow = VerticalWrapMode.Truncate;
            Refresh();
        }

        private string ResolveTitle(string originalRealm)
        {
            string player = read != null ? read() : null;
            return string.IsNullOrWhiteSpace(player) ? originalRealm : player;
        }

        private void LateUpdate() => Refresh();

        public void Refresh()
        {
            if (!header || !header.Realm || read == null) return;
            string resolved = ResolveTitle(header.CurrentRealmTitle);
            if (!string.Equals(header.Realm.text, resolved, StringComparison.Ordinal))
                header.Realm.Put(resolved);
            displayedName = resolved;
        }

        private void OnDestroy()
        {
            if (!header) return;
            if (ReferenceEquals(header.DisplayTitleOverride, boundTitle))
                header.DisplayTitleOverride = null;
            if (header.Realm) header.Realm.Put(header.CurrentRealmTitle);
        }
    }
}
