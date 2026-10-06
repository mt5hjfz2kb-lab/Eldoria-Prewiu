using Eldoria.Domain;
using UnityEngine;
using UnityEngine.Rendering;

// Final gate restart marker: no gameplay semantics.
// Region 1 final visual certification marker.
// Region 1 visual convergence candidate: certify full runtime + capture.
// Region 1 certified-terrain candidate restart marker.
// Region 1 certified-terrain final gate retry.
// Combined-main Region 1 final visual certification marker.
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
        static readonly Color Earth=new Color(.38f,.35f,.28f);
        static readonly Color EarthLight=new Color(.42f,.40f,.31f);
        static readonly Color Meadow=new Color(.29f,.34f,.23f);
        static readonly Color Pine=new Color(.12f,.24f,.15f);
        static readonly Color PineLight=new Color(.18f,.31f,.19f);
        static readonly Color Road=new Color(.20f,.145f,.095f);
        static readonly Color RoadEdge=new Color(.33f,.27f,.19f);
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
            ValoriaWorldFrameMountainTerrainV1.Build(root,state);
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
            RenderSettings.ambientSkyColor=new Color(.46f,.47f,.43f);
            RenderSettings.ambientEquatorColor=new Color(.34f,.32f,.27f);
            RenderSettings.ambientGroundColor=new Color(.16f,.145f,.12f);
            RenderSettings.ambientIntensity=1.08f;
            RenderSettings.fog=true;
            RenderSettings.fogMode=FogMode.Linear;
            RenderSettings.fogColor=new Color(.46f,.46f,.40f);
            RenderSettings.fogStartDistance=42f;
            RenderSettings.fogEndDistance=104f;

            var cameraGo=new GameObject("Isometric camera");
            cameraGo.tag="MainCamera";
            cameraGo.transform.SetParent(root,true);
            var camera=cameraGo.AddComponent<Camera>();
            camera.orthographic=true;
            float aspect=Screen.height>0?Screen.width/(float)Screen.height:1.777f;
            camera.orthographicSize=aspect<.72f?18f:14f;
            camera.clearFlags=CameraClearFlags.SolidColor;
            camera.backgroundColor=RenderSettings.fogColor;
            cameraGo.transform.position=new Vector3(20f,23f,-22f);
            cameraGo.transform.LookAt(new Vector3(0f,0f,1.5f));

            var sunGo=new GameObject("World Region 1 · dusk key");
            sunGo.transform.SetParent(root,true);
            var sun=sunGo.AddComponent<Light>();
            sun.type=LightType.Directional;
            sun.color=new Color(1f,.90f,.76f);
            sun.intensity=1.48f;
            sun.shadows=LightShadows.Soft;
            sun.shadowStrength=.48f;
            sun.transform.rotation=Quaternion.Euler(50f,-31f,0f);
        }

        static void BuildGeography()
        {
            BuildTerrainBase();
            BuildSurfaceDressing();

            GroundPatch("World Region 1 · west meadow",new Vector3(-14f,-.05f,8f),
                new Vector3(19f,.08f,25f),Meadow*.94f,-9f);
            GroundPatch("World Region 1 · east dryland",new Vector3(14f,-.05f,5f),
                new Vector3(17f,.08f,22f),EarthLight*.91f,11f);
            GroundPatch("World Region 1 · north moor",new Vector3(1f,-.045f,18f),
                new Vector3(28f,.07f,11f),new Color(.31f,.32f,.25f),-4f);
            GroundPatch("World Region 1 · Valoria approach",new Vector3(0f,-.04f,-7f),
                new Vector3(14f,.07f,8f),new Color(.40f,.36f,.28f),5f);

            int mountainIndex=0;
            foreach(var p in new[]{
                new Vector3(-22f,-1.4f,11f),new Vector3(-16f,-1.6f,23f),
                new Vector3(21f,-1.4f,16f),new Vector3(7f,-1.8f,27f)})
            {
                var mountain=WorldInventoryPiece("Mountain01",
                    "World Region 1 · mountain barrier",p,13.5f,8.8f,
                    Quaternion.Euler(0f,(p.x+p.z)*5f+mountainIndex*23f,0f));
                if(mountain==null)
                    mountain=ValoriaKit.TerrainPieceTinted("SM_Mountains_11",
                        "World Region 1 · mountain barrier",p,11f,6.5f,
                        Quaternion.Euler(0f,(p.x+p.z)*5f,0f),new Color(.34f,.36f,.34f,1f));
                Parent(mountain);
                mountainIndex++;
            }

            var centres=new[]{new Vector3(-13f,0,7f),new Vector3(-11f,0,17f),
                new Vector3(-1f,0,20f),new Vector3(16f,0,7f)};
            for(int cluster=0;cluster<centres.Length;cluster++)
            {
                GroundPatch("World Region 1 · forest floor A",centres[cluster]+new Vector3(-1.2f,-.018f,.5f),
                    new Vector3(6.6f,.045f,5.8f),new Color(.245f,.30f,.21f),cluster*13f-8f);
                GroundPatch("World Region 1 · forest floor B",centres[cluster]+new Vector3(1.4f,-.017f,-.6f),
                    new Vector3(5.8f,.042f,5.1f),new Color(.22f,.28f,.19f),cluster*13f+17f);
                for(int i=0;i<18;i++)
                {
                    float a=i*2.39996f+cluster*.71f;
                    float radius=.75f+Mathf.Sqrt(i+.5f)*.98f;
                    var p=centres[cluster]+new Vector3(Mathf.Cos(a)*radius,0,Mathf.Sin(a)*radius);
                    WorldTree("World Region 1 · forest mass",p,.72f+(i%5)*.06f,i+cluster);
                }
            }

            int geologyIndex=0;
            foreach(var p in new[]{new Vector3(-19f,0,-4f),new Vector3(18f,0,-5f),
                new Vector3(-18f,0,24f),new Vector3(20f,0,23f),
                new Vector3(-2f,0,12f),new Vector3(12f,0,6f)})
                WorldRock("World Region 1 · edge geology",p,1.55f,geologyIndex++);
        }

        static void BuildSurfaceDressing()
        {
            if(externalLibrary==null)return;
            var moss=externalLibrary.SlavicMoss;
            var mud=externalLibrary.SlavicMudFlat;
            var items=new[]{
                (moss,new Vector3(-11f,-.30f,10f),5.8f,18f),
                (moss,new Vector3(-2f,-.30f,18f),5.2f,-12f),
                (mud,new Vector3(10f,-.30f,4f),5.6f,11f),
                (mud,new Vector3(1f,-.30f,-5f),4.8f,-6f)
            };
            int i=0;
            foreach(var item in items)
            {
                if(item.Item1==null)continue;
                var patch=ValoriaKit.BenchmarkPiece("World Region 1 · authored surface "+i,
                    item.Item1,item.Item2,item.Item3,.22f,Quaternion.Euler(0f,item.Item4,0f));
                if(patch!=null)
                {
                    Parent(patch);
                    foreach(var col in patch.GetComponentsInChildren<Collider>(true))col.enabled=false;
                }
                i++;
            }
        }

        static void BuildRoutes()
        {
            RoadSegment("World Region 1 · Valoria main route",new Vector3(0f,.025f,-2.5f),8.5f,1.42f,0f);
            RoadSegment("World Region 1 · forest route",new Vector3(-3.0f,.03f,2.2f),8.2f,1.18f,-38f);
            RoadSegment("World Region 1 · ruin route",new Vector3(2.7f,.03f,2.0f),7.0f,1.13f,38f);
            RoadSegment("World Region 1 · threat route",new Vector3(6.7f,.03f,7.5f),6.2f,.98f,29f);
            foreach(var p in new[]{
                new Vector3(-2.0f,0f,-.8f),new Vector3(1.7f,0f,-.2f),
                new Vector3(-4.7f,0f,3.6f),new Vector3(4.3f,0f,3.0f),
                new Vector3(6.8f,0f,6.0f),new Vector3(-5.5f,0f,6.9f)})
                WorldShrub("World Region 1 · route scrub",p,.42f,(int)((p.x+12f)*7f+p.z));
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
                    if(span>.001f)city.transform.localScale*=8.2f/span;
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
            for(int i=0;i<13;i++)
            {
                float a=i*.64f;float r=.72f+(i%4)*.42f;
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

            var arch=WorldInventoryPiece("Arch_Gothic","World Region 1 · ruin arch",
                RuinPosition+new Vector3(-.75f,0f,.25f),4.8f,5.0f,Quaternion.Euler(0,-18f,0));
            var wall=WorldInventoryPiece("Wall_Broken","World Region 1 · ruin wall",
                RuinPosition+new Vector3(1.25f,0f,.65f),4.3f,3.6f,Quaternion.Euler(0,38f,0));
            Parent(arch);Parent(wall);

            if(arch==null&&wall==null)
            {
                var rescued=Resources.Load<GameObject>("Valoria/Rescued/TowerWallRock");
                if(rescued!=null)
                    Parent(ValoriaKit.BenchmarkPieceTinted("World Region 1 · ruin authored",
                        rescued,RuinPosition,4.2f,3.3f,Quaternion.Euler(0,-18f,0),Stone));
            }

            var rubble=WorldInventoryPiece("Rock02","World Region 1 · ruin rubble",
                RuinPosition+new Vector3(1.55f,0f,-.55f),2.5f,1.15f,Quaternion.Euler(0,27f,0));
            if(rubble==null)ValoriaKit.RockCluster("World Region 1 · ruin rubble",RuinPosition+new Vector3(1.6f,0,-.5f),.7f,6);
            else Parent(rubble);

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
                ScoutPosition+new Vector3(0,.90f,0),new Vector3(1.0f,1.9f,1.0f),new Color(.18f,.13f,.19f));
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

        static void BuildTerrainBase()
        {
            const int xSteps=24;
            const int zSteps=22;
            const float width=108f;
            const float depth=94f;
            var vertices=new Vector3[(xSteps+1)*(zSteps+1)];
            var uv=new Vector2[vertices.Length];
            var triangles=new int[xSteps*zSteps*6];

            int v=0;
            for(int z=0;z<=zSteps;z++)
            for(int x=0;x<=xSteps;x++)
            {
                float nx=x/(float)xSteps;
                float nz=z/(float)zSteps;
                float px=(nx-.5f)*width;
                float pz=(nz-.5f)*depth+4f;
                float broad=Mathf.Sin(px*.085f)*.18f+Mathf.Cos(pz*.071f)*.15f+
                    Mathf.Sin((px+pz)*.043f)*.10f;
                float centreFade=Mathf.Clamp01((Mathf.Abs(px)+Mathf.Abs(pz-3f))/34f);
                float y=-.42f+broad*(.45f+.55f*centreFade);
                vertices[v]=new Vector3(px,y,pz);
                uv[v]=new Vector2(nx*10f,nz*9f);
                v++;
            }

            int t=0;
            for(int z=0;z<zSteps;z++)
            for(int x=0;x<xSteps;x++)
            {
                int a=z*(xSteps+1)+x;
                int b=a+1;
                int c0=a+(xSteps+1);
                int d=c0+1;
                triangles[t++]=a;triangles[t++]=c0;triangles[t++]=b;
                triangles[t++]=b;triangles[t++]=c0;triangles[t++]=d;
            }

            var go=new GameObject("World Region 1 · terrain base");
            go.transform.SetParent(root,true);
            var mesh=new Mesh{name="World Region 1 · terrain mesh",vertices=vertices,uv=uv,triangles=triangles};
            mesh.RecalculateNormals();mesh.RecalculateBounds();
            go.AddComponent<MeshFilter>().sharedMesh=mesh;
            var terrainRenderer=go.AddComponent<MeshRenderer>();
            terrainRenderer.sharedMaterial=externalLibrary!=null&&externalLibrary.ValoriaDirtSurface!=null
                ?ValoriaKit.PbrSurfaceMaterial(externalLibrary.ValoriaDirtSurface,Earth,new Vector2(12f,11f),.035f,.72f)
                :ValoriaKit.SurfaceMaterial(Earth,"earth",new Vector2(12f,11f));
        }

        static void WorldShrub(string name,Vector3 position,float scale,int variant)
        {
            if(variant%2==0)
                WorldRock(name+" · stones",position+new Vector3(.18f,0,-.12f),.34f,variant);
            if(variant%3==0)
                ValoriaKit.PineTree(name+" · sapling",position+new Vector3(-.18f,0,.12f),scale*.55f);
        }

        static void GroundPatch(string name,Vector3 position,Vector3 scale,Color color,float yaw)
        {
            const int sides=12;
            var vertices=new Vector3[sides+1];
            var uv=new Vector2[sides+1];
            var triangles=new int[sides*3];
            vertices[0]=Vector3.zero;uv[0]=new Vector2(.5f,.5f);
            int seed=Mathf.Abs(name.GetHashCode()%97);
            for(int i=0;i<sides;i++)
            {
                float angle=i*Mathf.PI*2f/sides;
                float jitter=.86f+(((i*37+seed*11)%17)/100f);
                float x=Mathf.Cos(angle)*scale.x*.5f*jitter;
                float z=Mathf.Sin(angle)*scale.z*.5f*(.90f+(((i*19+seed)%13)/100f));
                vertices[i+1]=new Vector3(x,0,z);
                uv[i+1]=new Vector2(.5f+x/Mathf.Max(.01f,scale.x),.5f+z/Mathf.Max(.01f,scale.z));
                int n=(i+1)%sides;
                triangles[i*3]=0;triangles[i*3+1]=i+1;triangles[i*3+2]=n+1;
            }
            var go=new GameObject(name);
            go.transform.SetParent(root,true);
            go.transform.position=position;
            go.transform.rotation=Quaternion.Euler(0f,yaw,0f);
            var mesh=new Mesh{name=name+" mesh",vertices=vertices,uv=uv,triangles=triangles};
            mesh.RecalculateNormals();mesh.RecalculateBounds();
            go.AddComponent<MeshFilter>().sharedMesh=mesh;
            var patchRenderer=go.AddComponent<MeshRenderer>();
            var tiling=new Vector2(Mathf.Max(1.5f,scale.x/3f),Mathf.Max(1.5f,scale.z/3f));
            patchRenderer.sharedMaterial=externalLibrary!=null&&externalLibrary.ValoriaDirtSurface!=null
                ?ValoriaKit.PbrSurfaceMaterial(externalLibrary.ValoriaDirtSurface,color,tiling,.025f,.62f)
                :ValoriaKit.SurfaceMaterial(color,"earth",tiling);
        }

        static void RoadSegment(string name,Vector3 centre,float length,float width,float yaw)
        {
            if(externalLibrary!=null&&externalLibrary.SlavicCobbleRoad!=null)
            {
                var holder=new GameObject(name);
                holder.transform.SetParent(root,true);
                holder.transform.position=centre;
                holder.transform.rotation=Quaternion.Euler(0f,yaw,0f);
                const int pieces=5;
                float spacing=length/(pieces-1);
                for(int i=0;i<pieces;i++)
                {
                    float z=(i-(pieces-1)*.5f)*spacing;
                    var piece=ValoriaKit.BenchmarkPiece(name+" · track "+i,
                        externalLibrary.SlavicCobbleRoad,Vector3.zero,
                        width*1.75f,.28f,Quaternion.identity);
                    if(piece==null)continue;
                    piece.transform.SetParent(holder.transform,false);
                    piece.transform.localPosition=new Vector3(
                        Mathf.Sin(i*1.4f)*width*.10f,.018f,z);
                    piece.transform.localRotation=Quaternion.Euler(0f,Mathf.Sin(i*.9f)*4f,0f);
                    foreach(var col in piece.GetComponentsInChildren<Collider>(true))col.enabled=false;
                }
                return;
            }

            var route=WorldRouteKit.MarchRoute(name,centre,length,width,yaw);
            if(route==null)return;
            route.transform.SetParent(root,true);
            foreach(var renderer in route.GetComponentsInChildren<Renderer>(true))
            {
                renderer.gameObject.name=name+" · track 0";
                break;
            }
            foreach(var collider in route.GetComponentsInChildren<Collider>(true))
                collider.enabled=false;
        }

        static void WorldTree(string name,Vector3 position,float scale,int variant)
        {
            var inventoryTree=WorldInventoryPiece(variant%2==0?"Tree01A":"Tree01B",
                name,position,2.45f*scale,4.1f*scale,
                Quaternion.Euler(0f,(variant*47f)%360f,0f));
            if(inventoryTree!=null)
            {
                Parent(inventoryTree);
                foreach(var col in inventoryTree.GetComponentsInChildren<Collider>(true))col.enabled=false;
                return;
            }

            // Fallback keeps the Valoria conifer language if the donor is unavailable.
            ValoriaKit.PineTree(name,position,scale*(1.05f+(variant%4)*.06f));
        }

        static void WorldRock(string name,Vector3 position,float scale,int variant)
        {
            var inventoryRock=WorldInventoryPiece(variant%2==0?"Rock01":"Rock02",
                name,position,3.0f*scale,1.65f*scale,
                Quaternion.Euler(0f,(variant*61f)%360f,0f));
            if(inventoryRock!=null)
            {
                Parent(inventoryRock);
                foreach(var col in inventoryRock.GetComponentsInChildren<Collider>(true))col.enabled=false;
                return;
            }

            GameObject prefab=null;
            if(externalLibrary!=null)
                prefab=variant%2==0?externalLibrary.SlavicBoulder:externalLibrary.SlavicFlatRock;
            if(prefab!=null)
            {
                var rock=ValoriaKit.BenchmarkPiece(name,prefab,position,
                    2.2f*scale,1.35f*scale,Quaternion.Euler(0f,(variant*61f)%360f,0f));
                if(rock!=null)
                {
                    Parent(rock);
                    foreach(var col in rock.GetComponentsInChildren<Collider>(true))col.enabled=false;
                    return;
                }
            }
            ValoriaKit.RockCluster(name,position,.88f*scale,5);
        }

        static GameObject WorldInventoryPiece(string resourceName,string name,Vector3 ground,
            float footprint,float maxHeight,Quaternion rotation)
        {
            var prefab=Resources.Load<GameObject>("WorldInventory/"+resourceName);
            if(prefab==null)return null;
            return ValoriaKit.BenchmarkPiece(name,prefab,ground,footprint,maxHeight,rotation);
        }

        static Object NatureTreePrefab(int variant)
        {
            return null;
        }

        static void NatureBush(string name,Vector3 position,float scale,int variant)
        {
            WorldShrub(name,position,scale*.42f,variant);
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
