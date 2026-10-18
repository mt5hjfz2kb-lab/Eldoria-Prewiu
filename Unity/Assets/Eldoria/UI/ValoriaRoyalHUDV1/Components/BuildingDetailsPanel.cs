using UnityEngine;
namespace Eldoria.UI.RoyalGold {
 public sealed class BuildingDetailsPanel:MonoBehaviour {
  public RoyalText Title,Level,Function,Status,Costs,Requirements,Time;public PrimaryActionButton Primary;public RoyalButton Close;public RoyalAction Offer{get;private set;}
  public void Render(RoyalBuilding b,bool english,bool pending){gameObject.SetActive(b!=null);Offer=b?.Action;if(b==null)return;Title.Put(b.Name);Level.Put(b.Id=="march-main"?"":RoyalFormatting.L(english,"NIVEL ","LEVEL ")+b.Level);Function.Put(b.Function);Status.Put(b.State);Costs.Put(b.Costs);Requirements.Put(b.Requirements);Time.Put(b.Time);Primary.Render(Offer,pending);}
 }
}
