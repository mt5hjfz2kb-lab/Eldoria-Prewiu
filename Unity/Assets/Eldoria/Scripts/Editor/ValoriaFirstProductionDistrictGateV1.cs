using System;
using System.Collections.Generic;
using System.IO;
using Eldoria.Domain;
using Eldoria.Presentation;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Object=UnityEngine.Object;

namespace Eldoria.EditorTools
{
 public static class ValoriaFirstProductionDistrictGateV1
 {
  const string Folder="ValoriaFirstProductionDistrictV1Captures";
  public static void Capture()
  {
   ShaderUtil.allowAsyncCompilation=false;Directory.CreateDirectory(Folder);
   var state=new PlayerState{BastionLevel=3,SawmillLevel=1,BarracksLevel=1,CorruptionDiscovered=true};
   var scene=Create(state);var sig=ValoriaVisualFormulaGate.CollisionSignature();

   ValoriaProductionArtResetV1.Apply(scene.root.transform,state);
   ValoriaProductionFinalLookV1.Apply();
   ValoriaProductionDensityLifeV1.Enabled=true;ValoriaProductionDensityLifeV1.Apply(scene.root.transform,state);
   Physics.SyncTransforms();
   if(ValoriaVisualFormulaGate.CollisionSignature()!=sig)throw new Exception("Baseline changed gameplay signature");
   WriteMetrics(Folder+"/before-metrics.json");
   Save(scene.camera,Folder+"/before-9.png",9f,1280,720);
   Save(scene.camera,Folder+"/before-mobile.png",9.4f,390,844);

   ValoriaFirstProductionDistrictV1.Apply(scene.root.transform,state);Physics.SyncTransforms();
   if(ValoriaFirstProductionDistrictV1.Pieces!=7)throw new Exception("Expected 7 production pieces");
   if(ValoriaVisualFormulaGate.CollisionSignature()!=sig)throw new Exception("FPD changed gameplay collider/hotspot signature");
   WriteMetrics(Folder+"/after-metrics.json");
   Save(scene.camera,Folder+"/after-9.png",9f,1280,720);
   Save(scene.camera,Folder+"/after-mobile.png",9.4f,390,844);
   File.WriteAllText(Folder+"/evidence.json","{\n  \"gameplay_signature_preserved\": true,\n  \"pieces\": 7,\n  \"camera\": \"ORTHOGRAPHIC\",\n  \"final_look\": \"RESET_ACCEPTED\",\n  \"tripo_credits\": 0,\n  \"paid_credits\": 0\n}\n");
   EditorApplication.Exit(0);
  }

  struct SceneData{public Camera camera;public GameObject root;}
  static SceneData Create(PlayerState state)
  {
   SceneSetup.SetupRenderPipeline();EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
   ProductionVisualIntegration.ResetVisualCachesForGate();
   ProductionVisualIntegration.StoneArchitectureEnabled=true;ProductionVisualIntegration.TerrainTerraceEnabled=true;
   ProductionVisualIntegration.SurfaceCellEnabled=false;ProductionVisualIntegration.ProductionCellEnabled=false;
   ProductionVisualIntegration.CoherentCastleProofEnabled=false;ProductionVisualIntegration.SlavicDistrictProofEnabled=false;
   ProductionVisualIntegration.CompactFootprintReframeEnabled=true;AssetVisualUpliftPassV1.Enabled=false;
   AssetLibraryReprocessingPassV1.Enabled=true;VisualWorld.VisualIntegrationEnabled=true;VisualWorld.Create(true,state);
   var camera=Camera.main;var root=GameObject.Find("Valoria · integrated construction visual layer");
   if(camera==null||root==null)throw new Exception("FPD capture prerequisites missing");
   return new SceneData{camera=camera,root=root};
  }

  static void Save(Camera c,string path,float size,int w,int h)
  {
   c.transform.position=new Vector3(18.2f,14.6f,-25.8f);c.transform.LookAt(new Vector3(0f,3.15f,5.8f));c.orthographic=true;c.orthographicSize=size;
   var rt=new RenderTexture(w,h,24,RenderTextureFormat.ARGB32){antiAliasing=4};var prev=RenderTexture.active;
   try{c.targetTexture=rt;c.Render();c.Render();RenderTexture.active=rt;var im=new Texture2D(w,h,TextureFormat.RGB24,false);im.ReadPixels(new Rect(0,0,w,h),0,0);im.Apply();File.WriteAllBytes(path,im.EncodeToPNG());Object.DestroyImmediate(im);}
   finally{c.targetTexture=null;RenderTexture.active=prev;rt.Release();Object.DestroyImmediate(rt);}
  }

  static void WriteMetrics(string path)
  {
   int renderers=0,lights=0;long triangles=0;var materials=new HashSet<string>();
   foreach(var r in Object.FindObjectsByType<Renderer>(FindObjectsSortMode.None)){if(r==null||!r.enabled||!r.gameObject.activeInHierarchy)continue;renderers++;foreach(var m in r.sharedMaterials)if(m!=null)materials.Add(m.name);}
   foreach(var mf in Object.FindObjectsByType<MeshFilter>(FindObjectsSortMode.None)){if(mf==null||mf.sharedMesh==null||!mf.gameObject.activeInHierarchy)continue;var rr=mf.GetComponent<Renderer>();if(rr!=null&&!rr.enabled)continue;for(int s=0;s<mf.sharedMesh.subMeshCount;s++)triangles+=(long)mf.sharedMesh.GetIndexCount(s)/3L;}
   foreach(var l in Object.FindObjectsByType<Light>(FindObjectsSortMode.None))if(l!=null&&l.enabled&&l.gameObject.activeInHierarchy)lights++;
   File.WriteAllText(path,"{\n  \"active_renderers\": "+renderers+",\n  \"unique_materials\": "+materials.Count+",\n  \"scene_triangles\": "+triangles+",\n  \"active_lights\": "+lights+"\n}\n");
  }
 }
}
