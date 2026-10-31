using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;
namespace Eldoria.HeroesUI
{
    /// <summary>Live isolated portrait; approved geometry, no importer/gameplay actor/save.</summary>
    public sealed class HeroesModelSnapshot : MonoBehaviour
    {
        RenderTexture texture;GameObject previous,stage,visual;RawImage output;
        Camera camera;Light key,rim;Animation idle;AnimationState idleState;
        Mesh groundMesh;Material groundMaterial;Bounds bounds;float yaw=90,nextRender;
        public bool HasModel=>texture&&previous&&stage;
        public bool HasPerspectiveGround=>camera&&!camera.orthographic&&groundMaterial&&groundMaterial.shader.isSupported;
        public bool SamplesOriginalIdle=>idle&&idleState!=null;
        public void Rotate(float degrees){yaw=Mathf.Repeat(yaw+degrees,360);nextRender=0;}
        public bool Show(GameObject model,RawImage target,float initialYaw=90)
        {
            if(!model||!target)return false;
            try{
                if(previous!=model||!stage){Release();yaw=initialYaw;Build(model);previous=model;}
                output=target;RenderFrame();target.texture=texture;target.enabled=true;return true;
            }catch(Exception error){Debug.LogWarning("HERO_PREVIEW_UNAVAILABLE "+error);Release();return false;}
        }
        void Build(GameObject model)
        {
            stage=new GameObject("Hero portrait / isolated lighting and ground");
            stage.hideFlags=HideFlags.HideAndDontSave;stage.SetActive(false);
            stage.transform.position=new Vector3(0,-12000,0);
            visual=Instantiate(model,stage.transform,false);visual.name="Approved hero / presentation clone";
            visual.transform.localPosition=Vector3.zero;visual.transform.localRotation=Quaternion.identity;
            // Frame original mesh units; the city normalizes Aldric to metres.
            // Inheriting that parent scale makes legacy skin baking count it twice.
            visual.transform.localScale=Vector3.one;
            foreach(var node in visual.GetComponentsInChildren<Transform>(true))node.gameObject.layer=31;
            foreach(var script in visual.GetComponentsInChildren<MonoBehaviour>(true))script.enabled=false;
            foreach(var animator in visual.GetComponentsInChildren<Animator>(true))animator.enabled=false;
            foreach(var renderer in visual.GetComponentsInChildren<Renderer>(true)){
                renderer.enabled=true;renderer.shadowCastingMode=ShadowCastingMode.On;renderer.receiveShadows=true;
                if(renderer is SkinnedMeshRenderer skin)skin.updateWhenOffscreen=true;
            }
            foreach(var animation in visual.GetComponentsInChildren<Animation>(true)){
                animation.playAutomatically=false;animation.Stop();var clip=animation.GetClip("Aldric_Idle_v1");
                if(!clip)continue;
                // Exact approved Aldric has identity bind rotations/scales on all26bones.
                // Reset the copied walking pose before sampling its original Idle.
                foreach(var bone in visual.GetComponentsInChildren<Transform>(true))
                    if(bone.name.StartsWith("Eldoria_Aldric_",StringComparison.Ordinal))
                    {bone.localRotation=Quaternion.identity;bone.localScale=Vector3.one;}
                animation.Play(clip.name,UnityEngine.PlayMode.StopAll);idle=animation;
                idleState=animation[clip.name];idleState.speed=0;idleState.time=0;animation.Sample();
            }
            visual.SetActive(true);stage.SetActive(true);
            // Bind legacy animation on an active hierarchy before measuring its skin.
            if(idle&&idleState!=null){idle.Play(idleState.name);idleState.speed=0;idleState.time=0;idle.Sample();}
            bounds=ActualBounds(visual);
            Debug.Log("HERO_PREVIEW_GEOMETRY "+model.name+" source_scale="+model.transform.lossyScale+" local_stage_bounds="+bounds.size);
            float height=Mathf.Max(.1f,bounds.size.y);
            var shader=Resources.Load<Shader>("EldoriaV41/HeroesContactShadow");
            if(!shader||!shader.isSupported)throw new InvalidOperationException("Hero contact-shadow shader unavailable");
            groundMaterial=new Material(shader){name="Hero / transparent shadow receiver"};
            groundMaterial.SetColor("_ShadowTint",new Color(.035f,.025f,.018f,.72f));
            var floor=new GameObject("Hero / physical ground contact",typeof(MeshFilter),typeof(MeshRenderer));
            floor.transform.SetParent(stage.transform,false);floor.layer=31;
            floor.transform.position=new Vector3(bounds.center.x,bounds.min.y-.002f,bounds.center.z);
            float extent=height*1.8f;groundMesh=new Mesh{name="Hero / four-vertex shadow receiver"};
            groundMesh.vertices=new[]{new Vector3(-extent,0,-extent),new Vector3(-extent,0,extent),new Vector3(extent,0,extent),new Vector3(extent,0,-extent)};
            groundMesh.triangles=new[]{0,1,2,0,2,3};groundMesh.RecalculateNormals();groundMesh.RecalculateBounds();
            floor.GetComponent<MeshFilter>().sharedMesh=groundMesh;
            var ground=floor.GetComponent<MeshRenderer>();ground.sharedMaterial=groundMaterial;
            ground.shadowCastingMode=ShadowCastingMode.Off;ground.receiveShadows=true;
            var cameraObject=new GameObject("Hero / perspective camera");cameraObject.transform.SetParent(stage.transform,false);
            camera=cameraObject.AddComponent<Camera>();camera.enabled=false;camera.orthographic=false;
            camera.fieldOfView=32;camera.aspect=2f/3f;camera.cullingMask=1<<31;
            camera.clearFlags=CameraClearFlags.SolidColor;camera.backgroundColor=Color.clear;
            camera.allowHDR=false;camera.allowMSAA=false;camera.nearClipPlane=.01f;camera.farClipPlane=height*12;
            var data=cameraObject.AddComponent<UniversalAdditionalCameraData>();data.renderPostProcessing=false;data.renderShadows=true;
            key=Lamp("Hero / window key",new Color(.88f,.91f,1f),.95f);
            key.shadows=LightShadows.Soft;key.shadowStrength=.8f;key.shadowBias=.015f;key.shadowNormalBias=.025f;
            rim=Lamp("Hero / torch rim",new Color(1f,.58f,.27f),.55f);
            texture=new RenderTexture(512,768,24,RenderTextureFormat.ARGB32){name="Hero live perspective portrait"};texture.Create();camera.targetTexture=texture;
        }
        Light Lamp(string name,Color color,float intensity){
            var go=new GameObject(name);go.transform.SetParent(stage.transform,false);
            var light=go.AddComponent<Light>();light.type=LightType.Directional;light.cullingMask=1<<31;
            light.color=color;light.intensity=intensity;return light;
        }
        static Bounds ActualBounds(GameObject model){
            var result=default(Bounds);bool found=false;
            foreach(var renderer in model.GetComponentsInChildren<Renderer>(true)){
                if(renderer is SkinnedMeshRenderer skin){
                    var mesh=new Mesh();try{skin.BakeMesh(mesh,false);
                        foreach(var vertex in mesh.vertices){var p=skin.transform.TransformPoint(vertex);
                            if(found)result.Encapsulate(p);else{result=new Bounds(p,Vector3.zero);found=true;}}
                    }finally{Destroy(mesh);}
                }else if(found)result.Encapsulate(renderer.bounds);
                else{result=renderer.bounds;found=true;}
            }
            if(!found)throw new InvalidOperationException("Approved portrait contains no geometry");return result;
        }
        void Update(){if(output&&output.enabled&&output.gameObject.activeInHierarchy&&HasModel&&Time.unscaledTime>=nextRender)RenderFrame();}
        void RenderFrame(){
            if(!camera)return;
            if(idle&&idleState!=null){idleState.time=Mathf.Repeat(Time.unscaledTime,Mathf.Max(.01f,idleState.length));idle.Sample();}
            var rotation=Quaternion.Euler(7,yaw,0);var up=rotation*Vector3.up;var right=rotation*Vector3.right;var e=bounds.extents;
            float extent(Vector3 axis)=>Mathf.Abs(axis.x)*e.x+Mathf.Abs(axis.y)*e.y+Mathf.Abs(axis.z)*e.z;
            float tangent=Mathf.Tan(camera.fieldOfView*.5f*Mathf.Deg2Rad);
            float distance=Mathf.Max(extent(up)/tangent,extent(right)/(tangent*camera.aspect))*1.17f+extent(rotation*Vector3.forward)*.35f;
            camera.transform.rotation=rotation;camera.transform.position=bounds.center-camera.transform.forward*distance;
            key.transform.rotation=Quaternion.Euler(38,yaw-35,0);rim.transform.rotation=Quaternion.Euler(22,yaw+145,0);
            var sun=RenderSettings.sun;var mode=RenderSettings.ambientMode;var ambient=RenderSettings.ambientLight;
            var sky=RenderSettings.ambientSkyColor;var equator=RenderSettings.ambientEquatorColor;var ground=RenderSettings.ambientGroundColor;
            float reflection=RenderSettings.reflectionIntensity;bool fog=RenderSettings.fog;var target=RenderTexture.active;
            var lights=UnityEngine.Object.FindObjectsByType<Light>(FindObjectsSortMode.None);
            var masks=new int[lights.Length];for(int i=0;i<lights.Length;i++)masks[i]=lights[i].cullingMask;
            try{
                for(int i=0;i<lights.Length;i++)if(lights[i]!=key&&lights[i]!=rim)lights[i].cullingMask&=~(1<<31);
                RenderSettings.sun=key;RenderSettings.ambientMode=AmbientMode.Flat;RenderSettings.ambientLight=new Color(.22f,.24f,.29f);
                RenderSettings.reflectionIntensity=.35f;RenderSettings.fog=false;camera.Render();nextRender=Time.unscaledTime+.20f;
            }finally{
                for(int i=0;i<lights.Length;i++)if(lights[i])lights[i].cullingMask=masks[i];
                RenderSettings.sun=sun;RenderSettings.ambientMode=mode;RenderSettings.ambientLight=ambient;
                RenderSettings.ambientSkyColor=sky;RenderSettings.ambientEquatorColor=equator;RenderSettings.ambientGroundColor=ground;
                RenderSettings.reflectionIntensity=reflection;RenderSettings.fog=fog;RenderTexture.active=target;
            }
        }
        // Native visual gate: measure the actual GPU-rendered opaque silhouette,
        // excluding the transparent floor shadow, rather than trusting bounds.
        public float ReadOpaqueHeightCoverage(){
            if(!HasModel)return 0;var prior=RenderTexture.active;
            var pixels=new Texture2D(texture.width,texture.height,TextureFormat.RGBA32,false);
            try{RenderTexture.active=texture;pixels.ReadPixels(new Rect(0,0,texture.width,texture.height),0,0);pixels.Apply();
                var colors=pixels.GetPixels32();int first=texture.height,last=-1;
                for(int y=0;y<texture.height;y++)for(int x=0;x<texture.width;x++)
                    if(colors[y*texture.width+x].a>242){first=Mathf.Min(first,y);last=Mathf.Max(last,y);break;}
                return last<first?0:(last-first+1f)/texture.height;
            }finally{RenderTexture.active=prior;Destroy(pixels);}
        }
        public Sprite CopyPortrait(){
            if(!HasModel)return null;var prior=RenderTexture.active;var copy=new Texture2D(texture.width,texture.height,TextureFormat.RGBA32,false);
            try{RenderTexture.active=texture;copy.ReadPixels(new Rect(0,0,texture.width,texture.height),0,0);copy.Apply();copy.name="Portrait from approved Unity geometry";
                return Sprite.Create(copy,new Rect(copy.width*.20f,copy.height*.48f,copy.width*.60f,copy.height*.48f),new Vector2(.5f,.5f),100,0,SpriteMeshType.FullRect);
            }catch{Destroy(copy);throw;}finally{RenderTexture.active=prior;}
        }
        public void Release(){
            previous=null;output=null;idle=null;idleState=null;camera=null;key=null;rim=null;
            if(stage){stage.SetActive(false);Destroy(stage);}stage=null;visual=null;
            if(groundMesh)Destroy(groundMesh);groundMesh=null;if(groundMaterial)Destroy(groundMaterial);groundMaterial=null;
            if(texture){texture.Release();Destroy(texture);}texture=null;
        }
        void OnDisable()=>Release();void OnDestroy()=>Release();
    }
}
