# M03-B GitHub provenance, V1
READ-ONLY adapter. `tools/m03/github-evidence.mjs` offers `githubGet` (GET only, api.github.com allowlist), `verifyGitHub` (commit SHA, completed-success workflow run matching repo and SHA, active unexpired artifact with matching digest, run and candidate) and `evaluateM16Verified` (blocks dependency release unless provider evidence verifies). `tests/m03/github-evidence.test.mjs` mocks only provider HTTP responses; it does not certify any Unity build.

`githubGet` uses existing optional `GITHUB_TOKEN`; no new secrets or services. Caller must provide repository; no write permissions requested.

**Fail closed:** Missing run, wrong SHA, cancelled/failure, expired artifact, digest mismatch, insufficient artifact pagination, provider errors, missing deployment. GitHub deployment metadata cannot prove what bytes were viewed in the browser; any PUBLISHED PASS leaves `PUBLISHED_BYTES_NOT_VERIFIED` until a canonical runtime-visible build ID verifier is implemented.

**Caution:** The original synchronous M16-A `evaluate()` still accepts legacy untrusted snapshot mode without `schema_version=1`. Only `evaluateM16Verified()` may be used as a prerequisite for future automatic dispatch. No M16-B is authorized. Current hash-style certificate IDs are content address identifiers, NOT digitally signed attestations. For production trust, require attested issuer/permissions and resolve evidence provenance against provider; mutable run references and implicit GitHub Pages versions must be handled explicitly.

**Historical validation:** GH API observed `37772733956` success with artifact 11549887766 (digest sha256:5378928956c1d7c8d00db605dfb73b9e9a89c4b147d1490b1f5f170040875a01, SHA 4d70b9bd...), and `37775362058` FAIL with artifact 11548999555. Different runs and source SHAs cannot be combined into a passing certificate. Both are historical; neither certifies current published gameplay.

**History:** Keep certificate input/evidence payloads and provider receipts as immutable JSON snapshots retained outside ephemeral CI artifacts (e.g., a versioned separate audit branch/attestation store once authorized). V1 has no auto-write history backend; do NOT overwrite earlier evidence. No changes to workstream ownership, Unity, WebGL or credits.
