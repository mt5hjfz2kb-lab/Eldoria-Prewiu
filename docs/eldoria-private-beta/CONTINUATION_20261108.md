# PR179 continuation — 2026-11-08

Status: NOT RESOLVED; mobile6 owner's physical failure remains authoritative. No new public candidate or tester invitation in this continuation.

## Execution repairs
Previous native run42503628688 (03ca3f8) failed BEFORE Unity: local shared clone did not contain frozen commit948ffa9837168a0b68dab80be76b66350422b23a. Log128801504902 records the missing object. It was not cancelled.
Code review also found clean-clone workflow generated its own empty input audit but run_warm_structural.ps1 ignored it and loaded the prior147-file dirty-cache inventory. Empty Compare-Object calls were unsafe.

Commits009fdf1/b38a256/df627fd/38ba780:
- Explicit input-audit path; original dirty-cache validation preserved as default.
- Empty clean arrays accepted without weakening changed-path/hash rejection.
- Seven production-prefix regression cases pass in PowerShell7.4.13 locally and hosted Ubuntu.
- Frozen source now fetched from GitHub into a unique isolated folder; never reset shared warm project.
- Initial clean Unity import has a bounded60-minute allowance; other phases retain their deadlines.
Native run42505611257 targets38ba780d3311315914f9e39500c7e1f68ac81703. Preflight PASS; source checkout/Unity outcome must be retrieved live. No native certificate yet.

## Independent experiment while native runs
Run42506071704 targets5403c9bdfb6ec539cb890e22fef42467c34b55dc.
Exact terrain regression passed again. Same-machine Apple GPU WebKit26 comparison of published byte-identical mobile6 with unchanged allocation versus1024MiB allocated immediately after WASM instantiation, before Emscripten receives the instance. Only a QA init script changes allocation timing; neither game bytes nor public website is changed. Reports explicitly say desktop, not physical iPhone.
Rationale: Unity6000.3 memory documentation recommends initial heap near typical usage for mobile, and warns about growth failures. This is an experiment, not established phone cause.
https://docs.unity3d.com/6000.3/Documentation/Manual/webgl-memory.html

## Read-only payload review
Downloaded all30 live mobile6 decoded segments, checked every SHA256 and reconstructed501036611bytes. Inner493250926-byte bundle SHA256 equals e54d547487e0d915f5a8c3fbbd0354a79672041dd201b3e63672de5e740ea6c1.
UnityPy1.25.4 read851Mesh objects, serialized361295804bytes; readable359219828bytes. There are30 exact raw-mesh duplicate groups with27950072 redundant serialized bytes, NOT a measured runtime saving. No binary mesh modification made.
Source review confirms runtime reads/duplicates meshes for terrain, resource grounding and actor placement. Blanket readability removal remains rejected.
Previous public desktop report artifact13623376058 confirms resident memory exceeds WASM capacity: first ready used306012928B, WebContentRSS1771456KiB. The private15:11 Jetsam file is earlier than later failures and is not proof of game's termination. Do not publish that private log.

## Next
Retrieve both runs, inspect exact native phase reports and WebKit A/B before choosing another change. If native generated valid build but final restoration fails, preserve and inspect output before rebuilding; isolated imports may create metadata and must not be confused with shared-project corruption. No PASS from another source, no arbitrary memory/readability tweaks.
Canonical owner in main: codex-pr179-stability-2ea88ced1c46 revision13, same original workstream/PR179. Other projects/resources remain intact.

## Completed heap A/B — do not promote preallocation
Run42506071704 completed SUCCESS; artifact13627134593 ZIP SHA256 de289d0e47be726b45142d101a867bebc77252e132ce5d4281af75447bdec1be, downloaded and checked.
Both variants completed world initialization and90-second observation on WebKit26/Apple GPU with no captured runtime errors. Screenshots inspected: same name-entry screen and city composition; minor cursor/animation differences. This test did not perform nickname/reload and is not physical-device certification.
- Unchanged:21 growth calls, peak WASM1154023424B; peak sampled individual WebContentRSS2264832KiB (2211.75MiB).
- Preallocated1024:one early growth call, peak WASM1073741824B; peak WebContentRSS2420512KiB (2363.78125MiB).
The lower final resident size does not offset the higher startup peak for the current stability objective. Reject early1024MiB allocation as a demonstrated memory-peak improvement; no production change or additional phone retry requested. A single desktop A/B cannot identify the phone termination cause.
Native42505611257 progressed beyond exact-source checkout and clean audit into the baseline Unity probe (phase status observed). Await its actual results; do not report that as completed.

## Clean import failure and production-input recovery
Native42505611257 failed during baseline (before candidate): BASTION_III_MISSING_ART for Granary, stock and PrivateFissure. The frozen Git tree alone lacks the previously generated production files; existing-input-audit.json already records their exact SHA256. Its finally also detected first-import metadata/NatureStarterKit2 migrations and masked the baseline exception. Evidence13626974824 ZIP SHA256 a1dd01685b694fb51972e43d92101220bd053ce055de569902ad793a194bccdc inspected; no native PASS.
Commits0998daa/ca3eba4 recover only missing BastionIII files from shared warm project after verifying every hash against the existing production audit. Shared project is read-only. Reuse owned isolated import42505611257; reject unexpected source changes; preserve the first-import diff and hash inventory for both baseline and staged trials. No mesh/art redesign and no missing-art exceptions suppressed.
Run42507779126 has passed production-art recovery and entered the original-versus-staged Unity probe. Its final result remains pending; retrieve rather than assume.

## Native compilation correction and current chain
Run42507779126: original baseline completed two startup/reload passes, 2142 verified sites and17 validated cities, canonical saves unchanged. Candidate failed C# compilation: unqualified Application resolved to Eldoria.Application. Commit842aa5f qualifies UnityEngine.Application. Isolated input restoration passed exactly; input audit SHA256 DB0DC33D00DFA620AF5EA2F8B26557C999731F871F85A67A5985E4ECCE866AF0. Artifact13627928517 ZIP SHA256 b9987833e56ae311366539d151facfc2be51cadc1c76395151dad882dc364447 inspected.
Native42508059413 source6cf82e7bfb07a8bca7085ea5c3518fbb3c0f8a80 advanced past baseline/staged parity and canonical journey into WebGL build. Final reports, output and restoration remain pending; do not call the whole run passed.
Packaging42508475411 source239ae92da5aa3fe9fab04fbe3c18030d9a39c7bd independently verified the entire old deduplicated bundle versus published mobile6: only allowed texture/cubemap fields differ. New reuse_mobile6_profile.py applies only exact raw-source matches to fresh data and reopens/verifies every object. It has NOT yet processed a fresh build; waits for native success. Its Apple GPU comparison is public control versus local candidate, so network timing differs.
Additional QA workflow source097c4ce4c253c35eaaea47157f52d4d5198f6d5f waits for that exact package job, then serves both control and candidate locally on the same host/Apple GPU. This addresses transport timing as a memory-peak confound. No iPhone claim from either desktop test.
Read-only mobile6 event review: growth from412MiB through1100MiB occurs before REGION1_CANON_WORLD_INSTALLED; Aldric city import/ready messages follow. This does not prove phone termination cause, but supports measuring world initialization before blaming Aldric.
Apple Simulator was considered as an alternative physical-memory test; Apple documents that simulator memory/performance differs and macOS does not produce the same out-of-memory termination behavior. Do not substitute simulator PASS for physical iPhone evidence: https://developer.apple.com/documentation/xcode/gathering-information-about-memory-use
Existing public Sites project appgprj_6af0714ffbfc819187cc2b27651f3c08 version5 has been opened read-only for eventual shell replacement; audience remains public. No Site or Pages deployment in this continuation yet.


## Native output, recovery and browser results (18:56 UTC)
Native42508059413 compiled successfully and passed225 canonical checks, exact geometry/query parity,2142 sites and17 cities twice. Workflow failed only after build due two URP settings restoration. Artifact13629650525 SHA256 d761b4eb11b4e95b197cd02c024638e19d061ca1b4555dc1e0632a5334c36b27 inspected. Managed peaks fell78.62/63.59MiB; total native allocations did NOT consistently improve.
Export42511115716 recovered exact seven compiled files without Unity, source6cf82e7bfb07a8bca7085ea5c3518fbb3c0f8a80. Reviewed URP diff SHA256 b2fd708762e1abf446adcd16d8f3fc91bab3f54502710d623ba417e434644ec4 adds only existing SSAO resource reference; BakedSceneURP had no content diff. Restoration42511816427 passed: all115 original audited files restored, no shared-project/build modification. Artifact13629608178 SHA256 ce3ee5d884ca3079fc1d72ab52927fcd230f979e2f2ab02799a80247c350eff7 inspected. Wrapper now preserves both URP inputs automatically; seven regression checks passed.
Package42511382831 source04a3479ea465c4a5ef245abc25125f4fc25260ae applied168 exact approved texture transforms to fresh data. Candidate artifact13630155626 SHA256273b0119af4e8cce81fdd8f92d85d30e7d2a62b6d0da43558a741c2256236fa4. Desktop Apple GPU entry/reload PASS, but NOT accepted as memory improvement and NOT published.
Primary same-host QA42511461991 source2c683b9ab545d6719c89cf278424dc9b3652f8d2 artifact13630186586 SHA25677b0ad0582affdd034b8a08ad27685b3bc68e3d231ae4a50b277bc6edd2f3b5c inspected. Both90s startup, nickname and45s reload passed with no captured runtime failure. Peak sampled WebContent RSS: control2123552KiB; candidate2533744KiB. Peak WASM: control1052049408B; candidate1078525952B. Secondary different-transport comparison artifact13630067331 SHA256e25622259f5fdf3a6d6706a95036e667483d08340ddcde87e17a9351b466195c: control2281632KiB, candidate2299136KiB. No physical-device claim; no external testers.

## Next measured bottleneck and implementation under verification
Candidate actual WebKit metrics:21.58s used WASM472558584B;26.09s931490280B. Between these samples, temporary MeshCollider cooking logs huge-terrain warning during RegionOneResourcePlacement.Scan. This localizes a plausible allocation source, not proof of phone termination. Existing contact index already contains the same imported vertices/triangles and spatial bins.
New exact downward-triangle queries reuse that index and eliminate the temporary terrain MeshCollider from the player. No geometry/rendered art, masks, timers or saves changed. Native Editor-only validation compares every actual city/resource/route query against previous PhysX collider: hit equality, height/normal limits and both slope acceptance thresholds. Additional baseline/staged topology hash compares exact site IDs/types and traversable grid-cell membership; two reloads plus canonical225 checks remain required. All these new results are PENDING until the new run completes. Do not publish/claim improvement on implementation alone.
Unity documentation confirms LZ4 is the Web player archive default. Inspected shipped mobile6 already has LZ4 flags194/2; do not repeat an unsupported 'enable compression' fix. Mesh readability is still required for grounding; no blanket removal.
