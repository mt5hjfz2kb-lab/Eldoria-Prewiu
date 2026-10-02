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

            var earth=ValoriaKit.ExternalPbrSurfaceMaterial(
                "dirt",new Color(.30f,.265f,.21f,1f),new Vector2(3.6f,3.6f),.016f,.96f)
                ?? ValoriaKit.DetailedSurfaceMaterial(new Color(.30f,.265f,.21f,1f),"earth",new Vector2(3.6f,3.6f),.96f);

            var rock=ValoriaKit.ExternalPbrSurfaceMaterial(
                "rock",new Color(.31f,.31f,.285f,1f),new Vector2(3.0f,3.0f),.018f,1.02f)
                ?? ValoriaKit.DetailedSurfaceMaterial(new Color(.31f,.31f,.285f,1f),"stone",new Vector2(3.0f,3.0f),1.02f);

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
            var ring=new[]{
                new Vector2(-9.4f,-5.8f),
                new Vector2(-7.0f,-8.4f),
                new Vector2(-3.5f,-9.7f),
                new Vector2(0f,-10.2f),
                new Vector2(3.5f,-9.7f),
                new Vector2(7.0f,-8.4f),
                new Vector2(9.4f,-5.8f),
                new Vector2(9.8f,-2.0f),
                new Vector2(9.3f,2.3f),
                new Vector2(7.5f,4.9f),
                new Vector2(4.3f,5.7f),
                new Vector2(0f,6.1f),
                new Vector2(-4.3f,5.7f),
                new Vector2(-7.5f,4.9f),
                new Vector2(-9.3f,2.3f),
                new Vector2(-9.8f,-2.0f)
            };

            const float topY=-.18f;
            const float bottomY=-2.35f;

            var vertices=new List<Vector3>();
            var uvs=new List<Vector2>();
            vertices.Add(new Vector3(0f,topY,-1.1f));
            uvs.Add(new Vector2(.5f,.5f));

            for(int i=0;i<ring.Length;i++)
            {
                vertices.Add(new Vector3(ring[i].x,topY,ring[i].y));
                uvs.Add(new Vector2((ring[i].x+10f)/20f,(ring[i].y+10.5f)/17f));
            }

            int bottomStart=vertices.Count;
            for(int i=0;i<ring.Length;i++)
            {
                float variation=(i%3==0)?.32f:(i%3==1?-.18f:.08f);
                vertices.Add(new Vector3(ring[i].x*.94f,bottomY+variation,ring[i].y*.94f));
                uvs.Add(new Vector2(i/(float)ring.Length,0f));
            }

            var topTriangles=new List<int>();
            for(int i=0;i<ring.Length;i++)
            {
                int a=1+i;
                int b=1+((i+1)%ring.Length);
                topTriangles.Add(0);topTriangles.Add(b);topTriangles.Add(a);
            }

            var sideTriangles=new List<int>();
            for(int i=0;i<ring.Length;i++)
            {
                int ti=1+i;
                int tn=1+((i+1)%ring.Length);
                int bi=bottomStart+i;
                int bn=bottomStart+((i+1)%ring.Length);
                sideTriangles.Add(ti);sideTriangles.Add(tn);sideTriangles.Add(bn);
                sideTriangles.Add(ti);sideTriangles.Add(bn);sideTriangles.Add(bi);
            }

            var mesh=new Mesh{name="Valoria Lower City Plateau v1"};
            mesh.SetVertices(vertices);
            mesh.subMeshCount=2;
            mesh.SetTriangles(topTriangles,0);
            mesh.SetTriangles(sideTriangles,1);
            mesh.SetUVs(0,uvs);
            mesh.RecalculateNormals();
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
