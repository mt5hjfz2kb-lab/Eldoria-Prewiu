using System;
using System.Collections.Generic;
using Eldoria.UI.RoyalGold;
using Eldoria.UI.RoyalGold.V2;
using UnityEngine;
using UnityEngine.UI;

namespace Eldoria.BreachWatch
{
    /// <summary>
    /// Reusable, reversible presentation for real destination screens.
    /// All gameplay controls, live text, existing callbacks and save state
    /// stay in their canonical owners. Only their visible UI surfaces change.
    /// The originating design is Vigilia; never use the decorated Bastion
    /// cartouche as a generic panel (its crest and rulers are baked in).
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class BreachWatchSharedTheme : MonoBehaviour
    {
        const string StonePath = "VigiliaArt/PremiumSurfaces/panel_fill";
        const string ActionPath = "VigiliaArt/PremiumSurfaces/burgundy_action";

        sealed class Original
        {
            public Graphic Graphic;
            public bool Enabled;
        }
        readonly List<Original> originalGraphics = new List<Original>();
        readonly List<GameObject> createdSurfaces = new List<GameObject>();
        readonly Dictionary<Button, Graphic> originalButtonTargets =
            new Dictionary<Button, Graphic>();
        readonly Dictionary<Button, Selectable.Transition> originalTransitions =
            new Dictionary<Button, Selectable.Transition>();
        Sprite slicedStone;
        Image originalBackdrop;
        Color originalBackdropColor;
        readonly Dictionary<Text, Color> originalTextColors =
            new Dictionary<Text, Color>();
        Sprite stone, action;
        bool installed;

        public bool Installed => installed;
        public int RestyledPanels { get; private set; }
        public int RestyledButtons { get; private set; }
        public int ReplacedLegacyGraphics { get; private set; }
        // Read-only native QA signal; identify exactly which source graphic
        // breaks replacement without weakening the visual acceptance gate.
        public string FirstVisibleOldPainting
        {
            get
            {
                foreach(var graphic in GetComponentsInChildren<Graphic>(true))
                {
                    if(!graphic || !graphic.isActiveAndEnabled ||
                       !graphic.gameObject.activeInHierarchy)continue;
                    if(graphic is RoyalGoldV3ManagementBackdrop ||
                       graphic is RoyalGoldV3ManagementBackdropGraphic ||
                       graphic is RoyalGoldV3LayeredFinish)
                        return graphic.name+" ("+graphic.GetType().Name+")";
                    if(graphic is Image &&
                       (graphic.name=="Approved source / blue-panel" ||
                        graphic.name=="Approved source / gold-button" ||
                        graphic.name=="Approved source / training-courtyard-layer-v1" ||
                        graphic.name=="Approved source / HeroCardNineSliceGold" ||
                        graphic.name.StartsWith("Training / genuine costs") ||
                        graphic.name.StartsWith("Training / actual timer") ||
                        graphic.name.StartsWith("Training / unit manuscript") ||
                        graphic.name.StartsWith("Training / live quantity instruments") ||
                        graphic.name.StartsWith("Training / one continuous military order") ||
                        graphic.name.StartsWith("Training / framed transparent bust")))
                        return graphic.name+" (Image)";
                }
                return "";
            }
        }
        public bool NoLegacyBluePaintingVisible
        {
            get
            {
                if(!installed)return false;
                var graphics=GetComponentsInChildren<Graphic>(true);
                foreach(var item in graphics)
                {
                    if(!item || !item.isActiveAndEnabled ||
                       !item.gameObject.activeInHierarchy)continue;
                    if(item is RoyalGoldV3ManagementBackdrop ||
                       item is RoyalGoldV3ManagementBackdropGraphic ||
                       item is RoyalGoldV3LayeredFinish)return false;
                    if(item is Image &&
                       (item.name=="Approved source / blue-panel" ||
                        item.name=="Approved source / gold-button" ||
                        item.name=="Approved source / training-courtyard-layer-v1" ||
                     item.name=="Approved source / HeroCardNineSliceGold" ||
                     item.name.StartsWith("Training / genuine costs") ||
                     item.name.StartsWith("Training / actual timer") ||
                     item.name.StartsWith("Training / unit manuscript") ||
                     item.name.StartsWith("Training / live quantity instruments") ||
                     item.name.StartsWith("Training / one continuous military order") ||
                     item.name.StartsWith("Training / framed transparent bust")))
                        return false;
                }
                return true;
            }
        }

        public bool Apply(RectTransform destination)
        {
            if(installed) return true;
            if(!destination) return false;
            // Fail closed. Missing assets must NEVER replace working legacy UI
            // with an opaque or pixelated placeholder.
            stone=Resources.Load<Sprite>(StonePath);
            action=Resources.Load<Sprite>(ActionPath);
            if(!stone || !action)
            {
                Debug.LogWarning("VIGILIA_DESTINATION_THEME_MISSING_ASSETS");
                return false;
            }
            RestyledPanels=0;
            RestyledButtons=0;
            ReplacedLegacyGraphics=0;
            // Replace the old blue canvas with a neutral warm-stone stage.
            // This is a single authoritative background, not a dark image
            // pasted on top of the previous military backdrop.
            originalBackdrop=destination.GetComponent<Image>();
            if(originalBackdrop)
            {
                originalBackdropColor=originalBackdrop.color;
                originalBackdrop.color=new Color(.080f,.071f,.064f,1f);
            }
            ReplaceLegacyPainting(destination);

            // Only structural destination frames. Other images include
            // genuine soldiers, heroes, resources and narrative illustrations.
            string[] frameNames={
                "V3 / Destination header",
                "V3 / Featured military unit",
                "V3 / Authoritative status and cost",
                "V3 / Destination actions"
            };
            var rects=destination.GetComponentsInChildren<RectTransform>(true);
            foreach(var rect in rects)
            {
                bool isFrame=false;
                foreach(string name in frameNames)
                    if(rect.name==name){isFrame=true;break;}
                if(!isFrame)continue;
                var older=rect.GetComponent<RoyalGoldV2FrameGraphic>();
                if(older) Hide(older);
                AddSurface(rect,stone,"Vigilia / one stone panel",false);
                RestyledPanels++;
            }

            // Preserve real buttons and their event handlers; only replace
            // the frame painting, with an actual raycastable target attached
            // to each existing Selectable. No second button is created.
            foreach(var button in destination.GetComponentsInChildren<Button>(true))
            {
                if(!button)continue;
                var rect=button.transform as RectTransform;
                if(!rect)continue;
                var older=rect.GetComponent<RoyalGoldV2FrameGraphic>();
                if(!older)continue;
                originalButtonTargets[button]=button.targetGraphic;
                originalTransitions[button]=button.transition;
                Hide(older);
                // A hero roster choice is a portrait card, not a giant
                // stretched red command. Keep its real Button/callback.
                bool heroChoice=rect.name.StartsWith("Hero slot ");
                var paint=AddSurface(rect,heroChoice?stone:action,
                    heroChoice?"Vigilia / hero selection stone":
                    "Vigilia / burgundy action",true);
                BreachWatchControlKit.Paint(paint,
                    heroChoice?BreachWatchControlKit.Role.Secondary:
                        BreachWatchControlKit.Role.Primary);
                button.targetGraphic=paint;
                // No ColorTint that changes the color of this approved
                // burgundy artwork when its canonical interactable state updates.
                button.transition=Selectable.Transition.None;
                RestyledButtons++;
            }
            installed=RestyledPanels==frameNames.Length && ReplacedLegacyGraphics>=4;
            if(!installed)
            {
                Restore();
                Debug.LogWarning("VIGILIA_DESTINATION_THEME_INCOMPLETE_FRAME_SET");
            }
            return installed;
        }

        void ReplaceLegacyPainting(RectTransform destination)
        {
            // The old Royal management UI had independently layered blue
            // scenery, parchment images, foreground meshes and heraldry.
            // Hide those genuine graphics BEFORE installing the new surfaces.
            // Do not hide functional text, hero portraits, troop illustrations
            // or the original Slider/Selectables.
            var original=destination.GetComponentsInChildren<Graphic>(true);
            foreach(var graphic in original)
            {
                if(!graphic || graphic.transform==destination)continue;
                bool obsoleteBackdrop=graphic is RoyalGoldV3ManagementBackdrop ||
                    graphic is RoyalGoldV3ManagementBackdropGraphic ||
                    graphic is RoyalGoldV3LayeredFinish;
                bool obsoleteTrainingPaper=graphic is Image &&
                    (graphic.name.StartsWith("Training / genuine costs") ||
                     graphic.name.StartsWith("Training / actual timer") ||
                     graphic.name.StartsWith("Training / unit manuscript") ||
                     graphic.name.StartsWith("Training / live quantity instruments") ||
                     graphic.name.StartsWith("Training / one continuous military order") ||
                     graphic.name.StartsWith("Training / framed transparent bust"));
                bool obsoletePaper=graphic is Image &&
                    (graphic.name=="Approved source / blue-panel" ||
                     graphic.name=="Approved source / gold-button" ||
                     graphic.name=="Approved source / HeroCardNineSliceGold" ||
                     graphic.name=="Approved source / training-courtyard-layer-v1");
                if(!obsoleteBackdrop && !obsoletePaper && !obsoleteTrainingPaper)
                    continue;
                Hide(graphic);
                ReplacedLegacyGraphics++;
                // Repaint only independent subcards; the four parent panels
                // receive their own stone surface below in Apply().
                if(obsoleteTrainingPaper)
                {
                    var rect=graphic.transform as RectTransform;
                    if(rect)AddSurface(rect,stone,"Vigilia / replaced training parchment",false);
                }
                else if(obsoletePaper && graphic.name=="Approved source / blue-panel")
                {
                    var parent=graphic.transform.parent as RectTransform;
                    if(parent && parent.name!="V3 / Destination header" &&
                       parent.name!="V3 / Featured military unit" &&
                       parent.name!="V3 / Authoritative status and cost" &&
                       parent.name!="V3 / Destination actions")
                        AddSurface(parent,stone,"Vigilia / replaced inset paper",false);
                }
            }

            // Blue-paper text was dark; it becomes illegible on real
            // charcoal stone. Retint live labels without modifying content.
            foreach(var text in destination.GetComponentsInChildren<RoyalText>(true))
            {
                if(!text)continue;
                originalTextColors[text]=text.color;
                var c=text.color;
                bool wasInk=(c.r+c.g+c.b)/3f<.49f;
                bool wasGold=c.r>c.b*1.3f && c.g>c.b*1.12f;
                text.color=wasGold
                    ?new Color(.94f,.75f,.45f,c.a)
                    :new Color(.97f,.93f,.84f,c.a);
            }
        }

        // The authoritative Training/March controller legitimately recomputes
        // its screen on every orientation and progress refresh. Its Reflow()
        // can re-enable original blue paper Images AFTER our first swap.
        // Enforce one visible painting before render instead of permanently
        // copying or replacing the gameplay controller's layout logic.
        public void ReassertSinglePainting()
        {
            if(!installed)return;
            // Canonical Reflow may add or turn on new blue-paper Images
            // after the initial Apply(). Re-scan the REAL destination, not
            // only the snapshot of components captured at installation.
            // This is the root cause found by Unity: 4 panels/14 buttons
            // styled, but a source blue-panel still painted after reflow.
            foreach(var graphic in GetComponentsInChildren<Graphic>(true))
            {
                if(!graphic || !graphic.gameObject.activeInHierarchy ||
                    !graphic.enabled)continue;
                bool legacyStage=graphic is RoyalGoldV3ManagementBackdrop ||
                    graphic is RoyalGoldV3ManagementBackdropGraphic ||
                    graphic is RoyalGoldV3LayeredFinish;
                bool legacyPaper=graphic is Image &&
                    (graphic.name=="Approved source / blue-panel" ||
                     graphic.name=="Approved source / gold-button" ||
                     graphic.name=="Approved source / HeroCardNineSliceGold" ||
                     graphic.name=="Approved source / training-courtyard-layer-v1" ||
                     graphic.name.StartsWith("Training / genuine costs") ||
                     graphic.name.StartsWith("Training / actual timer") ||
                     graphic.name.StartsWith("Training / unit manuscript") ||
                     graphic.name.StartsWith("Training / live quantity instruments") ||
                     graphic.name.StartsWith("Training / one continuous military order") ||
                     graphic.name.StartsWith("Training / framed transparent bust"));
                if(!legacyStage&&!legacyPaper)continue;
                bool known=false;
                foreach(var existing in originalGraphics)
                    if(existing.Graphic==graphic){known=true;break;}
                if(known)graphic.enabled=false;
                else
                {
                    // New legacy art created by canonical controller refresh
                    // must be remembered for exact reversible restoration.
                    originalGraphics.Add(new Original {
                        Graphic=graphic, Enabled=true
                    });
                    graphic.enabled=false;
                    ReplacedLegacyGraphics++;
                }
            }
            // The canonical layout moves its complete isolated archer art
            // to the first sibling during Reflow. In Unity this put the
            // character BEHIND our opaque stone panel, leaving an enormous
            // empty block. Keep the ONE new panel behind its genuine content.
            // Do this after each canonical layout, not by adding hero art twice.
            foreach(var surface in createdSurfaces)
                if(surface && surface.name=="Vigilia / one stone panel")
                    surface.transform.SetAsFirstSibling();

            // Old paper styling used dark-brown live unit ink. Reflow sets it
            // again, making important titles literally near-black on the new
            // dark stone. Enforce high contrast on the genuine labels, while
            // keeping the authored gold labels and real text unchanged.
            foreach(var entry in originalTextColors)
            {
                var label=entry.Key;
                if(!label || !label.gameObject.activeInHierarchy)continue;
                var c=label.color;
                var brightness=.2126f*c.r+.7152f*c.g+.0722f*c.b;
                if(brightness<.49f)
                    label.color=new Color(.95f,.90f,.79f,c.a);
            }
            if(originalBackdrop)
                originalBackdrop.color=new Color(.080f,.071f,.064f,1f);
        }

        void OnEnable()
        {
            Canvas.willRenderCanvases += BeforeCanvasPaint;
        }
        void OnDisable()
        {
            Canvas.willRenderCanvases -= BeforeCanvasPaint;
        }
        void BeforeCanvasPaint()
        {
            ReassertSinglePainting();
        }

        void Hide(Graphic graphic)
        {
            originalGraphics.Add(new Original{Graphic=graphic,Enabled=graphic.enabled});
            graphic.enabled=false;
        }

        Image AddSurface(RectTransform parent,Sprite source,string name,bool isButton)
        {
            var go=new GameObject(name,typeof(RectTransform),
                typeof(CanvasRenderer),typeof(Image));
            go.layer=parent.gameObject.layer;
            go.transform.SetParent(parent,false);
            var rt=go.GetComponent<RectTransform>();
            rt.anchorMin=Vector2.zero;
            rt.anchorMax=Vector2.one;
            rt.offsetMin=Vector2.zero;
            rt.offsetMax=Vector2.zero;
            rt.SetAsFirstSibling();
            var image=go.GetComponent<Image>();
            // The approved blank square panel was imported without a slice
            // border. Add a small, fixed pixel border at runtime instead of
            // stretching its gilded bevel across wide management cards.
            if(source==stone && source.border.sqrMagnitude<.1f)
            {
                if(!slicedStone)
                {
                    var r=source.rect;
                    float edge=Mathf.Min(44f,Mathf.Min(r.width,r.height)*.12f);
                    slicedStone=Sprite.Create(source.texture,r,
                        new Vector2(.5f,.5f),source.pixelsPerUnit,0,
                        SpriteMeshType.FullRect,new Vector4(edge,edge,edge,edge));
                    slicedStone.name="Vigilia / runtime nine-slice stone";
                }
                image.sprite=slicedStone;
            }
            else image.sprite=source;
            image.type=Image.Type.Sliced;
            image.preserveAspect=false;
            image.color=Color.white;
            image.raycastTarget=isButton;
            createdSurfaces.Add(go);
            return image;
        }

        public void Restore()
        {
            foreach(var pair in originalButtonTargets)
                if(pair.Key)pair.Key.targetGraphic=pair.Value;
            originalButtonTargets.Clear();
            foreach(var pair in originalTransitions)
                if(pair.Key)pair.Key.transition=pair.Value;
            originalTransitions.Clear();
            foreach(var entry in originalGraphics)
                if(entry.Graphic)entry.Graphic.enabled=entry.Enabled;
            originalGraphics.Clear();
            foreach(var pair in originalTextColors)
                if(pair.Key)pair.Key.color=pair.Value;
            originalTextColors.Clear();
            if(originalBackdrop)originalBackdrop.color=originalBackdropColor;
            originalBackdrop=null;
            foreach(var go in createdSurfaces)
                if(go)Destroy(go);
            createdSurfaces.Clear();
            installed=false;
            RestyledPanels=0;
            RestyledButtons=0;
            ReplacedLegacyGraphics=0;
        }

        void OnDestroy()
        {
            Restore();
            if(slicedStone)Destroy(slicedStone);
        }
    }
}
