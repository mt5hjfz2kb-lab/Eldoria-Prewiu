using Eldoria.Domain;
using UnityEngine;
using UnityEngine.Rendering;
using Object=UnityEngine.Object;

namespace Eldoria.Presentation
{
    public static class ValoriaJaggedBackdropV1
    {
        public static bool Enabled=false;
        public static float VerticalBias=0f;
        const string RootName="Valoria · Jagged Peaks Backdrop v1";

        public static bool Build(Transform parent,PlayerState state)
        {
            if(!Enabled||parent==null)return false;
            var old=GameObject.Find(RootName);if(old!=null)Object.DestroyImmediate(old);
            var tex=Resources.Load<Texture2D>("Valoria/SkyCandidates/jagged_peaks_valley");
            var camera=Camera.main;
            if(tex==null||camera==null)return false;

            var root=new GameObject(RootName).transform;root.SetParent(parent,true);
            var forward=camera.transform.forward.normalized;
            var right=camera.transform.right.normalized;
            var up=camera.transform.up.normalized;

            // Orthographic backdrop fills the landscape viewport exactly; scale is independent of world distance.
            float halfH=11.8f;
            float halfW=halfH*(16f/9f);
            var center=camera.transform.position+forward*90f+up*VerticalBias;

            var mesh=new Mesh{name="Valoria Jagged Peaks Backdrop v1"};
            mesh.vertices=new[]{
                center-right*halfW-up*halfH,
                center+right*halfW-up*halfH,
                center-right*halfW+up*halfH,
                center+right*halfW+up*halfH
            };
            mesh.uv=new[]{new Vector2(0,0),new Vector2(1,0),new Vector2(0,1),new Vector2(1,1)};
            mesh.triangles=new[]{0,2,1,1,2,3};mesh.RecalculateBounds();

            var go=new GameObject("Valoria · Jagged Peaks · CC0 backplate");go.transform.SetParent(root,true);
            go.AddComponent<MeshFilter>().sharedMesh=mesh;
            var shader=Shader.Find("Universal Render Pipeline/Unlit")??Shader.Find("Unlit/Texture")??Shader.Find("Standard");
            var mat=new Material(shader){name="Valoria · Jagged Peaks CC0 backplate"};
            if(mat.HasProperty("_BaseMap"))mat.SetTexture("_BaseMap",tex);
            if(mat.HasProperty("_MainTex"))mat.SetTexture("_MainTex",tex);
            var tint=new Color(.78f,.86f,.93f,1f);
            if(mat.HasProperty("_BaseColor"))mat.SetColor("_BaseColor",tint);
            if(mat.HasProperty("_Color"))mat.SetColor("_Color",tint);
            if(mat.HasProperty("_Cull"))mat.SetFloat("_Cull",0f);
            var mr=go.AddComponent<MeshRenderer>();mr.sharedMaterial=mat;mr.shadowCastingMode=ShadowCastingMode.Off;mr.receiveShadows=false;

            camera.clearFlags=CameraClearFlags.SolidColor;
            camera.backgroundColor=new Color(.59f,.70f,.80f);
            camera.allowHDR=true;
            return true;
        }
    }
}
