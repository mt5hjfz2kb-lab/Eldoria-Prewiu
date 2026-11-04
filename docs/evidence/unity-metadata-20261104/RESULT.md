# Unity metadata recovery — 4 November 2026

Status: **candidate in PR #153; not integrated or published**.

## What was found

The original count of 279 versioned files without metadata contains 277 assets
that need metadata and two internal files in an Android bundled plug-in. Unity
treats that bundle as one plug-in. A subsequent directory audit found 69 missing
folder identities. Three apparent orphan metadata files resolve to two legitimate
empty-folder identities and one metadata file whose `.unitypackage` payload is
absent from Git.

`Unity/.gitignore` excluded metadata throughout `ArtTests`. That directory also
contains integrated production. This candidate removes the three broad metadata
exclusions, retaining cache and generated-output exclusions.

No original for the 277 file metadata paths was found in the complete ancestry of
`main` or the inspected PR #136 tree. This is not proof that no other branch or
external backup ever held them. Existing runner projects provided recovery copies.

## Recovery and changes

| Item | Restored in Unity candidate | Preserved pending active owner |
| --- | ---: | ---: |
| File metadata | 277 | 0 |
| Folder metadata | 68 | 0 |
| Total original metadata | 345 | 0 |

One additional folder, `Resources/Valoria/HeroBastionGenerated`, was absent from
the inspected runner copies. Its identity was not invented. The folder belongs to
the rejected legacy Bastion area: do not recreate or reactivate that model as a
metadata repair.

The unused `URP_Material.unitypackage.meta` is archived byte for byte outside
Unity. Its GUID occurs in none of the 3,617 inspected versioned Unity YAML files.
The valid empty-folder metadata remain in place; after archiving the orphan payload metadata, the audit preserves three empty-folder identities.

All 277 file recovery records match the expected Git asset blob, with exact
newline conversion verification for the two shaders. Folder records separately
state that stable identity was recovered without proving equality of all folder
contents. Two runner projects had different identities; the primary Unity project
retained complete importer settings, whereas many secondary construction scratch
metadata files were GUID-only stubs. The selected originals come from the primary
project and are retained unchanged.

Provenance is in [recovery-manifest.json](recovery-manifest.json): asset blob,
original metadata SHA-256, GUID, destination, disposition, owner dependency, native
snapshot run and artifact digest. The 57 formerly reserved originals have now been restored after explicit owner priority authorization; byte-identical backup copies remain under `pending/Unity/`.
The snapshot read existing files only; it did not open Unity or change the PC's
projects. Original archive digests were checked before extraction and use.

## Verification and limits

- Recovery, integrity, ignore rules and exact known-debt behavior have 21 focused
  tests. Workflow governance passes.
- Every recovered or archived metadata file is checked against its original
  SHA-256. Existing tracked metadata GUIDs are not rewritten; asset source contents
  are not edited. The one orphan is explicitly archived, not silently discarded.
- No duplicate GUIDs or metadata-excluding ignore rules remain in this candidate.
- A read-only portability inventory of 21,211 versioned paths found no case-insensitive file collisions or directory variants, no repository-relative paths over 240 characters, and a maximum relative length of 173. This does not certify all Windows paths after checkout or package-cache expansion.
- All 345 restored identities have no references in the 3,617 inspected versioned
  Unity YAML files. Dynamic Resources loads remain relevant; preserving original
  importer bytes avoids silently resetting their import settings.
- The preventive audit now expects zero missing file metadata and records only the one unrecovered rejected legacy folder as known debt.
- Native run 41816327312 compiled/imported candidate 6c02784fc6fca56e8fdf54edd78e25a77c9cff3e and recognized all 345 expected GUIDs. The overall validation FAILED: 345 raw-byte checks lacked a pre-import checkpoint and 11 scenes were probed with an inappropriate sub-asset API. A real Git reproduction demonstrates CRLF checkout conversion despite autocrlf=false; the corrected harness pins core.eol=lf and verifies original bytes BEFORE Unity, and uses typed SceneAsset references. No full native PASS or PlayMode certification is asserted. No player build, visual approval or device certification is claimed.

## Remaining integration

The owner explicitly authorized metadata priority on 4 November. Dialogue/spawnling
write reservations are temporarily paused with their exact prior states retained
in the live metadata workstream. Construction run 41811262373 finished with failure
before Windows transferred; no running job was cancelled. All 57 pending paths were
reconciled against exact main and PR136 heads in RESUME_RECONCILIATION.json: all 32
file asset blobs match; folder checks prove existence, not full tree equality.

Native validation uses an isolated exact checkout and unique
company/product save identity, original metadata GUID and importer-byte checks,
enabled-scene missing-script checks, full EditMode/PlayMode, exact settings restore.
The baseline runtime still loads/instantiates the rejected legacy Resource at Bastion III+, and the main Unity workflow builds a Player. Full scene/runtime validation and integration are therefore fenced while that owner policy is unresolved. A byte-preserving archive-only proposal is prepared separately (LEGACY_ARCHIVE_PROPOSAL.json, commit c2dab945bf1d3994703abfbbd2cb137d67e85443); it is NOT applied to this branch or main. Its static audit has zero missing metadata/folders and zero failures without inventing a folder GUID. The authorized metadata proof can continue in identities_only mode, with no scene opening or runtime tests.

The complete previous snapshots are recoverable through Actions runs
41805832811 and 41806653964; artifacts are temporary. The metadata needed for this
candidate and its owner handoff is preserved in Git rather than relying on that
artifact retention period.

## Additional baseline findings from native import

The actual import log also reports a missing script while upgrading the third-party
`Mega Fantasy Props Pack/Example Scenes/Prefabs/FPSController.prefab`, plus legacy
shader fallback warnings. Those paths are outside this metadata recovery and have
not been silently changed. Zero missing scripts in the one enabled scene does not
mean all vendor examples are healthy. The exact raw native report is retained in
NATIVE_41816327312.json; native attempt causes and rejected techniques are recorded
in NATIVE_ATTEMPTS.json. New metadata-only validation remains pending.
