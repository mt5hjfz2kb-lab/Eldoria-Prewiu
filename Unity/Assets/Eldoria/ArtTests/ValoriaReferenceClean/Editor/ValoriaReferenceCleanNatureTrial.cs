using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Security.Cryptography;
using UnityEngine;
using GLTFast;
namespace Eldoria.ReferenceClean.Editor {
public static class ValoriaReferenceCleanNatureTrial {
 [Serializable]class Report{public string baseline_source,baseline_glb_sha256,source_sha,scope;public bool same_camera,same_lighting,baseline_hash_verified;public int before_triangles,after_triangles;}
 static string Stamp(Camera c)=>c.transform.position.ToString("R")+c.transform.rotation.ToString("R")+c.orthographicSize+"/"+c.aspect;
 static string Fingerprint(string file){using(var h=SHA256.Create())return BitConverter.ToString(h.ComputeHash(File.ReadAllBytes(file))).Replace("-","").ToLowerInvariant();}
 public static async Task Capture(Camera cam,GameObject art,Light sun){
  var path=Environment.GetEnvironmentVariable("VALORIA_NATURE_BASELINE_GLB");if(string.IsNullOrEmpty(path))return;
  var expected=Environment.GetEnvironmentVariable("VALORIA_NATURE_BASELINE_HASH");var hash=Fingerprint(path);if(hash!=expected)throw new Exception("Nature baseline GLB hash mismatch");
  var target=Environment.GetEnvironmentVariable("VALORIA_NATURE_TARGET")??"River_bank_rocks";var prefix=Environment.GetEnvironmentVariable("VALORIA_NATURE_PREFIX")??"nature-zone";bool canopy=target=="Fir_tree_group";
  var loader=new GltfImport(null,new UninterruptedDeferAgent());var baseline=new GameObject("Temporary exact previous native river mesh for A-B");
  var current=art.GetComponentsInChildren<Renderer>().Single(r=>r.name.StartsWith(target,StringComparison.Ordinal));var pos=cam.transform.position;var rot=cam.transform.rotation;var zoom=cam.orthographicSize;var home=Stamp(cam);var lighting=sun.transform.rotation.ToString("R")+sun.intensity+sun.color.ToString("R");
  try{
   if(!await loader.Load(new Uri(path).AbsoluteUri,new ImportSettings{GenerateMipMaps=true,AnisotropicFilterLevel=4})||!await loader.InstantiateMainSceneAsync(baseline.transform))throw new Exception("Previous native model import failed for nature A-B");baseline.transform.rotation=Quaternion.Euler(0,180,0);
   var all=baseline.GetComponentsInChildren<Renderer>();var old=all.Single(r=>r.name.StartsWith(target,StringComparison.Ordinal));foreach(var r in all)r.enabled=r==old;
   var ev=Path.GetFullPath(Path.Combine(UnityEngine.Application.dataPath,"../../docs/evidence/valoria-reference-clean"));
   current.enabled=false;ValoriaReferenceCleanBuilder.Capture(cam,ev+"/"+prefix+"-before-main.png",1448,1086);
   cam.orthographicSize=canopy?12:16;cam.transform.position=canopy?new Vector3(45,20,-70):new Vector3(30,27,-49);cam.transform.LookAt(canopy?new Vector3(22,3,-42):new Vector3(15,-1,-23));var detail=Stamp(cam);ValoriaReferenceCleanBuilder.Capture(cam,ev+"/"+prefix+"-before-detail.png",1448,1086);
   old.enabled=false;current.enabled=true;ValoriaReferenceCleanBuilder.Capture(cam,ev+"/"+prefix+"-after-detail.png",1448,1086);bool same=detail==Stamp(cam);
   cam.transform.SetPositionAndRotation(pos,rot);cam.orthographicSize=zoom;ValoriaReferenceCleanBuilder.Capture(cam,ev+"/"+prefix+"-after-main.png",1448,1086);
   var report=new Report{baseline_source=Environment.GetEnvironmentVariable("VALORIA_NATURE_BASELINE_SOURCE"),baseline_glb_sha256=hash,baseline_hash_verified=true,source_sha=Environment.GetEnvironmentVariable("GITHUB_SHA"),scope=canopy?"Only right foreground pine BVH occlusion vertex colors and mesh normals; bounds and placement preserved, other surfaces shared":"Only existing right river bank boulders resculpted and mapped in Blender; other scene surfaces shared",same_camera=same&&home==Stamp(cam),same_lighting=lighting==sun.transform.rotation.ToString("R")+sun.intensity+sun.color.ToString("R"),before_triangles=old.GetComponent<MeshFilter>().sharedMesh.triangles.Length/3,after_triangles=current.GetComponent<MeshFilter>().sharedMesh.triangles.Length/3};
   if(!report.same_camera||!report.same_lighting)throw new Exception("Nature comparison changed camera or lighting");File.WriteAllText(ev+"/"+prefix+"-comparison.json",JsonUtility.ToJson(report,true));
  }finally{current.enabled=true;cam.transform.SetPositionAndRotation(pos,rot);cam.orthographicSize=zoom;UnityEngine.Object.DestroyImmediate(baseline);loader.Dispose();}
 }
}
}
