"""Eldoria CC0 PBR source test, 1k maps only; zero paid assets."""
import json,urllib.request,sys,pathlib,hashlib
out=pathlib.Path(sys.argv[1]);out.mkdir(parents=True,exist_ok=True)
ASSETS=['old_planks_02']
def get(url):
 req=urllib.request.Request(url,headers={'User-Agent':'Eldoria-CC0-ArtResearch/1.0'})
 with urllib.request.urlopen(req,timeout=60) as r:return r.read()
archive={}
for a in ASSETS:
 data=json.loads(get('https://api.polyhaven.com/files/'+a))
 print('AVAILABLE_MAPS',a,list(data)[:15],flush=True)
 archive[a]={'api':'https://api.polyhaven.com/files/'+a,'maps':{}}
 for typ in ('Diffuse','diffuse','nor_gl','Normal (GL)','normal','Rough','rough'):
  t=data.get(typ)
  if not t:continue
  sizes=t.get('1k') or t.get('2k')
  if not isinstance(sizes,dict):continue
  available=[(ext,v) for ext,v in sizes.items() if ext in ('jpg','png') and isinstance(v,dict) and v.get('url')]
  if not available:continue
  ext,entry=sorted(available,key=lambda pair:0 if pair[0]=='jpg' and typ.lower()=='diffuse' else 1)[0]
  binary=get(entry['url']);p=out/(a+'_'+typ.replace(' ','_').lower()+'.'+ext)
  if len(binary)<3000:raise RuntimeError('Empty texture '+str(p))
  p.write_bytes(binary)
  archive[a]['maps'][typ]={'url':entry['url'],'sha256':hashlib.sha256(binary).hexdigest(),'bytes':len(binary),'file':p.name}
  print('FETCH_CC0',a,typ,len(binary),p.name,flush=True)
 assert any(k.lower()=='diffuse' for k in archive[a]['maps']), f'Missing diffuse for {a}'
(out/'source_manifest.json').write_text(json.dumps({'author':'Poly Haven','license':'CC0 1.0',
'cost_eur':0,'maps':archive,'intent':'reference source test only, not artistic approval'},indent=2))
