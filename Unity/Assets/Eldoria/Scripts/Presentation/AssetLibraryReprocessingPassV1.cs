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

            ReplaceWithCertifiedHeroBastion(root);
            RefineLegacyHeroSurface();
            ReassembleHeroApproach(root);
            ReassembleMidTierCore(root);
            ReassembleTerrainSeams(root);
            RefineAtmosphere();
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

        static void ReplaceWithCertifiedHeroBastion(Transform root)
        {
            // Highest-return historical recovery in this pass. The exact certified optimized GLB is
            // staged by the proof/promotion workflow from artifact 11143009723 and verified by SHA.
            // If the source is not present (ordinary source-only checks), leave the current Bastion untouched.
            var source=Resources.Load<GameObject>("Valoria/HeroBastionGenerated/Valoria_HeroBastion_v1");
            if(source==null)return;

            HideLegacyBastionVisuals();

            var go=Object.Instantiate(source);
            go.name="Valoria · AssetLibrary Reprocessing · certified Hero Bastion v1";
            go.transform.rotation=Quaternion.Euler(0f,180f,0f);
            DisableGameplay(go);

            var renderers=go.GetComponentsInChildren<Renderer>(true);
            if(renderers.Length==0)throw new InvalidOperationException("Certified Hero Bastion has no renderers.");
            var bounds=renderers[0].bounds;
            for(int i=1;i<renderers.Length;i++)bounds.Encapsulate(renderers[i].bounds);
            float span=Mathf.Max(bounds.size.x,bounds.size.z);
            if(span<=.001f||bounds.size.y<=.001f)throw new InvalidOperationException("Certified Hero Bastion bounds invalid.");

            // Same proportions/orientation as the prior successful integrated proof, now seated into the
            // accepted Compact Footprint terrain. No mesh edits and no gameplay ownership.
            const float targetSpan=12.8f;
            const float targetHeight=10.2f;
            float scale=Mathf.Min(targetSpan/span,targetHeight/bounds.size.y);
            go.transform.localScale*=scale;

            renderers=go.GetComponentsInChildren<Renderer>(true);
            bounds=renderers[0].bounds;
            for(int i=1;i<renderers.Length;i++)bounds.Encapsulate(renderers[i].bounds);
            var targetCenter=new Vector3(0f,0f,8.75f);
            const float groundY=2.52f;
            go.transform.position+=new Vector3(
                targetCenter.x-bounds.center.x,
                groundY-bounds.min.y,
                targetCenter.z-bounds.center.z);

            FitCertifiedHeroSurface(go);
            go.transform.SetParent(root,true);
        }

        static void HideLegacyBastionVisuals()
        {
            foreach(var r in Object.FindObjectsByType<Renderer>(FindObjectsSortMode.None))
            {
                if(r==null||!r.enabled||!r.gameObject.activeInHierarchy)continue;
                bool legacy=false;
                for(var t=r.transform;t!=null;t=t.parent)
                {
                    if(t.name.StartsWith("Bastion ·",StringComparison.OrdinalIgnoreCase)||
                       string.Equals(t.name,"Bastion",StringComparison.OrdinalIgnoreCase)||
                       t.name.StartsWith("Valoria · Bastion hero",StringComparison.OrdinalIgnoreCase)||
                       t.name.StartsWith("Valoria · rescued hero flank",StringComparison.OrdinalIgnoreCase))
                    {
                        legacy=true;break;
                    }
                }
                if(legacy)r.enabled=false;
            }
        }

        static void FitCertifiedHeroSurface(GameObject go)
        {
            var cache=new Dictionary<int,Material>();
            foreach(var r in go.GetComponentsInChildren<Renderer>(true))
            {
                var srcs=r.sharedMaterials;var dst=new Material[srcs.Length];
                for(int i=0;i<srcs.Length;i++)
                {
                    var src=srcs[i];
                    if(src==null){dst[i]=null;continue;}
                    if(cache.TryGetValue(src.GetInstanceID(),out var cached)){dst[i]=cached;continue;}

                    Texture baseMap=null,normal=null,mask=null;
                    foreach(string p in new[]{"_BaseMap","_MainTex","_BaseColorTexture","baseColorTexture","_Texture"})
                        if(src.HasProperty(p)&&src.GetTexture(p)!=null){baseMap=src.GetTexture(p);break;}
                    foreach(string p in new[]{"_BumpMap","_NormalMap","normalTexture"})
                        if(src.HasProperty(p)&&src.GetTexture(p)!=null){normal=src.GetTexture(p);break;}
                    foreach(string p in new[]{"_MaskMap","_MetallicGlossMap","_OcclusionMap"})
                        if(src.HasProperty(p)&&src.GetTexture(p)!=null){mask=src.GetTexture(p);break;}

                    var shader=Shader.Find("Universal Render Pipeline/Lit")??Shader.Find("Standard");
                    var m=new Material(shader){name="Valoria AssetLibrary · Hero Bastion · "+src.name};
                    if(baseMap!=null&&m.HasProperty("_BaseMap"))m.SetTexture("_BaseMap",baseMap);
                    if(normal!=null&&m.HasProperty("_BumpMap"))
                    {
                        m.SetTexture("_BumpMap",normal);m.EnableKeyword("_NORMALMAP");
                        if(m.HasProperty("_BumpScale"))m.SetFloat("_BumpScale",1f);
                    }
                    if(mask!=null&&m.HasProperty("_OcclusionMap"))m.SetTexture("_OcclusionMap",mask);
                    if(m.HasProperty("_BaseColor"))m.SetColor("_BaseColor",new Color(.64f,.61f,.56f,1f));
                    if(m.HasProperty("_Color"))m.SetColor("_Color",new Color(.74f,.71f,.66f,1f));
                    if(m.HasProperty("_Metallic"))m.SetFloat("_Metallic",0f);
                    if(m.HasProperty("_Smoothness"))m.SetFloat("_Smoothness",.035f);
                    if(m.HasProperty("_OcclusionStrength"))m.SetFloat("_OcclusionStrength",1f);
                    cache[src.GetInstanceID()]=m;dst[i]=m;
                }
                r.sharedMaterials=dst;
            }
        }

        static void ReassembleHeroApproach(Transform root)
        {
            // Final subtractive seating pass: the recovered Hero Bastion already owns the landmark silhouette
            // and a natural rock base. Support it with buried terraces + two seams; do not frame it with
            // large repeated wall/tower props that compete at mobile scale.
            HideNamedRenderers("Valoria · rescued hero flank");

            AddTopAligned(root,"Valoria/TerrainTerraceKit_v1/BroadRockPlatform",
                "Hero lower terrace west",new Vector3(-3.65f,1.12f,5.92f),4.55f,12f,SurfaceFamily.Terrain);
            AddTopAligned(root,"Valoria/TerrainTerraceKit_v1/BroadRockPlatform",
                "Hero lower terrace east",new Vector3(3.65f,1.12f,5.98f),4.55f,168f,SurfaceFamily.Terrain);
            AddTopAligned(root,"Valoria/TerrainTerraceKit_v1/SteppedRockTerrace",
                "Hero upper terrace west",new Vector3(-2.72f,2.42f,7.05f),3.75f,98f,SurfaceFamily.Terrain);
            AddTopAligned(root,"Valoria/TerrainTerraceKit_v1/SteppedRockTerrace",
                "Hero upper terrace east",new Vector3(2.72f,2.42f,7.10f),3.75f,262f,SurfaceFamily.Terrain);

            AddResource(root,"Valoria/StoneArchitectureKit_v1/RockToWallTransition",
                "Hero approach seam west",new Vector3(-3.25f,.55f,4.48f),1.80f,42f,SurfaceFamily.Stone);
            AddResource(root,"Valoria/StoneArchitectureKit_v1/RockToWallTransition",
                "Hero approach seam east",new Vector3(3.25f,.55f,4.54f),1.80f,222f,SurfaceFamily.Stone);
        }

        static void HideNamedRenderers(string fragment)
        {
            foreach(var r in Object.FindObjectsByType<Renderer>(FindObjectsSortMode.None))
            {
                if(r==null||!r.enabled)continue;
                for(var t=r.transform;t!=null;t=t.parent)
                    if(t.name.IndexOf(fragment,StringComparison.OrdinalIgnoreCase)>=0){r.enabled=false;break;}
            }
        }

        static void RefineLegacyHeroSurface()
        {
            // If the certified generated Hero Bastion is unavailable in an ordinary checkout,
            // refine the existing production Bastion instead of inventing replacement geometry.
            foreach(var r in Object.FindObjectsByType<Renderer>(FindObjectsSortMode.None))
            {
                if(r==null||!r.enabled||!r.gameObject.activeInHierarchy)continue;
                string chain=Hierarchy(r.transform).ToLowerInvariant();
                if(!chain.Contains("bastion"))continue;
                // Never repaint the recovered certified Hero Bastion: its texture/normal preserving
                // surface-fit is already the accepted zero-credit treatment from the integrated proof.
                if(chain.Contains("certified hero bastion")||chain.Contains("assetlibrary · hero bastion"))continue;
                if(chain.Contains("banner")||chain.Contains("flag"))continue;

                var block=new MaterialPropertyBlock();r.GetPropertyBlock(block);
                bool deep=chain.Contains("plinth")||chain.Contains("backing")||chain.Contains("rubble")||chain.Contains("collapse");
                bool dark=chain.Contains("roof")||chain.Contains("crown")||chain.Contains("slit");
                Color tint=deep?new Color(.43f,.42f,.39f,1f):
                           dark?new Color(.35f,.35f,.34f,1f):
                           new Color(.67f,.65f,.60f,1f);
                var mat=r.sharedMaterial;
                if(mat!=null&&mat.HasProperty("_BaseColor"))block.SetColor("_BaseColor",tint);
                else if(mat!=null&&mat.HasProperty("_Color"))block.SetColor("_Color",tint);
                if(mat!=null&&mat.HasProperty("_Smoothness"))block.SetFloat("_Smoothness",.06f);
                r.SetPropertyBlock(block);
            }
        }

        static void RefineAtmosphere()
        {
            // Return the compact pass to the frozen Visual Formula depth range:
            // clearer architecture without removing atmospheric separation.
            RenderSettings.fog=true;
            RenderSettings.fogMode=FogMode.Linear;
            RenderSettings.fogStartDistance=30f;
            RenderSettings.fogEndDistance=68f;
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
