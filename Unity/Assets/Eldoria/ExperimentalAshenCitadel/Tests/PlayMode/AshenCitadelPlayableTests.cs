// Live Unity Editor PlayMode smoke. Requires previously saved native scene.
// These tests invoke Unity UI callbacks and verify real scene object state.
// They do not emulate physical iPhone taps or certify WebGL memory.
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
    [TestFixture]
    public sealed partial class AshenCitadelPlayableTests
    {
        const string ScenePath=
            "Assets/Eldoria/ExperimentalAshenCitadel/Scenes/AshenCitadel.unity";
        private string savePath;
        private string backupPath;

        [SetUp]
        public void IsolateSavedData()
        {
            savePath=Path.Combine(UnityEngine.Application.persistentDataPath,
                "eldoria-ashen-experiment-v1.json");
            backupPath=null;
            if(File.Exists(savePath))
            {
                backupPath=savePath+"-playmode-backup-"+Guid.NewGuid().ToString("N");
                File.Move(savePath,backupPath);
            }
        }

        [TearDown]
        public void RestoreSavedData()
        {
            if(File.Exists(savePath))File.Delete(savePath);
            if(!string.IsNullOrEmpty(backupPath)&&File.Exists(backupPath))
                File.Move(backupPath,savePath);
        }

        private static Transform Find(string name)
        {
            return UnityEngine.Object.FindObjectsByType<Transform>(
                FindObjectsInactive.Include,FindObjectsSortMode.None)
                .FirstOrDefault(x=>x.name==name);
        }

        private static Button RequireButton(string name)
        {
            var btn=UnityEngine.Object.FindObjectsByType<Button>(
                FindObjectsInactive.Include,FindObjectsSortMode.None)
                .FirstOrDefault(x=>x.name==name);
            Assert.That(btn,Is.Not.Null,"Expected real Unity Button: "+name);
            return btn;
        }

        private static IEnumerator EnterRealScene()
        {
            var scene=EditorSceneManager.LoadSceneInPlayMode(ScenePath,
                new LoadSceneParameters(LoadSceneMode.Single));
            Assert.That(scene.IsValid(),Is.True);
            yield return null;
            yield return null;
            Assert.That(Find("ASHEN · authoritative game adapter"),Is.Not.Null);
            Assert.That(Find("Ashen · Royal Interface"),Is.Not.Null,
                "Awake must construct the actual UI in PlayMode");
            Assert.That(Find("Ashen · Player camera"),Is.Not.Null);
        }

        [UnityTest]
        public IEnumerator NativeSceneLoadsHUDWithoutDuplicatedProductionUI()
        {
            yield return EnterRealScene();
            Assert.That(Find("Eldoria composition root"),Is.Null,
                "Legacy bootstrap cannot recreate its production HUD here");
            Assert.That(Find("Ashen City · authored hierarchy"),Is.Not.Null);
            Assert.That(Find("Ashen Frontier · authored experiment"),Is.Not.Null);
            var scaler=Find("Ashen · Royal Interface").GetComponent<CanvasScaler>();
            Assert.That(scaler.referenceResolution.x,Is.EqualTo(390));
            Assert.That(scaler.referenceResolution.y,Is.EqualTo(844));
            var tabs=new[]{"Nav · BASTIÓN","Nav · EDIFICIOS","Nav · EJÉRCITO","Nav · MUNDO"};
            foreach(var tab in tabs)RequireButton(tab);
            Assert.That(Find("Stage_1").gameObject.activeInHierarchy,Is.True);
            Assert.That(Find("Stage_2").gameObject.activeInHierarchy,Is.False);
            Assert.That(Find("Ashen_Sawmill").gameObject.activeInHierarchy,Is.False,
                "Unbuilt economic structures cannot be visibly complete");
        }

        [UnityTest]
        public IEnumerator RealNavigationCallbacksSwitchCityAndFrontier()
        {
            yield return EnterRealScene();
            RequireButton("Nav · MUNDO").onClick.Invoke();
            yield return null;
            Assert.That(Find("Ashen Frontier · authored experiment").gameObject.activeInHierarchy,Is.True);
            Assert.That(Find("Ashen City · authored hierarchy").gameObject.activeInHierarchy,Is.False);
            RequireButton("Nav · BASTIÓN").onClick.Invoke();
            yield return null;
            Assert.That(Find("Ashen City · authored hierarchy").gameObject.activeInHierarchy,Is.True);
            Assert.That(Find("Ashen Frontier · authored experiment").gameObject.activeInHierarchy,Is.False);
            Assert.That(Find("Sheet title").GetComponent<Text>().text,Does.Contain("BASTIÓN"));
        }

        [UnityTest]
        public IEnumerator MailBagHeroesChallengeAndDailyOpenTheirCorrectPanels()
        {
            yield return EnterRealScene();
            foreach(var step in new[]{
                ("Access · CORREO","BUZÓN"),
                ("Access · BOLSA","BOLSA"),
                ("Access · HÉROES","HÉROES"),
                ("Access · RETOS","VIGILANTES"),
                ("Daily treasury","TRIBUTO")})
            {
                RequireButton(step.Item1).onClick.Invoke();
                yield return null;
                Assert.That(Find("Sheet title").GetComponent<Text>().text,Does.Contain(step.Item2),
                    "Visible button content must match its real destination: "+step.Item1);
            }
        }
    }
}
#endif
