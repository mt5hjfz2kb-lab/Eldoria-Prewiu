using UnityEngine;

namespace Eldoria.Presentation
{
 public static class ValoriaScreenSpaceBreakpointV1
 {
  public const string GraneroName="Valoria · PFS v1 · Granero rich camera-first";
  public const string CuartelName="Valoria · PFS v1 · Cuartel rich camera-first";

  // Variant C only: bounded presentation-space cleanup. No source, parcel, material or gameplay changes.
  public static void ApplyVariantC()
  {
   var granero=GameObject.Find(GraneroName);
   var cuartel=GameObject.Find(CuartelName);
   if(granero==null||cuartel==null)throw new System.InvalidOperationException("Screen-space C requires paired functional sources");

   granero.transform.position+=new Vector3(.45f,0f,.30f);
   granero.transform.rotation=Quaternion.Euler(0f,-6f,0f)*granero.transform.rotation;

   cuartel.transform.position+=new Vector3(-.80f,0f,.55f);
   cuartel.transform.rotation=Quaternion.Euler(0f,8f,0f)*cuartel.transform.rotation;
   Physics.SyncTransforms();
  }
 }
}
