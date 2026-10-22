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

        // Only the skinned/LOD presentation scales. The original hotspot,
        // colliders, combat, rewards and persistent state are never modified.
        [Range(1f, 3f)] public float NearPresence = 1.9f;
        [Range(2f, 6f)] public float StrategicPresence = 4.5f;
        [Min(1f)] public float NearOrthographicZoom = 14f;
        [Min(2f)] public float FarOrthographicZoom = 70f;

        Vector3 restPosition, restScale;
        Quaternion restRotation;
        Vector3 restAnchorOffset, groundFootParent;
        bool restAnchorReady;
        float nextStateCheck;

        void Awake()
        {
            if (!MotionPivot) MotionPivot = transform;
            restPosition = MotionPivot.localPosition;
            restRotation = MotionPivot.localRotation;
            restScale = MotionPivot.localScale;
            CacheRestAnchor();
        }

        // Unity's SkinnedMeshRenderer.bounds can include a very large
        // precomputed animation envelope. It is NOT the creature's feet:
        // relying on that envelope displaced the enlarged model into the sky.
        // Sample the true skinned mesh vertices ONCE, using the same bind-pose
        // matrices as the original approved V4 Blender/Unity integration.
        bool TryGeometryFoot(out Vector3 foot)
        {
            foot = Vector3.zero;
            if (!ModelRoot) return false;
            bool found = false;
            Bounds joined = default;
            foreach (var renderer in ModelRoot.GetComponentsInChildren<SkinnedMeshRenderer>(true))
            {
                var mesh = renderer ? renderer.sharedMesh : null;
                if (!mesh || !mesh.isReadable) continue;
                var vertices = mesh.vertices;
                var weights = mesh.boneWeights;
                var bindposes = mesh.bindposes;
                var bones = renderer.bones;
                if (vertices.Length == 0 || weights.Length != vertices.Length ||
                    bones.Length == 0 || bindposes.Length != bones.Length) continue;
                var matrices = new Matrix4x4[bones.Length];
                bool valid = true;
                for (int j = 0; j < bones.Length; j++)
                {
                    if (!bones[j]) { valid = false; break; }
                    matrices[j] = bones[j].localToWorldMatrix * bindposes[j];
                }
                if (!valid) continue;
                for (int i = 0; i < vertices.Length; i++)
                {
                    var w = weights[i];
                    var v = vertices[i];
                    Vector3 point =
                        matrices[w.boneIndex0].MultiplyPoint3x4(v) * w.weight0 +
                        matrices[w.boneIndex1].MultiplyPoint3x4(v) * w.weight1 +
                        matrices[w.boneIndex2].MultiplyPoint3x4(v) * w.weight2 +
                        matrices[w.boneIndex3].MultiplyPoint3x4(v) * w.weight3;
                    if (!found) { joined = new Bounds(point, Vector3.zero); found = true; }
                    else joined.Encapsulate(point);
                }
            }
            if (!found) return false;
            foot = new Vector3(joined.center.x, joined.min.y, joined.center.z);
            return true;
        }

        // Compute actual existing prairie Y at this ONE encounter coordinate.
        // Read geometry without adding MeshColliders, new raycast targets,
        // gameplay terrain or saving anything. No runtime recurring scan.
        bool TryGroundHeight(Vector3 point, out float height)
        {
            height = 0f;
            if (!Host || !Host.WorldRoot) return false;
            foreach (var filter in Host.WorldRoot.GetComponentsInChildren<MeshFilter>(true))
            {
                if (!filter || filter.name != "ZoneOne continuous prairie source" ||
                    !filter.sharedMesh || !filter.sharedMesh.isReadable) continue;
                var vertices = filter.sharedMesh.vertices;
                var triangles = filter.sharedMesh.triangles;
                var matrix = filter.transform.localToWorldMatrix;
                bool found = false;
                for (int i = 0; i + 2 < triangles.Length; i += 3)
                {
                    Vector3 a = matrix.MultiplyPoint3x4(vertices[triangles[i]]);
                    Vector3 b = matrix.MultiplyPoint3x4(vertices[triangles[i + 1]]);
                    Vector3 d = matrix.MultiplyPoint3x4(vertices[triangles[i + 2]]);
                    float det = (b.z - d.z) * (a.x - d.x) +
                                (d.x - b.x) * (a.z - d.z);
                    if (Mathf.Abs(det) < .00001f) continue;
                    float u = ((b.z - d.z) * (point.x - d.x) +
                               (d.x - b.x) * (point.z - d.z)) / det;
                    float v = ((d.z - a.z) * (point.x - d.x) +
                               (a.x - d.x) * (point.z - d.z)) / det;
                    if (u < -.0001f || v < -.0001f || u + v > 1.0001f) continue;
                    float y = u * a.y + v * b.y + (1f - u - v) * d.y;
                    if (!found || y > height) height = y;
                    found = true;
                }
                if (found) return true;
            }
            return false;
        }

        void CacheRestAnchor()
        {
            if (restAnchorReady || !MotionPivot || !ModelRoot || !Host) return;
            if (!TryGeometryFoot(out var sourceFoot)) return;
            var hotspot = Host.FindTarget("engendro-valoria");
            if (!hotspot) return;
            Vector3 destination = hotspot.transform.position;
            if (TryGroundHeight(destination, out var groundY)) destination.y = groundY;
            var parent = MotionPivot.parent;
            Vector3 originalFoot = parent ? parent.InverseTransformPoint(sourceFoot) : sourceFoot;
            restAnchorOffset = originalFoot - restPosition;
            groundFootParent = parent ? parent.InverseTransformPoint(destination) : destination;
            restAnchorReady = true;
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
            // A small enemy disappears when the whole authored region is shown.
            // Increase its visual-only footprint with camera distance. It is
            // still a single original encounter, not a second strategic icon.
            if (!restAnchorReady) CacheRestAnchor();
            if (!restAnchorReady) return;
            float zoom = Host.WorldCamera ? Host.WorldCamera.orthographicSize : NearOrthographicZoom;
            float presence = PresenceAtZoom(zoom);
            MotionPivot.localScale = restScale * presence;
            float t = Time.unscaledTime;
            // With the actual skinned foot in parent coordinates, compensate
            // both the pre-existing source offset and every zoom-dependent
            // enlargement. The feet stay directly on the original hotspot.
            MotionPivot.localPosition = groundFootParent -
                restAnchorOffset * presence +
                Vector3.up * (BreatheAmplitude * Mathf.Sin(t * 1.55f));
            MotionPivot.localRotation = restRotation *
                Quaternion.Euler(1.1f * Mathf.Sin(t * 1.2f),
                    SwayDegrees * Mathf.Sin(t * .62f), 0f);
        }

        // Deterministic for editor/unit QA, even before the scene can be opened.
        public float PresenceAtZoom(float zoom)
        {
            float denominator = Mathf.Max(1f, FarOrthographicZoom - NearOrthographicZoom);
            float fraction = Mathf.Clamp01((zoom - NearOrthographicZoom) / denominator);
            float smooth = fraction * fraction * (3f - 2f * fraction);
            return Mathf.Lerp(NearPresence, StrategicPresence, smooth);
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
            MotionPivot.localScale = restScale;
        }
    }
}
