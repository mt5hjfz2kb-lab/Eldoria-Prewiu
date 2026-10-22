using System;
using UnityEngine;

namespace Eldoria.World4XCanon
{
    /// <summary>
    /// 2.5D/3D hybrid atlas: a purpose-made geographical *surface texture*,
    /// not a projection of the owner's reference screenshot. The real terrain,
    /// selectable 3D entities, camera, persistence and gameplay are unchanged.
    /// One deterministic atlas represents the same XYZ at every zoom.
    ///
    /// All world coordinates are local to Valoria's actual WorldHome; terrain
    /// bounds/offsets come from RegionOneCanonicalLandscape.
    /// No GameObjects, save data, dynamic textures per frame or fake POIs.
    /// </summary>
    public static class RegionOneHybridTerrainArt
    {
        public const int StyleRevision = 1;
        static readonly Color Moss = new Color(.35f,.45f,.23f);
        // The first real panoramic Unity A/B was too grey-olive versus the
        // owner's ochre/golden grassland. Warm only the two grass bases;
        // retain existing sage/forest shading, water and gameplay geography.
        static readonly Color YellowGrass = new Color(.77f,.62f,.31f);
        static readonly Color Prairie = new Color(.63f,.55f,.28f);
        static readonly Color BareEarth = new Color(.64f,.49f,.30f);
        static readonly Color Shale = new Color(.50f,.50f,.40f);
        static readonly Color PineFloor = new Color(.24f,.37f,.21f);
        static readonly Color River = new Color(.16f,.43f,.57f);
        static readonly Color Ripples = new Color(.33f,.62f,.68f);
        static readonly Color RiftGround = new Color(.21f,.17f,.24f);
        static readonly Color RiftGlow = new Color(.57f,.18f,.55f);
        static readonly Vector2[] RidgeSites = {
            new Vector2(-330f,190f),new Vector2(326f,215f),
            new Vector2(-320f,-220f),new Vector2(267f,-170f)
        };

        // Four natural topographic landmarks reuse the existing rock ridge
        // anchors already painted in the atlas. Their subtle 3D elevation
        // brings actual geographic silhouettes to the oblique panorama.
        // Shared with the real terrain mesh: one deterministic geography.
        public static float RidgeElevation(float x, float z)
        {
            float lift=0f;
            for(int i=0;i<RidgeSites.Length;i++)
            {
                float dx=(x-RidgeSites[i].x)/100f;
                float dz=(z-RidgeSites[i].y)/88f;
                float d2=dx*dx+dz*dz;
                if(d2>=2.7f)continue;
                float crown=1f-d2/2.7f;
                lift=Mathf.Max(lift,15f*crown*crown);
            }
            return lift;
        }

        static float Fractal(float x, float z, float seed)
        {
            float a=Mathf.PerlinNoise(x*.008f+seed,z*.009f+seed*.37f);
            float b=Mathf.PerlinNoise(x*.036f+seed*1.19f,z*.034f+seed*2.23f);
            float c=Mathf.PerlinNoise(x*.113f+seed*3.11f,z*.104f+seed*4.19f);
            return a*.55f+b*.32f+c*.13f;
        }

        static float RiverCentre(float z)
        {
            return RegionOneCanonicalLandscape.RiverCentre(z);
        }

        static float WaterMask(float x,float z)
        {
            float bank=0f;
            if(z>-450f&&z<460f)
            {
                float width=4.0f + Mathf.Sin(z*.034f)*.95f;
                float dx=Mathf.Abs(x-RiverCentre(z));
                bank=Mathf.Max(bank,Mathf.Clamp01((width+1.5f-dx)/2.7f));
            }
            // Short, non-parallel tributaries, all tied to the primary river:
            // geography only; no new gathering opportunities.
            float tributary1=RiverCentre(222f);
            if(z>215f&&z<380f)
            {
                float t=(z-215f)/165f;
                float tributaryX=tributary1 + 9f + t*154f + 11f*Mathf.Sin(t*9f);
                bank=Mathf.Max(bank,Mathf.Clamp01((2.9f-Mathf.Abs(x-tributaryX))/1.9f));
            }
            if(z<-110f&&z>-270f)
            {
                float t=(-z-110f)/160f;
                float x0=RiverCentre(-110f);
                float tributaryX=x0 - 12f-t*145f+8f*Mathf.Sin(t*8.7f);
                bank=Mathf.Max(bank,Mathf.Clamp01((2.9f-Mathf.Abs(x-tributaryX))/1.9f));
            }
            float lake1=Mathf.Pow((x-145f)/30f,2f)+Mathf.Pow((z+175f)/18f,2f);
            float lake2=Mathf.Pow((x+222f)/18f,2f)+Mathf.Pow((z-135f)/13f,2f);
            float lacustre=1f-.12f*Mathf.Sin(x*.19f)*Mathf.Cos(z*.17f);
            if(lake1<lacustre)bank=Mathf.Max(bank,Mathf.Clamp01((lacustre-lake1)*12f));
            if(lake2<lacustre)bank=Mathf.Max(bank,Mathf.Clamp01((lacustre-lake2)*12f));
            bank=Mathf.Max(bank,RegionOneCanonicalLandscape.CoastWaterMask(x,z));
            return bank;
        }

        // The SAME water mask paints rivers, tributaries, small lakes and
        // the limited coast in the canonical terrain atlas. All strategic
        // forest impostors (including the fallback) must respect it, rather
        // than testing only the primary river and placing trees in lakes.
        public static bool IsOpenWater(float x,float z)
        {
            return WaterMask(x,z) > .05f;
        }

        static float RoadMask(float x,float z)
        {
            // Paths trace actual winding corridors with pale earth shoulders.
            // No stacked X lines or icons at panoramic zoom.
            float w=Mathf.PerlinNoise(x*.078f+37f,z*.079f+19f);
            float d=RegionOneCanonicalLandscape.RoadDistance(x+(w-.5f)*1.3f,z);
            float centre=Mathf.Clamp01((2.7f-d)/1.8f);
            float shoulder=Mathf.Clamp01((5.2f-d)/3.1f)*.22f;
            return Mathf.Clamp01(centre*.77f+shoulder);
        }

        static uint Hash(int x,int z,int salt)
        {
            unchecked
            {
                uint k=(uint)x*1597334677u ^ (uint)z*3812015801u ^ (uint)salt*2654435761u;
                k^=k>>16; k*=2246822519; k^=k>>13; k*=3266489917;k^=k>>16;
                return k;
            }
        }
        static float Hash01(int x,int z,int salt)
        {
            return (Hash(x,z,salt)&65535u)/65535f;
        }
        static Color Lerp(Color a,Color b,float t) { return Color.Lerp(a,b,Mathf.Clamp01(t)); }

        static void StampTree(Color32[] px,int size,int x,int y,int radius,float variation)
        {
            if(x<radius+2||x>=size-radius-2||y<radius+2||y>=size-radius-2)return;
            Color shade=Lerp(new Color(.10f,.18f,.11f),new Color(.22f,.33f,.16f),variation);
            Color light=Lerp(new Color(.30f,.46f,.20f),new Color(.42f,.49f,.25f),variation);
            int r=Mathf.Max(1,radius);
            for(int dz=-r-1;dz<=r+1;dz++)
                for(int dx=-r-1;dx<=r+1;dx++)
                {
                    int pxX=x+dx,pxY=y+dz;
                    float d=(dx*dx+dz*dz)/(float)(r*r);
                    int idx=pxY*size+pxX;
                    if(d<1f)
                    {
                        float north=(-dz+dx*.24f)/(r*1.8f);
                        Color c=Lerp(shade,light,.45f+north*.40f);
                        px[idx]=Lerp(px[idx],c,.90f);
                    }
                    else if(dx>=0&&dz<=0&&d<1.46f)
                        px[idx]=Lerp(px[idx],new Color(.16f,.20f,.12f),.18f);
                }
        }

        static void StampStone(Color32[] px,int size,int x,int y,int radius,float variation)
        {
            if(x<radius+2||x>=size-radius-2||y<radius+2||y>=size-radius-2)return;
            var pale=Lerp(new Color(.60f,.58f,.45f),new Color(.76f,.68f,.49f),variation);
            var dark=Lerp(new Color(.34f,.34f,.29f),new Color(.50f,.46f,.34f),variation);
            for(int dy=-radius;dy<=radius;dy++)
                for(int dx=-radius;dx<=radius;dx++)
                {
                    if(Mathf.Abs(dx)*.9f+Mathf.Abs(dy)*1.1f>radius*1.45f)continue;
                    Color c=(dy>0||dx>0)?dark:pale;
                    px[(y+dy)*size+x+dx]=c;
                }
        }

        public static Texture2D Bake(int size,float halfWidth,float halfDepth,
                                     float breachX,float breachZ)
        {
            // No external procedural models, shader dependencies or paid API.
            // Generated once at world entry; CPU copy discarded after bake.
            var texture=new Texture2D(size,size,TextureFormat.RGB24,true,false)
            {name="Region 1 original painterly strategic atlas / shared geography"};
            var pixels=new Color32[size*size];
            for(int iy=0;iy<size;iy++)
                for(int ix=0;ix<size;ix++)
                {
                    float x=((ix+.5f)/size*2f-1f)*halfWidth;
                    float z=((iy+.5f)/size*2f-1f)*halfDepth;
                    float n=Fractal(x,z,12f);
                    float m=Mathf.PerlinNoise(x*.057f+87f,z*.053f+36f);
                    float v=Mathf.PerlinNoise(x*.19f+41f,z*.16f+83f);
                    // Stylized tonal variation: readable olive/honey/sage
                    // countryside rather than a uniform brown wash.
                    // Large readable patches of contrasting natural biomes.
                    // Earlier candidate changed only fine noise and was
                    // visually almost flat from the strategic camera.
                    float basin=Mathf.PerlinNoise(x*.013f+123f,z*.014f+195f);
                    float hills=Mathf.PerlinNoise(x*.019f+91f,z*.017f+41f);
                    Color ground=Lerp(Prairie,YellowGrass,.23f+.58f*n);
                    float verdant=Mathf.SmoothStep(.36f,.68f,basin);
                    ground=Lerp(ground,new Color(.35f,.52f,.28f),.42f*(1f-verdant));
                    ground=Lerp(ground,new Color(.81f,.69f,.38f),.37f*verdant);
                    float patchwork=Mathf.PerlinNoise(x*.029f+87f,z*.027f+46f);
                    ground=Lerp(ground,new Color(.37f,.48f,.26f),
                        Mathf.Clamp01((.40f-patchwork)*1.9f)*.33f);
                    ground=Lerp(ground,new Color(.75f,.65f,.40f),
                        Mathf.Clamp01((patchwork-.62f)*1.7f)*.34f);
                    // Panoramic composition must read as interconnected
                    // meadow biomes, not uniformly beige terrain with dots.
                    // These broad, warped chromatic regions are pure landcover:
                    // no gameplay resources, artificial field grid or buildings.
                    float ochreRegion=Mathf.PerlinNoise(
                        (x+15f*Mathf.Sin(z*.011f))*.0071f+217f,
                        (z+17f*Mathf.Sin(x*.009f))*.0082f+63f);
                    float sageRegion=Mathf.PerlinNoise(
                        (x-23f*Mathf.Sin(z*.010f))*.0096f+42f,
                        (z+19f*Mathf.Sin(x*.012f))*.0077f+187f);
                    float sunMeadow=Mathf.SmoothStep(.43f,.69f,ochreRegion);
                    float coolMeadow=Mathf.SmoothStep(.45f,.71f,sageRegion)
                        *(1f-.62f*sunMeadow);
                    ground=Lerp(ground,new Color(.86f,.71f,.39f),sunMeadow*.60f);
                    ground=Lerp(ground,new Color(.39f,.56f,.29f),coolMeadow*.52f);
                    ground=Lerp(ground,Moss,Mathf.Clamp01((.45f-m)*.36f));
                    ground=Lerp(ground,new Color(.78f,.65f,.42f),.10f*v);
                    float meadow=Mathf.Clamp01((.65f-m)*1.7f);
                    ground=Lerp(ground,new Color(.40f,.53f,.27f),meadow*.13f);
                    // Bake directional hill light from the SAME low-frequency
                    // elevation field as RegionOneCanonicalLandscape.SampleHeight
                    // (x*.009+13, z*.010+31). The old unrelated noise field
                    // produced disconnected camouflage-like blotches.
                    // Four Perlin samples, same cost as the previous shade.
                    float hillsideWest=Mathf.PerlinNoise(
                        (x-23f)*.009f+13f,z*.010f+31f);
                    float hillsideEast=Mathf.PerlinNoise(
                        (x+23f)*.009f+13f,z*.010f+31f);
                    float hillsideSouth=Mathf.PerlinNoise(
                        x*.009f+13f,(z-23f)*.010f+31f);
                    float hillsideNorth=Mathf.PerlinNoise(
                        x*.009f+13f,(z+23f)*.010f+31f);
                    float reliefSlope=(hillsideWest-hillsideEast)*.78f+
                        (hillsideNorth-hillsideSouth)*.46f;
                    float distanceFromHome=Mathf.Sqrt(x*x+z*z);
                    float distantRelief=Mathf.SmoothStep(0f,1f,
                        Mathf.Clamp01((distanceFromHome-225f)/240f));
                    // Real gameplay near the city stays on its already
                    // approved level ground; relief is purely distant art.
                    ground*=Mathf.Clamp(1f+reliefSlope*.60f*distantRelief,
                        .91f,1.09f);
                    float brush=Mathf.PerlinNoise(
                        x*.126f+72f,z*.143f+186f);
                    float eroded=Mathf.PerlinNoise(
                        x*.062f+205f,z*.054f+42f);
                    // High-frequency dry grass and green brush textures
                    // cohere to the existing biome atlas at all three zooms.
                    float grazing=Mathf.Clamp01((brush-.49f)*.80f)*
                        Mathf.Clamp01((eroded-.30f)*1.6f);
                    ground=Lerp(ground,new Color(.73f,.63f,.39f),grazing*.21f);
                    // Terraced stony slopes paint restrained physical relief,
                    // while reused 3D ridges supply silhouettes at far zoom.
                    float hillValue=0f,side=0f;
                    for(int ridge=0;ridge<RidgeSites.Length;ridge++)
                    {
                        float dx=(x-RidgeSites[ridge].x)/58f;
                        float dz=(z-RidgeSites[ridge].y)/47f;
                        float dist=dx*dx+dz*dz;
                        if(dist>=1f)continue;
                        float weight=(1f-dist)*(1f-dist);
                        hillValue=Mathf.Max(hillValue,weight);
                        side=Mathf.Max(side,weight*(-dx*.65f+dz*.35f));
                    }
                    if(hillValue>.01f)
                    {
                        Color ridgeRock=Lerp(new Color(.46f,.48f,.32f),
                            new Color(.72f,.65f,.47f),hills);
                        ground=Lerp(ground,ridgeRock,hillValue*.64f);
                        ground=Lerp(ground,new Color(.29f,.32f,.24f),
                            Mathf.Clamp01(-side)*.21f);
                    }
                    float radius=Mathf.Sqrt(x*x+z*z);
                    if(radius<180f)ground=Lerp(ground,new Color(.48f,.53f,.29f),
                        (1f-Mathf.SmoothStep(45f,180f,radius))*.33f);
                    // Small natural exposed-earth bands, not fake resources.
                    float scuff=Mathf.PerlinNoise(x*.078f+223f,z*.069f+183f);
                    if(scuff>.68f)ground=Lerp(ground,BareEarth,(scuff-.68f)*.95f);
                    if(RegionOneCanonicalLandscape.IsForest(x,z))
                    {
                        float gaps=Mathf.PerlinNoise(x*.116f+13f,z*.117f+27f);
                        ground=Lerp(ground,PineFloor,.43f+.23f*gaps);
                    }
                    float road=RoadMask(x,z);
                    if(road>0f)ground=Lerp(ground,new Color(.69f,.57f,.35f),road);
                    // Subtle damp meadow corridors make river valleys read as
                    // continuous geography at 244 zoom; no new water node.
                    if(z>-445f&&z<455f)
                    {
                        float riparian=Mathf.Clamp01((21f-Mathf.Abs(x-RiverCentre(z)))/18f);
                        ground=Lerp(ground,new Color(.41f,.53f,.28f),riparian*.17f);
                    }
                    float water=WaterMask(x,z);
                    if(water>0f)
                    {
                        // Tan shoreline attenuates harsh flat blue borders.
                        if(water<.70f)ground=Lerp(ground,new Color(.75f,.65f,.42f),.46f);
                        ground=Lerp(ground,Lerp(River,Ripples,v*.31f),water);
                    }
                    float d=Mathf.Sqrt((x-breachX)*(x-breachX)+(z-breachZ)*(z-breachZ));
                    if(d<53f)
                    {
                        float strength=Mathf.SmoothStep(0f,1f,
                            Mathf.Clamp01((53f-d)/45f));
                        float fractures=Mathf.Abs(Mathf.Sin(
                            Mathf.Atan2(z-breachZ,x-breachX)*17f + d*.095f));
                        ground=Lerp(ground,RiftGround,strength*(.57f+.20f*fractures));
                        if(d<8f)ground=Lerp(ground,RiftGlow,(8f-d)/8f*.52f);
                    }
                    // Tiny blue-green herb and ochre highlights survive high
                    // zoom as terrain texture, not fake 3D buildings.
                    float fleck=Hash01(ix,iy,117);
                    if(fleck>.984f && water<.15f && road<.1f)
                        ground=Lerp(ground, v>.5f ? new Color(.30f,.40f,.24f) :
                            new Color(.81f,.69f,.47f),.25f);
                    pixels[iy*size+ix]=ground;
                }
            // Light 2.5D tree crowns only where the actual geographic
            // forest mask says forest, consistent with batched 3D LODs.
            // At zoom 244 they provide painterly foliage texture, not icons.
            int crowns=(size*size)/180;
            for(int k=0;k<crowns;k++)
            {
                int ix=(int)(Hash(k,301,312)%((uint)size));
                int iy=(int)(Hash(k,521,913)%((uint)size));
                float x=((ix+.5f)/size*2f-1f)*halfWidth;
                float z=((iy+.5f)/size*2f-1f)*halfDepth;
                if(!RegionOneCanonicalLandscape.IsForest(x,z) ||
                    WaterMask(x,z)>.05f || RoadMask(x,z)>.1f ||
                    x*x+z*z<75f*75f)continue;
                int rad=1+(int)(Hash01(k,39,731)*2.99f);
                StampTree(pixels,size,ix,iy,rad,Hash01(k,423,818));
            }
            // Scattered small rocks on distant slopes, nowhere near an
            // interactable quarry node. These are 2.5D geology not resources.
            for(int k=0;k<size*size/1450;k++)
            {
                int ix=(int)(Hash(k,931,212)%((uint)size));
                int iy=(int)(Hash(k,839,373)%((uint)size));
                float x=((ix+.5f)/size*2f-1f)*halfWidth;
                float z=((iy+.5f)/size*2f-1f)*halfDepth;
                if(x*x+z*z<120f*120f ||
                    WaterMask(x,z)>.02f || RegionOneCanonicalLandscape.IsForest(x,z) ||
                    RoadMask(x,z)>.01f)continue;
                if(Mathf.Abs(x)<290f && Mathf.Abs(z)<205f &&
                   Hash01(k,43,774)<.86f)continue;
                StampStone(pixels,size,ix,iy,1+(k%2),Hash01(k,739,833));
            }
            texture.SetPixels32(pixels);
            texture.Apply(true,true);
            texture.filterMode=FilterMode.Bilinear;
            texture.wrapMode=TextureWrapMode.Clamp;
            return texture;
        }
    }
}
