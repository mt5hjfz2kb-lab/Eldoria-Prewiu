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

            // Method 3: localized fixed-camera matte. The full annulus proved invisible after
            // grading calibration but did not materially improve the frame. This patch exists
            // only over the lower-right hanging cliff residue and samples the exact backplate.
            var go=new GameObject(RootName+" · lower cliff matte");
            go.transform.SetParent(root.transform,true);

            var vp=new[]{
                new Vector2(.525f,.305f),
                new Vector2(.565f,.278f),
                new Vector2(.665f,.282f),
                new Vector2(.704f,.320f),
                new Vector2(.684f,.372f),
                new Vector2(.575f,.368f)
            };

            // The matte must sit in front of the fortress residue but behind UI. With the fixed
            // orthographic review camera, ViewportToWorldPoint gives deterministic placement.
            const float depth=24f;
            var verts=new Vector3[vp.Length+1];
            var uv=new Vector2[verts.Length];
            var colors=new Color[verts.Length];

            Vector2 centre=Vector2.zero;
            for(int i=0;i<vp.Length;i++)centre+=vp[i];
            centre/=vp.Length;

            verts[0]=camera.ViewportToWorldPoint(new Vector3(centre.x,centre.y,depth));
            uv[0]=centre;
            colors[0]=Color.black;
            for(int i=0;i<vp.Length;i++)
            {
                verts[i+1]=camera.ViewportToWorldPoint(new Vector3(vp[i].x,vp[i].y,depth));
                uv[i+1]=vp[i];
                colors[i+1]=Color.black;
            }

            var tris=new int[vp.Length*3];
            for(int i=0;i<vp.Length;i++)
            {
                int n=(i+1)%vp.Length;
                tris[i*3]=0;
                tris[i*3+1]=i+1;
                tris[i*3+2]=n+1;
            }

            var mesh=new Mesh{name="Valoria Visual Shell v2 · lower cliff matte"};
            mesh.vertices=verts;
            mesh.uv=uv;
            mesh.colors=colors;
            mesh.triangles=tris;
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();

            go.AddComponent<MeshFilter>().sharedMesh=mesh;
            var mr=go.AddComponent<MeshRenderer>();
            var mat=new Material(shader){name="Valoria Visual Shell v2 · exact projected matte"};
            mat.SetTexture("_BackplateTex",tex);
            mat.SetColor("_BackplateTint",new Color(.84f,.88f,.91f,1f));
            mat.SetFloat("_ContactStrength",0f);
            mr.sharedMaterial=mat;
            mr.shadowCastingMode=ShadowCastingMode.Off;
            mr.receiveShadows=false;

            foreach(var col in go.GetComponentsInChildren<Collider>(true))col.enabled=false;
            return true;
        }
    }
}
