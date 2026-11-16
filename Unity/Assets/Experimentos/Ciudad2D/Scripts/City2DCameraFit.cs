using UnityEngine;

namespace Eldoria.Experimentos.Ciudad2D
{
    [RequireComponent(typeof(Camera))]
    public sealed class City2DCameraFit : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer ground;
        private Camera sceneCamera;
        private int cachedWidth = -1;
        private int cachedHeight = -1;

        public void SetGround(SpriteRenderer renderer)
        {
            ground = renderer;
            Refit();
        }

        private void Awake() => sceneCamera = GetComponent<Camera>();
        private void OnEnable() => Refit();
        private void LateUpdate()
        {
            if (Screen.width != cachedWidth || Screen.height != cachedHeight)
                Refit();
        }

        private void Refit()
        {
            if (sceneCamera == null) sceneCamera = GetComponent<Camera>();
            if (sceneCamera == null || ground == null || ground.sprite == null) return;
            float aspect = Mathf.Max(0.001f, (float)Screen.width / Mathf.Max(1, Screen.height));
            Vector2 size = ground.sprite.bounds.size;
            sceneCamera.orthographicSize = Mathf.Max(size.y * 0.5f, size.x / (2f * aspect));
            cachedWidth = Screen.width;
            cachedHeight = Screen.height;
        }
    }
}
