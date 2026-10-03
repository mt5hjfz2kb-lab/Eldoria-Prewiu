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

        public static bool Build(Transform parent,PlayerState state)
        {
            if(!Enabled||parent==null)return false;
            var old=GameObject.Find(RootName);
            if(old!=null)Object.DestroyImmediate(old);

            var tex=Resources.Load<Texture2D>("Valoria/BackplateCandidates/kiara3_1");
            var shader=Shader.Find("Eldoria/Valoria Shell Terrain");
            if(tex==null||shader==null)return false;

            var root=new GameObject(RootName);
            root.transform.SetParent(parent,true);

            var go=new GameObject(RootName+" · projected valley annulus");
            go.transform.SetParent(root.transform,true);

            const int segments=96;
            const int rings=5;
            const float cx=0f,cz=5.55f;
            const float innerX=7.75f,innerZ=5.35f;
            const float outerX=22.5f,outerZ=17.5f;

            var verts=new Vector3[(rings+1)*segments];
            var uv=new Vector2[verts.Length];
            var colors=new Color[verts.Length];
            var tris=new int[rings*segments*6];

            for(int r=0;r<=rings;r++)
            {
                float t=r/(float)rings;
                // Bias vertices toward the inner edge so the contact gradient is smooth.
                float shaped=1f-Mathf.Pow(1f-t,1.65f);
                float rx=Mathf.Lerp(innerX,outerX,shaped);
                float rz=Mathf.Lerp(innerZ,outerZ,shaped);
                float y=Mathf.Lerp(-.18f,-1.35f,t);

                for(int s=0;s<segments;s++)
                {
                    float a=s/(float)segments*Mathf.PI*2f;
                    float wobble=1f+.028f*Mathf.Sin(a*5f)+.018f*Mathf.Sin(a*9f+1.7f);
                    float x=cx+Mathf.Cos(a)*rx*wobble;
                    float z=cz+Mathf.Sin(a)*rz*wobble;
                    int i=r*segments+s;
                    verts[i]=new Vector3(x,y,z);
                    uv[i]=new Vector2(s/(float)segments,t);

                    // Near-island contact darkening only. Outer ring becomes exact backplate.
                    float contact=(1f-t);
                    contact=contact*contact*.13f;
                    colors[i]=new Color(contact,0f,0f,1f);
                }
            }

            int ti=0;
            for(int r=0;r<rings;r++)
            for(int s=0;s<segments;s++)
            {
                int n=(s+1)%segments;
                int a=r*segments+s;
                int b=r*segments+n;
                int c=(r+1)*segments+s;
                int d=(r+1)*segments+n;
                tris[ti++]=a;tris[ti++]=c;tris[ti++]=b;
                tris[ti++]=b;tris[ti++]=c;tris[ti++]=d;
            }

            var mesh=new Mesh{name="Valoria Visual Shell v2 · annulus"};
            mesh.indexFormat=IndexFormat.UInt32;
            mesh.vertices=verts;
            mesh.uv=uv;
            mesh.colors=colors;
            mesh.triangles=tris;
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();

            go.AddComponent<MeshFilter>().sharedMesh=mesh;
            var mr=go.AddComponent<MeshRenderer>();
            var mat=new Material(shader){name="Valoria Visual Shell v2 · projected valley"};
            mat.SetTexture("_BackplateTex",tex);
            mat.SetFloat("_ContactStrength",1f);
            mr.sharedMaterial=mat;
            mr.shadowCastingMode=ShadowCastingMode.Off;
            mr.receiveShadows=false;

            foreach(var col in go.GetComponentsInChildren<Collider>(true))col.enabled=false;
            return true;
        }
    }
}
