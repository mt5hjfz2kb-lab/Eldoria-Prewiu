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
        Canvas retired;
        Sprite stone,action;
        bool applying;
        public int ReplacedDecorations { get; private set; }
        public bool RetiredCanvasDisabled => retired && !retired.enabled;

        public void Initialize(Canvas oldCanvas)
        {
            retired=oldCanvas;
            stone=Resources.Load<Sprite>("VigiliaArt/PremiumSurfaces/panel_fill");
            action=Resources.Load<Sprite>("VigiliaArt/PremiumSurfaces/burgundy_action");
            if(!retired || !stone || !action)
                throw new InvalidOperationException("Production Vigilia source artwork missing");
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
                n.StartsWith("training-courtyard") || n=="hero-locked";
        }
        public static bool IsRetiredPainter(Graphic graphic)
        {
            if(!graphic || graphic is Text)return false;
            string type=graphic.GetType().Name;
            if(graphic is RawImage content)
                return IsRetiredTexture(content.texture);
            // Old procedural UI paint is not a valid fallback in production.
            if(type.StartsWith("RoyalGold") || type=="RoyalPanel" ||
               type=="RoyalFrame" || type=="RoyalButtonGraphic")return true;
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
                    var button=graphic.GetComponent<Button>();
                    if(graphic is RawImage raw)
                    {
                        raw.texture=stone.texture;
                        raw.uvRect=new Rect(0,0,1,1);
                        raw.color=Color.white;
                    }
                    else if(graphic is Image image)
                    {
                        image.sprite=button?action:stone;
                        image.overrideSprite=null;
                        image.type=Image.Type.Sliced;
                        image.color=Color.white;
                    }
                    else
                    {
                        string type=graphic.GetType().Name;
                        bool structural=button || type=="RoyalGoldV2FrameGraphic" ||
                            type=="RoyalGoldV41ReportArtwork" ||
                            type.Contains("Parchment") || type.Contains("Contract") ||
                            type=="RoyalPanel" || type=="RoyalFrame";
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
                        graphic.enabled=false;
                        if(button){button.targetGraphic=surface;button.transition=Selectable.Transition.None;}
                    }
                }
            }
            finally{applying=false;}
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
