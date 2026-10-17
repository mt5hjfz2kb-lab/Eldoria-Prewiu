using UnityEngine;
namespace Eldoria.ReferenceClean {
public class ValoriaReferenceCleanInteraction:MonoBehaviour {
 public Camera playerCamera;public string selected="Valoria — Bastión I";public int plot=-1;public Vector3 homePosition;public Quaternion homeRotation;public float homeZoom=48;
 Vector3 last;bool dragged;
 void Awake(){if(!playerCamera)playerCamera=Camera.main;}
 void Update(){
  if(!playerCamera)return;
  if(Input.mousePosition.x<295 && Input.mousePosition.y>Screen.height-125 && Input.touchCount<2)return;
  float scroll=Input.mouseScrollDelta.y;if(Mathf.Abs(scroll)>.01f)playerCamera.orthographicSize=Mathf.Clamp(playerCamera.orthographicSize-scroll*2,14,65);
  if(Input.GetMouseButtonDown(0)){last=Input.mousePosition;dragged=false;}
  if(Input.GetMouseButton(0)){var delta=Input.mousePosition-last;if(delta.sqrMagnitude>9)dragged=true;if(dragged){var right=playerCamera.transform.right;right.y=0;var forward=Vector3.Cross(right,Vector3.up);playerCamera.transform.position-=right*delta.x*playerCamera.orthographicSize/Screen.height*2+forward*delta.y*playerCamera.orthographicSize/Screen.height*2;}last=Input.mousePosition;var p=playerCamera.transform.position;p.x=Mathf.Clamp(p.x,homePosition.x-45,homePosition.x+45);p.z=Mathf.Clamp(p.z,homePosition.z-45,homePosition.z+45);playerCamera.transform.position=p;}
  if(Input.touchCount==2){var a=Input.GetTouch(0);var b=Input.GetTouch(1);var previous=(a.position-a.deltaPosition)-(b.position-b.deltaPosition);playerCamera.orthographicSize=Mathf.Clamp(playerCamera.orthographicSize+(previous.magnitude-(a.position-b.position).magnitude)*.035f,14,65);}
  if(Input.GetMouseButtonUp(0)&&!dragged){if(Physics.Raycast(playerCamera.ScreenPointToRay(Input.mousePosition),out var hit,500,1<<8))Select(hit.collider.name);}
 }
 public void Select(string id){selected=id;plot=id.StartsWith("Parcela")?int.Parse(id.Substring(id.Length-1)): -1;}
 void OnGUI(){
  GUI.Box(new Rect(12,12,270,66),selected+"\n"+(plot>0?"Parcela libre · preparada para construir":"Selecciona el Bastión o una parcela"));
  if(GUI.Button(new Rect(12,84,95,32),"Vista inicial")){playerCamera.transform.SetPositionAndRotation(homePosition,homeRotation);playerCamera.orthographicSize=homeZoom;}
  if(GUI.Button(new Rect(115,84,45,32),"+"))playerCamera.orthographicSize=Mathf.Max(14,playerCamera.orthographicSize-4);
  if(GUI.Button(new Rect(168,84,45,32),"−"))playerCamera.orthographicSize=Mathf.Min(65,playerCamera.orthographicSize+4);
 }
}
}
