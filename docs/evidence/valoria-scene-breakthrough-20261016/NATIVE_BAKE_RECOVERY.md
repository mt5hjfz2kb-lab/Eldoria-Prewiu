# Native bake crash recovery

Run 38949465765, source d7626b7: actual Camera.Render native access violation (exit -1073741819) inside BakeDistant, after 3D capture suite and A scene save. Not a visual verdict. Preserve successful A scene, .meta and native PNG before any recovery. Recover B in separate fresh Unity process using same proven RGB capture path and a culling layer rather than changing live renderer enabled states during the async import continuation. Native source remains generated from 3D landscape/water only, no reference plate. Retain raw crash log artifact.

Frozen baseline is recovered read-only into the isolated scope and receives an explicitly recorded uniform frame normalization and matching sun/camera for A/B. Original source/PNG retained. Protected branches never edited.
