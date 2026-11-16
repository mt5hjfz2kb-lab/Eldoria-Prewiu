// Ashen Citadel · deterministic city-only premium environment (Unity 6 Editor).
// Gameplay, the World view, and production game art remain untouched.
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace Eldoria.ExperimentalAshenCitadel.Editor
{
    internal static class AshenPremiumTerrainBuilder
    {
        private const string MeshAsset="Assets/Eldoria/ExperimentalAshenCitadel/Meshes/AshenPremiumTerrain.asset";
        // Meadow, earth, limestone, dressed stone, basalt, timber, and two foliage tones.
        private const int Meadow=0, Earth=1, Limestone=2, Cutstone=3, Basalt=4,
            Timber=5, Foliage=6, FoliageLight=7;
        private static readonly Vector2[] Workshops={new Vector2(-17.2f,-6.2f),
            new Vector2(16.8f,-7.5f),new Vector2(-16.6f,7.8f),new Vector2(17.4f,8.8f)};

        private sealed class Geometry
        {
            internal readonly List<Vector3> vertices=new List<Vector3>(210000);
            internal readonly List<Vector2> uv=new List<Vector2>(210000);
            internal readonly List<int>[] indices=new List<int>[8];
            internal Geometry(){for(int i=0;i<indices.Length;i++)indices[i]=new List<int>();}
            internal void Tri(int mat,Vector3 a,Vector3 b,Vector3 c)
            {
                int start=vertices.Count;
                vertices.Add(a);vertices.Add(b);vertices.Add(c);
                uv.Add(new Vector2(a.x*.2f,a.z*.2f));
                uv.Add(new Vector2(b.x*.2f,b.z*.2f));
                uv.Add(new Vector2(c.x*.2f,c.z*.2f));
                indices[mat].Add(start);indices[mat].Add(start+1);indices[mat].Add(start+2);
            }
            internal void UpQuad(int mat,Vector3 a,Vector3 b,Vector3 c,Vector3 d)
            {
                Tri(mat,a,c,b);Tri(mat,a,d,c);
            }
        }
        private struct Seed
        {
            internal Vector2 p;
            internal float size;
            internal Seed(Vector2 p,float size){this.p=p;this.size=size;}
        }
        private static float BaseHeight(float x,float z)
        {
            // Matches the authored Ashen heightfield on which the buildings were placed.
            float rad=Mathf.Sqrt(x*x*.85f*.85f+z*z);
            float t=Mathf.Clamp01((rad-18f)/29f);
            float n=Mathf.Sin(x*.41f+Mathf.Cos(z*.18f))*Mathf.Cos(z*.47f)*.5f+
                Mathf.Sin(x*.11f+z*.24f)*.38f+Mathf.Sin(x*.82f-z*.60f)*.12f;
            return -.06f+(n*.55f+Mathf.Sin(x*.12f-z*.14f)*.35f)*t+
                t*t*2.2f+.19f*(1f-Mathf.Clamp01((rad-20f)/14f));
        }
        private static float TrackX(float z)=>
            Mathf.Sin((z+8f)*.15f)*.66f+Mathf.Sin((z+8f)*.52f)*.17f;
        private static float SoilHeight(float x,float z)
        {
            float detail=Mathf.PerlinNoise(x*.48f+11.7f,z*.48f+7.3f)-.5f;
            float fine=Mathf.PerlinNoise(x*1.65f+72f,z*1.65f+16f)-.5f;
            float nearWalls=Mathf.Max(Mathf.Abs(x)/8.7f,Mathf.Abs(z)/7.0f);
            float walls=1f-Mathf.SmoothStep(0f,1f,Mathf.InverseLerp(.75f,1.2f,nearWalls));
            float road=Mathf.Clamp01((Mathf.Abs(x-TrackX(z))-2.1f)/1.8f);
            float smooth=1f-Mathf.Max(walls*.86f,(z<-7f&&z>-42f?(1f-road)*.78f:0f));
            foreach(var b in Workshops)
                smooth*=Mathf.Lerp(1f,.24f,Mathf.Clamp01(1f-Vector2.Distance(new Vector2(x,z),b)/5f));
            return BaseHeight(x,z)+.025f+(detail*.12f+fine*.038f)*smooth;
        }
        private static float Rand(System.Random random,float lo,float hi)=>
            lo+(hi-lo)*(float)random.NextDouble();
        private static Vector3 At(float x,float z,float offset=0f)=>
            new Vector3(x,SoilHeight(x,z)+offset,z);

        internal static GameObject Build(Transform cityParent,IReadOnlyDictionary<string,Material> palette)
        {
            var geo=new Geometry();var rng=new System.Random(26813);
            AddContinuousMeadow(geo);
            AddWornRoad(geo);
            AddPaving(geo,rng);
            AddRockOutcrops(geo,rng);
            AddPlants(geo,rng);
            AddCraftDebris(geo,rng);
            var mesh=new Mesh{name="Ashen premium · sculpted meadow, organic paving and inhabited detail"};
            mesh.indexFormat=IndexFormat.UInt32;
            mesh.SetVertices(geo.vertices);mesh.SetUVs(0,geo.uv);
            mesh.subMeshCount=geo.indices.Length;
            for(int i=0;i<geo.indices.Length;i++)mesh.SetTriangles(geo.indices[i],i,false);
            mesh.RecalculateNormals();mesh.RecalculateBounds();
            var existing=AssetDatabase.LoadAssetAtPath<Mesh>(MeshAsset);
            if(existing==null){AssetDatabase.CreateAsset(mesh,MeshAsset);existing=mesh;}
            else{EditorUtility.CopySerialized(mesh,existing);UnityEngine.Object.DestroyImmediate(mesh);}
            var result=new GameObject("ASHEN · inhabited terrain / dressed path / artisan yards");
            result.transform.SetParent(cityParent,false);
            result.AddComponent<MeshFilter>().sharedMesh=existing;
            var renderer=result.AddComponent<MeshRenderer>();
            renderer.sharedMaterials=new[]{palette["moss"],palette["soil"],palette["stoneLight"],
                palette["cut"],palette["basalt"],palette["oak"],palette["grass_leaf"],palette["grass_leafLight"]};
            renderer.receiveShadows=true;renderer.shadowCastingMode=ShadowCastingMode.On;
            Debug.Log("ASHEN_PREMIUM_TERRAIN_BUILT vertices="+geo.vertices.Count+
                " triangles="+(geo.vertices.Count/3)+" materialSections="+geo.indices.Length);
            return result;
        }
        private static void AddContinuousMeadow(Geometry g)
        {
            // Dense central terrain: no coarse triangular flats or floating foundations.
            const int count=120;const float reach=68f;
            for(int iz=0;iz<count;iz++)for(int ix=0;ix<count;ix++)
            {
                float x0=-reach+2*reach*ix/count,x1=-reach+2*reach*(ix+1)/count;
                float z0=-reach+2*reach*iz/count,z1=-reach+2*reach*(iz+1)/count;
                g.UpQuad(Meadow,At(x0,z0),At(x1,z0),At(x1,z1),At(x0,z1));
            }
        }
        private static void AddWornRoad(Geometry g)
        {
            // Smoothly curving compact-earth base beneath fitted, irregular stone islands.
            for(int i=0;i<99;i++)
            {
                float z0=-43f+i*.37f,z1=z0+.37f;
                float width0=2.35f+.22f*Mathf.Sin(z0*.27f),width1=2.35f+.22f*Mathf.Sin(z1*.27f);
                float x0=TrackX(z0),x1=TrackX(z1);
                Vector3 a=At(x0-width0,z0,.016f),b=At(x0+width0,z0,.016f);
                Vector3 c=At(x1+width1,z1,.016f),d=At(x1-width1,z1,.016f);
                g.UpQuad(Earth,a,b,c,d);
            }
        }
        private static bool Footprint(float x,float z)
        {
            if(x>-.9f&&x<4.75f&&z>-3.2f&&z<4.0f)return true;
            if(x>-3.9f&&x<-.3f&&z>.7f&&z<5.1f)return true;
            return false;
        }
        private static void AddPaving(Geometry g,System.Random rng)
        {
            var sets=new List<List<Seed>>();
            var approach=new List<Seed>();
            // Jittered sites become clipped Voronoi polygons, NOT repeated rectangles.
            for(float z=-8.5f;z>=-39;z-=.79f)
            for(float x=-2.5f;x<=2.6f;x+=.77f)
            {
                float zz=z+Rand(rng,-.23f,.23f);
                float xx=x+Rand(rng,-.27f,.27f)+(Mathf.RoundToInt(-z/.79f)%2)*.18f+TrackX(zz);
                float edge=Mathf.Abs(xx-TrackX(zz));
                float chance=Mathf.Lerp(.93f,.50f,Mathf.InverseLerp(-9f,-39f,zz));
                chance*=edge>2.1f?.48f:1f;
                if((float)rng.NextDouble()>chance)continue;
                approach.Add(new Seed(new Vector2(xx,zz),Rand(rng,.58f,.94f)));
            }
            sets.Add(approach);
            var courtyard=new List<Seed>();
            for(float z=-5.2f;z<5f;z+=.77f)
            for(float x=-6.85f;x<6.85f;x+=.80f)
            {
                float xx=x+Rand(rng,-.28f,.28f)+(Mathf.RoundToInt(z/.77f)%2)*.20f;
                float zz=z+Rand(rng,-.23f,.23f);
                if(Footprint(xx,zz)||((float)rng.NextDouble()<.19f))continue;
                courtyard.Add(new Seed(new Vector2(xx,zz),Rand(rng,.58f,.88f)));
            }
            sets.Add(courtyard);
            foreach(var b in Workshops)
            {
                var apron=new List<Seed>();
                for(float z=-4.0f;z<=-1.75f;z+=.74f)
                for(float x=-2.0f;x<=2.0f;x+=.72f)
                {
                    if((float)rng.NextDouble()<.27f)continue;
                    apron.Add(new Seed(b+new Vector2(x+Rand(rng,-.2f,.2f),z+Rand(rng,-.2f,.2f)),Rand(rng,.50f,.85f)));
                }
                sets.Add(apron);
            }
            int stoneNumber=0;
            foreach(var set in sets)
            foreach(var seed in set)
            {
                var polygon=new List<Vector2>{seed.p+new Vector2(-seed.size,-seed.size),
                    seed.p+new Vector2(seed.size,-seed.size),seed.p+new Vector2(seed.size,seed.size),
                    seed.p+new Vector2(-seed.size,seed.size)};
                foreach(var other in set)
                {
                    Vector2 between=other.p-seed.p;
                    if(between.sqrMagnitude<.0001f||between.sqrMagnitude>6.25f)continue;
                    polygon=Clip(polygon,between,Vector2.Dot((other.p+seed.p)*.5f,between));
                    if(polygon.Count<3)break;
                }
                if(polygon.Count<3)continue;
                float shrink=Rand(rng,.89f,.96f);
                var centre=Vector2.zero;
                foreach(var point in polygon)centre+=point;
                centre/=polygon.Count;
                int material=stoneNumber%29==0?Basalt:(stoneNumber%7==0?Limestone:Cutstone);
                stoneNumber++;
                var top=new List<Vector3>(polygon.Count);
                var bottom=new List<Vector3>(polygon.Count);
                foreach(var pt in polygon)
                {
                    Vector2 q=centre+(pt-centre)*shrink;
                    float high=Rand(rng,.058f,.084f);
                    top.Add(At(q.x,q.y,high));bottom.Add(At(q.x,q.y,.010f));
                }
                var mid=At(centre.x,centre.y,.075f);
                for(int i=0;i<top.Count;i++)
                {
                    int next=(i+1)%top.Count;
                    g.Tri(material,mid,top[next],top[i]);
                    g.Tri(material,top[i],top[next],bottom[next]);
                    g.Tri(material,top[i],bottom[next],bottom[i]);
                }
            }
        }
        private static List<Vector2> Clip(List<Vector2> points,Vector2 normal,float limit)
        {
            var output=new List<Vector2>();
            for(int i=0;i<points.Count;i++)
            {
                var a=points[i],b=points[(i+1)%points.Count];
                float da=Vector2.Dot(a,normal)-limit,db=Vector2.Dot(b,normal)-limit;
                bool insideA=da<=0f,insideB=db<=0f;
                if(insideA)output.Add(a);
                if(insideA!=insideB)
                {
                    float t=da/(da-db);output.Add(Vector2.Lerp(a,b,Mathf.Clamp01(t)));
                }
            }
            return output;
        }
        private static void AddRockOutcrops(Geometry g,System.Random rng)
        {
            var pockets=new[]{new Vector2(-24,-15),new Vector2(28,16),new Vector2(-29,19),
                new Vector2(26,-20),new Vector2(-9,24),new Vector2(12,30)};
            foreach(var basePoint in pockets)
            for(int n=0;n<9;n++)
            {
                float x=basePoint.x+Rand(rng,-5,5),z=basePoint.y+Rand(rng,-4,4);
                float radius=Rand(rng,.27f,.95f),height=Rand(rng,.18f,.56f);
                int sides=7+RandomInt(rng,4);var floor=new Vector3[sides];var crown=new Vector3[sides];
                for(int i=0;i<sides;i++)
                {
                    float angle=2*Mathf.PI*i/sides;
                    float r=radius*Rand(rng,.76f,1.19f);
                    float px=x+Mathf.Cos(angle)*r,pz=z+Mathf.Sin(angle)*r;
                    floor[i]=At(px,pz,-.025f);
                    crown[i]=At(x+(px-x)*.59f,z+(pz-z)*.59f,height*Rand(rng,.70f,1.04f));
                }
                var peak=At(x,z,height*1.22f);
                for(int i=0;i<sides;i++)
                {
                    int j=(i+1)%sides;int mat=(i+n)%5==0?Limestone:Basalt;
                    g.Tri(mat,crown[j],crown[i],peak);
                    g.Tri(mat,floor[j],floor[i],crown[i]);
                    g.Tri(mat,floor[j],crown[i],crown[j]);
                }
            }
        }
        private static int RandomInt(System.Random rng,int max)=>rng.Next(max);
        private static bool Protected(float x,float z)
        {
            if(Mathf.Abs(x)<9.5f&&Mathf.Abs(z)<8.5f)return true;
            if(z< -7f&&z> -43f&&Mathf.Abs(x-TrackX(z))<3.35f)return true;
            foreach(var b in Workshops)
                if(Vector2.Distance(new Vector2(x,z),b)<4.3f)return true;
            return false;
        }
        private static void AddPlants(Geometry g,System.Random rng)
        {
            var pockets=new[]{new Vector2(-10,-15),new Vector2(12,-14),new Vector2(-12,15),
                new Vector2(11,14),new Vector2(-21,-1),new Vector2(22,1),
                new Vector2(-28,22),new Vector2(26,-22),new Vector2(0,22)};
            foreach(var b in pockets)
            for(int clump=0;clump<75;clump++)
            {
                float x=b.x+Rand(rng,-7,7),z=b.y+Rand(rng,-5.5f,5.5f);
                if(Protected(x,z))continue;
                int leaves=5+RandomInt(rng,6);
                float h=Rand(rng,.27f,.72f),w=Rand(rng,.13f,.25f);
                for(int i=0;i<leaves;i++)
                {
                    float a=2*Mathf.PI*i/leaves+Rand(rng,-.20f,.20f);
                    float px=x+Mathf.Cos(a)*w,pz=z+Mathf.Sin(a)*w;
                    var v0=At(x,z,.035f),tip=At(px+Mathf.Cos(a)*w,pz+Mathf.Sin(a)*w,h);
                    var v1=At(px-Mathf.Sin(a)*w*.45f,pz+Mathf.Cos(a)*w*.45f,.07f);
                    var v2=At(px+Mathf.Sin(a)*w*.45f,pz-Mathf.Cos(a)*w*.45f,.07f);
                    g.Tri((i+clump)%5==0?FoliageLight:Foliage,v0,tip,v1);
                    g.Tri((i+clump)%5==0?FoliageLight:Foliage,v0,v2,tip);
                }
            }
        }
        private static void AddCraftDebris(Geometry g,System.Random rng)
        {
            for(int i=0;i<22;i++)
            {
                float x=-17.2f+Rand(rng,-3.6f,3.6f),z=-6.2f+Rand(rng,-5.2f,-3.1f);
                float len=Rand(rng,.50f,1.7f),width=Rand(rng,.09f,.18f);
                var a=At(x-width,z,.08f),b=At(x+width,z,.08f);
                var c=At(x+width,z+len,.08f),d=At(x-width,z+len,.08f);
                g.UpQuad(Timber,a,b,c,d);
            }
            for(int i=0;i<28;i++)
            {
                float x=17.4f+Rand(rng,-4.3f,4.3f),z=8.8f+Rand(rng,-5.4f,-3.5f);
                float radius=Rand(rng,.17f,.43f),high=Rand(rng,.10f,.31f);
                var p0=At(x-radius,z-radius,.015f),p1=At(x+radius,z-radius,.015f);
                var p2=At(x+radius,z+radius,.015f),p3=At(x-radius,z+radius,.015f);
                var top=At(x,z,high);int material=i%7==0?Basalt:Limestone;
                g.Tri(material,top,p1,p0);g.Tri(material,top,p2,p1);
                g.Tri(material,top,p3,p2);g.Tri(material,top,p0,p3);
            }
        }
    }
}
