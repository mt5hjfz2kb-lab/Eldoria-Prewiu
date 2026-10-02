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
                "rock",new Color(.285f,.31f,.295f,1f),new Vector2(10f,10f),.018f,1.24f)
                ?? ValoriaKit.DetailedSurfaceMaterial(
                    new Color(.265f,.29f,.27f,1f),"earth",new Vector2(10f,10f),1.24f);

            BuildContinuousValley(root.transform,material);
        }

        static void BuildContinuousValley(Transform root,Material material)
        {
            // Review iteration: balanced side walls + open central saddle; geometry unchanged by proof trigger.
            const int cols=81;
            const int rows=73;
            const float xMin=-25f,xMax=25f,zMin=-13f,zMax=31f;
            const float hiddenY=-.64f;

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

                    float side=Mathf.SmoothStep(0f,1f,Mathf.Clamp01((Mathf.Abs(x)-13.4f)/6.3f));
                    float rear=Mathf.SmoothStep(0f,1f,Mathf.Clamp01((z-15.0f)/10.5f));
                    // Keep the player approach open, but let the world frame begin closer to the inhabited mass.
                    float frontGate=Mathf.SmoothStep(0f,1f,Mathf.Clamp01((z-1.0f)/6.0f));
                    float sideRelief=side*frontGate;
                    float relief=Mathf.Max(sideRelief,rear);

                    // Final framing refinement: keep the proven silhouette but make it read as rock,
                    // not a smooth berm. The matched camera magnifies the west/left wall, so it stays lower.
                    float rearSide=rear*side;
                    float sideScale=x<0f?.60f:.78f;
                    float ridge=sideRelief*sideRelief*3.35f*sideScale + rearSide*rearSide*1.70f;

                    float broad=Mathf.Sin(x*.115f+z*.035f)*.30f
                               +Mathf.Sin(z*.145f-x*.028f)*.24f
                               +Mathf.Sin((x+z)*.071f)*.16f;
                    float macroNoise=(Mathf.PerlinNoise(x*.085f+7.31f,z*.085f+11.17f)-.5f)*1.05f;
                    float detailNoise=(Mathf.PerlinNoise(x*.22f+19.43f,z*.22f+2.71f)-.5f)*.34f;
                    float strata=Mathf.Sin(z*.58f+x*.075f)*.10f;
                    float visibleRelief=Mathf.Max(sideRelief,rearSide);
                    float y=hiddenY + ridge
                        + (broad+macroNoise+detailNoise+strata)*visibleRelief*.72f;

                    // Keep a generous central basin and the full approach invisible beneath gameplay ground.
                    float cityX=1f-Mathf.SmoothStep(0f,1f,Mathf.Clamp01((Mathf.Abs(x)-10.6f)/3.8f));
                    float cityZ=1f-Mathf.SmoothStep(0f,1f,Mathf.Clamp01((z-10.5f)/7.0f));
                    float basin=cityX*cityZ;
                    y=Mathf.Lerp(y,hiddenY,basin);
                    if(z<1.5f)y=hiddenY;

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
