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
     float fade=Mathf.SmoothStep(0,1,Mathf.InverseLerp(27,38,size));
     foreach(var m in hybrid)m.SetFloat("_Fade",fade);
     foreach(var renderer in root.GetComponentsInChildren<MeshRenderer>())if(renderer.name=="Pilot_NearBatch"){renderer.enabled=fade<.999f;renderer.shadowCastingMode=UnityEngine.Rendering.ShadowCastingMode.On;}else if(renderer.name=="Pilot_FarBatch")renderer.enabled=fade>0;
     camera.orthographicSize=size;
     FadeEcologicalUnderstory(root,size);
     foreach(bool before in new[]{true,false}) {
     root.SetActive(!before);foreach(var r in prior)if(r)r.enabled=before;
     var rt=new RenderTexture(1280,960,24);var tex=new Texture2D(1280,960,TextureFormat.RGB24,false);
     camera.aspect=1280f/960;camera.targetTexture=rt;camera.Render();RenderTexture.active=rt;tex.ReadPixels(new Rect(0,0,1280,960),0,0);tex.Apply();
     File.WriteAllBytes(Path.Combine(output,(before?"previous-":"ribera-")+size+"-unity.png"),tex.EncodeToPNG());
     File.WriteAllText(Path.Combine(output,(before?"previous-":"ribera-")+size+"-camera.json"),JsonUtility.ToJson(new Evidence{zoom=size,position=camera.transform.position,rotation=camera.transform.rotation,unity=UnityEngine.Application.unityVersion,playMode=UnityEngine.Application.isPlaying,trees=treeCount},true));
     camera.targetTexture=oldRT;RenderTexture.active=oldActive;rt.Release();UnityEngine.Object.Destroy(rt);UnityEngine.Object.Destroy(tex);
     }
    }
    CaptureSector(camera,root,world,home,hybrid,output,prior);
    if(!root.GetComponentsInChildren<Transform>().Any(t=>t.name.StartsWith("P ridge exposed stone"))) {
    // Sample a continuous camera trajectory, not a slideshow of the four QA stills.
    root.SetActive(true);foreach(var r in prior)if(r)r.enabled=false;
    string sequence=Path.Combine(output,"zoom-sequence");Directory.CreateDirectory(sequence);
    var seqRT=new RenderTexture(960,720,24);var seqTex=new Texture2D(960,720,TextureFormat.RGB24,false);
    try {for(int frame=0;frame<=96;frame++) {
     float t=frame<=48?frame/48f:(96-frame)/48f;float size=Mathf.Lerp(34,52,Mathf.SmoothStep(0,1,t));
     float fade=Mathf.SmoothStep(0,1,Mathf.InverseLerp(27,38,size));
     foreach(var m in hybrid)m.SetFloat("_Fade",fade);
     foreach(var renderer in root.GetComponentsInChildren<MeshRenderer>())if(renderer.name=="Pilot_NearBatch"){renderer.enabled=fade<.999f;renderer.shadowCastingMode=UnityEngine.Rendering.ShadowCastingMode.On;}else if(renderer.name=="Pilot_FarBatch")renderer.enabled=fade>0;
     camera.orthographicSize=size;camera.aspect=4f/3;FadeEcologicalUnderstory(root,size);camera.targetTexture=seqRT;camera.Render();RenderTexture.active=seqRT;seqTex.ReadPixels(new Rect(0,0,960,720),0,0);seqTex.Apply();
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
   // Y retains the same atlas memory footprint, using 64px cells only if >256
   // source pines are authored. Preserve original 128px near-study baseline otherwise.
   int atlasColumns=trees.Length>256?32:16;
   int tilePixels=2048/atlasColumns;
   if(trees.Length>atlasColumns*atlasColumns)throw new InvalidOperationException("Pilot atlas capacity exceeded");
   var shader=Shader.Find("Eldoria/RiberaFade");if(!shader)throw new InvalidOperationException("Pilot fade shader missing");
   var atlas=new Texture2D(2048,2048,TextureFormat.RGBA32,false);atlas.SetPixels(new Color[2048*2048]);
   var far=new Material(shader);far.SetFloat("_Baked",1);far.SetTexture("_BaseMap",atlas);
   var mats=new System.Collections.Generic.List<Material>();mats.Add(far);
   var materialCache=new System.Collections.Generic.Dictionary<Material,Material>();
   var camObject=new GameObject("Temporary tree atlas camera");var bake=camObject.AddComponent<Camera>();bake.orthographic=true;bake.aspect=1;bake.clearFlags=CameraClearFlags.SolidColor;bake.backgroundColor=Color.clear;bake.cullingMask=1<<30;bake.nearClipPlane=.1f;bake.farClipPlane=100;
   bake.transform.rotation=view.transform.rotation;
   var previousActive=RenderTexture.active;var rt=new RenderTexture(tilePixels,tilePixels,24,RenderTextureFormat.ARGB32);bake.targetTexture=rt;
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
     int col=index%atlasColumns,row=index/atlasColumns;
     atlas.ReadPixels(new Rect(0,0,tilePixels,tilePixels),col*tilePixels,row*tilePixels,false);
     var card=new GameObject("Pilot_Impostor_"+index);card.transform.SetParent(root.transform,true);card.layer=r.gameObject.layer;card.transform.position=r.bounds.center;card.transform.rotation=view.transform.rotation;
     var mesh=new Mesh();mesh.vertices=new[]{new Vector3(-extent,-extent,0),new Vector3(extent,-extent,0),new Vector3(extent,extent,0),new Vector3(-extent,extent,0)};mesh.triangles=new[]{0,2,1,0,3,2};
     float u=col/(float)atlasColumns,vv=row/(float)atlasColumns,d=1f/atlasColumns;mesh.uv=new[]{new Vector2(u,vv),new Vector2(u+d,vv),new Vector2(u+d,vv+d),new Vector2(u,vv+d)};mesh.RecalculateNormals();mesh.RecalculateBounds();card.AddComponent<MeshFilter>().sharedMesh=mesh;card.AddComponent<MeshRenderer>().sharedMaterial=far;
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

  static void CaptureSector(Camera camera,GameObject root,Transform world,Vector3 home,Material[] hybrid,string output,Renderer[] originalEnabledRenderers)
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
   // The old 2.5D node markers used the procedural world-height sampler rather than this
   // imported Blender surface. Clone their RENDER MESHES ONLY, not their logical targets.
   // Keep the collider-based discrepancy in heightProbes as an independent unresolved measurement.
   var groundedVisualMarkers=new System.Collections.Generic.List<GroundedMarkerProbe>();
   var sourceMarkers=world.GetComponentsInChildren<MeshRenderer>(true);
   var markers=new[]{
    new {id="valoria-map-city",prefix="Reference city Bastion"},
    new {id="forest-valoria",prefix="Reference existing wood grove"},
    new {id="quarry-valoria",prefix="Reference existing quarry"},
    new {id="old-watch-ruin",prefix="Reference existing neutral watch ruin"}
   };
   foreach(var spec in markers) {
    var probe=probes.FirstOrDefault(p=>p.hotspotId==spec.id);
    if(probe==null)continue;
    foreach(var donor in sourceMarkers.Where(r=>r&&r.name.StartsWith(spec.prefix,StringComparison.Ordinal))) {
     var mf=donor.GetComponent<MeshFilter>();
     if(!mf||!mf.sharedMesh||mf.sharedMesh.vertexCount==0)continue;
     var sourceFoot=donor.transform.TransformPoint(mf.sharedMesh.vertices[0]);
     float delta=probe.authoredSurface.y+.08f-sourceFoot.y;
     var proxy=new GameObject("Pilot grounded visual "+donor.name);
     proxy.layer=root.layer;
     proxy.transform.SetParent(root.transform,true);
     proxy.transform.SetPositionAndRotation(donor.transform.position+Vector3.up*delta,donor.transform.rotation);
     proxy.transform.localScale=donor.transform.lossyScale;
     proxy.AddComponent<MeshFilter>().sharedMesh=mf.sharedMesh;
     proxy.AddComponent<MeshRenderer>().sharedMaterials=donor.sharedMaterials;
     float measuredFoot=proxy.transform.TransformPoint(mf.sharedMesh.vertices[0]).y;
     float error=measuredFoot-(probe.authoredSurface.y+.08f);
     if(Mathf.Abs(error)>.03f)throw new InvalidOperationException("Ribera node visual contact error: "+spec.id+" "+error);
     groundedVisualMarkers.Add(new GroundedMarkerProbe{id=spec.id,sourceRenderer=donor.name,originalVisualFootY=sourceFoot.y,authoredGroundY=probe.authoredSurface.y,groundedVisualFootY=measuredFoot,visualCorrectionMeters=delta,contactErrorMeters=error});
    }
   }
   if(groundedVisualMarkers.Count!=5)throw new InvalidOperationException("Expected five contact-verified visual node instances; found "+groundedVisualMarkers.Count);
   // Preserve the original animated Spawnling when the connected sector is captured.
   // Its source visual is independently grounded to the imported mesh for this
   // disposable capture only. The encounter, animator, collider and persistence stay untouched.
   var actor=world.GetComponentsInChildren<MonoBehaviour>(true).FirstOrDefault(c=>c&&c.GetType().Name=="SpawnlingEncounterVisual");
   var actorType=actor?actor.GetType():null;
   var actorModel=actorType?.GetField("ModelRoot")?.GetValue(actor) as GameObject;
   var actorPivot=actorType?.GetField("MotionPivot")?.GetValue(actor) as Transform;
   var actorOriginalPivot=actorPivot?actorPivot.localPosition:Vector3.zero;
   var actorRenderers=actorModel&&actorModel.activeInHierarchy?
    originalEnabledRenderers.Where(r=>r&&r.transform.IsChildOf(actorModel.transform)).ToArray():Array.Empty<Renderer>();
   var actorGroundingError=0f;bool actorContactMeasured=false;
   foreach(var r in actorRenderers)r.enabled=true;
   var actorProbe=probes.FirstOrDefault(p=>p.hotspotId=="engendro-valoria");
   if(actorPivot&&actorProbe!=null&&actorRenderers.Length>0&&actorPivot.GetComponentsInChildren<Collider>(true).Length==0) {
    var method=actorType.GetMethod("TryGeometryBounds");
    if(method!=null) {
     var args=new object[]{new Bounds()};
     if(method.Invoke(actor,args) is bool measured&&measured) {
      var bounds=(Bounds)args[0];
      actorPivot.position+=Vector3.up*(actorProbe.authoredSurface.y+.08f-bounds.min.y);
      args[0]=new Bounds();
      if(method.Invoke(actor,args) is bool checkedBounds&&checkedBounds) {
       actorGroundingError=((Bounds)args[0]).min.y-actorProbe.authoredSurface.y-.08f;
       actorContactMeasured=true;
       if(Mathf.Abs(actorGroundingError)>.15f)throw new InvalidOperationException("Animated visual contact not grounded on actual Blender terrain");
      }
     }
    }
   }
   var stats=new SectorStats{heightProbes=probes.ToArray(),logicalColliderCount=originalColliders.Length,physicalMobileTested=false,groundedVisualMarkers=groundedVisualMarkers.ToArray(),groundedVisualMarkerCount=groundedVisualMarkers.Count,actorVisualContactMeasured=actorContactMeasured,actorVisualContactError=actorGroundingError,animatedOriginalRenderersRetained=actorRenderers.Length};
   stats.maxHeightDifference=probes.Count>0?probes.Max(p=>Mathf.Abs(p.heightDifference)):0;
   // Collider bounds.min.y is only a provisional contact proxy. Mesh-contact certification remains pending.
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
   if(pickResults.Count<20||pickResults.Any(p=>p.inViewport&&!p.matched))throw new InvalidOperationException("Ground-aware gameplay target picking regressed");
   var meshes=root.GetComponentsInChildren<MeshFilter>();stats.meshInstances=meshes.Length;stats.rendererCount=root.GetComponentsInChildren<Renderer>().Length;
   stats.submittedMeshTriangles=meshes.Sum(m=>m.sharedMesh.triangles.Length/3);stats.editorAllocatedBytes=UnityEngine.Profiling.Profiler.GetTotalAllocatedMemoryLong();
   var oldRT=camera.targetTexture;var oldActive=RenderTexture.active;
   var lodReadings=new System.Collections.Generic.List<SectorLodSample>();
   try {
    foreach(bool portrait in new[]{false,true}) {
     string orientation=portrait?"portrait":"landscape";int width=portrait?720:1280,height=portrait?1280:720;
     for(int level=0;level<3;level++) {
      float size=portrait?new[]{32f,52f,74f}[level]:new[]{24f,38f,52f}[level];
      Vector3 target=home+new Vector3(Mathf.Lerp(20,0,level/2f),2,Mathf.Lerp(95,50,level/2f));SetSectorCamera(camera,root,hybrid,target,size,width/(float)height);
      var rendered=root.GetComponentsInChildren<MeshRenderer>().Where(r=>r.enabled).ToArray();
      lodReadings.Add(new SectorLodSample{orientation=orientation,scale=new[]{"close","regional","panorama"}[level],zoom=size,
        visibleRenderers=rendered.Length,visibleTriangles=rendered.Sum(r=>{var f=r.GetComponent<MeshFilter>();return f&&f.sharedMesh?f.sharedMesh.triangles.Length/3:0;})});
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
   } finally {
    camera.targetTexture=oldRT;RenderTexture.active=oldActive;
    if(actorPivot)actorPivot.localPosition=actorOriginalPivot;
    foreach(var r in actorRenderers)if(r)r.enabled=false;
   }
   for(int i=0;i<originalColliders.Length;i++)if(!originalColliders[i]||originalColliders[i].transform.position!=positions[i])throw new InvalidOperationException("Sector capture moved or removed a logical collider");
   stats.logicalPositionsUnchanged=true;
   stats.lodSamples=lodReadings.ToArray();
   var close=lodReadings.First(p=>p.orientation=="landscape"&&p.scale=="close").visibleTriangles;
   var far=lodReadings.First(p=>p.orientation=="portrait"&&p.scale=="panorama").visibleTriangles;
   if(far>=close*.80f)throw new InvalidOperationException("Mobile far LOD failed to reduce authored triangle submission: "+far+" versus "+close);
   File.WriteAllText(Path.Combine(dir,"integration-and-cost.json"),JsonUtility.ToJson(stats,true));
  }
  static void FadeEcologicalUnderstory(GameObject root,float size)
  {
   float fade=Mathf.SmoothStep(0,1,Mathf.InverseLerp(42,57,size));
   foreach(var r in root.GetComponentsInChildren<MeshRenderer>(true).Where(r=>(r.name.StartsWith("Z retained-sector woodland understory",StringComparison.Ordinal)||r.name.StartsWith("AE wild meadow ecology",StringComparison.Ordinal)))) {
    r.enabled=fade<.999f;
    r.shadowCastingMode=UnityEngine.Rendering.ShadowCastingMode.Off;
    var properties=new MaterialPropertyBlock();r.GetPropertyBlock(properties);properties.SetFloat("_Fade",fade);r.SetPropertyBlock(properties);
   }
  }
  static void SetSectorCamera(Camera camera,GameObject root,Material[] hybrid,Vector3 target,float size,float aspect)
  {
   camera.transform.position=target+new Vector3(0,130,-145);camera.transform.LookAt(target);camera.orthographicSize=size;camera.aspect=aspect;
   FadeEcologicalUnderstory(root,size);
   float fade=Mathf.SmoothStep(0,1,Mathf.InverseLerp(27,38,size));foreach(var m in hybrid)m.SetFloat("_Fade",fade);
   foreach(var r in root.GetComponentsInChildren<MeshRenderer>())if(r.name=="Pilot_NearBatch"){r.enabled=fade<.999f;r.shadowCastingMode=UnityEngine.Rendering.ShadowCastingMode.On;}else if(r.name=="Pilot_FarBatch")r.enabled=fade>0;
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
  [Serializable] class GroundedMarkerProbe {public string id,sourceRenderer;public float originalVisualFootY,authoredGroundY,groundedVisualFootY,visualCorrectionMeters,contactErrorMeters;}
  [Serializable] class HeightProbe {public string name,hotspotId;public Vector3 logicalPosition,authoredSurface,boundsMin,boundsMax;public float heightDifference;}
  [Serializable] class SectorLodSample {public string orientation,scale;public float zoom;public int visibleRenderers,visibleTriangles;}
  [Serializable] class SectorStats {public bool actorVisualContactMeasured;public float actorVisualContactError;public int animatedOriginalRenderersRetained;public SectorLodSample[] lodSamples;public HeightProbe[] heightProbes;public GroundedMarkerProbe[] groundedVisualMarkers;public int groundedVisualMarkerCount;public int logicalColliderCount,meshInstances,rendererCount,submittedMeshTriangles;public long editorAllocatedBytes;public float maxHeightDifference;public PickProbe[] selectionProbes;public bool heightDeltaIsGroundingVerdict,logicalPositionsUnchanged,physicalMobileTested;}

  [Serializable] class Evidence {public float zoom;public Vector3 position;public Quaternion rotation;public string unity;public bool playMode;public int trees;}
 }
}

