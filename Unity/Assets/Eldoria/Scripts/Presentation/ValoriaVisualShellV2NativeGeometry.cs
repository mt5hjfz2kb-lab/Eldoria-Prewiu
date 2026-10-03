using System.Collections.Generic;
using Eldoria.Domain;
using UnityEngine;
using Object=UnityEngine.Object;

namespace Eldoria.Presentation
{
    // Distinct Visual Shell v2 method: authored native 3D landform masses.
    // No screen-space/photo foreground and no procedural heightfield. Gameplay stays authoritative underneath.
    public static class ValoriaVisualShellV2NativeGeometry
    {
        public static bool Enabled=true;
        public static int PiecesBuilt{get;private set;}
        const string RootName="Valoria · Visual Shell v2 · Native Geometry";

        public static void Build(Transform parent,PlayerState state)
        {
            if(!Enabled||parent==null)return;
            var old=GameObject.Find(RootName);
            if(old!=null)Object.DestroyImmediate(old);

            PiecesBuilt=0;
            var root=new GameObject(RootName).transform;
            root.SetParent(parent,true);

            var top=ValoriaKit.ExternalPbrSurfaceMaterial(
                "dirt",new Color(.39f,.35f,.29f,1f),new Vector2(1.1f,1.1f),.025f,.92f)
                ?? ValoriaKit.DetailedSurfaceMaterial(new Color(.38f,.34f,.28f,1f),"earth",new Vector2(1.1f,1.1f),.9f);
            var cliff=ValoriaKit.ExternalPbrSurfaceMaterial(
                "rock",new Color(.36f,.37f,.35f,1f),new Vector2(.86f,.86f),.06f,1.02f)
                ?? ValoriaKit.DetailedSurfaceMaterial(new Color(.35f,.36f,.34f,1f),"stone",new Vector2(.86f,.86f),1.05f);

            // One authored crag around the hero district plus two compact functional shoulders.
            BuildCrag(root,"hero crag",new Vector3(0f,0f,4.10f),new[]{
                new Vector2(-4.95f,-2.15f),new Vector2(-3.55f,-3.25f),new Vector2(-1.55f,-3.65f),
                new Vector2(.15f,-3.35f),new Vector2(2.15f,-3.65f),new Vector2(4.05f,-2.85f),
                new Vector2(5.05f,-1.35f),new Vector2(5.15f,.75f),new Vector2(4.25f,2.55f),
                new Vector2(2.45f,3.55f),new Vector2(.25f,3.85f),new Vector2(-2.10f,3.55f),
                new Vector2(-4.15f,2.55f),new Vector2(-5.20f,.70f)
            },.58f,-2.20f,top,cliff);

            BuildCrag(root,"west functional shoulder",new Vector3(-5.55f,0f,-1.95f),new[]{
                new Vector2(-2.15f,-1.55f),new Vector2(-.65f,-2.15f),new Vector2(1.05f,-1.90f),
                new Vector2(2.05f,-.65f),new Vector2(1.85f,.95f),new Vector2(.55f,1.75f),
                new Vector2(-1.25f,1.55f),new Vector2(-2.35f,.35f)
            },.08f,-1.55f,top,cliff);

            BuildCrag(root,"east functional shoulder",new Vector3(5.55f,0f,-2.65f),new[]{
                new Vector2(-2.15f,-1.25f),new Vector2(-.75f,-2.00f),new Vector2(.95f,-1.85f),
                new Vector2(2.15f,-.45f),new Vector2(1.85f,1.10f),new Vector2(.45f,1.85f),
                new Vector2(-1.35f,1.45f),new Vector2(-2.30f,.25f)
            },.02f,-1.65f,top,cliff);

            AddRockSeams(root);
            AddRetainingConnectors(root);

            foreach(var c in root.GetComponentsInChildren<Collider>(true))Object.DestroyImmediate(c);
            foreach(var h in root.GetComponentsInChildren<WorldHotspot>(true))Object.DestroyImmediate(h);
        }

        static void BuildCrag(Transform parent,string name,Vector3 origin,Vector2[] contour,
            float topY,float bottomY,Material top,Material cliff)
        {
            int n=contour.Length;
            var verts=new Vector3[1+n+n];
            var uv=new Vector2[verts.Length];
            verts[0]=origin+Vector3.up*topY;
            uv[0]=new Vector2(.5f,.5f);
            for(int i=0;i<n;i++)
            {
                float wobble=.08f*Mathf.Sin(i*1.91f)+.045f*Mathf.Sin(i*3.17f+.7f);
                verts[1+i]=origin+new Vector3(contour[i].x,topY+wobble,contour[i].y);
                float drop=bottomY-.16f*(i%3)-.09f*Mathf.Sin(i*2.2f);
                verts[1+n+i]=origin+new Vector3(contour[i].x*1.04f,drop,contour[i].y*1.04f);
                uv[1+i]=new Vector2(contour[i].x*.11f+.5f,contour[i].y*.11f+.5f);
                uv[1+n+i]=new Vector2(i/(float)n,0f);
            }

            var topTris=new List<int>(n*3);
            var sideTris=new List<int>(n*6);
            for(int i=0;i<n;i++)
            {
                int next=(i+1)%n;
                topTris.Add(0); topTris.Add(1+next); topTris.Add(1+i);
                int a=1+i,b=1+next,c=1+n+i,d=1+n+next;
                sideTris.Add(a);sideTris.Add(b);sideTris.Add(c);
                sideTris.Add(b);sideTris.Add(d);sideTris.Add(c);
            }

            var mesh=new Mesh{name="Valoria Native Crag · "+name};
            mesh.vertices=verts;mesh.uv=uv;mesh.subMeshCount=2;
            mesh.SetTriangles(topTris,0);mesh.SetTriangles(sideTris,1);
            mesh.RecalculateNormals();mesh.RecalculateTangents();mesh.RecalculateBounds();

            var go=new GameObject("Valoria · Native crag · "+name);
            go.transform.SetParent(parent,true);
            go.AddComponent<MeshFilter>().sharedMesh=mesh;
            go.AddComponent<MeshRenderer>().sharedMaterials=new[]{top,cliff};
            PiecesBuilt++;
        }

        static void AddRockSeams(Transform root)
        {
            var art=ValoriaExternalAssetLibrary.Load();
            var src=art!=null?(art.SlavicBoulder??art.SlavicFlatRock):null;
            if(src==null)return;
            var specs=new[]{
                new Vector4(-4.65f,-.35f,28f,0f),new Vector4(-3.35f,1.10f,67f,0f),
                new Vector4(3.45f,.95f,241f,0f),new Vector4(4.75f,-.55f,302f,0f),
                new Vector4(-5.10f,-2.55f,18f,0f),new Vector4(5.05f,-3.20f,196f,0f)
            };
            for(int i=0;i<specs.Length;i++)
            {
                var s=specs[i];
                var go=ValoriaKit.BenchmarkPieceModulated("Valoria · Native seam rock "+i,src,
                    new Vector3(s.x,.02f,s.y),1.9f,.85f,Quaternion.Euler(0f,s.z,0f),
                    new Color(.54f,.54f,.50f,1f));
                if(go!=null){go.transform.SetParent(root,true);PiecesBuilt++;}
            }
        }

        static void AddRetainingConnectors(Transform root)
        {
            var specs=new[]{
                ("west lower","RockToWallTransition",new Vector3(-2.95f,.72f,2.15f),22f,2.15f,1.55f),
                ("east lower","RockToWallTransition",new Vector3( 2.95f,.72f,2.05f),202f,2.15f,1.55f),
                ("west mid","CornerWallL",new Vector3(-3.35f,1.18f,4.15f),105f,1.95f,1.95f),
                ("east mid","HighStraightWall",new Vector3(3.45f,1.15f,4.20f),78f,2.15f,1.90f)
            };
            foreach(var s in specs)
            {
                var src=Resources.Load<GameObject>("Valoria/StoneArchitectureKit_v1/"+s.Item2);
                if(src==null)continue;
                var go=ValoriaKit.BenchmarkPieceModulated("Valoria · Native connector · "+s.Item1,src,
                    s.Item3,s.Item5,s.Item6,Quaternion.Euler(0f,s.Item4,0f),
                    new Color(.76f,.76f,.72f,1f));
                if(go!=null){go.transform.SetParent(root,true);PiecesBuilt++;}
            }
        }
    }
}
