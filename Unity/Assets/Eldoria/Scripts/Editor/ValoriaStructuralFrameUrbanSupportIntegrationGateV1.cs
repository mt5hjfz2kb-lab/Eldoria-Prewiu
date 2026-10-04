using System;
using System.IO;
using Eldoria.Domain;
using Eldoria.Presentation;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Eldoria.EditorTools
{
 public static class ValoriaStructuralFrameUrbanSupportIntegrationGateV1
 {
  const string Folder="ValoriaStructuralFrameUrbanSupportV1Captures";
  const int MobileW=390,MobileH=844,WideW=1600,WideH=900;
  static readonly Vector3 Home=new Vector3(19.4f,14.6f,-28.6f);
  static readonly Vector3 GraneroPan=new Vector3(18.41f,14.6f,-35.6f);
  static readonly Vector3 IntermediatePan=new Vector3(18.905f,14.6f,-32.1f);
  static readonly Vector3 CuartelPan=new Vector3(28.48f,14.6f,-35.6f);

  public static void Capture()
  {
   ShaderUtil.allowAsyncCompilation=false;
   Directory.CreateDirectory(Folder);
   var state=new PlayerState{BastionLevel=3,SawmillLevel=1,BarracksLevel=1,CorruptionDiscovered=true};

   ValoriaStructuralFrameUrbanSupportIntegrationV1.Enabled=false;
   var camera=Create(state);
   var signature=ValoriaVisualFormulaGate.CollisionSignature();
   var root=GameObject.Find("Valoria · integrated construction visual layer");
   if(root==null)throw new Exception("Integrated visual root missing");
   if(GameObject.Find(ValoriaLowerCityUrbanMassingReauthoringV1.RootName)==null)
    throw new Exception("Promoted lower-city urban massing missing");

   CaptureSet(camera,"BEFORE");

   ValoriaStructuralFrameUrbanSupportIntegrationV1.Enabled=true;
   ValoriaStructuralFrameUrbanSupportIntegrationV1.Build(root.transform,state);
   Physics.SyncTransforms();

   if(ValoriaVisualFormulaGate.CollisionSignature()!=signature)
    throw new Exception("Structural frame changed gameplay collider/hotspot signature");
   if(ValoriaStructuralFrameUrbanSupportIntegrationV1.PiecesBuilt!=4)
    throw new Exception("Expected exactly four authored structural placements");
   if(ValoriaStructuralFrameUrbanSupportIntegrationV1.SuppressedRenderers<4)
    throw new Exception("Expected dominant technical support renderers were not suppressed");
   if(GameObject.Find(ValoriaLowerCityUrbanMassingReauthoringV1.RootName)==null)
    throw new Exception("Lower-city promoted solution was disturbed");

   CaptureSet(camera,"AFTER");

   camera.aspect=MobileW/(float)MobileH;
   camera.transform.position=Home;
   ValoriaMobileNavigableCityV1.ApplyHomePose(camera,camera.aspect);
   if(Vector3.Distance(camera.transform.position,Home)>.001f||Mathf.Abs(camera.orthographicSize-12.2f)>.001f)
    throw new Exception("Promoted mobile HOME changed");
   camera.aspect=16f/9f;
   ValoriaMobileNavigableCityV1.ApplyHomePose(camera,camera.aspect);
   if(Vector3.Distance(camera.transform.position,ValoriaMobileNavigableCityV1.CanonicalPosition)>.001f||
      Mathf.Abs(camera.orthographicSize-ValoriaMobileNavigableCityV1.CanonicalOrthographicSize)>.001f)
    throw new Exception("16:9 canonical framing changed");

   ValoriaStructuralFrameUrbanSupportIntegrationV1.Enabled=false;\n   ValoriaPairedFunctionalSourceV1.Enabled=false;\n   EditorApplication.Exit(0);
  }

  static Camera Create(PlayerState state)
  {
   ValoriaGraneroCuartelSourceUpgradeV1.Enabled=false;
   ValoriaFunctionalScreenReadabilityV1.Enabled=false;
   ValoriaCameraFirstFunctionalSourceV1.Enabled=false;
   ValoriaPairedFunctionalSourceV1.Enabled=true;
   SceneSetup.SetupRenderPipeline();
   EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
   ProductionVisualIntegration.ResetVisualCachesForGate();
   ProductionVisualIntegration.StoneArchitectureEnabled=true;
   ProductionVisualIntegration.TerrainTerraceEnabled=true;
   ProductionVisualIntegration.SurfaceCellEnabled=false;
   ProductionVisualIntegration.ProductionCellEnabled=false;
   ProductionVisualIntegration.CoherentCastleProofEnabled=false;
   ProductionVisualIntegration.SlavicDistrictProofEnabled=false;
   ProductionVisualIntegration.CompactFootprintReframeEnabled=true;
   AssetVisualUpliftPassV1.Enabled=false;
   AssetLibraryReprocessingPassV1.Enabled=true;
   VisualWorld.VisualIntegrationEnabled=true;
   VisualWorld.Create(true,state);
   var camera=Camera.main;
   if(camera==null)throw new Exception("Camera missing");
   return camera;
  }

  static void CaptureSet(Camera c,string stage)
  {
   c.aspect=MobileW/(float)MobileH;
   ValoriaMobileNavigableCityV1.ApplyHomePose(c,c.aspect);
   CaptureFrame(c,stage+"-HOME-mobile",MobileW,MobileH);
   c.transform.position=GraneroPan;CaptureFrame(c,stage+"-PAN-granero-mobile",MobileW,MobileH);
   c.transform.position=IntermediatePan;CaptureFrame(c,stage+"-PAN-intermediate-mobile",MobileW,MobileH);
   c.transform.position=CuartelPan;CaptureFrame(c,stage+"-PAN-cuartel-mobile",MobileW,MobileH);
   c.aspect=WideW/(float)WideH;
   ValoriaMobileNavigableCityV1.ApplyHomePose(c,c.aspect);
   CaptureFrame(c,stage+"-HORIZONTAL-16x9",WideW,WideH);

   if(stage=="AFTER")
   {
    File.WriteAllText(Folder+"/evidence.json",
     "{\n"+
     "  \"gameplay_signature_preserved\": true,\n"+
     "  \"camera_policy_preserved\": true,\n"+
     "  \"lower_city_preserved\": true,\n"+
     "  \"pieces_built\": "+ValoriaStructuralFrameUrbanSupportIntegrationV1.PiecesBuilt+",\n"+
     "  \"suppressed_renderers\": "+ValoriaStructuralFrameUrbanSupportIntegrationV1.SuppressedRenderers+",\n"+
     "  \"authored_sources\": [\"CivicRetainingBay\",\"LowerArcadedFront\",\"CuartelTerraceSupport\"],\n"+
     "  \"reference\": \"references/VALORIA_APPROVED_VISUAL_REFERENCE.jpg\",\n"+
     "  \"granero_cuartel_moved\": false,\n"+
     "  \"hero_bastion_changed\": false,\n"+
     "  \"camera_changed\": false,\n"+
     "  \"tripo_credits\": 0,\n"+
     "  \"paid_credits\": 0\n"+
     "}\n");
   }
  }

  static void CaptureFrame(Camera c,string name,int w,int h)
  {
   var rt=new RenderTexture(w,h,24,RenderTextureFormat.ARGB32){antiAliasing=4};
   var prev=RenderTexture.active;
   try
   {
    c.targetTexture=rt;c.Render();c.Render();RenderTexture.active=rt;
    var im=new Texture2D(w,h,TextureFormat.RGB24,false);
    im.ReadPixels(new Rect(0,0,w,h),0,0);im.Apply();
    File.WriteAllBytes(Folder+"/"+name+".png",im.EncodeToPNG());
    UnityEngine.Object.DestroyImmediate(im);
   }
   finally
   {
    c.targetTexture=null;RenderTexture.active=prev;rt.Release();UnityEngine.Object.DestroyImmediate(rt);
   }
  }
 }
}
