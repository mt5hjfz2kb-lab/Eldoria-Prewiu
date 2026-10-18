# World4X integration surface V1 — source checkpoint, native gate pending

Only consume this branch's `World4XZone1` assets/components after native evidence identifies the exact validated revision. Do not merge the entire historical branch into Valoria.

## Existing shared session, never a second save
`ZoneOneGatewayHost(ICommandGateway gateway, Action returnToCity, Func<long> utcTicks)` consumes the **same gateway instance already owned by Valoria**. It does not create LocalGateway, LocalServices, a wallet, or a store. `Snapshot`, `Inspect`, `Execute` and `Refreshed` let the interface bind selection and actions. `Refresh(true)` is optional explicit timer advancement: use `Refresh()` when the common host already ticks the session.

The UI supplies a stable nonempty request ID for each player action and retains that ID for retries. The existing gateway remains responsible for revision checks, availability, rewards and command idempotency. `Inspect.Enabled` is only a presentation hint; it cannot authorize an action. The map never writes PlayerState or substitutes its own economy.

Supported existing semantic IDs: `valoria-map-city`, `forest-valoria`, `quarry-valoria`, `corrupt-scout`, `engendro-valoria`. `old-watch-ruin` is inspect-only. Unknown/generated food/wood/enemy IDs are deliberately NOT remapped onto existing canonical nodes. Future quantities/IDs require genuine content and gameplay support by the shared-state owner.

## Camera and UI ownership
`ZoneOneStrategicCamera` retains a single coordinate space. Global/regional/local are camera levels; authored features are not repositioned. `ZoomAt` preserves a ground-plane cursor focus except where an edge limit necessarily clamps it. `FitToAuthoredBounds` fits the entire viewport in the authored patch in landscape and portrait. These are source-level capabilities pending the native gate, not physical-device certification.

Inject the UI hit-test through `PointerBlocked`, rather than adding a second EventSystem or hardcoding another HUD. Scene transition and return stay with the common Valoria/UI owner; this branch does not replace SliceBoot, package configuration or the UI's reusable art.

## Important scene limitation
This historical branch's `SliceBoot` initializes the old game automatically in PlayMode. It is owned by Valoria and is NOT edited here. Therefore the saved `ZoneOneGround_EditorReview.unity` is an **Editor review artifact**, not a standalone playable delivery. Use the generated ground prefab inside an opt-in scene/module controlled by the current host. A native Editor screenshot or mocked gateway test does not prove runtime scene transition or end-to-end gameplay.

Latest Work interface inspected read-only: `IPlayableSceneHost` provides PlayerCamera, Apply, Home/Pan/Zoom/Focus, Navigate, TargetName and Pick. That interface is not present in this older branch; no duplicate definition is introduced. The integration owner should adapt this branch's public camera/host API to its actual current interface at the integration revision.

## Current units and evidence
- Exact spatial identity test has executed successfully in run `39125021328`; retained recovery manifest and test output under `evidence/recovery-20261018`.
- Real source/PNG recovery from run `39126030933` succeeded. All three actual 8B plans were rejected **before geometry modification** for numeric/unit errors; raw outputs and the saved baseline are preserved.
- `resume_ground_finish.py` resumes that exact saved baseline and frozen original source hash. It uses the already-installed 14B text director with the actual source and independent visual observations, without pretending the text model directly sees images. Previous baseline renders are reused only after byte-hash checks.
- Native entry `ZoneOneNativeCandidate.Build`: actual engine import/camera/host checks, matched A/B captures, cloned persistent meshes/materials/textures, generated prefab, scene save/reopen. These checks are implemented but NOT yet claimed executed at this checkpoint.

Required next gate: complete the requested ground edit, inspect original paired PNGs independently, then request the smallest real Unity comparison of the exact candidate. Separate source, visual, camera, gameplay and physical-mobile results. No production or visual approval is implicit.
