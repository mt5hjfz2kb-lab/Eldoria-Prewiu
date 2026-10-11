using System;
using System.IO;
using Eldoria.Domain;
using Eldoria.Presentation;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Object=UnityEngine.Object;

namespace Eldoria.EditorTools
{
 public static class ValoriaSecondaryArchitectureReplacementGateV1
 {
  const string Folder="ValoriaSecondaryArchitectureReplacementV1Captures";
  static readonly (string id,string path,Vector3 p,float yaw,float span,float height)[] Specs={
   ("town_house_01","Assets/EmaceArt/Slavic World Free/Prefabs/Town/Building/EA03_Town_House_Comp_01a_PRE.prefab",new Vector3(-7.8f,.42f,-2.7f),12f,3.1f,3.7f),
   ("town_house_02","Assets/EmaceArt/Slavic World Free/Prefabs/Town/Building/EA03_Town_House_Comp_02a_PRE.prefab",new Vector3(7.7f,.42f,-3.2f),190f,3.0f,3.5f),
   ("town_house_03c","Assets/EmaceArt/Slavic World Free/Prefabs/Town/Building/EA03_Town_House_Comp_03c_PRE.prefab",new Vector3(-5.7f,.70f,2.8f),18f,2.7f,3.1f),
   ("admin_01a","Assets/EmaceArt/Slavic World Free/Prefabs/Town/Administrative/EA03_Town_Building_Administrative _01a_PRE.prefab",new Vector3(5.6f,.70f,2.9f),174f,2.8f,3.0f),
   ("town_house_03a","Assets/EmaceArt/Slavic World Free/Prefabs/Town/Building/EA03_Town_House_Comp_03a_PRE.prefab",new Vector3(-4.5f,1.52f,5.3f),12f,2.5f,3.0f),
   ("town_house_03b","Assets/EmaceArt/Slavic World Free/Prefabs/Town/Building/EA03_Town_House_Comp_03b_PRE.prefab",new Vector3(4.5f,1.52f,5.4f),188f,2.5f,3.0f)
  };

  public static void Capture()
  {
   ShaderUtil.allowAsyncCompilation=false;Directory.CreateDirectory(Folder);SceneSetup.SetupRenderPipeline();EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
   ProductionVisualIntegration.ResetVisualCachesForGate();
   ProductionVisualIntegration.StoneArchitectureEnabled=true;ProductionVisualIntegration.TerrainTerraceEnabled=true;ProductionVisualIntegration.SurfaceCellEnabled=false;ProductionVisualIntegration.ProductionCellEnabled=false;ProductionVisualIntegration.CoherentCastleProofEnabled=false;ProductionVisualIntegration.SlavicDistrictProofEnabled=false;ProductionVisualIntegration.CompactFootprintReframeEnabled=true;
   AssetVisualUpliftPassV1.Enabled=false;AssetLibraryReprocessingPassV1.Enabled=true;MidTierDistrictProduction.Enabled=true;ValoriaFullFrameArchitectureBatchV1.Enabled=true;ValoriaFullFrameForegroundEdgePassV1.Enabled=true;ValoriaOpenValleyCompositionV1.Enabled=true;
   ValoriaReferenceConvergencePassV2.Enabled=false;ValoriaInCitySurfacePassV1.Enabled=false;ValoriaStairLandingIntegrationV1.Enabled=false;ValoriaFullFrameConvergenceIteration1.Enabled=false;ValoriaFullFrameConvergenceIteration2.Enabled=false;ValoriaWorldFrameMountainTerrainV1.Enabled=false;VisualWorld.VisualIntegrationEnabled=true;
   var state=new PlayerState{BastionLevel=3,SawmillLevel=1,BarracksLevel=1,CorruptionDiscovered=true};VisualWorld.Create(true,state);
   var c=Camera.main;var root=GameObject.Find("Valoria · integrated construction visual layer");if(c==null||root==null)throw new Exception("capture prerequisites missing");
   var baseline=ValoriaVisualFormulaGate.CollisionSignature();
   var pos=new Vector3(18.2f,14.6f,-25.8f);var target=new Vector3(0f,3.35f,5.6f);
   Save(c,Folder+"/before.png",pos,target,9.1f,1280,720);Save(c,Folder+"/before-mobile.png",pos,target,9.1f,390,844);

   HideFamily("Valoria · Mid-Tier District v1 · production visual only");
   HideFamily("Valoria · Full Frame Architecture Batch v1");
   var proof=new GameObject("Valoria · secondary architecture replacement proof").transform;proof.SetParent(root.transform,true);
   int loaded=0;
   foreach(var s in Specs)
   {
    var src=AssetDatabase.LoadAssetAtPath<GameObject>(s.path);if(src==null)continue;
    var go=(GameObject)PrefabUtility.InstantiatePrefab(src);
    go.name="Valoria · premium secondary · "+s.id;
    go.transform.rotation=Quaternion.Euler(0f,s.yaw,0f);
    Fit(go,s.p,s.span,s.height);
    Neutralize(go);
    go.transform.SetParent(proof,true);
    foreach(var col in go.GetComponentsInChildren<Collider>(true))col.enabled=false;
    foreach(var b in go.GetComponentsInChildren<MonoBehaviour>(true))b.enabled=false;
    loaded++;
   }
   if(loaded<5)throw new Exception("insufficient premium family load: "+loaded);
   Physics.SyncTransforms();if(ValoriaVisualFormulaGate.CollisionSignature()!=baseline)throw new Exception("secondary replacement altered gameplay signature");
   Save(c,Folder+"/after.png",pos,target,9.1f,1280,720);Save(c,Folder+"/after-mobile.png",pos,target,9.1f,390,844);
   File.WriteAllText(Folder+"/evidence.json",$"{{\n  \"loaded\": {loaded},\n  \"camera_size\": 9.1,\n  \"collider_hotspot_signature_equal\": true,\n  \"existing_assets_only\": true,\n  \"tripo_credits\": 0\n}}\n");
   EditorApplication.Exit(0);
  }

  static void HideFamily(string rootName)
  {
   var go=GameObject.Find(rootName);if(go==null)return;
   foreach(var r in go.GetComponentsInChildren<Renderer>(true))r.enabled=false;
   foreach(var l in go.GetComponentsInChildren<Light>(true))l.enabled=false;
  }
  static void Fit(GameObject go,Vector3 ground,float span,float maxHeight)
  {
   var rs=go.GetComponentsInChildren<Renderer>(true);if(rs.Length==0)return;
   var b=rs[0].bounds;for(int i=1;i<rs.Length;i++)b.Encapsulate(rs[i].bounds);
   float s=Mathf.Min(span/Mathf.Max(.001f,Mathf.Max(b.size.x,b.size.z)),maxHeight/Mathf.Max(.001f,b.size.y));
   go.transform.localScale*=s;rs=go.GetComponentsInChildren<Renderer>(true);b=rs[0].bounds;for(int i=1;i<rs.Length;i++)b.Encapsulate(rs[i].bounds);
   go.transform.position+=ground-new Vector3(b.center.x,b.min.y,b.center.z);
  }
  static void Neutralize(GameObject go)
  {
   foreach(var r in go.GetComponentsInChildren<Renderer>(true))
   {
    var src=r.sharedMaterials;var dst=new Material[src.Length];
    for(int i=0;i<src.Length;i++)
    {
     if(src[i]==null){dst[i]=null;continue;}
     var m=new Material(src[i]){name="Valoria premium secondary · "+src[i].name};
     string n=(r.name+" "+src[i].name).ToLowerInvariant();
     Color tint=(n.Contains("roof")||n.Contains("tile")||n.Contains("shingle"))
       ?new Color(.34f,.39f,.40f,1f)
       :(n.Contains("wood")||n.Contains("beam")||n.Contains("timber"))
         ?new Color(.39f,.30f,.22f,1f)
         :new Color(.76f,.72f,.64f,1f);
     if(m.HasProperty("_BaseColor"))m.SetColor("_BaseColor",tint);
     else if(m.HasProperty("_Color"))m.SetColor("_Color",tint);
     if(m.HasProperty("_Smoothness"))m.SetFloat("_Smoothness",.04f);
     dst[i]=m;
    }
    r.sharedMaterials=dst;
   }
  }
  static void Save(Camera c,string path,Vector3 p,Vector3 t,float size,int w,int h)
  {
   c.transform.position=p;c.transform.LookAt(t);c.orthographic=true;c.orthographicSize=size;
   var rt=new RenderTexture(w,h,24,RenderTextureFormat.ARGB32);var prev=RenderTexture.active;
   try{c.targetTexture=rt;c.Render();c.Render();RenderTexture.active=rt;var im=new Texture2D(w,h,TextureFormat.RGB24,false);im.ReadPixels(new Rect(0,0,w,h),0,0);im.Apply();File.WriteAllBytes(path,im.EncodeToPNG());Object.DestroyImmediate(im);}
   finally{c.targetTexture=null;RenderTexture.active=prev;rt.Release();Object.DestroyImmediate(rt);}
  }
 }
}
