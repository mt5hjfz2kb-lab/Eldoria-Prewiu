"""Read existing Unity metadata without opening or changing a Unity project."""
import argparse
import hashlib
import json
import os
from pathlib import Path

SKIP = {'.git', 'Library', 'Temp', 'Logs', 'Build', 'Builds', 'node_modules',
        'obj', 'Obj', 'MemoryCaptures', 'PackageCache'}


def project_roots(roots, max_depth=5, max_directories=2500):
    seen, found, warnings = set(), [], []
    queue = [(Path(p), 0) for p in roots]
    while queue:
        p, depth = queue.pop(0)
        if p.is_symlink() or (hasattr(p, 'is_junction') and p.is_junction()):
            continue
        key = os.path.normcase(str(p.resolve()))
        if key in seen:
            continue
        seen.add(key)
        if len(seen) > max_directories:
            warnings.append('Directory search budget reached')
            break
        if (p / 'Assets').is_dir() and (p / 'ProjectSettings/ProjectVersion.txt').is_file():
            found.append(p)
            continue
        if depth >= max_depth:
            continue
        try:
            queue.extend((x, depth + 1) for x in sorted(p.iterdir())
                         if x.name not in SKIP and x.is_dir())
        except OSError as e:
            warnings.append(f'{p}: {type(e).__name__}')
    return found, warnings


def signature(path):
    s = path.stat()
    return s.st_size, s.st_mtime_ns


def git_blob(path):
    size = path.stat().st_size
    h = hashlib.sha1(f'blob {size}\0'.encode('ascii'))
    with path.open('rb') as f:
        while chunk := f.read(1024 * 1024):
            h.update(chunk)
    return h.hexdigest()


def snapshot(request, roots, output, byte_budget=2 * 1024**3):
    output = Path(output)
    # A new destination prevents accidentally overwriting an earlier recovery.
    output.mkdir(parents=True, exist_ok=False)
    projects, warnings = project_roots(roots)
    report = {'schema': 1, 'source_sha': request['source_sha'],
              'run_id': os.environ.get('GITHUB_RUN_ID'),
              'read_only_projects': True, 'unity_launched': False,
              'projects': [str(p) for p in projects], 'warnings': warnings,
              'candidates': [], 'observations': []}
    read_bytes = 0
    for index, project in enumerate(projects):
        for target in request['targets']:
            relative = target['path']
            parts = Path(relative).parts
            if not relative.startswith('Unity/Assets/') or '..' in parts or '\\' in relative:
                raise ValueError('Unsafe request path: ' + relative)
            asset = project.joinpath(*parts[1:])
            meta = Path(str(asset) + '.meta')
            if not asset.is_file() or not meta.is_file():
                continue
            observation = {'project': index, 'path': relative}
            try:
                if meta.stat().st_size > 2 * 1024**2:
                    raise ValueError('Metadata exceeds size budget')
                # Reject links also at the requested path and its parents.
                for path in (asset, meta, *asset.parents):
                    if path == project.parent:
                        break
                    if path.is_symlink() or (hasattr(path, 'is_junction') and path.is_junction()):
                        raise ValueError('Linked source path')
                before = signature(asset), signature(meta)
                if read_bytes + before[0][0] > byte_budget:
                    raise ValueError('Asset hashing budget reached')
                first = meta.read_bytes()
                actual = git_blob(asset)
                read_bytes += before[0][0]
                if before != (signature(asset), signature(meta)) or first != meta.read_bytes():
                    raise ValueError('Source changed during snapshot')
                if actual != target['blob_sha']:
                    observation.update(status='ASSET_DIFFERS', actual_blob_sha=actual)
                else:
                    destination = output / f'project-{index:03d}' / (relative + '.meta')
                    destination.parent.mkdir(parents=True, exist_ok=True)
                    destination.write_bytes(first)
                    report['candidates'].append({
                        'project': index, 'path': relative + '.meta',
                        'asset_blob_sha': actual,
                        'meta_sha256': hashlib.sha256(first).hexdigest(),
                        'snapshot_path': destination.relative_to(output).as_posix()})
                    observation['status'] = 'MATCHED_STABLE_SOURCE'
            except (OSError, ValueError) as e:
                observation.update(status='SKIPPED', reason=str(e))
            report['observations'].append(observation)
    report['asset_bytes_hashed'] = read_bytes
    (output / 'snapshot.json').write_text(json.dumps(report, indent=2), encoding='utf-8')
    return report


if __name__ == '__main__':
    ap = argparse.ArgumentParser(description=__doc__)
    ap.add_argument('--request', required=True)
    ap.add_argument('--search-root', action='append', required=True)
    ap.add_argument('--output', required=True)
    args = ap.parse_args()
    result = snapshot(json.loads(Path(args.request).read_text(encoding='utf-8-sig')),
                      args.search_root, args.output)
    print(json.dumps({'projects': len(result['projects']),
                      'matched_metadata': len(result['candidates']),
                      'warnings': result['warnings']}))
