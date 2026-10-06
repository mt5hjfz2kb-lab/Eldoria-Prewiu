using Eldoria.Domain;
using UnityEngine;
using UnityEngine.Rendering;

// Final gate restart marker: no gameplay semantics.
namespace Eldoria.Presentation
{
    /// <summary>
    /// First real bounded 4X world slice. Presentation only: authoritative resource,
    /// march, combat and save state remain in LocalGateway/PlayerState.
    /// </summary>
    public static class WorldRegion1Runtime
    {
        public static readonly Vector3 ValoriaPosition=new Vector3(0f,.12f,-6.5f);
        public static readonly Vector3 ForestPosition=new Vector3(-6.8f,.10f,5.6f);
        public static readonly Vector3 RuinPosition=new Vector3(5.2f,.10f,4.8f);
        public static readonly Vector3 ScoutPosition=new Vector3(8.4f,.10f,10.7f);
        public static readonly Vector3 QuarryPosition=new Vector3(8.4f,.10f,-.8f);

        static Transform root;
        static GameObject marchVisual;
        static ValoriaExternalAssetLibrary externalLibrary;
        static readonly Color Earth=new Color(.30f,.29f,.24f);
        static readonly Color EarthLight=new Color(.35f,.35f,.28f);
        static readonly Color Meadow=new Color(.23f,.30f,.21f);
        static readonly Color Pine=new Color(.16f,.27f,.18f);
        static readonly Color PineLight=new Color(.21f,.34f,.22f);
        static readonly Color Road=new Color(.28f,.22f,.16f);
        static readonly Color RoadEdge=new Color(.37f,.31f,.22f);
        static readonly Color Stone=new Color(.40f,.40f,.36f);
        static readonly Color WarmStone=new Color(.49f,.44f,.34f);
        static readonly Color Violet=new Color(.45f,.20f,.52f);
        static readonly Color Blue=new Color(.18f,.38f,.58f);

        public static void Create(PlayerState state)
        {
            foreach(var old in Object.FindObjectsByType<Camera>(FindObjectsSortMode.None))
                if(old.gameObject.name=="Isometric camera")Object.DestroyImmediate(old.gameObject);
            var prior=GameObject.Find("World Region 1 · root");
            if(prior!=null)Object.DestroyImmediate(prior);

            root=new GameObject("World Region 1 · root").transform;
            externalLibrary=ValoriaExternalAssetLibrary.Load();
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
            RenderSettings.ambientSkyColor=new Color(.38f,.42f,.42f);
            RenderSettings.ambientEquatorColor=new Color(.28f,.27f,.23f);
            RenderSettings.ambientGroundColor=new Color(.12f,.115f,.095f);
            RenderSettings.ambientIntensity=.92f;
            RenderSettings.fog=true;
            RenderSettings.fogMode=FogMode.Linear;
            RenderSettings.fogColor=new Color(.40f,.42f,.40f);
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
            cameraGo.transform.position=new Vector3(20f,23f,-22f);
            cameraGo.transform.LookAt(new Vector3(0f,0f,1.5f));

            var sunGo=new GameObject("World Region 1 · dusk key");
            sunGo.transform.SetParent(root,true);
            var sun=sunGo.AddComponent<Light>();
            sun.type=LightType.Directional;
            sun.color=new Color(1f,.90f,.76f);
            sun.intensity=1.35f;
            sun.shadows=LightShadows.Soft;
            sun.shadowStrength=.48f;
            sun.transform.rotation=Quaternion.Euler(50f,-31f,0f);
        }

        static void BuildGeography()
        {
            var baseGround=Primitive("World Region 1 · terrain base",PrimitiveType.Cube,
                new Vector3(0f,-.50f,3f),new Vector3(62f,.8f,56f),Earth);
            var groundCollider=baseGround.GetComponent<Collider>();
            if(groundCollider!=null)groundCollider.enabled=false;

            // Broad low-frequency colour masses replace the old repeated checker texture.
            GroundPatch("World Region 1 · west meadow",new Vector3(-14f,-.075f,8f),new Vector3(22f,.12f,30f),Meadow,-9f);
            GroundPatch("World Region 1 · east dryland",new Vector3(14f,-.07f,5f),new Vector3(20f,.11f,26f),EarthLight,11f);
            GroundPatch("World Region 1 · north moor",new Vector3(1f,-.06f,18f),new Vector3(34f,.10f,14f),new Color(.25f,.27f,.22f),-4f);
            GroundPatch("World Region 1 · Valoria approach",new Vector3(0f,-.05f,-7f),new Vector3(17f,.10f,10f),new Color(.36f,.32f,.24f),5f);

            foreach(var p in new[]{
                new Vector3(-22f,-1.4f,11f),new Vector3(-16f,-1.6f,23f),
                new Vector3(21f,-1.4f,16f),new Vector3(7f,-1.8f,27f)})
            {
                var mountain=ValoriaKit.TerrainPieceTinted("SM_Mountains_11",
                    "World Region 1 · mountain barrier",p,11f,6.5f,
                    Quaternion.Euler(0,(p.x+p.z)*5f,0),new Color(.34f,.36f,.34f,1f));
                Parent(mountain);
            }

            var centres=new[]{new Vector3(-13f,0,7f),new Vector3(-11f,0,17f),
                new Vector3(-1f,0,20f),new Vector3(16f,0,7f)};
            for(int cluster=0;cluster<centres.Length;cluster++)
            {
                GroundPatch("World Region 1 · forest floor",centres[cluster]+new Vector3(0,-.02f,0),
                    new Vector3(10f,.08f,9f),new Color(.18f,.245f,.17f),cluster*13f);
                for(int i=0;i<11;i++)
                {
                    float a=i*2.39996f+cluster*.71f;
                    float radius=1.0f+Mathf.Sqrt(i+.5f)*1.15f;
                    var p=centres[cluster]+new Vector3(Mathf.Cos(a)*radius,0,Mathf.Sin(a)*radius);
                    WorldTree("World Region 1 · forest mass",p,.80f+(i%3)*.10f,i+cluster);
                }
            }
            foreach(var p in new[]{new Vector3(-19f,0,-4f),new Vector3(18f,0,-5f),
                new Vector3(-18f,0,24f),new Vector3(20f,0,23f)})
                ValoriaKit.RockCluster("World Region 1 · edge geology",p,1.05f,6);
        }

        static void BuildRoutes()
        {
            RoadSegment("World Region 1 · Valoria main route",new Vector3(0f,.035f,-2.5f),8.5f,1.55f,0f);
            RoadSegment("World Region 1 · forest route",new Vector3(-3.0f,.04f,2.2f),8.2f,1.30f,-38f);
            RoadSegment("World Region 1 · ruin route",new Vector3(2.7f,.04f,2.0f),7.0f,1.25f,38f);
            RoadSegment("World Region 1 · threat route",new Vector3(6.7f,.04f,7.5f),6.2f,1.05f,29f);
            foreach(var p in new[]{
                new Vector3(-2.0f,0f,-.8f),new Vector3(1.7f,0f,-.2f),
                new Vector3(-4.7f,0f,3.6f),new Vector3(4.3f,0f,3.0f),
                new Vector3(6.8f,0f,6.0f),new Vector3(-5.5f,0f,6.9f)})
                NatureBush("World Region 1 · route scrub",p,.95f,(int)((p.x+12f)*7f+p.z));
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
                    if(span>.001f)city.transform.localScale*=5.3f/span;
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
            GroundPatch("World Region 1 · forest resource floor",ForestPosition+new Vector3(0,-.01f,0),
                new Vector3(6.8f,.09f,6.2f),new Color(.17f,.25f,.16f),8f);
            for(int i=0;i<9;i++)
            {
                float a=i*.78f;float r=.8f+(i%3)*.50f;
                WorldTree("World Region 1 · forest node pine",
                    ForestPosition+new Vector3(Mathf.Cos(a)*r,0,Mathf.Sin(a)*r),.90f+(i%2)*.10f,20+i);
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
            GroundPatch("World Region 1 · corruption stain",ScoutPosition+new Vector3(0,-.005f,0),
                new Vector3(5.0f,.08f,4.4f),new Color(.24f,.14f,.25f),13f);
            var body=Primitive("World Region 1 · scout silhouette",PrimitiveType.Capsule,
                ScoutPosition+new Vector3(0,.90f,0),new Vector3(.75f,1.45f,.75f),new Color(.18f,.13f,.19f));
            body.transform.SetParent(scout.transform,true);
            for(int i=0;i<3;i++)
            {
                var shard=Primitive("World Region 1 · corruption shard",PrimitiveType.Cube,
                    ScoutPosition+new Vector3((i-1)*.85f,.55f,.75f+Mathf.Abs(i-1)*.35f),
                    new Vector3(.28f,1.05f,.28f),Violet);
                shard.transform.rotation=Quaternion.Euler(0,25f+i*33f,18f*(i-1));
                shard.transform.SetParent(scout.transform,true);
                var col=shard.GetComponent<Collider>();if(col!=null)col.enabled=false;
            }
            ValoriaKit.RockCluster("World Region 1 · corruption rocks",ScoutPosition+new Vector3(0,0,.7f),.85f,5);
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

        static void GroundPatch(string name,Vector3 position,Vector3 scale,Color color,float yaw)
        {
            // Elliptical low-frequency masses avoid the tiled/checker-board read of large rectangles.
            var patch=Primitive(name,PrimitiveType.Cylinder,position,
                new Vector3(scale.x*.5f,scale.y*.5f,scale.z*.5f),color);
            patch.transform.rotation=Quaternion.Euler(0f,yaw,0f);
            var col=patch.GetComponent<Collider>();
            if(col!=null)col.enabled=false;
        }

        static void RoadSegment(string name,Vector3 centre,float length,float width,float yaw)
        {
            // Slightly drifting short pieces keep the strategic path readable without looking like
            // a rigid board-game strip. Gameplay topology stays entirely in independent hotspots.
            var holder=new GameObject(name);
            holder.transform.SetParent(root,true);
            holder.transform.position=centre;
            holder.transform.rotation=Quaternion.Euler(0f,yaw,0f);

            const int pieces=5;
            float pieceLength=length/pieces*1.18f;
            for(int i=0;i<pieces;i++)
            {
                float t=(i-(pieces-1)*.5f)/(pieces-1);
                float z=t*length*.82f;
                float x=Mathf.Sin((i+1)*1.31f)*width*.13f;
                float localYaw=Mathf.Sin(i*1.17f)*5.0f;

                var shoulder=Primitive(name+" · shoulder "+i,PrimitiveType.Cube,Vector3.zero,
                    new Vector3(width+.42f,.045f,pieceLength+.28f),RoadEdge);
                shoulder.transform.SetParent(holder.transform,false);
                shoulder.transform.localPosition=new Vector3(x,-.018f,z);
                shoulder.transform.localRotation=Quaternion.Euler(0f,localYaw,0f);
                var sc=shoulder.GetComponent<Collider>();if(sc!=null)sc.enabled=false;

                var road=Primitive(name+" · track "+i,PrimitiveType.Cube,Vector3.zero,
                    new Vector3(width,.07f,pieceLength),Road);
                road.transform.SetParent(holder.transform,false);
                road.transform.localPosition=new Vector3(x,.01f,z);
                road.transform.localRotation=Quaternion.Euler(0f,localYaw,0f);
                var rc=road.GetComponent<Collider>();if(rc!=null)rc.enabled=false;
            }
        }

        static void WorldTree(string name,Vector3 position,float scale,int variant)
        {
            var nature=NatureTreePrefab(variant);
            if(nature!=null)
            {
                var instance=ValoriaKit.BenchmarkPiece(name,nature,position,
                    2.05f*scale,2.9f*scale,Quaternion.Euler(0f,(variant*47)%360,0f));
                if(instance!=null)
                {
                    Parent(instance);
                    return;
                }
            }

            var holder=new GameObject(name);
            holder.transform.SetParent(root,true);
            holder.transform.position=position;

            var trunk=Primitive(name+" · trunk",PrimitiveType.Cylinder,
                position+new Vector3(0,.48f*scale,0),new Vector3(.20f*scale,.48f*scale,.20f*scale),
                new Color(.27f,.20f,.13f));
            trunk.transform.SetParent(holder.transform,true);
            var tc=trunk.GetComponent<Collider>();if(tc!=null)tc.enabled=false;

            var lower=Primitive(name+" · crown lower",PrimitiveType.Sphere,
                position+new Vector3(0,1.00f*scale,0),new Vector3(1.00f,.68f,1.00f)*scale,
                variant%2==0?Pine:PineLight);
            lower.transform.SetParent(holder.transform,true);
            var lc=lower.GetComponent<Collider>();if(lc!=null)lc.enabled=false;

            var upper=Primitive(name+" · crown upper",PrimitiveType.Sphere,
                position+new Vector3(.04f,1.48f*scale,-.02f),new Vector3(.74f,.64f,.74f)*scale,
                variant%2==0?PineLight:Pine);
            upper.transform.SetParent(holder.transform,true);
            var uc=upper.GetComponent<Collider>();if(uc!=null)uc.enabled=false;
        }

        static GameObject NatureTreePrefab(int variant)
        {
            if(externalLibrary==null)return null;
            switch(Mathf.Abs(variant)%4)
            {
                case 0:return externalLibrary.NatureTree01;
                case 1:return externalLibrary.NatureTree02;
                case 2:return externalLibrary.NatureTree03;
                default:return externalLibrary.NatureTree04;
            }
        }

        static void NatureBush(string name,Vector3 position,float scale,int variant)
        {
            if(externalLibrary==null||externalLibrary.NatureBush01==null)return;
            var bush=ValoriaKit.BenchmarkPiece(name,externalLibrary.NatureBush01,position,
                1.35f*scale,.85f*scale,Quaternion.Euler(0f,(variant*71)%360,0f));
            Parent(bush);
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
