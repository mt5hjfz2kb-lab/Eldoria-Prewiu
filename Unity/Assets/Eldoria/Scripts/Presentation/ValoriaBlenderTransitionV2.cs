using System;
using Eldoria.Domain;
using UnityEngine;
using UnityEngine.Rendering;
using Object=UnityEngine.Object;

namespace Eldoria.Presentation
{
    // This GLB is staged by the experimental workflow, never committed as production art.
    public static class ValoriaBlenderTransitionV2
    {
        public static int RenderersBuilt{get;private set;}
        public static void Build(Transform parent,PlayerState state)
        {
            var src=Resources.Load<GameObject>("Valoria/ExperimentalBlenderCliff/ValoriaShellTransition");
            if(src==null)throw new Exception("Blender shell resource was not staged.");
            var go=Object.Instantiate(src);
            go.name="Valoria · Blender transition v2 · visual only";
            go.transform.rotation=Quaternion.Euler(0,180,0);
            go.transform.SetParent(parent,true);
            var renderers=go.GetComponentsInChildren<Renderer>(true);
            if(renderers.Length==0)throw new Exception("Blender shell GLB has no renderers.");
            var ground=ValoriaKit.ExternalPbrSurfaceMaterial("dirt",new Color(.58f,.57f,.51f),new Vector2(.32f,.32f),.015f,.86f);
            var rock=ValoriaKit.ExternalPbrSurfaceMaterial("rock",new Color(.71f,.70f,.65f),new Vector2(.55f,.55f),.025f,.91f);
            if(ground==null||rock==null)throw new Exception("Shared Blender shell PBR maps missing.");
            foreach(var r in renderers)
            {
                var mats=r.sharedMaterials;
                for(int i=0;i<mats.Length;i++)
                    mats[i]=mats[i]!=null&&mats[i].name.Contains("ShellGround")?ground:rock;
                r.sharedMaterials=mats;
                r.shadowCastingMode=ShadowCastingMode.On;
                r.receiveShadows=true;
            }
            var bounds=renderers[0].bounds;
            for(int i=1;i<renderers.Length;i++)bounds.Encapsulate(renderers[i].bounds);
            go.transform.position+=new Vector3(-bounds.center.x,0f,3.0f-bounds.center.z);
            foreach(var c in go.GetComponentsInChildren<Collider>(true))Object.DestroyImmediate(c);
            foreach(var h in go.GetComponentsInChildren<WorldHotspot>(true))Object.DestroyImmediate(h);
            foreach(var m in go.GetComponentsInChildren<MonoBehaviour>(true))m.enabled=false;
            RenderersBuilt=renderers.Length;
        }
    }
}
