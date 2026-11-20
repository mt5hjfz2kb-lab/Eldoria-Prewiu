#!/usr/bin/env python3
"""Lossless SOURCE-ASSET repair. Decode original indexed alpha PNG to RGBA PNG
while retaining the exact original bytes, GUIDs, camera and all rendered pixels.
A Unity editor import must STILL be tested; this is not a visual PASS.
Run only in the isolated Umbral II workstream on the source-pinned branch.
"""
import hashlib
import io
import json
from pathlib import Path
from PIL import Image

root=Path(__file__).resolve().parents[2]
source=root/"Unity/Assets/Experimentos/ValoriaModular25D/Resources/ValoriaModular25D"
manifest_path=source/"sprite_manifest.json"
proof=root/"docs/valoria-modular-25d/WORKER_SAW_RGBA_SOURCE_CONVERSION.json"
backup=root/"tools/valoria-modular25d/provenance/worker_saw_original_indexed"
contract=root/"tools/valoria-modular25d/verify_source.py"
static=root/".github/workflows/valoria-modular25d-umbral-static.yml"

manifest=json.loads(manifest_path.read_text(encoding="utf-8"))
entries={e["name"]:e for e in manifest["sprites"]}
results=[]
backup.mkdir(parents=True,exist_ok=True)

for name in ("worker_saw_0","worker_saw_1"):
    png=source/(name+".png")
    original=png.read_bytes()
    sha=hashlib.sha256(original).hexdigest()
    expected=entries[name]["sha256"]
    assert sha==expected,(name,"source SHA mismatch")
    assert original[:8]==b"\x89PNG\r\n\x1a\n" and original[25]==3,(name,"expected original indexed source")
    meta=(source/(name+".png.meta")).read_bytes()
    with Image.open(io.BytesIO(original)) as im:
        im.load()
        assert im.size==(96,96) and im.mode in ("P","RGBA"),(name,im.size,im.mode)
        original_rgba=im.convert("RGBA")
        pixels=original_rgba.tobytes()
        pixel_sha=hashlib.sha256(pixels).hexdigest()
        buffer=io.BytesIO()
        original_rgba.save(buffer,format="PNG",optimize=True,compress_level=9)
    converted=buffer.getvalue()
    assert converted[25]==6 and converted[:8]==original[:8]
    with Image.open(io.BytesIO(converted)) as after:
        after.load()
        assert after.mode=="RGBA" and after.size==(96,96)
        assert after.tobytes()==pixels,(name,"pixel data changed")
    (backup/(name+".indexed.png")).write_bytes(original)
    png.write_bytes(converted)
    assert (source/(name+".png.meta")).read_bytes()==meta,(name,"meta importer modified")
    new_sha=hashlib.sha256(converted).hexdigest()
    entries[name]["sha256"]=new_sha
    entries[name]["bytes"]=len(converted)
    entries[name]["source"]+=("; original indexed PNG retained under tools/valoria-modular25d/provenance/"
                              "worker_saw_original_indexed; exact decoded RGBA pixel identity preserved;"
                              " Unity import still requires real native verification")
    results.append({"name":name,"indexed_sha256":sha,"rgba_sha256":new_sha,
                    "decoded_rgba_pixel_sha256":pixel_sha,"indexed_bytes":len(original),
                    "rgba_bytes":len(converted),"pixel_identical":True,
                    "guid_meta_preserved":True})
    print("LOSSLESS_ORIGINAL_RGBA_PIXELS_VERIFIED",name,sha,new_sha,pixel_sha)

manifest_path.write_text(json.dumps(manifest,ensure_ascii=False,indent=2)+"\n",encoding="utf-8")
code=contract.read_text(encoding="utf-8")
for row in results:
    assert row["indexed_sha256"] in code,(row["name"],"source-contract digest absent")
    code=code.replace(row["indexed_sha256"],row["rgba_sha256"])
    # Replacing the digest above makes the source contract easy to update
    # without disturbing anything else in its original curated asset list.
    old='"'+row["rgba_sha256"]+'",'+str(row["indexed_bytes"])
    new='"'+row["rgba_sha256"]+'",'+str(row["rgba_bytes"])
    assert old in code,(row["name"],"original expected size tuple not found")
    code=code.replace(old,new)
assert 'self.assertEqual(content_bytes[25],3,"Original paletted PNG with alpha, not a backplate")' in code
code=code.replace('self.assertEqual(content_bytes[25],3,"Original paletted PNG with alpha, not a backplate")',
                  'self.assertEqual(content_bytes[25],6,"Losslessly decoded RGBA PNG with alpha, not a backplate")')
contract.write_text(code,encoding="utf-8")
work=static.read_text(encoding="utf-8")
needle='assert b[:8]==bytes.fromhex("89504e470d0a1a0a"),name'
assert needle in work,"Static PNG signature source guard changed unexpectedly"
work=work.replace(needle,needle+'\\n               assert b[25]==6,(name,"RGBA expected after pixel-identical source re-encode")')
static.write_text(work,encoding="utf-8")
proof.parent.mkdir(parents=True,exist_ok=True)
proof.write_text(json.dumps({
    "schema":1,"reason":"Unity 6000.3 Resources.Load rejects original indexed worker_saw_1",
    "hypothesis":"PNG indexed palette+tRNS import compatibility; re-encode RGBA without pixel changes",
    "original_sources_preserved":True,
    "art_modified":False,"camera_modified":False,"gameplay_modified":False,
    "unity_verified":False,"assets":results,
},ensure_ascii=False,indent=2)+"\n",encoding="utf-8")
print("LOSSLESS_SAWYER_RGBA_CONVERSION_READY_FOR_UNITY_IMPORT_TEST")
