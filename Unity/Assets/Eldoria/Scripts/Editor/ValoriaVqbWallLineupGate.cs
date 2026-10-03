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
 public static class ValoriaVqbWallLineupGate
 {
  const string Folder="ValoriaVqbWallLineupCaptures";
  struct Spec
  {
   public string id,path;
   public Spec(string id,string path){this.id=id;this.path=path;}
  }

  static readonly Spec[] Specs={
   new Spec("simple_stone_wall","Assets/Eldoria/Resources/Valoria/Stone_Wall.prefab"),
   new Spec("stone_arch_high_straight","Assets/Eldoria/Resources/Valoria/StoneArchitectureKit_v1/HighStraightWall.glb"),
   new Spec("stone_arch_corner_l","Assets/Eldoria/Resources/Valoria/StoneArchitectureKit_v1/CornerWallL.glb"),
   new Spec("mega_wall_inventory","Assets/Eldoria/Resources/WorldInventory/MegaWall.fbx"),
   new Spec("mega_wall_detailed","Assets/Mega Fantasy Props Pack/Prefabs/Castle walls/stone_wall_detailed.prefab"),
   new Spec("mega_wall_corner","Assets/Mega Fantasy Props Pack/Prefabs/Castle walls/stone_wall_detailed_corner.prefab"),
   new Spec("simple_stone_gate","Assets/Eldoria/Resources/Valoria/Stone_Gate.prefab"),
   new Spec("simple_stone_tower","Assets/Eldoria/Resources/Valoria/Stone_Tower.prefab"),
   new Spec("mega_gate","Assets/Eldoria/Resources/WorldInventory/MegaGate.fbx"),
   new Spec("mega_tower","Assets/Eldoria/Resources/WorldInventory/MegaTower.fbx")
  };

  public static void Capture()
  {
   ShaderUtil.allowAsyncCompilation=false;
   Directory.CreateDirectory(Folder);
   SceneSetup.SetupRenderPipeline();
   EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);

   var root=new GameObject("Valoria VQB wall library lineup").transform;
   var ground=GameObject.CreatePrimitive(PrimitiveType.Plane);
   ground.transform.localScale=new Vector3(4.8f,1f,3.4f);ground.transform.position=new Vector3(0f,-.04f,0f);
   var gm=new Material(Shader.Find("Universal Render Pipeline/Lit")){name="wall lineup neutral ground"};
   gm.SetColor("_BaseColor",new Color(.30f,.32f,.28f,1f));gm.SetFloat("_Smoothness",.02f);
   ground.GetComponent<Renderer>().sharedMaterial=gm;Object.DestroyImmediate(ground.GetComponent<Collider>());

   var sunGo=new GameObject("wall lineup sun");var sun=sunGo.AddComponent<Light>();
   sun.type=LightType.Directional;sun.intensity=1.18f;sun.color=new Color(1f,.93f,.82f);sun.transform.rotation=Quaternion.Euler(48f,-34f,0f);
   RenderSettings.ambientMode=UnityEngine.Rendering.AmbientMode.Flat;RenderSettings.ambientLight=new Color(.43f,.46f,.49f,1f);

   var entries=new List<string>();int loaded=0;
   for(int i=0;i<Specs.Length;i++)
   {
    var s=Specs[i];var src=AssetDatabase.LoadAssetAtPath<GameObject>(s.path);
    if(src==null){entries.Add("{\"id\":\""+s.id+"\",\"loaded\":false}");continue;}
    GameObject go=s.path.EndsWith(".prefab",StringComparison.OrdinalIgnoreCase)
      ?(GameObject)PrefabUtility.InstantiatePrefab(src):Object.Instantiate(src);
    if(go==null){entries.Add("{\"id\":\""+s.id+"\",\"loaded\":false}");continue;}
    int col=i%5,row=i/5;
    var anchor=new Vector3((col-2f)*4.6f,0f,(.5f-row)*6.0f);
    go.name="wall-lineup · "+s.id;go.transform.rotation=Quaternion.Euler(0f,18f,0f);
    Fit(go,anchor,3.75f,4.0f);Neutralize(go);go.transform.SetParent(root,true);
    var b=Bounds(go);
    entries.Add("{\"id\":\""+s.id+"\",\"loaded\":true,\"x\":"+F(anchor.x)+",\"z\":"+F(anchor.z)+",\"height\":"+F(b.size.y)+",\"width\":"+F(b.size.x)+",\"depth\":"+F(b.size.z)+"}");
    loaded++;
   }
   if(loaded<8)throw new Exception("Wall lineup loaded only "+loaded+" / "+Specs.Length);

   var camGo=new GameObject("wall lineup camera");var c=camGo.AddComponent<Camera>();
   c.clearFlags=CameraClearFlags.SolidColor;c.backgroundColor=new Color(.12f,.15f,.17f,1f);c.orthographic=true;
   Save(c,Folder+"/wall-lineup-isometric.png",new Vector3(18f,16f,-23f),new Vector3(0f,1.4f,0f),12.6f,2048,1280);
   Save(c,Folder+"/wall-lineup-front.png",new Vector3(0f,6.8f,-26f),new Vector3(0f,1.4f,0f),10.8f,2048,1280);

   File.WriteAllText(Folder+"/evidence.json","{\n  \"loaded\": "+loaded+",\n  \"normalized_same_envelope\": true,\n  \"source_materials_retained\": true,\n  \"entries\": [\n    "+string.Join(",\n    ",entries)+"\n  ]\n}\n");
   Debug.Log("VALORIA_VQB_WALL_LINEUP=PASS");EditorApplication.Exit(0);
  }

  static string F(float v)=>v.ToString(System.Globalization.CultureInfo.InvariantCulture);
  static void Fit(GameObject go,Vector3 anchor,float span,float maxHeight)
  {
   var b=Bounds(go);float horizontal=Mathf.Max(.001f,Mathf.Max(b.size.x,b.size.z));
   float scale=Mathf.Min(span/horizontal,maxHeight/Mathf.Max(.001f,b.size.y));
   go.transform.localScale*=scale;b=Bounds(go);
   go.transform.position+=anchor-new Vector3(b.center.x,b.min.y,b.center.z);
  }
  static Bounds Bounds(GameObject go)
  {
   var rs=go.GetComponentsInChildren<Renderer>(true);if(rs.Length==0)return new Bounds(go.transform.position,Vector3.one);
   var b=rs[0].bounds;for(int i=1;i<rs.Length;i++)b.Encapsulate(rs[i].bounds);return b;
  }
  static void Neutralize(GameObject go)
  {
   foreach(var c in go.GetComponentsInChildren<Collider>(true))c.enabled=false;
   foreach(var h in go.GetComponentsInChildren<WorldHotspot>(true))Object.DestroyImmediate(h);
   foreach(var mb in go.GetComponentsInChildren<MonoBehaviour>(true))if(!(mb is WorldHotspot))mb.enabled=false;
  }
  static void Save(Camera c,string path,Vector3 p,Vector3 t,float size,int w,int h)
  {
   c.transform.position=p;c.transform.LookAt(t);c.orthographicSize=size;
   var rt=new RenderTexture(w,h,24,RenderTextureFormat.ARGB32);var prev=RenderTexture.active;
   try{c.targetTexture=rt;c.Render();c.Render();RenderTexture.active=rt;
    var im=new Texture2D(w,h,TextureFormat.RGB24,false);im.ReadPixels(new Rect(0,0,w,h),0,0);im.Apply();
    File.WriteAllBytes(path,im.EncodeToPNG());Object.DestroyImmediate(im);}
   finally{c.targetTexture=null;RenderTexture.active=prev;rt.Release();Object.DestroyImmediate(rt);}
  }
 }
}
