using Eldoria.Domain;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Eldoria.Presentation
{
    // VALORIA STAIR / LANDING INTEGRATION v1
    // Existing rescued geometry only. Visual-only overlays; authoritative stair/route remains untouched.
    public static class ValoriaStairLandingIntegrationV1
    {
        public static bool Enabled=false;
        const string RootName="Valoria · Stair Landing Integration v1 · visual only";

        public static int LastInstances{get;private set;}
        public static int LastCollidersDisabled{get;private set;}
        public static int LastBehavioursDisabled{get;private set;}

        public static void Build(Transform parent,PlayerState state)
        {
            if(!Enabled||parent==null||state==null||state.BastionLevel<3)return;
            var old=GameObject.Find(RootName);
            if(old!=null)Object.DestroyImmediate(old);

            LastInstances=0;
            LastCollidersDisabled=0;
            LastBehavioursDisabled=0;

            var root=new GameObject(RootName).transform;
            root.SetParent(parent,true);

            var stairRock=Resources.Load<GameObject>("Valoria/Rescued/TerraceStairRock");
            var landingRock=Resources.Load<GameObject>("Valoria/Rescued/StreetLandingTransition");

            var rockMaterial=ValoriaKit.ExternalPbrSurfaceMaterial(
                "rock",new Color(.52f,.50f,.45f,1f),new Vector2(2.8f,2.8f),.015f,.98f)
                ?? ValoriaKit.DetailedSurfaceMaterial(
                    new Color(.38f,.37f,.33f,1f),"earth",new Vector2(2.8f,2.8f),.96f);

            // Lower stair shoulders: keep a clear central corridor wider than the playable stair.
            AddVisual(root,stairRock,"lower west stair seat",
                new Vector3(-2.35f,.05f,1.55f),2.10f,1.65f,14f,rockMaterial);
            AddVisual(root,stairRock,"lower east stair seat",
                new Vector3(2.42f,.05f,1.72f),2.00f,1.60f,194f,rockMaterial);

            // Mid stair shoulders add rock continuity without covering any tread.
            AddVisual(root,stairRock,"mid west stair seat",
                new Vector3(-2.18f,.58f,3.45f),1.72f,1.45f,22f,rockMaterial);
            AddVisual(root,stairRock,"mid east stair seat",
                new Vector3(2.22f,.60f,3.58f),1.65f,1.40f,202f,rockMaterial);

            // Landing transitions are deliberately off-route. They are visual cheeks, never traversal.
            AddVisual(root,landingRock,"upper west landing cheek",
                new Vector3(-3.15f,1.82f,6.38f),2.00f,1.62f,92f,rockMaterial);
            AddVisual(root,landingRock,"upper east landing cheek",
                new Vector3(3.20f,1.78f,6.48f),1.92f,1.58f,268f,rockMaterial);
        }

        static void AddVisual(Transform root,GameObject source,string role,Vector3 anchor,float span,float maxHeight,float yaw,Material material)
        {
            if(source==null)return;

            var go=Object.Instantiate(source);
            go.name="Valoria · Stair Landing v1 · "+role;
            go.transform.rotation=Quaternion.Euler(0f,yaw,0f);

            var rs=go.GetComponentsInChildren<Renderer>(true);
            if(rs.Length==0){Object.DestroyImmediate(go);return;}

            var b=rs[0].bounds;
            for(int i=1;i<rs.Length;i++)b.Encapsulate(rs[i].bounds);

            float horizontal=Mathf.Max(b.size.x,b.size.z);
            float scale=Mathf.Min(
                span/Mathf.Max(.001f,horizontal),
                maxHeight/Mathf.Max(.001f,b.size.y));
            go.transform.localScale*=scale;

            rs=go.GetComponentsInChildren<Renderer>(true);
            b=rs[0].bounds;
            for(int i=1;i<rs.Length;i++)b.Encapsulate(rs[i].bounds);

            // Anchor to the lowest point so the rock seats into the existing terrain rather than floating.
            go.transform.position+=anchor-new Vector3(b.center.x,b.min.y,b.center.z);

            if(material!=null)
                foreach(var r in rs)r.sharedMaterial=material;

            foreach(var c in go.GetComponentsInChildren<Collider>(true))
            {
                c.enabled=false;
                LastCollidersDisabled++;
            }
            foreach(var behaviour in go.GetComponentsInChildren<MonoBehaviour>(true))
            {
                behaviour.enabled=false;
                LastBehavioursDisabled++;
            }

            go.transform.SetParent(root,true);
            LastInstances++;
        }
    }
}
