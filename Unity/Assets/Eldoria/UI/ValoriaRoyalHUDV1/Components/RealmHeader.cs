using UnityEngine;
namespace Eldoria.UI.RoyalGold {public sealed class RealmHeader:MonoBehaviour {public RoyalText Realm,Power;public RoyalButton Info;public void Render(RoyalSnapshot s,bool en){Realm.Put(s.Realm);Power.Put(RoyalFormatting.L(en,"Poder · ","Power · ")+RoyalFormatting.Amount(s.Power,en));}}}
