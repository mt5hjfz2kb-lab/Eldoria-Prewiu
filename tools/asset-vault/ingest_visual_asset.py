#!/usr/bin/env python3
"""Append-only local vault ingestion. Copy files, verify hashes; never move/delete sources."""
import argparse,hashlib,json,os,pathlib,shutil,sys,uuid
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
 if not destination.resolve().is_relative_to(vault):raise ValueError("unsafe vault object path")
 if destination.exists():
  if not destination.is_file() or sha(destination)!=digest:raise ValueError("destination conflict")
 else:
  destination.parent.mkdir(parents=True,exist_ok=True)
  temporary=destination.with_name(destination.name+"."+uuid.uuid4().hex+".part")
  try:
   shutil.copyfile(source,temporary)
   if sha(temporary)!=digest:raise ValueError("copy verification failed")
   if destination.exists():
    if sha(destination)!=digest:raise ValueError("destination conflict")
   else:
    try:os.link(temporary,destination)
    except FileExistsError:
     if not destination.is_file() or sha(destination)!=digest:raise ValueError("destination conflict")
  finally:temporary.unlink(missing_ok=True)
 record={"asset_id":asset_id,"kind":kind,"sha256":digest,"bytes":source.stat().st_size,"original_name":source.name,"vault_path":str(destination.relative_to(vault)),"source_path":str(source),"status":"UNREVIEWED"}
 catalog=vault/"records"/asset_id
 if not catalog.resolve().is_relative_to(vault):raise ValueError("unsafe catalog path")
 catalog.mkdir(parents=True,exist_ok=True)
 path=catalog/(digest+".json")
 if path.exists():
  old=json.loads(path.read_text())
  if any(old.get(k)!=record[k] for k in ("sha256","asset_id","kind","vault_path")):raise ValueError("record conflict")
  return old
 else:
  try:
   with path.open("x",encoding="utf-8") as stream:stream.write(json.dumps(record,indent=2)+"\n")
  except FileExistsError:
   old=json.loads(path.read_text())
   if any(old.get(k)!=record[k] for k in ("sha256","asset_id","kind","vault_path")):raise ValueError("record conflict")
   return old
 return record
def main():
 p=argparse.ArgumentParser();p.add_argument("--source",required=True);p.add_argument("--vault",required=True);p.add_argument("--asset-id",required=True);p.add_argument("--kind",required=True);a=p.parse_args()
 print(json.dumps(ingest(a.source,a.vault,a.asset_id,a.kind),indent=2))
if __name__=="__main__":main()
