using System;
using System.Reflection;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Eldoria.DailyTreasures
{
    /// <summary>
    /// One-frame cinematic close-up of the ACTUAL active Bastion in the
    /// player's own loaded city. No invented building, future tier, loot,
    /// screenshot upscaling or mutation of gameplay camera/state.
    /// The optional GPU texture is owned and destroyed by DailyTreasurePanel.
    /// </summary>
    public static class DailyTreasureBastionCamera
    {
        public const string SceneKey="RealBastion/LiveAuthenticatedCamera";
        public const string PresentationMethod="ACTUAL_BUILT_GEOMETRY_SCOPED_STUDIO";
        public static bool LastSceneStateRestored { get; private set; }
        public static int LastStudioRendererCount { get; private set; }

        // The saved city supports TWO genuine views, and tier V has a
        // different ACTIVE real model, not BastionTwo! The old resolver
        // found the tier-I/II city owner only; at Bastion V the progressive
        // world presentation explicitly disables those legacy renderers,
        // leading to a black stage. Read only LIVE existing meshes.
        // These scene hosts are in the foreign Assembly-CSharp; reflection
        // is necessary from the isolated DailyTreasures asmdef.
        static object ReadMember(object target,string member)
        {
            if(target==null)return null;
            var type=target.GetType();
            return type.GetProperty(member,
                BindingFlags.Instance|BindingFlags.Public)
                ?.GetValue(target,null) ??
                type.GetField(member,BindingFlags.Instance|
                    BindingFlags.Public)?.GetValue(target);
        }

        static int SavedBastionLevel(object city)
        {
            // A level-V achievement must never show the obsolete tier-II
            // building during the single frame while the actual
            // BastionIVVWorldPresentation is still instantiating.
            var gateway=ReadMember(city,"Gateway");
            var snapshot=gateway?.GetType().GetMethod("Snapshot",
                BindingFlags.Public|BindingFlags.Instance)
                ?.Invoke(gateway,null);
            var level=ReadMember(snapshot,"BastionLevel");
            return level is int committed ? committed : -1;
        }

        static bool TryActiveBounds(GameObject root,Camera camera,
            out Bounds result)
        {
            result=default;
            if(!root || !root.activeInHierarchy || !camera ||
                root.scene!=camera.gameObject.scene)return false;
            bool any=false;
            foreach(var r in root.GetComponentsInChildren<Renderer>(true))
            {
                if(!r || !r.enabled || !r.gameObject.activeInHierarchy)
                    continue;
                if((camera.cullingMask&(1<<r.gameObject.layer))==0)
                    continue;
                if(!any){result=r.bounds;any=true;}
                else result.Encapsulate(r.bounds);
            }
            return any && result.size.sqrMagnitude>.01f;
        }

        static bool TryCaptureOwner(object owner,Camera camera,
            string[] slots,out Bounds bounds,out GameObject root)
        {
            bounds=default;root=null;
            if(owner==null||!camera)return false;
            foreach(var name in slots)
            {
                var building=ReadMember(owner,name) as GameObject;
                if(TryActiveBounds(building,camera,out bounds)){root=building;return true;}
            }
            return false;
        }

        static bool Resolve(out Camera source,out Bounds bounds,out GameObject root)
        {
            source=null;bounds=default;root=null;
            var components=UnityEngine.Object.FindObjectsByType<MonoBehaviour>(
                FindObjectsInactive.Include,FindObjectsSortMode.None);
            bool pendingCanonicalTierFive=false;
            // FIRST: true playable owner. It chooses between city and region
            // and can display the real level-V Bastion via BastionIVVWorld-
            // Presentation, replacing WorldBastionTwo or City.BastionTwo.
            // Never show a tier-II building as the milestone for level V.
            foreach(var item in components)
            {
                if(!item || !item.isActiveAndEnabled ||
                   item.GetType().Name!="ValoriaIntegratedWorld" ||
                   !item.gameObject.scene.isLoaded)continue;
                var ready=ReadMember(item,"Ready");
                if(!(ready is bool) || !(bool)ready)continue;
                var camera=ReadMember(item,"PlayerCamera") as Camera;
                if(!camera || !camera.gameObject.scene.isLoaded)continue;
                bool inWorld=ReadMember(item,"InWorld") is bool value && value;
                var city=ReadMember(item,"City");
                int builtLevel=SavedBastionLevel(city);
                MonoBehaviour progression=null;
                foreach(var component in item.GetComponents<MonoBehaviour>())
                    if(component &&
                       component.GetType().Name=="BastionIVVWorldPresentation")
                    {progression=component;break;}
                if(builtLevel>=5 && progression &&
                   progression.isActiveAndEnabled)
                {
                    // WorldModel is the genuine active tier-V render. In the
                    // city the real replacement is ModelFor("bastion").
                    GameObject tierFive=null;
                    if(inWorld)
                        tierFive=ReadMember(progression,"WorldModel") as GameObject;
                    else
                    {
                        var method=progression.GetType().GetMethod("ModelFor",
                            BindingFlags.Instance|BindingFlags.Public);
                        tierFive=method?.Invoke(progression,
                            new object[]{"bastion"}) as GameObject;
                    }
                    if(TryActiveBounds(tierFive,camera,out bounds))
                    {source=camera;root=tierFive;return true;}
                }
                if(builtLevel>=5)
                {
                    // The authentic V replacement is not ready: do not
                    // misrepresent this recorded milestone with old II art.
                    pendingCanonicalTierFive=true;
                    continue;
                }
                if(inWorld)
                {
                    // Real owned world geometry, not invented fake castle.
                    if(TryCaptureOwner(item,camera,
                       new[]{"WorldBastionTwo","WorldBastionOne"},out bounds,out root))
                    {source=camera;return true;}
                }
                else
                {
                    if(TryCaptureOwner(city,camera,
                       new[]{"BastionTwo","BastionOne"},out bounds,out root))
                    {source=camera;return true;}
                }
            }
            // A loaded integrated game is tier V but its *authentic* model
            // has not appeared: return false so the same CITY is shown
            // through the transparent heritage frame until V loads.
            if(pendingCanonicalTierFive)return false;
            // SECOND: native editor's genuine standalone city scene without
            // an initialized integrated world. Retain existing validated
            // test coverage and original active player camera.
            foreach(var item in components)
            {
                if(!item || !item.isActiveAndEnabled ||
                   item.GetType().Name!="ValoriaReferenceCleanGameplay" ||
                   !item.gameObject.scene.isLoaded)continue;
                // The standalone test can also load a saved level V before
                // the specialised progression class is initialised. Never
                // disguise a level-V reward with the old tier-II mesh.
                if(SavedBastionLevel(item)>=5)continue;
                var camera=ReadMember(item,"PlayerCamera") as Camera;
                if(TryCaptureOwner(item,camera,
                   new[]{"BastionTwo","BastionOne"},out bounds,out root))
                {source=camera;return true;}
            }
            Debug.LogWarning(
                "TRUE_BASTION_NOT_ACTIVE: cannot manufacture ceremonial architecture");
            return false;
        }

        // The original city camera is a useful gameplay isometric view,
        // but its near-vertical angle produced a tiny, emotionally flat
        // building in the REAL 43005395878 captures. This ceremonial camera
        // should reveal the already-constructed facade and depth: a 32-degree
        // perspective 3/4 portrait lit by actual scene geometry. No invented
        // mesh, replacement asset, panorama or altered player camera.
        static void Frame(Camera camera,Bounds b,DailyTreasureStageLayout stage,
            float photoWidth,float photoHeight,out Rect subjectViewport)
        {
            var heading=Vector3.ProjectOnPlane(-camera.transform.forward,Vector3.up);
            if(heading.sqrMagnitude<.0001f)heading=Vector3.forward;
            heading.Normalize();
            heading=Quaternion.AngleAxis(35f,Vector3.up)*heading;
            const float elevationDegrees=24f;
            float elevation=elevationDegrees*Mathf.Deg2Rad;
            var away=heading*Mathf.Cos(elevation)+Vector3.up*Mathf.Sin(elevation);
            camera.orthographic=false;camera.usePhysicalProperties=false;
            camera.fieldOfView=32f;
            camera.ResetProjectionMatrix();camera.ResetWorldToCameraMatrix();
            camera.transform.rotation=Quaternion.LookRotation(-away,Vector3.up);
            float tanY=Mathf.Tan(camera.fieldOfView*Mathf.Deg2Rad*.5f);
            float tanX=tanY*camera.aspect;
            var right=camera.transform.right;var up=camera.transform.up;
            var forward=camera.transform.forward;
            var points=new Vector3[8];var corners=new DailyTreasureFrustumPoint[8];
            int index=0;
            for(int x=-1;x<=1;x+=2)for(int y=-1;y<=1;y+=2)for(int z=-1;z<=1;z+=2)
            {
                var p=new Vector3(b.extents.x*x,b.extents.y*y,b.extents.z*z);
                points[index]=b.center+p;
                corners[index++]=new DailyTreasureFrustumPoint(
                    Vector3.Dot(right,p),Vector3.Dot(up,p),Vector3.Dot(forward,p));
            }
            var placement=DailyTreasureStageDirection.FitBuiltBastion(
                stage,photoWidth,photoHeight,tanX,tanY,corners);
            float distance=placement.Distance;
            camera.transform.position=b.center+away*distance+
                right*placement.RightOffset+up*placement.UpOffset;
            camera.nearClipPlane=.12f;
            camera.farClipPlane=Mathf.Max(camera.farClipPlane,
                distance+b.extents.magnitude*7f);
            float minX=1f,minY=1f,maxX=0f,maxY=0f;
            foreach(var point in points)
            {
                var v=camera.WorldToViewportPoint(point);
                if(v.z<=camera.nearClipPlane || v.x<0f || v.x>1f || v.y<0f || v.y>1f)
                    throw new InvalidOperationException("REAL_BASTION_OUTSIDE_CONTINUOUS_LENS");
                minX=Mathf.Min(minX,v.x);minY=Mathf.Min(minY,v.y);
                maxX=Mathf.Max(maxX,v.x);maxY=Mathf.Max(maxY,v.y);
            }
            subjectViewport=Rect.MinMaxRect(minX,minY,maxX,maxY);
            var reserved=new Rect(.5f+(stage.SceneX-stage.SceneW*.5f)/photoWidth,
                .5f+(stage.SceneY-stage.SceneH*.5f)/photoHeight,
                stage.SceneW/photoWidth,stage.SceneH/photoHeight);
            if(subjectViewport.xMin<reserved.xMin-.002f ||
               subjectViewport.xMax>reserved.xMax+.002f ||
               subjectViewport.yMin<reserved.yMin-.002f ||
               subjectViewport.yMax>reserved.yMax+.002f)
                throw new InvalidOperationException("REAL_BASTION_CROSSES_COMMITTED_STORY_AREA");
        }


        readonly struct FogSnapshot
        {
            readonly bool enabled;
            readonly FogMode mode;
            readonly Color color;
            readonly float density,start,end;
            readonly AmbientMode ambientMode;
            readonly Color ambientLight;
            readonly float ambientIntensity;
            readonly SphericalHarmonicsL2 ambientProbe;
            public FogSnapshot(bool capture)
            {
                enabled=RenderSettings.fog;mode=RenderSettings.fogMode;
                color=RenderSettings.fogColor;density=RenderSettings.fogDensity;
                start=RenderSettings.fogStartDistance;end=RenderSettings.fogEndDistance;
                ambientMode=RenderSettings.ambientMode;ambientLight=RenderSettings.ambientLight;
                ambientIntensity=RenderSettings.ambientIntensity;ambientProbe=RenderSettings.ambientProbe;
            }
            public void Restore()
            {
                RenderSettings.fog=enabled;RenderSettings.fogMode=mode;
                RenderSettings.fogColor=color;RenderSettings.fogDensity=density;
                RenderSettings.fogStartDistance=start;RenderSettings.fogEndDistance=end;
                RenderSettings.ambientMode=ambientMode;RenderSettings.ambientLight=ambientLight;
                RenderSettings.ambientIntensity=ambientIntensity;RenderSettings.ambientProbe=ambientProbe;
            }
        }

        // A presentation of the CURRENT constructed mesh, not another castle
        // or a photograph. No source mesh, material, transform or player state
        // is changed. Re-layer only the live renderers for this synchronous GPU
        // request, then restore every original layer and source light state.
        sealed class ActualBuiltStudio : IDisposable
        {
            readonly Dictionary<GameObject,int> layers=new Dictionary<GameObject,int>();
            readonly Dictionary<Light,bool> lights=new Dictionary<Light,bool>();
            readonly GameObject root;
            readonly int layer;
            GameObject plinth,key,fill;
            Mesh mesh;
            Material material;
            public int RendererCount { get; private set; }
            public int Mask=>1<<layer;
            public bool Restored { get; private set; }
            public ActualBuiltStudio(GameObject built)
            {
                root=built;
                var occupied=new HashSet<int>();
                foreach(var t in UnityEngine.Object.FindObjectsByType<Transform>(
                    FindObjectsInactive.Include,FindObjectsSortMode.None))
                    if(t)occupied.Add(t.gameObject.layer);
                // Unity reserves layer31 for Editor Preview. It is never a
                // candidate for either actual geometry or the volume mask.
                int chosen=-1;
                for(int candidate=30;candidate>=8;candidate--)
                    if(!occupied.Contains(candidate)){chosen=candidate;break;}
                if(chosen<0)throw new InvalidOperationException("NO_PRIVATE_STUDIO_RENDER_LAYER");
                layer=chosen;
                foreach(var r in built.GetComponentsInChildren<Renderer>(true))
                    if(r && r.enabled && r.gameObject.activeInHierarchy)
                    {layers[r.gameObject]=r.gameObject.layer;RendererCount++;}
                if(RendererCount<1)throw new InvalidOperationException("NO_ACTUAL_BUILT_STUDIO_GEOMETRY");
                foreach(var light in UnityEngine.Object.FindObjectsByType<Light>(
                    FindObjectsInactive.Include,FindObjectsSortMode.None))
                    if(light)lights[light]=light.enabled;
            }
            public void Begin(Bounds bounds,Camera camera)
            {
                foreach(var item in layers)item.Key.layer=layer;
                // Exclude the broad gameplay sun from this one snapshot.
                // Real torch lights owned by this actual building stay real.
                foreach(var item in lights)
                    if(item.Key && !item.Key.transform.IsChildOf(root.transform))
                        item.Key.enabled=false;
                RenderSettings.ambientMode=AmbientMode.Flat;
                RenderSettings.ambientLight=new Color(.16f,.18f,.23f,1f);
                RenderSettings.ambientIntensity=.65f;
                var sh=new SphericalHarmonicsL2();
                sh.AddAmbientLight(new Color(.16f,.18f,.23f,1f));
                RenderSettings.ambientProbe=sh;
                BuildPlinth(bounds);
                float radius=Mathf.Max(1f,bounds.extents.magnitude);
                key=new GameObject("Bastion studio / scoped warm physical key");
                key.layer=layer;var main=key.AddComponent<Light>();
                main.GetUniversalAdditionalLightData();
                main.type=LightType.Directional;main.color=new Color(1f,.89f,.72f);
                main.intensity=1.65f;main.cullingMask=Mask;
                main.shadows=LightShadows.Soft;main.shadowStrength=.72f;
                main.shadowBias=.04f;main.shadowNormalBias=.20f;
                var keyPosition=bounds.center-camera.transform.forward*radius+
                    camera.transform.right*(-radius*.9f)+Vector3.up*radius*1.4f;
                key.transform.rotation=Quaternion.LookRotation(bounds.center-keyPosition,Vector3.up);
                fill=new GameObject("Bastion studio / scoped cool edge reflection");
                fill.layer=layer;var edge=fill.AddComponent<Light>();
                edge.GetUniversalAdditionalLightData();
                edge.type=LightType.Point;edge.color=new Color(.48f,.61f,.82f);
                edge.intensity=1.15f;edge.range=radius*4f;
                edge.cullingMask=Mask;edge.shadows=LightShadows.None;
                fill.transform.position=bounds.center+camera.transform.right*radius+
                    Vector3.up*radius*.55f+camera.transform.forward*radius*.25f;
                camera.cullingMask=Mask;
            }
            void BuildPlinth(Bounds bounds)
            {
                // Neutral ceremonial display geometry, not a city building.
                // The current real foundation and all source details remain.
                const int segments=80;
                float radius=Mathf.Max(bounds.extents.x,bounds.extents.z)*1.18f;
                float depth=Mathf.Max(.12f,bounds.size.y*.045f);
                var vertices=new List<Vector3>();var normals=new List<Vector3>();
                var uv=new List<Vector2>();var triangles=new List<int>();
                float top=bounds.min.y-.025f;
                Vector3 center=new Vector3(bounds.center.x,top,bounds.center.z);
                for(int i=0;i<segments;i++)
                {
                    float a=i*Mathf.PI*2f/segments,b=(i+1)*Mathf.PI*2f/segments;
                    var na=new Vector3(Mathf.Cos(a),0f,Mathf.Sin(a));
                    var nb=new Vector3(Mathf.Cos(b),0f,Mathf.Sin(b));
                    var ta=center+na*radius;var tb=center+nb*radius;
                    int n=vertices.Count;
                    vertices.Add(center);vertices.Add(tb);vertices.Add(ta);
                    normals.Add(Vector3.up);normals.Add(Vector3.up);normals.Add(Vector3.up);
                    uv.Add(new Vector2(.5f,.5f));uv.Add(new Vector2(nb.x*.5f+.5f,nb.z*.5f+.5f));
                    uv.Add(new Vector2(na.x*.5f+.5f,na.z*.5f+.5f));
                    triangles.Add(n);triangles.Add(n+1);triangles.Add(n+2);
                    n=vertices.Count;
                    vertices.Add(ta);vertices.Add(tb);vertices.Add(ta-Vector3.up*depth);vertices.Add(tb-Vector3.up*depth);
                    normals.Add(na);normals.Add(nb);normals.Add(na);normals.Add(nb);
                    uv.Add(new Vector2(0,1));uv.Add(new Vector2(1,1));uv.Add(Vector2.zero);uv.Add(Vector2.right);
                    triangles.Add(n);triangles.Add(n+1);triangles.Add(n+2);
                    triangles.Add(n+1);triangles.Add(n+3);triangles.Add(n+2);
                }
                mesh=new Mesh{name="Bastion studio / neutral display plinth only"};
                mesh.SetVertices(vertices);mesh.SetNormals(normals);mesh.SetUVs(0,uv);mesh.SetTriangles(triangles,0);
                mesh.RecalculateBounds();
                var shader=Shader.Find("Universal Render Pipeline/Lit");
                if(!shader)throw new InvalidOperationException("STUDIO_LIT_SHADER_UNAVAILABLE");
                material=new Material(shader){name="Bastion studio / transient matte charcoal"};
                material.SetColor("_BaseColor",new Color(.075f,.067f,.060f,1f));
                material.SetFloat("_Metallic",.12f);material.SetFloat("_Smoothness",.22f);
                plinth=new GameObject("Bastion studio / temporary ceremonial display");
                plinth.layer=layer;plinth.hideFlags=HideFlags.HideAndDontSave;
                plinth.AddComponent<MeshFilter>().sharedMesh=mesh;
                var renderer=plinth.AddComponent<MeshRenderer>();
                renderer.sharedMaterial=material;renderer.receiveShadows=true;
                renderer.shadowCastingMode=ShadowCastingMode.On;
            }
            public void Dispose()
            {
                bool exact=true;
                foreach(var item in layers)
                {
                    if(!item.Key){exact=false;continue;}
                    item.Key.layer=item.Value;
                    exact&=item.Key.layer==item.Value;
                }
                foreach(var item in lights)
                {
                    if(!item.Key){exact=false;continue;}
                    item.Key.enabled=item.Value;
                    exact&=item.Key.enabled==item.Value;
                }
                DestroyTemporary(plinth);DestroyTemporary(key);DestroyTemporary(fill);
                DestroyTemporary(mesh);DestroyTemporary(material);
                Restored=exact;
            }
        }

        static int UnusedVolumeLayer()
        {
            var volumes=UnityEngine.Object.FindObjectsByType<Volume>(
                FindObjectsInactive.Include,FindObjectsSortMode.None);
            for(int layer=30;layer>=8;layer--)
            {
                bool occupied=false;
                foreach(var volume in volumes)
                    if(volume && volume.gameObject.layer==layer){occupied=true;break;}
                if(!occupied)return layer;
            }
            throw new InvalidOperationException("NO_ISOLATED_CEREMONY_VOLUME_LAYER");
        }

        // One-shot optical treatment of the SAME real world. It begins beyond
        // the complete built masonry bounds, hiding the finite terrain/clear
        // colour discontinuity rather than painting invented architecture.
        static void BeginCeremonialEnvironment(Camera camera,Bounds bounds)
        {
            var horizon=new Color(.045f,.041f,.051f,1f);
            float radius=Mathf.Max(1f,bounds.extents.magnitude);
            float distance=Vector3.Distance(camera.transform.position,bounds.center);
            RenderSettings.fog=true;RenderSettings.fogMode=FogMode.Linear;
            RenderSettings.fogColor=horizon;
            RenderSettings.fogStartDistance=distance+radius*1.15f;
            RenderSettings.fogEndDistance=distance+radius*3.65f;
            camera.clearFlags=CameraClearFlags.SolidColor;
            camera.backgroundColor=horizon;
        }

        static void DestroyTemporary(UnityEngine.Object obj)
        {
            if(!obj)return;
            // Destroy is deferred until the end of the frame in PlayMode.
            // Several synchronous viewport captures would otherwise retain
            // enabled amber lights from earlier shots and change the actual
            // city lighting. Deactivate NOW, then safely destroy later.
            if(obj is GameObject temporary)temporary.SetActive(false);
            if(UnityEngine.Application.isPlaying)
                UnityEngine.Object.Destroy(obj);
            else UnityEngine.Object.DestroyImmediate(obj);
        }

        public static RenderTexture Capture(float panelWidth,float panelHeight)
        {return Capture(panelWidth,panelHeight,out _);}

        public static RenderTexture Capture(float panelWidth,float panelHeight,out Rect subjectViewport)
        {
            subjectViewport=default;
            LastSceneStateRestored=false;LastStudioRendererCount=0;
            if(!Resolve(out var source,out var bounds,out var builtRoot))return null;
            bool portrait=panelHeight>panelWidth;
            // Render the ACTUAL city at the same shape as the visible
            // cathedral-sized ceremony, not a 3:2 fixed photo cropped again
            // inside a 1:2 portrait. This also prevents GPU upscale blur
            // after expanding the genuine full-world modal on desktop.
            // One continuous theatre uses a genuine full-frame lens. The actual
            // built subject is placed inside the protected caption-free area.
            var stage=DailyTreasureStageDirection.Compose(
                "milestone:bastion:camera",panelWidth,panelHeight);
            float photoWidth=panelWidth,photoHeight=panelHeight;
            float screenAspect=photoWidth/Mathf.Max(1f,photoHeight);
            int height=portrait?(panelWidth>=650f?1152:1024):
                (panelWidth>=950f?720:540);
            int width=Mathf.Clamp(Mathf.RoundToInt(
                height*screenAspect),64,1360);
            // Bound both RT dimensions as a pair, preserving the actual
            // full-viewport lens after the previous minimum width is removed.
            height=Mathf.Clamp(Mathf.RoundToInt(width/screenAspect),64,1152);
            RenderTexture result=null,working=null;
            Camera camera=null;
            GameObject cameraObject=null,lightObject=null,volumeObject=null;
            VolumeProfile ceremonyProfile=null;
            ActualBuiltStudio studio=null;
            var environmentBefore=new FogSnapshot(true);
            var previous=RenderTexture.active;
            try
            {
                // One original-source render, not an enlarged low-resolution
                // PNG. Copy the ACTUAL player camera but never move that
                // camera, touch the player's construction state or save.
                cameraObject=new GameObject("Daily treasure / temporary genuine Bastion lens");
                camera=cameraObject.AddComponent<Camera>();
                camera.CopyFrom(source);
                camera.transform.SetPositionAndRotation(
                    source.transform.position,source.transform.rotation);
                camera.enabled=false;
                camera.rect=new Rect(0f,0f,1f,1f);
                camera.aspect=(float)width/height;
                camera.cullingMask=source.cullingMask & ~(1<<5);
                camera.allowHDR=true;
                camera.allowMSAA=false;
                Frame(camera,bounds,stage,photoWidth,photoHeight,out subjectViewport);
                studio=new ActualBuiltStudio(builtRoot);
                studio.Begin(bounds,camera);LastStudioRendererCount=studio.RendererCount;
                // The entire Eldoria project runs URP 17.3.0. Unity's
                // Camera.Render() is NOT the supported out-of-loop route in
                // the Scriptable Render Pipeline. Use the official Unity 6
                // render-request API and a detached Base Camera.
                // Preserve the real world camera's physical scene settings,
                // but never modify that active camera or its gameplay stack.
                var originalData=source.GetComponent<UniversalAdditionalCameraData>();
                var renderData=camera.GetUniversalAdditionalCameraData();
                renderData.renderType=CameraRenderType.Base;
                if(originalData)
                {
                    renderData.renderPostProcessing=originalData.renderPostProcessing;
                    renderData.renderShadows=originalData.renderShadows;
                    renderData.volumeLayerMask=originalData.volumeLayerMask;
                    renderData.antialiasing=originalData.antialiasing;
                }
                // Dedicated, transient URP volume: no source camera/volume is
                // edited and the profile is not kept on the player scene.
                int gradeLayer=UnusedVolumeLayer();
                volumeObject=new GameObject("Daily treasure / temporary ceremony grade");
                volumeObject.layer=gradeLayer;
                volumeObject.hideFlags=HideFlags.HideAndDontSave;
                var volume=volumeObject.AddComponent<Volume>();
                volume.isGlobal=true;volume.priority=10000f;volume.weight=1f;
                ceremonyProfile=ScriptableObject.CreateInstance<VolumeProfile>();
                ceremonyProfile.hideFlags=HideFlags.HideAndDontSave;
                volume.sharedProfile=ceremonyProfile;
                var grade=ceremonyProfile.Add<ColorAdjustments>(true);
                grade.postExposure.Override(-.15f);
                grade.contrast.Override(9f);
                grade.saturation.Override(-20f);
                var tone=ceremonyProfile.Add<Tonemapping>(true);
                tone.mode.Override(TonemappingMode.ACES);
                var vignette=ceremonyProfile.Add<Vignette>(true);
                vignette.intensity.Override(.22f);
                vignette.smoothness.Override(.55f);
                renderData.volumeLayerMask=1<<gradeLayer;
                renderData.volumeTrigger=camera.transform;
                renderData.renderPostProcessing=true;
                BeginCeremonialEnvironment(camera,bounds);
                camera.targetTexture=null;
                // The old camera-owned MSAA target existed as an object but
                // had no hardware storage after capture (native81111cb4).
                // Render into a checked single-sample working target, then
                // copy actual GPU pixels into a separately owned snapshot.
                working=new RenderTexture(width,height,24,RenderTextureFormat.ARGB32)
                {
                    name="TRUE Bastion / temporary render working target",
                    filterMode=FilterMode.Bilinear,
                    wrapMode=TextureWrapMode.Clamp,
                    antiAliasing=1
                };
                if(!working.Create() || !working.IsCreated())
                    throw new InvalidOperationException("URP_WORKING_TEXTURE_ALLOCATION_FAILED");
                camera.targetTexture=working;
                // One broad warm bounce, never a globally applied light or
                // violet/neon firework. Created/destroyed synchronously.
                lightObject=new GameObject("Daily treasure / temporary amber relief");
                var light=lightObject.AddComponent<Light>();
                light.type=LightType.Point;
                light.color=new Color(1f,.81f,.57f);
                light.range=Mathf.Max(8f,bounds.size.magnitude*2.2f);
                light.intensity=.82f;
                light.shadows=LightShadows.None;
                light.cullingMask=camera.cullingMask;
                // A single amber key from the camera's front-left
                // reveals original masonry relief, not imaginary VFX.
                lightObject.transform.position=bounds.center+
                    camera.transform.right*(-bounds.extents.magnitude*.72f)+
                    Vector3.up*(bounds.extents.magnitude*.95f)-
                    camera.transform.forward*(bounds.extents.magnitude*.86f);
                // A detached Base Camera has no overlay stack. Use the
                // supported full URP request so render-context initialization
                // is explicit, then retain a snapshot never bound to a camera.
                var request=new RenderPipeline.StandardRequest();
                if(!RenderPipeline.SupportsRenderRequest(camera,request))
                    throw new InvalidOperationException(
                        "URP_SNAPSHOT_UNSUPPORTED: genuine Bastion not captured");
                request.destination=working;
                RenderPipeline.SubmitRenderRequest(camera,request);
                if(!working.IsCreated())
                    throw new InvalidOperationException("URP_WORKING_TEXTURE_LOST_AFTER_RENDER");
                result=new RenderTexture(width,height,0,RenderTextureFormat.ARGB32)
                {
                    name="TRUE Bastion / one-frame authenticated milestone",
                    filterMode=FilterMode.Bilinear,
                    wrapMode=TextureWrapMode.Clamp,
                    antiAliasing=1
                };
                if(!result.Create() || !result.IsCreated())
                    throw new InvalidOperationException("URP_PERSISTENT_SNAPSHOT_ALLOCATION_FAILED");
                Graphics.Blit(working,result);
                if(!result.IsCreated())
                    throw new InvalidOperationException("URP_PERSISTENT_SNAPSHOT_LOST_AFTER_COPY");
                // Detach BEFORE camera cleanup; the returned texture belongs
                // exclusively to DailyTreasurePanel, until its actual Close.
                camera.targetTexture=null;
                return result;
            }
            catch(Exception error)
            {
                subjectViewport=default;
                Debug.LogWarning("REAL_BASTION_CAMERA_UNAVAILABLE "+error.Message);
                if(result){result.Release();DestroyTemporary(result);}
                return null;
            }
            finally
            {
                RenderTexture.active=previous;
                if(studio!=null){studio.Dispose();LastSceneStateRestored=studio.Restored;}
                environmentBefore.Restore();
                // Deactivate the global volume before deferred PlayMode
                // destruction; subsequent real player cameras keep their
                // exact original fog and post-processing environment.
                DestroyTemporary(volumeObject);
                if(ceremonyProfile)
                {
                    foreach(var component in ceremonyProfile.components)
                        DestroyTemporary(component);
                    ceremonyProfile.components.Clear();
                    DestroyTemporary(ceremonyProfile);
                }
                if(camera)camera.targetTexture=null;
                DestroyTemporary(cameraObject);
                DestroyTemporary(lightObject);
                if(working){working.Release();DestroyTemporary(working);}
            }
        }
        public static void Release(RenderTexture texture)
        {
            if(!texture)return;
            if(RenderTexture.active==texture)RenderTexture.active=null;
            texture.Release();
            DestroyTemporary(texture);
        }
    }
}


