"""Assess an isolated residency experiment without certifying physical iPhone."""
import json,pathlib,re,sys
root=pathlib.Path(sys.argv[1]);native=len(sys.argv)==3 and sys.argv[2]=='--native'
assert len(sys.argv)==2 or native
source='05e1846f0213c6320850505bc054110c53934f55' if native else 'ca5e1c3d52a58d8c6d9c9a9114d2d35445a18029'
if native:
 proof=json.loads((root/'native-build-identity.json').read_text())
 assert proof['source_sha']==source and proof['native_journey_verified'] and proof['native_destination_captures_verified']
else:
 proof=json.loads((root/'integrity.json').read_text())
 assert proof['all_objects_verified']==57656 and len(proof['changes'])==4
 assert proof['geometry_collision_and_textures_byte_identical']
rows=[]
for name in ('candidate','control'):
 report=json.loads((root/name/'report.json').read_text())
 assert report['passed'] and report['worldEntryAndReentryVerified'] and report['constructionReloadVerified'],name
 assert not report['errors'] and not report['physicalIphone'],name
 assert report['source']==source,name
 entries=report['worldEntries'];assert len(entries)==2
 samples=[s for e in entries for s in e['samples'] if s.get('metrics')]
 assert samples and all(not s.get('gpuLedger') for s in samples),'Use unperturbed rendering in comparison'
 log=(root/name/'console.log').read_text()
 if native:
  marker='ELDORIA_P0_STATIC_MESH_CPU_RETIRED count=4'
  if name=='candidate':
   assert marker in log and log.index(marker)<log.index('ELDORIA_P0_LAZY_WORLD_REQUESTED')
  else:assert marker not in log,'Control accidentally retired CPU mesh copies' 
 meshes=re.findall(r'ELDORIA_MEMORY_ASSETS phase=creatures-after type=Mesh count=(\d+) bytes=(\d+)',log)
 assert len(meshes)==1,meshes
 processes=[]
 for sample in samples:
  sums={'WebContent':0,'GPU':0}
  for line in sample.get('processes',[]):
   m=re.match(r'\s*\d+\s+\d+\s+(\d+)\s+(.*)',line)
   if not m:continue
   if 'WebContent' in m[2]:sums['WebContent']+=int(m[1])
   elif 'GPU' in m[2]:sums['GPU']+=int(m[1])
  processes.append(sums)
 rows.append(dict(name=name,mesh_count=int(meshes[0][0]),mesh_unity_bytes=int(meshes[0][1]),
  peak_used_wasm_bytes=max(s['metrics']['usedWASMHeapSize'] for s in samples),
  peak_wasm_capacity_bytes=max(s['metrics']['totalWASMHeapSize'] for s in samples),
  sampled_peak_combined_webcontent_gpu_kib=max(sum(p.values()) for p in processes),
  entry_wait_ms=[e['observedEntryWaitMs'] for e in entries]))
a,b=rows
result=dict(scope='DESKTOP_WEBKIT_NOT_PHYSICAL_IPHONE',rows=rows,
 mesh_unity_bytes_reduced=b['mesh_unity_bytes']-a['mesh_unity_bytes'],
 peak_used_wasm_bytes_reduced=b['peak_used_wasm_bytes']-a['peak_used_wasm_bytes'],
 physical_crash_resolved=False,root_cause_confirmed=False,native_source_equivalence_tested=native,
 criterion='At least40MiB used-WASM reduction before spending a native compile cycle; preserve real saved gameplay.')
result['worth_native_validation']=result['peak_used_wasm_bytes_reduced']>=40*1024*1024
(root/'comparison.json').write_text(json.dumps(result,indent=2));print(json.dumps(result))
assert result['worth_native_validation'],'Insufficient demonstrated benefit: investigate instead of publishing'
