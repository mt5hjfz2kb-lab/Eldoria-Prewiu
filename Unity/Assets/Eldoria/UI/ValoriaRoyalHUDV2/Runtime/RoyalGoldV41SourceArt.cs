using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Eldoria.UI.RoyalGold.V2
{
    // Approved source pixels or a cached render of the owner-selected model. No flattened UI or invented numbers,
    // runtime download or additional Canvas. See source provenance beside textures.
    public static class RoyalGoldV41SourceArt
    {
        static readonly Dictionary<string,Sprite> sprites=new Dictionary<string,Sprite>();
        public static System.Func<Sprite> LyraPortraitProvider;
        public static Sprite HeroPortrait(string id)
        {
            if(id=="aldric"){
                const string aldricKey="canonical-aldric-dialogue-portrait";
                if(sprites.TryGetValue(aldricKey,out var existing))return existing;
                var face=Resources.Load<Texture2D>("UI/AldricDialogue/aldric_neutral");
                if(!face)return Get("aldric-face");
                var approved=Sprite.Create(face,new Rect(0,0,face.width,face.height),new Vector2(.5f,.5f),100);
                sprites[aldricKey]=approved;return approved;
            }
            if(id!="lyra")return null;
            // The live 3D Lyra portrait is loaded asynchronously. During
            // that first second the provider may legitimately return null.
            // Do not replace an approved heroine with an empty silhouette:
            // display the exact owner-approved illustration until the
            // canonical model render becomes available, then switch over.
            if(LyraPortraitProvider!=null)
            {
                var realModelPortrait=LyraPortraitProvider();
                if(realModelPortrait)return realModelPortrait;
            }
            const string key="approved-lyra-portrait";
            if(sprites.TryGetValue(key,out var cached))return cached;
            var source=Resources.Load<Texture2D>("EldoriaV41/HeroesApprovedMaster");
            if(source==null || source.width!=1536 || source.height!=1024)return null;
            // Exact, text-free face from the owner's six-screen master.
            // No mockup number, skill label or silhouette is rendered as live UI.
            var portrait=Sprite.Create(source,new Rect(1224,182,148,180),
                new Vector2(.5f,.5f),100,0,SpriteMeshType.FullRect);
            sprites[key]=portrait;return portrait;
        }
        public static Sprite Get(string id,int border=0)
        {
            string key=id+":"+border;
            if(sprites.TryGetValue(key,out var cached))return cached;
            var texture=Resources.Load<Texture2D>("EldoriaV41/"+id);
            if(texture==null)return null;
            var sprite=Sprite.Create(texture,new Rect(0,0,texture.width,texture.height),
                new Vector2(.5f,.5f),100,0,SpriteMeshType.FullRect,
                new Vector4(border,border,border,border));
            sprites[key]=sprite;return sprite;
        }
        public static Image Surface(RectTransform parent,string id,int border=0)
        {
            var node=new GameObject("Approved source / "+id,typeof(RectTransform),typeof(CanvasRenderer),typeof(Image));
            var rect=(RectTransform)node.transform;rect.SetParent(parent,false);
            rect.anchorMin=Vector2.zero;rect.anchorMax=Vector2.one;
            rect.offsetMin=rect.offsetMax=Vector2.zero;rect.SetAsFirstSibling();
            var image=node.GetComponent<Image>();image.sprite=Get(id,border);
            image.type=border>0?Image.Type.Sliced:Image.Type.Simple;
            image.raycastTarget=false;return image;
        }
        public static Font Serif=>Resources.Load<Font>("EldoriaRetention/reference-serif")??
            Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
    }
}

