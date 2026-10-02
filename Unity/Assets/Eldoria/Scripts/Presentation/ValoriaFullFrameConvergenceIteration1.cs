using Eldoria.Domain;
using UnityEngine;
using UnityEngine.Rendering;
using Object = UnityEngine.Object;

namespace Eldoria.Presentation
{
    // Full-frame visual batch. Visual-only: no gameplay topology, colliders, hotspots or circulation ownership.
    public static class ValoriaFullFrameConvergenceIteration1
    {
        public static bool Enabled=false;
        const string RootName="Valoria · Full Frame Convergence · Iteration 1";

        public static void Build(Transform parent,PlayerState state)
        {
            if(!Enabled||parent==null)return;
            var old=GameObject.Find(RootName);
            if(old!=null)Object.DestroyImmediate(old);

            var root=new GameObject(RootName).transform;
            root.SetParent(parent,true);

            ApplyAtmosphere(root);
            BuildDistantWorld(root);
            BuildCorruptionThreat(root);
            BuildWarmOccupation(root);
        }

        static void ApplyAtmosphere(Transform root)
        {
            RenderSettings.ambientMode=AmbientMode.Trilight;
            RenderSettings.ambientSkyColor=new Color(.66f,.75f,.82f);
            RenderSettings.ambientEquatorColor=new Color(.47f,.49f,.46f);
            RenderSettings.ambientGroundColor=new Color(.23f,.22f,.20f);
            RenderSettings.ambientIntensity=.92f;
            RenderSettings.fog=true;
            RenderSettings.fogMode=FogMode.Linear;
            RenderSettings.fogColor=new Color(.59f,.68f,.74f);
            RenderSettings.fogStartDistance=31f;
            RenderSettings.fogEndDistance=92f;

            var camera=Camera.main;
            if(camera!=null)
            {
                camera.clearFlags=CameraClearFlags.SolidColor;
                camera.backgroundColor=new Color(.47f,.66f,.78f);
                camera.allowHDR=true;
            }

            foreach(var light in Object.FindObjectsByType<Light>(FindObjectsSortMode.None))
            {
                if(light.type!=LightType.Directional)continue;
                if(light.name.Contains("cool sky fill"))continue;
                light.color=new Color(1.0f,.90f,.77f);
                light.intensity=Mathf.Max(light.intensity,1.08f);
                light.shadowStrength=.52f;
                light.shadows=LightShadows.Soft;
                light.transform.rotation=Quaternion.Euler(50f,-31f,0f);
            }

            var fillGo=new GameObject("Valoria · Full Frame · cool sky fill");
            fillGo.transform.SetParent(root,true);
            fillGo.transform.rotation=Quaternion.Euler(31f,145f,0f);
            var fill=fillGo.AddComponent<Light>();
            fill.type=LightType.Directional;
            fill.color=new Color(.58f,.71f,.92f);
            fill.intensity=.16f;
            fill.shadows=LightShadows.None;
        }

        static void BuildDistantWorld(Transform root)
        {
            // Fixed-camera 2.5D horizon layers: cheap, deterministic and intentionally non-interactive.
            CreateRidge(root,"far blue mountains",38f,-2.6f,36f,5.9f,4.8f,.37f,new Color(.31f,.43f,.53f),31f);
            CreateRidge(root,"middle mountain chain",32f,-2.4f,34f,4.9f,3.6f,.49f,new Color(.35f,.43f,.47f),17f);
            CreateRidge(root,"near wooded ridge",25f,-2.2f,30f,3.2f,2.2f,.64f,new Color(.24f,.31f,.29f),7f);

            // Tree-line silhouettes strengthen scale without introducing gameplay objects or low-poly foreground trees.
            var treeMat=FlatMaterial("Valoria Full Frame · distant forest",new Color(.16f,.24f,.22f));
            for(int i=0;i<34;i++)
            {
                float x=-22f+i*1.35f;
                if(Mathf.Abs(x)<4.0f)continue;
                float z=21.4f+(i%4)*.55f;
                float h=1.55f+(i%5)*.18f;
                CreateTriangle(root,"distant tree "+i,new Vector3(x,.15f,z),.65f,h,treeMat);
            }
        }

        static void BuildCorruptionThreat(Transform root)
        {
            var violet=FlatMaterial("Valoria Full Frame · distant corruption",new Color(.52f,.16f,.69f));
            var violetSoft=FlatMaterial("Valoria Full Frame · corruption haze",new Color(.34f,.15f,.46f));

            CreateTriangle(root,"corruption spire A",new Vector3(16.2f,2.1f,36.8f),1.65f,7.4f,violet);
            CreateTriangle(root,"corruption spire B",new Vector3(18.7f,1.7f,37.3f),1.25f,5.9f,violetSoft);
            CreateTriangle(root,"corruption spire C",new Vector3(14.1f,1.5f,37.0f),.95f,4.9f,violetSoft);

            var glow=new GameObject("Valoria · Full Frame · corruption glow");
            glow.transform.SetParent(root,true);
            glow.transform.position=new Vector3(16.4f,7.0f,34.5f);
            var light=glow.AddComponent<Light>();
            light.type=LightType.Point;
            light.color=new Color(.58f,.22f,.78f);
            light.intensity=2.1f;
            light.range=10f;
            light.shadows=LightShadows.None;
        }

        static void BuildWarmOccupation(Transform root)
        {
            var spots=new[]{
                new Vector3(-9.4f,1.5f,-2.4f),
                new Vector3(8.4f,1.5f,-4.4f),
                new Vector3(-4.7f,3.6f,5.8f),
                new Vector3(4.8f,3.0f,4.8f)
            };
            for(int i=0;i<spots.Length;i++)
            {
                var go=new GameObject("Valoria · Full Frame · warm occupation "+i);
                go.transform.SetParent(root,true);
                go.transform.position=spots[i];
                var light=go.AddComponent<Light>();
                light.type=LightType.Point;
                light.color=new Color(1.0f,.62f,.30f);
                light.intensity=.72f+(i%2)*.12f;
                light.range=4.5f;
                light.shadows=LightShadows.None;
            }
        }

        static void CreateRidge(Transform root,string name,float z,float baseY,float halfWidth,float meanHeight,float variation,float frequency,Color color,float seed)
        {
            const int segments=48;
            var verts=new Vector3[(segments+1)*2];
            var uv=new Vector2[verts.Length];
            var tris=new int[segments*6];

            for(int i=0;i<=segments;i++)
            {
                float t=i/(float)segments;
                float x=Mathf.Lerp(-halfWidth,halfWidth,t);
                float n=Mathf.PerlinNoise(seed+x*.055f,seed*.17f)-.5f;
                float wave=Mathf.Sin(x*frequency+seed*.11f)*.72f+Mathf.Sin(x*(frequency*.47f)+seed*.29f)*.41f;
                float peak=meanHeight+(n*2.0f+wave)*variation;
                int b=i*2;
                verts[b]=new Vector3(x,baseY,z);
                verts[b+1]=new Vector3(x,baseY+Mathf.Max(.8f,peak),z);
                uv[b]=new Vector2(t,0f);
                uv[b+1]=new Vector2(t,1f);
            }
            int ti=0;
            for(int i=0;i<segments;i++)
            {
                int a=i*2,b=a+1,c=a+2,d=a+3;
                tris[ti++]=a;tris[ti++]=b;tris[ti++]=c;
                tris[ti++]=c;tris[ti++]=b;tris[ti++]=d;
            }

            var mesh=new Mesh{name="Valoria Full Frame · "+name};
            mesh.vertices=verts;mesh.uv=uv;mesh.triangles=tris;
            mesh.RecalculateNormals();mesh.RecalculateBounds();

            var go=new GameObject("Valoria · Full Frame · "+name);
            go.transform.SetParent(root,true);
            go.AddComponent<MeshFilter>().sharedMesh=mesh;
            var mr=go.AddComponent<MeshRenderer>();
            mr.sharedMaterial=FlatMaterial("Valoria Full Frame · "+name,color);
            mr.shadowCastingMode=ShadowCastingMode.Off;
            mr.receiveShadows=false;
        }

        static void CreateTriangle(Transform root,string name,Vector3 basePosition,float halfWidth,float height,Material material)
        {
            var mesh=new Mesh{name="Valoria Full Frame · "+name};
            mesh.vertices=new[]{
                basePosition+new Vector3(-halfWidth,0f,0f),
                basePosition+new Vector3(halfWidth,0f,0f),
                basePosition+new Vector3(0f,height,0f)
            };
            mesh.triangles=new[]{0,2,1};
            mesh.RecalculateNormals();mesh.RecalculateBounds();
            var go=new GameObject("Valoria · Full Frame · "+name);
            go.transform.SetParent(root,true);
            go.AddComponent<MeshFilter>().sharedMesh=mesh;
            var mr=go.AddComponent<MeshRenderer>();
            mr.sharedMaterial=material;
            mr.shadowCastingMode=ShadowCastingMode.Off;
            mr.receiveShadows=false;
        }

        static Material FlatMaterial(string name,Color color)
        {
            var shader=Shader.Find("Universal Render Pipeline/Unlit")??Shader.Find("Unlit/Color")??Shader.Find("Standard");
            var m=new Material(shader){name=name};
            if(m.HasProperty("_BaseColor"))m.SetColor("_BaseColor",color);
            if(m.HasProperty("_Color"))m.SetColor("_Color",color);
            if(m.HasProperty("_Surface"))m.SetFloat("_Surface",0f);
            return m;
        }
    }
}
