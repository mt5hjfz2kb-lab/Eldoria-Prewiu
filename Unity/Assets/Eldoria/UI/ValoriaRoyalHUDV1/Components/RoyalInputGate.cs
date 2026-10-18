using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
namespace Eldoria.UI.RoyalGold {
 public static class RoyalInputGate {
  static readonly List<RaycastResult> hits=new List<RaycastResult>();
  // Call at the start of the existing world gesture, retaining ownership until release.
  public static bool Blocks(Vector2 position){var es=EventSystem.current;if(!es)return false;hits.Clear();es.RaycastAll(new PointerEventData(es){position=position},hits);bool found=false;foreach(var h in hits){if(h.module is GraphicRaycaster&&h.gameObject.GetComponentInParent<RoyalHUD>()!=null){found=true;break;}}hits.Clear();return found;}
 }
}
