#if UNITY_EDITOR
using System;
using System.IO;
using UnityEditor;
using UnityEngine;

// Installed only in the preserved, isolated beta probe directory.
// run_warm_structural.ps1 restores every ProjectSettings byte after the run.
public static class EldoriaBetaCollisionBake {
 [Serializable] sealed class Result {
  public string source;
  public bool before,after,physicalIphoneVerified=false;
 }
 public static void Enable() {
  int exit=1;
  try {
   var output=Environment.GetEnvironmentVariable("ELDORIA_COLLISION_BAKE_OUT");
   if(string.IsNullOrEmpty(output)||!Directory.Exists(output))
    throw new InvalidOperationException("Explicit isolated evidence directory required");
   var result=new Result{source=Environment.GetEnvironmentVariable("GITHUB_SHA"),
    before=PlayerSettings.bakeCollisionMeshes};
   PlayerSettings.bakeCollisionMeshes=true;
   AssetDatabase.SaveAssets();
   result.after=PlayerSettings.bakeCollisionMeshes;
   if(!result.after)throw new InvalidOperationException("Collision prebake was not enabled");
   File.WriteAllText(Path.Combine(output,"collision-bake-setting.json"),JsonUtility.ToJson(result,true));
   Debug.Log("ELDORIA_COLLISION_PREBAKE_ENABLED geometry_unchanged=1 physical_iphone_verified=0");
   exit=0;
  } catch(Exception error) { Debug.LogException(error); }
  EditorApplication.Exit(exit);
 }
}
#endif
