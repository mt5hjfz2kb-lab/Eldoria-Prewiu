#!/usr/bin/env python3
"""Compare every protected staged file with pinned archive and actual live bytes."""
import argparse
import concurrent.futures
import hashlib
import json
import os
import sys
import urllib.request
from pathlib import Path

sys.path.insert(0, str(Path(__file__).with_name('iphone_ab')))
from stage_compact_delta_ab import restore_tar
from stage_engine18_owner_site import NEW_ROUTE, NEW_SOURCE, PREVIOUS_SOURCE, PROOF, tree

ORIGIN = 'https://mt5hjfz2kb-lab.github.io/Eldoria-Prewiu/'


def run(args):
    site, baseline = Path(args.site), Path(args.baseline)
    proof = json.loads((site / PROOF).read_text())
    assert proof['source_sha'] == NEW_SOURCE and not proof['published']
    assert not proof['external_testers_enabled'] and not proof['physical_iphone_verified']
    restore_tar(args.archive, baseline)
    previous = tree(baseline)
    expected = {k: dict(bytes=v[0], sha256=v[1]) for k, v in previous.items()
                if not k.startswith(NEW_ROUTE + '/')}
    assert proof['preserved_fingerprints'] == expected
    assert len(expected) >= 144
    for rel, record in expected.items():
        path = site / rel
        assert path.stat().st_size == record['bytes'], rel
        assert hashlib.sha256(path.read_bytes()).hexdigest() == record['sha256'], rel
    before = json.loads((baseline / NEW_ROUTE / 'release-identity.json').read_text())
    assert before['source_sha'] == PREVIOUS_SOURCE
    request = urllib.request.Request(ORIGIN + NEW_ROUTE + '/release-identity.json?verify=' +
                                    os.environ['GITHUB_RUN_ID'], headers={'Cache-Control': 'no-cache'})
    live = json.load(urllib.request.urlopen(request, timeout=40))
    assert live['source_sha'] == PREVIOUS_SOURCE, 'Live candidate changed while stage was prepared'

    def live_hash(item):
        rel, record = item
        request = urllib.request.Request(ORIGIN + rel + '?owner_preservation=' +
                                        os.environ['GITHUB_RUN_ID'],
                                        headers={'Accept-Encoding': 'identity', 'Cache-Control': 'no-cache'})
        with urllib.request.urlopen(request, timeout=60) as response:
            actual = hashlib.file_digest(response, 'sha256').hexdigest()
        assert actual == record['sha256'], 'Live protected route changed: ' + rel
        return rel

    public = [(rel, record) for rel, record in expected.items()
              if rel != 'CNAME' and not any(x.startswith('.') for x in Path(rel).parts)]
    with concurrent.futures.ThreadPoolExecutor(max_workers=4) as pool:
        checked = list(pool.map(live_hash, public))
    output = dict(source_sha=NEW_SOURCE, protected_files_verified=len(expected),
                  protected_live_files_sha256_verified=len(checked),
                  previous_pages_run=43152671564, owner_route_unchanged=NEW_ROUTE,
                  browser_saves_accessed=False, physical_iphone_verified=False,
                  external_testers_enabled=False)
    Path(args.proof).write_text(json.dumps(output, indent=2) + '\n')
    print('ALL_OTHER_EXISTING_OWNER_ROUTES_EXACT_ARCHIVE_AND_LIVE_SHA_PASS', json.dumps(output))


if __name__ == '__main__':
    parser = argparse.ArgumentParser()
    for key in ('site', 'baseline', 'archive', 'proof'):
        parser.add_argument('--' + key, required=True)
    run(parser.parse_args())
