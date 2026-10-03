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

            var rockTemplate=Resources.Load<Material>("Valoria/LowerCityWorldRock");
            Material rock;
            if(rockTemplate!=null)
            {
                rock=new Material(rockTemplate){name="Valoria · lower-city world-space rock"};
                if(rock.HasProperty("_Color"))rock.SetColor("_Color",new Color(.42f,.43f,.40f,1f));
                if(rock.HasProperty("_Tiling"))rock.SetFloat("_Tiling",.085f);
                if(rock.HasProperty("_Smoothness"))rock.SetFloat("_Smoothness",.14f);
                if(rock.HasProperty("_Strength"))rock.SetFloat("_Strength",.72f);
            }
            else
            {
                rock=ValoriaKit.ExternalPbrSurfaceMaterial(
                    "rock",new Color(.35f,.36f,.34f,1f),new Vector2(.95f,.95f),.018f,1.00f)
                    ?? ValoriaKit.DetailedSurfaceMaterial(new Color(.34f,.35f,.33f,1f),"stone",new Vector2(.95f,.95f),1.00f);
            }

            mr.sharedMaterials=new[]{rock};

            var causeway=new GameObject("Valoria · Lower City Plateau · narrow approach spine");
            causeway.transform.SetParent(root,false);
            var cmf=causeway.AddComponent<MeshFilter>();
            var cmr=causeway.AddComponent<MeshRenderer>();
            cmf.sharedMesh=BuildCausewayMesh();
            cmr.sharedMaterials=new[]{earth,rock};

            foreach(var c in go.GetComponentsInChildren<Collider>(true))c.enabled=false;
            foreach(var c in causeway.GetComponentsInChildren<Collider>(true))c.enabled=false;
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
            // Compact city substrate only. The old single mesh reached the bottom of the
            // frame and read as a technical slab; the approach is now a separate narrow spine.
            const int nx=76;
            const int nz=54;
            const float minX=-9.45f,maxX=9.45f;
            const float minZ=-4.75f,maxZ=6.20f;

            var vertices=new List<Vector3>((nx+1)*(nz+1));
            var uvs=new List<Vector2>((nx+1)*(nz+1));
            var triangles=new List<int>();

            for(int z=0;z<=nz;z++)
            {
                float vz=z/(float)nz;
                float wz=Mathf.Lerp(minZ,maxZ,vz);
                for(int x=0;x<=nx;x++)
                {
                    float vx=x/(float)nx;
                    float wx=Mathf.Lerp(minX,maxX,vx);

                    float ex=Mathf.Abs(wx)/9.05f;
                    float ez=Mathf.Abs((wz+.15f))/6.15f;
                    float d=Mathf.Pow(Mathf.Pow(ex,1.68f)+Mathf.Pow(ez,1.68f),1f/1.68f);

                    float n1=Mathf.PerlinNoise((wx+17.3f)*.17f,(wz+11.7f)*.17f)-.5f;
                    float n2=Mathf.PerlinNoise((wx-4.1f)*.37f,(wz+23.5f)*.37f)-.5f;
                    float n3=Mathf.PerlinNoise((wx+31.2f)*.09f,(wz-7.4f)*.09f)-.5f;
                    float top=-.22f+n1*.16f+n2*.05f;
                    float fall=Mathf.SmoothStep(0f,1f,Mathf.InverseLerp(.58f,.98f,d));
                    float bottom=-3.15f+n1*.44f+n3*.27f;
                    float y=Mathf.Lerp(top,bottom,fall);

                    float route=Mathf.Clamp01(1f-Mathf.Abs(wx)/1.75f)*
                                Mathf.Clamp01(1f-Mathf.Abs(wz+1.0f)/5.5f);

                    float pad=0f;
                    pad=Mathf.Max(pad,Disc(wx,wz,-7.15f,-2.65f,2.10f));
                    pad=Mathf.Max(pad,Disc(wx,wz, 6.65f,-3.80f,2.10f));
                    pad=Mathf.Max(pad,Disc(wx,wz,-5.15f, 2.55f,2.25f));
                    pad=Mathf.Max(pad,Disc(wx,wz, 5.10f, 2.75f,2.25f));
                    pad=Mathf.Max(pad,Disc(wx,wz,-3.95f, 5.05f,2.00f));
                    pad=Mathf.Max(pad,Disc(wx,wz, 3.95f, 5.10f,2.00f));

                    float support=Mathf.Max(route*.84f,pad*.66f);
                    y=Mathf.Lerp(y,Mathf.Max(y,-.16f),support*(1f-fall*.82f));

                    vertices.Add(new Vector3(wx,y,wz));
                    uvs.Add(new Vector2(wx*.10f,wz*.10f));
                }
            }

            int Row(int z)=>z*(nx+1);
            for(int z=0;z<nz;z++)
            for(int x=0;x<nx;x++)
            {
                int i0=Row(z)+x,i1=i0+1,i2=Row(z+1)+x,i3=i2+1;
                triangles.Add(i0);triangles.Add(i3);triangles.Add(i1);
                triangles.Add(i0);triangles.Add(i2);triangles.Add(i3);
            }

            var mesh=new Mesh{name="Valoria Lower City Compact Terrain v12"};
            mesh.indexFormat=UnityEngine.Rendering.IndexFormat.UInt32;
            mesh.SetVertices(vertices);
            mesh.SetTriangles(triangles,0);
            mesh.SetUVs(0,uvs);
            mesh.RecalculateNormals();
            mesh.RecalculateTangents();
            mesh.RecalculateBounds();
            return mesh;
        }

        static Mesh BuildCausewayMesh()
        {
            // Irregular top ribbon with real side faces. It supports the authored cobble
            // approach without filling the whole foreground with rock.
            const int segments=9;
            const float z0=-7.75f,z1=-4.20f;
            const float bottom=-2.65f;
            var vertices=new List<Vector3>();
            var uvs=new List<Vector2>();
            var topTris=new List<int>();
            var sideTris=new List<int>();

            for(int i=0;i<=segments;i++)
            {
                float t=i/(float)segments;
                float z=Mathf.Lerp(z0,z1,t);
                float wobble=(Mathf.PerlinNoise(7.3f,t*2.7f)-.5f)*.28f;
                float half=Mathf.Lerp(1.28f,1.72f,t)+wobble;
                float y=Mathf.Lerp(-.42f,-.18f,t)+(Mathf.PerlinNoise(2.1f,t*3.4f)-.5f)*.06f;

                vertices.Add(new Vector3(-half,y,z));
                vertices.Add(new Vector3( half,y,z));
                vertices.Add(new Vector3(-half*.94f,bottom,z));
                vertices.Add(new Vector3( half*.94f,bottom,z));
                uvs.Add(new Vector2(0f,t*2.4f));
                uvs.Add(new Vector2(1f,t*2.4f));
                uvs.Add(new Vector2(0f,t));
                uvs.Add(new Vector2(1f,t));
            }

            for(int i=0;i<segments;i++)
            {
                int a=i*4,b=a+1,bl=a+2,br=a+3;
                int n=(i+1)*4,nb=n+1,nbl=n+2,nbr=n+3;

                topTris.Add(a);topTris.Add(nb);topTris.Add(b);
                topTris.Add(a);topTris.Add(n);topTris.Add(nb);

                sideTris.Add(a);sideTris.Add(nbl);sideTris.Add(n);
                sideTris.Add(a);sideTris.Add(bl);sideTris.Add(nbl);
                sideTris.Add(b);sideTris.Add(nb);sideTris.Add(nbr);
                sideTris.Add(b);sideTris.Add(nbr);sideTris.Add(br);
            }

            var mesh=new Mesh{name="Valoria Lower City Approach Spine v1"};
            mesh.SetVertices(vertices);
            mesh.subMeshCount=2;
            mesh.SetTriangles(topTris,0);
            mesh.SetTriangles(sideTris,1);
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
