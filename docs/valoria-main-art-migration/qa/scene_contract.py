"""Print the serialized camera, collider and hotspot contract for a Unity scene."""
import argparse
import hashlib
import json
import re
import subprocess

SCENE = "Unity/Assets/Eldoria/ArtTests/ValoriaReferenceClean/Integration/Valoria_City_World_UI.unity"
HOTSPOT = "Unity/Assets/Eldoria/Scripts/Presentation/WorldHotspot.cs.meta"


def source(revision, path):
    return subprocess.check_output(["git", "show", f"{revision}:{path}"]).decode()


def field(document, name):
    result = re.search(rf"^  {re.escape(name)}: (.*)$", document, re.MULTILINE)
    if result is None:
        raise ValueError(f"Missing serialized field {name}")
    return result[1]


def file_id(document, name):
    return re.fullmatch(r"\{fileID: (-?\d+)\}", field(document, name))[1]


def scene_contract(revision):
    scene = source(revision, SCENE)
    guid = re.search(r"^guid: (\w+)$", source(revision, HOTSPOT), re.MULTILINE)[1]
    documents = {
        key: (int(kind), body)
        for kind, key, body in re.findall(
            r"^--- !u!(\d+) &(-?\d+)\n(.*?)(?=^--- !u!|\Z)", scene, re.MULTILINE | re.DOTALL
        )
    }
    transforms = {file_id(body, "m_GameObject"): key for key, (kind, body) in documents.items() if kind in (4, 224)}
    classes = {20: "Camera", 64: "MeshCollider", 65: "BoxCollider", 135: "SphereCollider", 136: "CapsuleCollider"}
    rows = []
    for key, (kind, body) in documents.items():
        hotspot = kind == 114 and guid in field(body, "m_Script")
        if kind not in classes and not hotspot:
            continue
        game_object = file_id(body, "m_GameObject")
        transform = transforms[game_object]
        chain = []
        while transform != "0":
            transform_body = documents[transform][1]
            object_body = documents[file_id(transform_body, "m_GameObject")][1]
            chain.append({
                "fileID": transform,
                "name": field(object_body, "m_Name"),
                "layer": field(object_body, "m_Layer"),
                "activeSelf": field(object_body, "m_IsActive"),
                "localPosition": field(transform_body, "m_LocalPosition"),
                "localRotation": field(transform_body, "m_LocalRotation"),
                "localScale": field(transform_body, "m_LocalScale"),
            })
            transform = file_id(transform_body, "m_Father")
        rows.append({
            "fileID": key,
            "type": "WorldHotspot" if hotspot else classes[kind],
            "id": field(body, "Id") if hotspot else None,
            "serializedSha256": hashlib.sha256(body.encode()).hexdigest(),
            "hierarchy": chain[::-1],
        })
    if not rows or not any(row["type"] == "Camera" for row in rows):
        raise ValueError("No applicable scene data")
    return sorted(rows, key=lambda row: int(row["fileID"]))


if __name__ == "__main__":
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("revision")
    print(json.dumps(scene_contract(parser.parse_args().revision), ensure_ascii=False, indent=2))
