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
        struct LabelTypography
        {
            public int Size, Minimum, Maximum;
            public bool BestFit;
            public HorizontalWrapMode Horizontal;
            public VerticalWrapMode Vertical;
            public TextAnchor Alignment;
        }
        readonly Dictionary<Text,LabelTypography> originalLabels =
            new Dictionary<Text,LabelTypography>();
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
        string thirdHeroOriginalLabel;
        Text thirdHeroLabel;
        RectTransform detailSheet;
        Text detailText;
        Button information;
        bool statusWasActive,statusCardWasActive,statusCaptured;
        public Button InformationButton => information;
        public Button InformationReturnButton => detailSheet?detailSheet.GetComponent<Button>():null;
        public bool SecondaryDetailsOpen => detailSheet && detailSheet.gameObject.activeSelf;
        public bool SecondaryDetailsBehindInformation => information && information.gameObject.activeInHierarchy && !status.gameObject.activeSelf;
        public bool UsesThreeHeroSpaces { get; private set; }
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

            // Keep all three canonical hero spaces at every orientation.
            if(root.rect.width>root.rect.height)
            {
                RefreshActualHeroCards(root);
                RefreshSecondaryInformation(root);
                UsesFullWidthMetrics=false;
                return false;
            }
            float w=right.rect.width,h=right.rect.height;
            if(w<210||h<180)return false;
            // A locked third space remains visible without granting formation permission.
            bool goodHeroComposition=RefreshActualHeroCards(root);
            if(!goodHeroComposition)return false;
            RefreshSecondaryInformation(root);
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
            float bottomH=42f;
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
            RefreshSecondaryInformation(root);
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
                UsesThreeHeroSpaces=false;
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
            // Preserve actual third-slot availability; presentation never unlocks it.
            if(!thirdHeroCaptured)
            {
                thirdHeroWasActive=heroSlots[2].gameObject.activeSelf;
                thirdHeroCaptured=true;
            }
            heroSlots[2].gameObject.SetActive(true);
            thirdHeroLabel=Find(heroSlots[2],"Button label")?.GetComponent<Text>();
            if(thirdHeroLabel && thirdHeroOriginalLabel==null)thirdHeroOriginalLabel=thirdHeroLabel.text;
            if(thirdHeroLabel && third && !third.interactable)
                thirdHeroLabel.text="HÉROE 3 · +";

            float space=heroRow.rect.width;
            float left=landscape?10f:13f, gap=landscape?8f:12f;
            float cardW=(space-2f*left-2f*gap)/3f;
            float top=-heroSlots[0].anchoredPosition.y;
            // Retain the actual stage height while reserving three equal hero cells.
            float cardH=heroSlots[0].rect.height;
            if(top<2f || cardW<78f || cardH<(landscape?78f:95f))
                return false;
            Remember(heroSlots[2]);
            At(heroSlots[2],left+2f*(cardW+gap),top,cardW,cardH);
            for(int i=0;i<2;i++)
            {
                var card=heroSlots[i];
                Remember(card);
                Remember(heroViewports[i]);
                Remember(heroPictures[i]);
                Remember(heroLabels[i]);
                At(card,left+i*(cardW+gap),top,cardW,cardH);
                // Compact landscape uses horizontal portraits; portrait keeps names below art.
                bool horizontalCard=landscape &&
                    cardW>=155f && cardH>=78f;
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
                    At(heroLabels[i],9f,portraitH+5f,cardW-18f,
                        Mathf.Max(28f,cardH-portraitH-14f));
                var label=heroLabels[i].GetComponent<Text>();
                if(label)
                {
                    if(!originalLabels.ContainsKey(label))
                        originalLabels.Add(label,new LabelTypography {
                            Size=label.fontSize, Minimum=label.resizeTextMinSize,
                            Maximum=label.resizeTextMaxSize,
                            BestFit=label.resizeTextForBestFit,
                            Horizontal=label.horizontalOverflow,
                            Vertical=label.verticalOverflow,
                            Alignment=label.alignment
                        });
                    label.fontSize=Mathf.RoundToInt(
                        Mathf.Clamp(cardW*(horizontalCard?.068f:.071f),
                            14f,22f));
                    label.alignment=TextAnchor.MiddleCenter;
                    if(landscape)
                    {
                        // Actual 844x390 Unity: source Text uses OVERFLOW and
                        // paints "LÍDER · SIR ALDRIC" right across the second
                        // card, despite correct RectTransform bounds. Wrap
                        // original live labels inside each forged card.
                        label.fontSize=Mathf.Min(label.fontSize,18);
                        label.resizeTextForBestFit=false;
                        label.horizontalOverflow=HorizontalWrapMode.Wrap;
                        label.verticalOverflow=VerticalWrapMode.Truncate;
                    }
                    else
                    {
                        label.resizeTextForBestFit=true;
                        label.resizeTextMinSize=14;
                        label.resizeTextMaxSize=label.fontSize;
                    }
                    label.SetVerticesDirty();
                }
            }
            if(thirdHeroLabel && heroLabels[1])
            {
                var matched=heroLabels[1].GetComponent<Text>();
                if(matched)
                {
                    if(!originalLabels.ContainsKey(thirdHeroLabel))
                        originalLabels.Add(thirdHeroLabel,new LabelTypography {
                            Size=thirdHeroLabel.fontSize,Minimum=thirdHeroLabel.resizeTextMinSize,
                            Maximum=thirdHeroLabel.resizeTextMaxSize,BestFit=thirdHeroLabel.resizeTextForBestFit,
                            Horizontal=thirdHeroLabel.horizontalOverflow,Vertical=thirdHeroLabel.verticalOverflow,
                            Alignment=thirdHeroLabel.alignment
                        });
                    thirdHeroLabel.fontSize=matched.fontSize;
                    thirdHeroLabel.resizeTextForBestFit=matched.resizeTextForBestFit;
                    thirdHeroLabel.resizeTextMinSize=matched.resizeTextMinSize;
                    thirdHeroLabel.resizeTextMaxSize=matched.resizeTextMaxSize;
                    thirdHeroLabel.alignment=TextAnchor.MiddleCenter;
                    thirdHeroLabel.horizontalOverflow=HorizontalWrapMode.Wrap;
                }
            }
            UsesThreeHeroSpaces=true;
            return true;
        }


        void RefreshSecondaryInformation(RectTransform root)
        {
            if(!status || !statusCard)return;
            if(!statusCaptured)
            {
                statusWasActive=status.gameObject.activeSelf;
                statusCardWasActive=statusCard.gameObject.activeSelf;
                statusCaptured=true;
            }
            if(!information)
            {
                var go=new GameObject("March / secondary information",typeof(RectTransform),typeof(Image),typeof(Button));
                go.transform.SetParent(right,false);
                information=go.GetComponent<Button>();
                var face=go.GetComponent<Image>();
                var original=heroSlots[0]?heroSlots[0].GetComponent<Image>():statusCard.GetComponent<Image>();
                if(original){face.sprite=original.sprite;face.type=original.type;}
                face.color=new Color(.55f,.43f,.29f,1f);
                information.targetGraphic=face;
                var labelGo=new GameObject("Information label",typeof(RectTransform),typeof(Text));
                labelGo.transform.SetParent(go.transform,false);
                var label=labelGo.GetComponent<Text>();var source=status.GetComponent<Text>();
                label.font=source.font;label.fontSize=26;label.color=new Color(.96f,.88f,.69f);
                label.alignment=TextAnchor.MiddleCenter;label.text="i  ·  Detalles del ejército";
                label.raycastTarget=false;
                var lr=label.rectTransform;lr.anchorMin=Vector2.zero;lr.anchorMax=Vector2.one;lr.offsetMin=lr.offsetMax=Vector2.zero;

                var sheetGo=new GameObject("March / army information sheet",typeof(RectTransform),typeof(Image),typeof(Button));
                sheetGo.transform.SetParent(root,false);detailSheet=sheetGo.GetComponent<RectTransform>();
                detailSheet.anchorMin=Vector2.zero;detailSheet.anchorMax=Vector2.one;
                detailSheet.offsetMin=detailSheet.offsetMax=Vector2.zero;
                sheetGo.GetComponent<Image>().color=new Color(.025f,.018f,.022f,.97f);
                sheetGo.GetComponent<Button>().onClick.AddListener(()=>detailSheet.gameObject.SetActive(false));
                var textGo=new GameObject("Live army details and return",typeof(RectTransform),typeof(Text));
                textGo.transform.SetParent(detailSheet,false);detailText=textGo.GetComponent<Text>();
                detailText.font=source.font;detailText.fontSize=25;detailText.color=new Color(.96f,.90f,.79f);
                detailText.alignment=TextAnchor.MiddleCenter;detailText.horizontalOverflow=HorizontalWrapMode.Wrap;
                detailText.raycastTarget=false;
                var tr=detailText.rectTransform;tr.anchorMin=new Vector2(.08f,.12f);tr.anchorMax=new Vector2(.92f,.88f);
                tr.offsetMin=tr.offsetMax=Vector2.zero;
                information.onClick.AddListener(()=>{detailText.text="DETALLES DEL EJÉRCITO\n\n"+status.GetComponent<Text>().text+"\n\n‹ Volver a la preparación";detailSheet.SetAsLastSibling();detailSheet.gameObject.SetActive(true);});
                detailSheet.gameObject.SetActive(false);
            }
            information.GetComponentInChildren<Text>().fontSize=26;
            detailText.fontSize=25;
            var rect=(RectTransform)information.transform;
            At(rect,12f,Mathf.Max(0,right.rect.height-39f),Mathf.Max(1,right.rect.width-24f),34f);
            if(SecondaryDetailsOpen)detailText.text="DETALLES DEL EJÉRCITO\n\n"+status.GetComponent<Text>().text+"\n\n‹ Volver a la preparación";
            status.gameObject.SetActive(false);statusCard.gameObject.SetActive(false);
        }

        public void Restore()
        {
            foreach(var pair in originalLabels)
            {
                var label=pair.Key;
                if(!label)continue;
                var old=pair.Value;
                label.fontSize=old.Size;
                label.resizeTextMinSize=old.Minimum;
                label.resizeTextMaxSize=old.Maximum;
                label.resizeTextForBestFit=old.BestFit;
                label.horizontalOverflow=old.Horizontal;
                label.verticalOverflow=old.Vertical;
                label.alignment=old.Alignment;
                label.SetVerticesDirty();
            }
            originalLabels.Clear();
            if(thirdHeroLabel && thirdHeroOriginalLabel!=null)thirdHeroLabel.text=thirdHeroOriginalLabel;
            thirdHeroOriginalLabel=null;thirdHeroLabel=null;
            if(statusCaptured){if(status)status.gameObject.SetActive(statusWasActive);if(statusCard)statusCard.gameObject.SetActive(statusCardWasActive);}
            statusCaptured=false;
            if(information)Destroy(information.gameObject);
            if(detailSheet)Destroy(detailSheet.gameObject);
            information=null;detailSheet=null;detailText=null;
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
            UsesThreeHeroSpaces=false;
            if(preview && initializedPreview)
                preview.gameObject.SetActive(originalPreviewActive);
            initializedPreview=false;right=preview=titleCard=summary=status=statusCard=null;
            for(int i=0;i<6;i++){cards[i]=null;values[i]=null;}
            UsesFullWidthMetrics=false;lastW=lastH=-1f;
        }
        void OnDestroy(){Restore();}
    }
}
