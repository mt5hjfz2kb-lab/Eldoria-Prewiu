"""Prepare a browser-verified compact candidate and its small existing Sites shell.
No device certification. Invocation is a publication preparation step, not approval.
"""
import hashlib,importlib.util,json,os,re,pathlib,shutil,subprocess
p=pathlib.Path('site/beta-native325');shell=pathlib.Path('owner-shell')
revision=os.environ['CANDIDATE_REVISION']
report=json.loads(pathlib.Path('evidence/candidate/report.json').read_text())
assert report['passed'] and not report['errors'] and report['engine']=='webkit' and report['variant']=='unchanged'
assert report['identity']['revision']==revision and report['entry']['reloaded'] and report['entry']['observationSeconds']==45
events=pathlib.Path('evidence/candidate/events.jsonl').read_text()
assert 'REGION1_CANON_WORLD_INSTALLED' in events and 'ELDORIA_CONTEXT_LOST' not in events and 'Apple GPU' in events
identity=json.loads((p/'release-identity.json').read_text())
assert identity['source_sha']==os.environ['NATIVE_SOURCE']
assert identity['candidate_bundle_sha256']==report['identity']['candidate_bundle_sha256']
assert identity['route_buffers_reused'] and identity['exact_population_and_routes_verified']
assert identity['native_parity_verified'] and identity['native_journey_verified'] and not identity['physical_mobile_tested']
assert identity['terrain_query_runtime_cooking'] is False and identity['terrain_query_cache_sha256']==report['identity']['terrain_query_cache_sha256']
assert 'ELDORIA_TERRAIN_QUERY_TAPE' in events and 'Source mesh has over 2,097,152 triangles' not in events
control=json.loads(pathlib.Path('evidence/control/report.json').read_text())
assert control['passed'] and control['identity']['revision']=='native325-mobile6-cubemap'
def used(r):return max((x.get('metrics') or {}).get('usedWASMHeapSize',0) for x in r['samples'])
def rss(r):return max(x.get('rssKiB',0) for sample in r['samples'] for x in sample['processes'] if 'WebContent' in x.get('name',''))
assert used(report)<used(control)*.8, 'Require a substantial measured heap reduction before another owner retry'
first_candidate=json.loads(pathlib.Path('first-qa-evidence/evidence/candidate/report.json').read_text())
first_control=json.loads(pathlib.Path('first-qa-evidence/evidence/control/report.json').read_text())
assert first_candidate['passed'] and first_control['passed']
assert first_candidate['identity']['candidate_bundle_sha256']==identity['candidate_bundle_sha256']
assert used(first_candidate)<used(first_control)*.8
# Include both orders, never select only the more favorable run.
mean_candidate=(rss(report)+rss(first_candidate))/2
mean_control=(rss(control)+rss(first_control))/2
assert mean_candidate<mean_control, 'Mean resident peak across both orders must improve'
memory_evidence={'runs':[42519214500,int(os.environ['QA_RUN'])],
 'control_peak_used_bytes':[used(first_control),used(control)],
 'candidate_peak_used_bytes':[used(first_candidate),used(report)],
 'control_peak_rss_kib':[rss(first_control),rss(control)],
 'candidate_peak_rss_kib':[rss(first_candidate),rss(report)],
 'interpretation':'Desktop samples; both orders retained; NOT physical iPhone certification'}

manifest=json.loads((p/'build-manifest.json').read_text());renamed={}
for key in ('loader','framework','code'):
 old=manifest[key];oldpath=old.split('?')[0];src=p/oldpath;digest=hashlib.sha256(src.read_bytes()).hexdigest()
 newpath='Build/'+digest[:20]+'-'+src.name;src.rename(p/newpath);renamed[oldpath]=newpath;manifest[key]=newpath
 for field in ('decoded_sizes','decoded_parts'):
  if old in manifest.get(field,{}):manifest[field][newpath]=manifest[field].pop(old)
for parts in manifest['decoded_parts'].values():
 for part in parts:
  if part['url'] in renamed:part['url']=renamed[part['url']]
for item in identity['decoded_files']:
 for field in ('url','path'):
  if item.get(field) in renamed:item[field]=renamed[item[field]]
entry='https://eldoria-valoria-beta-20261108.kbkvc5c45n.chatgpt.site/?eldoria_diag=1&build='+revision
identity['desktop_memory_comparison']=memory_evidence
identity.update(native_input_restoration_run=int(os.environ['NATIVE_RUN']),game_entry_url=entry,runtime_verified=True,runtime_qa_run=int(os.environ['QA_RUN']),release_for_external_testers=False,physical_mobile_tested=False,physical_iphone_verified=False,release_purpose='Owner physical iPhone verification; NOT physical-device certified',runtime_environment='macOS Apple GPU WebKit; NOT physical iPhone')
(p/'build-manifest.json').write_text(json.dumps(manifest,indent=2));(p/'release-identity.json').write_text(json.dumps(identity,indent=2))
subprocess.run(['node','tools/delivery/test-exact-decoded-transport.mjs',str(p)],check=True)
shell.mkdir(exist_ok=False)
for name in ('index.html','style.css','city.png','play.js','release-identity.json'):
 shutil.copyfile(p/name,shell/name)
for f in p.glob('play-*.js'):shutil.copyfile(f,shell/f.name)
if (p/'TemplateData').exists():shutil.copytree(p/'TemplateData',shell/'TemplateData')
# Same-origin shell; heavy bytes remain on the existing owner-authorized Pages host.
base='https://mt5hjfz2kb-lab.github.io/Eldoria-Prewiu/beta-native325/'
keys={manifest[k]:base+manifest[k] for k in ('loader','framework','code','data')}
for key in ('loader','framework','code','data'):manifest[key]=keys[manifest[key]]
manifest['decoded_sizes']={keys.get(k,k):v for k,v in manifest['decoded_sizes'].items()}
manifest['decoded_parts']={keys.get(k,k):[{**x,'url':base+x['url']} for x in parts] for k,parts in manifest['decoded_parts'].items()}
(shell/'build-manifest.json').write_text(json.dumps(manifest,indent=2))
redirect='<!doctype html><html lang="es"><meta charset="utf-8"><meta name="robots" content="noindex,nofollow"><meta http-equiv="refresh" content="0;url='+entry+'"><title>Eldoria</title><a href="'+entry+'">Entrar en Valoria</a></html>'
for alias in ('index.html','mobile-v9.html','mobile-v8.html','mobile-v7.html','mobile-v6.html','mobile-v4.html','mobile-v3.html','mobile-v1.html'):(p/alias).write_text(redirect)
spec=importlib.util.spec_from_file_location('prep','tools/eldoria-private-beta/prepare-pages.py');m=importlib.util.module_from_spec(spec);spec.loader.exec_module(m)
previous=m.mirror_previous('site');pathlib.Path('site/.nojekyll').touch()
size=sum(f.stat().st_size for f in pathlib.Path('site').rglob('*') if f.is_file());assert size<1073741824,size
print('COMPACT_CANDIDATE_PAGES_READY',size,'legacy_files',len(previous))
