#!/usr/bin/env python3
"""Fast independent integrity gate for the original Ashen Citadel art source.

This proves file integrity/topology and reproducibility, NOT visual quality,
shader import, Unity runtime, iPhone FPS or feature parity.
"""
import json
import math
import re
import struct
import zlib
from pathlib import Path

root=Path(__file__).resolve().parents[2]
source=root/"Unity/Assets/Eldoria/ExperimentalAshenCitadel"
meshes=source/"Meshes"
resources=source/"Resources"
manifest=json.loads((root/"tools/ashen-citadel/art-manifest.json").read_text())
palette=set(re.findall(r"^newmtl\s+(\S+)\s*$",(meshes/"AshenPalette.mtl").read_text(),re.M))
assert len(palette)==18, f"Expected 18 source materials, got {len(palette)}"
failures=[]
expected=[f"AshenBastion_{i}" for i in range(1,6)]+[
    "Ashen_Sawmill","Ashen_Barracks","Ashen_Granary","Ashen_Stoneworks","Ashen_Pine","Ashen_Rock"]
for name in expected:
    path=meshes/(name+".obj")
    content=path.read_text(encoding="utf-8")
    assert "mtllib AshenPalette.mtl" in content, path
    assert "Valoria_HeroBastion" not in content, "Prohibited old mesh reference"
    vertices=[]
    uv_coords=[]
    polygons=0
    active=None
    materials=set()
    for line in content.splitlines():
        if line.startswith("v "):
            pts=list(map(float,line[2:].split()))
            if len(pts)!=3 or any(not math.isfinite(p) or abs(p)>200 for p in pts):
                failures.append(f"{name}: invalid coordinate {line}")
            vertices.append(pts)
        elif line.startswith("vt "):
            xy=list(map(float,line[3:].split()))
            if len(xy)!=2 or not all(math.isfinite(x) for x in xy):
                failures.append(f"{name}: invalid texture coordinate")
            uv_coords.append(xy)
        elif line.startswith("usemtl "):
            active=line[7:].strip()
            materials.add(active)
        elif line.startswith("f "):
            tokens=line[2:].split()
            inds=[int(t.split("/")[0]) for t in tokens]
            uv_indices=[int(t.split("/")[1]) if "/" in t and t.split("/")[1] else -1 for t in tokens]
            if any(k<1 or k>len(uv_coords) for k in uv_indices):
                failures.append(f"{name}: missing/invalid UV0 index")
            if len(inds)<3 or not active or any(k<1 or k>len(vertices) for k in inds):
                failures.append(f"{name}: invalid face {line}")
            if len(set(inds))!=len(inds):
                failures.append(f"{name}: duplicate face indices")
            polygons+=1
    if not vertices or not polygons:failures.append(f"{name}: empty mesh")
    if len(uv_coords)!=len(vertices):failures.append(f"{name}: one UV0 required per authored face vertex")
    if not materials.issubset(palette):failures.append(f"{name}: unmapped source materials {materials-palette}")
    entry=("bastion"+name[-1]) if name.startswith("AshenBastion_") else name.removeprefix("Ashen_")
    expected_stats=manifest["stats"][entry]
    if len(vertices)!=expected_stats["vertices"] or polygons!=expected_stats["polygons"]:
        failures.append(f"{name}: generated topology differs from manifest")
    print(f"OK {name}: {len(vertices)} vertices, {polygons} polygons, {len(materials)} used MTL")

def validate_png(path,dimensions):
    b=path.read_bytes()
    assert b[:8]==b"\x89PNG\r\n\x1a\n",f"{path.name}: not PNG"
    at=8;data=b"";seen=[]
    while at<len(b):
        length=struct.unpack(">I",b[at:at+4])[0];kind=b[at+4:at+8]
        payload=b[at+8:at+8+length]
        sig=struct.unpack(">I",b[at+8+length:at+12+length])[0]
        assert (zlib.crc32(kind+payload)&0xffffffff)==sig,f"{path.name}: invalid CRC for {kind}"
        if kind==b"IHDR":
            w,h,depth,color,compression,filter_,interlace=struct.unpack(">IIBBBBB",payload)
            assert (w,h)==dimensions and depth==8 and color==6 and compression==0 and interlace==0
        if kind==b"IDAT":data+=payload
        seen.append(kind)
        at+=12+length
    assert seen[0]==b"IHDR" and b"IDAT" in seen and seen[-1]==b"IEND"
    pixels=zlib.decompress(data)
    assert len(pixels)==dimensions[1]*(1+4*dimensions[0]),f"{path.name}: invalid pixel size"
    assert all(pixels[y*(1+4*dimensions[0])]==0 for y in range(dimensions[1])),f"{path.name}: unsupported filter"
    print(f"OK {path.name}: {dimensions[0]}×{dimensions[1]}, CRC, zlib, RGBA")

for filename,size in [("AshenPanel.png",(256,256)),("AshenButtonPrimary.png",(256,72)),
                      ("AshenButtonSecondary.png",(256,72)),("AshenCrest.png",(128,128))]:
    validate_png(resources/filename,size)
for name in ["AshenStone.png","AshenRoof.png","AshenWood.png","AshenMoss.png","AshenMetal.png","AshenCloth.png"]:
    validate_png(source/"Textures"/name,(128,128))
mtl=(meshes/"AshenPalette.mtl").read_text()
if mtl.count("map_Kd ../Textures/Ashen")!=18:failures.append("MTL must bind all 18 authored materials")
if failures:raise SystemExit("\n".join(failures))
print("ASHEN_ART_SOURCE_INTEGRITY_PASS: 11 UV-mapped OBJ / 18 materials / 6 textures / 4 sprites")
