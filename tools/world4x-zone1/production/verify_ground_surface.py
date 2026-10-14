"""Audit the exact baked source asset and genuine matched images; never certify beauty or Unity."""
import argparse, hashlib, json, pathlib, struct
from PIL import Image, ImageChops, ImageStat

def digest(path):
    return hashlib.sha256(path.read_bytes()).hexdigest()

def audit(folder):
    execution = json.loads((folder / 'surface-execution.json').read_text())
    baseline = json.loads((folder / 'normal-refresh-proof.json').read_text())
    assert execution['camera_matrix'] == baseline['camera_matrix'], 'Camera moved'
    assert execution['geometry_unchanged'] and baseline['geometry_unchanged']
    assert execution['artist_plan_sha256'] == digest(folder / 'artist-plan.json'), 'Artist decision changed'
    model = folder / 'ground-surface.glb'
    assert digest(model) == execution['model_sha256'], 'Export hash differs'
    raw = model.read_bytes()
    magic, version, length = struct.unpack_from('<III', raw)
    assert magic == 0x46546c67 and version == 2 and length == len(raw)
    size, kind = struct.unpack_from('<II', raw, 12)
    assert kind == 0x4e4f534a
    gltf = json.loads(raw[20:20+size])
    terrain = [node for node in gltf['nodes'] if node.get('name') == 'ZoneOne continuous prairie source']
    assert len(terrain) == 1 and 'mesh' in terrain[0], 'Named terrain mesh absent'
    primitives = gltf['meshes'][terrain[0]['mesh']]['primitives']
    triangles = 0
    for primitive in primitives:
        assert primitive.get('mode', 4) == 4
        assert {'POSITION', 'NORMAL', 'TEXCOORD_0'} <= primitive['attributes'].keys()
        triangles += gltf['accessors'][primitive['indices']]['count'] // 3
        material = gltf['materials'][primitive['material']]
        assert 'baseColorTexture' in material['pbrMetallicRoughness']
        assert 'normalTexture' in material
    assert all('uri' not in image for image in gltf.get('images', [])), 'External image dependency'
    images = []
    for entry in execution['textures']:
        path = folder / entry['path']
        assert digest(path) == entry['sha256']
        with Image.open(path) as texture:
            assert texture.size == (1024, 1024)
            texture.load()
            images.append({'path': path.name, 'bytes': path.stat().st_size,
                           'dimensions': list(texture.size), 'rgb_stddev': ImageStat.Stat(texture.convert('RGB')).stddev})
    frames = []
    for name in ['geometry-normal-fixed.png', 'blender-surface-after.png']:
        with Image.open(folder / name) as image:
            frames.append(image.convert('RGB'))
    assert frames[0].size == frames[1].size, 'Capture dimensions differ'
    difference = ImageChops.difference(*frames)
    changed = sum(max(pixel) > 5 for pixel in difference.getdata())
    total = frames[0].width * frames[0].height
    assert changed / total >= .001, 'Source change imperceptible; reject before expensive native gate'
    return {'image_type': 'MATCHED_BLENDER_CYCLES_NOT_UNITY',
            'camera_equivalent': True, 'dimensions': list(frames[0].size),
            'before_sha256': digest(folder / 'geometry-normal-fixed.png'),
            'after_sha256': digest(folder / 'blender-surface-after.png'),
            'model_sha256': digest(model), 'terrain_triangles': triangles,
            'textures': images, 'changed_rgb_pixels_over_5': changed,
            'total_pixels': total, 'visibility_fraction': changed / total,
            'visibility_only_not_art_quality': True,
            'uncompressed_ground_texture_memory_bytes_without_mips': 2 * 1024 * 1024 * 4,
            'unity_executed': False, 'physical_device_performance_measured': False,
            'art_approved': False, 'weights_trained': False}

if __name__ == '__main__':
    parser = argparse.ArgumentParser()
    parser.add_argument('--work-dir', type=pathlib.Path, required=True)
    args = parser.parse_args()
    result = audit(args.work_dir)
    (args.work_dir / 'surface-technical-review.json').write_text(json.dumps(result, indent=2) + '\n')
    print(json.dumps(result, indent=2))
