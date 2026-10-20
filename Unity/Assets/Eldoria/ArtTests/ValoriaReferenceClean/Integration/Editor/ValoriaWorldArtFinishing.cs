#if UNITY_EDITOR
using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Security.Cryptography;
using Eldoria.World4XPrototype;
using Eldoria.Presentation;
using GLTFast;
using UnityEngine.Rendering;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Object=UnityEngine.Object;
namespace Eldoria.ReferenceClean.Editor {
 public static class ValoriaWorldArtFinishing {
  const string Scene="Assets/Eldoria/ArtTests/ValoriaReferenceClean/Integration/Valoria_City_World_UI.unity";
  static string Ev=>Path.GetFullPath("../docs/evidence/valoria-reference-clean/world204");
  [Serializable] public class Node {public string id;public Vector3 position;}
  [Serializable] public class Part {public string name,path,mesh;public bool active;public Vector3 min,max,position,scale;public int triangles,vertices;public string[] materials;}
  [Serializable] public class Ground {public Vector3[] vertices;public int[] triangles;public Vector2[] uv;}
  [Serializable] public class Data {public string scene_sha256,reference_sha256;public Node[] nodes;public Part[] parts;public Ground ground;public Vector3 camera_euler;public float camera_zoom;}
  static string Hash(string p){using(var h=SHA256.Create())return BitConverter.ToString(h.ComputeHash(File.ReadAllBytes(p))).Replace("-","").ToLowerInvariant();}
  static string PathOf(Transform t)=>t.parent?PathOf(t.parent)+"/"+t.name:t.name;
  static void CaptureViews(ValoriaIntegratedWorld host,string prefix){
   var c=host.WorldCamera;var pos=c.transform.position;var rot=c.transform.rotation;float zoom=c.orthographicSize;bool active=host.WorldRoot.activeSelf;host.WorldRoot.SetActive(true);
   try{foreach(string id in new[]{"forest-valoria","quarry-valoria","corrupt-scout","engendro-valoria"}){
    var point=host.WorldRoot.GetComponentsInChildren<WorldHotspot>(true).Single(n=>n.Id==id).transform.position;point.y=2;c.transform.position=point-c.transform.forward*110;
    foreach(float size in new[]{64f,32f,14f}){c.orthographicSize=size;ValoriaReferenceCleanBuilder.Capture(c,Ev+"/"+prefix+"-"+id+"-"+size.ToString("0")+".png",1448,1086);}
    c.orthographicSize=14;ValoriaReferenceCleanBuilder.Capture(c,Ev+"/"+prefix+"-"+id+"-portrait.png",900,1600);
   }}finally{c.transform.SetPositionAndRotation(pos,rot);c.orthographicSize=zoom;host.WorldRoot.SetActive(active);}
  }
  [Serializable] public class Indices {public int[] indices;}
  [Serializable] public class TreeMesh {public string name;public Vector3[] vertices,normals;public Vector2[] uv;public Indices[] submeshes;}
  [Serializable] public class Family {public TreeMesh[] variants,low_variants,extras;public Color[] colors;}
  static void Need(bool b,string message){if(!b)throw new InvalidOperationException(message);}
  static Mesh MakeMesh(TreeMesh source){var m=new Mesh{name=source.name,indexFormat=source.vertices.Length>65535?IndexFormat.UInt32:IndexFormat.UInt16};m.vertices=source.vertices;m.normals=source.normals;if(source.uv!=null&&source.uv.Length==source.vertices.Length)m.uv=source.uv;m.subMeshCount=source.submeshes.Length;for(int i=0;i<source.submeshes.Length;i++)m.SetTriangles(source.submeshes[i].indices,i);m.RecalculateBounds();return m;}
  static Bounds BoundsOf(GameObject obj){var rr=obj.GetComponentsInChildren<Renderer>(true);Need(rr.Length>0,"Missing model bounds");var b=rr[0].bounds;foreach(var r in rr.Skip(1))b.Encapsulate(r.bounds);return b;}
  [Serializable] public class Acceptance {public bool accepted;public string source_scene_sha256,family_sha256,atlas_sha256,quarry_sha256;public string[] image_names,image_hashes;}
  public static void Trial(){Run(false);}
  public static void Integrate(){Run(true);}
  static async void Run(bool integrate){try{
   Directory.CreateDirectory(Ev);EditorSceneManager.OpenScene(Scene,OpenSceneMode.Single);var source=JsonUtility.FromJson<Data>(File.ReadAllText(Ev+"/survey.json"));Need(source.scene_sha256==Hash(Scene),"Current world scene changed");var host=Object.FindFirstObjectByType<ValoriaIntegratedWorld>();bool wasActive=host.WorldRoot.activeSelf;host.WorldRoot.SetActive(true);
   Material originalGround=null;Renderer groundRenderer=null;bool saved=false;var approvedImages=new Dictionary<string,byte[]>();
   if(integrate){var a=JsonUtility.FromJson<Acceptance>(File.ReadAllText(Ev+"/accepted-candidate.json"));Need(a.accepted&&a.source_scene_sha256==Hash(Scene)&&a.family_sha256==Hash(Ev+"/conifer-family.json")&&a.atlas_sha256==Hash(Ev+"/conifer-needles.png")&&a.quarry_sha256==Hash("Assets/Eldoria/ArtTests/ValoriaReferenceClean/Sources/EnvironmentFinish/QuarryGeologyV18.glb"),"Exact visually reviewed world sources required");Need(a.image_names.Length==8&&a.image_hashes.Length==8,"All three zooms and both orientations required");for(int k=0;k<a.image_names.Length;k++){Need(Hash(Ev+"/"+a.image_names[k])==a.image_hashes[k],"Reviewed world image changed");approvedImages.Add(a.image_names[k],File.ReadAllBytes(Ev+"/"+a.image_names[k]));}}
   var originals=new List<Renderer>();var temporary=new List<Object>();var root=new GameObject("Forest and quarry / authored candidate");root.transform.SetParent(host.WorldRoot.transform,true);temporary.Add(root);var oldQuarry=host.WorldRoot.GetComponentsInChildren<Transform>(true).Single(t=>t.name=="Zone1 authored quarry-valoria");bool quarryActive=oldQuarry.gameObject.activeSelf;
   try{
    CaptureViews(host,"before");var family=JsonUtility.FromJson<Family>(File.ReadAllText(Ev+"/conifer-family.json"));Need(family.variants.Length==3&&family.low_variants.Length==3,"Three reviewed silhouettes and strategic meshes required");var meshes=family.variants.Select(MakeMesh).ToArray();var low=family.low_variants.Select(MakeMesh).ToArray();temporary.AddRange(meshes);temporary.AddRange(low);
    var needleAtlas=new Texture2D(2,2,TextureFormat.RGBA32,true,false){name="Original conifer needle atlas",wrapMode=TextureWrapMode.Clamp,filterMode=FilterMode.Trilinear,anisoLevel=2};Need(needleAtlas.LoadImage(File.ReadAllBytes(Ev+"/conifer-needles.png")),"Needle atlas missing");temporary.Add(needleAtlas);
    var materials=family.colors.Select((color,i)=>{var mat=new Material(Shader.Find("Universal Render Pipeline/Lit")){name="Authored conifer surface "+i,enableInstancing=true};mat.SetColor("_BaseColor",color);mat.SetFloat("_Smoothness",.04f);mat.SetFloat("_Metallic",0);if(i>=1&&i<=3){mat.SetTexture("_BaseMap",needleAtlas);mat.SetFloat("_AlphaClip",1);mat.SetFloat("_Cutoff",.32f);mat.EnableKeyword("_ALPHATEST_ON");mat.SetOverrideTag("RenderType","TransparentCutout");mat.renderQueue=2450;}return mat;}).ToArray();temporary.AddRange(materials);
    var ground=host.WorldRoot.GetComponentsInChildren<MeshFilter>(true).Single(f=>f.name=="ZoneOne continuous prairie source");groundRenderer=ground.GetComponent<Renderer>();originalGround=groundRenderer.sharedMaterial;
    // Retain the exact existing paths and destination masks; restore fine-scale
    // surface grain where their old baked painting had erased grass/soil detail.
    var baseProperty=originalGround.HasProperty("baseColorTexture")?"baseColorTexture":"_BaseMap";var baseMap=originalGround.GetTexture(baseProperty);Need(baseMap,"Existing world albedo missing");
    var detailSource=new Texture2D(2,2,TextureFormat.RGBA32,true,false);Need(detailSource.LoadImage(File.ReadAllBytes("Assets/Eldoria/ArtTests/ValoriaReferenceClean/Sources/MaterialInputs/sparse_grass/sparse_grass_diff_1k.png")),"Retained CC0 detail missing");
    var detail=new Texture2D(256,256,TextureFormat.RGBA32,true,false){name="Retained sparse grass neutral surface grain",wrapMode=TextureWrapMode.Repeat,filterMode=FilterMode.Trilinear,anisoLevel=4};var pixels=new Color[256*256];for(int y=0;y<256;y++)for(int x=0;x<256;x++){float lum=detailSource.GetPixelBilinear(x/255f,y/255f).grayscale;float value=Mathf.Clamp(.5f+(lum-.35f)*.20f,.43f,.59f);pixels[y*256+x]=new Color(value,value,value,1);}detail.SetPixels(pixels);detail.Apply();Object.DestroyImmediate(detailSource);temporary.Add(detail);
    var groundMat=new Material(Shader.Find("Universal Render Pipeline/Lit")){name="World retained identity with metric surface grain"};groundMat.SetTexture("_BaseMap",baseMap);groundMat.SetColor("_BaseColor",Color.white);groundMat.SetFloat("_Smoothness",.08f);groundMat.SetFloat("_Metallic",0);groundMat.SetTexture("_DetailAlbedoMap",detail);groundMat.SetTextureScale("_DetailAlbedoMap",new Vector2(60,60));groundMat.SetFloat("_DetailAlbedoMapScale",1);groundMat.EnableKeyword("_DETAIL_MULX2");temporary.Add(groundMat);groundRenderer.sharedMaterial=groundMat;
    var query=ground.GetComponent<MeshCollider>();if(!query){query=ground.gameObject.AddComponent<MeshCollider>();query.sharedMesh=ground.sharedMesh;temporary.Add(query);}Physics.SyncTransforms();
    float GroundAt(Vector3 p){return query.Raycast(new Ray(new Vector3(p.x,100,p.z),Vector3.down),out var hit,200)?hit.point.y:p.y;}
    var trees=host.WorldRoot.GetComponentsInChildren<MeshRenderer>(true).Where(r=>r.gameObject.activeInHierarchy&&r.enabled&&r.name.StartsWith("Forest retained Valoria pine")).OrderBy(r=>PathOf(r.transform),StringComparer.Ordinal).ToArray();Need(trees.Length==98,"Measured tree population changed");
    for(int i=0;i<trees.Length;i++){
     var old=trees[i];var bounds=old.bounds;int variant=i%3;var tree=new GameObject("Conifer / retained tree position "+i);tree.transform.SetParent(root.transform,true);float scale=bounds.size.y/meshes[variant].bounds.size.y;var p=bounds.center;p.y=GroundAt(p)-meshes[variant].bounds.min.y*scale-.055f;tree.transform.position=p;tree.transform.localScale=Vector3.one*scale;tree.transform.rotation=Quaternion.Euler(0,(i*137.508f)%360,0);
     Renderer MakeLOD(string name,Mesh mesh){var obj=new GameObject(name);obj.transform.SetParent(tree.transform,false);obj.AddComponent<MeshFilter>().sharedMesh=mesh;var renderer=obj.AddComponent<MeshRenderer>();renderer.sharedMaterials=materials;renderer.lightProbeUsage=LightProbeUsage.BlendProbes;renderer.reflectionProbeUsage=ReflectionProbeUsage.BlendProbes;return renderer;}
     var high=MakeLOD("Near branches",meshes[variant]);var far=MakeLOD("Strategic silhouette",low[variant]);var group=tree.AddComponent<LODGroup>();group.SetLODs(new[]{new LOD(.16f,new[]{high}),new LOD(.012f,new[]{far})});group.RecalculateBounds();tree.transform.SetParent(old.transform,true);temporary.Add(tree);old.enabled=false;originals.Add(old);
    }
    var forestVisual=host.WorldRoot.GetComponentsInChildren<Transform>(true).Single(t=>t.name=="Zone1 authored forest-valoria");
    var forestPoint=host.WorldRoot.GetComponentsInChildren<WorldHotspot>(true).Single(n=>n.Id=="forest-valoria").transform.position;
    foreach(var offset in new[]{new Vector3(-6,0,-2),new Vector3(5,0,1),new Vector3(-9,0,4),new Vector3(7,0,-3),new Vector3(-4,0,6),new Vector3(9,0,7),new Vector3(2,0,5)}){
     var p=forestPoint+offset;p.y=GroundAt(p)-.04f;var under=new GameObject("Forest understory / young conifer");under.transform.SetParent(root.transform,true);under.transform.position=p;under.transform.localScale=Vector3.one*.13f;under.transform.rotation=Quaternion.Euler(0,offset.x*31,0);under.AddComponent<MeshFilter>().sharedMesh=low[1];under.AddComponent<MeshRenderer>().sharedMaterials=materials;under.transform.SetParent(forestVisual,true);temporary.Add(under);
    }
    Need(family.extras!=null&&family.extras.Length==1,"Fallen timber source missing");var log=MakeMesh(family.extras[0]);temporary.Add(log);
    foreach(var offset in new[]{new Vector3(-5,0,-4),new Vector3(5,0,4)}){var p=forestPoint+offset;p.y=GroundAt(p)-.045f;var obj=new GameObject("Forest fallen timber / retained clearing");obj.transform.SetParent(root.transform,true);obj.transform.position=p;obj.transform.rotation=Quaternion.Euler(0,offset.x<0?28:113,0);obj.AddComponent<MeshFilter>().sharedMesh=log;obj.AddComponent<MeshRenderer>().sharedMaterials=materials;obj.transform.SetParent(forestVisual,true);temporary.Add(obj);}
    // Replace only the previous quarry visual; preserve the hotspot, crane/stock
    // language, every other destination and all gameplay components.
    var oldBounds=BoundsOf(oldQuarry.gameObject);var quarry=new GameObject("Zone1 authored quarry-valoria");quarry.transform.SetParent(root.transform,true);
    using(var loader=new GltfImport(null,new UninterruptedDeferAgent())){Need(await loader.Load(new Uri(Path.GetFullPath("Assets/Eldoria/ArtTests/ValoriaReferenceClean/Sources/EnvironmentFinish/QuarryGeologyV18.glb")).AbsoluteUri,new ImportSettings{GenerateMipMaps=true,AnisotropicFilterLevel=4}),"Authored quarry GLB load failed");Need(await loader.InstantiateMainSceneAsync(quarry.transform),"Authored quarry instantiate failed");
     var copied=new Dictionary<Object,Object>();Object Own(Object src){if(!src)return null;if(copied.TryGetValue(src,out var saved))return saved;var clone=Object.Instantiate(src);copied.Add(src,clone);temporary.Add(clone);return clone;}
     foreach(var filter in quarry.GetComponentsInChildren<MeshFilter>())filter.sharedMesh=(Mesh)Own(filter.sharedMesh);
     foreach(var renderer in quarry.GetComponentsInChildren<MeshRenderer>()){var mm=renderer.sharedMaterials;for(int k=0;k<mm.Length;k++){var original=mm[k];var material=(Material)Own(original);foreach(string property in original.GetTexturePropertyNames()){var texture=original.GetTexture(property);if(texture)material.SetTexture(property,(Texture)Own(texture));}mm[k]=material;}renderer.sharedMaterials=mm;}
    }
    Need(quarry.GetComponentsInChildren<MeshFilter>().All(f=>f.sharedMesh&&f.sharedMesh.vertexCount>0),"Imported quarry resources released");
    quarry.transform.rotation=oldQuarry.rotation;var qb=BoundsOf(quarry);Need(qb.size.x>10&&qb.size.x<40,"Invalid quarry import bounds");quarry.transform.localScale=Vector3.one*(oldBounds.size.x/qb.size.x);qb=BoundsOf(quarry);quarry.transform.position+=new Vector3(oldBounds.center.x-qb.center.x,oldBounds.min.y-qb.min.y,oldBounds.center.z-qb.center.z);oldQuarry.gameObject.SetActive(false);
    foreach(var r in quarry.GetComponentsInChildren<Renderer>()){r.lightmapIndex=-1;r.lightProbeUsage=LightProbeUsage.BlendProbes;r.reflectionProbeUsage=ReflectionProbeUsage.BlendProbes;}
    Physics.SyncTransforms();CaptureViews(host,"after");Need(Hash(Scene)==source.scene_sha256,"Trial scene unexpectedly saved");
    if(integrate){
     var diffs=new List<string>();foreach(var im in approvedImages){diffs.Add(ValoriaTerrainBankAuthoring.CompareRender(im.Value,Ev+"/"+im.Key));File.WriteAllBytes(Ev+"/approved-"+im.Key,im.Value);}File.WriteAllText(Ev+"/render-identity.json","["+string.Join(",",diffs)+"]");
     const string folder="Assets/Eldoria/ArtTests/ValoriaReferenceClean/Integration/WorldArt208";Need(!Directory.Exists(folder),"Refuse overwrite of world art assets");Directory.CreateDirectory(folder);AssetDatabase.Refresh();int ix=0;
     foreach(var o in temporary.OfType<Texture2D>())AssetDatabase.CreateAsset(o,folder+"/texture-"+(ix++)+".asset");foreach(var o in temporary.OfType<Material>())AssetDatabase.CreateAsset(o,folder+"/material-"+(ix++)+".mat");foreach(var o in temporary.OfType<Mesh>())AssetDatabase.CreateAsset(o,folder+"/mesh-"+(ix++)+".asset");
     // The borrowed/new ground query collider is editor-only. Existing collision
     // stays byte-identical; no newly authored visuals carry gameplay colliders.
     foreach(var o in temporary.OfType<MeshCollider>())if(o)Object.DestroyImmediate(o);
     oldQuarry.name="Retained superseded quarry source / hidden";root.name="World authored geology";host.WorldRoot.SetActive(wasActive);AssetDatabase.SaveAssets();EditorSceneManager.MarkSceneDirty(host.gameObject.scene);Need(EditorSceneManager.SaveScene(host.gameObject.scene,Scene),"World art scene save failed");saved=true;File.WriteAllText(Ev+"/integrated.json","{\"visual_scope_accepted\":true,\"scene_sha256\":\""+Hash(Scene)+"\",\"gameplay_gate\":\"PENDING_CURRENT_PROBE\",\"published\":false}");
    }
    if(!integrate)File.WriteAllText(Ev+"/unity-review.json","{\"same_camera_light\":true,\"scene_saved\":false,\"tree_count_preserved\":98,\"LOD_high_transition\":0.16,\"hotspots_unchanged\":true,\"art_approved\":false}");
   }finally{if(!saved){if(groundRenderer&&originalGround)groundRenderer.sharedMaterial=originalGround;foreach(var r in originals)r.enabled=true;oldQuarry.gameObject.SetActive(quarryActive);foreach(var obj in temporary)if(obj)Object.DestroyImmediate(obj);host.WorldRoot.SetActive(wasActive);}}
   if(integrate)ValoriaIntegrationBuild.ValidateCurrent();else{Debug.Log("VALORIA_REFERENCE_CLEAN_WORLD_ART_TRIAL_COMPLETE");EditorApplication.Exit(0);}
  }catch(Exception e){Debug.LogException(e);EditorApplication.Exit(1);}}
  public static void Survey(){try{
   Directory.CreateDirectory(Ev);EditorSceneManager.OpenScene(Scene,OpenSceneMode.Single);var host=Object.FindFirstObjectByType<ValoriaIntegratedWorld>();bool active=host.WorldRoot.activeSelf;host.WorldRoot.SetActive(true);
   var parts=new List<Part>();foreach(var f in host.WorldRoot.GetComponentsInChildren<MeshFilter>(true)){
    if(!f.sharedMesh)continue;var r=f.GetComponent<Renderer>();if(!r)continue;parts.Add(new Part{name=f.name,path=PathOf(f.transform),mesh=AssetDatabase.GetAssetPath(f.sharedMesh),active=f.gameObject.activeInHierarchy&&r.enabled,min=r.bounds.min,max=r.bounds.max,position=f.transform.position,scale=f.transform.lossyScale,vertices=f.sharedMesh.vertexCount,triangles=f.sharedMesh.triangles.Length/3,materials=r.sharedMaterials.Select(m=>m?AssetDatabase.GetAssetPath(m):"").ToArray()});
   }
   var ground=host.WorldRoot.GetComponentsInChildren<MeshFilter>(true).Single(f=>f.name=="ZoneOne continuous prairie source");var gm=ground.sharedMesh;
   var data=new Data{scene_sha256=Hash(Scene),reference_sha256=Hash("../references/VALORIA_APPROVED_VISUAL_REFERENCE.jpg"),nodes=host.WorldRoot.GetComponentsInChildren<WorldHotspot>(true).Select(n=>new Node{id=n.Id,position=n.transform.position}).ToArray(),parts=parts.ToArray(),ground=new Ground{vertices=gm.vertices.Select(v=>ground.transform.TransformPoint(v)).ToArray(),triangles=gm.triangles,uv=gm.uv},camera_euler=host.WorldCamera.transform.eulerAngles,camera_zoom=host.WorldCamera.orthographicSize};File.WriteAllText(Ev+"/survey.json",JsonUtility.ToJson(data));CaptureViews(host,"before");host.WorldRoot.SetActive(active);Debug.Log("VALORIA_REFERENCE_CLEAN_WORLD_ART_SURVEY_COMPLETE");EditorApplication.Exit(0);
  }catch(Exception e){Debug.LogException(e);EditorApplication.Exit(1);}}
 }
}
#endif
