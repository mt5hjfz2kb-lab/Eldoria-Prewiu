# Native232 — geometry retained, lighting correction required

Affine fitting preserves complete slate tiles and separates the original hall roof from the existing side towers. Direct detail, landscape and portrait review finds the desired architectural fit. However changed Bastion I faces show obsolete baked-atlas shading, unlike the non-baked Bastion II view; do not accept the current combined render.

Native233 reuses this exact candidate, not another geometry variant. The two changed mesh renderers disconnect their old lightmap binding and receive existing light probes plus direct lighting. ContributeGI is removed on those renderer nodes so a future bake does not silently reuse obsolete secondary UVs. All materials, colliders and other scene lighting remain intact. Trial restores original renderer and static settings without saving.

Native232 Unity author/render stages completed; overall workflow failed because Windows' python command was a Store alias. The existing Blender-bundled interpreter is used for lossless storage; no installation. Full artifact12296085839 retains editable sources and exact candidate, compact artifact12296220694 retains12 images. Recovered files are scoped only to roof-junction226.
