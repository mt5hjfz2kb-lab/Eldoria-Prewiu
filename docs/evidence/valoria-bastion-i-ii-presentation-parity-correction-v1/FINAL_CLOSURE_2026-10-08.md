# Valoria Bastion I-II presentation parity correction v1 — final closure evidence

Date: 2026-10-08
Workstream: `valoria-bastion-i-ii-presentation-parity-correction-v1`

## Final verdict

**PASS / CLOSED — declared Bastion I–II presentation-parity scope**

This closure is based on the final recertified UI candidate, its exact production artifact, the successful published WebGL build, the full published device-equivalent probe, and direct inspection of the resulting landscape/portrait screenshots.

It does **not** claim that Eldoria has reached final commercial artistic quality or that a physical human-device playtest is equivalent to the automated probe. Those remain separate quality questions. It does certify the scope authorized for this block: touch interaction, camera/pan/recenter, building selection, mobile UI presentation, Bastion I→II progression, persistence and canonical reset on the published owner WebGL.

## Final certified chain

- Final UI cleanup source: `1a18ddc59e65f3a4dffcd432a8fd3945c60f9e7f`
  - removes duplicate CTA clutter;
  - prevents contextual building panel / quest-card overlap.
- Objective-dock restoration fix: `4d70b9bdc5532ffb3108867a7d5450a72ea460bc`
  - restores the persistent objective dock after contextual actions.
- UI progression certification: **37772733872 — SUCCESS**
- Unity source/editor gate: **37772733932 — SUCCESS**
- Production run: **37772733956 — SUCCESS**
- Production artifact: **11549887766**
- Runtime parcel source artifact: **11547989564**
- Exact publish request: `7063fd36c0d2840f22ad2d1290fbccfe074ffd49`
- Publish run: **37776564090 — SUCCESS**
- Published WebGL probe: **37779269570 — SUCCESS**
- Published evidence artifact: **11551901858**
- Evidence artifact digest: `sha256:8b59598b4eff06a6825d6a99ef953681bc49906a714e920da599f78312601da6`
- Published owner URL: https://mt5hjfz2kb-lab.github.io/Eldoria-Prewiu/unity-owner/

The publication was staged from the certified production artifact for `4d70b9bd...`; unrelated later main commits did not replace that certified Valoria production input.

## Published gameplay / interaction acceptance

The final published probe completed the uninterrupted fresh-save Bastion I→II flow in **both landscape and portrait**.

Verified:
- canonical fresh Bastion I reset state;
- HOME camera framing;
- real horizontal touch pan in both directions;
- moderate vertical pan;
- recenter/HOME behavior;
- MUNDO navigation;
- Region 1 real gather/reward loop;
- return to Valoria;
- real visible selection of **Aserradero**, **Bastión** and **Cuartel**;
- Aserradero reconstruction;
- resource gathering and route progression;
- Bastion I→II ascent;
- Cuartel construction;
- +20 archer recruitment;
- configured march;
- scout / Engendro combat and reward;
- final `b2.complete` state;
- exact final-state save/reload persistence;
- deliberate two-step reset;
- canonical reset surviving reload.

Final state before reset in the accepted flow:

`revision=30, wood=400, stone=360, gatheredWood=1080, gatheredStone=700, sawmill=1, bastion=2, barracks=1, archers=56, trained=20, configured=true, scout=true, engendro=true, objective=b2.complete`.

The final probe report has `failure=null`, both full flows PASS, building-selection PASS, persistence PASS, reset PASS and `runtimeErrors=[]`.

## Visual evidence review

The final evidence artifact contains:
- fresh Bastion I;
- all three selected-building states;
- every guided progression step;
- Bastion II completion;
- pan-left / pan-right / vertical-pan evidence;
- World before/after action;
- return-to-Valoria;
- landscape and portrait variants.

Direct screenshot inspection of the final artifact confirms:
- the contextual building panel is associated with the selected building and remains inside the mobile viewport;
- the quest/objective card does not collide with the contextual building panel in the reviewed selected-building states;
- the previous duplicate primary/context CTA clutter is removed while the contextual action is open;
- the objective dock returns after contextual actions;
- bottom CIUDAD/MUNDO navigation remains visible;
- reset and recenter controls remain reachable;
- the camera remains city-first rather than becoming a full-city strategic overview;
- level/state cues are visible during Bastion I→II progression.

The city still uses a visually static SHARP-based beauty foundation and this closure does **not** promote the project to final-art or “finished commercial game” quality. Broader life/animation/art uplift remains outside this workstream.

## Scope integrity

- Bastion III not opened.
- World Region 1 gameplay/state not redesigned.
- Economy/progression/persistence remain canonical.
- SHARP visual authority preserved.
- No parallel gameplay/state system introduced.
- No new visual R&D lane opened.
- Paid credits: **0**.

## Closure rule

All declared current-head production, publication, published interaction, progression, persistence, reset and bounded mobile visual checks required by this workstream are now satisfied. The workstream may be released autonomously.

Concrete later owner/device feedback remains authoritative evidence and may reopen this exact block if it demonstrates a regression.


## Resource release guard

Closing the publish request originally triggered one unnecessary Pages workflow because the old source-preflight treated any change to `pipeline/unity-publish-request.json` as a heavy Unity request even when `enabled=false`.

The closure therefore also installed a bounded release hygiene fix:
- `8f0b42dbae939243e5c11975d8bb5d3753e71b6b` — disabled publish requests no longer acquire the Windows Unity runner;
- `8fbe4e392379e756e4ca8e213d1504cb3fd39119` — final disabled closure-guard state.

The superseded post-close run **37781441338** was cancelled by workflow concurrency before deployment. Replacement run **37782259376** completed SUCCESS with `unity-webgl=skipped` and `deploy=skipped`. This confirms the closure state no longer consumes the Unity runner or republishes a stale/default WebGL candidate.
