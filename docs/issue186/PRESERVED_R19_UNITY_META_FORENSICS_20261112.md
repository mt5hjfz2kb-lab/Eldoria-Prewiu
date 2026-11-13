# R19 → R21: forensic audit of Unity-importer metadata (off Windows)

**Observed artifacts, not artistic acceptance.** Native [43146245415](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/43146245415) preserved a 245-path Vigilia overlay after the actual saved-bag click failed. Restoration correctly refused an unfamiliar `.png.meta` SHA and did not erase the existing isolated Unity project. The next **read-only** [43149686136](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/43149686136) exported [artifact 13913127003](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/43149686136/artifacts/13913127003) with exact preserved current/installed metadata. No Unity capture, editor change or cleanup was authorized in R21.

## Measured with the artifact's 64 drift records

- The manifest lists **245** overlay paths; **64** have differing bytes, **all `.meta`**, and the audit report explicitly records `source_mutated=false`, `restored=false`.
- **60 PNG importer metadata files** exist in both the pinned installed source and the preserved current Unity tree. All 60 retain **the exact same Unity GUID**. Among these, **50** have importer `serializedVersion: 12 → 13`; **10** remain `13 → 13` but serialize additional fields.
- For those 60, a parsed YAML comparison found **zero value conflicts where both versions explicitly specify** the 17 tracked existing image-settings families: texture type, sprite mode/scale/mesh, alpha transparency, readability, texture size/format, filter/aniso/mip bias, wrap modes, mipmaps, pivot, border and sRGB. Removed keys, newly introduced fields, platform-name normalization and generated sprite sub-IDs were **not** treated as proof of equal Unity reference semantics.
- The remaining **four new metadata files** have no corresponding installed source: `CleanRoom.meta`, `CleanRoom/cleanroom_preview.jpg.meta`, `CleanRoom/manifest.json.meta`, `FreshArt/manifest.json.meta`. Their present GUIDs and bytes were archived; absence from the pinned source does **not** authorize automatic deletion while references remain uninspected.

## Causal assessment and safe recovery gate

The observed progression is consistent with Unity 6000.3 normalizing source metadata on import, not with an artwork/PNG change. This is a **supported hypothesis**, not a blanket determination that all 64 differences are disposable. The original runtime click failure came separately from V2 ChronicleLayout excluding the independent active Vigilia canvas; source correction has been made, but must be retested natively.

Before the next Unity checkout, compare **every live path SHA** against the archived R21 current SHA, and compare every pinned installed SHA and original backup with the R19 manifest. Reject any new drift, file, symlink or importer exception; do not run `git clean`. If a controlled cleanup is performed, retain the exact 64 current files, 245-path provenance and original backups outside the project; validate original GUIDs and potential sprite file-ID references. Only then allow exact, verified restoration, and record the result. The existing refusal must remain in place until this separate acceptance gate succeeds. Follow with the actual combined 8-PNG/20-first-hit/click/save/Back native proof in a single later fair Windows lot.

**Not proven:** Unity integrated bag flow, real full mailbox-row UI, A/B reference parity, art approval, physical iPhone, publication or main merge.
