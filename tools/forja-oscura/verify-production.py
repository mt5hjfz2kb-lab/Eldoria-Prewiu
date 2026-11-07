#!/usr/bin/env python3
"""Verify actual Unity production files, not a conceptual mockup or ZIP placeholder."""
from pathlib import Path
import json, struct, zlib, hashlib, sys
root=Path(__file__).resolve().parents[2]
ui=root/"Unity/Assets/Eldoria/VisualKit"
if not ui.exists():ui=root/"Unity/Assets/EldoriaVisualKit"
surfaces=sorted((ui/"Resources/ForjaOscura/Surfaces").glob("*.png"))
icons=sorted((ui/"Resources/ForjaOscura/Icons").glob("*.png"))
code=sorted(ui.rglob("*.cs")); asmdef=sorted(ui.rglob("*.asmdef"))
assert len(surfaces)==22, ("surfaces",len(surfaces))
assert len(icons)==20, ("icons",len(icons))
assert len(code)==5, ("csharp",len(code))
assert len(asmdef)==2, ("asmdefs",len(asmdef))
def verify_png(p):
    data=p.read_bytes()
    assert data.startswith(b"\x89PNG\r\n\x1a\n"),p
    pos=8; width=height=0; found_idat=False; found_end=False
    while pos<len(data):
        assert pos+12<=len(data),(p,"truncated chunk")
        n=struct.unpack(">I",data[pos:pos+4])[0]
        kind=data[pos+4:pos+8]
        end=pos+12+n
        assert end<=len(data),(p,kind,"incomplete chunk")
        payload=data[pos+8:pos+8+n]
        stored=struct.unpack(">I",data[pos+8+n:end])[0]
        assert stored==zlib.crc32(kind+payload)&0xFFFFFFFF,(p,kind,"CRC mismatch")
        if kind==b"IHDR":
            width,height,bitdepth,color,comp,filtr,inter=struct.unpack(">IIBBBBB",payload)
            assert color==6 and bitdepth==8 and inter==0,(p,"not RGBA 8-bit")
            assert width>=48 and height>=48,(p,"undersized")
        if kind==b"IDAT":found_idat=True
        if kind==b"IEND":found_end=True
        pos=end
    assert found_idat and found_end and pos==len(data),(p,"incomplete PNG")
    return width,height
for p in surfaces+icons:
    w,h=verify_png(p)
    if p in icons: assert (w,h)==(256,256),(p,(w,h))
names={json.loads(p.read_text())["name"]:json.loads(p.read_text()) for p in asmdef}
if ui.name == "VisualKit":
    metas=list(ui.rglob("*.meta"))
    rootmeta=Path(str(ui)+".meta")
    assert len(metas)==55 and rootmeta.is_file(), ("FORJA_META_MISSING",len(metas))
    import re
    guids=[re.search(r"(?m)^guid: ([0-9a-f]{32})$",m.read_text()).group(1) for m in metas+[rootmeta]]
    assert len(set(guids))==56,"FORJA_META_DUPLICATE_GUID"
assert "Eldoria.ForjaOscura.Runtime" in names and "Eldoria.ForjaOscura.Editor" in names
assert names["Eldoria.ForjaOscura.Editor"]["includePlatforms"]==["Editor"]
assert "Eldoria.RoyalGold.UI" in names["Eldoria.ForjaOscura.Runtime"]["references"]
runtime=(ui/"Runtime/ForjaOscuraLiveHudSkin.cs").read_text()
native=(ui/"Editor/ForjaOscuraNativeEvidence.cs").read_text()
assert "TrySkinV3Context" in runtime and "OnDestroy()" in runtime
assert "ApplyNow" in runtime and "SpriteSwap" in runtime
assert "CapturePrepared" in native and "ForjaNativeEvidence.json" in native
assert "VALORIA_GAMEPLAY_PROBE" in native and "physicalMobileTested=false" in native
print(json.dumps({"result":"FORJA_PRODUCTION_PNG_CRC_AND_ASMDEF_PASS",
"png":len(icons)+len(surfaces),"icons":len(icons),"surfaces":len(surfaces),
"csharp_sources":len(code),"unity_compiled":False,"unity_play_tested":False},indent=2))

painted=ui/"Resources/ForjaOscura/Surfaces/panel_dark_painted.png"
source=root/"tools/forja-oscura/art-source/panel-dark-painted-candidate.png"
assert painted.read_bytes()==source.read_bytes(),"Painted production source drift"
assert hashlib.sha256(painted.read_bytes()).hexdigest()=="31a073bc88213505795b37445983262b80b132d6b2249144abc26a385bef5447","Painted source hash drift"
print("FORJA_PAINTED_SOURCE_EXACT_PARITY_PASS")
