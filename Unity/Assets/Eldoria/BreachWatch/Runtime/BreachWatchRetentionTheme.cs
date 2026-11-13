using System.Collections.Generic;
using Eldoria.UI.RoyalGold.V2;
using UnityEngine;
using UnityEngine.UI;

namespace Eldoria.BreachWatch
{
    /// <summary>
    /// Replaces the existing Vigilants/Scouts CHROME only. Keep original
    /// painterly environment, real creature illustrations, contracts, reward
    /// data and every canonical action. No extra Canvas or fake destination.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class BreachWatchRetentionTheme : MonoBehaviour
    {
        readonly Dictionary<Graphic,bool> oldGraphics=new Dictionary<Graphic,bool>();
        readonly Dictionary<Button,Graphic> oldTargets=new Dictionary<Button,Graphic>();
        readonly Dictionary<Button,Selectable.Transition> oldTransitions=
            new Dictionary<Button,Selectable.Transition>();
        readonly List<GameObject> created=new List<GameObject>();
        Sprite stone,burgundy,slicedStone;
        Image curtain;
        Color originalCurtain;
        BreachWatchRecoveredRetentionArt recovered;
        bool installed;
        public bool Installed=>installed;
        public bool HasRestoredScoutPainting=>recovered && recovered.RestoredScoutPainting;
        public int RestoredPlaneCount=>recovered?recovered.RestoredPlaneCount:0;
        public void SynchronizeRecovered()=>recovered?.Synchronize();
        public int ReplacedFrames { get; private set; }
        public int ReplacedActions { get; private set; }
        public bool OldRoyalChromeHidden
        {
            get
            {
                if(!installed)return false;
                foreach(var g in GetComponentsInChildren<Graphic>(true))
                    if(g && g.isActiveAndEnabled && g.gameObject.activeInHierarchy &&
                       (g is RoyalGoldV2FrameGraphic ||
                        g is RoyalGoldV3RetentionActionGraphic))
                        return false;
                return true;
            }
        }

        public bool Apply(RectTransform canonicalScreen,bool scoutsDestination=false)
        {
            if(installed)return true;
            if(!canonicalScreen || canonicalScreen.name!=
                "Early retention / independent destination")return false;
            stone=Resources.Load<Sprite>("VigiliaArt/PremiumSurfaces/panel_fill");
            burgundy=Resources.Load<Sprite>("VigiliaArt/PremiumSurfaces/burgundy_action");
            if(!stone || !burgundy)return false;
            curtain=canonicalScreen.GetComponent<Image>();
            if(curtain)
            {
                originalCurtain=curtain.color;
                curtain.color=new Color(.073f,.064f,.057f,1f);
            }
            ReplacedFrames=0;ReplacedActions=0;
            foreach(var old in canonicalScreen.GetComponentsInChildren<Graphic>(true))
            {
                if(!old ||
                   !(old is RoyalGoldV2FrameGraphic) &&
                   !(old is RoyalGoldV3RetentionActionGraphic))continue;
                var rect=old.transform as RectTransform;
                if(!rect)continue;
                Hide(old);
                var button=rect.GetComponent<Button>();
                bool command=button!=null;
                if(command)
                {
                    oldTargets[button]=button.targetGraphic;
                    oldTransitions[button]=button.transition;
                }
                var picture=AddSurface(rect,command?burgundy:stone,command);
                if(command)
                {
                    picture.raycastTarget=true;
                    button.targetGraphic=picture;
                    button.transition=Selectable.Transition.None;
                    ReplacedActions++;
                }
                else ReplacedFrames++;
            }
            recovered=canonicalScreen.GetComponent<BreachWatchRecoveredRetentionArt>();
            if(!recovered)recovered=canonicalScreen.gameObject.AddComponent<BreachWatchRecoveredRetentionArt>();
            installed=ReplacedFrames>0 && ReplacedActions>0 && recovered.Initialize(canonicalScreen,scoutsDestination);
            if(!installed)Restore();
            return installed;
        }

        void Hide(Graphic original)
        {
            if(!oldGraphics.ContainsKey(original))
                oldGraphics.Add(original,original.enabled);
            original.enabled=false;
        }

        Image AddSurface(RectTransform parent,Sprite source,bool action)
        {
            var obj=new GameObject(action?"Vigilia / real retention action":
                "Vigilia / real retention stone",typeof(RectTransform),
                typeof(CanvasRenderer),typeof(Image));
            obj.layer=parent.gameObject.layer;
            var rect=obj.GetComponent<RectTransform>();
            rect.SetParent(parent,false);
            rect.anchorMin=Vector2.zero;rect.anchorMax=Vector2.one;
            rect.offsetMin=rect.offsetMax=Vector2.zero;
            rect.SetAsFirstSibling();
            var paint=obj.GetComponent<Image>();
            if(!action)
            {
                if(!slicedStone)
                {
                    float edge=Mathf.Min(44f,stone.rect.width*.12f);
                    slicedStone=Sprite.Create(stone.texture,stone.rect,
                        new Vector2(.5f,.5f),stone.pixelsPerUnit,0,
                        SpriteMeshType.FullRect,new Vector4(edge,edge,edge,edge));
                }
                paint.sprite=slicedStone;
            }
            else paint.sprite=source;
            paint.type=Image.Type.Sliced;
            paint.color=Color.white;
            paint.raycastTarget=action;
            created.Add(obj);
            return paint;
        }

        public void Restore()
        {
            if(recovered)recovered.Restore();
            foreach(var pair in oldTargets)
                if(pair.Key)pair.Key.targetGraphic=pair.Value;
            oldTargets.Clear();
            foreach(var pair in oldTransitions)
                if(pair.Key)pair.Key.transition=pair.Value;
            oldTransitions.Clear();
            foreach(var pair in oldGraphics)
                if(pair.Key)pair.Key.enabled=pair.Value;
            oldGraphics.Clear();
            foreach(var obj in created)if(obj)Destroy(obj);
            created.Clear();
            if(curtain)curtain.color=originalCurtain;
            curtain=null;installed=false;ReplacedFrames=0;ReplacedActions=0;
        }

        void OnDestroy()
        {
            Restore();
            if(slicedStone)Destroy(slicedStone);
        }
    }
}
