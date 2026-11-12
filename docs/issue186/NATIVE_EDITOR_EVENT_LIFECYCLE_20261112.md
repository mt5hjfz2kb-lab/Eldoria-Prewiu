# #186 — Native uGUI Editor lifecycle, 12 November 2026

The [native batch 43123810935](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/43123810935) exited cleanly, with 16 authentic Unity sprites/GUIDs/hashes imported, but failed before its first image at `Disposable real EventSystem is not the current active system`. Artifact 13903005932 contains the error log. Four screenshots, actual game flow and visual approval are **not** established.

Research: the [official Unity uGUI implementation](https://github.com/Unity-Technologies/uGUI/blob/main/com.unity.ugui/Runtime/UGUI/EventSystem/EventSystem.cs) registers an EventSystem in `OnEnable`; `EventSystem.current` only reorders an already-registered instance. `Selectable` is likewise registered on `OnEnable`, and the raycaster must remain genuinely active. A public `current` assignment does not repair missing Editor lifecycle registration.

The isolated fixture now sends its URP render request before creating the true Editor-only EventSystem; after each subsequent request it checks registration and, if needed, invokes Unity's normal GameObject disable/enable callbacks. No private-state mutation, fake RaycastAll results, click-through or removal of the modal-blocker negative control. Camera destruction now precedes RenderTexture release to prevent the observed warning.

This is a targeted **source correction**, not a Unity PASS. Next exclusive Windows batch must show genuine input dispatch in blocked and unblocked states, all four GPU frames, 48–96px legibility and direct A/B against approved visual reference. Do not monopolize Windows or merge before review.
