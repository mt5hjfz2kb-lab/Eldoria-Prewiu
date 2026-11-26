using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

namespace Eldoria.DailyTreasures
{
    /// <summary>Presentation-only shared Corona assets. No domain commands.
    /// This assembly is already referenced by RoyalGold.V2, so daily rewards
    /// and RoyalGold can consume one resource catalogue without a cycle.</summary>
    public static class CoronaPresentation
    {
        public const string ArtRoot="ValoriaModular25D/UI/CoronaBrecha/";
        public const string FontRoot="ValoriaModular25D/UI/Fonts/";
        public static readonly Color32 Ivory=new Color32(250,239,213,255);
        public static readonly Color32 Muted=new Color32(201,205,192,255);
        public static readonly Color32 Brass=new Color32(192,159,106,255);
        // Ornamental brass is not a small-text colour.
        public static readonly Color32 StatusGold=new Color32(234,207,151,255);
        public static readonly Color32 InProgress=new Color32(210,231,238,255);
        public static bool Active =>
            SceneUtility.GetBuildIndexByScenePath("Assets/Experimentos/ValoriaModular25D/Scenes/BastionI_Modular25D.unity")==0 &&
            SceneUtility.GetBuildIndexByScenePath("Assets/Eldoria/ArtTests/ValoriaReferenceClean/Integration/Valoria_City_World_UI.unity")==1;
        static Font heading,body;
        static Sprite wood,stone,food;
        public static Font Heading => heading?heading:(heading=Resources.Load<Font>(FontRoot+"CormorantGaramond-SemiBold"));
        public static Font Body => body?body:(body=Resources.Load<Font>(FontRoot+"SourceSans3-Semibold"));
        public static Font ResolveFont(bool title,Font fallback=null)
        {
            Font selected=title?Heading:Body;
            return selected?selected:fallback?fallback:Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        }
        // Loose resources use the same silhouettes everywhere; actual saved
        // resource packages keep their original packet art and real amounts.
        public static Sprite ResourceIcon(string kind)
        {
            switch(kind)
            {
                case "wood": return wood?wood:(wood=Resources.Load<Sprite>(ArtRoot+"icon_wood"));
                case "stone": return stone?stone:(stone=Resources.Load<Sprite>(ArtRoot+"icon_stone"));
                case "food": return food?food:(food=Resources.Load<Sprite>(ArtRoot+"icon_food"));
                default: return null;
            }
        }
        // Canonical quantity control: the real slider still owns its range,
        // fill direction, handle anchors, callbacks and input rectangle.
        // Only its presentation is changed; both sprites already ship.
        public static void QuantitySlider(Slider control)
        {
            if(!control || !control.fillRect || !control.handleRect)return;
            var track=control.GetComponent<Image>();
            var fill=control.fillRect.GetComponent<Image>();
            var thumb=control.handleRect.GetComponent<Image>();
            var inset=Resources.Load<Sprite>(ArtRoot+"wallet_inset");
            var medallion=Resources.Load<Sprite>(ArtRoot+"nav_medallion");
            if(track && inset)
            {
                track.sprite=inset;
                track.type=inset.border.sqrMagnitude>0?Image.Type.Sliced:Image.Type.Simple;
                track.color=Color.white;
            }
            if(fill)fill.color=new Color(.24f,.76f,.72f,1f);
            if(thumb && medallion)
            {
                thumb.sprite=medallion;
                thumb.type=Image.Type.Simple;
                thumb.preserveAspect=true;
                thumb.color=Color.white;
                // Allow the thumb to remain visible even on the compact
                // landscape track. Do not resize the root hit rectangle.
                control.handleRect.sizeDelta=new Vector2(28f,4f);
            }
            ButtonFeedback(control);
        }

        public static void ButtonFeedback(Selectable control)
        {
            control.transition=Selectable.Transition.ColorTint;
            var state=control.colors;
            state.normalColor=Color.white;
            state.highlightedColor=Ivory;
            state.selectedColor=new Color(.90f,.96f,.93f);
            state.pressedColor=new Color(.74f,.82f,.77f,1f);
            state.disabledColor=new Color(.55f,.57f,.56f,.7f);
            state.fadeDuration=.09f;
            control.colors=state;
        }
    }
}
