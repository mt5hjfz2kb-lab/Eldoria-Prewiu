#!/usr/bin/env python3
"""Identity gate for the approved full-body Aldric GLB. Does not certify Unity PlayMode."""
from __future__ import annotations

import hashlib
import json
from pathlib import Path
import struct
import sys

ROOT = Path(__file__).resolve().parents[2]
MODEL = ROOT / "Unity/Assets/StreamingAssets/Eldoria/Aldric/SirAldric_Approved_RiggedAnimated_v1.glb"
EXPECTED_SHA = "c28a65b6040225256ae047d8fd773849771895ac378041e0882f279517e78c8b"
EXPECTED_SIZE = 10140372
EXPECTED_CLIPS = {
    "Aldric_Idle_v1", "Aldric_Walk_v1", "Aldric_SwordAttack_v1",
    "Aldric_Hurt_v1", "Aldric_Defeat_v1",
}

def verify(path: Path) -> None:
    if not path.is_file():
        raise ValueError("MODEL_MISSING: exact approved full-body GLB must be installed")
    data = path.read_bytes()
    if len(data) != EXPECTED_SIZE or hashlib.sha256(data).hexdigest() != EXPECTED_SHA:
        raise ValueError("MODEL_IDENTITY_MISMATCH: never substitute a portrait or unapproved source")
    if len(data) < 20 or data[:4] != b"glTF" or struct.unpack_from("<I", data, 4)[0] != 2:
        raise ValueError("NOT_GLB_2")
    if struct.unpack_from("<I", data, 8)[0] != len(data):
        raise ValueError("INVALID_DECLARED_FILE_LENGTH")
    json_size, json_type = struct.unpack_from("<I4s", data, 12)
    if json_type != b"JSON" or json_size <= 0 or 20 + json_size > len(data):
        raise ValueError("INVALID_GLB_JSON_CHUNK")
    info = json.loads(data[20:20 + json_size])
    skins = info.get("skins", [])
    if len(skins) != 1 or len(skins[0].get("joints", [])) != 26:
        raise ValueError("RIG_NOT_APPROVED_26_JOINTS")
    if len(info.get("meshes", [])) != 10:
        raise ValueError("UNEXPECTED_MESH_COUNT")
    if {clip.get("name") for clip in info.get("animations", [])} != EXPECTED_CLIPS:
        raise ValueError("APPROVED_FIVE_ANIMATIONS_MISSING")
    for mesh in info["meshes"]:
        for primitive in mesh.get("primitives", []):
            attributes = primitive.get("attributes", {})
            if "JOINTS_0" not in attributes or "WEIGHTS_0" not in attributes:
                raise ValueError("UNSKINNED_MESH")
    for clip in info["animations"]:
        if not clip.get("channels") or not clip.get("samplers"):
            raise ValueError("EMPTY_ANIMATION: " + clip["name"])
    print("ALDRIC_APPROVED_GLB_PASS SHA256/size/GLB2/26 joints/10 meshes/5 animated clips")
    print("UNITY_PLAYMODE_AND_ANDROID_NOT_CERTIFIED")

if __name__ == "__main__":
    try:
        verify(Path(sys.argv[1]) if len(sys.argv) > 1 else MODEL)
    except (OSError, ValueError, KeyError, IndexError, json.JSONDecodeError) as error:
        print("ALDRIC_APPROVED_GLB_FAIL", str(error), file=sys.stderr)
        sys.exit(2)
