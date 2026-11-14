"""Recover a complete current Pages snapshot from its certified file inventory.

Actions' short-lived Pages tar may have expired. Every public byte must match
its last successful publication proof before this replacement input is usable.
No partial tree or guessed listing is accepted; no publication occurs here.
"""
import argparse
import concurrent.futures
import hashlib
import json
import tarfile
import urllib.request
from pathlib import Path, PurePosixPath


def need(ok, message):
    if not ok:
        raise ValueError(message)


def digest(path):
    with Path(path).open('rb') as stream:
        return hashlib.file_digest(stream, 'sha256').hexdigest()


def safe_path(name):
    p = PurePosixPath(name)
    need(name and not p.is_absolute() and '..' not in p.parts
         and '\\' not in name and ':' not in name, 'Unsafe public inventory path')
    return p


def fetch(url, dest):
    request = urllib.request.Request(url, headers={'Cache-Control': 'no-cache'})
    with urllib.request.urlopen(request, timeout=90) as response, dest.open('xb') as output:
        while chunk := response.read(1024 * 1024):
            output.write(chunk)


def restore(a, downloader=fetch):
    proof, output, archive = Path(a.proof), Path(a.output), Path(a.archive)
    need(digest(proof) == a.proof_sha256, 'Publication inventory checksum differs')
    data = json.loads(proof.read_text(encoding='utf-8-sig'))
    need(data['source_sha'] == a.source and data['candidate_route'] == 'beta-integrated-vigilia',
         'Unexpected currently published source')
    need(not output.exists() and not archive.exists(), 'Preserve existing recovery files')
    rows = dict(data['preserved_fingerprints'])
    for name, row in data['candidate_fingerprints'].items():
        path = 'beta-integrated-vigilia/' + str(safe_path(name))
        need(path not in rows, 'Duplicate public inventory path')
        rows[path] = row
    proof_name = 'integrated-owner-stage-proof.json'
    need(proof_name not in rows, 'Duplicate stage proof')
    rows[proof_name] = {'bytes': proof.stat().st_size, 'sha256': a.proof_sha256}
    for name, row in rows.items():
        safe_path(name)
        need(isinstance(row['bytes'], int) and row['bytes'] >= 0
             and len(row['sha256']) == 64, 'Invalid public inventory entry')
    need(sum(row['bytes'] for row in rows.values()) == data['combined_bytes'],
         'Publication inventory is not the complete site')
    output.mkdir(parents=True)

    def one(name):
        row = rows[name]
        target = output / safe_path(name)
        target.parent.mkdir(parents=True, exist_ok=True)
        downloader(a.origin.rstrip('/') + '/' + name + '?p0_snapshot=' + row['sha256'], target)
        need(target.stat().st_size == row['bytes'] and digest(target) == row['sha256'],
             'Public file has changed: ' + name)

    # Detect a different live publication before transferring its large assets.
    one(proof_name)
    with concurrent.futures.ThreadPoolExecutor(max_workers=4) as pool:
        list(pool.map(one, (name for name in rows if name != proof_name)))
    actual = {p.relative_to(output).as_posix() for p in output.rglob('*') if p.is_file()}
    need(actual == set(rows), 'Incomplete recovered public site')
    need(not any(p.is_symlink() for p in output.rglob('*')), 'Symlink recovery refused')
    archive.parent.mkdir(parents=True, exist_ok=True)
    with tarfile.open(archive, 'w') as tar:
        for name in sorted(rows):
            info = tar.gettarinfo(str(output / name), arcname=name)
            info.uid = info.gid = info.mtime = 0
            info.uname = info.gname = ''
            with (output / name).open('rb') as stream:
                tar.addfile(info, stream)
    result = {'status': 'FULL_PUBLIC_SITE_RECOVERED_BYTE_IDENTICAL', 'source': a.source,
              'files': len(rows), 'bytes': data['combined_bytes'],
              'publication_inventory_sha256': a.proof_sha256,
              'archive_sha256': digest(archive), 'published': False}
    Path(str(archive) + '.recovery.json').write_text(json.dumps(result, indent=2) + '\n')
    print(json.dumps(result))
    return result


if __name__ == '__main__':
    parser = argparse.ArgumentParser()
    for arg in ('proof', 'proof-sha256', 'source', 'origin', 'output', 'archive'):
        parser.add_argument('--' + arg, required=True)
    restore(parser.parse_args())
