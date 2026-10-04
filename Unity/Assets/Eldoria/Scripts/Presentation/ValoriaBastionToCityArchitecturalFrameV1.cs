using System;
using Eldoria.Domain;
using UnityEngine;
using Object=UnityEngine.Object;

namespace Eldoria.Presentation
{
 public static class ValoriaBastionToCityArchitecturalFrameV1
 {
  public static bool Enabled=false;
  public const string RootName="Valoria · Bastion-to-City Architectural Frame v1";
  public static int PiecesBuilt{get;private set;}
  public static int SuppressedRenderers{get;private set;}

  public static void Build(Transform parent,PlayerState state)
  {
   if(!Enabled||parent==null||state==null||state.BastionLevel<3)return;
   var old=GameObject.Find(RootName);if(old!=null)Object.DestroyImmediate(old);
   PiecesBuilt=0;SuppressedRenderers=0;
   SuppressDominantTechnicalFrame();

   var source=Resources.Load<GameObject>("Valoria/ProductionArt/BastionToCityArchitecturalFrameV1/Valoria_BastionToCityArchitecturalFrame_v1");
   if(source==null)throw new InvalidOperationException("Missing Bastion-to-City authored source");

   var go=Object.Instantiate(source);
   go.name=RootName+" · complete authored frame";
   go.transform.rotation=Quaternion.Euler(0f,0f,0f);
   var b=Bounds(go);
   const float targetWidth=16.9f;
   const float targetHeight=6.35f;
   float s=Mathf.Min(targetWidth/Mathf.Max(.001f,b.size.x),targetHeight/Mathf.Max(.001f,b.size.y));
   go.transform.localScale*=s;
   b=Bounds(go);
   var ground=new Vector3(0f,.28f,4.45f);
   go.transform.position+=ground-new Vector3(b.center.x,b.min.y,b.center.z);
   go.transform.SetParent(parent,true);
   DisableGameplay(go);PiecesBuilt=1;
  }

  static void SuppressDominantTechnicalFrame()
  {
   foreach(var r in Object.FindObjectsByType<Renderer>(FindObjectsSortMode.None))
   {
    if(r==null||!r.enabled||!r.gameObject.activeInHierarchy)continue;
    string h=Hierarchy(r.transform);
    if(h.Contains("hero bastion")||h.Contains("granero")||h.Contains("cuartel")||
       h.Contains("lower-city urban massing")||h.Contains("west craft court")||
       h.Contains("east merchant front")||h.Contains("upper terrace houses"))continue;

    bool named=
      h.Contains("retaining")||h.Contains("support")||h.Contains("shelf")||
      h.Contains("rescued seam")||h.Contains("terrainterrace");
    if(!named)continue;

    var b=r.bounds;var c=b.center;
    bool stairFrame=Mathf.Abs(c.x)<=8.8f&&c.z>=1.0f&&c.z<=10.8f&&b.size.y>=.35f;
    if(!stairFrame)continue;
    r.enabled=false;SuppressedRenderers++;
   }
  }

  static Bounds Bounds(GameObject go)
  {
   var rs=go.GetComponentsInChildren<Renderer>(true);
   if(rs.Length==0)return new Bounds(go.transform.position,Vector3.one);
   var b=rs[0].bounds;for(int i=1;i<rs.Length;i++)b.Encapsulate(rs[i].bounds);return b;
  }
  static string Hierarchy(Transform t){string s="";for(var p=t;p!=null;p=p.parent)s+="|"+p.name.ToLowerInvariant();return s;}
  static void DisableGameplay(GameObject go)
  {
   foreach(var c in go.GetComponentsInChildren<Collider>(true))c.enabled=false;
   foreach(var h in go.GetComponentsInChildren<WorldHotspot>(true))Object.DestroyImmediate(h);
   foreach(var m in go.GetComponentsInChildren<MonoBehaviour>(true))if(!(m is WorldHotspot))m.enabled=false;
  }
 }
}
