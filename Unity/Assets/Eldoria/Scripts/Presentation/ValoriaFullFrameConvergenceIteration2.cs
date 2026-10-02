using Eldoria.Domain;
using UnityEngine;
using UnityEngine.Rendering;
using Object = UnityEngine.Object;

namespace Eldoria.Presentation
{
    // Full-frame convergence iteration 2: volumetric 3D world depth, global atmosphere and existing surface pass.
    public static class ValoriaFullFrameConvergenceIteration2
    {
        public static bool Enabled=false;
        const string RootName="Valoria · Full Frame Convergence · Iteration 2";

        public static void Build(Transform parent,PlayerState state)
        {
            if(!Enabled||parent==null||state==null)return;
            var old=GameObject.Find(RootName);
            if(old!=null)Object.DestroyImmediate(old);

            var root=new GameObject(RootName).transform;
            root.SetParent(parent,true);

            ApplyGlobalLook(root);
            BuildMountainBasin(root);
            BuildDistantCorruption(root);
        }

        static void ApplyGlobalLook(Transform root)
        {
            RenderSettings.ambientMode=AmbientMode.Trilight;
            RenderSettings.ambientSkyColor=new Color(.67f,.77f,.86f);
            RenderSettings.ambientEquatorColor=new Color(.50f,.50f,.45f);
            RenderSettings.ambientGroundColor=new Color(.24f,.22f,.19f);
            RenderSettings.ambientIntensity=.96f;

            RenderSettings.fog=true;
            RenderSettings.fogMode=FogMode.Linear;
            RenderSettings.fogColor=new Color(.57f,.67f,.74f);
            RenderSettings.fogStartDistance=30f;
            RenderSettings.fogEndDistance=96f;

            var camera=Camera.main;
            if(camera!=null)
            {
                camera.clearFlags=CameraClearFlags.SolidColor;
                camera.backgroundColor=new Color(.43f,.63f,.78f);
                camera.allowHDR=true;
            }

            foreach(var light in Object.FindObjectsByType<Light>(FindObjectsSortMode.None))
            {
                if(light.type!=LightType.Directional)continue;
                light.color=new Color(1.0f,.90f,.77f);
                light.intensity=Mathf.Max(light.intensity,1.15f);
                light.shadowStrength=.48f;
                light.shadows=LightShadows.Soft;
                light.transform.rotation=Quaternion.Euler(49f,-29f,0f);
            }

            var fillGo=new GameObject("Valoria · Full Frame v2 · cool sky fill");
            fillGo.transform.SetParent(root,true);
            fillGo.transform.rotation=Quaternion.Euler(30f,148f,0f);
            var fill=fillGo.AddComponent<Light>();
            fill.type=LightType.Directional;
            fill.color=new Color(.58f,.72f,.94f);
            fill.intensity=.13f;
            fill.shadows=LightShadows.None;
        }

        static void BuildMountainBasin(Transform root)
        {
            var far=ValoriaKit.ExternalPbrSurfaceMaterial(
                "rock",new Color(.39f,.47f,.50f,1f),new Vector2(5.8f,5.8f),.012f,1.05f)
                ?? ValoriaKit.DetailedSurfaceMaterial(new Color(.35f,.42f,.45f,1f),"earth",new Vector2(5.8f,5.8f),1.04f);

            var mid=ValoriaKit.ExternalPbrSurfaceMaterial(
                "rock",new Color(.31f,.38f,.37f,1f),new Vector2(5.0f,5.0f),.014f,1.08f)
                ?? ValoriaKit.DetailedSurfaceMaterial(new Color(.29f,.35f,.34f,1f),"earth",new Vector2(5.0f,5.0f),1.07f);

            BuildMountainSheet(root,"far range",-39f,39f,24f,52f,-3.1f,10.2f,.26f,10.7f,far,17.3f);
            BuildMountainSheet(root,"mid range",-34f,34f,18f,39f,-2.4f,7.2f,.32f,8.2f,mid,8.6f);
        }

        static void BuildMountainSheet(Transform root,string role,float xMin,float xMax,float zMin,float zMax,
            float baseY,float maxRise,float centerValley,float sideBoost,Material material,float seed)
        {
            const int cols=57;
            const int rows=29;
            var verts=new Vector3[cols*rows];
            var uv=new Vector2[verts.Length];
            var tris=new int[(cols-1)*(rows-1)*6];

            for(int rz=0;rz<rows;rz++)
            {
                float tz=rz/(float)(rows-1);
                float z=Mathf.Lerp(zMin,zMax,tz);
                for(int cx=0;cx<cols;cx++)
                {
                    float tx=cx/(float)(cols-1);
                    float x=Mathf.Lerp(xMin,xMax,tx);

                    float side=Mathf.SmoothStep(0f,1f,Mathf.Clamp01((Mathf.Abs(x)-7.5f)/20f));
                    float rear=Mathf.SmoothStep(0f,1f,tz);
                    float basin=1f-Mathf.Exp(-(x*x)*.0065f);
                    basin=Mathf.Lerp(centerValley,1f,basin);

                    float n1=(Mathf.PerlinNoise(seed+x*.055f,seed+z*.045f)-.5f)*2f;
                    float n2=(Mathf.PerlinNoise(seed*1.7f+x*.13f,seed*.7f+z*.10f)-.5f)*2f;
                    float waves=Mathf.Sin(x*.16f+seed)*.55f+Mathf.Sin(x*.31f+z*.07f+seed*.3f)*.32f;
                    float crest=(.28f+.72f*rear)*(3.1f+maxRise*(.38f+.62f*side))*basin;
                    crest+=side*sideBoost*.28f;
                    float y=baseY+crest+(n1*1.20f+n2*.42f+waves*.55f)*(1.1f+side*.9f);

                    // Open the central skyline over the Bastion rather than drawing a wall behind it.
                    float centerOpen=1f-Mathf.SmoothStep(0f,1f,Mathf.Clamp01((Mathf.Abs(x)-3.5f)/8.5f));
                    float nearOpen=1f-Mathf.SmoothStep(0f,1f,Mathf.Clamp01((z-24f)/9f));
                    y-=centerOpen*nearOpen*4.5f;

                    int i=rz*cols+cx;
                    verts[i]=new Vector3(x,y,z);
                    uv[i]=new Vector2(x*.10f,z*.10f);
                }
            }

            int ti=0;
            for(int r=0;r<rows-1;r++)
            for(int c=0;c<cols-1;c++)
            {
                int a=r*cols+c,b=a+1,d=(r+1)*cols+c,e=d+1;
                tris[ti++]=a;tris[ti++]=d;tris[ti++]=b;
                tris[ti++]=b;tris[ti++]=d;tris[ti++]=e;
            }

            var mesh=new Mesh{name="Valoria Full Frame v2 · "+role};
            mesh.indexFormat=IndexFormat.UInt32;
            mesh.vertices=verts;
            mesh.uv=uv;
            mesh.triangles=tris;
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();

            var go=new GameObject("Valoria · Full Frame v2 · "+role);
            go.transform.SetParent(root,true);
            go.AddComponent<MeshFilter>().sharedMesh=mesh;
            var mr=go.AddComponent<MeshRenderer>();
            mr.sharedMaterial=material;
            mr.shadowCastingMode=ShadowCastingMode.On;
            mr.receiveShadows=true;
        }

        static void BuildDistantCorruption(Transform root)
        {
            // Reuse real project rock rather than flat/procedural iconography.
            var art=ValoriaExternalAssetLibrary.Load();
            if(art==null||art.SlavicBoulder==null)return;

            var anchors=new[]{
                new Vector3(17.0f,4.8f,31.5f),
                new Vector3(18.6f,4.3f,32.4f),
                new Vector3(15.5f,4.0f,32.8f)
            };
            for(int i=0;i<anchors.Length;i++)
            {
                var go=Object.Instantiate(art.SlavicBoulder);
                go.name="Valoria · Full Frame v2 · corruption rock "+i;
                go.transform.position=anchors[i];
                go.transform.rotation=Quaternion.Euler((i-1)*7f,31f+i*43f,(i-1)*6f);
                go.transform.localScale*=new Vector3(1.0f,.8f+(.18f*i),1.0f)*(1.8f+i*.28f);
                foreach(var c in go.GetComponentsInChildren<Collider>(true))c.enabled=false;
                foreach(var b in go.GetComponentsInChildren<MonoBehaviour>(true))b.enabled=false;
                Tint(go,new Color(.31f,.22f,.36f,1f));
                go.transform.SetParent(root,true);
            }

            var glow=new GameObject("Valoria · Full Frame v2 · distant corruption glow");
            glow.transform.SetParent(root,true);
            glow.transform.position=new Vector3(17.2f,7.4f,31.6f);
            var light=glow.AddComponent<Light>();
            light.type=LightType.Point;
            light.color=new Color(.54f,.22f,.73f);
            light.intensity=1.15f;
            light.range=8.2f;
            light.shadows=LightShadows.None;
        }

        static void Tint(GameObject go,Color tint)
        {
            foreach(var r in go.GetComponentsInChildren<Renderer>(true))
            {
                var mats=r.sharedMaterials;
                var copy=new Material[mats.Length];
                for(int i=0;i<mats.Length;i++)
                {
                    if(mats[i]==null){copy[i]=null;continue;}
                    var m=new Material(mats[i]){name="Valoria Full Frame v2 · corruption material"};
                    if(m.HasProperty("_BaseColor"))m.SetColor("_BaseColor",Color.Lerp(Color.white,tint,.58f));
                    if(m.HasProperty("_Color"))m.SetColor("_Color",Color.Lerp(Color.white,tint,.58f));
                    if(m.HasProperty("_Smoothness"))m.SetFloat("_Smoothness",.08f);
                    copy[i]=m;
                }
                r.sharedMaterials=copy;
            }
        }
    }
}
