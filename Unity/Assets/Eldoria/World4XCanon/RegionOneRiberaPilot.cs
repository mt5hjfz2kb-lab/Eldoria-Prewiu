using System;
using System.IO;
using System.Linq;
using UnityEngine;
namespace Eldoria.World4XCanon
{
 // Explicit, reversible art-study capture. Never installed by normal game startup.
 public static class RegionOneRiberaPilot
 {
  public static void Capture(Camera camera, Transform world, Vector3 home, string output)
  {
   var prefab=Resources.Load<GameObject>("World4X/Pilot/RiberaPilot");
   if(!prefab)throw new InvalidOperationException("Missing authored ribera pilot");
   var oldPos=camera.transform.position;var oldRot=camera.transform.rotation;float oldSize=camera.orthographicSize,oldAspect=camera.aspect;var oldRT=camera.targetTexture;var oldActive=RenderTexture.active;
   var prior=world.GetComponentsInChildren<Renderer>(true).Where(r=>r.enabled).ToArray();
   var root=UnityEngine.Object.Instantiate(prefab,world);root.name="Ribera hybrid art study (opt in)";root.layer=world.gameObject.layer;root.transform.position=home;root.transform.rotation=Quaternion.Euler(0,180,0);
   foreach(var r in root.GetComponentsInChildren<Renderer>())r.gameObject.layer=world.gameObject.layer;
   var surfaceCache=new System.Collections.Generic.Dictionary<Material,Material>();
   var studyShader=Shader.Find("Eldoria/RiberaFade");
   foreach(var renderer in root.GetComponentsInChildren<MeshRenderer>())renderer.sharedMaterials=renderer.sharedMaterials.Select(old=>{
    if(surfaceCache.TryGetValue(old,out var cached))return cached;
    var material=new Material(studyShader);Color c=Color.white;
    if(old.HasProperty("baseColorFactor"))c=old.GetColor("baseColorFactor");else if(old.HasProperty("_BaseColor"))c=old.GetColor("_BaseColor");
    material.SetColor("_BaseColor",c);
    for(int p=0;p<old.shader.GetPropertyCount();p++)if(old.shader.GetPropertyType(p)==UnityEngine.Rendering.ShaderPropertyType.Texture){string name=old.shader.GetPropertyName(p);if(name.ToLowerInvariant().Contains("basecolor"))material.SetTexture("_BaseMap",old.GetTexture(name));}
    surfaceCache.Add(old,material);return material;
   }).ToArray();
   if(root.GetComponentsInChildren<Collider>().Length!=0)throw new InvalidOperationException("Pilot must not introduce gameplay colliders");
   // Existing playable objects remain alive and retain IDs/colliders. Study hides prior renderers only, restoring in finally.
   foreach(var r in prior)r.enabled=false;
   var sun=RenderSettings.sun;
   if(!sun)sun=UnityEngine.Object.FindObjectsByType<Light>(FindObjectsSortMode.None).Where(l=>l.enabled&&l.type==LightType.Directional).OrderByDescending(l=>l.intensity).FirstOrDefault();
   var sunRotation=sun?sun.transform.rotation:Quaternion.identity;var sunShadows=sun?sun.shadows:LightShadows.None;float sunStrength=sun?sun.shadowStrength:0;
   var pipeline=UnityEngine.Rendering.GraphicsSettings.currentRenderPipeline as UnityEngine.Rendering.Universal.UniversalRenderPipelineAsset;
   float shadowDistance=pipeline?pipeline.shadowDistance:0;
   try {
    if(sun){sun.transform.rotation=Quaternion.Euler(48,-35,0);sun.shadows=LightShadows.Soft;sun.shadowStrength=.9f;}
    if(pipeline)pipeline.shadowDistance=300;
    Directory.CreateDirectory(output);
    var target=home+new Vector3(0,2,0);camera.transform.position=target+new Vector3(0,130,-145);camera.transform.LookAt(target);
    int treeCount=root.GetComponentsInChildren<Renderer>().Count(r=>r.name.StartsWith("Pilot_Pine"));
    var hybrid=BuildImpostors(root,camera,output);
    BatchSectorDecorations(root,camera,output);
    foreach(float size in new[]{34f,40f,46f,52f}) {
     float fade=Mathf.SmoothStep(0,1,Mathf.InverseLerp(36,50,size));
     foreach(var m in hybrid)m.SetFloat("_Fade",fade);
     foreach(var renderer in root.GetComponentsInChildren<MeshRenderer>())if(renderer.name=="Pilot_NearBatch"){renderer.enabled=true;renderer.shadowCastingMode=fade>=1?UnityEngine.Rendering.ShadowCastingMode.ShadowsOnly:UnityEngine.Rendering.ShadowCastingMode.On;}else if(renderer.name=="Pilot_FarBatch")renderer.enabled=fade>0;
     camera.orthographicSize=size;
     foreach(bool before in new[]{true,false}) {
     root.SetActive(!before);foreach(var r in prior)if(r)r.enabled=before;
     var rt=new RenderTexture(1280,960,24);var tex=new Texture2D(1280,960,TextureFormat.RGB24,false);
     camera.aspect=1280f/960;camera.targetTexture=rt;camera.Render();RenderTexture.active=rt;tex.ReadPixels(new Rect(0,0,1280,960),0,0);tex.Apply();
     File.WriteAllBytes(Path.Combine(output,(before?"previous-":"ribera-")+size+"-unity.png"),tex.EncodeToPNG());
     File.WriteAllText(Path.Combine(output,(before?"previous-":"ribera-")+size+"-camera.json"),JsonUtility.ToJson(new Evidence{zoom=size,position=camera.transform.position,rotation=camera.transform.rotation,unity=UnityEngine.Application.unityVersion,playMode=UnityEngine.Application.isPlaying,trees=treeCount},true));
     camera.targetTexture=oldRT;RenderTexture.active=oldActive;rt.Release();UnityEngine.Object.Destroy(rt);UnityEngine.Object.Destroy(tex);
     }
    }
    CaptureSector(camera,root,world,home,hybrid,output);
    if(!root.GetComponentsInChildren<Transform>().Any(t=>t.name.StartsWith("P ridge exposed stone"))) {
    // Sample a continuous camera trajectory, not a slideshow of the four QA stills.
    root.SetActive(true);foreach(var r in prior)if(r)r.enabled=false;
    string sequence=Path.Combine(output,"zoom-sequence");Directory.CreateDirectory(sequence);
    var seqRT=new RenderTexture(960,720,24);var seqTex=new Texture2D(960,720,TextureFormat.RGB24,false);
    try {for(int frame=0;frame<=96;frame++) {
     float t=frame<=48?frame/48f:(96-frame)/48f;float size=Mathf.Lerp(34,52,Mathf.SmoothStep(0,1,t));
     float fade=Mathf.SmoothStep(0,1,Mathf.InverseLerp(36,50,size));
     foreach(var m in hybrid)m.SetFloat("_Fade",fade);
     foreach(var renderer in root.GetComponentsInChildren<MeshRenderer>())if(renderer.name=="Pilot_NearBatch"){renderer.enabled=true;renderer.shadowCastingMode=fade>=1?UnityEngine.Rendering.ShadowCastingMode.ShadowsOnly:UnityEngine.Rendering.ShadowCastingMode.On;}else if(renderer.name=="Pilot_FarBatch")renderer.enabled=fade>0;
     camera.orthographicSize=size;camera.aspect=4f/3;camera.targetTexture=seqRT;camera.Render();RenderTexture.active=seqRT;seqTex.ReadPixels(new Rect(0,0,960,720),0,0);seqTex.Apply();
     File.WriteAllBytes(Path.Combine(sequence,"frame-"+frame.ToString("D3")+".png"),seqTex.EncodeToPNG());
     File.WriteAllText(Path.Combine(sequence,"frame-"+frame.ToString("D3")+".json"),JsonUtility.ToJson(new Evidence{zoom=size,position=camera.transform.position,rotation=camera.transform.rotation,unity=UnityEngine.Application.unityVersion,playMode=UnityEngine.Application.isPlaying,trees=treeCount},true));
    }}finally{camera.targetTexture=oldRT;RenderTexture.active=oldActive;seqRT.Release();UnityEngine.Object.DestroyImmediate(seqRT);UnityEngine.Object.DestroyImmediate(seqTex);}
    File.WriteAllText(Path.Combine(sequence,"sequence.json"),"{\"frames\":97,\"fps\":16,\"width\":960,\"height\":720,\"source\":\"actual Unity Camera.Render along continuous zoom trajectory; not runtime FPS benchmark\"}");
    }
   } finally {if(sun){sun.transform.rotation=sunRotation;sun.shadows=sunShadows;sun.shadowStrength=sunStrength;}if(pipeline)pipeline.shadowDistance=shadowDistance;foreach(var r in prior)if(r)r.enabled=true;UnityEngine.Object.DestroyImmediate(root);camera.transform.SetPositionAndRotation(oldPos,oldRot);camera.orthographicSize=oldSize;camera.aspect=oldAspect;camera.targetTexture=oldRT;RenderTexture.active=oldActive;}
  }

  static Material[] BuildImpostors(GameObject root,Camera view,string output)
  {
   var trees=root.GetComponentsInChildren<MeshRenderer>().Where(r=>r.name.StartsWith("Pilot_Pine")).ToArray();
   if(trees.Length>256)throw new InvalidOperationException("Pilot atlas capacity exceeded");
   var shader=Shader.Find("Eldoria/RiberaFade");if(!shader)throw new InvalidOperationException("Pilot fade shader missing");
   var atlas=new Texture2D(2048,2048,TextureFormat.RGBA32,false);atlas.SetPixels(new Color[2048*2048]);
   var far=new Material(shader);far.SetFloat("_Baked",1);far.SetTexture("_BaseMap",atlas);
   var mats=new System.Collections.Generic.List<Material>();mats.Add(far);
   var materialCache=new System.Collections.Generic.Dictionary<Material,Material>();
   var camObject=new GameObject("Temporary tree atlas camera");var bake=camObject.AddComponent<Camera>();bake.orthographic=true;bake.aspect=1;bake.clearFlags=CameraClearFlags.SolidColor;bake.backgroundColor=Color.clear;bake.cullingMask=1<<30;bake.nearClipPlane=.1f;bake.farClipPlane=100;
   bake.transform.rotation=view.transform.rotation;
   var previousActive=RenderTexture.active;var rt=new RenderTexture(128,128,24,RenderTextureFormat.ARGB32);bake.targetTexture=rt;
   try {
    for(int index=0;index<trees.Length;index++) {
     var r=trees[index];var converted=r.sharedMaterials.Select(old=>{
      if(materialCache.TryGetValue(old,out var existing))return existing;
      var m=new Material(shader);materialCache.Add(old,m);Color c=Color.white;
      if(old.HasProperty("baseColorFactor"))c=old.GetColor("baseColorFactor");else if(old.HasProperty("_BaseColorFactor"))c=old.GetColor("_BaseColorFactor");else if(old.HasProperty("_BaseColor"))c=old.GetColor("_BaseColor");
      m.SetColor("_BaseColor",c);mats.Add(m);return m;
     }).ToArray();r.sharedMaterials=converted;
     var copy=new GameObject("Atlas tree");copy.layer=30;copy.AddComponent<MeshFilter>().sharedMesh=r.GetComponent<MeshFilter>().sharedMesh;var cr=copy.AddComponent<MeshRenderer>();cr.sharedMaterials=converted;
     copy.transform.SetPositionAndRotation(new Vector3(0,-10000,0),r.transform.rotation);copy.transform.localScale=r.transform.lossyScale;
     var b=cr.bounds;float extent=0;
     for(int k=0;k<8;k++){var v=new Vector3((k&1)==0?-b.extents.x:b.extents.x,(k&2)==0?-b.extents.y:b.extents.y,(k&4)==0?-b.extents.z:b.extents.z);extent=Mathf.Max(extent,Mathf.Abs(Vector3.Dot(v,bake.transform.right)),Mathf.Abs(Vector3.Dot(v,bake.transform.up)));}
     extent*=1.08f;bake.orthographicSize=extent;bake.transform.position=b.center-bake.transform.forward*30;bake.Render();RenderTexture.active=rt;
     int col=index%16,row=index/16;atlas.ReadPixels(new Rect(0,0,128,128),col*128,row*128,false);
     var card=new GameObject("Pilot_Impostor_"+index);card.transform.SetParent(root.transform,true);card.layer=r.gameObject.layer;card.transform.position=r.bounds.center;card.transform.rotation=view.transform.rotation;
     var mesh=new Mesh();mesh.vertices=new[]{new Vector3(-extent,-extent,0),new Vector3(extent,-extent,0),new Vector3(extent,extent,0),new Vector3(-extent,extent,0)};mesh.triangles=new[]{0,2,1,0,3,2};
     float u=col/16f,vv=row/16f,d=1/16f;mesh.uv=new[]{new Vector2(u,vv),new Vector2(u+d,vv),new Vector2(u+d,vv+d),new Vector2(u,vv+d)};mesh.RecalculateNormals();mesh.RecalculateBounds();card.AddComponent<MeshFilter>().sharedMesh=mesh;card.AddComponent<MeshRenderer>().sharedMaterial=far;
     UnityEngine.Object.DestroyImmediate(copy);
    }
    atlas.Apply();
    // Batch the fixed study into one far draw and one near draw per shared pigment.
    foreach(var material in mats.ToArray()) {
     var combine=new System.Collections.Generic.List<CombineInstance>();var remove=new System.Collections.Generic.HashSet<MeshRenderer>();
     foreach(var renderer in root.GetComponentsInChildren<MeshRenderer>().Where(r=>r.name.StartsWith("Pilot_Pine")||r.name.StartsWith("Pilot_Impostor"))) {
      var mesh=renderer.GetComponent<MeshFilter>().sharedMesh;
      for(int sub=0;sub<renderer.sharedMaterials.Length;sub++)if(renderer.sharedMaterials[sub]==material){combine.Add(new CombineInstance{mesh=mesh,subMeshIndex=sub,transform=root.transform.worldToLocalMatrix*renderer.transform.localToWorldMatrix});remove.Add(renderer);}
     }
     if(combine.Count==0)continue;
     var merged=new Mesh();merged.indexFormat=UnityEngine.Rendering.IndexFormat.UInt32;merged.CombineMeshes(combine.ToArray(),true,true);
     var group=new GameObject(material==far?"Pilot_FarBatch":"Pilot_NearBatch");group.layer=root.layer;group.transform.SetParent(root.transform,false);group.AddComponent<MeshFilter>().sharedMesh=merged;group.AddComponent<MeshRenderer>().sharedMaterial=material;
    }
    foreach(var renderer in root.GetComponentsInChildren<MeshRenderer>().Where(r=>r.name.StartsWith("Pilot_Pine")||r.name.StartsWith("Pilot_Impostor")).ToArray())UnityEngine.Object.DestroyImmediate(renderer.gameObject);
    File.WriteAllBytes(Path.Combine(output,"trees-baked-from-actual-meshes.png"),atlas.EncodeToPNG());
   }finally{bake.targetTexture=null;RenderTexture.active=previousActive;rt.Release();UnityEngine.Object.DestroyImmediate(rt);UnityEngine.Object.DestroyImmediate(camObject);}
   return mats.ToArray();
  }
  static void BatchSectorDecorations(GameObject root,Camera camera,string output)
  {
   if(!root.GetComponentsInChildren<Transform>().Any(t=>t.name.StartsWith("P ridge exposed stone")))return;
   camera.orthographicSize=34;SaveSectorFrame(camera,Path.Combine(output,"batching-before"),1280,960);
   int before=root.GetComponentsInChildren<MeshRenderer>().Length;
   var groups=new System.Collections.Generic.Dictionary<string,System.Collections.Generic.List<CombineInstance>>();
   var materials=new System.Collections.Generic.Dictionary<string,Material>();var sources=new System.Collections.Generic.List<MeshRenderer>();
   foreach(var r in root.GetComponentsInChildren<MeshRenderer>()) {
    if(!(r.name.StartsWith("Bank forest stone")||r.name.StartsWith("Pilot_Rock")||r.name.StartsWith("P ridge exposed stone")))continue;
    var filter=r.GetComponent<MeshFilter>();if(!filter)continue;var centre=root.transform.InverseTransformPoint(r.bounds.center);
    for(int sub=0;sub<r.sharedMaterials.Length;sub++) {
     var mat=r.sharedMaterials[sub];string key=mat.GetInstanceID()+":"+Mathf.FloorToInt(centre.x/32)+":"+Mathf.FloorToInt(centre.z/32);
     if(!groups.ContainsKey(key)){groups.Add(key,new System.Collections.Generic.List<CombineInstance>());materials.Add(key,mat);}
     groups[key].Add(new CombineInstance{mesh=filter.sharedMesh,subMeshIndex=sub,transform=root.transform.worldToLocalMatrix*r.transform.localToWorldMatrix});
    }
    sources.Add(r);
   }
   foreach(var pair in groups) {
    var mesh=new Mesh();mesh.indexFormat=UnityEngine.Rendering.IndexFormat.UInt32;mesh.CombineMeshes(pair.Value.ToArray(),true,true);
    var go=new GameObject("Sector stone batch "+pair.Key);go.layer=root.layer;go.transform.SetParent(root.transform,false);go.AddComponent<MeshFilter>().sharedMesh=mesh;go.AddComponent<MeshRenderer>().sharedMaterial=materials[pair.Key];
   }
   foreach(var r in sources){UnityEngine.Object.DestroyImmediate(r.GetComponent<MeshFilter>());UnityEngine.Object.DestroyImmediate(r);}
   int after=root.GetComponentsInChildren<MeshRenderer>().Length;SaveSectorFrame(camera,Path.Combine(output,"batching-after"),1280,960);
   File.WriteAllText(Path.Combine(output,"batching.json"),"{\"renderers_before\":"+before+",\"renderers_after\":"+after+",\"source_stone_renderers\":"+sources.Count+",\"spatial_batches\":"+groups.Count+",\"cell_metres\":32,\"physical_mobile_tested\":false}");
  }

  static void CaptureSector(Camera camera,GameObject root,Transform world,Vector3 home,Material[] hybrid,string output)
  {
   if(!root.GetComponentsInChildren<Transform>().Any(t=>t.name.StartsWith("P ridge exposed stone")))return;
   root.SetActive(true);string dir=Path.Combine(output,"sector");Directory.CreateDirectory(dir);
   var originalColliders=world.GetComponentsInChildren<Collider>(true).Where(c=>!c.transform.IsChildOf(root.transform)).ToArray();
   var positions=originalColliders.Select(c=>c.transform.position).ToArray();
   var probes=new System.Collections.Generic.List<HeightProbe>();
   var temporary=new System.Collections.Generic.List<GameObject>();
   try {
    // Disposable surface queries use the imported authored meshes; no gameplay collider or navigation mutation.
    foreach(var mf in root.GetComponentsInChildren<MeshFilter>().Where(m=>m.name.StartsWith("Pilot_Terrain"))) {
     var go=new GameObject("Temporary authored-height query");temporary.Add(go);go.layer=31;go.transform.SetPositionAndRotation(mf.transform.position,mf.transform.rotation);go.transform.localScale=mf.transform.lossyScale;go.AddComponent<MeshCollider>().sharedMesh=mf.sharedMesh;
    }
    Physics.SyncTransforms();
    foreach(var c in originalColliders){var pos=c.transform.position;float dx=pos.x-home.x,dz=pos.z-home.z;if(Mathf.Abs(dx)>95||dz< -77||dz>169)continue;
     foreach(var go in temporary)if(go.GetComponent<MeshCollider>().Raycast(new Ray(new Vector3(pos.x,home.y+250,pos.z),Vector3.down),out var hit,500)){probes.Add(new HeightProbe{name=c.name,logicalPosition=pos,authoredSurface=hit.point,heightDifference=hit.point.y-c.bounds.min.y,hotspotId=HotspotId(c),boundsMin=c.bounds.min,boundsMax=c.bounds.max});break;}
    }
   } finally {foreach(var go in temporary)UnityEngine.Object.DestroyImmediate(go);}
   var stats=new SectorStats{heightProbes=probes.ToArray(),logicalColliderCount=originalColliders.Length,physicalMobileTested=false};
   stats.maxHeightDifference=probes.Count>0?probes.Max(p=>Mathf.Abs(p.heightDifference)):0;
   // Collider transform origins are not ground anchors: use bounds.min.y as a provisional foot-contact proxy.\n   // This diagnostic does not alter transforms, gameplay colliders, saves or navigation.\n   // The proxy still needs per-asset mesh-contact validation before certifying grounding.
   stats.heightDeltaIsGroundingVerdict=false;
   // Explicitly flag discrepancies; collider bounds are not authoritative feet.
   // Preserve every logical object and report the full raw evidence for mesh-contact review.
   if(stats.maxHeightDifference>1f)
    Debug.LogWarning("RIBERA_GROUNDING_REVIEW_REQUIRED max collider-foot delta="+stats.maxHeightDifference.ToString("F3")+"m; not a corrected grounding PASS");
   var pickResults=new System.Collections.Generic.List<PickProbe>();
   foreach(bool portrait in new[]{false,true}) {
    SetSectorCamera(camera,root,hybrid,home+new Vector3(-15,2,0),portrait?160:70,portrait?9f/16:16f/9);
    foreach(var p in probes.Where(p=>!string.IsNullOrEmpty(p.hotspotId))) {
     foreach(bool groundAim in new[]{false,true}) {
      var aim=groundAim?p.authoredSurface:p.logicalPosition;
      var viewport=camera.WorldToViewportPoint(aim);
      var hits=Physics.RaycastAll(camera.ViewportPointToRay(viewport),1000,1<<8,QueryTriggerInteraction.Collide).OrderBy(h=>h.distance).ToArray();
      string selected=hits.Length>0?HotspotId(hits[0].collider):"";
      pickResults.Add(new PickProbe{expectedId=p.hotspotId,selectedId=selected,portrait=portrait,aimAtAuthoredGround=groundAim,inViewport=viewport.x>=0&&viewport.x<=1&&viewport.y>=0&&viewport.y<=1&&viewport.z>0,matched=selected==p.hotspotId});
     }
    }
   }
   stats.selectionProbes=pickResults.ToArray();
   var meshes=root.GetComponentsInChildren<MeshFilter>();stats.meshInstances=meshes.Length;stats.rendererCount=root.GetComponentsInChildren<Renderer>().Length;
   stats.submittedMeshTriangles=meshes.Sum(m=>m.sharedMesh.triangles.Length/3);stats.editorAllocatedBytes=UnityEngine.Profiling.Profiler.GetTotalAllocatedMemoryLong();
   var oldRT=camera.targetTexture;var oldActive=RenderTexture.active;
   try {
    foreach(bool portrait in new[]{false,true}) {
     string orientation=portrait?"portrait":"landscape";int width=portrait?720:1280,height=portrait?1280:720;
     for(int level=0;level<3;level++) {
      float size=portrait?new[]{32f,52f,74f}[level]:new[]{24f,38f,52f}[level];
      Vector3 target=home+new Vector3(Mathf.Lerp(20,0,level/2f),2,Mathf.Lerp(95,50,level/2f));SetSectorCamera(camera,root,hybrid,target,size,width/(float)height);
      SaveSectorFrame(camera,Path.Combine(dir,orientation+"-"+new[]{"close","regional","panorama"}[level]),width,height);
     }
     // Continuous zoom and pan from the new forest edge towards the connected sector, same authored geography.
     string seq=Path.Combine(dir,orientation+"-zoom");Directory.CreateDirectory(seq);
     for(int frame=0;frame<=48;frame++) {
      float t=Mathf.SmoothStep(0,1,frame/48f);float size=portrait?Mathf.Lerp(32,74,t):Mathf.Lerp(24,52,t);
      SetSectorCamera(camera,root,hybrid,home+new Vector3(Mathf.Lerp(20,0,t),2,Mathf.Lerp(95,50,t)),size,portrait?9f/16:16f/9);
      SaveSectorFrame(camera,Path.Combine(seq,"frame-"+frame.ToString("D3")),portrait?540:960,portrait?960:540);
     }
    }
   } finally {camera.targetTexture=oldRT;RenderTexture.active=oldActive;}
   for(int i=0;i<originalColliders.Length;i++)if(!originalColliders[i]||originalColliders[i].transform.position!=positions[i])throw new InvalidOperationException("Sector capture moved or removed a logical collider");
   stats.logicalPositionsUnchanged=true;
   File.WriteAllText(Path.Combine(dir,"integration-and-cost.json"),JsonUtility.ToJson(stats,true));
  }
  static void SetSectorCamera(Camera camera,GameObject root,Material[] hybrid,Vector3 target,float size,float aspect)
  {
   camera.transform.position=target+new Vector3(0,130,-145);camera.transform.LookAt(target);camera.orthographicSize=size;camera.aspect=aspect;
   float fade=Mathf.SmoothStep(0,1,Mathf.InverseLerp(36,50,size));foreach(var m in hybrid)m.SetFloat("_Fade",fade);
   foreach(var r in root.GetComponentsInChildren<MeshRenderer>())if(r.name=="Pilot_NearBatch"){r.enabled=true;r.shadowCastingMode=fade>=1?UnityEngine.Rendering.ShadowCastingMode.ShadowsOnly:UnityEngine.Rendering.ShadowCastingMode.On;}else if(r.name=="Pilot_FarBatch")r.enabled=fade>0;
  }
  static void SaveSectorFrame(Camera camera,string path,int width,int height)
  {
   var rt=new RenderTexture(width,height,24);var tex=new Texture2D(width,height,TextureFormat.RGB24,false);
   try {camera.aspect=width/(float)height;camera.targetTexture=rt;camera.Render();RenderTexture.active=rt;tex.ReadPixels(new Rect(0,0,width,height),0,0);tex.Apply();File.WriteAllBytes(path+".png",tex.EncodeToPNG());File.WriteAllText(path+".json",JsonUtility.ToJson(new SectorCamera{position=camera.transform.position,rotation=camera.transform.rotation,orthoSize=camera.orthographicSize,width=width,height=height,playMode=UnityEngine.Application.isPlaying},true));}
   finally {camera.targetTexture=null;RenderTexture.active=null;rt.Release();UnityEngine.Object.DestroyImmediate(rt);UnityEngine.Object.DestroyImmediate(tex);}
  }
  [Serializable] class SectorCamera {public Vector3 position;public Quaternion rotation;public float orthoSize;public int width,height;public bool playMode;}
  static string HotspotId(Collider c) {if(!c)return "";foreach(var b in c.GetComponents<MonoBehaviour>()){if(b&&b.GetType().Name=="WorldHotspot")return b.GetType().GetField("Id")?.GetValue(b) as string??"";}return "";}
  [Serializable] class PickProbe {public string expectedId,selectedId;public bool portrait,aimAtAuthoredGround,inViewport,matched;}
  [Serializable] class HeightProbe {public string name,hotspotId;public Vector3 logicalPosition,authoredSurface,boundsMin,boundsMax;public float heightDifference;}
  [Serializable] class SectorStats {public HeightProbe[] heightProbes;public int logicalColliderCount,meshInstances,rendererCount,submittedMeshTriangles;public long editorAllocatedBytes;public float maxHeightDifference;public PickProbe[] selectionProbes;public bool heightDeltaIsGroundingVerdict,logicalPositionsUnchanged,physicalMobileTested;}

  [Serializable] class Evidence {public float zoom;public Vector3 position;public Quaternion rotation;public string unity;public bool playMode;public int trees;}
 }
}

