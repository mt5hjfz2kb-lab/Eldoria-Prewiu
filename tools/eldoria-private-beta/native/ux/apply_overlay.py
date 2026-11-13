"""Apply the reviewed frozen UX sources transactionally after hash verification."""
import argparse, hashlib, json
from pathlib import Path

def apply(source, manifest_path, existing_only=False):
    source=Path(source).resolve();manifest_path=Path(manifest_path).resolve()
    manifest=json.loads(manifest_path.read_text());pending=[]
    for item in manifest['files']:
        target=(source/item['path']).resolve();overlay=(manifest_path.parent/item['overlay']).resolve()
        if not target.is_relative_to(source) or not overlay.is_relative_to(manifest_path.parent):
            raise ValueError('Path outside isolated source/overlay')
        if existing_only and not target.exists(): continue
        before=target.read_bytes();candidate=overlay.read_bytes()
        if hashlib.sha256(before).hexdigest()!=item['baseline_sha256']:
            raise ValueError('Unexpected source: '+item['path'])
        if hashlib.sha256(candidate).hexdigest()!=item['candidate_sha256']:
            raise ValueError('Unexpected candidate: '+item['path'])
        pending.append((target,candidate))
    if not pending: raise ValueError('No sources selected')
    for target,candidate in pending: target.write_bytes(candidate)
    return len(pending)
if __name__=='__main__':
    parser=argparse.ArgumentParser();parser.add_argument('--source',required=True)
    parser.add_argument('--existing-only',action='store_true')
    args=parser.parse_args()
    print('Verified and applied',apply(args.source,Path(__file__).with_name('manifest.json'),args.existing_only),'frozen UX sources')
