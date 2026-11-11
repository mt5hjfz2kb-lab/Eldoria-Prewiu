using System.Collections.Generic;
using Eldoria.UI.RoyalGold;
using Eldoria.UI.RoyalGold.V2;
using UnityEngine;
using UnityEngine.UI;

namespace Eldoria.BreachWatch
{
    /// <summary>
    /// ART A/B: replace THREE original pale parchment backgrounds in the real
    /// Scouts cards IN PLACE with one approved nine-sliced forged-stone asset.
    /// No graphics are pasted over the parchment, no duplicate ScrollRect,
    /// missions, click handlers, story progress or save controller are made.
    ///
    /// The old roll-paper bevel is hidden, while authentic mission paintings,
    /// independently illuminated inset rims, wax seals and burgundy actions
    /// remain original. The canonical text receives brighter stone-safe ink.
    /// Restore() returns every changed pixel source and color to the original.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class BreachWatchScoutStoneContracts : MonoBehaviour
    {
        struct ImageState
        {
            public Sprite Sprite;
            public Color Color;
            public Image.Type Type;
            public float PixelMultiplier;
            public bool Enabled;
            public bool FillCenter;
        }

        readonly Dictionary<Image,ImageState> oldImages=new Dictionary<Image,ImageState>();
        readonly Dictionary<Graphic,bool> oldGraphics=new Dictionary<Graphic,bool>();
        readonly Dictionary<RoyalText,Color> oldInk=new Dictionary<RoyalText,Color>();
        Sprite stoneSprite;
        public bool Installed { get; private set; }
        public int ContractCount { get; private set; }

        public bool Apply(RectTransform realScreen)
        {
            if(Installed)return ContractCount==3;
            if(!realScreen || realScreen.name!="Early retention / independent destination")
                return false;
            var source=Resources.Load<Sprite>("VigiliaArt/PremiumSurfaces/panel_fill");
            if(!source || !source.texture)return false;
            // Source asset has no NineSlice border: create one once, not a new
            // bitmap or runtime graphic behind the original art.
            if(!stoneSprite)
            {
                var r=source.rect;
                float edge=Mathf.Clamp(Mathf.Min(r.width,r.height)*.12f,20f,44f);
                stoneSprite=Sprite.Create(source.texture,r,new Vector2(.5f,.5f),
                    source.pixelsPerUnit,0,SpriteMeshType.FullRect,
                    new Vector4(edge,edge,edge,edge));
                stoneSprite.name="Vigilia / real scouts carved slate";
            }
            ContractCount=0;
            for(int id=1;id<=3;id++)
            {
                RectTransform contract=null;
                foreach(var rect in realScreen.GetComponentsInChildren<RectTransform>(true))
                    if(rect && rect.name=="Scout / live contract "+id)
                    {contract=rect;break;}
                if(!contract)break;
                Image paper=null;
                RoyalText title=null,reward=null;
                foreach(var image in contract.GetComponentsInChildren<Image>(true))
                    if(image && image.name=="Reference parchment / visual only")
                    {paper=image;break;}
                foreach(var label in contract.GetComponentsInChildren<RoyalText>(true))
                {
                    if(!label)continue;
                    if(label.name=="Readable contract heading")title=label;
                    if(label.name=="Real scout order reward and status")reward=label;
                }
                if(!paper || !title || !reward)break;
                oldImages[paper]=new ImageState{
                    Sprite=paper.sprite,Color=paper.color,Type=paper.type,
                    PixelMultiplier=paper.pixelsPerUnitMultiplier,
                    Enabled=paper.enabled,FillCenter=paper.fillCenter};
                paper.sprite=stoneSprite;
                paper.type=Image.Type.Sliced;
                paper.color=Color.white;
                paper.pixelsPerUnitMultiplier=1f;
                paper.fillCenter=true;
                // One material instead of a parchment illuminated by paper
                // bevel passes. Keep recessed painting and real wax seal.
                foreach(var depth in contract.GetComponentsInChildren<
                    RoyalGoldV3ScoutPaperDepth>(true))
                {
                    if(!depth || depth.Role!=
                       RoyalGoldV3ScoutPaperDepth.MaterialLayer.PaperBevel)continue;
                    oldGraphics[depth]=depth.enabled;
                    depth.enabled=false;
                }
                oldInk[title]=title.color;
                oldInk[reward]=reward.color;
                title.color=new Color(.94f,.76f,.45f,1f);
                reward.color=new Color(.96f,.92f,.83f,1f);
                ContractCount++;
            }
            Installed=ContractCount==3;
            if(!Installed)Restore();
            return Installed;
        }

        public bool NoOldParchmentVisible
        {
            get
            {
                if(!Installed)return false;
                int count=0;
                foreach(var image in oldImages.Keys)
                {
                    if(!image || !image.enabled || image.sprite!=stoneSprite)return false;
                    count++;
                }
                foreach(var graphic in oldGraphics.Keys)
                    if(graphic && graphic.enabled)return false;
                return count==3;
            }
        }

        public void Restore()
        {
            foreach(var pair in oldImages)
            {
                var image=pair.Key;
                if(!image)continue;
                image.sprite=pair.Value.Sprite;
                image.color=pair.Value.Color;
                image.type=pair.Value.Type;
                image.pixelsPerUnitMultiplier=pair.Value.PixelMultiplier;
                image.enabled=pair.Value.Enabled;
                image.fillCenter=pair.Value.FillCenter;
            }
            oldImages.Clear();
            foreach(var pair in oldGraphics)
                if(pair.Key)pair.Key.enabled=pair.Value;
            oldGraphics.Clear();
            foreach(var pair in oldInk)
                if(pair.Key)pair.Key.color=pair.Value;
            oldInk.Clear();
            Installed=false;
            ContractCount=0;
        }

        void OnDestroy()
        {
            Restore();
            if(stoneSprite)Destroy(stoneSprite);
        }
    }
}
