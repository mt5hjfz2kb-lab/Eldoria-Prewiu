using System;
using System.Collections;
using System.Linq;
using Eldoria.Presentation;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Eldoria.ReferenceClean
{
    /// <summary>
    /// Pure visual replacement of the existing Bastion II Engendro destination.
    /// The canonical WorldHotspot, combat commands, save, rewards and camera remain
    /// owned by ValoriaIntegratedWorld. Does not introduce a new encounter.
    /// Auto-installs only when that exact integrated world is loaded.
    /// </summary>
    public sealed class SpawnlingB2Presentation : MonoBehaviour
    {
        public const string EncounterId = "engendro-valoria";
        public const string AssetKey = "SpawnlingB2/SpawnlingB2";
        public const string VisualRootName = "Zone1 authored engendro-valoria";
        const float WorldHeight = 4.0f;
        const float GroundClearance = 0.055f;
        ValoriaIntegratedWorld host;
        GameObject encounterVisual;
        Transform movingVisual;
        Vector3 restPosition;
        Quaternion restRotation;
        bool previousDefeated;
        float nextStateCheck;
        float idlePhase;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Register()
        {
            var world = Object.FindFirstObjectByType<ValoriaIntegratedWorld>(FindObjectsInactive.Include);
            if (world && !world.GetComponent<SpawnlingB2Presentation>())
                world.gameObject.AddComponent<SpawnlingB2Presentation>();
        }

        IEnumerator Start()
        {
            host = GetComponent<ValoriaIntegratedWorld>();
            if (!host) yield break;
            while (host && !host.Ready) yield return null;
            if (!host) yield break;
            if (!AttachVisual(host, out var why))
            {
                Debug.LogError("ELDORIA_SPAWNLING_ATTACH_FAILED " + why);
                yield break;
            }
            var root = host.WorldRoot.GetComponentsInChildren<Transform>(true)
                .FirstOrDefault(t => t.name == VisualRootName);
            encounterVisual = root ? root.gameObject : null;
            movingVisual = root && root.childCount > 0 ? root.GetChild(0) : null;
            if (movingVisual)
            {
                restPosition = movingVisual.localPosition;
                restRotation = movingVisual.localRotation;
                idlePhase = 1.27f;
            }
            SynchronizeDefeat();
        }

        public static bool AttachVisual(ValoriaIntegratedWorld world, out string failure)
        {
            failure = "";
            if (!world || !world.WorldRoot)
            {
                failure = "Canonical world not loaded"; return false;
            }
            var oldVisual = world.WorldRoot.GetComponentsInChildren<Transform>(true)
                .FirstOrDefault(t => t.name == VisualRootName);
            if (oldVisual)
            {
                // A second installation must not create another monster or collider.
                return oldVisual.GetComponentsInChildren<MeshRenderer>(true).Length > 0;
            }
            var target = world.WorldRoot.GetComponentsInChildren<WorldHotspot>(true)
                .FirstOrDefault(t => t.Id == EncounterId);
            if (!target || !target.GetComponent<BoxCollider>())
            {
                failure = "Original Engendro WorldHotspot/collider not found"; return false;
            }
            var prefab = Resources.Load<GameObject>(AssetKey);
            if (!prefab)
            {
                failure = "Finished near/far Unity prefab missing from Resources"; return false;
            }
            if (prefab.GetComponentsInChildren<Collider>(true).Length != 0 ||
                prefab.GetComponentsInChildren<WorldHotspot>(true).Length != 0)
            {
                failure = "Visual prefab must not contain any collider or new target"; return false;
            }
            var renderers = prefab.GetComponentsInChildren<MeshRenderer>(true);
            var lodGroup = prefab.GetComponent<LODGroup>();
            if (!lodGroup || renderers.Length < 2)
            {
                failure = "Finished genuine near/far LOD prefab is incomplete"; return false;
            }
            var origin = new GameObject(VisualRootName);
            origin.transform.SetParent(world.WorldRoot.transform, true);
            origin.transform.position = target.transform.position;
            origin.transform.rotation = Quaternion.identity;
            var creature = Object.Instantiate(prefab, origin.transform, false);
            creature.name = "Engendro de la Fisura / Tripo PBR real / Bastion II";
            creature.transform.localPosition = Vector3.zero;
            creature.transform.localRotation = Quaternion.identity;

            var rs = creature.GetComponentsInChildren<MeshRenderer>(true);
            if (rs.Length == 0)
            {
                Object.Destroy(origin); failure = "Imported creature has no mesh renderer"; return false;
            }
            Bounds before = rs[0].bounds;
            for (int i = 1; i < rs.Length; i++) before.Encapsulate(rs[i].bounds);
            if (before.size.y < .05f)
            {
                Object.Destroy(origin); failure = "Imported model has invalid world height"; return false;
            }
            // One constant, deliberately readable tactical scale across orientations.
            creature.transform.localScale = Vector3.one * (WorldHeight / before.size.y);
            creature.GetComponent<LODGroup>()?.RecalculateBounds();
            var cityNode = world.WorldRoot.GetComponentsInChildren<WorldHotspot>(true)
                .FirstOrDefault(x => x.Id == "valoria-map-city");
            if (cityNode)
            {
                var delta = cityNode.transform.position - target.transform.position;
                delta.y = 0;
                if (delta.sqrMagnitude > 1f)
                    creature.transform.rotation = Quaternion.Euler(0,
                        -Mathf.Atan2(delta.z, delta.x) * Mathf.Rad2Deg, 0);
            }
            var after = creature.GetComponentsInChildren<MeshRenderer>(true);
            Bounds placed = after[0].bounds;
            for (int i = 1; i < after.Length; i++) placed.Encapsulate(after[i].bounds);
            var contact = target.transform.position.y - target.GetComponent<BoxCollider>().size.y * .5f;
            if (TrySampleTerrain(world, target.transform.position, out float surface))
                contact = surface;
            creature.transform.position += Vector3.up * (contact - placed.min.y + GroundClearance);

            // Never re-enable these old visual stand-ins; their dedicated
            // encounter hotspot is a separate untouched GameObject.
            foreach (var name in new[] { "Enemy camp existing shelter",
                                          "Zone1 hostile camp engendro-valoria" })
            {
                var standIn = world.WorldRoot.GetComponentsInChildren<Transform>(true)
                    .FirstOrDefault(t => t.name == name);
                if (standIn && standIn.GetComponentsInChildren<WorldHotspot>(true).Length == 0 &&
                    standIn.GetComponentsInChildren<Collider>(true).Length == 0)
                    standIn.gameObject.SetActive(false);
            }
            return true;
        }

        static bool TrySampleTerrain(ValoriaIntegratedWorld world, Vector3 at, out float height)
        {
            height = 0;
            var filter = world.WorldRoot.GetComponentsInChildren<MeshFilter>(true)
                .FirstOrDefault(t => t.name == "ZoneOne continuous prairie source");
            if (!filter || !filter.sharedMesh || !filter.sharedMesh.isReadable) return false;
            var mesh = filter.sharedMesh;
            var vertices = mesh.vertices;
            var triangles = mesh.triangles;
            var toWorld = filter.transform.localToWorldMatrix;
            bool found = false;
            float best = float.NegativeInfinity;
            for (int i = 0; i < triangles.Length; i += 3)
            {
                var a = toWorld.MultiplyPoint3x4(vertices[triangles[i]]);
                var b = toWorld.MultiplyPoint3x4(vertices[triangles[i + 1]]);
                var c = toWorld.MultiplyPoint3x4(vertices[triangles[i + 2]]);
                if (at.x < Mathf.Min(a.x, Mathf.Min(b.x, c.x)) ||
                    at.x > Mathf.Max(a.x, Mathf.Max(b.x, c.x)) ||
                    at.z < Mathf.Min(a.z, Mathf.Min(b.z, c.z)) ||
                    at.z > Mathf.Max(a.z, Mathf.Max(b.z, c.z))) continue;
                float det = (b.z - c.z) * (a.x - c.x) + (c.x - b.x) * (a.z - c.z);
                if (Mathf.Abs(det) < .0000001f) continue;
                float u = ((b.z - c.z) * (at.x - c.x) + (c.x - b.x) * (at.z - c.z)) / det;
                float v = ((c.z - a.z) * (at.x - c.x) + (a.x - c.x) * (at.z - c.z)) / det;
                float w = 1f - u - v;
                if (u < -.0001f || v < -.0001f || w < -.0001f) continue;
                float y = u * a.y + v * b.y + w * c.y;
                best = Mathf.Max(best, y);
                found = true;
            }
            if (found) height = best;
            return found;
        }

        void SynchronizeDefeat()
        {
            if (!host || !host.City || !host.City.Gateway || !encounterVisual) return;
            previousDefeated = host.City.Gateway.Snapshot().EngendroDefeated;
            // A defeated Engendro must not continue breathing over a cleared node.
            if (movingVisual) movingVisual.gameObject.SetActive(!previousDefeated);
        }

        void LateUpdate()
        {
            if (!host || !host.Ready || !movingVisual) return;
            if (Time.unscaledTime >= nextStateCheck)
            {
                nextStateCheck = Time.unscaledTime + .4f;
                var defeated = host.City.Gateway.Snapshot().EngendroDefeated;
                if (defeated != previousDefeated) SynchronizeDefeat();
            }
            if (previousDefeated) return;
            float t = Time.unscaledTime * 1.45f + idlePhase;
            // Subtle breathing and weight transfer without inventing rigged combat.
            movingVisual.localPosition = restPosition + Vector3.up * (.045f * Mathf.Sin(t));
            movingVisual.localRotation = restRotation *
                Quaternion.Euler(Mathf.Sin(t * .68f) * 1.7f,
                                 Mathf.Sin(t * .31f) * 2.3f, 0);
        }
    }
}
