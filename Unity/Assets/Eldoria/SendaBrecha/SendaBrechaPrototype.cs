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
        public int VisibleSoldiers => Mathf.Min(14, Mathf.Max(0, Soldiers));
        public bool ShowOverlay = true;

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
        private readonly Color night = new Color(.095f, .115f, .17f);

        public static bool IsolatedFromMainGame => true;

        private Material Mat(Color c, float metallic = 0, float smoothness = .24f, bool glow = false)
        {
            int key = Color32Key(c) ^ (glow ? 0x4000000 : 0);
            if (materials.TryGetValue(key, out var existing)) return existing;
            Shader shader = Shader.Find("Universal Render Pipeline/Lit");
            if (!shader) shader = Shader.Find("Standard");
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
            if (collider) { if (Application.isPlaying) Destroy(collider); else DestroyImmediate(collider); }
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
            for (int i = 0; i < soldierActors.Length; ++i)
            {
                var body = Empty(i == 0 ? "Guardian - original temporary 3D avatar" : "Valorian soldier " + i, world, Vector3.zero);
                BuildSoldier(body, false, i == 0, false);
                soldierActors[i] = body.gameObject;
                soldierBodies[i] = body;
            }
            enemies = new GameObject[12];
            for (int i = 0; i < enemies.Length; ++i)
            {
                var body = Empty("Corrupted enemy visual proxy " + i, world, Vector3.zero);
                BuildSoldier(body, true, false, i == 0);
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
            SceneCamera.fieldOfView = 48;
            SceneCamera.nearClipPlane = .18f;
            SceneCamera.farClipPlane = 95;
            SceneCamera.clearFlags = CameraClearFlags.SolidColor;
            SceneCamera.backgroundColor = night;
            SceneCamera.transform.localPosition = new Vector3(0, 12.8f, -20f);
            SceneCamera.transform.LookAt(new Vector3(0,.5f,13.2f));
            var sun = new GameObject("Slanted cold-sun with warm highlights");
            sun.transform.SetParent(world,false);
            var light = sun.AddComponent<Light>();
            light.type=LightType.Directional; light.color=new Color(.91f,.82f,.75f);
            light.intensity=1.35f;
            sun.transform.rotation=Quaternion.Euler(51,-37,0);
            var rimObject = new GameObject("Rift fill");rimObject.transform.SetParent(world,false);
            var rim=rimObject.AddComponent<Light>();rim.type=LightType.Point;
            rim.color=new Color(.56f,.31f,.88f);rim.range=29;rim.intensity=15;
            rimObject.transform.position=new Vector3(0,7,27);
            RenderSettings.ambientLight=new Color(.53f,.55f,.64f);
            RenderSettings.fog=true;
            RenderSettings.fogColor=new Color(.18f,.20f,.28f);
            RenderSettings.fogMode=FogMode.Linear;
            RenderSettings.fogStartDistance=29f;
            RenderSettings.fogEndDistance=70f;
        }
        private void BuildEnvironment(Transform parent)
        {
            Color rock = new Color(.28f,.31f,.36f);
            Color grass = new Color(.20f,.28f,.31f);
            Color road = new Color(.28f,.27f,.32f);
            Mesh("Cliff and moss floor",PrimitiveType.Cube,parent,new Vector3(0,-.48f,22),new Vector3(80,.9f,125),grass);
            Mesh("Dark slate roadway",PrimitiveType.Cube,parent,new Vector3(0,-.11f,23),new Vector3(7.7f,.24f,124),road);
            Mesh("Worn left stone verge",PrimitiveType.Cube,parent,new Vector3(-4.0f,-.04f,23),new Vector3(.47f,.3f,124),new Color(.44f,.39f,.40f));
            Mesh("Worn right stone verge",PrimitiveType.Cube,parent,new Vector3(4.0f,-.04f,23),new Vector3(.47f,.3f,124),new Color(.44f,.39f,.40f));
            var features = new List<Transform>();
            // Each stone, arch and conifer is genuine Unity geometry, not a painted card.
            for (int i = 0; i < 66; i++)
            {
                float z = -5f+(i*1.87f);
                float x=((i*17)%23/23f-.5f)*6.5f;
                var stone=Mesh("Low-contrast stone path "+i, PrimitiveType.Cube,parent,
                    new Vector3(x,.031f,z),new Vector3(.55f+(i%4)*.19f,.038f,.28f+(i%3)*.12f),
                    i%3==0?new Color(.38f,.34f,.39f):new Color(.29f,.28f,.33f));
                stone.transform.localRotation=Quaternion.Euler(0,(i%7-3)*9,0);
            }
            for (int i = 0; i < 35; i++)
            {
                float z = (i*7.19f)%85f-8f;
                int side=i%2==0?-1:1;
                float x=side*(5.8f+(i%7)*.79f);
                var scenic=Empty("Moving lowpoly forest / rock " + i,parent,new Vector3(x,0,z));
                if (i%6==0)
                {
                    var boulder=Mesh("Basalt",PrimitiveType.Sphere,scenic,new Vector3(0,.36f,0),
                        new Vector3(1.2f,.9f,.85f),rock);
                    boulder.transform.localRotation=Quaternion.Euler(6,i*23,13);
                    Mesh("Corruption violet crystal",PrimitiveType.Cylinder,scenic,new Vector3(.25f,.83f,.07f),
                        new Vector3(.16f,1.1f,.16f),new Color(.43f,.23f,.65f),.1f,true).transform.localRotation=Quaternion.Euler(16,0,24);
                }
                else
                {
                    Mesh("Rough trunk",PrimitiveType.Cylinder,scenic,new Vector3(0,.78f,0),
                        new Vector3(.28f,1.56f,.28f),new Color(.28f,.22f,.21f));
                    for(int branch=0;branch<3;branch++)
                    {
                        Mesh("Conifer tiers",PrimitiveType.Cylinder,scenic,new Vector3(0,2.12f+branch*.65f,0),
                            new Vector3(1.17f-branch*.22f,.85f,1.17f-branch*.22f),
                            branch==1?new Color(.22f,.33f,.35f):new Color(.16f,.26f,.31f));
                    }
                }
                features.Add(scenic);
            }
            environment=features.ToArray();
            for(int n=0;n<9;n++)
            {
                float z=2+n*11.9f;
                foreach(int side in new[]{-1,1})
                {
                    float x=side*4.5f;
                    Mesh("Ruined road post",PrimitiveType.Cube,parent,new Vector3(x,.72f,z),new Vector3(.4f,1.45f,.42f),rock);
                    Mesh("Post cap",PrimitiveType.Cube,parent,new Vector3(x,1.55f,z),new Vector3(.61f,.18f,.61f),new Color(.38f,.36f,.41f));
                }
            }
        }
        private void BuildSoldier(Transform parent,bool evil,bool hero,bool heavy)
        {
            Color iron=evil?new Color(.30f,.26f,.31f):new Color(.52f,.54f,.55f);
            Color cloth=evil?new Color(.39f,.17f,.28f):hero?new Color(.30f,.25f,.49f):new Color(.23f,.33f,.46f);
            Color edge=evil?new Color(.61f,.30f,.44f):new Color(.70f,.53f,.30f);
            float scale=heavy?1.32f:hero?1.14f:1f;
            var model=Empty("Articulated medieval soldier / prototype",parent,Vector3.zero);
            model.localScale=Vector3.one*scale;
            Mesh("Boot left",PrimitiveType.Cube,model,new Vector3(-.14f,.19f,0),new Vector3(.22f,.42f,.35f),iron);
            Mesh("Boot right",PrimitiveType.Cube,model,new Vector3(.14f,.19f,0),new Vector3(.22f,.42f,.35f),iron);
            Mesh("Cloth cape",PrimitiveType.Cube,model,new Vector3(0,.91f,-.19f),new Vector3(.62f,.88f,.13f),cloth);
            Mesh("Cuirass",PrimitiveType.Capsule,model,new Vector3(0,.95f,0),new Vector3(.57f,.67f,.39f),iron,.55f);
            Mesh("Belt",PrimitiveType.Cube,model,new Vector3(0,.65f,.20f),new Vector3(.53f,.12f,.12f),edge,.5f);
            Mesh("Head",PrimitiveType.Sphere,model,new Vector3(0,1.6f,.035f),new Vector3(.40f,.44f,.39f),new Color(.69f,.52f,.40f));
            Mesh("Medieval helm",PrimitiveType.Cylinder,model,new Vector3(0,1.81f,.0f),new Vector3(.51f,.34f,.50f),iron,.6f);
            Mesh("Helmet crest",PrimitiveType.Cube,model,new Vector3(0,2.04f,.0f),new Vector3(.13f,.22f,.42f),edge);
            Mesh("Left shoulder",PrimitiveType.Sphere,model,new Vector3(-.40f,1.26f,0),new Vector3(.35f,.31f,.32f),iron,.5f);
            Mesh("Right shoulder",PrimitiveType.Sphere,model,new Vector3(.40f,1.26f,0),new Vector3(.35f,.31f,.32f),iron,.5f);
            Mesh("Round shield",PrimitiveType.Cylinder,model,new Vector3(-.47f,.95f,.25f),new Vector3(.47f,.13f,.47f),cloth,.4f)
                .transform.localRotation=Quaternion.Euler(70,0,0);
            Mesh("Shield boss",PrimitiveType.Sphere,model,new Vector3(-.47f,.96f,.34f),new Vector3(.15f,.15f,.15f),edge,.8f);
            var shaft=Mesh("Spear or staff",PrimitiveType.Cylinder,model,new Vector3(.5f,1.3f,.21f),new Vector3(.065f,1.85f,.065f),new Color(.38f,.25f,.18f));
            shaft.transform.localRotation=Quaternion.Euler(13,0,-8);
            Mesh("Spear metal",PrimitiveType.Cylinder,model,new Vector3(.64f,2.33f,.46f),new Vector3(.085f,.33f,.085f),edge,.85f);
            if(evil) Mesh("Corrupted eyes",PrimitiveType.Sphere,model,new Vector3(0,1.58f,.25f),new Vector3(.24f,.08f,.08f),new Color(.69f,.16f,.43f),0,true);
        }
        private void BuildGate(Transform parent)
        {
            var masonry=new Color(.36f,.35f,.41f);
            foreach (int side in new[]{-1,1})
            {
                Mesh("Rough stone portal pier",PrimitiveType.Cube,parent,new Vector3(side*.81f,1.36f,0),new Vector3(.37f,2.72f,.51f),masonry);
                Mesh("Bronze collar",PrimitiveType.Cube,parent,new Vector3(side*.81f,2.38f,0),new Vector3(.47f,.20f,.62f),new Color(.53f,.41f,.28f),.7f);
            }
            Mesh("Portal lintel",PrimitiveType.Cube,parent,new Vector3(0,2.65f,0),new Vector3(2.15f,.34f,.61f),masonry);
            Mesh("Rift blade",PrimitiveType.Cube,parent,new Vector3(0,1.32f,.035f),new Vector3(1.40f,1.91f,.06f),new Color(.22f,.44f,.47f),.16f,true);
            var inscription=Empty("Actual numbered gate label",parent,new Vector3(0,1.48f,-.10f));
            var text=inscription.gameObject.AddComponent<TextMesh>();
            text.text="+10";text.fontSize=84;text.characterSize=.024f;
            text.anchor=TextAnchor.MiddleCenter;text.alignment=TextAlignment.Center;
            text.color=Color.white;
            inscription.localRotation=Quaternion.Euler(0,180,0);
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
            float scroll=(State==Phase.Running?Elapsed*2.8f:0);
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
                int column=i%4,row=i/4;
                float laneX=(i==0?0:(column-1.5f)*.60f);
                float z=2.7f-row*.61f+(i==0?.7f:0);
                soldierBodies[i].localPosition=new Vector3(movingX+laneX,
                    State==Phase.Running?.035f*Mathf.Sin(Elapsed*10+i*1.3f):0,z);
            }
            bool running=State==Phase.Running || State==Phase.Intro;
            for(int i=0;i<portals.Length;i++)
            {
                float gateZ=31f-Mathf.Clamp01(StageTime/GateCrossing)*27.3f;
                portals[i].SetActive(running&&!GateTaken);
                portals[i].transform.localPosition=new Vector3((i-1)*2.15f,0,gateZ);
                var gate=Waves[Mathf.Clamp(Stage,0,Waves.Length-1)].Gates[i];
                Color c=gate.Type==GateType.Trap?new Color(.69f,.22f,.31f):gate.Type==GateType.Power?
                    new Color(.72f,.53f,.24f):gate.Type==GateType.Ward?new Color(.46f,.25f,.72f):new Color(.14f,.49f,.48f);
                var blade=portals[i].transform.Find("Rift blade")?.GetComponent<MeshRenderer>();
                if(blade)blade.sharedMaterial=Mat(c,.12f,.55f,true);
                var label=portals[i].GetComponentInChildren<TextMesh>();
                if(label){label.text=gate.Label;label.color=Color.white;}
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
            var saved=GUI.matrix;
            float sx=Screen.width/390f,sy=Screen.height/844f;
            float scale=Mathf.Min(sx,sy);if(scale<=0)scale=1;
            float dx=(Screen.width-390*scale)*.5f;
            float dy=(Screen.height-844*scale)*.5f;
            GUI.matrix=Matrix4x4.TRS(new Vector3(dx,dy,0),Quaternion.identity,Vector3.one*scale);
            Color original=GUI.color;
            GUI.color=new Color(.075f,.095f,.14f,.88f);
            GUI.Box(new Rect(8,10,374,116),GUIContent.none);
            GUI.color=Color.white;
            GUI.Label(new Rect(18,16,340,22),"ELDORIA  ·  SENDA DE LA BRECHA",LabelStyle(16,new Color(.95f,.83f,.64f)));
            GUI.Label(new Rect(18,42,340,22),"TRAMO "+(Stage+1)+"/6  ·  "+Waves[Stage].Name.ToUpperInvariant(),LabelStyle(13,Color.white));
            GUI.Label(new Rect(18,74,340,36),"GUARDIA "+Soldiers+"     ATAQUE "+(100+Power)+"%     ESCUDOS "+Wards,LabelStyle(14,Color.white));
            if(State==Phase.Intro || State==Phase.Victory || State==Phase.Defeat)
            {
                GUI.color=new Color(.09f,.11f,.18f,.96f);
                GUI.Box(new Rect(31,544,328,174),GUIContent.none);
                GUI.color=Color.white;
                string status=State==Phase.Intro?"CRUZA LA BRECHA":State==Phase.Victory?"¡BRECHA SELLADA!":"EXPEDICIÓN DERROTADA";
                GUI.Label(new Rect(44,561,300,49),status,LabelStyle(22,new Color(.95f,.81f,.56f),TextAnchor.MiddleCenter));
                if (GUI.Button(new Rect(63,641,264,56),State==Phase.Intro?"COMENZAR EXPEDICIÓN":"VOLVER A INTENTARLO")) Begin();
            }
            else
            {
                GUI.Label(new Rect(25,710,340,35),LastEvent,LabelStyle(15,Color.white,TextAnchor.MiddleCenter));
                if(GUI.Button(new Rect(23,762,93,62),"◀"))Steer(-1);
                if(GUI.Button(new Rect(274,762,93,62),"▶"))Steer(1);
                if(GUI.Button(new Rect(342,25,30,30),State==Phase.Paused?"▶":"Ⅱ")) TogglePause();
            }
            GUI.color=original;GUI.matrix=saved;
        }
        private void OnDestroy()
        {
            foreach(var material in materials.Values)if(material)
            { if(UnityEngine.Application.isPlaying) Destroy(material); else DestroyImmediate(material); }
            materials.Clear();
        }
    }
}
