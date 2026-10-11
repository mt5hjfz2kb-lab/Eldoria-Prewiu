using System;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Object=UnityEngine.Object;

namespace Eldoria.EditorTools
{
 public static class ValoriaMegaArchitectureAtlasGateV1
 {
  const string Folder="ValoriaMegaArchitectureAtlasV1Captures";
  static readonly (string id,string path)[] Items={
   ("half_gate","Assets/Mega Fantasy Props Pack/Prefabs/Castle walls/stone_half_gate.prefab"),
   ("half_gate_001","Assets/Mega Fantasy Props Pack/Prefabs/Castle walls/stone_half_gate.001.prefab"),
   ("wall_passage","Assets/Mega Fantasy Props Pack/Prefabs/Castle walls/Wall passage/wall_passage.prefab"),
   ("wall_passage_001","Assets/Mega Fantasy Props Pack/Prefabs/Castle walls/Wall passage/wall_passage.001.prefab"),
   ("wall_passage_003","Assets/Mega Fantasy Props Pack/Prefabs/Castle walls/Wall passage/wall_passage.003.prefab"),
   ("wall_passage_005","Assets/Mega Fantasy Props Pack/Prefabs/Castle walls/Wall passage/wall_passage.005.prefab"),
   ("wall_passage_007","Assets/Mega Fantasy Props Pack/Prefabs/Castle walls/Wall passage/wall_passage.007.prefab"),
   ("tower_destroyed","Assets/Mega Fantasy Props Pack/Prefabs/Castle walls/Tower/tower_destroyed.prefab"),
   ("tower_small_destroyed","Assets/Mega Fantasy Props Pack/Prefabs/Castle walls/Tower/tower_small_destroyed.prefab"),
   ("tower_small_destroyed_001","Assets/Mega Fantasy Props Pack/Prefabs/Castle walls/Tower/tower_small_destroyed.001.prefab"),
   ("wall_detailed","Assets/Mega Fantasy Props Pack/Prefabs/Castle walls/stone_wall_detailed.prefab"),
   ("wall_detailed_corner","Assets/Mega Fantasy Props Pack/Prefabs/Castle walls/stone_wall_detailed_corner.prefab"),
   ("wall_detailed_corner_001","Assets/Mega Fantasy Props Pack/Prefabs/Castle walls/stone_wall_detailed_corner.001.prefab"),
   ("stone_bridge","Assets/Mega Fantasy Props Pack/Prefabs/Miscellaneous/Bridges/stone_bridge.prefab"),
   ("stone_bridge_connector","Assets/Mega Fantasy Props Pack/Prefabs/Miscellaneous/Bridges/stone_bridge_connector.prefab"),
   ("stone_column","Assets/Mega Fantasy Props Pack/Prefabs/Columns/Stone/stone_column.prefab")
  };

  public static void Capture()
  {
   ShaderUtil.allowAsyncCompilation=false;
   Directory.CreateDirectory(Folder);
   SceneSetup.SetupRenderPipeline();
   EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);

   var root=new GameObject("Mega Architecture Atlas v1").transform;
   var ground=GameObject.CreatePrimitive(PrimitiveType.Plane);
   ground.name="atlas ground";ground.transform.localScale=new Vector3(7f,1f,5f);
   ground.GetComponent<Renderer>().sharedMaterial=Mat(new Color(.18f,.18f,.17f,1f));

   var sun=new GameObject("atlas sun").AddComponent<Light>();
   sun.type=LightType.Directional;sun.intensity=1.45f;sun.color=new Color(1f,.91f,.78f);sun.transform.rotation=Quaternion.Euler(48f,-32f,0f);
   RenderSettings.ambientMode=UnityEngine.Rendering.AmbientMode.Trilight;
   RenderSettings.ambientSkyColor=new Color(.58f,.66f,.72f);RenderSettings.ambientEquatorColor=new Color(.35f,.34f,.31f);RenderSettings.ambientGroundColor=new Color(.18f,.17f,.16f);RenderSettings.ambientIntensity=.9f;

   int loaded=0;
   for(int i=0;i<Items.Length;i++)
   {
    var item=Items[i];var src=AssetDatabase.LoadAssetAtPath<GameObject>(item.path);if(src==null)continue;
    int col=i%4,row=i/4;
    var go=(GameObject)PrefabUtility.InstantiatePrefab(src);
    go.name="atlas · "+item.id;go.transform.rotation=Quaternion.Euler(0f,35f,0f);
    Fit(go,new Vector3(-9f+col*6f,.05f,7.2f-row*5.4f),4.0f,4.5f);
    go.transform.SetParent(root,true);
    foreach(var c in go.GetComponentsInChildren<Collider>(true))c.enabled=false;
    loaded++;
   }
   if(loaded<12)throw new Exception("Mega atlas loaded only "+loaded+" candidates");

   var camGo=new GameObject("atlas camera");var cam=camGo.AddComponent<Camera>();cam.orthographic=true;cam.orthographicSize=13.2f;cam.clearFlags=CameraClearFlags.SolidColor;cam.backgroundColor=new Color(.46f,.58f,.65f);camGo.transform.position=new Vector3(19f,18f,-27f);camGo.transform.LookAt(new Vector3(0f,1.6f,-1f));
   Save(cam,Folder+"/mega-architecture-grid.png",1536,1024);

   for(int i=0;i<Items.Length;i++)
   {
    var go=GameObject.Find("atlas · "+Items[i].id);if(go==null)continue;
    foreach(var r in Object.FindObjectsByType<Renderer>(FindObjectsSortMode.None))
      if(r.gameObject!=ground)r.enabled=r.transform==go.transform||r.transform.IsChildOf(go.transform);
    cam.orthographicSize=3.4f;var b=Bounds(go);camGo.transform.position=b.center+new Vector3(5.4f,4.2f,-7.8f);camGo.transform.LookAt(b.center+Vector3.up*.3f);
    Save(cam,Folder+"/"+Items[i].id+".png",1024,768);
   }
   File.WriteAllText(Folder+"/evidence.json",$"{{\n  \"loaded\": {loaded},\n  \"candidate_count\": {Items.Length},\n  \"source\": \"Mega Fantasy Props Pack already in project\",\n  \"tripo_credits\": 0\n}}\n");
   EditorApplication.Exit(0);
  }

  static void Fit(GameObject go,Vector3 ground,float span,float maxHeight)
  {
   var b=Bounds(go);float s=Mathf.Min(span/Mathf.Max(.001f,Mathf.Max(b.size.x,b.size.z)),maxHeight/Mathf.Max(.001f,b.size.y));
   go.transform.localScale*=s;b=Bounds(go);go.transform.position+=ground-new Vector3(b.center.x,b.min.y,b.center.z);
  }
  static Bounds Bounds(GameObject go)
  {
   var rs=go.GetComponentsInChildren<Renderer>(true);if(rs.Length==0)return new Bounds(go.transform.position,Vector3.one);
   var b=rs[0].bounds;for(int i=1;i<rs.Length;i++)b.Encapsulate(rs[i].bounds);return b;
  }
  static Material Mat(Color c)
  {
   var s=Shader.Find("Universal Render Pipeline/Lit")??Shader.Find("Standard");var m=new Material(s);
   if(m.HasProperty("_BaseColor"))m.SetColor("_BaseColor",c);if(m.HasProperty("_Color"))m.SetColor("_Color",c);return m;
  }
  static void Save(Camera c,string path,int w,int h)
  {
   var rt=new RenderTexture(w,h,24,RenderTextureFormat.ARGB32);var prev=RenderTexture.active;
   try{c.targetTexture=rt;c.Render();c.Render();RenderTexture.active=rt;var im=new Texture2D(w,h,TextureFormat.RGB24,false);im.ReadPixels(new Rect(0,0,w,h),0,0);im.Apply();File.WriteAllBytes(path,im.EncodeToPNG());Object.DestroyImmediate(im);}
   finally{c.targetTexture=null;RenderTexture.active=prev;rt.Release();Object.DestroyImmediate(rt);}
  }
 }
}
