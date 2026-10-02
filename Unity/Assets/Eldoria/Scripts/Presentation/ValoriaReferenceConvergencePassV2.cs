using System;
using System.Collections.Generic;
using Eldoria.Domain;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Eldoria.Presentation
{
    // VALORIA REFERENCE CONVERGENCE PASS v2
    // Full-frame composition only; existing certified resources, visual-only.
    public static class ValoriaReferenceConvergencePassV2
    {
        public static bool Enabled = false;
        static Material stoneMat;
        static Material rockMat;

        public static void Build(Transform parent, PlayerState state)
        {
            if(!Enabled || parent==null || state==null || state.BastionLevel<3) return;
            if(GameObject.Find("Valoria · Reference Convergence v2 · visual only")!=null) return;

            var root=new GameObject("Valoria · Reference Convergence v2 · visual only").transform;
            root.SetParent(parent,true);

            // 1. Monumental framing. Existing gate/tower language enlarged and buried into cliffs.
            BuildMonumentalFrame(root);

            // 2. Deep terrain masses. These are background/edge silhouette devices, not gameplay floors.
            BuildLandscapeDepth(root);

            // 3. Tighten the compact nucleus with existing certified stone/terrain families.
            BuildVerticalCoreSupports(root);

            // 4. Whole-frame atmosphere only after geometry hierarchy exists.
            ApplyAtmosphere();
            AddCinematicWarmth(root);

            DisableGameplay(root.gameObject);
        }

        public static void BuildForGate(PlayerState state)
        {
            var parent=GameObject.Find("Valoria · integrated construction visual layer");
            if(parent==null) throw new InvalidOperationException("Valoria production visual root missing.");
            bool old=Enabled;Enabled=true;Build(parent.transform,state);Enabled=old;
        }

        static void BuildMonumentalFrame(Transform root)
        {
            // Reference target uses huge ruined arches to frame—not fill—the city.
            AddPrefab(root,"Valoria/Stone_Gate","left monumental ruin",
                new Vector3(-12.8f,1.1f,7.8f),new Vector3(5.7f,8.7f,3.4f),18f,Surface.Stone);
            AddPrefab(root,"Valoria/Stone_Gate","right monumental ruin",
                new Vector3(12.6f,1.1f,7.5f),new Vector3(5.9f,8.9f,3.5f),198f,Surface.Stone);

            AddPrefab(root,"Valoria/Stone_Tower","left broken tower",
                new Vector3(-9.7f,2.0f,9.5f),new Vector3(3.1f,7.6f,3.1f),12f,Surface.Stone);
            AddPrefab(root,"Valoria/Stone_Tower","right broken tower",
                new Vector3(9.8f,2.0f,9.3f),new Vector3(3.0f,7.3f,3.0f),205f,Surface.Stone);

            // Supporting wall fragments make the arches feel embedded rather than placed.
            AddPrefab(root,"Valoria/Stone_Wall","left ruin shoulder",
                new Vector3(-10.7f,.6f,5.3f),new Vector3(5.2f,2.1f,1.6f),22f,Surface.Stone);
            AddPrefab(root,"Valoria/Stone_Wall","right ruin shoulder",
                new Vector3(10.7f,.6f,5.2f),new Vector3(5.2f,2.1f,1.6f),202f,Surface.Stone);
        }

        static void BuildLandscapeDepth(Transform root)
        {
            // Existing environment prefabs create foreground cliffs and distant depth.
            AddPrefab(root,"Valoria/SM_Cliffs_01","foreground cliff west",
                new Vector3(-13.0f,-2.6f,1.2f),new Vector3(10.5f,5.6f,8.5f),28f,Surface.Rock);
            AddPrefab(root,"Valoria/SM_Cliffs_03","foreground cliff east",
                new Vector3(13.0f,-2.6f,1.0f),new Vector3(10.8f,5.7f,8.8f),205f,Surface.Rock);

            AddPrefab(root,"Valoria/SM_Hills_01","mid distance west",
                new Vector3(-16.5f,-3.4f,15.5f),new Vector3(16f,7f,12f),16f,Surface.Rock);
            AddPrefab(root,"Valoria/SM_Hills_01","mid distance east",
                new Vector3(16.2f,-3.5f,15.2f),new Vector3(16f,7f,12f),196f,Surface.Rock);

            AddPrefab(root,"Valoria/SM_Mountains_11","distant mountain west",
                new Vector3(-24f,-5f,27f),new Vector3(26f,17f,22f),7f,Surface.Rock);
            AddPrefab(root,"Valoria/SM_Mountains_11","distant mountain east",
                new Vector3(23f,-5.2f,29f),new Vector3(27f,18f,23f),191f,Surface.Rock);
        }

        static void BuildVerticalCoreSupports(Transform root)
        {
            AddTopAligned(root,"Valoria/TerrainTerraceKit_v1/BroadRockPlatform",
                "hero terrace west",new Vector3(-4.2f,1.35f,5.9f),4.8f,10f,Surface.Rock);
            AddTopAligned(root,"Valoria/TerrainTerraceKit_v1/BroadRockPlatform",
                "hero terrace east",new Vector3(4.2f,1.35f,5.9f),4.8f,170f,Surface.Rock);

            AddResource(root,"Valoria/StoneArchitectureKit_v1/HighStraightWall",
                "hero retaining spine west",new Vector3(-4.9f,.72f,5.3f),3.8f,88f,Surface.Stone);
            AddResource(root,"Valoria/StoneArchitectureKit_v1/HighStraightWall",
                "hero retaining spine east",new Vector3(4.9f,.72f,5.3f),3.8f,268f,Surface.Stone);

            AddResource(root,"Valoria/StoneArchitectureKit_v1/RockToWallTransition",
                "hero rock seam west",new Vector3(-3.3f,.58f,4.2f),1.85f,38f,Surface.Stone);
            AddResource(root,"Valoria/StoneArchitectureKit_v1/RockToWallTransition",
                "hero rock seam east",new Vector3(3.3f,.58f,4.2f),1.85f,218f,Surface.Stone);
        }

        static void ApplyAtmosphere()
        {
            RenderSettings.fog=true;
            RenderSettings.fogMode=FogMode.Linear;
            RenderSettings.fogColor=new Color(.57f,.63f,.66f);
            RenderSettings.fogStartDistance=28f;
            RenderSettings.fogEndDistance=78f;

            RenderSettings.ambientMode=UnityEngine.Rendering.AmbientMode.Flat;
            RenderSettings.ambientLight=new Color(.78f,.79f,.78f);

            foreach(var l in Object.FindObjectsByType<Light>(FindObjectsSortMode.None))
            {
                if(l.type==LightType.Directional)
                {
                    l.intensity=Mathf.Max(l.intensity,1.15f);
                    l.shadowStrength=.44f;
                }
            }
        }

        static void AddCinematicWarmth(Transform root)
        {
            AddWarm(root,"hero hearth west",new Vector3(-2.8f,2.4f,6.9f),.22f,3.4f);
            AddWarm(root,"hero hearth east",new Vector3(2.8f,2.4f,6.9f),.22f,3.4f);
            AddWarm(root,"lower civic warmth",new Vector3(0f,1.2f,1.2f),.14f,3.0f);
        }

        enum Surface { Stone, Rock }

        static void AddPrefab(Transform root,string resource,string role,Vector3 pos,Vector3 scale,float yaw,Surface family)
        {
            var source=Resources.Load<GameObject>(resource);
            if(source==null) throw new InvalidOperationException("Missing reference-convergence resource: "+resource);
            var go=Object.Instantiate(source);
            go.name="Valoria · Reference Convergence v2 · "+role;
            go.transform.SetPositionAndRotation(pos,Quaternion.Euler(0f,yaw,0f));
            go.transform.localScale=scale;
            ApplySurface(go,family);
            go.transform.SetParent(root,true);
            DisableGameplay(go);
        }

        static void AddResource(Transform root,string resource,string role,Vector3 anchor,float span,float yaw,Surface family)
        {
            var source=Resources.Load<GameObject>(resource);
            if(source==null) throw new InvalidOperationException("Missing reference-convergence resource: "+resource);
            var go=Object.Instantiate(source);
            go.name="Valoria · Reference Convergence v2 · "+role;
            go.transform.rotation=Quaternion.Euler(0f,yaw,0f);
            FitGround(go,anchor,span);
            ApplySurface(go,family);
            go.transform.SetParent(root,true);
            DisableGameplay(go);
        }

        static void AddTopAligned(Transform root,string resource,string role,Vector3 topAnchor,float span,float yaw,Surface family)
        {
            var source=Resources.Load<GameObject>(resource);
            if(source==null) throw new InvalidOperationException("Missing reference-convergence resource: "+resource);
            var go=Object.Instantiate(source);
            go.name="Valoria · Reference Convergence v2 · "+role;
            go.transform.rotation=Quaternion.Euler(0f,yaw,0f);
            var b=Bounds(go);
            go.transform.localScale*=span/Mathf.Max(.001f,Mathf.Max(b.size.x,b.size.z));
            b=Bounds(go);
            go.transform.position+=new Vector3(topAnchor.x-b.center.x,topAnchor.y-b.max.y,topAnchor.z-b.center.z);
            ApplySurface(go,family);
            go.transform.SetParent(root,true);
            DisableGameplay(go);
        }

        static void FitGround(GameObject go,Vector3 anchor,float span)
        {
            var b=Bounds(go);
            go.transform.localScale*=span/Mathf.Max(.001f,Mathf.Max(b.size.x,b.size.z));
            b=Bounds(go);
            go.transform.position+=anchor-new Vector3(b.center.x,b.min.y,b.center.z);
        }

        static Bounds Bounds(GameObject go)
        {
            var rs=go.GetComponentsInChildren<Renderer>(true);
            if(rs.Length==0)return new Bounds(go.transform.position,Vector3.zero);
            var b=rs[0].bounds;
            for(int i=1;i<rs.Length;i++)b.Encapsulate(rs[i].bounds);
            return b;
        }

        static void ApplySurface(GameObject go,Surface family)
        {
            var mat=family==Surface.Stone?SharedStone():SharedRock();
            foreach(var r in go.GetComponentsInChildren<Renderer>(true))
            {
                var arr=r.sharedMaterials;
                for(int i=0;i<arr.Length;i++) if(arr[i]!=null) arr[i]=mat;
                r.sharedMaterials=arr;
            }
        }

        static Material SharedStone()
        {
            if(stoneMat!=null)return stoneMat;
            var shader=Shader.Find("Universal Render Pipeline/Lit")??Shader.Find("Standard");
            stoneMat=new Material(shader){name="Valoria Reference v2 · Stone"};
            if(stoneMat.HasProperty("_BaseColor"))stoneMat.SetColor("_BaseColor",new Color(.63f,.61f,.56f,1f));
            if(stoneMat.HasProperty("_Color"))stoneMat.SetColor("_Color",new Color(.63f,.61f,.56f,1f));
            if(stoneMat.HasProperty("_Smoothness"))stoneMat.SetFloat("_Smoothness",.055f);
            return stoneMat;
        }

        static Material SharedRock()
        {
            if(rockMat!=null)return rockMat;
            var shader=Shader.Find("Universal Render Pipeline/Lit")??Shader.Find("Standard");
            rockMat=new Material(shader){name="Valoria Reference v2 · Rock"};
            if(rockMat.HasProperty("_BaseColor"))rockMat.SetColor("_BaseColor",new Color(.39f,.41f,.40f,1f));
            if(rockMat.HasProperty("_Color"))rockMat.SetColor("_Color",new Color(.39f,.41f,.40f,1f));
            if(rockMat.HasProperty("_Smoothness"))rockMat.SetFloat("_Smoothness",.025f);
            return rockMat;
        }

        static void AddWarm(Transform root,string name,Vector3 p,float intensity,float range)
        {
            var go=new GameObject("Valoria · Reference Convergence v2 · "+name);
            go.transform.SetParent(root,true);go.transform.position=p;
            var l=go.AddComponent<Light>();
            l.type=LightType.Point;l.color=new Color(1f,.58f,.30f);l.intensity=intensity;l.range=range;l.shadows=LightShadows.None;
        }

        static void DisableGameplay(GameObject go)
        {
            foreach(var c in go.GetComponentsInChildren<Collider>(true))c.enabled=false;
            foreach(var h in go.GetComponentsInChildren<WorldHotspot>(true))Object.DestroyImmediate(h);
            foreach(var b in go.GetComponentsInChildren<MonoBehaviour>(true))if(!(b is WorldHotspot))b.enabled=false;
            Physics.SyncTransforms();
        }
    }
}
