using Eldoria.Domain;
using UnityEngine;
using UnityEngine.Rendering;
using Object = UnityEngine.Object;

namespace Eldoria.Presentation
{
    // World Surround v2: open skyline + side terrain shoulders + real mountain inventory.
    // Visual-only; underlying gameplay world remains authoritative.
    public static class ValoriaFullFrameWorldSurroundV2
    {
        public static bool Enabled=false;
        const string RootName="Valoria · Full Frame World Surround v2";

        public static void Build(Transform parent,PlayerState state)
        {
            if(!Enabled||parent==null)return;
            var old=GameObject.Find(RootName);
            if(old!=null)Object.DestroyImmediate(old);
            var root=new GameObject(RootName).transform;
            root.SetParent(parent,true);

            ApplySky();
            BuildSideShoulder(root,true);
            BuildSideShoulder(root,false);
            BuildFrontSkirt(root);
            BuildRearMountains(root);
            BuildForestBands(root);
            BuildThreatRuin(root);
        }

        static void ApplySky()
        {
            var shader=Shader.Find("Skybox/Procedural");
            if(shader!=null)
            {
                var sky=new Material(shader){name="Valoria · Full Frame v2 · sky"};
                if(sky.HasProperty("_SkyTint"))sky.SetColor("_SkyTint",new Color(.43f,.63f,.82f));
                if(sky.HasProperty("_GroundColor"))sky.SetColor("_GroundColor",new Color(.27f,.31f,.31f));
                if(sky.HasProperty("_AtmosphereThickness"))sky.SetFloat("_AtmosphereThickness",1.18f);
                if(sky.HasProperty("_Exposure"))sky.SetFloat("_Exposure",1.18f);
                if(sky.HasProperty("_SunSize"))sky.SetFloat("_SunSize",.035f);
                RenderSettings.skybox=sky;
                DynamicGI.UpdateEnvironment();
            }

            var camera=Camera.main;
            if(camera!=null)
            {
                camera.clearFlags=shader!=null?CameraClearFlags.Skybox:CameraClearFlags.SolidColor;
                camera.backgroundColor=new Color(.47f,.68f,.82f);
                camera.allowHDR=true;
            }

            RenderSettings.ambientMode=AmbientMode.Trilight;
            RenderSettings.ambientSkyColor=new Color(.69f,.78f,.86f);
            RenderSettings.ambientEquatorColor=new Color(.50f,.50f,.46f);
            RenderSettings.ambientGroundColor=new Color(.24f,.23f,.21f);
            RenderSettings.ambientIntensity=.98f;
            RenderSettings.fog=true;
            RenderSettings.fogMode=FogMode.Linear;
            RenderSettings.fogColor=new Color(.60f,.70f,.78f);
            RenderSettings.fogStartDistance=36f;
            RenderSettings.fogEndDistance=108f;

            foreach(var light in Object.FindObjectsByType<Light>(FindObjectsSortMode.None))
            {
                if(light.type!=LightType.Directional)continue;
                light.color=new Color(1f,.91f,.79f);
                light.intensity=Mathf.Max(light.intensity,1.14f);
                light.shadowStrength=.46f;
                light.shadows=LightShadows.Soft;
                light.transform.rotation=Quaternion.Euler(50f,-30f,0f);
            }
        }

        static void BuildSideShoulder(Transform root,bool west)
        {
            const int nx=12;
            const int nz=28;
            float inner=west?-12.0f:12.0f;
            float outer=west?-23.0f:23.0f;
            var verts=new Vector3[nx*nz];
            var uv=new Vector2[verts.Length];
            var tris=new int[(nx-1)*(nz-1)*6];

            for(int z=0;z<nz;z++)
            {
                float tz=z/(float)(nz-1);
                float wz=Mathf.Lerp(-11f,18f,tz);
                for(int x=0;x<nx;x++)
                {
                    float tx=x/(float)(nx-1);
                    float wx=Mathf.Lerp(inner,outer,tx);
                    float rise=Mathf.Pow(tx,1.75f)*(1.8f+2.4f*Mathf.SmoothStep(0f,1f,Mathf.Clamp01((wz+2f)/16f)));
                    float n=(Mathf.PerlinNoise((west?3.1f:9.2f)+wx*.12f,7.6f+wz*.10f)-.5f)*.75f;
                    float ledge=(Mathf.Abs(Mathf.Sin(wz*.58f+wx*.09f))-.50f)*.22f;
                    float y=-.52f+rise+n*(.25f+.75f*tx)+ledge*tx;
                    int i=z*nx+x;
                    verts[i]=new Vector3(wx,y,wz);
                    uv[i]=new Vector2(tx,tz*2.4f);
                }
            }
            Fill(tris,nx,nz,west);
            var mat=ValoriaKit.ExternalPbrSurfaceMaterial(
                "rock",new Color(.34f,.35f,.31f,1f),new Vector2(4.4f,4.4f),.014f,1.06f)
                ?? ValoriaKit.DetailedSurfaceMaterial(new Color(.30f,.31f,.28f,1f),"earth",new Vector2(4.4f,4.4f),1.04f);
            MeshObject(root,west?"west shoulder":"east shoulder",verts,uv,tris,mat);
        }

        static void BuildFrontSkirt(Transform root)
        {
            const int nx=45;
            const int nz=8;
            var verts=new Vector3[nx*nz];
            var uv=new Vector2[verts.Length];
            var tris=new int[(nx-1)*(nz-1)*6];

            for(int z=0;z<nz;z++)
            {
                float tz=z/(float)(nz-1);
                float wz=Mathf.Lerp(-14.5f,-8.4f,tz);
                for(int x=0;x<nx;x++)
                {
                    float tx=x/(float)(nx-1);
                    float wx=Mathf.Lerp(-23f,23f,tx);
                    float edge=1f-tz;
                    float side=Mathf.Clamp01((Mathf.Abs(wx)-8f)/14f);
                    float n=(Mathf.PerlinNoise(wx*.10f+4.4f,wz*.13f+12f)-.5f)*.65f;
                    float y=-1.0f+edge*.8f+side*.50f+n*.35f;
                    int i=z*nx+x;
                    verts[i]=new Vector3(wx,y,wz);
                    uv[i]=new Vector2(tx*5f,tz);
                }
            }
            Fill(tris,nx,nz,false);
            var mat=ValoriaKit.ExternalPbrSurfaceMaterial(
                "dirt",new Color(.34f,.31f,.25f,1f),new Vector2(6f,2f),.010f,1.02f)
                ?? ValoriaKit.DetailedSurfaceMaterial(new Color(.30f,.28f,.23f,1f),"earth",new Vector2(6f,2f),1.0f);
            MeshObject(root,"front skirt",verts,uv,tris,mat);
        }

        static void BuildRearMountains(Transform root)
        {
            var mountain=Resources.Load<GameObject>("WorldInventory/Mountain01");
            var rock=Resources.Load<GameObject>("WorldInventory/Rock02");
            if(mountain==null)return;

            var far=new[]{
                new Vector4(-25f,30f,22f,11.0f),
                new Vector4(-13f,33f,19f,12.6f),
                new Vector4(0f,36f,21f,14.0f),
                new Vector4(14f,33f,20f,12.5f),
                new Vector4(27f,29f,22f,10.8f)
            };
            for(int i=0;i<far.Length;i++)
            {
                var s=far[i];
                Add(root,mountain,"far mountain "+i,new Vector3(s.x,-4.0f,s.y),s.z,s.w,137f+i*41f,
                    new Color(.55f,.63f,.67f,1f));
            }

            var mid=new[]{
                new Vector4(-22f,22f,13.5f,7.2f),
                new Vector4(-9f,24f,11.0f,6.3f),
                new Vector4(11f,24f,11.5f,6.5f),
                new Vector4(23f,21f,13.8f,7.0f)
            };
            for(int i=0;i<mid.Length;i++)
            {
                var s=mid[i];
                Add(root,mountain,"mid mountain "+i,new Vector3(s.x,-2.7f,s.y),s.z,s.w,168f+i*47f,
                    new Color(.42f,.48f,.48f,1f));
            }

            if(rock!=null)
            {
                Add(root,rock,"rear rock west",new Vector3(-14.6f,-.55f,16.6f),5.8f,2.8f,46f,new Color(.38f,.39f,.36f,1f));
                Add(root,rock,"rear rock east",new Vector3(14.4f,-.55f,16.9f),5.9f,2.8f,221f,new Color(.38f,.39f,.36f,1f));
            }
        }

        static void BuildForestBands(Transform root)
        {
            var a=Resources.Load<GameObject>("WorldInventory/Tree01A");
            var b=Resources.Load<GameObject>("WorldInventory/Tree01B");
            if(a==null&&b==null)return;

            for(int i=0;i<36;i++)
            {
                float x=-22f+i*1.27f;
                if(Mathf.Abs(x)<5.0f)continue;
                float z=13.0f+(i%5)*.85f;
                var source=(i%2==0?a:b)??a??b;
                Add(root,source,"rear forest "+i,new Vector3(x,-.12f,z),1.10f+(i%3)*.10f,2.7f+(i%4)*.22f,
                    (i*53)%360,new Color(.48f,.59f,.48f,1f));
            }

            for(int i=0;i<18;i++)
            {
                float side=i<9?-1f:1f;
                int k=i%9;
                float x=side*(13.4f+k*.95f);
                float z=-1.5f+k*1.35f;
                var source=(i%2==0?a:b)??a??b;
                Add(root,source,"side forest "+i,new Vector3(x,-.08f,z),1.00f,2.45f+(i%3)*.18f,
                    (i*67)%360,new Color(.45f,.57f,.46f,1f));
            }
        }

        static void BuildThreatRuin(Transform root)
        {
            var arch=Resources.Load<GameObject>("WorldInventory/Arch_Gothic");
            var wall=Resources.Load<GameObject>("WorldInventory/Wall_Broken");
            if(arch!=null)Add(root,arch,"threat arch",new Vector3(18.1f,-.35f,18.8f),4.8f,7.2f,198f,new Color(.42f,.40f,.43f,1f));
            if(wall!=null)
            {
                Add(root,wall,"threat wall west",new Vector3(15.2f,-.30f,19.6f),4.0f,3.8f,181f,new Color(.40f,.38f,.41f,1f));
                Add(root,wall,"threat wall east",new Vector3(20.8f,-.30f,20.1f),3.6f,3.5f,229f,new Color(.40f,.38f,.41f,1f));
            }
            var glow=new GameObject("Valoria · World Surround v2 · corruption glow");
            glow.transform.SetParent(root,true);
            glow.transform.position=new Vector3(18.1f,4.9f,19.0f);
            var l=glow.AddComponent<Light>();
            l.type=LightType.Point;
            l.color=new Color(.58f,.20f,.77f);
            l.intensity=.85f;
            l.range=7.2f;
            l.shadows=LightShadows.None;
        }

        static void Add(Transform root,GameObject source,string role,Vector3 p,float footprint,float maxHeight,float yaw,Color tint)
        {
            if(source==null)return;
            var go=ValoriaKit.BenchmarkPieceModulated("Valoria · World Surround v2 · "+role,source,p,footprint,maxHeight,Quaternion.Euler(0f,yaw,0f),tint);
            if(go==null)return;
            go.transform.SetParent(root,true);
            foreach(var c in go.GetComponentsInChildren<Collider>(true))c.enabled=false;
            foreach(var b in go.GetComponentsInChildren<MonoBehaviour>(true))b.enabled=false;
        }

        static void MeshObject(Transform root,string role,Vector3[] verts,Vector2[] uv,int[] tris,Material mat)
        {
            var mesh=new Mesh{name="Valoria World Surround v2 · "+role};
            mesh.indexFormat=IndexFormat.UInt32;
            mesh.vertices=verts;mesh.uv=uv;mesh.triangles=tris;mesh.RecalculateNormals();mesh.RecalculateBounds();
            var go=new GameObject("Valoria · World Surround v2 · "+role);
            go.transform.SetParent(root,true);
            go.AddComponent<MeshFilter>().sharedMesh=mesh;
            var mr=go.AddComponent<MeshRenderer>();
            mr.sharedMaterial=mat;
            mr.shadowCastingMode=ShadowCastingMode.On;
            mr.receiveShadows=true;
        }

        static void Fill(int[] tris,int nx,int nz,bool flip)
        {
            int ti=0;
            for(int z=0;z<nz-1;z++)
            for(int x=0;x<nx-1;x++)
            {
                int a=z*nx+x,b=a+1,d=(z+1)*nx+x,e=d+1;
                if(!flip){tris[ti++]=a;tris[ti++]=d;tris[ti++]=b;tris[ti++]=b;tris[ti++]=d;tris[ti++]=e;}
                else{tris[ti++]=a;tris[ti++]=b;tris[ti++]=d;tris[ti++]=b;tris[ti++]=e;tris[ti++]=d;}
            }
        }
    }
}
