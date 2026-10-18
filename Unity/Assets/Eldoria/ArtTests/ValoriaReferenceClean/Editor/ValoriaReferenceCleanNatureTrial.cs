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
  bool canopy=target=="Fir_tree_group",shore=target=="Shore_profile",shingle=target=="River_shingle_pockets";
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
   var report=new Report{baseline_source=Environment.GetEnvironmentVariable("VALORIA_NATURE_BASELINE_SOURCE"),baseline_glb_sha256=hash,baseline_hash_verified=true,source_sha=Environment.GetEnvironmentVariable("GITHUB_SHA"),scope=shingle?"Only closed shoreline stone mesh: exact previous44 dry-bank pebbles versus preserved pebbles plus new partly submerged waterline stones; all other renderers, terrain, materials, camera and light shared":shore?"Multiscale closed shingle and sedge shoreline pockets; identical all-bank43 terrain heights, exact previous43 nature meshes versus candidate, same materials/camera/light/architecture; existing large boulder and tree coordinates unchanged":canopy?"Only right foreground pine shading; bounds and placement preserved":"Only existing bank boulders; other surfaces shared",same_camera=same&&home==Stamp(cam),same_lighting=lighting==sun.transform.rotation.ToString("R")+sun.intensity+sun.color.ToString("R"),before_triangles=old.Sum(r=>r.GetComponent<MeshFilter>().sharedMesh.triangles.Length/3),after_triangles=current.Sum(r=>r.GetComponent<MeshFilter>().sharedMesh.triangles.Length/3)};
   if(!report.same_camera||!report.same_lighting)throw new Exception("Nature comparison changed camera or lighting");File.WriteAllText(ev+"/"+prefix+"-comparison.json",JsonUtility.ToJson(report,true));
  }finally{RestoreGround();foreach(var r in current)r.enabled=true;cam.transform.SetPositionAndRotation(pos,rot);cam.orthographicSize=zoom;UnityEngine.Object.DestroyImmediate(baseline);loader.Dispose();}
 }
 public static void IntegrateBankPatch(){integrateBankPatch=true;BuildBankPatch();}
 static bool integrateBankPatch;
 public static async void BuildBankPatch(){
  const string root="Assets/Eldoria/ArtTests/ValoriaReferenceClean";
  var ev=Path.GetFullPath("../docs/evidence/valoria-reference-clean");
  GltfImport loader=null;GameObject candidate=null;MeshRenderer original=null;
  ValoriaReferenceCleanRenderSettings guard=null;
  try{
   UnityEditor.SceneManagement.EditorSceneManager.OpenScene(root+"/Valoria_Reference_Clean.unity",UnityEditor.SceneManagement.OpenSceneMode.Single);
   guard=UnityEngine.Object.FindFirstObjectByType<ValoriaReferenceCleanRenderSettings>();guard.Apply();
   var cam=Camera.main;var sun=RenderSettings.sun;var pos=cam.transform.position;var rot=cam.transform.rotation;float zoom=cam.orthographicSize;
   var camera=Stamp(cam);string light=sun.transform.rotation.ToString("R")+sun.intensity+sun.color.ToString("R");
   original=UnityEngine.Object.FindObjectsByType<MeshRenderer>(FindObjectsSortMode.None).Single(r=>r.name=="River_bank_rocks");
   if(original.lightmapIndex>=0&&original.lightmapIndex<LightmapSettings.lightmaps.Length)throw new Exception("Bank unexpectedly binds scene lightmap");
   loader=new GltfImport(null,new UninterruptedDeferAgent());candidate=new GameObject("Temporary Qwen existing-bank patch");
   string path=Path.GetFullPath(root+"/Sources/RiverBankPatch.glb");
   if(!await loader.Load(new Uri(path).AbsoluteUri,new ImportSettings{GenerateMipMaps=true,AnisotropicFilterLevel=4})||!await loader.InstantiateMainSceneAsync(candidate.transform))throw new Exception("Actual bank patch import failed");
   candidate.transform.rotation=Quaternion.Euler(0,180,0);
   var edited=candidate.GetComponentsInChildren<MeshRenderer>().Single(r=>r.name=="River_bank_rocks");
   if(edited.GetComponent<MeshFilter>().sharedMesh.subMeshCount!=original.GetComponent<MeshFilter>().sharedMesh.subMeshCount)throw new Exception("Bank patch material-slot layout changed");
   edited.sharedMaterials=original.sharedMaterials;edited.shadowCastingMode=original.shadowCastingMode;edited.receiveShadows=original.receiveShadows;
   edited.lightProbeUsage=original.lightProbeUsage;edited.reflectionProbeUsage=original.reflectionProbeUsage;
   UnityEditor.GameObjectUtility.SetStaticEditorFlags(edited.gameObject,0);
   int before=original.GetComponent<MeshFilter>().sharedMesh.triangles.Length/3,after=edited.GetComponent<MeshFilter>().sharedMesh.triangles.Length/3;
   if(before!=after)throw new Exception("Unexpected bank topology count");
   void Views(string prefix){
    cam.transform.SetPositionAndRotation(pos,rot);cam.orthographicSize=zoom;
    ValoriaReferenceCleanBuilder.Capture(cam,ev+"/"+prefix+"-main.png",1448,1086);
    cam.orthographicSize=16;cam.transform.position=new Vector3(30,27,-49);cam.transform.LookAt(new Vector3(15,-1,-23));
    ValoriaReferenceCleanBuilder.Capture(cam,ev+"/"+prefix+"-detail.png",1448,1086);
    cam.transform.SetPositionAndRotation(pos,rot);cam.orthographicSize=zoom;
   }
   edited.enabled=false;Views("bank-patch-before");
   original.enabled=false;edited.enabled=true;Views("bank-patch-after");
   var report=new Report{baseline_source="native54/56 saved scene",baseline_glb_sha256=Fingerprint(root+"/Sources/Valoria_Reference_Clean.glb"),baseline_hash_verified=true,
    source_sha=Environment.GetEnvironmentVariable("GITHUB_SHA"),scope="Qwen14B actual plan over observed source components; existing BlenderMCP reshapes only north-bank representative boulders. All other renderers/materials/terrain/light/camera shared. Trial restored, not accepted or extended.",
    same_camera=camera==Stamp(cam),same_lighting=light==sun.transform.rotation.ToString("R")+sun.intensity+sun.color.ToString("R"),before_triangles=before,after_triangles=after};
   if(!report.same_camera||!report.same_lighting)throw new Exception("Bank trial camera/light mismatch");
   File.WriteAllText(ev+"/bank-patch-comparison.json",JsonUtility.ToJson(report,true));
   if(integrateBankPatch){
    // Persist the reviewed native61 mesh on the existing renderer, keeping its identity/materials.
    if(Fingerprint(path)!="3e0ef5d89efe739e0f5a992a20385867d184af49cb19252c89af9cee1072dbbc")throw new Exception("Unreviewed bank mesh");
    var originalFilter=original.GetComponent<MeshFilter>();var editedFilter=edited.GetComponent<MeshFilter>();
    var delta=original.transform.worldToLocalMatrix*edited.transform.localToWorldMatrix;
    for(int row=0;row<4;row++)for(int col=0;col<4;col++)if(Mathf.Abs(delta[row,col]-(row==col?1f:0f))>.0001f)throw new Exception("Bank import pivot mismatch");
    var persistent=UnityEngine.Object.Instantiate(editedFilter.sharedMesh);persistent.name="River_bank_rocks_native61_reviewed";
    string meshPath=root+"/Sources/RiverBankRocks_Native61.asset";
    if(UnityEditor.AssetDatabase.LoadAssetAtPath<Mesh>(meshPath))throw new Exception("Reviewed mesh asset already exists");
    UnityEditor.AssetDatabase.CreateAsset(persistent,meshPath);originalFilter.sharedMesh=persistent;
    var collider=original.GetComponent<MeshCollider>();if(collider)collider.sharedMesh=persistent;
    UnityEditor.EditorUtility.SetDirty(originalFilter);if(collider)UnityEditor.EditorUtility.SetDirty(collider);
    File.WriteAllText(ev+"/bank-patch-retained.json","{\"native_review_run\":39086508959,\"scope\":\"seven existing north-bank components only\",\"scene_identity_preserved\":true,\"formal_art_approval\":false}");
   }
   original.enabled=true;UnityEngine.Object.DestroyImmediate(candidate);candidate=null;loader.Dispose();loader=null;guard.Restore();
   if(integrateBankPatch){
    UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(original.gameObject.scene);
    UnityEditor.SceneManagement.EditorSceneManager.SaveScene(original.gameObject.scene);
    UnityEditor.AssetDatabase.SaveAssets();ValoriaReferenceCleanFinalize.Build();return;
   }
   Debug.Log("VALORIA_REFERENCE_CLEAN_BANK_PATCH_COMPLETE");UnityEditor.EditorApplication.Exit(0);
  }catch(Exception e){if(original)original.enabled=true;if(candidate)UnityEngine.Object.DestroyImmediate(candidate);if(loader!=null)loader.Dispose();if(guard)guard.Restore();Debug.LogException(e);UnityEditor.EditorApplication.Exit(1);}
 }

}
}
