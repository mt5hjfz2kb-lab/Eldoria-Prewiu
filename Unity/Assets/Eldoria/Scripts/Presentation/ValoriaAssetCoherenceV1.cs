using System;
using System.Collections.Generic;
using Eldoria.Domain;
using UnityEngine;
using UnityEngine.Rendering;
using Object=UnityEngine.Object;
namespace Eldoria.Presentation
{
 public static class ValoriaAssetCoherenceV1
 {
  public const string RootName="Valoria · Asset Coherence and Game Presentation v1";
  public static int MaterialCount,PopulationCount,GroundTriangles;
  public static readonly List<string> Audit=new List<string>();
  public static void Apply(Transform parent,PlayerState state)
  {
   MaterialCount=PopulationCount=GroundTriangles=0;Audit.Clear();
   var root=new GameObject(RootName).transform;root.SetParent(parent,true);
   var shader=Shader.Find("Eldoria/Valoria Coherence");if(shader==null)throw new Exception("Coherence shader missing");
   var rock=Resources.Load<Texture2D>("Valoria/SurfaceCellExternal/rock_diff");
   foreach(var r in parent.GetComponentsInChildren<Renderer>(true))
   {
    if(!r.enabled||!r.gameObject.activeInHierarchy)continue;
    string chain=Chain(r.transform);float family=0;
    if(chain.Contains("hero bastion"))family=1;
    else if(chain.Contains("production · aserradero"))family=2;
    else if(chain.Contains("production · cuartel"))family=3;
    else if(chain.Contains("production · granero"))family=4;
    else if(chain.Contains("art consolidation")&&(chain.Contains("gatehouse")||chain.Contains("tower")||chain.Contains("retaining face")||chain.Contains("return")||chain.Contains("expansion gate")))family=5;
    // Helper-created cottages can escape their original parent; suppress by complete scene chain below.
    if(family==0)continue;
    var mats=r.sharedMaterials;var dst=new Material[mats.Length];
    for(int j=0;j<mats.Length;j++){
     var src=mats[j];if(src==null)continue;
     string bp;var albedo=Map(src,out bp,"_BaseMap","_BaseColorTexture","baseColorTexture","_MainTex","_Texture","_Albedo");
     string np;var normal=Map(src,out np,"_BumpMap","_NormalTexture","_NormalMap","normalTexture");
     Audit.Add(r.name+" | family="+family+" | shader="+src.shader.name+" | albedo="+(albedo==null?"NONE":albedo.name+":"+albedo.width+"x"+albedo.height)+" | normal="+(normal==null?"NONE":normal.name)+" | world="+r.bounds);
     if(albedo==null){dst[j]=src;continue;}
     var m=new Material(shader){name="Valoria shared response · "+src.name};
     m.SetTexture("_BaseMap",albedo);m.SetTextureScale("_BaseMap",src.GetTextureScale(bp));m.SetTextureOffset("_BaseMap",src.GetTextureOffset(bp));
     if(normal!=null)m.SetTexture("_BumpMap",normal);
     m.SetFloat("_BumpScale",normal!=null?.75f:0);m.SetFloat("_Family",family);m.SetFloat("_Bottom",r.bounds.min.y);m.SetFloat("_Height",r.bounds.size.y);m.SetFloat("_Smoothness",.055f);
     if(rock!=null)m.SetTexture("_RockMap",rock);
     dst[j]=m;MaterialCount++;
    }
    r.sharedMaterials=dst;
   }
   foreach(var r in Object.FindObjectsByType<Renderer>(FindObjectsSortMode.None)){
    string n=Chain(r.transform);
    if(n.Contains("cottage west")||n.Contains("cottage east"))r.enabled=false;
    if(n.Contains("production surrounding meadow"))r.enabled=false;
   }
   BuildGround(root,shader);
   BuildLowFoliage(root);
   BuildLife(root,state);
  }
  static Texture Map(Material m,out string name,params string[] props){foreach(var p in props)if(m.HasProperty(p)&&m.GetTexture(p)!=null){name=p;return m.GetTexture(p);}name="";return null;}
  static string Chain(Transform t){string s="";for(;t!=null;t=t.parent)s+="|"+t.name.ToLowerInvariant();return s;}
  static void BuildGround(Transform root,Shader shader)
  {
   const int n=84;const float extent=66;
   var v=new Vector3[(n+1)*(n+1)];var uv=new Vector2[v.Length];var colors=new Color[v.Length];var tri=new int[n*n*6];
   for(int z=0;z<=n;z++)for(int x=0;x<=n;x++){
    int k=z*(n+1)+x;float px=-extent+2*extent*x/n,pz=-extent+2*extent*z/n;
    float city=Mathf.Max(Mathf.Abs(px)/11.4f,Mathf.Abs(pz-1.5f)/10.6f);
    float mask=Mathf.SmoothStep(0,1,Mathf.Clamp01((city-1)*1.6f));
    float noise=Mathf.PerlinNoise(px*.11f+16,pz*.11f+29);
    v[k]=new Vector3(px,-.055f+mask*(noise-.45f)*.62f,pz);uv[k]=new Vector2(px*.3f,pz*.3f);
    colors[k]=Color.Lerp(new Color(.46f,.49f,.30f),new Color(.68f,.63f,.43f),noise);
   }
   for(int z=0;z<n;z++)for(int x=0;x<n;x++){int k=z*(n+1)+x,t=(z*n+x)*6;tri[t]=k;tri[t+1]=k+n+1;tri[t+2]=k+1;tri[t+3]=k+1;tri[t+4]=k+n+1;tri[t+5]=k+n+2;}
   var mesh=new Mesh{name="Contained low relief meadow"};mesh.vertices=v;mesh.uv=uv;mesh.colors=colors;mesh.triangles=tri;mesh.RecalculateNormals();mesh.RecalculateBounds();GroundTriangles=tri.Length/3;
   var go=new GameObject("Valoria · contained natural surround");go.transform.SetParent(root,false);go.AddComponent<MeshFilter>().sharedMesh=mesh;
   var mat=new Material(shader);mat.SetFloat("_Ground",1);mat.SetFloat("_BumpScale",0);
   var dirt=Resources.Load<Texture2D>("Valoria/SurfaceCellExternal/dirt_diff");if(dirt!=null)mat.SetTexture("_BaseMap",dirt);
   go.AddComponent<MeshRenderer>().sharedMaterial=mat;
  }
  static Material Mat(Color c){var m=new Material(Shader.Find("Universal Render Pipeline/Lit"));m.color=c;m.SetFloat("_Smoothness",.06f);return m;}
  static GameObject Part(Transform parent,string name,PrimitiveType type,Vector3 p,Vector3 size,Material m){var g=GameObject.CreatePrimitive(type);g.name=name;g.transform.SetParent(parent,false);g.transform.localPosition=p;g.transform.localScale=size;g.GetComponent<Renderer>().sharedMaterial=m;Object.DestroyImmediate(g.GetComponent<Collider>());return g;}
  static void BuildLowFoliage(Transform root)
  {
   // Small continuous clumps only outside wall and expansion seams, combined into one mesh per palette.
   var green=Mat(new Color(.22f,.30f,.15f));var pale=Mat(new Color(.36f,.39f,.22f));
   var temporary=new List<GameObject>();var random=new System.Random(711);
   for(int i=0;i<130;i++){
    float a=(float)random.NextDouble()*Mathf.PI*2;float rx=12.2f+(float)random.NextDouble()*3.7f,rz=11.4f+(float)random.NextDouble()*3.1f;
    var p=new Vector3(Mathf.Cos(a)*rx,0,1.4f+Mathf.Sin(a)*rz);
    // Hard clearance for XW/XE and future upper approach, plus south gate road.
    if(Mathf.Abs(p.z-3.8f)<2||Mathf.Abs(p.x)<4.2f)continue;
    float size=.30f+(float)random.NextDouble()*.65f;
    var g=Part(root,"low hedge clump",PrimitiveType.Sphere,p+new Vector3(0,size*.2f,0),new Vector3(size,size*.42f,size*.78f),i%3==0?pale:green);temporary.Add(g);
   }
   foreach(var mat in new[]{green,pale}){
    var c=new List<CombineInstance>();foreach(var g in temporary)if(g.GetComponent<Renderer>().sharedMaterial==mat)c.Add(new CombineInstance{mesh=g.GetComponent<MeshFilter>().sharedMesh,transform=g.transform.localToWorldMatrix});
    var mesh=new Mesh();mesh.indexFormat=IndexFormat.UInt32;mesh.CombineMeshes(c.ToArray());
    var g2=new GameObject("Valoria · combined low foliage band");g2.transform.SetParent(root,true);g2.AddComponent<MeshFilter>().sharedMesh=mesh;g2.AddComponent<MeshRenderer>().sharedMaterial=mat;
   }
   foreach(var g in temporary)Object.DestroyImmediate(g);
  }
  static void BuildLife(Transform root,PlayerState state)
  {
   var cloth=Mat(new Color(.17f,.28f,.43f));var skin=Mat(new Color(.56f,.40f,.26f));var boots=Mat(new Color(.22f,.18f,.13f));
   var points=new[]{new Vector3(-7.4f,.15f,-2.7f),new Vector3(-4.4f,.15f,-2.5f),new Vector3(4.6f,.15f,-2.4f),new Vector3(7.5f,.15f,-3f),new Vector3(-1.3f,.16f,.3f),new Vector3(1.5f,.16f,1.9f),new Vector3(-1.5f,.16f,-5.7f),new Vector3(1.5f,.16f,-5.7f)};
   for(int i=0;i<points.Length;i++){
    if(i<2&&state.SawmillLevel==0||i>=2&&i<4&&state.BarracksLevel==0)continue;
    var person=new GameObject(i<2?"Valoria · worker":i<4?"Valoria · guard":"Valoria · inhabitant").transform;person.SetParent(root,true);person.position=points[i];person.rotation=Quaternion.Euler(0,i*47,0);
    Part(person,"tunic",PrimitiveType.Capsule,new Vector3(0,.33f,0),new Vector3(.19f,.22f,.15f),cloth);
    Part(person,"head",PrimitiveType.Sphere,new Vector3(0,.60f,0),Vector3.one*.14f,skin);
    foreach(float side in new[]{-1f,1f}){Part(person,"leg",PrimitiveType.Capsule,new Vector3(side*.05f,.12f,0),new Vector3(.07f,.13f,.07f),boots);var arm=Part(person,"arm",PrimitiveType.Capsule,new Vector3(side*.13f,.36f,0),new Vector3(.055f,.13f,.055f),cloth);arm.transform.localRotation=Quaternion.Euler(0,0,side*18);}
    var motion=person.gameObject.AddComponent<ValoriaAmbientMotionV1>();motion.Origin=points[i];motion.Phase=i;motion.Range=i<4?.15f:.32f;PopulationCount++;
   }
  }
 }
 public sealed class ValoriaAmbientMotionV1:MonoBehaviour
 {
  public Vector3 Origin;public float Phase,Range;
  void Update(){transform.position=Origin+new Vector3(Mathf.Sin(Time.time*.35f+Phase)*Range,0,0);}
 }
}
