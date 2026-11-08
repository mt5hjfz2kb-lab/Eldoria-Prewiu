using UnityEngine;
using UnityEngine.UI;

namespace Eldoria.BreachWatch
{
    /// <summary>
    /// Two separately rendered miniature forged hammers for the real Bastion
    /// upgrade action. Nine-slicing the hammer glyph INSIDE a button plate
    /// crushed it into horizontal '=' streaks across the title in native
    /// screenshots. This independent transparent uGUI component has no raycast
    /// and keeps the icon at a fixed physical visual size on every device.
    /// </summary>
    [RequireComponent(typeof(CanvasRenderer))]
    public sealed class BreachWatchCrossedHammers : MaskableGraphic
    {
        protected override void Awake()
        {
            base.Awake();
            raycastTarget=false;
            color=Color.white;
        }

        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();
            var rect=GetPixelAdjustedRect();
            var size=Mathf.Min(rect.width,rect.height);
            if(size<14f)return;
            var center=rect.center;
            var dark=new Color(.11f,.055f,.03f,1f);
            var bronze=new Color(.56f,.29f,.11f,1f);
            var gold=new Color(.93f,.70f,.37f,1f);
            var shine=new Color(1f,.89f,.65f,1f);

            void Stroke(Vector2 a,Vector2 b,float width,Color fill)
            {
                Vector2 d=b-a;
                if(d.sqrMagnitude<.0001f)return;
                var n=new Vector2(-d.y,d.x).normalized*(width*.5f);
                int v=vh.currentVertCount;
                vh.AddVert(a+n,fill,Vector2.zero);
                vh.AddVert(a-n,fill,Vector2.zero);
                vh.AddVert(b-n,fill,Vector2.zero);
                vh.AddVert(b+n,fill,Vector2.zero);
                vh.AddTriangle(v,v+1,v+2);
                vh.AddTriangle(v,v+2,v+3);
            }

            void Hammer(Vector2 direction)
            {
                direction.Normalize();
                var normal=new Vector2(-direction.y,direction.x);
                var bottom=center-direction*size*.33f;
                var top=center+direction*size*.27f;
                Stroke(bottom+new Vector2(.7f,-.65f),top+new Vector2(.7f,-.65f),
                    size*.14f,dark);
                Stroke(bottom,top,size*.10f,bronze);
                Stroke(bottom-normal*size*.019f,top-normal*size*.019f,
                    size*.027f,gold);
                // Raised anvil head: a real perpendicular forging tool,
                // not two diagonals pasted as an X.
                var headCenter=top+direction*size*.015f;
                var headA=headCenter-normal*size*.19f;
                var headB=headCenter+normal*size*.19f;
                Stroke(headA+Vector2.down*.7f,headB+Vector2.down*.7f,
                    size*.20f,dark);
                Stroke(headA,headB,size*.14f,gold);
                Stroke(headA+direction*size*.018f,headB+direction*size*.018f,
                    size*.027f,shine);
                // Small near-vertical metal head end cap.
                Stroke(headA-direction*size*.048f,headA+direction*size*.048f,
                    size*.06f,bronze);
                Stroke(headB-direction*size*.048f,headB+direction*size*.048f,
                    size*.06f,bronze);
            }
            Hammer(new Vector2(1f,1f));
            Hammer(new Vector2(-1f,1f));
            // A restrained golden pivot glint on the crossed forged handles.
            Stroke(center+Vector2.left*size*.038f,
                   center+Vector2.right*size*.038f,
                   size*.035f,shine);
        }
    }
}
