# Physical 21e1 failure: reopen, measure, isolate CPU geometry retention

Status: **PHYSICAL_IPHONE_FAIL; ROOT_CAUSE_UNCONFIRMED**. Same PR194 and
workstream `eldoria-private-beta-i-v-20261107`, owner revision84. No tester release.

The owner's latest persistent journal reaches `render-budget-ready`,
`world-entered` and `world-frame-2`, then records a new `boot back_forward`.
Entry1794748220528 -> newboot1794748225194 =4666ms. Requested1794748212790,
render budget ready1794748213862, entry7.738s after request. The previous MSAA
mitigation was active and **did not fix the physical failure**. This is not
proof of OOM, GPU fault, JS failure or navigation. No missing lifecycle event
is treated as a diagnosis. A loading overlay alone cannot address persistent
post-entry resource costs.

## Observed on the exact published engine

Native runtime ca5e1c3d52a58d8c6d9c9a9114d2d35445a18029. Delivered bundle
4eb972ac2f97aa1377950180504a5fefeec4ebd4d0c58c358601cbe9ae01a99d.
Read-only GPU ledger workflow65032676188a6072e34500308ade8624e6cd24a3,
run43718333142, artifact14164065722, ZIP SHA256
03fa37b08d7d0cb1308d7fbc0dae1fd43745456750148b8ac12357e7a9a4b6e9.
The original public certificate was checked before and after the run.

- Earned construction, saved nickname, reload, two world entries/returns and zoom
  journey passed in desktop WebKit, with no runtime errors or observed context loss.
- Before world: WebGL buffer allocations200582154 bytes,1508 live buffers.
  First-visit sampled maximum244849202 bytes. This is NOT a physical GPU memory measurement.
- Used WASM rises393254024 ->559044688 bytes; capacity518782976 ->622526464.
  This corroborates the prior uninstrumented ~375 ->531MiB used-heap trajectory.
- The ledger performs GL queries and visibly perturbs timing. Its FPS and process
  residency cannot certify normal performance. Explicit deletes only are counted;
  GC-only disposal is not observable. Observer errors0.
- Before activation, native memory inventory includes187372054 mesh bytes already
  present in city, then222738034 bytes after world construction. Four static
  Bastion mesh objects alone account for about87MB in that Unity inventory.

## Rejected explanation and bounded alternative

The delivered creature assets were inspected, rather than inferred from GLB names.
RockBoar and Spawnling near/far textures already use1024px authored mip chains.
Each creature's retained normal is5592404 bytes; albedo and ORM1398128 each.
Near/far share most texture objects. They do not independently explain the whole
~156MiB used-heap increase. No new blanket texture reduction is justified.

Next controlled experiment: disable CPU Read/Write retention on exactly four
static Bastion mesh objects in an isolated copy of the delivered data. Preserve
all vertex/index, material, texture, collision, scene, code and save bytes. The
published game is unchanged. These are neither terrain-query meshes, deformed
resource meshes, animated hero meshes nor creature fitting sources. Runtime
FitNearWidth callers target resources/creatures; building portraits consume
renderers and bounds. Known static collider uses have default cooking options30,
non-convex shapes and baked triangle data; scene transforms are positive without
shear. The trial script verifies the actual serialized consumers and fails closed
if these assumptions drift.

`prepare_static_mesh_residency_trial.py` must prove exactly four 1->0 byte changes,
then reload the bundle and independently hash **every serialized object**. No
geometry or collider payload may change. The trial is explicitly not a native
source build certificate. Native source implementation, full Bastion progression,
visual review and physical acceptance remain pending. A desktop memory improvement
alone will not be reported as fixing iPhone.

Local serialized-integrity gate PASS:57656 objects reloaded and checked; exactly
four bytes changed. Candidate bundle1bdee526eb59ec6592393b2403af78ee2a223fbc889c9d7032608e44d387463b.
Runtime measurement not yet performed at commit time.

Native implementation prepared behind `eldoria_p0_static_mesh_cpu=1`: retire the
same four static mesh CPU copies at initialized city startup using UploadMeshData.
Before any mutation it checks the complete cohort, skinning consumers and collider
cooking/transform constraints. It preserves bounds, vertex count and submesh index
counts and logs measured Unity memory before/after. Real PlayMode fixture now checks
non-readability before Region1 allocation and actual active-collider ray hits,
then runs existing cancellation/rejoin,12 entries,48 resources36 creatures and save
reload. Source UX parity gates15/15 pass. **Native compilation/runtime proof pending**;
this source is not a publication or iPhone fix claim. Same-engine serialized trial
run43720075089 is still running at this source checkpoint.

## Same-engine experiment result (independently downloaded and verified)

Run43720075089 completed SUCCESS. Artifact14164493498, ZIP
50435e11e1f45e7a4707f62b374c133cb714a540faf3ebab0dfe43b7ea571f51.
Both real browser journeys passed construction, nickname/reload, two world entries,
zooms, city return and persistent journal, zero runtime errors. No GL observer used.

| Metric | Control21e1 | Four-byte candidate |
|---|---:|---:|
| Peak sampled used WASM bytes |559923120|471715584|
| Maximum WASM capacity bytes |622592000|622592000|
| Mesh inventory bytes reported by Unity |222738034|222738034|
| Peak sampled combined WebContent+GPU KiB |2911520|2589952|
| Observed first entry wait ms |11817|17002|

Used-WASM reduction88207536 bytes =84.12MiB. The capacity did NOT shrink and the
Unity mesh-inventory number did NOT change; neither is substituted for the actual
used-heap observation. Combined process RSS is lower in this paired run, not a
physical-iOS measurement or a general peak bound. The slower first entry is retained
as an observed tradeoff/uncertainty; no startup-speed improvement is claimed.

Inspected control/candidate actual world entry, overview, close zoom and city-return
screens: no missing Bastion geometry/roof, terrain, resources or UI observed. The
camera trajectories are input-driven, not a pixel-identical offline render; no
artistic score or physical stability verdict inferred.

The predefined40MiB used-WASM threshold for a native validation cycle is met. Native
runtime UploadMeshData implementation is NOT presumed equivalent to an import-time
flag; it requires its own measured build comparison. Source05e184 remains pending
native compilation and verification. Current Windows holder is the separate
stylized-direction workstream; its run43721967300 is not preempted. P0 resource
request recorded in main, own resources remain empty.

## Recovered original build inputs — 2026-11-15

Native run43734224654 reconstructed exact source05e184 but stopped before Unity:
the original untracked BastionIII inputs were absent from all prior checkout paths.
This is a build-input recovery failure, not a new game-runtime finding.

Recovered original archive artifact14121008070 from run43613603217. Inner ZIP
SHA2562df938c208227f5808f31499268e04cb9b6532a7f1634aec1c5c20ecdc54324e.
All51 eligible files independently matched existing-input-audit.json byte hashes.
Only these exact51 entries are restored, never the remaining historical overlay.
Terrain source recovered from artifact14074420151, run43489752868; outer ZIP
SHA2560454b10c9b88113f65fac719a8a74e54ad1f6079ca2ff2d370be1a1abe488176.
The existing terrain byte and GUID hashes remain mandatory in the native script.

Run43739391559 passed hosted preflight and both exact artifact downloads, then
entered native certification on Windows. At this checkpoint it is still running;
no NUnit/native campaign/WebGL success or physical-iPhone stability is claimed.
The successful same-engine serialized experiment remains distinct from pending
compiled runtime UploadMeshData verification. No replacement has been published.

## Exact native certification completed

Run43739391559 (workflow79c74ce6bb26c4903f19686b900851a508957bce) completed
preflight, Windows native certification, WebGL compile and Ubuntu packaging.
Frozen runtime source05e1846f0213c6320850505bc054110c53934f55.
Native evidence artifact14173273151 SHA256
1745e646e5c7478564ce64a8a53c58e2f1ac28039c18683d8db53504c7c5c02d
was downloaded, independently hashed and inspected. Actual NUnit result1/1passed,
0failed/0skipped; marker proves12world reentries completed. Four eligible meshes
retired CPU readability with geometry preserved; real collider checks passed.
Editor mesh inventory before/after both174154848bytes: NOT a measured browser
CPU-memory reduction. No physical-iPhone stability claim follows from this.

Reviewed six actual Unity close/regional/panoramic portrait/landscape views plus
city/barracks,march,heroes,mail captures. No missing static models/roofs observed.
Native saved I-V campaign completed with exact final save. This is regression
review, not approval of overall art quality.

Packaged site artifact14173109200 SHA256
5eae51319416a2df8ed6e8cf519296923fc3a18b6517895399d356effd7dcfb7
was independently verified. Bundle8fd5fd1c37a0b30b4b23d2a9af3e6a2bd7cf682eef3e53d6732e74b2298e77f2;
delivered data07aedd85979e3f06ccea19c1fcd6a2bb7c0793ce8e7b6dbdabb54a6f5ef536a3.
Bounded journal21e2 playSHA99f5fe51df8c611833f340e6a8baca52dc9eb51501eec28582a2242229afc665.
Windows completed own generated-cache cleanup and reported403.99GiB free, then
resource was released in main. Browser comparison43744175007 at668b3a33ce3bf7d9965a82c48f7f53c79c107f8b
is pending.11local publication/restoration unit checks passed. Public21e1 unchanged.

## Compiled same-engine browser comparison passed

Exact-source run43744175007 completed all three journeys: CPU-retirement ON,
OFF, and original early-entry/resize/reload residency observations.0errors.
Artifact14174252792 SHA25602bd4f66e01432be831df4c659fb7557a825412ef5f689bb1635813b057aacc8
was independently downloaded, hashed, verified and visually reviewed.
Used WASM peaks: ON471787904bytes vs OFF559859592bytes, reduction88071688bytes
(about84MiB). Capacity did NOT decrease:622592000 vs622526464bytes.
Peak sampled combined WebContent+GPU RSS was higher in this pair:
ON2786640KiB vs OFF2687312KiB. Therefore no total-process/iOS-memory reduction
or physical-crash solution is claimed. First entry16.178s vs15.442s; no speed claim.
Both actual saved-game world journeys, real zooms, city returns and original
residency/reload observations passed. Reviewed world/zoom/city pairs and early
nickname/reload screenshots; geometry preserved, saved name visibly retained.
The bounded journal exported all five actual numeric memory samples.

## Owner21e2 publication and verification status

Publication43747367995 deployed source05e184 successfully at18:34:29UTC.
Post-deploy immediate all-file check failed on decoded part15; a subsequent
independent download matched its exact expected SHA256 and15204292bytes.
Certificate62e296b8ae5b1f0c63596f10f3f27e936ed676bc6e5bae2567fd618d39433e4a
was verified against artifact14175225220, ZIP
365a7e2630523244fc84144ed5ddd8b149ff3f9f4689f627e48480708ba8ff5d.
Full rollback archive of21e1 is retained in draft release
archive-pages-before-world-static-mesh-21e2.
No claim of complete public verification until independent read-only job passes.
A redundant queued publication43747483098 correctly refused replacing the now-new
public certificate; it did not overwrite21e2. Read-only public verification has
bounded retries for propagation but still requires every exact byte hash plus
the complete public saved-game browser journey. Physical iPhone remains pending.
