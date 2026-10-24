using UnityEngine;

namespace Eldoria.World4XCanon
{
    // Editable art layout authored by ChatGPT from the owner's three PNGs.
    // All coordinates are the existing world's local X/Z, shared at every zoom.
    // No Qwen output, gameplay objects, colliders, alternate scene or image backdrop.
    public static class RegionOneAuthoredComposition
    {
        public const string Revision = "director-layout-20261022-p0-composition-v2";
        public struct Grove
        {
            public Vector2 centre;
            public float width, depth, angle;
            public int stems;
            public Grove(float x,float z,float w,float d,float degrees,int stems=58)
            { centre=new Vector2(x,z);width=w;depth=d;angle=degrees*Mathf.Deg2Rad;this.stems=stems; }
        }
        // Varied grove sizes leave broad meadow corridors between river, paths
        // and the Breach. Outer groves continue the SAME land beyond the frame.
        // P0 director composition: six unequal woodland gestures and small
        // satellites frame broad navigable meadows. Counts are art decisions,
        // not a fixed 58 trees stamped into every circular grove.
        public static readonly Grove[] Groves = {
            // North-west: a long broken shoulder around the lake clearing.
            new Grove(-330,248,62,23,-18,72),
            new Grove(-256,221,38,17,28,36),
            new Grove(-215,166,46,20,-38,44),
            new Grove(-288,131,18,12,12,12),
            new Grove(-362,173,22,13,-15,17),
            // North-east: diagonal escarpment woodlands with open hilltops.
            new Grove(310,251,55,24,-34,64),
            new Grove(258,205,36,18,-25,34),
            new Grove(197,172,27,14,21,22),
            new Grove(358,141,31,16,30,28),
            new Grove(381,70,20,12,-18,14),
            // Northern river bank: narrow riparian ribbons, never across water.
            new Grove(-24,254,18,39,11,42),
            new Grove(7,179,28,14,-13,24),
            new Grove(93,285,31,18,14,26),
            new Grove(145,316,19,12,-21,12),
            // Western sweep: irregular edge, large meadow between its arms.
            new Grove(-359,-38,58,21,34,65),
            new Grove(-302,-81,32,16,13,29),
            new Grove(-234,-20,23,14,-29,19),
            new Grove(-178,92,22,13,25,16),
            new Grove(-134,152,17,11,-17,12),
            // Southern river valley: extended woods with scattered outliers.
            new Grove(-250,-223,63,25,-23,75),
            new Grove(-192,-169,34,19,28,37),
            new Grove(-143,-102,30,15,30,29),
            new Grove(-308,-162,19,12,-8,12),
            new Grove(-105,-297,34,18,-24,28),
            new Grove(-47,-249,18,12,14,12),
            // Eastern lake ring leaves clear water and an open central approach.
            new Grove(266,-117,55,20,-29,61),
            new Grove(203,-74,26,16,-36,24),
            new Grove(151,-105,20,13,21,17),
            new Grove(100,-258,49,17,12,44),
            new Grove(36,-192,24,13,-28,18),
            new Grove(178,-210,18,11,18,10),
            new Grove(298,-202,23,12,-21,17),
            // Quiet framing accents around the Breach, not a uniform ring.
            new Grove(119,114,35,16,-27,28),
            new Grove(190,48,17,12,24,11),
            // Continued real geography beyond the overview frame.
            new Grove(-410,356,47,20,29,45),
            new Grove(-192,397,55,24,-16,58),
            new Grove(-58,469,31,17,19,26),
            new Grove(183,405,62,23,14,63),
            new Grove(335,457,35,18,-23,32),
            new Grove(-355,-342,51,21,-22,48)
        };
        public static Vector2[] Centres()
        {
            var a=new Vector2[Groves.Length];
            for(int i=0;i<a.Length;i++)a[i]=Groves[i].centre;
            return a;
        }
        public static float Woodland(float x,float z)
        {
            float bx=x-RegionOneCanonicalLandscape.BreachOffsetX;
            float bz=z-RegionOneCanonicalLandscape.BreachOffsetZ;
            if(bx*bx+bz*bz<52f*52f)return 0;
            float result=0;
            for(int i=0;i<Groves.Length;i++)
            {
                var g=Groves[i];
                float dx=x-g.centre.x,dz=z-g.centre.y;
                if(Mathf.Abs(dx)>g.width*1.5f||Mathf.Abs(dz)>g.width*1.5f)continue;
                float c=Mathf.Cos(g.angle),s=Mathf.Sin(g.angle);
                float u=(c*dx+s*dz)/g.width,v=(-s*dx+c*dz)/g.depth;
                // Three overlapping crown masses, a deliberate crescent clearing.
                float d=Mathf.Min(u*u+v*v,
                    (u-.55f)*(u-.55f)*2.3f+(v+.12f)*(v+.12f)*1.8f);
                float edge=1f+.10f*Mathf.Sin(u*8f+v*4f+i);
                float value=Mathf.Clamp01((edge-d)*2.8f);
                float clearing=(u+.45f)*(u+.45f)+(v+.62f)*(v+.62f);
                value*=Mathf.SmoothStep(0f,1f,Mathf.InverseLerp(.08f,.32f,clearing));
                result=Mathf.Max(result,value);
            }
            return result;
        }
        static float Meadow(float x,float z,float cx,float cz,float rx,float rz)
        {
            float u=(x-cx)/rx,v=(z-cz)/rz;
            return Mathf.SmoothStep(0f,1f,Mathf.Clamp01(1f-u*u-v*v));
        }
        public static Color Ground(float x,float z,float grain)
        {
            Color grass=Color.Lerp(new Color(.66f,.65f,.32f),
                new Color(.76f,.70f,.38f),.42f+grain*.24f);
            // Honey uplands north/east, cool river basin, open southern meadow.
            float gold=Meadow(x,z,230,160,235,160)+Meadow(x,z,-300,-190,190,160);
            float sage=Meadow(x,z,-120,80,105,310)+Meadow(x,z,90,-170,175,110);
            grass=Color.Lerp(grass,new Color(.82f,.72f,.39f),Mathf.Clamp01(gold)*.67f);
            grass=Color.Lerp(grass,new Color(.49f,.61f,.33f),Mathf.Clamp01(sage)*.46f);
            return grass;
        }
        // Fractures are drawn paths with connected secondary branches, not
        // radial sine spokes. Root coordinates relative to the canonical Breach.
        internal static readonly Vector2[][] Faults = {
            new[]{new Vector2(-3,-9),new Vector2(3,-2),new Vector2(-2,7),new Vector2(5,15),new Vector2(1,25),new Vector2(10,36),new Vector2(7,52),new Vector2(17,69)},
            new[]{new Vector2(1,-2),new Vector2(-8,-14),new Vector2(-5,-27),new Vector2(-17,-39),new Vector2(-13,-53),new Vector2(-27,-68)},
            new[]{new Vector2(-2,7),new Vector2(-17,12),new Vector2(-26,23),new Vector2(-40,20),new Vector2(-51,32),new Vector2(-70,35)},
            new[]{new Vector2(5,15),new Vector2(20,11),new Vector2(32,20),new Vector2(47,16),new Vector2(62,28)},
            new[]{new Vector2(-8,-14),new Vector2(-24,-12),new Vector2(-32,-24),new Vector2(-48,-20),new Vector2(-65,-33)},
            new[]{new Vector2(1,-2),new Vector2(18,-9),new Vector2(29,-23),new Vector2(45,-25),new Vector2(57,-40),new Vector2(72,-43)},
            new[]{new Vector2(-26,23),new Vector2(-23,38),new Vector2(-35,49)},
            new[]{new Vector2(32,20),new Vector2(33,38),new Vector2(46,48)},
            new[]{new Vector2(-32,-24),new Vector2(-37,-42),new Vector2(-48,-51)},
            new[]{new Vector2(29,-23),new Vector2(23,-39),new Vector2(34,-54)}
        };
        public static bool FaultAllowed(float x,float z)
        {
            // Preserve the established Valoria approach and all visible water.
            return x*x+z*z>=48f*48f && !RegionOneHybridTerrainArt.IsOpenWater(x,z) &&
                !RegionOneCanonicalLandscape.IsCoast(x,z);
        }
        public static Color Breach(Color ground,float x,float z)
        {
            Vector2 p=new Vector2(x-RegionOneCanonicalLandscape.BreachOffsetX,
                                  z-RegionOneCanonicalLandscape.BreachOffsetZ);
            float radius=p.magnitude;
            if(radius>90f || x*x+z*z<48f*48f)return ground;
            Color originalGround=ground;
            float approachFade=Mathf.SmoothStep(0f,1f,Mathf.InverseLerp(48f,70f,Mathf.Sqrt(x*x+z*z)));
            float nearest=1000f;
            foreach(var line in Faults)for(int i=1;i<line.Length;i++)
            {
                Vector2 ab=line[i]-line[i-1];
                float t=Mathf.Clamp01(Vector2.Dot(p-line[i-1],ab)/ab.sqrMagnitude);
                nearest=Mathf.Min(nearest,Vector2.Distance(p,line[i-1]+t*ab));
            }
            float basin=Mathf.SmoothStep(0f,1f,Mathf.Clamp01((67f-radius)/57f));
            ground=Color.Lerp(ground,new Color(.36f,.29f,.37f),basin*.70f);
            float fade=Mathf.Clamp01((85f-radius)/32f);
            float rim=Mathf.Clamp01((3.8f-nearest)/3.8f)*fade;
            ground=Color.Lerp(ground,new Color(.48f,.39f,.35f),rim*.60f);
            float crack=Mathf.Clamp01((1.5f-nearest)/1.1f)*fade;
            ground=Color.Lerp(ground,new Color(.15f,.12f,.21f),crack*.95f);
            float glow=Mathf.Clamp01((.9f-nearest)/.9f)*
                Mathf.Clamp01((28f-radius)/24f);
            Color result=Color.Lerp(ground,new Color(.75f,.27f,.78f),glow*.85f);
            return Color.Lerp(originalGround,result,approachFade);
        }
    }
}
