// ISOLATED VALORIA63 - not a replacement for ProductionSlice/Runtime/Valoria.unity
// Unity 6000.3.23f1, URP17.3 / glTFast 6.14.1. Requires 63 immutable staged GLBs.
// In-editor entry: Eldoria / Art / Build clean Valoria from 63 actual GLBs.
// CI: -executeMethod Eldoria.Valoria63.Editor.Valoria63CleanSceneBuilder.Build
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
using UnityEngine.Rendering;
using GLTFast;

namespace Eldoria.Valoria63.Editor
{
  public static class Valoria63CleanSceneBuilder
  {
    const string Root="Assets/Eldoria/ArtTests/Valoria63CleanScene";
    const string Staged=Root+"/Sources";
    const string Generated=Root+"/Generated";
    const string Output=Root+"/Valoria_BastionI_Modular63_Clean.unity";
    const string Names=@"arch_bridge_support arch_complete_major arch_complete_minor arch_keystone_major arch_keystone_minor arch_voussoir_major arch_voussoir_minor arrow_slit_frame banner_royal_blue banner_small boulder_1 boulder_2 boulder_3 brazier_pedestal bridge_span_4m cliff_rock_column cliff_stone_chip cobble_pebbles dirt_ground_patch grass_ground_patch grass_tuft_1 grass_tuft_2 merlon_low merlon_tall mortar_joint_recess parapet_walk_4m paver_flagstone paver_rect_long paver_rect_short pine_medium pine_small pine_tall portcullis_large portcullis_small reinforced_oak_gate river_foam_line river_water_tile scaffold_small scaffold_watch_platform shrub_1 shrub_2 shrub_3 stair_step staircase_flight_12 stone_ashlar_01 stone_ashlar_02 stone_ashlar_03 stone_ashlar_04 stone_corner_quoin stone_half_block stone_slab_cap timber_beam_1 timber_beam_2 timber_brace_X torch_wall tower_square_short tower_square_tall wall_buttress wall_corner_90 wall_course_4m wall_panel_crenellated_4m wall_panel_regular_4m wall_panel_ruined_4m";
    static string[] All=>Names.Split(new[]{' ','\n','\r','\t'},StringSplitOptions.RemoveEmptyEntries);
    static Dictionary<string,GameObject> templates;
    static Dictionary<string,int> counts;
    static List<string> hashes;
    static List<UnityEngine.Object> toPersist;
    static HashSet<int> persisted;
    static Transform sceneRoot;
    static Material limestone,soil,rock,water,grass,road;
    static Camera view;
    static int nativeCounter;
    static string Repo=>Path.GetFullPath(Path.Combine(UnityEngine.UnityEngine.Application.dataPath,"..",".."));
    static string Evidence=>Path.Combine(Repo,"docs/evidence/valoria63-clean-scene-20261016");

    [MenuItem("Eldoria/Art/Build clean Valoria from 63 actual GLBs")]
    public static async void Build()
    {
      try { await BuildAndSave(); Debug.Log("VALORIA63_CLEAN_SCENE_COMPLETE assets=63"); EditorApplication.Exit(0); }
      catch(Exception e) { Debug.LogError("VALORIA63_CLEAN_SCENE_FAILED "+e);EditorApplication.Exit(1); }
    }
    static async Task BuildAndSave()
    {
      if(AssetDatabase.IsValidFolder(Generated))AssetDatabase.DeleteAsset(Generated);
      Directory.CreateDirectory(Generated);Directory.CreateDirectory(Evidence);
      if(All.Length!=63 || All.Distinct().Count()!=63)throw new Exception("Invalid 63 source IDs");
      foreach(string id in All)if(!File.Exists(Staged+"/"+id+".glb"))throw new FileNotFoundException("Original 63 GLB input missing",id);
      templates=new Dictionary<string,GameObject>();
      counts=new Dictionary<string,int>();
      hashes=new List<string>();
      toPersist=new List<UnityEngine.Object>();
      persisted=new HashSet<int>();
      nativeCounter=0;
      EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
      sceneRoot=new GameObject("VALORIA BASTION I - CLEAN ISOLATED COMPOSITION").transform;
      var sources=new GameObject("_IMPORTED_TEMPLATES_NOT_RENDERED").transform;
      sources.SetParent(sceneRoot);
      foreach(string id in All)await Import(id,sources);
      sources.gameObject.SetActive(false);
      limestone=Mat("Warm limestone",new Color(.58f,.55f,.49f));
      soil=Mat("Moss and earth",new Color(.36f,.36f,.23f));
      grass=Mat("Sunlit olive meadow",new Color(.39f,.43f,.24f));
      rock=Mat("Cliff slate",new Color(.29f,.31f,.35f));
      water=Mat("Deep blue river",new Color(.045f,.19f,.27f));
      road=Mat("Weathered stone path",new Color(.51f,.46f,.39f));
      BuildLandform();
      BuildBridgeAndOuterGate();
      BuildRoadAndUpperCitadel();
      BuildNaturalEdge();
      PlaceRemainingArchitecture();
      SetLightingAndCamera();

      // Requirement: every one of the 63 real library sources participates in
      // the composition, without using an old Bastion I scene or fake asset cube.
      var unused=All.Where(x=>!counts.ContainsKey(x)).ToArray();
      if(unused.Length!=0)throw new Exception("Not all 63 sources in scene: "+String.Join(",",unused));
      int moduleCount=counts.Values.Sum();
      if(moduleCount<120)throw new Exception("Insufficiently assembled module placement "+moduleCount);
      foreach(var c in new[]{new Vector3(0,0,-40),new Vector3(0,0,-6),
                               new Vector3(2,6,32),new Vector3(0,6,49)})
         if(!Physics.Raycast(c+Vector3.up*45,Vector3.down,100,~0,QueryTriggerInteraction.Ignore))
            throw new Exception("No physical ground at route "+c);
      foreach(var o in toPersist)Persist(o);
      foreach(Renderer r in sceneRoot.GetComponentsInChildren<Renderer>(true))
      {
        foreach(var m in r.sharedMaterials)
        {
          if(m==null)continue;
          foreach(string prop in m.GetTexturePropertyNames())Persist(m.GetTexture(prop));
          Persist(m);
        }
      }
      foreach(MeshFilter f in sceneRoot.GetComponentsInChildren<MeshFilter>(true))Persist(f.sharedMesh);
      foreach(SkinnedMeshRenderer s in sceneRoot.GetComponentsInChildren<SkinnedMeshRenderer>(true))Persist(s.sharedMesh);
      AssetDatabase.SaveAssets();
      if(!EditorSceneManager.SaveScene(SceneManager.GetActiveScene(),Output,true))throw new Exception("Scene not saved");
      RenderFrame("valoria63-clean-landscape",1536,1024);
      RenderFrame("valoria63-clean-wide",1280,720);
      RenderFrame("valoria63-clean-portrait",390,844);
      var manifest=new CompositionReport{unity=UnityEngine.UnityEngine.Application.unityVersion,scene=Output,source_count=63,
        module_instances=moduleCount,distinct_sources=counts.Count,used_existing_glbs=true,
        new_modeling=false,art_approved=false,gameplay_certified=false,mobile_certified=false,
        camera_position=view.transform.position,camera_rotation=view.transform.eulerAngles,
        source_sha256=hashes.ToArray(),source_placement_counts=counts.OrderBy(k=>k.Key).Select(k=>k.Key+":"+k.Value).ToArray()};
      File.WriteAllText(Path.Combine(Evidence,"assembly-evidence.json"),JsonUtility.ToJson(manifest,true));
      Debug.Log("VALORIA63_NATIVE_SCENE_WRITTEN "+Output+" instances="+moduleCount);
    }
    [Serializable] class CompositionReport
    {
      public string unity,scene;
      public int source_count,module_instances,distinct_sources;
      public bool used_existing_glbs,new_modeling,art_approved,gameplay_certified,mobile_certified;
      public Vector3 camera_position,camera_rotation;
      public string[] source_sha256,source_placement_counts;
    }
    static async Task Import(string id,Transform parent)
    {
      string p=Path.GetFullPath(Staged+"/"+id+".glb");
      using(var file=File.OpenRead(p))using(var hash=SHA256.Create())
        hashes.Add(id+" "+BitConverter.ToString(hash.ComputeHash(file)).Replace("-","").ToLowerInvariant());
      var gltf=new GltfImport(null,new UninterruptedDeferAgent());
      if(!await gltf.Load(new Uri(p).AbsoluteUri,new ImportSettings{GenerateMipMaps=true,AnisotropicFilterLevel=2}))
        throw new Exception("Real glTF import failed: "+id);
      var template=new GameObject("_SOURCE_"+id);
      template.transform.SetParent(parent,false);
      if(!await gltf.InstantiateMainSceneAsync(template.transform))throw new Exception("GLTF instantiation failed "+id);
      if(template.GetComponentsInChildren<Renderer>(true).Length==0)throw new Exception("GLB has no rendered volume "+id);
      template.SetActive(false);
      templates.Add(id,template);
    }
    static Material Mat(string name,Color c)
    {
      var shader=Shader.Find("Universal Render Pipeline/Lit");
      if(shader==null)throw new Exception("URP Lit shader unavailable");
      var material=new Material(shader){name="Valoria63_"+name,color=c};
      material.SetFloat("_Smoothness",.12f);
      material.enableInstancing=true;
      toPersist.Add(material);
      return material;
    }
    static Bounds Extents(GameObject o)
    {
      var all=o.GetComponentsInChildren<Renderer>(true);
      if(all.Length==0)throw new Exception("No renderer on "+o.name);
      Bounds b=all[0].bounds;
      foreach(var r in all.Skip(1))b.Encapsulate(r.bounds);
      return b;
    }
    static GameObject Place(string id,Transform parent,Vector3 at,float desiredWidth=0,float yaw=0)
    {
      var t=templates[id];
      var o=UnityEngine.Object.Instantiate(t,parent);
      o.name=id+"__"+(counts.ContainsKey(id)?counts[id]+1:1);
      o.SetActive(true);
      o.transform.rotation=Quaternion.Euler(0,yaw,0);
      o.transform.position=at;
      if(desiredWidth>0)
      {
        var b=Extents(o);
        // Never flatten or stretch a three-dimensional architectural module.
        float factor=desiredWidth/Mathf.Max(.03f,b.size.x);
        if(factor<.22f||factor>5.2f)throw new Exception("Implausible module scaling "+id+" factor="+factor);
        o.transform.localScale*=factor;
      }
      var fit=Extents(o);
      o.transform.position+=new Vector3(at.x-fit.center.x,at.y-fit.min.y,at.z-fit.center.z);
      counts[id]=counts.ContainsKey(id)?counts[id]+1:1;
      return o;
    }
    static Transform Group(string name){var g=new GameObject(name).transform;g.SetParent(sceneRoot,false);return g;}
    static GameObject Cube(string name,Vector3 center,Vector3 size,Material m,Transform parent)
    {
      // Unity scene-level surface only; not a new library asset.
      var obj=GameObject.CreatePrimitive(PrimitiveType.Cube);
      obj.name=name;obj.transform.SetParent(parent,false);
      obj.transform.position=center;obj.transform.localScale=size;
      obj.GetComponent<Renderer>().sharedMaterial=m;
      return obj;
    }
    static void Deck(string name,Vector2[] shape,float top,float baseHeight,Transform parent)
    {
      var vertices=new List<Vector3>();
      foreach(var p in shape)vertices.Add(new Vector3(p.x,top,p.y));
      int n=vertices.Count;
      foreach(var p in shape)vertices.Add(new Vector3(p.x,baseHeight,p.y));
      var tris=new List<int>();
      for(int i=1;i<n-1;i++)tris.AddRange(new[]{0,i+1,i});
      for(int i=0;i<n;i++){int j=(i+1)%n;tris.AddRange(new[]{i,i+n,j,i+n,j+n,j});}
      var mesh=new Mesh{name=name};mesh.indexFormat=IndexFormat.UInt32;
      mesh.SetVertices(vertices);mesh.SetTriangles(tris,0);mesh.RecalculateBounds();mesh.RecalculateNormals();
      // Geometry is authored from a clockwise x/z perimeter; guarantee that
      // upper-face normal points +Y (raycast, shading and collision depend on it).
      if(mesh.normals.Length>0 && mesh.normals[0].y<0){
        for(int i=0;i<tris.Count;i+=3){var temp=tris[i+1];tris[i+1]=tris[i+2];tris[i+2]=temp;}
        mesh.SetTriangles(tris,0);mesh.RecalculateNormals();
      }
      toPersist.Add(mesh);
      var o=new GameObject(name);o.transform.SetParent(parent,false);
      o.AddComponent<MeshFilter>().sharedMesh=mesh;
      o.AddComponent<MeshRenderer>().sharedMaterial=name.StartsWith("Upper")?grass:soil;
      o.AddComponent<MeshCollider>().sharedMesh=mesh;
    }
    static void BuildLandform()
    {
      var ground=Group("01 GENUINE LANDSCAPE - plateau and cliffs (Unity-native)");
      Deck("Main continuous buildable plateau",new[]{
        new Vector2(-45,-33),new Vector2(-50,-20),new Vector2(-48,3),new Vector2(-44,21),
        new Vector2(-31,31),new Vector2(27,34),new Vector2(47,23),new Vector2(50,5),
        new Vector2(47,-20),new Vector2(41,-34),new Vector2(14,-39),new Vector2(-18,-38)},0,-14,ground);
      Deck("Upper citadel ridge",new[]{
        new Vector2(-29,27),new Vector2(-27,40),new Vector2(-23,57),new Vector2(-13,61),
        new Vector2(25,59),new Vector2(33,53),new Vector2(30,33),new Vector2(20,25)},6,0,ground);
      Cube("Below bridge flowing river",new Vector3(0,-13.8f,-45),new Vector3(120,.25f,24),water,ground);
      Cube("Bridge shore beyond river",new Vector3(0,-14,-64),new Vector3(70,9,18),rock,ground);
      Cube("Bridge far end walkable surface",new Vector3(0,-8.6f,-59),new Vector3(14,.3f,8),soil,ground);
      var parcels=Group("INTERACTION RESERVES - Bastion I to Bastion II");
      foreach(var x in new[]{-17,15})
      {
        var parcel=new GameObject("BUILDABLE parcel (no obstructions)");parcel.transform.SetParent(parcels,false);
        parcel.transform.position=new Vector3(x,.35f,5);
        var box=parcel.AddComponent<BoxCollider>();box.isTrigger=true;box.size=new Vector3(18,.5f,15);
      }
    }
    static void BuildBridgeAndOuterGate()
    {
      var bridge=Group("02 LOWER BRIDGE - structural kit");
      var bridgeCollider=Cube("Continuous walkable bridge deck collider",new Vector3(0,-.1f,-46),
        new Vector3(6.0f,.3f,35),road,bridge);
      bridgeCollider.GetComponent<Renderer>().enabled=false;
      for(int z=-60;z<=-32;z+=4)
      {
        Place("bridge_span_4m",bridge,new Vector3(0,-.15f,z),6.1f,90);
        for(int side=-1;side<=1;side+=2)
          Place("stone_slab_cap",bridge,new Vector3(side*3.8f,.75f,z),.85f,90);
      }
      for(int z=-54;z<=-36;z+=8)
        Place("arch_bridge_support",bridge,new Vector3(0,-12,z),3.0f,90);
      Place("river_water_tile",bridge,new Vector3(-7,-13.65f,-45),4.0f);
      Place("river_foam_line",bridge,new Vector3(6,-13.6f,-47),2.4f);
      var entrance=Group("03 OUTER GATE - arch, true volume and towers");
      Place("tower_square_short",entrance,new Vector3(-6,0,-26),6.0f);
      Place("tower_square_short",entrance,new Vector3(6,0,-26),6.0f);
      Place("arch_complete_major",entrance,new Vector3(0,0,-26),5.5f);
      Place("portcullis_large",entrance,new Vector3(0,.1f,-27),3.6f);
      Place("reinforced_oak_gate",entrance,new Vector3(0,0,-23.9f),3.4f);
      Place("arch_keystone_major",entrance,new Vector3(0,5.7f,-27.3f),.75f);
      Place("arch_voussoir_major",entrance,new Vector3(-2.6f,3.8f,-27.1f),.60f,30);
      Place("arrow_slit_frame",entrance,new Vector3(-6,3.5f,-29.1f),.9f);
      Place("banner_royal_blue",entrance,new Vector3(-5.7f,5.1f,-30),1.3f);
      Place("banner_royal_blue",entrance,new Vector3(5.7f,5.1f,-30),1.3f);
      foreach(int side in new[]{-1,1})
      {
        Place("brazier_pedestal",entrance,new Vector3(side*4.0f,.1f,-32),.8f);
        Place("torch_wall",entrance,new Vector3(side*3.3f,2.1f,-27.7f),.4f);
        int i=0;
        foreach(int offset in new[]{11,15,19,23,27,31,35})
        {
          float x=side*offset, z=-25+Mathf.Abs(offset-11)*.14f;
          Place((i++%3)==0?"wall_panel_regular_4m":"wall_panel_crenellated_4m",
            entrance,new Vector3(x,0,z),4.0f);
          if(i==3 || i==6)
            Place("wall_buttress",entrance,new Vector3(x,0,z+2.1f),1.0f);
        }
        Place("wall_corner_90",entrance,new Vector3(side*39,0,-18),3.7f,side*20);
        Place("wall_panel_ruined_4m",entrance,new Vector3(side*42,0,-12),4.0f,90);
      }
      Place("merlon_low",entrance,new Vector3(-9.4f,3.1f,-25),.70f);
      Place("merlon_tall",entrance,new Vector3(9.4f,3.1f,-25),.72f);
    }
    static void BuildRoadAndUpperCitadel()
    {
      var route=Group("04 CONTINUOUS PLAYER ROUTE - 3D walkable modules");
      for(int z=-22;z<=18;z+=2)
      {
        for(int x=-2;x<=2;x+=2)
          Place("paver_flagstone",route,new Vector3(x,.02f,z),1.8f);
      }
      for(int z=27;z<=44;z+=2)
        for(int x=0;x<=4;x+=2)Place("paver_rect_long",route,new Vector3(x,6.05f,z),1.7f);
      var stairs=Place("staircase_flight_12",route,new Vector3(1.5f,.25f,23),5.3f,0);
      // Physics ramp for progression: the real modular stairs remain visible.
      var ramp=GameObject.CreatePrimitive(PrimitiveType.Cube);
      ramp.name="Navigable stairs collider strip";ramp.transform.SetParent(route,false);
      ramp.transform.position=new Vector3(1.5f,3,22.6f);
      ramp.transform.localScale=new Vector3(5.8f,.20f,11);
      ramp.transform.rotation=Quaternion.Euler(28,0,0);
      ramp.GetComponent<Renderer>().enabled=false;
      Place("stair_step",route,new Vector3(1.5f,5.78f,27),5.2f);
      var upper=Group("05 UPPER KEEP - real modular construction");
      Place("tower_square_tall",upper,new Vector3(-7,6,47),7.0f);
      Place("tower_square_short",upper,new Vector3(10,6,47),6.1f);
      Place("arch_complete_minor",upper,new Vector3(2,6,46.2f),4.8f);
      Place("portcullis_small",upper,new Vector3(2,6,44.8f),2.9f);
      Place("arch_keystone_minor",upper,new Vector3(2,10.8f,44.9f),.50f);
      Place("arch_voussoir_minor",upper,new Vector3(4,10.2f,44.9f),.42f);
      for(int x=-26;x<=26;x+=4)
      {
        if(x>=-9&&x<=12)continue;
        Place("wall_panel_crenellated_4m",upper,new Vector3(x,6,47),4.0f);
      }
      for(int z=31;z<=53;z+=4)
      {
        Place("wall_panel_regular_4m",upper,new Vector3(-26,6,z),4.0f,90);
        Place("wall_panel_crenellated_4m",upper,new Vector3(26,6,z),4.0f,90);
      }
      Place("parapet_walk_4m",upper,new Vector3(-16,9.1f,47),4.0f);
      Place("scaffold_watch_platform",upper,new Vector3(-16,6,40),3.7f);
      Place("scaffold_small",upper,new Vector3(-22,6,33),3.7f);
      Place("timber_beam_1",upper,new Vector3(-20,6,33),.35f);
      Place("timber_beam_2",upper,new Vector3(-17,6,33),.35f);
      Place("timber_brace_X",upper,new Vector3(-18.5f,6,34),2.8f);
      Place("banner_small",upper,new Vector3(-5.8f,11.8f,43),.85f);
      Place("banner_royal_blue",upper,new Vector3(9.2f,12,44),1.3f);
      Place("stone_corner_quoin",upper,new Vector3(-10.9f,6,46),.92f);
      Place("wall_course_4m",upper,new Vector3(-22,6,42),4.0f,90);
      Place("mortar_joint_recess",upper,new Vector3(-25,7.6f,46),1.0f,90);
      foreach(var x in new[]{-12f,-11f,-10f,-9f})
        Place("stone_ashlar_0"+((int)(x+13)),upper,new Vector3(x,6,41),.8f);
      Place("stone_half_block",upper,new Vector3(-14,6,42),.7f);
      Place("stone_slab_cap",upper,new Vector3(-14,7,43),.8f);
      Place("banner_royal_blue",upper,new Vector3(-6,12.8f,45),1.2f);
      Place("brazier_pedestal",upper,new Vector3(-4,6.1f,43),.85f);
      Place("brazier_pedestal",upper,new Vector3(7,6.1f,43),.85f);
    }
    static void BuildNaturalEdge()
    {
      var nature=Group("06 EXISTING VALORIA NATURE - cliff, tree, rock, foliage, water");
      var rng=new System.Random(63);
      for(int i=0;i<32;i++)
      {
        float side=i%2==0?-1:1;
        float z=-30+(i/2)*4.7f;
        float x=side*(42+(i%4)*2);
        Place("cliff_rock_column",nature,new Vector3(x,-12,z),3.7f,90*i);
        if(i%3==0)Place("cliff_stone_chip",nature,new Vector3(x-side*2,-1,z),.9f);
      }
      for(int i=0;i<34;i++)
      {
        float z=-34+(i%17)*6.2f;
        float side=i%2==0?-1:1;
        float x=side*(43+(i%6)*2.2f);
        string kind=i%3==0?"pine_tall":(i%3==1?"pine_medium":"pine_small");
        Place(kind,nature,new Vector3(x,0,z),i%3==0?5.5f:3.2f,(i*43)%360);
      }
      for(int i=0;i<18;i++)
      {
        float x=-35+i*4.3f,z=55+(i%3)*3.2f;
        Place(i%3==0?"pine_tall":(i%3==1?"pine_medium":"pine_small"),
          nature,new Vector3(x,6,z),i%2==0?4.3f:3.3f,i*37);
      }
      for(int i=0;i<42;i++)
      {
        float x=(float)rng.NextDouble()*86-43,z=(float)rng.NextDouble()*72-30;
        if(Mathf.Abs(x)<9 && z<23)continue; // road and expansion reserved
        Place("shrub_"+(i%3+1),nature,new Vector3(x,0,z),.7f+(i%3)*.2f,i*47);
        if(i%2==0)Place("grass_tuft_"+(i%2+1),nature,new Vector3(x+1.2f,0,z+.7f),.7f);
      }
      for(int i=0;i<34;i++)
      {
        float side=i%2==0?-1:1,x=side*(15+i%9*2.3f),z=(i*13)%54-29;
        string b=i%3==0?"boulder_1":(i%3==1?"boulder_2":"boulder_3");
        Place(b,nature,new Vector3(x,0,z),1.2f);
        if(i%4==0)Place("cobble_pebbles",nature,new Vector3(x+1.2f,0,z),1.3f);
      }
      Place("grass_ground_patch",nature,new Vector3(17,.01f,15),4.0f);
      Place("dirt_ground_patch",nature,new Vector3(-18,.01f,14),4.0f);
      Place("grass_tuft_2",nature,new Vector3(12,0,10),.45f);
      Place("paver_rect_short",nature,new Vector3(11,.02f,-8),.80f);
    }
    static void PlaceRemainingArchitecture()
    {
      // Small architectural modules are integrated into wall-contact areas,
      // not dumped into an asset gallery and not used to cover existing geometry.
      var details=Group("07 KEEP CONSTRUCTION DETAILS - complementary modular sockets");
      Place("arrow_slit_frame",details,new Vector3(9,9.5f,43),.65f);
      Place("merlon_low",details,new Vector3(-12,9.5f,47),.65f);
      Place("merlon_tall",details,new Vector3(-20,9.5f,47),.65f);
      Place("wall_buttress",details,new Vector3(24,6,39),1.15f,90);
      Place("torch_wall",details,new Vector3(-2.5f,8.5f,44),.40f);
    }
    static void SetLightingAndCamera()
    {
      var lighting=Group("08 DUSK LIGHTING - physically real Unity camera");
      var sun=new GameObject("Sunset low-key directional sun").AddComponent<Light>();
      sun.transform.SetParent(lighting,false);
      sun.type=LightType.Directional;sun.color=new Color(1,.81f,.62f);
      sun.intensity=1.20f;sun.shadows=LightShadows.Soft;sun.transform.rotation=Quaternion.Euler(38,225,0);
      foreach(var p in new[]{new Vector3(-4.2f,3.3f,-31),
                              new Vector3(4.2f,3.3f,-31),
                              new Vector3(-4,9.0f,42),new Vector3(7,9.0f,42)})
      {
        var torch=new GameObject("Torch glow - local").AddComponent<Light>();
        torch.transform.SetParent(lighting,false);torch.transform.position=p;
        torch.type=LightType.Point;torch.color=new Color(1,.41f,.12f);
        torch.range=8;torch.intensity=3.1f;torch.shadows=LightShadows.None;
      }
      RenderSettings.ambientMode=AmbientMode.Trilight;
      RenderSettings.ambientSkyColor=new Color(.40f,.48f,.60f);
      RenderSettings.ambientEquatorColor=new Color(.36f,.36f,.37f);
      RenderSettings.ambientGroundColor=new Color(.18f,.17f,.18f);
      RenderSettings.fog=true;
      RenderSettings.fogColor=new Color(.27f,.34f,.43f);RenderSettings.fogDensity=.0018f;
      view=new GameObject("Main Camera - composition match").AddComponent<Camera>();
      view.tag="MainCamera";view.transform.SetParent(lighting,false);
      view.orthographic=true;view.orthographicSize=47;view.nearClipPlane=.1f;view.farClipPlane=500;
      view.transform.position=new Vector3(64,84,-99);view.transform.LookAt(new Vector3(0,0,7));
      view.clearFlags=CameraClearFlags.SolidColor;view.backgroundColor=new Color(.20f,.29f,.42f);
      view.allowHDR=true;view.allowMSAA=true;
      // Do not touch game production camera or scripts.
    }
    static void Persist(UnityEngine.Object asset)
    {
      if(asset==null || AssetDatabase.Contains(asset) || !persisted.Add(asset.GetInstanceID()))return;
      if(!(asset is Mesh) && !(asset is Material) && !(asset is Texture2D))return;
      string ext=asset is Material?".mat":".asset";
      string path=Generated+"/"+(nativeCounter++).ToString("D5")+ext;
      AssetDatabase.CreateAsset(asset,path);
    }
    static void RenderFrame(string id,int width,int height)
    {
      var prior=RenderTexture.active;
      var original=view.targetTexture;
      var oldAspect=view.aspect;
      var rt=new RenderTexture(width,height,24){antiAliasing=4};
      var image=new Texture2D(width,height,TextureFormat.RGB24,false);
      try
      {
        view.aspect=(float)width/height;
        view.targetTexture=rt;view.Render();view.Render();
        RenderTexture.active=rt;
        image.ReadPixels(new Rect(0,0,width,height),0,0);image.Apply();
        File.WriteAllBytes(Path.Combine(Evidence,id+".png"),image.EncodeToPNG());
      }
      finally {view.targetTexture=original;view.aspect=oldAspect;
        RenderTexture.active=prior;rt.Release();UnityEngine.Object.DestroyImmediate(rt);
        UnityEngine.Object.DestroyImmediate(image);}
    }
    [MenuItem("Eldoria/Art/Validate isolated 63 scene opens")]
    public static void Validate()
    {
      try
      {
        var s=EditorSceneManager.OpenScene(Output,OpenSceneMode.Single);
        if(!s.IsValid() || !s.isLoaded)throw new Exception("Saved scene cannot be loaded");
        var expected=All;
        var names=UnityEngine.Object.FindObjectsByType<Transform>(FindObjectsInactive.Include,FindObjectsSortMode.None)
          .Select(o=>o.name).ToArray();
        foreach(var n in expected)if(!names.Any(x=>x.StartsWith(n+"__",StringComparison.Ordinal)))
          throw new Exception("Missing persisted module "+n);
        var present=UnityEngine.Object.FindObjectsByType<MeshRenderer>(FindObjectsInactive.Exclude,FindObjectsSortMode.None);
        int missing=present.Count(r=>r.GetComponent<MeshFilter>()==null || r.GetComponent<MeshFilter>().sharedMesh==null ||
                                        r.sharedMaterials.Any(m=>m==null));
        if(missing>0)throw new Exception("Broken mesh/material persistence count="+missing);
        foreach(var c in new[]{new Vector3(0,0,-40),new Vector3(0,0,0),new Vector3(2,6,35)})
          if(!Physics.Raycast(c+Vector3.up*40,Vector3.down,100,~0,QueryTriggerInteraction.Ignore))
            throw new Exception("Broken saved-scene collision at "+c);
        Debug.Log("VALORIA63_CLEAN_SCENE_REOPEN_VALIDATED sources=63 renderers="+present.Length);
        EditorApplication.Exit(0);
      }
      catch(Exception e){Debug.LogError("VALORIA63_REOPEN_FAILED "+e);EditorApplication.Exit(1);}
    }
  }
}
