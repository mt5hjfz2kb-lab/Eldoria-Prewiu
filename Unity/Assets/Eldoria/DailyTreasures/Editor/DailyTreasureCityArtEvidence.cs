#if UNITY_EDITOR
using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Security.Cryptography;
using Eldoria.Application;
using Eldoria.Domain;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace Eldoria.DailyTreasures.Editor
{
    /// <summary>
    /// Read-only artistic context proof. It opens the ACTUAL checked-in Valoria
    /// city scene and renders the authored daily panel above its real camera.
    /// Deliberately not a gameplay, earned-Bastion, Vigilia-PR173 or device test:
    /// no canonical state/scene is saved and no milestone is invented.
    /// </summary>
    public static class DailyTreasureCityArtEvidence
    {
        const string Scene="Assets/Eldoria/ArtTests/ValoriaReferenceClean/Integration/Valoria_City_World_UI.unity";
        const string Folder="DailyTreasureCityArtEvidence";

        sealed class TemporaryClock : IClock
        {
            public long UtcTicks { get; set; } =
                new DateTime(2026,11,10,12,0,0,DateTimeKind.Utc).Ticks;
        }

        sealed class MemoryStateStore : IStateStore
        {
            PlayerState state;
            public PlayerState Load() => state;
            public void Save(PlayerState next) { state=next; }
        }

        [Serializable] sealed class Shot
        {
            public string file,sha256,variant;
            public int width,height,bytes;
        }

        [Serializable] sealed class Report
        {
            public string status,source,scene,camera,reason;
            public int cityRenderers,pngCount,realGatewayClaims;
            public bool citySceneOpened,cityCameraUsed,artApproved;
            public bool fullSceneVerified,pr173Integrated,iosVerified;
            public Shot[] shots;
        }

        static void Require(bool condition,string message)
        {
            if(!condition)throw new InvalidOperationException(message);
        }

        static string Hash(string path)
        {
            using(var stream=File.OpenRead(path))
            using(var sha=SHA256.Create())
                return BitConverter.ToString(sha.ComputeHash(stream))
                    .Replace("-","").ToLowerInvariant();
        }

        static Shot Photograph(Camera camera,RenderTexture target,
            string folder,int width,int height,string label,
            bool requireCityPixels)
        {
            var oldActive=RenderTexture.active;
            var texture=new Texture2D(width,height,TextureFormat.RGB24,false);
            try
            {
                Canvas.ForceUpdateCanvases();
                camera.Render();
                camera.Render();
                RenderTexture.active=target;
                texture.ReadPixels(new Rect(0,0,width,height),0,0);
                texture.Apply();
                // A black fixture with only a chest is not proof of a city.
                // Inspect the unoccluded BASELINE, not decorative UI pixels.
                if(requireCityPixels)
                {
                    int total=0,lit=0;
                    var colors=new HashSet<int>();
                    var pixels=texture.GetPixels32();
                    for(int y=height/8;y<height*7/8;y+=11)
                        for(int x=width/8;x<width*7/8;x+=11)
                        {
                            var color=pixels[y*width+x];
                            int brightness=color.r+color.g+color.b;
                            total++;
                            if(brightness>30)lit++;
                            colors.Add(((int)color.r>>4)*256+
                                ((int)color.g>>4)*16+((int)color.b>>4));
                        }
                    Require(total>30 && lit>total/7 && colors.Count>=35,
                        "Real city baseline is dark/blank or camera points away: "+
                        "lit="+lit+"/"+total+" colors="+colors.Count);
                }
                string name="real-city-daily-"+label+"-"+width+"x"+height+".png";
                string file=Path.Combine(folder,name);
                var bytes=texture.EncodeToPNG();
                Require(bytes.Length>12000,"Empty city render: "+name);
                File.WriteAllBytes(file,bytes);
                return new Shot{file=name,width=width,height=height,
                    variant=label,bytes=bytes.Length,sha256=Hash(file)};
            }
            finally
            {
                RenderTexture.active=oldActive;
                Object.DestroyImmediate(texture);
            }
        }


        // A visual-only Bastion ceremony needs an actual camera view of the
        // already built canonical structure. Digitally zooming a flat PNG
        // pixelates masonry; moving the genuine Unity camera does not.
        // Never substitute a generated future castle or modify the scene.
        static Bounds FindVisibleRealBastion(MonoBehaviour city)
        {
            foreach(var slot in new[]{"BastionOne","BastionTwo"})
            {
                var type=city.GetType();
                var property=type.GetProperty(slot);
                var field=type.GetField(slot);
                var gameObject=property!=null
                    ?property.GetValue(city,null) as GameObject
                    :field!=null?field.GetValue(city) as GameObject:null;
                if(!gameObject || !gameObject.activeInHierarchy)continue;
                var visible=gameObject.GetComponentsInChildren<Renderer>(true)
                    .Where(r=>r && r.enabled && r.gameObject.activeInHierarchy)
                    .ToArray();
                if(visible.Length==0)continue;
                var bounds=visible[0].bounds;
                for(int i=1;i<visible.Length;i++)
                    bounds.Encapsulate(visible[i].bounds);
                Require(bounds.size.sqrMagnitude>.01f,
                    "Authentic "+slot+" has invalid renderer bounds");
                return bounds;
            }
            throw new InvalidOperationException(
                "No active canonical BastionOne/BastionTwo mesh in the real city; "+
                "do not substitute a screenshot, a fake building or an inactive future tier");
        }

        static void FrameRealBastion(Camera camera,Bounds bounds)
        {
            // Keep the player's genuine camera orientation. Only its position
            // and lens vary for this disposable illustration-only capture.
            var right=camera.transform.right;
            var up=camera.transform.up;
            var forward=camera.transform.forward;
            var e=bounds.extents;
            float dx=Mathf.Abs(right.x)*e.x+Mathf.Abs(right.y)*e.y+
                Mathf.Abs(right.z)*e.z;
            float dy=Mathf.Abs(up.x)*e.x+Mathf.Abs(up.y)*e.y+
                Mathf.Abs(up.z)*e.z;
            float dz=Mathf.Abs(forward.x)*e.x+Mathf.Abs(forward.y)*e.y+
                Mathf.Abs(forward.z)*e.z;
            float aspect=Mathf.Max(.25f,camera.aspect);
            if(camera.orthographic)
            {
                camera.orthographicSize=Mathf.Max(1f,
                    Mathf.Max(dy/.63f,dx/(aspect*.74f)));
                float depth=Mathf.Max(12f,dz*3f);
                camera.transform.position=bounds.center-forward*depth;
            }
            else
            {
                float tanY=Mathf.Tan(camera.fieldOfView*Mathf.Deg2Rad*.5f);
                float distance=Mathf.Max(dy/(tanY*.63f),
                    dx/(tanY*aspect*.74f))+dz*1.5f;
                camera.transform.position=bounds.center-forward*Mathf.Max(12f,distance);
            }
        }

        public static void Capture()
        {
            string project=Path.GetFullPath(
                Path.Combine(UnityEngine.Application.dataPath,".."));
            string folder=Path.GetFullPath(Path.Combine(project,Folder));
            Require(string.Equals(Path.GetDirectoryName(folder),project,
                    StringComparison.OrdinalIgnoreCase) &&
                Path.GetFileName(folder)==Folder,
                "Refuse to clear a folder outside isolated city art proof");
            if(Directory.Exists(folder))Directory.Delete(folder,true);
            Directory.CreateDirectory(folder);
            var report=new Report{
                status="CITY_ART_CONTEXT_INCOMPLETE",
                source=Environment.GetEnvironmentVariable("GITHUB_SHA")??"local",
                scene=Scene,reason="Read-only city composition only; no real city gameplay, PR173 HUD or physical device certified",
                artApproved=false,fullSceneVerified=false,pr173Integrated=false,
                iosVerified=false,realGatewayClaims=0,shots=new Shot[0]
            };
            var pictures=new List<Shot>();
            Camera camera=null;
            RenderTexture originalTarget=null;
            int originalMask=0;
            float originalAspect=1;
            Vector3 originalPosition=Vector3.zero;
            Quaternion originalRotation=Quaternion.identity;
            float originalOrthographicSize=1f;
            float originalFieldOfView=60f;
            GameObject canvasObject=null;
            try
            {
                var scene=EditorSceneManager.OpenScene(Scene,OpenSceneMode.Single);
                Require(scene.IsValid() && scene.path==Scene,
                    "The exact canonical Valoria city scene was not opened");
                report.citySceneOpened=true;
                var components=Object.FindObjectsByType<MonoBehaviour>(
                    FindObjectsInactive.Include,FindObjectsSortMode.None);
                var host=components.FirstOrDefault(x=>x &&
                    x.GetType().Name=="ValoriaIntegratedWorld");
                var city=components.FirstOrDefault(x=>x &&
                    x.GetType().Name=="ValoriaReferenceCleanGameplay");
                Require(host && city &&
                    host.gameObject==city.gameObject,
                    "Real ValoriaIntegratedWorld and gameplay host are not in canonical city scene");
                var cityCameraProperty=city.GetType().GetProperty("PlayerCamera");
                camera=cityCameraProperty?.GetValue(city,null) as Camera;
                Require(camera && camera.gameObject.scene==scene,
                    "Canonical city player camera is not available in scene");
                report.camera=camera.name;
                report.cityCameraUsed=true;
                report.cityRenderers=Object.FindObjectsByType<Renderer>(
                    FindObjectsInactive.Exclude,FindObjectsSortMode.None)
                    .Count(x=>x && x.enabled);
                Require(report.cityRenderers>=30,
                    "Real city scene has too few active visible 3D renderers: "+
                    report.cityRenderers);

                // The test only previews a REAL daily calendar. The actual
                // player's gateway, nickname, tutorials and save are untouched.
                var clock=new TemporaryClock();
                var gateway=new LocalGateway(clock,new MemoryStateStore());
                var before=gateway.Snapshot();
                originalTarget=camera.targetTexture;
                originalAspect=camera.aspect;
                originalMask=camera.cullingMask;
                originalPosition=camera.transform.position;
                originalRotation=camera.transform.rotation;
                originalOrthographicSize=camera.orthographicSize;
                originalFieldOfView=camera.fieldOfView;
                // This must resolve actual visible Bastion renderers in the
                // already opened canonical city scene, not an artist proxy.
                var bastionBounds=FindVisibleRealBastion(city);
                camera.cullingMask|=1<<5;
                canvasObject=new GameObject("TEMP / City daily art-context only",
                    typeof(RectTransform),typeof(Canvas),
                    typeof(CanvasScaler),typeof(GraphicRaycaster));
                canvasObject.layer=5;
                var canvas=canvasObject.GetComponent<Canvas>();
                canvas.renderMode=RenderMode.ScreenSpaceCamera;
                canvas.worldCamera=camera;
                canvas.planeDistance=1f;
                canvas.sortingOrder=5000;
                var scaler=canvasObject.GetComponent<CanvasScaler>();
                scaler.uiScaleMode=CanvasScaler.ScaleMode.ConstantPixelSize;
                scaler.scaleFactor=1f;
                var safeObject=new GameObject("Read-only art proof safe area",
                    typeof(RectTransform));
                safeObject.layer=5;
                safeObject.transform.SetParent(canvasObject.transform,false);
                var safe=(RectTransform)safeObject.transform;
                safe.anchorMin=Vector2.zero;
                safe.anchorMax=Vector2.one;
                safe.offsetMin=Vector2.zero;
                safe.offsetMax=Vector2.zero;
                var panel=DailyTreasurePanel.Install(safe);
                panel.Bind(gateway,()=>clock.UtcTicks,()=>{});
                Require(panel.HasAuthoredChestArt,
                    "City context lacks ORIGINAL coffer illustration");
                panel.Suppressed=true;
                foreach(var size in new []{(1600,900),(900,1600),(844,390),(390,844)})
                {
                    int width=size.Item1,height=size.Item2;
                    var root=(RectTransform)canvasObject.transform;
                    root.sizeDelta=new Vector2(width,height);
                    camera.aspect=width/(float)height;
                    camera.transform.position=originalPosition;
                    camera.transform.rotation=originalRotation;
                    camera.orthographicSize=originalOrthographicSize;
                    camera.fieldOfView=originalFieldOfView;
                    var target=new RenderTexture(width,height,24,
                        RenderTextureFormat.ARGB32);
                    try
                    {
                        camera.targetTexture=target;
                        Canvas.ForceUpdateCanvases();
                        var baseline=Photograph(camera,target,folder,width,height,
                            "actual-city-baseline",true);
                        pictures.Add(baseline);
                        panel.Suppressed=false;
                        panel.Open();
                        Require(panel.IsOpen,
                            "Daily calendar not visible above real city artwork");
                        Canvas.ForceUpdateCanvases();
                        var withCalendar=Photograph(camera,target,folder,width,height,
                            "authored-calendar-context",false);
                        pictures.Add(withCalendar);
                        Require(!string.Equals(baseline.sha256,withCalendar.sha256,
                                StringComparison.OrdinalIgnoreCase),
                            "Real city and daily panel images are identical: "+
                            width+"x"+height+"; UI was not drawn over the scene");
                        panel.Close();
                        // A second visual-only capture proves the actual city
                        // stays beneath the BASTION ceremonial coffer, rather
                        // than a fake future-building painting or a black
                        // component fixture. No level/report/reward is earned.
                        panel.Open();
                        Require(panel.IsOpen,
                            "Visual-only Bastion theatre did not open");
                        panel.Present("BASTIÓN · ESTUDIO VISUAL",
                            "PREVISUALIZACIÓN SIN HITO NI RECOMPENSA",
                            true,DailyTreasureCeremonyTier.Special,
                            "milestone:bastion:visual-only");
                        Require(panel.ActiveMilestoneStage==
                                DailyTreasureStageKind.Bastion &&
                            panel.HasMilestoneScenery &&
                            panel.MilestoneSceneryKey==
                                DailyTreasureBastionCamera.SceneKey &&
                            !panel.HasDistinctMilestoneArt &&
                            panel.MilestoneArtworkResourceKey==null,
                            "Real built Bastion must be the only architecture; "+
                            "a painted extra gate is forbidden");
                        Canvas.ForceUpdateCanvases();
                        var bastionStudy=Photograph(camera,target,folder,
                            width,height,"authored-bastion-visual-only",false);
                        pictures.Add(bastionStudy);
                        Require(baseline.sha256!=bastionStudy.sha256,
                            "Bastion stage was absent above real city camera");
                        panel.Close();
                        panel.Suppressed=true;
                        // High-resolution genuine camera framing, NOT raster
                        // scaling of a city screenshot. Keep a paired baseline
                        // from the same lens so visual reviewers can judge
                        // both architecture and whether the overlay hides it.
                        FrameRealBastion(camera,bastionBounds);
                        var heroBaseline=Photograph(camera,target,folder,
                            width,height,"real-bastion-camera-baseline",true);
                        pictures.Add(heroBaseline);
                        panel.Suppressed=false;
                        panel.Open();
                        Require(panel.IsOpen,
                            "Focused real Bastion visual-only stage did not open");
                        panel.Present("BASTIÓN · ESTUDIO VISUAL",
                            "PREVISUALIZACIÓN SIN HITO NI RECOMPENSA",
                            true,DailyTreasureCeremonyTier.Special,
                            "milestone:bastion:visual-only");
                        Canvas.ForceUpdateCanvases();
                        var heroStage=Photograph(camera,target,folder,
                            width,height,"real-bastion-camera-ceremony",false);
                        pictures.Add(heroStage);
                        Require(heroBaseline.sha256!=heroStage.sha256,
                            "Actual-camera Bastion ceremony was never drawn");
                        panel.Close();
                        panel.Suppressed=true;
                    }
                    finally
                    {
                        camera.targetTexture=null;
                        target.Release();
                        Object.DestroyImmediate(target);
                    }
                }
                var after=gateway.Snapshot();
                Require(before.Revision==after.Revision &&
                    before.Resources.Wood==after.Resources.Wood &&
                    before.Resources.Stone==after.Resources.Stone &&
                    before.Resources.Food==after.Resources.Food &&
                    before.Mailbox.Messages.Count==after.Mailbox.Messages.Count,
                    "An ART-ONLY city preview unexpectedly changed real rewards");
                Require(pictures.Count==20,
                    "Expected 20 genuine city, Bastion context AND canonical camera-framed PNGs");
                report.pngCount=pictures.Count;
                report.status="CITY_ART_CONTEXT_RENDERED_NOT_GAMEPLAY_CERTIFIED";
            }
            catch(Exception ex)
            {
                report.status="CITY_ART_CONTEXT_FAILED";
                report.reason=ex.ToString();
                Debug.LogException(ex);
            }
            finally
            {
                report.shots=pictures.ToArray();
                if(camera)
                {
                    camera.targetTexture=originalTarget;
                    camera.cullingMask=originalMask;
                    camera.aspect=originalAspect;
                    camera.transform.position=originalPosition;
                    camera.transform.rotation=originalRotation;
                    camera.orthographicSize=originalOrthographicSize;
                    camera.fieldOfView=originalFieldOfView;
                }
                if(canvasObject)Object.DestroyImmediate(canvasObject);
                File.WriteAllText(Path.Combine(folder,"city-art-report.json"),
                    JsonUtility.ToJson(report,true));
                // The scene is never saved: all artwork and gameplay belongs
                // to other workstreams. A running game is not simulated.
                EditorApplication.Exit(
                    report.status=="CITY_ART_CONTEXT_RENDERED_NOT_GAMEPLAY_CERTIFIED"?0:1);
            }
        }
    }
}
#endif
