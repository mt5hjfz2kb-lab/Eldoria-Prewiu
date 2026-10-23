using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Collections.Generic;
using Eldoria.ReferenceClean;
using UnityEngine;

namespace Eldoria.World4XCanon
{
 // The exported Blender asset is the only landscape. This component never generates terrain.
 public sealed class RegionOneBlenderLandscape : MonoBehaviour
 {
  const string AssetRoot="World4X/Blender/";
  static int nx,nz; static float width,depth; static float[] heights; static byte[] masks; static Dictionary<Vector2Int,System.Collections.Generic.List<VerificationVertex>> sourceVertices; static int sourceVertexCount;
  struct VerificationVertex { public Vector3 position; public int index; }
  GameObject artwork; MeshFilter terrain;
  public Mesh DirectedTerrain=>terrain.sharedMesh;
  public Vector3[] WorldVertices=>terrain.sharedMesh.vertices.Select(terrain.transform.TransformPoint).ToArray();
  public Texture2D GeographicMask
  {
   get {EnsureData();var texture=new Texture2D(nx,nz,TextureFormat.RGBA32,false,true);texture.LoadRawTextureData(masks);texture.Apply();return texture;}
  }
  public static RegionOneBlenderLandscape Install(Transform parent,ValoriaIntegratedWorld host)
  {
   var instance=parent.gameObject.AddComponent<RegionOneBlenderLandscape>();
   var prefab=Resources.Load<GameObject>(AssetRoot+"RegionOneBlenderWorld");
   if(!prefab)throw new InvalidOperationException("Blender GLB must be imported as a prefab by the installed glTFast importer.");
   EnsureData();
   instance.artwork=Instantiate(prefab,parent);instance.artwork.name="Blender authored geography";
   instance.artwork.transform.position=host.WorldHome;
   // glTF is right handed. Resolve the importer conversion using an authored asymmetrical anchor,
   // never by changing vertices or reconstructing any geographic feature.
   var floor=instance.artwork.GetComponentsInChildren<MeshRenderer>(true).Single(x=>x.name=="BLD_BreachFloor");
   var datum=instance.artwork.GetComponentsInChildren<Transform>(true).Single(x=>x.name=="BLD_BreachAnchor");
   var anchor=datum.position-host.WorldHome;
   if(Vector2.Distance(new Vector2(anchor.x,anchor.z),new Vector2(260,-150))<3)
    instance.artwork.transform.rotation=Quaternion.Euler(0,180,0);
   anchor=datum.position-host.WorldHome;
   if(Vector2.Distance(new Vector2(anchor.x,anchor.z),new Vector2(-260,150))>3)
    throw new InvalidOperationException("GLB axis conversion does not preserve Blender's Breach anchor.");
   instance.terrain=instance.artwork.GetComponentsInChildren<MeshFilter>(true).Single(x=>x.name=="BLD_Terrain");
   foreach(var r in instance.artwork.GetComponentsInChildren<Renderer>(true))
   {
    r.gameObject.layer=host.WorldRoot.layer;
    foreach(var m in r.sharedMaterials)
     if(!m||!m.shader||!m.shader.isSupported)throw new InvalidOperationException("Unsupported imported Blender material on "+r.name);
   }
   if(instance.artwork.GetComponentsInChildren<Collider>(true).Length!=0)
    throw new InvalidOperationException("Art export must not add gameplay colliders.");
   instance.ValidateImportedSurface(host.WorldHome);
   RegionOneProductionLod.Install(instance.artwork);
   Debug.Log("REGION1_BLENDER_IMPORTED source=GLB procedural_terrain=0 metres=1 anchor=-260,150");
   return instance;
  }
  void ValidateImportedSurface(Vector3 origin)
  {
   var positions=WorldVertices;
   if(positions.Length<10000)throw new InvalidOperationException("Incomplete Blender terrain import.");
   if(sourceVertices==null)throw new InvalidOperationException("Exact Blender vertex verification data missing.");
   var matched=new HashSet<int>();
   foreach(var p in positions)
   {
    var local=p-origin;var key=new Vector2Int(Mathf.FloorToInt(local.x*4),Mathf.FloorToInt(local.z*4));
    bool found=false;const float toleranceSquared=.002f*.002f;
    for(int dz=-1;dz<=1;dz++)for(int dx=-1;dx<=1;dx++)
     if(sourceVertices.TryGetValue(key+new Vector2Int(dx,dz),out var bucket))
      foreach(var vertex in bucket)
      {float distance=(vertex.position-local).sqrMagnitude;if(distance<toleranceSquared){found=true;matched.Add(vertex.index);}}
    if(!found)throw new InvalidOperationException("Imported terrain vertex differs from Blender source at "+local);
    // Import/world transforms may collapse sub-millimetre separations; cover every source position within the unchanged tolerance.
   }
   if(matched.Count!=sourceVertexCount)throw new InvalidOperationException("Incomplete imported Blender surface.");
  }

  public void Refresh() {} // Phase A uses the same meshes at every zoom.
  public void SetBlockout(bool value) {} // This entire candidate IS the composition blockout.
  static void EnsureData()
  {
   if(heights!=null)return;
   var asset=Resources.Load<TextAsset>(AssetRoot+"RegionOneBlenderQueries");
   if(!asset)throw new InvalidOperationException("Missing Blender terrain query export; no mathematical fallback.");
   using(var input=new MemoryStream(asset.bytes))using(var gzip=new GZipStream(input,CompressionMode.Decompress))using(var reader=new BinaryReader(gzip))
   {
    if(reader.ReadInt32()!=0x31424c45)throw new InvalidDataException("Unknown Blender query format");
    nx=reader.ReadInt32();nz=reader.ReadInt32();width=reader.ReadSingle();depth=reader.ReadSingle();
    if(nx<2||nz<2||nx*nz>2000000||width!=1820||depth!=2000)throw new InvalidDataException("Unexpected Blender terrain dimensions");
    var data=new float[nx*nz];for(int i=0;i<data.Length;i++)data[i]=reader.ReadSingle();
    masks=reader.ReadBytes(data.Length*4);if(masks.Length!=data.Length*4)throw new InvalidDataException("Truncated Blender masks");
    if(reader.ReadInt32()!=0x32564c45)throw new InvalidDataException("Exact Blender source vertices missing");
    int vertexCount=reader.ReadInt32();if(vertexCount<10000||vertexCount>1000000)throw new InvalidDataException("Unexpected source vertex count");
    sourceVertices=new Dictionary<Vector2Int,List<VerificationVertex>>();sourceVertexCount=vertexCount;
    for(int i=0;i<vertexCount;i++)
    {
     float x=reader.ReadSingle(),y=reader.ReadSingle(),z=reader.ReadSingle();
     var key=new Vector2Int(Mathf.FloorToInt(x*4),Mathf.FloorToInt(z*4));
     if(!sourceVertices.TryGetValue(key,out var bucket)){bucket=new List<VerificationVertex>();sourceVertices[key]=bucket;}
     bucket.Add(new VerificationVertex{position=new Vector3(x,y,z),index=i});
    }
    heights=data;
   }
  }
  static float Sample(float x,float z,int channel)
  {
   EnsureData();float u=Mathf.Clamp((x/width+.5f)*(nx-1),0,nx-1.001f),v=Mathf.Clamp((z/depth+.5f)*(nz-1),0,nz-1.001f);
   int ix=(int)u,iz=(int)v;u-=ix;v-=iz;int a=iz*nx+ix;
   return Mathf.Lerp(Mathf.Lerp(Value(a,channel),Value(a+1,channel),u),Mathf.Lerp(Value(a+nx,channel),Value(a+nx+1,channel),u),v);
  }
  static float Value(int i,int channel)=>channel<0?heights[i]:masks[i*4+channel]/255f;
  public static float Height(float x,float z)=>Sample(x,z,-1);
  // Saturates at +/-16m: explicitly a signed proximity mask, not unbounded distance.
  public static float Shore(float x,float z)=>(Sample(x,z,0)-.5f)*32;
  public static float Water(float x,float z)=>Sample(x,z,1);
  public static float Road(float x,float z)=>Sample(x,z,2);
  public static float Forest(float x,float z)=>Sample(x,z,3);
  public static float RoadDistance(float x,float z)=>(1-Road(x,z))*3;
 }
}
