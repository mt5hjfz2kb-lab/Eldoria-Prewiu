using System;
using System.IO;
using System.Linq;
using UnityEngine;
namespace Eldoria.World4XCanon
{
 // Explicit, reversible art-study capture. Never installed by normal game startup.
 public static class RegionOneRiberaPilot
 {
  public static void Capture(Camera camera, Transform world, Vector3 home, string output)
  {
   var prefab=Resources.Load<GameObject>("World4X/Pilot/RiberaPilot");
   if(!prefab)throw new InvalidOperationException("Missing authored ribera pilot");
   var oldPos=camera.transform.position;var oldRot=camera.transform.rotation;float oldSize=camera.orthographicSize;var oldRT=camera.targetTexture;var oldActive=RenderTexture.active;
   var prior=world.GetComponentsInChildren<Renderer>(true).Where(r=>r.enabled).ToArray();
   var root=UnityEngine.Object.Instantiate(prefab,world);root.name="Ribera hybrid art study (opt in)";root.transform.position=home;root.transform.rotation=Quaternion.Euler(0,180,0);
   foreach(var r in root.GetComponentsInChildren<Renderer>())r.gameObject.layer=world.gameObject.layer;
   if(root.GetComponentsInChildren<Collider>().Length!=0)throw new InvalidOperationException("Pilot must not introduce gameplay colliders");
   // Existing playable objects remain alive and retain IDs/colliders. Study hides prior renderers only, restoring in finally.
   foreach(var r in prior)r.enabled=false;
   try {
    Directory.CreateDirectory(output);
    var target=home+new Vector3(0,2,0);camera.transform.position=target+new Vector3(0,130,-145);camera.transform.LookAt(target);
    foreach(float size in new[]{46f,60f,76f,96f}) {
     camera.orthographicSize=size;
     var rt=new RenderTexture(1280,960,24);var tex=new Texture2D(1280,960,TextureFormat.RGB24,false);
     camera.aspect=1280f/960;camera.targetTexture=rt;camera.Render();RenderTexture.active=rt;tex.ReadPixels(new Rect(0,0,1280,960),0,0);tex.Apply();
     File.WriteAllBytes(Path.Combine(output,"ribera-"+size+"-unity.png"),tex.EncodeToPNG());
     File.WriteAllText(Path.Combine(output,"ribera-"+size+"-camera.json"),JsonUtility.ToJson(new Evidence{zoom=size,position=camera.transform.position,rotation=camera.transform.rotation,unity=Application.unityVersion,playMode=Application.isPlaying,trees=root.GetComponentsInChildren<Renderer>().Count(r=>r.name.StartsWith("Pilot_Pine"))},true));
     camera.targetTexture=oldRT;RenderTexture.active=oldActive;rt.Release();UnityEngine.Object.Destroy(rt);UnityEngine.Object.Destroy(tex);
    }
   } finally {foreach(var r in prior)if(r)r.enabled=true;UnityEngine.Object.DestroyImmediate(root);camera.transform.SetPositionAndRotation(oldPos,oldRot);camera.orthographicSize=oldSize;camera.targetTexture=oldRT;RenderTexture.active=oldActive;}
  }
  [Serializable] class Evidence {public float zoom;public Vector3 position;public Quaternion rotation;public string unity;public bool playMode;public int trees;}
 }
}
