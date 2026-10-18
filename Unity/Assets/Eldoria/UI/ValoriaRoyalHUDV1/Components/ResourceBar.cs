using UnityEngine;
namespace Eldoria.UI.RoyalGold {public sealed class ResourceBar:MonoBehaviour {public RoyalText Wood,Stone,Food;public void Render(RoyalSnapshot s,bool en){Wood.Put(RoyalFormatting.Amount(s.Wood,en));Stone.Put(RoyalFormatting.Amount(s.Stone,en));Food.Put(RoyalFormatting.Amount(s.Food,en));}}}
