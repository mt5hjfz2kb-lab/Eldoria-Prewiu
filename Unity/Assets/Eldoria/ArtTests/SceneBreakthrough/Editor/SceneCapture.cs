using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
namespace Eldoria.Breakthrough {
public static partial class SceneCapture {
 const string Root="Assets/Eldoria/ArtTests/SceneBreakthrough";
 const string Evidence="../docs/evidence/valoria-scene-breakthrough-20261016";
 static Camera cam; static int serial; static readonly List<UnityEngine.Object> owned=new List<UnityEngine.Object>();
 static Material stone,grass,rock,water,wood,blue,forest,path;
 static Vector3 Right=>Vector3.ProjectOnPlane(cam.transform.right,Vector3.up).normalized;
 static Vector3 Forward=>Vector3.Cross(Right,Vector3.up);
 static Quaternion Facing=>Quaternion.LookRotation(Forward,Vector3.up);
 static Vector3 P(float x,float y,float elevation) {
   var ray=cam.ViewportPointToRay(new Vector3(x/1536f,1-y/1024f,0));
   float t=(elevation-ray.origin.y)/ray.direction.y; return ray.GetPoint(t);
 }
 static Material Mat(string name,Color color) {
   var m=new Material(Shader.Find("Universal Render Pipeline/Lit")); m.name=name;m.color=color;
   m.SetFloat("_Smoothness",.15f); owned.Add(m);return m;
 }
 static GameObject Box(string name,Vector3 pos,Vector3 size,Material material) {
   var o=GameObject.CreatePrimitive(PrimitiveType.Cube);o.name=name;o.transform.position=pos;
   o.transform.rotation=Facing;o.transform.localScale=size;o.GetComponent<Renderer>().sharedMaterial=material;return o;
 }
 static void Deck(string name,Vector2[] outline,float top,float bottom,Material topMat,Material sideMat) {
   var v=new List<Vector3>();foreach(var p in outline)v.Add(P(p.x,p.y,top));
   int n=v.Count;var center=Vector3.zero;foreach(var p in v)center+=p;center/=n;
   v.Add(center);for(int i=0;i<n;i++)v.Add(new Vector3(v[i].x,bottom,v[i].z));
   var faces=new List<int>();for(int i=0;i<n;i++){faces.Add(n);faces.Add(i);faces.Add((i+1)%n);}
   var sides=new List<int>();for(int i=0;i<n;i++){int j=(i+1)%n;int a=i,b=j,c=n+1+i,d=n+1+j;sides.AddRange(new[]{a,c,b,b,c,d});}
   var mesh=new Mesh();mesh.name=name;mesh.vertices=v.ToArray();mesh.subMeshCount=2;
   mesh.SetTriangles(faces,0);mesh.SetTriangles(sides,1);mesh.RecalculateNormals();mesh.RecalculateBounds();owned.Add(mesh);
   // Correct winding automatically from the mean top normal.
   if(mesh.normals[n].y<0){for(int i=0;i<faces.Count;i+=3){int t=faces[i+1];faces[i+1]=faces[i+2];faces[i+2]=t;}mesh.SetTriangles(faces,0);mesh.RecalculateNormals();}
   var go=new GameObject(name);go.AddComponent<MeshFilter>().sharedMesh=mesh;
   go.AddComponent<MeshRenderer>().sharedMaterials=new[]{topMat,sideMat};go.AddComponent<MeshCollider>().sharedMesh=mesh;
 }
 static void Strip(string name,Vector3 a,Vector3 b,float width,float thickness,Material m) {
   var o=Box(name,(a+b)/2+Vector3.up*thickness/2,new Vector3(width,thickness,Vector3.Distance(a,b)),m);
   o.transform.rotation=Quaternion.LookRotation((b-a).normalized,Vector3.up);
 }
 static void Tower(string name,Vector3 foot,float width,float height) {
   Box(name,foot+Vector3.up*height/2,new Vector3(width,height,width),stone);
   Box(name+" crown",foot+Vector3.up*(height-.2f),new Vector3(width+ .4f,.65f,width+.4f),stone);
 }
 static void Gate(string name,Vector3 foot,float span,float height,bool twin) {
   float opening=span*.31f, thickness=2.5f;
   Box(name+" west pier",foot-Right*(span+opening)/4+Vector3.up*height*.44f,new Vector3((span-opening)/2,height*.88f,thickness),stone);
   Box(name+" east pier",foot+Right*(span+opening)/4+Vector3.up*height*.44f,new Vector3((span-opening)/2,height*.88f,thickness),stone);
   Box(name+" lintel",foot+Vector3.up*height*.8f,new Vector3(span,height*.2f,thickness),stone);
   if(twin){Tower(name+" west tower",foot-Right*span*.4f,span*.25f,height+1.1f);Tower(name+" east tower",foot+Right*span*.4f,span*.25f,height+1.1f);}
   else Tower(name+" keep",foot-Right*span*.2f,span*.2f,height+2.5f);
   var anchor=new GameObject(name+" selection");anchor.transform.position=foot+Vector3.up*2;var c=anchor.AddComponent<BoxCollider>();c.isTrigger=true;c.size=new Vector3(span,4,4);
 }
 static void Tree(Vector3 foot,float h) {
   Box("Blockout trunk",foot+Vector3.up*h*.3f,new Vector3(.3f,h*.6f,.3f),wood);
   for(int j=0;j<3;j++){var o=GameObject.CreatePrimitive(PrimitiveType.Cylinder);o.name="Blockout forest mass";o.transform.position=foot+Vector3.up*h*(.42f+j*.2f);o.transform.localScale=new Vector3(h*(.43f-j*.09f),h*.20f,h*(.43f-j*.09f));o.GetComponent<Renderer>().sharedMaterial=forest;UnityEngine.Object.DestroyImmediate(o.GetComponent<Collider>());}
 }
 public static void Run() {
  var old=GraphicsSettings.defaultRenderPipeline;var oldQ=QualitySettings.renderPipeline;
  try {
   Directory.CreateDirectory(Root+"/Generated");Directory.CreateDirectory(Evidence);
   EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
   var urp=UnityEngine.Object.Instantiate(old as UniversalRenderPipelineAsset);if(urp==null)throw new Exception("URP missing");
   urp.name="Breakthrough experimental URP";urp.shadowDistance=160;urp.shadowCascadeCount=2;urp.msaaSampleCount=4;urp.useSRPBatcher=true;owned.Add(urp);
   GraphicsSettings.defaultRenderPipeline=urp;QualitySettings.renderPipeline=urp;
   cam=new GameObject("Player camera - fixed orientation").AddComponent<Camera>();cam.tag="MainCamera";cam.orthographic=true;cam.orthographicSize=24;
   cam.aspect=1.5f;cam.nearClipPlane=.1f;cam.farClipPlane=400;cam.transform.position=new Vector3(30.818315f,63.093408f,-84.672624f);cam.transform.LookAt(Vector3.zero);
   cam.clearFlags=CameraClearFlags.SolidColor;cam.backgroundColor=new Color(.18f,.26f,.34f);
   stone=Mat("Blockout warm stone",new Color(.58f,.55f,.48f));grass=Mat("Blockout buildable ground",new Color(.37f,.4f,.22f));
   rock=Mat("Blockout exposed escarpment",new Color(.35f,.37f,.4f));water=Mat("Blockout river",new Color(.1f,.3f,.39f));
   wood=Mat("Blockout timber",new Color(.28f,.17f,.1f));blue=Mat("Blockout canvas",new Color(.12f,.25f,.48f));
   forest=Mat("Blockout forest",new Color(.12f,.23f,.18f));path=Mat("Blockout access spine",new Color(.55f,.49f,.38f));
   var platform=new[]{new Vector2(82,547),new Vector2(116,482),new Vector2(190,401),new Vector2(240,358),new Vector2(363,334),new Vector2(485,301),new Vector2(590,305),new Vector2(686,328),new Vector2(820,345),new Vector2(1050,326),new Vector2(1208,338),new Vector2(1390,375),new Vector2(1486,409),new Vector2(1530,493),new Vector2(1498,593),new Vector2(1471,686),new Vector2(1380,757),new Vector2(1220,788),new Vector2(1050,788),new Vector2(890,735),new Vector2(789,694),new Vector2(675,681),new Vector2(556,671),new Vector2(405,647),new Vector2(299,622),new Vector2(186,586)};
   Deck("Main continuous buildable plateau",platform,7,0,grass,rock);
   Deck("Upper terrace",new[]{new Vector2(420,205),new Vector2(580,139),new Vector2(1150,180),new Vector2(1355,255),new Vector2(1390,320),new Vector2(1110,320),new Vector2(980,280),new Vector2(850,274),new Vector2(790,315),new Vector2(600,283)},11,7,grass,rock);
   Deck("Foreground bank",new[]{new Vector2(-150,780),new Vector2(240,825),new Vector2(550,885),new Vector2(960,1024),new Vector2(-150,1200)},3,-2,grass,rock);
   Box("Water",(P(768,600,0)),new Vector3(180,.15f,180),water);
   var lower=P(691,702,7);var upper=P(952,217,11);var stairFoot=P(860,365,7);var stairTop=P(905,280,11);
   Gate("Lower gate",lower,11,7.2f,true);Gate("Upper citadel",upper,25,5.8f,false);
   Strip("Bridge functional deck",P(420,924,7),lower,6,.35f,path);
   Strip("Road functional surface",lower,stairFoot,4.7f,.12f,path);Strip("Upper landing",stairTop,upper,4.7f,.12f,path);
   for(int i=0;i<20;i++){float t=(i+.5f)/20;var pos=Vector3.Lerp(stairFoot,stairTop,t);Box("Stair tread "+i,pos,new Vector3(6,.24f,Vector3.Distance(stairFoot,stairTop)/20+.1f),stone);}
   Strip("Stair traversable ramp",stairFoot,stairTop,5,.08f,path);
   foreach(var x in new[]{-1,1}){Strip("Bridge parapet "+x,P(420,924,7)+Right*3*x,lower+Right*3*x, .45f,1.0f,stone);}
   Box("West partial wall",P(411,536,7)+Vector3.up*2,new Vector3(8,4,1.2f),stone);
   Box("East partial wall",P(1361,590,7)+Vector3.up*2,new Vector3(11,4,1.2f),stone);
   var cabin=P(451,398,7);Box("Cabin",cabin+Vector3.up*1.5f,new Vector3(5,3,4),wood);Box("Cabin roof",cabin+Vector3.up*3.3f,new Vector3(5.8f,1,4.7f),wood);
   Box("Camp tent",P(1261,457,7)+Vector3.up*1.4f,new Vector3(6,2.8f,4),blue);
   // Construction reserves retain substantial empty usable ground.
   foreach(var p in new[]{new Vector2(630,490),new Vector2(1030,520)}){var anchor=new GameObject("Bastion II expansion reserve");anchor.transform.position=P(p.x,p.y,7);var c=anchor.AddComponent<BoxCollider>();c.isTrigger=true;c.size=new Vector3(9,.15f,9);}
   var rng=new System.Random(40);
   foreach(int side in new[]{0,1})for(int i=0;i<15;i++){float x=side==0?115+i*17:1430+i*4;float y=side==0?535-i*25:680-i*28;float h=4+(float)rng.NextDouble()*3;Tree(P(x,y,side==0&&i>11?11:7),h);}
   for(int i=0;i<12;i++)Tree(P(420+i*78,175+(i%3)*10,11),4+(i%4));
   var sun=new GameObject("Single afternoon sun").AddComponent<Light>();sun.type=LightType.Directional;sun.intensity=1.15f;sun.color=new Color(1,.88f,.72f);sun.shadows=LightShadows.Soft;sun.transform.rotation=Quaternion.Euler(40,-45,0);
   RenderSettings.ambientMode=AmbientMode.Trilight;RenderSettings.ambientSkyColor=new Color(.48f,.55f,.65f);RenderSettings.ambientEquatorColor=new Color(.4f,.42f,.38f);RenderSettings.ambientGroundColor=new Color(.24f,.23f,.2f);RenderSettings.fog=false;
   cam.gameObject.AddComponent<SceneCamera>();
   Physics.SyncTransforms();
   // Exact point/route checks are isolated inspection evidence, not full gameplay certification.
   foreach(var p in new[]{lower,stairFoot,stairTop,cabin,P(1261,457,7)})if(!Physics.Raycast(p+Vector3.up*40,Vector3.down,100,~0,QueryTriggerInteraction.Ignore))throw new Exception("Missing supporting geometry at "+p);
   Capture("blockout-main",1536,1024);Capture("blockout-landscape",1280,720);
   Capture("blockout-portrait",390,844);
   cam.transform.position+=Right*7;Capture("blockout-pan",1536,1024);cam.transform.position-=Right*7;
   cam.orthographicSize=17;Capture("blockout-zoom",1536,1024);cam.orthographicSize=24;
   foreach(var o in owned){string p=Root+"/Generated/"+(serial++).ToString("D3")+".asset";AssetDatabase.DeleteAsset(p);AssetDatabase.CreateAsset(o,p);}
   AssetDatabase.SaveAssets();EditorSceneManager.SaveScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene(),Root+"/BastionI_Blockout.unity");
   File.WriteAllText(Evidence+"/blockout-result.json","{\"stage\":\"BLOCKOUT_REVIEW_REQUIRED\",\"engine\":\""+UnityEngine.Application.unityVersion+"\",\"art_approved\":false,\"mobile_performance_pass\":false,\"gameplay_pass\":false,\"support_raycasts\":5,\"source_commit\":\""+Environment.GetEnvironmentVariable("GITHUB_SHA")+"\"}");
   Debug.Log("BREAKTHROUGH_CAPTURE_COMPLETE");GraphicsSettings.defaultRenderPipeline=old;QualitySettings.renderPipeline=oldQ;EditorApplication.Exit(0);
  }catch(Exception e){Debug.LogException(e);GraphicsSettings.defaultRenderPipeline=old;QualitySettings.renderPipeline=oldQ;EditorApplication.Exit(1);}
 }
 [Serializable] class FrameEvidence {public string source_commit,engine,pipeline,color_space;public int width,height;public float orthographic_size;public Vector3 position;public Quaternion rotation;public Matrix4x4 view,projection;}
 static void Capture(string name,int width,int height) {
   cam.aspect=(float)width/height;var rt=new RenderTexture(width,height,24){antiAliasing=4};var prior=RenderTexture.active;var tex=new Texture2D(width,height,TextureFormat.RGB24,false);
   bool previous=ShaderUtil.allowAsyncCompilation;ShaderUtil.allowAsyncCompilation=false;
   try{cam.targetTexture=rt;cam.Render();cam.Render();cam.Render();RenderTexture.active=rt;tex.ReadPixels(new Rect(0,0,width,height),0,0);tex.Apply();File.WriteAllBytes(Evidence+"/"+name+".png",tex.EncodeToPNG());File.WriteAllBytes(Evidence+"/"+name+".jpg",tex.EncodeToJPG(82));File.WriteAllText(Evidence+"/camera-"+name+".json",JsonUtility.ToJson(new FrameEvidence{source_commit=Environment.GetEnvironmentVariable("GITHUB_SHA"),engine=UnityEngine.Application.unityVersion,pipeline=GraphicsSettings.currentRenderPipeline!=null?GraphicsSettings.currentRenderPipeline.name:"Built-in",color_space=QualitySettings.activeColorSpace.ToString(),width=width,height=height,orthographic_size=cam.orthographicSize,position=cam.transform.position,rotation=cam.transform.rotation,view=cam.worldToCameraMatrix,projection=cam.projectionMatrix},true));}
   finally{ShaderUtil.allowAsyncCompilation=previous;cam.targetTexture=null;RenderTexture.active=prior;rt.Release();UnityEngine.Object.DestroyImmediate(rt);UnityEngine.Object.DestroyImmediate(tex);}
 }
}
}
