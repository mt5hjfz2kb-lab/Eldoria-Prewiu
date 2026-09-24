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
            RenderSettings.ambientLight = new Color(.40f,.43f,.55f);
            RenderSettings.fog = true; RenderSettings.fogMode = FogMode.Linear;
            RenderSettings.fogColor = new Color(.085f,.11f,.16f); RenderSettings.fogStartDistance=24; RenderSettings.fogEndDistance=75;
            var cameraGo = new GameObject("Isometric camera");
            var camera = cameraGo.AddComponent<Camera>(); camera.orthographic=true;
            camera.orthographicSize = city ? 13 : 14;
            camera.backgroundColor = new Color(.07f,.1f,.15f); camera.clearFlags=CameraClearFlags.SolidColor;
            cameraGo.tag="MainCamera";
            cameraGo.transform.position = city ? new Vector3(21,25,-23) : new Vector3(20,24,-21);
            cameraGo.transform.LookAt(new Vector3(0,0,1));
            var sun = new GameObject("Cold dawn").AddComponent<Light>();
            sun.type=LightType.Directional; sun.color=new Color(.65f,.74f,1f); sun.intensity=1.15f;
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
            Box("Bastion · foundation",new Vector3(0,.7f,4),new Vector3(6,2.2f,5),Stone);
            Box("Bastion · upper hall",new Vector3(0,2.5f,4),new Vector3(4,1.7f,3.8f),Stone*.82f);
            for(int i=0;i<4;i++) Tower(new Vector3(i%2==0?-4:4,0,i<2?1:7),4.7f);
            for(int i=0;i<8;i++) Box("Battlement",new Vector3(-3.1f+i*.9f,4,1.5f),new Vector3(.48f,.7f,.65f),Stone*.88f);
            Box("Gate arch left",new Vector3(-2,1,-5.5f),new Vector3(1.1f,3,1),Stone);
            Box("Gate arch right",new Vector3(2,1,-5.5f),new Vector3(1.1f,3,1),Stone);
            Box("Gate lintel",new Vector3(0,2.7f,-5.5f),new Vector3(5.1f,.85f,1.3f),Stone*.85f);
            var gate=Box("Puerta · ir al mundo",new Vector3(0,.7f,-5.5f),new Vector3(2.6f,2.6f,.37f),new Color(.30f,.18f,.12f));
            gate.AddComponent<WorldHotspot>().Id="gate";
            Box("Sawmill base",new Vector3(-7,.3f,-.6f),new Vector3(3.3f,.8f,3),Stone*.8f);
            var mill=Box("Aserradero · interacción",new Vector3(-7,1.3f,-.6f),new Vector3(2.4f,1.5f,2.2f),
                state.SawmillLevel>0?new Color(.46f,.32f,.2f):new Color(.19f,.20f,.20f));
            mill.AddComponent<WorldHotspot>().Id="sawmill";
            if(state.SawmillLevel>0)
            {
                Box("Restored roof",new Vector3(-7,2.3f,-.6f),new Vector3(2.9f,.35f,2.6f),new Color(.39f,.22f,.16f));
                Glow("Sawmill fire",new Vector3(-6.25f,1.9f,-1),Amber,2.2f,5);
                for(int i=0;i<3;i++) Cylinder("Logs",new Vector3(-9+i*.4f,.35f,-2),new Vector3(.35f,2.4f,.35f),new Color(.33f,.23f,.18f),Quaternion.Euler(90,0,0));
            }
            else for(int i=0;i<4;i++) Box("Charred timber",new Vector3(-8+i*.45f,1.9f,-.4f),new Vector3(.2f,1.3f,.3f),new Color(.13f,.14f,.15f)).transform.rotation=Quaternion.Euler(0,0,17+i*8);
            Hero(new Vector3(-1.9f,0,-2.3f),1.0f);
            for(int i=0;i<5;i++) Archer(new Vector3(1.5f+i*.75f,0,-3.4f+(i%2)*.85f));
            Glow("Torch left",new Vector3(-2.8f,2,-5.2f),Amber,1.5f,3);
            Glow("Torch right",new Vector3(2.8f,2,-5.2f),Amber,1.5f,3);
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
