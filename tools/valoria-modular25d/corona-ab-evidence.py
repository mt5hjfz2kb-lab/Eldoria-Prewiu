"""Pair genuine baseline and candidate screenshots; no inference of artistic score."""
import hashlib
import json
import os
import sys
from pathlib import Path
from PIL import Image, ImageDraw

baseline, candidate, out = map(Path, sys.argv[1:4])
out.mkdir(parents=True, exist_ok=True)
dpr = int(os.environ.get("VALORIA_DPR", "1"))
screens = [
    ("hud-b1","bastion-1.png"), ("hud-b5","bastion-5-completo.png"),
    ("tutorial","guia-iniciar-obra.png"), ("construction","temporizador-en-parcela.png"),
    ("name","nombre-inicial.png"), ("march","sistema-march.png"),
    ("bag","sistema-bag.png"), ("mail","sistema-mail.png"),
    ("daily","sistema-daily.png"), ("heroes","sistema-heroes.png"),
    ("training","sistema-training.png"), ("scouts","sistema-scouts.png"),
    ("vigilants","sistema-vigilants.png"),
]
manifest = {"baseline_run":45802472999,"baseline_sha":"91b8de9874aed87219d733efa6e8e0aa56207f1e", "candidate_sha":os.environ.get("GITHUB_SHA"),"dpr":dpr,"pairs":[],"missing":[],"approval":False}
for role, name in screens:
    left, right = baseline/name, candidate/name
    if not left.is_file() or not right.is_file():
        manifest["missing"].append({"role":role,"baseline":left.is_file(),"candidate":right.is_file()})
        continue
    with Image.open(left) as before, Image.open(right) as after:
        before, after = before.convert("RGB"), after.convert("RGB")
        if before.size != after.size or before.size != (390*dpr,844*dpr):
            manifest["missing"].append({"role":role,"baseline_size":before.size,"candidate_size":after.size})
            continue
        w,h = before.size
        pair=Image.new("RGB",(2*w+12*dpr,h+26*dpr),"#172028")
        pair.paste(before,(0,26*dpr));pair.paste(after,(w+12*dpr,26*dpr))
        draw=ImageDraw.Draw(pair)
        draw.text((6*dpr,5*dpr),"BASELINE / "+role,fill="white")
        draw.text((w+18*dpr,5*dpr),"CANDIDATE / "+role,fill="white")
        file=out/(role+".png")
        pair.save(file)
        manifest["pairs"].append({"role":role,"file":file.name,"baseline_sha256":hashlib.sha256(left.read_bytes()).hexdigest(),"candidate_sha256":hashlib.sha256(right.read_bytes()).hexdigest(),"size":[w,h]})
(out/"manifest.json").write_text(json.dumps(manifest,indent=2),encoding="utf-8")
if manifest["missing"]:
    raise SystemExit("Incomplete comparison evidence: "+str(manifest["missing"]))
print("A/B pairs:",len(manifest["pairs"]),"No visual approval or quality score inferred.")
