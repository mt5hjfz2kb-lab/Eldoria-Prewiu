#!/usr/bin/env python3
"""Validate authored Blender physical outputs, not artistic quality or Unity/iPhone."""
from pathlib import Path
import struct,json
ROOT=Path(__file__).resolve().parents[2]
src=ROOT/"Unity/Assets/Art_Reboot/CC0System/Generated"
pictures=ROOT/"docs/cc0-art-system/previews"
expected={
 "CC0_BastionI_Lookdev_1440x900.png":(1440,900),
 "CC0_BastionV_Lookdev_1440x900.png":(1440,900),
 "CC0_BastionV_Portrait_720x1200.png":(720,1200)
}
for p,wh in expected.items():
 b=(pictures/p).read_bytes()
 assert len(b)>20000,(p,len(b))
 assert b[:8]==b'\x89PNG\r\n\x1a\n',p
 dims=struct.unpack(">II",b[16:24])
 assert dims==wh,(p,dims,wh)
 print("AUTHORED_RENDER_VALID",p,len(b))
for name in ("CC0_Valoria_Stage1.glb","CC0_Valoria_Stage5.glb"):
 b=(src/name).read_bytes()
 assert len(b)>60000 and b[:4]==b'glTF',name
 assert struct.unpack("<I",b[8:12])[0]==len(b),name
 chunklen,chunktyp=struct.unpack("<I4s",b[12:20])
 assert chunktyp==b'JSON',name
 info=json.loads(b[20:20+chunklen].decode('utf-8'))
 assert len(info["meshes"])>70,(name,len(info["meshes"]))
 print("PHYSICAL_GLB_VALID",name,len(b),len(info["meshes"]))
blend=src/"CC0_Valoria_Source.blend"
assert blend.stat().st_size>100000
assert blend.read_bytes()[:7]==b'BLENDER'
evidence=json.loads((pictures/"render-evidence.json").read_text())
assert evidence["native_unity_render"] is False
assert evidence["blender_render"] is True
assert evidence["art_approved"] is False
assert not evidence["external_sources_imported"]
print("ELDORIA_CC0_RENDER_PROVENANCE_PASS")
