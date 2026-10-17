using System;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEditor;
namespace Eldoria.ReferenceClean.Editor {
public static class ValoriaReferenceCleanGroundTrial {
 const string Root="Assets/Eldoria/ArtTests/ValoriaReferenceClean";
 static Texture2D Input(string asset,string suffix,bool normal){
  var path=Root+"/Sources/MaterialInputs/"+asset+"/"+asset+suffix;
  if(!File.Exists(path))throw new Exception("Ground source missing "+path);
  AssetDatabase.ImportAsset(path,ImportAssetOptions.ForceSynchronousImport);
  var imp=(TextureImporter)AssetImporter.GetAtPath(path);imp.textureType=normal?TextureImporterType.NormalMap:TextureImporterType.Default;imp.sRGBTexture=!normal;imp.maxTextureSize=1024;imp.mipmapEnabled=true;imp.wrapMode=TextureWrapMode.Repeat;imp.anisoLevel=4;imp.SaveAndReimport();return AssetDatabase.LoadAssetAtPath<Texture2D>(path);
 }
 static Texture2D AdaptGround(string asset){
  var t=new Texture2D(2,2,TextureFormat.RGBA32,true);t.LoadImage(File.ReadAllBytes(Root+"/Sources/MaterialInputs/"+asset+"/"+asset+"_diff_1k.png"));var px=t.GetPixels();
  for(int i=0;i<px.Length;i++){var c=px[i];bool green=c.g>c.r*.93f;px[i]=c*(green?new Color(.92f,1.08f,.60f,1):new Color(.85f,.80f,.65f,1));}
  t.SetPixels(px);t.Apply();t.name="Valoria "+asset+" adapted albedo";t.wrapMode=TextureWrapMode.Repeat;t.anisoLevel=4;AssetDatabase.CreateAsset(t,Root+"/GeneratedArt/"+asset+"_adapted.asset");return t;
 }
 static TerrainLayer Layer(string asset,string name,float tile){Input(asset,"_diff_1k.png",false);var l=new TerrainLayer{name=name,diffuseTexture=AdaptGround(asset),normalMapTexture=Input(asset,"_nor_gl_1k.png",true),normalScale=.65f,tileSize=new Vector2(tile,tile),smoothness=.02f,metallic=0};AssetDatabase.CreateAsset(l,Root+"/GeneratedArt/"+name+".terrainlayer");return l;}
 [Serializable]class Report{public string scope,source_sha;public bool same_camera,same_lighting,same_heights;public int changed_cells,source_texture_cap;}
 public static void Capture(Terrain terrain,Camera cam,Light sun,ValoriaReferenceCleanInteraction interaction){
  var input=Path.Combine("../tools/valoria-reference-clean/request.json");if(!File.ReadAllText(input).Contains("\"ground_treatment\": \"zone_surface\""))return;
  var td=terrain.terrainData;var oldLayers=td.terrainLayers;var n=td.alphamapResolution;var old=td.GetAlphamaps(0,0,n,n);var layers=oldLayers.Concat(new[]{Layer("leafy_grass","ScannedLeafMeadow",12),Layer("sparse_grass","ScannedWornSoil",8)}).ToArray();var next=new float[n,n,layers.Length];int changed=0;
  for(int z=0;z<n;z++)for(int x=0;x<n;x++){
   float wx=terrain.transform.position.x+x/(float)(n-1)*td.size.x,wz=terrain.transform.position.z+z/(float)(n-1)*td.size.z;
   float mx=1-Mathf.SmoothStep(0,1,Mathf.InverseLerp(11,15,Mathf.Abs(wx+1))),mz=1-Mathf.SmoothStep(0,1,Mathf.InverseLerp(9,13,Mathf.Abs(wz-14)));
   float path= Mathf.SmoothStep(0,1,Mathf.InverseLerp(4.1f,5.2f,Mathf.Abs(wx-ValoriaReferenceCleanArtBuilder.RoadX(wz))));float mask=mx*mz*path*.9f;
   foreach(var p in new[]{new Vector2(-23,28),new Vector2(-19,9),new Vector2(29,32),new Vector2(28,12)})if(Mathf.Abs(wx-p.x)<9.7f&&Mathf.Abs(wz-p.y)<7.2f)mask=0;
   float soil=.08f+.52f*Mathf.SmoothStep(0,1,Mathf.InverseLerp(.48f,.72f,Mathf.PerlinNoise(wx*.21f+63,wz*.21f+42)));
   for(int k=0;k<oldLayers.Length;k++)next[z,x,k]=old[z,x,k]*(1-mask);next[z,x,oldLayers.Length]=mask*(1-soil);next[z,x,oldLayers.Length+1]=mask*soil;if(mask>.05f)changed++;
  }
  var ev=Path.GetFullPath(Path.Combine(UnityEngine.Application.dataPath,"../../docs/evidence/valoria-reference-clean"));
  var light=sun.transform.rotation.ToString()+sun.intensity+sun.color.ToString();var heights=td.GetHeights(0,0,td.heightmapResolution,td.heightmapResolution);var pos=cam.transform.position;var rot=cam.transform.rotation;var zoom=cam.orthographicSize;
  ValoriaReferenceCleanBuilder.Capture(cam,ev+"/ground-zone-before-main.png",1448,1086);
  cam.orthographicSize=14;cam.transform.position=new Vector3(12,30,-19);cam.transform.LookAt(new Vector3(-3,0,17));var detailPos=cam.transform.position;var detailRot=cam.transform.rotation;
  ValoriaReferenceCleanBuilder.Capture(cam,ev+"/ground-zone-before-detail.png",1448,1086);
  td.terrainLayers=layers;td.SetAlphamaps(0,0,next);terrain.Flush();
  ValoriaReferenceCleanBuilder.Capture(cam,ev+"/ground-zone-after-detail.png",1448,1086);
  bool camera=cam.transform.position==detailPos&&cam.transform.rotation==detailRot&&cam.orthographicSize==14;
  cam.transform.SetPositionAndRotation(pos,rot);cam.orthographicSize=zoom;ValoriaReferenceCleanBuilder.Capture(cam,ev+"/ground-zone-after-main.png",1448,1086);
  var report=new Report{scope="Representative meadow beside central path only; plots and circulation preserved",source_sha=Environment.GetEnvironmentVariable("GITHUB_SHA"),same_camera=camera&&cam.transform.position==pos&&cam.transform.rotation==rot&&cam.orthographicSize==zoom,same_lighting=light==sun.transform.rotation.ToString()+sun.intensity+sun.color.ToString(),same_heights=heights.Cast<float>().SequenceEqual(td.GetHeights(0,0,td.heightmapResolution,td.heightmapResolution).Cast<float>()),changed_cells=changed,source_texture_cap=1024};
  if(!report.same_camera||!report.same_lighting||!report.same_heights||changed<100)throw new Exception("Ground trial identity or area failed");
  File.WriteAllText(ev+"/ground-zone-comparison.json",JsonUtility.ToJson(report,true));EditorUtility.SetDirty(td);
 }
}
}
