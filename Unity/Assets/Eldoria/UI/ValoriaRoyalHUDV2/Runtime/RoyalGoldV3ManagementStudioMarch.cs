using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Eldoria.UI.RoyalGold;

namespace Eldoria.UI.RoyalGold.V2
{
    /// <summary>Studio director's macro-layout on the same source-bound live
    /// controls. No added canvas, new command, artificial army or save.</summary>
    public sealed partial class RoyalGoldV3ManagementScreens
    {
#if UNITY_EDITOR
        public static bool StudioMarchPilotForEvidence = true;
#endif
        bool studioMarchActive;
        readonly List<Action> studioRestore=new List<Action>();
        readonly Dictionary<string,Image> studioPlates=new Dictionary<string,Image>();
        readonly RoyalText[] studioMetricCaptions=new RoyalText[6];
        Image studioBackdrop;
        RectTransform studioGalleryWindow;
        Sprite studioArcherT1,studioArcherT2;
        static readonly Color StudioSlate=new Color32(24,40,48,242);
        static readonly Color StudioInk=new Color32(14,25,33,245);
        static readonly Color StudioBrass=new Color32(218,191,143,255);
        static readonly Color StudioTeal=new Color32(32,118,115,255);

        void ResetStudioMarch()
        {
            for(int i=studioRestore.Count-1;i>=0;i--)studioRestore[i]();
            studioRestore.Clear();
            foreach(var plate in studioPlates.Values)if(plate)plate.gameObject.SetActive(false);
            foreach(var caption in studioMetricCaptions)if(caption)caption.gameObject.SetActive(false);
            if(studioBackdrop)studioBackdrop.gameObject.SetActive(false);
            if(studioGalleryWindow)studioGalleryWindow.gameObject.SetActive(false);
            studioMarchActive=false;
        }
        void HideStudioGraphic(Graphic g)
        {
            if(!g)return;
            bool prior=g.enabled;studioRestore.Add(()=>{if(g)g.enabled=prior;});g.enabled=false;
        }
        void HideDecorations(RectTransform r)
        {
            if(!r)return;
            foreach(var g in r.GetComponentsInChildren<Graphic>(true))
                // Only decorative helper layers. Approved source Images
                // include the actual hero portraits and must never be hidden.
                if(g.name.StartsWith("March art /",StringComparison.Ordinal))
                    HideStudioGraphic(g);
        }
        void StudioText(RoyalText t,int size,Color color,TextAnchor alignment,bool headingFont=false)
        {
            if(!t)return;
            var f=t.font;var c=t.color;var n=t.fontSize;var st=t.fontStyle;var a=t.alignment;
            var ho=t.horizontalOverflow;var vo=t.verticalOverflow;var fit=t.resizeTextForBestFit;
            var min=t.resizeTextMinSize;var max=t.resizeTextMaxSize;
            studioRestore.Add(()=>{if(!t)return;t.font=f;t.color=c;t.fontSize=n;t.fontStyle=st;t.alignment=a;
                t.horizontalOverflow=ho;t.verticalOverflow=vo;t.resizeTextForBestFit=fit;
                t.resizeTextMinSize=min;t.resizeTextMaxSize=max;});
            t.font=CoronaFinish.ResolveFont(headingFont);t.fontStyle=FontStyle.Normal;t.fontSize=size;
            t.color=color;t.alignment=alignment;t.resizeTextForBestFit=false;
            t.horizontalOverflow=HorizontalWrapMode.Wrap;t.verticalOverflow=VerticalWrapMode.Truncate;
        }
        Image StudioPlate(string key,RectTransform parent,float x,float y,float w,float h,Color colour,bool quiet=false,string material=null)
        {
            if(!studioPlates.TryGetValue(key,out var img)||!img)
            {
                var r=Rect("Studio / "+key,parent);
                img=r.gameObject.AddComponent<Image>();img.raycastTarget=false;
                studioPlates[key]=img;
            }
            img.gameObject.SetActive(true);
            string keyArt=material??(quiet?"secondary_quiet":null);
            img.sprite=keyArt==null?null:Resources.Load<Sprite>("ValoriaModular25D/UI/CoronaBrecha/"+keyArt);
            if(keyArt!=null&&!img.sprite)throw new InvalidOperationException("Missing original Corona material "+keyArt);
            img.color=keyArt==null?colour:new Color(.94f,.93f,.89f,colour.a);
            img.type=img.sprite&&img.sprite.border.sqrMagnitude>0?Image.Type.Sliced:Image.Type.Simple;
            img.pixelsPerUnitMultiplier=keyArt=="panel_obsidian"?3f:1.6f;
            img.fillCenter=true;img.raycastTarget=false;
            img.preserveAspect=false;
            Place(img.rectTransform,x,y,w,h);img.rectTransform.SetAsFirstSibling();
            return img;
        }
        // R4 scene-layer contract: only the independent, original
        // gallery is cropped. Never put the actual hero controls in this mask.
        void PlaceStudioGallery(RectTransform parent,float width,float height)
        {
            if(!studioGalleryWindow)
            {
                studioGalleryWindow=Rect("Studio / clipped command gallery environment",parent);
                studioGalleryWindow.gameObject.AddComponent<RectMask2D>();
            }
            studioGalleryWindow.gameObject.SetActive(true);
            Place(studioGalleryWindow,0,0,width,height);
            studioGalleryWindow.SetAsFirstSibling();
            var art=StudioPlate("gallery environment",studioGalleryWindow,0,0,width,height,Color.white,true,"march_command_gallery");
            art.color=Color.white;art.type=Image.Type.Simple;art.preserveAspect=true;
            float ratio=art.sprite.rect.width/art.sprite.rect.height;
            float artW=Mathf.Max(width,height*ratio),artH=artW/ratio;
            // Bottom anchor keeps the physical ledge; a change in aspect
            // can remove ceiling but cannot cut off the contact surface.
            Place(art.rectTransform,(width-artW)*.5f,height-artH,artW,artH);
        }
        void StudioContact(string key,RectTransform parent,Transform behind,float x,float y,float width,float height,float opacity)
        {
            var shadow=StudioPlate(key,parent,x,y,width,height,new Color(0,0,0,opacity),true,"march_gallery_contact");
            shadow.type=Image.Type.Simple;shadow.color=new Color(0,0,0,opacity);
            shadow.raycastTarget=false;
            shadow.transform.SetSiblingIndex(behind.GetSiblingIndex()+1);
        }
        void PutStudioTroopArt()
        {
            if(!studioMarchActive)return;
            if(!studioArcherT1)studioArcherT1=RoyalGoldV41SourceArt.Get("ValoriaModular25D/UI/CoronaBrecha/arquera");
            if(!studioArcherT2)studioArcherT2=RoyalGoldV41SourceArt.Get("ValoriaModular25D/UI/CoronaBrecha/arquera_veterana");
            var chosen=selectedTier==2?studioArcherT2:studioArcherT1;
            if(!chosen)throw new InvalidOperationException("Missing actual approved tier character");
            approvedTroopRow.sprite=chosen;
        }
        void StudioButton(RoyalButton b,Color fill,int fontSize,string material="secondary_quiet")
        {
            if(!b)return;
            var r=(RectTransform)b.transform;
            var label=b.GetComponentInChildren<RoyalText>(true);
            var plate=StudioPlate("button "+b.name,r,0,0,r.rect.width,r.rect.height,fill,true,material);
            // Art remains a child of the existing button. Make its full rect
            // the actual raycast/feedback graphic, not an unrelated overlay.
            var previous=b.targetGraphic;
            if(previous&&previous!=plate)HideStudioGraphic(previous);
            // Image defaults to rectangle hit-testing (alpha threshold zero).
            // Do not set the property on imported non-readable Corona sprites.
            plate.raycastTarget=true;
            b.targetGraphic=plate;
            studioRestore.Add(()=>{if(b)b.targetGraphic=previous;if(plate)plate.raycastTarget=false;});
            if(label)
            {
                StudioText(label,fontSize,CoronaFinish.Ivory,TextAnchor.MiddleCenter);
                label.rectTransform.SetAsLastSibling();
            }
        }
        void PutStudioMarchMetric(int i,string live)
        {
            if(!studioMarchActive)
            {
                marchMetricValues[i].Put(live);
                if(studioMetricCaptions[i])studioMetricCaptions[i].gameObject.SetActive(false);
                return;
            }
            int split=live.IndexOf('\n');
            if(split<0){marchMetricValues[i].Put(live);return;}
            studioMetricCaptions[i].Put(live.Substring(0,split));
            marchMetricValues[i].Put(live.Substring(split+1));
            FitStudioText(studioMetricCaptions[i],27,25);
            FitStudioText(marchMetricValues[i],35,29);
        }
        // Fit both dimensions, including the two-line hero labels. Width-only
        // fitting was insufficient for the real native metric caption rects.
        static void FitStudioText(RoyalText label,int max,int min)
        {
            if(!label)return;
            label.fontSize=max;
            while(label.fontSize>min &&
                (label.preferredHeight>label.rectTransform.rect.height-1f ||
                 label.preferredWidth>label.rectTransform.rect.width-1f))
                label.fontSize--;
        }

        void ApplyStudioMarchPortrait(float w,float h,float x,float insetRight,float top,float bottom)
        {
#if UNITY_EDITOR
            if(!StudioMarchPilotForEvidence)return;
#endif
            float width=w-x-insetRight;
            float headH=122f,footH=132f,bodyY=top+headH+8f;
            float bodyH=h-bodyY-footH-bottom-10f;
            // Preserve proven shorter and horizontal layouts.
            if(width<690f||bodyH<1320f)return;
            studioMarchActive=true;
            if(!studioBackdrop)
            {
                var r=Rect("Studio / original command table art",layoutRoot);
                studioBackdrop=r.gameObject.AddComponent<Image>();studioBackdrop.raycastTarget=false;
                studioBackdrop.sprite=Resources.Load<Sprite>("ValoriaModular25D/UI/CoronaBrecha/march_command_table");
            }
            if(!studioBackdrop.sprite)throw new InvalidOperationException("Missing source-pinned March studio art");
            studioBackdrop.gameObject.SetActive(true);
            studioBackdrop.color=new Color(.88f,.92f,.95f,1);
            float ratio=studioBackdrop.sprite.rect.width/studioBackdrop.sprite.rect.height;
            float artW=Mathf.Max(w,h*ratio),artH=artW/ratio;
            Place(studioBackdrop.rectTransform,(w-artW)*.5f,(h-artH)*.5f,artW,artH);
            studioBackdrop.transform.SetAsFirstSibling();
            HideStudioGraphic(paneSurfaces[header]);HideStudioGraphic(paneSurfaces[right]);
            HideStudioGraphic(marchEnvironment);
            HideStudioGraphic(right.GetComponent<RoyalGoldV2FrameGraphic>());
            HideStudioGraphic(marchReferenceSummaryPaper);
            foreach(var r in new[]{marchHeadingSurface,marchTroopSurface,marchSummaryBacking,marchStatusBacking})
                foreach(var g in r.GetComponentsInChildren<Graphic>(true))HideStudioGraphic(g);
            Place(header,x,top,width,headH);
            Place(footer,x,h-bottom-footH,width,footH);
            StudioPlate("navigation surface",header,0,0,width,headH,StudioInk,true,"panel_obsidian");
            Place((RectTransform)back.transform,8,12,96,96);
            StudioButton(back,StudioSlate,38);
            // Preserve the existing destination button even when Refresh
            // re-enables it. Its live rect no longer intersects the title.
            float destinationW=194f,headingW=width-destinationW-130f;
            Place((RectTransform)switchDestination.transform,width-destinationW-8f,12f,destinationW,96f);
            StudioButton(switchDestination,StudioSlate,26);
            Place(heading.rectTransform,118,8,headingW,68);
            Place(subheading.rectTransform,118,78,headingW,36);
            StudioText(heading,30,StudioBrass,TextAnchor.MiddleLeft,true);
            StudioText(subheading,25,CoronaFinish.Muted,TextAnchor.MiddleLeft);

            // R3 director and technical challenge, actual run 45882958150.
            // More portrait height; two-column obsidian summary; preserve order controls.
            // Actual R4 direction (run 45907960440): .35 operative
            // height and .46 leader width. Preserve tested control bands.
            // R5: restore the focal hierarchy observed in the approved early
            // room, with the R4 continuous environment and independent art.
            float gap=12f,heroH=bodyH*.38f,troopH=Mathf.Max(410f,bodyH*.30f);
            float summaryH=bodyH-heroH-troopH-gap;
            Place(left,x,bodyY,width,heroH+troopH);
            Place(right,x,bodyY+heroH+troopH+gap,width,summaryH);
            float lw=left.rect.width;
            PlaceStudioGallery(left,lw,heroH-5);
            var headingShade=StudioPlate("gallery heading shade",left,0,0,lw,91,new Color(.045f,.038f,.037f,.81f));
            headingShade.transform.SetSiblingIndex(studioGalleryWindow.GetSiblingIndex()+1);
            Place(unitTitle.rectTransform,17,10,lw*.61f,75);
            StudioText(unitTitle,30,StudioBrass,TextAnchor.MiddleLeft,true);
            Place((RectTransform)heroesButton.transform,lw-220,2,210,92);
            StudioButton(heroesButton,StudioInk,28);
            float cardGap=12f;
            float cardsW=lw-28f-cardGap,leaderW=cardsW*.55f,wingW=cardsW-leaderW;
            float wingX=14f+leaderW+cardGap;
            float leaderBottom=heroH*.94f,wingTop=102f;
            float wingH=(leaderBottom-wingTop-cardGap)*.5f;
            // A shared physical dossier rack supports the two secondary
            // slots; they are not floating or pretending to be scenery.
            var stand=StudioPlate("secondary dossier rack",left,wingX-5f,wingTop-6f,wingW+10f,leaderBottom-wingTop+12f,new Color32(35,28,25,245));
            stand.transform.SetSiblingIndex(studioGalleryWindow.GetSiblingIndex()+1);
            for(int i=0;i<3;i++)
            {
                // Preserve the director's triptych, but correct the visibly
                // diminished commander's portrait in the authentic R3 frame.
                float cardW=i==0?leaderW:wingW;
                float cardX=i==0?14f:wingX;
                float cardTop=i==0?94f:wingTop+(i-1)*(wingH+cardGap);
                float cardH=i==0?leaderBottom-cardTop:wingH;
                float baseY=cardTop+cardH;
                var card=(RectTransform)heroSlotButtons[i].transform;
                Place(card,cardX,cardTop,cardW,cardH);
                StudioContact("gallery contact "+i,left,studioGalleryWindow,cardX-4f,baseY-8f,cardW+8f,34f,i==0?.95f:.78f);
                HideDecorations(card);
                var heroButton=heroSlotButtons[i];
                var oldTarget=heroButton.targetGraphic;
                if(oldTarget)HideStudioGraphic(oldTarget);
                Color brass=i==0?new Color32(172,137,83,255):new Color32(116,100,77,255);
                var edge=StudioPlate("hero physical edge "+i,card,0,0,cardW,cardH,brass);
                edge.raycastTarget=true;heroButton.targetGraphic=edge;
                studioRestore.Add(()=>{if(heroButton)heroButton.targetGraphic=oldTarget;if(edge)edge.raycastTarget=false;});
                var backing=StudioPlate("hero physical backing "+i,card,3,3,cardW-6,cardH-6,new Color32(26,26,30,255));
                backing.transform.SetSiblingIndex(edge.transform.GetSiblingIndex()+1);
                float captionH=80f;
                float artW2=i==0?cardW-10:cardW*.47f-8f;
                float artH2=i==0?cardH-captionH-8:cardH-10;
                Place(heroPortraitViewports[i],5,5,artW2,artH2);
                var sprite=approvedHeroes[i].sprite;
                float ar=sprite?sprite.rect.width/sprite.rect.height:1f;
                bool portrait=!string.IsNullOrEmpty(heroSlots[i]);
                float pw=portrait?Mathf.Max(artW2,artH2*ar):Mathf.Min(artW2,artH2*ar);
                float ph=pw/ar;
                Place(approvedHeroes[i].rectTransform,(artW2-pw)*.5f,portrait?0:(artH2-ph)*.5f,pw,ph);
                // No repeated massive violet frames. The thin physical
                // brass edge and dark plaque provide the visual anchoring.
                float labelX=i==0?4f:artW2+9f;
                float labelY=i==0?cardH-captionH:4f;
                float labelW=i==0?cardW-8f:cardW-labelX-4f;
                float labelH=i==0?captionH-4f:cardH-8f;
                var ribbon=StudioPlate("hero caption "+i,card,labelX,labelY,labelW,labelH,new Color32(28,25,28,255));
                ribbon.transform.SetAsLastSibling();
                Place(heroSlotLabels[i].rectTransform,labelX+3f,labelY+3f,labelW-6f,labelH-6f);
                StudioText(heroSlotLabels[i],27,i==2?CoronaFinish.Muted:CoronaFinish.Ivory,TextAnchor.MiddleCenter);
                heroSlotLabels[i].transform.SetAsLastSibling();
            }
            float ty=heroH+4f;
            var troopDossier=StudioPlate("troop dossier",left,0,ty,lw,troopH-9,StudioSlate,true,"panel_obsidian");
            // Larger independent tier-specific archer, with actual boot
            // contact on the same material plane. Functional controls use
            // a separate right-hand column, not an illustration of a UI.
            float artWidth=lw*.29f,infoX=artWidth+26f;
            float controlW=lw-infoX-16f;
            Place(marchTroopChapter.rectTransform,infoX,ty+7,controlW,35);
            StudioText(marchTroopChapter,29,StudioBrass,TextAnchor.MiddleLeft,true);
            var oldTroopSprite=approvedTroopRow.sprite;
            studioRestore.Add(()=>{if(approvedTroopRow)approvedTroopRow.sprite=oldTroopSprite;});
            PutStudioTroopArt();
            Place(approvedTroopRow.rectTransform,13,ty+15,artWidth,troopH-32f);
            StudioContact("troop contact",left,troopDossier.transform,26,ty+troopH-45f,artWidth-24f,28,.90f);
            Place(unitDetail.rectTransform,infoX,ty+42,controlW,42);
            StudioText(unitDetail,30,CoronaFinish.Ivory,TextAnchor.MiddleLeft);
            float smallGap=8f,half=(controlW-smallGap)*.5f,qh=94f;
            Place((RectTransform)tierButton.transform,infoX,ty+86,half,qh);
            StudioButton(tierButton,StudioInk,27);
            Place((RectTransform)percentModeButton.transform,infoX+half+smallGap,ty+86,half,qh);
            StudioButton(percentModeButton,StudioInk,28);
            float qy=ty+188f,quantityW=controlW;
            Place(quantityRow,infoX,qy,quantityW,qh);
            StudioPlate("quantity inset",quantityRow,0,0,quantityW,qh,StudioInk,true,"secondary_quiet");
            Place((RectTransform)minus.transform,0,0,94,qh);
            Place((RectTransform)plus.transform,quantityW-94,0,94,qh);
            StudioButton(minus,StudioSlate,38);StudioButton(plus,StudioSlate,38);
            Place(selectedText.rectTransform,99,0,quantityW-198,qh);
            StudioText(selectedText,42,CoronaFinish.Ivory,TextAnchor.MiddleCenter);
            float sliderY=ty+290f;
            Place((RectTransform)slider.transform,infoX,sliderY,controlW,94);
            var track=slider.GetComponent<Image>();
            if(track)
            {
                var colour=track.color;var ray=track.raycastTarget;
                studioRestore.Add(()=>{if(track){track.color=colour;track.raycastTarget=ray;}});
                track.color=new Color(.025f,.04f,.05f,.06f);
                track.raycastTarget=true;
                var rail=StudioPlate("slider rail",(RectTransform)slider.transform,6,31,controlW-12,32,StudioInk,true,"wallet_inset");
                rail.transform.SetAsFirstSibling();
            }
            var fillArea=slider.fillRect?slider.fillRect.parent as RectTransform:null;
            if(fillArea)
            {
                var amin=fillArea.anchorMin;var amax=fillArea.anchorMax;
                studioRestore.Add(()=>{if(fillArea){fillArea.anchorMin=amin;fillArea.anchorMax=amax;}});
                fillArea.anchorMin=new Vector2(amin.x,.42f);fillArea.anchorMax=new Vector2(amax.x,.58f);
            }
            var handle=slider.handleRect;
            if(handle)
            {
                var old=handle.sizeDelta;var amin=handle.anchorMin;var amax=handle.anchorMax;
                studioRestore.Add(()=>{if(handle){handle.sizeDelta=old;handle.anchorMin=amin;handle.anchorMax=amax;}});
                handle.anchorMin=new Vector2(amin.x,.12f);handle.anchorMax=new Vector2(amax.x,.88f);
                handle.sizeDelta=new Vector2(56f,0f);
            }
            float rw=right.rect.width,rh=right.rect.height;
            StudioPlate("command scorecard",right,0,0,rw,rh,StudioInk,true,"panel_obsidian");
            Place(summary.rectTransform,18,7,rw-36,51);
            StudioText(summary,30,StudioBrass,TextAnchor.MiddleLeft,true);
            float tileTop=55,tileGap=7,statusH=128;
            float tileW=(rw-32f-tileGap)/2f;
            float troopMetricH=82f;
            float compactMetricH=(rh-tileTop-statusH-18f-2*tileGap-troopMetricH)*.5f;
            for(int i=0;i<6;i++)
            {
                var card=marchMetricCards[i];
                float tileH=i<2?troopMetricH:compactMetricH;
                float tileY=i<2?tileTop:tileTop+troopMetricH+tileGap+((i-2)/2)*(compactMetricH+tileGap);
                Place(card,16+(i%2)*(tileW+tileGap),tileY,tileW,tileH);
                // One shared summary surface with quiet separators,
                // not six competing illustrated frames.
                var tile=StudioPlate("metric tile "+i,card,0,0,tileW,tileH,new Color(.028f,.039f,.049f,i==2?.67f:.35f));
                var separator=StudioPlate("metric divider "+i,card,10,tileH-2f,tileW-20f,2f,new Color(.72f,.59f,.39f,i==2?.8f:.32f));
                separator.transform.SetAsLastSibling();
                if(!studioMetricCaptions[i])
                    studioMetricCaptions[i]=Text("Studio / real metric label "+i,card,28,CoronaFinish.Muted);
                var cap=studioMetricCaptions[i];cap.gameObject.SetActive(true);
                cap.font=CoronaFinish.ResolveFont(false);cap.fontStyle=FontStyle.Normal;
                cap.color=CoronaFinish.Muted;
                var value=marchMetricValues[i];
                if(i<2)
                {
                    cap.alignment=TextAnchor.MiddleCenter;
                    float captionH=Mathf.Max(36f,tileH*.43f);
                    Place(cap.rectTransform,8,2,tileW-16,captionH);
                    Place(value.rectTransform,8,captionH+2f,tileW-16,tileH-captionH-5f);
                    StudioText(value,35,CoronaFinish.Ivory,TextAnchor.MiddleCenter);
                }
                else
                {
                    cap.alignment=TextAnchor.MiddleLeft;
                    float labelW=tileW*.56f;
                    Place(cap.rectTransform,12,3,labelW-16f,tileH-6f);
                    Place(value.rectTransform,labelW,3,tileW-labelW-10f,tileH-6f);
                    StudioText(value,35,i==2?StudioBrass:(Color)CoronaFinish.Ivory,TextAnchor.MiddleRight);
                }
            }
            float sy=rh-statusH-4f;
            StudioPlate("readiness strip",right,12,sy,rw-24,statusH-6,StudioSlate,true,"secondary_quiet");
            Place(status.rectTransform,22,sy+6,rw-44,statusH-18);
            StudioText(status,29,CoronaFinish.Ivory,TextAnchor.MiddleLeft);
            Place((RectTransform)action.transform,12,28,width-24,96);
            StudioButton(action,StudioTeal,34,"action_arcane");
            StudioText(actionLabel,34,CoronaFinish.Ivory,TextAnchor.MiddleCenter);
            actionLabel.transform.SetAsLastSibling();
            Place(feedback.rectTransform,12,0,width-24,24);
            StudioText(feedback,26,CoronaFinish.Ivory,TextAnchor.MiddleCenter);
        }
    }
}
