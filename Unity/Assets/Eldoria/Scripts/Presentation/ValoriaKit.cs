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

        public static readonly Color Stone=new Color(.30f,.31f,.30f);
        public static readonly Color OldStone=new Color(.25f,.26f,.25f);
        public static readonly Color WarmStone=new Color(.40f,.37f,.31f);
        public static readonly Color Timber=new Color(.20f,.12f,.075f);
        public static readonly Color Slate=new Color(.11f,.13f,.14f);
        public static readonly Color Earth=new Color(.19f,.16f,.12f);
        public static readonly Color Pine=new Color(.075f,.15f,.105f);

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
                    v=.88f+n*.18f+mortar+chip;
                }
                else if(kind=="wood")
                {
                    float grain=.07f*Mathf.Sin((x+y*.18f)*.78f)+.04f*Mathf.Sin(x*.19f);
                    float seam=(x%18<=1)?-.18f:0f;
                    v=.88f+n*.10f+grain+seam;
                }
                else if(kind=="slate")
                {
                    float seam=(y%10<=1)?-.16f:0f;
                    v=.82f+n*.16f+seam;
                }
                else if(kind=="pine")
                {
                    v=.78f+n*.24f+((x+y)%9==0?-.08f:0f);
                }
                else
                {
                    float pebble=((x*5+y*11)%29==0)?.10f:0f;
                    v=.82f+n*.20f+pebble;
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
    }
}
