using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Eldoria.ReferenceClean.Editor {
 public static class ValoriaContentBuild {
  public const string Root="Assets/Eldoria/ArtTests/ValoriaReferenceClean/";
  public const string Bootstrap=Root+"Valoria_Content_Bootstrap.unity";
  public const string Output="Builds/ValoriaContent";
  public static string RuntimeContract(){
   // A code/package change requires a new bootstrap APK, never arbitrary downloaded code.
   var paths=Directory.GetFiles("Assets/Eldoria","*.cs",SearchOption.AllDirectories).Where(p=>!p.Replace('\\','/').Contains("/Editor/")&&!p.Replace('\\','/').Contains("/Tests/")).OrderBy(p=>p,StringComparer.Ordinal);
   var text=new StringBuilder(UnityEngine.Application.unityVersion+"\n");
   foreach(var p in paths)text.Append(p.Replace('\\','/')).Append('\n').Append(File.ReadAllText(p).Replace("\r\n","\n")).Append('\n');
   text.Append(File.ReadAllText("Packages/manifest.json").Replace("\r\n","\n"));
   using(var h=SHA256.Create())return BitConverter.ToString(h.ComputeHash(Encoding.UTF8.GetBytes(text.ToString()))).Replace("-","").ToLowerInvariant();
  }
  public static void PrepareBootstrap(){
   var scene=EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
   var loader=new GameObject("Valoria content updates / existing save preserved").AddComponent<ValoriaContentUpdate>();loader.RuntimeContract=RuntimeContract();
   if(!EditorSceneManager.SaveScene(scene,Bootstrap))throw new Exception("Bootstrap save failed");AssetDatabase.SaveAssets();
  }
  public static ValoriaContentManifest BuildBundle(BuildTarget target,int revision){
   string platform=target==BuildTarget.Android?"android":"windows",folder=Output+"/"+platform;Directory.CreateDirectory(folder);
   var manifest=BuildPipeline.BuildAssetBundles(folder,new[]{new AssetBundleBuild{assetBundleName="valoria.bundle",assetNames=new[]{ValoriaContentPolicy.Scene}}},BuildAssetBundleOptions.ChunkBasedCompression|BuildAssetBundleOptions.StrictMode,target);
   if(manifest==null)throw new Exception("Content bundle build failed");
   string file=folder+"/valoria.bundle";if(!BuildPipeline.GetCRCForAssetBundle(file,out var crc))throw new Exception("Content CRC unavailable");
   var hash=ValoriaContentPolicy.Hash(file);var name="valoria-"+platform+"-"+hash+".bundle";var final=folder+"/"+name;File.Copy(file,final,true);
   var result=new ValoriaContentManifest{revision=revision,minimumPlayerCode=6,platform=platform,unityVersion=UnityEngine.Application.unityVersion,runtimeContract=RuntimeContract(),url=ValoriaContentPolicy.DownloadPrefix+revision+"/"+name,sha256=hash,bytes=new FileInfo(final).Length,crc=crc,scene=ValoriaContentPolicy.Scene};
   if(!ValoriaContentPolicy.Accept(result,platform,result.runtimeContract,6,out var why))throw new Exception(why);
   File.WriteAllText(folder+"/content-channel.json",JsonUtility.ToJson(result,true));return result;
  }
  public static void Validate(){
   try{
    var m=new ValoriaContentManifest{revision=93,minimumPlayerCode=6,platform="windows",unityVersion=UnityEngine.Application.unityVersion,runtimeContract="test",url=ValoriaContentPolicy.DownloadPrefix+"93/valoria-windows-"+new string('a',64)+".bundle",sha256=new string('a',64),bytes=10,crc=1,scene=ValoriaContentPolicy.Scene};
    void Check(bool value,string reason){if(!value)throw new Exception("CONTENT_POLICY "+reason);}
    bool Accept()=>ValoriaContentPolicy.Accept(m,"windows","test",6,out _);
    Check(Accept(),"valid manifest");m.runtimeContract="different";Check(!Accept(),"code mismatch");m.runtimeContract="test";
    m.platform="android";Check(!Accept(),"platform");m.platform="windows";
    m.minimumPlayerCode=7;Check(!Accept(),"new APK required");m.minimumPlayerCode=6;
    m.url="https://example.com/file";Check(!Accept(),"foreign origin");m.url=ValoriaContentPolicy.DownloadPrefix+"93/valoria-windows-"+m.sha256+".bundle";
    m.bytes=ValoriaContentPolicy.MaximumBytes+1;Check(!Accept(),"size cap");m.bytes=10;
    m.scene="Assets/Other.unity";Check(!Accept(),"foreign scene");m.scene=ValoriaContentPolicy.Scene;
    string sceneHash=ValoriaContentPolicy.Hash(ValoriaContentPolicy.Scene);
    var bundle=BuildBundle(BuildTarget.StandaloneWindows64,93);
    Check(ValoriaContentPolicy.Hash(ValoriaContentPolicy.Scene)==sceneHash,"validated scene unchanged");
    PrepareBootstrap();
    File.WriteAllText("../docs/evidence/valoria-reference-clean/content93-policy.json",JsonUtility.ToJson(bundle,true));
    SessionState.SetString("Valoria.ContentProbe.Bundle",Path.GetFullPath(Output+"/windows/valoria-windows-"+bundle.sha256+".bundle"));
    SessionState.SetString("Valoria.ContentProbe.Manifest",JsonUtility.ToJson(bundle));
    ValoriaContentProbe.Enter();
   }catch(Exception e){Debug.LogException(e);EditorApplication.Exit(1);}
  }
 }
 [InitializeOnLoad] public static class ValoriaContentProbe {
  const string Flag="Valoria.ContentProbe.Active";
  static ValoriaContentProbe(){EditorApplication.playModeStateChanged+=Changed;}
  public static void Enter(){
   // No network and no production save in this isolated, exact bundle load test.
   Environment.SetEnvironmentVariable("VALORIA_GAMEPLAY_PROBE","1");
   Environment.SetEnvironmentVariable("VALORIA_CONTENT_PROBE","1");
   EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
   var camera=new GameObject("Content probe camera").AddComponent<Camera>();camera.backgroundColor=Color.black;
   SessionState.SetBool(Flag,true);EditorApplication.EnterPlaymode();
  }
  static void Changed(PlayModeStateChange state){
   if(state!=PlayModeStateChange.EnteredPlayMode||!SessionState.GetBool(Flag,false))return;
   SessionState.SetBool(Flag,false);EditorApplication.delayCall+=()=>{var r=new GameObject("Content probe runner").AddComponent<ValoriaContentUpdate>();r.SuppressStartupForEditorProbe=true;r.StartCoroutine(Run(r));};
  }
  static System.Collections.IEnumerator Run(ValoriaContentUpdate runner){
   UnityEngine.Object.DontDestroyOnLoad(runner.gameObject);
   var m=JsonUtility.FromJson<ValoriaContentManifest>(SessionState.GetString("Valoria.ContentProbe.Manifest",""));
   var file=SessionState.GetString("Valoria.ContentProbe.Bundle","");
   string owner=Path.Combine(UnityEngine.Application.persistentDataPath,"eldoria-unity-slice-owner-i-ii-v1.json"),legacy=Path.Combine(UnityEngine.Application.persistentDataPath,"eldoria-unity-slice-v1.json");
   string Fingerprint(string p)=>File.Exists(p)?ValoriaContentPolicy.Hash(p):"ABSENT";
   string a=Fingerprint(owner),b=Fingerprint(legacy);
   var request=AssetBundle.LoadFromFileAsync(file,m.crc);yield return request;
   var bundle=request.assetBundle;
   if(bundle==null){Debug.LogError("CONTENT_BUNDLE_LOAD_FAILED");EditorApplication.Exit(1);yield break;}
   var paths=bundle.GetAllScenePaths();
   if(paths.Length!=1||!string.Equals(paths[0],ValoriaContentPolicy.Scene,StringComparison.OrdinalIgnoreCase)){Debug.LogError("CONTENT_SCENE_MISMATCH");EditorApplication.Exit(1);yield break;}
   yield return SceneManager.LoadSceneAsync(paths[0],LoadSceneMode.Single);yield return null;yield return new WaitForSecondsRealtime(1);
   System.Collections.IEnumerator gameplay=null;
   try{
    var host=UnityEngine.Object.FindFirstObjectByType<ValoriaReferenceCleanGameplay>();
    if(host==null||!host.Ready||host.Gateway.Snapshot().BastionLevel!=1||Camera.allCamerasCount!=1)throw new Exception("Loaded bundle runtime/camera failed");
    if(a!=Fingerprint(owner)||b!=Fingerprint(legacy))throw new Exception("Canonical save changed");
    var c=host.PlayerCamera;var rt=new RenderTexture(1536,1024,24){antiAliasing=4};var old=c.targetTexture;var active=RenderTexture.active;var tex=new Texture2D(1536,1024,TextureFormat.RGB24,false);
    try{c.targetTexture=rt;c.Render();RenderTexture.active=rt;tex.ReadPixels(new Rect(0,0,1536,1024),0,0);tex.Apply();File.WriteAllBytes("../docs/evidence/valoria-reference-clean/content93-loaded.png",tex.EncodeToPNG());}finally{c.targetTexture=old;RenderTexture.active=active;rt.Release();UnityEngine.Object.Destroy(rt);UnityEngine.Object.Destroy(tex);}
    File.WriteAllText("../docs/evidence/valoria-reference-clean/content93-result.json","{\"status\":\"NATIVE_WINDOWS_SCENE_BUNDLE_PLAY_PASS\",\"policy_negative_cases\":6,\"one_camera\":true,\"canonical_saves_untouched\":true,\"android_tested\":false,\"remote_published\":false}");
    Debug.Log("VALORIA_REFERENCE_CLEAN_CONTENT_COMPLETE");
    // Reuse the actual I-II/chapter-II UI probe on THIS loaded bundle, not a source-scene substitute.
    gameplay=(System.Collections.IEnumerator)typeof(ValoriaReferenceCleanGameplayBuild).GetMethod("Probe",System.Reflection.BindingFlags.Static|System.Reflection.BindingFlags.NonPublic).Invoke(null,new object[]{host});
   }catch(Exception e){Debug.LogException(e);EditorApplication.Exit(1);}
   if(gameplay!=null)yield return gameplay;
  }
 }
}
