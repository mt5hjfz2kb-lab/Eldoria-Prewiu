using Eldoria.Domain;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Eldoria.Presentation
{
    // Authored full-frame world surround using only existing project inventory.
    // Visual-only: no gameplay colliders, hotspots, routes or progression ownership.
    public static class ValoriaFullFrameWorldSurroundV1
    {
        public static bool Enabled=false;
        const string RootName="Valoria · Full Frame World Surround v1";

        public static void Build(Transform parent,PlayerState state)
        {
            if(!Enabled||parent==null)return;
            var old=GameObject.Find(RootName);
            if(old!=null)Object.DestroyImmediate(old);

            var root=new GameObject(RootName).transform;
            root.SetParent(parent,true);

            ApplySkyAndAtmosphere(root);
            BuildMountainLayers(root);
            BuildForestDepth(root);
            BuildRuinedHorizon(root);
        }

        static void ApplySkyAndAtmosphere(Transform root)
        {
            var skyShader=Shader.Find("Skybox/Procedural");
            if(skyShader!=null)
            {
                var sky=new Material(skyShader){name="Valoria · Full Frame · procedural sky"};
                if(sky.HasProperty("_SkyTint"))sky.SetColor("_SkyTint",new Color(.45f,.64f,.82f));
                if(sky.HasProperty("_GroundColor"))sky.SetColor("_GroundColor",new Color(.31f,.34f,.34f));
                if(sky.HasProperty("_AtmosphereThickness"))sky.SetFloat("_AtmosphereThickness",1.05f);
                if(sky.HasProperty("_Exposure"))sky.SetFloat("_Exposure",1.08f);
                if(sky.HasProperty("_SunSize"))sky.SetFloat("_SunSize",.035f);
                RenderSettings.skybox=sky;
                DynamicGI.UpdateEnvironment();
            }

            var camera=Camera.main;
            if(camera!=null)
            {
                camera.clearFlags=skyShader!=null?CameraClearFlags.Skybox:CameraClearFlags.SolidColor;
                camera.backgroundColor=new Color(.48f,.67f,.80f);
                camera.allowHDR=true;
            }

            RenderSettings.ambientMode=UnityEngine.Rendering.AmbientMode.Trilight;
            RenderSettings.ambientSkyColor=new Color(.65f,.75f,.84f);
            RenderSettings.ambientEquatorColor=new Color(.48f,.49f,.45f);
            RenderSettings.ambientGroundColor=new Color(.23f,.22f,.20f);
            RenderSettings.ambientIntensity=.94f;
            RenderSettings.fog=true;
            RenderSettings.fogMode=FogMode.Linear;
            RenderSettings.fogColor=new Color(.58f,.68f,.76f);
            RenderSettings.fogStartDistance=34f;
            RenderSettings.fogEndDistance=102f;

            foreach(var light in Object.FindObjectsByType<Light>(FindObjectsSortMode.None))
            {
                if(light.type!=LightType.Directional)continue;
                light.color=new Color(1f,.91f,.80f);
                light.intensity=Mathf.Max(light.intensity,1.12f);
                light.shadowStrength=.48f;
                light.shadows=LightShadows.Soft;
                light.transform.rotation=Quaternion.Euler(51f,-31f,0f);
            }

            var fillGo=new GameObject("Valoria · World Surround · cool sky fill");
            fillGo.transform.SetParent(root,true);
            fillGo.transform.rotation=Quaternion.Euler(32f,149f,0f);
            var fill=fillGo.AddComponent<Light>();
            fill.type=LightType.Directional;
            fill.color=new Color(.59f,.72f,.94f);
            fill.intensity=.12f;
            fill.shadows=LightShadows.None;
        }

        static void BuildMountainLayers(Transform root)
        {
            var mountain=Resources.Load<GameObject>("WorldInventory/Mountain01");
            var rock1=Resources.Load<GameObject>("WorldInventory/Rock01");
            var rock2=Resources.Load<GameObject>("WorldInventory/Rock02");
            if(mountain==null)return;

            var rear=new[]{
                new Vector4(-24f,29f,24f,12.5f),
                new Vector4(-11f,32f,22f,14.0f),
                new Vector4(2f,34f,25f,15.0f),
                new Vector4(15f,31f,23f,13.4f),
                new Vector4(28f,28f,24f,12.3f)
            };
            for(int i=0;i<rear.Length;i++)
            {
                var s=rear[i];
                AddPiece(root,mountain,"rear mountain "+i,
                    new Vector3(s.x,-2.8f,s.y),s.z,s.w,180f+(i*37)%160,
                    new Color(.58f,.66f,.69f,1f));
            }

            var mid=new[]{
                new Vector4(-20f,18f,14f,8.0f),
                new Vector4(-8f,21f,13f,7.4f),
                new Vector4(10f,21f,14f,7.8f),
                new Vector4(21f,18f,13f,7.2f)
            };
            for(int i=0;i<mid.Length;i++)
            {
                var s=mid[i];
                AddPiece(root,mountain,"mid mountain "+i,
                    new Vector3(s.x,-1.7f,s.y),s.z,s.w,155f+(i*51)%190,
                    new Color(.46f,.53f,.53f,1f));
            }

            // Real rock shoulders hide hard world-frame edges and bridge city to the authored mountains.
            if(rock1!=null)
            {
                AddPiece(root,rock1,"west shoulder rock",new Vector3(-16.2f,-.45f,8.5f),8.5f,4.3f,33f,new Color(.44f,.46f,.42f,1f));
                AddPiece(root,rock1,"east shoulder rock",new Vector3(16.6f,-.45f,8.8f),8.7f,4.4f,211f,new Color(.44f,.46f,.42f,1f));
            }
            if(rock2!=null)
            {
                AddPiece(root,rock2,"west rear rock",new Vector3(-13.5f,-.4f,14.1f),6.0f,3.1f,74f,new Color(.46f,.48f,.44f,1f));
                AddPiece(root,rock2,"east rear rock",new Vector3(13.8f,-.4f,14.6f),6.1f,3.2f,248f,new Color(.46f,.48f,.44f,1f));
            }
        }

        static void BuildForestDepth(Transform root)
        {
            var treeA=Resources.Load<GameObject>("WorldInventory/Tree01A");
            var treeB=Resources.Load<GameObject>("WorldInventory/Tree01B");
            if(treeA==null&&treeB==null)return;

            for(int i=0;i<28;i++)
            {
                float x=-23.5f+i*1.75f;
                if(Mathf.Abs(x)<4.8f)continue;
                float z=14.8f+(i%4)*1.1f;
                var source=(i%2==0?treeA:treeB)??treeA??treeB;
                float footprint=1.25f+(i%3)*.10f;
                float height=3.1f+(i%5)*.24f;
                AddPiece(root,source,"rear tree "+i,new Vector3(x,-.10f,z),footprint,height,(i*47)%360,
                    new Color(.55f,.67f,.55f,1f));
            }

            for(int i=0;i<16;i++)
            {
                float side=i<8?-1f:1f;
                int k=i%8;
                float x=side*(12.5f+k*1.15f);
                float z=3.5f+k*1.55f;
                var source=(i%2==0?treeA:treeB)??treeA??treeB;
                AddPiece(root,source,"side tree "+i,new Vector3(x,-.08f,z),1.15f,2.8f,(i*61)%360,
                    new Color(.50f,.62f,.50f,1f));
            }
        }

        static void BuildRuinedHorizon(Transform root)
        {
            var arch=Resources.Load<GameObject>("WorldInventory/Arch_Gothic");
            var wall=Resources.Load<GameObject>("WorldInventory/Wall_Broken");

            if(arch!=null)
                AddPiece(root,arch,"distant ruined arch",new Vector3(18.4f,-.25f,20.8f),4.7f,7.6f,205f,
                    new Color(.48f,.47f,.48f,1f));
            if(wall!=null)
            {
                AddPiece(root,wall,"distant ruined wall A",new Vector3(15.7f,-.20f,21.4f),4.0f,4.2f,183f,
                    new Color(.45f,.44f,.46f,1f));
                AddPiece(root,wall,"distant ruined wall B",new Vector3(21.0f,-.20f,22.0f),3.5f,3.8f,228f,
                    new Color(.45f,.44f,.46f,1f));
            }

            var glow=new GameObject("Valoria · World Surround · distant corruption glow");
            glow.transform.SetParent(root,true);
            glow.transform.position=new Vector3(18.2f,5.0f,21.0f);
            var light=glow.AddComponent<Light>();
            light.type=LightType.Point;
            light.color=new Color(.57f,.20f,.76f);
            light.intensity=.75f;
            light.range=6.5f;
            light.shadows=LightShadows.None;
        }

        static void AddPiece(Transform root,GameObject source,string role,Vector3 ground,float footprint,float maxHeight,float yaw,Color tint)
        {
            if(source==null)return;
            var go=ValoriaKit.BenchmarkPieceModulated(
                "Valoria · World Surround · "+role,source,ground,footprint,maxHeight,Quaternion.Euler(0f,yaw,0f),tint);
            if(go==null)return;
            go.transform.SetParent(root,true);
            foreach(var c in go.GetComponentsInChildren<Collider>(true))c.enabled=false;
            foreach(var b in go.GetComponentsInChildren<MonoBehaviour>(true))b.enabled=false;
        }
    }
}
