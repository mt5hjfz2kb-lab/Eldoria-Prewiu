"""Additional spatial art evidence: real same-size frames only; no inferred scores."""
import argparse
import hashlib
import json
from pathlib import Path
from PIL import Image, ImageDraw

SCREENS = {
    'bastion-1': 'bastion-1.png', 'bastion-2': 'bastion-2.png',
    'bastion-3': 'bastion-3.png', 'bastion-4': 'bastion-4.png',
    'bastion-5': 'bastion-5-completo.png', 'bastion-5-panorama': 'bastion-5-panorama.png',
    'region-entry': 'ciudad-a-region-5.png', 'region-pan': 'region-1-pan-real-bastion-5.png',
    'region-zoom': 'region-1-pinch-real-bastion-5.png',
}

def digest(p):
    return hashlib.sha256(p.read_bytes()).hexdigest()

def main():
    p = argparse.ArgumentParser()
    for name in ('baseline', 'candidate', 'out'):
        p.add_argument(name, type=Path)
    p.add_argument('--baseline-sha', required=True)
    p.add_argument('--candidate-sha', required=True)
    a = p.parse_args()
    if a.baseline_sha == a.candidate_sha:
        raise ValueError('The compared source revisions must differ')
    for sha in (a.baseline_sha, a.candidate_sha):
        if len(sha) != 40 or any(c not in '0123456789abcdef' for c in sha):
            raise ValueError('Expected a full Git SHA')
    a.out.mkdir(parents=True, exist_ok=True)
    manifest = {'baseline_sha': a.baseline_sha, 'candidate_sha': a.candidate_sha,
                'pairs': [], 'missing': [], 'visual_approval': False,
                'limitation': 'Input source SHAs must be corroborated with Actions metadata; camera, save and geometry must be matched separately.'}
    for role, filename in SCREENS.items():
        left, right = a.baseline / filename, a.candidate / filename
        if not left.is_file() or not right.is_file():
            manifest['missing'].append(role)
            continue
        with Image.open(left) as l, Image.open(right) as r:
            if l.size != r.size or l.size != (390, 844):
                manifest['missing'].append(role + ': size mismatch')
                continue
            w, h = l.size
            pair = Image.new('RGB', (2*w+12, h+26), '#172028')
            pair.paste(l.convert('RGB'), (0,26)); pair.paste(r.convert('RGB'), (w+12,26))
            draw = ImageDraw.Draw(pair)
            draw.text((6,5), 'BASELINE / '+role, fill='white')
            draw.text((w+18,5), 'CANDIDATE / '+role, fill='white')
            dest = a.out / (role+'.png')
            pair.save(dest)
            manifest['pairs'].append({'role':role, 'file':dest.name, 'size':[w,h],
                                      'baseline_sha256':digest(left), 'candidate_sha256':digest(right)})
    (a.out/'manifest.json').write_text(json.dumps(manifest, indent=2), encoding='utf-8')
    if manifest['missing']:
        raise SystemExit('Incomplete spatial art evidence: '+str(manifest['missing']))
    print(f"{len(manifest['pairs'])} actual spatial pairs; independent visual judgment required.")

if __name__ == '__main__':
    main()
