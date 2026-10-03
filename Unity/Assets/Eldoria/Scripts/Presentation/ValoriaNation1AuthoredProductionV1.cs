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
   BuildHeroTerracedCore(root);
   BuildCivicDensity(root);
   BuildFunctionalDressing(root);
   BuildLighting(root);
   StripGameplay(root.gameObject);
  }

  static void BuildHeroTerracedCore(Transform root)
  {
   var residential=Resources.Load<GameObject>("Valoria/Rescued/ResidentialTerraceRock");
   var seam=Resources.Load<GameObject>("Valoria/Rescued/RockTerrainSeamFiller");
   var wall=Resources.Load<GameObject>("Valoria/Stone_Wall");
   if(residential==null||seam==null||wall==null)throw new InvalidOperationException("Nation1 rich core library incomplete.");

   // Preserve VQB's proven central stair/plaza. Build only rich, low-profile side integration around it.
   RichPiece(root,residential,"hero west shoulder",new Vector3(-4.05f,.13f,6.20f),2.85f,1.02f,24f);
   RichPiece(root,residential,"hero east shoulder",new Vector3(4.05f,.13f,6.22f),2.85f,1.02f,204f);
   RichPiece(root,seam,"hero west seam",new Vector3(-3.25f,.13f,5.05f),2.20f,.70f,24f);
   RichPiece(root,seam,"hero east seam",new Vector3(3.25f,.13f,5.08f),2.20f,.70f,204f);

   StonePiece(root,wall,"hero west retaining wall",new Vector3(-3.95f,.16f,5.48f),2.50f,1.18f,8f);
   StonePiece(root,wall,"hero east retaining wall",new Vector3(3.95f,.16f,5.48f),2.50f,1.18f,-8f);

   BuildCivicPaving(root);
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
   var p1=Resources.Load<GameObject>("Valoria/MidTierArchitectureKit_v1/Piece01");
   var p2=Resources.Load<GameObject>("Valoria/MidTierArchitectureKit_v1/Piece02");
   var p3=Resources.Load<GameObject>("Valoria/MidTierArchitectureKit_v1/Piece03");
   var p4=Resources.Load<GameObject>("Valoria/MidTierArchitectureKit_v1/Piece04");
   if(p1==null||p2==null||p3==null||p4==null)throw new InvalidOperationException("MidTier Architecture Kit v1 incomplete.");

   Building(root,p1,"upper west residence",new Vector3(-6.30f,.13f,3.25f),2.55f,2.85f,16f);
   Building(root,p2,"upper east residence",new Vector3(6.28f,.13f,3.18f),2.50f,2.80f,-15f);
   Building(root,p3,"plaza west guildhouse",new Vector3(-3.25f,.13f,1.45f),2.25f,2.45f,8f);
   Building(root,p4,"plaza east guildhouse",new Vector3(3.28f,.13f,1.38f),2.25f,2.45f,-9f);
  }

  static void BuildFunctionalDressing(Transform root)
  {
   var barrel=Resources.Load<GameObject>("Valoria/UrbanProps/Barrel");
   var crate=Resources.Load<GameObject>("Valoria/UrbanProps/Crate");
   var sack=Resources.Load<GameObject>("Valoria/UrbanProps/Sack");
   if(barrel==null||crate==null||sack==null)return;

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

  static void BuildLighting(Transform root)
  {
   Warm(root,"hero stair west",new Vector3(-1.25f,1.25f,4.40f),.22f,3.6f);
   Warm(root,"hero stair east",new Vector3(1.25f,1.25f,4.40f),.22f,3.6f);
   Warm(root,"hero terrace west",new Vector3(-3.55f,1.35f,5.45f),.16f,3.2f);
   Warm(root,"hero terrace east",new Vector3(3.55f,1.35f,5.45f),.16f,3.2f);
   Warm(root,"gate warmth",new Vector3(0f,1.55f,-5.95f),.20f,3.7f);
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
