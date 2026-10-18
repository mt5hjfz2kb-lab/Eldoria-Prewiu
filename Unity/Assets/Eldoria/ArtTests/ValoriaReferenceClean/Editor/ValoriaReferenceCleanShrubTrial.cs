using System;
using System.IO;
using System.Linq;
using GLTFast;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using Object=UnityEngine.Object;
namespace Eldoria.ReferenceClean.Editor {
public static class ValoriaReferenceCleanShrubTrial {
 const string Root="Assets/Eldoria/ArtTests/ValoriaReferenceClean";
 static string Ev=Path.GetFullPath("../docs/evidence/valoria-reference-clean");
 [Serializable]class Report{public string source_sha,scope;public int before_triangles,after_triangles;public bool same_camera,same_light,scene_unchanged,art_approved=false;}
 static string Hash(string path){using(var s=System.Security.Cryptography.SHA256.Create())return BitConverter.ToString(s.ComputeHash(File.ReadAllBytes(path)));}
 static string Stamp(Camera c)=>c.transform.localToWorldMatrix.ToString("R")+c.orthographicSize;
 static void Views(Camera c,string prefix){var p=c.transform.position;var r=c.transform.rotation;float z=c.orthographicSize;try{
 ValoriaReferenceCleanBuilder.Capture(c,Ev+"/"+prefix+"-main.png",1448,1086);
 c.transform.position=new Vector3(24,31,-8);c.transform.LookAt(new Vector3(0,4,35.5f));c.orthographicSize=18;ValoriaReferenceCleanBuilder.Capture(c,Ev+"/"+prefix+"-bastion.png",1448,1086);
 c.transform.position=new Vector3(25,15,13);c.transform.LookAt(new Vector3(10,.8f,32));c.orthographicSize=7;ValoriaReferenceCleanBuilder.Capture(c,Ev+"/"+prefix+"-detail.png",1448,1086);
 }finally{c.transform.SetPositionAndRotation(p,r);c.orthographicSize=z;}}
 public static async void Build(){
 ValoriaReferenceCleanRenderSettings guard=null;GltfImport loader=null;GameObject candidate=null;MeshRenderer original=null;
 try{
 string scene=Root+"/Valoria_Reference_Clean.unity",hash=Hash(scene);EditorSceneManager.OpenScene(scene,OpenSceneMode.Single);guard=Object.FindFirstObjectByType<ValoriaReferenceCleanRenderSettings>();guard.Apply();
 var cam=Camera.main;string camera=Stamp(cam),light=RenderSettings.sun.transform.rotation.ToString("R")+RenderSettings.sun.intensity;
 original=Object.FindObjectsByType<MeshRenderer>(FindObjectsSortMode.None).Single(r=>r.name=="Foundation_shrubs_and_groundcover");
 if(original.lightmapIndex>=0&&original.lightmapIndex<LightmapSettings.lightmaps.Length)throw new Exception("Shrub uses baked lightmap; separate bake required");
 loader=new GltfImport(null,new UninterruptedDeferAgent());candidate=new GameObject("Temporary existing shrub crown comparison");
 if(!await loader.Load(new Uri(Path.GetFullPath(Root+"/Sources/ShrubCrownPatch.glb")).AbsoluteUri,new ImportSettings{GenerateMipMaps=true,AnisotropicFilterLevel=4})||!await loader.InstantiateMainSceneAsync(candidate.transform))throw new Exception("Shrub patch import failed");
 candidate.transform.rotation=Quaternion.Euler(0,180,0);
 var edited=candidate.GetComponentsInChildren<MeshRenderer>().Single(r=>r.name=="Foundation_shrubs_and_groundcover");var before=original.GetComponent<MeshFilter>().sharedMesh;var after=edited.GetComponent<MeshFilter>().sharedMesh;
 if(before.subMeshCount!=after.subMeshCount)throw new Exception("Shrub material slot layout changed");
 var delta=original.transform.worldToLocalMatrix*edited.transform.localToWorldMatrix;
 for(int row=0;row<4;row++)for(int col=0;col<4;col++)if(Mathf.Abs(delta[row,col]-(row==col?1f:0f))>.0001f)throw new Exception("Shrub pivot mismatch");
 edited.sharedMaterials=original.sharedMaterials;edited.shadowCastingMode=original.shadowCastingMode;edited.receiveShadows=original.receiveShadows;edited.lightProbeUsage=original.lightProbeUsage;edited.reflectionProbeUsage=original.reflectionProbeUsage;GameObjectUtility.SetStaticEditorFlags(edited.gameObject,0);
 edited.enabled=false;Views(cam,"shrub-before");original.enabled=false;edited.enabled=true;Views(cam,"shrub-after");
 var report=new Report{source_sha=Environment.GetEnvironmentVariable("GITHUB_SHA"),scope="Replace only measured donor crowns beside Bastion with actual Qwen-authored overlapping leaf lobes; original crowns removed, all other donor geometry preserved. Same materials, sun, GI, terrain, camera; scene restored.",before_triangles=before.triangles.Length/3,after_triangles=after.triangles.Length/3,same_camera=Stamp(cam)==camera,same_light=light==RenderSettings.sun.transform.rotation.ToString("R")+RenderSettings.sun.intensity,scene_unchanged=Hash(scene)==hash};
 if(!report.same_camera||!report.same_light||!report.scene_unchanged||report.after_triangles-report.before_triangles>5000)throw new Exception("Shrub trial invariants failed");
 File.WriteAllText(Ev+"/shrub-comparison.json",JsonUtility.ToJson(report,true));
 original.enabled=true;Object.DestroyImmediate(candidate);candidate=null;loader.Dispose();loader=null;guard.Restore();Debug.Log("VALORIA_REFERENCE_CLEAN_SHRUB_TRIAL_COMPLETE");EditorApplication.Exit(0);
 }catch(Exception e){if(original)original.enabled=true;if(candidate)Object.DestroyImmediate(candidate);if(loader!=null)loader.Dispose();if(guard)guard.Restore();Debug.LogException(e);EditorApplication.Exit(1);}
 }
}
}
