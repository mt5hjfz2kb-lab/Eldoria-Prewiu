using UnityEngine;

namespace Eldoria.Presentation
{
 public static class ValoriaMobileNavigableCityV1
 {
  public const float PortraitAspectThreshold=0.72f;
  public const float MobileHomeOrthographicSize=12.2f;
  public static readonly Vector3 CanonicalPosition=new Vector3(18.2f,14.6f,-25.8f);
  public static readonly Vector3 CanonicalTarget=new Vector3(0f,3.35f,5.6f);
  public const float CanonicalOrthographicSize=9.1f;
  public static readonly Vector3 MobileHomeOffset=new Vector3(1.2f,0f,-2.8f);

  public static bool UsesMobileHome(float aspect)=>aspect>0f&&aspect<=PortraitAspectThreshold;

  public static void ApplyHomePose(Camera camera,float aspect)
  {
   if(camera==null)throw new System.ArgumentNullException(nameof(camera));
   camera.orthographic=true;
   if(UsesMobileHome(aspect))
   {
    camera.transform.position=CanonicalPosition+MobileHomeOffset;
    camera.transform.LookAt(CanonicalTarget+MobileHomeOffset);
    camera.orthographicSize=MobileHomeOrthographicSize;
   }
   else
   {
    camera.transform.position=CanonicalPosition;
    camera.transform.LookAt(CanonicalTarget);
    camera.orthographicSize=CanonicalOrthographicSize;
   }
  }

  public static Vector2 PanHalfExtents(int bastionLevel,float aspect)
  {
   float portraitExtra=Mathf.Clamp((.80f-aspect)*8f,0f,2.5f);
   if(bastionLevel<=10)return new Vector2(13.5f+portraitExtra,7f);
   if(bastionLevel<=15)return new Vector2(15f+portraitExtra,9f);
   if(bastionLevel<=20)return new Vector2(17.5f+portraitExtra,11f);
   if(bastionLevel<=25)return new Vector2(20f+portraitExtra,13f);
   return new Vector2(23f+portraitExtra,16f);
  }

  public static Vector3 ClampToEnvelope(Vector3 home,Vector3 desired,int bastionLevel,float aspect)
  {
   var half=PanHalfExtents(bastionLevel,aspect);
   var offset=desired-home;
   offset.x=Mathf.Clamp(offset.x,-half.x,half.x);
   offset.z=Mathf.Clamp(offset.z,-half.y,half.y);
   return new Vector3(home.x+offset.x,home.y,home.z+offset.z);
  }
 }
}
