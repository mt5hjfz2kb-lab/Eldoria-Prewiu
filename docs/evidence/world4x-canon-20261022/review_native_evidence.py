"""Verify an unpacked genuine World4X review artifact; never alter its images.
Python 3 stdlib; optional AG comparison requires Pillow and NumPy.
"""
import argparse
import hashlib
import json
from pathlib import Path
import re
import struct
import xml.etree.ElementTree as ET


def review(root, expected_count=126, expected_tests=57, ag_root=None):
    root = Path(root)
    images = {}
    for metadata in root.rglob('*.json'):
        if 'original-ag-review' in metadata.parts:
            continue
        try:
            item = json.loads(metadata.read_text(encoding='utf-8-sig'))
        except (ValueError, UnicodeError):
            continue
        if not isinstance(item, dict) or item.get('actualUnityCamera') is not True:
            continue
        assert item.get('playMode') is True, metadata
        assert item.get('version') == '6000.3.23f1', metadata
        assert item.get('sourceScene') == 'Assets/Eldoria/ArtTests/ValoriaReferenceClean/Integration/Valoria_City_World_UI.unity', metadata
        candidates = [metadata.with_suffix('.png'), metadata.with_name(metadata.name.replace('-source.json', '-unity.png'))]
        png = next((p for p in candidates if p.is_file()), None)
        assert png is not None, f'Missing PNG for {metadata}'
        data = png.read_bytes()
        assert data[:8] == b'\x89PNG\r\n\x1a\n', png
        width, height = struct.unpack('>II', data[16:24])
        assert (width, height) == (item['width'], item['height']), png
        digest = hashlib.sha256(data).hexdigest()
        assert digest == item['sha256'], f'PNG identity mismatch: {png}'
        assert png not in images, f'Duplicate image metadata: {png}'
        images[png] = {'path': str(png.relative_to(root)), 'sha256': digest, 'width': width, 'height': height}
    assert len(images) == expected_count, f'Expected {expected_count} current PNG identities, got {len(images)}'
    xml = root / 'eldoria-region1-canon-tests.xml'
    suite = ET.parse(xml).getroot()
    assert suite.get('result') == 'Passed', suite.attrib
    assert int(suite.get('passed', '0')) >= expected_tests, suite.attrib
    for name in ('failed', 'skipped', 'inconclusive'):
        assert int(suite.get(name, '0')) == 0, suite.attrib
    cases = list(suite.iter('test-case'))
    assert len(cases) == int(suite.get('total')), suite.attrib
    assert all(case.get('result') == 'Passed' for case in cases)
    cities = re.findall(r'REGION1_ACTUAL_CITY_RESERVES valid=(\d+) accessible=(\d+)', xml.read_text(encoding='utf-8-sig'))
    assert len(cities) == 1 and cities[0][0] == cities[0][1], cities
    report = {
        'kind': 'native_editor_evidence_verification',
        'unity': '6000.3.23f1', 'current_png_identities_verified': len(images),
        'tests_passed': int(suite.get('passed')), 'tests_failed': 0, 'tests_skipped': 0,
        'editor_suite_seconds_not_mobile_fps': float(suite.get('duration').replace(',', '.')),
        'accessible_city_reserves': int(cities[0][1]),
        'xml_sha256': hashlib.sha256(xml.read_bytes()).hexdigest(),
        'art_approval': False, 'physical_mobile_benchmark': False,
        'images': sorted(images.values(), key=lambda item: item['path']),
    }
    footprint = re.findall(r'REGION1_ACTUAL_PANORAMA_VIEWPORT_AREA (horizontal|vertical) ([0-9.]+)', xml.read_text(encoding='utf-8-sig'))
    if footprint:
        assert len(footprint) == 2 and {name for name, _ in footprint} == {'horizontal', 'vertical'}, footprint
        assert all(.65 < float(value) <= 1 for _, value in footprint), footprint
        report['actual_unity_panorama_projected_bbox_area'] = {name: float(value) for name, value in footprint}
    if expected_tests >= 57:
        manifests = list(root.rglob('general-proof.json'))
        assert len(manifests) == 1, manifests
        proof_path = manifests[0]
        proof = json.loads(proof_path.read_text(encoding='utf-8-sig'))
        assert proof['gpu_playmode_passed'] == int(suite.get('passed'))
        assert proof['editmode_passed'] >= 26 and proof['probe_checks'] >= 44
        assert proof['fresh_captures'] == len(proof['images']) >= 34
        assert {p.name for p in proof_path.parent.glob('*.png')} == {item['file'] for item in proof['images']}
        for item in proof['images']:
            data = (proof_path.parent / item['file']).read_bytes()
            assert data[:8] == b'\x89PNG\r\n\x1a\n'
            assert hashlib.sha256(data).hexdigest() == item['sha256']
        edit = ET.parse(proof_path.parent / 'editmode.xml').getroot()
        assert edit.get('result') == 'Passed' and int(edit.get('passed')) == proof['editmode_passed']
        assert all(case.get('result') == 'Passed' for case in edit.iter('test-case'))
        assert (proof_path.parent / 'playmode.xml').read_bytes() == xml.read_bytes()
        native = json.loads((proof_path.parent / 'integration-result.json').read_text(encoding='utf-8-sig'))
        assert native['source'] == proof['source'] and native['status'] == 'INTEGRATION_PASS_ART_REVIEW_REQUIRED'
        assert len(native['checks']) == proof['probe_checks'] and not native['error']
        report['general_native_certificate'] = proof
    if ag_root:
        import numpy as np
        from PIL import Image
        comparison = {}
        for name in ('landscape-close.png', 'portrait-close.png'):
            originals = list(Path(ag_root).rglob(name))
            current = [p for p in images if p.name == name and p.parent.name == 'retained-sector']
            assert len(originals) == len(current) == 1, (originals, current)
            a = np.array(Image.open(originals[0]).convert('RGB'), dtype=float)
            b = np.array(Image.open(current[0]).convert('RGB'), dtype=float)
            assert a.shape == b.shape
            difference = np.abs(a - b)
            comparison[name] = {
                'original_png_sha256': hashlib.sha256(originals[0].read_bytes()).hexdigest(),
                'current_png_sha256': images[current[0]]['sha256'],
                'mae_rgb_0_to_255': float(difference.mean()),
                'fraction_pixels_any_channel_difference_above_8': float((difference.max(axis=2) > 8).mean()),
                'meaning': 'matched-camera conservation measurement, not an artistic quality score',
            }
        report['matched_ag_close_comparison'] = comparison
    return report


if __name__ == '__main__':
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('root', type=Path)
    parser.add_argument('--ag-root', type=Path)
    parser.add_argument('--expected-count', type=int, default=126)
    parser.add_argument('--expected-tests', type=int, default=57)
    parser.add_argument('--output', type=Path)
    args = parser.parse_args()
    result = review(args.root, args.expected_count, args.expected_tests, args.ag_root)
    text = json.dumps(result, indent=2) + '\n'
    if args.output:
        args.output.write_text(text, encoding='utf-8')
    print(json.dumps({k: v for k, v in result.items() if k != 'images'}, indent=2))
