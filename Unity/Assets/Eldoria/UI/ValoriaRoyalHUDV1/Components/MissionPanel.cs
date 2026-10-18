using UnityEngine;
namespace Eldoria.UI.RoyalGold {
 public sealed class MissionPanel:MonoBehaviour {
  public RoyalText Chapter,Objective,ProgressText;public RectTransform Fill,Details;public RoyalButton Toggle,Open;bool collapsed;
  void OnEnable(){if(Toggle)Toggle.onClick.AddListener(Switch);}
  void OnDisable(){if(Toggle)Toggle.onClick.RemoveListener(Switch);}
  void Switch(){collapsed=!collapsed;if(Details)Details.gameObject.SetActive(!collapsed);var r=(RectTransform)transform;r.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical,collapsed?70:208);}
  public void Render(RoyalSnapshot s){Chapter.Put(s.Chapter);Objective.Put(s.Objective);ProgressText.Put(s.ObjectiveProgress);Fill.anchorMax=new Vector2(Mathf.Clamp01(s.MissionFraction),1);}
 }
}
