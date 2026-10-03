using Eldoria.Domain;
using UnityEngine;
using UnityEngine.Rendering;
using Object=UnityEngine.Object;

namespace Eldoria.Presentation
{
    // Experimental fixed-camera 2.5D terrain shell.
    // The shell samples the exact same backplate texture in screen space, so it can add
    // depth/contact around Valoria without introducing a foreign terrain colour or low-poly frame.
    public static class ValoriaVisualShellV2
    {
        public static bool Enabled=true;
        const string RootName="Valoria · Visual Shell v2";

        public static bool Build(Transform parent,PlayerState state,Camera camera)
        {
            if(!Enabled||parent==null||camera==null)return false;
            var old=GameObject.Find(RootName);
            if(old!=null)Object.DestroyImmediate(old);

            var tex=Resources.Load<Texture2D>("Valoria/BackplateCandidates/kiara3_1");
            var shader=Shader.Find("Eldoria/Valoria Shell Terrain");
            if(tex==null||shader==null)return false;

            var root=new GameObject(RootName);
            root.transform.SetParent(parent,true);

            // Continuous-terrain method 1: camera-locked photographic foreground ridge.
            // The ridge reuses the lower portion of the canonical mountain photograph, lifted
            // upward as a feathered foreground landform so Valoria sits behind real-looking
            // geology instead of floating over the distant valley.
            var go=new GameObject(RootName+" · photographic foreground ridge");
            go.transform.SetParent(root.transform,true);

            const int cols=25;
            const float depth=23f;
            var verts=new Vector3[cols*3];
            var uv=new Vector2[verts.Length];
            var colors=new Color[verts.Length];
            var tris=new int[(cols-1)*12];

            for(int i=0;i<cols;i++)
            {
                float x=i/(float)(cols-1);
                float centre=1f-Mathf.Abs(x-.5f)*2f;
                centre=Mathf.Clamp01(centre);

                // Lower at the edges, highest below the fortress; deterministic rock-like wobble.
                float ridge=.205f + .070f*Mathf.Pow(centre,1.45f)
                    + .012f*Mathf.Sin(x*Mathf.PI*5f)
                    + .008f*Mathf.Sin(x*Mathf.PI*11f+.7f);
                float feather=ridge+.045f;

                int b=i;
                int r=cols+i;
                int f=cols*2+i;

                verts[b]=camera.ViewportToWorldPoint(new Vector3(x,-.035f,depth));
                verts[r]=camera.ViewportToWorldPoint(new Vector3(x,ridge,depth));
                verts[f]=camera.ViewportToWorldPoint(new Vector3(x,feather,depth));

                // Reuse the bottom photographic geology but stretch it upward into the ridge.
                uv[b]=new Vector2(x,.015f);
                uv[r]=new Vector2(x,.225f);
                uv[f]=new Vector2(x,.255f);

                colors[b]=new Color(1,1,1,1);
                colors[r]=new Color(1,1,1,1);
                colors[f]=new Color(1,1,1,0);
            }

            int ti=0;
            for(int i=0;i<cols-1;i++)
            {
                int b0=i,b1=i+1,r0=cols+i,r1=cols+i+1,f0=cols*2+i,f1=cols*2+i+1;
                tris[ti++]=b0;tris[ti++]=r0;tris[ti++]=b1;
                tris[ti++]=b1;tris[ti++]=r0;tris[ti++]=r1;
                tris[ti++]=r0;tris[ti++]=f0;tris[ti++]=r1;
                tris[ti++]=r1;tris[ti++]=f0;tris[ti++]=f1;
            }

            var mesh=new Mesh{name="Valoria Visual Shell v2 · photographic ridge"};
            mesh.vertices=verts;
            mesh.uv=uv;
            mesh.colors=colors;
            mesh.triangles=tris;
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();

            go.AddComponent<MeshFilter>().sharedMesh=mesh;
            var mr=go.AddComponent<MeshRenderer>();
            var mat=new Material(shader){name="Valoria Visual Shell v2 · photographic ridge"};
            mat.SetTexture("_BackplateTex",tex);
            mat.SetColor("_BackplateTint",new Color(.84f,.88f,.91f,1f));
            mat.SetFloat("_ContactStrength",0f);
            mat.SetFloat("_UseMeshUv",1f);
            mr.sharedMaterial=mat;
            mr.shadowCastingMode=ShadowCastingMode.Off;
            mr.receiveShadows=false;

            return true;
        }
    }
}
