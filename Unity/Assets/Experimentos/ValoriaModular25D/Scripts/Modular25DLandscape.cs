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
        public static Vector2 BarracksFoot => ValoriaUrbanPlan.City.Barracks.Foot;
        public static Vector2 GranaryFoot => ValoriaUrbanPlan.City.Granary.Foot;
        public static Vector2 StoneworksFoot => ValoriaUrbanPlan.City.Quarry.Foot;
        const int RoadW=256, RoadH=512, FootW=512, FootH=320;
        static Vector2 Sawmill => ValoriaUrbanPlan.City.Mill.Foot;
        readonly Dictionary<string,GameObject> approaches=new Dictionary<string,GameObject>();
        public void SetDistricts(bool military,bool market,bool quarry)
        {
            approaches["barracks"].SetActive(military);
            approaches["granary"].SetActive(market);
            approaches["stoneworks"].SetActive(quarry);
            approaches["freight"].SetActive(market || quarry);
        }
        readonly List<Mesh> meshes=new List<Mesh>();
        readonly List<Texture2D> textures=new List<Texture2D>();
        readonly List<Material> materials=new List<Material>();
        bool built;
        public int RoadCount { get; private set; }
        public bool OriginalCobbleRoadLoaded { get; private set; }
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
            var earth=Resources.Load<Texture2D>("ValoriaModular25D/HaulersEarth");
            if(earth==null)earth=MakeRoadTexture();
            var earthMat=new Material(shader){name="Valoria haulers' earth road — original paint system"};
            earthMat.mainTexture=earth;
            materials.Add(earthMat);
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
            var roadShader=Resources.Load<Shader>("ValoriaModular25D/UrbanRoad");
            var paving=Resources.Load<Texture2D>("ValoriaModular25D/DistrictPaving");
            if(!roadShader || !paving)throw new InvalidOperationException("Missing authored road surface");
            earthMat.shader=roadShader;
            earthMat.mainTexture=earth;
            earthMat.SetTexture("_Paving",paving);
            if(OriginalCobbleRoadLoaded)
            {
                imperialMat.shader=roadShader;
                imperialMat.mainTexture=artistPath;
                imperialMat.SetTexture("_Paving",paving);
                imperialMat.SetFloat("_Stone",1f);
            }
            var plan = ValoriaUrbanPlan.City;
            foreach (var plot in new[]{plan.Keep,plan.Mill,plan.Barracks,plan.Granary,plan.Quarry})
            {
                bool paved = plot == plan.Keep || plot == plan.Barracks;
                var road=AddRoad(plot.Id+" | acceso",plan.Approach(plot),paved?1.32f:.92f,
                    paved?imperialMat:earthMat,paved);
                if (plot==plan.Barracks || plot==plan.Granary || plot==plan.Quarry)
                {
                    approaches.Add(plot.Id,road);
                    road.SetActive(false);
                }
            }
            var freight=AddRoad("Camino de abastos",plan.FreightRoad,1.15f,imperialMat,true);
            approaches.Add("freight",freight);
            freight.SetActive(false);

            var mill=MakeFootprint("Sawmill",new Color(.54f,.44f,.31f,1),
                new Color(.73f,.59f,.38f,1),false,true);
            PutFoot("Sawmill — earth, sawdust, trampled grass",mill,
                Sawmill+new Vector2(0,-.18f),1.00f);

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

        public GameObject AddRoad(string name,Vector2[] control,float width,Material material,bool cobbled)
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
            float totalLength=0;
            for(int i=1;i<spine.Length;i++)totalLength+=Vector2.Distance(spine[i],spine[i-1]);
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
                    // Keep a stable physical width at the open-meadow end.
                    // Narrowing the spline itself to a point produced the
                    // conspicuous triangular tongue below the barracks even
                    // after its redundant access branch was removed. Fade the
                    // paint longitudinally instead; route points stay exact.
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
                    // The ground at the crossing carries the wear between branches.
                    // Feather the open cut across the first half metre of each ribbon.
                    alpha*=Mathf.Lerp(.04f,1f,Sm(0,.63f,along));
                    alpha*=Mathf.Lerp(.16f,1f,Sm(0,.23f,totalLength-along));
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
            return obj;
        }

        Texture2D MakeFootprint(string label,Color soil,Color lit,bool cobbled,bool workshopDust)
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
                // Even an organic radial mask can retain a few alpha values
                // exactly on the texture frame. Bilinear sampling then reveals
                // the quad as a horizontal platform at retina resolution.
                float frame=Mathf.Min(1f-Mathf.Abs(xx),1f-Mathf.Abs(yy));
                fade*=Sm(0f,.13f,frame);
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
                if(workshopDust)
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

        void OnDestroy()
        {
            foreach(var mesh in meshes)if(mesh!=null)Destroy(mesh);
            foreach(var tex in textures)if(tex!=null)Destroy(tex);
            foreach(var mat in materials)if(mat!=null)Destroy(mat);
        }
    }
}

