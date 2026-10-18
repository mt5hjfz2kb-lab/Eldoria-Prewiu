using UnityEngine;
using UnityEngine.EventSystems;
namespace Eldoria.UI.RoyalGold {
 public sealed class ModalWindow:MonoBehaviour {
  public RoyalText Title,Body;public RoyalButton Close,Extra;public RoyalText ExtraLabel;GameObject previous;
  void OnEnable(){Close.onClick.AddListener(Hide);}
  void OnDisable(){Close.onClick.RemoveListener(Hide);}
  public void Show(string title,string body){previous=EventSystem.current?EventSystem.current.currentSelectedGameObject:null;gameObject.SetActive(true);Title.Put(title);Body.Put(body);if(EventSystem.current)EventSystem.current.SetSelectedGameObject(Close.gameObject);}
  public void Hide(){gameObject.SetActive(false);if(EventSystem.current&&previous&&previous.activeInHierarchy)EventSystem.current.SetSelectedGameObject(previous);}
 }
}
