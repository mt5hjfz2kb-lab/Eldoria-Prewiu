using System;
using System.Collections.Generic;
using Eldoria.Domain;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Eldoria.Presentation
{
    // VALORIA REFERENCE CONVERGENCE PASS v2
    // Existing certified/library geometry only. Visual-only: no gameplay, collider, hotspot or progression ownership.
    public static class ValoriaReferenceConvergencePassV2
    {
        public static bool Enabled = false;
        static readonly Color StoneTint = new Color(.63f,.61f,.56f,1f);
        static readonly Color RockTint = new Color(.40f,.42f,.42f,1f);
        static readonly Color FoliageTint = new Color(.42f,.50f,.39f,1f);

        public static void Build(Transform parent, PlayerState state)
        {
            if(!Enabled || parent==null || state==null || state.BastionLevel<3) return;
            if(GameObject.Find("Valoria · Reference Convergence v2 · visual only")!=null) return;

            var root=new GameObject("Valoria · Reference Convergence v2 · visual only").transform;
            root.SetParent(parent,true);
            var art=ValoriaExternalAssetLibrary.Load();

            BuildMonumentalFrame(root,art);
            BuildCliffEnvelope(root);
            RefitWorldGroundSurfaces();
            SuppressPrototypeGroundSurfaces();
            BuildLateralMargins(root,art);
            BuildMountainHorizon(root);
            BuildVegetationDepth(root,art);
            BuildOccupationAndAtmosphere(root);
            RefineGlobalAtmosphere();

            DisableGameplay(root.gameObject);
        }

        static void BuildMonumentalFrame(Transform root,ValoriaExternalAssetLibrary art)
        {
            // Iteration 9: the legacy Stone_Gate / Stone_Tower silhouettes read as two dark twin fortresses
            // in the official cameras and competed with the Hero Bastion. Keep the frame open here.
            // Lateral ruins are authored below from the certified terrain/terrace + stone-architecture vocabulary.
        }

        static void BuildCliffEnvelope(Transform root)
        {
            // Iteration 13: legacy SM_Cliffs are excluded from the convergence pass.
            // Their mixed grass/rock materials created flat green wedges at official camera distance.
            // The lateral world envelope is now authored only from the certified PBR Terrain & Terrace Kit below.
        }

        static void RefitWorldGroundSurfaces()
        {
            // Iteration 19: same dirt family everywhere, with world-scale-aware tiling.
            // IrregularGround uses 0..1 UVs regardless of physical size, so the 200x180 valley
            // needs roughly 8x the tiling of the ~24x26 inhabited floor to keep texture frequency coherent.
            Material valleyDirt=null;
            var valleyShader=Shader.Find("Universal Render Pipeline/Lit")??Shader.Find("Standard");
            if(valleyShader!=null)
            {
                valleyDirt=new Material(valleyShader){name="Valoria Reference v2 · matte world valley"};
                var valleyColor=new Color(.275f,.285f,.255f,1f);
                if(valleyDirt.HasProperty("_BaseColor"))valleyDirt.SetColor("_BaseColor",valleyColor);
                if(valleyDirt.HasProperty("_Color"))valleyDirt.SetColor("_Color",valleyColor);
                if(valleyDirt.HasProperty("_Metallic"))valleyDirt.SetFloat("_Metallic",0f);
                if(valleyDirt.HasProperty("_Smoothness"))valleyDirt.SetFloat("_Smoothness",.015f);
            }
            Material valleyWall=null;
            if(valleyShader!=null)
            {
                valleyWall=new Material(valleyShader){name="Valoria Reference v2 · continuous valley wall"};
                var wallColor=new Color(.255f,.285f,.29f,1f);
                if(valleyWall.HasProperty("_BaseColor"))valleyWall.SetColor("_BaseColor",wallColor);
                if(valleyWall.HasProperty("_Color"))valleyWall.SetColor("_Color",wallColor);
                if(valleyWall.HasProperty("_Metallic"))valleyWall.SetFloat("_Metallic",0f);
                if(valleyWall.HasProperty("_Smoothness"))valleyWall.SetFloat("_Smoothness",.018f);
            }

            var dirt=ValoriaKit.ExternalPbrSurfaceMaterial("dirt",
                new Color(.66f,.63f,.54f,1f),new Vector2(4.2f,4.2f),.014f,.90f)
                ?? ValoriaKit.DetailedSurfaceMaterial(new Color(.31f,.29f,.24f,1f),"earth",new Vector2(4.2f,4.2f),.78f);
            var terrace=ValoriaKit.ExternalPbrSurfaceMaterial("dirt",
                new Color(.74f,.69f,.59f,1f),new Vector2(5.5f,5.5f),.014f,.94f)
                ?? ValoriaKit.DetailedSurfaceMaterial(new Color(.37f,.33f,.27f,1f),"earth",new Vector2(5.5f,5.5f),.82f);

            Material inhabitedMatte=null;
            var matteShader=Shader.Find("Universal Render Pipeline/Lit")??Shader.Find("Standard");
            if(matteShader!=null)
            {
                inhabitedMatte=new Material(matteShader){name="Valoria Reference v2 · inhabited matte ground"};
                var c=new Color(.315f,.305f,.275f,1f);
                if(inhabitedMatte.HasProperty("_BaseColor"))inhabitedMatte.SetColor("_BaseColor",c);
                if(inhabitedMatte.HasProperty("_Color"))inhabitedMatte.SetColor("_Color",c);
                if(inhabitedMatte.HasProperty("_Metallic"))inhabitedMatte.SetFloat("_Metallic",0f);
                if(inhabitedMatte.HasProperty("_Smoothness"))inhabitedMatte.SetFloat("_Smoothness",.02f);
            }

            foreach(var renderer in Object.FindObjectsByType<Renderer>(FindObjectsSortMode.None))
            {
                if(renderer==null||!renderer.enabled||!renderer.gameObject.activeInHierarchy)continue;
                string n=renderer.gameObject.name;
                if(n=="Valoria · valley floor")
                {
                    renderer.sharedMaterial=valleyDirt??dirt;
                    var p=renderer.transform.position;
                    renderer.transform.position=new Vector3(p.x,.045f,p.z);
                    // Iteration 24: keep the authored 200x180 visual valley at canonical scale.
                    // The earlier contour issue was traced to HeroValleyTerrain(), not this sheet.
                }
                else if(n=="Valoria · Hero Frame valley terrain")
                {
                    // Iteration 30: continuous visual-only valley walls.
                    // Keep the compact inhabited corridor and all foreground flat, then grow side/rear
                    // mass smoothly from the same mesh so no detached mountain proxy or map corona appears.
                    renderer.enabled=true;
                    renderer.sharedMaterial=valleyDirt??dirt;

                    var mf=renderer.GetComponent<MeshFilter>();
                    if(mf!=null && mf.sharedMesh!=null)
                    {
                        var source=mf.sharedMesh;
                        var mesh=Object.Instantiate(source);
                        mesh.name=source.name+" · convergence continuous valley walls";
                        var verts=mesh.vertices;
                        const float flatY=-.20f;
                        for(int i=0;i<verts.Length;i++)
                        {
                            var v=verts[i];

                            float side=Mathf.SmoothStep(0f,1f,Mathf.Clamp01((Mathf.Abs(v.x)-10.0f)/11.0f));
                            float rear=Mathf.SmoothStep(0f,1f,Mathf.Clamp01((v.z-8.0f)/15.0f));
                            float frontGate=Mathf.SmoothStep(0f,1f,Mathf.Clamp01((v.z+1.0f)/6.0f));

                            // No side wall may rise in the foreground.
                            float sideRelief=side*frontGate;
                            float relief=Mathf.Max(sideRelief,rear);

                            float ridge=sideRelief*sideRelief*3.3f + rear*rear*4.1f;
                            float organic=(Mathf.Sin(v.x*.19f)+Mathf.Sin(v.z*.23f)+Mathf.Sin((v.x-v.z)*.11f))*.22f*relief;
                            v.y=flatY + ridge + organic;

                            // Preserve an open corridor around the compact city and approach.
                            float corridorX=1f-Mathf.SmoothStep(0f,1f,Mathf.Clamp01((Mathf.Abs(v.x)-8.5f)/4.0f));
                            float corridorZ=1f-Mathf.SmoothStep(0f,1f,Mathf.Clamp01((v.z-10f)/7f));
                            float corridor=corridorX*corridorZ;
                            v.y=Mathf.Lerp(v.y,flatY,corridor);

                            verts[i]=v;
                        }
                        mesh.vertices=verts;
                        mesh.RecalculateNormals();
                        mesh.RecalculateBounds();
                        mf.sharedMesh=mesh;
                    }
                }
                else if(n=="VPD · inhabited mountain floor")
                    renderer.sharedMaterial=inhabitedMatte??dirt;
                else if(n=="VPD · lower terrace earth"||n=="VPD · upper terrace earth")
                    renderer.sharedMaterial=terrace;
            }
        }

        static void SuppressPrototypeGroundSurfaces()
        {
            // Iteration 16: suppress only auxiliary rectangular planning/prototype shelves.
            // Keep the irregular valley floor visible: it is now PBR-reskinned above and prevents a floating-island edge.
            // Colliders, transforms, names, progression and interaction remain untouched.
            var exactNames=new HashSet<string>(StringComparer.Ordinal)
            {
                // The broad inhabited-floor sheet duplicates the continuous valley underneath and
                // exposes its near mesh boundary at zoom 12. Hide only its renderer; detailed terraces,
                // circulation, colliders and hotspots remain authoritative and visible.
                "VPD · inhabited mountain floor",
                "VPD · west expansion terrain",
                "VPD · east expansion terrain",
                "VPD · future valley shelf",
                "VPD · west authored apron",
                "VPD · east authored apron"
            };

            foreach(var r in Object.FindObjectsByType<Renderer>(FindObjectsSortMode.None))
            {
                if(r==null || r.gameObject==null)continue;
                if(exactNames.Contains(r.gameObject.name))
                    r.enabled=false;
            }
        }

        static void BuildLateralMargins(Transform root,ValoriaExternalAssetLibrary art)
        {
            // Iteration 30: layered certified PBR rock frame only.
            // Build continuous side/rear world mass without widening the playable city.

            // West curtain.
            AddTopAligned(root,"Valoria/TerrainTerraceKit_v1/BroadRockPlatform",
                "west foreground rock mass",new Vector3(-14.6f,.55f,-3.5f),10.8f,18f,new Color(.43f,.44f,.42f,1f));
            AddTopAligned(root,"Valoria/TerrainTerraceKit_v1/SteppedRockTerrace",
                "west mid cliff mass",new Vector3(-14.8f,2.15f,3.0f),8.6f,84f,new Color(.42f,.43f,.41f,1f));
            AddTopAligned(root,"Valoria/TerrainTerraceKit_v1/BroadRockPlatform",
                "west upper shelf mass",new Vector3(-14.2f,3.15f,9.2f),9.4f,40f,new Color(.44f,.45f,.43f,1f));
            AddTopAligned(root,"Valoria/TerrainTerraceKit_v1/SteppedRockTerrace",
                "west rear cliff mass",new Vector3(-12.2f,4.15f,15.0f),7.2f,128f,new Color(.41f,.42f,.40f,1f));

            // East curtain, intentionally asymmetric.
            AddTopAligned(root,"Valoria/TerrainTerraceKit_v1/BroadRockPlatform",
                "east foreground rock mass",new Vector3(15.0f,.40f,-2.8f),9.8f,208f,new Color(.43f,.44f,.42f,1f));
            AddTopAligned(root,"Valoria/TerrainTerraceKit_v1/SteppedRockTerrace",
                "east mid cliff mass",new Vector3(14.9f,1.95f,3.8f),8.2f,278f,new Color(.42f,.43f,.41f,1f));
            AddTopAligned(root,"Valoria/TerrainTerraceKit_v1/BroadRockPlatform",
                "east upper shelf mass",new Vector3(14.0f,2.85f,9.8f),8.8f,222f,new Color(.44f,.45f,.43f,1f));
            AddTopAligned(root,"Valoria/TerrainTerraceKit_v1/SteppedRockTerrace",
                "east rear cliff mass",new Vector3(11.8f,3.95f,15.4f),7.0f,236f,new Color(.41f,.42f,.40f,1f));

            // Rear shoulders close the empty horizon while preserving an open Bastion silhouette.
            AddTopAligned(root,"Valoria/TerrainTerraceKit_v1/BroadRockPlatform",
                "rear west shoulder",new Vector3(-8.3f,3.0f,17.0f),8.4f,28f,new Color(.43f,.44f,.42f,1f));
            AddTopAligned(root,"Valoria/TerrainTerraceKit_v1/BroadRockPlatform",
                "rear east shoulder",new Vector3(8.6f,2.8f,17.3f),8.0f,206f,new Color(.43f,.44f,.42f,1f));

            if(art!=null && art.SlavicBoulder!=null)
            {
                var rocks=new[]{
                    new Vector3(-13.8f,.10f,-1.2f),new Vector3(-13.2f,.15f,4.7f),new Vector3(-12.7f,.20f,10.4f),
                    new Vector3(13.9f,.10f,-.7f),new Vector3(13.0f,.15f,5.1f),new Vector3(12.5f,.20f,10.8f)
                };
                for(int i=0;i<rocks.Length;i++)
                    AddPrefab(root,art.SlavicBoulder,"layered frame boulder "+i,rocks[i],
                        1.55f+(i%3)*.18f,1.10f+(i%2)*.12f,(i*47)%360,new Color(.43f,.44f,.42f,1f),true);
            }
        }

        static void BuildMountainHorizon(Transform root)
        {
            // Iteration 30: no legacy mountain proxy. The certified layered PBR rock frame owns world mass.
        }

        static void BuildVegetationDepth(Transform root,ValoriaExternalAssetLibrary art)
        {
            // Iteration 25: no pass-owned vegetation.
            // The shader-safe procedural pines improved density but read too dark/low-poly against the reference.
            // Preserve the canonical scene vegetation; depth now comes from terrain, architecture and atmosphere.
        }

        static void BuildOccupationAndAtmosphere(Transform root)
        {
            AddWarmLight(root,"hero court",new Vector3(0f,4.1f,6.8f),.34f,4.4f);
            AddWarmLight(root,"west work quarter",new Vector3(-8.8f,1.25f,-3.6f),.20f,3.2f);
            AddWarmLight(root,"east military quarter",new Vector3(8.6f,1.30f,-4.0f),.20f,3.2f);
            AddWarmLight(root,"upper west ruin",new Vector3(-8.2f,3.9f,10.8f),.15f,2.6f);
            AddWarmLight(root,"upper east ruin",new Vector3(8.1f,3.8f,10.9f),.15f,2.6f);
        }

        static void RefineGlobalAtmosphere()
        {
            RenderSettings.ambientMode=UnityEngine.Rendering.AmbientMode.Trilight;
            // Iteration 23: restore atmospheric separation instead of washing the whole city into one grey plane.
            // Keep distant haze, but let the inhabited city retain stone/wood/vegetation colour and warm light.
            RenderSettings.ambientSkyColor=new Color(.72f,.82f,.90f);
            RenderSettings.ambientEquatorColor=new Color(.53f,.54f,.49f);
            RenderSettings.ambientGroundColor=new Color(.28f,.25f,.21f);
            RenderSettings.ambientIntensity=.94f;
            RenderSettings.fog=true;
            RenderSettings.fogMode=FogMode.Linear;
            RenderSettings.fogColor=new Color(.52f,.61f,.66f);
            RenderSettings.fogStartDistance=26f;
            RenderSettings.fogEndDistance=78f;

            var camera=Camera.main;
            if(camera!=null)
            {
                camera.clearFlags=CameraClearFlags.SolidColor;
                camera.backgroundColor=new Color(.45f,.58f,.66f);
                camera.allowHDR=true;
            }

            foreach(var light in Object.FindObjectsByType<Light>(FindObjectsSortMode.None))
            {
                if(light.type!=LightType.Directional)continue;
                light.color=new Color(1f,.86f,.70f);
                light.intensity=Mathf.Max(light.intensity,1.32f);
                light.shadowStrength=.61f;
                light.shadows=LightShadows.Soft;
            }
        }

        static void AddWarmLight(Transform root,string name,Vector3 p,float intensity,float range)
        {
            var go=new GameObject("Valoria · Reference Convergence v2 · "+name+" warmth");
            go.transform.SetParent(root,true);go.transform.position=p;
            var l=go.AddComponent<Light>();l.type=LightType.Point;l.color=new Color(1f,.58f,.28f);
            l.intensity=intensity;l.range=range;l.shadows=LightShadows.None;
        }

        static void AddTopAligned(Transform root,string resource,string role,Vector3 topAnchor,float span,float yaw,Color tint)
        {
            var source=Resources.Load<GameObject>(resource);
            if(source==null)return;
            var go=Object.Instantiate(source);
            go.name="Valoria · Reference Convergence v2 · "+role;
            go.transform.rotation=Quaternion.Euler(0f,yaw,0f);
            var b=Bounds(go);
            if(b.size.sqrMagnitude<.0001f){Object.DestroyImmediate(go);return;}
            go.transform.localScale*=span/Mathf.Max(.001f,Mathf.Max(b.size.x,b.size.z));
            b=Bounds(go);
            go.transform.position+=new Vector3(topAnchor.x-b.center.x,topAnchor.y-b.max.y,topAnchor.z-b.center.z);
            ApplyTint(go,tint);
            go.transform.SetParent(root,true);
            DisableGameplay(go);
        }

        static void AddResource(Transform root,string resource,string role,Vector3 anchor,float span,float maxHeight,float yaw,Color tint)
        {
            var source=Resources.Load<GameObject>(resource);
            if(source==null)return;
            AddPrefab(root,source,role,anchor,span,maxHeight,yaw,tint);
        }

        static void AddPrefab(Transform root,GameObject source,string role,Vector3 anchor,float span,float maxHeight,float yaw,Color tint,bool forceSafeLit=false)
        {
            if(source==null)return;
            var go=Object.Instantiate(source);
            go.name="Valoria · Reference Convergence v2 · "+role;
            go.transform.rotation=Quaternion.Euler(0f,yaw,0f);
            var b=Bounds(go);
            if(b.size.sqrMagnitude<.0001f){Object.DestroyImmediate(go);return;}
            float horizontal=Mathf.Max(b.size.x,b.size.z);
            float scale=Mathf.Min(span/Mathf.Max(.001f,horizontal),maxHeight/Mathf.Max(.001f,b.size.y));
            go.transform.localScale*=scale;
            b=Bounds(go);
            go.transform.position+=anchor-new Vector3(b.center.x,b.min.y,b.center.z);
            if(forceSafeLit)ApplySafeLit(go,tint);
            else ApplyTint(go,tint);
            go.transform.SetParent(root,true);
            DisableGameplay(go);
        }

        static void ApplySafeLit(GameObject go,Color tint)
        {
            var shader=Shader.Find("Universal Render Pipeline/Lit")??Shader.Find("Standard");
            if(shader==null){ApplyTint(go,tint);return;}

            foreach(var r in go.GetComponentsInChildren<Renderer>(true))
            {
                if(r==null)continue;
                var src=r.sharedMaterials;
                var dst=new Material[src.Length];

                for(int i=0;i<src.Length;i++)
                {
                    var old=src[i];
                    if(old==null){dst[i]=null;continue;}

                    var m=new Material(shader){name="Valoria Reference v2 · converted stone"};
                    Texture baseTex=null;
                    Vector2 scale=Vector2.one,offset=Vector2.zero;

                    if(old.HasProperty("_BaseMap"))
                    {
                        baseTex=old.GetTexture("_BaseMap");
                        scale=old.GetTextureScale("_BaseMap");
                        offset=old.GetTextureOffset("_BaseMap");
                    }
                    else if(old.HasProperty("_MainTex"))
                    {
                        baseTex=old.GetTexture("_MainTex");
                        scale=old.GetTextureScale("_MainTex");
                        offset=old.GetTextureOffset("_MainTex");
                    }

                    if(baseTex!=null)
                    {
                        if(m.HasProperty("_BaseMap"))
                        {
                            m.SetTexture("_BaseMap",baseTex);
                            m.SetTextureScale("_BaseMap",scale);
                            m.SetTextureOffset("_BaseMap",offset);
                        }
                        if(m.HasProperty("_MainTex"))
                        {
                            m.SetTexture("_MainTex",baseTex);
                            m.SetTextureScale("_MainTex",scale);
                            m.SetTextureOffset("_MainTex",offset);
                        }
                    }

                    Texture normal=null;
                    if(old.HasProperty("_BumpMap"))normal=old.GetTexture("_BumpMap");
                    else if(old.HasProperty("_NormalMap"))normal=old.GetTexture("_NormalMap");
                    if(normal!=null&&m.HasProperty("_BumpMap"))
                    {
                        m.SetTexture("_BumpMap",normal);
                        m.EnableKeyword("_NORMALMAP");
                    }

                    // Keep texture variation; tint only nudges it toward Valoria limestone.
                    var surfaceTint=Color.Lerp(Color.white,tint,.34f);
                    if(m.HasProperty("_BaseColor"))m.SetColor("_BaseColor",surfaceTint);
                    if(m.HasProperty("_Color"))m.SetColor("_Color",surfaceTint);
                    if(m.HasProperty("_Metallic"))m.SetFloat("_Metallic",0f);
                    if(m.HasProperty("_Smoothness"))m.SetFloat("_Smoothness",.055f);
                    dst[i]=m;
                }
                r.sharedMaterials=dst;
            }
        }

        static void ApplyTint(GameObject go,Color tint)
        {
            foreach(var r in go.GetComponentsInChildren<Renderer>(true))
            {
                if(r==null)continue;
                var block=new MaterialPropertyBlock();r.GetPropertyBlock(block);
                var mat=r.sharedMaterial;
                if(mat!=null&&mat.HasProperty("_BaseColor"))block.SetColor("_BaseColor",tint);
                else if(mat!=null&&mat.HasProperty("_Color"))block.SetColor("_Color",tint);
                r.SetPropertyBlock(block);
            }
        }

        static Bounds Bounds(GameObject go)
        {
            var rs=go.GetComponentsInChildren<Renderer>(true);
            if(rs.Length==0)return new Bounds(go.transform.position,Vector3.zero);
            var b=rs[0].bounds;
            for(int i=1;i<rs.Length;i++)b.Encapsulate(rs[i].bounds);
            return b;
        }

        static void DisableGameplay(GameObject go)
        {
            foreach(var c in go.GetComponentsInChildren<Collider>(true))c.enabled=false;
            foreach(var h in go.GetComponentsInChildren<WorldHotspot>(true))Object.DestroyImmediate(h);
            foreach(var b in go.GetComponentsInChildren<MonoBehaviour>(true))
                if(!(b is WorldHotspot))b.enabled=false;
            Physics.SyncTransforms();
        }
    }
}
