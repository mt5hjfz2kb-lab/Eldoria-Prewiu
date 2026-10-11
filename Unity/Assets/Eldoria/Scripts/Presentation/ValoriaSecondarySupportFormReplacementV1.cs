using Eldoria.Domain;
using UnityEngine;
using Object=UnityEngine.Object;

namespace Eldoria.Presentation
{
    // Visual-only replacement of large weak support masses and repetitive lower-city presentation.
    // The pass is deliberately subtractive where repetition is the defect: fewer, stronger masses.
    public static class ValoriaSecondarySupportFormReplacementV1
    {
        public static bool Enabled=false;
        public const string RootName="Valoria · Secondary Support Form Replacement v1";
        public static int PiecesBuilt{get;private set;}
        public static int SuppressedRenderers{get;private set;}
        public static int HousingRenderersSuppressed{get;private set;}

        public static void Build(Transform parent,PlayerState state)
        {
            if(!Enabled||parent==null||state==null)return;
            var old=GameObject.Find(RootName);
            if(old!=null)Object.DestroyImmediate(old);
            var root=new GameObject(RootName).transform;
            root.SetParent(parent,true);
            PiecesBuilt=0;
            SuppressedRenderers=0;
            HousingRenderersSuppressed=0;

            SuppressWeakSupport();
            ArticulateExistingWestHousing();
            BuildCertifiedSupportArchitecture(root);

            foreach(var c in root.GetComponentsInChildren<Collider>(true))c.enabled=false;
            foreach(var h in root.GetComponentsInChildren<WorldHotspot>(true))Object.DestroyImmediate(h);
        }

        static void SuppressWeakSupport()
        {
            foreach(var r in Object.FindObjectsByType<Renderer>(FindObjectsSortMode.None))
            {
                if(r==null||!r.enabled||!r.gameObject.activeInHierarchy)continue;
                var chain=Chain(r.transform);
                if(chain.Contains("vpd · groundkit l1 retaining edge")||
                   chain.Contains("vpd · retaining stone face")||
                   chain.Contains("vpd · authored retaining rock"))
                {
                    r.enabled=false;
                    SuppressedRenderers++;
                }
            }
        }

        static void ArticulateExistingWestHousing()
        {
            foreach(var r in Object.FindObjectsByType<Renderer>(FindObjectsSortMode.None))
            {
                if(r==null||!r.enabled||!r.gameObject.activeInHierarchy)continue;
                var chain=Chain(r.transform);

                // The procedural bodies remain underneath the already-authored reused houses in baseline,
                // so the same footprint reads twice. Remove those duplicate underlays completely.
                bool duplicateUnderlay=
                    chain.Contains("vpd · west rebuilders home")||
                    chain.Contains("vpd · west rebuilders upper dwelling");

                // Keep four authored houses but remove two immediate neighbours from the compressed row.
                // The resulting negative space becomes two readable courts/lanes, not empty city acreage.
                bool crowdedAuthored=
                    chain.Contains("valoria · reused civil house 1")||
                    chain.Contains("valoria · reused civil house 3");

                if(!duplicateUnderlay&&!crowdedAuthored)continue;
                r.enabled=false;
                SuppressedRenderers++;
                HousingRenderersSuppressed++;
            }
        }

        static void BuildCertifiedSupportArchitecture(Transform root)
        {
            var highWall=Resources.Load<GameObject>("Valoria/StoneArchitectureKit_v1/HighStraightWall");
            var corner=Resources.Load<GameObject>("Valoria/StoneArchitectureKit_v1/CornerWallL");
            var transition=Resources.Load<GameObject>("Valoria/StoneArchitectureKit_v1/RockToWallTransition");
            var terraceRock=Resources.Load<GameObject>("Valoria/Rescued/ResidentialTerraceRock");

            // Two large structural bays flank the main stair instead of a serial row of little faces.
            Add(root,highWall,"west structural bay",new Vector3(-4.95f,.40f,4.72f),3.35f,2.55f,1f);
            Add(root,highWall,"east structural bay",new Vector3(4.95f,.40f,4.72f),3.35f,2.55f,179f);

            // Corners give the terrace a deliberate architectural termination without closing the stair.
            Add(root,corner,"west terrace return",new Vector3(-7.15f,.34f,4.90f),2.30f,2.35f,108f);
            Add(root,corner,"east terrace return",new Vector3(7.15f,.34f,4.90f),2.30f,2.35f,252f);

            // Certified wall-to-rock transitions bury those architectural ends into the mountain mass.
            Add(root,transition,"west architecture-rock seam",new Vector3(-8.55f,.20f,5.20f),2.55f,2.00f,42f);
            Add(root,transition,"east architecture-rock seam",new Vector3(8.55f,.20f,5.20f),2.55f,2.00f,222f);

            // One broad rescued terrace rock supports the upper-west quarter as a single mass rather
            // than several small decorative rocks. It owns no route/floor/collision.
            Add(root,terraceRock,"west quarter buried terrace mass",new Vector3(-16.10f,.16f,4.70f),
                4.65f,1.55f,14f);
        }

        static void Add(Transform root,GameObject source,string role,Vector3 ground,
            float footprint,float maxHeight,float yaw)
        {
            if(source==null)return;
            var go=ValoriaKit.BenchmarkPiece("Valoria · SSR v1 · "+role,source,ground,
                footprint,maxHeight,Quaternion.Euler(0f,yaw,0f));
            if(go==null)return;
            go.transform.SetParent(root,true);
            foreach(var c in go.GetComponentsInChildren<Collider>(true))c.enabled=false;
            foreach(var h in go.GetComponentsInChildren<WorldHotspot>(true))Object.DestroyImmediate(h);
            PiecesBuilt++;
        }

        static string Chain(Transform t)
        {
            string s="";
            for(var p=t;p!=null;p=p.parent)s+="|"+p.name.ToLowerInvariant();
            return s;
        }
    }
}
