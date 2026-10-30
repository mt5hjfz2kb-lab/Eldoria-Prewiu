#!/usr/bin/env python3
"""Prepare missing normal streams for the exact approved GLB; never rewrite it.

glTFast 6.14.1 rejects Position+Tangent without Normal when a normal-mapped
material meets a source mesh with no NORMAL accessor. This is importer data,
not a model replacement: all existing binary bytes, positions, indices, UVs,
materials, textures, skin and animation remain unchanged.
"""
import argparse
import copy
import hashlib
import json
import math
from pathlib import Path
import struct

ROOT = Path(__file__).resolve().parents[2]
SOURCE = ROOT / 'Unity/Assets/StreamingAssets/Eldoria/Aldric/SirAldric_Approved_RiggedAnimated_v1.glb'
OUTPUT = ROOT / 'Unity/Assets/Eldoria/AldricCity/Resources/AldricImportNormals'
PROOF = ROOT / 'docs/aldric-city/normal-import-proof.json'
SOURCE_SHA = 'c28a65b6040225256ae047d8fd773849771895ac378041e0882f279517e78c8b'


def sha(data):
    return hashlib.sha256(data).hexdigest()


def prepare(source):
    data = source.read_bytes()
    assert len(data) == 10140372 and sha(data) == SOURCE_SHA, 'Unapproved source'
    assert struct.unpack_from('<III', data) == (0x46546c67, 2, len(data))
    json_size, json_kind = struct.unpack_from('<II', data, 12)
    assert json_kind == 0x4e4f534a
    original = json.loads(data[20:20 + json_size])
    bin_header = 20 + json_size
    bin_size, bin_kind = struct.unpack_from('<II', data, bin_header)
    assert bin_kind == 0x004e4942 and bin_header + 8 + bin_size == len(data)
    binary = data[bin_header + 8:]
    assert len(original['buffers']) == 1 and original['buffers'][0]['byteLength'] <= bin_size
    result = copy.deepcopy(original)
    streams = bytearray()
    report = []

    def read_accessor(index):
        accessor = original['accessors'][index]
        view = original['bufferViews'][accessor['bufferView']]
        assert view.get('buffer', 0) == 0 and 'sparse' not in accessor
        types = {5121: ('B', 1), 5123: ('H', 2), 5125: ('I', 4), 5126: ('f', 4)}
        fmt, size = types[accessor['componentType']]
        width = {'SCALAR': 1, 'VEC3': 3}[accessor['type']]
        stride = view.get('byteStride', width * size)
        offset = view.get('byteOffset', 0) + accessor.get('byteOffset', 0)
        assert stride >= width * size
        values = [struct.unpack_from('<' + fmt * width, binary, offset + i * stride)
                  for i in range(accessor['count'])]
        return accessor, values

    for mesh in result['meshes']:
        for primitive in mesh['primitives']:
            assert primitive.get('mode', 4) == 4 and 'NORMAL' not in primitive['attributes']
            position, vertices = read_accessor(primitive['attributes']['POSITION'])
            assert position['componentType'] == 5126 and position['type'] == 'VEC3'
            _, packed_indices = read_accessor(primitive['indices'])
            indices = [x[0] for x in packed_indices]
            assert len(indices) % 3 == 0 and all(0 <= i < len(vertices) for i in indices)
            normals = [[0.0, 0.0, 0.0] for _ in vertices]
            for at in range(0, len(indices), 3):
                ia, ib, ic = indices[at:at + 3]
                a, b, c = vertices[ia], vertices[ib], vertices[ic]
                u = [b[k] - a[k] for k in range(3)]
                v = [c[k] - a[k] for k in range(3)]
                normal = (u[1]*v[2]-u[2]*v[1], u[2]*v[0]-u[0]*v[2], u[0]*v[1]-u[1]*v[0])
                for i in (ia, ib, ic):
                    for k in range(3):
                        normals[i][k] += normal[k]
            start = len(streams)
            unused_or_degenerate = 0
            for normal in normals:
                length = math.sqrt(sum(x*x for x in normal))
                if length < 1e-18:
                    unused_or_degenerate += 1
                    normal = (0.0, 1.0, 0.0)
                else:
                    normal = tuple(x / length for x in normal)
                assert all(math.isfinite(x) for x in normal)
                streams.extend(struct.pack('<fff', *normal))
            view_index = len(result['bufferViews'])
            result['bufferViews'].append({'buffer': 0, 'byteOffset': bin_size + start,
                                         'byteLength': len(streams) - start, 'target': 34962})
            accessor_index = len(result['accessors'])
            result['accessors'].append({'bufferView': view_index, 'componentType': 5126,
                                        'count': len(vertices), 'type': 'VEC3'})
            primitive['attributes']['NORMAL'] = accessor_index
            report.append({'mesh': mesh['name'], 'vertices': len(vertices),
                           'triangles': len(indices)//3,
                           'unused_or_degenerate_normals': unused_or_degenerate})
    assert len(report) == 10 and len(streams) % 4 == 0
    result['buffers'][0]['byteLength'] = bin_size + len(streams)

    # Explicitly prove that every original semantic field survives untouched.
    restored = copy.deepcopy(result)
    for mesh in restored['meshes']:
        for primitive in mesh['primitives']:
            del primitive['attributes']['NORMAL']
    restored['accessors'] = restored['accessors'][:len(original['accessors'])]
    restored['bufferViews'] = restored['bufferViews'][:len(original['bufferViews'])]
    restored['buffers'] = copy.deepcopy(original['buffers'])
    assert restored == original, 'Any non-normal source change is forbidden'

    scene = json.dumps(result, separators=(',', ':'), ensure_ascii=False).encode('utf8')
    padded = scene + b' ' * (-len(scene) % 4)
    combined_binary = binary + streams
    derived = (struct.pack('<III', 0x46546c67, 2, 28 + len(padded) + len(combined_binary)) +
               struct.pack('<II', len(padded), 0x4e4f534a) + padded +
               struct.pack('<II', len(combined_binary), 0x004e4942) + combined_binary)
    assert combined_binary[:len(binary)] == binary
    proof = {'source_sha256': SOURCE_SHA, 'source_bytes': len(data),
             'normal_stream_sha256': sha(streams), 'normal_stream_bytes': len(streams),
             'import_scene_sha256': sha(scene), 'import_scene_bytes': len(scene),
             'transient_import_sha256': sha(derived), 'transient_import_bytes': len(derived),
             'original_binary_prefix_unchanged': True,
             'all_original_json_semantics_unchanged': True,
             'added_attributes_only': 'NORMAL', 'mesh_streams': report,
             'validation_scope': 'SOURCE_EQUIVALENCE_ONLY', 'physical_mobile_tested': False}
    return {OUTPUT / 'normal-stream.bytes': bytes(streams),
            OUTPUT / 'normal-import-scene.json': scene,
            PROOF: (json.dumps(proof, indent=2) + '\n').encode()}, proof


if __name__ == '__main__':
    parser = argparse.ArgumentParser()
    parser.add_argument('--source', type=Path, default=SOURCE)
    parser.add_argument('--write', action='store_true')
    args = parser.parse_args()
    files, proof = prepare(args.source)
    for path, data in files.items():
        if args.write:
            path.parent.mkdir(parents=True, exist_ok=True)
            path.write_bytes(data)
        else:
            assert path.read_bytes() == data, 'Import normal stream differs: ' + str(path)
    print('ALDRIC_NORMAL_IMPORT_SOURCE_PASS: original geometry/materials/rig/clips/binary preserved')
    print(json.dumps({k: v for k, v in proof.items() if k != 'mesh_streams'}))
