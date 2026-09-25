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
        static readonly Dictionary<Material,Material> AdaptedMaterials=new();
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
            foreach(var collider in go.GetComponentsInChildren<Collider>(true))collider.enabled=false;
            foreach(var renderer in go.GetComponentsInChildren<Renderer>(true))
            {
                var materials=renderer.sharedMaterials;
                for(int i=0;i<materials.Length;i++)materials[i]=AdaptForUrp(materials[i]);
                renderer.sharedMaterials=materials;
            }
            return go;
        }

        // Visual-only wrapper: the existing interaction volumes remain the authority for taps.
        // The source prefab is not modified, and missing assets leave the procedural fallback.
        public static GameObject BenchmarkPiece(string name,GameObject prefab,Vector3 ground,
            float footprint,float maxHeight,Quaternion rotation)
        {
            if(prefab==null)return null;
            var go=Object.Instantiate(prefab);
            go.name=name;
            go.transform.SetPositionAndRotation(ground,rotation);
            foreach(var collider in go.GetComponentsInChildren<Collider>(true))collider.enabled=false;
            foreach(var behaviour in go.GetComponentsInChildren<MonoBehaviour>(true))behaviour.enabled=false;
            var renderers=go.GetComponentsInChildren<Renderer>(true);
            if(renderers.Length==0){Object.Destroy(go);return null;}
            // Legacy Standard materials from the props pack cannot render in URP. Convert
            // instance references to shared URP materials while retaining their textures.
            foreach(var renderer in renderers)
            {
                var source=renderer.sharedMaterials;
                for(int i=0;i<source.Length;i++)source[i]=AdaptForUrp(source[i]);
                renderer.sharedMaterials=source;
            }
            var bounds=renderers[0].bounds;
            for(int i=1;i<renderers.Length;i++)bounds.Encapsulate(renderers[i].bounds);
            if(bounds.size.x<.001f||bounds.size.y<.001f||bounds.size.z<.001f)
            {Object.Destroy(go);return null;}
            float factor=Mathf.Min(footprint/Mathf.Max(bounds.size.x,bounds.size.z),maxHeight/bounds.size.y);
            go.transform.localScale*=factor;
            // The imported prefab may have an offset pivot: recompute after scaling.
            bounds=renderers[0].bounds;
            for(int i=1;i<renderers.Length;i++)bounds.Encapsulate(renderers[i].bounds);
            go.transform.position+=ground-new Vector3(bounds.center.x,bounds.min.y,bounds.center.z);
            return go;
        }

        public static GameObject BenchmarkPieceModulated(string name,GameObject prefab,Vector3 ground,
            float footprint,float maxHeight,Quaternion rotation,Color tint)
        {
            var go=BenchmarkPiece(name,prefab,ground,footprint,maxHeight,rotation);
            if(go==null)return null;
            foreach(var renderer in go.GetComponentsInChildren<Renderer>(true))
            {
                var mats=renderer.sharedMaterials;
                for(int i=0;i<mats.Length;i++)
                {
                    var source=mats[i];
                    if(source==null)continue;
                    var material=new Material(source){name="Valoria modulated · "+source.name};
                    var baseColor=material.HasProperty("_BaseColor")?material.GetColor("_BaseColor"):
                        material.HasProperty("_Color")?material.GetColor("_Color"):Color.white;
                    var modulated=new Color(baseColor.r*tint.r,baseColor.g*tint.g,baseColor.b*tint.b,
                        baseColor.a*tint.a);
                    if(material.HasProperty("_BaseColor"))material.SetColor("_BaseColor",modulated);
                    if(material.HasProperty("_Color"))material.SetColor("_Color",modulated);
                    if(material.HasProperty("_EmissionColor"))material.SetColor("_EmissionColor",Color.black);
                    if(material.HasProperty("_EmissionStrength"))material.SetFloat("_EmissionStrength",0f);
                    mats[i]=material;
                }
                renderer.sharedMaterials=mats;
            }
            return go;
        }

        public static GameObject BenchmarkPieceTinted(string name,GameObject prefab,Vector3 ground,
            float footprint,float maxHeight,Quaternion rotation,Color tint)
        {
            var go=BenchmarkPiece(name,prefab,ground,footprint,maxHeight,rotation);
            if(go==null)return null;
            var material=Material(tint);
            foreach(var renderer in go.GetComponentsInChildren<Renderer>(true))
            {
                var mats=renderer.sharedMaterials;
                for(int i=0;i<mats.Length;i++)mats[i]=material;
                renderer.sharedMaterials=mats;
            }
            return go;
        }

        static Material AdaptForUrp(Material source)
        {
            if(source==null||source.shader==null||source.shader.name.StartsWith("Universal Render Pipeline/"))return source;
            if(AdaptedMaterials.TryGetValue(source,out var adapted)&&adapted!=null)return adapted;
            var shader=Shader.Find("Universal Render Pipeline/Lit");
            if(shader==null)return source;
            adapted=new Material(shader){name="Valoria URP · "+source.name};
            var texture=source.HasProperty("_BaseMap")?source.GetTexture("_BaseMap"):
                source.HasProperty("_MainTex")?source.GetTexture("_MainTex"):null;
            if(texture!=null)adapted.SetTexture("_BaseMap",texture);
            var color=source.HasProperty("_BaseColor")?source.GetColor("_BaseColor"):
                source.HasProperty("_Color")?source.GetColor("_Color"):Color.white;
            adapted.SetColor("_BaseColor",color);
            adapted.SetFloat("_Smoothness",.10f);
            AdaptedMaterials[source]=adapted;
            return adapted;
        }

        public static readonly Color Stone=new Color(.41f,.42f,.40f);
        public static readonly Color OldStone=new Color(.35f,.36f,.34f);
        public static readonly Color WarmStone=new Color(.48f,.43f,.35f);
        public static readonly Color Timber=new Color(.31f,.20f,.12f);
        public static readonly Color Slate=new Color(.25f,.29f,.33f);
        public static readonly Color Earth=new Color(.30f,.27f,.23f);
        public static readonly Color Pine=new Color(.11f,.23f,.16f);

        public static GameObject CastleWall(string name,Vector3 position,Vector3 scale,Quaternion rotation)
            => ExternalPrefab(name,LoadExternal("Stone_Wall"),position,scale,rotation);

        public static GameObject CastleTower(string name,Vector3 position,Vector3 scale,Quaternion rotation)
            => ExternalPrefab(name,LoadExternal("Stone_Tower"),position,scale,rotation);

        public static GameObject CastleGate(string name,Vector3 position,Vector3 scale,Quaternion rotation)
            => ExternalPrefab(name,LoadExternal("Stone_Gate"),position,scale,rotation);

        public static GameObject TerrainPiece(string resourceName,string name,Vector3 position,Vector3 scale,Quaternion rotation)
        {
            var piece=ExternalPrefab(name,LoadExternal(resourceName),position,scale,rotation);
            if(piece==null)return null;
            // PolyOne's bright atlas read as white/yellow snow under the Valoria camera.
            // Retain its useful relief mesh, but give it Eldoria's shared earth/rock palette.
            var color=resourceName.Contains("Mountain")?new Color(.33f,.34f,.33f):
                resourceName.Contains("Hill")?new Color(.31f,.29f,.25f):new Color(.36f,.36f,.33f);
            foreach(var renderer in piece.GetComponentsInChildren<Renderer>(true))
                renderer.sharedMaterial=Material(color);
            return piece;
        }

        public static GameObject TerrainPieceModulated(string resourceName,string name,Vector3 ground,
            float footprint,float maxHeight,Quaternion rotation,Color tint)
        {
            // Background-only terrain: preserve the authored atlas and geometry, then mute it into
            // Valoria's atmospheric palette instead of flattening it to the old clay-like solid colour.
            return BenchmarkPieceModulated(name,LoadExternal(resourceName),ground,footprint,maxHeight,rotation,tint);
        }

        public static GameObject TerrainPieceTinted(string resourceName,string name,Vector3 ground,
            float footprint,float maxHeight,Quaternion rotation,Color tint)
        {
            // Some legacy terrain atlases still resolve to white in URP even after conversion.
            // In those cases keep the authored silhouette/mesh but enforce the Eldoria palette.
            return BenchmarkPieceTinted(name,LoadExternal(resourceName),ground,footprint,maxHeight,rotation,tint);
        }

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
            if((c.b>c.r*1.15f&&c.b>c.g*1.06f)||(c.r<.17f&&c.g<.19f&&c.b<.21f))return "slate";
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
                    // Staggered masonry courses avoid the square voxel/grid read of the old texture.
                    int course=y/11;
                    bool horizontal=y%11<=1;
                    int joint=(x+(course%2)*8)%18;
                    bool vertical=joint<=1 && !horizontal;
                    float mortar=(horizontal||vertical)?-.13f:0f;
                    float weather=.035f*Mathf.Sin(x*.21f+y*.09f)+.025f*Mathf.Sin(y*.37f);
                    float chip=((x*7+y*13)%43==0)?-.08f:0f;
                    v=.98f+n*.12f+weather+mortar+chip;
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
            // Two solid roof slabs avoid the single-sided black triangles produced by the old custom mesh.
            var root=new GameObject(name);root.transform.position=p;
            float halfWidth=size.x*.52f;
            float rise=Mathf.Max(.35f,size.y);
            float angle=Mathf.Atan2(rise,halfWidth)*Mathf.Rad2Deg;
            float slope=Mathf.Sqrt(halfWidth*halfWidth+rise*rise);
            var left=Block(name+" · west slope",p+new Vector3(-size.x*.235f,rise*.48f,0),
                new Vector3(slope,.16f,size.z),color);
            left.transform.rotation=Quaternion.Euler(0,0,-angle);
            left.transform.SetParent(root.transform,true);
            var right=Block(name+" · east slope",p+new Vector3(size.x*.235f,rise*.48f,0),
                new Vector3(slope,.16f,size.z),color);
            right.transform.rotation=Quaternion.Euler(0,0,angle);
            right.transform.SetParent(root.transform,true);
            return root;
        }

        public static void House(string name,Vector3 p,Vector3 size,bool lit,System.Action<string,Vector3,Color,float,float> glow)
        {
            // Valoria houses should read as rebuilt medieval structures, not box + roof placeholders.
            Block(name+" · stone footing",p+new Vector3(0,.16f,0),new Vector3(size.x*1.08f,.30f,size.z*1.08f),OldStone*.88f);
            Block(name+" · masonry",p+Vector3.up*(size.y*.48f),size,WarmStone*.78f);
            GableRoof(name+" · roof",p+new Vector3(0,size.y+.16f,0),
                new Vector3(size.x*1.10f,.92f,size.z*1.14f),Slate);
            Block(name+" · timber door",p+new Vector3(0,.66f,-size.z*.515f),
                new Vector3(.46f,1.15f,.12f),Timber);
            Block(name+" · lintel",p+new Vector3(0,1.30f,-size.z*.525f),
                new Vector3(.82f,.12f,.14f),Timber*.92f);
            foreach(float x in new[]{-size.x*.36f,size.x*.36f})
                Block(name+" · facade post",p+new Vector3(x,size.y*.53f,-size.z*.523f),
                    new Vector3(.11f,size.y*.88f,.12f),Timber*.86f);
            Block(name+" · cross beam",p+new Vector3(0,size.y*.72f,-size.z*.526f),
                new Vector3(size.x*.82f,.11f,.12f),Timber*.88f);
            Block(name+" · chimney",p+new Vector3(size.x*.28f,size.y+1.02f,size.z*.10f),
                new Vector3(.34f,1.35f,.34f),OldStone*.76f);
            Block(name+" · side lean-to",p+new Vector3(size.x*.58f,.58f,.05f),
                new Vector3(size.x*.30f,.95f,size.z*.72f),Timber*.72f);
            var leanRoof=Block(name+" · lean-to roof",p+new Vector3(size.x*.58f,1.12f,.05f),
                new Vector3(size.x*.36f,.12f,size.z*.82f),Slate*.92f);
            leanRoof.transform.rotation=Quaternion.Euler(0,0,-12f);
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

        public static void CyclopeanCauseway(string name,Vector3 center,float length,float height,float depth,
            Quaternion rotation,Color color)
        {
            // Monumental broken imperial infrastructure used as landscape, not as a gameplay building.
            // Large gaps and surviving piers make it read as a dead empire crossing the valley.
            int segments=9;
            float step=length/(segments-1);
            for(int i=0;i<segments;i++)
            {
                if(i==3 || i==4)continue;
                float x=-length*.5f+i*step;
                var local=new Vector3(x,height,0);
                var slab=Block(name+" · deck "+i,center+rotation*local,
                    new Vector3(step*1.10f,.70f,depth),color*(.84f+(i%3)*.035f));
                slab.transform.rotation=rotation*Quaternion.Euler(0,0,(i%2==0?1.5f:-1.2f));
            }
            foreach(float x in new[]{-length*.42f,-length*.16f,length*.18f,length*.43f})
            {
                float h=height-(Mathf.Abs(x)<length*.25f?1.1f:.35f);
                var pier=Block(name+" · pier",center+rotation*new Vector3(x,h*.5f,0),
                    new Vector3(1.35f,h,depth*.82f),color*.78f);
                pier.transform.rotation=rotation;
                Buttress(name+" · pier buttress",center+rotation*new Vector3(x,h*.05f,-depth*.45f),
                    Mathf.Max(2.8f,h*.62f),depth*.40f,color*.72f);
            }
            var fallen=Block(name+" · fallen span",center+rotation*new Vector3(-length*.02f,1.05f,depth*.35f),
                new Vector3(step*2.3f,.72f,depth*.86f),color*.70f);
            fallen.transform.rotation=rotation*Quaternion.Euler(0,0,-17f);
            Rubble(name+" · collapse",center+rotation*new Vector3(0,.1f,depth*.35f),1.35f,12);
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
            var art=ValoriaExternalAssetLibrary.Load();
            var stoneTower=art!=null?art.MasonryTower:null;
            var stoneWall=art!=null?art.MasonryWall:null;
            var stoneGate=art!=null?art.MasonryGate:null;

            // The plinth is deliberately rounded and mostly buried. A rectangular top plane
            // was still visible from the district camera and read as a black blockout slab.
            Cylinder(name+" · rock plinth",origin+new Vector3(0,.20f,.30f),
                new Vector3(4.75f,.42f,3.20f),OldStone*.74f,Quaternion.identity);
            // Capture-reviewed: no procedural palace slab above the authored skyline.

            // Use the Mega castle pieces only as one connected facade. A dark backing mass prevents
            // gaps, so individual modules never read as floating props.
            Wall(name+" · front curtain backing",origin+new Vector3(0,1.85f,-2.70f),
                new Vector3(8.8f,2.15f,.52f),OldStone*.58f,false);
            var megaGate=art!=null?art.MegaHalfGate:null;
            var megaTower=art!=null?art.MegaTower:null;
            var megaDestroyedTower=art!=null?art.MegaDestroyedTower:null;
            var megaWallPassage=art!=null?art.MegaWallPassage:null;
            // Preserve the authored stone materials here. Flattening these to a single tint erased
            // the masonry detail and made the connected facade read as one brown slab.
            if(BenchmarkPieceModulated(name+" · connected gate",megaGate,origin+new Vector3(0,.12f,-3.08f),
                2.65f,3.25f,Quaternion.identity,new Color(.58f,.58f,.56f,1f))==null)
                BenchmarkPieceTinted(name+" · connected gate fallback",megaGate,origin+new Vector3(0,.12f,-3.08f),
                    2.65f,3.25f,Quaternion.identity,WarmStone*.82f);
            foreach(float x in new[]{-2.9f,2.9f})
                if(BenchmarkPieceModulated(name+" · connected wall",stoneWall,origin+new Vector3(x,.12f,-3.02f),
                    3.55f,3.10f,Quaternion.identity,new Color(.58f,.58f,.56f,1f))==null)
                    Wall(name+" · connected wall fallback",origin+new Vector3(x,1.60f,-3.02f),
                        new Vector3(3.4f,2.7f,.62f),WarmStone*.72f,true);
            if(BenchmarkPieceModulated(name+" · connected tower west",megaTower,origin+new Vector3(-5.05f,.08f,-2.62f),
                2.95f,6.85f,Quaternion.Euler(0,-3f,0),new Color(.55f,.56f,.55f,1f))==null)
                Tower(name+" · connected tower west fallback",origin+new Vector3(-5.05f,.05f,-2.35f),1.30f,6.15f,WarmStone*.74f);
            if(BenchmarkPieceModulated(name+" · connected tower east",megaTower,origin+new Vector3(5.0f,.08f,-2.62f),
                2.55f,5.55f,Quaternion.Euler(0,4f,0),new Color(.52f,.54f,.53f,1f))==null)
                Tower(name+" · connected tower east fallback",origin+new Vector3(5.0f,.05f,-2.35f),1.15f,5.05f,WarmStone*.70f);
            if(BenchmarkPieceModulated(name+" · rear connected tower west",megaTower,origin+new Vector3(-4.65f,.08f,2.80f),
                2.48f,6.20f,Quaternion.Euler(0,8f,0),new Color(.49f,.51f,.50f,1f))==null)
                Tower(name+" · rear connected tower west fallback",origin+new Vector3(-4.65f,.05f,2.80f),1.12f,5.65f,OldStone*.72f);
            if(BenchmarkPieceModulated(name+" · rear connected tower east",megaTower,origin+new Vector3(4.20f,.08f,2.35f),
                2.08f,4.65f,Quaternion.Euler(0,-11f,0),new Color(.46f,.48f,.47f,1f))==null)
                Tower(name+" · rear connected tower east fallback",origin+new Vector3(4.20f,.05f,2.35f),.96f,4.35f,OldStone*.68f);

            // Keep: a small hidden backing volume surrounded by authored masonry on every visible side.
            Block(name+" · inner keep backing",origin+new Vector3(0,2.78f,1.15f),
                new Vector3(4.05f,.66f,2.45f),Stone*.36f);
            foreach(float x in new[]{-1.55f,1.55f})
                if(BenchmarkPieceModulated(name+" · keep detailed facing",stoneWall,
                    origin+new Vector3(x,2.42f,-1.28f),3.0f,2.62f,Quaternion.identity,new Color(.54f,.55f,.53f,1f))==null)
                    Wall(name+" · keep facing fallback",origin+new Vector3(x,3.10f,-1.26f),
                        new Vector3(2.85f,2.0f,.44f),OldStone*.64f,true);
            foreach(float x in new[]{-2.25f,2.25f})
                if(BenchmarkPieceModulated(name+" · keep side facing",stoneWall,
                    origin+new Vector3(x,2.32f,1.05f),2.75f,2.48f,Quaternion.Euler(0,90f,0),
                    new Color(.48f,.50f,.49f,1f))==null)
                    Wall(name+" · keep side fallback",origin+new Vector3(x,3.00f,1.05f),
                        new Vector3(.44f,1.95f,2.65f),OldStone*.60f,true);
            if(BenchmarkPieceModulated(name+" · keep rear facing",stoneWall,
                origin+new Vector3(0,2.28f,2.52f),4.35f,2.42f,Quaternion.Euler(0,180f,0),
                new Color(.45f,.47f,.46f,1f))==null)
                Wall(name+" · keep rear fallback",origin+new Vector3(0,2.95f,2.50f),
                    new Vector3(4.15f,1.90f,.42f),OldStone*.58f,true);
            GableRoof(name+" · inner keep west roof",origin+new Vector3(-1.15f,4.72f,1.02f),
                new Vector3(3.15f,1.36f,3.15f),Slate*.94f);
            GableRoof(name+" · inner keep east roof",origin+new Vector3(1.55f,4.24f,1.24f),
                new Vector3(2.30f,.92f,2.70f),Slate*.84f);
            Rubble(name+" · collapsed keep roof",origin+new Vector3(1.85f,3.70f,.10f),.58f,6);
            Buttress(name+" · buttress west",origin+new Vector3(-3.25f,.08f,-2.75f),3.45f,1.25f,WarmStone*.82f);
            Buttress(name+" · buttress east",origin+new Vector3(3.25f,.08f,-2.75f),3.45f,1.25f,WarmStone*.82f);

            foreach(float x in new[]{-1.85f,0f,1.85f})
                WindowSlit(name+" · keep slit",origin+new Vector3(x,4.0f,-1.78f),
                    new Vector3(.28f,.72f,.12f),x==0f);

            Banner(name+" · banner west",origin+new Vector3(-2.65f,3.55f,-1.82f),
                new Vector3(.62f,2.25f,.08f),new Color(.16f,.25f,.34f));
            Banner(name+" · banner east",origin+new Vector3(2.65f,3.55f,-1.82f),
                new Vector3(.62f,2.25f,.08f),new Color(.16f,.25f,.34f));

            // Dead-palace signature: use authored masonry fragments rather than a giant procedural arch.
            // The ruin should frame the keep and imply scale without becoming a black wall behind it.
            if(BenchmarkPieceModulated(name+" · dead palace wall west",megaWallPassage,
                origin+new Vector3(-4.15f,.10f,3.75f),4.20f,3.15f,Quaternion.Euler(0,16f,0),
                new Color(.47f,.49f,.48f,1f))==null)
                Wall(name+" · dead palace wall west fallback",origin+new Vector3(-4.15f,1.55f,3.75f),
                    new Vector3(3.8f,2.75f,.58f),OldStone*.58f,true);
            if(BenchmarkPieceModulated(name+" · dead palace wall east",stoneWall,
                origin+new Vector3(3.75f,.10f,4.20f),3.65f,2.85f,Quaternion.Euler(0,-18f,0),
                new Color(.44f,.46f,.45f,1f))==null)
                Wall(name+" · dead palace wall east fallback",origin+new Vector3(3.75f,1.35f,4.20f),
                    new Vector3(3.2f,2.35f,.54f),OldStone*.54f,true);
            if(BenchmarkPieceModulated(name+" · dead palace tower remnant",megaDestroyedTower,
                origin+new Vector3(-2.25f,-.18f,5.15f),2.35f,4.15f,Quaternion.Euler(0,12f,0),
                new Color(.54f,.55f,.53f,1f))==null)
                Tower(name+" · dead palace tower remnant fallback",origin+new Vector3(-2.10f,-.18f,5.35f),
                    .90f,3.55f,OldStone*.64f);
            Rubble(name+" · palace collapse west",origin+new Vector3(-5.0f,.18f,3.0f),1.45f,10);
            Rubble(name+" · palace collapse east",origin+new Vector3(4.6f,.18f,3.3f),1.25f,9);
            Scaffold(name+" · repair scaffold",origin+new Vector3(3.95f,2.35f,1.05f),
                new Vector3(1.55f,3.05f,1.35f));
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

        static GameObject Cone(string name,Vector3 center,float radius,float height,Color color)
        {
            const int sides=12;
            var vertices=new Vector3[sides+2];
            var triangles=new int[sides*6];
            vertices[0]=new Vector3(0,height*.5f,0);
            vertices[1]=new Vector3(0,-height*.5f,0);
            for(int i=0;i<sides;i++)
            {
                float a=i*Mathf.PI*2f/sides;
                vertices[i+2]=new Vector3(Mathf.Cos(a)*radius,-height*.5f,Mathf.Sin(a)*radius);
                int n=(i+1)%sides;
                int t=i*6;
                triangles[t]=0;triangles[t+1]=i+2;triangles[t+2]=n+2;
                triangles[t+3]=1;triangles[t+4]=n+2;triangles[t+5]=i+2;
            }
            var go=new GameObject(name);go.transform.position=center;
            var mesh=new Mesh{name=name+" mesh",vertices=vertices,triangles=triangles};
            mesh.RecalculateNormals();mesh.RecalculateBounds();
            go.AddComponent<MeshFilter>().sharedMesh=mesh;
            go.AddComponent<MeshRenderer>().sharedMaterial=Material(color);
            return go;
        }

        public static void PineTree(string name,Vector3 p,float scale)
        {
            Cylinder(name+" · trunk",p+Vector3.up*.9f*scale,new Vector3(.12f,.9f,.12f)*scale,
                new Color(.22f,.14f,.08f),Quaternion.identity);
            Cone(name+" · lower crown",p+Vector3.up*1.45f*scale,1.0f*scale,1.55f*scale,Pine*.88f);
            Cone(name+" · middle crown",p+Vector3.up*2.05f*scale,.78f*scale,1.35f*scale,Pine*.96f);
            Cone(name+" · upper crown",p+Vector3.up*2.55f*scale,.54f*scale,1.05f*scale,Pine*1.04f);
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
