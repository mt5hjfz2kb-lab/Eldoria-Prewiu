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
   BuildContainedExteriorEdge(root);
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


  static void BuildContainedExteriorEdge(Transform root)
  {
   // Cardinal growth interfaces remain open. Landscape cues sit diagonally/outside the wall
   // and frame the existing south road instead of filling future parcels.
   var shader=Shader.Find("Eldoria/Valoria Coherence");
   var dirt=Resources.Load<Texture2D>("Valoria/SurfaceCellExternal/dirt_diff");
   if(shader==null||dirt==null)return;
   var mat=new Material(shader){name="Valoria breakthrough low-relief world edge"};
   mat.SetTexture("_BaseMap",dirt);mat.SetFloat("_Ground",1f);mat.SetFloat("_BumpScale",0f);
   mat.SetColor("_BaseColor",new Color(.96f,.94f,.86f,1f));

   BermRibbon(root,"south road west bank",
      new[]{new Vector2(-3.8f,-7.0f),new Vector2(-4.55f,-9.3f),new Vector2(-5.65f,-12.1f),new Vector2(-7.0f,-15.0f)},1.25f,.22f,mat);
   BermRibbon(root,"south road east bank",
      new[]{new Vector2(3.8f,-7.0f),new Vector2(4.55f,-9.3f),new Vector2(5.55f,-12.1f),new Vector2(6.9f,-15.0f)},1.25f,.22f,mat);

   Mound(root,"south-west diagonal mound",new Vector2(-12.4f,-9.5f),new Vector2(3.0f,2.0f),.24f,mat);
   Mound(root,"south-east diagonal mound",new Vector2(12.2f,-9.2f),new Vector2(2.8f,1.9f),.22f,mat);
   Mound(root,"north-west diagonal mound",new Vector2(-12.2f,11.5f),new Vector2(2.8f,1.9f),.22f,mat);
   Mound(root,"north-east diagonal mound",new Vector2(12.0f,11.3f),new Vector2(2.7f,1.8f),.20f,mat);

   var art=ValoriaExternalAssetLibrary.Load();
   var bush=art!=null?art.SlavicBush:null;
   if(bush!=null)
   {
    var spots=new[]{
     new Vector4(-4.65f,-8.5f,17f,.88f),new Vector4(-5.55f,-10.7f,63f,.72f),new Vector4(-6.6f,-13.2f,121f,.84f),
     new Vector4(4.65f,-8.6f,199f,.82f),new Vector4(5.45f,-10.8f,247f,.70f),new Vector4(6.55f,-13.1f,301f,.88f),
     new Vector4(-12.8f,-9.4f,31f,.92f),new Vector4(-11.5f,-9.8f,156f,.74f),
     new Vector4(12.4f,-9.1f,221f,.88f),new Vector4(11.2f,-9.5f,284f,.72f),
     new Vector4(-12.5f,11.5f,75f,.82f),new Vector4(12.3f,11.2f,188f,.82f)
    };
    foreach(var v in spots)
    {
     var go=ValoriaKit.BenchmarkPieceModulated("Valoria · breakthrough · exterior grouped scrub",bush,
      new Vector3(v.x,.04f,v.y),v.w,.55f,Quaternion.Euler(0,v.z,0),new Color(.70f,.78f,.56f,1f));
     if(go==null)continue;go.transform.SetParent(root,true);StripGameplay(go);ExteriorModules++;
    }
   }

   var rock=Resources.Load<GameObject>("Valoria/Rescued/RockTerrainSeamFiller");
   if(rock!=null)
   {
    AddExteriorRock(root,rock,"west roadside stone",new Vector3(-5.05f,.02f,-9.7f),.78f,.34f,26f);
    AddExteriorRock(root,rock,"east roadside stone",new Vector3(5.05f,.02f,-9.8f),.72f,.31f,-19f);
    AddExteriorRock(root,rock,"north-west field stone",new Vector3(-11.8f,.02f,11.0f),.66f,.28f,67f);
    AddExteriorRock(root,rock,"north-east field stone",new Vector3(11.7f,.02f,10.9f),.62f,.27f,-72f);
   }
  }

  static void BermRibbon(Transform root,string name,Vector2[] path,float width,float height,Material mat)
  {
   if(path==null||path.Length<2)return;
   int n=path.Length;var verts=new Vector3[n*3];var cols=new Color[n*3];
   for(int i=0;i<n;i++)
   {
    Vector2 dir=(i==0?path[1]-path[0]:(i==n-1?path[n-1]-path[n-2]:path[i+1]-path[i-1])).normalized;
    Vector2 side=new Vector2(-dir.y,dir.x);
    float taper=Mathf.Sin(Mathf.PI*i/(n-1f));
    float y=-.025f+height*(.55f+.45f*taper);
    verts[i*3+0]=new Vector3(path[i].x-side.x*width,-.035f,path[i].y-side.y*width);
    verts[i*3+1]=new Vector3(path[i].x,y,path[i].y);
    verts[i*3+2]=new Vector3(path[i].x+side.x*width,-.035f,path[i].y+side.y*width);
    cols[i*3+0]=new Color(.43f,.55f,.31f);cols[i*3+1]=new Color(.52f,.60f,.34f);cols[i*3+2]=new Color(.43f,.55f,.31f);
   }
   var tris=new int[(n-1)*12];int t=0;
   for(int i=0;i<n-1;i++){int a=i*3,b=(i+1)*3;
    tris[t++]=a;tris[t++]=b;tris[t++]=a+1;tris[t++]=a+1;tris[t++]=b;tris[t++]=b+1;
    tris[t++]=a+1;tris[t++]=b+1;tris[t++]=a+2;tris[t++]=a+2;tris[t++]=b+1;tris[t++]=b+2;
   }
   MeshObject(root,name,verts,tris,cols,mat);ExteriorModules++;
  }

  static void Mound(Transform root,string name,Vector2 center,Vector2 radius,float height,Material mat)
  {
   const int seg=12;var verts=new Vector3[seg+1];var cols=new Color[seg+1];
   verts[0]=new Vector3(center.x,height-.02f,center.y);cols[0]=new Color(.50f,.60f,.35f);
   for(int i=0;i<seg;i++){float a=Mathf.PI*2*i/seg;
    verts[i+1]=new Vector3(center.x+Mathf.Cos(a)*radius.x,-.035f,center.y+Mathf.Sin(a)*radius.y);
    cols[i+1]=new Color(.44f,.56f,.32f);}
   var tris=new int[seg*3];for(int i=0;i<seg;i++){tris[i*3]=0;tris[i*3+1]=i+1;tris[i*3+2]=((i+1)%seg)+1;}
   MeshObject(root,name,verts,tris,cols,mat);ExteriorModules++;
  }

  static void MeshObject(Transform root,string name,Vector3[] verts,int[] tris,Color[] colors,Material mat)
  {
   var mesh=new Mesh{name="Valoria breakthrough "+name};mesh.vertices=verts;mesh.triangles=tris;mesh.colors=colors;mesh.RecalculateNormals();mesh.RecalculateBounds();
   var go=new GameObject("Valoria · breakthrough · "+name);go.transform.SetParent(root,true);
   go.AddComponent<MeshFilter>().sharedMesh=mesh;go.AddComponent<MeshRenderer>().sharedMaterial=mat;
  }

  static void AddExteriorRock(Transform root,GameObject source,string role,Vector3 p,float footprint,float maxHeight,float yaw)
  {
   var go=ValoriaKit.BenchmarkPiece("Valoria · breakthrough · "+role,source,p,footprint,maxHeight,Quaternion.Euler(0,yaw,0));
   if(go==null)return;go.transform.SetParent(root,true);StripGameplay(go);ExteriorModules++;
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
