"""Apply the exact reviewed MILL_YARD proposal to a copy; no generated code execution."""
import argparse,difflib,hashlib,json,math,pathlib,sys,unittest
OLD=""" half woodDust=(1-smoothstep(.04,.50,timberLine+.18*erosion))*millYard;
 half3 workEarth=lerp(crushedEarth,crushedEarth*half3(1.37,1.22,.91),woodDust);
 ground=lerp(ground,lerp(workEarth,ground,.23+.15*blades),millYard*.84);"""
NEW=""" half packedWear=millYard*lerp(.68,1.0,smoothstep(.28,.72,millYard+.22*erosion));
 half woodDust=(1-smoothstep(.05,.40,timberLine+.18*erosion))*millYard;
 half3 workEarth=lerp(crushedEarth,crushedEarth*half3(1.30,1.19,.94),woodDust);
 ground=lerp(ground,lerp(workEarth,ground,.23+.15*blades),packedWear*.84);"""
def replace(source):
    if source.count(OLD)!=1 or NEW in source:
        raise ValueError("Exact frozen shader block not found or already modified; no write")
    result=source.replace(OLD,NEW)
    if result.count(NEW)!=1 or result.replace(NEW,OLD)!=source:
        raise RuntimeError("Reversibility check failed")
    return result
def smoothstep(a,b,x):
    t=max(0,min(1,(x-a)/(b-a)))
    return t*t*(3-2*t)
def verify():
    samples=0; prior_sum=after_sum=0.; max_reduction=0.
    for i in range(101):
        for j in range(-100,101):
            m=i/100; erosion=j/100
            a=m*.84
            b=m*(.68+.32*smoothstep(.28,.72,m+.22*erosion))*.84
            if not (0<=b<=a+1e-12): raise RuntimeError("Blend expanded")
            if m==0 and b!=0: raise RuntimeError("Changed outside mask")
            prior_sum+=a;after_sum+=b;max_reduction=max(max_reduction,a-b)
            samples+=1
    for i in range(101):
        for j in range(-100,101):
            x=i/100+.18*j/100
            a=1-smoothstep(.04,.50,x)
            b=1-smoothstep(.05,.40,x)
            if not (0<=b<=1): raise RuntimeError("Dust blend out of range")
            if x>=.10 and b>a+1e-12: raise RuntimeError("Dust support unexpectedly expanded")
            if x>=.40 and b!=0: raise RuntimeError("New dust outside threshold")
            samples+=1
    return {"samples":samples,"blend_sum_ratio_on_uniform_test_grid":round(after_sum/prior_sum,5),
            "max_tested_blend_reduction":round(max_reduction,5),"scope":"three existing lines replaced with four; no mask, gameplay, resources or camera edits",
            "math_only":True,"native_shader_compiled":False,"art_approved":False}
class Tests(unittest.TestCase):
    def test_patch_only(self):
        a="prefix\n"+OLD+"\nsuffix"
        b=replace(a)
        self.assertEqual(b,"prefix\n"+NEW+"\nsuffix")
        self.assertEqual(b.replace(NEW,OLD),a)
    def test_source_drift(self):
        with self.assertRaises(ValueError): replace("wrong")
    def test_already_applied(self):
        with self.assertRaises(ValueError): replace(NEW)
    def test_math(self):
        self.assertLess(verify()["blend_sum_ratio_on_uniform_test_grid"],1)
if __name__=="__main__":
    if sys.argv[1:]==["--self-test"]: unittest.main(argv=[sys.argv[0]],verbosity=2)
    else:
        p=argparse.ArgumentParser()
        p.add_argument("--source",required=True);p.add_argument("--out",required=True)
        a=p.parse_args()
        src=pathlib.Path(a.source).read_text(encoding="utf-8")
        result=replace(src)
        proof=verify()
        dest=pathlib.Path(a.out);dest.mkdir(parents=True,exist_ok=False)
        (dest/"DistrictGround.candidate.shader").write_text(result,encoding="utf-8")
        (dest/"mill-yard.patch").write_text("".join(difflib.unified_diff(src.splitlines(True),result.splitlines(True),fromfile="DistrictGround.shader (frozen original)",tofile="DistrictGround.shader (candidate)")))
        proof.update(source_sha256=hashlib.sha256(src.encode()).hexdigest(),candidate_sha256=hashlib.sha256(result.encode()).hexdigest(),agent_run=45671428903,source_commit="5ef2aee67cf8fde57ba2fc4b3d929b517ee67944")
        (dest/"shader-proof.json").write_text(json.dumps(proof,indent=2))
        print("BOUNDED_SHADER_SOURCE_PASS",json.dumps(proof))
