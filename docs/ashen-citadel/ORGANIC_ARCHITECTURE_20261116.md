# Organic architecture — continuous authored forms

Owner: keep the good direction but buildings should feel more organic and characteristic, not assembled from fitted pieces. Baseline source b7646dad75ecdb53b2a4c0792968c141cc630e6a, native run 43846509441. Same experimental branch, spacing/camera/light/ground and authoritative game systems retained.

## Artistic decisions
Organic means coherent structural transitions and designed profiles, not random noise applied to vertices. Replace flat modular roof planes with closed roof shells, slightly unequal ridges, swept eaves and matching gable cheeks. Timber underside is visible only at overhangs. Masonry receives restrained corner chamfers. Castle round towers gain 12-sided drums and profiled caps; entrances become segmented stone arches with matching curved timber doors.

Sawmill: broad curved wooden roof spanning an open bay, bowed timber cross-brace and integrated lower canopy. Granary: gently swelling timber silo, fitted hoops and flared cap rather than perfect cylinder+cone. Barracks: continuous curved longhouse roof, arched entrance and drooping fabric porch. Stoneworks: cloth sags between posts; remove roof-like transverse timber stripes from canvas. Refine supports, not just decorate edges.

Reuse check found production Aserradero_AP2, Cuartel_AP2 and Granero_BIII; adapt the current owner-reviewed family instead of replacing it with another kit. Original authored geometry source remains generate.mjs, with genuine Blender render/editable source and actual Unity review. No paid resources or publication.

## Validation
UV/source integrity and winding; Blender I/V; same-camera native I–V and portrait; existing 7 EditMode + 4 PlayMode tests. Explicitly inspect eave/wall contact, roof thickness, arch normals, silhouette, room for selection and no clipped auxiliaries. The roof-normal guard stays unchanged: slate exterior faces point outward/up, timber soffits point down. Physical mobile and final artistic approval remain separate.

## Native result — completed 16 November

Geometry c33ee4addbb6053cc6a5119c97094433591ee163; exact native source 16761f13eaf6ba12059ccae73b50852f3ddc3aa1. Run 43851110708 SUCCESS, artifact 14217547766; archive SHA256 2e8aed831bca5312c079fb485cb24a645bf842fc781027994e5d27c7b333555c. All ten screenshot hashes verified. NUnit XML: 7 EditMode + 4 PlayMode passed, zero failed/skipped. Blender workflow 43849214620 SUCCESS. Local source integrity/outward winding pass.

Same-camera Unity comparison to b7646da: closed curved roofs replace flat fitted planes; fuller silo and shaped cap; sagging canvas without roof-like timber stripes. Arched portals most visible in level I, partly occluded by higher gatehouse. Inspected I/II/IV/V landscape and V portrait: no missing materials, floating platforms or clipped auxiliaries. Structural profiles improved, but some wall massing remains box-like; stone tiling remains repetitive and mobile overview is small. Not final commercial art approval or physical iPhone/WebGL validation. Windows released after terminal success and cleanup.

Lesson: controlled shell profiles with matching gable closure improve structural continuity without random noise. Keep light/camera identical for form comparisons. Future surface refinement is separate from this completed geometry pass.
