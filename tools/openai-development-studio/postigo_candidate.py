"""Materialize a bounded real Valoria road correction and an Editor-only A/B version.

Do not run on an owner checkout. The exact original blob is checked by the workflow.
No new meshes in production: two existing roads retain their spine, material and count.
"""
import argparse,difflib,hashlib,json,unittest
from pathlib import Path
SOURCE="5ef2aee67cf8fde57ba2fc4b3d929b517ee67944"
SIGNATURE="public GameObject AddRoad(string name,Vector2[] control,float width,Material material,bool cobbled)"
OFFSET="                    float offset=(u-.5f)*width*wav;"
FADE="                    alpha*=Mathf.Lerp(.04f,1f,Sm(0,.63f,along));"
OFFSET_NEW="""                    float offset=(u-.5f)*width*wav;
                    // Local frontage contact, not a new road or independent decal.
                    if(postigoTreatment==2 && name=="Calle del Postigo")
                    {
                        var plan=ValoriaUrbanPlan.City;
                        Vector2 rel=spine[i]-plan.PostigoLane(0);
                        float station=rel.x/ValoriaUrbanPlan.PostigoAxis.x;
                        float cross=rel.y-ValoriaUrbanPlan.PostigoAxis.y*station;
                        float frontage=(1f-Sm(1.65f,2.65f,Mathf.Abs(station)))*
                            (1f-Sm(.25f,.65f,Mathf.Abs(cross)));
                        float side=(u-.5f)*2f;
                        float extension=side*normal.y>=0f?.55f:.20f;
                        offset+=side*extension*frontage;
                    }"""
FADE_NEW="""                    // This branch starts on a connected street, not open meadow.
                    // Preserve the original feather everywhere else.
                    if(postigoTreatment==0 || name!="Paso bajo el postigo")
                        alpha*=Mathf.Lerp(.04f,1f,Sm(0,.63f,along));"""
def once(text,old,new):
    if text.count(old)!=1:raise ValueError("Frozen anchor drift: "+old[:90])
    return text.replace(old,new,1)
def patch(text,default):
    if default not in (0,2):raise ValueError("Unknown variant")
    result=once(text,SIGNATURE,SIGNATURE[:-1]+",int postigoTreatment="+str(default)+")")
    result=once(result,OFFSET,OFFSET_NEW)
    result=once(result,FADE,FADE_NEW)
    return result
def verify(src,prod,editor):
    assert prod==editor.replace("int postigoTreatment=0","int postigoTreatment=2",1)
    assert "int postigoTreatment=2" in prod
    # Exact reversibility; no changes to spline, vertex color banks, road IDs or routes.
    restored=once(prod,FADE_NEW,FADE)
    restored=once(restored,OFFSET_NEW,OFFSET)
    restored=once(restored,SIGNATURE[:-1]+",int postigoTreatment=2)",SIGNATURE)
    assert restored==src
def main():
    a=argparse.ArgumentParser();a.add_argument("--source",required=True);a.add_argument("--out",required=True);x=a.parse_args()
    p=Path(x.source);src=p.read_text(encoding="utf-8");out=Path(x.out);out.mkdir(exist_ok=False)
    prod=patch(src,2);editor=patch(src,0);verify(src,prod,editor)
    for name,data in (("Modular25DLandscape.candidate.cs",prod),("Modular25DLandscape.editor.cs",editor)):
        (out/name).write_bytes(data.encode("utf-8"))
    diff="".join(difflib.unified_diff(src.splitlines(keepends=True),prod.splitlines(keepends=True),fromfile="a/Unity/Assets/Experimentos/ValoriaModular25D/Scripts/Modular25DLandscape.cs",tofile="b/Unity/Assets/Experimentos/ValoriaModular25D/Scripts/Modular25DLandscape.cs"))
    (out/"postigo.patch").write_text(diff,encoding="utf-8")
    proof={"source":SOURCE,"source_sha256":hashlib.sha256(src.encode()).hexdigest(),"candidate_sha256":hashlib.sha256(prod.encode()).hexdigest(),"editor_sha256":hashlib.sha256(editor.encode()).hexdigest(),"exact_reverse":True,"source_only_verified":True,"native_verified":False,"art_approved":False}
    (out/"source-proof.json").write_text(json.dumps(proof,indent=2))
    print("POSTIGO_BOUNDED_SOURCE_PASS",json.dumps(proof))
class Tests(unittest.TestCase):
    def test_reversible(self):
        src="\n".join([SIGNATURE,OFFSET,FADE]);verify(src,patch(src,2),patch(src,0))
    def test_drift_rejected(self):
        with self.assertRaises(ValueError):patch(SIGNATURE+"\n"+OFFSET,2)
    def test_duplicate_rejected(self):
        with self.assertRaises(ValueError):patch("\n".join([SIGNATURE,OFFSET,OFFSET,FADE]),2)
    def test_mode_rejected(self):
        with self.assertRaises(ValueError):patch("",1)
if __name__=="__main__":
    import sys
    if sys.argv[1:]==["--self-test"]:unittest.main(argv=[sys.argv[0]])
    else:main()
