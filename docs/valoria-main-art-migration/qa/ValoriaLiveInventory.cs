using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.Profiling;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace Eldoria.MigrationQA
{
    // Diagnostic file for the Editor folder of a disposable checkout only.
    public static class ValoriaLiveInventory
    {
        [Serializable] class Item
        {
            public string path, component;
            public int componentOrdinal, hideFlags;
            public bool activeInHierarchy, enabled;
        }

        [Serializable] class Widget : Item
        {
            public bool interactable, effectiveInteractable;
            public string navigation, targetGraphic;
            public Vector2[] screenCorners;
            public Vector2 anchorMin, anchorMax, pivot, sizeDelta, anchoredPosition;
            public List<EventInfo> events = new List<EventInfo>();
        }

        [Serializable] class EventInfo
        {
            public string name;
            public string[] persistent;
            public int runtimeListenerCount;
        }

        [Serializable] class AssetRef
        {
            public string name, kind, assetPath;
            public int instanceId;
        }

        [Serializable] class MaterialSlot
        {
            public int slot;
            public AssetRef material, shader, mainTexture;
        }

        [Serializable] class Surface : Item
        {
            public bool raycastTarget, culled;
            public string canvas;
            public Vector2[] screenCorners;
        }

        [Serializable] class GraphicBinding : Item
        {
            public string sourceKind, sourceAssetPath;
            public AssetRef sourceSprite, sourceTexture;
            public MaterialSlot[] submittedMaterials, submittedPopMaterials;
        }

        [Serializable] class RendererBinding : Item
        {
            public MaterialSlot[] sharedMaterialSlots;
        }

        [Serializable] class CameraInfo : Item
        {
            public int cullingMask, targetDisplay;
            public bool orthographic;
            public float depth, fieldOfView, orthographicSize;
            public Rect pixelRect;
            public Matrix4x4 projection, view;
        }

        [Serializable] class CanvasInfo : Item
        {
            public string renderMode, camera;
            public int sortingOrder, targetDisplay;
            public float scaleFactor;
        }

        [Serializable] class Draw : Item
        {
            public string[] materials;
            public bool forceRenderingOff;
            public int sortingOrder;
        }

        [Serializable] class Spatial
        {
            public string key, kind;
            public int ordinal, layer;
            public bool active, enabled;
            public Vector3 position, localPosition, localScale;
            public Quaternion rotation, localRotation;
            public Matrix4x4 localToWorld;
        }

        [Serializable] class Hotspot : Spatial { public string id; }

        [Serializable] class Collision : Spatial
        {
            public string[] directHotspotIds;
            public bool isTrigger, convex;
            public Vector3 boundsCenter, boundsSize, shapeCenter, shapeSize;
            public float radius, height, contactOffset;
            public int direction, includeLayers, excludeLayers, layerOverridePriority, cookingOptions;
            public string meshGuid, meshLocalId, meshName;
            public int meshVertexCount;
        }

        [Serializable] class ProjectedRoute : Spatial
        {
            public bool pointsReadable;
            public Vector2[] projectedCanvasPoints;
        }

        [Serializable] class Loaded
        {
            public string name, kind, assetPath;
            public long runtimeBytes;
            public int instanceId;
        }

        [Serializable] class Report
        {
            public string unity, recordedUtc, sourceCommit, state, scene;
            public int frame, width, height;
            // Absent in inventories captured before resource bindings were recorded.
            public int resourceBindingVersion = 1;
            public List<GraphicBinding> graphicBindings = new List<GraphicBinding>();
            public List<RendererBinding> rendererBindings = new List<RendererBinding>();
            public List<Hotspot> hotspots = new List<Hotspot>();
            public List<Collision> colliders3D = new List<Collision>();
            public List<ProjectedRoute> projectedMarchRoutes = new List<ProjectedRoute>();
            public List<Widget> controls = new List<Widget>();
            public List<Surface> graphics = new List<Surface>();
            public List<Item> raycasters = new List<Item>();
            public List<CameraInfo> cameras = new List<CameraInfo>();
            public List<CanvasInfo> canvases = new List<CanvasInfo>();
            public List<Draw> renderers = new List<Draw>();
            public List<Loaded> loadedEditorObjects = new List<Loaded>();
        }

        static AssetRef Asset(Object value)
        {
            if (!value) return null;
            return new AssetRef { name = value.name, instanceId = value.GetInstanceID(),
                kind = value.GetType().FullName, assetPath = AssetDatabase.GetAssetPath(value) };
        }

        static MaterialSlot Slot(int index, Material material)
        {
            return new MaterialSlot { slot = index, material = Asset(material),
                shader = material ? Asset(material.shader) : null,
                mainTexture = material ? Asset(material.mainTexture) : null };
        }

        static GraphicBinding CaptureBinding(Graphic graphic)
        {
            var renderer = graphic.canvasRenderer;
            var surface = new GraphicBinding {
                sourceKind = "submitted-materials-only",
                submittedMaterials = new MaterialSlot[renderer.materialCount],
                submittedPopMaterials = new MaterialSlot[renderer.popMaterialCount]
            };
            if (graphic is Image image)
            {
                var sprite = image.overrideSprite ? image.overrideSprite : image.sprite;
                surface.sourceKind = "image";
                surface.sourceSprite = Asset(sprite);
                if (sprite) surface.sourceTexture = Asset(sprite.texture);
            }
            else if (graphic is RawImage raw)
            {
                surface.sourceKind = "raw-image";
                surface.sourceTexture = Asset(raw.texture);
            }
            // A runtime-generated sprite may have no asset path even when its texture does.
            surface.sourceAssetPath = surface.sourceSprite?.assetPath;
            if (string.IsNullOrEmpty(surface.sourceAssetPath))
                surface.sourceAssetPath = surface.sourceTexture?.assetPath;
            // Inspect bindings already submitted; evaluating materialForRendering could allocate
            // a fresh stencil material, and Graphic.mainTexture can invoke custom generators.
            for (int i = 0; i < surface.submittedMaterials.Length; ++i)
                surface.submittedMaterials[i] = Slot(i, renderer.GetMaterial(i));
            for (int i = 0; i < surface.submittedPopMaterials.Length; ++i)
                surface.submittedPopMaterials[i] = Slot(i, renderer.GetPopMaterial(i));
            return Identify(surface, graphic, graphic.enabled);
        }

        static string Hierarchy(Transform t)
        {
            if (t == null) return "";
            var names = new Stack<string>();
            for (var p = t; p != null; p = p.parent)
                names.Push(p.name + "[" + p.GetSiblingIndex() + "]");
            return t.gameObject.scene.name + "/" + string.Join("/", names);
        }

        static bool Live(Component c)
        {
            var scene = c.gameObject.scene;
            return scene.IsValid() && scene.isLoaded && !EditorUtility.IsPersistent(c)
                && !EditorSceneManager.IsPreviewScene(scene);
        }

        static IEnumerable<T> All<T>() where T : Component =>
            Resources.FindObjectsOfTypeAll<T>().Where(Live).OrderBy(c => Hierarchy(c.transform), StringComparer.Ordinal);

        static T Identify<T>(T row, Component c, bool enabled) where T : Item
        {
            row.path = Hierarchy(c.transform);
            row.component = c.GetType().FullName;
            row.componentOrdinal = Array.IndexOf(c.GetComponents(c.GetType()), c);
            row.activeInHierarchy = c.gameObject.activeInHierarchy;
            row.enabled = enabled;
            row.hideFlags = (int)c.hideFlags;
            return row;
        }

        static Vector2[] ScreenCorners(RectTransform rect, Canvas canvas)
        {
            if (canvas == null) return null;
            var root = canvas.rootCanvas;
            Camera camera = root.renderMode == RenderMode.ScreenSpaceOverlay ? null : root.worldCamera;
            if (camera == null && root.renderMode == RenderMode.WorldSpace) return null;
            var corners = new Vector3[4];
            rect.GetWorldCorners(corners);
            return corners.Select(p => RectTransformUtility.WorldToScreenPoint(camera, p)).ToArray();
        }

        // Runtime listeners have no public enumeration API. A missing internal field yields -1.
        static int RuntimeListenerCount(UnityEventBase evt)
        {
            var calls = typeof(UnityEventBase).GetField("m_Calls", BindingFlags.Instance | BindingFlags.NonPublic)?.GetValue(evt);
            var entries = calls?.GetType().GetField("m_RuntimeCalls", BindingFlags.Instance | BindingFlags.NonPublic)?.GetValue(calls);
            return entries is ICollection collection ? collection.Count : -1;
        }

        static Widget CaptureControl(Selectable s)
        {
            var r = (RectTransform)s.transform;
            var w = Identify(new Widget(), s, s.enabled);
            w.interactable = s.interactable;
            w.effectiveInteractable = s.isActiveAndEnabled && s.IsInteractable();
            w.targetGraphic = Hierarchy(s.targetGraphic == null ? null : s.targetGraphic.transform);
            w.navigation = s.navigation.mode.ToString();
            w.anchorMin = r.anchorMin;
            w.anchorMax = r.anchorMax;
            w.anchoredPosition = r.anchoredPosition;
            w.pivot = r.pivot;
            w.sizeDelta = r.sizeDelta;
            w.screenCorners = ScreenCorners(r, r.GetComponentInParent<Canvas>(true));
            // Read serialized event fields directly. Do not execute a custom property getter or invoke an event.
            for (Type type = s.GetType(); type != null && type != typeof(MonoBehaviour); type = type.BaseType)
            {
                foreach (var field in type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly))
                {
                    if (!typeof(UnityEventBase).IsAssignableFrom(field.FieldType)) continue;
                    if (!(field.GetValue(s) is UnityEventBase evt)) continue;
                    var targets = new List<string>();
                    for (int i = 0; i < evt.GetPersistentEventCount(); i++)
                    {
                        var target = evt.GetPersistentTarget(i);
                        string name = target is Component c ? Hierarchy(c.transform) + ":" + c.GetType().FullName :
                            (target == null ? "<missing>" : target.name);
                        targets.Add(name + "." + evt.GetPersistentMethodName(i) + " (" + evt.GetPersistentListenerState(i) + ")");
                    }
                    w.events.Add(new EventInfo { name = type.Name + "." + field.Name, persistent = targets.ToArray(),
                        runtimeListenerCount = RuntimeListenerCount(evt) });
                }
            }
            return w;
        }

        static string SpatialKey(Transform t)
        {
            // Ordinal among identically named siblings keeps unrelated decorative insertions out of the key.
            var parts = new Stack<string>();
            var scene = t.gameObject.scene;
            for (var p = t; p != null; p = p.parent)
            {
                int number = 0;
                if (p.parent != null)
                {
                    for (int i = 0; i < p.GetSiblingIndex(); i++)
                        if (p.parent.GetChild(i).name == p.name) number++;
                }
                else
                {
                    foreach (var root in scene.GetRootGameObjects())
                    {
                        if (root.transform == p) break;
                        if (root.name == p.name) number++;
                    }
                }
                parts.Push(Uri.EscapeDataString(p.name) + "[" + number + "]");
            }
            return Uri.EscapeDataString(string.IsNullOrEmpty(scene.path) ? scene.name : scene.path) + "/" + string.Join("/", parts);
        }

        static T Pose<T>(T row, Component c, bool enabled) where T : Spatial
        {
            var t = c.transform;
            row.key = SpatialKey(t);
            row.kind = c.GetType().FullName;
            row.ordinal = Array.IndexOf(c.GetComponents(c.GetType()), c);
            row.layer = c.gameObject.layer;
            row.active = c.gameObject.activeInHierarchy;
            row.enabled = enabled;
            row.position = t.position;
            row.rotation = t.rotation;
            row.localPosition = t.localPosition;
            row.localRotation = t.localRotation;
            row.localScale = t.localScale;
            row.localToWorld = t.localToWorldMatrix;
            return row;
        }

        static void CaptureSpatial(Report report)
        {
            var ids = new Dictionary<GameObject, List<string>>();
            foreach (var m in All<MonoBehaviour>())
            {
                if (m.GetType().FullName == "Eldoria.Presentation.WorldHotspot")
                {
                    var id = m.GetType().GetField("Id")?.GetValue(m) as string;
                    // Preserve missing IDs; the offline comparison explicitly rejects these captures.
                    report.hotspots.Add(Pose(new Hotspot { id = id ?? "" }, m, m.enabled));
                    if (!ids.TryGetValue(m.gameObject, out var list))
                        ids[m.gameObject] = list = new List<string>();
                    list.Add(id ?? "");
                }
                else if (m.GetType().FullName == "Eldoria.ReferenceClean.ValoriaMarchRouteGraphic")
                {
                    var field = m.GetType().GetField("points", BindingFlags.Instance | BindingFlags.NonPublic);
                    var points = field?.GetValue(m) as Vector2[];
                    report.projectedMarchRoutes.Add(Pose(new ProjectedRoute { pointsReadable = points != null,
                        projectedCanvasPoints = points == null ? Array.Empty<Vector2>() : (Vector2[])points.Clone() }, m, m.enabled));
                }
            }
            foreach (var c in All<Collider>())
            {
                var bounds = c.bounds;
                var row = Pose(new Collision { boundsCenter = bounds.center, boundsSize = bounds.size, isTrigger = c.isTrigger,
                    contactOffset = c.contactOffset, includeLayers = c.includeLayers.value, excludeLayers = c.excludeLayers.value,
                    layerOverridePriority = c.layerOverridePriority, directHotspotIds = ids.TryGetValue(c.gameObject, out var list)
                        ? list.OrderBy(id => id, StringComparer.Ordinal).ToArray() : Array.Empty<string>() }, c, c.enabled);
                if (c is BoxCollider box) { row.shapeCenter = box.center; row.shapeSize = box.size; }
                else if (c is SphereCollider sphere) { row.shapeCenter = sphere.center; row.radius = sphere.radius; }
                else if (c is CapsuleCollider capsule)
                {
                    row.shapeCenter = capsule.center; row.radius = capsule.radius;
                    row.height = capsule.height; row.direction = capsule.direction;
                }
                else if (c is MeshCollider mesh)
                {
                    row.convex = mesh.convex;
                    row.cookingOptions = (int)mesh.cookingOptions;
                    if (mesh.sharedMesh != null)
                    {
                        row.meshName = mesh.sharedMesh.name;
                        row.meshVertexCount = mesh.sharedMesh.vertexCount;
                        if (AssetDatabase.TryGetGUIDAndLocalFileIdentifier(mesh.sharedMesh, out string guid, out long localId))
                        {
                            row.meshGuid = guid;
                            row.meshLocalId = localId.ToString(System.Globalization.CultureInfo.InvariantCulture);
                        }
                    }
                }
                report.colliders3D.Add(row);
            }
        }

        [MenuItem("Tools/Eldoria QA/Capture live inventory", true)]
        static bool CanCapture() => EditorApplication.isPlaying;

        [MenuItem("Tools/Eldoria QA/Capture live inventory")]
        static void Save()
        {
            var path = EditorUtility.SaveFilePanel("Live inventory – choose a state label", "", "state.json", "json");
            if (path.Length == 0) return;
            Capture(path, Environment.GetEnvironmentVariable("ELDORIA_QA_SOURCE_SHA") ?? "", Path.GetFileNameWithoutExtension(path));
        }

        public static void Capture(string path, string sourceCommit, string stateLabel)
        {
            if (!EditorApplication.isPlaying) throw new InvalidOperationException("Enter Play Mode before capturing.");
            if (string.IsNullOrEmpty(sourceCommit) || !System.Text.RegularExpressions.Regex.IsMatch(sourceCommit, "^[0-9a-f]{40}$"))
                throw new ArgumentException("Provide ELDORIA_QA_SOURCE_SHA or pass the verified full SHA.");
            var report = new Report { sourceCommit = sourceCommit, state = stateLabel, unity = UnityEngine.Application.unityVersion,
                recordedUtc = DateTime.UtcNow.ToString("o"), scene = SceneManager.GetActiveScene().path,
                width = Screen.width, height = Screen.height, frame = Time.frameCount };
            CaptureSpatial(report);
            foreach (var s in All<Selectable>()) report.controls.Add(CaptureControl(s));
            foreach (var g in All<Graphic>())
            {
                report.graphics.Add(Identify(new Surface { raycastTarget = g.raycastTarget, culled = g.canvasRenderer.cull,
                    canvas = Hierarchy(g.canvas == null ? null : g.canvas.transform), screenCorners = ScreenCorners(g.rectTransform, g.canvas) }, g, g.enabled));
                report.graphicBindings.Add(CaptureBinding(g));
            }
            foreach (var ray in All<BaseRaycaster>()) report.raycasters.Add(Identify(new Item(), ray, ray.enabled));
            foreach (var c in All<Camera>())
                report.cameras.Add(Identify(new CameraInfo { orthographic = c.orthographic, depth = c.depth, cullingMask = c.cullingMask,
                    fieldOfView = c.fieldOfView, orthographicSize = c.orthographicSize, pixelRect = c.pixelRect,
                    targetDisplay = c.targetDisplay, view = c.worldToCameraMatrix, projection = c.projectionMatrix }, c, c.enabled));
            foreach (var c in All<Canvas>())
                report.canvases.Add(Identify(new CanvasInfo { renderMode = c.renderMode.ToString(), sortingOrder = c.sortingOrder,
                    camera = Hierarchy(c.worldCamera == null ? null : c.worldCamera.transform),
                    targetDisplay = c.targetDisplay, scaleFactor = c.scaleFactor }, c, c.enabled));
            foreach (var r in All<Renderer>())
            {
                var shared = r.sharedMaterials;
                report.renderers.Add(Identify(new Draw {
                    materials = shared.Select(m => m == null ? "" : m.name).ToArray(),
                    sortingOrder = r.sortingOrder, forceRenderingOff = r.forceRenderingOff }, r, r.enabled));
                report.rendererBindings.Add(Identify(new RendererBinding {
                    sharedMaterialSlots = shared.Select((m, i) => Slot(i, m)).ToArray() }, r, r.enabled));
            }
            foreach (var obj in Resources.FindObjectsOfTypeAll<Material>().Cast<Object>().Concat(Resources.FindObjectsOfTypeAll<Texture>()))
                report.loadedEditorObjects.Add(new Loaded { name = obj.name, kind = obj.GetType().FullName,
                    instanceId = obj.GetInstanceID(), assetPath = AssetDatabase.GetAssetPath(obj), runtimeBytes = Profiler.GetRuntimeMemorySizeLong(obj) });
            using (var stream = new FileStream(path, FileMode.CreateNew, FileAccess.Write))
            using (var writer = new StreamWriter(stream)) writer.Write(JsonUtility.ToJson(report, true));
        }
    }
}
