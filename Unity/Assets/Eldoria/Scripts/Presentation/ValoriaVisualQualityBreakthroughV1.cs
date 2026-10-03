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

   // Candidate 2: retain the accepted defensive ring and contained natural surround.
   // Only replace the boxiest Bastion support masses, then add coherent annex silhouettes
   // inside already-active functional-building envelopes.
   HiddenLegacyWallRenderers=HideBlockyBastionSupports(canonicalRoot);
   AuthoredWallModules=0;ExteriorModules=0;SecondaryFoundationModules=0;BastionTransitionModules=0;
   BuildSecondaryAnnexes(root,state);
   BuildBastionInterface(root);
   StripGameplay(root.gameObject);
  }

  static int HideBlockyBastionSupports(Transform root)
  {
   int count=0;
   foreach(var r in root.GetComponentsInChildren<Renderer>(true))
   {
    if(r==null||!r.enabled)continue;
    string n=Chain(r.transform);
    if(!n.Contains("art consolidation · bastion retaining mass") &&
       !n.Contains("art consolidation · bastion stair plinth"))continue;
    r.enabled=false;count++;
   }
   return count;
  }

  static void BuildSecondaryAnnexes(Transform root,PlayerState state)
  {
   var p1=Resources.Load<GameObject>("Valoria/MidTierArchitectureKit_v1/Piece01");
   var p2=Resources.Load<GameObject>("Valoria/MidTierArchitectureKit_v1/Piece02");
   var p3=Resources.Load<GameObject>("Valoria/MidTierArchitectureKit_v1/Piece03");

   // All annexes sit inside F1/F2/F3 maximum envelopes. They are visual extensions of active
   // functional buildings, not new houses and not occupants of R4/R5/R6.
   if(state.SawmillLevel>0 && p1!=null)
   {
    var g=Place(root,p1,"Aserradero timber annex",new Vector3(-7.45f,.10f,-.70f),1.75f,1.55f,-13f);
    Adapt(g,2f);SecondaryFoundationModules++;
   }
   if(state.BarracksLevel>0 && p2!=null)
   {
    var g=Place(root,p2,"Cuartel service wing",new Vector3(7.35f,.10f,-.82f),1.80f,1.65f,11f);
    Adapt(g,3f);SecondaryFoundationModules++;
   }
   if(p3!=null)
   {
    var g=Place(root,p3,"Granero storehouse wing",new Vector3(-4.05f,.10f,-4.20f),1.62f,1.45f,-7f);
    Adapt(g,4f);SecondaryFoundationModules++;
   }
  }

  static void BuildBastionInterface(Transform root)
  {
   var transition=Resources.Load<GameObject>("Valoria/StoneArchitectureKit_v1/RockToWallTransition");
   var landing=Resources.Load<GameObject>("Valoria/Rescued/StreetLandingTransition");

   if(transition!=null)
   {
    var west=Place(root,transition,"Bastion west transition",new Vector3(-3.05f,.09f,5.30f),2.55f,1.18f,10f);
    Adapt(west,5f);BastionTransitionModules++;
    var east=Place(root,transition,"Bastion east transition",new Vector3(3.05f,.09f,5.30f),2.55f,1.18f,170f);
    Adapt(east,5f);BastionTransitionModules++;
   }
   if(landing!=null)
   {
    var lower=Place(root,landing,"Bastion lower landing",new Vector3(0f,.09f,4.05f),3.85f,.42f,0f);
    Adapt(lower,5f);BastionTransitionModules++;
   }
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

  static void Adapt(GameObject go,float family)
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
     var m=new Material(shader){name="Valoria breakthrough response · "+src.name};
     m.SetTexture("_BaseMap",albedo);m.SetTextureScale("_BaseMap",src.GetTextureScale(bp));m.SetTextureOffset("_BaseMap",src.GetTextureOffset(bp));
     if(normal!=null)m.SetTexture("_BumpMap",normal);
     m.SetFloat("_BumpScale",normal!=null?.72f:0f);m.SetFloat("_Family",family);
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
