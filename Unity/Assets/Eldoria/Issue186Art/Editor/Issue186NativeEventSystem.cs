#if UNITY_EDITOR
using UnityEngine;
using UnityEngine.EventSystems;

namespace Eldoria.Issue186Art.Editor
{
    // Editor-only disposable art fixture. EventSystem itself is not
    // ExecuteAlways: without its genuine OnEnable registration, current
    // and isActiveAndEnabled are invalid in a synchronous Editor capture.
    // No private static injection, fake raycasts or production component.
    [ExecuteAlways]
    [AddComponentMenu("")]
    public sealed class Issue186NativeEventSystem : EventSystem
    {
        protected override void OnEnable() { base.OnEnable(); }
        protected override void OnDisable() { base.OnDisable(); }
    }
}
#endif
