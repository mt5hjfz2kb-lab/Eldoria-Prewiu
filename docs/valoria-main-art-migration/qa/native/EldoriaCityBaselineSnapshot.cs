#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Eldoria.ReferenceClean;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

// Editor-only observation from an existing play/capture flow. Never performs a selection.
public static class EldoriaCityBaselineSnapshot
{
    [Serializable] sealed class Pose
    {
        public string path;
        public Vector3 position;
        public Quaternion rotation;
        public Matrix4x4 localToWorld;
        public int layer;
        public bool active;
        public Pose(Transform t)
        {
            var parts = new Stack<string>();
            for (var node = t; node; node = node.parent)
                parts.Push(node.name + "[" + node.GetSiblingIndex() + "]");
            path = string.Join("/", parts);
            position = t.position;
            rotation = t.rotation;
            localToWorld = t.localToWorldMatrix;
            layer = t.gameObject.layer;
            active = t.gameObject.activeInHierarchy;
        }
    }

    [Serializable] sealed class Visual
    {
        public Pose pose;
        public string type;
        public bool enabled, forceRenderingOff;
        public Vector3 localCenter, localExtents, worldCenter, worldExtents;
        public Matrix4x4 rendererMatrix;
        public Vector3[] worldCorners;
        public string mesh, meshGuid;
        public string[] materials;
        public int firstFocusPoint = -1;
        public Visual(Renderer renderer)
        {
            pose = new Pose(renderer.transform);
            type = renderer.GetType().Name;
            enabled = renderer.enabled;
            forceRenderingOff = renderer.forceRenderingOff;
            localCenter = renderer.localBounds.center;
            localExtents = renderer.localBounds.extents;
            worldCenter = renderer.bounds.center;
            worldExtents = renderer.bounds.extents;
            rendererMatrix = renderer.localToWorldMatrix;
            worldCorners = Corners(renderer.localBounds, rendererMatrix);
            var filter = renderer.GetComponent<MeshFilter>();
            var shared = renderer is SkinnedMeshRenderer skinned ? skinned.sharedMesh : filter ? filter.sharedMesh : null;
            mesh = shared ? shared.name : "";
            meshGuid = shared ? AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(shared)) : "";
            materials = renderer.sharedMaterials.Select(m => m ? m.name : "").ToArray();
        }
    }

    [Serializable] sealed class Collision
    {
        public Pose pose;
        public string type, meshGuid;
        public bool enabled, trigger;
        public Bounds worldBounds;
        public Vector3 boxCenter, boxSize;
        public Collision(Collider c)
        {
            pose = new Pose(c.transform);
            type = c.GetType().Name;
            enabled = c.enabled;
            trigger = c.isTrigger;
            worldBounds = c.bounds;
            if (c is BoxCollider box) { boxCenter = box.center; boxSize = box.size; }
            if (c is MeshCollider physical && physical.sharedMesh)
                meshGuid = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(physical.sharedMesh));
        }
    }

    [Serializable] sealed class Target
    {
        public string id, focusSource;
        public int level;
        public Pose target;
        public bool framed;
        public Rect viewportProjection, freeViewport;
        public Vector3[] actualFocusPoints = Array.Empty<Vector3>();
        public List<Visual> renderers = new List<Visual>();
        public List<Collision> colliders = new List<Collision>();
    }

    [Serializable] sealed class Snapshot
    {
        public string sample, scene, sourceCommit, unity, selected, status;
        public int frame, bastion, editorWidth, editorHeight, targetWidth, targetHeight;
        public bool world, ready;
        public Pose camera;
        public Matrix4x4 view, projection;
        public Rect pixelRect;
        public float orthographicSize, fov;
        public bool orthographic;
        public List<Target> buildings = new List<Target>();
    }

    static Vector3[] Corners(Bounds bounds, Matrix4x4 matrix)
    {
        var result = new Vector3[8];
        var min = bounds.min;
        var max = bounds.max;
        for (int i = 0; i < result.Length; i++)
            result[i] = matrix.MultiplyPoint3x4(new Vector3((i & 1) == 0 ? min.x : max.x,
                (i & 2) == 0 ? min.y : max.y, (i & 4) == 0 ? min.z : max.z));
        return result;
    }

    // No copied projection or selection code. Compare sources to FocusPoints' actual result
    // in its original order; preserve unclassified results as such if the implementation drifts.
    static bool Match(Vector3[] worldPoints, int offset, Vector3[] candidate)
    {
        if (offset + candidate.Length > worldPoints.Length) return false;
        for (int i = 0; i < candidate.Length; i++)
            if (!worldPoints[offset + i].Equals(candidate[i])) return false;
        return true;
    }

    static Target Inspect(ValoriaIntegratedWorld host, MethodInfo focus, string id, int level,
        params GameObject[] baseRoots)
    {
        var item = new Target { id = id, level = level };
        var hotspot = host.FindTarget(id);
        var advanced = host.GetComponent<BastionIVVWorldPresentation>();
        var model = advanced ? advanced.ModelFor(id) : null;
        var roots = new List<GameObject>();
        if (model) roots.Add(model);
        roots.AddRange(baseRoots.Where(b => b));
        if (hotspot) roots.Add(hotspot.gameObject);
        var seen = new HashSet<Renderer>();
        var seenColliders = new HashSet<Collider>();
        var lookup = new Dictionary<Renderer, Visual>();
        foreach (var root in roots.Distinct())
        {
            foreach (var renderer in root.GetComponentsInChildren<Renderer>(true))
                if (seen.Add(renderer))
                {
                    var visual = new Visual(renderer);
                    lookup.Add(renderer, visual);
                    item.renderers.Add(visual);
                }
            foreach (var collider in root.GetComponentsInChildren<Collider>(true))
                if (seenColliders.Add(collider)) item.colliders.Add(new Collision(collider));
        }
        if (!hotspot) { item.focusSource = "no target in scene"; return item; }
        item.target = new Pose(hotspot.transform);
        var target = hotspot.gameObject;
        item.actualFocusPoints = ((List<Vector3>)focus.Invoke(host, new object[] { target })).ToArray();
        item.framed = host.IsTargetFramed(target, out item.viewportProjection, out item.freeViewport);

        // Each group mirrors one evaluation branch in the original city's FocusPoints.
        var groups = new List<GameObject[]>();
        if (model) groups.Add(new[] { model });
        if (baseRoots.Length != 0) groups.Add(baseRoots);
        groups.Add(new[] { target });
        foreach (var group in groups)
        {
            var candidates = group.Where(g => g && g.activeInHierarchy)
                .SelectMany(g => g.GetComponentsInChildren<Renderer>())
                .Where(r => (r is MeshRenderer || r is SkinnedMeshRenderer) && r.enabled && !r.forceRenderingOff)
                .ToArray();
            if (candidates.Length == 0) continue;
            if (candidates.Length * 8 == item.actualFocusPoints.Length &&
                candidates.Select((r, i) => Match(item.actualFocusPoints, i * 8, lookup[r].worldCorners)).All(x => x))
            {
                item.focusSource = "verified renderer corners";
                for (int i = 0; i < candidates.Length; i++) lookup[candidates[i]].firstFocusPoint = i * 8;
                return item;
            }
            item.focusSource = "source differs from baseline renderer rule";
            return item;
        }
        var hit = target.GetComponent<Collider>();
        if (hit is BoxCollider box && Match(item.actualFocusPoints, 0,
            Corners(new Bounds(box.center, box.size), box.transform.localToWorldMatrix)) && item.actualFocusPoints.Length == 8)
            item.focusSource = "verified box collider";
        else if (hit && item.actualFocusPoints.Length == 8 && Match(item.actualFocusPoints, 0, Corners(hit.bounds, Matrix4x4.identity)))
            item.focusSource = "verified collider world bounds";
        else if (!hit && item.actualFocusPoints.Length == 1 && item.actualFocusPoints[0].Equals(target.transform.position))
            item.focusSource = "verified target transform";
        else item.focusSource = "unclassified";
        return item;
    }

    public static void Capture(string directory, string sample)
    {
        if (string.IsNullOrEmpty(sample) || sample == "." || sample == ".." ||
            sample.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0 || sample.Contains("/") || sample.Contains("\\"))
            throw new ArgumentException("Invalid sample file stem", nameof(sample));
        var snapshot = new Snapshot { sample = sample, scene = SceneManager.GetActiveScene().path,
            sourceCommit = Environment.GetEnvironmentVariable("ELDORIA_QA_SOURCE_SHA"),
            unity = Application.unityVersion, frame = Time.frameCount, editorWidth = Screen.width, editorHeight = Screen.height };
        var host = UnityEngine.Object.FindFirstObjectByType<ValoriaIntegratedWorld>();
        snapshot.ready = host && host.Ready && host.City && host.City.Gateway != null;
        snapshot.status = snapshot.ready ? "observed" : "host not ready";
        if (snapshot.ready)
        {
            snapshot.world = host.InWorld;
            snapshot.selected = host.Selected;
            var cam = host.PlayerCamera;
            if (cam)
            {
                snapshot.camera = new Pose(cam.transform);
                snapshot.view = cam.worldToCameraMatrix;
                snapshot.projection = cam.projectionMatrix;
                snapshot.orthographic = cam.orthographic;
                snapshot.orthographicSize = cam.orthographicSize;
                snapshot.fov = cam.fieldOfView;
                snapshot.pixelRect = cam.pixelRect;
                snapshot.targetWidth = cam.targetTexture ? cam.targetTexture.width : cam.pixelWidth;
                snapshot.targetHeight = cam.targetTexture ? cam.targetTexture.height : cam.pixelHeight;
            }
            var focus = typeof(ValoriaIntegratedWorld).GetMethod("FocusPoints", BindingFlags.Instance | BindingFlags.NonPublic,
                null, new[] { typeof(GameObject) }, null);
            if (focus == null) throw new MissingMethodException("ValoriaIntegratedWorld.FocusPoints(GameObject)");
            var s = host.City.Gateway.Snapshot();
            snapshot.bastion = s.BastionLevel;
            var p = host.City.Parcels;
            snapshot.buildings.Add(Inspect(host, focus, "bastion", s.BastionLevel, host.City.BastionOne, host.City.BastionTwo));
            snapshot.buildings.Add(Inspect(host, focus, "sawmill", s.SawmillLevel,
                p ? p.LeftBuilt : null, p ? p.LeftConstruction : null, p ? p.LeftAvailable : null));
            snapshot.buildings.Add(Inspect(host, focus, "barracks", s.BarracksLevel,
                p ? p.RightBuilt : null, p ? p.RightConstruction : null, p ? p.RightAvailable : null));
            snapshot.buildings.Add(Inspect(host, focus, "granary", s.ChapterThree.GranaryLevel));
            snapshot.buildings.Add(Inspect(host, focus, "stoneworks", s.ChapterThree.StoneworksLevel));
        }
        Directory.CreateDirectory(directory);
        string filename = Path.Combine(directory, sample + ".city-focus.json");
        using (var stream = new FileStream(filename, FileMode.CreateNew, FileAccess.Write, FileShare.Read))
        using (var writer = new StreamWriter(stream))
            writer.WriteLine(JsonUtility.ToJson(snapshot, true));
    }
}
#endif
