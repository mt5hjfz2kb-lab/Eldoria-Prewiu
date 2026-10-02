using System;
using Eldoria.Domain;
using UnityEngine;
using Object=UnityEngine.Object;

namespace Eldoria.Presentation
{
    // Removes visual-only legacy/provisional residues exposed by the compact cliff-island reframe.
    // Keeps gameplay floors, colliders, hotspots, routes and reservations untouched.
    public static class ValoriaResidualCleanupV1
    {
        public static bool Enabled=false;
        const string RootName="Valoria · Residual Cleanup v1";
        public static int Suppressed{get;private set;}
        public static int Replacements{get;private set;}

        public static void Build(Transform parent,PlayerState state)
        {
            if(!Enabled||parent==null)return;
            var old=GameObject.Find(RootName);if(old!=null)Object.DestroyImmediate(old);
            var root=new GameObject(RootName).transform;root.SetParent(parent,true);
            Suppressed=0;Replacements=0;

            // The legacy upper dwelling is the dominant near-black block right of the Bastion.
            var upper=FindPresentationBounds("VPD · upper dwelling");
            SuppressChainContains("VPD · upper dwelling");
            if(upper.size.sqrMagnitude>.01f)
            {
                var art=ValoriaExternalAssetLibrary.Load();
                var source=art!=null?art.SlavicHouse:null;
                if(source==null)source=Resources.Load<GameObject>("Valoria/MidTierArchitectureKit_v1/Piece02");
                if(source!=null)
                {
                    var ground=new Vector3(upper.center.x,upper.min.y,upper.center.z);
                    var go=ValoriaKit.BenchmarkPieceIntegrated(
                        "Valoria · Residual Cleanup · upper residence",
                        source,ground,
                        Mathf.Clamp(Mathf.Max(upper.size.x,upper.size.z)*.82f,1.8f,3.0f),
                        Mathf.Clamp(upper.size.y*.88f,1.8f,3.4f),
                        Quaternion.Euler(0f,204f,0f),
                        new Color(.68f,.62f,.53f,1f));
                    if(go!=null)
                    {
                        go.transform.SetParent(root,true);
                        DisableGameplay(go);
                        Replacements++;
                    }
                }
            }

            // Remove primitive sphere-rock clusters now exposed against the photographic/painted valley.
            foreach(var token in new[]{
                "VPD · terrain seam · rock",
                "VPD · expansion edge geology · rock",
                "VPD · stair shoulder rock · rock",
                "Ruined imperial arch fall · rock",
                "Valoria · terrain seam · rock"
            }) SuppressNameContains(token);

            // Remove obsolete placeholder primitives/silhouettes that read as black cards at strategic zoom.
            foreach(var token in new[]{
                "Valoria · worker silhouette · placeholder",
                "Valoria · worker head · placeholder"
            }) SuppressNameContains(token);

            // Replace only a few edge rocks with real authored project rocks to preserve grounding.
            var assets=ValoriaExternalAssetLibrary.Load();
            if(assets!=null)
            {
                AddRock(root,assets.SlavicBoulder??assets.SlavicFlatRock,"lower west",new Vector3(-8.2f,-.10f,-3.8f),2.5f,1.15f,28f);
                AddRock(root,assets.SlavicFlatRock??assets.SlavicBoulder,"lower east",new Vector3(8.1f,-.10f,-4.0f),2.5f,1.10f,208f);
                AddRock(root,assets.SlavicBoulder??assets.SlavicFlatRock,"mid west",new Vector3(-7.2f,.48f,2.4f),2.2f,1.0f,72f);
                AddRock(root,assets.SlavicFlatRock??assets.SlavicBoulder,"mid east",new Vector3(7.1f,.48f,2.5f),2.2f,1.0f,252f);
            }
        }

        static Bounds FindPresentationBounds(string exact)
        {
            bool any=false;Bounds b=new Bounds();
            foreach(var r in Object.FindObjectsByType<Renderer>(FindObjectsSortMode.None))
            {
                if(r==null||!r.enabled||!r.gameObject.activeInHierarchy)continue;
                for(var t=r.transform;t!=null;t=t.parent)
                {
                    if(!string.Equals(t.name,exact,StringComparison.Ordinal))continue;
                    if(!any){b=r.bounds;any=true;}else b.Encapsulate(r.bounds);
                    break;
                }
            }
            return any?b:new Bounds(Vector3.zero,Vector3.zero);
        }

        static void SuppressChainContains(string token)
        {
            foreach(var r in Object.FindObjectsByType<Renderer>(FindObjectsSortMode.None))
            {
                if(r==null||!r.enabled||!r.gameObject.activeInHierarchy)continue;
                bool match=false;
                for(var t=r.transform;t!=null;t=t.parent)
                    if(t.name.IndexOf(token,StringComparison.OrdinalIgnoreCase)>=0){match=true;break;}
                if(match){r.enabled=false;Suppressed++;}
            }
        }

        static void SuppressNameContains(string token)
        {
            foreach(var r in Object.FindObjectsByType<Renderer>(FindObjectsSortMode.None))
            {
                if(r==null||!r.enabled||!r.gameObject.activeInHierarchy)continue;
                if(r.gameObject.name.IndexOf(token,StringComparison.OrdinalIgnoreCase)<0)continue;
                r.enabled=false;Suppressed++;
            }
        }

        static void AddRock(Transform root,GameObject source,string role,Vector3 p,float footprint,float height,float yaw)
        {
            if(source==null)return;
            var go=ValoriaKit.BenchmarkPieceIntegrated("Valoria · Residual Cleanup · rock "+role,source,p,footprint,height,
                Quaternion.Euler(0f,yaw,0f),new Color(.51f,.52f,.48f,1f));
            if(go==null)return;

            var rockMaterial=ValoriaKit.ExternalPbrSurfaceMaterial(
                "rock",new Color(.53f,.53f,.49f,1f),new Vector2(2.8f,2.8f),.025f,1.02f)
                ?? ValoriaKit.SurfaceMaterial(new Color(.50f,.50f,.46f,1f),"stone",new Vector2(2.8f,2.8f));
            foreach(var renderer in go.GetComponentsInChildren<Renderer>(true))
            {
                var mats=renderer.sharedMaterials;
                for(int i=0;i<mats.Length;i++)mats[i]=rockMaterial;
                renderer.sharedMaterials=mats;
            }

            go.transform.SetParent(root,true);DisableGameplay(go);Replacements++;
        }

        static void DisableGameplay(GameObject go)
        {
            foreach(var c in go.GetComponentsInChildren<Collider>(true))c.enabled=false;
            foreach(var h in go.GetComponentsInChildren<WorldHotspot>(true))Object.DestroyImmediate(h);
            foreach(var b in go.GetComponentsInChildren<MonoBehaviour>(true))if(!(b is WorldHotspot))b.enabled=false;
        }
    }
}
