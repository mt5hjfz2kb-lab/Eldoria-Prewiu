#!/usr/bin/env python3
"""Assemble an immutable owner-only iPhone A/B Pages preview without replacing beta10.

Input one previously verified beta10 Pages tree and one newly certified WebGL
candidate. Every byte of beta10 is retained; candidate goes to a NEW subpath.
This is source staging, not proof that a new build runs on a physical iPhone.
"""
import argparse
import gzip
import hashlib
import json
import os
import shutil
from pathlib import Path, PurePosixPath

STABLE_ROUTE = 'beta-native325'
CANDIDATE_ROUTE = 'beta-iphone-clock-ab'
STABLE_REVISION = 'native325-mobile10-pretest-ux'
STABLE_SOURCE = 'be49d62e3daf490f41dba0bf8123a8e2eee64480'
STABLE_BUNDLE = '57bf7878a89482c0403a66652db671456bed1748bf2c56c13b229839a05f0af2'
BYTE_LIMIT = 980_000_000  # below official 1 GiB; never silently drop old paths


def ensure(cond, msg):
    if not cond:
        raise ValueError(msg)


def load_json(path):
    return json.loads(path.read_text(encoding='utf-8-sig'))


def safe_rel(name):
    ensure(isinstance(name, str) and name, 'Empty manifest URL')
    ensure('?' not in name or name.partition('?')[0], 'Invalid manifest URL')
    before_query = name.split('?', 1)[0]
    p = PurePosixPath(before_query)
    ensure(not p.is_absolute() and '..' not in p.parts and ':' not in name
           and '\\' not in name and not name.startswith('//'), 'Unsafe or remote URL: '+name)
    return p


def files(base):
    for path in sorted(base.rglob('*')):
        ensure(not path.is_symlink(), 'Site symlinks are not permitted: '+str(path))
        if path.is_file():
            yield path


def sha256_file(path):
    h=hashlib.sha256()
    with path.open('rb') as f:
        for data in iter(lambda:f.read(1024*1024), b''):
            h.update(data)
    return h.hexdigest()


def fingerprint_tree(base):
    return {p.relative_to(base).as_posix():(p.stat().st_size,sha256_file(p)) for p in files(base)}


def check_binary_site(site, expected_rev, expected_source, expected_bundle, require_native):
    identity=load_json(site/'release-identity.json')
    ensure(identity.get('revision')==expected_rev, 'Wrong release revision: '+str(site))
    ensure(identity.get('source_sha')==expected_source, 'Wrong Unity source: '+str(site))
    ensure(identity.get('candidate_bundle_sha256')==expected_bundle, 'Wrong Unity bundle: '+str(site))
    ensure(identity.get('release_for_external_testers') is False,
           'External tester gate must remain disabled in both builds')
    ensure((site/'index.html').is_file() and (site/'play.js').is_file(),
           'Missing genuine Unity game entry')
    ensure(b'<html' in (site/'index.html').read_bytes().lower(), 'Missing HTML document')
    if require_native:
        ensure(identity.get('native_journey_verified') is True,
               'Candidate must pass exact Unity I-V PlayMode and WebGL tests')
        ensure(identity.get('physical_mobile_tested') is False,
               'This preview does not certify a physical phone')
        # A beta10 page that redirects to the mutable main route would only
        # appear to show the candidate. A/B must execute its own binary files.
        html=(site/'index.html').read_text(encoding='utf-8')
        script=(site/'play.js').read_text(encoding='utf-8')
        ensure('http-equiv="refresh"' not in html.lower() and
               "http-equiv='refresh'" not in html.lower(),
               'Candidate entry redirects instead of starting the independent player')
        old_data_root='https://mt5hjfz2kb-lab.github.io/Eldoria-Prewiu/beta-native325/'
        ensure(old_data_root not in html and old_data_root not in script,
               'Candidate shell still targets the old stable beta10 binary route')
    manifest=load_json(site/'build-manifest.json')
    decoded=manifest.get('decoded_parts',{})
    data_url=manifest['data']
    data_parts=decoded.get(data_url)
    # The real beta10/mobile12 Unity WebGL transport NEVER materializes a
    # 503 MiB .data.unityweb monolith. Its loader assembles 31 separately
    # gzip-compressed chunks recorded under the exact manifest data URL.
    # Requiring that nonexistent file rejected a correctly archived game.
    for name in ('loader','framework','code','data'):
        file=site/safe_rel(manifest[name])
        if name=='data' and not file.is_file():
            ensure(isinstance(data_parts,list) and len(data_parts)>=2,
                   'Missing WebGL data requires a complete streamed chunk manifest')
        else:
            ensure(file.is_file() and file.stat().st_size>0,
                   'Missing WebGL '+name+' '+str(file))
    for asset, parts in decoded.items():
        ensure(isinstance(parts,list) and len(parts)>0,
               'Empty decoded binary parts: '+asset)
        total=0
        for item in parts:
            path=site/safe_rel(item['url'])
            ensure(path.is_file() and path.stat().st_size>0,
                   'Missing binary part '+str(path))
            if 'sha256' in item:
                ensure(sha256_file(path)==item['sha256'],
                       'Corrupt compressed asset '+str(path))
            if 'decoded_sha256' in item:
                ensure(item.get('encoding')=='gzip',
                       'Decoded SHA requires a verifiable gzip stream')
                checksum=hashlib.sha256()
                size=0
                with gzip.open(path,'rb') as stream:
                    for block in iter(lambda:stream.read(1024*1024),b''):
                        checksum.update(block)
                        size+=len(block)
                ensure(size==item['decoded_bytes'] and
                       checksum.hexdigest()==item['decoded_sha256'],
                       'Corrupt decoded WebGL binary: '+str(path))
                total+=size
            elif 'decoded_bytes' in item:
                ensure(False,'Decoded bytes without decoded SHA: '+str(path))
        expected_size=manifest.get('decoded_sizes',{}).get(asset)
        if expected_size is not None:
            ensure(total==expected_size,
                   'Streamed WebGL byte-count mismatch: '+asset)
    if not (site/safe_rel(data_url)).is_file():
        ensure(data_parts is not None and
               all('decoded_sha256' in part for part in data_parts),
               'No monolithic data: every streamed chunk needs decoded SHA256')
        ensure(data_url in manifest.get('decoded_sizes',{}),
               'No monolithic data: decoded total bytes must be recorded')
    return identity


def stage(stable_root, candidate_root, destination, candidate_revision, candidate_source,
          candidate_bundle, byte_limit=BYTE_LIMIT):
    stable_root=Path(stable_root).resolve()
    candidate_root=Path(candidate_root).resolve()
    dest=Path(destination).resolve()
    ensure(stable_root.is_dir() and candidate_root.is_dir(), 'Both input sites required')
    ensure(not dest.exists(), 'Never overwrite an existing preview or public release')
    ensure(not dest.is_relative_to(stable_root) and not dest.is_relative_to(candidate_root),
           'Preview output must be independent of both source trees')
    stable=stable_root/STABLE_ROUTE
    a=check_binary_site(stable,STABLE_REVISION,STABLE_SOURCE,STABLE_BUNDLE,False)
    b=check_binary_site(candidate_root,candidate_revision,candidate_source,candidate_bundle,True)
    ensure(a['source_sha']!=b['source_sha'], 'Candidate is not a different Unity build')
    ensure(CANDIDATE_ROUTE not in {x.name for x in stable_root.iterdir()},
           'Separate iPhone QA route already exists; refuse overwrite')
    original=fingerprint_tree(stable_root)
    approx=sum(v[0] for v in original.values())+sum(p.stat().st_size for p in files(candidate_root))
    ensure(approx < byte_limit, 'Too large to stage both games in one GitHub Pages deployment')
    staging=dest.with_name(dest.name+'.unfinished')
    ensure(not staging.exists(), 'Previous incomplete staging exists')
    try:
        shutil.copytree(stable_root,staging,symlinks=False)
        shutil.copytree(candidate_root,staging/CANDIDATE_ROUTE,symlinks=False)
        ensure(fingerprint_tree(staging/STABLE_ROUTE)==
               {name.removeprefix(STABLE_ROUTE+'/'):v for name,v in original.items()
                if name.startswith(STABLE_ROUTE+'/')},
               'Original playable beta10 bytes changed: refuse staging')
        # Keep original root/legacy entries unchanged too.
        for name,(size,digest) in original.items():
            f=staging/name
            ensure(f.is_file() and f.stat().st_size==size and sha256_file(f)==digest,
                   'Stable Pages route changed: '+name)
        ensure(check_binary_site(staging/CANDIDATE_ROUTE,candidate_revision,
                                 candidate_source,candidate_bundle,True)==b,
               'Copied candidate identity differs')
        total=sum(p.stat().st_size for p in files(staging))
        ensure(total < byte_limit, 'Combined pages payload exceeds safety budget')
        proof={
            'schema':1,'role':'OWNER_AB_IOS_QA_ONLY',
            'external_testers_enabled':False,'physical_iphone_stability_verified':False,
            'stable_route':STABLE_ROUTE+'/',
            'stable_source_sha':STABLE_SOURCE,'stable_revision':STABLE_REVISION,
            'stable_manifest_sha256':sha256_file(staging/STABLE_ROUTE/'build-manifest.json'),
            'candidate_route':CANDIDATE_ROUTE+'/',
            'candidate_source_sha':candidate_source,
            'candidate_revision':candidate_revision,
            'candidate_bundle_sha256':candidate_bundle,
            'candidate_manifest_sha256':sha256_file(staging/CANDIDATE_ROUTE/'build-manifest.json'),
            'combined_bytes':total,
            'original_paths_preserved':len(original),
            'notice':'Only an isolated, pre-certified native WebGL candidate. Physical iPhone trial pending.'
        }
        (staging/'beta-iphone-clock-ab-proof.json').write_text(
            json.dumps(proof,indent=2,ensure_ascii=False)+'\n',encoding='utf-8')
        staging.rename(dest)
        return proof
    except BaseException:
        if staging.exists():shutil.rmtree(staging)
        raise


def main():
    a=argparse.ArgumentParser(description=__doc__)
    a.add_argument('--stable',required=True,type=Path)
    a.add_argument('--candidate',required=True,type=Path)
    a.add_argument('--out',required=True,type=Path)
    a.add_argument('--candidate-revision',required=True)
    a.add_argument('--candidate-source',required=True)
    a.add_argument('--candidate-bundle',required=True)
    a.add_argument('--max-bytes',type=int,default=BYTE_LIMIT)
    cfg=a.parse_args()
    print(json.dumps(stage(cfg.stable,cfg.candidate,cfg.out,cfg.candidate_revision,
                           cfg.candidate_source,cfg.candidate_bundle,cfg.max_bytes),indent=2))

if __name__=='__main__':main()
