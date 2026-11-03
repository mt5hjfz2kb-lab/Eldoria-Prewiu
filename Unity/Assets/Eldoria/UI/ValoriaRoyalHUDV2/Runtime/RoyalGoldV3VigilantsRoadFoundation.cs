using UnityEngine;
using UnityEngine.UI;

namespace Eldoria.UI.RoyalGold.V2
{
    /// <summary>
    /// Real 25-stage road masonry, physically grounded on the same projected
    /// path as RoyalGoldV3StageTrailGraphic. It is under the live stage UI,
    /// over the approved landscape and never catches touches.
    /// No new path, state, rewards, textures, scene, camera or canvas.
    /// Rebuilds only when the authoritative stage/crop/geometry changes.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class RoyalGoldV3VigilantsRoadFoundation : MaskableGraphic
    {
        const int Steps=25;
        readonly Vector2[] positions=new Vector2[Steps];
        int cleared=-1, current=1;
        bool positioned;
        public int BoundStageCount => positioned?Steps:0;
        public int Cleared => cleared;
        public int NextStage => current;

        public static RoyalGoldV3VigilantsRoadFoundation Install(
            RectTransform parent,int sibling)
        {
            var go=new GameObject("Vigilants / actual path stone foundation",
                typeof(RectTransform),typeof(CanvasRenderer),
                typeof(RoyalGoldV3VigilantsRoadFoundation));
            go.layer=parent.gameObject.layer;
            var r=(RectTransform)go.transform;
            r.SetParent(parent,false);
            r.anchorMin=Vector2.zero;
            r.anchorMax=Vector2.one;
            r.offsetMin=r.offsetMax=Vector2.zero;
            r.SetSiblingIndex(Mathf.Clamp(sibling,0,parent.childCount-1));
            var mesh=go.GetComponent<RoyalGoldV3VigilantsRoadFoundation>();
            mesh.raycastTarget=false;
            mesh.color=Color.white;
            return mesh;
        }

        public Vector2 StagePosition(int step)
            => positions[Mathf.Clamp(step,1,Steps)-1];

        public void BindRealRoad(RoyalGoldV3StageTrailGraphic trail,int realCleared)
        {
            if(!trail)return;
            bool changed=!positioned;
            for(int i=0;i<Steps;i++)
            {
                Vector2 local=rectTransform.InverseTransformPoint(
                    trail.transform.TransformPoint(trail.StagePosition(i+1)));
                if((positions[i]-local).sqrMagnitude>.04f)changed=true;
                positions[i]=local;
            }
            positioned=true;
            int next=(Mathf.Max(0,realCleared)%Steps)+1;
            if(cleared!=realCleared||current!=next)changed=true;
            cleared=realCleared;
            current=next;
            if(changed)SetVerticesDirty();
        }

        static Color C(float r,float g,float b,float a)
            => new Color(r,g,b,a);

        static void Quad(VertexHelper v,Vector2 a,Vector2 b,
            Vector2 c,Vector2 d,Color tone)
        {
            int k=v.currentVertCount;
            v.AddVert(a,tone,Vector2.zero);
            v.AddVert(b,tone,Vector2.zero);
            v.AddVert(c,tone,Vector2.zero);
            v.AddVert(d,tone,Vector2.zero);
            v.AddTriangle(k,k+1,k+2);
            v.AddTriangle(k,k+2,k+3);
        }

        static void Stroke(VertexHelper v,Vector2 a,Vector2 b,
            float width,Color tint)
        {
            Vector2 dir=b-a;
            if(dir.sqrMagnitude<.01f)return;
            Vector2 n=new Vector2(-dir.y,dir.x).normalized*(width*.5f);
            Quad(v,a-n,a+n,b+n,b-n,tint);
        }

        static void Ellipse(VertexHelper v,Vector2 center,
            float rx,float ry,Color tint,int sections=18)
        {
            int k=v.currentVertCount;
            v.AddVert(center,tint,Vector2.zero);
            for(int i=0;i<=sections;i++)
            {
                float t=i*Mathf.PI*2f/sections;
                v.AddVert(center+new Vector2(
                    Mathf.Cos(t)*rx,Mathf.Sin(t)*ry),
                    tint,Vector2.zero);
                if(i>0)v.AddTriangle(k,k+i,k+i+1);
            }
        }

        static void Rock(VertexHelper v,Vector2 center,float radius,
            int seed,Color tint)
        {
            int k=v.currentVertCount;
            v.AddVert(center,tint,Vector2.zero);
            const int edges=12;
            for(int i=0;i<=edges;i++)
            {
                int index=i%edges;
                float t=i*Mathf.PI*2f/edges;
                float variance=.85f+.12f*Mathf.Sin(index*2.17f+seed*.61f);
                var p=center+new Vector2(Mathf.Cos(t)*radius*variance,
                    Mathf.Sin(t)*radius*.59f*variance);
                v.AddVert(p,tint,Vector2.zero);
                if(i>0)v.AddTriangle(k,k+i,k+i+1);
            }
        }

        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();
            if(!positioned || rectTransform.rect.width<72f ||
                rectTransform.rect.height<72f)return;
            float scale=Mathf.Clamp(Mathf.Min(rectTransform.rect.width,
                rectTransform.rect.height)*.0105f,3.5f,10.5f);

            // Physical road cutting: contact shadow is displaced downhill,
            // cold stone face beneath and a sunlit lip follows exactly the
            // existing true 25-point itinerary. No decorative extra route.
            for(int i=0;i<Steps-1;i++)
            {
                Vector2 a=positions[i],b=positions[i+1];
                float taper=.78f+.32f*(1f-(float)i/Steps);
                float w=scale*taper;
                var down=new Vector2(w*.21f,-w*.51f);
                Stroke(vh,a+down,b+down,w*1.70f,
                    C(.012f,.024f,.030f,.51f));
                Stroke(vh,a,b,w*1.24f,
                    C(.25f,.22f,.18f,.31f));
                Stroke(vh,a+new Vector2(0f,w*.14f),
                    b+new Vector2(0f,w*.14f),w*.36f,
                    C(.91f,.72f,.41f,.27f));
                if((i+1)<=cleared)
                    Stroke(vh,a,b,w*.19f,C(1f,.78f,.34f,.52f));
            }

            // Every actual stage has a shallow stone socket, contact shadow
            // and upper plane. The existing disc, guardian number and banner
            // remain entirely ABOVE these platforms.
            for(int i=0;i<Steps;i++)
            {
                bool boss=(i+1)%5==0;
                bool done=i+1<=cleared;
                bool live=i+1==current;
                float size=scale*(boss?2.35f:1.60f);
                var p=positions[i];
                var down=new Vector2(size*.11f,-size*.45f);
                Ellipse(vh,p+down,size*1.07f,size*.62f,
                    C(.008f,.023f,.021f,boss?.79f:.67f));
                Rock(vh,p+new Vector2(0f,-size*.11f),size,
                    i,C(.17f,.20f,.20f,boss?.93f:.81f));
                Rock(vh,p+new Vector2(-size*.08f,size*.21f),
                    size*.85f,i+7,
                    boss?C(.57f,.42f,.27f,.67f):C(.52f,.49f,.42f,.42f));
                Ellipse(vh,p+new Vector2(-size*.10f,size*.19f),
                    size*.43f,size*.19f,
                    live?C(.99f,.74f,.31f,.64f):
                    done?C(.80f,.64f,.35f,.38f):
                         C(.52f,.57f,.52f,.26f));
                if(boss)
                {
                    // A raised buttress only at REAL guardian milestones,
                    // set into the path, not a row of unrelated badges.
                    float foot=size*.69f;
                    Stroke(vh,p+new Vector2(-foot,-size*.47f),
                        p+new Vector2(-foot*.65f,size*.35f),
                        scale*.66f,C(.08f,.10f,.11f,.73f));
                    Stroke(vh,p+new Vector2(foot,-size*.47f),
                        p+new Vector2(foot*.64f,size*.35f),
                        scale*.66f,C(.08f,.10f,.11f,.73f));
                    Stroke(vh,p+new Vector2(-foot*.61f,size*.32f),
                        p+new Vector2(foot*.60f,size*.32f),
                        scale*.32f,C(.94f,.68f,.32f,.62f));
                    Ellipse(vh,p+new Vector2(0f,-size*.51f),
                        size*.82f,size*.15f,
                        C(.025f,.028f,.025f,.39f));
                }
            }
        }
    }
}
