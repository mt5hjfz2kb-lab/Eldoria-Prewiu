#if UNITY_EDITOR
using System;
using System.IO;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Security.Cryptography;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Profiling;
using UnityEngine.SceneManagement;
using Eldoria.ReferenceClean;
using Eldoria.World4XCanon;
using Object=UnityEngine.Object;

[InitializeOnLoad]
public static class EldoriaBetaStructuralStartupProbe {
 const string Flag="Eldoria.BetaStructuralStartup.Active";
 const string ScenePath="Assets/Eldoria/ArtTests/ValoriaReferenceClean/Integration/Valoria_City_World_UI.unity";
 static double start=-1,readyAt=-1;static int pass,guardedFrames;
 static int terrainParityQueries;
 static long peakAllocated,peakReserved,peakManaged;
 static string firstGeometry,firstQueries;
 static readonly string[] Saves={"eldoria-unity-slice-v1.json","eldoria-unity-slice-owner-i-ii-v1.json"};
 static readonly string[] MeshNames={"BLD_Terrain","BLD_BreachFloor","BLD_BreachOutlyingFractures","BLD_BreachBrokenBasaltPlates","BLD_BreachTerracedStrata","BLD_BreachWeatheredRockfall"};
 static readonly List<string> errors=new List<string>();
 static readonly List<string> editorDiagnostics=new List<string>();
 [Serializable] public class Result {
  public string[] editorOnlyDiagnostics;
  public string source,inputAudit,mode,environment="Unity Editor Windows; NOT WebGL or physical iPhone",geometryHash,queryHash,topologyHash,populationRouteHash;
  public int terrainParityCheckedQueries;
  public int retainedContactNormals;
  public int pass,guardedFrames,retainedVerificationBuckets,verifiedSites,validatedCities;
  public double secondsToReady;
  public long peakAllocated,peakReserved,peakManaged,managedAfterFullGC;
  public bool ready,worldComplete,canonicalSavesUnchanged;
 }
 static string Out=>Environment.GetEnvironmentVariable("ELDORIA_MEMORY_OUT");
 static bool Staged=>Environment.GetEnvironmentVariable("ELDORIA_EXPECT_STAGED")=="1";
 static EldoriaBetaStructuralStartupProbe(){EditorApplication.update+=Tick;Application.logMessageReceived+=Log;}
 static void Log(string message,string stack,LogType type){
  if(SessionState.GetBool(Flag,false)&&message.StartsWith("ELDORIA_TERRAIN_QUERY_PARITY count=")){
   string value=message.Substring("ELDORIA_TERRAIN_QUERY_PARITY count=".Length).Split(' ')[0];terrainParityQueries+=int.Parse(value);
  }
  if(!SessionState.GetBool(Flag,false)||(type!=LogType.Error&&type!=LogType.Exception))return;
  // This observed Editor Search database initialization exception is not player code.
  // Preserve it explicitly; all other exceptions remain fatal to the test.
  if(message.StartsWith("ArgumentOutOfRangeException:")&&stack.Contains("UnityEditor.Search.SearchDatabase")&&stack.Contains("UnityEditor.Search.SearchInit.IndexationOnStartup")&&!stack.Contains("Eldoria.")){editorDiagnostics.Add(message+"\n"+stack);return;}
  errors.Add(message+"\n"+stack);
 }
 static void Need(bool value,string message){if(!value)throw new InvalidOperationException(message);}
 static string HashFile(string file){if(!File.Exists(file))return "absent";using(var f=File.OpenRead(file))using(var h=SHA256.Create())return BitConverter.ToString(h.ComputeHash(f)).Replace("-","");}
 public static void Run(){
  try {
   Need(!string.IsNullOrEmpty(Out),"Evidence output required");Directory.CreateDirectory(Out);
   Environment.SetEnvironmentVariable("VALORIA_GAMEPLAY_PROBE","1");
   foreach(var file in Saves)SessionState.SetString(Flag+file,HashFile(Path.Combine(Application.persistentDataPath,file)));
   EditorSceneManager.OpenScene(ScenePath,OpenSceneMode.Single);SessionState.SetBool(Flag,true);EditorApplication.EnterPlaymode();
  }catch(Exception e){Fail(e);}
 }
 static object Field(object value,string name){return value.GetType().GetField(name,BindingFlags.NonPublic|BindingFlags.Instance).GetValue(value);}
 static bool Built(RegionOneCanonicalLandscape adapter)=>(bool)Field(adapter,"built");
 static int VerificationBuckets(){var value=typeof(RegionOneBlenderLandscape).GetField("sourceVertices",BindingFlags.NonPublic|BindingFlags.Static).GetValue(null);return value==null?0:value is ICollection c?c.Count:(int)value.GetType().GetProperty("Count").GetValue(value);}
 static void Tick(){
  if(!SessionState.GetBool(Flag,false)||!EditorApplication.isPlaying)return;
  try {
   Need(errors.Count==0,"Unity errors: "+string.Join(" | ",errors));
   if(start<0)start=EditorApplication.timeSinceStartup;
   double now=EditorApplication.timeSinceStartup;
   Need(now-start<150,"World initialization/reload deadline exceeded");
   peakAllocated=Math.Max(peakAllocated,Profiler.GetTotalAllocatedMemoryLong());peakReserved=Math.Max(peakReserved,Profiler.GetTotalReservedMemoryLong());peakManaged=Math.Max(peakManaged,GC.GetTotalMemory(false));
   var host=Object.FindFirstObjectByType<ValoriaIntegratedWorld>();var adapter=Object.FindFirstObjectByType<RegionOneCanonicalLandscape>();
   if(Staged&&host&&adapter){
    var binding=typeof(ValoriaIntegratedWorld).GetProperty("PresentationBindingsReady",BindingFlags.NonPublic|BindingFlags.Instance);
    Need(binding!=null,"Staged host bindings contract missing");
    if((bool)binding.GetValue(host)&&!Built(adapter)){
     guardedFrames++;Need(!host.Ready,"Host exposes incomplete world as ready");
     Need(!host.TrySetWorld(true)&&!host.InWorld&&!host.WorldCamera.enabled&&!host.WorldRoot.activeSelf,"Incomplete world navigation was not rejected");
    }
   }
   if(!host||!host.Ready||!adapter||!Built(adapter))return;
   if(readyAt<0)readyAt=now;
   if(now-readyAt<5)return;
   Need(errors.Count==0,"Unity errors: "+string.Join(" | ",errors));
   if(Staged){Need(Environment.GetEnvironmentVariable("ELDORIA_BAKE_TERRAIN_QUERIES")=="1"||terrainParityQueries>10000,"Per-query collider parity missing");Need(guardedFrames>=2,"Staged startup did not yield protected frame boundaries");Need(VerificationBuckets()==0,"Verification cache remains resident");}
   var surface=Object.FindFirstObjectByType<RegionOneBlenderLandscape>(FindObjectsInactive.Include);Need(surface,"Missing exact terrain");
   var retainedNormals=Field(surface,"contactNormals") as Vector3[];
   if(Staged)Need(retainedNormals==null,"Initialization-only terrain normals remain resident");
   var placement=Object.FindFirstObjectByType<RegionOneResourcePlacement>(FindObjectsInactive.Include);Need(placement&&placement.VerifiedSiteCount>0,"Missing resource placement");
   string geometry=Geometry(surface),queries=Queries();
   if(pass==0){firstGeometry=geometry;firstQueries=queries;}else{Need(firstGeometry==geometry,"Geometry changed after real scene reload");Need(firstQueries==queries,"Terrain queries changed after real scene reload");}
   foreach(var file in Saves)Need(SessionState.GetString(Flag+file,"")==HashFile(Path.Combine(Application.persistentDataPath,file)),"Canonical save changed: "+file);
   var result=new Result{retainedContactNormals=retainedNormals==null?0:retainedNormals.Length,populationRouteHash=PopulationRoutes(host,placement),topologyHash=Topology(placement),terrainParityCheckedQueries=terrainParityQueries,editorOnlyDiagnostics=editorDiagnostics.ToArray(),source=Environment.GetEnvironmentVariable("ELDORIA_MEMORY_SOURCE"),inputAudit=Environment.GetEnvironmentVariable("ELDORIA_BASELINE_INPUT_AUDIT"),mode=Staged?"staged":"baseline",pass=pass,guardedFrames=guardedFrames,retainedVerificationBuckets=VerificationBuckets(),verifiedSites=placement.VerifiedSiteCount,validatedCities=placement.ValidatedCityCount,secondsToReady=readyAt-start,peakAllocated=peakAllocated,peakReserved=peakReserved,peakManaged=peakManaged,geometryHash=geometry,queryHash=queries,ready=true,worldComplete=true,canonicalSavesUnchanged=true};
   result.managedAfterFullGC=GC.GetTotalMemory(true);
   File.WriteAllText(Path.Combine(Out,"startup-"+pass+".json"),JsonUtility.ToJson(result,true));
   if(pass==0){
    pass=1;start=readyAt=-1;guardedFrames=0;terrainParityQueries=0;peakAllocated=peakReserved=peakManaged=0;
    EditorSceneManager.LoadSceneInPlayMode(ScenePath,new LoadSceneParameters(LoadSceneMode.Single));return;
   }
   if(Environment.GetEnvironmentVariable("ELDORIA_BAKE_TERRAIN_QUERIES")=="1"){
    var property=typeof(RegionOneResourcePlacement).GetProperty("CapturedTerrainQueries",BindingFlags.Public|BindingFlags.Static);
    var data=(byte[])property.GetValue(null);Need(data!=null&&data.Length>100000,"Missing complete terrain tape");
    var asset=Path.Combine(Application.dataPath,"Eldoria/World4XCanon/Resources/World4X/Blender/RuntimeTerrainQueries.bytes");
    Need(Directory.Exists(Path.GetDirectoryName(asset))&&!File.Exists(asset),"Terrain tape output already exists or folder missing");
    File.WriteAllBytes(asset,data);File.WriteAllBytes(Path.Combine(Out,"terrain-query-cache.bytes"),data);
   }
   File.WriteAllText(Path.Combine(Out,"complete.txt"),"STRUCTURAL_STARTUP_PASS; exact terrain validated twice; scene reload parity; canonical saves unchanged; no physical-device certification");
   SessionState.SetBool(Flag,false);EditorApplication.Exit(0);
  }catch(Exception e){Fail(e);}
 }
 static string PopulationRoutes(ValoriaIntegratedWorld host,RegionOneResourcePlacement placement)=>Digest(writer=>{
  var state=host.City.Gateway.Snapshot();
  foreach(var item in state.WorldResourceLocations.OrderBy(x=>x.TargetId,StringComparer.Ordinal)){
   writer.Write(item.TargetId??"");writer.Write(item.SiteId??"");writer.Write(item.Generation);writer.Write(item.Level);writer.Write(item.Capacity);
   writer.Write(item.X);writer.Write(item.Y);writer.Write(item.Z);
  }
  foreach(var item in state.WorldCreatureLocations.OrderBy(x=>x.TargetId,StringComparer.Ordinal)){
   writer.Write(item.TargetId??"");writer.Write(item.SiteId??"");writer.Write(item.CreatureKind??"");writer.Write(item.Generation);writer.Write(item.Level);
   writer.Write(item.X);writer.Write(item.Y);writer.Write(item.Z);writer.Write(item.Alive);
  }
  var routes=(RegionOneTerrainRoutes)Field(placement,"routes");
  var goals=state.WorldResourceLocations.Where(x=>x.Generation>0).Select(x=>new Vector3(x.X,x.Y,x.Z))
   .Concat(state.WorldCreatureLocations.Where(x=>x.Alive).Select(x=>new Vector3(x.X,x.Y,x.Z))).ToArray();
  foreach(var goal in goals){
   bool found=routes.TryRouteTo(state,goal,out var route);writer.Write(found);
   if(found){writer.Write(route.Count);foreach(var point in route){writer.Write(point.X);writer.Write(point.Y);writer.Write(point.Z);}}
  }
 });
 static string Topology(RegionOneResourcePlacement placement)=>Digest(writer=>{
  var sites=(IEnumerable)Field(placement,"sites");
  var names=new List<string>();foreach(var site in sites){var t=site.GetType();names.Add(t.GetField("Id").GetValue(site)+"|"+t.GetField("Wood").GetValue(site));}
  foreach(string value in names.OrderBy(x=>x,StringComparer.Ordinal))writer.Write(value);
  var routes=Field(placement,"routes");var cells=(IDictionary)Field(routes,"cells");
  var keys=new List<string>();foreach(DictionaryEntry cell in cells)keys.Add(cell.Key.ToString());
  foreach(string key in keys.OrderBy(x=>x,StringComparer.Ordinal))writer.Write(key);
 });
 static string Digest(Action<BinaryWriter> write){using(var stream=new MemoryStream()){using(var writer=new BinaryWriter(stream,System.Text.Encoding.UTF8,true))write(writer);stream.Position=0;using(var hash=SHA256.Create())return BitConverter.ToString(hash.ComputeHash(stream)).Replace("-","");}}
 static string Geometry(RegionOneBlenderLandscape surface){
  var artwork=(GameObject)Field(surface,"artwork");var filters=artwork.GetComponentsInChildren<MeshFilter>(true);
  return Digest(writer=>{foreach(string name in MeshNames){var filter=filters.Single(f=>f.name==name);var mesh=filter.sharedMesh;writer.Write(name);foreach(float value in MatrixValues(filter.transform.localToWorldMatrix))writer.Write(value);var vertices=mesh.vertices;writer.Write(vertices.Length);foreach(var v in vertices){writer.Write(v.x);writer.Write(v.y);writer.Write(v.z);}var normals=mesh.normals;writer.Write(normals.Length);foreach(var v in normals){writer.Write(v.x);writer.Write(v.y);writer.Write(v.z);}var triangles=mesh.triangles;writer.Write(triangles.Length);foreach(int index in triangles)writer.Write(index);}});
 }
 static IEnumerable<float> MatrixValues(Matrix4x4 matrix){for(int i=0;i<16;i++)yield return matrix[i];}
 static string Queries()=>Digest(writer=>{for(int z=-1700;z<=1700;z+=170)for(int x=-850;x<=850;x+=85){writer.Write(RegionOneBlenderLandscape.Height(x,z));writer.Write(RegionOneBlenderLandscape.Shore(x,z));writer.Write(RegionOneBlenderLandscape.Road(x,z));writer.Write(RegionOneBlenderLandscape.Forest(x,z));}});
 static void Fail(Exception e){SessionState.SetBool(Flag,false);if(!string.IsNullOrEmpty(Out)){Directory.CreateDirectory(Out);File.WriteAllText(Path.Combine(Out,"failure.txt"),e.ToString());}Debug.LogException(e);EditorApplication.Exit(1);}
}
#endif
