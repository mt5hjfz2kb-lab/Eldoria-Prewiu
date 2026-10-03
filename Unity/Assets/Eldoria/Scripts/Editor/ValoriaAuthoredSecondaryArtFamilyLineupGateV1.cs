using System;
using System.IO;
using System.Collections.Generic;
using Eldoria.Presentation;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Object=UnityEngine.Object;

namespace Eldoria.EditorTools
{
 public static class ValoriaAuthoredSecondaryArtFamilyLineupGateV1
 {
  const string Folder="ValoriaAuthoredSecondaryArtFamilyLineupV1";

  struct Family
  {
   public string id,sawmill,wall,gate;
   public Family(string id,string sawmill,string wall,string gate)
   {this.id=id;this.sawmill=sawmill;this.wall=wall;this.gate=gate;}
  }

  static readonly Family[] Families={
   new Family("canonical",
    "Assets/Eldoria/Resources/Valoria/Valoria_Aserradero_AP2_v1.glb",
    "Assets/Eldoria/Resources/Valoria/Stone_Wall.prefab",
    "Assets/Eldoria/Resources/Valoria/Stone_Gate.prefab"),
   new Family("source_semantic_simple",
    "Assets/Resources/Valoria/ASFCandidates/Valoria_ASF_Aserradero_v1.glb",
    "Assets/Resources/Valoria/ASFCandidates/Valoria_ASF_WallSupport_v1.glb",
    "Assets/Resources/Valoria/ASFCandidates/Valoria_ASF_GateSupport_v1.glb"),
   new Family("source_semantic_v2",
    "Assets/Resources/Valoria/ASFCandidates/Valoria_ASF_Aserradero_v1.glb",
    "Assets/Resources/Valoria/ASFCandidates/Valoria_ASF_SourceWall_v2.glb",
    "Assets/Resources/Valoria/ASFCandidates/Valoria_ASF_SourceGate_v2.glb"),
   new Family("modular_new",
    "Assets/Resources/Valoria/ASFCandidates/Valoria_Authored_Aserradero_v1.glb",
    "Assets/Resources/Valoria/ASFCandidates/Valoria_Authored_Wall_v1.glb",
    "Assets/Resources/Valoria/ASFCandidates/Valoria_Authored_Gate_v1.glb")
  };

  public static void Capture()
  {
   ShaderUtil.allowAsyncCompilation=false;
   Directory.CreateDirectory(Folder);
   SceneSetup.SetupRenderPipeline();
   EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);

   var ground=GameObject.CreatePrimitive(PrimitiveType.Plane);
   ground.name="ASF lineup neutral ground";ground.transform.localScale=new Vector3(4.4f,1f,3.3f);
   ground.transform.position=new Vector3(0f,-.04f,0f);
   var gm=new Material(Shader.Find("Universal Render Pipeline/Lit")){name="ASF lineup ground"};
   gm.SetColor("_BaseColor",new Color(.31f,.33f,.29f,1f));gm.SetFloat("_Smoothness",.025f);
   ground.GetComponent<Renderer>().sharedMaterial=gm;Object.DestroyImmediate(ground.GetComponent<Collider>());

   var sunGo=new GameObject("ASF lineup sun");var sun=sunGo.AddComponent<Light>();
   sun.type=LightType.Directional;sun.intensity=1.18f;sun.color=new Color(1f,.92f,.80f);
   sun.transform.rotation=Quaternion.Euler(46f,-33f,0f);
   var fillGo=new GameObject("ASF lineup fill");var fill=fillGo.AddComponent<Light>();
   fill.type=LightType.Directional;fill.intensity=.18f;fill.color=new Color(.62f,.72f,.90f);
   fill.transform.rotation=Quaternion.Euler(32f,148f,0f);
   RenderSettings.ambientMode=UnityEngine.Rendering.AmbientMode.Flat;
   RenderSettings.ambientLight=new Color(.39f,.42f,.45f,1f);

   var evidence=new List<string>();
   for(int i=0;i<Families.Length;i++)
   {
    var f=Families[i];float x=(i-1.5f)*6.65f;
    int sawTris=Place(f.sawmill,"ASF lineup · "+f.id+" · sawmill",new Vector3(x,0f,2.3f),5.25f,4.7f,18f);
    int wallTris=Place(f.wall,"ASF lineup · "+f.id+" · wall",new Vector3(x-1.65f,0f,-3.7f),3.15f,1.75f,8f);
    int gateTris=Place(f.gate,"ASF lineup · "+f.id+" · gate",new Vector3(x+1.70f,0f,-3.7f),3.15f,3.25f,-8f);
    evidence.Add("{\"id\":\""+f.id+"\",\"sawmill_triangles\":"+sawTris+",\"wall_triangles\":"+wallTris+",\"gate_triangles\":"+gateTris+"}");
   }

   var camGo=new GameObject("ASF lineup camera");var cam=camGo.AddComponent<Camera>();
   cam.clearFlags=CameraClearFlags.SolidColor;cam.backgroundColor=new Color(.12f,.15f,.17f,1f);cam.orthographic=true;
   Save(cam,Folder+"/lineup-isometric.png",new Vector3(23f,18f,-27f),new Vector3(0f,1.3f,0f),15.7f,2048,1280);
   Save(cam,Folder+"/lineup-front.png",new Vector3(0f,8.2f,-32f),new Vector3(0f,1.25f,0f),13.6f,2048,1280);
   Save(cam,Folder+"/lineup-sawmills.png",new Vector3(21f,13f,-25f),new Vector3(0f,1.8f,2.3f),11.8f,2048,1152);

   File.WriteAllText(Folder+"/evidence.json",
    "{\n  \"normalized_same_envelope\": true,\n  \"source_materials_retained\": true,\n  \"families\": [\n    "+
    string.Join(",\n    ",evidence)+"\n  ]\n}\n");
   Debug.Log("VALORIA_AUTHORED_SECONDARY_ART_FAMILY_LINEUP_V1=PASS");
   EditorApplication.Exit(0);
  }

  static int Place(string path,string name,Vector3 anchor,float span,float maxHeight,float yaw)
  {
   var src=AssetDatabase.LoadAssetAtPath<GameObject>(path);
   if(src==null)throw new InvalidOperationException("Missing ASF lineup source: "+path);
   GameObject go=path.EndsWith(".prefab",StringComparison.OrdinalIgnoreCase)
    ?(GameObject)PrefabUtility.InstantiatePrefab(src):Object.Instantiate(src);
   if(go==null)throw new InvalidOperationException("Could not instantiate ASF lineup source: "+path);
   go.name=name;go.transform.rotation=Quaternion.Euler(0,yaw,0);
   Fit(go,anchor,span,maxHeight);
   Neutralize(go);
   return Triangles(go);
  }

  static void Fit(GameObject go,Vector3 anchor,float span,float maxHeight)
  {
   var b=Bounds(go);float horizontal=Mathf.Max(.001f,Mathf.Max(b.size.x,b.size.z));
   float scale=Mathf.Min(span/horizontal,maxHeight/Mathf.Max(.001f,b.size.y));
   go.transform.localScale*=scale;b=Bounds(go);
   go.transform.position+=anchor-new Vector3(b.center.x,b.min.y,b.center.z);
  }

  static Bounds Bounds(GameObject go)
  {
   var rs=go.GetComponentsInChildren<Renderer>(true);
   if(rs.Length==0)throw new InvalidOperationException("Lineup object has no renderer: "+go.name);
   var b=rs[0].bounds;for(int i=1;i<rs.Length;i++)b.Encapsulate(rs[i].bounds);return b;
  }

  static int Triangles(GameObject go)
  {
   int n=0;
   foreach(var mf in go.GetComponentsInChildren<MeshFilter>(true))
    if(mf.sharedMesh!=null)for(int i=0;i<mf.sharedMesh.subMeshCount;i++)n+=(int)mf.sharedMesh.GetIndexCount(i)/3;
   return n;
  }

  static void Neutralize(GameObject go)
  {
   foreach(var c in go.GetComponentsInChildren<Collider>(true))Object.DestroyImmediate(c);
   foreach(var h in go.GetComponentsInChildren<WorldHotspot>(true))Object.DestroyImmediate(h);
   foreach(var mb in go.GetComponentsInChildren<MonoBehaviour>(true))if(!(mb is WorldHotspot))mb.enabled=false;
  }

  static void Save(Camera c,string path,Vector3 p,Vector3 target,float size,int w,int h)
  {
   c.transform.position=p;c.transform.LookAt(target);c.orthographicSize=size;
   var rt=new RenderTexture(w,h,24,RenderTextureFormat.ARGB32);var prev=RenderTexture.active;
   try{
    c.targetTexture=rt;c.Render();c.Render();RenderTexture.active=rt;
    var im=new Texture2D(w,h,TextureFormat.RGB24,false);im.ReadPixels(new Rect(0,0,w,h),0,0);im.Apply();
    File.WriteAllBytes(path,im.EncodeToPNG());Object.DestroyImmediate(im);
   }finally{c.targetTexture=null;RenderTexture.active=prev;rt.Release();Object.DestroyImmediate(rt);}
  }
 }
}
