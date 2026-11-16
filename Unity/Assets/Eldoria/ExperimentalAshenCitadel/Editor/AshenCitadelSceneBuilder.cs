// Authored Unity scene assembly; never edits production scenes, HUD or build settings.
// Run in Unity Editor: Eldoria / Experiments / Ashen Citadel / Build isolated scene
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using Eldoria.ExperimentalAshenCitadel;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;

namespace Eldoria.ExperimentalAshenCitadel.Editor
{
    public static class AshenCitadelSceneBuilder
    {
        private const string Root="Assets/Eldoria/ExperimentalAshenCitadel";
        private const string ScenePath=Root+"/Scenes/AshenCitadel.unity";
        private const string MeshPath=Root+"/Meshes/";
        private const string MaterialPath=Root+"/Materials/";
        private static readonly CultureInfo Invariant=CultureInfo.InvariantCulture;
        private static readonly Dictionary<string,Material> Colors=new Dictionary<string,Material>(StringComparer.OrdinalIgnoreCase);

        [MenuItem("Eldoria/Experiments/Ashen Citadel/Build isolated scene")]
        public static void Build()
        {
            Directory.CreateDirectory(Root+"/Scenes");
            Directory.CreateDirectory(MaterialPath);
            AssetDatabase.Refresh();
            CreateMaterials();
            // Private look-development pipeline; never mutate the production asset.
            var sourcePipeline=GraphicsSettings.defaultRenderPipeline as UniversalRenderPipelineAsset;
            if(sourcePipeline!=null){
                var pipeline=UnityEngine.Object.Instantiate(sourcePipeline);
                pipeline.name="Ashen private lookdev";pipeline.shadowDistance=140f;pipeline.msaaSampleCount=4;
                // URP 17.3 exposes these two flags as read-only public properties.
                var settings=new SerializedObject(pipeline);
                settings.FindProperty("m_MainLightShadowsSupported").boolValue=true;
                settings.FindProperty("m_SoftShadowsSupported").boolValue=true;
                settings.ApplyModifiedPropertiesWithoutUndo();
                pipeline.mainLightShadowmapResolution=2048;pipeline.shadowCascadeCount=2;
                pipeline.shadowDepthBias=.2f;pipeline.shadowNormalBias=.25f;
                var pipelinePath=Root+"/Materials/AshenLookdevPipeline.asset";
                var savedPipeline=AssetDatabase.LoadAssetAtPath<UniversalRenderPipelineAsset>(pipelinePath);
                if(savedPipeline==null){AssetDatabase.CreateAsset(pipeline,pipelinePath);savedPipeline=pipeline;}
                else{EditorUtility.CopySerialized(pipeline,savedPipeline);UnityEngine.Object.DestroyImmediate(pipeline);}
                GraphicsSettings.defaultRenderPipeline=savedPipeline;QualitySettings.renderPipeline=savedPipeline;
            }
            var scene=EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
            RenderSettings.ambientMode=AmbientMode.Trilight;
            RenderSettings.ambientSkyColor=new Color(.36f,.42f,.56f);
            RenderSettings.ambientEquatorColor=new Color(.25f,.29f,.38f);
            RenderSettings.ambientGroundColor=new Color(.16f,.17f,.21f);
            RenderSettings.fog=true;RenderSettings.fogColor=new Color(.26f,.29f,.37f);
            RenderSettings.fogMode=FogMode.Linear;RenderSettings.fogStartDistance=65;RenderSettings.fogEndDistance=165;
            QualitySettings.shadows=UnityEngine.ShadowQuality.All;

            var key=new GameObject("Sun · amber key");
            var sun=key.AddComponent<Light>();
            sun.type=LightType.Directional;sun.intensity=1.60f;
            sun.color=new Color(1f,.86f,.68f);
            RenderSettings.sun=sun;
            sun.shadows=LightShadows.Soft;sun.shadowStrength=.64f;sun.shadowBias=.2f;sun.shadowNormalBias=.25f;
            sun.GetUniversalAdditionalLightData().usePipelineSettings=false;
            sun.transform.rotation=Quaternion.Euler(24,-52,0);
            var fill=new GameObject("Sky · cold fill").AddComponent<Light>();
            fill.type=LightType.Directional;fill.intensity=.28f;fill.color=new Color(.54f,.67f,.77f);
            fill.transform.rotation=Quaternion.Euler(56,145,0);
            fill.shadows=LightShadows.None;

            var cameraGO=new GameObject("Ashen · Player camera");
            cameraGO.tag="MainCamera";
            var camera=cameraGO.AddComponent<Camera>();
            camera.orthographic=true;camera.orthographicSize=35f;
            camera.clearFlags=CameraClearFlags.SolidColor;
            camera.backgroundColor=new Color(.25f,.28f,.28f);
            camera.nearClipPlane=.3f;camera.farClipPlane=180f;
            camera.allowHDR=true;camera.allowMSAA=true;camera.depth=0;
            cameraGO.AddComponent<AudioListener>();
            var cameraData=cameraGO.AddComponent<UniversalAdditionalCameraData>();
            cameraData.renderShadows=true;cameraData.renderPostProcessing=true;
            AddDuskGrade();
            cameraGO.transform.position=new Vector3(20,24,-28);
            cameraGO.transform.LookAt(new Vector3(0,2.5f,0));

            var sceneRoot=new GameObject("Ashen City · authored hierarchy");
            // Independent physically authored 3D mesh: sculpted terrain, stone
            // causeway, wild meadow and rocky edge. Blender/Unity share its UVs.
            var cityTerrain=Import("AshenLandscape",sceneRoot.transform);
            cityTerrain.name="Ashen valley · true 3D landscape";
            DisableDecorativeColliders(cityTerrain);
            var stagesRoot=new GameObject("Bastion stage variants");stagesRoot.transform.SetParent(sceneRoot.transform,false);
            var stages=new GameObject[5];
            for(int n=1;n<=5;n++)
            {
                var imported=Import("AshenBastion_"+n,stagesRoot.transform);
                imported.name="Stage_"+n;
                BindClick(imported,"Bastión","bastion","building",
                    new Vector3(0,3.4f,0),new Vector3(18.6f,8f,17.6f));
                imported.SetActive(n==1);stages[n-1]=imported;
            }
            var sawmill=Import("Ashen_Sawmill",sceneRoot.transform);
            sawmill.transform.position=new Vector3(-17.2f,0f,-6.2f);
            BindClick(sawmill,"Aserradero","sawmill","building",new Vector3(0,3f,0),new Vector3(7f,7f,7f));
            var barracks=Import("Ashen_Barracks",sceneRoot.transform);
            barracks.transform.position=new Vector3(16.8f,0f,-7.5f);
            BindClick(barracks,"Cuartel","barracks","building",new Vector3(0,3f,0),new Vector3(7f,7f,7f));
            var granary=Import("Ashen_Granary",sceneRoot.transform);
            granary.transform.position=new Vector3(-16.6f,0f,7.8f);
            BindClick(granary,"Granero","granary","building",new Vector3(0,3f,0),new Vector3(7f,7f,7f));
            var stoneworks=Import("Ashen_Stoneworks",sceneRoot.transform);
            stoneworks.transform.position=new Vector3(17.4f,0f,8.8f);
            BindClick(stoneworks,"Taller de piedra","stoneworks","building",new Vector3(0,3f,0),new Vector3(7f,7f,7f));
            // Distinctive landscape frame, sparse around the playable fortress.
            var treePrefab=AssetDatabase.LoadAssetAtPath<GameObject>(MeshPath+"Ashen_Pine.obj");
            var rockPrefab=AssetDatabase.LoadAssetAtPath<GameObject>(MeshPath+"Ashen_Rock.obj");
            if(!treePrefab||!rockPrefab)throw new InvalidOperationException("Missing authored flora assets");
            var random=new System.Random(1795);
            // Art-directed woodland masses: asymmetrical groves with clear approach.
            // Reuse the authored pine; broad scale variation avoids evenly spaced rows.
            var groves=new[]{new Vector2(-25,13),new Vector2(23,17),
                new Vector2(-29,-17),new Vector2(29,-23),new Vector2(-9,29),
                new Vector2(16,33),new Vector2(-34,30)};
            for(int i=0;i<78;i++)
            {
                var grove=groves[i%groves.Length];
                float angle=(float)random.NextDouble()*Mathf.PI*2f;
                float radius=Mathf.Sqrt((float)random.NextDouble())*8f;
                var pos=new Vector3(grove.x+Mathf.Cos(angle)*radius,0,grove.y+Mathf.Sin(angle)*radius*.75f);
                if(Mathf.Abs(pos.x)<23f && Mathf.Abs(pos.z)<15f)continue;
                if(Mathf.Abs(pos.x)<6f && pos.z<0)continue;
                var tree=ImportPrefab(treePrefab,sceneRoot.transform,"Pine grove · "+i);
                pos.y=ValleyHeight(pos.x,pos.z);tree.transform.position=pos;
                float scale=.52f+.66f*(float)random.NextDouble();
                tree.transform.localScale=new Vector3(scale,scale*(.85f+.30f*(float)random.NextDouble()),scale);
                tree.transform.rotation=Quaternion.Euler(0,(float)random.NextDouble()*360f,0);
            }
            for(int i=0;i<27;i++)
            {
                float angle=i*2.349963f;
                float radius=19f+20f*(float)random.NextDouble();
                if(radius<25f)continue;
                var rock=ImportPrefab(rockPrefab,sceneRoot.transform,"Natural fieldstone · "+i);
                rock.transform.position=new Vector3(Mathf.Cos(angle)*radius,-.04f,Mathf.Sin(angle)*radius*.87f);
                var rp=rock.transform.position;rp.y=ValleyHeight(rp.x,rp.z)-.04f;rock.transform.position=rp;
                rock.transform.localScale=Vector3.one*Mathf.Lerp(.36f,1.00f,(float)random.NextDouble());
                rock.transform.rotation=Quaternion.Euler(0,(i*83f)%360f,0);
            }
            var world=new GameObject("Ashen Frontier · authored experiment");
            var frontierTerrain=Import("AshenLandscape",world.transform);
            frontierTerrain.name="Ashen frontier · true 3D landscape";
            frontierTerrain.transform.position=new Vector3(0,0,55);
            DisableDecorativeColliders(frontierTerrain);
            // Three readable regions with authored meshes; not imported legacy landmarks.
            for(int i=0;i<30;i++)
            {
                float x=-25+(i%8)*3.2f;
                float z=53+(i/8)*5.2f;
                if(x>-6f && x<8f)continue;
                var pine=ImportPrefab(treePrefab,world.transform,"Forest · "+i);
                pine.transform.position=new Vector3(x,0,z);
                pine.transform.localScale=Vector3.one*(.7f+(i%4)*.10f);
            }
            for(int i=0;i<13;i++)
            {
                var stone=ImportPrefab(rockPrefab,world.transform,"Quarry · "+i);
                stone.transform.position=new Vector3(11+(i%4)*2.7f,-.04f,43+(i/4)*2.5f);
                stone.transform.localScale=Vector3.one*(.45f+i%4*.18f);
            }
            AddRift(world.transform,new Vector3(13,.07f,73));
            AddRouteMarkers(world.transform);
            world.SetActive(false);

            var controller=new GameObject("ASHEN · authoritative game adapter").AddComponent<AshenCitadelExperience>();
            controller.Configure(stages,sawmill,barracks,granary,stoneworks,sceneRoot,world,camera);
            if(!EditorSceneManager.SaveScene(scene,ScenePath))
                throw new InvalidOperationException("Could not save new isolated Unity scene");
            AssetDatabase.SaveAssets();AssetDatabase.Refresh();
            Debug.Log("ASHEN_ART_SCENE_BUILT path="+ScenePath+" stages=5 auxiliary=4 world=yes");
        }
        private static void DisableDecorativeColliders(GameObject item)
        {
            foreach(var c in item.GetComponentsInChildren<Collider>(true))
                UnityEngine.Object.DestroyImmediate(c);
        }

        // Coarse author-time proxies, no cooked mesh colliders in the mobile runtime.
        private static void BindClick(GameObject target,string title,string id,string kind,
            Vector3 localCenter,Vector3 localSize)
        {
            var collider=target.AddComponent<BoxCollider>();
            collider.center=localCenter;collider.size=localSize;collider.isTrigger=true;
            target.AddComponent<AshenCitadelPickTarget>().Configure(title,id,kind);
        }

        private static GameObject Import(string name,Transform parent)
        {
            var asset=AssetDatabase.LoadAssetAtPath<GameObject>(MeshPath+name+".obj");
            if(asset==null)throw new FileNotFoundException("No imported OBJ: "+name);
            return ImportPrefab(asset,parent,name);
        }
        private static GameObject ImportPrefab(GameObject model,Transform parent,string name)
        {
            var go=(GameObject)PrefabUtility.InstantiatePrefab(model);
            go.name=name;go.transform.SetParent(parent,false);
            int count=0;
            foreach(var r in go.GetComponentsInChildren<MeshRenderer>(true))
            {
                var mats=r.sharedMaterials;
                for(int i=0;i<mats.Length;i++)
                {
                    var src=mats[i]?.name??"";
                    string id=Colors.Keys.OrderByDescending(k=>k.Length).FirstOrDefault(
                        key=>src.Equals(key,StringComparison.OrdinalIgnoreCase)||
                             src.StartsWith(key+" ",StringComparison.OrdinalIgnoreCase)||
                             src.EndsWith(":"+key,StringComparison.OrdinalIgnoreCase));
                    if(id==null)
                        throw new InvalidOperationException("Unknown imported material in "+name+": '"+src+
                            "'. Fix OBJ/MTL binding; do not hide magenta with a fallback.");
                    mats[i]=Colors[name=="AshenLandscape" && (id=="leaf"||id=="leafLight")?"grass_"+id:id];count++;
                }
                r.sharedMaterials=mats;r.shadowCastingMode=ShadowCastingMode.On;r.receiveShadows=true;
            }
            if(count==0)throw new InvalidOperationException("No MeshRenderer/material on "+name);
            return go;
        }
        private static void CreateMaterials()
        {
            Colors.Clear();
            string assetPath=MeshPath+"AshenPalette.mtl";
            if(!File.Exists(assetPath))throw new FileNotFoundException(assetPath);
            var shader=Shader.Find("Universal Render Pipeline/Lit");
            if(shader==null)throw new InvalidOperationException("URP Lit shader missing; no fallback/pink renders accepted");
            var valleyGround=CreateValleyGroundTexture();
            string current=null;
            foreach(var raw in File.ReadAllLines(assetPath))
            {
                var line=raw.Trim();
                if(line.StartsWith("newmtl ",StringComparison.Ordinal)){current=line.Substring(7).Trim();continue;}
                if(!line.StartsWith("Kd ",StringComparison.Ordinal)||string.IsNullOrEmpty(current))continue;
                var parts=line.Substring(3).Split(new[]{' '},StringSplitOptions.RemoveEmptyEntries);
                if(parts.Length!=3)throw new InvalidDataException("Bad Kd for "+current);
                var col=new Color(float.Parse(parts[0],Invariant),float.Parse(parts[1],Invariant),float.Parse(parts[2],Invariant),1);
                var mPath=MaterialPath+current+".mat";
                var mat=AssetDatabase.LoadAssetAtPath<Material>(mPath);
                if(!mat){mat=new Material(shader){name="Ashen "+current};AssetDatabase.CreateAsset(mat,mPath);}
                mat.shader=shader;mat.SetColor("_BaseColor",col);
                // Match the actual Blender source: stone 256 px, other reusable
                // artist-authored textures 128 px, multiplied by this palette color.
                var textureFamily=
                    current=="plaster"?"Plaster":
                    new[]{"basalt","stone","stoneLight","cut"}.Contains(current)?"Stone":
                    new[]{"roof","slate"}.Contains(current)?"Roof":
                    new[]{"wood","oak"}.Contains(current)?"Wood":
                    new[]{"soil","moss","leaf","leafLight"}.Contains(current)?"Moss":
                    new[]{"iron","brass","warm"}.Contains(current)?"Metal":
                    new[]{"banner","violet"}.Contains(current)?"Cloth":null;
                if(textureFamily==null)throw new InvalidDataException("Unknown source palette key: "+current);
                var texturePath=Root+"/Textures/Ashen"+textureFamily+".png";
                var texture=AssetDatabase.LoadAssetAtPath<Texture2D>(texturePath);
                if(texture==null)throw new FileNotFoundException("Required authored texture not imported: "+texturePath);
                mat.SetTexture("_BaseMap",new[]{"basalt","leaf","leafLight","warm"}.Contains(current)?null:texture);
                // Terrain UV0 comes from a single large floor primitive.
                // Repeat the lightweight moss/soil atlas instead of stretching
                // one 128px tile over the entire 146m² world.
                if(current=="moss" || current=="soil")
                    mat.SetTextureScale("_BaseMap",Vector2.one);
                if(current=="moss" || current=="soil"){
                    mat.SetColor("_BaseColor",Color.white);
                    mat.SetTexture("_BaseMap",valleyGround);
                    mat.SetTextureScale("_BaseMap",Vector2.one/36f);
                    mat.SetTextureOffset("_BaseMap",new Vector2(.5f,.5f));
                }
                if(current=="leaf" || current=="leafLight"){
                    var needles=AssetDatabase.LoadAssetAtPath<Texture2D>(Root+"/Textures/AshenNeedles.png");
                    if(needles==null)throw new FileNotFoundException("Original needle texture missing");
                    mat.SetTexture("_BaseMap",needles);mat.SetColor("_BaseColor",current=="leaf"?new Color(.72f,.84f,.82f):new Color(.90f,.95f,.85f));
                    mat.SetFloat("_AlphaClip",1f);mat.SetFloat("_Cutoff",.36f);mat.EnableKeyword("_ALPHATEST_ON");
                    mat.SetFloat("_Cull",0f);mat.SetOverrideTag("RenderType","TransparentCutout");mat.renderQueue=2450;
                }
                if(current=="warm"){mat.EnableKeyword("_EMISSION");mat.SetColor("_EmissionColor",new Color(3.8f,1.55f,.32f));}
                mat.SetFloat("_Smoothness",current=="brass"?.35f:.09f);
                mat.SetFloat("_Metallic",current=="brass"?.42f:.01f);
                EditorUtility.SetDirty(mat);Colors[current]=mat;
                if(current=="leaf"||current=="leafLight"){
                    var grassPath=MaterialPath+"grass_"+current+".mat";
                    var grass=AssetDatabase.LoadAssetAtPath<Material>(grassPath);
                    if(grass==null){grass=new Material(shader);AssetDatabase.CreateAsset(grass,grassPath);}
                    grass.SetColor("_BaseColor",current=="leafLight"?new Color(.43f,.45f,.23f):new Color(.24f,.33f,.19f));grass.SetTexture("_BaseMap",null);grass.SetFloat("_Smoothness",.025f);grass.SetFloat("_Metallic",0f);grass.SetFloat("_Cull",0f);EditorUtility.SetDirty(grass);Colors["grass_"+current]=grass;
                }
            }
            if(Colors.Count<16)throw new InvalidDataException("Incomplete authored material palette: "+Colors.Count);
            AssetDatabase.SaveAssets();
        }
        private static void AddDuskGrade()
        {
            var profile=ScriptableObject.CreateInstance<VolumeProfile>();profile.name="Ashen inhabited dusk";
            var bloom=profile.Add<Bloom>(true);bloom.intensity.Override(.22f);bloom.threshold.Override(1.15f);bloom.scatter.Override(.55f);
            var grade=profile.Add<ColorAdjustments>(true);grade.postExposure.Override(.22f);grade.contrast.Override(5f);grade.saturation.Override(2f);
            var split=profile.Add<SplitToning>(true);split.shadows.Override(new Color(.45f,.47f,.55f));split.highlights.Override(new Color(.55f,.51f,.45f));split.balance.Override(-8f);
            var tone=profile.Add<Tonemapping>(true);tone.mode.Override(TonemappingMode.Neutral);
            var path=MaterialPath+"AshenDuskVolume.asset";
            var old=AssetDatabase.LoadAssetAtPath<VolumeProfile>(path);
            if(old!=null)AssetDatabase.DeleteAsset(path);
            AssetDatabase.CreateAsset(profile,path);
            foreach(var component in profile.components)AssetDatabase.AddObjectToAsset(component,profile);
            var volume=new GameObject("Ashen dusk · selective bloom and restrained colour").AddComponent<Volume>();
            volume.isGlobal=true;volume.priority=10;volume.sharedProfile=profile;
        }

        // Keep the central building plots planar; raise the wooded rim, not the foundations.
        private static float ValleyHeight(float x,float z)
        {
            float rad=Mathf.Sqrt(x*x*.85f*.85f+z*z);
            float t=Mathf.Clamp01((rad-18f)/29f);
            float noise=Mathf.Sin(x*.41f+Mathf.Cos(z*.18f))*Mathf.Cos(z*.47f)*.5f+
                Mathf.Sin(x*.11f+z*.24f)*.38f+Mathf.Sin(x*.82f-z*.60f)*.12f;
            return -.06f+(noise*.55f+Mathf.Sin(x*.12f-z*.14f)*.35f)*t+
                t*t*2.2f;
        }
        private static float SegmentDistance(Vector2 p,Vector2 a,Vector2 b)
        {
            var d=b-a;float t=Mathf.Clamp01(Vector2.Dot(p-a,d)/d.sqrMagnitude);
            return Vector2.Distance(p,a+d*t);
        }
        private static Texture2D CreateValleyGroundTexture()
        {
            const int size=1024;
            var tex=new Texture2D(size,size,TextureFormat.RGBA32,true){name="Ashen valley hand-layered ground",
                wrapMode=TextureWrapMode.Clamp,filterMode=FilterMode.Trilinear,anisoLevel=4};
            var pixels=new Color[size*size];
            for(int y=0;y<size;y++)for(int x=0;x<size;x++){
                float wx=(x/(float)(size-1)-.5f)*180f,wz=(y/(float)(size-1)-.5f)*180f;
                float broad=Mathf.PerlinNoise(wx*.045f+29f,wz*.045f+51f);
                float detail=Mathf.PerlinNoise(wx*.39f+17f,wz*.39f+11f);
                var col=Color.Lerp(new Color(.19f,.245f,.14f),new Color(.43f,.43f,.275f),Mathf.InverseLerp(.23f,.77f,broad));
                col*=.90f+detail*.20f;
                float meadow=Mathf.SmoothStep(0f,1f,Mathf.InverseLerp(.25f,.75f,Mathf.PerlinNoise(wx*.13f+5f,wz*.13f+3f)));
                col=Color.Lerp(col,new Color(.34f,.37f,.235f),meadow*.38f);
                float fleck=Mathf.PerlinNoise(wx*1.4f+73f,wz*1.4f+23f);
                col=Color.Lerp(col,new Color(.38f,.365f,.245f),Mathf.Clamp01((fleck-.58f)*2.8f)*.25f);
                float routeX=Mathf.Sin((wz+8f)*.15f)*.66f+Mathf.Sin((wz+8f)*.52f)*.17f;
                float route=Mathf.Clamp01(1f-(Mathf.Abs(wx-routeX)-1.3f)/2.1f)*
                    Mathf.Clamp01((-wz-6f)/3f)*Mathf.Clamp01((wz+44f)/4f);
                // Two worn wheel tracks and a mossy centre, with irregular soft margins.
                float tracks=Mathf.Exp(-Mathf.Pow((Mathf.Abs(wx-routeX)-.90f)/.42f,2f));
                float brokenEdge=.72f+.28f*Mathf.PerlinNoise(wx*.8f+61f,wz*.8f+21f);
                col=Color.Lerp(col,new Color(.42f,.36f,.26f)*(.91f+detail*.18f),route*brokenEdge*(.58f+.25f*tracks));
                // Small-scale mineral flecks provide ground scale without a tiled grid.
                float grit=Mathf.Clamp01((fleck-.64f)*5f);
                col=Color.Lerp(col,new Color(.49f,.46f,.35f),grit*route*.32f);
                // Connected foot traffic: long soft paths, never circular plots or raised pads.
                var homes=new[]{new Vector2(-17.2f,-6.2f),new Vector2(16.8f,-7.5f),
                    new Vector2(-16.6f,7.8f),new Vector2(17.4f,8.8f)};
                for(int homeIndex=0;homeIndex<homes.Length;homeIndex++){
                    var home=homes[homeIndex];
                    float side=home.x<0?-1f:1f;
                    var start=new Vector2(side*2.8f,-12f);
                    var end=new Vector2(home.x,home.y-2.7f);
                    // Route via the outside of the walls, instead of crossing the fortress.
                    var bend=new Vector2(side*13.1f,-11.7f);
                    float dist=Mathf.Min(SegmentDistance(new Vector2(wx,wz),start,bend),
                        SegmentDistance(new Vector2(wx,wz),bend,end));
                    float worn=1f-Mathf.SmoothStep(.55f,1.9f+(detail-.5f)*.8f,dist);
                    col=Color.Lerp(col,new Color(.39f,.33f,.245f)*(.89f+detail*.22f),worn*.78f);
                    float hx=wx-home.x,hz=wz-home.y;
                    float activity=Mathf.Exp(-((hx-.25f)*(hx-.25f)/10f+(hz+2.2f)*(hz+2.2f)/4.5f));
                    activity*=Mathf.SmoothStep(.20f,.75f,Mathf.PerlinNoise(wx*.9f+11f,wz*.9f+36f));
                    var workTone=homeIndex==0?new Color(.50f,.41f,.25f):homeIndex==2?new Color(.48f,.44f,.26f):
                        homeIndex==3?new Color(.49f,.47f,.38f):new Color(.35f,.30f,.22f);
                    col=Color.Lerp(col,workTone,activity*.75f);
                }
                float dry=Mathf.PerlinNoise(wx*.19f+37f,wz*.19f+7f);
                col=Color.Lerp(col,new Color(.40f,.39f,.26f),Mathf.Clamp01((dry-.60f)*3f)*.30f);
                pixels[y*size+x]=col;
            }
            tex.SetPixels(pixels);tex.Apply(true,false);
            var path=Root+"/Textures/AshenValleyGround.asset";
            var saved=AssetDatabase.LoadAssetAtPath<Texture2D>(path);
            if(saved==null){AssetDatabase.CreateAsset(tex,path);return tex;}
            EditorUtility.CopySerialized(tex,saved);UnityEngine.Object.DestroyImmediate(tex);return saved;
        }

        // Contiguous terrain extends beyond every portrait and landscape camera
        // instead of exposing the rectangular edge of a tabletop city.
        private static void AddGround(Transform root,float x,float z,float w,float d)
        {
            var floor=GameObject.CreatePrimitive(PrimitiveType.Cube);
            floor.name="Deep valley · continuous terrain";
            floor.transform.SetParent(root,false);
            floor.transform.position=new Vector3(x,-.36f,z);
            floor.transform.localScale=new Vector3(w,.60f,d);
            floor.GetComponent<Renderer>().sharedMaterial=Colors["moss"];
            UnityEngine.Object.DestroyImmediate(floor.GetComponent<Collider>());

            // An authored winding causeway, not a single long rectangular strip.
            // These vertices are saved in the experimental Unity scene/mesh asset.
            var vertices=new List<Vector3>();
            var triangles=new List<int>();
            for(int i=0;i<=22;i++)
            {
                float t=i/22f;
                float at=z-34f+37f*t;
                float drift=Mathf.Sin(t*4.20f)*.57f+Mathf.Sin(t*10.7f)*.12f;
                float width=2.35f+.32f*Mathf.Sin(t*6.31f);
                vertices.Add(new Vector3(x+drift-width,.035f,at));
                vertices.Add(new Vector3(x+drift+width,.035f,at));
                if(i==0)continue;
                int previous=(i-1)*2,current=i*2;
                triangles.Add(previous);triangles.Add(current);triangles.Add(current+1);
                triangles.Add(previous);triangles.Add(current+1);triangles.Add(previous+1);
            }
            var roadMesh=new Mesh{name="Ashen · soft-curved worn approach"};
            roadMesh.SetVertices(vertices);roadMesh.SetTriangles(triangles,0);
            roadMesh.RecalculateNormals();roadMesh.RecalculateBounds();
            var path=Root+"/Meshes/AshenRoadMesh.asset";
            var existing=AssetDatabase.LoadAssetAtPath<Mesh>(path);
            if(existing==null){AssetDatabase.CreateAsset(roadMesh,path);existing=roadMesh;}
            else{EditorUtility.CopySerialized(roadMesh,existing);UnityEngine.Object.DestroyImmediate(roadMesh);}
            var road=new GameObject("Stone-dust road · terrain embedded");
            road.transform.SetParent(root,false);
            road.AddComponent<MeshFilter>().sharedMesh=existing;
            road.AddComponent<MeshRenderer>().sharedMaterial=Colors["soil"];

            // Small irregular stone islands give scale without outlining a square.
            var rng=new System.Random(3719+(int)z);
            for(int i=0;i<24;i++)
            {
                float progress=(float)rng.NextDouble();
                float zz=z-29f+29f*progress;
                float xx=x+Mathf.Sin(progress*4.20f)*.57f+
                    (rng.Next(2)==0?-1f:1f)*Mathf.Lerp(1.9f,4.25f,(float)rng.NextDouble());
                var pebble=GameObject.CreatePrimitive(PrimitiveType.Cube);
                pebble.name="Weathered route stone · "+i;
                pebble.transform.SetParent(root,false);
                pebble.transform.position=new Vector3(xx,.039f,zz);
                pebble.transform.rotation=Quaternion.Euler(0,(float)rng.NextDouble()*180f,0);
                pebble.transform.localScale=new Vector3(.22f+.27f*(float)rng.NextDouble(),
                    .048f,.37f+.25f*(float)rng.NextDouble());
                pebble.GetComponent<Renderer>().sharedMaterial=Colors["cut"];
                UnityEngine.Object.DestroyImmediate(pebble.GetComponent<Collider>());
            }
        }
        private static void AddRift(Transform parent,Vector3 center)
        {
            var root=new GameObject("BRECHA · geologic incision");root.transform.SetParent(parent,false);
            root.transform.position=center;
            // A discontinuous recessed seam; no upright neon totem.
            var vertices=new List<Vector3>();
            var tris=new List<int>();
            var points=new[]{new Vector3(-4,0,-3),new Vector3(-2.5f,0,-1.6f),
                new Vector3(-2.8f,0,.6f),new Vector3(-1.3f,0,2.2f),
                new Vector3(.3f,0,3.8f),new Vector3(2.2f,0,5.8f)};
            for(int i=0;i<points.Length;i++)
            {
                var p=points[i];float w=.33f+(i%3)*.10f;
                vertices.Add(p+new Vector3(-w,.04f,0));
                vertices.Add(p+new Vector3(w,.04f,0));
                vertices.Add(p+new Vector3(-w,-.24f,0));
                vertices.Add(p+new Vector3(w,-.24f,0));
                if(i==0)continue;
                int a=(i-1)*4,b=i*4;
                foreach(var tri in new[]{a,a+2,b+2,a,b+2,b,a+1,b+1,b+3,a+1,b+3,a+3})
                    tris.Add(tri);
            }
            var mesh=new Mesh{name="Ashen · fissure recessed geometry",indexFormat=IndexFormat.UInt16};
            mesh.SetVertices(vertices);mesh.SetTriangles(tris,0);mesh.RecalculateNormals();
            var assetPath=Root+"/Meshes/AshenFissureMesh.asset";
            var prior=AssetDatabase.LoadAssetAtPath<Mesh>(assetPath);
            if(prior==null){AssetDatabase.CreateAsset(mesh,assetPath);prior=mesh;}
            else{EditorUtility.CopySerialized(mesh,prior);UnityEngine.Object.DestroyImmediate(mesh);}
            var go=new GameObject("Fractured seam");go.transform.SetParent(root.transform,false);
            go.AddComponent<MeshFilter>().sharedMesh=prior;
            go.AddComponent<MeshRenderer>().sharedMaterial=Colors["violet"];
            for(int i=0;i<12;i++)
            {
                var slab=GameObject.CreatePrimitive(PrimitiveType.Cube);
                slab.name="Rift · fractured basalt lip";slab.transform.SetParent(root.transform,false);
                slab.transform.localPosition=new Vector3(-4+i*.54f,.04f,-3+i*.72f);
                slab.transform.localRotation=Quaternion.Euler(0,37+i*17,0);
                slab.transform.localScale=new Vector3(.8f,.14f,.38f);
                slab.GetComponent<Renderer>().sharedMaterial=Colors[i%3==0?"cut":"basalt"];
                UnityEngine.Object.DestroyImmediate(slab.GetComponent<Collider>());
            }
        }
        private static void AddRouteMarkers(Transform parent)
        {
            var milestones=new[]{
                new Vector3(-15,.05f,66),new Vector3(15,.05f,48),
                new Vector3(6,.05f,60),new Vector3(17,.05f,67),
                new Vector3(13,.05f,73)};
            for(int i=0;i<milestones.Length;i++)
            {
                var obj=GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                obj.name="Route landmark "+i;
                obj.transform.SetParent(parent,false);
                obj.transform.position=milestones[i];
                obj.transform.localScale=new Vector3(.84f,.14f,.84f);
                obj.GetComponent<Renderer>().sharedMaterial=Colors[i==4?"violet":"stoneLight"];
                var landmark=obj.AddComponent<AshenCitadelPickTarget>();
                string[] titles={"Bosque","Cantera","Explorador corrupto","Engendro","Brecha"};
                string[] ids={"forest-valoria","quarry-valoria","corrupt-scout","engendro-valoria","fissure-valoria"};
                string[] kinds={"gather","gather","fight","fight","fissure"};
                landmark.Configure(titles[i],ids[i],kinds[i]);
                var collider=obj.GetComponent<Collider>();
                if(collider!=null)collider.isTrigger=true;
            }
        }

        // QA artwork only: switches five distinct mesh variants in a SINGLE
        // scene/camera without changing PlayerState or pretending the player advanced.
        [MenuItem("Eldoria/Experiments/Ashen Citadel/Capture five levels")]
        public static void CaptureFiveLevels()
        {
            if(!File.Exists(ScenePath))Build();
            var scene=EditorSceneManager.OpenScene(ScenePath,OpenSceneMode.Single);
            var stages=GameObject.Find("Bastion stage variants");
            var camera=GameObject.Find("Ashen · Player camera")?.GetComponent<Camera>();
            if(stages==null||camera==null)throw new InvalidOperationException("Experimental scene incomplete");
            var outDir=Path.GetFullPath(Path.Combine(UnityEngine.Application.dataPath,"..","ArtEvidence"));
            Directory.CreateDirectory(outDir);
            foreach(var viewport in new[]{new Vector2Int(390,844),new Vector2Int(1600,900)})
            {
                for(int l=1;l<=5;l++)
                {
                    for(int j=1;j<=5;j++)
                    {
                        var t=stages.transform.Find("Stage_"+j);
                        if(t==null)throw new InvalidOperationException("Missing authored stage "+j);
                        t.gameObject.SetActive(j==l);
                    }
                    AshenCitadelExperience.FrameCameraForViewport(camera,viewport.x,viewport.y);
                    // Preserve the established landscape A/B composition.
                    if(viewport.x>viewport.y)camera.orthographicSize=14f;
                    if(camera.ViewportPointToRay(new Vector3(.5f,0,0)).origin.y<5.99f)
                        throw new InvalidOperationException("Portrait camera ray starts below terrain");
                    var rt=new RenderTexture(viewport.x,viewport.y,24,RenderTextureFormat.ARGB32);
                    camera.targetTexture=rt;camera.aspect=(float)viewport.x/viewport.y;
                    var request=new RenderPipeline.StandardRequest{destination=rt};
                    if(!RenderPipeline.SupportsRenderRequest(camera,request))throw new InvalidOperationException("URP render request unsupported");
                    // Each level/camera swap gets an explicit warm render. Retain the
                    // first frame as diagnostic evidence, rather than silently discarding it.
                    RenderPipeline.SubmitRenderRequest(camera,request);
                    RenderTexture.active=rt;
                    var first=new Texture2D(viewport.x,viewport.y,TextureFormat.RGB24,false);
                    first.ReadPixels(new Rect(0,0,viewport.x,viewport.y),0,0);first.Apply(false);
                    File.WriteAllBytes(Path.Combine(outDir,"Diagnostic_FirstFrame_"+l+"_"+viewport.x+"x"+viewport.y+".png"),first.EncodeToPNG());
                    UnityEngine.Object.DestroyImmediate(first);
                    RenderPipeline.SubmitRenderRequest(camera,request);
                    RenderTexture.active=rt;
                    var image=new Texture2D(viewport.x,viewport.y,TextureFormat.RGB24,false);
                    image.ReadPixels(new Rect(0,0,viewport.x,viewport.y),0,0);image.Apply(false);
                    var pixels=image.GetPixels32();
                    int pink=0;
                    double brightness=0,brightnessSquare=0;
                    foreach(var c in pixels)
                    {
                        if(c.r>205 && c.b>180 && c.g<80)pink++;
                        double v=(.2126*c.r+.7152*c.g+.0722*c.b);
                        brightness+=v;brightnessSquare+=v*v;
                    }
                    float pct=100f*pink/pixels.Length;
                    double mean=brightness/pixels.Length;
                    double variance=brightnessSquare/pixels.Length-mean*mean;
                    if(pct>.5f)throw new InvalidOperationException("PINK SHADER "+pct.ToString("F2",Invariant)+"% on Bastion "+l);
                    if(mean<14||variance<60)
                        throw new InvalidOperationException("Blank/flat camera render. Mean="+mean.ToString("F2",Invariant)+
                            " variance="+variance.ToString("F2",Invariant)+". No art certification.");
                    var name="Ashen_Bastion_"+l+"_"+viewport.x+"x"+viewport.y+".png";
                    File.WriteAllBytes(Path.Combine(outDir,name),image.EncodeToPNG());
                    Debug.Log("ASHEN_CAPTURE "+name+" suspect_magenta="+pct.ToString("F3",Invariant)+"%");
                    if(l==5 && viewport.x>viewport.y){
                        var light=RenderSettings.sun;var mode=light.shadows;
                        light.shadows=LightShadows.None;
                        RenderPipeline.SubmitRenderRequest(camera,request);
                        RenderTexture.active=rt;
                        image.ReadPixels(new Rect(0,0,viewport.x,viewport.y),0,0);image.Apply(false);
                        var noShadow=image.GetPixels32();int affected=0;double difference=0;
                        for(int k=0;k<pixels.Length;k++){
                            float delta=Mathf.Abs(noShadow[k].r-pixels[k].r)+Mathf.Abs(noShadow[k].g-pixels[k].g)+Mathf.Abs(noShadow[k].b-pixels[k].b);
                            if(delta>12f)affected++;difference+=delta/3f;
                        }
                        File.WriteAllBytes(Path.Combine(outDir,"Diagnostic_Bastion_V_shadows_off.png"),image.EncodeToPNG());
                        light.shadows=mode;
                        Debug.Log("ASHEN_SHADOW_AB affected_pixels="+affected+" mean_channel_delta="+(difference/pixels.Length).ToString("F4",Invariant)+
                            " pipeline="+QualitySettings.renderPipeline.name+" quality="+QualitySettings.GetQualityLevel());
                    }
                    camera.targetTexture=null;RenderTexture.active=null;
                    UnityEngine.Object.DestroyImmediate(rt);UnityEngine.Object.DestroyImmediate(image);
                }
            }
            Debug.Log("ASHEN_CAPTURE_COMPLETE 10 Unity-camera PNG in "+outDir);
        }
        public static void BuildAndCapture(){Build();CaptureFiveLevels();}
    }
}
