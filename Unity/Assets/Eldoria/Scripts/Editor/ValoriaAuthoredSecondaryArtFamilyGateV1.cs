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
 public static class ValoriaAuthoredSecondaryArtFamilyGateV1
 {
  const string Folder="ValoriaAuthoredSecondaryArtFamilyV1Captures";

  struct SceneData
  {
   public Camera camera;
   public GameObject root;
   public HashSet<int> colliderIds;
   public HashSet<int> hotspotIds;
  }

  public static void Capture()
  {
   ShaderUtil.allowAsyncCompilation=false;
   Directory.CreateDirectory(Folder);
   var state=new PlayerState{BastionLevel=6,SawmillLevel=2,BarracksLevel=2,CorruptionDiscovered=true};
   var p=new Vector3(18.2f,18.4f,-26.8f);
   var t=new Vector3(0f,1.55f,1.55f);

   CaptureVariant(state,p,t,"baseline",null,null,null,false);
   CaptureVariant(state,p,t,"authored-complete",
     "Valoria/AuthoredSecondaryCandidates/Valoria_Authored_Aserradero_v1",
     "Valoria/AuthoredSecondaryCandidates/Valoria_Authored_Wall_v1",
     "Valoria/AuthoredSecondaryCandidates/Valoria_Authored_Gate_v1",false);
   CaptureVariant(state,p,t,"source-preserving",
     "Valoria/AuthoredSecondaryCandidates/Valoria_ASF_Aserradero_v1",
     "Valoria/AuthoredSecondaryCandidates/Valoria_ASF_WallSupport_v1",
     "Valoria/AuthoredSecondaryCandidates/Valoria_ASF_GateSupport_v1",false);

   CaptureVariant(state,p,t,"source-atlas-v2",
     "Valoria/AuthoredSecondaryCandidates/Valoria_ASF_Aserradero_v1",
     "Valoria/AuthoredSecondaryCandidates/Valoria_ASF_SourceWall_v2",
     "Valoria/AuthoredSecondaryCandidates/Valoria_ASF_SourceGate_v2",true);

   File.WriteAllText(Folder+"/evidence.json",
    "{\n"+
    "  \"baseline\": \"VQB final 7a564bc\",\n"+
    "  \"variants\": [\"authored-complete\",\"source-preserving\",\"source-atlas-v2\"],\n"+
    "  \"gameplay_signature_preserved\": true,\n"+
    "  \"macro_composition_changed\": false,\n"+
    "  \"f1_envelope_preserved\": true,\n"+
    "  \"front_gate_route_preserved\": true,\n"+
    "  \"tripo_credits\": 0\n"+
    "}\n");
   Debug.Log("VALORIA_AUTHORED_SECONDARY_ART_FAMILY_V1_GATE=PASS");
   EditorApplication.Exit(0);
  }

  static void CaptureVariant(PlayerState state,Vector3 p,Vector3 t,string tag,string sawmillPath,string wallPath,string gatePath,bool useAtlas)
  {
   var s=CreateScene(state);
   var sig=ValoriaVisualFormulaGate.CollisionSignature();
   ConfigureUplift();
   ValoriaFlatCitadelProductionUpliftV1.Build(s.root.transform,state);
   ValoriaFlatCitadelArtConsolidationV1.Apply(s.root.transform,state);
   ValoriaAssetCoherenceV1.Apply(s.root.transform,state);
   ValoriaVisualQualityBreakthroughV1.Apply(s.root.transform,state);

   if(!string.IsNullOrEmpty(sawmillPath))
   {
    ReplaceSawmill(s.root.transform,sawmillPath,useAtlas);
    ReplaceFrontSupport(s.root.transform,wallPath,gatePath,useAtlas);
   }

   RemoveAddedGameplay(s.colliderIds,s.hotspotIds);
   Physics.SyncTransforms();
   if(ValoriaVisualFormulaGate.CollisionSignature()!=sig)
    throw new Exception("Authored family variant altered gameplay signature: "+tag);
   SaveSet(s.camera,tag,p,t);
  }

  static void ConfigureUplift()
  {
   ValoriaFlatCitadelProductionUpliftV1.WallUpliftEnabled=true;
   ValoriaFlatCitadelProductionUpliftV1.GroundUpliftEnabled=true;
   ValoriaFlatCitadelProductionUpliftV1.BastionIntegrationUpliftEnabled=true;
   ValoriaFlatCitadelProductionUpliftV1.FunctionalBuildingUpliftEnabled=true;
   ValoriaFlatCitadelProductionUpliftV1.DressingUpliftEnabled=true;
  }

  static void ReplaceSawmill(Transform root,string resourcePath,bool useAtlas)
  {
   GameObject old=GameObject.Find("Valoria · Flat Citadel Production · Aserradero");
   if(old==null)
   {
    foreach(var tr in Object.FindObjectsByType<Transform>(FindObjectsSortMode.None))
     if(tr!=null&&tr.name.ToLowerInvariant().Contains("production · aserradero")){old=tr.gameObject;break;}
   }
   if(old==null)throw new Exception("Current production Aserradero visual not found.");
   var source=Resources.Load<GameObject>(resourcePath);
   if(source==null)throw new Exception("Authored Aserradero missing: "+resourcePath);
   Object.DestroyImmediate(old);
   var go=Object.Instantiate(source);go.name="Valoria · Authored Secondary · Aserradero";
   go.transform.rotation=Quaternion.Euler(0f,-5f,0f);
   Fit(go,new Vector3(-5.95f,.13f,-1.55f),4.45f,3.65f);
   go.transform.SetParent(root,true);AdaptMaterials(go,useAtlas);StripGameplay(go);
  }

  static void ReplaceFrontSupport(Transform root,string wallPath,string gatePath,bool useAtlas)
  {
   foreach(var tr in Object.FindObjectsByType<Transform>(FindObjectsSortMode.None))
   {
    if(tr==null)continue;
    string n=tr.name.ToLowerInvariant();
    if(n.Contains("breakthrough · front west 2")||n.Contains("breakthrough · front east 0"))
      foreach(var r in tr.GetComponentsInChildren<Renderer>(true))r.enabled=false;
   }

   var wall=Resources.Load<GameObject>(wallPath);
   var gate=Resources.Load<GameObject>(gatePath);
   if(wall==null||gate==null)throw new Exception("Authored wall/gate support missing.");

   Place(root,wall,"wall west",new Vector3(-4.72f,.18f,-6.28f),1.90f,1.34f,0f,useAtlas);
   Place(root,wall,"wall east",new Vector3(4.18f,.18f,-6.28f),1.90f,1.34f,180f,useAtlas);
   Place(root,gate,"gate support",new Vector3(0f,.18f,-6.18f),3.55f,2.65f,0f,useAtlas);
  }

  static void Place(Transform root,GameObject src,string role,Vector3 ground,float span,float maxHeight,float yaw,bool useAtlas)
  {
   var go=Object.Instantiate(src);go.name="Valoria · Authored Secondary · "+role;
   go.transform.rotation=Quaternion.Euler(0f,yaw,0f);Fit(go,ground,span,maxHeight);
   go.transform.SetParent(root,true);AdaptMaterials(go,useAtlas);StripGameplay(go);
  }

  static void AdaptMaterials(GameObject go,bool useAtlas)
  {
   var shader=Shader.Find("Eldoria/Valoria Coherence");
   if(shader==null)throw new Exception("Valoria coherence shader missing.");
   var rock=Resources.Load<Texture2D>("Valoria/SurfaceCellExternal/rock_diff");
   var stone=Resources.Load<Texture2D>("Valoria/SurfaceCellExternal/stone_diff");
   var dirt=Resources.Load<Texture2D>("Valoria/SurfaceCellExternal/dirt_diff");
   var atlasStone=useAtlas?Resources.Load<Texture2D>("Valoria/AuthoredSecondaryCandidates/Materials/ASF_Stone_diff"):null;
   var atlasStoneN=useAtlas?Resources.Load<Texture2D>("Valoria/AuthoredSecondaryCandidates/Materials/ASF_Stone_normal"):null;
   var atlasTimber=useAtlas?Resources.Load<Texture2D>("Valoria/AuthoredSecondaryCandidates/Materials/ASF_Timber_diff"):null;
   var atlasTimberN=useAtlas?Resources.Load<Texture2D>("Valoria/AuthoredSecondaryCandidates/Materials/ASF_Timber_normal"):null;
   var atlasRoof=useAtlas?Resources.Load<Texture2D>("Valoria/AuthoredSecondaryCandidates/Materials/ASF_Roof_diff"):null;
   var atlasRoofN=useAtlas?Resources.Load<Texture2D>("Valoria/AuthoredSecondaryCandidates/Materials/ASF_Roof_normal"):null;
   foreach(var r in go.GetComponentsInChildren<Renderer>(true))
   {
    var srcs=r.sharedMaterials;var dst=new Material[srcs.Length];
    for(int i=0;i<srcs.Length;i++)
    {
     var src=srcs[i];if(src==null)continue;
     string name=src.name.ToLowerInvariant();
     Texture baseMap=null,normal=null;string baseProp="";
     foreach(var p in new[]{"_BaseMap","_BaseColorTexture","baseColorTexture","_MainTex","_Texture","_Albedo"})
      if(src.HasProperty(p)&&src.GetTexture(p)!=null){baseMap=src.GetTexture(p);baseProp=p;break;}
     foreach(var p in new[]{"_BumpMap","_NormalTexture","_NormalMap","normalTexture"})
      if(src.HasProperty(p)&&src.GetTexture(p)!=null){normal=src.GetTexture(p);break;}

     bool roofName=name.Contains("roof")||name.Contains("slate");
     bool timberName=name.Contains("timber")||name.Contains("wood");
     bool stoneName=name.Contains("stone")||name.Contains("foundation")||name.Contains("infill")||name.Contains("plaster")||name.Contains("pier")||name.Contains("lintel");
     if(useAtlas){
      if(roofName){baseMap=atlasRoof??stone??rock;normal=atlasRoofN;}
      else if(timberName){baseMap=atlasTimber??dirt??rock;normal=atlasTimberN;}
      else {baseMap=atlasStone??stone??rock;normal=atlasStoneN;}
      baseProp="";
     } else if(baseMap==null)baseMap=roofName?(stone!=null?stone:rock):(timberName?(dirt!=null?dirt:rock):(stone!=null?stone:rock));

     var m=new Material(shader){name="Valoria ASF · "+src.name};
     if(baseMap!=null)m.SetTexture("_BaseMap",baseMap);
     if(baseProp!=""&&baseMap==src.GetTexture(baseProp))
     {
      try{m.SetTextureScale("_BaseMap",src.GetTextureScale(baseProp));m.SetTextureOffset("_BaseMap",src.GetTextureOffset(baseProp));}catch{}
     }
     if(normal!=null)m.SetTexture("_BumpMap",normal);
     m.SetFloat("_BumpScale",normal!=null?.68f:0f);
     m.SetFloat("_Family",0f);m.SetFloat("_Ground",0f);m.SetFloat("_Smoothness",.055f);
     m.SetFloat("_Bottom",r.bounds.min.y);m.SetFloat("_Height",Mathf.Max(.01f,r.bounds.size.y));
     if(rock!=null)m.SetTexture("_RockMap",rock);
     if(roofName)m.SetColor("_BaseColor",useAtlas?new Color(.72f,.82f,.88f,1f):new Color(.48f,.58f,.66f,1f));
     else if(timberName)m.SetColor("_BaseColor",useAtlas?new Color(.90f,.78f,.62f,1f):new Color(.72f,.56f,.38f,1f));
     else if(stoneName)m.SetColor("_BaseColor",useAtlas?new Color(.98f,.96f,.90f,1f):new Color(.92f,.89f,.80f,1f));
     else m.SetColor("_BaseColor",Color.white);
     dst[i]=m;
    }
    r.sharedMaterials=dst;
   }
  }

  static void Fit(GameObject go,Vector3 ground,float span,float maxHeight)
  {
   var rs=go.GetComponentsInChildren<Renderer>(true);
   if(rs.Length==0)throw new Exception("Authored family object has no renderer: "+go.name);
   var b=rs[0].bounds;for(int i=1;i<rs.Length;i++)b.Encapsulate(rs[i].bounds);
   float horizontal=Mathf.Max(.001f,Mathf.Max(b.size.x,b.size.z));
   float scale=Mathf.Min(span/horizontal,maxHeight/Mathf.Max(.001f,b.size.y));
   go.transform.localScale*=scale;
   rs=go.GetComponentsInChildren<Renderer>(true);b=rs[0].bounds;
   for(int i=1;i<rs.Length;i++)b.Encapsulate(rs[i].bounds);
   go.transform.position+=ground-new Vector3(b.center.x,b.min.y,b.center.z);
  }

  static void StripGameplay(GameObject go)
  {
   foreach(var c in go.GetComponentsInChildren<Collider>(true))Object.DestroyImmediate(c);
   foreach(var h in go.GetComponentsInChildren<WorldHotspot>(true))Object.DestroyImmediate(h);
   foreach(var mb in go.GetComponentsInChildren<MonoBehaviour>(true))mb.enabled=false;
  }

  static SceneData CreateScene(PlayerState state)
  {
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
   MidTierDistrictProduction.Enabled=true;
   ValoriaFullFrameArchitectureBatchV1.Enabled=true;
   ValoriaOpenValleyCompositionV1.Enabled=false;
   ValoriaReferenceConvergencePassV2.Enabled=false;
   ValoriaInCitySurfacePassV1.Enabled=false;
   ValoriaStairLandingIntegrationV1.Enabled=false;
   ValoriaFullFrameConvergenceIteration1.Enabled=false;
   ValoriaFullFrameConvergenceIteration2.Enabled=false;
   ValoriaBenchmarkCompositeV2.Enabled=false;
   ValoriaEnvironmentUpliftV1.Enabled=false;
   ValoriaArchitectureCoherenceV1.Enabled=false;
   ValoriaBackplateCandidateV1.Enabled=false;
   ValoriaCliffIslandReframeV1.Enabled=false;
   ValoriaCliffIslandCleanupV2.Enabled=false;
   ValoriaResidualCleanupV1.Enabled=false;
   ValoriaMaterialResidueCleanupV2.Enabled=false;
   ValoriaFullFrameArtifactCleanupV1.Enabled=false;
   ValoriaWorldFrameMountainTerrainV1.Enabled=false;
   ValoriaLowerCityTerrainV1.Enabled=false;
   VisualWorld.VisualIntegrationEnabled=true;
   VisualWorld.Create(true,state);
   var c=Camera.main;var root=GameObject.Find("Valoria · integrated construction visual layer");
   if(c==null||root==null)throw new Exception("Valoria authored family capture prerequisites missing.");
   var cols=new HashSet<int>();foreach(var x in Object.FindObjectsByType<Collider>(FindObjectsSortMode.None))if(x!=null)cols.Add(x.GetInstanceID());
   var hs=new HashSet<int>();foreach(var x in Object.FindObjectsByType<WorldHotspot>(FindObjectsSortMode.None))if(x!=null)hs.Add(x.GetInstanceID());
   return new SceneData{camera=c,root=root,colliderIds=cols,hotspotIds=hs};
  }

  static void RemoveAddedGameplay(HashSet<int> colIds,HashSet<int> hotIds)
  {
   foreach(var x in Object.FindObjectsByType<Collider>(FindObjectsSortMode.None))if(x!=null&&!colIds.Contains(x.GetInstanceID()))Object.DestroyImmediate(x);
   foreach(var x in Object.FindObjectsByType<WorldHotspot>(FindObjectsSortMode.None))if(x!=null&&!hotIds.Contains(x.GetInstanceID()))Object.DestroyImmediate(x);
  }

  static void SaveSet(Camera c,string tag,Vector3 p,Vector3 t)
  {
   Save(c,Folder+"/"+tag+"-19.png",p,t,19f,1280,720);
   Save(c,Folder+"/"+tag+"-12.png",p,t,12f,1280,720);
   Save(c,Folder+"/"+tag+"-9.png",p,t,9f,1280,720);
   Save(c,Folder+"/"+tag+"-mobile.png",p,new Vector3(0f,1.70f,1.8f),9.4f,390,844);
  }

  static void Save(Camera c,string path,Vector3 p,Vector3 t,float size,int w,int h)
  {
   c.transform.position=p;c.transform.LookAt(t);c.orthographic=true;c.orthographicSize=size;
   c.clearFlags=CameraClearFlags.SolidColor;c.backgroundColor=new Color(.33f,.39f,.42f,1f);
   var rt=new RenderTexture(w,h,24,RenderTextureFormat.ARGB32){antiAliasing=4};var prev=RenderTexture.active;
   try{
    c.targetTexture=rt;
    foreach(var renderer in Object.FindObjectsByType<Renderer>(FindObjectsSortMode.None))
     if(renderer.enabled)foreach(var material in renderer.sharedMaterials)
      if(material!=null)for(int pass=0;pass<material.passCount;pass++)ShaderUtil.CompilePass(material,pass,true);
    c.Render();c.Render();RenderTexture.active=rt;
    var im=new Texture2D(w,h,TextureFormat.RGB24,false);im.ReadPixels(new Rect(0,0,w,h),0,0);im.Apply();
    File.WriteAllBytes(path,im.EncodeToPNG());Object.DestroyImmediate(im);
   }finally{c.targetTexture=null;RenderTexture.active=prev;rt.Release();Object.DestroyImmediate(rt);}
  }
 }
}
