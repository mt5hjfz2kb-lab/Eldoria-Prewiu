# Hero form review — 16 November 2026

Candidate ready for owner form approval. NOT ART_APPROVED. Do not derive/assemble the ruined and corrupted versions or advance Phase 2 before approval.

- Branch: art/visual-reboot. Selected contour B, Casa de la Campana; the three initial black studies are silhouettes.png.
- Actual native run: 43848924672, SUCCESS. Unity 6000.3.23f1 / URP 17.3.0.
- Executed source: f1876818638d23558ca7d2f0f20671c118fe7fff.
- Scene, native metadata and PNG persistence: d5083afe01345672e7611fdbf81163f21cc55dd1.
- Native artifact: 14215718841. Original PNGs: ../Evidence/native-43848924672/.
- Four resolutions: 1100x1250, 390x844, 1600x900, 844x390; plus actual black silhouette at 1100x1250. Four native manifest screenshot hashes verified locally.
- Game camera rotation (50,-13,0), orthographic, reframed to the whole isolated hero. Asset yaw corrected +180 degrees after the prior native rear view was rejected.
- 32,804 authored and imported triangles, atlas 256x256, UV and vertex color attributes verified in every imported part. Unity splits the mesh under 16-bit indexing; full geometry is checked by summed triangles and combined bounds.

## Form changes
Wide battered stone base; projecting timber upper floor; main roof width 6.63m versus conventional 5.1m (130%). Thick curved eaves, individual thick slates, staggered irregular stones and bevelled arrises. Offset tower moved right/back and raised, open belfry with visible bell, supported balcony, chimney, small gable emblem. Painted atlas edge washes and simple timber grain. No inherited prohibited Bastion model or paid generation.

## Actual visual review
Front facade, bell, arched windows, balcony and displaced tower visible in native Unity. Solid-black native silhouette separates hall roof and tower. Independent review considers it appropriate for owner form approval, not commercial-finish certification. Material limits: timber lines are too regular and lighting makes stone/bell quite light; these are finish decisions, not proof that the geometry must be rebuilt.

Review light: key elevation32, intensity1.5, color(1,.84,.65); cool fill .35; bloom .12/threshold1.1; fog disabled. Capture helper sets soft shadows2048/two cascades. capture.json retained its original zero defaults for unassigned shadow diagnostic fields; those zeros do not describe the actual pipeline settings. Source code is authoritative for this review setup.

## Preserved and pending
Original three-building pilot scene and game/domain directories are byte-unchanged against 05c2851714143e2d33ce41c81673f6dfc1f3db6f. Hero_Review.unity is a separate inspection scene. No main art, gameplay, publication, mobile-performance approval or Phase2 work. Windows released after terminal success.

## Lessons retained
43846720539 compiled but the one-renderer assertion failed because Unity split the mesh. 43848005730 compiled/rendered but showed the rear facade; its images are diagnostic evidence, not the final presentation. Final run corrects placement while retaining camera, geometry and light. Source script and editable Blend/GLB/FBX remain reproducible.
