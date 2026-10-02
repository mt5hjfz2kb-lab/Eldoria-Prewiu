using Eldoria.Domain;
using UnityEngine;
using Object=UnityEngine.Object;

namespace Eldoria.Presentation
{
    public static class ValoriaMonumentalArchFrameV2
    {
        public static bool Enabled=false;
        const string RootName="Valoria · Monumental Arch Frame v2";

        public static void Build(Transform parent,PlayerState state)
        {
            if(!Enabled||parent==null||state==null)return;
            var old=GameObject.Find(RootName);if(old!=null)Object.DestroyImmediate(old);
            var root=new GameObject(RootName).transform;root.SetParent(parent,true);

            var arch=Resources.Load<GameObject>("WorldInventory/Arch_Gothic");
            var wall=Resources.Load<GameObject>("WorldInventory/Wall_Broken");
            var rock=Resources.Load<GameObject>("WorldInventory/Rock02");
            if(arch==null||wall==null)return;

            // Framing masses stay outside certified circulation and leave the central Bastion silhouette clear.
            Add(root,arch,"west arch",new Vector3(-11.8f,-.05f,8.4f),4.15f,6.25f,24f,new Color(.51f,.50f,.46f,1f));
            Add(root,wall,"west wall",new Vector3(-14.0f,-.05f,7.4f),3.3f,3.15f,70f,new Color(.46f,.45f,.42f,1f));
            if(rock!=null)Add(root,rock,"west base",new Vector3(-12.6f,-.25f,6.7f),4.0f,1.25f,38f,new Color(.37f,.38f,.35f,1f));

            Add(root,arch,"east arch",new Vector3(11.5f,-.05f,8.2f),4.35f,6.55f,204f,new Color(.51f,.50f,.46f,1f));
            Add(root,wall,"east wall",new Vector3(13.8f,-.05f,7.2f),3.2f,3.05f,250f,new Color(.46f,.45f,.42f,1f));
            if(rock!=null)Add(root,rock,"east base",new Vector3(12.5f,-.25f,6.5f),4.0f,1.25f,220f,new Color(.37f,.38f,.35f,1f));

            AddWarmth(root,new Vector3(-10.9f,1.7f,7.8f));
            AddWarmth(root,new Vector3(10.8f,1.7f,7.7f));
        }

        static void Add(Transform root,GameObject source,string role,Vector3 p,float footprint,float maxHeight,float yaw,Color tint)
        {
            var go=ValoriaKit.BenchmarkPieceIntegrated("Valoria · Monumental Arch v2 · "+role,source,p,footprint,maxHeight,Quaternion.Euler(0f,yaw,0f),tint);
            if(go==null)return;
            go.transform.SetParent(root,true);
            foreach(var c in go.GetComponentsInChildren<Collider>(true))c.enabled=false;
            foreach(var h in go.GetComponentsInChildren<WorldHotspot>(true))Object.DestroyImmediate(h);
            foreach(var b in go.GetComponentsInChildren<MonoBehaviour>(true))if(!(b is WorldHotspot))b.enabled=false;
        }

        static void AddWarmth(Transform root,Vector3 p)
        {
            var go=new GameObject("Valoria · Monumental Arch v2 · warmth");go.transform.SetParent(root,true);go.transform.position=p;
            var l=go.AddComponent<Light>();l.type=LightType.Point;l.color=new Color(1f,.57f,.27f);l.intensity=.14f;l.range=2.7f;l.shadows=LightShadows.None;
        }
    }
}
