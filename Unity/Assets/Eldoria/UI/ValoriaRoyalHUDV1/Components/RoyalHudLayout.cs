using UnityEngine;
namespace Eldoria.UI.RoyalGold {
 [ExecuteAlways] public sealed class RoyalHudLayout:MonoBehaviour {
  public RectTransform Resources,Mission,Feedback;float prior=-1;
  void LateUpdate(){var r=(RectTransform)transform;float w=r.rect.width;if(Mathf.Abs(w-prior)<.5f)return;prior=w;Apply(w);}
  public void Apply(float width){if(!Resources||!Mission||!Feedback||width<1)return;
   // Keep the complete resource strip clear of the realm block on 4:3 tablets.
   if(width>=1080){Resources.anchoredPosition=new Vector2(Mathf.Max(370,(width-570)/2)+285-width/2,-22);Mission.anchoredPosition=new Vector2(26,-158);Feedback.anchoredPosition=new Vector2(0,-122);}
   else{Resources.anchoredPosition=new Vector2(0,-132);Mission.anchoredPosition=new Vector2(26,-248);Feedback.anchoredPosition=new Vector2(0,-225);}
  }
 }
}
