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
        bool applied,busy;
        readonly TextGenerator textSize=new TextGenerator(256);
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
            if(dimmer)oldDimmer=dimmer.color;
            return true;
        }
        void Restore()
        {
            if(!applied)return;applied=false;
            if(paper)paper.enabled=false;
            if(aperture)aperture.gameObject.SetActive(false);
            if(v3Paper)v3Paper.enabled=true;
            if(priorBust)priorBust.enabled=true;
            if(dimmer)dimmer.color=oldDimmer;
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
            float width=Mathf.Min(safe.rect.width-(landscape?26f:18f),landscape?1100f:730f);
            var nav=Source.Navigation?Source.Navigation.transform as RectTransform:null;
            float dock=nav?Mathf.Max(0f,nav.anchoredPosition.y+nav.rect.height+10f):0f;
            float maxHeight=Mathf.Max(155f,safe.rect.height-dock-18f);
            float height=Mathf.Min(maxHeight,landscape?345f:470f);
            float column=compact?170f:Mathf.Clamp(width*.265f,175f,290f);
            float x=compact?22f:column+15f;
            float textWidth=width-x-24f;
            float portraitSize=compact?Mathf.Min(235f,width*.33f):Mathf.Min(column+25f,height-25f);
            float portraitX=compact?-23f:-18f;
            float portraitY=compact?7f:Mathf.Max(5f,(height-portraitSize)*.45f);
            float bodyTop=compact?portraitY+portraitSize+17f:landscape?102f:116f;
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
            float minimum=Mathf.Min(height,compact?330f:landscape?270f:310f);
            float natural=bodyTop+Mathf.Max(needed,compact?68f:75f)+footer+14f;
            if(natural<height)
            {
                height=Mathf.Clamp(natural,minimum,height);
                available=height-bodyTop-footer;
            }
            portraitSize=compact?Mathf.Min(235f,width*.33f):Mathf.Min(column+25f,height-25f);
            portraitY=compact?7f:Mathf.Max(5f,(height-portraitSize)*.45f);
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
            paper.Configure(isOrin,isLyra,compact?0f:column);
            if(priorBust)priorBust.enabled=false;
            if(isAldric)
            {
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
                role.fontSize=Mathf.RoundToInt(Mathf.Clamp(16f/scale,16f,32f));
                if(isOrin)role.Put("Custodio del Relicario");
                Top(role.rectTransform,titleX+2f,27f+titleH,titleWidth-2f,
                    Mathf.Max(29f,role.fontSize*1.27f));
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