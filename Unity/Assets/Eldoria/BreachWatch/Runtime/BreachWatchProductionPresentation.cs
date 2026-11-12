using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Eldoria.BreachWatch
{
    /// <summary>
    /// Production-only visual boundary. Gameplay controllers survive; their
    /// retired frame/backdrop painters never render on the new Canvas.
    /// Character/creature portraits and earned-item illustrations are game
    /// content, not a second interface skin. No gateway or save writes here.
    /// </summary>
    [DisallowMultipleComponent, DefaultExecutionOrder(32000)]
    public sealed class BreachWatchProductionPresentation : MonoBehaviour
    {
        public static bool Required => !UnityEngine.Application.isEditor ||
            Environment.GetEnvironmentVariable("ELDORIA_NEW_UI_ONLY")=="1";
        readonly List<Graphic> graphics=new List<Graphic>(512);
        readonly Dictionary<Graphic,Image> replacements=new Dictionary<Graphic,Image>();
        readonly List<Canvas> oldCanvases=new List<Canvas>();
        readonly Dictionary<Graphic,Graphic> iconReplacements=new Dictionary<Graphic,Graphic>();
        readonly Dictionary<Graphic,int> iconKinds=new Dictionary<Graphic,int>();
        Canvas retired;
        Sprite stone,action,emptyHero;
        readonly Sprite[] resources=new Sprite[3];
        bool applying;
        public int ReplacedDecorations { get; private set; }
        public bool RetiredCanvasDisabled => retired && !retired.enabled;

        public void Initialize(Canvas oldCanvas)
        {
            retired=oldCanvas;
            stone=Resources.Load<Sprite>("VigiliaArt/PremiumSurfaces/panel_fill");
            action=Resources.Load<Sprite>("VigiliaArt/PremiumSurfaces/burgundy_action");
            emptyHero=Resources.Load<Sprite>("VigiliaArt/CleanRoom/nav_helm");
            for(int i=0;i<3;i++)resources[i]=Resources.Load<Sprite>("VigiliaArt/FreshArt/"+(i==0?"wood":i==1?"stone":"food"));
            if(!retired || !stone || !action || !emptyHero || !resources[0] || !resources[1] || !resources[2])
                throw new InvalidOperationException("Production Vigilia source artwork missing");
            // Keep the authoritative world-position projection and existing
            // construction clocks on the new Canvas. This transparent layer
            // owns no artwork; its retired controls are replaced by Enforce.
            var labels=retired.transform.Find("World and construction labels") as RectTransform;
            if(labels)
            {
                labels.SetParent(transform,false);labels.SetAsFirstSibling();
                labels.anchorMin=Vector2.zero;labels.anchorMax=Vector2.one;
                labels.offsetMin=labels.offsetMax=Vector2.zero;
            }
            var identity=retired.GetComponentInChildren<Eldoria.PlayerClarity.RoyalPlayerNamePrompt>(true);
            if(identity)
            {
                var root=(RectTransform)identity.transform;
                root.SetParent(transform,false);root.SetAsLastSibling();
                root.anchorMin=Vector2.zero;root.anchorMax=Vector2.one;
                root.offsetMin=root.offsetMax=Vector2.zero;
                foreach(var border in root.GetComponentsInChildren<Outline>(true))border.enabled=false;
                foreach(var plate in root.GetComponentsInChildren<Image>(true))
                {
                    if(plate.transform==root){plate.color=new Color(.025f,.022f,.025f,.90f);continue;}
                    if(plate.name=="Inner vellum" || plate.name=="Thin golden divider")
                    {plate.enabled=false;continue;}
                    plate.sprite=plate.GetComponent<Button>()?action:stone;
                    plate.type=Image.Type.Sliced;plate.color=Color.white;
                    var control=plate.GetComponent<Selectable>();
                    if(control){control.targetGraphic=plate;control.transition=Selectable.Transition.None;}
                }
                foreach(var label in root.GetComponentsInChildren<Text>(true))
                    label.color=label.name=="Validation"?new Color(1f,.58f,.42f):new Color(.96f,.91f,.80f);
            }
            Canvas.willRenderCanvases+=Enforce;
            Enforce();
        }
        // Enumerate decorative donors only. Real heroes, troops, nodes and
        // mission illustrations remain the same authoritative game content.
        public static bool IsRetiredTexture(Texture texture)
        {
            if(!texture)return false;
            string n=texture.name.ToLowerInvariant();
            return n=="blue-panel" || n=="gold-button" || n=="tab-idle" ||
                n=="mail-card" || n=="herocardnineslicegold" ||
                n.StartsWith("heroes-hall-") || n=="scout-parchment-v3" ||
                n=="battle-action-skin-v4" || n=="scout-action-skin-v4" ||
                n=="guardian-banner-v4" || n=="encounter-panel-v3" ||
                n=="scouts-painterly" || n=="scouts-painterly-v2" ||
                n=="clean_backdrop" || n=="vigilants-illustration" || n=="scouts-illustration" ||
                n=="vigilants-painterly" || n=="vigilants-painterly-v2" ||
                n.StartsWith("training-courtyard") || n=="hero-locked" || n=="hero-support";
        }
        public static bool IsRetiredPainter(Graphic graphic)
        {
            if(!graphic || graphic is Text)return false;
            if(graphic.name.StartsWith("March art / "))return true;
            string type=graphic.GetType().Name;
            if(graphic is RawImage content)
                return IsRetiredTexture(content.texture);
            // Old procedural UI paint is not a valid fallback in production.
            if(type=="RoyalResourcePictogram" || type=="RoyalIconGraphic")return true;
            if(graphic is Image marker && graphic.name.StartsWith("Clarity / ") && !marker.sprite)return true;
            if(type.StartsWith("RoyalGold") || type=="RoyalPanel" ||
               type=="RoyalFrame" || type=="RoyalFrameGraphic" || type=="RoyalButtonGraphic")return true;
            if(graphic is Image image)return IsRetiredTexture(image.sprite?image.sprite.texture:null);
            if(graphic is RawImage raw)return IsRetiredTexture(raw.texture);
            return false;
        }
        public void Enforce()
        {
            if(applying || !retired || !stone)return;
            applying=true;
            try
            {
                // The clarity adapter can bind after the HUD on a later frame.
                // Reparent its one live projection layer, preserving its own
                // camera/collision placement, tier values and visibility rules.
                var clarity=retired.transform.Find("Player clarity / non-interactive projected labels") as RectTransform;
                if(clarity)
                {
                    clarity.SetParent(transform,false);clarity.SetAsFirstSibling();
                    clarity.anchorMin=Vector2.zero;clarity.anchorMax=Vector2.one;
                    clarity.offsetMin=clarity.offsetMax=Vector2.zero;
                }
                retired.GetComponentsInChildren<Canvas>(true,oldCanvases);
                foreach(var canvas in oldCanvases)
                    if(canvas && !canvas.transform.IsChildOf(transform))canvas.enabled=false;
                retired.enabled=false;
                var oldInput=retired.GetComponent<GraphicRaycaster>();
                if(oldInput)oldInput.enabled=false;
                GetComponentsInChildren<Graphic>(true,graphics);
                foreach(var graphic in graphics)
                {
                    if(!graphic || !graphic.gameObject.activeInHierarchy ||
                       !IsRetiredPainter(graphic))continue;
                    // Existing Vigilia bridges already replace disabled old
                    // frame painters. Do not add a second surface underneath
                    // their authored replacement. Only our own retained
                    // replacements need synchronization after disabling.
                    if(!graphic.enabled && !replacements.ContainsKey(graphic) && !iconReplacements.ContainsKey(graphic))continue;
                    if(graphic is Eldoria.UI.RoyalGold.RoyalIconGraphic icon)
                    {
                        if(!iconReplacements.TryGetValue(icon,out var replacement) || !replacement || iconKinds[icon]!=icon.Icon)
                        {
                            if(replacement){replacement.gameObject.SetActive(false);Destroy(replacement.gameObject);}
                            iconReplacements[icon]=ReplaceIcon(icon);iconKinds[icon]=icon.Icon;
                        }
                        icon.enabled=false;continue;
                    }
                    // Canonical rim-only overlays sit AFTER portraits and labels.
                    // Replacing their atlas with an opaque panel hides live content.
                    // The new shared theme already owns the card background.
                    if(graphic.name.StartsWith("March art / ") ||
                       (graphic is Image rim && !rim.fillCenter && IsRetiredTexture(rim.sprite?rim.sprite.texture:null)))
                    {graphic.enabled=false;continue;}
                    var button=graphic.GetComponent<Button>();
                    if(graphic is RawImage raw)
                    {
                        raw.texture=stone.texture;
                        raw.uvRect=new Rect(0,0,1,1);
                        raw.color=Color.white;
                    }
                    else if(graphic is Image image)
                    {
                        // Empty hero slots are controls, not hero portraits.
                        // Use the new helm emblem for both slots rather than
                        // retaining the retired blue support silhouette or
                        // replacing the locked slot with a blank stone tile.
                        string donor=image.sprite && image.sprite.texture
                            ?image.sprite.texture.name.ToLowerInvariant():"";
                        bool emptySlot=donor=="hero-locked" || donor=="hero-support";
                        image.sprite=emptySlot?emptyHero:button?action:stone;
                        image.overrideSprite=null;
                        image.type=emptySlot?Image.Type.Simple:Image.Type.Sliced;
                        if(emptySlot)image.preserveAspect=true;
                        image.color=Color.white;
                        if(image.name.StartsWith("Clarity / "))
                            foreach(var outline in image.GetComponents<Outline>())outline.enabled=false;
                    }
                    else
                    {
                        string type=graphic.GetType().Name;
                        var pictogram=graphic as Eldoria.PlayerClarity.RoyalResourcePictogram;
                        bool structural=pictogram || button || type=="RoyalGoldV2FrameGraphic" ||
                            type=="RoyalGoldV41ReportArtwork" ||
                            type.Contains("Parchment") || type.Contains("Contract") ||
                            type=="RoyalPanel" || type=="RoyalFrame" || type=="RoyalFrameGraphic";
                        if(!structural){graphic.enabled=false;continue;}
                        if(!replacements.TryGetValue(graphic,out var surface) || !surface)
                        {
                            var go=new GameObject("Vigilia / exclusive production surface",
                                typeof(RectTransform),typeof(CanvasRenderer),typeof(Image));
                            go.layer=graphic.gameObject.layer;
                            var rect=go.GetComponent<RectTransform>();
                            rect.SetParent(graphic.transform,false);rect.SetAsFirstSibling();
                            rect.anchorMin=Vector2.zero;rect.anchorMax=Vector2.one;
                            rect.offsetMin=rect.offsetMax=Vector2.zero;
                            surface=go.GetComponent<Image>();surface.sprite=button?action:stone;
                            surface.type=Image.Type.Sliced;surface.color=Color.white;
                            surface.raycastTarget=graphic.raycastTarget;
                            replacements[graphic]=surface;ReplacedDecorations++;
                            // New mineral paper requires ivory text; do not
                            // leave old dark parchment ink unreadable.
                            foreach(var text in graphic.GetComponentsInChildren<Text>(true))
                                if(text.color.r<.65f && text.color.g<.65f && text.color.b<.65f)
                                    text.color=new Color(.94f,.89f,.77f,text.color.a);
                        }
                        if(pictogram)
                        {
                            surface.sprite=resources[(int)pictogram.Kind];
                            surface.type=Image.Type.Simple;surface.preserveAspect=true;
                            surface.raycastTarget=false;
                        }
                        graphic.enabled=false;
                        if(button){button.targetGraphic=surface;button.transition=Selectable.Transition.None;}
                    }
                }
            }
            finally{applying=false;}
        }
        Graphic ReplaceIcon(Eldoria.UI.RoyalGold.RoyalIconGraphic original)
        {
            var go=new GameObject("Vigilia / exclusive functional icon",typeof(RectTransform),typeof(CanvasRenderer));
            go.layer=original.gameObject.layer;
            var rect=go.GetComponent<RectTransform>();rect.SetParent(original.transform,false);
            rect.anchorMin=Vector2.zero;rect.anchorMax=Vector2.one;rect.offsetMin=rect.offsetMax=Vector2.zero;
            int kind=original.Icon;
            if(kind==13 || kind==14)
            {
                var text=go.AddComponent<Text>();text.text=kind==13?"−":"+";
                text.font=Resources.Load<Font>("VigiliaArt/Fonts/SourceSans3-Semibold");
                text.fontSize=24;text.alignment=TextAnchor.MiddleCenter;
                text.resizeTextForBestFit=true;text.resizeTextMinSize=10;text.resizeTextMaxSize=24;
                text.color=new Color(.96f,.91f,.80f);text.raycastTarget=false;return text;
            }
            string asset=kind==0?"CleanRoom/profile":kind==4?"CleanRoom/nav_helm":
                kind==5?"CleanRoom/nav_swords":kind==6?"CleanRoom/mission":
                kind==7?"CleanRoom/nav_provisions":kind==9?"CleanRoom/nav_castle":
                kind==11?"OwnerControls/close_medallion":kind==17?"CleanRoom/nav_raven":null;
            var sprite=kind>=1 && kind<=3?resources[kind-1]:
                asset!=null?Resources.Load<Sprite>("VigiliaArt/"+asset):null;
            if(sprite)
            {
                var image=go.AddComponent<Image>();image.sprite=sprite;
                image.preserveAspect=true;image.raycastTarget=false;return image;
            }
            var glyph=go.AddComponent<BreachWatchGlyph>();
            glyph.Type=kind==8||kind==16?BreachWatchGlyph.Sigil.World:
                kind==10?BreachWatchGlyph.Sigil.Settings:kind==15?BreachWatchGlyph.Sigil.Hammer:
                BreachWatchGlyph.Sigil.Arrow;
            glyph.raycastTarget=false;return glyph;
        }

        public string FirstVisibleRetiredPainting()
        {
            GetComponentsInChildren<Graphic>(true,graphics);
            foreach(var graphic in graphics)
                if(graphic && graphic.isActiveAndEnabled && IsRetiredPainter(graphic))
                    return graphic.name+" / "+graphic.GetType().Name;
            return "";
        }
        void OnDestroy(){Canvas.willRenderCanvases-=Enforce;}
    }
}
