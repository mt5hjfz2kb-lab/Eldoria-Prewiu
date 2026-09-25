using Eldoria.Domain;
using UnityEngine;
using UnityEngine.Rendering;

namespace Eldoria.Presentation
{
    // Original procedural study: provisional geometry/materials, no inherited web art or unlicensed assets.
    public static class VisualWorld
    {
        static readonly Color Stone = new Color(.23f,.27f,.31f), Deep = new Color(.08f,.12f,.16f);
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
            Box("Fractured ground",new Vector3(0,-.55f,0),new Vector3(33,1,29),Deep);
            for(int i=0;i<17;i++)
            {
                float x=-15+i*1.85f, z=11+(i%3)*.9f;
                Box("Ridge",new Vector3(x,.2f,z),new Vector3(1.8f,1.4f+(i%4)*.65f,2),Stone*.65f);
            }
            for(int i=0;i<7;i++)
            {
                var rock=Box("Ruined road",new Vector3(-2+i*.65f,-.01f,-12+i*3.6f),new Vector3(2.1f,.12f,1.5f),Stone*.72f);
                rock.transform.rotation=Quaternion.Euler(0,(i%2==0?13:-17),0);
            }
            if(city) City(state); else Frontier(state);
            Rift(city?new Vector3(11,1,10):new Vector3(9,1,8));
        }
        static void City(PlayerState state)
        {
            // Visual Bible pass 01: readable mobile city composition, Bastion first.
            // The geometry remains deliberately primitive/procedural; this establishes framing and hierarchy before real assets.

            // Broad rebuilt terrace keeps the playable city legible as one coherent place.
            Box("Upper terrace",new Vector3(0,.15f,3.8f),new Vector3(15,.55f,10),Stone*.72f);
            Box("Lower terrace",new Vector3(0,-.02f,-3.4f),new Vector3(18,.35f,6.5f),Stone*.58f);

            // Signature ancient landmark: a broken cyclopean arch reused by the living city.
            Box("Cyclopean arch · left pier",new Vector3(-8,3.2f,3.5f),new Vector3(2.15f,7.4f,2.4f),Stone*.64f);
            var archRight=Box("Cyclopean arch · broken right pier",new Vector3(7.6f,2.3f,4.5f),new Vector3(1.9f,5.2f,2.2f),Stone*.60f);
            archRight.transform.rotation=Quaternion.Euler(0,0,-8);
            var archSpan=Box("Cyclopean arch · fractured span",new Vector3(-1.4f,6.25f,3.8f),new Vector3(11.8f,1.35f,2.25f),Stone*.62f);
            archSpan.transform.rotation=Quaternion.Euler(0,0,-5);
            var fallen=Box("Cyclopean arch · fallen segment",new Vector3(6.2f,.55f,-1.1f),new Vector3(5.8f,1.3f,1.9f),Stone*.55f);
            fallen.transform.rotation=Quaternion.Euler(0,18,-14);

            // Bastion: larger, closer and visually dominant.
            Box("Bastion · foundation",new Vector3(0,1.0f,4.7f),new Vector3(8.4f,2.8f,6.4f),Stone);
            Box("Bastion · upper hall",new Vector3(0,3.35f,4.9f),new Vector3(5.7f,2.2f,4.4f),Stone*.84f);
            Box("Bastion · crown",new Vector3(0,5.05f,5.0f),new Vector3(3.1f,1.6f,2.7f),Stone*.72f);
            for(int i=0;i<4;i++) Tower(new Vector3(i%2==0?-5.2f:5.2f,0,i<2?1.9f:7.5f),5.5f);
            for(int i=0;i<9;i++) Box("Battlement",new Vector3(-3.6f+i*.9f,5.25f,1.45f),new Vector3(.48f,.72f,.65f),Stone*.9f);

            // Main gate and route to the world stay centered and obvious.
            Box("Gate arch left",new Vector3(-2.2f,1.1f,-4.9f),new Vector3(1.15f,3.2f,1),Stone);
            Box("Gate arch right",new Vector3(2.2f,1.1f,-4.9f),new Vector3(1.15f,3.2f,1),Stone);
            Box("Gate lintel",new Vector3(0,2.95f,-4.9f),new Vector3(5.6f,.9f,1.3f),Stone*.86f);
            var gate=Box("Puerta · ir al mundo",new Vector3(0,.8f,-4.9f),new Vector3(2.8f,2.75f,.4f),new Color(.30f,.18f,.12f));
            gate.AddComponent<WorldHotspot>().Id="gate";

            // Simple roads/steps visually lead the eye from foreground to the Bastion.
            for(int i=0;i<6;i++)
                Box("Bastion approach",new Vector3(0,.15f,-2.8f+i*1.15f),new Vector3(2.5f,.12f,.8f),Stone*.78f);

            // Left work district: Sawmill.
            Box("Sawmill yard",new Vector3(-6.4f,.15f,-1.6f),new Vector3(5.2f,.35f,4.4f),Stone*.54f);
            Box("Sawmill base",new Vector3(-6.5f,.6f,-1.5f),new Vector3(3.7f,1.0f,3.2f),Stone*.8f);
            var mill=Box("Aserradero · interacción",new Vector3(-6.5f,1.7f,-1.5f),new Vector3(2.7f,1.6f,2.4f),
                state.SawmillLevel>0?new Color(.46f,.32f,.2f):new Color(.19f,.20f,.20f));
            mill.AddComponent<WorldHotspot>().Id="sawmill";
            if(state.SawmillLevel>0)
            {
                Box("Restored roof",new Vector3(-6.5f,2.7f,-1.5f),new Vector3(3.2f,.35f,2.9f),new Color(.39f,.22f,.16f));
                Glow("Sawmill fire",new Vector3(-5.7f,2.1f,-1.8f),Amber,2.2f,5);
                for(int i=0;i<4;i++) Cylinder("Logs",new Vector3(-8.8f+i*.45f,.35f,-2.7f),new Vector3(.35f,2.4f,.35f),new Color(.33f,.23f,.18f),Quaternion.Euler(90,0,0));
            }
            else for(int i=0;i<4;i++) Box("Charred timber",new Vector3(-7.7f+i*.5f,2.0f,-1.4f),new Vector3(.2f,1.4f,.3f),new Color(.13f,.14f,.15f)).transform.rotation=Quaternion.Euler(0,0,17+i*8);

            // Right side: intentionally modest early defenses / supply structures.
            Box("Early barracks",new Vector3(6.1f,.65f,-1.7f),new Vector3(3.6f,1.35f,2.8f),Stone*.73f);
            Box("Barracks roof",new Vector3(6.1f,1.55f,-1.7f),new Vector3(3.9f,.35f,3.0f),new Color(.31f,.21f,.17f));
            Box("Granary",new Vector3(4.7f,.7f,-4.2f),new Vector3(2.8f,1.45f,2.1f),new Color(.36f,.29f,.20f));
            Box("Granary roof",new Vector3(4.7f,1.7f,-4.2f),new Vector3(3.1f,.3f,2.4f),new Color(.29f,.20f,.15f));

            // A few restrained life markers: enough to feel inhabited, not crowded.
            Hero(new Vector3(-1.8f,0,-2.0f),1.0f);
            for(int i=0;i<5;i++) Archer(new Vector3(2.0f+i*.68f,0,-2.9f+(i%2)*.7f));
            Glow("Torch left",new Vector3(-2.9f,2.1f,-4.55f),Amber,1.5f,3);
            Glow("Torch right",new Vector3(2.9f,2.1f,-4.55f),Amber,1.5f,3);
            Glow("Bastion warmth",new Vector3(0,3.6f,3.2f),Amber,1.6f,7);
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
