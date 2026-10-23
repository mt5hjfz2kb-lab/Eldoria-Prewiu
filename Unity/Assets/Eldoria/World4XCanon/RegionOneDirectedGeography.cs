using System;
using System.Collections.Generic;
using System.Linq;
using Eldoria.ReferenceClean;
using UnityEngine;
using UnityEngine.Rendering;
namespace Eldoria.World4XCanon
{
 // Clean authorial scene. All coordinates below are final metres in one X/Z space.
 // No ComposeZ, inherited terrain, duplicated near ground, or zoom-dependent geography.
 [DefaultExecutionOrder(1000)]
 public sealed class RegionOneDirectedGeography : MonoBehaviour
 {
  public const float DomainWidth=1820,DomainDepth=1560;
  static Vector2[] Line(params float[] v){var a=new Vector2[v.Length/2];for(int i=0;i<a.Length;i++)a[i]=new Vector2(v[i*2],v[i*2+1]);return a;}
  public static readonly Vector2[] Coast=Line(-900,-760,-900,760,110,760,120,615,105,560,145,525,133,480,174,449,163,419,197,390,177,362,207,328,194,303,224,275,204,246,234,213,216,184,243,155,225,126,249,94,228,65,254,34,233,2,258,-28,239,-59,260,-88,238,-120,254,-147,230,-175,247,-208,220,-237,235,-264,209,-289,219,-320,188,-342,196,-371,161,-393,133,-420,104,-414,76,-443,43,-433,12,-461,-26,-447,-59,-476,-97,-462,-133,-493,-174,-478,-217,-509,-261,-492,-308,-523,-358,-509,-410,-542,-467,-528,-531,-556,-603,-541,-682,-572,-774,-558,-900,-589);
  // Channel hierarchy: 0 is trunk; tributaries start at upland springs and join explicitly.
  public static readonly Vector2[][] Rivers={
   Line(5,585,-12,540,8,499,-12,458,7,420,-18,384,-12,349,-40,312,-34,280,-56,250,-48,222,-79,198,-107,176,-117,145,-104,115,-124,83,-116,54,-130,28,-104,0,-116,-29,-101,-59,-111,-89,-87,-119,-98,-150,-80,-180,-95,-212,-81,-242,-98,-269,-72,-300,-76,-328,-42,-350,-23,-380,12,-399,40,-425,65,-451,91,-481),
   Line(-390,405,-367,362,-381,317,-344,276,-327,232,-284,196,-259,152,-217,128,-197,86,-158,53,-130,28),
   Line(-541,191,-493,177,-453,151,-412,164,-371,143,-328,158,-284,196),
   Line(153,400,121,380,103,347,76,330,63,295,30,280,-5,250,-48,222),
   Line(189,120,155,141,126,164,91,182,65,206,27,211,-8,228,-48,222),
   Line(176,-61,147,-86,133,-117,104,-139,86,-169,60,-184,34,-210,-8,-224,-43,-244,-81,-242),
   Line(-98,-150,-59,-159,-30,-180,2,-187,34,-210),
   Line(34,-210,67,-237,96,-259,119,-290,151,-313,173,-342,207,-374),
   Line(-484,-131,-447,-151,-401,-143,-363,-172,-318,-185,-286,-214,-243,-230,-211,-265,-168,-281,-134,-318,-76,-328),
   Line(175,-49,132,-22,99,-35,71,-21,44,-27,18,-17,-8,-28,-33,-17,-63,-29,-86,-23,-116,-29)
  };
  public static readonly Vector2[][] Roads={
   Line(-40,-550,-61,-463,-45,-391,-57,-316,-35,-247,-52,-176,-29,-103,0,0,23,91,4,176,28,253,11,338,38,422,17,511,38,603),
   Line(155,-350,139,-278,157,-208,142,-139,160,-62,145,9,165,79,149,149,170,227,152,303,174,367),
   Line(-325,-488,-301,-413,-324,-341,-302,-266,-326,-192,-301,-116,-320,-43,-298,31,-319,103,-295,178,-310,230,-289,302,-310,380,-291,463),
   Line(-550,285,-476,303,-405,282,-337,309,-265,289,-196,309,-126,285,-55,305,11,338,84,317,152,303),
   Line(-540,95,-469,111,-395,91,-319,103,-247,81,-174,104,-98,83,-35,103,23,91,94,112,165,79),
   Line(-529,-94,-461,-79,-388,-103,-320,-43,-245,-63,-177,-84,-104,-73,-29,-103,45,-86,117,-108,160,-62),
   Line(-511,-275,-439,-294,-367,-271,-302,-266,-228,-289,-156,-269,-82,-290,-35,-247,39,-269,100,-245,157,-208),
   Line(-469,-433,-399,-411,-325,-432,-252,-410,-180,-433,-107,-414,-45,-391,26,-414,99,-389,155,-350)
  };
  // Forest lobes intentionally follow shoulders, shelter and river corridors; gaps are designed.
  public static readonly Vector4[] Groves={
   new Vector4(-32,550,22,16),new Vector4(50,493,19,14),new Vector4(-72,477,25,16),new Vector4(-135,425,29,17),new Vector4(18,409,22,15),new Vector4(114,366,24,15),
   new Vector4(-85,353,24,18),new Vector4(-204,363,30,18),new Vector4(-270,421,28,17),new Vector4(-360,449,35,21),new Vector4(-428,365,34,20),new Vector4(-465,337,27,18),
   new Vector4(-143,286,22,16),new Vector4(79,269,22,16),new Vector4(-310,230,31,19),new Vector4(-351,217,25,17),new Vector4(-473,239,37,20),new Vector4(-515,224,27,17),
   new Vector4(-208,153,23,15),new Vector4(-181,173,18,13),new Vector4(128,213,24,16),new Vector4(-52,167,23,14),new Vector4(-390,72,32,18),new Vector4(-437,57,29,20),
   new Vector4(-147,107,24,17),new Vector4(168,57,26,16),new Vector4(-283,32,33,17),new Vector4(-242,13,24,19),new Vector4(-514,43,31,21),
   new Vector4(-174,-25,26,18),new Vector4(175,-49,26,17),new Vector4(-366,-56,32,18),new Vector4(-401,-79,28,19),new Vector4(-502,-137,30,18),
   new Vector4(21,-116,21,16),new Vector4(111,-124,24,17),new Vector4(-211,-143,27,19),new Vector4(-250,-164,24,19),new Vector4(-390,-216,32,17),
   new Vector4(-135,-216,24,17),new Vector4(151,-213,27,16),new Vector4(-29,-275,24,17),new Vector4(-278,-298,31,20),new Vector4(-315,-315,27,17),
   new Vector4(-482,-328,35,21),new Vector4(-441,-347,27,18),new Vector4(92,-336,22,17),new Vector4(-132,-363,26,17),new Vector4(-174,-380,23,15),
   new Vector4(-34,-425,20,13),new Vector4(-261,-435,28,18),new Vector4(-351,-458,30,18),new Vector4(-553,430,30,22),new Vector4(-544,-455,34,22)
  };
  // Meadow islands are authored independently of canopy density; open ground can be green.
  static readonly Vector4[] Meadows={
   new Vector4(-30,-15,18,12),new Vector4(27,-8,16,20),new Vector4(-20,30,20,13),new Vector4(17,35,12,10),new Vector4(8,-48,12,11),new Vector4(38,-42,18,10),
   new Vector4(-61,505,54,35),new Vector4(78,447,42,32),new Vector4(-208,473,60,34),new Vector4(-378,511,61,36),
   new Vector4(-106,381,52,36),new Vector4(-287,352,58,33),new Vector4(-475,395,55,37),new Vector4(128,319,44,30),
   new Vector4(-78,287,41,31),new Vector4(-228,260,55,34),new Vector4(-427,267,56,33),new Vector4(68,178,46,30),
   new Vector4(-176,181,48,34),new Vector4(-353,176,59,36),new Vector4(-500,121,56,40),new Vector4(145,79,44,32),
   new Vector4(-229,72,48,30),new Vector4(-429,20,55,33),new Vector4(-133,15,40,31),
   new Vector4(-291,-60,55,32),new Vector4(-491,-75,59,35),new Vector4(128,-88,47,29),new Vector4(-162,-116,50,30),
   new Vector4(-354,-153,60,31),new Vector4(-484,-226,55,37),new Vector4(30,-160,45,28),new Vector4(-192,-218,52,31),
   new Vector4(-350,-324,61,37),new Vector4(-121,-300,48,32),new Vector4(130,-290,41,30),new Vector4(-33,-365,45,30),
   new Vector4(-244,-383,53,33),new Vector4(-447,-426,59,35),new Vector4(-152,-455,47,29)
  };
  static float Meadow(float x,float z)
  {float cover=0;foreach(var g in Meadows){float dx=(x-g.x)/g.z,dz=(z-g.y)/g.w;if(Mathf.Abs(dx)>1.2f||Mathf.Abs(dz)>1.2f)continue;float edge=dx*dx+dz*dz+(Mathf.PerlinNoise(x*.12f+19,z*.12f+7)-.5f)*.38f;cover=Mathf.Max(cover,.58f*(1-Mathf.SmoothStep(0,1,Mathf.InverseLerp(.55f,1.1f,edge))));}return cover;}
  static readonly Vector4[] Hills={new Vector4(-150,435,48,22),new Vector4(-350,310,58,18),new Vector4(171,286,38,16),new Vector4(-270,9,48,19),new Vector4(-433,-168,65,22),new Vector4(-155,-352,42,21),new Vector4(139,-303,32,16),new Vector4(-387,-460,50,23)};
  static readonly Vector2[][] Channels=Smooth(Rivers),Routes=Smooth(Roads);
  static Vector2[][] Smooth(Vector2[][] paths)
  {return paths.Select(line=>{var pts=new List<Vector2>();for(int i=0;i<line.Length-1;i++){var a=line[Mathf.Max(0,i-1)];var b=line[i];var c=line[i+1];var d=line[Mathf.Min(line.Length-1,i+2)];for(int j=0;j<5;j++){float t=j/5f;pts.Add(.5f*(2*b+(-a+c)*t+(2*a-5*b+4*c-d)*t*t+(-a+3*b-3*c+d)*t*t*t));}}pts.Add(line.Last());return pts.ToArray();}).ToArray();}
  static float Segment(Vector2 p,Vector2 a,Vector2 b){var d=b-a;return Vector2.Distance(p,a+d*Mathf.Clamp01(Vector2.Dot(p-a,d)/Mathf.Max(.001f,d.sqrMagnitude)));}
  public static float Shore(float x,float z)
  {var p=new Vector2(x,z);float dist=10000;bool inside=false;for(int i=0,j=Coast.Length-1;i<Coast.Length;j=i++){var a=Coast[j];var b=Coast[i];dist=Mathf.Min(dist,Segment(p,a,b));if((a.y>z)!=(b.y>z)&&x<(b.x-a.x)*(z-a.y)/(b.y-a.y)+a.x)inside=!inside;}return inside?dist:-dist;}
  static float ChannelEdge(float x,float z)
  {var p=new Vector2(x,z);float edge=12;for(int k=0;k<Channels.Length;k++){var line=Channels[k];for(int i=1;i<line.Length;i++){var a=line[i-1];var b=line[i];if(x<Mathf.Min(a.x,b.x)-14||x>Mathf.Max(a.x,b.x)+14||z<Mathf.Min(a.y,b.y)-14||z>Mathf.Max(a.y,b.y)+14)continue;float t=i/(float)(line.Length-1);float width=k==0?Mathf.Lerp(.65f,2.1f,t):Mathf.Lerp(.20f,k==7?2f:1.35f,Mathf.Sqrt(t));edge=Mathf.Min(edge,Segment(p,a,b)-width);}}
   float angleA=Mathf.Atan2((z-232)/18,(x+49)/24);
   float lakeA=(Mathf.Sqrt(Mathf.Pow((x+49)/24,2)+Mathf.Pow((z-232)/18,2))-(1+.14f*Mathf.Sin(angleA*5)+.08f*Mathf.Cos(angleA*8)))*18;
   float angleB=Mathf.Atan2((z+239)/22,(x-67)/29);
   float lakeB=(Mathf.Sqrt(Mathf.Pow((x-67)/29,2)+Mathf.Pow((z+239)/22,2))-(1+.16f*Mathf.Sin(angleB*4)+.07f*Mathf.Cos(angleB*7)))*22;
   return Mathf.Min(edge,Mathf.Min(lakeA,lakeB));}
  public static float Water(float x,float z)=>1-Mathf.SmoothStep(0,1,Mathf.InverseLerp(-.6f,1.3f,ChannelEdge(x,z)));
  public static float RoadDistance(float x,float z)
  {var p=new Vector2(x,z);float distance=4;foreach(var line in Routes)for(int i=1;i<line.Length;i++){var a=line[i-1];var b=line[i];if(x<Mathf.Min(a.x,b.x)-4||x>Mathf.Max(a.x,b.x)+4||z<Mathf.Min(a.y,b.y)-4||z>Mathf.Max(a.y,b.y)+4)continue;distance=Mathf.Min(distance,Segment(p,a,b));}return distance;}
  public static float Road(float x,float z)=>1-Mathf.SmoothStep(0,1,Mathf.InverseLerp(.45f,1.65f,RoadDistance(x,z)));
  public static float Forest(float x,float z)
  {float f=0;foreach(var g in Groves){float r=Mathf.Pow((x-g.x)/g.z,2)+Mathf.Pow((z-g.y)/g.w,2);f=Mathf.Max(f,Mathf.Clamp01(1-r));}return f;}
  public static float Height(float x,float z)
  {
   float shore=Shore(x,z);if(shore< -5)return -18;
   float h=-4;foreach(var p in Hills){float r=Mathf.Pow((x-p.x)/p.z,2)+Mathf.Pow((z-p.y)/(p.z*.7f),2);h+=p.w*Mathf.Exp(-r*2.3f);float shoulder=Mathf.Pow((x-p.x-p.z*.55f)/(p.z*.7f),2)+Mathf.Pow((z-p.y-p.z*.15f)/(p.z*.5f),2);h+=p.w*.43f*Mathf.Exp(-shoulder*2);}
   // Keep all original central interactions on their established datum.
   float guard=Mathf.SmoothStep(0,1,Mathf.InverseLerp(130,185,new Vector2(x,z).magnitude));h=Mathf.Lerp(-4,h,guard);
   h-=1.8f*(1-Mathf.SmoothStep(0,1,Mathf.InverseLerp(-2,9,ChannelEdge(x,z))));
   return Mathf.Lerp(-18,h,Mathf.SmoothStep(0,1,Mathf.InverseLerp(-3,2,shore)));
  }
  static Texture2D mask;static bool contactGroundPainted;
  static Texture2D Mask()
  {
   if(mask)return mask;const int w=1536,h=1280;var pixels=new Color32[w*h];
   for(int iy=0;iy<h;iy++)for(int ix=0;ix<w;ix++)
   {float x=((ix+.5f)/w-.5f)*DomainWidth,z=((iy+.5f)/h-.5f)*DomainDepth,s=Shore(x,z);float water=0,road=0,forest=0;
    if(s> -4){water=Water(x,z);road=Road(x,z)*(1-water);forest=Mathf.Max(Forest(x,z),Meadow(x,z))*(1-water)*(1-road*.9f);}
    pixels[iy*w+ix]=new Color(Mathf.Clamp01(.5f+s/32),water,road,forest);}
   mask=new Texture2D(w,h,TextureFormat.RGBA32,true,true){name="Clean authored semantic masks",wrapMode=TextureWrapMode.Clamp,filterMode=FilterMode.Bilinear};mask.SetPixels32(pixels);mask.Apply(true,false);return mask;
  }
  ValoriaIntegratedWorld host;Vector3 origin;Material ground,trees,scarMaterial,bridgeMaterial;bool blockout;
  readonly List<Mesh> meshes=new List<Mesh>();readonly List<Renderer> foliage=new List<Renderer>();Renderer scar;
  public Mesh DirectedTerrain{get;private set;}
  public Texture2D GeographicMask=>mask;
  public static RegionOneDirectedGeography Install(Transform parent,ValoriaIntegratedWorld host)
  {var c=parent.gameObject.AddComponent<RegionOneDirectedGeography>();c.host=host;c.origin=host.WorldHome;c.Build();return c;}
  void Build()
  {
   ground=new Material(Resources.Load<Shader>("World4X/RegionOneDirectedGround")){name="Clean single terrain surface"};ground.SetTexture("_Mask",Mask());ground.SetTexture("_Pigment",Resources.Load<Texture2D>("World4X/RegionOnePaintedLandcover"));ground.SetVector("_Origin",new Vector4(origin.x,origin.z,0,0));ground.SetVector("_Domain",new Vector4(DomainWidth,DomainDepth,0,0));
   trees=new Material(Shader.Find("Eldoria/World4X/LocalCanopy")){name="Clean shared woodland"};trees.mainTexture=Resources.Load<Texture2D>("World4X/RegionOnePaintedForest");trees.SetColor("_Tint",new Color(.83f,.93f,.86f,1));
   const int nx=320,nz=280;var v=new Vector3[(nx+1)*(nz+1)];var uv=new Vector2[v.Length];var indices=new int[nx*nz*6];
   for(int z=0;z<=nz;z++)for(int x=0;x<=nx;x++){float px=(x/(float)nx-.5f)*DomainWidth,pz=(z/(float)nz-.5f)*DomainDepth;int i=z*(nx+1)+x;v[i]=origin+new Vector3(px,Height(px,pz),pz);uv[i]=new Vector2(x/(float)nx,z/(float)nz);}
   int n=0;for(int z=0;z<nz;z++)for(int x=0;x<nx;x++){int i=z*(nx+1)+x;indices[n++]=i;indices[n++]=i+nx+1;indices[n++]=i+1;indices[n++]=i+1;indices[n++]=i+nx+1;indices[n++]=i+nx+2;}
   var r=Create("Clean terrain — sole geographic surface",v,uv,null,indices,ground);DirectedTerrain=r.GetComponent<MeshFilter>().sharedMesh;
   BuildForests();PaintRetainedGround();BuildBreach();BuildBridge();Refresh();
  }
  void PaintRetainedGround()
  {
   if(contactGroundPainted)return;var texture=Mask();var pixels=texture.GetPixels32();int w=texture.width,h=texture.height;
   // Paint ecological contact directly into the SAME semantic surface, using actual retained roots.
   foreach(var renderer in foliage)
   {
    var mesh=renderer.GetComponent<MeshFilter>().sharedMesh;var vertices=mesh.vertices;var offsets=mesh.uv2;
    for(int point=0;point<vertices.Length;point+=4)
    {
    float height=offsets[point+2].y;float x=vertices[point].x-origin.x+height*.18f,z=vertices[point].z-origin.z-height*.12f;
    float radius=Mathf.Clamp(Mathf.Abs(offsets[point].x)*1.5f,2,7);
    int x0=Mathf.Max(0,Mathf.FloorToInt((x-radius)/DomainWidth*w+w*.5f)),x1=Mathf.Min(w-1,Mathf.CeilToInt((x+radius)/DomainWidth*w+w*.5f));
    int z0=Mathf.Max(0,Mathf.FloorToInt((z-radius)/DomainDepth*h+h*.5f)),z1=Mathf.Min(h-1,Mathf.CeilToInt((z+radius)/DomainDepth*h+h*.5f));
    for(int iz=z0;iz<=z1;iz++)for(int ix=x0;ix<=x1;ix++)
    {float px=((ix+.5f)/w-.5f)*DomainWidth,pz=((iz+.5f)/h-.5f)*DomainDepth;float d=Mathf.Pow((px-x)/radius,2)+Mathf.Pow((pz-z)/radius,2);int k=iz*w+ix;var c=pixels[k];float f=Mathf.Clamp01(1-d)*.9f*(1-c.g/255f)*(1-c.b/255f);c.a=(byte)Mathf.Max(c.a,Mathf.RoundToInt(f*255));pixels[k]=c;}
   }
   }
   texture.SetPixels32(pixels);texture.Apply(true,false);contactGroundPainted=true;
  }
  Renderer Create(string name,Vector3[] v,Vector2[] uv,Vector2[] offsets,int[] t,Material mat,Transform parent=null)
  {
   var mesh=new Mesh{name=name,indexFormat=v.Length>65535?IndexFormat.UInt32:IndexFormat.UInt16};mesh.vertices=v;mesh.uv=uv;if(offsets!=null)mesh.uv2=offsets;mesh.triangles=t;mesh.RecalculateNormals();mesh.RecalculateBounds();if(offsets!=null){var b=mesh.bounds;b.Expand(40);mesh.bounds=b;}meshes.Add(mesh);
   var go=new GameObject(name);go.transform.SetParent(parent?parent:transform,true);go.AddComponent<MeshFilter>().sharedMesh=mesh;var r=go.AddComponent<MeshRenderer>();r.sharedMaterial=mat;r.shadowCastingMode=ShadowCastingMode.Off;r.receiveShadows=false;return r;
  }
  public static bool IsTreeDonor(Renderer r)=>IsTreeName(r.name);
  public static bool IsTreeName(string name)=>name.StartsWith("Forest retained Valoria pine",StringComparison.Ordinal)||name.StartsWith("Zone1 retained pine ",StringComparison.Ordinal)||name.StartsWith("Zone1 woodland edge / retained pine ",StringComparison.Ordinal)||name.StartsWith("Existing conifer canopy / forest ecological lobe ",StringComparison.Ordinal)||name=="Forest understory / young conifer";
  static void Tree(List<Vector3> v,List<Vector2> uv,List<Vector2> offsets,List<int> t,Vector3 p,float h,float w,int species)
  {int n=v.Count;for(int k=0;k<4;k++){bool right=k==1||k==2,top=k>=2;v.Add(p);uv.Add(new Vector2((species+(right?.995f:.005f))/3,top?1:0));offsets.Add(new Vector2((right?.5f:-.5f)*w,top?h:-h*.04f));}t.AddRange(new[]{n,n+1,n+2,n,n+2,n+3});}
  void BuildForests()
  {
   // Reuse measured near-tree positions once, not an old generated forest layer.
   var v=new List<Vector3>();var uv=new List<Vector2>();var off=new List<Vector2>();var t=new List<int>();int index=0;
   foreach(var r in host.WorldRoot.GetComponentsInChildren<MeshRenderer>(true).Where(IsTreeDonor).OrderBy(r=>r.transform.position.x).ThenBy(r=>r.transform.position.z))
   {var b=r.bounds;float h=Mathf.Clamp(b.size.y,1,18),w=Mathf.Clamp(Mathf.Max(b.size.x,b.size.z),h*.55f,h*1.1f);Tree(v,uv,off,t,new Vector3(b.center.x,origin.y+Height(b.center.x-origin.x,b.center.z-origin.z)+.07f,b.center.z),h,w,index++%3);}
   foliage.Add(Create("Clean retained tree coordinates",v.ToArray(),uv.ToArray(),off.ToArray(),t.ToArray(),trees));v.Clear();uv.Clear();off.Clear();t.Clear();
   var rng=new System.Random(231023);
   for(int i=0;i<Groves.Length;i++)
   {var g=Groves[i];int count=Mathf.RoundToInt(g.z*g.w*.055f);var accepted=new List<Vector2>();
    for(int k=0;k<count*6&&accepted.Count<count;k++)
    {float x=g.x+((float)rng.NextDouble()*2-1)*g.z,z=g.y+((float)rng.NextDouble()*2-1)*g.w;var p=new Vector2(x,z);float f=Forest(x,z);
     if(p.magnitude<130||f<.08f||rng.NextDouble()>Mathf.Sqrt(f)||Shore(x,z)<9||Water(x,z)>.03f||Road(x,z)>.15f||accepted.Any(a=>(a-p).sqrMagnitude<13))continue;
     accepted.Add(p);float h=7.5f+(float)rng.NextDouble()*8;Tree(v,uv,off,t,origin+new Vector3(x,Height(x,z)+.07f,z),h,h*.64f,(i+k)%3);}
   }
   foliage.Add(Create("Clean authored woodland masses",v.ToArray(),uv.ToArray(),off.ToArray(),t.ToArray(),trees));
  }
  void BuildBreach()
  {
   // Author a branching geological fault, not a zoom-scaled slab of purple rock.
   var parent=new GameObject("Region1 · grounded geologic Breach (not a portal)").transform;parent.SetParent(transform,true);
   scarMaterial=new Material(Shader.Find("Sprites/Default")){name="Clean grounded branching fault"};scarMaterial.mainTexture=Texture2D.whiteTexture;scarMaterial.renderQueue=3001;
   var v=new List<Vector3>();var uv=new List<Vector2>();var colors=new List<Color>();var indices=new List<int>();
   var faults=new[]{
    Line(0,0,-7,12,-3,23,-11,32,-8,43,-17,55,-14,71),
    Line(0,0,10,8,14,18,27,23,30,34,45,41,49,53),
    Line(0,0,15,-5,25,-2,37,-13,49,-12,60,-25,75,-23),
    Line(0,0,-13,3,-20,-6,-32,-4,-42,-15,-54,-12,-69,-20),
    Line(0,0,4,-13,-3,-23,5,-34,1,-44,10,-58),
    Line(-11,32,-25,35,-33,46,-47,43,-59,53),
    Line(27,23,39,17,51,23,64,18,76,27),
    Line(-32,-4,-35,9,-48,15,-52,29),
    Line(37,-13,37,-27,49,-35,46,-46),
    Line(-3,23,8,34,5,47,16,58),
    Line(-20,-6,-24,-19,-35,-25,-32,-38)
   };
   int faultIndex=0;
   foreach(var path in faults)
   {
    var points=new List<Vector2>();
    for(int segment=1;segment<path.Length;segment++)
    {
     var a=path[segment-1];var b=path[segment];var dir=(b-a).normalized;var normal=new Vector2(-dir.y,dir.x);
     int steps=Mathf.Max(2,Mathf.CeilToInt(Vector2.Distance(a,b)/1.4f));
     for(int j=0;j<steps;j++){float t=j/(float)steps;float jag=Mathf.Sin((j+segment*17+faultIndex*31)*2.17f)*.65f*Mathf.Sin(t*Mathf.PI);points.Add(Vector2.Lerp(a,b,t)+normal*jag);}
    }
    points.Add(path.Last());faultIndex++;
    int start=v.Count;
    for(int i=0;i<points.Count;i++)
    {
     var q=points[i];var direction=(points[Mathf.Min(i+1,points.Count-1)]-points[Mathf.Max(i-1,0)]).normalized;var normal=new Vector2(-direction.y,direction.x);
     float taper=Mathf.Lerp(1,.12f,Mathf.Clamp01(q.magnitude/85));float irregular=.78f+.22f*Mathf.Sin(i*1.87f+faultIndex);
     float[] widths={-1.7f,-.70f,-.13f,0,.13f,.70f,1.7f};
     Color[] tones={new Color(.26f,.16f,.25f,0),new Color(.25f,.12f,.27f,.37f),new Color(.16f,.045f,.21f,.95f),new Color(.69f,.22f,.82f,.94f),new Color(.16f,.045f,.21f,.95f),new Color(.25f,.12f,.27f,.37f),new Color(.26f,.16f,.25f,0)};
     for(int rank=0;rank<7;rank++)
     {var p=q+normal*widths[rank]*taper*irregular;float x=8+p.x,z=61+p.y;float fade=Mathf.SmoothStep(0,1,Mathf.InverseLerp(34,49,new Vector2(x,z).magnitude));var tint=tones[rank];tint.a*=fade*(1-Water(x,z));v.Add(new Vector3(p.x,Height(x,z)+.14f,p.y));uv.Add(Vector2.zero);colors.Add(tint);}
     if(i==0)continue;for(int rank=0;rank<6;rank++){int k=start+(i-1)*7+rank;indices.AddRange(new[]{k,k+1,k+7,k+1,k+8,k+7});}
    }
   }
   // Small luminous heart with fading perimeter, always flush with the ground.
   int heart=v.Count;v.Add(new Vector3(0,Height(8,61)+.19f,0));uv.Add(Vector2.zero);colors.Add(new Color(.96f,.45f,1,1));
   for(int i=0;i<=24;i++){float angle=i*Mathf.PI/12,r=5.5f+.7f*Mathf.Sin(i*4.7f);float x=Mathf.Cos(angle)*r,z=Mathf.Sin(angle)*r;v.Add(new Vector3(x,Height(8+x,61+z)+.18f,z));uv.Add(Vector2.zero);colors.Add(new Color(.59f,.06f,.75f,0));if(i>0)indices.AddRange(new[]{heart,heart+i+1,heart+i});}
   scar=Create("Region1 · authored Breach pigment at actual world anchor",v.ToArray(),uv.ToArray(),null,indices.ToArray(),scarMaterial,parent);scar.transform.position=new Vector3(origin.x+8,origin.y,origin.z+61);scar.GetComponent<MeshFilter>().sharedMesh.colors=colors.ToArray();
  }
  void BuildBridge()
  {
   // Non-interactive crossing where the authored local road meets the connected brook.
   bridgeMaterial=new Material(Resources.Load<Shader>("World4X/RegionOneGeographicFacets")){name="Clean local timber crossing"};
   var v=new List<Vector3>();var colors=new List<Color>();var t=new List<int>();var axis=new Vector2(.50f,.8660254f);var side=new Vector2(-axis.y,axis.x);var centre=new Vector2(-8,-28);
   Action<Vector3,Vector3,Vector3,Vector3,Color> quad=(a,b,c,d,tint)=>{int n=v.Count;v.Add(origin+a);v.Add(origin+b);v.Add(origin+c);v.Add(origin+d);for(int k=0;k<4;k++)colors.Add(tint);t.AddRange(new[]{n,n+1,n+2,n,n+2,n+3});};
   Func<float,float,float,Vector3> point=(along,across,height)=>{var p=centre+axis*along+side*across;return new Vector3(p.x,height,p.y);};
   for(int i=0;i<24;i++){float a=-5+i*10f/24,b=a+.38f;float y=-3.6f+.65f*(1-Mathf.Pow((a+.19f)/5,2));var tint=Color.Lerp(new Color(.41f,.29f,.16f),new Color(.68f,.53f,.32f),(i%4)/3f);quad(point(a,-1.8f,y),point(a,1.8f,y),point(b,1.8f,y),point(b,-1.8f,y),tint);}
   foreach(float across in new[]{-1.7f,1.7f})
   {quad(point(-5,across,-2.5f),point(-5,across,-2.25f),point(5,across,-2.25f),point(5,across,-2.5f),new Color(.46f,.34f,.20f));
    foreach(float along in new[]{-4f,0f,4f})quad(point(along-.10f,across,-3.6f),point(along-.10f,across,-2.15f),point(along+.10f,across,-2.15f),point(along+.10f,across,-3.6f),new Color(.42f,.30f,.17f));}
   var r=Create("Clean road crossing — no gameplay collider",v.ToArray(),new Vector2[v.Count],null,t.ToArray(),bridgeMaterial);r.GetComponent<MeshFilter>().sharedMesh.colors=colors.ToArray();
  }
  public void SetBlockout(bool value){blockout=value;Refresh();}
  public void Refresh(){if(!ground||!host||!host.WorldCamera)return;float zoom=host.WorldCamera.orthographicSize;ground.SetFloat("_Panorama",Mathf.SmoothStep(0,1,Mathf.InverseLerp(152,220,zoom)));ground.SetFloat("_Near",1-Mathf.SmoothStep(0,1,Mathf.InverseLerp(80,152,zoom)));ground.SetFloat("_Blockout",blockout?1:0);foreach(var r in foliage)r.enabled=!blockout;if(scar)scar.enabled=true;}
  void OnDestroy(){foreach(var mesh in meshes)if(mesh)Destroy(mesh);if(ground)Destroy(ground);if(trees)Destroy(trees);if(scarMaterial)Destroy(scarMaterial);if(bridgeMaterial)Destroy(bridgeMaterial);}
 }
}
