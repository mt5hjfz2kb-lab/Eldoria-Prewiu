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

            // One continuous terrain sheet avoids detached panels and inter-piece seams.
            // The inhabited corridor sits below canonical ground; only side/rear relief emerges.
            var material=ValoriaKit.ExternalPbrSurfaceMaterial(
                "rock",new Color(.29f,.32f,.30f,1f),new Vector2(6f,6f),.018f,1.18f)
                ?? ValoriaKit.DetailedSurfaceMaterial(
                    new Color(.275f,.30f,.275f,1f),"earth",new Vector2(6f,6f),1.18f);

            BuildContinuousValley(root.transform,material);
        }

        static void BuildContinuousValley(Transform root,Material material)
        {
            const int cols=81;
            const int rows=73;
            const float xMin=-33f,xMax=33f,zMin=-16f,zMax=37f;
            const float hiddenY=-.58f;

            var verts=new Vector3[cols*rows];
            var uv=new Vector2[verts.Length];
            var tris=new int[(cols-1)*(rows-1)*6];

            for(int rz=0;rz<rows;rz++)
            {
                float tz=rz/(float)(rows-1);
                float z=Mathf.Lerp(zMin,zMax,tz);
                for(int cx=0;cx<cols;cx++)
                {
                    float tx=cx/(float)(cols-1);
                    float x=Mathf.Lerp(xMin,xMax,tx);

                    float side=Mathf.SmoothStep(0f,1f,Mathf.Clamp01((Mathf.Abs(x)-11.2f)/11.8f));
                    float rear=Mathf.SmoothStep(0f,1f,Mathf.Clamp01((z-12.5f)/15.5f));
                    float frontGate=Mathf.SmoothStep(0f,1f,Mathf.Clamp01((z+1.0f)/7.0f));
                    float sideRelief=side*frontGate;
                    float relief=Mathf.Max(sideRelief,rear);

                    // Broad natural valley walls: restrained height, no giant planar curtain.
                    float rearSaddle=Mathf.Lerp(.10f,1f,Mathf.SmoothStep(0f,1f,Mathf.Clamp01((Mathf.Abs(x)-6.5f)/9.5f)));
                    float ridge=sideRelief*sideRelief*6.2f + rear*rear*(5.7f+side*2.4f)*rearSaddle;
                    float broad=Mathf.Sin(x*.115f+z*.035f)*.42f
                               +Mathf.Sin(z*.145f-x*.028f)*.33f
                               +Mathf.Sin((x+z)*.071f)*.23f;
                    float fine=Mathf.Sin(x*.31f-z*.17f)*.13f;
                    float centreSky=Mathf.SmoothStep(0f,1f,Mathf.Clamp01((Mathf.Abs(x)-5.5f)/8.5f));
                    float rearVisible=rear*Mathf.Lerp(.08f,1f,centreSky);
                    float visibleRelief=Mathf.Max(sideRelief,rearVisible);
                    float y=hiddenY + ridge + (broad+fine)*visibleRelief;

                    // Keep a generous central basin and the full approach invisible beneath gameplay ground.
                    float cityX=1f-Mathf.SmoothStep(0f,1f,Mathf.Clamp01((Mathf.Abs(x)-10.2f)/4.2f));
                    float cityZ=1f-Mathf.SmoothStep(0f,1f,Mathf.Clamp01((z-10.5f)/7.5f));
                    float basin=cityX*cityZ;
                    y=Mathf.Lerp(y,hiddenY,basin);
                    if(z<-4.5f)y=hiddenY;

                    int i=rz*cols+cx;
                    verts[i]=new Vector3(x,y,z);
                    uv[i]=new Vector2(x*.13f,z*.13f);
                }
            }

            FillGridTriangles(tris,rows,cols,false);
            CreateMeshObject(root,"continuous mountain valley",verts,uv,tris,material);
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
