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
        const float PatternMeters = 21f;
        Material material;
        Texture2D texture;
        public int ChunkCount { get; private set; }
        public void Build()
        {
            if (ChunkCount != 0) return;
            texture=MakeTexture();
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
        static Texture2D MakeTexture()
        {
            var t=new Texture2D(TextureSize,TextureSize,TextureFormat.RGBA32,false,false){
                name="Eldoria 512px repeatable procedural grass",wrapMode=TextureWrapMode.Repeat,
                filterMode=FilterMode.Bilinear};
            Color a=new Color(.29f,.40f,.30f,1),b=new Color(.37f,.51f,.37f,1),
                c=new Color(.44f,.56f,.37f,1),dry=new Color(.54f,.50f,.35f,1);
            var pixels=new Color32[TextureSize*TextureSize];
            for(int y=0;y<TextureSize;y++)for(int x=0;x<TextureSize;x++)
            {
                float u=x/(float)(TextureSize-1),v=y/(float)(TextureSize-1);
                float macro=Noise(u,v,3)*.56f+Noise(u,v,7)*.31f+Noise(u,v,16)*.13f;
                float fine=Noise(u,v,48)*.70f+Noise(u,v,96)*.30f;
                Color mix=Color.Lerp(a,b,Mathf.Clamp01(macro*.92f+.12f));
                mix=Color.Lerp(mix,c,Mathf.Clamp01((macro-.55f)*1.4f));
                mix=Color.Lerp(mix,dry,Mathf.Clamp01((Noise(u,v,5)-.66f)*.8f));
                mix*=.94f+.12f*fine;mix.a=1f;
                pixels[y*TextureSize+x]=mix;
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
            if(texture!=null)Destroy(texture);
        }
    }
}
