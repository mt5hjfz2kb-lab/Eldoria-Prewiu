#!/usr/bin/env python3
"""Append-only local vault ingestion. Copy files, verify hashes; never move/delete sources."""
import argparse,hashlib,json,pathlib,shutil,sys,uuid
def sha(p):
 h=hashlib.sha256()
 with p.open("rb") as f:
  for block in iter(lambda:f.read(1024*1024),b""):h.update(block)
 return h.hexdigest()
def ingest(source,vault,asset_id,kind):
 source=pathlib.Path(source).resolve();vault=pathlib.Path(vault).resolve()
 if not source.is_file():raise ValueError("source missing")
 if vault==source or vault in source.parents:raise ValueError("vault cannot be a source")
 if not asset_id or any(c not in "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-_" for c in asset_id):raise ValueError("unsafe asset ID")
 digest=sha(source);destination=vault/"objects"/digest[:2]/digest/source.name
 if destination.exists():
  if sha(destination)!=digest:raise ValueError("destination conflict")
 else:
  destination.parent.mkdir(parents=True,exist_ok=True)
  temporary=destination.with_name(destination.name+"."+uuid.uuid4().hex+".part")
  try:
   shutil.copyfile(source,temporary)
   if sha(temporary)!=digest:raise ValueError("copy verification failed")
   if destination.exists():
    if sha(destination)!=digest:raise ValueError("destination conflict")
   else:temporary.replace(destination)
  finally:temporary.unlink(missing_ok=True)
 record={"asset_id":asset_id,"kind":kind,"sha256":digest,"bytes":source.stat().st_size,"original_name":source.name,"vault_path":str(destination.relative_to(vault)),"source_path":str(source),"status":"UNREVIEWED"}
 catalog=vault/"records"/asset_id
 catalog.mkdir(parents=True,exist_ok=True)
 path=catalog/(digest+".json")
 if path.exists():
  old=json.loads(path.read_text())
  if old["sha256"]!=digest:raise ValueError("record conflict")
 else:path.write_text(json.dumps(record,indent=2)+"\n")
 return record
def main():
 p=argparse.ArgumentParser();p.add_argument("--source",required=True);p.add_argument("--vault",required=True);p.add_argument("--asset-id",required=True);p.add_argument("--kind",required=True);a=p.parse_args()
 print(json.dumps(ingest(a.source,a.vault,a.asset_id,a.kind),indent=2))
if __name__=="__main__":main()
