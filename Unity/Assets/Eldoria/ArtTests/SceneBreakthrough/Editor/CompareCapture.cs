using System;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.Rendering;
namespace Eldoria.Breakthrough {
public static partial class SceneCapture {
 public static void Compare() {
  try {
   var s=EditorSceneManager.OpenScene(Root+"/FrozenBaseline/FrozenCandidate.unity",OpenSceneMode.Single);
   cam=UnityEngine.Object.FindObjectsByType<Camera>(FindObjectsSortMode.None).FirstOrDefault(c=>c.orthographic);
   if(cam==null)throw new Exception("Frozen orthographic player camera missing");
   foreach(var ui in UnityEngine.Object.FindObjectsByType<Canvas>(FindObjectsSortMode.None))ui.gameObject.SetActive(false);
   Capture("frozen-authentic-main",1536,1024);
   foreach(var t in UnityEngine.Object.FindObjectsByType<Transform>(UnityEngine.FindObjectsInactive.Include,FindObjectsSortMode.None))GameObjectUtility.RemoveMonoBehavioursWithMissingScript(t.gameObject);
   var originalPose=cam.transform.position;float oldSize=cam.orthographicSize;var rotation=cam.transform.rotation;
   Transform gate=null;foreach(var t in UnityEngine.Object.FindObjectsByType<Transform>(FindObjectsSortMode.None))if(t.name=="LowerGate"){gate=t;break;}
   if(gate==null)throw new Exception("Frozen lower gate semantic node missing");
   var visual=new GameObject("FROZEN_SOURCE_FRAME_NORMALIZATION_ONLY");
   foreach(var root in s.GetRootGameObjects())if(root!=visual&&root!=cam.gameObject&&root.GetComponent<Light>()==null&&root.GetComponent<Canvas>()==null)root.transform.SetParent(visual.transform,true);
   visual.transform.rotation=Quaternion.Euler(0,180,0);visual.transform.localScale=Vector3.one*(24/oldSize);
   cam.transform.position=new Vector3(30.818315f,63.093408f,-84.672624f);cam.transform.LookAt(Vector3.zero);cam.orthographicSize=24;cam.aspect=1.5f;cam.nearClipPlane=.1f;cam.farClipPlane=400;
   var b=BoundsOf(gate.gameObject);visual.transform.position+=P(691,702,7)-new Vector3(b.center.x,b.min.y,b.center.z);
   int suns=0;foreach(var light in UnityEngine.Object.FindObjectsByType<Light>(FindObjectsSortMode.None)){if(light.type!=LightType.Directional){light.enabled=false;continue;}suns++;light.intensity=1.15f;light.color=new Color(1,.88f,.72f);light.shadows=LightShadows.Soft;light.transform.rotation=Quaternion.Euler(40,-45,0);}
   if(suns==0){var light=new GameObject("Comparison sun").AddComponent<Light>();light.type=LightType.Directional;light.intensity=1.15f;light.color=new Color(1,.88f,.72f);light.shadows=LightShadows.Soft;light.transform.rotation=Quaternion.Euler(40,-45,0);}
   RenderSettings.ambientMode=AmbientMode.Trilight;RenderSettings.ambientSkyColor=new Color(.48f,.55f,.65f);RenderSettings.ambientEquatorColor=new Color(.4f,.42f,.38f);RenderSettings.ambientGroundColor=new Color(.24f,.23f,.2f);RenderSettings.fog=true;RenderSettings.fogMode=FogMode.Linear;RenderSettings.fogStartDistance=145;RenderSettings.fogEndDistance=310;RenderSettings.fogColor=new Color(.28f,.37f,.46f);
   Suite("baseline-matched");
   EditorSceneManager.SaveScene(s,Root+"/BastionI_FrozenBaseline_MatchedReview.unity");
   File.WriteAllText(Evidence+"/baseline-camera-normalization.json","{\"source_orthographic_size\":"+oldSize.ToString(System.Globalization.CultureInfo.InvariantCulture)+",\"uniform_scale\":"+(24/oldSize).ToString("R",System.Globalization.CultureInfo.InvariantCulture)+",\"source_yaw_conversion\":180,\"anchor\":\"LowerGate renderer foot aligned to (691,702) on approved 1536x1024 framing\",\"geometry_and_material_sources_unchanged\":true,\"lighting\":\"same single sun, ambient, fog and pipeline as experiments\",\"original_native_view_preserved\":true}");
   Composite();Debug.Log("BREAKTHROUGH_CAPTURE_COMPLETE");EditorApplication.Exit(0);
  }catch(Exception e){Debug.LogException(e);EditorApplication.Exit(1);}
 }
 static void Composite() {
  int cell=400,totalHeight=1500;var result=new Texture2D(cell*4,totalHeight,TextureFormat.RGB24,false);var pixels=new Color32[cell*4*totalHeight];for(int i=0;i<pixels.Length;i++)pixels[i]=new Color32(24,29,35,255);
  string[,] rows=new string[,]{{"reference","baseline-matched-main","final3d-main","final25d-main"},{"reference","baseline-matched-landscape","final3d-landscape","final25d-landscape"},{"reference","baseline-matched-portrait","final3d-portrait","final25d-portrait"},{"reference","baseline-matched-pan-right","final3d-pan-right","final25d-pan-right"},{"reference","baseline-matched-zoom-in","final3d-zoom-in","final25d-zoom-in"}};
  int[] heights={270,230,430,270,270};int yTop=0;
  for(int row=0;row<5;row++){
   for(int col=0;col<4;col++){
    string file=rows[row,col]=="reference"?"../references/VALORIA_APPROVED_VISUAL_REFERENCE.jpg":Evidence+"/"+rows[row,col]+".png";
    var tex=new Texture2D(2,2,TextureFormat.RGB24,false);if(!tex.LoadImage(File.ReadAllBytes(file)))throw new Exception("Composite input invalid "+file);
    float scale=Mathf.Min((cell-8f)/tex.width,(heights[row]-8f)/tex.height);int w=Mathf.RoundToInt(tex.width*scale),h=Mathf.RoundToInt(tex.height*scale);
    var source=tex.GetPixels32();int left=col*cell+(cell-w)/2,top=yTop+(heights[row]-h)/2;
    for(int y=0;y<h;y++)for(int x=0;x<w;x++){int sy=Mathf.Min(tex.height-1,Mathf.FloorToInt(y/scale));int sx=Mathf.Min(tex.width-1,Mathf.FloorToInt(x/scale));pixels[(totalHeight-top-h+y)*cell*4+left+x]=source[sy*tex.width+sx];}
    UnityEngine.Object.DestroyImmediate(tex);
   }yTop+=heights[row];
  }
  result.SetPixels32(pixels);result.Apply();File.WriteAllBytes(Evidence+"/comparison-four-columns.jpg",result.EncodeToJPG(86));File.WriteAllBytes(Evidence+"/comparison-four-columns.png",result.EncodeToPNG());UnityEngine.Object.DestroyImmediate(result);
 }
}
}
