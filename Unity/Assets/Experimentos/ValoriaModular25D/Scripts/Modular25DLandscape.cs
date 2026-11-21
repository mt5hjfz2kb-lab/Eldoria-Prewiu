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
        public static readonly Vector2 BarracksFoot = new Vector2(-4.85f,-4.95f);
        public static readonly Vector2 GranaryFoot = new Vector2(3.85f,-7.65f);
        public static readonly Vector2 StoneworksFoot = new Vector2(-2.85f,-11.70f);
        const int RoadW=256, RoadH=512, FootW=512, FootH=320;
        static readonly Vector2 Keep=new Vector2(-.40f,1.45f);
        static readonly Vector2 Sawmill=new Vector2(.96f,-2.07f);
        readonly List<Mesh> meshes=new List<Mesh>();
        readonly List<Texture2D> textures=new List<Texture2D>();
        readonly List<Material> materials=new List<Material>();
        bool built;
        Material expansionRoadMaterial;
        public int RoadCount { get; private set; }
        public bool OriginalCobbleRoadLoaded { get; private set; }
        public int FootprintCount { get; private set; }
        public int CivicCourtyardsBuilt { get; private set; }

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
            var earth=Resources.Load<Texture2D>("ValoriaModular25D/HaulersEarth");
            if(earth==null)earth=MakeRoadTexture();
            var earthMat=new Material(shader){name="Valoria haulers' earth road — original paint system"};
            earthMat.mainTexture=earth;
            materials.Add(earthMat);
            expansionRoadMaterial=earthMat;
            // The larger Imperial Way gets a REAL illustrated paving material
            // sampled from the user-owned Eldoria terrain, cropped to path
            // pixels only, with soft edges. It remains mapped on the same
            // world-space spline mesh, not a giant image of a finished city.
            var artistPath=Resources.Load<Texture2D>("ValoriaModular25D/OrganicCobbleRoad");
            OriginalCobbleRoadLoaded=artistPath!=null;
            Material imperialMat=earthMat;
            if(OriginalCobbleRoadLoaded)
            {
                imperialMat=new Material(shader){name="Valoria original illustrated cobblestone road"};
                imperialMat.mainTexture=artistPath;
                // The original stones stay unchanged. A restrained warm tint
                // reduces the bluish floating-sticker effect against olive
                // meadow without rebuilding a complete city illustration.
                imperialMat.color=new Color(.93f,.85f,.74f,.93f);
                materials.Add(imperialMat);
            }
            else
                Debug.LogError("[Modular25D] No source-authored road tile; "+
                    "fallback is not artistic acceptance.",this);
            // The road is described as spatial segments; bend is interpolated,
            // not simulated by shifting a static full-scene background.
            var main=new []{
                new Vector2(-6.65f,-7.5f),new Vector2(-4.85f,-5.3f),
                new Vector2(-3.88f,-3.76f),new Vector2(-2.70f,-2.5f),
                new Vector2(-2.53f,-1.56f),new Vector2(-2.28f,-.40f),
                new Vector2(-1.23f,.10f),new Vector2(-.40f,1.00f)};
            var lumber=new[]{
                new Vector2(-2.70f,-2.5f),new Vector2(-1.57f,-3.16f),
                new Vector2(-.21f,-3.50f),new Vector2(.52f,-3.26f),
                new Vector2(.96f,-2.42f)};
            // Keep both branches legible but reduce their over-wide grey ribbons.
            // Their spline geometry, world-space UVs and hit-tested entrances stay.
            AddRoad("Imperial Way — original illustrated paving",main,1.47f,imperialMat,true);
            AddRoad("Haulers' path — compacted earth and gravel",lumber,1.26f,earthMat,false);

            var keep=MakeFootprint("Bastion",new Color(.49f,.41f,.31f,1),
                new Color(.74f,.69f,.52f,1),true);
            var mill=MakeFootprint("Sawmill",new Color(.54f,.44f,.31f,1),
                new Color(.73f,.59f,.38f,1),false);
            PutFoot("Bastion — no straight borders, stone approach",keep,
                Keep+new Vector2(0,-.23f),1.19f);
            PutFoot("Sawmill — earth, sawdust, trampled grass",mill,
                Sawmill+new Vector2(0,-.18f),1.00f);
            // One subdued shared civic clearing binds the well, gate and
            // timber road while leaving the future plots as natural meadow.
            // Different alpha / sorting from the two functional footprints.
            var yard=MakeCommonsSoil();
            PutCommons("Worked stone and soil | civic commons",yard,
                new Vector2(-.38f,-1.40f),.78f);
            EdgeProps();
        }

        public void ApplyExpansionGround(bool barracks,bool granary,bool stoneworks)
        {
            ExpansionGround("barracks",barracks,BarracksFoot,
                new[]{new Vector2(-3.88f,-3.76f),new Vector2(-4.2f,-4.65f),BarracksFoot+Vector2.down*.2f});
            ExpansionGround("granary",granary,GranaryFoot,
                new[]{new Vector2(-3.88f,-3.76f),new Vector2(-2.25f,-5.65f),new Vector2(.15f,-6.55f),new Vector2(1.95f,-7.9f),GranaryFoot+Vector2.down*.2f});
            ExpansionGround("stoneworks",stoneworks,StoneworksFoot,
                new[]{new Vector2(-4.85f,-5.3f),new Vector2(-4.00f,-7.65f),new Vector2(-4.40f,-9.6f),new Vector2(-4.35f,-11.65f),StoneworksFoot+Vector2.down*.2f});
        }

        void ExpansionGround(string id,bool visible,Vector2 foot,Vector2[] path)
        {
            var root=transform.Find("Unlocked ground | "+id);
            if(root==null && visible && expansionRoadMaterial!=null)
            {
                root=new GameObject("Unlocked ground | "+id).transform;
                root.SetParent(transform,false);
                int first=transform.childCount;
                AddRoad("Access | "+id,path,.80f,expansionRoadMaterial,false);
                PutFoot("Contact | "+id,MakeFootprint(id,new Color(.49f,.41f,.31f,1),
                    new Color(.70f,.64f,.49f,1),false),foot+new Vector2(0,-.18f),.83f);
                while(transform.childCount>first)transform.GetChild(first).SetParent(root,true);
            }
            if(root!=null)root.gameObject.SetActive(visible);
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
            // GROUND-TRUTHED ARTIST CHANGE: small chipped individual slabs
            // along a walkable earth route. This is precisely what the earlier
            // flat LineRenderer strokes could never express. All stones live
            // inside this modular road material; no city-sized illustration
            // or separate camera-facing billboard is involved.
            var cobbles=new System.Random(53377);
            for(int n=0;n<390;n++)
            {
                int lane=n%5;
                int cx=lane==0?RoadW/2+cobbles.Next(-43,44)
                    :lane%2==0?RoadW/2+cobbles.Next(-68,-46)
                             :RoadW/2+cobbles.Next(46,69);
                int cy=cobbles.Next(RoadH);
                int rx=cobbles.Next(7,16),ry=cobbles.Next(5,14);
                var shade=(float)cobbles.NextDouble();
                Color baseStone=Color.Lerp(
                    new Color(.48f,.44f,.36f,1),
                    new Color(.71f,.68f,.57f,1),shade);
                for(int dy=-ry;dy<=ry;dy++)for(int dx=-rx;dx<=rx;dx++)
                {
                    int x=cx+dx;
                    if(x<0||x>=RoadW)continue;
                    int y=(cy+dy+RoadH)%RoadH;
                    float normalized=(dx*dx)/(float)(rx*rx)+
                        (dy*dy)/(float)(ry*ry);
                    float fractured=(N(x*.18f,y*.21f,889)-.5f)*.23f;
                    if(normalized>1f+fractured)continue;
                    float edgeShade=Sm(.68f,1.12f,normalized);
                    float direction=.11f*(-dx/(float)rx+dy/(float)ry);
                    var color=Color.Lerp(baseStone,
                        new Color(.28f,.30f,.28f,1),edgeShade*.47f);
                    color=Color.Lerp(color,new Color(.84f,.78f,.63f,1),
                        Mathf.Clamp01(direction)*.33f);
                    int index=y*RoadW+x;
                    Color ground=pixels[index];
                    float opacity=lane==0?.58f:.76f;
                    var mixed=Color.Lerp(ground,color,opacity);
                    mixed.a=ground.a;
                    pixels[index]=(Color32)mixed;
                }
            }
            // End rows are identical so repetition along kilometres does not
            // introduce a transverse seam.
            for(int x=0;x<RoadW;x++)
                pixels[(RoadH-1)*RoadW+x]=pixels[x];
            tex.SetPixels32(pixels);tex.Apply(false,true);
            textures.Add(tex);
            return tex;
        }

        void AddRoad(string name,Vector2[] control,float width,Material material,bool cobbled)
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
            var edgeColor=new Color32[vtx.Length];
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
                    uv[index]=new Vector2(u,along/(cobbled ? 5.5f : width*2f));
                    // An original painted tile alone produced a continuous
                    // light-grey strip, almost like a river in 390x844. Blend
                    // actual path stones with neighbouring olive grass at
                    // the banks, without altering the artist's PNG or the
                    // spline geometry. Vertex alpha is read by Sprites/Default.
                    float bank=Mathf.Abs(u-.5f)*2f;
                    float feather=1f-Sm(.37f,.98f,bank);
                    float erosion=N(p.x*1.7f+u*4.3f,p.y*1.5f+u*6.1f,721);
                    float patches=N(p.x*4.1f,p.y*3.2f+u*5.1f,203);
                    float ink=(cobbled ? .71f : .57f)+.19f*erosion+.09f*patches;
                    float alpha=Mathf.Clamp01(feather*ink);
                    edgeColor[index]=(Color32)new Color(1f,1f,1f,alpha);
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
            var mesh=new Mesh{name=name,vertices=vtx,uv=uv,colors32=edgeColor,triangles=tris};
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
                float irregular=(broad-.5f)*.31f+(fine-.5f)*.07f;
                float fade=1f-Sm(.42f+irregular,1.02f+irregular,r);
                float dirt=N(xx*14f,yy*12f,8),grit=N(xx*59f,yy*58f,17);
                Color c=Color.Lerp(soil,lit,Mathf.Clamp01(.20f+.54f*dirt));
                // Stone paving around the keep gate is part of its own
                // alpha footprint kit, not a rectangular background board.
                // Organic entrance grading: avoid the rectangular hard
                // cobble inset previously visible as a separate platform.
                float gateOffset=xx+.15f*yy;
                float gateWidth=Mathf.Abs(gateOffset)*(1f+.10f*(broad-.5f));
                float stoneZone=cobbled?
                    (1f-Sm(-.63f,.76f,yy))*(1f-Sm(.18f,.83f,gateWidth)):
                    0f;
                if(stoneZone>0)
                {
                    float tilex=xx*5.8f+Mathf.Floor(yy*6.6f)*.5f;
                    float tiley=yy*6.6f;
                    float edgeX=Mathf.Abs(tilex-Mathf.Round(tilex));
                    float edgeY=Mathf.Abs(tiley-Mathf.Round(tiley));
                    float seam=1f-Sm(.035f,.115f,Mathf.Min(edgeX,edgeY));
                    Color paving=Color.Lerp(new Color(.35f,.35f,.29f,1),
                        new Color(.65f,.60f,.49f,1),dirt*.78f+fine*.22f);
                    paving=Color.Lerp(paving,shade,seam*.33f);
                    c=Color.Lerp(c,paving,stoneZone*.90f);
                }
                // Sawdust subtly warms the middle of the mill footprint.
                if(!cobbled)
                {
                    float sawdust=(1f-Sm(.10f,.69f,r))*(.28f+.2f*grit);
                    c=Color.Lerp(c,new Color(.78f,.65f,.40f,1),sawdust);
                }
                // Blend into actual painted grass and the incoming road rather
                // than drawing a solid ochre oval around both buildings.
                float alpha=fade*(cobbled ? .39f+.29f*fine : .29f+.23f*fine);
                c.a=Mathf.Clamp01(alpha);
                pixels[y*FootW+x]=(Color32)c;
            }
            tex.SetPixels32(pixels);tex.Apply(false,true);
            textures.Add(tex);
            return tex;
        }

        Texture2D MakeCommonsSoil()
        {
            // Low-ink shared passage, not a third rectangular building pad.
            // Distinct warm stone dust at the well and compressed olive soil
            // along the gate-to-sawmill connection. Fully feathered outer alpha.
            var tex=new Texture2D(FootW,FootH,TextureFormat.RGBA32,false,false){
                name="Reconstruction commons | warm compacted earth and chipped paving",
                filterMode=FilterMode.Bilinear,wrapMode=TextureWrapMode.Clamp};
            var colors=new Color32[FootW*FootH];
            var soil=new Color(.53f,.44f,.31f,1);
            var stone=new Color(.63f,.57f,.45f,1);
            var olive=new Color(.40f,.48f,.29f,1);
            for(int y=0;y<FootH;y++)for(int x=0;x<FootW;x++)
            {
                float px=(x/(float)(FootW-1)-.5f)*2f;
                float py=(y/(float)(FootH-1)-.5f)*2f;
                float broad=N(px*4.2f+4f,py*5.0f-3f,298);
                float speck=N(px*38f,py*36f,901);
                float radius=Mathf.Sqrt((px+.12f)*(px+.12f)*.94f+py*py*1.2f);
                float rough=(broad-.5f)*.30f+(speck-.5f)*.09f;
                float fade=1f-Sm(.31f+rough,.93f+rough,radius);
                // Soft small remnants of broken stone near the well, not a
                // regular cobble carpet or a light-grey oval around the keep.
                float mineral=N(px*15.2f,py*16.4f,819);
                float chips=Mathf.Clamp01((mineral-.58f)*3.2f)*
                    (1f-Sm(.13f,.82f,Mathf.Abs(py+.10f)));
                Color c=Color.Lerp(soil,olive,Mathf.Clamp01(.23f+.40f*broad));
                c=Color.Lerp(c,stone,chips*.39f);
                c.a=fade*(.14f+.20f*speck);
                colors[y*FootW+x]=(Color32)c;
            }
            tex.SetPixels32(colors);tex.Apply(false,true);
            textures.Add(tex);
            return tex;
        }

        void PutCommons(string name,Texture2D texture,Vector2 at,float scale)
        {
            var sprite=Sprite.Create(texture,new Rect(0,0,texture.width,texture.height),
                new Vector2(.5f,.5f),88f,0,SpriteMeshType.FullRect);
            var go=new GameObject(name);
            go.transform.SetParent(transform,false);
            go.transform.localPosition=new Vector3(at.x,at.y,-.08f);
            go.transform.localScale=Vector3.one*scale;
            var renderer=go.AddComponent<SpriteRenderer>();
            renderer.sprite=sprite;
            renderer.sortingOrder=-4130; // under road mesh and both building footprints
            CivicCourtyardsBuilt++;
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
            sr.sortingOrder=-4050;
            FootprintCount++;
        }

        void EdgeProps()
        {
            var rnd=new System.Random(88215);
            for(int i=0;i<34;i++)
            {
                bool nearKeep=i<18;
                Vector2 centre=nearKeep?Keep:Sawmill;
                float theta=(i*.61803398875f+rnd.Next(100)*.003f)*Mathf.PI*2f;
                float rx=nearKeep?2.8f:2.2f;
                float ry=nearKeep?1.52f:1.3f;
                float margin=.80f+(float)rnd.NextDouble()*.40f;
                var at=centre+new Vector2(Mathf.Cos(theta)*rx*margin,
                    Mathf.Sin(theta)*ry*margin-.18f);
                // Leave the road and the actual entrance walkable and clean.
                if(Mathf.Abs(at.x+2.42f)<.64f&&at.y<1.1f)continue;
                if(!nearKeep&&at.x>.56f&&at.y<-.8f&&at.y>-3.9f)continue;
                // Ground strokes outnumber stones. Keep big rocks outside the gate.
                // Tufts break the alpha edge of the footprint without forming
                // an unnatural bracelet of boulders around the castle.
                // The earlier geometric boulders and flat shrub discs cut
                // against the high-detail recovered original prop family.
                Sprite art=Modular25DArt.Tuft(i%2);
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

