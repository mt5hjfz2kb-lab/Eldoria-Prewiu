# Phase1 — approved hero family

Owner approved the Casa de la Campana hero shown by native run 43848924672 and authorized deriving its ruined and corrupted states and assembling Phase1. Phase2 remains blocked pending owner review of the assembled result.

The approved hero FBX, atlas and Hero_Review scene are preserved byte-for-byte. author_family.py executes the exact approved author_hero.py construction up to its export, retaining the named source components; it removes and reuses them for damage. New geometry supplements exposed interior beams, broken crown and embedded crystals. The ruin preserves an inhabited warm window on its left side. The corruption follows the tower and adjacent roof, with local ground cracks and slow particles. The environment is a continuous clearing with irregular paving and reduced background trees, not foundation platforms.

Build: Blender author_family.py -- --ruin / --corrupt / --environment, then verify_family.py. The exporter preserves UV0 and vertex colors. Unity verifies summed triangles across all imported submeshes (the 16-bit importer may split them), uses the proven 180-degree asset root correction and keeps camera tilt 50 degrees/yaw -13 degrees.

Review sequence: local Blender geometry review; native Unity import and four viewport captures plus two isolated closeups; independent visual review; owner review. Successful import or CI does not certify artistic quality or performance on physical iPhone. Scene is new Phase1_HeroFamily.unity; original scenes and gameplay are untouched.

Local first review corrected suspended tower roof fragments in the ruin. Fine appearance of floor joints, roof remnant contact, crystal facets and local ground fractures must be judged in the actual Unity capture before closing the work.

Native iteration 1: run 43852572242 imported and rendered all four resolutions plus two closeups successfully. Real PNG inspection found the clearing boundary visible, ground colors exceeding normalized vertex ranges, inverted horizontal fracture normals, and particle counts reset by closeup isolation. Iteration 2 extends only the perimeter to 144m, keeps the dense central grid, normalizes the ground palette, reverses ground-fracture winding, and preserves/re-simulates particles for the relevant capture. Independent review found no further clear geometry/contact blocker; the surviving thin tower-window mullion in the ruin remains a minor cleanup candidate.
