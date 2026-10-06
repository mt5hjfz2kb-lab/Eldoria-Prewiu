using Eldoria.Domain;
using UnityEngine;
using UnityEngine.Rendering;

namespace Eldoria.Presentation
{
    /// <summary>
    /// First real bounded 4X world slice. Presentation only: authoritative resource,
    /// march, combat and save state remain in LocalGateway/PlayerState.
    /// </summary>
    public static class WorldRegion1Runtime
    {
        public static readonly Vector3 ValoriaPosition=new Vector3(0f,.12f,-11.5f);
        public static readonly Vector3 ForestPosition=new Vector3(-8.8f,.10f,7.8f);
        public static readonly Vector3 RuinPosition=new Vector3(6.4f,.10f,7.2f);
        public static readonly Vector3 ScoutPosition=new Vector3(11.4f,.10f,14.4f);
        public static readonly Vector3 QuarryPosition=new Vector3(11.8f,.10f,-.5f);

        static Transform root;
        static GameObject marchVisual;
        static readonly Color Earth=new Color(.225f,.215f,.185f);
        static readonly Color Pine=new Color(.12f,.20f,.145f);
        static readonly Color Stone=new Color(.34f,.34f,.315f);
        static readonly Color WarmStone=new Color(.43f,.39f,.32f);
        static readonly Color Violet=new Color(.34f,.16f,.42f);
        static readonly Color Blue=new Color(.13f,.31f,.48f);

        public static void Create(PlayerState state)
        {
            foreach(var old in Object.FindObjectsByType<Camera>(FindObjectsSortMode.None))
                if(old.gameObject.name=="Isometric camera")Object.DestroyImmediate(old.gameObject);
            var prior=GameObject.Find("World Region 1 · root");
            if(prior!=null)Object.DestroyImmediate(prior);

            root=new GameObject("World Region 1 · root").transform;
            ConfigureEnvironment();
            BuildGeography();
            BuildRoutes();
            BuildValoria();
            BuildForest(state);
            BuildRuin();
            BuildScout(state);
            BuildQuarry(state);
            AdoptLooseVisuals();
            Refresh(state);
            Physics.SyncTransforms();
        }

        public static void Refresh(PlayerState state)
        {
            if(root==null||state==null)return;
            var forest=GameObject.Find("World Region 1 · forest target");
            if(forest!=null)
            {
                var marker=forest.transform.Find("remaining");
                if(marker!=null)marker.gameObject.SetActive(state.ForestRemaining>0);
            }
            var scout=GameObject.Find("World Region 1 · corrupt scout visual");
            if(scout!=null)scout.SetActive(!state.ScoutDefeated);
            var scoutTarget=GameObject.Find("World Region 1 · corrupt scout target");
            if(scoutTarget!=null)
            {
                var c=scoutTarget.GetComponent<Collider>();
                if(c!=null)c.enabled=!state.ScoutDefeated;
            }
            UpdateMarch(state);
        }

        static void ConfigureEnvironment()
        {
            RenderSettings.ambientMode=AmbientMode.Trilight;
            RenderSettings.ambientSkyColor=new Color(.29f,.33f,.35f);
            RenderSettings.ambientEquatorColor=new Color(.20f,.19f,.17f);
            RenderSettings.ambientGroundColor=new Color(.08f,.075f,.065f);
            RenderSettings.ambientIntensity=.76f;
            RenderSettings.fog=true;
            RenderSettings.fogMode=FogMode.Linear;
            RenderSettings.fogColor=new Color(.31f,.34f,.34f);
            RenderSettings.fogStartDistance=38f;
            RenderSettings.fogEndDistance=92f;

            var cameraGo=new GameObject("Isometric camera");
            cameraGo.tag="MainCamera";
            cameraGo.transform.SetParent(root,true);
            var camera=cameraGo.AddComponent<Camera>();
            camera.orthographic=true;
            camera.orthographicSize=14f;
            camera.clearFlags=CameraClearFlags.SolidColor;
            camera.backgroundColor=RenderSettings.fogColor;
            cameraGo.transform.position=new Vector3(23f,25f,-27f);
            cameraGo.transform.LookAt(new Vector3(0f,0f,2.5f));

            var sunGo=new GameObject("World Region 1 · dusk key");
            sunGo.transform.SetParent(root,true);
            var sun=sunGo.AddComponent<Light>();
            sun.type=LightType.Directional;
            sun.color=new Color(1f,.90f,.76f);
            sun.intensity=1.18f;
            sun.shadows=LightShadows.Soft;
            sun.shadowStrength=.48f;
            sun.transform.rotation=Quaternion.Euler(50f,-31f,0f);
        }

        static void BuildGeography()
        {
            var baseGround=Primitive("World Region 1 · terrain base",PrimitiveType.Cube,
                new Vector3(0f,-.48f,4f),new Vector3(74f,.8f,68f),Earth);
            baseGround.GetComponent<Renderer>().sharedMaterial=
                ValoriaKit.SurfaceMaterial(Earth,"earth",new Vector2(13f,12f));

            foreach(var p in new[]{
                new Vector3(-24f,-1.4f,11f),new Vector3(-20f,-1.6f,25f),
                new Vector3(23f,-1.4f,18f),new Vector3(5f,-1.8f,31f)})
            {
                var mountain=ValoriaKit.TerrainPieceTinted("SM_Mountains_11",
                    "World Region 1 · mountain barrier",p,12f,7f,
                    Quaternion.Euler(0,(p.x+p.z)*5f,0),new Color(.27f,.29f,.285f,1f));
                Parent(mountain);
            }

            // Forest masses form geography first; the interactable wood node remains independent.
            for(int cluster=0;cluster<4;cluster++)
            {
                var centre=new[]{new Vector3(-15f,0,8f),new Vector3(-13f,0,20f),
                    new Vector3(-3f,0,25f),new Vector3(19f,0,6f)}[cluster];
                for(int i=0;i<10;i++)
                {
                    float a=i*2.39996f+cluster*.71f;
                    float radius=1.5f+Mathf.Sqrt(i+.5f)*1.25f;
                    var p=centre+new Vector3(Mathf.Cos(a)*radius,0,Mathf.Sin(a)*radius);
                    ValoriaKit.PineTree("World Region 1 · forest mass",p,.60f+(i%3)*.08f);
                }
            }
            foreach(var p in new[]{new Vector3(-20f,0,-5f),new Vector3(20f,0,-6f),
                new Vector3(-21f,0,28f),new Vector3(22f,0,27f)})
            {
                ValoriaKit.RockCluster("World Region 1 · edge geology",p,1.15f,7);
            }
        }

        static void BuildRoutes()
        {
            Parent(WorldRouteKit.MarchRoute("World Region 1 · Valoria main route",
                new Vector3(0f,.04f,-4.5f),14f,2.1f,0f));
            Parent(WorldRouteKit.MarchRoute("World Region 1 · forest route",
                new Vector3(-4.1f,.05f,3.2f),12f,1.65f,-39f));
            Parent(WorldRouteKit.MarchRoute("World Region 1 · ruin route",
                new Vector3(3.2f,.05f,3.1f),10.5f,1.55f,36f));
            Parent(WorldRouteKit.MarchRoute("World Region 1 · threat route",
                new Vector3(8.8f,.05f,10.5f),8.6f,1.35f,30f));
        }

        static void BuildValoria()
        {
            var cityRoot=new GameObject("World Region 1 · Valoria");
            cityRoot.transform.SetParent(root,true);
            cityRoot.transform.position=ValoriaPosition;

            var prefab=Resources.Load<GameObject>("WorldPlayerCity/PlayerCity_v1");
            if(prefab!=null)
            {
                var city=Object.Instantiate(prefab);
                city.name="World Region 1 · Player City v1";
                city.transform.SetParent(cityRoot.transform,true);
                foreach(var c in city.GetComponentsInChildren<Collider>(true))c.enabled=false;
                foreach(var b in city.GetComponentsInChildren<MonoBehaviour>(true))b.enabled=false;
                var rs=city.GetComponentsInChildren<Renderer>(true);
                if(rs.Length>0)
                {
                    var bounds=rs[0].bounds;
                    for(int i=1;i<rs.Length;i++)bounds.Encapsulate(rs[i].bounds);
                    float span=Mathf.Max(bounds.size.x,bounds.size.z);
                    if(span>.001f)city.transform.localScale*=4.3f/span;
                    rs=city.GetComponentsInChildren<Renderer>(true);
                    bounds=rs[0].bounds;for(int i=1;i<rs.Length;i++)bounds.Encapsulate(rs[i].bounds);
                    city.transform.position+=ValoriaPosition-new Vector3(bounds.center.x,bounds.min.y,bounds.center.z);
                }
            }
            else
            {
                var keep=Primitive("World Region 1 · Valoria fallback keep",PrimitiveType.Cube,
                    ValoriaPosition+new Vector3(0,1.2f,0),new Vector3(3.5f,2.3f,3.1f),WarmStone);
                keep.transform.SetParent(cityRoot.transform,true);
            }
            ValoriaKit.Banner("World Region 1 · Valoria standard",
                ValoriaPosition+new Vector3(1.8f,.2f,.4f),new Vector3(.35f,1.45f,.06f),Blue);
            Hotspot("World Region 1 · Valoria target","valoria-map-city",
                ValoriaPosition+new Vector3(0,1.2f,0),new Vector3(5.3f,3.0f,5.0f));
        }

        static void BuildForest(PlayerState state)
        {
            var node=new GameObject("World Region 1 · forest resource");
            node.transform.SetParent(root,true);node.transform.position=ForestPosition;
            for(int i=0;i<8;i++)
            {
                float a=i*.78f;float r=1.0f+(i%3)*.55f;
                ValoriaKit.PineTree("World Region 1 · forest node pine",
                    ForestPosition+new Vector3(Mathf.Cos(a)*r,0,Mathf.Sin(a)*r),.68f+(i%2)*.08f);
            }
            var stock=Resources.Load<GameObject>("Valoria/UrbanProps/Crate");
            if(stock!=null)Parent(ValoriaKit.BenchmarkPieceTinted("World Region 1 · forest timber stock",
                stock,ForestPosition+new Vector3(.2f,.05f,-1.2f),1.35f,.85f,Quaternion.Euler(0,14f,0),
                new Color(.46f,.34f,.22f)));
            var target=Hotspot("World Region 1 · forest target","forest-valoria",
                ForestPosition+new Vector3(0,1f,0),new Vector3(5.0f,2.3f,4.7f));
            var remaining=Primitive("remaining",PrimitiveType.Cylinder,
                ForestPosition+new Vector3(0,.10f,0),new Vector3(1.6f,.12f,1.6f),Pine);
            remaining.transform.SetParent(target.transform,true);
            remaining.SetActive(state.ForestRemaining>0);
        }

        static void BuildRuin()
        {
            var ruin=new GameObject("World Region 1 · old watch ruin");
            ruin.transform.SetParent(root,true);
            var rescued=Resources.Load<GameObject>("Valoria/Rescued/TowerWallRock");
            if(rescued!=null)
                Parent(ValoriaKit.BenchmarkPieceTinted("World Region 1 · ruin authored",
                    rescued,RuinPosition,4.2f,3.3f,Quaternion.Euler(0,-18f,0),Stone));
            else
            {
                var tower=Primitive("World Region 1 · ruin fallback",PrimitiveType.Cube,
                    RuinPosition+new Vector3(0,1.0f,0),new Vector3(2.4f,2f,2.0f),Stone);
                tower.transform.SetParent(ruin.transform,true);
            }
            ValoriaKit.RockCluster("World Region 1 · ruin rubble",RuinPosition+new Vector3(1.6f,0,-.5f),.7f,6);
            Hotspot("World Region 1 · ruin target","old-watch-ruin",
                RuinPosition+new Vector3(0,1.2f,0),new Vector3(4.8f,3f,4.4f));
        }

        static void BuildScout(PlayerState state)
        {
            var scout=new GameObject("World Region 1 · corrupt scout visual");
            scout.transform.SetParent(root,true);scout.transform.position=ScoutPosition;
            var body=Primitive("World Region 1 · scout silhouette",PrimitiveType.Capsule,
                ScoutPosition+new Vector3(0,.85f,0),new Vector3(.65f,1.3f,.65f),new Color(.16f,.13f,.16f));
            body.transform.SetParent(scout.transform,true);
            ValoriaKit.RockCluster("World Region 1 · corruption rocks",ScoutPosition+new Vector3(0,0,.7f),.9f,6);
            var glowGo=new GameObject("World Region 1 · corruption glow");
            glowGo.transform.SetParent(scout.transform,true);glowGo.transform.position=ScoutPosition+Vector3.up*.65f;
            var glow=glowGo.AddComponent<Light>();glow.type=LightType.Point;glow.color=Violet;glow.intensity=.7f;glow.range=4.5f;
            scout.SetActive(!state.ScoutDefeated);
            var target=Hotspot("World Region 1 · corrupt scout target","corrupt-scout",
                ScoutPosition+new Vector3(0,1f,0),new Vector3(3.4f,2.8f,3.4f));
            target.GetComponent<Collider>().enabled=!state.ScoutDefeated;
        }

        static void BuildQuarry(PlayerState state)
        {
            Parent(WorldResourceKit.QuarryResourcePocket("World Region 1 · quarry resource",QuarryPosition,2.9f));
            var target=Hotspot("World Region 1 · quarry target","quarry-valoria",
                QuarryPosition+new Vector3(0,1f,0),new Vector3(4.8f,2.5f,4.4f));
            if(state.QuarryRemaining<=0)
            {
                var c=target.GetComponent<Collider>();
                if(c!=null)c.enabled=false;
            }
        }

        static void AdoptLooseVisuals()
        {
            // Several legacy ValoriaKit helpers intentionally create multiple sibling primitives and
            // return void. Region 1 owns their lifecycle, so adopt every loose world-prefixed root
            // before the frame is exposed. Rebuilding the world can then destroy one root without
            // leaking or duplicating vegetation, rubble or banners.
            foreach(var tr in Object.FindObjectsByType<Transform>(FindObjectsSortMode.None))
            {
                if(tr==null||tr==root||tr.parent!=null)continue;
                if(!tr.name.StartsWith("World Region 1 ·"))continue;
                tr.SetParent(root,true);
            }
        }

        static void UpdateMarch(PlayerState state)
        {
            if(state.March==null||state.March.Phase=="idle")
            {
                if(marchVisual!=null)Object.Destroy(marchVisual);
                marchVisual=null;return;
            }
            if(marchVisual==null)
            {
                marchVisual=new GameObject("World Region 1 · active march");
                marchVisual.transform.SetParent(root,true);
                var rider=Primitive("World Region 1 · march standard",PrimitiveType.Capsule,
                    Vector3.zero,new Vector3(.34f,.58f,.34f),Blue);
                rider.transform.SetParent(marchVisual.transform,false);
                var flag=Primitive("World Region 1 · march flag",PrimitiveType.Cube,
                    new Vector3(.28f,.65f,0),new Vector3(.48f,.38f,.05f),Blue);
                flag.transform.SetParent(marchVisual.transform,false);
                foreach(var c in marchVisual.GetComponentsInChildren<Collider>(true))c.enabled=false;
            }
            Vector3 target=TargetPosition(state.March.TargetId);
            float t=state.March.Phase=="outbound"?.48f:state.March.Phase=="returning"?.58f:1f;
            if(state.March.Phase=="returning")
                marchVisual.transform.position=Vector3.Lerp(target,ValoriaPosition,t);
            else
                marchVisual.transform.position=Vector3.Lerp(ValoriaPosition,target,t);
        }

        static Vector3 TargetPosition(string id)
        {
            if(id=="forest-valoria")return ForestPosition;
            if(id=="quarry-valoria")return QuarryPosition;
            if(id=="corrupt-scout"||id=="engendro-valoria")return ScoutPosition;
            return RuinPosition;
        }

        static GameObject Hotspot(string name,string id,Vector3 p,Vector3 scale)
        {
            var go=Primitive(name,PrimitiveType.Cube,p,scale,new Color(.1f,.1f,.1f,.01f));
            go.AddComponent<WorldHotspot>().Id=id;
            go.GetComponent<Renderer>().enabled=false;
            return go;
        }

        static GameObject Primitive(string name,PrimitiveType type,Vector3 p,Vector3 scale,Color color)
        {
            var go=GameObject.CreatePrimitive(type);
            go.name=name;go.transform.position=p;go.transform.localScale=scale;
            go.GetComponent<Renderer>().sharedMaterial=ValoriaKit.Material(color);
            if(root!=null)go.transform.SetParent(root,true);
            return go;
        }

        static void Parent(GameObject go){if(go!=null&&root!=null)go.transform.SetParent(root,true);}
    }
}
