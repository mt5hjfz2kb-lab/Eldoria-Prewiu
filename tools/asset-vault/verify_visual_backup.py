#!/usr/bin/env python3
"""Verify immutable asset backup from a manifest. Read only, no network."""
import argparse,hashlib,json,pathlib,sys
def verify(root,manifest):
    errors=[]
    for asset in manifest.get("assets",[]):
        rel=asset.get("path","")
        if not rel or pathlib.PurePosixPath(rel).is_absolute() or ".." in pathlib.PurePosixPath(rel).parts:
            errors.append("unsafe path: "+rel);continue
        file=(root/rel).resolve()
        if not file.is_relative_to(root.resolve()) or not file.is_file():
            errors.append("missing: "+rel);continue
        digest=hashlib.file_digest(file.open("rb"),"sha256").hexdigest()
        if digest!=asset.get("sha256"):errors.append("hash mismatch: "+rel)
    return errors
def main():
    p=argparse.ArgumentParser();p.add_argument("--root",required=True);p.add_argument("--manifest",required=True);a=p.parse_args()
    errors=verify(pathlib.Path(a.root),json.loads(pathlib.Path(a.manifest).read_text()))
    print(json.dumps({"verified":not errors,"errors":errors},indent=2))
    sys.exit(bool(errors))
if __name__=="__main__":main()
