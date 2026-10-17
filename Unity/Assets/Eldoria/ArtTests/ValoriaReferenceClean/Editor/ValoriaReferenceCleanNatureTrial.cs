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
 public static async Task Capture(Camera cam,GameObject art,Light sun,Terrain terrain=null,Func<float,float,float> originalHeight=null){
  var path=Environment.GetEnvironmentVariable("VALORIA_NATURE_BASELINE_GLB");if(string.IsNullOrEmpty(path))return;
  var hash=Fingerprint(path);if(hash!=Environment.GetEnvironmentVariable("VALORIA_NATURE_BASELINE_HASH"))throw new Exception("Nature baseline GLB hash mismatch");
  var target=Environment.GetEnvironmentVariable("VALORIA_NATURE_TARGET")??"River_bank_rocks";var prefix=Environment.GetEnvironmentVariable("VALORIA_NATURE_PREFIX")??"nature-zone";
  bool canopy=target=="Fir_tree_group",shore=target=="Shore_profile";
  Func<Renderer,bool> match=r=>shore?(r.name.StartsWith("River_bank_rocks")||r.name.StartsWith("River_shingle")||r.name.StartsWith("River_sedge")||r.name.StartsWith("River_shore_foam")||r.name.StartsWith("Meadow_tufts")||r.name.StartsWith("Fir_tree_group")||r.name.StartsWith("Shrub_group")||r.name.StartsWith("Woodland_ground_rocks")||r.name.StartsWith("Foundation_shrubs")):r.name.StartsWith(target,StringComparison.Ordinal);
  var current=art.GetComponentsInChildren<Renderer>().Where(match).ToArray();if(current.Length==0)throw new Exception("Current comparison geometry absent");
  var loader=new GltfImport(null,new UninterruptedDeferAgent());var baseline=new GameObject("Temporary hash-verified previous nature geometry");
  var pos=cam.transform.position;var rot=cam.transform.rotation;var zoom=cam.orthographicSize;var home=Stamp(cam);var lighting=sun.transform.rotation.ToString("R")+sun.intensity+sun.color.ToString("R");
  float[,] afterHeights=null;TerrainData td=null;
  if(shore){if(!terrain||originalHeight==null)throw new Exception("Original shoreline surface missing");td=terrain.terrainData;afterHeights=td.GetHeights(0,0,td.heightmapResolution,td.heightmapResolution);}
  void RestoreGround(){if(td){td.SetHeights(0,0,afterHeights);td.SyncHeightmap();terrain.Flush();}}
  try{
   if(!await loader.Load(new Uri(path).AbsoluteUri,new ImportSettings{GenerateMipMaps=true,AnisotropicFilterLevel=4})||!await loader.InstantiateMainSceneAsync(baseline.transform))throw new Exception("Previous native model import failed for nature A-B");
   baseline.transform.rotation=Quaternion.Euler(0,180,0);var all=baseline.GetComponentsInChildren<Renderer>();var old=all.Where(match).ToArray();if(old.Length==0)throw new Exception("Previous comparison geometry absent");foreach(var r in all){r.enabled=match(r);if(match(r)){var peer=current.Single(c=>c.name==r.name);r.sharedMaterials=peer.sharedMaterials;}r.gameObject.isStatic=false;if(r.name.StartsWith("Meadow_tufts")||r.name.StartsWith("Fir_tree_group")||r.name.StartsWith("Shrub_group")||r.name.StartsWith("Woodland_ground_rocks")||r.name.StartsWith("Foundation_shrubs"))r.shadowCastingMode=UnityEngine.Rendering.ShadowCastingMode.Off;}
   if(shore){int n=td.heightmapResolution;var before=new float[n,n];for(int z=0;z<n;z++)for(int x=0;x<n;x++){float wx=terrain.transform.position.x+x/(float)(n-1)*td.size.x,wz=terrain.transform.position.z+z/(float)(n-1)*td.size.z;before[z,x]=(originalHeight(wx,wz)-terrain.transform.position.y)/td.size.y;}td.SetHeights(0,0,before);td.SyncHeightmap();terrain.Flush();}
   var ev=Path.GetFullPath(Path.Combine(UnityEngine.Application.dataPath,"../../docs/evidence/valoria-reference-clean"));
   foreach(var r in current)r.enabled=false;ValoriaReferenceCleanBuilder.Capture(cam,ev+"/"+prefix+"-before-main.png",1448,1086);
   cam.orthographicSize=canopy?12:16;cam.transform.position=canopy?new Vector3(45,20,-70):new Vector3(30,27,-49);cam.transform.LookAt(canopy?new Vector3(22,3,-42):new Vector3(15,-1,-23));var detail=Stamp(cam);
   ValoriaReferenceCleanBuilder.Capture(cam,ev+"/"+prefix+"-before-detail.png",1448,1086);
   foreach(var r in old)r.enabled=false;foreach(var r in current)r.enabled=true;RestoreGround();ValoriaReferenceCleanBuilder.Capture(cam,ev+"/"+prefix+"-after-detail.png",1448,1086);bool same=detail==Stamp(cam);
   cam.transform.SetPositionAndRotation(pos,rot);cam.orthographicSize=zoom;ValoriaReferenceCleanBuilder.Capture(cam,ev+"/"+prefix+"-after-main.png",1448,1086);
   var report=new Report{baseline_source=Environment.GetEnvironmentVariable("VALORIA_NATURE_BASELINE_SOURCE"),baseline_glb_sha256=hash,baseline_hash_verified=true,source_sha=Environment.GetEnvironmentVariable("GITHUB_SHA"),scope=shore?"Multiscale closed shingle and sedge shoreline pockets; identical all-bank43 terrain heights, exact previous43 nature meshes versus candidate, same materials/camera/light/architecture; existing large boulder and tree coordinates unchanged":canopy?"Only right foreground pine shading; bounds and placement preserved":"Only existing bank boulders; other surfaces shared",same_camera=same&&home==Stamp(cam),same_lighting=lighting==sun.transform.rotation.ToString("R")+sun.intensity+sun.color.ToString("R"),before_triangles=old.Sum(r=>r.GetComponent<MeshFilter>().sharedMesh.triangles.Length/3),after_triangles=current.Sum(r=>r.GetComponent<MeshFilter>().sharedMesh.triangles.Length/3)};
   if(!report.same_camera||!report.same_lighting)throw new Exception("Nature comparison changed camera or lighting");File.WriteAllText(ev+"/"+prefix+"-comparison.json",JsonUtility.ToJson(report,true));
  }finally{RestoreGround();foreach(var r in current)r.enabled=true;cam.transform.SetPositionAndRotation(pos,rot);cam.orthographicSize=zoom;UnityEngine.Object.DestroyImmediate(baseline);loader.Dispose();}
 }
}
}
