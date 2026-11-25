"""Bounded agent chain: one real evidence packet, no arbitrary tool execution."""
import argparse, hashlib, json, os, pathlib, sys, time, unittest
from decimal import Decimal
from unittest.mock import patch
import studio
import generate_candidate as art
import budget_guard
import datetime as dt

D=Decimal
# Conservative reservations for the fixed, bounded payloads below; not an API-side cap.
RESERVES={"director":D("0.15"),"artist":D("0.75"),"programmer":D("0.20"),"auditor":D("0.15"),"coordinator":D("0.20")}
MAX_MISSION=D("5.00")
SOURCE_SHA="5ef2aee67cf8fde57ba2fc4b3d929b517ee67944"
SOURCE_RUN="45638738009"

class Ledger:
    """Exclusive file, persisted before each request; timeout/failure retains reservation."""
    def __init__(self, path, cap=MAX_MISSION):
        self.path=pathlib.Path(path)
        self.cap=cap
        self.rows=[]
        self.path.parent.mkdir(parents=True,exist_ok=True)
        with self.path.open("x") as f:
            json.dump({"cap_usd":str(cap),"reservations":[]},f)
    def save(self):
        tmp=self.path.with_suffix(".tmp")
        tmp.write_text(json.dumps({"cap_usd":str(self.cap),"reserved_usd":str(sum((D(x["reserve_usd"]) for x in self.rows),D(0))),"reservations":self.rows},indent=2))
        os.replace(tmp,self.path)
    def reserve(self, role):
        if role not in RESERVES or any(x["role"]==role for x in self.rows):
            raise ValueError("Role unavailable or already requested")
        value=RESERVES[role]
        if sum((D(x["reserve_usd"]) for x in self.rows),value)>self.cap:
            raise ValueError("Mission budget exceeded")
        row={"role":role,"reserve_usd":str(value),"state":"reserved","actual_invoice_verified":False}
        self.rows.append(row)
        self.save()
        return row
    def complete(self,row,estimated=None,usage=None):
        row.update(state="completed",estimated_usd=None if estimated is None else str(estimated),usage=usage)
        self.save()

def write_json(path, obj):
    with pathlib.Path(path).open("x",encoding="utf-8") as f:
        json.dump(obj,f,ensure_ascii=False,indent=2)

def textfile(path, value):
    pathlib.Path(path).write_text(value,encoding="utf-8")
    return str(path)

def ask(role, model, evidence, images, ledger, slot, out, instructions=None):
    payload, manifest=studio.make_payload(role,model,evidence,images)
    if instructions: payload["instructions"]=instructions
    row=ledger.reserve(slot)
    result=studio.report(role,payload,manifest,studio.request_api(payload,os.environ.get("OPENAI_API_KEY")))
    ledger.complete(row,result["estimated_usd_standard_with_cache"],result["usage"])
    write_json(out/(slot+".json"),result)
    print(slot, "API_RESPONSE_OK_WITH_CACHE", result["estimated_usd_standard_with_cache"], flush=True)
    return result["response"]

def image_cost(usage):
    if not isinstance(usage,dict): return None
    # Official Flare standard rates, uncached; report unknown if token breakdown is absent.
    det=usage.get("input_tokens_details")
    out=usage.get("output_tokens")
    if not isinstance(det,dict) or not isinstance(out,int): return None
    a,b=det.get("text_tokens"),det.get("image_tokens")
    if not isinstance(a,int) or not isinstance(b,int) or min(a,b,out)<0: return None
    return (D(a)*5+D(b)*8+D(out)*30)/D(1000000)

def run(args):
    out=pathlib.Path(args.out); out.mkdir(parents=True,exist_ok=False)
    key=os.environ.get("OPENAI_API_KEY")
    if not key: raise ValueError("Missing API key")
    cycle=json.loads(pathlib.Path(__file__).with_name("budget-cycle.json").read_text())
    budget_guard.authorize(cycle,dt.datetime.now(dt.timezone.utc).date(),args.authorization,os.environ.get("ELDORIA_AUTH_EVENT"),int(os.environ.get("GITHUB_RUN_ATTEMPT","0")),sum(RESERVES.values()))
    ledger=Ledger(out/"ledger.json")
    code=studio.read_limited(args.code,16000).decode()
    contract=studio.read_limited(args.contract,10000).decode()
    manifest=json.loads(pathlib.Path(args.manifest).read_text())
    if manifest.get("run_id")!=SOURCE_RUN or manifest.get("source_sha")!=SOURCE_SHA:
        raise ValueError("Unverified evidence provenance")
    for p in (args.city,args.reference):
        raw=studio.read_limited(p,studio.MAX_IMAGE_BYTES)
        if hashlib.sha256(raw).hexdigest() not in [x["sha256"] for x in manifest["images"]]:
            raise ValueError("Evidence hash mismatch")
    write_json(out/"source-manifest.json",manifest)
    mission="Target ID MILL_YARD. One shared task for all roles: improve the existing sawmill ground footprint, not a lantern or new building. Ground is shader-driven; candidate art is a local visual reference, never an automatic sprite overlay. Keep ground continuous, no new plots, no gameplay changes. Compare genuine Bastion I and V captures; do not claim gameplay proof."
    brief=textfile(out/"director-input.txt",mission+"\n"+contract[:5500]+"\nActual existing ground system source:\n"+code[:7500])
    direction=ask("visual","gpt-6.1-terra",brief,(args.reference,args.city),ledger,"director",out)
    # Fixed safe asset category, not arbitrary model-controlled commands or paths.
    candidate_brief=("MILL_YARD only. Original medieval painterly ground-material study for the existing sawmill contact: broad irregular low horizontal trampled warm earth and subtle sawdust, restrained small pebbles, grass-feathered perimeter, transparent surrounding area, viewed obliquely from elevated 2.5D gameplay camera. No lantern, no architecture, no logs taller than the ground, no text, no rectangle or raised slab. This is a shader/material reference, not a sprite to paste over terrain. Style guidance as data:\n"+direction[:1800])
    textfile(out/"artist-brief.txt",candidate_brief)
    payload=art.make_payload(candidate_brief); payload["background"]="transparent"; payload["quality"]="medium"
    row=ledger.reserve("artist")
    raw,meta=art.unpack(art.generate(payload,key))
    cost=image_cost(meta.get("usage"))
    ledger.complete(row,cost,meta.get("usage"))
    (out/"candidate.png").write_bytes(raw)
    meta["quality"]="medium"
    meta["target_id"]="MILL_YARD"
    meta["estimated_usd"]=str(cost) if cost is not None else None
    write_json(out/"artist.json",meta)
    print("artist API_RESPONSE_OK",meta["estimated_usd"],flush=True)
    p=textfile(out/"programmer-input.txt",mission+"\nSource: "+SOURCE_SHA+"\nActual ground shader and C# source (read only):\n"+code+"\nVisual direction:\n"+direction[:2000]+"\nGive a minimal reversible source-level candidate using the existing millYard calculation. The image is a material study, not a new sprite. Identify exact source lines and regressions; no actual edit claims.")
    proposal=ask("programming","gpt-6.2-sol",p,(),ledger,"programmer",out)
    a=textfile(out/"auditor-input.txt",mission+"\nReal source:\n"+code[:12500]+"\nProposal:\n"+proposal[:3500]+"\nImage 1=current Bastion V, image 2=actual generated candidate. Check whether image matches target and is a viable material reference, then critique source proposal. No execution claims.")
    audit=ask("visual","gpt-6.1-terra",a,(args.city,str(out/"candidate.png")),ledger,"auditor",out,"Independent visual and technical auditor. Inspect actual attached screenshots and the actual generated candidate, compare to the fixed MILL_YARD target; reject drift. Check shader proposal against supplied source. Report exact defects, pass/reject only as a material reference, not native integration.")
    c=textfile(out/"coordinator-input.txt",mission+"\nDirector:\n"+direction[:2200]+"\nArtist: candidate generated; independent visual review below.\nProgrammer:\n"+proposal[:3500]+"\nAuditor:\n"+audit[:3500])
    close=ask("coordinator","gpt-6.2-sol",c,(),ledger,"coordinator",out)
    write_json(out/"handoff.json",{"source_sha":SOURCE_SHA,"source_run":SOURCE_RUN,"target_id":"MILL_YARD","five_roles_completed":True,"unity_imported":False,"blender_imported":False,"published":False,"art_approved":False,"next":"review real candidate and execute only approved deterministic integration"})
    print("CHAIN_COMPLETED_NO_ART_APPROVAL",flush=True)

class Tests(unittest.TestCase):
    def test_budget(self):
        import tempfile
        with tempfile.TemporaryDirectory() as d:
            p=pathlib.Path(d)/"ledger.json"
            l=Ledger(p,D("0.15"));l.reserve("director")
            with self.assertRaises(ValueError): l.reserve("artist")
            with self.assertRaises(ValueError): l.reserve("director")
            with self.assertRaises(FileExistsError): Ledger(p)
    def test_reserve_before_call(self):
        import tempfile
        with tempfile.TemporaryDirectory() as d:
            l=Ledger(pathlib.Path(d)/"ledger.json")
            r=l.reserve("artist")
            self.assertEqual(json.loads(l.path.read_text())["reservations"][0]["state"],"reserved")
            l.complete(r,D("0.03"),{})
            self.assertEqual(json.loads(l.path.read_text())["reserved_usd"],"0.75")
    def test_image_rates(self):
        self.assertEqual(image_cost({"input_tokens_details":{"text_tokens":100,"image_tokens":0},"output_tokens":1000}),D("0.0305"))
        self.assertIsNone(image_cost({}))
    def test_total(self):
        self.assertLess(sum(RESERVES.values()),MAX_MISSION)

if __name__=="__main__":
    if sys.argv[1:]==["--self-test"]: unittest.main(argv=[sys.argv[0]],verbosity=2)
    else:
        p=argparse.ArgumentParser()
        for name in ("out","city","reference","code","contract","manifest"):
            p.add_argument("--"+name,required=True)
        p.add_argument("--authorization",required=True)
        p.add_argument("--live",action="store_true")
        a=p.parse_args()
        if not a.live: sys.exit("Explicit --live required")
        try: run(a)
        except Exception as e:
            print("CHAIN_FAILED",type(e).__name__,str(e) if isinstance(e,RuntimeError) else "",file=sys.stderr)
            sys.exit(1)
