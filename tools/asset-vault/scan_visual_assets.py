#!/usr/bin/env python3
"""Read-only tracked source inventory; omissions are explicit, never approvals."""
import argparse
import hashlib
import json
import pathlib
import re
import subprocess

EXT = {'.glb':'model3d', '.gltf':'model3d', '.fbx':'model3d', '.blend':'editable3d',
       '.obj':'model3d', '.png':'image2d', '.jpg':'image2d', '.jpeg':'image2d',
       '.webp':'image2d', '.tga':'image2d', '.psd':'editable2d', '.svg':'vector2d',
       '.aseprite':'editable2d', '.tif':'image2d', '.tiff':'image2d', '.exr':'image2d'}
GUID = re.compile(r'\bguid:\s*([a-fA-F0-9]{32})\b')
VETO = 'Unity/Assets/Eldoria/Resources/Valoria/HeroBastionGenerated/Valoria_HeroBastion_v1.glb'

def scan(root):
    root = pathlib.Path(root).resolve()
    tracked = subprocess.check_output(['git','-C',str(root),'ls-files','-z']).decode('utf-8').split('\0')
    paths = sorted(filter(None, tracked))
    references = {}
    # Unity references identify usage, not artistic acceptance or runtime visibility.
    for rel in paths:
        file = root / rel
        if file.suffix.lower() not in {'.unity','.prefab','.mat','.asset'}:
            continue
        if file.is_symlink() or not file.is_file() or not file.resolve().is_relative_to(root):
            continue
        if file.stat().st_size > 16 * 1024 * 1024:
            continue
        for guid in set(GUID.findall(file.read_text(encoding='utf-8', errors='replace'))):
            references.setdefault(guid.lower(), []).append(rel)
    for rel in paths:
        p = pathlib.PurePosixPath(rel)
        if p.suffix.lower() not in EXT:
            continue
        file = root / rel
        row = {'path':rel, 'kind':EXT[p.suffix.lower()], 'sha256':None, 'bytes':None,
               'approval':'OWNER_REJECTED' if rel == VETO else 'UNREVIEWED',
               'source':'tracked_git', 'availability':'MISSING_WORKTREE',
               'unity_guid':None, 'unity_references':[]}
        if file.is_symlink() or not file.resolve().is_relative_to(root):
            row['availability'] = 'SYMLINK_NOT_FOLLOWED'
        elif file.is_file():
            h = hashlib.sha256()
            with file.open('rb') as stream:
                prefix = stream.read(1024)
                h.update(prefix)
                for block in iter(lambda:stream.read(1048576), b''):
                    h.update(block)
            row.update(sha256=h.hexdigest(), bytes=file.stat().st_size, availability='PRESENT')
            if prefix.startswith(b'version https://git-lfs.github.com/spec/v1\n'):
                row['availability'] = 'LFS_POINTER_ONLY'
                match = re.search(rb'oid sha256:([a-f0-9]{64})',prefix)
                row['lfs_object_sha256'] = match.group(1).decode() if match else None
        meta = root / (rel + '.meta')
        if not meta.is_symlink() and meta.is_file() and meta.resolve().is_relative_to(root):
            match = GUID.search(meta.read_text(encoding='utf-8',errors='replace'))
            if match:
                row['unity_guid'] = match.group(1).lower()
                row['unity_references'] = references.get(row['unity_guid'],[])
        yield row

def main():
    parser = argparse.ArgumentParser()
    parser.add_argument('--root',default='.')
    parser.add_argument('--output',default='-')
    args = parser.parse_args()
    root = pathlib.Path(args.root).resolve()
    entries = list(scan(root))
    commit = subprocess.check_output(['git','-C',str(root),'rev-parse','HEAD'],text=True).strip()
    gaps = [r['path'] for r in entries if r['availability'] != 'PRESENT']
    doc = {'schema_version':2, 'read_only':True, 'source_commit':commit, 'count':len(entries),
           'all_tracked_bytes_present':not gaps, 'unavailable_paths':gaps,
           'reference_scope':'Serialized GUID references in present tracked Unity files up to 16 MiB; dynamic Resources/GLB loads need host/source review.',
           'backup_certified':False, 'entries':entries}
    payload = json.dumps(doc,ensure_ascii=False,indent=2)+'\n'
    if args.output == '-':
        print(payload,end='')
    else:
        pathlib.Path(args.output).write_text(payload,encoding='utf-8')
if __name__ == '__main__':
    main()
