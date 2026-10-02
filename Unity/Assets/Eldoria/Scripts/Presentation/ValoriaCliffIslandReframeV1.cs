using System;
using Eldoria.Domain;
using UnityEngine;
using Object=UnityEngine.Object;

namespace Eldoria.Presentation
{
    // Reframes the visual ground as a compact cliff/island around the playable core.
    // Gameplay floors, reserved plots and colliders remain authoritative and untouched.
    public static class ValoriaCliffIslandReframeV1
    {
        public static bool Enabled=false;
        const string RootName="Valoria · Cliff Island Reframe v1";
        public static int SuppressedGroundRenderers{get;private set;}
        public static int AddedTerracePieces{get;private set;}

        public static void Build(Transform parent,PlayerState state)
        {
            if(!Enabled||parent==null)return;
            var old=GameObject.Find(RootName);if(old!=null)Object.DestroyImmediate(old);
            var root=new GameObject(RootName).transform;root.SetParent(parent,true);
            SuppressedGroundRenderers=0;AddedTerracePieces=0;

            SuppressExact("Valoria · valley floor");
            SuppressExact("VPD · inhabited mountain floor");
            SuppressExact("VPD · lower terrace earth");
            SuppressExact("VPD · west expansion terrain");
            SuppressExact("VPD · east expansion terrain");
            SuppressExact("VPD · west authored apron");
            SuppressExact("VPD · east authored apron");

            BuildLowerIsland(root);
            BuildMidIsland(root);
            BuildUpperIsland(root);
            BuildCliffEdges(root);
        }

        static void BuildLowerIsland(Transform root)
        {
            AddTerrain(root,"BroadRockPlatform","lower core west",new Vector3(-4.6f,0f,-2.6f),.36f,7.2f,16f);
            AddTerrain(root,"BroadRockPlatform","lower core east",new Vector3(4.4f,0f,-2.8f),.36f,7.2f,164f);
            AddTerrain(root,"SteppedRockTerrace","lower approach",new Vector3(0f,0f,-6.0f),.34f,7.6f,90f);
        }

        static void BuildMidIsland(Transform root)
        {
            AddTerrain(root,"BroadRockPlatform","middle west",new Vector3(-4.15f,0f,2.2f),1.35f,5.6f,18f);
            AddTerrain(root,"BroadRockPlatform","middle east",new Vector3(4.10f,0f,2.35f),1.35f,5.6f,162f);
            AddTerrain(root,"SteppedRockTerrace","middle central",new Vector3(0f,0f,3.7f),1.55f,6.4f,90f);
        }

        static void BuildUpperIsland(Transform root)
        {
            AddTerrain(root,"BroadRockPlatform","upper bastion west",new Vector3(-3.4f,0f,6.75f),2.58f,4.8f,8f);
            AddTerrain(root,"BroadRockPlatform","upper bastion east",new Vector3(3.35f,0f,6.85f),2.58f,4.8f,172f);
            AddTerrain(root,"SteppedRockTerrace","upper bastion rear",new Vector3(0f,0f,8.2f),2.60f,6.0f,90f);
        }

        static void BuildCliffEdges(Transform root)
        {
            var art=ValoriaExternalAssetLibrary.Load();
            if(art==null)return;
            var points=new[]{
                new Vector4(-8.3f,-.22f,-5.0f,28f),new Vector4(8.2f,-.22f,-5.1f,208f),
                new Vector4(-8.8f,-.10f,-1.0f,62f),new Vector4(8.7f,-.10f,-1.0f,242f),
                new Vector4(-7.8f,.55f,3.3f,92f),new Vector4(7.7f,.55f,3.4f,272f),
                new Vector4(-6.2f,1.58f,6.5f,118f),new Vector4(6.1f,1.58f,6.6f,298f)
            };
            for(int i=0;i<points.Length;i++)
            {
                var p=points[i];
                var source=i%2==0?art.SlavicBoulder:art.SlavicFlatRock;
                if(source==null)source=art.SlavicBoulder??art.SlavicFlatRock;
                if(source==null)continue;
                var go=ValoriaKit.BenchmarkPieceIntegrated(
                    "Valoria · Cliff Island · edge "+i,source,new Vector3(p.x,p.y,p.z),
                    i<4?3.6f:3.0f,i<4?1.8f:1.45f,Quaternion.Euler(0f,p.w,0f),
                    new Color(.52f,.53f,.48f,1f));
                if(go==null)continue;
                go.transform.SetParent(root,true);DisableGameplay(go);AddedTerracePieces++;
            }
        }

        static void AddTerrain(Transform root,string resource,string role,Vector3 anchor,float topY,float span,float yaw)
        {
            var source=Resources.Load<GameObject>("Valoria/TerrainTerraceKit_v1/"+resource);
            if(source==null)return;
            var go=Object.Instantiate(source);go.name="Valoria · Cliff Island · "+role;
            go.transform.rotation=Quaternion.Euler(0f,yaw,0f);
            var b=Bounds(go);
            go.transform.localScale*=span/Mathf.Max(.001f,Mathf.Max(b.size.x,b.size.z));
            b=Bounds(go);
            go.transform.position+=new Vector3(anchor.x-b.center.x,topY-b.max.y,anchor.z-b.center.z);

            // TerrainTerraceKit prefabs may retain very dark imported materials. These large
            // presentation-only shelves dominate the official camera, so force the same
            // neutral PBR rock family used by the rest of Valoria instead of inheriting
            // importer color factors.
            var rock=ValoriaKit.ExternalPbrSurfaceMaterial(
                "rock",new Color(.60f,.59f,.54f,1f),new Vector2(3.4f,3.4f),.025f,1.04f)
                ?? ValoriaKit.SurfaceMaterial(new Color(.55f,.54f,.50f,1f),"stone",new Vector2(3.4f,3.4f));
            foreach(var renderer in go.GetComponentsInChildren<Renderer>(true))
            {
                var mats=renderer.sharedMaterials;
                for(int i=0;i<mats.Length;i++)mats[i]=rock;
                renderer.sharedMaterials=mats;
            }

            go.transform.SetParent(root,true);DisableGameplay(go);AddedTerracePieces++;
        }

        static void SuppressExact(string exact)
        {
            foreach(var r in Object.FindObjectsByType<Renderer>(FindObjectsSortMode.None))
            {
                if(r==null||!r.enabled||!r.gameObject.activeInHierarchy)continue;
                for(var t=r.transform;t!=null;t=t.parent)
                {
                    if(!string.Equals(t.name,exact,StringComparison.Ordinal))continue;
                    r.enabled=false;SuppressedGroundRenderers++;break;
                }
            }
        }

        static Bounds Bounds(GameObject go)
        {
            var rs=go.GetComponentsInChildren<Renderer>(true);
            if(rs.Length==0)return new Bounds(go.transform.position,Vector3.one);
            var b=rs[0].bounds;for(int i=1;i<rs.Length;i++)b.Encapsulate(rs[i].bounds);return b;
        }

        static void DisableGameplay(GameObject go)
        {
            foreach(var c in go.GetComponentsInChildren<Collider>(true))c.enabled=false;
            foreach(var h in go.GetComponentsInChildren<WorldHotspot>(true))Object.DestroyImmediate(h);
            foreach(var b in go.GetComponentsInChildren<MonoBehaviour>(true))if(!(b is WorldHotspot))b.enabled=false;
        }
    }
}
