"""Repackage verified mobile5 with the five-cubemap compatibility change only."""
import gc,gzip,hashlib,json,shutil,struct,sys
from pathlib import Path
from decode_reflection_cubemaps import run as decode
def sha(b):return hashlib.sha256(b).hexdigest()
def main(source,work,output):
    source=Path(source);work=Path(work);out=Path(output);work.mkdir(parents=True,exist_ok=True)
    m=json.loads((source/'build-manifest.json').read_text());assert m['revision']=='native325-mobile5'
    raw=bytearray()
    for p in m['decoded_parts'][m['data']]:
        b=gzip.decompress((source/p['url']).read_bytes());assert len(b)==p['decoded_bytes'] and sha(b)==p['decoded_sha256'];raw.extend(b)
    assert len(raw)==500847489 and raw[:16]==b'UnityWebData1.0\0'
    end=struct.unpack_from('<I',raw,16)[0];pos=20;entries=[]
    while pos<end:
        off,size,n=struct.unpack_from('<III',raw,pos);pos+=12;name=bytes(raw[pos:pos+n]).decode();pos+=n
        assert off>=end and off+size<=len(raw);entries.append((name,off,size))
    assert pos==end and len([e for e in entries if e[0]=='data.unity3d'])==1
    _,off,size=next(e for e in entries if e[0]=='data.unity3d');(work/'parent.unity3d').write_bytes(raw[off:off+size])
    (work/'data.bin').write_bytes(raw);del raw;gc.collect()
    decode(work/'parent.unity3d',work/'candidate');gc.collect()
    proof=json.loads((work/'candidate/integrity.json').read_text())
    assert proof['candidate_sha256']=='e54d547487e0d915f5a8c3fbbd0354a79672041dd201b3e63672de5e740ea6c1'
    raw=(work/'data.bin').read_bytes();bundle=(work/'candidate/data.unity3d').read_bytes()
    files=[(name,bundle if name=='data.unity3d' else raw[off:off+size]) for name,off,size in entries]
    header=20+sum(12+len(n.encode()) for n,_ in files);buf=bytearray(b'UnityWebData1.0\0'+struct.pack('<I',header));offset=header
    for name,data in files:
        nb=name.encode();buf.extend(struct.pack('<III',offset,len(data),len(nb))+nb);offset+=len(data)
    for _,data in files:buf.extend(data)
    shutil.copytree(source,out,dirs_exist_ok=False);shutil.rmtree(out/'payload-decoded');(out/'payload-decoded').mkdir()
    revision='native325-mobile6-cubemap';parts=[]
    for i,start in enumerate(range(0,len(buf),16*1024*1024)):
        b=bytes(buf[start:start+16*1024*1024]);name=f'payload-decoded/{revision}-{i}.gz';(out/name).write_bytes(gzip.compress(b,compresslevel=6,mtime=0));parts.append(dict(url=name,encoding='gzip',decoded_bytes=len(b),decoded_sha256=sha(b)))
    old=m['data'];m['data']='Build/'+revision+'.data';m['revision']=revision;m['decoded_sizes'].pop(old);m['decoded_sizes'][m['data']]=len(buf);m['decoded_parts'].pop(old);m['decoded_parts'][m['data']]=parts
    (out/'build-manifest.json').write_text(json.dumps(m,indent=2))
    identity=json.loads((out/'release-identity.json').read_text());identity['decoded_files']=[f for f in identity['decoded_files'] if not f.get('url','').startswith('payload-decoded/')]+parts
    identity.update(revision=revision,candidate_bundle_sha256=proof['candidate_sha256'],cubemap_compatibility=proof,decoded_parts=parts,runtime_verified=False,physical_mobile_tested=False,release_for_external_testers=False,physical_mobile_result='UNVERIFIED_MOBILE6',runtime_qa_run=None,graceful_reopen_verified=False,peak_wasm_capacity_bytes=None)
    (out/'release-identity.json').write_text(json.dumps(identity,indent=2))
    summary=dict(revision=revision,data_bytes=len(buf),site_bytes=sum(p.stat().st_size for p in out.rglob('*') if p.is_file()),bundle_sha256=proof['candidate_sha256'])
    (work/'package-summary.json').write_text(json.dumps(summary,indent=2));print(json.dumps(summary),flush=True)
if __name__=='__main__':main(*sys.argv[1:])
