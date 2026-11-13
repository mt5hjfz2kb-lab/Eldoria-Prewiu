using UnityEngine;
using UnityEngine.UI;

namespace Eldoria.DailyTreasures
{
    public enum DailyTreasureGlyphKind { Wood,Stone,Food,Time,Chest }

    /// <summary>
    /// Original small sculpted reward emblems, no emoji/font fallback, no fake loot.
    /// Pure graphic geometry and no raycast targets.
    /// </summary>
    [RequireComponent(typeof(CanvasRenderer))]
    public sealed class DailyTreasureRewardGlyph : MaskableGraphic
    {
        public DailyTreasureGlyphKind Kind;
        VertexHelper mesh;
        float ox,oy,scale;
        static readonly Color Bronze=new Color(.65f,.46f,.22f,1f);
        static readonly Color Shine=new Color(.96f,.78f,.42f,1f);
        static readonly Color Shadow=new Color(.11f,.09f,.08f,1f);
        void Quad(float x0,float y0,float x1,float y1,Color a,Color b)
        {
            int i=mesh.currentVertCount;
            mesh.AddVert(new Vector3(ox+x0*scale,oy+y0*scale),a,Vector2.zero);
            mesh.AddVert(new Vector3(ox+x1*scale,oy+y0*scale),a,Vector2.right);
            mesh.AddVert(new Vector3(ox+x1*scale,oy+y1*scale),b,Vector2.one);
            mesh.AddVert(new Vector3(ox+x0*scale,oy+y1*scale),b,Vector2.up);
            mesh.AddTriangle(i,i+1,i+2);mesh.AddTriangle(i,i+2,i+3);
        }
        void Poly(float x0,float y0,float x1,float y1,float x2,float y2,
            float x3,float y3,Color c)
        {
            int i=mesh.currentVertCount;
            mesh.AddVert(new Vector3(ox+x0*scale,oy+y0*scale),c,Vector2.zero);
            mesh.AddVert(new Vector3(ox+x1*scale,oy+y1*scale),c,Vector2.right);
            mesh.AddVert(new Vector3(ox+x2*scale,oy+y2*scale),c,Vector2.one);
            mesh.AddVert(new Vector3(ox+x3*scale,oy+y3*scale),c,Vector2.up);
            mesh.AddTriangle(i,i+1,i+2);mesh.AddTriangle(i,i+2,i+3);
        }
        void Disc(float x,float y,float r,Color center,Color rim)
        {
            int i=mesh.currentVertCount;
            mesh.AddVert(new Vector3(ox+x*scale,oy+y*scale),center,Vector2.zero);
            for(int k=0;k<=14;k++)
            {
                float angle=k*Mathf.PI*2f/14f;
                mesh.AddVert(new Vector3(ox+(x+Mathf.Cos(angle)*r)*scale,
                    oy+(y+Mathf.Sin(angle)*r)*scale),rim,Vector2.zero);
                if(k>0)mesh.AddTriangle(i,i+k,i+k+1);
            }
        }
        protected override void OnPopulateMesh(VertexHelper vertices)
        {
            mesh=vertices;mesh.Clear();
            var r=GetPixelAdjustedRect();
            scale=Mathf.Min(r.width/64f,r.height/64f);
            ox=r.center.x-32*scale;oy=r.center.y-32*scale;
            switch(Kind)
            {
                case DailyTreasureGlyphKind.Wood:
                {
                    var dark=new Color(.23f,.10f,.06f,1f);
                    var light=new Color(.59f,.32f,.17f,1f);
                    Poly(8,20,52,26,52,35,8,29,dark);
                    Poly(12,24,50,30,50,34,12,28,light);
                    Disc(12,24,7,light,dark);
                    Poly(11,32,47,39,46,48,10,41,dark);
                    Poly(13,36,45,42,45,45,13,39,light);
                    Disc(12,35,6,new Color(.77f,.50f,.28f,1f),dark);
                    Disc(12,35,2,dark,light);
                    break;
                }
                case DailyTreasureGlyphKind.Stone:
                {
                    var dark=new Color(.20f,.22f,.23f,1f);
                    var slate=new Color(.51f,.53f,.53f,1f);
                    var gleam=new Color(.78f,.79f,.76f,1f);
                    Poly(7,20,27,17,34,37,16,46,dark);
                    Poly(17,46,34,37,43,44,30,53,gleam);
                    Poly(27,17,55,22,56,42,34,37,slate);
                    Poly(34,37,56,42,45,50,30,53,dark);
                    break;
                }
                case DailyTreasureGlyphKind.Food:
                {
                    var russet=new Color(.69f,.30f,.10f,1f);
                    var dark=new Color(.34f,.14f,.09f,1f);
                    Disc(29,36,18,russet,dark);
                    Disc(24,39,7,new Color(.93f,.47f,.14f,1f),russet);
                    Poly(36,21,52,10,57,15,43,28,dark);
                    Disc(54,12,5,new Color(.89f,.81f,.60f,1f),Bronze);
                    Disc(58,17,4,new Color(.89f,.81f,.60f,1f),Bronze);
                    break;
                }
                case DailyTreasureGlyphKind.Time:
                {
                    var sand=new Color(.86f,.60f,.18f,1f);
                    Quad(12,50,52,54,Bronze,Shine);
                    Quad(12,10,52,14,Bronze,Shine);
                    Quad(17,15,21,49,Shadow,Bronze);
                    Quad(43,15,47,49,Shadow,Bronze);
                    Poly(23,45,41,45,33,34,31,34,new Color(.44f,.62f,.70f,.75f));
                    Poly(31,29,33,29,41,18,23,18,new Color(.44f,.62f,.70f,.75f));
                    Poly(23,18,41,18,37,24,27,24,sand);
                    Disc(32,30,3,Shine,sand);
                    break;
                }
                case DailyTreasureGlyphKind.Chest:
                {
                    Quad(7,16,57,39,Shadow,Bronze);
                    Quad(12,20,52,35,new Color(.39f,.18f,.09f,1f),
                         new Color(.63f,.33f,.15f,1f));
                    Quad(8,37,56,49,Shadow,Bronze);
                    Quad(12,41,52,46,new Color(.38f,.17f,.10f,1f),Shadow);
                    Quad(19,17,23,49,Shadow,Bronze);
                    Quad(43,17,47,49,Shadow,Bronze);
                    Quad(29,26,35,38,Bronze,Shine);
                    Disc(32,33,3,Shine,Bronze);
                    break;
                }
            }
        }
    }
}
