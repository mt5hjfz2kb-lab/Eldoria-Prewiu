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

## Next action

Physical-device evidence is now the missing input: owner opens the URL in Safari,
checks 21d4, enters the world once, and after the interruption reopens the same page,
opens Diagnóstico, copies the record and pastes it into this chat.
No cache/save deletion; no extra system-log search. Identify the last observed native
phase before choosing a targeted runtime intervention. Absence of pagehide remains
non-causal. Preserve original owner failure and do not announce the crash fixed.
No Windows lease or editor process held by this diagnostic work.

Public-origin follow-up job132763285353 is still running at this checkpoint; do not claim it passed yet.
