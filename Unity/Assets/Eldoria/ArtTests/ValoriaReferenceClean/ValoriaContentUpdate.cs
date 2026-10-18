using System;
using System.Collections;
using System.IO;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;

namespace Eldoria.ReferenceClean {
 // Content only: assemblies, progression rules and saves are never downloaded.
 [Serializable] public sealed class ValoriaContentManifest {
  public int schema=1, revision, minimumPlayerCode;
  public string platform, unityVersion, runtimeContract, url, sha256, scene, sceneSourceSha256;
  public long bytes;
  public uint crc;
 }
 public static class ValoriaContentPolicy {
  public const long MaximumBytes=512L*1024*1024;
  public const string Scene="Assets/Eldoria/ArtTests/ValoriaReferenceClean/Valoria_Reference_Clean.unity";
  public const string DownloadPrefix="https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/releases/download/valoria-content-";
  public static bool Accept(ValoriaContentManifest m,string platform,string runtime,int playerCode,out string reason){
   reason="Contenido incompatible";
   if(m==null||m.schema!=1||m.revision<=0||m.minimumPlayerCode>playerCode||m.minimumPlayerCode<=0)return false;
   if(m.platform!=platform||m.unityVersion!=UnityEngine.Application.unityVersion||m.runtimeContract!=runtime)return false;
   if(m.scene!=Scene||m.bytes<=0||m.bytes>MaximumBytes||m.crc==0)return false;
   if(string.IsNullOrEmpty(m.sha256)||!Regex.IsMatch(m.sha256,"^[0-9a-f]{64}$"))return false;
   if(string.IsNullOrEmpty(m.sceneSourceSha256)||!Regex.IsMatch(m.sceneSourceSha256,"^[0-9a-f]{64}$"))return false;
   if(string.IsNullOrEmpty(m.url)||!m.url.StartsWith(DownloadPrefix,StringComparison.Ordinal))return false;
   if(!Uri.TryCreate(m.url,UriKind.Absolute,out var u)||u.Scheme!="https"||u.Host!="github.com"||!string.IsNullOrEmpty(u.Query)||!string.IsNullOrEmpty(u.Fragment)||!string.IsNullOrEmpty(u.UserInfo))return false;
   if(!Regex.IsMatch(u.AbsolutePath,@"^/mt5hjfz2kb-lab/Eldoria-Prewiu/releases/download/valoria-content-[0-9]+/valoria-(android|windows)-[0-9a-f]{64}\.bundle$"))return false;
   if(!u.AbsolutePath.EndsWith("/valoria-"+platform+"-"+m.sha256+".bundle",StringComparison.Ordinal))return false;
   reason=null;return true;
  }
  public static string Hash(string path){using(var h=SHA256.Create())using(var s=File.OpenRead(path))return BitConverter.ToString(h.ComputeHash(s)).Replace("-","").ToLowerInvariant();}
  public static bool Verify(string path,ValoriaContentManifest m){return File.Exists(path)&&new FileInfo(path).Length==m.bytes&&Hash(path)==m.sha256;}
  public static bool RecheckOnResume(DateTime paused,DateTime now,bool ready){return ready&&paused!=default(DateTime)&&(now-paused).TotalSeconds>=30;}
 }
 // Attached only to the opt-in bootstrap scene, never the validated fallback scene.
 public sealed class ValoriaContentUpdate:MonoBehaviour {
  public const string BootstrapScene="Valoria_Content_Bootstrap";
  public const string FallbackScene="Valoria_Included_Fallback";
  public const string FallbackScenePath="Assets/Eldoria/ArtTests/ValoriaReferenceClean/Valoria_Included_Fallback.unity";
  public const string Channel="https://raw.githubusercontent.com/mt5hjfz2kb-lab/Eldoria-Prewiu/construction/valoria-master-plan-layers-20261016/tools/valoria-reference-clean/content-channel.json";
  public string RuntimeContract;
  public int PlayerCode=6;
  public string Status="Buscando contenido de Valoria…";
  string cache;
  AssetBundle loaded;
  bool sceneReady,fallbackOpened;int appliedRevision;
  DateTime pausedAt;bool restarting;
  public bool ContentReady=>sceneReady;
#if UNITY_EDITOR
  public bool SuppressStartupForEditorProbe;
  public string EditorChannelFile,EditorBundleFile,EditorCacheDirectory;
#endif
  string Platform=>UnityEngine.Application.platform==RuntimePlatform.Android?"android":"windows";
  void Awake(){DontDestroyOnLoad(gameObject);}
  IEnumerator Start(){
#if UNITY_EDITOR
   if(SuppressStartupForEditorProbe)yield break;
#endif
   cache=Path.Combine(UnityEngine.Application.persistentDataPath,"valoria-content-v1");
#if UNITY_EDITOR
   if(!string.IsNullOrEmpty(EditorCacheDirectory))cache=EditorCacheDirectory;
#endif
   try{Directory.CreateDirectory(cache);}catch(Exception e){Debug.LogWarning("VALORIA_CONTENT_CACHE "+e.Message);}
   ValoriaContentManifest next=null;
   string channel=Channel+"?check="+DateTime.UtcNow.Ticks;
#if UNITY_EDITOR
   if(!string.IsNullOrEmpty(EditorChannelFile))channel=new Uri(EditorChannelFile).AbsoluteUri;
#endif
   using(var req=UnityWebRequest.Get(channel)){
    req.timeout=8;yield return req.SendWebRequest();
    if(req.result==UnityWebRequest.Result.Success&&req.downloadHandler.data.Length<16384){try{next=JsonUtility.FromJson<ValoriaContentManifest>(req.downloadHandler.text);}catch(Exception e){Debug.LogWarning("VALORIA_CONTENT_MANIFEST "+e.Message);}}
   }
   var current=ReadCurrent();
   if(ValoriaContentPolicy.Accept(next,Platform,RuntimeContract,PlayerCode,out _)&&(current==null||next.revision>current.revision)){
    Status="Descargando Valoria · se conserva tu partida";
    string path=Path.Combine(cache,next.sha256+".bundle");
    if(!SafeVerify(path,next)){
     string part=path+".part";
     string url=next.url;
#if UNITY_EDITOR
     if(!string.IsNullOrEmpty(EditorBundleFile))url=new Uri(EditorBundleFile).AbsoluteUri;
#endif
     var download=CreateDownload(url,part);bool downloaded=false;
     if(download!=null)using(var req=download){
      var op=req.SendWebRequest();
      while(!op.isDone){Status="Descargando Valoria · "+Mathf.RoundToInt(Mathf.Clamp01(req.downloadProgress)*100)+" %";if(req.downloadedBytes>(ulong)ValoriaContentPolicy.MaximumBytes)req.Abort();yield return null;}
      downloaded=req.result==UnityWebRequest.Result.Success;
     }
     // Close DownloadHandlerFile before hashing/renaming, especially on Windows.
     if(downloaded&&SafeVerify(part,next)){
      try{if(File.Exists(path))File.Delete(path);File.Move(part,path);}catch(Exception e){Debug.LogWarning("VALORIA_CONTENT_STORE "+e.Message);}
     }
     try{if(File.Exists(part))File.Delete(part);}catch(Exception e){Debug.LogWarning(e.Message);}
    }
    if(SafeVerify(path,next)){
     yield return Load(next,path);
     if(sceneReady){Commit(next);yield break;}
    }
   }
   if(current!=null){yield return Load(current,Path.Combine(cache,current.sha256+".bundle"));if(sceneReady)yield break;}
   Status="Abriendo la versión incluida";
   AsyncOperation fallback;
#if UNITY_EDITOR
   fallback=UnityEditor.SceneManagement.EditorSceneManager.LoadSceneAsyncInPlayMode(FallbackScenePath,new LoadSceneParameters(LoadSceneMode.Single));
#else
   fallback=SceneManager.LoadSceneAsync(FallbackScene,LoadSceneMode.Single);
#endif
   yield return fallback;
   fallbackOpened=true;Status="Versión incluida";
  }
  UnityWebRequest CreateDownload(string url,string path){
   UnityWebRequest req=null;
   try{req=UnityWebRequest.Get(url);req.downloadHandler=new DownloadHandlerFile(path){removeFileOnAbort=true};req.timeout=120;return req;}
   catch(Exception e){if(req!=null)req.Dispose();Debug.LogWarning("VALORIA_CONTENT_DOWNLOAD "+e.Message);return null;}
  }
  bool SafeVerify(string path,ValoriaContentManifest m){try{return ValoriaContentPolicy.Verify(path,m);}catch(Exception e){Debug.LogWarning("VALORIA_CONTENT_VERIFY "+e.Message);return false;}}
  ValoriaContentManifest ReadCurrent(){
   try{var p=Path.Combine(cache,"current.json");if(!File.Exists(p))return null;var m=JsonUtility.FromJson<ValoriaContentManifest>(File.ReadAllText(p));return ValoriaContentPolicy.Accept(m,Platform,RuntimeContract,PlayerCode,out _)&&SafeVerify(Path.Combine(cache,m.sha256+".bundle"),m)?m:null;}
   catch(Exception e){Debug.LogWarning("VALORIA_CONTENT_CACHE "+e.Message);return null;}
  }
  void Commit(ValoriaContentManifest m){
   try{string path=Path.Combine(cache,"current.json"),tmp=path+".next";File.WriteAllText(tmp,JsonUtility.ToJson(m,true));if(File.Exists(path))File.Replace(tmp,path,path+".previous");else File.Move(tmp,path);PruneCache(m);}
   catch(Exception e){Debug.LogWarning("VALORIA_CONTENT_COMMIT "+e.Message);}
  }
  void PruneCache(ValoriaContentManifest current){
   // Only this updater's hash-named bundle cache; never gameplay saves or user files.
   try{
    var keep=new System.Collections.Generic.HashSet<string>{current.sha256};var previous=Path.Combine(cache,"current.json.previous");
    if(File.Exists(previous)){var m=JsonUtility.FromJson<ValoriaContentManifest>(File.ReadAllText(previous));if(m!=null&&!string.IsNullOrEmpty(m.sha256)&&Regex.IsMatch(m.sha256,"^[0-9a-f]{64}$"))keep.Add(m.sha256);}
    foreach(var file in Directory.GetFiles(cache,"*.bundle")){var name=Path.GetFileNameWithoutExtension(file);if(Regex.IsMatch(name,"^[0-9a-f]{64}$")&&!keep.Contains(name))File.Delete(file);}
   }catch(Exception e){Debug.LogWarning("VALORIA_CONTENT_OLD_CACHE "+e.Message);}
  }
  IEnumerator Load(ValoriaContentManifest m,string path){
   sceneReady=false;
   var load=AssetBundle.LoadFromFileAsync(path,m.crc);yield return load;loaded=load.assetBundle;
   if(loaded==null)yield break;
   var scenes=loaded.GetAllScenePaths();
   if(scenes.Length!=1||!string.Equals(scenes[0],ValoriaContentPolicy.Scene,StringComparison.OrdinalIgnoreCase)){loaded.Unload(true);loaded=null;yield break;}
   Status="Abriendo Valoria · contenido "+m.revision;
   AsyncOperation op=null;try{op=SceneManager.LoadSceneAsync(scenes[0],LoadSceneMode.Single);}catch(Exception e){Debug.LogWarning("VALORIA_CONTENT_SCENE "+e.Message);}
   if(op!=null){yield return op;yield return null;var h=FindFirstObjectByType<ValoriaReferenceCleanGameplay>();sceneReady=h!=null&&h.Ready;}
   if(sceneReady){appliedRevision=m.revision;Status="Contenido "+m.revision;Debug.Log("VALORIA_CONTENT_READY revision="+m.revision);}
   else{loaded.Unload(false);loaded=null;}
  }
  void OnGUI(){
#if UNITY_EDITOR
   if(SuppressStartupForEditorProbe)return;
#endif
   if(!sceneReady&&!fallbackOpened)GUI.Box(new Rect(20,20,Mathf.Min(Screen.width-40,600),70),Status);
  }
  void OnDestroy(){if(loaded!=null)loaded.Unload(false);}
  void OnApplicationPause(bool paused){
   if(paused){pausedAt=DateTime.UtcNow;return;}
   bool check=ValoriaContentPolicy.RecheckOnResume(pausedAt,DateTime.UtcNow,sceneReady||fallbackOpened);pausedAt=default(DateTime);
   if(!check||restarting)return;
#if UNITY_EDITOR
   if(SuppressStartupForEditorProbe||!string.IsNullOrEmpty(EditorChannelFile))return;
#endif
   restarting=true;StartCoroutine(CheckAfterResume());
  }
  IEnumerator CheckAfterResume(){
   bool update=false;
   using(var req=UnityWebRequest.Get(Channel+"?check="+DateTime.UtcNow.Ticks)){
    req.timeout=8;yield return req.SendWebRequest();
    if(req.result==UnityWebRequest.Result.Success&&req.downloadHandler.data.Length<16384){
     try{var next=JsonUtility.FromJson<ValoriaContentManifest>(req.downloadHandler.text);update=ValoriaContentPolicy.Accept(next,Platform,RuntimeContract,PlayerCode,out _)&&next.revision>appliedRevision;}
     catch(Exception e){Debug.LogWarning("VALORIA_CONTENT_RESUME_MANIFEST "+e.Message);}
    }
   }
   restarting=false;if(!update)yield break;
   // No reload when unchanged/offline. Commands persist synchronously through
   // the existing gateway; a newer compatible revision re-enters the bootstrap.
   try{if(loaded!=null){loaded.Unload(false);loaded=null;}SceneManager.LoadScene(BootstrapScene,LoadSceneMode.Single);Destroy(gameObject);}
   catch(Exception e){Debug.LogWarning("VALORIA_CONTENT_RESUME "+e.Message);}
  }
 }
}
