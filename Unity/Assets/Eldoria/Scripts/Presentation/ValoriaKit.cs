using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace Eldoria.Presentation
{
    // First reusable environment kit for Valoria.
    // It deliberately uses generated meshes/materials so the repository remains self-contained.
    // These modules can later be replaced 1:1 by authored FBX/prefabs without changing city layout/gameplay.
    public static class ValoriaKit
    {
        static readonly Dictionary<Color32,Material> Materials=new();
        static GameObject LoadExternal(string resourceName)
        {
            return Resources.Load<GameObject>("Valoria/"+resourceName);
        }

        public static GameObject ExternalPrefab(string name,GameObject prefab,Vector3 position,Vector3 scale,Quaternion rotation)
        {
            if(prefab==null)return null;
            var go=Object.Instantiate(prefab,position,rotation);
            go.name=name;
            go.transform.localScale=scale;
            return go;
        }

        public static readonly Color Stone=new Color(.43f,.43f,.40f);
        public static readonly Color OldStone=new Color(.36f,.36f,.34f);
        public static readonly Color WarmStone=new Color(.52f,.47f,.38f);
        public static readonly Color Timber=new Color(.29f,.18f,.10f);
        public static readonly Color Slate=new Color(.18f,.20f,.21f);
        public static readonly Color Earth=new Color(.28f,.23f,.17f);
        public static readonly Color Pine=new Color(.12f,.23f,.16f);

        public static GameObject CastleWall(string name,Vector3 position,Vector3 scale,Quaternion rotation)
            => ExternalPrefab(name,LoadExternal("Stone_Wall"),position,scale,rotation);

        public static GameObject CastleTower(string name,Vector3 position,Vector3 scale,Quaternion rotation)
            => ExternalPrefab(name,LoadExternal("Stone_Tower"),position,scale,rotation);

        public static GameObject CastleGate(string name,Vector3 position,Vector3 scale,Quaternion rotation)
            => ExternalPrefab(name,LoadExternal("Stone_Gate"),position,scale,rotation);

        public static void SmokePlume(string name,Vector3 position,float size=1f,float rate=7f)
        {
            // Package-free smoke approximation so the Unity slice does not depend on the optional
            // Particle System module. Each wisp is a lightweight animated primitive.
            for(int i=0;i<5;i++)
            {
                var wisp=GameObject.CreatePrimitive(PrimitiveType.Sphere);
                wisp.name=name+" · wisp "+i;
                wisp.transform.position=position+new Vector3(
                    ((i%2==0)?-.08f:.08f)*size,
                    i*.34f*size,
                    ((i%3)-1)*.05f*size);
                wisp.transform.localScale=Vector3.one*(.34f+i*.07f)*size;

                var collider=wisp.GetComponent<Collider>();
                if(collider!=null)Object.Destroy(collider);

                var renderer=wisp.GetComponent<Renderer>();
                renderer.sharedMaterial=Material(new Color(
                    .46f-i*.025f,.45f-i*.025f,.43f-i*.02f,1f));

                var drift=wisp.AddComponent<SmokeWisp>();
                drift.BasePosition=wisp.transform.position;
                drift.Phase=i*.83f;
                drift.Height=1.35f*size;
                drift.Speed=.22f+i*.018f;
                drift.BaseScale=wisp.transform.localScale;
            }
        }

        static readonly Dictionary<string,Texture2D> Textures=new();

        public static Material Material(Color color)
        {
            var key=(Color32)color;
            if(Materials.TryGetValue(key,out var cached)&&cached!=null)return cached;
            bool urp=GraphicsSettings.defaultRenderPipeline!=null;
            var shader=Shader.Find(urp?"Universal Render Pipeline/Lit":"Standard");
            if(shader==null)shader=Shader.Find("Unlit/Color");
            var mat=new Material(shader){name="Valoria "+ColorUtility.ToHtmlStringRGB(color)};
            mat.color=color;
            if(mat.HasProperty("_BaseColor"))mat.SetColor("_BaseColor",color);
            if(mat.HasProperty("_Smoothness"))mat.SetFloat("_Smoothness",.08f);
            if(mat.HasProperty("_Metallic"))mat.SetFloat("_Metallic",0f);

            string pattern=PatternFor(color);
            var tex=PatternTexture(pattern,color);
            if(mat.HasProperty("_BaseMap"))mat.SetTexture("_BaseMap",tex);
            else if(mat.HasProperty("_MainTex"))mat.SetTexture("_MainTex",tex);
            if(mat.HasProperty("_BaseMap"))mat.SetTextureScale("_BaseMap",new Vector2(2.6f,2.6f));
            else if(mat.HasProperty("_MainTex"))mat.SetTextureScale("_MainTex",new Vector2(2.6f,2.6f));

            Materials[key]=mat;
            return mat;
        }

        static string PatternFor(Color c)
        {
            if(c.g>c.r*1.25f && c.g>c.b*1.15f)return "pine";
            if(c.r>c.g*1.22f && c.g>c.b*1.15f)return c.r<.28f?"wood":"earth";
            if(c.r<.17f&&c.g<.19f&&c.b<.21f)return "slate";
            return "stone";
        }

        static Texture2D PatternTexture(string kind,Color baseColor)
        {
            string key=kind+"-"+ColorUtility.ToHtmlStringRGB(baseColor);
            if(Textures.TryGetValue(key,out var cached)&&cached!=null)return cached;
            const int size=64;
            var tex=new Texture2D(size,size,TextureFormat.RGBA32,false){name="Valoria "+kind+" "+key,wrapMode=TextureWrapMode.Repeat,filterMode=FilterMode.Bilinear};
            var pixels=new Color[size*size];
            int seed=17;
            for(int y=0;y<size;y++)
            for(int x=0;x<size;x++)
            {
                seed=(seed*1103515245+12345)&0x7fffffff;
                float n=((seed>>8)&255)/255f;
                float v=1f;
                if(kind=="stone")
                {
                    float mortar=(x%16<=1||y%12<=1)?-.20f:0f;
                    float chip=((x*7+y*13)%37==0)?-.12f:0f;
                    v=1.00f+n*.16f+mortar*.65f+chip*.65f;
                }
                else if(kind=="wood")
                {
                    float grain=.07f*Mathf.Sin((x+y*.18f)*.78f)+.04f*Mathf.Sin(x*.19f);
                    float seam=(x%18<=1)?-.18f:0f;
                    v=.98f+n*.10f+grain*.7f+seam*.65f;
                }
                else if(kind=="slate")
                {
                    float seam=(y%10<=1)?-.16f:0f;
                    v=.96f+n*.14f+seam*.65f;
                }
                else if(kind=="pine")
                {
                    v=.94f+n*.20f+((x+y)%9==0?-.05f:0f);
                }
                else
                {
                    float pebble=((x*5+y*11)%29==0)?.10f:0f;
                    v=.96f+n*.18f+pebble*.7f;
                }
                pixels[y*size+x]=new Color(Mathf.Clamp01(baseColor.r*v),Mathf.Clamp01(baseColor.g*v),Mathf.Clamp01(baseColor.b*v),1);
            }
            tex.SetPixels(pixels);tex.Apply(false,false);
            Textures[key]=tex;
            return tex;
        }

        public static GameObject Block(string name,Vector3 p,Vector3 scale,Color color)
        {
            var go=GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name=name;go.transform.position=p;go.transform.localScale=scale;
            go.GetComponent<Renderer>().sharedMaterial=Material(color);
            return go;
        }

        public static GameObject Cylinder(string name,Vector3 p,Vector3 scale,Color color,Quaternion rotation)
        {
            var go=GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            go.name=name;go.transform.position=p;go.transform.localScale=scale;go.transform.rotation=rotation;
            go.GetComponent<Renderer>().sharedMaterial=Material(color);
            return go;
        }

        public static void Wall(string name,Vector3 p,Vector3 size,Color color,bool battlements=true)
        {
            Block(name+" · wall",p,size,color);
            if(!battlements)return;
            int count=Mathf.Max(3,Mathf.RoundToInt(size.x/.72f));
            for(int i=0;i<count;i++)
            {
                float t=count==1?0:i/(float)(count-1);
                float x=Mathf.Lerp(-size.x*.46f,size.x*.46f,t);
                Block(name+" · merlon",p+new Vector3(x,size.y*.57f,0),
                    new Vector3(.38f,.55f,size.z*1.08f),color*.92f);
            }
        }

        public static void Tower(string name,Vector3 basePos,float radius,float height,Color color)
        {
            Cylinder(name+" · body",basePos+Vector3.up*(height*.5f),
                new Vector3(radius,height*.5f,radius),color,Quaternion.identity);
            Cylinder(name+" · rim",basePos+Vector3.up*(height+.12f),
                new Vector3(radius*1.13f,.20f,radius*1.13f),color*.88f,Quaternion.identity);
            int teeth=8;
            for(int i=0;i<teeth;i++)
            {
                float a=i*Mathf.PI*2/teeth;
                var q=basePos+new Vector3(Mathf.Cos(a)*radius*.88f,height+.48f,Mathf.Sin(a)*radius*.88f);
                Block(name+" · crenel",q,new Vector3(.34f,.62f,.34f),color*.94f);
            }
        }

        public static GameObject GableRoof(string name,Vector3 p,Vector3 size,Color color)
        {
            var go=new GameObject(name);go.transform.position=p;
            var mf=go.AddComponent<MeshFilter>();var mr=go.AddComponent<MeshRenderer>();
            float x=size.x*.5f,z=size.z*.5f,h=size.y;
            var mesh=new Mesh{name=name+" mesh"};
            mesh.vertices=new[]{
                new Vector3(-x,0,-z),new Vector3(x,0,-z),new Vector3(0,h,-z),
                new Vector3(-x,0,z),new Vector3(0,h,z),new Vector3(x,0,z)
            };
            mesh.triangles=new[]{0,1,2,3,4,5,0,2,4,0,4,3,1,5,4,1,4,2};
            mesh.RecalculateNormals();mesh.RecalculateBounds();
            mf.sharedMesh=mesh;mr.sharedMaterial=Material(color);
            return go;
        }

        public static void House(string name,Vector3 p,Vector3 size,bool lit,System.Action<string,Vector3,Color,float,float> glow)
        {
            Block(name+" · masonry",p+Vector3.up*(size.y*.48f),size,WarmStone*.75f);
            GableRoof(name+" · roof",p+new Vector3(0,size.y+.12f,0),
                new Vector3(size.x*1.14f,.75f,size.z*1.18f),Slate);
            Block(name+" · timber door",p+new Vector3(0,.66f,-size.z*.515f),
                new Vector3(.46f,1.15f,.12f),Timber);
            Block(name+" · beam",p+new Vector3(0,size.y*.72f,-size.z*.52f),
                new Vector3(size.x*.82f,.12f,.13f),Timber*.9f);
            if(lit&&glow!=null)glow(name+" · hearth",p+new Vector3(.42f,.95f,-size.z*.59f),new Color(.96f,.53f,.22f),.75f,2.5f);
        }

        public static void Scaffold(string name,Vector3 p,Vector3 size)
        {
            float hx=size.x*.5f,hz=size.z*.5f;
            foreach(float x in new[]{-hx,hx})
                foreach(float z in new[]{-hz,hz})
                    Block(name+" · post",p+new Vector3(x,0,z),new Vector3(.10f,size.y,.10f),Timber);
            for(int level=0;level<4;level++)
            {
                float y=-size.y*.44f+level*size.y*.30f;
                Block(name+" · rail",p+new Vector3(0,y,-hz),new Vector3(size.x,.09f,.10f),Timber);
                Block(name+" · rail",p+new Vector3(0,y,hz),new Vector3(size.x,.09f,.10f),Timber);
            }
            Block(name+" · platform",p+new Vector3(0,size.y*.05f,0),new Vector3(size.x,.12f,size.z),Timber*.86f);
        }

        public static void BrokenArch(string name,Vector3 center,float radius,float depth,Color color)
        {
            // One tall intact side and an intentionally broken crown: signature ruin, not a perfect bridge.
            Block(name+" · left pier",center+new Vector3(-radius+.45f,2.25f,0),new Vector3(1.7f,5.0f,depth),color);
            Block(name+" · right stump",center+new Vector3(radius-.55f,1.25f,0),new Vector3(1.55f,3.0f,depth),color*.88f);
            const int blocks=8;
            for(int i=0;i<blocks;i++)
            {
                float t=i/(float)(blocks-1);
                float degrees=Mathf.Lerp(52f,156f,t);
                float a=degrees*Mathf.Deg2Rad;
                var pos=center+new Vector3(Mathf.Cos(a)*radius,Mathf.Sin(a)*radius,0);
                var b=Block(name+" · voussoir",pos,new Vector3(1.55f,.92f,depth),color*(.92f+(i%2)*.04f));
                b.transform.rotation=Quaternion.Euler(0,0,90f-degrees);
            }
        }

        public static void Buttress(string name,Vector3 foot,float height,float depth,Color color)
        {
            var lower=Block(name+" · lower",foot+new Vector3(0,height*.24f,0),
                new Vector3(.78f,height*.48f,depth),color);
            lower.transform.rotation=Quaternion.Euler(-4f,0,0);
            var upper=Block(name+" · upper",foot+new Vector3(0,height*.66f,depth*.14f),
                new Vector3(.58f,height*.42f,depth*.72f),color*.94f);
            upper.transform.rotation=Quaternion.Euler(-7f,0,0);
        }

        public static void BrokenCrown(string name,Vector3 center,Color color)
        {
            float[] xs={-1.9f,-1.15f,-.4f,.35f,1.1f};
            float[] hs={1.8f,2.6f,2.15f,3.0f,1.55f};
            for(int i=0;i<xs.Length;i++)
            {
                var rib=Block(name+" · rib "+i,center+new Vector3(xs[i],hs[i]*.5f,0),
                    new Vector3(.42f,hs[i],1.0f),color*(.86f+(i%2)*.05f));
                rib.transform.rotation=Quaternion.Euler(0,(i-2)*3f,(i%2==0?-2f:2f));
            }
            Block(name+" · broken gallery",center+new Vector3(-.35f,1.15f,-.45f),
                new Vector3(4.7f,.42f,1.15f),color*.82f).transform.rotation=Quaternion.Euler(0,0,-3f);
        }

        public static void BastionCore(string name,Vector3 origin,System.Action<string,Vector3,Color,float,float> glow)
        {
            // Hybrid production pass: authored modular castle meshes carry the readable architecture,
            // while bespoke procedural masses preserve Eldoria's unique fortress-inside-a-dead-palace silhouette.
            var stoneTower=LoadExternal("Stone_Tower");
            var stoneWall=LoadExternal("Stone_Wall");
            var stoneGate=LoadExternal("Stone_Gate");

            Block(name+" · rock plinth",origin+new Vector3(0,.68f,0),
                new Vector3(10.7f,1.55f,7.6f),OldStone*.82f);
            Block(name+" · palace remnant",origin+new Vector3(-.45f,5.35f,.9f),
                new Vector3(4.35f,1.65f,3.25f),OldStone*.90f);

            if(stoneWall!=null && stoneTower!=null && stoneGate!=null)
            {
                // Real modular front curtain: three wall sections around a central gate.
                ExternalPrefab(name+" · authored gate",stoneGate,
                    origin+new Vector3(0,.06f,-3.0f),Vector3.one*.86f,Quaternion.identity);
                ExternalPrefab(name+" · authored wall west",stoneWall,
                    origin+new Vector3(-3.25f,.06f,-2.82f),new Vector3(.78f,.88f,.82f),Quaternion.identity);
                ExternalPrefab(name+" · authored wall east",stoneWall,
                    origin+new Vector3(3.25f,.06f,-2.82f),new Vector3(.78f,.88f,.82f),Quaternion.identity);

                ExternalPrefab(name+" · authored tower west",stoneTower,
                    origin+new Vector3(-5.0f,.04f,-2.1f),Vector3.one*.95f,Quaternion.identity);
                ExternalPrefab(name+" · authored tower east",stoneTower,
                    origin+new Vector3(5.0f,.04f,-2.1f),Vector3.one*.95f,Quaternion.identity);
                ExternalPrefab(name+" · authored rear west",stoneTower,
                    origin+new Vector3(-4.35f,.04f,2.45f),Vector3.one*.82f,Quaternion.identity);
                ExternalPrefab(name+" · authored rear east",stoneTower,
                    origin+new Vector3(4.35f,.04f,2.45f),Vector3.one*.82f,Quaternion.identity);
            }
            else
            {
                // Safe fallback keeps the slice functional if the external pack is unavailable.
                Wall(name+" · front curtain",origin+new Vector3(0,2.15f,-2.65f),
                    new Vector3(9.2f,2.7f,1.15f),Stone*.94f,true);
                Tower(name+" · west tower",origin+new Vector3(-5.0f,.05f,-2.15f),1.38f,5.7f,Stone*.90f);
                Tower(name+" · east tower",origin+new Vector3(5.0f,.05f,-2.15f),1.38f,5.7f,Stone*.90f);
                Tower(name+" · rear west",origin+new Vector3(-4.45f,.05f,2.5f),1.12f,5.0f,OldStone*.86f);
                Tower(name+" · rear east",origin+new Vector3(4.45f,.05f,2.5f),1.12f,5.0f,OldStone*.86f);
            }

            // Bespoke keep avoids becoming a generic asset-pack castle.
            Block(name+" · inner keep",origin+new Vector3(0,3.65f,.55f),
                new Vector3(6.25f,2.75f,4.55f),Stone*.90f);
            Buttress(name+" · buttress west",origin+new Vector3(-3.25f,.08f,-2.75f),3.45f,1.25f,WarmStone*.82f);
            Buttress(name+" · buttress east",origin+new Vector3(3.25f,.08f,-2.75f),3.45f,1.25f,WarmStone*.82f);

            foreach(float x in new[]{-1.85f,0f,1.85f})
                WindowSlit(name+" · keep slit",origin+new Vector3(x,4.0f,-1.78f),
                    new Vector3(.28f,.72f,.12f),x==0f);

            Banner(name+" · banner west",origin+new Vector3(-2.65f,3.55f,-1.82f),
                new Vector3(.62f,2.25f,.08f),new Color(.34f,.08f,.07f));
            Banner(name+" · banner east",origin+new Vector3(2.65f,3.55f,-1.82f),
                new Vector3(.62f,2.25f,.08f),new Color(.34f,.08f,.07f));

            BrokenCrown(name+" · broken crown",origin+new Vector3(-.3f,6.05f,.95f),OldStone*.90f);
            Scaffold(name+" · repair scaffold",origin+new Vector3(3.7f,3.15f,.75f),
                new Vector3(2.35f,4.6f,2.1f));
            Rubble(name+" · crown rubble",origin+new Vector3(-3.15f,.22f,2.05f),1.35f,8);

            if(glow!=null)glow(name+" · inhabited warmth",origin+new Vector3(0,3.55f,-1.15f),
                new Color(.96f,.53f,.22f),1.25f,6.0f);
        }

        public static void Banner(string name,Vector3 p,Vector3 size,Color color)
        {
            var cloth=Block(name,p,size,color);
            cloth.transform.rotation=Quaternion.Euler(0,0,1.5f);
            Block(name+" · bar",p+new Vector3(0,size.y*.54f,0),new Vector3(size.x*1.25f,.08f,size.z*1.8f),Timber*.8f);
        }

        public static void WindowSlit(string name,Vector3 p,Vector3 size,bool warm=false)
        {
            Block(name,p,size,warm?new Color(.48f,.25f,.08f):new Color(.045f,.05f,.055f));
        }

        public static void RockCluster(string name,Vector3 center,float scale,int count=5)
        {
            for(int i=0;i<count;i++)
            {
                float x=((i*41)%13-6)*.28f*scale;
                float z=((i*29)%11-5)*.24f*scale;
                float y=.16f+((i%3)*.05f)*scale;
                var rock=GameObject.CreatePrimitive(PrimitiveType.Sphere);
                rock.name=name+" · rock "+i;
                rock.transform.position=center+new Vector3(x,y,z);
                rock.transform.localScale=new Vector3(
                    (.72f+(i%3)*.18f)*scale,
                    (.38f+(i%2)*.14f)*scale,
                    (.58f+((i+1)%3)*.16f)*scale);
                rock.transform.rotation=Quaternion.Euler((i*11)%23,(i*37)%180,(i*7)%17);
                var col=rock.GetComponent<Collider>();
                if(col!=null)Object.Destroy(col);
                rock.GetComponent<Renderer>().sharedMaterial=Material(OldStone*(.78f+(i%3)*.06f));
            }
        }

        public static void RuinFragment(string name,Vector3 position,Vector3 scale,float yaw,float lean=0f)
        {
            var wall=CastleWall(name,position,scale,Quaternion.Euler(0,yaw,lean));
            if(wall==null)
            {
                wall=Block(name,position+Vector3.up*.7f,new Vector3(2.4f,1.8f,.65f),OldStone*.78f);
                wall.transform.rotation=Quaternion.Euler(0,yaw,lean);
            }
            Rubble(name+" rubble",position+new Vector3(0,.1f,0),Mathf.Max(.65f,scale.x),5);
        }

        public static void Rubble(string name,Vector3 p,float scale,int count=6)
        {
            for(int i=0;i<count;i++)
            {
                float x=((i*37)%11-5)*.18f*scale;
                float z=((i*23)%9-4)*.16f*scale;
                float s=(.18f+(i%3)*.08f)*scale;
                var r=Block(name+" · stone",p+new Vector3(x,s*.45f,z),new Vector3(s,s*.65f,s*.85f),OldStone*(.82f+(i%2)*.08f));
                r.transform.rotation=Quaternion.Euler(i*7f,i*29f,i*11f);
            }
        }

        public static void Stair(string name,Vector3 start,int steps,float width,float rise,float run,Color color)
        {
            for(int i=0;i<steps;i++)
                Block(name+" · step",start+new Vector3(0,i*rise,i*run),new Vector3(width,.16f,run+.05f),color);
        }

        public static void PineTree(string name,Vector3 p,float scale)
        {
            Cylinder(name+" · trunk",p+Vector3.up*1.1f*scale,new Vector3(.13f,1.1f,.13f)*scale,
                new Color(.20f,.13f,.08f),Quaternion.identity);
            for(int i=0;i<3;i++)
            {
                float y=(1.35f+i*.58f)*scale;
                // Cylinders are intentionally stylized silhouettes for this first kit.
                Cylinder(name+" · crown",p+Vector3.up*y,new Vector3((.9f-i*.14f)*scale,.42f*scale,(.9f-i*.14f)*scale),
                    Pine*(.88f+i*.05f),Quaternion.identity);
            }
        }
    }    public sealed class SmokeWisp:MonoBehaviour
    {
        public Vector3 BasePosition;
        public Vector3 BaseScale;
        public float Phase;
        public float Height=1f;
        public float Speed=.2f;

        void Update()
        {
            float t=Mathf.Repeat(Time.time*Speed+Phase,1f);
            float sway=Mathf.Sin((Time.time+Phase)*1.7f)*.12f;
            transform.position=BasePosition+new Vector3(sway,t*Height,sway*.45f);
            float scale=1f+t*.75f;
            transform.localScale=BaseScale*scale;
        }
    }


}
