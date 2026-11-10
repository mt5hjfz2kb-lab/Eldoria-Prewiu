#!/usr/bin/env python3
"""Stage exact 490MB owner-only iPhone A/B with no new storage provider.

Uses 158 pinned, independently verified copy/add operations over the existing
owner-playable beta10 Unity .data, plus eight megabytes of literal differences.
Preserves the whole old Pages artifact and game byte-for-byte. No saves touched.
New candidate is real Unity/WebGL compiled and desktop Apple GPU verified, NOT
yet physically iPhone-certified. External tester distribution remains blocked.
"""
import argparse, gzip, hashlib, json, mmap, re, shutil, tarfile, tempfile
from pathlib import Path, PurePosixPath

OLD_REV='native325-mobile10-pretest-ux'
OLD_SHA='be49d62e3daf490f41dba0bf8123a8e2eee64480'
NEW_REV='native325-mobile12-minimal-world-clock-qa'
NEW_SHA='004d3ef9c26e9070762e9e38ce84d69d525efd4f'
PART_SIZE=16777216

def need(value,message):
    if not value: raise RuntimeError(message)
def digest(buf):
    return hashlib.sha256(buf).hexdigest()
def safe_path(value):
    p=PurePosixPath(value.split('?',1)[0])
    need(p.parts and not p.is_absolute() and '..' not in p.parts and '\\' not in value,
         'Unsafe Unity asset path')
    return Path(*p.parts)
def files(root):
    return (p for p in root.rglob('*') if p.is_file())

def verified_stream(root,parts,path):
    total=0
    with path.open('wb') as out:
        for index,item in enumerate(parts):
            need(item['encoding']=='gzip', 'Unexpected Unity encoding')
            source=root/safe_path(item['url'])
            need(source.is_file(),'Unity chunk not present '+str(index))
            size=0;sha=hashlib.sha256()
            with gzip.open(source,'rb') as compressed:
                while True:
                    data=compressed.read(1024*1024)
                    if not data:break
                    sha.update(data);out.write(data);size+=len(data)
            need(size==item['decoded_bytes'] and sha.hexdigest()==item['decoded_sha256'],
                 'Untrusted or modified Unity data '+str(index))
            total+=size
    return total

def stage(old_archive,new_source,out,ops_path,js_file):
    old_archive,new_source,out,ops_path,js_file=map(Path,
        (old_archive,new_source,out,ops_path,js_file))
    need(not out.exists(),'Never replace a previously staged site')
    need(old_archive.is_file() and new_source.is_dir() and
         ops_path.is_file() and js_file.is_file(),'Exact input unavailable')
    out.mkdir(parents=True)
    with tarfile.open(old_archive,'r:*') as archive:
        for item in archive:
            name=PurePosixPath(item.name)
            need(not name.is_absolute() and '..' not in name.parts and
                 '\\' not in item.name, 'Unsafe original Pages archive')
            if item.isdir():continue
            need(item.isfile(),'Unexpected non-regular archive member')
            if name.as_posix()=='.':continue
            dest=out.joinpath(*name.parts)
            dest.parent.mkdir(parents=True,exist_ok=True)
            with archive.extractfile(item) as f,dest.open('wb') as w:
                shutil.copyfileobj(f,w,1024*1024)
    stable=out/'beta-native325'
    need((stable/'index.html').is_file() and
         (out/'unity-owner/index.html').is_file(),'Original playable Pages route absent')
    oldid=json.loads((stable/'release-identity.json').read_text(encoding='utf-8-sig'))
    newid=json.loads((new_source/'release-identity.json').read_text(encoding='utf-8-sig'))
    need(oldid['revision']==OLD_REV and oldid['source_sha']==OLD_SHA,
         'Not the exact owner-playable beta10')
    need(newid['revision']==NEW_REV and newid['source_sha']==NEW_SHA and
         newid['native_journey_verified'] is True and
         newid['physical_mobile_tested'] is False and
         newid['release_for_external_testers'] is False,
         'Unapproved Unity clock candidate')
    before=json.loads((stable/'build-manifest.json').read_text(encoding='utf-8-sig'))
    after=json.loads((new_source/'build-manifest.json').read_text(encoding='utf-8-sig'))
    a=before['decoded_parts'][before['data']]
    b=after['decoded_parts'][after['data']]
    need(len(a)==len(b)==31,'Missing streamed Unity chunks')
    stable_index_hash=digest((stable/'index.html').read_bytes())
    stable_manifest_hash=digest((stable/'build-manifest.json').read_bytes())
    candidate=out/'beta-iphone-clock-ab';candidate.mkdir()
    exclude={safe_path(x['url']).as_posix() for x in b}
    for source in files(new_source):
        rel=source.relative_to(new_source)
        if rel.as_posix() in exclude: continue
        target=candidate/rel;target.parent.mkdir(parents=True,exist_ok=True)
        shutil.copyfile(source,target)
    operations=json.loads(ops_path.read_text())
    need(len(operations)==158 and
         sum(n for _,_,n in operations)==after['decoded_sizes'][after['data']] and
         sum(n for k,_,n in operations if k=='literal')==8000096,
         'Pinned, exact copy/add program not present')
    patches=candidate/'copy-literals';patches.mkdir()
    with tempfile.TemporaryDirectory(prefix='eldoria-clock-copyadd-') as tmp:
        tmp=Path(tmp)
        old_size=verified_stream(stable,a,tmp/'old.bin')
        new_size=verified_stream(new_source,b,tmp/'new.bin')
        need(old_size==503830840 and new_size==503831429,
             'Unexpected old/new 503MB Unity payload')
        with (tmp/'old.bin').open('rb') as src,(tmp/'new.bin').open('rb') as dst:
            with mmap.mmap(src.fileno(),0,access=mmap.ACCESS_READ) as old, \
                 mmap.mmap(dst.fileno(),0,access=mmap.ACCESS_READ) as new:
                pos=op_idx=offset=0
                parts=[]
                for index,metadata in enumerate(b):
                    need(pos==index*PART_SIZE,'Target stream position changed')
                    end=pos+metadata['decoded_bytes']
                    literal=bytearray();ops=[];sha=hashlib.sha256()
                    while pos<end:
                        kind,at,n=operations[op_idx]
                        take=min(n-offset,end-pos)
                        need(take>0,'Invalid operation extent')
                        if kind=='copy':
                            start=at+offset
                            need(start+take<=len(old),'Copy beyond owner playable source')
                            fragment=old[start:start+take]
                            ops.append(['C',start,take])
                        elif kind=='literal':
                            need(at+offset==pos,'Literal offset not from certified target')
                            start=len(literal)
                            fragment=new[pos:pos+take]
                            literal.extend(fragment)
                            ops.append(['A',start,take])
                        else:raise RuntimeError('Untrusted operation type')
                        need(fragment==new[pos:pos+take],
                             'Pinned reconstruction does not match compiled Unity')
                        sha.update(fragment)
                        pos+=take;offset+=take
                        if offset==n:op_idx+=1;offset=0
                    need(sha.hexdigest()==metadata['decoded_sha256'],
                         'New Unity target SHA256 failed at part '+str(index))
                    name='part-%02d.bin'%index
                    (patches/name).write_bytes(literal)
                    parts.append({'index':index,'decoded_bytes':metadata['decoded_bytes'],
                       'decoded_sha256':metadata['decoded_sha256'],
                       'literal_url':'copy-literals/'+name,
                       'literal_bytes':len(literal),
                       'literal_sha256':digest(literal),'ops':ops})
                need(pos==new_size and op_idx==len(operations) and offset==0,
                     'Target stream incomplete')
    after['clock_copy_delta']={
        'format':'ELDORIA_COPY_ADD_V1',
        'stable_revision':OLD_REV,'stable_source_sha':OLD_SHA,
        'candidate_source_sha':NEW_SHA,'stable_part_size':PART_SIZE,
        'stable_size':old_size,'candidate_size':new_size,
        'stable_parts':[{'url':'../beta-native325/'+x['url'],
                         'decoded_bytes':x['decoded_bytes'],
                         'decoded_sha256':x['decoded_sha256']} for x in a],
        'target_parts':parts}
    (candidate/'build-manifest.json').write_text(json.dumps(after,indent=2)+'\n')
    html=(candidate/'index.html').read_text()
    script=re.search(r'<script src="(play-[^"/]+\.js)"></script>',html)
    need(script is not None,'Unknown Unity browser entry; fail closed')
    versioned=script.group(1)
    play=(candidate/'play.js').read_text()
    need(play==(candidate/versioned).read_text(),
         'Actual versioned loader differs from inspected play.js')
    old_call='installExactDecodedTransport(build.decoded_sizes,build.decoded_parts);'
    need(play.count(old_call)==1 and
         'installEldoriaCopyTransport' not in play,'Unity loader changed')
    play=play.replace(old_call,old_call+
        'installEldoriaCopyTransport(build.data,build.clock_copy_delta);')
    (candidate/'play.js').write_text(play)
    (candidate/versioned).write_text(play)
    transport_bytes=js_file.read_bytes()
    transport_name='copy-transport-'+digest(transport_bytes)[:20]+'.js'
    (candidate/transport_name).write_bytes(transport_bytes)
    html=html.replace('<script src="'+versioned+'"></script>',
        '<script src="./'+transport_name+'"></script>\n'+
        '<script src="'+versioned+'"></script>',1)
    (candidate/'index.html').write_text(html)
    shutil.copyfile(js_file,candidate/'copy-transport.js')
    need(digest((stable/'index.html').read_bytes())==stable_index_hash and
         digest((stable/'build-manifest.json').read_bytes())==stable_manifest_hash,
         'Owner-playable beta10 altered')
    bytes_out=sum(f.stat().st_size for f in files(out))
    need(bytes_out<650000000,
         'Fail-closed: Pages size may exceed prior successful deployment')
    proof={'role':'OWNER_AB_IOS_QA_ONLY',
       'external_testers_enabled':False,'physical_iphone_stability_verified':False,
       'stable_source_sha':OLD_SHA,'candidate_source_sha':NEW_SHA,
       'stable_revision':OLD_REV,'candidate_revision':NEW_REV,
       'data_verified_bytes':new_size,'parts_verified':31,
       'patch_literal_bytes':sum(x['literal_bytes'] for x in parts),
       'combined_bytes':bytes_out,'stable_game_sha256_unchanged':True,
       'transport_file':transport_name, 'transport_sha256':digest(transport_bytes),
       'method':'Forward-only 158-operation copy/add; no full-block reconstruction'}
    (out/'beta-iphone-clock-ab-proof.json').write_text(json.dumps(proof,indent=2)+'\n')
    print('ELDORIA_OWNER_IOS_COPYADD_31_EXACT_PARTS_PASS',
          json.dumps(proof,sort_keys=True),flush=True)
    return proof

if __name__=='__main__':
    parser=argparse.ArgumentParser()
    parser.add_argument('--stable-pages-artifact',type=Path,required=True)
    parser.add_argument('--candidate-site',type=Path,required=True)
    parser.add_argument('--output-site',type=Path,required=True)
    parser.add_argument('--ops',type=Path,required=True)
    parser.add_argument('--transport-source',type=Path,required=True)
    args=parser.parse_args()
    stage(args.stable_pages_artifact,args.candidate_site,args.output_site,
          args.ops,args.transport_source)
