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
            CoronaFinish.FitSingleLine(studioMetricCaptions[i],28,25);
            CoronaFinish.FitSingleLine(marchMetricValues[i],36,29);
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
            float gap=12f,heroH=bodyH*.31f,troopH=bodyH*.32f;
            float summaryH=bodyH-heroH-troopH-gap;
            Place(left,x,bodyY,width,heroH+troopH);
            Place(right,x,bodyY+heroH+troopH+gap,width,summaryH);
            float lw=left.rect.width;
            StudioPlate("hero dossier",left,0,0,lw,heroH-5,StudioSlate,true,"panel_obsidian");
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
                StudioPlate("hero slot "+i,card,1,1,cardW-2,cardH-2,StudioInk,true,"panel_obsidian");
                float captionH=72f,artW2=cardW-16,artH2=cardH-captionH-14;
                Place(heroPortraitViewports[i],8,8,artW2,artH2);
                var sprite=approvedHeroes[i].sprite;
                float ar=sprite?sprite.rect.width/sprite.rect.height:1f;
                bool portrait=!string.IsNullOrEmpty(heroSlots[i]);
                float pw=portrait?Mathf.Max(artW2,artH2*ar):Mathf.Min(artW2,artH2*ar);
                float ph=pw/ar;
                Place(approvedHeroes[i].rectTransform,(artW2-pw)*.5f,portrait?0:(artH2-ph)*.5f,pw,ph);
                // Fine original Corona frame; never place a fake portrait over the real one.
                var rim=StudioPlate("hero rim "+i,card,1,1,cardW-2,cardH-2,Color.white,true,"portrait_ring");
                if(rim.type==Image.Type.Sliced)rim.fillCenter=false;
                rim.transform.SetAsLastSibling();
                var ribbon=StudioPlate("hero caption "+i,card,6,cardH-captionH,cardW-12,captionH-5,StudioInk,true,"secondary_quiet");
                ribbon.transform.SetAsLastSibling();
                Place(heroSlotLabels[i].rectTransform,7,cardH-captionH,cardW-14,captionH-5);
                StudioText(heroSlotLabels[i],28,i==2?CoronaFinish.Muted:CoronaFinish.Ivory,TextAnchor.MiddleCenter);
                heroSlotLabels[i].transform.SetAsLastSibling();
            }
            float ty=heroH+4f;
            StudioPlate("troop dossier",left,0,ty,lw,troopH-9,StudioSlate,true,"panel_obsidian");
            Place(marchTroopChapter.rectTransform,18,ty+5,lw-36,44);
            StudioText(marchTroopChapter,30,StudioBrass,TextAnchor.MiddleLeft,true);
            float artWidth=lw*.23f;
            Place(approvedTroopRow.rectTransform,16,ty+51,artWidth,140);
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
            StudioPlate("quantity inset",quantityRow,0,0,quantityW,qh,StudioInk,true,"secondary_quiet");
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
                var colour=track.color;var ray=track.raycastTarget;
                studioRestore.Add(()=>{if(track){track.color=colour;track.raycastTarget=ray;}});
                track.color=new Color(.025f,.04f,.05f,.06f);
                track.raycastTarget=true;
                var rail=StudioPlate("slider rail",(RectTransform)slider.transform,6,30,lw-60,32,StudioInk,true,"wallet_inset");
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
            float tileW=(rw-32f-tileGap)/2f,tileH=(rh-tileTop-statusH-18f-2*tileGap)/3f;
            for(int i=0;i<6;i++)
            {
                var card=marchMetricCards[i];
                Place(card,16+(i%2)*(tileW+tileGap),tileTop+(i/2)*(tileH+tileGap),tileW,tileH);
                var tile=StudioPlate("metric tile "+i,card,0,0,tileW,tileH,StudioInk,true,"secondary_quiet");
                tile.color=i==2?new Color(.99f,.94f,.79f,1f):new Color(.85f,.88f,.87f,.96f);
                if(!studioMetricCaptions[i])
                    studioMetricCaptions[i]=Text("Studio / real metric label "+i,card,28,CoronaFinish.Muted);
                var cap=studioMetricCaptions[i];cap.gameObject.SetActive(true);
                cap.font=CoronaFinish.ResolveFont(false);cap.fontStyle=FontStyle.Normal;
                cap.color=CoronaFinish.Muted;cap.alignment=TextAnchor.MiddleCenter;
                Place(cap.rectTransform,8,4,tileW-16,tileH*.39f-3f);
                var value=marchMetricValues[i];
                Place(value.rectTransform,8,tileH*.39f,tileW-16,tileH*.61f-4f);
                StudioText(value,36,i==2?StudioBrass:(Color)CoronaFinish.Ivory,TextAnchor.MiddleCenter);
            }
            float sy=rh-statusH-4f;
            StudioPlate("readiness strip",right,12,sy,rw-24,statusH-6,StudioSlate,true,"secondary_quiet");
            Place(status.rectTransform,22,sy+6,rw-44,statusH-18);
            StudioText(status,26,CoronaFinish.Ivory,TextAnchor.MiddleLeft);
            Place((RectTransform)action.transform,12,28,width-24,96);
            StudioButton(action,StudioTeal,34,"action_arcane");
            StudioText(actionLabel,34,CoronaFinish.Ivory,TextAnchor.MiddleCenter);
            actionLabel.transform.SetAsLastSibling();
            Place(feedback.rectTransform,12,0,width-24,24);
            StudioText(feedback,26,CoronaFinish.Ivory,TextAnchor.MiddleCenter);
        }
    }
}
