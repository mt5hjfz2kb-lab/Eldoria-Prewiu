"""Prepare an isolated static PC copy from the exact verified delivery package."""
import gzip,hashlib,json,pathlib,shutil,sys,zipfile
def digest(p):
    with p.open('rb') as f:return hashlib.file_digest(f,'sha256').hexdigest()
def prepare(package, destination):
    package=pathlib.Path(package);destination=pathlib.Path(destination)
    report=json.loads((package/'identity.json').read_text())
    archive=package/('Eldoria-'+report['candidate']['revision']+'-itch.zip')
    if digest(archive)!=report['zip_sha256']:raise ValueError('Delivery archive hash mismatch')
    if destination.exists():raise ValueError('Version destination already exists')
    destination.mkdir(parents=True)
    identities={x['file']:x for x in report['files']}
    with zipfile.ZipFile(archive) as z:
        if set(z.namelist())!=set(identities):raise ValueError('Unexpected ZIP contents')
        for item in z.infolist():
            rel=pathlib.PurePosixPath(item.filename)
            if rel.is_absolute() or '..' in rel.parts or '\\' in item.filename:raise ValueError('Unsafe path')
            path=destination/str(rel);path.parent.mkdir(parents=True,exist_ok=True)
            with z.open(item) as src,path.open('wb') as dst:shutil.copyfileobj(src,dst)
            if digest(path)!=identities[item.filename]['sha256']:raise ValueError('File hash mismatch')
    manifest=json.loads((destination/'build-manifest.json').read_text())
    decoded=[]
    for key in ('data','framework','code'):
        name=manifest[key].split('?',1)[0];path=destination/name
        with path.open('rb') as f:magic=f.read(2)
        if magic!=b'\x1f\x8b':continue
        temp=path.with_suffix(path.suffix+'.decoded')
        with gzip.open(path,'rb') as src,temp.open('wb') as dst:shutil.copyfileobj(src,dst)
        expected=manifest['decoded_sizes'][manifest[key]]
        if temp.stat().st_size!=expected:raise ValueError('Decoded size mismatch')
        temp.replace(path)
        decoded.append({'file':name,'bytes':expected,'sha256':digest(path)})
    manifest['delivery_adapter']='pc-static-decoded-v1'
    manifest['decoded_sizes']={}
    (destination/'build-manifest.json').write_text(json.dumps(manifest,indent=2)+'\n')
    (destination/'pc-delivery.json').write_text(json.dumps({'revision':manifest['revision'],'package_sha256':report['zip_sha256'],'decoded':decoded},indent=2)+'\n')
    print(json.dumps({'status':'PC_FILES_VERIFIED_NOT_NETWORK_VERIFIED','revision':manifest['revision'],'decoded':len(decoded)}))
if __name__=='__main__':prepare(sys.argv[1],sys.argv[2])
