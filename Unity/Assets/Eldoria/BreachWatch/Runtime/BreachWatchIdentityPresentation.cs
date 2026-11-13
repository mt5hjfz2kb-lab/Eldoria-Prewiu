using UnityEngine;
using UnityEngine.UI;

namespace Eldoria.BreachWatch
{
    // Presentation only: the original InputField, explicit confirmation and
    // canonical persistence remain the sole authoritative entry flow.
    [DisallowMultipleComponent]
    public sealed class BreachWatchIdentityPresentation : MonoBehaviour
    {
        public bool Installed { get; private set; }
        static RectTransform Node(Transform parent,string name,Vector2 min,Vector2 max)
        {
            var go=new GameObject(name,typeof(RectTransform));go.layer=parent.gameObject.layer;
            var r=(RectTransform)go.transform;r.SetParent(parent,false);
            r.anchorMin=min;r.anchorMax=max;r.offsetMin=r.offsetMax=Vector2.zero;return r;
        }
        static void Rule(Transform parent,string name,Vector2 min,Vector2 max,Color color)
        {
            var image=Node(parent,name,min,max).gameObject.AddComponent<Image>();
            image.color=color;image.raycastTarget=false;
        }
        public bool Install(RectTransform root)
        {
            if(Installed)return true;
            var panel=root.Find("Aged parchment") as RectTransform;
            if(!panel)return false;
            var title=panel.Find("Opening question") as RectTransform;
            var question=panel.Find("Valoria's challenge") as RectTransform;
            var input=panel.Find("Player's chosen name") as RectTransform;
            var confirm=panel.Find("Confirm your name") as RectTransform;
            var crest=Resources.Load<Sprite>("VigiliaArt/OwnerReference/lion_crest");
            if(!title || !question || !input || !confirm || !crest)return false;
            root.GetComponent<Image>().color=new Color(.014f,.012f,.018f,.82f);
            var frame=Node(panel,"Vigilia / identity engraved bronze",Vector2.zero,Vector2.one);
            frame.gameObject.AddComponent<BreachWatchFrameEngraving>();
            var emblem=Node(panel,"Vigilia / identity lion seal",new Vector2(.39f,.815f),new Vector2(.61f,.955f));
            var picture=emblem.gameObject.AddComponent<Image>();picture.sprite=crest;
            picture.preserveAspect=true;picture.raycastTarget=false;
            title.anchorMin=new Vector2(.09f,.70f);title.anchorMax=new Vector2(.91f,.80f);
            var titleInk=title.GetComponent<Text>();titleInk.fontSize=29;titleInk.resizeTextMaxSize=29;
            titleInk.color=new Color(.98f,.84f,.58f);
            question.anchorMin=new Vector2(.12f,.49f);question.anchorMax=new Vector2(.88f,.67f);
            var questionInk=question.GetComponent<Text>();questionInk.fontSize=22;questionInk.resizeTextMaxSize=22;
            questionInk.color=new Color(.94f,.90f,.81f);
            var bronze=new Color(.65f,.43f,.23f,.85f);
            Rule(panel,"Vigilia / identity title rule",new Vector2(.20f,.682f),new Vector2(.80f,.685f),bronze);
            // Keep the same input and confirmation centres used by touch QA.
            input.GetComponent<Image>().color=new Color(.37f,.33f,.29f,1f);
            Rule(input,"Vigilia / input upper bevel",new Vector2(0,.98f),Vector2.one,bronze);
            Rule(input,"Vigilia / input lower bevel",Vector2.zero,new Vector2(1,.025f),new Color(.24f,.15f,.08f));
            var placeholder=input.Find("Placeholder").GetComponent<Text>();
            placeholder.color=new Color(.77f,.72f,.62f,.95f);
            input.GetComponent<InputField>().caretColor=new Color(1f,.87f,.59f);
            input.GetComponent<InputField>().customCaretColor=true;
            confirm.anchorMin=new Vector2(.18f,.055f);confirm.anchorMax=new Vector2(.82f,.19f);
            Installed=true;return true;
        }
    }
}
