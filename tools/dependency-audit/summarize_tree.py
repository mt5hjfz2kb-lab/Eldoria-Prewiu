#!/usr/bin/env python3
"""Summarize a saved git ls-tree -r -l -z inventory; never fetch or change Git."""
import argparse
import collections
import json
from pathlib import Path


def summarize(raw, commit):
    files = []
    for entry in raw.split(b'\0'):
        if not entry:
            continue
        header, name = entry.split(b'\t', 1)
        mode, kind, sha, size = header.decode('ascii').split()
        if kind != 'blob':
            continue
        if size == '-':
            raise ValueError('Missing blob size; inventory is incomplete')
        files.append({'path': name.decode('utf-8'), 'sha': sha, 'bytes': int(size)})
    groups = collections.defaultdict(lambda: {'files': 0, 'bytes': 0})
    objects = collections.defaultdict(list)
    for file in files:
        group = groups[file['path'].split('/')[0]]
        group['files'] += 1
        group['bytes'] += file['bytes']
        objects[file['sha']].append(file)
    total = sum(f['bytes'] for f in files)
    unique = sum(group[0]['bytes'] for group in objects.values())
    duplicates = [group for group in objects.values() if len(group) > 1]
    duplicates.sort(key=lambda group: group[0]['bytes'] * (len(group)-1), reverse=True)
    return {
        'snapshot_commit': commit, 'files': len(files),
        'tracked_file_bytes': total, 'unique_blob_bytes': unique,
        'repeated_path_bytes': total-unique,
        'groups': [{'path': key, **value} for key, value in
                   sorted(groups.items(), key=lambda pair: pair[1]['bytes'], reverse=True)],
        'largest_files': sorted(files, key=lambda f: f['bytes'], reverse=True)[:15],
        'big_duplicates': [
            {'blob': group[0]['sha'], 'bytes': group[0]['bytes'], 'count': len(group),
             'repeated_path_bytes': group[0]['bytes']*(len(group)-1),
             'paths': [f['path'] for f in group]}
            for group in duplicates[:15]],
        'warning': 'Logical uncompressed tree sizes, not packed Git history or APK. '
                   'Duplicate bytes do not establish interchangeable Unity GUIDs or authorize deletion.'}


if __name__ == '__main__':
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--inventory', type=Path, required=True)
    parser.add_argument('--commit', required=True)
    parser.add_argument('--output', type=Path, required=True)
    args = parser.parse_args()
    if len(args.commit) != 40 or any(c not in '0123456789abcdef' for c in args.commit):
        parser.error('--commit must be a full lowercase commit SHA')
    result = summarize(args.inventory.read_bytes(), args.commit)
    args.output.write_text(json.dumps(result, ensure_ascii=False, indent=2)+'\n')
    print(result['files'], 'files;', result['tracked_file_bytes'], 'logical bytes')
