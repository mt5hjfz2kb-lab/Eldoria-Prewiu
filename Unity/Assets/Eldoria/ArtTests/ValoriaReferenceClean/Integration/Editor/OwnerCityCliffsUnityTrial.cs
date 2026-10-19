#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using GLTFast;
using Eldoria.Domain;
using Object = UnityEngine.Object;

namespace Eldoria.ReferenceClean.Editor {
/// <summary>Independent, same-camera Unity art A/B. Keeps canonical scene immutable.
/// Reuses actual owner Blender source, licensed rock_3 textures and original game-rock.
/// The candidate has no collision and never touches the real user-save path.
/// </summary>
public static class OwnerCityCliffsUnityTrial {
 const string Base="Assets/Eldoria/ArtTests/ValoriaReferenceClean/Integration";
 const string Live=Base+"/Valoria_City_World_UI.unity";
 const string Source="Assets/Eldoria/ArtTests/ValoriaReferenceClean/Sources/OwnerCityCliffsV19.glb";
 const string Target=Base+"/ArtCandidates/OwnerCityCliffsV19";
 static string Ev=>Path.GetFullPath(Path.Combine(Application.dataPath,
    "../../docs/evidence/valoria-reference-clean"));
 [Serializable] sealed class BlenderSource {
  public string output_sha256,blend_sha256;public bool art_approved;public int triangles;
 }
 [Serializable] sealed class Report {
  public string source_commit,source_glb_sha256,original_scene_sha256,trial_scene;
  public int renderers,triangles,colliders;
  public bool original_scene_byte_preserved,terrain_preserved,hotspots_preserved,
    gameplay_plots_preserved,art_approved,physical_mobile_tested;
  public string[] images,limitations;
 }
 static void Require(bool valid,string reason) {
  if(!valid)throw new InvalidOperationException("Owner city cliff visual gate: "+reason);
 }
 static string Hash(string f) {
  using(var sha=SHA256.Create())return BitConverter.ToString(sha.ComputeHash(File.ReadAllBytes(f)))
    .Replace("-","").ToLowerInvariant();
 }
 static void Cap(Camera camera,string name,int w,int h) {
  ValoriaReferenceCleanBuilder.Capture(camera,Path.Combine(Ev,name+".png"),w,h);
 }
 static void CaptureSet(Camera cam,string name) {
  var p=cam.transform.position;var q=cam.transform.rotation;
  var ortho=cam.orthographicSize;var aspect=cam.aspect;
  try {
   Cap(cam,"owner-citycliffs-"+name+"-wide",1600,900);
   Cap(cam,"owner-citycliffs-"+name+"-portrait",900,1600);
   cam.transform.position=new Vector3(-46,28,-21);
   cam.transform.LookAt(new Vector3(-47,-1,-25));
   cam.orthographicSize=18;
   Cap(cam,"owner-citycliffs-"+name+"-river",1448,1086);
  }finally{cam.transform.SetPositionAndRotation(p,q);cam.orthographicSize=ortho;cam.aspect=aspect;}
 }
 static void PersistAll(GameObject root,Report report) {
  Directory.CreateDirectory(Target);AssetDatabase.Refresh();int n=0;
  foreach(var f in root.GetComponentsInChildren<MeshFilter>(true)){
   Require(f.sharedMesh!=null,"Blank GLB renderer mesh");
   var m=Object.Instantiate(f.sharedMesh);m.name="OriginalSourceRockMesh"+n;
   AssetDatabase.CreateAsset(m,Target+"/CliffMesh"+(n++)+".asset");f.sharedMesh=m;
   report.triangles+=m.triangles.Length/3;
  }
  foreach(var r in root.GetComponentsInChildren<MeshRenderer>(true)){
   r.lightmapIndex=-1;r.receiveGI=ReceiveGI.LightProbes;
   r.lightProbeUsage=LightProbeUsage.BlendProbes;
   r.shadowCastingMode=ShadowCastingMode.On;
   var mats=new Material[r.sharedMaterials.Length];
   for(int i=0;i<mats.Length;i++){
    Require(r.sharedMaterials[i]&&r.sharedMaterials[i].shader
      &&!r.sharedMaterials[i].shader.name.Contains("InternalErrorShader"),
      "Invalid GLTFast material");
    var mat=Object.Instantiate(r.sharedMaterials[i]);
    foreach(var texprop in mat.GetTexturePropertyNames()){
     var t=mat.GetTexture(texprop);
     if(t==null||!string.IsNullOrEmpty(AssetDatabase.GetAssetPath(t)))continue;
     var copy=Object.Instantiate(t);copy.name="RockPBRTexture"+n;
     AssetDatabase.CreateAsset(copy,Target+"/RockTexture"+(n++)+".asset");
     mat.SetTexture(texprop,copy);
    }
    AssetDatabase.CreateAsset(mat,Target+"/RockMaterial"+(n++)+".mat");
    mats[i]=mat;
   }
   r.sharedMaterials=mats;report.renderers++;
  }
  report.colliders=root.GetComponentsInChildren<Collider>(true).Length;
  Require(report.colliders==0,"Geological visual source must not add gameplay colliders");
  Require(report.renderers>=4&&report.triangles>=3000&&report.triangles<120000,
    "Actual donor and geological silhouette not recovered");
 }
 public static async void RenderCandidate() {
  Directory.CreateDirectory(Ev);
  try {
   var manifest=Path.Combine(Ev,"owner-city-cliffs-v19-source.json");
   Require(File.Exists(manifest)&&File.Exists(Source),"Blender source+proof must exist");
   var b=JsonUtility.FromJson<BlenderSource>(File.ReadAllText(manifest));
   Require(b!=null&&!b.art_approved&&b.output_sha256==Hash(Source),
     "Only genuine SHA-pinned Blender originals can be trialed");
   var hash=Hash(Live);
   var scene=EditorSceneManager.OpenScene(Live,OpenSceneMode.Single);
   var city=Object.FindFirstObjectByType<ValoriaReferenceCleanGameplay>();
   var host=Object.FindFirstObjectByType<ValoriaIntegratedWorld>();
   Require(city&&host&&host.WorldRoot&&city.PlayerCamera,"Actual integrated game is missing");
   var sourceTerrain=Object.FindObjectsByType<Terrain>(FindObjectsInactive.Include,
     FindObjectsSortMode.None).First(t=>!t.transform.IsChildOf(host.WorldRoot.transform));
   var sourceData=sourceTerrain.terrainData;
   var left=city.Parcels.LeftBuilt;var right=city.Parcels.RightBuilt;
   var nodes=host.WorldRoot.GetComponentsInChildren<WorldHotspot>(true)
      .ToDictionary(n=>n,n=>n.transform.position);
   var report=new Report{source_commit=Environment.GetEnvironmentVariable("GITHUB_SHA"),
     source_glb_sha256=b.output_sha256,original_scene_sha256=hash,art_approved=false,
     physical_mobile_tested=false,trial_scene=Target+"/ValoriaOwnerCliffsCandidate.unity",
     images=new[]{"owner-citycliffs-before-wide.png","owner-citycliffs-after-wide.png",
                  "owner-citycliffs-before-portrait.png","owner-citycliffs-after-portrait.png",
                  "owner-citycliffs-before-river.png","owner-citycliffs-after-river.png"},
     limitations=new[]{"This is an isolated visual art source candidate, not ART_APPROVED.",
       "Main playable scene and real user saves are untouched.",
       "Native player I-II and physical device QA required before integrating."}};
   city.Apply(new PlayerState{BastionLevel=2,SawmillLevel=1,BarracksLevel=1});
   CaptureSet(city.PlayerCamera,"before");
   var rock=new GameObject("Measured CC0 Holotna river escarpment / owner V19");
   rock.transform.SetParent(city.transform,true);
   rock.transform.SetPositionAndRotation(Vector3.zero,Quaternion.identity);
   using(var loader=new GltfImport(null,new UninterruptedDeferAgent())){
    Require(await loader.Load(new Uri(Path.GetFullPath(Source)).AbsoluteUri,
      new ImportSettings{GenerateMipMaps=true,AnisotropicFilterLevel=4}),
      "Real Blender geological GLB load failed");
    Require(await loader.InstantiateMainSceneAsync(rock.transform),"Blender GLB could not instantiate");
   }
   PersistAll(rock,report);
   var rs=rock.GetComponentsInChildren<MeshRenderer>(true);
   Require(rs.All(x=>Mathf.Abs(x.bounds.center.x)>18),
     "Protected city bridge or actual building access occupied");
   var all=rs[0].bounds;foreach(var r in rs.Skip(1))all.Encapsulate(r.bounds);
   Require(all.min.x< -68&&all.max.x>68&&all.min.y> -10&&all.max.y<12,
     "Measured river-bank scale/Unity axes differ from source");
   CaptureSet(city.PlayerCamera,"after");
   report.terrain_preserved=sourceData==sourceTerrain.terrainData;
   report.hotspots_preserved=nodes.All(k=>k.Key&&k.Key.transform.position==k.Value);
   report.gameplay_plots_preserved=city.Parcels.LeftBuilt==left&&city.Parcels.RightBuilt==right;
   Require(report.terrain_preserved&&report.hotspots_preserved&&report.gameplay_plots_preserved,
     "A visual-only rock donor modified gameplay");
   city.Apply(new PlayerState());
   AssetDatabase.SaveAssets();
   Require(EditorSceneManager.SaveScene(scene,report.trial_scene),"Isolated candidate scene save failed");
   report.original_scene_byte_preserved=Hash(Live)==hash;
   Require(report.original_scene_byte_preserved,"Canonical game scene unexpectedly changed");
   File.WriteAllText(Path.Combine(Ev,"owner-city-cliffs-v19-unity.json"),
     JsonUtility.ToJson(report,true));
   Debug.Log("VALORIA_REFERENCE_CLEAN_OWNER_CLIFFS_COMPLETE / SOURCE_ONLY_CANDIDATE_NOT_APPROVED");
   EditorApplication.Exit(0);
  }catch(Exception e){Debug.LogException(e);
   File.WriteAllText(Path.Combine(Ev,"owner-city-cliffs-v19-failure.txt"),e.ToString());
   EditorApplication.Exit(1);}
 }
}
}
#endif
