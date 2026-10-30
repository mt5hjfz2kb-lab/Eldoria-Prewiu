"""Adapt verified Unity transport to itch CDN without changing engine/game bytes."""
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

        # Sites splits a gzip stream; itch sniffs each file and sends Content-Encoding.
        # Restore the EXACT original compressed Unity bytes before CDN delivery.
        transport = []
        for name, entry in manifest.get('chunks', {}).items():
            rel = name.split('?', 1)[0]
            target = dist / rel
            if pathlib.PurePosixPath(rel).is_absolute() or '..' in pathlib.PurePosixPath(rel).parts:
                raise ValueError('Unsafe reconstructed path')
            target.parent.mkdir(parents=True, exist_ok=True)
            digest = hashlib.sha256()
            with target.open('wb') as out:
                for part in entry['parts']:
                    path = dist / part
                    if not path.resolve().is_relative_to(dist.resolve()):
                        raise ValueError('Unsafe chunk path')
                    data = path.read_bytes()
                    digest.update(data)
                    out.write(data)
            if target.stat().st_size != entry['bytes'] or digest.hexdigest() != entry['sha256']:
                raise ValueError('Reconstructed Unity bytes differ from original')
            # Fully validate gzip CRC and trailer, not just its header.
            import gzip
            with gzip.open(target, 'rb') as stream:
                while stream.read(1024 * 1024):
                    pass
            transport.append({'file':rel, 'sha256':digest.hexdigest(), 'bytes':target.stat().st_size})
        if transport:
            for entry in manifest['chunks'].values():
                for part in entry['parts']:
                    (dist / part).unlink(missing_ok=True)
            manifest['chunks'] = {}
            (dist / 'build-manifest.json').write_text(json.dumps(manifest, indent=2)+'\n')
        # Record exact decoded sizes; no buffered decompression in the browser adapter.
        import gzip
        manifest['decoded_sizes'] = {}
        manifest['decoded_parts'] = {}
        reencoded = []
        for key in ('data', 'framework', 'code'):
            name = manifest[key]
            asset = dist / name.split('?', 1)[0]
            with asset.open('rb') as raw:
                compressed = raw.read(2) == b'\x1f\x8b'
            if compressed:
                total = 0
                with gzip.open(asset, 'rb') as stream:
                    while block := stream.read(1024 * 1024):
                        total += len(block)
                manifest['decoded_sizes'][name] = total
                if asset.stat().st_size > 200_000_000:
                    # Separate COMPLETE gzip streams; never split a gzip header/body.
                    # The browser concatenates decoded streams, preserving Unity's
                    # exact input bytes with one final allocation in its own loader.
                    parts, source_hash, decoded_bytes = [], hashlib.sha256(), 0
                    with gzip.open(asset, 'rb') as stream:
                        while block := stream.read(64 * 1024 * 1024):
                            source_hash.update(block)
                            decoded_bytes += len(block)
                            rel = 'payload-decoded/' + key + '-' + str(len(parts)) + '.gz'
                            part = dist / rel
                            part.parent.mkdir(parents=True, exist_ok=True)
                            part.write_bytes(gzip.compress(block, compresslevel=6, mtime=0))
                            assert gzip.decompress(part.read_bytes()) == block
                            parts.append({'url':rel, 'decoded_bytes':len(block), 'decoded_sha256':hashlib.sha256(block).hexdigest()})
                    assert decoded_bytes == total
                    manifest['decoded_parts'][name] = parts
                    reencoded.append({'file':name, 'decoded_bytes':total, 'decoded_sha256':source_hash.hexdigest(), 'parts':parts})
                    asset.unlink()
        manifest['delivery_adapter'] = 'itch-decoded-stream-v3' if reencoded else 'itch-low-memory-v2'
        (dist / 'build-manifest.json').write_text(json.dumps(manifest, indent=2)+'\n')
        # Show the actual startup error to make physical-device failures diagnosable.
        play = dist / 'play.js'
        original = play.read_text()
        old = 'errorText.textContent="No se ha podido abrir Valoria. Vuelve a intentarlo con una conexión estable. Tu progreso guardado no se borra.";'
        new = 'errorText.textContent="No se ha podido abrir Valoria. Detalle: "+String(error?.message||error).replace(/<[^>]*>/g,"").slice(0,1200);'
        if original.count(old) != 1:
            raise ValueError('Unexpected welcome wrapper; diagnostic patch needs review')
        adapted = original.replace(old,new)
        needle = 'installChunkTransport(build.chunks);'
        if adapted.count(needle) != 1:
            raise ValueError('Unexpected transport initialization')
        adapted = adapted.replace(needle, 'installExactDecodedTransport(build.decoded_sizes,build.decoded_parts);')
        needle = 'streamingAssetsUrl:"StreamingAssets",'
        if adapted.count(needle) != 1:
            raise ValueError('Unexpected Unity configuration')
        # Avoid Response.clone() buffering the entire data file for UnityCache.
        # Normal browser HTTP caching and game saves remain independent.
        adapted = adapted.replace(needle, 'cacheControl:()=>"no-store",'+needle)
        helper = (ROOT/'tools/delivery/exact-decoded-transport.js').read_text()
        # Dormant unless ?eldoria_diag=1; no remote logging and no save reads.
        # This wraps only the HTML5 bootstrap, never the Unity engine bytes.
        diagnostic = (ROOT/'tools/delivery/optional-iphone-lifecycle-diagnostics.js').read_text()
        revision = 'window.__eldoriaDiagnosticRevision=' + json.dumps(expected['revision']) + ';'
        play.write_text(revision+'\n'+diagnostic+'\n'+helper+'\n'+adapted)

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
        report = {'status':'PACKAGE_READY_NOT_PUBLISHED', 'candidate':expected, 'original_unity_integrity':check, 'playable_files_unchanged':False, 'unity_bytes_unchanged':not reencoded, 'unity_decoded_bytes_unchanged':True, 'reencoded_transport':reencoded, 'transport_adapter':manifest['delivery_adapter'], 'reconstructed_files':transport, 'wrapper_changes':['build-manifest.json','play.js'], 'files':identities, 'zip_sha256':hashlib.file_digest(open(output,'rb'),'sha256').hexdigest(), 'hosted_verified':False}
        (destination/'identity.json').write_text(json.dumps(report,indent=2)+'\n')
        shutil.copytree(dist, destination/'dist')
        print(json.dumps({'status':report['status'], 'files':len(files), 'zip_sha256':report['zip_sha256']}))


if __name__ == '__main__':
    prepare(sys.argv[1], sys.argv[2], json.loads((ROOT/'pipeline/playtest-delivery.json').read_text()))

