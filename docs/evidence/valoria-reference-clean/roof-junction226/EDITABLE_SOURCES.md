# Editable roof sources

The current .blend contains the original SOURCE and revised CANDIDATE meshes for both existing Bastion stages, with UV and packed materials. GLBs and compressed Unity mesh data accompany author-proof.json.

If a .blend exceeds GitHub's single-file limit, its exact bytes are stored as .part01/.part02 plus .parts.json. Reconstruct with:

```sh
python tools/valoria-reference-clean/pack_roof_sources.py --restore docs/evidence/valoria-reference-clean/roof-junction226/RejectedHip230/BastionRoofJunctions.blend.parts.json
```

Use the relevant manifest path for another source. The command verifies each block and the original complete SHA256 before producing the .blend. No geometry is regenerated or simplified during storage. Sources below the limit remain ordinary .blend files. Rejected sources are not game assets.
