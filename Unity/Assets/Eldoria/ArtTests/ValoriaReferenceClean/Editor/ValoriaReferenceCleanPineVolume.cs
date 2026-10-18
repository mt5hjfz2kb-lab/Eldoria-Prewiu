using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using GLTFast;
using Object=UnityEngine.Object;
namespace Eldoria.ReferenceClean.Editor {
public static class ValoriaReferenceCleanPineVolume {
 const string Root="Assets/Eldoria/ArtTests/ValoriaReferenceClean";
 const string Dir=Root+"/ArtVariants/Pine77";
 static readonly string Ev=Path.GetFullPath("../docs/evidence/valoria-reference-clean");
 [Serializable]class Report{public string source_sha,original_scene_hash,candidate_scene,scope;public bool original_scene_unchanged,same_camera,same_light,bake_succeeded,no_static_batching,art_approved=false;public int original_triangles,candidate_triangles,lightmaps,lightprobes;public float bake_seconds;}
 static string Hash(string p){using(var h=System.Security.Cryptography.SHA256.Create())return BitConverter.ToString(h.ComputeHash(File.ReadAllBytes(p)));}
 static string Stamp(Camera c)=>c.transform.localToWorldMatrix.ToString("R")+c.orthographicSize;
 static void Views(Camera c,string prefix){var p=c.transform.position;var r=c.transform.rotation;float z=c.orthographicSize;try{
  ValoriaReferenceCleanBuilder.Capture(c,Ev+"/"+prefix+"-main.png",1448,1086);
  c.transform.position=new Vector3(24,31,-8);c.transform.LookAt(new Vector3(0,4,35.5f));c.orthographicSize=18;ValoriaReferenceCleanBuilder.Capture(c,Ev+"/"+prefix+"-bastion.png",1448,1086);
 }finally{c.transform.SetPositionAndRotation(p,r);c.orthographicSize=z;}}
 static void PersistTextures(Material m,string prefix){int i=0;foreach(string prop in m.GetTexturePropertyNames()){var t=m.GetTexture(prop);if(!t||!string.IsNullOrEmpty(AssetDatabase.GetAssetPath(t)))continue;var copy=Object.Instantiate(t);Put(copy,Dir+"/"+prefix+"_texture_"+(i++)+".asset");m.SetTexture(prop,copy);}EditorUtility.SetDirty(m);}
 static void RemoveOldFoundationInZone(){foreach(var f in Object.FindObjectsByType<MeshFilter>(FindObjectsSortMode.None).Where(f=>f.name=="Foundation_shrubs_and_groundcover"||f.name=="Shrub_group")){
  var mesh=Object.Instantiate(f.sharedMesh);var vertices=mesh.vertices;int removed=0;
  for(int s=0;s<mesh.subMeshCount;s++){var src=mesh.GetTriangles(s);var keep=new List<int>();for(int i=0;i<src.Length;i+=3){var p=f.transform.TransformPoint((vertices[src[i]]+vertices[src[i+1]]+vertices[src[i+2]])/3);if(Mathf.Abs(p.x)<14&&p.z>26&&p.z<43){removed++;continue;}keep.Add(src[i]);keep.Add(src[i+1]);keep.Add(src[i+2]);}mesh.SetTriangles(keep,s);}
  Put(mesh,Dir+"/"+f.name+"_outside_ensemble.asset");f.sharedMesh=mesh;Debug.Log("BASTION_OLD_FOUNDATION_REMOVED "+f.name+" triangles="+removed);
 }}


 public static async void Build(){ValoriaReferenceCleanRenderSettings guard=null;try{
 Directory.CreateDirectory(Dir);AssetDatabase.Refresh();string source=Root+"/Valoria_Reference_Clean.unity";string hash=Hash(source);var scene=EditorSceneManager.OpenScene(source,OpenSceneMode.Single);guard=Object.FindFirstObjectByType<ValoriaReferenceCleanRenderSettings>();guard.Apply();var cam=Camera.main;string camera=Stamp(cam);var sun=RenderSettings.sun;string light=sun.transform.rotation.ToString("R")+sun.intensity+sun.color.ToString("R");
 Views(cam,"pine77-before");string output=Dir+"/Valoria_Pine77.unity";EditorSceneManager.SaveScene(scene,output);
 var old=Object.FindObjectsByType<MeshFilter>(FindObjectsSortMode.None).Single(f=>f.name=="Fir_tree_group");var renderer=old.GetComponent<MeshRenderer>();var donor=renderer.sharedMaterials;if(donor.Length!=2)throw new Exception("Original pine material mapping changed");int before=old.sharedMesh.triangles.Length/3;
 var temp=new GameObject("Pine77 import temporary");temp.transform.rotation=Quaternion.Euler(0,180,0);var loader=new GltfImport(null,new UninterruptedDeferAgent());if(!await loader.Load(new Uri(Path.GetFullPath(Root+"/Sources/PineCanopy77.glb")).AbsoluteUri,new ImportSettings{GenerateMipMaps=true,AnisotropicFilterLevel=4})||!await loader.InstantiateMainSceneAsync(temp.transform))throw new Exception("Pine GLB import failed");
 var replacement=temp.GetComponentsInChildren<MeshFilter>().Single();var raw=replacement.GetComponent<MeshRenderer>().sharedMaterials;var mesh=PrepareUV(replacement.sharedMesh,raw,"Pine77");
 var m=old.transform.worldToLocalMatrix*replacement.transform.localToWorldMatrix;var normal=m.inverse.transpose;
 var vertices=mesh.vertices;for(int i=0;i<vertices.Length;i++)vertices[i]=m.MultiplyPoint3x4(vertices[i]);mesh.vertices=vertices;
 var normals=mesh.normals;for(int i=0;i<normals.Length;i++)normals[i]=normal.MultiplyVector(normals[i]).normalized;mesh.normals=normals;
 var tangents=mesh.tangents;for(int i=0;i<tangents.Length;i++){var t=m.MultiplyVector(new Vector3(tangents[i].x,tangents[i].y,tangents[i].z)).normalized;tangents[i]=new Vector4(t.x,t.y,t.z,tangents[i].w);}mesh.tangents=tangents;
 if(m.determinant<=0)throw new Exception("Unexpected tree handedness");mesh.RecalculateBounds();EditorUtility.SetDirty(mesh);old.sharedMesh=mesh;
 renderer.sharedMaterials=raw.Select(mat=>mat.name.StartsWith("Bark_")?donor[0]:mat.name.StartsWith("Leaves_Pine")?donor[1]:GIMaterial(mat)).ToArray();renderer.receiveGI=ReceiveGI.Lightmaps;GameObjectUtility.SetStaticEditorFlags(old.gameObject,StaticEditorFlags.ContributeGI);var collider=old.GetComponent<MeshCollider>();if(collider){collider.sharedMesh=null;collider.sharedMesh=mesh;}
 Object.DestroyImmediate(temp);
 var settings=Object.Instantiate(Lightmapping.lightingSettings);settings.name="Pine77 same Progressive CPU";settings.realtimeGI=false;settings.lightmapper=LightingSettings.Lightmapper.ProgressiveCPU;Put(settings,Dir+"/Lighting.lighting");Lightmapping.lightingSettings=settings;Lightmapping.lightingDataAsset=null;Lightmapping.giWorkflowMode=Lightmapping.GIWorkflowMode.OnDemand;EditorSceneManager.SaveScene(scene,output);AssetDatabase.SaveAssets();
 var watch=System.Diagnostics.Stopwatch.StartNew();bool baked=Lightmapping.Bake();watch.Stop();if(!baked||LightmapSettings.lightmaps.Length==0)throw new Exception("Pine native bake failed");
 foreach(var probe in Object.FindObjectsByType<ReflectionProbe>(FindObjectsSortMode.None)){if(probe.mode!=ReflectionProbeMode.Baked)continue;string path=Dir+"/"+probe.name.Replace(" ","_")+".exr";if(!Lightmapping.BakeReflectionProbe(probe,path))throw new Exception("Pine reflection bake failed");AssetDatabase.ImportAsset(path);probe.bakedTexture=AssetDatabase.LoadAssetAtPath<Texture>(path);}
 LightProbes.Tetrahedralize();Views(cam,"pine77-after");
 var report=new Report{source_sha=Environment.GetEnvironmentVariable("GITHUB_SHA"),original_scene_hash=hash,candidate_scene=output,scope="Three observed pine crowns only: tallest existing trunks retained, overlapping duplicate trunks removed and planar foliage replaced by Qwen-directed closed curved boughs. Outside source geometry/UV/corner normals preserved. Existing integrated74 scene remains authoritative. Same camera/light and separate native bake. Pending independent visual review.",original_scene_unchanged=Hash(source)==hash,same_camera=Stamp(cam)==camera,same_light=light==sun.transform.rotation.ToString("R")+sun.intensity+sun.color.ToString("R"),bake_succeeded=baked,bake_seconds=(float)watch.Elapsed.TotalSeconds,original_triangles=before,candidate_triangles=mesh.triangles.Length/3,lightmaps=LightmapSettings.lightmaps.Length,lightprobes=LightmapSettings.lightProbes.count,no_static_batching=Object.FindObjectsByType<MeshRenderer>(FindObjectsSortMode.None).All(r=>(GameObjectUtility.GetStaticEditorFlags(r.gameObject)&StaticEditorFlags.BatchingStatic)==0)};
 if(!report.original_scene_unchanged||!report.same_camera||!report.same_light||!report.no_static_batching)throw new Exception("Pine trial isolation failed");File.WriteAllText(Ev+"/pine77-comparison.json",JsonUtility.ToJson(report,true));EditorSceneManager.SaveScene(scene,output);AssetDatabase.SaveAssets();guard.Restore();Debug.Log("VALORIA_REFERENCE_CLEAN_PINE_VOLUME_COMPLETE");EditorApplication.Exit(0);
 }catch(Exception e){if(guard)guard.Restore();Debug.LogException(e);EditorApplication.Exit(1);}}
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
  PersistTextures(copy,"material_"+id);Put(copy,Dir+"/NativeGI_Material_"+id+".mat");giMaterials.Add(old,copy);return copy;
 }


}
}
