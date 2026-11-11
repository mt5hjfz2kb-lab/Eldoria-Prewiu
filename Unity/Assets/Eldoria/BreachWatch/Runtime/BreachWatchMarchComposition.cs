using System.Collections.Generic;
using Eldoria.UI.RoyalGold;
using Eldoria.UI.RoyalGold.V2;
using UnityEngine;
using UnityEngine.UI;

namespace Eldoria.BreachWatch
{
    /// <summary>
    /// Intentional full-width real army scorecard in the approved Vigilia
    /// visual language. The old march screen had an EMPTY dark-blue scenic
    /// preview occupying 30% of the phone, while six real statistics
    /// squeezed into the remaining space. Remove the empty preview entirely
    /// and give every genuine army metric the available gilded stone surface.
    /// No battle formulas or troop state are reimplemented.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class BreachWatchMarchComposition : MonoBehaviour
    {
        readonly Dictionary<RectTransform, Vector4> originals =
            new Dictionary<RectTransform, Vector4>();
        RoyalGoldV3ManagementScreens manager;
        RectTransform right,preview,titleCard,summary,status,statusCard;
        readonly RectTransform[] cards=new RectTransform[6];
        readonly RectTransform[] values=new RectTransform[6];
        bool originalPreviewActive;
        bool initializedPreview;
        float lastW=-1,lastH=-1;
        public bool UsesFullWidthMetrics { get; private set; }

        static RectTransform Find(RectTransform parent,string name)
        {
            if(!parent)return null;
            foreach(var child in parent.GetComponentsInChildren<RectTransform>(true))
                if(child.name==name)return child;
            return null;
        }
        static void At(RectTransform rt,float x,float y,float width,float height)
        {
            rt.anchorMin=rt.anchorMax=rt.pivot=new Vector2(0f,1f);
            rt.anchoredPosition=new Vector2(x,-y);
            rt.sizeDelta=new Vector2(Mathf.Max(1f,width),Mathf.Max(1f,height));
        }
        void Remember(RectTransform rt)
        {
            if(rt && !originals.ContainsKey(rt))
            {
                var p=rt.anchoredPosition;var z=rt.sizeDelta;
                originals.Add(rt,new Vector4(p.x,p.y,z.x,z.y));
            }
        }

        public void Initialize(RoyalGoldV3ManagementScreens source)
        {
            manager=source;
        }

        public bool RefreshScorecard()
        {
            if(!manager || !manager.IsOpen ||
               manager.CurrentDestination!=RoyalGoldV3ManagementScreens.Destination.March)
            {
                UsesFullWidthMetrics=false;return false;
            }
            var root=manager.VisibleRoot;
            if(!right || !right.IsChildOf(root))
            {
                right=Find(root,"V3 / Authoritative status and cost");
                preview=Find(right,"March / reference scene inset");
                titleCard=Find(right,"March / statistics title cartouche");
                summary=Find(right,"Training or expedition breakdown");
                status=Find(right,"Available and pending troops");
                statusCard=Find(right,"March / status parchment cartouche");
                for(int i=0;i<6;i++)
                {
                    cards[i]=Find(right,"March / real metric card "+i);
                    values[i]=Find(right,"March / live metric "+i);
                }
            }
            if(!right||!preview||!titleCard||!summary||!status||!statusCard)
                return false;
            foreach(var c in cards)if(!c)return false;
            foreach(var v in values)if(!v)return false;

            // Preserve the canonical authored landscape arrangement until
            // its own A/B pass. The blank inset is a portrait-only defect.
            if(root.rect.width>root.rect.height)
            {
                UsesFullWidthMetrics=false;return false;
            }
            float w=right.rect.width,h=right.rect.height;
            if(w<210||h<180)return false;
            if(UsesFullWidthMetrics && Mathf.Abs(lastW-w)<.5f &&
               Mathf.Abs(lastH-h)<.5f &&
               !preview.gameObject.activeSelf &&
               Mathf.Abs(summary.rect.width-(w-28f))<1f)
                return true;

            if(!initializedPreview)
            {
                originalPreviewActive=preview.gameObject.activeSelf;
                initializedPreview=true;
            }
            foreach(var rt in new[]{titleCard,summary,status,statusCard})
                Remember(rt);
            for(int i=0;i<6;i++)
            {Remember(cards[i]);Remember(values[i]);}
            preview.gameObject.SetActive(false);

            float margin=12f;
            float captionH=Mathf.Clamp(h*.11f,40f,52f);
            float bottomH=Mathf.Clamp(h*.29f,86f,150f);
            float tileTop=captionH+5f;
            float gap=6f;
            float tileH=Mathf.Max(30f,(h-tileTop-bottomH-12f-2f*gap)/3f);
            float tileW=(w-2f*margin-gap)*.5f;
            At(titleCard,margin-7f,3f,w-2f*margin+14f,captionH);
            At(summary,margin,7f,w-2f*margin,captionH-8f);
            var title=summary.GetComponent<Text>();
            if(title)title.fontSize=22;

            for(int i=0;i<6;i++)
            {
                float x=margin+(i%2)*(tileW+gap);
                float y=tileTop+(i/2)*(tileH+gap);
                At(cards[i],x,y,tileW,tileH);
                At(values[i],7f,3f,tileW-14f,tileH-6f);
                var text=values[i].GetComponent<Text>();
                if(text)
                {
                    text.fontSize=23;
                    text.color=new Color(.96f,.91f,.79f,1f);
                    text.alignment=TextAnchor.MiddleLeft;
                }
            }
            float statusY=tileTop+3f*tileH+2f*gap+7f;
            At(statusCard,margin-6f,statusY-3f,w-2f*margin+12f,
                Mathf.Max(30f,h-statusY));
            At(status,margin,statusY,w-2f*margin,
                Mathf.Max(28f,h-statusY-8f));
            var statusText=status.GetComponent<Text>();
            if(statusText){statusText.fontSize=19;statusText.color=
                new Color(.90f,.86f,.78f,1f);}
            lastW=w;lastH=h;UsesFullWidthMetrics=true;
            return true;
        }

        public void Restore()
        {
            foreach(var pair in originals)
            {
                var rt=pair.Key; if(!rt)continue;
                rt.anchoredPosition=new Vector2(pair.Value.x,pair.Value.y);
                rt.sizeDelta=new Vector2(pair.Value.z,pair.Value.w);
            }
            originals.Clear();
            if(preview && initializedPreview)
                preview.gameObject.SetActive(originalPreviewActive);
            initializedPreview=false;right=preview=titleCard=summary=status=statusCard=null;
            for(int i=0;i<6;i++){cards[i]=null;values[i]=null;}
            UsesFullWidthMetrics=false;lastW=lastH=-1f;
        }
        void OnDestroy(){Restore();}
    }
}
