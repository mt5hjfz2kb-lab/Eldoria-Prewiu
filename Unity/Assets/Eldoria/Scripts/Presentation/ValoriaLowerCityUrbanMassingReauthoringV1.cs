using Eldoria.Domain;
using UnityEngine;
using Object=UnityEngine.Object;

namespace Eldoria.Presentation
{
    // Screen-first reauthoring of the visible lower-city urban band.
    // Visual-only: it suppresses redundant presentation layers and rebuilds three authored groups.
    public static class ValoriaLowerCityUrbanMassingReauthoringV1
    {
        public static bool Enabled=false;
        public const string RootName="Valoria · Lower-City Urban Massing Reauthoring v1";
        public static int GroupsBuilt{get;private set;}
        public static int PiecesBuilt{get;private set;}
        public static int SuppressedRenderers{get;private set;}
        public static int SuppressedLights{get;private set;}

        public static void Build(Transform parent,PlayerState state)
        {
            if(!Enabled||parent==null||state==null||state.BastionLevel<3)return;
            var old=GameObject.Find(RootName);
            if(old!=null)Object.DestroyImmediate(old);

            GroupsBuilt=0;PiecesBuilt=0;SuppressedRenderers=0;SuppressedLights=0;
            SuppressRedundantUrbanLayers();

            var root=new GameObject(RootName).transform;
            root.SetParent(parent,true);

            BuildWestCraftCourt(root);
            BuildEastMerchantFront(root);
            BuildUpperTerraceGroup(root);

            DisableGameplay(root.gameObject);
        }

        static void SuppressRedundantUrbanLayers()
        {
            foreach(var r in Object.FindObjectsByType<Renderer>(FindObjectsSortMode.None))
            {
                if(r==null||!r.enabled||!r.gameObject.activeInHierarchy)continue;
                if(!IsRedundantUrbanLayer(Hierarchy(r.transform)))continue;
                r.enabled=false;SuppressedRenderers++;
            }
            foreach(var l in Object.FindObjectsByType<Light>(FindObjectsSortMode.None))
            {
                if(l==null||!l.enabled||!l.gameObject.activeInHierarchy)continue;
                if(!IsRedundantUrbanLayer(Hierarchy(l.transform)))continue;
                l.enabled=false;SuppressedLights++;
            }
        }

        static bool IsRedundantUrbanLayer(string n)
        {
            return n.Contains("valoria · reused civil house")||
                   n.Contains("valoria · mid-tier district v1 · production visual only")||
                   n.Contains("valoria · full frame architecture batch v1");
        }

        static void BuildWestCraftCourt(Transform parent)
        {
            GroupsBuilt++;
            var center=new Vector3(-11.35f,.38f,-2.75f);
            const float yaw=14f;
            var g=NewGroup(parent,"01 · west craft court");

            AddTerrain(g,"BroadRockPlatform",center,.40f,5.45f,yaw+84f);
            AddTerrain(g,"SteppedRockTerrace",center+Rot(yaw,new Vector3(-1.78f,-.03f,.55f)),.41f,2.55f,yaw-4f);

            // Broad, low frontage. The court stays open toward the central stair.
            AddPiece(g,"Piece03","west court main hall",center+Rot(yaw,new Vector3(-.45f,.04f,-.32f)),
                3.25f,3.45f,yaw,new Color(.66f,.60f,.51f,1f),false);
            AddPiece(g,"Piece01","west court arched house",center+Rot(yaw,new Vector3(-2.05f,.03f,.62f)),
                1.80f,2.10f,yaw+18f,new Color(.72f,.67f,.58f,1f),false);
            AddPiece(g,"Piece02","west court low residence",center+Rot(yaw,new Vector3(.98f,.03f,-.62f)),
                2.25f,2.55f,yaw-16f,new Color(.71f,.66f,.57f,1f),false);

            AddStone(g,"RockToWallTransition","west court buried seam",
                center+Rot(yaw,new Vector3(-2.28f,.01f,-.72f)),1.65f,yaw+126f);
        }

        static void BuildEastMerchantFront(Transform parent)
        {
            GroupsBuilt++;
            var center=new Vector3(6.15f,.40f,-2.55f);
            const float yaw=188f;
            var g=NewGroup(parent,"02 · east merchant front");

            AddTerrain(g,"BroadRockPlatform",center,.42f,5.70f,yaw+90f);

            // One stronger urban frontage instead of two compressed equal-height clusters.
            AddPiece(g,"Piece02","merchant tall core",center+Rot(yaw,new Vector3(.18f,.04f,-.38f)),
                3.55f,4.05f,yaw,new Color(.72f,.67f,.58f,1f),false);
            AddPiece(g,"Piece03","merchant side house",center+Rot(yaw,new Vector3(-2.05f,.03f,.34f)),
                2.20f,2.65f,yaw+17f,new Color(.63f,.57f,.49f,1f),false);
            AddPiece(g,"Piece01","merchant recessed front",center+Rot(yaw,new Vector3(1.72f,.03f,.88f)),
                1.75f,2.00f,yaw-8f,new Color(.70f,.65f,.56f,1f),false);
            // A single dark roof crown creates a vertical accent without another orange roof repeat.
            AddPiece(g,"Piece04","merchant slate crown",center+Rot(yaw,new Vector3(.50f,2.15f,-.35f)),
                2.15f,1.62f,yaw+4f,new Color(.31f,.34f,.36f,1f),true);

            AddStone(g,"RockToWallTransition","merchant buried seam",
                center+Rot(yaw,new Vector3(2.12f,.01f,-.86f)),1.72f,yaw-58f);
        }

        static void BuildUpperTerraceGroup(Transform parent)
        {
            GroupsBuilt++;
            var center=new Vector3(-6.35f,.73f,2.65f);
            const float yaw=24f;
            var g=NewGroup(parent,"03 · upper terrace houses");

            AddTerrain(g,"SteppedRockTerrace",center,.76f,4.45f,yaw+92f);

            // Smaller and taller than west court, clearly separated by a lane.
            AddPiece(g,"Piece02","upper terrace vertical house",center+Rot(yaw,new Vector3(.15f,.03f,-.24f)),
                2.80f,3.55f,yaw,new Color(.70f,.65f,.56f,1f),false);
            AddPiece(g,"Piece03","upper terrace side wing",center+Rot(yaw,new Vector3(-1.63f,.02f,.38f)),
                1.82f,2.15f,yaw+22f,new Color(.62f,.56f,.48f,1f),false);
            AddPiece(g,"Piece01","upper terrace entry",center+Rot(yaw,new Vector3(1.22f,.02f,.72f)),
                1.42f,1.72f,yaw-10f,new Color(.71f,.67f,.59f,1f),false);
            AddPiece(g,"Piece04","upper terrace slate cap",center+Rot(yaw,new Vector3(.18f,1.88f,-.22f)),
                1.62f,1.28f,yaw-6f,new Color(.30f,.33f,.35f,1f),true);

            AddStone(g,"RockToWallTransition","upper terrace rock seam",
                center+Rot(yaw,new Vector3(-1.80f,.00f,-.62f)),1.48f,yaw+128f);
        }

        static Transform NewGroup(Transform parent,string id)
        {
            var g=new GameObject("Valoria · Urban Group · "+id).transform;
            g.SetParent(parent,true);
            return g;
        }

        static Vector3 Rot(float yaw,Vector3 v)=>Quaternion.Euler(0f,yaw,0f)*v;

        static void AddPiece(Transform root,string resource,string role,Vector3 anchor,
            float span,float maxHeight,float yaw,Color tint,bool forceSlate)
        {
            var source=Resources.Load<GameObject>("Valoria/MidTierArchitectureKit_v1/"+resource);
            if(source==null)throw new System.InvalidOperationException("Missing MidTier source "+resource);
            var go=Object.Instantiate(source);
            go.name="Valoria · Urban Massing · "+role;
            go.transform.rotation=Quaternion.Euler(0f,yaw,0f);
            Fit(go,anchor,span,maxHeight);
            Tint(go,tint,forceSlate);
            go.transform.SetParent(root,true);
            DisableGameplay(go);
            PiecesBuilt++;
        }

        static void AddTerrain(Transform root,string resource,Vector3 anchor,float topY,float span,float yaw)
        {
            var source=Resources.Load<GameObject>("Valoria/TerrainTerraceKit_v1/"+resource);
            if(source==null)throw new System.InvalidOperationException("Missing TerrainTerrace source "+resource);
            var go=Object.Instantiate(source);
            go.name="Valoria · Urban Massing · buried "+resource;
            go.transform.rotation=Quaternion.Euler(0f,yaw,0f);
            var b=Bounds(go);
            go.transform.localScale*=span/Mathf.Max(.001f,Mathf.Max(b.size.x,b.size.z));
            b=Bounds(go);
            go.transform.position+=new Vector3(anchor.x-b.center.x,topY-b.max.y,anchor.z-b.center.z);
            go.transform.SetParent(root,true);
            DisableGameplay(go);
            PiecesBuilt++;
        }

        static void AddStone(Transform root,string resource,string role,Vector3 anchor,float span,float yaw)
        {
            var source=Resources.Load<GameObject>("Valoria/StoneArchitectureKit_v1/"+resource);
            if(source==null)throw new System.InvalidOperationException("Missing StoneArchitecture source "+resource);
            var go=Object.Instantiate(source);
            go.name="Valoria · Urban Massing · "+role;
            go.transform.rotation=Quaternion.Euler(0f,yaw,0f);
            var b=Bounds(go);
            go.transform.localScale*=span/Mathf.Max(.001f,Mathf.Max(b.size.x,b.size.z));
            b=Bounds(go);
            go.transform.position+=anchor-new Vector3(b.center.x,b.min.y,b.center.z);
            Tint(go,new Color(.63f,.62f,.57f,1f),false);
            go.transform.SetParent(root,true);
            DisableGameplay(go);
            PiecesBuilt++;
        }

        static void Fit(GameObject go,Vector3 anchor,float span,float maxHeight)
        {
            var b=Bounds(go);
            float scale=Mathf.Min(span/Mathf.Max(.001f,Mathf.Max(b.size.x,b.size.z)),
                                  maxHeight/Mathf.Max(.001f,b.size.y));
            go.transform.localScale*=scale;
            b=Bounds(go);
            go.transform.position+=anchor-new Vector3(b.center.x,b.min.y,b.center.z);
        }

        static void Tint(GameObject go,Color tint,bool forceSlate)
        {
            foreach(var r in go.GetComponentsInChildren<Renderer>(true))
            {
                var src=r.sharedMaterials;
                var dst=new Material[src.Length];
                for(int i=0;i<src.Length;i++)
                {
                    if(src[i]==null){dst[i]=null;continue;}
                    var m=new Material(src[i]){name="Valoria urban massing · "+src[i].name};
                    var c=forceSlate?new Color(.30f,.34f,.36f,1f):tint;
                    if(m.HasProperty("_BaseColor"))m.SetColor("_BaseColor",c);
                    if(m.HasProperty("_Color"))m.SetColor("_Color",c);
                    if(m.HasProperty("_BaseColorFactor"))m.SetColor("_BaseColorFactor",c.linear);
                    if(m.HasProperty("_Smoothness"))m.SetFloat("_Smoothness",.025f);
                    dst[i]=m;
                }
                r.sharedMaterials=dst;
            }
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
