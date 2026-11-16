# Ashen Citadel · Marcellus / Alegreya Sans typography, functional Vigilia UI

Owner requested substantial playable art and UI work on the isolated Ashen experiment. The old UI used Unity's built-in LegacyRuntime font, a very small header, repeated button styles, and at least one English/Spanish label that wrapped on 390px portrait.

## Implemented and original licensing

- Embedded three independently sourced original Google Fonts binaries in `Resources/Fonts`, alongside their separate SIL OFL v1.1 licence texts:
  - `Marcellus-Regular.ttf` (Roman inscription heading font), from `google/fonts/ofl/marcellus`.
  - `AlegreyaSans-Regular.ttf` (body text), from `google/fonts/ofl/alegreyasans`.
  - `AlegreyaSans-Bold.ttf` (interactive button text), same source.
- Use runtime `Resources.Load<Font>` for a real imported Unity Font, retaining built-in fallback only if an asset is missing. Labels, dynamic resources, nav, Bag, Mailbox and World use one consistent type hierarchy and previous functional gateway.
- Text-only nav labels shortened visually to `BASTIÓN / OBRAS / TROPAS / MUNDO`, while their **actual canonical Unity GameObject IDs** are unchanged: `Nav · BASTIÓN / EDIFICIOS / EJÉRCITO / MUNDO`. Icon roles remain consistent.
- Dark-slate and brass nine-slice sprites come from previously verified 12 committed PNG source atlas, not from concept mockups. Account ribbon and buttons remain real click targets.

## Tested vs visually reviewed

- Baseline real native run **43899740404**, SHA `228d50c5391b3bab1c518b5883127996de6fe5b8`: 7 EditMode + 4 PlayMode pass; 5 I–V screenshots and 5 actual City/World/Mailbox/Bag/Welcome UI frames.
- First typography run **43903777365**: genuine Unity scene and screenshots rendered, 7 EditMode passed but PlayMode **3/4** — the expected original button ID `Nav · EDIFICIOS` was changed when shortening the *visible caption*. This was not a gameplay failure, but a real UI-contract regression.
- Fixed the root cause: distinct `nav[]` button identifiers and `shortLabels[]` captions. Native run **43905069629**, exact SHA `5a79f9ff53a153cc4b6ca4d06070b266ba1a6153`, artifact `14245362250`, archive digest `sha256:d1ade9e782ff93a760228b226cda28a1fbec64973fce7035c834cddc185e832c`, passed **7/7 EditMode + 4/4 PlayMode** and generated all screens.
- Actual native **City**, **Bag**, **World**, **Welcome** 390×844 were inspected. The new UI now has dark panels (no previous white panel bug), unique functional icons, consistent typefaces, lower density and short captions without the EDIFICIOS two-line wrap.
- Visual defect discovered **independently after the passing test**: the Marcellus heading at 18px had only one of the two intended title lines visible in the compact header (`BASTIÓN I` was clipped by the resource block). Changed heading size to 16 and resource size to 13 in **commit 33de41fa68bef6ef4af55955cc465a43c33853d8**. This **final text-fit correction has NOT YET been rendered in Unity**.

## Acceptance and resource rules

Do not call the last header-size tweak validated, or claim premium-art approval, physical iPhone success, WebGL FPS/performance, or true player-feedback verification until a new same-camera native screenshot is inspected. Keep this workstream isolated from production `main`. The latest known Windows reservation at the time of this documentation was held by the separate Phase 2 visual-reboot experiment; do not hijack it. Reclaim through the canonical registry when released.

The source quality and technical tests must be reported separately: the last verified UI and Bastion scene is usable as comparison evidence, but the heading-text fix is pending.
