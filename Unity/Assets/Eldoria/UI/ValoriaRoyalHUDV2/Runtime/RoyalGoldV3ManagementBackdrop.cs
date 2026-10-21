using UnityEngine;
using UnityEngine.UI;

namespace Eldoria.UI.RoyalGold.V2
{
    /// <summary>
    /// Royal Gold V3 military hall. An entirely opaque, authored uGUI backdrop:
    /// midnight-blue vaults, repeated stone arches, burgundy banners, old-gold
    /// engravings. Decorative vertices only; never receives gestures.
    /// No textures, imported art, external assets or paid services.
    /// </summary>
    [AddComponentMenu("Eldoria/UI/V3 Military Hall Backdrop")]
    public sealed class RoyalGoldV3ManagementBackdrop : MaskableGraphic
    {
        static readonly Color Night = new Color(.018f,.032f,.057f,1f);
        static readonly Color Slate = new Color(.039f,.079f,.125f,1f);
        static readonly Color Stone = new Color(.075f,.104f,.145f,.99f);
        static readonly Color Gold = new Color(.72f,.54f,.29f,.85f);
        static readonly Color DimGold = new Color(.27f,.21f,.135f,.85f);
        static readonly Color Burgundy = new Color(.16f,.055f,.085f,.80f);
        static readonly Color Veil = new Color(.025f,.044f,.079f,.95f);

        void Quad(VertexHelper vh, Rect r, Color low, Color high)
        {
            if(r.width<=0||r.height<=0)return;
            int v=vh.currentVertCount;
            vh.AddVert(new Vector3(r.xMin,r.yMin),low,Vector2.zero);
            vh.AddVert(new Vector3(r.xMin,r.yMax),high,Vector2.up);
            vh.AddVert(new Vector3(r.xMax,r.yMax),high,Vector2.one);
            vh.AddVert(new Vector3(r.xMax,r.yMin),low,Vector2.right);
            vh.AddTriangle(v,v+1,v+2);vh.AddTriangle(v+2,v+3,v);
        }

        void Diamond(VertexHelper vh,Vector2 c,float radius,Color tint)
        {
            int v=vh.currentVertCount;
            vh.AddVert(new Vector3(c.x,c.y+radius),tint,Vector2.up);
            vh.AddVert(new Vector3(c.x+radius,c.y),tint,Vector2.right);
            vh.AddVert(new Vector3(c.x,c.y-radius),tint,Vector2.down);
            vh.AddVert(new Vector3(c.x-radius,c.y),tint,Vector2.left);
            vh.AddTriangle(v,v+1,v+2);vh.AddTriangle(v,v+2,v+3);
        }

        void Arch(VertexHelper vh,float x,float y,float width,float height)
        {
            // Columns and pointed tympanum: discreet architectural backdrop.
            float pillar=Mathf.Max(5,width*.105f);
            Color dark=new Color(.05f,.078f,.115f,.95f);
            Color highlight=new Color(.11f,.14f,.165f,.85f);
            Quad(vh,new Rect(x,y,pillar,height*.77f),Stone,highlight);
            Quad(vh,new Rect(x+width-pillar,y,pillar,height*.77f),Stone,highlight);
            Quad(vh,new Rect(x+pillar,y+height*.60f,width-2*pillar,height*.13f),dark,Stone);
            // Keystone crown and glimmer, angled with an actual triangle mesh.
            int v=vh.currentVertCount;
            vh.AddVert(new Vector3(x+width*.5f,y+height),Stone,Vector2.up);
            vh.AddVert(new Vector3(x+pillar,y+height*.73f),dark,Vector2.zero);
            vh.AddVert(new Vector3(x+width-pillar,y+height*.73f),dark,Vector2.right);
            vh.AddTriangle(v,v+1,v+2);
            Quad(vh,new Rect(x+pillar-1,y+height*.55f,width-2*pillar+2,2),
                DimGold,DimGold);
        }

        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();
            Rect r=rectTransform.rect;
            if(r.width<5||r.height<5)return;
            Quad(vh,r,Night,Slate);
            float w=r.width,h=r.height;
            // Large shadowed chambers, visible at either orientation; no city layer.
            float archW=Mathf.Clamp(w*.225f,130f,420f);
            float archH=h*.82f;
            for(int i=0;i<5;i++)
            {
                float x=r.xMin+(i-.7f)*(w/3.8f);
                Arch(vh,x,r.yMin-archH*.23f,archW,archH);
            }
            // V3 ink wash recedes behind the legible parchment/blue royal cards.
            Quad(vh,new Rect(r.xMin,r.yMin,w,h),Veil,
                new Color(.025f,.044f,.075f,.69f));
            // Flanking house banners and restrained raised metal trim.
            float bannerW=Mathf.Clamp(w*.055f,23f,76f);
            for(int i=0;i<2;i++)
            {
                float x=i==0?r.xMin+13:r.xMax-13-bannerW;
                float by=r.yMax-h*.43f;
                Quad(vh,new Rect(x,by,bannerW,h*.33f),Burgundy,
                    new Color(.23f,.08f,.105f,.86f));
                Quad(vh,new Rect(x,by+h*.33f-3,bannerW,3),Gold,Gold);
                Diamond(vh,new Vector2(x+bannerW*.5f,by+h*.23f),
                    Mathf.Min(bannerW*.25f,16),Gold);
                Quad(vh,new Rect(x+bannerW*.5f-1,by+h*.07f,2,h*.12f),
                    DimGold,DimGold);
            }
            float inset=10f;
            Quad(vh,new Rect(r.xMin+inset,r.yMax-7,w-2*inset,2),DimGold,Gold);
            Quad(vh,new Rect(r.xMin+inset,r.yMin+6,w-2*inset,2),DimGold,Gold);
            float d=Mathf.Clamp(w*.012f,4,12);
            Diamond(vh,new Vector2(r.xMin+25,r.yMax-24),d,Gold);
            Diamond(vh,new Vector2(r.xMax-25,r.yMax-24),d,Gold);
            Diamond(vh,new Vector2(r.xMin+25,r.yMin+24),d,Gold);
            Diamond(vh,new Vector2(r.xMax-25,r.yMin+24),d,Gold);
        }
    }
}
