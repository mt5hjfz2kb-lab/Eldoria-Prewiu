using System;
using Eldoria.Domain;
using UnityEngine;
using UnityEngine.Rendering;
using Object=UnityEngine.Object;

namespace Eldoria.Presentation
{
    // Experiment only: presentation owns no collider, hotspot or progression state.
    // A single authored height field carries all settlement and surrounding geology.
    public static class ValoriaVisualShellV2
    {
        public static int HiddenRenderers,VisualPieces;
        static Transform root;
        static Material terrain,stone;
        public static void Build(Transform parent,PlayerState state)
        {
            if(parent==null||state==null)throw new ArgumentNullException();
            HiddenRenderers=0;VisualPieces=0;
            foreach(var r in Object.FindObjectsByType<Renderer>(FindObjectsSortMode.None))
            {
                string n=Chain(r.transform);
                bool keep=n.Contains("certified hero bastion v1")||n.Contains("mountain backplate");
                if(!keep&&r.enabled){r.enabled=false;HiddenRenderers++;}
            }
            foreach(var t in Object.FindObjectsByType<Terrain>(FindObjectsSortMode.None))
            {t.drawHeightmap=false;t.drawTreesAndFoliage=false;}
            // The original functional art remains at its authoritative plot coordinates.
            Restore("Aserradero · dedicated sawmill");
            Restore("Cuartel · dedicated barracks");
            root=new GameObject("Valoria · Visual Shell v2 · visual only").transform;
            root.SetParent(parent,true);
            terrain=new Material(Shader.Find("Eldoria/ValoriaShellTerrain"));
            terrain.name="Valoria v2 · shared geological PBR";
            foreach(var pair in new[]{("_RockTex","rock_diff"),("_GroundTex","dirt_diff"),("_RockNormal","rock_normal")})
            {
                var tex=Resources.Load<Texture2D>("Valoria/SurfaceCellExternal/"+pair.Item2);
                if(tex==null)throw new Exception("Shell surface missing: "+pair.Item2);
                tex.wrapMode=TextureWrapMode.Repeat;tex.anisoLevel=8;
                terrain.SetTexture(pair.Item1,tex);
            }
            stone=ValoriaKit.ExternalPbrSurfaceMaterial("stone",new Color(.68f,.66f,.60f),new Vector2(.7f,.7f),.035f,.8f);
            BuildLandform();
            BuildArchitecture();
            BuildCirculation();
            Lighting();
        }
        static void Restore(string name)
        {
            var go=GameObject.Find(name);if(go==null)throw new Exception("Functional source missing: "+name);
            foreach(var r in go.GetComponentsInChildren<Renderer>(true))r.enabled=true;
        }
        static string Chain(Transform t)
        {string s="";for(;t!=null;t=t.parent)s+="|"+t.name.ToLowerInvariant();return s;}
        static float Disc(float x,float z,float cx,float cz,float rx,float rz,float edge)
        {
            float q=Mathf.Sqrt(Mathf.Pow((x-cx)/rx,2)+Mathf.Pow((z-cz)/rz,2));
            return 1f-Mathf.SmoothStep(0f,1f,Mathf.InverseLerp(1f-edge,1f+edge,q));
        }
        public static float Height(float x,float z)
        {
            float noise=(Mathf.PerlinNoise(x*.15f+12.1f,z*.15f+32.3f)-.5f);
            float basin=-5.3f+noise*.85f;
            // Lower civic terrace unifies workshop, military plot and the central street.
            float lower=Disc(x+noise*.35f,z,0,-1.4f,12.8f,10f,.07f);
            float y=Mathf.Lerp(basin,.27f+noise*.12f,lower);
            // Upper rock mass absorbs the hero's authored 2.52m foundation; no detached platform.
            float upper=Disc(x+noise*.32f,z,0,8.8f,7.4f,7.5f,.055f);
            y=Mathf.Lerp(y,2.53f+noise*.1f,upper);
            // One asymmetrical connected ridge, rather than two separate prefab mountains.
            float rear=Disc(x,z,-5.5f,22.5f,17f,11f,.22f);
            float crest=4.8f+Mathf.PerlinNoise(x*.085f+43f,z*.1f+27f)*3.5f;
            y=Mathf.Max(y,Mathf.Lerp(basin,crest,rear));
            float west=Disc(x,z,-19,7,9,19,.14f);
            float east=Disc(x,z,21,10,9,20,.15f);
            y=Mathf.Max(y,Mathf.Lerp(basin,3f+noise*1.8f,west));
            y=Mathf.Max(y,Mathf.Lerp(basin,4f+noise*2f,east));
            // Exact visual seats at existing gameplay parcels, not relocated targets.
            y=Mathf.Lerp(y,.32f,Disc(x,z,-7,-2.8f,3.25f,2.7f,.15f));
            y=Mathf.Lerp(y,.32f,Disc(x,z,7,-4,3.4f,2.8f,.15f));
            // The certified stair rises between the two levels along the central spine.
            if(Mathf.Abs(x)<1.65f&&z>-.2f&&z<5.6f)
                y=Mathf.Lerp(.27f,2.5f,Mathf.InverseLerp(0,5.55f,z))-.10f;
            return y;
        }
        static void BuildLandform()
        {
            const int n=257;var v=new Vector3[n*n];var uv=new Vector2[v.Length];var tris=new int[(n-1)*(n-1)*6];
            for(int j=0;j<n;j++)for(int i=0;i<n;i++)
            {float x=Mathf.Lerp(-65,65,i/(float)(n-1)),z=Mathf.Lerp(-55,80,j/(float)(n-1));int k=j*n+i;v[k]=new Vector3(x,Height(x,z),z);uv[k]=new Vector2(x,z);}
            int p=0;for(int j=0;j<n-1;j++)for(int i=0;i<n-1;i++)
            {int a=j*n+i,b=a+1,d=a+n,e=d+1;tris[p++]=a;tris[p++]=d;tris[p++]=b;tris[p++]=b;tris[p++]=d;tris[p++]=e;}
            var m=new Mesh{name="Valoria v2 · continuous authored landform",indexFormat=IndexFormat.UInt32};
            m.vertices=v;m.uv=uv;m.triangles=tris;m.RecalculateNormals();m.RecalculateBounds();
            var go=new GameObject(m.name);go.transform.SetParent(root,true);go.AddComponent<MeshFilter>().sharedMesh=m;
            go.AddComponent<MeshRenderer>().sharedMaterial=terrain;VisualPieces++;
        }
        static void BuildArchitecture()
        {
            // Retaining masonry belongs to the landform; foundation is buried into its cliff face.
            Add("Valoria/StoneArchitectureKit_v1/RockToWallTransition","west geological masonry",new Vector3(-5.8f,1.1f,4.3f),4.5f,2.5f,28f);
            Add("Valoria/StoneArchitectureKit_v1/RockToWallTransition","east geological masonry",new Vector3(5.8f,1.1f,4.3f),4.5f,2.5f,208f);
            Add("Valoria/MidTierArchitectureKit_v1/Piece03","upper civic service",new Vector3(-5.4f,2.45f,8f),2.5f,2.4f,10f);
            Add("Valoria/Valoria_Granero_BIII_v1","granary at lower civic plot",new Vector3(-2.5f,.28f,-5.2f),2.5f,2.5f,10f);
            // Compact archive/workshop wing bridges hero scale without residential multiplication.
            Add("Valoria/MidTierArchitectureKit_v1/Piece01","service entry",new Vector3(-4.8f,2.45f,6.5f),1.6f,1.7f,8f);
            var art=ValoriaExternalAssetLibrary.Load();
            if(art!=null)
            {
                // One broken ancient arch is the signature frame. No paired competing castles.
                Place(art.MegaHalfGate,"ancient western arch",new Vector3(-10.5f,Height(-10.5f,9)-.7f,9f),7.2f,8.5f,8f);
                // Trees are limited to geology joints and depth cues; this is not density filling.
                foreach(var p in new[]{new Vector2(-11,1),new Vector2(-10,6),new Vector2(11,0),new Vector2(12,6),new Vector2(-16,13),new Vector2(16,15)})
                    Place(art.SlavicTreeTall??art.SlavicTree,"ridge vegetation",new Vector3(p.x,Height(p.x,p.y)-.12f,p.y),1.6f,3.4f,p.x*19);
                Place(art.Firewood,"sawmill work material",new Vector3(-5.2f,.35f,-4.5f),1.2f,.6f,0);
            }
        }
        static void BuildCirculation()
        {
            for(int i=0;i<12;i++)
            {
                float z=.05f+i*.46f,y=.38f+i*.185f;
                Block("upper route step",new Vector3(0,y-.13f,z),new Vector3(2.8f,.27f,.55f));
            }
            // Broad civic route with burial at the boundaries instead of floating slab rims.
            for(int i=0;i<10;i++)Block("civic route",new Vector3(0,.28f,-5.4f+i*.53f),new Vector3(2.9f,.10f,.60f));
        }
        static void Block(string role,Vector3 p,Vector3 scale)
        {
            var go=GameObject.CreatePrimitive(PrimitiveType.Cube);go.name="Valoria v2 · "+role;go.transform.SetParent(root,true);
            go.transform.position=p;go.transform.localScale=scale;Object.DestroyImmediate(go.GetComponent<Collider>());
            go.GetComponent<Renderer>().sharedMaterial=stone;VisualPieces++;
        }
        static void Add(string path,string role,Vector3 p,float span,float height,float yaw)
        {var src=Resources.Load<GameObject>(path);if(src==null)throw new Exception("Required shell asset absent: "+path);Place(src,role,p,span,height,yaw);}
        static void Place(GameObject src,string role,Vector3 p,float span,float height,float yaw)
        {
            if(src==null)return;var go=Object.Instantiate(src);go.name="Valoria v2 · "+role;go.transform.rotation=Quaternion.Euler(0,yaw,0);
            var rs=go.GetComponentsInChildren<Renderer>(true);if(rs.Length==0){Object.DestroyImmediate(go);return;}
            Bounds b=rs[0].bounds;for(int i=1;i<rs.Length;i++)b.Encapsulate(rs[i].bounds);
            go.transform.localScale*=Mathf.Min(span/Mathf.Max(b.size.x,b.size.z),height/b.size.y);
            rs=go.GetComponentsInChildren<Renderer>(true);b=rs[0].bounds;for(int i=1;i<rs.Length;i++)b.Encapsulate(rs[i].bounds);
            go.transform.position+=p-new Vector3(b.center.x,b.min.y,b.center.z);go.transform.SetParent(root,true);
            foreach(var c in go.GetComponentsInChildren<Collider>(true))Object.DestroyImmediate(c);
            foreach(var h in go.GetComponentsInChildren<WorldHotspot>(true))Object.DestroyImmediate(h);
            foreach(var a in go.GetComponentsInChildren<MonoBehaviour>(true))a.enabled=false;
            foreach(var r in rs)
            {
                var mm=r.sharedMaterials;
                for(int i=0;i<mm.Length;i++)
                {
                    var old=mm[i];if(old==null)continue;
                    // Preserve authored PBR if it is already URP compatible.
                    if(old.shader!=null&&(old.shader.name.Contains("Universal")||old.shader.name.Contains("glTF")))continue;
                    var m=new Material(Shader.Find("Universal Render Pipeline/Lit"));
                    Texture tex=old.HasProperty("_BaseMap")?old.GetTexture("_BaseMap"):old.HasProperty("_MainTex")?old.GetTexture("_MainTex"):null;
                    m.SetTexture("_BaseMap",tex);m.SetColor("_BaseColor",new Color(.70f,.69f,.63f));m.SetFloat("_Smoothness",.04f);
                    if(old.HasProperty("_BumpMap")&&old.GetTexture("_BumpMap")!=null){m.SetTexture("_BumpMap",old.GetTexture("_BumpMap"));m.EnableKeyword("_NORMALMAP");}
                    mm[i]=m;
                }
                r.sharedMaterials=mm;
            }
            VisualPieces++;
        }
        static void Lighting()
        {
            foreach(var l in Object.FindObjectsByType<Light>(FindObjectsSortMode.None))
            {
                if(l.type==LightType.Directional){l.transform.rotation=Quaternion.Euler(48,-32,0);l.color=new Color(1f,.91f,.79f);l.intensity=1.15f;l.shadowStrength=.72f;l.shadowBias=.035f;}
                else l.intensity=Mathf.Min(l.intensity,.70f);
            }
            RenderSettings.ambientMode=AmbientMode.Trilight;
            RenderSettings.ambientSkyColor=new Color(.39f,.48f,.59f);
            RenderSettings.ambientEquatorColor=new Color(.34f,.37f,.39f);
            RenderSettings.ambientGroundColor=new Color(.17f,.18f,.17f);
            RenderSettings.fog=true;RenderSettings.fogMode=FogMode.Linear;
            RenderSettings.fogColor=new Color(.51f,.61f,.70f);RenderSettings.fogStartDistance=42;RenderSettings.fogEndDistance=105;
            QualitySettings.shadowDistance=90;QualitySettings.shadows=ShadowQuality.All;QualitySettings.shadowResolution=ShadowResolution.High;
        }
    }
}
