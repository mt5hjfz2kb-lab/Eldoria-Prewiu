using UnityEngine;
using UnityEngine.UI;

namespace Eldoria.UI.RoyalGold.V2
{
    /// <summary>
    /// Lightweight gold-edged expedition / battle buttons in the owner-approved
    /// Royal Gold visual language. No shaders, textures, canvases or new actions.
    /// The original RoyalButton remains the sole interactive element.
    /// </summary>
    public sealed class RoyalGoldV3RetentionActionGraphic : MaskableGraphic
    {
        public bool Battle;

        void Plate(VertexHelper mesh,Rect rect,float margin,Color bottom,Color top)
        {
            float left=rect.xMin+margin,right=rect.xMax-margin;
            float low=rect.yMin+margin,high=rect.yMax-margin;
            if(right<=left||high<=low)return;
            var a=new Color(bottom.r*color.r,bottom.g*color.g,
                bottom.b*color.b,bottom.a*color.a);
            var b=new Color(top.r*color.r,top.g*color.g,
                top.b*color.b,top.a*color.a);
            int start=mesh.currentVertCount;
            mesh.AddVert(new Vector3(left,low),a,Vector2.zero);
            mesh.AddVert(new Vector3(left,high),b,Vector2.up);
            mesh.AddVert(new Vector3(right,high),b,Vector2.one);
            mesh.AddVert(new Vector3(right,low),a,Vector2.right);
            mesh.AddTriangle(start,start+1,start+2);
            mesh.AddTriangle(start,start+2,start+3);
        }

        protected override void OnPopulateMesh(VertexHelper mesh)
        {
            mesh.Clear();
            var r=rectTransform.rect;
            if(r.width<24f||r.height<22f)return;
            Plate(mesh,r,0,new Color(.02f,.016f,.02f,.85f),
                new Color(.02f,.016f,.02f,.85f));
            Plate(mesh,r,2f,new Color(.41f,.25f,.08f,1),
                new Color(.94f,.70f,.30f,1));
            Plate(mesh,r,4f,new Color(.11f,.08f,.06f,1),
                new Color(.24f,.17f,.08f,1));
            Plate(mesh,r,6f,
                Battle?new Color(.39f,.035f,.045f,1):new Color(.025f,.19f,.40f,1),
                Battle?new Color(.86f,.16f,.12f,1):new Color(.11f,.49f,.86f,1));
            // Small upper glint, held away from the centered text caption.
            var glint=new Rect(r.xMin+14,r.yMax-12,r.width-28,2.0f);
            Plate(mesh,glint,0f,new Color(1,.85f,.60f,.38f),
                new Color(1,.91f,.67f,.58f));
        }
    }
}
