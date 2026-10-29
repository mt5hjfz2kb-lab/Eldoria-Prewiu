"""Retrieve only the already-paid, reviewed immutable artifacts. Never calls Tripo."""
import os,json,urllib.request,zipfile,io,hashlib,base64,uuid
from pathlib import Path
ROOT=Path('Unity/Assets/Eldoria/ArtTests/ValoriaReferenceClean/Integration/Resources/BastionIVV')
TOKEN=os.environ['GH_TOKEN']
class SafeRedirect(urllib.request.HTTPRedirectHandler):
 def redirect_request(self,req,fp,code,msg,headers,newurl):
  redirect=super().redirect_request(req,fp,code,msg,headers,newurl)
  # Blob SAS URLs authenticate via their own signature, never via GitHub's token.
  if urllib.parse.urlsplit(req.full_url).netloc!=urllib.parse.urlsplit(newurl).netloc:
   redirect.remove_header('Authorization')
  return redirect
OPENER=urllib.request.build_opener(SafeRedirect())
def fetch(url):
 return OPENER.open(urllib.request.Request(url,headers={'Authorization':'Bearer '+TOKEN,'Accept':'application/vnd.github+json','User-Agent':'Eldoria-existing-assets'}),timeout=180).read()
def source(artifact,filename,sha):
 data=fetch(f'https://api.github.com/repos/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/artifacts/{artifact}/zip')
 with zipfile.ZipFile(io.BytesIO(data)) as z:
  files=[n for n in z.namelist() if n.split('/')[-1]==filename]
  if len(files)!=1:raise RuntimeError(f'Ambiguous {artifact}/{filename}: {files}')
  content=z.read(files[0])
 if hashlib.sha256(content).hexdigest()!=sha:raise RuntimeError('Source checksum mismatch')
 return content
def write(path,data):
 path.parent.mkdir(parents=True,exist_ok=True);path.write_bytes(data)
 # Deterministic GUID before first Unity import. Unity retains it while generating importer settings.
 meta=Path(str(path)+'.meta')
 if not meta.exists():meta.write_text('fileFormatVersion: 2\nguid: '+uuid.uuid5(uuid.NAMESPACE_URL,'eldoria:iv-v:'+str(path)).hex+'\n')
for asset in json.load(open('tools/bastion-iv-v/assets.json')):
 name=asset['asset']
 for dest,artifact,filename,sha in [(ROOT/'Models'/f'{name}.glb',asset['finished_artifact'],'optimized.glb',asset['optimized_sha256']),(ROOT/'Portraits'/f'{name}.jpeg',asset['source_artifact'],'reference.jpeg',asset['reference_sha256'])]:
  content=dest.read_bytes() if dest.exists() else b''
  if hashlib.sha256(content).hexdigest()!=sha:content=source(artifact,filename,sha)
  write(dest,content)
 print('VERIFIED_EXISTING_ASSET',name,asset['optimized_sha256'])
# Existing coherent workshop, retained solely as a pre-V quarry candidate for native review.
blob=json.loads(fetch('https://api.github.com/repos/mt5hjfz2kb-lab/Eldoria-Prewiu/git/blobs/1d5a5a379f5d597cbc5932996047fd803653721d'))
write(ROOT/'Models/stoneworks-base.glb',base64.b64decode(blob['content']))
