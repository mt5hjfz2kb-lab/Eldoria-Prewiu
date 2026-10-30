# Heroes approved UI — isolated integration workstream

## Canonical art
Use only the owner-approved six-screen image (generation ID 95d70929-2cba-4d2d-802e-fd47f555ef4d; SHA256 48cbc4e73d4144c45af12731773f0fd60b45e81d2a871256b33fe2a00f7d6800). The image is available in the conversation's v14 package but has **not** been uploaded to this branch. Do not substitute another generation. No resource bar.

## What is committed
`HeroesApprovedDestination.cs`: isolated destination state machine for canonical Royal HUD mounting, hero selection, tab selection and read-only authoritative data. Reference sprites are optional staging inputs; they are not final gameplay UI. Does not own a save, second Canvas, EventSystem, camera, resource, gameplay command or unlock logic.

## Required before merge
1. Upload approved master and independently authored assets to canonical asset vault, preserve provenance.
2. Port actual reconstructed salon/profile widgets from the local v14 preparation package, verify source and assembly compatibility.
3. Wire `ReadHeroes` to Unity domain; never use illustrative values. Base stats Aldric HP420 ATK78 DEF112 BREAK34; Lyra HP300 ATK116 DEF62 BREAK82. Runtime state may differ.
4. Verify Bastion III Fissure Lyra recruitment is the actual unlock; no early availability.
5. Mount via existing RoyalHUD/ValoriaIntegratedWorld binding, not a separate scene or parallel canvas.
6. Bind real ability, talent, equipment actions; unimplemented actions remain visibly disabled and non-mutating.
7. Import existing Aldric GLB from `Unity/Assets/StreamingAssets/Eldoria/Aldric/SirAldric_Approved_RiggedAnimated_v1.glb`; isolate preview camera and RenderTexture. Verify Lyra asset independently.
8. Execute Unity EditMode, PlayMode, native portrait/landscape captures, Android/WebGL smoke tests and pixel-by-pixel visual review. **100% fidelity not certified.**
9. Do not touch current releases or shared runner until other workstream releases resources and integration is authorized.

## Honest state
**DRAFT — not integrated, not compiled, not visually approved.** This branch is an actual repository change, not a local ZIP alone. No claim of finished game UI.
