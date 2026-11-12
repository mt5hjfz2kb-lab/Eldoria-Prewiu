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
            // Actual on-screen image aperture, measured from Unity transforms.
            public float subjectX,subjectY,subjectW,subjectH;
        }

        [Serializable] sealed class Report
        {
            public string status,source,scene,camera,reason;
            public int cityRenderers,pngCount,realGatewayClaims;
            public int trueBastionPixelProofs,minimumOriginalColorBins;
            public float minimumLitSourceFraction;
            public int bastionSnapshotsVerified,bastionRotationProofs,bastionPerimeterProofs;
            public bool sceneEnvironmentRestored;
            public int actualBuiltStudioProofs;
            public string bastionPresentationMethod;
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

        // The studio deliberately contains dark negative space. Measure the
        // actual eight-corner projected masonry bounds, not a fixed viewport
        // center. Bounds come from the live 3D camera, never a bright-pixel
        // search (which could accidentally certify a decorative gold frame).
        static RectInt BuiltMasonryPixelRect(DailyTreasurePanel panel,int width,int height)
        {
            var field=typeof(DailyTreasurePanel).GetField(
                "capturedBastionSubjectViewport",
                System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.NonPublic);
            Require(field!=null,"Missing actual projected masonry bounds");
            var subject=(Rect)field.GetValue(panel);
            Require(!float.IsNaN(subject.xMin) && !float.IsNaN(subject.yMin) &&
                !float.IsNaN(subject.xMax) && !float.IsNaN(subject.yMax) &&
                subject.xMin>=0f && subject.yMin>=0f &&
                subject.xMax<=1f && subject.yMax<=1f &&
                subject.width>0f && subject.height>0f,
                "Actual projected masonry bounds are invalid");
            int x=Mathf.FloorToInt(subject.xMin*width);
            int y=Mathf.FloorToInt(subject.yMin*height);
            int right=Mathf.Min(width,Mathf.CeilToInt(subject.xMax*width));
            int top=Mathf.Min(height,Mathf.CeilToInt(subject.yMax*height));
            Require(right-x>=32 && top-y>=32,
                "Actual masonry is too small for a native visibility proof");
            return new RectInt(x,y,right-x,top-y);
        }

        static void ProveNativeBastionSilhouette(
            DailyTreasurePanel panel,Report report)
        {
            var raw=panel.GetComponentsInChildren<RawImage>(true)
                .FirstOrDefault(image=>image && image.gameObject.name==
                    "Genuine Eldoria milestone scenery / visual only");
            Require(raw && raw.texture is RenderTexture,
                "No true URP RenderTexture backing the Bastion ceremony");
            var rt=(RenderTexture)raw.texture;
            Require(rt.IsCreated() && rt.width>=64 && rt.height>=64 && Mathf.Max(rt.width,rt.height)>=1000,
                "Bastion hero shot is not an actual GPU render");
            var masonry=BuiltMasonryPixelRect(panel,rt.width,rt.height);
            int rx=masonry.x,ry=masonry.y;
            int w=masonry.width,h=masonry.height;
            var tex=new Texture2D(w,h,TextureFormat.RGB24,false);
            var previous=RenderTexture.active;
            try
            {
                // Editor fixture only. Read actual BUILDING camera pixels
                // rather than pixels of gold ornamental UI elements.
                RenderTexture.active=rt;
                tex.ReadPixels(new Rect(rx,ry,w,h),0,0);
                tex.Apply();
                int sampled=0,lit=0;
                var colors=new HashSet<int>();
                var pixels=tex.GetPixels32();
                for(int y=0;y<h;y+=7)
                    for(int x=0;x<w;x+=7)
                    {
                        var color=pixels[y*w+x];
                        sampled++;
                        if(color.r+color.g+color.b>=48)lit++;
                        colors.Add(((int)color.r>>4)*65536+
                            ((int)color.g>>4)*256+
                            ((int)color.b>>4));
                    }
                var fraction=sampled>0?(float)lit/sampled:0f;
                if(!(sampled>=150 && lit>sampled/6 && colors.Count>=36))
                    PreserveFailedOriginalGpu(rt,rt.width,rt.height);
                Require(sampled>=150 && lit>sampled/6 &&
                        colors.Count>=36,
                    "True Bastion camera contains no meaningful geometry: "+
                    lit+"/"+sampled+" lit, "+colors.Count+" colors");
                report.trueBastionPixelProofs++;
                report.minimumOriginalColorBins=
                    Math.Min(report.minimumOriginalColorBins,colors.Count);
                report.minimumLitSourceFraction=Math.Min(
                    report.minimumLitSourceFraction,fraction);
            }
            finally
            {
                RenderTexture.active=previous;
                Object.DestroyImmediate(tex);
            }
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


        // A RenderTexture name/SceneKey proves only allocation, not actual
        // masonry pixels. The new isolated URP SingleCameraRequest can return
        // a valid yet BLACK texture on a broken render route. Sample the real
        // GPU output in EDITOR ONLY; never stall the player's mobile GPU.
        static void PreserveFailedOriginalGpu(RenderTexture texture,int width,int height)
        {
            var prior=RenderTexture.active;
            var original=new Texture2D(texture.width,texture.height,TextureFormat.RGB24,false);
            try
            {
                RenderTexture.active=texture;
                original.ReadPixels(new Rect(0,0,texture.width,texture.height),0,0);
                original.Apply(false);
                string folder=Path.GetFullPath(Path.Combine(UnityEngine.Application.dataPath,
                    "../"+Folder+"/FailedGpuReadback"));
                Directory.CreateDirectory(folder);
                string file=Path.Combine(folder,"FAILED-actual-bastion-rt-"+width+"x"+height+".png");
                File.WriteAllBytes(file,original.EncodeToPNG());
                Debug.Log("FAILED_ORIGINAL_BASTION_GPU_PRESERVED "+file+
                    "; actual="+texture.width+"x"+texture.height+
                    "; sha256="+Hash(file)+"; NOT_ART_APPROVAL");
            }
            finally{RenderTexture.active=prior;Object.DestroyImmediate(original);}
        }

        static void VerifyLiveBastionTexture(DailyTreasurePanel panel,
            int width,int height)
        {
            var field=typeof(DailyTreasurePanel).GetField(
                "capturedBastionScene",
                System.Reflection.BindingFlags.Instance|
                System.Reflection.BindingFlags.NonPublic);
            var captured=field?.GetValue(panel) as RenderTexture;
            Require(captured && captured.IsCreated() &&
                    captured.width>=64 && captured.height>=64 &&
                    Mathf.Max(captured.width,captured.height)>=1000,
                "Bastion ceremony has no valid authentic URP render "+
                width+"x"+height+"; field="+(field!=null)+
                "; texture="+(captured!=null)+
                "; created="+(captured && captured.IsCreated())+
                "; actual="+(captured?captured.width+"x"+captured.height:"none"));
            var actualPictures=panel.GetComponentsInChildren<RawImage>(true)
                .Where(image=>image && image.gameObject.activeInHierarchy &&
                    image.enabled && image.texture==captured).ToArray();
            Require(actualPictures.Length==1 && actualPictures[0].color.a>=.999f,
                "Bastion camera is duplicated or transparent over a second city");
            // Resolve MSAA before reading; never attempt ReadPixels from a
            // multisampled source and never assume the texture was painted.
            const int samples=192;
            var resolved=RenderTexture.GetTemporary(samples,samples,0,
                RenderTextureFormat.ARGB32);
            var scan=new Texture2D(samples,samples,TextureFormat.RGB24,false);
            var previous=RenderTexture.active;
            try
            {
                var masonry=BuiltMasonryPixelRect(panel,captured.width,captured.height);
                Graphics.Blit(captured,resolved,
                    new Vector2((float)masonry.width/captured.width,(float)masonry.height/captured.height),
                    new Vector2((float)masonry.x/captured.width,(float)masonry.y/captured.height));
                RenderTexture.active=resolved;
                scan.ReadPixels(new Rect(0,0,samples,samples),0,0);
                scan.Apply(false);
                var pixels=scan.GetPixels32();
                var colors=new HashSet<int>();
                int tested=0,lit=0;
                for(int y=samples/8;y<samples*7/8;y+=3)
                    for(int x=samples/8;x<samples*7/8;x+=3)
                    {
                        var pixel=pixels[y*samples+x];
                        int brightness=pixel.r+pixel.g+pixel.b;
                        tested++;
                        if(brightness>45)lit++;
                        colors.Add(((int)pixel.r>>4)*256+
                            ((int)pixel.g>>4)*16+(pixel.b>>4));
                    }
                if(!(tested>700 && lit>tested/6 && colors.Count>=35))
                    PreserveFailedOriginalGpu(captured,width,height);
                Require(tested>700 && lit>tested/6 && colors.Count>=35,
                    "Real Bastion GPU texture is dark/flat/unrendered at "+
                    width+"x"+height+": lit "+lit+"/"+tested+
                    ", color bins "+colors.Count+
                    ". No fabricated premium city photo is acceptable.");
            }
            finally
            {
                RenderTexture.active=previous;
                Object.DestroyImmediate(scan);
                RenderTexture.ReleaseTemporary(resolved);
            }
        }

        // Verify the same OPEN milestone across a physical portrait/landscape
        // transition. The approved camera is a real RenderTexture, not a
        // stretched screenshot: after rotation it must have a NEW aspect,
        // actual nonblank masonry pixels, and preserve the existing receipt.
        // This does not change gateway state, claim gifts or add output PNGs.
        static void VerifyBastionOrientationRecapture(
            DailyTreasurePanel panel,RectTransform root,int width,int height)
        {
            var story=panel.GetComponentsInChildren<RawImage>(true)
                .FirstOrDefault(image=>image && image.gameObject.name==
                    "Genuine Eldoria milestone scenery / visual only");
            Require(story && story.texture is RenderTexture,
                "Native Bastion rotation requires the live 3D source");
            var initial=story.texture;
            RequireBastionApertureAspect(initial,width,height,"initial");
            var original=root.sizeDelta;
            try
            {
                root.sizeDelta=new Vector2(height,width);
                Canvas.ForceUpdateCanvases();
                panel.ProbeOrientationLayoutForNativeEvidence();
                Canvas.ForceUpdateCanvases();
                Require(story.texture is RenderTexture &&
                        !ReferenceEquals(story.texture,initial),
                    "Rotating device reused stale cropped 3D Bastion image");
                var rotated=(RenderTexture)story.texture;
                RequireBastionApertureAspect(rotated,height,width,"rotated");
                VerifyLiveBastionTexture(panel,height,width);
            }
            finally
            {
                root.sizeDelta=original;
                Canvas.ForceUpdateCanvases();
                panel.ProbeOrientationLayoutForNativeEvidence();
                Canvas.ForceUpdateCanvases();
            }
            Require(story.texture is RenderTexture,
                "Restored screen lost actual GPU Bastion snapshot");
            RequireBastionApertureAspect(story.texture,width,height,"restored");
            VerifyLiveBastionTexture(panel,width,height);
        }

        static void RequireBastionApertureAspect(Texture texture,
            int width,int height,string phase)
        {
            float expected=width/(float)height;
            Require(texture && Mathf.Abs((float)texture.width/texture.height-expected)<.01f,
                "Bastion GPU lens mismatches actual reserved picture aperture: "+phase);
        }
        static RawImage BastionPicture(DailyTreasurePanel panel)
        {
            return panel.GetComponentsInChildren<RawImage>(true).FirstOrDefault(
                image=>image && image.gameObject.name==
                    "Genuine Eldoria milestone scenery / visual only");
        }
        static void RecordPictureAperture(Shot shot,RawImage picture,Camera camera,DailyTreasurePanel panel)
        {
            var corners=new Vector3[4];picture.rectTransform.GetWorldCorners(corners);
            var a=RectTransformUtility.WorldToScreenPoint(camera,corners[0]);
            var b=RectTransformUtility.WorldToScreenPoint(camera,corners[2]);
            var field=typeof(DailyTreasurePanel).GetField(
                "capturedBastionSubjectViewport",
                System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.NonPublic);
            Require(field!=null,"True camera projected masonry bounds are missing");
            var projected=(Rect)field.GetValue(panel);
            Require(projected.width>0f&&projected.height>0f,
                "No actual projected built-tier masonry");
            var uv=picture.uvRect;
            float imageX=Mathf.Min(a.x,b.x),imageY=shot.height-Mathf.Max(a.y,b.y);
            float imageW=Mathf.Abs(b.x-a.x),imageH=Mathf.Abs(b.y-a.y);
            Require(Mathf.Abs(imageX)<1.5f && Mathf.Abs(imageY)<1.5f &&
                Mathf.Abs(imageW-shot.width)<1.5f && Mathf.Abs(imageH-shot.height)<1.5f,
                "Actual single Bastion picture does not cover the full captured viewport");
            shot.subjectX=imageX+(projected.xMin-uv.x)/uv.width*imageW;
            shot.subjectY=imageY+(1f-(projected.yMax-uv.y)/uv.height)*imageH;
            shot.subjectW=projected.width/uv.width*imageW;
            shot.subjectH=projected.height/uv.height*imageH;
            Require(shot.subjectW>100f && shot.subjectH>100f,
                "Actual Bastion aperture has no measurable pixel area");
        }
        static Shot PhotographSameBastionWithoutForeground(
            DailyTreasurePanel panel,Camera camera,RenderTexture target,
            string folder,int width,int height)
        {
            var picture=BastionPicture(panel);
            Require(picture && picture.texture is RenderTexture,
                "Cannot invent an aligned baseline without actual GPU picture");
            // Keep the SAME live texture, aperture, UVs, tint and camera.
            // Only hide foreground Graphics for the real A/B photograph.
            // No pixel translation/warping or generated reference is permitted.
            var enabled=panel.GetComponentsInChildren<Graphic>(true)
                .Where(g=>g && g.enabled && g.gameObject.activeInHierarchy && g!=picture).ToArray();
            try
            {
                foreach(var graphic in enabled)graphic.enabled=false;
                var shot=Photograph(camera,target,folder,width,height,
                    "real-bastion-camera-baseline",false);
                RecordPictureAperture(shot,picture,camera,panel);
                return shot;
            }
            finally {foreach(var graphic in enabled)if(graphic)graphic.enabled=true;}
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


        static string EnvironmentSignature()
        {
            var invariant=System.Globalization.CultureInfo.InvariantCulture;
            var values=new List<string>{
                RenderSettings.fog.ToString(),RenderSettings.fogMode.ToString(),
                JsonUtility.ToJson(RenderSettings.fogColor),
                RenderSettings.fogDensity.ToString("R",invariant),
                RenderSettings.fogStartDistance.ToString("R",invariant),
                RenderSettings.fogEndDistance.ToString("R",invariant),
                RenderSettings.ambientMode.ToString(),JsonUtility.ToJson(RenderSettings.ambientLight),
                RenderSettings.ambientIntensity.ToString("R",invariant)};
            var probe=RenderSettings.ambientProbe;
            for(int channel=0;channel<3;channel++)
                for(int coefficient=0;coefficient<9;coefficient++)
                    values.Add(probe[channel,coefficient].ToString("R",invariant));
            return string.Join("|",values);
        }
        static void VerifySingleBastionPerimeter(DailyTreasurePanel panel,
            int width,int height,Report report)
        {
            bool portrait=height>width;
            var rim=panel.GetComponentsInChildren<Image>(true).Single(
                image=>image.name=="Bastion / one original bronze perimeter");
            var ornament=panel.GetComponentsInChildren<Image>(true).Single(
                image=>image.name=="Authored narrative single-frame ornament / no hitbox");
            if(portrait)
                Require(rim.gameObject.activeInHierarchy && rim.sprite &&
                    rim.type==Image.Type.Sliced && !rim.fillCenter &&
                    !rim.raycastTarget && !ornament.gameObject.activeInHierarchy,
                    "Tall Bastion scene lacks its one original border-only frame");
            else
                Require(!rim.gameObject.activeInHierarchy &&
                    ornament.gameObject.activeInHierarchy && ornament.sprite &&
                    ornament.type==Image.Type.Sliced && !ornament.raycastTarget,
                    "Wide Bastion scene lacks its one original narrative frame");
            report.bastionPerimeterProofs++;
        }

        static void VerifyStudioRestoration(DailyTreasurePanel panel,string before,
            int width,int height)
        {
            var after=EnvironmentSignature();
            bool restored=DailyTreasureBastionCamera.LastSceneStateRestored;
            int count=DailyTreasureBastionCamera.LastStudioRendererCount;
            if(before!=after || !restored || count<1)
            {
                var names=new List<string>{"fog","fogMode","fogColor","fogDensity",
                    "fogStart","fogEnd","ambientMode","ambientLight","ambientIntensity"};
                for(int c=0;c<3;c++)for(int coefficient=0;coefficient<9;coefficient++)
                    names.Add("ambientProbe["+c+","+coefficient+"]");
                var a=before.Split('|');var b=after.Split('|');
                var differences=new List<string>();
                for(int i=0;i<Math.Max(a.Length,b.Length);i++)
                    if(i>=a.Length||i>=b.Length||a[i]!=b[i])
                        differences.Add((i<names.Count?names[i]:"field"+i)+
                            ": "+(i<a.Length?a[i]:"MISSING")+" -> "+
                            (i<b.Length?b[i]:"MISSING"));
                var folder=Path.GetFullPath(Path.Combine(UnityEngine.Application.dataPath,"../"+Folder));
                Directory.CreateDirectory(folder);
                File.WriteAllText(Path.Combine(folder,"scene-environment-diff-"+width+"x"+height+".txt"),
                    "layersAndLightsRestored="+restored+"; rendererCount="+count+"\n"+
                    string.Join("\n",differences));
                var picture=BastionPicture(panel);
                if(picture&&picture.texture is RenderTexture texture)
                    PreserveFailedOriginalGpu(texture,width,height);
                Debug.LogError("STUDIO_RESTORE_DIFFERENCES "+string.Join("; ",differences)+
                    "; layersAndLights="+restored+"; rendererCount="+count);
            }
            Require(before==after && restored && count>0,
                "Studio capture failed exact scoped environment restoration; see original GPU and field differences");
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
                minimumOriginalColorBins=int.MaxValue,
                minimumLitSourceFraction=1f,
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
            string environmentBefore=null;
            try
            {
                var scene=EditorSceneManager.OpenScene(Scene,OpenSceneMode.Single);
                Require(scene.IsValid() && scene.path==Scene,
                    "The exact canonical Valoria city scene was not opened");
                report.citySceneOpened=true;
                environmentBefore=EnvironmentSignature();
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
                        // URP may initialize the scene's ambient probe on its
                        // first real render. This is the actual source-city
                        // baseline from which the studio must restore exactly.
                        if(pictures.Count==1)environmentBefore=EnvironmentSignature();
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
                        var beforeStudio=EnvironmentSignature();
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
                        VerifyLiveBastionTexture(panel,width,height);
                        VerifySingleBastionPerimeter(panel,width,height,report);
                        VerifyStudioRestoration(panel,beforeStudio,width,height);
                        report.actualBuiltStudioProofs++;
                        report.bastionPresentationMethod=DailyTreasureBastionCamera.PresentationMethod;
                        report.bastionSnapshotsVerified++;
                        Canvas.ForceUpdateCanvases();
                        ProveNativeBastionSilhouette(panel,report);
                        VerifyBastionOrientationRecapture(
                            panel,root,width,height);
                        report.bastionRotationProofs++;
                        var bastionStudy=Photograph(camera,target,folder,
                            width,height,"authored-bastion-visual-only",false);
                        RecordPictureAperture(bastionStudy,BastionPicture(panel),camera,panel);
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
                        panel.Suppressed=false;
                        panel.Open();
                        Require(panel.IsOpen,
                            "Focused real Bastion visual-only stage did not open");
                        beforeStudio=EnvironmentSignature();
                        panel.Present("BASTIÓN · ESTUDIO VISUAL",
                            "PREVISUALIZACIÓN SIN HITO NI RECOMPENSA",
                            true,DailyTreasureCeremonyTier.Special,
                            "milestone:bastion:visual-only");
                        VerifyLiveBastionTexture(panel,width,height);
                        VerifySingleBastionPerimeter(panel,width,height,report);
                        VerifyStudioRestoration(panel,beforeStudio,width,height);
                        report.actualBuiltStudioProofs++;
                        report.bastionPresentationMethod=DailyTreasureBastionCamera.PresentationMethod;
                        report.bastionSnapshotsVerified++;
                        Canvas.ForceUpdateCanvases();
                        ProveNativeBastionSilhouette(panel,report);
                        var heroBaseline=PhotographSameBastionWithoutForeground(
                            panel,camera,target,folder,width,height);
                        pictures.Add(heroBaseline);
                        var heroStage=Photograph(camera,target,folder,
                            width,height,"real-bastion-camera-ceremony",false);
                        RecordPictureAperture(heroStage,BastionPicture(panel),camera,panel);
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
                Require(report.trueBastionPixelProofs==8 &&
                        report.minimumOriginalColorBins>=36 &&
                        report.minimumLitSourceFraction>1f/6f,
                    "At least one of eight genuine GPU Bastion renderings "+
                    "is blank or an unvarying clear color");
                Require(pictures.Count==20,
                    "Expected 20 genuine city, Bastion context AND canonical camera-framed PNGs");
                Require(report.bastionSnapshotsVerified==8,
                    "All four orientations must have TWO real nonblank URP Bastion snapshots");
                Require(report.bastionRotationProofs==4,
                    "The four genuine city screen sizes must survive changing "+
                    "device orientation without reusing a cropped 3D snapshot");
                Require(report.bastionPerimeterProofs==8 && EnvironmentSignature()==environmentBefore,
                    "Actual 8-frame cinematic source environment or single perimeter proof incomplete");
                report.sceneEnvironmentRestored=true;
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

