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
 public static class ValoriaJaggedBackdropGateV1
 {
  const string Folder="ValoriaJaggedBackdropV1Captures";
  public static void Capture()
  {
   ShaderUtil.allowAsyncCompilation=false;Directory.CreateDirectory(Folder);SceneSetup.SetupRenderPipeline();EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
   ProductionVisualIntegration.ResetVisualCachesForGate();
   ProductionVisualIntegration.StoneArchitectureEnabled=true;ProductionVisualIntegration.TerrainTerraceEnabled=true;ProductionVisualIntegration.SurfaceCellEnabled=false;ProductionVisualIntegration.ProductionCellEnabled=false;ProductionVisualIntegration.CoherentCastleProofEnabled=false;ProductionVisualIntegration.SlavicDistrictProofEnabled=false;ProductionVisualIntegration.CompactFootprintReframeEnabled=true;
   AssetVisualUpliftPassV1.Enabled=false;AssetLibraryReprocessingPassV1.Enabled=true;MidTierDistrictProduction.Enabled=true;ValoriaFullFrameArchitectureBatchV1.Enabled=true;ValoriaFullFrameForegroundEdgePassV1.Enabled=true;ValoriaOpenValleyCompositionV1.Enabled=true;
   ValoriaReferenceConvergencePassV2.Enabled=false;ValoriaInCitySurfacePassV1.Enabled=false;ValoriaStairLandingIntegrationV1.Enabled=false;ValoriaFullFrameConvergenceIteration1.Enabled=false;ValoriaFullFrameConvergenceIteration2.Enabled=false;ValoriaWorldFrameMountainTerrainV1.Enabled=false;ValoriaJaggedBackdropV1.Enabled=false;VisualWorld.VisualIntegrationEnabled=true;

   var state=new PlayerState{BastionLevel=3,SawmillLevel=1,BarracksLevel=1,CorruptionDiscovered=true};VisualWorld.Create(true,state);
   var c=Camera.main;var root=GameObject.Find("Valoria · integrated construction visual layer");if(c==null||root==null)throw new Exception("capture prerequisites missing");
   var baseline=ValoriaVisualFormulaGate.CollisionSignature();var p=new Vector3(18.2f,14.6f,-25.8f);var t=new Vector3(0f,3.35f,5.6f);
   Save(c,Folder+"/before.png",p,t,9.1f,1280,720);Save(c,Folder+"/before-mobile.png",p,t,9.1f,390,844);

   ValoriaJaggedBackdropV1.Enabled=true;
   float[] biases={-5.0f,-3.0f,-1.0f,1.0f,3.0f};
   for(int i=0;i<biases.Length;i++)
   {
    ValoriaJaggedBackdropV1.VerticalBias=biases[i];
    if(!ValoriaJaggedBackdropV1.Build(root.transform,state))throw new Exception("Jagged peaks backdrop unavailable");
    Save(c,Folder+"/after-bias"+i+".png",p,t,9.1f,1280,720);
    Save(c,Folder+"/after-bias"+i+"-mobile.png",p,t,9.1f,390,844);
   }
   Physics.SyncTransforms();if(ValoriaVisualFormulaGate.CollisionSignature()!=baseline)throw new Exception("backdrop altered gameplay signature");
   File.WriteAllText(Folder+"/evidence.json","{\n  \"source\": \"Wikimedia Commons - Jagged peaks over a valley\",\n  \"license\": \"CC0\",\n  \"camera_size\": 9.1,\n  \"vertical_biases\": [-5,-3,-1,1,3],\n  \"collider_hotspot_signature_equal\": true,\n  \"tripo_credits\": 0\n}\n");
   EditorApplication.Exit(0);
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
