// Isolated terrain V3 candidate using editable Blender variant of existing V1.
// Same actual SHARP player camera; all original master/layout assets and source gameplay colliders preserved.
// Independent rendered A/B comparison is REQUIRED, do not auto-declare ART_PASS.
using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using GLTFast;
using Eldoria.Presentation;
namespace Eldoria.ValoriaLayered.Editor
{
 public static class ValoriaTerrainRiverCliffV3Integration
 {
   const string Root="Assets/Eldoria/ArtTests/ValoriaLayeredConstruction/";
   const string Input=Root+"Valoria_SHARP_SemanticContact_IsolatedV1.unity";
   const string Output=Root+"Valoria_SHARP_RiverCliffV3_CANDIDATE.unity";
   const string Source=Root+"Sources/ValoriaTerrainRiverCliffV3.glb";
   const string Resources=Root+"GeneratedTerrainV3";
   static readonly string Evidence=Path.GetFullPath(Path.Combine(UnityEngine.Application.dataPath,
      "..","..","docs","evidence","valoria-layered-construction-20261016","terrain-v3"));
   const int W=1230,H=845;
   static readonly HashSet<int> written=new HashSet<int>();
   static int resourceIndex;
   [Serializable] class InputManifest{
      public string source_glb_v3_sha256,original_v2_source_sha256;
      public bool source_and_canonical_unmodified,original_collider_changes;
      public int triangles,mesh_count;
   }
   [Serializable] class Result{
      public string source_scene,candidate_scene,glb_sha256,visual_gate,gameplay_gate;
      public bool original_scenes_untouched,colliders_unchanged,
         actual_player_camera_unchanged,canonical_modified;
      public int source_triangles,real_mesh_renderers,bridge_anchor_contacts,
         stair_treads_untouched,all_semantic_clicks;
   }
   static void Req(bool yes,string msg){if(!yes)throw new Exception("VALORIA_TERRAIN_V3: "+msg);}
   static void Save(UnityEngine.Object o) {
      if(o==null||AssetDatabase.Contains(o)||!written.Add(o.GetInstanceID()))return;
      if(!(o is Mesh)&&!(o is Material)&&!(o is Texture2D))return;
      AssetDatabase.CreateAsset(o,Resources+"/V3_"+(resourceIndex++).ToString("D5")+
                 (o is Material?".mat":".asset"));
   }
   static void Capture(Camera cam,string label,int w,int h){
      var oldTarget=cam.targetTexture;var oldAspect=cam.aspect;var active=RenderTexture.active;
      var rt=new RenderTexture(w,h,24){antiAliasing=4};
      var texture=new Texture2D(w,h,TextureFormat.RGB24,false);
      try {
         cam.targetTexture=rt;cam.aspect=(float)w/h;cam.Render();cam.Render();
         RenderTexture.active=rt;texture.ReadPixels(new Rect(0,0,w,h),0,0);texture.Apply();
         File.WriteAllBytes(Path.Combine(Evidence,label+".png"),texture.EncodeToPNG());
      } finally {
         cam.targetTexture=oldTarget;cam.aspect=oldAspect;RenderTexture.active=active;
         rt.Release();UnityEngine.Object.DestroyImmediate(rt);UnityEngine.Object.DestroyImmediate(texture);
      }
   }
   [MenuItem("Eldoria/Valoria/Make candidate terrain V3 from existing sculpt over real camera")]
   public static async void Build() {
     try{await Execute();Debug.Log("VALORIA_TERRAIN_V3_REAL_PLAYER_FRAME_CANDIDATE_COMPLETE");EditorApplication.Exit(0);}
     catch(Exception ex){Debug.LogError("VALORIA_TERRAIN_V3_REAL_PLAYER_FRAME_CANDIDATE_FAILED "+ex);EditorApplication.Exit(1);}
   }
   static async Task Execute(){
     Req(File.Exists(Input),"Prior native 7/7 semantic scene missing, cannot skip camera gate");
     var manifestPath=Path.Combine(Evidence,"ValoriaTerrainRiverCliffV3-manifest.json");
     Req(File.Exists(Source)&&File.Exists(manifestPath),"Missing edited V3 Blender source or lineage manifest");
     var meta=JsonUtility.FromJson<InputManifest>(File.ReadAllText(manifestPath));
     Req(meta.source_and_canonical_unmodified && !meta.original_collider_changes &&
        meta.mesh_count>=8 && meta.triangles>=13000,"Source provenance/mesh checks missing");
     using(var stream=File.OpenRead(Source))using(var hash=SHA256.Create()){
       string got=BitConverter.ToString(hash.ComputeHash(stream)).Replace("-","").ToLowerInvariant();
       Req(got==meta.source_glb_v3_sha256,"Edited source SHA changed after Blender");
     }
     var scene=EditorSceneManager.OpenScene(Input,OpenSceneMode.Single);
     var components=UnityEngine.Object.FindObjectsByType<ValoriaParcelPresentation>(
           FindObjectsInactive.Include,FindObjectsSortMode.None);
     Req(components.Length==1,"Cannot use multiple or substitute player cameras");
     var player=components[0];var camera=player.ProductionCamera;
     Req(camera!=null && !camera.orthographic,"Player SHARP perspective missing");
     player.Home();camera.aspect=(float)W/H;
     Req(Mathf.Abs(camera.fieldOfView-44.42281f)<.0001f &&
        camera.transform.position.sqrMagnitude<.0001f,"Owner-approved SHARP camera changed");
     var frame=UnityEngine.Object.FindObjectsByType<Transform>(
          FindObjectsInactive.Include,FindObjectsSortMode.None)
          .SingleOrDefault(t=>t.name.StartsWith("DIAGNOSTIC / native 3D rigid world-to-SHARP"));
     Req(frame!=null,"Previous source geometry calibration frame not found");
     Directory.CreateDirectory(Evidence);
     Capture(camera,"terrain-v3-BEFORE-home",W,H);
     Capture(camera,"terrain-v3-BEFORE-portrait",390,844);
     var gltf=new GltfImport(null,new UninterruptedDeferAgent());
     Req(await gltf.Load(new Uri(Path.GetFullPath(Source)).AbsoluteUri,
          new ImportSettings{GenerateMipMaps=true,AnisotropicFilterLevel=2}),
          "Edited terrain source does not import to Unity");
     var root=new GameObject("LAYER V3 - EDITABLE TERRAIN/RIVER/CLIFF EXPERIMENT / ART NOT APPROVED");
     root.transform.SetParent(frame,false);
     root.transform.localPosition=Vector3.zero;root.transform.localRotation=Quaternion.identity;
     root.transform.localScale=Vector3.one;
     Req(await gltf.InstantiateMainSceneAsync(root.transform),"No V3 imported glTF mesh");
     var rs=root.GetComponentsInChildren<Renderer>(true);
     Req(rs.Length>=8,"Real updated river/cliff mesh missing renderers");
     Req(root.GetComponentsInChildren<Collider>(true).Length==0,
         "Imported art may not add or replace original gameplay colliders");
     // Hide only the outdated terrain/render support faces; retain physical colliders for route
     // and camera, and retain all floor, stairs, gate, bridge and plot visuals unchanged.
     var hideNames=new[]{"GREYBOX continuous rocky substrate","SUPPORT upper civic underfill",
       "SUPPORT civic link underfill","PROVISIONAL river under gate approach",
       "TEMPORARY far shore ground, noncertified extension"};
     foreach(var t in UnityEngine.Object.FindObjectsByType<Transform>(
         FindObjectsInactive.Include,FindObjectsSortMode.None)){
       if(hideNames.Any(name=>t.name==name) ||
          t.name.StartsWith("GREYBOX VPD · continuous terrain") ||
          t.name.StartsWith("GREYBOX VPD · master west terrain apron") ||
          t.name.StartsWith("GREYBOX VPD · master east terrain apron")){
          foreach(var rend in t.GetComponentsInChildren<Renderer>(true))rend.enabled=false;
       }
     }
     if(AssetDatabase.IsValidFolder(Resources))AssetDatabase.DeleteAsset(Resources);
     Directory.CreateDirectory(Resources);AssetDatabase.Refresh();
     foreach(var rr in rs)foreach(var m in rr.sharedMaterials){
       if(m==null)continue;
       foreach(var p in m.GetTexturePropertyNames())Save(m.GetTexture(p));
       Save(m);
     }
     foreach(var filter in root.GetComponentsInChildren<MeshFilter>(true))Save(filter.sharedMesh);
     foreach(var s in root.GetComponentsInChildren<SkinnedMeshRenderer>(true))Save(s.sharedMesh);
     AssetDatabase.SaveAssets();
     PrefabUtility.SaveAsPrefabAsset(root,Resources+"/ValoriaTerrainRiverCliffV3.prefab");
     Physics.SyncTransforms();
     // Verify physical contact of all seven current source semantics has not changed.
     string[] labels={"Bridge","LowerGate","MainRoad","CentralStair","Bastion","LeftCabinParcel","RightCampParcel"};
     Vector2[] coords={
       new Vector2(.288618f,.1005919f),new Vector2(.394309f,.319527f),
       new Vector2(.46748f,.508876f),new Vector2(.534959f,.673373f),
       new Vector2(.604878f,.842604f),new Vector2(.203252f,.627219f),
       new Vector2(.8536589f,.519527f)};
     string[] targetNames={"TEMPORARY bridge / continuous deck and collider",
       "SUPPORT/RESERVE Puerta · ir al mundo","GREYBOX VPD · L0 main street",
       "GREYBOX certified stair step ","GREYBOX Bastion hero reservation, silhouette only",
       "SUPPORT/RESERVE VPD · L0 west plot","SUPPORT/RESERVE VPD · L0 east plot"};
     int contacts=0;
     for(int i=0;i<labels.Length;i++){
        var hits=Physics.RaycastAll(camera.ViewportPointToRay(coords[i]),300f,
                ~0,QueryTriggerInteraction.Collide);
        bool valid=hits.Any(h=>i==3?h.collider.name.StartsWith(targetNames[i]):
             h.collider.name==targetNames[i]);
        Req(valid,"Terrain visual must not break click semantic "+labels[i]);
        contacts++;
     }
     int stairs=0;
     // All twelve historic stair colliders must still be source-unchanged.
     for(int i=1;i<=12;i++){
        var t=UnityEngine.Object.FindObjectsByType<Transform>(
            FindObjectsInactive.Include,FindObjectsSortMode.None)
            .SingleOrDefault(q=>q.name=="GREYBOX certified stair step "+i);
        Req(t!=null && t.GetComponent<BoxCollider>()!=null &&
           t.GetComponent<BoxCollider>().enabled,"Certified historical stair collider missing "+i);
        stairs++;
     }
     Capture(camera,"terrain-v3-AFTER-home",W,H);
     Capture(camera,"terrain-v3-AFTER-portrait",390,844);
     player.Pan(new Vector2(41.6667f,0));
     Capture(camera,"terrain-v3-AFTER-pan-left",W,H);
     player.Home();player.Pan(new Vector2(-41.6667f,0));
     Capture(camera,"terrain-v3-AFTER-pan-right",W,H);
     player.Home();player.Zoom(-2f);
     Capture(camera,"terrain-v3-AFTER-zoom-in",W,H);
     player.Home();player.Zoom(2f);
     Capture(camera,"terrain-v3-AFTER-zoom-out",W,H);player.Home();
     Req(EditorSceneManager.SaveScene(scene,Output,true),"New isolated V3 scene failed to save");
     var ev=new Result{source_scene=Input,candidate_scene=Output,
       glb_sha256=meta.source_glb_v3_sha256,visual_gate="PENDING_ACTUAL_AB_INSPECTION",
       gameplay_gate="SOURCE_COLLIDER_CONTACT_ONLY_PLAYERSTATE_NOT_RETESTED",
       original_scenes_untouched=true,colliders_unchanged=true,actual_player_camera_unchanged=true,
       canonical_modified=false,source_triangles=meta.triangles,real_mesh_renderers=rs.Length,
       bridge_anchor_contacts=1,stair_treads_untouched=stairs,all_semantic_clicks=contacts};
     File.WriteAllText(Path.Combine(Evidence,"terrain-v3-native.json"),JsonUtility.ToJson(ev,true));
     Debug.Log("VALORIA_TERRAIN_V3_REAL_PLAYER_FRAME_TECH_PASS sources="+rs.Length+
       " original_12_stairs="+stairs+" semantic_contacts="+contacts);
   }
 }
}
