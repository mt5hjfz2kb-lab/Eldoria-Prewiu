# Unity metadata recovery — 4 November 2026

Status: PR153 merged into main at `91419eadbb1b7c678e249438f0b449cbd7dec0bd`.
Native metadata certification and post-merge Unity editor/game tests plus Windows
build PASS. The post-merge UI job has also passed every executed test/validation
step and uploaded its evidence, but GitHub still reports its aggregate status as
in_progress with no conclusion. Technical repair is verified; administrative
closure remains pending that external acknowledgement. See CLOSEOUT.json.

## What changed

- Restored 277 original file metadata and 68 original folder metadata: **345**
  original identities and import settings, recovered from stable existing project
  snapshots. No new GUID was substituted for a recovered original.
- Removed three broad ArtTests metadata exclusions from Unity/.gitignore.
- Archived one orphan package metadata file outside Unity; retained three valid
  empty-folder identities. Two Android bundle internals need no individual metadata.
- Moved the expressly owner-rejected legacy Bastion GLB and its existing metadata
  to ArtSource/RejectedLegacyBastion, preserving both Git blobs, bytes and GUID.
  Its two catalog locators retain OWNER_BLOCKED_NO_USE. The missing folder identity
  was not invented: archival outside Assets resolves that folder obligation.
- Removed 57 redundant pending recovery copies after exact destination equality.
  Historical provenance remains pinned in recovery-manifest.json and Git history.
- Added automatic checks for missing file/folder metadata, duplicate or malformed
  GUIDs, orphan files, ignored metadata and return of the rejected source.

No authored gameplay code, scenes, prefabs, geometry or textures were edited.
The PR preserves other workstreams' source and records their temporary pause states.

## Verified

Native run **41844806444**, Unity6000.3.23f1, candidate
`aae457d6a203fe19a7e74f88fdc14b3d6ec2d948`:

- **345/345** original GUIDs and raw metadata hashes match. Hashes match both
  before import and after all tests. One enabled scene has zero missing scripts.
- **57 EditMode + 73 PlayMode passed**, zero failed or skipped. Assemblies originate
  from the exact isolated project; save identity and probe data are disposable.
- Canonical runtime preparation completed. Unexpected tracked changes: **0**.
- The isolated checkout is **not pristine**: 61 pre-existing import/fixture side
  effects are explicitly verified. Sixty use exact original/result Git blobs;
  the vendor demo scene additionally validates its generated LightingSettings
  reference and every remaining byte. None of those side effects is committed.
- Static integrity: zero missing metadata, duplicate GUIDs, file orphans or ignore
  regressions. The metadata suite has 23 passing tests; native harness contracts
  are exercised in PowerShell. Existing catalog utility tests and binary QA pass.

The immutable certificate, native report, tracked-state review and archive hashes
are committed alongside this report. All prior failed attempts remain recorded in
NATIVE_ATTEMPTS.json; they are not relabeled as successful workflows.

## Practical effect and limits

The original Unity identities/settings now travel with the repository instead of
existing only on one computer. Future missing or duplicated identities are detected
by CI. Archival keeps the rejected model out of new Unity imports and builds.

The native proof certifies metadata recovery and the tested game behavior. It does
not grant artistic approval or physical-mobile certification. The old vendor
FPSController example still has a missing-script warning, and legacy shader/import
warnings remain documented; this is not a claim that every third-party example is
clean. No external publication was requested or performed.

Original provenance and archived bytes: recovery-manifest.json and LEGACY_ARCHIVE.json.
Earlier reference/portability scans remain explicitly pinned to their source commits.

## Post-merge evidence and remaining administrative state

- Unity run 41850579988 completed successfully on the exact merge commit:
  57 EditMode + 73 PlayMode cases passed, zero failed/skipped, Windows build and
  benchmark capture succeeded. Downloaded test ZIP SHA256 and XML were verified.
- UI run 41850579678: all executed steps, including Complete job, succeeded by
  2026-11-04T15:50:37Z. Logs report 57 EditMode and 73 PlayMode passed. The downloaded
  shared-native artifact independently verifies exact requested/compiled merge SHA,
  73/73 PlayMode cases with no failures/skips, and twelve actual PNG files.
- The optional retention reference/discovery probe was SKIPPED by the existing
  workflow; it is not counted as a passed probe or an artistic approval.
- Despite completed steps and uploaded evidence, GitHub's run, job and commit-check
  endpoints still report in_progress with null conclusion. The cause is unconfirmed.
  No job cancellation, runner restart, fabricated status or extra native run was used.
- Final administrative closure must reread that run and inspect its actual conclusion;
  if successful, close this same workstream. If failure, diagnose its final logs.
  This is an external reporting discrepancy, not evidence of a failed Unity test.

## Resumen práctico

Se recuperaron 345 archivos originales que identifican y configuran los recursos
para Unity, se retiraron 57 copias redundantes y se archivó el modelo descartado
conservando sus originales. Los controles automáticos detectan futuras pérdidas
o duplicaciones. Esto reduce inconsistencias al descargar el proyecto en otro
ordenador y al integrar nuevos trabajos. No añade mecánicas ni certifica la calidad
visual de todo el juego. La reparación está integrada y probada; queda la confirmación
administrativa final de GitHub descrita arriba.
