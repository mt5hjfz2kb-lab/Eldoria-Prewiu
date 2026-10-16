using UnityEngine;
using UnityEngine.InputSystem;
namespace Eldoria.Breakthrough {
[RequireComponent(typeof(Camera))]
public sealed class SceneCamera:MonoBehaviour {
 Camera view;Vector3 home;Vector3 offset;float baseSize;Vector2 pressAt;string selected="";bool ownsPress;
 public string Selected=>selected;
 void Awake(){view=GetComponent<Camera>();home=transform.position;baseSize=view.orthographicSize;}
 public void Pan(Vector2 pixels){if(view==null)Awake();float unit=2*view.orthographicSize/Mathf.Max(1,view.pixelHeight);offset-=Vector3.ProjectOnPlane(transform.right*pixels.x+transform.up*pixels.y,Vector3.up)*unit;offset.x=Mathf.Clamp(offset.x,-12,12);offset.z=Mathf.Clamp(offset.z,-12,12);transform.position=home+offset;}
 public void Zoom(float d){if(view==null)Awake();view.orthographicSize=Mathf.Clamp(view.orthographicSize+d,14,28);}
 public void Home(){if(view==null)Awake();offset=Vector3.zero;transform.position=home;view.orthographicSize=baseSize;}
 public bool SelectAt(Vector2 pixel){if(view==null)Awake();if(Physics.Raycast(view.ScreenPointToRay(pixel),out var hit,400,~0,QueryTriggerInteraction.Collide)){selected=hit.collider.name;return true;}selected="";return false;}
 void Update(){
  var t=Touchscreen.current;
  if(t!=null&&t.primaryTouch.press.isPressed){
   UnityEngine.InputSystem.Controls.TouchControl a=null,b=null;
   foreach(var touch in t.touches)if(touch.press.isPressed){if(a==null)a=touch;else{b=touch;break;}}
   if(a!=null&&b!=null){float now=Vector2.Distance(a.position.ReadValue(),b.position.ReadValue());float before=Vector2.Distance(a.position.ReadValue()-a.delta.ReadValue(),b.position.ReadValue()-b.delta.ReadValue());Zoom(-(now-before)*view.orthographicSize/Mathf.Max(1,view.pixelHeight));}
   else if(a!=null){if(a.press.wasPressedThisFrame){pressAt=a.position.ReadValue();ownsPress=true;}Pan(a.delta.ReadValue());}
  } else {
   if(ownsPress){var p=t.primaryTouch.position.ReadValue();if(Vector2.Distance(p,pressAt)<10)SelectAt(p);ownsPress=false;}
   var m=Mouse.current;if(m!=null){if(m.leftButton.wasPressedThisFrame)pressAt=m.position.ReadValue();if(m.leftButton.isPressed)Pan(m.delta.ReadValue());if(m.leftButton.wasReleasedThisFrame&&Vector2.Distance(pressAt,m.position.ReadValue())<7)SelectAt(m.position.ReadValue());Zoom(-m.scroll.ReadValue().y*.015f);}
  }
  if(Keyboard.current!=null&&Keyboard.current.homeKey.wasPressedThisFrame)Home();
 }
 void OnGUI(){GUI.Label(new Rect(15,15,450,28),"Bastion I - isolated visual experiment");if(selected.Length>0)GUI.Label(new Rect(15,43,450,28),selected);}
}
}