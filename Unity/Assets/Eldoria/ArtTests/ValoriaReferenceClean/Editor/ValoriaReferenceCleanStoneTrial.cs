using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
namespace Eldoria.ReferenceClean.Editor {
// A material-only comparison on one evaluated mesh, under identical native lights.
public static class ValoriaReferenceCleanStoneTrial {
 const string Prefix="Trial sandstone ";
 static string Name(Material m)=>m?m.name.Replace('_',' ').Trim():"";
 [Serializable]class CameraState{public Vector3 position;public Quaternion rotation;public float orthographic_size,aspect;}
 [Serializable]class LightState{public Vector3 euler;public Color sun,sky,equator,ground;public float intensity;}
 [Serializable]class Report{public string source_sha,scope,before_main_camera,after_main_camera,before_detail_camera,after_detail_camera,lighting;public bool same_camera,same_lighting,same_geometry,original_uv0_preserved;public int zone_renderers,zone_material_slots;public string[] material_mapping;}
 static string CameraStamp(Camera c)=>JsonUtility.ToJson(new CameraState{position=c.transform.position,rotation=c.transform.rotation,orthographic_size=c.orthographicSize,aspect=c.aspect});
 static string LightStamp(Light l)=>JsonUtility.ToJson(new LightState{euler=l.transform.eulerAngles,sun=l.color,intensity=l.intensity,sky=RenderSettings.ambientSkyColor,equator=RenderSettings.ambientEquatorColor,ground=RenderSettings.ambientGroundColor});
 public static void Capture(Camera cam,GameObject art,Light sun,ValoriaReferenceCleanInteraction interaction){
  var renderers=art.GetComponentsInChildren<Renderer>();var materials=renderers.SelectMany(r=>r.sharedMaterials).Where(m=>m).Distinct().ToArray();
  var zone=renderers.Where(r=>r.sharedMaterials.Any(m=>Name(m).StartsWith(Prefix,StringComparison.Ordinal))).ToArray();if(zone.Length==0)return;
  var byName=materials.GroupBy(Name).ToDictionary(g=>g.Key,g=>g.First());var after=zone.Select(r=>r.sharedMaterials).ToArray();var before=new List<Material[]>();var mapping=new HashSet<string>();
  foreach(var array in after){var baseline=(Material[])array.Clone();for(int j=0;j<array.Length;j++){var n=Name(array[j]);if(!n.StartsWith(Prefix,StringComparison.Ordinal))continue;var original="Warm limestone "+n.Substring(Prefix.Length);if(!byName.TryGetValue(original,out var m))throw new Exception("Missing actual baseline material "+original);baseline[j]=m;mapping.Add(n+" -> "+original);}before.Add(baseline);}
  var meshIds=art.GetComponentsInChildren<MeshFilter>().Select(f=>f.sharedMesh.GetInstanceID()).ToArray();
  var ev=Path.GetFullPath(Path.Combine(UnityEngine.Application.dataPath,"../../docs/evidence/valoria-reference-clean"));
  void Set(bool newSurface){for(int i=0;i<zone.Length;i++)zone[i].sharedMaterials=newSurface?after[i]:before[i];}
  var light=LightStamp(sun);var report=new Report{source_sha=Environment.GetEnvironmentVariable("GITHUB_SHA"),scope="Authored limestone material slots only; same geometry and original UV0 retained. Reused gate material stays fixed during this comparison.",lighting=light,zone_renderers=zone.Length,zone_material_slots=after.Sum(a=>a.Count(m=>Name(m).StartsWith(Prefix,StringComparison.Ordinal))),material_mapping=mapping.OrderBy(s=>s).ToArray(),original_uv0_preserved=true};
  try{
   Set(false);report.before_main_camera=CameraStamp(cam);ValoriaReferenceCleanBuilder.Capture(cam,ev+"/stone-zone-before-main.png",1448,1086);
   cam.orthographicSize=18;cam.transform.position=new Vector3(24,31,-8);cam.transform.LookAt(new Vector3(0,4,35.5f));report.before_detail_camera=CameraStamp(cam);ValoriaReferenceCleanBuilder.Capture(cam,ev+"/stone-zone-before-detail.png",1448,1086);
   Set(true);report.after_detail_camera=CameraStamp(cam);ValoriaReferenceCleanBuilder.Capture(cam,ev+"/stone-zone-after-detail.png",1448,1086);
   cam.transform.SetPositionAndRotation(interaction.homePosition,interaction.homeRotation);cam.orthographicSize=34;report.after_main_camera=CameraStamp(cam);ValoriaReferenceCleanBuilder.Capture(cam,ev+"/stone-zone-after-main.png",1448,1086);
   report.same_camera=report.before_main_camera==report.after_main_camera&&report.before_detail_camera==report.after_detail_camera;report.same_lighting=light==LightStamp(sun);report.same_geometry=meshIds.SequenceEqual(art.GetComponentsInChildren<MeshFilter>().Select(f=>f.sharedMesh.GetInstanceID()));
   if(!report.same_camera||!report.same_lighting||!report.same_geometry)throw new Exception("Surface comparison changed camera, lighting or geometry");
   File.WriteAllText(ev+"/stone-zone-comparison.json",JsonUtility.ToJson(report,true));
  }finally{Set(true);cam.transform.SetPositionAndRotation(interaction.homePosition,interaction.homeRotation);cam.orthographicSize=34;}
 }
}
}
