using UnityEngine;
using UnityEngine.InputSystem;

namespace Eldoria.Presentation
{
    // Isolated visual feasibility test: never loaded by the Valoria gameplay scene.
    public sealed class LayeredValoriaArtTest : MonoBehaviour
    {
        public Camera ReviewCamera;
        public Renderer Bastion;
        public Material InitialStage;
        public Material RebuiltStage;

        int stage = 1;
        int zoom = 1;
        static readonly float[] ZoomSizes = { 19f, 12f, 9f };

        void Start() => Apply();

        void Update()
        {
            var keyboard = Keyboard.current;
            if (keyboard != null)
            {
                if (keyboard.digit1Key.wasPressedThisFrame) SetStage(1);
                if (keyboard.digit2Key.wasPressedThisFrame) SetStage(2);
                if (keyboard.qKey.wasPressedThisFrame) SetZoom(0);
                if (keyboard.wKey.wasPressedThisFrame) SetZoom(1);
                if (keyboard.eKey.wasPressedThisFrame) SetZoom(2);
            }
            var mouse = Mouse.current;
            if (mouse == null || !mouse.leftButton.wasPressedThisFrame || ReviewCamera == null) return;
            var ray = ReviewCamera.ScreenPointToRay(mouse.position.ReadValue());
            if (Physics.Raycast(ray, out var hit) && hit.collider.gameObject == Bastion.gameObject)
                SetStage(stage == 1 ? 2 : 1);
        }

        public void SetStage(int value) { stage = Mathf.Clamp(value, 1, 2); Apply(); }
        public void SetZoom(int value) { zoom = Mathf.Clamp(value, 0, 2); Apply(); }

        void Apply()
        {
            if (Bastion != null) Bastion.sharedMaterial = stage == 1 ? InitialStage : RebuiltStage;
            if (ReviewCamera != null) ReviewCamera.orthographicSize = ZoomSizes[zoom];
        }

        void OnGUI()
        {
            GUILayout.BeginArea(new Rect(14, 14, 215, 185), GUI.skin.box);
            GUILayout.Label("BASTIÓN · PRUEBA DE ARTE");
            GUILayout.Label("Pieza ilustrada · escena aislada");
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Inicio [1]")) SetStage(1);
            if (GUILayout.Button("Reconstruido [2]")) SetStage(2);
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Lejos [Q]")) SetZoom(0);
            if (GUILayout.Button("Ciudad [W]")) SetZoom(1);
            if (GUILayout.Button("Detalle [E]")) SetZoom(2);
            GUILayout.EndHorizontal();
            GUILayout.Label("Toca el Bastión para cambiar etapa.");
            GUILayout.EndArea();
        }
    }
}
