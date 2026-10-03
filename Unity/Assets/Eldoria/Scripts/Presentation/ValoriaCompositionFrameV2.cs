using System;
using Eldoria.Domain;
using UnityEngine;
using UnityEngine.Rendering;
using Object=UnityEngine.Object;

namespace Eldoria.Presentation
{
    // Experimental presentation only. All functional objects and camera contracts stay put.
    public static class ValoriaCompositionFrameV2
    {
        public static int VisualPieces{get;private set;}
        public static int RestoredFunctionalRenderers{get;private set;}
        static Transform root;
        static Material ground,rock,stone;

        public static void Build(Transform parent,PlayerState state)
        {
            if(parent==null||state==null)throw new ArgumentNullException();
            var old=GameObject.Find("Valoria · Composition Frame v2");
            if(old!=null)Object.DestroyImmediate(old);
            root=new GameObject("Valoria · Composition Frame v2").transform;
            root.SetParent(parent,true);
            VisualPieces=0;RestoredFunctionalRenderers=0;
            var shader=Shader.Find("Eldoria/ValoriaCompositionGround");
            if(shader==null)throw new Exception("Composition ground shader missing.");
            ground=new Material(shader){name="Valoria v2 · blended rock and earth"};
            foreach(var pair in new[]{("_RockTex","rock_diff"),("_GroundTex","dirt_diff"),("_RockNormal","rock_normal")})
            {
                var tex=Resources.Load<Texture2D>("Valoria/SurfaceCellExternal/"+pair.Item2);
                if(tex==null)throw new Exception("Shared surface missing: "+pair.Item2);
                tex.wrapMode=TextureWrapMode.Repeat;
                tex.anisoLevel=8;
                ground.SetTexture(pair.Item1,tex);
            }
            rock=ValoriaKit.ExternalPbrSurfaceMaterial("rock",new Color(.76f,.75f,.69f),new Vector2(.52f,.52f),.035f,.95f);
            stone=ValoriaKit.ExternalPbrSurfaceMaterial("stone",new Color(.77f,.75f,.68f),new Vector2(.54f,.54f),.035f,.92f);
            if(ground==null||rock==null||stone==null)throw new Exception("Composition frame shared PBR maps missing.");
            BuildCliffCitadelMass();
            RestoreFunctional("Aserradero · dedicated sawmill");
            RestoreFunctional("Cuartel · dedicated barracks");
            Add("Valoria/Valoria_Granero_BIII_v1","civic granary",new Vector3(-2.6f,.34f,-6.1f),3.3f,2.7f,16f);
            BuildArchitecturalRetaining();
            BuildEmbeddedDistrict();
            BuildRoute();
            RenderSettings.fog=true;
            RenderSettings.fogMode=FogMode.Linear;
            RenderSettings.fogColor=new Color(.55f,.61f,.68f);
            RenderSettings.fogStartDistance=38f;
            RenderSettings.fogEndDistance=95f;
        }

        static float Smooth(float a,float b,float x)
        {float t=Mathf.Clamp01((x-a)/(b-a));return t*t*(3f-2f*t);}
        static float Seat(float x,float z,float cx,float cz,float rx,float rz,float edge)
        {
            float r=Mathf.Sqrt(Mathf.Pow((x-cx)/rx,2)+Mathf.Pow((z-cz)/rz,2));
            return 1f-Smooth(1f-edge,1f+edge,r);
        }
        static float Height(float x,float z)
        {
            float macro=(Mathf.PerlinNoise(x*.18f+20f,z*.16f+31f)-.5f)*.46f;
            float micro=(Mathf.PerlinNoise(x*.91f+41f,z*.78f+18f)-.5f)*.12f;
            float lower=Seat(x,z,0,-2.8f,10.7f,8.3f,.29f);
            float y=Mathf.Lerp(-2.2f+macro,.35f+micro,lower);
            float mid=Seat(x,z,0,3.5f,8.35f,5.15f,.15f);
            y=Mathf.Lerp(y,1.18f+micro*.45f,mid);
            float upper=Seat(x,z,0,8.75f,7.3f,7.6f,.18f);
            y=Mathf.Lerp(y,2.46f+micro*.55f,upper);
            // Walkway rises from the civic terrace into the original Bastion stair.
            float route=1f-Smooth(1.15f,2f,Mathf.Abs(x));
            float segment=Smooth(-1f,.2f,z)*(1f-Smooth(5.8f,6.5f,z));
            y=Mathf.Lerp(y,Mathf.Lerp(.35f,2.35f,Smooth(-.3f,5.9f,z))-.13f,route*segment*.8f);
            return y;
        }
        static void BuildCliffCitadelMass()
        {
            // Distinct method: overlapping authored environment meshes form one geological city section.
            // No generated platform/deck geometry is used in this candidate.
            AddEnvironment("Valoria/SM_Cliffs_03","front west bedrock",new Vector3(-6.7f,-2.75f,-6.35f),11.4f,4.8f,18f);
            AddEnvironment("Valoria/SM_Cliffs_01","front east bedrock",new Vector3( 6.5f,-2.65f,-6.15f),11.0f,4.7f,198f);
            AddEnvironment("Valoria/SM_Cliffs_01","lower west rise",new Vector3(-7.2f,-2.05f,-1.85f),10.0f,4.9f,42f);
            AddEnvironment("Valoria/SM_Cliffs_03","lower east rise",new Vector3( 7.0f,-2.00f,-1.65f),10.2f,5.0f,222f);
            AddEnvironment("Valoria/SM_Cliffs_03","middle west rise",new Vector3(-6.3f,-1.15f,2.55f),9.0f,4.75f,68f);
            AddEnvironment("Valoria/SM_Cliffs_01","middle east rise",new Vector3( 6.15f,-1.10f,2.75f),9.2f,4.8f,248f);
            AddEnvironment("Valoria/SM_Cliffs_01","hero west buttress",new Vector3(-5.1f,-.35f,5.65f),7.5f,4.25f,92f);
            AddEnvironment("Valoria/SM_Cliffs_03","hero east buttress",new Vector3( 5.0f,-.30f,5.80f),7.6f,4.30f,272f);

            // Buried terrain meshes only close unavoidable gaps; they never become visible rectangular shelves.
            AddEnvironment("Valoria/SM_Terrain_03","buried lower ground",new Vector3(0f,-1.35f,-3.8f),14.0f,1.15f,9f);
            AddEnvironment("Valoria/SM_Terrain_03","buried middle ground",new Vector3(0f,-.52f,1.15f),11.8f,1.05f,191f);
        }
        static void AddEnvironment(string path,string name,Vector3 point,float span,float height,float yaw)
        {
            var src=Resources.Load<GameObject>(path);if(src==null)throw new Exception("Environment source missing: "+path);
            var go=Object.Instantiate(src);go.name="Valoria v2 · "+name;go.transform.rotation=Quaternion.Euler(0,yaw,0);
            var rs=go.GetComponentsInChildren<Renderer>(true);if(rs.Length==0)throw new Exception("Environment renderer missing: "+path);
            var b=rs[0].bounds;for(int i=1;i<rs.Length;i++)b.Encapsulate(rs[i].bounds);
            go.transform.localScale*=Mathf.Min(span/Mathf.Max(b.size.x,b.size.z),height/Mathf.Max(.01f,b.size.y));
            rs=go.GetComponentsInChildren<Renderer>(true);b=rs[0].bounds;for(int i=1;i<rs.Length;i++)b.Encapsulate(rs[i].bounds);
            go.transform.position+=point-new Vector3(b.center.x,b.min.y,b.center.z);
            go.transform.SetParent(root,true);
            foreach(var c in go.GetComponentsInChildren<Collider>(true))Object.DestroyImmediate(c);
            foreach(var h in go.GetComponentsInChildren<WorldHotspot>(true))Object.DestroyImmediate(h);
            foreach(var behaviour in go.GetComponentsInChildren<MonoBehaviour>(true))behaviour.enabled=false;
            foreach(var r in rs)r.sharedMaterial=rock;
            VisualPieces++;
        }
        static void RestoreFunctional(string name)
        {
            var go=GameObject.Find(name);
            if(go==null)throw new Exception("Functional source missing: "+name);
            foreach(var r in go.GetComponentsInChildren<Renderer>(true))
            {if(!r.enabled){r.enabled=true;RestoredFunctionalRenderers++;}}
        }
        static void BuildArchitecturalRetaining()
        {
            // Real architectural modules stitch the rock bands; no procedural arcade slabs.
            Add("Valoria/StoneArchitectureKit_v1/HighStraightWall","lower west retaining",new Vector3(-5.45f,-.05f,-2.35f),4.0f,2.0f,6f);
            Add("Valoria/StoneArchitectureKit_v1/HighStraightWall","lower east retaining",new Vector3( 5.35f,-.02f,-2.25f),4.0f,2.0f,174f);
            Add("Valoria/StoneArchitectureKit_v1/CornerWallL","lower west corner",new Vector3(-8.25f,-.10f,-1.15f),2.8f,2.25f,92f);
            Add("Valoria/StoneArchitectureKit_v1/CornerWallL","lower east corner",new Vector3( 8.15f,-.08f,-1.05f),2.8f,2.25f,268f);
            Add("Valoria/StoneArchitectureKit_v1/RockToWallTransition","middle west rock join",new Vector3(-5.9f,.95f,2.15f),3.2f,2.25f,28f);
            Add("Valoria/StoneArchitectureKit_v1/RockToWallTransition","middle east rock join",new Vector3( 5.85f,.97f,2.20f),3.2f,2.25f,208f);
            Add("Valoria/StoneArchitectureKit_v1/HighStraightWall","hero west retaining",new Vector3(-4.65f,1.78f,5.05f),3.15f,2.05f,8f);
            Add("Valoria/StoneArchitectureKit_v1/HighStraightWall","hero east retaining",new Vector3( 4.60f,1.80f,5.10f),3.15f,2.05f,172f);
            Add("Valoria/StoneArchitectureKit_v1/RockToWallTransition","west bastion join",new Vector3(-6.35f,1.45f,5.45f),2.65f,2.30f,36f);
            Add("Valoria/StoneArchitectureKit_v1/RockToWallTransition","east bastion join",new Vector3( 6.35f,1.45f,5.45f),2.65f,2.30f,216f);
        }
        static void BuildEmbeddedDistrict()
        {
            // Architecture is embedded in the retaining sequence so each terrace reads as a district, not an empty pad.
            Add("Valoria/MidTierArchitectureKit_v1/Piece01","lower west workshop row",new Vector3(-6.85f,.32f,-4.85f),2.35f,2.35f,18f);
            Add("Valoria/MidTierArchitectureKit_v1/Piece02","lower east military row",new Vector3(6.55f,.32f,-4.55f),2.45f,2.45f,342f);
            Add("Valoria/MidTierArchitectureKit_v1/Piece04","lower civic edge",new Vector3(2.85f,.34f,-5.35f),2.15f,2.20f,10f);
            Add("Valoria/MidTierArchitectureKit_v1/Piece03","middle west housing",new Vector3(-6.15f,1.31f,.65f),2.35f,2.55f,14f);
            Add("Valoria/MidTierArchitectureKit_v1/Piece01","middle east housing",new Vector3(6.05f,1.31f,.90f),2.20f,2.40f,346f);
            Add("Valoria/MidTierArchitectureKit_v1/Piece02","upper west service",new Vector3(-5.25f,2.31f,4.95f),2.05f,2.35f,16f);
            Add("Valoria/MidTierArchitectureKit_v1/Piece04","upper east service",new Vector3(5.15f,2.31f,5.05f),2.05f,2.35f,344f);
            Add("Valoria/StoneArchitectureKit_v1/CornerWallL","west terrace corner",new Vector3(-8.05f,.35f,-1.15f),2.2f,1.85f,92f);
            Add("Valoria/StoneArchitectureKit_v1/CornerWallL","east terrace corner",new Vector3(8.05f,.35f,-1.15f),2.2f,1.85f,268f);
        }
        static void BuildRoute()
        {
            const int n=32;var v=new Vector3[n*2];var uv=new Vector2[n*2];var tris=new int[(n-1)*6];
            for(int i=0;i<n;i++)
            {
                float z=Mathf.Lerp(-5.4f,5.55f,i/(float)(n-1));
                float h=z<-1.15f?.38f:(z<3.95f?1.34f:2.34f);
                float blend=.22f*Mathf.Sin((z+5.4f)*.62f);
                v[i*2]=new Vector3(-.92f,h+blend*.08f,z);v[i*2+1]=new Vector3(.92f,h+blend*.08f,z);
                uv[i*2]=new Vector2(-.45f,z*.42f);uv[i*2+1]=new Vector2(.45f,z*.42f);
            }
            int t=0;for(int i=0;i<n-1;i++)
            {int k=i*2;tris[t++]=k;tris[t++]=k+2;tris[t++]=k+1;tris[t++]=k+1;tris[t++]=k+2;tris[t++]=k+3;}
            var mesh=new Mesh{name="Valoria v2 · civic stone route"};mesh.vertices=v;mesh.uv=uv;mesh.triangles=tris;mesh.RecalculateNormals();
            var go=new GameObject(mesh.name);go.transform.SetParent(root,true);
            go.AddComponent<MeshFilter>().sharedMesh=mesh;go.AddComponent<MeshRenderer>().sharedMaterial=stone;
            VisualPieces++;
        }
        static void Add(string path,string name,Vector3 point,float span,float height,float yaw)
        {
            var src=Resources.Load<GameObject>(path);if(src==null)throw new Exception("Composition asset missing: "+path);
            var go=Object.Instantiate(src);go.name="Valoria v2 · "+name;go.transform.rotation=Quaternion.Euler(0,yaw,0);
            var rs=go.GetComponentsInChildren<Renderer>(true);if(rs.Length==0)throw new Exception("Asset renderer missing: "+path);
            var b=rs[0].bounds;for(int i=1;i<rs.Length;i++)b.Encapsulate(rs[i].bounds);
            go.transform.localScale*=Mathf.Min(span/Mathf.Max(b.size.x,b.size.z),height/b.size.y);
            rs=go.GetComponentsInChildren<Renderer>(true);b=rs[0].bounds;
            for(int i=1;i<rs.Length;i++)b.Encapsulate(rs[i].bounds);
            go.transform.position+=point-new Vector3(b.center.x,b.min.y,b.center.z);
            go.transform.SetParent(root,true);
            foreach(var c in go.GetComponentsInChildren<Collider>(true))Object.DestroyImmediate(c);
            foreach(var h in go.GetComponentsInChildren<WorldHotspot>(true))Object.DestroyImmediate(h);
            foreach(var behaviour in go.GetComponentsInChildren<MonoBehaviour>(true))behaviour.enabled=false;
            VisualPieces++;
        }
    }
}
