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
   HarmonizeGround(parent);
   BuildGround(root,shader);
   BuildLowFoliage(root);
   BuildLife(root,state);
   BuildSmoke(root,state);
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
    colors[k]=Color.Lerp(new Color(.45f,.58f,.33f),new Color(.60f,.65f,.41f),noise);
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
  static void HarmonizeGround(Transform parent)
  {
   var shader=Shader.Find("Eldoria/Valoria Coherence");
   foreach(var r in parent.GetComponentsInChildren<Renderer>(true)){
    if(!r.enabled||r.bounds.max.y>.30f)continue;string chain=Chain(r.transform);
    bool soil=chain.Contains("plane")||chain.Contains("parcel")||chain.Contains("yard")||chain.Contains("worn ")||chain.Contains("shoulder")||chain.Contains("future plot");
    bool road=chain.Contains("plaza")||chain.Contains("main street")||chain.Contains("branch street")||chain.Contains("road")&&!chain.Contains("shoulder");
    if(!soil&&!road)continue;
    var diffuse=Resources.Load<Texture2D>("Valoria/SurfaceCellExternal/"+(road?"cobble":"dirt")+"_diff");
    if(diffuse==null)continue;var m=new Material(shader){name="Valoria shared world-space "+(road?"paving":"earth")};m.SetTexture("_BaseMap",diffuse);m.SetFloat("_Family",road?7:6);m.SetFloat("_BumpScale",0);r.sharedMaterial=m;
    MaterialCount++;
   }
  }
  static void BuildLowFoliage(Transform root)
  {
   var art=ValoriaExternalAssetLibrary.Load();var source=art!=null?art.SlavicBush:null;
   if(source==null)source=Resources.Load<GameObject>("WorldInventory/Bush01");
   if(source==null){Audit.Add("Foliage source unavailable: no primitive substitute");return;}
   var random=new System.Random(711);
   for(int i=0;i<36;i++){
    float a=(float)random.NextDouble()*Mathf.PI*2;float rx=12.2f+(float)random.NextDouble()*2.7f,rz=11.4f+(float)random.NextDouble()*2.2f;
    var p=new Vector3(Mathf.Cos(a)*rx,0,1.4f+Mathf.Sin(a)*rz);
    if(Mathf.Abs(p.z-3.8f)<2||Mathf.Abs(p.x)<4.2f)continue;
    var go=ValoriaKit.BenchmarkPieceModulated("Valoria · authored low foliage",source,p,1.05f+(float)random.NextDouble()*.8f,.66f,Quaternion.Euler(0,i*71,0),new Color(.67f,.78f,.58f));
    if(go==null)continue;go.transform.SetParent(root,true);
    foreach(var c in go.GetComponentsInChildren<Collider>(true))Object.DestroyImmediate(c);
    foreach(var lod in go.GetComponentsInChildren<LODGroup>(true))lod.ForceLOD(0);
   }
  }
  static void BuildSmoke(Transform root,PlayerState state)
  {
   if(state.SawmillLevel==0)return;
   var shader=Shader.Find("Eldoria/Valoria Ambient Smoke");if(shader==null)return;
   var mat=new Material(shader);var camera=Camera.main;
   for(int i=0;i<7;i++){
    var g=Part(root,"Valoria · sawmill working smoke",PrimitiveType.Quad,new Vector3(-5.8f+i*.035f,2.7f+i*.16f,-1.75f),Vector3.one*(.15f+i*.027f),mat);
    if(camera!=null)g.transform.rotation=camera.transform.rotation;
    var motion=g.AddComponent<ValoriaSmokeMotionV1>();motion.Origin=g.transform.position;motion.Phase=i*.4f;
   }
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
 public sealed class ValoriaSmokeMotionV1:MonoBehaviour
 {
  public Vector3 Origin;public float Phase;
  void Update(){float t=Mathf.Repeat(Time.time*.14f+Phase,1);transform.position=Origin+new Vector3(t*.2f,t*.35f,0);if(Camera.main!=null)transform.rotation=Camera.main.transform.rotation;}
 }
 public sealed class ValoriaAmbientMotionV1:MonoBehaviour
 {
  public Vector3 Origin;public float Phase,Range;
  void Update(){transform.position=Origin+new Vector3(Mathf.Sin(Time.time*.35f+Phase)*Range,0,0);}
 }
}
