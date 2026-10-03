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
 public static class ValoriaVqbSecondaryLineupGate
 {
  const string Folder="ValoriaVqbSecondaryLineupCaptures";
  struct Spec
  {
   public string id,path;
   public Spec(string id,string path){this.id=id;this.path=path;}
  }

  static readonly Spec[] Specs={
   new Spec("current_aserradero","Assets/Eldoria/Resources/Valoria/Valoria_Aserradero_AP2_v1.glb"),
   new Spec("current_cuartel","Assets/Eldoria/Resources/Valoria/Valoria_Cuartel_AP2_v1.glb"),
   new Spec("current_granero","Assets/Eldoria/Resources/Valoria/Valoria_Granero_BIII_v1.glb"),
   new Spec("midtier_piece01","Assets/Eldoria/Resources/Valoria/MidTierArchitectureKit_v1/Piece01.glb"),
   new Spec("midtier_piece02","Assets/Eldoria/Resources/Valoria/MidTierArchitectureKit_v1/Piece02.glb"),
   new Spec("midtier_piece03","Assets/Eldoria/Resources/Valoria/MidTierArchitectureKit_v1/Piece03.glb"),
   new Spec("midtier_piece04","Assets/Eldoria/Resources/Valoria/MidTierArchitectureKit_v1/Piece04.glb"),
   new Spec("slavic_town01","Assets/EmaceArt/Slavic World Free/Prefabs/Town/Building/EA03_Town_House_Comp_01a_PRE.prefab"),
   new Spec("slavic_town02","Assets/EmaceArt/Slavic World Free/Prefabs/Town/Building/EA03_Town_House_Comp_02a_PRE.prefab"),
   new Spec("slavic_town03c","Assets/EmaceArt/Slavic World Free/Prefabs/Town/Building/EA03_Town_House_Comp_03c_PRE.prefab"),
   new Spec("slavic_admin01a","Assets/EmaceArt/Slavic World Free/Prefabs/Town/Administrative/EA03_Town_Building_Administrative _01a_PRE.prefab"),
   new Spec("mega_house001","Assets/Mega Fantasy Props Pack/Prefabs/Houses/House.001.prefab")
  };

  public static void Capture()
  {
   ShaderUtil.allowAsyncCompilation=false;
   Directory.CreateDirectory(Folder);
   SceneSetup.SetupRenderPipeline();
   EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);

   var root=new GameObject("Valoria VQB secondary library lineup").transform;
   var ground=GameObject.CreatePrimitive(PrimitiveType.Plane);
   ground.name="neutral ground";ground.transform.localScale=new Vector3(4.6f,1f,3.8f);
   ground.transform.position=new Vector3(0f,-.04f,0f);
   var gm=new Material(Shader.Find("Universal Render Pipeline/Lit")){name="lineup neutral ground"};
   gm.SetColor("_BaseColor",new Color(.28f,.31f,.27f,1f));gm.SetFloat("_Smoothness",.02f);
   ground.GetComponent<Renderer>().sharedMaterial=gm;Object.DestroyImmediate(ground.GetComponent<Collider>());

   var sunGo=new GameObject("lineup sun");
   var sun=sunGo.AddComponent<Light>();sun.type=LightType.Directional;sun.intensity=1.15f;
   sun.color=new Color(1f,.93f,.82f);sun.transform.rotation=Quaternion.Euler(48f,-34f,0f);
   RenderSettings.ambientMode=UnityEngine.Rendering.AmbientMode.Flat;
   RenderSettings.ambientLight=new Color(.42f,.45f,.48f,1f);

   var entries=new List<string>();
   int loaded=0;
   for(int i=0;i<Specs.Length;i++)
   {
    var s=Specs[i];
    var src=AssetDatabase.LoadAssetAtPath<GameObject>(s.path);
    if(src==null){entries.Add("{\"id\":\""+s.id+"\",\"loaded\":false}");continue;}
    GameObject go;
    if(s.path.EndsWith(".prefab",StringComparison.OrdinalIgnoreCase))
     go=(GameObject)PrefabUtility.InstantiatePrefab(src);
    else go=Object.Instantiate(src);
    if(go==null){entries.Add("{\"id\":\""+s.id+"\",\"loaded\":false}");continue;}
    int col=i%4,row=i/4;
    var anchor=new Vector3((col-1.5f)*5.4f,0f,(1-row)*5.2f);
    go.name="lineup · "+s.id;
    go.transform.rotation=Quaternion.Euler(0f,28f,0f);
    Fit(go,anchor,3.55f,4.35f);
    NeutralizeGameplay(go);
    go.transform.SetParent(root,true);
    var b=Bounds(go);
    entries.Add("{\"id\":\""+s.id+"\",\"loaded\":true,\"x\":"+anchor.x.ToString(System.Globalization.CultureInfo.InvariantCulture)+",\"z\":"+anchor.z.ToString(System.Globalization.CultureInfo.InvariantCulture)+",\"height\":"+b.size.y.ToString(System.Globalization.CultureInfo.InvariantCulture)+"}");
    loaded++;
   }
   if(loaded<10)throw new Exception("Secondary lineup loaded only "+loaded+" / "+Specs.Length);

   var camGo=new GameObject("lineup camera");var c=camGo.AddComponent<Camera>();
   c.clearFlags=CameraClearFlags.SolidColor;c.backgroundColor=new Color(.12f,.15f,.17f,1f);
   c.orthographic=true;
   Save(c,Folder+"/lineup-isometric.png",new Vector3(18f,18f,-22f),new Vector3(0f,1.6f,0f),14.8f,2048,1536);
   Save(c,Folder+"/lineup-front.png",new Vector3(0f,8.5f,-28f),new Vector3(0f,1.6f,0f),13.2f,2048,1536);

   File.WriteAllText(Folder+"/evidence.json","{\n  \"loaded\": "+loaded+",\n  \"normalized_same_envelope\": true,\n  \"material_authenticity\": \"source materials retained\",\n  \"entries\": [\n    "+string.Join(",\n    ",entries)+"\n  ]\n}\n");
   Debug.Log("VALORIA_VQB_SECONDARY_LINEUP=PASS");
   EditorApplication.Exit(0);
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
   var rs=go.GetComponentsInChildren<Renderer>(true);if(rs.Length==0)return new Bounds(go.transform.position,Vector3.one);
   var b=rs[0].bounds;for(int i=1;i<rs.Length;i++)b.Encapsulate(rs[i].bounds);return b;
  }

  static void NeutralizeGameplay(GameObject go)
  {
   foreach(var c in go.GetComponentsInChildren<Collider>(true))c.enabled=false;
   foreach(var h in go.GetComponentsInChildren<WorldHotspot>(true))Object.DestroyImmediate(h);
   foreach(var mb in go.GetComponentsInChildren<MonoBehaviour>(true))if(!(mb is WorldHotspot))mb.enabled=false;
  }

  static void Save(Camera c,string path,Vector3 p,Vector3 t,float size,int w,int h)
  {
   c.transform.position=p;c.transform.LookAt(t);c.orthographicSize=size;
   var rt=new RenderTexture(w,h,24,RenderTextureFormat.ARGB32);var prev=RenderTexture.active;
   try{
    c.targetTexture=rt;c.Render();c.Render();RenderTexture.active=rt;
    var im=new Texture2D(w,h,TextureFormat.RGB24,false);im.ReadPixels(new Rect(0,0,w,h),0,0);im.Apply();
    File.WriteAllBytes(path,im.EncodeToPNG());Object.DestroyImmediate(im);
   }finally{c.targetTexture=null;RenderTexture.active=prev;rt.Release();Object.DestroyImmediate(rt);}
  }
 }
}
