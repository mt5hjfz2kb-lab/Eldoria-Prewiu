# Eldoria — Direct delivery coordination, 2026-10-10

Purpose: durable, read-only-to-production coordination for disconnected chats. This document does not change ownership, create an additional department, authorize spending, or constitute a CI trigger. Always re-read live branch HEAD and GitHub Actions before making decisions.

## Workstream A — Region 1 visual v3
- Canonical branch: `main`. Registered owner: `chat-chief-programmer-takeover-20261010`.
- Current authoritative registry: `pipeline/active-workstreams.json`.
- Reserved shared resources: `windows-self-hosted-unity-6000-3-23f1`, GitHub Pages publication.
- Source of truth: `Unity/Assets/Eldoria/Scripts/Presentation/WorldRegion1Runtime.cs`, Unity gates and `pipeline/unity-publish-request.json`.
- At coordination snapshot: HEAD `672e38fe277e4bb5d020f44d53e9be414a5a1afd`; last exact candidate publish workflow `38070956363` completed SUCCESS; new published startup probe `38072238292` IN_PROGRESS. Publication request has `release_approved:false`. Neither build nor startup PASS implies final visual/gameplay approval.
- Closure: matched camera visual review; full candidate gameplay/persistence QA; exact artifact publication only after authorized gate; published QA. Preserve camera/Valoria and game state.

## Workstream B — replace SHARP with authored mesh
- Isolated branch: `prototype/valoria-bastion1-no-sharp-20261010`.
- At coordination snapshot: HEAD `d413e159a4a9314b82b456da0759625699ece8c7`.
- Recent authored source: `a9b3b5cd1a` (ashlar arch, stair masonry, roof courses), followed by stored Blender low-resolution proof. Prior isolated Blender preview `38070306673` completed SUCCESS.
- No automatic merge into main, no Pages publication, no shared Windows runner until A releases it.
- Closure: independent geometry, coherent terrain/architecture, inspected matched-camera Blender and Unity visuals, mobile rendering budget, gameplay parity; avoid equating source/pass or preview/pass with visual acceptance.

## Supporting zero-credit tooling
- `tools/art-rd/audit_glb.py` and `.github/workflows/art-glb-preflight.yml` exist on `main` and the experimental branch. `tools/art-rd/test_audit_glb.py` is currently on main. Recent main audit run `38070361696`: SUCCESS.
- Structural GLB checks are diagnostic only; not artistic, Unity runtime, mobile quality, or proof of absence of all glTF import issues.

## Recovery procedure on disconnected chats
1. Read `AGENTS.md`, this document, live `pipeline/active-workstreams.json`, branch HEADs, `pipeline/unity-publish-request.json` and the latest workflow jobs/artifacts by SHA. This snapshot is not itself live status.
2. Resume the existing workstream; do not reset branches, recreate tasks or make parallel duplicate candidate publishes.
3. If runner/Page resources are leased to A, B continues isolated Blender/source work. An idle Actions queue does not automatically release lease.
4. Keep every fix scoped to its own branch/files. Do not merge prototype into main until visual and gameplay gates are satisfied.
5. No paid credits or new subscriptions. Never label a technical gate as artistic PASS.
6. When a chat disconnects, its process may stop even if GitHub Actions finishes. A subsequent chat must explicitly inspect actual run conclusion and continue; a document cannot restart a chat automatically.

## Release priority
Complete Region 1 published visual/gameplay acceptance and free the shared runner. In parallel, keep raising the isolated authored mesh prototype to the visual threshold. Only then compare against SHARP with equal camera/lighting/gameplay and decide migration.
