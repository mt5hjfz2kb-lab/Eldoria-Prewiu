using System.Collections.Generic;
using Eldoria.Domain;
using UnityEngine;
using Object=UnityEngine.Object;

namespace Eldoria.Presentation
{
    // Visual-only authored terraced mesh family for the lower city.
    // It replaces both the broad TerrainData slab read and scattered floating-rock supports
    // while leaving all gameplay colliders/hotspots/routes untouched.
    public static class ValoriaLowerCityTerracedMeshV1
    {
        public static bool Enabled=false;
        public static int PiecesBuilt{get;private set;}

        const string RootName="Valoria · Lower City Terraced Mesh v1";

        public static void Build(Transform parent,PlayerState state)
        {
            if(!Enabled||parent==null)return;

            var old=GameObject.Find(RootName);
            if(old!=null)Object.DestroyImmediate(old);

            PiecesBuilt=0;
            var root=new GameObject(RootName).transform;
            root.SetParent(parent,true);

            var earth=ValoriaKit.ExternalPbrSurfaceMaterial(
                "dirt",new Color(.36f,.315f,.25f,1f),new Vector2(1.15f,1.15f),.016f,.90f)
                ?? ValoriaKit.DetailedSurfaceMaterial(new Color(.35f,.305f,.245f,1f),"earth",new Vector2(1.15f,1.15f),.90f);

            Material rock;
            var template=Resources.Load<Material>("Valoria/LowerCityWorldRock");
            if(template!=null)
            {
                rock=new Material(template){name="Valoria · terraced mesh world rock"};
                if(rock.HasProperty("_Color"))rock.SetColor("_Color",new Color(.41f,.42f,.39f,1f));
                if(rock.HasProperty("_Tiling"))rock.SetFloat("_Tiling",.080f);
                if(rock.HasProperty("_Smoothness"))rock.SetFloat("_Smoothness",.12f);
                if(rock.HasProperty("_Strength"))rock.SetFloat("_Strength",.76f);
            }
            else
            {
                rock=ValoriaKit.ExternalPbrSurfaceMaterial(
                    "rock",new Color(.38f,.39f,.37f,1f),new Vector2(.95f,.95f),.018f,.98f)
                    ?? ValoriaKit.DetailedSurfaceMaterial(new Color(.37f,.38f,.36f,1f),"stone",new Vector2(.95f,.95f),.98f);
            }

            // Central terrace: compact landing beneath the stair, not a full-width board.
            Create(root,"central landing",new[]{
                V(-4.25f,-2.05f),V(-2.95f,-2.65f),V(-1.35f,-2.35f),V(0f,-2.75f),
                V(1.40f,-2.38f),V(2.95f,-2.62f),V(4.30f,-2.00f),V(4.55f,-.65f),
                V(3.75f,.55f),V(2.15f,1.05f),V(.10f,.88f),V(-2.05f,1.02f),V(-3.75f,.52f),V(-4.55f,-.62f)
            },-.18f,-.72f,-1.42f,earth,rock);

            // West economic terrace, pulled inward and connected to the central landing.
            Create(root,"west economic",new[]{
                V(-9.00f,-4.45f),V(-7.55f,-5.10f),V(-5.85f,-4.75f),V(-4.55f,-3.85f),
                V(-4.15f,-2.55f),V(-4.65f,-1.45f),V(-5.85f,-.95f),V(-7.35f,-1.10f),V(-8.75f,-2.05f)
            },-.20f,-.82f,-1.48f,earth,rock);

            // East military terrace.
            Create(root,"east military",new[]{
                V(4.05f,-2.75f),V(4.65f,-4.25f),V(5.95f,-5.05f),V(7.65f,-5.15f),
                V(8.95f,-4.20f),V(8.90f,-2.75f),V(7.95f,-1.65f),V(6.35f,-1.28f),V(4.85f,-1.72f)
            },-.22f,-.84f,-1.50f,earth,rock);

        }

        static Vector2 V(float x,float z)=>new Vector2(x,z);

        static void Create(Transform root,string role,Vector2[] poly,float topY,float midY,float bottomY,Material earth,Material rock)
        {
            if(poly==null||poly.Length<3)return;

            var go=new GameObject("Valoria · terraced mesh · "+role);
            go.transform.SetParent(root,false);
            var mf=go.AddComponent<MeshFilter>();
            var mr=go.AddComponent<MeshRenderer>();
            mf.sharedMesh=BuildMesh(role,poly,topY,midY,bottomY);
            mr.sharedMaterials=new[]{earth,rock};
            PiecesBuilt++;
        }

        static Mesh BuildMesh(string role,Vector2[] poly,float topY,float midY,float bottomY)
        {
            int n=poly.Length;
            var centroid=Vector2.zero;
            for(int i=0;i<n;i++)centroid+=poly[i];
            centroid/=n;

            var verts=new List<Vector3>(1+n*3);
            var uvs=new List<Vector2>(1+n*3);
            var top=new List<int>(n*3);
            var sides=new List<int>(n*12);

            verts.Add(new Vector3(centroid.x,topY,centroid.y));
            uvs.Add(new Vector2(centroid.x*.12f,centroid.y*.12f));

            int topStart=verts.Count;
            for(int i=0;i<n;i++)
            {
                var p=poly[i];
                float noise=(Mathf.PerlinNoise((p.x+13f)*.23f,(p.y+17f)*.23f)-.5f)*.10f;
                verts.Add(new Vector3(p.x,topY+noise,p.y));
                uvs.Add(new Vector2(p.x*.12f,p.y*.12f));
            }

            int midStart=verts.Count;
            for(int i=0;i<n;i++)
            {
                var p=poly[i];
                var dir=(p-centroid).normalized;
                float irregular=.18f+(Mathf.PerlinNoise((p.x+29f)*.31f,(p.y+3f)*.31f)-.5f)*.16f;
                var q=p+dir*irregular;
                verts.Add(new Vector3(q.x,midY+(i%3-1)*.07f,q.y));
                uvs.Add(new Vector2(i/(float)n,.5f));
            }

            int botStart=verts.Count;
            for(int i=0;i<n;i++)
            {
                var p=poly[i];
                var dir=(p-centroid).normalized;
                float irregular=.48f+(Mathf.PerlinNoise((p.x+7f)*.19f,(p.y+41f)*.19f)-.5f)*.28f;
                var q=p+dir*irregular;
                verts.Add(new Vector3(q.x,bottomY+(i%4-1.5f)*.09f,q.y));
                uvs.Add(new Vector2(i/(float)n,0f));
            }

            for(int i=0;i<n;i++)
            {
                int a=topStart+i;
                int b=topStart+((i+1)%n);
                top.Add(0);top.Add(b);top.Add(a);

                int ma=midStart+i,mb=midStart+((i+1)%n);
                int ba=botStart+i,bb=botStart+((i+1)%n);

                sides.Add(a);sides.Add(b);sides.Add(mb);
                sides.Add(a);sides.Add(mb);sides.Add(ma);
                sides.Add(ma);sides.Add(mb);sides.Add(bb);
                sides.Add(ma);sides.Add(bb);sides.Add(ba);
            }

            var mesh=new Mesh{name="Valoria Lower City Terraced Mesh · "+role};
            mesh.indexFormat=UnityEngine.Rendering.IndexFormat.UInt32;
            mesh.SetVertices(verts);
            mesh.subMeshCount=2;
            mesh.SetTriangles(top,0);
            mesh.SetTriangles(sides,1);
            mesh.SetUVs(0,uvs);
            mesh.RecalculateNormals();
            mesh.RecalculateTangents();
            mesh.RecalculateBounds();
            return mesh;
        }
    }
}
