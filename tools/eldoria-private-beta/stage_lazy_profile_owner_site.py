#!/usr/bin/env python3
"""Stage an unpublished owner-only full-world route using proven small-frame COPY/ADD.
Preserve every existing Pages file; every reconstructed native data byte is verified.
"""
import argparse,gc,gzip,hashlib,json,mmap,re,shutil,tempfile
from pathlib import Path
import sys
sys.path.insert(0,str(Path(__file__).with_name('iphone_ab')))
from stage_lazy_profile_constants import *
from stage_compact_delta_ab import restore_tar
from stage_copyadd_owner_ab import verified_stream,safe_path,files,digest

def need(ok,message):
    if not ok:raise RuntimeError(message)
def filehash(p):
    with Path(p).open("rb") as f:return hashlib.file_digest(f,"sha256").hexdigest()
def tree(root):
    return {p.relative_to(root).as_posix():(p.stat().st_size,filehash(p)) for p in files(root)}
def derive(old,new):
    minimum=65536;block=1048576;cursor=scan=start=literal_bytes=0;ops=[]
    while scan+minimum<=len(new):
        at=old.find(new[scan:scan+minimum],cursor)
        if at<0:
            scan+=minimum
            need(literal_bytes+scan-start<=30000000,"New source needs too many literal bytes; do not publish")
            continue
        if scan>start:
            ops.append(("literal",start,scan-start));literal_bytes+=scan-start
        length=minimum;limit=min(len(old)-at,len(new)-scan)
        while length<limit:
            n=min(block,limit-length)
            if old[at+length:at+length+n]==new[scan+length:scan+length+n]:
                length+=n;continue
            lo,hi=0,n
            while lo<hi:
                mid=(lo+hi+1)//2
                if old[at+length:at+length+mid]==new[scan+length:scan+length+mid]:lo=mid
                else:hi=mid-1
            length+=lo;break
        ops.append(("copy",at,length));cursor=at+length;scan+=length;start=scan
        need(len(ops)<10000,"Reconstruction program is not bounded")
    if start<len(new):ops.append(("literal",start,len(new)-start))
    need(sum(n for k,p,n in ops)==len(new),"Incomplete derived reconstruction")
    print("FORWARD_ONLY_MATCH_PROGRAM",len(ops),"literal_bytes",sum(n for k,p,n in ops if k=="literal"),flush=True)
    return ops

def run(a):
    root=Path(a.output);candidate=Path(a.candidate);mac=Path(a.mac)
    identity=json.loads((candidate/"release-identity.json").read_text(encoding="utf-8-sig"))
    need(identity["source_sha"]==NEW_SOURCE and identity["revision"]==NEW_REVISION and
        identity["candidate_bundle_sha256"]==NEW_BUNDLE and
        identity["release_for_external_testers"] is False,"Wrong or externally released candidate")
    for mode in ("eager","deferred"):
        found=[p for p in mac.rglob("report.json") if p.parent.name==mode]
        need(len(found)==1,"Missing actual Apple GPU arm")
        r=json.loads(found[0].read_text(encoding="utf-8-sig"))
        need(r["passed"] is True and not r["errors"] and r["entry"]["reloaded"] is True and
            r["identity"]["source_sha"]==NEW_SOURCE and
            r["identity"]["candidate_bundle_sha256"]==NEW_BUNDLE,"Actual Mac arm failed")
        need(r.get("eagerWorldInstalledVerified") if mode=="eager" else
            r.get("lazyWorldDeferredToFirstEntryVerified"),"Wrong real world-start mode")
    restore_tar(a.pages,root)
    base=root/BASE_ROUTE
    oldid=json.loads((base/"release-identity.json").read_text(encoding="utf-8-sig"))
    need(oldid["source_sha"]==BASE_SOURCE and oldid["revision"]==BASE_REVISION,"Owner-playable city baseline changed")
    before=tree(root);dest=root/NEW_ROUTE;need(not dest.exists(),"Never replace an existing published route")
    oldman=json.loads((base/"build-manifest.json").read_text(encoding="utf-8-sig"))
    newman=json.loads((candidate/"build-manifest.json").read_text(encoding="utf-8-sig"))
    oldparts=oldman["decoded_parts"][oldman["data"]];newparts=newman["decoded_parts"][newman["data"]]
    need(len(oldparts)==len(newparts)==31,"Expected genuine31-part data for both native builds")
    dest.mkdir();exclude={safe_path(x["url"]).as_posix() for x in newparts}
    for source in files(candidate):
        rel=source.relative_to(candidate)
        if rel.as_posix() in exclude:continue
        target=dest/rel;target.parent.mkdir(parents=True,exist_ok=True);shutil.copyfile(source,target)
    (dest/"copy-literals").mkdir()
    shared_streaming=False;shared_city=False
    if (dest/"StreamingAssets").is_dir() and (base/"StreamingAssets").is_dir():
        if tree(dest/"StreamingAssets")==tree(base/"StreamingAssets"):
            shared_streaming=True
    if (dest/"city.png").is_file() and (base/"city.png").is_file():
        shared_city=filehash(dest/"city.png")==filehash(base/"city.png")
    print("CANDIDATE_STATIC_ASSET_REUSE",json.dumps(dict(streaming=shared_streaming,city=shared_city)),flush=True)

    with tempfile.TemporaryDirectory(prefix="eldoria-lazy-stream-") as tmp:
        tmp=Path(tmp)
        oldsize=verified_stream(base,oldparts,tmp/"old.bin")
        newsize=verified_stream(candidate,newparts,tmp/"new.bin")
        need(newsize==NEW_SIZE,"Fresh native data size changed")
        with (tmp/"old.bin").open("rb") as f,(tmp/"new.bin").open("rb") as g:
            with mmap.mmap(f.fileno(),0,access=mmap.ACCESS_READ) as old,mmap.mmap(g.fileno(),0,access=mmap.ACCESS_READ) as new:
                operations=derive(old,new);parts=[];position=op_index=offset=0
                for index,metadata in enumerate(newparts):
                    end=position+metadata["decoded_bytes"];literal=bytearray();ops=[];sha=hashlib.sha256()
                    while position<end:
                        kind,at,n=operations[op_index];take=min(n-offset,end-position)
                        need(take>0,"Invalid operation span")
                        if kind=="copy":
                            fragment=old[at+offset:at+offset+take];ops.append(["C",at+offset,take])
                        else:
                            need(at+offset==position,"Literal is not from genuine target")
                            fragment=new[position:position+take];ops.append(["A",len(literal),take]);literal.extend(fragment)
                        need(fragment==new[position:position+take],"Source reconstruction differs from native build")
                        sha.update(fragment);position+=take;offset+=take
                        if offset==n:op_index+=1;offset=0
                    need(sha.hexdigest()==metadata["decoded_sha256"],"Native target part SHA mismatch")
                    name="copy-literals/part-%02d.bin.gz"%index;(dest/name).write_bytes(gzip.compress(literal,compresslevel=9,mtime=0))
                    parts.append(dict(index=index,decoded_bytes=metadata["decoded_bytes"],
                        decoded_sha256=metadata["decoded_sha256"],literal_url=name,
                        literal_bytes=len(literal),literal_sha256=digest(literal),ops=ops))
                need(position==newsize and op_index==len(operations) and offset==0,"Truncated native data program")
    newman["clock_copy_delta"]=dict(format="ELDORIA_COPY_ADD_V1",stable_revision=BASE_REVISION,
        stable_source_sha=BASE_SOURCE,candidate_source_sha=NEW_SOURCE,stable_part_size=16777216,
        stable_size=oldsize,candidate_size=newsize,
        stable_parts=[dict(url="../"+BASE_ROUTE+"/"+x["url"],decoded_bytes=x["decoded_bytes"],
            decoded_sha256=x["decoded_sha256"]) for x in oldparts],target_parts=parts)
    (dest/"build-manifest.json").write_text(json.dumps(newman,indent=2)+"\n")
    transport=Path(a.transport).read_text()
    for oldvalue,newvalue in [
        ("be49d62e3daf490f41dba0bf8123a8e2eee64480",BASE_SOURCE),
        ("004d3ef9c26e9070762e9e38ce84d69d525efd4f",NEW_SOURCE),
        ("503830840",str(oldsize)),("503831429",str(newsize)),
        ("/beta-native325/payload-decoded/","/"+BASE_ROUTE+"/payload-decoded/"),
        ("/beta-iphone-clock-ab/copy-literals/","/"+NEW_ROUTE+"/copy-literals/"),
        ("p.literal_bytes>6000000","p.literal_bytes>16777216")]:
        need(oldvalue in transport,"Verified transport template changed: "+oldvalue)
        transport=transport.replace(oldvalue,newvalue)
    name="copy-transport-"+digest(transport.encode())[:20]+".js"
    (dest/name).write_text(transport)
    html=(dest/"index.html").read_text();match=re.search(r'<script src="(play-[^"/]+\.js)"></script>',html)
    need(match is not None,"Real bootstrap script reference changed")
    oldscript=match.group(1);play=(dest/"play.js").read_text()
    need(play==(dest/oldscript).read_text(),"Versioned/native bootstrap mismatch")
    call="installExactDecodedTransport(build.decoded_sizes,build.decoded_parts);"
    need(play.count(call)==1 and "installEldoriaCopyTransport" not in play,"Unknown real transport hook")
    play=play.replace(call,call+"installEldoriaCopyTransport(build.data,build.clock_copy_delta);")
    if shared_streaming:
        play,n=re.subn(r'(["\'])StreamingAssets\1',
            lambda m:m.group(1)+"../"+BASE_ROUTE+"/StreamingAssets"+m.group(1),play)
        need(n==1,"Refuse unknown streaming-assets bootstrap")
        shutil.rmtree(dest/"StreamingAssets")
    if shared_city:
        html=html.replace("city.png","../"+BASE_ROUTE+"/city.png")
        for css in dest.rglob("*.css"):
            css.write_text(css.read_text().replace("city.png","../"+BASE_ROUTE+"/city.png"))
        play=play.replace("city.png","../"+BASE_ROUTE+"/city.png")
        (dest/"city.png").unlink()

    newscript="play-"+digest(play.encode())[:16]+".js"
    (dest/"play.js").write_text(play);(dest/newscript).write_text(play)
    (dest/oldscript).unlink()
    html=html.replace('<script src="'+oldscript+'"></script>',
        '<script src="./'+name+'"></script>\n<script src="'+newscript+'"></script>',1)
    (dest/"index.html").write_text(html)
    identity["owner_delivery"]={"format":"ELDORIA_COPY_ADD_V1","native_input_provenance_retained":True,
        "all_native_data_bytes_verified":True,"published":False,"physical_iphone_verified":False}
    (dest/"release-identity.json").write_text(json.dumps(identity,indent=2))
    for path,(size,sha) in before.items():
        need((root/path).stat().st_size==size and filehash(root/path)==sha,"An existing owner-playable file changed: "+path)
    size=sum(p.stat().st_size for p in files(root))
    summary=dict(role="UNPUBLISHED_OWNER_QA_ONLY",native_run=NATIVE_RUN,mac_run=MAC_RUN,
        base_pages_run=BASE_PAGES_RUN,source_sha=NEW_SOURCE,revision=NEW_REVISION,
        native_bundle_sha256=NEW_BUNDLE,data_verified_bytes=newsize,base_data_bytes=oldsize,
        target_parts=31,operations=len(operations),literal_bytes=sum(p["literal_bytes"] for p in parts),
        compressed_literal_bytes=sum((dest/p["literal_url"]).stat().st_size for p in parts),
        shared_exact_streaming_assets=shared_streaming,shared_exact_city_background=shared_city,
        original_files_preserved=len(before),combined_bytes=size,
        published=False,art_approved=False,world_entry_verified=False,
        physical_iphone_verified=False,external_testers_enabled=False,
        owner_route=NEW_ROUTE+"/?eldoria_p0_lazy_world=1&eldoria_diag=1",transport_file=name)
    (root/"beta-lazy-world-owner-proof.json").write_text(json.dumps(summary,indent=2))
    print("EXACT_PRESERVED_OWNER_LAZY_SITE_STAGED",json.dumps(summary),flush=True)
    print("STAGED_LARGEST_NEW_FILES",json.dumps(sorted([(p.relative_to(dest).as_posix(),p.stat().st_size) for p in files(dest)],key=lambda x:-x[1])[:20]),flush=True)
    need(size<980000000,"Preserved owner site exceeds the unchanged Pages staging budget; do not publish")
if __name__=="__main__":
    p=argparse.ArgumentParser()
    for key in ("pages","candidate","mac","output","transport"):p.add_argument("--"+key,required=True)
    run(p.parse_args())
