#!/usr/bin/env python3
"""Stage *existing* 63-model Blender library and verified preferred versions.

No model is generated, modified, remeshed, trained or downloaded from unapproved
sources. Inputs are pinned GitHub Actions artifacts, staged by the workflow.
Only copies are written inside the separately claimed clean-scene directory.
"""
import argparse
import hashlib
import json
import pathlib
import shutil
import struct

NAMES = """
arch_bridge_support arch_complete_major arch_complete_minor arch_keystone_major
arch_keystone_minor arch_voussoir_major arch_voussoir_minor arrow_slit_frame
banner_royal_blue banner_small boulder_1 boulder_2 boulder_3 brazier_pedestal
bridge_span_4m cliff_rock_column cliff_stone_chip cobble_pebbles dirt_ground_patch
grass_ground_patch grass_tuft_1 grass_tuft_2 merlon_low merlon_tall
mortar_joint_recess parapet_walk_4m paver_flagstone paver_rect_long paver_rect_short
pine_medium pine_small pine_tall portcullis_large portcullis_small
reinforced_oak_gate river_foam_line river_water_tile scaffold_small
scaffold_watch_platform shrub_1 shrub_2 shrub_3 stair_step staircase_flight_12
stone_ashlar_01 stone_ashlar_02 stone_ashlar_03 stone_ashlar_04 stone_corner_quoin
stone_half_block stone_slab_cap timber_beam_1 timber_beam_2 timber_brace_X
torch_wall tower_square_short tower_square_tall wall_buttress wall_corner_90
wall_course_4m wall_panel_crenellated_4m wall_panel_regular_4m wall_panel_ruined_4m
""".split()
assert len(NAMES) == len(set(NAMES)) == 63

BASE_RUN = 38813966671
BASE_ARTIFACT = "valoria-individual-quality-63-38813966671"
# Sourced from the prior immutable per-asset selection review. This is *not*
# a claim that each is the newest version, nor a claim of 8/10 art approval.
PREFERRED = {
  "reinforced_oak_gate": ("gate", "eldoria-cc0-real-gate-ab/glb/reinforced_oak_gate.glb",
      "6b1ad6f41c0ce11341281caf6d9ba21438771119443ced089f6f5de691e94a98",38858438155),
  "portcullis_large": ("iron", "valoria-black-forged-two/glb/portcullis_large.glb",
      "09492e60b841e27bf4b65c532b9b973ec0e2dd1cebb2738beafabcbde52f3c1d",38860266981),
  "portcullis_small": ("iron", "valoria-black-forged-two/glb/portcullis_small.glb",
      "f74af38088acb53ee6561699c9fd610fd836687185cbbe1558e57e50b4d55809",38860266981),
  "wall_panel_regular_4m": ("wall", "eldoria-4m-wall-architectural-v6/glb/wall_panel_regular_4m_warm_limestone.glb",
      "928630408d47a1789a2d0e5080e40348979dc8338f4cc91ea8a3a64ba93257f3",38865543613),
  "pine_medium": ("pine", "eldoria-one-full-medium-pine/glb/pine_medium.glb",
      "003d59a9d184cb6cb04b2b2b21a8cb35c2b92e838cf788575044e576f7846e3e",38877895047),
  "cliff_rock_column": ("cliff", "eldoria-rockface-palette-02/glb/cliff_rock_column_stone_neutral.glb",
      "614b2f1db74d66cfc6c3ffc759dbc62083e434de107806b7ee47f2669df70447",38826318218),
}
# Extra unchanged base source SHA pins observed in the previous Unity A/B.
BASE_PINS = {
 "tower_square_short": "a514d97fc3241477f4b98b7e4d04b1213d75a1718e53d662fca9e83be8dfd4ac",
 "merlon_tall": "022bf407bcfeeb7e717874b0a27def4e3f9831bb0c6edfb1933bc99f4f447bb9",
 "wall_buttress": "f4d4d325106eae4f5c3e5c9a5e3f1433eecb8ca6bb063bd812ccd73101a53d64",
 "wall_panel_crenellated_4m": "476d4cd3bb9d220845489833b3386311c1994b6db55ab3353571cf564a6dd05f"
}

def glb_metadata(p):
    data = p.read_bytes()
    if len(data) < 100 or data[:4] != b"glTF":
        raise ValueError(f"Not an actual GLB: {p}")
    magic, version, size = struct.unpack_from("<4sII", data, 0)
    if version != 2 or size != len(data):
        raise ValueError(f"Invalid GLB version or length: {p}")
    chunk_size, chunk_type = struct.unpack_from("<II", data, 12)
    if chunk_type != 0x4E4F534A:
        raise ValueError(f"No GLB JSON chunk: {p}")
    obj = json.loads(data[20:20+chunk_size])
    if not obj.get("meshes"):
        raise ValueError(f"Empty 3D geometry: {p}")
    materials = []
    for raw in obj.get("materials", []):
        material = dict(raw)
        pbr = dict(material.get("pbrMetallicRoughness", {}))
        for key, value in (
           ("baseColorFactor", [1,1,1,1]),
           ("metallicFactor", 1),
           ("roughnessFactor", 1)):
            pbr.setdefault(key,value)
        for key in ("baseColorTexture", "metallicRoughnessTexture"):
            pbr.setdefault(key, {"index": -1})
        material["pbrMetallicRoughness"] = pbr
        material.setdefault("normalTexture", {"index": -1, "scale": 1})
        material["normalTexture"].setdefault("scale", 1)
        material.setdefault("alphaMode", "OPAQUE")
        material.setdefault("alphaCutoff", .5)
        material.setdefault("emissiveFactor", [0,0,0])
        material.setdefault("doubleSided", False)
        materials.append(material)
    return len(obj["meshes"]), len(obj.get("nodes",[])), materials

def main():
    parser = argparse.ArgumentParser()
    parser.add_argument("--artifact-root",required=True,type=pathlib.Path)
    parser.add_argument("--project-root",required=True,type=pathlib.Path)
    args = parser.parse_args()
    artifacts = args.artifact_root.resolve()
    source = artifacts / "base63"
    candidates = list(source.rglob("*.glb"))
    by_name = {}
    for p in candidates:
        if p.stem in NAMES:
            if p.stem in by_name:
                raise RuntimeError(f"Duplicate 63 base model: {p.stem}")
            by_name[p.stem] = p
    if set(by_name) != set(NAMES) or len(candidates) != 63:
        raise RuntimeError(f"Expected exactly 63 original GLBs: found {len(candidates)}, matched {len(by_name)}")
    out = args.project_root / "Unity/Assets/Eldoria/ArtTests/Valoria63CleanScene/Sources"
    out.mkdir(parents=True,exist_ok=True)
    review = args.project_root / "docs/evidence/valoria63-clean-scene-20261016"
    review.mkdir(parents=True,exist_ok=True)
    manifest = []
    for name in NAMES:
        src, run, kind = by_name[name], BASE_RUN, "preserved_base_63"
        if name in PREFERRED:
            key, rel, expected, run = PREFERRED[name]
            src = artifacts / key / pathlib.Path(rel)
            if not src.is_file():
                raise FileNotFoundError(f"Missing preferred approved-for-trial source {name}: {src}")
            kind = "documented_visually_preferred_not_final_approval"
            actual = hashlib.sha256(src.read_bytes()).hexdigest()
            if actual != expected:
                raise ValueError(f"Nonmatching immutable source {name}: {actual}")
        else:
            if name in BASE_PINS:
                actual = hashlib.sha256(src.read_bytes()).hexdigest()
                if actual != BASE_PINS[name]:
                    raise ValueError(f"Base source mismatch for {name}: {actual}")
        count, nodes, mats = glb_metadata(src)
        dest = out / (name+".glb")
        shutil.copy2(src,dest)
        (out / (name+"-material-bindings.json")).write_text(
            json.dumps({"materials":mats},indent=2)+"\n",encoding="utf-8")
        manifest.append({"id":name,"source_type":kind,"run_id":run,
            "artifact_name":BASE_ARTIFACT if run == BASE_RUN else PREFERRED[name][0],
            "sha256":hashlib.sha256(dest.read_bytes()).hexdigest(),
            "mesh_count":count,"node_count":nodes,"size_bytes":dest.stat().st_size})
    (review/"source-manifest.json").write_text(
        json.dumps({"assets":manifest,"expected_count":63,"staged_count":len(manifest),
         "base_run":BASE_RUN,"no_new_models":True,"art_approved":False},indent=2)+"\n",
        encoding="utf-8")
    print(f"VALORIA63_SOURCE_STAGED verified=63 preferred_overrides={len(PREFERRED)} "
          f"no_reauthoring=true stage={out}")

if __name__ == "__main__":
    main()
