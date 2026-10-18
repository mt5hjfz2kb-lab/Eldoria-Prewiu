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
  for(int i=0;i<px.Length;i++){var c=px[i];float green=Mathf.SmoothStep(0,1,Mathf.InverseLerp(.90f,1.10f,c.g/Mathf.Max(.01f,c.r)));px[i]=c*Color.Lerp(new Color(.90f,.86f,.78f,1),new Color(.95f,.98f,.85f,1),green)*new Color(.95f,.95f,.62f,1);}
  t.SetPixels(px);t.Apply();t.name="Valoria "+asset+" adapted albedo";t.wrapMode=TextureWrapMode.Repeat;t.anisoLevel=4;AssetDatabase.CreateAsset(t,Root+"/GeneratedArt/"+asset+"_adapted.asset");return t;
 }
 static TerrainLayer Layer(string asset,string name,float tile){Input(asset,"_diff_1k.png",false);var l=new TerrainLayer{name=name,diffuseTexture=AdaptGround(asset),normalMapTexture=Input(asset,"_nor_gl_1k.png",true),normalScale=.65f,tileSize=new Vector2(tile,tile),smoothness=.02f,metallic=0};AssetDatabase.CreateAsset(l,Root+"/GeneratedArt/"+name+".terrainlayer");return l;}
 [Serializable]class Report{public string scope,source_sha;public bool same_camera,same_lighting,same_heights;public int changed_cells,source_texture_cap;}
 public static void Capture(Terrain terrain,Camera cam,Light sun,ValoriaReferenceCleanInteraction interaction){
  var input=File.ReadAllText("../tools/valoria-reference-clean/request.json");bool global=input.Contains("\"ground_treatment\": \"all_surface\"");if(!global&&!input.Contains("\"ground_treatment\": \"zone_surface\""))return;
  var td=terrain.terrainData;var oldLayers=td.terrainLayers;var n=td.alphamapResolution;var old=td.GetAlphamaps(0,0,n,n);var leaf=Layer("leafy_grass","ScannedLeafMeadow",12);var soilLayer=Layer("sparse_grass","ScannedWornSoil",8);var layers=global?new[]{leaf,oldLayers[1],soilLayer,oldLayers[3]}:oldLayers.Concat(new[]{leaf,soilLayer}).ToArray();var next=new float[n,n,layers.Length];int changed=0;
  for(int z=0;z<n;z++)for(int x=0;x<n;x++){
   float wx=terrain.transform.position.x+x/(float)(n-1)*td.size.x,wz=terrain.transform.position.z+z/(float)(n-1)*td.size.z;
   float mx=1-Mathf.SmoothStep(0,1,Mathf.InverseLerp(11,15,Mathf.Abs(wx+1))),mz=1-Mathf.SmoothStep(0,1,Mathf.InverseLerp(9,13,Mathf.Abs(wz-14)));
   float path= Mathf.SmoothStep(0,1,Mathf.InverseLerp(4.1f,5.2f,Mathf.Abs(wx-ValoriaReferenceCleanArtBuilder.RoadX(wz))));float mask=mx*mz*path*.9f;
   foreach(var p in new[]{new Vector2(-23,28),new Vector2(-19,9),new Vector2(29,32),new Vector2(28,12)})if(Mathf.Abs(wx-p.x)<9.7f&&Mathf.Abs(wz-p.y)<7.2f)mask=0;
   float soil=.08f+.52f*Mathf.SmoothStep(0,1,Mathf.InverseLerp(.48f,.72f,Mathf.PerlinNoise(wx*.21f+63,wz*.21f+42)));
   if(global){float[] weights={old[z,x,0]*1.15f,old[z,x,1],old[z,x,2]*.95f,old[z,x,3]*.40f};float sum=weights.Sum();for(int k=0;k<4;k++)next[z,x,k]=weights[k]/sum;changed++;}else{for(int k=0;k<oldLayers.Length;k++)next[z,x,k]=old[z,x,k]*(1-mask);next[z,x,oldLayers.Length]=mask*(1-soil);next[z,x,oldLayers.Length+1]=mask*soil;if(mask>.05f)changed++;}
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
  var report=new Report{scope=global?"Extend inspected meadow and soil surfaces; four terrain layers, unchanged heightfield and plots/circulation":"Representative meadow beside central path only; plots and circulation preserved",source_sha=Environment.GetEnvironmentVariable("GITHUB_SHA"),same_camera=camera&&cam.transform.position==pos&&cam.transform.rotation==rot&&cam.orthographicSize==zoom,same_lighting=light==sun.transform.rotation.ToString()+sun.intensity+sun.color.ToString(),same_heights=heights.Cast<float>().SequenceEqual(td.GetHeights(0,0,td.heightmapResolution,td.heightmapResolution).Cast<float>()),changed_cells=changed,source_texture_cap=1024};
  if(!report.same_camera||!report.same_lighting||!report.same_heights||changed<100)throw new Exception("Ground trial identity or area failed");
  File.WriteAllText(ev+"/ground-zone-comparison.json",JsonUtility.ToJson(report,true));EditorUtility.SetDirty(td);
 }
 // Native lookdev on the saved scene, never the procedural scene builder.
 public static void BuildLocalized(){
  ValoriaReferenceCleanRenderSettings guard=null;Terrain terrain=null;TerrainData original=null,trial=null;
  Texture2D meadow=null;TerrainLayer meadowLayer=null;
  try{
   var scene=UnityEditor.SceneManagement.EditorSceneManager.OpenScene(Root+"/Valoria_Reference_Clean.unity",UnityEditor.SceneManagement.OpenSceneMode.Single);
   guard=UnityEngine.Object.FindFirstObjectByType<ValoriaReferenceCleanRenderSettings>();guard.Apply();
   terrain=UnityEngine.Object.FindFirstObjectByType<Terrain>();original=terrain.terrainData;
   var cam=Camera.main;var position=cam.transform.position;var rotation=cam.transform.rotation;float zoom=cam.orthographicSize;
   var ev=Path.GetFullPath("../docs/evidence/valoria-reference-clean");
   string light=RenderSettings.sun.transform.rotation.ToString("R")+RenderSettings.sun.intensity+RenderSettings.sun.color.ToString();
   void Views(string prefix){
    cam.transform.SetPositionAndRotation(position,rotation);cam.orthographicSize=zoom;
    ValoriaReferenceCleanBuilder.Capture(cam,ev+"/"+prefix+"-main.png",1448,1086);
    cam.transform.position=new Vector3(24,31,-8);cam.transform.LookAt(new Vector3(0,4,35.5f));cam.orthographicSize=18;
    ValoriaReferenceCleanBuilder.Capture(cam,ev+"/"+prefix+"-detail.png",1448,1086);
    cam.transform.SetPositionAndRotation(position,rotation);cam.orthographicSize=zoom;
   }
   Views("bastion-ground-before");
   trial=UnityEngine.Object.Instantiate(original);
   var layers=original.terrainLayers;
   var source=layers[0].diffuseTexture;int level=Mathf.Min(4,source.mipmapCount-1);
   var pixels=source.GetPixels(level);int width=Mathf.Max(1,source.width>>level),height=Mathf.Max(1,source.height>>level);
   var values=pixels.Select(p=>p.grayscale).OrderBy(v=>v).ToArray();
   float low=values[values.Length/10],high=values[values.Length*9/10];
   for(int i=0;i<pixels.Length;i++){
    float value=Mathf.SmoothStep(0,1,Mathf.InverseLerp(low,high,pixels[i].grayscale));
    pixels[i]=Color.Lerp(new Color(.24f,.30f,.075f),new Color(.43f,.46f,.16f),value);
   }
   meadow=new Texture2D(width,height,TextureFormat.RGB24,true){name="Trial broad meadow from existing CC0 scan",wrapMode=TextureWrapMode.Repeat,anisoLevel=4};
   meadow.SetPixels(pixels);meadow.Apply();
   meadowLayer=UnityEngine.Object.Instantiate(layers[0]);meadowLayer.name="Trial bastion meadow";
   meadowLayer.diffuseTexture=meadow;meadowLayer.tileSize=new Vector2(18,18);meadowLayer.normalScale=.25f;
   trial.terrainLayers=layers.Concat(new[]{meadowLayer}).ToArray();
   int n=original.alphamapResolution;var before=original.GetAlphamaps(0,0,n,n);var next=new float[n,n,layers.Length+1];int changed=0;
   for(int z=0;z<n;z++)for(int x=0;x<n;x++){
    float wx=terrain.transform.position.x+x/(float)(n-1)*original.size.x;
    float wz=terrain.transform.position.z+z/(float)(n-1)*original.size.z;
    float mask=(1-Mathf.SmoothStep(0,1,Mathf.InverseLerp(13,18,Mathf.Abs(wx))))*
     Mathf.SmoothStep(0,1,Mathf.InverseLerp(11,16,wz))*(1-Mathf.SmoothStep(0,1,Mathf.InverseLerp(44,49,wz)));
    foreach(var p in new[]{new Vector2(-23,28),new Vector2(-19,9),new Vector2(29,32),new Vector2(28,12)})
     if(Mathf.Abs(wx-p.x)<10&&Mathf.Abs(wz-p.y)<7.5f)mask=0;
    float fringe=(Mathf.PerlinNoise(wx*.22f+13,wz*.22f+47)-.5f)*2;
    float fortDistance=Mathf.Max(Mathf.Abs(wx)-10.6f,Mathf.Max(25.5f-wz,wz-44));
    float apron=1-Mathf.SmoothStep(0,1,Mathf.InverseLerp(.2f,4.5f,fortDistance+fringe));
    float shoulder=wz<27?1-Mathf.SmoothStep(0,1,Mathf.InverseLerp(3.7f,6.9f,Mathf.Abs(wx-ValoriaReferenceCleanArtBuilder.RoadX(wz))+fringe*.6f)):0;
    float soil=Mathf.Clamp01(Mathf.Max(apron,shoulder)*.75f);
    float meadowWeight=.92f*(1-soil);
    for(int k=0;k<layers.Length;k++)next[z,x,k]=before[z,x,k]*(1-mask);
    next[z,x,0]+=mask*(1-soil-meadowWeight);
    next[z,x,2]+=mask*soil;next[z,x,layers.Length]=mask*meadowWeight;
    if(mask>.05f)changed++;
   }
   trial.SetAlphamaps(0,0,next);terrain.terrainData=trial;terrain.Flush();
   Views("bastion-ground-after");
   var report=new Report{scope="Single bastion apron and adjacent meadow surface trial. Existing CC0 scan mip-filtered into coherent grass masses; irregular soil transition. No scene save or global extension.",
    source_sha=Environment.GetEnvironmentVariable("GITHUB_SHA"),same_camera=cam.transform.position==position&&cam.transform.rotation==rotation&&cam.orthographicSize==zoom,
    same_lighting=light==RenderSettings.sun.transform.rotation.ToString("R")+RenderSettings.sun.intensity+RenderSettings.sun.color.ToString(),
    same_heights=original.GetHeights(0,0,original.heightmapResolution,original.heightmapResolution).Cast<float>().SequenceEqual(trial.GetHeights(0,0,trial.heightmapResolution,trial.heightmapResolution).Cast<float>()),
    changed_cells=changed,source_texture_cap=width};
   if(!report.same_camera||!report.same_lighting||!report.same_heights||changed<100)throw new Exception("Localized lookdev identity failed");
   File.WriteAllText(ev+"/bastion-ground-trial.json",JsonUtility.ToJson(report,true));
   terrain.terrainData=original;terrain.Flush();guard.Restore();
   UnityEngine.Object.DestroyImmediate(trial);UnityEngine.Object.DestroyImmediate(meadowLayer);UnityEngine.Object.DestroyImmediate(meadow);
   Debug.Log("VALORIA_REFERENCE_CLEAN_GROUND_TRIAL_COMPLETE");EditorApplication.Exit(0);
  }catch(Exception e){
   if(terrain&&original){terrain.terrainData=original;terrain.Flush();}
   if(guard)guard.Restore();Debug.LogException(e);EditorApplication.Exit(1);
  }
 }

}
}
