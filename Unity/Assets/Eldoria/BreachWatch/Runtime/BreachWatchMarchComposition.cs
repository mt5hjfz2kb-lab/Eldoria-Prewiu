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
        readonly RectTransform[] heroViewports=new RectTransform[3];
        readonly RectTransform[] heroPictures=new RectTransform[3];
        readonly RectTransform[] heroLabels=new RectTransform[3];
        bool thirdHeroWasActive, thirdHeroCaptured;
        RectTransform compactOwner;
        readonly RectTransform[] compactParts=new RectTransform[16];
        public bool CompactPortraitApplied { get; private set; }
        string thirdHeroOriginalLabel;
        Text thirdHeroLabel;
        RectTransform detailSheet,detailCard;
        Button detailReturn;
        Text detailHeading,detailReturnText;
        Text detailText;
        Button information;
        bool statusWasActive,statusCardWasActive,statusCaptured;
        public Button InformationButton => information;
        public Button InformationReturnButton => detailReturn;
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
                UsesFullWidthMetrics=false;CompactPortraitApplied=false;return false;
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
            CompactPortraitApplied=false;
            if(root.rect.width>root.rect.height)
            {
                RefreshActualHeroCards(root);
                RefreshSecondaryInformation(root);
                UsesFullWidthMetrics=false;
                return false;
            }
            CompactPortraitApplied=ReflowCompactPortrait(root);
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
            int metricColumns=CompactPortraitApplied?3:2;
            int metricRows=CompactPortraitApplied?2:3;
            float tileH=Mathf.Max(30f,(h-tileTop-bottomH-12f-(metricRows-1)*gap)/metricRows);
            float tileW=(w-2f*margin-(metricColumns-1)*gap)/metricColumns;
            At(titleCard,margin-7f,3f,w-2f*margin+14f,captionH);
            At(summary,margin,7f,w-2f*margin,captionH-8f);
            var title=summary.GetComponent<Text>();
            if(title)title.fontSize=22;

            for(int i=0;i<6;i++)
            {
                float x=margin+(i%metricColumns)*(tileW+gap);
                float y=tileTop+(i/metricColumns)*(tileH+gap);
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
            float statusY=tileTop+metricRows*tileH+(metricRows-1)*gap+7f;
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


        bool ReflowCompactPortrait(RectTransform root)
        {
            // Recompose the actual existing controls into a content-sized
            // military sheet. A third hero must not inherit the old two-card
            // vertical stretch, and six metrics need only two rows.
            if(compactOwner!=root || !compactParts[0])
            {
                var names=new[]{
                    "V3 / Responsive military layout","V3 / Destination header",
                    "V3 / Featured military unit","V3 / Destination actions",
                    "March / hero section cartouche","March / live troop order cartouche",
                    "March / reference troop chapter","Approved source / archer-row",
                    "Unit profile and composition","Select archer tier",
                    "Edit march by percent","Quantity controls","Quantity slider",
                    "Reduce quantity","Increase quantity","Authoritative quantity"};
                var all=root.GetComponentsInChildren<RectTransform>(true);
                for(int i=0;i<names.Length;i++)
                {
                    compactParts[i]=null;
                    foreach(var candidate in all)
                        if(candidate.name==names[i]){compactParts[i]=candidate;break;}
                    if(!compactParts[i]){compactOwner=null;return false;}
                }
                compactOwner=root;
            }
            var layout=compactParts[0];var header=compactParts[1];
            var left=compactParts[2];var footer=compactParts[3];
            float width=right.rect.width,x=right.anchoredPosition.x;
            float available=root.rect.height/Mathf.Max(.01f,layout.localScale.y);
            float heroCardH=Mathf.Clamp(width*.36f,230f,320f);
            float heroH=98f+heroCardH,troopH=260f,summaryH=280f;
            float totalH=12f+110f+8f+heroH+troopH+12f+summaryH+8f+100f+12f;
            if(width<330f || available<totalH)return false;
            Remember(layout);Remember(header);Remember(left);Remember(right);Remember(footer);
            layout.sizeDelta=new Vector2(layout.sizeDelta.x,totalH);
            float bodyY=130f;
            At(header,x,12f,width,110f);
            At(left,x,bodyY,width,heroH+troopH);
            At(right,x,bodyY+heroH+troopH+12f,width,summaryH);
            At(footer,x,totalH-112f,width,100f);
            for(int i=0;i<3;i++)
            {
                var slot=heroSlots[i]?heroSlots[i]:Find(left,"Hero slot "+i);
                if(!slot)return false;
                Remember(slot);slot.sizeDelta=new Vector2(slot.sizeDelta.x,heroCardH);
            }
            for(int i=4;i<compactParts.Length;i++)Remember(compactParts[i]);
            At(compactParts[4],7f,2f,width-14f,85f);
            float troopTop=heroH+4f;
            At(compactParts[5],7f,troopTop,width-14f,troopH-9f);
            At(compactParts[6],19f,troopTop+5f,width-38f,43f);
            float pictureW=width*.26f,orderX=pictureW+23f;
            float orderW=Mathf.Max(120f,width-orderX-14f);
            At(compactParts[7],13f,troopTop+53f,pictureW,troopH-62f);
            float orderY=troopTop+52f,orderH=troopH-58f;
            float descH=Mathf.Clamp(orderH*.20f,30f,65f);
            At(compactParts[8],orderX,orderY,orderW,descH);
            At(compactParts[9],orderX,orderY+descH+4f,orderW,Mathf.Min(60f,orderH*.20f));
            float selectorsY=orderY+descH+9f+Mathf.Min(64f,orderH*.20f);
            float selectorsH=Mathf.Clamp(orderH*.26f,56f,83f);
            float quantityW=orderW*.53f,percentW=orderW-quantityW-8f;
            At(compactParts[10],orderX,selectorsY,percentW,selectorsH);
            At(compactParts[11],orderX+percentW+8f,selectorsY,quantityW,selectorsH);
            float buttonW=Mathf.Min(selectorsH*.85f,quantityW*.29f);
            At(compactParts[13],0f,0f,buttonW,selectorsH);
            At(compactParts[14],quantityW-buttonW,0f,buttonW,selectorsH);
            At(compactParts[15],buttonW+3f,0f,
                Mathf.Max(36f,quantityW-2f*buttonW-6f),selectorsH);
            At(compactParts[12],orderX+8f,selectorsY+selectorsH+11f,orderW-16f,22f);
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
                for(int i=0;i<3;i++)
                {
                    heroViewports[i]=Find(heroSlots[i],"Hero portrait viewport");
                    heroPictures[i]=FindPortrait(heroViewports[i]);
                    heroLabels[i]=Find(heroSlots[i],"Button label");
                }
            }
            if(!heroRow || heroRow.rect.width<280f ||
               !heroSlots[0] || !heroSlots[1] || !heroSlots[2] ||
               !heroViewports[0] || !heroViewports[1] || !heroViewports[2] ||
               !heroPictures[0] || !heroPictures[1] || !heroPictures[2] ||
               !heroLabels[0] || !heroLabels[1] || !heroLabels[2])
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
            for(int i=0;i<3;i++)
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


        string LiveArmyDetails()
        {
            var current=status?status.GetComponent<Text>():null;
            return current?current.text.Replace(" · ","\n"):"";
        }
        Text OwnedLabel(string name,Transform parent,Font font,int size)
        {
            var go=new GameObject(name,typeof(RectTransform),typeof(Text));
            go.layer=parent.gameObject.layer;go.transform.SetParent(parent,false);
            var text=go.GetComponent<Text>();text.font=font;text.fontSize=size;
            text.color=new Color(.96f,.90f,.79f);text.raycastTarget=false;
            text.horizontalOverflow=HorizontalWrapMode.Wrap;
            return text;
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
            var stone=heroSlots[0]?heroSlots[0].GetComponent<Button>()?.targetGraphic as Image:null;
            var font=status.GetComponent<Text>().font;
            if(!information)
            {
                var go=new GameObject("March / secondary information",typeof(RectTransform),typeof(Image),typeof(Button));
                go.layer=right.gameObject.layer;go.transform.SetParent(right,false);
                information=go.GetComponent<Button>();
                var face=go.GetComponent<Image>();
                if(stone){face.sprite=stone.sprite;face.type=Image.Type.Sliced;}
                face.color=Color.white;information.targetGraphic=face;
                var label=OwnedLabel("Information label",go.transform,font,26);
                label.alignment=TextAnchor.MiddleCenter;label.text="i  ·  Detalles del ejército";
                label.rectTransform.anchorMin=Vector2.zero;label.rectTransform.anchorMax=Vector2.one;
                label.rectTransform.offsetMin=label.rectTransform.offsetMax=Vector2.zero;

                var sheetGo=new GameObject("March / army information sheet",typeof(RectTransform),typeof(Image),typeof(Button));
                sheetGo.layer=root.gameObject.layer;sheetGo.transform.SetParent(root,false);
                detailSheet=sheetGo.GetComponent<RectTransform>();
                detailSheet.anchorMin=Vector2.zero;detailSheet.anchorMax=Vector2.one;
                detailSheet.offsetMin=detailSheet.offsetMax=Vector2.zero;
                var shade=sheetGo.GetComponent<Image>();shade.color=new Color(.018f,.014f,.018f,.94f);
                sheetGo.GetComponent<Button>().targetGraphic=shade;
                sheetGo.GetComponent<Button>().onClick.AddListener(()=>detailSheet.gameObject.SetActive(false));

                var cardGo=new GameObject("March / shared carved stone details",typeof(RectTransform),typeof(Image));
                cardGo.layer=root.gameObject.layer;cardGo.transform.SetParent(detailSheet,false);
                detailCard=cardGo.GetComponent<RectTransform>();
                detailCard.anchorMin=detailCard.anchorMax=detailCard.pivot=new Vector2(.5f,.5f);
                detailCard.anchoredPosition=Vector2.zero;
                var cardFace=cardGo.GetComponent<Image>();
                if(stone){cardFace.sprite=stone.sprite;cardFace.type=Image.Type.Sliced;}
                cardFace.color=Color.white;
                detailHeading=OwnedLabel("Army details heading",detailCard,font,22);
                detailHeading.text="DETALLES DEL EJÉRCITO";
                detailHeading.color=new Color(.94f,.78f,.48f);
                detailHeading.alignment=TextAnchor.MiddleCenter;
                detailText=OwnedLabel("Live saved army details",detailCard,font,19);
                detailText.alignment=TextAnchor.MiddleLeft;

                var returnGo=new GameObject("March / return to preparation",typeof(RectTransform),typeof(Image),typeof(Button));
                returnGo.layer=root.gameObject.layer;returnGo.transform.SetParent(detailCard,false);
                detailReturn=returnGo.GetComponent<Button>();var returnFace=returnGo.GetComponent<Image>();
                returnFace.sprite=Resources.Load<Sprite>("VigiliaArt/PremiumSurfaces/burgundy_action");
                returnFace.type=Image.Type.Sliced;detailReturn.targetGraphic=returnFace;
                detailReturn.onClick.AddListener(()=>detailSheet.gameObject.SetActive(false));
                detailReturnText=OwnedLabel("Return label",returnGo.transform,font,18);
                detailReturnText.text="VOLVER A LA MARCHA";detailReturnText.alignment=TextAnchor.MiddleCenter;
                detailReturnText.rectTransform.anchorMin=Vector2.zero;detailReturnText.rectTransform.anchorMax=Vector2.one;
                detailReturnText.rectTransform.offsetMin=detailReturnText.rectTransform.offsetMax=Vector2.zero;
                information.onClick.AddListener(()=>{
                    detailText.text=LiveArmyDetails();detailSheet.SetAsLastSibling();detailSheet.gameObject.SetActive(true);
                });
                detailSheet.gameObject.SetActive(false);
            }
            // The existing manager resets descendant button typography during
            // orientation reflow. Reapply only these owned presentation labels.
            information.GetComponentInChildren<Text>().fontSize=26;
            detailHeading.fontSize=22;detailText.fontSize=root.rect.width>root.rect.height?16:19;
            detailReturnText.fontSize=18;
            float cw=Mathf.Min(root.rect.width*.90f,640f);
            float ch=Mathf.Min(root.rect.height*.86f,500f);
            detailCard.sizeDelta=new Vector2(cw,ch);
            At(detailHeading.rectTransform,20f,20f,cw-40f,42f);
            At(detailText.rectTransform,28f,74f,cw-56f,Mathf.Max(80f,ch-156f));
            At((RectTransform)detailReturn.transform,24f,ch-67f,cw-48f,43f);
            var rect=(RectTransform)information.transform;
            At(rect,12f,Mathf.Max(0,right.rect.height-43f),Mathf.Max(1,right.rect.width-24f),38f);
            if(SecondaryDetailsOpen)detailText.text=LiveArmyDetails();
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
            compactOwner=null;for(int i=0;i<compactParts.Length;i++)compactParts[i]=null;
            CompactPortraitApplied=false;
            if(thirdHeroLabel && thirdHeroOriginalLabel!=null)thirdHeroLabel.text=thirdHeroOriginalLabel;
            thirdHeroOriginalLabel=null;thirdHeroLabel=null;
            if(statusCaptured){if(status)status.gameObject.SetActive(statusWasActive);if(statusCard)statusCard.gameObject.SetActive(statusCardWasActive);}
            statusCaptured=false;
            if(information)Destroy(information.gameObject);
            if(detailSheet)Destroy(detailSheet.gameObject);
            information=null;detailSheet=detailCard=null;detailText=detailHeading=detailReturnText=null;detailReturn=null;
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
            for(int i=0;i<3;i++)
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
