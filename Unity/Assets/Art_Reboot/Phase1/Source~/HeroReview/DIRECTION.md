# Hero form revision — pre-model decision
Owner request 2026-11-16. Phase 1 only; no scene assembly before approval.
Three solid-black contour studies in silhouettes.png precede modeling.
Selected B: Casa de la Campana. Wide low stone plinth, pinched walls, projecting timber upper hall,
heavy bowed slate roof, narrow offset bell tower, visible negative space at belfry, small side balcony.
A repeats the rejected box + crenellations; C has a readable roof but an insufficiently distinct tower.
Reuse search: sawmill AP2, granary BIII, TowerWallRock are candidates in the catalog; none matches this
new family silhouette. Preserve existing Reboot atlas shader/import/camera lessons, author new meshes.
No old forbidden AI Bastion. No external images, paid generation or texture licensing dependencies.
Author ChatGPT; Blender script is a reproducible mesh tool, not evidence of approval.
Game view rotation remains Unity (50,-13,0); close-up framing only.
Roof span target: conventional 5.1m -> 6.63m (130%). Stone walls taper 7%; tower ~1.55m width.
Ruined/corrupted derivatives must reuse this source after hero approval, no scene assembled here.

## Review corrections actually executed
First 50-degree Blender render hid the bell: raised belfry opening 0.60m, moved bell forward, narrowed cap.
Independent visual review identified black gaps in arch voussoirs and upper windows occluded by eave.
Replaced rotated rectangular voussoirs with fitted wedge meshes; lowered and shortened front windows.
Final local Blender preview shows bell, continuous arch rings and both window arches.
Blender structural validation is separate from Unity render and owner art acceptance.

Actual 3D silhouette at the game view initially merged tower into the roof.
Macro correction: tower +0.45m right / +0.73m back, height +10%; balcony follows its structural support.
Independent review accepts the form as a candidate for owner review after native capture; no art approval inferred.
Known finish limits: some facade symmetry remains, timber grain is restrained.

Native attempt 43846720539 compiled but stopped before rendering: Unity expanded the FBX UV/color corners
and split the mesh under its 16-bit index import. Capture now verifies all imported parts, summed
triangle count against source geometry.json, attributes on every part and their combined bounds.
Keep 16-bit import compatibility rather than assuming one renderer. Failure logs retained in artifact 14215617104.

Native 43848005730 succeeded technically (32804 triangles; five PNGs) but showed the BACK of the hero.
Do not deliver that image as accepted art. Preserve its evidence. Correction is asset-placement yaw +180 degrees,
with unchanged game camera (50,-13,0), imported geometry, lighting and materials. Re-run real Unity to verify facade.
