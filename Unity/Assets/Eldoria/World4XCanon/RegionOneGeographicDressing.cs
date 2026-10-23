using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
namespace Eldoria.World4XCanon
{
 // Geographic art only: no targets, colliders, navigation or gameplay state.
 // A continuous rock ribbon and sparse ecological transitions replace the
 // diagram-like separation between empty meadow, grove stamps and blue water.
 public sealed class RegionOneGeographicDressing : MonoBehaviour
 {
  readonly List<Mesh> meshes=new List<Mesh>();Material material;
  readonly List<Vector3> vertices=new List<Vector3>();readonly List<Color> colors=new List<Color>();readonly List<int> triangles=new List<int>();
  Vector3 origin;System.Random rng=new System.Random(17023);
  float Next(float a,float b)=>a+(float)rng.NextDouble()*(b-a);
  Color rock=new Color(.72f,.59f,.36f),grass=new Color(.48f,.52f,.20f);
  public static void Install(Transform parent,Vector3 origin)
  {var c=parent.gameObject.AddComponent<RegionOneGeographicDressing>();c.origin=origin;c.Build();}
  void Triangle(Vector3 a,Vector3 b,Vector3 c,Color color)
  {int n=vertices.Count;vertices.Add(origin+a);vertices.Add(origin+b);vertices.Add(origin+c);colors.Add(color);colors.Add(color);colors.Add(color);triangles.Add(n);triangles.Add(n+1);triangles.Add(n+2);}
  void Quad(Vector3 a,Vector3 b,Vector3 c,Vector3 d,Color color){Triangle(a,b,c,color);Triangle(a,c,d,color);}
  Vector3 P(Vector2 p,float y)=>new Vector3(p.x,y,p.y);
  void Coast()
  {
   var coast=RegionOneDirectedGeography.Coast;
   for(int i=0;i<coast.Length;i++)
   {Vector2 a=coast[i],b=coast[(i+1)%coast.Length],direction=(b-a).normalized,inward=new Vector2(direction.y,-direction.x);int count=Mathf.CeilToInt(Vector2.Distance(a,b)/7);
    for(int k=0;k<count;k++)
    {Vector2 p=Vector2.Lerp(a,b,k/(float)count),q=Vector2.Lerp(a,b,(k+1f)/count);float tone=Next(.83f,1.12f);var topA=P(p+inward*2.6f,-4.02f);var topB=P(q+inward*2.6f,-4.02f);
     Vector3 shoulderA=P(p-inward*.5f,-6.6f),shoulderB=P(q-inward*.5f,-6.6f);Vector3 footA=P(p-inward*2.3f,-20.02f),footB=P(q-inward*2.3f,-20.02f);
     Quad(topA,topB,shoulderB,shoulderA,new Color(.82f,.70f,.43f)*tone);
     Quad(shoulderA,shoulderB,footB,footA,rock*tone);
    }
   }
  }
  void Rock(Vector2 p,float radius,float height,Color color)
  {
   float y=RegionOneDirectedGeography.Height(p.x,p.y)+.08f;int sides=5;var ring=new Vector3[sides];float angle=Next(0,6.28f);
   for(int i=0;i<sides;i++){float a=angle+i*6.283185f/sides;ring[i]=P(p+new Vector2(Mathf.Cos(a),Mathf.Sin(a))*radius*Next(.75f,1.1f),y);}
   var peak=P(p+new Vector2(-radius*.22f,radius*.1f),y+height);
   for(int i=0;i<sides;i++)Triangle(ring[i],peak,ring[(i+1)%sides],color*Next(.76f,1.14f));
  }
  bool Clear(Vector2 p,float shore=8)
  {return RegionOneDirectedGeography.Shore(p.x,p.y)>shore&&RegionOneDirectedGeography.Water(p.x,p.y)<.03f&&RegionOneDirectedGeography.Road(p.x,p.y)<.1f&&p.magnitude>58&&Vector2.Distance(p,new Vector2(8,61))>60;}
  void Build()
  {
   var shader=Resources.Load<Shader>("World4X/RegionOneGeographicFacets");if(!shader)return;
   material=new Material(shader){name="Directed warm cliffs stone and ecological cover"};Coast();
   // Three restrained rocky shoulders, deliberately outside the gameplay core.
   foreach(var centre in new[]{new Vector2(-146,234),new Vector2(151,143),new Vector2(-148,-239)})
    for(int j=0;j<8;j++){var p=centre+new Vector2(Next(-15,15),Next(-19,19));if(Clear(p,4))Rock(p,Next(4,8),Next(5,13),new Color(.80f,.70f,.47f));}
   // Clusters with large empty areas: a spatial ecological field, not uniform dots.
   for(int i=0;i<1050;i++)
   {var p=new Vector2(Next(-560,265),Next(-450,500));if(!Clear(p))continue;
    float habitat=Mathf.PerlinNoise(p.x*.023f+17,p.y*.023f+9);if(habitat<.48f)continue;
    bool stone=i%5==0;int count=stone?2:3;
    for(int j=0;j<count;j++){var q=p+new Vector2(Next(-2.4f,2.4f),Next(-2.4f,2.4f));if(!Clear(q))continue;float r=Next(.45f,1.3f);Rock(q,r,stone?r*.65f:r*.9f,stone?rock:grass*Next(.85f,1.12f));}
   }
   var mesh=new Mesh{name="Authored coast and ecological transition facets",vertices=vertices.ToArray(),colors=colors.ToArray(),triangles=triangles.ToArray()};mesh.RecalculateNormals();mesh.RecalculateBounds();meshes.Add(mesh);
   var go=new GameObject("Directed cliff ribbon low ridges and ecological ground cover");go.transform.SetParent(transform,true);go.AddComponent<MeshFilter>().sharedMesh=mesh;var renderer=go.AddComponent<MeshRenderer>();renderer.sharedMaterial=material;renderer.shadowCastingMode=ShadowCastingMode.Off;renderer.receiveShadows=false;
   Debug.Log("REGION1_GEOGRAPHIC_DRESSING triangles="+triangles.Count/3+" collider_count=0");
  }
  void OnDestroy(){foreach(var mesh in meshes)if(mesh)Destroy(mesh);if(material)Destroy(material);}
 }
}
