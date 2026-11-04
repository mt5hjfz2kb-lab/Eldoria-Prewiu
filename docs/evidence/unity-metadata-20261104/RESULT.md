# Unity metadata recovery — 4 November 2026

Status: candidate in PR153; not merged or published. Native original-identity/byte verification and 57 EditMode tests PASS; complete PlayMode proof still pending.

## Implemented in this candidate

- Restored 277 original file metadata and 68 original folder metadata: **345**
  recovered identities. All 57 previously reserved originals are now restored.
- Removed the three broad ArtTests metadata exclusions from Unity/.gitignore.
- Archived the orphan URP_Material.unitypackage.meta outside Unity, retaining its
  exact bytes. Preserved three legitimate empty-folder identities.
- The original 279-file count included two Android bundled-plugin internals that
  correctly do not require individual metadata. They are not hidden debt.
- Enforced the existing owner veto by moving the rejected legacy Bastion GLB and
  its existing metadata to ArtSource/RejectedLegacyBastion. Both original Git blobs,
  fresh SHA-256 hashes and GUID are unchanged. No runtime code, scenes, geometry,
  textures, replacement art, gameplay settings or publication were added.
- Updated only its two blocked catalog locators; OWNER_BLOCKED_NO_USE and the
  prohibition on ordinary reuse remain. Real ordinary reuse search excludes it.
- The rejected source folder no longer exists inside Assets. Its original folder
  identity was never recovered or invented; archival resolves that obligation.
- Removed the 57 redundant pending copies after byte equality with their restored
  destinations was proved. Their original pending paths remain recoverable in
  commit 4e275f844f4306419c87f37b12b18b0520a372dc and in manifest provenance.

The archive README describes the Python/native safeguards actually implemented
here; the separate PR136 C# build guard and dialogue changes are not claimed as
integrated. No other branch or PR source was overwritten.

## Provenance and integrity

Original metadata came from stable read-only snapshots of existing runner projects
(runs 41805832811 and 41806653964). All 277 file asset Git blobs matched; only the
two explicit shader text cases required an exact CRLF-to-LF checkout equivalence.
Folder records prove stable existing identity, not full directory-tree equality.
The selected primary project retained full import settings; secondary projects
had differing GUIDs and often GUID-only stubs, so were not substituted.

recovery-manifest.json records original metadata SHA-256, GUID, asset/tree identity,
destination and provenance. The rejected source archive retains asset blob
e31aa6a63af826f4c6f666bb369fc270233c8d66, metadata blob
bab32c587d4bd8c194a242304ae97dd937bcc29b and GUID
fa36c6abf91adc7778caeac12130f567. The 13,257,704-byte model's freshly verified SHA-256
is afb6cee6ae572b0879650f18285b32798e263c17359a158ffe2bdd03fb62ad5c.
Structural parsing confirms preservation only, not artistic approval.

Static integrity expects **zero** missing file/folder metadata, zero duplicate
GUIDs, zero file orphans and zero metadata ignore regressions. It also rejects the
forbidden Resource's return and checks both archived Git blobs without requiring
large model downloads. The metadata/recovery suite has **23 passing tests**;
the existing asset-vault utility suite has **10 passing tests**.

All 345 recovered identities, and separately the rejected model GUID, have no
references in the 3,617 verified serialized Assets YAML files. Exact candidate
C# contents (448 files) were inspected: the one historical runtime Resource
caller returns when that model is absent. Full runtime tests must still verify
that fallback; static findings are not a gameplay acceptance claim.

The pinned portability scan covered 21,211 earlier-candidate paths: no casefold
file/directory collisions, no relative paths over 240 characters, maximum 173.
That does not certify Windows absolute paths or later package cache paths.

## Actual native attempts and corrections

1. Run41814006124 stopped before compilation: Unity Package Manager could not
   locate Git. The existing Git directory is now inherited through PATH.
2. Run41816079304 stopped before Editor: the optional temporary cache had been
   removed by runner cleanup. That unsupported approach was abandoned; each run
   uses an independent canonical clone.
3. Run41816327312 compiled/imported and recognized **345/345 expected GUIDs**.
   One enabled scene had zero missing scripts. The overall result was **FAIL**:
   all raw-byte checks differed without a pre-import checkpoint, and eleven
   scenes were incorrectly probed as sub-asset arrays. EditMode/PlayMode did not
   run. The immutable native report is NATIVE_41816327312.json.

A real Git reproduction proves that text=auto plus Windows/native core.eol can
convert LF to CRLF even with autocrlf=false. The corrected checkout pins BOTH
core.autocrlf=false and core.eol=lf, and verifies all original SHA-256 values
BEFORE Unity starts. Any genuine later differences are captured as raw metadata.
Scene references now use the documented typed SceneAsset API without instantiation.
Both validation modes are explicit: identities_only never opens scenes/runs tests;
full checks enabled scenes and EditMode/PlayMode, refusing the rejected Resource.

The initial import also logged a missing script in the third-party example
Mega Fantasy Props Pack/Example Scenes/Prefabs/FPSController.prefab and legacy
shader fallback warnings. Those unrelated vendor files have not been silently
changed. Zero missing scripts in one enabled scene is not a clean bill for every
vendor example. Record any resulting native failure rather than suppressing it.

## Coordination and remaining verification

The owner authorized metadata priority and a temporary dialogue/spawnling writing
pause. Exact latest prior owners/states were preserved for restoration. Construction
run41811262373 had already finished before Windows transferred; no running native
job was cancelled. The temporary pause uses the registry's valid blocked status,
not an unsupported paused status. Native revision fencing prevented further starts
of stale requests while the rejected-resource policy was reconciled.

Next: actual full corrected native proof on the exact archived candidate, review
all output and tracked changes, pass relevant CI, then integrate safely and restore
or release temporary claims/resources. The main push may run its normal Windows
Editor/player gate only after the rejected source is absent. No PASS, merge,
player build, mobile certification or visual approval is claimed in advance.

## Native checkpoint 41821882671

Actual Unity6000.3.23f1 import now verifies all 345 recovered GUIDs AND original
raw metadata bytes, with a successful pre-import checkpoint and no post-import
byte differences. The one enabled scene has zero missing scripts. EditMode:
57 passed, zero failed/skipped. PlayMode: 40 passed, 24 failed, 9 skipped; this
is NOT a full native PASS. Twenty-three failures report missing build-safe
BastionIII Granary art, because the custom harness omitted the preparation already
required by unity-slice.yml. Other omissions require the canonical disposable
probe/capture fixture. Both setup defects are corrected in the next harness;
real full results, source assembly identity and zero skips remain mandatory.

## Native checkpoint 41832431346

Unity6000.3.23f1 passed all 345 restored GUIDs/original bytes, 57 EditMode and
73 PlayMode cases with zero failures or skips. Canonical preparation completed,
and actual Windows redirected-process exit0/exit7 checks passed. The overall
workflow nevertheless FAILED its blanket clean-checkout assertion: 61 older
baseline files were serialized by import/canonical fixtures. Their source blobs
match unchanged main; no recovered entry changed. The exact binary/text patch
was reconstructed and all 27 other metadata GUIDs remain unchanged. See the
checkpoint JSON and exact serialization ledger. No side effects are committed.
The next run rejects every unknown path or differing before/after blob and
rechecks all restored raw bytes after tests. Full certificate remains pending.
