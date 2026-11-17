#!/usr/bin/env python3
"""Verify exact optional Cuartel art and transparent pixels; never auto-promote."""
import hashlib,json
from pathlib import Path
from PIL import Image
ROOT=Path(__file__).resolve().parents[1]
MAN=ROOT/"QA/cuartel_candidate_manifest.json"
m=json.loads(MAN.read_text(encoding="utf-8"))
assert m["schema"]==1 and m["state"]=="NOT_INTEGRATED_ART_REVIEW_PENDING"
assert m["candidate_dimensions"]==[1594,1733]
candidate=ROOT/"Sprites/Candidates/Cuartel_Candidato.png"
source=ROOT/"Sprites/Candidates/Cuartel_Runway_Source.png"
assert hashlib.sha256(candidate.read_bytes()).hexdigest()==m["candidate_sha256"]
assert hashlib.sha256(source.read_bytes()).hexdigest()==m["source_sha256"]
with Image.open(candidate) as im:
    assert im.size==(1594,1733) and im.mode=="RGBA"
    assert im.getchannel("A").getextrema()==(0,255)
scene=(ROOT/"Scripts/Editor/City2DSceneBuilder.cs").read_text(encoding="utf-8")
native=(ROOT/"Scripts/Editor/City2DNativeEvidence.cs").read_text(encoding="utf-8")
assert 'Cuartel_Candidato.png' not in scene, "Unapproved artwork silently made default"
assert 'Cuartel_Candidato.png' in native and 'candidateCuartel' in native
assert 'candidate_cuartel_comparison_only' in native
print("CUARTEL_CANDIDATE_REVIEW_ONLY_PASS - native evidence still required")
