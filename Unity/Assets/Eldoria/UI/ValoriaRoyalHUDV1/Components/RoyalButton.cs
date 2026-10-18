using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
namespace Eldoria.UI.RoyalGold {
 public sealed class RoyalButton:Button,IBeginDragHandler,IDragHandler,IEndDragHandler,ICancelHandler {
  bool owned,dragged;int pointer;Vector2 start;const float Threshold=12;
  protected override void Awake(){base.Awake();transition=Transition.ColorTint;var c=colors;c.normalColor=Color.white;c.highlightedColor=new Color(1,.94f,.78f,1);c.pressedColor=new Color(.7f,.73f,.79f,1);c.disabledColor=new Color(.6f,.6f,.6f,1);c.fadeDuration=.10f;colors=c;if(targetGraphic==null)targetGraphic=GetComponent<Graphic>();}
  public override void OnPointerDown(PointerEventData e){if(!IsInteractable())return;if(owned)return;owned=e.button==PointerEventData.InputButton.Left;pointer=e.pointerId;start=e.position;dragged=false;base.OnPointerDown(e);}
  public void OnBeginDrag(PointerEventData e){if(e.pointerId==pointer)dragged=true;}
  public void OnDrag(PointerEventData e){if(e.pointerId==pointer&&(e.position-start).sqrMagnitude>Threshold*Threshold)dragged=true;}
  public void OnEndDrag(PointerEventData e){if(e.pointerId==pointer)dragged=true;}
  public override void OnPointerClick(PointerEventData e){bool valid=owned&&!dragged&&e.pointerId==pointer&&(e.position-start).sqrMagnitude<=Threshold*Threshold;owned=false;if(valid)base.OnPointerClick(e);}
  public override void OnPointerExit(PointerEventData e){if(e.pointerId==pointer)dragged=true;base.OnPointerExit(e);}
  public void OnCancel(BaseEventData e){owned=false;dragged=true;}
  protected override void OnDisable(){owned=false;dragged=false;base.OnDisable();}
  void OnApplicationFocus(bool focus){if(!focus){owned=false;dragged=true;}}
 }
}
