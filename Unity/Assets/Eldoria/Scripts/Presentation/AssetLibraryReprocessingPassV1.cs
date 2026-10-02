using System;
using System.Collections.Generic;
using Eldoria.Domain;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Eldoria.Presentation
{
    // ASSET LIBRARY REPROCESSING PASS v1
    // Existing production geometry only. No gameplay authority, no Tripo, no new source meshes.
    public static class AssetLibraryReprocessingPassV1
    {
        public static bool Enabled = true;
        static readonly Dictionary<string, Material> sharedSurface = new();

        public static void Build(Transform parent, PlayerState state)
        {
            if(!Enabled || parent==null || state==null || state.BastionLevel<3) return;
            var root = new GameObject("Valoria · AssetLibrary Reprocessing v1 · visual only").transform;
            root.SetParent(parent,true);

            ReassembleHeroApproach(root);
            ReassembleMidTierCore(root);
            ReassembleTerrainSeams(root);
            RefineExistingSurfaces();
            AddRestrainedOccupation(root);
            DisableGameplay(root.gameObject);
        }

        public static void BuildForGate(PlayerState state)
        {
            var parent=GameObject.Find("Valoria · integrated construction visual layer");
            if(parent==null) throw new InvalidOperationException("Valoria production visual root missing for Asset Library Reprocessing gate.");
            if(GameObject.Find("Valoria · AssetLibrary Reprocessing v1 · visual only")!=null) return;
            bool old=Enabled;Enabled=true;Build(parent.transform,state);Enabled=old;
        }

        static void ReassembleHeroApproach(Transform root)
        {
            // Existing Stone Architecture pieces become side retaining shoulders rather than isolated props.
            AddResource(root,"Valoria/StoneArchitectureKit_v1/RockToWallTransition",
                "Hero approach seam west",new Vector3(-3.95f,1.02f,4.95f),2.15f,48f,new Color(.63f,.62f,.58f));
            AddResource(root,"Valoria/StoneArchitectureKit_v1/RockToWallTransition",
                "Hero approach seam east",new Vector3(3.95f,1.02f,5.02f),2.15f,228f,new Color(.63f,.62f,.58f));
            AddResource(root,"Valoria/StoneArchitectureKit_v1/CornerWallL",
                "Hero retaining shoulder west",new Vector3(-4.85f,1.34f,6.05f),2.00f,86f,new Color(.66f,.64f,.59f));
            AddResource(root,"Valoria/StoneArchitectureKit_v1/CornerWallL",
                "Hero retaining shoulder east",new Vector3(4.85f,1.34f,6.10f),2.00f,266f,new Color(.66f,.64f,.59f));
        }

        static void ReassembleMidTierCore(Transform root)
        {
            // D1 survives the accepted compact-footprint cut. Improve how that mass meets terrain instead of stamping more houses.
            AddTopAligned(root,"Valoria/TerrainTerraceKit_v1/BroadRockPlatform",
                "MidTier D1 buried base",new Vector3(6.85f,.44f,-3.85f),4.55f,176f,new Color(.43f,.42f,.38f));
            AddResource(root,"Valoria/StoneArchitectureKit_v1/HighStraightWall",
                "MidTier D1 rear retaining wall",new Vector3(7.70f,.40f,-2.10f),2.60f,184f,new Color(.64f,.62f,.57f));
            AddResource(root,"Valoria/StoneArchitectureKit_v1/RockToWallTransition",
                "MidTier D1 terrain seam",new Vector3(5.65f,.38f,-3.00f),1.72f,118f,new Color(.60f,.59f,.55f));
        }

        static void ReassembleTerrainSeams(Transform root)
        {
            // Reinforce central vertical read with buried historical terrain assets; no roads/floors/colliders are added.
            AddTopAligned(root,"Valoria/TerrainTerraceKit_v1/SteppedRockTerrace",
                "central west buried terrace",new Vector3(-5.15f,.44f,2.55f),2.55f,100f,new Color(.40f,.40f,.37f));
            AddTopAligned(root,"Valoria/TerrainTerraceKit_v1/SteppedRockTerrace",
                "central east buried terrace",new Vector3(5.05f,.44f,2.70f),2.55f,260f,new Color(.40f,.40f,.37f));
        }

        static void RefineExistingSurfaces()
        {
            // environment_surface: normalize only high-return surviving families.
            foreach(var r in Object.FindObjectsByType<Renderer>(FindObjectsSortMode.None))
            {
                if(r==null||!r.enabled||!r.gameObject.activeInHierarchy) continue;
                string chain=Hierarchy(r.transform);
                bool hero=chain.Contains("bastion",StringComparison.OrdinalIgnoreCase)||
                          chain.Contains("herobastion",StringComparison.OrdinalIgnoreCase);
                bool mid=chain.Contains("Valoria Mid-Tier · D1",StringComparison.OrdinalIgnoreCase);
                bool stone=chain.Contains("StoneArch",StringComparison.OrdinalIgnoreCase)||
                           chain.Contains("AssetLibrary Reprocessing",StringComparison.OrdinalIgnoreCase);
                bool terrain=chain.Contains("TerrainTerrace",StringComparison.OrdinalIgnoreCase)||
                             chain.Contains("buried terrace",StringComparison.OrdinalIgnoreCase);
                if(!(hero||mid||stone||terrain)) continue;

                var block=new MaterialPropertyBlock();r.GetPropertyBlock(block);
                Color tint=hero?new Color(.78f,.77f,.72f,1f):
                           mid?new Color(.74f,.70f,.62f,1f):
                           terrain?new Color(.58f,.58f,.54f,1f):
                           new Color(.70f,.68f,.63f,1f);
                var mat=r.sharedMaterial;
                if(mat!=null&&mat.HasProperty("_BaseColor")) block.SetColor("_BaseColor",tint);
                else if(mat!=null&&mat.HasProperty("_Color")) block.SetColor("_Color",tint);
                if(mat!=null&&mat.HasProperty("_Smoothness")) block.SetFloat("_Smoothness",hero?.22f:.16f);
                r.SetPropertyBlock(block);
            }

            // Slightly stronger depth separation, preserving the accepted compact composition.
            RenderSettings.fog=true;
            RenderSettings.fogMode=FogMode.Linear;
            RenderSettings.fogColor=new Color(.52f,.58f,.60f);
            RenderSettings.fogStartDistance=24f;
            RenderSettings.fogEndDistance=68f;
        }

        static void AddRestrainedOccupation(Transform root)
        {
            AddWarmLight(root,"Hero approach occupied warmth",new Vector3(0f,2.15f,4.45f),.24f,3.0f);
            AddWarmLight(root,"MidTier D1 occupied warmth",new Vector3(7.05f,1.45f,-3.05f),.16f,2.25f);
        }

        static void AddResource(Transform root,string resource,string role,Vector3 anchor,float span,float yaw,Color tint)
        {
            var source=Resources.Load<GameObject>(resource);
            if(source==null) throw new InvalidOperationException("Missing reprocessing resource: "+resource);
            var go=Object.Instantiate(source);go.name="Valoria · AssetLibrary Reprocessing · "+role;
            go.transform.rotation=Quaternion.Euler(0,yaw,0);
            FitGround(go,anchor,span);
            ApplySharedSurface(go,tint);
            go.transform.SetParent(root,true);
            DisableGameplay(go);
        }

        static void AddTopAligned(Transform root,string resource,string role,Vector3 topAnchor,float span,float yaw,Color tint)
        {
            var source=Resources.Load<GameObject>(resource);
            if(source==null) throw new InvalidOperationException("Missing reprocessing resource: "+resource);
            var go=Object.Instantiate(source);go.name="Valoria · AssetLibrary Reprocessing · "+role;
            go.transform.rotation=Quaternion.Euler(0,yaw,0);
            var b=Bounds(go);go.transform.localScale*=span/Mathf.Max(b.size.x,b.size.z);b=Bounds(go);
            go.transform.position+=new Vector3(topAnchor.x-b.center.x,topAnchor.y-b.max.y,topAnchor.z-b.center.z);
            ApplySharedSurface(go,tint);
            go.transform.SetParent(root,true);
            DisableGameplay(go);
        }

        static void FitGround(GameObject go,Vector3 anchor,float span)
        {
            var b=Bounds(go);float scale=span/Mathf.Max(b.size.x,b.size.z);
            go.transform.localScale*=scale;b=Bounds(go);
            go.transform.position+=anchor-new Vector3(b.center.x,b.min.y,b.center.z);
        }

        static void ApplySharedSurface(GameObject go,Color tint)
        {
            foreach(var r in go.GetComponentsInChildren<Renderer>(true))
            {
                var srcs=r.sharedMaterials;var dst=new Material[srcs.Length];
                for(int i=0;i<srcs.Length;i++)
                {
                    var src=srcs[i];if(src==null){dst[i]=null;continue;}
                    string key=src.name;
                    if(!sharedSurface.TryGetValue(key,out var m)||m==null)
                    {
                        m=new Material(src){name="Valoria Reprocessed · "+src.name};
                        if(m.HasProperty("_Smoothness"))m.SetFloat("_Smoothness",.16f);
                        sharedSurface[key]=m;
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

        static void AddWarmLight(Transform root,string name,Vector3 p,float intensity,float range)
        {
            var go=new GameObject("Valoria · AssetLibrary Reprocessing · "+name);go.transform.SetParent(root,true);go.transform.position=p;
            var l=go.AddComponent<Light>();l.type=LightType.Point;l.color=new Color(1f,.58f,.29f);
            l.intensity=intensity;l.range=range;l.shadows=LightShadows.None;
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
            var rs=go.GetComponentsInChildren<Renderer>(true);
            if(rs.Length==0)return new Bounds(go.transform.position,Vector3.zero);
            var b=rs[0].bounds;for(int i=1;i<rs.Length;i++)b.Encapsulate(rs[i].bounds);return b;
        }

        static string Hierarchy(Transform t)
        {
            string s="";for(var p=t;p!=null;p=p.parent)s=p.name+"/"+s;return s;
        }
    }
}
