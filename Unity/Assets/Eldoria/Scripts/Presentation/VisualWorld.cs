using Eldoria.Domain;
using UnityEngine;
using UnityEngine.Rendering;

namespace Eldoria.Presentation
{
    // Original procedural study: provisional geometry/materials, no inherited web art or unlicensed assets.
    public static class VisualWorld
    {
        static readonly Color Stone = new Color(.34f,.36f,.37f), Deep = new Color(.10f,.12f,.13f);
        static readonly Color WarmStone = new Color(.46f,.43f,.37f), Timber = new Color(.24f,.16f,.11f);
        static readonly Color Earth = new Color(.22f,.19f,.15f), Pine = new Color(.10f,.18f,.14f);
        static readonly Color Amber = new Color(.96f,.53f,.22f), Violet = new Color(.57f,.19f,.91f);
        public static void Create(bool city, PlayerState state)
        {
            RenderSettings.ambientLight = new Color(.46f,.48f,.54f);
            RenderSettings.fog = true; RenderSettings.fogMode = FogMode.Linear;
            RenderSettings.fogColor = new Color(.18f,.20f,.23f); RenderSettings.fogStartDistance=28; RenderSettings.fogEndDistance=86;
            var cameraGo = new GameObject("Isometric camera");
            var camera = cameraGo.AddComponent<Camera>(); camera.orthographic=true;
            camera.orthographicSize = city ? 10.8f : 14;
            camera.backgroundColor = new Color(.18f,.20f,.24f); camera.clearFlags=CameraClearFlags.SolidColor;
            cameraGo.tag="MainCamera";
            cameraGo.transform.position = city ? new Vector3(18,19,-19) : new Vector3(20,24,-21);
            cameraGo.transform.LookAt(city ? new Vector3(0,1.8f,2.8f) : new Vector3(0,0,1));
            var sun = new GameObject("Cold dawn").AddComponent<Light>();
            sun.type=LightType.Directional; sun.color=new Color(.82f,.80f,.72f); sun.intensity=1.18f;
            sun.transform.rotation=Quaternion.Euler(42,-35,0); sun.shadows=LightShadows.Soft;
            Box("World ground",new Vector3(0,-.7f,0),new Vector3(34,1.2f,30),Earth);
            // Layered rock masses create a believable playable plateau instead of a flat board.
            for(int i=0;i<11;i++)
            {
                float x=-15+i*3.05f;
                float h=1.4f+(i%4)*.55f;
                Box("Cliff rim",new Vector3(x,-.05f,12.2f-(i%2)*.55f),new Vector3(3.3f,h,3.1f),Stone*.68f);
            }
            for(int i=0;i<8;i++)
            {
                var rock=Box("Old imperial road",new Vector3(-1.2f+(i%2)*.28f,-.02f,-12+i*3.15f),new Vector3(2.7f,.16f,1.7f),WarmStone*.78f);
                rock.transform.rotation=Quaternion.Euler(0,(i%2==0?7:-8),0);
            }
            if(city) City(state); else Frontier(state);
            Rift(city?new Vector3(11,1,10):new Vector3(9,1,8));
        }
        static void City(PlayerState state)
        {
            // Visual Bible art pass 01: still procedural, but now built from reusable game-like modules.
            // Goal: prove silhouette, hierarchy, material language and mobile readability before external asset production.

            // Terrain terraces / cliff shelves.
            Box("Upper rocky shelf",new Vector3(0,.18f,4.2f),new Vector3(16,.65f,10.5f),Stone*.64f);
            Box("Lower inhabited shelf",new Vector3(0,.02f,-3.6f),new Vector3(19,.45f,6.6f),Earth*.92f);
            for(int i=0;i<7;i++)
            {
                float x=-9+i*3f;
                Box("Terrace cliff",new Vector3(x,-.65f,-6.4f),new Vector3(3.2f,2.2f,2.1f),Stone*.58f);
            }

            // Signature cyclopean arch: radial stone voussoirs rather than a simple lintel.
            Arch("Cyclopean arch",new Vector3(-6.8f,.25f,4.8f),4.5f,2.0f,11,Stone*.62f);
            var fallen=Box("Cyclopean arch · fallen segment",new Vector3(6.5f,.55f,1.3f),new Vector3(5.4f,1.35f,2.0f),Stone*.55f);
            fallen.transform.rotation=Quaternion.Euler(0,20,-17);

            // Bastion mass: fortress first, dead-palace bones second.
            Box("Bastion · lower keep",new Vector3(0,1.15f,5.0f),new Vector3(8.6f,3.0f,6.5f),WarmStone*.80f);
            Box("Bastion · upper keep",new Vector3(0,3.6f,5.15f),new Vector3(5.8f,2.2f,4.5f),Stone*.86f);
            Box("Bastion · broken crown",new Vector3(-.45f,5.35f,5.25f),new Vector3(3.8f,1.5f,3.1f),Stone*.74f);
            for(int i=0;i<4;i++) Tower(new Vector3(i%2==0?-5.25f:5.25f,0,i<2?2.0f:7.6f),5.7f);
            for(int i=0;i<10;i++)
                Box("Bastion battlement",new Vector3(-4.05f+i*.9f,5.15f,1.6f),new Vector3(.48f,.72f,.7f),Stone*.92f);

            // Palace-remnant ribs / repaired scaffold around the crown.
            for(int i=0;i<5;i++)
            {
                float x=-2.6f+i*1.3f;
                Box("Ancient rib",new Vector3(x,6.35f,5.6f),new Vector3(.32f,2.6f,.55f),Stone*.70f);
            }
            Scaffold(new Vector3(3.7f,3.0f,5.0f),new Vector3(2.4f,4.6f,2.5f));

            // Main gate and readable central approach.
            Box("Gate pier L",new Vector3(-2.25f,1.25f,-4.75f),new Vector3(1.2f,3.4f,1.1f),WarmStone*.78f);
            Box("Gate pier R",new Vector3(2.25f,1.25f,-4.75f),new Vector3(1.2f,3.4f,1.1f),WarmStone*.78f);
            Arch("Gate arch",new Vector3(0,.75f,-4.8f),2.35f,.62f,7,Stone*.86f);
            var gate=Box("Puerta · ir al mundo",new Vector3(0,.7f,-4.86f),new Vector3(2.5f,2.45f,.36f),Timber);
            gate.AddComponent<WorldHotspot>().Id="gate";
            for(int i=0;i<6;i++)
                Box("Main approach",new Vector3(0,.18f,-3.2f+i*1.18f),new Vector3(2.7f,.13f,.82f),WarmStone*.68f);

            // Left work district — Sawmill.
            Box("Sawmill yard",new Vector3(-6.6f,.16f,-1.75f),new Vector3(5.1f,.35f,4.4f),Earth*.82f);
            House("Sawmill house",new Vector3(-6.55f,.4f,-1.45f),new Vector3(3.4f,1.65f,2.7f),state.SawmillLevel>0);
            var mill=Box("Aserradero · interacción",new Vector3(-6.55f,1.1f,-1.45f),new Vector3(3.1f,1.55f,2.45f),
                state.SawmillLevel>0?new Color(.43f,.32f,.22f):new Color(.20f,.20f,.19f));
            mill.AddComponent<WorldHotspot>().Id="sawmill";
            if(state.SawmillLevel>0)
            {
                Glow("Sawmill fire",new Vector3(-5.45f,1.7f,-1.95f),Amber,2.0f,4.5f);
                for(int i=0;i<5;i++)
                    Cylinder("Timber stack",new Vector3(-8.8f+i*.42f,.38f,-2.85f),new Vector3(.30f,2.2f,.30f),new Color(.30f,.20f,.13f),Quaternion.Euler(90,0,0));
            }
            Scaffold(new Vector3(-8.0f,1.55f,.15f),new Vector3(1.6f,2.7f,1.3f));

            // Right side — modest early barracks and granary, clearly secondary to Bastion.
            House("Early barracks",new Vector3(6.25f,.42f,-1.65f),new Vector3(3.6f,1.75f,2.8f),true);
            House("Granary",new Vector3(4.75f,.40f,-4.15f),new Vector3(2.8f,1.55f,2.15f),true);
            Box("Training yard",new Vector3(7.0f,.17f,-4.1f),new Vector3(3.6f,.22f,2.6f),Earth*.72f);

            // Small rebuilt homes and shelters, intentionally sparse at Bastion I.
            House("Rebuilder home A",new Vector3(-3.6f,.36f,-3.5f),new Vector3(2.1f,1.25f,1.8f),true);
            House("Rebuilder home B",new Vector3(2.8f,.36f,-2.85f),new Vector3(2.0f,1.2f,1.7f),true);

            // Vegetation breaks the hard geometry without turning the city lush.
            foreach(var p in new[] {
                new Vector3(-9.2f,0,-.8f),new Vector3(-8.7f,0,5.0f),new Vector3(-4.4f,0,8.2f),
                new Vector3(6.6f,0,8.1f),new Vector3(8.8f,0,3.9f),new Vector3(8.9f,0,-.8f)})
                PineTree(p,1.0f);

            // Life markers.
            Hero(new Vector3(-1.8f,0,-2.0f),1.0f);
            for(int i=0;i<5;i++) Archer(new Vector3(2.0f+i*.68f,0,-2.9f+(i%2)*.7f));
            Glow("Gate torch L",new Vector3(-2.9f,2.05f,-4.45f),Amber,1.45f,3.2f);
            Glow("Gate torch R",new Vector3(2.9f,2.05f,-4.45f),Amber,1.45f,3.2f);
            Glow("Bastion warmth",new Vector3(0,3.6f,3.25f),Amber,1.75f,7.5f);
        }
        static void Frontier(PlayerState state)
        {
            for(int i=0;i<15;i++)
            {
                float x=-15+(i*37%29),z=-11+(i*13%25);
                if(x>-3 && x<3) continue;
                Tree(new Vector3(x,0,z),i%3==0);
            }
            Box("Forest reserve",new Vector3(-6,.1f,1),new Vector3(4.6f,.32f,4),new Color(.15f,.22f,.19f));
            var grove=Cylinder("Bosque de Valoria · recolectar",new Vector3(-6,1.4f,1),new Vector3(2,2.8f,2),new Color(.20f,.28f,.23f),Quaternion.identity);
            grove.AddComponent<WorldHotspot>().Id="forest-valoria";
            for(int i=0;i<5;i++) Tree(new Vector3(-8+(i%3)*1.4f,0,-.1f+(i/3)*2),true);
            var enemy=Sphere("Explorador corrupto",new Vector3(5,1,3),new Vector3(1.35f,2.1f,1.35f),
                state.ScoutDefeated?Stone*.5f:Violet*.52f);
            enemy.AddComponent<WorldHotspot>().Id="corrupt-scout";
            Box("Broken watchpost",new Vector3(5,.8f,4.6f),new Vector3(2.5f,1.7f,1.3f),Stone*.65f);
            Hero(new Vector3(0,0,-6),.9f);
            for(int i=0;i<4;i++) Archer(new Vector3(-1.3f+i*.75f,0,-7));
            if(state.March.Phase!="idle")
            {
                var marker=Sphere("March signal",new Vector3(0,.55f,-2),new Vector3(.9f,.25f,.9f),Amber);
                marker.AddComponent<BreachPulse>().Speed=1.5f;
            }
        }
        static void House(string name,Vector3 p,Vector3 size,bool lit)
        {
            Box(name+" · stone base",p+new Vector3(0,size.y*.45f,0),size,WarmStone*.72f);
            GableRoof(name+" · roof",p+new Vector3(0,size.y+0.35f,0),new Vector3(size.x*1.12f,.8f,size.z*1.16f),Timber);
            Box(name+" · door",p+new Vector3(0,.75f,-size.z*.52f),new Vector3(.48f,1.25f,.16f),Timber*.82f);
            if(lit) Glow(name+" · hearth",p+new Vector3(.45f,1.0f,-size.z*.58f),Amber,.85f,2.7f);
        }
        static GameObject GableRoof(string name,Vector3 p,Vector3 s,Color c)
        {
            var go=new GameObject(name);
            go.transform.position=p;
            var mf=go.AddComponent<MeshFilter>();
            var mr=go.AddComponent<MeshRenderer>();
            float x=s.x*.5f,z=s.z*.5f,h=s.y;
            var mesh=new Mesh();
            mesh.vertices=new[]{
                new Vector3(-x,0,-z),new Vector3(x,0,-z),new Vector3(0,h,-z),
                new Vector3(-x,0,z),new Vector3(0,h,z),new Vector3(x,0,z)
            };
            mesh.triangles=new[]{
                0,1,2, 3,4,5,
                0,2,4, 0,4,3,
                1,5,4, 1,4,2
            };
            mesh.RecalculateNormals();
            mf.sharedMesh=mesh; mr.sharedMaterial=Mat(c);
            return go;
        }
        static void Arch(string name,Vector3 center,float radius,float thickness,int blocks,Color color)
        {
            // Upper semicircle plus two massive piers.
            for(int i=0;i<blocks;i++)
            {
                float t=i/(float)(blocks-1);
                float a=Mathf.Lerp(20f,160f,t)*Mathf.Deg2Rad;
                var p=center+new Vector3(Mathf.Cos(a)*radius,Mathf.Sin(a)*radius,0);
                var b=Box(name+" · voussoir",p,new Vector3(thickness,1.05f,1.65f),color);
                b.transform.rotation=Quaternion.Euler(0,0,90f-Mathf.Rad2Deg*a);
            }
            Box(name+" · left pier",center+new Vector3(-radius+.45f,1.9f,0),new Vector3(thickness*1.25f,4.2f,1.9f),color*.95f);
            Box(name+" · right pier",center+new Vector3(radius-.45f,1.9f,0),new Vector3(thickness*1.25f,4.2f,1.9f),color*.95f);
        }
        static void Scaffold(Vector3 p,Vector3 s)
        {
            Color wood=new Color(.27f,.18f,.11f);
            float hx=s.x*.5f,hz=s.z*.5f;
            foreach(float x in new[]{-hx,hx})
                foreach(float z in new[]{-hz,hz})
                    Box("Scaffold post",p+new Vector3(x,0,z),new Vector3(.12f,s.y,.12f),wood);
            for(int level=0;level<3;level++)
            {
                float y=-s.y*.45f+level*s.y*.45f;
                Box("Scaffold rail",p+new Vector3(0,y,-hz),new Vector3(s.x,.10f,.10f),wood);
                Box("Scaffold rail",p+new Vector3(0,y,hz),new Vector3(s.x,.10f,.10f),wood);
            }
        }
        static void PineTree(Vector3 p,float scale)
        {
            Cylinder("Pine trunk",p+Vector3.up*1.25f*scale,new Vector3(.16f,1.3f,.16f)*scale,new Color(.22f,.15f,.10f),Quaternion.identity);
            for(int i=0;i<3;i++)
            {
                float y=(1.4f+i*.65f)*scale;
                Cylinder("Pine crown",p+Vector3.up*y,new Vector3((1.15f-i*.18f)*scale,.65f*scale,(1.15f-i*.18f)*scale),Pine*(.9f+i*.05f),Quaternion.identity);
            }
        }
        static void Tower(Vector3 p,float height)
        {
            Cylinder("Imperial tower",p+Vector3.up*(height/2),new Vector3(1.8f,height,1.8f),Stone*.8f,Quaternion.identity);
            Cylinder("Tower rim",p+Vector3.up*(height+.1f),new Vector3(2.2f,.45f,2.2f),Stone,Quaternion.identity);
            Cylinder("Slate roof",p+Vector3.up*(height+.45f),new Vector3(1.6f,.3f,1.6f),Deep,Quaternion.identity);
        }
        static void Tree(Vector3 p,bool living)
        {
            var trunk=Cylinder("Border tree",p+new Vector3(0,1.2f,0),new Vector3(.22f,2.4f,.22f),new Color(.24f,.21f,.2f),Quaternion.Euler(0,0,12));
            var branch=Box("Branch",p+new Vector3(.35f,2.3f,0),new Vector3(1,.12f,.12f),new Color(.25f,.23f,.2f)); branch.transform.rotation=Quaternion.Euler(0,0,28);
            if(living) Sphere("Ash needles",p+new Vector3(0,2.8f,0),new Vector3(1.3f,.85f,1.1f),new Color(.17f,.25f,.22f));
        }
        static void Hero(Vector3 p,float scale)
        {
            var body=Cylinder("Sir Aldric · guardian",p+Vector3.up*.95f*scale,new Vector3(.7f,1.65f,.58f)*scale,
                new Color(.34f,.40f,.47f),Quaternion.identity);
            Sphere("Aldric helm",p+Vector3.up*2.05f*scale,Vector3.one*.58f*scale,Stone*.78f);
            Box("Aldric cloak",p+new Vector3(0,.95f,.38f)*scale,new Vector3(.8f,1.3f,.13f)*scale,new Color(.29f,.14f,.16f));
            Box("Aldric shield",p+new Vector3(-.57f,1.05f,-.1f)*scale,new Vector3(.2f,.9f,.65f)*scale,new Color(.50f,.40f,.27f));
        }
        static void Archer(Vector3 p)
        {
            Cylinder("Archer silhouette",p+Vector3.up*.7f,new Vector3(.27f,1.2f,.28f),new Color(.31f,.33f,.33f),Quaternion.identity);
            Sphere("Archer hood",p+Vector3.up*1.48f,Vector3.one*.34f,new Color(.20f,.24f,.24f));
            Cylinder("Bow",p+new Vector3(.33f,.88f,0),new Vector3(.07f,1.2f,.07f),new Color(.55f,.39f,.2f),Quaternion.Euler(0,0,12));
        }
        static void Rift(Vector3 p)
        {
            for(int i=0;i<5;i++)
            {
                var shard=Box("Fractured Breach shard",p+new Vector3((i-2)*.77f,1.15f+i%2*.5f,i%2*.4f),
                    new Vector3(.28f,2.2f+i%2, .27f),Violet * (i%2==0?1:.6f));
                shard.transform.rotation=Quaternion.Euler(13,i*31,(i-2)*12);
                shard.AddComponent<BreachPulse>().Speed=.9f+i*.17f;
            }
            Glow("Breach wound",p+new Vector3(0,1.2f,0),Violet,2.4f,8);
            Cylinder("Corruption scar",p+new Vector3(0,.07f,0),new Vector3(4,.08f,3),new Color(.25f,.08f,.32f),Quaternion.identity);
        }
        static void Glow(string name,Vector3 p,Color color,float intensity,float range)
        {
            var light=new GameObject(name).AddComponent<Light>(); light.type=LightType.Point;
            light.transform.position=p; light.color=color; light.intensity=intensity; light.range=range;
            light.gameObject.AddComponent<BreachPulse>().Speed=color==Violet?.8f:1.3f;
        }
        static Material Mat(Color color)
        {
            bool urp=GraphicsSettings.defaultRenderPipeline!=null;
            var shader=Shader.Find(urp?"Universal Render Pipeline/Lit":"Standard");
            if(shader==null) shader=Shader.Find("Unlit/Color");
            var mat=new Material(shader);mat.color=color; return mat;
        }
        static GameObject Shape(string name,PrimitiveType type,Vector3 p,Vector3 scale,Color color)
        {
            var go=GameObject.CreatePrimitive(type);go.name=name;go.transform.position=p;go.transform.localScale=scale;
            go.GetComponent<Renderer>().sharedMaterial=Mat(color);return go;
        }
        static GameObject Box(string n,Vector3 p,Vector3 s,Color c)=>Shape(n,PrimitiveType.Cube,p,s,c);
        static GameObject Cylinder(string n,Vector3 p,Vector3 s,Color c,Quaternion q)
        {var go=Shape(n,PrimitiveType.Cylinder,p,s,c);go.transform.rotation=q;return go;}
        static GameObject Sphere(string n,Vector3 p,Vector3 s,Color c)=>Shape(n,PrimitiveType.Sphere,p,s,c);
    }
    public sealed class WorldHotspot:MonoBehaviour { public string Id; }
    public sealed class BreachPulse:MonoBehaviour
    {
        public float Speed=1;
        private Vector3 basis;private Light point;
        void Awake(){basis=transform.localScale;point=GetComponent<Light>();}
        void Update(){float s=1+.08f*Mathf.Sin(Time.time*Speed*2);transform.localScale=basis*s;
            if(point!=null)point.intensity=Mathf.Max(.8f,point.intensity+Mathf.Sin(Time.time*Speed)*.001f);}
    }
}
