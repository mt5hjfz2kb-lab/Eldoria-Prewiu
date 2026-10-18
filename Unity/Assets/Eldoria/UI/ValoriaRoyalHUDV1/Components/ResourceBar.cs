using UnityEngine;

namespace Eldoria.UI.RoyalGold
{
    public sealed class ResourceBar : MonoBehaviour
    {
        public RoyalText Wood, Stone, Food;
        private Transform foodIcon, foodName;
        private RoyalText woodName, stoneName;
        private bool resolved;
        public bool FoodVisible => Food && Food.gameObject.activeSelf;

        public void Render(RoyalSnapshot s, bool en)
        {
            if (!resolved)
            {
                // The shipped prefab keeps resource decorations as siblings of their numeric text.
                foodIcon = transform.Find("Comida icon"); foodName = transform.Find("Comida name");
                woodName = transform.Find("Madera name")?.GetComponent<RoyalText>();
                stoneName = transform.Find("Piedra name")?.GetComponent<RoyalText>();
                resolved = true;
            }
            Wood.Put(RoyalFormatting.Amount(s.Wood, en)); Stone.Put(RoyalFormatting.Amount(s.Stone, en));
            if (woodName) woodName.Put(RoyalFormatting.L(en, "Madera", "Wood"));
            if (stoneName) stoneName.Put(RoyalFormatting.L(en, "Piedra", "Stone"));
            // Visibility is an explicit presentation capability. Wallets and old saves remain untouched.
            if (Food) { Food.Put(RoyalFormatting.Amount(s.Food, en)); Food.gameObject.SetActive(s.ShowFood); }
            if (foodIcon) foodIcon.gameObject.SetActive(s.ShowFood);
            if (foodName) foodName.gameObject.SetActive(s.ShowFood);
            ((RectTransform)transform).SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, s.ShowFood ? 570 : 386);
        }
    }
}
