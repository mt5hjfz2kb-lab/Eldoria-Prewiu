using System;
using System.Collections.Generic;
using UnityEngine;

namespace Eldoria.Experimentos.ValoriaModular25D
{
    /// <summary>
    /// Ground language shared by all buildings: organic textured road ribbons,
    /// soil footprint decals and small edge vegetation. This authoring system
    /// creates independent geometry and textures, NOT one composited city image.
    /// The fixed seed/locations permit exact reproductions in later bastion tiers.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class Modular25DLandscape : MonoBehaviour
    {
        const int RoadW=256, RoadH=512, FootW=512, FootH=320;
        static readonly Vector2 Keep=new Vector2(-1.15f,1.3f);
        static readonly Vector2 Sawmill=new Vector2(1.95f,-2.2f);
        readonly List<Mesh> meshes=new List<Mesh>();
        readonly List<Texture2D> textures=new List<Texture2D>();
        readonly List<Material> materials=new List<Material>();
        bool built;
        public int RoadCount { get; private set; }
        public int FootprintCount { get; private set; }

        public void Build()
        {
            if(built)return;
            built=true;
            var shader=Shader.Find("Sprites/Default");
            if(shader==null)
            {
                Debug.LogError("[Modular25D] Sprites/Default unavailable; landscape cannot integrate.",this);
                return;
            }
            var road=MakeRoadTexture();
            var roadMat=new Material(shader){name="Valoria earth road — feathered, painted"};
            roadMat.mainTexture=road;
            materials.Add(roadMat);
            // The road is described as spatial segments; bend is interpolated,
            // not simulated by shifting a static full-scene background.
            var main=new []{
                new Vector2(-6.65f,-7.5f),new Vector2(-4.85f,-5.3f),
                new Vector2(-3.88f,-3.76f),new Vector2(-2.70f,-2.5f),
                new Vector2(-2.63f,-1.56f),new Vector2(-2.62f,-.55f),
                new Vector2(-2.3f,.1f),new Vector2(-1.15f,.85f)};
            var lumber=new[]{
                new Vector2(-2.70f,-2.5f),new Vector2(-1.57f,-3.16f),
                new Vector2(-.21f,-3.50f),new Vector2(1.04f,-3.41f),
                new Vector2(1.95f,-2.55f)};
            AddRoad("Imperial path — dirt, ruts, grass edges",main,1.78f,roadMat);
            AddRoad("Haulers' path — compacted earth",lumber,1.47f,roadMat);

            var keep=MakeFootprint("Bastion",new Color(.49f,.41f,.31f,1),
                new Color(.74f,.69f,.52f,1),true);
            var mill=MakeFootprint("Sawmill",new Color(.54f,.44f,.31f,1),
                new Color(.73f,.59f,.38f,1),false);
            PutFoot("Bastion — no straight borders, stone approach",keep,
                Keep+new Vector2(0,-.23f),1.19f);
            PutFoot("Sawmill — earth, sawdust, trampled grass",mill,
                Sawmill+new Vector2(0,-.18f),1.00f);
            EdgeProps();
        }

        static Color32 Mix(Color a,Color b,float t)
        {
            return (Color32)Color.Lerp(a,b,Mathf.Clamp01(t));
        }
        static float Sm(float a,float b,float x)
        {
            x=Mathf.Clamp01((x-a)/(b-a));
            return x*x*(3f-2f*x);
        }
        static float Hash(int x,int y,int seed)
        {
            unchecked{
                uint n=(uint)(x*374761393+y*668265263+seed*2147483647);
                n=(n^(n>>13))*1274126177u;
                return (n^(n>>16))/4294967295f;
            }
        }
        static float N(float x,float y,int seed)
        {
            int ix=Mathf.FloorToInt(x),iy=Mathf.FloorToInt(y);
            float tx=x-ix,ty=y-iy;
            tx=tx*tx*(3f-2f*tx);ty=ty*ty*(3f-2f*ty);
            return Mathf.Lerp(
              Mathf.Lerp(Hash(ix,iy,seed),Hash(ix+1,iy,seed),tx),
              Mathf.Lerp(Hash(ix,iy+1,seed),Hash(ix+1,iy+1,seed),tx),ty);
        }

        Texture2D MakeRoadTexture()
        {
            var tex=new Texture2D(RoadW,RoadH,TextureFormat.RGBA32,false,false){
                name="Earth path master alpha — modular directional ribbon",
                filterMode=FilterMode.Bilinear,wrapMode=TextureWrapMode.Repeat};
            var pixels=new Color32[RoadW*RoadH];
            var ochre=new Color(.58f,.47f,.33f,1);
            var light=new Color(.72f,.60f,.40f,1);
            var dark=new Color(.39f,.35f,.27f,1);
            for(int y=0;y<RoadH;y++)for(int x=0;x<RoadW;x++)
            {
                float u=x/(float)(RoadW-1),v=y/(float)(RoadH-1);
                float cross=Mathf.Abs(u-.5f)*2f;
                float small=N(u*31f,v*51f,91);
                float erosion=N(u*9f,v*18f,33);
                float irregular=(erosion-.5f)*.12f+(small-.5f)*.025f;
                float edge=1f-Sm(.64f+irregular,.96f+irregular,cross);
                float track=1f-Mathf.Abs(Mathf.Sin((u-.5f)*Mathf.PI*3.7f));
                float center=.50f*N(u*17f,v*37f,31)+.50f*N(u*57f,v*74f,67);
                float grit=N(u*104f,v*89f,58);
                float pebble=grit>.87f&&cross>.37f?.27f:0f;
                var c=Color.Lerp(ochre,light,Mathf.Clamp01(.1f+.52f*center+.18f*erosion+pebble));
                c=Color.Lerp(c,dark,Mathf.Clamp01((1f-track)*.24f+(.5f-center)*.10f));
                c*=.91f+.18f*small;
                c.a=edge*(.72f+.25f*erosion);
                pixels[y*RoadW+x]=(Color32)c;
            }
            // End rows are identical so repetition along kilometres does not
            // introduce a transverse seam.
            for(int x=0;x<RoadW;x++)
                pixels[(RoadH-1)*RoadW+x]=pixels[x];
            tex.SetPixels32(pixels);tex.Apply(false,true);
            textures.Add(tex);
            return tex;
        }

        void AddRoad(string name,Vector2[] control,float width,Material material)
        {
            const int Steps=8,Across=9;
            int samples=(control.Length-1)*Steps+1;
            var spine=new Vector2[samples];
            for(int i=0;i<samples;i++)
            {
                float t=i/(float)Steps;
                int k=Mathf.Min(control.Length-2,Mathf.FloorToInt(t));
                float f=t-k;
                Vector2 a=control[Mathf.Max(0,k-1)],b=control[k],
                    c=control[k+1],d=control[Mathf.Min(control.Length-1,k+2)];
                // Centripetal-style clamped Hermite: prevents sharp joins
                // and retains the original walkable waypoints.
                spine[i]=.5f*((2f*b)+(-a+c)*f+
                    (2f*a-5f*b+4f*c-d)*f*f+(-a+3f*b-3f*c+d)*f*f*f);
            }
            var vtx=new Vector3[samples*Across];
            var uv=new Vector2[vtx.Length];
            var tris=new int[(samples-1)*(Across-1)*6];
            float along=0;
            for(int i=0;i<samples;i++)
            {
                Vector2 tangent=(spine[Mathf.Min(samples-1,i+1)]-
                    spine[Mathf.Max(0,i-1)]).normalized;
                if(tangent.sqrMagnitude<.01f)tangent=Vector2.right;
                Vector2 normal=new Vector2(-tangent.y,tangent.x);
                if(i>0)along+=Vector2.Distance(spine[i],spine[i-1]);
                float wav=1f+.07f*Mathf.Sin(i*.73f)+.035f*Mathf.Sin(i*1.61f);
                for(int j=0;j<Across;j++)
                {
                    float u=j/(float)(Across-1);
                    float offset=(u-.5f)*width*wav;
                    int index=i*Across+j;
                    Vector2 p=spine[i]+normal*offset;
                    vtx[index]=new Vector3(p.x,p.y,-.075f);
                    uv[index]=new Vector2(u,along/5.5f);
                }
            }
            int idx=0;
            for(int i=0;i<samples-1;i++)
            for(int j=0;j<Across-1;j++)
            {
                int a=i*Across+j,b=a+Across;
                tris[idx++]=a;tris[idx++]=b;tris[idx++]=a+1;
                tris[idx++]=a+1;tris[idx++]=b;tris[idx++]=b+1;
            }
            var mesh=new Mesh{name=name,vertices=vtx,uv=uv,triangles=tris};
            mesh.RecalculateBounds();
            meshes.Add(mesh);
            var obj=new GameObject(name);
            obj.transform.SetParent(transform,false);
            obj.AddComponent<MeshFilter>().sharedMesh=mesh;
            var renderer=obj.AddComponent<MeshRenderer>();
            renderer.sharedMaterial=material;
            renderer.sortingOrder=-4100;
            RoadCount++;
        }

        Texture2D MakeFootprint(string label,Color soil,Color lit,bool cobbled)
        {
            var tex=new Texture2D(FootW,FootH,TextureFormat.RGBA32,false,false){
                name=label+" authored foot-plane earth/grass/stone transition",
                filterMode=FilterMode.Bilinear,wrapMode=TextureWrapMode.Clamp};
            var pixels=new Color32[FootW*FootH];
            Color shade=new Color(.28f,.31f,.25f,1);
            for(int y=0;y<FootH;y++)for(int x=0;x<FootW;x++)
            {
                float xx=(x/(float)(FootW-1)-.5f)*2f;
                float yy=(y/(float)(FootH-1)-.5f)*2f;
                float r=Mathf.Sqrt(xx*xx+yy*yy);
                float broad=N(xx*5.2f+10f,yy*6.2f+10f,42);
                float fine=N(xx*33f,yy*28f,79);
                float irregular=(broad-.5f)*.20f+(fine-.5f)*.06f;
                float fade=1f-Sm(.42f+irregular,1.02f+irregular,r);
                float dirt=N(xx*14f,yy*12f,8),grit=N(xx*59f,yy*58f,17);
                Color c=Color.Lerp(soil,lit,Mathf.Clamp01(.20f+.54f*dirt));
                // Stone paving around the keep gate is part of its own
                // alpha footprint kit, not a rectangular background board.
                float stoneZone=cobbled?(1f-Sm(-.15f,.52f,yy))*(1f-Sm(.47f,.77f,Mathf.Abs(xx))):0f;
                if(stoneZone>0)
                {
                    float tilex=xx*8.5f+Mathf.Floor(yy*10f)*.5f;
                    float tiley=yy*10f;
                    float edgeX=Mathf.Abs(tilex-Mathf.Round(tilex));
                    float edgeY=Mathf.Abs(tiley-Mathf.Round(tiley));
                    float seam=1f-Sm(.36f,.47f,Mathf.Min(edgeX,edgeY));
                    Color paving=Color.Lerp(new Color(.35f,.35f,.29f,1),
                        new Color(.65f,.60f,.49f,1),dirt*.78f+fine*.22f);
                    paving=Color.Lerp(paving,shade,seam*.33f);
                    c=Color.Lerp(c,paving,stoneZone*.77f);
                }
                // Sawdust subtly warms the middle of the mill footprint.
                if(!cobbled)
                {
                    float sawdust=(1f-Sm(.10f,.69f,r))*(.28f+.2f*grit);
                    c=Color.Lerp(c,new Color(.78f,.65f,.40f,1),sawdust);
                }
                float alpha=fade*(.51f+.31f*fine);
                c.a=Mathf.Clamp01(alpha);
                pixels[y*FootW+x]=(Color32)c;
            }
            tex.SetPixels32(pixels);tex.Apply(false,true);
            textures.Add(tex);
            return tex;
        }

        void PutFoot(string name,Texture2D texture,Vector2 at,float scale)
        {
            var sprite=Sprite.Create(texture,new Rect(0,0,texture.width,texture.height),
                new Vector2(.5f,.5f),88f,0,SpriteMeshType.FullRect);
            var go=new GameObject(name);
            go.transform.SetParent(transform,false);
            go.transform.localPosition=new Vector3(at.x,at.y,-.07f);
            go.transform.localScale=Vector3.one*scale;
            var sr=go.AddComponent<SpriteRenderer>();
            sr.sprite=sprite;
            sr.sortingOrder=-4300;
            FootprintCount++;
        }

        void EdgeProps()
        {
            var rnd=new System.Random(88215);
            for(int i=0;i<48;i++)
            {
                bool nearKeep=i<25;
                Vector2 centre=nearKeep?Keep:Sawmill;
                float theta=(i*.61803398875f+rnd.Next(100)*.003f)*Mathf.PI*2f;
                float rx=nearKeep?2.8f:2.2f;
                float ry=nearKeep?1.52f:1.3f;
                float margin=.80f+(float)rnd.NextDouble()*.40f;
                var at=centre+new Vector2(Mathf.Cos(theta)*rx*margin,
                    Mathf.Sin(theta)*ry*margin-.18f);
                // Leave the road and the actual entrance walkable and clean.
                if(Mathf.Abs(at.x+2.62f)<.64f&&at.y<1.1f)continue;
                if(!nearKeep&&at.x>1.12f&&at.y<-.8f&&at.y>-3.9f)continue;
                // Ground strokes outnumber stones. Keep big rocks outside the gate.
                // Tufts break the alpha edge of the footprint without forming
                // an unnatural bracelet of boulders around the castle.
                Sprite art=(i%13==0)?Modular25DArt.Rock():
                    (i%4==0)?Modular25DArt.Bush(i%3):
                              Modular25DArt.Tuft(i%2);
                var go=new GameObject("Ground contact — local grass/stone "+i);
                go.transform.SetParent(transform,false);
                go.transform.localPosition=new Vector3(at.x,at.y,-.02f);
                float sz=.27f+(float)rnd.NextDouble()*.24f;
                go.transform.localScale=Vector3.one*sz;
                var sr=go.AddComponent<SpriteRenderer>();
                sr.sprite=art;
                sr.sortingOrder=Modular25DWorld.FootOrder(at.y);
                sr.color=Color.Lerp(Color.white,new Color(.75f,.82f,.65f,1),
                    (float)rnd.NextDouble()*.22f);
            }
        }

        void OnDestroy()
        {
            foreach(var mesh in meshes)if(mesh!=null)Destroy(mesh);
            foreach(var tex in textures)if(tex!=null)Destroy(tex);
            foreach(var mat in materials)if(mat!=null)Destroy(mat);
        }
    }
}
