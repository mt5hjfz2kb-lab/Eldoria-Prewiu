# Eldoria production asset manifests

Each production 3D asset should eventually have one machine-readable JSON manifest in this directory.

The manifest exists to prevent identity and workflow state from being reconstructed from chat, logs or prose documents.

Required conceptual fields:
- asset_id
- function
- status
- approved_reference with file name, SHA-256, bytes and dimensions
- credit_authorization with approved cost and immutable input SHA
- generation with provider, model/version, task ID and consumed credits
- raw_model with SHA-256, bytes and triangle count
- optimized_model with SHA-256, triangle count and refinement flags
- integration with Unity path, yaw/scale/parcel constraints
- gates for TECH, INTERACTION and VISUAL
- evidence with commit/run/artifact IDs

Do not automatically infer a paid authorization from a previous asset. Authorization is asset + exact-input + exact-cost specific.

A future orchestrator may update these manifests automatically. Until then, creating a manifest is recommended for new paid production assets once the schema is stabilized.
Surface evidence is first-class and should record:
- material family/profile;
- authoring tool/version;
- source texture identities and inferred roles;
- optional bake configuration/results;
- Unity material/import profile;
- LookDev profile;
- SURFACE verdict.

The generated workflow artifact `eldoria-asset-manifest.json` is the machine evidence for a gate run. It complements, but does not automatically replace, a curated persistent manifest under this directory.
