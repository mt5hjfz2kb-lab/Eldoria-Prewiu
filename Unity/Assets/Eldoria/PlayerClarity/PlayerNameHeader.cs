using System;
using Eldoria.UI.RoyalGold;
using UnityEngine;

namespace Eldoria.PlayerClarity
{
    // The realm and power still belong to the canonical RoyalHUD.
    public sealed class PlayerNameHeader : MonoBehaviour
    {
        private RealmHeader header;
        private Func<string> read;
        private RoyalText name, kingdom;
        private Vector2 oldPower;
        private bool changed;

        public void Initialize(RealmHeader source, Func<string> readName)
        {
            header = source;
            read = readName;
            if (!header || read == null) return;
            if (name) return;
            var root = (RectTransform)header.transform;
            name = Label(root, "Character name", new Vector2(102,-11),22);
            kingdom = Label(root, "Kingdom", new Vector2(102,-42),14);
            kingdom.Put("Valoria");
            Refresh();
        }

        private static RoyalText Label(RectTransform parent,string title,Vector2 position,int size)
        {
            var go=new GameObject(title,typeof(RectTransform),typeof(RoyalText));
            var rect=(RectTransform)go.transform;
            rect.SetParent(parent,false);
            rect.anchorMin=rect.anchorMax=rect.pivot=new Vector2(0,1);
            rect.anchoredPosition=position;
            rect.sizeDelta=new Vector2(215,size+8);
            var label=go.GetComponent<RoyalText>();
            label.font=Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            label.fontSize=size;
            label.color=new Color(.95f,.80f,.53f);
            label.alignment=TextAnchor.MiddleLeft;
            label.raycastTarget=false;
            return label;
        }

        public void Refresh()
        {
            if (!header || read == null || !name) return;
            var text=read();
            bool hasName=!string.IsNullOrWhiteSpace(text);
            if (hasName)
            {
                name.Put(text);
                if (!changed)
                {
                    oldPower=header.Power.rectTransform.anchoredPosition;
                    header.Power.rectTransform.anchoredPosition=new Vector2(oldPower.x,-69);
                    changed=true;
                }
            }
            else if (changed)
            {
                header.Power.rectTransform.anchoredPosition=oldPower;
                changed=false;
            }
            header.Realm.gameObject.SetActive(!hasName);
            name.gameObject.SetActive(hasName);
            kingdom.gameObject.SetActive(hasName);
        }

        private void LateUpdate() => Refresh();

        private void OnDestroy()
        {
            if (!header) return;
            if (header.Realm) header.Realm.gameObject.SetActive(true);
            if (changed && header.Power) header.Power.rectTransform.anchoredPosition=oldPower;
        }
    }
}
