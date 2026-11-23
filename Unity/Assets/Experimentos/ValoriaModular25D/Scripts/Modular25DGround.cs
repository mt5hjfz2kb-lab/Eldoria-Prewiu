using System;
using UnityEngine;

namespace Eldoria.Experimentos.ValoriaModular25D
{
    /// <summary>
    /// Continuous repeatable ground in 8x8 world-metre mesh chunks. The original
    /// diamond carpet had visible outer edges at 390x844, repeated patterns and
    /// no natural continuation. This is a low-cost spatial base, not an image
    /// of a complete city. Structures/roads/vegetation stay independent.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class Modular25DGround : MonoBehaviour
    {
        const int Side = 8, HalfChunks = 4, TextureSize = 512;
        const float PatternMeters = 6.5f;
        Material material;
        Texture2D texture;
        bool generatedAtRuntime;
        public bool OriginalProjectMeadowLoaded { get; private set; }
        public int ChunkCount { get; private set; }
        public void Build()
        {
            if (ChunkCount != 0) return;
            // The same wrapping material spans all chunks; roads and buildings
            // keep their own geometry, contacts and sorting.
            texture=Resources.Load<Texture2D>("ValoriaModular25D/BastionMeadow");
            if(texture==null)texture=Resources.Load<Texture2D>("ValoriaModular25D/OrganicMeadowTile");
            OriginalProjectMeadowLoaded=texture!=null;
            if(texture==null)
            {
                Debug.LogError("[Modular25D] Original Eldoria grass tile missing; "+
                    "do not visually approve emergency procedural fallback.",this);
                generatedAtRuntime=true;
                texture=MakeTexture();
            }
            else
            {
                generatedAtRuntime=false;
                texture.wrapMode=TextureWrapMode.Repeat;
                texture.filterMode=FilterMode.Trilinear;
            }
            var shader=Shader.Find("Sprites/Default");
            if(shader==null)shader=Shader.Find("Universal Render Pipeline/Unlit");
            if(shader==null)
            {
                Debug.LogError("[Modular25D] No supported ground shader; visual QA is blocked.",this);
                return;
            }
            material=new Material(shader){name="Eldoria Modular Continuous Grass"};
            material.mainTexture=texture;
            if(material.HasProperty("_BaseMap"))material.SetTexture("_BaseMap",texture);
            if(material.HasProperty("_BaseColor"))material.SetColor("_BaseColor",Color.white);
            for(int gy=-HalfChunks;gy<=HalfChunks;gy++)
            for(int gx=-HalfChunks;gx<=HalfChunks;gx++)
            {
                var go=new GameObject("Ground chunk ("+gx+","+gy+")");
                go.transform.SetParent(transform,false);
                go.transform.localPosition=new Vector3(gx*Side,gy*Side,.1f);
                float ux=gx*Side/PatternMeters,uy=gy*Side/PatternMeters;
                float stride=Side/PatternMeters;
                var mesh=new Mesh{name="Ground chunk 8x8"};
                mesh.vertices=new[]{
                    Vector3.zero,new Vector3(Side,0,0),
                    new Vector3(Side,Side,0),new Vector3(0,Side,0)};
                mesh.uv=new[]{
                    new Vector2(ux,uy),new Vector2(ux+stride,uy),
                    new Vector2(ux+stride,uy+stride),new Vector2(ux,uy+stride)};
                // Camera looks along +Z, so facing -Z is the front face.
                mesh.triangles=new[]{0,2,1,0,3,2};
                mesh.RecalculateBounds();
                go.AddComponent<MeshFilter>().sharedMesh=mesh;
                var renderer=go.AddComponent<MeshRenderer>();
                renderer.sharedMaterial=material;
                renderer.sortingOrder=-6000;
                ChunkCount++;
            }
        }
        Material districtMaterial;
        Vector3Int previousDistricts;
        public void SetDistricts(bool barracks, bool granary, bool quarry)
        {
            var built = new Vector3Int(barracks ? 1 : 0, granary ? 1 : 0, quarry ? 1 : 0);
            if (districtMaterial && built == previousDistricts) return;
            previousDistricts = built;
            if (!districtMaterial)
            {
                districtMaterial = new Material(Resources.Load<Shader>("ValoriaModular25D/DistrictGround"));
                districtMaterial.mainTexture = texture;
                districtMaterial.SetTexture("_Paving", Resources.Load<Texture2D>("ValoriaModular25D/DistrictPaving"));
                districtMaterial.SetTexture("_Soil", Resources.Load<Texture2D>("ValoriaModular25D/HaulersEarth"));
                var plan=ValoriaUrbanPlan.City;
                districtMaterial.SetVector("_Forecourt",new Vector4(plan.CourtCenter.x,plan.CourtCenter.y,
                    plan.CourtRadii.x,plan.CourtRadii.y));
                districtMaterial.SetVector("_CivicAxis",new Vector4(plan.Keep.Foot.x,plan.Keep.Foot.y,plan.Junction.x,plan.Junction.y));
                districtMaterial.SetVector("_WorkLane",new Vector4(plan.Mill.Foot.x,plan.Mill.Foot.y,plan.Freight.x,plan.Freight.y));
                districtMaterial.SetTexture("_Slate",Resources.Load<Texture2D>("World4X/Illustrated/Slate"));
                SetYard("_MilitaryYard",plan.Barracks,1.30f);
                SetYard("_MarketYard",plan.Granary,1.16f);
                SetYard("_StoneFace",plan.Quarry,1.35f);
            }
            districtMaterial.SetVector("_Built", new Vector4(built.x, built.y, built.z, 0));
            foreach (var chunk in GetComponentsInChildren<MeshRenderer>())
                chunk.sharedMaterial = districtMaterial;
        }
        void SetYard(string property, ValoriaUrbanPlan.Parcel parcel, float apron)
        {
            districtMaterial.SetVector(property,new Vector4(parcel.Foot.x,parcel.Foot.y-.35f,
                parcel.GroundSize.x*.63f,parcel.GroundSize.y*.5f+apron));
        }

        static float Hash(int x,int y,int p)
        {
            x=(x%p+p)%p;y=(y%p+p)%p;
            unchecked {
                uint n=(uint)(x*374761393)+(uint)(y*668265263)+0x9e3779b9u;
                n=(n^(n>>13))*1274126177u;
                return (n^(n>>16))/4294967295f;
            }
        }
        static float Noise(float u,float v,int frequency)
        {
            float fx=u*frequency,fy=v*frequency;
            int x=Mathf.FloorToInt(fx),y=Mathf.FloorToInt(fy);
            float tx=fx-x,ty=fy-y;
            tx=tx*tx*(3-2*tx);ty=ty*ty*(3-2*ty);
            float a=Mathf.Lerp(Hash(x,y,frequency),Hash(x+1,y,frequency),tx);
            float b=Mathf.Lerp(Hash(x,y+1,frequency),Hash(x+1,y+1,frequency),tx);
            return Mathf.Lerp(a,b,ty);
        }
        static Texture2D MakePaintedSource(string encoded)
        {
            // This 128x128 JPEG is a crop of an ORIGINAL Eldoria art-direction
            // concept generated in this project, from GRASS-ONLY pixels.
            // The full city image is NEVER used as a Unity backplate.
            // It is encoded as a tiny Resources TextAsset with fixed SHA256.
            // Sample alpha=1 throughout and bake a periodic source tile ONCE.
            var binary=Convert.FromBase64String(encoded.Trim());
            if(binary.Length!=826 || binary[0]!=255 || binary[1]!=216 ||
               binary[binary.Length-2]!=255 || binary[binary.Length-1]!=217)
                throw new InvalidOperationException("Ground art source JPEG integrity failed.");
            var src=new Texture2D(2,2,TextureFormat.RGBA32,false,false);
            if(!ImageConversion.LoadImage(src,binary,false))
            {
                UnityEngine.Object.Destroy(src);
                throw new InvalidOperationException("Ground JPEG source failed to decode in Unity.");
            }
            if(src.width!=128 || src.height!=128)
            {
                UnityEngine.Object.Destroy(src);
                throw new InvalidOperationException("Ground JPEG source was resized.");
            }
            src.wrapMode=TextureWrapMode.Clamp;
            src.filterMode=FilterMode.Bilinear;
            var tile=new Texture2D(TextureSize,TextureSize,TextureFormat.RGBA32,false,false){
                name="Valoria hand-painted meadow | seamless tile from owned original art",
                wrapMode=TextureWrapMode.Repeat,
                filterMode=FilterMode.Bilinear};
            var pixels=new Color32[TextureSize*TextureSize];
            for(int y=0;y<TextureSize;y++)for(int x=0;x<TextureSize;x++)
            {
                float u=x/(float)(TextureSize-1);
                float v=y/(float)(TextureSize-1);
                float ox=Mathf.Repeat(u+.5f,1f);
                float oy=Mathf.Repeat(v+.5f,1f);
                // Image offset moves the source's edge seam into the centre.
                // Blend it with contiguous source through an elliptical cross
                // mask. Keep the tile perimeter untouched for smooth UV wrap.
                float dx=(u-.5f)/.125f,dy=(v-.5f)/.125f;
                float blend=.94f*Mathf.Max(
                    Mathf.Exp(-dx*dx),Mathf.Exp(-dy*dy));
                Color original=src.GetPixelBilinear(u,v);
                Color shifted=src.GetPixelBilinear(ox,oy);
                Color painted=Color.Lerp(shifted,original,blend);
                painted.a=1f;
                pixels[y*TextureSize+x]=(Color32)painted;
            }
            // Explicitly equalize opposite edges for a truly wrap-safe texture.
            for(int x=0;x<TextureSize;x++)
            {
                int a=x,b=(TextureSize-1)*TextureSize+x;
                Color32 p=pixels[a],q=pixels[b];
                pixels[a]=pixels[b]=(Color32)Color.Lerp((Color)p,(Color)q,.5f);
            }
            for(int y=0;y<TextureSize;y++)
            {
                int a=y*TextureSize,b=a+TextureSize-1;
                Color32 p=pixels[a],q=pixels[b];
                pixels[a]=pixels[b]=(Color32)Color.Lerp((Color)p,(Color)q,.5f);
            }
            tile.SetPixels32(pixels);tile.Apply(false,true);
            UnityEngine.Object.Destroy(src);
            return tile;
        }

        static Texture2D MakeTexture()
        {
            var source=Resources.Load<TextAsset>("ValoriaModular25D/grass_reference_b64");
            if(source!=null)
            {
                try{return MakePaintedSource(source.text);}
                catch(Exception error)
                {
                    Debug.LogError("[Modular25D] Original painterly meadow unavailable: "+error+
                        ". Procedural emergency fallback is NOT art-approved.");
                }
            }
            else Debug.LogError("[Modular25D] Missing hand-painted grass source; emergency fallback is NOT art-approved.");
            // Unified Valoria palette sampled/compared against the approved
            // *direction* (not a slice of the image): deep olive grass, yellow
            // highlights, occasional warm-earth patches and directional
            // painted grass fibres. This is one seamless reusable MATERIAL.
            const float tau=Mathf.PI*2f;
            var t=new Texture2D(TextureSize,TextureSize,TextureFormat.RGBA32,false,false){
                name="Valoria olive grass | painterly repeatable material v2",
                wrapMode=TextureWrapMode.Repeat,filterMode=FilterMode.Bilinear};
            var darkest=new Color(.263f,.361f,.181f,1);
            var moss=new Color(.344f,.449f,.215f,1);
            var open=new Color(.408f,.513f,.248f,1);
            var sun=new Color(.535f,.575f,.304f,1);
            var dry=new Color(.470f,.415f,.292f,1);
            var pixels=new Color32[TextureSize*TextureSize];
            for(int y=0;y<TextureSize;y++)
            for(int x=0;x<TextureSize;x++)
            {
                float u=x/(float)(TextureSize-1),v=y/(float)(TextureSize-1);
                // Periodic value noise. No seam across 8x8-m material chunks.
                // Macro variation uses broad COLOR FIELDS, not uniform fine
                // static like the earlier rejected procedural green mat.
                float macro=Noise(u,v,2)*.43f+
                    Noise(u,v,5)*.39f+Noise(u,v,11)*.18f;
                float hummock=Noise(u,v,9);
                float detail=Noise(u,v,37)*.65f+Noise(u,v,107)*.35f;
                float sunlit=Mathf.Clamp01((macro-.24f)*1.63f);
                Color c=Color.Lerp(darkest,moss,Mathf.Clamp01(sunlit*.72f+.15f));
                c=Color.Lerp(c,open,Mathf.Clamp01((macro-.46f)*1.7f+.16f));
                c=Color.Lerp(c,sun,Mathf.Clamp01((hummock-.60f)*1.23f));
                float wear=Noise(u,v,4)*.69f+Noise(u,v,9)*.31f;
                // Small dusty occlusions from historical rebuilding, not
                // random hard circular platforms or unnaturally brown grass.
                c=Color.Lerp(c,dry,Mathf.Clamp01((wear-.69f)*2.0f)*.43f);
                // Shared shallow grass brush direction (NW light, SE shadow).
                // Integer periods keep the repeated atlas tile seamless.
                float sweep=Mathf.Sin(tau*(u*67f+v*31f)+
                    (Noise(u,v,13)-.5f)*tau*1.2f);
                float fibre=Mathf.Max(0f,sweep*.5f+.5f);
                float amount=(detail-.5f)*.086f+(fibre-.5f)*.014f;
                c*=1f+amount;
                c.a=1;
                pixels[y*TextureSize+x]=(Color32)c;
            }
            // Individual micro-paint marks, not a copy of the concept image:
            // constrained hues, oriented 8px tufts, subtle layered strokes.
            var random=new System.Random(51269);
            for(int tuft=0;tuft<11800;tuft++)
            {
                int x=random.Next(TextureSize),y=random.Next(TextureSize);
                int len=2+random.Next(7);
                int bend=random.Next(-2,3);
                bool light=random.Next(3)!=0;
                for(int k=0;k<len;k++)
                {
                    int px=(x+bend*k/Mathf.Max(1,len)+k/3+TextureSize)%TextureSize;
                    int py=(y+k)%TextureSize;
                    int ix=py*TextureSize+px;
                    Color color=pixels[ix];
                    float f=(len-k)/(float)len;
                    color=Color.Lerp(color,light?
                        new Color(.57f,.61f,.29f,1):
                        new Color(.21f,.31f,.16f,1),.13f*f);
                    pixels[ix]=(Color32)color;
                }
            }
            // Rare multi-pixel pale flowers. These remain decorative ground
            // texture, not the reusable props (those are separate sprites).
            for(int flower=0;flower<95;flower++)
            {
                int x=random.Next(TextureSize),y=random.Next(TextureSize);
                float patch=Noise(x/(float)TextureSize,y/(float)TextureSize,8);
                if(patch<.51f)continue;
                for(int ox=-1;ox<=1;ox++)for(int oy=-1;oy<=1;oy++)
                {
                    int px=(x+ox+TextureSize)%TextureSize;
                    int py=(y+oy+TextureSize)%TextureSize;
                    int ix=py*TextureSize+px;
                    float alpha=ox==0&&oy==0?.31f:.10f;
                    pixels[ix]=(Color32)Color.Lerp((Color)pixels[ix],
                        new Color(.84f,.82f,.59f,1),alpha);
                }
            }
            for(int y=0;y<TextureSize;y++)
                pixels[y*TextureSize+TextureSize-1]=pixels[y*TextureSize];
            for(int x=0;x<TextureSize;x++)
                pixels[(TextureSize-1)*TextureSize+x]=pixels[x];
            t.SetPixels32(pixels);t.Apply(false,true);
            return t;
        }
        void OnDestroy()
        {
            for(int i=0;i<transform.childCount;i++)
            {
                var mesh=transform.GetChild(i).GetComponent<MeshFilter>();
                if(mesh!=null&&mesh.sharedMesh!=null)Destroy(mesh.sharedMesh);
            }
            if(material!=null)Destroy(material);
            if(districtMaterial)Destroy(districtMaterial);
            if(generatedAtRuntime&&texture!=null)Destroy(texture);
        }
    }
}
