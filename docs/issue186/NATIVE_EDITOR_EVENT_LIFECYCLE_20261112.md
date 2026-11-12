# #186 — Native uGUI Editor lifecycle, 12 November 2026

The [native batch 43123810935](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/43123810935) exited cleanly, with 16 authentic Unity sprites/GUIDs/hashes imported, but failed before its first image at `Disposable real EventSystem is not the current active system`. Artifact 13903005932 contains the error log. Four screenshots, actual game flow and visual approval are **not** established.

Research: the [official Unity uGUI implementation](https://github.com/Unity-Technologies/uGUI/blob/main/com.unity.ugui/Runtime/UGUI/EventSystem/EventSystem.cs) registers an EventSystem in `OnEnable`; `EventSystem.current` only reorders an already-registered instance. `Selectable` is likewise registered on `OnEnable`, and the raycaster must remain genuinely active. A public `current` assignment does not repair missing Editor lifecycle registration.

The isolated fixture now sends its URP render request before creating the true Editor-only EventSystem; after each subsequent request it checks registration and, if needed, invokes Unity's normal GameObject disable/enable callbacks. No private-state mutation, fake RaycastAll results, click-through or removal of the modal-blocker negative control. Camera destruction now precedes RenderTexture release to prevent the observed warning.

This is a targeted **source correction**, not a Unity PASS. Next exclusive Windows batch must show genuine input dispatch in blocked and unblocked states, all four GPU frames, 48–96px legibility and direct A/B against approved visual reference. Do not monopolize Windows or merge before review.

## Actual Unity diagnostic and structural repair — native run 43134015062

The exact pinned source `d2b2f1b5a6093805304a2d6a1df297d0861ef242` compiled in the real Unity 6000.3.23f1 Editor. Unity imported **16/16** actual PNG textures with their hashes and GUIDs, and exported **146** real installed managed API/dependency DLLs with recorded hashes, zero missing dependencies ([run](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/43134015062); native log artifact **13907471063**, API artifact **13906576787**). Then the Editor reported **both** disposable helpers as scripts that could not be attached to a GameObject because their `.cs` files lived in an `Editor` folder:

- `Issue186NativeRaycaster`: rejected Editor-folder `MonoBehaviour` attachment.
- `Issue186NativeEventSystem`: rejected Editor-folder `MonoBehaviour` attachment.

Consequently the subsequent assertion `exactly one active EventSystem` failed **before the first of four screenshots**. This is a proven script-placement/assembly membership error, not evidence of an opaque badge background or of a successful pointer hit. A previous strategy of repeatedly adjusting `EventSystem.current` and render timing could not address an **unattachable** component. The process exited; Windows was formally handed to PR179 at registry revision 15 without interrupting the native job or starting another batch.

The source repair on **PR #187 only** moves those exact two helper sources and their existing `.meta` files from `Issue186Art/Editor/` into `Issue186Art/Runtime/`, preserving both MonoImporter GUIDs (`e69a23728b864b85a194b9a6dfb2fb3b` and `17d6c579bf0e4986ac3f02b7ede2c3f3`) and their `#if UNITY_EDITOR` guards. The existing `Runtime/Eldoria.RoyalGold.V2.asmref` gives the fixture real UnityEngine/uGUI assembly membership without creating another game assembly. Real Editor-only test helpers remain excluded from player builds by conditional compilation; **exclusion itself has not yet been measured in a player build**.

The Roslyn/source audit now enforces source placement outside `Editor`, guards, and exact preserved GUIDs, rather than relying on syntax checks that could not catch the failure. Commit `5512b33a7ad4e4bcf7b066eed53a92555aad5e2e` passed [source workflow 43136085959](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/43136085959): 16 source sprites, 139 domain checks, **544 compiled gateway/JSON save checks**, 660 source-layout checks, and nine C# syntax trees. **No semantic Unity build, actual four GPU images, first-hit/modal-negative pointer proof, real bag canvas, reference A/B, or mobile proof** is certified by these tests.

**Next legitimate native test** (only after PR179 and subsequent scheduled Windows handoff): verify Unity attaches both real helpers, actual EventSystem/GraphicRaycaster lifecycle, four authentic frames, all-canvas frontmost raycast with modal obstruction, and 48–96px legibility. If the candidate renders, compare the *actual* Unity capture with the owner's previously approved reference before promotion to main. No same-turn repeated native job, no art approval or publication.
