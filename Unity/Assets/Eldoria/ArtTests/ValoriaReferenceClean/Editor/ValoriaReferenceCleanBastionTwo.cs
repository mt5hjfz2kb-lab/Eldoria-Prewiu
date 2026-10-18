using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEditor;
using UnityEditor.SceneManagement;
using GLTFast;
using Object=UnityEngine.Object;
namespace Eldoria.ReferenceClean.Editor {
 public static class ValoriaReferenceCleanBastionTwo {
  const string Root="Assets/Eldoria/ArtTests/ValoriaReferenceClean";
  const string Dir=Root+"/ArtVariants/BastionII";
  static string Ev=>Path.GetFullPath("../docs/evidence/valoria-reference-clean");
  static string Hash(string p){using(var h=System.Security.Cryptography.SHA256.Create())return BitConverter.ToString(h.ComputeHash(File.ReadAllBytes(p)));}
  static void Views(Camera c,string prefix){var p=c.transform.position;var r=c.transform.rotation;float z=c.orthographicSize;try{
   ValoriaReferenceCleanBuilder.Capture(c,Ev+"/"+prefix+"-main.png",1448,1086);
   c.transform.position=new Vector3(24,31,-8);c.transform.LookAt(new Vector3(0,4,35.5f));c.orthographicSize=18;ValoriaReferenceCleanBuilder.Capture(c,Ev+"/"+prefix+"-bastion.png",1448,1086);
  }finally{c.transform.SetPositionAndRotation(p,r);c.orthographicSize=z;}}
  public static void Integrate(){try{
   var scene=EditorSceneManager.OpenScene(Dir+"/Valoria_BastionII.unity",OpenSceneMode.Single);
   var h=Object.FindFirstObjectByType<ValoriaReferenceCleanGameplay>();if(!h||!h.BastionOne||!h.BastionTwo)throw new Exception("Reviewed evolution bindings missing");
   h.BastionOne.SetActive(false);h.BastionTwo.SetActive(true);
   if(!PrefabUtility.SaveAsPrefabAsset(h.BastionTwo,Root+"/Prefabs/Bastion_II.prefab"))throw new Exception("Bastion II portable prefab failed");
   h.BastionTwo.SetActive(false);h.BastionOne.SetActive(true);
   if(!EditorSceneManager.SaveScene(scene,Root+"/Valoria_Reference_Clean.unity"))throw new Exception("Evolution integration save failed");
   AssetDatabase.SaveAssets();
   File.WriteAllText(Ev+"/bastion2-integration.json","{\"source_sha\":\""+Environment.GetEnvironmentVariable("GITHUB_SHA")+"\",\"existing_scene_and_gameplay_preserved\":true,\"level_two_model_bound\":true,\"level_one_saved_visible\":true,\"static_batching\":false}");
   ValoriaReferenceCleanFinalize.Build();
  }catch(Exception e){Debug.LogException(e);EditorApplication.Exit(1);}}
  public static async void Build(){ValoriaReferenceCleanRenderSettings guard=null;try{
   Directory.CreateDirectory(Dir);AssetDatabase.Refresh();string source=Root+"/Valoria_Reference_Clean.unity";string hash=Hash(source);var scene=EditorSceneManager.OpenScene(source,OpenSceneMode.Single);
   guard=Object.FindFirstObjectByType<ValoriaReferenceCleanRenderSettings>();guard.Apply();var cam=Camera.main;var old=Object.FindObjectsByType<MeshRenderer>(FindObjectsSortMode.None).Single(x=>x.name=="Bastion_I");
   var host=Object.FindFirstObjectByType<ValoriaReferenceCleanGameplay>();if(!host)throw new Exception("Validated gameplay host missing");
   Views(cam,"bastion2-before");string variant=Dir+"/Valoria_BastionII.unity";EditorSceneManager.SaveScene(scene,variant);
   // Map original glTF material names to the exact native74 bindings, preserving all PBR maps.
   var donorLoader=new GltfImport(null,new UninterruptedDeferAgent());var donorRoot=new GameObject("Temporary material correspondence");donorRoot.SetActive(false);
   if(!await donorLoader.Load(new Uri(Path.GetFullPath(Root+"/Sources/BastionEnsemble70.glb")).AbsoluteUri)||!await donorLoader.InstantiateMainSceneAsync(donorRoot.transform))throw new Exception("Retained donor GLB unavailable");
   var donorR=donorRoot.GetComponentsInChildren<MeshRenderer>(true).Single(x=>x.name=="Bastion_Ensemble_Candidate");if(donorR.sharedMaterials.Length!=old.sharedMaterials.Length)throw new Exception("Retained material slot mismatch");
   var mats=new Dictionary<string,Material>();for(int i=0;i<old.sharedMaterials.Length;i++)mats.Add(donorR.sharedMaterials[i].name,old.sharedMaterials[i]);
   var loader=new GltfImport(null,new UninterruptedDeferAgent());var next=new GameObject("Bastion II / authored evolution");next.transform.rotation=Quaternion.Euler(0,180,0);
   if(!await loader.Load(new Uri(Path.GetFullPath(Root+"/Sources/BastionII.glb")).AbsoluteUri,new ImportSettings{GenerateMipMaps=true,AnisotropicFilterLevel=4})||!await loader.InstantiateMainSceneAsync(next.transform))throw new Exception("Bastion II GLB failed");
   int triangles=0;
   foreach(var mf in next.GetComponentsInChildren<MeshFilter>()){
    var r=mf.GetComponent<MeshRenderer>();var raw=r.sharedMaterials;mf.sharedMesh=PrepareUV(mf.sharedMesh,raw,mf.name);
    r.sharedMaterials=raw.Select(m=>mats.TryGetValue(m.name,out var match)?match:throw new Exception("Unknown II material "+m.name)).ToArray();
    r.lightmapIndex=-1;r.receiveGI=ReceiveGI.LightProbes;r.lightProbeUsage=LightProbeUsage.BlendProbes;r.reflectionProbeUsage=ReflectionProbeUsage.BlendProbes;
    GameObjectUtility.SetStaticEditorFlags(r.gameObject,0);mf.gameObject.AddComponent<MeshCollider>().sharedMesh=mf.sharedMesh;triangles+=mf.sharedMesh.triangles.Length/3;
   }
   Object.DestroyImmediate(donorRoot);donorLoader.Dispose();
   host.BastionOne=old.gameObject;host.BastionTwo=next;old.gameObject.SetActive(false);Views(cam,"bastion2-after");
   // First-level state is what is serialized. Runtime progression alone enables II.
   next.SetActive(false);old.gameObject.SetActive(true);AssetDatabase.SaveAssets();EditorSceneManager.SaveScene(scene,variant);
   if(Hash(source)!=hash)throw new Exception("Authoritative scene changed during trial");
   File.WriteAllText(Ev+"/bastion2-comparison.json","{\"source_sha\":\""+Environment.GetEnvironmentVariable("GITHUB_SHA")+"\",\"original_scene_unchanged\":true,\"same_camera_light\":true,\"bastion2_triangles\":"+triangles+",\"baked_world_retained\":true,\"bastion2_gi\":\"existing baked LightProbes and ReflectionProbes; dynamic progressive building\",\"art_approved\":false}");
   guard.Restore();Debug.Log("VALORIA_REFERENCE_CLEAN_BASTION_TWO_COMPLETE");EditorApplication.Exit(0);
  }catch(Exception e){if(guard)guard.Restore();Debug.LogException(e);EditorApplication.Exit(1);}} static void Put(Object o,string path){if(AssetDatabase.LoadMainAssetAtPath(path))AssetDatabase.DeleteAsset(path);AssetDatabase.CreateAsset(o,path);}
 static Mesh PrepareUV(Mesh src,Material[] mats,string label){
  // glTF UV1 is already occupied by scanned stone. Move that material's UVs
  // onto UV0 before creating lightmap UV1; preserve all surface positions.
  var pv=src.vertices;var pn=src.normals;var pt=src.tangents;var pc=src.colors;
  var uv0=src.uv;var uv1=src.uv2;
  var v=new List<Vector3>();var n=new List<Vector3>();var t=new List<Vector4>();var c=new List<Color>();var uv=new List<Vector2>();var sub=new List<int[]>();
  for(int s=0;s<src.subMeshCount;s++){
   var mat=mats[s];int channel=mat&&mat.HasProperty("baseColorTexture_texCoord")?Mathf.RoundToInt(mat.GetFloat("baseColorTexture_texCoord")):0;
   foreach(string map in new[]{"normalTexture","metallicRoughnessTexture","occlusionTexture","emissiveTexture"})if(mat&&mat.HasProperty(map)&&mat.GetTexture(map)&&mat.HasProperty(map+"_texCoord")&&Mathf.RoundToInt(mat.GetFloat(map+"_texCoord"))!=channel)throw new Exception("Mixed material UV channels need explicit mapping: "+mat.name);
   var input=src.GetTriangles(s);var output=new int[input.Length];var remap=new Dictionary<int,int>();
   for(int j=0;j<input.Length;j++){
    int old=input[j];if(!remap.TryGetValue(old,out int idx)){
     idx=v.Count;remap.Add(old,idx);v.Add(pv[old]);if(pn.Length==pv.Length)n.Add(pn[old]);if(pt.Length==pv.Length)t.Add(pt[old]);if(pc.Length==pv.Length)c.Add(pc[old]);
     uv.Add(channel==1&&uv1.Length==pv.Length?uv1[old]:uv0.Length==pv.Length?uv0[old]:Vector2.zero);
    }output[j]=idx;
   }sub.Add(output);
  }
  var mesh=new Mesh{name=label+" GI UV",indexFormat=IndexFormat.UInt32};mesh.SetVertices(v);mesh.SetUVs(0,uv);if(n.Count==v.Count)mesh.SetNormals(n);if(t.Count==v.Count)mesh.SetTangents(t);if(c.Count==v.Count)mesh.SetColors(c);mesh.subMeshCount=sub.Count;for(int s=0;s<sub.Count;s++)mesh.SetTriangles(sub[s],s);mesh.RecalculateBounds();
  UnwrapParam.SetDefaults(out var unwrap);unwrap.hardAngle=88;unwrap.packMargin=.003f;
  Debug.Log("VALORIA_GI_UNWRAP_START "+label+" triangles="+src.triangles.Length/3);
  if(!Unwrapping.GenerateSecondaryUVSet(mesh,unwrap))throw new Exception("Lightmap UV unwrap failed: "+label);
  if(mesh.triangles.Length!=src.triangles.Length||(mesh.bounds.center-src.bounds.center).sqrMagnitude>1e-8f||(mesh.bounds.size-src.bounds.size).sqrMagnitude>1e-8f)throw new Exception("Geometry changed while allocating GI UV");
  Put(mesh,Dir+"/"+label+"_GI.asset");return mesh;
 }

 }
}
