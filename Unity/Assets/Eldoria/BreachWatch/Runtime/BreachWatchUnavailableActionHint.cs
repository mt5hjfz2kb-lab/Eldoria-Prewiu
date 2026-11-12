using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Eldoria.BreachWatch
{
    /// <summary>
    /// A disabled building action must still explain WHY it is blocked.
    /// This is an independent input-only component and never submits
    /// commands; the actual RoyalAction.Reason comes from the canonical game.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class BreachWatchUnavailableActionHint : MonoBehaviour, IPointerClickHandler
    {
        BreachWatchHud owner;
        Button actionButton;

        public void Initialize(BreachWatchHud source,Button button)
        {
            owner=source;
            actionButton=button;
        }

        public void OnPointerClick(PointerEventData pointer)
        {
            // The normal Button submits only enabled actions. This listener
            // handles the disabled case without making a fake upgrade possible.
            if(!owner || !actionButton || actionButton.interactable)return;
            owner.ExplainUnavailableAction();
        }
    }
}
