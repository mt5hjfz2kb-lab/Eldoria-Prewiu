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
            BuildArchitectureFirstTransition(state);
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
        static void BuildArchitectureFirstTransition(PlayerState state)
        {
            // Architecture-first candidate: compact inhabited bands carry the eye from city to Bastion.
            // Rock is only a buried/supporting mass; there are no exposed terrain boards.
            // Distinct method: Blender fuses certified rock/terrace support forms into one manifold city foundation.
            ValoriaFusedTransitionV2.Build(root,state);
            VisualPieces+=ValoriaFusedTransitionV2.RenderersBuilt;

            // Bring the gate's visual-only functional clones into the city section.
            PlaceExistingPresentation("Valoria · Strongest v2 · compact sawmill visual",
                new Vector3(-6.35f,.22f,-3.05f),3.55f,3.15f,10f);
            PlaceExistingPresentation("Valoria · Strongest v2 · compact barracks visual",
                new Vector3( 6.35f,.24f,-2.85f),3.65f,3.25f,350f);

            // Lower inhabited frontage: dense, overlapping roofline rather than isolated plots.
            Add("Valoria/MidTierArchitectureKit_v1/Piece01","lower west frontage",new Vector3(-3.85f,.22f,-3.75f),2.85f,2.80f,15f);
            Add("Valoria/MidTierArchitectureKit_v1/Piece04","lower centre west",new Vector3(-1.25f,.24f,-4.25f),2.55f,2.55f,7f);
            Add("Valoria/Valoria_Granero_BIII_v1","civic granary",new Vector3(1.15f,.24f,-4.15f),2.85f,2.75f,352f);
            Add("Valoria/MidTierArchitectureKit_v1/Piece02","lower east frontage",new Vector3(3.95f,.22f,-3.65f),2.90f,2.85f,345f);

            // Middle district presses into the retaining line and visually connects to the Hero District.
            Add("Valoria/MidTierArchitectureKit_v1/Piece03","middle west house",new Vector3(-5.05f,1.18f,.25f),2.65f,2.85f,18f);
            Add("Valoria/MidTierArchitectureKit_v1/Piece01","middle west inner",new Vector3(-2.55f,1.20f,.65f),2.45f,2.55f,10f);
            Add("Valoria/MidTierArchitectureKit_v1/Piece04","middle east inner",new Vector3(2.50f,1.20f,.72f),2.45f,2.55f,350f);
            Add("Valoria/MidTierArchitectureKit_v1/Piece02","middle east house",new Vector3(5.05f,1.18f,.32f),2.65f,2.85f,342f);

            // Upper shoulders close the last gap into the Bastion's authored rock skirt.
            Add("Valoria/MidTierArchitectureKit_v1/Piece02","upper west service",new Vector3(-4.05f,2.02f,4.15f),2.35f,2.55f,16f);
            Add("Valoria/MidTierArchitectureKit_v1/Piece03","upper east service",new Vector3(4.05f,2.02f,4.20f),2.35f,2.55f,344f);

            // Continuous masonry bands stitch buildings and geology into one city section.
            Add("Valoria/StoneArchitectureKit_v1/CornerWallL","lower west city corner",new Vector3(-7.55f,-.02f,-1.25f),2.65f,2.25f,92f);
            Add("Valoria/StoneArchitectureKit_v1/HighStraightWall","lower west city wall",new Vector3(-4.75f,.02f,-1.55f),3.85f,2.10f,4f);
            Add("Valoria/StoneArchitectureKit_v1/HighStraightWall","lower east city wall",new Vector3(4.75f,.02f,-1.50f),3.85f,2.10f,176f);
            Add("Valoria/StoneArchitectureKit_v1/CornerWallL","lower east city corner",new Vector3(7.55f,-.02f,-1.20f),2.65f,2.25f,268f);
            Add("Valoria/StoneArchitectureKit_v1/RockToWallTransition","middle west join",new Vector3(-5.55f,1.00f,2.55f),3.05f,2.30f,30f);
            Add("Valoria/StoneArchitectureKit_v1/RockToWallTransition","middle east join",new Vector3(5.55f,1.00f,2.60f),3.05f,2.30f,210f);
            Add("Valoria/StoneArchitectureKit_v1/HighStraightWall","upper west wall",new Vector3(-3.15f,1.75f,4.95f),2.95f,2.10f,8f);
            Add("Valoria/StoneArchitectureKit_v1/HighStraightWall","upper east wall",new Vector3(3.15f,1.75f,5.00f),2.95f,2.10f,172f);
        }

        static void PlaceExistingPresentation(string name,Vector3 point,float span,float height,float yaw)
        {
            var go=GameObject.Find(name);if(go==null)throw new Exception("Presentation clone missing: "+name);
            go.transform.rotation=Quaternion.Euler(0,yaw,0);
            var rs=go.GetComponentsInChildren<Renderer>(true);if(rs.Length==0)throw new Exception("Presentation clone has no renderer: "+name);
            var b=rs[0].bounds;for(int i=1;i<rs.Length;i++)b.Encapsulate(rs[i].bounds);
            go.transform.localScale*=Mathf.Min(span/Mathf.Max(b.size.x,b.size.z),height/Mathf.Max(.01f,b.size.y));
            rs=go.GetComponentsInChildren<Renderer>(true);b=rs[0].bounds;for(int i=1;i<rs.Length;i++)b.Encapsulate(rs[i].bounds);
            go.transform.position+=point-new Vector3(b.center.x,b.min.y,b.center.z);
        }

        static void AddSupport(string path,string name,Vector3 point,float span,float height,float yaw)
        {
            var src=Resources.Load<GameObject>(path);if(src==null)throw new Exception("Certified support source missing: "+path);
            var go=Object.Instantiate(src);go.name="Valoria v2 · "+name;go.transform.rotation=Quaternion.Euler(0,yaw,0);
            var rs=go.GetComponentsInChildren<Renderer>(true);if(rs.Length==0)throw new Exception("Certified support renderer missing: "+path);
            var b=rs[0].bounds;for(int i=1;i<rs.Length;i++)b.Encapsulate(rs[i].bounds);
            go.transform.localScale*=Mathf.Min(span/Mathf.Max(b.size.x,b.size.z),height/Mathf.Max(.01f,b.size.y));
            rs=go.GetComponentsInChildren<Renderer>(true);b=rs[0].bounds;for(int i=1;i<rs.Length;i++)b.Encapsulate(rs[i].bounds);
            go.transform.position+=point-new Vector3(b.center.x,b.min.y,b.center.z);
            go.transform.SetParent(root,true);
            foreach(var c in go.GetComponentsInChildren<Collider>(true))Object.DestroyImmediate(c);
            foreach(var h in go.GetComponentsInChildren<WorldHotspot>(true))Object.DestroyImmediate(h);
            foreach(var behaviour in go.GetComponentsInChildren<MonoBehaviour>(true))behaviour.enabled=false;
            // Lane B / Surface v1: rescued support geometry is certified but its flat source material is not.
            // Apply the deterministic Valoria rock profile already created at Build() startup.
            foreach(var r in rs)if(rock!=null)r.sharedMaterial=rock;
            VisualPieces++;
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
