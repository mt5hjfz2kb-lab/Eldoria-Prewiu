// Eldoria / Valoria: real renderer influence on a COPY of the proven playable V3.
// This is not an art score or a substitute for physical mobile / play-mode QA.
using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Eldoria.Presentation;
namespace Eldoria.ValoriaLayered.Editor {
  public static class ValoriaBastionVisibilityGate {
    const string Root="Assets/Eldoria/ArtTests/ValoriaLayeredConstruction/";
    const string Candidate=Root+"Valoria_SHARP_Entry_BastionVisibility_CANDIDATE.unity";
    const string Prefab=Root+"GeneratedEntryV13/EntryZoneV1.prefab";
    static readonly string Evidence=Path.GetFullPath(Path.Combine(Application.dataPath,"..","..",
      "docs/evidence/valoria-layered-construction-20261016/bastion-source08-visibility"));
    [Serializable] public class EvidenceResult {
      public string scene,method,source_prefab,source_player_scene,verdict;
      public int bastion_renderers,disabled_previous_v12_roots,changed_pixels,changed_pixels_upper_band,
        changed_pixels_roi,width,height;
      public int pixel_bbox_min_x,pixel_bbox_min_y,pixel_bbox_max_x,pixel_bbox_max_y;
      public float fov;public bool same_presenter_camera,canonical_modified,mobile_device_certified,art_approved;
    }
    static void Check(bool condition,string reason){if(!condition)throw new Exception("BASTION_VISIBILITY: "+reason);}
    static bool DescendsFromBastion(Transform t){
      while(t!=null){if(t.name.StartsWith("ENTRY_V13_EXISTING_BASTION_"))return true;t=t.parent;}
      return false;
    }
    static Color32[] Capture(Camera camera,string filename,int w,int h){
      var oldTarget=camera.targetTexture;var oldActive=RenderTexture.active;float oldAspect=camera.aspect;
      var rt=new RenderTexture(w,h,24){antiAliasing=4};
      var bitmap=new Texture2D(w,h,TextureFormat.RGB24,false);
      try{
        camera.aspect=(float)w/h;camera.targetTexture=rt;camera.Render();camera.Render();
        RenderTexture.active=rt;bitmap.ReadPixels(new Rect(0,0,w,h),0,0);bitmap.Apply();
        File.WriteAllBytes(Path.Combine(Evidence,filename),bitmap.EncodeToPNG());
        return bitmap.GetPixels32();
      }finally{
        camera.targetTexture=oldTarget;camera.aspect=oldAspect;RenderTexture.active=oldActive;
        rt.Release();UnityEngine.Object.DestroyImmediate(rt);UnityEngine.Object.DestroyImmediate(bitmap);
      }
    }
    [MenuItem("Eldoria/Valoria/Verify Source08 Bastion visibility in playable frame")]
    public static void Build(){
      try{Run();Debug.Log("VALORIA_BASTION_SOURCE08_VISIBILITY_NATIVE_COMPLETE");EditorApplication.Exit(0);}
      catch(Exception e){Debug.LogError(e);EditorApplication.Exit(1);}
    }
    static void Run(){
      Directory.CreateDirectory(Evidence);
      var scene=EditorSceneManager.OpenScene(Root+"Valoria_SHARP_Entry_PlayableV3.unity",OpenSceneMode.Single);
      var transforms=UnityEngine.Object.FindObjectsByType<Transform>(FindObjectsInactive.Include,FindObjectsSortMode.None);
      var frame=transforms.Single(t=>t.name.StartsWith("EXPERIMENTAL / upright native-metre"));
      var originalV12=transforms.Where(t=>t.name.StartsWith("ENTRY V12 /")).ToArray();
      Check(originalV12.Length==1,"Expected one original V12 renderer root on V3");
      foreach(var old in originalV12)old.gameObject.SetActive(false);
      var prefab=AssetDatabase.LoadAssetAtPath<GameObject>(Prefab);
      Check(prefab!=null,"Original imported Bastion Source08/V13 prefab absent");
      var obj=PrefabUtility.InstantiatePrefab(prefab,frame) as GameObject;
      Check(obj!=null,"Unable to instantiate already-imported V13 original-source model");
      obj.name="Bastion Source08 original geometry / inherited V13 materials / visibility verification";
      obj.transform.localPosition=Vector3.zero;obj.transform.localRotation=Quaternion.identity;
      obj.transform.localScale=Vector3.one;
      var renderers=obj.GetComponentsInChildren<Renderer>(true).Where(r=>DescendsFromBastion(r.transform)).ToArray();
      Check(renderers.Length==9,"Expected all nine original Source08 Bastion module renderers; found "+renderers.Length);
      foreach(var r in renderers){r.gameObject.SetActive(true);r.enabled=true;}
      // Hide only the provisional visible greybox; KEEP its collider and the actual hotspot.
      var bastionProxy=transforms.Single(t=>t.name=="InteractiveProxy_Bastion");
      foreach(var r in bastionProxy.GetComponentsInChildren<Renderer>(true))r.enabled=false;
      var control=UnityEngine.Object.FindFirstObjectByType<ValoriaParcelPresentation>();
      var cameraBridge=UnityEngine.Object.FindFirstObjectByType<ValoriaUprightViewBridge>();
      Check(control!=null&&cameraBridge!=null,"Existing presentation/camera unavailable");
      control.Home();cameraBridge.Sync();
      var camera=cameraBridge.RenderCamera;
      Check(camera!=null&&!camera.orthographic,"Perspective SHARP camera not active");
      var target=bastionProxy.GetComponent<Collider>();
      Check(target!=null,"Game's Bastion hotspot/collider disappeared");
      var pixel=camera.WorldToViewportPoint(target.bounds.center);
      var presenterPixel=control.ProductionCamera.WorldToViewportPoint(target.bounds.center);
      bool same=Vector2.Distance(pixel,presenterPixel)<.0001f;
      Check(same,"Visual camera and actual player picking camera diverge");
      const int w=1536,h=1024;
      var on=Capture(camera,"bastion-renderers-ON-home.png",w,h);
      foreach(var r in renderers)r.enabled=false;
      var off=Capture(camera,"bastion-renderers-OFF-home.png",w,h);
      foreach(var r in renderers)r.enabled=true;
      int n=0,upper=0,roi=0,minx=w,miny=h,maxx=-1,maxy=-1;
      // Unity pixel buffers are bottom-left oriented; target top 32% is y >= 68% of h.
      for(int y=0;y<h;y++)for(int x=0;x<w;x++){
        int i=y*w+x;
        int diff=Math.Abs(on[i].r-off[i].r)+Math.Abs(on[i].g-off[i].g)+Math.Abs(on[i].b-off[i].b);
        if(diff<30)continue;
        n++;if(y>=h*0.67)upper++;
        if(x>=608&&x<=1265&&y>=h-270&&y<=h-64)roi++;
        minx=Math.Min(minx,x);miny=Math.Min(miny,y);maxx=Math.Max(maxx,x);maxy=Math.Max(maxy,y);
      }
      var report=new EvidenceResult{
        scene=Candidate,source_player_scene=Root+"Valoria_SHARP_Entry_PlayableV3.unity",
        source_prefab=Prefab,method="Same copied real playable-V3 scene, SHARP player camera, only nine original Bastion renderer enabled flags flipped; same lighting and resolution",
        bastion_renderers=renderers.Length,disabled_previous_v12_roots=originalV12.Length,
        changed_pixels=n,changed_pixels_upper_band=upper,changed_pixels_roi=roi,
        width=w,height=h,pixel_bbox_min_x=maxx<0?-1:minx,pixel_bbox_min_y=maxy<0?-1:miny,
        pixel_bbox_max_x=maxx,pixel_bbox_max_y=maxy,fov=camera.fieldOfView,
        same_presenter_camera=same,canonical_modified=false,mobile_device_certified=false,art_approved=false,
        verdict=n>=300&&upper>=150?"SOURCE08_BASTION_VISIBLE_IN_COPIED_PLAYER_CAMERA_NOT_ART_APPROVED":"BASTION_VISIBILITY_NOT_PROVEN_STOP_ART_ITERATION"
      };
      // A candidate copy, never the V3 source, can be inspected later in actual PlayMode.
      var runtime=UnityEngine.Object.FindFirstObjectByType<ValoriaEntryExperimentalRuntimeV3>();
      Check(runtime!=null,"V3 existing gameplay runtime missing");
      runtime.ExperimentalScene=Candidate;
      Check(EditorSceneManager.SaveScene(scene,Candidate,true),"Cannot save isolated candidate");
      File.WriteAllText(Path.Combine(Evidence,"visibility-result.json"),JsonUtility.ToJson(report,true));
      Check(n>=300&&upper>=150,"Bastion art is not visibly driving upper player image: "+n+" pixels / "+upper+" upper");
      Debug.Log("VALORIA_BASTION_VISIBLE pixels="+n+" upper="+upper+" roi="+roi+" FOV="+camera.fieldOfView);
    }
  }
}
