using UnityEngine;
using UnityEngine.InputSystem;
namespace Eldoria.Breakthrough {
[RequireComponent(typeof(Camera))]
public sealed class SceneCamera:MonoBehaviour {
 Camera view;Vector3 home;Vector3 offset;float baseSize;Vector2 start;bool dragging;
 void Awake(){view=GetComponent<Camera>();home=transform.position;baseSize=view.orthographicSize;}
 public void Pan(Vector2 pixels){float unit=2*view.orthographicSize/Mathf.Max(1,view.pixelHeight);offset-=Vector3.ProjectOnPlane(transform.right*pixels.x+transform.up*pixels.y,Vector3.up)*unit;offset.x=Mathf.Clamp(offset.x,-12,12);offset.z=Mathf.Clamp(offset.z,-12,12);transform.position=home+offset;}
 public void Zoom(float d){view.orthographicSize=Mathf.Clamp(view.orthographicSize+d,14,28);}
 public void Home(){offset=Vector3.zero;transform.position=home;view.orthographicSize=baseSize;}
 void Update(){
  var m=Mouse.current;if(m!=null){if(m.leftButton.isPressed)Pan(m.delta.ReadValue());Zoom(-m.scroll.ReadValue().y*.015f);}
  var t=Touchscreen.current;if(t!=null){var first=t.primaryTouch;if(first.press.isPressed)Pan(first.delta.ReadValue());}
  if(Keyboard.current!=null&&Keyboard.current.homeKey.wasPressedThisFrame)Home();
 }
}
}