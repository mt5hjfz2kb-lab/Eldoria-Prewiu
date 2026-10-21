using System;
using Eldoria.Domain;
using Eldoria.ReferenceClean;
using UnityEngine;

namespace Eldoria.SpawnlingBastion2
{
    /// <summary>
    /// Visual presentation only. Combat, rewards, unlocks and save data are
    /// controlled exclusively by the existing engendro-valoria gateway.
    /// No bespoke game command, collider, health pool or new interactable node.
    /// </summary>
    public sealed class SpawnlingEncounterVisual : MonoBehaviour
    {
        public ValoriaIntegratedWorld Host;
        public Transform MotionPivot;
        public GameObject ModelRoot;
        public Animation SkeletalAnimation;
        public string SwipeClip = "";
        bool wasAttacking;
        [Range(0f, 0.10f)] public float BreatheAmplitude = .028f;
        [Range(0f, 8f)] public float SwayDegrees = 3.5f;

        Vector3 restPosition;
        Quaternion restRotation;
        float nextStateCheck;

        void Awake()
        {
            if (!MotionPivot) MotionPivot = transform;
            restPosition = MotionPivot.localPosition;
            restRotation = MotionPivot.localRotation;
        }

        void OnEnable()
        {
            nextStateCheck = 0f;
        }

        void LateUpdate()
        {
            if (!Host || !Host.Ready || !ModelRoot || !MotionPivot) return;
            // Visual presence follows the ORIGINAL persisted Bastion II enemy flag.
            // A save reload or a win cannot leave a ghost creature on the map.
            if (Time.unscaledTime >= nextStateCheck)
            {
                nextStateCheck = Time.unscaledTime + .30f;
                var city = Host.City;
                if (city != null && city.Gateway != null)
                {
                    ApplySnapshot(city.Gateway.Snapshot(),city.Clock.UtcTicks,Host.InWorld);
                }
            }
            if (!ModelRoot.activeSelf || !Host.InWorld) return;
            float t = Time.unscaledTime;
            MotionPivot.localPosition = restPosition +
                Vector3.up * (BreatheAmplitude * Mathf.Sin(t * 1.55f));
            MotionPivot.localRotation = restRotation *
                Quaternion.Euler(1.1f * Mathf.Sin(t * 1.2f),
                    SwayDegrees * Mathf.Sin(t * .62f), 0f);
        }

        // The original gateway resolves combat directly from outbound to returning.
        // Start the visual windup during the last 1.5 seconds of approach; never
        // add a combat phase, delay victory, or write to the persisted state.
        void ApplySnapshot(PlayerState state,long now,bool inWorld)
        {
            bool shouldAppear=!state.EngendroDefeated;
            double remaining=(state.March.PhaseEndsUtcTicks-now)/(double)TimeSpan.TicksPerSecond;
            bool attacking=shouldAppear&&inWorld&&state.March.TargetId=="engendro-valoria"&&
                state.March.Phase=="outbound"&&remaining>0&&remaining<=1.5;
            if(SkeletalAnimation&&!string.IsNullOrEmpty(SwipeClip)) {
                if(attacking&&!wasAttacking)SkeletalAnimation.Play(SwipeClip,PlayMode.StopAll);
                if(!shouldAppear)ResetSkinPose();
            }
            wasAttacking=attacking;
            if(ModelRoot.activeSelf!=shouldAppear)ModelRoot.SetActive(shouldAppear);
        }

        void ResetSkinPose()
        {
            if(!SkeletalAnimation)return;
            SkeletalAnimation.Stop();
            var clip=SkeletalAnimation.GetClip(SwipeClip);
            if(clip)clip.SampleAnimation(SkeletalAnimation.gameObject,0);
        }

        void OnDisable()
        {
            wasAttacking=false;
            ResetSkinPose();
            if (!MotionPivot) return;
            MotionPivot.localPosition = restPosition;
            MotionPivot.localRotation = restRotation;
        }
    }
}
