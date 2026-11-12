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
        // Native 1600x900 evidence shows introduction glyphs behind the
        // category tabs. Reserve and measure the real exclusive reading band.
        Text hallIntro;
        TextAnchor hallIntroAlignment;
        HorizontalWrapMode hallIntroHorizontal;
        VerticalWrapMode hallIntroVertical;
        bool hallIntroBestFit;
        int hallIntroFontSize;
        Vector2 hallIntroSize;
        Vector2 hallIntroPosition;
        float hallIntroLineSpacing;
        public bool DesktopHallIntroductionUnobstructed { get; private set; } = true;
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
            if(newContent==content)
            {
                // The canonical layout can settle one frame after adoption.
                // Retry a missing reading band without cloning another UI.
                if(!hallIntro)RecomposeDesktopHallHeader();
                return restyled>0 && !VisibleOldRoyalFrame;
            }
            ResetPresentation();
            content=newContent;
            GradeActualHall(heroRoot);
            RecomposeDesktopHallHeader();
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

        // Native 43142184165 still shows the second sentence crossing the
        // top of four real tabs at 1600x900, despite a source-only font-fit
        // check passing. The prior method used anchoredPosition from the
        // original centered, BestFit Text as though it were rendered glyph
        // bounds. The hall is scaled and Unity centers those glyphs LOWER.
        // Change technique: reserve an actual bounded reading band BETWEEN
        // the title and tabs, top-align all glyphs and measure both lines.
        // No cover-up panel, fabricated command or changed narrative text.
        void RecomposeDesktopHallHeader()
        {
            DesktopHallIntroductionUnobstructed=true;
            if(!content || content.rect.width<=content.rect.height)return;
            Text intro=null;
            RectTransform tabs=null,title=null;
            foreach(var t in content.GetComponentsInChildren<Text>(true))
                if(t && t.name=="Heroes hall introduction") {intro=t;break;}
            // Compact landscape has NO introduction; don't add one.
            if(!intro)return;
            foreach(var r in content.GetComponentsInChildren<RectTransform>(true))
            {
                if(r && r.name=="Hall / category TODOS")tabs=r;
                else if(r && r.name=="Heroes hall title")title=r;
            }
            if(!tabs || !title)
            {
                DesktopHallIntroductionUnobstructed=false;
                return;
            }
            if(hallIntro==intro)return;
            float titleBottom=-title.anchoredPosition.y+title.rect.height;
            float tabTop=-tabs.anchoredPosition.y;
            // The 1600x900 canonical stage has a notably SMALLER logical
            // height than 900 due to its scale factor. Never exclude it
            // with a hard-coded content.rect.height>=600 check.
            float bandTop=Mathf.Max(titleBottom+9f,tabTop-57f);
            float bandBottom=tabTop-12f;
            float available=bandBottom-bandTop;
            if(available<34f)
            {
                DesktopHallIntroductionUnobstructed=false;
                return;
            }
            hallIntro=intro;
            hallIntroAlignment=intro.alignment;
            hallIntroHorizontal=intro.horizontalOverflow;
            hallIntroVertical=intro.verticalOverflow;
            hallIntroBestFit=intro.resizeTextForBestFit;
            hallIntroFontSize=intro.fontSize;
            hallIntroSize=intro.rectTransform.sizeDelta;
            hallIntroPosition=intro.rectTransform.anchoredPosition;
            hallIntroLineSpacing=intro.lineSpacing;
            intro.alignment=TextAnchor.UpperLeft;
            intro.horizontalOverflow=HorizontalWrapMode.Wrap;
            intro.verticalOverflow=VerticalWrapMode.Truncate;
            intro.resizeTextForBestFit=false;
            intro.lineSpacing=.88f;
            var rt=intro.rectTransform;
            rt.anchoredPosition=new Vector2(rt.anchoredPosition.x,-bandTop);
            rt.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical,available);
            bool fits=false;
            for(int size=Mathf.Min(hallIntroFontSize,21);size>=14;--size)
            {
                intro.fontSize=size;
                if(intro.preferredHeight<=available-2f){fits=true;break;}
            }
            DesktopHallIntroductionUnobstructed=fits &&
                bandTop>=titleBottom+7f &&
                bandTop+rt.rect.height<=tabTop-11f;
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
            if(hallIntro)
            {
                hallIntro.alignment=hallIntroAlignment;
                hallIntro.horizontalOverflow=hallIntroHorizontal;
                hallIntro.verticalOverflow=hallIntroVertical;
                hallIntro.resizeTextForBestFit=hallIntroBestFit;
                hallIntro.fontSize=hallIntroFontSize;
                hallIntro.lineSpacing=hallIntroLineSpacing;
                hallIntro.rectTransform.anchoredPosition=hallIntroPosition;
                hallIntro.rectTransform.sizeDelta=hallIntroSize;
            }
            hallIntro=null;
            DesktopHallIntroductionUnobstructed=true;
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

