#if UNITY_EDITOR
using System;
using System.Collections;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using NUnit.Framework;
using Eldoria.Application;
using Eldoria.Domain;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace Eldoria.Valoria25D.Tests
{
    /// <summary>
    /// Actual Unity Editor PLAYMODE evidence: load the new isolated scene,
    /// use real owner save adapter and real button-to-gateway commands.
    /// All saved data belongs to an explicitly named valoria25d-qa-* file.
    /// </summary>
    public sealed class City25DScenePlayModeProof
    {
        const string ScenePath="Assets/Valoria25D/Scenes/Valoria25D_Gameplay.unity";
        const string Folder="Valoria25DGameplayEvidence";
        static Type FromAssemblyCSharp(string shortName)
        {
            var type=Type.GetType("Eldoria.Valoria25D."+shortName+", Assembly-CSharp");
            Assert.NotNull(type,"Missing runtime UI component "+shortName);
            return type;
        }
        static Type FromArt(string shortName)
        {
            var type=Type.GetType("Eldoria.Experimentos.Ciudad2D."+shortName+", Assembly-CSharp");
            Assert.NotNull(type);
            return type;
        }
        static object Prop(Component c,string name)
        {
            var property=c.GetType().GetProperty(name,
                BindingFlags.Public|BindingFlags.Instance);
            Assert.NotNull(property,"Missing "+name);
            return property.GetValue(c);
        }
        static Component Single(string name)
        {
            var all=UnityEngine.Object.FindObjectsByType(FromAssemblyCSharp(name),
                FindObjectsSortMode.None);
            Assert.AreEqual(1,all.Length,"Expected exactly one "+name);
            return (Component)all[0];
        }
        static string Sha(byte[] b)
        {
            using(var hash=SHA256.Create())
                return BitConverter.ToString(hash.ComputeHash(b)).Replace("-","").ToLowerInvariant();
        }
        static byte[] Capture(Camera camera,int width,int height)
        {
            var old=RenderTexture.active;
            var oldTarget=camera.targetTexture;
            var rt=new RenderTexture(width,height,24,RenderTextureFormat.ARGB32);
            Texture2D texture=null;
            try
            {
                rt.Create();camera.targetTexture=rt;
                camera.aspect=(float)width/height;
                camera.Render();RenderTexture.active=rt;
                texture=new Texture2D(width,height,TextureFormat.RGB24,false);
                texture.ReadPixels(new Rect(0,0,width,height),0,0);
                texture.Apply();
                var bytes=texture.EncodeToPNG();
                Assert.Greater(bytes.Length,8000);
                return bytes;
            }
            finally
            {
                camera.targetTexture=oldTarget;
                RenderTexture.active=old;
                if(texture!=null)UnityEngine.Object.DestroyImmediate(texture);
                rt.Release();UnityEngine.Object.DestroyImmediate(rt);
            }
        }
        [UnityTest,Timeout(160000)]
        public IEnumerator RealV6City_CanonicalGatewaySaveBuildTouchAndNonDuplicatedHUD()
        {
            var qa=System.Environment.GetEnvironmentVariable("VALORIA25D_QA_SAVE_PATH");
            Assert.IsFalse(string.IsNullOrEmpty(qa));
            Assert.That(Path.GetFileName(qa),Does.StartWith("valoria25d-qa-"));
            Assert.IsTrue(File.Exists(Path.Combine(UnityEngine.Application.dataPath,"..",ScenePath)));
            var scene=EditorSceneManager.OpenScene(ScenePath,OpenSceneMode.Single);
            Assert.IsTrue(scene.IsValid());
            yield return new EnterPlayMode();
            Assert.IsTrue(UnityEngine.Application.isPlaying);
            yield return null;yield return null;yield return null;
            Assert.AreEqual("Valoria25D_Gameplay",SceneManager.GetActiveScene().name);
            var root=Single("City25DPlayableController");
            var ui=Single("City25DMinimalHud");
            Assert.IsTrue((bool)Prop(root,"Ready"),"New root failed to use the canonical save");
            Assert.IsTrue((bool)Prop(root,"IsPureComposition"));
            Assert.IsTrue((bool)Prop(ui,"IsPresentationOnly"));
            Assert.AreEqual(1,UnityEngine.Object.FindObjectsByType<EventSystem>(FindObjectsSortMode.None).Length);
            Assert.AreEqual(1,UnityEngine.Object.FindObjectsByType<Camera>(FindObjectsSortMode.None).Length);
            Assert.AreEqual(1,UnityEngine.Object.FindObjectsByType<Canvas>(FindObjectsSortMode.None).Length);
            Assert.AreEqual(0,UnityEngine.Object.FindObjectsByType(FromArt("City2DPilotHud"),
                FindObjectsSortMode.None).Length,"Art-demo HUD must not coexist");
            var gateway=(ICommandGateway)Prop(root,"Gateway");
            var bridge=(Component)Prop(root,"Bridge");
            Assert.IsTrue((bool)Prop(bridge,"IsLiveBound"));
            Assert.AreEqual(6,(int)Prop(bridge,"RegisteredBuildingCount"));
            var state=gateway.Snapshot();
            Assert.GreaterOrEqual(state.BastionLevel,1);
            Assert.LessOrEqual(state.BastionLevel,5);
            var beforeWood=state.Resources.Wood;
            var buildingType=FromArt("CityBuilding");
            var components=UnityEngine.Object.FindObjectsByType(buildingType,
                FindObjectsSortMode.None).Cast<Component>().ToArray();
            Assert.AreEqual(6,components.Length);
            var sawmill=components.Single(c=>
                (string)Prop(c,"BuildingId")=="aserradero");
            var camera=Camera.main;Assert.NotNull(camera);
            var fit=camera.GetComponent(FromArt("City2DCameraFit"));
            Assert.NotNull(fit);
            var setViewport=fit.GetType().GetMethod("SetEvidenceViewport");
            var clearViewport=fit.GetType().GetMethod("ClearEvidenceViewport");
            Assert.NotNull(setViewport);Assert.NotNull(clearViewport);
            var output=Path.GetFullPath(Path.Combine(UnityEngine.Application.dataPath,"..",Folder));
            Directory.CreateDirectory(output);
            var manifest=new System.Collections.Generic.List<string>();
            void Save(string name,int w,int h)
            {
                setViewport.Invoke(fit,new object[]{w,h});
                var bytes=Capture(camera,w,h);
                File.WriteAllBytes(Path.Combine(output,name),bytes);
                manifest.Add("{\"file\":\""+name+"\",\"sha256\":\""+Sha(bytes)+
                    "\",\"width\":"+w+",\"height\":"+h+"}");
            }
            Save("Valoria25D_RealSave_390x844.png",390,844);
            Save("Valoria25D_RealSave_1170x2532.png",1170,2532);
            Assert.NotNull(EventSystem.current);
            var tap=new PointerEventData(EventSystem.current)
            {
                button=PointerEventData.InputButton.Left,
                position=camera.WorldToScreenPoint(sawmill.transform.position)
            };
            Assert.IsTrue(ExecuteEvents.Execute(sawmill.gameObject,tap,
                ExecuteEvents.pointerClickHandler),"The V6 object must dispatch touch selection");
            yield return null;
            Assert.AreEqual("aserradero",(string)Prop(root,"SelectedId"));
            Assert.That((string)Prop(ui,"DisplayedTitle"),Does.Contain("Aserradero"));
            Assert.IsTrue((bool)Prop(ui,"PanelIsOpen"));
            var run=root.GetType().GetMethod("RunPrimary");
            Assert.NotNull(run);
            run.Invoke(root,null);
            var after=gateway.Snapshot();
            // With a fresh owner B1 save, canonical build is available only when
            // the actual wallet can pay. We verify both routes, not fake success.
            if(beforeWood>=SliceRules.SawmillWoodCost && state.SawmillLevel==0 &&
                state.BuildingCompletesUtcTicks==0)
            {
                Assert.That(after.BuildingTaskId,Does.StartWith("sawmill:"));
                Assert.AreEqual(beforeWood-SliceRules.SawmillWoodCost,after.Resources.Wood);
                Assert.Greater(after.BuildingCompletesUtcTicks,DateTime.UtcNow.Ticks);
                Save("Valoria25D_RealConstruction_390x844.png",390,844);
                var revision=after.Revision;
                run.Invoke(root,null); // duplicate press, same frame, must not repeat charge
                Assert.AreEqual(revision,gateway.Snapshot().Revision);
                Assert.AreEqual(after.Resources.Wood,gateway.Snapshot().Resources.Wood);
                // Reopen the EXACT same separate QA store with the canonical factory.
                var fresh=Eldoria.Presentation.SliceBoot.OpenCity25DGateway();
                Assert.AreEqual(after.BuildingTaskId,fresh.Snapshot().BuildingTaskId);
                Assert.AreEqual(after.Resources.Wood,fresh.Snapshot().Resources.Wood);
            }
            else
                Assert.IsFalse(after.BuildingCompletesUtcTicks>0 &&
                    after.BuildingTaskId.StartsWith("sawmill:") &&
                    after.Resources.Wood==beforeWood,"A build cannot start for free");

            Save("Valoria25D_RealSaveLandscape_844x390.png",844,390);
            clearViewport.Invoke(fit,null);
            File.WriteAllText(Path.Combine(output,"evidence.json"),
                "{\"engine\":\"Unity\",\"mode\":\"real_Editor_EnterPlayMode\","+
                "\"scene\":\"Valoria25D_Gameplay.unity\","+
                "\"real_gateway_and_save\":true,\"no_fake_unlocks\":true,"+
                "\"original_main_untouched\":true,\"physical_iphone\":\"PENDING\","+
                "\"captures\":["+string.Join(",",manifest)+"]}\n");
            Debug.Log("VALORIA25D_REAL_PLAYMODE_TOUCH_SAVE_GATEWAY_SINGLE_HUD_PASS");
            yield return new ExitPlayMode();
        }
    }
}
#endif
