using System.Collections.Generic;
using Eldoria.UI.RoyalGold;
using Eldoria.UI.RoyalGold.V2;
using UnityEngine;
using UnityEngine.UI;

namespace Eldoria.BreachWatch
{
    /// <summary>
    /// Deliberately RECOMPOSES the real Training view at a scene/character
    /// scale instead of stacking gold borders on its previous blue cards.
    /// Existing source art, native recruitment buttons, costs, quantity and
    /// save authority are reused; no second gameplay controller is created.
    /// A/B images must decide whether this macro-level direction is worth
    /// keeping. Not intended as an artistic sign-off.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class BreachWatchTrainingComposition : MonoBehaviour
    {
        readonly struct LayoutState
        {
            public readonly RectTransform Rect;
            public readonly Vector2 AnchorMin,AnchorMax,Pivot,Position,Size;
            public readonly int FontSize;
            public LayoutState(RectTransform r)
            {
                Rect=r;AnchorMin=r.anchorMin;AnchorMax=r.anchorMax;Pivot=r.pivot;
                Position=r.anchoredPosition;Size=r.sizeDelta;
                var text=r.GetComponent<Text>();
                FontSize=text?text.fontSize:0;
            }
            public void Restore()
            {
                if(!Rect)return;
                Rect.anchorMin=AnchorMin;Rect.anchorMax=AnchorMax;Rect.pivot=Pivot;
                Rect.anchoredPosition=Position;Rect.sizeDelta=Size;
                var text=Rect.GetComponent<Text>();
                if(text && FontSize>0)text.fontSize=FontSize;
            }
        }
        readonly Dictionary<RectTransform, LayoutState> originals=
            new Dictionary<RectTransform, LayoutState>();
        RoyalGoldV3ManagementScreens controller;
        RectTransform left,hero,identity,nameText,detailText,t1,t2;
        RectTransform[] portraits=new RectTransform[2];
        RectTransform[] labels=new RectTransform[2];
        float lastW=-1f,lastH=-1f;
        public bool IsCinematicPortrait { get; private set; }
        public int ChangedElements=>originals.Count;

        static RectTransform Find(RectTransform parent,string name)
        {
            if(!parent)return null;
            foreach(var rt in parent.GetComponentsInChildren<RectTransform>(true))
                if(rt.name==name)return rt;
            return null;
        }

        static RectTransform FindDirect(RectTransform parent,string name)
        {
            if(!parent)return null;
            foreach(Transform child in parent)
                if(child.name==name)return child as RectTransform;
            return null;
        }

        public void Initialize(RoyalGoldV3ManagementScreens manager)
        {
            controller=manager;
        }

        static void At(RectTransform rt,float x,float top,float w,float h)
        {
            rt.anchorMin=rt.anchorMax=rt.pivot=new Vector2(0f,1f);
            rt.anchoredPosition=new Vector2(x,-top);
            rt.sizeDelta=new Vector2(Mathf.Max(1f,w),Mathf.Max(1f,h));
        }

        void Remember(RectTransform rt)
        {
            if(rt&&!originals.ContainsKey(rt))
                originals.Add(rt,new LayoutState(rt));
        }

        public bool RefreshPortrait()
        {
            if(!controller || !controller.IsOpen ||
               controller.CurrentDestination!=
                   RoyalGoldV3ManagementScreens.Destination.Training)
            {
                IsCinematicPortrait=false;
                return false;
            }
            var root=controller.VisibleRoot;
            if(!root)return false;
            if(!left || !left.IsChildOf(root))
            {
                left=Find(root,"V3 / Featured military unit");
                hero=FindDirect(left,"Approved source / training-archer-isolated-v1");
                identity=Find(left,"Training / unit manuscript");
                nameText=Find(left,"Unit or hero title");
                detailText=Find(left,"Unit profile and composition");
                t1=Find(left,"Training unit card T1");
                t2=Find(left,"Training unit card T2");
                portraits[0]=Find(t1,"Approved source / training-archer-isolated-v1");
                portraits[1]=Find(t2,"Approved source / training-archer-isolated-v1");
                labels[0]=Find(t1,"Button label");
                labels[1]=Find(t2,"Button label");
            }
            if(!left || !hero || !identity || !nameText || !detailText ||
               !t1 || !t2 || !portraits[0] || !portraits[1] ||
               !labels[0] || !labels[1])
                return false;
            float w=left.rect.width,h=left.rect.height;
            // The canonical full-screen layout has separate rules for
            // landscape; leave it alone until a real A/B verifies those.
            if(w<200f || h<240f || root.rect.width>root.rect.height)
            {
                IsCinematicPortrait=false;
                return false;
            }
            // Reflow only when actual geometry was changed by the source
            // controller. No repeated transforms/allocations each frame.
            float cardW=Mathf.Clamp(w*.30f,180f,245f);
            float cardH=Mathf.Clamp(h*.29f,150f,205f);
            float firstY=Mathf.Clamp(h*.19f,104f,145f);
            if(IsCinematicPortrait && Mathf.Abs(w-lastW)<.5f &&
               Mathf.Abs(h-lastH)<.5f &&
               Mathf.Abs(t1.anchoredPosition.x-15f)<.5f &&
               Mathf.Abs(t1.anchoredPosition.y+firstY)<.5f)
                return true;

            foreach(var rt in new[]{hero,identity,nameText,detailText,
                t1,t2,portraits[0],portraits[1],labels[0],labels[1]})
                Remember(rt);

            float contentLeft=cardW+29f;
            // One horizontal title and subtitle, seated in ONE new stone
            // plaque. The unmodified previous screen had illegible dark
            // names floating in an empty brown pane.
            At(identity,contentLeft-12f,9f,w-contentLeft+2f,111f);
            At(nameText,contentLeft,21f,w-contentLeft-25f,41f);
            At(detailText,contentLeft,63f,w-contentLeft-23f,49f);
            var title=nameText.GetComponent<Text>();
            var subtitle=detailText.GetComponent<Text>();
            if(title){title.fontSize=29;title.color=new Color(.97f,.91f,.80f,1f);}
            if(subtitle){subtitle.fontSize=20;subtitle.color=new Color(.90f,.84f,.72f,1f);}

            // Full isolated archer is the artistic hero of the scene. Draw
            // it on the right at real character scale, with the two canonical
            // T1/T2 choices at left. Its art stays independent of input.
            At(hero,contentLeft-6f,126f,w-contentLeft+3f,
                Mathf.Max(138f,h-132f));
            At(t1,15f,firstY,cardW,cardH);
            At(t2,15f,firstY+cardH+14f,cardW,cardH);
            for(int i=0;i<2;i++)
            {
                At(portraits[i],-4f,-6f,cardW+8f,cardH-48f);
                At(labels[i],9f,cardH-77f,cardW-18f,74f);
                var text=labels[i].GetComponent<Text>();
                if(text)
                {
                    text.fontSize=21;
                    text.color=new Color(.99f,.95f,.85f,1f);
                    text.alignment=TextAnchor.MiddleCenter;
                }
            }
            lastW=w;lastH=h;IsCinematicPortrait=true;
            return true;
        }

        void OnDestroy(){Restore();}
        public void Restore()
        {
            foreach(var original in originals.Values)original.Restore();
            originals.Clear();
            left=hero=identity=nameText=detailText=t1=t2=null;
            lastW=lastH=-1f;IsCinematicPortrait=false;
        }
    }
}
