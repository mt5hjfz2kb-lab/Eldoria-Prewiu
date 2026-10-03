using System;
using System.IO;
using System.Collections.Generic;
using Eldoria.Domain;
using Eldoria.Presentation;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Object=UnityEngine.Object;

namespace Eldoria.EditorTools
{
 public static class ValoriaSourceLevelArtDirectionGateV1
 {
  const string Folder="ValoriaSourceLevelArtDirectionV1Captures";
  public static void CapturePhase1()
  {
   ShaderUtil.allowAsyncCompilation=false;Directory.CreateDirectory(Folder);
   var state=new PlayerState{BastionLevel=6,SawmillLevel=2,BarracksLevel=2,CorruptionDiscovered=true};
   var p=new Vector3(18.2f,18.4f,-26.8f);var t=new Vector3(0f,1.55f,1.55f);
   var b=Create(state);var sig=ValoriaVisualFormulaGate.CollisionSignature();
   ApplyAccepted(b.root.transform,state);RemoveAddedGameplay(b.cols,b.hotspots);
   if(ValoriaVisualFormulaGate.CollisionSignature()!=sig)throw new Exception("BEFORE changed gameplay signature");
   Save(b.camera,Folder+"/before-9.png",p,t,9f,1280,720);
   Save(b.camera,Folder+"/before-mobile.png",p,new Vector3(0f,1.7f,1.8f),9.4f,390,844);

   var a=Create(state);sig=ValoriaVisualFormulaGate.CollisionSignature();
   ApplyAccepted(a.root.transform,state);
   ValoriaSourceLevelArtDirectionV1.Apply(a.root.transform,state);
   RemoveAddedGameplay(a.cols,a.hotspots);Physics.SyncTransforms();
   if(ValoriaVisualFormulaGate.CollisionSignature()!=sig)throw new Exception("Candidate altered gameplay signature");
   Save(a.camera,Folder+"/after-9.png",p,t,9f,1280,720);
   Save(a.camera,Folder+"/after-mobile.png",p,new Vector3(0f,1.7f,1.8f),9.4f,390,844);
   File.WriteAllText(Folder+"/evidence.json","{\n  \"phase\": 1,\n  \"views\": [\"9\",\"mobile\"],\n  \"modules\": "+ValoriaSourceLevelArtDirectionV1.Modules+",\n  \"gameplay_signature_preserved\": true,\n  \"tripo_credits\": 0\n}\n");
   Debug.Log("VALORIA_SOURCE_LEVEL_ART_DIRECTION_PHASE1=PASS");EditorApplication.Exit(0);
  }
  struct S{public Camera camera;public GameObject root;public HashSet<int> cols;public HashSet<int> hotspots;}
  static S Create(PlayerState state)
  {
   SceneSetup.SetupRenderPipeline();EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
   ProductionVisualIntegration.ResetVisualCachesForGate();ProductionVisualIntegration.StoneArchitectureEnabled=true;
   ProductionVisualIntegration.TerrainTerraceEnabled=true;ProductionVisualIntegration.SurfaceCellEnabled=false;
   ProductionVisualIntegration.ProductionCellEnabled=false;ProductionVisualIntegration.CoherentCastleProofEnabled=false;
   ProductionVisualIntegration.SlavicDistrictProofEnabled=false;ProductionVisualIntegration.CompactFootprintReframeEnabled=true;
   AssetVisualUpliftPassV1.Enabled=false;AssetLibraryReprocessingPassV1.Enabled=true;MidTierDistrictProduction.Enabled=true;
   ValoriaFullFrameArchitectureBatchV1.Enabled=true;ValoriaOpenValleyCompositionV1.Enabled=false;ValoriaReferenceConvergencePassV2.Enabled=false;
   ValoriaInCitySurfacePassV1.Enabled=false;ValoriaStairLandingIntegrationV1.Enabled=false;ValoriaFullFrameConvergenceIteration1.Enabled=false;
   ValoriaFullFrameConvergenceIteration2.Enabled=false;VisualWorld.VisualIntegrationEnabled=true;VisualWorld.Create(true,state);
   var c=Camera.main;var root=GameObject.Find("Valoria · integrated construction visual layer");
   if(c==null||root==null)throw new Exception("Canonical scene missing");
   var cs=new HashSet<int>();foreach(var x in Object.FindObjectsByType<Collider>(FindObjectsSortMode.None))if(x!=null)cs.Add(x.GetInstanceID());
   var hs=new HashSet<int>();foreach(var x in Object.FindObjectsByType<WorldHotspot>(FindObjectsSortMode.None))if(x!=null)hs.Add(x.GetInstanceID());
   return new S{camera=c,root=root,cols=cs,hotspots=hs};
  }
  static void ApplyAccepted(Transform root,PlayerState state)
  {
   ValoriaFlatCitadelProductionUpliftV1.WallUpliftEnabled=true;ValoriaFlatCitadelProductionUpliftV1.GroundUpliftEnabled=true;
   ValoriaFlatCitadelProductionUpliftV1.BastionIntegrationUpliftEnabled=true;ValoriaFlatCitadelProductionUpliftV1.FunctionalBuildingUpliftEnabled=true;
   ValoriaFlatCitadelProductionUpliftV1.DressingUpliftEnabled=true;
   ValoriaFlatCitadelProductionUpliftV1.Build(root,state);ValoriaFlatCitadelArtConsolidationV1.Apply(root,state);
   ValoriaAssetCoherenceV1.Apply(root,state);ValoriaVisualQualityBreakthroughV1.Apply(root,state);
  }
  static void RemoveAddedGameplay(HashSet<int> c,HashSet<int> h)
  {
   foreach(var x in Object.FindObjectsByType<Collider>(FindObjectsSortMode.None))if(x!=null&&!c.Contains(x.GetInstanceID()))Object.DestroyImmediate(x);
   foreach(var x in Object.FindObjectsByType<WorldHotspot>(FindObjectsSortMode.None))if(x!=null&&!h.Contains(x.GetInstanceID()))Object.DestroyImmediate(x);
  }
  static void Save(Camera c,string path,Vector3 p,Vector3 t,float size,int w,int h)
  {
   c.transform.position=p;c.transform.LookAt(t);c.orthographic=true;c.orthographicSize=size;
   c.clearFlags=CameraClearFlags.SolidColor;c.backgroundColor=new Color(.33f,.39f,.42f,1);
   var rt=new RenderTexture(w,h,24,RenderTextureFormat.ARGB32){antiAliasing=4};var prev=RenderTexture.active;
   try{c.targetTexture=rt;foreach(var r in Object.FindObjectsByType<Renderer>(FindObjectsSortMode.None))if(r.enabled)foreach(var m in r.sharedMaterials)if(m!=null)for(int pass=0;pass<m.passCount;pass++)ShaderUtil.CompilePass(m,pass,true);
    c.Render();c.Render();RenderTexture.active=rt;var im=new Texture2D(w,h,TextureFormat.RGB24,false);im.ReadPixels(new Rect(0,0,w,h),0,0);im.Apply();File.WriteAllBytes(path,im.EncodeToPNG());Object.DestroyImmediate(im);}
   finally{c.targetTexture=null;RenderTexture.active=prev;rt.Release();Object.DestroyImmediate(rt);}
  }
 }
}
