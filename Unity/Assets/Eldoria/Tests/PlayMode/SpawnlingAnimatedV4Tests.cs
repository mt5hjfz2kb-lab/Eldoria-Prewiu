#if UNITY_EDITOR
using System;
using System.Collections;
using System.Reflection;
using Eldoria.Application;
using Eldoria.Domain;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
namespace Eldoria.Tests
{
    public sealed class SpawnlingAnimatedV4Tests
    {
        sealed class Clock:IClock {public long UtcTicks{get;set;}=DateTime.UtcNow.Ticks;public void Step(int seconds){UtcTicks+=TimeSpan.FromSeconds(seconds).Ticks;}}
        sealed class Store:IStateStore {public PlayerState State;public int Writes;public PlayerState Load()=>State;public void Save(PlayerState state){State=state;Writes++;}}
        GameObject root;AnimationClip clip;string previousProfile;
        [UnityTearDown]public IEnumerator Cleanup(){SliceContentProfiles.SetRuntimeProfileOverride(previousProfile);if(root)UnityEngine.Object.Destroy(root);if(clip)UnityEngine.Object.Destroy(clip);yield return null;}
        [Test]public void ExistingEnemyRemainsSubstantiallyLegibleAcrossAuthenticCameraZoom()
        {
            var actorType=Type.GetType("Eldoria.SpawnlingBastion2.SpawnlingEncounterVisual, Assembly-CSharp");
            Assert.IsNotNull(actorType);
            root=new GameObject("Existing animated Spawnling visual only");
            var actor=root.AddComponent(actorType);
            var presence=actorType.GetMethod("PresenceAtZoom",BindingFlags.Public|BindingFlags.Instance);
            Assert.IsNotNull(presence);
            float close=(float)presence.Invoke(actor,new object[]{14f});
            float far=(float)presence.Invoke(actor,new object[]{70f});
            Assert.That(close,Is.GreaterThanOrEqualTo(1.75f),
                "An insignificant 10–20% close-up change is rejected by the owner");
            Assert.That(far,Is.GreaterThanOrEqualTo(4f),
                "Macro 4X must keep the enemy readable while zooming out");
            Assert.That(far,Is.GreaterThan(close*2f));
            Assert.That((float)presence.Invoke(actor,new object[]{40f}),
                Is.InRange(close,far),"Presence must interpolate without replacing the model or collider");
        }


        [Test]public void NonserializedArtPresenceUpliftChangesRuntimeNearAndFarWithoutChangingSourceOrGameplay()
        {
            var actorType=Type.GetType("Eldoria.SpawnlingBastion2.SpawnlingEncounterVisual, Assembly-CSharp");
            Assert.IsNotNull(actorType);
            root=new GameObject("Eldoria Spawnling exact visual candidate A/B");
            var actor=root.AddComponent(actorType);
            var near=actorType.GetField("NearPresence");
            var far=actorType.GetField("StrategicPresence");
            var multiplier=actorType.GetField("ArtProofPresenceMultiplier");
            var presence=actorType.GetMethod("PresenceAtZoom",BindingFlags.Public|BindingFlags.Instance);
            Assert.IsNotNull(near);Assert.IsNotNull(far);
            Assert.IsNotNull(multiplier);Assert.IsNotNull(presence);
            float baselineNear=(float)near.GetValue(actor), baselineFar=(float)far.GetValue(actor);
            Assert.That(baselineNear,Is.GreaterThan(0f));
            Assert.That(baselineFar,Is.GreaterThan(baselineNear));
            // Simulate old serialized scene: the NearPresence/StrategicPresence
            // fields are untouched. Only the new nonserialized runtime factor moves.
            multiplier.SetValue(actor,1f);
            float oldClose=(float)presence.Invoke(actor,new object[]{14f});
            float oldRegional=(float)presence.Invoke(actor,new object[]{280f});
            multiplier.SetValue(actor,1.50f);
            float candidateClose=(float)presence.Invoke(actor,new object[]{14f});
            float candidateRegional=(float)presence.Invoke(actor,new object[]{280f});
            Assert.That(candidateClose,Is.EqualTo(oldClose*1.50f).Within(.001f));
            Assert.That(candidateRegional,Is.EqualTo(oldRegional*1.50f).Within(.001f));
            Assert.That((float)near.GetValue(actor),Is.EqualTo(baselineNear));
            Assert.That((float)far.GetValue(actor),Is.EqualTo(baselineFar));
            Assert.IsFalse(actor.gameObject.GetComponent<Collider>());
            Assert.IsFalse(actor.gameObject.GetComponent<Rigidbody>());
        }

        [UnityTest]public IEnumerator OriginalGatewayArrivalDrivesVisualWithoutChangingCombatOrSave()
        {
            previousProfile=SliceContentProfiles.ActiveRuntimeProfile;
            SliceContentProfiles.SetRuntimeProfileOverride(SliceContentProfiles.OwnerIiiId);
            var clock=new Clock();
            var store=new Store{State=new PlayerState{BastionLevel=2,BarracksLevel=1,MarchConfigured=true,
                PreparedHeroId="aldric",Available=new ArmyRoster{ArcherT1=100},PreparedTroops=new ArmyRoster{ArcherT1=100},
                ChapterProgress=new ChapterProgressState{TrainedArchers=100,MarchConfirmed=true}}};
            var gateway=new LocalGateway(clock,store);
            root=new GameObject("Isolated skeletal presentation");root.SetActive(false);
            var model=new GameObject("model");model.transform.SetParent(root.transform);
            var joint=new GameObject("joint");joint.transform.SetParent(root.transform);
            var animation=root.AddComponent<Animation>();animation.playAutomatically=false;
            clip=new AnimationClip{legacy=true,name="verified visual sample",wrapMode=WrapMode.Once};
            clip.SetCurve("joint",typeof(Transform),"localPosition.x",AnimationCurve.Linear(0,0,2.7f,1));
            animation.AddClip(clip,clip.name);animation.clip=clip;
            // Presentation lives in Assembly-CSharp; the domain test asmdef must
            // exercise that real component through its serialized boundary.
            var actorType=Type.GetType("Eldoria.SpawnlingBastion2.SpawnlingEncounterVisual, Assembly-CSharp");
            Assert.IsNotNull(actorType,"Canonical encounter presentation type missing");
            var actor=root.AddComponent(actorType);
            actorType.GetField("ModelRoot").SetValue(actor,model);
            actorType.GetField("MotionPivot").SetValue(actor,root.transform);
            actorType.GetField("SkeletalAnimation").SetValue(actor,animation);
            actorType.GetField("SwipeClip").SetValue(actor,clip.name);
            root.SetActive(true);yield return null;
            var apply=actorType.GetMethod("ApplySnapshot",BindingFlags.NonPublic|BindingFlags.Instance);
            void Present(PlayerState state,bool world=true){
                string before=JsonUtility.ToJson(state);int writes=store.Writes;
                apply.Invoke(actor,new object[]{state,clock.UtcTicks,world});
                Assert.AreEqual(before,JsonUtility.ToJson(state),"Presentation must never mutate command state");
                Assert.AreEqual(writes,store.Writes,"Presentation must never save");
            }
            var s=gateway.Snapshot();
            Assert.IsTrue(gateway.Execute(new GameCommand("animation-arrival",s.PlayerId,"Fight","engendro-valoria",s.Revision)).Ok);
            Present(gateway.Snapshot());
            Assert.IsFalse(animation.isPlaying,"No premature combat animation on departure");
            clock.Step(SliceRules.TravelSeconds-1);gateway.Advance();
            s=gateway.Snapshot();Assert.AreEqual("outbound",s.March.Phase);
            Present(s,false);Assert.IsFalse(animation.isPlaying,"City view must not start world animation");
            var resource=s.March.TargetId;s.March.TargetId="forest-valoria";Present(s);
            Assert.IsFalse(animation.isPlaying,"Resource gathering must not animate an enemy");
            s.March.TargetId=resource;Present(s);
            Assert.IsTrue(animation.IsPlaying(clip.name),"Real outbound arrival window starts windup");
            yield return new WaitForSeconds(.15f);
            Assert.Greater(joint.transform.localPosition.x,.01f,"Animation must advance a real transform");
            clock.Step(2);gateway.Advance();s=gateway.Snapshot();
            Assert.AreEqual("returning",s.March.Phase,"Original gateway resolves without an invented gathering/combat phase");
            Assert.IsTrue(s.EngendroDefeated);
            Present(s);Assert.IsFalse(model.activeSelf);Assert.IsFalse(animation.isPlaying);
            Assert.That(joint.transform.localPosition.x,Is.EqualTo(0).Within(.0001));
            Assert.IsTrue(new LocalGateway(clock,store).Snapshot().EngendroDefeated,"Existing persisted victory survives reload");
        }
    }
}
#endif
