using UnityEngine;
using UnityEngine.InputSystem;

namespace Eldoria.ArtTests.ImageTo3D
{
    // Lives only in the isolated review scene. No gameplay or VisualWorld integration.
    public sealed class BastionSelectionProbe : MonoBehaviour
    {
        public Camera ReviewCamera;
        public Transform BastionRoot;
        public bool LastClickSelectedBastion { get; private set; }

        void Update()
        {
            var mouse = Mouse.current;
            if (mouse == null || !mouse.leftButton.wasPressedThisFrame || ReviewCamera == null || BastionRoot == null)
                return;
            var ray = ReviewCamera.ScreenPointToRay(mouse.position.ReadValue());
            LastClickSelectedBastion = Physics.Raycast(ray, out var hit, 250f) &&
                (hit.transform == BastionRoot || hit.transform.IsChildOf(BastionRoot));
            Debug.Log("Bastion 3D raycast: " + (LastClickSelectedBastion ? "selected" : "no selection"));
        }
    }
}
