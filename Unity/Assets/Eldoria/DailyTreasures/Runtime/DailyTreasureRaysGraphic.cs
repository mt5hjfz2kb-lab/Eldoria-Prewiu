using UnityEngine;
using UnityEngine.UI;

namespace Eldoria.DailyTreasures
{
    /// <summary>
    /// Restrained amber volumetric-looking rays BEHIND the original illustrated
    /// chest. Reuses the Canvas material, makes no new textures per frame, and
    /// cannot block a touch. Subtle geometrical VFX, not replacement chest art.
    /// </summary>
    [RequireComponent(typeof(CanvasRenderer))]
    public sealed class DailyTreasureRaysGraphic : MaskableGraphic
    {
        [Range(0f,1f)] public float Intensity;
        static readonly Color Core=new Color(1f,.71f,.27f,1f);
        static readonly Color Fade=new Color(.73f,.36f,.12f,0f);
        const int Count=10;

        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();
            if(Intensity<=.002f)return;
            var rect=GetPixelAdjustedRect();
            float radius=Mathf.Min(rect.width,rect.height)*.48f;
            Vector2 center=rect.center+new Vector2(0f,rect.height*.06f);
            for(int i=0;i<Count;i++)
            {
                float angle=(i+0.15f)*Mathf.PI*2f/Count;
                float width=i%3==0?0.070f:0.044f;
                float len=radius*(.72f+(i%4)*.065f);
                var start=center+new Vector2(Mathf.Cos(angle),Mathf.Sin(angle))
                    *(radius*.09f);
                var left=center+new Vector2(Mathf.Cos(angle-width),
                    Mathf.Sin(angle-width))*len;
                var right=center+new Vector2(Mathf.Cos(angle+width),
                    Mathf.Sin(angle+width))*len;
                int n=vh.currentVertCount;
                var origin=new Color(Core.r,Core.g,Core.b,
                    (i%2==0?0.46f:0.33f)*Intensity);
                vh.AddVert(new Vector3(start.x,start.y),origin,Vector2.zero);
                vh.AddVert(new Vector3(left.x,left.y),Fade,Vector2.zero);
                vh.AddVert(new Vector3(right.x,right.y),Fade,Vector2.zero);
                vh.AddTriangle(n,n+1,n+2);
            }
        }
        public void SetIntensity(float value)
        {
            float clamped=Mathf.Clamp01(value);
            if(Mathf.Abs(Intensity-clamped)<.006f)return;
            Intensity=clamped;
            SetVerticesDirty();
        }
    }
}
