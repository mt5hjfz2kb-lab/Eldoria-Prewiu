"""Apply ONLY the desktop-QA-verified canvas wrapper to the owner candidate.

Never edits Unity binaries, the beta10 route, saves or tester release gates.
Requires the exact before/after script hashes emitted by real browser A/B QA.
"""
import argparse, hashlib, json, re
from pathlib import Path

def sha(data):
    return hashlib.sha256(data).hexdigest()

def stage(site, module, proof_path):
    site, module, proof_path = map(Path, (site, module, proof_path))
    candidate = site / 'beta-iphone-clock-ab'
    proof = json.loads(proof_path.read_text())
    assert proof['engine_modified'] is False and proof['physical_iphone_tested'] is False
    expected = {x['file']: x for x in proof['shell_changes']}
    assert expected and set(expected) == {p.name for p in candidate.glob('play*.js')}
    source = module.read_text()
    mutations = {}
    for name, row in expected.items():
        before = (candidate / name).read_text()
        assert sha(before.encode()) == row['before'], 'Stale startup script: ' + name
        assert before.count('matchWebGLToCanvasSize:true') == 1
        assert before.count('const instance=await createUnityInstance(canvas,{') == 1
        after = source + '\n' + before.replace('matchWebGLToCanvasSize:true', 'matchWebGLToCanvasSize:false').replace(
            'const instance=await createUnityInstance(canvas,{',
            'window.__eldoriaStableCanvas=installEldoriaStableCanvas(canvas);\n  const instance=await createUnityInstance(canvas,{')
        assert sha(after.encode()) == row['after'], 'Not the browser-tested script: ' + name
        mutations[name] = after
    identity = json.loads((candidate / 'release-identity.json').read_text())
    assert identity['source_sha'] == '004d3ef9c26e9070762e9e38ce84d69d525efd4f'
    assert identity['release_for_external_testers'] is False
    script = mutations['play.js']
    filename = 'play-' + sha(script.encode())[:16] + '.js'
    html = (candidate / 'index.html').read_text()
    html, count = re.subn(r'src="play(?:-[a-f0-9]+)?\.js(?:\?[^"]*)?"', 'src="'+filename+'"', html)
    assert count == 1
    # All guards precede mutation. Native engine source remains unchanged.
    for name, text in mutations.items():
        (candidate / name).write_text(text)
    (candidate / filename).write_text(script)
    (candidate / 'index.html').write_text(html)
    identity['shell_revision'] = 'stable-canvas-r1'
    identity['shell_source_commit'] = proof['source']
    identity['shell_script_sha256'] = sha(script.encode())
    identity['physical_iphone_verified'] = False
    (candidate / 'release-identity.json').write_text(json.dumps(identity, indent=2)+'\n')
    (candidate / 'stable-canvas-proof.json').write_text(json.dumps(proof, indent=2)+'\n')
    print(json.dumps({'shell_revision':identity['shell_revision'], 'script':filename,
                      'script_sha256':identity['shell_script_sha256'], 'external_testers_enabled':False}))

if __name__ == '__main__':
    p=argparse.ArgumentParser()
    p.add_argument('site');p.add_argument('module');p.add_argument('proof')
    a=p.parse_args();stage(a.site,a.module,a.proof)
