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
        RectTransform heroRow;
        readonly RectTransform[] heroSlots=new RectTransform[3];
        readonly RectTransform[] heroViewports=new RectTransform[2];
        readonly RectTransform[] heroPictures=new RectTransform[2];
        readonly RectTransform[] heroLabels=new RectTransform[2];
        bool thirdHeroWasActive, thirdHeroCaptured;
        public bool UsesTwoRelevantHeroCards { get; private set; }
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

            // Full-width metric rearrangement is portrait-only. But the
            // third permanently unavailable HERO card is a genuine landscape
            // defect too: eliminate that dead slot at all orientations while
            // preserving the original landscape stats and troop layout.
            if(root.rect.width>root.rect.height)
            {
                RefreshActualHeroCards(root);
                UsesFullWidthMetrics=false;
                return false;
            }
            float w=right.rect.width,h=right.rect.height;
            if(w<210||h<180)return false;
            // Remove a permanently unavailable THIRD slot rather than
            // presenting a giant empty "not in this version" image beside
            // a real hero. Expand the two canonical, clickable existing
            // slots; no new story or formation permission is introduced.
            bool goodHeroComposition=RefreshActualHeroCards(root);
            if(!goodHeroComposition)return false;
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

        static RectTransform FindPortrait(RectTransform hero)
        {
            if(!hero)return null;
            foreach(var i in hero.GetComponentsInChildren<Image>(true))
                if(i && i.transform.parent==hero &&
                   i.name.StartsWith("Approved source /"))
                    return i.rectTransform;
            return null;
        }

        bool RefreshActualHeroCards(RectTransform root)
        {
            if(!root)
            {
                UsesTwoRelevantHeroCards=false;
                return false;
            }
            bool landscape=root.rect.width>root.rect.height;
            if(!heroRow || !heroRow.IsChildOf(root))
            {
                heroRow=Find(root,"V3 / Featured military unit");
                for(int i=0;i<3;i++)
                    heroSlots[i]=Find(root,"Hero slot "+i);
                for(int i=0;i<2;i++)
                {
                    heroViewports[i]=Find(heroSlots[i],"Hero portrait viewport");
                    heroPictures[i]=FindPortrait(heroViewports[i]);
                    heroLabels[i]=Find(heroSlots[i],"Button label");
                }
            }
            if(!heroRow || heroRow.rect.width<280f ||
               !heroSlots[0] || !heroSlots[1] || !heroSlots[2] ||
               !heroViewports[0] || !heroViewports[1] ||
               !heroPictures[0] || !heroPictures[1] ||
               !heroLabels[0] || !heroLabels[1])
                return false;
            var third=heroSlots[2].GetComponent<Button>();
            // Future software can legitimately enable a third member;
            // never hide an actually interactable future hero.
            if(third && third.interactable)return false;
            if(!thirdHeroCaptured)
            {
                thirdHeroWasActive=heroSlots[2].gameObject.activeSelf;
                thirdHeroCaptured=true;
            }
            heroSlots[2].gameObject.SetActive(false);

            float space=heroRow.rect.width;
            float left=landscape?10f:13f, gap=landscape?8f:12f;
            float cardW=(space-2f*left-gap)*.5f;
            float top=-heroSlots[0].anchoredPosition.y;
            // Original stage owns the vertical composition. Keep its actual
            // height; only the useless third column is reclaimed.
            float cardH=heroSlots[0].rect.height;
            if(top<2f || cardW<122f || cardH<(landscape?78f:95f))
                return false;
            for(int i=0;i<2;i++)
            {
                var card=heroSlots[i];
                Remember(card);
                Remember(heroViewports[i]);
                Remember(heroPictures[i]);
                Remember(heroLabels[i]);
                At(card,left+i*(cardW+gap),top,cardW,cardH);
                // A pair of enlarged VERTICAL picture cards simply
                // stretched the old 3-slot composition and left 45% blank
                // burgundy at 900x1600. The real leader portrait and real
                // selection action belong in separate side-by-side wells.
                // Use full-height horizontal scorecards on large phones;
                // narrow physical phones retain the compact portrait flow.
                // A real 844x390 phone has only ~90u card height.
                // Horizontal hero artwork+label fits there, whereas three
                // tiny cards produced a fake "No disponible" action.
                bool horizontalCard=landscape
                    ?cardW>=155f && cardH>=78f
                    :cardW>=275f && cardH>=160f;
                float apertureW=horizontalCard
                    ?Mathf.Min(cardW*(landscape?.33f:.43f),
                        cardH*(landscape?.78f:.67f)):cardW-18f;
                float portraitH=horizontalCard
                    ?cardH-(landscape?10f:18f)
                    :Mathf.Max(52f,cardH*.67f);
                At(heroViewports[i],landscape?5f:9f,
                    horizontalCard?(landscape?5f:9f):6f,
                    apertureW,portraitH);
                var image=heroPictures[i].GetComponent<Image>();
                float ratio=image && image.sprite
                    ?image.sprite.rect.width/Mathf.Max(1f,image.sprite.rect.height)
                    :.82f;
                float faceW=Mathf.Min(apertureW-8f,(portraitH-8f)*ratio);
                float faceH=faceW/Mathf.Max(.1f,ratio);
                At(heroPictures[i],(apertureW-faceW)*.5f,
                    (portraitH-faceH)*.5f,faceW,faceH);
                if(horizontalCard)
                    At(heroLabels[i],apertureW+(landscape?11f:19f),
                        landscape?5f:12f,
                        cardW-apertureW-(landscape?17f:29f),
                        cardH-(landscape?10f:24f));
                else
                    At(heroLabels[i],9f,portraitH+8f,cardW-18f,
                        Mathf.Max(28f,cardH-portraitH-14f));
                var label=heroLabels[i].GetComponent<Text>();
                if(label)
                {
                    label.fontSize=Mathf.RoundToInt(
                        Mathf.Clamp(cardW*(horizontalCard?.068f:.071f),
                            16f,25f));
                    label.alignment=TextAnchor.MiddleCenter;
                    label.resizeTextForBestFit=true;
                    label.resizeTextMinSize=14;
                    label.resizeTextMaxSize=label.fontSize;
                }
            }
            UsesTwoRelevantHeroCards=true;
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
            if(heroSlots[2] && thirdHeroCaptured)
                heroSlots[2].gameObject.SetActive(thirdHeroWasActive);
            thirdHeroCaptured=false;
            heroRow=null;
            for(int i=0;i<3;i++)heroSlots[i]=null;
            for(int i=0;i<2;i++)
            {heroViewports[i]=null;heroPictures[i]=null;heroLabels[i]=null;}
            UsesTwoRelevantHeroCards=false;
            if(preview && initializedPreview)
                preview.gameObject.SetActive(originalPreviewActive);
            initializedPreview=false;right=preview=titleCard=summary=status=statusCard=null;
            for(int i=0;i<6;i++){cards[i]=null;values[i]=null;}
            UsesFullWidthMetrics=false;lastW=lastH=-1f;
        }
        void OnDestroy(){Restore();}
    }
}
