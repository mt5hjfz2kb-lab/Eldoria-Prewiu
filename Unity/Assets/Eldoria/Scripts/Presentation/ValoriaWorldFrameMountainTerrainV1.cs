using Eldoria.Domain;
using UnityEngine;
using UnityEngine.Rendering;
using Object = UnityEngine.Object;

namespace Eldoria.Presentation
{
    public static class ValoriaWorldFrameMountainTerrainV1
    {
        public static bool Enabled=true;
        const string RootName="Valoria · World Frame Mountain Terrain v1";

        public static void Build(Transform parent,PlayerState state)
        {
            if(!Enabled||parent==null)return;
            var old=GameObject.Find(RootName);
            if(old!=null)Object.DestroyImmediate(old);

            var root=new GameObject(RootName);
            root.transform.SetParent(parent,true);

            var material=ValoriaKit.ExternalPbrSurfaceMaterial(
                "rock",new Color(.34f,.36f,.34f,1f),new Vector2(6f,6f),.025f,1.12f)
                ?? ValoriaKit.DetailedSurfaceMaterial(
                    new Color(.31f,.33f,.30f,1f),"earth",new Vector2(6f,6f),1.12f);

            BuildSide(root.transform,"west wall",-1,material);
            BuildSide(root.transform,"east wall",1,material);
            BuildRear(root.transform,material);
        }

        static void BuildSide(Transform root,string role,int sign,Material material)
        {
            const int along=49;
            const int across=17;
            const float zMin=-5f,zMax=32f;
            const float inner=12.8f,outer=31.5f;
            var verts=new Vector3[along*across];
            var uv=new Vector2[verts.Length];
            var tris=new int[(along-1)*(across-1)*6];

            for(int iz=0;iz<along;iz++)
            {
                float tz=iz/(float)(along-1);
                float z=Mathf.Lerp(zMin,zMax,tz);
                float frontGate=Mathf.SmoothStep(0f,1f,Mathf.Clamp01((z+2f)/7f));
                float rearBoost=Mathf.SmoothStep(0f,1f,Mathf.Clamp01((z-14f)/15f));
                for(int ix=0;ix<across;ix++)
                {
                    float tx=ix/(float)(across-1);
                    float xAbs=Mathf.Lerp(inner,outer,tx);
                    float edge=Mathf.SmoothStep(0f,1f,tx);
                    float ridge=edge*edge*(5.2f+rearBoost*2.2f)*frontGate;
                    float macro=(Mathf.Sin(z*.21f+sign*.7f)+Mathf.Sin(xAbs*.18f)+Mathf.Sin((xAbs+z)*.095f))*0.36f*edge*frontGate;
                    float shelves=.28f*Mathf.Sin(tx*10.5f+z*.12f)*edge*frontGate;
                    float y=-.24f+ridge+macro+shelves;
                    if(tx<.09f)y=Mathf.Lerp(-.24f,y,tx/.09f);
                    int i=iz*across+ix;
                    verts[i]=new Vector3(sign*xAbs,y,z);
                    uv[i]=new Vector2(sign*xAbs*.11f,z*.11f);
                }
            }
            FillGridTriangles(tris,along,across,sign<0);
            CreateMeshObject(root,role,verts,uv,tris,material);
        }

        static void BuildRear(Transform root,Material material)
        {
            const int across=57;
            const int depth=17;
            const float xMin=-29f,xMax=29f,zMin=14.5f,zMax=37f;
            var verts=new Vector3[across*depth];
            var uv=new Vector2[verts.Length];
            var tris=new int[(across-1)*(depth-1)*6];

            for(int iz=0;iz<depth;iz++)
            {
                float tz=iz/(float)(depth-1);
                float z=Mathf.Lerp(zMin,zMax,tz);
                float rear=Mathf.SmoothStep(0f,1f,tz);
                for(int ix=0;ix<across;ix++)
                {
                    float tx=ix/(float)(across-1);
                    float x=Mathf.Lerp(xMin,xMax,tx);
                    float side=Mathf.SmoothStep(0f,1f,Mathf.Clamp01((Mathf.Abs(x)-7.5f)/17f));
                    float saddle=.35f+side*.65f;
                    float ridge=rear*rear*(4.8f+side*3.3f)*saddle;
                    float macro=(Mathf.Sin(x*.16f)+Mathf.Sin(z*.20f)+Mathf.Sin((x-z)*.10f))*0.42f*rear;
                    float y=-.26f+ridge+macro;
                    if(tz<.10f)y=Mathf.Lerp(-.26f,y,tz/.10f);
                    int i=iz*across+ix;
                    verts[i]=new Vector3(x,y,z);
                    uv[i]=new Vector2(x*.11f,z*.11f);
                }
            }
            FillGridTriangles(tris,depth,across,false);
            CreateMeshObject(root,"rear wall",verts,uv,tris,material);
        }

        static void FillGridTriangles(int[] tris,int rows,int cols,bool flip)
        {
            int ti=0;
            for(int r=0;r<rows-1;r++)
            for(int c=0;c<cols-1;c++)
            {
                int a=r*cols+c,b=a+1,d=(r+1)*cols+c,e=d+1;
                if(!flip)
                {
                    tris[ti++]=a;tris[ti++]=d;tris[ti++]=b;
                    tris[ti++]=b;tris[ti++]=d;tris[ti++]=e;
                }
                else
                {
                    tris[ti++]=a;tris[ti++]=b;tris[ti++]=d;
                    tris[ti++]=b;tris[ti++]=e;tris[ti++]=d;
                }
            }
        }

        static void CreateMeshObject(Transform root,string role,Vector3[] verts,Vector2[] uv,int[] tris,Material material)
        {
            var mesh=new Mesh{name="Valoria World Frame v1 · "+role};
            mesh.indexFormat=IndexFormat.UInt32;
            mesh.vertices=verts;
            mesh.uv=uv;
            mesh.triangles=tris;
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();

            var go=new GameObject("Valoria · World Frame v1 · "+role);
            go.transform.SetParent(root,true);
            go.AddComponent<MeshFilter>().sharedMesh=mesh;
            var renderer=go.AddComponent<MeshRenderer>();
            renderer.sharedMaterial=material;
            renderer.shadowCastingMode=ShadowCastingMode.On;
            renderer.receiveShadows=true;
        }
    }
}
