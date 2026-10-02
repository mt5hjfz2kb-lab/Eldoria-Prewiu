using System;
using Eldoria.Domain;
using UnityEngine;
using UnityEngine.Rendering;
using Object=UnityEngine.Object;

namespace Eldoria.Presentation
{
    // Candidate underlay for a single readable city landform.
    // Presentation-only: no collider, hotspot, route or gameplay floor ownership.
    public static class ValoriaFullFrameTerrainContinuityV1
    {
        public static bool Enabled=false;
        const string RootName="Valoria · Full Frame Terrain Continuity v1";
        public static int SuppressedShelfRenderers{get;private set;}

        public static void Build(Transform parent,PlayerState state)
        {
            if(!Enabled||parent==null)return;

            var old=GameObject.Find(RootName);
            if(old!=null)Object.DestroyImmediate(old);

            var root=new GameObject(RootName).transform;
            root.SetParent(parent,true);

            SuppressedShelfRenderers=0;
            SuppressFragmentedCliffShelves();
            BuildContinuousLandform(root);
        }

        static void SuppressFragmentedCliffShelves()
        {
            foreach(var r in Object.FindObjectsByType<Renderer>(FindObjectsSortMode.None))
            {
                if(r==null||!r.enabled||!r.gameObject.activeInHierarchy)continue;
                string chain=Chain(r.transform);

                bool shelf=
                    chain.Contains("valoria · cliff island · lower core")||
                    chain.Contains("valoria · cliff island · lower approach")||
                    chain.Contains("valoria · cliff island · middle west")||
                    chain.Contains("valoria · cliff island · middle east")||
                    chain.Contains("valoria · cliff island · middle central")||
                    chain.Contains("valoria · cliff island · upper bastion")||
                    chain.Contains("valoria · cliff island · edge");

                if(!shelf)continue;
                r.enabled=false;
                SuppressedShelfRenderers++;
            }
        }

        static void BuildContinuousLandform(Transform root)
        {
            const int nx=49;
            const int nz=43;
            const float minX=-11.7f;
            const float maxX=11.7f;
            const float minZ=-9.6f;
            const float maxZ=12.0f;

            var vertices=new Vector3[nx*nz];
            var uv=new Vector2[vertices.Length];
            var triangles=new int[(nx-1)*(nz-1)*6];

            for(int z=0;z<nz;z++)
            {
                float tz=z/(float)(nz-1);
                float wz=Mathf.Lerp(minZ,maxZ,tz);

                for(int x=0;x<nx;x++)
                {
                    float tx=x/(float)(nx-1);
                    float wx=Mathf.Lerp(minX,maxX,tx);

                    // Three continuous altitude bands corresponding to the existing L0 / middle / Bastion shelves.
                    float middle=Mathf.SmoothStep(0f,1f,Mathf.InverseLerp(.8f,4.3f,wz));
                    float upper=Mathf.SmoothStep(0f,1f,Mathf.InverseLerp(4.4f,7.5f,wz));
                    float y=.10f+middle*.88f+upper*1.30f;

                    // Keep the processional center slightly calmer/readable.
                    float centre=Mathf.Exp(-(wx*wx)*.045f);
                    y-=centre*(.10f+.10f*middle);

                    // Natural falloff creates one cliff-island silhouette instead of separate prefab shelves.
                    float side=Mathf.SmoothStep(0f,1f,Mathf.InverseLerp(7.5f,11.7f,Mathf.Abs(wx)));
                    float front=Mathf.SmoothStep(0f,1f,Mathf.InverseLerp(-6.4f,-9.6f,wz));
                    float rear=Mathf.SmoothStep(0f,1f,Mathf.InverseLerp(10.1f,12.0f,wz));
                    y-=side*1.45f+front*1.15f+rear*.45f;

                    float n1=(Mathf.PerlinNoise(wx*.15f+13.4f,wz*.14f+7.8f)-.5f)*.12f;
                    float n2=(Mathf.PerlinNoise(wx*.42f+3.2f,wz*.37f+19.1f)-.5f)*.04f;
                    y+=n1+n2;

                    int i=z*nx+x;
                    vertices[i]=new Vector3(wx,y,wz);
                    uv[i]=new Vector2((wx-minX)*.24f,(wz-minZ)*.24f);
                }
            }

            int ti=0;
            for(int z=0;z<nz-1;z++)
            {
                for(int x=0;x<nx-1;x++)
                {
                    int a=z*nx+x;
                    int b=a+1;
                    int d=(z+1)*nx+x;
                    int e=d+1;
                    triangles[ti++]=a;triangles[ti++]=d;triangles[ti++]=b;
                    triangles[ti++]=b;triangles[ti++]=d;triangles[ti++]=e;
                }
            }

            var mesh=new Mesh{name="Valoria Terrain Continuity v1 · compact landform"};
            mesh.indexFormat=IndexFormat.UInt32;
            mesh.vertices=vertices;
            mesh.uv=uv;
            mesh.triangles=triangles;
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();

            var go=new GameObject("Valoria · Terrain Continuity · compact landform");
            go.transform.SetParent(root,true);
            go.AddComponent<MeshFilter>().sharedMesh=mesh;

            var renderer=go.AddComponent<MeshRenderer>();
            renderer.sharedMaterial=
                ValoriaKit.ExternalPbrSurfaceMaterial(
                    "rock",new Color(.42f,.405f,.365f,1f),new Vector2(4.8f,4.6f),.022f,1.02f)
                ?? ValoriaKit.SurfaceMaterial(
                    new Color(.40f,.385f,.35f,1f),"stone",new Vector2(4.8f,4.6f));
            renderer.shadowCastingMode=ShadowCastingMode.On;
            renderer.receiveShadows=true;
        }

        static string Chain(Transform t)
        {
            string s="";
            for(var p=t;p!=null;p=p.parent)s+="|"+p.name.ToLowerInvariant();
            return s;
        }
    }
}
