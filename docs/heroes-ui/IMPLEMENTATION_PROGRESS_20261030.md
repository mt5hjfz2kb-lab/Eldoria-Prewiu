# PR135 — interactive implementation, awaiting integrated native certification

The former flattened reference display is replaced with real Unity controls on the existing HUD safe area: hero hall, narrative unlocks, selection, summary, abilities, locked talents/equipment pages, available-hero navigation and rotation of a cached 3D render. Every statistic is supplied by the host; no mock values, local save or parallel progression are created.

The model preview copies render data from an already loaded canonical model, including a baked skinned pose. It does not clone gameplay behaviours, import a second GLB, alter the city actor or run an extra per-frame camera. Its render texture and temporary meshes are released. Missing hero art is not replaced by another character.

Approved provenance: six-screen master SHA256 `48cbc4e73d4144c45af12731773f0fd60b45e81d2a871256b33fe2a00f7d6800`, package `Eldoria_Heroes_Integracion_Preparada_v14.zip`. The original master and flattened parts remain references, never interactive backdrops with fake values.

Pending: authoritative host binding, exact Unity compilation and interaction tests, real model orientation, landscape/portrait screenshots and comparison with the approved screens. This change is not a fidelity certificate and is not ready for publication on its own.
