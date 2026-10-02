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
            // Iteration 13: use certified rescued rock-only vocabulary.
            // SM_Cliffs_* was rejected because its grass sub-material created flat green wedges in matched captures.
            AddResource(root,"Valoria/Rescued/RockTerrainSeamFiller","left rock shelf",
                new Vector3(-11.8f,-1.35f,1.8f),8.8f,5.2f,26f,RockTint);
            AddResource(root,"Valoria/Rescued/RockTerrainSeamFiller","right rock shelf",
                new Vector3(11.6f,-1.35f,2.0f),8.8f,5.2f,206f,RockTint);

            AddResource(root,"Valoria/Rescued/ResidentialTerraceRock","rear integrated rock shelf",
                new Vector3(0f,-.70f,12.0f),11.5f,5.4f,92f,new Color(.43f,.45f,.44f,1f));
        }

        static void BuildLateralMargins(Transform root,ValoriaExternalAssetLibrary art)
        {
            // Iteration 13: lateral margins from rescued rock-only modules + sparse vegetation.
            AddResource(root,"Valoria/Rescued/RockTerrainSeamFiller","left outer rock curtain",
                new Vector3(-13.8f,-1.65f,3.5f),7.2f,4.6f,18f,new Color(.40f,.42f,.42f,1f));
            AddResource(root,"Valoria/Rescued/RockTerrainSeamFiller","right outer rock curtain",
                new Vector3(13.7f,-1.65f,3.7f),7.2f,4.6f,198f,new Color(.40f,.42f,.42f,1f));

            AddResource(root,"Valoria/Rescued/RockTerrainSeamFiller","left rear broken rock",
                new Vector3(-12.6f,-1.20f,9.6f),5.8f,3.8f,104f,new Color(.44f,.45f,.44f,1f));
            AddResource(root,"Valoria/Rescued/RockTerrainSeamFiller","right rear broken rock",
                new Vector3(12.6f,-1.20f,9.8f),5.8f,3.8f,284f,new Color(.44f,.45f,.44f,1f));

            if(art!=null)
            {
                var tree=art.SlavicTreeTall!=null?art.SlavicTreeTall:art.SlavicTree;
                if(tree!=null)
                {
                    var sideTrees=new[]{
                        new Vector3(-13.8f,.10f,-1.0f),new Vector3(-13.2f,.12f,2.7f),new Vector3(-12.7f,.12f,6.0f),new Vector3(-12.2f,.12f,9.4f),new Vector3(-13.1f,.12f,11.8f),
                        new Vector3(13.7f,.10f,-.8f),new Vector3(13.1f,.12f,2.9f),new Vector3(12.6f,.12f,6.2f),new Vector3(12.1f,.12f,9.6f),new Vector3(13.0f,.12f,12.0f)
                    };
                    for(int i=0;i<sideTrees.Length;i++)
                        AddPrefab(root,tree,"side rescued-rock tree "+i,sideTrees[i],
                            .80f+(i%3)*.10f,2.45f+(i%2)*.30f,(i*47)%360,FoliageTint);
                }

                if(art.SlavicBush!=null)
                {
                    var scrub=new[]{
                        new Vector3(-12.2f,.08f,.6f),new Vector3(-12.5f,.08f,4.5f),new Vector3(-11.9f,.08f,8.2f),
                        new Vector3(12.1f,.08f,.8f),new Vector3(12.4f,.08f,4.7f),new Vector3(11.8f,.08f,8.4f)
                    };
                    for(int i=0;i<scrub.Length;i++)
                        AddPrefab(root,art.SlavicBush,"side rescued-rock scrub "+i,scrub[i],
                            .58f,.62f,(i*31)%360,new Color(.37f,.43f,.33f,1f));
                }
            }
        }

        static void BuildMountainHorizon(Transform root)
        {
            // Iteration 13: atmosphere-only horizon.
            // Large pack mountains/cliffs read as repeated stage pieces; fog now owns the far-field transition.
        }

        static void BuildVegetationDepth(Transform root,ValoriaExternalAssetLibrary art)
        {
            if(art==null)return;
            var tree=art.SlavicTreeTall!=null?art.SlavicTreeTall:art.SlavicTree;
            if(tree==null)return;

            // Keep only six depth anchors. They break silhouettes without exploding renderer count.
            var anchors=new[]{
                new Vector3(-9.4f,.22f,2.8f),new Vector3(9.1f,.22f,3.0f),
                new Vector3(-7.4f,.24f,10.4f),new Vector3(7.5f,.24f,10.6f),
                new Vector3(-11.2f,.16f,-2.5f),new Vector3(10.8f,.16f,-2.2f)
            };
            for(int i=0;i<anchors.Length;i++)
                AddPrefab(root,tree,"depth anchor tree "+i,anchors[i],
                    .86f+(i%2)*.12f,2.7f+(i%3)*.30f,(i*67)%360,FoliageTint);
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
            RenderSettings.ambientSkyColor=new Color(.72f,.79f,.84f);
            RenderSettings.ambientEquatorColor=new Color(.48f,.50f,.48f);
            RenderSettings.ambientGroundColor=new Color(.24f,.24f,.22f);
            RenderSettings.ambientIntensity=.86f;
            RenderSettings.fog=true;
            RenderSettings.fogMode=FogMode.Linear;
            RenderSettings.fogColor=new Color(.60f,.67f,.71f);
            RenderSettings.fogStartDistance=22f;
            RenderSettings.fogEndDistance=58f;

            var camera=Camera.main;
            if(camera!=null)
            {
                camera.clearFlags=CameraClearFlags.SolidColor;
                camera.backgroundColor=new Color(.55f,.67f,.74f);
                camera.allowHDR=true;
            }

            foreach(var light in Object.FindObjectsByType<Light>(FindObjectsSortMode.None))
            {
                if(light.type!=LightType.Directional)continue;
                light.color=new Color(1f,.89f,.76f);
                light.intensity=Mathf.Max(light.intensity,1.12f);
                light.shadowStrength=.56f;
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
