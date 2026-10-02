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
            BuildMountainHorizon(root);
            BuildVegetationDepth(root,art);
            BuildOccupationAndAtmosphere(root);
            RefineGlobalAtmosphere();

            DisableGameplay(root.gameObject);
        }

        static void BuildMonumentalFrame(Transform root,ValoriaExternalAssetLibrary art)
        {
            if(art==null)return;

            // Reference target uses broken monumental arches as a frame around the playable city.
            // Keep them outside circulation and let the Bastion remain the focal point.
            AddPrefab(root,art.MegaHalfGate,"left monumental broken arch",
                new Vector3(-11.6f,.30f,5.8f),8.8f,12.5f,18f,StoneTint);
            AddPrefab(root,art.MegaHalfGate,"right monumental broken arch",
                new Vector3(11.9f,.25f,6.2f),9.3f,12.9f,198f,StoneTint);

            AddPrefab(root,art.MegaDestroyedTower,"left ruin crown",
                new Vector3(-12.8f,1.25f,8.7f),4.2f,7.3f,32f,new Color(.55f,.54f,.51f,1f));
            AddPrefab(root,art.MegaDestroyedTower,"right ruin crown",
                new Vector3(13.2f,1.10f,8.4f),4.0f,7.0f,214f,new Color(.55f,.54f,.51f,1f));

            AddPrefab(root,art.MegaWallPassage,"rear left ruined passage",
                new Vector3(-7.8f,2.0f,12.3f),5.4f,5.8f,8f,new Color(.57f,.56f,.52f,1f));
            AddPrefab(root,art.MegaWallPassage,"rear right ruined passage",
                new Vector3(7.7f,2.0f,12.6f),5.4f,5.8f,174f,new Color(.57f,.56f,.52f,1f));
        }

        static void BuildCliffEnvelope(Transform root)
        {
            AddResource(root,"Valoria/SM_Cliffs_01","left cliff shelf",
                new Vector3(-12.5f,-1.55f,1.4f),12.5f,8.2f,28f,RockTint);
            AddResource(root,"Valoria/SM_Cliffs_03","right cliff shelf",
                new Vector3(12.2f,-1.55f,1.7f),12.5f,8.2f,208f,RockTint);
            AddResource(root,"Valoria/SM_Cliffs_03","rear cliff bridge",
                new Vector3(0f,-.85f,13.8f),13.2f,7.0f,92f,new Color(.43f,.45f,.45f,1f));

            // Lower edge mass makes the compact city feel carved into a vertical mountain rather than placed on a board.
            AddResource(root,"Valoria/SM_Hills_01","front left mountain shoulder",
                new Vector3(-9.4f,-3.2f,-6.2f),11.0f,6.5f,35f,new Color(.38f,.42f,.40f,1f));
            AddResource(root,"Valoria/SM_Hills_01","front right mountain shoulder",
                new Vector3(9.2f,-3.2f,-6.0f),11.0f,6.5f,215f,new Color(.38f,.42f,.40f,1f));
        }

        static void BuildMountainHorizon(Transform root)
        {
            var specs=new[]{
                new Vector4(-18f,30f,15.5f,8f),
                new Vector4(-8f,34f,18f,24f),
                new Vector4(3f,36f,20f,-8f),
                new Vector4(14f,32f,17f,-22f),
                new Vector4(23f,29f,14f,-36f)
            };
            for(int i=0;i<specs.Length;i++)
            {
                var s=specs[i];
                AddResource(root,"Valoria/SM_Mountains_11","horizon mountain "+i,
                    new Vector3(s.x,-4.8f,s.y),s.z,10.5f,s.w,new Color(.46f,.50f,.51f,1f));
            }
        }

        static void BuildVegetationDepth(Transform root,ValoriaExternalAssetLibrary art)
        {
            if(art==null)return;
            var tree=art.SlavicTreeTall!=null?art.SlavicTreeTall:art.SlavicTree;
            var small=art.SlavicTree;
            if(tree==null&&small==null)return;

            var clusters=new[]{
                new Vector3(-10.0f,.25f,3.0f),new Vector3(9.6f,.25f,3.4f),
                new Vector3(-7.7f,.28f,10.2f),new Vector3(7.9f,.28f,10.4f),
                new Vector3(-12.4f,.18f,-2.8f),new Vector3(11.8f,.18f,-2.4f)
            };
            for(int c=0;c<clusters.Length;c++)
            for(int i=0;i<5;i++)
            {
                float a=(c*71+i*137)*Mathf.Deg2Rad;
                float r=.8f+(i%3)*.58f;
                var p=clusters[c]+new Vector3(Mathf.Cos(a)*r,0f,Mathf.Sin(a)*r);
                AddPrefab(root,(i%3==0&&tree!=null)?tree:small,"depth tree "+c+"-"+i,
                    p,.85f+(i%3)*.23f,2.5f+(i%4)*.42f,(c*37+i*53)%360,FoliageTint);
            }

            if(art.SlavicBush!=null)
            for(int i=0;i<14;i++)
            {
                float a=i*2.39996f;
                float radius=7.8f+(i%4)*1.15f;
                AddPrefab(root,art.SlavicBush,"cliff scrub "+i,
                    new Vector3(Mathf.Cos(a)*radius,.20f,3.5f+Mathf.Sin(a)*radius),
                    .72f,.72f,i*31,new Color(.40f,.46f,.34f,1f));
            }
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
            RenderSettings.fogColor=new Color(.58f,.65f,.69f);
            RenderSettings.fogStartDistance=38f;
            RenderSettings.fogEndDistance=112f;

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

        static void AddResource(Transform root,string resource,string role,Vector3 anchor,float span,float maxHeight,float yaw,Color tint)
        {
            var source=Resources.Load<GameObject>(resource);
            if(source==null)return;
            AddPrefab(root,source,role,anchor,span,maxHeight,yaw,tint);
        }

        static void AddPrefab(Transform root,GameObject source,string role,Vector3 anchor,float span,float maxHeight,float yaw,Color tint)
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
            ApplyTint(go,tint);
            go.transform.SetParent(root,true);
            DisableGameplay(go);
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
