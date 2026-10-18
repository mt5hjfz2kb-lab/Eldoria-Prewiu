using UnityEngine;
namespace Eldoria.UI.RoyalGold {
 public sealed class PrimaryActionButton:MonoBehaviour {
  public RoyalButton Button;public RoyalText Label;
  CanvasGroup visibility;
  public bool Visible {get;private set;}
  void Awake(){EnsureGroup();}
  void EnsureGroup(){
   if(!visibility)visibility=GetComponent<CanvasGroup>();
   if(!visibility)visibility=gameObject.AddComponent<CanvasGroup>();
  }
  public void Render(RoyalAction offer,bool pending){
   EnsureGroup();
   Visible=offer!=null&&!string.IsNullOrEmpty(offer.Kind);
   // Keep the same live uGUI Graphic registered with the Canvas through
   // training/construction transitions. Deactivating its GameObject in the
   // previous version left CanvasRenderer.absoluteDepth=-1 on reactivation,
   // making an enabled visible button unclickable behind its panel graphic.
   if(!gameObject.activeSelf)gameObject.SetActive(true);
   visibility.alpha=Visible?1f:.001f;
   visibility.blocksRaycasts=Visible;
   visibility.interactable=Visible;
   Button.interactable=Visible&&offer.Enabled&&!pending;
   if(Visible)Label.Put(offer.Label);
  }
 }
}
