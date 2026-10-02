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

            var art=ValoriaExternalAssetLibrary.Load();
            var mudSource=art!=null&&art.SlavicMudFlat!=null
                ?art.SlavicMudFlat.GetComponentInChildren<Renderer>(true)?.sharedMaterial
                :art?.ValoriaDirtSurface;
            var rockSource=art!=null&&art.SlavicFlatRock!=null
                ?art.SlavicFlatRock.GetComponentInChildren<Renderer>(true)?.sharedMaterial
                :art?.ValoriaStoneSurface;

            var earth=ValoriaKit.PbrSurfaceMaterial(
                mudSource,new Color(.72f,.68f,.58f,1f),new Vector2(5.2f,5.2f),.018f,.95f);
            var rock=ValoriaKit.PbrSurfaceMaterial(
                rockSource,new Color(.70f,.69f,.64f,1f),new Vector2(4.4f,4.4f),.022f,1.08f);

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
            const int nx=36;
            const int nz=34;
            const float minX=-10.2f,maxX=10.2f;
            const float minZ=-10.8f,maxZ=6.4f;

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

                    float ex=Mathf.Abs(wx)/10.0f;
                    float ez=Mathf.Abs((wz+2.15f))/8.35f;
                    float d=Mathf.Max(ex,ez);

                    float noiseA=Mathf.PerlinNoise((wx+17.3f)*.18f,(wz+11.7f)*.18f)-.5f;
                    float noiseB=Mathf.PerlinNoise((wx-4.1f)*.41f,(wz+23.5f)*.41f)-.5f;
                    float top=-.20f + noiseA*.16f + noiseB*.055f;

                    float edgeStart=.66f + (Mathf.PerlinNoise((wz+20f)*.13f,(wx+14f)*.13f)-.5f)*.07f;
                    float fall=Mathf.InverseLerp(edgeStart,1.02f,d);
                    fall=fall*fall*(3f-2f*fall);
                    float y=Mathf.Lerp(top,-2.75f+noiseA*.42f,fall);

                    float route=Mathf.Clamp01(1f-Mathf.Abs(wx)/2.9f)*
                                Mathf.Clamp01(1f-Mathf.Abs(wz+1.8f)/7.0f);
                    y=Mathf.Lerp(y,Mathf.Max(y,-.16f),route*.72f*(1f-fall));

                    vertices.Add(new Vector3(wx,y,wz));
                    uvs.Add(new Vector2(vx*4f,vz*3.5f));
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
                float ex=Mathf.Abs(center.x)/10.0f;
                float ez=Mathf.Abs((center.z+2.15f))/8.35f;
                bool inner=Mathf.Max(ex,ez)<.69f;
                var target=inner?innerTriangles:slopeTriangles;

                target.Add(i0);target.Add(i3);target.Add(i1);
                target.Add(i0);target.Add(i2);target.Add(i3);
            }

            var mesh=new Mesh{name="Valoria Lower City Organic Terrain v2"};
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

        static string Chain(Transform t)
        {
            string s="";
            for(var p=t;p!=null;p=p.parent)s+="|"+p.name.ToLowerInvariant();
            return s;
        }
    }
}
