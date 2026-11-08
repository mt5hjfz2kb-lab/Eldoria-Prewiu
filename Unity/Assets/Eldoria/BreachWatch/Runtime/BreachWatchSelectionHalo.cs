using UnityEngine;
using UnityEngine.UI;

namespace Eldoria.BreachWatch
{
    /// <summary>
    /// Restrained original ember halo around the currently active navigation
    /// medallion. A real transparent uGUI geometry layer; it cannot intercept
    /// touches or obscure the world and does not use the old HUD visual kit.
    /// </summary>
    [RequireComponent(typeof(CanvasRenderer))]
    public sealed class BreachWatchSelectionHalo : MaskableGraphic
    {
        const int Segments = 48;
        protected override void Awake()
        {
            base.Awake();
            raycastTarget = false;
            color = Color.white;
        }

        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();
            var r=GetPixelAdjustedRect();
            var center=r.center;
            float radius=Mathf.Min(r.width,r.height)*.5f;
            if(radius < 15f)return;

            // Faint outer ember bloom, a narrow warm metal reflection,
            // and one chiseled interior lip. Not a futuristic neon circle.
            AddRing(vh,center,radius*.79f,radius*.99f,
                new Color(.78f,.31f,.11f,.23f),
                new Color(.78f,.31f,.11f,0f));
            AddRing(vh,center,radius*.73f,radius*.78f,
                new Color(1f,.69f,.30f,.83f),
                new Color(.70f,.33f,.13f,.34f));
            AddRing(vh,center,radius*.66f,radius*.68f,
                new Color(.98f,.60f,.21f,.53f),
                new Color(.98f,.60f,.21f,.20f));
        }

        static void AddRing(VertexHelper vh,Vector2 center,float inner,float outer,
            Color innerColor,Color outerColor)
        {
            int start=vh.currentVertCount;
            for(int i=0;i<=Segments;i++)
            {
                float angle=i*Mathf.PI*2f/Segments;
                var dir=new Vector2(Mathf.Cos(angle),Mathf.Sin(angle));
                vh.AddVert(center+dir*inner,innerColor,Vector2.zero);
                vh.AddVert(center+dir*outer,outerColor,Vector2.zero);
            }
            for(int i=0;i<Segments;i++)
            {
                int a=start+i*2;
                vh.AddTriangle(a,a+2,a+1);
                vh.AddTriangle(a+1,a+2,a+3);
            }
        }
    }
}
