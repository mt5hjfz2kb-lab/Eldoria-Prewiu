// Genuine Unity 6 PlayMode UI look-dev snapshots.
// The Canvas is temporarily captured in its Camera-space mode so a headless
// RenderTexture includes real Unity uGUI; gameplay scene keeps Overlay at runtime.
// NEVER report these captures as physical-iPhone/GameView screenshots.
#if UNITY_EDITOR
using System;
using System.Collections;
using System.IO;
using System.Linq;
using NUnit.Framework;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace Eldoria.ExperimentalAshenCitadel.Tests
{
    public sealed partial class AshenCitadelPlayableTests
    {
        // ScenePath lives in the other half of this partial test fixture.
        private static string EvidencePath=>Path.GetFullPath(
            Path.Combine(UnityEngine.Application.dataPath,"..","ArtEvidence"));

        private static Color32[] ActualCameraPixels(Camera camera,Canvas canvas,
            int width,int height,bool includeUi)
        {
            var previous=RenderTexture.active;
            var original=camera.targetTexture;
            var wasEnabled=canvas.enabled;
            var prevAspect=camera.aspect;
            var rt=new RenderTexture(width,height,24,RenderTextureFormat.ARGB32);
            var pixels=new Texture2D(width,height,TextureFormat.RGBA32,false);
            try
            {
                rt.Create();
                Assert.That(rt.IsCreated(),Is.True,"Headless graphics RenderTexture unavailable");
                canvas.enabled=includeUi;
                camera.targetTexture=rt;
                camera.aspect=(float)width/height;
                Canvas.ForceUpdateCanvases();
                camera.Render();
                RenderTexture.active=rt;
                pixels.ReadPixels(new Rect(0,0,width,height),0,0);
                pixels.Apply(false);
                return pixels.GetPixels32();
            }
            finally
            {
                camera.targetTexture=original;
                camera.aspect=prevAspect;
                RenderTexture.active=previous;
                canvas.enabled=wasEnabled;
                UnityEngine.Object.DestroyImmediate(rt);
                UnityEngine.Object.DestroyImmediate(pixels);
            }
        }
        private static void SaveActualUi(Camera cam,Canvas canvas,string page,int width,int height)
        {
            // PlayMode tests live in their own asmdef; runtime MonoBehaviours
            // compile in Assembly-CSharp. Use Editor-only reflection rather
            // than an invalid direct compile-time dependency across assemblies.
            var host=Find("ASHEN · authoritative game adapter");
            var adapter=host.GetComponents<MonoBehaviour>()
                .FirstOrDefault(x=>x!=null && x.GetType().Name=="AshenCitadelExperience");
            Assert.That(adapter,Is.Not.Null,"Runtime art adapter was not instantiated");
            var reflow=adapter.GetType().GetMethod("ReflowForViewport");
            Assert.That(reflow,Is.Not.Null,"Runtime viewport rule must be shared with captures");
            reflow.Invoke(adapter,new object[]{width,height});
            Canvas.ForceUpdateCanvases();
            var without=ActualCameraPixels(cam,canvas,width,height,false);
            var with=ActualCameraPixels(cam,canvas,width,height,true);
            Assert.That(with.Length,Is.EqualTo(without.Length));
            int changed=0,visible=0;
            for(int i=0;i<with.Length;i++)
            {
                var a=with[i];var b=without[i];
                int difference=Math.Abs(a.r-b.r)+Math.Abs(a.g-b.g)+Math.Abs(a.b-b.b);
                if(difference>24)changed++;
                if(a.r+a.g+a.b>50)visible++;
            }
            double changedPct=changed/(double)with.Length;
            Assert.That(changedPct,Is.GreaterThan(.008),
                "Capture failed to composite real uGUI; never accept a camera-only image: "+page);
            Assert.That(visible,Is.GreaterThan(with.Length*.35),
                "Missing scene or blank screenshot: "+page);
            var outImage=new Texture2D(width,height,TextureFormat.RGBA32,false);
            try
            {
                outImage.SetPixels32(with);
                outImage.Apply(false);
                Directory.CreateDirectory(EvidencePath);
                string dest=Path.Combine(EvidencePath,"Ashen_UI_"+page+"_"+width+"x"+height+".png");
                File.WriteAllBytes(dest,outImage.EncodeToPNG());
                Assert.That(new FileInfo(dest).Length,Is.GreaterThan(10000));
                Debug.Log("ASHEN_REAL_UNITY_UI "+Path.GetFileName(dest)+
                    " changed_vs_camera_only="+changedPct.ToString("F4",System.Globalization.CultureInfo.InvariantCulture));
            }
            finally{UnityEngine.Object.DestroyImmediate(outImage);}
        }

        [UnityTest]
        public IEnumerator HeadlessNativeCompositesActualHudAndFourPanelStates()
        {
            yield return EnterRealScene();
            var camera=Find("Ashen · Player camera").GetComponent<Camera>();
            var canvas=Find("Ashen · Royal Interface").GetComponent<Canvas>();
            Assert.That(canvas.renderMode,Is.EqualTo(RenderMode.ScreenSpaceOverlay),
                "Actual playable scene must retain its original overlay UI");
            // In headless tests, Camera.Render does not capture an Overlay
            // Canvas. Switch only the test instance to Camera mode.
            canvas.renderMode=RenderMode.ScreenSpaceCamera;
            canvas.worldCamera=camera;
            canvas.planeDistance=2;
            Canvas.ForceUpdateCanvases();

            // Real layout dimensions, not screenshot-only pixel heuristics.
            var safe=Find("Device safe area").GetComponent<RectTransform>();
            var sheet=Find("Action sheet").GetComponent<RectTransform>();
            var nickname=Find("Player name field").GetComponent<RectTransform>();
            Assert.That(sheet.rect.height,Is.LessThan(safe.rect.height*.67f),
                "Welcome must not hide the entire city");
            Assert.That(nickname.rect.height,Is.GreaterThanOrEqualTo(40f),
                "Nickname must be a real visible input field, never a thin line");
            Assert.That(Find("Daily treasury").gameObject.activeInHierarchy,Is.False,
                "Daily action must not overlap an open modal");

            var frames=new[]{new Vector2Int(390,844),new Vector2Int(1600,900)};
            foreach(var frame in frames)
                SaveActualUi(camera,canvas,"Welcome",frame.x,frame.y);
            RequireButton("Close").onClick.Invoke();
            yield return null;
            Assert.That(Find("Daily treasury").gameObject.activeInHierarchy,Is.True,
                "Daily access must return when a panel closes");

            foreach(var v in new[]{
                ("City","Nav · BASTIÓN"),
                ("Mailbox","Access · CORREO"),
                ("Bag","Access · BOLSA"),
                ("World","Nav · MUNDO")})
            {
                RequireButton(v.Item2).onClick.Invoke();
                yield return null;
                Canvas.ForceUpdateCanvases();
                Assert.That(sheet.rect.height,Is.LessThanOrEqualTo(safe.rect.height*.68f),
                    "Action panel overflows mobile-safe game view: "+v.Item1);
                foreach(var frame in frames)
                    SaveActualUi(camera,canvas,v.Item1,frame.x,frame.y);
            }
            Debug.Log("ASHEN_REAL_UNITY_UI_COMPLETE 10 composited UI PNG, no real-iPhone claim");
            canvas.renderMode=RenderMode.ScreenSpaceOverlay;
        }
    }
}
#endif
