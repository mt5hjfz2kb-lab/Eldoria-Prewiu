#if UNITY_EDITOR
using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Reflection;
using Eldoria.VisualKit;
using Eldoria.UI.RoyalGold;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace Eldoria.VisualKit.Editor
{
    /// <summary>
    /// Same-scene, same-save native Unity A/B evidence. The actual screenshot
    /// function is the certified ValoriaIntegrationBuild.Capture camera/canvas
    /// pipeline, located by reflection so no game Editor assembly is patched.
    ///
    /// Captures only a disposable VALORIA_GAMEPLAY_PROBE session in batchmode.
    /// Never launches a second Unity process or changes user saves or scenes.
    /// Requires the live Unity Editor, same project's native scene and genuine
    /// RoyalHUD, not a rendering mockup. Fail closed if any of these are absent.
    /// </summary>
    [InitializeOnLoad]
    public static class ForjaOscuraNativeEvidence
    {
        const string Flag="ForjaOscura.Evidence.Active";
        const string BatchFlag="ForjaOscura.Evidence.Batch";
        const string Scene="Assets/Eldoria/ArtTests/ValoriaReferenceClean/Integration/Valoria_City_World_UI.unity";
        const string SkinOptIn="Eldoria.ForjaOscura.LivePreview.EditorOnly.v02";
        static double deadline;
        static bool running;

        static ForjaOscuraNativeEvidence()
        {
            EditorApplication.update -= Poll;
            EditorApplication.update += Poll;
        }

        [MenuItem("Eldoria/Forja Oscura/Capturar A-B nativo sin alterar escena")]
        public static void CaptureInPlay()
        {
            if (!EditorApplication.isPlaying) throw new InvalidOperationException("Enter Play in the canonical scene first.");
            RunCapture(false);
        }

        // Example: Unity -batchmode -force-d3d11 -projectPath <Unity> -executeMethod
        // Eldoria.VisualKit.Editor.ForjaOscuraNativeEvidence.CaptureBatch -logFile <log>
        public static void CaptureBatch()
        {
            if (EditorApplication.isPlaying) throw new InvalidOperationException("Batch capture needs an initial Editor state.");
            Environment.SetEnvironmentVariable("VALORIA_GAMEPLAY_PROBE", "1");
            EditorPrefs.SetBool(SkinOptIn, false);
            EditorSceneManager.OpenScene(Scene, OpenSceneMode.Single);
            SessionState.SetBool(Flag, true);
            SessionState.SetBool(BatchFlag, true);
            deadline = EditorApplication.timeSinceStartup + 240;
            EditorApplication.EnterPlaymode();
        }

        static void Poll()
        {
            if (!SessionState.GetBool(Flag, false) || running || !EditorApplication.isPlaying) return;
            if (deadline == 0) deadline = EditorApplication.timeSinceStartup + 240;
            if (EditorApplication.timeSinceStartup > deadline) { Fail("Timed out waiting for the genuine gameplay scene and HUD"); return; }
            var host = FindHost();
            if (!host) return;
            bool ready = BoolProperty(host,"Ready");
            if (!ready) return;
            var hud = HostHud(host);
            if (!hud) return;
            running = true;
            SessionState.SetBool(Flag, false);
            try { RunCapture(true); }
            catch (Exception e) { Debug.LogException(e); Fail(e.ToString()); }
            finally { running = false; }
        }

        static MonoBehaviour FindHost() => Object.FindObjectsByType<MonoBehaviour>(
            FindObjectsInactive.Exclude,FindObjectsSortMode.None)
            .FirstOrDefault(m=>m && m.GetType().FullName == "Eldoria.ReferenceClean.ValoriaIntegratedWorld");

        static bool BoolProperty(object obj,string name)
        {
            var prop=obj.GetType().GetProperty(name,BindingFlags.Public|BindingFlags.Instance);
            return prop!=null && prop.PropertyType==typeof(bool) && (bool)prop.GetValue(obj);
        }
        static RoyalHUD HostHud(object host)
        {
            return host?.GetType().GetProperty("Hud",BindingFlags.Public|BindingFlags.Instance)?.GetValue(host) as RoyalHUD;
        }
        static Type BuildType()
        {
            return AppDomain.CurrentDomain.GetAssemblies()
                .Select(a=>a.GetType("Eldoria.ReferenceClean.Editor.ValoriaIntegrationBuild",false))
                .FirstOrDefault(t=>t!=null);
        }
        static void Capture(Type captureType, object host,string basename,int width,int height,string outputDir,
            Action synchronizeVisuals = null)
        {
            // The public five-argument routine is the canonically established
            // native capture. For the candidate we must additionally refresh
            // nine-slice/nav geometry AFTER the offscreen Canvas gets its
            // new resolution. The canonical six-argument internal sibling
            // exposes precisely that pre-render callback. If it disappears in
            // a future project revision, FAIL instead of generating misleading
            // after screenshots with a stale layout.
            var all=captureType.GetMethods(BindingFlags.Public|BindingFlags.NonPublic|BindingFlags.Static);
            MethodInfo m;
            object[] args;
            if (synchronizeVisuals != null)
            {
                m=all.FirstOrDefault(x=>x.Name=="CapturePrepared"&&x.GetParameters().Length==6);
                if(m==null)throw new InvalidOperationException(
                    "Original native CapturePrepared pre-render callback missing; cannot prove responsive Forja UI");
                args=new object[]{host,basename,width,height,null,synchronizeVisuals};
            }
            else
            {
                m=all.FirstOrDefault(x=>x.Name=="Capture"&&x.GetParameters().Length==5);
                if(m==null)throw new InvalidOperationException(
                    "Original native 5-argument Capture missing; refuse simulated evidence");
                args=new object[]{host,basename,width,height,null};
            }
            var original=Path.GetFullPath(Path.Combine(Application.dataPath,
                "../../docs/evidence/valoria-reference-clean",basename));
            Directory.CreateDirectory(Path.GetDirectoryName(original));
            try{m.Invoke(null,args);}catch(TargetInvocationException e){throw e.InnerException??e;}
            if(!File.Exists(original)||new FileInfo(original).Length<4096)
                throw new IOException("Native Unity camera capture missing or tiny: "+original);
            File.Copy(original,Path.Combine(outputDir,basename),true);
        }
        [Serializable] class UiPixelDifference
        {
            public string resolution;
            public int navigationChanged;
            public int headerChanged;
            public int bodyChanged;
            public int navigationArea;
            public int headerArea;
        }

        private static int CountChangedInRect(Color32[] original, Color32[] candidate,
            int width, int height, float xmin, float xmax, float ymin, float ymax)
        {
            int left = Mathf.FloorToInt(width * xmin);
            int right = Mathf.CeilToInt(width * xmax);
            int bottom = Mathf.FloorToInt(height * ymin);
            int top = Mathf.CeilToInt(height * ymax);
            int result = 0;
            for (int y = bottom; y < top; y++)
                for (int x = left; x < right; x++)
                {
                    int i = y * width + x;
                    var a = original[i]; var b = candidate[i];
                    if (Math.Abs(a.r - b.r) + Math.Abs(a.g - b.g) +
                        Math.Abs(a.b - b.b) >= 50) result++;
                }
            return result;
        }

        private static UiPixelDifference CompareNativeUiRegions(string before, string after,
            int width, int height)
        {
            var a = new Texture2D(2, 2, TextureFormat.RGB24, false);
            var b = new Texture2D(2, 2, TextureFormat.RGB24, false);
            try
            {
                Need(a.LoadImage(File.ReadAllBytes(before), false), "Cannot decode real baseline PNG");
                Need(b.LoadImage(File.ReadAllBytes(after), false), "Cannot decode real candidate PNG");
                Need(a.width == width && a.height == height && b.width == width &&
                     b.height == height, "Native image dimensions differ from requested GameView");
                var p = a.GetPixels32();
                var q = b.GetPixels32();
                return new UiPixelDifference {
                    resolution = width + "x" + height,
                    navigationChanged = CountChangedInRect(p,q,width,height,0f,1f,0f,.20f),
                    headerChanged = CountChangedInRect(p,q,width,height,0f,1f,.80f,1f),
                    bodyChanged = CountChangedInRect(p,q,width,height,.27f,.73f,.31f,.68f),
                    navigationArea = Mathf.CeilToInt(width * height * .20f),
                    headerArea = Mathf.CeilToInt(width * height * .20f)
                };
            }
            finally
            {
                Object.DestroyImmediate(a);
                Object.DestroyImmediate(b);
            }
        }

        static string Hash(string file)
        {
            using(var sha=SHA256.Create())using(var fs=File.OpenRead(file))
                return BitConverter.ToString(sha.ComputeHash(fs)).Replace("-","").ToLowerInvariant();
        }
        static string Snapshot(RoyalHUD hud)
        {
            return (hud.Realm&&hud.Realm.Realm?hud.Realm.Realm.text:"") + "|" +
                   (hud.Resources&&hud.Resources.Wood?hud.Resources.Wood.text:"") + "|" +
                   (hud.Resources&&hud.Resources.Stone?hud.Resources.Stone.text:"") + "|" +
                   (hud.Resources&&hud.Resources.Food&&hud.Resources.Food.gameObject.activeSelf?hud.Resources.Food.text:"locked") + "|" +
                   (hud.Mission&&hud.Mission.Objective?hud.Mission.Objective.text:"") + "|" + hud.SelectedBuildingId;
        }
        static void Need(bool yes,string message){if(!yes)throw new InvalidOperationException(message);}
        static void ValidateNavigation(RoyalHUD hud)
        {
            var nav=hud.Navigation;
            Need(nav&&nav.Items!=null,"Existing navigation missing");
            int count=nav.Items.Count(i=>i!=null&&i.Button&&i.Button.gameObject.activeInHierarchy);
            Need(count==5,"Real nav must show exactly five canonical destinations, actual "+count);
            // Safe geometry and actual pointer dispatch, not synthetic onClick invocation.
            Canvas.ForceUpdateCanvases();
            if (!EventSystem.current) throw new InvalidOperationException("Existing EventSystem missing");
            foreach(var item in nav.Items.Where(i=>i!=null&&i.Button&&i.Button.gameObject.activeInHierarchy))
            {
                var button=item.Button;
                var rect=(RectTransform)button.transform;
                Need(rect.rect.width>=44&&rect.rect.height>=44,"Mobile destination hit target smaller than 44: "+item.Section);
                Need(button.GetComponentsInChildren<Graphic>(true).Any(g=>g.isActiveAndEnabled&&g.raycastTarget),
                    "Real destination has no enabled raycast graphic: "+item.Section);
                var corners=new Vector3[4];rect.GetWorldCorners(corners);
                var center=RectTransformUtility.WorldToScreenPoint(null,(corners[0]+corners[2])*.5f);
                // The native game handles real pointer events on the SAME
                // RoyalButton, regardless of whether the visible Image is a
                // child. Reject an overlay that steals the center hit.
                var pointer=new PointerEventData(EventSystem.current) {
                    pointerId=-71,button=PointerEventData.InputButton.Left,position=center
                };
                var hits=new List<RaycastResult>();EventSystem.current.RaycastAll(pointer,hits);
                Need(hits.Count>0 && ExecuteEvents.GetEventHandler<IPointerClickHandler>(hits[0].gameObject)==button.gameObject,
                    "An overlay intercepts the real navigation button: "+item.Section);

            }
        }
        static void RunCapture(bool batch)
        {
            var host=FindHost();Need(host&&BoolProperty(host,"Ready"),"Canonical gameplay host must be ready");
            var hud=HostHud(host);Need(hud,"Single RoyalHUD from canonical game host missing");
            var t=BuildType();Need(t!=null,"Original native Unity capture class missing");
            var probe=host.GetType().GetProperty("City")?.GetValue(host);
            Need(!batch || (probe!=null&&BoolProperty(probe,"Probe")),
                "Batch evidence REQUIRES disposable city Probe; refuse any real player save");
            if (batch)
            {
                var nickname=t.GetMethods(BindingFlags.Public|BindingFlags.Static)
                    .FirstOrDefault(x=>x.Name=="CompleteProbePlayerName" && x.GetParameters().Length==1);
                Need(nickname!=null,"Fixture name helper missing");
                try{nickname.Invoke(null,new[]{(object)host});}catch(TargetInvocationException e){throw e.InnerException??e;}
                // Select a REAL resource-building parcel through the existing
                // gameplay host, to make the contextual offer visible in BOTH
                // native A and native B. Selection never executes the offer.
                var select=host.GetType().GetMethod("Select",BindingFlags.Public|BindingFlags.Instance,
                    null,new[]{typeof(string)},null);
                Need(select!=null && (bool)select.Invoke(host,new object[]{"sawmill"}),
                    "Existing city cannot select its genuine sawmill/buildable parcel");
                hud.Refresh();
                Need(hud.SelectedBuildingId=="sawmill",
                    "Capture must reflect a genuine selected building");
            }
            Need(!hud.NarrativeOpen,"Narrative overlay obscures city; dismiss it through real UI before comparing");
            // The canonical contextual card is reprojected from the actual
            // camera and selection by its public synchronizer. Merely calling
            // RoyalHUD.Refresh does not update this independent V3 overlay.
            var context = hud.GetComponent<Eldoria.UI.RoyalGold.V2.RoyalGoldV3ContextHud>();
            if (context) context.SynchronizeNow();
            EditorPrefs.SetBool(SkinOptIn, false);
            var prior=hud.GetComponent<ForjaOscuraLiveHudSkin>();
            if(prior)Object.DestroyImmediate(prior);
            var original=Snapshot(hud);
            var root=Path.GetFullPath(Path.Combine(Application.dataPath,"../ForjaNativeEvidence"));
            Directory.CreateDirectory(root);
            const string before="native-before",after="native-after";
            foreach(var wh in new[]{(1600,900),(900,1600),(844,390),(390,844)})
                Capture(t,host,before+"-"+wh.Item1+"x"+wh.Item2+".png",wh.Item1,wh.Item2,root);
            ValidateNavigation(hud);
            var skin=hud.gameObject.AddComponent<ForjaOscuraLiveHudSkin>();
            Need(skin!=null,"Skin creation failed");
            skin.ApplyNow();Canvas.ForceUpdateCanvases();skin.ApplyNow();
            Need(skin.NavigationSkinned && skin.ObjectiveSkinned && skin.BuildingSkinned,
                 "Not all five existing nav/objective/building surfaces are skinned");
            if (context) {
                context.SynchronizeNow();
                skin.ApplyNow();
                Need(!context.ContextVisible || skin.ContextSkinned,
                    "Genuine V3 contextual building visible without the new Forja skin");
            }
            Need(hud.GetComponents<Canvas>().Length==1, "New standalone Canvas introduced unexpectedly");
            ValidateNavigation(hud);
            foreach(var wh in new[]{(1600,900),(900,1600),(844,390),(390,844)})
                Capture(t,host,after+"-"+wh.Item1+"x"+wh.Item2+".png",wh.Item1,wh.Item2,root,
                    () => { skin.ApplyNow(true); Canvas.ForceUpdateCanvases(); });
            Need(original==Snapshot(hud),"HUD gameplay data changed while applying presentation-only skin");
            Object.DestroyImmediate(skin);Canvas.ForceUpdateCanvases();
            ValidateNavigation(hud);
            var originalAfter=Snapshot(hud);
            Need(original==originalAfter,"Restoring skin changed existing HUD data");
            var files=new[]{(1600,900),(900,1600),(844,390),(390,844)}
                .SelectMany(wh=>new[]{before,after}.Select(prefix=>Path.Combine(root,
                    prefix+"-"+wh.Item1+"x"+wh.Item2+".png"))).ToArray();
            Need(files.Length==8&&files.All(f=>File.Exists(f)&&new FileInfo(f).Length>4096),
                 "Missing or invalid native A/B PNGs; expected eight exact output names");
            int differences=0;
            var uiDifferences = new List<UiPixelDifference>();
            foreach(var wh in new[]{(1600,900),(900,1600),(844,390),(390,844)})
            {
                var pathA=Path.Combine(root,before+"-"+wh.Item1+"x"+wh.Item2+".png");
                var pathB=Path.Combine(root,after+"-"+wh.Item1+"x"+wh.Item2+".png");
                if(Hash(pathA)!=Hash(pathB))differences++;
                var proof=CompareNativeUiRegions(pathA,pathB,wh.Item1,wh.Item2);
                uiDifferences.Add(proof);
                // Hash differences can result from a running world animation.
                // Demand visible change specifically in bottom navigation AND
                // top resource/identity UI for every target orientation.
                Need(proof.navigationChanged >= Mathf.Max(40,proof.navigationArea/2500),
                    "Native Forja navigation does not visibly change at "+proof.resolution);
                Need(proof.headerChanged >= Mathf.Max(40,proof.headerArea/2500),
                    "Native Forja header does not visibly change at "+proof.resolution);
            }
            Need(differences==4,"Native before/after frames have no detectable change in some orientations");
            var report=new EvidenceReport{
                status="UNITY_NATIVE_CAPTURED_VISUAL_REVIEW_PENDING",sourceScene=Scene,
                gameplayHost=host.GetType().FullName,dataBefore=original,dataAfter=originalAfter,
                screenshotCount=files.Length,orientationsWithDifference=differences,
                uiRegionDifferences=uiDifferences.ToArray(),
                captureMethod="ValoriaIntegrationBuild.Capture: Unity RenderTexture/Canvas",
                artifacts=files.Select(f=>new Artifact {name=Path.GetFileName(f),sha256=Hash(f)}).ToArray(),
                gameplayAndSaveRulesChanged=false,physicalMobileTested=false,artApproved=false
            };
            File.WriteAllText(Path.Combine(root,"ForjaNativeEvidence.json"),JsonUtility.ToJson(report,true));
            Debug.Log("[FORJA] REAL UNITY A/B PASS: "+root+" / 8 native images, 4 changed orientations, original HUD values unchanged");
            if(batch)EditorApplication.Exit(0);
        }
        static void Fail(string reason)
        {
            SessionState.SetBool(Flag,false);Debug.LogError("[FORJA] NATIVE_CAPTURE_FAILED: "+reason);
            if(SessionState.GetBool(BatchFlag,false))EditorApplication.Exit(1);
        }
        [Serializable] class Artifact{public string name,sha256;}
        [Serializable] class EvidenceReport
        {
            public string status,sourceScene,gameplayHost,dataBefore,dataAfter,captureMethod;
            public int screenshotCount,orientationsWithDifference;
            public UiPixelDifference[] uiRegionDifferences;
            public bool gameplayAndSaveRulesChanged,physicalMobileTested,artApproved;
            public Artifact[] artifacts;
        }
    }
}
#endif
