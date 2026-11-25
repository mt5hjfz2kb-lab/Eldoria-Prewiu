using System;
using System.Collections.Generic;
using UnityEngine;

namespace Eldoria.SendaBrecha
{
    /// <summary>
    /// Isolated, self-contained mobile 3D lane-event proof. No game-state, wallet,
    /// save, army, world, HUD or beta runtime dependencies. All units are virtual.
    /// Temporary primitives are NOT approved production hero/asset replacements.
    /// </summary>
    public sealed class SendaBrechaPrototype : MonoBehaviour
    {
        public enum Phase { Intro, Running, Paused, Victory, Defeat }
        public enum GateType { Recruit, Power, Ward, Trap }
        [Serializable] public struct Gate
        {
            public GateType Type;
            public int Value;
            public Gate(GateType kind, int value) { Type = kind; Value = value; }
            public string Label => (Type == GateType.Power ? "x1," : Type == GateType.Trap ? "-" : "+") + Value;
        }
        [Serializable] public struct Wave
        {
            public string Name;
            public int Enemy;
            public Gate[] Gates;
            public Wave(string name, int enemy, Gate first, Gate second, Gate third)
            { Name = name; Enemy = enemy; Gates = new[] { first, second, third }; }
        }
        public static readonly Wave[] Waves =
        {
            new Wave("El Umbral",13,new Gate(GateType.Recruit,10),new Gate(GateType.Power,35),new Gate(GateType.Trap,5)),
            new Wave("Bosque Marchito",22,new Gate(GateType.Trap,7),new Gate(GateType.Recruit,12),new Gate(GateType.Ward,12)),
            new Wave("Puente de Cenizas",30,new Gate(GateType.Power,45),new Gate(GateType.Recruit,13),new Gate(GateType.Trap,9)),
            new Wave("Las Ruinas",37,new Gate(GateType.Recruit,16),new Gate(GateType.Trap,10),new Gate(GateType.Ward,18)),
            new Wave("La Grieta",45,new Gate(GateType.Power,50),new Gate(GateType.Recruit,16),new Gate(GateType.Trap,13)),
            new Wave("El Guardián",62,new Gate(GateType.Recruit,20),new Gate(GateType.Ward,24),new Gate(GateType.Power,55))
        };
        public const float WaveLength = 11f;
        public const float GateCrossing = 4.15f;
        public const float FightStart = 6.4f;
        public const float FightResolve = 9f;
        public int Stage { get; private set; }
        public int Lane { get; private set; } = 1;
        public int Soldiers { get; private set; } = 22;
        public int Power { get; private set; }
        public int Wards { get; private set; }
        public int Score { get; private set; }
        public Phase State { get; private set; } = Phase.Intro;
        public bool GateTaken { get; private set; }
        public bool FightCompleted { get; private set; }
        public float StageTime { get; private set; }
        public float Elapsed { get; private set; }
        public string LastEvent { get; private set; } = "";

        public Camera SceneCamera { get; private set; }
        // Low virtual troop counts must visibly change the actual actor formation.
        // The virtual army never spawns one GameObject per soldier on iPhone.
        public int VisibleSoldiers => Soldiers <= 0 ? 0 : Mathf.Clamp(2 + Mathf.CeilToInt(Soldiers / 4f), 1, 14);
        public bool ShowOverlay = true;
        public bool HasLoadedScene => initialized && SceneCamera != null && world != null;

        private Transform world;
        private GameObject[] soldierActors;
        private Transform[] soldierBodies;
        private GameObject[] enemies;
        private GameObject[] portals;
        private GameObject[] shots;
        private Transform[] environment;
        private readonly Dictionary<int, Material> materials = new Dictionary<int, Material>();
        private float movingX;
        private float fireClock;
        private float uiFlash;
        private Vector2 swipeStart;
        private int shotCursor;
        private bool initialized;
        private float sessionSpeed = 1f;
        private readonly Color night = new Color(.12f, .17f, .15f);
        private Transform[][] soldierLimbs, enemyLimbs;
        private Transform[] laneMarkers;
        private UnityEngine.Mesh taperedMesh;
        private Texture2D roadTexture, earthTexture;

        public static bool IsolatedFromMainGame => true;

        private Material Mat(Color c, float metallic = 0, float smoothness = .24f, bool glow = false)
        {
            int key = Color32Key(c) ^ (glow ? 0x4000000 : 0);
            if (materials.TryGetValue(key, out var existing)) return existing;
            // Use the actual active pipeline. A standalone minimal Unity validation project
            // may use Built-in even though Eldoria proper uses URP 17.3.
            bool urp = UnityEngine.Rendering.GraphicsSettings.currentRenderPipeline != null;
            Shader shader = Shader.Find(urp ? "Universal Render Pipeline/Lit" : "Standard");
            if (!shader) shader = Shader.Find(urp ? "Standard" : "Universal Render Pipeline/Lit");
            if (!shader) shader = Shader.Find("Unlit/Color");
            var material = new Material(shader) { name = "SendaBrecha provisional " + key };
            if (material.HasProperty("_BaseColor")) material.SetColor("_BaseColor", c);
            else if (material.HasProperty("_Color")) material.SetColor("_Color", c);
            if (material.HasProperty("_Metallic")) material.SetFloat("_Metallic", metallic);
            if (material.HasProperty("_Smoothness")) material.SetFloat("_Smoothness", smoothness);
            if (glow && material.HasProperty("_EmissionColor"))
            { material.EnableKeyword("_EMISSION"); material.SetColor("_EmissionColor", c * 1.8f); }
            materials[key] = material;
            return material;
        }
        private static int Color32Key(Color c)
        {
            var x = (Color32)c;
            return (x.r << 16) | (x.g << 8) | x.b;
        }
        private GameObject Mesh(string name, PrimitiveType type, Transform parent, Vector3 pos, Vector3 scale, Color color, float metal = 0, bool glow = false)
        {
            GameObject g = GameObject.CreatePrimitive(type);
            g.name = name;
            g.transform.SetParent(parent, false);
            g.transform.localPosition = pos;
            g.transform.localScale = scale;
            var renderer = g.GetComponent<MeshRenderer>();
            renderer.sharedMaterial = Mat(color, metal, .3f, glow);
            var collider = g.GetComponent<Collider>();
            if (collider) { if (UnityEngine.Application.isPlaying) Destroy(collider); else DestroyImmediate(collider); }
            return g;
        }
        private static Transform Empty(string name, Transform parent, Vector3 pos)
        {
            var g = new GameObject(name).transform;
            g.SetParent(parent, false);
            g.localPosition = pos;
            return g;
        }
        public void Initialize()
        {
            if (initialized) return;
            initialized = true;
            world = Empty("ISOLATED SENDA BRECHA / no beta state", transform, Vector3.zero);
            var environmentRoot = Empty("Environment and actual 3D geometry", world, Vector3.zero);
            BuildEnvironment(environmentRoot);
            BuildCameraAndLighting();
            soldierActors = new GameObject[14];
            soldierBodies = new Transform[14];
            soldierLimbs = new Transform[14][];
            for (int i = 0; i < soldierActors.Length; ++i)
            {
                var body = Empty(i == 0 ? "Guardian - original temporary 3D avatar" : "Valorian soldier " + i, world, Vector3.zero);
                soldierLimbs[i] = BuildSoldier(body, false, i == 0, false);
                soldierActors[i] = body.gameObject;
                soldierBodies[i] = body;
            }
            enemies = new GameObject[12];
            enemyLimbs = new Transform[12][];
            for (int i = 0; i < enemies.Length; ++i)
            {
                var body = Empty("Corrupted enemy visual proxy " + i, world, Vector3.zero);
                enemyLimbs[i] = BuildSoldier(body, true, false, i == 0);
                enemies[i] = body.gameObject;
            }
            portals = new GameObject[3];
            for (int i = 0; i < portals.Length; ++i)
            {
                portals[i] = Empty("Gate physical lane " + i, world, new Vector3((i-1)*2.15f,0,29)).gameObject;
                BuildGate(portals[i].transform);
            }
            shots = new GameObject[12];
            for (int i = 0; i < shots.Length; ++i)
            {
                shots[i] = Mesh("Arrow tracer / pooled " + i, PrimitiveType.Cylinder, world,
                    new Vector3(0,-10,0), new Vector3(.044f,.50f,.044f), new Color(.96f,.71f,.39f), .45f, true);
                shots[i].transform.localRotation = Quaternion.Euler(90,0,0);
            }
            RefreshWorld();
        }
        private void Awake() { Initialize(); }
        private void BuildCameraAndLighting()
        {
            var cameraObject = new GameObject("Senda isolated perspective camera");
            cameraObject.transform.SetParent(world,false);
            SceneCamera = cameraObject.AddComponent<Camera>();
            SceneCamera.fieldOfView = 46;
            SceneCamera.nearClipPlane = .18f;
            SceneCamera.farClipPlane = 95;
            SceneCamera.clearFlags = CameraClearFlags.SolidColor;
            SceneCamera.backgroundColor = night;
            SceneCamera.transform.localPosition = new Vector3(0, 12f, -18f);
            SceneCamera.transform.LookAt(new Vector3(0,.3f,10.5f));
            var sun = new GameObject("Slanted cold-sun with warm highlights");
            sun.transform.SetParent(world,false);
            var light = sun.AddComponent<Light>();
            light.type=LightType.Directional; light.color=new Color(.91f,.82f,.75f);
            light.intensity=1.10f;
            sun.transform.rotation=Quaternion.Euler(51,-37,0);
            var rimObject = new GameObject("Rift fill");rimObject.transform.SetParent(world,false);
            var rim=rimObject.AddComponent<Light>();rim.type=LightType.Point;
            rim.color=new Color(.43f,.25f,.68f);rim.range=13;rim.intensity=2.1f;
            rimObject.transform.position=new Vector3(0,5,32);
            RenderSettings.ambientMode=UnityEngine.Rendering.AmbientMode.Flat;
            RenderSettings.ambientLight=new Color(.38f,.43f,.39f);
            RenderSettings.fog=true;
            RenderSettings.fogColor=night;
            RenderSettings.fogMode=FogMode.Linear;
            RenderSettings.fogStartDistance=22f;
            RenderSettings.fogEndDistance=64f;
        }
        // Shared tapered geometry (one reusable mesh) and deterministic textured
        // materials replace the flat primitive-demo look. No external art or gameplay dependencies.
        private GameObject Taper(string name, Transform parent, Vector3 pos, Vector3 scale, Color c, float metal=0)
        {
            if(!taperedMesh)
            {
                const int n=10;
                var vertices=new List<Vector3>();
                var tris=new List<int>();
                for(int i=0;i<n;i++)
                {
                    float angle=i*Mathf.PI*2/n;
                    vertices.Add(new Vector3(Mathf.Cos(angle)*.5f,-.5f,Mathf.Sin(angle)*.5f));
                    vertices.Add(new Vector3(Mathf.Cos(angle)*.08f,.5f,Mathf.Sin(angle)*.08f));
                }
                vertices.Add(new Vector3(0,-.5f,0));vertices.Add(new Vector3(0,.5f,0));
                for(int i=0;i<n;i++)
                {
                    int a=2*i,b=2*((i+1)%n);
                    tris.AddRange(new[]{a,a+1,b,b,a+1,b+1,2*n,a,b,2*n+1,b+1,a+1});
                }
                taperedMesh=new UnityEngine.Mesh {name="Senda shared tapered 10-sided mesh"};
                taperedMesh.SetVertices(vertices);taperedMesh.SetTriangles(tris,0);taperedMesh.RecalculateNormals();taperedMesh.RecalculateBounds();
            }
            var g=new GameObject(name);g.transform.SetParent(parent,false);g.transform.localPosition=pos;g.transform.localScale=scale;
            g.AddComponent<MeshFilter>().sharedMesh=taperedMesh;g.AddComponent<MeshRenderer>().sharedMaterial=Mat(c,metal);
            return g;
        }
        private Texture2D GroundTexture(bool stone)
        {
            var t=new Texture2D(128,128,TextureFormat.RGB24,true);
            t.name=stone?"Senda procedural worn slate":"Senda procedural forest floor";
            var pixels=new Color32[128*128];
            var random=new System.Random(stone?7821:7812);
            for(int y=0;y<128;y++)for(int x=0;x<128;x++)
            {
                int row=y/16;int sx=(x+(row%2)*16)%32;
                bool seam=stone&&(y%16==0||sx==0);
                float v=(stone?.42f:.32f)+(float)random.NextDouble()*.13f;
                if(stone)v+=((row*17+((x+(row%2)*16)/32)*11)%5-2)*.014f;
                if(seam)v*=.60f;
                pixels[y*128+x]=stone?new Color(v,v*.89f,v*.72f):new Color(v*.76f,v*.87f,v*.55f);
            }
            t.SetPixels32(pixels);t.Apply(true,false);t.wrapMode=TextureWrapMode.Repeat;t.filterMode=FilterMode.Bilinear;
            return t;
        }
        private void TextureSurface(GameObject surface,Texture2D texture,Vector2 repeat)
        {
            var mat=surface.GetComponent<MeshRenderer>().sharedMaterial;
            string property=mat.HasProperty("_BaseMap")?"_BaseMap":"_MainTex";
            if(mat.HasProperty(property)){mat.SetTexture(property,texture);mat.SetTextureScale(property,repeat);}
        }
        private void BuildEnvironment(Transform parent)
        {
            Color rock=new Color(.34f,.32f,.27f);
            roadTexture=GroundTexture(true);earthTexture=GroundTexture(false);
            var floor=Mesh("Forest floor",PrimitiveType.Cube,parent,new Vector3(0,-.48f,22),new Vector3(80,.9f,125),new Color(.91f,.94f,.83f));
            var road=Mesh("Worn slate roadway",PrimitiveType.Cube,parent,new Vector3(0,-.11f,23),new Vector3(7.7f,.24f,124),new Color(.95f,.92f,.88f));
            TextureSurface(floor,earthTexture,new Vector2(24,38));
            TextureSurface(road,roadTexture,new Vector2(3,44));
            foreach(int side in new[]{-1,1})
                Mesh("Stone verge",PrimitiveType.Cube,parent,new Vector3(side*4.0f,-.04f,23),new Vector3(.45f,.23f,124),new Color(.36f,.31f,.25f));
            var features=new List<Transform>();var markers=new List<Transform>();
            // Actual lane centers are -2.15,0,+2.15. Separators at +/-1.075,
            // visual only, aligned with physical gate spacing.
            for(int i=0;i<44;i++)foreach(int side in new[]{-1,1})
                markers.Add(Mesh("Inset lane separator "+i,PrimitiveType.Cube,parent,new Vector3(side*1.075f,.026f,i*2.4f-9),new Vector3(.07f,.014f,.78f),new Color(.62f,.54f,.37f)).transform);
            laneMarkers=markers.ToArray();
            for(int i=0;i<54;i++)
            {
                float z=-5f+i*2.25f,x=((i*17)%23/23f-.5f)*6.5f;
                var stone=Mesh("Worn uneven paving "+i,PrimitiveType.Cube,parent,new Vector3(x,.025f,z),new Vector3(.40f+(i%4)*.15f,.021f,.24f+(i%3)*.10f),i%3==0?new Color(.40f,.35f,.29f):new Color(.27f,.25f,.21f));
                stone.transform.localRotation=Quaternion.Euler(0,(i%7-3)*9,0);
            }
            for(int i=0;i<42;i++)
            {
                float z=(i*7.19f)%85f-8f;
                int side=i%2==0?-1:1;float x=side*(5.3f+(i%7)*.72f);
                var scenic=Empty("Forest and weathered stone "+i,parent,new Vector3(x,0,z));
                if(i%6==0)
                {
                    var boulder=Mesh("Basalt",PrimitiveType.Sphere,scenic,new Vector3(0,.39f,0),new Vector3(1.45f,.88f,.92f),rock);
                    boulder.transform.localRotation=Quaternion.Euler(6,i*23,13);
                    Taper("Fracture shard",scenic,new Vector3(.25f,.88f,.07f),new Vector3(.30f,1.04f,.30f),new Color(.37f,.22f,.48f)).transform.localRotation=Quaternion.Euler(16,0,24);
                }
                else
                {
                    float v=.82f+(i%5)*.14f;
                    Mesh("Trunk",PrimitiveType.Cylinder,scenic,new Vector3(0,.78f*v,0),new Vector3(.27f*v,.78f*v,.27f*v),new Color(.25f,.19f,.15f));
                    for(int tier=0;tier<4;tier++)
                    {
                        var c=new Color(.16f+tier*.027f,.25f+tier*.027f,.17f+tier*.017f);
                        Taper("Tapered conifer crown "+tier,scenic,new Vector3(.05f*Mathf.Sin(i+tier),(1.35f+tier*.55f)*v,0),new Vector3((1.85f-tier*.31f)*v,(1.42f-tier*.10f)*v,(1.85f-tier*.31f)*v),c).transform.localRotation=Quaternion.Euler(0,i*17+tier*13,0);
                    }
                    Mesh("Mossy root stone",PrimitiveType.Sphere,scenic,new Vector3(.48f,.20f,.22f),new Vector3(.72f,.40f,.52f),rock);
                }
                features.Add(scenic);
            }
            environment=features.ToArray();
            for(int n=0;n<9;n++)foreach(int side in new[]{-1,1})
            {
                float z=2+n*11.9f,x=side*4.55f;
                Mesh("Ruined foundation",PrimitiveType.Cube,parent,new Vector3(x,.19f,z),new Vector3(.78f,.38f,.77f),rock);
                for(int row=0;row<3;row++)Mesh("Weathered masonry block",PrimitiveType.Cube,parent,new Vector3(x,.46f+row*.32f,z),new Vector3(.44f,.29f,.43f),row%2==0?rock:new Color(.39f,.35f,.29f));
                Mesh("Broken cap",PrimitiveType.Cube,parent,new Vector3(x,1.41f,z),new Vector3(.67f,.19f,.63f),new Color(.50f,.43f,.31f));
            }
        }
        private Transform[] BuildSoldier(Transform parent,bool evil,bool hero,bool heavy)
        {
            Color iron=evil?new Color(.30f,.27f,.28f):new Color(.47f,.52f,.54f);
            Color cloth=evil?new Color(.39f,.14f,.12f):hero?new Color(.18f,.25f,.38f):new Color(.15f,.29f,.36f);
            Color edge=evil?new Color(.65f,.33f,.17f):new Color(.70f,.54f,.28f);
            var model=Empty("Articulated medieval guard",parent,Vector3.zero);
            model.localScale=Vector3.one*(heavy?1.26f:hero?1.17f:1f);
            var rig=new Transform[4];
            for(int i=0;i<2;i++)
            {
                int side=i==0?-1:1;
                var leg=Empty("Leg pivot "+i,model,new Vector3(side*.16f,.65f,0));rig[i]=leg;
                Mesh("Thigh",PrimitiveType.Capsule,leg,new Vector3(0,-.15f,0),new Vector3(.22f,.21f,.21f),cloth);
                Mesh("Steel greave",PrimitiveType.Cube,leg,new Vector3(0,-.39f,.025f),new Vector3(.22f,.30f,.24f),iron,.35f);
                Mesh("Boot",PrimitiveType.Cube,leg,new Vector3(0,-.55f,.08f),new Vector3(.24f,.16f,.34f),new Color(.20f,.17f,.14f));
                var arm=Empty("Arm pivot "+i,model,new Vector3(side*.36f,1.19f,0));rig[2+i]=arm;
                Mesh("Shoulder plate",PrimitiveType.Sphere,arm,Vector3.zero,new Vector3(.42f,.29f,.36f),iron,.50f);
                Mesh("Sleeve",PrimitiveType.Capsule,arm,new Vector3(side*.035f,-.24f,0),new Vector3(.21f,.22f,.21f),cloth);
                Mesh("Vambrace",PrimitiveType.Cube,arm,new Vector3(side*.045f,-.44f,.03f),new Vector3(.18f,.23f,.19f),iron,.45f);
            }
            Mesh("Cuirass",PrimitiveType.Capsule,model,new Vector3(0,.96f,0),new Vector3(.60f,.39f,.39f),iron,.48f);
            Taper("Cloth skirt",model,new Vector3(0,.64f,0),new Vector3(.65f,.40f,.42f),cloth).transform.localRotation=Quaternion.Euler(180,0,0);
            var cape=Mesh("Tapered silhouette cloak",PrimitiveType.Cube,model,new Vector3(0,.91f,-.22f),new Vector3(hero?.72f:.58f,.91f,.10f),cloth);
            cape.transform.localRotation=Quaternion.Euler(-8,0,0);
            Mesh("Belt",PrimitiveType.Cube,model,new Vector3(0,.71f,.23f),new Vector3(.54f,.11f,.12f),edge,.5f);
            Mesh("Face",PrimitiveType.Sphere,model,new Vector3(0,1.52f,.035f),new Vector3(.39f,.43f,.38f),evil?new Color(.40f,.33f,.26f):new Color(.64f,.46f,.34f));
            Mesh("Helm rim",PrimitiveType.Cylinder,model,new Vector3(0,1.70f,0),new Vector3(.52f,.11f,.50f),iron,.55f);
            Taper("Conical helm",model,new Vector3(0,1.86f,0),new Vector3(.51f,.42f,.50f),iron,.7f);
            Mesh("Face guard",PrimitiveType.Cube,model,new Vector3(0,1.57f,.20f),new Vector3(.25f,.11f,.08f),iron,.45f);
            if(hero) Taper("Valorian gold crest",model,new Vector3(0,2.10f,0),new Vector3(.17f,.31f,.25f),edge,.65f);
            if(evil)foreach(int side in new[]{-1,1})
                Taper("Corrupted horn",model,new Vector3(side*.22f,1.94f,0),new Vector3(.15f,.40f,.15f),edge,.3f).transform.localRotation=Quaternion.Euler(0,0,-side*24);
            var shield=Mesh("Shield",PrimitiveType.Cylinder,rig[2],new Vector3(-.12f,-.27f,.17f),new Vector3(.56f,.065f,.56f),cloth,.30f);
            shield.transform.localRotation=Quaternion.Euler(90,0,0);
            Mesh("Shield boss",PrimitiveType.Sphere,rig[2],new Vector3(-.12f,-.27f,.26f),new Vector3(.17f,.17f,.11f),edge,.65f);
            var shaft=Mesh("Spear shaft",PrimitiveType.Cylinder,rig[3],new Vector3(.12f,.03f,.20f),new Vector3(.065f,.81f,.065f),new Color(.33f,.22f,.13f));
            shaft.transform.localRotation=Quaternion.Euler(0,0,-7);
            Taper("Spear head",rig[3],new Vector3(.22f,.99f,.20f),new Vector3(.16f,.34f,.10f),edge,.8f).transform.localRotation=Quaternion.Euler(0,0,-7);
            if(evil)model.localRotation=Quaternion.Euler(0,180,0);
            return rig;
        }
        private static void AnimateRig(Transform[] rig,float time,int i,bool fallen)
        {
            float a=fallen?0:Mathf.Sin(time*9.2f+i*.68f)*22f;
            rig[0].localRotation=Quaternion.Euler(a,0,0);rig[1].localRotation=Quaternion.Euler(-a,0,0);
            rig[2].localRotation=Quaternion.Euler(-a*.55f,0,0);rig[3].localRotation=Quaternion.Euler(a*.75f,0,0);
        }
        private void BuildGate(Transform parent)
        {
            var masonry=new Color(.35f,.31f,.26f);
            foreach(int side in new[]{-1,1})
            {
                Mesh("Stone footing",PrimitiveType.Cube,parent,new Vector3(side*.78f,.17f,0),new Vector3(.43f,.34f,.68f),masonry);
                for(int i=0;i<5;i++)Mesh("Dressed masonry pier "+i,PrimitiveType.Cube,parent,new Vector3(side*.78f,.46f+i*.39f,0),new Vector3(.29f,.37f,.48f),i%2==0?masonry:new Color(.42f,.36f,.29f));
                Mesh("Bronze collar",PrimitiveType.Cube,parent,new Vector3(side*.78f,2.27f,0),new Vector3(.45f,.18f,.58f),new Color(.55f,.43f,.25f),.65f);
                Taper("Stone finial",parent,new Vector3(side*.78f,2.5f,0),new Vector3(.37f,.31f,.37f),masonry);
            }
            Mesh("Portal lintel",PrimitiveType.Cube,parent,new Vector3(0,2.48f,0),new Vector3(1.94f,.30f,.59f),masonry);
            Mesh("Bronze rim",PrimitiveType.Cube,parent,new Vector3(0,2.67f,0),new Vector3(1.49f,.10f,.36f),new Color(.57f,.45f,.26f),.45f);
            Mesh("Rift blade",PrimitiveType.Cube,parent,new Vector3(0,.99f,.045f),new Vector3(1.21f,1.73f,.04f),new Color(.15f,.35f,.31f),.06f,false);
            var labelBack=Mesh("Gate inscription dark plaque",PrimitiveType.Cube,parent,new Vector3(0,1.55f,-.31f),new Vector3(1.68f,.73f,.055f),new Color(.10f,.13f,.12f));
            var inscription=Empty("Actual numbered gate label",parent,new Vector3(0,1.56f,-.36f));
            var text=inscription.gameObject.AddComponent<TextMesh>();
            text.text="+10";text.fontSize=84;text.characterSize=.020f;
            text.anchor=TextAnchor.MiddleCenter;text.alignment=TextAlignment.Center;
            text.color=new Color(.97f,.92f,.79f);
            // TextMesh default front is toward camera at negative Z.
        }
        public void Begin()
        {
            Initialize();
            State = Phase.Running;Stage=0;Lane=1;Soldiers=22;Power=0;Wards=0;Score=0;Elapsed=0;StageTime=0;
            movingX=0;fireClock=0;GateTaken=false;FightCompleted=false;LastEvent="Comienza la expedición";
            RefreshWorld();
        }
        public void Steer(int direction)
        {
            if(State!=Phase.Running) return;
            Lane=Mathf.Clamp(Lane+Math.Sign(direction),0,2);
        }
        public void SetLane(int selected)
        {
            if (State==Phase.Running) Lane=Mathf.Clamp(selected,0,2);
        }
        public void TogglePause()
        {
            if(State==Phase.Paused) State=Phase.Running;
            else if(State==Phase.Running) State=Phase.Paused;
        }
        private void Update()
        {
            if(!initialized) Initialize();
            if(Input.GetKeyDown(KeyCode.LeftArrow)||Input.GetKeyDown(KeyCode.A))Steer(-1);
            if(Input.GetKeyDown(KeyCode.RightArrow)||Input.GetKeyDown(KeyCode.D))Steer(1);
            if(Input.GetKeyDown(KeyCode.Space)) { if(State==Phase.Intro||State==Phase.Victory||State==Phase.Defeat)Begin(); else TogglePause(); }
            if(Input.touchCount>0)
            {
                Touch touch=Input.GetTouch(0);
                if(touch.phase==TouchPhase.Began) swipeStart=touch.position;
                if(touch.phase==TouchPhase.Ended)
                {
                    float distance=touch.position.x-swipeStart.x;
                    if(Mathf.Abs(distance)>40f) Steer(distance>0?1:-1);
                }
            }
            if (State==Phase.Running) Step(Time.deltaTime);
            else RefreshWorld();
        }
        /// <summary>Deterministic, injectable clock for EditMode-native evidence.</summary>
        public void Step(float delta)
        {
            if(State!=Phase.Running || delta<0 || delta>1f) return;
            Elapsed+=delta;StageTime+=delta;
            movingX=Mathf.Lerp(movingX,(Lane-1)*2.15f,Mathf.Clamp01(delta*9f));
            if (StageTime>=GateCrossing && !GateTaken) ApplyGate();
            if (StageTime>=FightResolve && !FightCompleted) ResolveFight();
            if (StageTime>=WaveLength)
            {
                if(!FightCompleted)ResolveFight();
                if(Soldiers<=0)State=Phase.Defeat;
                else if(Stage==Waves.Length-1)State=Phase.Victory;
                else {Stage++;StageTime-=WaveLength;GateTaken=false;FightCompleted=false;LastEvent=Waves[Stage].Name;}
            }
            RefreshWorld();
        }
        private void ApplyGate()
        {
            GateTaken=true;
            Gate g = Waves[Stage].Gates[Lane];
            switch(g.Type)
            {
                case GateType.Recruit: Soldiers=Mathf.Min(100,Soldiers+g.Value);LastEvent="+"+g.Value+" refuerzos";break;
                case GateType.Power: Power=Mathf.Min(150,Power+g.Value);LastEvent="Ataque +"+g.Value+"%";break;
                case GateType.Ward: Wards=Mathf.Min(90,Wards+g.Value);LastEvent="+"+g.Value+" protección";break;
                case GateType.Trap: Soldiers=Mathf.Max(1,Soldiers-g.Value);LastEvent="Trampa: -"+g.Value;break;
            }
            uiFlash=1;
        }
        private void ResolveFight()
        {
            FightCompleted=true;
            int enemy=Waves[Stage].Enemy;
            float attack=Soldiers*(1f+Power/100f);
            float effort=enemy*(1f+.15f*Stage);
            int total=Mathf.Max(1,Mathf.CeilToInt(effort*.36f/(1+attack/48f)));
            if(attack<effort*.36f) total+=Mathf.CeilToInt((effort*.36f-attack)/3f);
            int absorbed=Mathf.Min(Wards,total);Wards-=absorbed;
            int losses=Mathf.Min(Soldiers,total-absorbed);
            Soldiers-=losses;
            Score+=Mathf.Max(0,Mathf.RoundToInt(enemy*10+Soldiers*4));
            LastEvent="Guardia: -"+losses+" · enemigo derrotado";
            uiFlash=1;
        }
        private void RefreshWorld()
        {
            if(!initialized)return;
            float scroll=Elapsed*2.8f;
            for(int i=0;i<laneMarkers.Length;i++){var mark=laneMarkers[i];var pos=mark.localPosition;pos.z=(((i/2)*2.4f-scroll)%105.6f+105.6f)%105.6f-9;mark.localPosition=pos;}
            for(int i=0;i<environment.Length;i++)
            {
                var t=environment[i];
                Vector3 p=t.localPosition;
                p.z=((i*7.19f-scroll)%85f+85f)%85f-8f;
                t.localPosition=p;
            }
            for(int i=0;i<soldierActors.Length;i++)
            {
                bool visible=i<VisibleSoldiers;
                soldierActors[i].SetActive(visible);
                if(!visible)continue;
                int column=(i+3)%4,row=(i-1)/4;
                float laneX=i==0?0:(column-1.5f)*.48f;
                float z=i==0?3.56f:2.65f-row*.68f;
                soldierBodies[i].localPosition=new Vector3(movingX+laneX,0,z);
                AnimateRig(soldierLimbs[i],Elapsed,i,false);
            }
            bool running=State==Phase.Running || State==Phase.Intro || State==Phase.Paused;
            for(int i=0;i<portals.Length;i++)
            {
                float gateZ=31f-Mathf.Clamp01(StageTime/GateCrossing)*27.3f;
                portals[i].SetActive(running&&!GateTaken);
                portals[i].transform.localPosition=new Vector3((i-1)*2.15f,0,gateZ);
                var gate=Waves[Mathf.Clamp(Stage,0,Waves.Length-1)].Gates[i];
                Color c=gate.Type==GateType.Trap?new Color(.69f,.22f,.31f):gate.Type==GateType.Power?
                    new Color(.72f,.53f,.24f):gate.Type==GateType.Ward?new Color(.46f,.25f,.72f):new Color(.14f,.49f,.48f);
                var blade=portals[i].transform.Find("Rift blade")?.GetComponent<MeshRenderer>();
                if(blade)blade.sharedMaterial=Mat(c,.12f,.38f,false);
                var label=portals[i].GetComponentInChildren<TextMesh>();
                if(label){label.text=gate.Label;label.color=new Color(.97f,.92f,.79f);}
            }
            bool showEnemies=running && StageTime>=FightStart && StageTime<WaveLength;
            float enemyTravel=Mathf.Clamp01((StageTime-FightStart)/(FightResolve-FightStart));
            for(int i=0;i<enemies.Length;i++)
            {
                bool dying=FightCompleted;
                bool visible=showEnemies && (!dying || (i%3==0));
                enemies[i].SetActive(visible);
                if(visible)
                {
                    float x=((i%4)-1.5f)*.93f;
                    float z=19f+(i/4)*1.15f-8f*enemyTravel;
                    enemies[i].transform.localPosition=new Vector3(x,dying?-.5f:0,z);
                    enemies[i].transform.localEulerAngles=dying?new Vector3(0,0,65):Vector3.zero;
                    AnimateRig(enemyLimbs[i],Elapsed,i,dying);
                }
            }
            fireClock+=State==Phase.Running?Time.deltaTime:0;
            for(int i=0;i<shots.Length;i++)
            {
                bool active=showEnemies && !FightCompleted;
                shots[i].SetActive(active);
                if(active)
                {
                    float k=((Elapsed*6.8f+i*.8f)%9f);
                    float x=movingX+((i%3)-1)*.47f;
                    shots[i].transform.localPosition=new Vector3(x,1.0f,3.3f+k*2.3f);
                }
            }
            uiFlash=Mathf.Max(0,uiFlash-Time.deltaTime*1.2f);
        }
        private static GUIStyle LabelStyle(int fontSize,Color color,TextAnchor align=TextAnchor.UpperLeft)
        {
            var style = new GUIStyle(GUI.skin.label) { fontSize=fontSize,fontStyle=FontStyle.Bold,alignment=align,richText=true,wordWrap=true };
            style.normal.textColor=color;
            return style;
        }
        private void OnGUI()
        {
            if (!ShowOverlay || !initialized) return;
            var saved = GUI.matrix;
            var original = GUI.color;
            bool landscape = Screen.width > Screen.height;
            float referenceW = landscape ? 844f : 390f;
            float referenceH = landscape ? 390f : 844f;
            float scale = Mathf.Min(Screen.width / referenceW, Screen.height / referenceH);
            if (scale <= 0f) scale = 1f;
            float dx = (Screen.width-referenceW*scale)*.5f;
            float dy = (Screen.height-referenceH*scale)*.5f;
            GUI.matrix = Matrix4x4.TRS(new Vector3(dx,dy,0),Quaternion.identity,Vector3.one*scale);
            Color accent = new Color(.95f,.83f,.64f);
            GUI.color = new Color(.075f,.095f,.14f,.88f);
            GUI.Box(landscape ? new Rect(10,7,475,57) : new Rect(8,10,374,116), GUIContent.none);
            GUI.color = Color.white;
            if(landscape)
            {
                GUI.Label(new Rect(19,10,465,24),"ELDORIA  ·  SENDA DE LA BRECHA",LabelStyle(16,accent));
                GUI.Label(new Rect(19,34,465,24),"TRAMO "+(Stage+1)+"/6 · "+Waves[Stage].Name+"     GUARDIA "+Soldiers+" · ATQ "+(100+Power)+"% · ESCUDOS "+Wards,LabelStyle(13,Color.white));
                if(State==Phase.Intro || State==Phase.Victory || State==Phase.Defeat)
                {
                    GUI.color = new Color(.09f,.11f,.18f,.96f);
                    GUI.Box(new Rect(234,130,376,158),GUIContent.none);
                    GUI.color = Color.white;
                    string status = State==Phase.Intro ? "CRUZA LA BRECHA" : State==Phase.Victory ? "¡BRECHA SELLADA!" : "EXPEDICIÓN DERROTADA";
                    GUI.Label(new Rect(253,145,340,50),status,LabelStyle(21,accent,TextAnchor.MiddleCenter));
                    if (GUI.Button(new Rect(300,208,244,57),State==Phase.Intro?"COMENZAR EXPEDICIÓN":"VOLVER A INTENTARLO")) Begin();
                }
                else
                {
                    GUI.Label(new Rect(240,331,370,31),LastEvent,LabelStyle(15,Color.white,TextAnchor.MiddleCenter));
                    if(GUI.Button(new Rect(20,311,88,60),"◀"))Steer(-1);
                    if(GUI.Button(new Rect(736,311,88,60),"▶"))Steer(1);
                    if(GUI.Button(new Rect(785,15,40,40),State==Phase.Paused?"▶":"Ⅱ")) TogglePause();
                }
            }
            else
            {
                GUI.Label(new Rect(18,16,340,22),"ELDORIA  ·  SENDA DE LA BRECHA",LabelStyle(16,accent));
                GUI.Label(new Rect(18,42,340,22),"TRAMO "+(Stage+1)+"/6  ·  "+Waves[Stage].Name.ToUpperInvariant(),LabelStyle(13,Color.white));
                GUI.Label(new Rect(18,74,340,36),"GUARDIA "+Soldiers+"     ATAQUE "+(100+Power)+"%     ESCUDOS "+Wards,LabelStyle(14,Color.white));
                if(State==Phase.Intro || State==Phase.Victory || State==Phase.Defeat)
                {
                    GUI.color = new Color(.09f,.11f,.18f,.96f);
                    GUI.Box(new Rect(31,544,328,174),GUIContent.none);
                    GUI.color = Color.white;
                    string status = State==Phase.Intro ? "CRUZA LA BRECHA" : State==Phase.Victory ? "¡BRECHA SELLADA!" : "EXPEDICIÓN DERROTADA";
                    GUI.Label(new Rect(44,561,300,49),status,LabelStyle(22,accent,TextAnchor.MiddleCenter));
                    if (GUI.Button(new Rect(63,641,264,56),State==Phase.Intro?"COMENZAR EXPEDICIÓN":"VOLVER A INTENTARLO")) Begin();
                }
                else
                {
                    GUI.Label(new Rect(25,710,340,35),LastEvent,LabelStyle(15,Color.white,TextAnchor.MiddleCenter));
                    if(GUI.Button(new Rect(23,762,93,62),"◀"))Steer(-1);
                    if(GUI.Button(new Rect(274,762,93,62),"▶"))Steer(1);
                    if(GUI.Button(new Rect(342,25,30,30),State==Phase.Paused?"▶":"Ⅱ")) TogglePause();
                }
            }
            GUI.color = original;
            GUI.matrix = saved;
        }
        private void OnDestroy()
        {
            foreach(var material in materials.Values)if(material)
            { if(UnityEngine.Application.isPlaying) Destroy(material); else DestroyImmediate(material); }
            materials.Clear();
            if(taperedMesh){if(UnityEngine.Application.isPlaying)Destroy(taperedMesh);else DestroyImmediate(taperedMesh);}
            if(roadTexture){if(UnityEngine.Application.isPlaying)Destroy(roadTexture);else DestroyImmediate(roadTexture);}
            if(earthTexture){if(UnityEngine.Application.isPlaying)Destroy(earthTexture);else DestroyImmediate(earthTexture);}
        }
    }
}
