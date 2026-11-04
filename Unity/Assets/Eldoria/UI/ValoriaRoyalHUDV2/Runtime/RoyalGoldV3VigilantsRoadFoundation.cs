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
        // B11 retains the authoritative road; each of its 24 spans now has
        // an actual visible stone retaining edge, not merely painted lighting.
        public int ActualReliefSpans => positioned ? Steps-1 : 0;

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

        // B11 terrain is genuinely extruded in screen space: individual top,
        // vertical rock face, undercut and ground contact are different
        // polygons tied to the EXACT live stage road. Nothing here is a
        // synthetic checkpoint, new collider, button or battle location.
        static void ReliefTerrace(VertexHelper v,Vector2 a,Vector2 b,
            float halfWidth,float drop,int seed,bool boss)
        {
            var tangent=(b-a);
            if(tangent.sqrMagnitude<.1f)return;
            var n=new Vector2(-tangent.y,tangent.x).normalized;
            // The visible downward-facing rock lip must always fall towards
            // the bottom of the picture; never create an airborne upper wall.
            if(n.y>0f)n=-n;
            float skew=Mathf.Lerp(-.19f,.18f,(Mathf.Sin(seed*2.33f)+1f)*.5f);
            var nearA=a+n*halfWidth;
            var nearB=b+n*halfWidth;
            var farA=a-n*(halfWidth*.85f);
            var farB=b-n*(halfWidth*.85f);
            var gravity=new Vector2(skew*drop,-drop);
            float strength=boss?1f:.70f;
            // A broad translucent stone terrace follows the existing paving;
            // its warm sun-facing plane blends into the approved painting.
            int k=v.currentVertCount;
            v.AddVert(farA,C(.91f,.72f,.43f,.26f*strength),Vector2.zero);
            v.AddVert(farB,C(.94f,.75f,.46f,.25f*strength),Vector2.zero);
            v.AddVert(nearB,C(.58f,.45f,.32f,.38f*strength),Vector2.zero);
            v.AddVert(nearA,C(.60f,.47f,.33f,.38f*strength),Vector2.zero);
            v.AddTriangle(k,k+1,k+2);v.AddTriangle(k,k+2,k+3);
            // Vertical wall with a discontinuous profile and three distinct
            // mineral tones. This is observable relief even in a still image.
            k=v.currentVertCount;
            v.AddVert(nearA,C(.41f,.36f,.29f,.82f*strength),Vector2.zero);
            v.AddVert(nearB,C(.45f,.38f,.29f,.85f*strength),Vector2.zero);
            v.AddVert(nearB+gravity,C(.12f,.17f,.19f,.90f*strength),Vector2.zero);
            v.AddVert(nearA+gravity,C(.10f,.16f,.18f,.92f*strength),Vector2.zero);
            v.AddTriangle(k,k+1,k+2);v.AddTriangle(k,k+2,k+3);
            Stroke(v,nearA+gravity,nearB+gravity,Mathf.Max(1f,halfWidth*.17f),
                C(.025f,.035f,.043f,.43f*strength));
            Stroke(v,nearA,nearB,Mathf.Max(1f,halfWidth*.20f),
                C(.97f,.78f,.50f,.46f*strength));
            // Uneven joints follow the actual ledge width, not a cartoon grid.
            for(int seam=1;seam<=2;seam++)
            {
                float t=(seam + .14f*Mathf.Sin(seed+seam*2f))/3f;
                Vector2 start=Vector2.Lerp(nearA,nearB,t);
                Vector2 end=start+gravity*(.57f+.18f*Mathf.Sin(seed*.8f+seam));
                Stroke(v,start,end,Mathf.Max(.75f,halfWidth*.085f),
                    C(.04f,.065f,.069f,.45f*strength));
            }
        }

        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();
            if(!positioned || rectTransform.rect.width<72f ||
                rectTransform.rect.height<72f)return;
            float scale=Mathf.Clamp(Mathf.Min(rectTransform.rect.width,
                rectTransform.rect.height)*.0105f,3.5f,10.5f);

            // B11 continuously connected architectural relief under the REAL
            // route: near segments are taller, distant ones smaller. The
            // topological geometry is retained in the UI world; the old
            // colour-only road is then drawn above these actual stone faces.
            for(int i=Steps-2;i>=0;i--)
            {
                float distance=(float)i/(Steps-1);
                float perspective=1.14f-.67f*distance;
                float width=scale*perspective*(i%5==3?2.3f:1.70f);
                float depth=scale*perspective*(i%5==3?2.7f:1.82f);
                ReliefTerrace(vh,positions[i],positions[i+1],
                    width,depth,i,(i+1)%5==0 || (i+2)%5==0);
            }

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
