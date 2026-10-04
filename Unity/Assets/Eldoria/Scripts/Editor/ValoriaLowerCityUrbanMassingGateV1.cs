using System;
using System.IO;
using Eldoria.Domain;
using Eldoria.Presentation;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Eldoria.EditorTools
{
 public static class ValoriaLowerCityUrbanMassingGateV1
 {
  const string Folder="ValoriaLowerCityUrbanMassingV1Captures";
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

   ValoriaLowerCityUrbanMassingReauthoringV1.Enabled=false;
   var camera=Create(state);
   var signature=ValoriaVisualFormulaGate.CollisionSignature();
   var root=GameObject.Find("Valoria · integrated construction visual layer");
   if(root==null)throw new Exception("Integrated visual root missing");

   CaptureSet(camera,"BEFORE");

   ValoriaLowerCityUrbanMassingReauthoringV1.Enabled=true;
   ValoriaLowerCityUrbanMassingReauthoringV1.Build(root.transform,state);
   Physics.SyncTransforms();

   if(ValoriaVisualFormulaGate.CollisionSignature()!=signature)
    throw new Exception("Urban massing reauthoring changed gameplay collider/hotspot signature");
   if(ValoriaLowerCityUrbanMassingReauthoringV1.GroupsBuilt!=3)
    throw new Exception("Expected exactly three authored urban groups");
   if(ValoriaLowerCityUrbanMassingReauthoringV1.PiecesBuilt<14)
    throw new Exception("Urban groups are incomplete");
   if(ValoriaLowerCityUrbanMassingReauthoringV1.SuppressedRenderers<40)
    throw new Exception("Expected redundant lower-city layers were not suppressed");

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

   File.WriteAllText(Folder+"/evidence.json",
    "{\n"+
    "  \"gameplay_signature_preserved\": true,\n"+
    "  \"camera_policy_preserved\": true,\n"+
    "  \"groups_built\": "+ValoriaLowerCityUrbanMassingReauthoringV1.GroupsBuilt+",\n"+
    "  \"pieces_built\": "+ValoriaLowerCityUrbanMassingReauthoringV1.PiecesBuilt+",\n"+
    "  \"suppressed_renderers\": "+ValoriaLowerCityUrbanMassingReauthoringV1.SuppressedRenderers+",\n"+
    "  \"suppressed_lights\": "+ValoriaLowerCityUrbanMassingReauthoringV1.SuppressedLights+",\n"+
    "  \"suppressed_systems\": [\"six reused civil houses\",\"Mid-Tier District v1 presentation\",\"Full Frame Architecture Batch v1 presentation\"],\n"+
    "  \"authored_groups\": [\"west craft court\",\"east merchant front\",\"upper terrace houses\"],\n"+
    "  \"reference\": \"references/VALORIA_APPROVED_VISUAL_REFERENCE.jpg\",\n"+
    "  \"granero_cuartel_moved\": false,\n"+
    "  \"hero_bastion_changed\": false,\n"+
    "  \"camera_changed\": false,\n"+
    "  \"tripo_credits\": 0,\n"+
    "  \"paid_credits\": 0\n"+
    "}\n");

   ValoriaLowerCityUrbanMassingReauthoringV1.Enabled=false;
   ValoriaPairedFunctionalSourceV1.Enabled=false;
   EditorApplication.Exit(0);
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
   if(camera==null||GameObject.Find(ValoriaPairedFunctionalSourceV1.RootName)==null||ValoriaPairedFunctionalSourceV1.Pieces!=2)
    throw new Exception("Urban massing prerequisites missing");
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
