# iPhone world-entry P0: local diagnostic 21d4 — 2026-11-15

The owner's physical iPhone still exits the game on world entry. Root cause is UNKNOWN.
This delivery repairs observability only; it is not a stability fix or a tester release.

## Evidence and interpretation

The supplied screenshot showed startup-ready, WASM heap capacity approximately 412 MiB,
then a new document navigation classified back_forward. HEAPU8.byteLength is not total
browser/device RAM. Neither navigation classification nor the supplied system report
established termination of the foreground game process. Do not ask for the same absent
iOS logs again or claim memory exhaustion based on the largestProcess field.

The old lifecycle journal did not capture native world phases. Startup chunk events could
also crowd earlier evidence out of its 48-event ring.

## Implemented

Canonical PR 194, same workstream and branch fix/eldoria-world-first-entry-p0-20261114.
Diagnostic source: 14c64284f2aef341c1787a1d815e54b369e15efc.
Native runtime remains d0b21fd5708c5070d1bd62dc6c6a98399c549051, built by 43653390105.
No Unity, game-save, gameplay, terrain or protected-route bytes changed.

tools/eldoria-private-beta/world-phase-diagnostic.js captures only allowlisted
existing native console markers. Separate bounded current/previous phase lists survive
a reboot and repeated landing-only opens. No raw log text, URLs, names, saves or requests.
The local key is eldoria.world-phase.diagnostic.v4. API:
window.EldoriaLocalDiagnostic.readWorldReport(). Existing eldoria_diag opt-in/off retained.
Button: Diagnóstico · 21d4; expanded panel provides Copiar registro with selection fallback.
world-frame-2 means two browser animation callbacks, NOT proof of GPU presentation or
successful physical-device rendering.

Do not add eldoria_profile=1 casually to the owner reproduction: native TraceStartupMemory
also enumerates all loaded textures/meshes and computes their runtime sizes at three
phases, perturbing the workload. This journal works with existing unconditionally emitted
world markers and does not need that heavier profiling flag.

## Validation and setbacks

Node VM tests passed for native marker mapping, original console receiver/return,
bounded storage, unknown/private log exclusion, blocked storage, opt-out and preservation
through both abrupt and landing-only reboots. Python tests prove exact native/protected
file preservation and rejection of altered native bytes.

Run 43696633066 correctly rejected a diagnostic overlay obscuring the actual nickname.
Move from top-left to the prior bottom-left area in 14c64284; do not weaken the nickname
or earned-construction gates. Run 43697064704 then stopped on public CDN HTTP503,
before playing or publication. Reuse the already independently verified durable archive;
reject any changed public certificate and verify all archived files again.

Successful prepublication real Mac WebKit journey in43697289069:
actual nickname, earned sawmill, reload persistence, two world entries/returns, real
wheel zooms, actual Unity world-requested/built/ready/entered markers and two callbacks,
then a genuine reload preserving the previous world journal. Zero reported runtime errors.
Reviewed world-visit-0 and city-return-sawmill-1: player name and footer controls visible,
diagnostic below the controls. These are desktop results, NOT physical iPhone acceptance.

Chrome live-page UI check separately opened Diagnóstico and clicked Copiar registro;
the visible button changed to Copiado. This is not a physical Safari clipboard test.

## Delivery and rollback

Publication workflow commit44b6b85298276cf5d251e32e4da270666fd3112e, run 43697289069.
Publication job 132760550035 succeeded and verified all 146 public files.
Public shell play-93e3d72e91b7ffe4.js SHA256:
93e3d72e91b7ffe4689d0845186b0e187a95be911eb4681441c3a0fe6d86c866.
Public stage-proof SHA256:
1e86acc219a7a05b94bcfcc7c7f901604c70ac4a08412f500a48882fd4c923ba.
Complete site:961140936 bytes. Native bundle unchanged:
6e33438d57aa5265e823d7730640432e0aa22421d71c7a62eb70fbf00bd74413.
Prepublication proof artifact 14154504994, ZIP SHA256:
c74d4969df24239706e1f0f57a8f729570889cba24eafff82bef4920442b5d04.

Durable rollback draft release 445924796, tag archive-pages-before-world-diagnostic-21d4,
asset 750405695 artifact.tar,961239040 bytes, SHA256:
7d053452b9739828310936e1b5e2ce759029fa8f458b24dfeada82d86673208e.
Previous public proof f4bc2c0ca3768f62992a66a7ef3fbe299ee2b561354c1b899847432fa62895fa.
All 145 old public files matched before patching. Other routes including stable beta18
remain byte-identical. Do not rerun this replacement against the new site with the old
certificate: that would correctly fail the stale-publication guard.

Owner URL:
https://mt5hjfz2kb-lab.github.io/Eldoria-Prewiu/beta-integrated-vigilia/?eldoria_p0_lazy_world=1&eldoria_diag=1&eldoria_p0_world_msaa_off=1&build=world21d4

## Physical results and next candidate

Owner supplied two full v4 journals after publication; the iPhone still fails.
The delayed-entry control has startup-ready at1794740195837, world-requested at1794740278799,
terrain-query at1794740289823, and a new back_forward boot at1794740291008.
City survives82.962s before the request; the restart follows the request by12.209s,
startup-ready by95.171s, and the last terrain-query marker by1.185s.
This rejects the earlier approximately10-second fixed-startup-timer hypothesis.
No further repetition of that control or search for the same absent device logs is needed.
Root cause remains UNKNOWN; missing pagehide does not prove memory termination.

Desktop public evidence in43697289069 shows4xMSAA city depth/color targets still resident
through creatures-after, about50.36MB combined, before the world camera-only toggle.
These are Unity runtime sizes, not total device process/GPU residency. The old toggle
cannot reduce the preactivation peak seen in the latest owner failures.

The new source candidate adds a separate eldoria_p0_entry_msaa_off=1 opt-in.
Before terrain loading it snapshots city/world camera MSAA settings, disables them,
and yields while the previous camera MSAA targets remain created (bounded90frames/15s).
It refuses terrain allocation if those old targets cannot retire. It logs measured target
counts/bytes before and after; no assumed memory reduction or fixed iPhone RAM limit.
Cancellation keeps the reduction while the shared background build runs, then restores
settings; return/failure/host disable restores the original camera values.
The default and previous world-only flag paths, terrain, resources, saves and approved UI remain intact.

Validation so far:15 existing source/UX parity checks pass. The actual Unity scene test
now verifies prebuild camera settings, no early terrain allocation, cancellation/rejoin,
world completion, restoration on return and the existing12reentries/persistent save.
Native execution, exact WebGL build, actual browser target reduction and physical iPhone
acceptance are PENDING. Do not publish this as a proven fix or ask the owner to test
unchanged21d4 again. Public runtime is still d0b21fd; no new native build is delivered.

## Exact native candidate evidence

Native source ca5e1c3d52a58d8c6d9c9a9114d2d35445a18029, workflow5c8cbebb3dd56b7e3816b69dd883815afc747b24,
run43707493997 build job132791702898 SUCCESS. ZIP14160399465 independently verified SHA256
22a534af1634c67e69291fb81b268b8b9d895a230701a9db0d115de1784e637b.
Real scene NUnit test1/1 passed in66.05s; first world load12.143s; cancellation/rejoin,
12reentries,48resources,36creatures and actual on-disk nickname/population reload retained.
Native I-V campaign and exact WebGL compile passed. Six near/regional/panoramic portrait/
landscape Unity images inspected: actual geography/population visible; not an art approval.

The native batch scene had2created render targets,0multisampled,1422321Unity bytes before
and after preparation. Prepared retired=0 frames=2. This is NOT proof of GPU memory savings.
The browser comparison must see actual pre-existing MSAA targets retire before the first
terrain stage and measure fewer target bytes, plus real saved-game journeys in both variants.

Windows job cleanly finished; generated owned Library cache reclaimed,48.64GiB free.
Lease released at revision82; packaging and subsequent browser QA use hosted runners only.
No new owner runtime published. Physical iPhone outcome is still the earlier FAIL.

## 21e1 desktop comparison — 2026-11-15

Native runtime ca5e1c3d52a58d8c6d9c9a9114d2d35445a18029, exact browser workflow
25bd1cd9678f0bd3615d55a0ff2ab720953b6924, run43712314700 / job132806380793 SUCCESS.
Artifact14161930181 ZIP SHA256177b8493f4af5faf6a78b33c557e52d9d6fe7e22caca8c09837eca201d98bbc1 independently downloaded and verified.

Identical compiled binary, old world-only flag control vs additional entry flag:
created render targets11 in both; multisampled3 to1; Unity target bytes72402259 to27965659.
Two old city targets retired after11frames, before terrain allocation: reduction44436600bytes
(42.38MiB). Original native batch had no multisampled targets and was not savings evidence.
Real browser startup, saved nickname, construction/reload, two world visits/returns, zoom
and persistent diagnostic journal passed in both variants with zero runtime errors.
World/city return screenshots inspected; terrain and saved sawmill visible, not art approval.

Total process residency is NOT proven lower. Peak sampled WebContent KiB:
control2264896, candidate2415216; GPU1350160 vs1216768 (non-simultaneous maxima).
Simultaneous WebContent+GPU peak per world visit KiB: first2701104 vs2849440;
second1669632 vs1442336. Global desktop process samples are not physical iPhone measurements.
Thus target retirement is measured, but neither total-memory improvement nor OS crash
root cause is established. Physical iPhone acceptance remains pending.

Publication request e519fb87327f8c8f3c4c0da8759b612be6708eeb / run43714355086
uses exact native/browser gates, all-file rollback and preservation. Publication outcome
must be appended before describing the candidate as delivered.
