#!/usr/bin/env python3
"""Package the EXACT ORIGINAL Eldoria sawyer PNGs for Unity TextAsset binary import.

Unity 6000.3 Windows failed Resources.Load<Sprite> and Resources.Load<Texture2D>
for the authored second frame even after PNG source normalized to RGBA.
Unity's supported .bytes TextAsset path bypasses that image importer while
retaining every original compressed byte. This script generates an ALIAS of
each source, never replaces/redraws its art or changes source .meta GUIDs.
"""
from pathlib import Path
import hashlib
import json
import struct
import zlib

ROOT=Path(__file__).resolve().parents[2]
ART=ROOT/"Unity/Assets/Experimentos/ValoriaModular25D/Resources/ValoriaModular25D"
MANIFEST=ART/"sprite_manifest.json"
PROOF=ROOT/"docs/valoria-modular-25d/WORKER_SAW_BINARY_TEXTASSET_PROOF.json"

def sha256(blob):
    return hashlib.sha256(blob).hexdigest()

def validate_png(blob, label):
    assert blob[:8]==b"\x89PNG\r\n\x1a\n",label
    assert len(blob)>200,label
    w,h,depth,colour,compression,filtermethod,interlace=struct.unpack(
        ">IIBBBBB",blob[16:29])
    assert (w,h,depth,colour)==(96,96,8,6),(label,w,h,depth,colour)
    assert (compression,filtermethod,interlace)==(0,0,0)
    offset=8
    idat=[]
    end=False
    while offset<len(blob):
        assert offset+12<=len(blob)
        length=struct.unpack(">I",blob[offset:offset+4])[0]
        tag=blob[offset+4:offset+8]
        segment=blob[offset+8:offset+8+length]
        checksum=struct.unpack(">I",blob[offset+8+length:offset+12+length])[0]
        assert (zlib.crc32(tag+segment)&0xffffffff)==checksum,(label,tag,"CRC")
        if tag==b"IDAT":idat.append(segment)
        offset+=12+length
        if tag==b"IEND":
            end=True
            break
    assert end and offset==len(blob) and idat,label
    decompressed=zlib.decompress(b"".join(idat))
    # Every PNG row has 1 filter byte plus 96x4 channel bytes.
    assert len(decompressed)==96*(1+96*4),(label,"Unexpected decoded pixel buffer")
    return {"width":w,"height":h,"bit_depth":depth,"color_type":colour}

def main():
    manifest=json.loads(MANIFEST.read_text(encoding="utf-8"))
    expected={s["name"]:s for s in manifest["sprites"]}
    records=[]
    for idx in (0,1):
        name=f"worker_saw_{idx}"
        src=ART/(name+".png")
        assert src.is_file(),src
        raw=src.read_bytes()
        assert sha256(raw)==expected[name]["sha256"],(name,"source manifest mismatch")
        assert len(raw)==expected[name]["bytes"],(name,"source bytes mismatch")
        geometry=validate_png(raw,name)
        dst=ART/(name+"_png.bytes")
        meta=ART/(name+"_png.bytes.meta")
        # Deterministic GUID independent of the existing .png .meta GUID.
        guid=hashlib.md5(("Eldoria-UmbralII-Original-Sawyer-TextAsset-"+name)
                         .encode("ascii")).hexdigest()
        if dst.exists(): assert dst.read_bytes()==raw,(name,"preexisting alias differs")
        else: dst.write_bytes(raw)
        metadata=("fileFormatVersion: 2\n"
                  f"guid: {guid}\n"
                  "TextScriptImporter:\n"
                  "  externalObjects: {}\n"
                  "  userData: \n"
                  "  assetBundleName: \n"
                  "  assetBundleVariant: \n")
        if meta.exists(): assert meta.read_text(encoding="utf-8")==metadata
        else: meta.write_text(metadata,encoding="utf-8")
        assert src.read_bytes()==dst.read_bytes()
        records.append({
            "name":name,"source_png":src.relative_to(ROOT).as_posix(),
            "unity_textasset_path":"ValoriaModular25D/"+name+"_png",
            "alias_file":dst.relative_to(ROOT).as_posix(),
            "original_and_textasset_sha256":sha256(raw),
            "size_bytes":len(raw),"source_png_guid_preserved":True,
            "source_pixels_unchanged":True,"geometry":geometry,
            "textasset_guid":guid
        })
        print("SAWYER_ORIGINAL_BYTES_TEXTASSET_IDENTITY_PASS",name,sha256(raw),len(raw))
    PROOF.parent.mkdir(parents=True,exist_ok=True)
    PROOF.write_text(json.dumps({
      "schema":1,"method":"Unity Resources.Load<TextAsset> + ImageConversion.LoadImage",
      "purpose":"Unity 6000.3 Windows importer loses source Sprite and Texture2D for frame 1",
      "source_assets_unchanged":True,"not_new_art":True,
      "engine_verified":False,"physical_iphone_tested":False,"frames":records
    },ensure_ascii=False,indent=2)+"\n",encoding="utf-8")
    print("SOURCE_BINARY_ALIAS_PREPARED_FOR_NATIVE_UNITY_RUNTIME_VERIFICATION")

if __name__=="__main__":
    main()
