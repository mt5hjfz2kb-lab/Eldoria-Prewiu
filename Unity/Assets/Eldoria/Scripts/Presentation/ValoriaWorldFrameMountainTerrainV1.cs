using Eldoria.Domain;
using UnityEngine;
using UnityEngine.Rendering;
using Object = UnityEngine.Object;

namespace Eldoria.Presentation
{
    public static class ValoriaWorldFrameMountainTerrainV1
    {
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
            const float xMin=-25f,xMax=25f,zMin=-13f,zMax=31f;
            const float hiddenY=-.64f;

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

                    float side=Mathf.SmoothStep(0f,1f,Mathf.Clamp01((Mathf.Abs(x)-13.4f)/6.3f));
                    float rear=Mathf.SmoothStep(0f,1f,Mathf.Clamp01((z-15.0f)/10.5f));
                    // Keep the player approach open, but let the world frame begin closer to the inhabited mass.
                    float frontGate=Mathf.SmoothStep(0f,1f,Mathf.Clamp01((z-1.0f)/6.0f));
                    float sideRelief=side*frontGate;
                    float relief=Mathf.Max(sideRelief,rear);

                    // Final framing refinement: keep the proven silhouette but make it read as rock,
                    // not a smooth berm. The matched camera magnifies the west/left wall, so it stays lower.
                    float rearSide=rear*side;
                    float sideScale=x<0f?.60f:.78f;
                    float ridge=sideRelief*sideRelief*3.35f*sideScale + rearSide*rearSide*1.70f;

                    float broad=Mathf.Sin(x*.115f+z*.035f)*.30f
                               +Mathf.Sin(z*.145f-x*.028f)*.24f
                               +Mathf.Sin((x+z)*.071f)*.16f;
                    float macroNoise=(Mathf.PerlinNoise(x*.085f+7.31f,z*.085f+11.17f)-.5f)*1.05f;
                    float detailNoise=(Mathf.PerlinNoise(x*.22f+19.43f,z*.22f+2.71f)-.5f)*.34f;
                    float strata=Mathf.Sin(z*.58f+x*.075f)*.10f;
                    float visibleRelief=Mathf.Max(sideRelief,rearSide);
                    float y=hiddenY + ridge
                        + (broad+macroNoise+detailNoise+strata)*visibleRelief*.72f;

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

            FillGridTriangles(tris,rows,cols,false);
            CreateMeshObject(root,"continuous mountain valley",verts,uv,tris,material);
        }

        static void BuildMarginOccupation(Transform root)
        {
            var art=ValoriaExternalAssetLibrary.Load();
            if(art==null)return;

            if(art.SlavicBoulder!=null)
            {
                var anchors=new[]{
                    new Vector3(-15.8f,.05f,-1.8f),new Vector3(-16.4f,.05f,4.1f),new Vector3(-15.3f,.05f,10.6f),
                    new Vector3(15.6f,.05f,-1.2f),new Vector3(16.2f,.05f,4.8f),new Vector3(15.1f,.05f,10.9f),
                    new Vector3(-8.8f,.08f,16.0f),new Vector3(9.2f,.08f,16.4f)
                };
                for(int i=0;i<anchors.Length;i++)
                    AddFramePrefab(root,art.SlavicBoulder,"margin boulder "+i,anchors[i],
                        1.55f+(i%3)*.16f,1.05f+(i%2)*.10f,(i*47)%360,new Color(.37f,.39f,.37f,1f));
            }

            if(art.SlavicFlatRock!=null)
            {
                var anchors=new[]{
                    new Vector3(-13.8f,.02f,-4.6f),new Vector3(-17.0f,.02f,7.2f),
                    new Vector3(13.9f,.02f,-4.0f),new Vector3(16.8f,.02f,7.8f),
                    new Vector3(-5.8f,.03f,16.8f),new Vector3(6.4f,.03f,17.0f)
                };
                for(int i=0;i<anchors.Length;i++)
                    AddFramePrefab(root,art.SlavicFlatRock,"margin flat rock "+i,anchors[i],
                        2.1f+(i%2)*.25f,.55f,(i*61)%360,new Color(.35f,.37f,.35f,1f));
            }

            if(art.SlavicStoneFence!=null)
            {
                AddFramePrefab(root,art.SlavicStoneFence,"west ruined edge fence",new Vector3(-13.8f,.02f,2.0f),
                    3.6f,1.15f,22f,new Color(.48f,.47f,.43f,1f));
                AddFramePrefab(root,art.SlavicStoneFence,"east ruined edge fence",new Vector3(13.7f,.02f,3.0f),
                    3.4f,1.15f,202f,new Color(.48f,.47f,.43f,1f));
            }

            if(art.Firewood!=null)
            {
                AddFramePrefab(root,art.Firewood,"west work-edge firewood",new Vector3(-11.9f,.03f,-2.7f),
                    1.9f,.95f,18f,new Color(.58f,.48f,.34f,1f));
                AddFramePrefab(root,art.Firewood,"east work-edge firewood",new Vector3(11.8f,.03f,-2.0f),
                    1.8f,.95f,198f,new Color(.58f,.48f,.34f,1f));
            }
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
