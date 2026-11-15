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
