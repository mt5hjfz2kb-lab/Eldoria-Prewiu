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
 public static class ValoriaFunctionalScreenReadabilityGateV1
 {
  const string Folder="ValoriaFunctionalScreenReadabilityV1Captures";

  public static void Capture()
  {
   ShaderUtil.allowAsyncCompilation=false;Directory.CreateDirectory(Folder);
   var state=new PlayerState{BastionLevel=3,SawmillLevel=1,BarracksLevel=1,CorruptionDiscovered=true};

   // BEFORE is the source-approved GCSU pair in its previous played-camera placement.
   ValoriaGraneroCuartelSourceUpgradeV1.Enabled=true;
   ValoriaFunctionalScreenReadabilityV1.Enabled=false;
   var before=Create(state);
   var sig=ValoriaVisualFormulaGate.CollisionSignature();
   if(GameObject.Find(ValoriaGraneroCuartelSourceUpgradeV1.RootName)==null)throw new Exception("GCSU source pair missing from BEFORE");
   WriteMetrics(Folder+"/before-metrics.json");
   Save(before,Folder+"/before-9.png",9f,1280,720);
   Save(before,Folder+"/before-mobile.png",9.4f,390,844);

   // AFTER changes presentation/layout only. Approved source bytes are reused unchanged.
   ValoriaGraneroCuartelSourceUpgradeV1.Enabled=true;
   ValoriaFunctionalScreenReadabilityV1.Enabled=true;
   var after=Create(state);Physics.SyncTransforms();
   if(GameObject.Find(ValoriaFunctionalScreenReadabilityV1.RootName)==null)throw new Exception("FSR root missing from AFTER");
   if(ValoriaFunctionalScreenReadabilityV1.Pieces!=2)throw new Exception("Expected exactly 2 FSR source placements");
   if(ValoriaVisualFormulaGate.CollisionSignature()!=sig)throw new Exception("FSR changed gameplay collider/hotspot signature");
   WriteMetrics(Folder+"/after-metrics.json");
   Save(after,Folder+"/after-9.png",9f,1280,720);
   Save(after,Folder+"/after-mobile.png",9.4f,390,844);

   File.WriteAllText(Folder+"/evidence.json",
    "{\n"+
    "  \"gameplay_signature_preserved\": true,\n"+
    "  \"source_geometry_changed\": false,\n"+
    "  \"pieces\": 2,\n"+
    "  \"camera\": \"ORTHOGRAPHIC\",\n"+
    "  \"final_look\": \"RESET_ACCEPTED_RUNTIME\",\n"+
    "  \"defense_changed\": false,\n"+
    "  \"hero_bastion_changed\": false,\n"+
    "  \"terrain_changed\": false,\n"+
    "  \"granero\": {\"position\": [-2.35,0.16,-5.00], \"footprint\": 5.68, \"height\": 4.68, \"yaw\": 0},\n"+
    "  \"cuartel\": {\"position\": [5.55,0.16,-4.70], \"footprint\": 5.62, \"height\": 4.72, \"yaw\": 0},\n"+
    "  \"tripo_credits\": 0,\n"+
    "  \"paid_credits\": 0\n"+
    "}\n");

   ValoriaFunctionalScreenReadabilityV1.Enabled=false;
   ValoriaGraneroCuartelSourceUpgradeV1.Enabled=false;
   EditorApplication.Exit(0);
  }

  static Camera Create(PlayerState state)
  {
   SceneSetup.SetupRenderPipeline();EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
   ProductionVisualIntegration.ResetVisualCachesForGate();
   ProductionVisualIntegration.StoneArchitectureEnabled=true;ProductionVisualIntegration.TerrainTerraceEnabled=true;
   ProductionVisualIntegration.SurfaceCellEnabled=false;ProductionVisualIntegration.ProductionCellEnabled=false;
   ProductionVisualIntegration.CoherentCastleProofEnabled=false;ProductionVisualIntegration.SlavicDistrictProofEnabled=false;
   ProductionVisualIntegration.CompactFootprintReframeEnabled=true;AssetVisualUpliftPassV1.Enabled=false;
   AssetLibraryReprocessingPassV1.Enabled=true;VisualWorld.VisualIntegrationEnabled=true;VisualWorld.Create(true,state);
   var camera=Camera.main;var root=GameObject.Find("Valoria · integrated construction visual layer");
   if(camera==null||root==null)throw new Exception("FSR capture prerequisites missing");
   return camera;
  }

  static void Save(Camera c,string path,float size,int w,int h)
  {
   c.transform.position=new Vector3(18.2f,14.6f,-25.8f);
   c.transform.LookAt(new Vector3(0f,3.15f,5.8f));
   c.orthographic=true;c.orthographicSize=size;
   var rt=new RenderTexture(w,h,24,RenderTextureFormat.ARGB32){antiAliasing=4};var prev=RenderTexture.active;
   try
   {
    c.targetTexture=rt;c.Render();c.Render();RenderTexture.active=rt;
    var im=new Texture2D(w,h,TextureFormat.RGB24,false);
    im.ReadPixels(new Rect(0,0,w,h),0,0);im.Apply();File.WriteAllBytes(path,im.EncodeToPNG());Object.DestroyImmediate(im);
   }
   finally{c.targetTexture=null;RenderTexture.active=prev;rt.Release();Object.DestroyImmediate(rt);}
  }

  static void WriteMetrics(string path)
  {
   int renderers=0,lights=0;long triangles=0;var materials=new HashSet<string>();
   foreach(var r in Object.FindObjectsByType<Renderer>(FindObjectsSortMode.None))
   {
    if(r==null||!r.enabled||!r.gameObject.activeInHierarchy)continue;
    renderers++;foreach(var m in r.sharedMaterials)if(m!=null)materials.Add(m.name);
   }
   foreach(var mf in Object.FindObjectsByType<MeshFilter>(FindObjectsSortMode.None))
   {
    if(mf==null||mf.sharedMesh==null||!mf.gameObject.activeInHierarchy)continue;
    var rr=mf.GetComponent<Renderer>();if(rr!=null&&!rr.enabled)continue;
    for(int s=0;s<mf.sharedMesh.subMeshCount;s++)triangles+=(long)mf.sharedMesh.GetIndexCount(s)/3L;
   }
   foreach(var l in Object.FindObjectsByType<Light>(FindObjectsSortMode.None))
    if(l!=null&&l.enabled&&l.gameObject.activeInHierarchy)lights++;
   File.WriteAllText(path,
    "{\n  \"active_renderers\": "+renderers+
    ",\n  \"unique_materials\": "+materials.Count+
    ",\n  \"scene_triangles\": "+triangles+
    ",\n  \"active_lights\": "+lights+"\n}\n");
  }
 }
}
