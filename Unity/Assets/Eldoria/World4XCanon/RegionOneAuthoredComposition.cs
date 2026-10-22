using UnityEngine;

namespace Eldoria.World4XCanon
{
    // Editable art layout authored by ChatGPT from the owner's three PNGs.
    // All coordinates are the existing world's local X/Z, shared at every zoom.
    // No Qwen output, gameplay objects, colliders, alternate scene or image backdrop.
    public static class RegionOneAuthoredComposition
    {
        public const string Revision = "director-layout-20261022-v1";
        public struct Grove
        {
            public Vector2 centre;
            public float width, depth, angle;
            public Grove(float x,float z,float w,float d,float degrees)
            { centre=new Vector2(x,z);width=w;depth=d;angle=degrees*Mathf.Deg2Rad; }
        }
        // Varied grove sizes leave broad meadow corridors between river, paths
        // and the Breach. Outer groves continue the SAME land beyond the frame.
        public static readonly Grove[] Groves = {
            new Grove(-315,218,43,25,-22),new Grove(-208,153,31,22,18),
            new Grove(-365,-72,39,23,30),new Grove(-255,-203,42,26,-18),
            new Grove(-113,-306,29,21,22),new Grove(-28,231,27,19,-15),
            new Grove(126,270,41,23,12),new Grove(315,204,43,25,-28),
            new Grove(367,61,31,23,36),new Grove(274,-112,39,25,-25),
            new Grove(91,-241,36,24,20),new Grove(-176,389,39,25,-12),
            new Grove(188,386,44,28,15),new Grove(-410,338,37,22,31),
            new Grove(-351,-337,43,25,-24),new Grove(382,-327,39,26,16),
            new Grove(-65,464,34,22,-20),new Grove(338,441,40,25,32),
            new Grove(-116,67,25,17,18),new Grove(114,93,30,20,-22),
            new Grove(-132,-76,31,19,24),new Grove(127,-92,29,21,-28),
            new Grove(-21,137,24,17,15),new Grove(-300,100,22,17,-16),
            new Grove(218,21,24,16,27),new Grove(-241,-17,28,18,-12),
            new Grove(22,-161,25,17,22),new Grove(209,175,26,18,16)
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
