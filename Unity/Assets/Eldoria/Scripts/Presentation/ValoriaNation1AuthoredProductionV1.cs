using System;
using Eldoria.Domain;
using UnityEngine;
using Object=UnityEngine.Object;

namespace Eldoria.Presentation
{
 public static class ValoriaNation1AuthoredProductionV1
 {
  public const string RootName="Valoria · Nation 1 Authored Production v1";
  public static int TerraceModules,MidTierBuildings,DetailProps,WarmLights;

  public static void Apply(Transform canonicalRoot,PlayerState state)
  {
   if(canonicalRoot==null)throw new ArgumentNullException(nameof(canonicalRoot));
   var old=GameObject.Find(RootName);if(old!=null)Object.DestroyImmediate(old);
   var root=new GameObject(RootName).transform;root.SetParent(canonicalRoot,true);
   TerraceModules=MidTierBuildings=DetailProps=WarmLights=0;
   BuildFortificationFamily(root);
   BuildHeroTerracedCore(root);
   ScaleCoreVisuals();
   BuildCivicDensity(root);
   BuildCentralPlaza(root);
   BuildGranaryFields(root);
   BuildFunctionalDressing(root);
   BuildGreenery(root);
   BuildBackdrop(root);
   BuildLighting(root);
   StripGameplay(root.gameObject);
  }

  static void BuildFortificationFamily(Transform root)
  {
   var wall=Resources.Load<GameObject>("Valoria/Nation1/Nation1_Wall_v1");
   var gate=Resources.Load<GameObject>("Valoria/Nation1/Nation1_Gate_v1");
   var tower=Resources.Load<GameObject>("Valoria/Nation1/Nation1_Tower_v1");
   if(wall==null||gate==null||tower==null)throw new InvalidOperationException("Nation1 authored fortification resources missing.");

   foreach(var r in Object.FindObjectsByType<Renderer>(FindObjectsSortMode.None))
   {
    if(r==null||!r.enabled)continue;
    string n=Chain(r.transform);
    bool breakthrough=n.Contains("valoria · breakthrough")&&(n.Contains("wall")||n.Contains("tower")||n.Contains("gate")||n.Contains("curtain")||n.Contains("front ")||n.Contains("west ")||n.Contains("east ")||n.Contains("rear "));
    bool production=n.Contains("flat citadel production")&&(n.Contains("wall")||n.Contains("tower")||n.Contains("gate")||n.Contains("curtain"));
    bool consolidation=n.Contains("art consolidation")&&(n.Contains("wall")||n.Contains("tower")||n.Contains("gate")||n.Contains("retaining")||n.Contains("return")||n.Contains("watchtower")||n.Contains("base"));
    if(breakthrough||production||consolidation)r.enabled=false;
   }

   Fort(root,gate,"main gate",new Vector3(0f,.10f,-6.52f),4.75f,3.45f,0f);
   Fort(root,tower,"gate west tower",new Vector3(-3.30f,.10f,-6.22f),2.18f,3.25f,2f);
   Fort(root,tower,"gate east tower",new Vector3(3.30f,.10f,-6.22f),2.18f,3.25f,-2f);

   foreach(var x in new[]{-8.05f,-5.55f,5.55f,8.05f})
     Fort(root,wall,"front curtain",new Vector3(x,.09f,-6.30f),3.20f,1.62f,0f);

   foreach(var z in new[]{-3.55f,-.45f,2.65f,5.75f,8.15f})
   {
    Fort(root,wall,"west curtain",new Vector3(-9.42f,.09f,z),3.25f,1.60f,90f);
    Fort(root,wall,"east curtain",new Vector3(9.42f,.09f,z),3.25f,1.60f,90f);
   }

   foreach(var x in new[]{-7.30f,-4.40f,-1.45f,1.45f,4.40f,7.30f})
     Fort(root,wall,"rear curtain",new Vector3(x,.09f,9.18f),3.15f,1.48f,0f);

   Fort(root,tower,"west mid tower",new Vector3(-9.42f,.10f,3.78f),1.95f,2.90f,8f);
   Fort(root,tower,"east mid tower",new Vector3(9.42f,.10f,3.78f),1.95f,2.90f,-8f);

   foreach(var s in new[]{
     new Vector4(-9.25f,-6.05f,8f,2.75f),new Vector4(9.25f,-6.05f,-8f,2.75f),
     new Vector4(-9.20f,9.05f,172f,2.60f),new Vector4(9.20f,9.05f,188f,2.60f)})
     Fort(root,tower,"corner tower",new Vector3(s.x,.10f,s.y),2.12f,s.w*1.08f,s.z);

   BuildWallHeraldry(root);
  }

  static void BuildWallHeraldry(Transform root)
  {
   foreach(var spec in new[]{
     new Vector4(-6.15f,1.52f,-6.60f,0f),new Vector4(6.15f,1.52f,-6.60f,0f),
     new Vector4(-9.62f,1.45f,-1.10f,90f),new Vector4(9.62f,1.45f,-1.10f,-90f),
     new Vector4(-9.62f,1.45f,6.30f,90f),new Vector4(9.62f,1.45f,6.30f,-90f)})
   {
    string name="Valoria · Nation1 · wall banner "+spec.x+" "+spec.z;
    ValoriaKit.Banner(name,new Vector3(spec.x,spec.y,spec.z),new Vector3(.34f,1.05f,.055f),new Color(.055f,.15f,.40f,1f));
    var go=GameObject.Find(name);
    if(go!=null)
    {
     go.transform.rotation=Quaternion.Euler(0f,spec.w,0f);
     go.transform.SetParent(root,true);
     StripGameplay(go);
     DetailProps++;
    }
   }
  }

  static void Fort(Transform root,GameObject src,string role,Vector3 p,float footprint,float height,float yaw)
  {
   var go=ValoriaKit.BenchmarkPiece("Valoria · Nation1 fortification · "+role,src,p,footprint,height,Quaternion.Euler(0,yaw,0));
   if(go==null)return;go.transform.SetParent(root,true);StripGameplay(go);TerraceModules++;
  }

  static string Chain(Transform t){string s="";for(;t!=null;t=t.parent)s+="|"+t.name.ToLowerInvariant();return s;}

  static void BuildHeroTerracedCore(Transform root)
  {
   var wall=Resources.Load<GameObject>("Valoria/Nation1/Nation1_Wall_v1");
   var stair=Resources.Load<GameObject>("Valoria/Nation1/Nation1_Stair_v1");
   if(wall==null||stair==null)throw new InvalidOperationException("Nation1 authored Hero integration resources missing.");

   // Replace only the old proof stair/retaining presentation, never gameplay.
   foreach(var r in Object.FindObjectsByType<Renderer>(FindObjectsSortMode.None))
   {
    if(r==null||!r.enabled)continue;
    string n=Chain(r.transform);
    if(n.Contains("production bastion stair")||n.Contains("production stair landing")||n.Contains("west retaining")||n.Contains("east retaining"))
      r.enabled=false;
   }

   Fort(root,stair,"hero monumental stair",new Vector3(0f,.14f,3.95f),4.65f,1.55f,0f);
   Fort(root,wall,"hero west retaining wall",new Vector3(-3.95f,.16f,5.48f),2.55f,1.30f,8f);
   Fort(root,wall,"hero east retaining wall",new Vector3(3.95f,.16f,5.48f),2.55f,1.30f,-8f);

   BuildCivicPaving(root);
  }

  static void ScaleCoreVisuals()
  {
   foreach(var t in Object.FindObjectsByType<Transform>(FindObjectsSortMode.None))
   {
    if(t==null)continue;
    string n=t.name.ToLowerInvariant();
    if(n=="valoria · flat citadel · hero bastion")
    {
     t.localScale*=1.16f;t.position+=new Vector3(0f,0f,-.34f);
    }
    else if(n.Contains("flat citadel production · aserradero")||n.Contains("flat citadel production · cuartel")||n.Contains("flat citadel production · granero"))
    {
     t.localScale*=1.07f;
    }
    else if(n.Contains("flat citadel production · main gate"))
    {
     t.localScale*=1.10f;
    }
   }
  }

  static void BuildCivicPaving(Transform root)
  {
   var cobble=ValoriaKit.ExternalPbrSurfaceMaterial("cobble",new Color(.84f,.82f,.77f,1f),new Vector2(3.1f,3.1f),.05f,1.05f)
      ?? ValoriaKit.DetailedSurfaceMaterial(new Color(.55f,.53f,.48f,1f),"stone",new Vector2(2.4f,2.4f),1f);
   Surface(root,"west civic lane",new Vector3(-3.55f,.165f,.95f),new Vector3(3.15f,.045f,1.35f),cobble);
   Surface(root,"east civic lane",new Vector3(3.55f,.165f,.95f),new Vector3(3.15f,.045f,1.35f),cobble);
   Surface(root,"upper west court",new Vector3(-5.85f,.16f,2.65f),new Vector3(2.55f,.04f,2.25f),cobble);
   Surface(root,"upper east court",new Vector3(5.85f,.16f,2.65f),new Vector3(2.55f,.04f,2.25f),cobble);
  }

  static void BuildCivicDensity(Transform root)
  {
   var house=Resources.Load<GameObject>("Valoria/Nation1/Nation1_CivicHouse_v1");
   var houseB=Resources.Load<GameObject>("Valoria/Nation1/Nation1_CivicHouseB_v1");
   var workshop=Resources.Load<GameObject>("Valoria/Nation1/Nation1_Workshop_v1");
   if(house==null||houseB==null||workshop==null)throw new InvalidOperationException("Nation1 authored civic family missing.");

   Building(root,house,"upper west residence",new Vector3(-6.10f,.13f,3.05f),2.35f,2.78f,15f);
   Building(root,houseB,"upper east residence",new Vector3(6.10f,.13f,3.02f),2.28f,2.70f,-15f);
   Building(root,houseB,"plaza west residence",new Vector3(-3.15f,.13f,1.34f),1.92f,2.28f,7f);
   Building(root,house,"plaza east residence",new Vector3(3.18f,.13f,1.30f),1.92f,2.28f,-8f);

   Building(root,workshop,"lower west workshop",new Vector3(-4.70f,.13f,-3.72f),2.10f,2.15f,12f);
   Building(root,workshop,"lower east workshop",new Vector3(4.62f,.13f,-4.02f),2.02f,2.08f,-13f);
   Building(root,houseB,"west inner house",new Vector3(-7.28f,.13f,.12f),1.72f,2.02f,18f);
   Building(root,house,"east inner house",new Vector3(7.25f,.13f,.10f),1.72f,2.02f,-18f);
  }

  static void BuildCentralPlaza(Transform root)
  {
   foreach(var r in Object.FindObjectsByType<Renderer>(FindObjectsSortMode.None))
   {
    if(r==null||!r.enabled)continue;
    string n=Chain(r.transform);
    if(n.Contains("plaza plinth")||n.Contains("central standard"))r.enabled=false;
   }

   var cobble=ValoriaKit.ExternalPbrSurfaceMaterial("cobble",new Color(.86f,.84f,.79f,1f),new Vector2(2.8f,2.8f),.045f,1.06f)
      ?? ValoriaKit.DetailedSurfaceMaterial(new Color(.58f,.56f,.51f,1f),"stone",new Vector2(2.2f,2.2f),1f);
   Disc(root,"civic plaza outer",new Vector3(0f,.174f,.72f),2.05f,.055f,cobble);

   var monument=Resources.Load<GameObject>("Valoria/Nation1/Nation1_CivicMonument_v1") ?? Resources.Load<GameObject>("Valoria/Nation1/Nation1_PlazaMonument_v1");
   if(monument==null)throw new InvalidOperationException("Nation1 plaza monument missing.");
   var go=ValoriaKit.BenchmarkPiece("Valoria · Nation1 · civic monument",monument,new Vector3(0f,.19f,.72f),2.35f,2.35f,Quaternion.identity);
   if(go!=null){go.transform.SetParent(root,true);StripGameplay(go);DetailProps++;}
  }

  static void Disc(Transform root,string name,Vector3 p,float radius,float height,Material mat)
  {
   var go=GameObject.CreatePrimitive(PrimitiveType.Cylinder);go.name="Valoria · Nation1 · "+name;go.transform.SetParent(root,true);
   go.transform.position=p;go.transform.localScale=new Vector3(radius*2f,height*.5f,radius*2f);
   go.GetComponent<Renderer>().sharedMaterial=mat;StripGameplay(go);
  }

  static void BuildGranaryFields(Transform root)
  {
   var crop=ValoriaKit.DetailedSurfaceMaterial(new Color(.72f,.56f,.18f,1f),"earth",new Vector2(2.0f,2.0f),.84f);
   var soil=ValoriaKit.DetailedSurfaceMaterial(new Color(.34f,.25f,.14f,1f),"earth",new Vector2(2.6f,2.6f),.94f);
   Field(root,"granary west field",new Vector3(-1.45f,.16f,-3.18f),1.22f,1.02f,crop,soil);
   Field(root,"granary east field",new Vector3(.95f,.16f,-3.42f),1.08f,.92f,crop,soil);
  }

  static void Field(Transform root,string name,Vector3 center,float halfX,float halfZ,Material crop,Material soil)
  {
   Surface(root,name+" soil",center,new Vector3(halfX*2f,.035f,halfZ*2f),soil);
   for(int i=0;i<6;i++)
   {
    float z=center.z-halfZ*.72f+i*(halfZ*1.44f/5f);
    Surface(root,name+" crop "+i,new Vector3(center.x,center.y+.07f,z),new Vector3(halfX*1.80f,.12f,.085f),crop);
   }
  }

  static void BuildFunctionalDressing(Transform root)
  {
   var barrel=Resources.Load<GameObject>("Valoria/UrbanProps/Barrel");
   var crate=Resources.Load<GameObject>("Valoria/UrbanProps/Crate");
   var sack=Resources.Load<GameObject>("Valoria/UrbanProps/Sack");
   if(barrel!=null&&crate!=null&&sack!=null)
   {
    Prop(root,barrel,"sawmill barrel 1",new Vector3(-7.55f,.16f,-2.75f),.48f,18f);
    Prop(root,barrel,"sawmill barrel 2",new Vector3(-7.12f,.16f,-2.62f),.44f,-12f);
    Prop(root,crate,"sawmill crate 1",new Vector3(-6.62f,.16f,-3.02f),.52f,8f);
    Prop(root,crate,"sawmill crate 2",new Vector3(-6.15f,.16f,-3.15f),.44f,-18f);
    Prop(root,crate,"barracks crate 1",new Vector3(6.65f,.16f,-3.30f),.48f,14f);
    Prop(root,barrel,"barracks barrel 1",new Vector3(7.15f,.16f,-3.18f),.43f,-8f);
    Prop(root,sack,"granary sack 1",new Vector3(-1.65f,.16f,-5.08f),.55f,10f);
    Prop(root,sack,"granary sack 2",new Vector3(-1.28f,.16f,-5.00f),.50f,-12f);
    Prop(root,barrel,"granary barrel",new Vector3(-3.95f,.16f,-5.05f),.44f,4f);
   }

   var art=ValoriaExternalAssetLibrary.Load();
   if(art!=null)
   {
    if(art.Firewood!=null)
    {
     foreach(var spec in new[]{
       new Vector4(-7.35f,.14f,-1.30f,12f),new Vector4(-6.72f,.14f,-1.15f,-18f),
       new Vector4(-5.92f,.14f,-2.55f,26f)})
     {
      var go=ValoriaKit.BenchmarkPiece("Valoria · Nation1 · sawmill firewood",art.Firewood,new Vector3(spec.x,spec.y,spec.z),.72f,.80f,Quaternion.Euler(0,spec.w,0));
      if(go!=null){go.transform.SetParent(root,true);StripGameplay(go);DetailProps++;}
     }
    }

    if(art.SlavicStoneFence!=null)
    {
     foreach(var spec in new[]{
       new Vector4(-2.50f,.14f,-5.55f,0f),new Vector4(.25f,.14f,-5.55f,0f),
       new Vector4(5.55f,.14f,-3.90f,90f),new Vector4(7.85f,.14f,-3.90f,90f)})
     {
      var go=ValoriaKit.BenchmarkPiece("Valoria · Nation1 · yard fence",art.SlavicStoneFence,new Vector3(spec.x,spec.y,spec.z),1.25f,.75f,Quaternion.Euler(0,spec.w,0));
      if(go!=null){go.transform.SetParent(root,true);StripGameplay(go);DetailProps++;}
     }
    }
   }
  }

  static void BuildGreenery(Transform root)
  {
   var treeA=Resources.Load<GameObject>("WorldInventory/Tree01A");
   var treeB=Resources.Load<GameObject>("WorldInventory/Tree01B");
   var art=ValoriaExternalAssetLibrary.Load();
   var fallback=art!=null?art.SlavicTree:null;

   var specs=new[]{
     new Vector4(-5.25f,4.45f,12f,1.05f),new Vector4(5.20f,4.42f,-18f,1.02f),
     new Vector4(-6.70f,5.65f,28f,.95f),new Vector4(6.62f,5.58f,-22f,.98f),
     new Vector4(-2.70f,7.25f,8f,.82f),new Vector4(2.72f,7.22f,-10f,.84f),
     new Vector4(-2.30f,2.55f,18f,.70f),new Vector4(2.35f,2.52f,-16f,.72f),
     new Vector4(-8.25f,1.85f,36f,.76f),new Vector4(8.22f,1.82f,-32f,.76f)
   };
   for(int i=0;i<specs.Length;i++)
   {
    var spec=specs[i];
    var src=(i%2==0?treeA:treeB)??treeA??treeB??fallback;
    if(src==null)continue;
    var go=ValoriaKit.BenchmarkPiece("Valoria · Nation1 · living tree",src,new Vector3(spec.x,.14f,spec.y),spec.w,spec.w*2.9f,Quaternion.Euler(0,spec.z,0));
    if(go!=null){go.transform.SetParent(root,true);StripGameplay(go);DetailProps++;}
   }
  }

  static void BuildBackdrop(Transform root)
  {
   BuildIrregularLake(root);

   var treeA=Resources.Load<GameObject>("WorldInventory/Tree01A");
   var treeB=Resources.Load<GameObject>("WorldInventory/Tree01B");
   if(treeA!=null||treeB!=null)
   {
    var specs=new[]{
     new Vector4(-14.4f,11.4f,8f,1.28f),new Vector4(-12.8f,12.6f,30f,1.36f),
     new Vector4(-11.2f,13.4f,-12f,1.22f),new Vector4(-9.5f,12.6f,18f,1.30f),
     new Vector4(-7.7f,13.6f,42f,1.16f),new Vector4(-5.8f,12.4f,-22f,1.14f),
     new Vector4(7.0f,12.3f,14f,1.12f),new Vector4(8.8f,13.1f,-16f,1.18f),
     new Vector4(10.6f,12.0f,22f,1.26f),new Vector4(12.2f,12.8f,-24f,1.32f),
     new Vector4(13.8f,11.4f,32f,1.36f),new Vector4(15.0f,10.6f,-14f,1.28f)
    };
    for(int i=0;i<specs.Length;i++)
    {
     var spec=specs[i];var src=(i%2==0?treeA:treeB)??treeA??treeB;
     var go=ValoriaKit.BenchmarkPiece("Valoria · Nation1 · distant tree",src,new Vector3(spec.x,.02f,spec.y),spec.w,spec.w*3.0f,Quaternion.Euler(0,spec.z,0));
     if(go!=null)
     {
      go.transform.SetParent(root,true);
      TintTree(go,new Color(.46f,.62f,.46f,1f));
      StripGameplay(go);DetailProps++;
     }
    }
   }
  }

  static void BuildIrregularLake(Transform root)
  {
   var shader=Shader.Find("Universal Render Pipeline/Lit")??Shader.Find("Standard");
   if(shader==null)return;
   var water=new Material(shader){name="Valoria Nation1 lake"};
   var col=new Color(.08f,.19f,.255f,1f);
   if(water.HasProperty("_BaseColor"))water.SetColor("_BaseColor",col);
   if(water.HasProperty("_Color"))water.SetColor("_Color",col);
   if(water.HasProperty("_Smoothness"))water.SetFloat("_Smoothness",.70f);
   if(water.HasProperty("_Metallic"))water.SetFloat("_Metallic",.02f);

   var go=new GameObject("Valoria · Nation1 · irregular outer lake");go.transform.SetParent(root,true);
   var mf=go.AddComponent<MeshFilter>();var mr=go.AddComponent<MeshRenderer>();
   var verts=new[]{
    new Vector3(4.8f,.018f,10.2f),new Vector3(8.6f,.018f,9.8f),
    new Vector3(12.7f,.018f,10.7f),new Vector3(16.6f,.018f,12.9f),
    new Vector3(17.6f,.018f,16.5f),new Vector3(15.5f,.018f,20.2f),
    new Vector3(10.7f,.018f,22.0f),new Vector3(6.4f,.018f,20.6f),
    new Vector3(3.4f,.018f,17.4f),new Vector3(3.8f,.018f,13.0f)
   };
   var tris=new[]{
    0,1,2,0,2,3,0,3,4,0,4,5,0,5,6,0,6,7,0,7,8,0,8,9
   };
   var mesh=new Mesh{name="Nation1 irregular lake"};mesh.vertices=verts;mesh.triangles=tris;
   var uv=new Vector2[verts.Length];for(int i=0;i<verts.Length;i++)uv[i]=new Vector2(verts[i].x*.08f,verts[i].z*.08f);
   mesh.uv=uv;mesh.RecalculateNormals();mesh.RecalculateBounds();mf.sharedMesh=mesh;mr.sharedMaterial=water;
   StripGameplay(go);DetailProps++;
  }

  static void TintTree(GameObject go,Color tint)
  {
   foreach(var r in go.GetComponentsInChildren<Renderer>(true))
   {
    var src=r.sharedMaterials;var dst=new Material[src.Length];
    for(int i=0;i<src.Length;i++)
    {
     var m=src[i];if(m==null){dst[i]=null;continue;}
     var clone=new Material(m){name="Nation1 distant forest · "+m.name};
     foreach(var prop in new[]{"_BaseColor","_Color"})
     {
      if(!clone.HasProperty(prop))continue;
      try
      {
       var col=clone.GetColor(prop);
       clone.SetColor(prop,new Color(col.r*tint.r,col.g*tint.g,col.b*tint.b,col.a));
      }catch{}
     }
     dst[i]=clone;
    }
    r.sharedMaterials=dst;
   }
  }

  static void BackdropPiece(Transform root,GameObject src,string role,Vector3 p,float footprint,float height,float yaw)
  {
   if(src==null)return;
   var go=ValoriaKit.BenchmarkPiece("Valoria · Nation1 · "+role,src,p,footprint,height,Quaternion.Euler(0,yaw,0));
   if(go==null)return;go.transform.SetParent(root,true);StripGameplay(go);DetailProps++;
  }

  static void BuildLighting(Transform root)
  {
   RenderSettings.ambientMode=UnityEngine.Rendering.AmbientMode.Trilight;
   RenderSettings.ambientSkyColor=new Color(.78f,.82f,.86f,1f);
   RenderSettings.ambientEquatorColor=new Color(.60f,.55f,.47f,1f);
   RenderSettings.ambientGroundColor=new Color(.27f,.23f,.19f,1f);
   RenderSettings.ambientIntensity=1.10f;
   RenderSettings.fog=true;
   RenderSettings.fogMode=FogMode.Linear;
   RenderSettings.fogColor=new Color(.43f,.49f,.50f,1f);
   RenderSettings.fogStartDistance=38f;
   RenderSettings.fogEndDistance=68f;

   foreach(var l in Object.FindObjectsByType<Light>(FindObjectsSortMode.None))
   {
    if(l.type!=LightType.Directional)continue;
    l.color=new Color(1f,.90f,.76f,1f);l.intensity=Mathf.Max(l.intensity,1.18f);
    l.shadows=LightShadows.Soft;l.shadowStrength=.72f;
    l.transform.rotation=Quaternion.Euler(48f,-34f,0f);
   }

   var fillGo=new GameObject("Valoria · Nation1 · cool sky fill");fillGo.transform.SetParent(root,true);fillGo.transform.rotation=Quaternion.Euler(35f,150f,0f);
   var fill=fillGo.AddComponent<Light>();fill.type=LightType.Directional;fill.color=new Color(.62f,.72f,.92f,1f);fill.intensity=.12f;fill.shadows=LightShadows.None;

   WarmExistingStone();
   HarmonizeBanners();
   Warm(root,"hero stair west",new Vector3(-1.25f,1.25f,4.40f),.72f,4.2f);
   Warm(root,"hero stair east",new Vector3(1.25f,1.25f,4.40f),.72f,4.2f);
   Warm(root,"hero terrace west",new Vector3(-3.55f,1.35f,5.45f),.48f,3.8f);
   Warm(root,"hero terrace east",new Vector3(3.55f,1.35f,5.45f),.48f,3.8f);
   Warm(root,"gate warmth",new Vector3(0f,1.55f,-5.95f),.68f,4.3f);
   Warm(root,"sawmill warmth",new Vector3(-6.0f,1.55f,-1.7f),.42f,3.5f);
   Warm(root,"barracks warmth",new Vector3(5.9f,1.55f,-1.9f),.38f,3.4f);
   Warm(root,"granary warmth",new Vector3(-2.7f,1.35f,-4.2f),.34f,3.2f);
  }

  static void HarmonizeBanners()
  {
   var shader=Shader.Find("Universal Render Pipeline/Unlit")??Shader.Find("Universal Render Pipeline/Lit")??Shader.Find("Standard");
   if(shader==null)return;
   var blue=new Material(shader){name="Valoria Nation1 heraldic blue"};
   if(blue.HasProperty("_BaseColor"))blue.SetColor("_BaseColor",new Color(.04f,.13f,.36f,1f));
   if(blue.HasProperty("_Color"))blue.SetColor("_Color",new Color(.04f,.13f,.36f,1f));
   foreach(var r in Object.FindObjectsByType<Renderer>(FindObjectsSortMode.None))
   {
    if(r==null||!r.enabled)continue;
    string n=Chain(r.transform);
    if((n.Contains("banner")||n.Contains("standard"))&&!n.Contains("hud"))
      r.sharedMaterial=blue;
   }
  }

  static void WarmExistingStone()
  {
   foreach(var r in Object.FindObjectsByType<Renderer>(FindObjectsSortMode.None))
   {
    string n=r.gameObject.name.ToLowerInvariant();
    bool target=n.Contains("breakthrough")||n.Contains("flat citadel production")||n.Contains("flat citadel · main gate");
    if(!target)continue;
    var src=r.sharedMaterials;var dst=new Material[src.Length];
    for(int i=0;i<src.Length;i++)
    {
     var m=src[i];if(m==null){dst[i]=null;continue;}
     var clone=new Material(m){name="Nation1 warm · "+m.name};
     foreach(var prop in new[]{"_BaseColor","_Color"})
     {
      if(!clone.HasProperty(prop))continue;
      try{var col=clone.GetColor(prop);clone.SetColor(prop,new Color(Mathf.Min(1f,col.r*1.08f),Mathf.Min(1f,col.g*1.055f),Mathf.Min(1f,col.b*.98f),col.a));}catch{}
     }
     dst[i]=clone;
    }
    r.sharedMaterials=dst;
   }
  }

  static GameObject Piece(Transform root,GameObject src,string role,Vector3 ground,float footprint,float maxHeight,float yaw)
  {
   var go=ValoriaKit.BenchmarkPiece("Valoria · Nation1 · "+role,src,ground,footprint,maxHeight,Quaternion.Euler(0,yaw,0));
   if(go==null)throw new InvalidOperationException("Failed Nation1 piece "+role);
   go.transform.SetParent(root,true);StripGameplay(go);TerraceModules++;return go;
  }

  static GameObject RichPiece(Transform root,GameObject src,string role,Vector3 ground,float footprint,float maxHeight,float yaw)
  {
   return Piece(root,src,role,ground,footprint,maxHeight,yaw);
  }

  static GameObject StonePiece(Transform root,GameObject src,string role,Vector3 ground,float footprint,float maxHeight,float yaw)
  {
   var go=Piece(root,src,role,ground,footprint,maxHeight,yaw);
   var stone=ValoriaKit.ExternalPbrSurfaceMaterial("stone",new Color(.78f,.75f,.69f,1f),new Vector2(2.2f,2.2f),.04f,1.08f)
      ?? ValoriaKit.DetailedSurfaceMaterial(new Color(.58f,.56f,.50f,1f),"stone",new Vector2(2.1f,2.1f),1f);
   foreach(var r in go.GetComponentsInChildren<Renderer>(true))r.sharedMaterial=stone;
   return go;
  }

  static void Surface(Transform root,string role,Vector3 p,Vector3 scale,Material material)
  {
   var go=GameObject.CreatePrimitive(PrimitiveType.Cube);go.name="Valoria · Nation1 · "+role;go.transform.SetParent(root,true);go.transform.position=p;go.transform.localScale=scale;
   var r=go.GetComponent<Renderer>();if(r!=null)r.sharedMaterial=material;StripGameplay(go);
  }

  static GameObject Building(Transform root,GameObject src,string role,Vector3 ground,float footprint,float maxHeight,float yaw)
  {
   var go=ValoriaKit.BenchmarkPieceModulated("Valoria · Nation1 · "+role,src,ground,footprint,maxHeight,Quaternion.Euler(0,yaw,0),new Color(.90f,.90f,.88f,1f));
   if(go==null)throw new InvalidOperationException("Failed Nation1 building "+role);
   go.transform.SetParent(root,true);StripGameplay(go);MidTierBuildings++;return go;
  }

  static void Prop(Transform root,GameObject src,string role,Vector3 ground,float footprint,float yaw)
  {
   var go=ValoriaKit.BenchmarkPiece("Valoria · Nation1 · "+role,src,ground,footprint,footprint*1.25f,Quaternion.Euler(0,yaw,0));
   if(go==null)return;go.transform.SetParent(root,true);StripGameplay(go);DetailProps++;
  }

  static void Warm(Transform root,string role,Vector3 p,float intensity,float range)
  {
   var go=new GameObject("Valoria · Nation1 · "+role);go.transform.SetParent(root,true);go.transform.position=p;
   var l=go.AddComponent<Light>();l.type=LightType.Point;l.color=new Color(1f,.56f,.25f);l.intensity=intensity;l.range=range;l.shadows=LightShadows.None;WarmLights++;
  }

  static void StripGameplay(GameObject go)
  {
   foreach(var c in go.GetComponentsInChildren<Collider>(true))Object.DestroyImmediate(c);
   foreach(var h in go.GetComponentsInChildren<WorldHotspot>(true))Object.DestroyImmediate(h);
   foreach(var lod in go.GetComponentsInChildren<LODGroup>(true))lod.ForceLOD(0);
  }
 }
}
