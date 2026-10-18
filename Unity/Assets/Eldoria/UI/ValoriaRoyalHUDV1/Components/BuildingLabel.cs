using UnityEngine;
namespace Eldoria.UI.RoyalGold {
 public sealed class BuildingLabel:MonoBehaviour {
  public RoyalText Label;public RoyalButton Select;public Transform Target;public Camera PlayerCamera;public RectTransform Layer;public Vector3 WorldOffset=new Vector3(0,2,0);
  public void Bind(string title,Transform target,Camera camera,RectTransform layer){Target=target;PlayerCamera=camera;Layer=layer;Label.Put(title);}
  void LateUpdate(){if(!Target||!PlayerCamera||!Layer){if(Select)Select.gameObject.SetActive(false);return;}var p=PlayerCamera.WorldToScreenPoint(Target.position+WorldOffset);bool visible=p.z>0&&p.x>=0&&p.y>=0&&p.x<=Screen.width&&p.y<=Screen.height;Select.gameObject.SetActive(visible);if(visible&&RectTransformUtility.ScreenPointToLocalPointInRectangle(Layer,p,null,out var local))((RectTransform)Select.transform).anchoredPosition=local;}
 }
}
