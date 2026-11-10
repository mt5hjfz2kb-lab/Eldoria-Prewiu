"""Reconstruct the exact compiled candidate OFFLINE for transport isolation.

No Unity binary transformation: every target part must match its original hash.
This produces a local QA route only; it never edits either existing route.
"""
import argparse, gzip, hashlib, json, re, shutil, tempfile
from pathlib import Path

def digest(b): return hashlib.sha256(b).hexdigest()

def stage(root):
    root=Path(root).resolve();candidate=root/'beta-iphone-clock-ab'
    destination=root/'beta-iphone-clock-direct'
    assert not destination.exists(), 'Never overwrite an existing control'
    manifest=json.loads((candidate/'build-manifest.json').read_text())
    info=manifest['clock_copy_delta']
    assert info['stable_source_sha']=='be49d62e3daf490f41dba0bf8123a8e2eee64480'
    assert info['candidate_source_sha']=='004d3ef9c26e9070762e9e38ce84d69d525efd4f'
    targets=manifest['decoded_parts'][manifest['data']]
    assert len(targets)==len(info['target_parts'])==31
    def source_file(relative):
        p=(candidate/relative).resolve()
        assert p.is_relative_to(root) and p.is_file(), 'Unsafe or absent source'
        return p
    prepared=[]
    with tempfile.TemporaryFile() as base:
        for part in info['stable_parts']:
            sha=hashlib.sha256();size=0
            with gzip.open(source_file(part['url']),'rb') as compressed:
                while chunk:=compressed.read(1024*1024):
                    sha.update(chunk);base.write(chunk);size+=len(chunk)
            assert size==part['decoded_bytes'] and sha.hexdigest()==part['decoded_sha256']
        assert base.tell()==info['stable_size']==503830840
        # Validate metadata before creating the new route.
        for part,target in zip(info['target_parts'],targets):
            assert part['decoded_bytes']==target['decoded_bytes']
            assert part['decoded_sha256']==target['decoded_sha256']
            assert target['encoding']=='gzip'
            assert not Path(target['url']).is_absolute() and '..' not in Path(target['url']).parts
        shutil.copytree(candidate,destination)
        for part,target in zip(info['target_parts'],targets):
            literal=source_file(part['literal_url']).read_bytes()
            assert len(literal)==part['literal_bytes'] and digest(literal)==part['literal_sha256']
            output=bytearray()
            for kind,offset,size in part['ops']:
                assert offset>=0 and size>0
                if kind=='C':
                    assert offset+size<=info['stable_size']
                    base.seek(offset);fragment=base.read(size)
                else:
                    assert kind=='A' and offset+size<=len(literal)
                    fragment=literal[offset:offset+size]
                assert len(fragment)==size;output.extend(fragment)
            assert len(output)==target['decoded_bytes'] and digest(output)==target['decoded_sha256']
            path=destination/target['url'];path.parent.mkdir(parents=True,exist_ok=True)
            path.write_bytes(gzip.compress(output,compresslevel=6,mtime=0))
            prepared.append({'url':target['url'],'bytes':len(output),'sha256':digest(output)})
    del manifest['clock_copy_delta']
    (destination/'build-manifest.json').write_text(json.dumps(manifest,indent=2)+'\n')
    play=(destination/'play.js').read_text()
    call='installEldoriaCopyTransport(build.data,build.clock_copy_delta);'
    assert play.count(call)==1
    play=play.replace(call,'');filename='play-'+digest(play.encode())[:16]+'.js'
    (destination/filename).write_text(play);(destination/'play.js').write_text(play)
    html=(destination/'index.html').read_text()
    html,n=re.subn(r'<script src="\./copy-transport(?:-[a-f0-9]+)?\.js"></script>','',html)
    assert n==1
    html,n=re.subn(r'src="play-[a-f0-9]+\.js"','src="'+filename+'"',html);assert n==1
    (destination/'index.html').write_text(html)
    identity=json.loads((destination/'release-identity.json').read_text())
    identity['transport_revision']='direct-r1'
    identity['shell_script_sha256']=digest(play.encode())
    identity['physical_mobile_tested']=False
    identity['release_for_external_testers']=False
    (destination/'release-identity.json').write_text(json.dumps(identity,indent=2)+'\n')
    proof={'role':'LOCAL_IOS_TRANSPORT_CONTROL_ONLY','physical_iphone_verified':False,
           'engine_bytes_unchanged':True,'parts':prepared,'decoded_bytes':sum(p['bytes'] for p in prepared)}
    assert proof['decoded_bytes']==503831429
    (destination/'direct-control-proof.json').write_text(json.dumps(proof,indent=2))
    print('DIRECT_CONTROL_31_COMPILED_PART_HASHES_PASS',proof['decoded_bytes'],flush=True)

if __name__=='__main__':
    p=argparse.ArgumentParser();p.add_argument('site');stage(p.parse_args().site)
