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
using Eldoria.ReferenceClean;
using Eldoria.World4XCanon;
using Object=UnityEngine.Object;

[InitializeOnLoad]
public static class EldoriaBetaMemoryProbe {
 const string Flag="Eldoria.BetaMemoryProbe.Active";
 const string Scene="Assets/Eldoria/ArtTests/ValoriaReferenceClean/Integration/Valoria_City_World_UI.unity";
 static double start=-1;static int phase;
 static readonly int[] times={0,3,10,30};
 [Serializable] public class Item {public string name,type,path,format;public long bytes;public int width,height,vertices;public bool readable;}
 [Serializable] public class Group {public string type;public long bytes;public int count;}
 [Serializable] public class Cache {public string name;public int count;}
 [Serializable] public class Snapshot {public string source,environment="Unity Editor Windows; NOT WebGL or iPhone";public int elapsed;public long allocated,reserved,managed;public bool ready;public Group[] groups;public Item[] largest;public Cache[] terrainCaches;}
 static EldoriaBetaMemoryProbe(){EditorApplication.update+=Tick;}
 static string Out=>Environment.GetEnvironmentVariable("ELDORIA_MEMORY_OUT")??Path.GetFullPath("../beta-memory-evidence");
 static string Hash(string path){if(!File.Exists(path))return "absent";using(var s=File.OpenRead(path))using(var h=SHA256.Create())return BitConverter.ToString(h.ComputeHash(s)).Replace("-","");}
 static readonly string[] Saves={"eldoria-unity-slice-v1.json","eldoria-unity-slice-owner-i-ii-v1.json"};
 public static void Run(){
  try{
   Directory.CreateDirectory(Out);Environment.SetEnvironmentVariable("VALORIA_GAMEPLAY_PROBE","1");
   foreach(var file in Saves)SessionState.SetString(Flag+file,Hash(Path.Combine(Application.persistentDataPath,file)));
   EditorSceneManager.OpenScene(Scene,OpenSceneMode.Single);
   SessionState.SetBool(Flag,true);EditorApplication.EnterPlaymode();
  }catch(Exception e){Fail(e);}
 }
 static void Tick(){
  if(!SessionState.GetBool(Flag,false)||!EditorApplication.isPlaying)return;
  try{
   if(start<0)start=EditorApplication.timeSinceStartup;
   int seconds=(int)(EditorApplication.timeSinceStartup-start);
   if(phase<times.Length&&seconds>=times[phase]){Capture(seconds);phase++;}
   if(phase==times.Length){
    foreach(var file in Saves)if(SessionState.GetString(Flag+file,"")!=Hash(Path.Combine(Application.persistentDataPath,file)))throw new Exception("Canonical save changed: "+file);
    File.WriteAllText(Path.Combine(Out,"complete.txt"),"MEMORY_PROBE_COMPLETE; canonical save bytes unchanged; no player actions; no physical-device certification");
    SessionState.SetBool(Flag,false);Debug.Log("ELDORIA_BETA_MEMORY_PROBE_COMPLETE");EditorApplication.Exit(0);
   }
  }catch(Exception e){Fail(e);}
 }
 static void Capture(int seconds){
  var host=Object.FindFirstObjectByType<ValoriaIntegratedWorld>();
  var all=Resources.FindObjectsOfTypeAll<Object>().Where(o=>o).Select(o=>{
   var item=new Item{name=o.name,type=o.GetType().Name,path=AssetDatabase.GetAssetPath(o),bytes=Profiler.GetRuntimeMemorySizeLong(o)};
   if(o is Texture2D t){item.width=t.width;item.height=t.height;item.format=t.format.ToString();item.readable=t.isReadable;}
   if(o is Mesh m){item.vertices=m.vertexCount;item.readable=m.isReadable;}
   return item;
  }).ToArray();
  var caches=new List<Cache>();var surface=Object.FindFirstObjectByType<RegionOneBlenderLandscape>(FindObjectsInactive.Include);
  foreach(var field in typeof(RegionOneBlenderLandscape).GetFields(BindingFlags.NonPublic|BindingFlags.Instance|BindingFlags.Static)){
   if(!field.IsStatic&&!surface)continue;var value=field.GetValue(field.IsStatic?null:surface);
   if(value is ICollection collection)caches.Add(new Cache{name=field.Name,count=collection.Count});
  }
  var report=new Snapshot{source=Environment.GetEnvironmentVariable("ELDORIA_MEMORY_SOURCE"),elapsed=seconds,allocated=Profiler.GetTotalAllocatedMemoryLong(),reserved=Profiler.GetTotalReservedMemoryLong(),managed=GC.GetTotalMemory(false),ready=host&&host.Ready,groups=all.GroupBy(i=>i.type).Select(g=>new Group{type=g.Key,count=g.Count(),bytes=g.Sum(i=>i.bytes)}).OrderByDescending(g=>g.bytes).ToArray(),largest=all.OrderByDescending(i=>i.bytes).Take(100).ToArray(),terrainCaches=caches.ToArray()};
  File.WriteAllText(Path.Combine(Out,"memory-"+phase+".json"),JsonUtility.ToJson(report,true));Debug.Log("ELDORIA_MEMORY_STAGE "+seconds+" allocated="+report.allocated+" managed="+report.managed+" ready="+report.ready);
 }
 static void Fail(Exception e){SessionState.SetBool(Flag,false);Directory.CreateDirectory(Out);File.WriteAllText(Path.Combine(Out,"failure.txt"),e.ToString());Debug.LogException(e);EditorApplication.Exit(1);}
}
#endif
