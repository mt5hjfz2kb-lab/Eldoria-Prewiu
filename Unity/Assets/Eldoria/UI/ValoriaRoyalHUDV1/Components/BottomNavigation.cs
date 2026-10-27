using System;
using UnityEngine;
namespace Eldoria.UI.RoyalGold {
 public sealed class BottomNavigation:MonoBehaviour {
  [Serializable] public sealed class Item{public RoyalSection Section;public RoyalButton Button;public RoyalText Label;}
  public Item[] Items=Array.Empty<Item>();public event Action<RoyalSection> Requested;UnityEngine.Events.UnityAction[] handlers;
  void OnEnable(){handlers=new UnityEngine.Events.UnityAction[Items.Length];for(int i=0;i<Items.Length;i++){var item=Items[i];handlers[i]=()=>Requested?.Invoke(item.Section);item.Button.onClick.AddListener(handlers[i]);}}
  void OnDisable(){if(handlers==null)return;for(int i=0;i<Items.Length&&i<handlers.Length;i++)Items[i].Button.onClick.RemoveListener(handlers[i]);}
  public void Layout(bool english,IRoyalNavigation host){
   int count=0;foreach(var i in Items){bool show=host!=null?host.CanOpen(i.Section):i.Section!=RoyalSection.Inventory&&i.Section!=RoyalSection.World;i.Button.gameObject.SetActive(show);if(show)count++;}
   // Scale existing, authorized actions within the existing safe-bottom bar.
   // The V2 skin applies circular sizes in LateUpdate; this base layout must
   // never temporarily send real buttons outside the portrait safe area.
   var root=(RectTransform)transform;
   float gap=count>4?4f:9f;
   float width=Mathf.Max(220f,root.rect.width);
   float w=count>0?Mathf.Min(140f,Mathf.Max(48f,(width-gap*Mathf.Max(0,count-1))/count)):140f;
   float total=count*w+Mathf.Max(0,count-1)*gap;
   int n=0;
   foreach(var i in Items){i.Label.Put(Name(i.Section,english));if(!i.Button.gameObject.activeSelf)continue;var rt=(RectTransform)i.Button.transform;rt.anchoredPosition=new Vector2(-total/2+w/2+n++*(w+gap),0);}
  }
  public static string Name(RoyalSection s,bool en){switch(s){case RoyalSection.Heroes:return en?"Heroes":"Héroes";case RoyalSection.Army:return en?"Army":"Ejército";case RoyalSection.Missions:return en?"Missions":"Misiones";case RoyalSection.Inventory:return en?"Inventory":"Inventario";case RoyalSection.World:return en?"World":"Mundo";case RoyalSection.Bastion:return en?"Bastion":"Bastión";case RoyalSection.Settings:return en?"Interface":"Interfaz";default:return "Valoria";}}
 }
}
