using UnityEngine;

namespace Eldoria.Presentation
{
 public static class ValoriaAdaptiveMobileHomePoseV1
 {
  public enum Profile { M0Current, M1CenterAdapted, M2CenterAndSize }

  public static readonly Vector3 CanonicalPosition=new Vector3(18.2f,14.6f,-25.8f);
  public static readonly Vector3 CanonicalTarget=new Vector3(0f,3.35f,5.6f);
  public const float CanonicalOrthographicSize=9.1f;

  // Camera and target move together, preserving the canonical orthographic viewing direction.
  public static readonly Vector3 MobileCenterOffset=new Vector3(1.2f,0f,-2.8f);
  public const float MobileOrthographicSize=12.2f;
  public const float PortraitAspectThreshold=0.72f;

  public static void Apply(Camera camera,Profile profile)
  {
   if(camera==null)throw new System.ArgumentNullException(nameof(camera));
   camera.orthographic=true;
   var offset=profile==Profile.M0Current?Vector3.zero:MobileCenterOffset;
   camera.transform.position=CanonicalPosition+offset;
   camera.transform.LookAt(CanonicalTarget+offset);
   camera.orthographicSize=profile==Profile.M2CenterAndSize?MobileOrthographicSize:CanonicalOrthographicSize;
  }

  public static bool ShouldUseMobileHomePose(float aspect)
   => aspect>0f&&aspect<=PortraitAspectThreshold;

  public static void ApplyRuntimeHomePose(Camera camera,float aspect)
  {
   Apply(camera,ShouldUseMobileHomePose(aspect)?Profile.M2CenterAndSize:Profile.M0Current);
  }
 }
}
