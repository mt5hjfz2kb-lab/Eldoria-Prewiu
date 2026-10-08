# M03 — Certification evaluator V1
This is an **offline, deterministic evidence evaluation layer**, not an official Unity certification or a trusted artifact-fetcher.
Run: `node tools/m03/certify.mjs <input.json>`.
Tests: `node --test tests/m03/certify.test.mjs tests/m16a/evaluate.test.mjs`.
Six independent gates: TECH, FUNCTIONAL, PUBLISHED, MOBILE_INTERACTION, VISUAL, EXPERIENCE_REVIEW.
Policy frozen for source_sha before build; independent provenance of every evidence row; reject mismatched/expired/duplicate rows, fatal FAIL and missing required gates.
The JSON Schemas specify candidate, policy, evidence; certificate is an evaluator-derived envelope with immutable policy/evidence digests.
M16-A supports strict M03 envelope when certifications.schema_version=1, while retaining compatibility with legacy test fixtures lacking version declaration.
**Security limit**: untrusted JSON could fabricate a structurally valid run/provenance. Real provider API verification and artifact/deploy inspection require read-only adapters and are deferred until permitted; until then outputs are local provisional certifications, NOT official certified Unity releases.
No M04 incident tracker, runners, workflows or workstreams are changed. Git commit history preserves versions; no credential or signing authority.
