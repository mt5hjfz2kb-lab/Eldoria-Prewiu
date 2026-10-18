using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using Object=UnityEngine.Object;
namespace Eldoria.ReferenceClean.Editor {
public static class ValoriaReferenceCleanBastionIntegrate {
 const string Root="Assets/Eldoria/ArtTests/ValoriaReferenceClean";
 const string Dir=Root+"/GeneratedArt/BastionReviewed";
 static readonly string Ev=Path.GetFullPath("../docs/evidence/valoria-reference-clean");
 [Serializable]class Report{public string source_sha,reviewed_variant;public int previous_triangles,replacement_triangles,environment_triangles,lightmaps,lightprobes;public bool original_selection_object_preserved,collider_matches_new_mesh,static_batching_disabled,art_target_8_of_10_certified=false;}
 static void Put(Object o,string path){if(AssetDatabase.LoadMainAssetAtPath(path))throw new Exception("Reviewed asset already exists: "+path);AssetDatabase.CreateAsset(o,path);}
 public static void Build(){ValoriaReferenceCleanRenderSettings guard=null;try{
  const string candidate=Root+"/ArtVariants/Bastion73/Valoria_Bastion73.unity";
  var scene=EditorSceneManager.OpenScene(candidate,OpenSceneMode.Single);guard=Object.FindFirstObjectByType<ValoriaReferenceCleanRenderSettings>();guard.Apply();Directory.CreateDirectory(Dir);AssetDatabase.Refresh();
  var old=Object.FindObjectsByType<MeshFilter>(FindObjectsSortMode.None).Single(f=>f.name=="Bastion_I");
  var replacement=Object.FindObjectsByType<MeshFilter>(FindObjectsSortMode.None).Single(f=>f.name=="Bastion_Ensemble_Candidate");
  var environment=Object.FindObjectsByType<MeshFilter>(FindObjectsSortMode.None).Single(f=>f.name=="Bastion_Environment72");
  var parent=GameObject.Find("Valoria authored architecture and environment");if(!parent)throw new Exception("Original architecture root missing");
  var identity=old.gameObject.GetInstanceID();int oldTriangles=old.sharedMesh.triangles.Length/3;
  var mesh=Object.Instantiate(replacement.sharedMesh);mesh.name="Bastion I reviewed native70 architecture";
  Matrix4x4 m=old.transform.worldToLocalMatrix*replacement.transform.localToWorldMatrix,norm=m.inverse.transpose;
  var vertices=mesh.vertices;for(int i=0;i<vertices.Length;i++)vertices[i]=m.MultiplyPoint3x4(vertices[i]);mesh.vertices=vertices;
  var normals=mesh.normals;for(int i=0;i<normals.Length;i++)normals[i]=norm.MultiplyVector(normals[i]).normalized;mesh.normals=normals;
  var tangents=mesh.tangents;for(int i=0;i<tangents.Length;i++){var t=m.MultiplyVector(new Vector3(tangents[i].x,tangents[i].y,tangents[i].z)).normalized;tangents[i]=new Vector4(t.x,t.y,t.z,tangents[i].w*Mathf.Sign(m.determinant));}mesh.tangents=tangents;
  if(m.determinant<0)for(int s=0;s<mesh.subMeshCount;s++){var t=mesh.GetTriangles(s);for(int i=0;i<t.Length;i+=3){int swap=t[i+1];t[i+1]=t[i+2];t[i+2]=swap;}mesh.SetTriangles(t,s);}
  mesh.RecalculateBounds();Put(mesh,Dir+"/Bastion_I.asset");old.sharedMesh=mesh;
  var renderer=old.GetComponent<MeshRenderer>();renderer.sharedMaterials=replacement.GetComponent<MeshRenderer>().sharedMaterials;renderer.enabled=true;renderer.receiveGI=ReceiveGI.Lightmaps;renderer.lightmapIndex=-1;
  GameObjectUtility.SetStaticEditorFlags(old.gameObject,StaticEditorFlags.ContributeGI);
  var settingsRenderer=new SerializedObject(renderer);settingsRenderer.FindProperty("m_ScaleInLightmap").floatValue=12;settingsRenderer.ApplyModifiedPropertiesWithoutUndo();
  var collider=old.GetComponent<MeshCollider>();if(!collider)throw new Exception("Original Bastion selection/collision component missing");collider.sharedMesh=null;collider.sharedMesh=mesh;
  environment.transform.SetParent(parent.transform,true);environment.name="Bastion_I_Grounding";GameObjectUtility.SetStaticEditorFlags(environment.gameObject,StaticEditorFlags.ContributeGI);
  // Delete the isolated duplicate architecture/old planting after transplanting the mesh into the existing gameplay object.
  var artCopy=GameObject.Find("Bastion ensemble art candidate 70");if(!artCopy)throw new Exception("Reviewed candidate hierarchy changed");Object.DestroyImmediate(artCopy);
  var envRoot=GameObject.Find("Bastion immediate environment72");if(envRoot&&envRoot.transform.childCount==0)Object.DestroyImmediate(envRoot);
  string output=Root+"/Valoria_Reference_Clean.unity";EditorSceneManager.SaveScene(scene,output);
  var lighting=Object.Instantiate(Lightmapping.lightingSettings);lighting.name="Valoria reviewed Bastion Progressive CPU";lighting.realtimeGI=false;lighting.lightmapper=LightingSettings.Lightmapper.ProgressiveCPU;Put(lighting,Dir+"/Lighting.lighting");Lightmapping.lightingSettings=lighting;Lightmapping.lightingDataAsset=null;Lightmapping.giWorkflowMode=Lightmapping.GIWorkflowMode.OnDemand;
  EditorSceneManager.SaveScene(scene,output);AssetDatabase.SaveAssets();if(!Lightmapping.Bake()||LightmapSettings.lightmaps.Length==0)throw new Exception("Integrated native bake failed");
  foreach(var probe in Object.FindObjectsByType<ReflectionProbe>(FindObjectsSortMode.None)){if(probe.mode!=ReflectionProbeMode.Baked)continue;string path=Dir+"/"+probe.name.Replace(" ","_")+".exr";if(!Lightmapping.BakeReflectionProbe(probe,path))throw new Exception("Integrated reflection bake failed");AssetDatabase.ImportAsset(path);probe.bakedTexture=AssetDatabase.LoadAssetAtPath<Texture>(path);}
  LightProbes.Tetrahedralize();
  var report=new Report{source_sha=Environment.GetEnvironmentVariable("GITHUB_SHA"),reviewed_variant=candidate,previous_triangles=oldTriangles,replacement_triangles=mesh.triangles.Length/3,environment_triangles=environment.sharedMesh.triangles.Length/3,lightmaps=LightmapSettings.lightmaps.Length,lightprobes=LightmapSettings.lightProbes.count,original_selection_object_preserved=old.gameObject.GetInstanceID()==identity,collider_matches_new_mesh=collider.sharedMesh==old.sharedMesh,static_batching_disabled=Object.FindObjectsByType<MeshRenderer>(FindObjectsSortMode.None).All(r=>(GameObjectUtility.GetStaticEditorFlags(r.gameObject)&StaticEditorFlags.BatchingStatic)==0)};
  if(!report.original_selection_object_preserved||!report.collider_matches_new_mesh||!report.static_batching_disabled)throw new Exception("Integration identity failed");
  File.WriteAllText(Ev+"/bastion-integrated.json",JsonUtility.ToJson(report,true));EditorSceneManager.SaveScene(scene,output);AssetDatabase.SaveAssets();guard.Restore();
  // Existing finalizer refreshes all eight portable prefabs and waits for a real closed package.
  ValoriaReferenceCleanFinalize.Build();
 }catch(Exception e){if(guard)guard.Restore();Debug.LogException(e);EditorApplication.Exit(1);}}
}
}
