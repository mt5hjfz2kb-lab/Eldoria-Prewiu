using Eldoria.Domain;
using UnityEngine;
using Object=UnityEngine.Object;

namespace Eldoria.Presentation
{
    // Visual-only authored structural frame that replaces the largest technical-looking support masses.
    // It never owns floors, routes, hotspots, colliders or camera state.
    public static class ValoriaStructuralFrameUrbanSupportIntegrationV1
    {
        public static bool Enabled=false;
        public const string RootName="Valoria · Structural Frame Urban Support Integration v1";
        public static int PiecesBuilt{get;private set;}
        public static int SuppressedRenderers{get;private set;}

        public static void Build(Transform parent,PlayerState state)
        {
            if(!Enabled||parent==null||state==null||state.BastionLevel<3)return;
            var old=GameObject.Find(RootName);
            if(old!=null)Object.DestroyImmediate(old);

            PiecesBuilt=0;SuppressedRenderers=0;
            SuppressTechnicalSupportMasses();

            var root=new GameObject(RootName).transform;
            root.SetParent(parent,true);

            // Two authored civic retaining bays flank the ceremonial stair.
            Add(root,"CivicRetainingBay","west civic retaining bay",
                new Vector3(-4.85f,.38f,4.72f),4.65f,3.35f,8f);
            Add(root,"CivicRetainingBay","east civic retaining bay",
                new Vector3(4.85f,.38f,4.72f),4.65f,3.35f,172f);

            // One continuous lower arcade replaces the row-of-boxes read without touching the urban groups.
            Add(root,"LowerArcadedFront","lower civic arcade",
                new Vector3(0f,.34f,-5.72f),10.85f,2.85f,0f);

            // Dedicated support below the Cuartel side reads as inhabited civic structure, not a pedestal.
            Add(root,"CuartelTerraceSupport","cuartel terrace support",
                new Vector3(7.35f,.34f,-4.55f),4.55f,2.70f,184f);

            DisableGameplay(root.gameObject);
        }

        static void SuppressTechnicalSupportMasses()
        {
            foreach(var r in Object.FindObjectsByType<Renderer>(FindObjectsSortMode.None))
            {
                if(r==null||!r.enabled||!r.gameObject.activeInHierarchy)continue;
                var n=Hierarchy(r.transform);
                if(!IsTarget(n))continue;
                r.enabled=false;
                SuppressedRenderers++;
            }
        }

        static bool IsTarget(string n)
        {
            return n.Contains("vpd · groundkit l1 retaining edge")||
                   n.Contains("vpd · retaining stone face")||
                   n.Contains("vpd · authored retaining rock")||
                   n.Contains("valoria · compactfootprint · west upper growth shelf")||
                   n.Contains("valoria · compactfootprint · east upper growth shelf")||
                   n.Contains("valoria · compactfootprint · central middle shelf")||
                   n.Contains("valoria · compactfootprint · bastion lower shelf")||
                   n.Contains("valoria · terrainterrace · upper civil support")||
                   n.Contains("valoria · terrainterrace · east upper retaining shelf")||
                   n.Contains("valoria · rescued seam · bastion west shelf")||
                   n.Contains("valoria · rescued seam · bastion east shelf")||
                   n.Contains("valoria · retaining foundation stone");
        }

        static void Add(Transform root,string resource,string role,Vector3 ground,
            float footprint,float maxHeight,float yaw)
        {
            var source=Resources.Load<GameObject>(
                "Valoria/ProductionArt/StructuralFrameUrbanSupportIntegrationV1/"+resource);
            if(source==null)throw new System.InvalidOperationException(
                "Missing authored structural source "+resource);

            var go=Object.Instantiate(source);
            go.name="Valoria · Structural Frame · "+role;
            go.transform.rotation=Quaternion.Euler(0f,yaw,0f);
            var b=Bounds(go);
            float scale=Mathf.Min(
                footprint/Mathf.Max(.001f,Mathf.Max(b.size.x,b.size.z)),
                maxHeight/Mathf.Max(.001f,b.size.y));
            go.transform.localScale*=scale;
            b=Bounds(go);
            go.transform.position+=ground-new Vector3(b.center.x,b.min.y,b.center.z);
            go.transform.SetParent(root,true);
            DisableGameplay(go);
            PiecesBuilt++;
        }

        static Bounds Bounds(GameObject go)
        {
            var rs=go.GetComponentsInChildren<Renderer>(true);
            if(rs.Length==0)return new Bounds(go.transform.position,Vector3.one);
            var b=rs[0].bounds;
            for(int i=1;i<rs.Length;i++)b.Encapsulate(rs[i].bounds);
            return b;
        }

        static string Hierarchy(Transform t)
        {
            string s="";
            for(var p=t;p!=null;p=p.parent)s+="|"+p.name.ToLowerInvariant();
            return s;
        }

        static void DisableGameplay(GameObject go)
        {
            foreach(var c in go.GetComponentsInChildren<Collider>(true))c.enabled=false;
            foreach(var h in go.GetComponentsInChildren<WorldHotspot>(true))Object.DestroyImmediate(h);
            foreach(var b in go.GetComponentsInChildren<MonoBehaviour>(true))
                if(!(b is WorldHotspot))b.enabled=false;
        }
    }
}
