using System.Collections.Generic;
using Eldoria.Domain;
using UnityEngine;
using Object=UnityEngine.Object;

namespace Eldoria.Presentation
{
    // Single visual-only stair mesh replacing the wide prototype tread renderers.
    public static class ValoriaCompactStairMeshV1
    {
        public static bool Enabled=false;
        public static int HiddenLegacyRenderers{get;private set;}

        const string RootName="Valoria · Compact Stair Mesh v1";

        public static void Build(Transform parent,PlayerState state)
        {
            if(!Enabled||parent==null)return;

            var old=GameObject.Find(RootName);
            if(old!=null)Object.DestroyImmediate(old);

            HiddenLegacyRenderers=0;
            foreach(var r in Object.FindObjectsByType<Renderer>(FindObjectsSortMode.None))
            {
                if(r==null||!r.enabled||!r.gameObject.activeInHierarchy)continue;
                if(!Chain(r.transform).Contains("vpd · vertical stair"))continue;
                r.enabled=false;
                HiddenLegacyRenderers++;
            }

            var root=new GameObject(RootName).transform;
            root.SetParent(parent,true);
            var go=new GameObject("Valoria · Compact Stair · unified cobble");
            go.transform.SetParent(root,false);
            var mf=go.AddComponent<MeshFilter>();
            var mr=go.AddComponent<MeshRenderer>();
            mf.sharedMesh=BuildMesh();

            var cobble=ValoriaKit.ExternalPbrSurfaceMaterial(
                "cobble",new Color(.44f,.42f,.37f,1f),new Vector2(1.35f,1.35f),.022f,.92f)
                ?? ValoriaKit.DetailedSurfaceMaterial(new Color(.42f,.40f,.36f,1f),"stone",new Vector2(1.35f,1.35f),.94f);
            mr.sharedMaterial=cobble;
        }

        static Mesh BuildMesh()
        {
            const int steps=11;
            const float z0=-.05f;
            const float depth=.535f;
            const float rise=.195f;
            const float baseY=.39f;

            var v=new List<Vector3>();
            var uv=new List<Vector2>();
            var tris=new List<int>();

            for(int i=0;i<steps;i++)
            {
                float t=i/(float)(steps-1);
                float width=Mathf.Lerp(2.32f,2.62f,t);
                float half=width*.5f;
                float y=baseY+i*rise;
                float za=z0+i*depth;
                float zb=za+depth*.94f;

                int top=v.Count;
                v.Add(new Vector3(-half,y,za));
                v.Add(new Vector3( half,y,za));
                v.Add(new Vector3( half,y,zb));
                v.Add(new Vector3(-half,y,zb));
                uv.Add(new Vector2(0f,i*.48f));uv.Add(new Vector2(1f,i*.48f));
                uv.Add(new Vector2(1f,(i+1)*.48f));uv.Add(new Vector2(0f,(i+1)*.48f));
                tris.Add(top);tris.Add(top+2);tris.Add(top+1);
                tris.Add(top);tris.Add(top+3);tris.Add(top+2);

                if(i>0)
                {
                    int riser=v.Count;
                    float prevY=baseY+(i-1)*rise;
                    v.Add(new Vector3(-half,prevY,za));
                    v.Add(new Vector3( half,prevY,za));
                    v.Add(new Vector3( half,y,za));
                    v.Add(new Vector3(-half,y,za));
                    uv.Add(new Vector2(0f,0f));uv.Add(new Vector2(1f,0f));
                    uv.Add(new Vector2(1f,.25f));uv.Add(new Vector2(0f,.25f));
                    tris.Add(riser);tris.Add(riser+2);tris.Add(riser+1);
                    tris.Add(riser);tris.Add(riser+3);tris.Add(riser+2);
                }
            }

            var mesh=new Mesh{name="Valoria Compact Stair Mesh v1"};
            mesh.SetVertices(v);mesh.SetTriangles(tris,0);mesh.SetUVs(0,uv);
            mesh.RecalculateNormals();mesh.RecalculateTangents();mesh.RecalculateBounds();
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
