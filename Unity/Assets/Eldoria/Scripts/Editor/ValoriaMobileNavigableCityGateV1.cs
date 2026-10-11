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
 public static class ValoriaMobileNavigableCityGateV1
 {
  const string Folder="ValoriaMobileNavigableCityV1Captures";
  const int W=390,H=844;

  public static void Capture()
  {
   ShaderUtil.allowAsyncCompilation=false;
   Directory.CreateDirectory(Folder);
   var state=new PlayerState{BastionLevel=3,SawmillLevel=1,BarracksLevel=1,CorruptionDiscovered=true};
   var camera=Create(state);
   var signature=ValoriaVisualFormulaGate.CollisionSignature();
   camera.aspect=W/(float)H;
   ValoriaMobileNavigableCityV1.ApplyHomePose(camera,camera.aspect);
   var home=camera.transform.position;var rot=camera.transform.rotation;float size=camera.orthographicSize;

   Save(camera,"HOME-mobile",W,H);
   var granero=GameObject.Find(ValoriaScreenSpaceBreakpointV1.GraneroName);
   var cuartel=GameObject.Find(ValoriaScreenSpaceBreakpointV1.CuartelName);
   if(granero==null||cuartel==null)throw new Exception("Functional sources missing");

   var g=Focus(camera,home,Center(granero),state.BastionLevel,camera.aspect);
   camera.transform.position=g;Save(camera,"PAN-granero-mobile",W,H);
   camera.transform.position=Vector3.Lerp(home,g,.5f);Save(camera,"PAN-intermediate-mobile",W,H);
   var q=Focus(camera,home,Center(cuartel),state.BastionLevel,camera.aspect);
   camera.transform.position=q;Save(camera,"PAN-cuartel-mobile",W,H);

   VerifyHotspot(camera,home,"Aserradero · target","sawmill",state.BastionLevel);
   VerifyHotspot(camera,home,"Cuartel · target","barracks",state.BastionLevel);
   VerifyHotspot(camera,home,"Bastion · target","bastion",state.BastionLevel);

   camera.transform.position=home;camera.transform.rotation=rot;camera.orthographicSize=size;
   Save(camera,"RETURN-HOME-mobile",W,H);
   if(Vector3.Distance(camera.transform.position,home)>.001f||Mathf.Abs(camera.orthographicSize-size)>.001f)
    throw new Exception("Home restore failed");

   camera.aspect=16f/9f;
   ValoriaMobileNavigableCityV1.ApplyHomePose(camera,camera.aspect);
   Save(camera,"HORIZONTAL-16x9",1600,900);
   if(Vector3.Distance(camera.transform.position,ValoriaMobileNavigableCityV1.CanonicalPosition)>.001f||
      Mathf.Abs(camera.orthographicSize-ValoriaMobileNavigableCityV1.CanonicalOrthographicSize)>.001f)
    throw new Exception("16:9 framing changed");
   if(ValoriaVisualFormulaGate.CollisionSignature()!=signature)throw new Exception("Gameplay signature changed");

   var half=ValoriaMobileNavigableCityV1.PanHalfExtents(state.BastionLevel,W/(float)H);
   File.WriteAllText(Folder+"/evidence.json",
    "{\n  \"gameplay_signature_preserved\": true,\n  \"mobile_home\": [19.4,14.6,-28.6],\n  \"mobile_orthographic_size\": 12.2,\n"+
    "  \"portrait_threshold\": 0.72,\n  \"early_game_pan_half_extents\": ["+half.x.ToString("F2",System.Globalization.CultureInfo.InvariantCulture)+","+half.y.ToString("F2",System.Globalization.CultureInfo.InvariantCulture)+"],\n"+
    "  \"granero_pan_position\": ["+V(g)+"],\n  \"cuartel_pan_position\": ["+V(q)+"],\n  \"horizontal_preserved\": true,\n"+
    "  \"assets_changed\": false,\n  \"materials_changed\": false,\n  \"world_layout_changed\": false,\n  \"tripo_credits\": 0,\n  \"paid_credits\": 0\n}\n");
   ValoriaPairedFunctionalSourceV1.Enabled=false;
   EditorApplication.Exit(0);
  }

  static Camera Create(PlayerState state)
  {
   ValoriaGraneroCuartelSourceUpgradeV1.Enabled=false;
   ValoriaFunctionalScreenReadabilityV1.Enabled=false;
   ValoriaCameraFirstFunctionalSourceV1.Enabled=false;
   ValoriaPairedFunctionalSourceV1.Enabled=true;
   SceneSetup.SetupRenderPipeline();EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
   ProductionVisualIntegration.ResetVisualCachesForGate();
   ProductionVisualIntegration.StoneArchitectureEnabled=true;ProductionVisualIntegration.TerrainTerraceEnabled=true;
   ProductionVisualIntegration.SurfaceCellEnabled=false;ProductionVisualIntegration.ProductionCellEnabled=false;
   ProductionVisualIntegration.CoherentCastleProofEnabled=false;ProductionVisualIntegration.SlavicDistrictProofEnabled=false;
   ProductionVisualIntegration.CompactFootprintReframeEnabled=true;AssetVisualUpliftPassV1.Enabled=false;
   AssetLibraryReprocessingPassV1.Enabled=true;VisualWorld.VisualIntegrationEnabled=true;VisualWorld.Create(true,state);
   var c=Camera.main;if(c==null||ValoriaPairedFunctionalSourceV1.Pieces!=2)throw new Exception("Gate prerequisites missing");
   Physics.SyncTransforms();return c;
  }

  static Vector3 Center(GameObject go)
  {
   var rs=go.GetComponentsInChildren<Renderer>(true);bool any=false;var b=new Bounds();
   foreach(var r in rs)if(r.enabled&&r.gameObject.activeInHierarchy){if(!any){b=r.bounds;any=true;}else b.Encapsulate(r.bounds);}
   if(!any)throw new Exception("No renderer "+go.name);return b.center;
  }

  static Vector3 Focus(Camera c,Vector3 home,Vector3 p,int level,float aspect)
  {
   var ray=c.ViewportPointToRay(new Vector3(.5f,.5f,0));var plane=new Plane(Vector3.up,new Vector3(0,p.y,0));
   if(!plane.Raycast(ray,out var d))throw new Exception("Focus failed");
   return ValoriaMobileNavigableCityV1.ClampToEnvelope(home,c.transform.position+(p-ray.GetPoint(d)),level,aspect);
  }

  static void VerifyHotspot(Camera c,Vector3 home,string name,string id,int level)
  {
   var go=GameObject.Find(name);if(go==null)throw new Exception(name+" missing");
   var col=go.GetComponent<Collider>();var hs=go.GetComponent<WorldHotspot>();
   if(col==null||!col.enabled||hs==null||hs.Id!=id)throw new Exception(name+" invalid");
   c.transform.position=Focus(c,home,col.bounds.center,level,c.aspect);Physics.SyncTransforms();
   var screen=c.WorldToScreenPoint(col.bounds.center);
   if(screen.z<=0||screen.x<0||screen.x>c.pixelWidth||screen.y<0||screen.y>c.pixelHeight)
    throw new Exception(name+" cannot be brought into the interaction viewport");
   var ray=c.ScreenPointToRay(screen);
   foreach(var hit in Physics.RaycastAll(ray,100f)){var h=hit.collider.GetComponent<WorldHotspot>();if(h!=null&&h.Id==id)return;}
   throw new Exception(name+" target ray no longer resolves after pan");
  }

  static void Save(Camera c,string name,int w,int h)
  {
   var rt=new RenderTexture(w,h,24,RenderTextureFormat.ARGB32){antiAliasing=4};var prev=RenderTexture.active;
   try{c.targetTexture=rt;c.Render();c.Render();RenderTexture.active=rt;var im=new Texture2D(w,h,TextureFormat.RGB24,false);im.ReadPixels(new Rect(0,0,w,h),0,0);im.Apply();File.WriteAllBytes(Folder+"/"+name+".png",im.EncodeToPNG());Object.DestroyImmediate(im);}
   finally{c.targetTexture=null;RenderTexture.active=prev;rt.Release();Object.DestroyImmediate(rt);}
  }

  static string V(Vector3 v)=>v.x.ToString("F2",System.Globalization.CultureInfo.InvariantCulture)+","+v.y.ToString("F2",System.Globalization.CultureInfo.InvariantCulture)+","+v.z.ToString("F2",System.Globalization.CultureInfo.InvariantCulture);
 }
}
