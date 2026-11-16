using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Eldoria.Experimentos.Ciudad2D
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Collider2D))]
    public sealed class CityBuilding : MonoBehaviour, IPointerClickHandler
    {
        [SerializeField] private string buildingName = "Edificio";
        [Min(1)] [SerializeField] private int level = 10;
        [SerializeField, Range(0.02f, 0.30f)] private float bounceDuration = 0.16f;
        [SerializeField, Range(1.01f, 1.25f)] private float bounceMultiplier = 1.085f;
        private Coroutine bounce;
        private Vector3 restScale;

        public string BuildingName => buildingName;
        public int Level => level;

        public void Configure(string newName, int newLevel)
        {
            buildingName = newName;
            level = Mathf.Max(1, newLevel);
        }

        private void Awake() => restScale = transform.localScale;

        public void OnPointerClick(PointerEventData eventData)
        {
            if (!isActiveAndEnabled || eventData == null ||
                eventData.button != PointerEventData.InputButton.Left)
                return;
            Activate();
        }

        public void Activate()
        {
            Debug.Log($"[Ciudad2D] {buildingName} — nivel {level}", this);
            if (!isActiveAndEnabled) return;
            if (bounce != null)
            {
                StopCoroutine(bounce);
                transform.localScale = restScale;
            }
            bounce = StartCoroutine(Bounce());
        }

        private IEnumerator Bounce()
        {
            float elapsed = 0f;
            while (elapsed < bounceDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsed / bounceDuration);
                float pulse = Mathf.Sin(t * Mathf.PI);
                transform.localScale = restScale * Mathf.Lerp(1f, bounceMultiplier, pulse);
                yield return null;
            }
            transform.localScale = restScale;
            bounce = null;
        }

        private void OnDisable()
        {
            if (bounce != null) StopCoroutine(bounce);
            bounce = null;
            if (restScale != Vector3.zero) transform.localScale = restScale;
        }
    }
}
