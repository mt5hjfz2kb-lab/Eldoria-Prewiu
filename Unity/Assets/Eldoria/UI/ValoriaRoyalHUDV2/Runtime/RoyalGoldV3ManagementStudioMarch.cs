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
        Image StudioPlate(string key,RectTransform parent,float x,float y,float w,float h,Color colour,bool quiet=false)
        {
            if(!studioPlates.TryGetValue(key,out var img)||!img)
            {
                var r=Rect("Studio / "+key,parent);
                img=r.gameObject.AddComponent<Image>();img.raycastTarget=false;
                studioPlates[key]=img;
            }
            img.gameObject.SetActive(true);img.color=quiet?new Color(.89f,.94f,.95f,colour.a):colour;
            img.sprite=quiet?Resources.Load<Sprite>("ValoriaModular25D/UI/CoronaBrecha/secondary_quiet"):null;
            img.type=img.sprite&&img.sprite.border.sqrMagnitude>0?Image.Type.Sliced:Image.Type.Simple;
            img.preserveAspect=false;
            Place(img.rectTransform,x,y,w,h);img.rectTransform.SetAsFirstSibling();
            return img;
        }
        void StudioButton(RoyalButton b,Color fill,int fontSize)
        {
            if(!b)return;
            var r=(RectTransform)b.transform;
            var label=b.GetComponentInChildren<RoyalText>(true);
            StudioPlate("button "+b.name,r,2,2,r.rect.width-4,r.rect.height-4,fill);
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
            CoronaFinish.FitSingleLine(studioMetricCaptions[i],28,25);
            CoronaFinish.FitSingleLine(marchMetricValues[i],38,29);
        }
        void ApplyStudioMarchPortrait(float w,float h,float x,float insetRight,float top,float bottom)
        {
#if UNITY_EDITOR
            if(!StudioMarchPilotForEvidence)return;
#endif
            float width=w-x-insetRight;
            float headH=112f,footH=122f,bodyY=top+headH+8f;
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
            HideStudioGraphic(right.GetComponent<RoyalGoldV2FrameGraphic>());
            HideStudioGraphic(marchReferenceSummaryPaper);
            foreach(var r in new[]{marchHeadingSurface,marchTroopSurface,marchSummaryBacking,marchStatusBacking})
                foreach(var g in r.GetComponentsInChildren<Graphic>(true))HideStudioGraphic(g);
            Place(header,x,top,width,headH);
            Place(footer,x,h-bottom-footH,width,footH);
            StudioPlate("navigation surface",header,0,0,width,headH,StudioInk);
            Place((RectTransform)back.transform,6,10,92,92);
            StudioButton(back,StudioSlate,38);
            Place(heading.rectTransform,112,9,width-124,50);
            Place(subheading.rectTransform,112,61,width-124,40);
            StudioText(heading,34,StudioBrass,TextAnchor.MiddleLeft,true);
            StudioText(subheading,27,CoronaFinish.Muted,TextAnchor.MiddleLeft);

            // Exact real agent decisions (run 45864197708).
            float gap=12f,heroH=bodyH*.26f,troopH=bodyH*.34f;
            float summaryH=bodyH-heroH-troopH-gap;
            Place(left,x,bodyY,width,heroH+troopH);
            Place(right,x,bodyY+heroH+troopH+gap,width,summaryH);
            float lw=left.rect.width;
            StudioPlate("hero dossier",left,0,0,lw,heroH-5,StudioSlate,true);
            Place(unitTitle.rectTransform,17,10,lw*.61f,75);
            StudioText(unitTitle,30,StudioBrass,TextAnchor.MiddleLeft,true);
            Place((RectTransform)heroesButton.transform,lw-220,2,210,92);
            StudioButton(heroesButton,StudioInk,28);
            float cardTop=97f,cardH=heroH-cardTop-12f,cardGap=10f,cardW=(lw-28f-2f*cardGap)/3f;
            for(int i=0;i<3;i++)
            {
                var card=(RectTransform)heroSlotButtons[i].transform;
                Place(card,14+i*(cardW+cardGap),cardTop,cardW,cardH);
                HideDecorations(card);
                StudioPlate("hero slot "+i,card,1,1,cardW-2,cardH-2,i==0?(Color)new Color32(31,57,64,255):StudioInk);
                float captionH=90f,artW2=cardW-12,artH2=cardH-captionH-10;
                Place(heroPortraitViewports[i],6,6,artW2,artH2);
                var sprite=approvedHeroes[i].sprite;
                float ar=sprite?sprite.rect.width/sprite.rect.height:1f;
                bool portrait=!string.IsNullOrEmpty(heroSlots[i]);
                float pw=portrait?Mathf.Max(artW2,artH2*ar):Mathf.Min(artW2,artH2*ar);
                float ph=pw/ar;
                Place(approvedHeroes[i].rectTransform,(artW2-pw)*.5f,portrait?0:(artH2-ph)*.5f,pw,ph);
                var ribbon=StudioPlate("hero caption "+i,card,4,cardH-captionH,cardW-8,captionH-4,StudioInk);
                ribbon.transform.SetAsLastSibling();
                Place(heroSlotLabels[i].rectTransform,7,cardH-captionH,cardW-14,captionH-5);
                StudioText(heroSlotLabels[i],28,i==2?CoronaFinish.Muted:CoronaFinish.Ivory,TextAnchor.MiddleCenter);
                heroSlotLabels[i].transform.SetAsLastSibling();
            }
            float ty=heroH+4f;
            StudioPlate("troop dossier",left,0,ty,lw,troopH-9,StudioSlate,true);
            Place(marchTroopChapter.rectTransform,18,ty+5,lw-36,44);
            StudioText(marchTroopChapter,30,StudioBrass,TextAnchor.MiddleLeft,true);
            float artWidth=lw*.21f;
            Place(approvedTroopRow.rectTransform,16,ty+51,artWidth,132);
            float infoX=artWidth+28f;
            Place(unitDetail.rectTransform,infoX,ty+51,lw-infoX-248,132);
            StudioText(unitDetail,30,CoronaFinish.Ivory,TextAnchor.MiddleLeft);
            Place((RectTransform)tierButton.transform,lw-240,ty+73,224,92);
            StudioButton(tierButton,StudioInk,27);
            float qy=ty+197f,qh=94f,qw=lw-32f;
            float modeW=qw*.33f,quantityW=qw-modeW-12f;
            Place((RectTransform)percentModeButton.transform,16,qy,modeW,qh);
            StudioButton(percentModeButton,StudioInk,28);
            Place(quantityRow,16+modeW+12,qy,quantityW,qh);
            StudioPlate("quantity inset",quantityRow,0,0,quantityW,qh,StudioInk);
            Place((RectTransform)minus.transform,0,0,94,qh);
            Place((RectTransform)plus.transform,quantityW-94,0,94,qh);
            StudioButton(minus,StudioSlate,38);StudioButton(plus,StudioSlate,38);
            Place(selectedText.rectTransform,99,0,quantityW-198,qh);
            StudioText(selectedText,42,CoronaFinish.Ivory,TextAnchor.MiddleCenter);
            float sliderY=qy+qh+10;
            Place((RectTransform)slider.transform,24,sliderY,lw-48,92);
            var track=slider.GetComponent<Image>();
            if(track)
            {
                var colour=track.color;studioRestore.Add(()=>{if(track)track.color=colour;});
                track.color=new Color(.025f,.04f,.05f,.75f);
            }
            var handle=slider.handleRect;
            if(handle)
            {
                var old=handle.sizeDelta;studioRestore.Add(()=>{if(handle)handle.sizeDelta=old;});
                handle.sizeDelta=new Vector2(56f,0f);
            }
            float rw=right.rect.width,rh=right.rect.height;
            StudioPlate("command scorecard",right,0,0,rw,rh,StudioInk,true);
            Place(summary.rectTransform,18,7,rw-36,51);
            StudioText(summary,30,StudioBrass,TextAnchor.MiddleLeft,true);
            float tileTop=61,tileGap=8,statusH=151;
            float tileW=(rw-32f-2*tileGap)/3f,tileH=(rh-tileTop-statusH-20f-tileGap)*.5f;
            for(int i=0;i<6;i++)
            {
                var card=marchMetricCards[i];
                Place(card,16+(i%3)*(tileW+tileGap),tileTop+(i/3)*(tileH+tileGap),tileW,tileH);
                StudioPlate("metric tile "+i,card,0,0,tileW,tileH,new Color32(30,52,60,244));
                if(!studioMetricCaptions[i])
                    studioMetricCaptions[i]=Text("Studio / real metric label "+i,card,28,CoronaFinish.Muted);
                var cap=studioMetricCaptions[i];cap.gameObject.SetActive(true);
                cap.font=CoronaFinish.ResolveFont(false);cap.fontStyle=FontStyle.Normal;
                cap.color=CoronaFinish.Muted;cap.alignment=TextAnchor.MiddleCenter;
                Place(cap.rectTransform,4,4,tileW-8,tileH*.34f);
                var value=marchMetricValues[i];
                Place(value.rectTransform,4,tileH*.35f,tileW-8,tileH*.60f);
                StudioText(value,38,i==2?StudioBrass:(Color)CoronaFinish.Ivory,TextAnchor.MiddleCenter);
            }
            float sy=rh-statusH-4f;
            StudioPlate("readiness strip",right,12,sy,rw-24,statusH-6,StudioSlate);
            Place(status.rectTransform,22,sy+6,rw-44,statusH-18);
            StudioText(status,29,CoronaFinish.Ivory,TextAnchor.MiddleLeft);
            Place((RectTransform)action.transform,12,24,width-24,92);
            var actionImage=action.targetGraphic as Image;
            if(actionImage)
            {
                var sprite=actionImage.sprite;var type=actionImage.type;var c=actionImage.color;
                var preserve=actionImage.preserveAspect;var r=actionImage.rectTransform;
                var amin=r.anchorMin;var amax=r.anchorMax;var pivot=r.pivot;
                var position=r.anchoredPosition;var size=r.sizeDelta;
                studioRestore.Add(()=>{if(!actionImage)return;actionImage.sprite=sprite;actionImage.type=type;
                    actionImage.color=c;actionImage.preserveAspect=preserve;r.anchorMin=amin;r.anchorMax=amax;
                    r.pivot=pivot;r.anchoredPosition=position;r.sizeDelta=size;});
                actionImage.sprite=null;actionImage.type=Image.Type.Simple;actionImage.color=StudioTeal;
                actionImage.preserveAspect=false;
                r.anchorMin=Vector2.zero;r.anchorMax=Vector2.one;r.offsetMin=r.offsetMax=Vector2.zero;
            }
            StudioText(actionLabel,34,CoronaFinish.Ivory,TextAnchor.MiddleCenter);
            actionLabel.transform.SetAsLastSibling();
            Place(feedback.rectTransform,12,0,width-24,24);
            StudioText(feedback,26,CoronaFinish.Ivory,TextAnchor.MiddleCenter);
        }
    }
}
