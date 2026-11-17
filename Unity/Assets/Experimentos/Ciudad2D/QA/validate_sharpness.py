#!/usr/bin/env python3
"""Static settings gate; native Unity importer and pixels are verified separately."""
from pathlib import Path
ROOT=Path(__file__).resolve().parents[1]
imp=(ROOT/"Scripts/Editor/City2DSpriteImporter.cs").read_text(encoding="utf-8")
native=(ROOT/"Scripts/Editor/City2DNativeEvidence.cs").read_text(encoding="utf-8")
checks={
 "lossless_import":"importer.textureCompression = TextureImporterCompression.Uncompressed;" in imp,
 "no_mips":"importer.mipmapEnabled = false;" in imp,
 "bilinear":"importer.filterMode = FilterMode.Bilinear;" in imp,
 "texture_cap":"importer.maxTextureSize = 4096;" in imp,
 "native_audit":"VerifyUncompressedSpriteImports();" in native,
 "native_report":'quality_import_uncompressed_pass' in native,
 "original_terrain_pixels":'Width=941, Height=1672' in native,
}
for key,ok in checks.items(): print(("PASS" if ok else "FAIL")+": "+key)
assert all(checks.values()),checks
print("QUALITY_STATIC_PASS: native proof and physical mobile remain separate gates")
