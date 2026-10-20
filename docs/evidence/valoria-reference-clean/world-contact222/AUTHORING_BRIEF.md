# World rendering and extraction-ground finishing

Observed actual native217 and Windows39473497402 gameplay: forest, quarry and threats lack contact shadows/depth compared with editor art previews. Source inspection confirms ZoneOneStrategicCamera.ApplyPose places the orthographic camera350 units from its target, while both current URP assets enable shadows only to220 units. Earlier artistic captures placed the camera110 units away. This is a real runtime/render mismatch, not proof that the authored models should be replaced.

Correction: retain default350 for historical scenes, expose a serialized depth offset and set140 only in the integrated scene. Preserve rotation, orthographic zoom, target, navigation algorithms and projected X/Y composition; verify numerical projection invariance. Keep existing220-unit shadow range and2048 shadowmap resolution. A/B must use350 before and140 after, documenting this intentional depth-only correction instead of falsely claiming identical camera matrices. Lights, URP settings and framing remain unchanged. Functional WebGL replay must validate the changed camera.

The quarry apron appears buried against the actual world surface. Measure signed distances, conform the existing professionally authored Blender apron mesh to the actual ground with its irregular boundary seated slightly below grade, preserve its topology/UV/materials and seat retained waste/equipment. Do not alter world terrain, resource nodes, colliders or rules. Preserve source.blend and original imported meshes.

No new camp, monument, building, resource, game rule or paid service. Forest mip coverage was measured rather than guessed: reduction generally increases coverage, so no blind alpha/culling change is justified.
