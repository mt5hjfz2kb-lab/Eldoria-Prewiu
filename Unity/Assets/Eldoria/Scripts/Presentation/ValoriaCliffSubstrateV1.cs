using System;
using Eldoria.Domain;
using UnityEngine;
using UnityEngine.Rendering;
using Object=UnityEngine.Object;

namespace Eldoria.Presentation
{
    // Replaces disconnected edge-rock presentation with one continuous faceted cliff substrate.
    // Pure presentation: gameplay floors, colliders, routes, hotspots and reserved plots remain authoritative.
    public static class ValoriaCliffSubstrateV1
    {
        public static bool Enabled=false;
        const string RootName="Valoria · Cliff Substrate v1";
        public static int SuppressedFragments{get;private set;}
        public static int CliffSegments{get;private set;}

        public static void Build(Transform parent,PlayerState state)
        {
            if(!Enabled||parent==null)return;
            var old=GameObject.Find(RootName);
            if(old!=null)Object.DestroyImmediate(old);

            var root=new GameObject(RootName).transform;
            root.SetParent(parent,true);

            SuppressedFragments=0;
            CliffSegments=0;
            SuppressDisconnectedEdgePresentation();
            BuildOuterCliff(root);
            BuildUpperRetainingCliffs(root);
        }

        static void SuppressDisconnectedEdgePresentation()
        {
            foreach(var r in Object.FindObjectsByType<Renderer>(FindObjectsSortMode.None))
            {
                if(r==null||!r.enabled||!r.gameObject.activeInHierarchy)continue;
                string chain=Chain(r.transform);
                bool fragment=
                    chain.Contains("valoria · foreground edge")||
                    chain.Contains("valoria · cliff island · edge")||
                    chain.Contains("valoria · residual cleanup · rock");
                if(!fragment)continue;
                r.enabled=false;
                SuppressedFragments++;
            }
        }

        static void BuildOuterCliff(Transform root)
        {
            // Clockwise perimeter of the compact city. Top heights follow the existing tier silhouette.
            var p=new[]{
                new Vector3(-9.0f,.15f,-6.7f),
                new Vector3(-5.7f,.10f,-8.5f),
                new Vector3(-1.8f,.08f,-9.2f),
                new Vector3( 2.2f,.08f,-9.2f),
                new Vector3( 6.0f,.12f,-8.4f),
                new Vector3( 9.0f,.18f,-6.5f),
                new Vector3(10.0f,.38f,-2.1f),
                new Vector3( 9.2f,.72f, 2.6f),
                new Vector3( 7.7f,1.35f, 5.5f),
                new Vector3( 6.1f,2.15f, 8.1f),
                new Vector3( 3.8f,2.45f,10.0f),
                new Vector3(-3.8f,2.45f,10.0f),
                new Vector3(-6.2f,2.12f, 8.0f),
                new Vector3(-7.8f,1.32f, 5.4f),
                new Vector3(-9.3f,.70f, 2.5f),
                new Vector3(-10.0f,.36f,-2.2f)
            };

            var mat=ValoriaKit.ExternalPbrSurfaceMaterial(
                "rock",new Color(.34f,.33f,.30f,1f),new Vector2(3.6f,3.6f),.018f,1.06f)
                ?? ValoriaKit.SurfaceMaterial(new Color(.33f,.32f,.29f,1f),"stone",new Vector2(3.6f,3.6f));

            for(int i=0;i<p.Length;i++)
            {
                var a=p[i];
                var b=p[(i+1)%p.Length];
                float wobbleA=((i*37)%7-.3f)*.10f;
                float wobbleB=(((i+1)*37)%7-.3f)*.10f;
                float bottomA=-2.65f-(i%3)*.20f+wobbleA;
                float bottomB=-2.65f-((i+1)%3)*.20f+wobbleB;
                AddFacetedWall(root,"outer "+i,a,b,bottomA,bottomB,mat,i);
            }
        }

        static void BuildUpperRetainingCliffs(Transform root)
        {
            var mat=ValoriaKit.ExternalPbrSurfaceMaterial(
                "rock",new Color(.37f,.35f,.31f,1f),new Vector2(3.0f,3.0f),.020f,1.05f)
                ?? ValoriaKit.SurfaceMaterial(new Color(.35f,.34f,.30f,1f),"stone",new Vector2(3.0f,3.0f));

            var bands=new[]{
                new[]{new Vector3(-6.0f,1.40f,4.1f),new Vector3(-3.2f,1.55f,5.0f),new Vector3(-1.0f,1.62f,5.2f)},
                new[]{new Vector3( 1.0f,1.62f,5.2f),new Vector3( 3.2f,1.55f,5.0f),new Vector3( 6.0f,1.40f,4.1f)}
            };
            int seed=50;
            foreach(var band in bands)
            {
                for(int i=0;i<band.Length-1;i++)
                    AddFacetedWall(root,"upper "+seed,band[i],band[i+1],.38f,.38f,mat,seed++);
            }
        }

        static void AddFacetedWall(Transform root,string role,Vector3 a,Vector3 b,float bottomA,float bottomB,Material mat,int seed)
        {
            // Split every span into 3 independent quads so normals stay faceted instead of reading like a smooth sheet.
            const int divisions=3;
            for(int d=0;d<divisions;d++)
            {
                float t0=d/(float)divisions;
                float t1=(d+1)/(float)divisions;
                var top0=Vector3.Lerp(a,b,t0);
                var top1=Vector3.Lerp(a,b,t1);
                float ba=Mathf.Lerp(bottomA,bottomB,t0);
                float bb=Mathf.Lerp(bottomA,bottomB,t1);

                float inset0=.10f*Mathf.Sin((seed*3+d)*1.73f);
                float inset1=.10f*Mathf.Sin((seed*3+d+1)*1.73f);
                top0.y+=inset0;
                top1.y+=inset1;

                var verts=new[]{
                    top0,
                    top1,
                    new Vector3(top0.x,ba,top0.z),
                    new Vector3(top1.x,bb,top1.z)
                };
                var mesh=new Mesh{name="Valoria Cliff Substrate · "+role+"."+d};
                mesh.vertices=verts;
                mesh.uv=new[]{new Vector2(0,1),new Vector2(1,1),new Vector2(0,0),new Vector2(1,0)};
                mesh.triangles=new[]{0,2,1,1,2,3};
                mesh.RecalculateNormals();
                mesh.RecalculateBounds();

                var go=new GameObject("Valoria · Cliff Substrate · "+role+"."+d);
                go.transform.SetParent(root,true);
                go.AddComponent<MeshFilter>().sharedMesh=mesh;
                var mr=go.AddComponent<MeshRenderer>();
                mr.sharedMaterial=mat;
                mr.shadowCastingMode=ShadowCastingMode.On;
                mr.receiveShadows=true;
                CliffSegments++;
            }
        }

        static string Chain(Transform t)
        {
            string s="";
            for(var p=t;p!=null;p=p.parent)s+="|"+p.name.ToLowerInvariant();
            return s;
        }
    }
}
