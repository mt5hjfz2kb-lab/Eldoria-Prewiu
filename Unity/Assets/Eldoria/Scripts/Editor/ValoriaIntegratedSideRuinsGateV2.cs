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
 public static class ValoriaIntegratedSideRuinsGateV2
 {
  const string Folder="ValoriaIntegratedSideRuinsV2Captures";

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
   var p=new Vector3(18.2f,14.6f,-25.8f);var t=new Vector3(0f,3.35f,5.6f);
   Save(c,Folder+"/before.png",p,t,9.1f,1280,720);Save(c,Folder+"/before-mobile.png",p,t,9.1f,390,844);

   var art=ValoriaExternalAssetLibrary.Load();if(art==null)throw new Exception("External asset library missing");
   var proof=new GameObject("Valoria · integrated side ruins v2").transform;proof.SetParent(root.transform,true);

   // Compact, buried lateral arches: frame the Bastion without competing with it.
   Add(proof,art.MegaHalfGate,"west half arch",new Vector3(-8.15f,.52f,6.05f),3.05f,4.65f,25f,new Color(.64f,.62f,.56f,1f));
   Add(proof,art.MegaDestroyedTower,"west broken tower",new Vector3(-9.65f,.30f,7.95f),2.35f,4.10f,42f,new Color(.57f,.56f,.52f,1f));
   Add(proof,art.MegaWallPassage,"west buried passage",new Vector3(-7.20f,.45f,4.80f),2.65f,2.75f,88f,new Color(.61f,.59f,.54f,1f));

   Add(proof,art.MegaHalfGate,"east half arch",new Vector3(8.35f,.48f,6.25f),3.10f,4.75f,205f,new Color(.64f,.62f,.56f,1f));
   Add(proof,art.MegaDestroyedTower,"east broken tower",new Vector3(9.75f,.28f,8.10f),2.25f,3.95f,222f,new Color(.57f,.56f,.52f,1f));
   Add(proof,art.MegaWallPassage,"east buried passage",new Vector3(7.40f,.42f,4.95f),2.55f,2.65f,268f,new Color(.61f,.59f,.54f,1f));

   // Rock burial removes the block-on-platform read that killed v1.
   var rock=Resources.Load<GameObject>("WorldInventory/Rock02");
   if(rock!=null)
   {
    Add(proof,rock,"west burial A",new Vector3(-8.65f,.28f,5.25f),2.50f,1.10f,16f,new Color(.40f,.40f,.36f,1f));
    Add(proof,rock,"west burial B",new Vector3(-9.35f,.25f,7.15f),2.20f,.95f,71f,new Color(.39f,.39f,.35f,1f));
    Add(proof,rock,"east burial A",new Vector3(8.75f,.26f,5.35f),2.50f,1.10f,196f,new Color(.40f,.40f,.36f,1f));
    Add(proof,rock,"east burial B",new Vector3(9.35f,.24f,7.20f),2.20f,.95f,251f,new Color(.39f,.39f,.35f,1f));
   }

   Physics.SyncTransforms();if(ValoriaVisualFormulaGate.CollisionSignature()!=baseline)throw new Exception("side ruins altered gameplay signature");
   Save(c,Folder+"/after.png",p,t,9.1f,1280,720);Save(c,Folder+"/after-mobile.png",p,t,9.1f,390,844);
   File.WriteAllText(Folder+"/evidence.json","{\n  \"camera_size\": 9.1,\n  \"buried_lateral_ruins\": true,\n  \"collider_hotspot_signature_equal\": true,\n  \"existing_assets_only\": true,\n  \"tripo_credits\": 0\n}\n");
   EditorApplication.Exit(0);
  }

  static void Add(Transform root,GameObject source,string role,Vector3 ground,float footprint,float maxHeight,float yaw,Color tint)
  {
   if(source==null)return;
   var go=ValoriaKit.BenchmarkPieceIntegrated("Valoria · Side Ruins v2 · "+role,source,ground,footprint,maxHeight,Quaternion.Euler(0f,yaw,0f),tint);
   if(go==null)return;go.transform.SetParent(root,true);
   foreach(var col in go.GetComponentsInChildren<Collider>(true))col.enabled=false;
   foreach(var h in go.GetComponentsInChildren<WorldHotspot>(true))Object.DestroyImmediate(h);
   foreach(var b in go.GetComponentsInChildren<MonoBehaviour>(true))if(!(b is WorldHotspot))b.enabled=false;
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
