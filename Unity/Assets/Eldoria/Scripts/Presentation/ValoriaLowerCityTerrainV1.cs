using Eldoria.Domain;
using UnityEngine;
using Object=UnityEngine.Object;

namespace Eldoria.Presentation
{
    // Visual-only TerrainData proof for the lower city. It replaces the generated
    // foreground mesh with a smoothed heightmap, slope-blended surfaces and an
    // irregular silhouette while leaving gameplay colliders/hotspots untouched.
    public static class ValoriaLowerCityTerrainV1
    {
        public static bool Enabled=false;
        public static int HoleSamples{get;private set;}
        public static int SurfaceSamples{get;private set;}
        public static int SuppressedLegacyRenderers{get;private set;}

        const string RootName="Valoria · Lower City TerrainData v1";

        public static void Build(Transform parent,PlayerState state)
        {
            if(!Enabled||parent==null)return;

            var old=GameObject.Find(RootName);
            if(old!=null)Object.DestroyImmediate(old);

            SuppressedLegacyRenderers=SuppressLegacySupports();

            const int hm=129;
            const int alpha=128;
            const float sizeX=22.0f;
            const float sizeZ=16.0f;
            const float sizeY=6.0f;
            const float originY=-5.05f;

            var data=new TerrainData{
                name="Valoria Lower City Hybrid Terrain v5",
                heightmapResolution=hm,
                alphamapResolution=alpha,
                baseMapResolution=256,
                size=new Vector3(sizeX,sizeY,sizeZ)
            };

            var heights=new float[hm,hm];
            for(int z=0;z<hm;z++)
            {
                float nz=z/(float)(hm-1);
                float wz=-8.0f+nz*sizeZ;
                for(int x=0;x<hm;x++)
                {
                    float nx=x/(float)(hm-1);
                    float wx=-11.0f+nx*sizeX;
                    float worldY=Height(wx,wz);
                    heights[z,x]=Mathf.Clamp01((worldY-originY)/sizeY);
                }
            }

            // Two light smoothing passes remove high-frequency procedural ridges without
            // flattening the authored route/pads.
            Smooth(heights,hm,2);
            data.SetHeights(0,0,heights);

            var dirtMat=ValoriaKit.ExternalPbrSurfaceMaterial(
                "dirt",new Color(.42f,.37f,.29f,1f),new Vector2(1.25f,1.25f),.016f,.90f);
            var rockMat=ValoriaKit.ExternalPbrSurfaceMaterial(
                "rock",new Color(.38f,.39f,.37f,1f),new Vector2(1.10f,1.10f),.018f,.98f);

            var dirtLayer=LayerFrom(dirtMat,"Valoria Terrain · dirt",new Vector2(3.2f,3.2f),.04f,new Color(.62f,.55f,.42f,1f));
            var rockLayer=LayerFrom(rockMat,"Valoria Terrain · rock",new Vector2(2.5f,2.5f),.08f,new Color(.52f,.53f,.49f,1f));
            data.terrainLayers=new[]{dirtLayer,rockLayer};

            var splat=new float[alpha,alpha,2];
            for(int z=0;z<alpha;z++)
            {
                float nz=z/(float)(alpha-1);
                float wz=-8.0f+nz*sizeZ;
                for(int x=0;x<alpha;x++)
                {
                    float nx=x/(float)(alpha-1);
                    float wx=-11.0f+nx*sizeX;
                    float slope=data.GetSteepness(nx,nz);
                    float steep=Mathf.SmoothStep(0f,1f,Mathf.InverseLerp(17f,44f,slope));
                    float edge=Mathf.SmoothStep(.45f,.95f,Footprint(wx,wz));
                    float route=Route(wx,wz);
                    float rock=Mathf.Clamp01(Mathf.Max(steep,edge*.72f)*(1f-route*.58f));
                    splat[z,x,0]=1f-rock;
                    splat[z,x,1]=rock;
                }
            }
            data.SetAlphamaps(0,0,splat);

            var holes=new bool[data.holesResolution,data.holesResolution];
            HoleSamples=0;SurfaceSamples=0;
            for(int z=0;z<data.holesResolution;z++)
            {
                float nz=z/(float)(data.holesResolution-1);
                float wz=-8.0f+nz*sizeZ;
                for(int x=0;x<data.holesResolution;x++)
                {
                    float nx=x/(float)(data.holesResolution-1);
                    float wx=-11.0f+nx*sizeX;
                    float land=LandformField(wx,wz);
                    float edgeNoise=(Mathf.PerlinNoise((wx+19.1f)*.16f,(wz+8.4f)*.16f)-.5f)*.055f;
                    bool surface=land>(.12f+edgeNoise);
                    holes[z,x]=surface;
                    if(surface)SurfaceSamples++; else HoleSamples++;
                }
            }
            data.SetHoles(0,0,holes);

            var go=Terrain.CreateTerrainGameObject(data);
            go.name=RootName+" · hybrid terrain v5";
            go.transform.SetParent(parent,true);
            go.transform.position=new Vector3(-11.0f,originY,-8.0f);

            var terrain=go.GetComponent<Terrain>();
            if(terrain!=null)
            {
                terrain.allowAutoConnect=false;
                terrain.drawInstanced=true;
                terrain.heightmapPixelError=2f;
                terrain.basemapDistance=200f;
                terrain.shadowCastingMode=UnityEngine.Rendering.ShadowCastingMode.On;

                // URP supplies Terrain Lit for Terrain rendering; keep the default
                // pipeline terrain material and drive appearance through TerrainLayer remaps.
            }

            // TerrainCollider lives in the optional Terrain Physics assembly, which this
            // presentation asmdef intentionally does not reference. Remove it without a hard type dependency.
            foreach(var component in go.GetComponents<Component>())
                if(component!=null&&component.GetType().Name=="TerrainCollider")
                    Object.DestroyImmediate(component);
            foreach(var h in go.GetComponentsInChildren<WorldHotspot>(true))Object.DestroyImmediate(h);
        }

        static int SuppressLegacySupports()
        {
            int count=0;
            foreach(var r in Object.FindObjectsByType<Renderer>(FindObjectsSortMode.None))
            {
                if(r==null||!r.enabled||!r.gameObject.activeInHierarchy)continue;
                var b=r.bounds;
                string chain=Chain(r.transform);

                if(chain.Contains("backplate")||chain.Contains("bastion")||
                   chain.Contains("aserradero")||chain.Contains("cuartel")||
                   chain.Contains("granary")||chain.Contains("granero")||
                   chain.Contains("premium secondary"))continue;

                bool lower=b.center.y<1.65f && b.center.z<5.8f;
                bool superseded=
                    chain.Contains("lower cliff authored rock")||
                    chain.Contains("foreground edge")||
                    chain.Contains("terrainterrace")||
                    chain.Contains("expansion edge geology")||
                    chain.Contains("environment uplift · rock")||
                    chain.Contains("cliff island · lower")||
                    chain.Contains("cliff island · middle")||
                    chain.Contains("assetlibrary reprocessing · hero lower terrace")||
                    chain.Contains("broadrockplatform")||
                    chain.Contains("steppedrockterrace")||
                    chain.Contains("local lower support");

                if(lower&&superseded){r.enabled=false;count++;}
            }
            return count;
        }

        static string Chain(Transform t)
        {
            string s="";
            for(var p=t;p!=null;p=p.parent)s+="|"+p.name.ToLowerInvariant();
            return s;
        }

        static TerrainLayer LayerFrom(Material source,string name,Vector2 tile,float smoothness,Color tint)
        {
            var layer=new TerrainLayer{name=name,tileSize=tile,metallic=0f,smoothness=smoothness};
            layer.diffuseRemapMin=new Vector4(tint.r*.20f,tint.g*.20f,tint.b*.20f,0f);
            layer.diffuseRemapMax=new Vector4(tint.r,tint.g,tint.b,1f);
            if(source!=null)
            {
                Texture2D baseTex=null,normalTex=null;
                if(source.HasProperty("_BaseMap"))baseTex=source.GetTexture("_BaseMap") as Texture2D;
                if(baseTex==null&&source.HasProperty("_MainTex"))baseTex=source.GetTexture("_MainTex") as Texture2D;
                if(source.HasProperty("_BumpMap"))normalTex=source.GetTexture("_BumpMap") as Texture2D;
                if(normalTex==null&&source.HasProperty("_NormalMap"))normalTex=source.GetTexture("_NormalMap") as Texture2D;
                layer.diffuseTexture=baseTex;
                layer.normalMapTexture=normalTex;
            }
            if(layer.diffuseTexture==null)layer.diffuseTexture=Texture2D.grayTexture;
            return layer;
        }

        static float Height(float x,float z)
        {
            float n1=Mathf.PerlinNoise((x+17.3f)*.14f,(z+11.7f)*.14f)-.5f;
            float n2=Mathf.PerlinNoise((x-4.1f)*.31f,(z+23.5f)*.31f)-.5f;
            float n3=Mathf.PerlinNoise((x+31.2f)*.075f,(z-7.4f)*.075f)-.5f;

            float land=LandformField(x,z);
            float top=-.22f+n1*.14f+n2*.045f;
            float edgeFall=Mathf.SmoothStep(0f,1f,Mathf.InverseLerp(.48f,.12f,land));
            // Terrain owns the walkable/inhabited shelf only. Vertical cliff language
            // is supplied by authored rock meshes around the hole boundary.
            float shoulder=-1.05f+n1*.18f+n3*.10f;
            float y=Mathf.Lerp(top,shoulder,edgeFall);

            // Gameplay buildings keep their certified world positions. The terrain rises
            // under their parcels and under the central stair/route, not across the whole frame.
            float support=Mathf.Max(Route(x,z)*.88f,Pads(x,z)*.78f);
            y=Mathf.Lerp(y,Mathf.Max(y,-.16f),support*(1f-edgeFall*.74f));

            // Slight hierarchy: Bastion shelf is highest, lower economic/military terraces sit below.
            float upper=EllipseField(x,z,0f,4.25f,7.9f,4.45f);
            float lowerTerraces=Mathf.Max(
                Disc(x,z,-7.0f,-2.8f,3.4f),
                Disc(x,z, 7.0f,-4.0f,3.45f));
            y+=upper*.24f-lowerTerraces*.08f;
            return y;
        }

        static float LandformField(float x,float z)
        {
            float central=EllipseField(x,z,0f,3.35f,8.75f,5.25f);
            float sawmill=Disc(x,z,-7.0f,-2.8f,3.55f);
            float barracks=Disc(x,z,7.0f,-4.0f,3.65f);

            float westLink=SegmentField(x,z,-4.55f,.45f,-7.0f,-2.8f,1.70f);
            float eastLink=SegmentField(x,z,4.45f,.15f,7.0f,-4.0f,1.72f);
            float approach=SegmentField(x,z,0f,-1.15f,0f,-7.85f,1.55f);

            return Mathf.Max(central,Mathf.Max(sawmill,
                Mathf.Max(barracks,Mathf.Max(westLink,Mathf.Max(eastLink,approach)))));
        }

        static float EllipseField(float x,float z,float cx,float cz,float rx,float rz)
        {
            float dx=(x-cx)/rx,dz=(z-cz)/rz;
            float d=Mathf.Sqrt(dx*dx+dz*dz);
            return Mathf.Clamp01(1f-d);
        }

        static float SegmentField(float x,float z,float ax,float az,float bx,float bz,float radius)
        {
            var p=new Vector2(x,z);
            var a=new Vector2(ax,az);
            var b=new Vector2(bx,bz);
            var ab=b-a;
            float denom=Mathf.Max(.0001f,Vector2.Dot(ab,ab));
            float t=Mathf.Clamp01(Vector2.Dot(p-a,ab)/denom);
            float d=Vector2.Distance(p,a+ab*t);
            return Mathf.Clamp01(1f-d/radius);
        }

        static float Footprint(float x,float z)
        {
            float ex=Mathf.Abs(x)/9.45f;
            float ez=Mathf.Abs((z+.8f))/6.85f;
            const float p=1.68f;
            return Mathf.Pow(Mathf.Pow(ex,p)+Mathf.Pow(ez,p),1f/p);
        }

        static float Route(float x,float z)
        {
            float width=Mathf.Lerp(1.45f,2.25f,Mathf.InverseLerp(-7.5f,4.5f,z));
            return Mathf.Clamp01(1f-Mathf.Abs(x)/width)*
                   Mathf.Clamp01(1f-Mathf.Abs(z+1.6f)/7.0f);
        }

        static float Pads(float x,float z)
        {
            float p=0f;
            p=Mathf.Max(p,Disc(x,z,-7.15f,-2.65f,2.20f));
            p=Mathf.Max(p,Disc(x,z, 6.65f,-3.80f,2.20f));
            p=Mathf.Max(p,Disc(x,z,-5.15f, 2.55f,2.35f));
            p=Mathf.Max(p,Disc(x,z, 5.10f, 2.75f,2.35f));
            p=Mathf.Max(p,Disc(x,z,-3.95f, 5.05f,2.10f));
            p=Mathf.Max(p,Disc(x,z, 3.95f, 5.10f,2.10f));
            return p;
        }

        static float Disc(float x,float z,float cx,float cz,float radius)
        {
            float dx=x-cx,dz=z-cz;
            return Mathf.Clamp01(1f-Mathf.Sqrt(dx*dx+dz*dz)/radius);
        }

        static void Smooth(float[,] h,int n,int passes)
        {
            var tmp=new float[n,n];
            for(int pass=0;pass<passes;pass++)
            {
                for(int z=0;z<n;z++)
                for(int x=0;x<n;x++)
                {
                    float sum=0f;int count=0;
                    for(int dz=-1;dz<=1;dz++)
                    for(int dx=-1;dx<=1;dx++)
                    {
                        int sx=Mathf.Clamp(x+dx,0,n-1);
                        int sz=Mathf.Clamp(z+dz,0,n-1);
                        sum+=h[sz,sx];count++;
                    }
                    tmp[z,x]=sum/count;
                }
                for(int z=0;z<n;z++)
                for(int x=0;x<n;x++)h[z,x]=tmp[z,x];
            }
        }
    }
}
