using System;
using System.IO;
using System.Collections.Generic;
using System.Reflection;
using Eldoria.Application;
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
   var hudState=new PlayerState{
    BastionLevel=2,SawmillLevel=1,BarracksLevel=1,CorruptionDiscovered=true,
    Resources=new ResourceWallet{Wood=456,Stone=388,Food=298},MarchConfigured=true
   };
   hudState.Available.ArcherT1=48;hudState.PreparedTroops=hudState.Available.Copy();hudState.PreparedHeroId="aldric";
   hudState.ChapterProgress.TrainedArchers=20;hudState.ChapterProgress.MarchConfirmed=true;hudState.ChapterProgress.ConfirmedExpeditionPower=2600;
   var p=new Vector3(18.2f,18.4f,-26.8f);var t=new Vector3(0f,1.55f,1.55f);

   var b=Create(state);var sig=ValoriaVisualFormulaGate.CollisionSignature();
   ApplyAccepted(b.root.transform,state);RemoveAddedGameplay(b.cols,b.hotspots);Physics.SyncTransforms();
   if(ValoriaVisualFormulaGate.CollisionSignature()!=sig)throw new Exception("BEFORE changed gameplay signature");
   AttachCanonicalHud(b.camera,hudState);
   Save(b.camera,Folder+"/before-9.png",p,t,9f,1280,720);
   Save(b.camera,Folder+"/before-mobile.png",p,new Vector3(0f,1.7f,1.8f),9.4f,390,844);

   var a=Create(state);sig=ValoriaVisualFormulaGate.CollisionSignature();
   ApplyAccepted(a.root.transform,state);
   ValoriaSourceLevelArtDirectionV1.Apply(a.root.transform,state);
   RemoveAddedGameplay(a.cols,a.hotspots);Physics.SyncTransforms();
   if(ValoriaVisualFormulaGate.CollisionSignature()!=sig)throw new Exception("Candidate altered gameplay signature");
   AttachCanonicalHud(a.camera,hudState);
   Save(a.camera,Folder+"/after-9.png",p,t,9f,1280,720);
   Save(a.camera,Folder+"/after-mobile.png",p,new Vector3(0f,1.7f,1.8f),9.4f,390,844);
   File.WriteAllText(Folder+"/evidence.json","{\n  \"phase\": 1,\n  \"views\": [\"9\",\"mobile\"],\n  \"real_hud\": true,\n  \"modules\": "+ValoriaSourceLevelArtDirectionV1.Modules+",\n  \"gameplay_signature_preserved\": true,\n  \"tripo_credits\": 0\n}\n");
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

  sealed class StaticGateway:ICommandGateway
  {
   readonly PlayerState state;
   public StaticGateway(PlayerState s){state=s;}
   public PlayerState Snapshot()=>state;
   public CommandResult Execute(GameCommand command)=>new CommandResult(false,"capture-only",state.Revision);
   public bool Advance()=>false;
  }

  static void AttachCanonicalHud(Camera camera,PlayerState state)
  {
   var go=new GameObject("Valoria · canonical HUD capture");
   var presenter=go.AddComponent<SlicePresenter>();presenter.Initialize(new StaticGateway(state));
   var type=typeof(SlicePresenter);const BindingFlags flags=BindingFlags.Instance|BindingFlags.NonPublic;
   SetPrivate(type,presenter,"city",true);SetPrivate(type,presenter,"renderedSawmill",state.SawmillLevel);
   SetPrivate(type,presenter,"renderedBarracks",state.BarracksLevel);SetPrivate(type,presenter,"renderedBastion",state.BastionLevel);
   SetPrivate(type,presenter,"renderedScout",state.ScoutDefeated);SetPrivate(type,presenter,"renderedEngendro",state.EngendroDefeated);
   SetPrivate(type,presenter,"renderedIdle",state.March.Phase=="idle");
   ValoriaHudPresentationV1.Enabled=true;type.GetMethod("CreateHud",flags)?.Invoke(presenter,null);type.GetMethod("Refresh",flags)?.Invoke(presenter,null);
   var safe=type.GetField("safe",flags)?.GetValue(presenter) as RectTransform;
   if(safe!=null){safe.anchorMin=Vector2.zero;safe.anchorMax=Vector2.one;safe.offsetMin=Vector2.zero;safe.offsetMax=Vector2.zero;}
   foreach(var canvas in Object.FindObjectsByType<Canvas>(FindObjectsSortMode.None))
   {if(canvas==null)continue;canvas.renderMode=RenderMode.ScreenSpaceCamera;canvas.worldCamera=camera;canvas.planeDistance=.5f;}
   Canvas.ForceUpdateCanvases();
  }

  static void SetPrivate(Type type,object target,string field,object value)
  {
   var f=type.GetField(field,BindingFlags.Instance|BindingFlags.NonPublic);
   if(f==null)throw new MissingFieldException(type.FullName,field);f.SetValue(target,value);
  }

  static void Save(Camera c,string path,Vector3 p,Vector3 t,float size,int w,int h)
  {
   c.transform.position=p;c.transform.LookAt(t);c.orthographic=true;c.orthographicSize=size;
   c.clearFlags=CameraClearFlags.SolidColor;c.backgroundColor=new Color(.33f,.39f,.42f,1);
   var rt=new RenderTexture(w,h,24,RenderTextureFormat.ARGB32){antiAliasing=4};var prev=RenderTexture.active;
   try
   {
    c.targetTexture=rt;
    foreach(var r in Object.FindObjectsByType<Renderer>(FindObjectsSortMode.None))
     if(r.enabled)foreach(var m in r.sharedMaterials)if(m!=null)for(int pass=0;pass<m.passCount;pass++)ShaderUtil.CompilePass(m,pass,true);
    foreach(var skin in Object.FindObjectsByType<ValoriaHudPresentationV1>(FindObjectsSortMode.None))skin.Apply(w,h);
    Canvas.ForceUpdateCanvases();c.Render();c.Render();Canvas.ForceUpdateCanvases();RenderTexture.active=rt;
    var im=new Texture2D(w,h,TextureFormat.RGB24,false);im.ReadPixels(new Rect(0,0,w,h),0,0);im.Apply();
    File.WriteAllBytes(path,im.EncodeToPNG());Object.DestroyImmediate(im);
   }
   finally{c.targetTexture=null;RenderTexture.active=prev;rt.Release();Object.DestroyImmediate(rt);}
  }
 }
}
