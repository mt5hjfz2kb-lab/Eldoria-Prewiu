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

            var rockTemplate=Resources.Load<Material>("Valoria/LowerCityWorldRock");
            Material rock;
            if(rockTemplate!=null)
            {
                rock=new Material(rockTemplate){name="Valoria · Compact Stair · support rock"};
                if(rock.HasProperty("_Color"))rock.SetColor("_Color",new Color(.40f,.41f,.38f,1f));
                if(rock.HasProperty("_Tiling"))rock.SetFloat("_Tiling",.082f);
                if(rock.HasProperty("_Smoothness"))rock.SetFloat("_Smoothness",.11f);
                if(rock.HasProperty("_Strength"))rock.SetFloat("_Strength",.74f);
            }
            else
            {
                rock=ValoriaKit.ExternalPbrSurfaceMaterial(
                    "rock",new Color(.38f,.39f,.37f,1f),new Vector2(.95f,.95f),.018f,.98f)
                    ?? ValoriaKit.DetailedSurfaceMaterial(new Color(.37f,.38f,.36f,1f),"stone",new Vector2(.95f,.95f),.98f);
            }

            // v25: no large monolithic support. The stair is presentation-only and now
            // sits directly against the fortress cliff; existing terrain/rock owns the mass.
        }

        static Mesh BuildSupportMesh()
        {
            // v16: follow the stair rise with a faceted rock spine instead of one broad
            // trapezoid. This keeps the support visually attached to the compact terrace
            // while breaking the artificial grey ramp silhouette at the official camera.
            // v25: crop the presentation-only support below the inhabited cliff foot.
            // Gameplay route/colliders remain untouched; only the visible rock spine starts higher.
            var z=new[]{1.28f,2.42f,3.58f,4.76f,5.95f};
            var half=new[]{.94f,1.02f,1.12f,1.24f,1.36f};
            var topY=new[]{.82f,1.12f,1.44f,1.78f,2.16f};
            var bottomY=new[]{.18f,.30f,.46f,.64f,.84f};

            var v=new List<Vector3>();
            var uv=new List<Vector2>();
            var tris=new List<int>();

            for(int i=0;i<z.Length;i++)
            {
                float wobble=(i%2==0?-.10f:.08f);
                float left=-half[i]+wobble;
                float right=half[i]+wobble*.45f;
                v.Add(new Vector3(left,topY[i],z[i]));
                v.Add(new Vector3(right,topY[i],z[i]));
                v.Add(new Vector3(left-.24f,bottomY[i],z[i]-.05f));
                v.Add(new Vector3(right+.24f,bottomY[i],z[i]-.05f));
                float u=i/(float)(z.Length-1);
                uv.Add(new Vector2(0f,u));
                uv.Add(new Vector2(1f,u));
                uv.Add(new Vector2(0f,u));
                uv.Add(new Vector2(1f,u));
            }

            for(int i=0;i<z.Length-1;i++)
            {
                int a=i*4;
                int b=(i+1)*4;

                // narrow top shoulder under the stair
                tris.Add(a);tris.Add(b+1);tris.Add(a+1);
                tris.Add(a);tris.Add(b);tris.Add(b+1);

                // left fractured face
                tris.Add(a+2);tris.Add(b);tris.Add(a);
                tris.Add(a+2);tris.Add(b+2);tris.Add(b);

                // right fractured face
                tris.Add(a+1);tris.Add(b+1);tris.Add(a+3);
                tris.Add(a+3);tris.Add(b+1);tris.Add(b+3);

                // underside closes the visual mass without affecting gameplay
                tris.Add(a+2);tris.Add(a+3);tris.Add(b+3);
                tris.Add(a+2);tris.Add(b+3);tris.Add(b+2);
            }

            // front and rear caps
            tris.Add(0);tris.Add(1);tris.Add(3);
            tris.Add(0);tris.Add(3);tris.Add(2);
            int e=(z.Length-1)*4;
            tris.Add(e);tris.Add(e+3);tris.Add(e+1);
            tris.Add(e);tris.Add(e+2);tris.Add(e+3);

            var mesh=new Mesh{name="Valoria Compact Stair Rock Support v2"};
            mesh.SetVertices(v);mesh.SetTriangles(tris,0);mesh.SetUVs(0,uv);
            mesh.RecalculateNormals();mesh.RecalculateTangents();mesh.RecalculateBounds();
            return mesh;
        }

        static Mesh BuildMesh()
        {
            // v25: the low three presentation steps caused a long bridge/ramp read.
            // Start the visible stair inside the cliff foot while preserving the hidden real route.
            const int steps=5;
            const float z0=3.15f;
            const float depth=.50f;
            const float rise=.22f;
            const float baseY=1.52f;

            var v=new List<Vector3>();
            var uv=new List<Vector2>();
            var tris=new List<int>();

            for(int i=0;i<steps;i++)
            {
                float t=i/(float)(steps-1);
                float width=Mathf.Lerp(1.42f,1.68f,t);
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
