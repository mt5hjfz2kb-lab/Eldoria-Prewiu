using System.Collections.Generic;
using System.Linq;
using Eldoria.UI.RoyalGold;
using Eldoria.UI.RoyalGold.V2;
using UnityEngine;
using UnityEngine.UI;

namespace Eldoria.BreachWatch
{
    /// <summary>
    /// Staged replacement of the *visible* canonical mailbox art, never a
    /// second mailbox or a full-screen overlay. The existing gateway, paging,
    /// reports, receipts, message lists and controls are the source of truth.
    /// A screen is approved only after native QA; this class alone does not
    /// expose the destination in the main Vigilia HUD.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class BreachWatchMailboxTheme : MonoBehaviour
    {
        const string Stone = "VigiliaArt/PremiumSurfaces/panel_fill";
        const string Burgundy = "VigiliaArt/PremiumSurfaces/burgundy_action";
        readonly Dictionary<Graphic, bool> originalEnabled =
            new Dictionary<Graphic, bool>();
        readonly Dictionary<Button, Graphic> originalTargets =
            new Dictionary<Button, Graphic>();
        readonly Dictionary<Button, Selectable.Transition> originalTransitions =
            new Dictionary<Button, Selectable.Transition>();
        readonly List<GameObject> newArtwork = new List<GameObject>();
        readonly Dictionary<RoyalText, Color> originalInkColors =
            new Dictionary<RoyalText, Color>();
        Sprite panel, action, nineSlicedPanel;
        Image backdrop;
        Color originalBackdropColor;
        RoyalText realEmptySentence;
        RoyalText realListSummary;
        bool originalSummaryVisible;
        RoyalText pageCaption;
        bool originalPageCaptionVisible;
        RectTransform emptyLabelRect;
        Vector2 originalEmptyPosition,originalEmptySize;
        Vector2 originalEmptyAnchorMin,originalEmptyAnchorMax,originalEmptyPivot;
        bool emptyLabelMoved;
        BreachWatchMailboxEmptyArtwork emptyIllustration;
        RectTransform actualWindow;
        // Unlike the red inbox rows, the real report is ink on LIGHT
        // parchment. Never globally tint its dark text ivory.
        RectTransform originalReportSheet;
        bool styled;

        public bool IsStyled => styled;
        public int ReplacedOldPaintings { get; private set; }
        public int ReskinnedButtons { get; private set; }
        public bool OldBackdropHidden => styled &&
            AllOldMailboxBackgroundsHidden();

        public bool Apply(RectTransform realWindow)
        {
            if(styled) return true;
            if(!realWindow || realWindow.name!="V41 / Buzón destination")
                return false;
            panel=Resources.Load<Sprite>(Stone);
            action=Resources.Load<Sprite>(Burgundy);
            if(!panel || !action) return false;
            actualWindow=realWindow;
            backdrop=realWindow.GetComponent<Image>();
            if(!backdrop)return false;
            originalBackdropColor=backdrop.color;
            backdrop.color=new Color(.082f,.075f,.068f,1f);
            // A genuinely empty mailbox left almost 55% of the phone as
            // featureless black. Replace its obsolete matte with ONE
            // continuous mineral folio from the same Vigilia material kit.
            // Keep the real list, messages and tab hit targets above it.
            var folio=AddSurface(realWindow,panel,
                "Vigilia / one continuous charcoal stone folio",false);
            var folioRect=folio.rectTransform;
            folioRect.offsetMin=new Vector2(9f,9f);
            folioRect.offsetMax=new Vector2(-9f,-9f);
            folio.raycastTarget=false;
            ReplacedOldPaintings=0;
            ReskinnedButtons=0;

            foreach(var g in realWindow.GetComponentsInChildren<Graphic>(true))
            {
                if(!g || g.transform==realWindow)continue;
                // Preserve genuine report diagrams, hero/scout pictures,
                // icons, live labels and scrolling controls.
                bool oldMatte=g is RoyalGoldV41MailboxBackdrop;
                bool oldWatermark=g.name=="Mailbox / passive manuscript watermark";
                bool oldTabOrRow=g is Image &&
                    (g.name=="Approved source / tab-idle" ||
                     g.name=="Approved source / mail-card");
                bool oldPanel=g is Image &&
                    (g.name=="Mailbox / adaptive collection frame" ||
                     g.name=="Mailbox / report document");
                bool oldRim=g is Image &&
                    g.name.EndsWith(" / independent metal nine-slice");
                if(!oldMatte&&!oldWatermark&&!oldTabOrRow&&!oldPanel&&!oldRim)
                    continue;
                Hide(g);
                ReplacedOldPaintings++;
                if(oldPanel)
                {
                    var rect=g.transform as RectTransform;
                    if(rect) AddSurface(rect,panel,"Vigilia / replacement stone document",false);
                }
            }

            // Replace the actual button graphic and raycast surface. No new
            // Button, action, fake tab, mailbox database or layered blue paper.
            foreach(var button in realWindow.GetComponentsInChildren<Button>(true))
            {
                if(!button)continue;
                var rect=button.transform as RectTransform;
                if(!rect)continue;
                var old=rect.GetComponent<RoyalGoldV2FrameGraphic>();
                if(old)Hide(old);
                if(button.targetGraphic && button.targetGraphic!=old)
                    Hide(button.targetGraphic);
                originalTargets[button]=button.targetGraphic;
                originalTransitions[button]=button.transition;
                var paint=AddSurface(rect,action,"Vigilia / real mailbox button",true);
                paint.raycastTarget=true;
                button.targetGraphic=paint;
                button.transition=Selectable.Transition.None;
                ReskinnedButtons++;
            }
            foreach(var text in realWindow.GetComponentsInChildren<RoyalText>(true))
                if(text)originalInkColors[text]=text.color;
            originalReportSheet=realWindow.GetComponentsInChildren<RectTransform>(true)
                .FirstOrDefault(x=>x && x.name==
                    "Mailbox / scrollable assembled report");
            RefreshInk();
            // A small stamp in a 1600px black void was not an art-directed
            // layout. Change technique: replace the entire empty composition
            // with ONE genuine vector manuscript vignette in the canonical
            // stone hall. The source message, unread count and tabs remain
            // untouched. When mail arrives the illustration vanishes.
            foreach(var text in realWindow.GetComponentsInChildren<RoyalText>(true))
            {
                if(!text)continue;
                if(text.name=="Mailbox / real collection summary")
                {
                    realListSummary=text;
                    originalSummaryVisible=text.enabled;
                }
                if(text.name=="Mailbox / genuine empty state")
                {
                    realEmptySentence=text;
                    emptyLabelRect=text.rectTransform;
                    originalEmptyPosition=emptyLabelRect.anchoredPosition;
                    originalEmptySize=emptyLabelRect.sizeDelta;
                    originalEmptyAnchorMin=emptyLabelRect.anchorMin;
                    originalEmptyAnchorMax=emptyLabelRect.anchorMax;
                    originalEmptyPivot=emptyLabelRect.pivot;
                }
                if(text.name=="Página")
                {
                    pageCaption=text;
                    originalPageCaptionVisible=text.enabled;
                }
            }
            emptyIllustration=BreachWatchMailboxEmptyArtwork.Install(realWindow);
            if(emptyIllustration)newArtwork.Add(emptyIllustration.gameObject);
            RefreshEmptyComposition();
            styled=ReplacedOldPaintings>=4 && ReskinnedButtons>=3 &&
                AllOldMailboxBackgroundsHidden();
            if(!styled) Restore();
            return styled;
        }

        void Hide(Graphic old)
        {
            if(!old)return;
            if(!originalEnabled.ContainsKey(old))
                originalEnabled.Add(old,old.enabled);
            old.enabled=false;
        }

        Image AddSurface(RectTransform parent,Sprite sprite,string name,bool isAction)
        {
            var go=new GameObject(name,typeof(RectTransform),typeof(CanvasRenderer),typeof(Image));
            go.layer=parent.gameObject.layer;
            var rect=go.GetComponent<RectTransform>();
            rect.SetParent(parent,false);
            rect.anchorMin=Vector2.zero; rect.anchorMax=Vector2.one;
            rect.offsetMin=Vector2.zero;rect.offsetMax=Vector2.zero;
            // Canonical mailbox Refresh() finds the FIRST Image under its
            // category button and updates that old tab sprite. Keep the
            // disabled original first, draw our replacement second, and
            // leave real text above it. Otherwise each refresh would
            // overwrite our new burgundy art with the old blue tab.
            bool hasLegacyTabImage=false;
            if(isAction)
                foreach(var old in parent.GetComponentsInChildren<Image>(true))
                    if(old && old.transform.parent==parent &&
                       (old.name=="Approved source / tab-idle" ||
                        old.name=="Approved source / mail-card"))
                    {hasLegacyTabImage=true;break;}
            rect.SetSiblingIndex(isAction && hasLegacyTabImage?1:0);
            var img=go.GetComponent<Image>();
            if(!isAction)
            {
                if(!nineSlicedPanel)
                {
                    // Authored blank stone lacks a configured border. Slicing
                    // keeps the gilded corners and mineral texture undistorted.
                    var source=panel.rect;
                    float edge=Mathf.Min(44f,Mathf.Min(source.width,source.height)*.12f);
                    nineSlicedPanel=Sprite.Create(panel.texture,source,
                        new Vector2(.5f,.5f),panel.pixelsPerUnit,0,
                        SpriteMeshType.FullRect,new Vector4(edge,edge,edge,edge));
                    nineSlicedPanel.name="Vigilia / mailbox stone nine-slice";
                }
                img.sprite=nineSlicedPanel;
            }
            else img.sprite=sprite;
            img.type=Image.Type.Sliced;
            img.color=Color.white;
            img.raycastTarget=isAction;
            newArtwork.Add(go);
            return img;
        }

        public void SynchronizeVisual()
        {
            if(!styled)return;
            RefreshInk();
            RefreshEmptyComposition();
        }
        void LateUpdate() { SynchronizeVisual(); }

        void RefreshEmptyComposition()
        {
            if(!realEmptySentence)return;
            // The source layout is correct when mail exists. Only genuine
            // empty categories get this separate high-impact composition.
            // Never hide report rows, fabricated content, or actual actions.
            bool actuallyEmpty=realEmptySentence.gameObject.activeInHierarchy;
            if(realListSummary)
                realListSummary.enabled=originalSummaryVisible&&!actuallyEmpty;
            if(pageCaption)
                pageCaption.enabled=originalPageCaptionVisible&&!actuallyEmpty;
            if(emptyIllustration)
                emptyIllustration.gameObject.SetActive(actuallyEmpty);
            if(!emptyLabelRect || !actualWindow)return;
            if(actuallyEmpty)
            {
                float w=actualWindow.rect.width,h=actualWindow.rect.height;
                // Center the ONE real "No hay mensajes ..." sentence below
                // the engraved envelope scene. A miniature empty-state label
                // at the top with 65% dead space was the actual visual defect.
                bool portrait=h>w&&h>=530f;
                bool spaciousLandscape=w>=1100f&&h>=650f&&w>h;
                if(portrait || spaciousLandscape)
                {
                    emptyLabelRect.anchorMin=emptyLabelRect.anchorMax=
                        emptyLabelRect.pivot=new Vector2(0f,1f);
                    // Desktop: real heading and tabs end at ~22% of height.
                    // The original authored letter occupies the centre;
                    // its one genuine empty-category label belongs BELOW,
                    // never across the wax seal or behind the tab rail.
                    emptyLabelRect.anchoredPosition=portrait
                        ? new Vector2(w*.10f,-h*.647f)
                        : new Vector2(w*.19f,-h*.755f);
                    emptyLabelRect.sizeDelta=portrait
                        ? new Vector2(w*.80f,Mathf.Clamp(h*.071f,42f,88f))
                        : new Vector2(w*.62f,Mathf.Clamp(h*.066f,42f,70f));
                    emptyLabelMoved=true;
                }
                else if(emptyLabelMoved)RestoreEmptyLabel();
            }
            else if(emptyLabelMoved)RestoreEmptyLabel();
        }

        void RestoreEmptyLabel()
        {
            if(!emptyLabelRect)return;
            emptyLabelRect.anchorMin=originalEmptyAnchorMin;
            emptyLabelRect.anchorMax=originalEmptyAnchorMax;
            emptyLabelRect.pivot=originalEmptyPivot;
            emptyLabelRect.anchoredPosition=originalEmptyPosition;
            emptyLabelRect.sizeDelta=originalEmptySize;
            emptyLabelMoved=false;
        }

        void RefreshInk()
        {
            // The canonical mailbox Refresh() updates unread/read colors
            // using ink designed for light blue parchment. Keep live
            // category/status logic, but never let that ink disappear on
            // charcoal stone after a paged refresh.
            foreach(var pair in originalInkColors)
            {
                var text=pair.Key;
                if(!text)continue;
                // Native Unity 390x844 proved the old global ink replacement
                // made CONTENIDO RECIBIDO and full report body nearly white
                // on cream. Different physical materials need different ink.
                // Keep the canonical author's bronze/brown within the light
                // scroll sheet; recolor ONLY labels on dark stone/burgundy.
                if(originalReportSheet &&
                   text.transform.IsChildOf(originalReportSheet))continue;
                Color c=text.color;
                if(c.r<.44f && c.g<.44f && c.b<.44f && c.a>.2f)
                    text.color=new Color(.94f,.89f,.78f,c.a);
            }
        }

        bool AllOldMailboxBackgroundsHidden()
        {
            foreach(var g in GetComponentsInChildren<Graphic>(true))
            {
                if(!g || !g.isActiveAndEnabled || !g.gameObject.activeInHierarchy)
                    continue;
                if(g is RoyalGoldV41MailboxBackdrop ||
                   g.name=="Mailbox / passive manuscript watermark" ||
                   g.name.EndsWith(" / independent metal nine-slice"))
                    return false;
            }
            return true;
        }

        public void Restore()
        {
            foreach(var pair in originalTargets)
                if(pair.Key)pair.Key.targetGraphic=pair.Value;
            originalTargets.Clear();
            foreach(var pair in originalTransitions)
                if(pair.Key)pair.Key.transition=pair.Value;
            originalTransitions.Clear();
            foreach(var pair in originalEnabled)
                if(pair.Key)pair.Key.enabled=pair.Value;
            originalEnabled.Clear();
            foreach(var pair in originalInkColors)
                if(pair.Key)pair.Key.color=pair.Value;
            originalInkColors.Clear();
            RestoreEmptyLabel();
            if(realListSummary)realListSummary.enabled=originalSummaryVisible;
            if(pageCaption)pageCaption.enabled=originalPageCaptionVisible;
            realListSummary=null;
            realEmptySentence=null;
            pageCaption=null;
            emptyIllustration=null;
            emptyLabelRect=null;
            actualWindow=null;
            originalReportSheet=null;
            if(backdrop)backdrop.color=originalBackdropColor;
            backdrop=null;
            foreach(var obj in newArtwork)
                if(obj)Destroy(obj);
            newArtwork.Clear();
            styled=false;
            ReplacedOldPaintings=0;
            ReskinnedButtons=0;
        }

        void OnDestroy()
        {
            Restore();
            if(nineSlicedPanel)Destroy(nineSlicedPanel);
        }
    }
}
