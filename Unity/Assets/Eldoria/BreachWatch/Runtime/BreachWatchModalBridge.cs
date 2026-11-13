using System.Collections.Generic;
using Eldoria.ReferenceClean;
using Eldoria.UI.RoyalGold;
using Eldoria.UI.RoyalGold.V2;
using UnityEngine;
using UnityEngine.UI;

namespace Eldoria.BreachWatch
{
    /// <summary>
    /// Reparents the ONE real canonical dialogue/modal under Vigilia's
    /// independent Canvas so the existing story, confirmations, accessibility
    /// and callbacks remain visible after the Royal HUD canvas is disabled.
    /// The old modal frame paintings are disabled and replaced, not overlaid.
    /// Never instantiates a second ModalWindow, RoyalButton or narrative.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class BreachWatchModalBridge : MonoBehaviour
    {
        const string Stone="VigiliaArt/PremiumSurfaces/panel_fill";
        const string Burgundy="VigiliaArt/PremiumSurfaces/burgundy_action";
        const string CloseMetal="VigiliaArt/OwnerControls/close_medallion";
        readonly Dictionary<Text,Color> oldTextColors=new Dictionary<Text,Color>();
        readonly Dictionary<Text,UnityEngine.Events.UnityAction> inkCallbacks=
            new Dictionary<Text,UnityEngine.Events.UnityAction>();
        readonly Dictionary<Graphic,bool> oldGraphics=new Dictionary<Graphic,bool>();
        readonly Dictionary<Button,Graphic> oldTargets=new Dictionary<Button,Graphic>();
        readonly Dictionary<Button,Selectable.Transition> oldTransitions=
            new Dictionary<Button,Selectable.Transition>();
        readonly List<GameObject> replacements=new List<GameObject>();
        ValoriaIntegratedWorld host;
        Canvas visualCanvas;
        ModalWindow realModal;
        RectTransform root,window;
        Transform originalParent;
        int originalSibling;
        Vector3 originalWindowScale;
        Vector2 originalAnchorMin,originalAnchorMax,originalPivot,originalPosition;
        Sprite slicedStone;
        int lastWidth=-1,lastHeight=-1;
        float lastCanvasScale=-1f;
        bool installed;
        public bool Installed => installed && realModal && visualCanvas &&
            realModal.transform.IsChildOf(visualCanvas.transform);

        public bool Initialize(ValoriaIntegratedWorld source,Canvas independentCanvas)
        {
            if(installed)return true;
            if(!source || !source.Hud || !source.Hud.Modal || !independentCanvas)
                return false;
            var stone=Resources.Load<Sprite>(Stone);
            var action=Resources.Load<Sprite>(Burgundy);
            var close=Resources.Load<Sprite>(CloseMetal);
            if(!stone || !action || !close)
            {
                Debug.LogWarning("VIGILIA_CANONICAL_MODAL_ART_MISSING");
                return false;
            }
            host=source;visualCanvas=independentCanvas;
            realModal=host.Hud.Modal;
            root=realModal.transform as RectTransform;
            window=root.Find("SafeArea/Window") as RectTransform;
            if(!root || !window)
            {
                Debug.LogWarning("VIGILIA_CANONICAL_MODAL_STRUCTURE_MISSING");
                return false;
            }
            originalParent=root.parent;
            originalSibling=root.GetSiblingIndex();
            originalWindowScale=window.localScale;
            originalAnchorMin=window.anchorMin;originalAnchorMax=window.anchorMax;
            originalPivot=window.pivot;originalPosition=window.anchoredPosition;
            var oldWindow=window.GetComponent<Graphic>();
            if(oldWindow)Hide(oldWindow);
            AddSurface(window,stone,"Vigilia / one forged stone modal",false,false);

            foreach(var button in new Button[] {
                realModal.Close,realModal.Extra,realModal.Secondary })
            {
                if(!button)continue;
                var rect=button.transform as RectTransform;
                if(!rect)continue;
                var old=rect.GetComponent<Graphic>();
                if(old)Hide(old);
                if(button==realModal.Close)
                {
                    // The close medallion already paints its own X.
                    // Suppress the obsolete vector glyph rather than
                    // drawing two crosses on top of each other.
                    var oldSymbol=rect.Find("Symbol");
                    if(oldSymbol)
                    {
                        var text=oldSymbol.GetComponent<Graphic>();
                        if(text)Hide(text);
                    }
                }
                oldTargets[button]=button.targetGraphic;
                oldTransitions[button]=button.transition;
                var img=AddSurface(rect,button==realModal.Close?close:action,
                    "Vigilia / canonical modal control",true,
                    button==realModal.Close);
                img.raycastTarget=true;
                button.targetGraphic=img;
                button.transition=Selectable.Transition.None;
                productionTargets[button]=img;
            }
            // Unmodified authoritative modal, same fields, same callbacks.
            root.SetParent(visualCanvas.transform,false);
            root.SetAsLastSibling();
            installed=true;
            Reflow();
            return true;
        }

        void Hide(Graphic visual)
        {
            if(!visual)return;
            if(!oldGraphics.ContainsKey(visual))
                oldGraphics.Add(visual,visual.enabled);
            visual.enabled=false;
        }

        Image AddSurface(RectTransform parent,Sprite source,string name,
            bool action,bool preserveAspect)
        {
            var go=new GameObject(name,typeof(RectTransform),
                typeof(CanvasRenderer),typeof(Image));
            go.layer=parent.gameObject.layer;
            var rect=go.GetComponent<RectTransform>();
            rect.SetParent(parent,false);
            rect.anchorMin=Vector2.zero;rect.anchorMax=Vector2.one;
            rect.offsetMin=Vector2.zero;rect.offsetMax=Vector2.zero;
            rect.SetAsFirstSibling();
            var image=go.GetComponent<Image>();
            if(!action && source.border.sqrMagnitude<.1f)
            {
                if(!slicedStone)
                {
                    float edge=Mathf.Min(44f,source.rect.width*.12f);
                    slicedStone=Sprite.Create(source.texture,source.rect,
                        new Vector2(.5f,.5f),source.pixelsPerUnit,0,
                        SpriteMeshType.FullRect,new Vector4(edge,edge,edge,edge));
                }
                image.sprite=slicedStone;
            }
            else image.sprite=source;
            image.type=preserveAspect?Image.Type.Simple:Image.Type.Sliced;
            image.preserveAspect=preserveAspect;
            image.raycastTarget=action;
            image.color=Color.white;
            replacements.Add(go);
            return image;
        }

        readonly Dictionary<Button,Graphic> productionTargets=new Dictionary<Button,Graphic>();
        void OnEnable(){Canvas.willRenderCanvases+=EnforcePresentation;}
        void OnDisable(){Canvas.willRenderCanvases-=EnforcePresentation;}
        void EnforcePresentation()
        {
            if(!installed)return;
            // Narrative show/hide restores its canonical painters. Keep the
            // real modal lifecycle while reasserting the one new skin,
            // including inactive dialogs before their next activation.
            foreach(var pair in oldGraphics)if(pair.Key)pair.Key.enabled=false;
            foreach(var pair in productionTargets)
                if(pair.Key && pair.Value)
                {pair.Key.targetGraphic=pair.Value;pair.Key.transition=Selectable.Transition.None;}
            // The canonical cinematic painter still applies parchment-brown
            // ink. The replacement is dark stone, so retain readable Vigilia
            // ivory after that painter runs, without changing story text/font.
            if(realModal && realModal.gameObject.activeInHierarchy)
            {
                ReadableInk(realModal.Body,new Color(.98f,.94f,.84f,1f));
                var role=window?window.Find("Narrative character role"):null;
                if(role)ReadableInk(role.GetComponent<Text>(),new Color(.88f,.85f,.79f,1f));
            }
            // Canonical narrative layout runs before this bridge and still
            // measures the retired Canvas. Its world coordinates cannot be
            // used in the independent camera Canvas. Resolve presentation
            // against the modal's actual safe-area parent on every render,
            // including the first story immediately after nickname commit.
            if(realModal && realModal.gameObject.activeInHierarchy)Reflow();
        }
        void ReadableInk(Text text,Color ink)
        {
            if(!text)return;
            if(!oldTextColors.ContainsKey(text))oldTextColors.Add(text,text.color);
            if(!inkCallbacks.ContainsKey(text))
            {
                // willRenderCanvases can run after CanvasUpdateRegistry has
                // already built the mesh. Enforce when a legacy painter dirties
                // the text, so the actual browser vertex colors cannot remain
                // brown while a later property-only assertion reports ivory.
                UnityEngine.Events.UnityAction restoreInk=()=>
                {
                    if(text && text.color!=ink)text.color=ink;
                };
                inkCallbacks.Add(text,restoreInk);
                text.RegisterDirtyVerticesCallback(restoreInk);
            }
            text.color=ink;
        }
        void LateUpdate()
        {
            EnforcePresentation();
            if(installed && realModal && realModal.gameObject.activeInHierarchy &&
               (lastWidth!=Screen.width || lastHeight!=Screen.height ||
                Mathf.Abs(lastCanvasScale-visualCanvas.scaleFactor)>.001f))
                Reflow();
        }

        void Reflow()
        {
            if(!window || !visualCanvas)return;
            lastWidth=Screen.width;lastHeight=Screen.height;
            lastCanvasScale=visualCanvas.scaleFactor;
            var actualSafeArea=window.parent as RectTransform;
            if(!actualSafeArea)return;
            float availableW=Mathf.Max(1f,actualSafeArea.rect.width-24f);
            float availableH=Mathf.Max(1f,actualSafeArea.rect.height-24f);
            var size=window.rect.size;
            if(size.x<1f||size.y<1f)return;
            float fit=Mathf.Min(1f,Mathf.Min(availableW/size.x,
                availableH/size.y));
            window.localScale=new Vector3(fit,fit,originalWindowScale.z);
            window.anchorMin=window.anchorMax=new Vector2(.5f,0f);
            window.pivot=new Vector2(.5f,0f);
            window.anchoredPosition=new Vector2(0f,12f);
        }

        public void Restore()
        {
            if(!root)return;
            foreach(var pair in inkCallbacks)
                if(pair.Key)pair.Key.UnregisterDirtyVerticesCallback(pair.Value);
            inkCallbacks.Clear();
            foreach(var pair in oldTextColors)if(pair.Key)pair.Key.color=pair.Value;
            oldTextColors.Clear();
            foreach(var pair in oldTargets)
                if(pair.Key)pair.Key.targetGraphic=pair.Value;
            oldTargets.Clear();productionTargets.Clear();
            foreach(var pair in oldTransitions)
                if(pair.Key)pair.Key.transition=pair.Value;
            oldTransitions.Clear();
            foreach(var pair in oldGraphics)
                if(pair.Key)pair.Key.enabled=pair.Value;
            oldGraphics.Clear();
            foreach(var go in replacements)if(go)Destroy(go);
            replacements.Clear();
            if(window){window.localScale=originalWindowScale;
                window.anchorMin=originalAnchorMin;window.anchorMax=originalAnchorMax;
                window.pivot=originalPivot;window.anchoredPosition=originalPosition;}
            if(root && originalParent)
            {
                root.SetParent(originalParent,false);
                root.SetSiblingIndex(Mathf.Clamp(originalSibling,0,
                    originalParent.childCount-1));
            }
            installed=false;
        }

        void OnDestroy()
        {
            Canvas.willRenderCanvases-=EnforcePresentation;
            Restore();
            if(slicedStone)Destroy(slicedStone);
        }
    }
}
