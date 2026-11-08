using UnityEngine;
using UnityEngine.UI;

namespace Eldoria.BreachWatch
{
    /// <summary>
    /// New portrait-oriented carved stone/bronze frame for Vigilia's contextual
    /// building display. Detailed original low-poly metalwork, not a screenshot
    /// background or inherited HUD border. Responds to exact RectTransform size.
    /// </summary>
    [RequireComponent(typeof(CanvasRenderer))]
    public sealed class BreachWatchFrameEngraving : MaskableGraphic
    {
        static readonly Color Shadow = new Color(.026f,.023f,.020f,.94f);
        static readonly Color Copper = new Color(.47f,.25f,.12f,.95f);
        static readonly Color Burnished = new Color(.73f,.49f,.28f,.95f);
        static readonly Color Glint = new Color(.98f,.79f,.44f,.89f);

        protected override void Awake()
        {
            base.Awake();
            raycastTarget=false;
            color=Color.white;
        }
        protected override void OnPopulateMesh(VertexHelper mesh)
        {
            mesh.Clear();
            Rect r=GetPixelAdjustedRect();
            if(r.width<50 || r.height<70)return;
            float l=r.xMin,t=r.yMax,ri=r.xMax,b=r.yMin;
            float cut=Mathf.Min(20f,Mathf.Min(r.width,r.height)*.072f);
            Vector2[] border={
                new Vector2(l+cut,t-7),new Vector2(ri-cut,t-7),
                new Vector2(ri-7,t-cut),new Vector2(ri-7,b+cut),
                new Vector2(ri-cut,b+7),new Vector2(l+cut,b+7),
                new Vector2(l+7,b+cut),new Vector2(l+7,t-cut)
            };

            void Stroke(Vector2 a,Vector2 c,float width,Color ink)
            {
                var d=c-a;
                if(d.sqrMagnitude<.001f)return;
                var n=new Vector2(-d.y,d.x).normalized*(width*.5f);
                int o=mesh.currentVertCount;
                mesh.AddVert(a+n,ink,Vector2.zero);
                mesh.AddVert(a-n,ink,Vector2.zero);
                mesh.AddVert(c-n,ink,Vector2.zero);
                mesh.AddVert(c+n,ink,Vector2.zero);
                mesh.AddTriangle(o,o+1,o+2);
                mesh.AddTriangle(o,o+2,o+3);
            }
            void Loop(Vector2[] points,float width,Color ink)
            {
                for(int i=0;i<points.Length;i++)
                    Stroke(points[i],points[(i+1)%points.Length],width,ink);
            }
            void Diamond(Vector2 c,float x,float y,Color bright)
            {
                Vector2[] points={
                    c+new Vector2(0,y),c+new Vector2(x,0),
                    c+new Vector2(0,-y),c+new Vector2(-x,0)
                };
                int idx=mesh.currentVertCount;
                mesh.AddVert(c,bright,Vector2.zero);
                for(int j=0;j<4;j++)
                    mesh.AddVert(points[j],j==0?Glint:Copper,Vector2.zero);
                for(int j=0;j<4;j++)
                    mesh.AddTriangle(idx,idx+j+1,idx+(j+1)%4+1);
                Loop(points,1.35f,Shadow);
                Stroke(c+new Vector2(-x*.30f,-1f),c+new Vector2(x*.30f,1f),.85f,Glint);
            }

            // Carved bevel: black relief shadow, aged copper groove, thin
            // raised ivory-gold edges. The center is kept dark for live text.
            for(int i=0;i<border.Length;i++)
            {
                var a=border[i];var c=border[(i+1)%border.Length];
                Stroke(a+new Vector2(0,-2f),c+new Vector2(0,-2f),4.6f,Shadow);
                Stroke(a,c,2.9f,Copper);
                Stroke(a+new Vector2(0,.75f),c+new Vector2(0,.75f),
                    1.18f,i==0||i==1||i==7?Glint:Burnished);
            }
            // Fine inner lip is isolated from actual material stretching.
            var inner=new []{
                new Vector2(l+cut+9,t-15),new Vector2(ri-cut-9,t-15),
                new Vector2(ri-15,t-cut-9),new Vector2(ri-15,b+cut+9),
                new Vector2(ri-cut-9,b+15),new Vector2(l+cut+9,b+15),
                new Vector2(l+15,b+cut+9),new Vector2(l+15,t-cut-9)
            };
            Loop(inner,.85f,new Color(.47f,.38f,.27f,.56f));
            // Real metallic center pins. Limited to fixed-size corners so
            // nine-slice stretching never deforms hardware ornamentation.
            var pad=cut+4f;
            foreach(var c in new[]{
                new Vector2(l+pad,t-pad),new Vector2(ri-pad,t-pad),
                new Vector2(l+pad,b+pad),new Vector2(ri-pad,b+pad)})
            {
                Diamond(c,3.1f,3.1f,Burnished);
            }
            // The diamond at each end evokes the Breach without glowing
            // holograms or large panels over the playable city.
            Diamond(new Vector2((l+ri)*.5f,t-11),8f,11f,Glint);
            Diamond(new Vector2((l+ri)*.5f,b+11),7f,10f,Burnished);
            float leftLabel=l+38, rightLabel=ri-38;
            Stroke(new Vector2(leftLabel,t-11),new Vector2((l+ri)*.5f-14,t-11),
                .75f,new Color(.63f,.44f,.28f,.75f));
            Stroke(new Vector2((l+ri)*.5f+14,t-11),new Vector2(rightLabel,t-11),
                .75f,new Color(.63f,.44f,.28f,.75f));
            // Short side engraving slits; corners remain coherent across
            // portrait and wide mobile profiles.
            for(int k=0;k<2;k++)
            {
                float yy=k==0?t-cut-24:b+cut+24;
                Stroke(new Vector2(l+8,yy),new Vector2(l+12,yy+(k==0?-12:12)),
                    1.15f,Burnished);
                Stroke(new Vector2(ri-8,yy),new Vector2(ri-12,yy+(k==0?-12:12)),
                    1.15f,Burnished);
            }
        }
    }
}
