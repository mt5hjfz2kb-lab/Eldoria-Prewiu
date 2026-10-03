using System;
using Eldoria.Domain;
using UnityEngine;
using Object=UnityEngine.Object;

namespace Eldoria.Presentation
{
 public static class ValoriaVisualQualityBreakthroughV1
 {
  public const string RootName="Valoria · Visual Quality Breakthrough v1";
  public static string HeroVariant="low20";
  public static bool ArchitecturalTerrace=true;
  public static int HiddenLegacyWallRenderers,AuthoredWallModules,SecondaryFoundationModules,BastionTransitionModules,ExteriorModules;

  public static void Apply(Transform canonicalRoot,PlayerState state)
  {
   if(canonicalRoot==null)throw new ArgumentNullException(nameof(canonicalRoot));
   var old=GameObject.Find(RootName);if(old!=null)Object.DestroyImmediate(old);
   var root=new GameObject(RootName).transform;root.SetParent(canonicalRoot,true);
   HiddenLegacyWallRenderers=AuthoredWallModules=SecondaryFoundationModules=BastionTransitionModules=ExteriorModules=0;

   var hero=GameObject.Find("Valoria · Flat Citadel · Hero Bastion");
   if(hero==null)throw new InvalidOperationException("Canonical Hero Bastion not found.");

   string key="Valoria/VQBSegments/Valoria_HeroBastion_segmented_"+HeroVariant;
   var source=Resources.Load<GameObject>(key);
   if(source==null)throw new InvalidOperationException("Segmented Hero candidate missing: "+key);

   Material heroResponse=null;
   foreach(var r in hero.GetComponentsInChildren<Renderer>(true))
   {
    if(heroResponse==null&&r.sharedMaterials.Length>0)heroResponse=r.sharedMaterials[0];
    if(r.enabled){r.enabled=false;HiddenLegacyWallRenderers++;}
   }

   var candidate=Object.Instantiate(source);
   candidate.name="Valoria · breakthrough · Hero Bastion "+HeroVariant;
   candidate.transform.SetParent(hero.transform.parent,true);
   candidate.transform.position=hero.transform.position;
   candidate.transform.rotation=hero.transform.rotation;
   candidate.transform.localScale=hero.transform.localScale;
   foreach(var r in candidate.GetComponentsInChildren<Renderer>(true))
   {
    if(heroResponse==null)continue;
    var mats=r.sharedMaterials;for(int i=0;i<mats.Length;i++)mats[i]=heroResponse;r.sharedMaterials=mats;
   }
   StripGameplay(candidate);

   if(ArchitecturalTerrace)
   {
    HideOldBlockyBastionInterface(canonicalRoot);
    BuildArchitecturalTerrace(root);
   }
  }

  static void HideOldBlockyBastionInterface(Transform root)
  {
   foreach(var r in root.GetComponentsInChildren<Renderer>(true))
   {
    if(r==null||!r.enabled)continue;
    string n=Chain(r.transform);
    bool hide=n.Contains("art consolidation · bastion retaining mass")||
              n.Contains("art consolidation · bastion stair plinth")||
              n.Contains("art consolidation · bastion west retaining face")||
              n.Contains("art consolidation · bastion east retaining face")||
              n.Contains("art consolidation · bastion west return")||
              n.Contains("art consolidation · bastion east return");
    if(hide){r.enabled=false;HiddenLegacyWallRenderers++;}
   }
  }

  static void BuildArchitecturalTerrace(Transform root)
  {
   var straight=Resources.Load<GameObject>("Valoria/StoneArchitectureKit_v1/HighStraightWall");
   var corner=Resources.Load<GameObject>("Valoria/StoneArchitectureKit_v1/CornerWallL");
   var transition=Resources.Load<GameObject>("Valoria/StoneArchitectureKit_v1/RockToWallTransition");
   var landing=Resources.Load<GameObject>("Valoria/Rescued/StreetLandingTransition");
   if(straight==null||transition==null)throw new InvalidOperationException("Canonical Bastion terrace kit missing.");

   // Front terrace shoulders frame the accepted processional stair rather than covering it.
   AddStone(root,straight,"Bastion terrace west front",new Vector3(-2.55f,.12f,5.02f),3.25f,1.48f,0f);
   AddStone(root,straight,"Bastion terrace east front",new Vector3( 2.55f,.12f,5.02f),3.25f,1.48f,180f);

   // Side returns bury the residual rock silhouette and make the upper level read as a built citadel terrace.
   AddStone(root,straight,"Bastion terrace west return",new Vector3(-3.72f,.12f,6.28f),2.95f,1.34f,90f);
   AddStone(root,straight,"Bastion terrace east return",new Vector3( 3.72f,.12f,6.28f),2.95f,1.34f,90f);

   if(corner!=null)
   {
    AddStone(root,corner,"Bastion terrace west corner",new Vector3(-3.55f,.11f,5.18f),1.55f,1.34f,0f);
    AddStone(root,corner,"Bastion terrace east corner",new Vector3( 3.55f,.11f,5.18f),1.55f,1.34f,180f);
   }

   // Authored rock-to-wall pieces absorb the unavoidable irregular Hero edge instead of exposing a raw cut.
   AddStone(root,transition,"Bastion west seam",new Vector3(-2.95f,.10f,6.35f),2.35f,1.02f,14f);
   AddStone(root,transition,"Bastion east seam",new Vector3( 2.95f,.10f,6.35f),2.35f,1.02f,166f);

   if(landing!=null)
    AddStone(root,landing,"Bastion processional landing",new Vector3(0f,.10f,4.18f),4.20f,.46f,0f);
  }

  static void AddStone(Transform root,GameObject source,string role,Vector3 ground,float footprint,float maxHeight,float yaw)
  {
   var go=Place(root,source,role,ground,footprint,maxHeight,yaw);
   AdaptStone(go);
   BastionTransitionModules++;
  }

  static GameObject Place(Transform root,GameObject source,string role,Vector3 ground,float footprint,float maxHeight,float yaw)
  {
   if(source==null)return null;
   var go=Object.Instantiate(source);go.name="Valoria · breakthrough · "+role;go.transform.SetParent(root,true);
   go.transform.position=Vector3.zero;go.transform.rotation=Quaternion.Euler(0,yaw,0);go.transform.localScale=Vector3.one;
   var b=BoundsOf(go);float w=Mathf.Max(b.size.x,b.size.z),h=Mathf.Max(.001f,b.size.y);
   float s=Mathf.Min(footprint/Mathf.Max(.001f,w),maxHeight/h);go.transform.localScale=Vector3.one*s;
   b=BoundsOf(go);go.transform.position+=new Vector3(ground.x-b.center.x,ground.y-b.min.y,ground.z-b.center.z);
   StripGameplay(go);return go;
  }

  static void AdaptStone(GameObject go)
  {
   if(go==null)return;
   var shader=Shader.Find("Eldoria/Valoria Coherence");if(shader==null)return;
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
     var m=new Material(shader){name="Valoria breakthrough terrace · "+src.name};
     m.SetTexture("_BaseMap",albedo);m.SetTextureScale("_BaseMap",src.GetTextureScale(bp));m.SetTextureOffset("_BaseMap",src.GetTextureOffset(bp));
     if(normal!=null)m.SetTexture("_BumpMap",normal);
     m.SetFloat("_BumpScale",normal!=null?.72f:0);m.SetFloat("_Family",5f);
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
  static Bounds BoundsOf(GameObject go)
  {
   var rs=go.GetComponentsInChildren<Renderer>(true);if(rs.Length==0)return new Bounds(go.transform.position,Vector3.one);
   var b=rs[0].bounds;for(int i=1;i<rs.Length;i++)b.Encapsulate(rs[i].bounds);return b;
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
