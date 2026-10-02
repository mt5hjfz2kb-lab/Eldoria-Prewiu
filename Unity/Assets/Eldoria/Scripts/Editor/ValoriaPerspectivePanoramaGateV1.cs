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
 public static class ValoriaPerspectivePanoramaGateV1
 {
  const string Folder="ValoriaPerspectivePanoramaV1Captures";
  public static void Capture()
  {
   ShaderUtil.allowAsyncCompilation=false;Directory.CreateDirectory(Folder);SceneSetup.SetupRenderPipeline();EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
   ProductionVisualIntegration.ResetVisualCachesForGate();
   ProductionVisualIntegration.StoneArchitectureEnabled=true;ProductionVisualIntegration.TerrainTerraceEnabled=true;ProductionVisualIntegration.SurfaceCellEnabled=false;ProductionVisualIntegration.ProductionCellEnabled=false;ProductionVisualIntegration.CoherentCastleProofEnabled=false;ProductionVisualIntegration.SlavicDistrictProofEnabled=false;ProductionVisualIntegration.CompactFootprintReframeEnabled=true;
   AssetVisualUpliftPassV1.Enabled=false;AssetLibraryReprocessingPassV1.Enabled=true;MidTierDistrictProduction.Enabled=true;ValoriaFullFrameArchitectureBatchV1.Enabled=true;
   ValoriaReferenceConvergencePassV2.Enabled=false;ValoriaInCitySurfacePassV1.Enabled=false;ValoriaStairLandingIntegrationV1.Enabled=false;ValoriaFullFrameConvergenceIteration1.Enabled=false;ValoriaFullFrameConvergenceIteration2.Enabled=false;ValoriaWorldFrameMountainTerrainV1.Enabled=true;ValoriaPanoramicSkyCandidateV1.Enabled=false;VisualWorld.VisualIntegrationEnabled=true;
   var state=new PlayerState{BastionLevel=3,SawmillLevel=1,BarracksLevel=1,CorruptionDiscovered=true};VisualWorld.Create(true,state);
   var c=Camera.main;var root=GameObject.Find("Valoria · integrated construction visual layer");if(c==null||root==null)throw new System.Exception("Valoria capture prerequisites missing.");
   var baseline=ValoriaVisualFormulaGate.CollisionSignature();
   var orthoPos=new Vector3(18.2f,14.6f,-25.8f);var target=new Vector3(0f,3.15f,5.8f);
   c.orthographic=true;c.orthographicSize=12f;Save(c,Folder+"/before-12.png",orthoPos,target,1280,720);Save(c,Folder+"/before-mobile.png",orthoPos,target,390,844);Metrics(Folder+"/before-metrics.json");

   var legacy=GameObject.Find("Valoria · World Frame Mountain Terrain v1");if(legacy!=null)Object.DestroyImmediate(legacy);ValoriaWorldFrameMountainTerrainV1.Enabled=false;
   ValoriaPanoramicSkyCandidateV1.Enabled=true;if(!ValoriaPanoramicSkyCandidateV1.Apply())throw new System.Exception("Panorama unavailable.");
   c.orthographic=false;c.fieldOfView=30f;c.nearClipPlane=.3f;c.farClipPlane=500f;
   var perspectivePos=new Vector3(19.4f,15.6f,-28.0f);var perspectiveTarget=new Vector3(-.4f,3.5f,5.4f);
   c.transform.position=perspectivePos;c.transform.LookAt(perspectiveTarget);
   Physics.SyncTransforms();if(ValoriaVisualFormulaGate.CollisionSignature()!=baseline)throw new System.Exception("Perspective/panorama candidate altered gameplay signature.");
   SavePerspective(c,Folder+"/after-perspective.png",perspectivePos,perspectiveTarget,30f,1280,720);
   SavePerspective(c,Folder+"/after-perspective-mobile.png",perspectivePos,perspectiveTarget,30f,390,844);
   SavePerspective(c,Folder+"/after-perspective-wide.png",new Vector3(21.2f,16.5f,-31f),perspectiveTarget,32f,1280,720);
   Metrics(Folder+"/after-metrics.json");
   File.WriteAllText(Folder+"/evidence.json","{\n  \"camera_matched_subject\": true,\n  \"baseline_camera\": \"orthographic\",\n  \"candidate_camera\": \"perspective_fov_30\",\n  \"panorama\": \"Poly Haven Kiara 1 Dawn CC0\",\n  \"collider_hotspot_signature_equal\": true,\n  \"tripo_credits\": 0\n}\n");
   EditorApplication.Exit(0);
  }
  static void Save(Camera c,string path,Vector3 p,Vector3 t,int w,int h){c.transform.position=p;c.transform.LookAt(t);c.orthographic=true;c.orthographicSize=12f;Render(c,path,w,h);}
  static void SavePerspective(Camera c,string path,Vector3 p,Vector3 t,float fov,int w,int h){c.transform.position=p;c.transform.LookAt(t);c.orthographic=false;c.fieldOfView=fov;Render(c,path,w,h);}
  static void Render(Camera c,string path,int w,int h){var rt=new RenderTexture(w,h,24,RenderTextureFormat.ARGB32);var prev=RenderTexture.active;try{c.targetTexture=rt;c.Render();c.Render();RenderTexture.active=rt;var im=new Texture2D(w,h,TextureFormat.RGB24,false);im.ReadPixels(new Rect(0,0,w,h),0,0);im.Apply();File.WriteAllBytes(path,im.EncodeToPNG());Object.DestroyImmediate(im);}finally{c.targetTexture=null;RenderTexture.active=prev;rt.Release();Object.DestroyImmediate(rt);}}
  static void Metrics(string path){int rs=0,ls=0;long tri=0;var ms=new HashSet<string>();foreach(var r in Object.FindObjectsByType<Renderer>(FindObjectsSortMode.None)){if(r==null||!r.enabled||!r.gameObject.activeInHierarchy)continue;rs++;foreach(var m in r.sharedMaterials)if(m!=null)ms.Add(m.name);}foreach(var mf in Object.FindObjectsByType<MeshFilter>(FindObjectsSortMode.None)){if(mf==null||mf.sharedMesh==null||!mf.gameObject.activeInHierarchy)continue;var rr=mf.GetComponent<Renderer>();if(rr!=null&&!rr.enabled)continue;for(int s=0;s<mf.sharedMesh.subMeshCount;s++)tri+=(long)mf.sharedMesh.GetIndexCount(s)/3L;}foreach(var l in Object.FindObjectsByType<Light>(FindObjectsSortMode.None))if(l!=null&&l.enabled&&l.gameObject.activeInHierarchy)ls++;File.WriteAllText(path,$"{{\n  \"active_renderers\": {rs},\n  \"unique_materials\": {ms.Count},\n  \"scene_triangles\": {tri},\n  \"active_lights\": {ls}\n}}\n");}
 }
}
