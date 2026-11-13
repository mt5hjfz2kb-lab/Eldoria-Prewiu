# Physical beta6 failure — compact terrain investigation (2026-11-08)

## Status
NOT RESOLVED. Owner reports mobile6 behaves like mobile5. IMG_5052.jpeg shows beta6, ready at16:57:53 Europe/Madrid, capacity412MiB, last visible line grow-request-mib495 at16:57:55. No completed grow, exception or navigation is visible in that excerpt. Do not infer OS termination cause from heap capacity.

## Concrete independent work
Source audit found mandatory terrain validation allocates331431 Dictionary/List buckets for369163 exported source vertices. It runs in the post-ready startup interval. This is avoidable transient allocation; it is not yet established as the phone crash trigger.

RegionOneBlenderLandscape now uses a fixed-array spatial hash with collision checks. It preserves the floor(x*4) cell mapping, nine-cell neighborhood, strict squared2mm threshold, every original source index and full-coverage rejection. A boolean coverage array replaces HashSet allocation. The index is still released in finally. No geometry, art, tutorial, rules or validation requirement is removed.

The unpublished candidate source has opt-in ELDORIA_MEMORY_PHASE checkpoints before/after prefab loading/instantiation, verification index creation/release, contact construction and pigment fitting. On WebGL enabled only with eldoria_profile=1; native comparison uses ELDORIA_MEMORY_PHASES=1. No player data collection or gameplay mutation.

## Verified
Run42498715432, source d3959c29d2647ed8e1f9605d0fe3b11926b4855e; artifact13624567646, ZIP SHA256 db71a73aa7c8ea12220e95e50a82ddddd8ce8184097bd58e6452b4e959f936ac.
Exact production C# helper extracted from the source and compiled against .NET8 target. Fixture extracted from exact mobile6 bundle e54d547487e0d915f5a8c3fbbd0354a79672041dd201b3e63672de5e740ea6c1.
- All369163 authored vertices: identical matches and coverage.
- 24000 near-tolerance/rejected-offset queries: identical decisions and coverage.
- Cell boundaries, negative coordinates, duplicates and strict tolerance: identical.
- Removed source coverage: both detect369162/369163 covered.
- Index construction allocation: original76354672bytes, compact10101032bytes; reduction66253640bytes (~86.8%). This is a managed allocation microbenchmark, NOT total game RAM saving.
- Measured full-query execution193.13ms versus151.92ms on hosted Ubuntu; NOT iPhone performance certification.

Unrestricted mesh-readability stripping was rejected after audit: canonical Bastion meshes296/306 are referenced by MeshCollider, and world meshes have runtime vertex access. No such binary patch was made.

## Native boundary and exact next work
Fresh Unity compilation, original/candidate native geometry/query parity, canonical225-check journey and WebGL rebuild are prepared in eldoria-beta-native-memory.yml using preserved exact warm source. Native preflight42499212153 parsed PowerShell successfully but explicitly SKIPPED Unity: windows-runner-heavy is held by eldoria-forja-oscura-visual-proof-20261107 / chatgpt-vigilia-20261108 / revision21. Failure or completion of one UI job does not release that claim. No jobs were cancelled and no Unity source was overwritten.

Alternate hosted Unity licensing is unconfigured according to read-only preflight job128784833038. No substitute license or paid route assumed.
Request for Windows is recorded on the beta workstream. Once legitimately released: claim the same resource using fresh main SHA; synchronize expected lease/snapshot; run the guarded native workflow; inspect phase allocations, geometry/query parity and native journey; rebuild and reapply verified packaging changes by exact identity; test on Apple WebKit; publish only a verified candidate. Do not republish old mobile6 as a fix.

## Test runner correction
The optional full-source Roslyn syntax check initially selected an installed .NET10 SDK despite net8.0 target, producing CS1705. Added global.json selecting installed .NET8 feature band. This was a test-toolchain mismatch, not a Unity game exception. Run42499272127 PASSED: full instrumented source parsed as C#9, exact production helper compiled and all real-terrain/adversarial parity and allocation checks repeated successfully. This does not replace Unity API compilation or runtime validation.
