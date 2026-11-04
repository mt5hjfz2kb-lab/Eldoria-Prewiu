using UnityEngine;
using UnityEngine.UI;

namespace Eldoria.UI.RoyalGold.V2
{
    /// <summary>
    /// Environmental illumination bound to the ACTUAL winding stage road.
    /// References the same cover UV as the approved painted scenery: lanterns,
    /// warm dawn, deep valley fog and the next real checkpoint are lit in-place.
    /// No photographic panel, duplicate texture, button or new camera/Canvas.
    /// The sunlight and stage pools move gently at a 5 Hz mesh budget.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class RoyalGoldV3VigilantsWorldLight : MaskableGraphic
    {
        static readonly Vector2[] Lanterns =
        {
            new Vector2(.126f,.322f),new Vector2(.355f,.405f),
            new Vector2(.490f,.515f),new Vector2(.630f,.607f),
            new Vector2(.736f,.725f)
        };

        Rect cover = new Rect(0f,0f,1f,1f);
        Vector2 nextPoint;
        int cleared, currentStage = 1;
        float nextRepaint;
        public int LastCleared => cleared;
        public int CurrentStage => currentStage;
        public Rect CoverUv => cover;

        public static RoyalGoldV3VigilantsWorldLight Install(
            RectTransform parent,int sibling)
        {
            var go = new GameObject("Vigilants / interactive landscape lighting",
                typeof(RectTransform),typeof(CanvasRenderer),
                typeof(RoyalGoldV3VigilantsWorldLight));
            go.layer=parent.gameObject.layer;
            var rt=(RectTransform)go.transform;
            rt.SetParent(parent,false);
            rt.anchorMin=Vector2.zero;
            rt.anchorMax=Vector2.one;
            rt.offsetMin=rt.offsetMax=Vector2.zero;
            rt.SetSiblingIndex(Mathf.Clamp(sibling,0,parent.childCount-1));
            var graphic=go.GetComponent<RoyalGoldV3VigilantsWorldLight>();
            graphic.raycastTarget=false;
            graphic.color=Color.white;
            return graphic;
        }

        public void SetState(Rect exactCover,int actualCleared,int liveStage,
            Vector2 pointInThisRect)
        {
            exactCover.width=Mathf.Max(.001f,exactCover.width);
            exactCover.height=Mathf.Max(.001f,exactCover.height);
            int next=Mathf.Clamp(liveStage,1,25);
            int progress=Mathf.Max(0,actualCleared);
            if(cover==exactCover && cleared==progress &&
                currentStage==next && (nextPoint-pointInThisRect).sqrMagnitude<.0001f)
                return;
            cover=exactCover;
            cleared=progress;
            currentStage=next;
            nextPoint=pointInThisRect;
            SetVerticesDirty();
        }

        void Update()
        {
            if(!isActiveAndEnabled||!gameObject.activeInHierarchy||
                Time.unscaledTime<nextRepaint)return;
            nextRepaint=Time.unscaledTime+.20f;
            SetVerticesDirty();
        }

        static Color C(float r,float g,float b,float a)
            => new Color(r,g,b,a);

        static void Radial(VertexHelper v,Vector2 center,float rx,float ry,
            Color light,int sides=24)
        {
            if(rx<=0f||ry<=0f)return;
            int k=v.currentVertCount;
            v.AddVert(center,light,Vector2.zero);
            for(int i=0;i<=sides;i++)
            {
                float a=i*2f*Mathf.PI/sides;
                var at=center+new Vector2(Mathf.Cos(a)*rx,Mathf.Sin(a)*ry);
                v.AddVert(at,C(light.r,light.g,light.b,0f),Vector2.zero);
                if(i>0)v.AddTriangle(k,k+i,k+i+1);
            }
        }

        static void Shade(VertexHelper v,Rect area,
            float height,Color near,Color away)
        {
            int k=v.currentVertCount;
            v.AddVert(new Vector2(area.xMin,area.yMin),near,Vector2.zero);
            v.AddVert(new Vector2(area.xMax,area.yMin),near,Vector2.zero);
            v.AddVert(new Vector2(area.xMax,area.yMin+height),away,Vector2.zero);
            v.AddVert(new Vector2(area.xMin,area.yMin+height),away,Vector2.zero);
            v.AddTriangle(k,k+1,k+2);v.AddTriangle(k,k+2,k+3);
        }

        Vector2 ProjectUv(Rect r,Vector2 uv)
        {
            return new Vector2(r.xMin+r.width*(uv.x-cover.x)/cover.width,
                r.yMin+r.height*(uv.y-cover.y)/cover.height);
        }

        bool Inside(Rect r,Vector2 v)
        {
            return r.Contains(v);
        }

        protected override void OnPopulateMesh(VertexHelper v)
        {
            v.Clear();
            Rect r=GetPixelAdjustedRect();
            if(r.width<50f||r.height<50f)return;
            float unit=Mathf.Min(r.width,r.height);
            float pulse=.5f+.5f*Mathf.Sin(Time.unscaledTime*1.1f);
            // Atmosphere from the approved scene's authentic setting sun:
            // no artificial hard-edged spotlight over the world texture.
            var sun=ProjectUv(r,new Vector2(.85f,.83f));
            if(Inside(r,sun))
            {
                Radial(v,sun,r.width*.29f,r.height*.27f,
                    C(1f,.72f,.34f,.065f+.018f*pulse),32);
                Radial(v,sun,r.width*.15f,r.height*.21f,
                    C(1f,.88f,.54f,.042f),24);
            }
            // Deep, cold valley mist belongs BEHIND the true foreground;
            // warm highlights remain concentrated on actual authored rocks.
            var valley=ProjectUv(r,new Vector2(.42f,.51f));
            if(Inside(r,valley))
                Radial(v,valley,r.width*.39f,r.height*.20f,
                    C(.51f,.71f,.84f,.12f+.024f*pulse),28);
            var falls=ProjectUv(r,new Vector2(.84f,.17f));
            if(Inside(r,falls))
                Radial(v,falls,r.width*.19f,r.height*.19f,
                    C(.57f,.78f,.88f,.074f),24);
            // Receding rock bank darkens toward the viewer, giving an
            // unmistakable near/middle/far material distinction.
            Shade(v,r,r.height*.23f,
                C(.018f,.034f,.033f,.22f),
                C(.028f,.043f,.051f,0f));
            // Each pool is anchored to a real torch already painted into
            // the approved scenery. No fictitious light source is drawn.
            for(int i=0;i<Lanterns.Length;i++)
            {
                var point=ProjectUv(r,Lanterns[i]);
                if(!Inside(r,point))continue;
                float distance=1f-.13f*i;
                float flicker=.90f+.10f*Mathf.Sin(Time.unscaledTime*1.3f+i*1.94f);
                Radial(v,point,unit*.097f*distance,unit*.11f*distance,
                    C(1f,.51f,.15f,.28f*flicker),20);
                Radial(v,point,unit*.037f*distance,unit*.064f*distance,
                    C(1f,.84f,.43f,.25f*flicker),18);
            }
            // The current, genuinely unlocked stage illuminates its own
            // stone underneath the existing interactive road markers.
            // This does not generate new markers or change progress/rewards.
            if(currentStage>=1&&currentStage<=25)
            {
                float size=Mathf.Clamp(unit*.075f,10f,67f);
                float a=.20f+.085f*pulse;
                Radial(v,nextPoint,size*1.8f,size*.82f,
                    C(1f,.62f,.23f,a),28);
                Radial(v,nextPoint+new Vector2(0f,size*.19f),
                    size*.64f,size*.44f,C(1f,.85f,.51f,.17f),20);
                Radial(v,nextPoint+new Vector2(0f,-size*.10f),
                    size*.65f,size*.20f,C(.05f,.04f,.03f,.29f),18);
            }
        }
    }
}
