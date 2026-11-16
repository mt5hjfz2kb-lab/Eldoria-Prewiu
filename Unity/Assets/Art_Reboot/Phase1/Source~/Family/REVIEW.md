# Phase1 family — actual Unity review

The owner-approved hero is retained unchanged. Its ruin exposes roof framing, fallen reused masonry/slates and the original bell. Its corrupted variant retains the architecture with embedded crystals and fractures in the tower, adjacent roof and immediate ground. New scene: `Phase1_HeroFamily.unity`.

## Evidence

- Native run 43853944970, Unity 6000.3.23f1 / URP17.3.
- Exact source: `27c147d2d65b2f6bbce136da82e0e40cd43d5f43`.
- Saved scene and captures: commit `3057e214eb13cce5f08df3eb16db7eacbb454d7d`.
- Artifact 14220910694; original PNGs in `Source~/Evidence/native-43853944970/`.
- Four camera viewports: 1100x1250, 390x844, 1600x900, 844x390. Two isolated variant closeups. Four viewport SHA256 hashes match native-proof.json.
- 97,263 triangles across hero, ruin, corruption and environment; verified imported triangle totals, UVs and vertex colors. Eleven simulated corruption particles. One local torch point light.
- Hero FBX/atlas and original scenes preserved byte-for-byte by native assertions.

## Corrections made from observed results

The first local ruined tower had disconnected roof pieces: removed them. First native family capture revealed a finite clearing boundary, clipped ground color range, ground-fracture backfaces and particles reset during isolated capture. Second native capture verifies the extended terrain perimeter, normalized varied ground colors, visible ground fractures and preserved/re-simulated particle state. No gameplay or production scene was modified.

## Visual assessment and limits

The three architectural states remain recognizable as the same family, and the warm intact hall contrasts with the exposed ruined frame and localized violet crystals. The broken roof remnant is connected to its diagonal beam. Ground now fills the reviewed viewports. Independent review of the first native captures found no further clear geometry/contact blockers, while noting a thin surviving window mullion in the ruined tower as a minor cleanup candidate.

This is a Phase1 composition candidate for the owner, not commercial-finish certification. Remaining refinement opportunities include less regular timber grain/paving, more organic fracture edges, the surviving ruined-window mullion, and a closer gameplay framing for narrow portrait screens (the current portrait capture deliberately fits all three states). Physical iPhone performance and full production-game integration are not tested. Phase2 has not started.

Independent final review of run43853944970: suitable to show as Phase1 candidate; no new geometry blockers. Terrain edge removed and three states distinguishable. Ground fractures still look like thin violet cables, and 390x844 overview loses detail. Use large capture and closeups for the art review; these findings remain open refinement items.
