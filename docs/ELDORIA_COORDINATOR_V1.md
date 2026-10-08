# ELDORIA — Coordinador Central v1
Date: 2026-10-08. Owner authorization: build a coordinated, cost-controlled pilot.
## Status and actual capabilities
This is a **read-only, zero-credit routing and evidence gate**, not a 15-agent autonomous studio.
It runs automatically on changes to canonical registry, daily, or manually, and produces a JSON artifact with a per-gate verdict.
It cannot awaken ChatGPT chats, start Work, create PRs, claim QA ownership, run Unity, certify art or modify the game.
No artifact is a substitute for actual independent evidence. A missing field produces BLOCKED.
## Pilot: Dirección General → Arte M07 → QA → Dirección General
1. Dirección General authorizes bounded Art M07 through existing canonical registry and Issue #23.
2. Art produces correction, post-correction real Unity screenshots, independent visual certification and WebGL evidence.
3. Art records the closure evidence under the completed M07 record's `result` object, including: `post_patch_capture_reviewed=true`, `post_patch_capture_artifact_id`, `post_patch_capture_source_sha`, `independent_visual_pass=true`, `visual_review_evidence`, `published_webgl_verified=true`, `published_probe_run_id`, `published_source_sha`. These are explicit cross-check references, not inferred PASS statuses.
4. The coordinator emits BLOCKED_NO_HANDOFF until ALL gates pass, then READY_FOR_QA_HANDOFF_REVIEW. READY means permission to evaluate a handoff request, not QA certification or auto-launch.
5. Only a separately authorized dispatcher or owning agent may open the QA handoff; QA must independently inspect evidence and run its checks. Direction General records final verdict in Issue #23.
## Integrity / boundaries
M07 remains blocked until third iteration screenshot inspection and exact-build verification. M11 is not authorized.
Workflow uses read-only repository permissions; no secrets, paid runners, edits, publishing or PR triggers.
Source of truth: AGENTS.md, SESSION_HANDOFF.md, PROJECT_STATE.md and pipeline/active-workstreams.json; Issue #23 is reporting, not an override.
When developing an actual autonomous executor v2, first specify an approved execution identity, API/token budget, idempotent dispatch, authorization gates, QA isolation and recovery semantics. Do not assume chats can be woken by GitHub.
