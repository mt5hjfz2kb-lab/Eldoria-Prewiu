// Isolated Valoria E: genuine historic library bridge modules over ABCD, without changing the collision route.
using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Security.Cryptography;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using GLTFast;

namespace Eldoria.ValoriaLayered.Editor
{
  public static class ValoriaAuthenticBridgeLayerE
  {
    const string Root="Assets/Eldoria/ArtTests/ValoriaLayeredConstruction";
    const string Input=Root+"/Sources",Generated=Root+"/GeneratedE";
    const string InScene=Root+"/Valoria_MasterPlan_Layers_ABCD.unity";
    const string OutScene=Root+"/Valoria_MasterPlan_Layers_ABCDE.unity";
    static readonly string Evidence=Path.GetFullPath(Path.Combine(UnityEngine.Application.dataPath,
      "..","..","docs","evidence","valoria-layered-construction-20261016"));
    static readonly string[] Ids={"bridge_span_4m","arch_bridge_support","stone_slab_cap"};
    static readonly string[] Hashes={
      "d84bfc9a245520399b8c2298e86fcc48afbbb6fbec8de32e4e639281351b6b49",
      "440fdec7acfa0942a2d50fe954c232c808edc88887a8391cfbd193682dfcd396",
      "0c632d3656e4db63ded29ba76265536c0b4cb64f5b96dfe265656985390a3383"
    };
    static readonly Dictionary<string,GameObject> imported=new Dictionary<string,GameObject>();
    static readonly HashSet<int> persisted=new HashSet<int>();
    static int index;
    static Camera view;
    [Serializable] sealed class Manifest {
      public string unity,scene,base_scene,classification,visual_status,gameplay_status;
      public bool actual_glbs,original_colliders_preserved,real_bridge_placed,art_approved,physical_mobile_tested;
      public int sources,instances,renderer_count;
      public string[] names,source_sha256;
      public float collision_deck_top,shore_top,contact_height_deviation_max;
    }
    [MenuItem("Eldoria/Valoria/Build E actual library bridge on ABCD")]
    public static async void Build(){
      try {await BuildInternal();Debug.Log("VALORIA_LAYERED_REAL_BRIDGE_E_COMPLETE");EditorApplication.Exit(0);}
      catch(Exception ex){Debug.LogError("VALORIA_LAYERED_REAL_BRIDGE_E_FAILED "+ex);EditorApplication.Exit(1);}
    }
    static void Gate(bool x,string m){if(!x)throw new InvalidOperationException("REAL_BRIDGE_E: "+m);}
    static Bounds BoundsOf(GameObject o){
      var r=o.GetComponentsInChildren<Renderer>(true);
      Gate(r.Length>0,"Missing actual mesh: "+o.name);
      var b=r[0].bounds;foreach(var q in r.Skip(1))b.Encapsulate(q.bounds);return b;
    }
    static async Task Import(string id,Transform parent){
      string p=Path.GetFullPath(Input+"/"+id+".glb");
      Gate(File.Exists(p),"GLB source missing "+p);
      using(var file=File.OpenRead(p))using(var hash=SHA256.Create()){
        var actual=BitConverter.ToString(hash.ComputeHash(file)).Replace("-","").ToLowerInvariant();
        Gate(actual==Hashes[Array.IndexOf(Ids,id)],"Archived source hash mismatch "+id);
      }
      var gltf=new GltfImport(null,new UninterruptedDeferAgent());
      Gate(await gltf.Load(new Uri(p).AbsoluteUri,new ImportSettings{GenerateMipMaps=true,AnisotropicFilterLevel=2}),
        "glTF load error "+id);
      var t=new GameObject("_HIDDEN_SOURCE_E_"+id);t.transform.SetParent(parent,false);
      Gate(await gltf.InstantiateMainSceneAsync(t.transform),"glTF real mesh not instantiated: "+id);
      t.SetActive(false);imported.Add(id,t);
    }
    static GameObject Place(string id,Transform parent,Vector3 where,float srcXWidth,float yaw,float desiredTopY){
      GameObject o=UnityEngine.Object.Instantiate(imported[id],parent);
      o.name="REAL_BRIDGE_SOURCE_"+id; o.SetActive(true);o.transform.rotation=Quaternion.identity;
      o.transform.position=where;
      var b=BoundsOf(o);
      var factor=srcXWidth/Mathf.Max(b.size.x,.03f);
      Gate(factor>=.22f&&factor<=5.2f,"Bridge scale outside native source family "+id+" factor "+factor);
      o.transform.localScale*=factor;
      o.transform.rotation=Quaternion.Euler(0,yaw,0);
      b=BoundsOf(o);
      o.transform.position+=new Vector3(where.x-b.center.x,desiredTopY-b.max.y,where.z-b.center.z);
      Gate(Mathf.Abs(BoundsOf(o).max.y-desiredTopY)<.01f,"Failed to align bridge top");
      return o;
    }
    static void Persist(UnityEngine.Object ob){
      if(ob==null||AssetDatabase.Contains(ob)||!persisted.Add(ob.GetInstanceID()))return;
      if(!(ob is Mesh)&&!(ob is Material)&&!(ob is Texture2D))return;
      AssetDatabase.CreateAsset(ob,Generated+"/E"+(index++).ToString("D5")+
        (ob is Material?".mat":".asset"));
    }
    static void Shot(string name,float size,int w,int h){
      var oldSize=view.orthographicSize;var oldTarget=view.targetTexture;
      var rt=new RenderTexture(w,h,24){antiAliasing=4};var tex=new Texture2D(w,h,TextureFormat.RGB24,false);
      var active=RenderTexture.active;
      try {
        view.orthographicSize=size;view.targetTexture=rt;view.Render();view.Render();RenderTexture.active=rt;
        tex.ReadPixels(new Rect(0,0,w,h),0,0);tex.Apply();
        File.WriteAllBytes(Path.Combine(Evidence,name+".png"),tex.EncodeToPNG());
      }
      finally {
        view.targetTexture=oldTarget;view.orthographicSize=oldSize;RenderTexture.active=active;
        rt.Release();UnityEngine.Object.DestroyImmediate(rt);UnityEngine.Object.DestroyImmediate(tex);
      }
    }
    static async Task BuildInternal(){
      Gate(File.Exists(InScene),"No certified ABCD scene source");
      foreach(string id in Ids)Gate(File.Exists(Input+"/"+id+".glb"),"Missing selected GLB: "+id);
      EditorSceneManager.OpenScene(InScene,OpenSceneMode.Single);
      view=Camera.main;
      Gate(view!=null&&view.orthographic,"Historical QA camera lost");
      const float top=.38f;
      var deck=UnityEngine.Object.FindObjectsByType<Transform>(FindObjectsSortMode.None)
          .FirstOrDefault(x=>x.name=="TEMPORARY bridge / continuous deck and collider");
      Gate(deck!=null&&deck.GetComponent<BoxCollider>()!=null,"Original bridge collider missing");
      var collider=deck.GetComponent<BoxCollider>();
      Gate(Mathf.Abs(deck.position.y+deck.localScale.y*.5f-top)<.001f,"Collider grade drift");
      // Hide primitive faces but keep the physical collision contract and the original model for reversibility.
      deck.GetComponent<Renderer>().enabled=false;
      var paras=UnityEngine.Object.FindObjectsByType<Transform>(FindObjectsSortMode.None)
          .Where(x=>x.name.StartsWith("GREYBOX bridge parapet")).ToArray();
      Gate(paras.Length==2,"Original parapets missing");
      foreach(var p in paras)p.GetComponent<Renderer>().enabled=false;
      if(AssetDatabase.IsValidFolder(Generated))AssetDatabase.DeleteAsset(Generated);
      Directory.CreateDirectory(Generated);Directory.CreateDirectory(Evidence);AssetDatabase.Refresh();
      var templates=new GameObject("HIDDEN ORIGINAL BRIDGE GLB INPUTS").transform;
      var assembly=new GameObject("LAYER E - REAL SELECTED 3D BRIDGE").transform;
      foreach(var id in Ids)await Import(id,templates);
      templates.gameObject.SetActive(false);
      var instances=new List<string>();float worstGap=0f;
      // Four contiguous bridge spans covering the 10.3m entry channel.
      foreach(float z in new[]{-18.35f,-15.75f,-13.15f,-10.55f}){
        var span=Place("bridge_span_4m",assembly,new Vector3(0,.1f,z),2.8f,90,top);
        instances.Add("bridge_span_4m");
        worstGap=Mathf.Max(worstGap,Mathf.Abs(BoundsOf(span).max.y-top));
      }
      // Real stone arches below the deck. The Unity physics strip remains unchanged.
      foreach(float z in new[]{-16.95f,-12.65f}){
        var arch=Place("arch_bridge_support",assembly,new Vector3(0,-1.84f,z),3.0f,90,.24f);
        instances.Add("arch_bridge_support");
        worstGap=Mathf.Max(worstGap,Mathf.Abs(BoundsOf(arch).max.y-.24f));
      }
      foreach(float z in new[]{-18.8f,-16f,-13.2f,-10.4f}){
        foreach(int side in new[]{-1,1}){
          Place("stone_slab_cap",assembly,new Vector3(side*2.0f,.5f,z),.65f,90,.97f);
          instances.Add("stone_slab_cap");
        }
      }
      Gate(instances.Count==14,"Complete 3D bridge assembly should have 14 source instances");
      foreach(var renderer in templates.GetComponentsInChildren<Renderer>(true).Concat(
            assembly.GetComponentsInChildren<Renderer>(true))){
        foreach(var material in renderer.sharedMaterials){
          if(material==null)continue;
          foreach(string prop in material.GetTexturePropertyNames())Persist(material.GetTexture(prop));
          Persist(material);
        }
      }
      foreach(var x in templates.GetComponentsInChildren<MeshFilter>(true))Persist(x.sharedMesh);
      foreach(var x in assembly.GetComponentsInChildren<MeshFilter>(true))Persist(x.sharedMesh);
      foreach(var x in templates.GetComponentsInChildren<SkinnedMeshRenderer>(true))Persist(x.sharedMesh);
      foreach(var x in assembly.GetComponentsInChildren<SkinnedMeshRenderer>(true))Persist(x.sharedMesh);
      AssetDatabase.SaveAssets();
      PrefabUtility.SaveAsPrefabAsset(assembly.gameObject,Generated+"/RealBridge_3SourceTypes.prefab");
      Physics.SyncTransforms();
      foreach(float z in new[]{-18f,-15f,-12f,-10.3f}){
        var point=new Vector3(0,top,z);
        Gate(Physics.Raycast(point+Vector3.up*.15f,Vector3.down,out var hit,1.0f,~0,
          QueryTriggerInteraction.Ignore),"Original bridge collider blocked at "+z);
        Gate(Mathf.Abs(hit.point.y-top)<.02f,"Bridge walkable height changed at "+z);
      }
      Gate(EditorSceneManager.SaveScene(SceneManager.GetActiveScene(),OutScene,true),"Saving ABCDE scene failed");
      Shot("master-bridge-E-home-12",12,1536,1024);
      Shot("master-bridge-E-home-19",19,1536,1024);
      Shot("master-bridge-E-portrait-12",12,390,844);
      var manifest=new Manifest{unity=UnityEngine.Application.unityVersion,scene=OutScene,
        base_scene=InScene,classification="AUTHORED 3D SOURCE CANDIDATE / REMAINING GREYBOX",
        visual_status="NOT_APPROVED_FULL_SCENE_MUST_BE_REVIEWED",gameplay_status="NOT_CERTIFIED",
        actual_glbs=true,original_colliders_preserved=true,real_bridge_placed=true,
        art_approved=false,physical_mobile_tested=false,sources=Ids.Length,instances=instances.Count,
        renderer_count=assembly.GetComponentsInChildren<Renderer>(true).Length,
        names=instances.ToArray(),source_sha256=Ids.Select((id,i)=>id+" "+Hashes[i]).ToArray(),
        collision_deck_top=top,shore_top=top,contact_height_deviation_max=worstGap};
      File.WriteAllText(Path.Combine(Evidence,"real-bridge-E-evidence.json"),JsonUtility.ToJson(manifest,true));
      Debug.Log("VALORIA_LAYER_E_BRIDGE_REAL_GEOMETRY sources="+Ids.Length+
                " instances="+instances.Count+" bridge_walking_contact_y="+top);
    }
  }
}
