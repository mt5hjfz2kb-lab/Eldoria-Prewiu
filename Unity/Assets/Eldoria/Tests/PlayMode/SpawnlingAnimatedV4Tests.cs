#if UNITY_EDITOR
using System;
using System.Collections;
using Eldoria.World4XCanon;
using Eldoria.SpawnlingBastion2;
using Eldoria.ReferenceClean;
using UnityEngine.SceneManagement;
using System.Linq;
using System.IO;
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


        static object ProofMember(object item,string name)
        {
            var binding=BindingFlags.Instance|BindingFlags.Public|BindingFlags.NonPublic;
            var prop=item.GetType().GetProperty(name,binding);
            if(prop!=null)return prop.GetValue(item);
            var field=item.GetType().GetField(name,binding);
            Assert.IsNotNull(field,"Missing integrated player field "+name);
            return field.GetValue(item);
        }
        static object ProofCall(object item,string name,params object[] args)
        {
            var methods=item.GetType().GetMethods(BindingFlags.Instance|BindingFlags.Public|BindingFlags.NonPublic);
            var method=methods.FirstOrDefault(m=>m.Name==name&&m.GetParameters().Length==args.Length);
            Assert.IsNotNull(method,"Missing integrated player method "+name);
            return method.Invoke(item,args);
        }
        static void ProofPng(Camera camera,string path,int width,int height)
        {
            var target=RenderTexture.GetTemporary(width,height,24,RenderTextureFormat.ARGB32);
            var texture=new Texture2D(width,height,TextureFormat.RGB24,false);
            var oldTarget=camera.targetTexture;var oldActive=RenderTexture.active;
            try {
                camera.targetTexture=target;
                camera.Render();
                RenderTexture.active=target;
                texture.ReadPixels(new Rect(0,0,width,height),0,0);
                texture.Apply();
                File.WriteAllBytes(path,texture.EncodeToPNG());
            } finally {
                camera.targetTexture=oldTarget;
                RenderTexture.active=oldActive;
                RenderTexture.ReleaseTemporary(target);
                UnityEngine.Object.DestroyImmediate(texture);
            }
        }
        [UnityTest]public IEnumerator OriginalIntegratedWorldShows1_5xSpawnlingA_BFromSameCameraWithoutGameplayChange()
        {
            SceneManager.LoadScene("Valoria_City_World_UI");
            ValoriaIntegratedWorld host=null;SpawnlingEncounterVisual actor=null;
            for(int frame=0;frame<300;frame++)
            {
                yield return null;
                host=UnityEngine.Object.FindFirstObjectByType<ValoriaIntegratedWorld>();
                if(host==null||!host.Ready||!host.WorldRoot)continue;
                if(!host.WorldRoot.transform.Find("Region 1 canonical continuous geography"))continue;
                actor=host.WorldRoot.GetComponentsInChildren<SpawnlingEncounterVisual>(true).FirstOrDefault();
                if(actor!=null&&actor.ModelRoot!=null&&actor.MotionPivot!=null)break;
            }
            Assert.IsNotNull(host,"Canonical integrated game did not boot");
            Assert.IsNotNull(actor,"Original authenticated Spawnling actor not present");
            bool opened=host.TrySetWorld(true);
            if(!opened)
            {
                var offer=ProofMember(ProofCall(host,"Read","sawmill",false),"Building");
                var action=ProofMember(offer,"Action");
                ProofCall(host,"Submit",action);
                var city=ProofMember(host,"City");
                ProofCall(ProofMember(city,"Clock"),"Step",SliceRules.SawmillBuildSeconds+1);
                ProofCall(ProofMember(city,"Gateway"),"Advance");
                yield return null;
                opened=host.TrySetWorld(true);
            }
            Assert.IsTrue(opened,"Original world unlock must work before the art proof");
            var hotspot=host.FindTarget("engendro-valoria");
            Assert.IsNotNull(hotspot,"Original enemy hotspot absent");
            var gateway=(ICommandGateway)ProofMember(ProofMember(host,"City"),"Gateway");
            var initial=JsonUtility.ToJson(gateway.Snapshot());
            Assert.IsFalse(gateway.Snapshot().EngendroDefeated,"A/B requires an undefeated original encounter");
            var navigator=host.WorldNavigation;
            var camera=host.WorldCamera;
            var landscape=UnityEngine.Object.FindFirstObjectByType<RegionOneCanonicalLandscape>();
            Assert.IsNotNull(landscape);
            var oldTarget=navigator.target;float oldZoom=camera.orthographicSize;float oldAspect=camera.aspect;
            float originalMultiplier=actor.ArtProofPresenceMultiplier;
            float oldBreathe=actor.BreatheAmplitude,oldSway=actor.SwayDegrees;
            string output=Path.GetFullPath(Path.Combine(Application.dataPath,"..","..",
                "docs","evidence","valoria-reference-clean"));
            Directory.CreateDirectory(output);
            actor.BreatheAmplitude=0f;actor.SwayDegrees=0f;
            try
            {
                foreach(var view in new[]{(level:2,label:"close",aspect:16f/9f,width:1280,height:720),
                    (level:1,label:"regional",aspect:16f/9f,width:1280,height:720),
                    (level:2,label:"close-portrait",aspect:390f/844f,width:390,height:844)})
                {
                    camera.aspect=view.aspect;
                    navigator.SetZoomLevel(view.level,true);
                    navigator.PanTo(hotspot.transform.position);
                    landscape.RefreshForCapture();
                    actor.ArtProofPresenceMultiplier=1f;
                    yield return null;
                    Assert.IsTrue(actor.ModelRoot.activeInHierarchy,"Original actor must render");
                    Assert.IsTrue(actor.TryGeometryBounds(out var before),"Actual imported skinned vertices required");
                    var prefix="integration-issue95-spawnling-"+view.label+"-";
                    ProofPng(camera,Path.Combine(output,prefix+"before.png"),view.width,view.height);
                    actor.ArtProofPresenceMultiplier=1.50f;
                    yield return null;
                    Assert.IsTrue(actor.TryGeometryBounds(out var after),"Actual source mesh lost");
                    ProofPng(camera,Path.Combine(output,prefix+"after.png"),view.width,view.height);
                    Assert.That(after.size.y,Is.GreaterThanOrEqualTo(before.size.y*1.35f),
                        "Native source height must increase visibly, not just metadata");
                    Assert.That(after.size.x,Is.GreaterThanOrEqualTo(before.size.x*1.30f),
                        "Original source width must expand as well; reject height-only distortions");
                    Assert.That(Mathf.Abs(after.min.y-before.min.y),Is.LessThan(.50f),
                        "Upscaling must not lift the original enemy off the terrain");
                    Debug.Log("ELDORIA_ISSUE95_SPAWNLING_NATIVE_AB view="+view.label+
                        " before_height="+before.size.y.ToString("F3")+
                        " after_height="+after.size.y.ToString("F3")+
                        " before_width="+before.size.x.ToString("F3")+
                        " after_width="+after.size.x.ToString("F3")+
                        " camera_zoom="+camera.orthographicSize.ToString("F1"));
                }
                Assert.That(JsonUtility.ToJson(gateway.Snapshot()),Is.EqualTo(initial),
                    "Visual only scale must preserve the authoritative player state");
                Assert.IsNotNull(hotspot.GetComponent<Collider>(),"Original encounter selection retained");
            }
            finally
            {
                actor.ArtProofPresenceMultiplier=originalMultiplier;
                actor.BreatheAmplitude=oldBreathe;actor.SwayDegrees=oldSway;
                camera.aspect=oldAspect;
                navigator.RestoreView(oldTarget,oldZoom);
            }
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
