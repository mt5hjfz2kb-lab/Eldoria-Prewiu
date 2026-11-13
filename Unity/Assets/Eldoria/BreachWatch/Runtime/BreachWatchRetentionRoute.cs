using Eldoria.UI.RoyalGold.V2;
using UnityEngine;
using UnityEngine.UI;

namespace Eldoria.BreachWatch
{
    // New Vigilia diagram, with no retired scenic texture or frame. The
    // canonical controller remains the authority for stage positions/progress
    // and its existing milestone labels. This view cannot issue commands.
    public sealed class BreachWatchRetentionRoute : MaskableGraphic
    {
        public RoyalGoldV3StageTrailGraphic Source;
        readonly Vector2[] points=new Vector2[25];
        int lastCleared=int.MinValue,lastChapter;
        Rect lastBounds,lastUv;

        public void Synchronize()
        {
            if(!Source)return;
            if(lastCleared==Source.Cleared && lastChapter==Source.Chapter &&
               lastBounds==rectTransform.rect && lastUv==Source.ArtworkUv)return;
            lastCleared=Source.Cleared;lastChapter=Source.Chapter;
            lastBounds=rectTransform.rect;lastUv=Source.ArtworkUv;
            for(int i=0;i<points.Length;i++)points[i]=Source.StagePosition(i+1);
            SetVerticesDirty();
        }

        void Link(VertexHelper mesh,Vector2 a,Vector2 b,float width,Color ink)
        {
            var direction=(b-a).normalized;
            var normal=new Vector2(-direction.y,direction.x)*width*.5f;
            int start=mesh.currentVertCount;
            mesh.AddVert(a-normal,ink,Vector2.zero);mesh.AddVert(a+normal,ink,Vector2.zero);
            mesh.AddVert(b+normal,ink,Vector2.zero);mesh.AddVert(b-normal,ink,Vector2.zero);
            mesh.AddTriangle(start,start+1,start+2);mesh.AddTriangle(start,start+2,start+3);
        }

        void Diamond(VertexHelper mesh,Vector2 p,float radius,Color ink)
        {
            int start=mesh.currentVertCount;
            mesh.AddVert(p+Vector2.up*radius,ink,Vector2.zero);
            mesh.AddVert(p+Vector2.right*radius,ink,Vector2.zero);
            mesh.AddVert(p+Vector2.down*radius,ink,Vector2.zero);
            mesh.AddVert(p+Vector2.left*radius,ink,Vector2.zero);
            mesh.AddTriangle(start,start+1,start+2);mesh.AddTriangle(start,start+2,start+3);
        }

        protected override void OnPopulateMesh(VertexHelper mesh)
        {
            mesh.Clear();if(!Source)return;
            var shadow=new Color(.04f,.035f,.03f,1f);
            var bronze=new Color(.51f,.36f,.19f,1f);
            var gold=new Color(.94f,.73f,.37f,1f);
            var violet=new Color(.68f,.47f,.83f,1f);
            int cleared=Mathf.Clamp(Source.Cleared-(Source.Chapter-1)*25,0,25);
            for(int i=1;i<points.Length;i++)
            {
                Link(mesh,points[i-1],points[i],5f,shadow);
                Link(mesh,points[i-1],points[i],2f,i<=cleared?gold:bronze);
            }
            float size=Mathf.Clamp(Mathf.Min(rectTransform.rect.width,rectTransform.rect.height)*.018f,3.5f,8f);
            for(int i=0;i<points.Length;i++)
            {
                float radius=(i+1)%5==0?size*1.45f:size;
                if(i==cleared)Diamond(mesh,points[i],radius+5f,new Color(.35f,.20f,.43f,1f));
                Diamond(mesh,points[i],radius+1.5f,i==cleared?violet:i<cleared?gold:bronze);
                Diamond(mesh,points[i],radius,shadow);
                if(i<=cleared)Diamond(mesh,points[i],radius*.48f,i==cleared?violet:gold);
            }
        }
    }
}
