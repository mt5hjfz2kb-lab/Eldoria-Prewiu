#if UNITY_EDITOR
using System;
using System.Collections;
using System.IO;
using System.Linq;
using System.Reflection;
using Eldoria.Application;
using Eldoria.Domain;
using Eldoria.Infrastructure;
using NUnit.Framework;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace Eldoria.Valoria25D.PlayModeProof
{
    /// <summary>
    /// Canonical gameplay acceptance using real command handlers and a disposable
    /// JSON-backed test state store. NEVER touches canonical local/iPhone saves.
    /// The scene proof separately checks the live 2.5D presentation and inputs.
    /// </summary>
    public sealed class City25DLiveGatewayPlayModeProof
    {
        const string PlayableScene="Assets/Valoria25D/Scenes/Valoria25D_Gameplay.unity";

        sealed class Clock : IClock
        {
            public long UtcTicks { get; private set; } =
                new DateTime(2026,11,17,9,0,0,DateTimeKind.Utc).Ticks;
            public void AdvanceSeconds(int seconds) { UtcTicks += TimeSpan.FromSeconds(seconds).Ticks; }
        }
        sealed class DurableTestStore : IStateStore
        {
            private string json;
            public DurableTestStore(PlayerState initial) { Save(initial); }
            public PlayerState Load() =>
                string.IsNullOrEmpty(json) ? null : JsonUtility.FromJson<PlayerState>(json);
            public void Save(PlayerState state)
            {
                json=JsonUtility.ToJson(state,true);
                if(string.IsNullOrEmpty(json)||json.Length<20)
                    throw new InvalidDataException("Test fixture JSON save did not serialize");
            }
            public string SavedJson=>json;
        }
        static PlayerState Seed(int bastion=1,int sawmill=0,int barracks=0)
        {
            var state=new PlayerState {
                BastionLevel=bastion,SawmillLevel=sawmill,BarracksLevel=barracks,
                Resources=new ResourceWallet {Wood=10000,Stone=10000,Food=10000},
                JourneyComplete=true
            };
            return state;
        }
        static CommandResult Act(LocalGateway gateway,string id,string kind,string target)
        {
            var s=gateway.Snapshot();
            return gateway.Execute(new GameCommand(id,s.PlayerId,kind,target,s.Revision));
        }
        static void AssertWalletSame(ResourceWallet a,ResourceWallet b)
        {
            Assert.That(a.Wood,Is.EqualTo(b.Wood));
            Assert.That(a.Stone,Is.EqualTo(b.Stone));
            Assert.That(a.Food,Is.EqualTo(b.Food));
        }
        [Test] public void Sawmill_RealWalletChargeTimedFinishPersistReload()
        {
            var clock=new Clock();
            var store=new DurableTestStore(Seed());
            var game=new LocalGateway(clock,store);
            var prior=game.Snapshot();
            Assert.That(prior.SawmillLevel,Is.Zero);
            Assert.That(Act(game,"sawmill-qa-1","Build","sawmill").Ok,Is.True);
            var underway=game.Snapshot();
            Assert.That(underway.Resources.Wood,
                Is.EqualTo(prior.Resources.Wood-SliceRules.SawmillWoodCost),
                "The actual LocalGateway must be the only resource writer");
            Assert.That(underway.SawmillLevel,Is.Zero,
                "The 2.5D city must not draw a finished sawmill before completion");
            Assert.That(underway.BuildingTaskId,Does.StartWith("sawmill:"));
            Assert.That(underway.BuildingCompletesUtcTicks,Is.GreaterThan(clock.UtcTicks));
            game=new LocalGateway(clock,store);
            Assert.That(game.Snapshot().BuildingCompletesUtcTicks,
                Is.EqualTo(underway.BuildingCompletesUtcTicks),
                "Actual deadline must persist across scene/app reload");
            Assert.That(Act(game,"sawmill-qa-2","Build","sawmill").Ok,Is.False,
                "No second build while the first task is pending");
            clock.AdvanceSeconds(SliceRules.SawmillBuildSeconds+1);
            Assert.That(game.Advance(),Is.True);
            var finished=game.Snapshot();
            Assert.That(finished.SawmillLevel,Is.EqualTo(1));
            Assert.That(finished.BuildingCompletesUtcTicks,Is.Zero);
            Assert.That(finished.BuildingTaskId,Is.Empty);
            game=new LocalGateway(clock,store);
            Assert.That(game.Snapshot().SawmillLevel,Is.EqualTo(1));
            Assert.That(game.Snapshot().Resources.Wood,
                Is.EqualTo(prior.Resources.Wood-SliceRules.SawmillWoodCost));
            Assert.That(game.Snapshot().CompletedTaskIds.Count,
                Is.EqualTo(finished.CompletedTaskIds.Count),
                "Cannot finish the same construction twice on reload");
        }

        [Test] public void Bastion_CanonicalCostPrerequisiteDeadlineAndOfflineProgress()
        {
            var clock=new Clock();var seed=Seed();
            seed.JourneyComplete=false;
            var store=new DurableTestStore(seed);
            var game=new LocalGateway(clock,store);
            var denied=game.Snapshot().Resources;
            Assert.That(Act(game,"b2-denied","AdvanceBastion","bastion").Ok,Is.False);
            AssertWalletSame(denied,game.Snapshot().Resources);
            // QA fixture supplies a legitimately completed prerequisite; gateway
            // still executes the real upgrade and timer, not a visual level change.
            seed=Seed();
            store=new DurableTestStore(seed);game=new LocalGateway(clock,store);
            var def=BastionProgressionCatalog.ForLevel(2);
            var before=game.Snapshot();
            Assert.That(Act(game,"b2-qa-real","AdvanceBastion","bastion").Ok,Is.True);
            var running=game.Snapshot();
            Assert.That(running.BastionLevel,Is.EqualTo(1));
            Assert.That(running.Resources.Wood,Is.EqualTo(before.Resources.Wood-def.WoodCost));
            Assert.That(running.Resources.Stone,Is.EqualTo(before.Resources.Stone-def.StoneCost));
            Assert.That(running.BastionUpgradeCompletesUtcTicks,
                Is.EqualTo(clock.UtcTicks+TimeSpan.FromSeconds(def.DurationSeconds).Ticks));
            game=new LocalGateway(clock,store);
            Assert.That(game.Snapshot().BastionLevel,Is.EqualTo(1));
            clock.AdvanceSeconds(def.DurationSeconds+1);game.Advance();
            var after=game.Snapshot();
            Assert.That(after.BastionLevel,Is.EqualTo(2));
            Assert.That(after.BastionUpgradeCompletesUtcTicks,Is.Zero);
            game=new LocalGateway(clock,store);
            Assert.That(game.Snapshot().BastionLevel,Is.EqualTo(2));
        }

        [Test] public void Barracks_RecruitCanBeStartedOnlyOnceAndFinishesOffline()
        {
            var clock=new Clock();var store=new DurableTestStore(Seed(2,1,1));
            var game=new LocalGateway(clock,store);
            var before=game.Snapshot();
            int batch=SliceRules.RecruitArchers;
            int wood=SliceRules.RecruitWoodCostFor(batch);
            int stone=SliceRules.RecruitStoneCostFor(batch);
            Assert.That(Act(game,"recruit-real-1","Recruit","archer:t1").Ok,Is.True);
            var pending=game.Snapshot();
            Assert.That(pending.Resources.Wood,Is.EqualTo(before.Resources.Wood-wood));
            Assert.That(pending.Resources.Stone,Is.EqualTo(before.Resources.Stone-stone));
            Assert.That(pending.PendingRecruitArchers,Is.EqualTo(batch));
            Assert.That(pending.Available.ArcherT1,Is.EqualTo(before.Available.ArcherT1));
            Assert.That(pending.RecruitmentCompletesUtcTicks,Is.GreaterThan(clock.UtcTicks));
            Assert.That(Act(game,"recruit-real-2","Recruit","archer:t1").Ok,Is.False);
            game=new LocalGateway(clock,store);
            clock.AdvanceSeconds(SliceRules.RecruitSecondsFor(batch)+1);
            game.Advance();
            var after=game.Snapshot();
            Assert.That(after.Available.ArcherT1,
                Is.EqualTo(before.Available.ArcherT1+batch));
            Assert.That(after.PendingRecruitArchers,Is.Zero);
            Assert.That(after.RecruitmentCompletesUtcTicks,Is.Zero);
            game=new LocalGateway(clock,store);
            Assert.That(game.Snapshot().Available.ArcherT1,Is.EqualTo(after.Available.ArcherT1));
        }

        [Test] public void ChapterThree_RealGranaryStoneworksAndSawmillUpgradeFinish()
        {
            var clock=new Clock();
            var store=new DurableTestStore(Seed(5,1,2));
            var game=new LocalGateway(clock,store);
            var initial=game.Snapshot();
            Assert.That(Act(game,"c3-granary-build","Build","granary").Ok,Is.True);
            Assert.That(game.Snapshot().BuildingTaskId,Does.StartWith("c3:granary:"));
            Assert.That(game.Snapshot().ChapterThree.GranaryLevel,Is.Zero);
            clock.AdvanceSeconds(12);game.Advance();
            Assert.That(game.Snapshot().ChapterThree.GranaryLevel,Is.EqualTo(1));
            game=new LocalGateway(clock,store);
            Assert.That(Act(game,"c5-stoneworks-build","Build","stoneworks").Ok,Is.True);
            Assert.That(game.Snapshot().ChapterThree.StoneworksLevel,Is.Zero);
            clock.AdvanceSeconds(14);game.Advance();
            Assert.That(game.Snapshot().ChapterThree.StoneworksLevel,Is.EqualTo(1));
            game=new LocalGateway(clock,store);
            Assert.That(Act(game,"c3-sawmill-upgrade","UpgradeBuilding","sawmill").Ok,Is.True);
            Assert.That(game.Snapshot().BuildingTaskId,Does.StartWith("c3:sawmill:"));
            clock.AdvanceSeconds(13);game.Advance();
            Assert.That(game.Snapshot().SawmillLevel,Is.EqualTo(2));
            game=new LocalGateway(clock,store);
            Assert.That(game.Snapshot().SawmillLevel,Is.EqualTo(2));
            Assert.That(game.Snapshot().ChapterThree.GranaryLevel,Is.EqualTo(1));
            Assert.That(game.Snapshot().ChapterThree.StoneworksLevel,Is.EqualTo(1));
            // A real level-III+ producer accrues after the upgrade; no 2.5D UI
            // may invent a resource tick outside the authoritative gateway.
            var oldWallet=game.Snapshot().Resources;
            clock.AdvanceSeconds(180);game.Advance();
            var credited=game.Snapshot().Resources;
            Assert.That(credited.Wood,Is.GreaterThan(oldWallet.Wood));
            Assert.That(credited.Stone,Is.GreaterThan(oldWallet.Stone));
            Assert.That(credited.Food,Is.GreaterThan(oldWallet.Food));
        }

        [Test] public void LockedOrUnfundedCommandsAreRejectedWithoutWalletChanges()
        {
            var clock=new Clock();var initial=Seed();
            initial.Resources.Wood=0;initial.Resources.Stone=0;
            var store=new DurableTestStore(initial);var game=new LocalGateway(clock,store);
            var before=game.Snapshot();
            Assert.That(Act(game,"no-wood-sawmill","Build","sawmill").Ok,Is.False);
            var after=game.Snapshot();
            AssertWalletSame(before.Resources,after.Resources);
            Assert.That(after.SawmillLevel,Is.Zero);
            Assert.That(after.BuildingCompletesUtcTicks,Is.Zero);
            Assert.That(Act(game,"locked-barracks","Build","barracks").Ok,Is.False);
            AssertWalletSame(after.Resources,game.Snapshot().Resources);
            Assert.That(Act(game,"locked-granary","Build","granary").Ok,Is.False);
            Assert.That(Act(game,"locked-stoneworks","Build","stoneworks").Ok,Is.False);
            Assert.That(game.Snapshot().ChapterThree.StoneworksLevel,Is.Zero);
        }

        [Test] public void SameCommandIdNeverChargesWalletTwiceAfterReload()
        {
            var clock=new Clock();var store=new DurableTestStore(Seed());
            var game=new LocalGateway(clock,store);var original=game.Snapshot();
            var command=new GameCommand("unique-qa-transaction",original.PlayerId,
                "Build","sawmill",original.Revision);
            Assert.That(game.Execute(command).Ok,Is.True);
            var after=game.Snapshot();
            game=new LocalGateway(clock,store);
            Assert.That(game.Execute(command).Ok,Is.True,
                "Canonical gateway returns an idempotent already-executed response");
            AssertWalletSame(after.Resources,game.Snapshot().Resources);
            Assert.That(game.Snapshot().BuildingCompletesUtcTicks,
                Is.EqualTo(after.BuildingCompletesUtcTicks));
        }

        static Type DynamicType(string fullName)
        {
            var type=Type.GetType(fullName+", Assembly-CSharp");
            Assert.NotNull(type,"Integrated runtime class must be compiled: "+fullName);
            return type;
        }
        static object Prop(Component target,string name)
        {
            var prop=target.GetType().GetProperty(name,BindingFlags.Public|BindingFlags.Instance);
            Assert.NotNull(prop,name);
            return prop.GetValue(target);
        }
        static Component DynamicComponent(string fullName)
        {
            var type=DynamicType(fullName);
            var all=UnityEngine.Object.FindObjectsByType(type,FindObjectsSortMode.None);
            Assert.That(all.Length,Is.EqualTo(1),"Exactly one composition/bridge: "+fullName);
            return (Component)all[0];
        }

        [UnityTest, Timeout(120000)]
        public IEnumerator ActualPlayablePremiumV6_SingleCameraEventHud_RealSnapshotTouch()
        {
            // Protect the real Unity Editor owner save. This absolute QA filename
            // is recognized only by the opt-in SliceBoot test isolation guard.
            var previous=Environment.GetEnvironmentVariable("VALORIA25D_QA_SAVE_PATH");
            string qaPath=Path.Combine(Path.GetTempPath(),
                "valoria25d-qa-live-"+Guid.NewGuid().ToString("N")+".json");
            Environment.SetEnvironmentVariable("VALORIA25D_QA_SAVE_PATH",qaPath);
            try
            {
                Assert.That(File.Exists(Path.Combine(UnityEngine.Application.dataPath,
                    "../"+PlayableScene)),Is.True,
                    "The scene constructor must save a concrete .unity file");
                // Load BEFORE entering PlayMode. Otherwise SliceBoot would
                // bootstrap a legacy presenter on the test runner's default
                // scene, and its DontDestroyOnLoad HUD would be carried here.
                // A real player enters this scene directly, as the build does.
                var edited=EditorSceneManager.OpenScene(PlayableScene,
                    UnityEditor.SceneManagement.OpenSceneMode.Single);
                Assert.IsTrue(edited.IsValid()&&edited.isLoaded);
                yield return new EnterPlayMode();
                yield return null;
                yield return null;
                Assert.That(SceneManager.GetActiveScene().name,Is.EqualTo("Valoria25D_Gameplay"));
                Assert.That(UnityEngine.Object.FindObjectsByType<Camera>(
                    FindObjectsSortMode.None).Length,Is.EqualTo(1));
                Assert.That(UnityEngine.Object.FindObjectsByType<EventSystem>(
                    FindObjectsSortMode.None).Length,Is.EqualTo(1));
                Assert.That(UnityEngine.Object.FindObjectsByType<Canvas>(
                    FindObjectsSortMode.None).Length,Is.EqualTo(1),
                    "Legacy art/demo Canvas must not duplicate gameplay HUD");
                Assert.That(UnityEngine.Object.FindObjectsByType(
                    DynamicType("Eldoria.Experimentos.Ciudad2D.City2DPilotHud"),
                    FindObjectsSortMode.None).Length,Is.Zero);
                var root=DynamicComponent("Eldoria.Valoria25D.City25DPlayableController");
                Assert.That((bool)Prop(root,"Ready"),Is.True);
                var bridge=DynamicComponent("Eldoria.Experimentos.Ciudad2D.City2DStateBridge");
                Assert.That((bool)Prop(bridge,"IsLiveBound"),Is.True,
                    "Demo-stage visual must be forbidden when real player is bound");
                Assert.That((int)Prop(bridge,"RegisteredBuildingCount"),Is.EqualTo(6));
                Assert.That(UnityEngine.Object.FindObjectsByType(
                    DynamicType("Eldoria.Experimentos.Ciudad2D.City2DPremiumShadowGate"),
                    FindObjectsSortMode.None).Length,Is.EqualTo(4),
                    "All Premium V6 independent contact shadows must remain");
                var snapshot=(PlayerState)Prop(root,"Gateway").GetType().GetMethod("Snapshot").Invoke(
                    Prop(root,"Gateway"),null);
                Assert.That(snapshot.BastionLevel,Is.EqualTo(1));
                Assert.That(snapshot.SawmillLevel,Is.Zero);
                Assert.That(snapshot.BuildingTaskId,Is.Empty);
                var components=UnityEngine.Object.FindObjectsByType(
                    DynamicType("Eldoria.Experimentos.Ciudad2D.CityBuilding"),
                    FindObjectsSortMode.None);
                Component bastion=null,sawmill=null;
                foreach(var item in components)
                {
                    var id=Prop((Component)item,"BuildingId")?.ToString();
                    if(id=="bastion")bastion=(Component)item;
                    if(id=="aserradero")sawmill=(Component)item;
                }
                Assert.NotNull(bastion);Assert.NotNull(sawmill);
                Assert.That((int)Prop(bastion,"Level"),Is.EqualTo(snapshot.BastionLevel));
                Assert.That((int)Prop(sawmill,"Level"),Is.EqualTo(snapshot.SawmillLevel));
                var ev=new PointerEventData(EventSystem.current) {
                    button=PointerEventData.InputButton.Left,
                    position=Camera.main.WorldToScreenPoint(bastion.transform.position)
                };
                Assert.That(ExecuteEvents.Execute(bastion.gameObject,ev,
                    ExecuteEvents.pointerClickHandler),Is.True);
                yield return null;
                Assert.That(Prop(root,"SelectedId"),Is.EqualTo("bastion"));
                var hud=DynamicComponent("Eldoria.Valoria25D.City25DMinimalHud");
                Assert.That((bool)Prop(hud,"PanelIsOpen"),Is.True);
                Assert.That(Prop(hud,"DisplayedTitle").ToString(),Does.Contain("Bastión"));
                var output=Environment.GetEnvironmentVariable("VALORIA25D_EVIDENCE_DIR");
                if(!string.IsNullOrEmpty(output))
                {
                    Directory.CreateDirectory(output);
                    var camera=Camera.main;
                    var fit=camera.GetComponent(
                        DynamicType("Eldoria.Experimentos.Ciudad2D.City2DCameraFit"));
                    Assert.NotNull(fit,"Authoritative V6 camera and pinch/pan must remain");
                    fit.GetType().GetMethod("SetEvidenceViewport").Invoke(
                        fit,new object[]{390,844});
                    var render=new RenderTexture(390,844,24,RenderTextureFormat.ARGB32);
                    var previousTarget=camera.targetTexture;
                    var previousActive=RenderTexture.active;
                    Texture2D tex=null;
                    try
                    {
                        render.Create();camera.targetTexture=render;camera.Render();
                        RenderTexture.active=render;
                        tex=new Texture2D(390,844,TextureFormat.RGB24,false);
                        tex.ReadPixels(new Rect(0,0,390,844),0,0);
                        tex.Apply();
                        var bytes=tex.EncodeToPNG();
                        Assert.That(bytes.Length,Is.GreaterThan(8000));
                        File.WriteAllBytes(Path.Combine(output,
                            "Valoria25D_Bastion1_CANONICAL_SAVE_Unity_RenderTexture_390x844.png"),bytes);
                    }
                    finally
                    {
                        camera.targetTexture=previousTarget;
                        RenderTexture.active=previousActive;
                        if(tex!=null)UnityEngine.Object.Destroy(tex);
                        render.Release();UnityEngine.Object.Destroy(render);
                    }
                }
                Assert.That(qaPath,Does.Contain("valoria25d-qa-"));
                yield return new ExitPlayMode();
            }
            finally
            {
                Environment.SetEnvironmentVariable("VALORIA25D_QA_SAVE_PATH",previous);
                if(File.Exists(qaPath))File.Delete(qaPath);
            }
        }
    }
}
#endif
