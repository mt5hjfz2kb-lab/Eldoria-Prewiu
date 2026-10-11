# Valoria mesh-only Bastion I — executable prototype and visual gate

This is an isolated proof, **NOT** a production Valoria replacement and **NOT** a visual PASS.

- Branch: `prototype/valoria-bastion1-no-sharp-20261010` (main has not been modified by this experiment).
- Unity editor command: `Eldoria/Experimental/Create Bastion I Mesh-Only Proof`.
- Unity generator: `Unity/Assets/Eldoria/Scripts/Editor/ValoriaMeshOnlyBastionOneProof.cs`.
- Inputs: the seven existing Blender-authoring GLBs under `art-source/valoria/production/*-family-v1`. No SHARP PLY/splats are loaded by the experiment.
- On execution, the Unity editor generator stages source GLBs and writes an isolated scene at `Assets/Eldoria/ProductionSlice/Experimental/ValoriaMeshOnlyBastionOne.unity` plus `ValoriaMeshOnlyProof/mesh-only-bastion-i-unity.png`.
- Independent zero-credit Blender raster proof: `tools/valoria_mesh_only_preview.py` run by `.github/workflows/valoria-mesh-only-preview.yml`.
- Blender proof run `38057518420` passed after fixing missing NumPy and the Ubuntu Blender denoiser limitation.
- Later camera-alignment preview run `38057660737` passed and committed `preview.jpg.base64.txt` (decoded low-resolution visual evidence).

## Visual judgment — FAIL / NOT READY FOR GAME

The actual raster preview is sparse and incoherent, with disconnected and misaligned structures on a flat gray background. It is nowhere close to the approved Valoria reference. Removing the dominant obscuring terrain mesh and using camera-projected scale improved visibility but did not make a cohesive settlement. The source GLBs were historically rendered `BeautyVisible=false` inside the canonical SHARP-backed composition. Their mere existence does not establish a premium stand-alone city.

This result must **not** be substituted for SHARP in the actual game or used to claim commercial readiness. The visual failure is a useful falsification of the naive 'turn the hidden GLBs on' approach.

## Remaining indispensable production gates

1. A new coordinated mesh/terrain scene design and local authored lookdev, not just rearranged support geometry, is required. Preserve canonical I/II progression and plot visibility.
2. Run isolated Unity scene-generation on an authorized Windows Unity runner; capture the official-camera screenshot; inspect visuals, import failures, and missing dependencies. No such Unity proof has passed.
3. Resolve the simultaneous active Region 1 workstream's claim on `windows-self-hosted-unity-6000-3-23f1` and shared Pages before heavy certification. The resource claim belongs to `world-region-1-visual-convergence-v3-20261010` and cannot be stolen just because an individual job is idle.
4. License-provenance review: Blender-authored geometry is a promising production path, but SHARP-independent render output by itself does not certify all embedded textures/materials for commercial use.

Status: **MESH-ONLY NON-SHARP EXPERIMENT BUILT; BLENDER RENDERED AND REVIEWED; VISUAL FAIL; UNITY NOT TESTED**. Main and public game unchanged.
