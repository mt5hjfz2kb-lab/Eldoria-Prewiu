using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using Object=UnityEngine.Object;
namespace Eldoria.ReferenceClean.Editor {
// Package the observed scene and current materials; no layout or gameplay changes.
public static class ValoriaReferenceCleanFinalize {
 const string Root="Assets/Eldoria/ArtTests/ValoriaReferenceClean";
 const string Scene=Root+"/Valoria_Reference_Clean.unity";
 static readonly string Ev=Path.GetFullPath("../docs/evidence/valoria-reference-clean");
 [Serializable]class Report{public string source_sha,scene=Scene,prefab_lighting="Portable prefabs use light probes; placed scene keeps its baked GI";public string[] prefabs;public int scene_triangles,scene_renderers;public bool geometry_unchanged,art_approved=false,physical_mobile_profiled=false;}
 static void Portable(GameObject root){foreach(var r in root.GetComponentsInChildren<MeshRenderer>(true)){r.lightmapIndex=-1;r.receiveGI=ReceiveGI.LightProbes;r.lightProbeUsage=LightProbeUsage.BlendProbes;GameObjectUtility.SetStaticEditorFlags(r.gameObject,0);}}
 public static void Build(){try{
  var scene=EditorSceneManager.OpenScene(Scene,OpenSceneMode.Single);var guard=Object.FindFirstObjectByType<ValoriaReferenceCleanRenderSettings>();guard.Apply();var cam=Camera.main;var meshes=Object.FindObjectsByType<MeshFilter>(FindObjectsSortMode.None);int before=meshes.Sum(f=>f.sharedMesh.triangles.Length/3);var results=new List<string>();
  void Section(string name,Func<MeshFilter,bool> match,Vector3 pivot){var group=new GameObject(name);group.transform.position=pivot;try{foreach(var mf in meshes.Where(match)){var copy=Object.Instantiate(mf.gameObject,mf.transform.parent);copy.transform.SetParent(group.transform,true);}if(group.transform.childCount==0)throw new Exception("Prefab source missing: "+name);Portable(group);group.transform.position=Vector3.zero;string path=Root+"/Prefabs/"+name+".prefab";PrefabUtility.SaveAsPrefabAsset(group,path);results.Add(path);}finally{Object.DestroyImmediate(group);}}
  Section("Bastion_I",f=>f.name.StartsWith("Bastion_I"),new Vector3(0,.35f,36.5f));Section("FortifiedGate",f=>f.name.StartsWith("Gate_"),new Vector3(-1.5f,.35f,-10));
  float st=Mathf.Sin(-14*Mathf.Deg2Rad),ct=Mathf.Cos(-14*Mathf.Deg2Rad);Section("MedievalBridge",f=>f.name.StartsWith("Bridge_"),new Vector3(-1.5f+13*st,.5f,-13*ct-10));
  Section("Paths",f=>f.name.StartsWith("Axial_Paving")||f.name.StartsWith("Outer_Road"),Vector3.zero);Section("BuildablePlots",f=>f.name.StartsWith("Plot_"),Vector3.zero);Section("Ruins",f=>f.name.StartsWith("Ruin_"),Vector3.zero);Section("RiverSurface",f=>f.name=="River water",Vector3.zero);
  var authored=GameObject.Find("Valoria authored architecture and environment");if(!authored)throw new Exception("Authored root missing");var assembly=Object.Instantiate(authored);try{Portable(assembly);string path=Root+"/Prefabs/ValoriaArchitecture.prefab";PrefabUtility.SaveAsPrefabAsset(assembly,path);results.Add(path);}finally{Object.DestroyImmediate(assembly);}
  int after=Object.FindObjectsByType<MeshFilter>(FindObjectsSortMode.None).Sum(f=>f.sharedMesh.triangles.Length/3);var report=new Report{source_sha=Environment.GetEnvironmentVariable("GITHUB_SHA"),prefabs=results.ToArray(),scene_triangles=after,scene_renderers=Object.FindObjectsByType<MeshRenderer>(FindObjectsSortMode.None).Length,geometry_unchanged=before==after};if(!report.geometry_unchanged)throw new Exception("Prefab packaging left geometry in scene");
  File.WriteAllText(Ev+"/editable-delivery.json",JsonUtility.ToJson(report,true));ValoriaReferenceCleanBuilder.Capture(cam,Ev+"/D-final.png",1448,1086);EditorSceneManager.SaveScene(scene,Scene);AssetDatabase.SaveAssets();guard.Restore();ExportExisting();
 }catch(Exception e){Debug.LogException(e);EditorApplication.Exit(1);}}
 [Serializable] class PackageReport {
  public string source_sha, status, package, sha256, scene_sha256, error;
  public long bytes, uncompressed_bytes;
  public bool native_import_verified=false;
 }
 static PackageReport packageReport;
 static double exportStarted, stableSince;
 static long lastLength=-1;
 static DateTime lastWrite;
 static string HashFile(string path){
  using(var sha=System.Security.Cryptography.SHA256.Create())
  using(var stream=File.OpenRead(path))
   return BitConverter.ToString(sha.ComputeHash(stream)).Replace("-","").ToLowerInvariant();
 }
 // Export only the existing validated files. No scene rebuild, prefab rewrite or GI bake.
 public static void ExportExisting(){
  try {
   Directory.CreateDirectory(Ev);
   VerifyExistingApk();
   string output=Path.GetFullPath("Builds/ValoriaReferenceClean/Valoria_Reference_Clean.unitypackage");
   Directory.CreateDirectory(Path.GetDirectoryName(output));
   if(File.Exists(output))File.Delete(output);
   packageReport=new PackageReport{source_sha=Environment.GetEnvironmentVariable("GITHUB_SHA"),
    status="EXPORT_PENDING",package=output,scene_sha256=HashFile(Scene)};
   File.WriteAllText(Ev+"/package-export.json",JsonUtility.ToJson(packageReport,true));
   AssetDatabase.SaveAssets();
   AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
   exportStarted=EditorApplication.timeSinceStartup;stableSince=exportStarted;lastLength=-1;
   EditorApplication.update+=CheckExport;
   AssetDatabase.ExportPackage(new[]{Scene,Root+"/Prefabs",Root+"/Sources"},output,
    ExportPackageOptions.Recurse|ExportPackageOptions.IncludeDependencies);
   Debug.Log("VALORIA_PACKAGE_EXPORT_RETURNED exists="+File.Exists(output));
  }catch(Exception e){ExportFailed(e);}
 }
 static void ExportFailed(Exception e){
  EditorApplication.update-=CheckExport;
  Debug.LogException(e);
  if(packageReport!=null){packageReport.status="EXPORT_FAILED";packageReport.error=e.ToString();
   File.WriteAllText(Ev+"/package-export.json",JsonUtility.ToJson(packageReport,true));}
  EditorApplication.Exit(1);
 }
 static void CheckExport(){
  try{
   double now=EditorApplication.timeSinceStartup;
   if(now-exportStarted>600)throw new TimeoutException("Package not complete within 600s");
   if(EditorApplication.isCompiling||EditorApplication.isUpdating||!File.Exists(packageReport.package))return;
   var info=new FileInfo(packageReport.package);
   if(info.Length!=lastLength||info.LastWriteTimeUtc!=lastWrite){
    lastLength=info.Length;lastWrite=info.LastWriteTimeUtc;stableSince=now;return;
   }
   if(lastLength<1024||now-stableSince<10)return;
   // An exclusive read plus complete gzip read rejects locked or incomplete output.
   using(var stream=new FileStream(packageReport.package,FileMode.Open,FileAccess.Read,FileShare.None))
   using(var gzip=new System.IO.Compression.GZipStream(stream,System.IO.Compression.CompressionMode.Decompress)){
    byte[] buffer=new byte[65536];int count;long total=0;
    while((count=gzip.Read(buffer,0,buffer.Length))>0)total+=count;
    if(total<1024||total%512!=0)throw new InvalidDataException("Invalid package TAR length");
    packageReport.uncompressed_bytes=total;
   }
   if(HashFile(Scene)!=packageReport.scene_sha256)throw new Exception("Export changed saved scene");
   packageReport.bytes=lastLength;packageReport.sha256=HashFile(packageReport.package);
   packageReport.status="EXPORTED_GZIP_VERIFIED_IMPORT_PENDING";
   File.WriteAllText(Ev+"/package-export.json",JsonUtility.ToJson(packageReport,true));
   EditorApplication.update-=CheckExport;
   Debug.Log("VALORIA_REFERENCE_CLEAN_PACKAGE_COMPLETE bytes="+lastLength+" sha256="+packageReport.sha256);
   EditorApplication.Exit(0);
  }catch(IOException e){
   if(EditorApplication.timeSinceStartup-exportStarted>600)ExportFailed(e);
  }catch(Exception e){ExportFailed(e);}
 }

 [Serializable] class ApkArchiveReport {
  public string source_sha,status,sha256;public long bytes;
  public string[] arm64_libraries,largest_entries;
  public bool physical_device_tested=false;
 }
 [Serializable] class PriorApk {public string apk_sha256;public long apk_bytes;}
 static void VerifyExistingApk(){
  const string apk="Builds/ValoriaReferenceClean/Valoria-Reference-Clean.apk";
  var expected=JsonUtility.FromJson<PriorApk>(File.ReadAllText(Ev+"/android-build.json"));
  if(!File.Exists(apk))throw new FileNotFoundException("native55 APK missing",apk);
  var digest=HashFile(apk);long length=new FileInfo(apk).Length;
  if(digest!=expected.apk_sha256||length!=expected.apk_bytes)throw new Exception("APK differs from native55 recorded bytes");
  var libs=new List<string>();var largest=new List<string>();
  using(var input=File.OpenRead(apk))
  using(var zip=new System.IO.Compression.ZipArchive(input,System.IO.Compression.ZipArchiveMode.Read)){
   if(zip.GetEntry("AndroidManifest.xml")==null)throw new Exception("APK manifest missing");
   foreach(var entry in zip.Entries.Where(e=>e.FullName.StartsWith("lib/")&&e.FullName.EndsWith(".so"))){
    if(!entry.FullName.StartsWith("lib/arm64-v8a/"))throw new Exception("Unexpected APK ABI "+entry.FullName);
    var header=new byte[64];using(var stream=entry.Open()){int offset=0,n;while(offset<header.Length&&(n=stream.Read(header,offset,header.Length-offset))>0)offset+=n;if(offset!=64)throw new Exception("Short ELF header");}
    if(header[0]!=127||header[1]!=69||header[2]!=76||header[3]!=70||header[4]!=2||header[5]!=1||BitConverter.ToUInt16(header,18)!=183)throw new Exception("Not AArch64 ELF: "+entry.FullName);
    libs.Add(entry.FullName);
   }
   foreach(var name in new[]{"libunity.so","libil2cpp.so"})if(!libs.Contains("lib/arm64-v8a/"+name))throw new Exception("Missing "+name);
   largest.AddRange(zip.Entries.OrderByDescending(e=>e.CompressedLength).Take(30).Select(e=>e.FullName+" | compressed="+e.CompressedLength+" | bytes="+e.Length));
  }
  File.WriteAllText(Ev+"/android-archive-verification.json",JsonUtility.ToJson(new ApkArchiveReport{
   source_sha=Environment.GetEnvironmentVariable("GITHUB_SHA"),status="NATIVE55_EXACT_HASH_AND_ARM64_ELF_VERIFIED",
   sha256=digest,bytes=length,arm64_libraries=libs.ToArray(),largest_entries=largest.ToArray()},true));
 }

}
}
