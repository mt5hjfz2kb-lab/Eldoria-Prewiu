"""Repackage a pinned, QA-approved archive without changing any playable bytes."""
import hashlib
import json
import pathlib
import shutil
import stat
import subprocess
import sys
import tempfile
import zipfile

ROOT = pathlib.Path(__file__).resolve().parents[2]


def prepare(archive, destination, config):
    expected = config['candidate']
    digest = hashlib.file_digest(open(archive, 'rb'), 'sha256').hexdigest()
    if digest != expected['artifact_sha256']:
        raise ValueError('Archive SHA-256 differs from QA-approved artifact')
    destination = pathlib.Path(destination)
    destination.mkdir(parents=True, exist_ok=False)
    with tempfile.TemporaryDirectory() as tmp:
        root = pathlib.Path(tmp)
        with zipfile.ZipFile(archive) as z:
            names = z.namelist()
            if len(names) != len(set(names)):
                raise ValueError('Duplicate archive paths')
            if sum(f.file_size for f in z.infolist()) > 500_000_000:
                raise ValueError('Package exceeds HTML5 size budget')
            for f in z.infolist():
                p = pathlib.PurePosixPath(f.filename)
                if p.is_absolute() or '..' in p.parts or '\\' in f.filename or stat.S_ISLNK(f.external_attr >> 16):
                    raise ValueError('Unsafe archive path')
                if not (f.filename.startswith('dist/') or f.filename == 'unity-output-verification.json'):
                    raise ValueError('Unexpected archive entry')
            z.extractall(root)
        # Reuse the existing hash/provenance/chunk gate, including Windows BOM handling.
        checker = "import {inspectPreparedSite} from './tools/closeout/closeout.mjs'; console.log(JSON.stringify(inspectPreparedSite({siteDirectory:process.argv[1],sourceSha:process.argv[2],validatedSourceSha:process.argv[2],nativeRun:Number(process.argv[3])})));"
        check = json.loads(subprocess.check_output(['node', '--input-type=module', '-e', checker, str(root), expected['source_sha'], str(expected['native_run'])], cwd=ROOT, text=True))
        dist = root / 'dist'
        manifest = json.loads((dist / 'build-manifest.json').read_text(encoding='utf-8-sig'))
        if manifest['revision'] != expected['revision']:
            raise ValueError('Unexpected playable revision')
        files = sorted(p for p in dist.rglob('*') if p.is_file())
        if not (dist / 'index.html').is_file() or len(files) > 1000 or any(p.stat().st_size > 200_000_000 for p in files):
            raise ValueError('itch.io HTML5 layout or file size limit violated')
        output = destination / ('Eldoria-' + expected['revision'] + '-itch.zip')
        identities = []
        with zipfile.ZipFile(output, 'w', zipfile.ZIP_STORED) as z:
            for p in files:
                rel = p.relative_to(dist).as_posix()
                if len(rel) > 240:
                    raise ValueError('itch.io path length exceeded')
                z.write(p, rel)
                identities.append({'file':rel, 'bytes':p.stat().st_size, 'sha256':hashlib.file_digest(open(p,'rb'),'sha256').hexdigest()})
        # Independently verify the repackaged bytes, including the loader and entry page.
        with zipfile.ZipFile(output) as z:
            for f in identities:
                data = z.read(f['file'])
                assert len(data) == f['bytes'] and hashlib.sha256(data).hexdigest() == f['sha256']
        report = {'status':'PACKAGE_READY_NOT_PUBLISHED', 'candidate':expected, 'original_unity_integrity':check, 'playable_files_unchanged':True, 'files':identities, 'zip_sha256':hashlib.file_digest(open(output,'rb'),'sha256').hexdigest(), 'hosted_verified':False}
        (destination/'identity.json').write_text(json.dumps(report,indent=2)+'\n')
        shutil.copytree(dist, destination/'dist')
        print(json.dumps({'status':report['status'], 'files':len(files), 'zip_sha256':report['zip_sha256']}))


if __name__ == '__main__':
    prepare(sys.argv[1], sys.argv[2], json.loads((ROOT/'pipeline/playtest-delivery.json').read_text()))
