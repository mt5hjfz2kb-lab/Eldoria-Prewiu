using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using Object=UnityEngine.Object;
namespace Eldoria.ReferenceClean.Editor {
// Opens the existing scene. This pass never authors, relocates or rebuilds the city.
public static class ValoriaReferenceCleanLightingPass {
 const string Root="Assets/Eldoria/ArtTests/ValoriaReferenceClean";
 const string Dir=Root+"/GeneratedLighting";
 const string Scene=Root+"/Valoria_Reference_Clean.unity";
 static readonly string Ev=Path.GetFullPath("../docs/evidence/valoria-reference-clean");
 [Serializable] class Report {
  public string source_sha,baseline_source="2c3d6174fe78ea5cf4031a4611dfb228d60019ac",method="Existing scene / Progressive CPU / baked indirect / realtime mixed sun direct / native URP Lit GI / baked probes";
  public bool same_camera,same_geometry,no_static_batching,realtime_gi=false,bake_succeeded,art_approved=false,physical_mobile_profiled=false;
  public int triangles_before,triangles_after,lightmaps,lightprobes,reflection_probes,gi_meshes;
  public float bake_seconds;public string[] lightmap_paths,reflection_paths;public string lighting_data;
 }
 static string CameraStamp(Camera c)=>c.transform.position.ToString("R")+c.transform.rotation.ToString("R")+c.orthographicSize;
 static string GeometryStamp(){
  using(var bytes=new MemoryStream())using(var writer=new BinaryWriter(bytes)){
   foreach(var f in Object.FindObjectsByType<MeshFilter>(FindObjectsSortMode.None).OrderBy(f=>f.name)){
    writer.Write(f.name);writer.Write(f.transform.localToWorldMatrix.ToString("R"));var v=f.sharedMesh.vertices;
    foreach(int index in f.sharedMesh.triangles){var p=v[index];writer.Write(p.x);writer.Write(p.y);writer.Write(p.z);}
   }writer.Flush();using(var sha=System.Security.Cryptography.SHA256.Create())return BitConverter.ToString(sha.ComputeHash(bytes.ToArray()));
  }
 }
 static int Triangles()=>Object.FindObjectsByType<MeshFilter>(FindObjectsSortMode.None).Sum(f=>f.sharedMesh.triangles.Length/3);
 public static void Build(){try{Run();Debug.Log("VALORIA_REFERENCE_CLEAN_LIGHTING_COMPLETE");EditorApplication.Exit(0);}catch(Exception e){Debug.LogException(e);EditorApplication.Exit(1);}}
 static void Views(Camera cam,string prefix){
  var p=cam.transform.position;var q=cam.transform.rotation;var size=cam.orthographicSize;
  try{
   ValoriaReferenceCleanBuilder.Capture(cam,Ev+"/"+prefix+"-main.png",1448,1086);
   cam.transform.position=new Vector3(24,31,-8);cam.transform.LookAt(new Vector3(0,4,35.5f));cam.orthographicSize=18;
   ValoriaReferenceCleanBuilder.Capture(cam,Ev+"/"+prefix+"-bastion.png",1448,1086);
   cam.transform.position=new Vector3(25,25,-48);cam.transform.LookAt(new Vector3(-3,0,-20));cam.orthographicSize=17;
   ValoriaReferenceCleanBuilder.Capture(cam,Ev+"/"+prefix+"-gate-bridge.png",1448,1086);
  }finally{cam.transform.SetPositionAndRotation(p,q);cam.orthographicSize=size;}
 }
 static void Put(Object o,string path){if(AssetDatabase.LoadMainAssetAtPath(path))AssetDatabase.DeleteAsset(path);AssetDatabase.CreateAsset(o,path);}
 static Mesh PrepareUV(Mesh src,Material[] mats,string label){
  // glTF UV1 is already occupied by scanned stone. Move that material's UVs
  // onto UV0 before creating lightmap UV1; preserve all surface positions.
  var pv=src.vertices;var pn=src.normals;var pt=src.tangents;var pc=src.colors;
  var uv0=src.uv;var uv1=src.uv2;
  var v=new List<Vector3>();var n=new List<Vector3>();var t=new List<Vector4>();var c=new List<Color>();var uv=new List<Vector2>();var sub=new List<int[]>();
  for(int s=0;s<src.subMeshCount;s++){
   var mat=mats[s];int channel=mat&&mat.HasProperty("baseColorTexture_texCoord")?Mathf.RoundToInt(mat.GetFloat("baseColorTexture_texCoord")):0;
   foreach(string map in new[]{"normalTexture","metallicRoughnessTexture","occlusionTexture","emissiveTexture"})if(mat&&mat.HasProperty(map)&&mat.GetTexture(map)&&mat.HasProperty(map+"_texCoord")&&Mathf.RoundToInt(mat.GetFloat(map+"_texCoord"))!=channel)throw new Exception("Mixed material UV channels need explicit mapping: "+mat.name);
   var input=src.GetTriangles(s);var output=new int[input.Length];var remap=new Dictionary<int,int>();
   for(int j=0;j<input.Length;j++){
    int old=input[j];if(!remap.TryGetValue(old,out int idx)){
     idx=v.Count;remap.Add(old,idx);v.Add(pv[old]);if(pn.Length==pv.Length)n.Add(pn[old]);if(pt.Length==pv.Length)t.Add(pt[old]);if(pc.Length==pv.Length)c.Add(pc[old]);
     uv.Add(channel==1&&uv1.Length==pv.Length?uv1[old]:uv0.Length==pv.Length?uv0[old]:Vector2.zero);
    }output[j]=idx;
   }sub.Add(output);
  }
  var mesh=new Mesh{name=label+" GI UV",indexFormat=IndexFormat.UInt32};mesh.SetVertices(v);mesh.SetUVs(0,uv);if(n.Count==v.Count)mesh.SetNormals(n);if(t.Count==v.Count)mesh.SetTangents(t);if(c.Count==v.Count)mesh.SetColors(c);mesh.subMeshCount=sub.Count;for(int s=0;s<sub.Count;s++)mesh.SetTriangles(sub[s],s);mesh.RecalculateBounds();
  UnwrapParam.SetDefaults(out var unwrap);unwrap.hardAngle=88;unwrap.packMargin=.003f;
  Debug.Log("VALORIA_GI_UNWRAP_START "+label+" triangles="+src.triangles.Length/3);
  if(!Unwrapping.GenerateSecondaryUVSet(mesh,unwrap))throw new Exception("Lightmap UV unwrap failed: "+label);
  if(mesh.triangles.Length!=src.triangles.Length||(mesh.bounds.center-src.bounds.center).sqrMagnitude>1e-8f||(mesh.bounds.size-src.bounds.size).sqrMagnitude>1e-8f)throw new Exception("Geometry changed while allocating GI UV");
  Put(mesh,Dir+"/"+label+"_GI.asset");return mesh;
 }
 static readonly Dictionary<Material,Material> giMaterials=new Dictionary<Material,Material>();
 static Texture2D ReadTexture(Texture source){
  var rt=RenderTexture.GetTemporary(source.width,source.height,0,RenderTextureFormat.ARGB32,RenderTextureReadWrite.Linear);var previous=RenderTexture.active;
  try{Graphics.Blit(source,rt);RenderTexture.active=rt;var result=new Texture2D(source.width,source.height,TextureFormat.RGBA32,false,true);result.ReadPixels(new Rect(0,0,source.width,source.height),0,0);result.Apply();return result;}finally{RenderTexture.active=previous;RenderTexture.ReleaseTemporary(rt);}
 }
 static Texture2D ImportMap(Texture2D raw,string path,bool normal){
  File.WriteAllBytes(path,raw.EncodeToPNG());Object.DestroyImmediate(raw);AssetDatabase.ImportAsset(path);var ti=(TextureImporter)AssetImporter.GetAtPath(path);ti.textureType=normal?TextureImporterType.NormalMap:TextureImporterType.Default;ti.sRGBTexture=false;ti.mipmapEnabled=true;ti.maxTextureSize=1024;ti.wrapMode=TextureWrapMode.Repeat;ti.anisoLevel=4;ti.SaveAndReimport();return AssetDatabase.LoadAssetAtPath<Texture2D>(path);
 }
 static Material GIMaterial(Material old){
  if(!old)return old;if(giMaterials.TryGetValue(old,out var found))return found;
  if(old.shader.name=="Universal Render Pipeline/Lit")return old;
  // A fixed UV0 URP Lit material avoids glTF shader UV selectors competing with
  // UV1 lightmaps. Convert ORM channels and normal-map import semantics explicitly.
  string id=giMaterials.Count.ToString("D3");var copy=new Material(Shader.Find("Universal Render Pipeline/Lit")){name=old.name+" native URP baked GI"};
  var color=old.HasProperty("baseColorFactor")?old.GetColor("baseColorFactor"):old.color;copy.SetColor("_BaseColor",color);
  if(old.HasProperty("baseColorTexture"))copy.SetTexture("_BaseMap",old.GetTexture("baseColorTexture"));
  float rough=old.HasProperty("roughnessFactor")?old.GetFloat("roughnessFactor"):.8f;float metal=old.HasProperty("metallicFactor")?old.GetFloat("metallicFactor"):0;
  copy.SetFloat("_Metallic",metal);copy.SetFloat("_Smoothness",1-rough);
  var orm=old.HasProperty("metallicRoughnessTexture")?old.GetTexture("metallicRoughnessTexture"):null;
  if(orm){var raw=ReadTexture(orm);var pixels=raw.GetPixels();for(int i=0;i<pixels.Length;i++){var a=pixels[i];pixels[i]=new Color(a.b*metal,0,0,1-a.g*rough);}raw.SetPixels(pixels);raw.Apply();copy.SetTexture("_MetallicGlossMap",ImportMap(raw,Dir+"/MetalSmooth_"+id+".png",false));copy.SetFloat("_Smoothness",1);copy.EnableKeyword("_METALLICSPECGLOSSMAP");}
  var normal=old.HasProperty("normalTexture")?old.GetTexture("normalTexture"):null;
  if(normal){copy.SetTexture("_BumpMap",ImportMap(ReadTexture(normal),Dir+"/Normal_"+id+".png",true));copy.SetFloat("_BumpScale",old.HasProperty("normalTexture_scale")?old.GetFloat("normalTexture_scale"):.3f);copy.EnableKeyword("_NORMALMAP");}
  var ao=old.HasProperty("occlusionTexture")?old.GetTexture("occlusionTexture"):null;
  if(ao){var raw=ReadTexture(ao);var pixels=raw.GetPixels();for(int i=0;i<pixels.Length;i++)pixels[i]=new Color(1,pixels[i].r,1,1);raw.SetPixels(pixels);raw.Apply();copy.SetTexture("_OcclusionMap",ImportMap(raw,Dir+"/Occlusion_"+id+".png",false));copy.SetFloat("_OcclusionStrength",old.HasProperty("occlusionTexture_strength")?old.GetFloat("occlusionTexture_strength"):1);copy.EnableKeyword("_OCCLUSIONMAP");}
  if(old.HasProperty("emissiveFactor")){var emission=old.GetColor("emissiveFactor");copy.SetColor("_EmissionColor",emission);if(emission.maxColorComponent>0){copy.EnableKeyword("_EMISSION");copy.globalIlluminationFlags=MaterialGlobalIlluminationFlags.BakedEmissive;}}
  copy.SetFloat("_Cull",old.HasProperty("_Cull")?old.GetFloat("_Cull"):2);copy.doubleSidedGI=true;
  Put(copy,Dir+"/NativeGI_Material_"+id+".mat");giMaterials.Add(old,copy);return copy;
 }

 static T Component<T>(VolumeProfile p) where T:VolumeComponent{if(p.TryGet<T>(out var c))return c;return p.Add<T>(true);}
 static void Run(){
  Directory.CreateDirectory(Dir);Directory.CreateDirectory(Ev);AssetDatabase.Refresh();
  if(!File.Exists(Ev+"/lighting-baseline-scene.unity"))File.Copy(Scene,Ev+"/lighting-baseline-scene.unity");
  var scene=EditorSceneManager.OpenScene(Scene,OpenSceneMode.Single);var guard=Object.FindFirstObjectByType<ValoriaReferenceCleanRenderSettings>();if(!guard)throw new Exception("Scene pipeline guard missing");guard.Apply();
  var cam=Camera.main;if(!cam)throw new Exception("Saved player camera missing");var cameraBefore=CameraStamp(cam);var geometryBefore=GeometryStamp();var report=new Report{source_sha=Environment.GetEnvironmentVariable("GITHUB_SHA"),triangles_before=Triangles()};
  Views(cam,"lighting-correction-before");
  var pipeline=Object.Instantiate(guard.scenePipeline);pipeline.name="Valoria baked GI URP";pipeline.colorGradingMode=ColorGradingMode.HighDynamicRange;var ps=new SerializedObject(pipeline);ps.FindProperty("m_ReflectionProbeBlending").boolValue=true;ps.FindProperty("m_ReflectionProbeBoxProjection").boolValue=true;ps.ApplyModifiedPropertiesWithoutUndo();
  Put(pipeline,Dir+"/BakedSceneURP.asset");guard.Restore();guard.scenePipeline=pipeline;guard.Apply();
  var volume=Object.FindObjectsByType<Volume>(FindObjectsSortMode.None).First(v=>v.isGlobal);
  var profile=ScriptableObject.CreateInstance<VolumeProfile>();foreach(var entry in volume.sharedProfile.components)profile.components.Add(Object.Instantiate(entry));Put(profile,Dir+"/BakedLook.asset");foreach(var entry in profile.components)AssetDatabase.AddObjectToAsset(entry,profile);volume.sharedProfile=profile;
  Component<Tonemapping>(profile).mode.Override(TonemappingMode.ACES);var grade=Component<ColorAdjustments>(profile);grade.postExposure.Override(.20f);grade.contrast.Override(7);grade.saturation.Override(-2);Component<WhiteBalance>(profile).temperature.Override(6);
  var bloom=Component<Bloom>(profile);bloom.intensity.Override(.12f);bloom.threshold.Override(1.2f);
  var sky=new Material(Shader.Find("Skybox/Procedural")){name="Warm afternoon environment"};sky.SetFloat("_SunSize",.035f);sky.SetFloat("_AtmosphereThickness",1.1f);sky.SetColor("_SkyTint",new Color(.50f,.56f,.60f));sky.SetColor("_GroundColor",new Color(.38f,.32f,.23f));sky.SetFloat("_Exposure",1.0f);Put(sky,Dir+"/AfternoonSky.mat");
  RenderSettings.skybox=sky;RenderSettings.ambientMode=AmbientMode.Skybox;RenderSettings.ambientIntensity=.9f;RenderSettings.defaultReflectionMode=DefaultReflectionMode.Skybox;RenderSettings.reflectionIntensity=.65f;RenderSettings.reflectionBounces=1;
  var sun=RenderSettings.sun;if(!sun)throw new Exception("Existing sun missing");sun.transform.rotation=Quaternion.Euler(38,32,0);sun.intensity=1.15f;sun.color=new Color(1,.90f,.74f);sun.shadows=LightShadows.Soft;sun.shadowStrength=.88f;sun.shadowBias=.025f;sun.shadowNormalBias=.10f;sun.lightmapBakeType=LightmapBakeType.Mixed;
  foreach(var light in Object.FindObjectsByType<Light>(FindObjectsSortMode.None).Where(l=>l.type==LightType.Point)){light.lightmapBakeType=LightmapBakeType.Baked;light.intensity=2.2f;}
  var settings=new LightingSettings{name="Valoria mobile baked illumination",realtimeGI=false,bakedGI=true,lightmapper=LightingSettings.Lightmapper.ProgressiveCPU,lightmapResolution=4,lightmapMaxSize=2048,lightmapPadding=4,directSampleCount=32,indirectSampleCount=64,environmentSampleCount=64,maxBounces=2,mixedBakeMode=MixedLightingMode.IndirectOnly,directionalityMode=LightmapsMode.CombinedDirectional};
  Put(settings,Dir+"/ValoriaLighting.lighting");Lightmapping.lightingSettings=settings;Lightmapping.giWorkflowMode=Lightmapping.GIWorkflowMode.OnDemand;
  foreach(var f in Object.FindObjectsByType<MeshFilter>(FindObjectsSortMode.None)){
   var r=f.GetComponent<MeshRenderer>();if(!r)continue;
   // GI eligibility is independent of BatchingStatic. Never set isStatic=true.
   GameObjectUtility.SetStaticEditorFlags(f.gameObject,0);r.lightProbeUsage=LightProbeUsage.BlendProbes;r.reflectionProbeUsage=ReflectionProbeUsage.BlendProbes;
   bool chosen=f.name.StartsWith("Bastion_I")||f.name.StartsWith("Gate_")||f.name.StartsWith("Bridge_")||f.name.StartsWith("Axial_Paving")||f.name.StartsWith("Outer_Road");if(!chosen)continue;
   bool ready=r.sharedMaterials.All(m=>m&&m.shader.name=="Universal Render Pipeline/Lit")&&AssetDatabase.GetAssetPath(f.sharedMesh).StartsWith(Dir+"/")&&f.sharedMesh.uv2.Length==f.sharedMesh.vertexCount;if(!ready)f.sharedMesh=PrepareUV(f.sharedMesh,r.sharedMaterials,f.name.Replace(" ","_"));r.sharedMaterials=r.sharedMaterials.Select(GIMaterial).ToArray();r.receiveGI=ReceiveGI.Lightmaps;
   GameObjectUtility.SetStaticEditorFlags(f.gameObject,StaticEditorFlags.ContributeGI);var sr=new SerializedObject(r);sr.FindProperty("m_ScaleInLightmap").floatValue=f.name.StartsWith("Bastion_I")||f.name.StartsWith("Gate_")?2f:1f;sr.ApplyModifiedPropertiesWithoutUndo();report.gi_meshes++;
  }
  var terrain=Object.FindFirstObjectByType<Terrain>();if(!terrain)throw new Exception("Existing terrain missing");GameObjectUtility.SetStaticEditorFlags(terrain.gameObject,StaticEditorFlags.ContributeGI);
  var group=Object.FindObjectsByType<LightProbeGroup>(FindObjectsSortMode.None).FirstOrDefault(g=>g.name=="Baked light probes for moving construction and foliage");if(!group)group=new GameObject("Baked light probes for moving construction and foliage").AddComponent<LightProbeGroup>();var probes=new List<Vector3>();
  for(int z=-48;z<=52;z+=10)for(int x=-42;x<=42;x+=12)foreach(float h in new[]{.7f,3,7})probes.Add(new Vector3(x,terrain.SampleHeight(new Vector3(x,0,z))+terrain.transform.position.y+h,z));group.probePositions=probes.ToArray();
  var reflection=new List<ReflectionProbe>();var paths=new List<string>();
  void Reflection(string name,Vector3 pos,Vector3 size){var p=Object.FindObjectsByType<ReflectionProbe>(FindObjectsSortMode.None).FirstOrDefault(v=>v.name==name+" baked reflection");if(!p)p=new GameObject(name+" baked reflection").AddComponent<ReflectionProbe>();p.transform.position=pos;p.size=size;p.mode=ReflectionProbeMode.Baked;p.resolution=128;p.boxProjection=true;p.clearFlags=ReflectionProbeClearFlags.Skybox;p.cullingMask=~0;p.nearClipPlane=.3f;p.farClipPlane=130;p.intensity=.65f;reflection.Add(p);paths.Add(Dir+"/"+name+"Reflection.exr");}
  Reflection("Bastion",new Vector3(0,5,35),new Vector3(32,18,30));Reflection("Gate",new Vector3(-1.5f,3,-10),new Vector3(28,16,24));Reflection("Bridge",new Vector3(-6,1,-23),new Vector3(30,15,26));Reflection("River",new Vector3(20,0,-23),new Vector3(65,14,32));
  EditorSceneManager.SaveScene(scene,Scene);AssetDatabase.SaveAssets();
  var watch=System.Diagnostics.Stopwatch.StartNew();Debug.Log("VALORIA_PROGRESSIVE_CPU_BAKE_START");report.bake_succeeded=Lightmapping.Bake();watch.Stop();report.bake_seconds=(float)watch.Elapsed.TotalSeconds;
  if(!report.bake_succeeded||LightmapSettings.lightmaps.Length==0)throw new Exception("No actual baked lightmaps produced");
  for(int i=0;i<reflection.Count;i++){if(!Lightmapping.BakeReflectionProbe(reflection[i],paths[i]))throw new Exception("Reflection bake failed: "+paths[i]);AssetDatabase.ImportAsset(paths[i]);reflection[i].bakedTexture=AssetDatabase.LoadAssetAtPath<Texture>(paths[i]);if(!reflection[i].bakedTexture)throw new Exception("Reflection texture missing");}
  LightProbes.Tetrahedralize();
  report.lightmaps=LightmapSettings.lightmaps.Length;report.lightprobes=LightmapSettings.lightProbes?LightmapSettings.lightProbes.count:0;report.reflection_probes=reflection.Count;report.reflection_paths=paths.ToArray();report.lightmap_paths=LightmapSettings.lightmaps.Select(m=>AssetDatabase.GetAssetPath(m.lightmapColor)).ToArray();report.lighting_data=AssetDatabase.GetAssetPath(Lightmapping.lightingDataAsset);
  if(report.lightprobes==0)throw new Exception("No baked light probes");
  Views(cam,"lighting-after");File.Copy(Ev+"/lighting-after-bastion.png",Ev+"/detail-bastion.png",true);File.Copy(Ev+"/lighting-after-gate-bridge.png",Ev+"/detail-gate-bridge.png",true);ValoriaReferenceCleanBuilder.Capture(cam,Ev+"/D-final.png",1448,1086);ValoriaReferenceCleanBuilder.Capture(cam,Ev+"/C-art-scene.png",1448,1086);
  report.same_camera=CameraStamp(cam)==cameraBefore;report.same_geometry=GeometryStamp()==geometryBefore;report.triangles_after=Triangles();report.no_static_batching=Object.FindObjectsByType<MeshRenderer>(FindObjectsSortMode.None).All(r=>(GameObjectUtility.GetStaticEditorFlags(r.gameObject)&StaticEditorFlags.BatchingStatic)==0);
  File.WriteAllText(Ev+"/lighting-comparison.json",JsonUtility.ToJson(report,true));if(!report.same_camera||!report.same_geometry||!report.no_static_batching)throw new Exception("Lighting isolation guard failed");
  EditorUtility.SetDirty(profile);EditorSceneManager.SaveScene(scene,Scene);AssetDatabase.SaveAssets();
  AssetDatabase.ExportPackage(new[]{Scene,Root+"/Prefabs"},Ev+"/Valoria_Reference_Clean.unitypackage",ExportPackageOptions.Recurse|ExportPackageOptions.IncludeDependencies);
  guard.Restore();
 }
}
}
