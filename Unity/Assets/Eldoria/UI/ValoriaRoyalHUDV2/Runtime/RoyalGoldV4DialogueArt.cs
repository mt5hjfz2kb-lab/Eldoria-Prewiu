using System;
using System.Linq;
using Eldoria.UI.RoyalGold;
using UnityEngine;
using UnityEngine.UI;
namespace Eldoria.UI.RoyalGold.V2
{
    // One real RoyalHUD, one ModalWindow, untouched narrative state and Continue.
    [DisallowMultipleComponent]
    [DefaultExecutionOrder(725)]
    public sealed class RoyalGoldV4DialogueArt : MonoBehaviour
    {
        public RoyalHUD Source;
        RectTransform frame,safe,aperture;
        RoyalGoldV4DialogueParchment paper;
        RoyalGoldV3ParchmentGraphic v3Paper;
        RawImage priorBust,bust;
        Image lyra;
        RoyalText sigil,role;
        Graphic dimmer;
        Color oldDimmer;
        Canvas canvas;
        Material feather, lyraFeather;
        Sprite lyraSprite;
        bool applied,busy,oldSigilActive,oldBustEnabled,oldV3PaperEnabled,roleOverridden;
        string canonicalRoleBeforeOverride;
        float originalBodyLineSpacing;
        Geometry[] originalGeometry;
        Typography[] originalTypography;
        readonly TextGenerator textSize=new TextGenerator(256);
        // V4 is a reversible visual layer. The same canonical ModalWindow must
        // return to V3 precisely when QA alternates A/B or the component stops.
        struct Geometry
        {
            readonly RectTransform target;
            readonly Vector2 anchorMin,anchorMax,pivot,position,size;
            public Geometry(RectTransform r)
            {
                target=r;
                anchorMin=r?r.anchorMin:Vector2.zero;
                anchorMax=r?r.anchorMax:Vector2.zero;
                pivot=r?r.pivot:Vector2.zero;
                position=r?r.anchoredPosition:Vector2.zero;
                size=r?r.sizeDelta:Vector2.zero;
            }
            public void Restore()
            {
                if(!target)return;
                target.anchorMin=anchorMin;target.anchorMax=anchorMax;
                target.pivot=pivot;target.anchoredPosition=position;
                target.sizeDelta=size;
            }
        }
        struct Typography
        {
            readonly RoyalText target;
            readonly int fontSize;
            readonly Color color;
            readonly TextAnchor alignment;
            readonly float lineSpacing;
            readonly HorizontalWrapMode horizontal;
            readonly VerticalWrapMode vertical;
            public Typography(RoyalText t)
            {
                target=t;
                fontSize=t?t.fontSize:0;color=t?t.color:Color.white;
                alignment=t?t.alignment:TextAnchor.UpperLeft;
                lineSpacing=t?t.lineSpacing:1f;
                horizontal=t?t.horizontalOverflow:HorizontalWrapMode.Wrap;
                vertical=t?t.verticalOverflow:VerticalWrapMode.Truncate;
            }
            public void Restore()
            {
                if(!target)return;
                target.fontSize=fontSize;target.color=color;
                target.alignment=alignment;target.lineSpacing=lineSpacing;
                target.horizontalOverflow=horizontal;target.verticalOverflow=vertical;
            }
        }
        void CaptureBaseline()
        {
            var modal=Source.Modal;
            originalGeometry=new[]
            {
                new Geometry(frame),
                new Geometry(modal.Title.rectTransform),
                new Geometry(modal.Body.rectTransform),
                new Geometry(modal.Extra.transform as RectTransform),
                new Geometry(modal.ExtraLabel.rectTransform),
                new Geometry(role?role.rectTransform:null),
                new Geometry(sigil?sigil.rectTransform:null)
            };
            originalTypography=new[]
            {
                new Typography(modal.Title),new Typography(modal.Body),
                new Typography(modal.ExtraLabel),new Typography(role),
                new Typography(sigil)
            };
            oldDimmer=dimmer?dimmer.color:Color.clear;
            oldSigilActive=sigil&&sigil.gameObject.activeSelf;
            oldBustEnabled=priorBust&&priorBust.enabled;
            oldV3PaperEnabled=v3Paper&&v3Paper.enabled;
            originalBodyLineSpacing=modal.Body.lineSpacing;
        }
        static readonly Color parchmentInk=new Color(.30f,.19f,.115f);
        static readonly Color gold=new Color(.96f,.88f,.70f);
        static void Top(RectTransform r,float x,float y,float w,float h)
        {
            r.anchorMin=r.anchorMax=r.pivot=new Vector2(0,1);
            r.anchoredPosition=new Vector2(x,-y);
            r.sizeDelta=new Vector2(Mathf.Max(0,w),Mathf.Max(0,h));
        }
        static void Stretch(RectTransform r)
        {
            r.anchorMin=Vector2.zero;r.anchorMax=Vector2.one;
            r.offsetMin=r.offsetMax=Vector2.zero;
        }
        void Awake(){if(!Source)Source=GetComponent<RoyalHUD>();}
        void OnEnable(){Canvas.willRenderCanvases+=SynchronizeNow;}
        void OnDisable(){Canvas.willRenderCanvases-=SynchronizeNow;Restore();}
        void OnDestroy(){if(feather)Destroy(feather);if(lyraFeather)Destroy(lyraFeather);}
        void LateUpdate(){SynchronizeNow();}
        bool Ensure()
        {
            if(!Source||!Source.Modal||!Source.Modal.Title)return false;
            if(frame&&paper&&aperture)return true;
            frame=Source.Modal.Title.rectTransform.parent as RectTransform;
            var layout=Source.GetComponentInChildren<RoyalHudLayout>(true);
            safe=layout?layout.transform as RectTransform:null;
            canvas=Source.GetComponent<Canvas>();
            if(!safe||!frame||!canvas)return false;
            role=frame.Find("Narrative character role")?
                frame.Find("Narrative character role").GetComponent<RoyalText>():null;
            sigil=frame.Find("Narrator sigil")?
                frame.Find("Narrator sigil").GetComponent<RoyalText>():null;
            priorBust=frame.Find("Canonical Aldric portrait")?
                frame.Find("Canonical Aldric portrait").GetComponent<RawImage>():null;
            var old=frame.Cast<Transform>().FirstOrDefault(t=>
                t.name=="V3 / cinematic royal parchment") as RectTransform;
            v3Paper=old?old.GetComponent<RoyalGoldV3ParchmentGraphic>():null;
            var parchmentNode=new GameObject("Dialogue / layered manuscript",
                typeof(RectTransform),typeof(CanvasRenderer),typeof(RoyalGoldV4DialogueParchment));
            parchmentNode.layer=frame.gameObject.layer;
            parchmentNode.transform.SetParent(frame,false);
            parchmentNode.transform.SetAsFirstSibling();
            paper=parchmentNode.GetComponent<RoyalGoldV4DialogueParchment>();
            paper.raycastTarget=false;Stretch(paper.rectTransform);paper.enabled=false;
            var maskNode=new GameObject("Dialogue / genuine portrait aperture",
                typeof(RectTransform),typeof(RectMask2D));
            maskNode.layer=frame.gameObject.layer;maskNode.transform.SetParent(frame,false);
            aperture=(RectTransform)maskNode.transform;
            aperture.SetSiblingIndex(Mathf.Min(2,frame.childCount-1));
            var a=new GameObject("Dialogue / real Aldric bust",
                typeof(RectTransform),typeof(CanvasRenderer),typeof(RawImage));
            a.layer=frame.gameObject.layer;a.transform.SetParent(aperture,false);
            bust=a.GetComponent<RawImage>();bust.raycastTarget=false;
            Stretch(bust.rectTransform);
            var l=new GameObject("Dialogue / approved Lyra portrait",
                typeof(RectTransform),typeof(CanvasRenderer),typeof(Image));
            l.layer=frame.gameObject.layer;l.transform.SetParent(aperture,false);
            lyra=l.GetComponent<Image>();lyra.preserveAspect=true;lyra.raycastTarget=false;
            Stretch(lyra.rectTransform);
            var shader=Shader.Find("UI/Eldoria/HallPainterly");
            if(shader)
            {
                feather=new Material(shader);
                if(feather.HasProperty("_PortraitFeather"))feather.SetFloat("_PortraitFeather",.18f);
                if(feather.HasProperty("_PortraitBleed"))feather.SetFloat("_PortraitBleed",.08f);
                bust.material=feather;
                // Lyra is sourced from a verified sprite subregion of the
                // HeroesApprovedMaster, not a 0..1 UV photograph. A separate
                // material keeps her true sprite bounds independent of
                // Aldric's original 3D bust and other Royal Hall portraits.
                lyraFeather=new Material(feather);
                if(lyraFeather.HasProperty("_PortraitFeather"))
                    lyraFeather.SetFloat("_PortraitFeather",.22f);
                lyra.material=lyraFeather;
            }
            aperture.gameObject.SetActive(false);
            dimmer=Source.Modal.GetComponent<Graphic>();
            return true;
        }
        void Restore()
        {
            if(!applied)return;
            applied=false;
            if(paper)paper.enabled=false;
            if(aperture)aperture.gameObject.SetActive(false);
            if(v3Paper)v3Paper.enabled=oldV3PaperEnabled;
            if(priorBust)priorBust.enabled=oldBustEnabled;
            // The original narrative controller restores ordinary modal
            // geometry/backdrop on Close. Never undo its already-run restore.
            bool narrativeStillOpen=Source&&Source.NarrativeOpen;
            if(narrativeStillOpen)
            {
                if(originalGeometry!=null)
                    foreach(var g in originalGeometry)g.Restore();
                if(originalTypography!=null)
                    foreach(var t in originalTypography)t.Restore();
                if(sigil)sigil.gameObject.SetActive(oldSigilActive);
                if(dimmer)dimmer.color=oldDimmer;
            }
            else if(Source&&Source.Modal&&Source.Modal.Body)
                Source.Modal.Body.lineSpacing=originalBodyLineSpacing;
            if(roleOverridden&&role&&role.text=="Custodio del Relicario")
                role.Put(canonicalRoleBeforeOverride);
            roleOverridden=false;
        }
        float Height(RoyalText text,string full,float width)
        {
            var settings=text.GetGenerationSettings(new Vector2(width,0));
            return textSize.GetPreferredHeight(full??"",settings)/
                Mathf.Max(.001f,text.pixelsPerUnit)+7f;
        }
        public void SynchronizeNow()
        {
            if(busy||!UnityEngine.Application.isPlaying||!Source)return;
            if(!Source.NarrativeOpen){Restore();return;}
            if(!Ensure()||safe.rect.width<250f||safe.rect.height<270f)return;
            busy=true;
            try{Layout();}
            finally{busy=false;}
        }
        void Layout()
        {
            var modal=Source.Modal;
            var narrative=modal.GetComponent<RoyalNarrativePresentation>();
            if(!narrative||!modal.Title||!modal.Body||!modal.Extra||!modal.ExtraLabel)return;
            if(!applied)CaptureBaseline();
            applied=true;
            if(v3Paper)v3Paper.enabled=false;
            paper.enabled=true;
            if(dimmer)dimmer.color=new Color(.01f,.013f,.022f,.40f);
            string who=Source.NarrativeSpeaker??"";
            bool isOrin=who=="Orín"||who=="Orin"||who=="Maestre Orin"||who=="Orus";
            bool isLyra=who=="Lyra";
            bool isAldric=who=="Sir Aldric"||who=="Aldric";
            bool landscape=safe.rect.width>safe.rect.height;
            float scale=Mathf.Max(.18f,canvas.scaleFactor);
            bool compact=!landscape&&safe.rect.width*scale<610f;
            // The proprietor chose "Diálogo 1 / Estilo Real Narrativo":
            // a cinematic character emerging beside a broad manuscript.
            // Preserve the previous compact layout on 844x390 mobile;
            // a wide character portrait would conceal the text there.
            // Orín remains a distinct arcane QA treatment until the
            // story event actually exists.
            bool cinematic=landscape&&safe.rect.width*scale>=1050f&&
                (isAldric||isLyra);
            float width=Mathf.Min(safe.rect.width-(landscape?26f:18f),
                cinematic?1360f:landscape?1100f:730f);
            var nav=Source.Navigation?Source.Navigation.transform as RectTransform:null;
            float dock=nav?Mathf.Max(0f,nav.anchoredPosition.y+nav.rect.height+10f):0f;
            float maxHeight=Mathf.Max(155f,safe.rect.height-dock-18f);
            float height=Mathf.Min(maxHeight,cinematic?395f:landscape?345f:470f);
            float column=compact?170f:cinematic?
                Mathf.Clamp(width*.30f,315f,425f):
                Mathf.Clamp(width*.265f,175f,290f);
            float x=compact?22f:column+(cinematic?12f:15f);
            float textWidth=Mathf.Min(width-x-24f,cinematic?795f:float.MaxValue);
            // Aldric's genuine 512px approved bust can fill a large
            // vignette. Lyra's atlas portrait is smaller, so cap its
            // size to avoid visibly magnifying the source pixels.
            float portraitSize=cinematic?
                (isLyra?Mathf.Min(height*.95f,350f):Mathf.Min(height*1.18f,465f)):
                compact?Mathf.Min(235f,width*.33f):Mathf.Min(column+25f,height-25f);
            // Lyra's narrower approved sprite needs a little more overlap
            // with the manuscript; its alpha silhouette otherwise floats
            // tens of pixels away from the paper in real 1600x900 renders.
            float portraitX=cinematic?(isLyra?10f:-54f):compact?-23f:-18f;
            float portraitY=cinematic?(isLyra?-25f:-56f):
                compact?7f:Mathf.Max(5f,(height-portraitSize)*.45f);
            // The illustrated header tab spans roughly y=26..87 from the
            // frame's top. The previous role started at y=71, so its first
            // line was hidden behind that tab in native Unity screenshots.
            // Reserve a real subtitle lane below the tab, then put the
            // complete narrative below both the role and the portrait.
            float roleTop=92f;
            int roleFont=Mathf.RoundToInt(Mathf.Clamp(16f/scale,16f,32f));
            float roleHeight=Mathf.Max(29f,roleFont*1.27f);
            float bodyTop=Mathf.Max(roleTop+roleHeight+11f,
                compact?portraitY+portraitSize+17f:0f);
            float buttonH=Mathf.Clamp(48f/scale,48f,112f);
            float footer=buttonH+23f;
            float available=height-bodyTop-footer;
            modal.Body.fontSize=Mathf.RoundToInt(Mathf.Clamp(18.5f/scale,22f,42f));
            modal.Body.lineSpacing=1.12f;
            modal.Body.horizontalOverflow=HorizontalWrapMode.Wrap;
            modal.Body.verticalOverflow=VerticalWrapMode.Truncate;
            var full=narrative.CurrentPageText??"";
            float needed=Height(modal.Body,full,textWidth);
            int floor=Mathf.Max(18,Mathf.RoundToInt(15f/scale));
            while(needed>available&&modal.Body.fontSize>floor)
            {
                modal.Body.fontSize--;needed=Height(modal.Body,full,textWidth);
            }
            if(needed>available&&height<maxHeight)
            {
                var add=Mathf.Min(needed-available,maxHeight-height);
                height+=add;available+=add;
            }
            // Cinematic portraits should not collapse to tiny avatars on a
            // short line of dialogue. Keep the character composition stable.
            float minimum=Mathf.Min(height,cinematic?350f:
                compact?330f:landscape?270f:310f);
            float natural=bodyTop+Mathf.Max(needed,compact?68f:75f)+footer+14f;
            if(natural<height)
            {
                height=Mathf.Clamp(natural,minimum,height);
                available=height-bodyTop-footer;
            }
            portraitSize=cinematic?
                (isLyra?Mathf.Min(height*.95f,350f):Mathf.Min(height*1.18f,465f)):
                compact?Mathf.Min(235f,width*.33f):Mathf.Min(column+25f,height-25f);
            portraitY=cinematic?(isLyra?-25f:-56f):
                compact?7f:Mathf.Max(5f,(height-portraitSize)*.45f);
            if(cinematic)
            {
                // The panel can still contract to 350 units for short text.
                // Keep the actual image silhouette overlapping the real
                // manuscript edge at every length, not just long dialogues.
                float paperLeft=column*.68f;
                // The approved cinematic bust must visibly cross the paper
                // edge without entering the first text glyph. The old +70
                // rule yielded only a feather-thin contact once the model's
                // transparent source edge was dissolved; use a broader visual
                // overlap, with the text still starting at column + 12.
                portraitX=Mathf.Max(portraitX,paperLeft+135f-portraitSize*.96f);
            }
            frame.anchorMin=frame.anchorMax=new Vector2(.5f,0);
            frame.pivot=new Vector2(.5f,0);
            frame.sizeDelta=new Vector2(width,height);
            var parent=frame.parent as RectTransform;
            if(parent)
            {
                var corners=new Vector3[4];safe.GetWorldCorners(corners);
                var left=parent.InverseTransformPoint(corners[0]);
                var right=parent.InverseTransformPoint(corners[3]);
                frame.anchoredPosition=new Vector2((left.x+right.x)*.5f-parent.rect.center.x,
                    left.y-parent.rect.yMin+dock+9f);
            }
            Top(aperture,portraitX+portraitSize*.04f,portraitY+portraitSize*.04f,
                portraitSize*.92f,portraitSize*.92f);
            paper.Configure(isOrin,isLyra,compact?0f:column,cinematic);
            if(priorBust)priorBust.enabled=false;
            if(isAldric)
            {
                // In the full cinematic cutout there is no medallion to
                // hide the approved bust's bright outer shoulder edge.
                // Soften its genuine source alpha at the right silhouette;
                // mobile keeps the earlier, less aggressive feather.
                if(feather&&feather.HasProperty("_PortraitFeather"))
                    feather.SetFloat("_PortraitFeather",cinematic?.265f:.18f);
                Texture portrait=priorBust?priorBust.texture:null;
                if(!portrait)portrait=Resources.Load<Texture2D>("UI/AldricDialogue/aldric_neutral");
                bust.texture=portrait;bust.gameObject.SetActive(portrait!=null);
                lyra.gameObject.SetActive(false);aperture.gameObject.SetActive(portrait!=null);
            }
            else if(isLyra)
            {
                if(!lyraSprite)lyraSprite=RoyalGoldV41SourceArt.HeroPortrait("lyra");
                if(!lyraSprite)
                {
                    var source=Resources.Load<Texture2D>("EldoriaV41/HeroesApprovedMaster");
                    if(source&&source.width==1536&&source.height==1024)
                        lyraSprite=Sprite.Create(source,new Rect(1224,182,148,180),
                            new Vector2(.5f,.5f),100);
                }
                lyra.sprite=lyraSprite;lyra.gameObject.SetActive(lyraSprite!=null);
                if(lyraSprite&&lyraFeather)
                {
                    var texUV=lyraSprite.uv;
                    float minU=float.PositiveInfinity,minV=float.PositiveInfinity;
                    float maxU=float.NegativeInfinity,maxV=float.NegativeInfinity;
                    foreach(var uv in texUV)
                    {
                        minU=Mathf.Min(minU,uv.x);minV=Mathf.Min(minV,uv.y);
                        maxU=Mathf.Max(maxU,uv.x);maxV=Mathf.Max(maxV,uv.y);
                    }
                    if(minU<maxU&&minV<maxV)
                        lyraFeather.SetVector("_PortraitUVRect",
                            new Vector4(minU,minV,maxU,maxV));
                }
                bust.gameObject.SetActive(false);aperture.gameObject.SetActive(lyraSprite!=null);
            }
            else
            {
                bust.gameObject.SetActive(false);lyra.gameObject.SetActive(false);
                aperture.gameObject.SetActive(false);
            }
            if(sigil)
            {
                bool visible=!aperture.gameObject.activeSelf;
                sigil.gameObject.SetActive(visible);
                Top(sigil.rectTransform,portraitX,portraitY,portraitSize,portraitSize);
                sigil.color=isOrin?new Color(.78f,.55f,.98f):gold;
                sigil.fontSize=Mathf.RoundToInt(portraitSize*.52f);
            }
            float titleX=compact?portraitX+portraitSize+13f:x+9f;
            float titleWidth=compact?width-titleX-15f:textWidth-24f;
            modal.Title.fontSize=Mathf.RoundToInt(Mathf.Clamp(23f/scale,23f,43f));
            if(compact&&who=="NARRADOR")modal.Title.fontSize=Mathf.Min(32,modal.Title.fontSize);
            modal.Title.color=gold;modal.Title.alignment=TextAnchor.MiddleLeft;
            float titleH=Mathf.Max(44f,modal.Title.fontSize*1.31f);
            Top(modal.Title.rectTransform,titleX,compact?23f:25f,titleWidth,titleH);
            if(role)
            {
                role.color=isOrin?new Color(.95f,.85f,1f):isLyra?gold:parchmentInk;
                role.fontSize=roleFont;
                if(isOrin&&!roleOverridden&&role.text!="Custodio del Relicario")
                {
                    canonicalRoleBeforeOverride=role.text;
                    roleOverridden=true;
                    role.Put("Custodio del Relicario");
                }
                else if(!isOrin&&roleOverridden)
                {
                    if(role.text=="Custodio del Relicario")
                        role.Put(canonicalRoleBeforeOverride);
                    roleOverridden=false;
                }
                Top(role.rectTransform,titleX+2f,roleTop,titleWidth-2f,roleHeight);
            }
            modal.Body.color=isOrin?new Color(.96f,.89f,1f):isLyra?gold:parchmentInk;
            modal.Body.alignment=TextAnchor.UpperLeft;
            Top(modal.Body.rectTransform,x,bodyTop,textWidth,Mathf.Max(54f,available));
            float buttonW=Mathf.Min(Mathf.Clamp(width*(compact?.50f:.26f),165f,242f),width-40f);
            Top((RectTransform)modal.Extra.transform,width-buttonW-19f,
                height-buttonH-13f,buttonW,buttonH);
            modal.ExtraLabel.fontSize=Mathf.RoundToInt(Mathf.Clamp(18f/scale,19f,38f));
            modal.ExtraLabel.color=gold;modal.ExtraLabel.alignment=TextAnchor.MiddleCenter;
            Top(modal.ExtraLabel.rectTransform,7f,3f,buttonW-14f,buttonH-6f);
        }
    }
}