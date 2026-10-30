#if UNITY_EDITOR
using System;
using System.IO;
using System.Linq;
using System.Collections;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Eldoria.Domain;
using Eldoria.World4XCanon;
using Object=UnityEngine.Object;
namespace Eldoria.ReferenceClean.Editor {
 [InitializeOnLoad] public static class ValoriaConsolidationValidation {
  const string Flag="Valoria.ConsolidationFinalCapture";
  static string Ev=>Path.GetFullPath("../docs/evidence/valoria-reference-clean/consolidation-20261027/"+Environment.GetEnvironmentVariable("GITHUB_RUN_ID"));
  static ValoriaConsolidationValidation(){EditorApplication.playModeStateChanged+=Entered;}
  static void Need(bool b,string s){if(!b)throw new InvalidOperationException(s);}
  static void Fail(Exception e){Debug.LogException(e);Directory.CreateDirectory(Ev);File.WriteAllText(Ev+"/failure.txt",e.ToString());EditorApplication.Exit(1);}
  public static void Capture(){try{Directory.CreateDirectory(Ev);Environment.SetEnvironmentVariable("VALORIA_GAMEPLAY_PROBE","1");EditorSceneManager.OpenScene("Assets/Eldoria/ArtTests/ValoriaReferenceClean/Integration/Valoria_City_World_UI.unity",OpenSceneMode.Single);SessionState.SetBool(Flag,true);EditorApplication.EnterPlaymode();}catch(Exception e){Fail(e);}}
  static void Entered(PlayModeStateChange state){if(state!=PlayModeStateChange.EnteredPlayMode||!SessionState.GetBool(Flag,false))return;SessionState.SetBool(Flag,false);EditorApplication.delayCall+=()=>{var host=Object.FindFirstObjectByType<ValoriaIntegratedWorld>();if(!host){Fail(new Exception("Actual host missing"));return;}host.StartCoroutine(Guard(host));};}
  static IEnumerator Guard(ValoriaIntegratedWorld h){var p=Probe(h);while(true){object value=null;bool more=false;try{more=p.MoveNext();if(more)value=p.Current;}catch(Exception e){Fail(e);yield break;}if(!more)yield break;yield return value;}}
  static IEnumerator Probe(ValoriaIntegratedWorld h){
   yield return null;yield return new WaitForSecondsRealtime(1);Need(h.Ready,"Actual host not ready");
   if(!h.TrySetWorld(true)){var offer=h.Read("sawmill",false).Building.Action;Need(offer!=null&&offer.Kind=="Build","Actual offer missing");h.Submit(offer);((ValoriaReferenceCleanGameplay.TestClock)h.City.Clock).Step(SliceRules.SawmillBuildSeconds+1);h.City.Gateway.Advance();yield return null;Need(h.TrySetWorld(true),"Actual World progression failed");}yield return null;
   var bridge=h.WorldRoot.GetComponentsInChildren<Transform>(true).SingleOrDefault(t=>t.name=="Valoria approved city-derived stone bridge");Need(bridge,"Reviewed bridge not installed by production runtime");Need(bridge.GetComponentsInChildren<Collider>(true).Length==0,"Visual bridge must not replace gameplay collision");
   var cam=h.WorldCamera;var nav=h.WorldNavigation;var canon=Object.FindFirstObjectByType<RegionOneCanonicalLandscape>();
   foreach(var size in new[]{(1280,720,"horizontal"),(900,1600,"vertical")})foreach(var view in new[]{(2,"cercana"),(1,"regional"),(0,"panoramica")}){
    cam.aspect=(float)size.Item1/size.Item2;nav.SetZoomLevel(view.Item1,true);nav.PanTo(h.WorldHome);nav.FitToAuthoredBounds();canon.RefreshForCapture();
    var city=h.WorldRoot.GetComponentsInChildren<MeshRenderer>(true).Where(r=>r.name.StartsWith("Reference city Bastion")&&r.enabled&&r.gameObject.activeInHierarchy).ToArray();Need(city.Length==1,"Actual progression must select exactly one strategic Bastion in every camera");Need(city[0].sharedMaterial.mainTexture.name=="BastionI","Disposable actual Bastion I must use approved original bake");
    ValoriaConsolidationTrial.Capture(cam,nav.target,"final",view.Item2+"-"+size.Item3,size.Item1,size.Item2);
   }
   Need(h.TrySetWorld(false),"Return to actual city failed");var bank=Object.FindObjectsByType<MeshFilter>(FindObjectsInactive.Include,FindObjectsSortMode.None).First(f=>f.name=="River_bank_rocks"&&!f.transform.IsChildOf(h.WorldRoot.transform));Need(AssetDatabase.GetAssetPath(bank.sharedMesh).Contains("Consolidation20261027/Riverbank.asset"),"City bank binding not integrated");
   foreach(var size in new[]{(1448,1086,"horizontal"),(900,1600,"vertical")})ValoriaConsolidationTrial.Capture(h.City.PlayerCamera,Vector3.zero,"final-city","city-"+size.Item3,size.Item1,size.Item2);
   File.WriteAllText(Ev+"/final-capture-receipt.json","{\"actual_production_runtime\":true,\"manual_visual_overrides\":false,\"gameplay_store\":\"disposable_probe\",\"physical_mobile_tested\":false,\"world_views\":6,\"city_views\":2}");Debug.Log("VALORIA_REFERENCE_CLEAN_CONSOLIDATION_VALIDATION_COMPLETE actual_runtime=1 manual_visual_overrides=0");EditorApplication.Exit(0);
  }
 }
}
#endif
