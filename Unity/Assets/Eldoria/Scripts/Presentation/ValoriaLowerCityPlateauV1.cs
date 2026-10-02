using System.Collections.Generic;
using Eldoria.Domain;
using UnityEngine;
using Object=UnityEngine.Object;

namespace Eldoria.Presentation
{
    // Visual-only coherent lower-city substrate. Replaces fragmented prototype support
    // renderers with one continuous authored plateau while preserving gameplay topology.
    public static class ValoriaLowerCityPlateauV1
    {
        public static bool Enabled=false;
        public static int SuppressedFragmentRenderers{get;private set;}

        const string RootName="Valoria · Lower City Plateau v1";

        public static void Build(Transform parent,PlayerState state)
        {
            if(!Enabled||parent==null)return;

            var old=GameObject.Find(RootName);
            if(old!=null)Object.DestroyImmediate(old);

            SuppressedFragmentRenderers=SuppressFragmentedSupports();

            var root=new GameObject(RootName).transform;
            root.SetParent(parent,true);

            var mesh=BuildPlateauMesh();
            var go=new GameObject("Valoria · Lower City Plateau · continuous substrate");
            go.transform.SetParent(root,false);
            var mf=go.AddComponent<MeshFilter>();
            var mr=go.AddComponent<MeshRenderer>();
            mf.sharedMesh=mesh;

            // Use Eldoria's known-good seamless PBR sources. The authored Slavic prefab
            // materials are atlas-mapped for their own meshes and produce catastrophic
            // checkerboard/atlas reads on this generated substrate.
            var earth=ValoriaKit.ExternalPbrSurfaceMaterial(
                "dirt",new Color(.39f,.345f,.27f,1f),new Vector2(1.05f,1.05f),.018f,.90f)
                ?? ValoriaKit.DetailedSurfaceMaterial(new Color(.38f,.335f,.265f,1f),"earth",new Vector2(1.05f,1.05f),.90f);

            var rock=ValoriaKit.ExternalPbrSurfaceMaterial(
                "rock",new Color(.35f,.36f,.34f,1f),new Vector2(.95f,.95f),.018f,1.00f)
                ?? ValoriaKit.DetailedSurfaceMaterial(new Color(.34f,.35f,.33f,1f),"stone",new Vector2(.95f,.95f),1.00f);

            mr.sharedMaterials=new[]{earth,rock};

            foreach(var c in go.GetComponentsInChildren<Collider>(true))c.enabled=false;
        }

        static int SuppressFragmentedSupports()
        {
            int count=0;
            foreach(var r in Object.FindObjectsByType<Renderer>(FindObjectsSortMode.None))
            {
                if(r==null||!r.enabled||!r.gameObject.activeInHierarchy)continue;
                var b=r.bounds;
                string chain=Chain(r.transform);

                if(chain.Contains("backplate")||
                   chain.Contains("bastion hero")||
                   chain.Contains("bastion · dedicated")||
                   chain.Contains("aserradero")||
                   chain.Contains("cuartel")||
                   chain.Contains("granary")||
                   chain.Contains("granero")||
                   chain.Contains("main street")||
                   chain.Contains("street slab")||
                   chain.Contains("processional")||
                   chain.Contains("premium secondary"))
                    continue;

                bool lower=b.center.y<1.35f && b.center.z<5.5f;
                if(!lower)continue;

                bool fragmented=
                    chain.Contains("lower cliff authored rock")||
                    chain.Contains("foreground edge")||
                    chain.Contains("terrainterrace")||
                    chain.Contains("expansion edge geology")||
                    chain.Contains("environment uplift · rock")||
                    chain.Contains("cliff island · lower")||
                    chain.Contains("cliff island · middle")||
                    chain.Contains("assetlibrary reprocessing · hero lower terrace")||
                    chain.Contains("broadrockplatform")||
                    chain.Contains("steppedrockterrace");

                if(fragmented)
                {
                    r.enabled=false;
                    count++;
                }
            }
            return count;
        }

        static Mesh BuildPlateauMesh()
        {
            const int nx=40;
            const int nz=38;
            const float minX=-9.65f,maxX=9.65f;
            const float minZ=-9.9f,maxZ=6.15f;

            var vertices=new List<Vector3>((nx+1)*(nz+1));
            var uvs=new List<Vector2>((nx+1)*(nz+1));
            var innerTriangles=new List<int>();
            var slopeTriangles=new List<int>();

            for(int z=0;z<=nz;z++)
            {
                float vz=z/(float)nz;
                float wz=Mathf.Lerp(minZ,maxZ,vz);
                for(int x=0;x<=nx;x++)
                {
                    float vx=x/(float)nx;
                    float wx=Mathf.Lerp(minX,maxX,vx);

                    float ex=Mathf.Abs(wx)/9.25f;
                    float ez=Mathf.Abs((wz+2.05f))/7.55f;
                    float d=Mathf.Pow(Mathf.Pow(ex,1.72f)+Mathf.Pow(ez,1.72f),1f/1.72f);

                    float noiseA=Mathf.PerlinNoise((wx+17.3f)*.18f,(wz+11.7f)*.18f)-.5f;
                    float noiseB=Mathf.PerlinNoise((wx-4.1f)*.41f,(wz+23.5f)*.41f)-.5f;
                    float noiseC=Mathf.PerlinNoise((wx+31.2f)*.095f,(wz-7.4f)*.095f)-.5f;
                    float top=-.22f + noiseA*.18f + noiseB*.065f;

                    float edgeNoise=(Mathf.PerlinNoise((wz+20f)*.13f,(wx+14f)*.13f)-.5f)*.10f;
                    float edgeStart=.50f+edgeNoise+noiseC*.055f;
                    float fall=Mathf.InverseLerp(edgeStart,.96f,d);
                    fall=fall*fall*(3f-2f*fall);
                    float erodedBottom=-3.55f+noiseA*.52f+noiseC*.34f;
                    float y=Mathf.Lerp(top,erodedBottom,fall);

                    float route=Mathf.Clamp01(1f-Mathf.Abs(wx)/2.35f)*
                                Mathf.Clamp01(1f-Mathf.Abs(wz+1.9f)/6.6f);

                    float pad=0f;
                    pad=Mathf.Max(pad,Disc(wx,wz,-6.2f,-4.4f,3.05f));
                    pad=Mathf.Max(pad,Disc(wx,wz, 6.2f,-4.5f,3.05f));
                    pad=Mathf.Max(pad,Disc(wx,wz,-5.2f, 2.5f,2.65f));
                    pad=Mathf.Max(pad,Disc(wx,wz, 5.1f, 2.6f,2.65f));
                    pad=Mathf.Max(pad,Disc(wx,wz,-3.9f, 5.0f,2.25f));
                    pad=Mathf.Max(pad,Disc(wx,wz, 3.9f, 5.0f,2.25f));

                    float support=Mathf.Max(route*.92f,pad*.78f);
                    y=Mathf.Lerp(y,Mathf.Max(y,-.16f),support*(1f-fall*.76f));

                    vertices.Add(new Vector3(wx,y,wz));
                    Vector2 uv;
                    if(d>.47f)
                    {
                        // Vertical projection on eroded skirts avoids the roof/carpet stretch
                        // caused by XZ-only UVs on steep terrain faces.
                        uv=ex>=ez
                            ?new Vector2(wz*.155f,(y+3.8f)*.36f)
                            :new Vector2(wx*.155f,(y+3.8f)*.36f);
                    }
                    else uv=new Vector2(wx*.105f,wz*.105f);
                    uvs.Add(uv);
                }
            }

            int Row(int z)=>z*(nx+1);
            for(int z=0;z<nz;z++)
            for(int x=0;x<nx;x++)
            {
                int i0=Row(z)+x;
                int i1=i0+1;
                int i2=Row(z+1)+x;
                int i3=i2+1;

                var center=(vertices[i0]+vertices[i1]+vertices[i2]+vertices[i3])*.25f;
                float ex=Mathf.Abs(center.x)/9.25f;
                float ez=Mathf.Abs((center.z+2.05f))/7.55f;
                float d=Mathf.Pow(Mathf.Pow(ex,1.72f)+Mathf.Pow(ez,1.72f),1f/1.72f);
                float route=Mathf.Clamp01(1f-Mathf.Abs(center.x)/2.35f)*
                            Mathf.Clamp01(1f-Mathf.Abs(center.z+1.9f)/6.6f);
                bool inner=d<.49f||route>.42f;
                var target=inner?innerTriangles:slopeTriangles;

                target.Add(i0);target.Add(i3);target.Add(i1);
                target.Add(i0);target.Add(i2);target.Add(i3);
            }

            var mesh=new Mesh{name="Valoria Lower City Organic Terrain v4"};
            mesh.indexFormat=UnityEngine.Rendering.IndexFormat.UInt32;
            mesh.SetVertices(vertices);
            mesh.subMeshCount=2;
            mesh.SetTriangles(innerTriangles,0);
            mesh.SetTriangles(slopeTriangles,1);
            mesh.SetUVs(0,uvs);
            mesh.RecalculateNormals();
            mesh.RecalculateTangents();
            mesh.RecalculateBounds();
            return mesh;
        }

        static float Disc(float x,float z,float cx,float cz,float radius)
        {
            float dx=x-cx,dz=z-cz;
            return Mathf.Clamp01(1f-Mathf.Sqrt(dx*dx+dz*dz)/radius);
        }

        static string Chain(Transform t)
        {
            string s="";
            for(var p=t;p!=null;p=p.parent)s+="|"+p.name.ToLowerInvariant();
            return s;
        }
    }
}
