#if UNITY_EDITOR
using System;
using System.IO;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Eldoria.Domain;
using Eldoria.World4XCanon;
using Object=UnityEngine.Object;
namespace Eldoria.ReferenceClean.Editor {
 [InitializeOnLoad] public static class ValoriaConsolidationTrial {
  const string Scene="Assets/Eldoria/ArtTests/ValoriaReferenceClean/Integration/Valoria_City_World_UI.unity",Flag="Valoria.ConsolidationTrial";
  static string Ev=>Path.GetFullPath("../docs/evidence/valoria-reference-clean/consolidation-20261027/"+(Environment.GetEnvironmentVariable("GITHUB_RUN_ID")??"local"));
  [Serializable] public class Slot {public int[] triangles;}
  [Serializable] public class Part {public string name,asset;public Vector3[] vertices,normals;public Vector2[] uv;public Color[] colors;public Slot[] slots;}
  [Serializable] public class Parts {public Part[] parts;}
  [Serializable] public class Pose {public string source,phase,view;public Vector3 position,euler,target;public float zoom,aspect;public int width,height;public bool playMode;}
  static ValoriaConsolidationTrial(){EditorApplication.playModeStateChanged+=Entered;}
  static void Need(bool b,string s){if(!b)throw new InvalidOperationException(s);}
  public static void Trial(){try{
   Directory.CreateDirectory(Ev);Environment.SetEnvironmentVariable("VALORIA_GAMEPLAY_PROBE","1");
   EditorSceneManager.OpenScene(Scene,OpenSceneMode.Single);
   var city=Object.FindFirstObjectByType<ValoriaReferenceCleanGameplay>();Need(city&&city.BastionOne&&city.BastionTwo,"Approved city I-II required");
   var filters=city.BastionOne.GetComponentsInChildren<MeshFilter>(true).Concat(city.BastionTwo.GetComponentsInChildren<MeshFilter>(true)).Concat(Object.FindObjectsByType<MeshFilter>(FindObjectsInactive.Include,FindObjectsSortMode.None).Where(f=>f.name=="River_bank_rocks"||f.name.StartsWith("Bridge_")||f.name.StartsWith("Gate_"))).ToArray();
   var meshes=filters.Select(f=>f.sharedMesh).Where(m=>m).Distinct().ToArray();
   var parts=meshes.Select((m,i)=>new Part{name=filters.First(f=>f.sharedMesh==m).name+"_"+i,asset=AssetDatabase.GetAssetPath(m),vertices=m.vertices,normals=m.normals,uv=m.uv,colors=m.colors,slots=Enumerable.Range(0,m.subMeshCount).Select(j=>new Slot{triangles=m.GetTriangles(j)}).ToArray()}).ToArray();
   Need(parts.All(p=>!p.asset.Contains("HeroBastionGenerated")),"Rejected source prohibited");
   File.WriteAllText(Ev+"/mesh-input.json",JsonUtility.ToJson(new Parts{parts=parts}));
   var start=new System.Diagnostics.ProcessStartInfo(Environment.GetEnvironmentVariable("ELDORIA_CONSOLIDATION_BLENDER"),"-b -t 4 --python-exit-code 1 --python \""+Path.GetFullPath("../tools/valoria-reference-clean/consolidation_lod.py")+"\" -- \""+Ev+"\""){UseShellExecute=false};
   using(var process=System.Diagnostics.Process.Start(start)){if(!process.WaitForExit(480000)){process.Kill();throw new TimeoutException("Own Blender LOD operation timed out");}Need(process.ExitCode==0,"Blender LOD rejected");}
   SessionState.SetBool(Flag,true);EditorApplication.EnterPlaymode();
  }catch(Exception e){Fail(e);}}
  static void Fail(Exception e){Debug.LogException(e);Directory.CreateDirectory(Ev);File.WriteAllText(Ev+"/failure.txt",e.ToString());EditorApplication.Exit(1);}
  static void Entered(PlayModeStateChange state){if(state!=PlayModeStateChange.EnteredPlayMode||!SessionState.GetBool(Flag,false))return;SessionState.SetBool(Flag,false);EditorApplication.delayCall+=()=>{var h=Object.FindFirstObjectByType<ValoriaIntegratedWorld>();if(!h){Fail(new Exception("Host missing"));return;}h.StartCoroutine(Guard(h));};}
  static IEnumerator Guard(ValoriaIntegratedWorld h){var p=Probe(h);while(true){object c=null;bool more=false;try{more=p.MoveNext();if(more)c=p.Current;}catch(Exception e){Fail(e);yield break;}if(!more)yield break;yield return c;}}
  static Bounds BoundsOf(GameObject g){var rs=g.GetComponentsInChildren<MeshRenderer>(true);var b=rs[0].bounds;foreach(var r in rs)b.Encapsulate(r.bounds);return b;}
  static GameObject Clone(GameObject source,ValoriaIntegratedWorld h,Dictionary<string,Mesh> meshes){
   var root=new GameObject("Trial strategic "+source.name);root.transform.SetParent(h.WorldRoot.transform,true);var b=BoundsOf(source);var centre=new Vector3(b.center.x,b.min.y,b.center.z);
   foreach(var r in source.GetComponentsInChildren<MeshRenderer>(true)){var f=r.GetComponent<MeshFilter>();if(!f)continue;var go=new GameObject(r.name);go.layer=29;go.transform.SetParent(root.transform,false);var matrix=Matrix4x4.Translate(-centre)*r.transform.localToWorldMatrix;go.transform.localPosition=matrix.GetColumn(3);go.transform.localRotation=matrix.rotation;go.transform.localScale=matrix.lossyScale;go.AddComponent<MeshFilter>().sharedMesh=meshes[AssetDatabase.GetAssetPath(f.sharedMesh)];go.AddComponent<MeshRenderer>().sharedMaterials=r.sharedMaterials;}
   var location=h.WorldBastionOne.transform.position;location.y=h.WorldHome.y+RegionOneCanonicalLandscape.SampleHeight(location.x-h.WorldHome.x,location.z-h.WorldHome.z)+.10f;root.transform.position=location;root.transform.localScale=Vector3.one*24f/Mathf.Max(b.size.x,b.size.z);return root;
  }
  static Action<bool> Bridge(ValoriaIntegratedWorld h,Dictionary<string,Mesh> reduced){
   var source=Object.FindObjectsByType<MeshFilter>(FindObjectsInactive.Include,FindObjectsSortMode.None).Where(f=>f.name.StartsWith("Bridge_")&&!f.transform.IsChildOf(h.WorldRoot.transform)).ToArray();
   var paving=source.First(f=>f.name=="Bridge_Paving");var pv=paving.sharedMesh.vertices.Select(paving.transform.TransformPoint).ToArray();var pc=pv.Aggregate(Vector3.zero,(a,b)=>a+b)/pv.Length;
   double xx=0,zz=0,xz=0;foreach(var v in pv){var d=v-pc;xx+=d.x*d.x;zz+=d.z*d.z;xz+=d.x*d.z;}float angle=.5f*Mathf.Atan2((float)(2*xz),(float)(zz-xx))*Mathf.Rad2Deg;var unrotate=Quaternion.Euler(0,-angle,0);
   var aligned=pv.Select(v=>unrotate*(v-pc)).ToArray();float lo=aligned.Min(v=>v.z),hi=aligned.Max(v=>v.z),left=aligned.Min(v=>v.x),right=aligned.Max(v=>v.x);
   var original=new Dictionary<MeshFilter,Mesh>();var changed=new Dictionary<MeshFilter,Mesh>();var deck=new List<Vector3>();
   bool Centre(Vector3 p)=>Mathf.Abs(p.x-h.WorldHome.x)<30&&Mathf.Abs(p.z-h.WorldHome.z)<100;
   foreach(var f in h.WorldRoot.GetComponentsInChildren<MeshFilter>(true)){
    var r=f.GetComponent<Renderer>();if(!r||!f.sharedMesh)continue;var m=f.sharedMesh;var v=m.vertices;Mesh copy=null;
    for(int sub=0;sub<Mathf.Min(m.subMeshCount,r.sharedMaterials.Length);sub++)if(r.sharedMaterials[sub].name.StartsWith("Bridge weathered timber")||r.sharedMaterials[sub].name.StartsWith("Bridge alternate weathered plank")){
     var tris=m.GetTriangles(sub);var kept=new List<int>();for(int i=0;i<tris.Length;i+=3){var point=f.transform.TransformPoint((v[tris[i]]+v[tris[i+1]]+v[tris[i+2]])/3);if(Centre(point)){for(int k=0;k<3;k++)deck.Add(f.transform.TransformPoint(v[tris[i+k]]));}else kept.AddRange(new[]{tris[i],tris[i+1],tris[i+2]});}
     if(kept.Count!=tris.Length){if(!copy)copy=Object.Instantiate(m);copy.SetTriangles(kept,sub);}
    }
    if(copy){original[f]=m;changed[f]=copy;}
   }
   Need(deck.Count>0,"Existing Valoria deck not measured");float x0=deck.Min(v=>v.x),x1=deck.Max(v=>v.x),z0=deck.Min(v=>v.z),z1=deck.Max(v=>v.z);
   float DeckY(float z){float distance=deck.Min(v=>Mathf.Abs(v.z-z));return deck.Where(v=>Mathf.Abs(v.z-z)<=distance+.001f).Average(v=>v.y);}
   var root=new GameObject("Trial approved city stone bridge");root.transform.SetParent(h.WorldRoot.transform,true);root.transform.position=Vector3.zero;
   foreach(var f in source){var m=Object.Instantiate(reduced[AssetDatabase.GetAssetPath(f.sharedMesh)]);var vv=m.vertices;var nn=m.normals;
    for(int i=0;i<vv.Length;i++){var world=f.transform.TransformPoint(vv[i]);var local=unrotate*(world-pc);float z=Mathf.LerpUnclamped(z0,z1,(local.z-lo)/(hi-lo));vv[i]=new Vector3(Mathf.LerpUnclamped(x0,x1,(local.x-left)/(right-left)),DeckY(z)+(world.y-pc.y),z);nn[i]=unrotate*f.transform.TransformDirection(nn[i]);}m.vertices=vv;m.normals=nn;m.RecalculateBounds();
    var go=new GameObject(f.name);go.layer=29;go.transform.SetParent(root.transform,false);go.AddComponent<MeshFilter>().sharedMesh=m;go.AddComponent<MeshRenderer>().sharedMaterials=f.GetComponent<Renderer>().sharedMaterials;
   }
   var rails=h.WorldRoot.GetComponentsInChildren<MeshRenderer>(true).Where(r=>r.name.StartsWith("BLD_CrossingRails_")&&Centre(r.bounds.center)).ToArray();var enabled=rails.Select(r=>r.enabled).ToArray();root.SetActive(false);
   File.WriteAllText(Ev+"/bridge-adaptation.txt","Source actual city bridge; original deck extent="+new Vector3(x0,DeckY(z0),z0).ToString("F4")+" to "+new Vector3(x1,DeckY(z1),z1).ToString("F4")+"; collider/navigation unchanged");
   return on=>{foreach(var pair in original)pair.Key.sharedMesh=on?changed[pair.Key]:pair.Value;for(int i=0;i<rails.Length;i++)rails[i].enabled=on?false:enabled[i];root.SetActive(on);};
  }
  static IEnumerator Probe(ValoriaIntegratedWorld h){
   yield return null;yield return new WaitForSecondsRealtime(1);Need(h.Ready,"Host not ready");
   if(!h.TrySetWorld(true)){var offer=h.Read("sawmill",false).Building.Action;Need(offer!=null&&offer.Kind=="Build","Real sawmill offer required");h.Submit(offer);((ValoriaReferenceCleanGameplay.TestClock)h.City.Clock).Step(SliceRules.SawmillBuildSeconds+1);h.City.Gateway.Advance();yield return null;Need(h.TrySetWorld(true),"Real world progression failed");}
   yield return null;
   var meshes=new Dictionary<string,Mesh>();foreach(var p in JsonUtility.FromJson<Parts>(File.ReadAllText(Ev+"/mesh-output.json")).parts){var m=new Mesh{name=p.name,indexFormat=UnityEngine.Rendering.IndexFormat.UInt32,vertices=p.vertices,normals=p.normals,uv=p.uv,colors=p.colors,subMeshCount=p.slots.Length};for(int i=0;i<p.slots.Length;i++)m.SetTriangles(p.slots[i].triangles,i);m.RecalculateBounds();meshes.Add(p.asset,m);}
   var one=Clone(h.City.BastionOne,h,meshes);var two=Clone(h.City.BastionTwo,h,meshes);var scale=one.transform.localScale;one.SetActive(false);two.SetActive(false);
   File.WriteAllLines(Ev+"/world-bridge-parts.txt",h.WorldRoot.GetComponentsInChildren<MeshRenderer>(true).Where(r=>r.name.IndexOf("bridge",StringComparison.OrdinalIgnoreCase)>=0).Select(r=>r.name+" "+r.bounds+" "+string.Join(",",r.sharedMaterials.Select(m=>m.name))));
   var bridgeSurvey=new List<string>();
   foreach(var f in h.WorldRoot.GetComponentsInChildren<MeshFilter>(true)){
    var r=f.GetComponent<Renderer>();if(!r||!f.sharedMesh)continue;var m=f.sharedMesh;var vv=m.vertices;
    for(int i=0;i<Mathf.Min(m.subMeshCount,r.sharedMaterials.Length);i++)if(r.sharedMaterials[i].name.IndexOf("Bridge",StringComparison.OrdinalIgnoreCase)>=0){var points=m.GetTriangles(i).Distinct().Select(v=>f.transform.TransformPoint(vv[v])).Where(v=>Mathf.Abs(v.x-h.WorldHome.x)<30&&Mathf.Abs(v.z-h.WorldHome.z)<100).ToArray();if(points.Length>0){var b=new Bounds(points[0],Vector3.zero);foreach(var v in points)b.Encapsulate(v);bridgeSurvey.Add(f.name+" | "+r.sharedMaterials[i].name+" | "+"min="+b.min.ToString("F4")+" max="+b.max.ToString("F4")+" vertices="+points.Length);}}
   }
   File.WriteAllLines(Ev+"/bridge-measured.txt",bridgeSurvey);
   var bridge=Bridge(h,meshes);
   var cam=h.WorldCamera;var nav=h.WorldNavigation;var canon=Object.FindFirstObjectByType<RegionOneCanonicalLandscape>();
   foreach(var size in new[]{(1280,720,"horizontal"),(900,1600,"vertical")})foreach(var view in new[]{(2,"cercana"),(1,"regional"),(0,"panoramica")}){
    cam.aspect=(float)size.Item1/size.Item2;nav.SetZoomLevel(view.Item1,true);nav.PanTo(h.WorldHome);nav.FitToAuthoredBounds();canon.RefreshForCapture();
    string name=view.Item2+"-"+size.Item3;
    Capture(cam,nav.target,"before",name,size.Item1,size.Item2);
    bridge(true);
    foreach(var r in h.WorldRoot.GetComponentsInChildren<MeshRenderer>(true).Where(r=>r.name.StartsWith("Reference city Bastion")))r.enabled=false;
    float zoom=cam.orthographicSize;float factor=Mathf.Lerp(1,1.8f,Mathf.InverseLerp(110,280,zoom))*Mathf.Max(1,zoom/280);
    one.transform.localScale=scale*factor;two.transform.localScale=scale*factor;one.SetActive(true);Capture(cam,nav.target,"candidate-I",name,size.Item1,size.Item2);one.SetActive(false);two.SetActive(true);Capture(cam,nav.target,"candidate-II",name,size.Item1,size.Item2);two.SetActive(false);bridge(false);
   }
   var materials=h.WorldRoot.GetComponentsInChildren<Renderer>(true).SelectMany(r=>r.sharedMaterials).Where(m=>m&&m.shader.name=="Eldoria/RegionOneAuthoredSurface").Distinct().ToArray();
   foreach(var variant in new[]{"Boundary","Breach"}){
    var sh=Shader.Find("Eldoria/ConsolidationTrial/"+variant);Need(sh&&sh.isSupported,"Trial shader unavailable "+variant);
    var previous=materials.Select(m=>m.shader).ToArray();foreach(var m in materials)m.shader=sh;
    foreach(var size in new[]{(1280,720,"horizontal"),(900,1600,"vertical")})foreach(var view in new[]{(2,"cercana"),(1,"regional"),(0,"panoramica")}){cam.aspect=(float)size.Item1/size.Item2;nav.SetZoomLevel(view.Item1,true);nav.PanTo(h.WorldHome);nav.FitToAuthoredBounds();canon.RefreshForCapture();Capture(cam,nav.target,variant,view.Item2+"-"+size.Item3,size.Item1,size.Item2);}
    for(int i=0;i<materials.Length;i++)materials[i].shader=previous[i];
   }
   Need(h.TrySetWorld(false),"City return failed");
   var bank=Object.FindObjectsByType<MeshFilter>(FindObjectsInactive.Include,FindObjectsSortMode.None).First(f=>f.name=="River_bank_rocks"&&!f.transform.IsChildOf(h.WorldRoot.transform));var bankMesh=bank.sharedMesh;
   foreach(var size in new[]{(1448,1086,"horizontal"),(900,1600,"vertical")}){bank.sharedMesh=bankMesh;Capture(h.City.PlayerCamera,Vector3.zero,"bank-before","city-"+size.Item3,size.Item1,size.Item2);bank.sharedMesh=meshes[AssetDatabase.GetAssetPath(bankMesh)];Capture(h.City.PlayerCamera,Vector3.zero,"bank-candidate","city-"+size.Item3,size.Item1,size.Item2);}
   bank.sharedMesh=bankMesh;
   File.WriteAllText(Ev+"/trial-status.json","{\"art_approved\":false,\"playable_art_modified\":false,\"actual_unity_playmode\":true,\"views\":18,\"paid_calls\":0}");
   Debug.Log("VALORIA_REFERENCE_CLEAN_CONSOLIDATION_TRIAL_COMPLETE actual_Unity=1 art_approved=0");EditorApplication.Exit(0);
  }
  static void Capture(Camera cam,Vector3 target,string phase,string name,int w,int h){string dir=Ev+"/"+phase;Directory.CreateDirectory(dir);ValoriaReferenceCleanBuilder.Capture(cam,dir+"/"+name+".png",w,h);File.WriteAllText(dir+"/"+name+".json",JsonUtility.ToJson(new Pose{source=Environment.GetEnvironmentVariable("GITHUB_SHA"),phase=phase,view=name,position=cam.transform.position,euler=cam.transform.eulerAngles,target=target,zoom=cam.orthographicSize,aspect=cam.aspect,width=w,height=h,playMode=UnityEngine.Application.isPlaying},true));}
 }
}
#endif
