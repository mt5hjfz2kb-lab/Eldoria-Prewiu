using Eldoria.Domain;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Eldoria.Presentation
{
    // Full-frame horizon candidate using real project mountain inventory, not procedural backdrop geometry.
    public static class ValoriaFullFrameHorizonAssetPassV1
    {
        public static bool Enabled=false;
        const string RootName="Valoria · Full Frame Horizon Asset Pass v1";

        public static void Build(Transform parent,PlayerState state)
        {
            if(!Enabled||parent==null||state==null)return;
            var old=GameObject.Find(RootName);
            if(old!=null)Object.DestroyImmediate(old);
            var root=new GameObject(RootName).transform;
            root.SetParent(parent,true);

            ApplyAtmosphere();

            var mountain=Resources.Load<GameObject>("WorldInventory/Mountain01");
            var rock=Resources.Load<GameObject>("WorldInventory/Rock02");
            if(mountain==null)return;

            Add(root,mountain,"far west",new Vector3(-17.5f,-1.8f,30.0f),15.5f,8.3f,14f,new Color(.45f,.50f,.51f,1f));
            Add(root,mountain,"far crown",new Vector3(0f,-2.0f,35.5f),18.0f,10.8f,-8f,new Color(.48f,.53f,.55f,1f));
            Add(root,mountain,"far east",new Vector3(18.0f,-1.8f,31.2f),16.0f,8.8f,-22f,new Color(.44f,.49f,.51f,1f));

            Add(root,mountain,"mid west",new Vector3(-19.0f,-1.2f,22.0f),11.2f,6.2f,31f,new Color(.36f,.40f,.39f,1f));
            Add(root,mountain,"mid east",new Vector3(18.8f,-1.2f,23.0f),11.8f,6.6f,-35f,new Color(.35f,.39f,.39f,1f));

            if(rock!=null)
            {
                Add(root,rock,"west foothill",new Vector3(-14.8f,-.35f,18.5f),5.6f,2.2f,18f,new Color(.34f,.35f,.33f,1f));
                Add(root,rock,"east foothill",new Vector3(15.2f,-.35f,19.1f),5.8f,2.3f,-24f,new Color(.34f,.35f,.33f,1f));
            }
        }

        static void ApplyAtmosphere()
        {
            RenderSettings.ambientIntensity=.92f;
            RenderSettings.fog=true;
            RenderSettings.fogMode=FogMode.Linear;
            RenderSettings.fogColor=new Color(.56f,.65f,.70f);
            RenderSettings.fogStartDistance=33f;
            RenderSettings.fogEndDistance=88f;
            var camera=Camera.main;
            if(camera!=null)
            {
                camera.clearFlags=CameraClearFlags.SolidColor;
                camera.backgroundColor=new Color(.45f,.64f,.76f);
                camera.allowHDR=true;
            }
        }

        static void Add(Transform root,GameObject source,string role,Vector3 anchor,float span,float maxHeight,float yaw,Color tint)
        {
            var go=Object.Instantiate(source);
            go.name="Valoria · Full Frame Horizon · "+role;
            go.transform.rotation=Quaternion.Euler(0f,yaw,0f);
            var rs=go.GetComponentsInChildren<Renderer>(true);
            if(rs.Length==0){Object.DestroyImmediate(go);return;}
            var b=rs[0].bounds;
            for(int i=1;i<rs.Length;i++)b.Encapsulate(rs[i].bounds);
            float horizontal=Mathf.Max(.001f,Mathf.Max(b.size.x,b.size.z));
            float scale=Mathf.Min(span/horizontal,maxHeight/Mathf.Max(.001f,b.size.y));
            go.transform.localScale*=scale;
            rs=go.GetComponentsInChildren<Renderer>(true);
            b=rs[0].bounds;
            for(int i=1;i<rs.Length;i++)b.Encapsulate(rs[i].bounds);
            go.transform.position+=anchor-new Vector3(b.center.x,b.min.y,b.center.z);
            Tint(go,tint);
            foreach(var c in go.GetComponentsInChildren<Collider>(true))c.enabled=false;
            foreach(var mb in go.GetComponentsInChildren<MonoBehaviour>(true))mb.enabled=false;
            go.transform.SetParent(root,true);
        }

        static void Tint(GameObject go,Color tint)
        {
            foreach(var r in go.GetComponentsInChildren<Renderer>(true))
            {
                var mats=r.sharedMaterials;
                var dst=new Material[mats.Length];
                for(int i=0;i<mats.Length;i++)
                {
                    if(mats[i]==null){dst[i]=null;continue;}
                    var m=new Material(mats[i]){name="Valoria Horizon · "+mats[i].name};
                    if(m.HasProperty("_BaseColor"))m.SetColor("_BaseColor",tint);
                    else if(m.HasProperty("_Color"))m.SetColor("_Color",tint);
                    if(m.HasProperty("_Smoothness"))m.SetFloat("_Smoothness",.02f);
                    dst[i]=m;
                }
                r.sharedMaterials=dst;
            }
        }
    }
}
