using UnityEngine;
using UnityEngine.UI;

namespace Eldoria.BreachWatch
{
    /// <summary>
    /// Quiet atmospheric shading for the independently authored city UI.
    /// Does not replace the game camera, world textures or post processing.
    /// Edge-only transparent veil: city center and building targets stay visible.
    /// The mesh belongs entirely to the new Canvas, not to RoyalHUD.
    /// </summary>
    [RequireComponent(typeof(CanvasRenderer))]
    public sealed class BreachWatchAtmosphere : MaskableGraphic
    {
        const int Columns = 18;
        const int Rows = 12;

        protected override void Awake()
        {
            base.Awake();
            raycastTarget = false;
            color = Color.white;
        }

        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();
            var rect = rectTransform.rect;
            var width = rect.width;
            var height = rect.height;
            if(width <= 0 || height <= 0)return;
            var baseVertex = UIVertex.simpleVert;

            for(int row=0;row<=Rows;row++)
            {
                float fy=row/(float)Rows;
                float dy=Mathf.Abs(fy-.5f)*2f;
                for(int col=0;col<=Columns;col++)
                {
                    float fx=col/(float)Columns;
                    float dx=Mathf.Abs(fx-.5f)*2f;
                    // Single, continuous soft fall-off. Never draw a heavy
                    // rectangular panel over the game view.
                    float elliptical=Mathf.Sqrt(dx*dx*.76f+dy*dy*1.24f);
                    float ring=Mathf.Clamp01((elliptical-.20f)/1.18f);
                    // A/B of material compatibility: the production
                    // city is brighter/greener than the owner's approved
                    // twilight concept. A single continuously blended
                    // ink-blue dusk film sits BEHIND all real UI controls,
                    // instead of inventing a second scene, backdrop cards
                    // or additional shadow layers. Small central tint keeps
                    // the real Bastion, road and resources readable.
                    // One quiet edge vignette, retaining the canonical
                    // sunny terrain and model texture fidelity. Stronger
                    // blue-black film experiments 42659088409/42660051677
                    // darkened grass but did NOT solve the owner-reported
                    // card-over-world visual mismatch, so both are rejected.
                    float alpha=.008f+.070f*ring*ring+
                        .019f*Mathf.Pow(dy,3f);
                    baseVertex.position = new Vector3(rect.xMin+width*fx,
                        rect.yMin+height*fy,0f);
                    baseVertex.color = new Color(.025f,.025f,.027f,
                        Mathf.Clamp(alpha,0f,.105f));
                    baseVertex.uv0 = Vector2.zero;
                    vh.AddVert(baseVertex);
                }
            }
            for(int row=0;row<Rows;row++)
            for(int col=0;col<Columns;col++)
            {
                int first=row*(Columns+1)+col;
                vh.AddTriangle(first,first+1,first+Columns+1);
                vh.AddTriangle(first+1,first+Columns+2,first+Columns+1);
            }
        }
    }
}
