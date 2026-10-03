using Eldoria.Domain;
using UnityEngine;
using UnityEngine.Rendering;
using System.Collections.Generic;
using Object = UnityEngine.Object;

namespace Eldoria.Presentation
{
    public static class ValoriaWorldFrameMountainTerrainV1
    {
        // Production visual frame: validated by matched WorldFrame proof and the Valoria/world-map formula gates.
        public static bool Enabled=true;
        const string RootName="Valoria · World Frame Mountain Terrain v1";

        public static void Build(Transform parent,PlayerState state)
        {
            if(!Enabled||parent==null)return;
            var old=GameObject.Find(RootName);
            if(old!=null)Object.DestroyImmediate(old);

            var root=new GameObject(RootName);
            root.transform.SetParent(parent,true);

            // One continuous terrain sheet avoids detached panels and inter-piece seams.
            // The inhabited corridor sits below canonical ground; only side/rear relief emerges.
            var material=ValoriaKit.ExternalPbrSurfaceMaterial(
                "rock",new Color(.25f,.275f,.255f,1f),new Vector2(5.2f,5.2f),.018f,1.18f)
                ?? ValoriaKit.DetailedSurfaceMaterial(
                    new Color(.245f,.27f,.25f,1f),"earth",new Vector2(5.2f,5.2f),1.18f);

            BuildContinuousValley(root.transform,material);
            BuildMarginOccupation(root.transform);
        }

        static void BuildContinuousValley(Transform root,Material material)
        {
            // Review iteration: balanced side walls + open central saddle; geometry unchanged by proof trigger.
            const int cols=81;
            const int rows=73;
            const float xMin=-23f,xMax=23f,zMin=-13f,zMax=31f;
            const float hiddenY=-.64f;

            var verts=new Vector3[cols*rows];
            var uv=new Vector2[verts.Length];

            for(int rz=0;rz<rows;rz++)
            {
                float tz=rz/(float)(rows-1);
                float z=Mathf.Lerp(zMin,zMax,tz);
                for(int cx=0;cx<cols;cx++)
                {
                    float tx=cx/(float)(cols-1);
                    float x=Mathf.Lerp(xMin,xMax,tx);

                    // Iteration 2: the first continuous valley proved the concept, but its side walls
                    // entered the review frame as broad smooth ramps. Start farther out and compress the rise
                    // into a narrower rocky crest so the city remains visually open.
                    float side=Mathf.SmoothStep(0f,1f,Mathf.Clamp01((Mathf.Abs(x)-14.8f)/4.7f));
                    float rear=Mathf.SmoothStep(0f,1f,Mathf.Clamp01((z-18.0f)/8.0f));
                    float frontGate=Mathf.SmoothStep(0f,1f,Mathf.Clamp01((z-2.0f)/7.0f));
                    float sideRelief=side*frontGate;
                    float rearSide=rear*Mathf.Max(.35f,side);
                    float visibleRelief=Mathf.Max(sideRelief,rearSide);

                    float sideScale=x<0f?.56f:.66f;
                    float crest=Mathf.Pow(sideRelief,2.65f)*3.15f*sideScale;
                    float rearCrest=Mathf.Pow(rearSide,2.25f)*1.55f;

                    // Layered rock breakup: broad shape + deterministic Perlin + sharper ledge bands.
                    float broad=Mathf.Sin(x*.17f+z*.065f)*.34f
                               +Mathf.Sin(z*.23f-x*.055f)*.27f
                               +Mathf.Sin((x+z)*.105f)*.18f;
                    float macroNoise=(Mathf.PerlinNoise(x*.115f+7.31f,z*.115f+11.17f)-.5f)*1.30f;
                    float detailNoise=(Mathf.PerlinNoise(x*.31f+19.43f,z*.31f+2.71f)-.5f)*.46f;
                    float ledges=(Mathf.Abs(Mathf.Sin(z*.72f+x*.11f))-.50f)*.28f;
                    float notch=(Mathf.PerlinNoise(x*.19f+3.2f,z*.14f+8.4f)-.5f)*.48f;
                    float y=hiddenY + crest + rearCrest
                        + (broad+macroNoise+detailNoise+ledges+notch)*visibleRelief*.78f;

                    // Keep a generous central basin and the full approach invisible beneath gameplay ground.
                    float cityX=1f-Mathf.SmoothStep(0f,1f,Mathf.Clamp01((Mathf.Abs(x)-10.6f)/3.8f));
                    float cityZ=1f-Mathf.SmoothStep(0f,1f,Mathf.Clamp01((z-10.5f)/7.0f));
                    float basin=cityX*cityZ;
                    y=Mathf.Lerp(y,hiddenY,basin);
                    if(z<1.5f)y=hiddenY;

                    int i=rz*cols+cx;
                    verts[i]=new Vector3(x,y,z);
                    uv[i]=new Vector2(x*.13f,z*.13f);
                }
            }

            // v40: do not tessellate the central/front basin. In v39 those lowered
            // vertices still formed a giant visible grey sheet from the strategic camera.
            // Keep only lateral and rear mountain cells so the photographic valley/backplate
            // remains visible through a genuine opening around and in front of Valoria.
            var triList=new List<int>((cols-1)*(rows-1)*6);
            for(int rz=0;rz<rows-1;rz++)
            {
                float zc=Mathf.Lerp(zMin,zMax,(rz+.5f)/(rows-1f));
                for(int cx=0;cx<cols-1;cx++)
                {
                    float xc=Mathf.Lerp(xMin,xMax,(cx+.5f)/(cols-1f));
                    float ax=Mathf.Abs(xc);

                    // Side mountains begin outside the playable city envelope.
                    // Rear mountains close the horizon, with a wider opening toward camera.
                    bool sideCell=ax>=13.25f && zc>=1.25f;
                    bool rearCell=zc>=15.25f && ax>=8.75f;
                    if(!sideCell && !rearCell)continue;

                    int a=rz*cols+cx,b=a+1,d=(rz+1)*cols+cx,e=d+1;
                    triList.Add(a);triList.Add(d);triList.Add(b);
                    triList.Add(b);triList.Add(d);triList.Add(e);
                }
            }
            CreateMeshObject(root,"open mountain valley frame",verts,uv,triList.ToArray(),material);
        }

        static void BuildMarginOccupation(Transform root)
        {
            // Keep only shader-safe dark boulders. The tested flat rocks, stone fences and firewood
            // read as bright pasted props at strategic zooms and reduce visual cohesion.
            var art=ValoriaExternalAssetLibrary.Load();
            if(art==null||art.SlavicBoulder==null)return;

            var anchors=new[]{
                new Vector3(-16.2f,.05f,-.8f),new Vector3(-16.5f,.05f,4.8f),new Vector3(-15.8f,.05f,10.8f),
                new Vector3(16.1f,.05f,-.3f),new Vector3(16.4f,.05f,5.2f),new Vector3(15.7f,.05f,11.1f),
                new Vector3(-9.4f,.08f,17.2f),new Vector3(9.6f,.08f,17.4f)
            };
            for(int i=0;i<anchors.Length;i++)
                AddFramePrefab(root,art.SlavicBoulder,"margin boulder "+i,anchors[i],
                    1.32f+(i%3)*.12f,.92f+(i%2)*.07f,(i*47)%360,new Color(.34f,.36f,.34f,1f));

            // No pass-owned pines: the canonical procedural pine reads as a dark low-poly cone
            // at strategic zooms and reduces the quality of the validated World Frame.
        }

        static void AddFramePrefab(Transform root,GameObject source,string role,Vector3 anchor,float span,float maxHeight,float yaw,Color tint)
        {
            if(source==null)return;
            var go=Object.Instantiate(source);
            go.name="Valoria · World Frame v1 · "+role;
            go.transform.rotation=Quaternion.Euler(0f,yaw,0f);

            var rs=go.GetComponentsInChildren<Renderer>(true);
            if(rs.Length==0){Object.DestroyImmediate(go);return;}
            var b=rs[0].bounds;
            for(int i=1;i<rs.Length;i++)b.Encapsulate(rs[i].bounds);

            float horizontal=Mathf.Max(b.size.x,b.size.z);
            float scale=Mathf.Min(span/Mathf.Max(.001f,horizontal),maxHeight/Mathf.Max(.001f,b.size.y));
            go.transform.localScale*=scale;

            rs=go.GetComponentsInChildren<Renderer>(true);
            b=rs[0].bounds;
            for(int i=1;i<rs.Length;i++)b.Encapsulate(rs[i].bounds);
            go.transform.position+=anchor-new Vector3(b.center.x,b.min.y,b.center.z);

            ApplyFrameSafeLit(go,tint);
            foreach(var c in go.GetComponentsInChildren<Collider>(true))c.enabled=false;
            foreach(var behaviour in go.GetComponentsInChildren<MonoBehaviour>(true))behaviour.enabled=false;
            go.transform.SetParent(root,true);
        }

        static void ApplyFrameSafeLit(GameObject go,Color tint)
        {
            var shader=Shader.Find("Universal Render Pipeline/Lit")??Shader.Find("Standard");
            if(shader==null)return;

            foreach(var r in go.GetComponentsInChildren<Renderer>(true))
            {
                var source=r.sharedMaterials;
                var target=new Material[source.Length];
                for(int i=0;i<source.Length;i++)
                {
                    var old=source[i];
                    if(old==null){target[i]=null;continue;}
                    var m=new Material(shader){name="Valoria World Frame v1 · safe lit"};

                    Texture tex=null;
                    Vector2 scale=Vector2.one,offset=Vector2.zero;
                    if(old.HasProperty("_BaseMap"))
                    {
                        tex=old.GetTexture("_BaseMap");
                        scale=old.GetTextureScale("_BaseMap");
                        offset=old.GetTextureOffset("_BaseMap");
                    }
                    else if(old.HasProperty("_MainTex"))
                    {
                        tex=old.GetTexture("_MainTex");
                        scale=old.GetTextureScale("_MainTex");
                        offset=old.GetTextureOffset("_MainTex");
                    }

                    if(tex!=null)
                    {
                        if(m.HasProperty("_BaseMap")){m.SetTexture("_BaseMap",tex);m.SetTextureScale("_BaseMap",scale);m.SetTextureOffset("_BaseMap",offset);}
                        if(m.HasProperty("_MainTex")){m.SetTexture("_MainTex",tex);m.SetTextureScale("_MainTex",scale);m.SetTextureOffset("_MainTex",offset);}
                    }

                    var surface=Color.Lerp(Color.white,tint,.30f);
                    if(m.HasProperty("_BaseColor"))m.SetColor("_BaseColor",surface);
                    if(m.HasProperty("_Color"))m.SetColor("_Color",surface);
                    if(m.HasProperty("_Metallic"))m.SetFloat("_Metallic",0f);
                    if(m.HasProperty("_Smoothness"))m.SetFloat("_Smoothness",.045f);
                    target[i]=m;
                }
                r.sharedMaterials=target;
            }
        }

        static void FillGridTriangles(int[] tris,int rows,int cols,bool flip)
        {
            int ti=0;
            for(int r=0;r<rows-1;r++)
            for(int c=0;c<cols-1;c++)
            {
                int a=r*cols+c,b=a+1,d=(r+1)*cols+c,e=d+1;
                if(!flip)
                {
                    tris[ti++]=a;tris[ti++]=d;tris[ti++]=b;
                    tris[ti++]=b;tris[ti++]=d;tris[ti++]=e;
                }
                else
                {
                    tris[ti++]=a;tris[ti++]=b;tris[ti++]=d;
                    tris[ti++]=b;tris[ti++]=e;tris[ti++]=d;
                }
            }
        }

        static void CreateMeshObject(Transform root,string role,Vector3[] verts,Vector2[] uv,int[] tris,Material material)
        {
            var mesh=new Mesh{name="Valoria World Frame v1 · "+role};
            mesh.indexFormat=IndexFormat.UInt32;
            mesh.vertices=verts;
            mesh.uv=uv;
            mesh.triangles=tris;
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();

            var go=new GameObject("Valoria · World Frame v1 · "+role);
            go.transform.SetParent(root,true);
            go.AddComponent<MeshFilter>().sharedMesh=mesh;
            var renderer=go.AddComponent<MeshRenderer>();
            renderer.sharedMaterial=material;
            renderer.shadowCastingMode=ShadowCastingMode.On;
            renderer.receiveShadows=true;
        }
    }
}
