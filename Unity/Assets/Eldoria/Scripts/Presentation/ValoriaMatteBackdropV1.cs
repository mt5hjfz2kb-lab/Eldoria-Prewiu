using Eldoria.Domain;
using UnityEngine;
using UnityEngine.Rendering;
using Object=UnityEngine.Object;

namespace Eldoria.Presentation
{
    // Camera-facing 2.5D distant matte for Valoria's fixed gameplay composition.
    // Proof-only until a crop is accepted and persisted.
    public static class ValoriaMatteBackdropV1
    {
        public static bool Enabled=false;
        public static Rect UvRect=new Rect(0f,.30f,.5f,.45f);
        const string RootName="Valoria · Matte Backdrop v1";

        public static bool Build(Transform parent,PlayerState state)
        {
            if(!Enabled||parent==null)return false;
            var old=GameObject.Find(RootName);
            if(old!=null)Object.DestroyImmediate(old);

            var tex=Resources.Load<Texture2D>("Valoria/SkyCandidates/alps_field");
            if(tex==null)return false;

            var camera=Camera.main;
            if(camera==null)return false;

            var root=new GameObject(RootName).transform;
            root.SetParent(parent,true);

            var forward=camera.transform.forward.normalized;
            var right=camera.transform.right.normalized;
            var up=camera.transform.up.normalized;
            var center=camera.transform.position+forward*85f+up*1.5f;

            const float halfW=54f;
            const float halfH=24f;
            var mesh=new Mesh{name="Valoria Matte Backdrop v1"};
            mesh.vertices=new[]{
                center-right*halfW-up*halfH,
                center+right*halfW-up*halfH,
                center-right*halfW+up*halfH,
                center+right*halfW+up*halfH
            };
            float u0=UvRect.xMin,u1=UvRect.xMax,v0=UvRect.yMin,v1=UvRect.yMax;
            mesh.uv=new[]{
                new Vector2(u0,v0),new Vector2(u1,v0),
                new Vector2(u0,v1),new Vector2(u1,v1)
            };
            mesh.triangles=new[]{0,2,1,1,2,3};
            mesh.RecalculateBounds();

            var go=new GameObject("Valoria · Matte Backdrop · Alps Field");
            go.transform.SetParent(root,true);
            go.AddComponent<MeshFilter>().sharedMesh=mesh;

            var shader=Shader.Find("Universal Render Pipeline/Unlit")??Shader.Find("Unlit/Texture")??Shader.Find("Standard");
            var mat=new Material(shader){name="Valoria · Matte Backdrop · Alps Field CC0"};
            if(mat.HasProperty("_BaseMap"))mat.SetTexture("_BaseMap",tex);
            if(mat.HasProperty("_MainTex"))mat.SetTexture("_MainTex",tex);
            if(mat.HasProperty("_BaseColor"))mat.SetColor("_BaseColor",new Color(.92f,.96f,1f,1f));
            if(mat.HasProperty("_Color"))mat.SetColor("_Color",new Color(.92f,.96f,1f,1f));
            if(mat.HasProperty("_Cull"))mat.SetFloat("_Cull",0f);

            var mr=go.AddComponent<MeshRenderer>();
            mr.sharedMaterial=mat;
            mr.shadowCastingMode=ShadowCastingMode.Off;
            mr.receiveShadows=false;

            RenderSettings.fog=true;
            RenderSettings.fogMode=FogMode.Linear;
            RenderSettings.fogColor=new Color(.59f,.68f,.75f);
            RenderSettings.fogStartDistance=33f;
            RenderSettings.fogEndDistance=96f;
            RenderSettings.ambientMode=AmbientMode.Trilight;
            RenderSettings.ambientSkyColor=new Color(.69f,.77f,.84f);
            RenderSettings.ambientEquatorColor=new Color(.50f,.49f,.44f);
            RenderSettings.ambientGroundColor=new Color(.24f,.22f,.19f);
            RenderSettings.ambientIntensity=.94f;

            camera.clearFlags=CameraClearFlags.SolidColor;
            camera.backgroundColor=new Color(.57f,.70f,.80f);
            camera.allowHDR=true;

            return true;
        }
    }
}
