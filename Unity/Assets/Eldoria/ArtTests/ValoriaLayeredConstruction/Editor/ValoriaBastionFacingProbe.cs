// Read-only actual SHARP player-camera facade direction audit of sculpted Source08.
using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using Eldoria.Presentation;
namespace Eldoria.ValoriaLayered.Editor {
 public static class ValoriaBastionFacingProbe {
  const string Root="Assets/Eldoria/ArtTests/ValoriaLayeredConstruction/";
  static readonly string Out=Path.GetFullPath(Path.Combine(UnityEngine.Application.dataPath,"..","..","docs/evidence/valoria-layered-construction-20261016/entry-zone-v18"));
  [Serializable] public class MaterialFace {
   public string name;public int triangles,front_facing_triangles,back_facing_triangles,in_frustum_triangles;
   public float front_facing_area,back_facing_area;public int visible_banner_triangles;
   public float min_screen_x,max_screen_x,min_screen_y,max_screen_y;
  }
  [Serializable] public class Evidence {
   public string scene,verdict,kind;public Vector3 camera_world_position,camera_world_forward,keep_center,keep_to_camera,keep_world_forward;
   public float fov;public int mesh_objects,triangle_count,heraldry_front,heraldry_back,heraldry_in_frustum;
   public MaterialFace[] material_faces;
   public bool same_picking_camera,source_geometry_modified,art_approved,canonical_modified;
  }
  [MenuItem("Eldoria/Valoria/Measure actual player's view of keep heraldry")]
  public static void Build(){
   try{Run();Debug.Log("VALORIA_V18_REAL_FACADE_FACING_DIAGNOSTIC_COMPLETE");EditorApplication.Exit(0);}
   catch(Exception ex){Debug.LogError(ex);EditorApplication.Exit(1);}
  }
  static void Check(bool yes,string msg){if(!yes)throw new Exception("BASTION_FACING: "+msg);}
  static void Run(){
   Directory.CreateDirectory(Out);
   EditorSceneManager.OpenScene(Root+"Valoria_SHARP_EntryZoneV18_CANDIDATE.unity",OpenSceneMode.Single);
   var control=UnityEngine.Object.FindObjectsByType<ValoriaParcelPresentation>(FindObjectsSortMode.None).Single();
   var bridge=UnityEngine.Object.FindObjectsByType<ValoriaUprightViewBridge>(FindObjectsSortMode.None).Single();
   control.Home();bridge.Sync();
   var camera=bridge.RenderCamera;
   Check(camera!=null&&!camera.orthographic,"Real SHARP camera not active");
   Check(Mathf.Abs(camera.fieldOfView-44.4228096f)<.0001f,"Historical FOV changed");
   var filters=UnityEngine.Object.FindObjectsByType<MeshFilter>(FindObjectsInactive.Include,FindObjectsSortMode.None)
    .Where(f=>f.gameObject.activeInHierarchy&&f.GetComponent<Renderer>()?.enabled==true&&f.name.Contains("ENTRY_V18_SCULPTED_BASTION_BastionKeep")).ToArray();
   Check(filters.Length==1,"Exactly one actual new Bastion keep renderer is required");
   var filter=filters[0];var mesh=filter.sharedMesh;var r=filter.GetComponent<Renderer>();
   Check(mesh!=null&&r!=null,"No valid actual keep geometry");
   var report=new Evidence{
    scene=Root+"Valoria_SHARP_EntryZoneV18_CANDIDATE.unity",
    kind="Read-only native Unity imported actual model triangle facing counts, NOT Blender preview inference",
    fov=camera.fieldOfView,camera_world_position=camera.transform.position,camera_world_forward=camera.transform.forward,
    keep_center=r.bounds.center,keep_to_camera=(camera.transform.position-r.bounds.center).normalized,
    keep_world_forward=filter.transform.forward,
    mesh_objects=filters.Length,triangle_count=mesh.triangles.Length/3,
    same_picking_camera=Vector2.Distance(camera.WorldToViewportPoint(r.bounds.center),control.ProductionCamera.WorldToViewportPoint(r.bounds.center))<.0001f,
    source_geometry_modified=false,art_approved=false,canonical_modified=false
   };
   var vv=mesh.vertices;var localToWorld=filter.transform.localToWorldMatrix;
   var mats=r.sharedMaterials;var details=new List<MaterialFace>();
   for(int sm=0;sm<mesh.subMeshCount;sm++){
    string name=sm<mats.Length&&mats[sm]!=null?mats[sm].name:"NO_MATERIAL_"+sm;
    var item=new MaterialFace{name=name,min_screen_x=10,max_screen_x=-10,min_screen_y=10,max_screen_y=-10};
    var ids=mesh.GetIndices(sm);
    for(int i=0;i+2<ids.Length;i+=3){
     var a=localToWorld.MultiplyPoint3x4(vv[ids[i]]);
     var b=localToWorld.MultiplyPoint3x4(vv[ids[i+1]]);
     var c=localToWorld.MultiplyPoint3x4(vv[ids[i+2]]);
     var cross=Vector3.Cross(b-a,c-a);
     if(cross.sqrMagnitude<.0000001f)continue;
     var center=(a+b+c)/3;
     var vp=camera.WorldToViewportPoint(center);
     bool inside=vp.z>0&&vp.x>=0&&vp.x<=1&&vp.y>=0&&vp.y<=1;
     float dot=Vector3.Dot(cross.normalized,(camera.transform.position-center).normalized);
     float area=cross.magnitude*.5f;
     item.triangles++;
     if(dot>0)item.front_facing_triangles++;
     else item.back_facing_triangles++;
     if(dot>0)item.front_facing_area+=area;else item.back_facing_area+=area;
     if(inside){
      item.in_frustum_triangles++;item.min_screen_x=Mathf.Min(item.min_screen_x,vp.x);
      item.max_screen_x=Mathf.Max(item.max_screen_x,vp.x);
      item.min_screen_y=Mathf.Min(item.min_screen_y,vp.y);
      item.max_screen_y=Mathf.Max(item.max_screen_y,vp.y);
      if(dot>0&&name.ToLowerInvariant().Contains("banner"))item.visible_banner_triangles++;
     }
    }
    if(name.ToLowerInvariant().Contains("banner")){
     report.heraldry_front+=item.front_facing_triangles;report.heraldry_back+=item.back_facing_triangles;
     report.heraldry_in_frustum+=item.in_frustum_triangles;
    }
    details.Add(item);
   }
   report.material_faces=details.ToArray();
   report.verdict=report.heraldry_front==0&&report.heraldry_back>0?
    "SOURCE08_BANNER_GEOMETRY_FACES_AWAY_FROM_SHARP_PLAYER_CAMERA":
    report.heraldry_front>0&&report.heraldry_in_frustum>0?
    "BANNER_FRONTS_PLAYER_CAMERA_CHECK_SHADING_OCCLUSION_OR_DISTANCE":
    "BANNER_NOT_FOUND_OR_NOT_IN_VIEW_CHECK_HIERARCHY_CAMERA";
   Check(report.same_picking_camera,"Camera differs from game picking camera");
   File.WriteAllText(Path.Combine(Out,"actual-player-bastion-facing.json"),JsonUtility.ToJson(report,true));
   Debug.Log("REAL_VALORIA_FACING "+report.verdict+" camera="+report.camera_world_position+
    " heroFront="+report.heraldry_front+" heroBack="+report.heraldry_back+" heroInside="+report.heraldry_in_frustum);
  }
 }
}
