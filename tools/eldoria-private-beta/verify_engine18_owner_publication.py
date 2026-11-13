#!/usr/bin/env python3
"""Final owner-only input gate; no physical-phone or tester self-certification."""
import argparse
import base64
import hashlib
import json
import os
import urllib.request
from pathlib import Path
from types import SimpleNamespace

from stage_engine18_owner_site import NEW_ROUTE, NEW_SOURCE, NEW_BUNDLE, PROOF, files, verify_binary_urls
from verify_engine18_preservation import run as verify_preservation

STAGE_RUN = 43283946677
STAGE_SOURCE = 'd90b2bd28db2fb3b23e5440c876e26df7e05b839'
SITE_ARTIFACT = 13974700649
SITE_DIGEST = 'dc072e36fd5726c30f7ff7263bfa531733540798e893ed8005ecc6f8783446d2'
PUBLICATION_PROOF = 'beta-mobile18-owner-publication-proof.json'


def get_json(path):
    return json.loads(Path(path).read_text(encoding='utf-8-sig'))


def api(path):
    url = 'https://api.github.com/repos/' + os.environ['GITHUB_REPOSITORY'] + '/' + path
    request = urllib.request.Request(url, headers={'Authorization': 'Bearer ' + os.environ['GH_TOKEN'],
                                                  'Accept': 'application/vnd.github+json'})
    return json.load(urllib.request.urlopen(request, timeout=40))


def validate_desktop(report):
    assert report['passed'] is True and report['errors'] == []
    assert report['entry']['reloaded'] is True and report['lazyWorldDeferredToFirstEntryVerified'] is True
    assert report['identity']['source_sha'] == NEW_SOURCE
    assert report['identity']['candidate_bundle_sha256'] == NEW_BUNDLE
    assert len(report['storyAdvanceAttempt']['actualCanvasClicks']) == 5
    assert report['identity']['release_for_external_testers'] is False


def run(args):
    site, mac = Path(args.site), Path(args.mac)
    result = api('actions/runs/' + str(STAGE_RUN))
    assert result['head_sha'] == STAGE_SOURCE and result['status'] == 'completed' and result['conclusion'] == 'success'
    artifact = api('actions/artifacts/' + str(SITE_ARTIFACT))
    assert artifact['workflow_run']['id'] == STAGE_RUN and artifact['digest'] == 'sha256:' + SITE_DIGEST
    gate_meta = api('contents/pipeline/tester-readiness/release-request.json?ref=main')
    gate = json.loads(base64.b64decode(gate_meta['content']))
    assert gate['enabled'] is False and gate['status'] == 'BLOCKED'
    reports = [p for p in mac.rglob('report.json') if p.parent.name == 'deferred']
    assert len(reports) == 1
    report = get_json(reports[0])
    validate_desktop(report)
    proof = get_json(site / PROOF)
    assert proof['source_sha'] == NEW_SOURCE and proof['native_bundle_sha256'] == NEW_BUNDLE
    assert proof['data_verified_bytes'] == 513656003 and proof['target_parts'] == 31
    assert proof['published'] is False and proof['external_testers_enabled'] is False
    assert proof['original_files_preserved'] == 146 and len(proof['preserved_fingerprints']) == 146
    identity = get_json(site / NEW_ROUTE / 'release-identity.json')
    assert identity['source_sha'] == NEW_SOURCE and identity['candidate_bundle_sha256'] == NEW_BUNDLE
    manifest = get_json(site / NEW_ROUTE / 'build-manifest.json')
    assert len(verify_binary_urls(site / NEW_ROUTE, manifest)) == 2
    verify_preservation(SimpleNamespace(site=args.site, baseline=args.baseline, archive=args.archive,
                                        proof=args.live_proof))
    output = dict(role='OWNER_ONLY_ENGINE18_PUBLICATION_INPUT_VERIFIED', source_sha=NEW_SOURCE,
                  native_bundle_sha256=NEW_BUNDLE, stage_run=STAGE_RUN, stage_source=STAGE_SOURCE,
                  staged_site_artifact=SITE_ARTIFACT, staged_site_digest=SITE_DIGEST,
                  preserved_files=146, data_verified_bytes=513656003,
                  actual_staged_desktop_webkit_passed=True, actual_staged_reload_passed=True,
                  previous_pages_run=43152671564, owner_route=NEW_ROUTE,
                  world_entry_physical_verified=False, physical_iphone_verified=False,
                  art_approved=False, external_testers_enabled=False,
                  publication_workflow_run=os.environ['GITHUB_RUN_ID'])
    target = site / PUBLICATION_PROOF
    assert not target.exists(), 'Do not overwrite an earlier publication proof'
    target.write_text(json.dumps(output, indent=2) + '\n')
    assert sum(p.stat().st_size for p in files(site)) < 980000000
    print('EXACT_PRESERVED_ENGINE18_OWNER_PUBLICATION_READY_NOT_TESTER_RELEASE', json.dumps(output))


if __name__ == '__main__':
    parser = argparse.ArgumentParser()
    for key in ('site', 'baseline', 'mac', 'archive', 'live-proof'):
        parser.add_argument('--' + key, required=True)
    run(parser.parse_args())
