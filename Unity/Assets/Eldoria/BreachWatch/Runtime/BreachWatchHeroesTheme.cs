using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Eldoria.BreachWatch
{
    /// <summary>
    /// Repaints the real approved heroes destination in-place using Vigilia's
    /// one-surface stone/gold and burgundy controls. Hero imagery, dynamic
    /// stats, models, unlocks, tabs and callbacks remain untouched.
    /// The canonical HeroesApprovedDestination rebuilds its content when
    /// selecting pages, so this adapter reacts to the actual new subtree.
    /// This is staged UI source until real Unity review, not art approval.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class BreachWatchHeroesTheme : MonoBehaviour
    {
        readonly Dictionary<Graphic,bool> originals=new Dictionary<Graphic,bool>();
        readonly List<GameObject> surfaces=new List<GameObject>();
        Sprite stone,button,stoneSliced;
        Material warmHall;
        Image originalBackdrop;
        Color backdropColor;
        readonly Dictionary<RawImage,Material> originalHallMaterials =
            new Dictionary<RawImage,Material>();
        readonly Dictionary<Image,Color> originalAccentColors =
            new Dictionary<Image,Color>();
        RectTransform content;
        int restyled;
        public int RestyledFrames=>restyled;
        public bool HallGradeActive
        {
            get
            {
                if(!content || !warmHall)return false;
                foreach(var photo in content.GetComponentsInChildren<RawImage>(true))
                    if(photo && photo.transform.parent==content && photo.texture &&
                       photo.material==warmHall)
                        return true;
                return false;
            }
        }
        public string HallGradeDiagnostic
        {
            get
            {
                if(!content)return "content=null";
                string result="warm="+(warmHall!=null)+";";
                foreach(var photo in content.GetComponentsInChildren<RawImage>(true))
                {
                    if(!photo || !photo.texture)continue;
                    result+=photo.name+" parent="+
                        (photo.transform.parent?photo.transform.parent.name:"null")+
                        " material="+(photo.material?photo.material.name:"null")+";";
                    if(result.Length>1100)break;
                }
                return result;
            }
        }
        public bool VisibleOldRoyalFrame
        {
            get
            {
                if(!content)return false;
                foreach(var image in content.GetComponentsInChildren<Image>(true))
                    if(image && image.isActiveAndEnabled &&
                       image.gameObject.activeInHierarchy &&
                       IsOldRoyalFrame(image.name))
                        return true;
                return false;
            }
        }

        public bool Initialize(RectTransform heroRoot)
        {
            if(!heroRoot)return false;
            stone=Resources.Load<Sprite>("VigiliaArt/PremiumSurfaces/panel_fill");
            button=Resources.Load<Sprite>("VigiliaArt/PremiumSurfaces/burgundy_action");
            var shader=Resources.Load<Shader>(
                "VigiliaArt/BreachWatchHallWarmStone");
            if(!stone || !button || !shader)return false;
            if(!warmHall)
            {
                warmHall=new Material(shader){
                    name="Vigilia / warm illustrated hero hall",
                    hideFlags=HideFlags.DontSave};
                warmHall.SetFloat("_Warmth",.72f);
                warmHall.SetFloat("_ShadowFloor",.013f);
            }
            return Refresh(heroRoot);
        }

        void LateUpdate()
        {
            if(gameObject.activeInHierarchy)
                Refresh(transform as RectTransform);
        }

        public bool Refresh(RectTransform heroRoot)
        {
            if(!heroRoot)return false;
            RectTransform newContent=null;
            foreach(Transform child in heroRoot)
                if(child.name=="Heroes / live controls")
                {newContent=child as RectTransform;break;}
            if(!newContent)return false;
            if(newContent==content)return restyled>0 && !VisibleOldRoyalFrame;
            ResetPresentation();
            content=newContent;
            GradeActualHall(heroRoot);
            var frames=content.GetComponentsInChildren<RectTransform>(true);
            foreach(var rect in frames)
            {
                bool paintedFrame=false;
                foreach(Transform child in rect)
                {
                    if(!IsOldRoyalFrame(child.name))continue;
                    var graphic=child.GetComponent<Graphic>();
                    if(!graphic)continue;
                    if(!originals.ContainsKey(graphic))
                        originals.Add(graphic,graphic.enabled);
                    graphic.enabled=false;
                    paintedFrame=true;
                }
                if(!paintedFrame)continue;
                bool isButton=rect.GetComponent<Button>()!=null;
                AddSurface(rect,isButton?button:stone,
                    isButton?"Vigilia / real hero tab":"Vigilia / real hero card",
                    isButton);
                restyled++;
            }
            // The legacy full-height 3D Lyra source had empty sky above.
            // Its upward crop would shave the original 2D heroine's hair.
            // Keep the new genuine bust's top inside its existing mask.
            foreach(var photo in content.GetComponentsInChildren<Image>(true))
                if(photo && photo.name=="Verified portrait" && photo.sprite &&
                   photo.sprite.name==BreachWatchHeroesBridge.OriginalLyraBustName)
                {
                    var position=photo.rectTransform.anchoredPosition;
                    position.y=Mathf.Min(0f,position.y);
                    photo.rectTransform.anchoredPosition=position;
                }
            return restyled>0&&!VisibleOldRoyalFrame;
        }

        void GradeActualHall(RectTransform heroRoot)
        {
            // A single *real* illustrated RawImage is graded in place, rather
            // than hiding the environment with another opaque blue screen.
            // Hero faces, 3D models, portraits and live controls stay intact.
            originalBackdrop=heroRoot.GetComponent<Image>();
            if(originalBackdrop)
            {
                backdropColor=originalBackdrop.color;
                originalBackdrop.color=new Color(.071f,.062f,.055f,1f);
            }
            foreach(var photo in content.GetComponentsInChildren<RawImage>(true))
            {
                // RawImage.name has historically resolved to either the
                // component or its painted-stage GameObject name. Select the
                // ONLY full-screen illustrated RawImage directly below the
                // authoritative live content, never nested hero portraits.
                if(!photo || !photo.texture || photo.transform.parent!=content ||
                   !photo.name.StartsWith("Hall /"))continue;
                originalHallMaterials[photo]=photo.material;
                photo.material=warmHall;
            }
            foreach(var accent in content.GetComponentsInChildren<Image>(true))
            {
                if(!accent || accent.name!="Hall / layered royal banner")
                    continue;
                originalAccentColors[accent]=accent.color;
                accent.color=new Color(.19f,.055f,.055f,.84f);
            }
        }

        static bool IsOldRoyalFrame(string name)
        {
            return name=="Royal Gold / translucent ink" ||
                name=="Royal Gold / top rim" ||
                name=="Royal Gold / bottom rim" ||
                name=="Royal Gold / left rim" ||
                name=="Royal Gold / right rim";
        }

        void AddSurface(RectTransform parent,Sprite sprite,string label,bool action)
        {
            var go=new GameObject(label,typeof(RectTransform),
                typeof(CanvasRenderer),typeof(Image));
            go.layer=parent.gameObject.layer;
            var r=go.GetComponent<RectTransform>();
            r.SetParent(parent,false);
            r.anchorMin=Vector2.zero;r.anchorMax=Vector2.one;
            r.offsetMin=r.offsetMax=Vector2.zero;
            r.SetAsFirstSibling();
            var image=go.GetComponent<Image>();
            if(!action)
            {
                if(!stoneSliced)
                {
                    float edge=Mathf.Min(44f,stone.rect.width*.12f);
                    stoneSliced=Sprite.Create(stone.texture,stone.rect,
                        new Vector2(.5f,.5f),stone.pixelsPerUnit,0,
                        SpriteMeshType.FullRect,new Vector4(edge,edge,edge,edge));
                    stoneSliced.name="Vigilia / heroes stone nine-slice";
                }
                image.sprite=stoneSliced;
            }
            else image.sprite=sprite;
            image.type=Image.Type.Sliced;
            image.raycastTarget=false; // original full-size Button is intact
            image.color=Color.white;
            surfaces.Add(go);
        }

        void ResetPresentation()
        {
            foreach(var p in originals)
                if(p.Key)p.Key.enabled=p.Value;
            originals.Clear();
            foreach(var item in originalHallMaterials)
                if(item.Key)item.Key.material=item.Value;
            originalHallMaterials.Clear();
            foreach(var item in originalAccentColors)
                if(item.Key)item.Key.color=item.Value;
            originalAccentColors.Clear();
            if(originalBackdrop)originalBackdrop.color=backdropColor;
            originalBackdrop=null;
            foreach(var go in surfaces)
                if(go)Destroy(go);
            surfaces.Clear();
            restyled=0;
            content=null;
        }

        public void Restore() { ResetPresentation(); }
        void OnDestroy()
        {
            ResetPresentation();
            if(stoneSliced)Destroy(stoneSliced);
            if(warmHall)Destroy(warmHall);
        }
    }
}

