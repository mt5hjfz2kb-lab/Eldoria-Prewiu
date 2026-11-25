"""Immutable zero-credit request for the reviewed native MILL_YARD adapter.
Windows is not acquired until this hosted gate validates the live registry.
"""
import argparse,base64,json,os,pathlib,re,sys,unittest
import request_gate as rg

SOURCE="5ef2aee67cf8fde57ba2fc4b3d929b517ee67944"
CANDIDATE="4e582b25a420383798e3aa4895d1b71b33158f18de0087165b8055adef068785"
IDENT="eldoria-openai-development-studio-20261125"
def validate(d,reg):
    rg.require(d.get("schema")==1 and d.get("approved_native_execution") is True,"Explicit native authorization required")
    rg.require(isinstance(d.get("id"),str) and rg.IDENT.fullmatch(d["id"]),"Invalid native request ID")
    rg.require(d.get("source_sha")==SOURCE and d.get("candidate_sha256")==CANDIDATE,"Only frozen reviewed candidate supported")
    rg.require(d.get("workstream_id")==IDENT,"Wrong workstream")
    rg.require(isinstance(d.get("owner"),str) and re.fullmatch(r"[a-z0-9-]{3,100}",d["owner"]),"Invalid owner")
    rg.require(type(d.get("revision")) is int and d["revision"]>0,"Invalid revision")
    held=[x for x in reg["active"] if "windows-runner-heavy" in x.get("resources",[])]
    rg.require(len(held)==1 and held[0]["id"]==IDENT and held[0]["owner"]==d["owner"] and
        held[0]["ownership_revision"]==d["revision"] and held[0]["ownership_state"]=="claimed" and
        reg.get("resource_rotation",{}).get("current_workstream")==IDENT,"Native lease not exclusively ours")
def gate(event,attempt,head,ref):
    rg.require(ref==rg.BRANCH and attempt==1,"Only first attempt on Studio branch")
    before=event.get("before","")
    rg.require(rg.SHA.fullmatch(before) and rg.SHA.fullmatch(head) and event.get("after")==head and
        event.get("forced") is False and event.get("deleted") is False,"Invalid native push")
    rg.require(rg.git("rev-parse","HEAD")==head and rg.git("rev-parse","HEAD^")==before,"Expected direct child commit")
    diff=rg.git("diff","--name-status",before,head).splitlines()
    rg.require(len(diff)==1,"Exactly one new native request file required")
    parts=diff[0].split("\t")
    rg.require(len(parts)==2 and parts[0]=="A" and re.fullmatch(re.escape(rg.PREFIX)+"native-requests/[a-z0-9][a-z0-9-]{2,79}\\.json",parts[1]),"Request path invalid")
    path=parts[1];raw=pathlib.Path(path).read_bytes()
    rg.require(0<len(raw)<=4000,"Oversized request")
    d=json.loads(raw)
    rg.require(path==rg.PREFIX+"native-requests/"+d["id"]+".json","Native request ID mismatch")
    from urllib.parse import quote
    history=rg.gh("repos/"+rg.REPO+"/commits?sha="+before+"&path="+quote(path,safe="")+"&per_page=1")
    rg.require(isinstance(history,list) and not history,"Native request path has historical use")
    meta=rg.gh("repos/"+rg.REPO+"/contents/pipeline/active-workstreams.json?ref=main")
    blob=rg.gh("repos/"+rg.REPO+"/git/blobs/"+meta["sha"])
    rg.require(blob["sha"]==meta["sha"] and blob["encoding"]=="base64","Registry blob mismatch")
    reg=json.loads(base64.b64decode(blob["content"]))
    validate(d,reg)
    return raw
class Tests(unittest.TestCase):
    def setUp(self):
        self.d={"schema":1,"id":"native-test","approved_native_execution":True,"source_sha":SOURCE,
            "candidate_sha256":CANDIDATE,"workstream_id":IDENT,"owner":"chat-studio","revision":1}
        self.r={"active":[{"id":IDENT,"owner":"chat-studio","ownership_revision":1,"ownership_state":"claimed","resources":["windows-runner-heavy"]}],"resource_rotation":{"current_workstream":IDENT}}
    def test_pass(self):validate(self.d,self.r)
    def test_other_owner(self):
        self.r["active"][0]["owner"]="other"
        with self.assertRaises(ValueError):validate(self.d,self.r)
    def test_no_authorization(self):
        self.d["approved_native_execution"]=False
        with self.assertRaises(ValueError):validate(self.d,self.r)
    def test_source_drift(self):
        self.d["source_sha"]="a"*40
        with self.assertRaises(ValueError):validate(self.d,self.r)
if __name__=="__main__":
    if sys.argv[1:]==["--self-test"]:unittest.main(argv=[sys.argv[0]],verbosity=2)
    else:
        p=argparse.ArgumentParser();p.add_argument("--out",required=True);a=p.parse_args()
        raw=gate(json.loads(pathlib.Path(os.environ["GITHUB_EVENT_PATH"]).read_text()),int(os.environ["GITHUB_RUN_ATTEMPT"]),os.environ["GITHUB_SHA"],os.environ["GITHUB_REF"])
        pathlib.Path(a.out).write_bytes(raw)
        print("STUDIO_EXPLICIT_NATIVE_REQUEST_PASS")
