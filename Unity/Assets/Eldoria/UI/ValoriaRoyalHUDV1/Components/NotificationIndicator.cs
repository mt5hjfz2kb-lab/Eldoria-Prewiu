using UnityEngine;
namespace Eldoria.UI.RoyalGold {public sealed class NotificationIndicator:MonoBehaviour {public RoyalText Count;public void Render(int count){gameObject.SetActive(count>0);Count.Put(count>99?"99+":count.ToString());}}}
