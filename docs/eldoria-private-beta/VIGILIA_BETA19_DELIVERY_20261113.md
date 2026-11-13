# Integrated Vigilia delivery

The owner authorized integration and beta publication. PR179 merged as
`f1c8dd673ee74072d4bcf646da1413cd551ebaf6`, retaining current main
`874eb50be38a181735414cf629a5fc92ee462b89` and exact candidate
`fb8727d3ac45e31e61ab8ba244fa395477eff16e` as parents. Canonical PR173,
PR181 and PR187 are now merged too.

## Verified source and artifacts

- Integrated native/WebGL/package/WebKit run43329543960 succeeded.
- Full certificate run43329553450 succeeded: 218/218 EditMode, 79/79 PlayMode.
- Terrain replay: two passes of119870 queries,2142 sites and17 cities; geometry,
  queries, routes and saved bytes preserved. Native I–V journey passed.
- Native proof13995963431 SHA256
  `0c4ecb3a05f3aa71f2947affe1e3ad4c3a2a1cc5f06b4f7505bc8d1396f75d87`.
- UI proof13995488869 SHA256
  `05b512d3f43612ab016b4e51873dfc0e0e4d7b01f7b4526b6d5922f8c1cfa11e`.
- Packaged candidate13995794244 SHA256
  `34b1177ff9d98d1feb5c97a041827dc5c337f0ceb15c73049522efc8afb44849`.
- Packaging proof13997156118 SHA256
  `f82c4ec39ee346d430f6452130951b4d1a638e54098393816f5c7ce698be134e`.
- Desktop WebKit13998070186 SHA256
  `67910a05daa2d77f07b452ea205c4213fcd5f81853de7929bcb6e1df17d935a7`.

These five archives were downloaded and their hashes independently checked.
Actual WebKit screenshots show the entered name Beta QA before and after reload.
Vigilia installed twice; no browser errors. Peak WebContent RSS was2710256 KiB
for beta18 and2818464 KiB for the candidate. Desktop evidence is not iPhone
certification. Exploradores and Vigilantes remain visually provisional.

## Publication completed — owner beta19

The candidate contains531369638 decoded data bytes and its packaged ZIP is
480134834 bytes. Its bundle SHA256 is
`eb9192488a6cc34dd38309f48cdd7fdb3dc3ea5a2d6fb7dc23a023d8e23b0048`.
All57654 remaining objects were verified after exact deduplication and reuse
of157 byte-matched, previously shipped mobile texture transformations.

The original copy/add strategy cannot compact this new bundle adequately:
measurement found516292608 literal bytes,442422519 gzip bytes, despite only
17720962 more decoded bytes than beta14. Keeping all historical builds would
exceed Pages capacity. Do not relax the original980000000-byte staging budget.

The prepared publication instead retains beta14 and beta18 byte for byte,
adds the new `beta-integrated-vigilia` route, and archives beta10 plus the old
clock diagnostic. Their entry URLs become explicit archive notices linking
to the stable and new betas. Before any deployment, the complete original
Pages tar must be uploaded to the draft release
`archive-pages-before-vigilia-20261113`, downloaded again and compared exactly.
The original artifact is13975981075 from run43286895185; ZIP SHA256
`beeb0964b468b433c7c7d01668d884df43cc3221834fb10ec17c61b557327b50`.

The new route has separate URL-based WebGL storage; beta18 and its existing
player storage are preserved. This is an owner test candidate, not an automatic
player-save migration or an external-tester release.

The delivery workflow tests the exact staged route in WebKit before deployment,
requires durable archive verification, checks unchanged public stable files,
then verifies the live identity and repeats WebKit on the actual public route.
Physical iPhone and final art approval remain false. Publication run43338146206
completed successfully, including actual public WebKit. This is an owner-only
candidate, not an external-tester release.

## Main validation ownership

The same integration workstream moves to main at ownership revision65 for
postmerge validation and delivery. The main UI certificate accepts that exact
postmerge phase and keeps every native test. The old main invocation43337524782
uses the premerge branch-only lease rule and cannot enter Unity. Delivery may
cancel only that exact invocation while queued or in its first lease wait;
it must refuse cancellation if any later step has started. No live Editor
may be cancelled. A fresh main certificate runs with the updated lease rule.

## Verified public delivery — 2026-11-13

Public URL: https://mt5hjfz2kb-lab.github.io/Eldoria-Prewiu/beta-integrated-vigilia/?eldoria_p0_lazy_world=1&eldoria_diag=1

- Publication run43338146206: stage, staged-webkit, archive, publish and
  public-webkit all SUCCESS.
- Combined site961106461 bytes;96 protected files unchanged,95 historical files
  archived and46 new candidate files. The980000000-byte budget was retained.
- Complete original Pages tar971489280 bytes; SHA256
  `c8b709509a23c88ac03fa79d150ec8707fb6d7037aef848a2952fd376994b6f8`.
  Archive job131601859304 uploaded the durable draft release
  `archive-pages-before-vigilia-20261113`, downloaded it again and compared
  all bytes before deployment. Archive proof13997753212.
- Deployment job131602581278 succeeded, including verification of live candidate
  identity, stable identities/manifests/HTML and staged file proof.
- Public WebKit job131603503347 succeeded. Artifact13999166130 was downloaded
  independently and SHA256 verified:
  `f2b7e05c05ce287e1175a55fb0131b0e42d806926ec97caab4b1d69e38858a1b`.
  Actual report: passed=true, errors=[], two Vigilia production installations,
  early nickname entry, four viewport resizes, actual Quit/reload and45 seconds
  of observation after reload. The genuine public reloaded screenshot was
  inspected: Beta QA remains visible with the new interface.
- Staged-route WebKit also passed; artifact13997902911 SHA256
  `6cb14911e1e8e1dcf49bf7bb6ffe1a0bb7eda57a42c094b4969665705dc68d70`.
  Its genuine reloaded screenshot was also inspected.
- Beta18 remains at its original URL with existing save storage. The new route
  uses separate WebGL storage: no automatic progress migration is claimed.

The immutable packaged release-identity.json retains pre-publication flags
published=false/runtime_verified=false. Those describe the original package
stage. The subsequent deployment run and public WebKit evidence above record
publication; no build identity bytes were rewritten to manufacture a PASS.

## Postmerge native results and unresolved QA

Main merge f1c8dd673ee74072d4bcf646da1413cd551ebaf6 passed run43337524622,
including its actual Windows desktop build. Independently downloaded test
artifact13998757786, SHA256
`fdb2c38c055574d276cdac7a5c7a6a27bb7192aa4b16e66382f10d165835dbc7`,
contains native XML reporting218/218 EditMode and79/79 PlayMode, no failures,
no skipped tests. PlayMode ran641.77seconds.

Additional main UI certification43338146170 FAILED; do not describe main as
fully green. Job131602641597 compiled successfully, then the legacy V4.1
fixture failed at EldoriaV41NativeCapture.cs:1095:
Navigation.Items.First(i => i.Section == RoyalSection.Inventory).
The fixture describes this as the old CONSTRUIR shortcut and throws
InvalidOperationException: Sequence contains no matching element.
The later retention artifact upload also failed because its prerequisite
capture was skipped. Native Editor cleanup succeeded; no live Unity was killed.
Failure evidence artifact13998689779 and the job logs remain available.

Investigated, NOT IMPLEMENTED: the main UI workflow still invokes legacy
navigation/visual fixtures. Adapt the applicable checks to the current
production contract, retaining meaningful navigation, persistence and economy
assertions and using the existing Vigilia native fixtures where appropriate.
Do not merely skip the failing assertion or restore retired art in the player.
Confirm the precise navigation contract before changing the fixture: the
exception proves a missing expected entry, not an iPhone crash or an automatic
gameplay defect. No speculative test correction was committed.

The local execution environment became unavailable during this diagnosis
(exec_command returned "No usable execution environment is available").
GitHub connector reads/writes remained available. No additional native repair
run was launched without a reviewed correction. All owned native jobs are
completed; the Windows reservation is released while source ownership is
retained for this same task, as permitted by AGENTS.md.

Next autonomous action: recover the execution environment, update the old
native fixture against the actual navigation contract, run the corrected main
certification, inspect its outputs and update this record. Do not rebuild or
republish unchanged beta19 solely for an Editor-only QA repair.

Remaining product verification: beta19 on a physical iPhone; final artistic
acceptance (especially Exploradores and Vigilantes); external tester release.
Beta18 was owner-reported stable, which does not certify beta19.

Other unrelated draft PRs remain open (#127, #128, #151, #152, #184).
This delivery reconciles #162/#173/#179/#181/#187, not every experimental task.
