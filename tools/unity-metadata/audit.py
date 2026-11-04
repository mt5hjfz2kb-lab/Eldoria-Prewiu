"""Audit versioned Unity file metadata; never generates or edits GUIDs."""
import argparse
import hashlib
import json
from pathlib import Path, PurePosixPath
import re
import subprocess

PREFIX = 'Unity/Assets/'
BUNDLES = {'.androidlib', '.bundle', '.framework', '.plugin'}
# Binding owner veto, 25 October 2026 (AGENTS.md). Preserve the source outside
# Assets; Resources content would otherwise be included even without references.
REJECTED_RESOURCE = (PREFIX + 'Eldoria/Resources/Valoria/HeroBastionGenerated/'
                     'Valoria_HeroBastion_v1.glb')


def bundled_internal(path):
    return any(PurePosixPath(p).suffix in BUNDLES for p in PurePosixPath(path).parts[:-1])


def audit(entries, read_bytes, baseline):
    paths = set(entries)
    failures, missing, known, orphans, empty_folders = [], [], [], [], []
    if REJECTED_RESOURCE in paths:
        failures.append('Owner-rejected legacy model must remain archived outside Unity: ' + REJECTED_RESOURCE)
    guids = {}
    directories = set()
    for p in paths:
        directories.update(x.as_posix() for x in PurePosixPath(p).parents)
    for p in sorted(paths):
        if not p.startswith(PREFIX) or bundled_internal(p):
            continue
        if any(x.startswith('.') for x in PurePosixPath(p).parts):
            continue
        if not p.endswith('.meta'):
            if p + '.meta' not in paths:
                row = {'path': p, 'blob_sha': entries[p]}
                missing.append(row)
                if baseline.get('allowed_missing', {}).get(p) == entries[p]:
                    known.append(row)
                else:
                    failures.append('Unbaselined missing metadata: ' + p)
            continue
        try:
            data = read_bytes(p)
        except OSError:
            failures.append('Metadata not readable: ' + p)
            continue
        text = data.decode('utf-8-sig')
        match = re.search(r'^guid: ([0-9a-fA-F]{32})\s*$', text, re.M)
        if not match:
            failures.append('Invalid or absent metadata GUID: ' + p)
        else:
            guid = match[1].lower()
            guids.setdefault(guid, []).append(p)
        asset = p[:-5]
        if asset not in paths and asset not in directories:
            if re.search(r'^folderAsset: yes\s*$', text, re.M):
                # Git cannot represent an empty directory. Preserve its identity.
                empty_folders.append(p)
            else:
                row = {'path': p, 'blob_sha': entries[p]}
                orphans.append(row)
                if baseline.get('allowed_orphans', {}).get(p) != entries[p]:
                    failures.append('Unbaselined orphan metadata: ' + p)
    duplicates = {g: ps for g, ps in guids.items() if len(ps) > 1}
    for g, ps in duplicates.items():
        failures.append('Duplicate GUID ' + g + ': ' + ', '.join(ps))
    folder_debt = sorted(d for d in directories if d.startswith(PREFIX)
                         and not bundled_internal(d) and d + '.meta' not in paths)
    for folder in folder_debt:
        if folder not in baseline.get('allowed_missing_folders', []):
            failures.append('Unbaselined missing folder metadata: ' + folder)
    return {'missing': missing, 'known_missing': known, 'orphan_files': orphans,
            'preserved_empty_folder_metadata': empty_folders,
            'missing_folder_metadata': folder_debt, 'duplicate_guids': duplicates,
            'failures': failures}


def git_entries(root, ref):
    raw = subprocess.check_output(['git', 'ls-tree', '-r', '-z', ref], cwd=root)
    return {p.decode('utf-8'): head.decode().split()[2]
            for item in raw.split(b'\0') if item for head, p in [item.split(b'\t', 1)]}


def verify_recovery(root, manifest):
    failures = []
    for item in manifest['entries'] + manifest.get('archived_orphans', []):
        p = root / item['stored_path']
        if not p.is_file() or hashlib.sha256(p.read_bytes()).hexdigest() != item['meta_sha256']:
            failures.append('Recovery bytes differ: ' + item['stored_path'])
    return failures


def verify_archives(entries, manifest):
    """Check immutable archived blobs using Git, without loading large models."""
    failures = []
    for item in manifest.get('archived_rejected_assets', []):
        for path, expected in [(item['destination'], item['asset_blob_sha']),
                               (item['destination'] + '.meta', item['metadata_blob_sha'])]:
            if entries.get(path) != expected:
                failures.append('Archived original Git blob differs or is missing: ' + path)
        for path in [item['source'], item['source'] + '.meta']:
            if path in entries:
                failures.append('Rejected source remains inside Unity: ' + path)
    return failures


def ignored_metadata(root, paths):
    probe = subprocess.run(['git', 'check-ignore', '--no-index', '-z', '--stdin'],
                           cwd=root, input=b'\0'.join(p.encode() for p in paths) + b'\0',
                           stdout=subprocess.PIPE, stderr=subprocess.PIPE)
    if probe.returncode not in (0, 1):
        raise RuntimeError(probe.stderr.decode())
    return [p.decode() for p in probe.stdout.split(b'\0') if p]


if __name__ == '__main__':
    ap = argparse.ArgumentParser(description=__doc__)
    ap.add_argument('--ref', default='HEAD')
    ap.add_argument('--baseline', default='tools/unity-metadata/baseline.json')
    ap.add_argument('--verify-recovery')
    ap.add_argument('--archive-manifest')
    ap.add_argument('--output')
    args = ap.parse_args()
    root = Path(subprocess.check_output(['git', 'rev-parse', '--show-toplevel']).decode().strip())
    entries = git_entries(root, args.ref)
    result = audit(entries, lambda p: (root / p).read_bytes(),
                   json.loads((root / args.baseline).read_text()))
    expected_meta = {p for p in entries if p.startswith(PREFIX) and p.endswith('.meta')}
    expected_meta.update(x['path'] + '.meta' for x in result['missing'])
    expected_meta.update(p + '.meta' for p in result['missing_folder_metadata'])
    result['ignored_metadata'] = ignored_metadata(root, sorted(expected_meta))
    result['failures'] += ['Metadata excluded by ignore rules: ' + p for p in result['ignored_metadata']]
    if args.verify_recovery:
        result['failures'] += verify_recovery(root, json.loads((root / args.verify_recovery).read_text()))
    if args.archive_manifest:
        result['failures'] += verify_archives(entries, json.loads((root / args.archive_manifest).read_text()))
    if args.output:
        Path(args.output).write_text(json.dumps(result, indent=2) + '\n')
    print(json.dumps({k: len(result[k]) for k in result}))
    for failure in result['failures']:
        print('FAIL: ' + failure)
    raise SystemExit(bool(result['failures']))
