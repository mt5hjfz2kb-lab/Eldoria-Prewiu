using UnityEngine;
using UnityEngine.UI;
namespace Eldoria.UI.RoyalGold {
 public sealed class RoyalText:Text {
  protected override void Awake(){base.Awake();if(font==null)font=Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");raycastTarget=false;supportRichText=false;}
  public void Put(string value){value=value??"";if(text!=value)text=value;}
 }
}
