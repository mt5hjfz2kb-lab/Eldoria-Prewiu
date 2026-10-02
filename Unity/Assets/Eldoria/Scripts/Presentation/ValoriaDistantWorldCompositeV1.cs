using System;
using Eldoria.Domain;
using UnityEngine;
using Object=UnityEngine.Object;

namespace Eldoria.Presentation
{
    // Integrated distant-world solution selected from the full-frame convergence loop.
    // Uses the stylized adaptive backdrop only as distant depth and bridges it with real project vegetation/rock.
    public static class ValoriaDistantWorldCompositeV1
    {
        public static bool Enabled=false;
        const string RootName="Valoria · Distant World Composite v1";

        public static void Build(Transform parent,PlayerState state)
        {
            if(!Enabled||parent==null||state==null)return;
            var old=GameObject.Find(RootName);
            if(old!=null)Object.DestroyImmediate(old);
            var root=new GameObject(RootName).transform;
            root.SetParent(parent,true);

            SuppressChain("Valoria · rescued hero flank");

            var camera=Camera.main;
            if(camera==null)return;

            ValoriaCameraBackdropV3.Enabled=true;
            ValoriaCameraBackdropV3.DesktopCrop=new Vector4(.60f,.33f,.35f,.41f);
            ValoriaCameraBackdropV3.MobileCrop=new Vector4(.18f,.25f,.44f,.49f);
            if(!ValoriaCameraBackdropV3.Build(parent,camera))
                throw new InvalidOperationException("Distant world stylized backdrop unavailable.");

            camera.clearFlags=CameraClearFlags.SolidColor;
            camera.backgroundColor=new Color(.49f,.58f,.63f);
            camera.farClipPlane=Mathf.Max(camera.farClipPlane,500f);

            BuildTransitionBand(root);
            ValoriaFullFrameFinishV1.Enabled=true;
            ValoriaFullFrameFinishV1.Build(root,state);
        }

        static void BuildTransitionBand(Transform root)
        {
            var treeA=Resources.Load<GameObject>("WorldInventory/Tree01A");
            var treeB=Resources.Load<GameObject>("WorldInventory/Tree01B");
            var rock=Resources.Load<GameObject>("WorldInventory/Rock02");

            // Scale is intentionally small: orthographic camera otherwise destroys distance cues.
            var trees=new[]{
                new Vector4(-15.8f,13.3f,.48f,17f),
                new Vector4(-13.3f,14.4f,.58f,61f),
                new Vector4(-10.7f,13.8f,.43f,104f),
                new Vector4(-8.5f,15.2f,.52f,149f),
                new Vector4(8.2f,15.0f,.50f,211f),
                new Vector4(10.6f,13.9f,.43f,257f),
                new Vector4(13.0f,14.4f,.57f,301f),
                new Vector4(15.6f,13.1f,.47f,342f)
            };
            for(int i=0;i<trees.Length;i++)
            {
                var s=trees[i];
                var src=(i%2==0?treeA:treeB)??treeA??treeB;
                Add(root,src,"distant tree "+i,new Vector3(s.x,-.12f,s.y),
                    s.z,1.15f+(i%3)*.10f,s.w,new Color(.25f,.31f,.27f,1f));
            }

            if(rock!=null)
            {
                Add(root,rock,"rear west bridge rock",new Vector3(-11.5f,-.34f,12.8f),2.9f,1.05f,43f,new Color(.36f,.37f,.35f,1f));
                Add(root,rock,"rear east bridge rock",new Vector3(11.3f,-.34f,12.9f),2.9f,1.05f,223f,new Color(.36f,.37f,.35f,1f));
                Add(root,rock,"rear centre-left rock",new Vector3(-6.8f,-.30f,13.6f),2.15f,.82f,92f,new Color(.38f,.39f,.36f,1f));
                Add(root,rock,"rear centre-right rock",new Vector3(6.9f,-.30f,13.7f),2.15f,.82f,272f,new Color(.38f,.39f,.36f,1f));
            }

            // Sparse cool haze pools sit behind the real city and in front of the plate.
            AddHazeLight(root,new Vector3(-7.5f,2.1f,13.2f));
            AddHazeLight(root,new Vector3(7.5f,2.1f,13.2f));
        }

        static void Add(Transform root,GameObject source,string role,Vector3 p,float footprint,float height,float yaw,Color tint)
        {
            if(source==null)return;
            var go=ValoriaKit.BenchmarkPieceIntegrated("Valoria · Distant World · "+role,source,p,footprint,height,
                Quaternion.Euler(0f,yaw,0f),tint);
            if(go==null)return;
            go.transform.SetParent(root,true);
            foreach(var c in go.GetComponentsInChildren<Collider>(true))c.enabled=false;
            foreach(var h in go.GetComponentsInChildren<WorldHotspot>(true))Object.DestroyImmediate(h);
            foreach(var mb in go.GetComponentsInChildren<MonoBehaviour>(true))if(!(mb is WorldHotspot))mb.enabled=false;
        }

        static void AddHazeLight(Transform root,Vector3 p)
        {
            var go=new GameObject("Valoria · Distant World · cool haze fill");
            go.transform.SetParent(root,true);go.transform.position=p;
            var l=go.AddComponent<Light>();
            l.type=LightType.Point;
            l.color=new Color(.56f,.66f,.72f);
            l.intensity=.08f;
            l.range=4.5f;
            l.shadows=LightShadows.None;
        }

        static void SuppressChain(string exactName)
        {
            foreach(var r in Object.FindObjectsByType<Renderer>(FindObjectsSortMode.None))
            {
                if(r==null||!r.enabled||!r.gameObject.activeInHierarchy)continue;
                for(var t=r.transform;t!=null;t=t.parent)
                {
                    if(!string.Equals(t.name,exactName,StringComparison.Ordinal))continue;
                    r.enabled=false;
                    break;
                }
            }
        }
    }
}
