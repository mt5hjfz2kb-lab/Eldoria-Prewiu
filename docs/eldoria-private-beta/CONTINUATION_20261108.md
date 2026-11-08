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
