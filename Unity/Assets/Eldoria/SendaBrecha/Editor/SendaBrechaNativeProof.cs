#if UNITY_EDITOR
using System;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Eldoria.SendaBrecha.Editor
{
    /// <summary>
    /// Native visual evidence contract. Run via Unity -batchmode -executeMethod
    /// Eldoria.SendaBrecha.Editor.SendaBrechaNativeProof.Run (NO -nographics).
    /// Does not save or modify the integrated city / beta scene.
    /// Screenshots are actual RenderTexture pixels, never conceptual mockups.
    /// </summary>
    public static class SendaBrechaNativeProof
    {
        [Serializable] private sealed class ProofReport
        {
            public string product = "La Senda de la Brecha";
            public string test = "isolated procedural native GPU screenshot (pipeline recorded)";
            public string renderPipeline = UnityEngine.Rendering.GraphicsSettings.currentRenderPipeline ? UnityEngine.Rendering.GraphicsSettings.currentRenderPipeline.GetType().Name : "Built-in";
            public string platform = UnityEngine.Application.platform.ToString();
            public string unity = UnityEngine.Application.unityVersion;
            public bool mainSceneModified;
            public bool physicalIphoneTested;
            public bool artApproved;
            public int capturedPng;
            public int originalAldricSkins, originalArchitecture, originalFoliage, nativeGuardRigs, guardTriangles;
            public bool slateInstalled,studioBreachInstalled,studioRuinInstalled;
            public int movingVergeCount;
            public int studioPortalCount,actorShadowCasters;
            public bool originalEnemy, originalAnimationSampled;
            public bool positiveGateChangesVisibleActors;
            public bool victoryRouteChecked;
            public bool defeatRouteChecked;
            public int finalSoldiers;
            public int totalStages;
            public string result;
        }
        private static void Need(bool condition, string reason)
        {
            if (!condition) throw new InvalidOperationException(reason);
        }
        private static void Advance(SendaBrechaPrototype scene, float seconds)
        {
            const float step = .05f;
            int count = Mathf.CeilToInt(seconds / step);
            for (int i=0;i<count;i++) scene.Step(step);
        }
        private static void SaveCapture(Camera camera, string directory, string filename, int width, int height)
        {
            Need(camera != null, "Missing real Unity camera");
            Directory.CreateDirectory(directory);
            RenderTexture target = RenderTexture.GetTemporary(width,height,24,RenderTextureFormat.ARGB32);
            var previousTarget = camera.targetTexture;
            float previousAspect = camera.aspect;
            var previousActive = RenderTexture.active;
            Texture2D pixels = null;
            try
            {
                camera.targetTexture=target;
                camera.aspect=(float)width/height;
                camera.Render();
                RenderTexture.active=target;
                pixels=new Texture2D(width,height,TextureFormat.RGB24,false);
                pixels.ReadPixels(new Rect(0,0,width,height),0,0);
                pixels.Apply(false);
                Color32[] values=pixels.GetPixels32();
                int min=255,max=0;
                for(int i=0;i<values.Length;i+=83)
                {
                    int c=(values[i].r+values[i].g+values[i].b)/3;
                    min=Math.Min(min,c);max=Math.Max(max,c);
                }
                Need(max-min>22,filename+": camera returned flat image");
                File.WriteAllBytes(Path.Combine(directory,filename),pixels.EncodeToPNG());
            }
            finally
            {
                camera.targetTexture=previousTarget;
                camera.aspect=previousAspect;
                RenderTexture.active=previousActive;
                RenderTexture.ReleaseTemporary(target);
                if(pixels) UnityEngine.Object.DestroyImmediate(pixels);
            }
        }
        [MenuItem("Eldoria/Prototipos/Senda de la Brecha (escena aislada)")]
        public static void Preview()
        {
            if(!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            CreateIsolatedScene(false);
            Debug.Log("SENDA_PREVIEW_READY: Press Play to generate runtime 3D geometry, then use COMENZAR EXPEDICIÓN / arrows or swipe. No beta files modified.");
        }
        [MenuItem("Eldoria/Prototipos/Senda de la Brecha (guardar escena aislada)")]
        public static void SaveStandaloneScene()
        {
            if(!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            CreateIsolatedScene(true);
        }
        private const string Mini = "Assets/Eldoria/SendaBrecha/CanonicalSource/";
        private const string Full = "Assets/Experimentos/ValoriaModular25D/Resources/ValoriaModular25D/";
        private const string Family = "Assets/Eldoria/Resources/Valoria/ProductionArt/CoherentFamilyV1/";
        private static T Canonical<T>(string file,string original) where T:UnityEngine.Object
        {
            var v=AssetDatabase.LoadAssetAtPath<T>(Mini+file);
            if(!v)v=AssetDatabase.LoadAssetAtPath<T>(original);
            return v;
        }
        private static void ConfigureCanonical(SendaBrechaPrototype game)
        {
            const string hero="SirAldric_Approved_RiggedAnimated_v1.glb";
            const string heroPath="Assets/StreamingAssets/Eldoria/Aldric/"+hero;
            game.CanonicalAldric=Canonical<GameObject>(hero,heroPath);
            game.CanonicalGuard=AssetDatabase.LoadAssetAtPath<GameObject>(Mini+"Valoria_Guard_Rigged_LOD.glb");
            game.StudioSlate=AssetDatabase.LoadAssetAtPath<Texture2D>(Mini+"senda-worn-slate.png");
            game.StudioBreach=AssetDatabase.LoadAssetAtPath<Texture2D>(Mini+"senda-breach-landmark.png");
            game.StudioRuin=AssetDatabase.LoadAssetAtPath<Texture2D>(Mini+"senda-valoria-ruin.png");
            game.StudioPortal=AssetDatabase.LoadAssetAtPath<Texture2D>(Mini+"senda-valoria-portal.png");
            game.CanonicalBastion=Canonical<Texture2D>("bastion_i.png",Full+"bastion_i.png");
            game.CanonicalRuinA=Canonical<Texture2D>("ruin_arch_0.png",Full+"ruin_arch_0.png");
            game.CanonicalRuinB=Canonical<Texture2D>("ruin_arch_1.png",Full+"ruin_arch_1.png");
            if(game.CanonicalGuard)foreach(var asset in AssetDatabase.LoadAllAssetsAtPath(AssetDatabase.GetAssetPath(game.CanonicalGuard)))if(asset is AnimationClip clip){if(clip.name=="Aldric_Walk_v1")game.GuardWalk=clip;if(clip.name=="Aldric_SwordAttack_v1")game.GuardAttack=clip;}
            game.CanonicalGate=Canonical<GameObject>("Valoria_MainGate_CohV1.glb",Family+"Valoria_MainGate_CohV1.glb");
            game.CanonicalTower=Canonical<GameObject>("Valoria_Tower_CohV1.glb",Family+"Valoria_Tower_CohV1.glb");
            game.CanonicalWall=Canonical<GameObject>("Valoria_WallSegment_CohV1.glb",Family+"Valoria_WallSegment_CohV1.glb");
            game.CanonicalSpawnling=Canonical<GameObject>("SpawnlingNear.glb","Assets/Eldoria/World4XCanon/Resources/World4X/Creatures/SpawnlingNear.glb");
            game.CanonicalPaving=Canonical<Texture2D>("DistrictPaving.jpg",Full+"DistrictPaving.jpg");
            game.CanonicalMeadow=Canonical<Texture2D>("BastionMeadow.jpg",Full+"BastionMeadow.jpg");
            game.CanonicalCrest=Canonical<Texture2D>("identity_crest.png",Full+"UI/CoronaBrechaPremium/identity_crest.png");
            game.CanonicalPanel=Canonical<Texture2D>("panel_obsidian.png",Full+"UI/CoronaBrechaPremium/panel_obsidian.png");
            game.CanonicalPines=new Texture2D[3];
            for(int i=0;i<3;i++)game.CanonicalPines[i]=Canonical<Texture2D>("pine_"+i+".png",Full+"pine_"+i+".png");
            Debug.Log("SENDA_CANONICAL_IMPORT status hero="+(bool)game.CanonicalAldric+" paving="+(bool)game.CanonicalPaving+" meadow="+(bool)game.CanonicalMeadow+" crest="+(bool)game.CanonicalCrest+" panel="+(bool)game.CanonicalPanel);
            if(game.CanonicalAldric){
                var path=AssetDatabase.GetAssetPath(game.CanonicalAldric);
                foreach(var clip in AssetDatabase.LoadAllAssetsAtPath(path)){
                    if(clip is AnimationClip a){
                        Debug.Log("SENDA_CANONICAL_CLIP "+a.name+" duration="+a.length);
                        if(a.name=="Aldric_Walk_v1")game.CanonicalWalk=a;
                        if(a.name=="Aldric_SwordAttack_v1")game.CanonicalAttack=a;
                    }
                }
            }
        }
        private static void CreateIsolatedScene(bool save)
        {
            var scene=EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
            var root=new GameObject("SendaBrecha standalone 3D - prototype (runtime generated)");
            ConfigureCanonical(root.AddComponent<SendaBrechaPrototype>());
            Selection.activeGameObject=root;
            if(save)
            {
                const string folder="Assets/Eldoria/SendaBrecha/Scenes";
                if(!AssetDatabase.IsValidFolder(folder))AssetDatabase.CreateFolder("Assets/Eldoria/SendaBrecha","Scenes");
                const string path=folder+"/SendaBrecha_Experimental.unity";
                Need(EditorSceneManager.SaveScene(scene,path),"Failed to save isolated Senda scene: "+path);
                Debug.Log("SENDA_STANDALONE_SCENE_SAVED "+path+" (not in production build scenes)");
            }
        }
        public static void Run()
        {
            if(!UnityEngine.Application.isBatchMode)
                throw new InvalidOperationException("Native automated proof requires a disposable batchmode Unity instance.");
            int code=1;
            try
            {
                string output=Environment.GetEnvironmentVariable("SENDA_BRECHA_CAPTURE_DIR");
                if(string.IsNullOrWhiteSpace(output))
                    output=Path.GetFullPath(Path.Combine(UnityEngine.Application.dataPath,"../../senda-brecha-native-evidence"));
                Directory.CreateDirectory(output);
                EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
                var root=new GameObject("SendaBrecha isolated native QA root");
                var game=root.AddComponent<SendaBrechaPrototype>();
                ConfigureCanonical(game);
                Need(game.CanonicalAldric&&game.CanonicalWalk&&game.CanonicalAttack&&game.CanonicalPaving&&game.CanonicalMeadow&&game.CanonicalCrest&&game.CanonicalPanel&&game.CanonicalGuard&&game.GuardWalk&&game.GuardAttack&&game.StudioSlate&&game.CanonicalBastion&&game.CanonicalRuinA&&game.CanonicalRuinB&&game.StudioBreach&&game.StudioPortal&&game.StudioRuin,"Original imported assets / clips missing");
                QualitySettings.shadows=ShadowQuality.All;QualitySettings.shadowDistance=38;QualitySettings.shadowResolution=ShadowResolution.Medium;
                game.Initialize();
                Need(game.NativeBreachInstalled&&game.NativeRuinInstalled&&game.MovingVergeCount>20&&game.StudioPortalCount==3&&game.ActorShadowCasters>10,"Native landmark, actual portal contour and actor shadows required");
                Need(game.CanonicalSkinCount>0&&game.CanonicalArchitectureCount==3&&game.CanonicalFoliageCount>25&&game.CanonicalEnemyInstalled&&game.CanonicalAnimationSampled&&game.NativeGuardRigs==24&&game.GuardTriangles>1000&&game.GuardTriangles<=12000&&game.SlateInstalled,"Original guard rigs and Studio slate not installed in real scene");
                Need(game.SceneCamera!=null && game.SceneCamera.gameObject.scene==SceneManager.GetActiveScene(),
                    "Generated camera must belong to isolated scene");
                Need(SendaBrechaPrototype.Waves.Length==6, "Exactly six event chapters expected");
                Need(game.HasLoadedScene,"Procedural 3D runtime world must initialize");
                SaveCapture(game.SceneCamera,output,"01-intro-390x844.png",390,844);
                // The initial prototype incorrectly showed 14 actors at 22 virtual troops,
                // so recruiting did not visibly grow the army. This regression must fail.
                game.Begin();
                int initialVisualCount=game.VisibleSoldiers;
                game.SetLane(0);Advance(game,4.5f);
                Need(game.GateTaken && game.Soldiers==32 && game.VisibleSoldiers>initialVisualCount,
                    "Recruit gate must visibly grow the real 3D soldier formation");
                SaveCapture(game.SceneCamera,output,"02-recruits-grow-on-gate-390x844.png",390,844);
                game.Begin();
                Need(game.State==SendaBrechaPrototype.Phase.Running && game.Soldiers==22,"Start gameplay state");
                game.SetLane(0);Advance(game,3);
                SaveCapture(game.SceneCamera,output,"02-choose-lane-390x844.png",390,844);
                Need(game.Lane==0 && !game.GateTaken,"Three-dimensional lane steering before gate");
                game.SetLane(1);Advance(game,1.5f);
                Need(game.GateTaken && game.Soldiers==22 && game.Power==35,"Power portal must apply exactly once");
                SaveCapture(game.SceneCamera,output,"03-portal-passed-390x844.png",390,844);
                Advance(game,3.5f);
                SaveCapture(game.SceneCamera,output,"04-real-enemy-render-390x844.png",390,844);
                SaveCapture(game.SceneCamera,output,"05-real-enemy-landscape-844x390.png",844,390);
                Advance(game,2.0f);
                Need(game.FightCompleted,"First battle must complete deterministically");
                Need(game.Score>0,"Score must follow the battle");
                SaveCapture(game.SceneCamera,output,"06-after-battle-390x844.png",390,844);
                int remaining=game.Soldiers;
                Need(remaining>0,"First encounter must not automatically defeat new player");
                while(game.State==SendaBrechaPrototype.Phase.Running && game.Elapsed<80)
                {
                    // Predictable route with favorable gates, full playable six stages.
                    if(game.Stage==2 || game.Stage==4)game.SetLane(1);
                    else game.SetLane(0);
                    Advance(game,.25f);
                }
                Need(game.State!=SendaBrechaPrototype.Phase.Running,"Finite encounter, no endless stage loop");
                Need(game.State==SendaBrechaPrototype.Phase.Victory && game.Stage==5,
                    "Favorable real six-stage route must result in victory");
                SaveCapture(game.SceneCamera,output,"08-victory-390x844.png",390,844);
                int victorySoldiers=game.Soldiers;
                game.Begin();
                while(game.State==SendaBrechaPrototype.Phase.Running && game.Elapsed<80)
                {
                    game.SetLane(0);Advance(game,.25f);
                }
                Need(game.State==SendaBrechaPrototype.Phase.Defeat,
                    "Risk route must also allow genuine defeat and replay");
                SaveCapture(game.SceneCamera,output,"09-defeat-390x844.png",390,844);
                game.Begin();
                Need(game.State==SendaBrechaPrototype.Phase.Running && game.Soldiers==22 && game.Stage==0,
                    "Replay must reset all virtual progression without touching game state");
                var report=new ProofReport
                {
                    mainSceneModified=false,physicalIphoneTested=false,artApproved=false,
                    studioBreachInstalled=game.NativeBreachInstalled,studioRuinInstalled=game.NativeRuinInstalled,movingVergeCount=game.MovingVergeCount,studioPortalCount=game.StudioPortalCount,actorShadowCasters=game.ActorShadowCasters,nativeGuardRigs=game.NativeGuardRigs,guardTriangles=game.GuardTriangles,slateInstalled=game.SlateInstalled,originalAldricSkins=game.CanonicalSkinCount,originalArchitecture=game.CanonicalArchitectureCount,originalFoliage=game.CanonicalFoliageCount,originalEnemy=game.CanonicalEnemyInstalled,originalAnimationSampled=game.CanonicalAnimationSampled,
                    capturedPng=9,finalSoldiers=victorySoldiers,totalStages=6,
                    positiveGateChangesVisibleActors=true,victoryRouteChecked=true,defeatRouteChecked=true,
                    result="VictoryAndDefeatNativeSimulationPass"
                };
                File.WriteAllText(Path.Combine(output,"proof.json"),JsonUtility.ToJson(report,true));
                Debug.Log("SENDA_NATIVE_PROOF_PASS stages=6 result="+report.result+" captures="+report.capturedPng+
                    " (does not certify physical iPhone or art approval)");
                code=0;
            }
            catch(Exception e)
            {
                Debug.LogError("SENDA_NATIVE_PROOF_FAILED: "+e);
            }
            finally
            {
                EditorApplication.Exit(code);
            }
        }
    }
}
#endif
