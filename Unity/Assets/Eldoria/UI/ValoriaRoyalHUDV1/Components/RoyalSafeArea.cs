using UnityEngine;
namespace Eldoria.UI.RoyalGold {
 [ExecuteAlways] public sealed class RoyalSafeArea:MonoBehaviour {
  Rect prior;int width,height;
  void OnEnable(){Apply();}void Update(){if(prior!=Screen.safeArea||width!=Screen.width||height!=Screen.height)Apply();}
  void Apply(){if(Screen.width<1||Screen.height<1)return;width=Screen.width;height=Screen.height;prior=Screen.safeArea;var r=(RectTransform)transform;r.anchorMin=new Vector2(Mathf.Clamp01(prior.xMin/width),Mathf.Clamp01(prior.yMin/height));r.anchorMax=new Vector2(Mathf.Clamp01(prior.xMax/width),Mathf.Clamp01(prior.yMax/height));r.offsetMin=r.offsetMax=Vector2.zero;}
 }
}
