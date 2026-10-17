# ELDORIA — Three isolated real-Unity visual trials (2026-10-17)

**Status: IMPLEMENTED PREVIEW SCENE SOURCES, NOT YET UNITY-RENDER-VALIDATED.**
This branch is a user-authorized independent **look development experiment**, not a pivot of the canonical Valoria pipeline. Never merge to main or replace the gameplay scene without a new owner's decision.

## Three real Unity scene files

| Unity scene | Artistic approach | Chosen territory |
| --- | --- | --- |
| `01_FantasyStrategy.unity` | Playable-scale fantasy strategy architecture | Green valley, riverside entry, rising defensive plateau |
| `02_DetailedMedieval.unity` | Detailed stylized medieval stone architecture | Steep alpine foothills with monumental high Bastion |
| `03_ColorfulCastle.unity` | Colorful and approachable strategy town | Lush hilltop settlement, vivid roof and heraldry palette |

Open **the Unity 6000.3.23f1 project in `Unity/`**, then open each scene under
`Assets/Eldoria/ArtTests/ThreeVisualStyles/`. An `[ExecuteAlways]` script generates
the landmass, water, bridge, gate, walls, upper Bastion, two functional building
locations, secondary structures, crops, ruin details, trees, rocks and fixed
review camera. Switch to the **Game** tab for the camera's genuine Unity view.
Three variants use different geometry ratios, terrain and palettes. Each
`.unity` is independently loadable, and generation runs again when opened.

### Actual native screenshot / permanent scene creation (once Unity is free)

In Unity menu choose:
- `Eldoria > Art tests > Three visual styles > Capture all three PNGs`
- `Eldoria > Art tests > Three visual styles > Bake three editable Unity scenes`

Alternative Unity batch methods:
- `-executeMethod Eldoria.ArtTests.ThreeVisualStyles.ThreeVisualStylesCapture.CaptureAll`
- `-executeMethod Eldoria.ArtTests.ThreeVisualStyles.ThreeVisualStylesCapture.BakeAll`

Captures go to `docs/evidence/eldoria-three-visual-styles-20261017/`.
The bake turns generated geometry into saved native `.unity` objects and stores
persistent `.mat` and `.asset` files within this same isolated folder.
Capture and bake are opt-in, not scheduled on the occupied Windows runner.
The editor should be opened with **this branch checked out**, without a worktree
sharing the canonical Valoria Windows session.

## What IS and IS NOT proved

- This is a real C# Unity scene implementation. **It has not yet been executed or photographed in Unity**. There are no claimed real Unity screenshots, no FPS benchmark, and no art acceptance.
- The current geometry is **original procedural Unity mesh/primitives**, not the actual Quaternius or Kenney pack models. Thus it is a **first architectural and terrain trial** for each aesthetic, not a fair final judgment on the three commercial-quality asset families.
- Next artistic step (not yet done) is to import a legally verified free kit per style, replace building and fortification facades with authentic meshes, ensure source texture/material/pivot compatibility, and rerender all three under consistent camera review.
- The lower gate, riverside connection, Bastion I, production buildings and reconstruction motifs preserve narrative roles. They are LOOK DEV ONLY; no invented power/economy, no save state, no fake claims about Sir Aldric, Breach events, cards or narrative progression.
- No original `PlayerState`, `LocalGateway`, scene, build settings, `main` or active workstream is modified. Current gameplay remains in the real established project. These scenes are not standalone gameplay slices.
- `GameObject` materials and procedural preview geometry are not persistent until explicitly baked. Mobile QA and Unity CI are **NOT RUN**.

## CC0 art sources planned for the true three-family A/B

- Strategy: [Quaternius Ultimate Fantasy RTS](https://quaternius.com/packs/ultimatefantasyrts.html), CC0.
- Detailed medieval: [Quaternius Modular Medieval Buildings](https://quaternius.com/packs/modularmedievalbuildings.html) and free [Medieval Village MegaKit Standard](https://quaternius.com/packs/medievalvillagemegakit.html) if available. Verify the exact free package content and licenses.
- Colorful castle: [Kenney Castle Kit](https://kenney.nl/assets/castle-kit) and [Fantasy Town Kit](https://kenney.nl/assets/fantasy-town-kit), CC0.

**Do not represent procedural placeholders as any author's CC0 art.**
Do not embed Internet-sourced previews or AI artwork into the Unity scenes.

## Remaining acceptance gates

1. Unity import and compile succeeds for all three isolated scene files on the specified engine.
2. Three genuine `_UNITY_REAL.png` files, consistent resolution/lighting and camera provenance.
3. Original CC0 model assets installed with source and license notes; one unified family per scene, no mixed visual sources unless deliberately approved.
4. Readable mobile strategic view and visually continuous terrain/bridge/gate/Bastion.
5. Verify zero changes to canonical workstream. Device QA only when a runner/device becomes available without interfering with the existing constructor.

**Do not mark ART PASS before those gates.**
