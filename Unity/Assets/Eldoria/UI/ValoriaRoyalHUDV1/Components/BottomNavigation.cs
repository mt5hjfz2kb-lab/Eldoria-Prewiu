using System;
using UnityEngine;
namespace Eldoria.UI.RoyalGold {
 public sealed class BottomNavigation:MonoBehaviour {
  [Serializable] public sealed class Item{public RoyalSection Section;public RoyalButton Button;public RoyalText Label;}
  public Item[] Items=Array.Empty<Item>();public event Action<RoyalSection> Requested;UnityEngine.Events.UnityAction[] handlers;
  // Opt-in for the real integrated Crónica Real HUD only. The historic
  // vertical slice keeps the original progression-driven hide behavior.
  public bool ChronicleLayout;
  void OnEnable(){handlers=new UnityEngine.Events.UnityAction[Items.Length];for(int i=0;i<Items.Length;i++){var item=Items[i];handlers[i]=()=>Requested?.Invoke(item.Section);item.Button.onClick.AddListener(handlers[i]);}}
  void OnDisable(){if(handlers==null)return;for(int i=0;i<Items.Length&&i<handlers.Length;i++)Items[i].Button.onClick.RemoveListener(handlers[i]);}
  public void Layout(bool english,IRoyalNavigation host){
   int count=0;
   foreach(var item in Items){
    bool unlocked=host!=null?host.CanOpen(item.Section):
      item.Section!=RoyalSection.Inventory&&item.Section!=RoyalSection.World;
    bool realDestination=item.Section!=RoyalSection.Heroes;
    // Three genuine ever-present destinations: Reino, Construir, Crónicas.
    // The Army and World buttons MUST NOT appear before their real gateway
    // abilities have unlocked. Owner's 28/10 progression visibility rule.
    bool utility=ChronicleLayout&&(item.Section==RoyalSection.Bastion||
      item.Section==RoyalSection.Inventory||item.Section==RoyalSection.Missions);
    bool show=ChronicleLayout?realDestination&&(utility||unlocked):unlocked;
    item.Button.gameObject.SetActive(show);
    item.Button.interactable=show&&(unlocked||utility);
    item.Label.Put(ChronicleLayout?ChronicleName(item.Section,english):
      Name(item.Section,english));
    if(show)count++;
   }
   if(ChronicleLayout)return; // The V3 artist owns exact safe-edge reflow.
   int n=0;float w=140,gap=12,total=count*w+Mathf.Max(0,count-1)*gap;
   foreach(var item in Items){
    if(!item.Button.gameObject.activeSelf)continue;
    var rt=(RectTransform)item.Button.transform;
    rt.anchoredPosition=new Vector2(-total/2+w/2+n++*(w+gap),0);
   }
  }
  public static string ChronicleName(RoyalSection s,bool en){
   switch(s){
    case RoyalSection.Bastion:return en?"Realm":"Reino";
    case RoyalSection.Inventory:return en?"Build":"Construir";
    case RoyalSection.Army:return en?"Units":"Unidades";
    case RoyalSection.World:return en?"World":"Mundo";
    case RoyalSection.Missions:return en?"Chronicles":"Crónicas";
    default:return Name(s,en);
   }
  }
  public static int ChronicleOrder(RoyalSection s){
   switch(s){
    case RoyalSection.Bastion:return 0;
    case RoyalSection.Inventory:return 1;
    case RoyalSection.Army:return 2;
    case RoyalSection.World:return 3;
    case RoyalSection.Missions:return 4;
    default:return 99;
   }
  }
  public static string Name(RoyalSection s,bool en){switch(s){case RoyalSection.Heroes:return en?"Heroes":"Héroes";case RoyalSection.Army:return en?"Army":"Ejército";case RoyalSection.Missions:return en?"Missions":"Misiones";case RoyalSection.Inventory:return en?"Inventory":"Inventario";case RoyalSection.World:return en?"World":"Mundo";case RoyalSection.Bastion:return en?"Bastion":"Bastión";case RoyalSection.Settings:return en?"Interface":"Interfaz";default:return "Valoria";}}
 }
}
