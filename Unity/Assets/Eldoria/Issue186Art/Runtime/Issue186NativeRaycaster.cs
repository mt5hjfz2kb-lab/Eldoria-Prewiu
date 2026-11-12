#if UNITY_EDITOR
using UnityEngine;
using UnityEngine.UI;

namespace Eldoria.Issue186Art.Editor
{
    // BaseRaycaster is also not ExecuteAlways. Run its real registration
    // lifecycle so EventSystem.RaycastAll includes this actual camera/Canvas.
    // Its inherited GraphicRaycaster.Raycast and sorting are unchanged.
    [ExecuteAlways]
    [AddComponentMenu("")]
    public sealed class Issue186NativeRaycaster : GraphicRaycaster
    {
        protected override void OnEnable() { base.OnEnable(); }
        protected override void OnDisable() { base.OnDisable(); }
    }
}
#endif
