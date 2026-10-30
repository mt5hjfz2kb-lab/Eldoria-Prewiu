using UnityEngine;
using UnityEngine.UI;

namespace Eldoria.UI.RoyalGold.V2
{
    /// <summary>
    /// Crónica Real's live selected-building miniature: lightweight medieval
    /// architectural engraving, not a screenshot/static HUD mockup. It stays
    /// tied to the actual selected building and never blocks player input.
    /// </summary>
    public sealed class RoyalGoldV3BuildingEngravingGraphic : MaskableGraphic
    {
        public int Kind; // 0 house, 1 timber, 2 barracks, 3 bastion, 4 granary

        Color Tint(Color c) => new Color(c.r * color.r,c.g * color.g,c.b * color.b,c.a * color.a);
        void Quad(VertexHelper vh,float x,float y,float w,float h,Color c1,Color c2){
            if(w<=0||h<=0)return;
            int k=vh.currentVertCount;
            vh.AddVert(new Vector3(x,y),Tint(c1),Vector2.zero);
            vh.AddVert(new Vector3(x,y+h),Tint(c2),Vector2.up);
            vh.AddVert(new Vector3(x+w,y+h),Tint(c2),Vector2.one);
            vh.AddVert(new Vector3(x+w,y),Tint(c1),Vector2.right);
            vh.AddTriangle(k,k+1,k+2);vh.AddTriangle(k,k+2,k+3);
        }
        void Triangle(VertexHelper vh,Vector2 a,Vector2 b,Vector2 c,Color low,Color high){
            int k=vh.currentVertCount;
            vh.AddVert(a,Tint(low),Vector2.zero);
            vh.AddVert(b,Tint(high),Vector2.one);
            vh.AddVert(c,Tint(low),Vector2.right);
            vh.AddTriangle(k,k+1,k+2);
        }
        void Diamond(VertexHelper vh,float x,float y,float r,Color c){
            Triangle(vh,new Vector2(x-r,y),new Vector2(x,y+r),new Vector2(x+r,y),c,c);
            Triangle(vh,new Vector2(x-r,y),new Vector2(x+r,y),new Vector2(x,y-r),c,c);
        }
        protected override void OnPopulateMesh(VertexHelper vh){
            vh.Clear();var r=rectTransform.rect;
            if(r.width<25||r.height<25)return;
            float x=r.xMin,y=r.yMin,w=r.width,h=r.height;
            Color dark=new Color(.027f,.05f,.073f,1),slate=new Color(.13f,.20f,.24f,1),
                gold=new Color(.77f,.58f,.31f,1),stone=new Color(.67f,.63f,.52f,1),
                shade=new Color(.34f,.32f,.30f,1),timber=new Color(.34f,.20f,.12f,1),
                roof=new Color(.28f,.23f,.25f,1),hilite=new Color(.93f,.83f,.59f,1);
            Quad(vh,x,y,w,h,dark,slate);
            Quad(vh,x+3,y+3,w-6,h-6,new Color(.06f,.10f,.12f,1),new Color(.16f,.20f,.22f,1));
            // The small vignette retains a readable architectural silhouette
            // with a local stone base, roof and several illuminated details.
            float bx=x+w*.20f,by=y+h*.22f,bw=w*.60f,bh=h*.44f;
            Quad(vh,bx-5,by-3,bw+10,6,shade,stone);
            if(Kind==3){
                // Canonical square-towered bastion, no old rejected GLB model.
                Quad(vh,bx+4,by,bw-8,bh*.85f,shade,stone);
                Quad(vh,bx-6,by+bh*.15f,bw*.28f,bh*.97f,shade,stone);
                Quad(vh,bx+bw*.75f,by+bh*.15f,bw*.28f,bh*.97f,shade,stone);
                for(int i=0;i<4;i++){
                    Quad(vh,bx-7+i*6,by+bh*1.08f,4,8,stone,hilite);
                    Quad(vh,bx+bw*.75f+i*6,by+bh*1.08f,4,8,stone,hilite);
                }
                Quad(vh,bx+bw*.40f,by,bw*.20f,bh*.44f,dark,timber);
                Quad(vh,bx+bw*.47f,by+bh*.55f,bw*.07f,10,gold,hilite);
            }else{
                Quad(vh,bx,by,bw,bh,Kind==1?timber:shade,Kind==1?new Color(.58f,.41f,.25f,1):stone);
                Triangle(vh,new Vector2(bx-8,by+bh),new Vector2(bx+bw*.48f,by+bh+h*.19f),
                    new Vector2(bx+bw+8,by+bh),roof,new Color(.48f,.35f,.29f,1));
                Quad(vh,bx+bw*.43f,by,bw*.18f,bh*.58f,dark,timber);
                for(int i=0;i<3;i++){
                    float sx=bx+bw*(.1f+i*.30f);
                    Quad(vh,sx,by+bh*.40f,bw*.10f,bh*.15f,dark,gold);
                    Quad(vh,sx+bw*.045f,by+bh*.40f,1,bh*.15f,stone,stone);
                }
                Quad(vh,bx+3,by+bh*.86f,bw-6,2,timber,hilite);
                if(Kind==4){
                    // Granary: hay and raised store sacks, not a second building.
                    for(int i=0;i<3;i++){
                        Quad(vh,bx+bw*.08f+i*8,by+3,7,h*.13f,new Color(.57f,.42f,.19f,1),gold);
                        Diamond(vh,bx+bw*.80f+i*5,by+4,4,new Color(.79f,.65f,.34f,1));
                    }
                }
                if(Kind==1)for(int i=0;i<3;i++)
                    Quad(vh,bx+bw*.05f,by+6+i*5,bw*.25f,3,timber,gold);
                if(Kind==2)for(int i=0;i<3;i++)
                    Triangle(vh,new Vector2(bx+bw*.72f+i*5,by+bh*.12f),
                        new Vector2(bx+bw*.76f+i*5,by+bh*.51f),
                        new Vector2(bx+bw*.80f+i*5,by+bh*.12f),gold,hilite);
            }
            Quad(vh,x+4,y+4,w-8,1,gold,gold);
            Quad(vh,x+4,y+h-5,w-8,1,gold,gold);
            Diamond(vh,x+10,y+10,3,gold);Diamond(vh,x+w-10,y+h-10,3,gold);
        }
    }
}