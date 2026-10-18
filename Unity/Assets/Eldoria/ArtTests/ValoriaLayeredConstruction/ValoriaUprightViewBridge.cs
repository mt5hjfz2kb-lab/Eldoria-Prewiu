// Experimental coordinate conversion only. The canonical controller and its lens,
// HOME, pan limits and zoom code remain unchanged. Native world stays Unity Y-up.
using UnityEngine;
using Eldoria.Presentation;
namespace Eldoria.ValoriaLayered {
 [ExecuteAlways]
 public sealed class ValoriaUprightViewBridge : MonoBehaviour {
  public Camera CanonicalInput;
  public Camera RenderCamera;
  public Vector3 NativeOrigin;
  public Quaternion NativeRotation=Quaternion.identity;
  public void Sync(){
   if(CanonicalInput==null||RenderCamera==null)return;
   RenderCamera.transform.SetPositionAndRotation(NativeOrigin+NativeRotation*CanonicalInput.transform.position,
      NativeRotation*CanonicalInput.transform.rotation);
   RenderCamera.fieldOfView=CanonicalInput.fieldOfView;
   RenderCamera.orthographic=false;
   // Existing SlicePresenter reads this camera for projection/picking while its
   // transform continues to hold canonical pan input coordinates.
   CanonicalInput.aspect=RenderCamera.aspect;
   CanonicalInput.worldToCameraMatrix=RenderCamera.worldToCameraMatrix;
  }
  void LateUpdate(){Sync();}
 }
}
