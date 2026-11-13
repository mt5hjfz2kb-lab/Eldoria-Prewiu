"""Stage exact Vigilia beside unchanged beta18, with reversible legacy archives.

This does not publish. The caller must durably retain the original Pages tar
before deployment. Only beta10 and its old clock diagnostic leave active hosting.
Their entry URLs become explicit archive notices; beta14 and beta18 are unchanged.
"""
import argparse
import gzip
import hashlib
import json
import shutil
import sys
from pathlib import Path

sys.path.insert(0, str(Path(__file__).with_name('iphone_ab')))
from stage_compact_delta_ab import restore_tar
from stage_copyadd_owner_ab import safe_path, verified_stream

SOURCE = 'fb8727d3ac45e31e61ab8ba244fa395477eff16e'
BUNDLE = 'eb9192488a6cc34dd38309f48cdd7fdb3dc3ea5a2d6fb7dc23a023d8e23b0048'
DATA = '38d389c5f2895988a7ee6dac2dda1bdeda57c7b6dd0cd1a46d6b751b84fa844f'
ROUTE = 'beta-integrated-vigilia'
RETIRED = ('beta-native325', 'beta-iphone-clock-ab')
STABLE = {
    'beta-iphone-p0-city': '63fb2095412a3066e18f3a8a31c2a03f40ef4944',
    'beta-iphone-p0-lazy-world': 'de63660d25bee9a32d7b804d6245744ed1f0dbca',
}
LIMIT = 980_000_000


def need(ok, message):
    if not ok:
        raise ValueError(message)


def read(path):
    return json.loads(Path(path).read_text(encoding='utf-8-sig'))


def sha(path):
    with Path(path).open('rb') as stream:
        return hashlib.file_digest(stream, 'sha256').hexdigest()


def tree(root):
    result = {}
    for p in Path(root).rglob('*'):
        need(not p.is_symlink(), 'Symlink input refused')
        if p.is_file():
            result[p.relative_to(root).as_posix()] = dict(bytes=p.stat().st_size, sha256=sha(p))
    return result


def browser_gate(root):
    reports = list(Path(root).rglob('candidate/report.json'))
    need(len(reports) == 1, 'Exact candidate browser report missing')
    report = read(reports[0])
    need(report.get('passed') is True and not report.get('errors'), 'Candidate WebKit failed')
    need(report.get('entry', {}).get('reloaded') is True, 'Actual reload missing')
    need(report.get('vigiliaProductionInstallations', 0) >= 2, 'Vigilia missing after reload')
    identity = report.get('identity', {})
    need(identity.get('source_sha') == SOURCE and identity.get('candidate_bundle_sha256') == BUNDLE,
         'Browser proof identifies a different build')


def stage(a):
    root, candidate, work = Path(a.output), Path(a.candidate), Path(a.work)
    need(not root.exists() and not work.exists(), 'Refuse replacing a local stage')
    browser_gate(a.browser)
    identity = read(candidate / 'release-identity.json')
    need(identity.get('source_sha') == SOURCE and identity.get('candidate_bundle_sha256') == BUNDLE
         and identity.get('delivered_data_sha256') == DATA, 'Wrong native candidate')
    need(identity.get('native_journey_verified') is True
         and identity.get('native_destination_captures_verified') is True
         and identity.get('release_for_external_testers') is False
         and identity.get('physical_mobile_tested') is False, 'Candidate certification boundary changed')
    work.mkdir(parents=True)
    manifest = read(candidate / 'build-manifest.json')
    data = work / 'verified-candidate.bin'
    verified_stream(candidate, manifest['decoded_parts'][manifest['data']], data)
    need(sha(data) == DATA, 'Delivered native data changed')
    need(data.stat().st_size == manifest['decoded_sizes'][manifest['data']], 'Native data extent changed')
    data.unlink()
    for key in ('framework', 'code'):
        url = manifest[key]
        path = candidate / safe_path(url)
        need(url.endswith('?v=' + sha(path)[:16]), 'Native binary version differs from encoded bytes')
        with gzip.open(path, 'rb') as source:
            size = sum(len(block) for block in iter(lambda: source.read(1048576), b''))
        need(size == manifest['decoded_sizes'][url], 'Native binary decoded size changed')
    restore_tar(a.pages, root)
    before = tree(root)
    for route, source in STABLE.items():
        need(read(root / route / 'release-identity.json')['source_sha'] == source, 'Stable public route changed')
    replacement_source = getattr(a, 'replace_source', None)
    replaced = {}
    if replacement_source:
        need(read(root / ROUTE / 'release-identity.json')['source_sha'] == replacement_source,
             'Unexpected previously published candidate')
        replaced = {k: v for k, v in before.items()
                    if k.startswith(ROUTE + '/') or k == 'integrated-owner-stage-proof.json'}
        protected = {k: v for k, v in before.items() if k not in replaced}
        retired = {}
        need(len(protected) > 60 and len(replaced) > 10, 'Incomplete replacement snapshot')
        shutil.rmtree(root / ROUTE)
    else:
        need(read(root / 'beta-native325/release-identity.json')['source_sha'] ==
             'be49d62e3daf490f41dba0bf8123a8e2eee64480', 'Unknown historical beta10')
        need(not (root / ROUTE).exists(), 'New route already exists')
        retired = {k: v for k, v in before.items() if k.split('/')[0] in RETIRED}
        protected = {k: v for k, v in before.items() if k not in retired}
        need(len(retired) > 60 and len(protected) > 60, 'Incomplete original site')
        for route in RETIRED:
            need((root / route).is_dir(), 'Historical route missing')
            shutil.rmtree(root / route)
            (root / route).mkdir()
            (root / route / 'index.html').write_text('''<!doctype html><html lang="es"><meta charset="utf-8">
    <meta name="viewport" content="width=device-width,initial-scale=1"><title>Eldoria · versión archivada</title>
    <body style="background:#14110e;color:#ead6ad;font:20px system-ui;max-width:38rem;margin:12vh auto;padding:24px">
    <h1>Versión de diagnóstico archivada</h1><p>La copia original se conserva para recuperación.</p>
    <p><a style="color:#f4ce79" href="../beta-iphone-p0-lazy-world/?eldoria_p0_lazy_world=1">Abrir beta18 estable</a></p>
    <p><a style="color:#f4ce79" href="../beta-integrated-vigilia/?eldoria_p0_lazy_world=1">Probar la interfaz integrada</a></p>
    </body></html>''')
    shutil.copytree(candidate, root / ROUTE)
    after = tree(root)
    need(all(after.get(k) == v for k, v in protected.items()), 'A retained public file changed')
    proof = dict(status='STAGED_NOT_PUBLISHED_NOT_PHYSICAL_IPHONE', source_sha=SOURCE,
        bundle_sha256=BUNDLE, candidate_route=ROUTE, original_pages_tar_sha256=sha(a.pages),
        original_pages_tar_bytes=Path(a.pages).stat().st_size, required_durable_archive=True,
        native_run=43329543960, full_certificate_run=43329553450,
        stable_routes_preserved=list(STABLE), preserved_fingerprints=protected,
        archived_fingerprints=retired, archived_routes=list(RETIRED) if not replacement_source else [],
        replaced_source_sha=replacement_source, replaced_fingerprints=replaced,
        candidate_fingerprints=tree(root / ROUTE), physical_iphone_verified=False,
        art_approved=False, external_testers_enabled=False, published=False)
    target = root / 'integrated-owner-stage-proof.json'
    for _ in range(4):
        target.write_text(json.dumps(proof, indent=2) + '\n')
        proof['combined_bytes'] = sum(p.stat().st_size for p in root.rglob('*') if p.is_file())
    target.write_text(json.dumps(proof, indent=2) + '\n')
    total = sum(p.stat().st_size for p in root.rglob('*') if p.is_file())
    need(total == proof['combined_bytes'] and total < LIMIT, 'Original Pages size budget exceeded')
    print(json.dumps({k: v for k, v in proof.items() if not k.endswith('fingerprints')}, indent=2))


if __name__ == '__main__':
    parser = argparse.ArgumentParser(description=__doc__)
    for name in ('pages', 'candidate', 'browser', 'output', 'work'):
        parser.add_argument('--' + name, required=True)
    parser.add_argument('--replace-source', help='Exact source in the previous full Pages snapshot; replace only its Vigilia route')
    stage(parser.parse_args())
