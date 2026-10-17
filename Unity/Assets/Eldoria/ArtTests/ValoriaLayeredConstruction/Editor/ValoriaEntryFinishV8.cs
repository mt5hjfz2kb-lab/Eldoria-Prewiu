using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using Eldoria.Presentation;

namespace Eldoria.ValoriaLayered.Editor {
  // Isolated V8 finish pass: retains every V7 source mesh and gameplay collider.
  // Routes and river are never reauthored by replacing scene/camera/gateway.
  public static class ValoriaEntryFinishV8 {
    const string Root = "Assets/Eldoria/ArtTests/ValoriaLayeredConstruction/";
    const string AssetRoot = Root + "GeneratedEntryV8";
    const string Source = Root + "Valoria_SHARP_EntryZoneV7_CANDIDATE.unity";
    const string Destination = Root + "Valoria_SHARP_EntryZoneV8_CANDIDATE.unity";
    static readonly string Evidence=Path.GetFullPath(Path.Combine(UnityEngine.Application.dataPath,"..","..","docs/evidence/valoria-layered-construction-20261016/entry-zone-v8"));
    static Camera camera;
    static void Require(bool ok,string message){if(!ok)throw new Exception("VALORIA_V8: "+message);}
    [Serializable] class Record {
      public string scene=Destination,source=Source,art_status="CANDIDATE_REQUIRES_INDEPENDENT_VISUAL_REVIEW",gameplay="SOURCE_COLLIDERS_ONLY",water_material="",illumination="NEUTRAL_COASTAL_DAYLIGHT";
      public int source_targets=0,route_probes=0,route_passes=0,certified_steps=0,disabled_beige_renderers=0,generated_paving_triangles=0,source_colliders_before=0,source_colliders_after=0,footings_samples=0;
      public float footings_min_y=0,footings_max_gap=0,footings_min_gap=0,home_fov=0;
      public bool water_adjusted=false,gate_lens_preserved=false,pedestrian_clear=false,source_objects_untouched=true,bridge_footings_changed=false,canonical_modified=false,mobile_profiled=false;
    }
    static void Shot(string name,int width=1536,int height=1024){
      var old=camera.targetTexture;var prev=RenderTexture.active;float aspect=camera.aspect;
      var rt=new RenderTexture(width,height,24){antiAliasing=4};var tex=new Texture2D(width,height,TextureFormat.RGB24,false);
      try { camera.aspect=(float)width/height; camera.targetTexture=rt;camera.Render();camera.Render();
        RenderTexture.active=rt;tex.ReadPixels(new Rect(0,0,width,height),0,0);tex.Apply();
        File.WriteAllBytes(Path.Combine(Evidence,name+".png"),tex.EncodeToPNG());
      }finally{camera.targetTexture=old;camera.aspect=aspect;RenderTexture.active=prev;rt.Release();UnityEngine.Object.DestroyImmediate(rt);UnityEngine.Object.DestroyImmediate(tex);}
    }
    static float N(float x){return Mathf.Sin(x*3.413f+.7f);}
    static void Stone(List<Vector3> v,List<int> t,List<Vector2> uv,Transform frame,float x,float z,float bottom,float width,float length,float thick){
      float a=x-width*.5f,b=x+width*.5f,c=z-length*.5f,d=z+length*.5f;
      Vector3[] p={new Vector3(a,bottom,c),new Vector3(a,bottom,d),new Vector3(b,bottom,d),new Vector3(b,bottom,c),
                   new Vector3(a,bottom+thick,c),new Vector3(a,bottom+thick,d),new Vector3(b,bottom+thick,d),new Vector3(b,bottom+thick,c)};
      int offset=v.Count;foreach(var q in p){v.Add(frame.InverseTransformPoint(q));uv.Add(new Vector2(q.x*.22f,q.z*.22f));}
      int[] faces={4,5,6,4,6,7,0,2,1,0,3,2,0,1,5,0,5,4,1,2,6,1,6,5,2,3,7,2,7,6,3,0,4,3,4,7};
      foreach(int f in faces)t.Add(offset+f);
    }
    static int CobbleRoute(Transform frame,Transform route,Material stone,int side){
      var box=route.GetComponent<Collider>();Require(box!=null,"Route source collider missing "+route.name);
      var b=box.bounds;Require(b.size.x>5f&&b.size.z>1.7f,"Unexpected route bounding box "+b);
      var vertices=new List<Vector3>();var indices=new List<int>();var tex=new List<Vector2>();
      float sx=b.center.x,sz=b.center.z,top=b.max.y;
      int cols=15,rows=5;
      for(int i=0;i<cols;i++)for(int j=0;j<rows;j++){
        float u=((i+.5f)/cols-.5f)*b.size.x;
        float taper=.60f+.40f*(1f-Mathf.Pow(Mathf.Abs(u)/(b.size.x*.5f),2f));
        float row=(j-(rows-1)/2f);
        float jitter=.045f*N(i*4.3f+j*1.7f+side);
        float z=sz+row*.43f*taper+jitter;
        if((i==0||i==cols-1)&&(j==0||j==rows-1))continue; // irregular tapered edge
        float x=sx+u+.035f*N(i*3+j*11);
        Stone(vertices,indices,tex,frame,x,z,top-.04f,.39f+.03f*N(i+j),.39f,.082f+.017f*N(i*7+j*3));
      }
      var mesh=new Mesh{name="V8_ParcelRoute_SourceColliderPreserved"};
      mesh.indexFormat=UnityEngine.Rendering.IndexFormat.UInt32;
      mesh.SetVertices(vertices);mesh.SetTriangles(indices,0);mesh.SetUVs(0,tex);mesh.RecalculateNormals();mesh.RecalculateBounds();
      var go=new GameObject("ENTRY V8 / "+(side<0?"west":"east")+" stonemason route replaces beige visual");
      go.transform.SetParent(frame,false);
      go.AddComponent<MeshFilter>().sharedMesh=mesh;go.AddComponent<MeshRenderer>().sharedMaterial=stone;
      AssetDatabase.CreateAsset(mesh,AssetRoot+"/V8_Route_"+(side<0?"West":"East")+".asset");
      return indices.Count/3;
    }
    static int Apron(Transform frame,Transform apron,Material stone){
      var c=apron.GetComponent<Collider>();Require(c!=null,"Lower apron collision missing");
      var b=c.bounds;var vertices=new List<Vector3>();var indices=new List<int>();var uvs=new List<Vector2>();
      // Existing central stone street remains visible; use short edge-contact stones only.
      for(int j=0;j<5;j++)for(int side=-1;side<=1;side+=2){
        float x=b.center.x+side*(1.78f+.27f*j/5f),z=b.center.z+(j-2)*.39f;
        Stone(vertices,indices,uvs,frame,x,z,b.max.y-.035f,.33f,.34f,.055f);
      }
      var mesh=new Mesh{name="V8_LowerBridgeApronEdgeContact"};mesh.SetVertices(vertices);mesh.SetTriangles(indices,0);mesh.SetUVs(0,uvs);mesh.RecalculateNormals();
      var obj=new GameObject("ENTRY V8 / lower entry apron interlock");obj.transform.SetParent(frame,false);
      obj.AddComponent<MeshFilter>().sharedMesh=mesh;obj.AddComponent<MeshRenderer>().sharedMaterial=stone;
      AssetDatabase.CreateAsset(mesh,AssetRoot+"/V8_LowerApron.asset");
      return indices.Count/3;
    }
    static void CheckFootings(Transform[] original,Record result) {
      var feet=original.Where(x=>x.name.Contains("ENTRY_SUPPORTED_VALORIA_ARCH_FOOTINGS")).SelectMany(x=>x.GetComponentsInChildren<MeshFilter>(true)).ToArray();
      var ground=original.SelectMany(x=>x.GetComponents<MeshCollider>()).FirstOrDefault(c=>c.name.Contains("ValoriaEntryTerrainV7"));
      Require(feet.Length>0&&ground!=null,"Bridge source or terrain collider absent");
      var samples=feet.SelectMany(f=>f.sharedMesh.vertices.Select(v=>f.transform.TransformPoint(v))).ToArray();
      float min=samples.Min(v=>v.y);float bottom=min+.075f;var endpoints=samples.Where(v=>v.y<=bottom).ToArray();
      float gapMin=float.MaxValue,gapMax=float.MinValue;int n=0;
      foreach(var p in endpoints){
        if(ground.Raycast(new Ray(new Vector3(p.x,8,p.z),Vector3.down),out var hit,30f)){
          float gap=p.y-hit.point.y;
          gapMin=Mathf.Min(gapMin,gap);gapMax=Mathf.Max(gapMax,gap);n++;
        }
      }
      result.footings_samples=n;result.footings_min_y=min;result.footings_min_gap=n>0?gapMin:0;result.footings_max_gap=n>0?gapMax:0;
      // Deliberately leave footings untouched until gaps can be interpreted against true bank samples.
      Require(n>0,"No genuine bank contact sample for bridge footing");
    }
    static void CheckSource(Transform[] source,ValoriaParcelPresentation control,ValoriaUprightViewBridge bridge,Record result) {
      var names=new[]{"TEMPORARY bridge / continuous deck and collider","SUPPORT/RESERVE Puerta · ir al mundo","GREYBOX VPD · L0 main street","GREYBOX certified stair step 6","GREYBOX Bastion hero reservation, silhouette only","SUPPORT/RESERVE VPD · L0 west plot","SUPPORT/RESERVE VPD · L0 east plot"};
      control.Home();camera.aspect=1.5f;bridge.Sync();
      foreach(var name in names){
        var t=source.Single(x=>x.name==name);var c=t.GetComponent<Collider>();Require(c!=null,"Source collider lost "+name);
        Vector3 p=camera.WorldToViewportPoint(c.bounds.center),pc=control.ProductionCamera.WorldToViewportPoint(c.bounds.center);
        Require(p.z>0&&p.x>0&&p.x<1&&p.y>0&&p.y<1,"Target leaves home camera "+name);
        Require(Vector2.Distance(p,pc)<.0001f,"Picking camera mismatch "+name);
        Require(Physics.RaycastAll(control.ProductionCamera.ViewportPointToRay(pc),500,~0,QueryTriggerInteraction.Collide).Any(h=>h.collider==c),"Lost original target "+name);
        result.source_targets++;
      }
      var steps=source.Where(x=>x.name.StartsWith("GREYBOX certified stair step ")).OrderBy(x=>x.position.z).ToArray();
      Require(steps.Length==12,"Original stairs changed");result.certified_steps=steps.Length;
      int total=0,passes=0;
      void Probe(Vector3 point,string expected){total++;if(Physics.Raycast(point+Vector3.up*.10f,Vector3.down,out var h,.22f,~0,QueryTriggerInteraction.Ignore)&&h.collider.name==expected)passes++;}
      var start=new Vector3(0,.38f,-8.7f);var end=new Vector3(steps[0].position.x,.30f,steps[0].position.z-.26f);
      for(int i=1;i<12;i++)Probe(Vector3.Lerp(start,end,i/12f),"GREYBOX VPD · L0 main street");
      foreach(var step in steps){var b=step.GetComponent<Collider>().bounds;Probe(new Vector3(b.center.x,b.max.y,b.center.z),step.name);}
      for(int i=1;i<6;i++)Probe(Vector3.Lerp(new Vector3(0,.38f,-19.65f),new Vector3(0,.38f,-9.35f),i/6f),"TEMPORARY bridge / continuous deck and collider");
      result.route_probes=total;result.route_passes=passes;
      result.pedestrian_clear=!Physics.SphereCast(start+Vector3.up*.8f,.3f,(end-start).normalized,out var block,(end-start).magnitude,~0,QueryTriggerInteraction.Ignore);
      Require(total==28&&passes==28&&result.pedestrian_clear,"Protected road/stair/bridge circulation regression "+passes+"/"+total);
      result.home_fov=camera.fieldOfView;result.gate_lens_preserved=Mathf.Abs(result.home_fov-44.4228096f)<.0001f;
      Require(result.gate_lens_preserved,"Native camera lens changed");
    }
    [MenuItem("Eldoria/Valoria/Build V8 local entry finish candidate")]
    public static void Build(){try{Run();Debug.Log("VALORIA_ENTRY_FINISH_V8_NATIVE_COMPLETE");EditorApplication.Exit(0);}catch(Exception e){Debug.LogError(e);EditorApplication.Exit(1);}}
    static void Run(){
      Directory.CreateDirectory(Evidence);Directory.CreateDirectory(AssetRoot);AssetDatabase.Refresh();
      Require(File.Exists(Source),"Certified V7 scene missing");
      var scene=EditorSceneManager.OpenScene(Source,OpenSceneMode.Single);
      var original=UnityEngine.Object.FindObjectsByType<Transform>(FindObjectsInactive.Include,FindObjectsSortMode.None);
      var frame=original.Single(t=>t.name.StartsWith("EXPERIMENTAL / upright native-metre"));
      var control=UnityEngine.Object.FindFirstObjectByType<ValoriaParcelPresentation>();
      var bridge=UnityEngine.Object.FindFirstObjectByType<ValoriaUprightViewBridge>();
      Require(control!=null&&bridge!=null,"Original camera/presentation bridge missing");
      camera=bridge.RenderCamera;control.Home();bridge.Sync();
      var result=new Record();
      result.source_colliders_before=UnityEngine.Object.FindObjectsByType<Collider>(FindObjectsInactive.Include,FindObjectsSortMode.None).Length;
      Shot("entry-BEFORE-home");Shot("entry-BEFORE-portrait",390,844);
      var roadMat=UnityEngine.Object.FindObjectsByType<Renderer>(FindObjectsInactive.Include,FindObjectsSortMode.None)
        .SelectMany(r=>r.sharedMaterials).FirstOrDefault(m=>m!=null&&m.name.Contains("ENTRY_road_masonry_PBR"));
      Require(roadMat!=null,"Existing source PBR stone material not found");
      string[] routes={"GREYBOX VPD · master east route","GREYBOX VPD · master west route"};
      foreach(string name in routes){
        var t=original.Single(x=>x.name==name);
        var renderers=t.GetComponentsInChildren<Renderer>(true);Require(renderers.Length>0,"Expected beige visual is absent "+name);
        foreach(var r in renderers){if(r.enabled){r.enabled=false;result.disabled_beige_renderers++;}}
        result.generated_paving_triangles+=CobbleRoute(frame,t,roadMat,name.Contains("west")?-1:1);
      }
      var lower=original.Single(x=>x.name=="GREYBOX VPD · lower entry apron");
      foreach(var r in lower.GetComponentsInChildren<Renderer>(true)){if(r.enabled){r.enabled=false;result.disabled_beige_renderers++;}}
      result.generated_paving_triangles+=Apron(frame,lower,roadMat);
      var water=original.SelectMany(t=>t.GetComponents<Renderer>()).FirstOrDefault(r=>r.name.Contains("RIVER_CONTINUOUS_RIPPLED_WATER"));
      Require(water!=null&&water.sharedMaterial!=null,"Real V7 water source not found");
      var riverMat=new Material(water.sharedMaterial){name="ENTRY_V8_natural_river_muted_blue_green"};
      Require(riverMat.HasProperty("_BaseColor"),"Expected glTF/URP water color not present");
      riverMat.SetColor("_BaseColor",new Color(.23f,.39f,.40f,1f));
      if(riverMat.HasProperty("_Smoothness"))riverMat.SetFloat("_Smoothness",.72f);
      if(riverMat.HasProperty("_Metallic"))riverMat.SetFloat("_Metallic",.06f);
      AssetDatabase.CreateAsset(riverMat,AssetRoot+"/V8_RiverMaterial.mat");
      water.sharedMaterial=riverMat;result.water_adjusted=true;result.water_material=riverMat.name;
      var sun=UnityEngine.Object.FindObjectsByType<Light>(FindObjectsInactive.Include,FindObjectsSortMode.None).FirstOrDefault(x=>x.type==LightType.Directional);
      if(sun!=null){sun.color=new Color(1f,.94f,.86f);sun.intensity=1.14f;sun.transform.rotation=Quaternion.Euler(50f,-41f,0f);}
      RenderSettings.ambientSkyColor=new Color(.43f,.49f,.53f);
      RenderSettings.ambientEquatorColor=new Color(.33f,.35f,.34f);
      RenderSettings.ambientGroundColor=new Color(.20f,.20f,.18f);
      Physics.SyncTransforms();
      CheckFootings(original,result);
      CheckSource(original,control,bridge,result);
      control.Home();bridge.Sync();Shot("entry-AFTER-home");Shot("entry-AFTER-portrait",390,844);
      control.Pan(new Vector2(41.6667f,0));bridge.Sync();Shot("entry-AFTER-pan-left");
      control.Home();control.Zoom(-2);bridge.Sync();Shot("entry-AFTER-zoom-in");
      control.Home();bridge.Sync();
      result.source_colliders_after=UnityEngine.Object.FindObjectsByType<Collider>(FindObjectsInactive.Include,FindObjectsSortMode.None).Length;
      Require(result.source_colliders_after==result.source_colliders_before,"Source collider count changed");
      AssetDatabase.SaveAssets();
      Require(EditorSceneManager.SaveScene(scene,Destination,true),"Cannot preserve V8 candidate scene");
      File.WriteAllText(Path.Combine(Evidence,"entry-native.json"),JsonUtility.ToJson(result,true));
    }
  }
}
