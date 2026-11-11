using System;
using System.Collections.Generic;
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
        Sprite stone, action;
        bool installed;

        public bool Installed => installed;
        public int RestyledPanels { get; private set; }
        public int RestyledButtons { get; private set; }

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
                var paint=AddSurface(rect,action,"Vigilia / burgundy action",true);
                paint.raycastTarget=true;
                button.targetGraphic=paint;
                // No ColorTint that changes the color of this approved
                // burgundy artwork when its canonical interactable state updates.
                button.transition=Selectable.Transition.None;
                RestyledButtons++;
            }
            installed=RestyledPanels==frameNames.Length;
            if(!installed)
            {
                Restore();
                Debug.LogWarning("VIGILIA_DESTINATION_THEME_INCOMPLETE_FRAME_SET");
            }
            return installed;
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
            if(!isButton && source.border.sqrMagnitude<.1f)
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
            foreach(var go in createdSurfaces)
                if(go)Destroy(go);
            createdSurfaces.Clear();
            installed=false;
            RestyledPanels=0;
            RestyledButtons=0;
        }

        void OnDestroy()
        {
            Restore();
            if(slicedStone)Destroy(slicedStone);
        }
    }
}
