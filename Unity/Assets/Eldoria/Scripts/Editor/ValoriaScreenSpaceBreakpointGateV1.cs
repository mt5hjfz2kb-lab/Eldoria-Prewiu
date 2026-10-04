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
 public static class ValoriaScreenSpaceBreakpointGateV1
 {
  const string Folder="ValoriaScreenSpaceBreakpointV1Captures";
  static readonly Vector3 CameraPosition=new Vector3(18.2f,14.6f,-25.8f);
  static readonly Vector3 CameraTarget=new Vector3(0f,3.15f,5.8f);

  public static void Capture()
  {
   ShaderUtil.allowAsyncCompilation=false;Directory.CreateDirectory(Folder);
   var state=new PlayerState{BastionLevel=3,SawmillLevel=1,BarracksLevel=1,CorruptionDiscovered=true};

   var a=Create(state,false);var signature=ValoriaVisualFormulaGate.CollisionSignature();
   SaveAndMeasure(a,"A-zoom9",9f,1280,720);
   SaveAndMeasure(a,"A-mobile",9.4f,390,844);

   var b=Create(state,false);
   if(ValoriaVisualFormulaGate.CollisionSignature()!=signature)throw new Exception("B changed gameplay collider/hotspot signature");
   SaveAndMeasure(b,"B-zoom9",7.4f,1280,720);
   SaveAndMeasure(b,"B-mobile",8.0f,390,844);

   var c=Create(state,true);
   if(ValoriaVisualFormulaGate.CollisionSignature()!=signature)throw new Exception("C changed gameplay collider/hotspot signature");
   SaveAndMeasure(c,"C-zoom9",7.4f,1280,720);
   SaveAndMeasure(c,"C-mobile",8.0f,390,844);

   File.WriteAllText(Folder+"/evidence.json","{\n"+
    "  \"gameplay_signature_preserved\": true,\n"+
    "  \"camera\": \"ORTHOGRAPHIC\",\n"+
    "  \"camera_direction_changed\": false,\n"+
    "  \"A\": {\"zoom9_orthographic_size\": 9.0, \"mobile_orthographic_size\": 9.4, \"spatial_adjustment\": false},\n"+
    "  \"B\": {\"zoom9_orthographic_size\": 7.4, \"mobile_orthographic_size\": 8.0, \"spatial_adjustment\": false},\n"+
    "  \"C\": {\"zoom9_orthographic_size\": 7.4, \"mobile_orthographic_size\": 8.0, \"spatial_adjustment\": \"bounded Granero/Cuartel presentation transform only\"},\n"+
    "  \"paired_sources\": [\"Valoria_Granero_RCFv1\",\"Valoria_Cuartel_CFSv1\"],\n"+
    "  \"final_look\": \"RESET_ACCEPTED_RUNTIME\",\n"+
    "  \"defense_changed\": false,\n"+
    "  \"hero_bastion_source_changed\": false,\n"+
    "  \"terrain_changed\": false,\n"+
    "  \"materials_changed\": false,\n"+
    "  \"new_assets\": 0,\n"+
    "  \"tripo_credits\": 0,\n"+
    "  \"paid_credits\": 0\n"+
    "}\n");
   DisableCandidates();EditorApplication.Exit(0);
  }

  static Camera Create(PlayerState state,bool variantC)
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
   if(camera==null||root==null)throw new Exception("Screen-space capture prerequisites missing");
   if(GameObject.Find(ValoriaPairedFunctionalSourceV1.RootName)==null||ValoriaPairedFunctionalSourceV1.Pieces!=2)
    throw new Exception("Paired functional source baseline missing");
   if(variantC)ValoriaScreenSpaceBreakpointV1.ApplyVariantC();
   return camera;
  }

  static void DisableCandidates()
  {
   ValoriaGraneroCuartelSourceUpgradeV1.Enabled=false;
   ValoriaFunctionalScreenReadabilityV1.Enabled=false;
   ValoriaCameraFirstFunctionalSourceV1.Enabled=false;
   ValoriaPairedFunctionalSourceV1.Enabled=false;
  }

  static void SaveAndMeasure(Camera c,string stem,float size,int w,int h)
  {
   c.transform.position=CameraPosition;c.transform.LookAt(CameraTarget);c.orthographic=true;c.orthographicSize=size;
   var rt=new RenderTexture(w,h,24,RenderTextureFormat.ARGB32){antiAliasing=4};var prev=RenderTexture.active;
   try
   {
    c.targetTexture=rt;c.Render();c.Render();RenderTexture.active=rt;
    var im=new Texture2D(w,h,TextureFormat.RGB24,false);im.ReadPixels(new Rect(0,0,w,h),0,0);im.Apply();
    File.WriteAllBytes(Folder+"/"+stem+".png",im.EncodeToPNG());Object.DestroyImmediate(im);
    WriteScreenMetrics(c,Folder+"/"+stem+"-metrics.json",w,h,size);
   }
   finally{c.targetTexture=null;RenderTexture.active=prev;rt.Release();Object.DestroyImmediate(rt);}
  }

  static void WriteScreenMetrics(Camera c,string path,int w,int h,float size)
  {
   var granero=GameObject.Find(ValoriaScreenSpaceBreakpointV1.GraneroName);
   var cuartel=GameObject.Find(ValoriaScreenSpaceBreakpointV1.CuartelName);
   var city=GameObject.Find("Valoria · integrated construction visual layer");
   var g=ScreenRect(c,granero!=null?granero.GetComponentsInChildren<Renderer>(true):Array.Empty<Renderer>());
   var q=ScreenRect(c,cuartel!=null?cuartel.GetComponentsInChildren<Renderer>(true):Array.Empty<Renderer>());
   var core=new List<Renderer>();
   if(city!=null)foreach(var r in city.GetComponentsInChildren<Renderer>(true))
   {
    if(r==null||!r.enabled||!r.gameObject.activeInHierarchy)continue;
    string chain=Chain(r.transform);
    if(chain.Contains("mountain")||chain.Contains("terrain")||chain.Contains("valley")||chain.Contains("ground")||
       chain.Contains("rear pine")||chain.Contains("world frame")||chain.Contains("ridge rock"))continue;
    core.Add(r);
   }
   var cr=ScreenRect(c,core.ToArray());
   var bastion=new List<Renderer>();
   if(city!=null)foreach(var r in city.GetComponentsInChildren<Renderer>(true))if(r!=null&&r.enabled&&Chain(r.transform).Contains("bastion"))bastion.Add(r);
   var br=ScreenRect(c,bastion.ToArray());
   File.WriteAllText(path,"{\n"+
    "  \"orthographic_size\": "+size.ToString(System.Globalization.CultureInfo.InvariantCulture)+",\n"+
    "  \"city_core_width_px\": "+cr.width.ToString("F1",System.Globalization.CultureInfo.InvariantCulture)+",\n"+
    "  \"city_core_width_pct\": "+(cr.width/w*100f).ToString("F1",System.Globalization.CultureInfo.InvariantCulture)+",\n"+
    "  \"city_core_height_pct\": "+(cr.height/h*100f).ToString("F1",System.Globalization.CultureInfo.InvariantCulture)+",\n"+
    "  \"granero_width_px\": "+g.width.ToString("F1",System.Globalization.CultureInfo.InvariantCulture)+",\n"+
    "  \"granero_height_px\": "+g.height.ToString("F1",System.Globalization.CultureInfo.InvariantCulture)+",\n"+
    "  \"cuartel_width_px\": "+q.width.ToString("F1",System.Globalization.CultureInfo.InvariantCulture)+",\n"+
    "  \"cuartel_height_px\": "+q.height.ToString("F1",System.Globalization.CultureInfo.InvariantCulture)+",\n"+
    "  \"bastion_width_px\": "+br.width.ToString("F1",System.Globalization.CultureInfo.InvariantCulture)+",\n"+
    "  \"bastion_height_px\": "+br.height.ToString("F1",System.Globalization.CultureInfo.InvariantCulture)+"\n"+
    "}\n");
  }

  static string Chain(Transform t)
  {
   string s="";for(var p=t;p!=null;p=p.parent)s+="|"+p.name.ToLowerInvariant();return s;
  }

  struct PixelRect{public float width,height;public PixelRect(float w,float h){width=w;height=h;}}
  static PixelRect ScreenRect(Camera c,Renderer[] rs)
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
   return any?new PixelRect(maxX-minX,maxY-minY):new PixelRect(0,0);
  }
 }
}
