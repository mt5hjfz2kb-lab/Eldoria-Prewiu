"""Compare the actual packaged Android ASTC illustrations to exact source PNGs/JPEGs.

Reads only APK and build-proof artifacts. No engine state or source edits.
Requires UnityPy, Pillow and NumPy. Reports metrics for visual review, not artistic approval.
"""
import argparse, hashlib, json, math, re, zipfile
from collections import defaultdict
from pathlib import Path
import numpy as np
from PIL import Image, ImageDraw
import UnityPy
from UnityPy.enums import TextureFormat

def compare(original, decoded):
    a = np.asarray(original.convert('RGBA'), dtype=np.float32) / 255
    b = np.asarray(decoded.convert('RGBA'), dtype=np.float32) / 255
    errors = []
    for backdrop in (0, 1):
        x = a[..., :3] * a[..., 3:] + backdrop * (1 - a[..., 3:])
        y = b[..., :3] * b[..., 3:] + backdrop * (1 - b[..., 3:])
        errors.append(np.abs(x - y))
    worst = np.maximum(*errors)
    mse = float(np.mean(worst ** 2))
    return {'psnr_db': 99.0 if mse == 0 else -10 * math.log10(mse),
            'mean_error_255': float(worst.mean() * 255),
            'p99_error_255': float(np.percentile(worst, 99) * 255),
            'alpha_mean_error_255': float(np.abs(a[..., 3]-b[..., 3]).mean() * 255)}

def main():
    parser = argparse.ArgumentParser()
    parser.add_argument('apk', type=Path)
    parser.add_argument('proof', type=Path)
    parser.add_argument('out', type=Path)
    args = parser.parse_args()
    report_files = list(args.proof.rglob('android-build.json'))
    assert len(report_files) == 1, 'Require one exact build report'
    report = json.loads(report_files[0].read_text(encoding='utf-8-sig'))
    assert report['status'] == 'APK_COMPILED_NOT_DEVICE_VALIDATED'
    assert report['source_texture_metadata_restored'] is True
    assert hashlib.sha256(args.apk.read_bytes()).hexdigest() == report['apk_sha256']
    expected = report['android_textures']
    assert expected, 'No texture proof'
    wanted = {Path(x['path']).stem for x in expected}
    found = defaultdict(list)
    args.out.mkdir(parents=True, exist_ok=True)
    with zipfile.ZipFile(args.apk) as apk:
        for entry in apk.infolist():
            # Resources use independently serialized files in this Unity Android build.
            if not re.fullmatch(r'assets/bin/Data/[0-9a-f]{32}', entry.filename):
                continue
            if entry.file_size > 16 * 1024 * 1024:
                continue
            env = UnityPy.load(apk.read(entry))
            for obj in env.objects:
                if obj.type.name != 'Texture2D':
                    continue
                texture = obj.read()
                if texture.m_Name not in wanted:
                    continue
                fmt = TextureFormat(texture.m_TextureFormat).name
                if 'ASTC' not in fmt or '4x4' not in fmt:
                    continue
                found[texture.m_Name].append((entry.filename, obj.path_id, fmt, texture.image.copy()))
    comparisons = []
    images = []
    used = {}
    for item in expected:
        source_files = [p for p in args.proof.rglob(Path(item['path']).name)
                        if p.as_posix().endswith(item['path'].removeprefix('Assets/'))]
        assert len(source_files) == 1, item['path']
        source_file = source_files[0]
        assert hashlib.sha256(source_file.read_bytes()).hexdigest() == item['source_sha256']
        original = Image.open(source_file).convert('RGBA')
        expected_size = (item['width'], item['height'])
        source_size = original.size
        resized_reference = source_size != expected_size
        if resized_reference:
            # Manifest records the already-imported size BEFORE the Android override.
            # Require the same aspect and original max-size cap; this is an approximate
            # filtered reference, not an assertion of Unity's exact resize kernel.
            assert max(expected_size) == item['max_size']
            assert abs(source_size[0]/source_size[1] - expected_size[0]/expected_size[1]) < .003
            original = original.resize(expected_size, Image.Resampling.LANCZOS)
        candidates = []
        for entry, object_id, fmt, decoded in found[Path(item['path']).stem]:
            if decoded.size != expected_size:
                continue
            key = (entry, object_id)
            if key in used and used[key] != item['source_sha256']:
                continue
            metrics = compare(original, decoded)
            candidates.append((metrics['psnr_db'], entry, object_id, fmt, decoded, metrics))
        assert candidates, 'Missing full-resolution ASTC4x4 texture: ' + item['path']
        _, entry, object_id, fmt, decoded, metrics = max(candidates, key=lambda x: x[0])
        used[(entry, object_id)] = item['source_sha256']
        record = {**item, 'apk_entry': entry, 'object_id': object_id, 'actual_format': fmt, 'source_file_size': source_size, 'reference_resized_lanczos': resized_reference, **metrics}
        comparisons.append(record)
        images.append((metrics['psnr_db'], item['path'], original, decoded))
    result = {'source_sha': report['source_sha'], 'apk_sha256': report['apk_sha256'],
              'apk_bytes': args.apk.stat().st_size, 'textures_checked': len(comparisons),
              'all_dimensions_retained': True, 'all_source_hashes_verified': True,
              'all_actual_formats_astc4x4': True, 'artistic_approval': False,
              'physical_phone_tested': False, 'textures': comparisons}
    (args.out/'texture-comparison.json').write_text(json.dumps(result, indent=2)+'\n')
    for index, (_, name, original, decoded) in enumerate(sorted(images, key=lambda x: x[0])[:6]):
        # Identical central crops at native pixels reveal fine-detail compression.
        w,h=original.size; side=min(512,w,h);box=((w-side)//2,(h-side)//2,(w+side)//2,(h+side)//2)
        canvas=Image.new('RGB',(side*2,side+48),(36,36,36));draw=ImageDraw.Draw(canvas)
        draw.text((8,8),'SOURCE / '+Path(name).name,fill='white');draw.text((side+8,8),'ACTUAL APK ASTC4x4',fill='white')
        for x,img in ((0,original),(side,decoded)):
            crop=img.crop(box);canvas.paste(crop,(x,48),crop)
        canvas.save(args.out/f'comparison-{index+1}.png')
    print(json.dumps({k:v for k,v in result.items() if k!='textures'}))
    print(json.dumps(sorted(comparisons,key=lambda x:x['psnr_db'])[:6],indent=2))

if __name__ == '__main__':
    main()
