# Ashen Citadel — source and visual review continuation

Scope: existing `experiment/ashen-citadel-i-v-20261115`, same workstream and owner identity. No production merge, deployment, paid generation or gameplay rule modification.

## Observed root cause and actual corrections

Native baseline `c4bcb07198c9cede22ba8a1c4bc1769086675f78`, run 43770724604, artifact 14183534101 visibly exposed roof interiors, hollow conifers and open rocks. Generated box, prism, cone, frustum and roof face winding pointed inward. `verify_winding.py` rejects that original mesh set and passes the corrected one (signed solid volume and roof normals); general source validation alone had missed the defect.

Original meshes were repaired, not replaced with legacy artwork. Palisade stakes now join; conifer boughs have a deliberately irregular silhouette; fieldstones are sculpted polygon rings with untextured natural material instead of masonry. Soil material boundaries no longer draw large polygon patches. Dark slate, grey stone, muted teal foliage, warm details and restrained red banners form one palette. Five .blend sources and five UV/normal-bearing GLB exports were rendered and inspected in real Blender. `pack_sources.py` embeds all texture bytes for portable editable sources.

Reuse search: curated/current/historical catalogues returned 36 candidate matches. Historical rock terrain and pine families remain possible future sources but were not imported: this task continues its original, independently authored low-poly visual family; the prohibited legacy Bastion remains excluded.

## Native verification and findings

- `2a7e9556b6ac2d16ff3785efe192bb60662fe917`: source integrity and Blender CI passed (43771905255 / 43771905256). Local geometry/UV/PNG validation passed.
- `e5d4ea5948a88f1b92ece4465cdab1c79d7392f1`: native run 43772073861 succeeded, 7 EditMode + 4 PlayMode tests. Artifact 14184646602. Visual review confirmed opaque roofs and improved trees; portrait framing and ground boundary still required rework.
- `0bba20dae7bb82b2bb39905ba70f10dc8f88b214`: run 43772683066 succeeded, 7 + 4 tests, artifact 14185445759. Explicit URP StandardRequest captures and a shared runtime viewport rule were introduced. Increasing terrain extent alone did NOT eliminate the lower empty strip. Do not repeat this ineffective fix.
- Root cause of the remaining strip: an orthographic camera can have its lowest ray originate below the terrain when half-height exceeds camera elevation. `FrameCameraForViewport` now translates along the view axis until the lower ray origin is at least Y=2. This preserves orthographic composition and is shared by actual runtime and captures. The capture gate rejects a lower ray below 1.99. Landscape coverage remains 180 metres across.

## Lessons and remaining limits

1. Test solid face orientation, not only vertex counts/material assignments. Blender's two-sided preview can conceal errors that Unity's surface culling exposes.
2. A successful camera capture is not artistic approval. Compare actual terrain contact, silhouettes, framing and material scale.
3. Orthographic framing involves ray origins as well as field size; enlarging terrain cannot repair below-ground rays.
4. Use documented URP render requests for explicit capture, and drive viewport framing from the same runtime rule. Headless UI captures use real uGUI in temporary camera-space mode; they are not physical-phone screenshots.
5. Shadows/contact richness and the interface remain below final production ambition. No claim of finished UI, physical iPhone performance, complete I–V player journey, or global ART_APPROVED. The deliverable is a first real architecture/environment review sample, kept separate from the beta.

Official render request reference consulted: https://docs.unity3d.com/cn/6000.0/Manual/urp/User-Render-Requests.html

## Final evidence for this review sample

Verified code `b0390aa11ad3f9ca4ae596e88ddadc94fb0ea530`: native run 43773241535 completed SUCCESS; artifact 14185198110, zip SHA256 303f8b3b14a4f4ab53caaa352101254b6629a2b11717dc937cc02d5d5f93196b. The actual XML reports 7/7 EditMode and 4/4 PlayMode passed. Source CI 43773241336 passed.

All five portrait views and the final Bastion V landscape image were visually inspected. The lower empty strip is absent, the castle is fully framed, roof exteriors are opaque and I-V silhouettes progress visibly. This is a reviewable architecture/environment sample, not global artistic approval. Lighting remains flat, terrain detail repetitive, interface unfinished and physical iPhone performance unverified. No production merge or deployment.

The before/after sheet uses real Unity screenshots with labels only. It is not an exact identical-camera comparison: the final camera uses a corrected depth position. The five portable Blend/GLB source exports identify art-source commit 2a7e9556b6ac2d16ff3785efe192bb60662fe917 and do not pretend to contain subsequent Unity camera/runtime changes.
