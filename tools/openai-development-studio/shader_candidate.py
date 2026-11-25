"""Deterministic, bounded source actuator for a Valoria ground candidate; never writes production."""
import argparse,difflib,hashlib,json,math,pathlib,sys,unittest
OLD="""half millYard=1-smoothstep(.48,1.18,length(float2((millGround.x+millGround.y*.25)/2.75,
                  (millGround.y+.16)/1.53))+vergeBreak*.16);"""
OLD_BLEND="millYard*.84);"
def replace(source,rx,ry,strength):
    if not (2.0<=rx<=2.74 and 1.1<=ry<=1.52 and .65<=strength<=.84):
        raise ValueError("Outside approved localized numerical bounds")
    if source.count(OLD)!=1 or source.count(OLD_BLEND)!=1:
        raise ValueError("Exact frozen shader shape not found; no write")
    new=OLD.replace("/2.75",f"/{rx:.2f}").replace("/1.53",f"/{ry:.2f}")
    result=source.replace(OLD,new).replace(OLD_BLEND,f"millYard*{strength:.2f});")
    if result.replace(new,OLD).replace(f"millYard*{strength:.2f});",OLD_BLEND)!=source:
        raise RuntimeError("Reversibility check failed")
    return result
def smoothstep(a,b,x):
    t=max(0,min(1,(x-a)/(b-a)))
    return t*t*(3-2*t)
def mask(x,y,verge,rx,ry,strength):
    q=math.hypot((x+y*.25)/rx,(y+.16)/ry)+verge*.16
    return (1-smoothstep(.48,1.18,q))*strength
def verify(rx,ry,strength):
    before=after=0.0; n=0
    for i in range(-60,61):
        for j in range(-40,41):
            for v in (-.5,0,.5):
                x,y=i/10,j/10
                a=mask(x,y,v,2.75,1.53,.84);b=mask(x,y,v,rx,ry,strength)
                if b>a+1e-10 or not (0<=b<=.84): raise RuntimeError("Mask expands or exceeds safe opacity")
                before+=a;after+=b;n+=1
    center=mask(0,-.16,0,rx,ry,strength)
    if center<.65 or not (.4<=after/before<.99): raise RuntimeError("Excessive shrink or no useful difference")
    return {"samples":n,"weighted_coverage_ratio":round(after/before,4),"center_strength":round(center,4),
            "mask_only":True,"native_shader_compiled":False,"art_approved":False}
class Tests(unittest.TestCase):
    def test_patch_only(self):
        a="prefix\n"+OLD+"\n"+OLD_BLEND+"\nsuffix"
        b=replace(a,2.35,1.33,.80)
        self.assertIn("/2.35",b);self.assertIn("millYard*0.80);",b)
        self.assertTrue(b.startswith("prefix"));self.assertTrue(b.endswith("suffix"))
    def test_source_drift(self):
        with self.assertRaises(ValueError): replace("wrong",2.35,1.33,.80)
    def test_bad_values(self):
        with self.assertRaises(ValueError): replace(OLD+OLD_BLEND,4,1.33,.80)
    def test_math(self):
        self.assertGreater(verify(2.35,1.33,.8)["weighted_coverage_ratio"],.4)
if __name__=="__main__":
    if sys.argv[1:]==["--self-test"]: unittest.main(argv=[sys.argv[0]],verbosity=2)
    else:
        p=argparse.ArgumentParser()
        p.add_argument("--source",required=True);p.add_argument("--out",required=True)
        p.add_argument("--x-radius",type=float,required=True);p.add_argument("--y-radius",type=float,required=True);p.add_argument("--strength",type=float,required=True)
        a=p.parse_args()
        src=pathlib.Path(a.source).read_text(encoding="utf-8")
        result=replace(src,a.x_radius,a.y_radius,a.strength)
        proof=verify(a.x_radius,a.y_radius,a.strength)
        dest=pathlib.Path(a.out);dest.mkdir(parents=True,exist_ok=False)
        (dest/"DistrictGround.candidate.shader").write_text(result,encoding="utf-8")
        (dest/"mill-yard.patch").write_text("".join(difflib.unified_diff(src.splitlines(True),result.splitlines(True),fromfile="DistrictGround.shader (frozen original)",tofile="DistrictGround.shader (candidate)")))
        proof.update(source_sha256=hashlib.sha256(src.encode()).hexdigest(),candidate_sha256=hashlib.sha256(result.encode()).hexdigest(),parameters=[a.x_radius,a.y_radius,a.strength])
        (dest/"shader-proof.json").write_text(json.dumps(proof,indent=2))
        print("BOUNDED_SHADER_SOURCE_PASS",json.dumps(proof))
