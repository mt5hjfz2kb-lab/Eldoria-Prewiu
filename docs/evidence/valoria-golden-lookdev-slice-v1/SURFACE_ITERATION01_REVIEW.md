# Golden Surface iteration01 — VISUAL REVIEW

Verdict: **TECH PASS / VISUAL FAIL**

Authoritative run: **37359204369**  
Artifact: **11365997109**

## Technical / regression
- Unity capture: PASS.
- Focused gameplay tests: PASS.
- Surface authoring: PASS.
- Authored renderers: **20**.
- Surface families: **5**.
- Channel stack: albedo + tangent normal + AO + metallic/smoothness.
- 256×256 maps; estimated uncompressed proof footprint: **6,990,506 bytes**.
- Water treatment: PASS.
- No Tripo credits.

## Visual verdict
The surface channel stack is real, but its screen-space coverage is insufficient. In the Golden close crop the Gate/contact pieces receive richer response, yet the dominant surfaces still read as the old prototype:
- `MainPlatform`, `PlayableGround` and `ForegroundBank` occupy most ground/cliff pixels and remain visually flat/faceted;
- the source03 local berm/road/bridge-shoulder names are not all routed into the GROUND surface family;
- Bridge/Road/threshold surfaces keep the old repetitive value hierarchy;
- water retains a mostly uniform teal value with no convincing shallow/deep relationship.

The frame is still a cleaner prototype, not premium-close.

## Estimated quality (0–5)
- material richness: **2.5**
- lighting depth: **2**
- contact quality: **2.5**
- terrain/architecture integration: **2.5**
- water/shore integration: **2**
- natural integration: **2**
- premium perception: **2**
- mobile readability: **4**

## Failure class
Dominant: **SURFACE COVERAGE**.  
Secondary: **WATER**.

## Iteration02 must materially change
Do not only increase texture contrast. Route the actual Golden-crop renderers into the authored surface hierarchy:
- local Gate source03 berm/verge/shoulder pieces → GROUND;
- Bridge deck/parapets/support, threshold and the two road segments visible in the crop → STONE/GROUND as appropriate;
- adjacent `ForegroundBank` → ROCK (the specific cliff requested by Golden scope);
- existing rock contacts → ROCK;
- existing vegetation → VEGETATION;
- shore → SHORE.

Do not globally re-author `MainPlatform` / `PlayableGround`; they remain outside this bounded surface proof.

Water iteration02 must replace 9× repeated uniform response with one bounded shallow→deep value field plus multi-frequency normal response.

No geometry expansion and no broad-scene rollout in iteration02.
