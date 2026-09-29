using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Eldoria.Presentation
{
    /// <summary>
    /// Player-facing art skin for the approved mobile HUD reference.
    /// It deliberately decorates the existing functional HUD instead of replacing it,
    /// so gameplay wiring, safe-area budgets and owner-playtest interactions stay intact.
    /// </summary>
    public sealed class ReferenceUiArtPass : MonoBehaviour
    {
        static readonly Color Panel = new Color(.045f,.055f,.060f,.97f);
        static readonly Color PanelDeep = new Color(.018f,.024f,.027f,.98f);
        static readonly Color Bronze = new Color(.56f,.42f,.23f,1f);
        static readonly Color BronzeDark = new Color(.20f,.14f,.075f,1f);
        static readonly Color Gold = new Color(.78f,.63f,.34f,1f);
        static readonly Color GoldSoft = new Color(.93f,.80f,.53f,1f);
        static readonly Color Ink = new Color(.055f,.050f,.038f,1f);
        static readonly Color Disabled = new Color(.20f,.22f,.22f,.92f);

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Boot()
        {
            SceneManager.sceneLoaded -= OnLoaded;
            SceneManager.sceneLoaded += OnLoaded;
            Attach();
        }

        static void OnLoaded(Scene scene, LoadSceneMode mode) => Attach();

        static void Attach()
        {
            if (Object.FindFirstObjectByType<ReferenceUiArtPass>() != null) return;
            var go = new GameObject("Reference UI art pass", typeof(ReferenceUiArtPass));
            DontDestroyOnLoad(go);
        }

        IEnumerator Start()
        {
            while (true)
            {
                yield return null;
                DecorateCurrentHud();
                yield return new WaitForSecondsRealtime(.5f);
            }
        }

        static void DecorateCurrentHud()
        {
            var canvas = GameObject.Find("Eldoria HUD");
            if (canvas == null) return;

            Frame(GameObject.Find("Reference topbar"), false, 2f);
            Frame(GameObject.Find("Quest panel"), true, 2f);
            Frame(GameObject.Find("World objective dock"), true, 2f);
            Frame(GameObject.Find("Bottom navigation"), false, 2f);
            Frame(GameObject.Find("Building interaction panel"), true, 3f);

            var buttons = canvas.GetComponentsInChildren<Button>(true);
            foreach (var button in buttons) StyleButton(button);

            var texts = canvas.GetComponentsInChildren<Text>(true);
            foreach (var text in texts) StyleText(text);
        }

        static void Frame(GameObject go, bool corners, float edge)
        {
            if (go == null || go.transform.Find("ReferenceArtFrame") != null) return;
            var image = go.GetComponent<Image>();
            if (image != null)
            {
                image.color = Panel;
                var shadow = image.GetComponent<Shadow>() ?? image.gameObject.AddComponent<Shadow>();
                shadow.effectColor = new Color(0f, 0f, 0f, .70f);
                shadow.effectDistance = new Vector2(0f, -3f);
                shadow.useGraphicAlpha = true;
            }

            var frame = new GameObject("ReferenceArtFrame", typeof(RectTransform));
            frame.transform.SetParent(go.transform, false);
            frame.transform.SetAsFirstSibling();
            var rt = frame.GetComponent<RectTransform>();
            Stretch(rt, 0f);
            var ignore = frame.AddComponent<LayoutElement>();
            ignore.ignoreLayout = true;

            Edge(frame.transform, "Top bronze", new Vector2(0, 1), new Vector2(1, 1), new Vector2(0, -edge), new Vector2(0, 0), Bronze);
            Edge(frame.transform, "Bottom bronze", new Vector2(0, 0), new Vector2(1, 0), new Vector2(0, 0), new Vector2(0, edge), BronzeDark);
            Edge(frame.transform, "Left bronze", new Vector2(0, 0), new Vector2(0, 1), new Vector2(0, 0), new Vector2(edge, 0), Bronze);
            Edge(frame.transform, "Right bronze", new Vector2(1, 0), new Vector2(1, 1), new Vector2(-edge, 0), new Vector2(0, 0), BronzeDark);

            Edge(frame.transform, "Top inner", new Vector2(0, 1), new Vector2(1, 1), new Vector2(5, -5), new Vector2(-5, -3.5f), new Color(.84f,.68f,.38f,.45f));
            Edge(frame.transform, "Bottom inner", new Vector2(0, 0), new Vector2(1, 0), new Vector2(5, 3.5f), new Vector2(-5, 5), new Color(.13f,.09f,.045f,.9f));

            if (corners)
            {
                Corner(frame.transform, "TL", new Vector2(0,1), new Vector2(7,-7));
                Corner(frame.transform, "TR", new Vector2(1,1), new Vector2(-7,-7));
                Corner(frame.transform, "BL", new Vector2(0,0), new Vector2(7,7));
                Corner(frame.transform, "BR", new Vector2(1,0), new Vector2(-7,7));
            }
        }

        static void StyleButton(Button button)
        {
            if (button == null) return;
            var image = button.GetComponent<Image>();
            if (image == null) return;
            bool primary = button.gameObject.name == "CONTINUAR" ||
                           button.gameObject.name == "Building action";
            bool nav = button.transform.parent != null && button.transform.parent.name == "Bottom navigation";

            var cb = button.colors;
            if (!button.interactable)
            {
                image.color = nav ? new Color(.03f,.04f,.045f,.18f) : Disabled;
            }
            else if (primary)
            {
                image.color = Gold;
                cb.normalColor = Gold;
                cb.highlightedColor = GoldSoft;
                cb.pressedColor = new Color(.53f,.39f,.19f,1f);
                cb.selectedColor = GoldSoft;
            }
            else if (!nav)
            {
                image.color = PanelDeep;
                cb.normalColor = PanelDeep;
                cb.highlightedColor = new Color(.13f,.12f,.09f,.98f);
                cb.pressedColor = new Color(.08f,.065f,.045f,.98f);
            }
            button.colors = cb;

            if (button.transform.Find("ReferenceButtonFrame") == null)
            {
                var f = new GameObject("ReferenceButtonFrame", typeof(RectTransform));
                f.transform.SetParent(button.transform, false);
                f.transform.SetAsFirstSibling();
                var rt = f.GetComponent<RectTransform>();
                Stretch(rt, 0f);
                var ignore = f.AddComponent<LayoutElement>(); ignore.ignoreLayout = true;
                var outline = image.GetComponent<Outline>() ?? image.gameObject.AddComponent<Outline>();
                outline.effectColor = primary ? BronzeDark : new Color(.42f,.31f,.17f,.92f);
                outline.effectDistance = new Vector2(1.5f,-1.5f);
                outline.useGraphicAlpha = true;
                Edge(f.transform,"Button top",new Vector2(0,1),new Vector2(1,1),new Vector2(3,-3),new Vector2(-3,-1.5f),primary?GoldSoft:new Color(.58f,.44f,.24f,.75f));
                Edge(f.transform,"Button bottom",new Vector2(0,0),new Vector2(1,0),new Vector2(3,1.5f),new Vector2(-3,3),new Color(.09f,.055f,.025f,.95f));
                if(primary)
                {
                    Cap(f.transform,"Left cap",new Vector2(0,.5f),new Vector2(5,0));
                    Cap(f.transform,"Right cap",new Vector2(1,.5f),new Vector2(-5,0));
                }
            }
        }

        static void StyleText(Text text)
        {
            if (text == null || text.transform.parent == null) return;
            bool primary = text.transform.parent.name == "CONTINUAR" || text.transform.parent.name == "Building action";
            if (primary)
            {
                text.color = Ink;
                text.fontStyle = FontStyle.Bold;
            }
            else if (text.name == "Quest kicker")
            {
                text.color = GoldSoft;
                text.fontStyle = FontStyle.Bold;
            }
            else if (text.name == "Building title")
            {
                text.color = GoldSoft;
                text.fontStyle = FontStyle.Bold;
            }
        }

        static void Edge(Transform parent,string name,Vector2 amin,Vector2 amax,Vector2 omin,Vector2 omax,Color color)
        {
            var go=new GameObject(name,typeof(RectTransform),typeof(Image));
            go.transform.SetParent(parent,false);
            var rt=go.GetComponent<RectTransform>();rt.anchorMin=amin;rt.anchorMax=amax;rt.offsetMin=omin;rt.offsetMax=omax;
            var img=go.GetComponent<Image>();img.color=color;img.raycastTarget=false;
        }

        static void Corner(Transform parent,string name,Vector2 anchor,Vector2 pos)
        {
            var go=new GameObject("Corner "+name,typeof(RectTransform),typeof(Image));
            go.transform.SetParent(parent,false);
            var rt=go.GetComponent<RectTransform>();rt.anchorMin=rt.anchorMax=anchor;rt.sizeDelta=new Vector2(9,9);rt.anchoredPosition=pos;rt.localRotation=Quaternion.Euler(0,0,45);
            var img=go.GetComponent<Image>();img.color=Gold;img.raycastTarget=false;
            var inner=new GameObject("Inset",typeof(RectTransform),typeof(Image));inner.transform.SetParent(go.transform,false);
            var ir=inner.GetComponent<RectTransform>();ir.anchorMin=Vector2.zero;ir.anchorMax=Vector2.one;ir.offsetMin=new Vector2(2,2);ir.offsetMax=new Vector2(-2,-2);
            inner.GetComponent<Image>().color=PanelDeep;inner.GetComponent<Image>().raycastTarget=false;
        }

        static void Cap(Transform parent,string name,Vector2 anchor,Vector2 pos)
        {
            var go=new GameObject(name,typeof(RectTransform),typeof(Image));go.transform.SetParent(parent,false);
            var rt=go.GetComponent<RectTransform>();rt.anchorMin=rt.anchorMax=anchor;rt.sizeDelta=new Vector2(7,22);rt.anchoredPosition=pos;
            var img=go.GetComponent<Image>();img.color=Bronze;img.raycastTarget=false;
        }

        static void Stretch(RectTransform rt,float inset)
        {
            rt.anchorMin=Vector2.zero;rt.anchorMax=Vector2.one;rt.offsetMin=new Vector2(inset,inset);rt.offsetMax=new Vector2(-inset,-inset);
        }
    }
}
