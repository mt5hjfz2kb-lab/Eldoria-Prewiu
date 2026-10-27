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
  [Serializable] public class Part {public string name,asset;public Vector3[] vertices,normals;public Vector2[] uv;public Color[] colors;public Slot[] slots;public float waterline;public Vector3[] protectedPoints;}
  [Serializable] public class Parts {public Part[] parts;}
  [Serializable] public class Pose {public string source,phase,view;public Vector3 position,euler,target;public float zoom,aspect;public int width,height;public bool playMode,actualUnityCamera,physicalIphone;public string label,sha256,sourceScene,version,context;public Vector3 cameraPosition,worldTarget;public Quaternion cameraRotation;}
  static ValoriaConsolidationTrial(){EditorApplication.playModeStateChanged+=Entered;}
  static void Need(bool b,string s){if(!b)throw new InvalidOperationException(s);}
  public static void Trial(){try{
   Directory.CreateDirectory(Ev);Environment.SetEnvironmentVariable("VALORIA_GAMEPLAY_PROBE","1");
   EditorSceneManager.OpenScene(Scene,OpenSceneMode.Single);
   var city=Object.FindFirstObjectByType<ValoriaReferenceCleanGameplay>();Need(city&&city.BastionOne&&city.BastionTwo,"Approved city I-II required");
   Bake(city.BastionOne,city.PlayerCamera,"BastionI");Bake(city.BastionTwo,city.PlayerCamera,"BastionII");
   var filters=city.BastionOne.GetComponentsInChildren<MeshFilter>(true).Concat(city.BastionTwo.GetComponentsInChildren<MeshFilter>(true)).Concat(Object.FindObjectsByType<MeshFilter>(FindObjectsInactive.Include,FindObjectsSortMode.None).Where(f=>f.name=="River_bank_rocks"||f.name.StartsWith("Bridge_")||f.name.StartsWith("Gate_"))).ToArray();
   var meshes=filters.Select(f=>f.sharedMesh).Where(m=>m).Distinct().ToArray();
   var parts=meshes.Select((m,i)=>new Part{name=filters.First(f=>f.sharedMesh==m).name+"_"+i,asset=AssetDatabase.GetAssetPath(m),vertices=m.vertices,normals=m.normals,uv=m.uv,colors=m.colors,slots=Enumerable.Range(0,m.subMeshCount).Select(j=>new Slot{triangles=m.GetTriangles(j)}).ToArray()}).ToArray();
   foreach(var part in parts.Where(p=>p.name.StartsWith("River_bank_rocks"))){var filter=filters.First(f=>f.sharedMesh&&AssetDatabase.GetAssetPath(f.sharedMesh)==part.asset);part.waterline=filter.transform.InverseTransformPoint(new Vector3(0,-2.05f,0)).y;
    var world=Object.FindFirstObjectByType<ValoriaIntegratedWorld>().WorldRoot.transform;
    part.protectedPoints=Object.FindObjectsByType<MeshFilter>(FindObjectsInactive.Include,FindObjectsSortMode.None).Where(f=>f.sharedMesh&&!f.transform.IsChildOf(world)&&(f.name.IndexOf("Fir",StringComparison.OrdinalIgnoreCase)>=0||f.name.IndexOf("Pine",StringComparison.OrdinalIgnoreCase)>=0||f.name.IndexOf("tree",StringComparison.OrdinalIgnoreCase)>=0)).SelectMany(f=>f.sharedMesh.vertices.Select(f.transform.TransformPoint)).Where(v=>v.y<1.5f&&v.z>-38&&v.z<-8).Select(filter.transform.InverseTransformPoint).ToArray();
   }
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
  static void Bake(GameObject source,Camera reference,string name){
   var root=new GameObject("Isolated approved architecture bake");
   foreach(var f in source.GetComponentsInChildren<MeshFilter>(true)){var r=f.GetComponent<Renderer>();if(!r)continue;var go=new GameObject(f.name);go.layer=28;go.transform.SetParent(root.transform,false);go.transform.position=f.transform.position;go.transform.rotation=f.transform.rotation;go.transform.localScale=f.transform.lossyScale;go.AddComponent<MeshFilter>().sharedMesh=f.sharedMesh;go.AddComponent<MeshRenderer>().sharedMaterials=r.sharedMaterials;}
   var bounds=BoundsOf(root);var obj=new GameObject("Actual architecture bake camera");var c=obj.AddComponent<Camera>();c.CopyFrom(reference);c.enabled=false;c.cullingMask=1<<28;c.clearFlags=CameraClearFlags.SolidColor;c.backgroundColor=Color.clear;c.orthographic=true;c.aspect=1;c.transform.rotation=reference.transform.rotation;
   float extent=0;for(int i=0;i<8;i++){var v=new Vector3((i&1)==0?bounds.min.x:bounds.max.x,(i&2)==0?bounds.min.y:bounds.max.y,(i&4)==0?bounds.min.z:bounds.max.z)-bounds.center;extent=Mathf.Max(extent,Mathf.Abs(Vector3.Dot(v,c.transform.up)),Mathf.Abs(Vector3.Dot(v,c.transform.right)));}
   c.orthographicSize=extent*1.04f;c.transform.position=bounds.center-c.transform.forward*100;c.nearClipPlane=.1f;c.farClipPlane=250;
   var rt=new RenderTexture(1024,1024,24,RenderTextureFormat.ARGB32);rt.antiAliasing=4;c.targetTexture=rt;var prior=RenderTexture.active;c.Render();c.Render();RenderTexture.active=rt;var pixels=new Texture2D(1024,1024,TextureFormat.RGBA32,false);pixels.ReadPixels(new Rect(0,0,1024,1024),0,0);pixels.Apply();var colors=pixels.GetPixels32();int x0=1023,y0=1023,x1=0,y1=0;for(int y=0;y<1024;y++)for(int x=0;x<1024;x++)if(colors[y*1024+x].a>6){x0=Mathf.Min(x0,x);x1=Mathf.Max(x1,x);y0=Mathf.Min(y0,y);y1=Mathf.Max(y1,y);}
   Need(x1>x0&&y1>y0,"Actual source bake empty");x0=Mathf.Max(0,x0-2);y0=Mathf.Max(0,y0-2);x1=Mathf.Min(1023,x1+2);y1=Mathf.Min(1023,y1+2);var cropped=new Texture2D(x1-x0+1,y1-y0+1,TextureFormat.RGBA32,false);cropped.SetPixels(pixels.GetPixels(x0,y0,cropped.width,cropped.height));cropped.Apply();File.WriteAllBytes(Ev+"/"+name+".png",cropped.EncodeToPNG());
   c.targetTexture=null;RenderTexture.active=prior;Object.DestroyImmediate(rt);Object.DestroyImmediate(pixels);Object.DestroyImmediate(cropped);Object.DestroyImmediate(obj);Object.DestroyImmediate(root);
  }
  static GameObject Icon(ValoriaIntegratedWorld h,string name){
   var texture=new Texture2D(2,2,TextureFormat.RGBA32,false);Need(texture.LoadImage(File.ReadAllBytes(Ev+"/"+name+".png")),"Source bake unavailable");texture.wrapMode=TextureWrapMode.Clamp;
   var material=new Material(Shader.Find("Eldoria/World4X/LocalCanopy")){name="Approved architecture "+name,mainTexture=texture};
   var pos=h.WorldBastionOne.transform.position;pos.z-=8;pos.y=h.WorldHome.y+RegionOneCanonicalLandscape.SampleHeight(pos.x-h.WorldHome.x,pos.z-h.WorldHome.z)+.10f;
   float width=24,height=width*texture.height/texture.width;var m=new Mesh{name=name,vertices=new[]{pos,pos,pos,pos},uv=new[]{Vector2.zero,Vector2.right,Vector2.one,Vector2.up},uv2=new[]{new Vector2(-width/2,0),new Vector2(width/2,0),new Vector2(width/2,height),new Vector2(-width/2,height)},triangles=new[]{0,1,2,0,2,3}};m.bounds=new Bounds(pos,Vector3.one*800);
   var go=new GameObject(name);go.layer=29;go.transform.SetParent(h.WorldRoot.transform,true);go.transform.position=Vector3.zero;go.AddComponent<MeshFilter>().sharedMesh=m;var r=go.AddComponent<MeshRenderer>();r.sharedMaterial=material;r.shadowCastingMode=UnityEngine.Rendering.ShadowCastingMode.Off;return go;
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
  static Mesh Foam(MeshFilter bank,Mesh candidate,MeshFilter foam){
   var verts=candidate.vertices.Select(bank.transform.TransformPoint).ToArray();var tris=candidate.triangles;var output=new List<Vector3>();var uv=new List<Vector2>();var faces=new List<int>();const float level=-2.05f;
   for(int i=0;i<tris.Length;i+=3){var points=new List<Vector3>();for(int k=0;k<3;k++){var a=verts[tris[i+k]];var b=verts[tris[i+(k+1)%3]];if((a.y-level)*(b.y-level)<0)points.Add(Vector3.LerpUnclamped(a,b,(level-a.y)/(b.y-a.y)));}if(points.Count!=2||i%9==0)continue;var p=points[0];var q=points[1];var d=q-p;if(d.sqrMagnitude<.0004f)continue;var offset=Vector3.Cross(Vector3.up,d).normalized*.025f;p.y=q.y=level+.023f;int n=output.Count;foreach(var v in new[]{p-offset,p+offset,q+offset,q-offset})output.Add(foam.transform.InverseTransformPoint(v));uv.AddRange(new[]{Vector2.zero,Vector2.up,Vector2.one,Vector2.right});faces.AddRange(new[]{n,n+1,n+2,n,n+2,n+3});}
   var mesh=new Mesh{name="Foam at actual retained stone-water intersections",indexFormat=UnityEngine.Rendering.IndexFormat.UInt32,vertices=output.ToArray(),uv=uv.ToArray(),triangles=faces.ToArray()};mesh.RecalculateNormals();mesh.RecalculateBounds();return mesh;
  }
  static IEnumerator Probe(ValoriaIntegratedWorld h){
   yield return null;yield return new WaitForSecondsRealtime(1);Need(h.Ready,"Host not ready");
   if(!h.TrySetWorld(true)){var offer=h.Read("sawmill",false).Building.Action;Need(offer!=null&&offer.Kind=="Build","Real sawmill offer required");h.Submit(offer);((ValoriaReferenceCleanGameplay.TestClock)h.City.Clock).Step(SliceRules.SawmillBuildSeconds+1);h.City.Gateway.Advance();yield return null;Need(h.TrySetWorld(true),"Real world progression failed");}
   yield return null;
   var meshes=new Dictionary<string,Mesh>();foreach(var p in JsonUtility.FromJson<Parts>(File.ReadAllText(Ev+"/mesh-output.json")).parts){var m=new Mesh{name=p.name,indexFormat=UnityEngine.Rendering.IndexFormat.UInt32,vertices=p.vertices,normals=p.normals,uv=p.uv,colors=p.colors,subMeshCount=p.slots.Length};for(int i=0;i<p.slots.Length;i++)m.SetTriangles(p.slots[i].triangles,i);m.RecalculateBounds();meshes.Add(p.asset,m);}
   var one=Icon(h,"BastionI");var two=Icon(h,"BastionII");one.SetActive(false);two.SetActive(false);
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
    one.GetComponent<Renderer>().sharedMaterial.SetFloat("_BillboardScale",factor);two.GetComponent<Renderer>().sharedMaterial.SetFloat("_BillboardScale",factor);one.SetActive(true);Capture(cam,nav.target,"candidate-I",name,size.Item1,size.Item2);one.SetActive(false);two.SetActive(true);Capture(cam,nav.target,"candidate-II",name,size.Item1,size.Item2);two.SetActive(false);bridge(false);
   }
   var materials=h.WorldRoot.GetComponentsInChildren<Renderer>(true).SelectMany(r=>r.sharedMaterials).Where(m=>m&&m.shader.name=="Eldoria/RegionOneAuthoredSurface").Distinct().ToArray();
   foreach(var variant in new[]{"Boundary","Breach"}){
    var sh=Shader.Find("Eldoria/ConsolidationTrial/"+variant);Need(sh&&sh.isSupported,"Trial shader unavailable "+variant);
    var previous=materials.Select(m=>m.shader).ToArray();foreach(var m in materials)m.shader=sh;
    foreach(var size in new[]{(1280,720,"horizontal"),(900,1600,"vertical")})foreach(var view in new[]{(2,"cercana"),(1,"regional"),(0,"panoramica")}){cam.aspect=(float)size.Item1/size.Item2;nav.SetZoomLevel(view.Item1,true);nav.PanTo(h.WorldHome);nav.FitToAuthoredBounds();canon.RefreshForCapture();Capture(cam,nav.target,variant,view.Item2+"-"+size.Item3,size.Item1,size.Item2);}
    for(int i=0;i<materials.Length;i++)materials[i].shader=previous[i];
   }
   Need(h.TrySetWorld(false),"City return failed");
   var bank=Object.FindObjectsByType<MeshFilter>(FindObjectsInactive.Include,FindObjectsSortMode.None).First(f=>f.name=="River_bank_rocks"&&!f.transform.IsChildOf(h.WorldRoot.transform));var bankMesh=bank.sharedMesh;var foam=Object.FindObjectsByType<MeshFilter>(FindObjectsInactive.Include,FindObjectsSortMode.None).First(f=>f.name=="River_shore_foam"&&!f.transform.IsChildOf(h.WorldRoot.transform));var foamMesh=foam.sharedMesh;var newFoam=Foam(bank,meshes[AssetDatabase.GetAssetPath(bankMesh)],foam);
   foreach(var size in new[]{(1448,1086,"horizontal"),(900,1600,"vertical")}){bank.sharedMesh=bankMesh;foam.sharedMesh=foamMesh;Capture(h.City.PlayerCamera,Vector3.zero,"bank-before","city-"+size.Item3,size.Item1,size.Item2);bank.sharedMesh=meshes[AssetDatabase.GetAssetPath(bankMesh)];foam.sharedMesh=newFoam;Capture(h.City.PlayerCamera,Vector3.zero,"bank-candidate","city-"+size.Item3,size.Item1,size.Item2);}
   bank.sharedMesh=bankMesh;foam.sharedMesh=foamMesh;
   File.WriteAllText(Ev+"/trial-status.json","{\"art_approved\":false,\"playable_art_modified\":false,\"actual_unity_playmode\":true,\"views\":18,\"paid_calls\":0}");
   Debug.Log("VALORIA_REFERENCE_CLEAN_CONSOLIDATION_TRIAL_COMPLETE actual_Unity=1 art_approved=0");EditorApplication.Exit(0);
  }
  static void Capture(Camera cam,Vector3 target,string phase,string name,int w,int h){string dir=Ev+"/"+phase;Directory.CreateDirectory(dir);cam.aspect=(float)w/h;ValoriaReferenceCleanBuilder.Capture(cam,dir+"/"+name+".png",w,h);string hash;using(var sha=System.Security.Cryptography.SHA256.Create())hash=BitConverter.ToString(sha.ComputeHash(File.ReadAllBytes(dir+"/"+name+".png"))).Replace("-","").ToLowerInvariant();File.WriteAllText(dir+"/"+name+".json",JsonUtility.ToJson(new Pose{source=Environment.GetEnvironmentVariable("GITHUB_SHA"),phase=phase,view=name,label=name,sha256=hash,sourceScene=Scene,version=UnityEngine.Application.unityVersion,context="Actual integrated Unity PlayMode; disposable genuine progression",actualUnityCamera=true,physicalIphone=false,cameraPosition=cam.transform.position,cameraRotation=cam.transform.rotation,worldTarget=target,zoom=cam.orthographicSize,position=cam.transform.position,euler=cam.transform.eulerAngles,target=target,aspect=cam.aspect,width=w,height=h,playMode=UnityEngine.Application.isPlaying},true));}
 }
}
#endif
