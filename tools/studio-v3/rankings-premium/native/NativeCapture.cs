using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using Eldoria.LegacyProgression;

public static class NativeCapture {
 static string Out=>Environment.GetEnvironmentVariable("PR198_NATIVE_OUT")??throw new Exception("Output not set");
 [Serializable] class Shot {public string view,section,path,sha256;public int rowCount,textCount;}
 [Serializable] class Proof {public string unityVersion,sourceSha;public bool compiledRealProjection,gameModified,publicRankings,artApproved;public Shot[] shots;}
 static Font FontAt(string path){var f=AssetDatabase.LoadAssetAtPath<Font>(path);if(!f)throw new Exception("Missing font "+path);return f;}
 static string Hash(string path){using(var h=System.Security.Cryptography.SHA256.Create())return BitConverter.ToString(h.ComputeHash(File.ReadAllBytes(path))).Replace("-","").ToLowerInvariant();}
 public static string Run(){
  Directory.CreateDirectory(Out);var d=JsonUtility.FromJson<NativeProfile.Design>(File.ReadAllText(Path.Combine(Application.dataPath,"native-design.json")));
  if(d.screens.Length!=7)throw new Exception("Design scope");var tf=FontAt("Assets/Fonts/Cormorant.ttf");var bf=FontAt("Assets/Fonts/SourceSans.ttf");
  var result=new List<Shot>();
  foreach(var dims in new[]{(390,844,"phone"),(320,720,"compact"),(844,390,"landscape")}){
   int w=dims.Item1,h=dims.Item2;var canvasObj=new GameObject("QA Canvas",typeof(RectTransform),typeof(Canvas),typeof(GraphicRaycaster));
   var canvas=canvasObj.GetComponent<Canvas>();canvas.renderMode=RenderMode.WorldSpace;
   var rect=canvasObj.GetComponent<RectTransform>();rect.pivot=new Vector2(0,1);rect.sizeDelta=new Vector2(w,h);rect.position=Vector3.zero;
   var camObj=new GameObject("Native UI camera",typeof(Camera));var cam=camObj.GetComponent<Camera>();cam.orthographic=true;cam.orthographicSize=h/2f;cam.aspect=w/(float)h;cam.transform.position=new Vector3(w/2f,-h/2f,-10);cam.clearFlags=CameraClearFlags.SolidColor;cam.backgroundColor=new Color(.04f,.025f,.065f);canvas.worldCamera=cam;
   var rt=new RenderTexture(w,h,24,RenderTextureFormat.ARGB32);rt.Create();cam.targetTexture=rt;
   var view=canvasObj.AddComponent<NativeProfile>();var profile=NativeProfile.Fixture();view.Bind(profile,w,h,d,tf,bf);
   if(profile.PublicRankingsAvailable||ProfilePresentation.Sections(NativeProfile.Fixture(1)).Count!=1||ProfilePresentation.Sections(NativeProfile.Fixture(5)).Count!=6)throw new Exception("Domain gates");
   var sections=ProfilePresentation.Sections(profile);
   foreach(var sec in sections.Concat(new[]{ProfileSection.Discoveries})){
    string detail=result.Count%7==6?"ashes":null;view.Show(sec,detail);Canvas.ForceUpdateCanvases();
    var rows=detail!=null?ProfilePresentation.DiscoveryEntries(profile,detail):ProfilePresentation.Rows(profile,sec,new DateTime(2026,11,24,13,0,0,DateTimeKind.Utc));
    var labels=canvasObj.GetComponentsInChildren<Text>(false).Select(x=>x.text).ToList();
    foreach(var e in rows)if(!labels.Contains(e.Title)||!labels.Contains(e.Detail))throw new Exception("Missing live data "+e.Key);
    if(sec==ProfileSection.Rankings&&(rows.First().State!=ProfileEntryState.Unavailable||rows.Any(e=>e.CommandKind!="")))throw new Exception("Public ranking mismatch");
    var file=Path.Combine(Out,dims.Item3+"-"+view.ActiveView+".png");
    cam.Render();var old=RenderTexture.active;RenderTexture.active=rt;var image=new Texture2D(w,h,TextureFormat.RGB24,false);image.ReadPixels(new Rect(0,0,w,h),0,0);image.Apply();File.WriteAllBytes(file,image.EncodeToPNG());RenderTexture.active=old;UnityEngine.Object.DestroyImmediate(image);
    result.Add(new Shot{view=dims.Item3,section=view.ActiveView,path=Path.GetFileName(file),sha256=Hash(file),rowCount=rows.Count,textCount=labels.Count});
   }
   cam.targetTexture=null;rt.Release();UnityEngine.Object.DestroyImmediate(rt);UnityEngine.Object.DestroyImmediate(camObj);UnityEngine.Object.DestroyImmediate(canvasObj);
  }
  var proof=new Proof{unityVersion=Application.unityVersion,sourceSha="230bdf286c8b0b2cc0a1fe4aef0af797fcb9d84a",compiledRealProjection=true,gameModified=false,publicRankings=false,artApproved=false,shots=result.ToArray()};
  File.WriteAllText(Path.Combine(Out,"unity-proof.json"),JsonUtility.ToJson(proof,true));
  Debug.Log("PR198_NATIVE_CAPTURE_COMPLETE "+result.Count);return "PR198_NATIVE_CAPTURE_COMPLETE "+result.Count;
 }
 public static void Batch(){Run();}
}
