using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
namespace Eldoria.UI.RoyalGold {
 public sealed class RoyalButton:Button,IBeginDragHandler,IDragHandler,IEndDragHandler,ICancelHandler {
  bool owned,dragged,ready;int pointer;Vector2 start;const float Threshold=12;
  protected override void Awake(){base.Awake();transition=Transition.ColorTint;var c=colors;c.normalColor=Color.white;c.highlightedColor=new Color(1,.94f,.78f,1);c.pressedColor=new Color(.7f,.73f,.79f,1);c.disabledColor=new Color(.6f,.6f,.6f,1);c.fadeDuration=.10f;colors=c;if(targetGraphic==null)targetGraphic=GetComponent<Graphic>();}
  public override void OnPointerDown(PointerEventData e){if(!IsInteractable()||owned)return;owned=e.button==PointerEventData.InputButton.Left;ready=false;pointer=e.pointerId;start=e.position;dragged=false;base.OnPointerDown(e);}
  public override void OnPointerUp(PointerEventData e){if(e.pointerId!=pointer)return;ready=owned&&!dragged&&(e.position-start).sqrMagnitude<=Threshold*Threshold;owned=false;base.OnPointerUp(e);}
  public void OnBeginDrag(PointerEventData e){if(e.pointerId==pointer){dragged=true;ready=false;}}
  public void OnDrag(PointerEventData e){if(e.pointerId==pointer&&(e.position-start).sqrMagnitude>Threshold*Threshold){dragged=true;ready=false;}}
  public void OnEndDrag(PointerEventData e){if(e.pointerId==pointer){dragged=true;ready=false;}}
  public override void OnPointerClick(PointerEventData e){bool valid=ready&&!dragged&&e.pointerId==pointer;ready=false;if(valid)base.OnPointerClick(e);}
  public override void OnPointerExit(PointerEventData e){if(e.pointerId==pointer){dragged=true;ready=false;}base.OnPointerExit(e);}
  public void OnCancel(BaseEventData e){owned=false;ready=false;dragged=true;}
  protected override void OnDisable(){owned=false;ready=false;dragged=false;base.OnDisable();}
  void OnApplicationFocus(bool focus){if(!focus){owned=false;ready=false;dragged=true;}}
 }
}
