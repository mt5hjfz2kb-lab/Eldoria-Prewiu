using System;
using Eldoria.Domain;
using UnityEngine;
using UnityEngine.Rendering;
using Object=UnityEngine.Object;

namespace Eldoria.Presentation
{
 public static class ValoriaVisualQualityBreakthroughV1
 {
  public const string RootName="Valoria · Visual Quality Breakthrough v1";
  public static int HiddenLegacyWallRenderers,AuthoredWallModules,SecondaryFoundationModules,BastionTransitionModules,ExteriorModules;
  static readonly Color Stone=new Color(.58f,.57f,.53f,1f);
  static readonly Color StoneDark=new Color(.45f,.45f,.43f,1f);
  static readonly Color Earth=new Color(.42f,.34f,.24f,1f);

  public static void Apply(Transform canonicalRoot,PlayerState state)
  {
   if(canonicalRoot==null)throw new ArgumentNullException(nameof(canonicalRoot));
   var old=GameObject.Find(RootName);if(old!=null)Object.DestroyImmediate(old);
   var root=new GameObject(RootName).transform;root.SetParent(canonicalRoot,true);
   HiddenLegacyWallRenderers=HidePrototypeWallAndBastionBlocks(canonicalRoot);
   AuthoredWallModules=SecondaryFoundationModules=BastionTransitionModules=ExteriorModules=0;
   BuildAuthoredDefensiveRing(root);
   UpgradeSecondaryArchitecture(root,state);
   ReauthorBastionInterface(root);
   BuildContainedExterior(root);
   DisableGameplay(root.gameObject);
  }

  static int HidePrototypeWallAndBastionBlocks(Transform root)
  {
   int count=0;
   foreach(var r in root.GetComponentsInChildren<Renderer>(true))
   {
    if(r==null||!r.enabled)continue;
    string n=Chain(r.transform);
    bool oldWall=n.Contains("art consolidation · front west base")||
      n.Contains("art consolidation · front east base")||
      n.Contains("art consolidation · west lower base")||
      n.Contains("art consolidation · west upper base")||
      n.Contains("art consolidation · east lower base")||
      n.Contains("art consolidation · east upper base")||
      n.Contains("art consolidation · rear base")||
      n.Contains(" merlon");
    bool oldBastion=n.Contains("art consolidation · bastion retaining mass")||
      n.Contains("art consolidation · bastion stair plinth")||
      n.Contains("art consolidation · bastion west retaining face")||
      n.Contains("art consolidation · bastion east retaining face")||
      n.Contains("art consolidation · bastion west return")||
      n.Contains("art consolidation · bastion east return");
    if(!oldWall&&!oldBastion)continue;
    r.enabled=false;count++;
   }
   return count;
  }

  static void BuildAuthoredDefensiveRing(Transform root)
  {
   var straight=Resources.Load<GameObject>("Valoria/StoneArchitectureKit_v1/HighStraightWall");
   var corner=Resources.Load<GameObject>("Valoria/StoneArchitectureKit_v1/CornerWallL");
   var transition=Resources.Load<GameObject>("Valoria/StoneArchitectureKit_v1/RockToWallTransition");
   if(straight==null)straight=Resources.Load<GameObject>("Valoria/Stone_Wall");
   if(corner==null)corner=straight;
   if(transition==null)transition=straight;

   // Long authored curtains replace dozens of identical cube merlons.
   // XW/XE remain open around z~3.65 as hard growth interfaces.
   Place(root,straight,"front-west curtain",new Vector3(-6.35f,.08f,-6.28f),5.55f,1.42f,0f); 
   Place(root,straight,"front-east curtain",new Vector3( 6.35f,.08f,-6.28f),5.30f,1.34f,0f);
   Place(root,straight,"west-lower curtain",new Vector3(-9.46f,.08f,-1.35f),6.25f,1.28f,90f);
   Place(root,straight,"east-lower curtain",new Vector3( 9.46f,.08f,-1.55f),6.05f,1.36f,90f);
   Place(root,straight,"west-upper curtain",new Vector3(-9.46f,.08f,7.15f),3.80f,1.18f,90f);
   Place(root,straight,"east-upper curtain",new Vector3( 9.46f,.08f,7.00f),4.05f,1.26f,90f);
   Place(root,straight,"rear-west curtain",new Vector3(-5.85f,.08f,9.28f),6.45f,1.12f,0f);
   Place(root,straight,"rear-east curtain",new Vector3( 5.75f,.08f,9.28f),6.70f,1.18f,0f);

   // Corners and gate shoulders deliberately differ in mass and height to create rhythm.
   Place(root,corner,"front-west corner",new Vector3(-9.25f,.08f,-6.05f),2.25f,1.55f,0f);
   Place(root,corner,"front-east corner",new Vector3( 9.25f,.08f,-6.05f),2.15f,1.45f,180f);
   Place(root,transition,"gate west transition",new Vector3(-4.20f,.07f,-6.33f),1.65f,.92f,8f);
   Place(root,transition,"gate east transition",new Vector3( 4.15f,.07f,-6.31f),1.55f,.86f,-8f);
  }

  static void UpgradeSecondaryArchitecture(Transform root,PlayerState state)
  {
   var foundation=Resources.Load<GameObject>("Valoria/StoneKit/piece_05_4966tris");
   var landing=Resources.Load<GameObject>("Valoria/Rescued/StreetLandingTransition");
   var block=Resources.Load<GameObject>("Valoria/StoneKit/piece_06_7602tris");
   if(foundation!=null)
   {
    if(state.SawmillLevel>0){Place(root,foundation,"Aserradero dressed foundation",new Vector3(-5.95f,.04f,-1.55f),4.75f,.42f,-5f);SecondaryFoundationModules++;}
    if(state.BarracksLevel>0){Place(root,foundation,"Cuartel dressed foundation",new Vector3(5.95f,.04f,-1.75f),4.85f,.44f,4f);SecondaryFoundationModules++;}
    Place(root,foundation,"Granero dressed foundation",new Vector3(-2.75f,.04f,-4.38f),4.10f,.38f,0f);SecondaryFoundationModules++;
   }
   if(landing!=null)
   {
    Place(root,landing,"Aserradero work landing",new Vector3(-5.95f,.06f,-3.35f),2.90f,.42f,0f);SecondaryFoundationModules++;
    Place(root,landing,"Cuartel training landing",new Vector3(5.95f,.06f,-3.45f),3.05f,.42f,180f);SecondaryFoundationModules++;
   }
   if(block!=null)
   {
    Place(root,block,"Granero stone storage",new Vector3(-4.15f,.06f,-5.35f),1.10f,.65f,18f);SecondaryFoundationModules++;
    Place(root,block,"Cuartel buttress cache",new Vector3(7.55f,.06f,-2.95f),.95f,.58f,-12f);SecondaryFoundationModules++;
   }
  }

  static void ReauthorBastionInterface(Transform root)
  {
   var transition=Resources.Load<GameObject>("Valoria/StoneArchitectureKit_v1/RockToWallTransition");
   var seam=Resources.Load<GameObject>("Valoria/Rescued/RockTerrainSeamFiller");
   var landing=Resources.Load<GameObject>("Valoria/Rescued/StreetLandingTransition");
   var stair=Resources.Load<GameObject>("Valoria/Rescued/TerraceStairRock");

   if(transition!=null)
   {
    Place(root,transition,"Bastion west architectural transition",new Vector3(-3.20f,.08f,5.30f),3.00f,1.55f,12f);BastionTransitionModules++;
    Place(root,transition,"Bastion east architectural transition",new Vector3( 3.20f,.08f,5.30f),3.00f,1.55f,168f);BastionTransitionModules++;
   }
   if(seam!=null)
   {
    Place(root,seam,"Bastion west rubble seam",new Vector3(-3.30f,.05f,6.65f),2.25f,.88f,92f);BastionTransitionModules++;
    Place(root,seam,"Bastion east rubble seam",new Vector3( 3.30f,.05f,6.65f),2.20f,.84f,-92f);BastionTransitionModules++;
   }
   if(landing!=null){Place(root,landing,"Bastion lower landing transition",new Vector3(0f,.08f,4.10f),4.45f,.48f,0f);BastionTransitionModules++;}
   if(stair!=null){Place(root,stair,"Bastion stair rock skirt",new Vector3(0f,.06f,5.00f),3.70f,.88f,0f);BastionTransitionModules++;}
  }

  static void BuildContainedExterior(Transform root)
  {
   var dirt=Resources.Load<Texture2D>("Valoria/SurfaceCellExternal/dirt_diff");
   var shader=Shader.Find("Eldoria/Valoria Coherence");
   Material fieldMat;
   if(shader!=null&&dirt!=null)
   {
    fieldMat=new Material(shader){name="Valoria breakthrough field earth"};
    fieldMat.SetTexture("_BaseMap",dirt);fieldMat.SetFloat("_Family",6);fieldMat.SetFloat("_BumpScale",0);fieldMat.SetColor("_BaseColor",new Color(.92f,.88f,.72f,1));
   }
   else fieldMat=ValoriaKit.DetailedSurfaceMaterial(Earth,"earth",new Vector2(4,4),.9f);

   // Bounded cultivated wedges flank the gate road; no giant terrain, no parcel occupation.
   Patch(root,"south-west field",new[]{new Vector2(-15.5f,-12.5f),new Vector2(-4.0f,-12.5f),new Vector2(-3.2f,-7.6f),new Vector2(-10.5f,-7.1f),new Vector2(-15.0f,-8.4f)},-.01f,fieldMat);
   Patch(root,"south-east field",new[]{new Vector2(4.0f,-12.3f),new Vector2(15.0f,-11.4f),new Vector2(15.4f,-7.5f),new Vector2(10.2f,-7.0f),new Vector2(3.2f,-7.6f)},-.012f,fieldMat);
   Patch(root,"west pasture",new[]{new Vector2(-15.2f,-5.8f),new Vector2(-10.5f,-5.9f),new Vector2(-10.9f,1.2f),new Vector2(-15.8f,.2f)},-.015f,fieldMat);

   var art=ValoriaExternalAssetLibrary.Load();
   var bush=art!=null?art.SlavicBush:null;
   if(bush!=null)
   {
    foreach(var v in new[]{
      new Vector4(-12.6f,-7.6f,14,1.15f),new Vector4(-9.0f,-7.4f,42,1.00f),new Vector4(-5.2f,-7.55f,77,.95f),
      new Vector4(5.5f,-7.45f,118,.95f),new Vector4(9.1f,-7.35f,154,1.05f),new Vector4(12.7f,-7.6f,194,1.15f),
      new Vector4(-11.2f,-4.0f,231,.95f),new Vector4(-11.0f,-.4f,275,1.0f)})
    {
     var g=ValoriaKit.BenchmarkPieceModulated("Valoria · breakthrough · hedgerow",bush,new Vector3(v.x,.02f,v.y),v.w,.62f,Quaternion.Euler(0,v.z,0),new Color(.62f,.72f,.48f,1));
     if(g==null)continue;g.transform.SetParent(root,true);StripGameplay(g);ExteriorModules++;
    }
   }
   var rock=Resources.Load<GameObject>("Valoria/Rescued/RockTerrainSeamFiller");
   if(rock!=null)
   {
    Place(root,rock,"roadside west stone",new Vector3(-3.15f,.02f,-8.15f),1.20f,.42f,28f);ExteriorModules++;
    Place(root,rock,"roadside east stone",new Vector3(3.20f,.02f,-8.05f),1.05f,.38f,-18f);ExteriorModules++;
   }
  }

  static GameObject Place(Transform root,GameObject source,string role,Vector3 ground,float footprint,float maxHeight,float yaw)
  {
   if(source==null)return null;
   var go=Object.Instantiate(source);go.name="Valoria · breakthrough · "+role;go.transform.SetParent(root,true);
   go.transform.position=Vector3.zero;go.transform.rotation=Quaternion.Euler(0,yaw,0);go.transform.localScale=Vector3.one;
   var b=BoundsOf(go);
   float w=Mathf.Max(b.size.x,b.size.z),h=Mathf.Max(.001f,b.size.y);
   float s=Mathf.Min(footprint/Mathf.Max(.001f,w),maxHeight/h);
   go.transform.localScale=Vector3.one*s;
   b=BoundsOf(go);go.transform.position+=new Vector3(ground.x-b.center.x,ground.y-b.min.y,ground.z-b.center.z);
   StripGameplay(go);AuthoredWallModules++;
   return go;
  }

  static Bounds BoundsOf(GameObject go)
  {
   var rs=go.GetComponentsInChildren<Renderer>(true);if(rs.Length==0)return new Bounds(go.transform.position,Vector3.one);
   var b=rs[0].bounds;for(int i=1;i<rs.Length;i++)b.Encapsulate(rs[i].bounds);return b;
  }

  static void Patch(Transform root,string name,Vector2[] ring,float y,Material mat)
  {
   if(ring==null||ring.Length<3)return;int n=ring.Length;
   var verts=new Vector3[n+1];Vector2 c=Vector2.zero;for(int i=0;i<n;i++)c+=ring[i];c/=n;verts[0]=new Vector3(c.x,y,c.y);
   for(int i=0;i<n;i++)verts[i+1]=new Vector3(ring[i].x,y,ring[i].y);
   var tris=new int[n*3];for(int i=0;i<n;i++){tris[i*3]=0;tris[i*3+1]=((i+1)%n)+1;tris[i*3+2]=i+1;}
   var mesh=new Mesh{name="Valoria breakthrough "+name};mesh.vertices=verts;mesh.triangles=tris;mesh.RecalculateNormals();mesh.RecalculateBounds();
   var go=new GameObject("Valoria · breakthrough · "+name);go.transform.SetParent(root,true);go.AddComponent<MeshFilter>().sharedMesh=mesh;
   var r=go.AddComponent<MeshRenderer>();r.sharedMaterial=mat;r.receiveShadows=true;r.shadowCastingMode=ShadowCastingMode.Off;ExteriorModules++;
  }

  static void StripGameplay(GameObject go)
  {
   foreach(var c in go.GetComponentsInChildren<Collider>(true))Object.DestroyImmediate(c);
   foreach(var h in go.GetComponentsInChildren<WorldHotspot>(true))Object.DestroyImmediate(h);
   foreach(var lod in go.GetComponentsInChildren<LODGroup>(true))lod.ForceLOD(0);
  }
  static void DisableGameplay(GameObject go){StripGameplay(go);}
  static string Chain(Transform t){string s="";while(t!=null){s=t.name.ToLowerInvariant()+"/"+s;t=t.parent;}return s;}
 }
}
