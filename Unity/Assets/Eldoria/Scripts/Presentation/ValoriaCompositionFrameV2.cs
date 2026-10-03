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
            BuildUnifiedAuthoredTransition();
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
        static void BuildUnifiedAuthoredTransition()
        {
            // Materially distinct candidate: one camera-authored continuous Blender section owns
            // the geology, retaining bands, hero landing and civic circulation. Existing buildings
            // are embedded into that mass; no terrain board or scattered rock-support assembly.
            AddAuthoredTransition(
                "Valoria/ExperimentalAuthoredTransition/HeroCityTransitionEnvelopeV2",
                Vector3.zero,1f,1f,0f);

            // Presentation-only functional clones stay inside the dense lower frontage.
            PlaceExistingPresentation("Valoria · Strongest v2 · compact sawmill visual",
                new Vector3(-6.15f,.18f,-3.15f),3.45f,3.05f,10f);
            PlaceExistingPresentation("Valoria · Strongest v2 · compact barracks visual",
                new Vector3( 6.10f,.20f,-3.00f),3.55f,3.15f,350f);

            // Lower inhabited frontage.
            Add("Valoria/MidTierArchitectureKit_v1/Piece01","lower west frontage",new Vector3(-3.75f,.20f,-3.90f),2.70f,2.72f,15f);
            Add("Valoria/MidTierArchitectureKit_v1/Piece04","lower centre west",new Vector3(-1.30f,.24f,-4.10f),2.42f,2.48f,7f);
            Add("Valoria/Valoria_Granero_BIII_v1","civic granary",new Vector3(1.20f,.24f,-4.05f),2.72f,2.68f,352f);
            Add("Valoria/MidTierArchitectureKit_v1/Piece02","lower east frontage",new Vector3(3.90f,.20f,-3.80f),2.75f,2.78f,345f);

            // Middle inhabited band follows the authored retaining line.
            Add("Valoria/MidTierArchitectureKit_v1/Piece03","middle west house",new Vector3(-4.80f,1.18f,-.05f),2.48f,2.70f,18f);
            Add("Valoria/MidTierArchitectureKit_v1/Piece01","middle west inner",new Vector3(-2.40f,1.20f,.35f),2.28f,2.42f,10f);
            Add("Valoria/MidTierArchitectureKit_v1/Piece04","middle east inner",new Vector3(2.38f,1.20f,.40f),2.28f,2.42f,350f);
            Add("Valoria/MidTierArchitectureKit_v1/Piece02","middle east house",new Vector3(4.80f,1.18f,.02f),2.48f,2.70f,342f);

            // Upper shoulder architecture visually hands off into the Hero Bastion.
            Add("Valoria/MidTierArchitectureKit_v1/Piece02","upper west service",new Vector3(-3.65f,2.12f,3.80f),2.20f,2.42f,16f);
            Add("Valoria/MidTierArchitectureKit_v1/Piece03","upper east service",new Vector3(3.65f,2.12f,3.85f),2.20f,2.42f,344f);
        }

        static void AddAuthoredTransition(string path,Vector3 point,float span,float height,float yaw)
        {
            var src=Resources.Load<GameObject>(path);
            if(src==null)throw new Exception("Authored Hero-to-city transition missing: "+path);
            var go=Object.Instantiate(src);
            go.name="Valoria v2 · unified authored Hero-to-city transition";
            go.transform.SetParent(root,true);
            go.transform.localPosition=point;
            go.transform.localRotation=Quaternion.Euler(0,yaw,0);
            go.transform.localScale=Vector3.one;
            var rs=go.GetComponentsInChildren<Renderer>(true);
            if(rs.Length==0)throw new Exception("Authored Hero-to-city transition has no renderers.");
            foreach(var c in go.GetComponentsInChildren<Collider>(true))Object.DestroyImmediate(c);
            foreach(var h in go.GetComponentsInChildren<WorldHotspot>(true))Object.DestroyImmediate(h);
            foreach(var behaviour in go.GetComponentsInChildren<MonoBehaviour>(true))behaviour.enabled=false;

            foreach(var r in rs)
            {
                string n=HierarchyName(r.transform);
                bool groundBand=n.IndexOf("TerraceStone",StringComparison.OrdinalIgnoreCase)>=0;
                bool masonry=n.IndexOf("CivicSteps",StringComparison.OrdinalIgnoreCase)>=0||
                    n.IndexOf("RetainingMasonry",StringComparison.OrdinalIgnoreCase)>=0||
                    n.IndexOf("HeroLanding",StringComparison.OrdinalIgnoreCase)>=0;
                r.sharedMaterial=groundBand?ground:(masonry?stone:ground);
            }
            VisualPieces+=rs.Length;
        }

        static string HierarchyName(Transform t)
        {
            string s="";
            for(var p=t;p!=null;p=p.parent)s=p.name+"/"+s;
            return s;
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
