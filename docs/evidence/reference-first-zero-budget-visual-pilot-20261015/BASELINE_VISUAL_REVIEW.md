# Baseline visual review — reference-first Unity pilot

**Reference**: `references/VALORIA_APPROVED_VISUAL_REFERENCE.jpg` on `main`; authentic 1536×1024 baseline outputs: `01-wide-camera.png` and `02-main-architecture.png`, source SHA `d35aa1126cc316514ebab2b593c99e35635fd2b1`, Unity run [38796237340](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/38796237340) and artifact 12002711642. Original bytes were recovered and committed by Ubuntu run [38797716456](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/38797716456).

**Independent visual judgement**: `VISUAL_REJECTED` as a production-quality comparison. This is a useful working pipeline demonstration, **not** 8/10 artwork. No automatic approval based on successful import or PNG existence.

| Dimension | Directly observed in baseline Unity PNG | Difference from approved target |
| --- | --- | --- |
| Silhouette/composition | Single narrow central manor/tower, dispersed landscape lumps, broad empty background | Target has upper fortress and lower bridge/gate as the dominant dual hierarchy, a deliberately shaped peninsula and forested horizon |
| Geometry/contact | Flat brown/green rectangular staging plane, disconnected hill/cliff prefabs with white faceted edges; road composed of isolated tiles | Target requires a continuous raised rock mesa, contiguous path, meaningful stair/bridge contacts, masonry into rock |
| Materials | Oversaturated lime foliage ground on cliff prefabs; almost untextured stage plane/road; pale flat rock | Target has cohesive weathered warm stone, varied compact vegetation and slate-grey rock/terrain |
| Atmosphere | Uniform desaturated ambient blue backdrop and weak depth cues; no focal warm light | Target emphasizes warm torch/architectural light vs distant atmospheric cool blues |
| Game camera | The authentic Unity perspective is valid, but the composition does not fill the playable reference framing | Need to restore upper/lower vertical levels, readable access sequence, scenic depth; HUD integration is explicitly out of this isolated art test |
| Rights and approval | Imported local game repository models; `licenseVerified=false`, `artApproved=false` | Do not publish or promote until provenance and visual approvals are confirmed |

**Targeted next hypothesis** (not approval): keep the camera/view identical, instantiate the source `Stone_Gate.prefab` and `Stone_Wall.prefab` twice/four times as a nested defensive structure, replace distracting cliff material with the project's existing authored `rock_albedo`, add ground/road texture maps. Measure whether the recognizable fortified-access shape and material hierarchy improve visibly. This is `SOURCE_REUSE_LOOKDEV`, not Qwen-generated new authored 3D.

**Decision rule:** Reject cosmetic micro-adjustments until the large-scale silhouette and rock/fortress contact are convincing. If the next PNG still contains visibly detached terrain blocks, do not pretend the materials are sufficient: next action must replace the underlying terrain composition or choose a more appropriate existing cliff family. No quantitative score is asserted by a script or threshold; any subjective rating must be tied to authentic images and the permanent rubric.
