# Unified near ground and playable context

The reviewed native view still used a separate dull green prairie and local zoom 36, causing a large visual discontinuity from the painted world and a very narrow portrait crop. The canonical close reference shows the city in a wider field of actual resources.

Use the same world-coordinate atlas on positively identified existing prairie renderers, with a repeatable painted meadow detail tile. Geometry, colliders, targets and bounds remain original. Shared material arrays are retained and restored on teardown. URP shader uses world XZ rather than imported UVs; blue water suppresses meadow detail. Two texture samples, no lighting or scene rewrite. Source basis: Unity 6 manual, https://docs.unity3d.com/6000.0/Documentation/Manual/urp/writing-shaders-urp-unlit-texture.html (consulted 22 Oct 2026).

Detail source: built-in imagegen `exec-4fa54529-468a-458f-a922-e6e83df28714.png`, already authored before the P0 trial; a golden/olive painterly ground tile with no buildings/trees/POIs. Imported at 1024px with repeat wrapping. Native performance remains unprofiled.

Local zoom 64, local-ground threshold 72. Regional/panoramic zooms remain 113 and 244/456. Near portrait width increases from about 33 to 59 world units. This is a deliberate framing change, so old/new near images are not claimed to have identical cameras. Regional/panoramic comparisons still match. Existing real wood-target reachability tests run in both mobile aspects; add near-material support and portrait-context assertions.

Prior step retained after native run 39854149671 (commit a3b73044b975c1f3514469feacba9b717a37e1cb, artifact 12458977713): seven tests passed, compact anchored scar visibly clears Valoria's approach. No artistic or physical-device approval is claimed.
