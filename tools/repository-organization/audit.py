#!/usr/bin/env python3
"""Read-only Git-tree inventory. Duplicate bytes are not a deletion recommendation."""
import argparse
import collections
import json
import posixpath
import re
import subprocess
from urllib.parse import unquote, urlsplit


def git(*args):
    return subprocess.check_output(['git', *args])


def category(path):
    if path.startswith('Unity/'):
        return 'unity-project-preserve-guid-and-dynamic-loads'
    if path.startswith(('docs/evidence/', 'docs/archive/', 'pipeline/evidence/')):
        return 'historical-evidence-preserve-provenance'
    if re.match(r'(v\d[^/]*|tester-v0265)/', path):
        return 'legacy-web-preserve-parity-and-snapshots'
    if path.startswith(('ArtSource/', 'art-source/', 'references/', 'artifacts/', 'assets/')):
        return 'art-source-reference-or-evidence-review-licence-and-links'
    if path.startswith('docs/'):
        return 'documentation-check-catalog-and-specialist-authority'
    if path.startswith(('tools/', 'scripts/', 'qa/', 'tests/', '.github/')):
        return 'tooling-tests-and-automation'
    if path.startswith(('pipeline/', 'updates/')):
        return 'operational-records-and-delivery'
    return 'root-entry-contract-or-legacy-launcher'


def local_links(text):
    # Deliberately limited to inline Markdown links outside fenced code blocks.
    # No claim to validate anchors, reference-style links, HTML or dynamic loaders.
    text = re.sub(r'^(```|~~~).*?^\1[^\n]*$', '', text, flags=re.M | re.S)
    for raw in re.findall(r'!?\[[^\]\n]*\]\((<[^>]+>|[^)\n]+)\)', text):
        target = raw.strip()
        if target.startswith('<') and target.endswith('>'):
            target = target[1:-1]
        else:
            target = re.sub(r'\s+[\"\'][^\"\']*[\"\']$', '', target)
        if not target or target.startswith('#') or urlsplit(target).scheme:
            continue
        target = unquote(target.split('#')[0].split('?')[0])
        if target:
            yield target


def audit(ref):
    commit = git('rev-parse', ref + '^{commit}').decode().strip()
    records = []
    for row in git('ls-tree', '-r', '-z', commit).split(b'\0'):
        if not row:
            continue
        metadata, raw_path = row.split(b'\t', 1)
        mode, kind, oid = metadata.decode().split()
        records.append({'path': raw_path.decode(), 'blob': oid, 'kind': kind, 'mode': mode})
    paths = {r['path'] for r in records}
    dirs = set()
    for p in paths:
        parent = posixpath.dirname(p)
        while parent:
            dirs.add(parent)
            parent = posixpath.dirname(parent)
    groups = collections.defaultdict(list)
    folded = collections.defaultdict(list)
    roots = collections.Counter()
    categories = collections.Counter()
    for r in records:
        p = r['path']
        roots[p.split('/')[0] if '/' in p else '[root files]'] += 1
        categories[category(p)] += 1
        folded[p.casefold()].append(p)
        if r['kind'] == 'blob':
            groups[r['blob']].append(p)
    duplicates = [{'blob': oid, 'paths': ps, 'categories': sorted({category(p) for p in ps}),
                   'disposition': 'KEEP_PENDING_DEPENDENCY_AND_PROVENANCE_REVIEW'}
                  for oid, ps in sorted(groups.items()) if len(ps) > 1]
    catalog = json.loads(git('show', commit + ':docs/documentation-catalog.json'))
    states = {e['path']: e['status'] for e in catalog['entries']}
    docs = [r for r in records if r['path'].endswith('.md')]
    # One batch fetch/read; never download binary art simply to count or compare it.
    proc = subprocess.run(['git', 'cat-file', '--batch'],
                          input=''.join(r['blob'] + '\n' for r in docs).encode(),
                          stdout=subprocess.PIPE, check=True)
    data, cursor = proc.stdout, 0
    broken, checked = [], 0
    for r in docs:
        end = data.index(b'\n', cursor)
        header = data[cursor:end].decode().split()
        if len(header) != 3 or header[1] != 'blob':
            raise RuntimeError('Markdown blob unavailable: ' + r['path'])
        size = int(header[2]); start = end + 1
        text = data[start:start + size].decode('utf-8', errors='replace')
        cursor = start + size + 1
        for target in local_links(text):
            checked += 1
            resolved = posixpath.normpath(posixpath.join(posixpath.dirname(r['path']), target))
            if resolved not in paths and resolved not in dirs:
                broken.append({'source': r['path'], 'target': target, 'resolved': resolved,
                               'catalog_status': states.get(r['path'], 'not-in-direct-docs-catalog')})
    return {'schema_version': 1, 'source_commit': commit,
            'source_tree': git('rev-parse', commit + '^{tree}').decode().strip(),
            'coverage': {'tracked_entries': len(records), 'roots': dict(sorted(roots.items())),
                         'categories': dict(sorted(categories.items())),
                         'markdown_files_scanned': len(docs), 'inline_local_links_checked': checked},
            'documentation_status_counts': dict(sorted(collections.Counter(states.values()).items())),
            'documentation_review_pending': sorted(p for p, s in states.items() if s == 'review_pending'),
            'casefold_collisions': [ps for ps in folded.values() if len(ps) > 1],
            'paths_over_240_characters': sorted(p for p in paths if len(p) > 240),
            'duplicate_groups': duplicates, 'unresolved_inline_links': broken,
            'limits': ['Exact blob equality only; distinct paths may be required dependencies or provenance.',
                       'Inline Markdown link paths only; no anchors, HTML or reference-link validation.',
                       'Missing paths may be historical outputs or examples; inspect before editing.',
                       'No unused-Unity-asset verdict: Resources, Addressables and runtime names need native analysis.',
                       'Catalog status is reported as declared, not independently certified as current.',
                       'No remote branch inventory, untracked files, license approval or physical-device QA.']}


if __name__ == '__main__':
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--ref', default='HEAD')
    parser.add_argument('--output', required=True)
    args = parser.parse_args()
    result = audit(args.ref)
    with open(args.output, 'w', encoding='utf-8') as stream:
        json.dump(result, stream, ensure_ascii=False, indent=2)
        stream.write('\n')
    print(json.dumps({'commit': result['source_commit'], 'files': result['coverage']['tracked_entries'],
                      'duplicate_groups': len(result['duplicate_groups']),
                      'markdown_files_scanned': result['coverage']['markdown_files_scanned'],
                      'unresolved_inline_links': len(result['unresolved_inline_links']),
                      'casefold_collisions': len(result['casefold_collisions'])}))
