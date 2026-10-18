"""Read-only byte-level verification of the actual Valoria delivery archives."""
import argparse
import hashlib
import json
import pathlib
import struct
import tarfile
import zipfile

ROOT = 'Assets/Eldoria/ArtTests/ValoriaReferenceClean/'
PREFABS = ('Bastion_I', 'FortifiedGate', 'MedievalBridge', 'Paths',
           'BuildablePlots', 'Ruins', 'RiverSurface', 'ValoriaArchitecture')

def sha256(path):
    digest = hashlib.sha256()
    with open(path, 'rb') as stream:
        for chunk in iter(lambda: stream.read(1024 * 1024), b''):
            digest.update(chunk)
    return digest.hexdigest()

def verify_apk(path):
    with zipfile.ZipFile(path) as archive:
        bad = archive.testzip()
        if bad:
            raise ValueError('ZIP CRC failure: ' + bad)
        names = set(archive.namelist())
        assert 'AndroidManifest.xml' in names, 'Missing Android manifest'
        libraries = sorted(n for n in names if n.startswith('lib/') and n.endswith('.so'))
        abis = sorted({n.split('/')[1] for n in libraries})
        assert abis == ['arm64-v8a'], ('Unexpected ABIs', abis)
        for name in ('libunity.so', 'libil2cpp.so'):
            assert 'lib/arm64-v8a/' + name in names, 'Missing ' + name
        for name in libraries:
            with archive.open(name) as stream:
                header = stream.read(64)
            assert header[:4] == b'\x7fELF' and header[4:6] == b'\x02\x01', name
            assert struct.unpack_from('<H', header, 18)[0] == 183, name
        assert any(n.startswith('assets/bin/Data/') for n in names), 'Missing Unity data'
        return dict(status='APK_ARCHIVE_AND_ARM64_VERIFIED', abis=abis,
                    native_libraries=libraries, zip_entries=len(names),
                    physical_device_tested=False)

def verify_package(path, scene_sha=None):
    with tarfile.open(path, 'r:gz') as archive:
        members = archive.getmembers()
        by_name = {m.name: m for m in members}
        paths = {}
        for member in members:
            p = pathlib.PurePosixPath(member.name)
            assert not p.is_absolute() and '..' not in p.parts, member.name
            assert not member.issym() and not member.islnk(), member.name
            if member.name.endswith('/pathname'):
                asset_path = archive.extractfile(member).read().decode('utf-8-sig').strip('\x00\r\n')
                assert asset_path not in paths, ('Duplicate asset path', asset_path)
                paths[asset_path] = member.name.rsplit('/', 1)[0]
        required = [ROOT + 'Valoria_Reference_Clean.unity', ROOT + 'Sources/Valoria_Reference_Clean.glb']
        required += [ROOT + 'Prefabs/' + name + '.prefab' for name in PREFABS]
        for asset_path in required:
            assert asset_path in paths, ('Missing asset', asset_path)
            guid = paths[asset_path]
            assert guid + '/asset' in by_name and guid + '/asset.meta' in by_name, asset_path
        scene = archive.extractfile(paths[required[0]] + '/asset').read()
        digest = hashlib.sha256(scene).hexdigest()
        if scene_sha:
            assert digest == scene_sha, 'Package scene differs from validated source'
        lighting = sorted(p for p in paths if 'GeneratedLighting/' in p)
        assert lighting, 'No baked-lighting assets in package'
        # Read every regular file to detect truncated tar data without extracting anything.
        for member in members:
            if member.isfile():
                with archive.extractfile(member) as stream:
                    while stream.read(1024 * 1024):
                        pass
        return dict(status='PACKAGE_CONTENT_VERIFIED', assets=len(paths),
                    scene_sha256=digest, prefabs=list(PREFABS),
                    lighting_assets=lighting, native_import_verified=False)

if __name__ == '__main__':
    parser = argparse.ArgumentParser()
    parser.add_argument('kind', choices=['apk', 'unitypackage'])
    parser.add_argument('path', type=pathlib.Path)
    parser.add_argument('--scene-sha256')
    parser.add_argument('--output', type=pathlib.Path)
    args = parser.parse_args()
    result = verify_apk(args.path) if args.kind == 'apk' else verify_package(args.path, args.scene_sha256)
    result.update(file=args.path.name, bytes=args.path.stat().st_size, sha256=sha256(args.path))
    payload = json.dumps(result, indent=2)
    if args.output:
        args.output.write_text(payload + '\n', encoding='utf-8')
    print(payload)
