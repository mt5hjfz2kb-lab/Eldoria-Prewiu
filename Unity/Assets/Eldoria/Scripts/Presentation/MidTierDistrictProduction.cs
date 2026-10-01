using System;
using System.Collections.Generic;
using Eldoria.Domain;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Eldoria.Presentation
{
    // Production visual-only rollout of the certified Mid-Tier three-parcel sector.
    // It never owns gameplay topology, colliders, hotspots or progression.
    public static class MidTierDistrictProduction
    {
        public static bool Enabled=true;
        static readonly Dictionary<int,Material> sharedSurfaceMaterials=new();

        struct Parcel
        {
            public string id; public Vector3 center; public float yaw;
            public Parcel(string id,Vector3 center,float yaw){this.id=id;this.center=center;this.yaw=yaw;}
        }

        static readonly Parcel[] Parcels={
            new Parcel("D1",new Vector3(7.20f,.42f,-4.00f),180f),
            new Parcel("D2",new Vector3(11.60f,.34f,-2.25f),194f),
            new Parcel("D3",new Vector3(14.80f,.34f,1.95f),166f)
        };

        public static void Build(Transform parent,PlayerState state)
        {
            // The certified comparison was made at Bastion III. Do not leak this dressing into earlier progression states.
            if(!Enabled||parent==null||state==null||state.BastionLevel<3)return;
            HideLegacyParcelRenderers();

            var root=new GameObject("Valoria · Mid-Tier District v1 · production visual only").transform;
            root.SetParent(parent,true);
            foreach(var p in Parcels)BuildParcel(root,p);
            DisableGameplay(root.gameObject);
        }

        static void BuildParcel(Transform parent,Parcel p)
        {
            var root=new GameObject("Valoria Mid-Tier · "+p.id).transform;root.SetParent(parent,true);
            AddTerrain(root,"BroadRockPlatform",p.center,p.center.y+.02f,p.id=="D1"?5.10f:p.id=="D2"?4.85f:4.35f,p.id=="D1"?92f:p.id=="D2"?205f:174f);
            AddTerrain(root,"SteppedRockTerrace",p.center+Rotated(p.yaw,new Vector3(-1.05f,-.02f,-.30f)),p.center.y+.01f,2.85f,p.yaw+90f);

            if(p.id=="D1")
            {
                AddPiece(root,"Piece02","inhabited core",p.center+Rotated(p.yaw,new Vector3(.15f,.05f,-.12f)),3.55f,4.15f,p.yaw,new Color(.73f,.69f,.61f,1f));
                AddPiece(root,"Piece03","workshop wing",p.center+Rotated(p.yaw,new Vector3(-2.05f,.03f,-.18f)),2.65f,3.05f,p.yaw+8f,new Color(.64f,.58f,.49f,1f));
                AddPiece(root,"Piece04","roof crown",p.center+Rotated(p.yaw,new Vector3(.55f,2.28f,-.18f)),2.65f,2.20f,p.yaw+2f,new Color(.70f,.64f,.55f,1f));
                AddPiece(root,"Piece01","arched porch",p.center+Rotated(p.yaw,new Vector3(.25f,.04f,1.72f)),1.95f,2.20f,p.yaw,new Color(.72f,.68f,.60f,1f));
                AddStone(root,"HighStraightWall","rear masonry spine",p.center+Rotated(p.yaw,new Vector3(.55f,.06f,-1.45f)),2.55f,p.yaw+3f);
                AddStone(root,"RockToWallTransition","rock seam",p.center+Rotated(p.yaw,new Vector3(1.55f,.04f,-.48f)),1.95f,p.yaw-58f);
            }
            else if(p.id=="D2")
            {
                AddPiece(root,"Piece03","civic core",p.center+Rotated(p.yaw,new Vector3(0f,.04f,-.05f)),3.20f,3.70f,p.yaw,new Color(.69f,.63f,.54f,1f));
                AddPiece(root,"Piece02","side residence",p.center+Rotated(p.yaw,new Vector3(1.72f,.03f,.15f)),2.45f,3.15f,p.yaw-10f,new Color(.75f,.70f,.62f,1f));
                AddPiece(root,"Piece04","offset roof",p.center+Rotated(p.yaw,new Vector3(-.42f,2.05f,-.12f)),2.30f,1.95f,p.yaw+12f,new Color(.66f,.60f,.51f,1f));
                AddPiece(root,"Piece01","recessed entry",p.center+Rotated(p.yaw,new Vector3(-.62f,.03f,1.42f)),1.65f,1.95f,p.yaw+4f,new Color(.70f,.66f,.58f,1f));
                AddStone(root,"RockToWallTransition","rock seam",p.center+Rotated(p.yaw,new Vector3(1.55f,.04f,-.48f)),1.95f,p.yaw-58f);
                AddStone(root,"CornerWallL","corner retaining mass",p.center+Rotated(p.yaw,new Vector3(1.18f,.04f,-.35f)),1.80f,p.yaw+88f);
            }
            else
            {
                AddPiece(root,"Piece02","compact core",p.center+Rotated(p.yaw,new Vector3(.05f,.04f,-.08f)),2.95f,3.45f,p.yaw,new Color(.70f,.66f,.58f,1f));
                AddPiece(root,"Piece04","tall roofline",p.center+Rotated(p.yaw,new Vector3(.30f,1.95f,-.10f)),2.15f,2.05f,p.yaw-7f,new Color(.67f,.61f,.52f,1f));
                AddPiece(root,"Piece03","lower service wing",p.center+Rotated(p.yaw,new Vector3(-1.55f,.03f,.18f)),2.10f,2.45f,p.yaw+16f,new Color(.62f,.56f,.47f,1f));
                AddPiece(root,"Piece01","fortified entry",p.center+Rotated(p.yaw,new Vector3(.45f,.03f,1.30f)),1.55f,1.85f,p.yaw-3f,new Color(.71f,.67f,.59f,1f));
                AddStone(root,"HighStraightWall","rear masonry spine",p.center+Rotated(p.yaw,new Vector3(.55f,.06f,-1.45f)),2.55f,p.yaw+3f);
                AddStone(root,"CornerWallL","corner retaining mass",p.center+Rotated(p.yaw,new Vector3(1.18f,.04f,-.35f)),1.80f,p.yaw+88f);
            }
            AddWarmth(root,p.center+Rotated(p.yaw,new Vector3(.15f,1.25f,1.18f)),.18f,2.15f);
        }

        static Vector3 Rotated(float yaw,Vector3 offset)=>Quaternion.Euler(0,yaw,0)*offset;

        static void HideLegacyParcelRenderers()
        {
            foreach(var r in Object.FindObjectsByType<Renderer>(FindObjectsSortMode.None))
            {
                if(r==null||!r.enabled||!r.gameObject.activeInHierarchy)continue;
                string n=HierarchyName(r.transform);
                bool named=n.Contains("Cuartel · dedicated barracks",StringComparison.OrdinalIgnoreCase)||
                    n.Contains("Cuartel · fallback guardhouse",StringComparison.OrdinalIgnoreCase)||
                    n.Contains("VPD · west rebuilders home",StringComparison.OrdinalIgnoreCase)||
                    n.Contains("VPD · west rebuilders upper dwelling",StringComparison.OrdinalIgnoreCase)||
                    n.Contains("Valoria lower-town home",StringComparison.OrdinalIgnoreCase)||
                    n.Contains("Rebuilder shelter",StringComparison.OrdinalIgnoreCase)||
                    n.Contains("Valoria · reused civil house",StringComparison.OrdinalIgnoreCase)||
                    n.Contains("Valoria · hero frame inhabited roofline",StringComparison.OrdinalIgnoreCase);
                if(named&&NearDistrict(r.bounds.center,4.25f))r.enabled=false;
            }
        }

        static bool NearDistrict(Vector3 p,float radius)
        {
            float r2=radius*radius;
            foreach(var s in Parcels){var d=new Vector2(p.x-s.center.x,p.z-s.center.z);if(d.sqrMagnitude<=r2)return true;}
            return false;
        }

        static void AddPiece(Transform root,string resource,string role,Vector3 anchor,float span,float maxHeight,float yaw,Color tint)
        {
            var source=Resources.Load<GameObject>("Valoria/MidTierArchitectureKit_v1/"+resource);
            if(source==null)throw new InvalidOperationException("Missing production Mid-Tier resource "+resource);
            var go=Object.Instantiate(source);go.name="Valoria Mid-Tier · "+role;go.transform.rotation=Quaternion.Euler(0,yaw,0);
            Fit(go,anchor,span,maxHeight);ApplySharedSurface(go,tint);go.transform.SetParent(root,true);DisableGameplay(go);
        }

        static void AddTerrain(Transform root,string resource,Vector3 anchor,float topY,float span,float yaw)
        {
            var source=Resources.Load<GameObject>("Valoria/TerrainTerraceKit_v1/"+resource);
            if(source==null)throw new InvalidOperationException("Missing Terrain Terrace resource "+resource);
            var go=Object.Instantiate(source);go.name="Valoria Mid-Tier · buried "+resource;go.transform.rotation=Quaternion.Euler(0,yaw,0);
            var b=Bounds(go);go.transform.localScale*=span/Mathf.Max(b.size.x,b.size.z);b=Bounds(go);
            go.transform.position+=new Vector3(anchor.x-b.center.x,topY-b.max.y,anchor.z-b.center.z);go.transform.SetParent(root,true);DisableGameplay(go);
        }

        static void AddStone(Transform root,string resource,string role,Vector3 anchor,float span,float yaw)
        {
            var source=Resources.Load<GameObject>("Valoria/StoneArchitectureKit_v1/"+resource);
            if(source==null)throw new InvalidOperationException("Missing Stone Architecture resource "+resource);
            var go=Object.Instantiate(source);go.name="Valoria Mid-Tier · "+role;go.transform.rotation=Quaternion.Euler(0,yaw,0);
            var b=Bounds(go);go.transform.localScale*=span/Mathf.Max(b.size.x,b.size.z);b=Bounds(go);
            go.transform.position+=anchor-new Vector3(b.center.x,b.min.y,b.center.z);ApplySharedSurface(go,new Color(.66f,.64f,.59f,1f));
            go.transform.SetParent(root,true);DisableGameplay(go);
        }

        static void Fit(GameObject go,Vector3 anchor,float span,float maxHeight)
        {
            var b=Bounds(go);float scale=Mathf.Min(span/Mathf.Max(b.size.x,b.size.z),maxHeight/b.size.y);
            go.transform.localScale*=scale;b=Bounds(go);go.transform.position+=anchor-new Vector3(b.center.x,b.min.y,b.center.z);
        }

        static void ApplySharedSurface(GameObject go,Color tint)
        {
            foreach(var r in go.GetComponentsInChildren<Renderer>(true))
            {
                var srcs=r.sharedMaterials;var dst=new Material[srcs.Length];
                for(int i=0;i<srcs.Length;i++)
                {
                    var src=srcs[i];if(src==null){dst[i]=null;continue;}
                    int key=src.GetInstanceID();
                    if(!sharedSurfaceMaterials.TryGetValue(key,out var m)||m==null)
                    {
                        m=new Material(src){name="Valoria Mid-Tier · "+src.name};
                        sharedSurfaceMaterials[key]=m;
                    }
                    dst[i]=m;
                }
                r.sharedMaterials=dst;
                var block=new MaterialPropertyBlock();r.GetPropertyBlock(block);
                if(r.sharedMaterial!=null&&r.sharedMaterial.HasProperty("_BaseColor"))block.SetColor("_BaseColor",tint);
                else if(r.sharedMaterial!=null&&r.sharedMaterial.HasProperty("_Color"))block.SetColor("_Color",tint);
                r.SetPropertyBlock(block);
            }
        }

        static void AddWarmth(Transform root,Vector3 p,float intensity,float range)
        {
            var go=new GameObject("Valoria Mid-Tier · occupied warmth");go.transform.SetParent(root,true);go.transform.position=p;
            var l=go.AddComponent<Light>();l.type=LightType.Point;l.color=new Color(1f,.59f,.30f);l.intensity=intensity;l.range=range;l.shadows=LightShadows.None;
        }

        static void DisableGameplay(GameObject go)
        {
            foreach(var c in go.GetComponentsInChildren<Collider>(true))c.enabled=false;
            foreach(var h in go.GetComponentsInChildren<WorldHotspot>(true))Object.DestroyImmediate(h);
            foreach(var b in go.GetComponentsInChildren<MonoBehaviour>(true))if(!(b is WorldHotspot))b.enabled=false;
            Physics.SyncTransforms();
        }

        static Bounds Bounds(GameObject go)
        {
            var rs=go.GetComponentsInChildren<Renderer>(true);if(rs.Length==0)return new Bounds(go.transform.position,Vector3.zero);
            var b=rs[0].bounds;for(int i=1;i<rs.Length;i++)b.Encapsulate(rs[i].bounds);return b;
        }

        static string HierarchyName(Transform t){string s="";for(var p=t;p!=null;p=p.parent)s=p.name+"/"+s;return s;}
    }
}
