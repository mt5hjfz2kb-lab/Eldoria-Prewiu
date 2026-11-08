using UnityEngine;
using UnityEngine.UI;

namespace Eldoria.BreachWatch
{
    /// <summary>
    /// Solid slate-leather inset beneath real live building text. The approved
    /// carved frame remains at the edges, but its original bright transparent
    /// interior must not let the moving world collide visually with lettering.
    /// Does not alter or reuse any production HUD component.
    /// </summary>
    [RequireComponent(typeof(CanvasRenderer))]
    public sealed class BreachWatchPanelInk : MaskableGraphic
    {
        const int Columns = 28;
        const int Rows = 42;
        protected override void Awake()
        {
            base.Awake();
            raycastTarget = false;
            color = Color.white;
        }

        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();
            var bounds=GetPixelAdjustedRect();
            if(bounds.width<30f || bounds.height<50f) return;
            for(int y=0;y<=Rows;y++)
            {
                float v=y/(float)Rows;
                for(int x=0;x<=Columns;x++)
                {
                    float u=x/(float)Columns;
                    float grain=Mathf.PerlinNoise(u*7.7f+11.8f,v*9.3f+21.1f);
                    float fine=Mathf.PerlinNoise(u*25.8f+5.3f,v*31.1f+6.1f);
                    // Near-black stone with quietly varied warm mineral tones.
                    float tint=(grain-.5f)*.025f+(fine-.5f)*.009f;
                    float edge=Mathf.Clamp01(Mathf.Abs(u-.5f)*1.9f);
                    float red=.068f+tint-edge*.016f+v*.008f;
                    float green=.071f+tint-edge*.015f+v*.006f;
                    float blue=.075f+tint-edge*.016f+v*.002f;
                    // The original OwnerReference gold border has clipped
                    // shoulders and a tapered lower tip. A rectangular black
                    // underlying panel showed up OUTSIDE that border in the
                    // last genuine Unity image. Carve the *actual* ink mesh
                    // into a matching transparent silhouette. In particular,
                    // make its top and bottom corners disappear instead of
                    // displaying a cheap black box behind the approved trim.
                    float distTop=(1f-v)*bounds.height;
                    float distBottom=v*bounds.height;
                    float sideInset=9f;
                    if(distTop<30f)
                        sideInset+=Mathf.Pow(1f-distTop/30f,1.45f)*28f;
                    if(distBottom<33f)
                        sideInset+=Mathf.Pow(1f-distBottom/33f,1.35f)*23f;
                    float sideDist=Mathf.Min(u,1f-u)*bounds.width;
                    float sideFade=Mathf.SmoothStep(0f,1f,
                        Mathf.Clamp01((sideDist-sideInset)/3.4f));
                    float topFade=Mathf.SmoothStep(0f,1f,
                        Mathf.Clamp01((distTop-2.8f)/5.5f));
                    float bottomFade=Mathf.SmoothStep(0f,1f,
                        Mathf.Clamp01((distBottom-5.3f)/8.0f));
                    float alpha=.997f*sideFade*topFade*bottomFade;
                    var node=UIVertex.simpleVert;
                    node.position=new Vector3(bounds.xMin+u*bounds.width,
                        bounds.yMin+v*bounds.height,0);
                    node.color=new Color(red,green,blue,alpha);
                    node.uv0=Vector2.zero;
                    vh.AddVert(node);
                }
            }
            for(int y=0;y<Rows;y++)
            for(int x=0;x<Columns;x++)
            {
                int i=y*(Columns+1)+x;
                vh.AddTriangle(i,i+1,i+Columns+1);
                vh.AddTriangle(i+1,i+Columns+2,i+Columns+1);
            }
        }
    }
}
