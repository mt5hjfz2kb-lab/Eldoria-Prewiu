"""Owner quality contract: real matched A/B, reviewer grounded in original SHA,
no Qwen self-approval, no rejected-candidate chaining, no fake calibration.
"""
import hashlib,json,pathlib,sys,tempfile,unittest
sys.path.insert(0,str(pathlib.Path(__file__).resolve().parents[1]))
from artistic_quality_gate import (weighted,stage_visual_request,
    validate_independent_review,append_accepted,status)

def sha(x):return hashlib.sha256(x).hexdigest()
def rubric(v):return {k:v for k in ("geometry","architecture","materials","style","camera")}

class ArtistPermanentContractTests(unittest.TestCase):
    def make_request(self,where):
        from PIL import Image
        p=pathlib.Path(where)
        source=p/"tower.glb";source.write_bytes(b"glTF--ACTUAL SOURCE TEST BYTES")
        plan=p/"plan.json";plan.write_text('{"actions":[]}\n')
        before=p/"before.png";after=p/"after.png"
        Image.new("RGB",(25,24),(12,24,30)).save(before)
        Image.new("RGB",(25,24),(22,34,40)).save(after)
        ref=p/"reference.png";Image.new("RGB",(25,24),(0,0,0)).save(ref)
        proof=p/"execution-proof.json"
        proof.write_text(json.dumps({"blender_execution":True,"matched_review_camera":True,
            "source_sha256":sha(source.read_bytes()),"artist_plan_sha256":sha(plan.read_bytes()),
            "before_blender_sha256":sha(before.read_bytes()),
            "after_blender_sha256":sha(after.read_bytes()),
            "blend_sha256":"a"*64,"glb_sha256":"b"*64}))
        request=stage_visual_request(output=p/"review-request.json",reference=ref,
            original_source_sha=sha(source.read_bytes()),input_source=source,
            raw_plan=plan,raw_response_file=plan,execution_proof=proof,before=before,after=after,
            model="qwen8b",memory_proof=None,run_id=123)
        return request,source,before,after
    def review(self,req):
        return {"review_kind":"INDEPENDENT_BLENDER_VISUAL_REVIEW",
            "independent":True,"reviewer_id":"external-visual-inspector",
            "original_asset_sha256":req["original_asset_sha256"],
            "input_asset_sha256":req["input_asset_sha256"],
            "output_asset_sha256":req["output_asset_sha256"],
            "qwen_plan_sha256":req["qwen_plan_sha256"],
            "execution_plan_sha256":req["execution_plan_sha256"],
            "reference_sha256":req["reference_sha256"],
            "before_png_sha256":req["before_png_sha256"],
            "after_png_sha256":req["after_png_sha256"],
            "before":rubric(7.0),"after":rubric(7.35),
            "diagnosis":"Actual authored roof joins, contacts, proportions and reference alignment examined against original.",
            "observed_improvement":"Independent matched-camera comparison supports an original readable roof edge with coherent silhouette.",
            "protected_parts_impact":"No new interference with preserved source timber roof, exterior stairs, door, textures or navigation.",
            "protected_regions_ok":True,"severe_regressions":[],"accepted":True}
    def test_sha_linked_visible_blender_source_requires_external_review(self):
        with tempfile.TemporaryDirectory() as d:
            req,source,_,after=self.make_request(d)
            self.assertEqual(req["status"],"ART_REVIEW_REQUIRED")
            self.assertFalse(req["art_approved"])
            review=self.review(req)
            case=validate_independent_review(req,review)
            self.assertEqual(case["status"],"BLENDER_VISUAL_ACCEPTED")
            self.assertFalse(case["unity_verified"])
            chain=append_accepted([],case,frozen_original=req["original_asset_sha256"])
            self.assertEqual(status(chain)["blender_visual_accepted_count"],1)
            self.assertFalse(status(chain)["calibration_validated"])
    def test_model_cannot_approve_own_output(self):
        with tempfile.TemporaryDirectory() as d:
            req,_,_,_=self.make_request(d)
            rv=self.review(req);rv["reviewer_id"]="qwen8b"
            with self.assertRaisesRegex(ValueError,"Qwen"):validate_independent_review(req,rv)
    def test_false_score_and_protected_damage_rejected(self):
        with tempfile.TemporaryDirectory() as d:
            req,_,_,_=self.make_request(d)
            rv=self.review(req);rv["after"]=rubric(7.1)
            with self.assertRaisesRegex(ValueError,">=0.25"):validate_independent_review(req,rv)
            rv=self.review(req);rv["after"]["materials"]=6.0
            with self.assertRaisesRegex(ValueError,"regressed"):validate_independent_review(req,rv)
            rv=self.review(req);rv["protected_regions_ok"]=False
            with self.assertRaisesRegex(ValueError,"Protected"):validate_independent_review(req,rv)
    def test_reject_fake_original_and_unrelated_next_input(self):
        with tempfile.TemporaryDirectory() as d:
            req,_,_,_=self.make_request(d)
            rv=self.review(req);rv["before_png_sha256"]="f"*64
            with self.assertRaisesRegex(ValueError,"before_png_sha256"):
                validate_independent_review(req,rv)
            ok=validate_independent_review(req,self.review(req))
            chain=append_accepted([],ok,frozen_original=req["original_asset_sha256"])
            with self.assertRaisesRegex(ValueError,"source"):
                append_accepted(chain,ok,frozen_original=req["original_asset_sha256"])
    def test_three_blender_improvements_not_false_calibration_or_art_approval(self):
        with tempfile.TemporaryDirectory() as d:
            req,_,_,_=self.make_request(d)
            rec=validate_independent_review(req,self.review(req))
            chain=append_accepted([],rec,frozen_original=req["original_asset_sha256"])
            for i in (2,3):
                nxt={**rec,"input_asset_sha256":chain[-1]["output_asset_sha256"],
                     "output_asset_sha256":str(i)*64,
                     "before_score":chain[-1]["after_score"],
                     "after_score":round(chain[-1]["after_score"]+.3,5)}
                chain=append_accepted(chain,nxt,frozen_original=req["original_asset_sha256"])
            result=status(chain)
            self.assertEqual(result["blender_visual_accepted_count"],3)
            self.assertFalse(result["calibration_validated"])
            self.assertFalse(result["art_approved"])
            self.assertFalse(result["unity_verified"])
if __name__=="__main__":unittest.main()
