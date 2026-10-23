using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using Eldoria.ReferenceClean;
using UnityEngine;

namespace Eldoria.World4XCanon
{
 // The exported Blender asset is the only landscape. This component never generates terrain.
 public sealed class RegionOneBlenderLandscape : MonoBehaviour
 {
  const string AssetRoot="World4X/Blender/";
  static int nx,nz; static float width,depth; static float[] heights; static byte[] masks;
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
   var anchor=floor.bounds.center-host.WorldHome;
   if(Vector2.Distance(new Vector2(anchor.x,anchor.z),new Vector2(-8,-61))<3)
    instance.artwork.transform.rotation=Quaternion.Euler(0,180,0);
   anchor=floor.bounds.center-host.WorldHome;
   if(Vector2.Distance(new Vector2(anchor.x,anchor.z),new Vector2(8,61))>3)
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
   Debug.Log("REGION1_BLENDER_IMPORTED source=GLB procedural_terrain=0 metres=1 anchor=8,61");
   return instance;
  }
  void ValidateImportedSurface(Vector3 origin)
  {
   var positions=WorldVertices;
   if(positions.Length<60000)throw new InvalidOperationException("Incomplete Blender terrain import.");
   foreach(var p in positions.Where((v,i)=>i%197==0))
    if(Mathf.Abs(p.y-origin.y-Height(p.x-origin.x,p.z-origin.z))>.06f)
     throw new InvalidOperationException("Blender query/mesh scale or orientation mismatch at "+p);
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
