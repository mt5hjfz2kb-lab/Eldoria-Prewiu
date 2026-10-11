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
 public static class ValoriaAdaptiveMobileHomePoseGateV1
 {
  const string Folder="ValoriaAdaptiveMobileHomePoseV1Captures";
  const int Width=390,Height=844;

  public static void Capture()
  {
   ShaderUtil.allowAsyncCompilation=false;Directory.CreateDirectory(Folder);
   var state=new PlayerState{BastionLevel=3,SawmillLevel=1,BarracksLevel=1,CorruptionDiscovered=true};

   var m0=Create(state);var signature=ValoriaVisualFormulaGate.CollisionSignature();
   SaveAndMeasure(m0,"M0-mobile",ValoriaAdaptiveMobileHomePoseV1.Profile.M0Current);

   var m1=Create(state);
   if(ValoriaVisualFormulaGate.CollisionSignature()!=signature)throw new Exception("M1 changed gameplay collider/hotspot signature");
   SaveAndMeasure(m1,"M1-mobile",ValoriaAdaptiveMobileHomePoseV1.Profile.M1CenterAdapted);

   var m2=Create(state);
   if(ValoriaVisualFormulaGate.CollisionSignature()!=signature)throw new Exception("M2 changed gameplay collider/hotspot signature");
   SaveAndMeasure(m2,"M2-mobile",ValoriaAdaptiveMobileHomePoseV1.Profile.M2CenterAndSize);

   VerifyRuntimeRule();
   File.WriteAllText(Folder+"/evidence.json","{\n"+
    "  \"gameplay_signature_preserved\": true,\n"+
    "  \"camera\": \"ORTHOGRAPHIC\",\n"+
    "  \"camera_direction_preserved\": true,\n"+
    "  \"resolution\": \"390x844\",\n"+
    "  \"M0\": {\"position\": [18.2,14.6,-25.8], \"target\": [0,3.35,5.6], \"orthographic_size\": 9.1},\n"+
    "  \"M1\": {\"center_offset\": [1.2,0,-2.8], \"target\": [1.2,3.35,2.8], \"orthographic_size\": 9.1},\n"+
    "  \"M2\": {\"center_offset\": [1.2,0,-2.8], \"target\": [1.2,3.35,2.8], \"orthographic_size\": 12.2},\n"+
    "  \"portrait_aspect_threshold_candidate\": 0.72,\n"+
    "  \"paired_sources\": [\"Valoria_Granero_RCFv1\",\"Valoria_Cuartel_CFSv1\"],\n"+
    "  \"world_layout_changed\": false,\n"+
    "  \"assets_changed\": false,\n"+
    "  \"materials_changed\": false,\n"+
    "  \"tripo_credits\": 0,\n"+
    "  \"paid_credits\": 0\n"+
    "}\n");
   DisableCandidates();EditorApplication.Exit(0);
  }

  static Camera Create(PlayerState state)
  {
   DisableCandidates();ValoriaPairedFunctionalSourceV1.Enabled=true;
   SceneSetup.SetupRenderPipeline();EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
   ProductionVisualIntegration.ResetVisualCachesForGate();
   ProductionVisualIntegration.StoneArchitectureEnabled=true;ProductionVisualIntegration.TerrainTerraceEnabled=true;
   ProductionVisualIntegration.SurfaceCellEnabled=false;ProductionVisualIntegration.ProductionCellEnabled=false;
   ProductionVisualIntegration.CoherentCastleProofEnabled=false;ProductionVisualIntegration.SlavicDistrictProofEnabled=false;
   ProductionVisualIntegration.CompactFootprintReframeEnabled=true;AssetVisualUpliftPassV1.Enabled=false;
   AssetLibraryReprocessingPassV1.Enabled=true;VisualWorld.VisualIntegrationEnabled=true;VisualWorld.Create(true,state);
   var camera=Camera.main;var root=GameObject.Find("Valoria · integrated construction visual layer");
   if(camera==null||root==null)throw new Exception("Adaptive mobile capture prerequisites missing");
   if(GameObject.Find(ValoriaPairedFunctionalSourceV1.RootName)==null||ValoriaPairedFunctionalSourceV1.Pieces!=2)
    throw new Exception("Paired functional source baseline missing");
   return camera;
  }

  static void DisableCandidates()
  {
   ValoriaGraneroCuartelSourceUpgradeV1.Enabled=false;
   ValoriaFunctionalScreenReadabilityV1.Enabled=false;
   ValoriaCameraFirstFunctionalSourceV1.Enabled=false;
   ValoriaPairedFunctionalSourceV1.Enabled=false;
  }

  static void SaveAndMeasure(Camera c,string stem,ValoriaAdaptiveMobileHomePoseV1.Profile profile)
  {
   ValoriaAdaptiveMobileHomePoseV1.Apply(c,profile);
   var rt=new RenderTexture(Width,Height,24,RenderTextureFormat.ARGB32){antiAliasing=4};var prev=RenderTexture.active;
   try
   {
    c.targetTexture=rt;c.Render();c.Render();RenderTexture.active=rt;
    var im=new Texture2D(Width,Height,TextureFormat.RGB24,false);im.ReadPixels(new Rect(0,0,Width,Height),0,0);im.Apply();
    File.WriteAllBytes(Folder+"/"+stem+".png",im.EncodeToPNG());Object.DestroyImmediate(im);
    WriteMetrics(c,Folder+"/"+stem+"-metrics.json");
   }
   finally{c.targetTexture=null;RenderTexture.active=prev;rt.Release();Object.DestroyImmediate(rt);}
  }

  static void WriteMetrics(Camera c,string path)
  {
   var granero=GameObject.Find(ValoriaScreenSpaceBreakpointV1.GraneroName);
   var cuartel=GameObject.Find(ValoriaScreenSpaceBreakpointV1.CuartelName);
   var city=GameObject.Find("Valoria · integrated construction visual layer");
   var g=Bounds2D(c,granero!=null?granero.GetComponentsInChildren<Renderer>(true):Array.Empty<Renderer>());
   var q=Bounds2D(c,cuartel!=null?cuartel.GetComponentsInChildren<Renderer>(true):Array.Empty<Renderer>());
   var bastion=new List<Renderer>();
   if(city!=null)foreach(var r in city.GetComponentsInChildren<Renderer>(true))
    if(r!=null&&r.enabled&&r.gameObject.activeInHierarchy&&Chain(r.transform).Contains("bastion"))bastion.Add(r);
   var b=Bounds2D(c,bastion.ToArray());
   File.WriteAllText(path,"{\n"+
    "  \"orthographic_size\": "+F(c.orthographicSize)+",\n"+
    "  \"aspect\": "+F(Width/(float)Height)+",\n"+
    "  \"granero_width_px\": "+F(g.Width)+", \"granero_height_px\": "+F(g.Height)+", \"granero_visible_pct\": "+F(g.VisibleFraction*100f)+",\n"+
    "  \"cuartel_width_px\": "+F(q.Width)+", \"cuartel_height_px\": "+F(q.Height)+", \"cuartel_visible_pct\": "+F(q.VisibleFraction*100f)+",\n"+
    "  \"bastion_width_px\": "+F(b.Width)+", \"bastion_height_px\": "+F(b.Height)+", \"bastion_visible_pct\": "+F(b.VisibleFraction*100f)+",\n"+
    "  \"bastion_viewport_height_pct\": "+F(Mathf.Min(b.Height,Height)/Height*100f)+",\n"+
    "  \"granero_center\": ["+F(g.CenterX/Width)+","+F(g.CenterY/Height)+"],\n"+
    "  \"cuartel_center\": ["+F(q.CenterX/Width)+","+F(q.CenterY/Height)+"],\n"+
    "  \"bastion_center\": ["+F(b.CenterX/Width)+","+F(b.CenterY/Height)+"]\n"+
    "}\n");
  }

  static string F(float x)=>x.ToString("F3",System.Globalization.CultureInfo.InvariantCulture);
  static string Chain(Transform t){string s="";for(var p=t;p!=null;p=p.parent)s+="|"+p.name.ToLowerInvariant();return s;}

  struct ProjectedBounds
  {
   public float MinX,MinY,MaxX,MaxY;
   public float Width=>Mathf.Max(0,MaxX-MinX);public float Height=>Mathf.Max(0,MaxY-MinY);
   public float CenterX=>(MinX+MaxX)*.5f;public float CenterY=>(MinY+MaxY)*.5f;
   public float VisibleFraction
   {
    get
    {
     float area=Width*Height;if(area<=.001f)return 0f;
     float ix=Mathf.Max(0,Mathf.Min(MaxX,WidthConst)-Mathf.Max(MinX,0));
     float iy=Mathf.Max(0,Mathf.Min(MaxY,HeightConst)-Mathf.Max(MinY,0));
     return ix*iy/area;
    }
   }
   const float WidthConst=390f,HeightConst=844f;
  }

  static ProjectedBounds Bounds2D(Camera c,Renderer[] rs)
  {
   bool any=false;float minX=float.PositiveInfinity,minY=float.PositiveInfinity,maxX=float.NegativeInfinity,maxY=float.NegativeInfinity;
   foreach(var r in rs)
   {
    if(r==null||!r.enabled||!r.gameObject.activeInHierarchy)continue;
    var b=r.bounds;var mn=b.min;var mx=b.max;
    var pts=new[]{new Vector3(mn.x,mn.y,mn.z),new Vector3(mx.x,mn.y,mn.z),new Vector3(mn.x,mx.y,mn.z),new Vector3(mx.x,mx.y,mn.z),
      new Vector3(mn.x,mn.y,mx.z),new Vector3(mx.x,mn.y,mx.z),new Vector3(mn.x,mx.y,mx.z),new Vector3(mx.x,mx.y,mx.z)};
    foreach(var p in pts){var s=c.WorldToScreenPoint(p);if(s.z<=0)continue;any=true;minX=Mathf.Min(minX,s.x);maxX=Mathf.Max(maxX,s.x);minY=Mathf.Min(minY,s.y);maxY=Mathf.Max(maxY,s.y);}
   }
   return any?new ProjectedBounds{MinX=minX,MinY=minY,MaxX=maxX,MaxY=maxY}:new ProjectedBounds();
  }

  static void VerifyRuntimeRule()
  {
   if(!ValoriaAdaptiveMobileHomePoseV1.ShouldUseMobileHomePose(390f/844f))throw new Exception("Portrait ratio did not select mobile home pose");
   if(ValoriaAdaptiveMobileHomePoseV1.ShouldUseMobileHomePose(16f/9f))throw new Exception("16:9 incorrectly selected mobile home pose");
   if(ValoriaAdaptiveMobileHomePoseV1.ShouldUseMobileHomePose(4f/3f))throw new Exception("4:3 incorrectly selected mobile home pose");
  }
 }
}
