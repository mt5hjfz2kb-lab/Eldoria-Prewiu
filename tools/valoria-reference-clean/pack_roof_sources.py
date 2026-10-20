"""Lossless storage of oversized editable roof sources; no geometry changes."""
import pathlib,json,hashlib,sys
ROOT=pathlib.Path('docs/evidence/valoria-reference-clean/roof-junction226')
def digest(p):
 h=hashlib.sha256()
 with p.open('rb') as f:
  for b in iter(lambda:f.read(1024*1024),b''):h.update(b)
 return h.hexdigest()
if len(sys.argv)==3 and sys.argv[1]=='--restore':
 manifest=pathlib.Path(sys.argv[2]);d=json.loads(manifest.read_text());target=manifest.parent/d['filename']
 assert target.parent==manifest.parent and target.name==d['filename']
 if target.exists():
  assert digest(target)==d['sha256'];print('EXACT_EDITABLE_SOURCE_ALREADY_PRESENT');sys.exit(0)
 temp=target.with_suffix(target.suffix+'.restoring')
 try:
  with temp.open('wb') as out:
   for part in d['parts']:
    p=manifest.parent/part['filename'];assert p.parent==manifest.parent and digest(p)==part['sha256']
    with p.open('rb') as f:
     for b in iter(lambda:f.read(1024*1024),b''):out.write(b)
  assert temp.stat().st_size==d['bytes'] and digest(temp)==d['sha256'];temp.replace(target)
 finally:
  if temp.exists():temp.unlink()
 print('EXACT_EDITABLE_BLEND_RESTORED',target);sys.exit(0)
assert len(sys.argv)==1
for source in ROOT.rglob('*.blend'):
 if source.stat().st_size<95*1024*1024:continue
 d={'filename':source.name,'bytes':source.stat().st_size,'sha256':digest(source),'parts':[]}
 with source.open('rb') as f:
  index=0
  while True:
   data=f.read(64*1024*1024)
   if not data:break
   index+=1;p=source.with_name(source.name+f'.part{index:02d}')
   if p.exists():assert p.read_bytes()==data
   else:p.write_bytes(data)
   d['parts'].append({'filename':p.name,'bytes':len(data),'sha256':digest(p)})
 # Re-read all stored blocks and prove exact reassembly before removing oversized copy.
 h=hashlib.sha256();size=0
 for part in d['parts']:
  with (source.parent/part['filename']).open('rb') as f:
   for b in iter(lambda:f.read(1024*1024),b''):h.update(b);size+=len(b)
 assert h.hexdigest()==d['sha256'] and size==d['bytes']
 manifest=source.with_name(source.name+'.parts.json');manifest.write_text(json.dumps(d,indent=2))
 source.unlink()
 print('EDITABLE_BLEND_LOSSLESS_PARTS_VERIFIED',manifest,d['sha256'])
