using System;
using System.Collections.Generic;
using System.Linq;
using Eldoria.ReferenceClean;
using UnityEngine;
using UnityEngine.Rendering;

namespace Eldoria.World4XCanon
{
 // Explicitly authored geography in final world units, never a compressed atlas.
 // The coastline, river graph, lakes, roads, groves and relief share this domain.
 [DefaultExecutionOrder(1000)]
 public sealed class RegionOneDirectedGeography : MonoBehaviour
 {
  public static readonly Vector2[] Coast = {
   new Vector2(-92,-442),new Vector2(-135,-405),new Vector2(-170,-365),new Vector2(-179,-310),
   new Vector2(-191,-255),new Vector2(-176,-205),new Vector2(-190,-155),new Vector2(-181,-100),
   new Vector2(-192,-48),new Vector2(-182,10),new Vector2(-190,70),new Vector2(-177,125),
   new Vector2(-184,179),new Vector2(-163,226),new Vector2(-165,268),new Vector2(-138,309),
   new Vector2(-122,353),new Vector2(-95,389),new Vector2(-60,431),new Vector2(-31,464),
   new Vector2(18,476),new Vector2(45,451),new Vector2(42,427),new Vector2(73,413),
   new Vector2(71,391),new Vector2(104,375),new Vector2(97,351),new Vector2(132,334),
   new Vector2(125,312),new Vector2(158,289),new Vector2(149,270),new Vector2(176,244),
   new Vector2(165,232),new Vector2(182,220),new Vector2(187,202),new Vector2(170,188),
   new Vector2(167,169),new Vector2(175,150),new Vector2(182,141),new Vector2(176,115),
   new Vector2(164,101),new Vector2(170,84),new Vector2(190,74),new Vector2(194,50),
   new Vector2(180,35),new Vector2(179,15),new Vector2(191,7),new Vector2(187,-17),
   new Vector2(170,-26),new Vector2(168,-46),new Vector2(184,-56),new Vector2(190,-75),
   new Vector2(179,-97),new Vector2(161,-106),new Vector2(167,-124),new Vector2(182,-139),
   new Vector2(177,-160),new Vector2(157,-173),new Vector2(159,-197),new Vector2(174,-209),
   new Vector2(168,-230),new Vector2(148,-245),new Vector2(152,-270),new Vector2(165,-281),
   new Vector2(157,-302),new Vector2(136,-317),new Vector2(140,-343),new Vector2(119,-357),
   new Vector2(119,-378),
   new Vector2(101,-385),new Vector2(92,-409),new Vector2(60,-413),new Vector2(39,-437),
   new Vector2(5,-432),new Vector2(-20,-453),new Vector2(-52,-443)
  };
  public static readonly Vector2[][] Rivers={
   Line(0,438,-12,409,5,379,-17,352,-6,327,-34,307,-30,279,-54,255,-41,235,
        -80,212,-111,193,-132,162,-119,137,-142,111,-133,88,-154,64,-145,37,
        -120,14,-108,-12,-123,-38,-103,-64,-109,-91,-83,-118,-91,-144,
        -75,-171,-94,-199,-82,-225,-105,-252,-93,-278,-61,-300,-67,-323,
        -34,-338,-12,-358,20,-367,40,-387,57,-414),
   Line(-178,167,-158,155,-146,142,-119,137),
   Line(-177,-38,-152,-46,-123,-38),
   Line(-91,-144,-56,-152,-36,-172,-11,-179,10,-201,34,-212,61,-235),
   Line(61,-235,90,-253,115,-262,146,-287),
   Line(-6,327,24,313,34,289,65,276,86,249),
   Line(-41,235,-8,217,9,196,36,183,51,155),
   Line(98,80,75,94,62,113,51,155),
   Line(-61,-300,-36,-278,-18,-252,13,-244,34,-212)
  };
  public static readonly Vector2[][] Roads={
   Line(-134,-370,-119,-320,-139,-270,-127,-215,-151,-166,-139,-110,-157,-65,-147,-13,-164,40,-156,103,-145,150,-153,214,-129,271,-115,321,-75,375,-31,420),
   Line(80,-370,109,-328,104,-286,124,-244,111,-188,139,-146,126,-102,146,-55,136,-8,156,39,140,88,150,137,130,182,145,229,113,271,90,319,53,355,21,405),
   Line(-134,-370,-84,-356,-34,-375,13,-354,80,-370),
   Line(-139,-270,-83,-286,-34,-268,15,-281,55,-262,104,-286),
   Line(-151,-166,-106,-181,-51,-166,0,-183,53,-165,111,-188),
   Line(-139,-110,-92,-101,-48,-119,1,-105,58,-120,126,-102),
   Line(-147,-13,-99,-25,-53,-12,0,-20,52,-5,96,-19,136,-8),
   Line(-164,40,-117,51,-65,33,-35,20),
   Line(48,35,89,43,113,29,156,39),
   Line(-156,103,-107,120,-70,105,-35,124,15,112,58,130,103,112,140,88),
   Line(-153,214,-109,227,-72,210,-28,226,11,212,54,230,94,214,145,229),
   Line(-115,321,-70,338,-31,319,15,336,57,320,90,319),
   Line(-35,-375,-49,-330,-34,-268,-48,-218,-51,-166,-48,-119,-33,-72,-53,-12),
   Line(53,-165,42,-119,58,-120,70,-72,52,-5,48,35),
   Line(-35,124,-51,171,-28,226,-42,267,-31,319)
  };
  // Centre x,z, lateral radius, longitudinal radius. Deliberate bands/openings.
  public static readonly Vector4[] Groves={
   new Vector4(-28,418,10,13),new Vector4(-68,371,15,12),new Vector4(10,370,12,11),
   new Vector4(42,322,15,12),new Vector4(-103,309,17,12),new Vector4(-136,266,15,10),
   new Vector4(-87,267,12,10),new Vector4(96,267,17,11),new Vector4(142,216,13,11),
   new Vector4(-143,206,18,10),new Vector4(11,197,13,12),new Vector4(66,169,15,11),
   new Vector4(-61,165,19,13),new Vector4(-143,121,17,10),new Vector4(131,119,15,10),
   new Vector4(-112,74,17,11),new Vector4(126,49,13,10),new Vector4(-143,6,16,11),
   new Vector4(113,-31,17,11),new Vector4(-144,-76,18,12),new Vector4(10,-105,13,9),
   new Vector4(114,-103,17,13),new Vector4(-115,-139,15,10),new Vector4(-9,-151,18,11),
   new Vector4(82,-166,13,10),new Vector4(-151,-211,19,13),new Vector4(-40,-215,16,10),
   new Vector4(130,-216,13,10),new Vector4(-129,-284,16,12),new Vector4(-32,-289,13,10),
   new Vector4(94,-291,16,11),new Vector4(-73,-337,19,11),new Vector4(40,-327,15,10),
   new Vector4(-113,-376,14,10),new Vector4(1,-395,18,11),new Vector4(81,-371,12,9)
  };
  static Vector2[] Line(params float[] v){var a=new Vector2[v.Length/2];for(int i=0;i<a.Length;i++)a[i]=new Vector2(v[i*2],v[i*2+1]);return a;}
  static float Segment(Vector2 p,Vector2 a,Vector2 b){var d=b-a;return Vector2.Distance(p,a+d*Mathf.Clamp01(Vector2.Dot(p-a,d)/Mathf.Max(.001f,d.sqrMagnitude)));}
  public static float Shore(float x,float z)
  {
   var p=new Vector2(x,z);float d=10000;bool inside=false;
   for(int i=0,j=Coast.Length-1;i<Coast.Length;j=i++)
   {var a=Coast[j];var b=Coast[i];d=Mathf.Min(d,Segment(p,a,b));
    if((a.y>z)!=(b.y>z) && x<(b.x-a.x)*(z-a.y)/(b.y-a.y)+a.x)inside=!inside;}
   return inside?d:-d;
  }
  static readonly Vector2[][] CurvedRivers,CurvedRoads;
  static float ComposeZ(float z){return Mathf.Abs(z)<=130?z:Mathf.Sign(z)*(130+(Mathf.Abs(z)-130)*.6f);}
  static RegionOneDirectedGeography()
  {
   // Author vector control points at reference-like proportions. The actual
   // central gameplay corridor stays fixed; pigment and tree shapes never stretch.
   for(int i=0;i<Coast.Length;i++)Coast[i]=new Vector2(Coast[i].x,ComposeZ(Coast[i].y));
   foreach(var paths in new[]{Rivers,Roads})foreach(var line in paths)for(int i=0;i<line.Length;i++)line[i]=new Vector2(line[i].x,ComposeZ(line[i].y));
   for(int i=0;i<Groves.Length;i++){var g=Groves[i];g.y=ComposeZ(g.y);g.z*=i%3==0?1.35f:.95f;g.w*=i%4==0?1.15f:.82f;Groves[i]=g;}
   CurvedRivers=Smooth(Rivers);CurvedRoads=Smooth(Roads);
  }
  static Vector2[][] Smooth(Vector2[][] input)
  {
   var result=new Vector2[input.Length][];
   for(int k=0;k<input.Length;k++)
   {var line=input[k];var points=new List<Vector2>();
    for(int i=0;i<line.Length-1;i++)
    {var a=line[Mathf.Max(0,i-1)];var b=line[i];var c=line[i+1];var d=line[Mathf.Min(line.Length-1,i+2)];
     for(int j=0;j<5;j++){float t=j/5f;points.Add(.5f*((2*b)+(-a+c)*t+(2*a-5*b+4*c-d)*t*t+(-a+3*b-3*c+d)*t*t*t));}}
    points.Add(line[line.Length-1]);result[k]=points.ToArray();
   }return result;
  }
  static float LinesDistance(Vector2 p,Vector2[][] paths)
  {float distance=3f;foreach(var line in paths)for(int i=1;i<line.Length;i++)
   {var a=line[i-1];var b=line[i];if(p.x<Mathf.Min(a.x,b.x)-3 || p.x>Mathf.Max(a.x,b.x)+3 || p.y<Mathf.Min(a.y,b.y)-3 || p.y>Mathf.Max(a.y,b.y)+3)continue;
    distance=Mathf.Min(distance,Segment(p,a,b));}return distance;
  }
  public static float Water(float x,float z)
  {
   float d=LinesDistance(new Vector2(x,z),CurvedRivers);
   float river=1f-Mathf.SmoothStep(0,1,Mathf.InverseLerp(1.15f,2.4f,d));
   float angleA=Mathf.Atan2((z-ComposeZ(240))/15f,(x+43)/19f);
   float a=Mathf.Sqrt(Mathf.Pow((x+43)/19f,2)+Mathf.Pow((z-ComposeZ(240))/15f,2))/(1+.12f*Mathf.Sin(angleA*5)+.06f*Mathf.Cos(angleA*8));
   float angleB=Mathf.Atan2((z-ComposeZ(-235))/19f,(x-61)/27f);
   float b=Mathf.Sqrt(Mathf.Pow((x-61)/27f,2)+Mathf.Pow((z-ComposeZ(-235))/19f,2))/(1+.13f*Mathf.Sin(angleB*4)+.07f*Mathf.Cos(angleB*7));
   return Mathf.Max(river,1-Mathf.SmoothStep(0,1,Mathf.InverseLerp(.92f,1.05f,Mathf.Min(a,b))));
  }
  public static float Road(float x,float z){return 1-Mathf.SmoothStep(0,1,Mathf.InverseLerp(.5f,1.5f,LinesDistance(new Vector2(x,z),CurvedRoads)));}
  static float GroveMask(float x,float z)
  {float f=0;foreach(var g in Groves){float d=Mathf.Pow((x-g.x)/g.z,2)+Mathf.Pow((z-g.y)/g.w,2);f=Mathf.Max(f,Mathf.Clamp01(1-d));}return f;}
  public static float Height(float x,float z)
  {
   float shore=Shore(x,z);if(shore< -4)return -20;
   float land=Mathf.SmoothStep(0,1,Mathf.InverseLerp(-1.5f,2f,shore));
   float ridge=0;
   foreach(var p in new[]{new Vector3(-146,303,22),new Vector3(151,151,18),new Vector3(-148,-312,19)})
    ridge+=4f*Mathf.Max(0,1-Mathf.Pow((x-p.x)/p.z,2)-Mathf.Pow((z-ComposeZ(p.y))/(p.z*1.5f),2));
   return -20+land*(16+ridge);
  }
  static Texture2D mask;
  static Texture2D Mask()
  {
   if(mask)return mask;
   const int w=512,h=1024;var pixels=new Color32[w*h];
   for(int iy=0;iy<h;iy++)for(int ix=0;ix<w;ix++)
   {
    float x=((ix+.5f)/w-.5f)*500,z=((iy+.5f)/h-.5f)*1040;
    float shore=Shore(x,z),water=0,road=0,forest=0;
    if(shore> -3){water=Water(x,z);road=Road(x,z)*(1-water);forest=GroveMask(x,z)*(1-water);}
    pixels[iy*w+ix]=new Color(Mathf.Clamp01(.5f+shore/32f),water,road,forest);
   }
   mask=new Texture2D(w,h,TextureFormat.RGBA32,true,true){name="Directed coastline water routes grove masks",wrapMode=TextureWrapMode.Clamp,filterMode=FilterMode.Bilinear};
   mask.SetPixels32(pixels);mask.Apply(true,false);return mask;
  }
  public Mesh DirectedTerrain {get;private set;}
  public Texture2D GeographicMask=>mask;
  ValoriaIntegratedWorld host;Vector3 origin;GameObject art;Material ground,forestMaterial;
  readonly List<Mesh> meshes=new List<Mesh>();readonly List<Renderer> replaced=new List<Renderer>();
  readonly List<Renderer> prairie=new List<Renderer>();readonly List<Material[]> prairieMaterials=new List<Material[]>();
  Renderer retainedNear,innerForest;MeshFilter breachFilter;Mesh originalScar,directedScar;bool before,prairieDirected;
  public static RegionOneDirectedGeography Install(Transform parent,ValoriaIntegratedWorld host)
  {
   var shader=Resources.Load<Shader>("World4X/RegionOneDirectedGround");
   var trees=Resources.Load<Texture2D>("World4X/RegionOnePaintedForest");
   if(!shader||!trees){Debug.LogError("DIRECTED_GEOGRAPHY_ASSETS_MISSING");return null;}
   var owner=parent.gameObject.AddComponent<RegionOneDirectedGeography>();owner.host=host;owner.origin=host.WorldHome;
   owner.Build(shader,trees);return owner;
  }
  void Build(Shader shader,Texture2D trees)
  {
   art=new GameObject("Region1 expressly directed geographic layers");art.transform.SetParent(transform,true);
   ground=new Material(shader){name="Region1 authored land water paths forest ground"};
   ground.SetTexture("_Mask",Mask());ground.SetTexture("_Pigment",Resources.Load<Texture2D>("World4X/RegionOnePaintedLandcover"));
   ground.SetVector("_Origin",new Vector4(origin.x,origin.z,0,0));
   forestMaterial=new Material(Shader.Find("Eldoria/World4X/LocalCanopy")){name="Region1 directed grove art"};forestMaterial.mainTexture=trees;forestMaterial.SetColor("_Tint",new Color(.76f,.91f,.94f,1));
   foreach(var r in host.WorldRoot.GetComponentsInChildren<Renderer>(true))
   {
    if(r.transform.IsChildOf(art.transform))continue;
    if(r.name=="Region1 · one coherent terrain mesh" || r.name=="Region1 · retained local forest canopy" ||
       r.name.StartsWith("Region1 2.5D forest band",StringComparison.Ordinal) ||
       r.name.StartsWith("Region1 · mountain facets",StringComparison.Ordinal))replaced.Add(r);
    if(r is MeshRenderer && r.sharedMaterials.Any(m=>m&&m.shader&&m.shader.name=="Eldoria/World4X/WorldAlignedPaintedGround") && r.name!="Region1 · one coherent terrain mesh")
    {prairie.Add(r);prairieMaterials.Add(r.sharedMaterials);}
   }
   BuildTerrain();RegionOneGeographicDressing.Install(art.transform,origin);BuildGroves(false);innerForest=BuildGroves(true);BuildRetainedNear();BuildHeroBreach();
   Debug.Log("REGION1_DIRECTED_GEOGRAPHY polygon="+Coast.Length+" river_layers="+Rivers.Length+" roads="+Roads.Length+" groves="+Groves.Length+" replaced="+replaced.Count);
   Refresh();
  }
  void BuildTerrain()
  {
   const int nx=160,nz=300;var v=new Vector3[(nx+1)*(nz+1)];var uv=new Vector2[v.Length];var t=new int[nx*nz*6];
   for(int z=0;z<=nz;z++)for(int x=0;x<=nx;x++)
   {float px=(x/(float)nx-.5f)*500,pz=(z/(float)nz-.5f)*1040;int i=z*(nx+1)+x;v[i]=origin+new Vector3(px,Height(px,pz),pz);uv[i]=new Vector2(x/(float)nx,z/(float)nz);}
   int n=0;for(int z=0;z<nz;z++)for(int x=0;x<nx;x++){int i=z*(nx+1)+x;t[n++]=i;t[n++]=i+nx+1;t[n++]=i+1;t[n++]=i+1;t[n++]=i+nx+1;t[n++]=i+nx+2;}
   MeshObject("Directed terrain continuous coast relief",v,uv,null,t,ground);
   MeshObject("Directed ocean continuation",new[]{origin+new Vector3(-1000,-20.1f,-900),origin+new Vector3(1000,-20.1f,-900),origin+new Vector3(1000,-20.1f,900),origin+new Vector3(-1000,-20.1f,900)},new Vector2[4],null,new[]{0,2,1,0,3,2},ground);
  }
  Renderer MeshObject(string name,Vector3[] v,Vector2[] uv,Vector2[] offsets,int[] t,Material mat)
  {
   var mesh=new Mesh{name=name,vertices=v,uv=uv,triangles=t};if(offsets!=null)mesh.uv2=offsets;
   if(name=="Directed terrain continuous coast relief")DirectedTerrain=mesh;
   mesh.RecalculateNormals();mesh.RecalculateBounds();if(offsets!=null){var b=mesh.bounds;b.Expand(40);mesh.bounds=b;}meshes.Add(mesh);
   var go=new GameObject(name);go.transform.SetParent(art.transform,true);go.AddComponent<MeshFilter>().sharedMesh=mesh;
   var r=go.AddComponent<MeshRenderer>();r.sharedMaterial=mat;r.shadowCastingMode=ShadowCastingMode.Off;r.receiveShadows=false;return r;
  }
  static void Quad(List<Vector3> v,List<Vector2> uv,List<Vector2> offsets,List<int> tris,Vector3 foot,float width,float height,int species)
  {
   int n=v.Count;for(int k=0;k<4;k++){bool right=k==1||k==2,top=k>=2;v.Add(foot);offsets.Add(new Vector2((right?.5f:-.5f)*width,top?height:-height*.04f));uv.Add(new Vector2((species+(right?.995f:.005f))/3f,top?1:0));}
   tris.AddRange(new[]{n,n+1,n+2,n,n+2,n+3});
  }
  Renderer BuildGroves(bool inner)
  {
   var v=new List<Vector3>();var uv=new List<Vector2>();var offsets=new List<Vector2>();var tris=new List<int>();
   for(int i=0;i<Groves.Length;i++)
   {var g=Groves[i];if((new Vector2(g.x,g.y).magnitude<135)!=inner)continue;
    int count=9+(i*7)%17;for(int k=0;k<count;k++)
    {float a=k*2.399963f+i*.7f,r=Mathf.Sqrt((k+.5f)/count)*.93f;float x=g.x+Mathf.Cos(a)*g.z*r,z=g.y+Mathf.Sin(a)*g.w*r;
     if(Shore(x,z)<7 || Water(x,z)>.05f || Road(x,z)>.3f)continue;
     float h=11.5f+(i*7+k*3)%5*1.15f;Quad(v,uv,offsets,tris,origin+new Vector3(x,Height(x,z)+.06f,z),h*.72f,h,(i+k)%3);
    }
   }
   if(!inner)
   {
    var rng=new System.Random(19211);
    for(int k=0;k<420;k++)
    {float x=-185+(float)rng.NextDouble()*370,z=-320+(float)rng.NextDouble()*650;
     if(new Vector2(x,z).magnitude<140||Shore(x,z)<10||Water(x,z)>.02f||Road(x,z)>.1f||Mathf.PerlinNoise(x*.028f+7,z*.028f+11)<.54f)continue;
     float h=3.5f+(float)rng.NextDouble()*4.5f;Quad(v,uv,offsets,tris,origin+new Vector3(x,Height(x,z)+.08f,z),h*.58f,h,k%2);
    }
   }
   return MeshObject(inner?"Directed inner grove silhouettes":"Directed outer grove silhouettes",v.ToArray(),uv.ToArray(),offsets.ToArray(),tris.ToArray(),forestMaterial);
  }
  void BuildRetainedNear()
  {
   var v=new List<Vector3>();var uv=new List<Vector2>();var offsets=new List<Vector2>();var tris=new List<int>();int i=0;
   foreach(var r in host.WorldRoot.GetComponentsInChildren<MeshRenderer>(true).Where(r=>r.name.StartsWith("Forest retained Valoria pine",StringComparison.Ordinal)||r.name.StartsWith("Existing conifer canopy / forest ecological lobe ",StringComparison.Ordinal)||r.name=="Forest understory / young conifer"))
   {var b=r.bounds;float h=Mathf.Clamp(b.size.y,1,18),w=Mathf.Clamp(Mathf.Max(b.size.x,b.size.z),h*.55f,h*1.1f);Quad(v,uv,offsets,tris,new Vector3(b.center.x,b.min.y+.03f,b.center.z),w,h,i++%3);}
   retainedNear=MeshObject("Directed retained actual near trees",v.ToArray(),uv.ToArray(),offsets.ToArray(),tris.ToArray(),forestMaterial);
  }
  void BuildHeroBreach()
  {
   breachFilter=transform.GetComponentsInChildren<MeshFilter>(true).FirstOrDefault(f=>f.name=="Region1 · authored Breach pigment at actual world anchor");
   if(!breachFilter)return;originalScar=breachFilter.sharedMesh;directedScar=Instantiate(originalScar);directedScar.name="Directed central geological scar";
   var v=directedScar.vertices;var colors=directedScar.colors;
   for(int i=0;i<v.Length;i++)
   {v[i].x*=2.1f;v[i].z*=2.1f;float x=8+v[i].x,z=61+v[i].z;v[i].y=Height(x,z)+.12f;
    float approach=Mathf.SmoothStep(0,1,Mathf.InverseLerp(32,48,new Vector2(x,z).magnitude));colors[i]=new Color(1,1,1,approach*(1-Water(x,z)));}
   directedScar.vertices=v;directedScar.colors=colors;directedScar.RecalculateBounds();meshes.Add(directedScar);
  }
  public void SetBefore(bool value){before=value;Refresh();}
  public void Refresh()
  {
   if(!art||!host||!host.WorldCamera)return;
   float zoom=host.WorldCamera.orthographicSize;art.SetActive(!before);
   if(breachFilter)breachFilter.sharedMesh=before?originalScar:directedScar;
   if(!before)
   {foreach(var r in replaced)if(r)r.enabled=false;
    if(!prairieDirected){for(int i=0;i<prairie.Count;i++)if(prairie[i]){var mats=new Material[prairieMaterials[i].Length];for(int k=0;k<mats.Length;k++)mats[k]=ground;prairie[i].sharedMaterials=mats;}prairieDirected=true;}
    if(retainedNear)retainedNear.enabled=zoom<152;
    if(innerForest)innerForest.enabled=zoom>=152;
    ground.SetFloat("_Near",1-Mathf.SmoothStep(0,1,Mathf.InverseLerp(80,152,zoom)));
   }
   else
   {foreach(var r in replaced)if(r)r.enabled=r.name=="Region1 · retained local forest canopy"?zoom<152:true;
    if(prairieDirected){for(int i=0;i<prairie.Count;i++)if(prairie[i])prairie[i].sharedMaterials=prairieMaterials[i];prairieDirected=false;}}
  }
  void LateUpdate(){Refresh();}
  void OnDestroy(){if(breachFilter)breachFilter.sharedMesh=originalScar;for(int i=0;i<prairie.Count;i++)if(prairie[i])prairie[i].sharedMaterials=prairieMaterials[i];if(art)Destroy(art);foreach(var m in meshes)if(m)Destroy(m);if(ground)Destroy(ground);if(forestMaterial)Destroy(forestMaterial);}
 }
}
