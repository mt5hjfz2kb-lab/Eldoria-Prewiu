using System;
using System.IO;
using UnityEngine;
namespace Eldoria.ReferenceClean.Editor {
public static class ValoriaReferenceCleanWaterTrial {
 [Serializable]class Report{public string source_sha,scope;public bool same_camera,same_lighting,same_geometry;}
 static string Stamp(Camera c)=>c.transform.position.ToString("R")+c.transform.rotation.ToString("R")+c.orthographicSize+"/"+c.aspect;
 public static void Capture(Camera cam,Renderer water,Light sun){
  if(!File.ReadAllText("../tools/valoria-reference-clean/request.json").Contains("\"water_treatment\": \"flow_surface\""))return;
  var shader=Shader.Find("Eldoria/ReferenceClean/Comparison/RiverBefore");if(!shader)throw new Exception("Baseline river shader missing");var before=new Material(water.sharedMaterial);before.shader=shader;before.renderQueue=2000;before.color=new Color(.030f,.34f,.57f);var after=water.sharedMaterial;
  var ev=Path.GetFullPath(Path.Combine(UnityEngine.Application.dataPath,"../../docs/evidence/valoria-reference-clean"));var pos=cam.transform.position;var rot=cam.transform.rotation;var zoom=cam.orthographicSize;var home=Stamp(cam);var light=sun.transform.rotation.ToString("R")+sun.color.ToString("R")+sun.intensity;var mesh=water.GetComponent<MeshFilter>().sharedMesh.GetInstanceID();
  try{
   water.sharedMaterial=before;ValoriaReferenceCleanBuilder.Capture(cam,ev+"/water-before-main.png",1448,1086);cam.orthographicSize=17;cam.transform.position=new Vector3(25,25,-48);cam.transform.LookAt(new Vector3(-3,0,-20));var close=Stamp(cam);ValoriaReferenceCleanBuilder.Capture(cam,ev+"/water-before-detail.png",1448,1086);
   water.sharedMaterial=after;ValoriaReferenceCleanBuilder.Capture(cam,ev+"/water-after-detail.png",1448,1086);bool same=close==Stamp(cam);cam.transform.SetPositionAndRotation(pos,rot);cam.orthographicSize=zoom;ValoriaReferenceCleanBuilder.Capture(cam,ev+"/water-after-main.png",1448,1086);
   var report=new Report{source_sha=Environment.GetEnvironmentVariable("GITHUB_SHA"),scope="Native water depth/absorption and terrain-rock contact foam versus exact previous45 flow shader; inherited wave texture/palette, geometry/camera/lights fixed",same_camera=same&&home==Stamp(cam),same_lighting=light==sun.transform.rotation.ToString("R")+sun.color.ToString("R")+sun.intensity,same_geometry=mesh==water.GetComponent<MeshFilter>().sharedMesh.GetInstanceID()};
   if(!report.same_camera||!report.same_lighting||!report.same_geometry)throw new Exception("Water comparison identity failed");File.WriteAllText(ev+"/water-comparison.json",JsonUtility.ToJson(report,true));
  }finally{water.sharedMaterial=after;cam.transform.SetPositionAndRotation(pos,rot);cam.orthographicSize=zoom;UnityEngine.Object.DestroyImmediate(before);}
 }
}
}
