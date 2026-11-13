"""Publish only the independently verified five-cubemap candidate; keep owner site."""
import importlib.util,json,hashlib,pathlib,subprocess
p=pathlib.Path('site/beta-native325')
reports=[]
for folder in ['evidence-1.55.0','evidence-1.65.0']:
    r=json.loads(pathlib.Path(folder,'report.json').read_text())
    assert r['passed'] and not r['errors'] and r['engine']=='webkit' and r['variant']=='unchanged'
    assert r['entry']['reloaded'] and r['entry']['observationSeconds']==45
    assert r['identity']['revision']=='native325-mobile6-cubemap'
    events=pathlib.Path(folder,'events.jsonl').read_text()
    assert 'REGION1_CANON_WORLD_INSTALLED' in events and 'ELDORIA_CONTEXT_LOST' not in events
    gl=[json.loads(x)['text'] for x in events.splitlines() if 'ELDORIA_ACTUAL_GL ' in x]
    assert gl and all('Apple GPU' in x for x in gl)
    reports.append(r)
identity=json.loads((p/'release-identity.json').read_text())
proof=identity['cubemap_compatibility']
assert identity['candidate_bundle_sha256']=='e54d547487e0d915f5a8c3fbbd0354a79672041dd201b3e63672de5e740ea6c1'
assert proof['all_objects_verified']==57375 and proof['changed_cubemaps']==5 and proof['decoded_faces_and_mips']==240
assert identity['native_parity_verified'] and identity['native_journey_verified'] and identity['physical_mobile_tested'] is False
manifest=json.loads((p/'build-manifest.json').read_text());renamed={}
identity['decoded_files']=[x for x in identity['decoded_files'] if not x.get('url','').startswith('payload-decoded/')]+manifest['decoded_parts'][manifest['data']]
for key in ['loader','framework','code']:
    old=manifest[key];oldpath=old.split('?')[0];src=p/oldpath;digest=hashlib.sha256(src.read_bytes()).hexdigest()
    newpath='Build/'+digest[:20]+'-'+src.name;src.rename(p/newpath);renamed[oldpath]=newpath;manifest[key]=newpath
    for field in ['decoded_sizes','decoded_parts']:
        if old in manifest.get(field,{}):manifest[field][newpath]=manifest[field].pop(old)
for parts in manifest['decoded_parts'].values():
    for part in parts:
        if part['url'] in renamed:part['url']=renamed[part['url']]
for item in identity['decoded_files']:
    for field in ['url','path']:
        if item.get(field) in renamed:item[field]=renamed[item[field]]
(p/'build-manifest.json').write_text(json.dumps(manifest,indent=2))
subprocess.run(['node','tools/delivery/test-exact-decoded-transport.mjs',str(p)],check=True)
entry_url='https://eldoria-valoria-beta-20261108.kbkvc5c45n.chatgpt.site/?eldoria_diag=1&build=mobile6-cubemap'
peak=max(s['wasmCapacityBytes'] for r in reports for s in r['samples'])
identity.update(engine_cache_addressed=True,game_entry_url=entry_url,external_parts_sha256={},runtime_verified=True,runtime_qa_run=42493894120,entry_qa_passed=True,graceful_reopen_verified=True,raw_reload_verified=False,release_for_external_testers=False,release_purpose='Owner physical iPhone verification; not certified on physical iPhone',runtime_environment='macOS Apple GPU, WebKit 26 and 27; NOT physical iPhone',actual_renderer='Apple GPU',peak_wasm_capacity_bytes=peak)
(p/'release-identity.json').write_text(json.dumps(identity,indent=2))
redirect='<!doctype html><html lang="es"><meta charset="utf-8"><meta name="robots" content="noindex,nofollow"><meta http-equiv="refresh" content="0;url='+entry_url+'"><title>Eldoria</title><a href="'+entry_url+'">Entrar en Valoria</a></html>'
for alias in ['index.html','mobile-v6.html','mobile-v4.html','mobile-v3.html','mobile-v1.html']:(p/alias).write_text(redirect)
spec=importlib.util.spec_from_file_location('prep','tools/eldoria-private-beta/prepare-pages.py');m=importlib.util.module_from_spec(spec);spec.loader.exec_module(m)
previous=m.mirror_previous('site');pathlib.Path('site/.nojekyll').touch()
size=sum(f.stat().st_size for f in pathlib.Path('site').rglob('*') if f.is_file());assert size<1073741824,size
print('CUBEMAP_CANDIDATE_PAGES_READY',size,'legacy_files',len(previous))
