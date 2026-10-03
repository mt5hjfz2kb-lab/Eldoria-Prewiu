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

        const string RootName="Valoria · Lower City TerrainData v1";

        public static void Build(Transform parent,PlayerState state)
        {
            if(!Enabled||parent==null)return;

            var old=GameObject.Find(RootName);
            if(old!=null)Object.DestroyImmediate(old);

            const int hm=129;
            const int alpha=128;
            const float sizeX=22.0f;
            const float sizeZ=16.0f;
            const float sizeY=6.0f;
            const float originY=-5.05f;

            var data=new TerrainData{
                name="Valoria Lower City TerrainData v1",
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

            var dirtLayer=LayerFrom(dirtMat,"Valoria Terrain · dirt",new Vector2(3.2f,3.2f),.04f);
            var rockLayer=LayerFrom(rockMat,"Valoria Terrain · rock",new Vector2(2.5f,2.5f),.08f);
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
                    float d=Footprint(wx,wz);
                    float front=Mathf.InverseLerp(-2.7f,-7.6f,wz);
                    float ax=Mathf.Abs(wx);
                    float route=Route(wx,wz);
                    float pad=Pads(wx,wz);
                    bool valleyGap=front>.38f && ax>2.2f && ax<4.8f && route<.22f && pad<.28f;
                    bool outer=d>1.00f && route<.18f && pad<.24f;
                    bool surface=!valleyGap&&!outer;
                    holes[z,x]=surface;
                    if(surface)SurfaceSamples++; else HoleSamples++;
                }
            }
            data.SetHoles(0,0,holes);

            var go=Terrain.CreateTerrainGameObject(data);
            go.name=RootName;
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
            }

            var collider=go.GetComponent<TerrainCollider>();
            if(collider!=null)collider.enabled=false;
            foreach(var h in go.GetComponentsInChildren<WorldHotspot>(true))Object.DestroyImmediate(h);
        }

        static TerrainLayer LayerFrom(Material source,string name,Vector2 tile,float smoothness)
        {
            var layer=new TerrainLayer{name=name,tileSize=tile,metallic=0f,smoothness=smoothness};
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
            float d=Footprint(x,z);
            float n1=Mathf.PerlinNoise((x+17.3f)*.14f,(z+11.7f)*.14f)-.5f;
            float n2=Mathf.PerlinNoise((x-4.1f)*.31f,(z+23.5f)*.31f)-.5f;
            float n3=Mathf.PerlinNoise((x+31.2f)*.075f,(z-7.4f)*.075f)-.5f;

            float top=-.24f+n1*.18f+n2*.055f;
            float fall=Mathf.SmoothStep(0f,1f,Mathf.InverseLerp(.54f,.98f,d));
            float bottom=-4.15f+n1*.52f+n3*.30f;
            float y=Mathf.Lerp(top,bottom,fall);

            float route=Route(x,z);
            float pad=Pads(x,z);
            float support=Mathf.Max(route*.90f,pad*.72f);
            y=Mathf.Lerp(y,Mathf.Max(y,-.18f),support*(1f-fall*.82f));

            float front=Mathf.InverseLerp(-2.6f,-7.6f,z);
            float side=Mathf.SmoothStep(0f,1f,Mathf.InverseLerp(4.8f,8.0f,Mathf.Abs(x)));
            float valley=front*(1f-route)*(1f-side)*(1f-pad*.65f);
            y-=valley*1.20f;
            y+=front*side*.18f;
            return y;
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
