// Copy into Unity/Assets/Eldoria/Scripts/Editor ONLY on this isolated branch,
// after checking and acquiring the shared Windows lease. No gameplay/publishing.
using System;
using System.IO;
using System.Reflection;
using System.Security.Cryptography;
using System.Threading.Tasks;
using GLTFast;
using UnityEditor;
using UnityEngine;
namespace Eldoria.EditorTools {
 public static class ValoriaStudioRockUnityProof {
  public static async void Capture() {
   try { await Execute(); EditorApplication.Exit(0); }
   catch(Exception e) { Debug.LogException(e); EditorApplication.Exit(1); }
  }
  static async Task Execute() {
   // Refuse later integration assets until the candidate is rebased; no stale replacement.
   var baseline=Path.GetFullPath("Assets/Eldoria/ArtTests/ValoriaGatehouseV1/valoria_visual_slice_environment_v2.glb");
   using(var sha=SHA256.Create()) {
    var actual=BitConverter.ToString(sha.ComputeHash(File.ReadAllBytes(baseline))).Replace("-", "").ToLowerInvariant();
    if(actual!="98366806ef12197e9c77b51edf9b02cb58cf340900ef167e1e7b8e4a58881595")
     throw new Exception("Integration terrain changed: rebase this isolated candidate before comparison");
   }
   // Reuse the exact existing scene author, including the original camera,
   // fortifications and vegetation, rather than inventing another Unity world.
   var method=typeof(ValoriaGatehouseRealUnityV1).GetMethod("Execute",BindingFlags.Static|BindingFlags.NonPublic);
   if(method==null) throw new Exception("Existing exact Unity proof entrypoint unavailable");
   await (Task)method.Invoke(null,null);
   var cameras=UnityEngine.Object.FindObjectsByType<Camera>(FindObjectsSortMode.None);
   if(cameras.Length!=1)throw new Exception("Matched camera ambiguous");
   var camera=cameras[0];Directory.CreateDirectory("ValoriaStudioRockProof");
   Save(camera,"ValoriaStudioRockProof/unity-before.png");
   int count=0;
   foreach(var mf in UnityEngine.Object.FindObjectsByType<MeshFilter>(FindObjectsSortMode.None)) {
    if(mf.sharedMesh!=null && mf.sharedMesh.name.StartsWith("Valoria authored geological mesh")) {
     var renderer=mf.GetComponent<Renderer>();if(renderer!=null){renderer.enabled=false;count++;}
    }
   }
   if(count==0)throw new Exception("Original environment renderer identity not found; refusing unmatched comparison");
   var path=Path.GetFullPath("../art-source/valoria/production/studio-rocks-v1/ValoriaEnvironmentCandidate.glb");
   var import=new GltfImport(null,new UninterruptedDeferAgent());
   if(!await import.LoadFile(path))throw new Exception("Candidate import failed");
   var root=new GameObject("Isolated studio CC0 cliff candidate");
   if(!await import.InstantiateMainSceneAsync(root.transform))throw new Exception("Candidate instantiate failed");
   Save(camera,"ValoriaStudioRockProof/unity-after.png");
   File.WriteAllText("ValoriaStudioRockProof/evidence.json","{\"actual_unity_capture\":true,\"matching_camera\":true,\"gameplay_changed\":false,\"visual_pass\":false}");
  }
  static void Save(Camera c,string p) {
   var rt=new RenderTexture(1280,720,24,RenderTextureFormat.ARGB32);var prior=c.targetTexture;var active=RenderTexture.active;
   var tex=new Texture2D(1280,720,TextureFormat.RGB24,false);
   try {c.targetTexture=rt;c.Render();RenderTexture.active=rt;tex.ReadPixels(new Rect(0,0,1280,720),0,0);tex.Apply();File.WriteAllBytes(p,tex.EncodeToPNG());}
   finally {c.targetTexture=prior;RenderTexture.active=active;UnityEngine.Object.DestroyImmediate(tex);rt.Release();UnityEngine.Object.DestroyImmediate(rt);}
  }
 }
}
