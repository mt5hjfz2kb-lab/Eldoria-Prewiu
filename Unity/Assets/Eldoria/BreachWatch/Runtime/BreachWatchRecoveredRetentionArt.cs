using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Eldoria.UI.RoyalGold.V2;

namespace Eldoria.BreachWatch
{
    // Owner-requested recovery of the approved illustrations (13 Nov 2026).
    // New Vigilia render nodes own these pixels. Retired Royal painters, frames,
    // actions and canvases stay disabled; canonical game data/actions stay intact.
    [DefaultExecutionOrder(31000)]
    public sealed class BreachWatchRecoveredRetentionArt : MonoBehaviour
    {
        static readonly string[] Pieces={"left_castle","bridge","right_castle","left_rock","right_cliff","near_cliff"};
        static readonly Rect[] PieceUv={
            new Rect(.05924479f,.58789062f,.30143229f,.38574219f),
            new Rect(.25195312f,.39550781f,.34765625f,.30957031f),
            new Rect(.52539062f,.48632812f,.39778646f,.48730469f),
            new Rect(0f,.30566406f,.39583333f,.53808594f),
            new Rect(.51757812f,.04199219f,.48242188f,.68945312f),
            new Rect(0f,0f,.59895833f,.36816406f)};
        readonly HashSet<Graphic> owned=new HashSet<Graphic>();
        readonly Dictionary<Graphic,bool> hidden=new Dictionary<Graphic,bool>();
        readonly List<GameObject> created=new List<GameObject>();
        readonly List<Sprite> runtimeSprites=new List<Sprite>();
        readonly List<RawImage> pieces=new List<RawImage>();
        readonly List<RoyalGoldV3ScoutContractGraphic> contractSources=new List<RoyalGoldV3ScoutContractGraphic>();
        readonly List<RawImage> medallions=new List<RawImage>();
        RawImage source,backdrop;
        bool scouts,installed;
        public bool Installed=>installed && backdrop && backdrop.enabled;
        public int RestoredPlaneCount=>Installed?1+pieces.Count:0;
        public bool Owns(Graphic graphic)=>graphic && owned.Contains(graphic);
        public bool RestoredScoutPainting=>Installed && scouts && pieces.Count==0;

        static RectTransform RectNode(Transform parent,string name)
        {
            var go=new GameObject(name,typeof(RectTransform));go.layer=parent.gameObject.layer;
            var rect=(RectTransform)go.transform;rect.SetParent(parent,false);
            rect.anchorMin=Vector2.zero;rect.anchorMax=Vector2.one;
            rect.offsetMin=rect.offsetMax=Vector2.zero;return rect;
        }
        RawImage Painting(Transform parent,string name,Texture texture)
        {
            var rect=RectNode(parent,name);rect.gameObject.AddComponent<CanvasRenderer>();
            var image=rect.gameObject.AddComponent<RawImage>();image.texture=texture;
            image.color=Color.white;image.raycastTarget=false;owned.Add(image);
            created.Add(rect.gameObject);return image;
        }
        void Hide(Graphic graphic)
        {
            if(!graphic || owned.Contains(graphic))return;
            if(!hidden.ContainsKey(graphic))hidden.Add(graphic,graphic.enabled);
            graphic.enabled=false;
        }
        public bool Initialize(RectTransform screen,bool isScouts)
        {
            if(installed)return true;scouts=isScouts;
            foreach(var raw in screen.GetComponentsInChildren<RawImage>(true))
                if(raw.name=="Original illustrated world / decorative only"){source=raw;break;}
            if(!source)return false;
            var art=source.transform.parent as RectTransform;
            var texture=Resources.Load<Texture2D>(scouts?"EldoriaRetention/scouts-painterly-v2":"EldoriaRetention/DepthB11/clean_backdrop");
            if(!texture)return false;
            var separated=new Texture2D[scouts?0:Pieces.Length];
            for(int i=0;i<separated.Length;i++)
            {
                separated[i]=Resources.Load<Texture2D>("EldoriaRetention/DepthB11/"+Pieces[i]);
                if(!separated[i])return false; // No incomplete castle/bridge reconstruction.
            }
            // Suppress decoration before the generic retirement boundary can
            // turn a transparent overlay into an opaque stone rectangle.
            foreach(var graphic in screen.GetComponentsInChildren<Graphic>(true))
            {
                if(graphic is Text)continue;
                if(graphic==source || graphic.name.StartsWith("Vigilants / original paint plane / ") ||
                   graphic.name=="Reference parchment / visual only" ||
                   (graphic.name=="Vigilia / real retention stone" && graphic.transform.parent.name.StartsWith("Scout / live contract ")) ||
                   (!(graphic is RawImage) && graphic.GetType().Name.StartsWith("RoyalGold") &&
                    !(graphic is Eldoria.UI.RoyalGold.V2.RoyalGoldV3StageTrailGraphic)))Hide(graphic);
            }
            var room=RectNode(art,"Vigilia / recovered source composition");created.Add(room.gameObject);
            room.offsetMin=new Vector2(5,5);room.offsetMax=new Vector2(-5,-5);
            room.SetSiblingIndex(Mathf.Min(1,art.childCount-1));
            backdrop=Painting(room,"Vigilia / authored landscape backplate",texture);
            for(int i=0;i<separated.Length;i++)pieces.Add(Painting(room,"Vigilia / separate authored mass / "+Pieces[i],separated[i]));
            // The approved blank paper is a material, not a flattened UI.
            // Existing headings, thumbnails, states and Vigilia buttons remain live.
            if(scouts)
            {
                var paper=Resources.Load<Texture2D>("EldoriaRetention/scout-parchment-v3");
                var badge=Resources.Load<Texture2D>("EldoriaRetention/scout-medallions-v5");
                if(!paper || !badge){Restore();return false;}
                var sprite=Sprite.Create(paper,new Rect(0,0,paper.width,paper.height),new Vector2(.5f,.5f),100f,0,SpriteMeshType.FullRect,new Vector4(70,90,70,90));
                runtimeSprites.Add(sprite);
                foreach(var child in screen.GetComponentsInChildren<RectTransform>(true))
                {
                    if(!child.name.StartsWith("Scout / live contract "))continue;
                    var shadow=RectNode(child,"Vigilia / paper contact shadow");created.Add(shadow.gameObject);
                    shadow.offsetMin=new Vector2(3,-5);shadow.offsetMax=new Vector2(3,-5);
                    var ink=shadow.gameObject.AddComponent<Image>();ink.color=new Color(.015f,.012f,.008f,.65f);ink.raycastTarget=false;owned.Add(ink);
                    shadow.SetAsFirstSibling();
                    var sheet=RectNode(child,"Vigilia / recovered blank cartographer paper");created.Add(sheet.gameObject);
                    var image=sheet.gameObject.AddComponent<Image>();image.sprite=sprite;image.type=Image.Type.Sliced;
                    image.pixelsPerUnitMultiplier=3f;image.color=new Color(.96f,.91f,.81f,1f);image.raycastTarget=false;
                    owned.Add(image);sheet.SetSiblingIndex(1);
                    var canonical=child.GetComponentInChildren<RoyalGoldV3ScoutContractGraphic>(true);
                    if(!canonical){Restore();return false;}
                    var seal=Painting(child,"Vigilia / live expedition family medallion",badge);
                    seal.rectTransform.anchorMin=seal.rectTransform.anchorMax=new Vector2(.5f,1f);
                    contractSources.Add(canonical);medallions.Add(seal);
                }
            }
            installed=true;Synchronize();return true;
        }
        public bool TryStyleText(Text text)
        {
            if(!installed || !scouts || !text)return false;
            if(text.name=="Readable contract heading")text.color=new Color(.25f,.15f,.075f,1f);
            else if(text.name=="Real scout order reward and status")text.color=new Color(.23f,.20f,.15f,1f);
            else return false;
            return true;
        }
        public void Synchronize()
        {
            if(!installed || !source)return;
            foreach(var pair in hidden)if(pair.Key)pair.Key.enabled=false;
            // Reuse only authoritative artwork-space crop. All visual nodes
            // here are new and never alter that crop or any route/save data.
            var cover=source.uvRect;backdrop.uvRect=cover;
            for(int i=0;i<pieces.Count;i++)
            {
                var rect=pieces[i].rectTransform;var bounds=PieceUv[i];
                rect.anchorMin=new Vector2((bounds.xMin-cover.xMin)/Mathf.Max(.0001f,cover.width),(bounds.yMin-cover.yMin)/Mathf.Max(.0001f,cover.height));
                rect.anchorMax=new Vector2((bounds.xMax-cover.xMin)/Mathf.Max(.0001f,cover.width),(bounds.yMax-cover.yMin)/Mathf.Max(.0001f,cover.height));
                rect.offsetMin=rect.offsetMax=Vector2.zero;
            }
            if(scouts)
            {
                for(int i=0;i<medallions.Count;i++)
                {
                    var donor=contractSources[i];var seal=medallions[i];
                    int kind=Mathf.Clamp(donor.Kind,0,3);
                    seal.uvRect=new Rect((kind%2)*.5f,kind<2?.5f:0f,.5f,.5f);
                    float radius=Mathf.Clamp(donor.rectTransform.rect.width*.065f,15f,28f)+4f;
                    seal.rectTransform.sizeDelta=Vector2.one*radius*2f;
                    seal.rectTransform.anchoredPosition=new Vector2(0,-22f);
                }
                foreach(var text in GetComponentsInChildren<Text>(true))TryStyleText(text);
            }
        }
        void LateUpdate()=>Synchronize();
        void OnEnable(){Canvas.willRenderCanvases+=Synchronize;}
        void OnDisable(){Canvas.willRenderCanvases-=Synchronize;}
        public void Restore()
        {
            installed=false;
            foreach(var pair in hidden)if(pair.Key)pair.Key.enabled=pair.Value;
            hidden.Clear();owned.Clear();pieces.Clear();contractSources.Clear();medallions.Clear();
            foreach(var go in created)if(go){go.SetActive(false);Destroy(go);}created.Clear();
            foreach(var sprite in runtimeSprites)if(sprite)Destroy(sprite);runtimeSprites.Clear();
            source=backdrop=null;
        }
        void OnDestroy()=>Restore();
    }
}
