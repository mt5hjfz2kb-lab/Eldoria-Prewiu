using UnityEngine;
namespace Eldoria.ReferenceClean {
public class ValoriaReferenceCleanInteraction:MonoBehaviour {
 public Camera playerCamera;public string selected="Valoria — Bastión I";public int plot=-1;public Vector3 homePosition;public Quaternion homeRotation;public float homeZoom=48;
 public bool gameplayOwnsInput;
 Vector3 last;bool dragged;float previousAspect;
 float UiScale=>Mathf.Clamp(Screen.height/720f,1,3);
 void Awake(){if(!playerCamera)playerCamera=Camera.main;}
 public void ZoomBy(float delta){if(!playerCamera)return;playerCamera.orthographicSize=Mathf.Clamp(playerCamera.orthographicSize+delta,14,65);FitToTerrain();}
 public void PanBy(Vector2 delta){if(!playerCamera)return;var right=playerCamera.transform.right;right.y=0;var forward=Vector3.Cross(right,Vector3.up);var p=playerCamera.transform.position-(right*delta.x+forward*delta.y)*playerCamera.orthographicSize/Mathf.Max(1,Screen.height)*2;p.x=Mathf.Clamp(p.x,homePosition.x-45,homePosition.x+45);p.z=Mathf.Clamp(p.z,homePosition.z-45,homePosition.z+45);playerCamera.transform.position=p;FitToTerrain();}
 public void ResetView(){if(!playerCamera)return;playerCamera.transform.SetPositionAndRotation(homePosition,homeRotation);playerCamera.orthographicSize=homeZoom;FitToTerrain();}
 void LateUpdate(){if(playerCamera&&Mathf.Abs(playerCamera.aspect-previousAspect)>.001f){FitToTerrain();previousAspect=playerCamera.aspect;}}
 bool GroundFrame(out Rect frame){
  var plane=new Plane(Vector3.up,new Vector3(0,.35f,0));float xmin=float.PositiveInfinity,zmin=xmin,xmax=float.NegativeInfinity,zmax=xmax;
  for(int i=0;i<4;i++){var ray=playerCamera.ViewportPointToRay(new Vector3(i&1,(i>>1)&1,0));if(!plane.Raycast(ray,out var distance)){frame=default;return false;}var p=ray.GetPoint(distance);xmin=Mathf.Min(xmin,p.x);xmax=Mathf.Max(xmax,p.x);zmin=Mathf.Min(zmin,p.z);zmax=Mathf.Max(zmax,p.z);}
  frame=Rect.MinMaxRect(xmin,zmin,xmax,zmax);return true;
 }
 public void FitToTerrain(){
  var terrain=Terrain.activeTerrain;if(!playerCamera||!terrain||!terrain.terrainData||!GroundFrame(out var frame))return;
  var origin=terrain.transform.position;var size=terrain.terrainData.size;const float margin=10;
  float xmin=origin.x+margin,xmax=origin.x+size.x-margin,zmin=origin.z+margin,zmax=origin.z+size.z-margin;
  float fit=Mathf.Min((xmax-xmin)/frame.width,(zmax-zmin)/frame.height);
  if(fit<1){playerCamera.orthographicSize*=fit*.999f;if(!GroundFrame(out frame))return;}
  float dx=Mathf.Clamp(0,xmin-frame.xMin,xmax-frame.xMax),dz=Mathf.Clamp(0,zmin-frame.yMin,zmax-frame.yMax);
  playerCamera.transform.position+=new Vector3(dx,0,dz);
 }
 void Update(){
  if(!playerCamera||gameplayOwnsInput)return;
  if(Input.touchCount>=2){var a=Input.GetTouch(0);var b=Input.GetTouch(1);var previous=(a.position-a.deltaPosition)-(b.position-b.deltaPosition);ZoomBy((previous.magnitude-(a.position-b.position).magnitude)*.035f);last=a.position;dragged=true;return;}
  if(Input.mousePosition.x<295*UiScale && Input.mousePosition.y>Screen.height-145*UiScale)return;
  float scroll=Input.mouseScrollDelta.y;if(Mathf.Abs(scroll)>.01f)ZoomBy(-scroll*2);
  if(Input.GetMouseButtonDown(0)){last=Input.mousePosition;dragged=false;}
  if(Input.GetMouseButton(0)){var delta=Input.mousePosition-last;if(delta.sqrMagnitude>9)dragged=true;if(dragged)PanBy(new Vector2(delta.x,delta.y));last=Input.mousePosition;}
  if(Input.GetMouseButtonUp(0)&&!dragged){if(Physics.Raycast(playerCamera.ScreenPointToRay(Input.mousePosition),out var hit,500,1<<8))Select(hit.collider.name);}
 }
 public void Select(string id){selected=id;plot=id.StartsWith("Parcela")?int.Parse(id.Substring(id.Length-1)): -1;}
 void OnGUI(){
  if(!playerCamera||gameplayOwnsInput)return;var previous=GUI.matrix;GUI.matrix=Matrix4x4.Scale(new Vector3(UiScale,UiScale,1));
  GUI.Box(new Rect(12,12,270,66),selected+"\n"+(plot>0?"Parcela libre · preparada para construir":"Selecciona el Bastión o una parcela"));
  if(GUI.Button(new Rect(12,84,110,44),"Vista inicial"))ResetView();
  if(GUI.Button(new Rect(130,84,60,44),"+"))ZoomBy(-4);
  if(GUI.Button(new Rect(198,84,60,44),"−"))ZoomBy(4);
  GUI.matrix=previous;
 }
}
}
