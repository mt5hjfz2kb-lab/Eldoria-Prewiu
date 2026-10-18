using UnityEngine;
namespace Eldoria.UI.RoyalGold {
 public sealed class PrimaryActionButton:MonoBehaviour {public RoyalButton Button;public RoyalText Label;
  public void Render(RoyalAction offer,bool pending){bool show=offer!=null&&!string.IsNullOrEmpty(offer.Kind);gameObject.SetActive(show);if(!show)return;Button.interactable=offer.Enabled&&!pending;Label.Put(offer.Label);}
 }
}
