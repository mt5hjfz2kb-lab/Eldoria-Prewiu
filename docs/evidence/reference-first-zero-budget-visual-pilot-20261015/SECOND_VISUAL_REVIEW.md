# Independent review — second authentic Unity capture (2026-10-15)

**Exact executed source:** `b5133dd5abf73ed1747f5464f25ff1bb7e749406`, Windows job [38798569103](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/38798569103) SUCCESS; original artifact 12003642386; branch commit `345ee4991a875b72219cbb2070585efedb9211da` preserved two authentic 1536×1024 frames and manifest (`artApproved=false`).

**Compared to baseline:** same two camera positions and image sizes, source baseline `d35aa1126cc316514ebab2b593c99e35635fd2b1`, recovered from run 38796237340. Authentic shots, not image-generation previews.

**Verdict:** `ART_REJECTED` / **not better enough for acceptance**.

- New sources demonstrably instantiate: 2 Stone_Gate and 4 Stone_Wall prefabs; 22 trees, 14 cliffs, 11 rocks and 13 hero renderers. Better structural semantics than no defensive envelope, but the gate and wall placements are disjoint around an unchanged manor-sized main building and lack the bridge/upper-tier hierarchy from the approved reference.
- Severe regression: forest/tree materials have become visually neon saturated **blue** in the second Unity image, whereas the baseline conifers were natural dark green. This alone blocks approval regardless of composition.
- Cliff palette changes from high-key green/white to muted stone, but the resulting large repeated contour-ring hill prefabs remain strongly disconnected from one another and visibly artificial.
- Terrain still consists of a uniform flat stage base; road and fortifications do not establish convincing functional continuity with a rock mesa.
- The reference's water-facing front escarpment, connected road and stairs, deep forestry, warm gate torchlights and continuous upper defensive line remain absent.
- `TECH_PASS=true` for the fixed source; `VISUAL_PASS=false`, `ART_APPROVED=false`, `REUSE_PROVEN=false`, `LICENSE_PASS=false`.

**Next strategy change:** do not just retint the same fourteen detached cliff pieces or generate another independent manor/gate arrangement. Test a continuous, two-level terrain shape and existing upper-architecture/lower-gate families at one full frame. Preserve original screenshots and separately document any new material regression. No production merge/publication, no claim of 8/10 until an authentic Unity view supports it.
