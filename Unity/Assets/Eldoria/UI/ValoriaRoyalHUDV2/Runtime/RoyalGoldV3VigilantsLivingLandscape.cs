using UnityEngine;
using UnityEngine.UI;

namespace Eldoria.UI.RoyalGold.V2
{
    /// <summary>
    /// One original authored landscape rendered as a living, material-aware
    /// terrain instead of a flat photographic UI quad. Its existing cover UV
    /// stays EXACTLY unchanged, and the real 25 stage positions drive the
    /// sculpted road's lighting. No new texture, camera, canvas or false stage.
    ///
    /// 28x24 static tinted quads at most; never recompute per frame. Existing
    /// light/foreground/stone layers animate independently and are raycast-off.
    /// Scouts use RawImage's untouched normal path.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class RoyalGoldV3VigilantsLivingLandscape : RawImage
    {
        const int Columns=28,Rows=24,Steps=25;
        readonly Vector2[] road=new Vector2[Steps];
        bool live,roadBound;
        int cleared=-1;
        public bool LivingScene => live;
        public int RoadStageCount => roadBound?Steps:0;
        public int ActualCleared => cleared;

        public void SetLivingScene(bool active,
            RoyalGoldV3StageTrailGraphic actualTrail,int actualCleared)
        {
            bool changed=live!=active;
            live=active;
            if(active && actualTrail)
            {
                var bounds=rectTransform.rect;
                if(bounds.width<1f||bounds.height<1f)return;
                for(int i=0;i<Steps;i++)
                {
                    Vector2 p=rectTransform.InverseTransformPoint(
                        actualTrail.transform.TransformPoint(
                            actualTrail.StagePosition(i+1)));
                    Vector2 uv=new Vector2(
                        (p.x-bounds.xMin)/bounds.width,
                        (p.y-bounds.yMin)/bounds.height);
                    if((road[i]-uv).sqrMagnitude>.000002f)changed=true;
                    road[i]=uv;
                }
                roadBound=true;
            }
            int progress=Mathf.Max(0,actualCleared);
            if(cleared!=progress)changed=true;
            cleared=progress;
            if(changed)SetVerticesDirty();
        }

        static float Smooth(float a,float b,float v)
        {
            float t=Mathf.Clamp01((v-a)/Mathf.Max(.00001f,b-a));
            return t*t*(3f-2f*t);
        }

        static float SquaredDistanceToSegment(Vector2 p,Vector2 a,Vector2 b)
        {
            Vector2 direction=b-a;
            float denom=direction.sqrMagnitude;
            if(denom<.00001f)return (p-a).sqrMagnitude;
            float t=Mathf.Clamp01(Vector2.Dot(p-a,direction)/denom);
            return (p-(a+direction*t)).sqrMagnitude;
        }

        Color DepthTone(float x,float y)
        {
            // screen-relative location uses the SAME image cover crop as
            // original RawImage, so rocks and 25-stage bases never slide.
            float u=uvRect.x+x*uvRect.width;
            float v=uvRect.y+y*uvRect.height;

            // Three deliberately different material families:
            // distant mountain/sun haze; shaded fortress/river basin;
            // heavy foreground rocks, vegetation and earth.
            float foreground=1f-Smooth(.19f,.47f,v);
            float distance=Smooth(.53f,.82f,v);
            float valley=(1f-Smooth(.20f,.40f,v))*Smooth(.50f,.75f,v);
            float sun=Mathf.Exp(-(
                Mathf.Pow((u-.80f)/.34f,2f)+
                Mathf.Pow((v-.84f)/.23f,2f))*1.25f);
            float fortress=Mathf.Exp(-(
                Mathf.Pow((u-.77f)/.17f,2f)+
                Mathf.Pow((v-.70f)/.24f,2f))*1.5f);
            float cliffs=(1f-Smooth(.31f,.46f,v))*
                (Smooth(.54f,.75f,u)+1f-Smooth(.17f,.35f,u))*.5f;

            // Material values are purposeful: the near ground carries more
            // contrast and cool shadow, while the castle remains sunset-warm.
            float r=.94f-.20f*foreground-.09f*valley
                +.055f*distance+.08f*sun+.015f*fortress;
            float g=.95f-.18f*foreground-.045f*valley
                +.025f*distance+.020f*sun;
            float b=.97f-.26f*foreground+.005f*valley
                +.040f*distance-.055f*sun;
            r-=.09f*cliffs;g-=.075f*cliffs;b-=.075f*cliffs;

            if(roadBound)
            {
                Vector2 p=new Vector2(x,y);
                float best=1f;
                int closest=0;
                for(int i=0;i<Steps-1;i++)
                {
                    float d=SquaredDistanceToSegment(p,road[i],road[i+1]);
                    if(d<best){best=d;closest=i;}
                }
                // Stone road has a sunlit raised top and a cold undercut.
                // Real saved progression warms only the completed stretch.
                float roadTop=Mathf.Exp(-best*570f);
                float roadShoulder=Mathf.Exp(-best*125f)-roadTop;
                float advanced=closest<cleared?1f:.25f;
                r+=roadTop*(.14f+.10f*advanced);
                g+=roadTop*(.07f+.06f*advanced);
                b-=roadTop*.06f;
                r-=roadShoulder*.12f;
                g-=roadShoulder*.10f;
                b-=roadShoulder*.115f;
            }

            // The same local falloff gives the whole materialized environment
            // readable volume at 390 px without creating visible photo seams.
            float framed=Smooth(0f,.12f,x)*Smooth(0f,.11f,1f-x)
                *Smooth(0f,.06f,y)*Smooth(0f,.10f,1f-y);
            float edge=.88f+.12f*framed;
            Color tint=color;
            return new Color(Mathf.Clamp01(r*edge*tint.r),
                Mathf.Clamp01(g*edge*tint.g),
                Mathf.Clamp01(b*edge*tint.b),tint.a);
        }

        protected override void OnPopulateMesh(VertexHelper vh)
        {
            if(!live)
            {
                base.OnPopulateMesh(vh);
                return;
            }
            vh.Clear();
            Rect r=GetPixelAdjustedRect();
            if(r.width<50f||r.height<50f)return;
            // Shared vertices + regular indices; no per-cell allocations.
            for(int j=0;j<=Rows;j++)
            for(int i=0;i<=Columns;i++)
            {
                float x=(float)i/Columns,y=(float)j/Rows;
                Vector2 local=new Vector2(r.xMin+r.width*x,
                    r.yMin+r.height*y);
                Vector2 originalUv=new Vector2(
                    uvRect.x+x*uvRect.width,uvRect.y+y*uvRect.height);
                vh.AddVert(local,DepthTone(x,y),originalUv);
            }
            for(int j=0;j<Rows;j++)
            for(int i=0;i<Columns;i++)
            {
                int index=j*(Columns+1)+i;
                vh.AddTriangle(index,index+1,index+Columns+2);
                vh.AddTriangle(index,index+Columns+2,index+Columns+1);
            }
        }
    }
}
