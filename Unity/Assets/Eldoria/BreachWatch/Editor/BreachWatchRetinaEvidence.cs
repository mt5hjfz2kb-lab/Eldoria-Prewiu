#if UNITY_EDITOR
using System;
using System.IO;
using System.Security.Cryptography;
using System.Collections.Generic;
using Eldoria.ReferenceClean;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace Eldoria.BreachWatch.Editor
{
    // QA-only 3x render. It does NOT configure WebGL devicePixelRatio,
    // allocate a player framebuffer, alter assets, save or create a second HUD.
    // The 1x and 3x layouts share the exact same logical phone viewport.
    public static class BreachWatchRetinaEvidence
    {
        [Serializable] public sealed class RetinaShot
        {
            public string file,sha256,selection;
            public int logicalWidth,logicalHeight,physicalWidth,physicalHeight;
            public float density,authoredScale,retinaScale;
            public bool realUnity,noUserSaveChanges;
        }
        [Serializable] public sealed class RetinaReport
        {
            public string status,source,notice;
            public bool artworkApproved,physicalIphoneTested,changedWebGLPixelRatio;
            public RetinaShot[] shots;
        }
        static void Need(bool value,string reason)
        {
            if(!value)throw new InvalidOperationException(reason);
        }
        static string Sha(string path)
        {
            using(var sha=SHA256.Create())
            using(var stream=File.OpenRead(path))
                return BitConverter.ToString(sha.ComputeHash(stream))
                    .Replace("-","").ToLowerInvariant();
        }
        public static void Capture(BreachWatchHud ui,ValoriaIntegratedWorld host,string output)
        {
            Need(ui && host && host.City && host.City.Probe,
                "Retina capture requires the actual disposable Unity city");
            var camera=host.PlayerCamera;
            Need(camera,"Camera for true Unity Retina capture is unavailable");
            string save=JsonUtility.ToJson(host.City.Gateway.Snapshot());
            string originalSelection=ui.SelectedBuildingId;
            var evidence=new List<RetinaShot>();
            var retinaFolder=Path.Combine(output,"retina-3x");
            Directory.CreateDirectory(retinaFolder);
            try
            {
                foreach(string selection in new[]{"","bastion"})
                {
                    ui.SelectBuilding(selection);
                    foreach(var logical in new[]{(390,844),(844,390),(900,1600)})
                        evidence.Add(Shoot(ui,host,camera,retinaFolder,selection,
                            logical.Item1,logical.Item2,save));
                }
            }
            finally
            {
                ui.SetNativeCaptureDensity(1f);
                ui.SelectBuilding(originalSelection);
                ui.Reflow(390,844);
                ui.RenderNow();
            }
            Need(save==JsonUtility.ToJson(host.City.Gateway.Snapshot()),
                "Native 3x capture mutated actual game state");
            var report=new RetinaReport {
                status="UNITY_RETINA_3X_CAPTURED_ART_REVIEW_PENDING",
                source=Environment.GetEnvironmentVariable("GITHUB_SHA")??"local",
                notice="Genuine 3x Unity Editor RenderTexture on real probe scene; not a physical iPhone, not a WebGL DPR measurement or artistic approval.",
                artworkApproved=false,physicalIphoneTested=false,
                changedWebGLPixelRatio=false,shots=evidence.ToArray()
            };
            File.WriteAllText(Path.Combine(retinaFolder,"retina-native-report.json"),
                JsonUtility.ToJson(report,true));
        }
        static RetinaShot Shoot(BreachWatchHud ui,ValoriaIntegratedWorld host,
            Camera camera,string output,string selection,int logicalWidth,
            int logicalHeight,string initialSave)
        {
            const int density=3;
            int pxWidth=logicalWidth*density,pxHeight=logicalHeight*density;
            var target=new RenderTexture(pxWidth,pxHeight,24,RenderTextureFormat.ARGB32);
            var image=new Texture2D(pxWidth,pxHeight,TextureFormat.RGB24,false);
            var previous=camera.targetTexture;
            var aspect=camera.aspect;
            var mask=camera.cullingMask;
            var active=RenderTexture.active;
            string label=selection==""?"city":"bastion";
            string filename="real-unity-"+label+"-"+logicalWidth+"x"+
                logicalHeight+"-at-3x.png";
            string path=Path.Combine(output,filename);
            try
            {
                camera.targetTexture=target;
                camera.aspect=pxWidth/(float)pxHeight;
                camera.cullingMask |= (1<<5);
                ui.NativeCanvas.worldCamera=camera;
                ui.SetNativeCaptureDensity(density);
                ui.Reflow(pxWidth,pxHeight);
                ui.RenderNow();
                Canvas.ForceUpdateCanvases();
                camera.Render();
                Canvas.ForceUpdateCanvases();
                camera.Render();
                float expected=(logicalHeight>logicalWidth
                    ?Mathf.Clamp(logicalWidth/514f,.91f,3f)
                    :Mathf.Clamp(Mathf.Min(logicalWidth/1250f,
                        logicalHeight/680f),.86f,1.3f));
                float actual=ui.GetComponent<CanvasScaler>().scaleFactor;
                Need(Mathf.Abs(actual/density-expected)<.02f,
                    "Retina layout size changed from source view at "+
                    logicalWidth+"x"+logicalHeight+
                    ": expected "+expected*density+" got "+actual);
                RenderTexture.active=target;
                image.ReadPixels(new Rect(0,0,pxWidth,pxHeight),0,0);
                image.Apply(false);
                File.WriteAllBytes(path,image.EncodeToPNG());
                Need(File.Exists(path) && new FileInfo(path).Length>16384,
                    "No genuine high-resolution Unity screenshot "+filename);
                bool safe=initialSave==JsonUtility.ToJson(host.City.Gateway.Snapshot());
                Need(safe,"Retina capture unexpectedly changed game state");
                return new RetinaShot {
                    file=filename,sha256=Sha(path),
                    selection=selection,logicalWidth=logicalWidth,
                    logicalHeight=logicalHeight,
                    physicalWidth=pxWidth,physicalHeight=pxHeight,
                    density=density,authoredScale=expected,retinaScale=actual,
                    realUnity=true,noUserSaveChanges=safe
                };
            }
            finally
            {
                camera.targetTexture=previous;
                camera.aspect=aspect;
                camera.cullingMask=mask;
                RenderTexture.active=active;
                ui.SetNativeCaptureDensity(1f);
                target.Release();Object.DestroyImmediate(target);
                Object.DestroyImmediate(image);
            }
        }
    }
}
#endif
