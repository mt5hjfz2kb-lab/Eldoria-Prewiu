using System;
using System.IO;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.Rendering;
namespace Eldoria.ReferenceClean.Editor {
public static class ValoriaReferenceCleanBuilder {
 const string Root="Assets/Eldoria/ArtTests/ValoriaReferenceClean";
 const string Ev="../docs/evidence/valoria-reference-clean";
 static Material Mat(string n,Color c){var m=new Material(Shader.Find("Universal Render Pipeline/Lit"));m.name=n;m.color=c;AssetDatabase.CreateAsset(m,Root+"/Generated/"+n+".mat");return m;}
 static GameObject Box(string n,Vector3 p,Vector3 s,Material m){var o=GameObject.CreatePrimitive(PrimitiveType.Cube);o.name=n;o.transform.position=p;o.transform.localScale=s;o.GetComponent<Renderer>().sharedMaterial=m;return o;}
 public static void Build(){try {Run();EditorApplication.Exit(0);}catch(Exception e){Debug.LogException(e);EditorApplication.Exit(1);}}
 static void Run(){
 Directory.CreateDirectory(Root+"/Generated");Directory.CreateDirectory(Ev);
 var scene=EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);new GameObject("Isolated scene render settings").AddComponent<ValoriaReferenceCleanRenderSettings>();
 var grass=Mat("Grass",new Color(.32f,.40f,.13f));var stone=Mat("Stone",new Color(.57f,.52f,.43f));var dirt=Mat("Plots",new Color(.47f,.30f,.15f));var water=Mat("Water",new Color(.025f,.28f,.43f));var roof=Mat("Roof",new Color(.045f,.15f,.30f));
 var td=new TerrainData {heightmapResolution=257,size=new Vector3(140,14,150)};
 var heights=new float[257,257];
 for(int z=0;z<257;z++)for(int x=0;x<257;x++) {float wx=x/256f*140-70,wz=z/256f*150-55;float river=-23+1.5f*Mathf.Sin(wx*.065f);float bank=Mathf.SmoothStep(0,1,Mathf.InverseLerp(5.2f,9,Mathf.Abs(wz-river)));float h=.35f+.15f*Mathf.Sin(wx*.12f)*Mathf.Sin(wz*.1f);if(wz>40)h+=(wz-40)*.045f;if((Mathf.Abs(wx)<11&&wz>-45&&wz<39)||(Mathf.Abs(Mathf.Abs(wx)-21)<9&&((Mathf.Abs(wz-2)<6.5f)||(Mathf.Abs(wz-22)<6.5f))))h=.35f;heights[z,x]=(2+h-2.3f*(1-bank))/14;}
 td.SetHeights(0,0,heights);AssetDatabase.CreateAsset(td,Root+"/Generated/Ground.asset");var ground=Terrain.CreateTerrainGameObject(td);ground.name="ContinuousTerrain";ground.transform.position=new Vector3(-70,-2,-55);var tm=new Material(Shader.Find("Universal Render Pipeline/Terrain/Lit"));AssetDatabase.CreateAsset(tm,Root+"/Generated/Terrain.mat");ground.GetComponent<Terrain>().materialTemplate=tm;
 var tex=new Texture2D(4,4);var pixels=new Color[16];for(int i=0;i<16;i++)pixels[i]=grass.color;tex.SetPixels(pixels);tex.Apply();AssetDatabase.CreateAsset(tex,Root+"/Generated/GrassTexture.asset");var layer=new TerrainLayer{diffuseTexture=tex,tileSize=new Vector2(6,6)};AssetDatabase.CreateAsset(layer,Root+"/Generated/GrassLayer.terrainlayer");td.terrainLayers=new[]{layer};
 Box("River",new Vector3(0,-.7f,-23),new Vector3(140,.1f,15),water);
 Box("Bridge_BLOCKOUT",new Vector3(0,-.1f,-23),new Vector3(7,1.5f,23),stone);
 Box("Gate_Left_BLOCKOUT",new Vector3(-4,3.5f,-10),new Vector3(3,7,4),stone);Box("Gate_Right_BLOCKOUT",new Vector3(4,3.5f,-10),new Vector3(3,7,4),stone);Box("Gate_Arch_BLOCKOUT",new Vector3(0,5.4f,-10),new Vector3(6,2,3),stone);
 Box("AxialRoad",new Vector3(0,.39f,8),new Vector3(5,.15f,34),stone);Box("OuterRoad",new Vector3(0,.39f,-43),new Vector3(5,.15f,17),stone);
 Box("Bastion_Main_BLOCKOUT",new Vector3(0,3.1f,32),new Vector3(13,5.5f,9),stone);Box("Bastion_Roof_BLOCKOUT",new Vector3(0,6.2f,32),new Vector3(13,1.2f,9),roof);Box("Bastion_CentralTower_BLOCKOUT",new Vector3(0,6,29),new Vector3(4.5f,11,4.5f),stone);
 foreach(float x in new[]{-8f,8f})Box("Bastion_Flank_BLOCKOUT",new Vector3(x,3.5f,30),new Vector3(3.5f,6.5f,5),stone);
 foreach(float x in new[]{-21f,21f})foreach(float z in new[]{2f,22f})Box("BuildablePlot_BLOCKOUT",new Vector3(x,.38f,z),new Vector3(17,.12f,12),dirt);
 foreach(var p in new[]{new Vector3(-42,1.8f,39),new Vector3(42,1.8f,42),new Vector3(-39,1.8f,-3),new Vector3(41,1.8f,-9)})Box("Ruin_BLOCKOUT",p,new Vector3(7,3.5f,1.5f),stone);
 var sun=new GameObject("Sun").AddComponent<Light>();sun.type=LightType.Directional;sun.transform.rotation=Quaternion.Euler(47,-38,0);sun.intensity=1.1f;sun.color=new Color(1,.89f,.71f);sun.shadows=LightShadows.Soft;RenderSettings.ambientMode=AmbientMode.Flat;RenderSettings.ambientLight=new Color(.3f,.34f,.4f);RenderSettings.fog=false;QualitySettings.shadowDistance=180;
 var cam=new GameObject("PlayerCamera").AddComponent<Camera>();cam.tag="MainCamera";cam.orthographic=true;cam.orthographicSize=34;cam.transform.position=new Vector3(46,77,-104);cam.transform.LookAt(new Vector3(5,0,10));cam.nearClipPlane=.1f;cam.farClipPlane=400;cam.backgroundColor=new Color(.35f,.46f,.5f);cam.clearFlags=CameraClearFlags.SolidColor;
 EditorSceneManager.SaveScene(scene,Root+"/Valoria_Reference_Clean_Blockout.unity");AssetDatabase.SaveAssets();
 Capture(cam,Ev+"/A-blockout.png",1448,1086);
 File.WriteAllText(Ev+"/stage-A.json",JsonUtility.ToJson(new Report {stage="04_BLOCKOUT_REQUIRES_VISUAL_REVIEW",scene=Root+"/Valoria_Reference_Clean_Blockout.unity",source_sha=Environment.GetEnvironmentVariable("GITHUB_SHA"),plots=4,art_approved=false},true));Debug.Log("VALORIA_REFERENCE_CLEAN_STAGE_A_COMPLETE");
 }
 [Serializable]class Report{public string stage,scene,source_sha;public int plots;public bool art_approved;}
 public static void Capture(Camera c,string file,int w,int h){var rt=new RenderTexture(w,h,24,RenderTextureFormat.ARGB32);var descriptor=rt.descriptor;descriptor.msaaSamples=4;rt.antiAliasing=SystemInfo.GetRenderTextureSupportedMSAASampleCount(descriptor);c.targetTexture=rt;c.Render();c.Render();RenderTexture.active=rt;var tex=new Texture2D(w,h,TextureFormat.RGB24,false);tex.ReadPixels(new Rect(0,0,w,h),0,0);tex.Apply();File.WriteAllBytes(file,tex.EncodeToPNG());File.WriteAllText(Path.ChangeExtension(file,".preview.b64"),Convert.ToBase64String(tex.EncodeToJPG(88)));c.targetTexture=null;RenderTexture.active=null;UnityEngine.Object.DestroyImmediate(rt);UnityEngine.Object.DestroyImmediate(tex);}
}
}
