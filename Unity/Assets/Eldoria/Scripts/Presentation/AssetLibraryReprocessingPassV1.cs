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

        public static void Build(Transform parent, PlayerState state)
        {
            if(!Enabled || parent==null || state==null || state.BastionLevel<3) return;
            var root = new GameObject("Valoria · AssetLibrary Reprocessing v1 · visual only").transform;
            root.SetParent(parent,true);

            ReassembleHeroApproach(root);
            ReassembleMidTierCore(root);
            ReassembleTerrainSeams(root);
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
            // Use certified masonry as restrained retaining structure, not bright freestanding props.
            AddResource(root,"Valoria/StoneArchitectureKit_v1/HighStraightWall",
                "Hero retaining wall west",new Vector3(-4.65f,1.22f,6.05f),2.75f,88f,SurfaceFamily.Stone);
            AddResource(root,"Valoria/StoneArchitectureKit_v1/HighStraightWall",
                "Hero retaining wall east",new Vector3(4.65f,1.22f,6.10f),2.75f,268f,SurfaceFamily.Stone);
            AddResource(root,"Valoria/StoneArchitectureKit_v1/RockToWallTransition",
                "Hero approach seam west",new Vector3(-3.72f,.92f,4.78f),1.72f,42f,SurfaceFamily.Stone);
            AddResource(root,"Valoria/StoneArchitectureKit_v1/RockToWallTransition",
                "Hero approach seam east",new Vector3(3.72f,.92f,4.84f),1.72f,222f,SurfaceFamily.Stone);
        }

        static void ReassembleMidTierCore(Transform root)
        {
            // D1 is the surviving compact-core parcel. Give it one buried base and one coherent retaining spine.
            AddTopAligned(root,"Valoria/TerrainTerraceKit_v1/BroadRockPlatform",
                "MidTier D1 buried base",new Vector3(6.85f,.43f,-3.85f),4.35f,176f,SurfaceFamily.Terrain);
            AddResource(root,"Valoria/StoneArchitectureKit_v1/HighStraightWall",
                "MidTier D1 rear retaining wall",new Vector3(7.55f,.37f,-2.15f),2.45f,184f,SurfaceFamily.Stone);
            AddResource(root,"Valoria/StoneArchitectureKit_v1/RockToWallTransition",
                "MidTier D1 terrain seam",new Vector3(5.70f,.35f,-3.05f),1.55f,118f,SurfaceFamily.Stone);
        }

        static void ReassembleTerrainSeams(Transform root)
        {
            // Central supports stay buried and dark; they reinforce vertical progression without becoming pedestals.
            AddTopAligned(root,"Valoria/TerrainTerraceKit_v1/SteppedRockTerrace",
                "central west buried terrace",new Vector3(-5.05f,.42f,2.65f),2.35f,100f,SurfaceFamily.Terrain);
            AddTopAligned(root,"Valoria/TerrainTerraceKit_v1/SteppedRockTerrace",
                "central east buried terrace",new Vector3(4.95f,.42f,2.78f),2.35f,260f,SurfaceFamily.Terrain);
        }

        static void RefineExistingSurfaces()
        {
            // Intentionally unused in iteration 2. Existing production Bastion/D1 surfaces remain untouched;
            // only reused historical modules receive the normalized families below.
        }

        static void AddRestrainedOccupation(Transform root)
        {
            AddWarmLight(root,"Hero approach occupied warmth",new Vector3(0f,2.15f,4.45f),.18f,2.8f);
        }

        enum SurfaceFamily { Stone, Terrain }
        static Material reprocessedStone;
        static Material reprocessedTerrain;

        static void AddResource(Transform root,string resource,string role,Vector3 anchor,float span,float yaw,SurfaceFamily family)
        {
            var source=Resources.Load<GameObject>(resource);
            if(source==null) throw new InvalidOperationException("Missing reprocessing resource: "+resource);
            var go=Object.Instantiate(source);go.name="Valoria · AssetLibrary Reprocessing · "+role;
            go.transform.rotation=Quaternion.Euler(0,yaw,0);
            FitGround(go,anchor,span);
            ApplySharedSurface(go,family);
            go.transform.SetParent(root,true);
            DisableGameplay(go);
        }

        static void AddTopAligned(Transform root,string resource,string role,Vector3 topAnchor,float span,float yaw,SurfaceFamily family)
        {
            var source=Resources.Load<GameObject>(resource);
            if(source==null) throw new InvalidOperationException("Missing reprocessing resource: "+resource);
            var go=Object.Instantiate(source);go.name="Valoria · AssetLibrary Reprocessing · "+role;
            go.transform.rotation=Quaternion.Euler(0,yaw,0);
            var b=Bounds(go);go.transform.localScale*=span/Mathf.Max(b.size.x,b.size.z);b=Bounds(go);
            go.transform.position+=new Vector3(topAnchor.x-b.center.x,topAnchor.y-b.max.y,topAnchor.z-b.center.z);
            ApplySharedSurface(go,family);
            go.transform.SetParent(root,true);
            DisableGameplay(go);
        }

        static void FitGround(GameObject go,Vector3 anchor,float span)
        {
            var b=Bounds(go);float scale=span/Mathf.Max(b.size.x,b.size.z);
            go.transform.localScale*=scale;b=Bounds(go);
            go.transform.position+=anchor-new Vector3(b.center.x,b.min.y,b.center.z);
        }

        static void ApplySharedSurface(GameObject go,SurfaceFamily family)
        {
            Material material;
            if(family==SurfaceFamily.Terrain)
            {
                if(reprocessedTerrain==null)
                {
                    reprocessedTerrain=new Material(Shader.Find("Universal Render Pipeline/Lit")){name="Valoria Reprocessed · buried terrain stone"};
                    reprocessedTerrain.SetColor("_BaseColor",new Color(.285f,.275f,.245f,1f));
                    reprocessedTerrain.SetFloat("_Smoothness",.025f);
                    reprocessedTerrain.SetFloat("_Metallic",0f);
                }
                material=reprocessedTerrain;
            }
            else
            {
                if(reprocessedStone==null)
                {
                    reprocessedStone=new Material(Shader.Find("Universal Render Pipeline/Lit")){name="Valoria Reprocessed · architectural stone"};
                    reprocessedStone.SetColor("_BaseColor",new Color(.40f,.385f,.34f,1f));
                    reprocessedStone.SetFloat("_Smoothness",.055f);
                    reprocessedStone.SetFloat("_Metallic",0f);
                }
                material=reprocessedStone;
            }
            foreach(var r in go.GetComponentsInChildren<Renderer>(true))
            {
                var mats=r.sharedMaterials;
                for(int i=0;i<mats.Length;i++)mats[i]=material;
                r.sharedMaterials=mats;
                r.SetPropertyBlock(null);
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
