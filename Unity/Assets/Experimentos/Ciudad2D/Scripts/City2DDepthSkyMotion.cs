using UnityEngine;

namespace Eldoria.Experimentos.Ciudad2D
{
    /// <summary>
    /// Experimental and reversible far-scenery movement ONLY. The original terrain,
    /// buildings, roads, paths, shadows and props are never repositioned.
    /// No gameplay calls or save access. Must be reviewed in native Unity motion.
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(SpriteRenderer))]
    public sealed class City2DDepthSkyMotion : MonoBehaviour
    {
        [SerializeField] private Camera observer;
        [SerializeField] private SpriteRenderer terrain;
        [SerializeField] private Vector3 sourcePosition;
        [SerializeField] private Vector3 homeCameraPosition;
        [SerializeField, Range(0f, .15f)] private float farPanResponse = .085f;
        [SerializeField, Range(0f, .16f)] private float maxDisplacement = .095f;
        [SerializeField] private bool configured;
        private SpriteRenderer visual;
        private Material isolatedMaterial;

        public bool IsPresentationOnly => true;
        public float MaximumWorldDisplacement => maxDisplacement;

        public void Configure(Camera camera, SpriteRenderer ground, float response = .085f)
        {
            if (camera == null || ground == null || ground.sprite == null ||
                !camera.orthographic || response < 0f || response > .15f)
                throw new System.ArgumentException("Invalid far-scenery depth pilot");
            observer = camera;
            terrain = ground;
            sourcePosition = ground.transform.position;
            homeCameraPosition = camera.transform.position;
            farPanResponse = response;
            configured = true;
            visual = GetComponent<SpriteRenderer>();
            visual.sprite = ground.sprite;
            visual.sortingLayerID = ground.sortingLayerID;
            visual.sortingOrder = ground.sortingOrder + 1;
            EnsureMaterial();
            EvaluateForCamera(camera.transform.position);
        }

        private void OnEnable()
        {
            visual = GetComponent<SpriteRenderer>();
            if (configured) EnsureMaterial();
        }

        private void LateUpdate()
        {
            if (UnityEngine.Application.isPlaying && configured && observer != null)
                EvaluateForCamera(observer.transform.position);
        }

        /// <summary>Editor evidence can use the identical sampled camera pose as control A.</summary>
        public void EvaluateForCamera(Vector3 cameraPosition)
        {
            if (!configured || observer == null || terrain == null) return;
            EnsureMaterial();
            Vector3 delta = cameraPosition - homeCameraPosition;
            Vector2 shift = Vector2.ClampMagnitude(
                new Vector2(delta.x, delta.y) * farPanResponse, maxDisplacement);
            transform.position = sourcePosition + new Vector3(shift.x, shift.y, -.0001f);
            transform.rotation = terrain.transform.rotation;
            transform.localScale = terrain.transform.lossyScale;
        }

        private void EnsureMaterial()
        {
            if (visual == null) visual = GetComponent<SpriteRenderer>();
            if (isolatedMaterial != null) return;
            var shader = Shader.Find("Eldoria/City2D/DepthSkyMask");
            if (shader == null || !shader.isSupported)
                throw new System.InvalidOperationException("DepthSkyMask shader absent or unsupported");
            isolatedMaterial = new Material(shader)
            {
                name = "DepthPilot_Sky_TransientMaterial",
                hideFlags = HideFlags.DontSave
            };
            isolatedMaterial.SetFloat("_MaskStart", .80f);
            isolatedMaterial.SetFloat("_MaskEnd", .90f);
            visual.sharedMaterial = isolatedMaterial;
        }

        private void OnDisable()
        {
            if (isolatedMaterial == null) return;
            visual.sharedMaterial = null;
            if (UnityEngine.Application.isPlaying) Destroy(isolatedMaterial);
            else DestroyImmediate(isolatedMaterial);
            isolatedMaterial = null;
        }
    }
}
