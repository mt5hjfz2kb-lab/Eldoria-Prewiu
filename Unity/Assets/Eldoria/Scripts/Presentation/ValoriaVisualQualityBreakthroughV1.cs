using System;
using Eldoria.Domain;
using UnityEngine;
using Object=UnityEngine.Object;

namespace Eldoria.Presentation
{
 public static class ValoriaVisualQualityBreakthroughV1
 {
  public const string RootName="Valoria · Visual Quality Breakthrough v1";
  public static int HiddenLegacyWallRenderers,AuthoredWallModules,SecondaryFoundationModules,BastionTransitionModules,ExteriorModules;

  public static void Apply(Transform canonicalRoot,PlayerState state)
  {
   if(canonicalRoot==null)throw new ArgumentNullException(nameof(canonicalRoot));
   var old=GameObject.Find(RootName);if(old!=null)Object.DestroyImmediate(old);
   var root=new GameObject(RootName).transform;root.SetParent(canonicalRoot,true);
   HiddenLegacyWallRenderers=AuthoredWallModules=SecondaryFoundationModules=BastionTransitionModules=ExteriorModules=0;

   HidePrimitiveMerlons(canonicalRoot);
   BuildAuthoredCurtainCaps(root);
   UpliftFunctionalMaterials(canonicalRoot);
   StripGameplay(root.gameObject);
  }

  static void HidePrimitiveMerlons(Transform root)
  {
   foreach(var r in root.GetComponentsInChildren<Renderer>(true))
   {
    if(r==null||!r.enabled)continue;
    string n=Chain(r.transform);
    if(n.Contains("art consolidation")&&n.Contains(" merlon"))
    {
     r.enabled=false;
     HiddenLegacyWallRenderers++;
    }
   }
  }

  static void BuildAuthoredCurtainCaps(Transform root)
  {
   var wall=Resources.Load<GameObject>("Valoria/Stone_Wall");
   var corner=Resources.Load<GameObject>("Valoria/StoneArchitectureKit_v1/CornerWallL");
   if(wall==null)throw new InvalidOperationException("Canonical Stone_Wall missing.");

   // Front curtains stop before the gatehouse shoulders.
   Line(root,wall,"front west",new Vector3(-8.60f,.18f,-6.28f),new Vector3(1,0,0),
      new[]{2.10f,2.05f,1.85f},new[]{1.23f,1.31f,1.18f},0f);
   Line(root,wall,"front east",new Vector3(4.15f,.18f,-6.28f),new Vector3(1,0,0),
      new[]{1.85f,2.05f,2.10f},new[]{1.18f,1.31f,1.23f},0f);

   // Lower side curtains lead to, but do not occupy, XW / XE future expansion seams.
   Line(root,wall,"west lower",new Vector3(-9.46f,.18f,-4.95f),new Vector3(0,0,1),
      new[]{2.05f,2.15f,2.00f,1.85f},new[]{1.20f,1.29f,1.18f,1.25f},90f);
   Line(root,wall,"east lower",new Vector3(9.46f,.18f,-5.00f),new Vector3(0,0,1),
      new[]{1.90f,2.10f,2.15f,1.90f},new[]{1.24f,1.18f,1.30f,1.20f},90f);

   // Upper side curtains resume after the expansion seams and terminate at rear towers.
   Line(root,wall,"west upper",new Vector3(-9.46f,.18f,4.85f),new Vector3(0,0,1),
      new[]{1.55f,1.70f,1.55f},new[]{1.18f,1.27f,1.16f},90f);
   Line(root,wall,"east upper",new Vector3(9.46f,.18f,4.85f),new Vector3(0,0,1),
      new[]{1.55f,1.70f,1.55f},new[]{1.16f,1.27f,1.18f},90f);

   // Rear rhythm is intentionally longest and quietest; the existing rear watchtowers carry hierarchy.
   Line(root,wall,"rear west",new Vector3(-8.00f,.18f,9.26f),new Vector3(1,0,0),
      new[]{1.80f,2.00f,2.05f,1.85f},new[]{1.14f,1.22f,1.18f,1.12f},0f);
   Line(root,wall,"rear east",new Vector3(.10f,.18f,9.26f),new Vector3(1,0,0),
      new[]{1.85f,2.05f,2.00f,1.80f},new[]{1.12f,1.18f,1.22f,1.14f},0f);

   if(corner!=null)
   {
    Add(root,corner,"front west corner treatment",new Vector3(-9.32f,.16f,-6.06f),1.55f,1.30f,0f);
    Add(root,corner,"front east corner treatment",new Vector3(9.32f,.16f,-6.06f),1.55f,1.30f,180f);
   }
  }

  static void Line(Transform root,GameObject source,string role,Vector3 start,Vector3 axis,float[] spans,float[] heights,float yaw)
  {
   Vector3 p=start;
   for(int i=0;i<spans.Length;i++)
   {
    float span=spans[i];
    Add(root,source,role+" "+i,p,span,heights[i],yaw+(i%2==0?-1.25f:1.25f));
    p+=axis*(span*.93f);
   }
  }

  static void Add(Transform root,GameObject source,string role,Vector3 ground,float footprint,float maxHeight,float yaw)
  {
   var go=ValoriaKit.BenchmarkPiece("Valoria · breakthrough · "+role,source,ground,footprint,maxHeight,Quaternion.Euler(0,yaw,0));
   if(go==null)return;
   go.transform.SetParent(root,true);
   AdaptStone(go);
   StripGameplay(go);
   AuthoredWallModules++;
  }

  static void UpliftFunctionalMaterials(Transform root)
  {
   int count=0;
   foreach(var r in root.GetComponentsInChildren<Renderer>(true))
   {
    if(r==null||!r.enabled)continue;
    string n=Chain(r.transform);
    if(!n.Contains("production · aserradero")&&!n.Contains("production · cuartel")&&!n.Contains("production · granero"))continue;
    var mats=r.sharedMaterials;bool touched=false;
    for(int i=0;i<mats.Length;i++)
    {
     var m=mats[i];
     if(m==null||m.shader==null||m.shader.name!="Eldoria/Valoria Coherence"||!m.HasProperty("_SemanticUplift"))continue;
     m.SetFloat("_SemanticUplift",1f);touched=true;
    }
    if(touched){r.sharedMaterials=mats;count++;}
   }
   SecondaryFoundationModules=count;
  }

  static void AdaptStone(GameObject go)
  {
   var shader=Shader.Find("Eldoria/Valoria Coherence");if(shader==null||go==null)return;
   var rock=Resources.Load<Texture2D>("Valoria/SurfaceCellExternal/rock_diff");
   foreach(var r in go.GetComponentsInChildren<Renderer>(true))
   {
    var srcs=r.sharedMaterials;var dst=new Material[srcs.Length];
    for(int i=0;i<srcs.Length;i++)
    {
     var src=srcs[i];if(src==null)continue;
     string bp;var albedo=Map(src,out bp,"_BaseMap","_BaseColorTexture","baseColorTexture","_MainTex","_Texture","_Albedo");
     string np;var normal=Map(src,out np,"_BumpMap","_NormalTexture","_NormalMap","normalTexture");
     if(albedo==null){dst[i]=src;continue;}
     var m=new Material(shader){name="Valoria breakthrough wall · "+src.name};
     m.SetTexture("_BaseMap",albedo);m.SetTextureScale("_BaseMap",src.GetTextureScale(bp));m.SetTextureOffset("_BaseMap",src.GetTextureOffset(bp));
     if(normal!=null)m.SetTexture("_BumpMap",normal);
     m.SetFloat("_BumpScale",normal!=null?.72f:0f);m.SetFloat("_Family",5f);
     m.SetFloat("_Bottom",r.bounds.min.y);m.SetFloat("_Height",Mathf.Max(.01f,r.bounds.size.y));m.SetFloat("_Smoothness",.05f);
     if(rock!=null)m.SetTexture("_RockMap",rock);dst[i]=m;
    }
    r.sharedMaterials=dst;
   }
  }

  static Texture Map(Material m,out string name,params string[] props)
  {
   foreach(var p in props)if(m.HasProperty(p)&&m.GetTexture(p)!=null){name=p;return m.GetTexture(p);}
   name="";return null;
  }
  static void StripGameplay(GameObject go)
  {
   foreach(var c in go.GetComponentsInChildren<Collider>(true))Object.DestroyImmediate(c);
   foreach(var h in go.GetComponentsInChildren<WorldHotspot>(true))Object.DestroyImmediate(h);
   foreach(var lod in go.GetComponentsInChildren<LODGroup>(true))lod.ForceLOD(0);
  }
  static string Chain(Transform t){string s="";for(;t!=null;t=t.parent)s+="|"+t.name.ToLowerInvariant();return s;}
 }
}
