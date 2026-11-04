# Rejected legacy Bastion — archival preservation only

The owner explicitly prohibited this model on 25 October 2026 (AGENTS.md).
Do not import, instantiate, promote, use as fallback artwork, include in a Player
or publish it without a later explicit, specific owner authorization.

The original 13,257,704-byte GLB remains unchanged (Git blob
`e31aa6a63af826f4c6f666bb369fc270233c8d66`, SHA-256
`afb6cee6ae572b0879650f18285b32798e263c17359a158ffe2bdd03fb62ad5c`).
Its original metadata is preserved alongside it (Git blob
`bab32c587d4bd8c194a242304ae97dd937bcc29b`, GUID
`fa36c6abf91adc7778caeac12130f567`).

Both files are outside Unity Assets, Resources and StreamingAssets. Their former
Resource path was `Valoria/HeroBastionGenerated/Valoria_HeroBastion_v1`.
The existing historical runtime caller returns when that Resource is absent;
this archival move adds no replacement model, scene or gameplay code.

The metadata audit rejects reintroduction at the former Unity path and verifies
both archived Git blobs. The native validation script refuses full scene/runtime
validation while that forbidden Resource exists. Catalog entries remain
OWNER_BLOCKED_NO_USE. These are preservation safeguards, not artistic approval
or a claim that the separate PR136 C# build guard has been integrated.

PR136 additionally makes the historical runtime replacement inert.
RejectedLegacyBastionBuildGuard rejects prohibited shipping identities, runtime
source references and enabled-scene dependencies. The native proof writes
legacy-bastion-veto.json. Retain both this build guard and the metadata audit.
No replacement model or artistic approval is implied.
