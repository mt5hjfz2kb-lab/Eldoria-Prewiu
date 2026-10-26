# Regional scale correction — actual Unity evidence

Candidate source: 336328b92479d08421686fef587a4e5821390fc8. Native run 39842389820, artifact 12453951277. NUnit XML 7/7 passed; total 13.3627s. Native Unity job success. No art approval or release.

Actual expanded renderer inventory identifies three background meshes under exact ancestors Existing Holotna mountain.001/.002/.003, footprints approximately 62x61, 46x46, 32x33 world units. These disproportionate dark shapes overlaid the regional view. Only their renderer visibility now follows the close-to-regional transition. They return in close view; original states restored on adapter destruction. No transform, collider, resource, target, save or combat change. Quarry and neutral ruins remain. This is visual LOD of existing non-interactive background, not removal of gameplay mountains.

Compared to native 39841339374: close landscape, close portrait and both panoramas have zero pixels differing by >5/255. Regional landscape changed fraction .0260569; portrait .00003038. Source log confirms exactly 3 decorative mountain renderers. Captures visually reviewed: oversized dark background mountains removed; actual quarry/ruins and Spawnling retained.

This correction plus the macro coast/relief/forest/network changes is retained as a useful candidate. Still not close enough to reference for ART_APPROVED. Next large art priority: reconcile the original local foliage and strategic canopy scale/material language in regional view, maintaining actual wood target readability and exact close view; do not spend time on tiny stones or extra flecks. Portrait full-region framing remains unresolved. Existing camera is fixed-yaw and bound to a wide X/Z core, so a full portrait solution requires a deliberate framing/navigation design, not claiming the present half-width view is complete.

Current panorama remains identical to the retained 34d4245 capture. The 2048 baked source texture is unchanged and must not be regenerated merely for this renderer LOD edit. Future terrain/paint changes must follow PREBAKED_ATLAS_PROVENANCE_20261022.md. All work stays on PR100 candidate; no publication, no physical mobile certification.
