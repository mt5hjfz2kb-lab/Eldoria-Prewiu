using System;
using System.Collections.Generic;
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
        Camera camera;Light key,rim,fill;Animation idle;AnimationState idleState,poseState;AnimationClip poseClip;
        Mesh groundMesh;Material groundMaterial;Bounds bounds;float yaw=90,nextRender;int renderLayer;
        readonly Transform[] previewShoulders=new Transform[2],previewElbows=new Transform[2],previewWrists=new Transform[2];
        Mesh stonePlinthMesh,bronzePlinthMesh,stoneJointMesh;Material stonePlinthMaterial,bronzePlinthMaterial,stoneJointMaterial;
        public bool HasPhysicalPlinth=>stonePlinthMesh&&bronzePlinthMesh&&stonePlinthMaterial&&bronzePlinthMaterial;
        bool logPoseOnce;
        public bool HasModel=>texture&&previous&&stage;
        public bool HasPerspectiveGround=>camera&&!camera.orthographic&&groundMaterial&&groundMaterial.shader.isSupported;
        public bool SamplesOriginalIdle=>idle&&idleState!=null;
        public void Rotate(float degrees){yaw=Mathf.Repeat(yaw+degrees,360);nextRender=0;}
        public bool Show(GameObject model,RawImage target,float initialYaw=90)
        {
            if(!model||!target)return false;
            try{
                // The one canonical live profile and its inactive portrait cache can
                // coexist while an asynchronous model import completes. Never mix them.
                int layer=target.gameObject.activeInHierarchy?31:30;
                if(previous!=model||!stage||renderLayer!=layer){Release();renderLayer=layer;yaw=initialYaw;Build(model);previous=model;}
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
            foreach(var node in visual.GetComponentsInChildren<Transform>(true))node.gameObject.layer=renderLayer;
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
            LocatePreviewShoulders();logPoseOnce=true;RelaxAldricPreviewArms();
            InstallPresentationPoseAnimation();
            bounds=ActualBounds(visual);
            Debug.Log("HERO_PREVIEW_GEOMETRY "+model.name+" source_scale="+model.transform.lossyScale+" local_stage_bounds="+bounds.size);
            float height=Mathf.Max(.1f,bounds.size.y);
            // Do not spawn the rejected round stage. Original hall floor is
            // the stage surface; this portrait retains real physical contact
            // shadow and matched warm/cool lighting, without a pasted pedestal.
            var shader=Resources.Load<Shader>("EldoriaV41/HeroesContactShadow");
            if(!shader||!shader.isSupported)throw new InvalidOperationException("Hero contact-shadow shader unavailable");
            groundMaterial=new Material(shader){name="Hero / transparent shadow receiver"};
            groundMaterial.SetColor("_ShadowTint",new Color(.035f,.025f,.018f,.72f));
            var floor=new GameObject("Hero / physical ground contact",typeof(MeshFilter),typeof(MeshRenderer));
            floor.transform.SetParent(stage.transform,false);floor.layer=renderLayer;
            floor.transform.position=new Vector3(bounds.center.x,bounds.min.y-.002f,bounds.center.z);
            float extent=height*1.8f;groundMesh=new Mesh{name="Hero / four-vertex shadow receiver"};
            groundMesh.vertices=new[]{new Vector3(-extent,0,-extent),new Vector3(-extent,0,extent),new Vector3(extent,0,extent),new Vector3(extent,0,-extent)};
            groundMesh.triangles=new[]{0,1,2,0,2,3};groundMesh.RecalculateNormals();groundMesh.RecalculateBounds();
            floor.GetComponent<MeshFilter>().sharedMesh=groundMesh;
            var ground=floor.GetComponent<MeshRenderer>();ground.sharedMaterial=groundMaterial;
            ground.shadowCastingMode=ShadowCastingMode.Off;ground.receiveShadows=true;
            var cameraObject=new GameObject("Hero / perspective camera");cameraObject.transform.SetParent(stage.transform,false);
            camera=cameraObject.AddComponent<Camera>();camera.enabled=false;camera.orthographic=false;
            camera.fieldOfView=32;camera.aspect=2f/3f;camera.cullingMask=1<<renderLayer;
            camera.clearFlags=CameraClearFlags.SolidColor;camera.backgroundColor=Color.clear;
            camera.allowHDR=false;camera.allowMSAA=true;camera.nearClipPlane=.01f;camera.farClipPlane=height*12;
            var data=cameraObject.AddComponent<UniversalAdditionalCameraData>();data.renderPostProcessing=false;data.renderShadows=true;
            key=Lamp("Hero / window key",new Color(.96f,.87f,.72f),1.12f);
            key.shadows=LightShadows.Soft;key.shadowStrength=.8f;key.shadowBias=.015f;key.shadowNormalBias=.025f;
            rim=Lamp("Hero / torch rim",new Color(1f,.62f,.33f),.59f);
            fill=Lamp("Hero / soft front fill",new Color(.76f,.82f,.96f),.56f);
            // Source-space supersampling keeps the approved Lyra's facial/cloth
            // detail intact when a 3D-derived still fills a tall gallery card.
            // No extra model, importer, save state or physical camera is added.
            texture=new RenderTexture(1024,1536,24,RenderTextureFormat.ARGB32){
                name="Hero live perspective portrait",filterMode=FilterMode.Bilinear,
                // Multi-sample the ORIGINAL geometry, not a sharpened sprite.
                // Mobile uses 2x to limit memory; desktops use 4x.
                antiAliasing=UnityEngine.Application.isMobilePlatform?2:4};
            texture.Create();camera.targetTexture=texture;
        }
        Light Lamp(string name,Color color,float intensity){
            var go=new GameObject(name);go.transform.SetParent(stage.transform,false);
            var light=go.AddComponent<Light>();light.type=LightType.Directional;light.cullingMask=1<<renderLayer;
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

        // Relax ONLY the isolated display clone. Real city animation, GLB,
        // skin weights and original clips are byte-for-byte untouched.
        // Shoulder -> elbow is measured in world space to avoid the sign
        // error introduced by a previous rig-local +/-35 degree additive clip.
        void LocatePreviewShoulders()
        {
            if(!visual||!idle)return;
            var nodes=visual.GetComponentsInChildren<Transform>(true);
            string[] ids={"Eldoria_Aldric_upperarm_L","Eldoria_Aldric_upperarm_R"};
            for(int i=0;i<2;i++)
            {
                previewShoulders[i]=null;previewElbows[i]=null;
                foreach(var node in nodes)if(node.name==ids[i]){previewShoulders[i]=node;break;}
                var upper=previewShoulders[i];if(!upper)continue;
                Transform fallback=null;
                foreach(var joint in upper.GetComponentsInChildren<Transform>(true))
                {
                    if(joint==upper||(joint.position-upper.position).sqrMagnitude<.00001f)continue;
                    if(!fallback)fallback=joint;
                    string name=joint.name.ToLowerInvariant();
                    if(name.Contains("forearm")||name.Contains("lowerarm")||name.Contains("elbow"))
                    {fallback=joint;break;}
                }
                previewElbows[i]=fallback;
                if(fallback){
                    Transform wrist=null;
                    foreach(var node in fallback.GetComponentsInChildren<Transform>(true)){
                        if(node==fallback||(node.position-fallback.position).sqrMagnitude<.00001f)continue;
                        string n=node.name.ToLowerInvariant();
                        if(n.Contains("hand")||n.Contains("wrist")){wrist=node;break;}
                        if(!wrist)wrist=node;
                    }
                    previewWrists[i]=wrist;
                }
            }
        }
        void RelaxAldricPreviewArms()
        {
            if(!idle)return;
            for(int i=0;i<2;i++)
            {
                var arm=previewShoulders[i];var elbow=previewElbows[i];
                if(!arm||!elbow)continue;
                var downArm=elbow.position-arm.position;
                if(downArm.sqrMagnitude<.00001f)continue;
                var spread=Vector3.ProjectOnPlane(downArm,Vector3.up);
                if(spread.sqrMagnitude<.00001f)continue;
                // Slightly open arms, bent naturally at the existing elbow.
                // The first A/B only changed the framing, not the visibly
                // abducted arms. Aim for a true near-resting silhouette.
                // Original weighted armour visibly retains the A-pose even
                // after the joint reads 10 degrees from vertical. Correct
                // the presentation-only bone beyond the rig's apparent
                // spread so the actual rendered arm hangs beside the body.
                // Native A/B proved a 27-degree inward bone pose still
                // renders the authored A-pose's metal sleeves far out.
                // Compensate that bind-space sleeve silhouette on the
                // throwaway preview only; do not modify gameplay's rig.
                var target=(Vector3.down*.73f-spread.normalized*.68f).normalized;
                float before=Vector3.Angle(downArm,Vector3.down);
                var swing=Quaternion.FromToRotation(downArm.normalized,target);
                arm.rotation=swing*arm.rotation;
                // Arm upper segment is now ~10 degrees off vertical. The
                // original source Idle keeps forearms splayed; reorient only
                // that preview segment so the gauntlets settle beside the
                // thighs rather than forming an unnatural claw silhouette.
                var wrist=previewWrists[i];
                if(wrist){
                    Vector3 lower=wrist.position-elbow.position;
                    if(lower.sqrMagnitude>.00001f){
                        var side=Vector3.ProjectOnPlane(lower,Vector3.up);
                        Vector3 desired=(Vector3.down*.82f-
                            (side.sqrMagnitude>.00001f?side.normalized*.57f:Vector3.zero)).normalized;
                        var wristSwing=Quaternion.FromToRotation(lower.normalized,desired);
                        elbow.rotation=wristSwing*elbow.rotation;
                    }
                }
                if(logPoseOnce){
                    Debug.Log("HERO_PREVIEW_ARM_POSE "+i+" upper="+arm.name+
                        " elbow="+elbow.name+" before="+before.ToString("F1")+
                        " after="+Vector3.Angle(elbow.position-arm.position,Vector3.down).ToString("F1"));
                    Debug.Log("HERO_PREVIEW_WRIST_POSE "+i+" wrist="+(wrist?wrist.name:"missing")+
                        " lower_angle="+(wrist?Vector3.Angle(wrist.position-elbow.position,Vector3.down).ToString("F1"):"n/a"));
                }
            }
        }
        // Genuine lit 3D geometry within the SAME isolated RenderTexture as
        // the original hero. This is not a pasted/plotted 2D ellipse: both
        // hero and stage share real shadows, light, occlusion and camera.
        void BuildPhysicalHeroDais(float height)
        {
            Shader lit=Shader.Find("Universal Render Pipeline/Lit");
            if(!lit)lit=Shader.Find("Universal Render Pipeline/Simple Lit");
            if(!lit)throw new InvalidOperationException("URP hero dais shader unavailable");
            stonePlinthMaterial=new Material(lit){name="Hero / honed blue stone stage"};
            bronzePlinthMaterial=new Material(lit){name="Hero / chased bronze stage edge"};
            stonePlinthMaterial.SetColor("_BaseColor",new Color(.27f,.275f,.29f,1f));
            bronzePlinthMaterial.SetColor("_BaseColor",new Color(.60f,.40f,.18f,1f));
            stoneJointMaterial=new Material(lit){name="Hero / engraved stone joints"};
            stoneJointMaterial.SetColor("_BaseColor",new Color(.55f,.45f,.30f,1f));
            if(stonePlinthMaterial.HasProperty("_Smoothness"))stonePlinthMaterial.SetFloat("_Smoothness",.16f);
            if(bronzePlinthMaterial.HasProperty("_Smoothness"))bronzePlinthMaterial.SetFloat("_Smoothness",.42f);
            var center=new Vector3(bounds.center.x,bounds.min.y,bounds.center.z);
            // Slightly elliptical in perspective; a low solid stone top,
            // physically joined to the hero's foot level, over a gold rim.
            stonePlinthMesh=Disc("Hero / stone dais, shared depth and shadows",
                center,height*.51f,height*.38f,-height*.012f,-height*.077f,stonePlinthMaterial);
            bronzePlinthMesh=Disc("Hero / bronze dais rim",
                center,height*.535f,height*.402f,-height*.034f,-height*.062f,bronzePlinthMaterial);
            stoneJointMesh=StoneJointEngraving(center,height);
        }
        Mesh Disc(string name,Vector3 center,float radiusX,float radiusZ,
            float upper,float lower,Material material)
        {
            const int count=48;
            var go=new GameObject(name,typeof(MeshFilter),typeof(MeshRenderer));
            go.hideFlags=HideFlags.HideAndDontSave;
            go.transform.SetParent(stage.transform,false);
            go.transform.position=center;
            go.layer=renderLayer;
            var vertices=new Vector3[1+count*2];
            var triangles=new int[count*9];vertices[0]=new Vector3(0,upper,0);
            for(int i=0;i<count;i++){
                float angle=i*Mathf.PI*2f/count;
                float px=Mathf.Cos(angle)*radiusX,pz=Mathf.Sin(angle)*radiusZ;
                vertices[1+i]=new Vector3(px,upper,pz);
                vertices[1+count+i]=new Vector3(px,lower,pz);
            }
            int t=0;
            for(int i=0;i<count;i++){
                int next=(i+1)%count,top=1+i,topNext=1+next,
                    bottom=1+count+i,bottomNext=1+count+next;
                triangles[t++]=0;triangles[t++]=topNext;triangles[t++]=top;
                triangles[t++]=top;triangles[t++]=topNext;triangles[t++]=bottom;
                triangles[t++]=topNext;triangles[t++]=bottomNext;triangles[t++]=bottom;
            }
            var mesh=new Mesh{name=name};mesh.vertices=vertices;mesh.triangles=triangles;
            mesh.RecalculateNormals();mesh.RecalculateBounds();
            go.GetComponent<MeshFilter>().sharedMesh=mesh;
            var renderer=go.GetComponent<MeshRenderer>();
            renderer.sharedMaterial=material;
            renderer.receiveShadows=true;renderer.shadowCastingMode=ShadowCastingMode.On;
            return mesh;
        }
        Mesh StoneJointEngraving(Vector3 center,float height)
        {
            // Twelve actual shallow 3D radial stone joints on the dais top,
            // lit by the portrait lights, not painted into the background.
            const int segments=12;
            var vertices=new Vector3[segments*4];
            var triangles=new int[segments*6];
            float top=-height*.0107f,inner=height*.17f,outer=height*.474f;
            float half=Mathf.Max(.0013f,height*.0020f);
            for(int i=0;i<segments;i++){
                float angle=i*Mathf.PI*2f/segments;
                Vector3 axis=new Vector3(Mathf.Cos(angle),0,Mathf.Sin(angle));
                Vector3 tangent=new Vector3(-axis.z,0,axis.x);
                float scaleZ=.38f/.51f;
                Vector3 a=Vector3.Scale(axis*inner,new Vector3(1,1,scaleZ));
                Vector3 b=Vector3.Scale(axis*outer,new Vector3(1,1,scaleZ));
                Vector3 offset=Vector3.Scale(tangent*half,new Vector3(1,1,scaleZ));
                int v=i*4,t=i*6;
                vertices[v]=a-offset+Vector3.up*top;
                vertices[v+1]=b-offset+Vector3.up*top;
                vertices[v+2]=b+offset+Vector3.up*top;
                vertices[v+3]=a+offset+Vector3.up*top;
                triangles[t]=v;triangles[t+1]=v+2;triangles[t+2]=v+1;
                triangles[t+3]=v;triangles[t+4]=v+3;triangles[t+5]=v+2;
            }
            var mesh=new Mesh{name="Hero / physically engraved dais joints"};
            mesh.vertices=vertices;mesh.triangles=triangles;
            mesh.RecalculateNormals();mesh.RecalculateBounds();
            var go=new GameObject(mesh.name,typeof(MeshFilter),typeof(MeshRenderer));
            go.hideFlags=HideFlags.HideAndDontSave;
            go.transform.SetParent(stage.transform,false);
            go.transform.position=center;go.layer=renderLayer;
            go.GetComponent<MeshFilter>().sharedMesh=mesh;
            var renderer=go.GetComponent<MeshRenderer>();
            renderer.sharedMaterial=stoneJointMaterial;renderer.receiveShadows=true;
            renderer.shadowCastingMode=ShadowCastingMode.Off;
            return mesh;
        }
        // The legacy Animation component owns final skinning before the
        // camera render. Mutating Transform.rotation after Animation.Sample
        // changed logged joint angles but the GPU mesh remained in the A-pose
        // in 8 genuine captures. Give the presentation clone an ANIMATION
        // LAYER, just as the already working city rig does, rather than
        // repeatedly applying out-of-band transform rotations.
        void InstallPresentationPoseAnimation()
        {
            if(!idle||!previewShoulders[0]||!previewShoulders[1])return;
            var clip=new AnimationClip {legacy=true,
                name="Aldric_Profile_ArmsDown_PresentationOnly",
                wrapMode=WrapMode.ClampForever};
            try
            {
                int joints=0;
                for(int i=0;i<2;i++)
                {
                    var bones=new[]{previewShoulders[i],previewElbows[i]};
                    foreach(var bone in bones)
                    {
                        if(!bone)continue;
                        var cursor=bone;var segments=new List<string>();
                        while(cursor&&cursor!=idle.transform)
                        {
                            segments.Add(cursor.name);
                            cursor=cursor.parent;
                        }
                        if(!cursor)throw new InvalidOperationException(
                            "Presentation joint outside approved clip root: "+bone.name);
                        segments.Reverse();
                        var path=string.Join("/",segments);
                        var q=bone.localRotation;
                        float[] values={q.x,q.y,q.z,q.w};
                        string[] properties={"localRotation.x","localRotation.y",
                            "localRotation.z","localRotation.w"};
                        for(int c=0;c<4;c++)
                            clip.SetCurve(path,typeof(Transform),properties[c],
                                AnimationCurve.Constant(0f,1f,values[c]));
                        joints++;
                    }
                }
                if(joints!=4)throw new InvalidOperationException(
                    "Aldric presentation requires two shoulders and two forearms: "+joints);
                clip.EnsureQuaternionContinuity();
                idle.AddClip(clip,clip.name);
                var state=idle[clip.name];
                state.layer=1;
                state.blendMode=AnimationBlendMode.Blend;
                state.wrapMode=WrapMode.ClampForever;
                state.speed=0f;state.time=0f;state.weight=1f;
                for(int i=0;i<2;i++){
                    state.AddMixingTransform(previewShoulders[i],false);
                    state.AddMixingTransform(previewElbows[i],false);
                }
                idle.Play(clip.name,UnityEngine.PlayMode.StopSameLayer);
                state.speed=0f;state.time=0f;state.weight=1f;
                poseState=state;poseClip=clip;
                idle.Sample();
                Debug.Log("HERO_PREVIEW_POSE_LAYER_ACTIVE "+clip.name+
                    " joints="+joints+" weight="+state.weight);
            }
            catch(Exception error)
            {
                Debug.LogError("HERO_PREVIEW_POSE_LAYER_FAILED "+error);
                if(clip)Destroy(clip);
                throw;
            }
        }
        void Update(){if(output&&output.enabled&&output.gameObject.activeInHierarchy&&HasModel&&Time.unscaledTime>=nextRender)RenderFrame();}
        void RenderFrame(){
            if(!camera)return;
            logPoseOnce=false;
            if(idle&&idleState!=null){
                idleState.time=Mathf.Repeat(Time.unscaledTime,Mathf.Max(.01f,idleState.length));
                if(poseState!=null){poseState.time=0f;poseState.weight=1f;}
                idle.Sample();
            }
            var rotation=Quaternion.Euler(7,yaw,0);var up=rotation*Vector3.up;var right=rotation*Vector3.right;var e=bounds.extents;
            float extent(Vector3 axis)=>Mathf.Abs(axis.x)*e.x+Mathf.Abs(axis.y)*e.y+Mathf.Abs(axis.z)*e.z;
            float tangent=Mathf.Tan(camera.fieldOfView*.5f*Mathf.Deg2Rad);
            float distance=Mathf.Max(extent(up)/tangent,extent(right)/(tangent*camera.aspect))*1.17f+extent(rotation*Vector3.forward)*.35f;
            camera.transform.rotation=rotation;camera.transform.position=bounds.center-camera.transform.forward*distance;
            key.transform.rotation=Quaternion.Euler(38,yaw-35,0);rim.transform.rotation=Quaternion.Euler(22,yaw+145,0);fill.transform.rotation=Quaternion.Euler(10,yaw,0);
            var sun=RenderSettings.sun;var mode=RenderSettings.ambientMode;var ambient=RenderSettings.ambientLight;
            var sky=RenderSettings.ambientSkyColor;var equator=RenderSettings.ambientEquatorColor;var ground=RenderSettings.ambientGroundColor;
            float reflection=RenderSettings.reflectionIntensity;bool fog=RenderSettings.fog;var target=RenderTexture.active;
            var lights=UnityEngine.Object.FindObjectsByType<Light>(FindObjectsSortMode.None);
            var masks=new int[lights.Length];for(int i=0;i<lights.Length;i++)masks[i]=lights[i].cullingMask;
            try{
                for(int i=0;i<lights.Length;i++)if(lights[i]!=key&&lights[i]!=rim&&lights[i]!=fill)lights[i].cullingMask&=~(1<<renderLayer);
                RenderSettings.sun=key;RenderSettings.ambientMode=AmbientMode.Flat;RenderSettings.ambientLight=new Color(.30f,.32f,.37f);
                RenderSettings.reflectionIntensity=.35f;RenderSettings.fog=false;camera.Render();nextRender=Time.unscaledTime+.20f;
            }finally{
                for(int i=0;i<lights.Length;i++)if(lights[i])lights[i].cullingMask=masks[i];
                RenderSettings.sun=sun;RenderSettings.ambientMode=mode;RenderSettings.ambientLight=ambient;
                RenderSettings.ambientSkyColor=sky;RenderSettings.ambientEquatorColor=equator;RenderSettings.ambientGroundColor=ground;
                RenderSettings.reflectionIntensity=reflection;RenderSettings.fog=fog;RenderTexture.active=target;
            }
        }
        // Explicitly resolve the multisampled GPU portrait into a single-sample
        // temporary texture before any ReadPixels. Direct CPU reads from an
        // MSAA surface are not portable across Unity graphics backends.
        RenderTexture CpuReadablePortrait()
        {
            if(!texture||texture.antiAliasing<=1)return texture;
            var resolved=RenderTexture.GetTemporary(texture.width,texture.height,0,
                RenderTextureFormat.ARGB32);
            resolved.filterMode=FilterMode.Bilinear;
            Graphics.Blit(texture,resolved);
            return resolved;
        }
        void ReleaseReadablePortrait(RenderTexture resolved)
        {
            if(resolved&&resolved!=texture)RenderTexture.ReleaseTemporary(resolved);
        }
        // Native visual gate: measure the actual GPU-rendered opaque silhouette,
        // excluding the transparent floor shadow, rather than trusting bounds.
        public float ReadOpaqueHeightCoverage(){
            if(!HasModel)return 0;var prior=RenderTexture.active;
            var resolved=CpuReadablePortrait();
            var pixels=new Texture2D(texture.width,texture.height,TextureFormat.RGBA32,false);
            try{RenderTexture.active=resolved;pixels.ReadPixels(new Rect(0,0,texture.width,texture.height),0,0);pixels.Apply();
                var colors=pixels.GetPixels32();int first=texture.height,last=-1;
                for(int y=0;y<texture.height;y++)for(int x=0;x<texture.width;x++)
                    if(colors[y*texture.width+x].a>242){first=Mathf.Min(first,y);last=Mathf.Max(last,y);break;}
                return last<first?0:(last-first+1f)/texture.height;
            }finally{RenderTexture.active=prior;ReleaseReadablePortrait(resolved);Destroy(pixels);}
        }
        public Sprite CopyPortrait(){
            if(!HasModel)return null;var prior=RenderTexture.active;
            var resolved=CpuReadablePortrait();
            var copy=new Texture2D(texture.width,texture.height,TextureFormat.RGBA32,false);
            try{RenderTexture.active=resolved;copy.ReadPixels(new Rect(0,0,texture.width,texture.height),0,0);copy.Apply();copy.name="Portrait from approved Unity geometry";
                copy.filterMode=FilterMode.Bilinear;copy.wrapMode=TextureWrapMode.Clamp;
                return Sprite.Create(copy,new Rect(copy.width*.20f,copy.height*.48f,copy.width*.60f,copy.height*.48f),new Vector2(.5f,.5f),100,0,SpriteMeshType.FullRect);
            }catch{Destroy(copy);throw;}finally{RenderTexture.active=prior;ReleaseReadablePortrait(resolved);}
        }
        public void Release(){
            previous=null;output=null;idle=null;idleState=null;poseState=null;
            if(poseClip)Destroy(poseClip);poseClip=null;
            camera=null;key=null;rim=null;fill=null;
            for(int i=0;i<2;i++){previewShoulders[i]=null;previewElbows[i]=null;previewWrists[i]=null;}
            if(stoneJointMesh)Destroy(stoneJointMesh);stoneJointMesh=null;
            if(stoneJointMaterial)Destroy(stoneJointMaterial);stoneJointMaterial=null;
            if(stonePlinthMesh)Destroy(stonePlinthMesh);stonePlinthMesh=null;
            if(bronzePlinthMesh)Destroy(bronzePlinthMesh);bronzePlinthMesh=null;
            if(stonePlinthMaterial)Destroy(stonePlinthMaterial);stonePlinthMaterial=null;
            if(bronzePlinthMaterial)Destroy(bronzePlinthMaterial);bronzePlinthMaterial=null;
            if(stage){stage.SetActive(false);Destroy(stage);}stage=null;visual=null;
            if(groundMesh)Destroy(groundMesh);groundMesh=null;if(groundMaterial)Destroy(groundMaterial);groundMaterial=null;
            if(texture){texture.Release();Destroy(texture);}texture=null;
        }
        void OnDisable()=>Release();void OnDestroy()=>Release();
    }
}
